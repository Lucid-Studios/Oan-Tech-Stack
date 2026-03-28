param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoRestore,

    [string] $BuildVersion,

    [string] $AssemblyVersion,

    [switch] $ValidateHopng,

    [switch] $HopngPrimeInspect,

    [switch] $HopngCompareSurface,

    [string] $HdtRoot,

    [string] $HopngArtifactPath,

    [string] $HopngCompareArtifactPath,

    [string] $OutputRoot = ".audit/runs"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-JsonFile {
    param([string] $Path, [object] $Value)

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Add-NdjsonLine {
    param([string] $Path, [object] $Value)

    $Value | ConvertTo-Json -Depth 12 -Compress | Add-Content -LiteralPath $Path -Encoding utf8
}

function Get-FileSha256 {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
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

    $lines = @(
        @($stdout, $stderr) |
            ForEach-Object { "$_".Trim() } |
            ForEach-Object { $_ -split "(`r`n|`n|`r)" } |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_) -and
                -not $_.StartsWith('warning:', [System.StringComparison]::OrdinalIgnoreCase)
            }
    )

    return ($lines | Out-String).Trim()
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

function New-EventRecord {
    param(
        [string] $EventType,
        [string] $RunId,
        [string] $Status,
        [string] $Step,
        [hashtable] $Data
    )

    return [ordered]@{
        eventType = $EventType
        runId = $RunId
        timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
        status = $Status
        step = $Step
        data = $Data
    }
}

function Invoke-AuditCommand {
    param(
        [string] $Executable,
        [string[]] $Arguments,
        [string] $LogPath
    )

    $startedAt = Get-Date
    $captured = @()
    $exitCode = 0

    try {
        $captured = & $Executable @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        if ($null -eq $exitCode) {
            $exitCode = 0
        }
    }
    catch {
        $captured = @($_.Exception.ToString())
        $exitCode = 1
    }

    $endedAt = Get-Date
    $capturedText = @($captured | ForEach-Object { "$_" })
    Set-Content -LiteralPath $LogPath -Value $capturedText -Encoding utf8

    return [ordered]@{
        startedAtUtc = $startedAt.ToUniversalTime().ToString("o")
        completedAtUtc = $endedAt.ToUniversalTime().ToString("o")
        durationMs = [int][Math]::Round(($endedAt - $startedAt).TotalMilliseconds)
        exitCode = $exitCode
        succeeded = ($exitCode -eq 0)
        logPath = $LogPath
        outputLines = @($capturedText).Count
    }
}

