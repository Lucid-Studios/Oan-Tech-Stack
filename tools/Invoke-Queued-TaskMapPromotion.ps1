param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-tasking.json',
    [string] $ActiveTaskMapStatePath = '.audit/state/local-automation-active-task-map-selection.json'
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
$resolvedActiveTaskMapStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $ActiveTaskMapStatePath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json
$activeTaskMapState = Read-JsonFileOrNull -Path $resolvedActiveTaskMapStatePath

$activeRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.queuedTaskMapPromotionOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.queuedTaskMapPromotionStatePath)
$startTaskMapRunScriptPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'tools/Start-Local-Automation-TaskMapRun.ps1'

$activeRun = Read-JsonFileOrNull -Path $activeRunStatePath
$longFormTaskMaps = @($taskingPolicy.longFormTaskMaps)
$activeTaskMapId = if ($null -ne $activeTaskMapState -and -not [string]::IsNullOrWhiteSpace([string] $activeTaskMapState.activeTaskMapId)) {
    [string] $activeTaskMapState.activeTaskMapId
} else {
    [string] $taskingPolicy.activeTaskMapId
}
$activeTaskMap = @($longFormTaskMaps | Where-Object { [string] $_.id -eq $activeTaskMapId } | Select-Object -First 1)
if ($activeTaskMap -is [System.Array]) {
    $activeTaskMap = if ($activeTaskMap.Count -gt 0) { $activeTaskMap[0] } else { $null }
}

$nextTaskMap = $null
if ($null -ne $activeTaskMap) {
    $taskMapIndex = [array]::IndexOf($longFormTaskMaps, $activeTaskMap)
    if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $longFormTaskMaps.Count) {
        $nextTaskMap = $longFormTaskMaps[$taskMapIndex + 1]
    }
}

$promotionState = 'candidate-only'
$reasonCode = 'queued-task-map-promotion-candidate-only'
$nextAction = 'continue-candidate-automation'
$promoted = $false
$newRunStatePath = $null

if ($null -eq $activeTaskMap -or $null -eq $activeRun) {
    $promotionState = 'awaiting-evidence'
    $reasonCode = 'queued-task-map-promotion-evidence-missing'
    $nextAction = 'complete-long-form-promotion-prerequisites'
} elseif ($null -eq $nextTaskMap) {
    $promotionState = 'no-next-map-declared'
    $reasonCode = 'queued-task-map-promotion-next-map-missing'
    $nextAction = 'declare-the-next-task-map'
} elseif ([string] $activeRun.runStatus -ne 'collapsed') {
    $promotionState = 'awaiting-current-collapse'
    $reasonCode = 'queued-task-map-promotion-run-still-active'
    $nextAction = 'allow-current-run-to-collapse'
} else {
    $activeTaskMapStatePayload = [ordered]@{
        schemaVersion = 1
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        activeTaskMapId = [string] $nextTaskMap.id
        previousTaskMapId = [string] $activeTaskMap.id
        source = 'queued-task-map-promotion'
    }
    Write-JsonFile -Path $resolvedActiveTaskMapStatePath -Value $activeTaskMapStatePayload

    $startOutput = & powershell -ExecutionPolicy Bypass -File $startTaskMapRunScriptPath -RepoRoot $resolvedRepoRoot -TaskingPolicyPath ([string] $TaskingPolicyPath) -ActiveTaskMapStatePath ([string] $ActiveTaskMapStatePath)
    if ($LASTEXITCODE -ne 0) {
        throw 'Queued task map promotion could not start the next task map run.'
    }

    $newRunStatePath = @($startOutput | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
    $promotionState = 'promoted-to-next-map'
    $reasonCode = 'queued-task-map-promotion-promoted'
    $nextAction = 'continue-next-map-automation'
    $promoted = $true
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundlePath = Join-Path $outputRoot $timestamp
$bundleJsonPath = Join-Path $bundlePath 'queued-task-map-promotion.json'
$bundleMarkdownPath = Join-Path $bundlePath 'queued-task-map-promotion.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    promotionState = $promotionState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sourceTaskMapId = if ($null -ne $activeTaskMap) { [string] $activeTaskMap.id } else { $null }
    targetTaskMapId = if ($null -ne $nextTaskMap) { [string] $nextTaskMap.id } else { $null }
    promoted = $promoted
    nextRunStatePath = if (-not [string]::IsNullOrWhiteSpace([string] $newRunStatePath)) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath ([string] $newRunStatePath) } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Queued Task Map Promotion',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Promotion state: `{0}`' -f $payload.promotionState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Source task map: `{0}`' -f $(if ($payload.sourceTaskMapId) { $payload.sourceTaskMapId } else { 'missing' })),
    ('- Target task map: `{0}`' -f $(if ($payload.targetTaskMapId) { $payload.targetTaskMapId } else { 'missing' })),
    ('- Promoted: `{0}`' -f $payload.promoted),
    ('- Next run state path: `{0}`' -f $(if ($payload.nextRunStatePath) { $payload.nextRunStatePath } else { 'none' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    promotionState = $payload.promotionState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    sourceTaskMapId = $payload.sourceTaskMapId
    targetTaskMapId = $payload.targetTaskMapId
    promoted = $payload.promoted
    nextRunStatePath = $payload.nextRunStatePath
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[queued-task-map-promotion] Bundle: {0}' -f $bundlePath)
$bundlePath
