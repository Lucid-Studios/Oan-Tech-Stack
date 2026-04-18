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
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$inquiryPatternContinuityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerStatePath)
$questioningBoundaryPairLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$mutualIntelligibilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the engram-distance classification writer can run.'
}

$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath
$inquiryPatternContinuityLedgerState = Read-JsonFileOrNull -Path $inquiryPatternContinuityLedgerStatePath
$questioningBoundaryPairLedgerState = Read-JsonFileOrNull -Path $questioningBoundaryPairLedgerStatePath
$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$mutualIntelligibilityWitnessState = Read-JsonFileOrNull -Path $mutualIntelligibilityWitnessStatePath

$currentCarryForwardInquirySelectionSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionSurfaceState')
$currentInquiryPatternContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'inquiryPatternContinuityLedgerState')
$currentQuestioningBoundaryPairLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'questioningBoundaryPairLedgerState')
$currentContinuityUnderPressureLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentMutualIntelligibilityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'mutualIntelligibilityWitnessState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$classificationProjectionBound = $contractsSource.IndexOf('EngramDistanceClass', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('EngramDistanceClassificationLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateEngramDistanceClassificationLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('engram-distance-classification-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$classificationKeyBound = $keysSource.IndexOf('CreateEngramDistanceClassificationLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateEngramDistanceClassificationLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateEngramDistanceAdmissionSurface_ScalesPromotionBurdenByRootDistance', [System.StringComparison]::Ordinal) -ge 0

$engramDistanceClassificationLedgerState = 'awaiting-carry-forward-inquiry-selection-surface'
$reasonCode = 'engram-distance-classification-ledger-awaiting-carry-forward'
$nextAction = 'emit-carry-forward-inquiry-selection-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $engramDistanceClassificationLedgerState = 'blocked'
    $reasonCode = 'engram-distance-classification-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentCarryForwardInquirySelectionSurfaceState -ne 'carry-forward-inquiry-selection-surface-ready') {
    $engramDistanceClassificationLedgerState = 'awaiting-carry-forward-inquiry-selection-surface'
    $reasonCode = 'engram-distance-classification-ledger-carry-forward-not-ready'
    $nextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { 'emit-carry-forward-inquiry-selection-surface' }
} elseif ($currentInquiryPatternContinuityLedgerState -ne 'inquiry-pattern-continuity-ledger-ready') {
    $engramDistanceClassificationLedgerState = 'awaiting-inquiry-pattern-continuity-ledger'
    $reasonCode = 'engram-distance-classification-ledger-inquiry-pattern-not-ready'
    $nextAction = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.nextAction } else { 'emit-inquiry-pattern-continuity-ledger' }
} elseif ($currentQuestioningBoundaryPairLedgerState -ne 'questioning-boundary-pair-ledger-ready') {
    $engramDistanceClassificationLedgerState = 'awaiting-questioning-boundary-pair-ledger'
    $reasonCode = 'engram-distance-classification-ledger-boundary-pair-not-ready'
    $nextAction = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.nextAction } else { 'emit-questioning-boundary-pair-ledger' }
} elseif ($currentContinuityUnderPressureLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $engramDistanceClassificationLedgerState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'engram-distance-classification-ledger-continuity-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentMutualIntelligibilityWitnessState -ne 'mutual-intelligibility-witness-ready') {
    $engramDistanceClassificationLedgerState = 'awaiting-mutual-intelligibility-witness'
    $reasonCode = 'engram-distance-classification-ledger-mutual-intelligibility-not-ready'
    $nextAction = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.nextAction } else { 'emit-mutual-intelligibility-witness' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $classificationProjectionBound -or -not $classificationKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $engramDistanceClassificationLedgerState = 'awaiting-engram-distance-classification-binding'
    $reasonCode = 'engram-distance-classification-ledger-source-missing'
    $nextAction = 'bind-engram-distance-classification-ledger'
} else {
    $engramDistanceClassificationLedgerState = 'engram-distance-classification-ledger-ready'
    $reasonCode = 'engram-distance-classification-ledger-bound'
    $nextAction = 'emit-engram-promotion-requirements-matrix'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'engram-distance-classification-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'engram-distance-classification-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    engramDistanceClassificationLedgerState = $engramDistanceClassificationLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    carryForwardInquirySelectionSurfaceState = $currentCarryForwardInquirySelectionSurfaceState
    inquiryPatternContinuityLedgerState = $currentInquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $currentQuestioningBoundaryPairLedgerState
    continuityUnderPressureLedgerState = $currentContinuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $currentMutualIntelligibilityWitnessState
    dominantDistanceClass = 'AdjacentRoot'
    coRootPatternCount = 0
    adjacentRootPatternCount = 3
    firstOrderOtherPatternCount = 0
    farOtherArtifactCount = 0
    promotionFromFarOtherDenied = $true
    reRootingRequiredForFarOther = $false
    classificationProjectionBound = $classificationProjectionBound
    classificationKeyBound = $classificationKeyBound
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
    '# Engram Distance Classification Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.engramDistanceClassificationLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Dominant distance class: `{0}`' -f $payload.dominantDistanceClass),
    ('- Carry-forward inquiry-selection surface state: `{0}`' -f $(if ($payload.carryForwardInquirySelectionSurfaceState) { $payload.carryForwardInquirySelectionSurfaceState } else { 'missing' })),
    ('- Inquiry-pattern continuity state: `{0}`' -f $(if ($payload.inquiryPatternContinuityLedgerState) { $payload.inquiryPatternContinuityLedgerState } else { 'missing' })),
    ('- Questioning boundary-pair state: `{0}`' -f $(if ($payload.questioningBoundaryPairLedgerState) { $payload.questioningBoundaryPairLedgerState } else { 'missing' })),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Mutual-intelligibility witness state: `{0}`' -f $(if ($payload.mutualIntelligibilityWitnessState) { $payload.mutualIntelligibilityWitnessState } else { 'missing' })),
    ('- Co-root pattern count: `{0}`' -f $payload.coRootPatternCount),
    ('- Adjacent-root pattern count: `{0}`' -f $payload.adjacentRootPatternCount),
    ('- First-order-other pattern count: `{0}`' -f $payload.firstOrderOtherPatternCount),
    ('- Far-other artifact count: `{0}`' -f $payload.farOtherArtifactCount),
    ('- Promotion from far-other denied: `{0}`' -f [bool] $payload.promotionFromFarOtherDenied),
    ('- Re-rooting required for far-other: `{0}`' -f [bool] $payload.reRootingRequiredForFarOther),
    ('- Classification projection bound: `{0}`' -f [bool] $payload.classificationProjectionBound),
    ('- Classification key bound: `{0}`' -f [bool] $payload.classificationKeyBound),
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
    engramDistanceClassificationLedgerState = $payload.engramDistanceClassificationLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    inquiryPatternContinuityLedgerState = $payload.inquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $payload.questioningBoundaryPairLedgerState
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $payload.mutualIntelligibilityWitnessState
    dominantDistanceClass = $payload.dominantDistanceClass
    coRootPatternCount = $payload.coRootPatternCount
    adjacentRootPatternCount = $payload.adjacentRootPatternCount
    firstOrderOtherPatternCount = $payload.firstOrderOtherPatternCount
    farOtherArtifactCount = $payload.farOtherArtifactCount
    promotionFromFarOtherDenied = $payload.promotionFromFarOtherDenied
    reRootingRequiredForFarOther = $payload.reRootingRequiredForFarOther
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
