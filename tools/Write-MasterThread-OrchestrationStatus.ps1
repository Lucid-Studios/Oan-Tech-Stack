param(
    [string] $RepoRoot,
    [string] $OrchestrationPolicyPath = 'OAN Mortalis V1.0/build/master-thread-orchestration.json',
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

    return @($paths)
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

function Get-InstructionEffectiveState {
    param(
        [object] $InstructionPayload,
        [datetime] $NowUtc,
        [string] $CurrentBranch,
        [string] $CurrentHeadCommit,
        [string] $CurrentWorktreeState,
        [string] $RequiredPublishedBranch,
        [string] $RequiredWorktreeState,
        [string] $MovementAdmissibilityState
    )

    $storedState = [string] (Get-ObjectPropertyValueOrNull -InputObject $InstructionPayload -PropertyName 'lifecycleState')
    if ($storedState -in @('completed', 'failed', 'blocked', 'superseded', 'movement', 'settling')) {
        return $storedState
    }

    $sourceMasterThread = Get-ObjectPropertyValueOrNull -InputObject $InstructionPayload -PropertyName 'sourceMasterThread'
    $sourceCommit = [string] (Get-ObjectPropertyValueOrNull -InputObject $sourceMasterThread -PropertyName 'sourceCommit')
    $waitPolicy = Get-ObjectPropertyValueOrNull -InputObject $InstructionPayload -PropertyName 'waitPolicy'
    $earliestEligibleRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $waitPolicy -PropertyName 'earliestEligibleRunUtc')

    $commitObserved = (
        -not [string]::IsNullOrWhiteSpace($sourceCommit) -and
        $CurrentBranch -eq $RequiredPublishedBranch -and
        $CurrentWorktreeState -eq $RequiredWorktreeState -and
        $CurrentHeadCommit -eq $sourceCommit
    )

    if (-not $commitObserved) {
        return 'polling'
    }

    if ($null -eq $earliestEligibleRunUtc -or $NowUtc -lt $earliestEligibleRunUtc) {
        return 'weighted-wait'
    }

    if ($MovementAdmissibilityState -ne 'admissible-after-delay') {
        return 'blocked'
    }

    return 'released'
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

$statusJsonPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statusJsonPath)
$statusMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.statusMarkdownPath)
$instructionOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.instructionOutputRoot)
$instructionIndexStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $policy.instructionIndexStatePath)

$nowUtc = (Get-Date).ToUniversalTime()
$currentBranch = Get-GitStringValue -RepositoryRoot $resolvedRepoRoot -ArgumentList @('rev-parse', '--abbrev-ref', 'HEAD')
$currentHeadCommit = Get-GitStringValue -RepositoryRoot $resolvedRepoRoot -ArgumentList @('rev-parse', 'HEAD')
$currentWorktreeState = if (@(Get-GitChangedRepoPaths -RepositoryRoot $resolvedRepoRoot).Count -gt 0) { 'dirty' } else { 'clean' }
$currentAutomationPosture = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastKnownStatus')
$movementAdmissibilityState = if ($currentAutomationPosture -eq [string] $policy.blockedStatus) {
    'held-by-blocked-posture'
} elseif (@($policy.allowedContinuationStatuses) -contains $currentAutomationPosture) {
    'admissible-after-delay'
} else {
    'held-by-review-posture'
}
$publishReady = ($currentBranch -eq [string] $policy.requiredPublishedBranch -and $currentWorktreeState -eq [string] $policy.requiredWorktreeState)

$instructionBundles = @()
if (Test-Path -LiteralPath $instructionOutputRoot -PathType Container) {
    $instructionBundles = @(
        Get-ChildItem -LiteralPath $instructionOutputRoot -Directory |
            Sort-Object -Property Name -Descending |
            Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'instruction.json') -PathType Leaf }
    )
}

