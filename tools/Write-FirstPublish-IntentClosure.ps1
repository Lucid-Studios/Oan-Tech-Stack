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
$firstPublishIntentOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentOutputRoot)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$deployablesPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'policy.deployables' -CyclePolicy $cyclePolicy
$versionPolicyPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId 'policy.versionPolicy' -CyclePolicy $cyclePolicy
    $familyMaturityPath = Join-Path (Join-Path $resolvedRepoRoot 'OAN Mortalis V1.1.1') 'build\family-maturity.json'

$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json
$versionPolicy = Get-Content -Raw -LiteralPath $versionPolicyPath | ConvertFrom-Json
$familyMaturity = Get-Content -Raw -LiteralPath $familyMaturityPath | ConvertFrom-Json
$projectsByPath = @{}
foreach ($project in @($familyMaturity.projects)) {
    $projectsByPath[[string] $project.path] = $project
}

$firstPublishDeployables = @($deployables.deployables | Where-Object { [bool] $_.includedInFirstPublish })
$intentState = 'open-intent'
$reasonCode = 'first-publish-intent-open'

$declaredMismatches = @()
foreach ($deployable in $firstPublishDeployables) {
    $project = $projectsByPath[[string] $deployable.projectPath]
    if ($null -eq $project) {
        $declaredMismatches += ('missing-family-maturity:{0}' -f [string] $deployable.projectPath)
        continue
    }

    if (-not [bool] $project.deployable -or [bool] $project.excludedFromFirstPublish) {
        $declaredMismatches += ('undeployable-project:{0}' -f [string] $deployable.name)
    }
}

if ($firstPublishDeployables.Count -eq 0) {
    $intentState = 'open-intent'
    $reasonCode = 'first-publish-intent-no-deployables'
} elseif ($declaredMismatches.Count -gt 0) {
    $intentState = 'mismatch'
    $reasonCode = 'first-publish-intent-mismatch'
} elseif ([string]::IsNullOrWhiteSpace([string] $versionPolicy.targetFirstPublishVersion)) {
    $intentState = 'open-intent'
    $reasonCode = 'first-publish-intent-no-target-version'
} elseif (@($firstPublishDeployables | Where-Object { -not [bool] $_.requiresHitlForPublication }).Count -gt 0) {
    $intentState = 'mismatch'
    $reasonCode = 'first-publish-intent-hitl-weakened'
} else {
    $intentState = 'closed-candidate-intent'
    $reasonCode = 'first-publish-intent-closed'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundlePath = Join-Path $firstPublishIntentOutputRoot $timestamp
$bundleJsonPath = Join-Path $bundlePath 'first-publish-intent.json'
$bundleMarkdownPath = Join-Path $bundlePath 'first-publish-intent.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    intentState = $intentState
    reasonCode = $reasonCode
    targetFirstPublishVersion = [string] $versionPolicy.targetFirstPublishVersion
    versionScheme = [string] $versionPolicy.versionScheme
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
    declaredMismatches = $declaredMismatches
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# First Publish Intent Closure',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Intent state: `{0}`' -f $payload.intentState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Target first publish version: `{0}`' -f $payload.targetFirstPublishVersion),
    ('- Version scheme: `{0}`' -f $payload.versionScheme),
    ('- First publish deployables: `{0}`' -f $(if ($payload.firstPublishDeployables.Count -gt 0) { ($payload.firstPublishDeployables | ForEach-Object { [string] $_.name }) -join '`, `' } else { 'none' }))
)

if ($declaredMismatches.Count -gt 0) {
    $markdownLines += ('- Declared mismatches: `{0}`' -f ($declaredMismatches -join '`, `'))
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    intentState = $payload.intentState
    reasonCode = $payload.reasonCode
    targetFirstPublishVersion = $payload.targetFirstPublishVersion
}

Write-JsonFile -Path $firstPublishIntentStatePath -Value $statePayload
Write-Host ('[first-publish-intent] Bundle: {0}' -f $bundlePath)
$bundlePath