function Get-SolutionProjects {
    param(
        [string] $SolutionPath,
        [string] $RepoRoot
    )

    $solutionDirectory = Split-Path -Parent $SolutionPath
    $projects = @()

    foreach ($line in Get-Content -LiteralPath $SolutionPath) {
        if ($line -match 'Project\([^)]*\)\s*=\s*"[^"]+",\s*"([^"]+\.csproj)"') {
            $relativeProjectPath = $Matches[1].Replace('\', '/')
            $fullProjectPath = [System.IO.Path]::GetFullPath((Join-Path $solutionDirectory $relativeProjectPath))
            $repoRelativePath = Get-RelativePathString -BasePath $RepoRoot -TargetPath $fullProjectPath
            $projects += [pscustomobject][ordered]@{
                projectName = [System.IO.Path]::GetFileNameWithoutExtension($fullProjectPath)
                projectPath = $repoRelativePath
                fullPath = $fullProjectPath
                kind = if ($repoRelativePath -match '/tests?/') { 'test' } else { 'source' }
            }
        }
    }

    return $projects | Sort-Object projectPath -Unique
}

function Get-TargetFrameworks {
    param([string] $ProjectPath)

    [xml] $xml = Get-Content -LiteralPath $ProjectPath
    $frameworkValues = @()

    foreach ($propertyGroup in $xml.Project.PropertyGroup) {
        $targetFrameworkProperty = $propertyGroup.PSObject.Properties['TargetFramework']
        if ($null -ne $targetFrameworkProperty -and -not [string]::IsNullOrWhiteSpace([string] $targetFrameworkProperty.Value)) {
            $frameworkValues += $propertyGroup.TargetFramework
        }

        $targetFrameworksProperty = $propertyGroup.PSObject.Properties['TargetFrameworks']
        if ($null -ne $targetFrameworksProperty -and -not [string]::IsNullOrWhiteSpace([string] $targetFrameworksProperty.Value)) {
            $frameworkValues += ($propertyGroup.TargetFrameworks -split ';')
        }
    }

    $frameworkValues = $frameworkValues | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
    if (-not $frameworkValues) {
        return @('net8.0')
    }

    return @($frameworkValues)
}

function Get-AssemblyName {
    param([string] $ProjectPath)

    [xml] $xml = Get-Content -LiteralPath $ProjectPath
    foreach ($propertyGroup in $xml.Project.PropertyGroup) {
        $assemblyNameProperty = $propertyGroup.PSObject.Properties['AssemblyName']
        if ($null -ne $assemblyNameProperty -and -not [string]::IsNullOrWhiteSpace([string] $assemblyNameProperty.Value)) {
            return [string] $propertyGroup.AssemblyName
        }
    }

    return [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
}

function Get-ProjectOutputWitness {
    param(
        [string] $ProjectPath,
        [string] $RepoRoot,
        [string] $Configuration
    )

    $assemblyName = Get-AssemblyName -ProjectPath $ProjectPath
    $frameworks = Get-TargetFrameworks -ProjectPath $ProjectPath
    $projectDirectory = Split-Path -Parent $ProjectPath
    $outputPaths = New-Object System.Collections.Generic.List[string]

    foreach ($framework in $frameworks) {
        $candidate = Join-Path $projectDirectory ("bin\{0}\{1}\{2}.dll" -f $Configuration, $framework, $assemblyName)
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $outputPaths.Add((Get-RelativePathString -BasePath $RepoRoot -TargetPath $candidate))
        }
    }

    if ($outputPaths.Count -gt 0) {
        return [ordered]@{
            classification = 'payload_present'
            outputPaths = @($outputPaths)
            note = 'Validated by solution build wrapper; per-project warnings and timings were not individually measured in audit v1.'
        }
    }

    return [ordered]@{
        classification = 'empty_by_observation'
        outputPaths = @()
        note = 'Solution build completed, but no expected binary output was observed at the standard bin path for this project.'
    }
}

$repoRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $MyInvocation.MyCommand.Path)).Path | Split-Path -Parent
$activeBuildRoot = Join-Path $repoRoot 'OAN Mortalis V1.0'
$solutionPath = Join-Path $activeBuildRoot 'Oan.sln'
$buildScriptPath = Join-Path $repoRoot 'build.ps1'
$testScriptPath = Join-Path $repoRoot 'test.ps1'
$verifyScriptPath = Join-Path $activeBuildRoot 'tools\verify-private-corpus.ps1'

$runTimestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$commitSha = Get-GitValue -Arguments @('rev-parse', 'HEAD') -WorkingDirectory $repoRoot
$shortSha = if ($commitSha) { $commitSha.Substring(0, [Math]::Min(8, $commitSha.Length)) } else { 'nogit' }
$runId = '{0}-{1}' -f $runTimestamp, $shortSha
$bundleRoot = Join-Path $repoRoot $OutputRoot
$bundlePath = Join-Path $bundleRoot $runId
$logsPath = Join-Path $bundlePath 'logs'

New-Item -ItemType Directory -Force -Path $logsPath | Out-Null

$eventsPath = Join-Path $bundlePath 'events.ndjson'
$projectsPath = Join-Path $bundlePath 'projects.json'
$testsPath = Join-Path $bundlePath 'tests.json'
$payloadsPath = Join-Path $bundlePath 'payloads.json'
$runPath = Join-Path $bundlePath 'run.json'
$summaryPath = Join-Path $bundlePath 'summary.md'

$payloadWitnesses = New-Object System.Collections.Generic.List[object]
$stepResults = New-Object System.Collections.Generic.List[object]

