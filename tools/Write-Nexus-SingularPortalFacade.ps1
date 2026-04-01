param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json'
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$protectedStateLegibilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceStatePath)
$bondedOperatorLocalityReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the nexus singular portal facade can run.'
}

$protectedStateLegibilitySurfaceState = Read-JsonFileOrNull -Path $protectedStateLegibilitySurfaceStatePath
$bondedOperatorLocalityReadinessState = Read-JsonFileOrNull -Path $bondedOperatorLocalityReadinessStatePath

$resolvedPortalSourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'nexus-portal-base' -CyclePolicy $cyclePolicy
$missingPortalSourceFiles = @($resolvedPortalSourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })

$portalState = 'awaiting-bounded-legibility'
$reasonCode = 'nexus-singular-portal-facade-awaiting-bounded-legibility'
$nextAction = 'emit-protected-state-legibility-surface'

$protectedLegibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedStateLegibilitySurfaceState -PropertyName 'legibilityState')
$bondedReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedOperatorLocalityReadinessState -PropertyName 'readinessState')

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $portalState = 'blocked'
    $reasonCode = 'nexus-singular-portal-facade-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $protectedStateLegibilitySurfaceState -or $protectedLegibilityState -ne 'bounded-legibility-ready') {
    $portalState = 'awaiting-bounded-legibility'
    $reasonCode = 'nexus-singular-portal-facade-legibility-not-ready'
    $nextAction = if ($null -ne $protectedStateLegibilitySurfaceState) { [string] $protectedStateLegibilitySurfaceState.nextAction } else { 'emit-protected-state-legibility-surface' }
} elseif ($missingPortalSourceFiles.Count -gt 0) {
    $portalState = 'awaiting-portal-binding'
    $reasonCode = 'nexus-singular-portal-facade-source-missing'
    $nextAction = 'bind-singular-nexus-portal-code-surface'
} else {
    $portalState = 'portal-facade-ready'
    $reasonCode = 'nexus-singular-portal-facade-bound'
    $nextAction = 'bind-duplex-predicate-envelope'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'nexus-singular-portal-facade.json'
$bundleMarkdownPath = Join-Path $bundlePath 'nexus-singular-portal-facade.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    portalState = $portalState
    reasonCode = $reasonCode
    nextAction = $nextAction
    protectedLegibilityState = $protectedLegibilityState
    bondedOperatorLocalityReadinessState = $bondedReadinessState
    sourceFileCount = @($resolvedPortalSourceFiles).Count
    missingSourceFileCount = @($missingPortalSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $resolvedPortalSourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Nexus Singular Portal Facade',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Portal state: `{0}`' -f $payload.portalState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Protected legibility state: `{0}`' -f $(if ($payload.protectedLegibilityState) { $payload.protectedLegibilityState } else { 'missing' })),
    ('- Bonded operator locality readiness state: `{0}`' -f $(if ($payload.bondedOperatorLocalityReadinessState) { $payload.bondedOperatorLocalityReadinessState } else { 'missing' })),
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
    portalState = $payload.portalState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    protectedLegibilityState = $payload.protectedLegibilityState
    bondedOperatorLocalityReadinessState = $payload.bondedOperatorLocalityReadinessState
    sourceFileCount = $payload.sourceFileCount
    missingSourceFileCount = $payload.missingSourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[nexus-singular-portal-facade] Bundle: {0}' -f $bundlePath)
$bundlePath
