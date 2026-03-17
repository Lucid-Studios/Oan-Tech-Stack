using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class InnerWeatherProjectorTests
{
    [Fact]
    public void ProjectForLoop_StableWindow_ReturnsIntactStableEvidence()
    {
        var batch = CreateBatch(
            new ObservationSpec(),
            new ObservationSpec(),
            new ObservationSpec());

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.Intact, evidence!.WindowIntegrityState);
        Assert.Equal(AttentionResidueState.None, evidence.Residue.ResidueState);
        Assert.Equal(ShellCompetitionState.Absent, evidence.ShellCompetition.CompetitionState);
        Assert.Equal(HotCoolContactState.InContact, evidence.HotCoolContactState);
    }

    [Fact]
    public void ProjectForLoop_OneOffAdvisoryDivergence_RemainsLow()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(AdvisoryDisposition: CompassSeedAdvisoryDisposition.Deferred),
            new ObservationSpec()
            },
            finalDriftState: CompassDriftState.Held,
            finalAdvisoryDivergenceCount: 1);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(AttentionResidueState.Low, evidence!.Residue.ResidueState);
        Assert.DoesNotContain(AttentionResidueContributor.CompetingPressure, evidence.Residue.Contributors);
    }

    [Fact]
    public void ProjectForLoop_RepeatedDivergenceWithoutLoss_ReturnsPersistentResidue()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(AdvisoryDisposition: CompassSeedAdvisoryDisposition.Deferred),
            new ObservationSpec(AdvisoryDisposition: CompassSeedAdvisoryDisposition.Rejected)
            },
            finalDriftState: CompassDriftState.Weakened,
            finalAdvisoryDivergenceCount: 2);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(AttentionResidueState.Persistent, evidence!.Residue.ResidueState);
        Assert.Contains(AttentionResidueContributor.AdvisoryDivergence, evidence.Residue.Contributors);
        Assert.Contains(StewardAttentionCause.DriftWeakening, evidence.StewardAttentionCauses);
    }

    [Fact]
    public void ProjectForLoop_CompetingPressure_ReturnsPresentShellCompetition()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.FluidContinuityLaw,
                CompetingBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                AnchorState: CompassAnchorState.Weakened),
            new ObservationSpec()
            },
            finalDriftState: CompassDriftState.Weakened,
            finalCompetingMigrationCount: 1);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(ShellCompetitionState.Present, evidence!.ShellCompetition.CompetitionState);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, evidence.ShellCompetition.VisibilityClass);
    }

    [Fact]
    public void ProjectForLoop_SparseWindow_BiasesToSparseIntegrity()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec()
            },
            finalDriftState: CompassDriftState.Held,
            finalWindowSize: 3);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.Sparse, evidence!.WindowIntegrityState);
        Assert.Equal(HotCoolContactState.InContact, evidence.HotCoolContactState);
    }

    [Fact]
    public void ProjectForLoop_HighConfidenceRejectedSeedRead_DoesNotForceEscalation()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Rejected,
                AdvisoryConfidence: 0.99),
            new ObservationSpec()
            },
            finalDriftState: CompassDriftState.Held,
            finalAdvisoryDivergenceCount: 1);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(AttentionResidueState.Low, evidence!.Residue.ResidueState);
        Assert.DoesNotContain(StewardAttentionCause.DriftLoss, evidence.StewardAttentionCauses);
    }

    [Fact]
    public void ProjectForLoop_JournalGap_IsExplicitBoundaryBreak()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(),
            new ObservationSpec()
            },
            issues:
            [
                new GovernanceJournalReplayIssue(17, "missing-line", "{}", null)
            ]);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.JournalGap, evidence!.WindowIntegrityState);
    }

    [Fact]
    public void ProjectForLoop_RuntimeRestart_IsExplicitBoundaryBreak()
    {
        var batch = CreateBatch(
            new ObservationSpec(WorkingStateHandle: "soulframe-working://shared/restart"),
            new ObservationSpec(WorkingStateHandle: "soulframe-working://shared/restart"),
            new ObservationSpec(WorkingStateHandle: "soulframe-working://shared/restart"));

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.RuntimeRestart, evidence!.WindowIntegrityState);
    }

    [Fact]
    public void ProjectForLoop_CmeReselected_IsExplicitBoundaryBreak()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(),
            new ObservationSpec()
            },
            extraReviewRequests:
            [
                new ExtraReviewRequestSpec(
                    LoopKey: "loop-reselected",
                    CMEId: "cme-other",
                    SessionHandle: "soulframe-session://cme-weather/3",
                    ProtectionClass: "cryptic-review",
                    EnvelopeId: "control-envelope://reselected")
            ]);

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.CmeReselected, evidence!.WindowIntegrityState);
    }

    [Fact]
    public void ProjectForLoop_VisibilityDowngraded_IsExplicitBoundaryBreak()
    {
        var batch = CreateBatch(
            new ObservationSpec(ProtectionClass: "community-review"),
            new ObservationSpec(ProtectionClass: "operator-guarded-review"),
            new ObservationSpec(ProtectionClass: "cryptic-review"));

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.VisibilityDowngraded, evidence!.WindowIntegrityState);
    }

    [Fact]
    public void ProjectForLoop_GovernanceReset_IsExplicitBoundaryBreak()
    {
        var batch = CreateBatchWithOptions(
            new[]
            {
            new ObservationSpec(),
            new ObservationSpec(),
            new ObservationSpec()
            },
            currentLoopAlternateEnvelopeId: "control-envelope://reset");

        var evidence = InnerWeatherProjector.ProjectForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(evidence);
        Assert.Equal(WindowIntegrityState.GovernanceReset, evidence!.WindowIntegrityState);
    }

    private static BatchSpec CreateBatch(
        params ObservationSpec[] observations) =>
        CreateBatchWithOptions(
            observations,
            finalDriftState: CompassDriftState.Held,
            finalAdvisoryDivergenceCount: observations.Count(spec =>
                spec.AdvisoryDisposition is CompassSeedAdvisoryDisposition.Deferred or CompassSeedAdvisoryDisposition.Rejected),
            finalCompetingMigrationCount: observations.Count(spec =>
                spec.ActiveBasin == CompassDoctrineBasin.FluidContinuityLaw),
            finalWindowSize: Math.Max(3, observations.Length),
            issues: null,
            extraReviewRequests: null,
            currentLoopAlternateEnvelopeId: null);

    private static BatchSpec CreateBatchWithOptions(
        ObservationSpec[] observations,
        CompassDriftState finalDriftState = CompassDriftState.Held,
        int finalAdvisoryDivergenceCount = 0,
        int finalCompetingMigrationCount = 0,
        int finalWindowSize = 3,
        IReadOnlyList<GovernanceJournalReplayIssue>? issues = null,
        IReadOnlyList<ExtraReviewRequestSpec>? extraReviewRequests = null,
        string? currentLoopAlternateEnvelopeId = null)
    {
        var cmeId = "cme-weather";
        var baseTimestamp = new DateTimeOffset(2026, 3, 16, 10, 0, 0, TimeSpan.Zero);
        var entries = new List<GovernanceJournalEntry>();
        var observationHandles = new List<string>(observations.Length);
        string? finalLoopKey = null;

        for (var index = 0; index < observations.Length; index++)
        {
            var spec = observations[index];
            var candidateId = Guid.Parse($"00000000-0000-0000-0000-{index + 1:000000000000}");
            var provenance = $"membrane-derived:cme:{cmeId}|policy:weather-{index + 1}";
            var loopKey = GovernanceLoopKeys.Create(candidateId, provenance);
            finalLoopKey = loopKey;
            var timestamp = baseTimestamp.AddMinutes(index);
            var sessionHandle = spec.SessionHandle ?? $"soulframe-session://{cmeId}/{index + 1}";
            var workingStateHandle = spec.WorkingStateHandle ?? $"soulframe-working://{cmeId}/{index + 1}";
            var reviewRequest = CreateReviewRequest(
                candidateId,
                provenance,
                cmeId,
                loopKey,
                ordinal: index + 1,
                sessionHandle,
                workingStateHandle,
                spec.ProtectionClass,
                envelopeId: $"control-envelope://{loopKey}");
            var observationHandle = $"compass-observation://{index + 1:0000000000000000}";
            observationHandles.Add(observationHandle);

            entries.Add(new GovernanceJournalEntry(
                loopKey,
                GovernanceJournalEntryKind.CompassObservation,
                GovernanceLoopStage.BoundedCognitionCompleted,
                timestamp.UtcDateTime,
                DecisionReceipt: null,
                DeferredReview: null,
                ActReceipt: null,
                ReviewRequest: reviewRequest,
                Annotation: null,
                HopngArtifactReceipt: null,
                TargetWitnessReceipt: null,
                CompassObservationReceipt: new GovernedCompassObservationReceipt(
                    WitnessHandle: CompassObservationKeys.CreateWitnessHandle(loopKey, observationHandle),
                    Stage: GovernanceLoopStage.BoundedCognitionCompleted,
                    ObservationHandle: observationHandle,
                    ActiveBasin: spec.ActiveBasin,
                    CompetingBasin: spec.CompetingBasin,
                    OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
                    SelfTouchClass: spec.SelfTouchClass,
                    AnchorState: spec.AnchorState,
                    Provenance: CompassObservationProvenance.Braided,
                    WitnessedBy: "CradleTek",
                    WorkingStateHandle: workingStateHandle,
                    CSelfGelHandle: $"soulframe-cselfgel://{cmeId}/{index + 1}",
                    SelfGelHandle: $"soulframe-selfgel://{cmeId}/{index + 1}",
                    ValidationReferenceHandle: spec.ValidationReferenceHandle,
                    Objective: "observe civic inner weather",
                    AdvisoryAccepted: spec.AdvisoryDisposition == CompassSeedAdvisoryDisposition.Accepted,
                    AdvisoryDecision: "advisory",
                    AdvisoryTrace: "response-ready",
                    AdvisoryConfidence: spec.AdvisoryConfidence,
                    AdvisorySuggestedActiveBasin: spec.ActiveBasin,
                    AdvisorySuggestedCompetingBasin: spec.CompetingBasin,
                    AdvisorySuggestedAnchorState: spec.AnchorState,
                    AdvisorySuggestedSelfTouchClass: spec.SelfTouchClass,
                    AdvisoryDisposition: spec.AdvisoryDisposition,
                    AdvisoryDispositionReason: spec.AdvisoryDisposition.ToString().ToLowerInvariant(),
                    AdvisoryJustification: "weather observation",
                    TimestampUtc: timestamp)));
        }

        if (currentLoopAlternateEnvelopeId is not null && finalLoopKey is not null)
        {
            entries.Add(new GovernanceJournalEntry(
                finalLoopKey,
                GovernanceJournalEntryKind.Annotation,
                GovernanceLoopStage.BoundedCognitionCompleted,
                baseTimestamp.AddMinutes(observations.Length).UtcDateTime,
                DecisionReceipt: null,
                DeferredReview: null,
                ActReceipt: null,
                ReviewRequest: CreateReviewRequest(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    "membrane-derived:cme:cme-weather|policy:reset",
                    cmeId,
                    finalLoopKey,
                    ordinal: observations.Length,
                    sessionHandle: $"soulframe-session://{cmeId}/{observations.Length}",
                    workingStateHandle: $"soulframe-working://{cmeId}/{observations.Length}",
                    protectionClass: observations[^1].ProtectionClass,
                    envelopeId: currentLoopAlternateEnvelopeId),
                Annotation: new GovernanceDeferredAnnotation(finalLoopKey, Guid.NewGuid(), "reset", "CradleTek", "reset", DateTime.UtcNow)));
        }

        if (finalLoopKey is not null)
        {
            entries.Add(new GovernanceJournalEntry(
                finalLoopKey,
                GovernanceJournalEntryKind.CompassDrift,
                GovernanceLoopStage.BoundedCognitionCompleted,
                baseTimestamp.AddMinutes(observations.Length).UtcDateTime,
                DecisionReceipt: null,
                DeferredReview: null,
                ActReceipt: null,
                ReviewRequest: null,
                Annotation: null,
                HopngArtifactReceipt: null,
                TargetWitnessReceipt: null,
                CompassObservationReceipt: null,
                CompassDriftReceipt: new GovernedCompassDriftReceipt(
                    DriftHandle: CompassObservationKeys.CreateDriftHandle(
                        finalLoopKey,
                        cmeId,
                        finalDriftState,
                        observationHandles),
                    LoopKey: finalLoopKey,
                    Stage: GovernanceLoopStage.BoundedCognitionCompleted,
                    CMEId: cmeId,
                    DriftState: finalDriftState,
                    BaselineActiveBasin: observations[0].ActiveBasin,
                    BaselineCompetingBasin: observations[0].CompetingBasin,
                    LatestActiveBasin: observations[^1].ActiveBasin,
                    ObservationCount: observations.Length,
                    WindowSize: finalWindowSize,
                    AdvisoryDivergenceCount: finalAdvisoryDivergenceCount,
                    CompetingMigrationCount: finalCompetingMigrationCount,
                    WitnessedBy: "CradleTek",
                    ObservationHandles: observationHandles.ToArray(),
                    TimestampUtc: baseTimestamp.AddMinutes(observations.Length))));
        }

        if (extraReviewRequests is not null)
        {
            foreach (var extra in extraReviewRequests)
            {
                entries.Add(new GovernanceJournalEntry(
                    extra.LoopKey,
                    GovernanceJournalEntryKind.Annotation,
                    GovernanceLoopStage.BoundedCognitionCompleted,
                    baseTimestamp.AddMinutes(observations.Length + 1).UtcDateTime,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: CreateReviewRequest(
                        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        $"membrane-derived:cme:{extra.CMEId}|policy:extra",
                        extra.CMEId,
                        extra.LoopKey,
                        ordinal: observations.Length + 1,
                        sessionHandle: extra.SessionHandle,
                        workingStateHandle: $"soulframe-working://{extra.CMEId}/extra",
                        protectionClass: extra.ProtectionClass,
                        envelopeId: extra.EnvelopeId),
                    Annotation: new GovernanceDeferredAnnotation(extra.LoopKey, Guid.NewGuid(), "extra", "CradleTek", "extra", DateTime.UtcNow)));
            }
        }

        return new BatchSpec(
            finalLoopKey!,
            new GovernanceJournalReplayBatch(entries, issues ?? []));
    }

    private static ReturnCandidateReviewRequest CreateReviewRequest(
        Guid candidateId,
        string provenance,
        string cmeId,
        string loopKey,
        int ordinal,
        string sessionHandle,
        string workingStateHandle,
        string protectionClass,
        string envelopeId)
    {
        var contentHandle = $"agenticore-return://candidate/{ordinal}";
        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: contentHandle,
            originSurface: "prime",
            provenanceMarker: provenance,
            sourceSubsystem: "AgentiCore");
        var baseEnvelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.StewardReturnReview,
            requestedBy: "CradleTek",
            scopeHandle: sessionHandle,
            protectionClass: protectionClass,
            witnessRequirement: "governance-witness",
            actionableContent: actionableContent,
            parentEnvelopeId: $"control-envelope://{loopKey}");
        var envelope = baseEnvelope with { EnvelopeId = envelopeId };

        return new ReturnCandidateReviewRequest(
            CandidateId: candidateId,
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: cmeId,
            ContextId: Guid.NewGuid(),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ReturnCandidatePointer: contentHandle,
            ProvenanceMarker: provenance,
            IntakeIntent: "candidate-return-evaluation",
            SubmittedBy: "CradleTek",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            RequestEnvelope: envelope);
    }

    private sealed record BatchSpec(
        string FinalLoopKey,
        GovernanceJournalReplayBatch ReplayBatch);

    private sealed record ObservationSpec(
        CompassDoctrineBasin ActiveBasin = CompassDoctrineBasin.BoundedLocalityContinuity,
        CompassDoctrineBasin CompetingBasin = CompassDoctrineBasin.FluidContinuityLaw,
        CompassAnchorState AnchorState = CompassAnchorState.Held,
        CompassSeedAdvisoryDisposition AdvisoryDisposition = CompassSeedAdvisoryDisposition.Accepted,
        double AdvisoryConfidence = 0.71,
        string ProtectionClass = "cryptic-review",
        string? SessionHandle = null,
        string? WorkingStateHandle = null,
        string? ValidationReferenceHandle = "soulframe-selfgel://validation",
        CompassSelfTouchClass SelfTouchClass = CompassSelfTouchClass.ValidationTouch);

    private sealed record ExtraReviewRequestSpec(
        string LoopKey,
        string CMEId,
        string SessionHandle,
        string ProtectionClass,
        string EnvelopeId);
}
