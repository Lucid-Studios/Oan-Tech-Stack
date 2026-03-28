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
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchStatePath)
$dormantWindowLedgerOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerOutputRoot)
$dormantWindowLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before dormant window ledger can run.'
}

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$staleSurfaceContradictionWatchState = Read-JsonFileOrNull -Path $staleSurfaceContradictionWatchStatePath
$previousState = Read-JsonFileOrNull -Path $dormantWindowLedgerStatePath

$releaseCandidateRunUtc = [string] $cycleState.lastReleaseCandidateRunUtc
$previousObservedRunUtc = if ($null -ne $previousState) { [string] $previousState.observedReleaseCandidateRunUtc } else { $null }
$previousCount = if ($null -ne $previousState) { [int] $previousState.consecutiveDormantWindows } else { 0 }
$consecutiveDormantWindows = $previousCount

$ledgerState = 'candidate-only'
$reasonCode = 'dormant-window-ledger-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'dormant-window-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $schedulerExecutionReceiptState -or $null -eq $staleSurfaceContradictionWatchState) {
    $ledgerState = 'awaiting-evidence'
    $reasonCode = 'dormant-window-ledger-evidence-missing'
    $nextAction = 'complete-map-10-prerequisites'
} elseif ([string] $staleSurfaceContradictionWatchState.watchState -eq 'contradiction-detected') {
    $ledgerState = 'contradiction-detected'
    $reasonCode = 'dormant-window-ledger-contradiction-detected'
    $nextAction = 'investigate-surface-ordering'
} elseif ([string] $schedulerExecutionReceiptState.receiptState -eq 'receipt-captured') {
    $ledgerState = 'dormancy-complete'
    $reasonCode = 'dormant-window-ledger-scheduler-proof-observed'
    $nextAction = 'collapse-unattended-proof'
} elseif ([string] $staleSurfaceContradictionWatchState.watchState -eq 'dormant-consistent') {
    if (-not [string]::IsNullOrWhiteSpace($releaseCandidateRunUtc) -and $releaseCandidateRunUtc -ne $previousObservedRunUtc) {
        $consecutiveDormantWindows = $previousCount + 1
    }

    $ledgerState = 'dormant-consistent-window'
    $reasonCode = 'dormant-window-ledger-dormant-consistent'
    $nextAction = 'allow-scheduled-cycle-to-fire'
} else {
    $ledgerState = 'active-non-dormant'
    $reasonCode = 'dormant-window-ledger-non-dormant'
    $nextAction = 'continue-unattended-observation'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $dormantWindowLedgerOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'dormant-window-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'dormant-window-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ledgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    consecutiveDormantWindows = $consecutiveDormantWindows
    observedReleaseCandidateRunUtc = $releaseCandidateRunUtc
    schedulerReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
    contradictionWatchState = if ($null -ne $staleSurfaceContradictionWatchState) { [string] $staleSurfaceContradictionWatchState.watchState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Dormant Window Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ledgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Consecutive dormant windows: `{0}`' -f $payload.consecutiveDormantWindows),
    ('- Observed release-candidate run (UTC): `{0}`' -f $(if ($payload.observedReleaseCandidateRunUtc) { $payload.observedReleaseCandidateRunUtc } else { 'missing' })),
    ('- Scheduler receipt state: `{0}`' -f $(if ($payload.schedulerReceiptState) { $payload.schedulerReceiptState } else { 'missing' })),
    ('- Contradiction watch state: `{0}`' -f $(if ($payload.contradictionWatchState) { $payload.contradictionWatchState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    ledgerState = $payload.ledgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    consecutiveDormantWindows = $payload.consecutiveDormantWindows
    observedReleaseCandidateRunUtc = $payload.observedReleaseCandidateRunUtc
}

Write-JsonFile -Path $dormantWindowLedgerStatePath -Value $statePayload
Write-Host ('[dormant-window-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
