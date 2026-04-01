param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json'
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

    if (-not (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue)) {
        $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
            . $oanWorkspaceResolverPath
        }
    }

    if (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue) {
        return Resolve-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    }

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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$promotionGateOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateOutputRoot)
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$deployablesPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'policy.deployables' -CyclePolicy $cyclePolicy

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before promotion gate evaluation can run.'
}

$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath
$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json

$digestBundlePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cycleState.lastDigestBundle)
$digestJsonPath = Join-Path $digestBundlePath 'release-candidate-digest.json'
$digest = Read-JsonFileOrNull -Path $digestJsonPath
if ($null -eq $digest) {
    throw 'Promotion gate evaluation requires the latest digest bundle.'
}

$latestRun = $digest.latestRun
$publicationDeployables = @($deployables.deployables | Where-Object { [bool] $_.includedInFirstPublish })
$publicationRequiresHitl = @($publicationDeployables | Where-Object { [bool] $_.requiresHitlForPublication })

$recommendation = 'continue-candidate-only'
$reasonCode = 'promotion-gate-candidate-only'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $recommendation = 'block'
    $reasonCode = 'promotion-gate-automation-blocked'
} elseif ([bool] $digest.requiresImmediateHitl -or [string] $cycleState.lastKnownStatus -eq 'hitl-required') {
    $recommendation = 'review-required'
    $reasonCode = 'promotion-gate-digest-hitl-required'
} elseif ($null -ne $latestRun -and [bool] $latestRun.versionDecision.requiresHitl) {
    $recommendation = 'review-required'
    $reasonCode = 'promotion-gate-version-hitl'
} elseif ($null -ne $latestRun -and @($latestRun.versionDecision.reasonCodes).Count -gt 0 -and @($latestRun.versionDecision.reasonCodes | Where-Object { $_ -ne 'version-no-promotable-change' }).Count -gt 0) {
    $recommendation = 'review-required'
    $reasonCode = 'promotion-gate-version-reasons'
} elseif ($publicationRequiresHitl.Count -gt 0) {
    $recommendation = 'ratification-required'
    $reasonCode = 'promotion-gate-publication-hitl'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = if ($null -ne $latestRun) { [string] $latestRun.repo.commitSha } else { 'no-run' }
$shortSha = if ($commitSha.Length -gt 8) { $commitSha.Substring(0, 8) } else { $commitSha }
$bundlePath = Join-Path $promotionGateOutputRoot ('{0}-{1}' -f $timestamp, $shortSha)
$bundleJsonPath = Join-Path $bundlePath 'promotion-gate.json'
$bundleMarkdownPath = Join-Path $bundlePath 'promotion-gate.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    cycleStatus = [string] $cycleState.lastKnownStatus
    digestRecommendedAction = [string] $digest.recommendedAction
    requiresImmediateHitl = [bool] $digest.requiresImmediateHitl
    recommendation = $recommendation
    reasonCode = $reasonCode
    latestRunId = if ($null -ne $latestRun) { [string] $latestRun.runId } else { $null }
    latestRunStatus = if ($null -ne $latestRun) { [string] $latestRun.status } else { $null }
    proposedVersion = if ($null -ne $latestRun) { [string] $latestRun.versionDecision.proposedVersion } else { $null }
    versionRequiresHitl = if ($null -ne $latestRun) { [bool] $latestRun.versionDecision.requiresHitl } else { $null }
    versionReasonCodes = if ($null -ne $latestRun) { @($latestRun.versionDecision.reasonCodes | ForEach-Object { [string] $_ }) } else { @() }
    publicationDeployables = @(
        $publicationDeployables | ForEach-Object {
            [ordered]@{
                name = [string] $_.name
                publishLane = [string] $_.publishLane
                requiresHitlForPublication = [bool] $_.requiresHitlForPublication
            }
        }
    )
    seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
    schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $null }
    cmeConsolidationState = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { $null }
    digestBundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $digestBundlePath
    taskStatusPath = '.audit/state/local-automation-tasking-status.md'
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Promotion Gate Bundle',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Cycle status: `{0}`' -f $payload.cycleStatus),
    ('- Digest recommended action: `{0}`' -f $payload.digestRecommendedAction),
    ('- Requires immediate HITL: `{0}`' -f $payload.requiresImmediateHitl),
    ('- Recommendation: `{0}`' -f $payload.recommendation),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Proposed version: `{0}`' -f $(if ($payload.proposedVersion) { $payload.proposedVersion } else { 'none' })),
    ('- Seeded governance: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'none' })),
    ('- Scheduler aligned: `{0}`' -f $(if ($null -ne $payload.schedulerAligned) { $payload.schedulerAligned } else { 'unknown' })),
    ('- CME consolidation: `{0}`' -f $(if ($payload.cmeConsolidationState) { $payload.cmeConsolidationState } else { 'unknown' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    recommendation = $payload.recommendation
    reasonCode = $payload.reasonCode
    proposedVersion = $payload.proposedVersion
}

Write-JsonFile -Path $promotionGateStatePath -Value $statePayload
Write-Host ('[promotion-gate] Bundle: {0}' -f $bundlePath)
$bundlePath
