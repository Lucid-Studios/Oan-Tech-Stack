namespace Oan.Audit.Tests;

using System.Text.Json;
using Oan.Common;

public sealed class ListeningFrameCompassContractsTests
{
    [Fact]
    public void ListeningFrameProjectionPacket_RoundTrips_AsCandidateOnlyPostureSurface()
    {
        var packet = CreateListeningFrameProjectionPacket();

        var json = JsonSerializer.Serialize(packet);
        var roundTrip = JsonSerializer.Deserialize<ListeningFrameProjectionPacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(packet.PacketHandle, roundTrip!.PacketHandle);
        Assert.Equal(ListeningFrameVisibilityPosture.OperatorGuarded, roundTrip.VisibilityPosture);
        Assert.Equal(ListeningFrameIntegrityState.Usable, roundTrip.IntegrityState);
        Assert.Equal(ListeningFrameReviewPosture.CandidateOnly, roundTrip.ReviewPosture);
        Assert.True(roundTrip.UsableForCompassProjection);
    }

    [Fact]
    public void CompassProjectionPacket_RoundTrips_AsNonSovereignCandidateRead()
    {
        var packet = CreateCompassProjectionPacket();

        var json = JsonSerializer.Serialize(packet);
        var roundTrip = JsonSerializer.Deserialize<CompassProjectionPacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(packet.PacketHandle, roundTrip!.PacketHandle);
        Assert.Equal(CompassDriftState.Weakened, roundTrip.DriftState);
        Assert.Equal(CompassOrientationPosture.Seeking, roundTrip.OrientationPosture);
        Assert.Equal(CompassAdmissibilityEstimate.Reviewable, roundTrip.AdmissibilityEstimate);
        Assert.Equal(CompassTransitionRecommendation.RepairRecommended, roundTrip.TransitionRecommendation);
        Assert.Equal(CompassAuthorityPosture.CandidateOnly, roundTrip.AuthorityPosture);
        Assert.Single(roundTrip.CandidateInputs);
    }

    [Fact]
    public void FirstRunLivingAgentiCorePacket_CarriesTypedAttachments_BesideLegacyHandles()
    {
        var packet = new FirstRunLivingAgentiCorePacket(
            PacketHandle: "packet://first-run/living-agenticore",
            LivingAgentiCoreHandle: "agenticore://living/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            ZedOfDeltaHandle: "zed://delta/session-a",
            SelfGelAttachmentHandle: "selfgel://attachment/session-a",
            ToolUseContextHandle: "tools://context/session-a",
            CompassEmbodimentHandle: "compass://embodiment/session-a",
            EngineeredCognitionHandle: "ec://session-a",
            WiderPublicWideningWithheld: true,
            TimestampUtc: new DateTimeOffset(2026, 04, 08, 19, 15, 00, TimeSpan.Zero),
            ListeningFrameProjectionPacket: CreateListeningFrameProjectionPacket(),
            CompassProjectionPacket: CreateCompassProjectionPacket());

        var json = JsonSerializer.Serialize(packet);
        var roundTrip = JsonSerializer.Deserialize<FirstRunLivingAgentiCorePacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("listening://frame/session-a", roundTrip!.ListeningFrameHandle);
        Assert.Equal("compass://embodiment/session-a", roundTrip.CompassEmbodimentHandle);
        Assert.NotNull(roundTrip.ListeningFrameProjectionPacket);
        Assert.NotNull(roundTrip.CompassProjectionPacket);
        Assert.Equal(ListeningFrameReviewPosture.CandidateOnly, roundTrip.ListeningFrameProjectionPacket!.ReviewPosture);
        Assert.Equal(CompassAuthorityPosture.CandidateOnly, roundTrip.CompassProjectionPacket!.AuthorityPosture);
    }

