[CmdletBinding()]
param(
    [string] $TaskName = 'OAN Mortalis Governed Automation Cycle',
    [string] $RepoRoot,
    [switch] $StartNow
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)

if (-not (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue)) {
    throw 'Windows scheduled-task support is unavailable on this machine.'
}

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
if ($task.State -eq 'Disabled') {
    Enable-ScheduledTask -TaskName $TaskName -ErrorAction Stop | Out-Null
}

if ($StartNow.IsPresent) {
    Start-ScheduledTask -TaskName $TaskName -ErrorAction Stop
}

$taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot | Out-Null
}

Write-Host ('[local-automation-task] TaskName: {0}' -f $TaskName)
Write-Host ('[local-automation-task] State: {0}' -f (Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop).State)
if ($taskInfo.NextRunTime -and $taskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[local-automation-task] NextRunTime: {0}' -f ([datetime] $taskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
