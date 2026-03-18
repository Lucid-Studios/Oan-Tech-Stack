param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [string] $HostEndpoint,

    [string] $OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Split-Path -Parent $toolsRoot
$repoRoot = Split-Path -Parent $activeBuildRoot
$projectPath = Join-Path $activeBuildRoot "tests\Oan.Runtime.IntegrationTests\Oan.Runtime.IntegrationTests.csproj"

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Integration test project not found at '$projectPath'."
}

$resolvedHostEndpoint = if (-not [string]::IsNullOrWhiteSpace($HostEndpoint)) {
    $HostEndpoint
} elseif (-not [string]::IsNullOrWhiteSpace($env:OAN_SOULFRAME_HOST_URL)) {
    $env:OAN_SOULFRAME_HOST_URL
} else {
    "http://127.0.0.1:8181"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $stamp = [DateTimeOffset]::UtcNow.ToString("yyyyMMddTHHmmssZ")
    $OutputRoot = Join-Path $activeBuildRoot ".local\evals\local-llm-preflight\$stamp"
}

$gitCommit = $null
try {
    $gitCommit = (& git -C $repoRoot rev-parse HEAD).Trim()
} catch {
    $gitCommit = $null
}

Write-Host "[preflight] Host endpoint: $resolvedHostEndpoint"
Write-Host "[preflight] Output root: $OutputRoot"
if (-not [string]::IsNullOrWhiteSpace($gitCommit)) {
    Write-Host "[preflight] Git commit: $gitCommit"
}

$previousRunFlag = $env:OAN_RUN_LOCAL_LLM_PREFLIGHT
$previousHost = $env:OAN_LOCAL_LLM_PREFLIGHT_HOST_ENDPOINT
$previousOutput = $env:OAN_LOCAL_LLM_PREFLIGHT_OUTPUT_ROOT
$previousRunner = $env:OAN_LOCAL_LLM_PREFLIGHT_RUNNER_VERSION
$previousCommit = $env:OAN_LOCAL_LLM_PREFLIGHT_GIT_COMMIT

try {
    $env:OAN_RUN_LOCAL_LLM_PREFLIGHT = "1"
    $env:OAN_LOCAL_LLM_PREFLIGHT_HOST_ENDPOINT = $resolvedHostEndpoint
    $env:OAN_LOCAL_LLM_PREFLIGHT_OUTPUT_ROOT = $OutputRoot
    $env:OAN_LOCAL_LLM_PREFLIGHT_RUNNER_VERSION = "local-llm-preflight-runner-v1"
    if (-not [string]::IsNullOrWhiteSpace($gitCommit)) {
        $env:OAN_LOCAL_LLM_PREFLIGHT_GIT_COMMIT = $gitCommit
    }

    $testArgs = @(
        "test",
        $projectPath,
        "-c", $Configuration,
        "-v", "minimal",
        "--filter", "FullyQualifiedName~LocalLlmPreflightLiveHarnessTests"
    )

    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE."
    }

    $requiredArtifacts = @(
        "run-summary.json",
        "scenario-ledger.jsonl",
        "telemetry-records.jsonl",
        "summary.md"
    )

    foreach ($artifact in $requiredArtifacts) {
        $artifactPath = Join-Path $OutputRoot $artifact
        if (-not (Test-Path -LiteralPath $artifactPath -PathType Leaf)) {
            throw "Expected pre-flight artifact was not written: '$artifactPath'."
        }
    }

    Write-Host "[preflight] Artifacts written successfully."
    Write-Host "[preflight] Summary: $(Join-Path $OutputRoot 'summary.md')"
}
finally {
    $env:OAN_RUN_LOCAL_LLM_PREFLIGHT = $previousRunFlag
    $env:OAN_LOCAL_LLM_PREFLIGHT_HOST_ENDPOINT = $previousHost
    $env:OAN_LOCAL_LLM_PREFLIGHT_OUTPUT_ROOT = $previousOutput
    $env:OAN_LOCAL_LLM_PREFLIGHT_RUNNER_VERSION = $previousRunner
    $env:OAN_LOCAL_LLM_PREFLIGHT_GIT_COMMIT = $previousCommit
}
