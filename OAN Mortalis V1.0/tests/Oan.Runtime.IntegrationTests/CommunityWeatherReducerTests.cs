using System.Text.Json;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class CommunityWeatherReducerTests
{
    [Fact]
    public void Reduce_IntactHeldReceipt_ReturnsStablePacket()
    {
        var packet = CommunityWeatherReducer.Reduce(CreateReceipt(
            driftState: CompassDriftState.Held,
            windowIntegrityState: WindowIntegrityState.Intact,
            residueState: AttentionResidueState.None,
            shellCompetitionState: ShellCompetitionState.Absent,
            hotCoolContactState: HotCoolContactState.InContact));

        Assert.Equal(CommunityWeatherStatus.Stable, packet.Status);
        Assert.Equal(CommunityStewardAttentionState.None, packet.StewardAttention);
        Assert.Equal(CompassVisibilityClass.CommunityLegible, packet.VisibilityClass);
    }

    [Fact]
    public void Reduce_SparseEvidence_BiasesToUnknown()
    {
        var packet = CommunityWeatherReducer.Reduce(CreateReceipt(
            driftState: CompassDriftState.Held,
            windowIntegrityState: WindowIntegrityState.Sparse,
            observationCount: 2,
            residueState: AttentionResidueState.Low,
            shellCompetitionState: ShellCompetitionState.Unknown,
            hotCoolContactState: HotCoolContactState.Unknown));

        Assert.Equal(CommunityWeatherStatus.Unknown, packet.Status);
        Assert.Equal(CommunityStewardAttentionState.Recommended, packet.StewardAttention);
    }

    [Fact]
    public void Reduce_MissedCheckIn_ReturnsMissedCheckInAndNeeded()
    {
        var packet = CommunityWeatherReducer.Reduce(CreateReceipt(
            driftState: CompassDriftState.Weakened,
            windowIntegrityState: WindowIntegrityState.JournalGap,
            residueState: AttentionResidueState.Present,
            shellCompetitionState: ShellCompetitionState.Present,
            hotCoolContactState: HotCoolContactState.MissedCheckIn));

        Assert.Equal(CommunityWeatherStatus.MissedCheckIn, packet.Status);
        Assert.Equal(CommunityStewardAttentionState.Needed, packet.StewardAttention);
    }

    [Fact]
    public void Reduce_GuardedReceipt_DoesNotSerializeRawGuardedFields()
    {
        var packet = CommunityWeatherReducer.Reduce(CreateReceipt(
            driftState: CompassDriftState.Weakened,
            windowIntegrityState: WindowIntegrityState.Intact,
            residueState: AttentionResidueState.Persistent,
            shellCompetitionState: ShellCompetitionState.Rising,
            hotCoolContactState: HotCoolContactState.Cool,
            residueVisibilityClass: CompassVisibilityClass.CrypticOnly,
            shellCompetitionVisibilityClass: CompassVisibilityClass.OperatorGuarded,
            hotCoolVisibilityClass: CompassVisibilityClass.OperatorGuarded));

        var json = JsonSerializer.Serialize(packet);

        Assert.Equal(CommunityWeatherStatus.Degraded, packet.Status);
        Assert.DoesNotContain("Persistent", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Rising", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Cool", json, StringComparison.Ordinal);
        Assert.DoesNotContain("CrypticOnly", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Reduce_CoolPosture_DoesNotBecomeAllClearWhenOtherPressureExists()
    {
        var packet = CommunityWeatherReducer.Reduce(CreateReceipt(
            driftState: CompassDriftState.Weakened,
            windowIntegrityState: WindowIntegrityState.Intact,
            residueState: AttentionResidueState.Present,
            shellCompetitionState: ShellCompetitionState.Absent,
            hotCoolContactState: HotCoolContactState.Cool));

        Assert.Equal(CommunityWeatherStatus.Unstable, packet.Status);
        Assert.NotEqual(CommunityWeatherStatus.Stable, packet.Status);
    }

    private static GovernedInnerWeatherReceipt CreateReceipt(
        CompassDriftState driftState,
        WindowIntegrityState windowIntegrityState,
        AttentionResidueState residueState,
        ShellCompetitionState shellCompetitionState,
        HotCoolContactState hotCoolContactState,
        int observationCount = 3,
        int windowSize = 3,
        CompassVisibilityClass residueVisibilityClass = CompassVisibilityClass.CommunityLegible,
        CompassVisibilityClass shellCompetitionVisibilityClass = CompassVisibilityClass.CommunityLegible,
        CompassVisibilityClass hotCoolVisibilityClass = CompassVisibilityClass.CommunityLegible)
    {
        return new GovernedInnerWeatherReceipt(
            InnerWeatherHandle: "inner-weather://aaaaaaaaaaaaaaaa",
            LoopKey: "loop:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-weather",
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            DriftState: driftState,
            WindowIntegrityState: windowIntegrityState,
            ObservationCount: observationCount,
            WindowSize: windowSize,
            ResidueState: residueState,
            ResidueVisibilityClass: residueVisibilityClass,
            ResidueContributors: residueState == AttentionResidueState.None
                ? []
                : [AttentionResidueContributor.DriftInstability],
            ShellCompetitionState: shellCompetitionState,
            ShellCompetitionVisibilityClass: shellCompetitionVisibilityClass,
            HotCoolContactState: hotCoolContactState,
            HotCoolContactVisibilityClass: hotCoolVisibilityClass,
            StewardAttentionCauses: driftState == CompassDriftState.Held
                ? []
                : [StewardAttentionCause.DriftWeakening],
            WitnessedBy: "CradleTek",
            DriftHandle: "compass-drift://bbbbbbbbbbbbbbbb",
            ObservationHandles:
            [
                "compass-observation://1111111111111111",
                "compass-observation://2222222222222222",
                "compass-observation://3333333333333333"
            ],
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
