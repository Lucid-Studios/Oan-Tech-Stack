namespace San.Audit.Tests;

using San.Common;
using San.Common;

public sealed class ZedDeltaSelfBasisContractsTests
{
    [Fact]
    public void ZedDeltaSelfBasis_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                ZedBasisDirectionKind.Center,
                ZedBasisDirectionKind.Ahead,
                ZedBasisDirectionKind.Behind,
                ZedBasisDirectionKind.Above,
                ZedBasisDirectionKind.Below,
                ZedBasisDirectionKind.Left,
                ZedBasisDirectionKind.Right
            ],
            Enum.GetValues<ZedBasisDirectionKind>());

        Assert.Equal(
            [
                ZedDeltaSelfBasisBand.Stable,
                ZedDeltaSelfBasisBand.Reviewable,
                ZedDeltaSelfBasisBand.Withheld
            ],
            Enum.GetValues<ZedDeltaSelfBasisBand>());

        Assert.Equal(
            [
                ZedDeltaSelfBasisDisposition.Orient,
                ZedDeltaSelfBasisDisposition.Review,
                ZedDeltaSelfBasisDisposition.Withhold
            ],
            Enum.GetValues<ZedDeltaSelfBasisDisposition>());
    }

    [Fact]
    public void Stable_Self_Basis_Preserves_SoulFrame_Storage_And_ListeningFrame_Cast()
    {
        var receipt = ZedDeltaSelfBasisEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            soulFrameHandle: "soulframe://session-a",
            oeHandle: "oe://session-a",
            selfGelHandle: "selfgel://session-a",
            cOeHandle: "coe://session-a",
            cSelfGelHandle: "cselfgel://session-a",
            zedOfDeltaHandle: "zed://delta/session-a",
            engineeredCognitionHandle: "ec://session-a",
            ecIuttLispMatrixHandle: "iutt-lisp://matrix/session-a",
            receiptHandle: "receipt://zed-delta-self-basis/stable");

        Assert.Equal(ZedDeltaSelfBasisBand.Stable, receipt.BasisBand);
        Assert.Equal(ZedDeltaSelfBasisDisposition.Orient, receipt.Disposition);
        Assert.True(receipt.AnchoredByOe);
        Assert.True(receipt.StoredInSoulFrame);
        Assert.True(receipt.CastIntoListeningFrame);
        Assert.True(receipt.WiredThroughSelfGel);
        Assert.True(receipt.CandidateOnly);
        Assert.True(receipt.PersistenceAuthorityWithheld);
        Assert.True(receipt.ContinuityAdmissionWithheld);
        Assert.Equal(
            [
                ZedBasisDirectionKind.Center,
                ZedBasisDirectionKind.Ahead,
                ZedBasisDirectionKind.Behind,
                ZedBasisDirectionKind.Above,
                ZedBasisDirectionKind.Below,
                ZedBasisDirectionKind.Left,
                ZedBasisDirectionKind.Right
            ],
            receipt.CardinalDirections);
        Assert.Contains("zed-delta-self-basis-oe-anchor-preserved", receipt.ConstraintCodes);
        Assert.Contains("zed-delta-self-basis-listening-frame-cast-preserved", receipt.ConstraintCodes);
    }

    [Fact]
    public void Missing_Cast_Withholds_Self_Basis()
    {
        var receipt = ZedDeltaSelfBasisEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            soulFrameHandle: "soulframe://session-a",
            oeHandle: "oe://session-a",
            selfGelHandle: "selfgel://session-a",
            cOeHandle: null,
            cSelfGelHandle: "cselfgel://session-a",
            zedOfDeltaHandle: "zed://delta/session-a",
            engineeredCognitionHandle: "ec://session-a",
            ecIuttLispMatrixHandle: "iutt-lisp://matrix/session-a",
            receiptHandle: "receipt://zed-delta-self-basis/withheld");

        Assert.Equal(ZedDeltaSelfBasisBand.Withheld, receipt.BasisBand);
        Assert.Equal(ZedDeltaSelfBasisDisposition.Withhold, receipt.Disposition);
        Assert.False(receipt.CastIntoListeningFrame);
        Assert.Equal("zed-delta-self-basis-listening-frame-cast-incomplete", receipt.ReasonCode);
        Assert.Contains("zed-delta-self-basis-listening-frame-cast-incomplete", receipt.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_Zed_Delta_Self_Basis_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var bridgePath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md");
        var firstRunPacketPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_LIVING_AGENTICORE_PACKET.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);
        var bridgeText = File.ReadAllText(bridgePath);
        var firstRunPacketText = File.ReadAllText(firstRunPacketPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("dodecahedral lineage", lawText, StringComparison.Ordinal);
        Assert.Contains("Ahead", lawText, StringComparison.Ordinal);
        Assert.Contains("Behind", lawText, StringComparison.Ordinal);
        Assert.Contains("OE and SelfGEL remain stored in SoulFrame", lawText, StringComparison.Ordinal);
        Assert.Contains("cOE and cSelfGEL are cast into the ListeningFrame", lawText, StringComparison.Ordinal);
        Assert.Contains("AgentiCore EC IUTT-Lisp Matrix", lawText, StringComparison.Ordinal);
        Assert.Contains("ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("ZedDeltaSelfBasisReceipt", firstRunPacketText, StringComparison.Ordinal);
        Assert.Contains("bounded zed self-basis receipts", firstRunText, StringComparison.Ordinal);
        Assert.Contains("zed-delta-self-orientation-basis-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Zed-of-Delta self-orientation basis preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("`ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("OE and SelfGEL remain stored in SoulFrame", bridgeText, StringComparison.Ordinal);
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
            PostureMarkers: ["posture:legible", "zed:centered"],
            ReviewNotes: ["candidate-only-posture-surface"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 19, 00, 00, TimeSpan.Zero));
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
