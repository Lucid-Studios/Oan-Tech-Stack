param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-tasking.json',
    [string] $FormalSurfaceMarkdownPath = 'OAN Mortalis V1.1.1/docs/LOCAL_AUTOMATION_TASKING_SURFACE.md'
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
$localHostSanctuaryResidencyEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localHostSanctuaryResidencyEnvelopeStatePath)
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$boundedInhabitationLaunchRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedInhabitationLaunchRehearsalStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postHabitationHorizonLatticeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postHabitationHorizonLatticeStatePath)
$formalSurfacePath = $resolvedFormalSurfacePath

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the post-habitation horizon lattice writer can run.'
}

$envelopeState = Read-JsonFileOrNull -Path $localHostSanctuaryResidencyEnvelopeStatePath
$readinessLedgerState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$launchRehearsalState = Read-JsonFileOrNull -Path $boundedInhabitationLaunchRehearsalStatePath
$formalSurfaceText = if (Test-Path -LiteralPath $formalSurfacePath -PathType Leaf) { Get-Content -Raw -LiteralPath $formalSurfacePath } else { '' }

$mapIds = @(
    'automation-maturation-map-25',
    'automation-maturation-map-26',
    'automation-maturation-map-27'
)
$surfaceHeadings = @(
    '### Automation Maturation Map 25',
    '### Automation Maturation Map 26',
    '### Automation Maturation Map 27'
)

$taskingDeclarationBound = $true
foreach ($mapId in $mapIds) {
    if ($taskingPolicy.longFormTaskMaps.id -notcontains $mapId) {
        $taskingDeclarationBound = $false
        break
    }
}

$surfaceMirrorBound = $true
foreach ($heading in $surfaceHeadings) {
    if ($formalSurfaceText.IndexOf($heading, [System.StringComparison]::Ordinal) -lt 0) {
        $surfaceMirrorBound = $false
        break
    }
}

$sourceFiles = @(
    $resolvedTaskingPolicyPath,
    $formalSurfacePath
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })

$currentEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $envelopeState -PropertyName 'envelopeState')
$currentReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $readinessLedgerState -PropertyName 'readinessLedgerState')
$currentLaunchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $launchRehearsalState -PropertyName 'launchRehearsalState')

$latticeState = 'awaiting-bounded-habitation'
$reasonCode = 'post-habitation-horizon-lattice-awaiting-bounded-habitation'
$nextAction = 'emit-local-host-sanctuary-residency-envelope'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $latticeState = 'blocked'
    $reasonCode = 'post-habitation-horizon-lattice-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentEnvelopeState -ne 'local-host-sanctuary-residency-envelope-ready' -or
    $currentReadinessState -ne 'runtime-habitation-readiness-ledger-ready' -or
    $currentLaunchState -ne 'bounded-inhabitation-launch-rehearsal-ready') {
    $latticeState = 'awaiting-bounded-habitation'
    $reasonCode = 'post-habitation-horizon-lattice-habitation-not-ready'
    $nextAction = 'complete-map-24-habitation-lane'
} elseif (-not $taskingDeclarationBound -or -not $surfaceMirrorBound -or $missingSourceFiles.Count -gt 0) {
    $latticeState = 'awaiting-horizon-declaration'
    $reasonCode = 'post-habitation-horizon-lattice-declarations-missing'
    $nextAction = 'declare-post-habitation-horizon-maps'
} else {
    $latticeState = 'post-habitation-horizon-lattice-ready'
    $reasonCode = 'post-habitation-horizon-lattice-bound'
    $nextAction = 'emit-bounded-horizon-research-brief'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'post-habitation-horizon-lattice.json'
$bundleMarkdownPath = Join-Path $bundlePath 'post-habitation-horizon-lattice.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    latticeState = $latticeState
    reasonCode = $reasonCode
    nextAction = $nextAction
    envelopeState = $currentEnvelopeState
    readinessLedgerState = $currentReadinessState
    launchRehearsalState = $currentLaunchState
    anchorReceiptCount = 3
    candidateHorizonCount = 3
    withheldExpansionCount = 3
    researchCycleBounded = $true
    taskingDeclarationBound = $taskingDeclarationBound
    surfaceMirrorBound = $surfaceMirrorBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
    declaredMapIds = $mapIds
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
    '# Post-Habitation Horizon Lattice',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Lattice state: `{0}`' -f $payload.latticeState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Envelope state: `{0}`' -f $(if ($payload.envelopeState) { $payload.envelopeState } else { 'missing' })),
    ('- Readiness-ledger state: `{0}`' -f $(if ($payload.readinessLedgerState) { $payload.readinessLedgerState } else { 'missing' })),
    ('- Launch-rehearsal state: `{0}`' -f $(if ($payload.launchRehearsalState) { $payload.launchRehearsalState } else { 'missing' })),
    ('- Anchor-receipt count: `{0}`' -f $payload.anchorReceiptCount),
    ('- Candidate-horizon count: `{0}`' -f $payload.candidateHorizonCount),
    ('- Withheld-expansion count: `{0}`' -f $payload.withheldExpansionCount),
    ('- Research cycle bounded: `{0}`' -f [bool] $payload.researchCycleBounded),
    ('- Tasking declaration bound: `{0}`' -f [bool] $payload.taskingDeclarationBound),
    ('- Surface mirror bound: `{0}`' -f [bool] $payload.surfaceMirrorBound),
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
    latticeState = $payload.latticeState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    envelopeState = $payload.envelopeState
    readinessLedgerState = $payload.readinessLedgerState
    launchRehearsalState = $payload.launchRehearsalState
    anchorReceiptCount = $payload.anchorReceiptCount
    candidateHorizonCount = $payload.candidateHorizonCount
    withheldExpansionCount = $payload.withheldExpansionCount
    researchCycleBounded = $payload.researchCycleBounded
    taskingDeclarationBound = $payload.taskingDeclarationBound
    surfaceMirrorBound = $payload.surfaceMirrorBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[post-habitation-horizon-lattice] Bundle: {0}' -f $bundlePath)
$bundlePath
