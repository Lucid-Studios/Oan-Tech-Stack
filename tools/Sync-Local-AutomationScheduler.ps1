param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
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

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
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

    if ($InputObject -is [System.Collections.IDictionary]) {
        $InputObject[$PropertyName] = $Value
        return
    }

    if ($InputObject.PSObject.Properties[$PropertyName]) {
        $InputObject.PSObject.Properties[$PropertyName].Value = $Value
    } else {
        Add-Member -InputObject $InputObject -NotePropertyName $PropertyName -NotePropertyValue $Value -Force
    }
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
    }
    catch {
    }

    return [pscustomobject] $snapshot
}

function Get-DriftMinutes {
    param(
        [object] $DesiredUtc,
        [object] $ActualUtc
    )

    if ($null -eq $DesiredUtc -or $null -eq $ActualUtc) {
        return [double]::PositiveInfinity
    }

    $desiredDateTime = [datetime] $DesiredUtc
    $actualDateTime = [datetime] $ActualUtc
    return [math]::Abs(($actualDateTime - $desiredDateTime).TotalMinutes)
}

function Invoke-InstallScript {
    param(
        [string] $ScriptPath,
        [string] $RepoRoot,
        [string] $TaskName,
        [string] $Configuration,
        [datetime] $DesiredStartUtc
    )

    $minimumUtc = (Get-Date).ToUniversalTime().AddSeconds(15)
    $effectiveStartUtc = if ($DesiredStartUtc -lt $minimumUtc) { $minimumUtc } else { $DesiredStartUtc }
    $localStart = [System.TimeZoneInfo]::ConvertTimeFromUtc($effectiveStartUtc, [System.TimeZoneInfo]::Local)

    & powershell -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File $ScriptPath `
        -RepoRoot $RepoRoot `
        -TaskName $TaskName `
        -Configuration $Configuration `
        -StartAt $localStart.ToString('o') | Out-Null
}

function Get-MainWorkerTerminalState {
    param([object] $CycleState)

    $terminalState = [string] $CycleState.mainWorkerTerminalState
    if (-not [string]::IsNullOrWhiteSpace($terminalState)) {
        return $terminalState
    }

    switch ([string] $CycleState.lastKnownStatus) {
        'candidate-ready' { return 'continue' }
        'hitl-required' { return 'continue' }
        'blocked' { return 'fault-recoverable' }
        'done' { return 'done' }
        default { return 'fault-recoverable' }
    }
}

