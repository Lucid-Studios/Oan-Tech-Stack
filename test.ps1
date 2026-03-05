param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Join-Path $repoRoot "OAN Mortalis V1.0"
$solutionPath = Join-Path $activeBuildRoot "Oan.sln"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Active solution not found at '$solutionPath'."
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

Write-Host "[test] Solution: $solutionPath"
Write-Host "[test] Configuration: $Configuration"

& dotnet @testArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed with exit code $LASTEXITCODE."
}
