param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$seededGovernanceOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceOutputRoot)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$seedPolicy = $cyclePolicy.seededGovernancePolicy
$cycleState = Read-JsonFileOrNull -Path $cycleStatePath

if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before seeded governance can run.'
}

$lastReleaseCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
if ([string]::IsNullOrWhiteSpace($lastReleaseCandidateBundle)) {
    throw 'Seeded governance requires a release-candidate bundle.'
}

$resolvedReleaseCandidateBundle = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $lastReleaseCandidateBundle
$manifestPath = Join-Path $resolvedReleaseCandidateBundle 'build-evidence-manifest.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$latestStatus = [string] $manifest.status
$digestBundlePath = [string] $cycleState.lastDigestBundle
$resolvedDigestBundle = if ([string]::IsNullOrWhiteSpace($digestBundlePath)) {
    $null
} else {
    Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $digestBundlePath
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = [string] $manifest.repo.commitSha
$shortSha = if ($commitSha.Length -gt 8) { $commitSha.Substring(0, 8) } else { $commitSha }
$bundleId = '{0}-{1}' -f $timestamp, $shortSha
$bundlePath = Join-Path $seededGovernanceOutputRoot $bundleId
$bundleJsonPath = Join-Path $bundlePath 'seeded-governance.json'
$bundleMarkdownPath = Join-Path $bundlePath 'seeded-governance.md'
$preflightOutputRoot = Join-Path $bundlePath 'preflight'

$hostEndpoint = if (-not [string]::IsNullOrWhiteSpace([string] $seedPolicy.hostEndpoint)) {
    [string] $seedPolicy.hostEndpoint
} elseif (-not [string]::IsNullOrWhiteSpace($env:OAN_SOULFRAME_HOST_URL)) {
    $env:OAN_SOULFRAME_HOST_URL
} else {
    'http://127.0.0.1:8181'
}

$disposition = 'Deferred'
$dispositionReason = 'seed-governance-not-started'
$provenance = 'SeedAssisted'
$hostReachable = $false
$preflightAttempted = $false
$preflightSucceeded = $false
$preflightReadinessStatus = $null
$preflightCriticalFailureCount = $null
$preflightSummaryPath = $null
$preflightSummary = $null
$preflightError = $null

if (-not [bool] $seedPolicy.enabled) {
    $disposition = 'Deferred'
    $dispositionReason = 'seed-governance-disabled'
} elseif ($latestStatus -eq [string] $cyclePolicy.blockedStatus) {
    $disposition = 'Rejected'
    $dispositionReason = 'automation-blocked'
} else {
    $endpointUri = [uri] $hostEndpoint
    $hostReachable = Test-HostEndpointReachable -Endpoint $endpointUri

    if (-not $hostReachable) {
        $disposition = [string] $seedPolicy.unreachableDisposition
        $dispositionReason = 'seed-host-unavailable'
    } elseif (-not [bool] $seedPolicy.attemptLocalPreflight) {
        $disposition = 'Deferred'
        $dispositionReason = 'seed-preflight-disabled'
    } else {
        $preflightAttempted = $true
        $runLocalLlmPreflightPath = Join-Path $resolvedRepoRoot 'OAN Mortalis V1.0\tools\run-local-llm-preflight.ps1'
        try {
            & powershell -ExecutionPolicy Bypass -File $runLocalLlmPreflightPath `
                -Configuration $Configuration `
                -HostEndpoint $hostEndpoint `
                -OutputRoot $preflightOutputRoot | Out-Null

            $preflightSummaryPath = Join-Path $preflightOutputRoot 'run-summary.json'
            $preflightSummary = Get-Content -Raw -LiteralPath $preflightSummaryPath | ConvertFrom-Json
            $preflightSucceeded = $true
            $preflightReadinessStatus = [string] $preflightSummary.ReadinessStatus
            $preflightCriticalFailureCount = [int] $preflightSummary.CriticalFailureCount

            $acceptedStatuses = @($seedPolicy.acceptedReadinessStatuses | ForEach-Object { [string] $_ })
            $deferredStatuses = @($seedPolicy.deferredReadinessStatuses | ForEach-Object { [string] $_ })
            $rejectedStatuses = @($seedPolicy.rejectedReadinessStatuses | ForEach-Object { [string] $_ })

            if ($preflightCriticalFailureCount -gt 0 -or $rejectedStatuses -contains $preflightReadinessStatus) {
                $disposition = 'Rejected'
                $dispositionReason = 'seed-preflight-not-ready'
            } elseif ($acceptedStatuses -contains $preflightReadinessStatus) {
                $disposition = 'Accepted'
                $dispositionReason = 'seed-preflight-ready'
            } elseif ($deferredStatuses -contains $preflightReadinessStatus) {
                $disposition = 'Deferred'
                $dispositionReason = 'seed-preflight-borderline'
            } else {
                $disposition = 'Deferred'
                $dispositionReason = 'seed-preflight-unclassified'
            }
        }
        catch {
            $preflightError = $_.Exception.Message
            $disposition = 'Deferred'
            $dispositionReason = 'seed-preflight-run-failed'
        }
    }
}

if ($latestStatus -eq 'hitl-required' -and $disposition -eq 'Accepted') {
    $disposition = 'Deferred'
    $dispositionReason = 'automation-hitl-required'
}

if ($disposition -eq 'Accepted') {
    $provenance = 'Braided'
}

$bundlePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    sourceStatus = $latestStatus
    releaseCandidateBundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedReleaseCandidateBundle
    digestBundlePath = if ($null -ne $resolvedDigestBundle) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedDigestBundle } else { $null }
    hostEndpoint = $hostEndpoint
    hostReachable = $hostReachable
    preflightAttempted = $preflightAttempted
    preflightSucceeded = $preflightSucceeded
    preflightOutputRoot = if ($preflightAttempted) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $preflightOutputRoot } else { $null }
    preflightSummaryPath = if ($preflightSummaryPath) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $preflightSummaryPath } else { $null }
    preflightReadinessStatus = $preflightReadinessStatus
    preflightCriticalFailureCount = $preflightCriticalFailureCount
    preflightSuiteVersion = if ($null -ne $preflightSummary) { [string] $preflightSummary.SuiteVersion } else { $null }
    disposition = $disposition
    dispositionReason = $dispositionReason
    provenance = $provenance
    preflightError = $preflightError
}

Write-JsonFile -Path $bundleJsonPath -Value $bundlePayload

$markdownLines = @(
    '# Seeded Build Governance',
    '',
    ('- Generated at (UTC): `{0}`' -f $bundlePayload.generatedAtUtc),
    ('- Source status: `{0}`' -f $bundlePayload.sourceStatus),
    ('- Disposition: `{0}`' -f $bundlePayload.disposition),
    ('- Disposition reason: `{0}`' -f $bundlePayload.dispositionReason),
    ('- Provenance: `{0}`' -f $bundlePayload.provenance),
    ('- Host endpoint: `{0}`' -f $bundlePayload.hostEndpoint),
    ('- Host reachable: `{0}`' -f $bundlePayload.hostReachable),
    ('- Preflight attempted: `{0}`' -f $bundlePayload.preflightAttempted),
    ('- Preflight succeeded: `{0}`' -f $bundlePayload.preflightSucceeded)
)

if ($bundlePayload.preflightReadinessStatus) {
    $markdownLines += ('- Preflight readiness: `{0}`' -f $bundlePayload.preflightReadinessStatus)
}

if ($null -ne $bundlePayload.preflightCriticalFailureCount) {
    $markdownLines += ('- Critical failure count: `{0}`' -f $bundlePayload.preflightCriticalFailureCount)
}

if ($bundlePayload.preflightSummaryPath) {
    $markdownLines += ('- Preflight summary: `{0}`' -f $bundlePayload.preflightSummaryPath)
}

if ($bundlePayload.preflightError) {
    $markdownLines += ('- Preflight error: `{0}`' -f $bundlePayload.preflightError)
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $bundlePayload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    disposition = $bundlePayload.disposition
    dispositionReason = $bundlePayload.dispositionReason
    provenance = $bundlePayload.provenance
    preflightAttempted = $bundlePayload.preflightAttempted
    preflightSucceeded = $bundlePayload.preflightSucceeded
    preflightReadinessStatus = $bundlePayload.preflightReadinessStatus
}

Write-JsonFile -Path $seededGovernanceStatePath -Value $statePayload
Write-Host ('[seeded-build-governance] Bundle: {0}' -f $bundlePath)
$bundlePath
