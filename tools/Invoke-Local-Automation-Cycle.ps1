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
    nextReleaseCandidateRunUtc = $statePayload.nextReleaseCandidateRunUtc
    nextMandatoryHitlReviewUtc = $statePayload.nextMandatoryHitlReviewUtc
}

Write-JsonFile -Path $summaryPath -Value $summary

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
if (-not [string]::IsNullOrWhiteSpace($notificationStatePathFromRun)) {
    Write-Host ('[local-automation-cycle] Notification: {0}' -f $notificationStatePathFromRun)
}
if (-not [string]::IsNullOrWhiteSpace($blockedEscalationBundlePath)) {
    Write-Host ('[local-automation-cycle] BlockedEscalation: {0}' -f $blockedEscalationBundlePath)
}
if (-not [string]::IsNullOrWhiteSpace($taskStatusPath)) {
    Write-Host ('[local-automation-cycle] TaskStatus: {0}' -f $taskStatusPath)
}

if ($latestStatus -eq $blockedStatus) {
    throw 'Local automation cycle ended in blocked status.'
}

$summaryPath
