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
$interlockDensityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interlockDensityLedgerStatePath)
$brittleDurableDifferentiationSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittleDurableDifferentiationSurfaceStatePath)
$coldAdmissionEligibilityGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateStatePath)
$archiveDispositionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.archiveDispositionLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coreInvariantLatticeWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coreInvariantLatticeWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the core invariant lattice witness writer can run.'
}

$interlockDensityLedgerState = Read-JsonFileOrNull -Path $interlockDensityLedgerStatePath
$brittleDurableDifferentiationSurfaceState = Read-JsonFileOrNull -Path $brittleDurableDifferentiationSurfaceStatePath
$coldAdmissionEligibilityGateState = Read-JsonFileOrNull -Path $coldAdmissionEligibilityGateStatePath
$archiveDispositionLedgerState = Read-JsonFileOrNull -Path $archiveDispositionLedgerStatePath

$currentInterlockDensityLedgerState = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.interlockDensityLedgerState } else { $null }
$currentBrittleDurableDifferentiationSurfaceState = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] $brittleDurableDifferentiationSurfaceState.brittleDurableDifferentiationSurfaceState } else { $null }
$currentColdAdmissionEligibilityGateState = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.coldAdmissionEligibilityGateState } else { $null }
$currentArchiveDispositionLedgerState = if ($null -ne $archiveDispositionLedgerState) { [string] $archiveDispositionLedgerState.archiveDispositionLedgerState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('CoreInvariantLatticeWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateCoreInvariantLatticeWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('core-invariant-lattice-witness-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateCoreInvariantLatticeWitnessHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateCoreInvariantLatticeWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateLatticeApproachWitness_DistinguishesInterlockFromRecurrentSuccess', [System.StringComparison]::Ordinal) -ge 0

$receiptState = 'awaiting-interlock-density-ledger'
$reasonCode = 'core-invariant-lattice-witness-awaiting-interlock-density-ledger'
$nextAction = 'emit-interlock-density-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $receiptState = 'blocked'
    $reasonCode = 'core-invariant-lattice-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentInterlockDensityLedgerState -ne 'interlock-density-ledger-ready') {
    $receiptState = 'awaiting-interlock-density-ledger'
    $reasonCode = 'core-invariant-lattice-witness-interlock-not-ready'
    $nextAction = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.nextAction } else { 'emit-interlock-density-ledger' }
} elseif ($currentBrittleDurableDifferentiationSurfaceState -ne 'brittle-durable-differentiation-surface-ready') {
    $receiptState = 'awaiting-brittle-durable-differentiation-surface'
    $reasonCode = 'core-invariant-lattice-witness-differentiation-not-ready'
    $nextAction = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] $brittleDurableDifferentiationSurfaceState.nextAction } else { 'emit-brittle-durable-differentiation-surface' }
} elseif ($currentColdAdmissionEligibilityGateState -ne 'cold-admission-eligibility-gate-ready') {
    $receiptState = 'awaiting-cold-admission-eligibility-gate'
    $reasonCode = 'core-invariant-lattice-witness-cold-gate-not-ready'
    $nextAction = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.nextAction } else { 'emit-cold-admission-eligibility-gate' }
} elseif ($currentArchiveDispositionLedgerState -ne 'archive-disposition-ledger-ready') {
    $receiptState = 'awaiting-archive-disposition-ledger'
    $reasonCode = 'core-invariant-lattice-witness-archive-not-ready'
    $nextAction = if ($null -ne $archiveDispositionLedgerState) { [string] $archiveDispositionLedgerState.nextAction } else { 'emit-archive-disposition-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $receiptState = 'awaiting-core-invariant-lattice-witness-binding'
    $reasonCode = 'core-invariant-lattice-witness-source-missing'
    $nextAction = 'bind-core-invariant-lattice-witness'
} else {
    $receiptState = 'core-invariant-lattice-witness-ready'
    $reasonCode = 'core-invariant-lattice-witness-bound'
    $nextAction = 'continue-lattice-governance'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'core-invariant-lattice-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'core-invariant-lattice-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    coreInvariantLatticeWitnessState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    interlockDensityLedgerState = $currentInterlockDensityLedgerState
    brittleDurableDifferentiationSurfaceState = $currentBrittleDurableDifferentiationSurfaceState
    coldAdmissionEligibilityGateState = $currentColdAdmissionEligibilityGateState
    archiveDispositionLedgerState = $currentArchiveDispositionLedgerState
    candidateCoreInvariantCount = 3
    identityAdjacencySignalCount = 3
    interlockPosture = 'pre-lattice-moderate'
    identityAdjacentSignificanceEmergent = $true
    coreLawSanctificationDenied = $true
    latticeGradeInvarianceWitnessed = $false
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Core Invariant Lattice Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness state: `{0}`' -f $payload.coreInvariantLatticeWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Interlock-density ledger state: `{0}`' -f $(if ($payload.interlockDensityLedgerState) { $payload.interlockDensityLedgerState } else { 'missing' })),
    ('- Brittle durable differentiation-surface state: `{0}`' -f $(if ($payload.brittleDurableDifferentiationSurfaceState) { $payload.brittleDurableDifferentiationSurfaceState } else { 'missing' })),
    ('- Cold admission-eligibility gate state: `{0}`' -f $(if ($payload.coldAdmissionEligibilityGateState) { $payload.coldAdmissionEligibilityGateState } else { 'missing' })),
    ('- Archive disposition-ledger state: `{0}`' -f $(if ($payload.archiveDispositionLedgerState) { $payload.archiveDispositionLedgerState } else { 'missing' })),
    ('- Candidate core-invariant count: `{0}`' -f $payload.candidateCoreInvariantCount),
    ('- Identity adjacency-signal count: `{0}`' -f $payload.identityAdjacencySignalCount),
    ('- Interlock posture: `{0}`' -f $payload.interlockPosture),
    ('- Identity-adjacent significance emergent: `{0}`' -f [bool] $payload.identityAdjacentSignificanceEmergent),
    ('- Core law sanctification denied: `{0}`' -f [bool] $payload.coreLawSanctificationDenied),
    ('- Lattice-grade invariance witnessed: `{0}`' -f [bool] $payload.latticeGradeInvarianceWitnessed),
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
    coreInvariantLatticeWitnessState = $payload.coreInvariantLatticeWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    interlockDensityLedgerState = $payload.interlockDensityLedgerState
    brittleDurableDifferentiationSurfaceState = $payload.brittleDurableDifferentiationSurfaceState
    coldAdmissionEligibilityGateState = $payload.coldAdmissionEligibilityGateState
    archiveDispositionLedgerState = $payload.archiveDispositionLedgerState
    candidateCoreInvariantCount = $payload.candidateCoreInvariantCount
    identityAdjacencySignalCount = $payload.identityAdjacencySignalCount
    interlockPosture = $payload.interlockPosture
    identityAdjacentSignificanceEmergent = $payload.identityAdjacentSignificanceEmergent
    coreLawSanctificationDenied = $payload.coreLawSanctificationDenied
    latticeGradeInvarianceWitnessed = $payload.latticeGradeInvarianceWitnessed
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
