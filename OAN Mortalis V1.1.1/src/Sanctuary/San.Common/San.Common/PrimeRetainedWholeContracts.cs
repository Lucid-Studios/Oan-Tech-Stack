namespace San.Common;

public enum PrimeRetainedWholeKind
{
    NotRetained = 0,
    RetainedPartial = 1,
    RetainedWholeUnclosed = 2,
    ClosureCandidate = 3,
    StillDeferred = 4
}

public sealed record PrimeRetainedHistoryRecord(
    string RecordHandle,
    string MembraneHistoryReceiptHandle,
    string HistoryHandle,
    string MembraneHandle,
    string ProjectionHandle,
    PrimeMembraneReceiptKind ReceiptKind,
    PrimeRetainedWholeKind RetainedWholeKind,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PreservedDistinctionVisible,
    bool RetainedWholenessStillBounded,
    bool ExplicitClosureActStillRequired,
    bool PrimeClosureStillWithheld,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> RetainedResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> UnresolvedResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeRetainedWholeEvaluator
{
    public static PrimeRetainedHistoryRecord Evaluate(
        PrimeMembraneHistoryReceipt historyReceipt,
        string recordHandle)
    {
        ArgumentNullException.ThrowIfNull(historyReceipt);

        if (string.IsNullOrWhiteSpace(recordHandle))
        {
            throw new ArgumentException("Record handle must be provided.", nameof(recordHandle));
        }

        var visibleResidues = NormalizeResidues(historyReceipt.VisibleLineResidues);
        var deferredResidues = NormalizeResidues(historyReceipt.DeferredLineResidues);
        var retainedWholeKind = DetermineRetainedWholeKind(
            historyReceipt,
            visibleResidues.Count,
            deferredResidues.Count);
        var retainedResidues = DetermineRetainedResidues(
            retainedWholeKind,
            visibleResidues,
            deferredResidues);
        var unresolvedResidues = DetermineUnresolvedResidues(
            retainedWholeKind,
            visibleResidues,
            deferredResidues,
            retainedResidues);

        return new PrimeRetainedHistoryRecord(
            RecordHandle: recordHandle,
            MembraneHistoryReceiptHandle: historyReceipt.ReceiptHandle,
            HistoryHandle: historyReceipt.HistoryHandle,
            MembraneHandle: historyReceipt.MembraneHandle,
            ProjectionHandle: historyReceipt.ProjectionHandle,
            ReceiptKind: historyReceipt.ReceiptKind,
            RetainedWholeKind: retainedWholeKind,
            AdvisoryClosureEligibility: historyReceipt.AdvisoryClosureEligibility,
            PreservedDistinctionVisible: historyReceipt.PreservedDistinctionVisible,
            RetainedWholenessStillBounded: true,
            ExplicitClosureActStillRequired: true,
            PrimeClosureStillWithheld: true,
            RetainedResidues: retainedResidues,
            UnresolvedResidues: unresolvedResidues,
            ConstraintCodes: DetermineConstraintCodes(
                historyReceipt,
                retainedWholeKind,
                unresolvedResidues.Count),
            ReasonCode: DetermineReasonCode(historyReceipt, retainedWholeKind),
            LawfulBasis: DetermineLawfulBasis(retainedWholeKind),
            TimestampUtc: historyReceipt.TimestampUtc);
    }

    public static PrimeRetainedWholeKind DetermineRetainedWholeKind(
        PrimeMembraneHistoryReceipt historyReceipt,
        int visibleResidueCount,
        int deferredResidueCount)
    {
        ArgumentNullException.ThrowIfNull(historyReceipt);

        if (!historyReceipt.PreservedDistinctionVisible ||
            historyReceipt.ReceiptKind == PrimeMembraneReceiptKind.Deferred)
        {
            return PrimeRetainedWholeKind.StillDeferred;
        }

        if (historyReceipt.ReceiptKind == PrimeMembraneReceiptKind.SeenOnly)
        {
            return PrimeRetainedWholeKind.NotRetained;
        }

        if (historyReceipt.ReceiptKind == PrimeMembraneReceiptKind.ReturnBearingUnclosed)
        {
            return PrimeRetainedWholeKind.ClosureCandidate;
        }

        if (historyReceipt.ReceiptKind == PrimeMembraneReceiptKind.ReceiptedHistory)
        {
            if (deferredResidueCount > 0 || visibleResidueCount < 2)
            {
                return PrimeRetainedWholeKind.RetainedPartial;
            }

            return PrimeRetainedWholeKind.RetainedWholeUnclosed;
        }

        return PrimeRetainedWholeKind.NotRetained;
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

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> DetermineRetainedResidues(
        PrimeRetainedWholeKind retainedWholeKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> visibleResidues,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> deferredResidues)
    {
        return retainedWholeKind switch
        {
            PrimeRetainedWholeKind.RetainedPartial =>
                visibleResidues
                    .Where(residue =>
                        !deferredResidues.Any(
                            deferred => string.Equals(
                                deferred.LineHandle,
                                residue.LineHandle,
                                StringComparison.OrdinalIgnoreCase)))
                    .Take(1)
                    .ToArray(),
            PrimeRetainedWholeKind.RetainedWholeUnclosed =>
                visibleResidues,
            PrimeRetainedWholeKind.ClosureCandidate =>
                visibleResidues,
            _ =>
                []
        };
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> DetermineUnresolvedResidues(
        PrimeRetainedWholeKind retainedWholeKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> visibleResidues,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> deferredResidues,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> retainedResidues)
    {
        if (retainedWholeKind is PrimeRetainedWholeKind.NotRetained or PrimeRetainedWholeKind.StillDeferred)
        {
            return visibleResidues
                .Concat(deferredResidues)
                .GroupBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .OrderBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return visibleResidues
            .Concat(deferredResidues)
            .Where(residue =>
                !retainedResidues.Any(
                    retained => string.Equals(
                        retained.LineHandle,
                        residue.LineHandle,
                        StringComparison.OrdinalIgnoreCase)))
            .GroupBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeMembraneHistoryReceipt historyReceipt,
        PrimeRetainedWholeKind retainedWholeKind,
        int unresolvedResidueCount)
    {
        var constraints = new List<string>
        {
            "prime-retained-whole-evaluation-not-closure",
            "prime-retained-wholeness-still-bounded",
            "explicit-prime-closure-act-still-required",
            "prime-closure-still-withheld"
        };

        constraints.Add(retainedWholeKind switch
        {
            PrimeRetainedWholeKind.RetainedPartial => "prime-retained-whole-partial",
            PrimeRetainedWholeKind.RetainedWholeUnclosed => "prime-retained-whole-unclosed",
            PrimeRetainedWholeKind.ClosureCandidate => "prime-retained-whole-closure-candidate",
            PrimeRetainedWholeKind.StillDeferred => "prime-retained-whole-still-deferred",
            _ => "prime-retained-whole-not-retained"
        });

        if (!historyReceipt.PreservedDistinctionVisible)
        {
            constraints.Add("prime-retained-whole-distinction-not-visible");
        }

        if (unresolvedResidueCount > 0)
        {
            constraints.Add("prime-retained-whole-unresolved-residues");
        }

        if (retainedWholeKind == PrimeRetainedWholeKind.ClosureCandidate)
        {
            constraints.Add("prime-retained-whole-closure-candidate-still-unclosed");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeMembraneHistoryReceipt historyReceipt,
        PrimeRetainedWholeKind retainedWholeKind)
    {
        if (!historyReceipt.PreservedDistinctionVisible)
        {
            return "prime-retained-whole-distinction-not-visible";
        }

        return retainedWholeKind switch
        {
            PrimeRetainedWholeKind.RetainedPartial =>
                "prime-retained-whole-partial",
            PrimeRetainedWholeKind.RetainedWholeUnclosed =>
                "prime-retained-whole-unclosed",
            PrimeRetainedWholeKind.ClosureCandidate =>
                "prime-retained-whole-closure-candidate",
            PrimeRetainedWholeKind.StillDeferred =>
                "prime-retained-whole-still-deferred",
            _ =>
                "prime-retained-whole-not-retained"
        };
    }

    private static string DetermineLawfulBasis(
        PrimeRetainedWholeKind retainedWholeKind)
    {
        return retainedWholeKind switch
        {
            PrimeRetainedWholeKind.NotRetained =>
                "prime-side retained-whole evaluation may leave receipted history unretained when the membrane has only seen the history and has not yet admitted it into retained form.",
            PrimeRetainedWholeKind.RetainedPartial =>
                "prime-side retained-whole evaluation may admit receipted history into partial retained form while preserving unresolved residues and withholding closure.",
            PrimeRetainedWholeKind.RetainedWholeUnclosed =>
                "prime-side retained-whole evaluation may admit receipted history into retained whole form while preserving bounded distinction and still withholding closure.",
            PrimeRetainedWholeKind.ClosureCandidate =>
                "prime-side retained-whole evaluation may mark receipted return-bearing history as a closure candidate while preserving distinction and still requiring a later explicit closure act.",
            PrimeRetainedWholeKind.StillDeferred =>
                "prime-side retained-whole evaluation must keep receipted history deferred whenever distinction fails or the membrane receipt remains deferred.",
            _ =>
                "prime-side retained-whole evaluation remains bounded."
        };
    }
}
