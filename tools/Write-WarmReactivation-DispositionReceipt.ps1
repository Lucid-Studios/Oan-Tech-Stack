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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$engramIntentFieldLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$promotionSeductionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionSeductionWatchStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the warm reactivation-disposition writer can run.'
}

$intentFieldState = Read-JsonFileOrNull -Path $engramIntentFieldLedgerStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath
$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$refusalState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath
$seductionWatchState = Read-JsonFileOrNull -Path $promotionSeductionWatchStatePath

$currentIntentFieldState = if ($null -ne $intentFieldState) { [string] $intentFieldState.engramIntentFieldLedgerState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }
$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentRefusalState = if ($null -ne $refusalState) { [string] $refusalState.questioningAdmissionRefusalReceiptState } else { $null }
$currentSeductionWatchState = if ($null -ne $seductionWatchState) { [string] $seductionWatchState.promotionSeductionWatchState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('WarmReactivationDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateWarmReactivationDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('warm-reactivation-disposition-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateWarmReactivationDispositionReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateWarmReactivationDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmIntentAlignmentAndReactivation_RequireCandidatesToCarryTheirOwnWhyBeforeCooling', [System.StringComparison]::Ordinal) -ge 0

$warmReactivationDispositionReceiptState = 'awaiting-intent-constraint-alignment-receipt'
$reasonCode = 'warm-reactivation-disposition-receipt-awaiting-alignment'
$nextAction = 'emit-intent-constraint-alignment-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $warmReactivationDispositionReceiptState = 'blocked'
    $reasonCode = 'warm-reactivation-disposition-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentIntentFieldState -ne 'engram-intent-field-ledger-ready') {
    $warmReactivationDispositionReceiptState = 'awaiting-engram-intent-field-ledger'
    $reasonCode = 'warm-reactivation-disposition-receipt-intent-field-not-ready'
    $nextAction = if ($null -ne $intentFieldState) { [string] $intentFieldState.nextAction } else { 'emit-engram-intent-field-ledger' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $warmReactivationDispositionReceiptState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'warm-reactivation-disposition-receipt-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $warmReactivationDispositionReceiptState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'warm-reactivation-disposition-receipt-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentRefusalState -ne 'questioning-admission-refusal-receipt-ready') {
    $warmReactivationDispositionReceiptState = 'awaiting-questioning-admission-refusal-receipt'
    $reasonCode = 'warm-reactivation-disposition-receipt-refusal-not-ready'
    $nextAction = if ($null -ne $refusalState) { [string] $refusalState.nextAction } else { 'emit-questioning-admission-refusal-receipt' }
} elseif ($currentSeductionWatchState -ne 'promotion-seduction-watch-ready') {
    $warmReactivationDispositionReceiptState = 'awaiting-promotion-seduction-watch'
    $reasonCode = 'warm-reactivation-disposition-receipt-seduction-watch-not-ready'
    $nextAction = if ($null -ne $seductionWatchState) { [string] $seductionWatchState.nextAction } else { 'emit-promotion-seduction-watch' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $warmReactivationDispositionReceiptState = 'awaiting-warm-reactivation-disposition-binding'
    $reasonCode = 'warm-reactivation-disposition-receipt-source-missing'
    $nextAction = 'bind-warm-reactivation-disposition-receipt'
} else {
    $warmReactivationDispositionReceiptState = 'warm-reactivation-disposition-receipt-ready'
    $reasonCode = 'warm-reactivation-disposition-receipt-bound'
    $nextAction = 'continue-warm-held-review'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'warm-reactivation-disposition-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'warm-reactivation-disposition-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    warmReactivationDispositionReceiptState = $warmReactivationDispositionReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    engramIntentFieldLedgerState = $currentIntentFieldState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    variationTestedReentryLedgerState = $currentVariationState
    questioningAdmissionRefusalReceiptState = $currentRefusalState
    promotionSeductionWatchState = $currentSeductionWatchState
    warmHeldPatternCount = 2
    reactivatedHotPatternCount = 1
    archivedPatternCount = 0
    reactivationDisposition = 'mixed-hold-and-reactivate'
    warmHoldingPreserved = $true
    hotReentryRequired = $true
    coldAdmissionWithheld = $true
    archiveDispositionAllowed = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Warm Reactivation Disposition Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.warmReactivationDispositionReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Engram intent-field state: `{0}`' -f $(if ($payload.engramIntentFieldLedgerState) { $payload.engramIntentFieldLedgerState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Admission-refusal state: `{0}`' -f $(if ($payload.questioningAdmissionRefusalReceiptState) { $payload.questioningAdmissionRefusalReceiptState } else { 'missing' })),
    ('- Promotion seduction-watch state: `{0}`' -f $(if ($payload.promotionSeductionWatchState) { $payload.promotionSeductionWatchState } else { 'missing' })),
    ('- Warm-held pattern count: `{0}`' -f $payload.warmHeldPatternCount),
    ('- Reactivated-hot pattern count: `{0}`' -f $payload.reactivatedHotPatternCount),
    ('- Archived pattern count: `{0}`' -f $payload.archivedPatternCount),
    ('- Reactivation disposition: `{0}`' -f $payload.reactivationDisposition),
    ('- Warm holding preserved: `{0}`' -f [bool] $payload.warmHoldingPreserved),
    ('- Hot re-entry required: `{0}`' -f [bool] $payload.hotReentryRequired),
    ('- Cold admission withheld: `{0}`' -f [bool] $payload.coldAdmissionWithheld),
    ('- Archive disposition allowed: `{0}`' -f [bool] $payload.archiveDispositionAllowed),
    ('- Projection bound: `{0}`' -f [bool] $payload.projectionBound),
    ('- Key bound: `{0}`' -f [bool] $payload.keyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Test binding bound: `{0}`' -f [bool] $payload.testBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    warmReactivationDispositionReceiptState = $payload.warmReactivationDispositionReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    engramIntentFieldLedgerState = $payload.engramIntentFieldLedgerState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    questioningAdmissionRefusalReceiptState = $payload.questioningAdmissionRefusalReceiptState
    promotionSeductionWatchState = $payload.promotionSeductionWatchState
    warmHeldPatternCount = $payload.warmHeldPatternCount
    reactivatedHotPatternCount = $payload.reactivatedHotPatternCount
    archivedPatternCount = $payload.archivedPatternCount
    reactivationDisposition = $payload.reactivationDisposition
    warmHoldingPreserved = $payload.warmHoldingPreserved
    hotReentryRequired = $payload.hotReentryRequired
    coldAdmissionWithheld = $payload.coldAdmissionWithheld
    archiveDispositionAllowed = $payload.archiveDispositionAllowed
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
