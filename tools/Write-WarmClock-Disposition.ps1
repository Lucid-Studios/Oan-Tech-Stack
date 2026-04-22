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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$engramDistanceClassificationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerStatePath)
$warmReactivationDispositionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the warm clock disposition writer can run.'
}

$formationPhaseState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$variationState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$distanceState = Read-JsonFileOrNull -Path $engramDistanceClassificationLedgerStatePath
$warmDispositionState = Read-JsonFileOrNull -Path $warmReactivationDispositionReceiptStatePath

$currentFormationPhaseState = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.formationPhaseVectorState } else { $null }
$currentVariationState = if ($null -ne $variationState) { [string] $variationState.variationTestedReentryLedgerState } else { $null }
$currentDistanceState = if ($null -ne $distanceState) { [string] $distanceState.engramDistanceClassificationLedgerState } else { $null }
$currentWarmDispositionState = if ($null -ne $warmDispositionState) { [string] $warmDispositionState.warmReactivationDispositionReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('WarmClockDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateWarmClockDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('warm-clock-disposition-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateWarmClockDispositionReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateWarmClockDispositionReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmClockRipeningAndCoolingPressure_WitnessTemporalLawInsideWarmLane', [System.StringComparison]::Ordinal) -ge 0

$warmClockDispositionState = 'awaiting-formation-phase-vector'
$reasonCode = 'warm-clock-disposition-awaiting-formation-phase-vector'
$nextAction = 'emit-formation-phase-vector'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $warmClockDispositionState = 'blocked'
    $reasonCode = 'warm-clock-disposition-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentFormationPhaseState -ne 'formation-phase-vector-ready') {
    $warmClockDispositionState = 'awaiting-formation-phase-vector'
    $reasonCode = 'warm-clock-disposition-phase-vector-not-ready'
    $nextAction = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.nextAction } else { 'emit-formation-phase-vector' }
} elseif ($currentVariationState -ne 'variation-tested-reentry-ledger-ready') {
    $warmClockDispositionState = 'awaiting-variation-tested-reentry-ledger'
    $reasonCode = 'warm-clock-disposition-reentry-not-ready'
    $nextAction = if ($null -ne $variationState) { [string] $variationState.nextAction } else { 'emit-variation-tested-reentry-ledger' }
} elseif ($currentDistanceState -ne 'engram-distance-classification-ledger-ready') {
    $warmClockDispositionState = 'awaiting-engram-distance-classification-ledger'
    $reasonCode = 'warm-clock-disposition-distance-ledger-not-ready'
    $nextAction = if ($null -ne $distanceState) { [string] $distanceState.nextAction } else { 'emit-engram-distance-classification-ledger' }
} elseif ($currentWarmDispositionState -ne 'warm-reactivation-disposition-receipt-ready') {
    $warmClockDispositionState = 'awaiting-warm-reactivation-disposition-receipt'
    $reasonCode = 'warm-clock-disposition-warm-disposition-not-ready'
    $nextAction = if ($null -ne $warmDispositionState) { [string] $warmDispositionState.nextAction } else { 'emit-warm-reactivation-disposition-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $warmClockDispositionState = 'awaiting-warm-clock-disposition-binding'
    $reasonCode = 'warm-clock-disposition-source-missing'
    $nextAction = 'bind-warm-clock-disposition'
} else {
    $warmClockDispositionState = 'warm-clock-disposition-ready'
    $reasonCode = 'warm-clock-disposition-bound'
    $nextAction = 'emit-ripening-staleness-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'warm-clock-disposition.json'
$bundleMarkdownPath = Join-Path $bundlePath 'warm-clock-disposition.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    warmClockDispositionState = $warmClockDispositionState
    reasonCode = $reasonCode
    nextAction = $nextAction
    formationPhaseVectorState = $currentFormationPhaseState
    variationTestedReentryLedgerState = $currentVariationState
    engramDistanceClassificationLedgerState = $currentDistanceState
    warmReactivationDispositionReceiptState = $currentWarmDispositionState
    warmClockCount = 4
    unresolvedUnknownLoad = 1
    ripeningDisposition = 'ripening-active'
    stalenessDisposition = 'staleness-risk-present'
    reentryClockActive = $true
    distanceBurdenStillActive = $true
    failureSignatureFreshnessRequired = $true
    warmRipeningUnderway = $true
    stalenessRiskPresent = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Warm Clock Disposition',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.warmClockDispositionState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Formation phase-vector state: `{0}`' -f $(if ($payload.formationPhaseVectorState) { $payload.formationPhaseVectorState } else { 'missing' })),
    ('- Variation-tested reentry state: `{0}`' -f $(if ($payload.variationTestedReentryLedgerState) { $payload.variationTestedReentryLedgerState } else { 'missing' })),
    ('- Engram distance-classification state: `{0}`' -f $(if ($payload.engramDistanceClassificationLedgerState) { $payload.engramDistanceClassificationLedgerState } else { 'missing' })),
    ('- Warm reactivation-disposition state: `{0}`' -f $(if ($payload.warmReactivationDispositionReceiptState) { $payload.warmReactivationDispositionReceiptState } else { 'missing' })),
    ('- Warm-clock count: `{0}`' -f $payload.warmClockCount),
    ('- Unresolved unknown load: `{0}`' -f $payload.unresolvedUnknownLoad),
    ('- Ripening disposition: `{0}`' -f $payload.ripeningDisposition),
    ('- Staleness disposition: `{0}`' -f $payload.stalenessDisposition),
    ('- Re-entry clock active: `{0}`' -f [bool] $payload.reentryClockActive),
    ('- Distance burden still active: `{0}`' -f [bool] $payload.distanceBurdenStillActive),
    ('- Failure-signature freshness required: `{0}`' -f [bool] $payload.failureSignatureFreshnessRequired),
    ('- Warm ripening underway: `{0}`' -f [bool] $payload.warmRipeningUnderway),
    ('- Staleness risk present: `{0}`' -f [bool] $payload.stalenessRiskPresent),
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
    warmClockDispositionState = $payload.warmClockDispositionState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    formationPhaseVectorState = $payload.formationPhaseVectorState
    variationTestedReentryLedgerState = $payload.variationTestedReentryLedgerState
    engramDistanceClassificationLedgerState = $payload.engramDistanceClassificationLedgerState
    warmReactivationDispositionReceiptState = $payload.warmReactivationDispositionReceiptState
    warmClockCount = $payload.warmClockCount
    unresolvedUnknownLoad = $payload.unresolvedUnknownLoad
    ripeningDisposition = $payload.ripeningDisposition
    stalenessDisposition = $payload.stalenessDisposition
    reentryClockActive = $payload.reentryClockActive
    distanceBurdenStillActive = $payload.distanceBurdenStillActive
    failureSignatureFreshnessRequired = $payload.failureSignatureFreshnessRequired
    warmRipeningUnderway = $payload.warmRipeningUnderway
    stalenessRiskPresent = $payload.stalenessRiskPresent
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
