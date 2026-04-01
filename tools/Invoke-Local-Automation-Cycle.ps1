param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $PolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [string] $BaseRef,
    [string] $RequestedVersion,
    [switch] $ForceReleaseCandidate,
    [switch] $ForceDigest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $RepoRoot = Split-Path -Parent $PSScriptRoot
    } else {
        $RepoRoot = (Get-Location).Path
    }
}

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath
$automationControlSignalsHelperPath = Join-Path $PSScriptRoot 'Automation-ControlSignals.ps1'
. $automationControlSignalsHelperPath

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    if (Test-Path -LiteralPath $resolvedBase -PathType Leaf) {
        $resolvedBase = Split-Path -Parent $resolvedBase
    }

    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('\', '/')
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-OptionalDateTimeUtc {
    param([object] $Value)

    if ($null -eq $Value) {
        return $null
    }

    $stringValue = [string] $Value
    if ([string]::IsNullOrWhiteSpace($stringValue)) {
        return $null
    }

    return [datetime]::Parse($stringValue, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
}

function Get-ScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
}

function Invoke-ChildPowershellScript {
    param(
        [string[]] $ArgumentList,
        [string] $FailureContext
    )

    $scriptFileIndex = [System.Array]::IndexOf($ArgumentList, '-File')
    if ($scriptFileIndex -ge 0 -and ($scriptFileIndex + 1) -lt $ArgumentList.Length) {
        $resolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $resolverPath -PathType Leaf) {
            $scriptPath = [string] $ArgumentList[$scriptFileIndex + 1]
            $scriptArgs = if (($scriptFileIndex + 2) -lt $ArgumentList.Length) {
                @($ArgumentList[($scriptFileIndex + 2)..($ArgumentList.Length - 1)])
            } else {
                @()
            }

            $escapedResolverPath = $resolverPath.Replace("'", "''")
            $escapedScriptPath = $scriptPath.Replace("'", "''")
            $renderedScriptArgs = @(
                foreach ($scriptArg in $scriptArgs) {
                    $scriptArgText = [string] $scriptArg
                    if ($scriptArgText.StartsWith('-')) {
                        $scriptArgText
                    } else {
                        "'" + $scriptArgText.Replace("'", "''") + "'"
                    }
                }
            )

            $command = "& { . '$escapedResolverPath'; & '$escapedScriptPath'"
            if ($renderedScriptArgs.Count -gt 0) {
                $command += ' ' + ($renderedScriptArgs -join ' ')
            }
            $command += ' }'

            $output = & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -Command $command
            if ($LASTEXITCODE -ne 0) {
                throw '{0} failed with exit code {1}.' -f $FailureContext, $LASTEXITCODE
            }

            return @($output)
        }
    }

    $output = & powershell -NoProfile -NonInteractive -WindowStyle Hidden @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw '{0} failed with exit code {1}.' -f $FailureContext, $LASTEXITCODE
    }

    return @($output)
}

function Get-LatestBundlePath {
    param([string] $RootPath)

    if (-not (Test-Path -LiteralPath $RootPath -PathType Container)) {
        return $null
    }

    $latest = Get-ChildItem -LiteralPath $RootPath -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'build-evidence-manifest.json') -PathType Leaf } |
        Sort-Object -Property Name -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        return $null
    }

    return $latest.FullName
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $PolicyPath
$policy = Get-Content -Raw -LiteralPath $resolvedPolicyPath | ConvertFrom-Json

$releaseCandidateOutputRoot = [string] $policy.releaseCandidateOutputRoot
$digestOutputRoot = [string] $policy.digestOutputRoot
$blockedEscalationOutputRoot = [string] $policy.blockedEscalationOutputRoot
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statePath)
$dopingHeaderStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.dopingHeaderStatePath)
$cycleReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.cycleReceiptStatePath)
$readinessNoticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.readinessNoticeStatePath)
$pauseNoticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.pauseNoticeStatePath)
$retentionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.retentionStatePath)
$blockedEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.blockedEscalationStatePath)
$notificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.notificationStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.schedulerReconciliationStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.cmeConsolidationStatePath)
$cmeFormationAndOfficeLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.cmeFormationAndOfficeLedgerStatePath)
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.promotionGateStatePath)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.ciConcordanceStatePath)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.releaseRatificationStatePath)
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.seededPromotionReviewStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.firstPublishIntentStatePath)
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.releaseHandshakeStatePath)
$publishRequestEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.publishRequestEnvelopeStatePath)
$postPublishEvidenceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.postPublishEvidenceStatePath)
$seedBraidEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.seedBraidEscalationStatePath)
$publishedRuntimeReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.publishedRuntimeReceiptStatePath)
$artifactAttestationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.artifactAttestationStatePath)
$postPublishDriftWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.postPublishDriftWatchStatePath)
$operationalPublicationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.operationalPublicationLedgerStatePath)
$externalConsumerConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.externalConsumerConcordanceStatePath)
$postPublishGovernanceLoopStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.postPublishGovernanceLoopStatePath)
$publicationCadenceLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.publicationCadenceLedgerStatePath)
$downstreamRuntimeObservationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.downstreamRuntimeObservationStatePath)
$multiIntervalGovernanceBraidStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.multiIntervalGovernanceBraidStatePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.unattendedIntervalConcordanceStatePath)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.staleSurfaceContradictionWatchStatePath)
$unattendedProofCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.unattendedProofCollapseStatePath)
$dormantWindowLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.dormantWindowLedgerStatePath)
$silentCadenceIntegrityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.silentCadenceIntegrityStatePath)
$longFormPhaseWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.longFormPhaseWitnessStatePath)
$longFormWindowBoundaryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.longFormWindowBoundaryStatePath)
$autonomousLongFormCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.autonomousLongFormCollapseStatePath)
$schedulerProofHarvestStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.schedulerProofHarvestStatePath)
$intervalOriginClarificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.intervalOriginClarificationStatePath)
$queuedTaskMapPromotionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.queuedTaskMapPromotionStatePath)
$masterThreadOrchestrationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.masterThreadOrchestrationStatePath)
$runtimeDeployabilityEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.runtimeDeployabilityEnvelopeStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.runtimeWorkSurfaceAdmissibilityStatePath)
$reachAccessTopologyLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.reachAccessTopologyLedgerStatePath)
$bondedOperatorLocalityReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.bondedOperatorLocalityReadinessStatePath)
$protectedStateLegibilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.protectedStateLegibilitySurfaceStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.nexusSingularPortalFacadeStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.duplexPredicateEnvelopeStatePath)
$operatorActualWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.operatorActualWorkSessionRehearsalStatePath)
$identityInvariantThreadRootStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.identityInvariantThreadRootStatePath)
$governedThreadBirthReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.governedThreadBirthReceiptStatePath)
$interWorkerBraidHandoffPacketStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.interWorkerBraidHandoffPacketStatePath)
$agentiCoreActualUtilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.agentiCoreActualUtilitySurfaceStatePath)
$reachDuplexRealizationSeamStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.reachDuplexRealizationSeamStatePath)
$bondedParticipationLocalityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.bondedParticipationLocalityLedgerStatePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.amenableDayDreamTierAdmissibilityStatePath)
$selfRootedCrypticDepthGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.selfRootedCrypticDepthGateStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.runtimeWorkbenchSessionLedgerStatePath)
$companionToolTelemetryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.companionToolTelemetryStatePath)
$v111EnrichmentPathwayStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.v111EnrichmentPathwayStatePath)
$runIsolatedBuildPathwayStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.runIsolatedBuildPathwayStatePath)
$dayDreamCollapseReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.dayDreamCollapseReceiptStatePath)
$crypticDepthReturnReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.crypticDepthReturnReceiptStatePath)
$bondedCoWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.bondedCoWorkSessionRehearsalStatePath)
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.reachReturnDissolutionReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.localityDistinctionWitnessLedgerStatePath)
$localHostSanctuaryResidencyEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.localHostSanctuaryResidencyEnvelopeStatePath)
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.runtimeHabitationReadinessLedgerStatePath)
$boundedInhabitationLaunchRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.boundedInhabitationLaunchRehearsalStatePath)
$postHabitationHorizonLatticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.postHabitationHorizonLatticeStatePath)
$boundedHorizonResearchBriefStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.boundedHorizonResearchBriefStatePath)
$nextEraBatchSelectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.nextEraBatchSelectorStatePath)
$inquirySessionDisciplineSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.inquirySessionDisciplineSurfaceStatePath)
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.boundaryConditionLedgerStatePath)
$coherenceGainWitnessReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.coherenceGainWitnessReceiptStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.operatorInquirySelectionEnvelopeStatePath)
$bondedCrucibleSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.bondedCrucibleSessionRehearsalStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.sharedBoundaryMemoryLedgerStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.continuityUnderPressureLedgerStatePath)
$expressiveDeformationReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.expressiveDeformationReceiptStatePath)
$mutualIntelligibilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.mutualIntelligibilityWitnessStatePath)
$inquiryPatternContinuityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.inquiryPatternContinuityLedgerStatePath)
$questioningBoundaryPairLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.questioningBoundaryPairLedgerStatePath)
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.carryForwardInquirySelectionSurfaceStatePath)
$engramDistanceClassificationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.engramDistanceClassificationLedgerStatePath)
$engramPromotionRequirementsMatrixStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.engramPromotionRequirementsMatrixStatePath)
$distanceWeightedQuestioningAdmissionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.distanceWeightedQuestioningAdmissionSurfaceStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.questioningGelPromotionGateStatePath)
$protectedQuestioningPatternSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.protectedQuestioningPatternSurfaceStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.variationTestedReentryLedgerStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.questioningAdmissionRefusalReceiptStatePath)
$promotionSeductionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.promotionSeductionWatchStatePath)
$engramIntentFieldLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.engramIntentFieldLedgerStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.intentConstraintAlignmentReceiptStatePath)
$warmReactivationDispositionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.warmReactivationDispositionReceiptStatePath)
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.formationPhaseVectorStatePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.brittlenessWitnessStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.durabilityWitnessStatePath)
$warmClockDispositionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.warmClockDispositionStatePath)
$ripeningStalenessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.ripeningStalenessLedgerStatePath)
$coolingPressureWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.coolingPressureWitnessStatePath)
$hotReactivationTriggerReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.hotReactivationTriggerReceiptStatePath)
$coldAdmissionEligibilityGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.coldAdmissionEligibilityGateStatePath)
$archiveDispositionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.archiveDispositionLedgerStatePath)
$interlockDensityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.interlockDensityLedgerStatePath)
$brittleDurableDifferentiationSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.brittleDurableDifferentiationSurfaceStatePath)
$coreInvariantLatticeWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.coreInvariantLatticeWitnessStatePath)
$releaseCandidateRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $releaseCandidateOutputRoot
$digestRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $digestOutputRoot
$releaseCadenceHours = [int] $policy.localReleaseCandidateCadenceHours
$digestCadenceHours = [int] $policy.mandatoryHitlDigestCadenceHours
$digestWindowHours = [int] $policy.digestWindowHours
$blockedStatus = [string] $policy.blockedStatus

$state = Read-JsonFileOrNull -Path $statePath
$previousStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastKnownStatus')
$nowUtc = (Get-Date).ToUniversalTime()
$cycleRunStartedAtUtc = $nowUtc
$automationCycleRunId = 'local-automation-cycle-{0}' -f $cycleRunStartedAtUtc.ToString('yyyyMMddTHHmmssZ')
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReleaseCandidateRunUtc')
$lastDigestUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDigestUtc')

