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
$publishRequestEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeStatePath)
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeStatePath)
$publishedRuntimeReceiptOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptOutputRoot)
$publishedRuntimeReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before published runtime receipt can run.'
}

$publishRequestEnvelopeState = Read-JsonFileOrNull -Path $publishRequestEnvelopeStatePath
$releaseHandshakeState = Read-JsonFileOrNull -Path $releaseHandshakeStatePath

$receiptState = 'candidate-only'
$reasonCode = 'published-runtime-receipt-candidate-only'
$nextAction = 'continue-candidate-automation'
$publicationObserved = $false

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $receiptState = 'blocked'
    $reasonCode = 'published-runtime-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $publishRequestEnvelopeState -or $null -eq $releaseHandshakeState) {
    $receiptState = 'awaiting-evidence'
    $reasonCode = 'published-runtime-receipt-evidence-missing'
    $nextAction = 'complete-map-07-surfaces'
} elseif ([string] $publishRequestEnvelopeState.requestState -ne 'ready-for-hitl-request') {
    $receiptState = 'awaiting-request-readiness'
    $reasonCode = 'published-runtime-receipt-request-not-ready'
    $nextAction = [string] $publishRequestEnvelopeState.nextAction
} else {
    $receiptState = 'awaiting-ratified-publication'
    $reasonCode = 'published-runtime-receipt-publication-not-observed'
    $nextAction = 'capture-published-runtime-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $publishedRuntimeReceiptOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'published-runtime-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'published-runtime-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    receiptState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    publicationObserved = $publicationObserved
    publishRequestState = if ($null -ne $publishRequestEnvelopeState) { [string] $publishRequestEnvelopeState.requestState } else { $null }
    releaseHandshakeState = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.handshakeState } else { $null }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Published Runtime Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.receiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Publication observed: `{0}`' -f $payload.publicationObserved),
    ('- Publish request state: `{0}`' -f $(if ($payload.publishRequestState) { $payload.publishRequestState } else { 'missing' })),
    ('- Release handshake state: `{0}`' -f $(if ($payload.releaseHandshakeState) { $payload.releaseHandshakeState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    receiptState = $payload.receiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $publishedRuntimeReceiptStatePath -Value $statePayload
Write-Host ('[published-runtime-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
