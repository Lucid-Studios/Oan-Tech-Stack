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
$postPublishDriftWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishDriftWatchStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$postPublishGovernanceLoopOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopOutputRoot)
$postPublishGovernanceLoopStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the post-publish governance loop can run.'
}

$operationalPublicationLedgerState = Read-JsonFileOrNull -Path $operationalPublicationLedgerStatePath
$externalConsumerConcordanceState = Read-JsonFileOrNull -Path $externalConsumerConcordanceStatePath
$postPublishDriftWatchState = Read-JsonFileOrNull -Path $postPublishDriftWatchStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath

$governanceLoopState = 'candidate-only'
$reasonCode = 'post-publish-governance-loop-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $governanceLoopState = 'blocked'
    $reasonCode = 'post-publish-governance-loop-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $operationalPublicationLedgerState -or $null -eq $externalConsumerConcordanceState -or $null -eq $postPublishDriftWatchState -or $null -eq $seededGovernanceState) {
    $governanceLoopState = 'awaiting-evidence'
    $reasonCode = 'post-publish-governance-loop-evidence-missing'
    $nextAction = 'complete-map-08-prerequisites'
} elseif ([string] $operationalPublicationLedgerState.ledgerState -ne 'ledger-captured') {
    $governanceLoopState = 'dormant-before-publication'
    $reasonCode = 'post-publish-governance-loop-ledger-not-ready'
    $nextAction = [string] $operationalPublicationLedgerState.nextAction
} elseif ([string] $externalConsumerConcordanceState.concordanceState -ne 'concordance-confirmed') {
    $governanceLoopState = 'awaiting-consumer-concordance'
    $reasonCode = 'post-publish-governance-loop-consumer-not-confirmed'
    $nextAction = [string] $externalConsumerConcordanceState.nextAction
} elseif ([string] $postPublishDriftWatchState.driftWatchState -ne 'stable-post-publish-runtime') {
    $governanceLoopState = 'awaiting-runtime-stability'
    $reasonCode = 'post-publish-governance-loop-runtime-not-stable'
    $nextAction = [string] $postPublishDriftWatchState.nextAction
} elseif ([string] $seededGovernanceState.disposition -ne 'Accepted') {
    $governanceLoopState = 'awaiting-seeded-governance-closure'
    $reasonCode = 'post-publish-governance-loop-seed-not-accepted'
    $nextAction = 'stabilize-seeded-governance'
} else {
    $governanceLoopState = 'loop-stabilized'
    $reasonCode = 'post-publish-governance-loop-stabilized'
    $nextAction = 'continue-multi-interval-publication-governance'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $postPublishGovernanceLoopOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'post-publish-governance-loop.json'
$bundleMarkdownPath = Join-Path $bundlePath 'post-publish-governance-loop.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    governanceLoopState = $governanceLoopState
    reasonCode = $reasonCode
    nextAction = $nextAction
    operationalPublicationLedgerState = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.ledgerState } else { $null }
    externalConsumerConcordanceState = if ($null -ne $externalConsumerConcordanceState) { [string] $externalConsumerConcordanceState.concordanceState } else { $null }
    postPublishDriftWatchState = if ($null -ne $postPublishDriftWatchState) { [string] $postPublishDriftWatchState.driftWatchState } else { $null }
    seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Post-Publish Governance Loop',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Governance loop state: `{0}`' -f $payload.governanceLoopState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Operational publication ledger state: `{0}`' -f $(if ($payload.operationalPublicationLedgerState) { $payload.operationalPublicationLedgerState } else { 'missing' })),
    ('- External consumer concordance state: `{0}`' -f $(if ($payload.externalConsumerConcordanceState) { $payload.externalConsumerConcordanceState } else { 'missing' })),
    ('- Post-publish drift watch state: `{0}`' -f $(if ($payload.postPublishDriftWatchState) { $payload.postPublishDriftWatchState } else { 'missing' })),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    governanceLoopState = $payload.governanceLoopState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $postPublishGovernanceLoopStatePath -Value $statePayload
Write-Host ('[post-publish-governance-loop] Bundle: {0}' -f $bundlePath)
$bundlePath
