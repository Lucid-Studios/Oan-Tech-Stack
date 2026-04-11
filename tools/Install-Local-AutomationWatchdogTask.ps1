[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $TaskName = 'OAN Mortalis Governed Automation Watchdog',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [int] $CadenceHours = 1,
    [datetime] $StartAt,
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

function Get-SafeLocalStartTime {
    param([datetime] $DesiredUtc)

    $minimumUtc = (Get-Date).ToUniversalTime().AddSeconds(15)
    $effectiveUtc = if ($DesiredUtc -lt $minimumUtc) { $minimumUtc } else { $DesiredUtc }
    return [System.TimeZoneInfo]::ConvertTimeFromUtc($effectiveUtc, [System.TimeZoneInfo]::Local)
}

function Get-NextHourlyAnchorUtc {
    param([int] $Minute)

    $localNow = Get-Date
    $candidateLocal = Get-Date -Year $localNow.Year -Month $localNow.Month -Day $localNow.Day -Hour $localNow.Hour -Minute $Minute -Second 0
    if ($candidateLocal -le $localNow) {
        $candidateLocal = $candidateLocal.AddHours(1)
    }

    return $candidateLocal.ToUniversalTime()
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$watchdogScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Local-Automation-Watchdog.ps1'
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedCycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-cycle.json'

if (-not (Test-Path -LiteralPath $watchdogScriptPath -PathType Leaf)) {
    throw "Local automation watchdog script not found at '$watchdogScriptPath'."
}

if (-not $PSBoundParameters.ContainsKey('StartAt')) {
    if (Test-Path -LiteralPath $resolvedCyclePolicyPath -PathType Leaf) {
        $policy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
        $topology = $policy.PSObject.Properties['schedulerTaskTopology']
        if ($null -ne $topology -and $null -ne $topology.Value.PSObject.Properties['watchdogCadenceHours']) {
            $CadenceHours = [int] $topology.Value.watchdogCadenceHours
        }
    }

    $StartAt = Get-SafeLocalStartTime -DesiredUtc (Get-NextHourlyAnchorUtc -Minute 0)
}

$scheduledPowershellArguments = @(
    '-NoProfile',
    '-NonInteractive',
    '-WindowStyle', 'Hidden',
    '-ExecutionPolicy', 'Bypass',
    '-File', ('"{0}"' -f $watchdogScriptPath),
    '-Configuration', $Configuration,
    '-RepoRoot', ('"{0}"' -f $resolvedRepoRoot)
)

$action = New-ScheduledTaskAction `
    -Execute 'powershell.exe' `
    -Argument ([string]::Join(' ', $scheduledPowershellArguments)) `
    -WorkingDirectory $resolvedRepoRoot
$trigger = New-ScheduledTaskTrigger -Once -At $StartAt -RepetitionInterval (New-TimeSpan -Hours $CadenceHours) -RepetitionDuration (New-TimeSpan -Days 3650)
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew
$description = 'Runs the hourly OAN automation watchdog for reflection, drift detection, and crash recovery.'

if ($PSCmdlet.ShouldProcess($TaskName, 'Register scheduled task')) {
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description $description -Force | Out-Null
}

$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot | Out-Null
}

$registeredTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
$registeredTaskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
Write-Host ('[local-automation-task] TaskName: {0}' -f $TaskName)
Write-Host ('[local-automation-task] Mode: repeating-watchdog')
Write-Host ('[local-automation-task] StartAt: {0}' -f $StartAt.ToString('o'))
Write-Host ('[local-automation-task] State: {0}' -f $registeredTask.State)
if ($registeredTaskInfo.NextRunTime -and $registeredTaskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[local-automation-task] NextRunTime: {0}' -f ([datetime] $registeredTaskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
