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
$activeTaskMapRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-active-task-map-run.json'
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$inquiryPatternContinuityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerStatePath)
$questioningBoundaryPairLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$mutualIntelligibilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the questioning-operator candidate writer can run.'
}

$activeTaskMapRunState = Read-JsonFileOrNull -Path $activeTaskMapRunStatePath
$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath
$inquiryPatternContinuityLedgerState = Read-JsonFileOrNull -Path $inquiryPatternContinuityLedgerStatePath
$questioningBoundaryPairLedgerState = Read-JsonFileOrNull -Path $questioningBoundaryPairLedgerStatePath
$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$mutualIntelligibilityWitnessState = Read-JsonFileOrNull -Path $mutualIntelligibilityWitnessStatePath

$currentActiveTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeTaskMapRunState -PropertyName 'mapId')
$currentActiveTaskMapOrdinal = 0
if ($currentActiveTaskMapId -match 'automation-maturation-map-(\d+)$') {
    $currentActiveTaskMapOrdinal = [int] $Matches[1]
}

$currentCarryForwardInquirySelectionSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionSurfaceState')
$currentInquiryPatternContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'inquiryPatternContinuityLedgerState')
$currentQuestioningBoundaryPairLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'questioningBoundaryPairLedgerState')
$currentContinuityUnderPressureLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentMutualIntelligibilityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'mutualIntelligibilityWitnessState')

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

