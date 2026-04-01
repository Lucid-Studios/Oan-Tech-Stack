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
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchStatePath)
$unattendedProofCollapseOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseOutputRoot)
$unattendedProofCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before unattended proof collapse can run.'
}

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$unattendedIntervalConcordanceState = Read-JsonFileOrNull -Path $unattendedIntervalConcordanceStatePath
$staleSurfaceContradictionWatchState = Read-JsonFileOrNull -Path $staleSurfaceContradictionWatchStatePath

$collapseState = 'candidate-only'
$reasonCode = 'unattended-proof-collapse-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $collapseState = 'blocked'
    $reasonCode = 'unattended-proof-collapse-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $schedulerExecutionReceiptState -or $null -eq $unattendedIntervalConcordanceState -or $null -eq $staleSurfaceContradictionWatchState) {
    $collapseState = 'awaiting-evidence'
    $reasonCode = 'unattended-proof-collapse-evidence-missing'
    $nextAction = 'complete-map-10-prerequisites'
} elseif ([string] $staleSurfaceContradictionWatchState.watchState -eq 'contradiction-detected') {
    $collapseState = 'contradiction-hold'
    $reasonCode = 'unattended-proof-collapse-contradiction-detected'
    $nextAction = 'investigate-surface-ordering'
} elseif ([string] $schedulerExecutionReceiptState.receiptState -ne 'receipt-captured') {
    $collapseState = 'awaiting-scheduler-run'
    $reasonCode = 'unattended-proof-collapse-scheduler-run-pending'
    $nextAction = [string] $schedulerExecutionReceiptState.nextAction
} else {
    switch ([string] $unattendedIntervalConcordanceState.concordanceState) {
        'concordant-unattended-interval' {
            $collapseState = 'collapsed-unattended-proof'
            $reasonCode = 'unattended-proof-collapse-concordant'
            $nextAction = 'continue-unattended-automation'
        }
        'manual-overhang-after-scheduler-proof' {
            $collapseState = 'collapsed-with-manual-overhang'
            $reasonCode = 'unattended-proof-collapse-manual-overhang'
            $nextAction = 'allow-next-scheduled-cycle-to-refresh-proof'
        }
        default {
            $collapseState = 'awaiting-cycle-match'
            $reasonCode = 'unattended-proof-collapse-cycle-match-pending'
            $nextAction = [string] $unattendedIntervalConcordanceState.nextAction
        }
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $unattendedProofCollapseOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'unattended-proof-collapse.json'
$bundleMarkdownPath = Join-Path $bundlePath 'unattended-proof-collapse.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    collapseState = $collapseState
    reasonCode = $reasonCode
    nextAction = $nextAction
    schedulerReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
    unattendedIntervalConcordanceState = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.concordanceState } else { $null }
    contradictionWatchState = if ($null -ne $staleSurfaceContradictionWatchState) { [string] $staleSurfaceContradictionWatchState.watchState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Unattended Proof Collapse',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Collapse state: `{0}`' -f $payload.collapseState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Scheduler receipt state: `{0}`' -f $(if ($payload.schedulerReceiptState) { $payload.schedulerReceiptState } else { 'missing' })),
    ('- Unattended interval concordance state: `{0}`' -f $(if ($payload.unattendedIntervalConcordanceState) { $payload.unattendedIntervalConcordanceState } else { 'missing' })),
    ('- Contradiction watch state: `{0}`' -f $(if ($payload.contradictionWatchState) { $payload.contradictionWatchState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    collapseState = $payload.collapseState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $unattendedProofCollapseStatePath -Value $statePayload
Write-Host ('[unattended-proof-collapse] Bundle: {0}' -f $bundlePath)
$bundlePath
