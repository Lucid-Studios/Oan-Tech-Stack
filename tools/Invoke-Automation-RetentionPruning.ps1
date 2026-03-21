param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.0/build/local-automation-tasking.json'
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

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    if (Test-Path -LiteralPath $resolvedBase -PathType Leaf) {
        $resolvedBase = Split-Path -Parent $resolvedBase
    }

    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('\', '/')
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

function Get-ProtectedDirectorySet {
    param(
        [string] $RepoRootPath,
        [object] $CycleState,
        [object] $BlockedEscalationState,
        [object] $TaskingPolicy,
        [string] $TaskingPolicyPathValue
    )

    $protected = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($candidate in @(
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastReleaseCandidateBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastDigestBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastSeededGovernanceBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $CycleState -PropertyName 'lastBlockedEscalationBundle'),
        [string] (Get-ObjectPropertyValueOrNull -InputObject $BlockedEscalationState -PropertyName 'bundlePath')
    )) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            [void] $protected.Add((Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath $candidate))
        }
    }

    $activeRunStatePath = Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath ([string] $TaskingPolicy.activeLongFormRunStatePath)
    $activeRunState = Read-JsonFileOrNull -Path $activeRunStatePath
    if ($null -ne $activeRunState) {
        $runRoot = Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath ([string] $TaskingPolicy.longFormRunOutputRoot)
        $activeRunId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeRunState -PropertyName 'runId')
        if (-not [string]::IsNullOrWhiteSpace($activeRunId)) {
            [void] $protected.Add((Join-Path $runRoot $activeRunId))
        }
    }

    return $protected
}

function Invoke-PruneRoot {
    param(
        [string] $RootPath,
        [int] $KeepCount,
        [System.Collections.Generic.HashSet[string]] $ProtectedDirectories,
        [string] $RepoRootPath
    )

    $result = [ordered]@{
        rootPath = Get-RelativePathString -BasePath $RepoRootPath -TargetPath $RootPath
        keepCount = $KeepCount
        discoveredCount = 0
        deleted = @()
        preserved = @()
    }

    if (-not (Test-Path -LiteralPath $RootPath -PathType Container)) {
        return $result
    }

    $directories = @(Get-ChildItem -LiteralPath $RootPath -Directory | Sort-Object Name -Descending)
    $result.discoveredCount = $directories.Count

    $retained = 0
    foreach ($directory in $directories) {
        $fullName = [System.IO.Path]::GetFullPath($directory.FullName)
        $shouldProtect = $ProtectedDirectories.Contains($fullName)

        if ($shouldProtect -or $retained -lt $KeepCount) {
            $retained += 1
            $result.preserved += (Get-RelativePathString -BasePath $RepoRootPath -TargetPath $directory.FullName)
            continue
        }

        Remove-Item -LiteralPath $directory.FullName -Recurse -Force
        $result.deleted += (Get-RelativePathString -BasePath $RepoRootPath -TargetPath $directory.FullName)
    }

    return $result
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$retentionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.retentionStatePath)
$blockedEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$blockedEscalationState = Read-JsonFileOrNull -Path $blockedEscalationStatePath
$protectedDirectories = Get-ProtectedDirectorySet -RepoRootPath $resolvedRepoRoot -CycleState $cycleState -BlockedEscalationState $blockedEscalationState -TaskingPolicy $taskingPolicy -TaskingPolicyPathValue $resolvedTaskingPolicyPath

$retentionPolicy = $cyclePolicy.retentionPolicy
$roots = @(
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseCandidateOutputRoot)
        keep = [int] $retentionPolicy.keepReleaseCandidateBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.digestOutputRoot)
        keep = [int] $retentionPolicy.keepDigestBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.longFormRunOutputRoot)
        keep = [int] $retentionPolicy.keepLongFormTaskMapRuns
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceOutputRoot)
        keep = [int] $retentionPolicy.keepSeededGovernanceBundles
    },
    [ordered]@{
        path = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationOutputRoot)
        keep = [int] $retentionPolicy.keepBlockedEscalationBundles
    }
)

$pruneResults = @(
    $roots |
    ForEach-Object {
        Invoke-PruneRoot -RootPath ([string] $_.path) -KeepCount ([int] $_.keep) -ProtectedDirectories $protectedDirectories -RepoRootPath $resolvedRepoRoot
    }
)

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    protectedDirectories = @($protectedDirectories | ForEach-Object { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $_ } | Sort-Object)
    pruneResults = $pruneResults
}

Write-JsonFile -Path $retentionStatePath -Value $statePayload
Write-Host ('[automation-retention-pruning] State: {0}' -f $retentionStatePath)
$retentionStatePath
