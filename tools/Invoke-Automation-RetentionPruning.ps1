param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.0/build/local-automation-tasking.json'
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

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
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

function Get-ProtectedDirectorySet {
    param(
        [string] $RepoRootPath,
        [object] $CycleState,
        [object] $BlockedEscalationState,
        [object] $TaskingPolicy,
        [string] $TaskingPolicyPathValue
    )

    $protected = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($candidate in @(
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReleaseCandidateBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDigestBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSeededGovernanceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastBlockedEscalationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastNotificationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPromotionGateBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastCmeFormationAndOfficeLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastCiConcordanceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReleaseRatificationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSeededPromotionReviewBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastFirstPublishIntentBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReleaseHandshakeBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPublishRequestEnvelopeBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPostPublishEvidenceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSeedBraidEscalationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPublishedRuntimeReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastArtifactAttestationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPostPublishDriftWatchBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastOperationalPublicationLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastExternalConsumerConcordanceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPostPublishGovernanceLoopBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastPublicationCadenceLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDownstreamRuntimeObservationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastMultiIntervalGovernanceBraidBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSchedulerExecutionReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastUnattendedIntervalConcordanceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastStaleSurfaceContradictionWatchBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastUnattendedProofCollapseBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDormantWindowLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSilentCadenceIntegrityBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastLongFormPhaseWitnessBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastLongFormWindowBoundaryBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastAutonomousLongFormCollapseBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSchedulerProofHarvestBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastIntervalOriginClarificationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastQueuedTaskMapPromotionBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastRuntimeDeployabilityEnvelopeBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSanctuaryRuntimeReadinessBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastRuntimeWorkSurfaceAdmissibilityBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReachAccessTopologyLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastBondedOperatorLocalityReadinessBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastProtectedStateLegibilitySurfaceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastNexusSingularPortalFacadeBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDuplexPredicateEnvelopeBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastOperatorActualWorkSessionRehearsalBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastIdentityInvariantThreadRootBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastGovernedThreadBirthReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastInterWorkerBraidHandoffPacketBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastAgentiCoreActualUtilitySurfaceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReachDuplexRealizationSeamBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastBondedParticipationLocalityLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSanctuaryRuntimeWorkbenchSurfaceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastAmenableDayDreamTierAdmissibilityBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSelfRootedCrypticDepthGateBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastRuntimeWorkbenchSessionLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDayDreamCollapseReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastCrypticDepthReturnReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastBondedCoWorkSessionRehearsalBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReachReturnDissolutionReceiptBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastLocalityDistinctionWitnessLedgerBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $BlockedEscalationState -PropertyName 'bundlePath')
    )) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            [void] $protected.Add((Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath $candidate))
        }
    }

    $activeRunStatePath = Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath ([string] $TaskingPolicy.activeLongFormRunStatePath)
    $activeRunState = Read-JsonFileOrNull -Path $activeRunStatePath
    if ($null -ne $activeRunState) {
        $runRoot = Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath ([string] $TaskingPolicy.longFormRunOutputRoot)
        $activeRunId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeRunState -PropertyName 'runId')
        if (-not [string]::IsNullOrWhiteSpace($activeRunId)) {
            [void] $protected.Add((Join-Path $runRoot $activeRunId))
        }
    }

    return $protected
}

