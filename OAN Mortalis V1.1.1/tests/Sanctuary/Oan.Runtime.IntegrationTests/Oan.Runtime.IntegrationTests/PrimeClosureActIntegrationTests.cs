using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class PrimeClosureActIntegrationTests
{
    [Fact]
    public void EvaluateClosure_For_Clean_Candidate_Executes_Remaining_Product_Attestation()
    {
        var reopeningEvaluation = EvaluateReopeningChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "executed",
            CommunicativeCarrierClassKind.DecisionBearing,
            ReopeningModeKind.RedopedReopen);

        var issuer = new PrimeClosureActIssuer();
        var evaluation = issuer.Evaluate(
            reopeningEvaluation,
            "record://prime-closure/executed",
            "product://prime-closure/executed");

        Assert.Equal(PrimeClosureActKind.ClosureExecuted, evaluation.ClosureActRecord.ClosureActKind);
        Assert.Equal("remaining-product-only", evaluation.ClosureActRecord.ClosureScope);
        Assert.Equal("prime-closure-act-only", evaluation.GovernanceTrace);
        Assert.Equal(2, evaluation.ClosureActRecord.AttestedRemainingProductResidues.Count);
    }

    [Fact]
    public void EvaluateClosure_For_NonCandidate_Remains_Withheld()
    {
        var reopeningEvaluation = EvaluateReopeningChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "withheld",
            CommunicativeCarrierClassKind.ChosenPathBearing,
            ReopeningModeKind.SimpleReopen);

        var issuer = new PrimeClosureActIssuer();
        var evaluation = issuer.Evaluate(
            reopeningEvaluation,
            "record://prime-closure/withheld",
            "product://prime-closure/withheld");

        Assert.Equal(PrimeClosureActKind.ClosureWithheld, evaluation.ClosureActRecord.ClosureActKind);
        Assert.Equal("prime-closure-withheld-not-closure-candidate", evaluation.ClosureActRecord.ReasonCode);
    }

    [Fact]
    public void EvaluateClosure_For_Deferred_Reopening_Declines()
    {
        var reopeningEvaluation = EvaluateReopeningChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "declined",
            CommunicativeCarrierClassKind.DecisionBearing,
            ReopeningModeKind.DistributedContinuation);

        var issuer = new PrimeClosureActIssuer();
        var evaluation = issuer.Evaluate(
            reopeningEvaluation,
            "record://prime-closure/declined",
            "product://prime-closure/declined");

        Assert.Equal(PrimeClosureActKind.ClosureDeclined, evaluation.ClosureActRecord.ClosureActKind);
        Assert.Equal("prime-closure-declined-reopening-deferred", evaluation.ClosureActRecord.ReasonCode);
        Assert.True(evaluation.ClosureActRecord.DeferredResiduesStillVisible);
    }

    private static LawfulReopeningParticipationEvaluation EvaluateReopeningChain(
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

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            lines,
            $"receipt://rtme/prime-closure/{token}");
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

        return reopeningIssuer.Issue(
            communicative,
            reopeningMode,
            $"record://lawful-reopening/{token}",
            ["marker://reopening"]);
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-i",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-i",
                MembraneHandle: "membrane://prime/session-i",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-i",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-i"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-i",
                ProjectionHandle: "projection://cryptic/session-i",
                MembraneHandle: "membrane://prime/session-i",
                ListeningFrameHandle: "listening://frame/session-i",
                CompassEmbodimentHandle: "compass://embodiment/session-i",
                EngineeredCognitionHandle: "ec://session-i",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-i",
                CmeHandle: "cme://bounded/session-i",
                EngineeredCognitionHandle: "ec://session-i",
                ProjectionHandle: "projection://cryptic/session-i",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 13, 00, TimeSpan.Zero));
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
