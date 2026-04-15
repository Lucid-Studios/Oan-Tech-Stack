using San.Common;
using SLI.Lisp;

namespace SLI.Engine;

public sealed record PrimeMembraneProjectedBraidInterpretation(
    PrimeMembraneProjectedBraidHistoryPacket HistoryPacket,
    PrimeMembraneProjectedBraidInterpretationReceipt InterpretationReceipt,
    string GovernanceTrace);

public interface IPrimeMembraneProjectedBraidInterpreter
{
    PrimeMembraneProjectedBraidInterpretation Interpret(
        CrypticDuplexBraidEvaluation evaluation,
        string receiptHandle);
}

public sealed class PrimeMembraneProjectedBraidInterpreter : IPrimeMembraneProjectedBraidInterpreter
{
    public PrimeMembraneProjectedBraidInterpretation Interpret(
        CrypticDuplexBraidEvaluation evaluation,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var historyPacket = CreateHistoryPacket(evaluation);
        var interpretationReceipt = PrimeMembraneProjectedBraidHistoryEvaluator.Evaluate(historyPacket, receiptHandle);

        return new PrimeMembraneProjectedBraidInterpretation(
            HistoryPacket: historyPacket,
            InterpretationReceipt: interpretationReceipt,
            GovernanceTrace: "prime-membrane-projected-braid-interpretation-only");
    }

    private static PrimeMembraneProjectedBraidHistoryPacket CreateHistoryPacket(
        CrypticDuplexBraidEvaluation evaluation)
    {
        return new PrimeMembraneProjectedBraidHistoryPacket(
            HistoryHandle: evaluation.BraidSnapshot.SnapshotHandle,
            SourceDuplexPacketHandle: evaluation.BraidSnapshot.SourcePacketHandle,
            EmittedDuplexPacketHandle: evaluation.BraidSnapshot.EmittedPacketHandle,
            MembraneHandle: evaluation.EmittedPacket.MembraneSource.MembraneHandle,
            ProjectionHandle: evaluation.BraidSnapshot.ProjectionHandle,
            BraidState: MapBraidState(evaluation.BraidSnapshot.BraidState),
            AdvisoryClosureEligibility: evaluation.AdvisoryClosureEligibility,
            PrimeClosureIssued: evaluation.PrimeClosureIssued,
            LineResidues: evaluation.BraidSnapshot.LineResidues
                .Select(MapResidue)
                .ToArray(),
            OutcomeCode: evaluation.OutcomeCode,
            LawfulBasis: evaluation.BraidSnapshot.LawfulBasis,
            TimestampUtc: evaluation.BraidSnapshot.TimestampUtc);
    }

    private static PrimeMembraneProjectedBraidStateKind MapBraidState(
        RtmeBraidStateKind braidState)
    {
        return braidState switch
        {
            RtmeBraidStateKind.Clustered => PrimeMembraneProjectedBraidStateKind.Clustered,
            RtmeBraidStateKind.Swarmed => PrimeMembraneProjectedBraidStateKind.Swarmed,
            RtmeBraidStateKind.CoherentBraid => PrimeMembraneProjectedBraidStateKind.CoherentBraid,
            RtmeBraidStateKind.UnstableBraid => PrimeMembraneProjectedBraidStateKind.UnstableBraid,
            _ => PrimeMembraneProjectedBraidStateKind.Dispersed
        };
    }

    private static PrimeMembraneProjectedLineResidue MapResidue(
        RtmeProjectedLineResidue residue)
    {
        return new PrimeMembraneProjectedLineResidue(
            LineHandle: residue.LineHandle,
            SourceSurfaceHandle: residue.SourceSurfaceHandle,
            ResidualPosture: residue.ResidualPosture,
            ParticipationKind: residue.ParticipationKind == RtmeLineParticipationKind.Swarmed
                ? PrimeMembraneProjectedParticipationKind.Swarmed
                : PrimeMembraneProjectedParticipationKind.Clustered,
            AcceptedContributionHandles: residue.AcceptedContributionHandles,
            DistinctionPreserved: residue.DistinctionPreserved,
            ResidueNotes: residue.ResidueNotes);
    }
}
