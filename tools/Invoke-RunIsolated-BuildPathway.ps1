param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [switch] $SkipLocalAutomationCycle
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $RepoRoot = Split-Path -Parent $PSScriptRoot
    } else {
        $RepoRoot = (Get-Location).Path
    }
}

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Invoke-ChildPowershellScript {
    param(
        [string[]] $ArgumentList,
        [string] $FailureContext
    )

    $output = & powershell -NoProfile -NonInteractive -WindowStyle Hidden @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw '{0} failed with exit code {1}.' -f $FailureContext, $LASTEXITCODE
    }

    return @($output)
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath

if (-not $SkipLocalAutomationCycle.IsPresent) {
    $cycleScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Local-Automation-Cycle.ps1'
    Invoke-ChildPowershellScript -ArgumentList @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $cycleScriptPath,
        '-Configuration', $Configuration,
        '-RepoRoot', $resolvedRepoRoot,
        '-PolicyPath', $resolvedCyclePolicyPath
    ) -FailureContext 'Local automation cycle' | Out-Null
} else {
    $writerScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-RunIsolated-BuildPathway.ps1'
    Invoke-ChildPowershellScript -ArgumentList @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $writerScriptPath,
        '-RepoRoot', $resolvedRepoRoot,
        '-CyclePolicyPath', $resolvedCyclePolicyPath
    ) -FailureContext 'Run-isolated build pathway writer' | Out-Null
}

$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runIsolatedBuildPathwayStatePath)
$state = Read-JsonFileOrNull -Path $statePath
if ($null -eq $state) {
    throw 'Run-isolated build pathway state was not produced.'
}

$bundlePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $state.bundlePath)
Write-Host ('[run-isolated-build-pathway] State: {0}' -f $statePath)
Write-Host ('[run-isolated-build-pathway] Bundle: {0}' -f $bundlePath)
$bundlePath

