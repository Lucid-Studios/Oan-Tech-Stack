[CmdletBinding()]
param(
    [string] $TaskName = 'OAN Mortalis Governed Automation Cycle',
    [string] $RepoRoot,
    [switch] $StartNow,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json'
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

function Set-JsonNoteProperty {
    param(
        [object] $InputObject,
        [string] $PropertyName,
        [object] $Value
    )

    if ($null -eq $InputObject) {
        throw 'InputObject is required.'
    }

    if ($InputObject.PSObject.Properties[$PropertyName]) {
        $InputObject.PSObject.Properties[$PropertyName].Value = $Value
    } else {
        Add-Member -InputObject $InputObject -NotePropertyName $PropertyName -NotePropertyValue $Value -Force
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$policy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statePath)
$pauseNoticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.pauseNoticeStatePath)
$schedulerSyncScriptPath = Join-Path $resolvedRepoRoot 'tools\Sync-Local-AutomationScheduler.ps1'
$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
$topology = $policy.schedulerTaskTopology

if (-not (Test-Path -LiteralPath $cycleStatePath -PathType Leaf)) {
    throw 'Local automation cycle state is required before resume can run.'
}

$cycleState = Get-Content -Raw -LiteralPath $cycleStatePath | ConvertFrom-Json
$terminalState = [string] $cycleState.mainWorkerTerminalState
$armState = [string] $cycleState.mainWorkerArmState
$lastKnownStatus = [string] $cycleState.lastKnownStatus

if ($lastKnownStatus -eq [string] $policy.blockedStatus -or $terminalState -eq 'fault-recoverable') {
    throw 'Main worker cannot be resumed until the current blocked or fault-recoverable state is cleared.'
}

if ($terminalState -eq 'done' -or $armState -eq 'done-retired') {
    throw 'Main worker is retired for the current objective and may not be resumed without a new admission.'
}

$nextWakeUtc = if ($StartNow.IsPresent) {
    (Get-Date).ToUniversalTime().AddSeconds(15)
} else {
    (Get-Date).ToUniversalTime().AddMinutes([int] $topology.mainWorkerCadenceMinutes)
}

Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'mainWorkerTerminalState' -Value 'continue'
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'mainWorkerArmState' -Value 'awaiting-rearm'
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'lastMainWorkerCloseDisposition' -Value 'resumed-by-operator'
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextMainWorkerWakeUtc' -Value $nextWakeUtc.ToString('o')
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextAutomationCycleRunUtc' -Value $nextWakeUtc.ToString('o')
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'currentNoticeType' -Value $null
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'currentNoticeStatus' -Value $null
Write-JsonFile -Path $cycleStatePath -Value $cycleState

if (Test-Path -LiteralPath $pauseNoticeStatePath -PathType Leaf) {
    Remove-Item -LiteralPath $pauseNoticeStatePath -Force
}

& powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $schedulerSyncScriptPath -Configuration Release -RepoRoot $resolvedRepoRoot -CyclePolicyPath $resolvedCyclePolicyPath | Out-Null

if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot | Out-Null
}

$taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
Write-Host ('[local-automation-task] TaskName: {0}' -f $TaskName)
Write-Host ('[local-automation-task] ResumeMode: reconstitution')
if ($taskInfo.NextRunTime -and $taskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[local-automation-task] NextRunTime: {0}' -f ([datetime] $taskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
