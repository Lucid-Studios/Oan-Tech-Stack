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

function Test-HostEndpointReachable {
    param([uri] $Endpoint)

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $async = $client.BeginConnect($Endpoint.Host, $Endpoint.Port, $null, $null)
        if (-not $async.AsyncWaitHandle.WaitOne([TimeSpan]::FromSeconds(2))) {
            return $false
        }

        $client.EndConnect($async)
        return $true
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

function Invoke-SeedReadiness {
    param(
        [string] $ResolvedRepoRoot,
        [string] $ResolvedHostEndpoint,
        [int] $StartupWaitSeconds
    )

    $ensureSeedReadyScriptPath = Join-Path $ResolvedRepoRoot 'tools\Ensure-Seeded-GovernanceReady.ps1'
    $readinessOutput = & powershell -ExecutionPolicy Bypass -File $ensureSeedReadyScriptPath `
        -HostEndpoint $ResolvedHostEndpoint `
        -StartupWaitSeconds $StartupWaitSeconds

    if ($LASTEXITCODE -ne 0) {
        throw "Seed readiness worker failed with exit code $LASTEXITCODE."
    }

    $json = @($readinessOutput) -join [Environment]::NewLine
    return $json | ConvertFrom-Json
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceStatePath)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$seededPromotionReviewOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewOutputRoot)
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewStatePath)
$seedPolicy = $cyclePolicy.seededGovernancePolicy

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before seeded promotion review can run.'
}

$promotionGateState = Read-JsonFileOrNull -Path $promotionGateStatePath
$ciConcordanceState = Read-JsonFileOrNull -Path $ciConcordanceStatePath
$releaseRatificationState = Read-JsonFileOrNull -Path $releaseRatificationStatePath
$firstPublishIntentState = Read-JsonFileOrNull -Path $firstPublishIntentStatePath

$hostEndpoint = if (-not [string]::IsNullOrWhiteSpace([string] $seedPolicy.hostEndpoint)) {
    [string] $seedPolicy.hostEndpoint
} elseif (-not [string]::IsNullOrWhiteSpace($env:OAN_SOULFRAME_HOST_URL)) {
    $env:OAN_SOULFRAME_HOST_URL
} else {
    'http://127.0.0.1:8181'
}

$endpointUri = [uri] $hostEndpoint
$hostReachable = Test-HostEndpointReachable -Endpoint $endpointUri
$readyState = if ($hostReachable) { 'ready' } else { 'not-ready' }
$readyReasonCode = if ($hostReachable) { 'seed-runtime-already-healthy' } else { 'seed-host-unavailable' }
$readyActionTaken = 'none'
$startAttempted = $false
$startSucceeded = $false

if ([bool] $seedPolicy.ensureReadyOnCall) {
    $seedReadiness = Invoke-SeedReadiness -ResolvedRepoRoot $resolvedRepoRoot -ResolvedHostEndpoint $hostEndpoint -StartupWaitSeconds ([int] $seedPolicy.startupWaitSeconds)
    $hostReachable = [bool] $seedReadiness.hostReachable
    $readyState = [string] $seedReadiness.readyState
    $readyReasonCode = [string] $seedReadiness.reasonCode
    $readyActionTaken = [string] $seedReadiness.actionTaken
    $startAttempted = [bool] $seedReadiness.startAttempted
    $startSucceeded = [bool] $seedReadiness.startSucceeded
}

$disposition = 'Deferred'
$reasonCode = 'seeded-promotion-review-not-started'
$provenance = 'SeedAssisted'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $disposition = 'Rejected'
    $reasonCode = 'seeded-promotion-review-automation-blocked'
} elseif ($null -eq $promotionGateState -or $null -eq $ciConcordanceState -or $null -eq $releaseRatificationState -or $null -eq $firstPublishIntentState) {
    $disposition = 'Deferred'
    $reasonCode = 'seeded-promotion-review-missing-evidence'
} elseif (-not $hostReachable) {
    $disposition = 'Deferred'
    $reasonCode = if ($readyReasonCode) { $readyReasonCode } else { 'seeded-promotion-review-host-unavailable' }
} elseif ([string] $promotionGateState.recommendation -eq 'block' -or [string] $ciConcordanceState.concordanceState -eq 'blocked') {
    $disposition = 'Rejected'
    $reasonCode = 'seeded-promotion-review-evidence-blocked'
} elseif ([string] $firstPublishIntentState.intentState -ne 'closed-candidate-intent') {
    $disposition = 'Deferred'
    $reasonCode = 'seeded-promotion-review-intent-open'
} else {
    $disposition = 'Accepted'
    $reasonCode = 'seeded-promotion-review-ready'
    $provenance = 'Braided'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $seededPromotionReviewOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'seeded-promotion-review.json'
$bundleMarkdownPath = Join-Path $bundlePath 'seeded-promotion-review.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    hostEndpoint = $hostEndpoint
    hostReachable = $hostReachable
    readyState = $readyState
    readyReasonCode = $readyReasonCode
    readyActionTaken = $readyActionTaken
    startAttempted = $startAttempted
    startSucceeded = $startSucceeded
    disposition = $disposition
    reasonCode = $reasonCode
    provenance = $provenance
    promotionGateRecommendation = if ($null -ne $promotionGateState) { [string] $promotionGateState.recommendation } else { $null }
    ciConcordanceState = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.concordanceState } else { $null }
    releaseRatificationState = if ($null -ne $releaseRatificationState) { [string] $releaseRatificationState.rehearsalState } else { $null }
    firstPublishIntentState = if ($null -ne $firstPublishIntentState) { [string] $firstPublishIntentState.intentState } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Seeded Promotion Review',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Host endpoint: `{0}`' -f $payload.hostEndpoint),
    ('- Host reachable: `{0}`' -f $payload.hostReachable),
    ('- Ready state: `{0}`' -f $payload.readyState),
    ('- Ready reason: `{0}`' -f $payload.readyReasonCode),
    ('- Ready action: `{0}`' -f $payload.readyActionTaken),
    ('- Start attempted: `{0}`' -f $payload.startAttempted),
    ('- Start succeeded: `{0}`' -f $payload.startSucceeded),
    ('- Disposition: `{0}`' -f $payload.disposition),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Provenance: `{0}`' -f $payload.provenance),
    ('- Promotion gate recommendation: `{0}`' -f $(if ($payload.promotionGateRecommendation) { $payload.promotionGateRecommendation } else { 'missing' })),
    ('- CI concordance state: `{0}`' -f $(if ($payload.ciConcordanceState) { $payload.ciConcordanceState } else { 'missing' })),
    ('- Release ratification state: `{0}`' -f $(if ($payload.releaseRatificationState) { $payload.releaseRatificationState } else { 'missing' })),
    ('- First publish intent state: `{0}`' -f $(if ($payload.firstPublishIntentState) { $payload.firstPublishIntentState } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    disposition = $payload.disposition
    reasonCode = $payload.reasonCode
    provenance = $payload.provenance
    readyState = $payload.readyState
    readyReasonCode = $payload.readyReasonCode
    readyActionTaken = $payload.readyActionTaken
    startAttempted = $payload.startAttempted
    startSucceeded = $payload.startSucceeded
}

Write-JsonFile -Path $seededPromotionReviewStatePath -Value $statePayload
Write-Host ('[seeded-promotion-review] Bundle: {0}' -f $bundlePath)
$bundlePath
