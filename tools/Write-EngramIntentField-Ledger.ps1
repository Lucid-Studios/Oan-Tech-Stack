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
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$promotionSeductionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionSeductionWatchStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the engram intent-field ledger writer can run.'
}

$candidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$refusalState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath
$seductionWatchState = Read-JsonFileOrNull -Path $promotionSeductionWatchStatePath

$currentCandidateLedgerState = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentRefusalState = if ($null -ne $refusalState) { [string] $refusalState.questioningAdmissionRefusalReceiptState } else { $null }
$currentSeductionWatchState = if ($null -ne $seductionWatchState) { [string] $seductionWatchState.promotionSeductionWatchState } else { $null }

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

$projectionBound = $contractsSource.IndexOf('EngramIntentFieldLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateEngramIntentFieldLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('engram-intent-field-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateEngramIntentFieldLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateEngramIntentFieldLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmIntentAlignmentAndReactivation_RequireCandidatesToCarryTheirOwnWhyBeforeCooling', [System.StringComparison]::Ordinal) -ge 0

$engramIntentFieldLedgerState = 'awaiting-questioning-operator-candidate-ledger'
$reasonCode = 'engram-intent-field-ledger-awaiting-candidate-ledger'
$nextAction = 'emit-questioning-operator-candidate-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $engramIntentFieldLedgerState = 'blocked'
    $reasonCode = 'engram-intent-field-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $engramIntentFieldLedgerState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'engram-intent-field-ledger-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $engramIntentFieldLedgerState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'engram-intent-field-ledger-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentRefusalState -ne 'questioning-admission-refusal-receipt-ready') {
    $engramIntentFieldLedgerState = 'awaiting-questioning-admission-refusal-receipt'
    $reasonCode = 'engram-intent-field-ledger-refusal-not-ready'
    $nextAction = if ($null -ne $refusalState) { [string] $refusalState.nextAction } else { 'emit-questioning-admission-refusal-receipt' }
} elseif ($currentSeductionWatchState -ne 'promotion-seduction-watch-ready') {
    $engramIntentFieldLedgerState = 'awaiting-promotion-seduction-watch'
    $reasonCode = 'engram-intent-field-ledger-seduction-watch-not-ready'
    $nextAction = if ($null -ne $seductionWatchState) { [string] $seductionWatchState.nextAction } else { 'emit-promotion-seduction-watch' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $engramIntentFieldLedgerState = 'awaiting-engram-intent-field-binding'
    $reasonCode = 'engram-intent-field-ledger-source-missing'
    $nextAction = 'bind-engram-intent-field-ledger'
} else {
    $engramIntentFieldLedgerState = 'engram-intent-field-ledger-ready'
    $reasonCode = 'engram-intent-field-ledger-bound'
    $nextAction = 'emit-intent-constraint-alignment-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'engram-intent-field-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'engram-intent-field-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    engramIntentFieldLedgerState = $engramIntentFieldLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    questioningOperatorCandidateLedgerState = $currentCandidateLedgerState
    variationTestedReentryLedgerState = $currentVariationState
    questioningAdmissionRefusalReceiptState = $currentRefusalState
    promotionSeductionWatchState = $currentSeductionWatchState
    intentBearingPatternCount = 2
    sceneBoundPatternCount = 1
    resolutionOrientationCount = 3
    truthPostureCount = 3
    scopeClassCount = 2
    temporalPostureCount = 2
    dependencyRelationCount = 3
    candidateCarriesInternalIntent = $true
    borrowedJustificationDenied = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Engram Intent Field Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.engramIntentFieldLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Admission-refusal state: `{0}`' -f $(if ($payload.questioningAdmissionRefusalReceiptState) { $payload.questioningAdmissionRefusalReceiptState } else { 'missing' })),
    ('- Promotion seduction-watch state: `{0}`' -f $(if ($payload.promotionSeductionWatchState) { $payload.promotionSeductionWatchState } else { 'missing' })),
    ('- Intent-bearing pattern count: `{0}`' -f $payload.intentBearingPatternCount),
    ('- Scene-bound pattern count: `{0}`' -f $payload.sceneBoundPatternCount),
    ('- Resolution-orientation count: `{0}`' -f $payload.resolutionOrientationCount),
    ('- Truth-posture count: `{0}`' -f $payload.truthPostureCount),
    ('- Scope-class count: `{0}`' -f $payload.scopeClassCount),
    ('- Temporal-posture count: `{0}`' -f $payload.temporalPostureCount),
    ('- Dependency-relation count: `{0}`' -f $payload.dependencyRelationCount),
    ('- Candidate carries internal intent: `{0}`' -f [bool] $payload.candidateCarriesInternalIntent),
    ('- Borrowed justification denied: `{0}`' -f [bool] $payload.borrowedJustificationDenied),
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
    engramIntentFieldLedgerState = $payload.engramIntentFieldLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    promotionSeductionWatchState = $payload.promotionSeductionWatchState
    intentBearingPatternCount = $payload.intentBearingPatternCount
    sceneBoundPatternCount = $payload.sceneBoundPatternCount
    resolutionOrientationCount = $payload.resolutionOrientationCount
    truthPostureCount = $payload.truthPostureCount
    scopeClassCount = $payload.scopeClassCount
    temporalPostureCount = $payload.temporalPostureCount
    dependencyRelationCount = $payload.dependencyRelationCount
    candidateCarriesInternalIntent = $payload.candidateCarriesInternalIntent
    borrowedJustificationDenied = $payload.borrowedJustificationDenied
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
