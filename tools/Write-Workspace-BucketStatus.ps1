param(
    [string] $RepoRoot,
    [string] $BucketPolicyPath = 'OAN Mortalis V1.0/build/workspace-bucket-groups.json',
    [string] $FamilyMaturityPath = 'OAN Mortalis V1.0/build/family-maturity.json',
    [string] $DeployablesPath = 'OAN Mortalis V1.0/build/deployables.json',
    [string] $TaskStatusPath = '.audit/state/local-automation-tasking-status.json'
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

function Read-JsonFile {
    param([string] $Path)

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
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

function Normalize-RepoRelativePath {
    param([string] $Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ''
    }

    $normalized = ($Path -replace '\\', '/').Trim('"')
    return $normalized.TrimStart('.', '/')
}

function Test-MatchesAnyPattern {
    param(
        [string] $Path,
        [object[]] $Patterns
    )

    $normalizedPath = Normalize-RepoRelativePath -Path $Path
    foreach ($patternValue in @($Patterns)) {
        if ($null -eq $patternValue) {
            continue
        }

        $pattern = Normalize-RepoRelativePath -Path ([string] $patternValue)
        if ([string]::IsNullOrWhiteSpace($pattern)) {
            continue
        }

        if ($normalizedPath -like $pattern) {
            return $true
        }
    }

    return $false
}

function New-UniqueStringArray {
    param([object[]] $Values)

    $items = New-Object System.Collections.Generic.List[string]
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($value in $Values) {
        if ($null -eq $value) {
            continue
        }

        $stringValue = [string] $value
        if ([string]::IsNullOrWhiteSpace($stringValue)) {
            continue
        }

        if ($seen.Add($stringValue)) {
            [void] $items.Add($stringValue)
        }
    }

    return [string[]] $items.ToArray()
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

function Get-GitChangedRepoPaths {
    param([string] $RepositoryRoot)

    $output = & git -C $RepositoryRoot status --porcelain=v1 2>$null
    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    $paths = New-Object System.Collections.Generic.List[string]
    foreach ($line in @($output)) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.Length -lt 4) {
            continue
        }

        $payload = $line.Substring(3).Trim()
        if ([string]::IsNullOrWhiteSpace($payload)) {
            continue
        }

        if ($payload -like '* -> *') {
            $payload = ($payload -split ' -> ')[-1]
        }

        [void] $paths.Add((Normalize-RepoRelativePath -Path $payload))
    }

    return New-UniqueStringArray -Values $paths
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedBucketPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $BucketPolicyPath
$resolvedFamilyMaturityPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $FamilyMaturityPath
$resolvedDeployablesPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $DeployablesPath
$resolvedTaskStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskStatusPath

$bucketPolicy = Read-JsonFile -Path $resolvedBucketPolicyPath
$familyMaturity = Read-JsonFile -Path $resolvedFamilyMaturityPath
$deployablesPolicy = Read-JsonFile -Path $resolvedDeployablesPath
$taskStatus = Read-JsonFileOrNull -Path $resolvedTaskStatusPath

$statusJsonPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $bucketPolicy.statusJsonPath)
$statusMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $bucketPolicy.statusMarkdownPath)

$changedRepoPaths = Get-GitChangedRepoPaths -RepositoryRoot $resolvedRepoRoot
$repoWorktreeState = if ($changedRepoPaths.Count -gt 0) { 'dirty' } else { 'clean' }
$currentPostureObject = Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'currentPosture'
$currentAutomationPosture = ''
if ($currentPostureObject -is [string]) {
    $currentAutomationPosture = [string] $currentPostureObject
} elseif ($null -ne $currentPostureObject) {
    $currentAutomationPosture = [string] (Get-ObjectPropertyValueOrNull -InputObject $currentPostureObject -PropertyName 'lastKnownStatus')
}

if ([string]::IsNullOrWhiteSpace($currentAutomationPosture)) {
    $currentAutomationPosture = [string] (Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'lastKnownStatus')
}

if ([string]::IsNullOrWhiteSpace($currentAutomationPosture)) {
    $currentAutomationPosture = 'unknown'
}

