using San.Common;
using SLI.Engine;
using SLI.Lisp;

namespace San.Runtime.IntegrationTests;

public sealed class CommunicativeFilamentIntegrationTests
{
    [Fact]
    public void IssueFilament_For_ClosureCandidate_DecisionBearing_Routes_To_Oe()
    {
        var retainedWholeEvaluation = EvaluateRetainedWholeChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "decision");

        var issuer = new CommunicativeFilamentIssuer();
        var evaluation = issuer.Issue(
            retainedWholeEvaluation,
            CommunicativeCarrierClassKind.DecisionBearing,
            "carrier://communicative-filament/decision",
            "receipt://communicative-filament/decision");

        Assert.Equal(PrimeRetainedWholeKind.ClosureCandidate, evaluation.RetainedWholeEvaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Equal(FilamentResolutionTargetKind.OE, evaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Preserve, evaluation.ResolutionReceipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-resolution-only", evaluation.GovernanceTrace);
    }

    [Fact]
    public void IssueFilament_For_RetainedWhole_ChosenPath_Routes_To_SelfGel()
    {
        var retainedWholeEvaluation = EvaluateRetainedWholeChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "chosen-path");

        var issuer = new CommunicativeFilamentIssuer();
        var evaluation = issuer.Issue(
            retainedWholeEvaluation,
            CommunicativeCarrierClassKind.ChosenPathBearing,
            "carrier://communicative-filament/chosen-path",
            "receipt://communicative-filament/chosen-path",
            selfEchoDetected: true);

        Assert.Equal(PrimeRetainedWholeKind.RetainedWholeUnclosed, evaluation.RetainedWholeEvaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Equal(FilamentResolutionTargetKind.SelfGEL, evaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.DeduplicateCarrier, evaluation.ResolutionReceipt.AntiEchoDisposition);
        Assert.Contains("communicative-filament-self-echo-detected", evaluation.ResolutionReceipt.ConstraintCodes);
    }

    [Fact]
    public void IssueFilament_For_Distributed_Deferred_Edge_Routes_To_CGoA()
    {
        var retainedWholeEvaluation = EvaluateRetainedWholeChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Unresolved, RtmeLineParticipationKind.Clustered, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "deferred-edge");

        var issuer = new CommunicativeFilamentIssuer();
        var evaluation = issuer.Issue(
            retainedWholeEvaluation,
            CommunicativeCarrierClassKind.DeferredEdgeBearing,
            "carrier://communicative-filament/deferred-edge",
            "receipt://communicative-filament/deferred-edge");

        Assert.Equal(PrimeRetainedWholeKind.StillDeferred, evaluation.RetainedWholeEvaluation.RetainedHistoryRecord.RetainedWholeKind);
        Assert.Equal(FilamentResolutionTargetKind.CGoA, evaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Defer, evaluation.ResolutionReceipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-deferred", evaluation.ResolutionReceipt.ReasonCode);
    }

    [Fact]
    public void IssueFilament_For_RedundantEcho_Remains_WorkingSurface_Only()
    {
        var retainedWholeEvaluation = EvaluateRetainedWholeChain(
            CreatePacket(CrypticProjectionPostureKind.Rehearsing, CmeBoundedStateKind.Bounded, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Rehearsing, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")]),
                CreateLine("line://c", "sli://surface/c", CrypticProjectionPostureKind.Latent, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://c", "sli://surface/c", "swarm-band")])
            ],
            "redundant-echo");

        var issuer = new CommunicativeFilamentIssuer();
        var evaluation = issuer.Issue(
            retainedWholeEvaluation,
            CommunicativeCarrierClassKind.RedundantEchoCandidate,
            "carrier://communicative-filament/redundant-echo",
            "receipt://communicative-filament/redundant-echo",
            selfEchoDetected: true);

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, evaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.ThinAsEcho, evaluation.ResolutionReceipt.AntiEchoDisposition);
        Assert.True(evaluation.ResolutionReceipt.CarrierTransportOnly);
    }

    [Fact]
    public void IssueFilament_Refuses_GelSubstance_Consumption_And_Keeps_Transport_Only()
    {
        var retainedWholeEvaluation = EvaluateRetainedWholeChain(
            CreatePacket(CrypticProjectionPostureKind.Braided, CmeBoundedStateKind.Anchored, true),
            [
                CreateLine("line://a", "sli://surface/a", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://a", "sli://surface/a", "listening-band")]),
                CreateLine("line://b", "sli://surface/b", CrypticProjectionPostureKind.Braided, RtmeLineParticipationKind.Swarmed, [CreateContribution("contribution://b", "sli://surface/b", "compass-band")])
            ],
            "gel-refusal");

        var issuer = new CommunicativeFilamentIssuer();
        var evaluation = issuer.Issue(
            retainedWholeEvaluation,
            CommunicativeCarrierClassKind.DecisionBearing,
            "carrier://communicative-filament/gel-refusal",
            "receipt://communicative-filament/gel-refusal",
            gelSubstanceConsumptionRequested: true);

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, evaluation.ResolutionReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Defer, evaluation.ResolutionReceipt.AntiEchoDisposition);
        Assert.True(evaluation.ResolutionReceipt.GelSubstanceConsumptionRequested);
        Assert.True(evaluation.ResolutionReceipt.GelSubstanceConsumptionRefused);
        Assert.True(evaluation.ResolutionReceipt.CarrierTransportOnly);
    }

    private static PrimeRetainedWholeEvaluation EvaluateRetainedWholeChain(
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
            $"receipt://rtme/communicative-filament/{token}");
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
            PacketHandle: "packet://duplex-field/session-g",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-g",
                MembraneHandle: "membrane://prime/session-g",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-g",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: true,
                WitnessHandles: ["witness://mother/session-g"],
                SourceNotes: ["membrane-origin-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 07, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-g",
                ProjectionHandle: "projection://cryptic/session-g",
                MembraneHandle: "membrane://prime/session-g",
                ListeningFrameHandle: "listening://frame/session-g",
                CompassEmbodimentHandle: "compass://embodiment/session-g",
                EngineeredCognitionHandle: "ec://session-g",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles: ["sli://surface/base"],
                ProjectionNotes: ["rtme-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 07, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-g",
                CmeHandle: "cme://bounded/session-g",
                EngineeredCognitionHandle: "ec://session-g",
                ProjectionHandle: "projection://cryptic/session-g",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-preserved"],
                TimestampUtc: new DateTimeOffset(2026, 04, 14, 07, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 07, 13, 00, TimeSpan.Zero));
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
