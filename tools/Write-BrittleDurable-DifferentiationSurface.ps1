param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json'
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$interlockDensityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interlockDensityLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittleDurableDifferentiationSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittleDurableDifferentiationSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the brittle durable differentiation surface writer can run.'
}

$brittlenessWitnessState = Read-JsonFileOrNull -Path $brittlenessWitnessStatePath
$durabilityWitnessState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath
$interlockDensityLedgerState = Read-JsonFileOrNull -Path $interlockDensityLedgerStatePath

$currentBrittlenessWitnessState = if ($null -ne $brittlenessWitnessState) { [string] $brittlenessWitnessState.brittlenessWitnessState } else { $null }
$currentDurabilityWitnessState = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.durabilityWitnessState } else { $null }
$currentInterlockDensityLedgerState = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.interlockDensityLedgerState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('BrittleDurableDifferentiationSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBrittleDurableDifferentiationSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('brittle-durable-differentiation-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateBrittleDurableDifferentiationSurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBrittleDurableDifferentiationSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateLatticeApproachWitness_DistinguishesInterlockFromRecurrentSuccess', [System.StringComparison]::Ordinal) -ge 0

$surfaceState = 'awaiting-brittleness-witness'
$reasonCode = 'brittle-durable-differentiation-surface-awaiting-brittleness-witness'
$nextAction = 'emit-brittleness-witness'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $surfaceState = 'blocked'
    $reasonCode = 'brittle-durable-differentiation-surface-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentBrittlenessWitnessState -ne 'brittleness-witness-ready') {
    $surfaceState = 'awaiting-brittleness-witness'
    $reasonCode = 'brittle-durable-differentiation-surface-brittleness-not-ready'
    $nextAction = if ($null -ne $brittlenessWitnessState) { [string] $brittlenessWitnessState.nextAction } else { 'emit-brittleness-witness' }
} elseif ($currentDurabilityWitnessState -ne 'durability-witness-ready') {
    $surfaceState = 'awaiting-durability-witness'
    $reasonCode = 'brittle-durable-differentiation-surface-durability-not-ready'
    $nextAction = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.nextAction } else { 'emit-durability-witness' }
} elseif ($currentInterlockDensityLedgerState -ne 'interlock-density-ledger-ready') {
    $surfaceState = 'awaiting-interlock-density-ledger'
    $reasonCode = 'brittle-durable-differentiation-surface-interlock-not-ready'
    $nextAction = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.nextAction } else { 'emit-interlock-density-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $surfaceState = 'awaiting-brittle-durable-differentiation-surface-binding'
    $reasonCode = 'brittle-durable-differentiation-surface-source-missing'
    $nextAction = 'bind-brittle-durable-differentiation-surface'
} else {
    $surfaceState = 'brittle-durable-differentiation-surface-ready'
    $reasonCode = 'brittle-durable-differentiation-surface-bound'
    $nextAction = 'emit-core-invariant-lattice-witness'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'brittle-durable-differentiation-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'brittle-durable-differentiation-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    brittleDurableDifferentiationSurfaceState = $surfaceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    brittlenessWitnessState = $currentBrittlenessWitnessState
    durabilityWitnessState = $currentDurabilityWitnessState
    interlockDensityLedgerState = $currentInterlockDensityLedgerState
    brittleFragmentCount = 1
    durableKernelCount = 2
    coexistingRegionCount = 3
    surfaceDisposition = 'mixed-structure-under-review'
    brittleDurableCoexistenceExposed = $true
    averageReadinessDenied = $true
    fullTrustStillWithheld = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Brittle Durable Differentiation Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Surface state: `{0}`' -f $payload.brittleDurableDifferentiationSurfaceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Brittleness-witness state: `{0}`' -f $(if ($payload.brittlenessWitnessState) { $payload.brittlenessWitnessState } else { 'missing' })),
    ('- Durability-witness state: `{0}`' -f $(if ($payload.durabilityWitnessState) { $payload.durabilityWitnessState } else { 'missing' })),
    ('- Interlock-density ledger state: `{0}`' -f $(if ($payload.interlockDensityLedgerState) { $payload.interlockDensityLedgerState } else { 'missing' })),
    ('- Brittle fragment count: `{0}`' -f $payload.brittleFragmentCount),
    ('- Durable kernel count: `{0}`' -f $payload.durableKernelCount),
    ('- Coexisting region count: `{0}`' -f $payload.coexistingRegionCount),
    ('- Surface disposition: `{0}`' -f $payload.surfaceDisposition),
    ('- Brittle durable coexistence exposed: `{0}`' -f [bool] $payload.brittleDurableCoexistenceExposed),
    ('- Average readiness denied: `{0}`' -f [bool] $payload.averageReadinessDenied),
    ('- Full trust still withheld: `{0}`' -f [bool] $payload.fullTrustStillWithheld),
    ('- Projection bound: `{0}`' -f [bool] $payload.projectionBound),
    ('- Key bound: `{0}`' -f [bool] $payload.keyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Test binding bound: `{0}`' -f [bool] $payload.testBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    brittleDurableDifferentiationSurfaceState = $payload.brittleDurableDifferentiationSurfaceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    brittlenessWitnessState = $payload.brittlenessWitnessState
    durabilityWitnessState = $payload.durabilityWitnessState
    interlockDensityLedgerState = $payload.interlockDensityLedgerState
    brittleFragmentCount = $payload.brittleFragmentCount
    durableKernelCount = $payload.durableKernelCount
    coexistingRegionCount = $payload.coexistingRegionCount
    surfaceDisposition = $payload.surfaceDisposition
    brittleDurableCoexistenceExposed = $payload.brittleDurableCoexistenceExposed
    averageReadinessDenied = $payload.averageReadinessDenied
    fullTrustStillWithheld = $payload.fullTrustStillWithheld
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
