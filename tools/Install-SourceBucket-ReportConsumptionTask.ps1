[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $TaskName = 'OAN Mortalis Source-Bucket Report Consumption',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [int] $CadenceHours = 1,
    [datetime] $StartAt,
    [string] $RepoRoot,
    [string] $ConsumptionPolicyPath = 'OAN Mortalis V1.1.1/Automation/source-bucket-report-consumption.json'
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

function Get-NextHalfHourUtc {
    $localNow = Get-Date
    $candidateLocal = Get-Date -Year $localNow.Year -Month $localNow.Month -Day $localNow.Day -Hour $localNow.Hour -Minute 30 -Second 0
    if ($candidateLocal -le $localNow) {
        $candidateLocal = $candidateLocal.AddHours(1)
    }

    return $candidateLocal.ToUniversalTime()
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$consumerScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-SourceBucket-ReportConsumption.ps1'
$resolvedConsumptionPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $ConsumptionPolicyPath

if (-not (Test-Path -LiteralPath $consumerScriptPath -PathType Leaf)) {
    throw "Source-bucket report consumption script not found at '$consumerScriptPath'."
}

if (-not $PSBoundParameters.ContainsKey('StartAt')) {
    $desiredStartUtc = $null
    if (Test-Path -LiteralPath $resolvedConsumptionPolicyPath -PathType Leaf) {
        $policy = Get-Content -Raw -LiteralPath $resolvedConsumptionPolicyPath | ConvertFrom-Json
        $statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.reportConsumptionStatePath)
        if (Test-Path -LiteralPath $statePath -PathType Leaf) {
            $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
            $desiredStartUtc = Get-OptionalDateTimeUtc -Value ([string] $state.nextRunUtc)
        }
    }

    if ($null -eq $desiredStartUtc) {
        $desiredStartUtc = Get-NextHalfHourUtc
    }

    $StartAt = Get-SafeLocalStartTime -DesiredUtc $desiredStartUtc
}

$scheduledPowershellArguments = @(
    '-NoProfile',
    '-NonInteractive',
    '-WindowStyle', 'Hidden',
    '-ExecutionPolicy', 'Bypass',
    '-File', ('"{0}"' -f $consumerScriptPath),
    '-Configuration', $Configuration,
    '-RepoRoot', ('"{0}"' -f $resolvedRepoRoot)
)

$action = New-ScheduledTaskAction `
    -Execute 'powershell.exe' `
    -Argument ([string]::Join(' ', $scheduledPowershellArguments)) `
    -WorkingDirectory $resolvedRepoRoot
$trigger = New-ScheduledTaskTrigger -Once -At $StartAt -RepetitionInterval (New-TimeSpan -Hours $CadenceHours) -RepetitionDuration (New-TimeSpan -Days 3650)
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew
$description = 'Consumes hourly source-bucket appendices into bounded standing summaries, continuity updates, and GEL candidate packets.'

if ($PSCmdlet.ShouldProcess($TaskName, 'Register scheduled task')) {
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description $description -Force | Out-Null
}

$registeredTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
$registeredTaskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
Write-Host ('[source-bucket-report-consumption-task] TaskName: {0}' -f $TaskName)
Write-Host ('[source-bucket-report-consumption-task] StartAt: {0}' -f $StartAt.ToString('o'))
Write-Host ('[source-bucket-report-consumption-task] State: {0}' -f $registeredTask.State)
if ($registeredTaskInfo.NextRunTime -and $registeredTaskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[source-bucket-report-consumption-task] NextRunTime: {0}' -f ([datetime] $registeredTaskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
