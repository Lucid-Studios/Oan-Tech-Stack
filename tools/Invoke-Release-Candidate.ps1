param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $BaseRef,
    [string] $RequestedVersion,
    [string] $OutputRoot = '.audit/runs/release-candidates'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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

function Get-ScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
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

$repoRoot = Split-Path -Parent $PSScriptRoot
$activeBuildRoot = Join-Path $repoRoot 'OAN Mortalis V1.0'
$deployablesPath = Join-Path $activeBuildRoot 'build\deployables.json'
$resolveVersionScriptPath = Join-Path $repoRoot 'tools\Resolve-Build-Version.ps1'
$writeManifestScriptPath = Join-Path $repoRoot 'tools\Write-Build-EvidenceManifest.ps1'
$buildAuditScriptPath = Join-Path $repoRoot 'tools\Invoke-Build-Audit.ps1'
$subsystemAuditScriptPath = Join-Path $repoRoot 'tools\Invoke-Subsystem-Audit.ps1'

$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = ((& git rev-parse HEAD) | Out-String).Trim()
$shortSha = $commitSha
if ($commitSha.Length -gt 8) {
    $shortSha = $commitSha.Substring(0, 8)
}
$runId = '{0}-{1}' -f $timestamp, $shortSha
$runRoot = Join-Path $repoRoot $OutputRoot
$runPath = Join-Path $runRoot $runId
$artifactPath = Join-Path $runPath 'artifacts'
$versionDecisionPath = Join-Path $runPath 'version-decision.json'
$evidenceManifestPath = Join-Path $runPath 'build-evidence-manifest.json'
$summaryPath = Join-Path $runPath 'summary.md'

New-Item -ItemType Directory -Force -Path $artifactPath | Out-Null

$resolveArgs = @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $resolveVersionScriptPath,
    '-RepoRoot', $repoRoot,
    '-OutputPath', $versionDecisionPath
)

if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
    $resolveArgs += @('-BaseRef', $BaseRef)
}

if (-not [string]::IsNullOrWhiteSpace($RequestedVersion)) {
    $resolveArgs += @('-RequestedVersion', $RequestedVersion)
}

& powershell @resolveArgs | Out-Null
$versionDecision = Get-Content -Raw -LiteralPath $versionDecisionPath | ConvertFrom-Json
$buildVersion = [string] $versionDecision.versionDecision.proposedVersion
$assemblyVersion = [string] $versionDecision.versionDecision.proposedAssemblyVersion

$buildAuditOutput = & powershell -ExecutionPolicy Bypass -File $buildAuditScriptPath `
    -Configuration $Configuration `
    -OutputRoot '.audit/runs/release-candidate-build-audits' `
    -BuildVersion $buildVersion `
    -AssemblyVersion $assemblyVersion

$buildAuditBundlePath = Get-ScriptOutputTail -Output $buildAuditOutput
if ([string]::IsNullOrWhiteSpace($buildAuditBundlePath)) {
    throw 'Build audit did not return a bundle path.'
}

$subsystemAuditOutput = & powershell -ExecutionPolicy Bypass -File $subsystemAuditScriptPath `
    -Configuration $Configuration `
    -AuditBundlePath $buildAuditBundlePath

$subsystemResultsPath = Get-ScriptOutputTail -Output $subsystemAuditOutput
$publishResults = New-Object System.Collections.Generic.List[object]

foreach ($deployable in @($deployables.deployables | Where-Object { $_.includedInFirstPublish })) {
    $projectPath = Join-Path $activeBuildRoot ([string] $deployable.projectPath).Replace('/', '\')
    $outputPath = Join-Path $artifactPath ([string] $deployable.name)
    $publishArgs = @(
        'publish',
        $projectPath,
        '-c', $Configuration,
        '--no-restore',
        '-v', 'minimal',
        '-o', $outputPath,
        ('-p:OanBuildVersion={0}' -f $buildVersion),
        ('-p:OanAssemblyVersion={0}' -f $assemblyVersion)
    )

    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for '$($deployable.name)'."
    }

    $publishResults.Add([ordered]@{
        name = [string] $deployable.name
        outputPath = Get-RelativePathString -BasePath $repoRoot -TargetPath $outputPath
        buildVersion = $buildVersion
        assemblyVersion = $assemblyVersion
    }) | Out-Null
}

$status = 'candidate-ready'
if ([bool] $versionDecision.versionDecision.requiresHitl -or
    @($versionDecision.gatesTriggered).Count -gt 0) {
    $status = 'hitl-required'
}

$publishResultsArray = @($publishResults | ForEach-Object { $_ })

& powershell -ExecutionPolicy Bypass -File $writeManifestScriptPath `
    -RepoRoot $repoRoot `
    -VersionDecisionPath $versionDecisionPath `
    -BuildAuditBundlePath $buildAuditBundlePath `
    -SubsystemResultsPath $subsystemResultsPath `
    -PublishRoot $artifactPath `
    -OutputPath $evidenceManifestPath `
    -Status $status | Out-Null

$summaryLines = @(
    '# Release Candidate Summary',
    '',
    ('- Run ID: `{0}`' -f $runId),
    ('- Status: `{0}`' -f $status),
    ('- Proposed version: `{0}`' -f $buildVersion),
    ('- Proposed assembly version: `{0}`' -f $assemblyVersion),
    ('- Build audit bundle: `{0}`' -f (Get-RelativePathString -BasePath $repoRoot -TargetPath $buildAuditBundlePath)),
    ('- Subsystem results: `{0}`' -f (Get-RelativePathString -BasePath $repoRoot -TargetPath $subsystemResultsPath)),
    ''
)

if ($publishResultsArray.Count -gt 0) {
    $summaryLines += @(
        '## Published Artifacts',
        '',
        '| Name | Path | Build Version |',
        '| --- | --- | --- |'
    )

    foreach ($publishResult in $publishResultsArray) {
        $summaryLines += ('| {0} | {1} | {2} |' -f $publishResult.name, $publishResult.outputPath, $publishResult.buildVersion)
    }
}

Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8

Write-Host ('[release-candidate] Bundle: {0}' -f $runPath)
$runPath
