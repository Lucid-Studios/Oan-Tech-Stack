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
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationStatePath)
$publishRequestEnvelopeOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeOutputRoot)
$publishRequestEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeStatePath)
$deployablesPath = Join-Path (Join-Path $resolvedRepoRoot 'OAN Mortalis V1.0') 'build\deployables.json'
$versionPolicyPath = Join-Path (Join-Path $resolvedRepoRoot 'OAN Mortalis V1.0') 'build\version-policy.json'
$hitlGatesPath = Join-Path (Join-Path $resolvedRepoRoot 'OAN Mortalis V1.0') 'build\hitl-gates.json'

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before publish request envelope can run.'
}

$releaseHandshakeState = Read-JsonFileOrNull -Path $releaseHandshakeStatePath
$firstPublishIntentState = Read-JsonFileOrNull -Path $firstPublishIntentStatePath
$releaseRatificationState = Read-JsonFileOrNull -Path $releaseRatificationStatePath
$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json
$versionPolicy = Get-Content -Raw -LiteralPath $versionPolicyPath | ConvertFrom-Json
$hitlGates = Get-Content -Raw -LiteralPath $hitlGatesPath | ConvertFrom-Json

$firstPublishDeployables = @($deployables.deployables | Where-Object { [bool] $_.includedInFirstPublish })
$requestState = 'candidate-only'
$reasonCode = 'publish-request-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $requestState = 'blocked'
    $reasonCode = 'publish-request-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $releaseHandshakeState -or $null -eq $firstPublishIntentState -or $null -eq $releaseRatificationState) {
    $requestState = 'awaiting-evidence'
    $reasonCode = 'publish-request-evidence-missing'
    $nextAction = 'complete-map-06-surfaces'
} elseif ([string] $firstPublishIntentState.intentState -ne 'closed-candidate-intent') {
    $requestState = 'awaiting-intent-closure'
    $reasonCode = 'publish-request-intent-open'
    $nextAction = 'close-first-publish-intent'
} elseif ([string] $releaseHandshakeState.handshakeState -eq 'blocked') {
    $requestState = 'blocked'
    $reasonCode = 'publish-request-handshake-blocked'
    $nextAction = [string] $releaseHandshakeState.nextAction
} elseif ([string] $releaseHandshakeState.handshakeState -ne 'awaiting-ratification') {
    $requestState = 'awaiting-handshake-stability'
    $reasonCode = 'publish-request-handshake-not-ready'
    $nextAction = [string] $releaseHandshakeState.nextAction
} elseif ([string] $releaseRatificationState.rehearsalState -ne 'ready-for-rehearsal') {
    $requestState = 'awaiting-ratification-readiness'
    $reasonCode = 'publish-request-ratification-not-ready'
    $nextAction = [string] $releaseRatificationState.nextHumanDecision
} else {
    $requestState = 'ready-for-hitl-request'
    $reasonCode = 'publish-request-envelope-ready'
    $nextAction = 'present-bounded-publish-request'
}

$gatesForEnvelope = @(
    $hitlGates.gates |
    Where-Object { [string] $_.id -in @('modular-set-promotion', 'publication-promotion') } |
    ForEach-Object {
        [ordered]@{
            id = [string] $_.id
            outcome = [string] $_.outcome
            trigger = [string] $_.trigger
        }
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $publishRequestEnvelopeOutputRoot ('{0}-{1}' -f $timestamp, $commitKey)
$bundleJsonPath = Join-Path $bundlePath 'publish-request-envelope.json'
$bundleMarkdownPath = Join-Path $bundlePath 'publish-request-envelope.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    requestState = $requestState
    reasonCode = $reasonCode
    nextAction = $nextAction
    targetFirstPublishVersion = [string] $versionPolicy.targetFirstPublishVersion
    releaseHandshakeState = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.handshakeState } else { $null }
    releaseRatificationState = if ($null -ne $releaseRatificationState) { [string] $releaseRatificationState.rehearsalState } else { $null }
    firstPublishIntentState = if ($null -ne $firstPublishIntentState) { [string] $firstPublishIntentState.intentState } else { $null }
    firstPublishDeployables = @(
        $firstPublishDeployables | ForEach-Object {
            [ordered]@{
                name = [string] $_.name
                projectPath = [string] $_.projectPath
                publishLane = [string] $_.publishLane
                requiresHitlForPublication = [bool] $_.requiresHitlForPublication
            }
        }
    )
    requiredHitlGates = $gatesForEnvelope
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Publish Request Envelope',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Request state: `{0}`' -f $payload.requestState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Target first publish version: `{0}`' -f $payload.targetFirstPublishVersion),
    ('- Release handshake state: `{0}`' -f $(if ($payload.releaseHandshakeState) { $payload.releaseHandshakeState } else { 'missing' })),
    ('- Release ratification state: `{0}`' -f $(if ($payload.releaseRatificationState) { $payload.releaseRatificationState } else { 'missing' })),
    ('- First publish intent state: `{0}`' -f $(if ($payload.firstPublishIntentState) { $payload.firstPublishIntentState } else { 'missing' })),
    ('- First publish deployables: `{0}`' -f $(if ($payload.firstPublishDeployables.Count -gt 0) { ($payload.firstPublishDeployables | ForEach-Object { [string] $_.name }) -join '`, `' } else { 'none' })),
    ('- Required HITL gates: `{0}`' -f $(if ($gatesForEnvelope.Count -gt 0) { ($gatesForEnvelope | ForEach-Object { [string] $_.id }) -join '`, `' } else { 'none' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    requestState = $payload.requestState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $publishRequestEnvelopeStatePath -Value $statePayload
Write-Host ('[publish-request-envelope] Bundle: {0}' -f $bundlePath)
$bundlePath
