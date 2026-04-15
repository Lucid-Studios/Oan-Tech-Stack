using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class SliRtmeDuplexBraidIntegrationTests
{
    [Fact]
    public void EvaluateBraid_With_One_Line_Remains_Dispersed()
    {
        var evaluator = new CrypticDuplexBraidEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Candidate,
            standingFormed: true);

        var evaluation = evaluator.EvaluateBraid(
            packet,
            [
                CreateLine(
                    "line://a",
                    "sli://surface/a",
                    CrypticProjectionPostureKind.Hovering,
                    RtmeLineParticipationKind.Clustered,
                    [CreateContribution("contribution://a", "sli://surface/a", "listening-band")])
            ],
            "receipt://rtme/braid/dispersed");

        Assert.Equal(RtmeBraidStateKind.Dispersed, evaluation.BraidSnapshot.BraidState);
        Assert.Equal(RtmeNonClosureStatusKind.PrimeClosureWithheld, evaluation.BraidSnapshot.NonClosureStatus);
        Assert.Single(evaluation.BraidSnapshot.LineResidues);
        Assert.True(evaluation.BraidSnapshot.LineResidues[0].DistinctionPreserved);
        Assert.False(evaluation.PrimeClosureIssued);
    }

    [Fact]
    public void EvaluateBraid_With_Clustered_Lines_Preserves_PerLine_Residue()
    {
        var evaluator = new CrypticDuplexBraidEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var evaluation = evaluator.EvaluateBraid(
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
            "receipt://rtme/braid/clustered");

        Assert.Equal(RtmeBraidStateKind.Clustered, evaluation.BraidSnapshot.BraidState);
        Assert.Equal(2, evaluation.BraidSnapshot.LineResidues.Count);
        Assert.All(evaluation.BraidSnapshot.LineResidues, static residue => Assert.True(residue.DistinctionPreserved));
        Assert.All(evaluation.BraidSnapshot.LineResidues, static residue => Assert.Equal(RtmeLineParticipationKind.Clustered, residue.ParticipationKind));
        Assert.Equal(PrimeClosureEligibilityKind.CandidateOnly, evaluation.AdvisoryClosureEligibility);
        Assert.False(evaluation.PrimeClosureIssued);
    }

    [Fact]
    public void EvaluateBraid_With_Swarmed_Lines_Forms_CoherentBraid_Without_Closure()
    {
        var evaluator = new CrypticDuplexBraidEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var evaluation = evaluator.EvaluateBraid(
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
            "receipt://rtme/braid/coherent");

        Assert.Equal(RtmeBraidStateKind.CoherentBraid, evaluation.BraidSnapshot.BraidState);
        Assert.Equal(CrypticProjectionPostureKind.Braided, evaluation.EmittedPacket.ProjectionState.ProjectionPosture);
        Assert.Equal(RtmeNonClosureStatusKind.PrimeClosureWithheld, evaluation.BraidSnapshot.NonClosureStatus);
        Assert.False(evaluation.PrimeClosureIssued);
        Assert.Equal("rtme-duplex-braid-projected-field-only", evaluation.GovernanceTrace);
    }

    [Fact]
    public void EvaluateBraid_With_Unresolved_Line_Remains_Unstable()
    {
        var evaluator = new CrypticDuplexBraidEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var evaluation = evaluator.EvaluateBraid(
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
            "receipt://rtme/braid/unstable");

        Assert.Equal(RtmeBraidStateKind.UnstableBraid, evaluation.BraidSnapshot.BraidState);
        Assert.False(evaluation.PrimeClosureIssued);
        Assert.All(evaluation.BraidSnapshot.LineResidues, static residue => Assert.True(residue.DistinctionPreserved));
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-c",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-c",
                MembraneHandle: "membrane://prime/session-c",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-c",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-c"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 00, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-c",
                ProjectionHandle: "projection://cryptic/session-c",
                MembraneHandle: "membrane://prime/session-c",
                ListeningFrameHandle: "listening://frame/session-c",
                CompassEmbodimentHandle: "compass://embodiment/session-c",
                EngineeredCognitionHandle: "ec://session-c",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 00, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-c",
                CmeHandle: "cme://bounded/session-c",
                EngineeredCognitionHandle: "ec://session-c",
                ProjectionHandle: "projection://cryptic/session-c",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 00, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 00, 13, 00, TimeSpan.Zero));
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
