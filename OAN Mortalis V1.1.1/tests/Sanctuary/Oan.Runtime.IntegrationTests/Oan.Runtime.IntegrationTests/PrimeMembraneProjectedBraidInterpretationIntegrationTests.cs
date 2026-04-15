using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class PrimeMembraneProjectedBraidInterpretationIntegrationTests
{
    [Fact]
    public void InterpretProjectedHistory_For_Clustered_Field_Remains_ActiveProjection()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine(
                    "line://a",
                    "sli://surface/a",
                    CrypticProjectionPostureKind.Hovering,
                    RtmeLineParticipationKind.Clustered,
                    [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine(
                    "line://b",
                    "sli://surface/b",
                    CrypticProjectionPostureKind.Rehearsing,
                    RtmeLineParticipationKind.Clustered,
                    [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/braid-history/active");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/history/active");

        Assert.Equal(
            PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection,
            interpretation.InterpretationReceipt.Interpretation);
        Assert.True(interpretation.InterpretationReceipt.PreservedDistinctionVisible);
        Assert.Equal(2, interpretation.InterpretationReceipt.VisibleLineResidues.Count);
        Assert.Equal("prime-membrane-projected-braid-interpretation-only", interpretation.GovernanceTrace);
    }

    [Fact]
    public void InterpretProjectedHistory_For_Coherent_Braid_Reads_StableBraid_Without_Closure()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine(
                    "line://a",
                    "sli://surface/a",
                    CrypticProjectionPostureKind.Rehearsing,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine(
                    "line://b",
                    "sli://surface/b",
                    CrypticProjectionPostureKind.Rehearsing,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine(
                    "line://c",
                    "sli://surface/c",
                    CrypticProjectionPostureKind.Latent,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "receipt://rtme/braid-history/stable");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/history/stable");

        Assert.Equal(
            PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
            interpretation.InterpretationReceipt.Interpretation);
        Assert.Equal(PrimeClosureEligibilityKind.ReviewRequired, interpretation.InterpretationReceipt.AdvisoryClosureEligibility);
        Assert.True(interpretation.InterpretationReceipt.PrimeClosureStillWithheld);
    }

    [Fact]
    public void InterpretProjectedHistory_For_Ripening_Anchored_Braid_Reads_ReturnCandidate()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Braided,
            boundedState: CmeBoundedStateKind.Anchored,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine(
                    "line://a",
                    "sli://surface/a",
                    CrypticProjectionPostureKind.Braided,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine(
                    "line://b",
                    "sli://surface/b",
                    CrypticProjectionPostureKind.Braided,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/braid-history/return-candidate");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/history/return-candidate");

        Assert.Equal(
            PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate,
            interpretation.InterpretationReceipt.Interpretation);
        Assert.Equal(
            PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            interpretation.InterpretationReceipt.AdvisoryClosureEligibility);
        Assert.True(interpretation.InterpretationReceipt.PreservedDistinctionVisible);
        Assert.True(interpretation.InterpretationReceipt.ExplicitMembraneReceiptStillRequired);
    }

    [Fact]
    public void InterpretProjectedHistory_For_Unstable_Braid_Remains_DeferOnlyEvidence()
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            [
                CreateLine(
                    "line://a",
                    "sli://surface/a",
                    CrypticProjectionPostureKind.Unresolved,
                    RtmeLineParticipationKind.Clustered,
                    [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine(
                    "line://b",
                    "sli://surface/b",
                    CrypticProjectionPostureKind.Rehearsing,
                    RtmeLineParticipationKind.Swarmed,
                    [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "receipt://rtme/braid-history/defer");

        var interpretation = interpreter.Interpret(
            braidEvaluation,
            "receipt://prime-membrane/history/defer");

        Assert.Equal(
            PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence,
            interpretation.InterpretationReceipt.Interpretation);
        Assert.Equal("projected-history-unstable", interpretation.InterpretationReceipt.ReasonCode);
        Assert.All(
            interpretation.InterpretationReceipt.VisibleLineResidues,
            static residue => Assert.True(residue.DistinctionPreserved));
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-d",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-d",
                MembraneHandle: "membrane://prime/session-d",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-d",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-d"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 04, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-d",
                ProjectionHandle: "projection://cryptic/session-d",
                MembraneHandle: "membrane://prime/session-d",
                ListeningFrameHandle: "listening://frame/session-d",
                CompassEmbodimentHandle: "compass://embodiment/session-d",
                EngineeredCognitionHandle: "ec://session-d",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 04, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-d",
                CmeHandle: "cme://bounded/session-d",
                EngineeredCognitionHandle: "ec://session-d",
                ProjectionHandle: "projection://cryptic/session-d",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 04, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 04, 13, 00, TimeSpan.Zero));
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
