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
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewStatePath)
$publishRequestEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeStatePath)
$postPublishEvidenceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishEvidenceStatePath)
$seedBraidEscalationOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seedBraidEscalationOutputRoot)
$seedBraidEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seedBraidEscalationStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before seed braid escalation can run.'
}

$seededPromotionReviewState = Read-JsonFileOrNull -Path $seededPromotionReviewStatePath
$publishRequestEnvelopeState = Read-JsonFileOrNull -Path $publishRequestEnvelopeStatePath
$postPublishEvidenceState = Read-JsonFileOrNull -Path $postPublishEvidenceStatePath

$laneState = 'candidate-only'
$reasonCode = 'seed-braid-escalation-candidate-only'
$nextAction = 'continue-candidate-automation'
$trigger = 'none'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $laneState = 'escalated'
    $reasonCode = 'seed-braid-escalation-automation-blocked'
    $nextAction = 'resolve-blocked-automation-state'
    $trigger = 'automation-blocked'
} elseif ($null -eq $seededPromotionReviewState -or $null -eq $publishRequestEnvelopeState -or $null -eq $postPublishEvidenceState) {
    $laneState = 'awaiting-evidence'
    $reasonCode = 'seed-braid-escalation-evidence-missing'
    $nextAction = 'complete-map-06-surfaces'
    $trigger = 'missing-evidence'
} elseif ([string] $postPublishEvidenceState.loopState -eq 'blocked') {
    $laneState = 'escalated'
    $reasonCode = 'seed-braid-escalation-post-publish-blocked'
    $nextAction = [string] $postPublishEvidenceState.nextAction
    $trigger = 'post-publish-blocked'
} elseif ([string] $publishRequestEnvelopeState.requestState -eq 'ready-for-hitl-request' -and [string] $seededPromotionReviewState.disposition -eq 'Rejected') {
    $laneState = 'prepublish-escalation'
    $reasonCode = 'seed-braid-escalation-seed-rejected'
    $nextAction = 'resolve-seed-contradiction-before-request'
    $trigger = 'seed-rejected'
} else {
    $laneState = 'dormant-before-publication'
    $reasonCode = 'seed-braid-escalation-dormant'
    $nextAction = 'wait-for-publication-or-contradiction'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $seedBraidEscalationOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'seed-braid-escalation.json'
$bundleMarkdownPath = Join-Path $bundlePath 'seed-braid-escalation.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    laneState = $laneState
    reasonCode = $reasonCode
    nextAction = $nextAction
    trigger = $trigger
    seededPromotionDisposition = if ($null -ne $seededPromotionReviewState) { [string] $seededPromotionReviewState.disposition } else { $null }
    publishRequestState = if ($null -ne $publishRequestEnvelopeState) { [string] $publishRequestEnvelopeState.requestState } else { $null }
    postPublishLoopState = if ($null -ne $postPublishEvidenceState) { [string] $postPublishEvidenceState.loopState } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Seed Braid Escalation Lane',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Lane state: `{0}`' -f $payload.laneState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Trigger: `{0}`' -f $payload.trigger),
    ('- Seeded promotion disposition: `{0}`' -f $(if ($payload.seededPromotionDisposition) { $payload.seededPromotionDisposition } else { 'missing' })),
    ('- Publish request state: `{0}`' -f $(if ($payload.publishRequestState) { $payload.publishRequestState } else { 'missing' })),
    ('- Post-publish loop state: `{0}`' -f $(if ($payload.postPublishLoopState) { $payload.postPublishLoopState } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    laneState = $payload.laneState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    trigger = $payload.trigger
}

Write-JsonFile -Path $seedBraidEscalationStatePath -Value $statePayload
Write-Host ('[seed-braid-escalation] Bundle: {0}' -f $bundlePath)
$bundlePath
