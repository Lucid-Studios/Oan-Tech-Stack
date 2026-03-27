param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [switch] $NoRestore,

    [string] $BuildVersion,

    [string] $AssemblyVersion,

    [switch] $SkipHygieneCheck,

    [switch] $ValidateHopng,

    [switch] $HopngPrimeInspect,

    [switch] $HopngCompareSurface,

    [string] $HdtRoot,

    [string] $HopngArtifactPath,

    [string] $HopngCompareArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Join-Path $repoRoot "OAN Mortalis V1.1.1"
$solutionPath = Join-Path $activeBuildRoot "Oan.sln"
$hygieneScriptPath = Join-Path $activeBuildRoot "tools\verify-private-corpus.ps1"
$hopngValidationScriptPath = Join-Path $activeBuildRoot "tools\verify-hopng-toolchain.ps1"

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

if ($ValidateHopng) {
    if (-not (Test-Path -LiteralPath $hopngValidationScriptPath -PathType Leaf)) {
        throw "HDT validation script not found at '$hopngValidationScriptPath'."
    }

    Write-Host "[build] Running optional .hopng validation preflight"
    $hopngArgs = @(
        "-ExecutionPolicy", "Bypass",
        "-File", $hopngValidationScriptPath
    )

    if (-not [string]::IsNullOrWhiteSpace($HdtRoot)) {
        $hopngArgs += @("-HdtRoot", $HdtRoot)
    }

    if (-not [string]::IsNullOrWhiteSpace($HopngArtifactPath)) {
        $hopngArgs += @("-ArtifactPath", $HopngArtifactPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($HopngCompareArtifactPath)) {
        $hopngArgs += @("-CompareArtifactPath", $HopngCompareArtifactPath)
    }

    if ($HopngPrimeInspect) {
        $hopngArgs += "-PrimeInspect"
    }

    if ($HopngCompareSurface) {
        $hopngArgs += "-CompareSurface"
    }

    & powershell @hopngArgs
    if ($LASTEXITCODE -ne 0) {
        throw ".hopng validation preflight failed with exit code $LASTEXITCODE."
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

Write-Host "[build] Solution: $solutionPath"
Write-Host "[build] Configuration: $Configuration"
if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
    Write-Host "[build] Build version: $BuildVersion"
}
if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
    Write-Host "[build] Assembly version: $AssemblyVersion"
}

& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}
