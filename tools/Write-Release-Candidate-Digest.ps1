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

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath

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

function Get-StringArrayOrEmpty {
    param([object] $Value)

    if ($null -eq $Value) {
        return @()
    }

    return @($Value | ForEach-Object { [string] $_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-PublishedArtifactNames {
    param([object] $Manifest)

    if ($null -eq $Manifest) {
        return @()
    }

    return @(
        $Manifest.publishedArtifacts |
        ForEach-Object { [string] $_.name } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Compare-StringSets {
    param(
        [string[]] $Current,
        [string[]] $Previous
    )

    $currentSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $previousSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($item in @($Current)) {
        if (-not [string]::IsNullOrWhiteSpace($item)) {
            [void] $currentSet.Add($item)
        }
    }

    foreach ($item in @($Previous)) {
        if (-not [string]::IsNullOrWhiteSpace($item)) {
            [void] $previousSet.Add($item)
        }
    }

    $added = @($currentSet | Where-Object { -not $previousSet.Contains($_) } | Sort-Object)
    $removed = @($previousSet | Where-Object { -not $currentSet.Contains($_) } | Sort-Object)

    [ordered]@{
        added = $added
        removed = $removed
        changed = (($added.Count + $removed.Count) -gt 0)
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

$previousRun = $null
if ($runSnapshots.Count -gt 1) {
    $previousRun = $runSnapshots[1]
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
$deltaSummary = $null

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

    $previousTouchedProjects = @()
    $previousTouchedFamilies = @()
    $previousPublishedArtifactNames = @()
    $previousRepo = $null
    $previousVersionDecision = $null

    if ($null -ne $previousRun) {
        $previousTouchedProjects = Get-StringArrayOrEmpty -Value $previousRun.Manifest.touchedProjects
        $previousTouchedFamilies = Get-StringArrayOrEmpty -Value $previousRun.Manifest.touchedFamilies
        $previousPublishedArtifactNames = Get-PublishedArtifactNames -Manifest $previousRun.Manifest
        $previousRepo = [ordered]@{
            branch = [string] $previousRun.Manifest.repo.branch
            commitSha = [string] $previousRun.Manifest.repo.commitSha
            worktreeState = [string] $previousRun.Manifest.repo.worktreeState
        }
        $previousVersionDecision = [ordered]@{
            proposedVersion = [string] $previousRun.Manifest.versionDecision.proposedVersion
            classification = [string] $previousRun.Manifest.versionDecision.classification
        }
    }

    $deltaSummary = [ordered]@{
        comparisonAvailable = ($null -ne $previousRun)
        latestRunId = [string] $latestRun.RunId
        previousRunId = if ($null -ne $previousRun) { [string] $previousRun.RunId } else { $null }
        latestStatus = [string] $latestRun.Manifest.status
        previousStatus = if ($null -ne $previousRun) { [string] $previousRun.Manifest.status } else { $null }
        statusChanged = if ($null -ne $previousRun) { ([string] $latestRun.Manifest.status -ne [string] $previousRun.Manifest.status) } else { $false }
        latestCommitSha = [string] $latestRun.Manifest.repo.commitSha
        previousCommitSha = if ($null -ne $previousRun) { [string] $previousRun.Manifest.repo.commitSha } else { $null }
        commitChanged = if ($null -ne $previousRun) { ([string] $latestRun.Manifest.repo.commitSha -ne [string] $previousRun.Manifest.repo.commitSha) } else { $false }
        latestVersion = [string] $latestRun.Manifest.versionDecision.proposedVersion
        previousVersion = if ($null -ne $previousRun) { [string] $previousRun.Manifest.versionDecision.proposedVersion } else { $null }
        versionChanged = if ($null -ne $previousRun) { ([string] $latestRun.Manifest.versionDecision.proposedVersion -ne [string] $previousRun.Manifest.versionDecision.proposedVersion) } else { $false }
        latestClassification = [string] $latestRun.Manifest.versionDecision.classification
        previousClassification = if ($null -ne $previousRun) { [string] $previousRun.Manifest.versionDecision.classification } else { $null }
        classificationChanged = if ($null -ne $previousRun) { ([string] $latestRun.Manifest.versionDecision.classification -ne [string] $previousRun.Manifest.versionDecision.classification) } else { $false }
        touchedProjects = Compare-StringSets -Current $latestTouchedProjects -Previous $previousTouchedProjects
        touchedFamilies = Compare-StringSets -Current $latestTouchedFamilies -Previous $previousTouchedFamilies
        publishedArtifacts = Compare-StringSets -Current (Get-PublishedArtifactNames -Manifest $latestRun.Manifest) -Previous $previousPublishedArtifactNames
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
    deltaSummary = $deltaSummary
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
Add-AutomationCascadeOperatorPromptProperty -InputObject $digest | Out-Null

$jsonPath = Join-Path $digestBundlePath 'release-candidate-digest.json'
$markdownPath = Join-Path $digestBundlePath 'release-candidate-digest.md'
$deltaJsonPath = Join-Path $digestBundlePath 'delta-summary.json'
$deltaMarkdownPath = Join-Path $digestBundlePath 'delta-summary.md'
Write-JsonFile -Path $jsonPath -Value $digest
if ($null -ne $deltaSummary) {
    Add-AutomationCascadeOperatorPromptProperty -InputObject $deltaSummary | Out-Null
    Write-JsonFile -Path $deltaJsonPath -Value $deltaSummary
}

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

if ($null -ne $deltaSummary) {
    $markdownLines += @(
        '',
        '## Delta Summary',
        '',
        ('- Comparison available: `{0}`' -f $deltaSummary.comparisonAvailable),
        ('- Latest run: `{0}`' -f $deltaSummary.latestRunId),
        ('- Previous run: `{0}`' -f $(if ($deltaSummary.previousRunId) { $deltaSummary.previousRunId } else { 'none' })),
        ('- Status changed: `{0}`' -f $deltaSummary.statusChanged),
        ('- Commit changed: `{0}`' -f $deltaSummary.commitChanged),
        ('- Version changed: `{0}`' -f $deltaSummary.versionChanged),
        ('- Classification changed: `{0}`' -f $deltaSummary.classificationChanged)
    )

    if ($deltaSummary.touchedProjects.changed -or $deltaSummary.touchedFamilies.changed -or $deltaSummary.publishedArtifacts.changed) {
        $markdownLines += @(
            '',
            '| Surface | Added | Removed |',
            '| --- | --- | --- |',
            ('| Touched Projects | {0} | {1} |' -f $(if ($deltaSummary.touchedProjects.added.Count -gt 0) { $deltaSummary.touchedProjects.added -join ', ' } else { 'none' }), $(if ($deltaSummary.touchedProjects.removed.Count -gt 0) { $deltaSummary.touchedProjects.removed -join ', ' } else { 'none' })),
            ('| Touched Families | {0} | {1} |' -f $(if ($deltaSummary.touchedFamilies.added.Count -gt 0) { $deltaSummary.touchedFamilies.added -join ', ' } else { 'none' }), $(if ($deltaSummary.touchedFamilies.removed.Count -gt 0) { $deltaSummary.touchedFamilies.removed -join ', ' } else { 'none' })),
            ('| Published Artifacts | {0} | {1} |' -f $(if ($deltaSummary.publishedArtifacts.added.Count -gt 0) { $deltaSummary.publishedArtifacts.added -join ', ' } else { 'none' }), $(if ($deltaSummary.publishedArtifacts.removed.Count -gt 0) { $deltaSummary.publishedArtifacts.removed -join ', ' } else { 'none' }))
        )
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

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $markdownPath -Value $markdownLines -Encoding utf8

if ($null -ne $deltaSummary) {
    $deltaMarkdownLines = @(
        '# Release Candidate Delta Summary',
        '',
        ('- Latest run: `{0}`' -f $deltaSummary.latestRunId),
        ('- Previous run: `{0}`' -f $(if ($deltaSummary.previousRunId) { $deltaSummary.previousRunId } else { 'none' })),
        ('- Status changed: `{0}`' -f $deltaSummary.statusChanged),
        ('- Commit changed: `{0}`' -f $deltaSummary.commitChanged),
        ('- Version changed: `{0}`' -f $deltaSummary.versionChanged),
        ('- Classification changed: `{0}`' -f $deltaSummary.classificationChanged),
        '',
        '| Surface | Added | Removed |',
        '| --- | --- | --- |',
        ('| Touched Projects | {0} | {1} |' -f $(if ($deltaSummary.touchedProjects.added.Count -gt 0) { $deltaSummary.touchedProjects.added -join ', ' } else { 'none' }), $(if ($deltaSummary.touchedProjects.removed.Count -gt 0) { $deltaSummary.touchedProjects.removed -join ', ' } else { 'none' })),
        ('| Touched Families | {0} | {1} |' -f $(if ($deltaSummary.touchedFamilies.added.Count -gt 0) { $deltaSummary.touchedFamilies.added -join ', ' } else { 'none' }), $(if ($deltaSummary.touchedFamilies.removed.Count -gt 0) { $deltaSummary.touchedFamilies.removed -join ', ' } else { 'none' })),
        ('| Published Artifacts | {0} | {1} |' -f $(if ($deltaSummary.publishedArtifacts.added.Count -gt 0) { $deltaSummary.publishedArtifacts.added -join ', ' } else { 'none' }), $(if ($deltaSummary.publishedArtifacts.removed.Count -gt 0) { $deltaSummary.publishedArtifacts.removed -join ', ' } else { 'none' }))
    )

    $deltaMarkdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $deltaMarkdownLines
    Set-Content -LiteralPath $deltaMarkdownPath -Value $deltaMarkdownLines -Encoding utf8
}

Write-Host ('[release-candidate-digest] Bundle: {0}' -f $digestBundlePath)
$digestBundlePath
