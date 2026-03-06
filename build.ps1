param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoRestore,

    [switch] $SkipHygieneCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Join-Path $repoRoot "OAN Mortalis V1.0"
$solutionPath = Join-Path $activeBuildRoot "Oan.sln"
$hygieneScriptPath = Join-Path $activeBuildRoot "tools\verify-private-corpus.ps1"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Active solution not found at '$solutionPath'."
}

if (-not $SkipHygieneCheck) {
    if (-not (Test-Path -LiteralPath $hygieneScriptPath -PathType Leaf)) {
        throw "Workspace hygiene script not found at '$hygieneScriptPath'."
    }

    Write-Host "[build] Running workspace path hygiene preflight"
    & powershell -ExecutionPolicy Bypass -File $hygieneScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "Workspace path hygiene failed with exit code $LASTEXITCODE."
    }
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
