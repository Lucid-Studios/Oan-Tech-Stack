namespace San.Audit.Tests;

using San.Common;
using San.Common;

public sealed class PostIngressDiscernmentContractsTests
{
    [Fact]
    public void Stabilized_Discernment_Requires_StableOne_And_No_Investigatory_Carry()
    {
        var receipt = PostIngressDiscernmentEvaluator.Evaluate(
            CreateThetaIngressReceipt(),
            stableOneAchieved: true,
            stableOneHandle: "stable-one://thread/session-a",
            discernmentSignals: [PostIngressDiscernmentSignalKind.None],
            questionHandles: [],
            enrichmentHandles: [],
            carriedIncompleteHandles: [],
            receiptHandle: "receipt://post-ingress-discernment/stabilized");

        Assert.Equal(PostIngressDiscernmentStateKind.Stabilized, receipt.DiscernmentState);
        Assert.True(receipt.StableOneAchieved);
        Assert.Equal("stable-one://thread/session-a", receipt.StableOneHandle);
        Assert.Empty(receipt.QuestionHandles);
        Assert.Empty(receipt.EnrichmentHandles);
        Assert.Empty(receipt.CarriedIncompleteHandles);
        Assert.True(receipt.SemanticRiseWithheld);
        Assert.True(receipt.PersistenceAuthorityWithheld);
        Assert.True(receipt.InheritanceWithheld);
        Assert.True(receipt.SelfMutationWithheld);
        Assert.True(receipt.PulseAuthorityWithheld);
        Assert.Equal("ec://session-a", receipt.EngineeredCognitionHandle);
    }

    [Fact]
    public void Investigatory_Discernment_Generates_Questions_When_StableOne_Fails()
    {
        var receipt = PostIngressDiscernmentEvaluator.Evaluate(
            CreateThetaIngressReceipt(),
            stableOneAchieved: false,
            stableOneHandle: null,
            discernmentSignals:
            [
                PostIngressDiscernmentSignalKind.Ambiguity,
                PostIngressDiscernmentSignalKind.UnresolvedRelation
            ],
            questionHandles:
            [
                "question://thread/session-a/resolve-ambiguity",
                "question://thread/session-a/resolve-relation"
            ],
            enrichmentHandles:
            [
                "enrichment://thread/session-a/context-fill"
            ],
            carriedIncompleteHandles: [],
            receiptHandle: "receipt://post-ingress-discernment/investigatory");

        Assert.Equal(PostIngressDiscernmentStateKind.Investigatory, receipt.DiscernmentState);
        Assert.False(receipt.StableOneAchieved);
        Assert.Equal(2, receipt.QuestionHandles.Count);
        Assert.Contains("post-ingress-discernment-investigatory-signal-present", receipt.ConstraintCodes);
        Assert.Contains("post-ingress-discernment-question-handles-visible", receipt.ConstraintCodes);
        Assert.Single(receipt.EnrichmentHandles);
        Assert.Empty(receipt.CarriedIncompleteHandles);
    }

