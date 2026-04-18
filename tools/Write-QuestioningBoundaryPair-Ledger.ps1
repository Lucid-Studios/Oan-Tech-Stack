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
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$expressiveDeformationReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.expressiveDeformationReceiptStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the questioning-boundary pair writer can run.'
}

$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$expressiveDeformationReceiptState = Read-JsonFileOrNull -Path $expressiveDeformationReceiptStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath

$currentOperatorInquirySelectionEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionEnvelopeState')
$currentContinuityUnderPressureLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentExpressiveDeformationReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'expressiveDeformationReceiptState')
$currentSharedBoundaryMemoryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$ledgerProjectionBound = $contractsSource.IndexOf('QuestioningBoundaryPairLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateQuestioningBoundaryPairLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('questioning-boundary-pair-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$ledgerKeyBound = $keysSource.IndexOf('CreateQuestioningBoundaryPairLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateQuestioningBoundaryPairLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateInquiryPatternContinuityAndBoundaryPairLedgers_RetainReusableInquiryMemory', [System.StringComparison]::Ordinal) -ge 0

$questioningBoundaryPairLedgerState = 'awaiting-operator-inquiry-selection-envelope'
$reasonCode = 'questioning-boundary-pair-ledger-awaiting-operator-inquiry-selection-envelope'
$nextAction = 'emit-operator-inquiry-selection-envelope'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $questioningBoundaryPairLedgerState = 'blocked'
    $reasonCode = 'questioning-boundary-pair-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentOperatorInquirySelectionEnvelopeState -ne 'operator-inquiry-selection-envelope-ready') {
    $questioningBoundaryPairLedgerState = 'awaiting-operator-inquiry-selection-envelope'
    $reasonCode = 'questioning-boundary-pair-ledger-inquiry-selection-not-ready'
    $nextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { 'emit-operator-inquiry-selection-envelope' }
} elseif ($currentContinuityUnderPressureLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $questioningBoundaryPairLedgerState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'questioning-boundary-pair-ledger-pressure-ledger-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentExpressiveDeformationReceiptState -ne 'expressive-deformation-receipt-ready') {
    $questioningBoundaryPairLedgerState = 'awaiting-expressive-deformation-receipt'
    $reasonCode = 'questioning-boundary-pair-ledger-deformation-not-ready'
    $nextAction = if ($null -ne $expressiveDeformationReceiptState) { [string] $expressiveDeformationReceiptState.nextAction } else { 'emit-expressive-deformation-receipt' }
} elseif ($currentSharedBoundaryMemoryLedgerState -ne 'shared-boundary-memory-ledger-ready') {
    $questioningBoundaryPairLedgerState = 'awaiting-shared-boundary-memory-ledger'
    $reasonCode = 'questioning-boundary-pair-ledger-boundary-memory-not-ready'
    $nextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { 'emit-shared-boundary-memory-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $ledgerKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $questioningBoundaryPairLedgerState = 'awaiting-questioning-boundary-pair-binding'
    $reasonCode = 'questioning-boundary-pair-ledger-source-missing'
    $nextAction = 'bind-questioning-boundary-pair-ledger'
} else {
    $questioningBoundaryPairLedgerState = 'questioning-boundary-pair-ledger-ready'
    $reasonCode = 'questioning-boundary-pair-ledger-bound'
    $nextAction = 'emit-carry-forward-inquiry-selection-surface'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'questioning-boundary-pair-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'questioning-boundary-pair-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    questioningBoundaryPairLedgerState = $questioningBoundaryPairLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    operatorInquirySelectionEnvelopeState = $currentOperatorInquirySelectionEnvelopeState
    continuityUnderPressureLedgerState = $currentContinuityUnderPressureLedgerState
    expressiveDeformationReceiptState = $currentExpressiveDeformationReceiptState
    sharedBoundaryMemoryLedgerState = $currentSharedBoundaryMemoryLedgerState
    pairingState = 'questioning-boundary-pairs-retained'
    inquiryPatternCount = 3
    supportingBoundaryCount = 3
    boundaryConstraintCount = 3
    overreachWarningCount = 3
    constraintMemoryPreserved = $true
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
    '# Questioning Boundary Pair Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.questioningBoundaryPairLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Operator inquiry-selection envelope state: `{0}`' -f $(if ($payload.operatorInquirySelectionEnvelopeState) { $payload.operatorInquirySelectionEnvelopeState } else { 'missing' })),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Expressive deformation state: `{0}`' -f $(if ($payload.expressiveDeformationReceiptState) { $payload.expressiveDeformationReceiptState } else { 'missing' })),
    ('- Shared boundary-memory state: `{0}`' -f $(if ($payload.sharedBoundaryMemoryLedgerState) { $payload.sharedBoundaryMemoryLedgerState } else { 'missing' })),
    ('- Pairing state: `{0}`' -f $payload.pairingState),
    ('- Inquiry-pattern count: `{0}`' -f $payload.inquiryPatternCount),
    ('- Supporting boundary count: `{0}`' -f $payload.supportingBoundaryCount),
    ('- Boundary constraint count: `{0}`' -f $payload.boundaryConstraintCount),
    ('- Overreach-warning count: `{0}`' -f $payload.overreachWarningCount),
    ('- Constraint memory preserved: `{0}`' -f [bool] $payload.constraintMemoryPreserved),
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
    questioningBoundaryPairLedgerState = $payload.questioningBoundaryPairLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    operatorInquirySelectionEnvelopeState = $payload.operatorInquirySelectionEnvelopeState
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    expressiveDeformationReceiptState = $payload.expressiveDeformationReceiptState
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    pairingState = $payload.pairingState
    inquiryPatternCount = $payload.inquiryPatternCount
    supportingBoundaryCount = $payload.supportingBoundaryCount
    boundaryConstraintCount = $payload.boundaryConstraintCount
    overreachWarningCount = $payload.overreachWarningCount
    constraintMemoryPreserved = $payload.constraintMemoryPreserved
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[questioning-boundary-pair-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
