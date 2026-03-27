param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoRestore,

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

    Write-Host "[v1.1.1 build] Running workspace path hygiene preflight"
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

if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
    $buildArgs += ("-p:OanBuildVersion={0}" -f $BuildVersion)
}

if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
    $buildArgs += ("-p:OanAssemblyVersion={0}" -f $AssemblyVersion)
}

Write-Host "[v1.1.1 build] Solution: $solutionPath"
Write-Host "[v1.1.1 build] Configuration: $Configuration"

& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}
