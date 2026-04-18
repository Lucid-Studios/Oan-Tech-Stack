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
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceStatePath)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationStatePath)
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$releaseHandshakeOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeOutputRoot)
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before release handshake can run.'
}

$promotionGateState = Read-JsonFileOrNull -Path $promotionGateStatePath
$ciConcordanceState = Read-JsonFileOrNull -Path $ciConcordanceStatePath
$releaseRatificationState = Read-JsonFileOrNull -Path $releaseRatificationStatePath
$seededPromotionReviewState = Read-JsonFileOrNull -Path $seededPromotionReviewStatePath
$firstPublishIntentState = Read-JsonFileOrNull -Path $firstPublishIntentStatePath

$handshakeState = 'candidate-only'
$reasonCode = 'release-handshake-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $handshakeState = 'blocked'
    $reasonCode = 'release-handshake-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $promotionGateState -or $null -eq $ciConcordanceState -or $null -eq $releaseRatificationState -or $null -eq $seededPromotionReviewState -or $null -eq $firstPublishIntentState) {
    $handshakeState = 'awaiting-evidence'
    $reasonCode = 'release-handshake-evidence-missing'
    $nextAction = 'complete-map-05-surfaces'
} elseif ([string] $promotionGateState.recommendation -eq 'block' -or [string] $ciConcordanceState.concordanceState -eq 'blocked') {
    $handshakeState = 'blocked'
    $reasonCode = 'release-handshake-evidence-blocked'
    $nextAction = 'stabilize-promotion-evidence'
} elseif ([string] $firstPublishIntentState.intentState -ne 'closed-candidate-intent') {
    $handshakeState = 'awaiting-intent-closure'
    $reasonCode = 'release-handshake-intent-open'
    $nextAction = 'close-first-publish-intent'
} elseif ([string] $seededPromotionReviewState.disposition -ne 'Accepted') {
    $handshakeState = 'awaiting-seeded-review'
    $reasonCode = 'release-handshake-seed-deferred'
    $nextAction = 'stabilize-seeded-promotion-review'
} elseif ([string] $releaseRatificationState.rehearsalState -ne 'ready-for-rehearsal') {
    $handshakeState = 'awaiting-ratification-readiness'
    $reasonCode = 'release-handshake-ratification-not-ready'
    $nextAction = [string] $releaseRatificationState.nextHumanDecision
} elseif ([string] $promotionGateState.recommendation -eq 'ratification-required') {
    $handshakeState = 'awaiting-ratification'
    $reasonCode = 'release-handshake-ratification-required'
    $nextAction = 'present-ratification-bundle-when-requested'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $releaseHandshakeOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'release-handshake.json'
$bundleMarkdownPath = Join-Path $bundlePath 'release-handshake.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    handshakeState = $handshakeState
    reasonCode = $reasonCode
    nextAction = $nextAction
    promotionGateRecommendation = if ($null -ne $promotionGateState) { [string] $promotionGateState.recommendation } else { $null }
    ciConcordanceState = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.concordanceState } else { $null }
    releaseRatificationState = if ($null -ne $releaseRatificationState) { [string] $releaseRatificationState.rehearsalState } else { $null }
    seededPromotionDisposition = if ($null -ne $seededPromotionReviewState) { [string] $seededPromotionReviewState.disposition } else { $null }
    firstPublishIntentState = if ($null -ne $firstPublishIntentState) { [string] $firstPublishIntentState.intentState } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Release Handshake Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Handshake state: `{0}`' -f $payload.handshakeState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Promotion gate recommendation: `{0}`' -f $(if ($payload.promotionGateRecommendation) { $payload.promotionGateRecommendation } else { 'missing' })),
    ('- CI concordance state: `{0}`' -f $(if ($payload.ciConcordanceState) { $payload.ciConcordanceState } else { 'missing' })),
    ('- Release ratification state: `{0}`' -f $(if ($payload.releaseRatificationState) { $payload.releaseRatificationState } else { 'missing' })),
    ('- Seeded promotion disposition: `{0}`' -f $(if ($payload.seededPromotionDisposition) { $payload.seededPromotionDisposition } else { 'missing' })),
    ('- First publish intent state: `{0}`' -f $(if ($payload.firstPublishIntentState) { $payload.firstPublishIntentState } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    handshakeState = $payload.handshakeState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $releaseHandshakeStatePath -Value $statePayload
Write-Host ('[release-handshake] Bundle: {0}' -f $bundlePath)
$bundlePath
