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

$taskEntries = @(
    [ordered]@{
        id = 'release-candidate-cycle'
        label = 'Release Candidate Cycle'
        status = $releaseCandidateTaskStatus
        lastRunUtc = if ($null -ne $lastReleaseCandidateRunUtc) { $lastReleaseCandidateRunUtc.ToString('o') } else { $null }
        nextRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
        latestBundle = $lastReleaseCandidateBundle
    },
    [ordered]@{
        id = 'daily-hitl-digest'
        label = 'Daily HITL Digest'
        status = $dailyDigestTaskStatus
        lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
        nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
        latestBundle = $lastDigestBundle
    },
    [ordered]@{
        id = 'promotion-watch'
        label = 'Promotion Watch'
        status = $promotionWatchStatus
        lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
        nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
        latestBundle = $lastDigestBundle
    }
)

$statusPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    cyclePolicyPath = $resolvedCyclePolicyPath
    taskingPolicyPath = $resolvedTaskingPolicyPath
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

$markdownLines += @(
    '## Tasks',
    '',
    '| Task | Status | Last Run (UTC) | Next Run (UTC) |',
    '| --- | --- | --- | --- |'
)

foreach ($task in $taskEntries) {
    $markdownLines += ('| {0} | {1} | {2} | {3} |' -f $task.label, $task.status, $(if ($task.lastRunUtc) { $task.lastRunUtc } else { 'not-yet-run' }), $(if ($task.nextRunUtc) { $task.nextRunUtc } else { 'not-scheduled' }))
}

Set-Content -LiteralPath $statusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[local-automation-status] JSON: {0}' -f $statusJsonPath)
$statusJsonPath
