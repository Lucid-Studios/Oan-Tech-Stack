[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $TaskName = 'OAN Mortalis Governed Automation Cycle',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [int] $IntervalHours = 6,
    [datetime] $StartAt = ([datetime]::Now.AddMinutes(5)),
    [string] $RepoRoot
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

if (-not (Test-Path -LiteralPath $cycleScriptPath -PathType Leaf)) {
    throw "Local automation cycle script not found at '$cycleScriptPath'."
}

$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ('-ExecutionPolicy Bypass -File "{0}" -Configuration {1}' -f $cycleScriptPath, $Configuration)
$trigger = New-ScheduledTaskTrigger -Once -At $StartAt -RepetitionInterval (New-TimeSpan -Hours $IntervalHours) -RepetitionDuration (New-TimeSpan -Days 1)
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew
$description = 'Runs the governed OAN release-candidate conveyor on a local cadence and emits a 24-hour digest for trust-verified HITL review.'

if ($PSCmdlet.ShouldProcess($TaskName, 'Register scheduled task')) {
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description $description -Force | Out-Null
}

Write-Host ('[local-automation-task] TaskName: {0}' -f $TaskName)
Write-Host ('[local-automation-task] IntervalHours: {0}' -f $IntervalHours)
Write-Host ('[local-automation-task] StartAt: {0}' -f $StartAt.ToString('o'))
