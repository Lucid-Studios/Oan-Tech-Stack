param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-tasking.json',
    [string] $ActiveTaskMapStatePath = '.audit/state/local-automation-active-task-map-selection.json'
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
$automationControlSignalsHelperPath = Join-Path $PSScriptRoot 'Automation-ControlSignals.ps1'
. $automationControlSignalsHelperPath

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

function Get-ScheduledTaskSnapshot {
    param([string] $TaskName)

    $snapshot = [ordered]@{
        taskName = $TaskName
        registered = $false
        state = 'not-registered'
        lastRunTimeUtc = $null
        nextRunTimeUtc = $null
    }

    if (-not (Get-Command -Name Get-ScheduledTask -ErrorAction SilentlyContinue)) {
        return [pscustomobject] $snapshot
    }

    try {
        $scheduledTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        $scheduledInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
        $snapshot.registered = $true
        $snapshot.state = [string] $scheduledTask.State
        $snapshot.lastRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.LastRunTime)
        if ([string]::Equals([string] $scheduledTask.State, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase)) {
            $snapshot.nextRunTimeUtc = $null
        } else {
            $snapshot.nextRunTimeUtc = Get-MeaningfulScheduledDateTimeUtcStringOrNull -Value ([datetime] $scheduledInfo.NextRunTime)
        }
    } catch {
    }

    return [pscustomobject] $snapshot
}

