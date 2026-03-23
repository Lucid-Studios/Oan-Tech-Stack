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
$bondedCoWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$inquirySessionDisciplineSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceStatePath)
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)
$coherenceGainWitnessReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptStatePath)
$nextEraBatchSelectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nextEraBatchSelectorStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the operator inquiry selection writer can run.'
}

$bondedCoWorkSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCoWorkSessionRehearsalStatePath
$activeTaskMapRunState = Read-JsonFileOrNull -Path $activeTaskMapRunStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath
$inquirySessionDisciplineSurfaceState = Read-JsonFileOrNull -Path $inquirySessionDisciplineSurfaceStatePath
$boundaryConditionLedgerState = Read-JsonFileOrNull -Path $boundaryConditionLedgerStatePath
$coherenceGainWitnessReceiptState = Read-JsonFileOrNull -Path $coherenceGainWitnessReceiptStatePath
$nextEraBatchSelectorState = Read-JsonFileOrNull -Path $nextEraBatchSelectorStatePath

$currentCoWorkRehearsalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'rehearsalReceiptState')
$currentLocalityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')
$currentInquirySurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'inquirySurfaceState')
$currentBoundaryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'boundaryLedgerState')
$currentCoherenceWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherenceWitnessState')
$currentNextEraSelectorState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectorState')
$selectedNextMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedNextMapId')
$currentActiveTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeTaskMapRunState -PropertyName 'mapId')
$currentActiveTaskMapOrdinal = 0
if ($currentActiveTaskMapId -match 'automation-maturation-map-(\d+)$') {
    $currentActiveTaskMapOrdinal = [int] $Matches[1]
}

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

