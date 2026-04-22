namespace San.Common;

public enum PostPrimeClosureContinuityKind
{
    FieldContinuing = 0,
    CarrierActive = 1,
    ResidueLawful = 2,
    DeferredEdgeOpen = 3,
    ReentryPermitted = 4,
    ContinuityWithheld = 5
}

public sealed record PostPrimeClosureContinuityRecord(
    string RecordHandle,
    string SourceClosureActRecordHandle,
    string SourceReopeningRecordHandle,
    PrimeClosureActKind ClosureActKind,
    PostPrimeClosureContinuityKind ContinuityKind,
    string ClosedProductHandle,
    string ContinuityScope,
    bool ClosedProductStillStanding,
    bool BearingFieldNonVoidAttested,
    bool ActiveCarriersStillPresent,
    bool LawfulResiduesStillPresent,
    bool DeferredEdgesStillOpen,
    bool ReentryPermitted,
    bool PreservedDistinctionStillVisible,
    IReadOnlyList<string> ActiveCarrierHandles,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> LawfulResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredEdges,
    IReadOnlyList<string> ReentryConditions,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PostPrimeClosureContinuityEvaluator
{
    public static PostPrimeClosureContinuityRecord Evaluate(
        PrimeClosureActRecord closureActRecord,
        LawfulReopeningParticipationRecord reopeningRecord,
        string recordHandle)
    {
        ArgumentNullException.ThrowIfNull(closureActRecord);
        ArgumentNullException.ThrowIfNull(reopeningRecord);

        if (string.IsNullOrWhiteSpace(recordHandle))
        {
            throw new ArgumentException("Record handle must be provided.", nameof(recordHandle));
        }

        if (!string.Equals(
                closureActRecord.SourceReopeningRecordHandle,
                reopeningRecord.RecordHandle,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Continuity must derive from the same reopening-bearing record as the closure act.",
                nameof(reopeningRecord));
        }

        var activeCarrierHandles = NormalizeTokens(reopeningRecord.SymbolicCarrierHandles);
        var lawfulResidues = NormalizeResidues(closureActRecord.AttestedRemainingProductResidues);
        var deferredEdges = NormalizeResidues(
            closureActRecord.DeferredResidues.Count > 0
                ? closureActRecord.DeferredResidues
                : reopeningRecord.DeferredResidues);
        var reentryConditions = DetermineReentryConditions(
            closureActRecord,
            reopeningRecord,
            activeCarrierHandles.Count,
            lawfulResidues.Count);
        var reentryPermitted = DetermineReentryPermitted(
            closureActRecord,
            reopeningRecord,
            reentryConditions.Count);
        var continuityKind = DetermineContinuityKind(
            closureActRecord,
            reentryPermitted,
            activeCarrierHandles.Count,
            lawfulResidueCount: lawfulResidues.Count,
            deferredEdgeCount: deferredEdges.Count);

        return new PostPrimeClosureContinuityRecord(
            RecordHandle: recordHandle,
            SourceClosureActRecordHandle: closureActRecord.RecordHandle,
            SourceReopeningRecordHandle: reopeningRecord.RecordHandle,
            ClosureActKind: closureActRecord.ClosureActKind,
            ContinuityKind: continuityKind,
            ClosedProductHandle: closureActRecord.AttestedRemainingProductHandle,
            ContinuityScope: DetermineContinuityScope(continuityKind),
            ClosedProductStillStanding: closureActRecord.ClosureActKind == PrimeClosureActKind.ClosureExecuted,
            BearingFieldNonVoidAttested: closureActRecord.BearingFieldStillVisible,
            ActiveCarriersStillPresent: activeCarrierHandles.Count > 0,
            LawfulResiduesStillPresent: lawfulResidues.Count > 0,
            DeferredEdgesStillOpen: deferredEdges.Count > 0,
            ReentryPermitted: reentryPermitted,
            PreservedDistinctionStillVisible: closureActRecord.PriorPassageStillVisible &&
                reopeningRecord.PreservedDistinctionVisible,
            ActiveCarrierHandles: activeCarrierHandles,
            LawfulResidues: lawfulResidues,
            DeferredEdges: deferredEdges,
            ReentryConditions: reentryConditions,
            ConstraintCodes: DetermineConstraintCodes(
                closureActRecord,
                reopeningRecord,
                continuityKind,
                activeCarrierHandles.Count,
                lawfulResidues.Count,
                deferredEdges.Count,
                reentryPermitted),
            ReasonCode: DetermineReasonCode(
                closureActRecord,
                continuityKind,
                deferredEdges.Count,
                reentryPermitted),
            LawfulBasis: DetermineLawfulBasis(continuityKind),
            TimestampUtc: closureActRecord.TimestampUtc);
    }

    public static PostPrimeClosureContinuityKind DetermineContinuityKind(
        PrimeClosureActRecord closureActRecord,
        bool reentryPermitted,
        int activeCarrierCount,
        int lawfulResidueCount,
        int deferredEdgeCount)
    {
        ArgumentNullException.ThrowIfNull(closureActRecord);

        if (!closureActRecord.BearingFieldStillVisible ||
            !closureActRecord.GelSubstanceStillUnconsumed)
        {
            return PostPrimeClosureContinuityKind.ContinuityWithheld;
        }

        if (closureActRecord.ClosureActKind == PrimeClosureActKind.ClosureExecuted)
        {
            if (reentryPermitted)
            {
                return PostPrimeClosureContinuityKind.ReentryPermitted;
            }

            if (lawfulResidueCount > 0)
            {
                return PostPrimeClosureContinuityKind.ResidueLawful;
            }

            if (activeCarrierCount > 0)
            {
                return PostPrimeClosureContinuityKind.CarrierActive;
            }

            return PostPrimeClosureContinuityKind.FieldContinuing;
        }

        if (closureActRecord.ClosureActKind == PrimeClosureActKind.ClosureDeclined &&
            deferredEdgeCount > 0)
        {
            return PostPrimeClosureContinuityKind.DeferredEdgeOpen;
        }

        return PostPrimeClosureContinuityKind.ContinuityWithheld;
    }

    private static bool DetermineReentryPermitted(
        PrimeClosureActRecord closureActRecord,
        LawfulReopeningParticipationRecord reopeningRecord,
        int reentryConditionCount)
    {
        return closureActRecord.ClosureActKind == PrimeClosureActKind.ClosureExecuted &&
               reopeningRecord.PriorReceiptHistoryPreserved &&
               reopeningRecord.PriorPassageStillVisible &&
               reopeningRecord.FalseFreshStartRefused &&
               reopeningRecord.PreservedDistinctionVisible &&
               reopeningRecord.AdjacentPossibilityPreserved &&
               reopeningRecord.GelSubstanceStillUnconsumed &&
               reentryConditionCount > 0;
    }

    private static IReadOnlyList<string> DetermineReentryConditions(
        PrimeClosureActRecord closureActRecord,
        LawfulReopeningParticipationRecord reopeningRecord,
        int activeCarrierCount,
        int lawfulResidueCount)
    {
        var conditions = new List<string>();

        if (closureActRecord.ClosureActKind == PrimeClosureActKind.ClosureExecuted)
        {
            conditions.Add("post-prime-continuity-closure-executed");
        }

        if (reopeningRecord.PriorReceiptHistoryPreserved)
        {
            conditions.Add("post-prime-continuity-prior-receipt-history-preserved");
        }

        if (reopeningRecord.PriorPassageStillVisible)
        {
            conditions.Add("post-prime-continuity-prior-passage-visible");
        }

        if (reopeningRecord.FalseFreshStartRefused)
        {
            conditions.Add("post-prime-continuity-false-fresh-start-refused");
        }

        if (reopeningRecord.PreservedDistinctionVisible)
        {
            conditions.Add("post-prime-continuity-distinction-preserved");
        }

        if (reopeningRecord.AdjacentPossibilityPreserved)
        {
            conditions.Add("post-prime-continuity-adjacent-possibility-preserved");
        }

        if (reopeningRecord.GelSubstanceStillUnconsumed)
        {
            conditions.Add("post-prime-continuity-gel-substrate-still-unconsumed");
        }

        if (activeCarrierCount > 0)
        {
            conditions.Add("post-prime-continuity-active-carriers-visible");
        }

        if (lawfulResidueCount > 0)
        {
            conditions.Add("post-prime-continuity-lawful-residues-visible");
        }

        if (reopeningRecord.ReopeningMarkers.Count > 0)
        {
            conditions.Add("post-prime-continuity-reopening-markers-visible");
        }

        return conditions;
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return [];
        }

        return tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

    private static string DetermineContinuityScope(
        PostPrimeClosureContinuityKind continuityKind)
    {
        return continuityKind switch
        {
            PostPrimeClosureContinuityKind.FieldContinuing =>
                "closed-product-field-continuing",
            PostPrimeClosureContinuityKind.CarrierActive =>
                "closed-product-active-carriers",
            PostPrimeClosureContinuityKind.ResidueLawful =>
                "closed-product-lawful-residue",
            PostPrimeClosureContinuityKind.DeferredEdgeOpen =>
                "closure-boundary-deferred-edge-open",
            PostPrimeClosureContinuityKind.ReentryPermitted =>
                "closed-product-bounded-reentry",
            _ =>
                "continuity-withheld-visible-boundary"
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeClosureActRecord closureActRecord,
        LawfulReopeningParticipationRecord reopeningRecord,
        PostPrimeClosureContinuityKind continuityKind,
        int activeCarrierCount,
        int lawfulResidueCount,
        int deferredEdgeCount,
        bool reentryPermitted)
    {
        var constraints = new List<string>
        {
            "post-prime-continuity-bearing-field-not-void",
            "post-prime-continuity-gel-substrate-still-unconsumed",
            "post-prime-continuity-closed-product-and-field-distinct"
        };

        constraints.Add(continuityKind switch
        {
            PostPrimeClosureContinuityKind.FieldContinuing => "post-prime-continuity-field-continuing",
            PostPrimeClosureContinuityKind.CarrierActive => "post-prime-continuity-carriers-active",
            PostPrimeClosureContinuityKind.ResidueLawful => "post-prime-continuity-lawful-residue",
            PostPrimeClosureContinuityKind.DeferredEdgeOpen => "post-prime-continuity-deferred-edge-open",
            PostPrimeClosureContinuityKind.ReentryPermitted => "post-prime-continuity-reentry-permitted",
            _ => "post-prime-continuity-withheld"
        });

        if (closureActRecord.ClosureActKind != PrimeClosureActKind.ClosureExecuted)
        {
            constraints.Add("post-prime-continuity-closure-not-executed");
        }

        if (!reopeningRecord.PreservedDistinctionVisible)
        {
            constraints.Add("post-prime-continuity-distinction-not-visible");
        }

        if (activeCarrierCount > 0)
        {
            constraints.Add("post-prime-continuity-active-carriers-visible");
        }

        if (lawfulResidueCount > 0)
        {
            constraints.Add("post-prime-continuity-lawful-residues-visible");
        }

        if (deferredEdgeCount > 0)
        {
            constraints.Add("post-prime-continuity-deferred-edges-visible");
        }

        if (reentryPermitted)
        {
            constraints.Add("post-prime-continuity-bounded-reentry");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeClosureActRecord closureActRecord,
        PostPrimeClosureContinuityKind continuityKind,
        int deferredEdgeCount,
        bool reentryPermitted)
    {
        if (continuityKind == PostPrimeClosureContinuityKind.ContinuityWithheld &&
            closureActRecord.ClosureActKind != PrimeClosureActKind.ClosureExecuted)
        {
            return "post-prime-continuity-withheld-closure-not-executed";
        }

        if (continuityKind == PostPrimeClosureContinuityKind.DeferredEdgeOpen &&
            deferredEdgeCount > 0)
        {
            return "post-prime-continuity-deferred-edge-open";
        }

        if (reentryPermitted)
        {
            return "post-prime-continuity-reentry-permitted";
        }

        return continuityKind switch
        {
            PostPrimeClosureContinuityKind.ResidueLawful =>
                "post-prime-continuity-lawful-residue",
            PostPrimeClosureContinuityKind.CarrierActive =>
                "post-prime-continuity-carrier-active",
            PostPrimeClosureContinuityKind.FieldContinuing =>
                "post-prime-continuity-field-continuing",
            _ =>
                "post-prime-continuity-withheld"
        };
    }

    private static string DetermineLawfulBasis(
        PostPrimeClosureContinuityKind continuityKind)
    {
        return continuityKind switch
        {
            PostPrimeClosureContinuityKind.FieldContinuing =>
                "post-closure continuity may attest that the bearing field remains non-void after explicit Prime closure, even when only bounded continuity rather than active re-entry still remains.",
            PostPrimeClosureContinuityKind.CarrierActive =>
                "post-closure continuity may attest that symbolic carriers remain in lawful circulation after a product stands, without pretending the field is equally unresolved everywhere.",
            PostPrimeClosureContinuityKind.ResidueLawful =>
                "post-closure continuity may attest that lawful residues remain visible around a closed product, so closure does not erase the field substance that still bears relation.",
            PostPrimeClosureContinuityKind.DeferredEdgeOpen =>
                "post-closure continuity may keep deferred edges visible at the closure boundary when the field is not yet lawfully empty, rather than collapsing those edges into false completion.",
            PostPrimeClosureContinuityKind.ReentryPermitted =>
                "post-closure continuity may permit bounded re-entry after explicit Prime closure when prior passage, preserved distinction, adjacent possibility, and non-consumptive relation to GEL substrate all remain attested.",
            _ =>
                "post-closure continuity must remain withheld whenever continuity would falsely simulate a living field that has not yet been lawfully attested."
        };
    }
}
