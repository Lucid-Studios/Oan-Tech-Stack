param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $PolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
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

    $output = & powershell @ArgumentList
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
$releaseCandidateRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $releaseCandidateOutputRoot
$digestRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $digestOutputRoot
$releaseCadenceHours = [int] $policy.localReleaseCandidateCadenceHours
$digestCadenceHours = [int] $policy.mandatoryHitlDigestCadenceHours
$digestWindowHours = [int] $policy.digestWindowHours
$blockedStatus = [string] $policy.blockedStatus

$state = Read-JsonFileOrNull -Path $statePath
$previousStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastKnownStatus')
$nowUtc = (Get-Date).ToUniversalTime()
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReleaseCandidateRunUtc')
$lastDigestUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDigestUtc')

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

$statePayload = [ordered]@{
    schemaVersion = 1
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
}
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

$summary = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    statePath = $statePath
    lastReleaseCandidateBundle = $releaseCandidateBundlePath
    lastKnownStatus = $latestStatus
    lastDigestBundle = $digestBundlePath
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
    nextReleaseCandidateRunUtc = $statePayload.nextReleaseCandidateRunUtc
    nextMandatoryHitlReviewUtc = $statePayload.nextMandatoryHitlReviewUtc
}

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

Write-Host ('[local-automation-cycle] Status: {0}' -f $latestStatus)
Write-Host ('[local-automation-cycle] State: {0}' -f $statePath)
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

if ($latestStatus -eq $blockedStatus) {
    throw 'Local automation cycle ended in blocked status.'
}

$summaryPath