function Invoke-PruneRoot {
    param(
        [string] $RootPath,
        [int] $KeepCount,
        [System.Collections.Generic.HashSet[string]] $ProtectedDirectories,
        [string] $RepoRootPath
    )

    $result = [ordered]@{
        rootPath = Get-RelativePathString -BasePath $RepoRootPath -TargetPath $RootPath
        keepCount = $KeepCount
        discoveredCount = 0
        deleted = @()
        preserved = @()
    }

    if (-not (Test-Path -LiteralPath $RootPath -PathType Container)) {
        return $result
    }

    $directories = @(Get-ChildItem -LiteralPath $RootPath -Directory | Sort-Object Name -Descending)
    $result.discoveredCount = $directories.Count

    $retained = 0
    foreach ($directory in $directories) {
        $fullName = [System.IO.Path]::GetFullPath($directory.FullName)
        $shouldProtect = $ProtectedDirectories.Contains($fullName)

        if ($shouldProtect -or $retained -lt $KeepCount) {
            $retained += 1
            $result.preserved += (Get-RelativePathString -BasePath $RepoRootPath -TargetPath $directory.FullName)
            continue
        }

        Remove-Item -LiteralPath $directory.FullName -Recurse -Force
        $result.deleted += (Get-RelativePathString -BasePath $RepoRootPath -TargetPath $directory.FullName)
    }

    return $result
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$retentionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.retentionStatePath)
$blockedEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$blockedEscalationState = Read-JsonFileOrNull -Path $blockedEscalationStatePath
$protectedDirectories = Get-ProtectedDirectorySet -RepoRootPath $resolvedRepoRoot -CycleState $cycleState -BlockedEscalationState $blockedEscalationState -TaskingPolicy $taskingPolicy -TaskingPolicyPathValue $resolvedTaskingPolicyPath

