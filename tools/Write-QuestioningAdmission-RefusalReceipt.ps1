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
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$protectedQuestioningPatternSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedQuestioningPatternSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the questioning admission-refusal writer can run.'
}

$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$candidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$promotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath
$protectedPatternState = Read-JsonFileOrNull -Path $protectedQuestioningPatternSurfaceStatePath

$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentCandidateLedgerState = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
$currentPromotionGateState = if ($null -ne $promotionGateState) { [string] $promotionGateState.questioningGelPromotionGateState } else { $null }
$currentProtectedPatternState = if ($null -ne $protectedPatternState) { [string] $protectedPatternState.protectedQuestioningPatternSurfaceState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('QuestioningAdmissionRefusalReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateQuestioningAdmissionRefusalReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('questioning-admission-refusal-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateQuestioningAdmissionRefusalReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateQuestioningAdmissionRefusalReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateVariationReentryRefusalAndSeductionWatch_HardenPromotionLaneAgainstAlmostGoodEnoughCandidates', [System.StringComparison]::Ordinal) -ge 0

$questioningAdmissionRefusalReceiptState = 'awaiting-variation-tested-reentry-ledger'
$reasonCode = 'questioning-admission-refusal-receipt-awaiting-reentry'
$nextAction = 'emit-variation-tested-reentry-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $questioningAdmissionRefusalReceiptState = 'blocked'
    $reasonCode = 'questioning-admission-refusal-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $questioningAdmissionRefusalReceiptState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'questioning-admission-refusal-receipt-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $questioningAdmissionRefusalReceiptState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'questioning-admission-refusal-receipt-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $candidateLedgerState) { [string] $candidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentPromotionGateState -ne 'questioning-gel-promotion-gate-ready') {
    $questioningAdmissionRefusalReceiptState = 'awaiting-questioning-gel-promotion-gate'
    $reasonCode = 'questioning-admission-refusal-receipt-promotion-gate-not-ready'
    $nextAction = if ($null -ne $promotionGateState) { [string] $promotionGateState.nextAction } else { 'emit-questioning-gel-promotion-gate' }
} elseif ($currentProtectedPatternState -ne 'protected-questioning-pattern-surface-ready') {
    $questioningAdmissionRefusalReceiptState = 'awaiting-protected-questioning-pattern-surface'
    $reasonCode = 'questioning-admission-refusal-receipt-protected-surface-not-ready'
    $nextAction = if ($null -ne $protectedPatternState) { [string] $protectedPatternState.nextAction } else { 'emit-protected-questioning-pattern-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $questioningAdmissionRefusalReceiptState = 'awaiting-questioning-admission-refusal-binding'
    $reasonCode = 'questioning-admission-refusal-receipt-source-missing'
    $nextAction = 'bind-questioning-admission-refusal-receipt'
} else {
    $questioningAdmissionRefusalReceiptState = 'questioning-admission-refusal-receipt-ready'
    $reasonCode = 'questioning-admission-refusal-receipt-bound'
    $nextAction = 'emit-promotion-seduction-watch'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'questioning-admission-refusal-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'questioning-admission-refusal-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    questioningAdmissionRefusalReceiptState = $questioningAdmissionRefusalReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    variationTestedReentryLedgerState = $currentVariationState
    questioningOperatorCandidateLedgerState = $currentCandidateLedgerState
    questioningGelPromotionGateState = $currentPromotionGateState
    protectedQuestioningPatternSurfaceState = $currentProtectedPatternState
    refusedPatternCount = 1
    deferredPatternCount = 0
    refusalReasonCount = 3
    attractiveButUnderEvidencedDenied = $true
    archiveProtectionPreserved = $true
    delayWithoutDisposalAllowed = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Questioning Admission Refusal Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.questioningAdmissionRefusalReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Promotion-gate state: `{0}`' -f $(if ($payload.questioningGelPromotionGateState) { $payload.questioningGelPromotionGateState } else { 'missing' })),
    ('- Protected questioning-pattern state: `{0}`' -f $(if ($payload.protectedQuestioningPatternSurfaceState) { $payload.protectedQuestioningPatternSurfaceState } else { 'missing' })),
    ('- Refused pattern count: `{0}`' -f $payload.refusedPatternCount),
    ('- Deferred pattern count: `{0}`' -f $payload.deferredPatternCount),
    ('- Refusal-reason count: `{0}`' -f $payload.refusalReasonCount),
    ('- Attractive but under-evidenced denied: `{0}`' -f [bool] $payload.attractiveButUnderEvidencedDenied),
    ('- Archive protection preserved: `{0}`' -f [bool] $payload.archiveProtectionPreserved),
    ('- Delay without disposal allowed: `{0}`' -f [bool] $payload.delayWithoutDisposalAllowed),
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
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    protectedQuestioningPatternSurfaceState = $payload.protectedQuestioningPatternSurfaceState
    refusedPatternCount = $payload.refusedPatternCount
    deferredPatternCount = $payload.deferredPatternCount
    refusalReasonCount = $payload.refusalReasonCount
    attractiveButUnderEvidencedDenied = $payload.attractiveButUnderEvidencedDenied
    archiveProtectionPreserved = $payload.archiveProtectionPreserved
    delayWithoutDisposalAllowed = $payload.delayWithoutDisposalAllowed
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
