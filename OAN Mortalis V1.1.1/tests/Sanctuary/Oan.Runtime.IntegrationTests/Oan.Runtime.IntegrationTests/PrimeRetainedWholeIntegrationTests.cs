using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class PrimeRetainedWholeIntegrationTests
{
    [Fact]
    public void EvaluateRetainedWhole_For_SeenOnly_History_Remains_NotRetained()
    {
        var evaluation = EvaluateChain(
            CreatePacket(CrypticProjectionPostureKind.Hovering, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Hovering, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "seen");

        Assert.Equal(PrimeRetainedWholeKind.NotRetained, evaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Empty(evaluation.RetainedHistoryRecord.RetainedResidues);
        Assert.Equal("prime-retained-whole-evaluation-only", evaluation.GovernanceTrace);
    }

    [Fact]
    public void EvaluateRetainedWhole_For_Stable_Braid_Becomes_RetainedWholeUnclosed()
    {
        var evaluation = EvaluateChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "stable");

        Assert.Equal(PrimeRetainedWholeKind.RetainedWholeUnclosed, evaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Equal(3, evaluation.RetainedHistoryRecord.RetainedResidues.Count);
        Assert.True(evaluation.RetainedHistoryRecord.PrimeClosureStillWithheld);
    }

    [Fact]
    public void EvaluateRetainedWhole_For_ReturnBearing_History_Becomes_ClosureCandidate()
    {
        var evaluation = EvaluateChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "closure-candidate");

        Assert.Equal(PrimeRetainedWholeKind.ClosureCandidate, evaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.True(evaluation.RetainedHistoryRecord.ExplicitClosureActStillRequired);
        Assert.True(evaluation.RetainedHistoryRecord.PrimeClosureStillWithheld);
    }

    [Fact]
    public void EvaluateRetainedWhole_For_Unstable_History_Remains_StillDeferred()
    {
        var evaluation = EvaluateChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Unresolved, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "deferred");

        Assert.Equal(PrimeRetainedWholeKind.StillDeferred, evaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Empty(evaluation.RetainedHistoryRecord.RetainedResidues);
        Assert.Equal(2, evaluation.RetainedHistoryRecord.UnresolvedResidues.Count);
    }

    private static PrimeRetainedWholeEvaluation EvaluateChain(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string token)
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var receiptIssuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var retainedWholeIssuer = new PrimeRetainedWholeIssuer();

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            lines,
            $"receipt://rtme/retained-whole/{token}");
        var interpretation = interpreter.Interpret(
            braidEvaluation,
            $"receipt://prime-membrane/interpretation/{token}");
        var historyReceipt = receiptIssuer.Issue(
            interpretation,
            $"receipt://prime-membrane/history/{token}");

        return retainedWholeIssuer.Evaluate(
            historyReceipt,
            $"record://prime-retained-whole/{token}");
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-f",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-f",
                MembraneHandle: "membrane://prime/session-f",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-f",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-f"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 06, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-f",
                ProjectionHandle: "projection://cryptic/session-f",
                MembraneHandle: "membrane://prime/session-f",
                ListeningFrameHandle: "listening://frame/session-f",
                CompassEmbodimentHandle: "compass://embodiment/session-f",
                EngineeredCognitionHandle: "ec://session-f",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 06, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-f",
                CmeHandle: "cme://bounded/session-f",
                EngineeredCognitionHandle: "ec://session-f",
                ProjectionHandle: "projection://cryptic/session-f",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 06, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 06, 13, 00, TimeSpan.Zero));
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
