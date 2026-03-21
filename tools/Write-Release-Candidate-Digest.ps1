param(
    [string] $RepoRoot,
    [string] $RunRoot = '.audit/runs/release-candidates',
    [string] $OutputRoot = '.audit/runs/release-digests',
    [int] $WindowHours = 24,
    [datetime] $ReferenceTimeUtc = ([datetime]::UtcNow),
    [datetime] $NextMandatoryReviewUtc
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

function Get-RunSnapshot {
    param(
        [string] $RunDirectoryPath,
        [string] $BasePath
    )

    $manifestPath = Join-Path $RunDirectoryPath 'build-evidence-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        return $null
    }

    $summaryPath = Join-Path $RunDirectoryPath 'summary.md'
    $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
    $generatedAtUtc = [datetime]::Parse([string] $manifest.generatedAtUtc, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind)

    return [pscustomobject]@{
        RunId = [System.IO.Path]::GetFileName($RunDirectoryPath)
        RunPath = Get-RelativePathString -BasePath $BasePath -TargetPath $RunDirectoryPath
        ManifestPath = Get-RelativePathString -BasePath $BasePath -TargetPath $manifestPath
        SummaryPath = if (Test-Path -LiteralPath $summaryPath -PathType Leaf) {
            Get-RelativePathString -BasePath $BasePath -TargetPath $summaryPath
        } else {
            $null
        }
        GeneratedAtUtc = $generatedAtUtc
        Manifest = $manifest
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedRunRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $RunRoot
$resolvedOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $OutputRoot
$windowStartUtc = $ReferenceTimeUtc.ToUniversalTime().AddHours(-1 * $WindowHours)

$runSnapshots = @()
if (Test-Path -LiteralPath $resolvedRunRoot -PathType Container) {
    $runSnapshots = @(
        Get-ChildItem -LiteralPath $resolvedRunRoot -Directory |
        ForEach-Object { Get-RunSnapshot -RunDirectoryPath $_.FullName -BasePath $resolvedRepoRoot } |
        Where-Object { $null -ne $_ } |
        Sort-Object -Property GeneratedAtUtc -Descending
    )
}

$windowRuns = @(
    $runSnapshots |
    Where-Object {
        $_.GeneratedAtUtc -ge $windowStartUtc -and $_.GeneratedAtUtc -le $ReferenceTimeUtc.ToUniversalTime()
    }
)

$latestRun = $null
if ($runSnapshots.Count -gt 0) {
    $latestRun = $runSnapshots[0]
}

$statusCounts = [ordered]@{
    candidateReady = @($windowRuns | Where-Object { [string] $_.Manifest.status -eq 'candidate-ready' }).Count
    hitlRequired = @($windowRuns | Where-Object { [string] $_.Manifest.status -eq 'hitl-required' }).Count
    blocked = @($windowRuns | Where-Object { [string] $_.Manifest.status -eq 'blocked' }).Count
}

$requiresImmediateHitl = $false
if (@($windowRuns | Where-Object { [string] $_.Manifest.status -in @('hitl-required', 'blocked') }).Count -gt 0) {
    $requiresImmediateHitl = $true
}

$recommendedAction = 'no-release-candidate-runs'
if ($statusCounts.blocked -gt 0) {
    $recommendedAction = 'review-required-blocked'
} elseif ($requiresImmediateHitl) {
    $recommendedAction = 'review-required-before-promotion'
} elseif ($null -ne $latestRun) {
    $recommendedAction = 'continue-automation-until-daily-review'
}

$hitlRuns = @(
    $windowRuns |
    Where-Object { [string] $_.Manifest.status -in @('hitl-required', 'blocked') } |
    ForEach-Object {
        [ordered]@{
            runId = [string] $_.RunId
            status = [string] $_.Manifest.status
            generatedAtUtc = $_.GeneratedAtUtc.ToString('o')
            gatesTriggered = @($_.Manifest.gatesTriggered | ForEach-Object { [string] $_ })
            summaryPath = [string] $_.SummaryPath
        }
    }
)

$digestTimestamp = $ReferenceTimeUtc.ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$digestSuffix = if ($null -ne $latestRun) {
    $latestRun.RunId
} else {
    'no-runs'
}
$digestBundlePath = Join-Path $resolvedOutputRoot ('{0}-{1}' -f $digestTimestamp, $digestSuffix)

$latestTouchedProjects = @()
$latestTouchedFamilies = @()
$latestPublishedArtifacts = @()
$latestVersionDecision = $null
$latestRepo = $null

if ($null -ne $latestRun) {
    $latestTouchedProjects = @($latestRun.Manifest.touchedProjects | ForEach-Object { [string] $_ })
    $latestTouchedFamilies = @($latestRun.Manifest.touchedFamilies | ForEach-Object { [string] $_ })
    $latestPublishedArtifacts = @(
        $latestRun.Manifest.publishedArtifacts |
        ForEach-Object {
            [ordered]@{
                name = [string] $_.name
                path = [string] $_.path
            }
        }
    )

    $latestVersionDecision = [ordered]@{
        proposedVersion = [string] $latestRun.Manifest.versionDecision.proposedVersion
        proposedAssemblyVersion = [string] $latestRun.Manifest.versionDecision.proposedAssemblyVersion
        classification = [string] $latestRun.Manifest.versionDecision.classification
        requiresHitl = [bool] $latestRun.Manifest.versionDecision.requiresHitl
        reasonCodes = @($latestRun.Manifest.versionDecision.reasonCodes | ForEach-Object { [string] $_ })
    }

    $latestRepo = [ordered]@{
        branch = [string] $latestRun.Manifest.repo.branch
        commitSha = [string] $latestRun.Manifest.repo.commitSha
        worktreeState = [string] $latestRun.Manifest.repo.worktreeState
    }
}

$digest = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $ReferenceTimeUtc.ToUniversalTime().ToString('o')
    windowHours = $WindowHours
    windowStartUtc = $windowStartUtc.ToString('o')
    windowEndUtc = $ReferenceTimeUtc.ToUniversalTime().ToString('o')
    nextMandatoryReviewUtc = if ($PSBoundParameters.ContainsKey('NextMandatoryReviewUtc')) { $NextMandatoryReviewUtc.ToUniversalTime().ToString('o') } else { $null }
    recommendedAction = $recommendedAction
    requiresImmediateHitl = $requiresImmediateHitl
    runCountInWindow = $windowRuns.Count
    statusCounts = $statusCounts
    latestRun = if ($null -ne $latestRun) {
        [ordered]@{
            runId = [string] $latestRun.RunId
            runPath = [string] $latestRun.RunPath
            manifestPath = [string] $latestRun.ManifestPath
            summaryPath = [string] $latestRun.SummaryPath
            generatedAtUtc = $latestRun.GeneratedAtUtc.ToString('o')
            status = [string] $latestRun.Manifest.status
            repo = $latestRepo
            versionDecision = $latestVersionDecision
            touchedProjects = $latestTouchedProjects
            touchedFamilies = $latestTouchedFamilies
            publishedArtifacts = $latestPublishedArtifacts
        }
    } else {
        $null
    }
    runsRequiringHitl = $hitlRuns
}

$jsonPath = Join-Path $digestBundlePath 'release-candidate-digest.json'
$markdownPath = Join-Path $digestBundlePath 'release-candidate-digest.md'
Write-JsonFile -Path $jsonPath -Value $digest

$markdownLines = @(
    '# Release Candidate Daily Digest',
    '',
    ('- Generated at (UTC): `{0}`' -f $digest.generatedAtUtc),
    ('- Review window: last `{0}` hours' -f $WindowHours),
    ('- Recommended action: `{0}`' -f $recommendedAction),
    ('- Requires immediate HITL: `{0}`' -f $requiresImmediateHitl)
)

if ($PSBoundParameters.ContainsKey('NextMandatoryReviewUtc')) {
    $markdownLines += ('- Next mandatory review (UTC): `{0}`' -f $NextMandatoryReviewUtc.ToUniversalTime().ToString('o'))
}

$markdownLines += @(
    '',
    '## Window Status',
    '',
    ('- Candidate-ready runs: `{0}`' -f $statusCounts.candidateReady),
    ('- HITL-required runs: `{0}`' -f $statusCounts.hitlRequired),
    ('- Blocked runs: `{0}`' -f $statusCounts.blocked)
)

if ($null -ne $latestRun) {
    $markdownLines += @(
        '',
        '## Latest Run',
        '',
        ('- Run ID: `{0}`' -f $latestRun.RunId),
        ('- Status: `{0}`' -f $latestRun.Manifest.status),
        ('- Generated at (UTC): `{0}`' -f $latestRun.GeneratedAtUtc.ToString('o')),
        ('- Branch: `{0}`' -f $latestRun.Manifest.repo.branch),
        ('- Commit: `{0}`' -f $latestRun.Manifest.repo.commitSha),
        ('- Worktree state at run time: `{0}`' -f $latestRun.Manifest.repo.worktreeState),
        ('- Proposed version: `{0}`' -f $latestRun.Manifest.versionDecision.proposedVersion),
        ('- Classification: `{0}`' -f $latestRun.Manifest.versionDecision.classification),
        ('- Run bundle: `{0}`' -f $latestRun.RunPath),
        ('- Summary: `{0}`' -f $latestRun.SummaryPath)
    )

    if ($latestTouchedFamilies.Count -gt 0) {
        $markdownLines += ('- Touched families: `{0}`' -f ($latestTouchedFamilies -join '`, `'))
    }

    if ($latestTouchedProjects.Count -gt 0) {
        $markdownLines += ('- Touched projects: `{0}`' -f ($latestTouchedProjects -join '`, `'))
    }

    if ($latestPublishedArtifacts.Count -gt 0) {
        $markdownLines += @(
            '',
            '## Published Artifacts',
            '',
            '| Name | Path |',
            '| --- | --- |'
        )

        foreach ($artifact in $latestPublishedArtifacts) {
            $markdownLines += ('| {0} | {1} |' -f $artifact.name, $artifact.path)
        }
    }
}

if ($hitlRuns.Count -gt 0) {
    $markdownLines += @(
        '',
        '## Runs Requiring HITL',
        '',
        '| Run ID | Status | Generated At (UTC) | Gates |',
        '| --- | --- | --- | --- |'
    )

    foreach ($run in $hitlRuns) {
        $gates = if ($run.gatesTriggered.Count -gt 0) { $run.gatesTriggered -join ', ' } else { 'none-recorded' }
        $markdownLines += ('| {0} | {1} | {2} | {3} |' -f $run.runId, $run.status, $run.generatedAtUtc, $gates)
    }
}

Set-Content -LiteralPath $markdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[release-candidate-digest] Bundle: {0}' -f $digestBundlePath)
$digestBundlePath
