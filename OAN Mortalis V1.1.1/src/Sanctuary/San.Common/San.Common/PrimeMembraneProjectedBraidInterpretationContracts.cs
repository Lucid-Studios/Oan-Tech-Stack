namespace San.Common;

public enum PrimeMembraneProjectedParticipationKind
{
    Clustered = 0,
    Swarmed = 1
}

public enum PrimeMembraneProjectedBraidStateKind
{
    Dispersed = 0,
    Clustered = 1,
    Swarmed = 2,
    CoherentBraid = 3,
    UnstableBraid = 4
}

public enum PrimeMembraneProjectedHistoryInterpretationKind
{
    ActiveProjection = 0,
    StableBraid = 1,
    ReturnCandidate = 2,
    DeferOnlyEvidence = 3
}

public sealed record PrimeMembraneProjectedLineResidue(
    string LineHandle,
    string SourceSurfaceHandle,
    CrypticProjectionPostureKind ResidualPosture,
    PrimeMembraneProjectedParticipationKind ParticipationKind,
    IReadOnlyList<string> AcceptedContributionHandles,
    bool DistinctionPreserved,
    IReadOnlyList<string> ResidueNotes);

public sealed record PrimeMembraneProjectedBraidHistoryPacket(
    string HistoryHandle,
    string SourceDuplexPacketHandle,
    string EmittedDuplexPacketHandle,
    string MembraneHandle,
    string ProjectionHandle,
    PrimeMembraneProjectedBraidStateKind BraidState,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PrimeClosureIssued,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> LineResidues,
    string OutcomeCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record PrimeMembraneProjectedBraidInterpretationReceipt(
    string ReceiptHandle,
    string HistoryHandle,
    string SourceDuplexPacketHandle,
    string EmittedDuplexPacketHandle,
    string MembraneHandle,
    string ProjectionHandle,
    PrimeMembraneProjectedHistoryInterpretationKind Interpretation,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PreservedDistinctionRequired,
    bool PreservedDistinctionVisible,
    bool ExplicitMembraneReceiptStillRequired,
    bool PrimeClosureStillWithheld,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> VisibleLineResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeMembraneProjectedBraidHistoryEvaluator
{
    public static PrimeMembraneProjectedBraidInterpretationReceipt Evaluate(
        PrimeMembraneProjectedBraidHistoryPacket history,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(history);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var visibleResidues = NormalizeVisibleResidues(history.LineResidues);
        var preservedDistinctionVisible = DeterminePreservedDistinctionVisible(visibleResidues);
        var interpretation = DetermineInterpretation(history, preservedDistinctionVisible);

        return new PrimeMembraneProjectedBraidInterpretationReceipt(
            ReceiptHandle: receiptHandle,
            HistoryHandle: history.HistoryHandle,
            SourceDuplexPacketHandle: history.SourceDuplexPacketHandle,
            EmittedDuplexPacketHandle: history.EmittedDuplexPacketHandle,
            MembraneHandle: history.MembraneHandle,
            ProjectionHandle: history.ProjectionHandle,
            Interpretation: interpretation,
            AdvisoryClosureEligibility: history.AdvisoryClosureEligibility,
            PreservedDistinctionRequired: true,
            PreservedDistinctionVisible: preservedDistinctionVisible,
            ExplicitMembraneReceiptStillRequired: true,
            PrimeClosureStillWithheld: true,
            VisibleLineResidues: visibleResidues,
            ConstraintCodes: DetermineConstraintCodes(history, interpretation, preservedDistinctionVisible),
            ReasonCode: DetermineReasonCode(history, interpretation, preservedDistinctionVisible),
            LawfulBasis: DetermineLawfulBasis(interpretation),
            TimestampUtc: history.TimestampUtc);
    }

    public static PrimeMembraneProjectedHistoryInterpretationKind DetermineInterpretation(
        PrimeMembraneProjectedBraidHistoryPacket history,
        bool preservedDistinctionVisible)
    {
        ArgumentNullException.ThrowIfNull(history);

        if (history.PrimeClosureIssued || !preservedDistinctionVisible)
        {
            return PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence;
        }

        if (history.BraidState == PrimeMembraneProjectedBraidStateKind.UnstableBraid)
        {
            return PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence;
        }

        if (history.BraidState == PrimeMembraneProjectedBraidStateKind.CoherentBraid &&
            history.AdvisoryClosureEligibility == PrimeClosureEligibilityKind.EligibleForMembraneReceipt)
        {
            return PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate;
        }

        if (history.BraidState == PrimeMembraneProjectedBraidStateKind.CoherentBraid)
        {
            return PrimeMembraneProjectedHistoryInterpretationKind.StableBraid;
        }

        return PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection;
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> NormalizeVisibleResidues(
        IReadOnlyList<PrimeMembraneProjectedLineResidue>? residues)
    {
        if (residues is null || residues.Count == 0)
        {
            return [];
        }

        return residues
            .Where(static residue =>
                residue is not null &&
                !string.IsNullOrWhiteSpace(residue.LineHandle) &&
                !string.IsNullOrWhiteSpace(residue.SourceSurfaceHandle))
            .GroupBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool DeterminePreservedDistinctionVisible(
        IReadOnlyList<PrimeMembraneProjectedLineResidue> visibleResidues)
    {
        return visibleResidues.Count > 0 &&
               visibleResidues.All(static residue => residue.DistinctionPreserved) &&
               visibleResidues
                   .Select(static residue => residue.LineHandle)
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .Count() == visibleResidues.Count;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeMembraneProjectedBraidHistoryPacket history,
        PrimeMembraneProjectedHistoryInterpretationKind interpretation,
        bool preservedDistinctionVisible)
    {
        var constraints = new List<string>
        {
            "prime-membrane-interpretation-classification-only",
            "prime-closure-still-withheld-until-explicit-membrane-receipt"
        };

        constraints.Add(history.BraidState switch
        {
            PrimeMembraneProjectedBraidStateKind.Clustered => "projected-history-braid-clustered",
            PrimeMembraneProjectedBraidStateKind.Swarmed => "projected-history-braid-swarmed",
            PrimeMembraneProjectedBraidStateKind.CoherentBraid => "projected-history-braid-coherent",
            PrimeMembraneProjectedBraidStateKind.UnstableBraid => "projected-history-braid-unstable",
            _ => "projected-history-braid-dispersed"
        });

        if (!preservedDistinctionVisible)
        {
            constraints.Add("projected-history-distinction-not-visible");
        }

        if (history.PrimeClosureIssued)
        {
            constraints.Add("projected-history-self-authorized-closure-refused");
        }

        if (interpretation == PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate)
        {
            constraints.Add("projected-history-advisory-return-signal-only");
        }

        if (interpretation == PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence)
        {
            constraints.Add("projected-history-defer-only-evidence");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeMembraneProjectedBraidHistoryPacket history,
        PrimeMembraneProjectedHistoryInterpretationKind interpretation,
        bool preservedDistinctionVisible)
    {
        if (history.PrimeClosureIssued)
        {
            return "projected-history-self-authorized-closure-refused";
        }

        if (!preservedDistinctionVisible)
        {
            return "projected-history-distinction-not-visible";
        }

        if (history.BraidState == PrimeMembraneProjectedBraidStateKind.UnstableBraid)
        {
            return "projected-history-unstable";
        }

        return interpretation switch
        {
            PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate =>
                "projected-history-return-candidate-visible",
            PrimeMembraneProjectedHistoryInterpretationKind.StableBraid =>
                "projected-history-stable-braid-visible",
            PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence =>
                "projected-history-defer-only-evidence",
            _ => "projected-history-active-projection"
        };
    }

    private static string DetermineLawfulBasis(
        PrimeMembraneProjectedHistoryInterpretationKind interpretation)
    {
        return interpretation switch
        {
            PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection =>
                "membrane interpretation may read projected plurality as active field history while preserving per-line distinction and withholding receipt or closure.",
            PrimeMembraneProjectedHistoryInterpretationKind.StableBraid =>
                "membrane interpretation may classify coherent projected braid history while keeping preserved distinction visible and withholding receipt or closure.",
            PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate =>
                "membrane interpretation may classify coherent projected braid history as a return candidate only when preserved distinction remains visible and the return signal remains advisory rather than receipted.",
            PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence =>
                "membrane interpretation must treat unstable, flattened, or self-authorized projected history as defer-only evidence rather than receipt or closure.",
            _ =>
                "projected braid history remains membrane-readable but not yet receipted."
        };
    }
}
