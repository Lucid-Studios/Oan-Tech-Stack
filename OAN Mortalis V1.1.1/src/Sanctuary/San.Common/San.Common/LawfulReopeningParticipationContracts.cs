namespace San.Common;

public enum ReopeningModeKind
{
    SimpleReopen = 0,
    ModifiedReopen = 1,
    RedopedReopen = 2,
    DistributedContinuation = 3
}

public enum ContinuedParticipationStateKind
{
    Reopened = 0,
    ReopenedModified = 1,
    Redoped = 2,
    ContinuedDistributed = 3,
    Deferred = 4
}

public sealed record LawfulReopeningParticipationRecord(
    string RecordHandle,
    string CommunicativeReceiptHandle,
    string CarrierHandle,
    string SourceRetainedWholeRecordHandle,
    CommunicativeCarrierClassKind SourceCarrierClass,
    FilamentResolutionTargetKind SourceResolutionTarget,
    AntiEchoDispositionKind SourceAntiEchoDisposition,
    ReopeningModeKind ReopeningMode,
    ContinuedParticipationStateKind ParticipationState,
    bool PriorReceiptHistoryPreserved,
    bool PriorPassageStillVisible,
    bool FalseFreshStartRefused,
    bool PreservedDistinctionVisible,
    bool AdjacentPossibilityPreserved,
    bool GelSubstanceStillUnconsumed,
    IReadOnlyList<string> PointerHandles,
    IReadOnlyList<string> SymbolicCarrierHandles,
    IReadOnlyList<string> ReopeningMarkers,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> ActiveResidues,
    IReadOnlyList<PrimeMembraneProjectedLineResidue> DeferredResidues,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class LawfulReopeningParticipationEvaluator
{
    public static LawfulReopeningParticipationRecord Evaluate(
        CommunicativeFilamentResolutionReceipt communicativeReceipt,
        ReopeningModeKind reopeningMode,
        string recordHandle,
        IReadOnlyList<string>? reopeningMarkers = null)
    {
        ArgumentNullException.ThrowIfNull(communicativeReceipt);

        if (string.IsNullOrWhiteSpace(recordHandle))
        {
            throw new ArgumentException("Record handle must be provided.", nameof(recordHandle));
        }

        var activeResidues = NormalizeResidues(communicativeReceipt.PreservedResidues);
        var deferredResidues = NormalizeResidues(communicativeReceipt.DeferredResidues);
        var normalizedMarkers = NormalizeTokens(reopeningMarkers);
        var participationState = DetermineParticipationState(
            communicativeReceipt,
            reopeningMode);
        var adjacentPossibilityPreserved = DetermineAdjacentPossibilityPreserved(
            communicativeReceipt,
            participationState);

        return new LawfulReopeningParticipationRecord(
            RecordHandle: recordHandle,
            CommunicativeReceiptHandle: communicativeReceipt.ReceiptHandle,
            CarrierHandle: communicativeReceipt.CarrierHandle,
            SourceRetainedWholeRecordHandle: communicativeReceipt.SourceRetainedWholeRecordHandle,
            SourceCarrierClass: communicativeReceipt.CarrierClass,
            SourceResolutionTarget: communicativeReceipt.ResolutionTarget,
            SourceAntiEchoDisposition: communicativeReceipt.AntiEchoDisposition,
            ReopeningMode: reopeningMode,
            ParticipationState: participationState,
            PriorReceiptHistoryPreserved: true,
            PriorPassageStillVisible: true,
            FalseFreshStartRefused: true,
            PreservedDistinctionVisible: communicativeReceipt.PreservedDistinctionVisible,
            AdjacentPossibilityPreserved: adjacentPossibilityPreserved,
            GelSubstanceStillUnconsumed: communicativeReceipt.GelSubstanceConsumptionRequested
                ? communicativeReceipt.GelSubstanceConsumptionRefused
                : true,
            PointerHandles: NormalizeTokens(communicativeReceipt.PointerHandles),
            SymbolicCarrierHandles: NormalizeTokens(communicativeReceipt.SymbolicCarrierHandles),
            ReopeningMarkers: normalizedMarkers,
            ActiveResidues: participationState == ContinuedParticipationStateKind.Deferred
                ? []
                : activeResidues,
            DeferredResidues: participationState == ContinuedParticipationStateKind.Deferred
                ? CombineResidues(activeResidues, deferredResidues)
                : deferredResidues,
            ConstraintCodes: DetermineConstraintCodes(
                communicativeReceipt,
                reopeningMode,
                participationState,
                normalizedMarkers.Count,
                deferredResidues.Count),
            ReasonCode: DetermineReasonCode(
                communicativeReceipt,
                reopeningMode,
                participationState),
            LawfulBasis: DetermineLawfulBasis(participationState),
            TimestampUtc: communicativeReceipt.TimestampUtc);
    }

    public static ContinuedParticipationStateKind DetermineParticipationState(
        CommunicativeFilamentResolutionReceipt communicativeReceipt,
        ReopeningModeKind reopeningMode)
    {
        ArgumentNullException.ThrowIfNull(communicativeReceipt);

        if (!communicativeReceipt.PreservedDistinctionVisible ||
            communicativeReceipt.ResolutionTarget == FilamentResolutionTargetKind.WorkingSurfaceOnly ||
            communicativeReceipt.AntiEchoDisposition == AntiEchoDispositionKind.ThinAsEcho ||
            communicativeReceipt.GelSubstanceConsumptionRequested)
        {
            return ContinuedParticipationStateKind.Deferred;
        }

        if (reopeningMode == ReopeningModeKind.DistributedContinuation &&
            communicativeReceipt.ResolutionTarget != FilamentResolutionTargetKind.CGoA)
        {
            return ContinuedParticipationStateKind.Deferred;
        }

        return reopeningMode switch
        {
            ReopeningModeKind.SimpleReopen =>
                ContinuedParticipationStateKind.Reopened,
            ReopeningModeKind.ModifiedReopen =>
                ContinuedParticipationStateKind.ReopenedModified,
            ReopeningModeKind.RedopedReopen =>
                ContinuedParticipationStateKind.Redoped,
            ReopeningModeKind.DistributedContinuation =>
                ContinuedParticipationStateKind.ContinuedDistributed,
            _ =>
                ContinuedParticipationStateKind.Deferred
        };
    }

    private static bool DetermineAdjacentPossibilityPreserved(
        CommunicativeFilamentResolutionReceipt communicativeReceipt,
        ContinuedParticipationStateKind participationState)
    {
        return participationState != ContinuedParticipationStateKind.Deferred &&
               communicativeReceipt.PreservedDistinctionVisible &&
               communicativeReceipt.CarrierTransportOnly &&
               (!communicativeReceipt.GelSubstanceConsumptionRequested ||
                communicativeReceipt.GelSubstanceConsumptionRefused);
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

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> CombineResidues(
        IReadOnlyList<PrimeMembraneProjectedLineResidue> first,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> second)
    {
        return first
            .Concat(second)
            .GroupBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static residue => residue.LineHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        CommunicativeFilamentResolutionReceipt communicativeReceipt,
        ReopeningModeKind reopeningMode,
        ContinuedParticipationStateKind participationState,
        int markerCount,
        int deferredResidueCount)
    {
        var constraints = new List<string>
        {
            "lawful-reopening-prior-history-preserved",
            "lawful-reopening-no-false-fresh-start",
            "lawful-reopening-gel-substance-still-unconsumed"
        };

        constraints.Add(participationState switch
        {
            ContinuedParticipationStateKind.Reopened => "lawful-reopening-simple",
            ContinuedParticipationStateKind.ReopenedModified => "lawful-reopening-modified",
            ContinuedParticipationStateKind.Redoped => "lawful-reopening-redoped",
            ContinuedParticipationStateKind.ContinuedDistributed => "lawful-reopening-distributed-continuation",
            _ => "lawful-reopening-deferred"
        });

        if (!communicativeReceipt.PreservedDistinctionVisible)
        {
            constraints.Add("lawful-reopening-distinction-not-visible");
        }

        if (communicativeReceipt.ResolutionTarget == FilamentResolutionTargetKind.WorkingSurfaceOnly)
        {
            constraints.Add("lawful-reopening-working-surface-only");
        }

        if (reopeningMode == ReopeningModeKind.DistributedContinuation &&
            communicativeReceipt.ResolutionTarget != FilamentResolutionTargetKind.CGoA)
        {
            constraints.Add("lawful-reopening-distributed-continuation-not-lawful");
        }

        if (markerCount > 0)
        {
            constraints.Add("lawful-reopening-markers-visible");
        }

        if (deferredResidueCount > 0)
        {
            constraints.Add("lawful-reopening-deferred-residues-visible");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        CommunicativeFilamentResolutionReceipt communicativeReceipt,
        ReopeningModeKind reopeningMode,
        ContinuedParticipationStateKind participationState)
    {
        if (!communicativeReceipt.PreservedDistinctionVisible)
        {
            return "lawful-reopening-distinction-not-visible";
        }

        if (communicativeReceipt.GelSubstanceConsumptionRequested)
        {
            return "lawful-reopening-gel-substance-still-withheld";
        }

        if (communicativeReceipt.ResolutionTarget == FilamentResolutionTargetKind.WorkingSurfaceOnly)
        {
            return "lawful-reopening-working-surface-only";
        }

        if (reopeningMode == ReopeningModeKind.DistributedContinuation &&
            communicativeReceipt.ResolutionTarget != FilamentResolutionTargetKind.CGoA)
        {
            return "lawful-reopening-distributed-continuation-not-lawful";
        }

        return participationState switch
        {
            ContinuedParticipationStateKind.Reopened =>
                "lawful-reopening-simple",
            ContinuedParticipationStateKind.ReopenedModified =>
                "lawful-reopening-modified",
            ContinuedParticipationStateKind.Redoped =>
                "lawful-reopening-redoped",
            ContinuedParticipationStateKind.ContinuedDistributed =>
                "lawful-reopening-distributed-continuation",
            _ =>
                "lawful-reopening-deferred"
        };
    }

    private static string DetermineLawfulBasis(
        ContinuedParticipationStateKind participationState)
    {
        return participationState switch
        {
            ContinuedParticipationStateKind.Reopened =>
                "lawful reopening may return receipted and retained communicative matter to active participation without voiding prior passage, while preserving distinction and non-consumptive relation to GEL substrate.",
            ContinuedParticipationStateKind.ReopenedModified =>
                "lawful reopening may re-enter participation under modification while preserving prior receipt history, visible distinction, and deferred edges rather than pretending the form is freshly born.",
            ContinuedParticipationStateKind.Redoped =>
                "lawful reopening may redope active posture or carrier density while preserving lawful history, visible distinction, and non-consumptive relation to GEL substrate.",
            ContinuedParticipationStateKind.ContinuedDistributed =>
                "lawful reopening may continue participation distributively through cGoA while preserving prior history, distributed burden, and adjacent possibility.",
            _ =>
                "lawful reopening must defer whenever continued participation would flatten prior passage, destroy adjacent possibility, or pretend a false fresh start."
        };
    }
}