    [Fact]
    public void GoverningOfficeAuthorityAssessment_CarriesPosturePackets_WithoutGrantingAuthority()
    {
        var assessment = new GoverningOfficeAuthorityAssessment(
            CMEId: "cme://bonded/session-a",
            Office: InternalGoverningCmeOffice.Mother,
            AuthoritySurface: OfficeAuthoritySurface.GuardedReviewSurface,
            ViewEligibility: OfficeViewEligibility.GuardedView,
            AcknowledgmentEligibility: OfficeAcknowledgmentEligibility.Allowed,
            ActionEligibility: OfficeActionEligibility.ViewOnly,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sparse,
            WindowIntegrityState: WindowIntegrityState.Intact,
            DisclosureScope: WeatherDisclosureScope.OperatorGuarded,
            OfficeAttached: true,
            BondedConfirmed: true,
            GuardedReviewConfirmed: true,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.OperatorGuarded,
                TimestampUtc: new DateTimeOffset(2026, 04, 08, 19, 30, 00, TimeSpan.Zero),
                ListeningFrameProjectionPacket: CreateListeningFrameProjectionPacket(),
                CompassProjectionPacket: CreateCompassProjectionPacket()),
            SourceReasonCodes: [StewardAttentionCause.DriftWeakening],
            SourceWithheldMarkers: [WeatherWithheldMarker.GuardedEvidence],
            Prohibitions:
            [
                OfficeAuthorityProhibition.MayNotOriginateTruth,
                OfficeAuthorityProhibition.MayNotWidenDisclosure
            ],
            WeatherDisclosureHandle: "weather://session-a",
            TimestampUtc: new DateTimeOffset(2026, 04, 08, 19, 35, 00, TimeSpan.Zero));

        Assert.Equal(OfficeActionEligibility.ViewOnly, assessment.ActionEligibility);
        Assert.Contains(OfficeAuthorityProhibition.MayNotOriginateTruth, assessment.Prohibitions);
        Assert.Contains(OfficeAuthorityProhibition.MayNotWidenDisclosure, assessment.Prohibitions);
        Assert.NotNull(assessment.CommunityWeatherPacket.ListeningFrameProjectionPacket);
        Assert.NotNull(assessment.CommunityWeatherPacket.CompassProjectionPacket);
        Assert.Equal(CompassAuthorityPosture.CandidateOnly, assessment.CommunityWeatherPacket.CompassProjectionPacket!.AuthorityPosture);
    }

    [Fact]
    public void Docs_BridgePhenomenology_To_RuntimeContractFamily()
    {
        var lineRoot = GetLineRoot();
        var bridgePath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var primeCrypticPath = Path.Combine(lineRoot, "docs", "PRIME_CRYPTIC_DUPLEX_LAW.md");
        var mosPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");

        Assert.True(File.Exists(bridgePath));

        var bridgeText = File.ReadAllText(bridgePath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);
        var primeCrypticText = File.ReadAllText(primeCrypticPath);
        var mosText = File.ReadAllText(mosPath);
        var firstRunText = File.ReadAllText(firstRunPath);

        Assert.Contains("ListeningFrame` is the loomed resonant body", bridgeText, StringComparison.Ordinal);
        Assert.Contains("Compass` is the strumming surface", bridgeText, StringComparison.Ordinal);
        Assert.Contains("ListeningFrameContracts.cs", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("CompassContracts.cs", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", primeCrypticText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", mosText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", firstRunText, StringComparison.Ordinal);
    }

    private static ListeningFrameProjectionPacket CreateListeningFrameProjectionPacket()
    {
        return new ListeningFrameProjectionPacket(
            PacketHandle: "packet://listening-frame/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            ChamberHandle: "soulframe://session-a",
            SourceSurfaceHandle: "agenticore://session-a",
            VisibilityPosture: ListeningFrameVisibilityPosture.OperatorGuarded,
            IntegrityState: ListeningFrameIntegrityState.Usable,
            ReviewPosture: ListeningFrameReviewPosture.CandidateOnly,
            UsableForCompassProjection: true,
            PostureMarkers: ["posture:legible", "drift:reviewable"],
            ReviewNotes: ["candidate-only-posture-surface"],
            TimestampUtc: new DateTimeOffset(2026, 04, 08, 19, 00, 00, TimeSpan.Zero));
    }

    private static CompassProjectionPacket CreateCompassProjectionPacket()
    {
        return new CompassProjectionPacket(
            PacketHandle: "packet://compass/session-a",
            CompassEmbodimentHandle: "compass://embodiment/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            DriftState: CompassDriftState.Weakened,
            OrientationPosture: CompassOrientationPosture.Seeking,
            AdmissibilityEstimate: CompassAdmissibilityEstimate.Reviewable,
            TransitionRecommendation: CompassTransitionRecommendation.RepairRecommended,
            AuthorityPosture: CompassAuthorityPosture.CandidateOnly,
            CandidateInputs:
            [
                new CompassCandidateModulationInput(
                    InputHandle: "modulation://repair/session-a",
                    InputKind: "repair-band",
                    SourceReason: "drift-weakening")
            ],
            ReviewNotes: ["candidate-read-only", "promotion-still-withheld"],
            TimestampUtc: new DateTimeOffset(2026, 04, 08, 19, 05, 00, TimeSpan.Zero));
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
