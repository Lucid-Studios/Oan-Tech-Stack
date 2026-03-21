param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "[build-serial] Running build then test sequentially" -ForegroundColor Cyan
Write-Host "[build-serial] Repository: $repoRoot" -ForegroundColor DarkGray
Write-Host "[build-serial] Configuration: $Configuration" -ForegroundColor DarkGray

& powershell -ExecutionPolicy Bypass -File (Join-Path $repoRoot "build.ps1") -Configuration $Configuration
& powershell -ExecutionPolicy Bypass -File (Join-Path $repoRoot "test.ps1") -Configuration $Configuration

Write-Host "[build-serial] Complete" -ForegroundColor Green
