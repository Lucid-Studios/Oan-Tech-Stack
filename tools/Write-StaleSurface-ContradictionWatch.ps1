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
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

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
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)
$publishedRuntimeReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptStatePath)
$operationalPublicationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerStatePath)
$publicationCadenceLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerStatePath)
$downstreamRuntimeObservationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.downstreamRuntimeObservationStatePath)
$multiIntervalGovernanceBraidStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.multiIntervalGovernanceBraidStatePath)
$staleSurfaceContradictionWatchOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchOutputRoot)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before stale-surface contradiction watch can run.'
}

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$unattendedIntervalConcordanceState = Read-JsonFileOrNull -Path $unattendedIntervalConcordanceStatePath
$publishedRuntimeReceiptState = Read-JsonFileOrNull -Path $publishedRuntimeReceiptStatePath
$operationalPublicationLedgerState = Read-JsonFileOrNull -Path $operationalPublicationLedgerStatePath
$publicationCadenceLedgerState = Read-JsonFileOrNull -Path $publicationCadenceLedgerStatePath
$downstreamRuntimeObservationState = Read-JsonFileOrNull -Path $downstreamRuntimeObservationStatePath
$multiIntervalGovernanceBraidState = Read-JsonFileOrNull -Path $multiIntervalGovernanceBraidStatePath

$watchState = 'candidate-only'
$reasonCode = 'stale-surface-contradiction-watch-candidate-only'
$nextAction = 'continue-candidate-automation'
$contradictions = [System.Collections.Generic.List[string]]::new()

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $watchState = 'blocked'
    $reasonCode = 'stale-surface-contradiction-watch-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $schedulerExecutionReceiptState -or $null -eq $unattendedIntervalConcordanceState -or
          $null -eq $publishedRuntimeReceiptState -or $null -eq $operationalPublicationLedgerState -or
          $null -eq $publicationCadenceLedgerState -or $null -eq $downstreamRuntimeObservationState -or
          $null -eq $multiIntervalGovernanceBraidState) {
    $watchState = 'awaiting-evidence'
    $reasonCode = 'stale-surface-contradiction-watch-evidence-missing'
    $nextAction = 'complete-map-10-prerequisites'
} else {
    if ([string] $publishedRuntimeReceiptState.receiptState -ne 'receipt-captured' -and [string] $operationalPublicationLedgerState.ledgerState -eq 'ledger-captured') {
        $contradictions.Add('operational-ledger-without-runtime-receipt')
    }

    if ([string] $operationalPublicationLedgerState.ledgerState -ne 'ledger-captured' -and [string] $publicationCadenceLedgerState.cadenceState -eq 'awaiting-next-publication-interval') {
        $contradictions.Add('publication-cadence-advanced-before-operational-ledger')
    }

    if ([string] $publicationCadenceLedgerState.cadenceState -ne 'awaiting-next-publication-interval' -and [string] $downstreamRuntimeObservationState.observationState -eq 'awaiting-downstream-interval-observation') {
        $contradictions.Add('downstream-observation-advanced-before-cadence')
    }

    if ([string] $downstreamRuntimeObservationState.observationState -ne 'awaiting-downstream-interval-observation' -and [string] $multiIntervalGovernanceBraidState.braidState -eq 'awaiting-next-governance-interval') {
        $contradictions.Add('governance-braid-advanced-before-downstream-observation')
    }

    if ([string] $schedulerExecutionReceiptState.receiptState -eq 'receipt-captured' -and [string] $unattendedIntervalConcordanceState.concordanceState -eq 'awaiting-scheduler-run') {
        $contradictions.Add('unattended-concordance-lags-scheduler-receipt')
    }

    if ($contradictions.Count -gt 0) {
        $watchState = 'contradiction-detected'
        $reasonCode = 'stale-surface-contradiction-watch-detected'
        $nextAction = 'investigate-surface-ordering'
    } elseif ([string] $schedulerExecutionReceiptState.receiptState -ne 'receipt-captured') {
        $watchState = 'dormant-consistent'
        $reasonCode = 'stale-surface-contradiction-watch-dormant-consistent'
        $nextAction = 'allow-scheduled-cycle-to-fire'
    } else {
        $watchState = 'watch-stable-no-contradictions'
        $reasonCode = 'stale-surface-contradiction-watch-stable'
        $nextAction = 'continue-unattended-observation'
    }
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $staleSurfaceContradictionWatchOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'stale-surface-contradiction-watch.json'
$bundleMarkdownPath = Join-Path $bundlePath 'stale-surface-contradiction-watch.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    watchState = $watchState
    reasonCode = $reasonCode
    nextAction = $nextAction
    contradictions = @($contradictions)
    schedulerReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
    unattendedIntervalConcordanceState = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.concordanceState } else { $null }
    publishedRuntimeReceiptState = if ($null -ne $publishedRuntimeReceiptState) { [string] $publishedRuntimeReceiptState.receiptState } else { $null }
    operationalPublicationLedgerState = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.ledgerState } else { $null }
    publicationCadenceState = if ($null -ne $publicationCadenceLedgerState) { [string] $publicationCadenceLedgerState.cadenceState } else { $null }
    downstreamRuntimeObservationState = if ($null -ne $downstreamRuntimeObservationState) { [string] $downstreamRuntimeObservationState.observationState } else { $null }
    multiIntervalGovernanceBraidState = if ($null -ne $multiIntervalGovernanceBraidState) { [string] $multiIntervalGovernanceBraidState.braidState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Stale Surface Contradiction Watch',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Watch state: `{0}`' -f $payload.watchState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Scheduler receipt state: `{0}`' -f $(if ($payload.schedulerReceiptState) { $payload.schedulerReceiptState } else { 'missing' })),
    ('- Unattended interval concordance state: `{0}`' -f $(if ($payload.unattendedIntervalConcordanceState) { $payload.unattendedIntervalConcordanceState } else { 'missing' })),
    ('- Published runtime receipt state: `{0}`' -f $(if ($payload.publishedRuntimeReceiptState) { $payload.publishedRuntimeReceiptState } else { 'missing' })),
    ('- Operational publication ledger state: `{0}`' -f $(if ($payload.operationalPublicationLedgerState) { $payload.operationalPublicationLedgerState } else { 'missing' })),
    ('- Publication cadence state: `{0}`' -f $(if ($payload.publicationCadenceState) { $payload.publicationCadenceState } else { 'missing' })),
    ('- Downstream runtime observation state: `{0}`' -f $(if ($payload.downstreamRuntimeObservationState) { $payload.downstreamRuntimeObservationState } else { 'missing' })),
    ('- Multi-interval governance braid state: `{0}`' -f $(if ($payload.multiIntervalGovernanceBraidState) { $payload.multiIntervalGovernanceBraidState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' })),
    '',
    '## Contradictions',
    ''
)

if ($payload.contradictions.Count -gt 0) {
    foreach ($contradiction in $payload.contradictions) {
        $markdownLines += ('- `{0}`' -f [string] $contradiction)
    }
} else {
    $markdownLines += '- none'
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    watchState = $payload.watchState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    contradictions = $payload.contradictions
}

Write-JsonFile -Path $staleSurfaceContradictionWatchStatePath -Value $statePayload
Write-Host ('[stale-surface-contradiction-watch] Bundle: {0}' -f $bundlePath)
$bundlePath
