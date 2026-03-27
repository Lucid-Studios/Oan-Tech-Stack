param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoBuild,

    [string] $BuildVersion,

    [string] $AssemblyVersion,

    [switch] $SkipHygieneCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$lineRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $lineRoot "Oan.sln"
$hygieneScriptPath = Join-Path $lineRoot "tools\verify-private-corpus.ps1"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Line-local solution not found at '$solutionPath'."
}

if (-not $SkipHygieneCheck) {
    if (-not (Test-Path -LiteralPath $hygieneScriptPath -PathType Leaf)) {
        throw "Line-local hygiene script not found at '$hygieneScriptPath'."
    }

    Write-Host "[v1.1.1 test] Running workspace path hygiene preflight"
    & powershell -ExecutionPolicy Bypass -File $hygieneScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "Workspace path hygiene failed with exit code $LASTEXITCODE."
    }
}

$testArgs = @(
    "test",
    $solutionPath,
    "-c", $Configuration,
    "-v", "minimal"
)

if ($NoBuild) {
    $testArgs += "--no-build"
}

if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
    $testArgs += ("-p:OanBuildVersion={0}" -f $BuildVersion)
}

if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
    $testArgs += ("-p:OanAssemblyVersion={0}" -f $AssemblyVersion)
}

Write-Host "[v1.1.1 test] Solution: $solutionPath"
Write-Host "[v1.1.1 test] Configuration: $Configuration"

& dotnet @testArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed with exit code $LASTEXITCODE."
}
