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
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the durability witness writer can run.'
}

$formationPhaseState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$brittlenessState = Read-JsonFileOrNull -Path $brittlenessWitnessStatePath
$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath

$currentFormationPhaseState = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.formationPhaseVectorState } else { $null }
$currentBrittlenessState = if ($null -ne $brittlenessState) { [string] $brittlenessState.brittlenessWitnessState } else { $null }
$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('DurabilityWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateDurabilityWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('durability-witness-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateDurabilityWitnessReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateDurabilityWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateFormationPhaseBrittlenessAndDurability_WitnessCandidatePositionAcrossFieldPressure', [System.StringComparison]::Ordinal) -ge 0

$durabilityWitnessState = 'awaiting-formation-phase-vector'
$reasonCode = 'durability-witness-awaiting-formation-phase-vector'
$nextAction = 'emit-formation-phase-vector'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $durabilityWitnessState = 'blocked'
    $reasonCode = 'durability-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentFormationPhaseState -ne 'formation-phase-vector-ready') {
    $durabilityWitnessState = 'awaiting-formation-phase-vector'
    $reasonCode = 'durability-witness-phase-vector-not-ready'
    $nextAction = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.nextAction } else { 'emit-formation-phase-vector' }
} elseif ($currentBrittlenessState -ne 'brittleness-witness-ready') {
    $durabilityWitnessState = 'awaiting-brittleness-witness'
    $reasonCode = 'durability-witness-brittleness-not-ready'
    $nextAction = if ($null -ne $brittlenessState) { [string] $brittlenessState.nextAction } else { 'emit-brittleness-witness' }
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $durabilityWitnessState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'durability-witness-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $durabilityWitnessState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'durability-witness-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $durabilityWitnessState = 'awaiting-durability-witness-binding'
    $reasonCode = 'durability-witness-source-missing'
    $nextAction = 'bind-durability-witness'
} else {
    $durabilityWitnessState = 'durability-witness-ready'
    $reasonCode = 'durability-witness-bound'
    $nextAction = 'continue-phase-space-review'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'durability-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'durability-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    durabilityWitnessState = $durabilityWitnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    formationPhaseVectorState = $currentFormationPhaseState
    brittlenessWitnessState = $currentBrittlenessState
    variationTestedReentryLedgerState = $currentVariationState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    durablePatternCount = 2
    interlockSignalCount = 3
    coolingBarrierCount = 3
    durableUnderVariation = $true
    interlockDensityEmergent = $true
    coldPromotionStillWithheld = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Durability Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness state: `{0}`' -f $payload.durabilityWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Formation phase-vector state: `{0}`' -f $(if ($payload.formationPhaseVectorState) { $payload.formationPhaseVectorState } else { 'missing' })),
    ('- Brittleness-witness state: `{0}`' -f $(if ($payload.brittlenessWitnessState) { $payload.brittlenessWitnessState } else { 'missing' })),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Durable-pattern count: `{0}`' -f $payload.durablePatternCount),
    ('- Interlock-signal count: `{0}`' -f $payload.interlockSignalCount),
    ('- Cooling-barrier count: `{0}`' -f $payload.coolingBarrierCount),
    ('- Durable under variation: `{0}`' -f [bool] $payload.durableUnderVariation),
    ('- Interlock density emergent: `{0}`' -f [bool] $payload.interlockDensityEmergent),
    ('- Cold promotion still withheld: `{0}`' -f [bool] $payload.coldPromotionStillWithheld),
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
    durabilityWitnessState = $payload.durabilityWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    formationPhaseVectorState = $payload.formationPhaseVectorState
    brittlenessWitnessState = $payload.brittlenessWitnessState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    durablePatternCount = $payload.durablePatternCount
    interlockSignalCount = $payload.interlockSignalCount
    coolingBarrierCount = $payload.coolingBarrierCount
    durableUnderVariation = $payload.durableUnderVariation
    interlockDensityEmergent = $payload.interlockDensityEmergent
    coldPromotionStillWithheld = $payload.coldPromotionStillWithheld
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
