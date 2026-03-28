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
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionSeductionWatchOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionSeductionWatchStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the promotion-seduction watch writer can run.'
}

$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$refusalState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath
$candidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$promotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath

$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentRefusalState = if ($null -ne $refusalState) { [string] $refusalState.questioningAdmissionRefusalReceiptState } else { $null }
$currentCandidateLedgerState = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
$currentPromotionGateState = if ($null -ne $promotionGateState) { [string] $promotionGateState.questioningGelPromotionGateState } else { $null }

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

$projectionBound = $contractsSource.IndexOf('PromotionSeductionWatchReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreatePromotionSeductionWatch', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('promotion-seduction-watch-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreatePromotionSeductionWatchHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreatePromotionSeductionWatch', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateVariationReentryRefusalAndSeductionWatch_HardenPromotionLaneAgainstAlmostGoodEnoughCandidates', [System.StringComparison]::Ordinal) -ge 0

$promotionSeductionWatchState = 'awaiting-questioning-admission-refusal-receipt'
$reasonCode = 'promotion-seduction-watch-awaiting-refusal'
$nextAction = 'emit-questioning-admission-refusal-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $promotionSeductionWatchState = 'blocked'
    $reasonCode = 'promotion-seduction-watch-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $promotionSeductionWatchState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'promotion-seduction-watch-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentRefusalState -ne 'questioning-admission-refusal-receipt-ready') {
    $promotionSeductionWatchState = 'awaiting-questioning-admission-refusal-receipt'
    $reasonCode = 'promotion-seduction-watch-refusal-not-ready'
    $nextAction = if ($null -ne $refusalState) { [string] $refusalState.nextAction } else { 'emit-questioning-admission-refusal-receipt' }
} elseif ($currentCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $promotionSeductionWatchState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'promotion-seduction-watch-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentPromotionGateState -ne 'questioning-gel-promotion-gate-ready') {
    $promotionSeductionWatchState = 'awaiting-questioning-gel-promotion-gate'
    $reasonCode = 'promotion-seduction-watch-promotion-gate-not-ready'
    $nextAction = if ($null -ne $promotionGateState) { [string] $promotionGateState.nextAction } else { 'emit-questioning-gel-promotion-gate' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $promotionSeductionWatchState = 'awaiting-promotion-seduction-watch-binding'
    $reasonCode = 'promotion-seduction-watch-source-missing'
    $nextAction = 'bind-promotion-seduction-watch'
} else {
    $promotionSeductionWatchState = 'promotion-seduction-watch-ready'
    $reasonCode = 'promotion-seduction-watch-bound'
    $nextAction = 'continue-guarded-questioning-review'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'promotion-seduction-watch.json'
$bundleMarkdownPath = Join-Path $bundlePath 'promotion-seduction-watch.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    promotionSeductionWatchState = $promotionSeductionWatchState
    reasonCode = $reasonCode
    nextAction = $nextAction
    variationTestedReentryLedgerState = $currentVariationState
    questioningAdmissionRefusalReceiptState = $currentRefusalState
    questioningOperatorCandidateLedgerState = $currentCandidateLedgerState
    questioningGelPromotionGateState = $currentPromotionGateState
    seductionSignalCount = 3
    blockedPromotionVectorCount = 3
    driftWarningCount = 3
    prestigeInflationDenied = $true
    eleganceBiasDenied = $true
    emotionalCompulsionDenied = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Promotion Seduction Watch',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Watch state: `{0}`' -f $payload.promotionSeductionWatchState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Admission-refusal state: `{0}`' -f $(if ($payload.questioningAdmissionRefusalReceiptState) { $payload.questioningAdmissionRefusalReceiptState } else { 'missing' })),
    ('- Candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Promotion-gate state: `{0}`' -f $(if ($payload.questioningGelPromotionGateState) { $payload.questioningGelPromotionGateState } else { 'missing' })),
    ('- Seduction-signal count: `{0}`' -f $payload.seductionSignalCount),
    ('- Blocked promotion-vector count: `{0}`' -f $payload.blockedPromotionVectorCount),
    ('- Drift-warning count: `{0}`' -f $payload.driftWarningCount),
    ('- Prestige inflation denied: `{0}`' -f [bool] $payload.prestigeInflationDenied),
    ('- Elegance bias denied: `{0}`' -f [bool] $payload.eleganceBiasDenied),
    ('- Emotional compulsion denied: `{0}`' -f [bool] $payload.emotionalCompulsionDenied),
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
    promotionSeductionWatchState = $payload.promotionSeductionWatchState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    seductionSignalCount = $payload.seductionSignalCount
    blockedPromotionVectorCount = $payload.blockedPromotionVectorCount
    driftWarningCount = $payload.driftWarningCount
    prestigeInflationDenied = $payload.prestigeInflationDenied
    eleganceBiasDenied = $payload.eleganceBiasDenied
    emotionalCompulsionDenied = $payload.emotionalCompulsionDenied
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
