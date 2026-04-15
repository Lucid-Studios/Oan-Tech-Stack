namespace Oan.Audit.Tests;

using System.Text.Json;
using Oan.Common;
using San.Common;

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
            CompassProjectionPacket: CreateCompassProjectionPacket(),
            ListeningFrameInstrumentationReceipt: CreateInstrumentationReceipt(),
            ZedDeltaSelfBasisReceipt: CreateZedDeltaSelfBasisReceipt(),
            ThetaIngressSensoryClusterReceipt: CreateThetaIngressReceipt(),
            PostIngressDiscernmentReceipt: CreatePostIngressDiscernmentReceipt());

        var json = JsonSerializer.Serialize(packet);
        var roundTrip = JsonSerializer.Deserialize<FirstRunLivingAgentiCorePacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("listening://frame/session-a", roundTrip!.ListeningFrameHandle);
        Assert.Equal("compass://embodiment/session-a", roundTrip.CompassEmbodimentHandle);
        Assert.NotNull(roundTrip.ListeningFrameProjectionPacket);
        Assert.NotNull(roundTrip.CompassProjectionPacket);
        Assert.NotNull(roundTrip.ListeningFrameInstrumentationReceipt);
        Assert.NotNull(roundTrip.ZedDeltaSelfBasisReceipt);
        Assert.NotNull(roundTrip.ThetaIngressSensoryClusterReceipt);
        Assert.NotNull(roundTrip.PostIngressDiscernmentReceipt);
        Assert.Equal(ListeningFrameReviewPosture.CandidateOnly, roundTrip.ListeningFrameProjectionPacket!.ReviewPosture);
        Assert.Equal(CompassAuthorityPosture.CandidateOnly, roundTrip.CompassProjectionPacket!.AuthorityPosture);
        Assert.Equal(ListeningFrameInstrumentationBand.RepairBearing, roundTrip.ListeningFrameInstrumentationReceipt!.InstrumentationBand);
        Assert.True(roundTrip.ZedDeltaSelfBasisReceipt!.StoredInSoulFrame);
        Assert.True(roundTrip.ZedDeltaSelfBasisReceipt.CastIntoListeningFrame);
        Assert.True(roundTrip.ThetaIngressSensoryClusterReceipt!.ContextualizationBegun);
        Assert.Equal(
            PostIngressDiscernmentStateKind.Investigatory,
            roundTrip.PostIngressDiscernmentReceipt!.DiscernmentState);
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
                CompassProjectionPacket: CreateCompassProjectionPacket(),
                ListeningFrameInstrumentationReceipt: CreateInstrumentationReceipt(),
                ZedDeltaSelfBasisReceipt: CreateZedDeltaSelfBasisReceipt(),
                ThetaIngressSensoryClusterReceipt: CreateThetaIngressReceipt(),
                PostIngressDiscernmentReceipt: CreatePostIngressDiscernmentReceipt()),
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
        Assert.NotNull(assessment.CommunityWeatherPacket.ListeningFrameInstrumentationReceipt);
        Assert.NotNull(assessment.CommunityWeatherPacket.ZedDeltaSelfBasisReceipt);
        Assert.NotNull(assessment.CommunityWeatherPacket.ThetaIngressSensoryClusterReceipt);
        Assert.NotNull(assessment.CommunityWeatherPacket.PostIngressDiscernmentReceipt);
        Assert.Equal(CompassAuthorityPosture.CandidateOnly, assessment.CommunityWeatherPacket.CompassProjectionPacket!.AuthorityPosture);
        Assert.Equal(ListeningFrameInstrumentationDisposition.Repair, assessment.CommunityWeatherPacket.ListeningFrameInstrumentationReceipt!.Disposition);
        Assert.Contains(
            ZedBasisDirectionKind.Ahead,
            assessment.CommunityWeatherPacket.ZedDeltaSelfBasisReceipt!.CardinalDirections);
        Assert.Equal(
            ThetaIngressStatusKind.Lawful,
            assessment.CommunityWeatherPacket.ThetaIngressSensoryClusterReceipt!.IngressStatus);
        Assert.Contains(
            "question://thread/session-a/resolve-ambiguity",
            assessment.CommunityWeatherPacket.PostIngressDiscernmentReceipt!.QuestionHandles);
    }

    [Fact]
    public void Docs_BridgePhenomenology_To_RuntimeContractFamily()
    {
        var lineRoot = GetLineRoot();
        var bridgePath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var zedBasisLawPath = Path.Combine(lineRoot, "docs", "ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md");
        var discernmentLawPath = Path.Combine(lineRoot, "docs", "POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md");
        var primeCrypticPath = Path.Combine(lineRoot, "docs", "PRIME_CRYPTIC_DUPLEX_LAW.md");
        var mosPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var sanctuaryBiadPath = Path.Combine(lineRoot, "docs", "SANCTUARY_BIAD_AND_CRADLETEK_GOVERNING_SURFACE_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(bridgePath));

        var bridgeText = File.ReadAllText(bridgePath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);
        var zedBasisLawText = File.ReadAllText(zedBasisLawPath);
        var discernmentLawText = File.ReadAllText(discernmentLawPath);
        var primeCrypticText = File.ReadAllText(primeCrypticPath);
        var mosText = File.ReadAllText(mosPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var sanctuaryBiadText = File.ReadAllText(sanctuaryBiadPath);
        var readinessText = File.ReadAllText(readinessPath);

        Assert.Contains("ListeningFrame` is the loomed resonant body", bridgeText, StringComparison.Ordinal);
        Assert.Contains("Compass` is the strumming surface", bridgeText, StringComparison.Ordinal);
        Assert.Contains("lawful pulse", bridgeText, StringComparison.Ordinal);
        Assert.Contains("ListeningFrameContracts.cs", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("CompassContracts.cs", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_BIAD_AND_CRADLETEK_GOVERNING_SURFACE_NOTE.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("lawful continuity", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", primeCrypticText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", mosText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md", firstRunText, StringComparison.Ordinal);
        Assert.Contains("ListeningFrame / Compass loom-weave bridge now lives in", readinessText, StringComparison.Ordinal);
        Assert.Contains("listening-frame-compass-loom-weave-bridge: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary holds the biad", bridgeText, StringComparison.Ordinal);
        Assert.Contains("OE and SelfGEL remain stored in SoulFrame", bridgeText, StringComparison.Ordinal);
        Assert.Contains("cOE and cSelfGEL are cast surfaces", bridgeText, StringComparison.Ordinal);
        Assert.Contains("Ahead", zedBasisLawText, StringComparison.Ordinal);
        Assert.Contains("stable one", discernmentLawText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sanctuary holds the biad", sanctuaryBiadText, StringComparison.Ordinal);
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

    private static ListeningFrameInstrumentationReceipt CreateInstrumentationReceipt()
    {
        return ListeningFrameInstrumentationEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateCompassProjectionPacket(),
            "receipt://listening-frame-instrumentation/session-a");
    }

    private static ZedDeltaSelfBasisReceipt CreateZedDeltaSelfBasisReceipt()
    {
        return ZedDeltaSelfBasisEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            soulFrameHandle: "soulframe://session-a",
            oeHandle: "oe://session-a",
            selfGelHandle: "selfgel://session-a",
            cOeHandle: "coe://session-a",
            cSelfGelHandle: "cselfgel://session-a",
            zedOfDeltaHandle: "zed://delta/session-a",
            engineeredCognitionHandle: "ec://session-a",
            ecIuttLispMatrixHandle: "iutt-lisp://matrix/session-a",
            receiptHandle: "receipt://zed-delta-self-basis/session-a");
    }

    private static ThetaIngressSensoryClusterReceipt CreateThetaIngressReceipt()
    {
        return ThetaIngressSensoryClusterEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateZedDeltaSelfBasisReceipt(),
            thetaHandle: "theta://thread/session-a",
            thetaMarkers:
            [
                "theta:live-thread",
                "theta:crosses-center"
            ],
            receiptHandle: "receipt://theta-ingress/session-a");
    }

    private static PostIngressDiscernmentReceipt CreatePostIngressDiscernmentReceipt()
    {
        return PostIngressDiscernmentEvaluator.Evaluate(
            CreateThetaIngressReceipt(),
            stableOneAchieved: false,
            stableOneHandle: null,
            discernmentSignals:
            [
                PostIngressDiscernmentSignalKind.Ambiguity,
                PostIngressDiscernmentSignalKind.GroundingInsufficient
            ],
            questionHandles:
            [
                "question://thread/session-a/resolve-ambiguity"
            ],
            enrichmentHandles:
            [
                "enrichment://thread/session-a/source-check"
            ],
            carriedIncompleteHandles: [],
            receiptHandle: "receipt://post-ingress-discernment/session-a");
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
