param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
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
$schedulerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$cycleState = Read-JsonFileOrNull -Path $cycleStatePath

if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before scheduler reconciliation can run.'
}

$desiredNextRunUtc = [datetime]::Parse([string] $cycleState.nextReleaseCandidateRunUtc, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
$taskName = 'OAN Mortalis Governed Automation Cycle'
$toleranceMinutes = [int] $cyclePolicy.schedulerReconciliationPolicy.driftToleranceMinutes
$actionTaken = 'none'
$registeredBefore = $false
$previousNextRunUtc = $null
$finalNextRunUtc = $null

if (-not (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue)) {
    $statePayload = [ordered]@{
        schemaVersion = 1
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        actionTaken = 'unsupported'
        desiredNextRunUtc = $desiredNextRunUtc.ToString('o')
        aligned = $false
    }

    Write-JsonFile -Path $schedulerStatePath -Value $statePayload
    Write-Host ('[local-automation-scheduler-sync] State: {0}' -f $schedulerStatePath)
    $schedulerStatePath
    return
}

try {
    $task = Get-ScheduledTask -TaskName $taskName -ErrorAction Stop
    $taskInfo = Get-ScheduledTaskInfo -TaskName $taskName -ErrorAction Stop
    $registeredBefore = $true
    $previousNextRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $taskInfo.NextRunTime)
}
catch {
    $registeredBefore = $false
}

$driftMinutes = if ($null -ne $previousNextRunUtc) {
    [math]::Abs(($previousNextRunUtc - $desiredNextRunUtc).TotalMinutes)
} else {
    [double]::PositiveInfinity
}

if (-not $registeredBefore -or $driftMinutes -gt $toleranceMinutes) {
    $installScriptPath = Join-Path $resolvedRepoRoot 'tools\Install-Local-AutomationCycleTask.ps1'
    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $installScriptPath `
        -RepoRoot $resolvedRepoRoot `
        -TaskName $taskName `
        -Configuration $Configuration `
        -IntervalHours ([int] $cyclePolicy.localReleaseCandidateCadenceHours) | Out-Null

    $actionTaken = if ($registeredBefore) { 'rescheduled' } else { 'registered' }
}

$finalTask = Get-ScheduledTask -TaskName $taskName -ErrorAction Stop
$finalTaskInfo = Get-ScheduledTaskInfo -TaskName $taskName -ErrorAction Stop
$finalNextRunUtc = Get-MeaningfulScheduledDateTimeUtcOrNull -Value ([datetime] $finalTaskInfo.NextRunTime)
$finalDriftMinutes = if ($null -ne $finalNextRunUtc) {
    [math]::Abs(($finalNextRunUtc - $desiredNextRunUtc).TotalMinutes)
} else {
    [double]::PositiveInfinity
}

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    taskName = $taskName
    registeredBefore = $registeredBefore
    actionTaken = $actionTaken
    desiredNextRunUtc = $desiredNextRunUtc.ToString('o')
    previousNextRunUtc = if ($null -ne $previousNextRunUtc) { $previousNextRunUtc.ToString('o') } else { $null }
    finalNextRunUtc = if ($null -ne $finalNextRunUtc) { $finalNextRunUtc.ToString('o') } else { $null }
    driftToleranceMinutes = $toleranceMinutes
    finalDriftMinutes = $finalDriftMinutes
    aligned = ($finalDriftMinutes -le $toleranceMinutes)
}

Write-JsonFile -Path $schedulerStatePath -Value $statePayload
Write-Host ('[local-automation-scheduler-sync] State: {0}' -f $schedulerStatePath)
$schedulerStatePath
