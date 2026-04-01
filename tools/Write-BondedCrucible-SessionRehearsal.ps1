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
$bondedCoWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)
$coherenceGainWitnessReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the bonded crucible rehearsal writer can run.'
}

$bondedCoWorkSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCoWorkSessionRehearsalStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$boundaryConditionLedgerState = Read-JsonFileOrNull -Path $boundaryConditionLedgerStatePath
$coherenceGainWitnessReceiptState = Read-JsonFileOrNull -Path $coherenceGainWitnessReceiptStatePath

$currentCoWorkRehearsalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'rehearsalReceiptState')
$currentOperatorSelectionState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentBoundaryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'boundaryLedgerState')
$currentCoherenceWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherenceWitnessState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$crucibleProjectionBound = $contractsSource.IndexOf('BondedCrucibleSessionRehearsalReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBondedCrucibleSessionRehearsal', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('bonded-crucible-session-rehearsal-bound', [System.StringComparison]::Ordinal) -ge 0
$crucibleKeyBound = $keysSource.IndexOf('CreateBondedCrucibleSessionRehearsalHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBondedCrucibleSessionRehearsal', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateBondedCrucibleSessionRehearsalAndSharedBoundaryMemory_PreserveSharedUncertainty', [System.StringComparison]::Ordinal) -ge 0

$bondedCrucibleSessionRehearsalState = 'awaiting-bonded-cowork-session-rehearsal'
$reasonCode = 'bonded-crucible-session-rehearsal-awaiting-bonded-cowork-session-rehearsal'
$nextAction = 'emit-bonded-cowork-session-rehearsal'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $bondedCrucibleSessionRehearsalState = 'blocked'
    $reasonCode = 'bonded-crucible-session-rehearsal-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentCoWorkRehearsalState -ne 'bonded-cowork-session-rehearsal-ready') {
    $bondedCrucibleSessionRehearsalState = 'awaiting-bonded-cowork-session-rehearsal'
    $reasonCode = 'bonded-crucible-session-rehearsal-cowork-not-ready'
    $nextAction = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.nextAction } else { 'emit-bonded-cowork-session-rehearsal' }
} elseif ($currentOperatorSelectionState -ne 'operator-inquiry-selection-envelope-ready') {
    $bondedCrucibleSessionRehearsalState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'bonded-crucible-session-rehearsal-operator-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentBoundaryLedgerState -ne 'boundary-condition-ledger-ready') {
    $bondedCrucibleSessionRehearsalState = 'awaiting-boundary-condition-ledger'
    $reasonCode = 'bonded-crucible-session-rehearsal-boundary-ledger-not-ready'
    $nextAction = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.nextAction } else { 'emit-boundary-condition-ledger' }
} elseif ($currentCoherenceWitnessState -ne 'coherence-gain-witness-receipt-ready') {
    $bondedCrucibleSessionRehearsalState = 'awaiting-coherence-gain-witness-receipt'
    $reasonCode = 'bonded-crucible-session-rehearsal-coherence-witness-not-ready'
    $nextAction = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.nextAction } else { 'emit-coherence-gain-witness-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $crucibleProjectionBound -or -not $crucibleKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $bondedCrucibleSessionRehearsalState = 'awaiting-bonded-crucible-binding'
    $reasonCode = 'bonded-crucible-session-rehearsal-source-missing'
    $nextAction = 'bind-bonded-crucible-session-rehearsal'
} else {
    $bondedCrucibleSessionRehearsalState = 'bonded-crucible-session-rehearsal-ready'
    $reasonCode = 'bonded-crucible-session-rehearsal-bound'
    $nextAction = 'emit-shared-boundary-memory-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bonded-crucible-session-rehearsal.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bonded-crucible-session-rehearsal.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    bondedCrucibleSessionRehearsalState = $bondedCrucibleSessionRehearsalState
    reasonCode = $reasonCode
    nextAction = $nextAction
    bondedCoWorkSessionRehearsalState = $currentCoWorkRehearsalState
    operatorInquirySelectionEnvelopeState = $currentOperatorSelectionState
    boundaryConditionLedgerState = $currentBoundaryLedgerState
    coherenceGainWitnessReceiptState = $currentCoherenceWitnessState
    crucibleState = 'bonded-crucible-session-rehearsal-ready'
    selectedInquiryStanceCount = 3
    sharedUnknownFacetCount = 3
    coordinationHoldCount = 3
    exposedBoundaryCount = 3
    preScriptedAnswerDenied = $true
    remoteDominanceDenied = $true
    crucibleProjectionBound = $crucibleProjectionBound
    crucibleKeyBound = $crucibleKeyBound
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
    '# Bonded Crucible Session Rehearsal',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Crucible-rehearsal state: `{0}`' -f $payload.bondedCrucibleSessionRehearsalState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Bonded co-work rehearsal state: `{0}`' -f $(if ($payload.bondedCoWorkSessionRehearsalState) { $payload.bondedCoWorkSessionRehearsalState } else { 'missing' })),
    ('- Operator inquiry-selection state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Boundary-ledger state: `{0}`' -f $(if ($payload.boundaryConditionLedgerState) { $payload.boundaryConditionLedgerState } else { 'missing' })),
    ('- Coherence-witness state: `{0}`' -f $(if ($payload.coherenceGainWitnessReceiptState) { $payload.coherenceGainWitnessReceiptState } else { 'missing' })),
    ('- Selected inquiry-stance count: `{0}`' -f $payload.selectedInquiryStanceCount),
    ('- Shared unknown-facet count: `{0}`' -f $payload.sharedUnknownFacetCount),
    ('- Coordination-hold count: `{0}`' -f $payload.coordinationHoldCount),
    ('- Exposed boundary count: `{0}`' -f $payload.exposedBoundaryCount),
    ('- Pre-scripted answer denied: `{0}`' -f [bool] $payload.preScriptedAnswerDenied),
    ('- Remote dominance denied: `{0}`' -f [bool] $payload.remoteDominanceDenied),
    ('- Crucible projection bound: `{0}`' -f [bool] $payload.crucibleProjectionBound),
    ('- Crucible key bound: `{0}`' -f [bool] $payload.crucibleKeyBound),
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
    bondedCrucibleSessionRehearsalState = $payload.bondedCrucibleSessionRehearsalState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    bondedCoWorkSessionRehearsalState = $payload.bondedCoWorkSessionRehearsalState
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    boundaryConditionLedgerState = $payload.boundaryConditionLedgerState
    coherenceGainWitnessReceiptState = $payload.coherenceGainWitnessReceiptState
    crucibleState = $payload.crucibleState
    selectedInquiryStanceCount = $payload.selectedInquiryStanceCount
    sharedUnknownFacetCount = $payload.sharedUnknownFacetCount
    coordinationHoldCount = $payload.coordinationHoldCount
    exposedBoundaryCount = $payload.exposedBoundaryCount
    preScriptedAnswerDenied = $payload.preScriptedAnswerDenied
    remoteDominanceDenied = $payload.remoteDominanceDenied
    crucibleProjectionBound = $payload.crucibleProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bonded-crucible-session-rehearsal] Bundle: {0}' -f $bundlePath)
$bundlePath
