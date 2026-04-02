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

function Get-MeaningfulScheduledDateTimeUtcOrNull {
    param([datetime] $Value)

    $utcValue = $Value.ToUniversalTime()
    if ($utcValue -le [datetime]'2000-01-01T00:00:00Z') {
        return $null
    }

    return $utcValue
}

function Get-TaskSnapshot {
    param([string] $TaskName)

    $snapshot = [ordered]@{
        taskName = $TaskName
        registered = $false
        state = 'not-registered'
        lastRunUtc = $null
        nextRunUtc = $null
    }

    if (-not (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue)) {
        return [pscustomobject] $snapshot
    }

    try {
        $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        $taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
        $snapshot.registered = $true
        $snapshot.state = [string] $task.State
        $snapshot.lastRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $taskInfo.LastRunTime)
        if ([string]::Equals([string] $task.State, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase)) {
            $snapshot.nextRunUtc = $null
        } else {
            $snapshot.nextRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $taskInfo.NextRunTime)
        }
    } catch {
    }

    return [pscustomobject] $snapshot
}

function Convert-SnapshotForPayload {
    param([object] $Snapshot)

    return [ordered]@{
        taskName = [string] $Snapshot.taskName
        registered = [bool] $Snapshot.registered
        state = [string] $Snapshot.state
        lastRunUtc = if ($null -ne $Snapshot.lastRunUtc) { $Snapshot.lastRunUtc.ToString('o') } else { $null }
        nextRunUtc = if ($null -ne $Snapshot.nextRunUtc) { $Snapshot.nextRunUtc.ToString('o') } else { $null }
    }
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
$topology = $cyclePolicy.schedulerTaskTopology

$mainWorkerSnapshot = Get-TaskSnapshot -TaskName ([string] $topology.mainWorkerTaskName)
$watchdogSnapshot = Get-TaskSnapshot -TaskName ([string] $topology.watchdogTaskName)
$dailyDigestSnapshot = Get-TaskSnapshot -TaskName ([string] $topology.dailyDigestTaskName)

$terminalState = [string] $cycleState.mainWorkerTerminalState
$armState = [string] $cycleState.mainWorkerArmState
$receiptState = switch ($terminalState) {
    'pause-hitl' { 'paused-hitl' }
    'done' { 'done-retired' }
    'fault-recoverable' { 'fault-recoverable' }
    default {
        if ($armState -eq 'armed') {
            'healthy-armed'
        } elseif ($armState -eq 'awaiting-rearm') {
            'healthy-awaiting-rearm'
        } elseif ($armState -eq 'drift-detected') {
            'drift-detected'
        } else {
            'healthy-awaiting-rearm'
        }
    }
}

$reasonCode = switch ($receiptState) {
    'healthy-armed' { 'scheduler-execution-main-worker-armed' }
    'healthy-awaiting-rearm' { 'scheduler-execution-awaiting-rearm' }
    'paused-hitl' { 'scheduler-execution-paused-hitl' }
    'done-retired' { 'scheduler-execution-done-retired' }
    'fault-recoverable' { 'scheduler-execution-fault-recoverable' }
    default { 'scheduler-execution-drift-detected' }
}

$nextAction = switch ($receiptState) {
    'healthy-armed' { 'allow-main-worker-to-run' }
    'healthy-awaiting-rearm' { 'allow-watchdog-or-close-handler-to-rearm' }
    'paused-hitl' { 'await-operator-resume' }
    'done-retired' { 'hold-complete-state-until-new-admission' }
    'fault-recoverable' { 'await-remediation-before-rearm' }
    default { 'inspect-drift-before-rearm' }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleSuffix = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.runId)) { [string] $cycleState.runId } else { 'no-run' }
$bundlePath = Join-Path $schedulerExecutionReceiptOutputRoot ('{0}-{1}' -f $timestamp, $bundleSuffix)
$bundleJsonPath = Join-Path $bundlePath 'scheduler-execution-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'scheduler-execution-receipt.md'

$payload = [ordered]@{
    schemaVersion = 2
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    receiptState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    mainWorkerTerminalState = $terminalState
    mainWorkerArmState = $armState
    schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $false }
    schedulerReconciliationAction = if ($null -ne $schedulerReconciliationState) { @($schedulerReconciliationState.actionTaken | ForEach-Object { [string] $_ }) } else { @() }
    tasks = [ordered]@{
        mainWorker = Convert-SnapshotForPayload -Snapshot $mainWorkerSnapshot
        watchdog = Convert-SnapshotForPayload -Snapshot $watchdogSnapshot
        dailyDigest = Convert-SnapshotForPayload -Snapshot $dailyDigestSnapshot
    }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
    sourceDigestBundle = [string] $cycleState.lastDigestBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Scheduler Execution Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.receiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Main-worker terminal state: `{0}`' -f $payload.mainWorkerTerminalState),
    ('- Main-worker arm state: `{0}`' -f $payload.mainWorkerArmState),
    ('- Scheduler aligned: `{0}`' -f $payload.schedulerAligned),
    ''
)

foreach ($taskEntry in @(
        [ordered]@{ Label = 'Main Worker'; Snapshot = $payload.tasks.mainWorker },
        [ordered]@{ Label = 'Watchdog'; Snapshot = $payload.tasks.watchdog },
        [ordered]@{ Label = 'Daily Digest'; Snapshot = $payload.tasks.dailyDigest }
    )) {
    $snapshot = $taskEntry.Snapshot
    $markdownLines += @(
        ('## {0}' -f $taskEntry.Label),
        '',
        ('- Task name: `{0}`' -f $snapshot.taskName),
        ('- Registered: `{0}`' -f $snapshot.registered),
        ('- State: `{0}`' -f $snapshot.state),
        ('- Last run (UTC): `{0}`' -f $(if ($snapshot.lastRunUtc) { $snapshot.lastRunUtc } else { 'not-yet-run' })),
        ('- Next run (UTC): `{0}`' -f $(if ($snapshot.nextRunUtc) { $snapshot.nextRunUtc } else { 'not-scheduled' })),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 2
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    receiptState = $payload.receiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    mainWorkerTerminalState = $payload.mainWorkerTerminalState
    mainWorkerArmState = $payload.mainWorkerArmState
    schedulerAligned = $payload.schedulerAligned
    tasks = $payload.tasks
}

Write-JsonFile -Path $schedulerExecutionReceiptStatePath -Value $statePayload
Write-Host ('[scheduler-execution-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