function Resolve-OfficeSchedulerStatus {
    param(
        [object] $Snapshot,
        [string] $DesiredStatusWhenReady = 'healthy-armed',
        [bool] $AllowAwaitingRearm = $false
    )

    if ($null -eq $Snapshot -or -not [bool] $Snapshot.registered) {
        return 'scheduler-unregistered'
    }

    if ([string]::Equals([string] $Snapshot.state, 'Disabled', [System.StringComparison]::OrdinalIgnoreCase)) {
        if ($AllowAwaitingRearm) {
            return 'healthy-awaiting-rearm'
        }

        return 'drift-detected'
    }

    if ([string]::IsNullOrWhiteSpace([string] $Snapshot.nextRunTimeUtc)) {
        if ($AllowAwaitingRearm) {
            return 'healthy-awaiting-rearm'
        }

        return 'drift-detected'
    }

    return $DesiredStatusWhenReady
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
        [object] $UnattendedProofCollapseState,
        [object] $DormantWindowLedgerState,
        [object] $SilentCadenceIntegrityState,
        [object] $LongFormPhaseWitnessState,
        [object] $LongFormWindowBoundaryState,
        [object] $AutonomousLongFormCollapseState,
        [object] $SchedulerProofHarvestState,
        [object] $IntervalOriginClarificationState,
        [object] $QueuedTaskMapPromotionState,
        [object] $RuntimeDeployabilityEnvelopeState,
        [object] $SanctuaryRuntimeReadinessState,
        [object] $RuntimeWorkSurfaceAdmissibilityState,
        [object] $ReachAccessTopologyLedgerState,
        [object] $BondedOperatorLocalityReadinessState,
        [object] $ProtectedStateLegibilitySurfaceState,
        [object] $NexusSingularPortalFacadeState,
        [object] $DuplexPredicateEnvelopeState,
        [object] $OperatorActualWorkSessionRehearsalState,
        [object] $IdentityInvariantThreadRootState,
        [object] $GovernedThreadBirthReceiptState,
        [object] $InterWorkerBraidHandoffPacketState,
        [object] $AgentiCoreActualUtilitySurfaceState,
        [object] $ReachDuplexRealizationSeamState,
        [object] $BondedParticipationLocalityLedgerState,
        [object] $SanctuaryRuntimeWorkbenchSurfaceState,
        [object] $AmenableDayDreamTierAdmissibilityState,
        [object] $SelfRootedCrypticDepthGateState,
        [object] $RuntimeWorkbenchSessionLedgerState,
        [object] $RunIsolatedBuildPathwayState,
        [object] $DayDreamCollapseReceiptState,
        [object] $CrypticDepthReturnReceiptState,
        [object] $BondedCoWorkSessionRehearsalState,
        [object] $ReachReturnDissolutionReceiptState,
        [object] $LocalityDistinctionWitnessLedgerState,
        [object] $LocalHostSanctuaryResidencyEnvelopeState,
        [object] $RuntimeHabitationReadinessLedgerState,
        [object] $BoundedInhabitationLaunchRehearsalState,
        [object] $PostHabitationHorizonLatticeState,
        [object] $BoundedHorizonResearchBriefState,
        [object] $NextEraBatchSelectorState,
        [object] $InquirySessionDisciplineSurfaceState,
        [object] $BoundaryConditionLedgerState,
        [object] $CoherenceGainWitnessReceiptState,
        [object] $OperatorInquirySelectionEnvelopeState,
        [object] $BondedCrucibleSessionRehearsalState,
        [object] $SharedBoundaryMemoryLedgerState,
        [object] $ContinuityUnderPressureLedgerState,
        [object] $ExpressiveDeformationReceiptState,
        [object] $MutualIntelligibilityWitnessState,
        [object] $InquiryPatternContinuityLedgerState,
        [object] $QuestioningBoundaryPairLedgerState,
        [object] $CarryForwardInquirySelectionSurfaceState,
        [object] $EngramDistanceClassificationLedgerState,
        [object] $EngramPromotionRequirementsMatrixState,
        [object] $DistanceWeightedQuestioningAdmissionSurfaceState,
        [object] $QuestioningOperatorCandidateLedgerState,
        [object] $QuestioningGelPromotionGateState,
        [object] $ProtectedQuestioningPatternSurfaceState,
        [object] $VariationTestedReentryLedgerState,
        [object] $QuestioningAdmissionRefusalReceiptState,
        [object] $PromotionSeductionWatchState,
        [object] $EngramIntentFieldLedgerState,
        [object] $IntentConstraintAlignmentReceiptState,
        [object] $WarmReactivationDispositionReceiptState,
        [object] $FormationPhaseVectorState,
        [object] $BrittlenessWitnessState,
        [object] $DurabilityWitnessState,
        [object] $WarmClockDispositionState,
        [object] $RipeningStalenessLedgerState,
        [object] $CoolingPressureWitnessState,
        [object] $HotReactivationTriggerReceiptState,
        [object] $ColdAdmissionEligibilityGateState,
        [object] $ArchiveDispositionLedgerState,
        [object] $InterlockDensityLedgerState,
        [object] $BrittleDurableDifferentiationSurfaceState,
        [object] $CoreInvariantLatticeWitnessState,
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
        'unattended-proof-collapse' {
            if ($null -ne $UnattendedProofCollapseState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'dormant-window-ledger' {
            if ($null -ne $DormantWindowLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'silent-cadence-integrity' {
            if ($null -ne $SilentCadenceIntegrityState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'long-form-phase-witness' {
            if ($null -ne $LongFormPhaseWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'long-form-window-boundary' {
            if ($null -ne $LongFormWindowBoundaryState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'autonomous-long-form-collapse' {
            if ($null -ne $AutonomousLongFormCollapseState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'scheduler-proof-harvest' {
            if ($null -ne $SchedulerProofHarvestState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'interval-origin-clarification' {
            if ($null -ne $IntervalOriginClarificationState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'queued-task-map-promotion' {
            if ($null -ne $QueuedTaskMapPromotionState) {
                if ([bool] (Get-ObjectPropertyValueOrNull -InputObject $QueuedTaskMapPromotionState -PropertyName 'promoted')) {
                    return 'completed'
                }

                return [string] (Get-ObjectPropertyValueOrNull -InputObject $QueuedTaskMapPromotionState -PropertyName 'promotionState')
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'runtime-deployability-envelope' {
            if ($null -ne $RuntimeDeployabilityEnvelopeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'sanctuary-runtime-readiness-receipt' {
            if ($null -ne $SanctuaryRuntimeReadinessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'runtime-work-surface-admissibility' {
            if ($null -ne $RuntimeWorkSurfaceAdmissibilityState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'reach-access-topology-ledger' {
            if ($null -ne $ReachAccessTopologyLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bonded-operator-locality-readiness' {
            if ($null -ne $BondedOperatorLocalityReadinessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'protected-state-legibility-surface' {
            if ($null -ne $ProtectedStateLegibilitySurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'nexus-singular-portal-facade' {
            if ($null -ne $NexusSingularPortalFacadeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'duplex-predicate-envelope' {
            if ($null -ne $DuplexPredicateEnvelopeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'operator-actual-work-session-rehearsal' {
            if ($null -ne $OperatorActualWorkSessionRehearsalState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'identity-invariant-thread-root' {
            if ($null -ne $IdentityInvariantThreadRootState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'governed-thread-birth-receipt' {
            if ($null -ne $GovernedThreadBirthReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'inter-worker-braid-handoff-packet' {
            if ($null -ne $InterWorkerBraidHandoffPacketState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'agenticore-actual-utility-surface' {
            if ($null -ne $AgentiCoreActualUtilitySurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'reach-duplex-realization-seam' {
            if ($null -ne $ReachDuplexRealizationSeamState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bonded-participation-locality-ledger' {
            if ($null -ne $BondedParticipationLocalityLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'sanctuary-runtime-workbench-surface' {
            if ($null -ne $SanctuaryRuntimeWorkbenchSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'amenable-day-dream-tier-admissibility' {
            if ($null -ne $AmenableDayDreamTierAdmissibilityState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'self-rooted-cryptic-depth-gate' {
            if ($null -ne $SelfRootedCrypticDepthGateState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'runtime-workbench-session-ledger' {
            if ($null -ne $RuntimeWorkbenchSessionLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'run-isolated-build-pathway' {
            if ($null -ne $RunIsolatedBuildPathwayState) {
                if ([bool] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'legacyFree') -and
                    [string] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'pathwayState') -eq 'run-isolated-build-ready') {
                    return 'completed'
                }

                return [string] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'pathwayState')
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'build-lane-fallback-burndown' {
            if ($null -ne $RunIsolatedBuildPathwayState) {
                if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'remainingLegacyTouchPointCount') -eq 0 -and
                    [int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -eq 0) {
                    return 'completed'
                }

                if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -gt 0) {
                    return 'touchpoint-resolution-incomplete'
                }

                return 'legacy-fallbacks-remain'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'lawful-exclusion-ledger' {
            if ($null -ne $RunIsolatedBuildPathwayState) {
                if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -eq 0) {
                    return 'completed'
                }

                return 'touchpoint-resolution-incomplete'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'day-dream-collapse-receipt' {
            if ($null -ne $DayDreamCollapseReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'cryptic-depth-return-receipt' {
            if ($null -ne $CrypticDepthReturnReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bonded-cowork-session-rehearsal' {
            if ($null -ne $BondedCoWorkSessionRehearsalState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'reach-return-dissolution-receipt' {
            if ($null -ne $ReachReturnDissolutionReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'locality-distinction-witness-ledger' {
            if ($null -ne $LocalityDistinctionWitnessLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'local-host-sanctuary-residency-envelope' {
            if ($null -ne $LocalHostSanctuaryResidencyEnvelopeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'runtime-habitation-readiness-ledger' {
            if ($null -ne $RuntimeHabitationReadinessLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bounded-inhabitation-launch-rehearsal' {
            if ($null -ne $BoundedInhabitationLaunchRehearsalState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'post-habitation-horizon-lattice' {
            if ($null -ne $PostHabitationHorizonLatticeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bounded-horizon-research-brief' {
            if ($null -ne $BoundedHorizonResearchBriefState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'next-era-batch-selector' {
            if ($null -ne $NextEraBatchSelectorState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'inquiry-session-discipline-surface' {
            if ($null -ne $InquirySessionDisciplineSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'boundary-condition-ledger' {
            if ($null -ne $BoundaryConditionLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'coherence-gain-witness-receipt' {
            if ($null -ne $CoherenceGainWitnessReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'operator-inquiry-selection-envelope' {
            if ($null -ne $OperatorInquirySelectionEnvelopeState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'bonded-crucible-session-rehearsal' {
            if ($null -ne $BondedCrucibleSessionRehearsalState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'shared-boundary-memory-ledger' {
            if ($null -ne $SharedBoundaryMemoryLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'continuity-under-pressure-ledger' {
            if ($null -ne $ContinuityUnderPressureLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'expressive-deformation-receipt' {
            if ($null -ne $ExpressiveDeformationReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'mutual-intelligibility-witness' {
            if ($null -ne $MutualIntelligibilityWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'inquiry-pattern-continuity-ledger' {
            if ($null -ne $InquiryPatternContinuityLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'questioning-boundary-pair-ledger' {
            if ($null -ne $QuestioningBoundaryPairLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'carry-forward-inquiry-selection-surface' {
            if ($null -ne $CarryForwardInquirySelectionSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'engram-distance-classification-ledger' {
            if ($null -ne $EngramDistanceClassificationLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'engram-promotion-requirements-matrix' {
            if ($null -ne $EngramPromotionRequirementsMatrixState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'distance-weighted-questioning-admission-surface' {
            if ($null -ne $DistanceWeightedQuestioningAdmissionSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'questioning-operator-candidate-ledger' {
            if ($null -ne $QuestioningOperatorCandidateLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'questioning-gel-promotion-gate' {
            if ($null -ne $QuestioningGelPromotionGateState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'protected-questioning-pattern-surface' {
            if ($null -ne $ProtectedQuestioningPatternSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'variation-tested-reentry-ledger' {
            if ($null -ne $VariationTestedReentryLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'questioning-admission-refusal-receipt' {
            if ($null -ne $QuestioningAdmissionRefusalReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'promotion-seduction-watch' {
            if ($null -ne $PromotionSeductionWatchState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'engram-intent-field-ledger' {
            if ($null -ne $EngramIntentFieldLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'intent-constraint-alignment-receipt' {
            if ($null -ne $IntentConstraintAlignmentReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'warm-reactivation-disposition-receipt' {
            if ($null -ne $WarmReactivationDispositionReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'formation-phase-vector' {
            if ($null -ne $FormationPhaseVectorState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'brittleness-witness' {
            if ($null -ne $BrittlenessWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'durability-witness' {
            if ($null -ne $DurabilityWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'warm-clock-disposition' {
            if ($null -ne $WarmClockDispositionState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'ripening-staleness-ledger' {
            if ($null -ne $RipeningStalenessLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'cooling-pressure-witness' {
            if ($null -ne $CoolingPressureWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'hot-reactivation-trigger-receipt' {
            if ($null -ne $HotReactivationTriggerReceiptState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'cold-admission-eligibility-gate' {
            if ($null -ne $ColdAdmissionEligibilityGateState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'archive-disposition-ledger' {
            if ($null -ne $ArchiveDispositionLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'interlock-density-ledger' {
            if ($null -ne $InterlockDensityLedgerState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'brittle-durable-differentiation-surface' {
            if ($null -ne $BrittleDurableDifferentiationSurfaceState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
        'core-invariant-lattice-witness' {
            if ($null -ne $CoreInvariantLatticeWitnessState) {
                return 'completed'
            }

            if ($PolicyStatus -eq 'selected') {
                return 'active'
            }
        }
    }

    return $PolicyStatus
}

function Resolve-RunIsolatedBuildTaskLiveStatus {
    param(
        [string] $TaskId,
        [string] $CurrentLiveStatus,
        [object] $RunIsolatedBuildPathwayState
    )

    if ($null -eq $RunIsolatedBuildPathwayState) {
        return $CurrentLiveStatus
    }

    switch ($TaskId) {
        'run-isolated-build-pathway' {
            if ([bool] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'legacyFree') -and
                [string] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'pathwayState') -eq 'run-isolated-build-ready') {
                return 'completed'
            }

            return [string] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'pathwayState')
        }
        'build-lane-fallback-burndown' {
            if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'remainingLegacyTouchPointCount') -eq 0 -and
                [int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -eq 0) {
                return 'completed'
            }

            if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -gt 0) {
                return 'touchpoint-resolution-incomplete'
            }

            return 'legacy-fallbacks-remain'
        }
        'lawful-exclusion-ledger' {
            if ([int] (Get-ObjectPropertyValueOrNull -InputObject $RunIsolatedBuildPathwayState -PropertyName 'unresolvedTouchPointCount') -eq 0) {
                return 'completed'
            }

            return 'touchpoint-resolution-incomplete'
        }
    }

    return $CurrentLiveStatus
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$resolvedActiveTaskMapStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $ActiveTaskMapStatePath
$cyclePolicy = Read-JsonFile -Path $resolvedCyclePolicyPath
$taskingPolicy = Read-JsonFile -Path $resolvedTaskingPolicyPath
$activeTaskMapState = Read-JsonFileOrNull -Path $resolvedActiveTaskMapStatePath
$taskDefinitions = @($taskingPolicy.tasks)
$longFormTaskMaps = @($taskingPolicy.longFormTaskMaps)
$activeTaskMapId = if ($null -ne $activeTaskMapState -and -not [string]::IsNullOrWhiteSpace([string] $activeTaskMapState.activeTaskMapId)) {
    [string] $activeTaskMapState.activeTaskMapId
} else {
    [string] $taskingPolicy.activeTaskMapId
}
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
$currentActionClass = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'actionClass')
if ([string]::IsNullOrWhiteSpace($currentActionClass)) {
    $currentActionClass = Get-AutomationActionClassFromStatus -Status $lastKnownStatus
}
$mainWorkerTerminalState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'mainWorkerTerminalState')
$mainWorkerArmState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'mainWorkerArmState')
$lastMainWorkerCloseUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastMainWorkerCloseUtc')
$nextMainWorkerWakeUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextMainWorkerWakeUtc')
$nextWatchdogRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextWatchdogRunUtc')
$nextDailyHitlDigestRunUtc = Get-OptionalDateTimeUtc -Value (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'nextDailyHitlDigestRunUtc')
$lastMainWorkerCloseDisposition = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastMainWorkerCloseDisposition')
$dopingHeaderStatePathFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'dopingHeaderStatePath')
$cycleReceiptStatePathFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'cycleReceiptStatePath')
$readinessNoticeStatePathFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'readinessNoticeStatePath')
$pauseNoticeStatePathFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'pauseNoticeStatePath')
$currentNoticeTypeFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'currentNoticeType')
$currentNoticeStatusFromCycle = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'currentNoticeStatus')
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
$cmeFormationAndOfficeLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerStatePath)
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
$watchdogStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.watchdogStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)
$staleSurfaceContradictionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.staleSurfaceContradictionWatchStatePath)
$unattendedProofCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseStatePath)
$dormantWindowLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerStatePath)
$silentCadenceIntegrityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.silentCadenceIntegrityStatePath)
$longFormPhaseWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessStatePath)
$longFormWindowBoundaryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormWindowBoundaryStatePath)
$autonomousLongFormCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.autonomousLongFormCollapseStatePath)
$schedulerProofHarvestStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerProofHarvestStatePath)
$intervalOriginClarificationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intervalOriginClarificationStatePath)
$queuedTaskMapPromotionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.queuedTaskMapPromotionStatePath)
$masterThreadOrchestrationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.masterThreadOrchestrationStatePath)
$runtimeDeployabilityEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$reachAccessTopologyLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerStatePath)
$bondedOperatorLocalityReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedOperatorLocalityReadinessStatePath)
$protectedStateLegibilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceStatePath)
$nexusSingularPortalFacadeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nexusSingularPortalFacadeStatePath)
$duplexPredicateEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.duplexPredicateEnvelopeStatePath)
$operatorActualWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorActualWorkSessionRehearsalStatePath)
$identityInvariantThreadRootStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootStatePath)
$governedThreadBirthReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptStatePath)
$interWorkerBraidHandoffPacketStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interWorkerBraidHandoffPacketStatePath)
$agentiCoreActualUtilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceStatePath)
$reachDuplexRealizationSeamStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamStatePath)
$bondedParticipationLocalityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerStatePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)
$selfRootedCrypticDepthGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$runIsolatedBuildPathwayStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runIsolatedBuildPathwayStatePath)
$dayDreamCollapseReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptStatePath)
$crypticDepthReturnReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptStatePath)
$bondedCoWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalStatePath)
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$localHostSanctuaryResidencyEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localHostSanctuaryResidencyEnvelopeStatePath)
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$boundedInhabitationLaunchRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedInhabitationLaunchRehearsalStatePath)
$postHabitationHorizonLatticeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.postHabitationHorizonLatticeStatePath)
$boundedHorizonResearchBriefStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundedHorizonResearchBriefStatePath)
$nextEraBatchSelectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nextEraBatchSelectorStatePath)
$inquirySessionDisciplineSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceStatePath)
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)
$coherenceGainWitnessReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptStatePath)
$operatorInquirySelectionEnvelopeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.operatorInquirySelectionEnvelopeStatePath)
$bondedCrucibleSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$expressiveDeformationReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.expressiveDeformationReceiptStatePath)
$mutualIntelligibilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessStatePath)
$inquiryPatternContinuityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquiryPatternContinuityLedgerStatePath)
$questioningBoundaryPairLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningBoundaryPairLedgerStatePath)
$carryForwardInquirySelectionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.carryForwardInquirySelectionSurfaceStatePath)
$engramDistanceClassificationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerStatePath)
$engramPromotionRequirementsMatrixStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramPromotionRequirementsMatrixStatePath)
$distanceWeightedQuestioningAdmissionSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.distanceWeightedQuestioningAdmissionSurfaceStatePath)
$questioningOperatorCandidateLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningOperatorCandidateLedgerStatePath)
$questioningGelPromotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningGelPromotionGateStatePath)
$protectedQuestioningPatternSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedQuestioningPatternSurfaceStatePath)
$variationTestedReentryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.variationTestedReentryLedgerStatePath)
$questioningAdmissionRefusalReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.questioningAdmissionRefusalReceiptStatePath)
$promotionSeductionWatchStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionSeductionWatchStatePath)
$engramIntentFieldLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$warmReactivationDispositionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptStatePath)
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$warmClockDispositionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionStatePath)
$ripeningStalenessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerStatePath)
$coolingPressureWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coolingPressureWitnessStatePath)
$hotReactivationTriggerReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.hotReactivationTriggerReceiptStatePath)
$coldAdmissionEligibilityGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateStatePath)
$archiveDispositionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.archiveDispositionLedgerStatePath)
$interlockDensityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.interlockDensityLedgerStatePath)
$brittleDurableDifferentiationSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittleDurableDifferentiationSurfaceStatePath)
$coreInvariantLatticeWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coreInvariantLatticeWitnessStatePath)
$retentionState = Read-JsonFileOrNull -Path $retentionStatePath
$blockedEscalationState = Read-JsonFileOrNull -Path $blockedEscalationStatePath
$notificationState = Read-JsonFileOrNull -Path $notificationStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath
$cmeFormationAndOfficeLedgerState = Read-JsonFileOrNull -Path $cmeFormationAndOfficeLedgerStatePath
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
$watchdogState = Read-JsonFileOrNull -Path $watchdogStatePath
$unattendedIntervalConcordanceState = Read-JsonFileOrNull -Path $unattendedIntervalConcordanceStatePath
$staleSurfaceContradictionWatchState = Read-JsonFileOrNull -Path $staleSurfaceContradictionWatchStatePath
$unattendedProofCollapseState = Read-JsonFileOrNull -Path $unattendedProofCollapseStatePath
$dormantWindowLedgerState = Read-JsonFileOrNull -Path $dormantWindowLedgerStatePath
$silentCadenceIntegrityState = Read-JsonFileOrNull -Path $silentCadenceIntegrityStatePath
$longFormPhaseWitnessState = Read-JsonFileOrNull -Path $longFormPhaseWitnessStatePath
$longFormWindowBoundaryState = Read-JsonFileOrNull -Path $longFormWindowBoundaryStatePath
$autonomousLongFormCollapseState = Read-JsonFileOrNull -Path $autonomousLongFormCollapseStatePath
$schedulerProofHarvestState = Read-JsonFileOrNull -Path $schedulerProofHarvestStatePath
$intervalOriginClarificationState = Read-JsonFileOrNull -Path $intervalOriginClarificationStatePath
$queuedTaskMapPromotionState = Read-JsonFileOrNull -Path $queuedTaskMapPromotionStatePath
$masterThreadOrchestrationState = Read-JsonFileOrNull -Path $masterThreadOrchestrationStatePath
$runtimeDeployabilityEnvelopeState = Read-JsonFileOrNull -Path $runtimeDeployabilityEnvelopeStatePath
$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$reachAccessTopologyLedgerState = Read-JsonFileOrNull -Path $reachAccessTopologyLedgerStatePath
$bondedOperatorLocalityReadinessState = Read-JsonFileOrNull -Path $bondedOperatorLocalityReadinessStatePath
$protectedStateLegibilitySurfaceState = Read-JsonFileOrNull -Path $protectedStateLegibilitySurfaceStatePath
$nexusSingularPortalFacadeState = Read-JsonFileOrNull -Path $nexusSingularPortalFacadeStatePath
$duplexPredicateEnvelopeState = Read-JsonFileOrNull -Path $duplexPredicateEnvelopeStatePath
$operatorActualWorkSessionRehearsalState = Read-JsonFileOrNull -Path $operatorActualWorkSessionRehearsalStatePath
$identityInvariantThreadRootState = Read-JsonFileOrNull -Path $identityInvariantThreadRootStatePath
$governedThreadBirthReceiptState = Read-JsonFileOrNull -Path $governedThreadBirthReceiptStatePath
$interWorkerBraidHandoffPacketState = Read-JsonFileOrNull -Path $interWorkerBraidHandoffPacketStatePath
$agentiCoreActualUtilitySurfaceState = Read-JsonFileOrNull -Path $agentiCoreActualUtilitySurfaceStatePath
$reachDuplexRealizationSeamState = Read-JsonFileOrNull -Path $reachDuplexRealizationSeamStatePath
$bondedParticipationLocalityLedgerState = Read-JsonFileOrNull -Path $bondedParticipationLocalityLedgerStatePath
$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$amenableDayDreamTierAdmissibilityState = Read-JsonFileOrNull -Path $amenableDayDreamTierAdmissibilityStatePath
$selfRootedCrypticDepthGateState = Read-JsonFileOrNull -Path $selfRootedCrypticDepthGateStatePath
$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$runIsolatedBuildPathwayState = Read-JsonFileOrNull -Path $runIsolatedBuildPathwayStatePath
$dayDreamCollapseReceiptState = Read-JsonFileOrNull -Path $dayDreamCollapseReceiptStatePath
$crypticDepthReturnReceiptState = Read-JsonFileOrNull -Path $crypticDepthReturnReceiptStatePath
$bondedCoWorkSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCoWorkSessionRehearsalStatePath
$reachReturnDissolutionReceiptState = Read-JsonFileOrNull -Path $reachReturnDissolutionReceiptStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath
$localHostSanctuaryResidencyEnvelopeState = Read-JsonFileOrNull -Path $localHostSanctuaryResidencyEnvelopeStatePath
$runtimeHabitationReadinessLedgerState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$boundedInhabitationLaunchRehearsalState = Read-JsonFileOrNull -Path $boundedInhabitationLaunchRehearsalStatePath
$postHabitationHorizonLatticeState = Read-JsonFileOrNull -Path $postHabitationHorizonLatticeStatePath
$boundedHorizonResearchBriefState = Read-JsonFileOrNull -Path $boundedHorizonResearchBriefStatePath
$nextEraBatchSelectorState = Read-JsonFileOrNull -Path $nextEraBatchSelectorStatePath
$inquirySessionDisciplineSurfaceState = Read-JsonFileOrNull -Path $inquirySessionDisciplineSurfaceStatePath
$boundaryConditionLedgerState = Read-JsonFileOrNull -Path $boundaryConditionLedgerStatePath
$coherenceGainWitnessReceiptState = Read-JsonFileOrNull -Path $coherenceGainWitnessReceiptStatePath
$operatorInquirySelectionEnvelopeState = Read-JsonFileOrNull -Path $operatorInquirySelectionEnvelopeStatePath
$bondedCrucibleSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCrucibleSessionRehearsalStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath
$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$expressiveDeformationReceiptState = Read-JsonFileOrNull -Path $expressiveDeformationReceiptStatePath
$mutualIntelligibilityWitnessState = Read-JsonFileOrNull -Path $mutualIntelligibilityWitnessStatePath
$inquiryPatternContinuityLedgerState = Read-JsonFileOrNull -Path $inquiryPatternContinuityLedgerStatePath
$questioningBoundaryPairLedgerState = Read-JsonFileOrNull -Path $questioningBoundaryPairLedgerStatePath
$carryForwardInquirySelectionSurfaceState = Read-JsonFileOrNull -Path $carryForwardInquirySelectionSurfaceStatePath
$engramDistanceClassificationLedgerState = Read-JsonFileOrNull -Path $engramDistanceClassificationLedgerStatePath
$engramPromotionRequirementsMatrixState = Read-JsonFileOrNull -Path $engramPromotionRequirementsMatrixStatePath
$distanceWeightedQuestioningAdmissionSurfaceState = Read-JsonFileOrNull -Path $distanceWeightedQuestioningAdmissionSurfaceStatePath
$questioningOperatorCandidateLedgerState = Read-JsonFileOrNull -Path $questioningOperatorCandidateLedgerStatePath
$questioningGelPromotionGateState = Read-JsonFileOrNull -Path $questioningGelPromotionGateStatePath
$protectedQuestioningPatternSurfaceState = Read-JsonFileOrNull -Path $protectedQuestioningPatternSurfaceStatePath
$variationTestedReentryLedgerState = Read-JsonFileOrNull -Path $variationTestedReentryLedgerStatePath
$questioningAdmissionRefusalReceiptState = Read-JsonFileOrNull -Path $questioningAdmissionRefusalReceiptStatePath
$promotionSeductionWatchState = Read-JsonFileOrNull -Path $promotionSeductionWatchStatePath
$engramIntentFieldLedgerState = Read-JsonFileOrNull -Path $engramIntentFieldLedgerStatePath
$intentConstraintAlignmentReceiptState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath
$warmReactivationDispositionReceiptState = Read-JsonFileOrNull -Path $warmReactivationDispositionReceiptStatePath
$formationPhaseVectorState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$brittlenessWitnessState = Read-JsonFileOrNull -Path $brittlenessWitnessStatePath
$durabilityWitnessState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath
$warmClockDispositionState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$ripeningStalenessLedgerState = Read-JsonFileOrNull -Path $ripeningStalenessLedgerStatePath
$coolingPressureWitnessState = Read-JsonFileOrNull -Path $coolingPressureWitnessStatePath
$hotReactivationTriggerReceiptState = Read-JsonFileOrNull -Path $hotReactivationTriggerReceiptStatePath
$coldAdmissionEligibilityGateState = Read-JsonFileOrNull -Path $coldAdmissionEligibilityGateStatePath
$archiveDispositionLedgerState = Read-JsonFileOrNull -Path $archiveDispositionLedgerStatePath
$interlockDensityLedgerState = Read-JsonFileOrNull -Path $interlockDensityLedgerStatePath
$brittleDurableDifferentiationSurfaceState = Read-JsonFileOrNull -Path $brittleDurableDifferentiationSurfaceStatePath
$coreInvariantLatticeWitnessState = Read-JsonFileOrNull -Path $coreInvariantLatticeWitnessStatePath

$digestJson = $null
if (-not [string]::IsNullOrWhiteSpace($lastDigestBundle)) {
    $digestJsonPath = Join-Path $lastDigestBundle 'release-candidate-digest.json'
    if (Test-Path -LiteralPath $digestJsonPath -PathType Leaf) {
        $digestJson = Read-JsonFile -Path $digestJsonPath
    }
}

$schedulerTopology = $taskingPolicy.schedulerTaskTopology
$mainWorkerScheduler = Get-ScheduledTaskSnapshot -TaskName ([string] $schedulerTopology.mainWorkerTaskName)
$watchdogScheduler = Get-ScheduledTaskSnapshot -TaskName ([string] $schedulerTopology.watchdogTaskName)
$dailyDigestScheduler = Get-ScheduledTaskSnapshot -TaskName ([string] $schedulerTopology.dailyDigestTaskName)
$scheduler = [ordered]@{
    topology = [ordered]@{
        mainWorkerTaskName = [string] $schedulerTopology.mainWorkerTaskName
        watchdogTaskName = [string] $schedulerTopology.watchdogTaskName
        dailyDigestTaskName = [string] $schedulerTopology.dailyDigestTaskName
        mainWorkerCadenceMinutes = [int] $schedulerTopology.mainWorkerCadenceMinutes
        watchdogCadenceHours = [int] $schedulerTopology.watchdogCadenceHours
        dailyDigestCadenceHours = [int] $schedulerTopology.dailyDigestCadenceHours
    }
    mainWorker = $mainWorkerScheduler
    watchdog = $watchdogScheduler
    dailyDigest = $dailyDigestScheduler
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

$mainWorkerTaskStatus = switch ($mainWorkerTerminalState) {
    'pause-hitl' { 'paused-hitl' }
    'done' { 'done-retired' }
    'fault-recoverable' { 'fault-recoverable' }
    default {
        switch ($mainWorkerArmState) {
            'armed' { 'healthy-armed' }
            'awaiting-rearm' { 'healthy-awaiting-rearm' }
            'paused-hitl' { 'paused-hitl' }
            'done-retired' { 'done-retired' }
            'fault-recoverable' { 'fault-recoverable' }
            'drift-detected' { 'drift-detected' }
            default { Resolve-OfficeSchedulerStatus -Snapshot $mainWorkerScheduler -DesiredStatusWhenReady 'healthy-armed' -AllowAwaitingRearm $true }
        }
    }
}

$watchdogTaskStatus = if ($null -ne $watchdogState) {
    [string] $watchdogState.watchdogState
} else {
    Resolve-OfficeSchedulerStatus -Snapshot $watchdogScheduler -DesiredStatusWhenReady 'healthy-armed'
}

$dailyDigestTaskStatus = if (-not [bool] $dailyDigestScheduler.registered) {
    'scheduler-unregistered'
} elseif ([string]::IsNullOrWhiteSpace([string] $dailyDigestScheduler.nextRunTimeUtc)) {
    'healthy-awaiting-rearm'
} else {
    'healthy-armed'
}

$taskEntries = @(
    foreach ($taskDefinition in $taskDefinitions) {
        $taskId = [string] $taskDefinition.id
        $status = 'uninitialized'
        $lastRunUtc = $null
        $nextRunUtc = $null
        $latestBundle = $null

        switch ($taskId) {
            'main-worker-cycle' {
                $status = $mainWorkerTaskStatus
                $lastRunUtc = if ($null -ne $lastMainWorkerCloseUtc) { $lastMainWorkerCloseUtc.ToString('o') } elseif ($mainWorkerScheduler.lastRunTimeUtc) { [string] $mainWorkerScheduler.lastRunTimeUtc } else { $null }
                $nextRunUtc = if ($null -ne $nextMainWorkerWakeUtc) { $nextMainWorkerWakeUtc.ToString('o') } elseif ($mainWorkerScheduler.nextRunTimeUtc) { [string] $mainWorkerScheduler.nextRunTimeUtc } else { $null }
                $latestBundle = $lastReleaseCandidateBundle
            }
            'hourly-watchdog' {
                $status = $watchdogTaskStatus
                $lastRunUtc = if ($watchdogScheduler.lastRunTimeUtc) { [string] $watchdogScheduler.lastRunTimeUtc } else { $null }
                $nextRunUtc = if ($null -ne $nextWatchdogRunUtc) { $nextWatchdogRunUtc.ToString('o') } elseif ($watchdogScheduler.nextRunTimeUtc) { [string] $watchdogScheduler.nextRunTimeUtc } else { $null }
                $latestBundle = if ($null -ne $watchdogState) { [string] $watchdogState.bundlePath } else { [string] $watchdogScheduler.taskName }
            }
            'daily-hitl-digest' {
                $status = $dailyDigestTaskStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextDailyHitlDigestRunUtc) { $nextDailyHitlDigestRunUtc.ToString('o') } elseif ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } elseif ($dailyDigestScheduler.nextRunTimeUtc) { [string] $dailyDigestScheduler.nextRunTimeUtc } else { $null }
                $latestBundle = $lastDigestBundle
            }
            'promotion-watch' {
                $status = $promotionWatchStatus
                $lastRunUtc = if ($null -ne $lastDigestUtc) { $lastDigestUtc.ToString('o') } else { $null }
                $nextRunUtc = if ($null -ne $nextDailyHitlDigestRunUtc) { $nextDailyHitlDigestRunUtc.ToString('o') } elseif ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
                $latestBundle = $lastDigestBundle
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
$queuedBatchTaskMaps = @()

if ($null -ne $activeLongFormTaskMap) {
    $activeLongFormTasks = @($activeLongFormTaskMap.tasks)
    $activeLongFormTasksTotal = $activeLongFormTasks.Count
    $activeLongFormTaskStatuses = @(
        $activeLongFormTasks |
        ForEach-Object {
            $taskLiveStatus = Resolve-LongFormTaskLiveStatus `
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
                -UnattendedProofCollapseState $unattendedProofCollapseState `
                -DormantWindowLedgerState $dormantWindowLedgerState `
                -SilentCadenceIntegrityState $silentCadenceIntegrityState `
                -LongFormPhaseWitnessState $longFormPhaseWitnessState `
                -LongFormWindowBoundaryState $longFormWindowBoundaryState `
                -AutonomousLongFormCollapseState $autonomousLongFormCollapseState `
                -SchedulerProofHarvestState $schedulerProofHarvestState `
                -IntervalOriginClarificationState $intervalOriginClarificationState `
                -QueuedTaskMapPromotionState $queuedTaskMapPromotionState `
                -RuntimeDeployabilityEnvelopeState $runtimeDeployabilityEnvelopeState `
                -SanctuaryRuntimeReadinessState $sanctuaryRuntimeReadinessState `
                -RuntimeWorkSurfaceAdmissibilityState $runtimeWorkSurfaceAdmissibilityState `
                -ReachAccessTopologyLedgerState $reachAccessTopologyLedgerState `
                -BondedOperatorLocalityReadinessState $bondedOperatorLocalityReadinessState `
                -ProtectedStateLegibilitySurfaceState $protectedStateLegibilitySurfaceState `
                -NexusSingularPortalFacadeState $nexusSingularPortalFacadeState `
                -DuplexPredicateEnvelopeState $duplexPredicateEnvelopeState `
                -OperatorActualWorkSessionRehearsalState $operatorActualWorkSessionRehearsalState `
                -IdentityInvariantThreadRootState $identityInvariantThreadRootState `
                -GovernedThreadBirthReceiptState $governedThreadBirthReceiptState `
                -InterWorkerBraidHandoffPacketState $interWorkerBraidHandoffPacketState `
                -AgentiCoreActualUtilitySurfaceState $agentiCoreActualUtilitySurfaceState `
                -ReachDuplexRealizationSeamState $reachDuplexRealizationSeamState `
                -BondedParticipationLocalityLedgerState $bondedParticipationLocalityLedgerState `
                -SanctuaryRuntimeWorkbenchSurfaceState $sanctuaryRuntimeWorkbenchSurfaceState `
                -AmenableDayDreamTierAdmissibilityState $amenableDayDreamTierAdmissibilityState `
                -SelfRootedCrypticDepthGateState $selfRootedCrypticDepthGateState `
                -RuntimeWorkbenchSessionLedgerState $runtimeWorkbenchSessionLedgerState `
                -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState `
                -DayDreamCollapseReceiptState $dayDreamCollapseReceiptState `
                -CrypticDepthReturnReceiptState $crypticDepthReturnReceiptState `
                -BondedCoWorkSessionRehearsalState $bondedCoWorkSessionRehearsalState `
                -ReachReturnDissolutionReceiptState $reachReturnDissolutionReceiptState `
                -LocalityDistinctionWitnessLedgerState $localityDistinctionWitnessLedgerState `
                -LocalHostSanctuaryResidencyEnvelopeState $localHostSanctuaryResidencyEnvelopeState `
                -RuntimeHabitationReadinessLedgerState $runtimeHabitationReadinessLedgerState `
                -BoundedInhabitationLaunchRehearsalState $boundedInhabitationLaunchRehearsalState `
                -PostHabitationHorizonLatticeState $postHabitationHorizonLatticeState `
                -BoundedHorizonResearchBriefState $boundedHorizonResearchBriefState `
                -NextEraBatchSelectorState $nextEraBatchSelectorState `
                -InquirySessionDisciplineSurfaceState $inquirySessionDisciplineSurfaceState `
                -BoundaryConditionLedgerState $boundaryConditionLedgerState `
                -CoherenceGainWitnessReceiptState $coherenceGainWitnessReceiptState `
                -OperatorInquirySelectionEnvelopeState $operatorInquirySelectionEnvelopeState `
                -BondedCrucibleSessionRehearsalState $bondedCrucibleSessionRehearsalState `
                -SharedBoundaryMemoryLedgerState $sharedBoundaryMemoryLedgerState `
                -ContinuityUnderPressureLedgerState $continuityUnderPressureLedgerState `
                -ExpressiveDeformationReceiptState $expressiveDeformationReceiptState `
                -MutualIntelligibilityWitnessState $mutualIntelligibilityWitnessState `
                -InquiryPatternContinuityLedgerState $inquiryPatternContinuityLedgerState `
                -QuestioningBoundaryPairLedgerState $questioningBoundaryPairLedgerState `
                -CarryForwardInquirySelectionSurfaceState $carryForwardInquirySelectionSurfaceState `
                -EngramDistanceClassificationLedgerState $engramDistanceClassificationLedgerState `
                -EngramPromotionRequirementsMatrixState $engramPromotionRequirementsMatrixState `
                -DistanceWeightedQuestioningAdmissionSurfaceState $distanceWeightedQuestioningAdmissionSurfaceState `
                -QuestioningOperatorCandidateLedgerState $questioningOperatorCandidateLedgerState `
                -QuestioningGelPromotionGateState $questioningGelPromotionGateState `
                -ProtectedQuestioningPatternSurfaceState $protectedQuestioningPatternSurfaceState `
                -VariationTestedReentryLedgerState $variationTestedReentryLedgerState `
                -QuestioningAdmissionRefusalReceiptState $questioningAdmissionRefusalReceiptState `
                -PromotionSeductionWatchState $promotionSeductionWatchState `
                -EngramIntentFieldLedgerState $engramIntentFieldLedgerState `
                -IntentConstraintAlignmentReceiptState $intentConstraintAlignmentReceiptState `
                -WarmReactivationDispositionReceiptState $warmReactivationDispositionReceiptState `
                -FormationPhaseVectorState $formationPhaseVectorState `
                -BrittlenessWitnessState $brittlenessWitnessState `
                -DurabilityWitnessState $durabilityWitnessState `
                -WarmClockDispositionState $warmClockDispositionState `
                -RipeningStalenessLedgerState $ripeningStalenessLedgerState `
                -CoolingPressureWitnessState $coolingPressureWitnessState `
                -HotReactivationTriggerReceiptState $hotReactivationTriggerReceiptState `
                -ColdAdmissionEligibilityGateState $coldAdmissionEligibilityGateState `
                -ArchiveDispositionLedgerState $archiveDispositionLedgerState `
                -InterlockDensityLedgerState $interlockDensityLedgerState `
                -BrittleDurableDifferentiationSurfaceState $brittleDurableDifferentiationSurfaceState `
                -CoreInvariantLatticeWitnessState $coreInvariantLatticeWitnessState `
                -LastKnownStatus $lastKnownStatus `
                -BlockedStatus ([string] $cyclePolicy.blockedStatus)

            Resolve-RunIsolatedBuildTaskLiveStatus `
                -TaskId ([string] $_.id) `
                -CurrentLiveStatus $taskLiveStatus `
                -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState
        }
    )
    $activeLongFormTasksCompleted = @($activeLongFormTaskStatuses | Where-Object { $_ -eq 'completed' }).Count

    $activeRunCollapsed = $true
    if ($null -ne $activeLongFormRun) {
        $activeRunCollapsed = @('collapsed', 'completed') -contains ([string] $activeLongFormRun.runStatus)
    }

    if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
        $activeLongFormTaskMapStatus = 'blocked'
    } elseif ($requiresImmediateHitl -or $lastKnownStatus -eq 'hitl-required') {
        $activeLongFormTaskMapStatus = 'waiting-for-hitl'
    } elseif ($activeLongFormTasksTotal -gt 0 -and $activeLongFormTasksCompleted -ge $activeLongFormTasksTotal -and $activeRunCollapsed) {
        $activeLongFormTaskMapStatus = 'completed'
    } else {
        $activeLongFormTaskMapStatus = 'in-progress'
    }

    $taskMapIndex = [array]::IndexOf($longFormTaskMaps, $activeLongFormTaskMap)
    if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $longFormTaskMaps.Count) {
        $eligibleNextTaskMap = $longFormTaskMaps[$taskMapIndex + 1]
    }
    $queuedBatchTaskMaps = @()
    if ($taskMapIndex -ge 0 -and ($taskMapIndex + 1) -lt $longFormTaskMaps.Count) {
        $queuedBatchTaskMaps = @($longFormTaskMaps | Select-Object -Skip ($taskMapIndex + 1) -First 3)
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
        $activeRunCollapsed -and
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
                    -UnattendedProofCollapseState $unattendedProofCollapseState `
                    -DormantWindowLedgerState $dormantWindowLedgerState `
                    -SilentCadenceIntegrityState $silentCadenceIntegrityState `
                    -LongFormPhaseWitnessState $longFormPhaseWitnessState `
                    -LongFormWindowBoundaryState $longFormWindowBoundaryState `
                    -AutonomousLongFormCollapseState $autonomousLongFormCollapseState `
                    -SchedulerProofHarvestState $schedulerProofHarvestState `
                    -IntervalOriginClarificationState $intervalOriginClarificationState `
                    -QueuedTaskMapPromotionState $queuedTaskMapPromotionState `
                    -RuntimeDeployabilityEnvelopeState $runtimeDeployabilityEnvelopeState `
                    -SanctuaryRuntimeReadinessState $sanctuaryRuntimeReadinessState `
                    -RuntimeWorkSurfaceAdmissibilityState $runtimeWorkSurfaceAdmissibilityState `
                    -ReachAccessTopologyLedgerState $reachAccessTopologyLedgerState `
                    -BondedOperatorLocalityReadinessState $bondedOperatorLocalityReadinessState `
                    -ProtectedStateLegibilitySurfaceState $protectedStateLegibilitySurfaceState `
                    -NexusSingularPortalFacadeState $nexusSingularPortalFacadeState `
                    -DuplexPredicateEnvelopeState $duplexPredicateEnvelopeState `
                    -OperatorActualWorkSessionRehearsalState $operatorActualWorkSessionRehearsalState `
                    -IdentityInvariantThreadRootState $identityInvariantThreadRootState `
                    -GovernedThreadBirthReceiptState $governedThreadBirthReceiptState `
                    -InterWorkerBraidHandoffPacketState $interWorkerBraidHandoffPacketState `
                    -AgentiCoreActualUtilitySurfaceState $agentiCoreActualUtilitySurfaceState `
                    -ReachDuplexRealizationSeamState $reachDuplexRealizationSeamState `
                    -BondedParticipationLocalityLedgerState $bondedParticipationLocalityLedgerState `
                    -SanctuaryRuntimeWorkbenchSurfaceState $sanctuaryRuntimeWorkbenchSurfaceState `
                    -AmenableDayDreamTierAdmissibilityState $amenableDayDreamTierAdmissibilityState `
                    -SelfRootedCrypticDepthGateState $selfRootedCrypticDepthGateState `
                    -RuntimeWorkbenchSessionLedgerState $runtimeWorkbenchSessionLedgerState `
                    -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState `
                    -DayDreamCollapseReceiptState $dayDreamCollapseReceiptState `
                    -CrypticDepthReturnReceiptState $crypticDepthReturnReceiptState `
                    -BondedCoWorkSessionRehearsalState $bondedCoWorkSessionRehearsalState `
                    -ReachReturnDissolutionReceiptState $reachReturnDissolutionReceiptState `
                    -LocalityDistinctionWitnessLedgerState $localityDistinctionWitnessLedgerState `
                    -LocalHostSanctuaryResidencyEnvelopeState $localHostSanctuaryResidencyEnvelopeState `
                    -RuntimeHabitationReadinessLedgerState $runtimeHabitationReadinessLedgerState `
                    -BoundedInhabitationLaunchRehearsalState $boundedInhabitationLaunchRehearsalState `
                    -PostHabitationHorizonLatticeState $postHabitationHorizonLatticeState `
                    -BoundedHorizonResearchBriefState $boundedHorizonResearchBriefState `
                    -NextEraBatchSelectorState $nextEraBatchSelectorState `
                    -InquirySessionDisciplineSurfaceState $inquirySessionDisciplineSurfaceState `
                    -BoundaryConditionLedgerState $boundaryConditionLedgerState `
                    -CoherenceGainWitnessReceiptState $coherenceGainWitnessReceiptState `
                    -OperatorInquirySelectionEnvelopeState $operatorInquirySelectionEnvelopeState `
                    -BondedCrucibleSessionRehearsalState $bondedCrucibleSessionRehearsalState `
                    -SharedBoundaryMemoryLedgerState $sharedBoundaryMemoryLedgerState `
                    -ContinuityUnderPressureLedgerState $continuityUnderPressureLedgerState `
                    -ExpressiveDeformationReceiptState $expressiveDeformationReceiptState `
                    -MutualIntelligibilityWitnessState $mutualIntelligibilityWitnessState `
                    -InquiryPatternContinuityLedgerState $inquiryPatternContinuityLedgerState `
                    -QuestioningBoundaryPairLedgerState $questioningBoundaryPairLedgerState `
                    -CarryForwardInquirySelectionSurfaceState $carryForwardInquirySelectionSurfaceState `
                    -EngramDistanceClassificationLedgerState $engramDistanceClassificationLedgerState `
                    -EngramPromotionRequirementsMatrixState $engramPromotionRequirementsMatrixState `
                    -DistanceWeightedQuestioningAdmissionSurfaceState $distanceWeightedQuestioningAdmissionSurfaceState `
                    -QuestioningOperatorCandidateLedgerState $questioningOperatorCandidateLedgerState `
                    -QuestioningGelPromotionGateState $questioningGelPromotionGateState `
                    -ProtectedQuestioningPatternSurfaceState $protectedQuestioningPatternSurfaceState `
                    -VariationTestedReentryLedgerState $variationTestedReentryLedgerState `
                    -QuestioningAdmissionRefusalReceiptState $questioningAdmissionRefusalReceiptState `
                    -PromotionSeductionWatchState $promotionSeductionWatchState `
                    -EngramIntentFieldLedgerState $engramIntentFieldLedgerState `
                    -IntentConstraintAlignmentReceiptState $intentConstraintAlignmentReceiptState `
                    -WarmReactivationDispositionReceiptState $warmReactivationDispositionReceiptState `
                    -FormationPhaseVectorState $formationPhaseVectorState `
                    -BrittlenessWitnessState $brittlenessWitnessState `
                    -DurabilityWitnessState $durabilityWitnessState `
                    -WarmClockDispositionState $warmClockDispositionState `
                    -RipeningStalenessLedgerState $ripeningStalenessLedgerState `
                    -CoolingPressureWitnessState $coolingPressureWitnessState `
                    -HotReactivationTriggerReceiptState $hotReactivationTriggerReceiptState `
                    -ColdAdmissionEligibilityGateState $coldAdmissionEligibilityGateState `
                    -ArchiveDispositionLedgerState $archiveDispositionLedgerState `
                    -InterlockDensityLedgerState $interlockDensityLedgerState `
                    -BrittleDurableDifferentiationSurfaceState $brittleDurableDifferentiationSurfaceState `
                    -CoreInvariantLatticeWitnessState $coreInvariantLatticeWitnessState `
                    -LastKnownStatus $lastKnownStatus `
                    -BlockedStatus ([string] $cyclePolicy.blockedStatus)

                $liveStatus = Resolve-RunIsolatedBuildTaskLiveStatus `
                    -TaskId ([string] $_.id) `
                    -CurrentLiveStatus $liveStatus `
                    -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState

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
        queuedBatchTaskMapIds = @($queuedBatchTaskMaps | ForEach-Object { [string] $_.id })
        queuedBatchTaskMapLabels = @($queuedBatchTaskMaps | ForEach-Object { [string] $_.label })
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
        actionClass = $currentActionClass
        mainWorkerTerminalState = $mainWorkerTerminalState
        mainWorkerArmState = $mainWorkerArmState
        lastMainWorkerCloseUtc = if ($null -ne $lastMainWorkerCloseUtc) { $lastMainWorkerCloseUtc.ToString('o') } else { $null }
        lastMainWorkerCloseDisposition = $lastMainWorkerCloseDisposition
        nextMainWorkerWakeUtc = if ($null -ne $nextMainWorkerWakeUtc) { $nextMainWorkerWakeUtc.ToString('o') } else { $null }
        nextWatchdogRunUtc = if ($null -ne $nextWatchdogRunUtc) { $nextWatchdogRunUtc.ToString('o') } else { $null }
        nextDailyHitlDigestRunUtc = if ($null -ne $nextDailyHitlDigestRunUtc) { $nextDailyHitlDigestRunUtc.ToString('o') } else { $null }
        recommendedAction = $recommendedAction
        requiresImmediateHitl = $requiresImmediateHitl
        lastReleaseCandidateBundle = $lastReleaseCandidateBundle
        lastDigestBundle = $lastDigestBundle
        dopingHeaderStatePath = $dopingHeaderStatePathFromCycle
        cycleReceiptStatePath = $cycleReceiptStatePathFromCycle
        readinessNoticeStatePath = $readinessNoticeStatePathFromCycle
        pauseNoticeStatePath = $pauseNoticeStatePathFromCycle
        currentNoticeType = $currentNoticeTypeFromCycle
        currentNoticeStatus = $currentNoticeStatusFromCycle
        seededGovernanceDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { $null }
        seededGovernanceReason = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.dispositionReason } else { $null }
        seededGovernanceProvenance = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.provenance } else { $null }
        seededGovernanceReadyState = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyState') } else { $null }
        seededGovernanceReadyReason = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyReasonCode') } else { $null }
        seededGovernanceReadyAction = if ($null -ne $seededGovernanceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'readyActionTaken') } else { $null }
        notificationTriggered = if ($null -ne $notificationState) { [bool] $notificationState.triggered } else { $null }
        notificationTriggerReason = if ($null -ne $notificationState) { [string] $notificationState.triggerReason } else { $null }
        lastNotificationBundle = if ($null -ne $notificationState) { [string] $notificationState.lastNotificationBundle } else { $null }
        schedulerReconciliationAction = if ($null -ne $schedulerReconciliationState) { @($schedulerReconciliationState.actionTaken | ForEach-Object { [string] $_ }) } else { @() }
        schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $null }
        cmeConsolidationState = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.consolidationState } else { $null }
        cmeConsolidationReason = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.reasonCode } else { $null }
        cmeFormationAndOfficeLedgerState = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.ledgerState } else { $null }
        cmeFormationAndOfficeLedgerReason = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.reasonCode } else { $null }
        cmeFormationAndOfficeLedgerNextAction = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.nextAction } else { $null }
        cmeCapabilityLedgerState = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.capabilityLedgerState } else { $null }
        cmeFormationLedgerState = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.formationLedgerState } else { $null }
        cmeOfficeLedgerState = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.officeLedgerState } else { $null }
        cmeCareerContinuityLedgerState = if ($null -ne $cmeFormationAndOfficeLedgerState) { [string] $cmeFormationAndOfficeLedgerState.careerContinuityLedgerState } else { $null }
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
        unattendedProofCollapseState = if ($null -ne $unattendedProofCollapseState) { [string] $unattendedProofCollapseState.collapseState } else { $null }
        unattendedProofCollapseReason = if ($null -ne $unattendedProofCollapseState) { [string] $unattendedProofCollapseState.reasonCode } else { $null }
        unattendedProofCollapseNextAction = if ($null -ne $unattendedProofCollapseState) { [string] $unattendedProofCollapseState.nextAction } else { $null }
        dormantWindowLedgerState = if ($null -ne $dormantWindowLedgerState) { [string] $dormantWindowLedgerState.ledgerState } else { $null }
        dormantWindowLedgerReason = if ($null -ne $dormantWindowLedgerState) { [string] $dormantWindowLedgerState.reasonCode } else { $null }
        dormantWindowLedgerNextAction = if ($null -ne $dormantWindowLedgerState) { [string] $dormantWindowLedgerState.nextAction } else { $null }
        dormantWindowLedgerCount = if ($null -ne $dormantWindowLedgerState) { [int] $dormantWindowLedgerState.consecutiveDormantWindows } else { $null }
        silentCadenceIntegrityState = if ($null -ne $silentCadenceIntegrityState) { [string] $silentCadenceIntegrityState.integrityState } else { $null }
        silentCadenceIntegrityReason = if ($null -ne $silentCadenceIntegrityState) { [string] $silentCadenceIntegrityState.reasonCode } else { $null }
        silentCadenceIntegrityNextAction = if ($null -ne $silentCadenceIntegrityState) { [string] $silentCadenceIntegrityState.nextAction } else { $null }
        longFormPhaseWitnessState = if ($null -ne $longFormPhaseWitnessState) { [string] $longFormPhaseWitnessState.witnessState } else { $null }
        longFormPhaseWitnessReason = if ($null -ne $longFormPhaseWitnessState) { [string] $longFormPhaseWitnessState.reasonCode } else { $null }
        longFormPhaseWitnessNextAction = if ($null -ne $longFormPhaseWitnessState) { [string] $longFormPhaseWitnessState.nextAction } else { $null }
        longFormWindowBoundaryState = if ($null -ne $longFormWindowBoundaryState) { [string] $longFormWindowBoundaryState.boundaryState } else { $null }
        longFormWindowBoundaryReason = if ($null -ne $longFormWindowBoundaryState) { [string] $longFormWindowBoundaryState.reasonCode } else { $null }
        longFormWindowBoundaryNextAction = if ($null -ne $longFormWindowBoundaryState) { [string] $longFormWindowBoundaryState.nextAction } else { $null }
        autonomousLongFormCollapseState = if ($null -ne $autonomousLongFormCollapseState) { [string] $autonomousLongFormCollapseState.collapseState } else { $null }
        autonomousLongFormCollapseReason = if ($null -ne $autonomousLongFormCollapseState) { [string] $autonomousLongFormCollapseState.reasonCode } else { $null }
        autonomousLongFormCollapseNextAction = if ($null -ne $autonomousLongFormCollapseState) { [string] $autonomousLongFormCollapseState.nextAction } else { $null }
        schedulerProofHarvestState = if ($null -ne $schedulerProofHarvestState) { [string] $schedulerProofHarvestState.harvestState } else { $null }
        schedulerProofHarvestReason = if ($null -ne $schedulerProofHarvestState) { [string] $schedulerProofHarvestState.reasonCode } else { $null }
        schedulerProofHarvestNextAction = if ($null -ne $schedulerProofHarvestState) { [string] $schedulerProofHarvestState.nextAction } else { $null }
        intervalOriginClarificationState = if ($null -ne $intervalOriginClarificationState) { [string] $intervalOriginClarificationState.originState } else { $null }
        intervalOriginClarificationReason = if ($null -ne $intervalOriginClarificationState) { [string] $intervalOriginClarificationState.reasonCode } else { $null }
        intervalOriginClarificationNextAction = if ($null -ne $intervalOriginClarificationState) { [string] $intervalOriginClarificationState.nextAction } else { $null }
        queuedTaskMapPromotionState = if ($null -ne $queuedTaskMapPromotionState) { [string] $queuedTaskMapPromotionState.promotionState } else { $null }
        queuedTaskMapPromotionReason = if ($null -ne $queuedTaskMapPromotionState) { [string] $queuedTaskMapPromotionState.reasonCode } else { $null }
        queuedTaskMapPromotionNextAction = if ($null -ne $queuedTaskMapPromotionState) { [string] $queuedTaskMapPromotionState.nextAction } else { $null }
        runtimeDeployabilityEnvelopeState = if ($null -ne $runtimeDeployabilityEnvelopeState) { [string] $runtimeDeployabilityEnvelopeState.envelopeState } else { $null }
        runtimeDeployabilityEnvelopeReason = if ($null -ne $runtimeDeployabilityEnvelopeState) { [string] $runtimeDeployabilityEnvelopeState.reasonCode } else { $null }
        runtimeDeployabilityEnvelopeNextAction = if ($null -ne $runtimeDeployabilityEnvelopeState) { [string] $runtimeDeployabilityEnvelopeState.nextAction } else { $null }
        sanctuaryRuntimeReadinessState = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.readinessState } else { $null }
        sanctuaryRuntimeReadinessReason = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.reasonCode } else { $null }
        sanctuaryRuntimeReadinessNextAction = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.nextAction } else { $null }
        runtimeWorkSurfaceAdmissibilityState = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.admissibilityState } else { $null }
        runtimeWorkSurfaceAdmissibilityReason = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.reasonCode } else { $null }
        runtimeWorkSurfaceAdmissibilityNextAction = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.nextAction } else { $null }
        runtimeWorkAdmissibleSurfaceCount = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [int] $runtimeWorkSurfaceAdmissibilityState.admissibleSurfaceCount } else { $null }
        runtimeWorkDeniedSurfaceCount = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [int] $runtimeWorkSurfaceAdmissibilityState.deniedSurfaceCount } else { $null }
        reachAccessTopologyLedgerState = if ($null -ne $reachAccessTopologyLedgerState) { [string] $reachAccessTopologyLedgerState.ledgerState } else { $null }
        reachAccessTopologyLedgerReason = if ($null -ne $reachAccessTopologyLedgerState) { [string] $reachAccessTopologyLedgerState.reasonCode } else { $null }
        reachAccessTopologyLedgerNextAction = if ($null -ne $reachAccessTopologyLedgerState) { [string] $reachAccessTopologyLedgerState.nextAction } else { $null }
        reachAccessTopologyDisclosedSurfaceCount = if ($null -ne $reachAccessTopologyLedgerState) { [int] $reachAccessTopologyLedgerState.disclosedSurfaceCount } else { $null }
        bondedOperatorLocalityReadinessState = if ($null -ne $bondedOperatorLocalityReadinessState) { [string] $bondedOperatorLocalityReadinessState.readinessState } else { $null }
        bondedOperatorLocalityReadinessReason = if ($null -ne $bondedOperatorLocalityReadinessState) { [string] $bondedOperatorLocalityReadinessState.reasonCode } else { $null }
        bondedOperatorLocalityReadinessNextAction = if ($null -ne $bondedOperatorLocalityReadinessState) { [string] $bondedOperatorLocalityReadinessState.nextAction } else { $null }
        protectedStateLegibilitySurfaceState = if ($null -ne $protectedStateLegibilitySurfaceState) { [string] $protectedStateLegibilitySurfaceState.legibilityState } else { $null }
        protectedStateLegibilitySurfaceReason = if ($null -ne $protectedStateLegibilitySurfaceState) { [string] $protectedStateLegibilitySurfaceState.reasonCode } else { $null }
        protectedStateLegibilitySurfaceNextAction = if ($null -ne $protectedStateLegibilitySurfaceState) { [string] $protectedStateLegibilitySurfaceState.nextAction } else { $null }
        protectedStateLegibilityVisibleSignalCount = if ($null -ne $protectedStateLegibilitySurfaceState) { [int] $protectedStateLegibilitySurfaceState.visibleSignalCount } else { $null }
        nexusSingularPortalFacadeState = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.portalState } else { $null }
        nexusSingularPortalFacadeReason = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.reasonCode } else { $null }
        nexusSingularPortalFacadeNextAction = if ($null -ne $nexusSingularPortalFacadeState) { [string] $nexusSingularPortalFacadeState.nextAction } else { $null }
        nexusSingularPortalFacadeSourceFileCount = if ($null -ne $nexusSingularPortalFacadeState) { [int] $nexusSingularPortalFacadeState.sourceFileCount } else { $null }
        duplexPredicateEnvelopeState = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.duplexState } else { $null }
        duplexPredicateEnvelopeReason = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.reasonCode } else { $null }
        duplexPredicateEnvelopeNextAction = if ($null -ne $duplexPredicateEnvelopeState) { [string] $duplexPredicateEnvelopeState.nextAction } else { $null }
        duplexPredicateEnvelopeWorkPredicateBound = if ($null -ne $duplexPredicateEnvelopeState) { [bool] $duplexPredicateEnvelopeState.workPredicateBound } else { $null }
        duplexPredicateEnvelopeGovernancePredicateBound = if ($null -ne $duplexPredicateEnvelopeState) { [bool] $duplexPredicateEnvelopeState.governancePredicateBound } else { $null }
        operatorActualWorkSessionRehearsalState = if ($null -ne $operatorActualWorkSessionRehearsalState) { [string] $operatorActualWorkSessionRehearsalState.rehearsalState } else { $null }
        operatorActualWorkSessionRehearsalReason = if ($null -ne $operatorActualWorkSessionRehearsalState) { [string] $operatorActualWorkSessionRehearsalState.reasonCode } else { $null }
        operatorActualWorkSessionRehearsalNextAction = if ($null -ne $operatorActualWorkSessionRehearsalState) { [string] $operatorActualWorkSessionRehearsalState.nextAction } else { $null }
        operatorActualWorkSessionRehearsalCoRealizedSurfaceCount = if ($null -ne $operatorActualWorkSessionRehearsalState) { [int] $operatorActualWorkSessionRehearsalState.coRealizedSurfaceCount } else { $null }
        operatorActualWorkSessionRehearsalWithheldSurfaceCount = if ($null -ne $operatorActualWorkSessionRehearsalState) { [int] $operatorActualWorkSessionRehearsalState.withheldSurfaceCount } else { $null }
        identityInvariantThreadRootState = if ($null -ne $identityInvariantThreadRootState) { [string] $identityInvariantThreadRootState.threadRootState } else { $null }
        identityInvariantThreadRootReason = if ($null -ne $identityInvariantThreadRootState) { [string] $identityInvariantThreadRootState.reasonCode } else { $null }
        identityInvariantThreadRootNextAction = if ($null -ne $identityInvariantThreadRootState) { [string] $identityInvariantThreadRootState.nextAction } else { $null }
        identityInvariantThreadRootSourceFileCount = if ($null -ne $identityInvariantThreadRootState) { [int] $identityInvariantThreadRootState.sourceFileCount } else { $null }
        governedThreadBirthReceiptState = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.receiptState } else { $null }
        governedThreadBirthReceiptReason = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.reasonCode } else { $null }
        governedThreadBirthReceiptNextAction = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.nextAction } else { $null }
        governedThreadBirthReceiptWitnessedOfficeCount = if ($null -ne $governedThreadBirthReceiptState) { [int] $governedThreadBirthReceiptState.witnessedOfficeCount } else { $null }
        interWorkerBraidHandoffPacketState = if ($null -ne $interWorkerBraidHandoffPacketState) { [string] $interWorkerBraidHandoffPacketState.packetState } else { $null }
        interWorkerBraidHandoffPacketReason = if ($null -ne $interWorkerBraidHandoffPacketState) { [string] $interWorkerBraidHandoffPacketState.reasonCode } else { $null }
        interWorkerBraidHandoffPacketNextAction = if ($null -ne $interWorkerBraidHandoffPacketState) { [string] $interWorkerBraidHandoffPacketState.nextAction } else { $null }
        interWorkerBraidHandoffPacketIdentityInheritanceDenied = if ($null -ne $interWorkerBraidHandoffPacketState) { [bool] $interWorkerBraidHandoffPacketState.identityInheritanceDenied } else { $null }
        agentiCoreActualUtilitySurfaceState = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] $agentiCoreActualUtilitySurfaceState.utilityState } else { $null }
        agentiCoreActualUtilitySurfaceReason = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] $agentiCoreActualUtilitySurfaceState.reasonCode } else { $null }
        agentiCoreActualUtilitySurfaceNextAction = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] $agentiCoreActualUtilitySurfaceState.nextAction } else { $null }
        agentiCoreActualUtilitySurfacePosture = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'utilityPosture') } else { $null }
        reachDuplexRealizationSeamState = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.seamState } else { $null }
        reachDuplexRealizationSeamReason = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.reasonCode } else { $null }
        reachDuplexRealizationSeamNextAction = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.nextAction } else { $null }
        reachDuplexRealizationSeamGrantImplied = if ($null -ne $reachDuplexRealizationSeamState) { [bool] $reachDuplexRealizationSeamState.grantImplied } else { $null }
        bondedParticipationLocalityLedgerState = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.ledgerState } else { $null }
        bondedParticipationLocalityLedgerReason = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.reasonCode } else { $null }
        bondedParticipationLocalityLedgerNextAction = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.nextAction } else { $null }
        bondedParticipationLocalityLedgerCoRealizedSurfaceCount = if ($null -ne $bondedParticipationLocalityLedgerState) { [int] $bondedParticipationLocalityLedgerState.coRealizedSurfaceCount } else { $null }
        bondedParticipationLocalityLedgerWithheldSurfaceCount = if ($null -ne $bondedParticipationLocalityLedgerState) { [int] $bondedParticipationLocalityLedgerState.withheldSurfaceCount } else { $null }
        sanctuaryRuntimeWorkbenchSurfaceState = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.workbenchState } else { $null }
        sanctuaryRuntimeWorkbenchSurfaceReason = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.reasonCode } else { $null }
        sanctuaryRuntimeWorkbenchSurfaceNextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { $null }
        sanctuaryRuntimeWorkbenchSessionPosture = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'sessionPosture') } else { $null }
        sanctuaryRuntimeWorkbenchBoundedWorkClass = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'boundedWorkClass') } else { $null }
        amenableDayDreamTierAdmissibilityState = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.admissibilityState } else { $null }
        amenableDayDreamTierAdmissibilityReason = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.reasonCode } else { $null }
        amenableDayDreamTierAdmissibilityNextAction = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.nextAction } else { $null }
        amenableDayDreamTierExploratoryOnly = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'exploratoryOnly') } else { $null }
        amenableDayDreamTierIdentityBearingDescentDenied = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'identityBearingDescentDenied') } else { $null }
        amenableDayDreamTierContinuityInflationDenied = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'continuityInflationDenied') } else { $null }
        selfRootedCrypticDepthGateState = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.gateState } else { $null }
        selfRootedCrypticDepthGateReason = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.reasonCode } else { $null }
        selfRootedCrypticDepthGateNextAction = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.nextAction } else { $null }
        selfRootedCrypticDepthGateMode = if ($null -ne $selfRootedCrypticDepthGateState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'gateMode') } else { $null }
        selfRootedCrypticDepthGateCrypticBiadRooted = if ($null -ne $selfRootedCrypticDepthGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'crypticBiadRooted') } else { $null }
        selfRootedCrypticDepthGateSharedAmenableOriginDenied = if ($null -ne $selfRootedCrypticDepthGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'sharedAmenableOriginDenied') } else { $null }
        selfRootedCrypticDepthGateDeepAccessGranted = if ($null -ne $selfRootedCrypticDepthGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'deepAccessGranted') } else { $null }
        runtimeWorkbenchSessionLedgerState = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.sessionLedgerState } else { $null }
        runtimeWorkbenchSessionLedgerReason = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.reasonCode } else { $null }
        runtimeWorkbenchSessionLedgerNextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { $null }
        runtimeWorkbenchSessionLedgerSessionState = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionState') } else { $null }
        runtimeWorkbenchSessionLedgerAdmittedLaneCount = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'admittedLaneCount') } else { $null }
        runtimeWorkbenchSessionLedgerWithheldLaneCount = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'withheldLaneCount') } else { $null }
        runtimeWorkbenchSessionLedgerSessionEventCount = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionEventCount') } else { $null }
        runtimeWorkbenchSessionLedgerBoundaryConditionCount = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'boundaryConditionCount') } else { $null }
        dayDreamCollapseReceiptState = if ($null -ne $dayDreamCollapseReceiptState) { [string] $dayDreamCollapseReceiptState.collapseReceiptState } else { $null }
        dayDreamCollapseReceiptReason = if ($null -ne $dayDreamCollapseReceiptState) { [string] $dayDreamCollapseReceiptState.reasonCode } else { $null }
        dayDreamCollapseReceiptNextAction = if ($null -ne $dayDreamCollapseReceiptState) { [string] $dayDreamCollapseReceiptState.nextAction } else { $null }
        dayDreamCollapseState = if ($null -ne $dayDreamCollapseReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'collapseState') } else { $null }
        dayDreamCollapseConsideredPredicateCount = if ($null -ne $dayDreamCollapseReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'consideredPredicateCount') } else { $null }
        dayDreamCollapseBoundedOutputCount = if ($null -ne $dayDreamCollapseReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'boundedOutputCount') } else { $null }
        dayDreamCollapseRemainingNonFinalOutputCount = if ($null -ne $dayDreamCollapseReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'remainingNonFinalOutputCount') } else { $null }
        dayDreamCollapseExploratoryProvenancePreserved = if ($null -ne $dayDreamCollapseReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'exploratoryProvenancePreserved') } else { $null }
        dayDreamCollapseBoundaryConditionCount = if ($null -ne $dayDreamCollapseReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'boundaryConditionCount') } else { $null }
        dayDreamCollapseResidueMarkerCount = if ($null -ne $dayDreamCollapseReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'residueMarkerCount') } else { $null }
        crypticDepthReturnReceiptState = if ($null -ne $crypticDepthReturnReceiptState) { [string] $crypticDepthReturnReceiptState.returnReceiptState } else { $null }
        crypticDepthReturnReceiptReason = if ($null -ne $crypticDepthReturnReceiptState) { [string] $crypticDepthReturnReceiptState.reasonCode } else { $null }
        crypticDepthReturnReceiptNextAction = if ($null -ne $crypticDepthReturnReceiptState) { [string] $crypticDepthReturnReceiptState.nextAction } else { $null }
        crypticDepthReturnState = if ($null -ne $crypticDepthReturnReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'returnState') } else { $null }
        crypticDepthReturnContinuityMarkerCount = if ($null -ne $crypticDepthReturnReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'continuityMarkerCount') } else { $null }
        crypticDepthReturnResidueMarkerCount = if ($null -ne $crypticDepthReturnReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'residueMarkerCount') } else { $null }
        crypticDepthReturnBoundaryConditionCount = if ($null -ne $crypticDepthReturnReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'boundaryConditionCount') } else { $null }
        crypticDepthReturnReturnedCleanly = if ($null -ne $crypticDepthReturnReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'returnedCleanly') } else { $null }
        crypticDepthReturnSharedAmenableLaneClear = if ($null -ne $crypticDepthReturnReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'sharedAmenableLaneClear') } else { $null }
        crypticDepthReturnIdentityBleedDetected = if ($null -ne $crypticDepthReturnReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'identityBleedDetected') } else { $null }
        bondedCoWorkSessionRehearsalState = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.rehearsalReceiptState } else { $null }
        bondedCoWorkSessionRehearsalReason = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.reasonCode } else { $null }
        bondedCoWorkSessionRehearsalNextAction = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.nextAction } else { $null }
        bondedCoWorkSessionRehearsalPhase = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'rehearsalState') } else { $null }
        bondedCoWorkSessionRehearsalSharedWorkLoopCount = if ($null -ne $bondedCoWorkSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'sharedWorkLoopCount') } else { $null }
        bondedCoWorkSessionRehearsalDuplexPredicateLaneCount = if ($null -ne $bondedCoWorkSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'duplexPredicateLaneCount') } else { $null }
        bondedCoWorkSessionRehearsalWithheldLaneCount = if ($null -ne $bondedCoWorkSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'withheldLaneCount') } else { $null }
        bondedCoWorkSessionRehearsalRemoteControlDenied = if ($null -ne $bondedCoWorkSessionRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'remoteControlDenied') } else { $null }
        bondedCoWorkSessionRehearsalLocalityCollapseDenied = if ($null -ne $bondedCoWorkSessionRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'localityCollapseDenied') } else { $null }
        reachReturnDissolutionReceiptState = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.returnReceiptState } else { $null }
        reachReturnDissolutionReceiptReason = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.reasonCode } else { $null }
        reachReturnDissolutionReceiptNextAction = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.nextAction } else { $null }
        reachReturnDissolutionReturnState = if ($null -ne $reachReturnDissolutionReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnState') } else { $null }
        reachReturnDissolutionDissolutionState = if ($null -ne $reachReturnDissolutionReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'dissolutionState') } else { $null }
        reachReturnDissolutionBondedEventReturned = if ($null -ne $reachReturnDissolutionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'bondedEventReturned') } else { $null }
        reachReturnDissolutionBondedEventDissolved = if ($null -ne $reachReturnDissolutionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'bondedEventDissolved') } else { $null }
        reachReturnDissolutionAmbientGrantDenied = if ($null -ne $reachReturnDissolutionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'ambientGrantDenied') } else { $null }
        reachReturnDissolutionLocalityDistinctionPreserved = if ($null -ne $reachReturnDissolutionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'localityDistinctionPreserved') } else { $null }
        localityDistinctionWitnessLedgerState = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.witnessLedgerState } else { $null }
        localityDistinctionWitnessLedgerReason = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.reasonCode } else { $null }
        localityDistinctionWitnessLedgerNextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { $null }
        localityDistinctionWitnessLedgerSharedSurfaceCount = if ($null -ne $localityDistinctionWitnessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'sharedSurfaceCount') } else { $null }
        localityDistinctionWitnessLedgerSanctuaryLocalSurfaceCount = if ($null -ne $localityDistinctionWitnessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'sanctuaryLocalSurfaceCount') } else { $null }
        localityDistinctionWitnessLedgerOperatorLocalSurfaceCount = if ($null -ne $localityDistinctionWitnessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'operatorLocalSurfaceCount') } else { $null }
        localityDistinctionWitnessLedgerWithheldSurfaceCount = if ($null -ne $localityDistinctionWitnessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'withheldSurfaceCount') } else { $null }
        localityDistinctionWitnessLedgerLocalityCollapseDetected = if ($null -ne $localityDistinctionWitnessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'localityCollapseDetected') } else { $null }
        localityDistinctionWitnessLedgerProjectionTheaterDenied = if ($null -ne $localityDistinctionWitnessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'projectionTheaterDenied') } else { $null }
        localHostSanctuaryResidencyEnvelopeState = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] $localHostSanctuaryResidencyEnvelopeState.envelopeState } else { $null }
        localHostSanctuaryResidencyEnvelopeReason = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] $localHostSanctuaryResidencyEnvelopeState.reasonCode } else { $null }
        localHostSanctuaryResidencyEnvelopeNextAction = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] $localHostSanctuaryResidencyEnvelopeState.nextAction } else { $null }
        localHostSanctuaryResidencyState = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'residencyState') } else { $null }
        localHostSanctuaryResidencyClass = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'residencyClass') } else { $null }
        localHostSanctuaryHostLocalResourceCount = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'hostLocalResourceCount') } else { $null }
        localHostSanctuaryAdmittedResidencyLaneCount = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'admittedResidencyLaneCount') } else { $null }
        localHostSanctuaryWithheldResidencyLaneCount = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'withheldResidencyLaneCount') } else { $null }
        localHostSanctuaryBondedReleaseDenied = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'bondedReleaseDenied') } else { $null }
        localHostSanctuaryPublicationMaturityDenied = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'publicationMaturityDenied') } else { $null }
        localHostSanctuaryMosBearingDepthDenied = if ($null -ne $localHostSanctuaryResidencyEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'mosBearingDepthDenied') } else { $null }
        runtimeHabitationReadinessLedgerState = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] $runtimeHabitationReadinessLedgerState.readinessLedgerState } else { $null }
        runtimeHabitationReadinessLedgerReason = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] $runtimeHabitationReadinessLedgerState.reasonCode } else { $null }
        runtimeHabitationReadinessLedgerNextAction = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] $runtimeHabitationReadinessLedgerState.nextAction } else { $null }
        runtimeHabitationState = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'habitationState') } else { $null }
        runtimeHabitationClass = if ($null -ne $runtimeHabitationReadinessLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'habitationClass') } else { $null }
        runtimeHabitationReadyConditionCount = if ($null -ne $runtimeHabitationReadinessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'readyConditionCount') } else { $null }
        runtimeHabitationWithheldConditionCount = if ($null -ne $runtimeHabitationReadinessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'withheldConditionCount') } else { $null }
        runtimeHabitationRecurringWorkReady = if ($null -ne $runtimeHabitationReadinessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'recurringWorkReady') } else { $null }
        runtimeHabitationReturnLawBound = if ($null -ne $runtimeHabitationReadinessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'returnLawBound') } else { $null }
        runtimeHabitationBondedReleaseDenied = if ($null -ne $runtimeHabitationReadinessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'bondedReleaseDenied') } else { $null }
        runtimeHabitationPublicationMaturityDenied = if ($null -ne $runtimeHabitationReadinessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'publicationMaturityDenied') } else { $null }
        runtimeHabitationMosBearingDepthDenied = if ($null -ne $runtimeHabitationReadinessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'mosBearingDepthDenied') } else { $null }
        boundedInhabitationLaunchRehearsalState = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [string] $boundedInhabitationLaunchRehearsalState.launchRehearsalState } else { $null }
        boundedInhabitationLaunchRehearsalReason = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [string] $boundedInhabitationLaunchRehearsalState.reasonCode } else { $null }
        boundedInhabitationLaunchRehearsalNextAction = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [string] $boundedInhabitationLaunchRehearsalState.nextAction } else { $null }
        boundedInhabitationLaunchState = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'launchState') } else { $null }
        boundedInhabitationEntryConditionCount = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'entryConditionCount') } else { $null }
        boundedInhabitationDeniedLaneCount = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'deniedLaneCount') } else { $null }
        boundedInhabitationReturnClosureState = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'returnClosureState') } else { $null }
        boundedInhabitationLaunchBounded = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'launchBounded') } else { $null }
        boundedInhabitationReturnClosureWitnessed = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'returnClosureWitnessed') } else { $null }
        boundedInhabitationAmbientBondDenied = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'ambientBondDenied') } else { $null }
        boundedInhabitationPublicationPromotionDenied = if ($null -ne $boundedInhabitationLaunchRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'publicationPromotionDenied') } else { $null }
        postHabitationHorizonLatticeState = if ($null -ne $postHabitationHorizonLatticeState) { [string] $postHabitationHorizonLatticeState.latticeState } else { $null }
        postHabitationHorizonLatticeReason = if ($null -ne $postHabitationHorizonLatticeState) { [string] $postHabitationHorizonLatticeState.reasonCode } else { $null }
        postHabitationHorizonLatticeNextAction = if ($null -ne $postHabitationHorizonLatticeState) { [string] $postHabitationHorizonLatticeState.nextAction } else { $null }
        postHabitationHorizonAnchorReceiptCount = if ($null -ne $postHabitationHorizonLatticeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'anchorReceiptCount') } else { $null }
        postHabitationCandidateHorizonCount = if ($null -ne $postHabitationHorizonLatticeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'candidateHorizonCount') } else { $null }
        postHabitationWithheldExpansionCount = if ($null -ne $postHabitationHorizonLatticeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'withheldExpansionCount') } else { $null }
        boundedHorizonResearchBriefState = if ($null -ne $boundedHorizonResearchBriefState) { [string] $boundedHorizonResearchBriefState.researchBriefState } else { $null }
        boundedHorizonResearchBriefReason = if ($null -ne $boundedHorizonResearchBriefState) { [string] $boundedHorizonResearchBriefState.reasonCode } else { $null }
        boundedHorizonResearchBriefNextAction = if ($null -ne $boundedHorizonResearchBriefState) { [string] $boundedHorizonResearchBriefState.nextAction } else { $null }
        boundedHorizonPrimaryPressurePoint = if ($null -ne $boundedHorizonResearchBriefState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'primaryPressurePoint') } else { $null }
        boundedHorizonQueuedHorizonCount = if ($null -ne $boundedHorizonResearchBriefState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'queuedHorizonCount') } else { $null }
        boundedHorizonWithheldExpansionCount = if ($null -ne $boundedHorizonResearchBriefState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'withheldExpansionCount') } else { $null }
        nextEraBatchSelectorState = if ($null -ne $nextEraBatchSelectorState) { [string] $nextEraBatchSelectorState.selectorState } else { $null }
        nextEraBatchSelectorReason = if ($null -ne $nextEraBatchSelectorState) { [string] $nextEraBatchSelectorState.reasonCode } else { $null }
        nextEraBatchSelectorNextAction = if ($null -ne $nextEraBatchSelectorState) { [string] $nextEraBatchSelectorState.nextAction } else { $null }
        nextEraBatchSelectedMapId = if ($null -ne $nextEraBatchSelectorState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedNextMapId') } else { $null }
        nextEraBatchSelectedCluster = if ($null -ne $nextEraBatchSelectorState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedCluster') } else { $null }
        nextEraBatchQueuedMapCount = if ($null -ne $nextEraBatchSelectorState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'queuedMapCount') } else { $null }
        nextEraBatchSelectionBoundedToDeclaredMaps = if ($null -ne $nextEraBatchSelectorState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectionBoundedToDeclaredMaps') } else { $null }
        inquirySessionDisciplineSurfaceState = if ($null -ne $inquirySessionDisciplineSurfaceState) { [string] $inquirySessionDisciplineSurfaceState.inquirySurfaceState } else { $null }
        inquirySessionDisciplineSurfaceReason = if ($null -ne $inquirySessionDisciplineSurfaceState) { [string] $inquirySessionDisciplineSurfaceState.reasonCode } else { $null }
        inquirySessionDisciplineSurfaceNextAction = if ($null -ne $inquirySessionDisciplineSurfaceState) { [string] $inquirySessionDisciplineSurfaceState.nextAction } else { $null }
        inquirySessionDisciplineState = if ($null -ne $inquirySessionDisciplineSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'inquiryState') } else { $null }
        inquirySessionDisciplineStanceCount = if ($null -ne $inquirySessionDisciplineSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'inquiryStanceCount') } else { $null }
        inquirySessionDisciplineAssumptionExposureModeCount = if ($null -ne $inquirySessionDisciplineSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'assumptionExposureModeCount') } else { $null }
        inquirySessionDisciplineSilenceDispositionCount = if ($null -ne $inquirySessionDisciplineSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'silenceDispositionCount') } else { $null }
        inquirySessionDisciplineChamberNativeInquiryBound = if ($null -ne $inquirySessionDisciplineSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'chamberNativeInquiryBound') } else { $null }
        inquirySessionDisciplineHiddenPressureDenied = if ($null -ne $inquirySessionDisciplineSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'hiddenPressureDenied') } else { $null }
        inquirySessionDisciplinePrematureGelPromotionDenied = if ($null -ne $inquirySessionDisciplineSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'prematureGelPromotionDenied') } else { $null }
        boundaryConditionLedgerState = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.boundaryLedgerState } else { $null }
        boundaryConditionLedgerReason = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.reasonCode } else { $null }
        boundaryConditionLedgerNextAction = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.nextAction } else { $null }
        boundaryConditionLedgerRetainedBoundaryConditionCount = if ($null -ne $boundaryConditionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'retainedBoundaryConditionCount') } else { $null }
        boundaryConditionLedgerContinuityRequirementCount = if ($null -ne $boundaryConditionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'continuityRequirementCount') } else { $null }
        boundaryConditionLedgerWithheldCrossingCount = if ($null -ne $boundaryConditionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'withheldCrossingCount') } else { $null }
        boundaryConditionLedgerBoundaryMemoryCarriedForward = if ($null -ne $boundaryConditionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'boundaryMemoryCarriedForward') } else { $null }
        boundaryConditionLedgerFailurePunishmentDenied = if ($null -ne $boundaryConditionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'failurePunishmentDenied') } else { $null }
        boundaryConditionLedgerIdentityBleedDetected = if ($null -ne $boundaryConditionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'identityBleedDetected') } else { $null }
        coherenceGainWitnessReceiptState = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.coherenceWitnessState } else { $null }
        coherenceGainWitnessReceiptReason = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.reasonCode } else { $null }
        coherenceGainWitnessReceiptNextAction = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.nextAction } else { $null }
        coherenceGainWitnessState = if ($null -ne $coherenceGainWitnessReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherenceState') } else { $null }
        coherenceGainWitnessCoherencePreservingEventCount = if ($null -ne $coherenceGainWitnessReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherencePreservingEventCount') } else { $null }
        coherenceGainWitnessHiddenAssumptionDeniedCount = if ($null -ne $coherenceGainWitnessReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'hiddenAssumptionDeniedCount') } else { $null }
        coherenceGainWitnessBoundaryConditionCount = if ($null -ne $coherenceGainWitnessReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'boundaryConditionCount') } else { $null }
        coherenceGainWitnessSharedIntelligibilityPreserved = if ($null -ne $coherenceGainWitnessReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'sharedIntelligibilityPreserved') } else { $null }
        coherenceGainWitnessAdmissibilitySpacePreserved = if ($null -ne $coherenceGainWitnessReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'admissibilitySpacePreserved') } else { $null }
        coherenceGainWitnessPrematureClosureDetected = if ($null -ne $coherenceGainWitnessReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'prematureClosureDetected') } else { $null }
        operatorInquirySelectionEnvelopeState = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.operatorInquirySelectionEnvelopeState } else { $null }
        operatorInquirySelectionEnvelopeReason = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.reasonCode } else { $null }
        operatorInquirySelectionEnvelopeNextAction = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] $operatorInquirySelectionEnvelopeState.nextAction } else { $null }
        operatorInquirySelectionState = if ($null -ne $operatorInquirySelectionEnvelopeState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionState') } else { $null }
        operatorInquirySelectionAvailableInquiryStanceCount = if ($null -ne $operatorInquirySelectionEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'availableInquiryStanceCount') } else { $null }
        operatorInquirySelectionKnownBoundaryWarningCount = if ($null -ne $operatorInquirySelectionEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'knownBoundaryWarningCount') } else { $null }
        operatorInquirySelectionLawfulUseConditionCount = if ($null -ne $operatorInquirySelectionEnvelopeState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'lawfulUseConditionCount') } else { $null }
        operatorInquirySelectionProtectedInteriorityDenied = if ($null -ne $operatorInquirySelectionEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'protectedInteriorityDenied') } else { $null }
        operatorInquirySelectionLocalityBypassDenied = if ($null -ne $operatorInquirySelectionEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'localityBypassDenied') } else { $null }
        operatorInquirySelectionRawGrantDenied = if ($null -ne $operatorInquirySelectionEnvelopeState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'rawGrantDenied') } else { $null }
        bondedCrucibleSessionRehearsalState = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.bondedCrucibleSessionRehearsalState } else { $null }
        bondedCrucibleSessionRehearsalReason = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.reasonCode } else { $null }
        bondedCrucibleSessionRehearsalNextAction = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.nextAction } else { $null }
        bondedCrucibleState = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'crucibleState') } else { $null }
        bondedCrucibleSelectedInquiryStanceCount = if ($null -ne $bondedCrucibleSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'selectedInquiryStanceCount') } else { $null }
        bondedCrucibleSharedUnknownFacetCount = if ($null -ne $bondedCrucibleSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'sharedUnknownFacetCount') } else { $null }
        bondedCrucibleCoordinationHoldCount = if ($null -ne $bondedCrucibleSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'coordinationHoldCount') } else { $null }
        bondedCrucibleExposedBoundaryCount = if ($null -ne $bondedCrucibleSessionRehearsalState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'exposedBoundaryCount') } else { $null }
        bondedCruciblePreScriptedAnswerDenied = if ($null -ne $bondedCrucibleSessionRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'preScriptedAnswerDenied') } else { $null }
        bondedCrucibleRemoteDominanceDenied = if ($null -ne $bondedCrucibleSessionRehearsalState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'remoteDominanceDenied') } else { $null }
        sharedBoundaryMemoryLedgerState = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.sharedBoundaryMemoryLedgerState } else { $null }
        sharedBoundaryMemoryLedgerReason = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.reasonCode } else { $null }
        sharedBoundaryMemoryLedgerNextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { $null }
        sharedBoundaryMemoryState = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryState') } else { $null }
        sharedBoundaryMemorySharedBoundaryConditionCount = if ($null -ne $sharedBoundaryMemoryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryConditionCount') } else { $null }
        sharedBoundaryMemorySharedContinuityRequirementCount = if ($null -ne $sharedBoundaryMemoryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedContinuityRequirementCount') } else { $null }
        sharedBoundaryMemoryWithheldCommonPropertyClaimCount = if ($null -ne $sharedBoundaryMemoryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'withheldCommonPropertyClaimCount') } else { $null }
        sharedBoundaryMemoryLocalityProvenancePreserved = if ($null -ne $sharedBoundaryMemoryLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'localityProvenancePreserved') } else { $null }
        sharedBoundaryMemoryIdentityBleedDetected = if ($null -ne $sharedBoundaryMemoryLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'identityBleedDetected') } else { $null }
        sharedBoundaryMemoryAmbientCommonPropertyDenied = if ($null -ne $sharedBoundaryMemoryLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'ambientCommonPropertyDenied') } else { $null }
        continuityUnderPressureLedgerState = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.continuityUnderPressureLedgerState } else { $null }
        continuityUnderPressureLedgerReason = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.reasonCode } else { $null }
        continuityUnderPressureLedgerNextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { $null }
        continuityUnderPressureState = if ($null -ne $continuityUnderPressureLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'pressureState') } else { $null }
        continuityUnderPressureHeldContinuityCount = if ($null -ne $continuityUnderPressureLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'heldContinuityCount') } else { $null }
        continuityUnderPressurePartialContinuityCount = if ($null -ne $continuityUnderPressureLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'partialContinuityCount') } else { $null }
        continuityUnderPressureRequiredPreservationCount = if ($null -ne $continuityUnderPressureLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'requiredPreservationCount') } else { $null }
        continuityUnderPressureBoundaryPressureCount = if ($null -ne $continuityUnderPressureLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'boundaryPressureCount') } else { $null }
        continuityUnderPressureFluentSuccessDenied = if ($null -ne $continuityUnderPressureLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'fluentSuccessDenied') } else { $null }
        expressiveDeformationReceiptState = if ($null -ne $expressiveDeformationReceiptState) { [string] $expressiveDeformationReceiptState.expressiveDeformationReceiptState } else { $null }
        expressiveDeformationReceiptReason = if ($null -ne $expressiveDeformationReceiptState) { [string] $expressiveDeformationReceiptState.reasonCode } else { $null }
        expressiveDeformationReceiptNextAction = if ($null -ne $expressiveDeformationReceiptState) { [string] $expressiveDeformationReceiptState.nextAction } else { $null }
        expressiveDeformationState = if ($null -ne $expressiveDeformationReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'deformationState') } else { $null }
        expressiveDeformationChangedExpressionCount = if ($null -ne $expressiveDeformationReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'changedExpressionCount') } else { $null }
        expressiveDeformationRecognizableContinuityCount = if ($null -ne $expressiveDeformationReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'recognizableContinuityCount') } else { $null }
        expressiveDeformationFractureBoundaryCount = if ($null -ne $expressiveDeformationReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'fractureBoundaryCount') } else { $null }
        expressiveDeformationAdaptiveRefinementPreserved = if ($null -ne $expressiveDeformationReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'adaptiveRefinementPreserved') } else { $null }
        expressiveDeformationIdentityCollapseDetected = if ($null -ne $expressiveDeformationReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'identityCollapseDetected') } else { $null }
        mutualIntelligibilityWitnessState = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.mutualIntelligibilityWitnessState } else { $null }
        mutualIntelligibilityWitnessReason = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.reasonCode } else { $null }
        mutualIntelligibilityWitnessNextAction = if ($null -ne $mutualIntelligibilityWitnessState) { [string] $mutualIntelligibilityWitnessState.nextAction } else { $null }
        mutualIntelligibilitySharedUnderstandingState = if ($null -ne $mutualIntelligibilityWitnessState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'sharedUnderstandingState') } else { $null }
        mutualIntelligibilityHeldCount = if ($null -ne $mutualIntelligibilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'heldIntelligibilityCount') } else { $null }
        mutualIntelligibilityNarrowedCount = if ($null -ne $mutualIntelligibilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'narrowedIntelligibilityCount') } else { $null }
        mutualIntelligibilityBrokenCount = if ($null -ne $mutualIntelligibilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'brokenIntelligibilityCount') } else { $null }
        mutualIntelligibilitySamenessCollapseDenied = if ($null -ne $mutualIntelligibilityWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'samenessCollapseDenied') } else { $null }
        mutualIntelligibilityOpaqueDivergenceDetected = if ($null -ne $mutualIntelligibilityWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'opaqueDivergenceDetected') } else { $null }
        inquiryPatternContinuityLedgerState = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.inquiryPatternContinuityLedgerState } else { $null }
        inquiryPatternContinuityLedgerReason = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.reasonCode } else { $null }
        inquiryPatternContinuityLedgerNextAction = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] $inquiryPatternContinuityLedgerState.nextAction } else { $null }
        inquiryPatternContinuityState = if ($null -ne $inquiryPatternContinuityLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'carryForwardState') } else { $null }
        inquiryPatternReusableCount = if ($null -ne $inquiryPatternContinuityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'reusableInquiryPatternCount') } else { $null }
        inquiryPatternTriggerConditionCount = if ($null -ne $inquiryPatternContinuityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'triggerConditionCount') } else { $null }
        inquiryPatternPreservedConstraintCount = if ($null -ne $inquiryPatternContinuityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'preservedConstraintCount') } else { $null }
        inquiryPatternBoundaryPairCount = if ($null -ne $inquiryPatternContinuityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'boundaryPairCount') } else { $null }
        inquiryPatternIdentityBleedDenied = if ($null -ne $inquiryPatternContinuityLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'identityBleedDenied') } else { $null }
        questioningBoundaryPairLedgerState = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.questioningBoundaryPairLedgerState } else { $null }
        questioningBoundaryPairLedgerReason = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.reasonCode } else { $null }
        questioningBoundaryPairLedgerNextAction = if ($null -ne $questioningBoundaryPairLedgerState) { [string] $questioningBoundaryPairLedgerState.nextAction } else { $null }
        questioningBoundaryPairState = if ($null -ne $questioningBoundaryPairLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'pairingState') } else { $null }
        questioningBoundaryPairInquiryPatternCount = if ($null -ne $questioningBoundaryPairLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'inquiryPatternCount') } else { $null }
        questioningBoundaryPairSupportingBoundaryCount = if ($null -ne $questioningBoundaryPairLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'supportingBoundaryCount') } else { $null }
        questioningBoundaryPairBoundaryConstraintCount = if ($null -ne $questioningBoundaryPairLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'boundaryConstraintCount') } else { $null }
        questioningBoundaryPairOverreachWarningCount = if ($null -ne $questioningBoundaryPairLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'overreachWarningCount') } else { $null }
        questioningBoundaryPairConstraintMemoryPreserved = if ($null -ne $questioningBoundaryPairLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'constraintMemoryPreserved') } else { $null }
        carryForwardInquirySelectionSurfaceState = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.carryForwardInquirySelectionSurfaceState } else { $null }
        carryForwardInquirySelectionSurfaceReason = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.reasonCode } else { $null }
        carryForwardInquirySelectionSurfaceNextAction = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] $carryForwardInquirySelectionSurfaceState.nextAction } else { $null }
        carryForwardInquirySelectionState = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionState') } else { $null }
        carryForwardAvailablePatternCount = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'availableCarryForwardPatternCount') } else { $null }
        carryForwardAdmittedReuseConditionCount = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'admittedReuseConditionCount') } else { $null }
        carryForwardWithheldReuseWarningCount = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'withheldReuseWarningCount') } else { $null }
        carryForwardLocalitySafeReview = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'localitySafeReview') } else { $null }
        carryForwardAmbientHabitDenied = if ($null -ne $carryForwardInquirySelectionSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'ambientHabitDenied') } else { $null }
        engramDistanceClassificationLedgerState = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.engramDistanceClassificationLedgerState } else { $null }
        engramDistanceClassificationReason = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.reasonCode } else { $null }
        engramDistanceClassificationNextAction = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.nextAction } else { $null }
        engramDistanceDominantClass = if ($null -ne $engramDistanceClassificationLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $engramDistanceClassificationLedgerState -PropertyName 'dominantDistanceClass') } else { $null }
        engramDistanceAdjacentRootPatternCount = if ($null -ne $engramDistanceClassificationLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $engramDistanceClassificationLedgerState -PropertyName 'adjacentRootPatternCount') } else { $null }
        engramDistanceFarOtherPromotionDenied = if ($null -ne $engramDistanceClassificationLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $engramDistanceClassificationLedgerState -PropertyName 'promotionFromFarOtherDenied') } else { $null }
        engramPromotionRequirementsMatrixState = if ($null -ne $engramPromotionRequirementsMatrixState) { [string] $engramPromotionRequirementsMatrixState.engramPromotionRequirementsMatrixState } else { $null }
        engramPromotionRequirementsReason = if ($null -ne $engramPromotionRequirementsMatrixState) { [string] $engramPromotionRequirementsMatrixState.reasonCode } else { $null }
        engramPromotionRequirementsNextAction = if ($null -ne $engramPromotionRequirementsMatrixState) { [string] $engramPromotionRequirementsMatrixState.nextAction } else { $null }
        engramPromotionRequirementEntryCount = if ($null -ne $engramPromotionRequirementsMatrixState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $engramPromotionRequirementsMatrixState -PropertyName 'requirementEntryCount') } else { $null }
        engramPromotionBurdenScalingPreserved = if ($null -ne $engramPromotionRequirementsMatrixState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $engramPromotionRequirementsMatrixState -PropertyName 'burdenScalingPreserved') } else { $null }
        distanceWeightedQuestioningAdmissionSurfaceState = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [string] $distanceWeightedQuestioningAdmissionSurfaceState.distanceWeightedQuestioningAdmissionSurfaceState } else { $null }
        distanceWeightedQuestioningAdmissionReason = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [string] $distanceWeightedQuestioningAdmissionSurfaceState.reasonCode } else { $null }
        distanceWeightedQuestioningAdmissionNextAction = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [string] $distanceWeightedQuestioningAdmissionSurfaceState.nextAction } else { $null }
        distanceWeightedQuestioningPromotionCeiling = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $distanceWeightedQuestioningAdmissionSurfaceState -PropertyName 'promotionCeiling') } else { $null }
        distanceWeightedQuestioningAdmittedPatternCount = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $distanceWeightedQuestioningAdmissionSurfaceState -PropertyName 'admittedCandidatePatternCount') } else { $null }
        distanceWeightedQuestioningWithheldPatternCount = if ($null -ne $distanceWeightedQuestioningAdmissionSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $distanceWeightedQuestioningAdmissionSurfaceState -PropertyName 'withheldCandidatePatternCount') } else { $null }
        questioningOperatorCandidateLedgerState = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] $questioningOperatorCandidateLedgerState.questioningOperatorCandidateLedgerState } else { $null }
        questioningOperatorCandidateLedgerReason = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] $questioningOperatorCandidateLedgerState.reasonCode } else { $null }
        questioningOperatorCandidateLedgerNextAction = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] $questioningOperatorCandidateLedgerState.nextAction } else { $null }
        questioningOperatorCandidateClassificationState = if ($null -ne $questioningOperatorCandidateLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'candidateClassificationState') } else { $null }
        questioningOperatorEventBoundFormCount = if ($null -ne $questioningOperatorCandidateLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'eventBoundInquiryFormCount') } else { $null }
        questioningOperatorCandidatePatternCount = if ($null -ne $questioningOperatorCandidateLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'candidateInquiryPatternCount') } else { $null }
        questioningOperatorPromotionEvidenceCount = if ($null -ne $questioningOperatorCandidateLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'promotionEvidenceCount') } else { $null }
        questioningOperatorRequiredReentryConditionCount = if ($null -ne $questioningOperatorCandidateLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'requiredReentryConditionCount') } else { $null }
        questioningOperatorFailureSignatureExpectationCount = if ($null -ne $questioningOperatorCandidateLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'failureSignatureExpectationCount') } else { $null }
        questioningOperatorHiddenAuthorityPatternsDenied = if ($null -ne $questioningOperatorCandidateLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'hiddenAuthorityPatternsDenied') } else { $null }
        questioningOperatorIdentityBoundPatternsWithheld = if ($null -ne $questioningOperatorCandidateLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'identityBoundPatternsWithheld') } else { $null }
        questioningGelPromotionGateState = if ($null -ne $questioningGelPromotionGateState) { [string] $questioningGelPromotionGateState.questioningGelPromotionGateState } else { $null }
        questioningGelPromotionGateReason = if ($null -ne $questioningGelPromotionGateState) { [string] $questioningGelPromotionGateState.reasonCode } else { $null }
        questioningGelPromotionGateNextAction = if ($null -ne $questioningGelPromotionGateState) { [string] $questioningGelPromotionGateState.nextAction } else { $null }
        questioningGelPromotionGateReviewState = if ($null -ne $questioningGelPromotionGateState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionGateState') } else { $null }
        questioningGelCandidatePatternCount = if ($null -ne $questioningGelPromotionGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'candidateInquiryPatternCount') } else { $null }
        questioningGelSatisfiedPromotionConditionCount = if ($null -ne $questioningGelPromotionGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'satisfiedPromotionConditionCount') } else { $null }
        questioningGelUnmetPromotionConditionCount = if ($null -ne $questioningGelPromotionGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'unmetPromotionConditionCount') } else { $null }
        questioningGelPromotionWarningCount = if ($null -ne $questioningGelPromotionGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionWarningCount') } else { $null }
        questioningGelLocalitySeparationPreserved = if ($null -ne $questioningGelPromotionGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'localitySeparationPreserved') } else { $null }
        questioningGelAuthoritySeparationPreserved = if ($null -ne $questioningGelPromotionGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'authoritySeparationPreserved') } else { $null }
        questioningGelTruthSeekingInvariantPreserved = if ($null -ne $questioningGelPromotionGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'truthSeekingInvariantPreserved') } else { $null }
        questioningGelOutcomeSeekingDenied = if ($null -ne $questioningGelPromotionGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'outcomeSeekingDenied') } else { $null }
        questioningGelPromotionReviewAdmitted = if ($null -ne $questioningGelPromotionGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionReviewAdmitted') } else { $null }
        protectedQuestioningPatternSurfaceState = if ($null -ne $protectedQuestioningPatternSurfaceState) { [string] $protectedQuestioningPatternSurfaceState.protectedQuestioningPatternSurfaceState } else { $null }
        protectedQuestioningPatternSurfaceReason = if ($null -ne $protectedQuestioningPatternSurfaceState) { [string] $protectedQuestioningPatternSurfaceState.reasonCode } else { $null }
        protectedQuestioningPatternSurfaceNextAction = if ($null -ne $protectedQuestioningPatternSurfaceState) { [string] $protectedQuestioningPatternSurfaceState.nextAction } else { $null }
        protectedQuestioningReviewState = if ($null -ne $protectedQuestioningPatternSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'protectedReviewState') } else { $null }
        protectedQuestioningReviewableCandidatePatternCount = if ($null -ne $protectedQuestioningPatternSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'reviewableCandidatePatternCount') } else { $null }
        protectedQuestioningLawfulReviewEnvelopeCount = if ($null -ne $protectedQuestioningPatternSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'lawfulReviewEnvelopeCount') } else { $null }
        protectedQuestioningWithheldInteriorityWarningCount = if ($null -ne $protectedQuestioningPatternSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'withheldInteriorityWarningCount') } else { $null }
        protectedQuestioningLocalitySafeLegibility = if ($null -ne $protectedQuestioningPatternSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'localitySafeLegibility') } else { $null }
        protectedQuestioningRawInteriorityDenied = if ($null -ne $protectedQuestioningPatternSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'rawInteriorityDenied') } else { $null }
        protectedQuestioningAutomaticGrantDenied = if ($null -ne $protectedQuestioningPatternSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'automaticGrantDenied') } else { $null }
        variationTestedReentryLedgerState = if ($null -ne $variationTestedReentryLedgerState) { [string] $variationTestedReentryLedgerState.variationTestedReentryLedgerState } else { $null }
        variationTestedReentryReason = if ($null -ne $variationTestedReentryLedgerState) { [string] $variationTestedReentryLedgerState.reasonCode } else { $null }
        variationTestedReentryNextAction = if ($null -ne $variationTestedReentryLedgerState) { [string] $variationTestedReentryLedgerState.nextAction } else { $null }
        variationTestedReentrySurvivingPatternCount = if ($null -ne $variationTestedReentryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'survivingPatternCount') } else { $null }
        variationTestedReentryFailedPatternCount = if ($null -ne $variationTestedReentryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'failedPatternCount') } else { $null }
        variationTestedReentryRequiredRetestPatternCount = if ($null -ne $variationTestedReentryLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'requiredRetestPatternCount') } else { $null }
        variationTestedReentryVariationBurdenSatisfied = if ($null -ne $variationTestedReentryLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'variationBurdenSatisfied') } else { $null }
        questioningAdmissionRefusalReceiptState = if ($null -ne $questioningAdmissionRefusalReceiptState) { [string] $questioningAdmissionRefusalReceiptState.questioningAdmissionRefusalReceiptState } else { $null }
        questioningAdmissionRefusalReason = if ($null -ne $questioningAdmissionRefusalReceiptState) { [string] $questioningAdmissionRefusalReceiptState.reasonCode } else { $null }
        questioningAdmissionRefusalNextAction = if ($null -ne $questioningAdmissionRefusalReceiptState) { [string] $questioningAdmissionRefusalReceiptState.nextAction } else { $null }
        questioningAdmissionRefusalRefusedPatternCount = if ($null -ne $questioningAdmissionRefusalReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'refusedPatternCount') } else { $null }
        questioningAdmissionRefusalRefusalReasonCount = if ($null -ne $questioningAdmissionRefusalReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'refusalReasonCount') } else { $null }
        questioningAdmissionRefusalArchiveProtectionPreserved = if ($null -ne $questioningAdmissionRefusalReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'archiveProtectionPreserved') } else { $null }
        promotionSeductionWatchState = if ($null -ne $promotionSeductionWatchState) { [string] $promotionSeductionWatchState.promotionSeductionWatchState } else { $null }
        promotionSeductionWatchReason = if ($null -ne $promotionSeductionWatchState) { [string] $promotionSeductionWatchState.reasonCode } else { $null }
        promotionSeductionWatchNextAction = if ($null -ne $promotionSeductionWatchState) { [string] $promotionSeductionWatchState.nextAction } else { $null }
        promotionSeductionSignalCount = if ($null -ne $promotionSeductionWatchState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'seductionSignalCount') } else { $null }
        promotionSeductionBlockedVectorCount = if ($null -ne $promotionSeductionWatchState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'blockedPromotionVectorCount') } else { $null }
        promotionSeductionDriftWarningCount = if ($null -ne $promotionSeductionWatchState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'driftWarningCount') } else { $null }
        promotionSeductionPrestigeInflationDenied = if ($null -ne $promotionSeductionWatchState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'prestigeInflationDenied') } else { $null }
        engramIntentFieldLedgerState = if ($null -ne $engramIntentFieldLedgerState) { [string] $engramIntentFieldLedgerState.engramIntentFieldLedgerState } else { $null }
        engramIntentFieldReason = if ($null -ne $engramIntentFieldLedgerState) { [string] $engramIntentFieldLedgerState.reasonCode } else { $null }
        engramIntentFieldNextAction = if ($null -ne $engramIntentFieldLedgerState) { [string] $engramIntentFieldLedgerState.nextAction } else { $null }
        engramIntentBearingPatternCount = if ($null -ne $engramIntentFieldLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'intentBearingPatternCount') } else { $null }
        engramIntentSceneBoundPatternCount = if ($null -ne $engramIntentFieldLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'sceneBoundPatternCount') } else { $null }
        engramIntentCandidateCarriesInternalIntent = if ($null -ne $engramIntentFieldLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'candidateCarriesInternalIntent') } else { $null }
        engramIntentBorrowedJustificationDenied = if ($null -ne $engramIntentFieldLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'borrowedJustificationDenied') } else { $null }
        intentConstraintAlignmentReceiptState = if ($null -ne $intentConstraintAlignmentReceiptState) { [string] $intentConstraintAlignmentReceiptState.intentConstraintAlignmentReceiptState } else { $null }
        intentConstraintAlignmentReason = if ($null -ne $intentConstraintAlignmentReceiptState) { [string] $intentConstraintAlignmentReceiptState.reasonCode } else { $null }
        intentConstraintAlignmentNextAction = if ($null -ne $intentConstraintAlignmentReceiptState) { [string] $intentConstraintAlignmentReceiptState.nextAction } else { $null }
        intentConstraintAlignedPatternCount = if ($null -ne $intentConstraintAlignmentReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'alignedPatternCount') } else { $null }
        intentConstraintMisalignedPatternCount = if ($null -ne $intentConstraintAlignmentReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'misalignedPatternCount') } else { $null }
        intentConstraintAlignmentSatisfied = if ($null -ne $intentConstraintAlignmentReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'structureConstraintAlignmentSatisfied') } else { $null }
        intentConstraintProvenanceAlignedWithIntent = if ($null -ne $intentConstraintAlignmentReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'provenanceAlignedWithIntent') } else { $null }
        intentConstraintSceneBoundIntentDetected = if ($null -ne $intentConstraintAlignmentReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'sceneBoundIntentDetected') } else { $null }
        warmReactivationDispositionReceiptState = if ($null -ne $warmReactivationDispositionReceiptState) { [string] $warmReactivationDispositionReceiptState.warmReactivationDispositionReceiptState } else { $null }
        warmReactivationDispositionReason = if ($null -ne $warmReactivationDispositionReceiptState) { [string] $warmReactivationDispositionReceiptState.reasonCode } else { $null }
        warmReactivationDispositionNextAction = if ($null -ne $warmReactivationDispositionReceiptState) { [string] $warmReactivationDispositionReceiptState.nextAction } else { $null }
        warmHeldPatternCount = if ($null -ne $warmReactivationDispositionReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'warmHeldPatternCount') } else { $null }
        warmReactivatedHotPatternCount = if ($null -ne $warmReactivationDispositionReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'reactivatedHotPatternCount') } else { $null }
        warmArchivedPatternCount = if ($null -ne $warmReactivationDispositionReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'archivedPatternCount') } else { $null }
        warmHoldingPreserved = if ($null -ne $warmReactivationDispositionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'warmHoldingPreserved') } else { $null }
        warmHotReentryRequired = if ($null -ne $warmReactivationDispositionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'hotReentryRequired') } else { $null }
        warmColdAdmissionWithheld = if ($null -ne $warmReactivationDispositionReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'coldAdmissionWithheld') } else { $null }
        formationPhaseVectorState = if ($null -ne $formationPhaseVectorState) { [string] $formationPhaseVectorState.formationPhaseVectorState } else { $null }
        formationPhaseVectorReason = if ($null -ne $formationPhaseVectorState) { [string] $formationPhaseVectorState.reasonCode } else { $null }
        formationPhaseVectorNextAction = if ($null -ne $formationPhaseVectorState) { [string] $formationPhaseVectorState.nextAction } else { $null }
        formationPhaseAxisCount = if ($null -ne $formationPhaseVectorState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'phaseAxisCount') } else { $null }
        formationStabilityAxisCount = if ($null -ne $formationPhaseVectorState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'stabilityAxisCount') } else { $null }
        formationThermalRegionCount = if ($null -ne $formationPhaseVectorState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'thermalRegionCount') } else { $null }
        formationWarmGovernanceDominant = if ($null -ne $formationPhaseVectorState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'warmGovernanceDominant') } else { $null }
        formationCoolingEligible = if ($null -ne $formationPhaseVectorState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'coolingEligible') } else { $null }
        formationReheatingSensitive = if ($null -ne $formationPhaseVectorState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'reheatingSensitive') } else { $null }
        brittlenessWitnessState = if ($null -ne $brittlenessWitnessState) { [string] $brittlenessWitnessState.brittlenessWitnessState } else { $null }
        brittlenessWitnessReason = if ($null -ne $brittlenessWitnessState) { [string] $brittlenessWitnessState.reasonCode } else { $null }
        brittlenessWitnessNextAction = if ($null -ne $brittlenessWitnessState) { [string] $brittlenessWitnessState.nextAction } else { $null }
        brittlePatternCount = if ($null -ne $brittlenessWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'brittlePatternCount') } else { $null }
        brittlenessFractureAxisCount = if ($null -ne $brittlenessWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'fractureAxisCount') } else { $null }
        brittlenessOverfitWarningCount = if ($null -ne $brittlenessWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'overfitWarningCount') } else { $null }
        sceneBoundBrittlenessDetected = if ($null -ne $brittlenessWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'sceneBoundBrittlenessDetected') } else { $null }
        misalignmentPressureDetected = if ($null -ne $brittlenessWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'misalignmentPressureDetected') } else { $null }
        prematureCoolingDenied = if ($null -ne $brittlenessWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'prematureCoolingDenied') } else { $null }
        durabilityWitnessState = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.durabilityWitnessState } else { $null }
        durabilityWitnessReason = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.reasonCode } else { $null }
        durabilityWitnessNextAction = if ($null -ne $durabilityWitnessState) { [string] $durabilityWitnessState.nextAction } else { $null }
        durablePatternCount = if ($null -ne $durabilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'durablePatternCount') } else { $null }
        durabilityInterlockSignalCount = if ($null -ne $durabilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'interlockSignalCount') } else { $null }
        durabilityCoolingBarrierCount = if ($null -ne $durabilityWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'coolingBarrierCount') } else { $null }
        durableUnderVariation = if ($null -ne $durabilityWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'durableUnderVariation') } else { $null }
        interlockDensityEmergent = if ($null -ne $durabilityWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'interlockDensityEmergent') } else { $null }
        coldPromotionStillWithheld = if ($null -ne $durabilityWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'coldPromotionStillWithheld') } else { $null }
        warmClockDispositionState = if ($null -ne $warmClockDispositionState) { [string] $warmClockDispositionState.warmClockDispositionState } else { $null }
        warmClockDispositionReason = if ($null -ne $warmClockDispositionState) { [string] $warmClockDispositionState.reasonCode } else { $null }
        warmClockDispositionNextAction = if ($null -ne $warmClockDispositionState) { [string] $warmClockDispositionState.nextAction } else { $null }
        warmClockCount = if ($null -ne $warmClockDispositionState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'warmClockCount') } else { $null }
        warmClockUnresolvedUnknownLoad = if ($null -ne $warmClockDispositionState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'unresolvedUnknownLoad') } else { $null }
        warmClockRipeningDisposition = if ($null -ne $warmClockDispositionState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'ripeningDisposition') } else { $null }
        warmClockStalenessDisposition = if ($null -ne $warmClockDispositionState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'stalenessDisposition') } else { $null }
        warmClockReentryClockActive = if ($null -ne $warmClockDispositionState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'reentryClockActive') } else { $null }
        warmClockDistanceBurdenStillActive = if ($null -ne $warmClockDispositionState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'distanceBurdenStillActive') } else { $null }
        warmClockFailureSignatureFreshnessRequired = if ($null -ne $warmClockDispositionState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'failureSignatureFreshnessRequired') } else { $null }
        warmClockRipeningUnderway = if ($null -ne $warmClockDispositionState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'warmRipeningUnderway') } else { $null }
        warmClockStalenessRiskPresent = if ($null -ne $warmClockDispositionState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'stalenessRiskPresent') } else { $null }
        ripeningStalenessLedgerState = if ($null -ne $ripeningStalenessLedgerState) { [string] $ripeningStalenessLedgerState.ripeningStalenessLedgerState } else { $null }
        ripeningStalenessLedgerReason = if ($null -ne $ripeningStalenessLedgerState) { [string] $ripeningStalenessLedgerState.reasonCode } else { $null }
        ripeningStalenessLedgerNextAction = if ($null -ne $ripeningStalenessLedgerState) { [string] $ripeningStalenessLedgerState.nextAction } else { $null }
        ripeningPatternCount = if ($null -ne $ripeningStalenessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'ripeningPatternCount') } else { $null }
        stalePatternCount = if ($null -ne $ripeningStalenessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'stalePatternCount') } else { $null }
        ripeningWindowCount = if ($null -ne $ripeningStalenessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'ripeningWindowCount') } else { $null }
        staleWindowCount = if ($null -ne $ripeningStalenessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'staleWindowCount') } else { $null }
        ripeningRefreshRequiredCount = if ($null -ne $ripeningStalenessLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'refreshRequiredCount') } else { $null }
        honestWarmRipeningPreserved = if ($null -ne $ripeningStalenessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'honestWarmRipeningPreserved') } else { $null }
        administrativeSuspensionDenied = if ($null -ne $ripeningStalenessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'administrativeSuspensionDenied') } else { $null }
        freshConstraintContactStillRequired = if ($null -ne $ripeningStalenessLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'freshConstraintContactStillRequired') } else { $null }
        coolingPressureWitnessState = if ($null -ne $coolingPressureWitnessState) { [string] $coolingPressureWitnessState.coolingPressureWitnessState } else { $null }
        coolingPressureWitnessReason = if ($null -ne $coolingPressureWitnessState) { [string] $coolingPressureWitnessState.reasonCode } else { $null }
        coolingPressureWitnessNextAction = if ($null -ne $coolingPressureWitnessState) { [string] $coolingPressureWitnessState.nextAction } else { $null }
        coolingPressureForceCount = if ($null -ne $coolingPressureWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingForceCount') } else { $null }
        coolingPressureBarrierCount = if ($null -ne $coolingPressureWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingBarrierCount') } else { $null }
        coolingPressureDisposition = if ($null -ne $coolingPressureWitnessState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'pressureDisposition') } else { $null }
        coolingPressureEmergent = if ($null -ne $coolingPressureWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingPressureEmergent') } else { $null }
        coolingPressureColdApproachLawful = if ($null -ne $coolingPressureWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coldApproachLawful') } else { $null }
        reheatingOrArchivePressureStillStronger = if ($null -ne $coolingPressureWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'reheatingOrArchivePressureStillStronger') } else { $null }
        hotReactivationTriggerReceiptState = if ($null -ne $hotReactivationTriggerReceiptState) { [string] $hotReactivationTriggerReceiptState.hotReactivationTriggerReceiptState } else { $null }
        hotReactivationTriggerReason = if ($null -ne $hotReactivationTriggerReceiptState) { [string] $hotReactivationTriggerReceiptState.reasonCode } else { $null }
        hotReactivationTriggerNextAction = if ($null -ne $hotReactivationTriggerReceiptState) { [string] $hotReactivationTriggerReceiptState.nextAction } else { $null }
        hotReactivationTriggerCount = if ($null -ne $hotReactivationTriggerReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reactivationTriggerCount') } else { $null }
        hotReactivationFailedInvariantCount = if ($null -ne $hotReactivationTriggerReceiptState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'failedInvariantCount') } else { $null }
        hotReactivationDisposition = if ($null -ne $hotReactivationTriggerReceiptState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reactivationDisposition') } else { $null }
        hotReturnLawful = if ($null -ne $hotReactivationTriggerReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'hotReturnLawful') } else { $null }
        warmHoldingInsufficient = if ($null -ne $hotReactivationTriggerReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'warmHoldingInsufficient') } else { $null }
        reentryAsFormationPreserved = if ($null -ne $hotReactivationTriggerReceiptState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reentryAsFormationPreserved') } else { $null }
        coldAdmissionEligibilityGateState = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.coldAdmissionEligibilityGateState } else { $null }
        coldAdmissionEligibilityGateReason = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.reasonCode } else { $null }
        coldAdmissionEligibilityGateNextAction = if ($null -ne $coldAdmissionEligibilityGateState) { [string] $coldAdmissionEligibilityGateState.nextAction } else { $null }
        coldEligibilitySignalCount = if ($null -ne $coldAdmissionEligibilityGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'eligibilitySignalCount') } else { $null }
        coldEligibilityRemainingBarrierCount = if ($null -ne $coldAdmissionEligibilityGateState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'remainingBarrierCount') } else { $null }
        coldEligibilityDisposition = if ($null -ne $coldAdmissionEligibilityGateState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'eligibilityDisposition') } else { $null }
        coldEligibilityColdApproachLawful = if ($null -ne $coldAdmissionEligibilityGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'coldApproachLawful') } else { $null }
        coldEligibilityPreFreezeOnly = if ($null -ne $coldAdmissionEligibilityGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'preFreezeOnly') } else { $null }
        coldEligibilityFinalInheritanceStillWithheld = if ($null -ne $coldAdmissionEligibilityGateState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'finalInheritanceStillWithheld') } else { $null }
        archiveDispositionLedgerState = if ($null -ne $archiveDispositionLedgerState) { [string] $archiveDispositionLedgerState.archiveDispositionLedgerState } else { $null }
        archiveDispositionLedgerReason = if ($null -ne $archiveDispositionLedgerState) { [string] $archiveDispositionLedgerState.reasonCode } else { $null }
        archiveDispositionLedgerNextAction = if ($null -ne $archiveDispositionLedgerState) { [string] $archiveDispositionLedgerState.nextAction } else { $null }
        archiveRouteCount = if ($null -ne $archiveDispositionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'archiveRouteCount') } else { $null }
        preservedProvenanceMarkCount = if ($null -ne $archiveDispositionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'preservedProvenanceMarkCount') } else { $null }
        deniedRewriteRiskCount = if ($null -ne $archiveDispositionLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'deniedRewriteRiskCount') } else { $null }
        archiveDisposition = if ($null -ne $archiveDispositionLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'archiveDisposition') } else { $null }
        archiveProvenancePreserved = if ($null -ne $archiveDispositionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'provenancePreserved') } else { $null }
        archivePseudoLineageDenied = if ($null -ne $archiveDispositionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'pseudoLineageDenied') } else { $null }
        archiveWarmIndefiniteHoldingDenied = if ($null -ne $archiveDispositionLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'warmIndefiniteHoldingDenied') } else { $null }
        interlockDensityLedgerState = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.interlockDensityLedgerState } else { $null }
        interlockDensityLedgerReason = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.reasonCode } else { $null }
        interlockDensityLedgerNextAction = if ($null -ne $interlockDensityLedgerState) { [string] $interlockDensityLedgerState.nextAction } else { $null }
        interlockDensityIndependentConstraintLinkCount = if ($null -ne $interlockDensityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'independentConstraintLinkCount') } else { $null }
        interlockDensityReentrySurvivalCount = if ($null -ne $interlockDensityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'reentrySurvivalCount') } else { $null }
        interlockDensityDurableAlignmentCount = if ($null -ne $interlockDensityLedgerState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'durableAlignmentCount') } else { $null }
        interlockDensityDisposition = if ($null -ne $interlockDensityLedgerState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'interlockDensityDisposition') } else { $null }
        interlockDensityDenseInterweaveEmergent = if ($null -ne $interlockDensityLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'denseInterweaveEmergent') } else { $null }
        interlockDensityLatticeClaimStillWithheld = if ($null -ne $interlockDensityLedgerState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'latticeClaimStillWithheld') } else { $null }
        brittleDurableDifferentiationSurfaceState = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] $brittleDurableDifferentiationSurfaceState.brittleDurableDifferentiationSurfaceState } else { $null }
        brittleDurableDifferentiationSurfaceReason = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] $brittleDurableDifferentiationSurfaceState.reasonCode } else { $null }
        brittleDurableDifferentiationSurfaceNextAction = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] $brittleDurableDifferentiationSurfaceState.nextAction } else { $null }
        brittleFragmentCount = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'brittleFragmentCount') } else { $null }
        durableKernelCount = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'durableKernelCount') } else { $null }
        coexistingRegionCount = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'coexistingRegionCount') } else { $null }
        brittleDurableSurfaceDisposition = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'surfaceDisposition') } else { $null }
        brittleDurableCoexistenceExposed = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'brittleDurableCoexistenceExposed') } else { $null }
        averageReadinessDenied = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'averageReadinessDenied') } else { $null }
        fullTrustStillWithheld = if ($null -ne $brittleDurableDifferentiationSurfaceState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'fullTrustStillWithheld') } else { $null }
        coreInvariantLatticeWitnessState = if ($null -ne $coreInvariantLatticeWitnessState) { [string] $coreInvariantLatticeWitnessState.coreInvariantLatticeWitnessState } else { $null }
        coreInvariantLatticeWitnessReason = if ($null -ne $coreInvariantLatticeWitnessState) { [string] $coreInvariantLatticeWitnessState.reasonCode } else { $null }
        coreInvariantLatticeWitnessNextAction = if ($null -ne $coreInvariantLatticeWitnessState) { [string] $coreInvariantLatticeWitnessState.nextAction } else { $null }
        candidateCoreInvariantCount = if ($null -ne $coreInvariantLatticeWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'candidateCoreInvariantCount') } else { $null }
        identityAdjacencySignalCount = if ($null -ne $coreInvariantLatticeWitnessState) { [int] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'identityAdjacencySignalCount') } else { $null }
        coreInvariantInterlockPosture = if ($null -ne $coreInvariantLatticeWitnessState) { [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'interlockPosture') } else { $null }
        identityAdjacentSignificanceEmergent = if ($null -ne $coreInvariantLatticeWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'identityAdjacentSignificanceEmergent') } else { $null }
        coreLawSanctificationDenied = if ($null -ne $coreInvariantLatticeWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'coreLawSanctificationDenied') } else { $null }
        latticeGradeInvarianceWitnessed = if ($null -ne $coreInvariantLatticeWitnessState) { [bool] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'latticeGradeInvarianceWitnessed') } else { $null }
        nextReleaseCandidateRunUtc = if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { $null }
        nextMandatoryHitlReviewUtc = if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { $null }
    }
    orchestration = [ordered]@{
        state = if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.orchestrationState } else { $null }
        reasonCode = if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.reasonCode } else { $null }
        nextAction = if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.nextAction } else { $null }
        codexRunOnceSupportState = if ($null -ne $masterThreadOrchestrationState) { [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'codexAutomationSupport') -PropertyName 'supportState') } else { $null }
        movementAdmissibilityState = if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.movementAdmissibilityState } else { $null }
        eligibleTargetBucketLabels = if ($null -ne $masterThreadOrchestrationState) { @($masterThreadOrchestrationState.eligibleTargetBucketLabels) } else { @() }
        instructionCount = if ($null -ne $masterThreadOrchestrationState) { [int] $masterThreadOrchestrationState.instructionCount } else { 0 }
        queuedInstructionCount = if ($null -ne $masterThreadOrchestrationState) { [int] $masterThreadOrchestrationState.queuedInstructionCount } else { 0 }
        releasableInstructionCount = if ($null -ne $masterThreadOrchestrationState) { [int] $masterThreadOrchestrationState.releasableInstructionCount } else { 0 }
        latestInstructionId = if ($null -ne $masterThreadOrchestrationState) { [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'latestInstruction') -PropertyName 'instructionId') } else { $null }
        latestInstructionState = if ($null -ne $masterThreadOrchestrationState) { [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'latestInstruction') -PropertyName 'effectiveLifecycleState') } else { $null }
    }
    tasks = $taskEntries
}

if ($null -ne $runIsolatedBuildPathwayState -and $null -ne $statusPayload.longFormTasking) {
    $runIsolatedTaskMap = @(
        @($statusPayload.longFormTasking.taskMaps) |
        Where-Object { [string] $_.id -eq 'automation-maturation-map-38' } |
        Select-Object -First 1
    )
    if ($runIsolatedTaskMap -is [System.Array]) {
        $runIsolatedTaskMap = if ($runIsolatedTaskMap.Count -gt 0) { $runIsolatedTaskMap[0] } else { $null }
    }

    if ($null -ne $runIsolatedTaskMap) {
        foreach ($taskEntry in @($runIsolatedTaskMap.tasks)) {
            $taskEntry.liveStatus = Resolve-RunIsolatedBuildTaskLiveStatus `
                -TaskId ([string] $taskEntry.id) `
                -CurrentLiveStatus ([string] $taskEntry.liveStatus) `
                -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState
        }

        $runIsolatedTaskMap.completedTaskCount = @(
            @($runIsolatedTaskMap.tasks) |
            Where-Object { [string] $_.liveStatus -eq 'completed' }
        ).Count

        if ([string] $statusPayload.longFormTasking.activeTaskMapId -eq 'automation-maturation-map-38') {
            $statusPayload.longFormTasking.activeTaskMapCompletedTaskCount = $runIsolatedTaskMap.completedTaskCount
            $statusPayload.longFormTasking.activeTaskMapTotalTaskCount = @($runIsolatedTaskMap.tasks).Count
        }
    }
}

Add-AutomationCascadeOperatorPromptProperty -InputObject $statusPayload | Out-Null

Write-JsonFile -Path $statusJsonPath -Value $statusPayload

$markdownLines = @(
    '# Local Automation Tasking Status',
    '',
    ('- Generated at (UTC): `{0}`' -f $statusPayload.generatedAtUtc),
    ('- Formal tasking surface: `{0}`' -f $statusPayload.formalSurfaceMarkdownPath),
    ('- Active long-form task map: `{0}`' -f $activeTaskMapId),
    ('- Active map posture: `{0}`' -f $activeLongFormTaskMapStatus),
    ('- Pull-forward allowed from next map: `{0}`' -f $canPullForwardFromNextMap),
    ('- Queued next maps: `{0}`' -f $(if (@($queuedBatchTaskMaps).Count -gt 0) { ((@($queuedBatchTaskMaps | ForEach-Object { [string] $_.label }) -join ' -> ')) } else { 'none' })),
    ('- Current posture: `{0}`' -f $lastKnownStatus),
    ('- Current action class: `{0}`' -f $currentActionClass),
    ('- Main worker terminal state: `{0}`' -f $(if (-not [string]::IsNullOrWhiteSpace($mainWorkerTerminalState)) { $mainWorkerTerminalState } else { 'uninitialized' })),
    ('- Main worker arm state: `{0}`' -f $(if (-not [string]::IsNullOrWhiteSpace($mainWorkerArmState)) { $mainWorkerArmState } else { 'uninitialized' })),
    ('- Recommended action: `{0}`' -f $recommendedAction),
    ('- Requires immediate HITL: `{0}`' -f $requiresImmediateHitl),
    ('- Active control notice: `{0}`' -f $(if (-not [string]::IsNullOrWhiteSpace($currentNoticeTypeFromCycle)) { ('{0} ({1})' -f $currentNoticeTypeFromCycle, $currentNoticeStatusFromCycle) } else { 'uninitialized' })),
    ('- Master-thread orchestration: `{0}`' -f $(if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.orchestrationState } else { 'uninitialized' })),
    ('- Orchestration next action: `{0}`' -f $(if ($null -ne $masterThreadOrchestrationState) { [string] $masterThreadOrchestrationState.nextAction } else { 'uninitialized' })),
    ('- Next main-worker wake (UTC): `{0}`' -f $(if ($null -ne $nextMainWorkerWakeUtc) { $nextMainWorkerWakeUtc.ToString('o') } else { 'not-scheduled' })),
    ('- Next watchdog run (UTC): `{0}`' -f $(if ($null -ne $nextWatchdogRunUtc) { $nextWatchdogRunUtc.ToString('o') } else { 'not-scheduled' })),
    ('- Next daily digest run (UTC): `{0}`' -f $(if ($null -ne $nextDailyHitlDigestRunUtc) { $nextDailyHitlDigestRunUtc.ToString('o') } else { 'not-scheduled' })),
    ('- Next release-candidate run (UTC): `{0}`' -f $(if ($null -ne $nextReleaseCandidateRunUtc) { $nextReleaseCandidateRunUtc.ToString('o') } else { 'uninitialized' })),
    ('- Next mandatory HITL review (UTC): `{0}`' -f $(if ($null -ne $nextMandatoryHitlReviewUtc) { $nextMandatoryHitlReviewUtc.ToString('o') } else { 'uninitialized' })),
    ''
)

$markdownLines += @(
    '## Scheduler',
    '',
    ('- Main worker task: `{0}`' -f $scheduler.mainWorker.taskName),
    ('- Main worker registered: `{0}`' -f $scheduler.mainWorker.registered),
    ('- Main worker state: `{0}`' -f $scheduler.mainWorker.state),
    ('- Main worker last run (UTC): `{0}`' -f $(if ($scheduler.mainWorker.lastRunTimeUtc) { $scheduler.mainWorker.lastRunTimeUtc } else { 'not-yet-run' })),
    ('- Main worker next run (UTC): `{0}`' -f $(if ($scheduler.mainWorker.nextRunTimeUtc) { $scheduler.mainWorker.nextRunTimeUtc } else { 'not-scheduled' })),
    ('- Watchdog task: `{0}`' -f $scheduler.watchdog.taskName),
    ('- Watchdog registered: `{0}`' -f $scheduler.watchdog.registered),
    ('- Watchdog state: `{0}`' -f $scheduler.watchdog.state),
    ('- Watchdog last run (UTC): `{0}`' -f $(if ($scheduler.watchdog.lastRunTimeUtc) { $scheduler.watchdog.lastRunTimeUtc } else { 'not-yet-run' })),
    ('- Watchdog next run (UTC): `{0}`' -f $(if ($scheduler.watchdog.nextRunTimeUtc) { $scheduler.watchdog.nextRunTimeUtc } else { 'not-scheduled' })),
    ('- Daily digest task: `{0}`' -f $scheduler.dailyDigest.taskName),
    ('- Daily digest registered: `{0}`' -f $scheduler.dailyDigest.registered),
    ('- Daily digest state: `{0}`' -f $scheduler.dailyDigest.state),
    ('- Daily digest last run (UTC): `{0}`' -f $(if ($scheduler.dailyDigest.lastRunTimeUtc) { $scheduler.dailyDigest.lastRunTimeUtc } else { 'not-yet-run' })),
    ('- Daily digest next run (UTC): `{0}`' -f $(if ($scheduler.dailyDigest.nextRunTimeUtc) { $scheduler.dailyDigest.nextRunTimeUtc } else { 'not-scheduled' })),
    ''
)

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
        ('- Action taken: `{0}`' -f ((@($schedulerReconciliationState.actionTaken | ForEach-Object { [string] $_ }) -join '`, `'))),
        ('- Aligned: `{0}`' -f [bool] $schedulerReconciliationState.aligned),
        ('- Main worker terminal state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $schedulerReconciliationState -PropertyName 'mainWorker') -PropertyName 'terminalState')),
        ('- Main worker final arm state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $schedulerReconciliationState -PropertyName 'mainWorker') -PropertyName 'finalArmState')),
        ('- Main worker final next wake (UTC): `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $schedulerReconciliationState -PropertyName 'mainWorker') -PropertyName 'finalNextWakeUtc')),
        ('- Watchdog next run (UTC): `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $schedulerReconciliationState -PropertyName 'watchdog') -PropertyName 'finalNextRunUtc')),
        ('- Daily digest next run (UTC): `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $schedulerReconciliationState -PropertyName 'dailyDigest') -PropertyName 'finalNextRunUtc')),
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

if ($null -ne $cmeFormationAndOfficeLedgerState) {
    $markdownLines += @(
        '## CME Formation and Office Ledger',
        '',
        ('- Ledger state: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.ledgerState),
        ('- Reason code: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.nextAction),
        ('- Capability ledger state: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.capabilityLedgerState),
        ('- Formation ledger state: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.formationLedgerState),
        ('- Office ledger state: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.officeLedgerState),
        ('- Career continuity state: `{0}`' -f [string] $cmeFormationAndOfficeLedgerState.careerContinuityLedgerState),
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

if ($null -ne $unattendedProofCollapseState) {
    $markdownLines += @(
        '## Unattended Proof Collapse',
        '',
        ('- Collapse state: `{0}`' -f [string] $unattendedProofCollapseState.collapseState),
        ('- Reason code: `{0}`' -f [string] $unattendedProofCollapseState.reasonCode),
        ('- Next action: `{0}`' -f [string] $unattendedProofCollapseState.nextAction),
        ''
    )
}

if ($null -ne $dormantWindowLedgerState) {
    $markdownLines += @(
        '## Dormant Window Ledger',
        '',
        ('- Ledger state: `{0}`' -f [string] $dormantWindowLedgerState.ledgerState),
        ('- Reason code: `{0}`' -f [string] $dormantWindowLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $dormantWindowLedgerState.nextAction),
        ('- Consecutive dormant windows: `{0}`' -f [int] $dormantWindowLedgerState.consecutiveDormantWindows),
        ''
    )
}

if ($null -ne $silentCadenceIntegrityState) {
    $markdownLines += @(
        '## Silent Cadence Integrity',
        '',
        ('- Integrity state: `{0}`' -f [string] $silentCadenceIntegrityState.integrityState),
        ('- Reason code: `{0}`' -f [string] $silentCadenceIntegrityState.reasonCode),
        ('- Next action: `{0}`' -f [string] $silentCadenceIntegrityState.nextAction),
        ''
    )
}

if ($null -ne $longFormPhaseWitnessState) {
    $markdownLines += @(
        '## Long-Form Phase Witness',
        '',
        ('- Witness state: `{0}`' -f [string] $longFormPhaseWitnessState.witnessState),
        ('- Reason code: `{0}`' -f [string] $longFormPhaseWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $longFormPhaseWitnessState.nextAction),
        ('- Target phase: `{0}`' -f [string] $longFormPhaseWitnessState.targetPhaseLabel),
        ''
    )
}

if ($null -ne $longFormWindowBoundaryState) {
    $markdownLines += @(
        '## Long-Form Window Boundary',
        '',
        ('- Boundary state: `{0}`' -f [string] $longFormWindowBoundaryState.boundaryState),
        ('- Reason code: `{0}`' -f [string] $longFormWindowBoundaryState.reasonCode),
        ('- Next action: `{0}`' -f [string] $longFormWindowBoundaryState.nextAction),
        ('- Minutes remaining: `{0}`' -f $(if ($null -ne $longFormWindowBoundaryState.minutesRemaining) { [string] $longFormWindowBoundaryState.minutesRemaining } else { 'unknown' })),
        ''
    )
}

if ($null -ne $autonomousLongFormCollapseState) {
    $markdownLines += @(
        '## Autonomous Long-Form Collapse',
        '',
        ('- Collapse state: `{0}`' -f [string] $autonomousLongFormCollapseState.collapseState),
        ('- Reason code: `{0}`' -f [string] $autonomousLongFormCollapseState.reasonCode),
        ('- Next action: `{0}`' -f [string] $autonomousLongFormCollapseState.nextAction),
        ('- Run status: `{0}`' -f [string] $autonomousLongFormCollapseState.runStatus),
        ('- Current phase: `{0}`' -f [string] $autonomousLongFormCollapseState.currentPhaseLabel),
        ''
    )
}

if ($null -ne $schedulerProofHarvestState) {
    $markdownLines += @(
        '## Scheduler Proof Harvest',
        '',
        ('- Harvest state: `{0}`' -f [string] $schedulerProofHarvestState.harvestState),
        ('- Reason code: `{0}`' -f [string] $schedulerProofHarvestState.reasonCode),
        ('- Next action: `{0}`' -f [string] $schedulerProofHarvestState.nextAction),
        ''
    )
}

if ($null -ne $intervalOriginClarificationState) {
    $markdownLines += @(
        '## Interval Origin Clarification',
        '',
        ('- Origin state: `{0}`' -f [string] $intervalOriginClarificationState.originState),
        ('- Reason code: `{0}`' -f [string] $intervalOriginClarificationState.reasonCode),
        ('- Next action: `{0}`' -f [string] $intervalOriginClarificationState.nextAction),
        ''
    )
}

if ($null -ne $queuedTaskMapPromotionState) {
    $markdownLines += @(
        '## Queued Task Map Promotion',
        '',
        ('- Promotion state: `{0}`' -f [string] $queuedTaskMapPromotionState.promotionState),
        ('- Reason code: `{0}`' -f [string] $queuedTaskMapPromotionState.reasonCode),
        ('- Next action: `{0}`' -f [string] $queuedTaskMapPromotionState.nextAction),
        ('- Promoted: `{0}`' -f [bool] $queuedTaskMapPromotionState.promoted),
        ''
    )
}

if ($null -ne $masterThreadOrchestrationState) {
    $eligibleBucketLabels = @($masterThreadOrchestrationState.eligibleTargetBucketLabels)
    $latestInstruction = Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'latestInstruction'

    $markdownLines += @(
        '## Master Thread Orchestration',
        '',
        ('- Orchestration state: `{0}`' -f [string] $masterThreadOrchestrationState.orchestrationState),
        ('- Reason code: `{0}`' -f [string] $masterThreadOrchestrationState.reasonCode),
        ('- Next action: `{0}`' -f [string] $masterThreadOrchestrationState.nextAction),
        ('- Codex run-once support: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'codexAutomationSupport') -PropertyName 'supportState')),
        ('- Movement admissibility: `{0}`' -f [string] $masterThreadOrchestrationState.movementAdmissibilityState),
        ('- Eligible target buckets: `{0}`' -f $(if ($eligibleBucketLabels.Count -gt 0) { ($eligibleBucketLabels -join '`, `') } else { 'none' })),
        ('- Total instructions: `{0}`' -f [int] $masterThreadOrchestrationState.instructionCount),
        ('- Queued instructions: `{0}`' -f [int] $masterThreadOrchestrationState.queuedInstructionCount),
        ('- Releasable instructions: `{0}`' -f [int] $masterThreadOrchestrationState.releasableInstructionCount),
        ('- Latest instruction id: `{0}`' -f $(if ($null -ne $latestInstruction) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestInstruction -PropertyName 'instructionId') } else { 'none' })),
        ('- Latest instruction state: `{0}`' -f $(if ($null -ne $latestInstruction) { [string] (Get-ObjectPropertyValueOrNull -InputObject $latestInstruction -PropertyName 'effectiveLifecycleState') } else { 'none' })),
        ''
    )
}

if ($null -ne $runtimeDeployabilityEnvelopeState) {
    $markdownLines += @(
        '## Runtime Deployability Envelope',
        '',
        ('- Envelope state: `{0}`' -f [string] $runtimeDeployabilityEnvelopeState.envelopeState),
        ('- Reason code: `{0}`' -f [string] $runtimeDeployabilityEnvelopeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $runtimeDeployabilityEnvelopeState.nextAction),
        ('- Candidate status: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityEnvelopeState -PropertyName 'candidateStatus')),
        ('- Ready promotable deployables: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityEnvelopeState -PropertyName 'readyPromotableDeployableCount')),
        ''
    )
}

if ($null -ne $sanctuaryRuntimeReadinessState) {
    $markdownLines += @(
        '## Sanctuary Runtime Readiness',
        '',
        ('- Readiness state: `{0}`' -f [string] $sanctuaryRuntimeReadinessState.readinessState),
        ('- Reason code: `{0}`' -f [string] $sanctuaryRuntimeReadinessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $sanctuaryRuntimeReadinessState.nextAction),
        ('- Working state class: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'workingStateClass')),
        ('- CME office state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'cmeOfficeLedgerState')),
        ''
    )
}

if ($null -ne $runtimeWorkSurfaceAdmissibilityState) {
    $markdownLines += @(
        '## Runtime Work Surface Admissibility',
        '',
        ('- Admissibility state: `{0}`' -f [string] $runtimeWorkSurfaceAdmissibilityState.admissibilityState),
        ('- Reason code: `{0}`' -f [string] $runtimeWorkSurfaceAdmissibilityState.reasonCode),
        ('- Next action: `{0}`' -f [string] $runtimeWorkSurfaceAdmissibilityState.nextAction),
        ('- Admissible surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibleSurfaceCount')),
        ('- Denied surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'deniedSurfaceCount')),
        ''
    )
}

if ($null -ne $reachAccessTopologyLedgerState) {
    $markdownLines += @(
        '## Reach Access Topology Ledger',
        '',
        ('- Ledger state: `{0}`' -f [string] $reachAccessTopologyLedgerState.ledgerState),
        ('- Reason code: `{0}`' -f [string] $reachAccessTopologyLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $reachAccessTopologyLedgerState.nextAction),
        ('- Disclosed surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'disclosedSurfaceCount')),
        ('- Denied surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'deniedSurfaceCount')),
        ('- Inventory entries: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'inventoryEntryCount')),
        ''
    )
}

if ($null -ne $bondedOperatorLocalityReadinessState) {
    $markdownLines += @(
        '## Bonded Operator Locality Readiness',
        '',
        ('- Readiness state: `{0}`' -f [string] $bondedOperatorLocalityReadinessState.readinessState),
        ('- Reason code: `{0}`' -f [string] $bondedOperatorLocalityReadinessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $bondedOperatorLocalityReadinessState.nextAction),
        ('- Runtime readiness state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedOperatorLocalityReadinessState -PropertyName 'runtimeReadinessState')),
        ('- Reach ledger state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedOperatorLocalityReadinessState -PropertyName 'reachLedgerState')),
        ''
    )
}

if ($null -ne $protectedStateLegibilitySurfaceState) {
    $markdownLines += @(
        '## Protected State Legibility Surface',
        '',
        ('- Legibility state: `{0}`' -f [string] $protectedStateLegibilitySurfaceState.legibilityState),
        ('- Reason code: `{0}`' -f [string] $protectedStateLegibilitySurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $protectedStateLegibilitySurfaceState.nextAction),
        ('- Protected interiority exposure: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedStateLegibilitySurfaceState -PropertyName 'protectedInteriorityExposure')),
        ('- Visible signals: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedStateLegibilitySurfaceState -PropertyName 'visibleSignalCount')),
        ''
    )
}

if ($null -ne $nexusSingularPortalFacadeState) {
    $markdownLines += @(
        '## Nexus Singular Portal Facade',
        '',
        ('- Portal state: `{0}`' -f [string] $nexusSingularPortalFacadeState.portalState),
        ('- Reason code: `{0}`' -f [string] $nexusSingularPortalFacadeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $nexusSingularPortalFacadeState.nextAction),
        ('- Protected legibility state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'protectedLegibilityState')),
        ('- Source files present: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nexusSingularPortalFacadeState -PropertyName 'sourceFileCount')),
        ''
    )
}

if ($null -ne $duplexPredicateEnvelopeState) {
    $markdownLines += @(
        '## Duplex Predicate Envelope',
        '',
        ('- Duplex state: `{0}`' -f [string] $duplexPredicateEnvelopeState.duplexState),
        ('- Reason code: `{0}`' -f [string] $duplexPredicateEnvelopeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $duplexPredicateEnvelopeState.nextAction),
        ('- Runtime admissibility state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'runtimeAdmissibilityState')),
        ('- Nexus portal state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'nexusPortalState')),
        ('- Work predicate bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'workPredicateBound')),
        ('- Governance predicate bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $duplexPredicateEnvelopeState -PropertyName 'governancePredicateBound')),
        ''
    )
}

if ($null -ne $operatorActualWorkSessionRehearsalState) {
    $markdownLines += @(
        '## Operator.actual Work Session Rehearsal',
        '',
        ('- Rehearsal state: `{0}`' -f [string] $operatorActualWorkSessionRehearsalState.rehearsalState),
        ('- Reason code: `{0}`' -f [string] $operatorActualWorkSessionRehearsalState.reasonCode),
        ('- Next action: `{0}`' -f [string] $operatorActualWorkSessionRehearsalState.nextAction),
        ('- Reach ledger state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'reachLedgerState')),
        ('- Nexus portal state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'nexusPortalState')),
        ('- Duplex state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'duplexState')),
        ('- Co-realized surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'coRealizedSurfaceCount')),
        ('- Withheld surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorActualWorkSessionRehearsalState -PropertyName 'withheldSurfaceCount')),
        ''
    )
}

if ($null -ne $identityInvariantThreadRootState) {
    $markdownLines += @(
        '## Identity-Invariant Thread Root',
        '',
        ('- Thread-root state: `{0}`' -f [string] $identityInvariantThreadRootState.threadRootState),
        ('- Reason code: `{0}`' -f [string] $identityInvariantThreadRootState.reasonCode),
        ('- Next action: `{0}`' -f [string] $identityInvariantThreadRootState.nextAction),
        ('- Runtime readiness state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'runtimeReadinessState')),
        ('- Thread-root projection bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'threadRootProjectionBound')),
        ('- Service binding bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'serviceBindingBound')),
        ''
    )
}

if ($null -ne $governedThreadBirthReceiptState) {
    $markdownLines += @(
        '## Governed Thread Birth Receipt',
        '',
        ('- Receipt state: `{0}`' -f [string] $governedThreadBirthReceiptState.receiptState),
        ('- Reason code: `{0}`' -f [string] $governedThreadBirthReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $governedThreadBirthReceiptState.nextAction),
        ('- Thread-root state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'threadRootState')),
        ('- Nexus portal state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'nexusPortalState')),
        ('- Witnessed offices: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'witnessedOfficeCount')),
        ''
    )
}

if ($null -ne $interWorkerBraidHandoffPacketState) {
    $markdownLines += @(
        '## Inter-Worker Braid Handoff Packet',
        '',
        ('- Packet state: `{0}`' -f [string] $interWorkerBraidHandoffPacketState.packetState),
        ('- Reason code: `{0}`' -f [string] $interWorkerBraidHandoffPacketState.reasonCode),
        ('- Next action: `{0}`' -f [string] $interWorkerBraidHandoffPacketState.nextAction),
        ('- Thread-birth state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interWorkerBraidHandoffPacketState -PropertyName 'threadBirthState')),
        ('- Duplex state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interWorkerBraidHandoffPacketState -PropertyName 'duplexState')),
        ('- Identity inheritance denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interWorkerBraidHandoffPacketState -PropertyName 'identityInheritanceDenied')),
        ''
    )
}

if ($null -ne $agentiCoreActualUtilitySurfaceState) {
    $markdownLines += @(
        '## AgentiCore.actual Utility Surface',
        '',
        ('- Utility state: `{0}`' -f [string] $agentiCoreActualUtilitySurfaceState.utilityState),
        ('- Reason code: `{0}`' -f [string] $agentiCoreActualUtilitySurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $agentiCoreActualUtilitySurfaceState.nextAction),
        ('- Utility posture: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'utilityPosture')),
        ('- Thread-birth state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'threadBirthState')),
        ('- Duplex state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'duplexState')),
        ''
    )
}

if ($null -ne $reachDuplexRealizationSeamState) {
    $markdownLines += @(
        '## Reach Duplex Realization Seam',
        '',
        ('- Seam state: `{0}`' -f [string] $reachDuplexRealizationSeamState.seamState),
        ('- Reason code: `{0}`' -f [string] $reachDuplexRealizationSeamState.reasonCode),
        ('- Next action: `{0}`' -f [string] $reachDuplexRealizationSeamState.nextAction),
        ('- Utility state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'utilityState')),
        ('- Reach topology state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'reachAccessTopologyState')),
        ('- Protected legibility state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'protectedLegibilityState')),
        ('- Grant implied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'grantImplied')),
        ''
    )
}

if ($null -ne $bondedParticipationLocalityLedgerState) {
    $markdownLines += @(
        '## Bonded Participation Locality Ledger',
        '',
        ('- Ledger state: `{0}`' -f [string] $bondedParticipationLocalityLedgerState.ledgerState),
        ('- Reason code: `{0}`' -f [string] $bondedParticipationLocalityLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $bondedParticipationLocalityLedgerState.nextAction),
        ('- Reach seam state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'reachSeamState')),
        ('- Operator rehearsal state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'operatorRehearsalState')),
        ('- Operator locality state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'operatorLocalityState')),
        ('- Co-realized surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'coRealizedSurfaceCount')),
        ('- Withheld surfaces: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'withheldSurfaceCount')),
        ''
    )
}

if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) {
    $markdownLines += @(
        '## Sanctuary Runtime Workbench Surface',
        '',
        ('- Session state: `{0}`' -f [string] $sanctuaryRuntimeWorkbenchSurfaceState.workbenchState),
        ('- Reason code: `{0}`' -f [string] $sanctuaryRuntimeWorkbenchSurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction),
        ('- Session posture: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'sessionPosture')),
        ('- Bounded work class: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'boundedWorkClass')),
        ('- Bonded operator lane withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'bondedOperatorLaneWithheld')),
        ('- MoS-bearing release denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'mosBearingReleaseDenied')),
        ''
    )
}

if ($null -ne $amenableDayDreamTierAdmissibilityState) {
    $markdownLines += @(
        '## Amenable Day-Dream Tier Admissibility',
        '',
        ('- Admissibility state: `{0}`' -f [string] $amenableDayDreamTierAdmissibilityState.admissibilityState),
        ('- Reason code: `{0}`' -f [string] $amenableDayDreamTierAdmissibilityState.reasonCode),
        ('- Next action: `{0}`' -f [string] $amenableDayDreamTierAdmissibilityState.nextAction),
        ('- Exploratory only: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'exploratoryOnly')),
        ('- Identity-bearing descent denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'identityBearingDescentDenied')),
        ('- Continuity inflation denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'continuityInflationDenied')),
        ''
    )
}

if ($null -ne $selfRootedCrypticDepthGateState) {
    $markdownLines += @(
        '## Self-Rooted Cryptic Depth Gate',
        '',
        ('- Gate state: `{0}`' -f [string] $selfRootedCrypticDepthGateState.gateState),
        ('- Reason code: `{0}`' -f [string] $selfRootedCrypticDepthGateState.reasonCode),
        ('- Next action: `{0}`' -f [string] $selfRootedCrypticDepthGateState.nextAction),
        ('- Gate mode: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'gateMode')),
        ('- Cryptic biad rooted: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'crypticBiadRooted')),
        ('- Shared amenable origin denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'sharedAmenableOriginDenied')),
        ('- Deep access granted: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'deepAccessGranted')),
        ''
    )
}

if ($null -ne $runtimeWorkbenchSessionLedgerState) {
    $markdownLines += @(
        '## Runtime Workbench Session Ledger',
        '',
        ('- Session-ledger state: `{0}`' -f [string] $runtimeWorkbenchSessionLedgerState.sessionLedgerState),
        ('- Reason code: `{0}`' -f [string] $runtimeWorkbenchSessionLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $runtimeWorkbenchSessionLedgerState.nextAction),
        ('- Session state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionState')),
        ('- Admitted lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'admittedLaneCount')),
        ('- Withheld lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'withheldLaneCount')),
        ('- Session event count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionEventCount')),
        ('- Boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'boundaryConditionCount')),
        ''
    )
}

if ($null -ne $dayDreamCollapseReceiptState) {
    $markdownLines += @(
        '## Day-Dream Collapse Receipt',
        '',
        ('- Collapse-receipt state: `{0}`' -f [string] $dayDreamCollapseReceiptState.collapseReceiptState),
        ('- Reason code: `{0}`' -f [string] $dayDreamCollapseReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $dayDreamCollapseReceiptState.nextAction),
        ('- Collapse state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'collapseState')),
        ('- Considered predicate count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'consideredPredicateCount')),
        ('- Bounded output count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'boundedOutputCount')),
        ('- Remaining non-final output count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'remainingNonFinalOutputCount')),
        ('- Exploratory provenance preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'exploratoryProvenancePreserved')),
        ('- Boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'boundaryConditionCount')),
        ('- Residue-marker count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'residueMarkerCount')),
        ''
    )
}

if ($null -ne $crypticDepthReturnReceiptState) {
    $markdownLines += @(
        '## Cryptic Depth Return Receipt',
        '',
        ('- Return-receipt state: `{0}`' -f [string] $crypticDepthReturnReceiptState.returnReceiptState),
        ('- Reason code: `{0}`' -f [string] $crypticDepthReturnReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $crypticDepthReturnReceiptState.nextAction),
        ('- Return state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'returnState')),
        ('- Continuity-marker count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'continuityMarkerCount')),
        ('- Residue-marker count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'residueMarkerCount')),
        ('- Boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'boundaryConditionCount')),
        ('- Returned cleanly: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'returnedCleanly')),
        ('- Shared amenable lane clear: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'sharedAmenableLaneClear')),
        ('- Identity bleed detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $crypticDepthReturnReceiptState -PropertyName 'identityBleedDetected')),
        ''
    )
}

if ($null -ne $bondedCoWorkSessionRehearsalState) {
    $markdownLines += @(
        '## Bonded Co-Work Session Rehearsal',
        '',
        ('- Rehearsal state: `{0}`' -f [string] $bondedCoWorkSessionRehearsalState.rehearsalReceiptState),
        ('- Reason code: `{0}`' -f [string] $bondedCoWorkSessionRehearsalState.reasonCode),
        ('- Next action: `{0}`' -f [string] $bondedCoWorkSessionRehearsalState.nextAction),
        ('- Shared work-loop count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'sharedWorkLoopCount')),
        ('- Duplex predicate-lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'duplexPredicateLaneCount')),
        ('- Withheld lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'withheldLaneCount')),
        ('- Remote control denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'remoteControlDenied')),
        ('- Locality collapse denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'localityCollapseDenied')),
        ''
    )
}

if ($null -ne $reachReturnDissolutionReceiptState) {
    $markdownLines += @(
        '## Reach Return Dissolution Receipt',
        '',
        ('- Return-receipt state: `{0}`' -f [string] $reachReturnDissolutionReceiptState.returnReceiptState),
        ('- Reason code: `{0}`' -f [string] $reachReturnDissolutionReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $reachReturnDissolutionReceiptState.nextAction),
        ('- Return state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnState')),
        ('- Dissolution state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'dissolutionState')),
        ('- Bonded event returned: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'bondedEventReturned')),
        ('- Bonded event dissolved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'bondedEventDissolved')),
        ('- Ambient grant denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'ambientGrantDenied')),
        ('- Locality distinction preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'localityDistinctionPreserved')),
        ''
    )
}

if ($null -ne $localityDistinctionWitnessLedgerState) {
    $markdownLines += @(
        '## Locality Distinction Witness Ledger',
        '',
        ('- Witness-ledger state: `{0}`' -f [string] $localityDistinctionWitnessLedgerState.witnessLedgerState),
        ('- Reason code: `{0}`' -f [string] $localityDistinctionWitnessLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $localityDistinctionWitnessLedgerState.nextAction),
        ('- Shared surface count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'sharedSurfaceCount')),
        ('- Sanctuary-local surface count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'sanctuaryLocalSurfaceCount')),
        ('- Operator-local surface count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'operatorLocalSurfaceCount')),
        ('- Withheld surface count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'withheldSurfaceCount')),
        ('- Locality collapse detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'localityCollapseDetected')),
        ('- Projection theater denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'projectionTheaterDenied')),
        ''
    )
}

if ($null -ne $localHostSanctuaryResidencyEnvelopeState) {
    $markdownLines += @(
        '## Local Host Sanctuary Residency Envelope',
        '',
        ('- Envelope state: `{0}`' -f [string] $localHostSanctuaryResidencyEnvelopeState.envelopeState),
        ('- Reason code: `{0}`' -f [string] $localHostSanctuaryResidencyEnvelopeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $localHostSanctuaryResidencyEnvelopeState.nextAction),
        ('- Residency state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'residencyState')),
        ('- Residency class: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'residencyClass')),
        ('- Host-local resource count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'hostLocalResourceCount')),
        ('- Admitted residency-lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'admittedResidencyLaneCount')),
        ('- Withheld residency-lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'withheldResidencyLaneCount')),
        ('- Bonded release denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'bondedReleaseDenied')),
        ('- Publication maturity denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'publicationMaturityDenied')),
        ('- MoS-bearing depth denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $localHostSanctuaryResidencyEnvelopeState -PropertyName 'mosBearingDepthDenied')),
        ''
    )
}

if ($null -ne $runtimeHabitationReadinessLedgerState) {
    $markdownLines += @(
        '## Runtime Habitation Readiness Ledger',
        '',
        ('- Readiness-ledger state: `{0}`' -f [string] $runtimeHabitationReadinessLedgerState.readinessLedgerState),
        ('- Reason code: `{0}`' -f [string] $runtimeHabitationReadinessLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $runtimeHabitationReadinessLedgerState.nextAction),
        ('- Habitation state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'habitationState')),
        ('- Habitation class: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'habitationClass')),
        ('- Ready-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'readyConditionCount')),
        ('- Withheld-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'withheldConditionCount')),
        ('- Recurring work ready: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'recurringWorkReady')),
        ('- Return law bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'returnLawBound')),
        ('- Bonded release denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'bondedReleaseDenied')),
        ('- Publication maturity denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'publicationMaturityDenied')),
        ('- MoS-bearing depth denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeHabitationReadinessLedgerState -PropertyName 'mosBearingDepthDenied')),
        ''
    )
}

if ($null -ne $boundedInhabitationLaunchRehearsalState) {
    $markdownLines += @(
        '## Bounded Inhabitation Launch Rehearsal',
        '',
        ('- Launch-rehearsal state: `{0}`' -f [string] $boundedInhabitationLaunchRehearsalState.launchRehearsalState),
        ('- Reason code: `{0}`' -f [string] $boundedInhabitationLaunchRehearsalState.reasonCode),
        ('- Next action: `{0}`' -f [string] $boundedInhabitationLaunchRehearsalState.nextAction),
        ('- Launch state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'launchState')),
        ('- Entry-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'entryConditionCount')),
        ('- Denied-lane count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'deniedLaneCount')),
        ('- Return-closure state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'returnClosureState')),
        ('- Launch bounded: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'launchBounded')),
        ('- Return closure witnessed: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'returnClosureWitnessed')),
        ('- Ambient bond denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'ambientBondDenied')),
        ('- Publication promotion denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedInhabitationLaunchRehearsalState -PropertyName 'publicationPromotionDenied')),
        ''
    )
}

if ($null -ne $postHabitationHorizonLatticeState) {
    $markdownLines += @(
        '## Post-Habitation Horizon Lattice',
        '',
        ('- Lattice state: `{0}`' -f [string] $postHabitationHorizonLatticeState.latticeState),
        ('- Reason code: `{0}`' -f [string] $postHabitationHorizonLatticeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $postHabitationHorizonLatticeState.nextAction),
        ('- Anchor-receipt count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'anchorReceiptCount')),
        ('- Candidate-horizon count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'candidateHorizonCount')),
        ('- Withheld-expansion count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'withheldExpansionCount')),
        ('- Research cycle bounded: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $postHabitationHorizonLatticeState -PropertyName 'researchCycleBounded')),
        ''
    )
}

if ($null -ne $boundedHorizonResearchBriefState) {
    $markdownLines += @(
        '## Bounded Horizon Research Brief',
        '',
        ('- Research-brief state: `{0}`' -f [string] $boundedHorizonResearchBriefState.researchBriefState),
        ('- Reason code: `{0}`' -f [string] $boundedHorizonResearchBriefState.reasonCode),
        ('- Next action: `{0}`' -f [string] $boundedHorizonResearchBriefState.nextAction),
        ('- Primary pressure point: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'primaryPressurePoint')),
        ('- Queued horizon count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'queuedHorizonCount')),
        ('- Withheld-expansion count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'withheldExpansionCount')),
        ('- Bonded release still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'bondedReleaseStillWithheld')),
        ('- Publication maturity still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'publicationMaturityStillWithheld')),
        ('- MoS-bearing depth still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundedHorizonResearchBriefState -PropertyName 'mosBearingDepthStillWithheld')),
        ''
    )
}

if ($null -ne $nextEraBatchSelectorState) {
    $markdownLines += @(
        '## Next Era Batch Selector',
        '',
        ('- Selector state: `{0}`' -f [string] $nextEraBatchSelectorState.selectorState),
        ('- Reason code: `{0}`' -f [string] $nextEraBatchSelectorState.reasonCode),
        ('- Next action: `{0}`' -f [string] $nextEraBatchSelectorState.nextAction),
        ('- Selected next map: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedNextMapId')),
        ('- Selected cluster: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedCluster')),
        ('- Queued map count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'queuedMapCount')),
        ('- Selection bounded to declared maps: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectionBoundedToDeclaredMaps')),
        ''
    )
}

if ($null -ne $inquirySessionDisciplineSurfaceState) {
    $markdownLines += @(
        '## Inquiry Session Discipline Surface',
        '',
        ('- Inquiry-surface state: `{0}`' -f [string] $inquirySessionDisciplineSurfaceState.inquirySurfaceState),
        ('- Reason code: `{0}`' -f [string] $inquirySessionDisciplineSurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $inquirySessionDisciplineSurfaceState.nextAction),
        ('- Inquiry state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'inquiryState')),
        ('- Inquiry-stance count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'inquiryStanceCount')),
        ('- Assumption-exposure mode count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'assumptionExposureModeCount')),
        ('- Silence-disposition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'silenceDispositionCount')),
        ('- Chamber-native inquiry bound: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'chamberNativeInquiryBound')),
        ('- Hidden pressure denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'hiddenPressureDenied')),
        ('- Premature GEL promotion denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySessionDisciplineSurfaceState -PropertyName 'prematureGelPromotionDenied')),
        ''
    )
}

if ($null -ne $boundaryConditionLedgerState) {
    $markdownLines += @(
        '## Boundary Condition Ledger',
        '',
        ('- Boundary-ledger state: `{0}`' -f [string] $boundaryConditionLedgerState.boundaryLedgerState),
        ('- Reason code: `{0}`' -f [string] $boundaryConditionLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $boundaryConditionLedgerState.nextAction),
        ('- Retained boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'retainedBoundaryConditionCount')),
        ('- Continuity-requirement count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'continuityRequirementCount')),
        ('- Withheld-crossing count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'withheldCrossingCount')),
        ('- Boundary memory carried forward: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'boundaryMemoryCarriedForward')),
        ('- Failure punishment denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'failurePunishmentDenied')),
        ('- Identity bleed detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'identityBleedDetected')),
        ''
    )
}

if ($null -ne $coherenceGainWitnessReceiptState) {
    $markdownLines += @(
        '## Coherence Gain Witness Receipt',
        '',
        ('- Coherence-witness state: `{0}`' -f [string] $coherenceGainWitnessReceiptState.coherenceWitnessState),
        ('- Reason code: `{0}`' -f [string] $coherenceGainWitnessReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $coherenceGainWitnessReceiptState.nextAction),
        ('- Coherence state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherenceState')),
        ('- Coherence-preserving event count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherencePreservingEventCount')),
        ('- Hidden-assumption denied count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'hiddenAssumptionDeniedCount')),
        ('- Boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'boundaryConditionCount')),
        ('- Shared intelligibility preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'sharedIntelligibilityPreserved')),
        ('- Admissibility space preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'admissibilitySpacePreserved')),
        ('- Premature closure detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'prematureClosureDetected')),
        ''
    )
}

if ($null -ne $operatorInquirySelectionEnvelopeState) {
    $markdownLines += @(
        '## Operator Inquiry Selection Envelope',
        '',
        ('- Selection-envelope state: `{0}`' -f [string] $operatorInquirySelectionEnvelopeState.operatorInquirySelectionEnvelopeState),
        ('- Reason code: `{0}`' -f [string] $operatorInquirySelectionEnvelopeState.reasonCode),
        ('- Next action: `{0}`' -f [string] $operatorInquirySelectionEnvelopeState.nextAction),
        ('- Operator inquiry-selection state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'operatorInquirySelectionState')),
        ('- Available inquiry stance count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'availableInquiryStanceCount')),
        ('- Known boundary warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'knownBoundaryWarningCount')),
        ('- Lawful use-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'lawfulUseConditionCount')),
        ('- Protected interiority denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'protectedInteriorityDenied')),
        ('- Locality bypass denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'localityBypassDenied')),
        ('- Raw grant denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $operatorInquirySelectionEnvelopeState -PropertyName 'rawGrantDenied')),
        ''
    )
}

if ($null -ne $bondedCrucibleSessionRehearsalState) {
    $markdownLines += @(
        '## Bonded Crucible Session Rehearsal',
        '',
        ('- Crucible-rehearsal state: `{0}`' -f [string] $bondedCrucibleSessionRehearsalState.bondedCrucibleSessionRehearsalState),
        ('- Reason code: `{0}`' -f [string] $bondedCrucibleSessionRehearsalState.reasonCode),
        ('- Next action: `{0}`' -f [string] $bondedCrucibleSessionRehearsalState.nextAction),
        ('- Crucible state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'crucibleState')),
        ('- Selected inquiry-stance count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'selectedInquiryStanceCount')),
        ('- Shared unknown-facet count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'sharedUnknownFacetCount')),
        ('- Coordination-hold count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'coordinationHoldCount')),
        ('- Exposed boundary count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'exposedBoundaryCount')),
        ('- Pre-scripted answer denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'preScriptedAnswerDenied')),
        ('- Remote dominance denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'remoteDominanceDenied')),
        ''
    )
}

if ($null -ne $sharedBoundaryMemoryLedgerState) {
    $markdownLines += @(
        '## Shared Boundary Memory Ledger',
        '',
        ('- Shared-boundary-memory state: `{0}`' -f [string] $sharedBoundaryMemoryLedgerState.sharedBoundaryMemoryLedgerState),
        ('- Reason code: `{0}`' -f [string] $sharedBoundaryMemoryLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $sharedBoundaryMemoryLedgerState.nextAction),
        ('- Shared boundary-memory class: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryState')),
        ('- Shared boundary-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryConditionCount')),
        ('- Shared continuity-requirement count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedContinuityRequirementCount')),
        ('- Withheld common-property claim count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'withheldCommonPropertyClaimCount')),
        ('- Locality provenance preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'localityProvenancePreserved')),
        ('- Identity bleed detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'identityBleedDetected')),
        ('- Ambient common property denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'ambientCommonPropertyDenied')),
        ''
    )
}

if ($null -ne $continuityUnderPressureLedgerState) {
    $markdownLines += @(
        '## Continuity Under Pressure Ledger',
        '',
        ('- Continuity-under-pressure state: `{0}`' -f [string] $continuityUnderPressureLedgerState.continuityUnderPressureLedgerState),
        ('- Reason code: `{0}`' -f [string] $continuityUnderPressureLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $continuityUnderPressureLedgerState.nextAction),
        ('- Pressure state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'pressureState')),
        ('- Held continuity count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'heldContinuityCount')),
        ('- Partial continuity count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'partialContinuityCount')),
        ('- Required preservation count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'requiredPreservationCount')),
        ('- Boundary pressure count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'boundaryPressureCount')),
        ('- Fluent success denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'fluentSuccessDenied')),
        ''
    )
}

if ($null -ne $expressiveDeformationReceiptState) {
    $markdownLines += @(
        '## Expressive Deformation Receipt',
        '',
        ('- Expressive-deformation state: `{0}`' -f [string] $expressiveDeformationReceiptState.expressiveDeformationReceiptState),
        ('- Reason code: `{0}`' -f [string] $expressiveDeformationReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $expressiveDeformationReceiptState.nextAction),
        ('- Deformation state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'deformationState')),
        ('- Changed expression count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'changedExpressionCount')),
        ('- Recognizable continuity count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'recognizableContinuityCount')),
        ('- Fracture boundary count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'fractureBoundaryCount')),
        ('- Adaptive refinement preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'adaptiveRefinementPreserved')),
        ('- Identity collapse detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'identityCollapseDetected')),
        ''
    )
}

if ($null -ne $mutualIntelligibilityWitnessState) {
    $markdownLines += @(
        '## Mutual Intelligibility Witness',
        '',
        ('- Mutual-intelligibility state: `{0}`' -f [string] $mutualIntelligibilityWitnessState.mutualIntelligibilityWitnessState),
        ('- Reason code: `{0}`' -f [string] $mutualIntelligibilityWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $mutualIntelligibilityWitnessState.nextAction),
        ('- Shared understanding state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'sharedUnderstandingState')),
        ('- Held intelligibility count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'heldIntelligibilityCount')),
        ('- Narrowed intelligibility count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'narrowedIntelligibilityCount')),
        ('- Broken intelligibility count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'brokenIntelligibilityCount')),
        ('- Sameness collapse denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'samenessCollapseDenied')),
        ('- Opaque divergence detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $mutualIntelligibilityWitnessState -PropertyName 'opaqueDivergenceDetected')),
        ''
    )
}

if ($null -ne $inquiryPatternContinuityLedgerState) {
    $markdownLines += @(
        '## Inquiry Pattern Continuity Ledger',
        '',
        ('- Inquiry-pattern continuity state: `{0}`' -f [string] $inquiryPatternContinuityLedgerState.inquiryPatternContinuityLedgerState),
        ('- Reason code: `{0}`' -f [string] $inquiryPatternContinuityLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $inquiryPatternContinuityLedgerState.nextAction),
        ('- Carry-forward state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'carryForwardState')),
        ('- Reusable inquiry-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'reusableInquiryPatternCount')),
        ('- Trigger-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'triggerConditionCount')),
        ('- Preserved constraint count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'preservedConstraintCount')),
        ('- Boundary-pair count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'boundaryPairCount')),
        ('- Identity bleed denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $inquiryPatternContinuityLedgerState -PropertyName 'identityBleedDenied')),
        ''
    )
}

if ($null -ne $questioningBoundaryPairLedgerState) {
    $markdownLines += @(
        '## Questioning Boundary Pair Ledger',
        '',
        ('- Questioning boundary-pair state: `{0}`' -f [string] $questioningBoundaryPairLedgerState.questioningBoundaryPairLedgerState),
        ('- Reason code: `{0}`' -f [string] $questioningBoundaryPairLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $questioningBoundaryPairLedgerState.nextAction),
        ('- Pairing state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'pairingState')),
        ('- Inquiry-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'inquiryPatternCount')),
        ('- Supporting boundary count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'supportingBoundaryCount')),
        ('- Boundary constraint count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'boundaryConstraintCount')),
        ('- Overreach warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'overreachWarningCount')),
        ('- Constraint memory preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningBoundaryPairLedgerState -PropertyName 'constraintMemoryPreserved')),
        ''
    )
}

if ($null -ne $carryForwardInquirySelectionSurfaceState) {
    $markdownLines += @(
        '## Carry-Forward Inquiry Selection Surface',
        '',
        ('- Carry-forward inquiry-selection surface state: `{0}`' -f [string] $carryForwardInquirySelectionSurfaceState.carryForwardInquirySelectionSurfaceState),
        ('- Reason code: `{0}`' -f [string] $carryForwardInquirySelectionSurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $carryForwardInquirySelectionSurfaceState.nextAction),
        ('- Carry-forward inquiry-selection state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'carryForwardInquirySelectionState')),
        ('- Available carry-forward pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'availableCarryForwardPatternCount')),
        ('- Admitted reuse-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'admittedReuseConditionCount')),
        ('- Withheld reuse-warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'withheldReuseWarningCount')),
        ('- Locality-safe review: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'localitySafeReview')),
        ('- Ambient habit denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $carryForwardInquirySelectionSurfaceState -PropertyName 'ambientHabitDenied')),
        ''
    )
}

if ($null -ne $questioningOperatorCandidateLedgerState) {
    $markdownLines += @(
        '## Questioning Operator Candidate Ledger',
        '',
        ('- Questioning operator candidate-ledger state: `{0}`' -f [string] $questioningOperatorCandidateLedgerState.questioningOperatorCandidateLedgerState),
        ('- Reason code: `{0}`' -f [string] $questioningOperatorCandidateLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $questioningOperatorCandidateLedgerState.nextAction),
        ('- Candidate classification state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'candidateClassificationState')),
        ('- Event-bound inquiry-form count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'eventBoundInquiryFormCount')),
        ('- Candidate inquiry-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'candidateInquiryPatternCount')),
        ('- Promotion-evidence count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'promotionEvidenceCount')),
        ('- Required re-entry condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'requiredReentryConditionCount')),
        ('- Failure-signature expectation count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'failureSignatureExpectationCount')),
        ('- Hidden authority patterns denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'hiddenAuthorityPatternsDenied')),
        ('- Identity-bound patterns withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningOperatorCandidateLedgerState -PropertyName 'identityBoundPatternsWithheld')),
        ''
    )
}

if ($null -ne $questioningGelPromotionGateState) {
    $markdownLines += @(
        '## Questioning GEL Promotion Gate',
        '',
        ('- Questioning GEL promotion-gate state: `{0}`' -f [string] $questioningGelPromotionGateState.questioningGelPromotionGateState),
        ('- Reason code: `{0}`' -f [string] $questioningGelPromotionGateState.reasonCode),
        ('- Next action: `{0}`' -f [string] $questioningGelPromotionGateState.nextAction),
        ('- Promotion gate state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionGateState')),
        ('- Candidate inquiry-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'candidateInquiryPatternCount')),
        ('- Satisfied promotion-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'satisfiedPromotionConditionCount')),
        ('- Unmet promotion-condition count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'unmetPromotionConditionCount')),
        ('- Promotion warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionWarningCount')),
        ('- Locality separation preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'localitySeparationPreserved')),
        ('- Authority separation preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'authoritySeparationPreserved')),
        ('- Truth-seeking invariant preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'truthSeekingInvariantPreserved')),
        ('- Outcome-seeking denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'outcomeSeekingDenied')),
        ('- Promotion review admitted: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningGelPromotionGateState -PropertyName 'promotionReviewAdmitted')),
        ''
    )
}

if ($null -ne $protectedQuestioningPatternSurfaceState) {
    $markdownLines += @(
        '## Protected Questioning Pattern Surface',
        '',
        ('- Protected questioning-pattern surface state: `{0}`' -f [string] $protectedQuestioningPatternSurfaceState.protectedQuestioningPatternSurfaceState),
        ('- Reason code: `{0}`' -f [string] $protectedQuestioningPatternSurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $protectedQuestioningPatternSurfaceState.nextAction),
        ('- Protected review state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'protectedReviewState')),
        ('- Reviewable candidate-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'reviewableCandidatePatternCount')),
        ('- Lawful review-envelope count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'lawfulReviewEnvelopeCount')),
        ('- Withheld interiority-warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'withheldInteriorityWarningCount')),
        ('- Locality-safe legibility: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'localitySafeLegibility')),
        ('- Raw interiority denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'rawInteriorityDenied')),
        ('- Automatic grant denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedQuestioningPatternSurfaceState -PropertyName 'automaticGrantDenied')),
        ''
    )
}

if ($null -ne $variationTestedReentryLedgerState) {
    $markdownLines += @(
        '## Variation-Tested Reentry Ledger',
        '',
        ('- Variation-tested reentry-ledger state: `{0}`' -f [string] $variationTestedReentryLedgerState.variationTestedReentryLedgerState),
        ('- Reason code: `{0}`' -f [string] $variationTestedReentryLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $variationTestedReentryLedgerState.nextAction),
        ('- Variation burden state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'variationBurdenState')),
        ('- Variation context count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'variationContextCount')),
        ('- Surviving pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'survivingPatternCount')),
        ('- Failed pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'failedPatternCount')),
        ('- Required retest pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'requiredRetestPatternCount')),
        ('- Required re-entry pass count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'requiredReentryPassCount')),
        ('- Variation burden satisfied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'variationBurdenSatisfied')),
        ('- Portable patterns withstood variation: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $variationTestedReentryLedgerState -PropertyName 'portablePatternsWithstoodVariation')),
        ''
    )
}

if ($null -ne $questioningAdmissionRefusalReceiptState) {
    $markdownLines += @(
        '## Questioning Admission Refusal Receipt',
        '',
        ('- Questioning admission-refusal receipt state: `{0}`' -f [string] $questioningAdmissionRefusalReceiptState.questioningAdmissionRefusalReceiptState),
        ('- Reason code: `{0}`' -f [string] $questioningAdmissionRefusalReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $questioningAdmissionRefusalReceiptState.nextAction),
        ('- Refusal posture state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'refusalPostureState')),
        ('- Refused pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'refusedPatternCount')),
        ('- Deferred pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'deferredPatternCount')),
        ('- Refusal reason count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'refusalReasonCount')),
        ('- Attractive but under-evidenced denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'attractiveButUnderEvidencedDenied')),
        ('- Archive protection preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'archiveProtectionPreserved')),
        ('- Delay without disposal allowed: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $questioningAdmissionRefusalReceiptState -PropertyName 'delayWithoutDisposalAllowed')),
        ''
    )
}

if ($null -ne $promotionSeductionWatchState) {
    $markdownLines += @(
        '## Promotion Seduction Watch',
        '',
        ('- Promotion seduction-watch state: `{0}`' -f [string] $promotionSeductionWatchState.promotionSeductionWatchState),
        ('- Reason code: `{0}`' -f [string] $promotionSeductionWatchState.reasonCode),
        ('- Next action: `{0}`' -f [string] $promotionSeductionWatchState.nextAction),
        ('- Seduction watch state: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'seductionWatchState')),
        ('- Seduction signal count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'seductionSignalCount')),
        ('- Blocked promotion-vector count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'blockedPromotionVectorCount')),
        ('- Drift warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'driftWarningCount')),
        ('- Prestige inflation denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'prestigeInflationDenied')),
        ('- Elegance bias denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'eleganceBiasDenied')),
        ('- Emotional compulsion denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionSeductionWatchState -PropertyName 'emotionalCompulsionDenied')),
        ''
    )
}

if ($null -ne $engramIntentFieldLedgerState) {
    $markdownLines += @(
        '## Engram Intent Field Ledger',
        '',
        ('- Engram intent-field ledger state: `{0}`' -f [string] $engramIntentFieldLedgerState.engramIntentFieldLedgerState),
        ('- Reason code: `{0}`' -f [string] $engramIntentFieldLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $engramIntentFieldLedgerState.nextAction),
        ('- Intent-bearing pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'intentBearingPatternCount')),
        ('- Scene-bound pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'sceneBoundPatternCount')),
        ('- Resolution-orientation count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'resolutionOrientationCount')),
        ('- Truth-posture count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'truthPostureCount')),
        ('- Candidate carries internal intent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'candidateCarriesInternalIntent')),
        ('- Borrowed justification denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $engramIntentFieldLedgerState -PropertyName 'borrowedJustificationDenied')),
        ''
    )
}

if ($null -ne $intentConstraintAlignmentReceiptState) {
    $markdownLines += @(
        '## Intent Constraint Alignment Receipt',
        '',
        ('- Intent-constraint alignment receipt state: `{0}`' -f [string] $intentConstraintAlignmentReceiptState.intentConstraintAlignmentReceiptState),
        ('- Reason code: `{0}`' -f [string] $intentConstraintAlignmentReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $intentConstraintAlignmentReceiptState.nextAction),
        ('- Aligned pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'alignedPatternCount')),
        ('- Misaligned pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'misalignedPatternCount')),
        ('- Structure-constraint alignment count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'structureConstraintAlignmentCount')),
        ('- Intent-constraint alignment count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'intentConstraintAlignmentCount')),
        ('- Provenance aligned with intent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'provenanceAlignedWithIntent')),
        ('- Scene-bound intent detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $intentConstraintAlignmentReceiptState -PropertyName 'sceneBoundIntentDetected')),
        ''
    )
}

if ($null -ne $warmReactivationDispositionReceiptState) {
    $markdownLines += @(
        '## Warm Reactivation Disposition Receipt',
        '',
        ('- Warm reactivation-disposition receipt state: `{0}`' -f [string] $warmReactivationDispositionReceiptState.warmReactivationDispositionReceiptState),
        ('- Reason code: `{0}`' -f [string] $warmReactivationDispositionReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $warmReactivationDispositionReceiptState.nextAction),
        ('- Warm-held pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'warmHeldPatternCount')),
        ('- Reactivated-hot pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'reactivatedHotPatternCount')),
        ('- Archived pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'archivedPatternCount')),
        ('- Reactivation disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'reactivationDisposition')),
        ('- Warm holding preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'warmHoldingPreserved')),
        ('- Hot re-entry required: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'hotReentryRequired')),
        ('- Cold admission withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmReactivationDispositionReceiptState -PropertyName 'coldAdmissionWithheld')),
        ''
    )
}

if ($null -ne $formationPhaseVectorState) {
    $markdownLines += @(
        '## Formation Phase Vector',
        '',
        ('- Formation phase-vector state: `{0}`' -f [string] $formationPhaseVectorState.formationPhaseVectorState),
        ('- Reason code: `{0}`' -f [string] $formationPhaseVectorState.reasonCode),
        ('- Next action: `{0}`' -f [string] $formationPhaseVectorState.nextAction),
        ('- Phase-axis count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'phaseAxisCount')),
        ('- Stability-axis count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'stabilityAxisCount')),
        ('- Thermal-region count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'thermalRegionCount')),
        ('- Formation region: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'formationRegion')),
        ('- Warm governance dominant: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'warmGovernanceDominant')),
        ('- Cooling eligible: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'coolingEligible')),
        ('- Reheating sensitive: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $formationPhaseVectorState -PropertyName 'reheatingSensitive')),
        ''
    )
}

if ($null -ne $brittlenessWitnessState) {
    $markdownLines += @(
        '## Brittleness Witness',
        '',
        ('- Brittleness-witness state: `{0}`' -f [string] $brittlenessWitnessState.brittlenessWitnessState),
        ('- Reason code: `{0}`' -f [string] $brittlenessWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $brittlenessWitnessState.nextAction),
        ('- Brittle-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'brittlePatternCount')),
        ('- Fracture-axis count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'fractureAxisCount')),
        ('- Overfit-warning count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'overfitWarningCount')),
        ('- Scene-bound brittleness detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'sceneBoundBrittlenessDetected')),
        ('- Misalignment pressure detected: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'misalignmentPressureDetected')),
        ('- Premature cooling denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittlenessWitnessState -PropertyName 'prematureCoolingDenied')),
        ''
    )
}

if ($null -ne $durabilityWitnessState) {
    $markdownLines += @(
        '## Durability Witness',
        '',
        ('- Durability-witness state: `{0}`' -f [string] $durabilityWitnessState.durabilityWitnessState),
        ('- Reason code: `{0}`' -f [string] $durabilityWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $durabilityWitnessState.nextAction),
        ('- Durable-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'durablePatternCount')),
        ('- Interlock-signal count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'interlockSignalCount')),
        ('- Cooling-barrier count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'coolingBarrierCount')),
        ('- Durable under variation: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'durableUnderVariation')),
        ('- Interlock density emergent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'interlockDensityEmergent')),
        ('- Cold promotion still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $durabilityWitnessState -PropertyName 'coldPromotionStillWithheld')),
        ''
    )
}

if ($null -ne $warmClockDispositionState) {
    $markdownLines += @(
        '## Warm Clock Disposition',
        '',
        ('- Warm-clock disposition state: `{0}`' -f [string] $warmClockDispositionState.warmClockDispositionState),
        ('- Reason code: `{0}`' -f [string] $warmClockDispositionState.reasonCode),
        ('- Next action: `{0}`' -f [string] $warmClockDispositionState.nextAction),
        ('- Warm-clock count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'warmClockCount')),
        ('- Unresolved unknown load: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'unresolvedUnknownLoad')),
        ('- Ripening disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'ripeningDisposition')),
        ('- Staleness disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'stalenessDisposition')),
        ('- Re-entry clock active: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'reentryClockActive')),
        ('- Distance burden still active: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'distanceBurdenStillActive')),
        ('- Failure-signature freshness required: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'failureSignatureFreshnessRequired')),
        ('- Warm ripening underway: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'warmRipeningUnderway')),
        ('- Staleness risk present: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $warmClockDispositionState -PropertyName 'stalenessRiskPresent')),
        ''
    )
}

if ($null -ne $ripeningStalenessLedgerState) {
    $markdownLines += @(
        '## Ripening Staleness Ledger',
        '',
        ('- Ripening-staleness ledger state: `{0}`' -f [string] $ripeningStalenessLedgerState.ripeningStalenessLedgerState),
        ('- Reason code: `{0}`' -f [string] $ripeningStalenessLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $ripeningStalenessLedgerState.nextAction),
        ('- Ripening-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'ripeningPatternCount')),
        ('- Stale-pattern count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'stalePatternCount')),
        ('- Ripening-window count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'ripeningWindowCount')),
        ('- Stale-window count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'staleWindowCount')),
        ('- Refresh-required count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'refreshRequiredCount')),
        ('- Honest warm ripening preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'honestWarmRipeningPreserved')),
        ('- Administrative suspension denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'administrativeSuspensionDenied')),
        ('- Fresh constraint contact still required: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $ripeningStalenessLedgerState -PropertyName 'freshConstraintContactStillRequired')),
        ''
    )
}

if ($null -ne $coolingPressureWitnessState) {
    $markdownLines += @(
        '## Cooling Pressure Witness',
        '',
        ('- Cooling-pressure witness state: `{0}`' -f [string] $coolingPressureWitnessState.coolingPressureWitnessState),
        ('- Reason code: `{0}`' -f [string] $coolingPressureWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $coolingPressureWitnessState.nextAction),
        ('- Cooling-force count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingForceCount')),
        ('- Cooling-barrier count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingBarrierCount')),
        ('- Pressure disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'pressureDisposition')),
        ('- Cooling pressure emergent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coolingPressureEmergent')),
        ('- Cold approach lawful: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'coldApproachLawful')),
        ('- Reheating or archive pressure still stronger: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coolingPressureWitnessState -PropertyName 'reheatingOrArchivePressureStillStronger')),
        ''
    )
}

if ($null -ne $hotReactivationTriggerReceiptState) {
    $markdownLines += @(
        '## Hot Reactivation Trigger Receipt',
        '',
        ('- Hot reactivation-trigger receipt state: `{0}`' -f [string] $hotReactivationTriggerReceiptState.hotReactivationTriggerReceiptState),
        ('- Reason code: `{0}`' -f [string] $hotReactivationTriggerReceiptState.reasonCode),
        ('- Next action: `{0}`' -f [string] $hotReactivationTriggerReceiptState.nextAction),
        ('- Reactivation-trigger count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reactivationTriggerCount')),
        ('- Failed-invariant count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'failedInvariantCount')),
        ('- Reactivation disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reactivationDisposition')),
        ('- Hot return lawful: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'hotReturnLawful')),
        ('- Warm holding insufficient: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'warmHoldingInsufficient')),
        ('- Re-entry as formation preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $hotReactivationTriggerReceiptState -PropertyName 'reentryAsFormationPreserved')),
        ''
    )
}

if ($null -ne $coldAdmissionEligibilityGateState) {
    $markdownLines += @(
        '## Cold Admission Eligibility Gate',
        '',
        ('- Cold admission-eligibility gate state: `{0}`' -f [string] $coldAdmissionEligibilityGateState.coldAdmissionEligibilityGateState),
        ('- Reason code: `{0}`' -f [string] $coldAdmissionEligibilityGateState.reasonCode),
        ('- Next action: `{0}`' -f [string] $coldAdmissionEligibilityGateState.nextAction),
        ('- Eligibility-signal count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'eligibilitySignalCount')),
        ('- Remaining-barrier count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'remainingBarrierCount')),
        ('- Eligibility disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'eligibilityDisposition')),
        ('- Cold approach lawful: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'coldApproachLawful')),
        ('- Pre-freeze only: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'preFreezeOnly')),
        ('- Final inheritance still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coldAdmissionEligibilityGateState -PropertyName 'finalInheritanceStillWithheld')),
        ''
    )
}

if ($null -ne $archiveDispositionLedgerState) {
    $markdownLines += @(
        '## Archive Disposition Ledger',
        '',
        ('- Archive disposition-ledger state: `{0}`' -f [string] $archiveDispositionLedgerState.archiveDispositionLedgerState),
        ('- Reason code: `{0}`' -f [string] $archiveDispositionLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $archiveDispositionLedgerState.nextAction),
        ('- Archive-route count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'archiveRouteCount')),
        ('- Preserved provenance-mark count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'preservedProvenanceMarkCount')),
        ('- Denied rewrite-risk count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'deniedRewriteRiskCount')),
        ('- Archive disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'archiveDisposition')),
        ('- Provenance preserved: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'provenancePreserved')),
        ('- Pseudo-lineage denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'pseudoLineageDenied')),
        ('- Warm indefinite holding denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $archiveDispositionLedgerState -PropertyName 'warmIndefiniteHoldingDenied')),
        ''
    )
}

if ($null -ne $interlockDensityLedgerState) {
    $markdownLines += @(
        '## Interlock Density Ledger',
        '',
        ('- Interlock density-ledger state: `{0}`' -f [string] $interlockDensityLedgerState.interlockDensityLedgerState),
        ('- Reason code: `{0}`' -f [string] $interlockDensityLedgerState.reasonCode),
        ('- Next action: `{0}`' -f [string] $interlockDensityLedgerState.nextAction),
        ('- Independent constraint-link count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'independentConstraintLinkCount')),
        ('- Reentry-survival count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'reentrySurvivalCount')),
        ('- Durable-alignment count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'durableAlignmentCount')),
        ('- Interlock density disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'interlockDensityDisposition')),
        ('- Dense interweave emergent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'denseInterweaveEmergent')),
        ('- Lattice claim still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $interlockDensityLedgerState -PropertyName 'latticeClaimStillWithheld')),
        ''
    )
}

if ($null -ne $brittleDurableDifferentiationSurfaceState) {
    $markdownLines += @(
        '## Brittle Durable Differentiation Surface',
        '',
        ('- Brittle durable differentiation-surface state: `{0}`' -f [string] $brittleDurableDifferentiationSurfaceState.brittleDurableDifferentiationSurfaceState),
        ('- Reason code: `{0}`' -f [string] $brittleDurableDifferentiationSurfaceState.reasonCode),
        ('- Next action: `{0}`' -f [string] $brittleDurableDifferentiationSurfaceState.nextAction),
        ('- Brittle fragment count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'brittleFragmentCount')),
        ('- Durable kernel count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'durableKernelCount')),
        ('- Coexisting region count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'coexistingRegionCount')),
        ('- Surface disposition: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'surfaceDisposition')),
        ('- Brittle durable coexistence exposed: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'brittleDurableCoexistenceExposed')),
        ('- Average readiness denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'averageReadinessDenied')),
        ('- Full trust still withheld: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $brittleDurableDifferentiationSurfaceState -PropertyName 'fullTrustStillWithheld')),
        ''
    )
}

if ($null -ne $coreInvariantLatticeWitnessState) {
    $markdownLines += @(
        '## Core Invariant Lattice Witness',
        '',
        ('- Core invariant lattice-witness state: `{0}`' -f [string] $coreInvariantLatticeWitnessState.coreInvariantLatticeWitnessState),
        ('- Reason code: `{0}`' -f [string] $coreInvariantLatticeWitnessState.reasonCode),
        ('- Next action: `{0}`' -f [string] $coreInvariantLatticeWitnessState.nextAction),
        ('- Candidate core-invariant count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'candidateCoreInvariantCount')),
        ('- Identity adjacency-signal count: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'identityAdjacencySignalCount')),
        ('- Interlock posture: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'interlockPosture')),
        ('- Identity-adjacent significance emergent: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'identityAdjacentSignificanceEmergent')),
        ('- Core law sanctification denied: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'coreLawSanctificationDenied')),
        ('- Lattice-grade invariance witnessed: `{0}`' -f [string] (Get-ObjectPropertyValueOrNull -InputObject $coreInvariantLatticeWitnessState -PropertyName 'latticeGradeInvarianceWitnessed')),
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

    if (@($queuedBatchTaskMaps).Count -gt 0) {
        $markdownLines += ('- Queued batch: `{0}`' -f ((@($queuedBatchTaskMaps | ForEach-Object { [string] $_.label }) -join ' -> ')))
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
            -UnattendedProofCollapseState $unattendedProofCollapseState `
            -DormantWindowLedgerState $dormantWindowLedgerState `
            -SilentCadenceIntegrityState $silentCadenceIntegrityState `
            -LongFormPhaseWitnessState $longFormPhaseWitnessState `
            -LongFormWindowBoundaryState $longFormWindowBoundaryState `
            -AutonomousLongFormCollapseState $autonomousLongFormCollapseState `
            -SchedulerProofHarvestState $schedulerProofHarvestState `
            -IntervalOriginClarificationState $intervalOriginClarificationState `
            -QueuedTaskMapPromotionState $queuedTaskMapPromotionState `
            -RuntimeDeployabilityEnvelopeState $runtimeDeployabilityEnvelopeState `
            -SanctuaryRuntimeReadinessState $sanctuaryRuntimeReadinessState `
            -RuntimeWorkSurfaceAdmissibilityState $runtimeWorkSurfaceAdmissibilityState `
            -ReachAccessTopologyLedgerState $reachAccessTopologyLedgerState `
            -BondedOperatorLocalityReadinessState $bondedOperatorLocalityReadinessState `
            -ProtectedStateLegibilitySurfaceState $protectedStateLegibilitySurfaceState `
            -NexusSingularPortalFacadeState $nexusSingularPortalFacadeState `
            -DuplexPredicateEnvelopeState $duplexPredicateEnvelopeState `
            -OperatorActualWorkSessionRehearsalState $operatorActualWorkSessionRehearsalState `
            -IdentityInvariantThreadRootState $identityInvariantThreadRootState `
            -GovernedThreadBirthReceiptState $governedThreadBirthReceiptState `
            -InterWorkerBraidHandoffPacketState $interWorkerBraidHandoffPacketState `
            -AgentiCoreActualUtilitySurfaceState $agentiCoreActualUtilitySurfaceState `
            -ReachDuplexRealizationSeamState $reachDuplexRealizationSeamState `
            -BondedParticipationLocalityLedgerState $bondedParticipationLocalityLedgerState `
            -SanctuaryRuntimeWorkbenchSurfaceState $sanctuaryRuntimeWorkbenchSurfaceState `
            -AmenableDayDreamTierAdmissibilityState $amenableDayDreamTierAdmissibilityState `
            -SelfRootedCrypticDepthGateState $selfRootedCrypticDepthGateState `
            -RuntimeWorkbenchSessionLedgerState $runtimeWorkbenchSessionLedgerState `
            -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState `
            -DayDreamCollapseReceiptState $dayDreamCollapseReceiptState `
            -CrypticDepthReturnReceiptState $crypticDepthReturnReceiptState `
            -BondedCoWorkSessionRehearsalState $bondedCoWorkSessionRehearsalState `
            -ReachReturnDissolutionReceiptState $reachReturnDissolutionReceiptState `
            -LocalityDistinctionWitnessLedgerState $localityDistinctionWitnessLedgerState `
            -LocalHostSanctuaryResidencyEnvelopeState $localHostSanctuaryResidencyEnvelopeState `
            -RuntimeHabitationReadinessLedgerState $runtimeHabitationReadinessLedgerState `
            -BoundedInhabitationLaunchRehearsalState $boundedInhabitationLaunchRehearsalState `
            -PostHabitationHorizonLatticeState $postHabitationHorizonLatticeState `
            -BoundedHorizonResearchBriefState $boundedHorizonResearchBriefState `
            -NextEraBatchSelectorState $nextEraBatchSelectorState `
            -InquirySessionDisciplineSurfaceState $inquirySessionDisciplineSurfaceState `
            -BoundaryConditionLedgerState $boundaryConditionLedgerState `
            -CoherenceGainWitnessReceiptState $coherenceGainWitnessReceiptState `
            -OperatorInquirySelectionEnvelopeState $operatorInquirySelectionEnvelopeState `
            -BondedCrucibleSessionRehearsalState $bondedCrucibleSessionRehearsalState `
            -SharedBoundaryMemoryLedgerState $sharedBoundaryMemoryLedgerState `
            -ContinuityUnderPressureLedgerState $continuityUnderPressureLedgerState `
            -ExpressiveDeformationReceiptState $expressiveDeformationReceiptState `
            -MutualIntelligibilityWitnessState $mutualIntelligibilityWitnessState `
            -InquiryPatternContinuityLedgerState $inquiryPatternContinuityLedgerState `
            -QuestioningBoundaryPairLedgerState $questioningBoundaryPairLedgerState `
            -CarryForwardInquirySelectionSurfaceState $carryForwardInquirySelectionSurfaceState `
            -EngramDistanceClassificationLedgerState $engramDistanceClassificationLedgerState `
            -EngramPromotionRequirementsMatrixState $engramPromotionRequirementsMatrixState `
            -DistanceWeightedQuestioningAdmissionSurfaceState $distanceWeightedQuestioningAdmissionSurfaceState `
            -QuestioningOperatorCandidateLedgerState $questioningOperatorCandidateLedgerState `
            -QuestioningGelPromotionGateState $questioningGelPromotionGateState `
            -ProtectedQuestioningPatternSurfaceState $protectedQuestioningPatternSurfaceState `
            -VariationTestedReentryLedgerState $variationTestedReentryLedgerState `
            -QuestioningAdmissionRefusalReceiptState $questioningAdmissionRefusalReceiptState `
            -PromotionSeductionWatchState $promotionSeductionWatchState `
            -EngramIntentFieldLedgerState $engramIntentFieldLedgerState `
            -IntentConstraintAlignmentReceiptState $intentConstraintAlignmentReceiptState `
            -WarmReactivationDispositionReceiptState $warmReactivationDispositionReceiptState `
            -FormationPhaseVectorState $formationPhaseVectorState `
            -BrittlenessWitnessState $brittlenessWitnessState `
            -DurabilityWitnessState $durabilityWitnessState `
            -WarmClockDispositionState $warmClockDispositionState `
            -RipeningStalenessLedgerState $ripeningStalenessLedgerState `
            -CoolingPressureWitnessState $coolingPressureWitnessState `
            -HotReactivationTriggerReceiptState $hotReactivationTriggerReceiptState `
            -ColdAdmissionEligibilityGateState $coldAdmissionEligibilityGateState `
            -ArchiveDispositionLedgerState $archiveDispositionLedgerState `
            -InterlockDensityLedgerState $interlockDensityLedgerState `
            -BrittleDurableDifferentiationSurfaceState $brittleDurableDifferentiationSurfaceState `
            -CoreInvariantLatticeWitnessState $coreInvariantLatticeWitnessState `
            -LastKnownStatus $lastKnownStatus `
            -BlockedStatus ([string] $cyclePolicy.blockedStatus)
        $taskLiveStatus = Resolve-RunIsolatedBuildTaskLiveStatus `
            -TaskId ([string] $task.id) `
            -CurrentLiveStatus $taskLiveStatus `
            -RunIsolatedBuildPathwayState $runIsolatedBuildPathwayState
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

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $statusMarkdownPath -Value $markdownLines -Encoding utf8

Write-Host ('[local-automation-status] JSON: {0}' -f $statusJsonPath)
$statusJsonPath
