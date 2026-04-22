using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class LawfulReopeningParticipationIntegrationTests
{
    [Fact]
    public void IssueReopening_For_ChosenPath_Simple_Reopen_Preserves_Adjacent_Possibility()
    {
        var filamentEvaluation = EvaluateFilamentChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "simple",
            CommunicativeCarrierClassKind.ChosenPathBearing);

        var issuer = new LawfulReopeningParticipationIssuer();
        var evaluation = issuer.Issue(
            filamentEvaluation,
            ReopeningModeKind.SimpleReopen,
            "record://lawful-reopening/simple");

        Assert.Equal(FilamentResolutionTargetKind.SelfGEL, evaluation.FilamentEvaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(ContinuedParticipationStateKind.Reopened, evaluation.ParticipationRecord.ParticipationState);
        Assert.True(evaluation.ParticipationRecord.AdjacentPossibilityPreserved);
        Assert.Equal("lawful-reopening-participation-only", evaluation.GovernanceTrace);
    }

    [Fact]
    public void IssueReopening_For_Modified_Reopen_Keeps_Markers_Visible()
    {
        var filamentEvaluation = EvaluateFilamentChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "modified",
            CommunicativeCarrierClassKind.ChosenPathBearing,
            selfEchoDetected: true);

        var issuer = new LawfulReopeningParticipationIssuer();
        var evaluation = issuer.Issue(
            filamentEvaluation,
            ReopeningModeKind.ModifiedReopen,
            "record://lawful-reopening/modified",
            ["marker://heat-shift", "marker://focus-shift"]);

        Assert.Equal(ContinuedParticipationStateKind.ReopenedModified, evaluation.ParticipationRecord.ParticipationState);
        Assert.Equal(2, evaluation.ParticipationRecord.ReopeningMarkers.Count);
        Assert.Contains("lawful-reopening-markers-visible", evaluation.ParticipationRecord.ConstraintCodes);
    }

    [Fact]
    public void IssueReopening_For_Redoped_Path_Remains_NonConsumptive()
    {
        var filamentEvaluation = EvaluateFilamentChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "redoped",
            CommunicativeCarrierClassKind.DecisionBearing);

        var issuer = new LawfulReopeningParticipationIssuer();
        var evaluation = issuer.Issue(
            filamentEvaluation,
            ReopeningModeKind.RedopedReopen,
            "record://lawful-reopening/redoped",
            ["marker://dopant-band"]);

        Assert.Equal(ContinuedParticipationStateKind.Redoped, evaluation.ParticipationRecord.ParticipationState);
        Assert.True(evaluation.ParticipationRecord.GelSubstanceStillUnconsumed);
        Assert.True(evaluation.ParticipationRecord.FalseFreshStartRefused);
    }

    [Fact]
    public void IssueReopening_For_Distributed_Continuation_Reenters_Through_CGoA()
    {
        var filamentEvaluation = EvaluateFilamentChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Unresolved, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "distributed",
            CommunicativeCarrierClassKind.DistributedIntegrationBearing);

        var issuer = new LawfulReopeningParticipationIssuer();
        var evaluation = issuer.Issue(
            filamentEvaluation,
            ReopeningModeKind.DistributedContinuation,
            "record://lawful-reopening/distributed",
            ["marker://distributed-reentry"]);

        Assert.Equal(FilamentResolutionTargetKind.CGoA, evaluation.FilamentEvaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(ContinuedParticipationStateKind.ContinuedDistributed, evaluation.ParticipationRecord.ParticipationState);
        Assert.True(evaluation.ParticipationRecord.AdjacentPossibilityPreserved);
    }

    [Fact]
    public void IssueReopening_For_WorkingSurfaceOnly_Echo_Remains_Deferred()
    {
        var filamentEvaluation = EvaluateFilamentChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "echo",
            CommunicativeCarrierClassKind.RedundantEchoCandidate,
            selfEchoDetected: true);

        var issuer = new LawfulReopeningParticipationIssuer();
        var evaluation = issuer.Issue(
            filamentEvaluation,
            ReopeningModeKind.SimpleReopen,
            "record://lawful-reopening/echo");

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, evaluation.FilamentEvaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(ContinuedParticipationStateKind.Deferred, evaluation.ParticipationRecord.ParticipationState);
        Assert.Equal("lawful-reopening-working-surface-only", evaluation.ParticipationRecord.ReasonCode);
    }

    private static CommunicativeFilamentEvaluation EvaluateFilamentChain(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string token,
        CommunicativeCarrierClassKind carrierClass,
        bool selfEchoDetected = false,
        bool gelSubstanceConsumptionRequested = false)
    {
        var braidEvaluator = new CrypticDuplexBraidEvaluator();
        var interpreter = new PrimeMembraneProjectedBraidInterpreter();
        var historyReceiptIssuer = new PrimeMembraneProjectedHistoryReceiptIssuer();
        var retainedWholeIssuer = new PrimeRetainedWholeIssuer();
        var communicativeIssuer = new CommunicativeFilamentIssuer();

        var braidEvaluation = braidEvaluator.EvaluateBraid(
            packet,
            lines,
            $"receipt://rtme/lawful-reopening/{token}");
        var interpretation = interpreter.Interpret(
            braidEvaluation,
            $"receipt://prime-membrane/interpretation/{token}");
        var historyReceipt = historyReceiptIssuer.Issue(
            interpretation,
            $"receipt://prime-membrane/history/{token}");
        var retainedWhole = retainedWholeIssuer.Evaluate(
            historyReceipt,
            $"record://prime-retained-whole/{token}");

        return communicativeIssuer.Issue(
            retainedWhole,
            carrierClass,
            $"carrier://communicative-filament/{token}",
            $"receipt://communicative-filament/{token}",
            selfEchoDetected: selfEchoDetected,
            gelSubstanceConsumptionRequested: gelSubstanceConsumptionRequested);
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-h",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-h",
                MembraneHandle: "membrane://prime/session-h",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-h",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-h"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 08, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-h",
                ProjectionHandle: "projection://cryptic/session-h",
                MembraneHandle: "membrane://prime/session-h",
                ListeningFrameHandle: "listening://frame/session-h",
                CompassEmbodimentHandle: "compass://embodiment/session-h",
                EngineeredCognitionHandle: "ec://session-h",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 08, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-h",
                CmeHandle: "cme://bounded/session-h",
                EngineeredCognitionHandle: "ec://session-h",
                ProjectionHandle: "projection://cryptic/session-h",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 08, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 08, 13, 00, TimeSpan.Zero));
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
