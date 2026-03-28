param(
    [string] $CradleTekRoot = '',
    [string] $HostEndpoint = '',
    [int] $StartupWaitSeconds = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RuntimeRoot {
    param([string] $ConfiguredRoot)

    if (-not [string]::IsNullOrWhiteSpace($ConfiguredRoot)) {
        return [System.IO.Path]::GetFullPath($ConfiguredRoot)
    }

    $fromEnv = [Environment]::GetEnvironmentVariable('CRADLETEK_RUNTIME_ROOT', 'Process')
    if ([string]::IsNullOrWhiteSpace($fromEnv)) {
        $fromEnv = [Environment]::GetEnvironmentVariable('CRADLETEK_RUNTIME_ROOT', 'User')
    }

    if (-not [string]::IsNullOrWhiteSpace($fromEnv)) {
        return [System.IO.Path]::GetFullPath($fromEnv)
    }

    return Join-Path $env:SystemDrive 'CradleTek'
}

function Resolve-HostUrl {
    param([string] $ConfiguredHostEndpoint)

    if (-not [string]::IsNullOrWhiteSpace($ConfiguredHostEndpoint)) {
        return $ConfiguredHostEndpoint
    }

    $configured = [Environment]::GetEnvironmentVariable('OAN_SOULFRAME_HOST_URL', 'Process')
    if ([string]::IsNullOrWhiteSpace($configured)) {
        $configured = [Environment]::GetEnvironmentVariable('OAN_SOULFRAME_HOST_URL', 'User')
    }

    if ([string]::IsNullOrWhiteSpace($configured)) {
        return 'http://127.0.0.1:8181'
    }

    return $configured
}

function Test-HostEndpointReachable {
    param([uri] $Endpoint)

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $async = $client.BeginConnect($Endpoint.Host, $Endpoint.Port, $null, $null)
        if (-not $async.AsyncWaitHandle.WaitOne([TimeSpan]::FromSeconds(2))) {
            return $false
        }

        $client.EndConnect($async)
        return $true
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

function Test-HealthEndpoint {
    param([string] $HostUrl)

    try {
        $health = Invoke-RestMethod -Method Get -Uri ($HostUrl.TrimEnd('/') + '/health') -TimeoutSec 5
        return [ordered]@{
            reachable = $true
            status = if ($null -ne $health.status) { [string] $health.status } else { 'healthy' }
        }
    }
    catch {
        return [ordered]@{
            reachable = $false
            status = 'service-unreachable'
        }
    }
}

function Resolve-ConfiguredModelPath {
    param(
        [string] $Root,
        [string] $ConfigPath
    )

    $envModelPath = [Environment]::GetEnvironmentVariable('OAN_MODEL_PATH', 'Process')
    if ([string]::IsNullOrWhiteSpace($envModelPath)) {
        $envModelPath = [Environment]::GetEnvironmentVariable('OAN_MODEL_PATH', 'User')
    }

    if (-not [string]::IsNullOrWhiteSpace($envModelPath)) {
        return [System.IO.Path]::GetFullPath($envModelPath)
    }

    if (Test-Path -LiteralPath $ConfigPath -PathType Leaf) {
        $config = Get-Content -Raw -LiteralPath $ConfigPath | ConvertFrom-Json
        if ($null -ne $config.model_path -and -not [string]::IsNullOrWhiteSpace([string] $config.model_path)) {
            return [System.IO.Path]::GetFullPath([string] $config.model_path)
        }
    }

    $preferred = Join-Path $Root 'models\seed.gguf'
    if (Test-Path -LiteralPath $preferred -PathType Leaf) {
        return [System.IO.Path]::GetFullPath($preferred)
    }

    $modelsRoot = Join-Path $Root 'models'
    if (Test-Path -LiteralPath $modelsRoot -PathType Container) {
        $firstModel = Get-ChildItem -Path $modelsRoot -Filter *.gguf -File -ErrorAction SilentlyContinue |
            Sort-Object Name |
            Select-Object -First 1
        if ($null -ne $firstModel) {
            return $firstModel.FullName
        }
    }

    return [System.IO.Path]::GetFullPath($preferred)
}

function Get-RuntimeState {
    param(
        [bool] $BinaryPresent,
        [bool] $ModelPresent
    )

    if ($BinaryPresent -and $ModelPresent) {
        return 'ready-for-inference'
    }

    if ($BinaryPresent) {
        return 'ready-for-model-drop'
    }

    if ($ModelPresent) {
        return 'runtime-binary-missing'
    }

    return 'runtime-binary-and-model-missing'
}

function Resolve-LlamaCli {
    param([string] $Root)

    $candidates = @(
        (Join-Path $Root 'runtime\llama.cpp\bin\llama-cli.exe'),
        (Join-Path $Root 'runtime\llama.cpp\build\bin\Release\llama-cli.exe'),
        (Join-Path $Root 'runtime\llama.cpp\bin\main.exe'),
        (Join-Path $Root 'runtime\llama.cpp\build\bin\Release\main.exe')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }

    return $null
}

function Start-InferenceServiceIfPossible {
    param(
        [string] $Root,
        [string] $HostUrl
    )

    $runtimeDir = Join-Path $Root 'runtime'
    $serviceDir = Join-Path $runtimeDir 'inference_service'
    $servicePath = Join-Path $serviceDir 'app.py'
    $configPath = Join-Path $runtimeDir 'config.json'
    $venvPython = Join-Path $runtimeDir 'venv\Scripts\python.exe'
    $pidPath = Join-Path $runtimeDir 'inference_service.pid'

    if (-not (Test-Path -LiteralPath $servicePath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $configPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $venvPython -PathType Leaf)) {
        return [ordered]@{
            attempted = $false
            started = $false
            reason = 'seed-runtime-service-files-missing'
            pid = $null
        }
    }

    if (Test-Path -LiteralPath $pidPath -PathType Leaf) {
        $pidValue = Get-Content -LiteralPath $pidPath -ErrorAction SilentlyContinue
        if ($pidValue -match '^\d+$') {
            $running = Get-Process -Id ([int] $pidValue) -ErrorAction SilentlyContinue
            if ($null -ne $running) {
                return [ordered]@{
                    attempted = $true
                    started = $true
                    reason = 'seed-runtime-already-running'
                    pid = [int] $pidValue
                }
            }
        }
    }

    $config = Get-Content -Raw -LiteralPath $configPath | ConvertFrom-Json
    $port = if ($null -ne $config.inference_port) { [int] $config.inference_port } else { 8181 }
    $modelPath = Resolve-ConfiguredModelPath -Root $Root -ConfigPath $configPath

    [Environment]::SetEnvironmentVariable('CRADLETEK_RUNTIME_ROOT', $Root, 'Process')
    [Environment]::SetEnvironmentVariable('SOULFRAME_API_PORT', "$port", 'Process')
    [Environment]::SetEnvironmentVariable('OAN_MODEL_PATH', $modelPath, 'Process')
    [Environment]::SetEnvironmentVariable('OAN_SOULFRAME_HOST_URL', $HostUrl, 'Process')

    $pythonw = Join-Path (Split-Path -Parent $venvPython) 'pythonw.exe'
    $launchExecutable = if (Test-Path -LiteralPath $pythonw -PathType Leaf) { $pythonw } else { $venvPython }

    $process = Start-Process -FilePath $launchExecutable -ArgumentList 'app.py' -WorkingDirectory $serviceDir -WindowStyle Hidden -PassThru

    if ($null -ne $process) {
        $process.Id | Set-Content -LiteralPath $pidPath -Encoding utf8
    }

    return [ordered]@{
        attempted = $true
        started = $true
        reason = 'seed-runtime-start-attempted'
        pid = if ($null -ne $process) { $process.Id } else { $null }
    }
}

$root = Resolve-RuntimeRoot -ConfiguredRoot $CradleTekRoot
$hostUrl = Resolve-HostUrl -ConfiguredHostEndpoint $HostEndpoint
$endpointUri = [uri] $hostUrl
$configPath = Join-Path $root 'runtime\config.json'
$servicePath = Join-Path $root 'runtime\inference_service\app.py'
$venvPython = Join-Path $root 'runtime\venv\Scripts\python.exe'
$llamaCli = Resolve-LlamaCli -Root $root
$modelPath = Resolve-ConfiguredModelPath -Root $root -ConfigPath $configPath
$binaryPresent = -not [string]::IsNullOrWhiteSpace($llamaCli)
$modelPresent = Test-Path -LiteralPath $modelPath -PathType Leaf
$runtimeState = Get-RuntimeState -BinaryPresent:$binaryPresent -ModelPresent:$modelPresent
$preHealth = Test-HealthEndpoint -HostUrl $hostUrl
$hostReachable = [bool] $preHealth.reachable

$readyState = 'ready'
$reasonCode = 'seed-runtime-already-healthy'
$actionTaken = 'none'
$startAttempted = $false
$startSucceeded = $false
$servicePid = $null

if (-not $hostReachable) {
    $serviceReady = (Test-Path -LiteralPath $configPath -PathType Leaf) -and
        (Test-Path -LiteralPath $servicePath -PathType Leaf) -and
        (Test-Path -LiteralPath $venvPython -PathType Leaf) -and
        ($runtimeState -eq 'ready-for-inference')

    if (-not $serviceReady) {
        $readyState = 'not-ready'
        $reasonCode = 'seed-runtime-not-provisioned'
        $actionTaken = 'none'
    } else {
        $startResult = Start-InferenceServiceIfPossible -Root $root -HostUrl $hostUrl
        $startAttempted = [bool] $startResult.attempted
        $servicePid = $startResult.pid
        $actionTaken = [string] $startResult.reason

        $deadline = (Get-Date).ToUniversalTime().AddSeconds([Math]::Max(5, $StartupWaitSeconds))
        do {
            Start-Sleep -Seconds 2
            $health = Test-HealthEndpoint -HostUrl $hostUrl
            if ([bool] $health.reachable) {
                $hostReachable = $true
                $startSucceeded = $true
                $readyState = 'ready'
                $reasonCode = 'seed-runtime-started'
                break
            }
        } while ((Get-Date).ToUniversalTime() -lt $deadline)

        if (-not $hostReachable) {
            $readyState = 'not-ready'
            $reasonCode = 'seed-runtime-start-failed'
        }
    }
}

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    hostEndpoint = $hostUrl
    hostReachable = $hostReachable
    readyState = $readyState
    reasonCode = $reasonCode
    actionTaken = $actionTaken
    startAttempted = $startAttempted
    startSucceeded = $startSucceeded
    servicePid = $servicePid
    runtimeRootPresent = Test-Path -LiteralPath $root -PathType Container
    runtimeConfigPresent = Test-Path -LiteralPath $configPath -PathType Leaf
    runtimeServicePresent = Test-Path -LiteralPath $servicePath -PathType Leaf
    venvPythonPresent = Test-Path -LiteralPath $venvPython -PathType Leaf
    runtimeState = $runtimeState
    healthStatus = [string] (Test-HealthEndpoint -HostUrl $hostUrl).status
}

$payload | ConvertTo-Json -Depth 8