$runMetadata = [ordered]@{
    runId = $runId
    startedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    completedAtUtc = $null
    repoRoot = $repoRoot
    solutionPath = Get-RelativePathString -BasePath $repoRoot -TargetPath $solutionPath
    configuration = $Configuration
    commitSha = $commitSha
    branch = Get-GitValue -Arguments @('rev-parse', '--abbrev-ref', 'HEAD') -WorkingDirectory $repoRoot
    worktreeState = if ([string]::IsNullOrWhiteSpace((Get-GitValue -Arguments @('status', '--porcelain') -WorkingDirectory $repoRoot))) { 'clean' } else { 'dirty' }
    toolchain = [ordered]@{
        dotnet = ((& dotnet --version) | Out-String).Trim()
        git = Get-GitValue -Arguments @('--version') -WorkingDirectory $repoRoot
        powershell = $PSVersionTable.PSVersion.ToString()
    }
    versionOverride = [ordered]@{
        buildVersion = if ([string]::IsNullOrWhiteSpace($BuildVersion)) { $null } else { $BuildVersion }
        assemblyVersion = if ([string]::IsNullOrWhiteSpace($AssemblyVersion)) { $null } else { $AssemblyVersion }
    }
    hopngValidationStatus = if ($ValidateHopng) { 'requested' } else { 'not_requested' }
    scriptDigests = [ordered]@{
        build = Get-FileSha256 -Path $buildScriptPath
        test = Get-FileSha256 -Path $testScriptPath
        verify = Get-FileSha256 -Path $verifyScriptPath
        audit = Get-FileSha256 -Path $MyInvocation.MyCommand.Path
    }
    outputDigests = @{}
}

Write-JsonFile -Path $runPath -Value $runMetadata
Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'BUILD_RUN_STARTED' -RunId $runId -Status 'started' -Step 'build-audit' -Data @{ configuration = $Configuration; validateHopng = $ValidateHopng.IsPresent })

$solutionProjects = Get-SolutionProjects -SolutionPath $solutionPath -RepoRoot $repoRoot
$sourceProjects = @($solutionProjects | Where-Object kind -eq 'source')
$testProjects = @($solutionProjects | Where-Object kind -eq 'test')
$auditFailed = $false
$failureMessage = $null