function Get-MainWorkerArmState {
    param(
        [object] $CycleState,
        [string] $TerminalState
    )

    $armState = [string] $CycleState.mainWorkerArmState
    if (-not [string]::IsNullOrWhiteSpace($armState)) {
        return $armState
    }

    switch ($TerminalState) {
        'continue' { return 'awaiting-rearm' }
        'pause-hitl' { return 'paused-hitl' }
        'done' { return 'done-retired' }
        default { return 'fault-recoverable' }
    }
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

function Get-NextDailyAnchorUtc {
    param(
        [int] $Hour,
        [int] $Minute
    )

    $localNow = Get-Date
    $candidateLocal = Get-Date -Year $localNow.Year -Month $localNow.Month -Day $localNow.Day -Hour $Hour -Minute $Minute -Second 0
    if ($candidateLocal -le $localNow) {
        $candidateLocal = $candidateLocal.AddDays(1)
    }

    return $candidateLocal.ToUniversalTime()
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

$topology = $cyclePolicy.schedulerTaskTopology
$reconciliationPolicy = $cyclePolicy.schedulerReconciliationPolicy
$mainWorkerTaskName = [string] $topology.mainWorkerTaskName
$watchdogTaskName = [string] $topology.watchdogTaskName
$reportConsumptionTaskName = [string] $topology.reportConsumptionTaskName
$dailyDigestTaskName = [string] $topology.dailyDigestTaskName
$mainWorkerCadenceMinutes = [int] $topology.mainWorkerCadenceMinutes
$reportConsumptionCadenceMinutes = [int] $topology.reportConsumptionCadenceMinutes
$reportConsumptionCadenceMinuteOffset = [int] $topology.reportConsumptionCadenceMinuteOffset
$watchdogCadenceHours = [int] $topology.watchdogCadenceHours
$dailyDigestCadenceHours = [int] $topology.dailyDigestCadenceHours
$dailyDigestLocalHour = [int] $topology.dailyDigestLocalHour
$dailyDigestLocalMinute = [int] $topology.dailyDigestLocalMinute
$driftToleranceMinutes = [int] $reconciliationPolicy.driftToleranceMinutes
$pauseMainWorkerOnTerminalStates = @($reconciliationPolicy.pauseMainWorkerOnTerminalStates | ForEach-Object { [string] $_ })
$pauseMainWorkerOnBlocked = [bool] $reconciliationPolicy.pauseMainWorkerOnBlocked
$blockedStatus = [string] $cyclePolicy.blockedStatus
$nowUtc = (Get-Date).ToUniversalTime()

if (-not (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue)) {
    $unsupportedPayload = [ordered]@{
        schemaVersion = 2
        generatedAtUtc = $nowUtc.ToString('o')
        actionTaken = @('unsupported')
        aligned = $false
    }
    Write-JsonFile -Path $schedulerStatePath -Value $unsupportedPayload
    Write-Host ('[local-automation-scheduler-sync] State: {0}' -f $schedulerStatePath)
    $schedulerStatePath
    return
}

$mainWorkerTerminalState = Get-MainWorkerTerminalState -CycleState $cycleState
$desiredMainWorkerArmState = Get-MainWorkerArmState -CycleState $cycleState -TerminalState $mainWorkerTerminalState
$desiredMainWorkerWakeValue = $null
if ($cycleState.PSObject.Properties['nextMainWorkerWakeUtc']) {
    $desiredMainWorkerWakeValue = $cycleState.nextMainWorkerWakeUtc
} elseif ($cycleState.PSObject.Properties['nextAutomationCycleRunUtc']) {
    $desiredMainWorkerWakeValue = $cycleState.nextAutomationCycleRunUtc
}
$desiredMainWorkerWakeUtc = Get-OptionalDateTimeUtc -Value $desiredMainWorkerWakeValue
$desiredDigestRunValue = $null
if ($cycleState.PSObject.Properties['nextDailyHitlDigestRunUtc']) {
    $desiredDigestRunValue = $cycleState.nextDailyHitlDigestRunUtc
} elseif ($cycleState.PSObject.Properties['nextMandatoryHitlReviewUtc']) {
    $desiredDigestRunValue = $cycleState.nextMandatoryHitlReviewUtc
}
$desiredDigestRunUtc = Get-OptionalDateTimeUtc -Value $desiredDigestRunValue
$reportConsumptionState = $null
$desiredReportConsumptionRunUtc = $null
if ($cyclePolicy.PSObject.Properties['sourceBucketReportConsumptionStatePath']) {
    $reportConsumptionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sourceBucketReportConsumptionStatePath)
    $reportConsumptionState = Read-JsonFileOrNull -Path $reportConsumptionStatePath
    if ($null -ne $reportConsumptionState) {
        $desiredReportConsumptionRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $reportConsumptionState -PropertyName 'nextRunUtc')
    }
}

if ($null -eq $desiredMainWorkerWakeUtc -and $mainWorkerTerminalState -eq 'continue') {
    $desiredMainWorkerWakeUtc = Get-NextHourlyAnchorUtc -Minute 0
}
$desiredWatchdogRunUtc = Get-NextHourlyAnchorUtc -Minute 0
if ($null -eq $desiredReportConsumptionRunUtc) {
    $desiredReportConsumptionRunUtc = Get-NextHourlyAnchorUtc -Minute $reportConsumptionCadenceMinuteOffset
}
if ($null -eq $desiredDigestRunUtc) {
    $desiredDigestRunUtc = Get-NextDailyAnchorUtc -Hour $dailyDigestLocalHour -Minute $dailyDigestLocalMinute
}

$installMainWorkerScriptPath = Join-Path $resolvedRepoRoot 'tools\Install-Local-AutomationCycleTask.ps1'
$installWatchdogScriptPath = Join-Path $resolvedRepoRoot 'tools\Install-Local-AutomationWatchdogTask.ps1'
$installReportConsumptionScriptPath = Join-Path $resolvedRepoRoot 'tools\Install-SourceBucket-ReportConsumptionTask.ps1'
$installDigestScriptPath = Join-Path $resolvedRepoRoot 'tools\Install-Local-AutomationDigestTask.ps1'
$actionsTaken = New-Object System.Collections.Generic.List[string]

$mainWorkerBefore = Get-TaskSnapshot -TaskName $mainWorkerTaskName
$watchdogBefore = Get-TaskSnapshot -TaskName $watchdogTaskName
$reportConsumptionBefore = Get-TaskSnapshot -TaskName $reportConsumptionTaskName
$dailyDigestBefore = Get-TaskSnapshot -TaskName $dailyDigestTaskName

$shouldArmMainWorker = ($mainWorkerTerminalState -eq 'continue') -and ($desiredMainWorkerArmState -notin @('paused-hitl', 'done-retired', 'fault-recoverable', 'drift-detected'))
$finalMainWorkerArmState = $desiredMainWorkerArmState