$longFormTasking = Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'longFormTasking'
$activeTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $longFormTasking -PropertyName 'activeTaskMapId')
$activeTaskMapStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $longFormTasking -PropertyName 'activeTaskMapStatus')

$projectEntries = foreach ($project in @($familyMaturity.projects)) {
    $repoRelativeProjectPath = Normalize-RepoRelativePath -Path (Join-Path 'OAN Mortalis V1.0' ([string] $project.path))
    [pscustomobject]@{
        project = [string] $project.project
        family = [string] $project.family
        repoRelativePath = $repoRelativeProjectPath
        buildable = [bool] $project.buildable
        deployable = [bool] $project.deployable
        authoritative = [bool] $project.authoritative
        promotable = [bool] $project.promotable
    }
}

$deployableEntries = foreach ($deployable in @($deployablesPolicy.deployables)) {
    $repoRelativeProjectPath = Normalize-RepoRelativePath -Path (Join-Path 'OAN Mortalis V1.0' ([string] $deployable.projectPath))
    [pscustomobject]@{
        name = [string] $deployable.name
        family = [string] $deployable.family
        repoRelativePath = $repoRelativeProjectPath
        publishLane = [string] $deployable.publishLane
        includedInFirstPublish = [bool] $deployable.includedInFirstPublish
        promotable = [bool] $deployable.promotable
    }
}

$bucketSummaries = foreach ($bucket in @($bucketPolicy.buckets)) {
    $bucketFamilies = New-UniqueStringArray -Values @($bucket.families)
    $bucketProjectNames = New-UniqueStringArray -Values @($bucket.projectNames)
    $bucketPatterns = New-UniqueStringArray -Values @($bucket.pathPatterns)

    $matchedProjects = @(
        $projectEntries | Where-Object {
            (@($bucketFamilies).Count -gt 0 -and @($bucketFamilies) -contains $_.family) -or
            (@($bucketProjectNames).Count -gt 0 -and @($bucketProjectNames) -contains $_.project) -or
            (Test-MatchesAnyPattern -Path $_.repoRelativePath -Patterns $bucketPatterns)
        } | Sort-Object project -Unique
    )

    $matchedDeployables = @(
        $deployableEntries | Where-Object {
            (@($bucketFamilies).Count -gt 0 -and @($bucketFamilies) -contains $_.family) -or
            (@($bucketProjectNames).Count -gt 0 -and @($bucketProjectNames) -contains $_.name) -or
            (Test-MatchesAnyPattern -Path $_.repoRelativePath -Patterns $bucketPatterns)
        } | Sort-Object name -Unique
    )

    $matchedChangedPaths = @(
        $changedRepoPaths | Where-Object { Test-MatchesAnyPattern -Path $_ -Patterns $bucketPatterns } | Sort-Object -Unique
    )

    $bucketState = if ($matchedChangedPaths.Count -gt 0) {
        'active'
    } elseif ($matchedDeployables.Count -gt 0) {
        'deployable-aware'
    } elseif ($matchedProjects.Count -gt 0) {
        'ready'
    } else {
        'reference-only'
    }

    [pscustomobject]@{
        id = [string] $bucket.id
        label = [string] $bucket.label
        purpose = [string] $bucket.purpose
        state = $bucketState
        families = $bucketFamilies
        projectNames = $bucketProjectNames
        pathPatterns = $bucketPatterns
        projectCount = $matchedProjects.Count
        buildableProjectCount = @($matchedProjects | Where-Object { $_.buildable }).Count
        authoritativeProjectCount = @($matchedProjects | Where-Object { $_.authoritative }).Count
        promotableProjectCount = @($matchedProjects | Where-Object { $_.promotable }).Count
        deployableProjectCount = $matchedDeployables.Count
        deployableNames = @($matchedDeployables | ForEach-Object { $_.name })
        includedInFirstPublishCount = @($matchedDeployables | Where-Object { $_.includedInFirstPublish }).Count
        changedPathCount = $matchedChangedPaths.Count
        changedPaths = $matchedChangedPaths
        projectPaths = @($matchedProjects | ForEach-Object { $_.repoRelativePath })
    }
}