try {
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'VERIFY_RUN_STARTED' -RunId $runId -Status 'started' -Step 'workspace-hygiene' -Data @{})
    $verifyLog = Join-Path $logsPath 'verify-private-corpus.log'
    $verifyResult = Invoke-AuditCommand -Executable 'powershell' -Arguments @('-ExecutionPolicy', 'Bypass', '-File', $verifyScriptPath) -LogPath $verifyLog
    $stepResults.Add([ordered]@{
        step = 'verify-private-corpus'
        category = 'verify'
        result = if ($verifyResult.succeeded) { 'succeeded' } else { 'failed' }
        durationMs = $verifyResult.durationMs
        exitCode = $verifyResult.exitCode
        logPath = Get-RelativePathString -BasePath $bundlePath -TargetPath $verifyLog
    }) | Out-Null
    $payloadWitnesses.Add([ordered]@{
        step = 'verify-private-corpus'
        classification = 'empty_by_design'
        note = 'The hygiene script is a verification gate and intentionally emits no runtime payload artifact.'
        references = @((Get-RelativePathString -BasePath $bundlePath -TargetPath $verifyLog))
    }) | Out-Null
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'PAYLOAD_WITNESS_CAPTURED' -RunId $runId -Status 'recorded' -Step 'verify-private-corpus' -Data @{ classification = 'empty_by_design' })
    $verifyStatus = if ($verifyResult.succeeded) { 'succeeded' } else { 'failed' }
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'VERIFY_RUN_COMPLETED' -RunId $runId -Status $verifyStatus -Step 'workspace-hygiene' -Data @{ durationMs = $verifyResult.durationMs; exitCode = $verifyResult.exitCode })
    if (-not $verifyResult.succeeded) {
        throw "Workspace hygiene verification failed."
    }

    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'PROJECT_BUILD_STARTED' -RunId $runId -Status 'started' -Step 'solution-build' -Data @{ projectCount = $sourceProjects.Count; testProjectCount = $testProjects.Count })
    $buildLog = Join-Path $logsPath 'build.log'
    $buildArgs = @('-ExecutionPolicy', 'Bypass', '-File', $buildScriptPath, '-Configuration', $Configuration, '-SkipHygieneCheck')
    if ($NoRestore) {
        $buildArgs += '-NoRestore'
    }
    if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
        $buildArgs += @('-BuildVersion', $BuildVersion)
    }
    if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
        $buildArgs += @('-AssemblyVersion', $AssemblyVersion)
    }
    if ($ValidateHopng) {
        $buildArgs += '-ValidateHopng'
        if ($HopngPrimeInspect) {
            $buildArgs += '-HopngPrimeInspect'
        }
        if ($HopngCompareSurface) {
            $buildArgs += '-HopngCompareSurface'
        }
        if (-not [string]::IsNullOrWhiteSpace($HdtRoot)) {
            $buildArgs += @('-HdtRoot', $HdtRoot)
        }
        if (-not [string]::IsNullOrWhiteSpace($HopngArtifactPath)) {
            $buildArgs += @('-HopngArtifactPath', $HopngArtifactPath)
        }
        if (-not [string]::IsNullOrWhiteSpace($HopngCompareArtifactPath)) {
            $buildArgs += @('-HopngCompareArtifactPath', $HopngCompareArtifactPath)
        }
    }

    $buildResult = Invoke-AuditCommand -Executable 'powershell' -Arguments $buildArgs -LogPath $buildLog
    $stepResults.Add([ordered]@{
        step = 'build'
        category = 'build'
        result = if ($buildResult.succeeded) { 'succeeded' } else { 'failed' }
        durationMs = $buildResult.durationMs
        exitCode = $buildResult.exitCode
        logPath = Get-RelativePathString -BasePath $bundlePath -TargetPath $buildLog
    }) | Out-Null
    $payloadWitnesses.Add([ordered]@{
        step = 'build'
        classification = if ($buildResult.succeeded) { 'payload_present' } else { 'dropped_error' }
        note = if ($buildResult.succeeded) { 'Compiled outputs were observed under standard bin paths and summarized in projects.json.' } else { 'Build did not complete; compiled payloads are incomplete or absent.' }
        references = @((Get-RelativePathString -BasePath $bundlePath -TargetPath $buildLog), 'projects.json')
    }) | Out-Null
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'PAYLOAD_WITNESS_CAPTURED' -RunId $runId -Status 'recorded' -Step 'build' -Data @{ classification = if ($buildResult.succeeded) { 'payload_present' } else { 'dropped_error' } })
    $buildStatus = if ($buildResult.succeeded) { 'succeeded' } else { 'failed' }
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'PROJECT_BUILD_COMPLETED' -RunId $runId -Status $buildStatus -Step 'solution-build' -Data @{ durationMs = $buildResult.durationMs; exitCode = $buildResult.exitCode })
    if (-not $buildResult.succeeded) {
        throw "Solution build failed."
    }

    $projectEntries = foreach ($project in $solutionProjects) {
        $witness = Get-ProjectOutputWitness -ProjectPath $project.fullPath -RepoRoot $repoRoot -Configuration $Configuration
        [ordered]@{
            projectName = $project.projectName
            projectPath = $project.projectPath
            kind = $project.kind
            result = 'succeeded'
            durationMs = $null
            warningCount = $null
            resultSource = 'solution_build_wrapper'
            outputClassification = $witness.classification
            outputPaths = $witness.outputPaths
            note = $witness.note
        }
    }
    Write-JsonFile -Path $projectsPath -Value $projectEntries

    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'TEST_RUN_STARTED' -RunId $runId -Status 'started' -Step 'solution-test' -Data @{ testProjectCount = $testProjects.Count })
    $testLog = Join-Path $logsPath 'test.log'
    $testArgs = @('-ExecutionPolicy', 'Bypass', '-File', $testScriptPath, '-Configuration', $Configuration, '-NoBuild', '-SkipHygieneCheck')
    if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
        $testArgs += @('-BuildVersion', $BuildVersion)
    }
    if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
        $testArgs += @('-AssemblyVersion', $AssemblyVersion)
    }
    $testResult = Invoke-AuditCommand -Executable 'powershell' -Arguments $testArgs -LogPath $testLog
    $stepResults.Add([ordered]@{
        step = 'test'
        category = 'test'
        result = if ($testResult.succeeded) { 'succeeded' } else { 'failed' }
        durationMs = $testResult.durationMs
        exitCode = $testResult.exitCode
        logPath = Get-RelativePathString -BasePath $bundlePath -TargetPath $testLog
    }) | Out-Null
    $payloadWitnesses.Add([ordered]@{
        step = 'test'
        classification = if ($testResult.succeeded) { 'summary_only' } else { 'dropped_error' }
        note = if ($testResult.succeeded) { 'Solution-level test execution completed; individual test project probes are captured in tests.json.' } else { 'Solution-level test execution failed before the audit lane could trust summary output.' }
        references = @((Get-RelativePathString -BasePath $bundlePath -TargetPath $testLog), 'tests.json')
    }) | Out-Null
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'PAYLOAD_WITNESS_CAPTURED' -RunId $runId -Status 'recorded' -Step 'test' -Data @{ classification = if ($testResult.succeeded) { 'summary_only' } else { 'dropped_error' } })
    $testStatus = if ($testResult.succeeded) { 'succeeded' } else { 'failed' }
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'TEST_RUN_COMPLETED' -RunId $runId -Status $testStatus -Step 'solution-test' -Data @{ durationMs = $testResult.durationMs; exitCode = $testResult.exitCode })
    if (-not $testResult.succeeded) {
        throw "Solution tests failed."
    }

    $testEntries = New-Object System.Collections.Generic.List[object]
    foreach ($testProject in $testProjects) {
        $probeLog = Join-Path $logsPath ("test-probe-{0}.log" -f $testProject.projectName)
        $probeResult = Invoke-AuditCommand -Executable 'dotnet' -Arguments @('test', $testProject.fullPath, '-c', $Configuration, '--no-build', '-v', 'minimal') -LogPath $probeLog
        $testEntries.Add([ordered]@{
            projectName = $testProject.projectName
            projectPath = $testProject.projectPath
            result = if ($probeResult.succeeded) { 'succeeded' } else { 'failed' }
            durationMs = $probeResult.durationMs
            exitCode = $probeResult.exitCode
            resultSource = 'individual_audit_probe'
            payloadClassification = 'summary_only'
            note = 'The audit probe captures console test output and timing without emitting a separate TRX artifact in v1.'
            logPath = Get-RelativePathString -BasePath $bundlePath -TargetPath $probeLog
        }) | Out-Null

        if (-not $probeResult.succeeded) {
            throw "Individual test audit probe failed for '$($testProject.projectName)'."
        }
    }
    Write-JsonFile -Path $testsPath -Value $testEntries
}
catch {
    $auditFailed = $true
    $failureMessage = $_.Exception.Message
}
finally {
    Write-JsonFile -Path $payloadsPath -Value $payloadWitnesses

    $completedAt = (Get-Date).ToUniversalTime()
    $runMetadata.completedAtUtc = $completedAt.ToString('o')
    $runMetadata.outputDigests = [ordered]@{
        events = Get-FileSha256 -Path $eventsPath
        projects = Get-FileSha256 -Path $projectsPath
        tests = Get-FileSha256 -Path $testsPath
        payloads = Get-FileSha256 -Path $payloadsPath
    }
    $runMetadata.result = if ($auditFailed) { 'failed' } else { 'succeeded' }
    if ($failureMessage) {
        $runMetadata.failureMessage = $failureMessage
    }
    Write-JsonFile -Path $runPath -Value $runMetadata

    $summaryLines = @(
        '# Build Audit Summary',
        '',
        ('- Run ID: `{0}`' -f $runId),
        ('- Result: `{0}`' -f $runMetadata.result),
        ('- Commit: `{0}`' -f $runMetadata.commitSha),
        ('- Branch: `{0}`' -f $runMetadata.branch),
        ('- Worktree: `{0}`' -f $runMetadata.worktreeState),
        ('- Configuration: `{0}`' -f $Configuration),
        ('- HOPNG validation: `{0}`' -f $runMetadata.hopngValidationStatus),
        ''
    )

    if ($failureMessage) {
        $summaryLines += @(
            '## Failure',
            '',
            ('- `{0}`' -f $failureMessage),
            ''
        )
    }

    $summaryLines += @(
        '## Step Results',
        '',
        '| Step | Result | Duration (ms) | Exit |',
        '| --- | --- | ---: | ---: |'
    )

    foreach ($step in $stepResults) {
        $summaryLines += ('| {0} | {1} | {2} | {3} |' -f $step.step, $step.result, $step.durationMs, $step.exitCode)
    }

    $summaryLines += @(
        '',
        '## Payload Witnesses',
        '',
        '| Step | Classification | Note |',
        '| --- | --- | --- |'
    )

    foreach ($payload in $payloadWitnesses) {
        $summaryLines += ('| {0} | {1} | {2} |' -f $payload.step, $payload.classification, $payload.note)
    }

    Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8

    $auditStatus = if ($auditFailed) { 'failed' } else { 'succeeded' }
    Add-NdjsonLine -Path $eventsPath -Value (New-EventRecord -EventType 'AUDIT_RUN_COMPLETED' -RunId $runId -Status $auditStatus -Step 'build-audit' -Data @{ bundlePath = (Get-RelativePathString -BasePath $repoRoot -TargetPath $bundlePath); failureMessage = $failureMessage })
}

Write-Host ("[audit] Bundle: {0}" -f $bundlePath)
if ($auditFailed) {
    throw $failureMessage
}

$bundlePath
