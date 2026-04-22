using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class SliRtmeDuplexPostureIntegrationTests
{
    [Fact]
    public void Evaluate_Hovering_Packet_With_One_Contribution_Deepens_To_Rehearsing()
    {
        var evaluator = new CrypticDuplexPostureEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Candidate,
            standingFormed: true);

        var evaluation = evaluator.Evaluate(
            packet,
            [
                CreateContribution("contribution://a", "sli://surface/a", "listening-band")
            ],
            "receipt://rtme/hovering");

        Assert.Equal(CrypticProjectionPostureKind.Rehearsing, evaluation.EmittedPacket.ProjectionState.ProjectionPosture);
        Assert.Equal(RtmeProjectionTransitionKind.Deepen, evaluation.Receipt.TransitionKind);
        Assert.Equal(PrimeClosureEligibilityKind.CandidateOnly, evaluation.AdvisoryClosureEligibility);
        Assert.False(evaluation.PrimeClosureIssued);
        Assert.Equal("rtme-duplex-posture-projected-field-only", evaluation.GovernanceTrace);
        Assert.Equal(CmeBoundedStateKind.Candidate, evaluation.EmittedPacket.BoundedState.BoundedState);
        Assert.True(evaluation.EmittedPacket.BoundedState.StandingFormed);
    }

    [Fact]
    public void Evaluate_Rehearsing_Packet_With_MultiSurface_Contributions_Braids_Without_Mutating_BoundedState()
    {
        var evaluator = new CrypticDuplexPostureEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Rehearsing,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var evaluation = evaluator.Evaluate(
            packet,
            [
                CreateContribution("contribution://a", "sli://surface/a", "listening-band"),
                CreateContribution("contribution://b", "sli://surface/b", "compass-band")
            ],
            "receipt://rtme/braided");

        Assert.Equal(CrypticProjectionPostureKind.Braided, evaluation.EmittedPacket.ProjectionState.ProjectionPosture);
        Assert.Equal(RtmeProjectionTransitionKind.Braid, evaluation.Receipt.TransitionKind);
        Assert.Equal(PrimeClosureEligibilityKind.ReviewRequired, evaluation.AdvisoryClosureEligibility);
        Assert.False(evaluation.PrimeClosureIssued);
        Assert.Equal(CmeBoundedStateKind.Bounded, evaluation.EmittedPacket.BoundedState.BoundedState);
        Assert.Contains("sli://surface/a", evaluation.EmittedPacket.ProjectionState.ParticipatingSurfaceHandles, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("sli://surface/b", evaluation.EmittedPacket.ProjectionState.ParticipatingSurfaceHandles, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_Braided_Anchored_Packet_Ripens_And_Stays_Advisory_Only()
    {
        var evaluator = new CrypticDuplexPostureEvaluator();
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Braided,
            boundedState: CmeBoundedStateKind.Anchored,
            standingFormed: true);

        var evaluation = evaluator.Evaluate(
            packet,
            [
                CreateContribution("contribution://c", "sli://surface/c", "ripening-band")
            ],
            "receipt://rtme/ripening");

        Assert.Equal(CrypticProjectionPostureKind.Ripening, evaluation.EmittedPacket.ProjectionState.ProjectionPosture);
        Assert.Equal(RtmeProjectionTransitionKind.Stabilize, evaluation.Receipt.TransitionKind);
        Assert.Equal(PrimeClosureEligibilityKind.EligibleForMembraneReceipt, evaluation.AdvisoryClosureEligibility);
        Assert.False(evaluation.PrimeClosureIssued);
        Assert.True(evaluation.Receipt.LawfulBasis.Contains("may not issue Prime closure", StringComparison.Ordinal));
        Assert.Equal(CmeBoundedStateKind.Anchored, evaluation.EmittedPacket.BoundedState.BoundedState);
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-b",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-b",
                MembraneHandle: "membrane://prime/session-b",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-b",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-b"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 40, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-b",
                ProjectionHandle: "projection://cryptic/session-b",
                MembraneHandle: "membrane://prime/session-b",
                ListeningFrameHandle: "listening://frame/session-b",
                CompassEmbodimentHandle: "compass://embodiment/session-b",
                EngineeredCognitionHandle: "ec://session-b",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 41, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-b",
                CmeHandle: "cme://bounded/session-b",
                EngineeredCognitionHandle: "ec://session-b",
                ProjectionHandle: "projection://cryptic/session-b",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 42, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 43, 00, TimeSpan.Zero));
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