$activeBuckets = @($bucketSummaries | Where-Object { $_.changedPathCount -gt 0 })

$summary = [pscustomobject]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    policyPath = $resolvedBucketPolicyPath
    formalSurfaceMarkdownPath = [string] $bucketPolicy.formalSurfaceMarkdownPath
    repoWorktreeState = $repoWorktreeState
    currentAutomationPosture = $currentAutomationPosture
    activeLongFormTaskMapId = $activeTaskMapId
    activeLongFormTaskMapStatus = $activeTaskMapStatus
    changedRepoPathCount = $changedRepoPaths.Count
    changedRepoPaths = $changedRepoPaths
    bucketCount = $bucketSummaries.Count
    activeBucketCount = $activeBuckets.Count
    activeBucketIds = @($activeBuckets | ForEach-Object { $_.id })
    buckets = $bucketSummaries
}

Write-JsonFile -Path $statusJsonPath -Value $summary

$markdownLines = New-Object System.Collections.Generic.List[string]
[void] $markdownLines.Add('# Workspace Bucket Status')
[void] $markdownLines.Add('')
[void] $markdownLines.Add(('- Generated at (UTC): `{0}`' -f $summary.generatedAtUtc))
[void] $markdownLines.Add(('- Formal surface: `{0}`' -f $summary.formalSurfaceMarkdownPath))
[void] $markdownLines.Add(('- Repo worktree state: `{0}`' -f $repoWorktreeState))
[void] $markdownLines.Add(('- Current automation posture: `{0}`' -f $currentAutomationPosture))
if (-not [string]::IsNullOrWhiteSpace($activeTaskMapId)) {
    [void] $markdownLines.Add(('- Active long-form task map: `{0}`' -f $activeTaskMapId))
}
if (-not [string]::IsNullOrWhiteSpace($activeTaskMapStatus)) {
    [void] $markdownLines.Add(('- Active long-form task map status: `{0}`' -f $activeTaskMapStatus))
}
[void] $markdownLines.Add(('- Active buckets: `{0}`' -f $summary.activeBucketCount))
[void] $markdownLines.Add('')

foreach ($bucket in $bucketSummaries) {
    [void] $markdownLines.Add(('## {0}' -f $bucket.label))
    [void] $markdownLines.Add('')
    [void] $markdownLines.Add(('- State: `{0}`' -f $bucket.state))
    [void] $markdownLines.Add(('- Purpose: {0}' -f $bucket.purpose))
    if (@($bucket.families).Count -gt 0) {
        [void] $markdownLines.Add(('- Families: `{0}`' -f (@($bucket.families) -join '`, `')))
    } else {
        [void] $markdownLines.Add('- Families: `none explicitly bound`')
    }
    [void] $markdownLines.Add(('- Projects: `{0}` total, `{1}` authoritative, `{2}` promotable' -f $bucket.projectCount, $bucket.authoritativeProjectCount, $bucket.promotableProjectCount))
    if (@($bucket.deployableNames).Count -gt 0) {
        [void] $markdownLines.Add(('- Deployables: `{0}`' -f (@($bucket.deployableNames) -join '`, `')))
    } else {
        [void] $markdownLines.Add('- Deployables: `none`')
    }
    [void] $markdownLines.Add(('- Changed paths: `{0}`' -f $bucket.changedPathCount))
    if (@($bucket.changedPaths).Count -gt 0) {
        foreach ($changedPath in @($bucket.changedPaths)) {
            [void] $markdownLines.Add(('  - `{0}`' -f $changedPath))
        }
    }
    [void] $markdownLines.Add('')
}

$markdownDirectory = Split-Path -Parent $statusMarkdownPath
if (-not [string]::IsNullOrWhiteSpace($markdownDirectory)) {
    New-Item -ItemType Directory -Force -Path $markdownDirectory | Out-Null
}

$markdownLines | Set-Content -LiteralPath $statusMarkdownPath -Encoding utf8

Write-Host ('[workspace-bucket-status] State: {0}' -f $statusJsonPath)
Write-Host ('[workspace-bucket-status] Markdown: {0}' -f $statusMarkdownPath)

$statusJsonPath