$retentionPolicy = $cyclePolicy.retentionPolicy
$roots = @(
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseCandidateOutputRoot)
        keep = [int] $retentionPolicy.keepReleaseCandidateBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.digestOutputRoot)
        keep = [int] $retentionPolicy.keepDigestBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.longFormRunOutputRoot)
        keep = [int] $retentionPolicy.keepLongFormTaskMapRuns
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceOutputRoot)
        keep = [int] $retentionPolicy.keepSeededGovernanceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepCmeFormationAndOfficeLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.notificationOutputRoot)
        keep = [int] $retentionPolicy.keepNotificationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateOutputRoot)
        keep = [int] $retentionPolicy.keepPromotionGateBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceOutputRoot)
        keep = [int] $retentionPolicy.keepCiConcordanceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationOutputRoot)
        keep = [int] $retentionPolicy.keepReleaseRatificationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewOutputRoot)
        keep = [int] $retentionPolicy.keepSeededPromotionReviewBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentOutputRoot)
        keep = [int] $retentionPolicy.keepFirstPublishIntentBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeOutputRoot)
        keep = [int] $retentionPolicy.keepReleaseHandshakeBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeOutputRoot)
        keep = [int] $retentionPolicy.keepPublishRequestEnvelopeBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishEvidenceOutputRoot)
        keep = [int] $retentionPolicy.keepPostPublishEvidenceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seedBraidEscalationOutputRoot)
        keep = [int] $retentionPolicy.keepSeedBraidEscalationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepPublishedRuntimeReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.artifactAttestationOutputRoot)
        keep = [int] $retentionPolicy.keepArtifactAttestationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishDriftWatchOutputRoot)
        keep = [int] $retentionPolicy.keepPostPublishDriftWatchBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepOperationalPublicationLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.externalConsumerConcordanceOutputRoot)
        keep = [int] $retentionPolicy.keepExternalConsumerConcordanceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopOutputRoot)
        keep = [int] $retentionPolicy.keepPostPublishGovernanceLoopBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepPublicationCadenceLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.downstreamRuntimeObservationOutputRoot)
        keep = [int] $retentionPolicy.keepDownstreamRuntimeObservationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.multiIntervalGovernanceBraidOutputRoot)
        keep = [int] $retentionPolicy.keepMultiIntervalGovernanceBraidBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepSchedulerExecutionReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceOutputRoot)
        keep = [int] $retentionPolicy.keepUnattendedIntervalConcordanceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchOutputRoot)
        keep = [int] $retentionPolicy.keepStaleSurfaceContradictionWatchBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseOutputRoot)
        keep = [int] $retentionPolicy.keepUnattendedProofCollapseBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepDormantWindowLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.silentCadenceIntegrityOutputRoot)
        keep = [int] $retentionPolicy.keepSilentCadenceIntegrityBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessOutputRoot)
        keep = [int] $retentionPolicy.keepLongFormPhaseWitnessBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormWindowBoundaryOutputRoot)
        keep = [int] $retentionPolicy.keepLongFormWindowBoundaryBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.autonomousLongFormCollapseOutputRoot)
        keep = [int] $retentionPolicy.keepAutonomousLongFormCollapseBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerProofHarvestOutputRoot)
        keep = [int] $retentionPolicy.keepSchedulerProofHarvestBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intervalOriginClarificationOutputRoot)
        keep = [int] $retentionPolicy.keepIntervalOriginClarificationBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.queuedTaskMapPromotionOutputRoot)
        keep = [int] $retentionPolicy.keepQueuedTaskMapPromotionBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeOutputRoot)
        keep = [int] $retentionPolicy.keepRuntimeDeployabilityEnvelopeBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessOutputRoot)
        keep = [int] $retentionPolicy.keepSanctuaryRuntimeReadinessBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityOutputRoot)
        keep = [int] $retentionPolicy.keepRuntimeWorkSurfaceAdmissibilityBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepReachAccessTopologyLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessOutputRoot)
        keep = [int] $retentionPolicy.keepBondedOperatorLocalityReadinessBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceOutputRoot)
        keep = [int] $retentionPolicy.keepProtectedStateLegibilitySurfaceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeOutputRoot)
        keep = [int] $retentionPolicy.keepNexusSingularPortalFacadeBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeOutputRoot)
        keep = [int] $retentionPolicy.keepDuplexPredicateEnvelopeBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorActualWorkSessionRehearsalOutputRoot)
        keep = [int] $retentionPolicy.keepOperatorActualWorkSessionRehearsalBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootOutputRoot)
        keep = [int] $retentionPolicy.keepIdentityInvariantThreadRootBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepGovernedThreadBirthReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interWorkerBraidHandoffPacketOutputRoot)
        keep = [int] $retentionPolicy.keepInterWorkerBraidHandoffPacketBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceOutputRoot)
        keep = [int] $retentionPolicy.keepAgentiCoreActualUtilitySurfaceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamOutputRoot)
        keep = [int] $retentionPolicy.keepReachDuplexRealizationSeamBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepBondedParticipationLocalityLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceOutputRoot)
        keep = [int] $retentionPolicy.keepSanctuaryRuntimeWorkbenchSurfaceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityOutputRoot)
        keep = [int] $retentionPolicy.keepAmenableDayDreamTierAdmissibilityBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateOutputRoot)
        keep = [int] $retentionPolicy.keepSelfRootedCrypticDepthGateBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepRuntimeWorkbenchSessionLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepDayDreamCollapseReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepCrypticDepthReturnReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalOutputRoot)
        keep = [int] $retentionPolicy.keepBondedCoWorkSessionRehearsalBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptOutputRoot)
        keep = [int] $retentionPolicy.keepReachReturnDissolutionReceiptBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerOutputRoot)
        keep = [int] $retentionPolicy.keepLocalityDistinctionWitnessLedgerBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationOutputRoot)
        keep = [int] $retentionPolicy.keepBlockedEscalationBundles
    }
)

$pruneResults = @(
    $roots |
    ForEach-Object {
        Invoke-PruneRoot -RootPath ([string] $_.path) -KeepCount ([int] $_.keep) -ProtectedDirectories $protectedDirectories -RepoRootPath $resolvedRepoRoot
    }
)

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    protectedDirectories = @($protectedDirectories | ForEach-Object { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $_ } | Sort-Object)
    pruneResults = $pruneResults
}

Write-JsonFile -Path $retentionStatePath -Value $statePayload
Write-Host ('[automation-retention-pruning] State: {0}' -f $retentionStatePath)
$retentionStatePath
