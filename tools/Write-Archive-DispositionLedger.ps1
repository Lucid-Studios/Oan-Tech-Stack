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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$hotReactivationTriggerReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.hotReactivationTriggerReceiptStatePath)
$coldAdmissionEligibilityGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateStatePath)
$warmClockDispositionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionStatePath)
$ripeningStalenessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.archiveDispositionLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.archiveDispositionLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the archive disposition ledger writer can run.'
}

$hotReactivationTriggerState = Read-JsonFileOrNull -Path $hotReactivationTriggerReceiptStatePath
$coldAdmissionEligibilityGateState = Read-JsonFileOrNull -Path $coldAdmissionEligibilityGateStatePath
$warmClockState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$ripeningState = Read-JsonFileOrNull -Path $ripeningStalenessLedgerStatePath

$currentHotReactivationTriggerState = if ($null -ne $hotReactivationTriggerState) { [string] $hotReactivationTriggerState.hotReactivationTriggerReceiptState } else { $null }
$currentColdAdmissionEligibilityGateState = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.coldAdmissionEligibilityGateState } else { $null }
$currentWarmClockState = if ($null -ne $warmClockState) { [string] $warmClockState.warmClockDispositionState } else { $null }
$currentRipeningState = if ($null -ne $ripeningState) { [string] $ripeningState.ripeningStalenessLedgerState } else { $null }

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

$projectionBound = $contractsSource.IndexOf('ArchiveDispositionLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateArchiveDispositionLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('archive-disposition-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateArchiveDispositionLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateArchiveDispositionLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmExitLaw_ReheatColdAndArchiveRoutesRemainDistinct', [System.StringComparison]::Ordinal) -ge 0

$ledgerState = 'awaiting-hot-reactivation-trigger-receipt'
$reasonCode = 'archive-disposition-ledger-awaiting-hot-reactivation-trigger-receipt'
$nextAction = 'emit-hot-reactivation-trigger-receipt'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'archive-disposition-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentHotReactivationTriggerState -ne 'hot-reactivation-trigger-receipt-ready') {
    $ledgerState = 'awaiting-hot-reactivation-trigger-receipt'
    $reasonCode = 'archive-disposition-ledger-hot-trigger-not-ready'
    $nextAction = if ($null -ne $hotReactivationTriggerState) { [string] $hotReactivationTriggerState.nextAction } else { 'emit-hot-reactivation-trigger-receipt' }
} elseif ($currentColdAdmissionEligibilityGateState -ne 'cold-admission-eligibility-gate-ready') {
    $ledgerState = 'awaiting-cold-admission-eligibility-gate'
    $reasonCode = 'archive-disposition-ledger-cold-gate-not-ready'
    $nextAction = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.nextAction } else { 'emit-cold-admission-eligibility-gate' }
} elseif ($currentWarmClockState -ne 'warm-clock-disposition-ready') {
    $ledgerState = 'awaiting-warm-clock-disposition'
    $reasonCode = 'archive-disposition-ledger-warm-clock-not-ready'
    $nextAction = if ($null -ne $warmClockState) { [string] $warmClockState.nextAction } else { 'emit-warm-clock-disposition' }
} elseif ($currentRipeningState -ne 'ripening-staleness-ledger-ready') {
    $ledgerState = 'awaiting-ripening-staleness-ledger'
    $reasonCode = 'archive-disposition-ledger-ripening-not-ready'
    $nextAction = if ($null -ne $ripeningState) { [string] $ripeningState.nextAction } else { 'emit-ripening-staleness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $ledgerState = 'awaiting-archive-disposition-ledger-binding'
    $reasonCode = 'archive-disposition-ledger-source-missing'
    $nextAction = 'bind-archive-disposition-ledger'
} else {
    $ledgerState = 'archive-disposition-ledger-ready'
    $reasonCode = 'archive-disposition-ledger-bound'
    $nextAction = 'continue-warm-exit-governance'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'archive-disposition-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'archive-disposition-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    archiveDispositionLedgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    hotReactivationTriggerReceiptState = $currentHotReactivationTriggerState
    coldAdmissionEligibilityGateState = $currentColdAdmissionEligibilityGateState
    warmClockDispositionState = $currentWarmClockState
    ripeningStalenessLedgerState = $currentRipeningState
    archiveRouteCount = 4
    preservedProvenanceMarkCount = 3
    deniedRewriteRiskCount = 3
    archiveDisposition = 'archive-available-but-hot-preferred'
    provenancePreserved = $true
    pseudoLineageDenied = $true
    warmIndefiniteHoldingDenied = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Archive Disposition Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.archiveDispositionLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Hot reactivation-trigger state: `{0}`' -f $(if ($payload.hotReactivationTriggerReceiptState) { $payload.hotReactivationTriggerReceiptState } else { 'missing' })),
    ('- Cold admission-eligibility gate state: `{0}`' -f $(if ($payload.coldAdmissionEligibilityGateState) { $payload.coldAdmissionEligibilityGateState } else { 'missing' })),
    ('- Warm-clock disposition state: `{0}`' -f $(if ($payload.warmClockDispositionState) { $payload.warmClockDispositionState } else { 'missing' })),
    ('- Ripening-staleness ledger state: `{0}`' -f $(if ($payload.ripeningStalenessLedgerState) { $payload.ripeningStalenessLedgerState } else { 'missing' })),
    ('- Archive-route count: `{0}`' -f $payload.archiveRouteCount),
    ('- Preserved provenance-mark count: `{0}`' -f $payload.preservedProvenanceMarkCount),
    ('- Denied rewrite-risk count: `{0}`' -f $payload.deniedRewriteRiskCount),
    ('- Archive disposition: `{0}`' -f $payload.archiveDisposition),
    ('- Provenance preserved: `{0}`' -f [bool] $payload.provenancePreserved),
    ('- Pseudo-lineage denied: `{0}`' -f [bool] $payload.pseudoLineageDenied),
    ('- Warm indefinite holding denied: `{0}`' -f [bool] $payload.warmIndefiniteHoldingDenied),
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
    archiveDispositionLedgerState = $payload.archiveDispositionLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    hotReactivationTriggerReceiptState = $payload.hotReactivationTriggerReceiptState
    coldAdmissionEligibilityGateState = $payload.coldAdmissionEligibilityGateState
    warmClockDispositionState = $payload.warmClockDispositionState
    ripeningStalenessLedgerState = $payload.ripeningStalenessLedgerState
    archiveRouteCount = $payload.archiveRouteCount
    preservedProvenanceMarkCount = $payload.preservedProvenanceMarkCount
    deniedRewriteRiskCount = $payload.deniedRewriteRiskCount
    archiveDisposition = $payload.archiveDisposition
    provenancePreserved = $payload.provenancePreserved
    pseudoLineageDenied = $payload.pseudoLineageDenied
    warmIndefiniteHoldingDenied = $payload.warmIndefiniteHoldingDenied
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