foreach ($repeatingTask in @(
        [pscustomobject]@{
            TaskName = $watchdogTaskName
            DesiredRunUtc = $desiredWatchdogRunUtc
            InstallScriptPath = $installWatchdogScriptPath
        },
        [pscustomobject]@{
            TaskName = $reportConsumptionTaskName
            DesiredRunUtc = $desiredReportConsumptionRunUtc
            InstallScriptPath = $installReportConsumptionScriptPath
        },
        [pscustomobject]@{
            TaskName = $dailyDigestTaskName
            DesiredRunUtc = $desiredDigestRunUtc
            InstallScriptPath = $installDigestScriptPath
        }
    )) {
    $snapshot = Get-TaskSnapshot -TaskName $repeatingTask.TaskName
    $driftMinutes = Get-DriftMinutes -DesiredUtc $repeatingTask.DesiredRunUtc -ActualUtc $snapshot.nextRunUtc
    if ((-not $snapshot.registered) -or
        [string]::Equals([string] $snapshot.state, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase) -or
        $null -eq $snapshot.nextRunUtc -or
        $driftMinutes -gt $driftToleranceMinutes) {
        Invoke-InstallScript -ScriptPath $repeatingTask.InstallScriptPath -RepoRoot $resolvedRepoRoot -TaskName $repeatingTask.TaskName -Configuration $Configuration -DesiredStartUtc $repeatingTask.DesiredRunUtc
        [void] $actionsTaken.Add(('reconciled:{0}' -f $repeatingTask.TaskName))
    }
}

if ($shouldArmMainWorker) {
    $mainDriftMinutes = Get-DriftMinutes -DesiredUtc $desiredMainWorkerWakeUtc -ActualUtc $mainWorkerBefore.nextRunUtc
    if ((-not $mainWorkerBefore.registered) -or
        [string]::Equals([string] $mainWorkerBefore.state, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase) -or
        $null -eq $mainWorkerBefore.nextRunUtc -or
        $mainDriftMinutes -gt $driftToleranceMinutes) {
        Invoke-InstallScript -ScriptPath $installMainWorkerScriptPath -RepoRoot $resolvedRepoRoot -TaskName $mainWorkerTaskName -Configuration $Configuration -DesiredStartUtc $desiredMainWorkerWakeUtc
        [void] $actionsTaken.Add(('rearmed:{0}' -f $mainWorkerTaskName))
    }
} else {
    if ($mainWorkerBefore.registered -and -not [string]::Equals([string] $mainWorkerBefore.state, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase)) {
        try {
            Disable-ScheduledTask -TaskName $mainWorkerTaskName -ErrorAction Stop | Out-Null
            [void] $actionsTaken.Add(('disabled:{0}' -f $mainWorkerTaskName))
        }
        catch {
        }
    }
}

$mainWorkerAfter = Get-TaskSnapshot -TaskName $mainWorkerTaskName
$watchdogAfter = Get-TaskSnapshot -TaskName $watchdogTaskName
$reportConsumptionAfter = Get-TaskSnapshot -TaskName $reportConsumptionTaskName
$dailyDigestAfter = Get-TaskSnapshot -TaskName $dailyDigestTaskName

$mainWorkerAligned = $false
if ($shouldArmMainWorker) {
    $finalMainDriftMinutes = Get-DriftMinutes -DesiredUtc $desiredMainWorkerWakeUtc -ActualUtc $mainWorkerAfter.nextRunUtc
    $mainWorkerAligned = $mainWorkerAfter.registered -and
        (-not [string]::Equals([string] $mainWorkerAfter.state, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase)) -and
        ($null -ne $mainWorkerAfter.nextRunUtc) -and
        ($finalMainDriftMinutes -le $driftToleranceMinutes)
    $finalMainWorkerArmState = if ($mainWorkerAligned) { 'armed' } else { 'drift-detected' }
} else {
    $mainWorkerAligned = ($null -eq $mainWorkerAfter.nextRunUtc)
    if ($mainWorkerTerminalState -eq 'pause-hitl') {
        $finalMainWorkerArmState = 'paused-hitl'
    } elseif ($mainWorkerTerminalState -eq 'done') {
        $finalMainWorkerArmState = 'done-retired'
    } elseif ($mainWorkerTerminalState -eq 'fault-recoverable' -or $cycleState.lastKnownStatus -eq $blockedStatus) {
        $finalMainWorkerArmState = 'fault-recoverable'
    }
}

$watchdogAligned = $watchdogAfter.registered -and ($null -ne $watchdogAfter.nextRunUtc)
$reportConsumptionAligned = $reportConsumptionAfter.registered -and ($null -ne $reportConsumptionAfter.nextRunUtc)
$dailyDigestAligned = $dailyDigestAfter.registered -and ($null -ne $dailyDigestAfter.nextRunUtc)

