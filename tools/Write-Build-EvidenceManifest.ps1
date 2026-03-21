param(
    [string] $RepoRoot,
    [Parameter(Mandatory = $true)]
    [string] $VersionDecisionPath,
    [Parameter(Mandatory = $true)]
    [string] $BuildAuditBundlePath,
    [string] $SubsystemResultsPath,
    [string] $PublishRoot,
    [Parameter(Mandatory = $true)]
    [string] $OutputPath,
    [ValidateSet('candidate-ready', 'hitl-required', 'blocked')]
    [string] $Status
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$versionDecision = Get-Content -Raw -LiteralPath $VersionDecisionPath | ConvertFrom-Json
$buildRun = Get-Content -Raw -LiteralPath (Join-Path $BuildAuditBundlePath 'run.json') | ConvertFrom-Json
$projects = @((Get-Content -Raw -LiteralPath (Join-Path $BuildAuditBundlePath 'projects.json') | ConvertFrom-Json) | ForEach-Object { $_ })
$tests = @((Get-Content -Raw -LiteralPath (Join-Path $BuildAuditBundlePath 'tests.json') | ConvertFrom-Json) | ForEach-Object { $_ })
$subsystemResults = @()
if (-not [string]::IsNullOrWhiteSpace($SubsystemResultsPath) -and (Test-Path -LiteralPath $SubsystemResultsPath -PathType Leaf)) {
    $subsystemResults = @((Get-Content -Raw -LiteralPath $SubsystemResultsPath | ConvertFrom-Json) | ForEach-Object { $_ })
}

$subsystemResultsRelativePath = $null
if (-not [string]::IsNullOrWhiteSpace($SubsystemResultsPath)) {
    $subsystemResultsRelativePath = Get-RelativePathString -BasePath $RepoRoot -TargetPath $SubsystemResultsPath
}

$publishedArtifacts = @()
if (-not [string]::IsNullOrWhiteSpace($PublishRoot) -and (Test-Path -LiteralPath $PublishRoot -PathType Container)) {
    $publishedArtifacts = @(
        Get-ChildItem -LiteralPath $PublishRoot -Directory | Sort-Object Name | ForEach-Object {
            $files = @(Get-ChildItem -LiteralPath $_.FullName -Recurse -File)
            [ordered]@{
                name = $_.Name
                path = Get-RelativePathString -BasePath $RepoRoot -TargetPath $_.FullName
                fileCount = $files.Count
                totalBytes = ($files | Measure-Object -Property Length -Sum).Sum
            }
        }
    )
}

if ([string]::IsNullOrWhiteSpace($Status)) {
    if ([string] $buildRun.result -ne 'succeeded') {
        $Status = 'blocked'
    }
    elseif ([bool] $versionDecision.versionDecision.requiresHitl -or @($versionDecision.gatesTriggered).Count -gt 0) {
        $Status = 'hitl-required'
    }
    else {
        $Status = 'candidate-ready'
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    status = $Status
    repo = [ordered]@{
        branch = [string] $buildRun.branch
        commitSha = [string] $buildRun.commitSha
        worktreeState = [string] $buildRun.worktreeState
    }
    versionDecision = $versionDecision.versionDecision
    gatesTriggered = @($versionDecision.gatesTriggered)
    touchedProjects = @($versionDecision.touchedProjects)
    touchedFamilies = @($versionDecision.touchedFamilies)
    unmatchedSourcePaths = @($versionDecision.unmatchedSourcePaths)
    buildAudit = [ordered]@{
        bundlePath = Get-RelativePathString -BasePath $RepoRoot -TargetPath $BuildAuditBundlePath
        result = [string] $buildRun.result
        configuration = [string] $buildRun.configuration
        projectCount = @($projects).Count
        testProjectCount = @($tests).Count
    }
    subsystemAudit = [ordered]@{
        resultCount = @($subsystemResults).Count
        resultsPath = $subsystemResultsRelativePath
        results = @($subsystemResults)
    }
    publishedArtifacts = @($publishedArtifacts)
}

Write-JsonFile -Path $OutputPath -Value $manifest
$manifest | ConvertTo-Json -Depth 12
