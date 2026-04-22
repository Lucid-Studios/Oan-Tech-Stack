param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-tasking.json'
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
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$activeRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$phaseWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormWindowBoundaryOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormWindowBoundaryStatePath)

$activeRun = Read-JsonFileOrNull -Path $activeRunStatePath
$phaseWitnessState = Read-JsonFileOrNull -Path $phaseWitnessStatePath
if ($null -eq $activeRun) {
    throw 'Active long-form run state is required before long-form window boundary receipt can run.'
}

$nowUtc = (Get-Date).ToUniversalTime()
$windowEndUtc = Get-OptionalDateTimeUtc -Value $activeRun.timeframe.endUtc
$boundaryState = 'candidate-only'
$reasonCode = 'long-form-window-boundary-candidate-only'
$nextAction = 'continue-candidate-automation'
$minutesRemaining = $null

if ($null -eq $windowEndUtc) {
    $boundaryState = 'window-missing'
    $reasonCode = 'long-form-window-boundary-window-missing'
    $nextAction = 'repair-active-run-window'
} else {
    $minutesRemaining = [math]::Round(($windowEndUtc - $nowUtc).TotalMinutes, 2)
    if ($minutesRemaining -le 0) {
        $boundaryState = 'window-ended'
        $reasonCode = 'long-form-window-boundary-window-ended'
        $nextAction = 'collapse-active-run-now'
    } elseif ($minutesRemaining -le 60) {
        $boundaryState = 'window-nearing-edge'
        $reasonCode = 'long-form-window-boundary-nearing-edge'
        $nextAction = 'prepare-final-collapse'
    } else {
        $boundaryState = 'within-window'
        $reasonCode = 'long-form-window-boundary-within-window'
        $nextAction = 'continue-exploratory-structures'
    }
}

if ($null -ne $phaseWitnessState -and [string] $phaseWitnessState.targetPhaseId -eq 'structure-04') {
    $boundaryState = 'ready-for-final-collapse'
    $reasonCode = 'long-form-window-boundary-ready-for-final-collapse'
    $nextAction = 'collapse-active-run'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$runKey = if (-not [string]::IsNullOrWhiteSpace([string] $activeRun.runId)) { [string] $activeRun.runId } else { 'no-run' }
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $runKey)
$bundleJsonPath = Join-Path $bundlePath 'long-form-window-boundary.json'
$bundleMarkdownPath = Join-Path $bundlePath 'long-form-window-boundary.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    boundaryState = $boundaryState
    reasonCode = $reasonCode
    nextAction = $nextAction
    activeRunId = [string] $activeRun.runId
    windowEndUtc = if ($null -ne $windowEndUtc) { $windowEndUtc.ToString('o') } else { $null }
    minutesRemaining = $minutesRemaining
    targetPhaseId = if ($null -ne $phaseWitnessState) { [string] $phaseWitnessState.targetPhaseId } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Long-Form Window Boundary Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Boundary state: `{0}`' -f $payload.boundaryState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Active run: `{0}`' -f $payload.activeRunId),
    ('- Window end (UTC): `{0}`' -f $(if ($payload.windowEndUtc) { $payload.windowEndUtc } else { 'missing' })),
    ('- Minutes remaining: `{0}`' -f $(if ($null -ne $payload.minutesRemaining) { $payload.minutesRemaining } else { 'unknown' })),
    ('- Target phase ID: `{0}`' -f $(if ($payload.targetPhaseId) { $payload.targetPhaseId } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    boundaryState = $payload.boundaryState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    minutesRemaining = $payload.minutesRemaining
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[long-form-window-boundary] Bundle: {0}' -f $bundlePath)
$bundlePath
