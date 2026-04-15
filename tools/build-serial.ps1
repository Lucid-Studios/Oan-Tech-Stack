param(
    [string]$Configuration = "Release",
    [ValidateRange(1, 7200)]
    [int]$VerificationLockTimeoutSeconds = 900,
    [string]$BuildVersion,
    [string]$AssemblyVersion
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "[build-serial] Running build then test sequentially" -ForegroundColor Cyan
Write-Host "[build-serial] Repository: $repoRoot" -ForegroundColor DarkGray
Write-Host "[build-serial] Configuration: $Configuration" -ForegroundColor DarkGray
Write-Host "[build-serial] Verification lock timeout seconds: $VerificationLockTimeoutSeconds" -ForegroundColor DarkGray
$buildArgs = @(
    "-ExecutionPolicy", "Bypass",
    "-File", (Join-Path $repoRoot "build.ps1"),
    "-Configuration", $Configuration,
    "-VerificationLockTimeoutSeconds", $VerificationLockTimeoutSeconds
)

$testArgs = @(
    "-ExecutionPolicy", "Bypass",
    "-File", (Join-Path $repoRoot "test.ps1"),
    "-Configuration", $Configuration,
    "-VerificationLockTimeoutSeconds", $VerificationLockTimeoutSeconds
)

if (-not [string]::IsNullOrWhiteSpace($BuildVersion)) {
    Write-Host "[build-serial] Build version: $BuildVersion" -ForegroundColor DarkGray
    $buildArgs += @("-BuildVersion", $BuildVersion)
    $testArgs += @("-BuildVersion", $BuildVersion)
}

if (-not [string]::IsNullOrWhiteSpace($AssemblyVersion)) {
    Write-Host "[build-serial] Assembly version: $AssemblyVersion" -ForegroundColor DarkGray
    $buildArgs += @("-AssemblyVersion", $AssemblyVersion)
    $testArgs += @("-AssemblyVersion", $AssemblyVersion)
}

& powershell @buildArgs
& powershell @testArgs

Write-Host "[build-serial] Complete" -ForegroundColor Green