$instructionSummaries = @(
foreach ($bundle in $instructionBundles) {
    $instructionPayload = Read-JsonFile -Path (Join-Path $bundle.FullName 'instruction.json')
    $effectiveState = Get-InstructionEffectiveState `
        -InstructionPayload $instructionPayload `
        -NowUtc $nowUtc `
        -CurrentBranch $currentBranch `
        -CurrentHeadCommit $currentHeadCommit `
        -CurrentWorktreeState $currentWorktreeState `
        -RequiredPublishedBranch ([string] $policy.requiredPublishedBranch) `
        -RequiredWorktreeState ([string] $policy.requiredWorktreeState) `
        -MovementAdmissibilityState $movementAdmissibilityState

    $waitPolicy = Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'waitPolicy'
    [pscustomobject]@{
        instructionId = [string] $instructionPayload.instructionId
        bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundle.FullName
        sourceThreadLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'sourceThreadContext') -PropertyName 'label')
        sourceCommit = [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'sourceMasterThread') -PropertyName 'sourceCommit')
        storedLifecycleState = [string] (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'lifecycleState')
        effectiveLifecycleState = $effectiveState
        reasonCode = [string] (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'reasonCode')
        nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'nextAction')
        targetBucketIds = @($instructionPayload.targetBucketIds)
        earliestEligibleRunUtc = if ($null -ne (Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $waitPolicy -PropertyName 'earliestEligibleRunUtc'))) { (Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $waitPolicy -PropertyName 'earliestEligibleRunUtc')).ToString('o') } else { $null }
        movementAdmissibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $instructionPayload -PropertyName 'movementGate') -PropertyName 'movementAdmissibilityState')
    }
})

$instructionCountsByEffectiveState = [ordered]@{}
foreach ($stateName in @($policy.instructionLifecycleStates)) {
    $instructionCountsByEffectiveState[$stateName] = @($instructionSummaries | Where-Object { $_.effectiveLifecycleState -eq $stateName }).Count
}

$latestInstruction = @($instructionSummaries) | Select-Object -First 1
$queuedInstructionCount = @($instructionSummaries | Where-Object { $_.effectiveLifecycleState -in @('prepared', 'weighted-wait', 'polling', 'commit-observed', 'released') }).Count
$releasableInstructionCount = @($instructionSummaries | Where-Object { $_.effectiveLifecycleState -eq 'released' }).Count
$attentionInstructionCount = @($instructionSummaries | Where-Object { $_.effectiveLifecycleState -in @('blocked', 'failed') }).Count
$eligibleTargetBuckets = @($bucketStatus.buckets | Where-Object { [string] $_.state -in @('active', 'ready', 'deployable-aware') })

$orchestrationState = 'dormant'
$reasonCode = 'master-thread-orchestration-dormant'
$nextAction = 'continue-bounded-observation'

if (-not $publishReady) {
    $orchestrationState = 'awaiting-publishable-master-thread'
    $reasonCode = 'master-thread-orchestration-awaiting-publishable-master-thread'
    $nextAction = 'publish-clean-commit-to-git-workflow-before-releasing-intent'
} elseif ($attentionInstructionCount -gt 0) {
    $orchestrationState = 'attention-required'
    $reasonCode = 'master-thread-orchestration-attention-required'
    $nextAction = 'resolve-blocked-or-failed-instruction'
} elseif ($releasableInstructionCount -gt 0) {
    $orchestrationState = 'released-intent-awaiting-materialization'
    $reasonCode = 'master-thread-orchestration-released-intent'
    $nextAction = 'materialize-codex-automation-from-latest-envelope'
} elseif ($queuedInstructionCount -gt 0) {
    $orchestrationState = 'weighted-or-polling'
    $reasonCode = 'master-thread-orchestration-weighted-or-polling'
    $nextAction = 'wait-for-release-window-or-commit-observation'
} elseif ($publishReady) {
    $orchestrationState = 'ready-for-instruction-publication'
    $reasonCode = 'master-thread-orchestration-ready-for-publication'
    $nextAction = 'author-next-bucket-scoped-instruction'
}

$statusPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    policyPath = $resolvedOrchestrationPolicyPath
    formalSurfaceMarkdownPath = [string] $policy.formalSurfaceMarkdownPath
    codexAutomationIntentContractMarkdownPath = [string] $policy.codexAutomationIntentContractMarkdownPath
    orchestrationState = $orchestrationState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sourceMasterThread = [ordered]@{
        currentBranch = $currentBranch
        currentHeadCommit = $currentHeadCommit
        repoWorktreeState = $currentWorktreeState
        currentAutomationPosture = $currentAutomationPosture
        publishReady = $publishReady
        activeLongFormTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'longFormTasking') -PropertyName 'activeTaskMapId')
    }
    codexAutomationSupport = [ordered]@{
        supportState = [string] $policy.codexAutomationSupport.supportState
        nativeRunOnceSupported = [bool] $policy.codexAutomationSupport.nativeRunOnceSupported
        supportReason = [string] $policy.codexAutomationSupport.reason
    }
    movementAdmissibilityState = $movementAdmissibilityState
    eligibleTargetBucketIds = @($eligibleTargetBuckets | ForEach-Object { [string] $_.id })
    eligibleTargetBucketLabels = @($eligibleTargetBuckets | ForEach-Object { [string] $_.label })
    instructionCount = @($instructionSummaries).Count
    queuedInstructionCount = $queuedInstructionCount
    releasableInstructionCount = $releasableInstructionCount
    attentionInstructionCount = $attentionInstructionCount
    instructionCountsByEffectiveState = $instructionCountsByEffectiveState
    latestInstruction = $latestInstruction
    instructionIndexStatePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $instructionIndexStatePath
    instructions = $instructionSummaries
}

