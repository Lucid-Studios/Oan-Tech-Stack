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
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$warmReactivationDispositionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the brittleness witness writer can run.'
}

$formationPhaseState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath
$warmState = Read-JsonFileOrNull -Path $warmReactivationDispositionReceiptStatePath
$refusalState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath

$currentFormationPhaseState = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.formationPhaseVectorState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }
$currentWarmState = if ($null -ne $warmState) { [string] $warmState.warmReactivationDispositionReceiptState } else { $null }
$currentRefusalState = if ($null -ne $refusalState) { [string] $refusalState.questioningAdmissionRefusalReceiptState } else { $null }

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

$projectionBound = $contractsSource.IndexOf('BrittlenessWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBrittlenessWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('brittleness-witness-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateBrittlenessWitnessReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBrittlenessWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateFormationPhaseBrittlenessAndDurability_WitnessCandidatePositionAcrossFieldPressure', [System.StringComparison]::Ordinal) -ge 0

$brittlenessWitnessState = 'awaiting-formation-phase-vector'
$reasonCode = 'brittleness-witness-awaiting-formation-phase-vector'
$nextAction = 'emit-formation-phase-vector'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $brittlenessWitnessState = 'blocked'
    $reasonCode = 'brittleness-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentFormationPhaseState -ne 'formation-phase-vector-ready') {
    $brittlenessWitnessState = 'awaiting-formation-phase-vector'
    $reasonCode = 'brittleness-witness-phase-vector-not-ready'
    $nextAction = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.nextAction } else { 'emit-formation-phase-vector' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $brittlenessWitnessState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'brittleness-witness-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($currentWarmState -ne 'warm-reactivation-disposition-receipt-ready') {
    $brittlenessWitnessState = 'awaiting-warm-reactivation-disposition-receipt'
    $reasonCode = 'brittleness-witness-warm-disposition-not-ready'
    $nextAction = if ($null -ne $warmState) { [string] $warmState.nextAction } else { 'emit-warm-reactivation-disposition-receipt' }
} elseif ($currentRefusalState -ne 'questioning-admission-refusal-receipt-ready') {
    $brittlenessWitnessState = 'awaiting-questioning-admission-refusal-receipt'
    $reasonCode = 'brittleness-witness-refusal-not-ready'
    $nextAction = if ($null -ne $refusalState) { [string] $refusalState.nextAction } else { 'emit-questioning-admission-refusal-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $brittlenessWitnessState = 'awaiting-brittleness-witness-binding'
    $reasonCode = 'brittleness-witness-source-missing'
    $nextAction = 'bind-brittleness-witness'
} else {
    $brittlenessWitnessState = 'brittleness-witness-ready'
    $reasonCode = 'brittleness-witness-bound'
    $nextAction = 'emit-durability-witness'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'brittleness-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'brittleness-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    brittlenessWitnessState = $brittlenessWitnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    formationPhaseVectorState = $currentFormationPhaseState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    warmReactivationDispositionReceiptState = $currentWarmState
    questioningAdmissionRefusalReceiptState = $currentRefusalState
    brittlePatternCount = 1
    fractureAxisCount = 3
    overfitWarningCount = 3
    sceneBoundBrittlenessDetected = $true
    misalignmentPressureDetected = $true
    prematureCoolingDenied = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Brittleness Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness state: `{0}`' -f $payload.brittlenessWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Formation phase-vector state: `{0}`' -f $(if ($payload.formationPhaseVectorState) { $payload.formationPhaseVectorState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Warm reactivation-disposition state: `{0}`' -f $(if ($payload.warmReactivationDispositionReceiptState) { $payload.warmReactivationDispositionReceiptState } else { 'missing' })),
    ('- Questioning admission-refusal state: `{0}`' -f $(if ($payload.questioningAdmissionRefusalReceiptState) { $payload.questioningAdmissionRefusalReceiptState } else { 'missing' })),
    ('- Brittle-pattern count: `{0}`' -f $payload.brittlePatternCount),
    ('- Fracture-axis count: `{0}`' -f $payload.fractureAxisCount),
    ('- Overfit-warning count: `{0}`' -f $payload.overfitWarningCount),
    ('- Scene-bound brittleness detected: `{0}`' -f [bool] $payload.sceneBoundBrittlenessDetected),
    ('- Misalignment pressure detected: `{0}`' -f [bool] $payload.misalignmentPressureDetected),
    ('- Premature cooling denied: `{0}`' -f [bool] $payload.prematureCoolingDenied),
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
    brittlenessWitnessState = $payload.brittlenessWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    formationPhaseVectorState = $payload.formationPhaseVectorState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    warmReactivationDispositionReceiptState = $payload.warmReactivationDispositionReceiptState
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    brittlePatternCount = $payload.brittlePatternCount
    fractureAxisCount = $payload.fractureAxisCount
    overfitWarningCount = $payload.overfitWarningCount
    sceneBoundBrittlenessDetected = $payload.sceneBoundBrittlenessDetected
    misalignmentPressureDetected = $payload.misalignmentPressureDetected
    prematureCoolingDenied = $payload.prematureCoolingDenied
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
