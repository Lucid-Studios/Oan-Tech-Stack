param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
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

    if ([string]::IsNullOrWhiteSpace($TargetPath)) {
        return $null
    }

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

function Get-GitValue {
    param(
        [string[]] $Arguments,
        [string] $WorkingDirectory
    )

    $escapedArguments = $Arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + ($_ -replace '"', '\"') + '"'
        }
        else {
            $_
        }
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = 'git'
    $startInfo.Arguments = [string]::Join(' ', $escapedArguments)
    if (-not [string]::IsNullOrWhiteSpace($WorkingDirectory)) {
        $startInfo.WorkingDirectory = $WorkingDirectory
    }
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    [void] $process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        return $null
    }

    return (@($stdout, $stderr) | Out-String).Trim()
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

function Invoke-ChildPowershellScript {
    param(
        [string[]] $ArgumentList,
        [string] $FailureContext
    )

    $output = & powershell -NoProfile -NonInteractive -WindowStyle Hidden @ArgumentList 2>&1
    if ($LASTEXITCODE -ne 0) {
        $tail = Get-ScriptOutputTail -Output $output
        if ($tail.Count -gt 0) {
            throw '{0} failed with exit code {1}: {2}' -f $FailureContext, $LASTEXITCODE, $tail[0]
        }

        throw '{0} failed with exit code {1}.' -f $FailureContext, $LASTEXITCODE
    }

    return @($output)
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $repoRoot = Split-Path -Parent $PSScriptRoot
}
else {
    $repoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
}

$activeBuildRoot = Join-Path $repoRoot 'OAN Mortalis V1.1.1'
$automationPolicyRoot = Join-Path $repoRoot 'OAN Mortalis V1.1.1\build'
$deployablesPath = Join-Path $automationPolicyRoot 'deployables.json'
$resolveVersionScriptPath = Join-Path $repoRoot 'tools\Resolve-Build-Version.ps1'
$writeManifestScriptPath = Join-Path $repoRoot 'tools\Write-Build-EvidenceManifest.ps1'
$buildAuditScriptPath = Join-Path $repoRoot 'tools\Invoke-Build-Audit.ps1'
$subsystemAuditScriptPath = Join-Path $repoRoot 'tools\Invoke-Subsystem-Audit.ps1'

$deployables = Get-Content -Raw -LiteralPath $deployablesPath | ConvertFrom-Json

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = Get-GitValue -Arguments @('rev-parse', 'HEAD') -WorkingDirectory $repoRoot
if ([string]::IsNullOrWhiteSpace($commitSha)) {
    throw 'Release candidate conveyor could not resolve the current git commit from the repo root.'
}
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

$resolveOutput = Invoke-ChildPowershellScript -ArgumentList $resolveArgs -FailureContext 'Build-version resolution'
$versionDecision = Get-Content -Raw -LiteralPath $versionDecisionPath | ConvertFrom-Json
$buildVersion = [string] $versionDecision.versionDecision.proposedVersion
$assemblyVersion = [string] $versionDecision.versionDecision.proposedAssemblyVersion

$buildAuditArgs = @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $buildAuditScriptPath,
    '-Configuration', $Configuration,
    '-OutputRoot', '.audit/runs/release-candidate-build-audits',
    '-BuildVersion', $buildVersion,
    '-AssemblyVersion', $assemblyVersion
)

$buildAuditOutput = Invoke-ChildPowershellScript -ArgumentList $buildAuditArgs -FailureContext 'Build audit'

$buildAuditBundlePath = Get-ScriptOutputTail -Output $buildAuditOutput
if ([string]::IsNullOrWhiteSpace($buildAuditBundlePath) -or -not (Test-Path -LiteralPath $buildAuditBundlePath -PathType Container)) {
    throw 'Build audit did not return a valid bundle path.'
}

$subsystemAuditArgs = @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $subsystemAuditScriptPath,
    '-Configuration', $Configuration,
    '-AuditBundlePath', $buildAuditBundlePath
)

$subsystemAuditOutput = Invoke-ChildPowershellScript -ArgumentList $subsystemAuditArgs -FailureContext 'Subsystem audit'

$subsystemResultsPath = Get-ScriptOutputTail -Output $subsystemAuditOutput
$subsystemResultsPathForManifest = $null
if (-not [string]::IsNullOrWhiteSpace($subsystemResultsPath)) {
    if (-not (Test-Path -LiteralPath $subsystemResultsPath -PathType Leaf)) {
        throw 'Subsystem audit did not return a valid results path.'
    }

    $subsystemResultsPathForManifest = $subsystemResultsPath
}

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

$writeManifestArgs = @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $writeManifestScriptPath,
    '-RepoRoot', $repoRoot,
    '-VersionDecisionPath', $versionDecisionPath,
    '-BuildAuditBundlePath', $buildAuditBundlePath,
    '-PublishRoot', $artifactPath,
    '-OutputPath', $evidenceManifestPath,
    '-Status', $status
)

if (-not [string]::IsNullOrWhiteSpace($subsystemResultsPathForManifest)) {
    $writeManifestArgs += @('-SubsystemResultsPath', $subsystemResultsPathForManifest)
}

$writeManifestOutput = Invoke-ChildPowershellScript -ArgumentList $writeManifestArgs -FailureContext 'Evidence manifest writer'

$summaryLines = @(
    '# Release Candidate Summary',
    '',
    ('- Run ID: `{0}`' -f $runId),
    ('- Status: `{0}`' -f $status),
    ('- Proposed version: `{0}`' -f $buildVersion),
    ('- Proposed assembly version: `{0}`' -f $assemblyVersion),
    ('- Build audit bundle: `{0}`' -f (Get-RelativePathString -BasePath $repoRoot -TargetPath $buildAuditBundlePath)),
    ('- Subsystem results: `{0}`' -f $(if ($subsystemResultsPathForManifest) { Get-RelativePathString -BasePath $repoRoot -TargetPath $subsystemResultsPathForManifest } else { 'not-emitted' })),
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
