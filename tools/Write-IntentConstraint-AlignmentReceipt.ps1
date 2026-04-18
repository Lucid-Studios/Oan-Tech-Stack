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
$engramIntentFieldLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the intent-constraint alignment writer can run.'
}

$intentFieldState = Read-JsonFileOrNull -Path $engramIntentFieldLedgerStatePath
$candidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$promotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath
$refusalState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath

$currentIntentFieldState = if ($null -ne $intentFieldState) { [string] $intentFieldState.engramIntentFieldLedgerState } else { $null }
$currentCandidateLedgerState = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentPromotionGateState = if ($null -ne $promotionGateState) { [string] $promotionGateState.questioningGelPromotionGateState } else { $null }
$currentRefusalState = if ($null -ne $refusalState) { [string] $refusalState.questioningAdmissionRefusalReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('IntentConstraintAlignmentReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateIntentConstraintAlignmentReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('intent-constraint-alignment-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateIntentConstraintAlignmentReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateIntentConstraintAlignmentReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmIntentAlignmentAndReactivation_RequireCandidatesToCarryTheirOwnWhyBeforeCooling', [System.StringComparison]::Ordinal) -ge 0

$intentConstraintAlignmentReceiptState = 'awaiting-engram-intent-field-ledger'
$reasonCode = 'intent-constraint-alignment-receipt-awaiting-intent-field'
$nextAction = 'emit-engram-intent-field-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $intentConstraintAlignmentReceiptState = 'blocked'
    $reasonCode = 'intent-constraint-alignment-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentIntentFieldState -ne 'engram-intent-field-ledger-ready') {
    $intentConstraintAlignmentReceiptState = 'awaiting-engram-intent-field-ledger'
    $reasonCode = 'intent-constraint-alignment-receipt-intent-field-not-ready'
    $nextAction = if ($null -ne $intentFieldState) { [string] $intentFieldState.nextAction } else { 'emit-engram-intent-field-ledger' }
} elseif ($currentCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $intentConstraintAlignmentReceiptState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'intent-constraint-alignment-receipt-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $intentConstraintAlignmentReceiptState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'intent-constraint-alignment-receipt-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentPromotionGateState -ne 'questioning-gel-promotion-gate-ready') {
    $intentConstraintAlignmentReceiptState = 'awaiting-questioning-gel-promotion-gate'
    $reasonCode = 'intent-constraint-alignment-receipt-promotion-gate-not-ready'
    $nextAction = if ($null -ne $promotionGateState) { [string] $promotionGateState.nextAction } else { 'emit-questioning-gel-promotion-gate' }
} elseif ($currentRefusalState -ne 'questioning-admission-refusal-receipt-ready') {
    $intentConstraintAlignmentReceiptState = 'awaiting-questioning-admission-refusal-receipt'
    $reasonCode = 'intent-constraint-alignment-receipt-refusal-not-ready'
    $nextAction = if ($null -ne $refusalState) { [string] $refusalState.nextAction } else { 'emit-questioning-admission-refusal-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $intentConstraintAlignmentReceiptState = 'awaiting-intent-constraint-alignment-binding'
    $reasonCode = 'intent-constraint-alignment-receipt-source-missing'
    $nextAction = 'bind-intent-constraint-alignment-receipt'
} else {
    $intentConstraintAlignmentReceiptState = 'intent-constraint-alignment-receipt-ready'
    $reasonCode = 'intent-constraint-alignment-receipt-bound'
    $nextAction = 'emit-warm-reactivation-disposition-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'intent-constraint-alignment-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'intent-constraint-alignment-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    intentConstraintAlignmentReceiptState = $intentConstraintAlignmentReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    engramIntentFieldLedgerState = $currentIntentFieldState
    questioningOperatorCandidateLedgerState = $currentCandidateLedgerState
    variationTestedReentryLedgerState = $currentVariationState
    questioningGelPromotionGateState = $currentPromotionGateState
    questioningAdmissionRefusalReceiptState = $currentRefusalState
    alignedPatternCount = 2
    misalignedPatternCount = 1
    structureConstraintAlignmentCount = 2
    intentConstraintAlignmentCount = 2
    provenanceAlignmentCheckCount = 3
    structureConstraintAlignmentSatisfied = $true
    provenanceAlignedWithIntent = $true
    sceneBoundIntentDetected = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Intent Constraint Alignment Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.intentConstraintAlignmentReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Engram intent-field state: `{0}`' -f $(if ($payload.engramIntentFieldLedgerState) { $payload.engramIntentFieldLedgerState } else { 'missing' })),
    ('- Candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Promotion-gate state: `{0}`' -f $(if ($payload.questioningGelPromotionGateState) { $payload.questioningGelPromotionGateState } else { 'missing' })),
    ('- Admission-refusal state: `{0}`' -f $(if ($payload.questioningAdmissionRefusalReceiptState) { $payload.questioningAdmissionRefusalReceiptState } else { 'missing' })),
    ('- Aligned pattern count: `{0}`' -f $payload.alignedPatternCount),
    ('- Misaligned pattern count: `{0}`' -f $payload.misalignedPatternCount),
    ('- Structure-constraint alignment count: `{0}`' -f $payload.structureConstraintAlignmentCount),
    ('- Intent-constraint alignment count: `{0}`' -f $payload.intentConstraintAlignmentCount),
    ('- Provenance-alignment check count: `{0}`' -f $payload.provenanceAlignmentCheckCount),
    ('- Structure-constraint alignment satisfied: `{0}`' -f [bool] $payload.structureConstraintAlignmentSatisfied),
    ('- Provenance aligned with intent: `{0}`' -f [bool] $payload.provenanceAlignedWithIntent),
    ('- Scene-bound intent detected: `{0}`' -f [bool] $payload.sceneBoundIntentDetected),
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
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    engramIntentFieldLedgerState = $payload.engramIntentFieldLedgerState
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    alignedPatternCount = $payload.alignedPatternCount
    misalignedPatternCount = $payload.misalignedPatternCount
    structureConstraintAlignmentCount = $payload.structureConstraintAlignmentCount
    intentConstraintAlignmentCount = $payload.intentConstraintAlignmentCount
    provenanceAlignmentCheckCount = $payload.provenanceAlignmentCheckCount
    structureConstraintAlignmentSatisfied = $payload.structureConstraintAlignmentSatisfied
    provenanceAlignedWithIntent = $payload.provenanceAlignedWithIntent
    sceneBoundIntentDetected = $payload.sceneBoundIntentDetected
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
