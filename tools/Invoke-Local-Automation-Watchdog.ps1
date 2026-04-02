param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
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
$policy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statePath)
$watchdogOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.watchdogOutputRoot)
$watchdogStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.watchdogStatePath)
$schedulerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.schedulerReconciliationStatePath)
$syncScriptPath = Join-Path $resolvedRepoRoot 'tools\Sync-Local-AutomationScheduler.ps1'
$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
$nowUtc = (Get-Date).ToUniversalTime()

$syncOutput = & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $syncScriptPath -Configuration $Configuration -RepoRoot $resolvedRepoRoot -CyclePolicyPath $resolvedCyclePolicyPath
if ($LASTEXITCODE -ne 0) {
    throw 'Scheduler reconciliation failed during watchdog execution.'
}

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$schedulerState = Read-JsonFileOrNull -Path $schedulerStatePath

if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the watchdog can run.'
}

$terminalState = [string] $cycleState.mainWorkerTerminalState
$armState = [string] $cycleState.mainWorkerArmState
$watchdogState = switch ($terminalState) {
    'pause-hitl' { 'paused-hitl' }
    'done' { 'done-retired' }
    default {
        switch ($armState) {
            'armed' { 'healthy-armed' }
            'awaiting-rearm' { 'healthy-awaiting-rearm' }
            'drift-detected' { 'drift-detected' }
            'fault-recoverable' { 'fault-recoverable' }
            default {
                if ([string] $cycleState.lastKnownStatus -eq [string] $policy.blockedStatus) {
                    'fault-recoverable'
                } else {
                    'healthy-awaiting-rearm'
                }
            }
        }
    }
}

$reasonCode = switch ($watchdogState) {
    'healthy-armed' { 'watchdog-main-worker-armed' }
    'healthy-awaiting-rearm' { 'watchdog-main-worker-awaiting-rearm' }
    'paused-hitl' { 'watchdog-main-worker-paused-hitl' }
    'done-retired' { 'watchdog-main-worker-retired' }
    'fault-recoverable' { 'watchdog-main-worker-fault-recoverable' }
    default { 'watchdog-main-worker-drift-detected' }
}

$nextAction = switch ($watchdogState) {
    'healthy-armed' { 'allow-main-worker-close-governed-continuation' }
    'healthy-awaiting-rearm' { 'reconcile-next-main-worker-wake' }
    'paused-hitl' { 'preserve-pause-until-operator-resume' }
    'done-retired' { 'leave-main-worker-retired' }
    'fault-recoverable' { 'await-hitl-or-remediation-before-rearm' }
    default { 'inspect-watchdog-drift-before-rearm' }
}

$bundlePath = Join-Path $watchdogOutputRoot ($nowUtc.ToString('yyyyMMddTHHmmssZ'))
$bundleJsonPath = Join-Path $bundlePath 'automation-watchdog.json'
$bundleMarkdownPath = Join-Path $bundlePath 'automation-watchdog.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    watchdogState = $watchdogState
    reasonCode = $reasonCode
    nextAction = $nextAction
    mainWorkerTerminalState = $terminalState
    mainWorkerArmState = $armState
    nextMainWorkerWakeUtc = [string] $cycleState.nextMainWorkerWakeUtc
    schedulerAligned = if ($null -ne $schedulerState) { [bool] $schedulerState.aligned } else { $false }
    schedulerStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $schedulerStatePath
}

Write-JsonFile -Path $bundleJsonPath -Value $payload
Set-Content -LiteralPath $bundleMarkdownPath -Value @(
    '# Local Automation Watchdog',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Watchdog state: `{0}`' -f $payload.watchdogState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Main worker terminal state: `{0}`' -f $payload.mainWorkerTerminalState),
    ('- Main worker arm state: `{0}`' -f $payload.mainWorkerArmState),
    ('- Next main-worker wake (UTC): `{0}`' -f $(if ($payload.nextMainWorkerWakeUtc) { $payload.nextMainWorkerWakeUtc } else { 'none' })),
    ('- Scheduler aligned: `{0}`' -f $payload.schedulerAligned)
) -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    watchdogState = $payload.watchdogState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}
Write-JsonFile -Path $watchdogStatePath -Value $statePayload

if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot | Out-Null
}

Write-Host ('[local-automation-watchdog] Bundle: {0}' -f $bundlePath)
$bundlePath