$dopingHeaderPayload = New-AutomationDopingHeaderPayload `
    -RunId $automationCycleRunId `
    -Lane 'build-governance-automation' `
    -Objective 'Run the governed local automation cycle and reconcile release-candidate, digest, and downstream evidence surfaces.' `
    -Phase 'cycle-entry' `
    -Milestone 'local-automation-cycle' `
    -Artifacts @(
        'tools/Invoke-Local-Automation-Cycle.ps1'
        'tools/Automation-CascadePrompt.ps1'
        'tools/Automation-ControlSignals.ps1'
        $PolicyPath
    ) `
    -AuthorizedTools @(
        'powershell'
        'git'
        'dotnet'
        'repo-local scripts'
        'windows-scheduled-task'
    ) `
    -VerificationExpectations @(
        'release-candidate manifest'
        'digest bundle when due'
        'task status refresh'
        'master-thread orchestration refresh'
    ) `
    -ForwardConditioningNotes @(
        'Reconcile first against the admitted root state in OAN Tech Stack before advancing downstream automation surfaces.'
    )
Add-AutomationCascadeOperatorPromptProperty -InputObject $dopingHeaderPayload | Out-Null
Write-JsonFile -Path $dopingHeaderStatePath -Value $dopingHeaderPayload

$releaseCandidateDue = $ForceReleaseCandidate.IsPresent
if (-not $releaseCandidateDue) {
    if ($null -eq $lastReleaseCandidateRunUtc) {
        $releaseCandidateDue = $true
    } else {
        $releaseCandidateDue = $lastReleaseCandidateRunUtc.AddHours($releaseCadenceHours) -le $nowUtc
    }
}

$releaseCandidateBundlePath = $null
if ($releaseCandidateDue) {
    $invokeReleaseCandidateScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Release-Candidate.ps1'
    $releaseCandidateArgs = @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $invokeReleaseCandidateScriptPath,
        '-Configuration', $Configuration,
        '-RepoRoot', $resolvedRepoRoot,
        '-OutputRoot', $releaseCandidateOutputRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
        $releaseCandidateArgs += @('-BaseRef', $BaseRef)
    }

    if (-not [string]::IsNullOrWhiteSpace($RequestedVersion)) {
        $releaseCandidateArgs += @('-RequestedVersion', $RequestedVersion)
    }

    $releaseCandidateOutput = Invoke-ChildPowershellScript -ArgumentList $releaseCandidateArgs -FailureContext 'Release-candidate conveyor'

    $releaseCandidateBundlePath = Get-ScriptOutputTail -Output $releaseCandidateOutput
    if ([string]::IsNullOrWhiteSpace($releaseCandidateBundlePath)) {
        throw 'Local automation cycle did not receive a release-candidate bundle path.'
    }
} else {
    $releaseCandidateBundlePath = Get-LatestBundlePath -RootPath $releaseCandidateRunRoot
}

if ([string]::IsNullOrWhiteSpace($releaseCandidateBundlePath)) {
    throw 'Local automation cycle could not locate a release-candidate bundle.'
}

$manifestPath = Join-Path $releaseCandidateBundlePath 'build-evidence-manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Release-candidate manifest not found at '$manifestPath'."
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$latestStatus = [string] $manifest.status
$latestRunGeneratedAtUtc = [datetime]::Parse([string] $manifest.generatedAtUtc, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
$nowUtc = (Get-Date).ToUniversalTime()

$digestDue = $ForceDigest.IsPresent
if (-not $digestDue) {
    if ($latestStatus -in @('hitl-required', $blockedStatus)) {
        $digestDue = $true
    } elseif ($null -eq $lastDigestUtc) {
        $digestDue = $true
    } else {
        $digestDue = $lastDigestUtc.AddHours($digestCadenceHours) -le $nowUtc
    }
}

$digestReportedNextMandatoryReviewUtc = if ($latestStatus -eq $blockedStatus) {
    $nowUtc
} elseif ($digestDue) {
    $nowUtc.AddHours($digestCadenceHours)
} elseif ($null -eq $lastDigestUtc) {
    $nowUtc.AddHours($digestCadenceHours)
} else {
    $lastDigestUtc.AddHours($digestCadenceHours)
}

$digestBundlePath = $null
if ($digestDue) {
    $writeDigestScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Release-Candidate-Digest.ps1'
    $digestArgs = @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $writeDigestScriptPath,
        '-RepoRoot', $resolvedRepoRoot,
        '-RunRoot', $releaseCandidateOutputRoot,
        '-OutputRoot', $digestOutputRoot,
        '-WindowHours', $digestWindowHours,
        '-ReferenceTimeUtc', $nowUtc.ToString('o'),
        '-NextMandatoryReviewUtc', $digestReportedNextMandatoryReviewUtc.ToString('o')
    )

    $digestOutput = Invoke-ChildPowershellScript -ArgumentList $digestArgs -FailureContext 'Daily HITL digest writer'

    $digestBundlePath = Get-ScriptOutputTail -Output $digestOutput
    if ([string]::IsNullOrWhiteSpace($digestBundlePath)) {
        throw 'Local automation cycle did not receive a digest bundle path.'
    }
} else {
    if (Test-Path -LiteralPath $digestRunRoot -PathType Container) {
        $latestDigest = Get-ChildItem -LiteralPath $digestRunRoot -Directory |
            Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'release-candidate-digest.json') -PathType Leaf } |
            Sort-Object -Property Name -Descending |
            Select-Object -First 1

        if ($null -ne $latestDigest) {
            $digestBundlePath = $latestDigest.FullName
        }
    }
}

$newLastDigestUtc = if ($digestDue) { $nowUtc } else { $lastDigestUtc }
$nextMandatoryReviewUtc = if ($null -eq $newLastDigestUtc) {
    $nowUtc.AddHours($digestCadenceHours)
} else {
    $newLastDigestUtc.AddHours($digestCadenceHours)
}
$automationActionClass = Get-AutomationActionClassFromStatus -Status $latestStatus
$normalizedLatestStatus = ([string] $latestStatus).ToLowerInvariant()

$statePayload = [ordered]@{
    schemaVersion = 1
    runId = $automationCycleRunId
    generatedAtUtc = $nowUtc.ToString('o')
    policyPath = $resolvedPolicyPath
    cadenceHours = [ordered]@{
        releaseCandidate = $releaseCadenceHours
        mandatoryHitlDigest = $digestCadenceHours
        digestWindow = $digestWindowHours
    }
    lastReleaseCandidateRunUtc = $latestRunGeneratedAtUtc.ToString('o')
    lastReleaseCandidateBundle = $releaseCandidateBundlePath
    lastKnownStatus = $latestStatus
    lastDigestUtc = if ($null -ne $newLastDigestUtc) { $newLastDigestUtc.ToString('o') } else { $null }
    lastDigestBundle = $digestBundlePath
    nextReleaseCandidateRunUtc = $latestRunGeneratedAtUtc.AddHours($releaseCadenceHours).ToString('o')
    nextMandatoryHitlReviewUtc = $nextMandatoryReviewUtc.ToString('o')
    releaseCandidateTriggered = $releaseCandidateDue
    digestTriggered = $digestDue
    actionClass = $automationActionClass
    dopingHeaderStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $dopingHeaderStatePath
    cycleReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $cycleReceiptStatePath
    readinessNoticeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $readinessNoticeStatePath
    pauseNoticeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $pauseNoticeStatePath
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null
$summaryPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-cycle-last-run.json'
$statePayload.lastBlockedEscalationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBlockedEscalationBundle')
$statePayload.blockedEscalationTriggered = $false
$statePayload.retentionStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $retentionStatePath
$statePayload.lastNotificationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastNotificationBundle')
$statePayload.notificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $notificationStatePath
$statePayload.lastSeededGovernanceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSeededGovernanceBundle')
$statePayload.seededGovernanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seededGovernanceStatePath
$statePayload.schedulerReconciliationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerReconciliationStatePath
$statePayload.cmeConsolidationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $cmeConsolidationStatePath
$statePayload.lastCmeFormationAndOfficeLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCmeFormationAndOfficeLedgerBundle')
$statePayload.cmeFormationAndOfficeLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $cmeFormationAndOfficeLedgerStatePath
$statePayload.lastPromotionGateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPromotionGateBundle')
$statePayload.promotionGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $promotionGateStatePath
$statePayload.lastCiConcordanceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCiConcordanceBundle')
$statePayload.ciConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $ciConcordanceStatePath
$statePayload.lastReleaseRatificationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReleaseRatificationBundle')
$statePayload.releaseRatificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $releaseRatificationStatePath
$statePayload.lastSeededPromotionReviewBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSeededPromotionReviewBundle')
$statePayload.seededPromotionReviewStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seededPromotionReviewStatePath
$statePayload.lastFirstPublishIntentBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastFirstPublishIntentBundle')
$statePayload.firstPublishIntentStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $firstPublishIntentStatePath
$statePayload.lastReleaseHandshakeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReleaseHandshakeBundle')
$statePayload.releaseHandshakeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $releaseHandshakeStatePath
$statePayload.lastPublishRequestEnvelopeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPublishRequestEnvelopeBundle')
$statePayload.publishRequestEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publishRequestEnvelopeStatePath
$statePayload.lastPostPublishEvidenceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPostPublishEvidenceBundle')
$statePayload.postPublishEvidenceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishEvidenceStatePath
$statePayload.lastSeedBraidEscalationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSeedBraidEscalationBundle')
$statePayload.seedBraidEscalationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seedBraidEscalationStatePath
$statePayload.lastPublishedRuntimeReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPublishedRuntimeReceiptBundle')
$statePayload.publishedRuntimeReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publishedRuntimeReceiptStatePath
$statePayload.lastArtifactAttestationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastArtifactAttestationBundle')
$statePayload.artifactAttestationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $artifactAttestationStatePath
$statePayload.lastPostPublishDriftWatchBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPostPublishDriftWatchBundle')
$statePayload.postPublishDriftWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishDriftWatchStatePath
$statePayload.lastOperationalPublicationLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastOperationalPublicationLedgerBundle')
$statePayload.operationalPublicationLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operationalPublicationLedgerStatePath
$statePayload.lastExternalConsumerConcordanceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastExternalConsumerConcordanceBundle')
$statePayload.externalConsumerConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $externalConsumerConcordanceStatePath
$statePayload.lastPostPublishGovernanceLoopBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPostPublishGovernanceLoopBundle')
$statePayload.postPublishGovernanceLoopStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishGovernanceLoopStatePath
$statePayload.lastPublicationCadenceLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPublicationCadenceLedgerBundle')
$statePayload.publicationCadenceLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publicationCadenceLedgerStatePath
$statePayload.lastDownstreamRuntimeObservationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDownstreamRuntimeObservationBundle')
$statePayload.downstreamRuntimeObservationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $downstreamRuntimeObservationStatePath
$statePayload.lastMultiIntervalGovernanceBraidBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastMultiIntervalGovernanceBraidBundle')
$statePayload.multiIntervalGovernanceBraidStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $multiIntervalGovernanceBraidStatePath
$statePayload.lastSchedulerExecutionReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSchedulerExecutionReceiptBundle')
$statePayload.schedulerExecutionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerExecutionReceiptStatePath
$statePayload.lastUnattendedIntervalConcordanceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastUnattendedIntervalConcordanceBundle')
$statePayload.unattendedIntervalConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $unattendedIntervalConcordanceStatePath
$statePayload.lastStaleSurfaceContradictionWatchBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastStaleSurfaceContradictionWatchBundle')
$statePayload.staleSurfaceContradictionWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $staleSurfaceContradictionWatchStatePath
$statePayload.lastUnattendedProofCollapseBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastUnattendedProofCollapseBundle')
$statePayload.unattendedProofCollapseStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $unattendedProofCollapseStatePath
$statePayload.lastDormantWindowLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDormantWindowLedgerBundle')
$statePayload.dormantWindowLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $dormantWindowLedgerStatePath
$statePayload.lastSilentCadenceIntegrityBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSilentCadenceIntegrityBundle')
$statePayload.silentCadenceIntegrityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $silentCadenceIntegrityStatePath
$statePayload.lastLongFormPhaseWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastLongFormPhaseWitnessBundle')
$statePayload.longFormPhaseWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $longFormPhaseWitnessStatePath
$statePayload.lastLongFormWindowBoundaryBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastLongFormWindowBoundaryBundle')
$statePayload.longFormWindowBoundaryStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $longFormWindowBoundaryStatePath
$statePayload.lastAutonomousLongFormCollapseBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastAutonomousLongFormCollapseBundle')
$statePayload.autonomousLongFormCollapseStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $autonomousLongFormCollapseStatePath
$statePayload.lastSchedulerProofHarvestBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSchedulerProofHarvestBundle')
$statePayload.schedulerProofHarvestStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerProofHarvestStatePath
$statePayload.lastIntervalOriginClarificationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastIntervalOriginClarificationBundle')
$statePayload.intervalOriginClarificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $intervalOriginClarificationStatePath
$statePayload.lastQueuedTaskMapPromotionBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastQueuedTaskMapPromotionBundle')
$statePayload.queuedTaskMapPromotionStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $queuedTaskMapPromotionStatePath
$statePayload.masterThreadOrchestrationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $masterThreadOrchestrationStatePath
$statePayload.lastRuntimeDeployabilityEnvelopeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRuntimeDeployabilityEnvelopeBundle')
$statePayload.runtimeDeployabilityEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeDeployabilityEnvelopeStatePath
$statePayload.lastSanctuaryRuntimeReadinessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSanctuaryRuntimeReadinessBundle')
$statePayload.sanctuaryRuntimeReadinessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sanctuaryRuntimeReadinessStatePath
$statePayload.lastRuntimeWorkSurfaceAdmissibilityBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRuntimeWorkSurfaceAdmissibilityBundle')
$statePayload.runtimeWorkSurfaceAdmissibilityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeWorkSurfaceAdmissibilityStatePath
$statePayload.lastReachAccessTopologyLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReachAccessTopologyLedgerBundle')
$statePayload.reachAccessTopologyLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachAccessTopologyLedgerStatePath
$statePayload.lastBondedOperatorLocalityReadinessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBondedOperatorLocalityReadinessBundle')
$statePayload.bondedOperatorLocalityReadinessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedOperatorLocalityReadinessStatePath
$statePayload.lastProtectedStateLegibilitySurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastProtectedStateLegibilitySurfaceBundle')
$statePayload.protectedStateLegibilitySurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $protectedStateLegibilitySurfaceStatePath
$statePayload.lastNexusSingularPortalFacadeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastNexusSingularPortalFacadeBundle')
$statePayload.nexusSingularPortalFacadeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $nexusSingularPortalFacadeStatePath
$statePayload.lastDuplexPredicateEnvelopeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDuplexPredicateEnvelopeBundle')
$statePayload.duplexPredicateEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $duplexPredicateEnvelopeStatePath
$statePayload.lastOperatorActualWorkSessionRehearsalBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastOperatorActualWorkSessionRehearsalBundle')
$statePayload.operatorActualWorkSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operatorActualWorkSessionRehearsalStatePath
$statePayload.lastIdentityInvariantThreadRootBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastIdentityInvariantThreadRootBundle')
$statePayload.identityInvariantThreadRootStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $identityInvariantThreadRootStatePath
$statePayload.lastGovernedThreadBirthReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastGovernedThreadBirthReceiptBundle')
$statePayload.governedThreadBirthReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $governedThreadBirthReceiptStatePath
$statePayload.lastInterWorkerBraidHandoffPacketBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastInterWorkerBraidHandoffPacketBundle')
$statePayload.interWorkerBraidHandoffPacketStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $interWorkerBraidHandoffPacketStatePath
$statePayload.lastAgentiCoreActualUtilitySurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastAgentiCoreActualUtilitySurfaceBundle')
$statePayload.agentiCoreActualUtilitySurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $agentiCoreActualUtilitySurfaceStatePath
$statePayload.lastReachDuplexRealizationSeamBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReachDuplexRealizationSeamBundle')
$statePayload.reachDuplexRealizationSeamStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachDuplexRealizationSeamStatePath
$statePayload.lastBondedParticipationLocalityLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBondedParticipationLocalityLedgerBundle')
$statePayload.bondedParticipationLocalityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedParticipationLocalityLedgerStatePath
$statePayload.lastSanctuaryRuntimeWorkbenchSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSanctuaryRuntimeWorkbenchSurfaceBundle')
$statePayload.sanctuaryRuntimeWorkbenchSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sanctuaryRuntimeWorkbenchSurfaceStatePath
$statePayload.lastAmenableDayDreamTierAdmissibilityBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastAmenableDayDreamTierAdmissibilityBundle')
$statePayload.amenableDayDreamTierAdmissibilityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $amenableDayDreamTierAdmissibilityStatePath
$statePayload.lastSelfRootedCrypticDepthGateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSelfRootedCrypticDepthGateBundle')
$statePayload.selfRootedCrypticDepthGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $selfRootedCrypticDepthGateStatePath
$statePayload.lastRuntimeWorkbenchSessionLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRuntimeWorkbenchSessionLedgerBundle')
$statePayload.runtimeWorkbenchSessionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeWorkbenchSessionLedgerStatePath
$statePayload.lastCompanionToolTelemetryBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCompanionToolTelemetryBundle')
$statePayload.companionToolTelemetryStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $companionToolTelemetryStatePath
$statePayload.lastV111EnrichmentPathwayBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastV111EnrichmentPathwayBundle')
$statePayload.v111EnrichmentPathwayStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $v111EnrichmentPathwayStatePath
$statePayload.lastRunIsolatedBuildPathwayBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRunIsolatedBuildPathwayBundle')
$statePayload.runIsolatedBuildPathwayStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runIsolatedBuildPathwayStatePath
$statePayload.lastDayDreamCollapseReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDayDreamCollapseReceiptBundle')
$statePayload.dayDreamCollapseReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $dayDreamCollapseReceiptStatePath
$statePayload.lastCrypticDepthReturnReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCrypticDepthReturnReceiptBundle')
$statePayload.crypticDepthReturnReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $crypticDepthReturnReceiptStatePath
$statePayload.lastBondedCoWorkSessionRehearsalBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBondedCoWorkSessionRehearsalBundle')
$statePayload.bondedCoWorkSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedCoWorkSessionRehearsalStatePath
$statePayload.lastReachReturnDissolutionReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReachReturnDissolutionReceiptBundle')
$statePayload.reachReturnDissolutionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachReturnDissolutionReceiptStatePath
$statePayload.lastLocalityDistinctionWitnessLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastLocalityDistinctionWitnessLedgerBundle')
$statePayload.localityDistinctionWitnessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $localityDistinctionWitnessLedgerStatePath
$statePayload.lastLocalHostSanctuaryResidencyEnvelopeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastLocalHostSanctuaryResidencyEnvelopeBundle')
$statePayload.localHostSanctuaryResidencyEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $localHostSanctuaryResidencyEnvelopeStatePath
$statePayload.lastRuntimeHabitationReadinessLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRuntimeHabitationReadinessLedgerBundle')
$statePayload.runtimeHabitationReadinessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeHabitationReadinessLedgerStatePath
$statePayload.lastBoundedInhabitationLaunchRehearsalBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBoundedInhabitationLaunchRehearsalBundle')
$statePayload.boundedInhabitationLaunchRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundedInhabitationLaunchRehearsalStatePath
$statePayload.lastPostHabitationHorizonLatticeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPostHabitationHorizonLatticeBundle')
$statePayload.postHabitationHorizonLatticeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postHabitationHorizonLatticeStatePath
$statePayload.lastBoundedHorizonResearchBriefBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBoundedHorizonResearchBriefBundle')
$statePayload.boundedHorizonResearchBriefStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundedHorizonResearchBriefStatePath
$statePayload.lastNextEraBatchSelectorBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastNextEraBatchSelectorBundle')
$statePayload.nextEraBatchSelectorStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $nextEraBatchSelectorStatePath
$statePayload.lastInquirySessionDisciplineSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastInquirySessionDisciplineSurfaceBundle')
$statePayload.inquirySessionDisciplineSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $inquirySessionDisciplineSurfaceStatePath
$statePayload.lastBoundaryConditionLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBoundaryConditionLedgerBundle')
$statePayload.boundaryConditionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundaryConditionLedgerStatePath
$statePayload.lastCoherenceGainWitnessReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCoherenceGainWitnessReceiptBundle')
$statePayload.coherenceGainWitnessReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coherenceGainWitnessReceiptStatePath
$statePayload.lastOperatorInquirySelectionEnvelopeBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastOperatorInquirySelectionEnvelopeBundle')
$statePayload.operatorInquirySelectionEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operatorInquirySelectionEnvelopeStatePath
$statePayload.lastBondedCrucibleSessionRehearsalBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBondedCrucibleSessionRehearsalBundle')
$statePayload.bondedCrucibleSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedCrucibleSessionRehearsalStatePath
$statePayload.lastSharedBoundaryMemoryLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastSharedBoundaryMemoryLedgerBundle')
$statePayload.sharedBoundaryMemoryLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sharedBoundaryMemoryLedgerStatePath
$statePayload.lastContinuityUnderPressureLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastContinuityUnderPressureLedgerBundle')
$statePayload.continuityUnderPressureLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $continuityUnderPressureLedgerStatePath
$statePayload.lastExpressiveDeformationReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastExpressiveDeformationReceiptBundle')
$statePayload.expressiveDeformationReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $expressiveDeformationReceiptStatePath
$statePayload.lastMutualIntelligibilityWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastMutualIntelligibilityWitnessBundle')
$statePayload.mutualIntelligibilityWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $mutualIntelligibilityWitnessStatePath
$statePayload.lastInquiryPatternContinuityLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastInquiryPatternContinuityLedgerBundle')
$statePayload.inquiryPatternContinuityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $inquiryPatternContinuityLedgerStatePath
$statePayload.lastQuestioningBoundaryPairLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastQuestioningBoundaryPairLedgerBundle')
$statePayload.questioningBoundaryPairLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningBoundaryPairLedgerStatePath
$statePayload.lastCarryForwardInquirySelectionSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCarryForwardInquirySelectionSurfaceBundle')
$statePayload.carryForwardInquirySelectionSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $carryForwardInquirySelectionSurfaceStatePath
$statePayload.lastEngramDistanceClassificationLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastEngramDistanceClassificationLedgerBundle')
$statePayload.engramDistanceClassificationLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramDistanceClassificationLedgerStatePath
$statePayload.lastEngramPromotionRequirementsMatrixBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastEngramPromotionRequirementsMatrixBundle')
$statePayload.engramPromotionRequirementsMatrixStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramPromotionRequirementsMatrixStatePath
$statePayload.lastDistanceWeightedQuestioningAdmissionSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDistanceWeightedQuestioningAdmissionSurfaceBundle')
$statePayload.distanceWeightedQuestioningAdmissionSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $distanceWeightedQuestioningAdmissionSurfaceStatePath
$statePayload.lastQuestioningOperatorCandidateLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastQuestioningOperatorCandidateLedgerBundle')
$statePayload.questioningOperatorCandidateLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningOperatorCandidateLedgerStatePath
$statePayload.lastQuestioningGelPromotionGateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastQuestioningGelPromotionGateBundle')
$statePayload.questioningGelPromotionGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningGelPromotionGateStatePath
$statePayload.lastProtectedQuestioningPatternSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastProtectedQuestioningPatternSurfaceBundle')
$statePayload.protectedQuestioningPatternSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $protectedQuestioningPatternSurfaceStatePath
$statePayload.lastVariationTestedReentryLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastVariationTestedReentryLedgerBundle')
$statePayload.variationTestedReentryLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $variationTestedReentryLedgerStatePath
$statePayload.lastQuestioningAdmissionRefusalReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastQuestioningAdmissionRefusalReceiptBundle')
$statePayload.questioningAdmissionRefusalReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningAdmissionRefusalReceiptStatePath
$statePayload.lastPromotionSeductionWatchBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastPromotionSeductionWatchBundle')
$statePayload.promotionSeductionWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $promotionSeductionWatchStatePath
$statePayload.lastEngramIntentFieldLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastEngramIntentFieldLedgerBundle')
$statePayload.engramIntentFieldLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramIntentFieldLedgerStatePath
$statePayload.lastIntentConstraintAlignmentReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastIntentConstraintAlignmentReceiptBundle')
$statePayload.intentConstraintAlignmentReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $intentConstraintAlignmentReceiptStatePath
$statePayload.lastWarmReactivationDispositionReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastWarmReactivationDispositionReceiptBundle')
$statePayload.warmReactivationDispositionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $warmReactivationDispositionReceiptStatePath
$statePayload.lastFormationPhaseVectorBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastFormationPhaseVectorBundle')
$statePayload.formationPhaseVectorStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $formationPhaseVectorStatePath
$statePayload.lastBrittlenessWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBrittlenessWitnessBundle')
$statePayload.brittlenessWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $brittlenessWitnessStatePath
$statePayload.lastDurabilityWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDurabilityWitnessBundle')
$statePayload.durabilityWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $durabilityWitnessStatePath
$statePayload.lastWarmClockDispositionBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastWarmClockDispositionBundle')
$statePayload.warmClockDispositionStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $warmClockDispositionStatePath
$statePayload.lastRipeningStalenessLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastRipeningStalenessLedgerBundle')
$statePayload.ripeningStalenessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $ripeningStalenessLedgerStatePath
$statePayload.lastCoolingPressureWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCoolingPressureWitnessBundle')
$statePayload.coolingPressureWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coolingPressureWitnessStatePath
$statePayload.lastHotReactivationTriggerReceiptBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastHotReactivationTriggerReceiptBundle')
$statePayload.hotReactivationTriggerReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $hotReactivationTriggerReceiptStatePath
$statePayload.lastColdAdmissionEligibilityGateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastColdAdmissionEligibilityGateBundle')
$statePayload.coldAdmissionEligibilityGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coldAdmissionEligibilityGateStatePath
$statePayload.lastArchiveDispositionLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastArchiveDispositionLedgerBundle')
$statePayload.archiveDispositionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $archiveDispositionLedgerStatePath
$statePayload.lastInterlockDensityLedgerBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastInterlockDensityLedgerBundle')
$statePayload.interlockDensityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $interlockDensityLedgerStatePath
$statePayload.lastBrittleDurableDifferentiationSurfaceBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastBrittleDurableDifferentiationSurfaceBundle')
$statePayload.brittleDurableDifferentiationSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $brittleDurableDifferentiationSurfaceStatePath
$statePayload.lastCoreInvariantLatticeWitnessBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastCoreInvariantLatticeWitnessBundle')
$statePayload.coreInvariantLatticeWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coreInvariantLatticeWitnessStatePath
Write-JsonFile -Path $statePath -Value $statePayload

$blockedEscalationBundlePath = $null
if ($latestStatus -eq $blockedStatus) {
    $blockedEscalationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Blocked-EscalationBundle.ps1'
    $blockedEscalationArgs = @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $blockedEscalationScriptPath,
        '-RepoRoot', $resolvedRepoRoot,
        '-ManifestPath', $manifestPath,
        '-CyclePolicyPath', $resolvedPolicyPath
    )

    if (-not [string]::IsNullOrWhiteSpace($digestBundlePath)) {
        $blockedEscalationArgs += @('-DigestBundlePath', $digestBundlePath)
    }

    $blockedEscalationOutput = Invoke-ChildPowershellScript -ArgumentList $blockedEscalationArgs -FailureContext 'Blocked escalation bundle writer'
    $blockedEscalationBundlePath = Get-ScriptOutputTail -Output $blockedEscalationOutput
    $statePayload.lastBlockedEscalationBundle = $blockedEscalationBundlePath
    $statePayload.blockedEscalationTriggered = $true
    Write-JsonFile -Path $statePath -Value $statePayload
}

$retentionScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Automation-RetentionPruning.ps1'
$retentionOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $retentionScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Automation retention pruning'
$retentionStatePathFromRun = Get-ScriptOutputTail -Output $retentionOutput
if (-not [string]::IsNullOrWhiteSpace($retentionStatePathFromRun)) {
    $statePayload.retentionStatePath = $retentionStatePathFromRun
    Write-JsonFile -Path $statePath -Value $statePayload
}

$seededGovernanceScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Seeded-Build-Governance.ps1'
$seededGovernanceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $seededGovernanceScriptPath, '-Configuration', $Configuration, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Seeded build governance'
$seededGovernanceBundlePath = Get-ScriptOutputTail -Output $seededGovernanceOutput
if (-not [string]::IsNullOrWhiteSpace($seededGovernanceBundlePath)) {
    $statePayload.lastSeededGovernanceBundle = $seededGovernanceBundlePath
    $statePayload.seededGovernanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seededGovernanceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$schedulerSyncScriptPath = Join-Path $resolvedRepoRoot 'tools\Sync-Local-AutomationScheduler.ps1'
$schedulerSyncOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $schedulerSyncScriptPath, '-Configuration', $Configuration, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Scheduler reconciliation'
$schedulerSyncStatePathFromRun = Get-ScriptOutputTail -Output $schedulerSyncOutput
if (-not [string]::IsNullOrWhiteSpace($schedulerSyncStatePathFromRun)) {
    $statePayload.schedulerReconciliationStatePath = $schedulerSyncStatePathFromRun
    Write-JsonFile -Path $statePath -Value $statePayload
}

$cmeConsolidationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CmeFormalization-ConsolidationStatus.ps1'
$cmeConsolidationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $cmeConsolidationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'CME consolidation writer'
$cmeConsolidationStatePathFromRun = Get-ScriptOutputTail -Output $cmeConsolidationOutput
if (-not [string]::IsNullOrWhiteSpace($cmeConsolidationStatePathFromRun)) {
    $statePayload.cmeConsolidationStatePath = $cmeConsolidationStatePathFromRun
    Write-JsonFile -Path $statePath -Value $statePayload
}

$promotionGateScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Promotion-GateBundle.ps1'
$promotionGateOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $promotionGateScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Promotion gate bundle writer'
$promotionGateBundlePath = Get-ScriptOutputTail -Output $promotionGateOutput
if (-not [string]::IsNullOrWhiteSpace($promotionGateBundlePath)) {
    $statePayload.lastPromotionGateBundle = $promotionGateBundlePath
    $statePayload.promotionGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $promotionGateStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$ciConcordanceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CiArtifactConcordance.ps1'
$ciConcordanceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $ciConcordanceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'CI artifact concordance writer'
$ciConcordanceBundlePath = Get-ScriptOutputTail -Output $ciConcordanceOutput
if (-not [string]::IsNullOrWhiteSpace($ciConcordanceBundlePath)) {
    $statePayload.lastCiConcordanceBundle = $ciConcordanceBundlePath
    $statePayload.ciConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $ciConcordanceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$releaseRatificationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Release-RatificationRehearsal.ps1'
$releaseRatificationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $releaseRatificationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Release ratification rehearsal writer'
$releaseRatificationBundlePath = Get-ScriptOutputTail -Output $releaseRatificationOutput
if (-not [string]::IsNullOrWhiteSpace($releaseRatificationBundlePath)) {
    $statePayload.lastReleaseRatificationBundle = $releaseRatificationBundlePath
    $statePayload.releaseRatificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $releaseRatificationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$firstPublishIntentScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-FirstPublish-IntentClosure.ps1'
$firstPublishIntentOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $firstPublishIntentScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'First publish intent writer'
$firstPublishIntentBundlePath = Get-ScriptOutputTail -Output $firstPublishIntentOutput
if (-not [string]::IsNullOrWhiteSpace($firstPublishIntentBundlePath)) {
    $statePayload.lastFirstPublishIntentBundle = $firstPublishIntentBundlePath
    $statePayload.firstPublishIntentStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $firstPublishIntentStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$seededPromotionReviewScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Seeded-PromotionReview.ps1'
$seededPromotionReviewOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $seededPromotionReviewScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Seeded promotion review writer'
$seededPromotionReviewBundlePath = Get-ScriptOutputTail -Output $seededPromotionReviewOutput
if (-not [string]::IsNullOrWhiteSpace($seededPromotionReviewBundlePath)) {
    $statePayload.lastSeededPromotionReviewBundle = $seededPromotionReviewBundlePath
    $statePayload.seededPromotionReviewStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seededPromotionReviewStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$releaseHandshakeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Release-HandshakeSurface.ps1'
$releaseHandshakeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $releaseHandshakeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Release handshake writer'
$releaseHandshakeBundlePath = Get-ScriptOutputTail -Output $releaseHandshakeOutput
if (-not [string]::IsNullOrWhiteSpace($releaseHandshakeBundlePath)) {
    $statePayload.lastReleaseHandshakeBundle = $releaseHandshakeBundlePath
    $statePayload.releaseHandshakeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $releaseHandshakeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$cmeFormationAndOfficeLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CmeFormationAndOfficeLedger.ps1'
$cmeFormationAndOfficeLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $cmeFormationAndOfficeLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'CME formation and office ledger writer'
$cmeFormationAndOfficeLedgerBundlePath = Get-ScriptOutputTail -Output $cmeFormationAndOfficeLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($cmeFormationAndOfficeLedgerBundlePath)) {
    $statePayload.lastCmeFormationAndOfficeLedgerBundle = $cmeFormationAndOfficeLedgerBundlePath
    $statePayload.cmeFormationAndOfficeLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $cmeFormationAndOfficeLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$publishRequestEnvelopeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Publish-RequestEnvelope.ps1'
$publishRequestEnvelopeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $publishRequestEnvelopeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Publish request envelope writer'
$publishRequestEnvelopeBundlePath = Get-ScriptOutputTail -Output $publishRequestEnvelopeOutput
if (-not [string]::IsNullOrWhiteSpace($publishRequestEnvelopeBundlePath)) {
    $statePayload.lastPublishRequestEnvelopeBundle = $publishRequestEnvelopeBundlePath
    $statePayload.publishRequestEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publishRequestEnvelopeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$postPublishEvidenceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PostPublish-EvidenceLoop.ps1'
$postPublishEvidenceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $postPublishEvidenceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Post-publish evidence writer'
$postPublishEvidenceBundlePath = Get-ScriptOutputTail -Output $postPublishEvidenceOutput
if (-not [string]::IsNullOrWhiteSpace($postPublishEvidenceBundlePath)) {
    $statePayload.lastPostPublishEvidenceBundle = $postPublishEvidenceBundlePath
    $statePayload.postPublishEvidenceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishEvidenceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$seedBraidEscalationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SeedBraid-EscalationLane.ps1'
$seedBraidEscalationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $seedBraidEscalationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Seed braid escalation writer'
$seedBraidEscalationBundlePath = Get-ScriptOutputTail -Output $seedBraidEscalationOutput
if (-not [string]::IsNullOrWhiteSpace($seedBraidEscalationBundlePath)) {
    $statePayload.lastSeedBraidEscalationBundle = $seedBraidEscalationBundlePath
    $statePayload.seedBraidEscalationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $seedBraidEscalationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$publishedRuntimeReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PublishedRuntime-Receipt.ps1'
$publishedRuntimeReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $publishedRuntimeReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Published runtime receipt writer'
$publishedRuntimeReceiptBundlePath = Get-ScriptOutputTail -Output $publishedRuntimeReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($publishedRuntimeReceiptBundlePath)) {
    $statePayload.lastPublishedRuntimeReceiptBundle = $publishedRuntimeReceiptBundlePath
    $statePayload.publishedRuntimeReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publishedRuntimeReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$artifactAttestationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Artifact-AttestationSurface.ps1'
$artifactAttestationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $artifactAttestationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Artifact attestation writer'
$artifactAttestationBundlePath = Get-ScriptOutputTail -Output $artifactAttestationOutput
if (-not [string]::IsNullOrWhiteSpace($artifactAttestationBundlePath)) {
    $statePayload.lastArtifactAttestationBundle = $artifactAttestationBundlePath
    $statePayload.artifactAttestationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $artifactAttestationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$postPublishDriftWatchScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PostPublish-DriftWatch.ps1'
$postPublishDriftWatchOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $postPublishDriftWatchScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Post-publish drift watch writer'
$postPublishDriftWatchBundlePath = Get-ScriptOutputTail -Output $postPublishDriftWatchOutput
if (-not [string]::IsNullOrWhiteSpace($postPublishDriftWatchBundlePath)) {
    $statePayload.lastPostPublishDriftWatchBundle = $postPublishDriftWatchBundlePath
    $statePayload.postPublishDriftWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishDriftWatchStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$operationalPublicationLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-OperationalPublication-Ledger.ps1'
$operationalPublicationLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $operationalPublicationLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Operational publication ledger writer'
$operationalPublicationLedgerBundlePath = Get-ScriptOutputTail -Output $operationalPublicationLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($operationalPublicationLedgerBundlePath)) {
    $statePayload.lastOperationalPublicationLedgerBundle = $operationalPublicationLedgerBundlePath
    $statePayload.operationalPublicationLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operationalPublicationLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$externalConsumerConcordanceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ExternalConsumer-Concordance.ps1'
$externalConsumerConcordanceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $externalConsumerConcordanceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'External consumer concordance writer'
$externalConsumerConcordanceBundlePath = Get-ScriptOutputTail -Output $externalConsumerConcordanceOutput
if (-not [string]::IsNullOrWhiteSpace($externalConsumerConcordanceBundlePath)) {
    $statePayload.lastExternalConsumerConcordanceBundle = $externalConsumerConcordanceBundlePath
    $statePayload.externalConsumerConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $externalConsumerConcordanceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$postPublishGovernanceLoopScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PostPublish-GovernanceLoop.ps1'
$postPublishGovernanceLoopOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $postPublishGovernanceLoopScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Post-publish governance loop writer'
$postPublishGovernanceLoopBundlePath = Get-ScriptOutputTail -Output $postPublishGovernanceLoopOutput
if (-not [string]::IsNullOrWhiteSpace($postPublishGovernanceLoopBundlePath)) {
    $statePayload.lastPostPublishGovernanceLoopBundle = $postPublishGovernanceLoopBundlePath
    $statePayload.postPublishGovernanceLoopStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postPublishGovernanceLoopStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$publicationCadenceLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Publication-CadenceLedger.ps1'
$publicationCadenceLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $publicationCadenceLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Publication cadence ledger writer'
$publicationCadenceLedgerBundlePath = Get-ScriptOutputTail -Output $publicationCadenceLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($publicationCadenceLedgerBundlePath)) {
    $statePayload.lastPublicationCadenceLedgerBundle = $publicationCadenceLedgerBundlePath
    $statePayload.publicationCadenceLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $publicationCadenceLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$downstreamRuntimeObservationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-DownstreamRuntime-Observation.ps1'
$downstreamRuntimeObservationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $downstreamRuntimeObservationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Downstream runtime observation writer'
$downstreamRuntimeObservationBundlePath = Get-ScriptOutputTail -Output $downstreamRuntimeObservationOutput
if (-not [string]::IsNullOrWhiteSpace($downstreamRuntimeObservationBundlePath)) {
    $statePayload.lastDownstreamRuntimeObservationBundle = $downstreamRuntimeObservationBundlePath
    $statePayload.downstreamRuntimeObservationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $downstreamRuntimeObservationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$multiIntervalGovernanceBraidScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-MultiInterval-GovernanceBraid.ps1'
$multiIntervalGovernanceBraidOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $multiIntervalGovernanceBraidScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Multi-interval governance braid writer'
$multiIntervalGovernanceBraidBundlePath = Get-ScriptOutputTail -Output $multiIntervalGovernanceBraidOutput
if (-not [string]::IsNullOrWhiteSpace($multiIntervalGovernanceBraidBundlePath)) {
    $statePayload.lastMultiIntervalGovernanceBraidBundle = $multiIntervalGovernanceBraidBundlePath
    $statePayload.multiIntervalGovernanceBraidStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $multiIntervalGovernanceBraidStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$schedulerExecutionReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SchedulerExecution-Receipt.ps1'
$schedulerExecutionReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $schedulerExecutionReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Scheduler execution receipt writer'
$schedulerExecutionReceiptBundlePath = Get-ScriptOutputTail -Output $schedulerExecutionReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($schedulerExecutionReceiptBundlePath)) {
    $statePayload.lastSchedulerExecutionReceiptBundle = $schedulerExecutionReceiptBundlePath
    $statePayload.schedulerExecutionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerExecutionReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$unattendedIntervalConcordanceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-UnattendedInterval-Concordance.ps1'
$unattendedIntervalConcordanceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $unattendedIntervalConcordanceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Unattended interval concordance writer'
$unattendedIntervalConcordanceBundlePath = Get-ScriptOutputTail -Output $unattendedIntervalConcordanceOutput
if (-not [string]::IsNullOrWhiteSpace($unattendedIntervalConcordanceBundlePath)) {
    $statePayload.lastUnattendedIntervalConcordanceBundle = $unattendedIntervalConcordanceBundlePath
    $statePayload.unattendedIntervalConcordanceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $unattendedIntervalConcordanceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$staleSurfaceContradictionWatchScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-StaleSurface-ContradictionWatch.ps1'
$staleSurfaceContradictionWatchOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $staleSurfaceContradictionWatchScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Stale surface contradiction watch writer'
$staleSurfaceContradictionWatchBundlePath = Get-ScriptOutputTail -Output $staleSurfaceContradictionWatchOutput
if (-not [string]::IsNullOrWhiteSpace($staleSurfaceContradictionWatchBundlePath)) {
    $statePayload.lastStaleSurfaceContradictionWatchBundle = $staleSurfaceContradictionWatchBundlePath
    $statePayload.staleSurfaceContradictionWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $staleSurfaceContradictionWatchStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$unattendedProofCollapseScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-UnattendedProof-Collapse.ps1'
$unattendedProofCollapseOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $unattendedProofCollapseScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Unattended proof collapse writer'
$unattendedProofCollapseBundlePath = Get-ScriptOutputTail -Output $unattendedProofCollapseOutput
if (-not [string]::IsNullOrWhiteSpace($unattendedProofCollapseBundlePath)) {
    $statePayload.lastUnattendedProofCollapseBundle = $unattendedProofCollapseBundlePath
    $statePayload.unattendedProofCollapseStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $unattendedProofCollapseStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$dormantWindowLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-DormantWindow-Ledger.ps1'
$dormantWindowLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $dormantWindowLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Dormant window ledger writer'
$dormantWindowLedgerBundlePath = Get-ScriptOutputTail -Output $dormantWindowLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($dormantWindowLedgerBundlePath)) {
    $statePayload.lastDormantWindowLedgerBundle = $dormantWindowLedgerBundlePath
    $statePayload.dormantWindowLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $dormantWindowLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$silentCadenceIntegrityScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SilentCadence-Integrity.ps1'
$silentCadenceIntegrityOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $silentCadenceIntegrityScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Silent cadence integrity writer'
$silentCadenceIntegrityBundlePath = Get-ScriptOutputTail -Output $silentCadenceIntegrityOutput
if (-not [string]::IsNullOrWhiteSpace($silentCadenceIntegrityBundlePath)) {
    $statePayload.lastSilentCadenceIntegrityBundle = $silentCadenceIntegrityBundlePath
    $statePayload.silentCadenceIntegrityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $silentCadenceIntegrityStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$longFormPhaseWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-LongForm-PhaseWitness.ps1'
$longFormPhaseWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $longFormPhaseWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Long-form phase witness writer'
$longFormPhaseWitnessBundlePath = Get-ScriptOutputTail -Output $longFormPhaseWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($longFormPhaseWitnessBundlePath)) {
    $statePayload.lastLongFormPhaseWitnessBundle = $longFormPhaseWitnessBundlePath
    $statePayload.longFormPhaseWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $longFormPhaseWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$longFormWindowBoundaryScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-LongForm-WindowBoundaryReceipt.ps1'
$longFormWindowBoundaryOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $longFormWindowBoundaryScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Long-form window boundary writer'
$longFormWindowBoundaryBundlePath = Get-ScriptOutputTail -Output $longFormWindowBoundaryOutput
if (-not [string]::IsNullOrWhiteSpace($longFormWindowBoundaryBundlePath)) {
    $statePayload.lastLongFormWindowBoundaryBundle = $longFormWindowBoundaryBundlePath
    $statePayload.longFormWindowBoundaryStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $longFormWindowBoundaryStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$autonomousLongFormCollapseScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Autonomous-LongFormRunCollapse.ps1'
$autonomousLongFormCollapseOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $autonomousLongFormCollapseScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Autonomous long-form collapse writer'
$autonomousLongFormCollapseBundlePath = Get-ScriptOutputTail -Output $autonomousLongFormCollapseOutput
if (-not [string]::IsNullOrWhiteSpace($autonomousLongFormCollapseBundlePath)) {
    $statePayload.lastAutonomousLongFormCollapseBundle = $autonomousLongFormCollapseBundlePath
    $statePayload.autonomousLongFormCollapseStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $autonomousLongFormCollapseStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$schedulerProofHarvestScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SchedulerProof-Harvest.ps1'
$schedulerProofHarvestOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $schedulerProofHarvestScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Scheduler proof harvest writer'
$schedulerProofHarvestBundlePath = Get-ScriptOutputTail -Output $schedulerProofHarvestOutput
if (-not [string]::IsNullOrWhiteSpace($schedulerProofHarvestBundlePath)) {
    $statePayload.lastSchedulerProofHarvestBundle = $schedulerProofHarvestBundlePath
    $statePayload.schedulerProofHarvestStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerProofHarvestStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$intervalOriginClarificationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Interval-OriginClarification.ps1'
$intervalOriginClarificationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $intervalOriginClarificationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Interval origin clarification writer'
$intervalOriginClarificationBundlePath = Get-ScriptOutputTail -Output $intervalOriginClarificationOutput
if (-not [string]::IsNullOrWhiteSpace($intervalOriginClarificationBundlePath)) {
    $statePayload.lastIntervalOriginClarificationBundle = $intervalOriginClarificationBundlePath
    $statePayload.intervalOriginClarificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $intervalOriginClarificationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$queuedTaskMapPromotionScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Queued-TaskMapPromotion.ps1'
$queuedTaskMapPromotionOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $queuedTaskMapPromotionScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Queued task map promotion writer'
$queuedTaskMapPromotionBundlePath = Get-ScriptOutputTail -Output $queuedTaskMapPromotionOutput
if (-not [string]::IsNullOrWhiteSpace($queuedTaskMapPromotionBundlePath)) {
    $statePayload.lastQueuedTaskMapPromotionBundle = $queuedTaskMapPromotionBundlePath
    $statePayload.queuedTaskMapPromotionStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $queuedTaskMapPromotionStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$runtimeDeployabilityEnvelopeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Runtime-DeployabilityEnvelope.ps1'
$runtimeDeployabilityEnvelopeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $runtimeDeployabilityEnvelopeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Runtime deployability envelope writer'
$runtimeDeployabilityEnvelopeBundlePath = Get-ScriptOutputTail -Output $runtimeDeployabilityEnvelopeOutput
if (-not [string]::IsNullOrWhiteSpace($runtimeDeployabilityEnvelopeBundlePath)) {
    $statePayload.lastRuntimeDeployabilityEnvelopeBundle = $runtimeDeployabilityEnvelopeBundlePath
    $statePayload.runtimeDeployabilityEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeDeployabilityEnvelopeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$sanctuaryRuntimeReadinessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Sanctuary-RuntimeReadinessReceipt.ps1'
$sanctuaryRuntimeReadinessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $sanctuaryRuntimeReadinessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Sanctuary runtime readiness writer'
$sanctuaryRuntimeReadinessBundlePath = Get-ScriptOutputTail -Output $sanctuaryRuntimeReadinessOutput
if (-not [string]::IsNullOrWhiteSpace($sanctuaryRuntimeReadinessBundlePath)) {
    $statePayload.lastSanctuaryRuntimeReadinessBundle = $sanctuaryRuntimeReadinessBundlePath
    $statePayload.sanctuaryRuntimeReadinessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sanctuaryRuntimeReadinessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$runtimeWorkSurfaceAdmissibilityScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Runtime-WorkSurfaceAdmissibility.ps1'
$runtimeWorkSurfaceAdmissibilityOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $runtimeWorkSurfaceAdmissibilityScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Runtime work-surface admissibility writer'
$runtimeWorkSurfaceAdmissibilityBundlePath = Get-ScriptOutputTail -Output $runtimeWorkSurfaceAdmissibilityOutput
if (-not [string]::IsNullOrWhiteSpace($runtimeWorkSurfaceAdmissibilityBundlePath)) {
    $statePayload.lastRuntimeWorkSurfaceAdmissibilityBundle = $runtimeWorkSurfaceAdmissibilityBundlePath
    $statePayload.runtimeWorkSurfaceAdmissibilityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeWorkSurfaceAdmissibilityStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$reachAccessTopologyLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Reach-AccessTopologyLedger.ps1'
$reachAccessTopologyLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $reachAccessTopologyLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Reach access-topology ledger writer'
$reachAccessTopologyLedgerBundlePath = Get-ScriptOutputTail -Output $reachAccessTopologyLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($reachAccessTopologyLedgerBundlePath)) {
    $statePayload.lastReachAccessTopologyLedgerBundle = $reachAccessTopologyLedgerBundlePath
    $statePayload.reachAccessTopologyLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachAccessTopologyLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$protectedStateLegibilitySurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ProtectedState-LegibilitySurface.ps1'
$protectedStateLegibilitySurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $protectedStateLegibilitySurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Protected-state legibility writer'
$protectedStateLegibilitySurfaceBundlePath = Get-ScriptOutputTail -Output $protectedStateLegibilitySurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($protectedStateLegibilitySurfaceBundlePath)) {
    $statePayload.lastProtectedStateLegibilitySurfaceBundle = $protectedStateLegibilitySurfaceBundlePath
    $statePayload.protectedStateLegibilitySurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $protectedStateLegibilitySurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$nexusSingularPortalFacadeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Nexus-SingularPortalFacade.ps1'
$nexusSingularPortalFacadeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $nexusSingularPortalFacadeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Nexus singular portal facade writer'
$nexusSingularPortalFacadeBundlePath = Get-ScriptOutputTail -Output $nexusSingularPortalFacadeOutput
if (-not [string]::IsNullOrWhiteSpace($nexusSingularPortalFacadeBundlePath)) {
    $statePayload.lastNexusSingularPortalFacadeBundle = $nexusSingularPortalFacadeBundlePath
    $statePayload.nexusSingularPortalFacadeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $nexusSingularPortalFacadeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$duplexPredicateEnvelopeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Duplex-PredicateEnvelope.ps1'
$duplexPredicateEnvelopeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $duplexPredicateEnvelopeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Duplex predicate envelope writer'
$duplexPredicateEnvelopeBundlePath = Get-ScriptOutputTail -Output $duplexPredicateEnvelopeOutput
if (-not [string]::IsNullOrWhiteSpace($duplexPredicateEnvelopeBundlePath)) {
    $statePayload.lastDuplexPredicateEnvelopeBundle = $duplexPredicateEnvelopeBundlePath
    $statePayload.duplexPredicateEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $duplexPredicateEnvelopeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$bondedOperatorLocalityReadinessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Bonded-OperatorLocalityReadiness.ps1'
$bondedOperatorLocalityReadinessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $bondedOperatorLocalityReadinessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bonded operator locality readiness writer'
$bondedOperatorLocalityReadinessBundlePath = Get-ScriptOutputTail -Output $bondedOperatorLocalityReadinessOutput
if (-not [string]::IsNullOrWhiteSpace($bondedOperatorLocalityReadinessBundlePath)) {
    $statePayload.lastBondedOperatorLocalityReadinessBundle = $bondedOperatorLocalityReadinessBundlePath
    $statePayload.bondedOperatorLocalityReadinessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedOperatorLocalityReadinessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$operatorActualWorkSessionRehearsalScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-OperatorActual-WorkSessionRehearsal.ps1'
$operatorActualWorkSessionRehearsalOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $operatorActualWorkSessionRehearsalScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Operator.actual work-session rehearsal writer'
$operatorActualWorkSessionRehearsalBundlePath = Get-ScriptOutputTail -Output $operatorActualWorkSessionRehearsalOutput
if (-not [string]::IsNullOrWhiteSpace($operatorActualWorkSessionRehearsalBundlePath)) {
    $statePayload.lastOperatorActualWorkSessionRehearsalBundle = $operatorActualWorkSessionRehearsalBundlePath
    $statePayload.operatorActualWorkSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operatorActualWorkSessionRehearsalStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$identityInvariantThreadRootScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-IdentityInvariant-ThreadRoot.ps1'
$identityInvariantThreadRootOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $identityInvariantThreadRootScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Identity-invariant thread-root writer'
$identityInvariantThreadRootBundlePath = Get-ScriptOutputTail -Output $identityInvariantThreadRootOutput
if (-not [string]::IsNullOrWhiteSpace($identityInvariantThreadRootBundlePath)) {
    $statePayload.lastIdentityInvariantThreadRootBundle = $identityInvariantThreadRootBundlePath
    $statePayload.identityInvariantThreadRootStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $identityInvariantThreadRootStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$governedThreadBirthReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Governed-ThreadBirthReceipt.ps1'
$governedThreadBirthReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $governedThreadBirthReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Governed thread-birth receipt writer'
$governedThreadBirthReceiptBundlePath = Get-ScriptOutputTail -Output $governedThreadBirthReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($governedThreadBirthReceiptBundlePath)) {
    $statePayload.lastGovernedThreadBirthReceiptBundle = $governedThreadBirthReceiptBundlePath
    $statePayload.governedThreadBirthReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $governedThreadBirthReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$interWorkerBraidHandoffPacketScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-InterWorker-BraidHandoffPacket.ps1'
$interWorkerBraidHandoffPacketOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $interWorkerBraidHandoffPacketScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Inter-worker braid-handoff packet writer'
$interWorkerBraidHandoffPacketBundlePath = Get-ScriptOutputTail -Output $interWorkerBraidHandoffPacketOutput
if (-not [string]::IsNullOrWhiteSpace($interWorkerBraidHandoffPacketBundlePath)) {
    $statePayload.lastInterWorkerBraidHandoffPacketBundle = $interWorkerBraidHandoffPacketBundlePath
    $statePayload.interWorkerBraidHandoffPacketStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $interWorkerBraidHandoffPacketStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$agentiCoreActualUtilitySurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-AgentiCoreActual-UtilitySurface.ps1'
$agentiCoreActualUtilitySurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $agentiCoreActualUtilitySurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'AgentiCore.actual utility-surface writer'
$agentiCoreActualUtilitySurfaceBundlePath = Get-ScriptOutputTail -Output $agentiCoreActualUtilitySurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($agentiCoreActualUtilitySurfaceBundlePath)) {
    $statePayload.lastAgentiCoreActualUtilitySurfaceBundle = $agentiCoreActualUtilitySurfaceBundlePath
    $statePayload.agentiCoreActualUtilitySurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $agentiCoreActualUtilitySurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$reachDuplexRealizationSeamScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Reach-DuplexRealizationSeam.ps1'
$reachDuplexRealizationSeamOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $reachDuplexRealizationSeamScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Reach duplex-realization seam writer'
$reachDuplexRealizationSeamBundlePath = Get-ScriptOutputTail -Output $reachDuplexRealizationSeamOutput
if (-not [string]::IsNullOrWhiteSpace($reachDuplexRealizationSeamBundlePath)) {
    $statePayload.lastReachDuplexRealizationSeamBundle = $reachDuplexRealizationSeamBundlePath
    $statePayload.reachDuplexRealizationSeamStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachDuplexRealizationSeamStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$bondedParticipationLocalityLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BondedParticipation-LocalityLedger.ps1'
$bondedParticipationLocalityLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $bondedParticipationLocalityLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bonded participation locality-ledger writer'
$bondedParticipationLocalityLedgerBundlePath = Get-ScriptOutputTail -Output $bondedParticipationLocalityLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($bondedParticipationLocalityLedgerBundlePath)) {
    $statePayload.lastBondedParticipationLocalityLedgerBundle = $bondedParticipationLocalityLedgerBundlePath
    $statePayload.bondedParticipationLocalityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedParticipationLocalityLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$sanctuaryRuntimeWorkbenchSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Sanctuary-RuntimeWorkbenchSurface.ps1'
$sanctuaryRuntimeWorkbenchSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $sanctuaryRuntimeWorkbenchSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Sanctuary runtime-workbench surface writer'
$sanctuaryRuntimeWorkbenchSurfaceBundlePath = Get-ScriptOutputTail -Output $sanctuaryRuntimeWorkbenchSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($sanctuaryRuntimeWorkbenchSurfaceBundlePath)) {
    $statePayload.lastSanctuaryRuntimeWorkbenchSurfaceBundle = $sanctuaryRuntimeWorkbenchSurfaceBundlePath
    $statePayload.sanctuaryRuntimeWorkbenchSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sanctuaryRuntimeWorkbenchSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$amenableDayDreamTierAdmissibilityScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Amenable-DayDreamTierAdmissibility.ps1'
$amenableDayDreamTierAdmissibilityOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $amenableDayDreamTierAdmissibilityScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Amenable day-dream tier admissibility writer'
$amenableDayDreamTierAdmissibilityBundlePath = Get-ScriptOutputTail -Output $amenableDayDreamTierAdmissibilityOutput
if (-not [string]::IsNullOrWhiteSpace($amenableDayDreamTierAdmissibilityBundlePath)) {
    $statePayload.lastAmenableDayDreamTierAdmissibilityBundle = $amenableDayDreamTierAdmissibilityBundlePath
    $statePayload.amenableDayDreamTierAdmissibilityStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $amenableDayDreamTierAdmissibilityStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$selfRootedCrypticDepthGateScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SelfRooted-CrypticDepthGate.ps1'
$selfRootedCrypticDepthGateOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $selfRootedCrypticDepthGateScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Self-rooted cryptic-depth gate writer'
$selfRootedCrypticDepthGateBundlePath = Get-ScriptOutputTail -Output $selfRootedCrypticDepthGateOutput
if (-not [string]::IsNullOrWhiteSpace($selfRootedCrypticDepthGateBundlePath)) {
    $statePayload.lastSelfRootedCrypticDepthGateBundle = $selfRootedCrypticDepthGateBundlePath
    $statePayload.selfRootedCrypticDepthGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $selfRootedCrypticDepthGateStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$runtimeWorkbenchSessionLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-RuntimeWorkbench-SessionLedger.ps1'
$runtimeWorkbenchSessionLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $runtimeWorkbenchSessionLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Runtime workbench session-ledger writer'
$runtimeWorkbenchSessionLedgerBundlePath = Get-ScriptOutputTail -Output $runtimeWorkbenchSessionLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($runtimeWorkbenchSessionLedgerBundlePath)) {
    $statePayload.lastRuntimeWorkbenchSessionLedgerBundle = $runtimeWorkbenchSessionLedgerBundlePath
    $statePayload.runtimeWorkbenchSessionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeWorkbenchSessionLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$companionToolTelemetryScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CompanionToolTelemetry.ps1'
$companionToolTelemetryOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $companionToolTelemetryScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Companion tool telemetry writer'
$companionToolTelemetryBundlePath = Get-ScriptOutputTail -Output $companionToolTelemetryOutput
if (-not [string]::IsNullOrWhiteSpace($companionToolTelemetryBundlePath)) {
    $statePayload.lastCompanionToolTelemetryBundle = $companionToolTelemetryBundlePath
    $statePayload.companionToolTelemetryStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $companionToolTelemetryStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$v111EnrichmentPathwayScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-V111-EnrichmentPathway.ps1'
$v111EnrichmentPathwayOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $v111EnrichmentPathwayScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'V1.1.1 enrichment pathway writer'
$v111EnrichmentPathwayBundlePath = Get-ScriptOutputTail -Output $v111EnrichmentPathwayOutput
if (-not [string]::IsNullOrWhiteSpace($v111EnrichmentPathwayBundlePath)) {
    $statePayload.lastV111EnrichmentPathwayBundle = $v111EnrichmentPathwayBundlePath
    $statePayload.v111EnrichmentPathwayStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $v111EnrichmentPathwayStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$runIsolatedBuildPathwayScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-RunIsolated-BuildPathway.ps1'
$runIsolatedBuildPathwayOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $runIsolatedBuildPathwayScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Run-isolated build pathway writer'
$runIsolatedBuildPathwayBundlePath = Get-ScriptOutputTail -Output $runIsolatedBuildPathwayOutput
if (-not [string]::IsNullOrWhiteSpace($runIsolatedBuildPathwayBundlePath)) {
    $statePayload.lastRunIsolatedBuildPathwayBundle = $runIsolatedBuildPathwayBundlePath
    $statePayload.runIsolatedBuildPathwayStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runIsolatedBuildPathwayStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$dayDreamCollapseReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-DayDream-CollapseReceipt.ps1'
$dayDreamCollapseReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $dayDreamCollapseReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Day-dream collapse receipt writer'
$dayDreamCollapseReceiptBundlePath = Get-ScriptOutputTail -Output $dayDreamCollapseReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($dayDreamCollapseReceiptBundlePath)) {
    $statePayload.lastDayDreamCollapseReceiptBundle = $dayDreamCollapseReceiptBundlePath
    $statePayload.dayDreamCollapseReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $dayDreamCollapseReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$crypticDepthReturnReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CrypticDepth-ReturnReceipt.ps1'
$crypticDepthReturnReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $crypticDepthReturnReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Cryptic-depth return receipt writer'
$crypticDepthReturnReceiptBundlePath = Get-ScriptOutputTail -Output $crypticDepthReturnReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($crypticDepthReturnReceiptBundlePath)) {
    $statePayload.lastCrypticDepthReturnReceiptBundle = $crypticDepthReturnReceiptBundlePath
    $statePayload.crypticDepthReturnReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $crypticDepthReturnReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$bondedCoWorkSessionRehearsalScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BondedCoWork-SessionRehearsal.ps1'
$bondedCoWorkSessionRehearsalOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $bondedCoWorkSessionRehearsalScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bonded co-work session rehearsal writer'
$bondedCoWorkSessionRehearsalBundlePath = Get-ScriptOutputTail -Output $bondedCoWorkSessionRehearsalOutput
if (-not [string]::IsNullOrWhiteSpace($bondedCoWorkSessionRehearsalBundlePath)) {
    $statePayload.lastBondedCoWorkSessionRehearsalBundle = $bondedCoWorkSessionRehearsalBundlePath
    $statePayload.bondedCoWorkSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedCoWorkSessionRehearsalStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$reachReturnDissolutionReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ReachReturn-DissolutionReceipt.ps1'
$reachReturnDissolutionReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $reachReturnDissolutionReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Reach return dissolution receipt writer'
$reachReturnDissolutionReceiptBundlePath = Get-ScriptOutputTail -Output $reachReturnDissolutionReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($reachReturnDissolutionReceiptBundlePath)) {
    $statePayload.lastReachReturnDissolutionReceiptBundle = $reachReturnDissolutionReceiptBundlePath
    $statePayload.reachReturnDissolutionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $reachReturnDissolutionReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$localityDistinctionWitnessLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-LocalityDistinction-WitnessLedger.ps1'
$localityDistinctionWitnessLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $localityDistinctionWitnessLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Locality distinction witness ledger writer'
$localityDistinctionWitnessLedgerBundlePath = Get-ScriptOutputTail -Output $localityDistinctionWitnessLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($localityDistinctionWitnessLedgerBundlePath)) {
    $statePayload.lastLocalityDistinctionWitnessLedgerBundle = $localityDistinctionWitnessLedgerBundlePath
    $statePayload.localityDistinctionWitnessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $localityDistinctionWitnessLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$localHostSanctuaryResidencyEnvelopeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-LocalHost-SanctuaryResidencyEnvelope.ps1'
$localHostSanctuaryResidencyEnvelopeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $localHostSanctuaryResidencyEnvelopeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Local host Sanctuary residency envelope writer'
$localHostSanctuaryResidencyEnvelopeBundlePath = Get-ScriptOutputTail -Output $localHostSanctuaryResidencyEnvelopeOutput
if (-not [string]::IsNullOrWhiteSpace($localHostSanctuaryResidencyEnvelopeBundlePath)) {
    $statePayload.lastLocalHostSanctuaryResidencyEnvelopeBundle = $localHostSanctuaryResidencyEnvelopeBundlePath
    $statePayload.localHostSanctuaryResidencyEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $localHostSanctuaryResidencyEnvelopeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$runtimeHabitationReadinessLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-RuntimeHabitation-ReadinessLedger.ps1'
$runtimeHabitationReadinessLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $runtimeHabitationReadinessLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Runtime habitation readiness ledger writer'
$runtimeHabitationReadinessLedgerBundlePath = Get-ScriptOutputTail -Output $runtimeHabitationReadinessLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($runtimeHabitationReadinessLedgerBundlePath)) {
    $statePayload.lastRuntimeHabitationReadinessLedgerBundle = $runtimeHabitationReadinessLedgerBundlePath
    $statePayload.runtimeHabitationReadinessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $runtimeHabitationReadinessLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$boundedInhabitationLaunchRehearsalScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BoundedInhabitation-LaunchRehearsal.ps1'
$boundedInhabitationLaunchRehearsalOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $boundedInhabitationLaunchRehearsalScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bounded inhabitation launch rehearsal writer'
$boundedInhabitationLaunchRehearsalBundlePath = Get-ScriptOutputTail -Output $boundedInhabitationLaunchRehearsalOutput
if (-not [string]::IsNullOrWhiteSpace($boundedInhabitationLaunchRehearsalBundlePath)) {
    $statePayload.lastBoundedInhabitationLaunchRehearsalBundle = $boundedInhabitationLaunchRehearsalBundlePath
    $statePayload.boundedInhabitationLaunchRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundedInhabitationLaunchRehearsalStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$postHabitationHorizonLatticeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PostHabitation-HorizonLattice.ps1'
$postHabitationHorizonLatticeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $postHabitationHorizonLatticeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Post-habitation horizon lattice writer'
$postHabitationHorizonLatticeBundlePath = Get-ScriptOutputTail -Output $postHabitationHorizonLatticeOutput
if (-not [string]::IsNullOrWhiteSpace($postHabitationHorizonLatticeBundlePath)) {
    $statePayload.lastPostHabitationHorizonLatticeBundle = $postHabitationHorizonLatticeBundlePath
    $statePayload.postHabitationHorizonLatticeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $postHabitationHorizonLatticeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$boundedHorizonResearchBriefScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BoundedHorizon-ResearchBrief.ps1'
$boundedHorizonResearchBriefOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $boundedHorizonResearchBriefScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bounded horizon research brief writer'
$boundedHorizonResearchBriefBundlePath = Get-ScriptOutputTail -Output $boundedHorizonResearchBriefOutput
if (-not [string]::IsNullOrWhiteSpace($boundedHorizonResearchBriefBundlePath)) {
    $statePayload.lastBoundedHorizonResearchBriefBundle = $boundedHorizonResearchBriefBundlePath
    $statePayload.boundedHorizonResearchBriefStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundedHorizonResearchBriefStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$nextEraBatchSelectorScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-NextEra-BatchSelector.ps1'
$nextEraBatchSelectorOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $nextEraBatchSelectorScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Next era batch selector writer'
$nextEraBatchSelectorBundlePath = Get-ScriptOutputTail -Output $nextEraBatchSelectorOutput
if (-not [string]::IsNullOrWhiteSpace($nextEraBatchSelectorBundlePath)) {
    $statePayload.lastNextEraBatchSelectorBundle = $nextEraBatchSelectorBundlePath
    $statePayload.nextEraBatchSelectorStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $nextEraBatchSelectorStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$inquirySessionDisciplineSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-InquirySession-DisciplineSurface.ps1'
$inquirySessionDisciplineSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $inquirySessionDisciplineSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Inquiry session discipline writer'
$inquirySessionDisciplineSurfaceBundlePath = Get-ScriptOutputTail -Output $inquirySessionDisciplineSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($inquirySessionDisciplineSurfaceBundlePath)) {
    $statePayload.lastInquirySessionDisciplineSurfaceBundle = $inquirySessionDisciplineSurfaceBundlePath
    $statePayload.inquirySessionDisciplineSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $inquirySessionDisciplineSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$boundaryConditionLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BoundaryCondition-Ledger.ps1'
$boundaryConditionLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $boundaryConditionLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Boundary condition ledger writer'
$boundaryConditionLedgerBundlePath = Get-ScriptOutputTail -Output $boundaryConditionLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($boundaryConditionLedgerBundlePath)) {
    $statePayload.lastBoundaryConditionLedgerBundle = $boundaryConditionLedgerBundlePath
    $statePayload.boundaryConditionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $boundaryConditionLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$coherenceGainWitnessReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CoherenceGain-WitnessReceipt.ps1'
$coherenceGainWitnessReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $coherenceGainWitnessReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Coherence gain witness writer'
$coherenceGainWitnessReceiptBundlePath = Get-ScriptOutputTail -Output $coherenceGainWitnessReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($coherenceGainWitnessReceiptBundlePath)) {
    $statePayload.lastCoherenceGainWitnessReceiptBundle = $coherenceGainWitnessReceiptBundlePath
    $statePayload.coherenceGainWitnessReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coherenceGainWitnessReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$operatorInquirySelectionEnvelopeScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-OperatorInquiry-SelectionEnvelope.ps1'
$operatorInquirySelectionEnvelopeOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $operatorInquirySelectionEnvelopeScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Operator inquiry selection writer'
$operatorInquirySelectionEnvelopeBundlePath = Get-ScriptOutputTail -Output $operatorInquirySelectionEnvelopeOutput
if (-not [string]::IsNullOrWhiteSpace($operatorInquirySelectionEnvelopeBundlePath)) {
    $statePayload.lastOperatorInquirySelectionEnvelopeBundle = $operatorInquirySelectionEnvelopeBundlePath
    $statePayload.operatorInquirySelectionEnvelopeStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $operatorInquirySelectionEnvelopeStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$bondedCrucibleSessionRehearsalScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BondedCrucible-SessionRehearsal.ps1'
$bondedCrucibleSessionRehearsalOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $bondedCrucibleSessionRehearsalScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Bonded crucible rehearsal writer'
$bondedCrucibleSessionRehearsalBundlePath = Get-ScriptOutputTail -Output $bondedCrucibleSessionRehearsalOutput
if (-not [string]::IsNullOrWhiteSpace($bondedCrucibleSessionRehearsalBundlePath)) {
    $statePayload.lastBondedCrucibleSessionRehearsalBundle = $bondedCrucibleSessionRehearsalBundlePath
    $statePayload.bondedCrucibleSessionRehearsalStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bondedCrucibleSessionRehearsalStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$sharedBoundaryMemoryLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SharedBoundaryMemory-Ledger.ps1'
$sharedBoundaryMemoryLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $sharedBoundaryMemoryLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Shared boundary memory writer'
$sharedBoundaryMemoryLedgerBundlePath = Get-ScriptOutputTail -Output $sharedBoundaryMemoryLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($sharedBoundaryMemoryLedgerBundlePath)) {
    $statePayload.lastSharedBoundaryMemoryLedgerBundle = $sharedBoundaryMemoryLedgerBundlePath
    $statePayload.sharedBoundaryMemoryLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $sharedBoundaryMemoryLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$continuityUnderPressureLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ContinuityUnderPressure-Ledger.ps1'
$continuityUnderPressureLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $continuityUnderPressureLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Continuity under pressure writer'
$continuityUnderPressureLedgerBundlePath = Get-ScriptOutputTail -Output $continuityUnderPressureLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($continuityUnderPressureLedgerBundlePath)) {
    $statePayload.lastContinuityUnderPressureLedgerBundle = $continuityUnderPressureLedgerBundlePath
    $statePayload.continuityUnderPressureLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $continuityUnderPressureLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$expressiveDeformationReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ExpressiveDeformation-Receipt.ps1'
$expressiveDeformationReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $expressiveDeformationReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Expressive deformation writer'
$expressiveDeformationReceiptBundlePath = Get-ScriptOutputTail -Output $expressiveDeformationReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($expressiveDeformationReceiptBundlePath)) {
    $statePayload.lastExpressiveDeformationReceiptBundle = $expressiveDeformationReceiptBundlePath
    $statePayload.expressiveDeformationReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $expressiveDeformationReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$mutualIntelligibilityWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-MutualIntelligibility-Witness.ps1'
$mutualIntelligibilityWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $mutualIntelligibilityWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Mutual intelligibility writer'
$mutualIntelligibilityWitnessBundlePath = Get-ScriptOutputTail -Output $mutualIntelligibilityWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($mutualIntelligibilityWitnessBundlePath)) {
    $statePayload.lastMutualIntelligibilityWitnessBundle = $mutualIntelligibilityWitnessBundlePath
    $statePayload.mutualIntelligibilityWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $mutualIntelligibilityWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$inquiryPatternContinuityLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-InquiryPatternContinuity-Ledger.ps1'
$inquiryPatternContinuityLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $inquiryPatternContinuityLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Inquiry-pattern continuity writer'
$inquiryPatternContinuityLedgerBundlePath = Get-ScriptOutputTail -Output $inquiryPatternContinuityLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($inquiryPatternContinuityLedgerBundlePath)) {
    $statePayload.lastInquiryPatternContinuityLedgerBundle = $inquiryPatternContinuityLedgerBundlePath
    $statePayload.inquiryPatternContinuityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $inquiryPatternContinuityLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$questioningBoundaryPairLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-QuestioningBoundaryPair-Ledger.ps1'
$questioningBoundaryPairLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $questioningBoundaryPairLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Questioning boundary-pair writer'
$questioningBoundaryPairLedgerBundlePath = Get-ScriptOutputTail -Output $questioningBoundaryPairLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($questioningBoundaryPairLedgerBundlePath)) {
    $statePayload.lastQuestioningBoundaryPairLedgerBundle = $questioningBoundaryPairLedgerBundlePath
    $statePayload.questioningBoundaryPairLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningBoundaryPairLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$carryForwardInquirySelectionSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CarryForwardInquiry-SelectionSurface.ps1'
$carryForwardInquirySelectionSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $carryForwardInquirySelectionSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Carry-forward inquiry-selection writer'
$carryForwardInquirySelectionSurfaceBundlePath = Get-ScriptOutputTail -Output $carryForwardInquirySelectionSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($carryForwardInquirySelectionSurfaceBundlePath)) {
    $statePayload.lastCarryForwardInquirySelectionSurfaceBundle = $carryForwardInquirySelectionSurfaceBundlePath
    $statePayload.carryForwardInquirySelectionSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $carryForwardInquirySelectionSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$engramDistanceClassificationLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-EngramDistance-ClassificationLedger.ps1'
$engramDistanceClassificationLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $engramDistanceClassificationLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Engram-distance classification writer'
$engramDistanceClassificationLedgerBundlePath = Get-ScriptOutputTail -Output $engramDistanceClassificationLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($engramDistanceClassificationLedgerBundlePath)) {
    $statePayload.lastEngramDistanceClassificationLedgerBundle = $engramDistanceClassificationLedgerBundlePath
    $statePayload.engramDistanceClassificationLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramDistanceClassificationLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$engramPromotionRequirementsMatrixScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-EngramPromotionRequirements-Matrix.ps1'
$engramPromotionRequirementsMatrixOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $engramPromotionRequirementsMatrixScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Engram-promotion requirements writer'
$engramPromotionRequirementsMatrixBundlePath = Get-ScriptOutputTail -Output $engramPromotionRequirementsMatrixOutput
if (-not [string]::IsNullOrWhiteSpace($engramPromotionRequirementsMatrixBundlePath)) {
    $statePayload.lastEngramPromotionRequirementsMatrixBundle = $engramPromotionRequirementsMatrixBundlePath
    $statePayload.engramPromotionRequirementsMatrixStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramPromotionRequirementsMatrixStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$distanceWeightedQuestioningAdmissionSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-DistanceWeighted-QuestioningAdmissionSurface.ps1'
$distanceWeightedQuestioningAdmissionSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $distanceWeightedQuestioningAdmissionSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Distance-weighted questioning-admission writer'
$distanceWeightedQuestioningAdmissionSurfaceBundlePath = Get-ScriptOutputTail -Output $distanceWeightedQuestioningAdmissionSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($distanceWeightedQuestioningAdmissionSurfaceBundlePath)) {
    $statePayload.lastDistanceWeightedQuestioningAdmissionSurfaceBundle = $distanceWeightedQuestioningAdmissionSurfaceBundlePath
    $statePayload.distanceWeightedQuestioningAdmissionSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $distanceWeightedQuestioningAdmissionSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$questioningOperatorCandidateLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-QuestioningOperator-CandidateLedger.ps1'
$questioningOperatorCandidateLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $questioningOperatorCandidateLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Questioning-operator candidate writer'
$questioningOperatorCandidateLedgerBundlePath = Get-ScriptOutputTail -Output $questioningOperatorCandidateLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($questioningOperatorCandidateLedgerBundlePath)) {
    $statePayload.lastQuestioningOperatorCandidateLedgerBundle = $questioningOperatorCandidateLedgerBundlePath
    $statePayload.questioningOperatorCandidateLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningOperatorCandidateLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$questioningGelPromotionGateScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-QuestioningGEL-PromotionGate.ps1'
$questioningGelPromotionGateOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $questioningGelPromotionGateScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Questioning GEL promotion-gate writer'
$questioningGelPromotionGateBundlePath = Get-ScriptOutputTail -Output $questioningGelPromotionGateOutput
if (-not [string]::IsNullOrWhiteSpace($questioningGelPromotionGateBundlePath)) {
    $statePayload.lastQuestioningGelPromotionGateBundle = $questioningGelPromotionGateBundlePath
    $statePayload.questioningGelPromotionGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningGelPromotionGateStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$protectedQuestioningPatternSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ProtectedQuestioningPatternSurface.ps1'
$protectedQuestioningPatternSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $protectedQuestioningPatternSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Protected questioning-pattern writer'
$protectedQuestioningPatternSurfaceBundlePath = Get-ScriptOutputTail -Output $protectedQuestioningPatternSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($protectedQuestioningPatternSurfaceBundlePath)) {
    $statePayload.lastProtectedQuestioningPatternSurfaceBundle = $protectedQuestioningPatternSurfaceBundlePath
    $statePayload.protectedQuestioningPatternSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $protectedQuestioningPatternSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$variationTestedReentryLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-VariationTested-ReentryLedger.ps1'
$variationTestedReentryLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $variationTestedReentryLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Variation-tested reentry writer'
$variationTestedReentryLedgerBundlePath = Get-ScriptOutputTail -Output $variationTestedReentryLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($variationTestedReentryLedgerBundlePath)) {
    $statePayload.lastVariationTestedReentryLedgerBundle = $variationTestedReentryLedgerBundlePath
    $statePayload.variationTestedReentryLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $variationTestedReentryLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$questioningAdmissionRefusalReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-QuestioningAdmission-RefusalReceipt.ps1'
$questioningAdmissionRefusalReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $questioningAdmissionRefusalReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Questioning admission-refusal writer'
$questioningAdmissionRefusalReceiptBundlePath = Get-ScriptOutputTail -Output $questioningAdmissionRefusalReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($questioningAdmissionRefusalReceiptBundlePath)) {
    $statePayload.lastQuestioningAdmissionRefusalReceiptBundle = $questioningAdmissionRefusalReceiptBundlePath
    $statePayload.questioningAdmissionRefusalReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $questioningAdmissionRefusalReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$promotionSeductionWatchScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-PromotionSeduction-Watch.ps1'
$promotionSeductionWatchOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $promotionSeductionWatchScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Promotion-seduction watch writer'
$promotionSeductionWatchBundlePath = Get-ScriptOutputTail -Output $promotionSeductionWatchOutput
if (-not [string]::IsNullOrWhiteSpace($promotionSeductionWatchBundlePath)) {
    $statePayload.lastPromotionSeductionWatchBundle = $promotionSeductionWatchBundlePath
    $statePayload.promotionSeductionWatchStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $promotionSeductionWatchStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$engramIntentFieldLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-EngramIntentField-Ledger.ps1'
$engramIntentFieldLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $engramIntentFieldLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Engram intent-field ledger writer'
$engramIntentFieldLedgerBundlePath = Get-ScriptOutputTail -Output $engramIntentFieldLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($engramIntentFieldLedgerBundlePath)) {
    $statePayload.lastEngramIntentFieldLedgerBundle = $engramIntentFieldLedgerBundlePath
    $statePayload.engramIntentFieldLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $engramIntentFieldLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$intentConstraintAlignmentReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-IntentConstraint-AlignmentReceipt.ps1'
$intentConstraintAlignmentReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $intentConstraintAlignmentReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Intent-constraint alignment writer'
$intentConstraintAlignmentReceiptBundlePath = Get-ScriptOutputTail -Output $intentConstraintAlignmentReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($intentConstraintAlignmentReceiptBundlePath)) {
    $statePayload.lastIntentConstraintAlignmentReceiptBundle = $intentConstraintAlignmentReceiptBundlePath
    $statePayload.intentConstraintAlignmentReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $intentConstraintAlignmentReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$warmReactivationDispositionReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-WarmReactivation-DispositionReceipt.ps1'
$warmReactivationDispositionReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $warmReactivationDispositionReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Warm reactivation disposition writer'
$warmReactivationDispositionReceiptBundlePath = Get-ScriptOutputTail -Output $warmReactivationDispositionReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($warmReactivationDispositionReceiptBundlePath)) {
    $statePayload.lastWarmReactivationDispositionReceiptBundle = $warmReactivationDispositionReceiptBundlePath
    $statePayload.warmReactivationDispositionReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $warmReactivationDispositionReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$formationPhaseVectorScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-FormationPhase-Vector.ps1'
$formationPhaseVectorOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $formationPhaseVectorScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Formation phase-vector writer'
$formationPhaseVectorBundlePath = Get-ScriptOutputTail -Output $formationPhaseVectorOutput
if (-not [string]::IsNullOrWhiteSpace($formationPhaseVectorBundlePath)) {
    $statePayload.lastFormationPhaseVectorBundle = $formationPhaseVectorBundlePath
    $statePayload.formationPhaseVectorStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $formationPhaseVectorStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$brittlenessWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Brittleness-Witness.ps1'
$brittlenessWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $brittlenessWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Brittleness witness writer'
$brittlenessWitnessBundlePath = Get-ScriptOutputTail -Output $brittlenessWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($brittlenessWitnessBundlePath)) {
    $statePayload.lastBrittlenessWitnessBundle = $brittlenessWitnessBundlePath
    $statePayload.brittlenessWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $brittlenessWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$durabilityWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Durability-Witness.ps1'
$durabilityWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $durabilityWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Durability witness writer'
$durabilityWitnessBundlePath = Get-ScriptOutputTail -Output $durabilityWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($durabilityWitnessBundlePath)) {
    $statePayload.lastDurabilityWitnessBundle = $durabilityWitnessBundlePath
    $statePayload.durabilityWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $durabilityWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$warmClockDispositionScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-WarmClock-Disposition.ps1'
$warmClockDispositionOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $warmClockDispositionScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Warm clock disposition writer'
$warmClockDispositionBundlePath = Get-ScriptOutputTail -Output $warmClockDispositionOutput
if (-not [string]::IsNullOrWhiteSpace($warmClockDispositionBundlePath)) {
    $statePayload.lastWarmClockDispositionBundle = $warmClockDispositionBundlePath
    $statePayload.warmClockDispositionStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $warmClockDispositionStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$ripeningStalenessLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Ripening-Staleness-Ledger.ps1'
$ripeningStalenessLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $ripeningStalenessLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Ripening staleness ledger writer'
$ripeningStalenessLedgerBundlePath = Get-ScriptOutputTail -Output $ripeningStalenessLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($ripeningStalenessLedgerBundlePath)) {
    $statePayload.lastRipeningStalenessLedgerBundle = $ripeningStalenessLedgerBundlePath
    $statePayload.ripeningStalenessLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $ripeningStalenessLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$coolingPressureWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CoolingPressure-Witness.ps1'
$coolingPressureWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $coolingPressureWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Cooling pressure witness writer'
$coolingPressureWitnessBundlePath = Get-ScriptOutputTail -Output $coolingPressureWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($coolingPressureWitnessBundlePath)) {
    $statePayload.lastCoolingPressureWitnessBundle = $coolingPressureWitnessBundlePath
    $statePayload.coolingPressureWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coolingPressureWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$hotReactivationTriggerReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-HotReactivation-TriggerReceipt.ps1'
$hotReactivationTriggerReceiptOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $hotReactivationTriggerReceiptScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Hot reactivation trigger receipt writer'
$hotReactivationTriggerReceiptBundlePath = Get-ScriptOutputTail -Output $hotReactivationTriggerReceiptOutput
if (-not [string]::IsNullOrWhiteSpace($hotReactivationTriggerReceiptBundlePath)) {
    $statePayload.lastHotReactivationTriggerReceiptBundle = $hotReactivationTriggerReceiptBundlePath
    $statePayload.hotReactivationTriggerReceiptStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $hotReactivationTriggerReceiptStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$coldAdmissionEligibilityGateScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-ColdAdmission-EligibilityGate.ps1'
$coldAdmissionEligibilityGateOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $coldAdmissionEligibilityGateScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Cold admission eligibility gate writer'
$coldAdmissionEligibilityGateBundlePath = Get-ScriptOutputTail -Output $coldAdmissionEligibilityGateOutput
if (-not [string]::IsNullOrWhiteSpace($coldAdmissionEligibilityGateBundlePath)) {
    $statePayload.lastColdAdmissionEligibilityGateBundle = $coldAdmissionEligibilityGateBundlePath
    $statePayload.coldAdmissionEligibilityGateStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coldAdmissionEligibilityGateStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$archiveDispositionLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Archive-DispositionLedger.ps1'
$archiveDispositionLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $archiveDispositionLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Archive disposition ledger writer'
$archiveDispositionLedgerBundlePath = Get-ScriptOutputTail -Output $archiveDispositionLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($archiveDispositionLedgerBundlePath)) {
    $statePayload.lastArchiveDispositionLedgerBundle = $archiveDispositionLedgerBundlePath
    $statePayload.archiveDispositionLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $archiveDispositionLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$interlockDensityLedgerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-InterlockDensity-Ledger.ps1'
$interlockDensityLedgerOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $interlockDensityLedgerScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Interlock density ledger writer'
$interlockDensityLedgerBundlePath = Get-ScriptOutputTail -Output $interlockDensityLedgerOutput
if (-not [string]::IsNullOrWhiteSpace($interlockDensityLedgerBundlePath)) {
    $statePayload.lastInterlockDensityLedgerBundle = $interlockDensityLedgerBundlePath
    $statePayload.interlockDensityLedgerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $interlockDensityLedgerStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$brittleDurableDifferentiationSurfaceScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-BrittleDurable-DifferentiationSurface.ps1'
$brittleDurableDifferentiationSurfaceOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $brittleDurableDifferentiationSurfaceScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Brittle durable differentiation surface writer'
$brittleDurableDifferentiationSurfaceBundlePath = Get-ScriptOutputTail -Output $brittleDurableDifferentiationSurfaceOutput
if (-not [string]::IsNullOrWhiteSpace($brittleDurableDifferentiationSurfaceBundlePath)) {
    $statePayload.lastBrittleDurableDifferentiationSurfaceBundle = $brittleDurableDifferentiationSurfaceBundlePath
    $statePayload.brittleDurableDifferentiationSurfaceStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $brittleDurableDifferentiationSurfaceStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$coreInvariantLatticeWitnessScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-CoreInvariant-LatticeWitness.ps1'
$coreInvariantLatticeWitnessOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $coreInvariantLatticeWitnessScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath) -FailureContext 'Core invariant lattice witness writer'
$coreInvariantLatticeWitnessBundlePath = Get-ScriptOutputTail -Output $coreInvariantLatticeWitnessOutput
if (-not [string]::IsNullOrWhiteSpace($coreInvariantLatticeWitnessBundlePath)) {
    $statePayload.lastCoreInvariantLatticeWitnessBundle = $coreInvariantLatticeWitnessBundlePath
    $statePayload.coreInvariantLatticeWitnessStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $coreInvariantLatticeWitnessStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
}

$summary = [ordered]@{
    schemaVersion = 1
    runId = $automationCycleRunId
    generatedAtUtc = $nowUtc.ToString('o')
    statePath = $statePath
    lastReleaseCandidateBundle = $releaseCandidateBundlePath
    lastKnownStatus = $latestStatus
    actionClass = $automationActionClass
    lastDigestBundle = $digestBundlePath
    dopingHeaderStatePath = $statePayload.dopingHeaderStatePath
    cycleReceiptStatePath = $statePayload.cycleReceiptStatePath
    readinessNoticeStatePath = $statePayload.readinessNoticeStatePath
    pauseNoticeStatePath = $statePayload.pauseNoticeStatePath
    lastNotificationBundle = $statePayload.lastNotificationBundle
    notificationStatePath = $statePayload.notificationStatePath
    lastSeededGovernanceBundle = $seededGovernanceBundlePath
    retentionStatePath = $statePayload.retentionStatePath
    schedulerReconciliationStatePath = $statePayload.schedulerReconciliationStatePath
    cmeConsolidationStatePath = $statePayload.cmeConsolidationStatePath
    lastCmeFormationAndOfficeLedgerBundle = $statePayload.lastCmeFormationAndOfficeLedgerBundle
    cmeFormationAndOfficeLedgerStatePath = $statePayload.cmeFormationAndOfficeLedgerStatePath
    lastPromotionGateBundle = $statePayload.lastPromotionGateBundle
    promotionGateStatePath = $statePayload.promotionGateStatePath
    lastCiConcordanceBundle = $statePayload.lastCiConcordanceBundle
    ciConcordanceStatePath = $statePayload.ciConcordanceStatePath
    lastReleaseRatificationBundle = $statePayload.lastReleaseRatificationBundle
    releaseRatificationStatePath = $statePayload.releaseRatificationStatePath
    lastFirstPublishIntentBundle = $statePayload.lastFirstPublishIntentBundle
    firstPublishIntentStatePath = $statePayload.firstPublishIntentStatePath
    lastSeededPromotionReviewBundle = $statePayload.lastSeededPromotionReviewBundle
    seededPromotionReviewStatePath = $statePayload.seededPromotionReviewStatePath
    lastReleaseHandshakeBundle = $statePayload.lastReleaseHandshakeBundle
    releaseHandshakeStatePath = $statePayload.releaseHandshakeStatePath
    lastPublishRequestEnvelopeBundle = $statePayload.lastPublishRequestEnvelopeBundle
    publishRequestEnvelopeStatePath = $statePayload.publishRequestEnvelopeStatePath
    lastPostPublishEvidenceBundle = $statePayload.lastPostPublishEvidenceBundle
    postPublishEvidenceStatePath = $statePayload.postPublishEvidenceStatePath
    lastSeedBraidEscalationBundle = $statePayload.lastSeedBraidEscalationBundle
    seedBraidEscalationStatePath = $statePayload.seedBraidEscalationStatePath
    lastPublishedRuntimeReceiptBundle = $statePayload.lastPublishedRuntimeReceiptBundle
    publishedRuntimeReceiptStatePath = $statePayload.publishedRuntimeReceiptStatePath
    lastArtifactAttestationBundle = $statePayload.lastArtifactAttestationBundle
    artifactAttestationStatePath = $statePayload.artifactAttestationStatePath
    lastPostPublishDriftWatchBundle = $statePayload.lastPostPublishDriftWatchBundle
    postPublishDriftWatchStatePath = $statePayload.postPublishDriftWatchStatePath
    lastOperationalPublicationLedgerBundle = $statePayload.lastOperationalPublicationLedgerBundle
    operationalPublicationLedgerStatePath = $statePayload.operationalPublicationLedgerStatePath
    lastExternalConsumerConcordanceBundle = $statePayload.lastExternalConsumerConcordanceBundle
    externalConsumerConcordanceStatePath = $statePayload.externalConsumerConcordanceStatePath
    lastPostPublishGovernanceLoopBundle = $statePayload.lastPostPublishGovernanceLoopBundle
    postPublishGovernanceLoopStatePath = $statePayload.postPublishGovernanceLoopStatePath
    lastPublicationCadenceLedgerBundle = $statePayload.lastPublicationCadenceLedgerBundle
    publicationCadenceLedgerStatePath = $statePayload.publicationCadenceLedgerStatePath
    lastDownstreamRuntimeObservationBundle = $statePayload.lastDownstreamRuntimeObservationBundle
    downstreamRuntimeObservationStatePath = $statePayload.downstreamRuntimeObservationStatePath
    lastMultiIntervalGovernanceBraidBundle = $statePayload.lastMultiIntervalGovernanceBraidBundle
    multiIntervalGovernanceBraidStatePath = $statePayload.multiIntervalGovernanceBraidStatePath
    lastSchedulerExecutionReceiptBundle = $statePayload.lastSchedulerExecutionReceiptBundle
    schedulerExecutionReceiptStatePath = $statePayload.schedulerExecutionReceiptStatePath
    lastUnattendedIntervalConcordanceBundle = $statePayload.lastUnattendedIntervalConcordanceBundle
    unattendedIntervalConcordanceStatePath = $statePayload.unattendedIntervalConcordanceStatePath
    lastStaleSurfaceContradictionWatchBundle = $statePayload.lastStaleSurfaceContradictionWatchBundle
    staleSurfaceContradictionWatchStatePath = $statePayload.staleSurfaceContradictionWatchStatePath
    lastUnattendedProofCollapseBundle = $statePayload.lastUnattendedProofCollapseBundle
    unattendedProofCollapseStatePath = $statePayload.unattendedProofCollapseStatePath
    lastDormantWindowLedgerBundle = $statePayload.lastDormantWindowLedgerBundle
    dormantWindowLedgerStatePath = $statePayload.dormantWindowLedgerStatePath
    lastSilentCadenceIntegrityBundle = $statePayload.lastSilentCadenceIntegrityBundle
    silentCadenceIntegrityStatePath = $statePayload.silentCadenceIntegrityStatePath
    lastLongFormPhaseWitnessBundle = $statePayload.lastLongFormPhaseWitnessBundle
    longFormPhaseWitnessStatePath = $statePayload.longFormPhaseWitnessStatePath
    lastLongFormWindowBoundaryBundle = $statePayload.lastLongFormWindowBoundaryBundle
    longFormWindowBoundaryStatePath = $statePayload.longFormWindowBoundaryStatePath
    lastAutonomousLongFormCollapseBundle = $statePayload.lastAutonomousLongFormCollapseBundle
    autonomousLongFormCollapseStatePath = $statePayload.autonomousLongFormCollapseStatePath
    lastSchedulerProofHarvestBundle = $statePayload.lastSchedulerProofHarvestBundle
    schedulerProofHarvestStatePath = $statePayload.schedulerProofHarvestStatePath
    lastIntervalOriginClarificationBundle = $statePayload.lastIntervalOriginClarificationBundle
    intervalOriginClarificationStatePath = $statePayload.intervalOriginClarificationStatePath
    lastQueuedTaskMapPromotionBundle = $statePayload.lastQueuedTaskMapPromotionBundle
    queuedTaskMapPromotionStatePath = $statePayload.queuedTaskMapPromotionStatePath
    masterThreadOrchestrationStatePath = $statePayload.masterThreadOrchestrationStatePath
    lastRuntimeDeployabilityEnvelopeBundle = $statePayload.lastRuntimeDeployabilityEnvelopeBundle
    runtimeDeployabilityEnvelopeStatePath = $statePayload.runtimeDeployabilityEnvelopeStatePath
    lastSanctuaryRuntimeReadinessBundle = $statePayload.lastSanctuaryRuntimeReadinessBundle
    sanctuaryRuntimeReadinessStatePath = $statePayload.sanctuaryRuntimeReadinessStatePath
    lastRuntimeWorkSurfaceAdmissibilityBundle = $statePayload.lastRuntimeWorkSurfaceAdmissibilityBundle
    runtimeWorkSurfaceAdmissibilityStatePath = $statePayload.runtimeWorkSurfaceAdmissibilityStatePath
    lastReachAccessTopologyLedgerBundle = $statePayload.lastReachAccessTopologyLedgerBundle
    reachAccessTopologyLedgerStatePath = $statePayload.reachAccessTopologyLedgerStatePath
    lastBondedOperatorLocalityReadinessBundle = $statePayload.lastBondedOperatorLocalityReadinessBundle
    bondedOperatorLocalityReadinessStatePath = $statePayload.bondedOperatorLocalityReadinessStatePath
    lastProtectedStateLegibilitySurfaceBundle = $statePayload.lastProtectedStateLegibilitySurfaceBundle
    protectedStateLegibilitySurfaceStatePath = $statePayload.protectedStateLegibilitySurfaceStatePath
    lastNexusSingularPortalFacadeBundle = $statePayload.lastNexusSingularPortalFacadeBundle
    nexusSingularPortalFacadeStatePath = $statePayload.nexusSingularPortalFacadeStatePath
    lastDuplexPredicateEnvelopeBundle = $statePayload.lastDuplexPredicateEnvelopeBundle
    duplexPredicateEnvelopeStatePath = $statePayload.duplexPredicateEnvelopeStatePath
    lastOperatorActualWorkSessionRehearsalBundle = $statePayload.lastOperatorActualWorkSessionRehearsalBundle
    operatorActualWorkSessionRehearsalStatePath = $statePayload.operatorActualWorkSessionRehearsalStatePath
    lastIdentityInvariantThreadRootBundle = $statePayload.lastIdentityInvariantThreadRootBundle
    identityInvariantThreadRootStatePath = $statePayload.identityInvariantThreadRootStatePath
    lastGovernedThreadBirthReceiptBundle = $statePayload.lastGovernedThreadBirthReceiptBundle
    governedThreadBirthReceiptStatePath = $statePayload.governedThreadBirthReceiptStatePath
    lastInterWorkerBraidHandoffPacketBundle = $statePayload.lastInterWorkerBraidHandoffPacketBundle
    interWorkerBraidHandoffPacketStatePath = $statePayload.interWorkerBraidHandoffPacketStatePath
    lastAgentiCoreActualUtilitySurfaceBundle = $statePayload.lastAgentiCoreActualUtilitySurfaceBundle
    agentiCoreActualUtilitySurfaceStatePath = $statePayload.agentiCoreActualUtilitySurfaceStatePath
    lastReachDuplexRealizationSeamBundle = $statePayload.lastReachDuplexRealizationSeamBundle
    reachDuplexRealizationSeamStatePath = $statePayload.reachDuplexRealizationSeamStatePath
    lastBondedParticipationLocalityLedgerBundle = $statePayload.lastBondedParticipationLocalityLedgerBundle
    bondedParticipationLocalityLedgerStatePath = $statePayload.bondedParticipationLocalityLedgerStatePath
    lastSanctuaryRuntimeWorkbenchSurfaceBundle = $statePayload.lastSanctuaryRuntimeWorkbenchSurfaceBundle
    sanctuaryRuntimeWorkbenchSurfaceStatePath = $statePayload.sanctuaryRuntimeWorkbenchSurfaceStatePath
    lastAmenableDayDreamTierAdmissibilityBundle = $statePayload.lastAmenableDayDreamTierAdmissibilityBundle
    amenableDayDreamTierAdmissibilityStatePath = $statePayload.amenableDayDreamTierAdmissibilityStatePath
    lastSelfRootedCrypticDepthGateBundle = $statePayload.lastSelfRootedCrypticDepthGateBundle
    selfRootedCrypticDepthGateStatePath = $statePayload.selfRootedCrypticDepthGateStatePath
    lastRuntimeWorkbenchSessionLedgerBundle = $statePayload.lastRuntimeWorkbenchSessionLedgerBundle
    runtimeWorkbenchSessionLedgerStatePath = $statePayload.runtimeWorkbenchSessionLedgerStatePath
    lastCompanionToolTelemetryBundle = $statePayload.lastCompanionToolTelemetryBundle
    companionToolTelemetryStatePath = $statePayload.companionToolTelemetryStatePath
    lastV111EnrichmentPathwayBundle = $statePayload.lastV111EnrichmentPathwayBundle
    v111EnrichmentPathwayStatePath = $statePayload.v111EnrichmentPathwayStatePath
    lastRunIsolatedBuildPathwayBundle = $statePayload.lastRunIsolatedBuildPathwayBundle
    runIsolatedBuildPathwayStatePath = $statePayload.runIsolatedBuildPathwayStatePath
    lastDayDreamCollapseReceiptBundle = $statePayload.lastDayDreamCollapseReceiptBundle
    dayDreamCollapseReceiptStatePath = $statePayload.dayDreamCollapseReceiptStatePath
    lastCrypticDepthReturnReceiptBundle = $statePayload.lastCrypticDepthReturnReceiptBundle
    crypticDepthReturnReceiptStatePath = $statePayload.crypticDepthReturnReceiptStatePath
    lastBondedCoWorkSessionRehearsalBundle = $statePayload.lastBondedCoWorkSessionRehearsalBundle
    bondedCoWorkSessionRehearsalStatePath = $statePayload.bondedCoWorkSessionRehearsalStatePath
    lastReachReturnDissolutionReceiptBundle = $statePayload.lastReachReturnDissolutionReceiptBundle
    reachReturnDissolutionReceiptStatePath = $statePayload.reachReturnDissolutionReceiptStatePath
    lastLocalityDistinctionWitnessLedgerBundle = $statePayload.lastLocalityDistinctionWitnessLedgerBundle
    localityDistinctionWitnessLedgerStatePath = $statePayload.localityDistinctionWitnessLedgerStatePath
    lastLocalHostSanctuaryResidencyEnvelopeBundle = $statePayload.lastLocalHostSanctuaryResidencyEnvelopeBundle
    localHostSanctuaryResidencyEnvelopeStatePath = $statePayload.localHostSanctuaryResidencyEnvelopeStatePath
    lastRuntimeHabitationReadinessLedgerBundle = $statePayload.lastRuntimeHabitationReadinessLedgerBundle
    runtimeHabitationReadinessLedgerStatePath = $statePayload.runtimeHabitationReadinessLedgerStatePath
    lastBoundedInhabitationLaunchRehearsalBundle = $statePayload.lastBoundedInhabitationLaunchRehearsalBundle
    boundedInhabitationLaunchRehearsalStatePath = $statePayload.boundedInhabitationLaunchRehearsalStatePath
    lastPostHabitationHorizonLatticeBundle = $statePayload.lastPostHabitationHorizonLatticeBundle
    postHabitationHorizonLatticeStatePath = $statePayload.postHabitationHorizonLatticeStatePath
    lastBoundedHorizonResearchBriefBundle = $statePayload.lastBoundedHorizonResearchBriefBundle
    boundedHorizonResearchBriefStatePath = $statePayload.boundedHorizonResearchBriefStatePath
    lastNextEraBatchSelectorBundle = $statePayload.lastNextEraBatchSelectorBundle
    nextEraBatchSelectorStatePath = $statePayload.nextEraBatchSelectorStatePath
    lastInquirySessionDisciplineSurfaceBundle = $statePayload.lastInquirySessionDisciplineSurfaceBundle
    inquirySessionDisciplineSurfaceStatePath = $statePayload.inquirySessionDisciplineSurfaceStatePath
    lastBoundaryConditionLedgerBundle = $statePayload.lastBoundaryConditionLedgerBundle
    boundaryConditionLedgerStatePath = $statePayload.boundaryConditionLedgerStatePath
    lastCoherenceGainWitnessReceiptBundle = $statePayload.lastCoherenceGainWitnessReceiptBundle
    coherenceGainWitnessReceiptStatePath = $statePayload.coherenceGainWitnessReceiptStatePath
    lastOperatorInquirySelectionEnvelopeBundle = $statePayload.lastOperatorInquirySelectionEnvelopeBundle
    operatorInquirySelectionEnvelopeStatePath = $statePayload.operatorInquirySelectionEnvelopeStatePath
    lastBondedCrucibleSessionRehearsalBundle = $statePayload.lastBondedCrucibleSessionRehearsalBundle
    bondedCrucibleSessionRehearsalStatePath = $statePayload.bondedCrucibleSessionRehearsalStatePath
    lastSharedBoundaryMemoryLedgerBundle = $statePayload.lastSharedBoundaryMemoryLedgerBundle
    sharedBoundaryMemoryLedgerStatePath = $statePayload.sharedBoundaryMemoryLedgerStatePath
    lastContinuityUnderPressureLedgerBundle = $statePayload.lastContinuityUnderPressureLedgerBundle
    continuityUnderPressureLedgerStatePath = $statePayload.continuityUnderPressureLedgerStatePath
    lastExpressiveDeformationReceiptBundle = $statePayload.lastExpressiveDeformationReceiptBundle
    expressiveDeformationReceiptStatePath = $statePayload.expressiveDeformationReceiptStatePath
    lastMutualIntelligibilityWitnessBundle = $statePayload.lastMutualIntelligibilityWitnessBundle
    mutualIntelligibilityWitnessStatePath = $statePayload.mutualIntelligibilityWitnessStatePath
    lastInquiryPatternContinuityLedgerBundle = $statePayload.lastInquiryPatternContinuityLedgerBundle
    inquiryPatternContinuityLedgerStatePath = $statePayload.inquiryPatternContinuityLedgerStatePath
    lastQuestioningBoundaryPairLedgerBundle = $statePayload.lastQuestioningBoundaryPairLedgerBundle
    questioningBoundaryPairLedgerStatePath = $statePayload.questioningBoundaryPairLedgerStatePath
    lastCarryForwardInquirySelectionSurfaceBundle = $statePayload.lastCarryForwardInquirySelectionSurfaceBundle
    carryForwardInquirySelectionSurfaceStatePath = $statePayload.carryForwardInquirySelectionSurfaceStatePath
    lastEngramDistanceClassificationLedgerBundle = $statePayload.lastEngramDistanceClassificationLedgerBundle
    engramDistanceClassificationLedgerStatePath = $statePayload.engramDistanceClassificationLedgerStatePath
    lastEngramPromotionRequirementsMatrixBundle = $statePayload.lastEngramPromotionRequirementsMatrixBundle
    engramPromotionRequirementsMatrixStatePath = $statePayload.engramPromotionRequirementsMatrixStatePath
    lastDistanceWeightedQuestioningAdmissionSurfaceBundle = $statePayload.lastDistanceWeightedQuestioningAdmissionSurfaceBundle
    distanceWeightedQuestioningAdmissionSurfaceStatePath = $statePayload.distanceWeightedQuestioningAdmissionSurfaceStatePath
    lastQuestioningOperatorCandidateLedgerBundle = $statePayload.lastQuestioningOperatorCandidateLedgerBundle
    questioningOperatorCandidateLedgerStatePath = $statePayload.questioningOperatorCandidateLedgerStatePath
    lastQuestioningGelPromotionGateBundle = $statePayload.lastQuestioningGelPromotionGateBundle
    questioningGelPromotionGateStatePath = $statePayload.questioningGelPromotionGateStatePath
    lastProtectedQuestioningPatternSurfaceBundle = $statePayload.lastProtectedQuestioningPatternSurfaceBundle
    protectedQuestioningPatternSurfaceStatePath = $statePayload.protectedQuestioningPatternSurfaceStatePath
    lastVariationTestedReentryLedgerBundle = $statePayload.lastVariationTestedReentryLedgerBundle
    variationTestedReentryLedgerStatePath = $statePayload.variationTestedReentryLedgerStatePath
    lastQuestioningAdmissionRefusalReceiptBundle = $statePayload.lastQuestioningAdmissionRefusalReceiptBundle
    questioningAdmissionRefusalReceiptStatePath = $statePayload.questioningAdmissionRefusalReceiptStatePath
    lastPromotionSeductionWatchBundle = $statePayload.lastPromotionSeductionWatchBundle
    promotionSeductionWatchStatePath = $statePayload.promotionSeductionWatchStatePath
    lastEngramIntentFieldLedgerBundle = $statePayload.lastEngramIntentFieldLedgerBundle
    engramIntentFieldLedgerStatePath = $statePayload.engramIntentFieldLedgerStatePath
    lastIntentConstraintAlignmentReceiptBundle = $statePayload.lastIntentConstraintAlignmentReceiptBundle
    intentConstraintAlignmentReceiptStatePath = $statePayload.intentConstraintAlignmentReceiptStatePath
    lastWarmReactivationDispositionReceiptBundle = $statePayload.lastWarmReactivationDispositionReceiptBundle
    warmReactivationDispositionReceiptStatePath = $statePayload.warmReactivationDispositionReceiptStatePath
    lastFormationPhaseVectorBundle = $statePayload.lastFormationPhaseVectorBundle
    formationPhaseVectorStatePath = $statePayload.formationPhaseVectorStatePath
    lastBrittlenessWitnessBundle = $statePayload.lastBrittlenessWitnessBundle
    brittlenessWitnessStatePath = $statePayload.brittlenessWitnessStatePath
    lastDurabilityWitnessBundle = $statePayload.lastDurabilityWitnessBundle
    durabilityWitnessStatePath = $statePayload.durabilityWitnessStatePath
    lastWarmClockDispositionBundle = $statePayload.lastWarmClockDispositionBundle
    warmClockDispositionStatePath = $statePayload.warmClockDispositionStatePath
    lastRipeningStalenessLedgerBundle = $statePayload.lastRipeningStalenessLedgerBundle
    ripeningStalenessLedgerStatePath = $statePayload.ripeningStalenessLedgerStatePath
    lastCoolingPressureWitnessBundle = $statePayload.lastCoolingPressureWitnessBundle
    coolingPressureWitnessStatePath = $statePayload.coolingPressureWitnessStatePath
    lastHotReactivationTriggerReceiptBundle = $statePayload.lastHotReactivationTriggerReceiptBundle
    hotReactivationTriggerReceiptStatePath = $statePayload.hotReactivationTriggerReceiptStatePath
    lastColdAdmissionEligibilityGateBundle = $statePayload.lastColdAdmissionEligibilityGateBundle
    coldAdmissionEligibilityGateStatePath = $statePayload.coldAdmissionEligibilityGateStatePath
    lastArchiveDispositionLedgerBundle = $statePayload.lastArchiveDispositionLedgerBundle
    archiveDispositionLedgerStatePath = $statePayload.archiveDispositionLedgerStatePath
    lastInterlockDensityLedgerBundle = $statePayload.lastInterlockDensityLedgerBundle
    interlockDensityLedgerStatePath = $statePayload.interlockDensityLedgerStatePath
    lastBrittleDurableDifferentiationSurfaceBundle = $statePayload.lastBrittleDurableDifferentiationSurfaceBundle
    brittleDurableDifferentiationSurfaceStatePath = $statePayload.brittleDurableDifferentiationSurfaceStatePath
    lastCoreInvariantLatticeWitnessBundle = $statePayload.lastCoreInvariantLatticeWitnessBundle
    coreInvariantLatticeWitnessStatePath = $statePayload.coreInvariantLatticeWitnessStatePath
    nextReleaseCandidateRunUtc = $statePayload.nextReleaseCandidateRunUtc
    nextMandatoryHitlReviewUtc = $statePayload.nextMandatoryHitlReviewUtc
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $summary | Out-Null

Write-JsonFile -Path $summaryPath -Value $summary

$workspaceBucketStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Workspace-BucketStatus.ps1'
$workspaceBucketStatusOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $workspaceBucketStatusScriptPath, '-RepoRoot', $resolvedRepoRoot) -FailureContext 'Workspace bucket status writer'
$workspaceBucketStatusPath = Get-ScriptOutputTail -Output $workspaceBucketStatusOutput

$masterThreadOrchestrationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-MasterThread-OrchestrationStatus.ps1'
$masterThreadOrchestrationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $masterThreadOrchestrationScriptPath, '-RepoRoot', $resolvedRepoRoot) -FailureContext 'Master-thread orchestration writer'
$masterThreadOrchestrationStatePathFromRun = Get-ScriptOutputTail -Output $masterThreadOrchestrationOutput
if (-not [string]::IsNullOrWhiteSpace($masterThreadOrchestrationStatePathFromRun) -and (Test-Path -LiteralPath $masterThreadOrchestrationStatePathFromRun -PathType Leaf)) {
    $statePayload.masterThreadOrchestrationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $masterThreadOrchestrationStatePathFromRun
    $summary.masterThreadOrchestrationStatePath = $statePayload.masterThreadOrchestrationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
    Write-JsonFile -Path $summaryPath -Value $summary
}

$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
$taskStatusOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $taskStatusScriptPath, '-RepoRoot', $resolvedRepoRoot) -FailureContext 'Task status writer'
$taskStatusPath = Get-ScriptOutputTail -Output $taskStatusOutput

$notificationScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-AutomationNotification.ps1'
$notificationOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $notificationScriptPath, '-RepoRoot', $resolvedRepoRoot, '-CyclePolicyPath', $resolvedPolicyPath, '-PreviousStatus', $previousStatus, '-CurrentStatus', $latestStatus) -FailureContext 'Automation notification writer'
$notificationStatePathFromRun = Get-ScriptOutputTail -Output $notificationOutput
if (-not [string]::IsNullOrWhiteSpace($notificationStatePathFromRun) -and (Test-Path -LiteralPath $notificationStatePathFromRun -PathType Leaf)) {
    $notificationStateFromRun = Read-JsonFileOrNull -Path $notificationStatePathFromRun
    $statePayload.notificationStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $notificationStatePathFromRun
    if ($null -ne $notificationStateFromRun) {
        $statePayload.lastNotificationBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $notificationStateFromRun -PropertyName 'lastNotificationBundle')
    }

    $summary.lastNotificationBundle = $statePayload.lastNotificationBundle
    $summary.notificationStatePath = $statePayload.notificationStatePath
    Write-JsonFile -Path $statePath -Value $statePayload
    Write-JsonFile -Path $summaryPath -Value $summary
}

$taskStatusOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $taskStatusScriptPath, '-RepoRoot', $resolvedRepoRoot) -FailureContext 'Task status writer'
$taskStatusPath = Get-ScriptOutputTail -Output $taskStatusOutput

$receiptStatus = if ($latestStatus -eq $blockedStatus) { 'blocked' } else { 'completed' }
$receiptStandingResult = switch ($normalizedLatestStatus) {
    'candidate-ready' { 'candidate_ready' }
    'hitl-required' { 'review_gated' }
    'blocked' { 'suspended' }
    default { 'clarification_required' }
}
$receiptSummary = switch ($normalizedLatestStatus) {
    'candidate-ready' { 'Automation cycle completed in candidate-ready posture and may continue within cadence.' }
    'hitl-required' { 'Automation cycle completed in review-gated posture; bounded mechanical continuation may proceed while promotion awaits HITL.' }
    'blocked' { 'Automation cycle ended in blocked posture and is paused pending HITL review.' }
    default { 'Automation cycle completed with an unclassified posture and requires clarification before wider promotion.' }
}
$carryForwardClass = switch ($normalizedLatestStatus) {
    'candidate-ready' { 'continue-within-cadence' }
    'hitl-required' { 'promotable-with-review' }
    'blocked' { 'blocked-awaiting-hitl' }
    default { 'clarification-required' }
}
$nextLawfulActions = switch ($normalizedLatestStatus) {
    'candidate-ready' { @('continue-automation-until-next-cadence', 'refresh-status-surfaces-on-schedule') }
    'hitl-required' { @('continue-bounded-mechanical-maintenance', 'await-hitl-review-before-promotion') }
    'blocked' { @('wait-for-hitl-blocked-review', 'preserve-bounded-state-until-remediation') }
    default { @('clarify-current-posture-before-next-promotion-step') }
}
$receiptVerification = [ordered]@{
    release_candidate_manifest = if (Test-Path -LiteralPath $manifestPath -PathType Leaf) { 'passed' } else { 'missing' }
    digest_surface = if (-not [string]::IsNullOrWhiteSpace($digestBundlePath)) { 'available' } else { 'not-required' }
    task_status_surface = if (-not [string]::IsNullOrWhiteSpace($taskStatusPath)) { 'written' } else { 'missing' }
    workspace_bucket_surface = if (-not [string]::IsNullOrWhiteSpace($workspaceBucketStatusPath)) { 'written' } else { 'missing' }
    master_thread_orchestration_surface = if (-not [string]::IsNullOrWhiteSpace($masterThreadOrchestrationStatePathFromRun)) { 'written' } else { 'missing' }
    notification_surface = if (-not [string]::IsNullOrWhiteSpace($notificationStatePathFromRun)) { 'written' } else { 'not-triggered' }
}
$receiptArtifactsTouched = @(
    $dopingHeaderStatePath
    $statePath
    $summaryPath
    $taskStatusPath
    $workspaceBucketStatusPath
    $masterThreadOrchestrationStatePathFromRun
    $notificationStatePathFromRun
    $releaseCandidateBundlePath
    $digestBundlePath
    $blockedEscalationBundlePath
) | ForEach-Object {
    if (-not [string]::IsNullOrWhiteSpace($_)) {
        Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $_
    }
}
$cycleReceiptPayload = New-AutomationReceiptPayload `
    -ReceiptId ('receipt-{0}' -f $automationCycleRunId) `
    -RunId $automationCycleRunId `
    -Lane 'build-governance-automation' `
    -Summary $receiptSummary `
    -Status $receiptStatus `
    -StandingResult $receiptStandingResult `
    -ArtifactsTouched $receiptArtifactsTouched `
    -Verification $receiptVerification `
    -CarryForwardClass $carryForwardClass `
    -NextLawfulActions $nextLawfulActions `
    -HitlRequired ($latestStatus -in @('hitl-required', $blockedStatus)) `
    -HitlReason $(if ($latestStatus -eq 'hitl-required') { 'promotion-or-commit-review-required' } elseif ($latestStatus -eq $blockedStatus) { 'blocked-posture-requires-hitl-review' } else { '' })
Add-AutomationCascadeOperatorPromptProperty -InputObject $cycleReceiptPayload | Out-Null
Write-JsonFile -Path $cycleReceiptStatePath -Value $cycleReceiptPayload

$statePayload.lastCycleReceiptId = [string] $cycleReceiptPayload.receipt_id
$statePayload.lastCycleReceiptStatus = [string] $cycleReceiptPayload.status
$summary.lastCycleReceiptId = $statePayload.lastCycleReceiptId
$summary.lastCycleReceiptStatus = $statePayload.lastCycleReceiptStatus

$activeNoticePath = $null
if ($latestStatus -eq $blockedStatus) {
    if (Test-Path -LiteralPath $readinessNoticeStatePath -PathType Leaf) {
        Remove-Item -LiteralPath $readinessNoticeStatePath -Force
    }

    $pauseDependsOn = @(
        $manifestPath
        $blockedEscalationBundlePath
        $masterThreadOrchestrationStatePathFromRun
    ) | ForEach-Object {
        if (-not [string]::IsNullOrWhiteSpace($_)) {
            Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $_
        }
    }
    $pauseNoticePayload = New-AutomationNoticePayload `
        -NoticeId ('notice-{0}-pause' -f $automationCycleRunId) `
        -Lane 'build-governance-automation' `
        -Type 'pause_notice' `
        -Status 'paused' `
        -Summary 'Automation cycle is paused because the current posture is blocked.' `
        -DependsOn $pauseDependsOn `
        -EnablesWhenCleared @('resume-bounded-cycle-execution', 'reissue-readiness-notice-after-remediation') `
        -NextLawfulAction 'wait-for-hitl-blocked-review' `
        -HitlRequired $true
    Add-AutomationCascadeOperatorPromptProperty -InputObject $pauseNoticePayload | Out-Null
    Write-JsonFile -Path $pauseNoticeStatePath -Value $pauseNoticePayload
    $activeNoticePath = $pauseNoticeStatePath
    $statePayload.currentNoticeType = [string] $pauseNoticePayload.type
    $statePayload.currentNoticeStatus = [string] $pauseNoticePayload.status
    $statePayload.lastNoticeId = [string] $pauseNoticePayload.notice_id
    $summary.currentNoticeType = $statePayload.currentNoticeType
    $summary.currentNoticeStatus = $statePayload.currentNoticeStatus
    $summary.lastNoticeId = $statePayload.lastNoticeId
} else {
    if (Test-Path -LiteralPath $pauseNoticeStatePath -PathType Leaf) {
        Remove-Item -LiteralPath $pauseNoticeStatePath -Force
    }

    $readinessStatus = if ($latestStatus -eq 'hitl-required') { 'ready-with-review-gate' } else { 'ready' }
    $readinessSummary = if ($latestStatus -eq 'hitl-required') {
        'Automation cycle completed in review-gated posture; bounded continuation may proceed while promotion remains under HITL review.'
    } else {
        'Automation cycle completed in candidate-ready posture and may continue within its scheduled cadence.'
    }
    $readinessNoticePayload = New-AutomationNoticePayload `
        -NoticeId ('notice-{0}-readiness' -f $automationCycleRunId) `
        -Lane 'build-governance-automation' `
        -Type 'readiness_notice' `
        -Status $readinessStatus `
        -Summary $readinessSummary `
        -DependsOn @() `
        -EnablesWhenCleared @('continue-bounded-cycle-execution', 'allow-downstream-status-ingestion') `
        -NextLawfulAction $(if ($latestStatus -eq 'hitl-required') { 'await-hitl-review-before-promotion' } else { 'continue-automation-until-next-cadence' }) `
        -HitlRequired ($latestStatus -eq 'hitl-required')
    Add-AutomationCascadeOperatorPromptProperty -InputObject $readinessNoticePayload | Out-Null
    Write-JsonFile -Path $readinessNoticeStatePath -Value $readinessNoticePayload
    $activeNoticePath = $readinessNoticeStatePath
    $statePayload.currentNoticeType = [string] $readinessNoticePayload.type
    $statePayload.currentNoticeStatus = [string] $readinessNoticePayload.status
    $statePayload.lastNoticeId = [string] $readinessNoticePayload.notice_id
    $summary.currentNoticeType = $statePayload.currentNoticeType
    $summary.currentNoticeStatus = $statePayload.currentNoticeStatus
    $summary.lastNoticeId = $statePayload.lastNoticeId
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-JsonFile -Path $summaryPath -Value $summary

$taskStatusOutput = Invoke-ChildPowershellScript -ArgumentList @('-ExecutionPolicy', 'Bypass', '-File', $taskStatusScriptPath, '-RepoRoot', $resolvedRepoRoot) -FailureContext 'Task status writer'
$taskStatusPath = Get-ScriptOutputTail -Output $taskStatusOutput

Set-AutomationCascadePromptOnArtifacts -RepoRoot $resolvedRepoRoot -Value @(
    $statePayload
    $summary
    $dopingHeaderStatePath
    $cycleReceiptStatePath
    $activeNoticePath
    $taskStatusPath
    $workspaceBucketStatusPath
    $masterThreadOrchestrationStatePathFromRun
    $notificationStatePathFromRun
)

Write-Host ('[local-automation-cycle] Status: {0}' -f $latestStatus)
Write-Host ('[local-automation-cycle] State: {0}' -f $statePath)
Write-Host ('[local-automation-cycle] DopingHeader: {0}' -f $dopingHeaderStatePath)
Write-Host ('[local-automation-cycle] CycleReceipt: {0}' -f $cycleReceiptStatePath)
if (-not [string]::IsNullOrWhiteSpace($activeNoticePath)) {
    Write-Host ('[local-automation-cycle] ControlNotice: {0}' -f $activeNoticePath)
}
if (-not [string]::IsNullOrWhiteSpace($retentionStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] Retention: {0}' -f $retentionStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($seededGovernanceBundlePath)) {
    Write-Host ('[local-automation-cycle] SeededGovernance: {0}' -f $seededGovernanceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($schedulerSyncStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] SchedulerSync: {0}' -f $schedulerSyncStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($cmeConsolidationStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] CmeConsolidation: {0}' -f $cmeConsolidationStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($promotionGateBundlePath)) {
    Write-Host ('[local-automation-cycle] PromotionGate: {0}' -f $promotionGateBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($ciConcordanceBundlePath)) {
    Write-Host ('[local-automation-cycle] CiConcordance: {0}' -f $ciConcordanceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($releaseRatificationBundlePath)) {
    Write-Host ('[local-automation-cycle] ReleaseRatification: {0}' -f $releaseRatificationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($firstPublishIntentBundlePath)) {
    Write-Host ('[local-automation-cycle] FirstPublishIntent: {0}' -f $firstPublishIntentBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($seededPromotionReviewBundlePath)) {
    Write-Host ('[local-automation-cycle] SeededPromotionReview: {0}' -f $seededPromotionReviewBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($releaseHandshakeBundlePath)) {
    Write-Host ('[local-automation-cycle] ReleaseHandshake: {0}' -f $releaseHandshakeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($publishRequestEnvelopeBundlePath)) {
    Write-Host ('[local-automation-cycle] PublishRequestEnvelope: {0}' -f $publishRequestEnvelopeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($postPublishEvidenceBundlePath)) {
    Write-Host ('[local-automation-cycle] PostPublishEvidence: {0}' -f $postPublishEvidenceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($seedBraidEscalationBundlePath)) {
    Write-Host ('[local-automation-cycle] SeedBraidEscalation: {0}' -f $seedBraidEscalationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($publishedRuntimeReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] PublishedRuntimeReceipt: {0}' -f $publishedRuntimeReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($artifactAttestationBundlePath)) {
    Write-Host ('[local-automation-cycle] ArtifactAttestation: {0}' -f $artifactAttestationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($postPublishDriftWatchBundlePath)) {
    Write-Host ('[local-automation-cycle] PostPublishDriftWatch: {0}' -f $postPublishDriftWatchBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($operationalPublicationLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] OperationalPublicationLedger: {0}' -f $operationalPublicationLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($externalConsumerConcordanceBundlePath)) {
    Write-Host ('[local-automation-cycle] ExternalConsumerConcordance: {0}' -f $externalConsumerConcordanceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($postPublishGovernanceLoopBundlePath)) {
    Write-Host ('[local-automation-cycle] PostPublishGovernanceLoop: {0}' -f $postPublishGovernanceLoopBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($publicationCadenceLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] PublicationCadenceLedger: {0}' -f $publicationCadenceLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($downstreamRuntimeObservationBundlePath)) {
    Write-Host ('[local-automation-cycle] DownstreamRuntimeObservation: {0}' -f $downstreamRuntimeObservationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($multiIntervalGovernanceBraidBundlePath)) {
    Write-Host ('[local-automation-cycle] MultiIntervalGovernanceBraid: {0}' -f $multiIntervalGovernanceBraidBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($schedulerExecutionReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] SchedulerExecutionReceipt: {0}' -f $schedulerExecutionReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($unattendedIntervalConcordanceBundlePath)) {
    Write-Host ('[local-automation-cycle] UnattendedIntervalConcordance: {0}' -f $unattendedIntervalConcordanceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($staleSurfaceContradictionWatchBundlePath)) {
    Write-Host ('[local-automation-cycle] StaleSurfaceContradictionWatch: {0}' -f $staleSurfaceContradictionWatchBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($unattendedProofCollapseBundlePath)) {
    Write-Host ('[local-automation-cycle] UnattendedProofCollapse: {0}' -f $unattendedProofCollapseBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($dormantWindowLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] DormantWindowLedger: {0}' -f $dormantWindowLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($silentCadenceIntegrityBundlePath)) {
    Write-Host ('[local-automation-cycle] SilentCadenceIntegrity: {0}' -f $silentCadenceIntegrityBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($longFormPhaseWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] LongFormPhaseWitness: {0}' -f $longFormPhaseWitnessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($longFormWindowBoundaryBundlePath)) {
    Write-Host ('[local-automation-cycle] LongFormWindowBoundary: {0}' -f $longFormWindowBoundaryBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($autonomousLongFormCollapseBundlePath)) {
    Write-Host ('[local-automation-cycle] AutonomousLongFormCollapse: {0}' -f $autonomousLongFormCollapseBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($schedulerProofHarvestBundlePath)) {
    Write-Host ('[local-automation-cycle] SchedulerProofHarvest: {0}' -f $schedulerProofHarvestBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($intervalOriginClarificationBundlePath)) {
    Write-Host ('[local-automation-cycle] IntervalOriginClarification: {0}' -f $intervalOriginClarificationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($queuedTaskMapPromotionBundlePath)) {
    Write-Host ('[local-automation-cycle] QueuedTaskMapPromotion: {0}' -f $queuedTaskMapPromotionBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($notificationStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] Notification: {0}' -f $notificationStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($blockedEscalationBundlePath)) {
    Write-Host ('[local-automation-cycle] BlockedEscalation: {0}' -f $blockedEscalationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($taskStatusPath)) {
    Write-Host ('[local-automation-cycle] TaskStatus: {0}' -f $taskStatusPath)
}
if (-not [string]::IsNullOrWhiteSpace($workspaceBucketStatusPath)) {
    Write-Host ('[local-automation-cycle] WorkspaceBuckets: {0}' -f $workspaceBucketStatusPath)
}
if (-not [string]::IsNullOrWhiteSpace($masterThreadOrchestrationStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] MasterThreadOrchestration: {0}' -f $masterThreadOrchestrationStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($runtimeDeployabilityEnvelopeBundlePath)) {
    Write-Host ('[local-automation-cycle] RuntimeDeployabilityEnvelope: {0}' -f $runtimeDeployabilityEnvelopeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($sanctuaryRuntimeReadinessBundlePath)) {
    Write-Host ('[local-automation-cycle] SanctuaryRuntimeReadiness: {0}' -f $sanctuaryRuntimeReadinessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($runtimeWorkSurfaceAdmissibilityBundlePath)) {
    Write-Host ('[local-automation-cycle] RuntimeWorkSurfaceAdmissibility: {0}' -f $runtimeWorkSurfaceAdmissibilityBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($reachAccessTopologyLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] ReachAccessTopologyLedger: {0}' -f $reachAccessTopologyLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($bondedOperatorLocalityReadinessBundlePath)) {
    Write-Host ('[local-automation-cycle] BondedOperatorLocalityReadiness: {0}' -f $bondedOperatorLocalityReadinessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($protectedStateLegibilitySurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] ProtectedStateLegibilitySurface: {0}' -f $protectedStateLegibilitySurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($nexusSingularPortalFacadeBundlePath)) {
    Write-Host ('[local-automation-cycle] NexusSingularPortalFacade: {0}' -f $nexusSingularPortalFacadeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($duplexPredicateEnvelopeBundlePath)) {
    Write-Host ('[local-automation-cycle] DuplexPredicateEnvelope: {0}' -f $duplexPredicateEnvelopeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($operatorActualWorkSessionRehearsalBundlePath)) {
    Write-Host ('[local-automation-cycle] OperatorActualWorkSessionRehearsal: {0}' -f $operatorActualWorkSessionRehearsalBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($identityInvariantThreadRootBundlePath)) {
    Write-Host ('[local-automation-cycle] IdentityInvariantThreadRoot: {0}' -f $identityInvariantThreadRootBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($governedThreadBirthReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] GovernedThreadBirthReceipt: {0}' -f $governedThreadBirthReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($interWorkerBraidHandoffPacketBundlePath)) {
    Write-Host ('[local-automation-cycle] InterWorkerBraidHandoffPacket: {0}' -f $interWorkerBraidHandoffPacketBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($agentiCoreActualUtilitySurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] AgentiCoreActualUtilitySurface: {0}' -f $agentiCoreActualUtilitySurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($reachDuplexRealizationSeamBundlePath)) {
    Write-Host ('[local-automation-cycle] ReachDuplexRealizationSeam: {0}' -f $reachDuplexRealizationSeamBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($bondedParticipationLocalityLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] BondedParticipationLocalityLedger: {0}' -f $bondedParticipationLocalityLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($sanctuaryRuntimeWorkbenchSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] SanctuaryRuntimeWorkbenchSurface: {0}' -f $sanctuaryRuntimeWorkbenchSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($amenableDayDreamTierAdmissibilityBundlePath)) {
    Write-Host ('[local-automation-cycle] AmenableDayDreamTierAdmissibility: {0}' -f $amenableDayDreamTierAdmissibilityBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($selfRootedCrypticDepthGateBundlePath)) {
    Write-Host ('[local-automation-cycle] SelfRootedCrypticDepthGate: {0}' -f $selfRootedCrypticDepthGateBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($runtimeWorkbenchSessionLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] RuntimeWorkbenchSessionLedger: {0}' -f $runtimeWorkbenchSessionLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($companionToolTelemetryBundlePath)) {
    Write-Host ('[local-automation-cycle] CompanionToolTelemetry: {0}' -f $companionToolTelemetryBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($v111EnrichmentPathwayBundlePath)) {
    Write-Host ('[local-automation-cycle] V111EnrichmentPathway: {0}' -f $v111EnrichmentPathwayBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($runIsolatedBuildPathwayBundlePath)) {
    Write-Host ('[local-automation-cycle] RunIsolatedBuildPathway: {0}' -f $runIsolatedBuildPathwayBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($dayDreamCollapseReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] DayDreamCollapseReceipt: {0}' -f $dayDreamCollapseReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($crypticDepthReturnReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] CrypticDepthReturnReceipt: {0}' -f $crypticDepthReturnReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($bondedCoWorkSessionRehearsalBundlePath)) {
    Write-Host ('[local-automation-cycle] BondedCoWorkSessionRehearsal: {0}' -f $bondedCoWorkSessionRehearsalBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($reachReturnDissolutionReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] ReachReturnDissolutionReceipt: {0}' -f $reachReturnDissolutionReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($localityDistinctionWitnessLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] LocalityDistinctionWitnessLedger: {0}' -f $localityDistinctionWitnessLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($localHostSanctuaryResidencyEnvelopeBundlePath)) {
    Write-Host ('[local-automation-cycle] LocalHostSanctuaryResidencyEnvelope: {0}' -f $localHostSanctuaryResidencyEnvelopeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($runtimeHabitationReadinessLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] RuntimeHabitationReadinessLedger: {0}' -f $runtimeHabitationReadinessLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($boundedInhabitationLaunchRehearsalBundlePath)) {
    Write-Host ('[local-automation-cycle] BoundedInhabitationLaunchRehearsal: {0}' -f $boundedInhabitationLaunchRehearsalBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($postHabitationHorizonLatticeBundlePath)) {
    Write-Host ('[local-automation-cycle] PostHabitationHorizonLattice: {0}' -f $postHabitationHorizonLatticeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($boundedHorizonResearchBriefBundlePath)) {
    Write-Host ('[local-automation-cycle] BoundedHorizonResearchBrief: {0}' -f $boundedHorizonResearchBriefBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($nextEraBatchSelectorBundlePath)) {
    Write-Host ('[local-automation-cycle] NextEraBatchSelector: {0}' -f $nextEraBatchSelectorBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($inquirySessionDisciplineSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] InquirySessionDisciplineSurface: {0}' -f $inquirySessionDisciplineSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($boundaryConditionLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] BoundaryConditionLedger: {0}' -f $boundaryConditionLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($coherenceGainWitnessReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] CoherenceGainWitnessReceipt: {0}' -f $coherenceGainWitnessReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($operatorInquirySelectionEnvelopeBundlePath)) {
    Write-Host ('[local-automation-cycle] OperatorInquirySelectionEnvelope: {0}' -f $operatorInquirySelectionEnvelopeBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($bondedCrucibleSessionRehearsalBundlePath)) {
    Write-Host ('[local-automation-cycle] BondedCrucibleSessionRehearsal: {0}' -f $bondedCrucibleSessionRehearsalBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($sharedBoundaryMemoryLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] SharedBoundaryMemoryLedger: {0}' -f $sharedBoundaryMemoryLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($continuityUnderPressureLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] ContinuityUnderPressureLedger: {0}' -f $continuityUnderPressureLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($expressiveDeformationReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] ExpressiveDeformationReceipt: {0}' -f $expressiveDeformationReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($mutualIntelligibilityWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] MutualIntelligibilityWitness: {0}' -f $mutualIntelligibilityWitnessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($inquiryPatternContinuityLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] InquiryPatternContinuityLedger: {0}' -f $inquiryPatternContinuityLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($questioningBoundaryPairLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] QuestioningBoundaryPairLedger: {0}' -f $questioningBoundaryPairLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($carryForwardInquirySelectionSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] CarryForwardInquirySelectionSurface: {0}' -f $carryForwardInquirySelectionSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($engramDistanceClassificationLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] EngramDistanceClassificationLedger: {0}' -f $engramDistanceClassificationLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($engramPromotionRequirementsMatrixBundlePath)) {
    Write-Host ('[local-automation-cycle] EngramPromotionRequirementsMatrix: {0}' -f $engramPromotionRequirementsMatrixBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($distanceWeightedQuestioningAdmissionSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] DistanceWeightedQuestioningAdmissionSurface: {0}' -f $distanceWeightedQuestioningAdmissionSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($questioningOperatorCandidateLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] QuestioningOperatorCandidateLedger: {0}' -f $questioningOperatorCandidateLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($questioningGelPromotionGateBundlePath)) {
    Write-Host ('[local-automation-cycle] QuestioningGelPromotionGate: {0}' -f $questioningGelPromotionGateBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($protectedQuestioningPatternSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] ProtectedQuestioningPatternSurface: {0}' -f $protectedQuestioningPatternSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($variationTestedReentryLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] VariationTestedReentryLedger: {0}' -f $variationTestedReentryLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($questioningAdmissionRefusalReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] QuestioningAdmissionRefusalReceipt: {0}' -f $questioningAdmissionRefusalReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($promotionSeductionWatchBundlePath)) {
    Write-Host ('[local-automation-cycle] PromotionSeductionWatch: {0}' -f $promotionSeductionWatchBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($engramIntentFieldLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] EngramIntentFieldLedger: {0}' -f $engramIntentFieldLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($intentConstraintAlignmentReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] IntentConstraintAlignmentReceipt: {0}' -f $intentConstraintAlignmentReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($warmReactivationDispositionReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] WarmReactivationDispositionReceipt: {0}' -f $warmReactivationDispositionReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($formationPhaseVectorBundlePath)) {
    Write-Host ('[local-automation-cycle] FormationPhaseVector: {0}' -f $formationPhaseVectorBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($brittlenessWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] BrittlenessWitness: {0}' -f $brittlenessWitnessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($durabilityWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] DurabilityWitness: {0}' -f $durabilityWitnessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($warmClockDispositionBundlePath)) {
    Write-Host ('[local-automation-cycle] WarmClockDisposition: {0}' -f $warmClockDispositionBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($ripeningStalenessLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] RipeningStalenessLedger: {0}' -f $ripeningStalenessLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($coolingPressureWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] CoolingPressureWitness: {0}' -f $coolingPressureWitnessBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($hotReactivationTriggerReceiptBundlePath)) {
    Write-Host ('[local-automation-cycle] HotReactivationTriggerReceipt: {0}' -f $hotReactivationTriggerReceiptBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($coldAdmissionEligibilityGateBundlePath)) {
    Write-Host ('[local-automation-cycle] ColdAdmissionEligibilityGate: {0}' -f $coldAdmissionEligibilityGateBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($archiveDispositionLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] ArchiveDispositionLedger: {0}' -f $archiveDispositionLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($interlockDensityLedgerBundlePath)) {
    Write-Host ('[local-automation-cycle] InterlockDensityLedger: {0}' -f $interlockDensityLedgerBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($brittleDurableDifferentiationSurfaceBundlePath)) {
    Write-Host ('[local-automation-cycle] BrittleDurableDifferentiationSurface: {0}' -f $brittleDurableDifferentiationSurfaceBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($coreInvariantLatticeWitnessBundlePath)) {
    Write-Host ('[local-automation-cycle] CoreInvariantLatticeWitness: {0}' -f $coreInvariantLatticeWitnessBundlePath)
}

if ($latestStatus -eq $blockedStatus) {
    throw 'Local automation cycle ended in blocked status.'
}

$summaryPath
