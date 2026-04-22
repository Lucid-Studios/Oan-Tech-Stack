param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-tasking.json',
    [string] $FormalSurfaceMarkdownPath = 'OAN Mortalis V1.1.1/docs/SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md'
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

    if (-not (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue)) {
        $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
            . $oanWorkspaceResolverPath
        }
    }

    if (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue) {
        return Resolve-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    }

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param([string] $BasePath, [string] $TargetPath)

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
    param([string] $Path, [object] $Value)

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-ObjectPropertyValueOrNull {
    param([object] $InputObject, [string] $PropertyName)

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$resolvedFormalSurfacePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $FormalSurfaceMarkdownPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$researchBriefStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedHorizonResearchBriefStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nextEraBatchSelectorOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nextEraBatchSelectorStatePath)
$formalSurfacePath = $resolvedFormalSurfacePath

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the next era batch selector writer can run.'
}

$researchBriefState = Read-JsonFileOrNull -Path $researchBriefStatePath
$formalSurfaceText = if (Test-Path -LiteralPath $formalSurfacePath -PathType Leaf) { Get-Content -Raw -LiteralPath $formalSurfacePath } else { '' }

$selectedNextMapId = 'automation-maturation-map-26'
$queuedTailMapId = 'automation-maturation-map-27'
$selectedCluster = 'chamber-native-inquiry-and-boundary-memory'

$selectionBoundedToDeclaredMaps =
    ($taskingPolicy.longFormTaskMaps.id -contains $selectedNextMapId) -and
    ($taskingPolicy.longFormTaskMaps.id -contains $queuedTailMapId) -and
    ($formalSurfaceText.IndexOf('### Automation Maturation Map 26', [System.StringComparison]::Ordinal) -ge 0) -and
    ($formalSurfaceText.IndexOf('### Automation Maturation Map 27', [System.StringComparison]::Ordinal) -ge 0)

$sourceFiles = @(
    $resolvedTaskingPolicyPath,
    $formalSurfacePath
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })

$currentResearchBriefState = [string] (Get-ObjectPropertyValueOrNull -InputObject $researchBriefState -PropertyName 'researchBriefState')
$selectorState = 'awaiting-bounded-horizon-research-brief'
$reasonCode = 'next-era-batch-selector-awaiting-bounded-horizon-research-brief'
$nextAction = 'emit-bounded-horizon-research-brief'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $selectorState = 'blocked'
    $reasonCode = 'next-era-batch-selector-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentResearchBriefState -ne 'bounded-horizon-research-brief-ready') {
    $selectorState = 'awaiting-bounded-horizon-research-brief'
    $reasonCode = 'next-era-batch-selector-research-brief-not-ready'
    $nextAction = if ($null -ne $researchBriefState) { [string] $researchBriefState.nextAction } else { 'emit-bounded-horizon-research-brief' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $selectionBoundedToDeclaredMaps) {
    $selectorState = 'awaiting-next-era-selection'
    $reasonCode = 'next-era-batch-selector-declarations-missing'
    $nextAction = 'declare-next-era-maps'
} else {
    $selectorState = 'next-era-batch-selector-ready'
    $reasonCode = 'next-era-batch-selector-bound'
    $nextAction = 'pull-forward-to-map-26'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'next-era-batch-selector.json'
$bundleMarkdownPath = Join-Path $bundlePath 'next-era-batch-selector.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    selectorState = $selectorState
    reasonCode = $reasonCode
    nextAction = $nextAction
    researchBriefState = $currentResearchBriefState
    selectedNextMapId = $selectedNextMapId
    selectedCluster = $selectedCluster
    queuedMapCount = 2
    queuedTailMapId = $queuedTailMapId
    selectionBoundedToDeclaredMaps = $selectionBoundedToDeclaredMaps
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $sourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Next Era Batch Selector',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Selector state: `{0}`' -f $payload.selectorState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Research-brief state: `{0}`' -f $(if ($payload.researchBriefState) { $payload.researchBriefState } else { 'missing' })),
    ('- Selected next map: `{0}`' -f $payload.selectedNextMapId),
    ('- Selected cluster: `{0}`' -f $payload.selectedCluster),
    ('- Queued map count: `{0}`' -f $payload.queuedMapCount),
    ('- Queued tail map: `{0}`' -f $payload.queuedTailMapId),
    ('- Selection bounded to declared maps: `{0}`' -f [bool] $payload.selectionBoundedToDeclaredMaps),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount),
    ''
)

foreach ($file in @($payload.sourceFiles)) {
    $markdownLines += @(
        ('## {0}' -f [string] $file.path),
        ('- Present: `{0}`' -f [bool] $file.present),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    selectorState = $payload.selectorState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    researchBriefState = $payload.researchBriefState
    selectedNextMapId = $payload.selectedNextMapId
    selectedCluster = $payload.selectedCluster
    queuedMapCount = $payload.queuedMapCount
    queuedTailMapId = $payload.queuedTailMapId
    selectionBoundedToDeclaredMaps = $payload.selectionBoundedToDeclaredMaps
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[next-era-batch-selector] Bundle: {0}' -f $bundlePath)
$bundlePath
