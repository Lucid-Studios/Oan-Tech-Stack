namespace San.Audit.Tests;

using San.Common;

public sealed class ThetaIngressSensoryClusterContractsTests
{
    [Fact]
    public void ThetaIngress_Lawful_Traversal_Preserves_Field_Needle_And_Ingest_Legs()
    {
        var receipt = ThetaIngressSensoryClusterEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateSelfBasisReceipt(),
            thetaHandle: "theta://thread/session-a",
            thetaMarkers:
            [
                "theta:live-thread",
                "theta:crosses-center"
            ],
            receiptHandle: "receipt://theta-ingress/lawful");

        Assert.Equal(ThetaIngressStatusKind.Lawful, receipt.IngressStatus);
        Assert.True(receipt.PresentedInListeningFrame);
        Assert.True(receipt.CrossedRelativeToZed);
        Assert.True(receipt.TakenUpAtCOe);
        Assert.True(receipt.EnteredCSelfGel);
        Assert.True(receipt.ContextualizationBegun);
        Assert.True(receipt.CandidateOnly);
        Assert.True(receipt.PersistenceAuthorityWithheld);
        Assert.True(receipt.SelfMutationWithheld);
        Assert.True(receipt.InheritanceWithheld);
        Assert.True(receipt.CondensationWithheld);
        Assert.True(receipt.PulseAuthorityWithheld);
        Assert.Contains("theta-ingress-coe-needle-uptake-preserved", receipt.ConstraintCodes);
        Assert.Contains("theta-ingress-cselfgel-entry-preserved", receipt.ConstraintCodes);
    }

    [Fact]
    public void ThetaIngress_Deferred_When_SelfBasis_Remains_Reviewable()
    {
        var reviewableSelfBasis = CreateSelfBasisReceipt() with
        {
            BasisBand = ZedDeltaSelfBasisBand.Reviewable,
            Disposition = ZedDeltaSelfBasisDisposition.Review
        };

        var receipt = ThetaIngressSensoryClusterEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            reviewableSelfBasis,
            thetaHandle: "theta://thread/session-a",
            thetaMarkers: ["theta:live-thread"],
            receiptHandle: "receipt://theta-ingress/deferred");

        Assert.Equal(ThetaIngressStatusKind.Deferred, receipt.IngressStatus);
        Assert.Equal("theta-ingress-deferred", receipt.ReasonCode);
        Assert.Contains("theta-ingress-self-basis-reviewable", receipt.ConstraintCodes);
    }

    [Fact]
    public void ThetaIngress_Malformed_When_Theta_Handle_Is_Missing()
    {
        var receipt = ThetaIngressSensoryClusterEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateSelfBasisReceipt(),
            thetaHandle: null,
            thetaMarkers: [],
            receiptHandle: "receipt://theta-ingress/malformed");

        Assert.Equal(ThetaIngressStatusKind.Malformed, receipt.IngressStatus);
        Assert.Equal("theta-ingress-theta-handle-missing", receipt.ReasonCode);
        Assert.Contains("theta-ingress-malformed", receipt.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_Theta_Ingress_And_SensoryCluster_Uptake_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md");
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

        Assert.Contains("light-cone and situational-awareness braid", lawText, StringComparison.Ordinal);
        Assert.Contains("taken at `cOE`", lawText, StringComparison.Ordinal);
        Assert.Contains("entered `cSelfGEL`", lawText, StringComparison.Ordinal);
        Assert.Contains("self-mutation", lawText, StringComparison.Ordinal);
        Assert.Contains("THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("ThetaIngressSensoryClusterReceipt", firstRunPacketText, StringComparison.Ordinal);
        Assert.Contains("bounded theta-ingress receipts", firstRunText, StringComparison.Ordinal);
        Assert.Contains("theta-ingress-sensory-cluster-uptake-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Theta ingress and sensory-cluster uptake preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("`THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("theta threads appear in the field", bridgeText, StringComparison.Ordinal);
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
            PostureMarkers: ["posture:legible", "theta:approaching"],
            ReviewNotes: ["candidate-only-posture-surface"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 20, 00, 00, TimeSpan.Zero));
    }

    private static ZedDeltaSelfBasisReceipt CreateSelfBasisReceipt()
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