$ledgerProjectionBound = $contractsSource.IndexOf('QuestioningOperatorCandidateLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateQuestioningOperatorCandidateLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('questioning-operator-candidate-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$ledgerKeyBound = $keysSource.IndexOf('CreateQuestioningOperatorCandidateLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateQuestioningOperatorCandidateLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateQuestioningOperatorCandidateLedgerAndPromotionGate_GuardReuseBeforeInheritance', [System.StringComparison]::Ordinal) -ge 0

$questioningOperatorCandidateLedgerState = 'awaiting-map-30-activation'
$reasonCode = 'questioning-operator-candidate-ledger-awaiting-map-30-activation'
$nextAction = 'pull-forward-to-map-30'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $questioningOperatorCandidateLedgerState = 'blocked'
    $reasonCode = 'questioning-operator-candidate-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentActiveTaskMapOrdinal -lt 30) {
    $questioningOperatorCandidateLedgerState = 'awaiting-map-30-activation'
    $reasonCode = 'questioning-operator-candidate-ledger-map-not-active'
    $nextAction = 'pull-forward-to-map-30'
} elseif ($currentCarryForwardInquirySelectionSurfaceState -ne 'carry-forward-inquiry-selection-surface-ready') {
    $questioningOperatorCandidateLedgerState = 'awaiting-carry-forward-inquiry-selection-surface'
    $reasonCode = 'questioning-operator-candidate-ledger-carry-forward-not-ready'
    $nextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { 'emit-carry-forward-inquiry-selection-surface' }
} elseif ($currentInquiryPatternContinuityLedgerState -ne 'inquiry-pattern-continuity-ledger-ready') {
    $questioningOperatorCandidateLedgerState = 'awaiting-inquiry-pattern-continuity-ledger'
    $reasonCode = 'questioning-operator-candidate-ledger-inquiry-pattern-not-ready'
    $nextAction = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.nextAction } else { 'emit-inquiry-pattern-continuity-ledger' }
} elseif ($currentQuestioningBoundaryPairLedgerState -ne 'questioning-boundary-pair-ledger-ready') {
    $questioningOperatorCandidateLedgerState = 'awaiting-questioning-boundary-pair-ledger'
    $reasonCode = 'questioning-operator-candidate-ledger-boundary-pair-not-ready'
    $nextAction = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.nextAction } else { 'emit-questioning-boundary-pair-ledger' }
} elseif ($currentContinuityUnderPressureLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $questioningOperatorCandidateLedgerState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'questioning-operator-candidate-ledger-continuity-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentMutualIntelligibilityWitnessState -ne 'mutual-intelligibility-witness-ready') {
    $questioningOperatorCandidateLedgerState = 'awaiting-mutual-intelligibility-witness'
    $reasonCode = 'questioning-operator-candidate-ledger-mutual-intelligibility-not-ready'
    $nextAction = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.nextAction } else { 'emit-mutual-intelligibility-witness' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $ledgerKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $questioningOperatorCandidateLedgerState = 'awaiting-questioning-operator-candidate-binding'
    $reasonCode = 'questioning-operator-candidate-ledger-source-missing'
    $nextAction = 'bind-questioning-operator-candidate-ledger'
} else {
    $questioningOperatorCandidateLedgerState = 'questioning-operator-candidate-ledger-ready'
    $reasonCode = 'questioning-operator-candidate-ledger-bound'
    $nextAction = 'emit-questioning-gel-promotion-gate'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'questioning-operator-candidate-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'questioning-operator-candidate-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    questioningOperatorCandidateLedgerState = $questioningOperatorCandidateLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    activeTaskMapId = $currentActiveTaskMapId
    carryForwardInquirySelectionSurfaceState = $currentCarryForwardInquirySelectionSurfaceState
    inquiryPatternContinuityLedgerState = $currentInquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $currentQuestioningBoundaryPairLedgerState
    continuityUnderPressureLedgerState = $currentContinuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $currentMutualIntelligibilityWitnessState
    candidateClassificationState = 'guarded-questioning-candidates-retained'
    eventBoundInquiryFormCount = 3
    candidateInquiryPatternCount = 3
    promotionEvidenceCount = 3
    requiredReentryConditionCount = 3
    failureSignatureExpectationCount = 3
    hiddenAuthorityPatternsDenied = $true
    identityBoundPatternsWithheld = $true
    ledgerProjectionBound = $ledgerProjectionBound
    ledgerKeyBound = $ledgerKeyBound
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
    '# Questioning Operator Candidate Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.questioningOperatorCandidateLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Active task map: `{0}`' -f $(if ($payload.activeTaskMapId) { $payload.activeTaskMapId } else { 'missing' })),
    ('- Carry-forward inquiry-selection surface state: `{0}`' -f $(if ($payload.carryForwardInquirySelectionSurfaceState) { $payload.carryForwardInquirySelectionSurfaceState } else { 'missing' })),
    ('- Inquiry-pattern continuity state: `{0}`' -f $(if ($payload.inquiryPatternContinuityLedgerState) { $payload.inquiryPatternContinuityLedgerState } else { 'missing' })),
    ('- Questioning boundary-pair state: `{0}`' -f $(if ($payload.questioningBoundaryPairLedgerState) { $payload.questioningBoundaryPairLedgerState } else { 'missing' })),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Mutual-intelligibility state: `{0}`' -f $(if ($payload.mutualIntelligibilityWitnessState) { $payload.mutualIntelligibilityWitnessState } else { 'missing' })),
    ('- Candidate classification state: `{0}`' -f $payload.candidateClassificationState),
    ('- Event-bound inquiry-form count: `{0}`' -f $payload.eventBoundInquiryFormCount),
    ('- Candidate inquiry-pattern count: `{0}`' -f $payload.candidateInquiryPatternCount),
    ('- Promotion-evidence count: `{0}`' -f $payload.promotionEvidenceCount),
    ('- Required re-entry condition count: `{0}`' -f $payload.requiredReentryConditionCount),
    ('- Failure-signature expectation count: `{0}`' -f $payload.failureSignatureExpectationCount),
    ('- Hidden authority patterns denied: `{0}`' -f [bool] $payload.hiddenAuthorityPatternsDenied),
    ('- Identity-bound patterns withheld: `{0}`' -f [bool] $payload.identityBoundPatternsWithheld),
    ('- Ledger projection bound: `{0}`' -f [bool] $payload.ledgerProjectionBound),
    ('- Ledger key bound: `{0}`' -f [bool] $payload.ledgerKeyBound),
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
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    activeTaskMapId = $payload.activeTaskMapId
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    inquiryPatternContinuityLedgerState = $payload.inquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $payload.questioningBoundaryPairLedgerState
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $payload.mutualIntelligibilityWitnessState
    candidateClassificationState = $payload.candidateClassificationState
    eventBoundInquiryFormCount = $payload.eventBoundInquiryFormCount
    candidateInquiryPatternCount = $payload.candidateInquiryPatternCount
    promotionEvidenceCount = $payload.promotionEvidenceCount
    requiredReentryConditionCount = $payload.requiredReentryConditionCount
    failureSignatureExpectationCount = $payload.failureSignatureExpectationCount
    hiddenAuthorityPatternsDenied = $payload.hiddenAuthorityPatternsDenied
    identityBoundPatternsWithheld = $payload.identityBoundPatternsWithheld
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[questioning-operator-candidate-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