$envelopeProjectionBound = $contractsSource.IndexOf('OperatorInquirySelectionEnvelopeReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateOperatorInquirySelectionEnvelope', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('operator-inquiry-selection-envelope-bound', [System.StringComparison]::Ordinal) -ge 0
$envelopeKeyBound = $keysSource.IndexOf('CreateOperatorInquirySelectionEnvelopeHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateOperatorInquirySelectionEnvelope', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateOperatorInquirySelectionEnvelope_BindsBoundaryAwareSelectionAcrossTheBond', [System.StringComparison]::Ordinal) -ge 0

$operatorInquirySelectionEnvelopeState = 'awaiting-bonded-cowork-session-rehearsal'
$reasonCode = 'operator-inquiry-selection-envelope-awaiting-bonded-cowork-session-rehearsal'
$nextAction = 'emit-bonded-cowork-session-rehearsal'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $operatorInquirySelectionEnvelopeState = 'blocked'
    $reasonCode = 'operator-inquiry-selection-envelope-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentCoWorkRehearsalState -ne 'bonded-cowork-session-rehearsal-ready') {
    $operatorInquirySelectionEnvelopeState = 'awaiting-bonded-cowork-session-rehearsal'
    $reasonCode = 'operator-inquiry-selection-envelope-cowork-not-ready'
    $nextAction = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.nextAction } else { 'emit-bonded-cowork-session-rehearsal' }
} elseif ($currentLocalityWitnessState -ne 'locality-distinction-witness-ledger-ready') {
    $operatorInquirySelectionEnvelopeState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'operator-inquiry-selection-envelope-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($currentInquirySurfaceState -ne 'inquiry-session-discipline-ready') {
    $operatorInquirySelectionEnvelopeState = 'awaiting-inquiry-session-discipline-surface'
    $reasonCode = 'operator-inquiry-selection-envelope-inquiry-surface-not-ready'
    $nextAction = if ($null -ne $inquirySessionDisciplineSurfaceState) { [string] $inquirySessionDisciplineSurfaceState.nextAction } else { 'emit-inquiry-session-discipline-surface' }
} elseif ($currentBoundaryLedgerState -ne 'boundary-condition-ledger-ready') {
    $operatorInquirySelectionEnvelopeState = 'awaiting-boundary-condition-ledger'
    $reasonCode = 'operator-inquiry-selection-envelope-boundary-ledger-not-ready'
    $nextAction = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.nextAction } else { 'emit-boundary-condition-ledger' }
} elseif ($currentCoherenceWitnessState -ne 'coherence-gain-witness-receipt-ready') {
    $operatorInquirySelectionEnvelopeState = 'awaiting-coherence-gain-witness-receipt'
    $reasonCode = 'operator-inquiry-selection-envelope-coherence-witness-not-ready'
    $nextAction = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.nextAction } else { 'emit-coherence-gain-witness-receipt' }
} elseif (($currentNextEraSelectorState -ne 'next-era-batch-selector-ready' -or $selectedNextMapId -ne 'automation-maturation-map-27') -and
    $currentActiveTaskMapOrdinal -lt 27) {
    $operatorInquirySelectionEnvelopeState = 'awaiting-map-27-selection'
    $reasonCode = 'operator-inquiry-selection-envelope-next-era-not-selected'
    $nextAction = if ($null -ne $nextEraBatchSelectorState) { [string] $nextEraBatchSelectorState.nextAction } else { 'emit-next-era-batch-selector' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $envelopeProjectionBound -or -not $envelopeKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $operatorInquirySelectionEnvelopeState = 'awaiting-operator-inquiry-selection-binding'
    $reasonCode = 'operator-inquiry-selection-envelope-source-missing'
    $nextAction = 'bind-operator-inquiry-selection-envelope'
} else {
    $operatorInquirySelectionEnvelopeState = 'operator-inquiry-selection-envelope-ready'
    $reasonCode = 'operator-inquiry-selection-envelope-bound'
    $nextAction = 'emit-bonded-crucible-session-rehearsal'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'operator-inquiry-selection-envelope.json'
$bundleMarkdownPath = Join-Path $bundlePath 'operator-inquiry-selection-envelope.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    operatorInquirySelectionEnvelopeState = $operatorInquirySelectionEnvelopeState
    reasonCode = $reasonCode
    nextAction = $nextAction
    bondedCoWorkSessionRehearsalState = $currentCoWorkRehearsalState
    localityDistinctionWitnessLedgerState = $currentLocalityWitnessState
    inquirySessionDisciplineSurfaceState = $currentInquirySurfaceState
    boundaryConditionLedgerState = $currentBoundaryLedgerState
    coherenceGainWitnessReceiptState = $currentCoherenceWitnessState
    nextEraSelectorState = $currentNextEraSelectorState
    nextEraSelectedMapId = $selectedNextMapId
    activeTaskMapId = $currentActiveTaskMapId
    operatorInquirySelectionState = 'operator-inquiry-selection-ready'
    availableInquiryStanceCount = 4
    knownBoundaryWarningCount = 3
    lawfulUseConditionCount = 3
    protectedInteriorityDenied = $true
    localityBypassDenied = $true
    rawGrantDenied = $true
    envelopeProjectionBound = $envelopeProjectionBound
    envelopeKeyBound = $envelopeKeyBound
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
    '# Operator Inquiry Selection Envelope',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Envelope state: `{0}`' -f $payload.operatorInquirySelectionEnvelopeState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Bonded co-work rehearsal state: `{0}`' -f $(if ($payload.bondedCoWorkSessionRehearsalState) { $payload.bondedCoWorkSessionRehearsalState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Inquiry surface state: `{0}`' -f $(if ($payload.inquirySessionDisciplineSurfaceState) { $payload.inquirySessionDisciplineSurfaceState } else { 'missing' })),
    ('- Boundary ledger state: `{0}`' -f $(if ($payload.boundaryConditionLedgerState) { $payload.boundaryConditionLedgerState } else { 'missing' })),
    ('- Coherence witness state: `{0}`' -f $(if ($payload.coherenceGainWitnessReceiptState) { $payload.coherenceGainWitnessReceiptState } else { 'missing' })),
    ('- Next-era selector state: `{0}`' -f $(if ($payload.nextEraSelectorState) { $payload.nextEraSelectorState } else { 'missing' })),
    ('- Next-era selected map: `{0}`' -f $(if ($payload.nextEraSelectedMapId) { $payload.nextEraSelectedMapId } else { 'missing' })),
    ('- Active task map: `{0}`' -f $(if ($payload.activeTaskMapId) { $payload.activeTaskMapId } else { 'missing' })),
    ('- Available inquiry-stance count: `{0}`' -f $payload.availableInquiryStanceCount),
    ('- Known boundary-warning count: `{0}`' -f $payload.knownBoundaryWarningCount),
    ('- Lawful use-condition count: `{0}`' -f $payload.lawfulUseConditionCount),
    ('- Protected interiority denied: `{0}`' -f [bool] $payload.protectedInteriorityDenied),
    ('- Locality bypass denied: `{0}`' -f [bool] $payload.localityBypassDenied),
    ('- Raw grant denied: `{0}`' -f [bool] $payload.rawGrantDenied),
    ('- Envelope projection bound: `{0}`' -f [bool] $payload.envelopeProjectionBound),
    ('- Envelope key bound: `{0}`' -f [bool] $payload.envelopeKeyBound),
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
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    bondedCoWorkSessionRehearsalState = $payload.bondedCoWorkSessionRehearsalState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    inquirySessionDisciplineSurfaceState = $payload.inquirySessionDisciplineSurfaceState
    boundaryConditionLedgerState = $payload.boundaryConditionLedgerState
    coherenceGainWitnessReceiptState = $payload.coherenceGainWitnessReceiptState
    nextEraSelectorState = $payload.nextEraSelectorState
    nextEraSelectedMapId = $payload.nextEraSelectedMapId
    activeTaskMapId = $payload.activeTaskMapId
    operatorInquirySelectionState = $payload.operatorInquirySelectionState
    availableInquiryStanceCount = $payload.availableInquiryStanceCount
    knownBoundaryWarningCount = $payload.knownBoundaryWarningCount
    lawfulUseConditionCount = $payload.lawfulUseConditionCount
    protectedInteriorityDenied = $payload.protectedInteriorityDenied
    localityBypassDenied = $payload.localityBypassDenied
    rawGrantDenied = $payload.rawGrantDenied
    envelopeProjectionBound = $payload.envelopeProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[operator-inquiry-selection-envelope] Bundle: {0}' -f $bundlePath)
$bundlePath
