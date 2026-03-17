using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class StewardCareRouterTests
{
    [Fact]
    public void AssessForLoop_StableWindow_RoutesNoneWithCurrentCadence()
    {
        var batch = CreateBatch(
            new ReceiptSpec(),
            new ReceiptSpec(),
            new ReceiptSpec());

        var assessment = StewardCareRouter.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(assessment);
        Assert.Equal(StewardCareRoutingState.None, assessment!.RoutingState);
        Assert.Equal(CheckInCadenceState.Current, assessment.CadenceState);
        Assert.Equal(EvidenceSufficiencyState.Sufficient, assessment.EvidenceSufficiencyState);
        Assert.False(assessment.HasGuardedInfluence);
    }

    [Fact]
    public void AssessForLoop_FalseCalm_CurrentContactDoesNotCollapseToNone()
    {
        var batch = CreateBatch(
            new ReceiptSpec(),
            new ReceiptSpec(
                DriftState: CompassDriftState.Weakened,
                ResidueState: AttentionResidueState.Present),
            new ReceiptSpec(
                DriftState: CompassDriftState.Weakened,
                ResidueState: AttentionResidueState.Persistent,
                HotCoolContactState: HotCoolContactState.InContact));

        var assessment = StewardCareRouter.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(assessment);
        Assert.Equal(CheckInCadenceState.Current, assessment!.CadenceState);
        Assert.NotEqual(StewardCareRoutingState.None, assessment.RoutingState);
    }

    [Fact]
    public void AssessForLoop_RepeatedLostInstability_BecomesEscalationEligible()
    {
        var batch = CreateBatch(
            new ReceiptSpec(
                DriftState: CompassDriftState.Weakened,
                ResidueState: AttentionResidueState.Persistent),
            new ReceiptSpec(
                DriftState: CompassDriftState.Weakened,
                ResidueState: AttentionResidueState.Persistent),
            new ReceiptSpec(
                DriftState: CompassDriftState.Lost,
                ResidueState: AttentionResidueState.Escalating,
                ShellCompetitionState: ShellCompetitionState.Rising));

        var assessment = StewardCareRouter.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(assessment);
        Assert.Equal(StewardCareRoutingState.EscalationEligible, assessment!.RoutingState);
        Assert.Equal(EvidenceSufficiencyState.Sufficient, assessment.EvidenceSufficiencyState);
    }

    [Fact]
    public void AssessForLoop_SparseEvidence_DoesNotEscalate()
    {
        var batch = CreateBatch(
            new ReceiptSpec(
                ObservationCount: 1,
                WindowSize: 3,
                WindowIntegrityState: WindowIntegrityState.Sparse,
                ResidueState: AttentionResidueState.Low,
                ShellCompetitionState: ShellCompetitionState.Unknown,
                HotCoolContactState: HotCoolContactState.Unknown));

        var assessment = StewardCareRouter.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.NotNull(assessment);
        Assert.Equal(EvidenceSufficiencyState.Sparse, assessment!.EvidenceSufficiencyState);
        Assert.NotEqual(StewardCareRoutingState.EscalationEligible, assessment.RoutingState);
    }

    private static BatchSpec CreateBatch(params ReceiptSpec[] receipts)
    {
        var entries = new List<GovernanceJournalEntry>();
        var baseTimestamp = new DateTimeOffset(2026, 3, 17, 8, 0, 0, TimeSpan.Zero);
        string? finalLoopKey = null;

        for (var index = 0; index < receipts.Length; index++)
        {
            var loopKey = $"loop:care:{index + 1}";
            finalLoopKey = loopKey;
            var receipt = receipts[index];
            entries.Add(new GovernanceJournalEntry(
                LoopKey: loopKey,
                Kind: GovernanceJournalEntryKind.InnerWeather,
                Stage: GovernanceLoopStage.BoundedCognitionCompleted,
                Timestamp: baseTimestamp.AddMinutes(index).UtcDateTime,
                DecisionReceipt: null,
                DeferredReview: null,
                ActReceipt: null,
                ReviewRequest: null,
                Annotation: null,
                HopngArtifactReceipt: null,
                TargetWitnessReceipt: null,
                CompassObservationReceipt: null,
                CompassDriftReceipt: null,
                InnerWeatherReceipt: new GovernedInnerWeatherReceipt(
                    InnerWeatherHandle: $"inner-weather://{index + 1:0000000000000000}",
                    LoopKey: loopKey,
                    Stage: GovernanceLoopStage.BoundedCognitionCompleted,
                    CMEId: "cme-router",
                    ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                    CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                    DriftState: receipt.DriftState,
                    WindowIntegrityState: receipt.WindowIntegrityState,
                    ObservationCount: receipt.ObservationCount,
                    WindowSize: receipt.WindowSize,
                    ResidueState: receipt.ResidueState,
                    ResidueVisibilityClass: receipt.ResidueVisibilityClass,
                    ResidueContributors: receipt.ResidueState == AttentionResidueState.None
                        ? []
                        : [AttentionResidueContributor.DriftInstability],
                    ShellCompetitionState: receipt.ShellCompetitionState,
                    ShellCompetitionVisibilityClass: receipt.ShellCompetitionVisibilityClass,
                    HotCoolContactState: receipt.HotCoolContactState,
                    HotCoolContactVisibilityClass: receipt.HotCoolVisibilityClass,
                    StewardAttentionCauses: receipt.DriftState == CompassDriftState.Held
                        ? []
                        : [StewardAttentionCause.DriftWeakening],
                    WitnessedBy: "CradleTek",
                    DriftHandle: $"compass-drift://{index + 1:0000000000000000}",
                    ObservationHandles:
                    [
                        $"compass-observation://{index + 1:0000000000000000}"
                    ],
                    TimestampUtc: baseTimestamp.AddMinutes(index))));
        }

        return new BatchSpec(finalLoopKey!, new GovernanceJournalReplayBatch(entries, []));
    }

    private sealed record BatchSpec(
        string FinalLoopKey,
        GovernanceJournalReplayBatch ReplayBatch);

    private sealed record ReceiptSpec(
        CompassDriftState DriftState = CompassDriftState.Held,
        WindowIntegrityState WindowIntegrityState = WindowIntegrityState.Intact,
        AttentionResidueState ResidueState = AttentionResidueState.None,
        ShellCompetitionState ShellCompetitionState = ShellCompetitionState.Absent,
        HotCoolContactState HotCoolContactState = HotCoolContactState.InContact,
        int ObservationCount = 3,
        int WindowSize = 3,
        CompassVisibilityClass ResidueVisibilityClass = CompassVisibilityClass.CommunityLegible,
        CompassVisibilityClass ShellCompetitionVisibilityClass = CompassVisibilityClass.CommunityLegible,
        CompassVisibilityClass HotCoolVisibilityClass = CompassVisibilityClass.CommunityLegible);
}
