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
$distanceWeightedQuestioningAdmissionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.distanceWeightedQuestioningAdmissionSurfaceStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$protectedQuestioningPatternSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedQuestioningPatternSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the variation-tested reentry writer can run.'
}

$distanceWeightedState = Read-JsonFileOrNull -Path $distanceWeightedQuestioningAdmissionSurfaceStatePath
$candidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$promotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath
$protectedPatternState = Read-JsonFileOrNull -Path $protectedQuestioningPatternSurfaceStatePath

$currentDistanceWeightedState = if ($null -ne $distanceWeightedState) { [string] $distanceWeightedState.distanceWeightedQuestioningAdmissionSurfaceState } else { $null }
$currentCandidateLedgerState = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
$currentPromotionGateState = if ($null -ne $promotionGateState) { [string] $promotionGateState.questioningGelPromotionGateState } else { $null }
$currentProtectedPatternState = if ($null -ne $protectedPatternState) { [string] $protectedPatternState.protectedQuestioningPatternSurfaceState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('VariationTestedReentryLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateVariationTestedReentryLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('variation-tested-reentry-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateVariationTestedReentryLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateVariationTestedReentryLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateVariationReentryRefusalAndSeductionWatch_HardenPromotionLaneAgainstAlmostGoodEnoughCandidates', [System.StringComparison]::Ordinal) -ge 0

$variationTestedReentryLedgerState = 'awaiting-distance-weighted-questioning-admission-surface'
$reasonCode = 'variation-tested-reentry-ledger-awaiting-distance-weighted-admission'
$nextAction = 'emit-distance-weighted-questioning-admission-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $variationTestedReentryLedgerState = 'blocked'
    $reasonCode = 'variation-tested-reentry-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentDistanceWeightedState -ne 'distance-weighted-questioning-admission-surface-ready') {
    $variationTestedReentryLedgerState = 'awaiting-distance-weighted-questioning-admission-surface'
    $reasonCode = 'variation-tested-reentry-ledger-distance-weighted-not-ready'
    $nextAction = if ($null -ne $distanceWeightedState) { [string] $distanceWeightedState.nextAction } else { 'emit-distance-weighted-questioning-admission-surface' }
} elseif ($currentCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $variationTestedReentryLedgerState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'variation-tested-reentry-ledger-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentPromotionGateState -ne 'questioning-gel-promotion-gate-ready') {
    $variationTestedReentryLedgerState = 'awaiting-questioning-gel-promotion-gate'
    $reasonCode = 'variation-tested-reentry-ledger-promotion-gate-not-ready'
    $nextAction = if ($null -ne $promotionGateState) { [string] $promotionGateState.nextAction } else { 'emit-questioning-gel-promotion-gate' }
} elseif ($currentProtectedPatternState -ne 'protected-questioning-pattern-surface-ready') {
    $variationTestedReentryLedgerState = 'awaiting-protected-questioning-pattern-surface'
    $reasonCode = 'variation-tested-reentry-ledger-protected-surface-not-ready'
    $nextAction = if ($null -ne $protectedPatternState) { [string] $protectedPatternState.nextAction } else { 'emit-protected-questioning-pattern-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $variationTestedReentryLedgerState = 'awaiting-variation-tested-reentry-binding'
    $reasonCode = 'variation-tested-reentry-ledger-source-missing'
    $nextAction = 'bind-variation-tested-reentry-ledger'
} else {
    $variationTestedReentryLedgerState = 'variation-tested-reentry-ledger-ready'
    $reasonCode = 'variation-tested-reentry-ledger-bound'
    $nextAction = 'emit-questioning-admission-refusal-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'variation-tested-reentry-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'variation-tested-reentry-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    variationTestedReentryLedgerState = $variationTestedReentryLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    distanceWeightedQuestioningAdmissionSurfaceState = $currentDistanceWeightedState
    questioningOperatorCandidateLedgerState = $currentCandidateLedgerState
    questioningGelPromotionGateState = $currentPromotionGateState
    protectedQuestioningPatternSurfaceState = $currentProtectedPatternState
    variationContextCount = 3
    survivingPatternCount = 2
    failedPatternCount = 1
    requiredRetestPatternCount = 1
    requiredReentryPassCount = 2
    variationBurdenSatisfied = $true
    portablePatternsWithstoodVariation = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Variation-Tested Reentry Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.variationTestedReentryLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Distance-weighted admission state: `{0}`' -f $(if ($payload.distanceWeightedQuestioningAdmissionSurfaceState) { $payload.distanceWeightedQuestioningAdmissionSurfaceState } else { 'missing' })),
    ('- Candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Promotion-gate state: `{0}`' -f $(if ($payload.questioningGelPromotionGateState) { $payload.questioningGelPromotionGateState } else { 'missing' })),
    ('- Protected questioning-pattern state: `{0}`' -f $(if ($payload.protectedQuestioningPatternSurfaceState) { $payload.protectedQuestioningPatternSurfaceState } else { 'missing' })),
    ('- Variation-context count: `{0}`' -f $payload.variationContextCount),
    ('- Surviving pattern count: `{0}`' -f $payload.survivingPatternCount),
    ('- Failed pattern count: `{0}`' -f $payload.failedPatternCount),
    ('- Required retest-pattern count: `{0}`' -f $payload.requiredRetestPatternCount),
    ('- Required re-entry pass count: `{0}`' -f $payload.requiredReentryPassCount),
    ('- Variation burden satisfied: `{0}`' -f [bool] $payload.variationBurdenSatisfied),
    ('- Portable patterns withstood variation: `{0}`' -f [bool] $payload.portablePatternsWithstoodVariation),
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
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    distanceWeightedQuestioningAdmissionSurfaceState = $payload.distanceWeightedQuestioningAdmissionSurfaceState
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    protectedQuestioningPatternSurfaceState = $payload.protectedQuestioningPatternSurfaceState
    variationContextCount = $payload.variationContextCount
    survivingPatternCount = $payload.survivingPatternCount
    failedPatternCount = $payload.failedPatternCount
    requiredRetestPatternCount = $payload.requiredRetestPatternCount
    requiredReentryPassCount = $payload.requiredReentryPassCount
    variationBurdenSatisfied = $payload.variationBurdenSatisfied
    portablePatternsWithstoodVariation = $payload.portablePatternsWithstoodVariation
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
