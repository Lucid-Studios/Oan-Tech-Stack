using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class CompassDriftProjectorTests
{
    [Fact]
    public void ProjectForLoop_StableThreeObservationWindow_ReturnsHeld()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Held, assessment!.DriftState);
        Assert.Equal(3, assessment.WindowSize);
        Assert.Equal(3, assessment.ObservationCount);
        Assert.Equal(0, assessment.AdvisoryDivergenceCount);
        Assert.Equal(0, assessment.CompetingMigrationCount);
    }

    [Fact]
    public void ProjectForLoop_OneOffAnomaly_RemainsHeld()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Deferred),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Held, assessment!.DriftState);
        Assert.Equal(1, assessment.AdvisoryDivergenceCount);
    }

    [Fact]
    public void ProjectForLoop_RepeatedAdvisoryDivergence_ReturnsWeakened()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Deferred),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Rejected));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Weakened, assessment!.DriftState);
        Assert.Equal(2, assessment.AdvisoryDivergenceCount);
        Assert.Equal(0, assessment.CompetingMigrationCount);
    }

    [Fact]
    public void ProjectForLoop_CompetingBasinPressure_ReturnsWeakened()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.FluidContinuityLaw,
                CompetingBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                AnchorState: CompassAnchorState.Weakened,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Weakened, assessment!.DriftState);
        Assert.Equal(1, assessment.CompetingMigrationCount);
    }

    [Fact]
    public void ProjectForLoop_LatestCompetingBasin_ReturnsLost()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.FluidContinuityLaw,
                CompetingBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                AnchorState: CompassAnchorState.Weakened,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.FluidContinuityLaw,
                CompetingBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Lost, assessment!.DriftState);
        Assert.Equal(2, assessment.CompetingMigrationCount);
    }

    [Fact]
    public void ProjectForLoop_HighConfidenceRejectedAdvisory_RemainsHeld()
    {
        var assessment = Project(
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Rejected,
                AdvisoryConfidence: 0.99),
            new ObservationSpec(
                ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                AnchorState: CompassAnchorState.Held,
                AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted));

        Assert.NotNull(assessment);
        Assert.Equal(CompassDriftState.Held, assessment!.DriftState);
        Assert.Equal(1, assessment.AdvisoryDivergenceCount);
    }

    private static CompassDriftAssessment? Project(params ObservationSpec[] observations)
    {
        var cmeId = "cme-drift";
        var entries = new List<GovernanceJournalEntry>(observations.Length);
        string? finalLoopKey = null;

        for (var index = 0; index < observations.Length; index++)
        {
            var observation = observations[index];
            var candidateId = Guid.Parse($"00000000-0000-0000-0000-{index + 1:000000000000}");
            var provenance = $"membrane-derived:cme:{cmeId}|policy:drift-{index + 1}";
            var loopKey = GovernanceLoopKeys.Create(candidateId, provenance);
            finalLoopKey = loopKey;
            var timestamp = DateTimeOffset.UtcNow.AddMinutes(index);
            var reviewRequest = CreateReviewRequest(candidateId, provenance, cmeId, loopKey, index + 1);
            var observationHandle = $"compass-observation://{index + 1:0000000000000000}";

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
                    ActiveBasin: observation.ActiveBasin,
                    CompetingBasin: observation.CompetingBasin,
                    OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
                    SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
                    AnchorState: observation.AnchorState,
                    Provenance: CompassObservationProvenance.Braided,
                    WitnessedBy: "CradleTek",
                    WorkingStateHandle: $"soulframe-working://{cmeId}/{index + 1}",
                    CSelfGelHandle: $"soulframe-cselfgel://{cmeId}/{index + 1}",
                    SelfGelHandle: $"soulframe-selfgel://{cmeId}/{index + 1}",
                    ValidationReferenceHandle: $"soulframe-selfgel://{cmeId}/{index + 1}",
                    Objective: "observe bounded continuity drift",
                    AdvisoryAccepted: observation.AdvisoryDisposition == CompassSeedAdvisoryDisposition.Accepted,
                    AdvisoryDecision: "advisory",
                    AdvisoryTrace: "response-ready",
                    AdvisoryConfidence: observation.AdvisoryConfidence,
                    AdvisorySuggestedActiveBasin: observation.ActiveBasin,
                    AdvisorySuggestedCompetingBasin: observation.CompetingBasin,
                    AdvisorySuggestedAnchorState: observation.AnchorState,
                    AdvisorySuggestedSelfTouchClass: CompassSelfTouchClass.ValidationTouch,
                    AdvisoryDisposition: observation.AdvisoryDisposition,
                    AdvisoryDispositionReason: observation.AdvisoryDisposition.ToString().ToLowerInvariant(),
                    AdvisoryJustification: "bounded continuity observation",
                    TimestampUtc: timestamp)));
        }

        return CompassDriftProjector.ProjectForLoop(finalLoopKey!, entries);
    }

    private static ReturnCandidateReviewRequest CreateReviewRequest(
        Guid candidateId,
        string provenance,
        string cmeId,
        string loopKey,
        int ordinal)
    {
        var contentHandle = $"agenticore-return://candidate/{ordinal}";
        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: contentHandle,
            originSurface: "prime",
            provenanceMarker: provenance,
            sourceSubsystem: "AgentiCore");
        var envelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.StewardReturnReview,
            requestedBy: "CradleTek",
            scopeHandle: $"soulframe-session://{cmeId}/{ordinal}",
            protectionClass: "cryptic-review",
            witnessRequirement: "governance-witness",
            actionableContent: actionableContent,
            parentEnvelopeId: $"control-envelope://{loopKey}");

        return new ReturnCandidateReviewRequest(
            CandidateId: candidateId,
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: cmeId,
            ContextId: Guid.NewGuid(),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: $"soulframe-session://{cmeId}/{ordinal}",
            WorkingStateHandle: $"soulframe-working://{cmeId}/{ordinal}",
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

    private sealed record ObservationSpec(
        CompassDoctrineBasin ActiveBasin,
        CompassDoctrineBasin CompetingBasin,
        CompassAnchorState AnchorState,
        CompassSeedAdvisoryDisposition AdvisoryDisposition,
        double AdvisoryConfidence = 0.71);
}
