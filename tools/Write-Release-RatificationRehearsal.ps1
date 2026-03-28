param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json'
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
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$releaseRatificationOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationOutputRoot)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before release ratification rehearsal can run.'
}

$promotionGateState = Read-JsonFileOrNull -Path $promotionGateStatePath
$ciConcordanceState = Read-JsonFileOrNull -Path $ciConcordanceStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath

$checklist = @(
    [ordered]@{
        id = 'automation-posture'
        label = 'Automation posture remains candidate-ready'
        satisfied = ([string] $cycleState.lastKnownStatus -eq 'candidate-ready')
        evidence = [string] $cycleState.lastKnownStatus
    },
    [ordered]@{
        id = 'promotion-gate'
        label = 'Promotion gate bundle is available'
        satisfied = ($null -ne $promotionGateState)
        evidence = if ($null -ne $promotionGateState) { [string] $promotionGateState.recommendation } else { 'missing' }
    },
    [ordered]@{
        id = 'ci-concordance'
        label = 'CI/local artifact concordance is not blocked'
        satisfied = ($null -ne $ciConcordanceState -and [string] $ciConcordanceState.concordanceState -ne 'blocked')
        evidence = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.concordanceState } else { 'missing' }
    },
    [ordered]@{
        id = 'scheduler-aligned'
        label = 'Scheduler is aligned to cadence'
        satisfied = ($null -ne $schedulerReconciliationState -and [bool] $schedulerReconciliationState.aligned)
        evidence = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { 'missing' }
    },
    [ordered]@{
        id = 'seeded-governance-visible'
        label = 'Seeded governance disposition is visible'
        satisfied = ($null -ne $seededGovernanceState)
        evidence = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { 'missing' }
    },
    [ordered]@{
        id = 'cme-consolidation-visible'
        label = 'CME consolidation state is visible'
        satisfied = ($null -ne $cmeConsolidationState)
        evidence = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { 'missing' }
    }
)

$unsatisfied = @($checklist | Where-Object { -not [bool] $_.satisfied })
$rehearsalState = 'ready-for-rehearsal'
$reasonCode = 'ratification-rehearsal-ready'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $rehearsalState = 'blocked'
    $reasonCode = 'ratification-rehearsal-automation-blocked'
} elseif ($unsatisfied.Count -gt 0) {
    $rehearsalState = 'needs-stabilization'
    $reasonCode = 'ratification-rehearsal-needs-stabilization'
} elseif ($null -ne $seededGovernanceState -and [string] $seededGovernanceState.disposition -ne 'Accepted') {
    $rehearsalState = 'needs-seed-stability'
    $reasonCode = 'ratification-rehearsal-seed-not-braided'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = 'no-run'
if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    $commitSha = [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
}
$bundlePath = Join-Path $releaseRatificationOutputRoot ('{0}-{1}' -f $timestamp, $commitSha)
$bundleJsonPath = Join-Path $bundlePath 'release-ratification.json'
$bundleMarkdownPath = Join-Path $bundlePath 'release-ratification.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    rehearsalState = $rehearsalState
    reasonCode = $reasonCode
    nextHumanDecision = if ($rehearsalState -eq 'ready-for-rehearsal') { 'ratify-publication-when-requested' } else { 'stabilize-evidence-before-ratification' }
    promotionGateRecommendation = if ($null -ne $promotionGateState) { [string] $promotionGateState.recommendation } else { $null }
    ciConcordanceState = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.concordanceState } else { $null }
    seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
    cmeConsolidationState = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { $null }
    checklist = $checklist
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Release Ratification Rehearsal',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Rehearsal state: `{0}`' -f $payload.rehearsalState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next human decision: `{0}`' -f $payload.nextHumanDecision),
    ('- Promotion gate recommendation: `{0}`' -f $(if ($payload.promotionGateRecommendation) { $payload.promotionGateRecommendation } else { 'missing' })),
    ('- CI concordance state: `{0}`' -f $(if ($payload.ciConcordanceState) { $payload.ciConcordanceState } else { 'missing' })),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- CME consolidation state: `{0}`' -f $(if ($payload.cmeConsolidationState) { $payload.cmeConsolidationState } else { 'missing' })),
    '',
    '| Checklist Item | Satisfied | Evidence |',
    '| --- | --- | --- |'
)

foreach ($item in $checklist) {
    $markdownLines += ('| {0} | {1} | {2} |' -f $item.label, $item.satisfied, $item.evidence)
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    rehearsalState = $payload.rehearsalState
    reasonCode = $payload.reasonCode
    nextHumanDecision = $payload.nextHumanDecision
}

Write-JsonFile -Path $releaseRatificationStatePath -Value $statePayload
Write-Host ('[release-ratification] Bundle: {0}' -f $bundlePath)
$bundlePath
