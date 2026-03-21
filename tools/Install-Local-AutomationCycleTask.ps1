[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $TaskName = 'OAN Mortalis Governed Automation Cycle',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [int] $IntervalHours = 6,
    [datetime] $StartAt,
    [string] $RepoRoot,
    [string] $CycleStatePath = '.audit/state/local-automation-cycle.json'
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

if ($IntervalHours -lt 1) {
    throw 'IntervalHours must be greater than or equal to 1.'
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$cycleScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Local-Automation-Cycle.ps1'
$resolvedCycleStatePath = if ([System.IO.Path]::IsPathRooted($CycleStatePath)) {
    [System.IO.Path]::GetFullPath($CycleStatePath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $resolvedRepoRoot $CycleStatePath))
}

if (-not (Test-Path -LiteralPath $cycleScriptPath -PathType Leaf)) {
    throw "Local automation cycle script not found at '$cycleScriptPath'."
}

if (-not $PSBoundParameters.ContainsKey('StartAt')) {
    $nextRunUtc = $null
    if (Test-Path -LiteralPath $resolvedCycleStatePath -PathType Leaf) {
        $state = Get-Content -Raw -LiteralPath $resolvedCycleStatePath | ConvertFrom-Json
        $nextRunString = $state.PSObject.Properties['nextReleaseCandidateRunUtc']
        if ($null -ne $nextRunString -and -not [string]::IsNullOrWhiteSpace([string] $nextRunString.Value)) {
            $nextRunUtc = [datetime]::Parse([string] $nextRunString.Value, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
        }
    }

    if ($null -ne $nextRunUtc) {
        $StartAt = [System.TimeZoneInfo]::ConvertTimeFromUtc($nextRunUtc, [System.TimeZoneInfo]::Local)
    } else {
        $StartAt = [datetime]::Now.AddMinutes(5)
    }
}

$action = New-ScheduledTaskAction `
    -Execute 'powershell.exe' `
    -Argument ('-ExecutionPolicy Bypass -File "{0}" -Configuration {1} -RepoRoot "{2}"' -f $cycleScriptPath, $Configuration, $resolvedRepoRoot) `
    -WorkingDirectory $resolvedRepoRoot
$trigger = New-ScheduledTaskTrigger -Once -At $StartAt -RepetitionInterval (New-TimeSpan -Hours $IntervalHours) -RepetitionDuration (New-TimeSpan -Days 3650)
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew
$description = 'Runs the governed OAN release-candidate conveyor on a local cadence and emits a 24-hour digest for trust-verified HITL review.'

if ($PSCmdlet.ShouldProcess($TaskName, 'Register scheduled task')) {
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description $description -Force | Out-Null
}

$registeredTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
$registeredTaskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop

$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    & powershell -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot | Out-Null
}

Write-Host ('[local-automation-task] TaskName: {0}' -f $TaskName)
Write-Host ('[local-automation-task] IntervalHours: {0}' -f $IntervalHours)
Write-Host ('[local-automation-task] StartAt: {0}' -f $StartAt.ToString('o'))
Write-Host ('[local-automation-task] State: {0}' -f $registeredTask.State)
if ($registeredTaskInfo.NextRunTime -and $registeredTaskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[local-automation-task] NextRunTime: {0}' -f ([datetime] $registeredTaskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
