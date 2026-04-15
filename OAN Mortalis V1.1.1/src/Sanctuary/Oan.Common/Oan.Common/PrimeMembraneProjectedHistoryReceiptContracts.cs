namespace San.Common;

public enum PrimeMembraneReceiptKind
{
    SeenOnly = 0,
    ReceiptedHistory = 1,
    ReturnBearingUnclosed = 2,
    Deferred = 3
}

public sealed record PrimeMembraneHistoryReceipt(
    string ReceiptHandle,
    string HistoryHandle,
    string MembraneHandle,
    string ProjectionHandle,
    PrimeMembraneProjectedHistoryInterpretationKind Interpretation,
    PrimeMembraneReceiptKind ReceiptKind,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PreservedDistinctionVisible,
    bool RetainedWholenessStillWithheld,
    bool PrimeClosureStillWithheld,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> VisibleLineResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredLineResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeMembraneHistoryReceiptEvaluator
{
    public static PrimeMembraneHistoryReceipt Evaluate(
        PrimeMembraneProjectedBraidInterpretationReceipt interpretation,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(interpretation);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var visibleResidues = NormalizeResidues(interpretation.VisibleLineResidues);
        var receiptKind = DetermineReceiptKind(interpretation);
        var deferredResidues = DetermineDeferredResidues(receiptKind, visibleResidues);

        return new PrimeMembraneHistoryReceipt(
            ReceiptHandle: receiptHandle,
            HistoryHandle: interpretation.HistoryHandle,
            MembraneHandle: interpretation.MembraneHandle,
            ProjectionHandle: interpretation.ProjectionHandle,
            Interpretation: interpretation.Interpretation,
            ReceiptKind: receiptKind,
            AdvisoryClosureEligibility: interpretation.AdvisoryClosureEligibility,
            PreservedDistinctionVisible: interpretation.PreservedDistinctionVisible,
            RetainedWholenessStillWithheld: true,
            PrimeClosureStillWithheld: true,
            VisibleLineResidues: visibleResidues,
            DeferredLineResidues: deferredResidues,
            ConstraintCodes: DetermineConstraintCodes(interpretation, receiptKind, deferredResidues.Count),
            ReasonCode: DetermineReasonCode(interpretation, receiptKind),
            LawfulBasis: DetermineLawfulBasis(receiptKind),
            TimestampUtc: interpretation.TimestampUtc);
    }

    public static PrimeMembraneReceiptKind DetermineReceiptKind(
        PrimeMembraneProjectedBraidInterpretationReceipt interpretation)
    {
        ArgumentNullException.ThrowIfNull(interpretation);

        if (!interpretation.PreservedDistinctionVisible ||
            interpretation.Interpretation == PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence)
        {
            return PrimeMembraneReceiptKind.Deferred;
        }

        return interpretation.Interpretation switch
        {
            PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate =>
                PrimeMembraneReceiptKind.ReturnBearingUnclosed,
            PrimeMembraneProjectedHistoryInterpretationKind.StableBraid =>
                PrimeMembraneReceiptKind.ReceiptedHistory,
            _ =>
                PrimeMembraneReceiptKind.SeenOnly
        };
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> NormalizeResidues(
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

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> DetermineDeferredResidues(
        PrimeMembraneReceiptKind receiptKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> visibleResidues)
    {
        return receiptKind switch
        {
            PrimeMembraneReceiptKind.SeenOnly => visibleResidues,
            PrimeMembraneReceiptKind.Deferred => visibleResidues,
            _ => []
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeMembraneProjectedBraidInterpretationReceipt interpretation,
        PrimeMembraneReceiptKind receiptKind,
        int deferredResidueCount)
    {
        var constraints = new List<string>
        {
            "prime-membrane-history-receipt-not-closure",
            "retained-wholeness-still-withheld",
            "prime-closure-still-withheld"
        };

        constraints.Add(receiptKind switch
        {
            PrimeMembraneReceiptKind.ReceiptedHistory => "prime-membrane-history-receipted",
            PrimeMembraneReceiptKind.ReturnBearingUnclosed => "prime-membrane-history-return-bearing-unclosed",
            PrimeMembraneReceiptKind.Deferred => "prime-membrane-history-deferred",
            _ => "prime-membrane-history-seen-only"
        });

        if (!interpretation.PreservedDistinctionVisible)
        {
            constraints.Add("prime-membrane-history-distinction-not-visible");
        }

        if (deferredResidueCount > 0)
        {
            constraints.Add("prime-membrane-history-residues-deferred");
        }

        if (receiptKind == PrimeMembraneReceiptKind.ReturnBearingUnclosed)
        {
            constraints.Add("prime-membrane-history-return-still-advisory");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeMembraneProjectedBraidInterpretationReceipt interpretation,
        PrimeMembraneReceiptKind receiptKind)
    {
        if (!interpretation.PreservedDistinctionVisible)
        {
            return "prime-membrane-history-receipt-distinction-not-visible";
        }

        return receiptKind switch
        {
            PrimeMembraneReceiptKind.ReceiptedHistory => "prime-membrane-history-receipted",
            PrimeMembraneReceiptKind.ReturnBearingUnclosed => "prime-membrane-history-receipted-return-bearing-unclosed",
            PrimeMembraneReceiptKind.Deferred => "prime-membrane-history-deferred",
            _ => "prime-membrane-history-seen-only"
        };
    }

    private static string DetermineLawfulBasis(
        PrimeMembraneReceiptKind receiptKind)
    {
        return receiptKind switch
        {
            PrimeMembraneReceiptKind.SeenOnly =>
                "prime membrane may acknowledge interpreted projected history as seen without promoting it into retained wholeness, stronger receipt, or closure.",
            PrimeMembraneReceiptKind.ReceiptedHistory =>
                "prime membrane may receipt interpreted stable braid history as membrane-visible while preserving distinction and withholding retained wholeness and closure.",
            PrimeMembraneReceiptKind.ReturnBearingUnclosed =>
                "prime membrane may receipt interpreted return-bearing history as unclosed membrane-visible history while preserving distinction and withholding Prime closure.",
            PrimeMembraneReceiptKind.Deferred =>
                "prime membrane must keep interpreted history deferred whenever distinction fails or the interpreted history remains defer-only evidence.",
            _ =>
                "prime membrane history receipt remains bounded."
        };
    }
}
