using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class InnerWeatherDisclosureReducerTests
{
    [Fact]
    public void Reduce_CommunitySafeAssessment_RemainsCommunityScoped()
    {
        var decision = InnerWeatherDisclosureReducer.Reduce(CreateAssessment());

        Assert.Equal(WeatherDisclosureScope.Community, decision.DisclosureScope);
        Assert.Equal(WeatherDisclosureRationaleCode.CommunityWhitelisted, decision.RationaleCode);
        Assert.Empty(decision.WithheldMarkers);
    }

    [Fact]
    public void Reduce_GuardedInfluence_IsWithheldButUseful()
    {
        var decision = InnerWeatherDisclosureReducer.Reduce(CreateAssessment(
            routingState: StewardCareRoutingState.CheckInRecommended,
            hasGuardedInfluence: true,
            reasonCodes:
            [
                StewardAttentionCause.ResiduePersistence,
                StewardAttentionCause.DriftWeakening
            ]));

        Assert.Equal(WeatherDisclosureScope.Steward, decision.DisclosureScope);
        Assert.Contains(WeatherWithheldMarker.GuardedEvidence, decision.WithheldMarkers);
        Assert.Contains(StewardAttentionCause.ResiduePersistence, decision.StewardReasonCodes);
        Assert.Equal(CommunityWeatherStatus.Stable, decision.CommunityWeatherPacket.Status);
    }

    [Fact]
    public void Reduce_BrokenWindow_BecomesOperatorGuarded()
    {
        var decision = InnerWeatherDisclosureReducer.Reduce(CreateAssessment(
            routingState: StewardCareRoutingState.CheckInRecommended,
            evidenceSufficiencyState: EvidenceSufficiencyState.BrokenWindow,
            windowIntegrityState: WindowIntegrityState.JournalGap));

        Assert.Equal(WeatherDisclosureScope.OperatorGuarded, decision.DisclosureScope);
        Assert.Contains(WeatherWithheldMarker.BrokenWindow, decision.WithheldMarkers);
        Assert.Equal(WeatherDisclosureRationaleCode.BrokenWindowReduction, decision.RationaleCode);
    }

    [Fact]
    public void Reduce_CrypticInfluence_DoesNotWidenCommunityFields()
    {
        var decision = InnerWeatherDisclosureReducer.Reduce(CreateAssessment(
            routingState: StewardCareRoutingState.EscalationEligible,
            hasGuardedInfluence: true,
            hasCrypticInfluence: true));

        Assert.Equal(WeatherDisclosureScope.OperatorGuarded, decision.DisclosureScope);
        Assert.Contains(WeatherWithheldMarker.CrypticEvidence, decision.WithheldMarkers);
        Assert.Equal(
            [
                CommunityWeatherField.Status,
                CommunityWeatherField.StewardAttention,
                CommunityWeatherField.AnchorState,
                CommunityWeatherField.VisibilityClass,
                CommunityWeatherField.TimestampUtc
            ],
            decision.AllowedCommunityFields);
    }

    private static StewardCareAssessment CreateAssessment(
        StewardCareRoutingState routingState = StewardCareRoutingState.None,
        CheckInCadenceState cadenceState = CheckInCadenceState.Current,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient,
        WindowIntegrityState windowIntegrityState = WindowIntegrityState.Intact,
        bool hasGuardedInfluence = false,
        bool hasCrypticInfluence = false,
        IReadOnlyList<StewardAttentionCause>? reasonCodes = null)
    {
        return new StewardCareAssessment(
            CMEId: "cme-disclosure",
            RoutingState: routingState,
            CadenceState: cadenceState,
            EvidenceSufficiencyState: evidenceSufficiencyState,
            WindowIntegrityState: windowIntegrityState,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Stable,
                StewardAttention: CommunityStewardAttentionState.None,
                AnchorState: CompassDriftState.Held,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: DateTimeOffset.UtcNow),
            HasGuardedInfluence: hasGuardedInfluence,
            HasCrypticInfluence: hasCrypticInfluence,
            ReasonCodes: reasonCodes ?? [],
            InnerWeatherHandle: "inner-weather://aaaaaaaaaaaaaaaa",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
