Set-StrictMode -Version Latest

function Get-LineVerificationMutexName {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $LineRoot
    )

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\')
    $resolvedLineRoot = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepositoryRoot $LineRoot)).TrimEnd('\')
    $identity = "line-verification|$resolvedRepositoryRoot|$resolvedLineRoot"

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($identity))
    }
    finally {
        $sha256.Dispose()
    }

    $hash = -join ($hashBytes | ForEach-Object { $_.ToString("x2") })
    return "Local\OanTechStack.LineVerification.$hash"
}

function Use-LineVerificationLock {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $LineRoot,

        [Parameter(Mandatory)]
        [string] $OperationName,

        [Parameter(Mandatory)]
        [scriptblock] $ScriptBlock,

        [ValidateRange(1, 7200)]
        [int] $TimeoutSeconds = 900
    )

    $mutexName = Get-LineVerificationMutexName -RepositoryRoot $RepositoryRoot -LineRoot $LineRoot
    $mutex = $null
    $lockAcquired = $false
    $waitStopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $mutex = New-Object System.Threading.Mutex($false, $mutexName)
        Write-Host ("[{0}] Waiting for shared line verification lock" -f $OperationName)

        try {
            $lockAcquired = $mutex.WaitOne([TimeSpan]::FromSeconds($TimeoutSeconds))
        }
        catch [System.Threading.AbandonedMutexException] {
            $lockAcquired = $true
            Write-Warning ("[{0}] Recovered an abandoned shared line verification lock." -f $OperationName)
        }

        if (-not $lockAcquired) {
            throw ("[{0}] Timed out waiting for the shared line verification lock after {1} seconds." -f $OperationName, $TimeoutSeconds)
        }

        $waitStopwatch.Stop()
        Write-Host ("[{0}] Acquired shared line verification lock after {1:N1}s" -f $OperationName, $waitStopwatch.Elapsed.TotalSeconds)

        & $ScriptBlock
    }
    finally {
        if ($waitStopwatch.IsRunning) {
            $waitStopwatch.Stop()
        }

        if ($lockAcquired -and $null -ne $mutex) {
            try {
                [void] $mutex.ReleaseMutex()
                Write-Host ("[{0}] Released shared line verification lock" -f $OperationName)
            }
            catch [System.ApplicationException] {
                Write-Warning ("[{0}] Shared line verification lock was not owned at release time." -f $OperationName)
            }
        }

        if ($null -ne $mutex) {
            $mutex.Dispose()
        }
    }
}
