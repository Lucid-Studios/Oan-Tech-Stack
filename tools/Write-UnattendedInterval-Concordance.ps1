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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceOutputRoot)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before unattended interval concordance can run.'
}

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$toleranceMinutes = [int] $cyclePolicy.schedulerReconciliationPolicy.driftToleranceMinutes
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value $cycleState.lastReleaseCandidateRunUtc
$lastScheduledRunUtc = if ($null -ne $schedulerExecutionReceiptState) { Get-OptionalDateTimeUtc -Value $schedulerExecutionReceiptState.lastScheduledRunUtc } else { $null }

$concordanceState = 'candidate-only'
$reasonCode = 'unattended-interval-concordance-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $concordanceState = 'blocked'
    $reasonCode = 'unattended-interval-concordance-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $schedulerExecutionReceiptState) {
    $concordanceState = 'awaiting-evidence'
    $reasonCode = 'unattended-interval-concordance-receipt-missing'
    $nextAction = 'capture-scheduler-execution-receipt'
} elseif ([string] $schedulerExecutionReceiptState.receiptState -ne 'receipt-captured') {
    $concordanceState = 'awaiting-scheduler-run'
    $reasonCode = 'unattended-interval-concordance-scheduler-run-pending'
    $nextAction = [string] $schedulerExecutionReceiptState.nextAction
} elseif ($null -eq $lastScheduledRunUtc -or $null -eq $lastReleaseCandidateRunUtc) {
    $concordanceState = 'awaiting-cycle-match'
    $reasonCode = 'unattended-interval-concordance-cycle-time-missing'
    $nextAction = 'allow-next-scheduled-cycle'
} else {
    $deltaMinutes = ($lastReleaseCandidateRunUtc - $lastScheduledRunUtc).TotalMinutes
    if ([math]::Abs($deltaMinutes) -le $toleranceMinutes) {
        $concordanceState = 'concordant-unattended-interval'
        $reasonCode = 'unattended-interval-concordance-matched'
        $nextAction = 'allow-next-unattended-window'
    } elseif ($deltaMinutes -gt $toleranceMinutes) {
        $concordanceState = 'manual-overhang-after-scheduler-proof'
        $reasonCode = 'unattended-interval-concordance-manual-overhang'
        $nextAction = 'allow-next-scheduled-cycle-to-refresh-proof'
    } else {
        $concordanceState = 'awaiting-cycle-match'
        $reasonCode = 'unattended-interval-concordance-cycle-lags-scheduler'
        $nextAction = 'allow-cycle-state-to-catch-up'
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $unattendedIntervalConcordanceOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'unattended-interval-concordance.json'
$bundleMarkdownPath = Join-Path $bundlePath 'unattended-interval-concordance.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    concordanceState = $concordanceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    schedulerReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
    lastScheduledRunUtc = if ($null -ne $lastScheduledRunUtc) { $lastScheduledRunUtc.ToString('o') } else { $null }
    lastReleaseCandidateRunUtc = if ($null -ne $lastReleaseCandidateRunUtc) { $lastReleaseCandidateRunUtc.ToString('o') } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Unattended Interval Concordance',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Concordance state: `{0}`' -f $payload.concordanceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Scheduler receipt state: `{0}`' -f $(if ($payload.schedulerReceiptState) { $payload.schedulerReceiptState } else { 'missing' })),
    ('- Last scheduled run (UTC): `{0}`' -f $(if ($payload.lastScheduledRunUtc) { $payload.lastScheduledRunUtc } else { 'not-yet-run' })),
    ('- Last release-candidate run (UTC): `{0}`' -f $(if ($payload.lastReleaseCandidateRunUtc) { $payload.lastReleaseCandidateRunUtc } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    concordanceState = $payload.concordanceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $unattendedIntervalConcordanceStatePath -Value $statePayload
Write-Host ('[unattended-interval-concordance] Bundle: {0}' -f $bundlePath)
$bundlePath
