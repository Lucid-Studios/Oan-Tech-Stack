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
$operationalPublicationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerStatePath)
$externalConsumerConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.externalConsumerConcordanceStatePath)
$postPublishGovernanceLoopStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopStatePath)
$publicationCadenceLedgerOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerOutputRoot)
$publicationCadenceLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the publication cadence ledger can run.'
}

$operationalPublicationLedgerState = Read-JsonFileOrNull -Path $operationalPublicationLedgerStatePath
$externalConsumerConcordanceState = Read-JsonFileOrNull -Path $externalConsumerConcordanceStatePath
$postPublishGovernanceLoopState = Read-JsonFileOrNull -Path $postPublishGovernanceLoopStatePath

$cadenceState = 'candidate-only'
$reasonCode = 'publication-cadence-ledger-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $cadenceState = 'blocked'
    $reasonCode = 'publication-cadence-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $operationalPublicationLedgerState -or $null -eq $externalConsumerConcordanceState -or $null -eq $postPublishGovernanceLoopState) {
    $cadenceState = 'awaiting-evidence'
    $reasonCode = 'publication-cadence-ledger-evidence-missing'
    $nextAction = 'complete-map-09-prerequisites'
} elseif ([string] $operationalPublicationLedgerState.ledgerState -ne 'ledger-captured') {
    $cadenceState = 'dormant-before-operational-publication'
    $reasonCode = 'publication-cadence-ledger-ledger-not-ready'
    $nextAction = [string] $operationalPublicationLedgerState.nextAction
} elseif ([string] $externalConsumerConcordanceState.concordanceState -ne 'concordance-confirmed') {
    $cadenceState = 'awaiting-consumer-concordance'
    $reasonCode = 'publication-cadence-ledger-consumer-not-confirmed'
    $nextAction = [string] $externalConsumerConcordanceState.nextAction
} elseif ([string] $postPublishGovernanceLoopState.governanceLoopState -ne 'loop-stabilized') {
    $cadenceState = 'awaiting-governance-loop-stability'
    $reasonCode = 'publication-cadence-ledger-governance-loop-not-stable'
    $nextAction = [string] $postPublishGovernanceLoopState.nextAction
} else {
    $cadenceState = 'awaiting-next-publication-interval'
    $reasonCode = 'publication-cadence-ledger-next-interval-pending'
    $nextAction = 'observe-next-governed-publication-interval'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $publicationCadenceLedgerOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'publication-cadence-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'publication-cadence-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    cadenceState = $cadenceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    operationalPublicationLedgerState = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.ledgerState } else { $null }
    externalConsumerConcordanceState = if ($null -ne $externalConsumerConcordanceState) { [string] $externalConsumerConcordanceState.concordanceState } else { $null }
    postPublishGovernanceLoopState = if ($null -ne $postPublishGovernanceLoopState) { [string] $postPublishGovernanceLoopState.governanceLoopState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Publication Cadence Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Cadence state: `{0}`' -f $payload.cadenceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Operational publication ledger state: `{0}`' -f $(if ($payload.operationalPublicationLedgerState) { $payload.operationalPublicationLedgerState } else { 'missing' })),
    ('- External consumer concordance state: `{0}`' -f $(if ($payload.externalConsumerConcordanceState) { $payload.externalConsumerConcordanceState } else { 'missing' })),
    ('- Post-publish governance loop state: `{0}`' -f $(if ($payload.postPublishGovernanceLoopState) { $payload.postPublishGovernanceLoopState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    cadenceState = $payload.cadenceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $publicationCadenceLedgerStatePath -Value $statePayload
Write-Host ('[publication-cadence-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
