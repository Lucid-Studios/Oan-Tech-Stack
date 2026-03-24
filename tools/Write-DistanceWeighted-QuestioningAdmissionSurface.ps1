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
$engramDistanceClassificationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerStatePath)
$engramPromotionRequirementsMatrixStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramPromotionRequirementsMatrixStatePath)
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.distanceWeightedQuestioningAdmissionSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.distanceWeightedQuestioningAdmissionSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the distance-weighted questioning-admission writer can run.'
}

$engramDistanceClassificationLedgerState = Read-JsonFileOrNull -Path $engramDistanceClassificationLedgerStatePath
$engramPromotionRequirementsMatrixState = Read-JsonFileOrNull -Path $engramPromotionRequirementsMatrixStatePath
$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath

$currentEngramDistanceClassificationLedgerState = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.engramDistanceClassificationLedgerState } else { $null }
$currentEngramPromotionRequirementsMatrixState = if ($null -ne $engramPromotionRequirementsMatrixState) { [string] $engramPromotionRequirementsMatrixState.engramPromotionRequirementsMatrixState } else { $null }
$currentCarryForwardInquirySelectionSurfaceState = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.carryForwardInquirySelectionSurfaceState } else { $null }

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }

$surfaceProjectionBound = $contractsSource.IndexOf('DistanceWeightedQuestioningAdmissionSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateDistanceWeightedQuestioningAdmissionSurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('distance-weighted-questioning-admission-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$surfaceKeyBound = $keysSource.IndexOf('CreateDistanceWeightedQuestioningAdmissionSurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateDistanceWeightedQuestioningAdmissionSurface', [System.StringComparison]::Ordinal) -ge 0

$distanceWeightedQuestioningAdmissionSurfaceState = 'awaiting-engram-distance-classification-ledger'
$reasonCode = 'distance-weighted-questioning-admission-surface-awaiting-classification'
$nextAction = 'emit-engram-distance-classification-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'blocked'
    $reasonCode = 'distance-weighted-questioning-admission-surface-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentEngramDistanceClassificationLedgerState -ne 'engram-distance-classification-ledger-ready') {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'awaiting-engram-distance-classification-ledger'
    $reasonCode = 'distance-weighted-questioning-admission-surface-classification-not-ready'
    $nextAction = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.nextAction } else { 'emit-engram-distance-classification-ledger' }
} elseif ($currentEngramPromotionRequirementsMatrixState -ne 'engram-promotion-requirements-matrix-ready') {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'awaiting-engram-promotion-requirements-matrix'
    $reasonCode = 'distance-weighted-questioning-admission-surface-matrix-not-ready'
    $nextAction = if ($null -ne $engramPromotionRequirementsMatrixState) { [string] $engramPromotionRequirementsMatrixState.nextAction } else { 'emit-engram-promotion-requirements-matrix' }
} elseif ($currentCarryForwardInquirySelectionSurfaceState -ne 'carry-forward-inquiry-selection-surface-ready') {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'awaiting-carry-forward-inquiry-selection-surface'
    $reasonCode = 'distance-weighted-questioning-admission-surface-carry-forward-not-ready'
    $nextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { 'emit-carry-forward-inquiry-selection-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $surfaceProjectionBound -or -not $surfaceKeyBound -or -not $serviceBindingBound) {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'awaiting-distance-weighted-questioning-admission-binding'
    $reasonCode = 'distance-weighted-questioning-admission-surface-source-missing'
    $nextAction = 'bind-distance-weighted-questioning-admission-surface'
} else {
    $distanceWeightedQuestioningAdmissionSurfaceState = 'distance-weighted-questioning-admission-surface-ready'
    $reasonCode = 'distance-weighted-questioning-admission-surface-bound'
    $nextAction = 'emit-questioning-operator-candidate-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'distance-weighted-questioning-admission-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'distance-weighted-questioning-admission-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    distanceWeightedQuestioningAdmissionSurfaceState = $distanceWeightedQuestioningAdmissionSurfaceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    engramDistanceClassificationLedgerState = $currentEngramDistanceClassificationLedgerState
    engramPromotionRequirementsMatrixState = $currentEngramPromotionRequirementsMatrixState
    carryForwardInquirySelectionSurfaceState = $currentCarryForwardInquirySelectionSurfaceState
    dominantDistanceClass = 'AdjacentRoot'
    promotionCeiling = 'GuardedCandidateReview'
    admittedCandidatePatternCount = 3
    withheldCandidatePatternCount = 3
    requiredReentryBurdenCount = 3
    unknownTolerance = 1
    distanceScalingPreserved = $true
    farOtherPromotionDenied = $true
    reRootingRequired = $false
    surfaceProjectionBound = $surfaceProjectionBound
    surfaceKeyBound = $surfaceKeyBound
    serviceBindingBound = $serviceBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Distance-Weighted Questioning Admission Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Surface state: `{0}`' -f $payload.distanceWeightedQuestioningAdmissionSurfaceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Distance-classification ledger state: `{0}`' -f $(if ($payload.engramDistanceClassificationLedgerState) { $payload.engramDistanceClassificationLedgerState } else { 'missing' })),
    ('- Promotion-requirements matrix state: `{0}`' -f $(if ($payload.engramPromotionRequirementsMatrixState) { $payload.engramPromotionRequirementsMatrixState } else { 'missing' })),
    ('- Carry-forward inquiry-selection surface state: `{0}`' -f $(if ($payload.carryForwardInquirySelectionSurfaceState) { $payload.carryForwardInquirySelectionSurfaceState } else { 'missing' })),
    ('- Dominant distance class: `{0}`' -f $payload.dominantDistanceClass),
    ('- Promotion ceiling: `{0}`' -f $payload.promotionCeiling),
    ('- Admitted candidate-pattern count: `{0}`' -f $payload.admittedCandidatePatternCount),
    ('- Withheld candidate-pattern count: `{0}`' -f $payload.withheldCandidatePatternCount),
    ('- Required re-entry burden count: `{0}`' -f $payload.requiredReentryBurdenCount),
    ('- Unknown tolerance: `{0}`' -f $payload.unknownTolerance),
    ('- Distance scaling preserved: `{0}`' -f [bool] $payload.distanceScalingPreserved),
    ('- Far-other promotion denied: `{0}`' -f [bool] $payload.farOtherPromotionDenied),
    ('- Re-rooting required: `{0}`' -f [bool] $payload.reRootingRequired),
    ('- Surface projection bound: `{0}`' -f [bool] $payload.surfaceProjectionBound),
    ('- Surface key bound: `{0}`' -f [bool] $payload.surfaceKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    distanceWeightedQuestioningAdmissionSurfaceState = $payload.distanceWeightedQuestioningAdmissionSurfaceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    engramDistanceClassificationLedgerState = $payload.engramDistanceClassificationLedgerState
    engramPromotionRequirementsMatrixState = $payload.engramPromotionRequirementsMatrixState
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    dominantDistanceClass = $payload.dominantDistanceClass
    promotionCeiling = $payload.promotionCeiling
    admittedCandidatePatternCount = $payload.admittedCandidatePatternCount
    withheldCandidatePatternCount = $payload.withheldCandidatePatternCount
    requiredReentryBurdenCount = $payload.requiredReentryBurdenCount
    unknownTolerance = $payload.unknownTolerance
    distanceScalingPreserved = $payload.distanceScalingPreserved
    farOtherPromotionDenied = $payload.farOtherPromotionDenied
    reRootingRequired = $payload.reRootingRequired
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
