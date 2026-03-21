param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [string] $RepoRoot,
    [string] $PolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $BaseRef,
    [string] $RequestedVersion,
    [switch] $ForceReleaseCandidate,
    [switch] $ForceDigest
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

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-OptionalDateTimeUtc {
    param([object] $Value)

    if ($null -eq $Value) {
        return $null
    }

    $stringValue = [string] $Value
    if ([string]::IsNullOrWhiteSpace($stringValue)) {
        return $null
    }

    return [datetime]::Parse($stringValue, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
}

function Get-ScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
}

function Get-LatestBundlePath {
    param([string] $RootPath)

    if (-not (Test-Path -LiteralPath $RootPath -PathType Container)) {
        return $null
    }

    $latest = Get-ChildItem -LiteralPath $RootPath -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'build-evidence-manifest.json') -PathType Leaf } |
        Sort-Object -Property Name -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        return $null
    }

    return $latest.FullName
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $PolicyPath
$policy = Get-Content -Raw -LiteralPath $resolvedPolicyPath | ConvertFrom-Json

$releaseCandidateOutputRoot = [string] $policy.releaseCandidateOutputRoot
$digestOutputRoot = [string] $policy.digestOutputRoot
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statePath)
$releaseCandidateRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $releaseCandidateOutputRoot
$digestRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $digestOutputRoot
$releaseCadenceHours = [int] $policy.localReleaseCandidateCadenceHours
$digestCadenceHours = [int] $policy.mandatoryHitlDigestCadenceHours
$digestWindowHours = [int] $policy.digestWindowHours
$blockedStatus = [string] $policy.blockedStatus

$state = Read-JsonFileOrNull -Path $statePath
$nowUtc = (Get-Date).ToUniversalTime()
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastReleaseCandidateRunUtc')
$lastDigestUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $state -PropertyName 'lastDigestUtc')

$releaseCandidateDue = $ForceReleaseCandidate.IsPresent
if (-not $releaseCandidateDue) {
    if ($null -eq $lastReleaseCandidateRunUtc) {
        $releaseCandidateDue = $true
    } else {
        $releaseCandidateDue = $lastReleaseCandidateRunUtc.AddHours($releaseCadenceHours) -le $nowUtc
    }
}

$releaseCandidateBundlePath = $null
if ($releaseCandidateDue) {
    $invokeReleaseCandidateScriptPath = Join-Path $resolvedRepoRoot 'tools\Invoke-Release-Candidate.ps1'
    $releaseCandidateArgs = @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $invokeReleaseCandidateScriptPath,
        '-Configuration', $Configuration,
        '-OutputRoot', $releaseCandidateOutputRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
        $releaseCandidateArgs += @('-BaseRef', $BaseRef)
    }

    if (-not [string]::IsNullOrWhiteSpace($RequestedVersion)) {
        $releaseCandidateArgs += @('-RequestedVersion', $RequestedVersion)
    }

    $releaseCandidateOutput = & powershell @releaseCandidateArgs

    $releaseCandidateBundlePath = Get-ScriptOutputTail -Output $releaseCandidateOutput
    if ([string]::IsNullOrWhiteSpace($releaseCandidateBundlePath)) {
        throw 'Local automation cycle did not receive a release-candidate bundle path.'
    }
} else {
    $releaseCandidateBundlePath = Get-LatestBundlePath -RootPath $releaseCandidateRunRoot
}

if ([string]::IsNullOrWhiteSpace($releaseCandidateBundlePath)) {
    throw 'Local automation cycle could not locate a release-candidate bundle.'
}

