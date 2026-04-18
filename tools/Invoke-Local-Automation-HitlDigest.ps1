[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json'
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

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Set-JsonNoteProperty {
    param(
        [object] $InputObject,
        [string] $PropertyName,
        [object] $Value
    )

    if ($null -eq $InputObject) {
        throw 'InputObject is required.'
    }

    if ($InputObject.PSObject.Properties[$PropertyName]) {
        $InputObject.PSObject.Properties[$PropertyName].Value = $Value
    } else {
        Add-Member -InputObject $InputObject -NotePropertyName $PropertyName -NotePropertyValue $Value -Force
    }
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

function Get-ScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$policy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statePath)
$cycleState = Read-JsonFileOrNull -Path $cycleStatePath

if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the HITL digest can run.'
}

$digestCadenceHours = if ($policy.PSObject.Properties['mandatoryHitlDigestCadenceHours']) {
    [int] $policy.mandatoryHitlDigestCadenceHours
} else {
    24
}
$digestWindowHours = [int] $policy.digestWindowHours
$digestScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Release-Candidate-Digest.ps1'
$schedulerSyncScriptPath = Join-Path $resolvedRepoRoot 'tools\Sync-Local-AutomationScheduler.ps1'
$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
$nowUtc = (Get-Date).ToUniversalTime()
$nextReviewUtc = $nowUtc.AddHours($digestCadenceHours)

$digestOutput = Invoke-ChildPowershellScript -ArgumentList @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $digestScriptPath,
    '-RepoRoot', $resolvedRepoRoot,
    '-RunRoot', ([string] $policy.releaseCandidateOutputRoot),
    '-OutputRoot', ([string] $policy.digestOutputRoot),
    '-WindowHours', $digestWindowHours.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '-ReferenceTimeUtc', $nowUtc.ToString('o'),
    '-NextMandatoryReviewUtc', $nextReviewUtc.ToString('o')
) -FailureContext 'Daily HITL digest writer'

$digestBundlePath = Get-ScriptOutputTail -Output $digestOutput
if ([string]::IsNullOrWhiteSpace($digestBundlePath)) {
    throw 'Local automation HITL digest did not receive a bundle path.'
}

Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'lastDigestUtc' -Value $nowUtc.ToString('o')
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'lastDigestBundle' -Value $digestBundlePath
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextMandatoryHitlReviewUtc' -Value $nextReviewUtc.ToString('o')
Set-JsonNoteProperty -InputObject $cycleState -PropertyName 'nextDailyHitlDigestRunUtc' -Value $nextReviewUtc.ToString('o')
Write-JsonFile -Path $cycleStatePath -Value $cycleState

Invoke-ChildPowershellScript -ArgumentList @(
    '-ExecutionPolicy', 'Bypass',
    '-File', $schedulerSyncScriptPath,
    '-Configuration', $Configuration,
    '-RepoRoot', $resolvedRepoRoot,
    '-CyclePolicyPath', $resolvedCyclePolicyPath
) -FailureContext 'Scheduler reconciliation after digest' | Out-Null

if (Test-Path -LiteralPath $taskStatusScriptPath -PathType Leaf) {
    Invoke-ChildPowershellScript -ArgumentList @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $taskStatusScriptPath,
        '-RepoRoot', $resolvedRepoRoot
    ) -FailureContext 'Task status writer after digest' | Out-Null
}

Write-Host ('[local-automation-hitl-digest] Bundle: {0}' -f $digestBundlePath)
$digestBundlePath
