param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.0/build/local-automation-tasking.json'
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

function Read-JsonFile {
    param([string] $Path)

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

function Get-MeaningfulScheduledDateTimeUtcStringOrNull {
    param([datetime] $Value)

    $utcValue = $Value.ToUniversalTime()
    if ($utcValue -le [datetime]'2000-01-01T00:00:00Z') {
        return $null
    }

    return $utcValue.ToString('o')
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Read-JsonFile -Path $resolvedCyclePolicyPath
$taskingPolicy = Read-JsonFile -Path $resolvedTaskingPolicyPath
$taskDefinitions = @($taskingPolicy.tasks)
$longFormTaskMaps = @($taskingPolicy.longFormTaskMaps)
$activeTaskMapId = [string] $taskingPolicy.activeTaskMapId
$activeLongFormRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$activeLongFormRun = $null
if (Test-Path -LiteralPath $activeLongFormRunStatePath -PathType Leaf) {
    $activeLongFormRun = Read-JsonFile -Path $activeLongFormRunStatePath
}

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$statusJsonPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.statusJsonPath)
$statusMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.statusMarkdownPath)
$nowUtc = (Get-Date).ToUniversalTime()

$cycleState = $null
if (Test-Path -LiteralPath $cycleStatePath -PathType Leaf) {
    $cycleState = Read-JsonFile -Path $cycleStatePath
}

$lastKnownStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastKnownStatus')
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastReleaseCandidateRunUtc')
$nextReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextReleaseCandidateRunUtc')
$lastDigestUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestUtc')
$nextMandatoryHitlReviewUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextMandatoryHitlReviewUtc')
$lastReleaseCandidateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastReleaseCandidateBundle')
$lastDigestBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestBundle')

$digestJson = $null
if (-not [string]::IsNullOrWhiteSpace($lastDigestBundle)) {
    $digestJsonPath = Join-Path $lastDigestBundle 'release-candidate-digest.json'
    if (Test-Path -LiteralPath $digestJsonPath -PathType Leaf) {
        $digestJson = Read-JsonFile -Path $digestJsonPath
    }
}

$scheduler = [ordered]@{
    taskName = [string] $taskingPolicy.scheduledTaskName
    registered = $false
    state = 'not-registered'
    lastRunTimeUtc = $null
    nextRunTimeUtc = $null
}

if (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue) {
    try {
        $scheduledTask = Get-ScheduledTask -TaskName ([string] $taskingPolicy.scheduledTaskName) -ErrorAction Stop
        $scheduledInfo = Get-ScheduledTaskInfo -TaskName ([string] $taskingPolicy.scheduledTaskName) -ErrorAction Stop
        $scheduler.registered = $true
        $scheduler.state = [string] $scheduledTask.State
        $scheduler.lastRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.LastRunTime)
        $scheduler.nextRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.NextRunTime)
    } catch {
    }
}

$releaseCandidateTaskStatus = if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    'blocked'
} elseif ($null -eq $nextReleaseCandidateRunUtc) {
    'uninitialized'
} elseif ($nextReleaseCandidateRunUtc -le $nowUtc) {
    'due'
} else {
    'waiting-for-cadence'
}

$dailyDigestTaskStatus = if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    'review-now-blocked'
} elseif ($null -eq $nextMandatoryHitlReviewUtc) {
    'uninitialized'
} elseif ($nextMandatoryHitlReviewUtc -le $nowUtc) {
    'review-due'
} else {
    'waiting-for-daily-review'
}

$promotionWatchStatus = 'clear-to-continue'
$recommendedAction = $null
$requiresImmediateHitl = $false
if ($null -ne $digestJson) {
    $recommendedAction = [string] $digestJson.recommendedAction
    $requiresImmediateHitl = [bool] $digestJson.requiresImmediateHitl
}

if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $promotionWatchStatus = 'blocked'
} elseif ($requiresImmediateHitl -or $lastKnownStatus -eq 'hitl-required') {
    $promotionWatchStatus = 'hitl-required'
}