Write-JsonFile -Path $statusJsonPath -Value $statusPayload

$markdownLines = @(
    '# Master Thread Orchestration Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $statusPayload.generatedAtUtc),
    ('- Formal surface: `{0}`' -f $statusPayload.formalSurfaceMarkdownPath),
    ('- Orchestration state: `{0}`' -f $statusPayload.orchestrationState),
    ('- Reason code: `{0}`' -f $statusPayload.reasonCode),
    ('- Next action: `{0}`' -f $statusPayload.nextAction),
    ('- Current branch: `{0}`' -f $statusPayload.sourceMasterThread.currentBranch),
    ('- Current head commit: `{0}`' -f $statusPayload.sourceMasterThread.currentHeadCommit),
    ('- Repo worktree state: `{0}`' -f $statusPayload.sourceMasterThread.repoWorktreeState),
    ('- Current automation posture: `{0}`' -f $statusPayload.sourceMasterThread.currentAutomationPosture),
    ('- Publish ready: `{0}`' -f $statusPayload.sourceMasterThread.publishReady),
    ('- Codex run-once support: `{0}`' -f $statusPayload.codexAutomationSupport.supportState),
    ('- Movement admissibility: `{0}`' -f $statusPayload.movementAdmissibilityState),
    ('- Eligible target buckets: `{0}`' -f $(if (@($statusPayload.eligibleTargetBucketLabels).Count -gt 0) { ((@($statusPayload.eligibleTargetBucketLabels) -join '`, `')) } else { 'none' })),
    ('- Total instructions: `{0}`' -f $statusPayload.instructionCount),
    ('- Queued instructions: `{0}`' -f $statusPayload.queuedInstructionCount),
    ('- Releasable instructions: `{0}`' -f $statusPayload.releasableInstructionCount),
    ('- Attention-required instructions: `{0}`' -f $statusPayload.attentionInstructionCount),
    ''
)

if ($null -ne $latestInstruction) {
    $markdownLines += @(
        '## Latest Instruction',
        '',
        ('- Instruction id: `{0}`' -f $latestInstruction.instructionId),
        ('- Source thread: `{0}`' -f $latestInstruction.sourceThreadLabel),
        ('- Source commit: `{0}`' -f $latestInstruction.sourceCommit),
        ('- Stored lifecycle: `{0}`' -f $latestInstruction.storedLifecycleState),
        ('- Effective lifecycle: `{0}`' -f $latestInstruction.effectiveLifecycleState),
        ('- Target buckets: `{0}`' -f ((@($latestInstruction.targetBucketIds) -join '`, `'))),
        ('- Earliest eligible run (UTC): `{0}`' -f $(if ($latestInstruction.earliestEligibleRunUtc) { $latestInstruction.earliestEligibleRunUtc } else { 'uninitialized' })),
        ('- Next action: `{0}`' -f $latestInstruction.nextAction),
        ''
    )
}

if (@($instructionSummaries).Count -gt 0) {
    $markdownLines += @(
        '## Instruction Queue',
        '',
        '| Instruction | Effective State | Source Thread | Target Buckets |',
        '| --- | --- | --- | --- |'
    )

    foreach ($instruction in @($instructionSummaries | Select-Object -First 12)) {
        $markdownLines += ('| {0} | {1} | {2} | {3} |' -f [string] $instruction.instructionId, [string] $instruction.effectiveLifecycleState, [string] $instruction.sourceThreadLabel, ((@($instruction.targetBucketIds) -join ', ')))
    }

    $markdownLines += ''
}

Set-Content -LiteralPath $statusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[master-thread-orchestration] State: {0}' -f $statusJsonPath)
$statusJsonPath
