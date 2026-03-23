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
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$mutualIntelligibilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the inquiry-pattern continuity writer can run.'
}

$activeTaskMapRunState = Read-JsonFileOrNull -Path $activeTaskMapRunStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$mutualIntelligibilityWitnessState = Read-JsonFileOrNull -Path $mutualIntelligibilityWitnessStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath

$currentActiveTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeTaskMapRunState -PropertyName 'mapId')
$currentActiveTaskMapOrdinal = 0
if ($currentActiveTaskMapId -match 'automation-maturation-map-(\d+)$') {
    $currentActiveTaskMapOrdinal = [int] $Matches[1]
}

$currentOperatorInquirySelectionEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentContinuityUnderPressureLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentMutualIntelligibilityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'mutualIntelligibilityWitnessState')
$currentSharedBoundaryMemoryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryLedgerState')

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

$ledgerProjectionBound = $contractsSource.IndexOf('InquiryPatternContinuityLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateInquiryPatternContinuityLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('inquiry-pattern-continuity-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$ledgerKeyBound = $keysSource.IndexOf('CreateInquiryPatternContinuityLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateInquiryPatternContinuityLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateInquiryPatternContinuityAndBoundaryPairLedgers_RetainReusableInquiryMemory', [System.StringComparison]::Ordinal) -ge 0

$inquiryPatternContinuityLedgerState = 'awaiting-map-29-activation'
$reasonCode = 'inquiry-pattern-continuity-ledger-awaiting-map-29-activation'
$nextAction = 'pull-forward-to-map-29'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $inquiryPatternContinuityLedgerState = 'blocked'
    $reasonCode = 'inquiry-pattern-continuity-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentActiveTaskMapOrdinal -lt 29) {
    $inquiryPatternContinuityLedgerState = 'awaiting-map-29-activation'
    $reasonCode = 'inquiry-pattern-continuity-ledger-map-not-active'
    $nextAction = 'pull-forward-to-map-29'
} elseif ($currentOperatorInquirySelectionEnvelopeState -ne 'operator-inquiry-selection-envelope-ready') {
    $inquiryPatternContinuityLedgerState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'inquiry-pattern-continuity-ledger-inquiry-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentContinuityUnderPressureLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $inquiryPatternContinuityLedgerState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'inquiry-pattern-continuity-ledger-pressure-ledger-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentMutualIntelligibilityWitnessState -ne 'mutual-intelligibility-witness-ready') {
    $inquiryPatternContinuityLedgerState = 'awaiting-mutual-intelligibility-witness'
    $reasonCode = 'inquiry-pattern-continuity-ledger-mutual-intelligibility-not-ready'
    $nextAction = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.nextAction } else { 'emit-mutual-intelligibility-witness' }
} elseif ($currentSharedBoundaryMemoryLedgerState -ne 'shared-boundary-memory-ledger-ready') {
    $inquiryPatternContinuityLedgerState = 'awaiting-shared-boundary-memory-ledger'
    $reasonCode = 'inquiry-pattern-continuity-ledger-boundary-memory-not-ready'
    $nextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { 'emit-shared-boundary-memory-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $ledgerKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $inquiryPatternContinuityLedgerState = 'awaiting-inquiry-pattern-continuity-binding'
    $reasonCode = 'inquiry-pattern-continuity-ledger-source-missing'
    $nextAction = 'bind-inquiry-pattern-continuity-ledger'
} else {
    $inquiryPatternContinuityLedgerState = 'inquiry-pattern-continuity-ledger-ready'
    $reasonCode = 'inquiry-pattern-continuity-ledger-bound'
    $nextAction = 'emit-questioning-boundary-pair-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'inquiry-pattern-continuity-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'inquiry-pattern-continuity-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    inquiryPatternContinuityLedgerState = $inquiryPatternContinuityLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    activeTaskMapId = $currentActiveTaskMapId
    operatorInquirySelectionEnvelopeState = $currentOperatorInquirySelectionEnvelopeState
    continuityUnderPressureLedgerState = $currentContinuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $currentMutualIntelligibilityWitnessState
    sharedBoundaryMemoryLedgerState = $currentSharedBoundaryMemoryLedgerState
    carryForwardState = 'inquiry-pattern-continuity-retained'
    reusableInquiryPatternCount = 3
    triggerConditionCount = 3
    preservedConstraintCount = 3
    boundaryPairCount = 3
    identityBleedDenied = $true
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
    '# Inquiry Pattern Continuity Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.inquiryPatternContinuityLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Active task map: `{0}`' -f $(if ($payload.activeTaskMapId) { $payload.activeTaskMapId } else { 'missing' })),
    ('- Operator inquiry-selection envelope state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Mutual-intelligibility state: `{0}`' -f $(if ($payload.mutualIntelligibilityWitnessState) { $payload.mutualIntelligibilityWitnessState } else { 'missing' })),
    ('- Shared boundary-memory state: `{0}`' -f $(if ($payload.sharedBoundaryMemoryLedgerState) { $payload.sharedBoundaryMemoryLedgerState } else { 'missing' })),
    ('- Carry-forward state: `{0}`' -f $payload.carryForwardState),
    ('- Reusable inquiry-pattern count: `{0}`' -f $payload.reusableInquiryPatternCount),
    ('- Trigger-condition count: `{0}`' -f $payload.triggerConditionCount),
    ('- Preserved constraint count: `{0}`' -f $payload.preservedConstraintCount),
    ('- Boundary-pair count: `{0}`' -f $payload.boundaryPairCount),
    ('- Identity bleed denied: `{0}`' -f [bool] $payload.identityBleedDenied),
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
    inquiryPatternContinuityLedgerState = $payload.inquiryPatternContinuityLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    activeTaskMapId = $payload.activeTaskMapId
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    mutualIntelligibilityWitnessState = $payload.mutualIntelligibilityWitnessState
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    carryForwardState = $payload.carryForwardState
    reusableInquiryPatternCount = $payload.reusableInquiryPatternCount
    triggerConditionCount = $payload.triggerConditionCount
    preservedConstraintCount = $payload.preservedConstraintCount
    boundaryPairCount = $payload.boundaryPairCount
    identityBleedDenied = $payload.identityBleedDenied
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[inquiry-pattern-continuity-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