    [Fact]
    public void CarriedIncomplete_Discernment_Exposes_Incompleteness_Without_Questions()
    {
        var receipt = PostIngressDiscernmentEvaluator.Evaluate(
            CreateThetaIngressReceipt(),
            stableOneAchieved: false,
            stableOneHandle: null,
            discernmentSignals: [PostIngressDiscernmentSignalKind.GroundingInsufficient],
            questionHandles: [],
            enrichmentHandles:
            [
                "enrichment://thread/session-a/revisit-context"
            ],
            carriedIncompleteHandles:
            [
                "incomplete://thread/session-a/anchor-gap"
            ],
            receiptHandle: "receipt://post-ingress-discernment/carried-incomplete");

        Assert.Equal(PostIngressDiscernmentStateKind.CarriedIncomplete, receipt.DiscernmentState);
        Assert.False(receipt.StableOneAchieved);
        Assert.Empty(receipt.QuestionHandles);
        Assert.Single(receipt.EnrichmentHandles);
        Assert.Single(receipt.CarriedIncompleteHandles);
        Assert.Equal("post-ingress-discernment-carried-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Blocked_Discernment_Withholds_Question_Creation_When_Ingress_Is_Not_Lawful()
    {
        var thetaIngress = CreateThetaIngressReceipt() with
        {
            IngressStatus = ThetaIngressStatusKind.Blocked,
            ContextualizationBegun = false
        };

        var receipt = PostIngressDiscernmentEvaluator.Evaluate(
            thetaIngress,
            stableOneAchieved: false,
            stableOneHandle: null,
            discernmentSignals: [PostIngressDiscernmentSignalKind.StructuralConflict],
            questionHandles:
            [
                "question://thread/session-a/should-not-survive"
            ],
            enrichmentHandles:
            [
                "enrichment://thread/session-a/also-withheld"
            ],
            carriedIncompleteHandles:
            [
                "incomplete://thread/session-a/not-admitted"
            ],
            receiptHandle: "receipt://post-ingress-discernment/blocked");

        Assert.Equal(PostIngressDiscernmentStateKind.Blocked, receipt.DiscernmentState);
        Assert.False(receipt.StableOneAchieved);
        Assert.Empty(receipt.QuestionHandles);
        Assert.Empty(receipt.EnrichmentHandles);
        Assert.Empty(receipt.CarriedIncompleteHandles);
        Assert.Equal("post-ingress-discernment-theta-ingress-not-lawful", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_PostIngress_Discernment_And_StableOne_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var thetaLawPath = Path.Combine(lineRoot, "docs", "THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md");
        var firstRunPacketPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_LIVING_AGENTICORE_PACKET.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);
        var thetaLawText = File.ReadAllText(thetaLawPath);
        var firstRunPacketText = File.ReadAllText(firstRunPacketPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("awareness precedes knowing", lawText, StringComparison.Ordinal);
        Assert.Contains("stable `1`", lawText, StringComparison.Ordinal);
        Assert.Contains("questions are generated", lawText, StringComparison.Ordinal);
        Assert.Contains("POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md", minimalBuildText, StringComparison.Ordinal);
        Assert.Contains("ThetaIngressSensoryClusterReceipt", thetaLawText, StringComparison.Ordinal);
        Assert.Contains("PostIngressDiscernmentReceipt", firstRunPacketText, StringComparison.Ordinal);
        Assert.Contains("bounded post-ingress discernment receipts", firstRunText, StringComparison.Ordinal);
        Assert.Contains("post-ingress-discernment-and-stable-one-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Post-ingress discernment and stable-one sufficiency preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("`POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static ThetaIngressSensoryClusterReceipt CreateThetaIngressReceipt()
    {
        var listeningFrame = new ListeningFrameProjectionPacket(
            PacketHandle: "packet://listening-frame/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            ChamberHandle: "soulframe://session-a",
            SourceSurfaceHandle: "agenticore://session-a",
            VisibilityPosture: ListeningFrameVisibilityPosture.OperatorGuarded,
            IntegrityState: ListeningFrameIntegrityState.Usable,
            ReviewPosture: ListeningFrameReviewPosture.CandidateOnly,
            UsableForCompassProjection: true,
            PostureMarkers: ["theta:approaching"],
            ReviewNotes: ["candidate-only-posture-surface"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 21, 00, 00, TimeSpan.Zero));

        var selfBasis = ZedDeltaSelfBasisEvaluator.Evaluate(
            listeningFrame,
            soulFrameHandle: "soulframe://session-a",
            oeHandle: "oe://session-a",
            selfGelHandle: "selfgel://session-a",
            cOeHandle: "coe://session-a",
            cSelfGelHandle: "cselfgel://session-a",
            zedOfDeltaHandle: "zed://delta/session-a",
            engineeredCognitionHandle: "ec://session-a",
            ecIuttLispMatrixHandle: "iutt-lisp://matrix/session-a",
            receiptHandle: "receipt://zed-delta-self-basis/session-a");

        return ThetaIngressSensoryClusterEvaluator.Evaluate(
            listeningFrame,
            selfBasis,
            thetaHandle: "theta://thread/session-a",
            thetaMarkers:
            [
                "theta:live-thread",
                "theta:crosses-center"
            ],
            receiptHandle: "receipt://theta-ingress/session-a");
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