$schedulerWatchStatus = if (-not [bool] $scheduler.registered) {
    'scheduler-unregistered'
} elseif ([string]::IsNullOrWhiteSpace([string] $scheduler.nextRunTimeUtc)) {
    'scheduler-missing-next-run'
} else {
    'scheduler-ready'
}

$taskEntries = @(
    foreach ($taskDefinition in $taskDefinitions) {
        $taskId = [string] $taskDefinition.id
        $status = 'uninitialized'
        $lastRunUtc = $null
        $nextRunUtc = $null
        $latestBundle = $null

        switch ($taskId) {
            'release-candidate-cycle' {
                $status = $releaseCandidateTaskStatus
                $lastRunUtc = if ($null -ne $lastReleaseCandidateRunUtc) { $lastReleaseCandidateRunUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
                $latestBundle = $lastReleaseCandidateBundle
            }
            'daily-hitl-digest' {
                $status = $dailyDigestTaskStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
                $latestBundle = $lastDigestBundle
            }
            'promotion-watch' {
                $status = $promotionWatchStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
                $latestBundle = $lastDigestBundle
            }
            'scheduler-watch' {
                $status = $schedulerWatchStatus
                $lastRunUtc = [string] $scheduler.lastRunTimeUtc
                $nextRunUtc = [string] $scheduler.nextRunTimeUtc
                $latestBundle = [string] $scheduler.taskName
            }
        }

        [ordered]@{
            id = $taskId
            label = [string] $taskDefinition.label
            taskClass = [string] $taskDefinition.taskClass
            owner = [string] $taskDefinition.owner
            authority = [string] $taskDefinition.authority
            purpose = [string] $taskDefinition.purpose
            completionSignal = [string] $taskDefinition.completionSignal
            outputs = @($taskDefinition.outputs | ForEach-Object { [string] $_ })
            escalatesWhen = @($taskDefinition.escalatesWhen | ForEach-Object { [string] $_ })
            status = $status
            lastRunUtc = $lastRunUtc
            nextRunUtc = $nextRunUtc
            latestBundle = $latestBundle
        }
    }
)

$activeLongFormTaskMap = $null
if (-not [string]::IsNullOrWhiteSpace($activeTaskMapId)) {
    $activeLongFormTaskMap = @($longFormTaskMaps | Where-Object { [string] $_.id -eq $activeTaskMapId } | Select-Object -First 1)
    if ($activeLongFormTaskMap -is [System.Array]) {
        $activeLongFormTaskMap = if ($activeLongFormTaskMap.Count -gt 0) { $activeLongFormTaskMap[0] } else { $null }
    }
}

$activeLongFormTaskMapStatus = 'uninitialized'
$activeLongFormTasksCompleted = 0
$activeLongFormTasksTotal = 0
$canPullForwardFromNextMap = $false
$eligibleNextTaskMap = $null

if ($null -ne $activeLongFormTaskMap) {
    $activeLongFormTasks = @($activeLongFormTaskMap.tasks)
    $activeLongFormTasksTotal = $activeLongFormTasks.Count
    $activeLongFormTasksCompleted = @($activeLongFormTasks | Where-Object { [string] $_.status -eq 'completed' }).Count

    if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
        $activeLongFormTaskMapStatus = 'blocked'
    } elseif ($requiresImmediateHitl -or $lastKnownStatus -eq 'hitl-required') {
        $activeLongFormTaskMapStatus = 'waiting-for-hitl'
    } elseif ($activeLongFormTasksTotal -gt 0 -and $activeLongFormTasksCompleted -ge $activeLongFormTasksTotal) {
        $activeLongFormTaskMapStatus = 'completed'
    } else {
        $activeLongFormTaskMapStatus = 'in-progress'
    }

    $taskMapIndex = [array]::IndexOf($longFormTaskMaps, $activeLongFormTaskMap)
    if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $longFormTaskMaps.Count) {
        $eligibleNextTaskMap = $longFormTaskMaps[$taskMapIndex + 1]
    }

    $timeDilationPolicy = $taskingPolicy.PSObject.Properties['timeDilationPolicy']
    $allowPullForward = $false
    $pullForwardMaxMaps = 0
    if ($null -ne $timeDilationPolicy) {
        $allowPullForward = [bool] $timeDilationPolicy.Value.allowPullForward
        $pullForwardMaxMaps = [int] $timeDilationPolicy.Value.pullForwardMaxMaps
    }

    if ($allowPullForward -and $pullForwardMaxMaps -ge 1 -and
        $activeLongFormTaskMapStatus -eq 'completed' -and
        $null -ne $eligibleNextTaskMap) {
        $canPullForwardFromNextMap = $true
    }
}

