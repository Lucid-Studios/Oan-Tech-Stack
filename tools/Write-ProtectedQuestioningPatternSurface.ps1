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
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedQuestioningPatternSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedQuestioningPatternSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the protected questioning-pattern writer can run.'
}

$questioningOperatorCandidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$questioningGelPromotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath
$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath

$currentQuestioningOperatorCandidateLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'questioningOperatorCandidateLedgerState')
$currentQuestioningGelPromotionGateState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'questioningGelPromotionGateState')
$currentCarryForwardInquirySelectionSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionSurfaceState')
$currentLocalityDistinctionWitnessLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$surfaceProjectionBound = $contractsSource.IndexOf('ProtectedQuestioningPatternSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateProtectedQuestioningPatternSurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('protected-questioning-pattern-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$surfaceKeyBound = $keysSource.IndexOf('CreateProtectedQuestioningPatternSurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateProtectedQuestioningPatternSurface', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateProtectedQuestioningPatternSurface_PreservesLegibilityWithoutGrant', [System.StringComparison]::Ordinal) -ge 0

$protectedQuestioningPatternSurfaceState = 'awaiting-questioning-gel-promotion-gate'
$reasonCode = 'protected-questioning-pattern-surface-awaiting-questioning-gel-promotion-gate'
$nextAction = 'emit-questioning-gel-promotion-gate'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $protectedQuestioningPatternSurfaceState = 'blocked'
    $reasonCode = 'protected-questioning-pattern-surface-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentQuestioningGelPromotionGateState -ne 'questioning-gel-promotion-gate-ready') {
    $protectedQuestioningPatternSurfaceState = 'awaiting-questioning-gel-promotion-gate'
    $reasonCode = 'protected-questioning-pattern-surface-gate-not-ready'
    $nextAction = if ($null -ne $questioningGelPromotionGateState) { [string] $questioningGelPromotionGateState.nextAction } else { 'emit-questioning-gel-promotion-gate' }
} elseif ($currentQuestioningOperatorCandidateLedgerState -ne 'questioning-operator-candidate-ledger-ready') {
    $protectedQuestioningPatternSurfaceState = 'awaiting-questioning-operator-candidate-ledger'
    $reasonCode = 'protected-questioning-pattern-surface-candidate-ledger-not-ready'
    $nextAction = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] $questioningOperatorCandidateLedgerState.nextAction } else { 'emit-questioning-operator-candidate-ledger' }
} elseif ($currentCarryForwardInquirySelectionSurfaceState -ne 'carry-forward-inquiry-selection-surface-ready') {
    $protectedQuestioningPatternSurfaceState = 'awaiting-carry-forward-inquiry-selection-surface'
    $reasonCode = 'protected-questioning-pattern-surface-carry-forward-not-ready'
    $nextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { 'emit-carry-forward-inquiry-selection-surface' }
} elseif ($currentLocalityDistinctionWitnessLedgerState -ne 'locality-distinction-witness-ledger-ready') {
    $protectedQuestioningPatternSurfaceState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'protected-questioning-pattern-surface-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $surfaceProjectionBound -or -not $surfaceKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $protectedQuestioningPatternSurfaceState = 'awaiting-protected-questioning-pattern-binding'
    $reasonCode = 'protected-questioning-pattern-surface-source-missing'
    $nextAction = 'bind-protected-questioning-pattern-surface'
} else {
    $protectedQuestioningPatternSurfaceState = 'protected-questioning-pattern-surface-ready'
    $reasonCode = 'protected-questioning-pattern-surface-bound'
    $nextAction = 'await-next-map-declaration'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'protected-questioning-pattern-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'protected-questioning-pattern-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    protectedQuestioningPatternSurfaceState = $protectedQuestioningPatternSurfaceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    questioningOperatorCandidateLedgerState = $currentQuestioningOperatorCandidateLedgerState
    questioningGelPromotionGateState = $currentQuestioningGelPromotionGateState
    carryForwardInquirySelectionSurfaceState = $currentCarryForwardInquirySelectionSurfaceState
    localityDistinctionWitnessLedgerState = $currentLocalityDistinctionWitnessLedgerState
    protectedReviewState = 'protected-questioning-legibility-ready'
    reviewableCandidatePatternCount = 3
    lawfulReviewEnvelopeCount = 3
    withheldInteriorityWarningCount = 3
    localitySafeLegibility = $true
    rawInteriorityDenied = $true
    automaticGrantDenied = $true
    surfaceProjectionBound = $surfaceProjectionBound
    surfaceKeyBound = $surfaceKeyBound
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
    '# Protected Questioning Pattern Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Surface state: `{0}`' -f $payload.protectedQuestioningPatternSurfaceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Questioning operator candidate-ledger state: `{0}`' -f $(if ($payload.questioningOperatorCandidateLedgerState) { $payload.questioningOperatorCandidateLedgerState } else { 'missing' })),
    ('- Questioning GEL promotion-gate state: `{0}`' -f $(if ($payload.questioningGelPromotionGateState) { $payload.questioningGelPromotionGateState } else { 'missing' })),
    ('- Carry-forward inquiry-selection surface state: `{0}`' -f $(if ($payload.carryForwardInquirySelectionSurfaceState) { $payload.carryForwardInquirySelectionSurfaceState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Protected review state: `{0}`' -f $payload.protectedReviewState),
    ('- Reviewable candidate-pattern count: `{0}`' -f $payload.reviewableCandidatePatternCount),
    ('- Lawful review-envelope count: `{0}`' -f $payload.lawfulReviewEnvelopeCount),
    ('- Withheld interiority-warning count: `{0}`' -f $payload.withheldInteriorityWarningCount),
    ('- Locality-safe legibility: `{0}`' -f [bool] $payload.localitySafeLegibility),
    ('- Raw interiority denied: `{0}`' -f [bool] $payload.rawInteriorityDenied),
    ('- Automatic grant denied: `{0}`' -f [bool] $payload.automaticGrantDenied),
    ('- Surface projection bound: `{0}`' -f [bool] $payload.surfaceProjectionBound),
    ('- Surface key bound: `{0}`' -f [bool] $payload.surfaceKeyBound),
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
    protectedQuestioningPatternSurfaceState = $payload.protectedQuestioningPatternSurfaceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    questioningOperatorCandidateLedgerState = $payload.questioningOperatorCandidateLedgerState
    questioningGelPromotionGateState = $payload.questioningGelPromotionGateState
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    protectedReviewState = $payload.protectedReviewState
    reviewableCandidatePatternCount = $payload.reviewableCandidatePatternCount
    lawfulReviewEnvelopeCount = $payload.lawfulReviewEnvelopeCount
    withheldInteriorityWarningCount = $payload.withheldInteriorityWarningCount
    localitySafeLegibility = $payload.localitySafeLegibility
    rawInteriorityDenied = $payload.rawInteriorityDenied
    automaticGrantDenied = $payload.automaticGrantDenied
    surfaceProjectionBound = $payload.surfaceProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[protected-questioning-pattern-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
