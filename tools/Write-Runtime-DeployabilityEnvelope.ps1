param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $DeployablesPolicyPath = 'OAN Mortalis V1.0/build/deployables.json'
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

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedDeployablesPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $DeployablesPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$deployablesPolicy = Get-Content -Raw -LiteralPath $resolvedDeployablesPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before runtime deployability envelope can run.'
}

$lastCandidateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastReleaseCandidateBundle')
$manifestPath = if (-not [string]::IsNullOrWhiteSpace($lastCandidateBundle)) {
    Join-Path $lastCandidateBundle 'build-evidence-manifest.json'
} else {
    $null
}
$manifest = if (-not [string]::IsNullOrWhiteSpace($manifestPath)) { Read-JsonFileOrNull -Path $manifestPath } else { $null }
$declaredDeployables = @($deployablesPolicy.deployables)
$publishedArtifacts = if ($null -ne $manifest) { @($manifest.publishedArtifacts) } else { @() }

$deployableCandidates = @(
    foreach ($deployable in $declaredDeployables) {
        $artifact = @($publishedArtifacts | Where-Object { [string] $_.name -eq [string] $deployable.name } | Select-Object -First 1)
        if ($artifact -is [System.Array]) {
            $artifact = if ($artifact.Count -gt 0) { $artifact[0] } else { $null }
        }

        [ordered]@{
            name = [string] $deployable.name
            family = [string] $deployable.family
            projectPath = [string] $deployable.projectPath
            artifactKind = [string] $deployable.artifactKind
            publishLane = [string] $deployable.publishLane
            includedInFirstPublish = [bool] $deployable.includedInFirstPublish
            promotable = [bool] $deployable.promotable
            requiresHitlForPublication = [bool] $deployable.requiresHitlForPublication
            artifactPresent = ($null -ne $artifact)
            artifactPath = if ($null -ne $artifact) { [string] $artifact.path } else { $null }
            fileCount = if ($null -ne $artifact) { [int] $artifact.fileCount } else { 0 }
            totalBytes = if ($null -ne $artifact) { [int64] $artifact.totalBytes } else { 0 }
        }
    }
)

$promotableCandidates = @($deployableCandidates | Where-Object { [bool] $_.promotable })
$readyPromotableCandidates = @($promotableCandidates | Where-Object { [bool] $_.artifactPresent })
$includedFirstPublishCandidates = @($deployableCandidates | Where-Object { [bool] $_.includedInFirstPublish })

$envelopeState = 'candidate-only'
$reasonCode = 'runtime-deployability-envelope-candidate-only'
$nextAction = 'continue-candidate-automation'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $envelopeState = 'blocked'
    $reasonCode = 'runtime-deployability-envelope-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($declaredDeployables.Count -eq 0) {
    $envelopeState = 'awaiting-evidence'
    $reasonCode = 'runtime-deployability-envelope-no-deployables-declared'
    $nextAction = 'declare-runtime-deployable'
} elseif ($null -eq $manifest) {
    $envelopeState = 'awaiting-evidence'
    $reasonCode = 'runtime-deployability-envelope-manifest-missing'
    $nextAction = 'run-release-candidate-conveyor'
} elseif ($promotableCandidates.Count -gt 0 -and $readyPromotableCandidates.Count -ge $promotableCandidates.Count) {
    $envelopeState = 'deployable-candidate-ready'
    $reasonCode = 'runtime-deployability-envelope-promotable-artifacts-present'
    $nextAction = 'derive-runtime-readiness'
} elseif (@($publishedArtifacts).Count -gt 0) {
    $envelopeState = 'candidate-only'
    $reasonCode = 'runtime-deployability-envelope-partial-artifacts'
    $nextAction = 'complete-declared-runtime-artifacts'
} else {
    $envelopeState = 'awaiting-evidence'
    $reasonCode = 'runtime-deployability-envelope-artifacts-missing'
    $nextAction = 'publish-declared-runtime-artifacts-into-candidate-bundle'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace($lastCandidateBundle)) {
    [System.IO.Path]::GetFileName($lastCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'runtime-deployability-envelope.json'
$bundleMarkdownPath = Join-Path $bundlePath 'runtime-deployability-envelope.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    envelopeState = $envelopeState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sourceCandidateBundle = $lastCandidateBundle
    sourceManifest = if (-not [string]::IsNullOrWhiteSpace($manifestPath)) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $manifestPath } else { $null }
    candidateStatus = if ($null -ne $manifest) { [string] $manifest.status } else { $null }
    candidateBranch = if ($null -ne $manifest) { [string] $manifest.repo.branch } else { $null }
    candidateCommitSha = if ($null -ne $manifest) { [string] $manifest.repo.commitSha } else { $null }
    candidateWorktreeState = if ($null -ne $manifest) { [string] $manifest.repo.worktreeState } else { $null }
    declaredDeployableCount = $declaredDeployables.Count
    promotableDeployableCount = $promotableCandidates.Count
    readyPromotableDeployableCount = $readyPromotableCandidates.Count
    firstPublishDeployableCount = $includedFirstPublishCandidates.Count
    deployableCandidates = $deployableCandidates
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Runtime Deployability Envelope',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Envelope state: `{0}`' -f $payload.envelopeState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Candidate status: `{0}`' -f $(if ($payload.candidateStatus) { $payload.candidateStatus } else { 'missing' })),
    ('- Candidate branch: `{0}`' -f $(if ($payload.candidateBranch) { $payload.candidateBranch } else { 'missing' })),
    ('- Candidate commit: `{0}`' -f $(if ($payload.candidateCommitSha) { $payload.candidateCommitSha } else { 'missing' })),
    ('- Declared deployables: `{0}`' -f $payload.declaredDeployableCount),
    ('- Promotable deployables: `{0}`' -f $payload.promotableDeployableCount),
    ('- Ready promotable deployables: `{0}`' -f $payload.readyPromotableDeployableCount),
    ('- First-publish deployables: `{0}`' -f $payload.firstPublishDeployableCount),
    ''
)

foreach ($candidate in $deployableCandidates) {
    $markdownLines += @(
        ('## {0}' -f [string] $candidate.name),
        ('- Family: `{0}`' -f [string] $candidate.family),
        ('- Publish lane: `{0}`' -f [string] $candidate.publishLane),
        ('- Artifact kind: `{0}`' -f [string] $candidate.artifactKind),
        ('- Promotable: `{0}`' -f [bool] $candidate.promotable),
        ('- Included in first publish: `{0}`' -f [bool] $candidate.includedInFirstPublish),
        ('- Requires HITL for publication: `{0}`' -f [bool] $candidate.requiresHitlForPublication),
        ('- Artifact present: `{0}`' -f [bool] $candidate.artifactPresent),
        ('- Artifact path: `{0}`' -f $(if ($candidate.artifactPath) { [string] $candidate.artifactPath } else { 'missing' })),
        ('- File count: `{0}`' -f [int] $candidate.fileCount),
        ('- Total bytes: `{0}`' -f [int64] $candidate.totalBytes),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    envelopeState = $payload.envelopeState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    candidateStatus = $payload.candidateStatus
    candidateCommitSha = $payload.candidateCommitSha
    declaredDeployableCount = $payload.declaredDeployableCount
    promotableDeployableCount = $payload.promotableDeployableCount
    readyPromotableDeployableCount = $payload.readyPromotableDeployableCount
    firstPublishDeployableCount = $payload.firstPublishDeployableCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[runtime-deployability-envelope] Bundle: {0}' -f $bundlePath)
$bundlePath
