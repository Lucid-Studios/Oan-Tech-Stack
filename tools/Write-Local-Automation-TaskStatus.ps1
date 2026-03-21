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

function Get-MeaningfulScheduledDateTimeUtcStringOrNull {
    param([datetime] $Value)

    $utcValue = $Value.ToUniversalTime()
    if ($utcValue -le [datetime]'2000-01-01T00:00:00Z') {
        return $null
    }

    return $utcValue.ToString('o')
}

function Resolve-LongFormTaskLiveStatus {
    param(
        [string] $TaskId,
        [string] $PolicyStatus,
        [string] $LatestDigestBundlePath,
        [object] $RetentionState,
        [object] $BlockedEscalationState,
        [object] $SeededGovernanceState,
        [object] $SchedulerReconciliationState,
        [object] $CmeConsolidationState,
        [object] $PromotionGateState,
        [object] $CiConcordanceState,
        [object] $ReleaseRatificationState,
        [object] $SeededPromotionReviewState,
        [object] $FirstPublishIntentState,
        [object] $ReleaseHandshakeState,
        [object] $PublishRequestEnvelopeState,
        [object] $PostPublishEvidenceState,
        [object] $SeedBraidEscalationState,
        [object] $PublishedRuntimeReceiptState,
        [object] $ArtifactAttestationState,
        [object] $PostPublishDriftWatchState,
        [object] $OperationalPublicationLedgerState,
        [object] $ExternalConsumerConcordanceState,
        [object] $PostPublishGovernanceLoopState,
        [object] $PublicationCadenceLedgerState,
        [object] $DownstreamRuntimeObservationState,
        [object] $MultiIntervalGovernanceBraidState,
        [object] $SchedulerExecutionReceiptState,
        [object] $UnattendedIntervalConcordanceState,
        [object] $StaleSurfaceContradictionWatchState,
        [string] $LastKnownStatus,
        [string] $BlockedStatus
    )

    switch ($TaskId) {
        'delta-summary-surface' {
            if (-not [string]::IsNullOrWhiteSpace($LatestDigestBundlePath) -and
                (Test-Path -LiteralPath (Join-Path $LatestDigestBundlePath 'delta-summary.json') -PathType Leaf)) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'artifact-retention-pruning' {
            if ($null -ne $RetentionState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'blocked-escalation-bundle' {
            if ($LastKnownStatus -eq $BlockedStatus) {
                if ($null -ne $BlockedEscalationState) {
                    return 'completed'
                }

                return 'awaiting-blocked-bundle'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'armed'
            }
        }
        'seeded-governance-lane' {
            if ($null -ne $SeededGovernanceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'scheduler-cadence-reconciliation' {
            if ($null -ne $SchedulerReconciliationState) {
                if ([bool] $SchedulerReconciliationState.aligned) {
                    return 'completed'
                }

                return 'active'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'cme-formalization-consolidation-surface' {
            if ($null -ne $CmeConsolidationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'promotion-gate-bundle' {
            if ($null -ne $PromotionGateState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'ci-artifact-concordance' {
            if ($null -ne $CiConcordanceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'release-ratification-rehearsal' {
            if ($null -ne $ReleaseRatificationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'seeded-promotion-review' {
            if ($null -ne $SeededPromotionReviewState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'first-publish-intent-closure' {
            if ($null -ne $FirstPublishIntentState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'release-handshake-surface' {
            if ($null -ne $ReleaseHandshakeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'publish-request-envelope' {
            if ($null -ne $PublishRequestEnvelopeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'post-publish-evidence-loop' {
            if ($null -ne $PostPublishEvidenceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'seed-braid-escalation-lane' {
            if ($null -ne $SeedBraidEscalationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'published-runtime-receipt' {
            if ($null -ne $PublishedRuntimeReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'artifact-attestation-surface' {
            if ($null -ne $ArtifactAttestationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'post-publish-drift-watch' {
            if ($null -ne $PostPublishDriftWatchState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'operational-publication-ledger' {
            if ($null -ne $OperationalPublicationLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'external-consumer-concordance' {
            if ($null -ne $ExternalConsumerConcordanceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'post-publish-governance-loop' {
            if ($null -ne $PostPublishGovernanceLoopState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'publication-cadence-ledger' {
            if ($null -ne $PublicationCadenceLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'downstream-runtime-observation' {
            if ($null -ne $DownstreamRuntimeObservationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'multi-interval-governance-braid' {
            if ($null -ne $MultiIntervalGovernanceBraidState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'scheduler-execution-receipt' {
            if ($null -ne $SchedulerExecutionReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'unattended-interval-concordance' {
            if ($null -ne $UnattendedIntervalConcordanceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'stale-surface-contradiction-watch' {
            if ($null -ne $StaleSurfaceContradictionWatchState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
    }

    return $PolicyStatus
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Read-JsonFile -Path $resolvedCyclePolicyPath
$taskingPolicy = Read-JsonFile -Path $resolvedTaskingPolicyPath
$taskDefinitions = @($taskingPolicy.tasks)
$longFormTaskMaps = @($taskingPolicy.longFormTaskMaps)
$activeTaskMapId = [string] $taskingPolicy.activeTaskMapId
$activeLongFormRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$activeLongFormRun = $null
if (Test-Path -LiteralPath $activeLongFormRunStatePath -PathType Leaf) {
    $activeLongFormRun = Read-JsonFile -Path $activeLongFormRunStatePath
}

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$statusJsonPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.statusJsonPath)
$statusMarkdownPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.statusMarkdownPath)
$nowUtc = (Get-Date).ToUniversalTime()

$cycleState = $null
if (Test-Path -LiteralPath $cycleStatePath -PathType Leaf) {
    $cycleState = Read-JsonFile -Path $cycleStatePath
}

$lastKnownStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastKnownStatus')
$lastReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastReleaseCandidateRunUtc')
$nextReleaseCandidateRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextReleaseCandidateRunUtc')
$lastDigestUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestUtc')
$nextMandatoryHitlReviewUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextMandatoryHitlReviewUtc')
$lastReleaseCandidateBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastReleaseCandidateBundle')
$lastDigestBundle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastDigestBundle')
$latestDigestBundlePath = if (-not [string]::IsNullOrWhiteSpace($lastDigestBundle)) {
    Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $lastDigestBundle
} else {
    $null
}
$retentionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.retentionStatePath)
$blockedEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.blockedEscalationStatePath)
$notificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.notificationStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$ciConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ciConcordanceStatePath)
$releaseRatificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseRatificationStatePath)
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeStatePath)
$publishRequestEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishRequestEnvelopeStatePath)
$postPublishEvidenceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishEvidenceStatePath)
$seedBraidEscalationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seedBraidEscalationStatePath)
$publishedRuntimeReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publishedRuntimeReceiptStatePath)
$artifactAttestationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.artifactAttestationStatePath)
$postPublishDriftWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishDriftWatchStatePath)
$operationalPublicationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operationalPublicationLedgerStatePath)
$externalConsumerConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.externalConsumerConcordanceStatePath)
$postPublishGovernanceLoopStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postPublishGovernanceLoopStatePath)
$publicationCadenceLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.publicationCadenceLedgerStatePath)
$downstreamRuntimeObservationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.downstreamRuntimeObservationStatePath)
$multiIntervalGovernanceBraidStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.multiIntervalGovernanceBraidStatePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchStatePath)
$retentionState = Read-JsonFileOrNull -Path $retentionStatePath
$blockedEscalationState = Read-JsonFileOrNull -Path $blockedEscalationStatePath
$notificationState = Read-JsonFileOrNull -Path $notificationStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath
$promotionGateState = Read-JsonFileOrNull -Path $promotionGateStatePath
$ciConcordanceState = Read-JsonFileOrNull -Path $ciConcordanceStatePath
$releaseRatificationState = Read-JsonFileOrNull -Path $releaseRatificationStatePath
$seededPromotionReviewState = Read-JsonFileOrNull -Path $seededPromotionReviewStatePath
$firstPublishIntentState = Read-JsonFileOrNull -Path $firstPublishIntentStatePath
$releaseHandshakeState = Read-JsonFileOrNull -Path $releaseHandshakeStatePath
$publishRequestEnvelopeState = Read-JsonFileOrNull -Path $publishRequestEnvelopeStatePath
$postPublishEvidenceState = Read-JsonFileOrNull -Path $postPublishEvidenceStatePath
$seedBraidEscalationState = Read-JsonFileOrNull -Path $seedBraidEscalationStatePath
$publishedRuntimeReceiptState = Read-JsonFileOrNull -Path $publishedRuntimeReceiptStatePath
$artifactAttestationState = Read-JsonFileOrNull -Path $artifactAttestationStatePath
$postPublishDriftWatchState = Read-JsonFileOrNull -Path $postPublishDriftWatchStatePath
$operationalPublicationLedgerState = Read-JsonFileOrNull -Path $operationalPublicationLedgerStatePath
$externalConsumerConcordanceState = Read-JsonFileOrNull -Path $externalConsumerConcordanceStatePath
$postPublishGovernanceLoopState = Read-JsonFileOrNull -Path $postPublishGovernanceLoopStatePath
$publicationCadenceLedgerState = Read-JsonFileOrNull -Path $publicationCadenceLedgerStatePath
$downstreamRuntimeObservationState = Read-JsonFileOrNull -Path $downstreamRuntimeObservationStatePath
$multiIntervalGovernanceBraidState = Read-JsonFileOrNull -Path $multiIntervalGovernanceBraidStatePath
$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$unattendedIntervalConcordanceState = Read-JsonFileOrNull -Path $unattendedIntervalConcordanceStatePath
$staleSurfaceContradictionWatchState = Read-JsonFileOrNull -Path $staleSurfaceContradictionWatchStatePath

$digestJson = $null
if (-not [string]::IsNullOrWhiteSpace($lastDigestBundle)) {
    $digestJsonPath = Join-Path $lastDigestBundle 'release-candidate-digest.json'
    if (Test-Path -LiteralPath $digestJsonPath -PathType Leaf) {
        $digestJson = Read-JsonFile -Path $digestJsonPath
    }
}

$scheduler = [ordered]@{
    taskName = [string] $taskingPolicy.scheduledTaskName
    registered = $false
    state = 'not-registered'
    lastRunTimeUtc = $null
    nextRunTimeUtc = $null
}

if (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue) {
    try {
        $scheduledTask = Get-ScheduledTask -TaskName ([string] $taskingPolicy.scheduledTaskName) -ErrorAction Stop
        $scheduledInfo = Get-ScheduledTaskInfo -TaskName ([string] $taskingPolicy.scheduledTaskName) -ErrorAction Stop
        $scheduler.registered = $true
        $scheduler.state = [string] $scheduledTask.State
        $scheduler.lastRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.LastRunTime)
        $scheduler.nextRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.NextRunTime)
    } catch {
    }
}

$releaseCandidateTaskStatus = if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    'blocked'
} elseif ($null -eq $nextReleaseCandidateRunUtc) {
    'uninitialized'
} elseif ($nextReleaseCandidateRunUtc -le $nowUtc) {
    'due'
} else {
    'waiting-for-cadence'
}

$dailyDigestTaskStatus = if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    'review-now-blocked'
} elseif ($null -eq $nextMandatoryHitlReviewUtc) {
    'uninitialized'
} elseif ($nextMandatoryHitlReviewUtc -le $nowUtc) {
    'review-due'
} else {
    'waiting-for-daily-review'
}

$promotionWatchStatus = 'clear-to-continue'
$recommendedAction = $null
$requiresImmediateHitl = $false
if ($null -ne $digestJson) {
    $recommendedAction = [string] $digestJson.recommendedAction
    $requiresImmediateHitl = [bool] $digestJson.requiresImmediateHitl
}

if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $promotionWatchStatus = 'blocked'
} elseif ($requiresImmediateHitl -or $lastKnownStatus -eq 'hitl-required') {
    $promotionWatchStatus = 'hitl-required'
}

$schedulerWatchStatus = if (-not [bool] $scheduler.registered) {
    'scheduler-unregistered'
} elseif ([string]::IsNullOrWhiteSpace([string] $scheduler.nextRunTimeUtc)) {
    'scheduler-missing-next-run'
} else {
    'scheduler-ready'
}

$taskEntries = @(
    foreach ($taskDefinition in $taskDefinitions) {
        $taskId = [string] $taskDefinition.id
        $status = 'uninitialized'
        $lastRunUtc = $null
        $nextRunUtc = $null
        $latestBundle = $null

        switch ($taskId) {
            'release-candidate-cycle' {
                $status = $releaseCandidateTaskStatus
                $lastRunUtc = if ($null -ne $lastReleaseCandidateRunUtc) { $lastReleaseCandidateRunUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
                $latestBundle = $lastReleaseCandidateBundle
            }
            'daily-hitl-digest' {
                $status = $dailyDigestTaskStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
                $latestBundle = $lastDigestBundle
            }
            'promotion-watch' {
                $status = $promotionWatchStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
                $latestBundle = $lastDigestBundle
            }
            'scheduler-watch' {
                $status = $schedulerWatchStatus
                $lastRunUtc = [string] $scheduler.lastRunTimeUtc
                $nextRunUtc = [string] $scheduler.nextRunTimeUtc
                $latestBundle = [string] $scheduler.taskName
            }
        }

        [ordered]@{
            id = $taskId
            label = [string] $taskDefinition.label
            taskClass = [string] $taskDefinition.taskClass
            owner = [string] $taskDefinition.owner
            authority = [string] $taskDefinition.authority
            purpose = [string] $taskDefinition.purpose
            completionSignal = [string] $taskDefinition.completionSignal
            outputs = @($taskDefinition.outputs | ForEach-Object { [string] $_ })
            escalatesWhen = @($taskDefinition.escalatesWhen | ForEach-Object { [string] $_ })
            status = $status
            lastRunUtc = $lastRunUtc
            nextRunUtc = $nextRunUtc
            latestBundle = $latestBundle
        }
    }
)

$activeLongFormTaskMap = $null
if (-not [string]::IsNullOrWhiteSpace($activeTaskMapId)) {
    $activeLongFormTaskMap = @($longFormTaskMaps | Where-Object { [string] $_.id -eq $activeTaskMapId } | Select-Object -First 1)
    if ($activeLongFormTaskMap -is [System.Array]) {
        $activeLongFormTaskMap = if ($activeLongFormTaskMap.Count -gt 0) { $activeLongFormTaskMap[0] } else { $null }
    }
}

$activeLongFormTaskMapStatus = 'uninitialized'
$activeLongFormTasksCompleted = 0
$activeLongFormTasksTotal = 0
$canPullForwardFromNextMap = $false
$eligibleNextTaskMap = $null

if ($null -ne $activeLongFormTaskMap) {
    $activeLongFormTasks = @($activeLongFormTaskMap.tasks)
    $activeLongFormTasksTotal = $activeLongFormTasks.Count
    $activeLongFormTaskStatuses = @(
        $activeLongFormTasks |
        ForEach-Object {
            Resolve-LongFormTaskLiveStatus `
                -TaskId ([string] $_.id) `
                -PolicyStatus ([string] $_.status) `
                -LatestDigestBundlePath $latestDigestBundlePath `
                -RetentionState $retentionState `
                -BlockedEscalationState $blockedEscalationState `
                -SeededGovernanceState $seededGovernanceState `
                -SchedulerReconciliationState $schedulerReconciliationState `
                -CmeConsolidationState $cmeConsolidationState `
                -PromotionGateState $promotionGateState `
                -CiConcordanceState $ciConcordanceState `
                -ReleaseRatificationState $releaseRatificationState `
                -SeededPromotionReviewState $seededPromotionReviewState `
                -FirstPublishIntentState $firstPublishIntentState `
                -ReleaseHandshakeState $releaseHandshakeState `
                -PublishRequestEnvelopeState $publishRequestEnvelopeState `
                -PostPublishEvidenceState $postPublishEvidenceState `
                -SeedBraidEscalationState $seedBraidEscalationState `
                -PublishedRuntimeReceiptState $publishedRuntimeReceiptState `
                -ArtifactAttestationState $artifactAttestationState `
                -PostPublishDriftWatchState $postPublishDriftWatchState `
                -OperationalPublicationLedgerState $operationalPublicationLedgerState `
                -ExternalConsumerConcordanceState $externalConsumerConcordanceState `
                -PostPublishGovernanceLoopState $postPublishGovernanceLoopState `
                -PublicationCadenceLedgerState $publicationCadenceLedgerState `
                -DownstreamRuntimeObservationState $downstreamRuntimeObservationState `
                -MultiIntervalGovernanceBraidState $multiIntervalGovernanceBraidState `
                -SchedulerExecutionReceiptState $schedulerExecutionReceiptState `
                -UnattendedIntervalConcordanceState $unattendedIntervalConcordanceState `
                -StaleSurfaceContradictionWatchState $staleSurfaceContradictionWatchState `
                -LastKnownStatus $lastKnownStatus `
                -BlockedStatus ([string] $cyclePolicy.blockedStatus)
        }
    )
    $activeLongFormTasksCompleted = @($activeLongFormTaskStatuses | Where-Object { $_ -eq 'completed' }).Count

    if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
        $activeLongFormTaskMapStatus = 'blocked'
    } elseif ($requiresImmediateHitl -or $lastKnownStatus -eq 'hitl-required') {
        $activeLongFormTaskMapStatus = 'waiting-for-hitl'
    } elseif ($activeLongFormTasksTotal -gt 0 -and $activeLongFormTasksCompleted -ge $activeLongFormTasksTotal) {
        $activeLongFormTaskMapStatus = 'completed'
    } else {
        $activeLongFormTaskMapStatus = 'in-progress'
    }

    $taskMapIndex = [array]::IndexOf($longFormTaskMaps, $activeLongFormTaskMap)
    if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $longFormTaskMaps.Count) {
        $eligibleNextTaskMap = $longFormTaskMaps[$taskMapIndex + 1]
    }

    $timeDilationPolicy = $taskingPolicy.PSObject.Properties['timeDilationPolicy']
    $allowPullForward = $false
    $pullForwardMaxMaps = 0
    if ($null -ne $timeDilationPolicy) {
        $allowPullForward = [bool] $timeDilationPolicy.Value.allowPullForward
        $pullForwardMaxMaps = [int] $timeDilationPolicy.Value.pullForwardMaxMaps
    }

    if ($allowPullForward -and $pullForwardMaxMaps -ge 1 -and
        $activeLongFormTaskMapStatus -eq 'completed' -and
        $null -ne $eligibleNextTaskMap) {
        $canPullForwardFromNextMap = $true
    }
}

$taskMapEntries = @(
    foreach ($taskMap in $longFormTaskMaps) {
        $taskMapTasks = @($taskMap.tasks)
        $taskMapTaskEntries = @(
            $taskMapTasks |
            ForEach-Object {
                $policyStatus = [string] $_.status
                $liveStatus = Resolve-LongFormTaskLiveStatus `
                    -TaskId ([string] $_.id) `
                    -PolicyStatus $policyStatus `
                    -LatestDigestBundlePath $latestDigestBundlePath `
                    -RetentionState $retentionState `
                    -BlockedEscalationState $blockedEscalationState `
                    -SeededGovernanceState $seededGovernanceState `
                    -SchedulerReconciliationState $schedulerReconciliationState `
                    -CmeConsolidationState $cmeConsolidationState `
                    -PromotionGateState $promotionGateState `
                    -CiConcordanceState $ciConcordanceState `
                    -ReleaseRatificationState $releaseRatificationState `
                    -SeededPromotionReviewState $seededPromotionReviewState `
                    -FirstPublishIntentState $firstPublishIntentState `
                    -ReleaseHandshakeState $releaseHandshakeState `
                    -PublishRequestEnvelopeState $publishRequestEnvelopeState `
                    -PostPublishEvidenceState $postPublishEvidenceState `
                    -SeedBraidEscalationState $seedBraidEscalationState `
                    -PublishedRuntimeReceiptState $publishedRuntimeReceiptState `
                    -ArtifactAttestationState $artifactAttestationState `
                    -PostPublishDriftWatchState $postPublishDriftWatchState `
                    -OperationalPublicationLedgerState $operationalPublicationLedgerState `
                    -ExternalConsumerConcordanceState $externalConsumerConcordanceState `
                    -PostPublishGovernanceLoopState $postPublishGovernanceLoopState `
                    -PublicationCadenceLedgerState $publicationCadenceLedgerState `
                    -DownstreamRuntimeObservationState $downstreamRuntimeObservationState `
                    -MultiIntervalGovernanceBraidState $multiIntervalGovernanceBraidState `
                    -SchedulerExecutionReceiptState $schedulerExecutionReceiptState `
                    -UnattendedIntervalConcordanceState $unattendedIntervalConcordanceState `
                    -StaleSurfaceContradictionWatchState $staleSurfaceContradictionWatchState `
                    -LastKnownStatus $lastKnownStatus `
                    -BlockedStatus ([string] $cyclePolicy.blockedStatus)

                [ordered]@{
                    id = [string] $_.id
                    label = [string] $_.label
                    owner = [string] $_.owner
                    authority = [string] $_.authority
                    status = $policyStatus
                    liveStatus = $liveStatus
                    purpose = [string] $_.purpose
                    completionSignal = [string] $_.completionSignal
                    escalatesWhen = @($_.escalatesWhen | ForEach-Object { [string] $_ })
                }
            }
        )
        $completedCount = @($taskMapTaskEntries | Where-Object { [string] $_.liveStatus -eq 'completed' }).Count
        [ordered]@{
            id = [string] $taskMap.id
            label = [string] $taskMap.label
            status = [string] $taskMap.status
            expectedReviewWindows = [int] $taskMap.expectedReviewWindows
            goal = [string] $taskMap.goal
            completedTaskCount = $completedCount
            totalTaskCount = $taskMapTasks.Count
            taskIds = @($taskMap.taskIds | ForEach-Object { [string] $_ })
            tasks = $taskMapTaskEntries
        }
    }
)

$statusPayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $nowUtc.ToString('o')
    cyclePolicyPath = $resolvedCyclePolicyPath
    taskingPolicyPath = $resolvedTaskingPolicyPath
    formalSurfaceMarkdownPath = [string] $taskingPolicy.formalSurfaceMarkdownPath
    longFormTasking = [ordered]@{
        activeTaskMapId = $activeTaskMapId
        activeTaskMapStatus = $activeLongFormTaskMapStatus
        activeTaskMapCompletedTaskCount = $activeLongFormTasksCompleted
        activeTaskMapTotalTaskCount = $activeLongFormTasksTotal
        canPullForwardFromNextMap = $canPullForwardFromNextMap
        eligibleNextTaskMapId = if ($null -ne $eligibleNextTaskMap) { [string] $eligibleNextTaskMap.id } else { $null }
        pullForwardRule = [string] $taskingPolicy.timeDilationPolicy.rule
        activeRunStatePath = $activeLongFormRunStatePath
        activeRun = if ($null -ne $activeLongFormRun) {
            [ordered]@{
                runId = [string] $activeLongFormRun.runId
                runStatus = [string] $activeLongFormRun.runStatus
                currentPhaseId = [string] $activeLongFormRun.currentPhaseId
                currentPhaseLabel = [string] $activeLongFormRun.currentPhaseLabel
                windowEndUtc = [string] $activeLongFormRun.timeframe.endUtc
                iterationLaw = $activeLongFormRun.iterationLaw
            }
        } else {
            $null
        }
        taskMaps = $taskMapEntries
    }
    scheduler = $scheduler
    currentPosture = [ordered]@{
        lastKnownStatus = $lastKnownStatus
        recommendedAction = $recommendedAction
        requiresImmediateHitl = $requiresImmediateHitl
        lastReleaseCandidateBundle = $lastReleaseCandidateBundle
        lastDigestBundle = $lastDigestBundle
        seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
        seededGovernanceReason = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.dispositionReason } else { $null }
        seededGovernanceProvenance = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.provenance } else { $null }
        seededGovernanceReadyState = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyState') } else { $null }
        seededGovernanceReadyReason = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyReasonCode') } else { $null }
        seededGovernanceReadyAction = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyActionTaken') } else { $null }
        notificationTriggered = if ($null -ne $notificationState) { [bool] $notificationState.triggered } else { $null }
        notificationTriggerReason = if ($null -ne $notificationState) { [string] $notificationState.triggerReason } else { $null }
        lastNotificationBundle = if ($null -ne $notificationState) { [string] $notificationState.lastNotificationBundle } else { $null }
        schedulerReconciliationAction = if ($null -ne $schedulerReconciliationState) { [string] $schedulerReconciliationState.actionTaken } else { $null }
        schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $null }
        cmeConsolidationState = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { $null }
        cmeConsolidationReason = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.reasonCode } else { $null }
        promotionGateRecommendation = if ($null -ne $promotionGateState) { [string] $promotionGateState.recommendation } else { $null }
        promotionGateReason = if ($null -ne $promotionGateState) { [string] $promotionGateState.reasonCode } else { $null }
        ciConcordanceState = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.concordanceState } else { $null }
        ciConcordanceReason = if ($null -ne $ciConcordanceState) { [string] $ciConcordanceState.reasonCode } else { $null }
        releaseRatificationState = if ($null -ne $releaseRatificationState) { [string] $releaseRatificationState.rehearsalState } else { $null }
        releaseRatificationDecision = if ($null -ne $releaseRatificationState) { [string] $releaseRatificationState.nextHumanDecision } else { $null }
        seededPromotionReviewDisposition = if ($null -ne $seededPromotionReviewState) { [string] $seededPromotionReviewState.disposition } else { $null }
        seededPromotionReviewReason = if ($null -ne $seededPromotionReviewState) { [string] $seededPromotionReviewState.reasonCode } else { $null }
        seededPromotionReviewReadyState = if ($null -ne $seededPromotionReviewState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyState') } else { $null }
        seededPromotionReviewReadyReason = if ($null -ne $seededPromotionReviewState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyReasonCode') } else { $null }
        seededPromotionReviewReadyAction = if ($null -ne $seededPromotionReviewState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyActionTaken') } else { $null }
        firstPublishIntentState = if ($null -ne $firstPublishIntentState) { [string] $firstPublishIntentState.intentState } else { $null }
        firstPublishIntentReason = if ($null -ne $firstPublishIntentState) { [string] $firstPublishIntentState.reasonCode } else { $null }
        releaseHandshakeState = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.handshakeState } else { $null }
        releaseHandshakeReason = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.reasonCode } else { $null }
        releaseHandshakeNextAction = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.nextAction } else { $null }
        publishRequestEnvelopeState = if ($null -ne $publishRequestEnvelopeState) { [string] $publishRequestEnvelopeState.requestState } else { $null }
        publishRequestEnvelopeReason = if ($null -ne $publishRequestEnvelopeState) { [string] $publishRequestEnvelopeState.reasonCode } else { $null }
        publishRequestEnvelopeNextAction = if ($null -ne $publishRequestEnvelopeState) { [string] $publishRequestEnvelopeState.nextAction } else { $null }
        postPublishEvidenceState = if ($null -ne $postPublishEvidenceState) { [string] $postPublishEvidenceState.loopState } else { $null }
        postPublishEvidenceReason = if ($null -ne $postPublishEvidenceState) { [string] $postPublishEvidenceState.reasonCode } else { $null }
        postPublishEvidenceNextAction = if ($null -ne $postPublishEvidenceState) { [string] $postPublishEvidenceState.nextAction } else { $null }
        seedBraidEscalationState = if ($null -ne $seedBraidEscalationState) { [string] $seedBraidEscalationState.laneState } else { $null }
        seedBraidEscalationReason = if ($null -ne $seedBraidEscalationState) { [string] $seedBraidEscalationState.reasonCode } else { $null }
        seedBraidEscalationNextAction = if ($null -ne $seedBraidEscalationState) { [string] $seedBraidEscalationState.nextAction } else { $null }
        publishedRuntimeReceiptState = if ($null -ne $publishedRuntimeReceiptState) { [string] $publishedRuntimeReceiptState.receiptState } else { $null }
        publishedRuntimeReceiptReason = if ($null -ne $publishedRuntimeReceiptState) { [string] $publishedRuntimeReceiptState.reasonCode } else { $null }
        publishedRuntimeReceiptNextAction = if ($null -ne $publishedRuntimeReceiptState) { [string] $publishedRuntimeReceiptState.nextAction } else { $null }
        artifactAttestationState = if ($null -ne $artifactAttestationState) { [string] $artifactAttestationState.attestationState } else { $null }
        artifactAttestationReason = if ($null -ne $artifactAttestationState) { [string] $artifactAttestationState.reasonCode } else { $null }
        artifactAttestationNextAction = if ($null -ne $artifactAttestationState) { [string] $artifactAttestationState.nextAction } else { $null }
        postPublishDriftWatchState = if ($null -ne $postPublishDriftWatchState) { [string] $postPublishDriftWatchState.driftWatchState } else { $null }
        postPublishDriftWatchReason = if ($null -ne $postPublishDriftWatchState) { [string] $postPublishDriftWatchState.reasonCode } else { $null }
        postPublishDriftWatchNextAction = if ($null -ne $postPublishDriftWatchState) { [string] $postPublishDriftWatchState.nextAction } else { $null }
        operationalPublicationLedgerState = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.ledgerState } else { $null }
        operationalPublicationLedgerReason = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.reasonCode } else { $null }
        operationalPublicationLedgerNextAction = if ($null -ne $operationalPublicationLedgerState) { [string] $operationalPublicationLedgerState.nextAction } else { $null }
        externalConsumerConcordanceState = if ($null -ne $externalConsumerConcordanceState) { [string] $externalConsumerConcordanceState.concordanceState } else { $null }
        externalConsumerConcordanceReason = if ($null -ne $externalConsumerConcordanceState) { [string] $externalConsumerConcordanceState.reasonCode } else { $null }
        externalConsumerConcordanceNextAction = if ($null -ne $externalConsumerConcordanceState) { [string] $externalConsumerConcordanceState.nextAction } else { $null }
        postPublishGovernanceLoopState = if ($null -ne $postPublishGovernanceLoopState) { [string] $postPublishGovernanceLoopState.governanceLoopState } else { $null }
        postPublishGovernanceLoopReason = if ($null -ne $postPublishGovernanceLoopState) { [string] $postPublishGovernanceLoopState.reasonCode } else { $null }
        postPublishGovernanceLoopNextAction = if ($null -ne $postPublishGovernanceLoopState) { [string] $postPublishGovernanceLoopState.nextAction } else { $null }
        publicationCadenceLedgerState = if ($null -ne $publicationCadenceLedgerState) { [string] $publicationCadenceLedgerState.cadenceState } else { $null }
        publicationCadenceLedgerReason = if ($null -ne $publicationCadenceLedgerState) { [string] $publicationCadenceLedgerState.reasonCode } else { $null }
        publicationCadenceLedgerNextAction = if ($null -ne $publicationCadenceLedgerState) { [string] $publicationCadenceLedgerState.nextAction } else { $null }
        downstreamRuntimeObservationState = if ($null -ne $downstreamRuntimeObservationState) { [string] $downstreamRuntimeObservationState.observationState } else { $null }
        downstreamRuntimeObservationReason = if ($null -ne $downstreamRuntimeObservationState) { [string] $downstreamRuntimeObservationState.reasonCode } else { $null }
        downstreamRuntimeObservationNextAction = if ($null -ne $downstreamRuntimeObservationState) { [string] $downstreamRuntimeObservationState.nextAction } else { $null }
        multiIntervalGovernanceBraidState = if ($null -ne $multiIntervalGovernanceBraidState) { [string] $multiIntervalGovernanceBraidState.braidState } else { $null }
        multiIntervalGovernanceBraidReason = if ($null -ne $multiIntervalGovernanceBraidState) { [string] $multiIntervalGovernanceBraidState.reasonCode } else { $null }
        multiIntervalGovernanceBraidNextAction = if ($null -ne $multiIntervalGovernanceBraidState) { [string] $multiIntervalGovernanceBraidState.nextAction } else { $null }
        schedulerExecutionReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
        schedulerExecutionReceiptReason = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.reasonCode } else { $null }
        schedulerExecutionReceiptNextAction = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.nextAction } else { $null }
        unattendedIntervalConcordanceState = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.concordanceState } else { $null }
        unattendedIntervalConcordanceReason = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.reasonCode } else { $null }
        unattendedIntervalConcordanceNextAction = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.nextAction } else { $null }
        staleSurfaceContradictionWatchState = if ($null -ne $staleSurfaceContradictionWatchState) { [string] $staleSurfaceContradictionWatchState.watchState } else { $null }
        staleSurfaceContradictionWatchReason = if ($null -ne $staleSurfaceContradictionWatchState) { [string] $staleSurfaceContradictionWatchState.reasonCode } else { $null }
        staleSurfaceContradictionWatchNextAction = if ($null -ne $staleSurfaceContradictionWatchState) { [string] $staleSurfaceContradictionWatchState.nextAction } else { $null }
        nextReleaseCandidateRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
        nextMandatoryHitlReviewUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
    }
    tasks = $taskEntries
}

Write-JsonFile -Path $statusJsonPath -Value $statusPayload

$markdownLines = @(
    '# Local Automation Tasking Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $statusPayload.generatedAtUtc),
    ('- Formal tasking surface: `{0}`' -f $statusPayload.formalSurfaceMarkdownPath),
    ('- Active long-form task map: `{0}`' -f $activeTaskMapId),
    ('- Active map posture: `{0}`' -f $activeLongFormTaskMapStatus),
    ('- Pull-forward allowed from next map: `{0}`' -f $canPullForwardFromNextMap),
    ('- Scheduler task: `{0}`' -f $scheduler.taskName),
    ('- Scheduler registered: `{0}`' -f $scheduler.registered),
    ('- Scheduler state: `{0}`' -f $scheduler.state),
    ('- Current posture: `{0}`' -f $lastKnownStatus),
    ('- Recommended action: `{0}`' -f $recommendedAction),
    ('- Requires immediate HITL: `{0}`' -f $requiresImmediateHitl),
    ('- Next release-candidate run (UTC): `{0}`' -f $(if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { 'uninitialized' })),
    ('- Next mandatory HITL review (UTC): `{0}`' -f $(if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { 'uninitialized' })),
    ''
)

if ($scheduler.registered) {
    $markdownLines += @(
        '## Scheduler',
        '',
        ('- Last scheduled run (UTC): `{0}`' -f $(if ($scheduler.lastRunTimeUtc) { $scheduler.lastRunTimeUtc } else { 'not-yet-run' })),
        ('- Next scheduled run (UTC): `{0}`' -f $(if ($scheduler.nextRunTimeUtc) { $scheduler.nextRunTimeUtc } else { 'not-available' })),
        ''
    )
}

if ($null -ne $seededGovernanceState) {
    $markdownLines += @(
        '## Seeded Governance',
        '',
        ('- Disposition: `{0}`' -f [string] $seededGovernanceState.disposition),
        ('- Reason: `{0}`' -f [string] $seededGovernanceState.dispositionReason),
        ('- Provenance: `{0}`' -f [string] $seededGovernanceState.provenance),
        ('- Ready state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyState')),
        ('- Ready reason: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyReasonCode')),
        ('- Ready action: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyActionTaken')),
        ''
    )
}

if ($null -ne $notificationState) {
    $markdownLines += @(
        '## Notification Surface',
        '',
        ('- Triggered on last cycle: `{0}`' -f [bool] $notificationState.triggered),
        ('- Trigger reason: `{0}`' -f [string] $notificationState.triggerReason),
        ('- Last notification bundle: `{0}`' -f $(if ([string] $notificationState.lastNotificationBundle) { [string] $notificationState.lastNotificationBundle } else { 'none' })),
        ''
    )
}

if ($null -ne $schedulerReconciliationState) {
    $markdownLines += @(
        '## Scheduler Reconciliation',
        '',
        ('- Action taken: `{0}`' -f [string] $schedulerReconciliationState.actionTaken),
        ('- Aligned: `{0}`' -f [bool] $schedulerReconciliationState.aligned),
        ('- Desired next run (UTC): `{0}`' -f [string] $schedulerReconciliationState.desiredNextRunUtc),
        ('- Final next run (UTC): `{0}`' -f [string] $schedulerReconciliationState.finalNextRunUtc),
        ''
    )
}

if ($null -ne $cmeConsolidationState) {
    $markdownLines += @(
        '## CME Consolidation',
        '',
        ('- Consolidation state: `{0}`' -f [string] $cmeConsolidationState.consolidationState),
        ('- Reason code: `{0}`' -f [string] $cmeConsolidationState.reasonCode),
        ('- Consecutive accepted seed runs: `{0}`' -f [int] $cmeConsolidationState.consecutiveAcceptedCount),
        ''
    )
}

if ($null -ne $promotionGateState) {
    $markdownLines += @(
        '## Promotion Gate',
        '',
        ('- Recommendation: `{0}`' -f [string] $promotionGateState.recommendation),
        ('- Reason code: `{0}`' -f [string] $promotionGateState.reasonCode),
        ''
    )
}

if ($null -ne $ciConcordanceState) {
    $markdownLines += @(
        '## CI Concordance',
        '',
        ('- Concordance state: `{0}`' -f [string] $ciConcordanceState.concordanceState),
        ('- Reason code: `{0}`' -f [string] $ciConcordanceState.reasonCode),
        ''
    )
}

if ($null -ne $releaseRatificationState) {
    $markdownLines += @(
        '## Release Ratification Rehearsal',
        '',
        ('- Rehearsal state: `{0}`' -f [string] $releaseRatificationState.rehearsalState),
        ('- Next human decision: `{0}`' -f [string] $releaseRatificationState.nextHumanDecision),
        ''
    )
}

if ($null -ne $seededPromotionReviewState) {
    $markdownLines += @(
        '## Seeded Promotion Review',
        '',
        ('- Disposition: `{0}`' -f [string] $seededPromotionReviewState.disposition),
        ('- Reason code: `{0}`' -f [string] $seededPromotionReviewState.reasonCode),
        ('- Provenance: `{0}`' -f [string] $seededPromotionReviewState.provenance),
        ('- Ready state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyState')),
        ('- Ready reason: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyReasonCode')),
        ('- Ready action: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'readyActionTaken')),
        ''
    )
}

if ($null -ne $firstPublishIntentState) {
    $markdownLines += @(
        '## First Publish Intent',
        '',
        ('- Intent state: `{0}`' -f [string] $firstPublishIntentState.intentState),
        ('- Reason code: `{0}`' -f [string] $firstPublishIntentState.reasonCode),
        ('- Target first publish version: `{0}`' -f [string] $firstPublishIntentState.targetFirstPublishVersion),
        ''
    )
}

if ($null -ne $releaseHandshakeState) {
    $markdownLines += @(
        '## Release Handshake',
        '',
        ('- Handshake state: `{0}`' -f [string] $releaseHandshakeState.handshakeState),
        ('- Reason code: `{0}`' -f [string] $releaseHandshakeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $releaseHandshakeState.nextAction),
        ''
    )
}

if ($null -ne $publishRequestEnvelopeState) {
    $markdownLines += @(
        '## Publish Request Envelope',
        '',
        ('- Request state: `{0}`' -f [string] $publishRequestEnvelopeState.requestState),
        ('- Reason code: `{0}`' -f [string] $publishRequestEnvelopeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $publishRequestEnvelopeState.nextAction),
        ''
    )
}

if ($null -ne $postPublishEvidenceState) {
    $markdownLines += @(
        '## Post-Publish Evidence Loop',
        '',
        ('- Loop state: `{0}`' -f [string] $postPublishEvidenceState.loopState),
        ('- Reason code: `{0}`' -f [string] $postPublishEvidenceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $postPublishEvidenceState.nextAction),
        ''
    )
}

if ($null -ne $seedBraidEscalationState) {
    $markdownLines += @(
        '## Seed Braid Escalation',
        '',
        ('- Lane state: `{0}`' -f [string] $seedBraidEscalationState.laneState),
        ('- Reason code: `{0}`' -f [string] $seedBraidEscalationState.reasonCode),
        ('- Next action: `{0}`' -f [string] $seedBraidEscalationState.nextAction),
        ''
    )
}

if ($null -ne $publishedRuntimeReceiptState) {
    $markdownLines += @(
        '## Published Runtime Receipt',
        '',
        ('- Receipt state: `{0}`' -f [string] $publishedRuntimeReceiptState.receiptState),
        ('- Reason code: `{0}`' -f [string] $publishedRuntimeReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $publishedRuntimeReceiptState.nextAction),
        ''
    )
}

if ($null -ne $artifactAttestationState) {
    $markdownLines += @(
        '## Artifact Attestation',
        '',
        ('- Attestation state: `{0}`' -f [string] $artifactAttestationState.attestationState),
        ('- Reason code: `{0}`' -f [string] $artifactAttestationState.reasonCode),
        ('- Next action: `{0}`' -f [string] $artifactAttestationState.nextAction),
        ''
    )
}

if ($null -ne $postPublishDriftWatchState) {
    $markdownLines += @(
        '## Post-Publish Drift Watch',
        '',
        ('- Drift watch state: `{0}`' -f [string] $postPublishDriftWatchState.driftWatchState),
        ('- Reason code: `{0}`' -f [string] $postPublishDriftWatchState.reasonCode),
        ('- Next action: `{0}`' -f [string] $postPublishDriftWatchState.nextAction),
        ''
    )
}

if ($null -ne $operationalPublicationLedgerState) {
    $markdownLines += @(
        '## Operational Publication Ledger',
        '',
        ('- Ledger state: `{0}`' -f [string] $operationalPublicationLedgerState.ledgerState),
        ('- Reason code: `{0}`' -f [string] $operationalPublicationLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $operationalPublicationLedgerState.nextAction),
        ''
    )
}

if ($null -ne $externalConsumerConcordanceState) {
    $markdownLines += @(
        '## External Consumer Concordance',
        '',
        ('- Concordance state: `{0}`' -f [string] $externalConsumerConcordanceState.concordanceState),
        ('- Reason code: `{0}`' -f [string] $externalConsumerConcordanceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $externalConsumerConcordanceState.nextAction),
        ''
    )
}

if ($null -ne $postPublishGovernanceLoopState) {
    $markdownLines += @(
        '## Post-Publish Governance Loop',
        '',
        ('- Governance loop state: `{0}`' -f [string] $postPublishGovernanceLoopState.governanceLoopState),
        ('- Reason code: `{0}`' -f [string] $postPublishGovernanceLoopState.reasonCode),
        ('- Next action: `{0}`' -f [string] $postPublishGovernanceLoopState.nextAction),
        ''
    )
}

if ($null -ne $publicationCadenceLedgerState) {
    $markdownLines += @(
        '## Publication Cadence Ledger',
        '',
        ('- Cadence state: `{0}`' -f [string] $publicationCadenceLedgerState.cadenceState),
        ('- Reason code: `{0}`' -f [string] $publicationCadenceLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $publicationCadenceLedgerState.nextAction),
        ''
    )
}

if ($null -ne $downstreamRuntimeObservationState) {
    $markdownLines += @(
        '## Downstream Runtime Observation',
        '',
        ('- Observation state: `{0}`' -f [string] $downstreamRuntimeObservationState.observationState),
        ('- Reason code: `{0}`' -f [string] $downstreamRuntimeObservationState.reasonCode),
        ('- Next action: `{0}`' -f [string] $downstreamRuntimeObservationState.nextAction),
        ''
    )
}

if ($null -ne $multiIntervalGovernanceBraidState) {
    $markdownLines += @(
        '## Multi-Interval Governance Braid',
        '',
        ('- Braid state: `{0}`' -f [string] $multiIntervalGovernanceBraidState.braidState),
        ('- Reason code: `{0}`' -f [string] $multiIntervalGovernanceBraidState.reasonCode),
        ('- Next action: `{0}`' -f [string] $multiIntervalGovernanceBraidState.nextAction),
        ''
    )
}

if ($null -ne $schedulerExecutionReceiptState) {
    $markdownLines += @(
        '## Scheduler Execution Receipt',
        '',
        ('- Receipt state: `{0}`' -f [string] $schedulerExecutionReceiptState.receiptState),
        ('- Reason code: `{0}`' -f [string] $schedulerExecutionReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $schedulerExecutionReceiptState.nextAction),
        ('- Last scheduled run (UTC): `{0}`' -f $(if ($schedulerExecutionReceiptState.lastScheduledRunUtc) { [string] $schedulerExecutionReceiptState.lastScheduledRunUtc } else { 'not-yet-run' })),
        ''
    )
}

if ($null -ne $unattendedIntervalConcordanceState) {
    $markdownLines += @(
        '## Unattended Interval Concordance',
        '',
        ('- Concordance state: `{0}`' -f [string] $unattendedIntervalConcordanceState.concordanceState),
        ('- Reason code: `{0}`' -f [string] $unattendedIntervalConcordanceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $unattendedIntervalConcordanceState.nextAction),
        ''
    )
}

if ($null -ne $staleSurfaceContradictionWatchState) {
    $markdownLines += @(
        '## Stale Surface Contradiction Watch',
        '',
        ('- Watch state: `{0}`' -f [string] $staleSurfaceContradictionWatchState.watchState),
        ('- Reason code: `{0}`' -f [string] $staleSurfaceContradictionWatchState.reasonCode),
        ('- Next action: `{0}`' -f [string] $staleSurfaceContradictionWatchState.nextAction),
        ('- Contradictions: `{0}`' -f $(if ($staleSurfaceContradictionWatchState.contradictions -and @($staleSurfaceContradictionWatchState.contradictions).Count -gt 0) { ((@($staleSurfaceContradictionWatchState.contradictions) -join ', ')) } else { 'none' })),
        ''
    )
}

if ($null -ne $activeLongFormTaskMap) {
    $markdownLines += @(
        '## Long-Form Task Map',
        '',
        ('- Active map: `{0}`' -f $activeLongFormTaskMap.label),
        ('- Goal: `{0}`' -f $activeLongFormTaskMap.goal),
        ('- Expected review windows: `{0}`' -f $activeLongFormTaskMap.expectedReviewWindows),
        ('- Completed tasks: `{0}/{1}`' -f $activeLongFormTasksCompleted, $activeLongFormTasksTotal),
        ('- Pull-forward rule: `{0}`' -f [string] $taskingPolicy.timeDilationPolicy.rule)
    )

    if ($null -ne $eligibleNextTaskMap) {
        $markdownLines += ('- Eligible next map: `{0}`' -f [string] $eligibleNextTaskMap.label)
    }

    $markdownLines += @(
        '',
        '| Task | Owner | Policy | Live |',
        '| --- | --- | --- | --- |'
    )

    foreach ($task in @($activeLongFormTaskMap.tasks)) {
        $taskLiveStatus = Resolve-LongFormTaskLiveStatus `
            -TaskId ([string] $task.id) `
            -PolicyStatus ([string] $task.status) `
            -LatestDigestBundlePath $latestDigestBundlePath `
            -RetentionState $retentionState `
            -BlockedEscalationState $blockedEscalationState `
            -SeededGovernanceState $seededGovernanceState `
            -SchedulerReconciliationState $schedulerReconciliationState `
            -CmeConsolidationState $cmeConsolidationState `
            -PromotionGateState $promotionGateState `
            -CiConcordanceState $ciConcordanceState `
            -ReleaseRatificationState $releaseRatificationState `
            -SeededPromotionReviewState $seededPromotionReviewState `
            -FirstPublishIntentState $firstPublishIntentState `
            -ReleaseHandshakeState $releaseHandshakeState `
            -PublishRequestEnvelopeState $publishRequestEnvelopeState `
            -PostPublishEvidenceState $postPublishEvidenceState `
            -SeedBraidEscalationState $seedBraidEscalationState `
            -PublishedRuntimeReceiptState $publishedRuntimeReceiptState `
            -ArtifactAttestationState $artifactAttestationState `
            -PostPublishDriftWatchState $postPublishDriftWatchState `
            -OperationalPublicationLedgerState $operationalPublicationLedgerState `
            -ExternalConsumerConcordanceState $externalConsumerConcordanceState `
            -PostPublishGovernanceLoopState $postPublishGovernanceLoopState `
            -PublicationCadenceLedgerState $publicationCadenceLedgerState `
            -DownstreamRuntimeObservationState $downstreamRuntimeObservationState `
            -MultiIntervalGovernanceBraidState $multiIntervalGovernanceBraidState `
            -SchedulerExecutionReceiptState $schedulerExecutionReceiptState `
            -UnattendedIntervalConcordanceState $unattendedIntervalConcordanceState `
            -StaleSurfaceContradictionWatchState $staleSurfaceContradictionWatchState `
            -LastKnownStatus $lastKnownStatus `
            -BlockedStatus ([string] $cyclePolicy.blockedStatus)
        $markdownLines += ('| {0} | {1} | {2} | {3} |' -f [string] $task.label, [string] $task.owner, [string] $task.status, $taskLiveStatus)
    }

    if ($null -ne $activeLongFormRun) {
        $markdownLines += @(
            '',
            '## Active Long-Form Run',
            '',
            ('- Run ID: `{0}`' -f [string] $activeLongFormRun.runId),
            ('- Run status: `{0}`' -f [string] $activeLongFormRun.runStatus),
            ('- Current phase: `{0}`' -f [string] $activeLongFormRun.currentPhaseLabel),
            ('- Window end (UTC): `{0}`' -f [string] $activeLongFormRun.timeframe.endUtc),
            ('- Iteration law: `{0}`' -f [string] $activeLongFormRun.iterationLaw.rule)
        )
    }
}

$markdownLines += @(
    '## Tasks',
    '',
    '| Task | Owner | Status | Last Run (UTC) | Next Run (UTC) |',
    '| --- | --- | --- | --- | --- |'
)

foreach ($task in $taskEntries) {
    $markdownLines += ('| {0} | {1} | {2} | {3} | {4} |' -f $task.label, $task.owner, $task.status, $(if ($task.lastRunUtc) { $task.lastRunUtc } else { 'not-yet-run' }), $(if ($task.nextRunUtc) { $task.nextRunUtc } else { 'not-scheduled' }))
}

Set-Content -LiteralPath $statusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[local-automation-status] JSON: {0}' -f $statusJsonPath)
$statusJsonPath
