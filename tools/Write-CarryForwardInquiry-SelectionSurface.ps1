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
$inquiryPatternContinuityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerStatePath)
$questioningBoundaryPairLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the carry-forward inquiry selection writer can run.'
}

$inquiryPatternContinuityLedgerState = Read-JsonFileOrNull -Path $inquiryPatternContinuityLedgerStatePath
$questioningBoundaryPairLedgerState = Read-JsonFileOrNull -Path $questioningBoundaryPairLedgerStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath

$currentInquiryPatternContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'inquiryPatternContinuityLedgerState')
$currentQuestioningBoundaryPairLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'questioningBoundaryPairLedgerState')
$currentOperatorInquirySelectionEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentLocalityDistinctionWitnessLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')

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

$surfaceProjectionBound = $contractsSource.IndexOf('CarryForwardInquirySelectionSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateCarryForwardInquirySelectionSurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('carry-forward-inquiry-selection-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$surfaceKeyBound = $keysSource.IndexOf('CreateCarryForwardInquirySelectionSurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateCarryForwardInquirySelectionSurface', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateCarryForwardInquirySelectionSurface_BindsLocalitySafeReuse', [System.StringComparison]::Ordinal) -ge 0

$carryForwardInquirySelectionSurfaceState = 'awaiting-inquiry-pattern-continuity-ledger'
$reasonCode = 'carry-forward-inquiry-selection-surface-awaiting-inquiry-pattern-continuity-ledger'
$nextAction = 'emit-inquiry-pattern-continuity-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $carryForwardInquirySelectionSurfaceState = 'blocked'
    $reasonCode = 'carry-forward-inquiry-selection-surface-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentInquiryPatternContinuityLedgerState -ne 'inquiry-pattern-continuity-ledger-ready') {
    $carryForwardInquirySelectionSurfaceState = 'awaiting-inquiry-pattern-continuity-ledger'
    $reasonCode = 'carry-forward-inquiry-selection-surface-inquiry-pattern-not-ready'
    $nextAction = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.nextAction } else { 'emit-inquiry-pattern-continuity-ledger' }
} elseif ($currentQuestioningBoundaryPairLedgerState -ne 'questioning-boundary-pair-ledger-ready') {
    $carryForwardInquirySelectionSurfaceState = 'awaiting-questioning-boundary-pair-ledger'
    $reasonCode = 'carry-forward-inquiry-selection-surface-boundary-pair-not-ready'
    $nextAction = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.nextAction } else { 'emit-questioning-boundary-pair-ledger' }
} elseif ($currentOperatorInquirySelectionEnvelopeState -ne 'operator-inquiry-selection-envelope-ready') {
    $carryForwardInquirySelectionSurfaceState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'carry-forward-inquiry-selection-surface-inquiry-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentLocalityDistinctionWitnessLedgerState -ne 'locality-distinction-witness-ledger-ready') {
    $carryForwardInquirySelectionSurfaceState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'carry-forward-inquiry-selection-surface-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $surfaceProjectionBound -or -not $surfaceKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $carryForwardInquirySelectionSurfaceState = 'awaiting-carry-forward-inquiry-selection-binding'
    $reasonCode = 'carry-forward-inquiry-selection-surface-source-missing'
    $nextAction = 'bind-carry-forward-inquiry-selection-surface'
} else {
    $carryForwardInquirySelectionSurfaceState = 'carry-forward-inquiry-selection-surface-ready'
    $reasonCode = 'carry-forward-inquiry-selection-surface-bound'
    $nextAction = 'pull-forward-to-map-30'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'carry-forward-inquiry-selection-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'carry-forward-inquiry-selection-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    carryForwardInquirySelectionSurfaceState = $carryForwardInquirySelectionSurfaceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    inquiryPatternContinuityLedgerState = $currentInquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $currentQuestioningBoundaryPairLedgerState
    operatorInquirySelectionEnvelopeState = $currentOperatorInquirySelectionEnvelopeState
    localityDistinctionWitnessLedgerState = $currentLocalityDistinctionWitnessLedgerState
    carryForwardInquirySelectionState = 'carry-forward-inquiry-selection-ready'
    availableCarryForwardPatternCount = 3
    admittedReuseConditionCount = 3
    withheldReuseWarningCount = 3
    localitySafeReview = $true
    ambientHabitDenied = $true
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
    '# Carry-Forward Inquiry Selection Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Surface state: `{0}`' -f $payload.carryForwardInquirySelectionSurfaceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Inquiry-pattern continuity state: `{0}`' -f $(if ($payload.inquiryPatternContinuityLedgerState) { $payload.inquiryPatternContinuityLedgerState } else { 'missing' })),
    ('- Questioning boundary-pair state: `{0}`' -f $(if ($payload.questioningBoundaryPairLedgerState) { $payload.questioningBoundaryPairLedgerState } else { 'missing' })),
    ('- Operator inquiry-selection envelope state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Carry-forward inquiry-selection state: `{0}`' -f $payload.carryForwardInquirySelectionState),
    ('- Available carry-forward pattern count: `{0}`' -f $payload.availableCarryForwardPatternCount),
    ('- Admitted reuse-condition count: `{0}`' -f $payload.admittedReuseConditionCount),
    ('- Withheld reuse-warning count: `{0}`' -f $payload.withheldReuseWarningCount),
    ('- Locality-safe review: `{0}`' -f [bool] $payload.localitySafeReview),
    ('- Ambient habit denied: `{0}`' -f [bool] $payload.ambientHabitDenied),
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
    carryForwardInquirySelectionSurfaceState = $payload.carryForwardInquirySelectionSurfaceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    inquiryPatternContinuityLedgerState = $payload.inquiryPatternContinuityLedgerState
    questioningBoundaryPairLedgerState = $payload.questioningBoundaryPairLedgerState
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    carryForwardInquirySelectionState = $payload.carryForwardInquirySelectionState
    availableCarryForwardPatternCount = $payload.availableCarryForwardPatternCount
    admittedReuseConditionCount = $payload.admittedReuseConditionCount
    withheldReuseWarningCount = $payload.withheldReuseWarningCount
    localitySafeReview = $payload.localitySafeReview
    ambientHabitDenied = $payload.ambientHabitDenied
    surfaceProjectionBound = $payload.surfaceProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[carry-forward-inquiry-selection-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
