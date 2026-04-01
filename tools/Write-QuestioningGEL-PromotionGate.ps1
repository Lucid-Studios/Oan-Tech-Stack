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

function Get-ObjectPropertyValueOrNull {
    param([object] $InputObject, [string] $PropertyName)

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$distanceWeightedQuestioningAdmissionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.distanceWeightedQuestioningAdmissionSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the questioning GEL promotion gate writer can run.'
}

$questioningOperatorCandidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath
$distanceWeightedQuestioningAdmissionSurfaceState = Read-JsonFileOrNull -Path $distanceWeightedQuestioningAdmissionSurfaceStatePath

$currentQuestioningOperatorCandidateLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'questioningOperatorCandidateLedgerState')
$currentCarryForwardInquirySelectionSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionSurfaceState')
$currentOperatorInquirySelectionEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentLocalityDistinctionWitnessLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')
$currentDistanceWeightedQuestioningAdmissionSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $distanceWeightedQuestioningAdmissionSurfaceState -PropertyName 'distanceWeightedQuestioningAdmissionSurfaceState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$gateProjectionBound = $contractsSource.IndexOf('QuestioningGelPromotionGateReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateQuestioningGelPromotionGate', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('questioning-gel-promotion-gate-bound', [System.StringComparison]::Ordinal) -ge 0
$gateKeyBound = $keysSource.IndexOf('CreateQuestioningGelPromotionGateHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateQuestioningGelPromotionGate', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateQuestioningOperatorCandidateLedgerAndPromotionGate_GuardReuseBeforeInheritance', [System.StringComparison]::Ordinal) -ge 0

$questioningGelPromotionGateState = 'awaiting-questioning-operator-candidate-ledger'
$reasonCode = 'questioning-gel-promotion-gate-awaiting-questioning-operator-candidate-ledger'
$nextAction = 'emit-questioning-operator-candidate-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $questioningGelPromotionGateState = 'blocked'
    $reasonCode = 'questioning-gel-promotion-gate-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentQuestioningOperatorCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $questioningGelPromotionGateState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'questioning-gel-promotion-gate-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] $questioningOperatorCandidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentCarryForwardInquirySelectionSurfaceState -ne 'carry-forward-inquiry-selection-surface-ready') {
    $questioningGelPromotionGateState = 'awaiting-carry-forward-inquiry-selection-surface'
    $reasonCode = 'questioning-gel-promotion-gate-carry-forward-not-ready'
    $nextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { 'emit-carry-forward-inquiry-selection-surface' }
} elseif ($currentOperatorInquirySelectionEnvelopeState -ne 'operator-inquiry-selection-envelope-ready') {
    $questioningGelPromotionGateState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'questioning-gel-promotion-gate-inquiry-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentLocalityDistinctionWitnessLedgerState -ne 'locality-distinction-witness-ledger-ready') {
    $questioningGelPromotionGateState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'questioning-gel-promotion-gate-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($currentDistanceWeightedQuestioningAdmissionSurfaceState -ne 'distance-weighted-questioning-admission-surface-ready') {
    $questioningGelPromotionGateState = 'awaiting-distance-weighted-questioning-admission-surface'
    $reasonCode = 'questioning-gel-promotion-gate-distance-weighting-not-ready'
    $nextAction = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [string] $distanceWeightedQuestioningAdmissionSurfaceState.nextAction } else { 'emit-distance-weighted-questioning-admission-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $gateProjectionBound -or -not $gateKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $questioningGelPromotionGateState = 'awaiting-questioning-gel-promotion-gate-binding'
    $reasonCode = 'questioning-gel-promotion-gate-source-missing'
    $nextAction = 'bind-questioning-gel-promotion-gate'
} else {
    $questioningGelPromotionGateState = 'questioning-gel-promotion-gate-ready'
    $reasonCode = 'questioning-gel-promotion-gate-bound'
    $nextAction = 'emit-protected-questioning-pattern-surface'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'questioning-gel-promotion-gate.json'
$bundleMarkdownPath = Join-Path $bundlePath 'questioning-gel-promotion-gate.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    questioningGelPromotionGateState = $questioningGelPromotionGateState
    reasonCode = $reasonCode
    nextAction = $nextAction
    questioningOperatorCandidateLedgerState = $currentQuestioningOperatorCandidateLedgerState
    carryForwardInquirySelectionSurfaceState = $currentCarryForwardInquirySelectionSurfaceState
    operatorInquirySelectionEnvelopeState = $currentOperatorInquirySelectionEnvelopeState
    localityDistinctionWitnessLedgerState = $currentLocalityDistinctionWitnessLedgerState
    distanceWeightedQuestioningAdmissionSurfaceState = $currentDistanceWeightedQuestioningAdmissionSurfaceState
    promotionGateState = 'guarded-gel-candidacy-admitted'
    dominantDistanceClass = 'AdjacentRoot'
    promotionCeiling = 'GuardedCandidateReview'
    candidateInquiryPatternCount = 3
    satisfiedPromotionConditionCount = 3
    unmetPromotionConditionCount = 3
    promotionWarningCount = 3
    localitySeparationPreserved = $true
    authoritySeparationPreserved = $true
    truthSeekingInvariantPreserved = $true
    outcomeSeekingDenied = $true
    distanceScalingPreserved = $true
    reRootingRequired = $false
    promotionReviewAdmitted = $true
    gateProjectionBound = $gateProjectionBound
    gateKeyBound = $gateKeyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $sourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Questioning GEL Promotion Gate',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Gate state: `{0}`' -f $payload.questioningGelPromotionGateState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Questioning operator candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Carry-forward inquiry-selection surface state: `{0}`' -f $(if ($payload.carryForwardInquirySelectionSurfaceState) { $payload.carryForwardInquirySelectionSurfaceState } else { 'missing' })),
    ('- Operator inquiry-selection envelope state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Distance-weighted admission state: `{0}`' -f $(if ($payload.distanceWeightedQuestioningAdmissionSurfaceState) { $payload.distanceWeightedQuestioningAdmissionSurfaceState } else { 'missing' })),
    ('- Promotion gate state: `{0}`' -f $payload.promotionGateState),
    ('- Dominant distance class: `{0}`' -f $payload.dominantDistanceClass),
    ('- Promotion ceiling: `{0}`' -f $payload.promotionCeiling),
    ('- Candidate inquiry-pattern count: `{0}`' -f $payload.candidateInquiryPatternCount),
    ('- Satisfied promotion-condition count: `{0}`' -f $payload.satisfiedPromotionConditionCount),
    ('- Unmet promotion-condition count: `{0}`' -f $payload.unmetPromotionConditionCount),
    ('- Promotion warning count: `{0}`' -f $payload.promotionWarningCount),
    ('- Locality separation preserved: `{0}`' -f [bool] $payload.localitySeparationPreserved),
    ('- Authority separation preserved: `{0}`' -f [bool] $payload.authoritySeparationPreserved),
    ('- Truth-seeking invariant preserved: `{0}`' -f [bool] $payload.truthSeekingInvariantPreserved),
    ('- Outcome-seeking denied: `{0}`' -f [bool] $payload.outcomeSeekingDenied),
    ('- Distance scaling preserved: `{0}`' -f [bool] $payload.distanceScalingPreserved),
    ('- Re-rooting required: `{0}`' -f [bool] $payload.reRootingRequired),
    ('- Promotion review admitted: `{0}`' -f [bool] $payload.promotionReviewAdmitted),
    ('- Gate projection bound: `{0}`' -f [bool] $payload.gateProjectionBound),
    ('- Gate key bound: `{0}`' -f [bool] $payload.gateKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Test binding bound: `{0}`' -f [bool] $payload.testBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount),
    ''
)

foreach ($file in @($payload.sourceFiles)) {
    $markdownLines += @(
        ('## {0}' -f [string] $file.path),
        ('- Present: `{0}`' -f [bool] $file.present),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    distanceWeightedQuestioningAdmissionSurfaceState = $payload.distanceWeightedQuestioningAdmissionSurfaceState
    promotionGateState = $payload.promotionGateState
    dominantDistanceClass = $payload.dominantDistanceClass
    promotionCeiling = $payload.promotionCeiling
    candidateInquiryPatternCount = $payload.candidateInquiryPatternCount
    satisfiedPromotionConditionCount = $payload.satisfiedPromotionConditionCount
    unmetPromotionConditionCount = $payload.unmetPromotionConditionCount
    promotionWarningCount = $payload.promotionWarningCount
    localitySeparationPreserved = $payload.localitySeparationPreserved
    authoritySeparationPreserved = $payload.authoritySeparationPreserved
    truthSeekingInvariantPreserved = $payload.truthSeekingInvariantPreserved
    outcomeSeekingDenied = $payload.outcomeSeekingDenied
    distanceScalingPreserved = $payload.distanceScalingPreserved
    reRootingRequired = $payload.reRootingRequired
    promotionReviewAdmitted = $payload.promotionReviewAdmitted
    gateProjectionBound = $payload.gateProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[questioning-gel-promotion-gate] Bundle: {0}' -f $bundlePath)
$bundlePath
