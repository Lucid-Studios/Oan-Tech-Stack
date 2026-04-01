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

    if (-not (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue)) {
        $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
            . $oanWorkspaceResolverPath
        }
    }

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

function Compare-StringSets {
    param(
        [string[]] $Left,
        [string[]] $Right
    )

    $leftSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $rightSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($item in @($Left)) {
        if (-not [string]::IsNullOrWhiteSpace($item)) {
            [void] $leftSet.Add($item)
        }
    }

    foreach ($item in @($Right)) {
        if (-not [string]::IsNullOrWhiteSpace($item)) {
            [void] $rightSet.Add($item)
        }
    }

    return [ordered]@{
        leftOnly = @($leftSet | Where-Object { -not $rightSet.Contains($_) } | Sort-Object)
        rightOnly = @($rightSet | Where-Object { -not $leftSet.Contains($_) } | Sort-Object)
        matched = (@($leftSet | Where-Object { $rightSet.Contains($_) }).Count -eq $leftSet.Count -and @($rightSet | Where-Object { $leftSet.Contains($_) }).Count -eq $rightSet.Count)
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$ciConcordanceOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceOutputRoot)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceStatePath)
$deployablesPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'policy.deployables' -CyclePolicy $cyclePolicy
$workflowPath = Join-Path $resolvedRepoRoot '.github\workflows\release-candidate.yml'

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before CI artifact concordance can run.'
}

$manifestPath = Join-Path (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cycleState.lastReleaseCandidateBundle)) 'build-evidence-manifest.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json
$workflowText = Get-Content -Raw -LiteralPath $workflowPath

$expectedArtifacts = @($deployables.deployables | Where-Object { [bool] $_.includedInFirstPublish } | ForEach-Object { [string] $_.name })
$localArtifacts = @($manifest.publishedArtifacts | ForEach-Object { [string] $_.name })
$artifactSetComparison = Compare-StringSets -Left $expectedArtifacts -Right $localArtifacts
$workflowInvokesConveyor = $workflowText -match 'Invoke-Release-Candidate\.ps1'
$workflowUploadsEvidence = $workflowText -match 'release-candidate-evidence' -and $workflowText -match '\.audit/runs/release-candidates'
$ciEvidenceAvailable = $false

$concordanceState = 'declared-concordant'
$reasonCode = 'ci-concordance-workflow-declared'
if (-not $artifactSetComparison.matched) {
    $concordanceState = 'blocked'
    $reasonCode = 'ci-concordance-local-artifact-mismatch'
} elseif (-not $workflowInvokesConveyor -or -not $workflowUploadsEvidence) {
    $concordanceState = 'blocked'
    $reasonCode = 'ci-concordance-workflow-mismatch'
} elseif ($ciEvidenceAvailable) {
    $concordanceState = 'fully-concordant'
    $reasonCode = 'ci-concordance-full'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = [string] $manifest.repo.commitSha
$shortSha = if ($commitSha.Length -gt 8) { $commitSha.Substring(0, 8) } else { $commitSha }
$bundlePath = Join-Path $ciConcordanceOutputRoot ('{0}-{1}' -f $timestamp, $shortSha)
$bundleJsonPath = Join-Path $bundlePath 'ci-concordance.json'
$bundleMarkdownPath = Join-Path $bundlePath 'ci-concordance.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    concordanceState = $concordanceState
    reasonCode = $reasonCode
    ciEvidenceAvailable = $ciEvidenceAvailable
    workflowInvokesGovernedConveyor = $workflowInvokesConveyor
    workflowUploadsReleaseCandidateEvidence = $workflowUploadsEvidence
    expectedArtifactNames = $expectedArtifacts
    localArtifactNames = $localArtifacts
    comparison = $artifactSetComparison
    workflowPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $workflowPath
    manifestPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $manifestPath
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# CI Artifact Concordance',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Concordance state: `{0}`' -f $payload.concordanceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- CI evidence available: `{0}`' -f $payload.ciEvidenceAvailable),
    ('- Workflow invokes governed conveyor: `{0}`' -f $payload.workflowInvokesGovernedConveyor),
    ('- Workflow uploads release-candidate evidence: `{0}`' -f $payload.workflowUploadsReleaseCandidateEvidence),
    ('- Expected artifacts: `{0}`' -f $(if ($expectedArtifacts.Count -gt 0) { $expectedArtifacts -join '`, `' } else { 'none' })),
    ('- Local artifacts: `{0}`' -f $(if ($localArtifacts.Count -gt 0) { $localArtifacts -join '`, `' } else { 'none' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    concordanceState = $payload.concordanceState
    reasonCode = $payload.reasonCode
}

Write-JsonFile -Path $ciConcordanceStatePath -Value $statePayload
Write-Host ('[ci-concordance] Bundle: {0}' -f $bundlePath)
$bundlePath
