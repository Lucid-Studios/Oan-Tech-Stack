[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $TaskName = 'OAN Mortalis Governed HITL Digest',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [int] $CadenceHours = 24,
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$digestScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Local-Automation-HitlDigest.ps1'
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedCycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-cycle.json'

if (-not (Test-Path -LiteralPath $digestScriptPath -PathType Leaf)) {
    throw "Local automation HITL digest script not found at '$digestScriptPath'."
}

$desiredStartUtc = $null
if (-not $PSBoundParameters.ContainsKey('StartAt')) {
    if (Test-Path -LiteralPath $resolvedCyclePolicyPath -PathType Leaf) {
        $policy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
        $topology = $policy.PSObject.Properties['schedulerTaskTopology']
        if ($null -ne $topology -and $null -ne $topology.Value.PSObject.Properties['dailyDigestCadenceHours']) {
            $CadenceHours = [int] $topology.Value.dailyDigestCadenceHours
        }
    }

    if (Test-Path -LiteralPath $resolvedCycleStatePath -PathType Leaf) {
        $state = Get-Content -Raw -LiteralPath $resolvedCycleStatePath | ConvertFrom-Json
        $desiredDigestValue = $null
        if ($state.PSObject.Properties['nextDailyHitlDigestRunUtc']) {
            $desiredDigestValue = $state.nextDailyHitlDigestRunUtc
        } elseif ($state.PSObject.Properties['nextMandatoryHitlReviewUtc']) {
            $desiredDigestValue = $state.nextMandatoryHitlReviewUtc
        }

        $desiredStartUtc = Get-OptionalDateTimeUtc -Value $desiredDigestValue
    }

    if ($null -ne $desiredStartUtc) {
        $StartAt = Get-SafeLocalStartTime -DesiredUtc $desiredStartUtc
    } else {
        $StartAt = Get-SafeLocalStartTime -DesiredUtc ((Get-Date).ToUniversalTime().AddHours($CadenceHours))
    }
}

$scheduledPowershellArguments = @(
    '-NoProfile',
    '-NonInteractive',
    '-WindowStyle', 'Hidden',
    '-ExecutionPolicy', 'Bypass',
    '-File', ('"{0}"' -f $digestScriptPath),
    '-Configuration', $Configuration,
    '-RepoRoot', ('"{0}"' -f $resolvedRepoRoot)
)

$action = New-ScheduledTaskAction `
    -Execute 'powershell.exe' `
    -Argument ([string]::Join(' ', $scheduledPowershellArguments)) `
    -WorkingDirectory $resolvedRepoRoot
$trigger = New-ScheduledTaskTrigger -Once -At $StartAt -RepetitionInterval (New-TimeSpan -Hours $CadenceHours) -RepetitionDuration (New-TimeSpan -Days 3650)
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew
$description = 'Runs the daily OAN HITL digest surface independently of the main worker cadence.'

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
Write-Host ('[local-automation-task] Mode: repeating-daily-digest')
Write-Host ('[local-automation-task] StartAt: {0}' -f $StartAt.ToString('o'))
Write-Host ('[local-automation-task] State: {0}' -f $registeredTask.State)
if ($registeredTaskInfo.NextRunTime -and $registeredTaskInfo.NextRunTime.Year -gt 1900) {
    Write-Host ('[local-automation-task] NextRunTime: {0}' -f ([datetime] $registeredTaskInfo.NextRunTime).ToUniversalTime().ToString('o'))
}
