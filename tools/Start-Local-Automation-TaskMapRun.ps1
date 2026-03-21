param(
    [string] $RepoRoot,
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.0/build/local-automation-tasking.json',
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

    $Value | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $Path -Encoding utf8
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$resolvedCycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CycleStatePath
$taskingPolicy = Read-JsonFile -Path $resolvedTaskingPolicyPath
$cycleState = Read-JsonFile -Path $resolvedCycleStatePath

$activeTaskMapId = [string] $taskingPolicy.activeTaskMapId
$activeTaskMap = @($taskingPolicy.longFormTaskMaps | Where-Object { [string] $_.id -eq $activeTaskMapId } | Select-Object -First 1)
if ($activeTaskMap -is [System.Array]) {
    $activeTaskMap = if ($activeTaskMap.Count -gt 0) { $activeTaskMap[0] } else { $null }
}

if ($null -eq $activeTaskMap) {
    throw "Active long-form task map '$activeTaskMapId' is not declared."
}

$windowStartUtc = (Get-Date).ToUniversalTime()
$windowEndUtc = Get-OptionalDateTimeUtc -Value $cycleState.nextReleaseCandidateRunUtc
if ($null -eq $windowEndUtc -or $windowEndUtc -le $windowStartUtc) {
    $windowEndUtc = $windowStartUtc.AddHours(6)
}

$exploratoryStructures = [int] $taskingPolicy.longFormRunPolicy.exploratoryStructures
$finalCollapsedStructures = [int] $taskingPolicy.longFormRunPolicy.finalCollapsedStructures
$totalStructures = $exploratoryStructures + $finalCollapsedStructures
$runTimestamp = $windowStartUtc.ToString('yyyyMMddTHHmmssZ')
$runId = '{0}-{1}' -f $runTimestamp, $activeTaskMapId
$runRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.longFormRunOutputRoot)
$runPath = Join-Path $runRoot $runId
$runJsonPath = Join-Path $runPath 'task-map-run.json'
$runMarkdownPath = Join-Path $runPath 'task-map-run.md'
$runStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)

$eligibleNextTaskMap = $null
$taskMaps = @($taskingPolicy.longFormTaskMaps)
$taskMapIndex = [array]::IndexOf($taskMaps, $activeTaskMap)
if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $taskMaps.Count) {
    $eligibleNextTaskMap = $taskMaps[$taskMapIndex + 1]
}

$phaseEntries = @()
for ($index = 1; $index -le $exploratoryStructures; $index++) {
    $phaseEntries += [ordered]@{
        id = ('structure-{0:00}' -f $index)
        ordinal = $index
        kind = 'exploratory'
        label = ('Exploratory Structure {0}' -f $index)
        status = if ($index -eq 1) { 'active' } else { 'queued' }
    }
}

for ($index = 1; $index -le $finalCollapsedStructures; $index++) {
    $ordinal = $exploratoryStructures + $index
    $phaseEntries += [ordered]@{
        id = ('structure-{0:00}' -f $ordinal)
        ordinal = $ordinal
        kind = 'final-collapsed'
        label = ('Final Collapsed Structure {0}' -f $ordinal)
        status = 'queued'
    }
}

$selectedTasks = @(
    $activeTaskMap.tasks |
    ForEach-Object {
        [ordered]@{
            id = [string] $_.id
            label = [string] $_.label
            owner = [string] $_.owner
            authority = [string] $_.authority
            status = [string] $_.status
            purpose = [string] $_.purpose
            completionSignal = [string] $_.completionSignal
        }
    }
)

$runPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $windowStartUtc.ToString('o')
    runId = $runId
    mapId = [string] $activeTaskMap.id
    mapLabel = [string] $activeTaskMap.label
    runStatus = 'active'
    currentPhaseId = 'structure-01'
    currentPhaseLabel = 'Exploratory Structure 1'
    timeframe = [ordered]@{
        startUtc = $windowStartUtc.ToString('o')
        endUtc = $windowEndUtc.ToString('o')
        source = [string] $taskingPolicy.longFormRunPolicy.timeframeSource
    }
    iterationLaw = [ordered]@{
        exploratoryStructures = $exploratoryStructures
        finalCollapsedStructures = $finalCollapsedStructures
        totalStructures = $totalStructures
        collapseBeforeWindowEnd = [bool] $taskingPolicy.longFormRunPolicy.collapseBeforeWindowEnd
        rule = [string] $taskingPolicy.longFormRunPolicy.rule
    }
    pullForward = [ordered]@{
        allowFromNextMapOnly = [bool] $taskingPolicy.timeDilationPolicy.allowPullForward
        pullForwardMaxMaps = [int] $taskingPolicy.timeDilationPolicy.pullForwardMaxMaps
        eligibleNextTaskMapId = if ($null -ne $eligibleNextTaskMap) { [string] $eligibleNextTaskMap.id } else { $null }
        eligibleNextTaskMapLabel = if ($null -ne $eligibleNextTaskMap) { [string] $eligibleNextTaskMap.label } else { $null }
    }
    selectedTasks = $selectedTasks
    phases = $phaseEntries
}

Write-JsonFile -Path $runJsonPath -Value $runPayload
Write-JsonFile -Path $runStatePath -Value $runPayload

$markdownLines = @(
    '# Long-Form Task Map Run',
    '',
    ('- Run ID: `{0}`' -f $runId),
    ('- Map: `{0}`' -f $activeTaskMap.label),
    ('- Run status: `active`'),
    ('- Current phase: `Exploratory Structure 1`'),
    ('- Window start (UTC): `{0}`' -f $windowStartUtc.ToString('o')),
    ('- Window end (UTC): `{0}`' -f $windowEndUtc.ToString('o')),
    ('- Iteration law: `{0}`' -f [string] $taskingPolicy.longFormRunPolicy.rule)
)

if ($null -ne $eligibleNextTaskMap) {
    $markdownLines += ('- Eligible next map: `{0}`' -f [string] $eligibleNextTaskMap.label)
}

$markdownLines += @(
    '',
    '## Selected Tasks',
    '',
    '| Task | Owner | Status |',
    '| --- | --- | --- |'
)

foreach ($task in $selectedTasks) {
    $markdownLines += ('| {0} | {1} | {2} |' -f $task.label, $task.owner, $task.status)
}

$markdownLines += @(
    '',
    '## Phases',
    '',
    '| Phase | Kind | Status |',
    '| --- | --- | --- |'
)

foreach ($phase in $phaseEntries) {
    $markdownLines += ('| {0} | {1} | {2} |' -f $phase.label, $phase.kind, $phase.status)
}

Set-Content -LiteralPath $runMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[long-form-task-map-run] State: {0}' -f $runStatePath)
$runStatePath
