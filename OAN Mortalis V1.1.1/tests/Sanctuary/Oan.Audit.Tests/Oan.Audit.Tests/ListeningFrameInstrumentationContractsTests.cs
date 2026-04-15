namespace San.Audit.Tests;

using Oan.Common;

public sealed class ListeningFrameInstrumentationContractsTests
{
    [Fact]
    public void ListeningFrameInstrumentation_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                ListeningFrameInstrumentationBand.Stable,
                ListeningFrameInstrumentationBand.Reviewable,
                ListeningFrameInstrumentationBand.RepairBearing,
                ListeningFrameInstrumentationBand.Withheld
            ],
            Enum.GetValues<ListeningFrameInstrumentationBand>());

        Assert.Equal(
            [
                ListeningFrameInstrumentationDisposition.Observe,
                ListeningFrameInstrumentationDisposition.Review,
                ListeningFrameInstrumentationDisposition.Repair,
                ListeningFrameInstrumentationDisposition.Withhold
            ],
            Enum.GetValues<ListeningFrameInstrumentationDisposition>());
    }

    [Fact]
    public void Weakened_Drift_Emits_RepairBearing_Instrumentation_Receipt()
    {
        var receipt = ListeningFrameInstrumentationEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateCompassProjectionPacket(),
            "receipt://listening-frame-instrumentation/repair");

        Assert.Equal(ListeningFrameInstrumentationBand.RepairBearing, receipt.InstrumentationBand);
        Assert.Equal(ListeningFrameInstrumentationDisposition.Repair, receipt.Disposition);
        Assert.True(receipt.CandidateOnly);
        Assert.True(receipt.PersistenceAuthorityWithheld);
        Assert.True(receipt.ContinuityAdmissionWithheld);
        Assert.Contains("listening-frame-instrumentation-drift-weakened", receipt.ConstraintCodes);
        Assert.Contains("modulation://repair/session-a", receipt.CandidateInputHandles);
    }

    [Fact]
    public void Handle_Mismatch_Withholds_Instrumentation()
    {
        var listeningFrame = CreateListeningFrameProjectionPacket() with
        {
            ListeningFrameHandle = "listening://frame/session-a"
        };
        var compass = CreateCompassProjectionPacket() with
        {
            ListeningFrameHandle = "listening://frame/session-b"
        };

        var receipt = ListeningFrameInstrumentationEvaluator.Evaluate(
            listeningFrame,
            compass,
            "receipt://listening-frame-instrumentation/mismatch");

        Assert.Equal(ListeningFrameInstrumentationBand.Withheld, receipt.InstrumentationBand);
        Assert.Equal(ListeningFrameInstrumentationDisposition.Withhold, receipt.Disposition);
        Assert.Equal("listening-frame-instrumentation-listening-frame-handle-mismatch", receipt.ReasonCode);
        Assert.Contains("listening-frame-instrumentation-listening-frame-handle-mismatch", receipt.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_ListeningFrame_Instrumentation_Receipt_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_INSTRUMENTATION_RECEIPT_LAW.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var bridgePath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);
        var bridgeText = File.ReadAllText(bridgePath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("bounded instrumentation receipt", lawText, StringComparison.Ordinal);
        Assert.Contains("it may not grant persistence, continuity admission", lawText, StringComparison.Ordinal);
        Assert.Contains("pulse by itself", lawText, StringComparison.Ordinal);
        Assert.Contains("ListeningFrameInstrumentationContracts.cs", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("explicit instrumentation", bridgeText, StringComparison.Ordinal);
        Assert.Contains("bounded instrumentation receipts", firstRunText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_INSTRUMENTATION_RECEIPT_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("listening-frame-instrumentation-receipt-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("ListeningFrame instrumentation receipt preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_INSTRUMENTATION_RECEIPT_LAW.md", baselineText, StringComparison.Ordinal);
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
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 18, 00, 00, TimeSpan.Zero));
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
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 18, 05, 00, TimeSpan.Zero));
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