$taskMapEntries = @(
    foreach ($taskMap in $longFormTaskMaps) {
        $taskMapTasks = @($taskMap.tasks)
        $completedCount = @($taskMapTasks | Where-Object { [string] $_.status -eq 'completed' }).Count
        [ordered]@{
            id = [string] $taskMap.id
            label = [string] $taskMap.label
            status = [string] $taskMap.status
            expectedReviewWindows = [int] $taskMap.expectedReviewWindows
            goal = [string] $taskMap.goal
            completedTaskCount = $completedCount
            totalTaskCount = $taskMapTasks.Count
            taskIds = @($taskMap.taskIds | ForEach-Object { [string] $_ })
            tasks = @(
                $taskMapTasks |
                ForEach-Object {
                    [ordered]@{
                        id = [string] $_.id
                        label = [string] $_.label
                        owner = [string] $_.owner
                        authority = [string] $_.authority
                        status = [string] $_.status
                        purpose = [string] $_.purpose
                        completionSignal = [string] $_.completionSignal
                        escalatesWhen = @($_.escalatesWhen | ForEach-Object { [string] $_ })
                    }
                }
            )
        }
    }
)

$statusPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    cyclePolicyPath = $resolvedCyclePolicyPath
    taskingPolicyPath = $resolvedTaskingPolicyPath
    formalSurfaceMarkdownPath = [string] $taskingPolicy.formalSurfaceMarkdownPath
    longFormTasking = [ordered]@{
        activeTaskMapId = $activeTaskMapId
        activeTaskMapStatus = $activeLongFormTaskMapStatus
        activeTaskMapCompletedTaskCount = $activeLongFormTasksCompleted
        activeTaskMapTotalTaskCount = $activeLongFormTasksTotal
        canPullForwardFromNextMap = $canPullForwardFromNextMap
        eligibleNextTaskMapId = if ($null -ne $eligibleNextTaskMap) { [string] $eligibleNextTaskMap.id } else { $null }
        pullForwardRule = [string] $taskingPolicy.timeDilationPolicy.rule
        activeRunStatePath = $activeLongFormRunStatePath
        activeRun = if ($null -ne $activeLongFormRun) {
            [ordered]@{
                runId = [string] $activeLongFormRun.runId
                runStatus = [string] $activeLongFormRun.runStatus
                currentPhaseId = [string] $activeLongFormRun.currentPhaseId
                currentPhaseLabel = [string] $activeLongFormRun.currentPhaseLabel
                windowEndUtc = [string] $activeLongFormRun.timeframe.endUtc
                iterationLaw = $activeLongFormRun.iterationLaw
            }
        } else {
            $null
        }
        taskMaps = $taskMapEntries
    }
    scheduler = $scheduler
    currentPosture = [ordered]@{
        lastKnownStatus = $lastKnownStatus
        recommendedAction = $recommendedAction
        requiresImmediateHitl = $requiresImmediateHitl
        lastReleaseCandidateBundle = $lastReleaseCandidateBundle
        lastDigestBundle = $lastDigestBundle
        nextReleaseCandidateRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
        nextMandatoryHitlReviewUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
    }
    tasks = $taskEntries
}

Write-JsonFile -Path $statusJsonPath -Value $statusPayload

