param(
    [Parameter(Mandatory = $true)]
    [string] $ThreadContextLabel,
    [Parameter(Mandatory = $true)]
    [string] $Subject,
    [Parameter(Mandatory = $true)]
    [string] $Predicate,
    [Parameter(Mandatory = $true)]
    [string[]] $Actions,
    [string[]] $TargetBucketIds,
    [string] $ContextSummary,
    [int] $DelayMinutes,
    [int] $PollingIntervalSeconds,
    [int] $PollingWindowMinutes,
    [string] $RepoRoot,
    [string] $OrchestrationPolicyPath = 'OAN Mortalis V1.1.1/build/master-thread-orchestration.json',
    [string] $BucketStatusPath = '.audit/state/workspace-bucket-status.json',
    [string] $CycleStatePath = '.audit/state/local-automation-cycle.json',
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

    $Value | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $Path -Encoding utf8
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

function Get-GitStringValue {
    param(
        [string] $RepositoryRoot,
        [string[]] $ArgumentList
    )

    $output = & git -C $RepositoryRoot @ArgumentList 2>$null
    if ($LASTEXITCODE -ne 0) {
        return ''
    }

    return ([string] (($output | Select-Object -First 1))).Trim()
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

        [void] $paths.Add($payload.Replace('\', '/'))
    }

    return New-UniqueStringArray -Values $paths
}

function New-Slug {
    param([string] $Value)

    $collapsed = [regex]::Replace($Value.ToLowerInvariant(), '[^a-z0-9]+', '-')
    return $collapsed.Trim('-')
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedOrchestrationPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $OrchestrationPolicyPath
$resolvedBucketStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $BucketStatusPath
$resolvedCycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CycleStatePath
$resolvedTaskStatusPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskStatusPath

$policy = Read-JsonFile -Path $resolvedOrchestrationPolicyPath
$bucketStatus = Read-JsonFile -Path $resolvedBucketStatusPath
$cycleState = Read-JsonFileOrNull -Path $resolvedCycleStatePath
$taskStatus = Read-JsonFileOrNull -Path $resolvedTaskStatusPath

$instructionOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.instructionOutputRoot)
$instructionIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.instructionIndexStatePath)

$effectiveDelayMinutes = if ($PSBoundParameters.ContainsKey('DelayMinutes')) { $DelayMinutes } else { [int] $policy.minimumPublishDelayMinutes }
$effectivePollingIntervalSeconds = if ($PSBoundParameters.ContainsKey('PollingIntervalSeconds')) { $PollingIntervalSeconds } else { [int] $policy.defaultPollingIntervalSeconds }
$effectivePollingWindowMinutes = if ($PSBoundParameters.ContainsKey('PollingWindowMinutes')) { $PollingWindowMinutes } else { [int] $policy.defaultPollingWindowMinutes }

$currentBranch = Get-GitStringValue -RepositoryRoot $resolvedRepoRoot -ArgumentList @('rev-parse', '--abbrev-ref', 'HEAD')
$currentHeadCommit = Get-GitStringValue -RepositoryRoot $resolvedRepoRoot -ArgumentList @('rev-parse', 'HEAD')
$changedRepoPaths = Get-GitChangedRepoPaths -RepositoryRoot $resolvedRepoRoot
$repoWorktreeState = if (@($changedRepoPaths).Count -gt 0) { 'dirty' } else { 'clean' }

$targetBucketIdSet = if ($PSBoundParameters.ContainsKey('TargetBucketIds') -and @($TargetBucketIds).Count -gt 0) {
    New-UniqueStringArray -Values $TargetBucketIds
} else {
    @($bucketStatus.activeBucketIds)
}

if (@($targetBucketIdSet).Count -eq 0) {
    throw 'TargetBucketIds are required when no active buckets are currently present.'
}

$targetBuckets = foreach ($bucket in @($bucketStatus.buckets)) {
    if (@($targetBucketIdSet) -contains ([string] $bucket.id)) {
        [pscustomobject]@{
            id = [string] $bucket.id
            label = [string] $bucket.label
            state = [string] $bucket.state
            changedPathCount = [int] $bucket.changedPathCount
            projectCount = [int] $bucket.projectCount
            deployableNames = @($bucket.deployableNames)
        }
    }
}

if (@($targetBuckets).Count -ne @($targetBucketIdSet).Count) {
    $resolvedIds = @($targetBuckets | ForEach-Object { [string] $_.id })
    $missingIds = @($targetBucketIdSet | Where-Object { $resolvedIds -notcontains $_ })
    throw ('Unknown bucket ids: {0}' -f ($missingIds -join ', '))
}

$lastKnownStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastKnownStatus')
$activeTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'longFormTasking') -PropertyName 'activeTaskMapId')
$publishedBranch = [string] $policy.requiredPublishedBranch
$publishEligible = ($currentBranch -eq $publishedBranch -and $repoWorktreeState -eq [string] $policy.requiredWorktreeState)
$movementAdmissibilityState = if ($lastKnownStatus -eq [string] $policy.blockedStatus) {
    'held-by-blocked-posture'
} elseif (@($policy.allowedContinuationStatuses) -contains $lastKnownStatus) {
    'admissible-after-delay'
} else {
    'held-by-review-posture'
}