$manifestPath = Join-Path $releaseCandidateBundlePath 'build-evidence-manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Release-candidate manifest not found at '$manifestPath'."
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$latestStatus = [string] $manifest.status
$latestRunGeneratedAtUtc = [datetime]::Parse([string] $manifest.generatedAtUtc, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()

$digestDue = $ForceDigest.IsPresent
if (-not $digestDue) {
    if ($latestStatus -in @('hitl-required', $blockedStatus)) {
        $digestDue = $true
    } elseif ($null -eq $lastDigestUtc) {
        $digestDue = $true
    } else {
        $digestDue = $lastDigestUtc.AddHours($digestCadenceHours) -le $nowUtc
    }
}

$digestReportedNextMandatoryReviewUtc = if ($latestStatus -eq $blockedStatus) {
    $nowUtc
} elseif ($digestDue) {
    $nowUtc.AddHours($digestCadenceHours)
} elseif ($null -eq $lastDigestUtc) {
    $nowUtc.AddHours($digestCadenceHours)
} else {
    $lastDigestUtc.AddHours($digestCadenceHours)
}

$digestBundlePath = $null
if ($digestDue) {
    $writeDigestScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Release-Candidate-Digest.ps1'
    $digestArgs = @(
        '-ExecutionPolicy', 'Bypass',
        '-File', $writeDigestScriptPath,
        '-RepoRoot', $resolvedRepoRoot,
        '-RunRoot', $releaseCandidateOutputRoot,
        '-OutputRoot', $digestOutputRoot,
        '-WindowHours', $digestWindowHours,
        '-ReferenceTimeUtc', $nowUtc.ToString('o'),
        '-NextMandatoryReviewUtc', $digestReportedNextMandatoryReviewUtc.ToString('o')
    )

    $digestOutput = & powershell @digestArgs

    $digestBundlePath = Get-ScriptOutputTail -Output $digestOutput
    if ([string]::IsNullOrWhiteSpace($digestBundlePath)) {
        throw 'Local automation cycle did not receive a digest bundle path.'
    }
} else {
    if (Test-Path -LiteralPath $digestRunRoot -PathType Container) {
        $latestDigest = Get-ChildItem -LiteralPath $digestRunRoot -Directory |
            Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'release-candidate-digest.json') -PathType Leaf } |
            Sort-Object -Property Name -Descending |
            Select-Object -First 1

        if ($null -ne $latestDigest) {
            $digestBundlePath = $latestDigest.FullName
        }
    }
}

$newLastDigestUtc = if ($digestDue) { $nowUtc } else { $lastDigestUtc }
$nextMandatoryReviewUtc = if ($null -eq $newLastDigestUtc) {
    $nowUtc.AddHours($digestCadenceHours)
} else {
    $newLastDigestUtc.AddHours($digestCadenceHours)
}

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    policyPath = $resolvedPolicyPath
    cadenceHours = [ordered]@{
        releaseCandidate = $releaseCadenceHours
        mandatoryHitlDigest = $digestCadenceHours
        digestWindow = $digestWindowHours
    }
    lastReleaseCandidateRunUtc = $latestRunGeneratedAtUtc.ToString('o')
    lastReleaseCandidateBundle = $releaseCandidateBundlePath
    lastKnownStatus = $latestStatus
    lastDigestUtc = if ($null -ne $newLastDigestUtc) { $newLastDigestUtc.ToString('o') } else { $null }
    lastDigestBundle = $digestBundlePath
    nextReleaseCandidateRunUtc = $latestRunGeneratedAtUtc.AddHours($releaseCadenceHours).ToString('o')
    nextMandatoryHitlReviewUtc = $nextMandatoryReviewUtc.ToString('o')
    releaseCandidateTriggered = $releaseCandidateDue
    digestTriggered = $digestDue
}

Write-JsonFile -Path $statePath -Value $statePayload

$summary = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    statePath = $statePath
    lastReleaseCandidateBundle = $releaseCandidateBundlePath
    lastKnownStatus = $latestStatus
    lastDigestBundle = $digestBundlePath
    nextReleaseCandidateRunUtc = $statePayload.nextReleaseCandidateRunUtc
    nextMandatoryHitlReviewUtc = $statePayload.nextMandatoryHitlReviewUtc
}

$summaryPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-cycle-last-run.json'
Write-JsonFile -Path $summaryPath -Value $summary

$taskStatusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-Local-Automation-TaskStatus.ps1'
$taskStatusOutput = & powershell -ExecutionPolicy Bypass -File $taskStatusScriptPath -RepoRoot $resolvedRepoRoot
$taskStatusPath = Get-ScriptOutputTail -Output $taskStatusOutput

Write-Host ('[local-automation-cycle] Status: {0}' -f $latestStatus)
Write-Host ('[local-automation-cycle] State: {0}' -f $statePath)
if (-not [string]::IsNullOrWhiteSpace($taskStatusPath)) {
    Write-Host ('[local-automation-cycle] TaskStatus: {0}' -f $taskStatusPath)
}

if ($latestStatus -eq $blockedStatus) {
    throw 'Local automation cycle ended in blocked status.'
}

$summaryPath
