param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-tasking.json'
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

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    if (Test-Path -LiteralPath $resolvedBase -PathType Leaf) {
        $resolvedBase = Split-Path -Parent $resolvedBase
    }

    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('\', '/')
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

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$activeRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$phaseWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessStatePath)
$windowBoundaryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormWindowBoundaryStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.autonomousLongFormCollapseOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.autonomousLongFormCollapseStatePath)
$runRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.longFormRunOutputRoot)

$activeRun = Read-JsonFileOrNull -Path $activeRunStatePath
$phaseWitnessState = Read-JsonFileOrNull -Path $phaseWitnessStatePath
$windowBoundaryState = Read-JsonFileOrNull -Path $windowBoundaryStatePath
if ($null -eq $activeRun -or $null -eq $phaseWitnessState -or $null -eq $windowBoundaryState) {
    throw 'Active run, phase witness, and window boundary state are required before autonomous long-form collapse can run.'
}

$targetPhaseId = [string] $phaseWitnessState.targetPhaseId
$targetPhaseLabel = [string] $phaseWitnessState.targetPhaseLabel
if ([string]::IsNullOrWhiteSpace($targetPhaseId)) {
    $targetPhaseId = [string] $activeRun.currentPhaseId
}
if ([string]::IsNullOrWhiteSpace($targetPhaseLabel)) {
    $targetPhaseLabel = [string] $activeRun.currentPhaseLabel
}

$collapseState = 'phase-updated'
$reasonCode = 'autonomous-long-form-collapse-phase-updated'
$nextAction = 'continue-active-run'
$runStatus = 'active'

if ([string] $windowBoundaryState.boundaryState -eq 'window-ended' -and $targetPhaseId -ne 'structure-04') {
    $targetPhaseId = 'structure-04'
    $targetPhaseLabel = 'Final Collapsed Structure 4'
    $collapseState = 'window-edge-collapse'
    $reasonCode = 'autonomous-long-form-collapse-window-edge'
    $nextAction = 'start-next-map-when-declared'
    $runStatus = 'collapsed'
} elseif ([string] $phaseWitnessState.targetPhaseId -eq 'structure-04') {
    $collapseState = 'proof-collapse'
    $reasonCode = 'autonomous-long-form-collapse-proof'
    $nextAction = 'start-next-map-when-declared'
    $runStatus = 'collapsed'
}

$updatedPhases = @()
foreach ($phase in @($activeRun.phases)) {
    $phaseId = [string] $phase.id
    $status = 'queued'
    $targetOrdinal = [int] $targetPhaseId.Substring($targetPhaseId.Length - 2)
    $phaseOrdinal = [int] $phase.ordinal

    if ($runStatus -eq 'collapsed' -and $phaseId -eq 'structure-04') {
        $status = 'completed'
    } elseif ($phaseOrdinal -lt $targetOrdinal) {
        $status = 'completed'
    } elseif ($phaseId -eq $targetPhaseId) {
        $status = if ($runStatus -eq 'collapsed') { 'completed' } else { 'active' }
    }

    $updatedPhases += [ordered]@{
        id = $phaseId
        ordinal = [int] $phase.ordinal
        kind = [string] $phase.kind
        label = [string] $phase.label
        status = $status
    }
}

$activeRun.currentPhaseId = $targetPhaseId
$activeRun.currentPhaseLabel = $targetPhaseLabel
$activeRun.runStatus = $runStatus
$activeRun.generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
$activeRun.phases = $updatedPhases

$runPath = Join-Path $runRoot ([string] $activeRun.runId)
$runJsonPath = Join-Path $runPath 'task-map-run.json'
$runMarkdownPath = Join-Path $runPath 'task-map-run.md'

Write-JsonFile -Path $runJsonPath -Value $activeRun
Write-JsonFile -Path $activeRunStatePath -Value $activeRun

$markdownLines = @(
    '# Long-Form Task Map Run',
    '',
    ('- Run ID: `{0}`' -f [string] $activeRun.runId),
    ('- Map: `{0}`' -f [string] $activeRun.mapLabel),
    ('- Run status: `{0}`' -f [string] $activeRun.runStatus),
    ('- Current phase: `{0}`' -f [string] $activeRun.currentPhaseLabel),
    ('- Window start (UTC): `{0}`' -f [string] $activeRun.timeframe.startUtc),
    ('- Window end (UTC): `{0}`' -f [string] $activeRun.timeframe.endUtc),
    ('- Iteration law: `{0}`' -f [string] $activeRun.iterationLaw.rule),
    ''
)

$markdownLines += @(
    '## Selected Tasks',
    '',
    '| Task | Owner | Status |',
    '| --- | --- | --- |'
)

foreach ($task in @($activeRun.selectedTasks)) {
    $markdownLines += ('| {0} | {1} | {2} |' -f [string] $task.label, [string] $task.owner, [string] $task.status)
}

$markdownLines += @(
    '',
    '## Phases',
    '',
    '| Phase | Kind | Status |',
    '| --- | --- | --- |'
)

foreach ($phase in $updatedPhases) {
    $markdownLines += ('| {0} | {1} | {2} |' -f [string] $phase.label, [string] $phase.kind, [string] $phase.status)
}

Set-Content -LiteralPath $runMarkdownPath -Value $markdownLines -Encoding utf8

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, [string] $activeRun.runId)
$bundleJsonPath = Join-Path $bundlePath 'autonomous-long-form-collapse.json'
$bundleMarkdownPath = Join-Path $bundlePath 'autonomous-long-form-collapse.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    collapseState = $collapseState
    reasonCode = $reasonCode
    nextAction = $nextAction
    runId = [string] $activeRun.runId
    runStatus = [string] $activeRun.runStatus
    currentPhaseId = [string] $activeRun.currentPhaseId
    currentPhaseLabel = [string] $activeRun.currentPhaseLabel
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$bundleMarkdownLines = @(
    '# Autonomous Long-Form Collapse',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Collapse state: `{0}`' -f $payload.collapseState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Run ID: `{0}`' -f $payload.runId),
    ('- Run status: `{0}`' -f $payload.runStatus),
    ('- Current phase: `{0}`' -f $payload.currentPhaseLabel)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $bundleMarkdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    collapseState = $payload.collapseState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    runStatus = $payload.runStatus
    currentPhaseId = $payload.currentPhaseId
    currentPhaseLabel = $payload.currentPhaseLabel
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[autonomous-long-form-collapse] Bundle: {0}' -f $bundlePath)
$bundlePath
