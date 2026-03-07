param(
    [string] $HdtRoot,

    [string] $ArtifactPath,

    [string] $CompareArtifactPath,

    [switch] $PrimeInspect,

    [switch] $CompareSurface
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$activeBuildRoot = Split-Path -Parent $toolRoot
$localRoot = Join-Path $activeBuildRoot ".local"
$localHdtRootPath = Join-Path $localRoot "hdt_root.txt"
$localArtifactPath = Join-Path $localRoot "hopng_validation_target.txt"
$localComparePath = Join-Path $localRoot "hopng_compare_target.txt"

function Get-PreferredValue {
    param(
        [string] $ExplicitValue,
        [string] $LocalConfigPath,
        [string] $EnvironmentVariableName,
        [string] $FallbackValue
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitValue)) {
        return $ExplicitValue.Trim()
    }

    if (Test-Path -LiteralPath $LocalConfigPath -PathType Leaf) {
        $value = (Get-Content -LiteralPath $LocalConfigPath -Raw).Trim()
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    $envValue = [Environment]::GetEnvironmentVariable($EnvironmentVariableName)
    if (-not [string]::IsNullOrWhiteSpace($envValue)) {
        return $envValue.Trim()
    }

    return $FallbackValue
}

$resolvedHdtRoot = Get-PreferredValue -ExplicitValue $HdtRoot -LocalConfigPath $localHdtRootPath -EnvironmentVariableName "HDT_ROOT" -FallbackValue ""
if ([string]::IsNullOrWhiteSpace($resolvedHdtRoot)) {
    throw "HDT root was not provided. Set -HdtRoot, define HDT_ROOT, or create '$localHdtRootPath'."
}

if (-not (Test-Path -LiteralPath $resolvedHdtRoot -PathType Container)) {
    throw "Resolved HDT root '$resolvedHdtRoot' does not exist."
}

$testScriptPath = Join-Path $resolvedHdtRoot "Test-HOPNG.ps1"
$showScriptPath = Join-Path $resolvedHdtRoot "Show-HOPNG.ps1"
$compareScriptPath = Join-Path $resolvedHdtRoot "Compare-HOPNGSurfaces.ps1"

if (-not (Test-Path -LiteralPath $testScriptPath -PathType Leaf)) {
    throw "HDT validation script not found at '$testScriptPath'."
}

$defaultArtifactPath = Join-Path $resolvedHdtRoot "examples\phase2-sample.hopng.json"
$resolvedArtifactPath = Get-PreferredValue -ExplicitValue $ArtifactPath -LocalConfigPath $localArtifactPath -EnvironmentVariableName "OAN_HOPNG_ARTIFACT" -FallbackValue $defaultArtifactPath
if (-not (Test-Path -LiteralPath $resolvedArtifactPath -PathType Leaf)) {
    throw "Resolved .hopng validation target '$resolvedArtifactPath' does not exist."
}

Write-Host "[hopng] HDT root: $resolvedHdtRoot"
Write-Host "[hopng] Validation target: $resolvedArtifactPath"
& powershell -ExecutionPolicy Bypass -File $testScriptPath --path $resolvedArtifactPath --json
if ($LASTEXITCODE -ne 0) {
    throw ".hopng validation failed with exit code $LASTEXITCODE."
}

if ($PrimeInspect) {
    if (-not (Test-Path -LiteralPath $showScriptPath -PathType Leaf)) {
        throw "HDT inspection script not found at '$showScriptPath'."
    }

    Write-Host "[hopng] Running Prime-safe inspection"
    & powershell -ExecutionPolicy Bypass -File $showScriptPath --path $resolvedArtifactPath --view prime --json
    if ($LASTEXITCODE -ne 0) {
        throw ".hopng prime-safe inspection failed with exit code $LASTEXITCODE."
    }
}

if ($CompareSurface) {
    if (-not (Test-Path -LiteralPath $compareScriptPath -PathType Leaf)) {
        throw "HDT comparison script not found at '$compareScriptPath'."
    }

    $defaultComparePath = Join-Path $resolvedHdtRoot "examples\phase1-sample.hopng.json"
    $resolvedComparePath = Get-PreferredValue -ExplicitValue $CompareArtifactPath -LocalConfigPath $localComparePath -EnvironmentVariableName "OAN_HOPNG_COMPARE_ARTIFACT" -FallbackValue $defaultComparePath
    if (-not (Test-Path -LiteralPath $resolvedComparePath -PathType Leaf)) {
        throw "Resolved .hopng comparison target '$resolvedComparePath' does not exist."
    }

    Write-Host "[hopng] Comparing artifact surfaces"
    & powershell -ExecutionPolicy Bypass -File $compareScriptPath --left $resolvedArtifactPath --right $resolvedComparePath --json
    if ($LASTEXITCODE -ne 0) {
        throw ".hopng comparison failed with exit code $LASTEXITCODE."
    }
}