$finalMainWorkerWakeUtcString = if ($mainWorkerAligned -and $null -ne $mainWorkerAfter.nextRunUtc) { $mainWorkerAfter.nextRunUtc.ToString('o') } else { $null }
$finalWatchdogRunUtcString = if ($watchdogAligned -and $null -ne $watchdogAfter.nextRunUtc) { $watchdogAfter.nextRunUtc.ToString('o') } else { $null }
$finalReportConsumptionRunUtcString = if ($reportConsumptionAligned -and $null -ne $reportConsumptionAfter.nextRunUtc) { $reportConsumptionAfter.nextRunUtc.ToString('o') } else { $null }
$finalDailyDigestRunUtcString = if ($dailyDigestAligned -and $null -ne $dailyDigestAfter.nextRunUtc) { $dailyDigestAfter.nextRunUtc.ToString('o') } else { $null }
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'mainWorkerTerminalState' -Value $mainWorkerTerminalState
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'mainWorkerArmState' -Value $finalMainWorkerArmState
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextMainWorkerWakeUtc' -Value $finalMainWorkerWakeUtcString
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextAutomationCycleRunUtc' -Value $finalMainWorkerWakeUtcString
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextWatchdogRunUtc' -Value $finalWatchdogRunUtcString
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextSourceBucketReportConsumptionRunUtc' -Value $finalReportConsumptionRunUtcString
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextDailyHitlDigestRunUtc' -Value $finalDailyDigestRunUtcString
Write-JsonFile -Path $cycleStatePath -Value $cycleState

$payload = [ordered]@{
    schemaVersion = 2
    generatedAtUtc = $nowUtc.ToString('o')
    actionTaken = if ($actionsTaken.Count -gt 0) { @($actionsTaken) } else { @('none') }
    driftToleranceMinutes = $driftToleranceMinutes
    aligned = ($mainWorkerAligned -and $watchdogAligned -and $reportConsumptionAligned -and $dailyDigestAligned)
    mainWorker = [ordered]@{
        taskName = $mainWorkerTaskName
        terminalState = $mainWorkerTerminalState
        desiredArmState = $desiredMainWorkerArmState
        finalArmState = $finalMainWorkerArmState
        desiredNextWakeUtc = if ($null -ne $desiredMainWorkerWakeUtc) { $desiredMainWorkerWakeUtc.ToString('o') } else { $null }
        finalNextWakeUtc = if ($null -ne $mainWorkerAfter.nextRunUtc) { $mainWorkerAfter.nextRunUtc.ToString('o') } else { $null }
        registeredBefore = $mainWorkerBefore.registered
        state = $mainWorkerAfter.state
        aligned = $mainWorkerAligned
    }
    watchdog = [ordered]@{
        taskName = $watchdogTaskName
        desiredNextRunUtc = if ($null -ne $desiredWatchdogRunUtc) { $desiredWatchdogRunUtc.ToString('o') } else { $null }
        finalNextRunUtc = if ($null -ne $watchdogAfter.nextRunUtc) { $watchdogAfter.nextRunUtc.ToString('o') } else { $null }
        registeredBefore = $watchdogBefore.registered
        state = $watchdogAfter.state
        aligned = $watchdogAligned
    }
    reportConsumption = [ordered]@{
        taskName = $reportConsumptionTaskName
        desiredNextRunUtc = if ($null -ne $desiredReportConsumptionRunUtc) { $desiredReportConsumptionRunUtc.ToString('o') } else { $null }
        finalNextRunUtc = if ($null -ne $reportConsumptionAfter.nextRunUtc) { $reportConsumptionAfter.nextRunUtc.ToString('o') } else { $null }
        registeredBefore = $reportConsumptionBefore.registered
        state = $reportConsumptionAfter.state
        aligned = $reportConsumptionAligned
    }
    dailyDigest = [ordered]@{
        taskName = $dailyDigestTaskName
        desiredNextRunUtc = if ($null -ne $desiredDigestRunUtc) { $desiredDigestRunUtc.ToString('o') } else { $null }
        finalNextRunUtc = if ($null -ne $dailyDigestAfter.nextRunUtc) { $dailyDigestAfter.nextRunUtc.ToString('o') } else { $null }
        registeredBefore = $dailyDigestBefore.registered
        state = $dailyDigestAfter.state
        aligned = $dailyDigestAligned
    }
}

Write-JsonFile -Path $schedulerStatePath -Value $payload
Write-Host ('[local-automation-scheduler-sync] State: {0}' -f $schedulerStatePath)
$schedulerStatePath
