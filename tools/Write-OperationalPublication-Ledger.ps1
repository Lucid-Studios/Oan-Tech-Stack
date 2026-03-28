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
$publishedRuntimeReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptStatePath)
$artifactAttestationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.artifactAttestationStatePath)
$postPublishDriftWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishDriftWatchStatePath)
$operationalPublicationLedgerOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerOutputRoot)
$operationalPublicationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the operational publication ledger can run.'
}

$publishedRuntimeReceiptState = Read-JsonFileOrNull -Path $publishedRuntimeReceiptStatePath
$artifactAttestationState = Read-JsonFileOrNull -Path $artifactAttestationStatePath
$postPublishDriftWatchState = Read-JsonFileOrNull -Path $postPublishDriftWatchStatePath

$ledgerState = 'candidate-only'
$reasonCode = 'operational-publication-ledger-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'operational-publication-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $publishedRuntimeReceiptState -or $null -eq $artifactAttestationState -or $null -eq $postPublishDriftWatchState) {
    $ledgerState = 'awaiting-evidence'
    $reasonCode = 'operational-publication-ledger-evidence-missing'
    $nextAction = 'complete-map-08-prerequisites'
} elseif ([string] $publishedRuntimeReceiptState.receiptState -ne 'receipt-captured') {
    $ledgerState = 'dormant-before-publication'
    $reasonCode = 'operational-publication-ledger-publication-not-observed'
    $nextAction = [string] $publishedRuntimeReceiptState.nextAction
} elseif ([string] $artifactAttestationState.attestationState -ne 'attested-concordant') {
    $ledgerState = 'awaiting-attestation'
    $reasonCode = 'operational-publication-ledger-attestation-not-ready'
    $nextAction = [string] $artifactAttestationState.nextAction
} elseif ([string] $postPublishDriftWatchState.driftWatchState -ne 'stable-post-publish-runtime') {
    $ledgerState = 'awaiting-stable-runtime-window'
    $reasonCode = 'operational-publication-ledger-runtime-window-not-stable'
    $nextAction = [string] $postPublishDriftWatchState.nextAction
} else {
    $ledgerState = 'ledger-captured'
    $reasonCode = 'operational-publication-ledger-captured'
    $nextAction = 'continue-governed-publication-observation'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $operationalPublicationLedgerOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'operational-publication-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'operational-publication-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ledgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    publishedRuntimeReceiptState = if ($null -ne $publishedRuntimeReceiptState) { [string] $publishedRuntimeReceiptState.receiptState } else { $null }
    artifactAttestationState = if ($null -ne $artifactAttestationState) { [string] $artifactAttestationState.attestationState } else { $null }
    postPublishDriftWatchState = if ($null -ne $postPublishDriftWatchState) { [string] $postPublishDriftWatchState.driftWatchState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Operational Publication Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ledgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Published runtime receipt state: `{0}`' -f $(if ($payload.publishedRuntimeReceiptState) { $payload.publishedRuntimeReceiptState } else { 'missing' })),
    ('- Artifact attestation state: `{0}`' -f $(if ($payload.artifactAttestationState) { $payload.artifactAttestationState } else { 'missing' })),
    ('- Post-publish drift watch state: `{0}`' -f $(if ($payload.postPublishDriftWatchState) { $payload.postPublishDriftWatchState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    ledgerState = $payload.ledgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $operationalPublicationLedgerStatePath -Value $statePayload
Write-Host ('[operational-publication-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