$markdownLines = @(
    '# Local Automation Tasking Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $statusPayload.generatedAtUtc),
    ('- Formal tasking surface: `{0}`' -f $statusPayload.formalSurfaceMarkdownPath),
    ('- Active long-form task map: `{0}`' -f $activeTaskMapId),
    ('- Active map posture: `{0}`' -f $activeLongFormTaskMapStatus),
    ('- Pull-forward allowed from next map: `{0}`' -f $canPullForwardFromNextMap),
    ('- Scheduler task: `{0}`' -f $scheduler.taskName),
    ('- Scheduler registered: `{0}`' -f $scheduler.registered),
    ('- Scheduler state: `{0}`' -f $scheduler.state),
    ('- Current posture: `{0}`' -f $lastKnownStatus),
    ('- Recommended action: `{0}`' -f $recommendedAction),
    ('- Requires immediate HITL: `{0}`' -f $requiresImmediateHitl),
    ('- Next release-candidate run (UTC): `{0}`' -f $(if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { 'uninitialized' })),
    ('- Next mandatory HITL review (UTC): `{0}`' -f $(if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { 'uninitialized' })),
    ''
)

if ($scheduler.registered) {
    $markdownLines += @(
        '## Scheduler',
        '',
        ('- Last scheduled run (UTC): `{0}`' -f $(if ($scheduler.lastRunTimeUtc) { $scheduler.lastRunTimeUtc } else { 'not-yet-run' })),
        ('- Next scheduled run (UTC): `{0}`' -f $(if ($scheduler.nextRunTimeUtc) { $scheduler.nextRunTimeUtc } else { 'not-available' })),
        ''
    )
}

if ($null -ne $activeLongFormTaskMap) {
    $markdownLines += @(
        '## Long-Form Task Map',
        '',
        ('- Active map: `{0}`' -f $activeLongFormTaskMap.label),
        ('- Goal: `{0}`' -f $activeLongFormTaskMap.goal),
        ('- Expected review windows: `{0}`' -f $activeLongFormTaskMap.expectedReviewWindows),
        ('- Completed tasks: `{0}/{1}`' -f $activeLongFormTasksCompleted, $activeLongFormTasksTotal),
        ('- Pull-forward rule: `{0}`' -f [string] $taskingPolicy.timeDilationPolicy.rule)
    )

    if ($null -ne $eligibleNextTaskMap) {
        $markdownLines += ('- Eligible next map: `{0}`' -f [string] $eligibleNextTaskMap.label)
    }

    $markdownLines += @(
        '',
        '| Task | Owner | Status |',
        '| --- | --- | --- |'
    )

    foreach ($task in @($activeLongFormTaskMap.tasks)) {
        $markdownLines += ('| {0} | {1} | {2} |' -f [string] $task.label, [string] $task.owner, [string] $task.status)
    }

    if ($null -ne $activeLongFormRun) {
        $markdownLines += @(
            '',
            '## Active Long-Form Run',
            '',
            ('- Run ID: `{0}`' -f [string] $activeLongFormRun.runId),
            ('- Run status: `{0}`' -f [string] $activeLongFormRun.runStatus),
            ('- Current phase: `{0}`' -f [string] $activeLongFormRun.currentPhaseLabel),
            ('- Window end (UTC): `{0}`' -f [string] $activeLongFormRun.timeframe.endUtc),
            ('- Iteration law: `{0}`' -f [string] $activeLongFormRun.iterationLaw.rule)
        )
    }
}

$markdownLines += @(
    '## Tasks',
    '',
    '| Task | Owner | Status | Last Run (UTC) | Next Run (UTC) |',
    '| --- | --- | --- | --- | --- |'
)

foreach ($task in $taskEntries) {
    $markdownLines += ('| {0} | {1} | {2} | {3} | {4} |' -f $task.label, $task.owner, $task.status, $(if ($task.lastRunUtc) { $task.lastRunUtc } else { 'not-yet-run' }), $(if ($task.nextRunUtc) { $task.nextRunUtc } else { 'not-scheduled' }))
}

Set-Content -LiteralPath $statusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[local-automation-status] JSON: {0}' -f $statusJsonPath)
$statusJsonPath
