param(
    [string]$CradleTekRoot = ""
)

$ErrorActionPreference = "Stop"

function Resolve-RuntimeRoot {
    param([string]$ConfiguredRoot)

    if (-not [string]::IsNullOrWhiteSpace($ConfiguredRoot)) {
        return [System.IO.Path]::GetFullPath($ConfiguredRoot)
    }

    $fromEnv = [Environment]::GetEnvironmentVariable("CRADLETEK_RUNTIME_ROOT", "Process")
    if ([string]::IsNullOrWhiteSpace($fromEnv)) {
        $fromEnv = [Environment]::GetEnvironmentVariable("CRADLETEK_RUNTIME_ROOT", "User")
    }

    if (-not [string]::IsNullOrWhiteSpace($fromEnv)) {
        return [System.IO.Path]::GetFullPath($fromEnv)
    }

    return Join-Path $env:SystemDrive "CradleTek"
}

function Resolve-LlamaCli {
    param([string]$Root)

    $candidates = @(
        (Join-Path $Root "runtime\llama.cpp\bin\llama-cli.exe"),
        (Join-Path $Root "runtime\llama.cpp\build\bin\Release\llama-cli.exe"),
        (Join-Path $Root "runtime\llama.cpp\bin\main.exe"),
        (Join-Path $Root "runtime\llama.cpp\build\bin\Release\main.exe")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }

    return $null
}

function Resolve-ConfiguredModelPath {
    param(
        [string]$Root,
        [string]$ConfigPath
    )

    $envModelPath = [Environment]::GetEnvironmentVariable("OAN_MODEL_PATH", "Process")
    if ([string]::IsNullOrWhiteSpace($envModelPath)) {
        $envModelPath = [Environment]::GetEnvironmentVariable("OAN_MODEL_PATH", "User")
    }

    if (-not [string]::IsNullOrWhiteSpace($envModelPath)) {
        return [System.IO.Path]::GetFullPath($envModelPath)
    }

    if (Test-Path $ConfigPath) {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
        if ($null -ne $config.model_path -and -not [string]::IsNullOrWhiteSpace([string]$config.model_path)) {
            return [System.IO.Path]::GetFullPath([string]$config.model_path)
        }
    }

    $preferred = Join-Path $Root "models\seed.gguf"
    if (Test-Path $preferred) {
        return [System.IO.Path]::GetFullPath($preferred)
    }

    $modelsRoot = Join-Path $Root "models"
    if (Test-Path $modelsRoot) {
        $firstModel = Get-ChildItem -Path $modelsRoot -Filter *.gguf -File -ErrorAction SilentlyContinue |
            Sort-Object Name |
            Select-Object -First 1
        if ($null -ne $firstModel) {
            return $firstModel.FullName
        }
    }

    return [System.IO.Path]::GetFullPath($preferred)
}

function Resolve-HostUrl {
    $configured = [Environment]::GetEnvironmentVariable("OAN_SOULFRAME_HOST_URL", "Process")
    if ([string]::IsNullOrWhiteSpace($configured)) {
        $configured = [Environment]::GetEnvironmentVariable("OAN_SOULFRAME_HOST_URL", "User")
    }

    if ([string]::IsNullOrWhiteSpace($configured)) {
        return "http://127.0.0.1:8181"
    }

    return $configured
}

function Get-RuntimeState {
    param(
        [bool]$BinaryPresent,
        [bool]$ModelPresent
    )

    if ($BinaryPresent -and $ModelPresent) {
        return "ready-for-inference"
    }

    if ($BinaryPresent) {
        return "ready-for-model-drop"
    }

    if ($ModelPresent) {
        return "runtime-binary-missing"
    }

    return "runtime-binary-and-model-missing"
}

$root = Resolve-RuntimeRoot -ConfiguredRoot $CradleTekRoot
$configPath = Join-Path $root "runtime\config.json"
$servicePath = Join-Path $root "runtime\inference_service\app.py"
$venvPython = Join-Path $root "runtime\venv\Scripts\python.exe"
$llamaCli = Resolve-LlamaCli -Root $root
$modelPath = Resolve-ConfiguredModelPath -Root $root -ConfigPath $configPath
$hostUrl = Resolve-HostUrl
$binaryPresent = -not [string]::IsNullOrWhiteSpace($llamaCli)
$modelPresent = Test-Path $modelPath
$runtimeState = Get-RuntimeState -BinaryPresent:$binaryPresent -ModelPresent:$modelPresent

$serviceHealthy = $false
$healthStatus = $null
try {
    $health = Invoke-RestMethod -Method Get -Uri ($hostUrl.TrimEnd('/') + "/health") -TimeoutSec 5
    $serviceHealthy = $true
    $healthStatus = $health.status
}
catch {
    $healthStatus = "service-unreachable"
}

$result = [ordered]@{
    runtime_root = $root
    runtime_binary_exists = $binaryPresent
    runtime_binary_path = $llamaCli
    model_path = $modelPath
    model_present = $modelPresent
    config_present = Test-Path $configPath
    service_script_present = Test-Path $servicePath
    venv_python_present = Test-Path $venvPython
    host_url = $hostUrl
    service_healthy = $serviceHealthy
    health_status = $healthStatus
    governance_protocol_enabled = Test-Path $servicePath
    runtime_state = $runtimeState
}

$result | ConvertTo-Json -Depth 4

if (-not $binaryPresent -or -not (Test-Path $configPath) -or -not (Test-Path $servicePath) -or -not (Test-Path $venvPython)) {
    exit 1
}

exit 0
