using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class PrimeMembraneProjectedHistoryReceiptIntegrationTests
{
    [Fact]
    public void IssueReceipt_For_Clustered_Field_Remains_SeenOnly()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var issuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Hovering, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/history-receipt/seen");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/interpretation/seen");
        var receipt = issuer.Issue(
            interpretation,
            "receipt://prime-membrane/history/seen");

        Assert.Equal(PrimeMembraneReceiptKind.SeenOnly, receipt.HistoryReceipt.ReceiptKind);
        Assert.Equal(2, receipt.HistoryReceipt.VisibleLineResidues.Count);
        Assert.Equal(2, receipt.HistoryReceipt.DeferredLineResidues.Count);
        Assert.Equal("prime-membrane-projected-history-receipt-only", receipt.GovernanceTrace);
    }

    [Fact]
    public void IssueReceipt_For_Coherent_Braid_Receipts_History_Without_Closure()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var issuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "receipt://rtme/history-receipt/stable");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/interpretation/stable");
        var receipt = issuer.Issue(
            interpretation,
            "receipt://prime-membrane/history/stable");

        Assert.Equal(PrimeMembraneReceiptKind.ReceiptedHistory, receipt.HistoryReceipt.ReceiptKind);
        Assert.Empty(receipt.HistoryReceipt.DeferredLineResidues);
        Assert.True(receipt.HistoryReceipt.PrimeClosureStillWithheld);
    }

    [Fact]
    public void IssueReceipt_For_Ripening_Anchored_Braid_Is_ReturnBearing_Unclosed()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var issuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Braided,
            boundedState: CmeBoundedStateKind.Anchored,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/history-receipt/return-bearing");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/interpretation/return-bearing");
        var receipt = issuer.Issue(
            interpretation,
            "receipt://prime-membrane/history/return-bearing");

        Assert.Equal(PrimeMembraneReceiptKind.ReturnBearingUnclosed, receipt.HistoryReceipt.ReceiptKind);
        Assert.Equal(PrimeClosureEligibilityKind.EligibleForMembraneReceipt, receipt.HistoryReceipt.AdvisoryClosureEligibility);
        Assert.True(receipt.HistoryReceipt.RetainedWholenessStillWithheld);
        Assert.True(receipt.HistoryReceipt.PrimeClosureStillWithheld);
    }

    [Fact]
    public void IssueReceipt_For_Unstable_Braid_Remains_Deferred()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var issuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Unresolved, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/history-receipt/deferred");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/interpretation/deferred");
        var receipt = issuer.Issue(
            interpretation,
            "receipt://prime-membrane/history/deferred");

        Assert.Equal(PrimeMembraneReceiptKind.Deferred, receipt.HistoryReceipt.ReceiptKind);
        Assert.Equal(receipt.HistoryReceipt.VisibleLineResidues.Count, receipt.HistoryReceipt.DeferredLineResidues.Count);
        Assert.Equal("prime-membrane-history-deferred", receipt.HistoryReceipt.ReasonCode);
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-e",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-e",
                MembraneHandle: "membrane://prime/session-e",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-e",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-e"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 05, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-e",
                ProjectionHandle: "projection://cryptic/session-e",
                MembraneHandle: "membrane://prime/session-e",
                ListeningFrameHandle: "listening://frame/session-e",
                CompassEmbodimentHandle: "compass://embodiment/session-e",
                EngineeredCognitionHandle: "ec://session-e",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 05, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-e",
                CmeHandle: "cme://bounded/session-e",
                EngineeredCognitionHandle: "ec://session-e",
                ProjectionHandle: "projection://cryptic/session-e",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 05, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 05, 13, 00, TimeSpan.Zero));
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
