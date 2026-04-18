param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
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

$oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
    . $oanWorkspaceResolverPath
}
$seededGovernanceAdmissionHelperPath = Join-Path $PSScriptRoot 'Seeded-GovernanceAdmission.ps1'
if (Test-Path -LiteralPath $seededGovernanceAdmissionHelperPath -PathType Leaf) {
    . $seededGovernanceAdmissionHelperPath
}

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue) {
        return Resolve-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    }

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

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string[]] $PropertyNames
    )

    if ($null -eq $InputObject) {
        return $null
    }

    foreach ($propertyName in $PropertyNames) {
        $property = $InputObject.PSObject.Properties[$propertyName]
        if ($null -ne $property) {
            return $property.Value
        }
    }

    return $null
}

function Normalize-SeedStatus {
    param([string] $Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    return ($Value.ToLowerInvariant() -replace '[^a-z0-9]', '')
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
$readyState = 'unknown'
$readyReasonCode = 'seed-readiness-not-evaluated'
$readyActionTaken = 'none'
$startAttempted = $false
$startSucceeded = $false
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

    if ([bool] $seedPolicy.ensureReadyOnCall) {
        $seedReadiness = Invoke-SeedReadiness -ResolvedRepoRoot $resolvedRepoRoot -ResolvedHostEndpoint $hostEndpoint -StartupWaitSeconds ([int] $seedPolicy.startupWaitSeconds)
        $hostReachable = [bool] $seedReadiness.hostReachable
        $readyState = [string] $seedReadiness.readyState
        $readyReasonCode = [string] $seedReadiness.reasonCode
        $readyActionTaken = [string] $seedReadiness.actionTaken
        $startAttempted = [bool] $seedReadiness.startAttempted
        $startSucceeded = [bool] $seedReadiness.startSucceeded
    } else {
        $readyState = if ($hostReachable) { 'ready' } else { 'not-ready' }
        $readyReasonCode = if ($hostReachable) { 'seed-runtime-already-healthy' } else { 'seed-host-unavailable' }
        $readyActionTaken = 'none'
    }

    if (-not $hostReachable) {
        $disposition = [string] $seedPolicy.unreachableDisposition
        $dispositionReason = if ($readyReasonCode) { $readyReasonCode } else { 'seed-host-unavailable' }
    } elseif (-not [bool] $seedPolicy.attemptLocalPreflight) {
        $disposition = 'Deferred'
        $dispositionReason = 'seed-preflight-disabled'
    } else {
        $preflightLaneClass = Get-OanWorkspaceTouchPointLaneClass -BasePath $resolvedRepoRoot -TouchPointId 'tool.localLlmPreflight' -CyclePolicy $cyclePolicy
        if ([string]::Equals($preflightLaneClass, 'research-lane', [System.StringComparison]::OrdinalIgnoreCase)) {
            $disposition = 'Deferred'
            $dispositionReason = 'seed-preflight-routed-to-research'
            $preflightError = Get-OanWorkspaceTouchPointResearchReason -BasePath $resolvedRepoRoot -TouchPointId 'tool.localLlmPreflight' -CyclePolicy $cyclePolicy
            if ([string]::IsNullOrWhiteSpace($preflightError)) {
                $preflightError = 'Local LLM preflight is currently routed to research refinement.'
            }
        } else {
            $preflightAttempted = $true
            $runLocalLlmPreflightPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'tool.localLlmPreflight' -CyclePolicy $cyclePolicy
            try {
                & powershell -ExecutionPolicy Bypass -File $runLocalLlmPreflightPath `
                    -Configuration $Configuration `
                    -HostEndpoint $hostEndpoint `
                    -OutputRoot $preflightOutputRoot | Out-Null

                $preflightSummaryPath = Join-Path $preflightOutputRoot 'run-summary.json'
                $preflightSummary = Get-Content -Raw -LiteralPath $preflightSummaryPath | ConvertFrom-Json
                $preflightSucceeded = $true
                $preflightReadinessStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $preflightSummary -PropertyNames @('ReadinessStatus', 'readiness_status'))
                $preflightCriticalFailureCount = [int] (Get-ObjectPropertyValueOrNull -InputObject $preflightSummary -PropertyNames @('CriticalFailureCount', 'critical_failure_count'))

                $acceptedStatuses = @($seedPolicy.acceptedReadinessStatuses | ForEach-Object { Normalize-SeedStatus -Value ([string] $_) })
                $deferredStatuses = @($seedPolicy.deferredReadinessStatuses | ForEach-Object { Normalize-SeedStatus -Value ([string] $_) })
                $rejectedStatuses = @($seedPolicy.rejectedReadinessStatuses | ForEach-Object { Normalize-SeedStatus -Value ([string] $_) })
                $normalizedPreflightReadinessStatus = Normalize-SeedStatus -Value $preflightReadinessStatus

                if ($preflightCriticalFailureCount -gt 0 -or $rejectedStatuses -contains $normalizedPreflightReadinessStatus) {
                    $disposition = 'Rejected'
                    $dispositionReason = 'seed-preflight-not-ready'
                } elseif ($acceptedStatuses -contains $normalizedPreflightReadinessStatus) {
                    $disposition = 'Accepted'
                    $dispositionReason = 'seed-preflight-ready'
                } elseif ($deferredStatuses -contains $normalizedPreflightReadinessStatus) {
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
}

if ($latestStatus -eq 'hitl-required' -and $disposition -eq 'Accepted') {
    $disposition = 'Deferred'
    $dispositionReason = 'automation-hitl-required'
}

if ($disposition -eq 'Accepted') {
    $provenance = 'Braided'
}

$seededGovernanceAdmission = Get-SeededGovernanceBuildAdmission -SeededGovernanceState ([pscustomobject]@{
    disposition = $disposition
    dispositionReason = $dispositionReason
    readyState = $readyState
}) -CyclePolicy $cyclePolicy
$buildAdmissionState = [string] $seededGovernanceAdmission.buildAdmissionState
$buildAdmissionReason = [string] $seededGovernanceAdmission.buildAdmissionReason
$buildAdmissionIsAdmitted = [bool] $seededGovernanceAdmission.buildAdmissionIsAdmitted

$bundlePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    sourceStatus = $latestStatus
    releaseCandidateBundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedReleaseCandidateBundle
    digestBundlePath = if ($null -ne $resolvedDigestBundle) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedDigestBundle } else { $null }
    hostEndpoint = $hostEndpoint
    hostReachable = $hostReachable
    readyState = $readyState
    readyReasonCode = $readyReasonCode
    readyActionTaken = $readyActionTaken
    startAttempted = $startAttempted
    startSucceeded = $startSucceeded
    preflightAttempted = $preflightAttempted
    preflightSucceeded = $preflightSucceeded
    preflightOutputRoot = if ($preflightAttempted) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $preflightOutputRoot } else { $null }
    preflightSummaryPath = if ($preflightSummaryPath) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $preflightSummaryPath } else { $null }
    preflightReadinessStatus = $preflightReadinessStatus
    preflightCriticalFailureCount = $preflightCriticalFailureCount
    preflightSuiteVersion = if ($null -ne $preflightSummary) { [string] (Get-ObjectPropertyValueOrNull -InputObject $preflightSummary -PropertyNames @('SuiteVersion', 'suite_version')) } else { $null }
    preflightLaneClass = Get-OanWorkspaceTouchPointLaneClass -BasePath $resolvedRepoRoot -TouchPointId 'tool.localLlmPreflight' -CyclePolicy $cyclePolicy
    preflightResearchBucketLabel = Get-OanWorkspaceTouchPointResearchBucketLabel -BasePath $resolvedRepoRoot -TouchPointId 'tool.localLlmPreflight' -CyclePolicy $cyclePolicy
    disposition = $disposition
    dispositionReason = $dispositionReason
    buildAdmissionState = $buildAdmissionState
    buildAdmissionReason = $buildAdmissionReason
    buildAdmissionIsAdmitted = $buildAdmissionIsAdmitted
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
    ('- Build admission state: `{0}`' -f $bundlePayload.buildAdmissionState),
    ('- Build admission reason: `{0}`' -f $bundlePayload.buildAdmissionReason),
    ('- Build admitted: `{0}`' -f $bundlePayload.buildAdmissionIsAdmitted),
    ('- Provenance: `{0}`' -f $bundlePayload.provenance),
    ('- Host endpoint: `{0}`' -f $bundlePayload.hostEndpoint),
    ('- Host reachable: `{0}`' -f $bundlePayload.hostReachable),
    ('- Ready state: `{0}`' -f $bundlePayload.readyState),
    ('- Ready reason: `{0}`' -f $bundlePayload.readyReasonCode),
    ('- Ready action: `{0}`' -f $bundlePayload.readyActionTaken),
    ('- Start attempted: `{0}`' -f $bundlePayload.startAttempted),
    ('- Start succeeded: `{0}`' -f $bundlePayload.startSucceeded),
    ('- Preflight attempted: `{0}`' -f $bundlePayload.preflightAttempted),
    ('- Preflight succeeded: `{0}`' -f $bundlePayload.preflightSucceeded)
)

if ($bundlePayload.preflightLaneClass) {
    $markdownLines += ('- Preflight lane class: `{0}`' -f $bundlePayload.preflightLaneClass)
}

if ($bundlePayload.preflightResearchBucketLabel) {
    $markdownLines += ('- Preflight research bucket: `{0}`' -f $bundlePayload.preflightResearchBucketLabel)
}

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
    buildAdmissionState = $bundlePayload.buildAdmissionState
    buildAdmissionReason = $bundlePayload.buildAdmissionReason
    buildAdmissionIsAdmitted = $bundlePayload.buildAdmissionIsAdmitted
    provenance = $bundlePayload.provenance
    readyState = $bundlePayload.readyState
    readyReasonCode = $bundlePayload.readyReasonCode
    readyActionTaken = $bundlePayload.readyActionTaken
    startAttempted = $bundlePayload.startAttempted
    startSucceeded = $bundlePayload.startSucceeded
    preflightAttempted = $bundlePayload.preflightAttempted
    preflightSucceeded = $bundlePayload.preflightSucceeded
    preflightReadinessStatus = $bundlePayload.preflightReadinessStatus
}

Write-JsonFile -Path $seededGovernanceStatePath -Value $statePayload
Write-Host ('[seeded-build-governance] Bundle: {0}' -f $bundlePath)
$bundlePath
