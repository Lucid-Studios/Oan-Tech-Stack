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

function Get-MeaningfulScheduledDateTimeUtcOrNull {
    param([datetime] $Value)

    $utcValue = $Value.ToUniversalTime()
    if ($utcValue -le [datetime]'2000-01-01T00:00:00Z') {
        return $null
    }

    return $utcValue
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$schedulerExecutionReceiptOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptOutputRoot)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the scheduler execution receipt can run.'
}

$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$taskName = 'OAN Mortalis Governed Automation Cycle'
$schedulerRegistered = $false
$schedulerState = 'not-registered'
$lastScheduledRunUtc = $null
$nextScheduledRunUtc = $null

if (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue) {
    try {
        $scheduledTask = Get-ScheduledTask -TaskName $taskName -ErrorAction Stop
        $scheduledInfo = Get-ScheduledTaskInfo -TaskName $taskName -ErrorAction Stop
        $schedulerRegistered = $true
        $schedulerState = [string] $scheduledTask.State
        $lastScheduledRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $scheduledInfo.LastRunTime)
        $nextScheduledRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $scheduledInfo.NextRunTime)
    } catch {
    }
}

$receiptState = 'candidate-only'
$reasonCode = 'scheduler-execution-receipt-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $receiptState = 'blocked'
    $reasonCode = 'scheduler-execution-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif (-not $schedulerRegistered) {
    $receiptState = 'awaiting-scheduler-registration'
    $reasonCode = 'scheduler-execution-receipt-scheduler-unregistered'
    $nextAction = 'register-or-reconcile-scheduler'
} elseif ($null -eq $lastScheduledRunUtc) {
    $receiptState = 'awaiting-scheduler-run'
    $reasonCode = 'scheduler-execution-receipt-not-yet-observed'
    $nextAction = 'allow-scheduled-cycle-to-fire'
} else {
    $receiptState = 'receipt-captured'
    $reasonCode = 'scheduler-execution-receipt-captured'
    $nextAction = 'reconcile-unattended-interval'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $schedulerExecutionReceiptOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'scheduler-execution-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'scheduler-execution-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    receiptState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    taskName = $taskName
    schedulerRegistered = $schedulerRegistered
    schedulerState = $schedulerState
    lastScheduledRunUtc = if ($null -ne $lastScheduledRunUtc) { $lastScheduledRunUtc.ToString('o') } else { $null }
    nextScheduledRunUtc = if ($null -ne $nextScheduledRunUtc) { $nextScheduledRunUtc.ToString('o') } else { $null }
    schedulerReconciliationAction = if ($null -ne $schedulerReconciliationState) { [string] $schedulerReconciliationState.actionTaken } else { $null }
    lastReleaseCandidateRunUtc = [string] $cycleState.lastReleaseCandidateRunUtc
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Scheduler Execution Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.receiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Scheduler registered: `{0}`' -f $payload.schedulerRegistered),
    ('- Scheduler state: `{0}`' -f $payload.schedulerState),
    ('- Last scheduled run (UTC): `{0}`' -f $(if ($payload.lastScheduledRunUtc) { $payload.lastScheduledRunUtc } else { 'not-yet-run' })),
    ('- Next scheduled run (UTC): `{0}`' -f $(if ($payload.nextScheduledRunUtc) { $payload.nextScheduledRunUtc } else { 'not-available' })),
    ('- Scheduler reconciliation action: `{0}`' -f $(if ($payload.schedulerReconciliationAction) { $payload.schedulerReconciliationAction } else { 'unknown' })),
    ('- Last release-candidate run (UTC): `{0}`' -f $(if ($payload.lastReleaseCandidateRunUtc) { $payload.lastReleaseCandidateRunUtc } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    receiptState = $payload.receiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    lastScheduledRunUtc = $payload.lastScheduledRunUtc
    nextScheduledRunUtc = $payload.nextScheduledRunUtc
}

Write-JsonFile -Path $schedulerExecutionReceiptStatePath -Value $statePayload
Write-Host ('[scheduler-execution-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
