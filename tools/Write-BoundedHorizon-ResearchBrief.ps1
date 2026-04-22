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
$postHabitationHorizonLatticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postHabitationHorizonLatticeStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedHorizonResearchBriefOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedHorizonResearchBriefStatePath)
$formalSurfacePath = $resolvedFormalSurfacePath

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the bounded horizon research brief writer can run.'
}

$horizonLatticeState = Read-JsonFileOrNull -Path $postHabitationHorizonLatticeStatePath
$formalSurfaceText = if (Test-Path -LiteralPath $formalSurfacePath -PathType Leaf) { Get-Content -Raw -LiteralPath $formalSurfacePath } else { '' }

$inquirySignals = @(
    'inquiry-session-discipline-surface',
    'boundary-condition-ledger',
    'coherence-gain-witness-receipt'
)
$bondedSignals = @(
    'operator-inquiry-selection-envelope',
    'bonded-crucible-session-rehearsal',
    'shared-boundary-memory-ledger'
)

$inquiryFocusBound = $true
foreach ($signal in $inquirySignals) {
    if ($taskingPolicy.longFormTaskMaps.tasks.id -notcontains $signal -and $formalSurfaceText.IndexOf($signal, [System.StringComparison]::Ordinal) -lt 0) {
        $inquiryFocusBound = $false
        break
    }
}

$bondedFocusBound = $true
foreach ($signal in $bondedSignals) {
    if ($taskingPolicy.longFormTaskMaps.tasks.id -notcontains $signal -and $formalSurfaceText.IndexOf($signal, [System.StringComparison]::Ordinal) -lt 0) {
        $bondedFocusBound = $false
        break
    }
}

$sourceFiles = @(
    $resolvedTaskingPolicyPath,
    $formalSurfacePath
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })

$currentLatticeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $horizonLatticeState -PropertyName 'latticeState')
$researchBriefState = 'awaiting-horizon-lattice'
$reasonCode = 'bounded-horizon-research-brief-awaiting-horizon-lattice'
$nextAction = 'emit-post-habitation-horizon-lattice'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $researchBriefState = 'blocked'
    $reasonCode = 'bounded-horizon-research-brief-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentLatticeState -ne 'post-habitation-horizon-lattice-ready') {
    $researchBriefState = 'awaiting-horizon-lattice'
    $reasonCode = 'bounded-horizon-research-brief-lattice-not-ready'
    $nextAction = if ($null -ne $horizonLatticeState) { [string] $horizonLatticeState.nextAction } else { 'emit-post-habitation-horizon-lattice' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $inquiryFocusBound -or -not $bondedFocusBound) {
    $researchBriefState = 'awaiting-horizon-focus'
    $reasonCode = 'bounded-horizon-research-brief-focus-missing'
    $nextAction = 'declare-inquiry-and-bonded-horizon-focus'
} else {
    $researchBriefState = 'bounded-horizon-research-brief-ready'
    $reasonCode = 'bounded-horizon-research-brief-bound'
    $nextAction = 'emit-next-era-batch-selector'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bounded-horizon-research-brief.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bounded-horizon-research-brief.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    researchBriefState = $researchBriefState
    reasonCode = $reasonCode
    nextAction = $nextAction
    latticeState = $currentLatticeState
    primaryPressurePoint = 'chamber-native-inquiry-and-boundary-memory'
    queuedHorizonCount = 2
    withheldExpansionCount = 3
    bondedReleaseStillWithheld = $true
    publicationMaturityStillWithheld = $true
    mosBearingDepthStillWithheld = $true
    inquiryFocusBound = $inquiryFocusBound
    bondedFocusBound = $bondedFocusBound
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
    '# Bounded Horizon Research Brief',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Research-brief state: `{0}`' -f $payload.researchBriefState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Lattice state: `{0}`' -f $(if ($payload.latticeState) { $payload.latticeState } else { 'missing' })),
    ('- Primary pressure point: `{0}`' -f $payload.primaryPressurePoint),
    ('- Queued horizon count: `{0}`' -f $payload.queuedHorizonCount),
    ('- Withheld-expansion count: `{0}`' -f $payload.withheldExpansionCount),
    ('- Bonded release still withheld: `{0}`' -f [bool] $payload.bondedReleaseStillWithheld),
    ('- Publication maturity still withheld: `{0}`' -f [bool] $payload.publicationMaturityStillWithheld),
    ('- MoS-bearing depth still withheld: `{0}`' -f [bool] $payload.mosBearingDepthStillWithheld),
    ('- Inquiry focus bound: `{0}`' -f [bool] $payload.inquiryFocusBound),
    ('- Bonded focus bound: `{0}`' -f [bool] $payload.bondedFocusBound),
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
    researchBriefState = $payload.researchBriefState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    latticeState = $payload.latticeState
    primaryPressurePoint = $payload.primaryPressurePoint
    queuedHorizonCount = $payload.queuedHorizonCount
    withheldExpansionCount = $payload.withheldExpansionCount
    bondedReleaseStillWithheld = $payload.bondedReleaseStillWithheld
    publicationMaturityStillWithheld = $payload.publicationMaturityStillWithheld
    mosBearingDepthStillWithheld = $payload.mosBearingDepthStillWithheld
    inquiryFocusBound = $payload.inquiryFocusBound
    bondedFocusBound = $payload.bondedFocusBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bounded-horizon-research-brief] Bundle: {0}' -f $bundlePath)
$bundlePath
