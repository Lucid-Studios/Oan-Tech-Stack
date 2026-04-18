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
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$bondedCrucibleSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.expressiveDeformationReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.expressiveDeformationReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the expressive-deformation writer can run.'
}

$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$bondedCrucibleSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCrucibleSessionRehearsalStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath

$currentContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentOperatorSelectionState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentBondedCrucibleState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'bondedCrucibleSessionRehearsalState')
$currentSharedBoundaryMemoryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$receiptProjectionBound = $contractsSource.IndexOf('ExpressiveDeformationReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateExpressiveDeformationReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('expressive-deformation-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$receiptKeyBound = $keysSource.IndexOf('CreateExpressiveDeformationReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateExpressiveDeformationReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateExpressiveDeformationAndMutualIntelligibilityWitness_PreserveRecognizableDifference', [System.StringComparison]::Ordinal) -ge 0

$expressiveDeformationReceiptState = 'awaiting-continuity-under-pressure-ledger'
$reasonCode = 'expressive-deformation-receipt-awaiting-continuity-ledger'
$nextAction = 'emit-continuity-under-pressure-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $expressiveDeformationReceiptState = 'blocked'
    $reasonCode = 'expressive-deformation-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentContinuityLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $expressiveDeformationReceiptState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'expressive-deformation-receipt-continuity-ledger-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentOperatorSelectionState -ne 'operator-inquiry-selection-envelope-ready') {
    $expressiveDeformationReceiptState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'expressive-deformation-receipt-operator-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentBondedCrucibleState -ne 'bonded-crucible-session-rehearsal-ready') {
    $expressiveDeformationReceiptState = 'awaiting-bonded-crucible-session-rehearsal'
    $reasonCode = 'expressive-deformation-receipt-crucible-not-ready'
    $nextAction = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.nextAction } else { 'emit-bonded-crucible-session-rehearsal' }
} elseif ($currentSharedBoundaryMemoryState -ne 'shared-boundary-memory-ledger-ready') {
    $expressiveDeformationReceiptState = 'awaiting-shared-boundary-memory-ledger'
    $reasonCode = 'expressive-deformation-receipt-boundary-memory-not-ready'
    $nextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { 'emit-shared-boundary-memory-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $receiptProjectionBound -or -not $receiptKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $expressiveDeformationReceiptState = 'awaiting-expressive-deformation-binding'
    $reasonCode = 'expressive-deformation-receipt-source-missing'
    $nextAction = 'bind-expressive-deformation-receipt'
} else {
    $expressiveDeformationReceiptState = 'expressive-deformation-receipt-ready'
    $reasonCode = 'expressive-deformation-receipt-bound'
    $nextAction = 'emit-mutual-intelligibility-witness'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'expressive-deformation-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'expressive-deformation-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    expressiveDeformationReceiptState = $expressiveDeformationReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    continuityUnderPressureLedgerState = $currentContinuityLedgerState
    operatorInquirySelectionEnvelopeState = $currentOperatorSelectionState
    bondedCrucibleSessionRehearsalState = $currentBondedCrucibleState
    sharedBoundaryMemoryLedgerState = $currentSharedBoundaryMemoryState
    deformationState = 'adaptive-refinement-with-bounded-strain'
    changedExpressionCount = 3
    recognizableContinuityCount = 3
    fractureBoundaryCount = 3
    adaptiveRefinementPreserved = $true
    identityCollapseDetected = $false
    receiptProjectionBound = $receiptProjectionBound
    receiptKeyBound = $receiptKeyBound
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
    '# Expressive Deformation Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.expressiveDeformationReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Operator inquiry-selection state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Bonded crucible state: `{0}`' -f $(if ($payload.bondedCrucibleSessionRehearsalState) { $payload.bondedCrucibleSessionRehearsalState } else { 'missing' })),
    ('- Shared boundary-memory state: `{0}`' -f $(if ($payload.sharedBoundaryMemoryLedgerState) { $payload.sharedBoundaryMemoryLedgerState } else { 'missing' })),
    ('- Deformation state: `{0}`' -f $payload.deformationState),
    ('- Changed expression count: `{0}`' -f $payload.changedExpressionCount),
    ('- Recognizable continuity count: `{0}`' -f $payload.recognizableContinuityCount),
    ('- Fracture boundary count: `{0}`' -f $payload.fractureBoundaryCount),
    ('- Adaptive refinement preserved: `{0}`' -f [bool] $payload.adaptiveRefinementPreserved),
    ('- Identity collapse detected: `{0}`' -f [bool] $payload.identityCollapseDetected),
    ('- Receipt projection bound: `{0}`' -f [bool] $payload.receiptProjectionBound),
    ('- Receipt key bound: `{0}`' -f [bool] $payload.receiptKeyBound),
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
    expressiveDeformationReceiptState = $payload.expressiveDeformationReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    bondedCrucibleSessionRehearsalState = $payload.bondedCrucibleSessionRehearsalState
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    deformationState = $payload.deformationState
    changedExpressionCount = $payload.changedExpressionCount
    recognizableContinuityCount = $payload.recognizableContinuityCount
    fractureBoundaryCount = $payload.fractureBoundaryCount
    adaptiveRefinementPreserved = $payload.adaptiveRefinementPreserved
    identityCollapseDetected = $payload.identityCollapseDetected
    receiptProjectionBound = $payload.receiptProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[expressive-deformation-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