$lifecycleState = if ($publishEligible) { 'weighted-wait' } else { 'prepared' }
$reasonCode = if ($publishEligible) { 'instruction-weighted-after-published-master-thread' } else { 'instruction-awaiting-publishable-master-thread' }
$nextAction = if ($publishEligible) { 'wait-until-earliest-run-window-then-materialize-codex-automation' } else { 'publish-clean-commit-to-git-workflow-before-release' }

$nowUtc = (Get-Date).ToUniversalTime()
$earliestEligibleRunUtc = if ($publishEligible) {
    $nowUtc.AddMinutes($effectiveDelayMinutes)
} else {
    $null
}

$instructionId = '{0}-{1}' -f $nowUtc.ToString('yyyyMMddTHHmmssZ'), (New-Slug -Value $Subject)
$bundlePath = Join-Path $instructionOutputRoot $instructionId
$instructionJsonPath = Join-Path $bundlePath 'instruction.json'
$instructionMarkdownPath = Join-Path $bundlePath 'instruction.md'
$codexAutomationIntentJsonPath = Join-Path $bundlePath 'codex-automation-intent.json'
$codexAutomationIntentMarkdownPath = Join-Path $bundlePath 'codex-automation-intent.md'

$subjectPredicateActionSets = @(
    [ordered]@{
        subject = $Subject
        predicate = $Predicate
        actions = @($Actions)
    }
)

$codexPromptLines = @(
    ('Target buckets: {0}.' -f ((@($targetBuckets | ForEach-Object { [string] $_.label }) -join '; '))),
    ('Subject: {0}.' -f $Subject),
    ('Predicate: {0}.' -f $Predicate),
    ('Actions: {0}.' -f ((@($Actions) -join '; '))),
    'Ground all work in current repo truth from the master-thread state, workspace bucket status, and local automation posture.',
    'Do not widen scope beyond the named buckets or subject-predicate-action set.',
    'Emit bounded receipts that localize what changed, what remains waiting, and whether the movement surface should settle, continue, or block.'
)
$codexPrompt = ($codexPromptLines -join ' ')

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    instructionId = $instructionId
    lifecycleState = $lifecycleState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sourceThreadContext = [ordered]@{
        label = $ThreadContextLabel
        summary = $ContextSummary
        origin = 'interactive-turn'
    }
    sourceMasterThread = [ordered]@{
        sourceBranch = $currentBranch
        sourceCommit = $currentHeadCommit
        repoWorktreeState = $repoWorktreeState
        currentAutomationPosture = $lastKnownStatus
        activeLongFormTaskMapId = $activeTaskMapId
        workspaceBucketStatusPath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedBucketStatusPath
        cycleStatePath = if ($null -ne $cycleState) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedCycleStatePath } else { $null }
        taskStatusPath = if ($null -ne $taskStatus) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $resolvedTaskStatusPath } else { $null }
    }
    targetBucketIds = @($targetBuckets | ForEach-Object { [string] $_.id })
    targetBuckets = $targetBuckets
    subjectPredicateActionSets = $subjectPredicateActionSets
    waitPolicy = [ordered]@{
        requestedDelayMinutes = $effectiveDelayMinutes
        earliestEligibleRunUtc = if ($null -ne $earliestEligibleRunUtc) { $earliestEligibleRunUtc.ToString('o') } else { $null }
        pollingIntervalSeconds = $effectivePollingIntervalSeconds
        pollingWindowMinutes = $effectivePollingWindowMinutes
        commitDependencyState = if ($publishEligible) { 'published-source-commit' } else { 'awaiting-published-source-commit' }
    }
    movementGate = [ordered]@{
        movementAdmissibilityState = $movementAdmissibilityState
        requiredPublishedBranch = $publishedBranch
        requiredWorktreeState = [string] $policy.requiredWorktreeState
        currentBranch = $currentBranch
        currentWorktreeState = $repoWorktreeState
        publishEligible = $publishEligible
    }
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null

$codexAutomationIntentPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    instructionId = $instructionId
    supportState = [string] $policy.codexAutomationSupport.supportState
    nativeRunOnceSupported = [bool] $policy.codexAutomationSupport.nativeRunOnceSupported
    supportReason = [string] $policy.codexAutomationSupport.reason
    materializationState = if ($publishEligible) { 'intent-ready-after-delay' } else { 'awaiting-published-commit' }
    materializationNextAction = if ($publishEligible) { 'materialize-codex-automation-after-delay' } else { 'publish-clean-commit-before-materialization' }
    suggestedName = ('Bucket Orchestration {0}' -f $Subject).Trim()
    suggestedPrompt = $codexPrompt
    workingDirectories = @($resolvedRepoRoot)
    targetBucketIds = @($payload.targetBucketIds)
    subjectPredicateActionSets = $subjectPredicateActionSets
    earliestEligibleRunUtc = if ($null -ne $earliestEligibleRunUtc) { $earliestEligibleRunUtc.ToString('o') } else { $null }
    requestedDelayMinutes = $effectiveDelayMinutes
    pollingIntervalSeconds = $effectivePollingIntervalSeconds
    pollingWindowMinutes = $effectivePollingWindowMinutes
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $codexAutomationIntentPayload | Out-Null

Write-JsonFile -Path $instructionJsonPath -Value $payload
Write-JsonFile -Path $codexAutomationIntentJsonPath -Value $codexAutomationIntentPayload

$instructionMarkdownLines = @(
    '# Master Thread Orchestration Instruction',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Instruction id: `{0}`' -f $payload.instructionId),
    ('- Lifecycle state: `{0}`' -f $payload.lifecycleState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Source thread: `{0}`' -f $payload.sourceThreadContext.label),
    ('- Source branch: `{0}`' -f $payload.sourceMasterThread.sourceBranch),
    ('- Source commit: `{0}`' -f $payload.sourceMasterThread.sourceCommit),
    ('- Repo worktree state: `{0}`' -f $payload.sourceMasterThread.repoWorktreeState),
    ('- Current automation posture: `{0}`' -f $payload.sourceMasterThread.currentAutomationPosture),
    ('- Target buckets: `{0}`' -f ((@($targetBuckets | ForEach-Object { [string] $_.label }) -join '`, `'))),
    ('- Subject: `{0}`' -f $Subject),
    ('- Predicate: `{0}`' -f $Predicate),
    ('- Actions: `{0}`' -f ((@($Actions) -join '; '))),
    ('- Earliest eligible run (UTC): `{0}`' -f $(if ($null -ne $earliestEligibleRunUtc) { $earliestEligibleRunUtc.ToString('o') } else { 'awaiting-publish' })),
    ('- Movement admissibility: `{0}`' -f $movementAdmissibilityState),
    ''
)
$instructionMarkdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $instructionMarkdownLines
Set-Content -LiteralPath $instructionMarkdownPath -Value $instructionMarkdownLines -Encoding utf8

$automationMarkdownLines = @(
    '# Codex Automation Once Intent',
    '',
    ('- Generated at (UTC): `{0}`' -f $codexAutomationIntentPayload.generatedAtUtc),
    ('- Instruction id: `{0}`' -f $codexAutomationIntentPayload.instructionId),
    ('- Support state: `{0}`' -f $codexAutomationIntentPayload.supportState),
    ('- Native run-once supported: `{0}`' -f $codexAutomationIntentPayload.nativeRunOnceSupported),
    ('- Materialization state: `{0}`' -f $codexAutomationIntentPayload.materializationState),
    ('- Materialization next action: `{0}`' -f $codexAutomationIntentPayload.materializationNextAction),
    ('- Suggested name: `{0}`' -f $codexAutomationIntentPayload.suggestedName),
    ('- Earliest eligible run (UTC): `{0}`' -f $(if ($null -ne $earliestEligibleRunUtc) { $earliestEligibleRunUtc.ToString('o') } else { 'awaiting-publish' })),
    ('- Polling interval seconds: `{0}`' -f $effectivePollingIntervalSeconds),
    ('- Polling window minutes: `{0}`' -f $effectivePollingWindowMinutes),
    '',
    '## Suggested Prompt',
    '',
    $codexAutomationIntentPayload.suggestedPrompt
)
$automationMarkdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $automationMarkdownLines
Set-Content -LiteralPath $codexAutomationIntentMarkdownPath -Value $automationMarkdownLines -Encoding utf8

$existingIndex = Read-JsonFileOrNull -Path $instructionIndexStatePath
$createdInstructionIds = New-UniqueStringArray -Values @(
    $(if ($null -ne $existingIndex) { @($existingIndex.recentInstructionIds) } else { @() }),
    $instructionId
)

$indexPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    lastInstructionId = $instructionId
    lastInstructionBundle = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    recentInstructionIds = @($createdInstructionIds | Select-Object -Last 16)
    codexAutomationSupportState = [string] $policy.codexAutomationSupport.supportState
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $indexPayload | Out-Null

Write-JsonFile -Path $instructionIndexStatePath -Value $indexPayload
Write-Host ('[master-thread-orchestration] Instruction: {0}' -f $bundlePath)
$bundlePath
