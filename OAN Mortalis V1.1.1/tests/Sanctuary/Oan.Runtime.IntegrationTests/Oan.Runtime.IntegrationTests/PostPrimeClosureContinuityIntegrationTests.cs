using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class PostPrimeClosureContinuityIntegrationTests
{
    [Fact]
    public void EvaluatePostClosureContinuity_For_Executed_Closure_Attests_Bounded_Reentry()
    {
        var closureEvaluation = EvaluateClosureChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "executed",
            CommunicativeCarrierClassKind.DecisionBearing,
            ReopeningModeKind.RedopedReopen);

        var issuer = new PostPrimeClosureContinuityIssuer();
        var evaluation = issuer.Evaluate(
            closureEvaluation,
            "record://post-prime-continuity/executed");

        Assert.Equal(PostPrimeClosureContinuityKind.ReentryPermitted, evaluation.ContinuityRecord.ContinuityKind);
        Assert.Equal("closed-product-bounded-reentry", evaluation.ContinuityRecord.ContinuityScope);
        Assert.Equal("post-prime-closure-continuity-only", evaluation.GovernanceTrace);
        Assert.True(evaluation.ContinuityRecord.BearingFieldNonVoidAttested);
        Assert.True(evaluation.ContinuityRecord.ActiveCarriersStillPresent);
    }

    [Fact]
    public void EvaluatePostClosureContinuity_For_Deferred_Boundary_Keeps_Deferred_Edge_Open()
    {
        var closureEvaluation = EvaluateClosureChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "declined",
            CommunicativeCarrierClassKind.DecisionBearing,
            ReopeningModeKind.DistributedContinuation);

        var issuer = new PostPrimeClosureContinuityIssuer();
        var evaluation = issuer.Evaluate(
            closureEvaluation,
            "record://post-prime-continuity/declined");

        Assert.Equal(PostPrimeClosureContinuityKind.DeferredEdgeOpen, evaluation.ContinuityRecord.ContinuityKind);
        Assert.Equal("post-prime-continuity-deferred-edge-open", evaluation.ContinuityRecord.ReasonCode);
        Assert.True(evaluation.ContinuityRecord.DeferredEdgesStillOpen);
    }

    [Fact]
    public void EvaluatePostClosureContinuity_For_Withheld_Closure_Withholds_Continuity()
    {
        var closureEvaluation = EvaluateClosureChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "withheld",
            CommunicativeCarrierClassKind.ChosenPathBearing,
            ReopeningModeKind.SimpleReopen);

        var issuer = new PostPrimeClosureContinuityIssuer();
        var evaluation = issuer.Evaluate(
            closureEvaluation,
            "record://post-prime-continuity/withheld");

        Assert.Equal(PostPrimeClosureContinuityKind.ContinuityWithheld, evaluation.ContinuityRecord.ContinuityKind);
        Assert.Equal("post-prime-continuity-withheld-closure-not-executed", evaluation.ContinuityRecord.ReasonCode);
    }

    private static PrimeClosureActEvaluation EvaluateClosureChain(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string token,
        CommunicativeCarrierClassKind carrierClass,
        ReopeningModeKind reopeningMode,
        bool selfEchoDetected = false)
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var historyReceiptIssuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var retainedWholeIssuer = new PrimeRetainedWholeIssuer();
        var communicativeIssuer = new CommunicativeFilamentIssuer();
        var reopeningIssuer = new LawfulReopeningParticipationIssuer();
        var closureIssuer = new PrimeClosureActIssuer();

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            lines,
            $"receipt://rtme/post-prime-continuity/{token}");
        var interpretation = interpreter.Interpret(
            braidEvaluation,
            $"receipt://prime-membrane/interpretation/{token}");
        var historyReceipt = historyReceiptIssuer.Issue(
            interpretation,
            $"receipt://prime-membrane/history/{token}");
        var retainedWhole = retainedWholeIssuer.Evaluate(
            historyReceipt,
            $"record://prime-retained-whole/{token}");
        var communicative = communicativeIssuer.Issue(
            retainedWhole,
            carrierClass,
            $"carrier://communicative-filament/{token}",
            $"receipt://communicative-filament/{token}",
            selfEchoDetected: selfEchoDetected);
        var reopening = reopeningIssuer.Issue(
            communicative,
            reopeningMode,
            $"record://lawful-reopening/{token}",
            ["marker://reopening"]);

        return closureIssuer.Evaluate(
            reopening,
            $"record://prime-closure/{token}",
            $"product://prime-closure/{token}");
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-j",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-j",
                MembraneHandle: "membrane://prime/session-j",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-j",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-j"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 20, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-j",
                ProjectionHandle: "projection://cryptic/session-j",
                MembraneHandle: "membrane://prime/session-j",
                ListeningFrameHandle: "listening://frame/session-j",
                CompassEmbodimentHandle: "compass://embodiment/session-j",
                EngineeredCognitionHandle: "ec://session-j",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 21, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-j",
                CmeHandle: "cme://bounded/session-j",
                EngineeredCognitionHandle: "ec://session-j",
                ProjectionHandle: "projection://cryptic/session-j",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 22, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 23, 00, TimeSpan.Zero));
    }

    private static RtmeProjectedLineInput CreateLine(
        string lineHandle,
        string surfaceHandle,
        CrypticProjectionPostureKind posture,
        RtmeLineParticipationKind participationKind,
        IReadOnlyList<RtmeProjectionContribution> contributions)
    {
        return new RtmeProjectedLineInput(
            LineHandle: lineHandle,
            SourceSurfaceHandle: surfaceHandle,
            LinePosture: posture,
            ParticipationKind: participationKind,
            Contributions: contributions,
            Notes: ["per-line-origin-preserved"]);
    }

    private static RtmeProjectionContribution CreateContribution(
        string handle,
        string surfaceHandle,
        string kind)
    {
        return new RtmeProjectionContribution(
            ContributionHandle: handle,
            SourceSurfaceHandle: surfaceHandle,
            ContributionKind: kind,
            Notes: ["projected-field-contribution"]);
    }
}
