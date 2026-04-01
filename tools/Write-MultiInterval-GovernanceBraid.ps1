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
$publicationCadenceLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerStatePath)
$downstreamRuntimeObservationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.downstreamRuntimeObservationStatePath)
$postPublishGovernanceLoopStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$multiIntervalGovernanceBraidOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.multiIntervalGovernanceBraidOutputRoot)
$multiIntervalGovernanceBraidStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.multiIntervalGovernanceBraidStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the multi-interval governance braid can run.'
}

$publicationCadenceLedgerState = Read-JsonFileOrNull -Path $publicationCadenceLedgerStatePath
$downstreamRuntimeObservationState = Read-JsonFileOrNull -Path $downstreamRuntimeObservationStatePath
$postPublishGovernanceLoopState = Read-JsonFileOrNull -Path $postPublishGovernanceLoopStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath

$braidState = 'candidate-only'
$reasonCode = 'multi-interval-governance-braid-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $braidState = 'blocked'
    $reasonCode = 'multi-interval-governance-braid-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $publicationCadenceLedgerState -or $null -eq $downstreamRuntimeObservationState -or $null -eq $postPublishGovernanceLoopState -or $null -eq $seededGovernanceState) {
    $braidState = 'awaiting-evidence'
    $reasonCode = 'multi-interval-governance-braid-evidence-missing'
    $nextAction = 'complete-map-09-prerequisites'
} elseif ([string] $publicationCadenceLedgerState.cadenceState -ne 'awaiting-next-publication-interval') {
    $braidState = 'dormant-before-cadence'
    $reasonCode = 'multi-interval-governance-braid-cadence-not-ready'
    $nextAction = [string] $publicationCadenceLedgerState.nextAction
} elseif ([string] $downstreamRuntimeObservationState.observationState -ne 'awaiting-downstream-interval-observation') {
    $braidState = 'awaiting-downstream-observation'
    $reasonCode = 'multi-interval-governance-braid-downstream-not-ready'
    $nextAction = [string] $downstreamRuntimeObservationState.nextAction
} elseif ([string] $postPublishGovernanceLoopState.governanceLoopState -ne 'loop-stabilized') {
    $braidState = 'awaiting-governance-loop-stability'
    $reasonCode = 'multi-interval-governance-braid-governance-loop-not-stable'
    $nextAction = [string] $postPublishGovernanceLoopState.nextAction
} elseif ([string] $seededGovernanceState.disposition -ne 'Accepted') {
    $braidState = 'awaiting-seeded-governance-closure'
    $reasonCode = 'multi-interval-governance-braid-seed-not-accepted'
    $nextAction = 'stabilize-seeded-governance'
} else {
    $braidState = 'awaiting-next-governance-interval'
    $reasonCode = 'multi-interval-governance-braid-next-interval-pending'
    $nextAction = 'observe-next-governed-publication-window'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $multiIntervalGovernanceBraidOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'multi-interval-governance-braid.json'
$bundleMarkdownPath = Join-Path $bundlePath 'multi-interval-governance-braid.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    braidState = $braidState
    reasonCode = $reasonCode
    nextAction = $nextAction
    publicationCadenceState = if ($null -ne $publicationCadenceLedgerState) { [string] $publicationCadenceLedgerState.cadenceState } else { $null }
    downstreamRuntimeObservationState = if ($null -ne $downstreamRuntimeObservationState) { [string] $downstreamRuntimeObservationState.observationState } else { $null }
    postPublishGovernanceLoopState = if ($null -ne $postPublishGovernanceLoopState) { [string] $postPublishGovernanceLoopState.governanceLoopState } else { $null }
    seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Multi-Interval Governance Braid',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Braid state: `{0}`' -f $payload.braidState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Publication cadence state: `{0}`' -f $(if ($payload.publicationCadenceState) { $payload.publicationCadenceState } else { 'missing' })),
    ('- Downstream runtime observation state: `{0}`' -f $(if ($payload.downstreamRuntimeObservationState) { $payload.downstreamRuntimeObservationState } else { 'missing' })),
    ('- Post-publish governance loop state: `{0}`' -f $(if ($payload.postPublishGovernanceLoopState) { $payload.postPublishGovernanceLoopState } else { 'missing' })),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    braidState = $payload.braidState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $multiIntervalGovernanceBraidStatePath -Value $statePayload
Write-Host ('[multi-interval-governance-braid] Bundle: {0}' -f $bundlePath)
$bundlePath
