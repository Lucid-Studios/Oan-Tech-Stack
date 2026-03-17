using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GoverningOfficeAuthorityViewReducerTests
{
    [Fact]
    public void Reduce_WithheldOffice_AddsOfficeNotAttachedMarker()
    {
        var view = GoverningOfficeAuthorityViewReducer.Reduce(CreateAssessment(
            office: InternalGoverningCmeOffice.Father,
            viewEligibility: OfficeViewEligibility.Withheld,
            officeAttached: false));

        Assert.Null(view.CommunityWeatherPacket);
        Assert.Contains(OfficeAuthorityWithheldMarker.OfficeNotAttached, view.WithheldMarkers);
        Assert.Equal(OfficeAuthorityRationaleCode.WithheldForOfficeNotAttached, view.RationaleCode);
    }

    [Fact]
    public void Reduce_StewardOfficeSpecificView_PassesThroughReasonCodes()
    {
        var view = GoverningOfficeAuthorityViewReducer.Reduce(CreateAssessment(
            office: InternalGoverningCmeOffice.Steward,
            viewEligibility: OfficeViewEligibility.OfficeSpecificView,
            actionEligibility: OfficeActionEligibility.CheckInAllowed,
            sourceReasonCodes:
            [
                StewardAttentionCause.DriftWeakening,
                StewardAttentionCause.ResiduePersistence
            ]));

        Assert.Equal(
            [
                StewardAttentionCause.DriftWeakening,
                StewardAttentionCause.ResiduePersistence
            ],
            view.AllowedReasonCodes);
        Assert.Equal(OfficeAuthorityRationaleCode.OfficeSpecificStewardView, view.RationaleCode);
    }

    [Fact]
    public void Reduce_FatherGuardedView_FiltersToBoundaryReasons()
    {
        var view = GoverningOfficeAuthorityViewReducer.Reduce(CreateAssessment(
            office: InternalGoverningCmeOffice.Father,
            viewEligibility: OfficeViewEligibility.GuardedView,
            sourceReasonCodes:
            [
                StewardAttentionCause.DriftLoss,
                StewardAttentionCause.ResiduePersistence,
                StewardAttentionCause.WindowIntegrityBreak
            ]));

        Assert.Equal(
            [
                StewardAttentionCause.DriftLoss,
                StewardAttentionCause.WindowIntegrityBreak
            ],
            view.AllowedReasonCodes);
        Assert.Equal(OfficeAuthorityRationaleCode.GuardedViewForOffice, view.RationaleCode);
    }

    [Fact]
    public void Reduce_MotherWithoutBond_AddsBondRequiredMarker()
    {
        var view = GoverningOfficeAuthorityViewReducer.Reduce(CreateAssessment(
            office: InternalGoverningCmeOffice.Mother,
            viewEligibility: OfficeViewEligibility.Withheld,
            officeAttached: true,
            bondedConfirmed: false,
            disclosureScope: WeatherDisclosureScope.OperatorGuarded,
            sourceWithheldMarkers:
            [
                WeatherWithheldMarker.GuardedEvidence
            ]));

        Assert.Contains(OfficeAuthorityWithheldMarker.GuardedEvidence, view.WithheldMarkers);
        Assert.Contains(OfficeAuthorityWithheldMarker.BondRequired, view.WithheldMarkers);
        Assert.Equal(OfficeAuthorityRationaleCode.WithheldForAuthorityRequirement, view.RationaleCode);
    }

    [Fact]
    public void Reduce_CommunityReducedView_StripsReasonCodes()
    {
        var view = GoverningOfficeAuthorityViewReducer.Reduce(CreateAssessment(
            office: InternalGoverningCmeOffice.Mother,
            viewEligibility: OfficeViewEligibility.CommunityReduced,
            actionEligibility: OfficeActionEligibility.AcknowledgeAllowed,
            sourceReasonCodes:
            [
                StewardAttentionCause.ResiduePersistence
            ]));

        Assert.Empty(view.AllowedReasonCodes);
        Assert.Equal(OfficeAuthorityRationaleCode.CommunityReducedForOffice, view.RationaleCode);
    }

    private static GoverningOfficeAuthorityAssessment CreateAssessment(
        InternalGoverningCmeOffice office,
        OfficeViewEligibility viewEligibility,
        OfficeActionEligibility actionEligibility = OfficeActionEligibility.ViewOnly,
        bool officeAttached = true,
        bool bondedConfirmed = false,
        WeatherDisclosureScope disclosureScope = WeatherDisclosureScope.Steward,
        IReadOnlyList<StewardAttentionCause>? sourceReasonCodes = null,
        IReadOnlyList<WeatherWithheldMarker>? sourceWithheldMarkers = null)
    {
        return new GoverningOfficeAuthorityAssessment(
            CMEId: "cme-authority-view",
            Office: office,
            AuthoritySurface: office switch
            {
                InternalGoverningCmeOffice.Steward => OfficeAuthoritySurface.StewardSurface,
                InternalGoverningCmeOffice.Father => OfficeAuthoritySurface.GuardedReviewSurface,
                _ => OfficeAuthoritySurface.PrimeCareSurface
            },
            ViewEligibility: viewEligibility,
            AcknowledgmentEligibility: officeAttached && viewEligibility != OfficeViewEligibility.Withheld
                ? OfficeAcknowledgmentEligibility.Allowed
                : OfficeAcknowledgmentEligibility.NotAllowed,
            ActionEligibility: actionEligibility,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sufficient,
            WindowIntegrityState: WindowIntegrityState.Intact,
            DisclosureScope: disclosureScope,
            OfficeAttached: officeAttached,
            BondedConfirmed: bondedConfirmed,
            GuardedReviewConfirmed: office == InternalGoverningCmeOffice.Father,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: DateTimeOffset.UtcNow),
            SourceReasonCodes: sourceReasonCodes ?? [],
            SourceWithheldMarkers: sourceWithheldMarkers ?? [],
            Prohibitions:
            [
                OfficeAuthorityProhibition.MayNotOriginateTruth,
                OfficeAuthorityProhibition.MayNotWidenDisclosure
            ],
            WeatherDisclosureHandle: "weather-disclosure://cccccccccccccccc",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
