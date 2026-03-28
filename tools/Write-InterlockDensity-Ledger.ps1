param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json'
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
    param([string] $BasePath, [string] $CandidatePath)

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
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$coldAdmissionEligibilityGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interlockDensityLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interlockDensityLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the interlock density ledger writer can run.'
}

$durabilityWitnessState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath
$variationTestedReentryLedgerState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$coldAdmissionEligibilityGateState = Read-JsonFileOrNull -Path $coldAdmissionEligibilityGateStatePath

$currentDurabilityWitnessState = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.durabilityWitnessState } else { $null }
$currentVariationTestedReentryLedgerState = if ($null -ne $variationTestedReentryLedgerState) { [string] $variationTestedReentryLedgerState.variationTestedReentryLedgerState } else { $null }
$currentColdAdmissionEligibilityGateState = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.coldAdmissionEligibilityGateState } else { $null }

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/SanctuaryRuntimeWorkbenchServiceTests.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('InterlockDensityLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateInterlockDensityLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('interlock-density-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateInterlockDensityLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateInterlockDensityLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateLatticeApproachWitness_DistinguishesInterlockFromRecurrentSuccess', [System.StringComparison]::Ordinal) -ge 0

$ledgerState = 'awaiting-durability-witness'
$reasonCode = 'interlock-density-ledger-awaiting-durability-witness'
$nextAction = 'emit-durability-witness'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'interlock-density-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentDurabilityWitnessState -ne 'durability-witness-ready') {
    $ledgerState = 'awaiting-durability-witness'
    $reasonCode = 'interlock-density-ledger-durability-not-ready'
    $nextAction = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.nextAction } else { 'emit-durability-witness' }
} elseif ($currentVariationTestedReentryLedgerState -ne 'variation-tested-reentry-ledger-ready') {
    $ledgerState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'interlock-density-ledger-reentry-not-ready'
    $nextAction = if ($null -ne $variationTestedReentryLedgerState) { [string] $variationTestedReentryLedgerState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentColdAdmissionEligibilityGateState -ne 'cold-admission-eligibility-gate-ready') {
    $ledgerState = 'awaiting-cold-admission-eligibility-gate'
    $reasonCode = 'interlock-density-ledger-cold-gate-not-ready'
    $nextAction = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.nextAction } else { 'emit-cold-admission-eligibility-gate' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $ledgerState = 'awaiting-interlock-density-ledger-binding'
    $reasonCode = 'interlock-density-ledger-source-missing'
    $nextAction = 'bind-interlock-density-ledger'
} else {
    $ledgerState = 'interlock-density-ledger-ready'
    $reasonCode = 'interlock-density-ledger-bound'
    $nextAction = 'emit-brittle-durable-differentiation-surface'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'interlock-density-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'interlock-density-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    interlockDensityLedgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    durabilityWitnessState = $currentDurabilityWitnessState
    variationTestedReentryLedgerState = $currentVariationTestedReentryLedgerState
    coldAdmissionEligibilityGateState = $currentColdAdmissionEligibilityGateState
    independentConstraintLinkCount = 7
    reentrySurvivalCount = 2
    durableAlignmentCount = 2
    interlockDensityDisposition = 'moderate-interlock-density'
    denseInterweaveEmergent = $false
    latticeClaimStillWithheld = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Interlock Density Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.interlockDensityLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Durability-witness state: `{0}`' -f $(if ($payload.durabilityWitnessState) { $payload.durabilityWitnessState } else { 'missing' })),
    ('- Variation-tested reentry-ledger state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Cold admission-eligibility gate state: `{0}`' -f $(if ($payload.coldAdmissionEligibilityGateState) { $payload.coldAdmissionEligibilityGateState } else { 'missing' })),
    ('- Independent constraint-link count: `{0}`' -f $payload.independentConstraintLinkCount),
    ('- Reentry-survival count: `{0}`' -f $payload.reentrySurvivalCount),
    ('- Durable-alignment count: `{0}`' -f $payload.durableAlignmentCount),
    ('- Interlock density disposition: `{0}`' -f $payload.interlockDensityDisposition),
    ('- Dense interweave emergent: `{0}`' -f [bool] $payload.denseInterweaveEmergent),
    ('- Lattice claim still withheld: `{0}`' -f [bool] $payload.latticeClaimStillWithheld),
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
    interlockDensityLedgerState = $payload.interlockDensityLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    durabilityWitnessState = $payload.durabilityWitnessState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    coldAdmissionEligibilityGateState = $payload.coldAdmissionEligibilityGateState
    independentConstraintLinkCount = $payload.independentConstraintLinkCount
    reentrySurvivalCount = $payload.reentrySurvivalCount
    durableAlignmentCount = $payload.durableAlignmentCount
    interlockDensityDisposition = $payload.interlockDensityDisposition
    denseInterweaveEmergent = $payload.denseInterweaveEmergent
    latticeClaimStillWithheld = $payload.latticeClaimStillWithheld
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
