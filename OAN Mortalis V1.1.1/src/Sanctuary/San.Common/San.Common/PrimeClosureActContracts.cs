namespace San.Common;

public enum PrimeClosureActKind
{
    ClosureWithheld = 0,
    ClosureDeclined = 1,
    ClosureExecuted = 2
}

public sealed record PrimeClosureActRecord(
    string RecordHandle,
    string SourceRetainedWholeRecordHandle,
    string SourceReopeningRecordHandle,
    PrimeRetainedWholeKind RetainedWholeKind,
    ContinuedParticipationStateKind ReopeningParticipationState,
    PrimeClosureActKind ClosureActKind,
    string ClosureScope,
    string AttestedRemainingProductHandle,
    bool PriorPassageStillVisible,
    bool UnresolvedResiduesStillVisible,
    bool DeferredResiduesStillVisible,
    bool GelSubstanceStillUnconsumed,
    bool BearingFieldStillVisible,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> AttestedRemainingProductResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> UnresolvedResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeClosureActEvaluator
{
    public static PrimeClosureActRecord Evaluate(
        PrimeRetainedHistoryRecord retainedHistory,
        LawfulReopeningParticipationRecord reopeningRecord,
        string recordHandle,
        string attestedRemainingProductHandle)
    {
        ArgumentNullException.ThrowIfNull(retainedHistory);
        ArgumentNullException.ThrowIfNull(reopeningRecord);

        if (string.IsNullOrWhiteSpace(recordHandle))
        {
            throw new ArgumentException("Record handle must be provided.", nameof(recordHandle));
        }

        if (string.IsNullOrWhiteSpace(attestedRemainingProductHandle))
        {
            throw new ArgumentException("Attested remaining product handle must be provided.", nameof(attestedRemainingProductHandle));
        }

        if (!string.Equals(
                reopeningRecord.SourceRetainedWholeRecordHandle,
                retainedHistory.RecordHandle,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Reopening record must derive from the retained-whole record.",
                nameof(reopeningRecord));
        }

        var unresolvedResidues = NormalizeResidues(retainedHistory.UnresolvedResidues);
        var deferredResidues = NormalizeResidues(reopeningRecord.DeferredResidues);
        var closureActKind = DetermineClosureActKind(
            retainedHistory,
            reopeningRecord,
            unresolvedResidues.Count,
            deferredResidues.Count);
        var attestedProductResidues = DetermineAttestedRemainingProductResidues(
            reopeningRecord,
            closureActKind);

        return new PrimeClosureActRecord(
            RecordHandle: recordHandle,
            SourceRetainedWholeRecordHandle: retainedHistory.RecordHandle,
            SourceReopeningRecordHandle: reopeningRecord.RecordHandle,
            RetainedWholeKind: retainedHistory.RetainedWholeKind,
            ReopeningParticipationState: reopeningRecord.ParticipationState,
            ClosureActKind: closureActKind,
            ClosureScope: DetermineClosureScope(closureActKind),
            AttestedRemainingProductHandle: attestedRemainingProductHandle,
            PriorPassageStillVisible: reopeningRecord.PriorPassageStillVisible,
            UnresolvedResiduesStillVisible: unresolvedResidues.Count > 0,
            DeferredResiduesStillVisible: deferredResidues.Count > 0,
            GelSubstanceStillUnconsumed: reopeningRecord.GelSubstanceStillUnconsumed,
            BearingFieldStillVisible: true,
            AttestedRemainingProductResidues: attestedProductResidues,
            UnresolvedResidues: unresolvedResidues,
            DeferredResidues: deferredResidues,
            ConstraintCodes: DetermineConstraintCodes(
                retainedHistory,
                reopeningRecord,
                closureActKind,
                unresolvedResidues.Count,
                deferredResidues.Count),
            ReasonCode: DetermineReasonCode(
                retainedHistory,
                reopeningRecord,
                closureActKind,
                unresolvedResidues.Count,
                deferredResidues.Count),
            LawfulBasis: DetermineLawfulBasis(closureActKind),
            TimestampUtc: reopeningRecord.TimestampUtc);
    }

    public static PrimeClosureActKind DetermineClosureActKind(
        PrimeRetainedHistoryRecord retainedHistory,
        LawfulReopeningParticipationRecord reopeningRecord,
        int unresolvedResidueCount,
        int deferredResidueCount)
    {
        ArgumentNullException.ThrowIfNull(retainedHistory);
        ArgumentNullException.ThrowIfNull(reopeningRecord);

        if (retainedHistory.RetainedWholeKind != PrimeRetainedWholeKind.ClosureCandidate)
        {
            return PrimeClosureActKind.ClosureWithheld;
        }

        if (reopeningRecord.ParticipationState == ContinuedParticipationStateKind.Deferred ||
            !retainedHistory.PreservedDistinctionVisible ||
            !reopeningRecord.PreservedDistinctionVisible ||
            !reopeningRecord.FalseFreshStartRefused ||
            !reopeningRecord.AdjacentPossibilityPreserved ||
            !reopeningRecord.GelSubstanceStillUnconsumed ||
            unresolvedResidueCount > 0 ||
            deferredResidueCount > 0)
        {
            return PrimeClosureActKind.ClosureDeclined;
        }

        return PrimeClosureActKind.ClosureExecuted;
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

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> DetermineAttestedRemainingProductResidues(
        LawfulReopeningParticipationRecord reopeningRecord,
        PrimeClosureActKind closureActKind)
    {
        return closureActKind == PrimeClosureActKind.ClosureExecuted
            ? NormalizeResidues(reopeningRecord.ActiveResidues)
            : [];
    }

    private static string DetermineClosureScope(
        PrimeClosureActKind closureActKind)
    {
        return closureActKind switch
        {
            PrimeClosureActKind.ClosureExecuted => "remaining-product-only",
            PrimeClosureActKind.ClosureDeclined => "closure-declined-visible-boundary",
            _ => "closure-withheld-visible-boundary"
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeRetainedHistoryRecord retainedHistory,
        LawfulReopeningParticipationRecord reopeningRecord,
        PrimeClosureActKind closureActKind,
        int unresolvedResidueCount,
        int deferredResidueCount)
    {
        var constraints = new List<string>
        {
            "prime-closure-explicit-act-only",
            "prime-closure-not-reset",
            "prime-closure-does-not-void-bearing-field",
            "prime-closure-gel-substrate-still-unconsumed"
        };

        constraints.Add(closureActKind switch
        {
            PrimeClosureActKind.ClosureExecuted => "prime-closure-executed",
            PrimeClosureActKind.ClosureDeclined => "prime-closure-declined",
            _ => "prime-closure-withheld"
        });

        if (retainedHistory.RetainedWholeKind != PrimeRetainedWholeKind.ClosureCandidate)
        {
            constraints.Add("prime-closure-not-closure-candidate");
        }

        if (reopeningRecord.ParticipationState == ContinuedParticipationStateKind.Deferred)
        {
            constraints.Add("prime-closure-reopening-deferred");
        }

        if (!reopeningRecord.AdjacentPossibilityPreserved)
        {
            constraints.Add("prime-closure-adjacent-possibility-not-preserved");
        }

        if (unresolvedResidueCount > 0)
        {
            constraints.Add("prime-closure-unresolved-residues-visible");
        }

        if (deferredResidueCount > 0)
        {
            constraints.Add("prime-closure-deferred-residues-visible");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeRetainedHistoryRecord retainedHistory,
        LawfulReopeningParticipationRecord reopeningRecord,
        PrimeClosureActKind closureActKind,
        int unresolvedResidueCount,
        int deferredResidueCount)
    {
        if (retainedHistory.RetainedWholeKind != PrimeRetainedWholeKind.ClosureCandidate)
        {
            return "prime-closure-withheld-not-closure-candidate";
        }

        if (reopeningRecord.ParticipationState == ContinuedParticipationStateKind.Deferred)
        {
            return "prime-closure-declined-reopening-deferred";
        }

        if (unresolvedResidueCount > 0)
        {
            return "prime-closure-declined-unresolved-residues-visible";
        }

        if (deferredResidueCount > 0)
        {
            return "prime-closure-declined-deferred-residues-visible";
        }

        if (!reopeningRecord.AdjacentPossibilityPreserved)
        {
            return "prime-closure-declined-adjacent-possibility-not-preserved";
        }

        return closureActKind switch
        {
            PrimeClosureActKind.ClosureExecuted =>
                "prime-closure-executed-remaining-product-attested",
            PrimeClosureActKind.ClosureDeclined =>
                "prime-closure-declined",
            _ =>
                "prime-closure-withheld"
        };
    }

    private static string DetermineLawfulBasis(
        PrimeClosureActKind closureActKind)
    {
        return closureActKind switch
        {
            PrimeClosureActKind.ClosureWithheld =>
                "Prime closure may remain withheld whenever retained history has not yet lawfully matured into a closure candidate, so closure is never inferred from earlier stages alone.",
            PrimeClosureActKind.ClosureDeclined =>
                "Prime closure must decline whenever reopening, unresolved residue, deferred edges, or field-preservation law show that a lawful product does not yet stand without lying about what remains open.",
            PrimeClosureActKind.ClosureExecuted =>
                "Prime closure may execute only as explicit attestation that a lawful remaining product now stands from the process, without resetting history, voiding the bearing field, or consuming GEL substrate.",
            _ =>
                "Prime closure remains an explicit act."
        };
    }
}
