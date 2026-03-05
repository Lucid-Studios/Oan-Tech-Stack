param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Join-Path $repoRoot "OAN Mortalis V1.0"
$solutionPath = Join-Path $activeBuildRoot "Oan.sln"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Active solution not found at '$solutionPath'."
}

$buildArgs = @(
    "build",
    $solutionPath,
    "-c", $Configuration,
    "-v", "minimal"
)

if ($NoRestore) {
    $buildArgs += "--no-restore"
}

Write-Host "[build] Solution: $solutionPath"
Write-Host "[build] Configuration: $Configuration"

& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}
