namespace San.Common;

public enum CmeTruthSeekingBalanceRegionKind
{
    JointlySatisfiable = 0,
    FixationRegion = 1,
    DriftRegion = 2,
    WeightlessRegion = 3,
    OverclaimRegion = 4,
    DeferWithTension = 5,
    BalanceRefused = 6
}

public enum CmeTruthSeekingBalanceDispositionKind
{
    Admissible = 0,
    Deferred = 1,
    Refused = 2
}

public enum CmeTruthSeekingTransitionTriggerKind
{
    EvidenceUpdate = 0,
    Contradiction = 1,
    Decay = 2,
    OperatorReview = 3,
    ScheduledReevaluation = 4
}

public sealed record CmeTruthSeekingOrientationStateRecord(
    string StateHandle,
    string ClaimHandle,
    decimal CenterCoherenceScore,
    decimal EvidenceSupportScore,
    decimal MaintenanceCostScore,
    decimal UncertaintyBoundScore,
    IReadOnlyList<string> EngramLinkHandles,
    IReadOnlyList<string> EvidenceBasisHandles,
    IReadOnlyList<string> TensionHandles,
    IReadOnlyList<string> UnknownScopeHandles,
    DateTimeOffset TimestampUtc);

public sealed record CmeTruthSeekingBalanceTransitionRequest(
    string RequestHandle,
    CmeTruthSeekingOrientationReceipt OrientationReceipt,
    CmeTruthSeekingOrientationStateRecord PriorState,
    CmeTruthSeekingOrientationStateRecord ProposedState,
    CmeTruthSeekingTransitionTriggerKind TriggerKind,
    IReadOnlyList<string> JustificationTraceHandles,
    IReadOnlyList<string> MaintenanceJustificationHandles,
    IReadOnlyList<string> RevisionJustificationHandles,
    IReadOnlyList<string> ContradictionReceiptHandles,
    bool PreserveClaimSolelyForCoherence,
    bool ScopeExpandedWithoutUncertaintyBounds,
    bool ReEvaluationScheduled,
    DateTimeOffset TimestampUtc);

public sealed record CmeTruthSeekingBalanceTransitionReceipt(
    string ReceiptHandle,
    string RequestHandle,
    string OrientationReceiptHandle,
    string PriorStateHandle,
    string ProposedStateHandle,
    string ClaimHandle,
    CmeTruthSeekingTransitionTriggerKind TriggerKind,
    CmeTruthSeekingBalanceRegionKind RegionKind,
    CmeTruthSeekingBalanceDispositionKind Disposition,
    decimal CenterDelta,
    decimal CorrectionDelta,
    decimal CostDelta,
    decimal HumilityDelta,
    bool OrientationAttested,
    bool ScoresWithinRange,
    bool CenterWithinBound,
    bool CorrectionWithinBound,
    bool CostWithinBound,
    bool HumilityWithinBound,
    bool JointlySatisfiable,
    bool NoPressureSilentlyDegraded,
    bool SymmetricJustificationPresent,
    bool DeferWithTension,
    bool PreserveAndReviseJudgedUnderSameLaw,
    bool ClaimPreservedSolelyForCoherenceRefused,
    bool ScopeExpansionWithoutUncertaintyRefused,
    bool CmeMintingWithheld,
    bool RoleEnactmentWithheld,
    bool ActionAuthorityWithheld,
    bool CandidateOnly,
    IReadOnlyList<string> JustificationTraceHandles,
    IReadOnlyList<string> MaintenanceJustificationHandles,
    IReadOnlyList<string> RevisionJustificationHandles,
    IReadOnlyList<string> ContradictionReceiptHandles,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class CmeTruthSeekingBalanceEvaluator
{
    private const decimal MinimumPressureScore = 0.20m;
    private const decimal DominanceMargin = 0.40m;

    public static CmeTruthSeekingBalanceTransitionReceipt Evaluate(
        CmeTruthSeekingBalanceTransitionRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OrientationReceipt);
        ArgumentNullException.ThrowIfNull(request.PriorState);
        ArgumentNullException.ThrowIfNull(request.ProposedState);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var justificationTraceHandles = NormalizeTokens(request.JustificationTraceHandles);
        var maintenanceJustificationHandles = NormalizeTokens(request.MaintenanceJustificationHandles);
        var revisionJustificationHandles = NormalizeTokens(request.RevisionJustificationHandles);
        var contradictionReceiptHandles = NormalizeTokens(request.ContradictionReceiptHandles);
        var orientationAttested = request.OrientationReceipt.TruthSeekingOrientationAttested &&
                                  request.OrientationReceipt.Disposition == CmeTruthSeekingOrientationDispositionKind.Attested;
        var scoresWithinRange = ScoresWithinRange(request.PriorState) &&
                                ScoresWithinRange(request.ProposedState);
        var centerWithinBound = request.ProposedState.CenterCoherenceScore >= MinimumPressureScore;
        var correctionWithinBound = request.ProposedState.EvidenceSupportScore >= MinimumPressureScore;
        var costWithinBound = request.ProposedState.MaintenanceCostScore >= MinimumPressureScore;
        var humilityWithinBound = request.ProposedState.UncertaintyBoundScore >= MinimumPressureScore;
        var symmetricJustificationPresent = maintenanceJustificationHandles.Count > 0 &&
                                            revisionJustificationHandles.Count > 0;
        var pressureDegradedBelowBound = PressureDegradedBelowBound(request.PriorState, request.ProposedState);
        var noPressureSilentlyDegraded = !pressureDegradedBelowBound ||
                                         (contradictionReceiptHandles.Count > 0 && request.ReEvaluationScheduled);
        var centerCorrectionDominanceDetected =
            Math.Abs(request.ProposedState.CenterCoherenceScore - request.ProposedState.EvidenceSupportScore) >=
            DominanceMargin;
        var jointlySatisfiable = orientationAttested &&
                                 scoresWithinRange &&
                                 centerWithinBound &&
                                 correctionWithinBound &&
                                 costWithinBound &&
                                 humilityWithinBound &&
                                 symmetricJustificationPresent &&
                                 noPressureSilentlyDegraded &&
                                 !centerCorrectionDominanceDetected &&
                                 !request.PreserveClaimSolelyForCoherence &&
                                 !request.ScopeExpandedWithoutUncertaintyBounds;
        var regionKind = DetermineRegionKind(
            orientationAttested,
            scoresWithinRange,
            centerWithinBound,
            correctionWithinBound,
            costWithinBound,
            humilityWithinBound,
            symmetricJustificationPresent,
            noPressureSilentlyDegraded,
            request.PreserveClaimSolelyForCoherence,
            request.ScopeExpandedWithoutUncertaintyBounds,
            request.ProposedState);
        var disposition = DetermineDisposition(regionKind);
        var deferWithTension = disposition == CmeTruthSeekingBalanceDispositionKind.Deferred;

        return new CmeTruthSeekingBalanceTransitionReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            OrientationReceiptHandle: request.OrientationReceipt.ReceiptHandle,
            PriorStateHandle: request.PriorState.StateHandle,
            ProposedStateHandle: request.ProposedState.StateHandle,
            ClaimHandle: NormalizeHandle(request.ProposedState.ClaimHandle),
            TriggerKind: request.TriggerKind,
            RegionKind: regionKind,
            Disposition: disposition,
            CenterDelta: request.ProposedState.CenterCoherenceScore - request.PriorState.CenterCoherenceScore,
            CorrectionDelta: request.ProposedState.EvidenceSupportScore - request.PriorState.EvidenceSupportScore,
            CostDelta: request.ProposedState.MaintenanceCostScore - request.PriorState.MaintenanceCostScore,
            HumilityDelta: request.ProposedState.UncertaintyBoundScore - request.PriorState.UncertaintyBoundScore,
            OrientationAttested: orientationAttested,
            ScoresWithinRange: scoresWithinRange,
            CenterWithinBound: centerWithinBound,
            CorrectionWithinBound: correctionWithinBound,
            CostWithinBound: costWithinBound,
            HumilityWithinBound: humilityWithinBound,
            JointlySatisfiable: jointlySatisfiable,
            NoPressureSilentlyDegraded: noPressureSilentlyDegraded,
            SymmetricJustificationPresent: symmetricJustificationPresent,
            DeferWithTension: deferWithTension,
            PreserveAndReviseJudgedUnderSameLaw: symmetricJustificationPresent,
            ClaimPreservedSolelyForCoherenceRefused: request.PreserveClaimSolelyForCoherence,
            ScopeExpansionWithoutUncertaintyRefused: request.ScopeExpandedWithoutUncertaintyBounds,
            CmeMintingWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            CandidateOnly: true,
            JustificationTraceHandles: justificationTraceHandles,
            MaintenanceJustificationHandles: maintenanceJustificationHandles,
            RevisionJustificationHandles: revisionJustificationHandles,
            ContradictionReceiptHandles: contradictionReceiptHandles,
            ConstraintCodes: DetermineConstraintCodes(
                orientationAttested,
                scoresWithinRange,
                centerWithinBound,
                correctionWithinBound,
                costWithinBound,
                humilityWithinBound,
                jointlySatisfiable,
                symmetricJustificationPresent,
                noPressureSilentlyDegraded,
                request.PreserveClaimSolelyForCoherence,
                request.ScopeExpandedWithoutUncertaintyBounds,
                regionKind),
            ReasonCode: DetermineReasonCode(
                orientationAttested,
                scoresWithinRange,
                centerWithinBound,
                correctionWithinBound,
                costWithinBound,
                humilityWithinBound,
                symmetricJustificationPresent,
                noPressureSilentlyDegraded,
                request.PreserveClaimSolelyForCoherence,
                request.ScopeExpandedWithoutUncertaintyBounds,
                regionKind),
            LawfulBasis: DetermineLawfulBasis(regionKind),
            TimestampUtc: MaxTimestamp(
                request.TimestampUtc,
                request.OrientationReceipt.TimestampUtc,
                request.PriorState.TimestampUtc,
                request.ProposedState.TimestampUtc));
    }

    private static CmeTruthSeekingBalanceRegionKind DetermineRegionKind(
        bool orientationAttested,
        bool scoresWithinRange,
        bool centerWithinBound,
        bool correctionWithinBound,
        bool costWithinBound,
        bool humilityWithinBound,
        bool symmetricJustificationPresent,
        bool noPressureSilentlyDegraded,
        bool preserveClaimSolelyForCoherence,
        bool scopeExpandedWithoutUncertaintyBounds,
        CmeTruthSeekingOrientationStateRecord proposedState)
    {
        if (!scoresWithinRange ||
            preserveClaimSolelyForCoherence ||
            scopeExpandedWithoutUncertaintyBounds)
        {
            return CmeTruthSeekingBalanceRegionKind.BalanceRefused;
        }

        if (!costWithinBound)
        {
            return CmeTruthSeekingBalanceRegionKind.WeightlessRegion;
        }

        if (!humilityWithinBound)
        {
            return CmeTruthSeekingBalanceRegionKind.OverclaimRegion;
        }

        if (!centerWithinBound ||
            proposedState.EvidenceSupportScore - proposedState.CenterCoherenceScore >= DominanceMargin)
        {
            return CmeTruthSeekingBalanceRegionKind.DriftRegion;
        }

        if (!correctionWithinBound ||
            proposedState.CenterCoherenceScore - proposedState.EvidenceSupportScore >= DominanceMargin)
        {
            return CmeTruthSeekingBalanceRegionKind.FixationRegion;
        }

        if (!orientationAttested ||
            !symmetricJustificationPresent ||
            !noPressureSilentlyDegraded)
        {
            return CmeTruthSeekingBalanceRegionKind.DeferWithTension;
        }

        return CmeTruthSeekingBalanceRegionKind.JointlySatisfiable;
    }

    private static CmeTruthSeekingBalanceDispositionKind DetermineDisposition(
        CmeTruthSeekingBalanceRegionKind regionKind)
    {
        return regionKind switch
        {
            CmeTruthSeekingBalanceRegionKind.JointlySatisfiable =>
                CmeTruthSeekingBalanceDispositionKind.Admissible,
            CmeTruthSeekingBalanceRegionKind.WeightlessRegion or
            CmeTruthSeekingBalanceRegionKind.OverclaimRegion or
            CmeTruthSeekingBalanceRegionKind.BalanceRefused =>
                CmeTruthSeekingBalanceDispositionKind.Refused,
            _ =>
                CmeTruthSeekingBalanceDispositionKind.Deferred
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        bool orientationAttested,
        bool scoresWithinRange,
        bool centerWithinBound,
        bool correctionWithinBound,
        bool costWithinBound,
        bool humilityWithinBound,
        bool jointlySatisfiable,
        bool symmetricJustificationPresent,
        bool noPressureSilentlyDegraded,
        bool preserveClaimSolelyForCoherence,
        bool scopeExpandedWithoutUncertaintyBounds,
        CmeTruthSeekingBalanceRegionKind regionKind)
    {
        var constraints = new List<string>
        {
            "cme-truth-balance-orientation-required",
            "cme-truth-balance-center-bound-required",
            "cme-truth-balance-correction-bound-required",
            "cme-truth-balance-cost-bound-required",
            "cme-truth-balance-humility-bound-required",
            "cme-truth-balance-symmetric-justification-required",
            "cme-truth-balance-no-silent-pressure-degradation",
            "cme-truth-balance-no-identity-coherence-lock-in",
            "cme-truth-balance-minting-withheld",
            "cme-truth-balance-role-enactment-withheld",
            "cme-truth-balance-action-authority-withheld"
        };

        AddMissingConstraint(constraints, orientationAttested, "orientation");
        AddMissingConstraint(constraints, scoresWithinRange, "bounded-scores");
        AddMissingConstraint(constraints, centerWithinBound, "center");
        AddMissingConstraint(constraints, correctionWithinBound, "correction");
        AddMissingConstraint(constraints, costWithinBound, "cost");
        AddMissingConstraint(constraints, humilityWithinBound, "humility");
        AddMissingConstraint(constraints, symmetricJustificationPresent, "symmetric-justification");
        AddMissingConstraint(constraints, noPressureSilentlyDegraded, "silent-pressure-degradation");

        if (preserveClaimSolelyForCoherence)
        {
            constraints.Add("cme-truth-balance-identity-coherence-preservation-refused");
        }

        if (scopeExpandedWithoutUncertaintyBounds)
        {
            constraints.Add("cme-truth-balance-overclaim-scope-expansion-refused");
        }

        constraints.Add($"cme-truth-balance-region-{ToReasonToken(regionKind)}");
        constraints.Add(jointlySatisfiable
            ? "cme-truth-balance-jointly-satisfiable"
            : "cme-truth-balance-not-jointly-satisfiable");

        return constraints;
    }

    private static string DetermineReasonCode(
        bool orientationAttested,
        bool scoresWithinRange,
        bool centerWithinBound,
        bool correctionWithinBound,
        bool costWithinBound,
        bool humilityWithinBound,
        bool symmetricJustificationPresent,
        bool noPressureSilentlyDegraded,
        bool preserveClaimSolelyForCoherence,
        bool scopeExpandedWithoutUncertaintyBounds,
        CmeTruthSeekingBalanceRegionKind regionKind)
    {
        if (!scoresWithinRange)
        {
            return "cme-truth-balance-bounded-scores-required";
        }

        if (preserveClaimSolelyForCoherence)
        {
            return "cme-truth-balance-identity-coherence-preservation-refused";
        }

        if (scopeExpandedWithoutUncertaintyBounds)
        {
            return "cme-truth-balance-overclaim-scope-expansion-refused";
        }

        if (!costWithinBound)
        {
            return "cme-truth-balance-weightless-region";
        }

        if (!humilityWithinBound)
        {
            return "cme-truth-balance-overclaim-region";
        }

        if (!centerWithinBound)
        {
            return "cme-truth-balance-drift-region";
        }

        if (!correctionWithinBound)
        {
            return "cme-truth-balance-fixation-region";
        }

        if (!orientationAttested)
        {
            return "cme-truth-balance-orientation-not-attested";
        }

        if (!symmetricJustificationPresent)
        {
            return "cme-truth-balance-symmetric-justification-missing";
        }

        if (!noPressureSilentlyDegraded)
        {
            return "cme-truth-balance-silent-pressure-degradation";
        }

        return regionKind == CmeTruthSeekingBalanceRegionKind.JointlySatisfiable
            ? "cme-truth-balance-jointly-satisfiable"
            : $"cme-truth-balance-{ToReasonToken(regionKind)}";
    }

    private static string DetermineLawfulBasis(
        CmeTruthSeekingBalanceRegionKind regionKind)
    {
        return regionKind switch
        {
            CmeTruthSeekingBalanceRegionKind.JointlySatisfiable =>
                "center, correction, cost, and humility remain jointly satisfiable under this transition, with preservation and revision judged by the same law.",
            CmeTruthSeekingBalanceRegionKind.FixationRegion =>
                "truth balance must defer when center dominates correction and valid revision pressure risks being suppressed.",
            CmeTruthSeekingBalanceRegionKind.DriftRegion =>
                "truth balance must defer when correction dominates center and continuity risks being shed without stable accumulation.",
            CmeTruthSeekingBalanceRegionKind.WeightlessRegion =>
                "truth balance must refuse claim updates that lack cost accounting.",
            CmeTruthSeekingBalanceRegionKind.OverclaimRegion =>
                "truth balance must refuse scope expansion or claim transition without uncertainty bounds.",
            CmeTruthSeekingBalanceRegionKind.BalanceRefused =>
                "truth balance must refuse invalid scores, identity-coherence lock-in, or unbounded overclaim.",
            _ =>
                "truth balance must defer with tension when pressures cannot yet be jointly satisfied without silent degradation."
        };
    }

    private static bool PressureDegradedBelowBound(
        CmeTruthSeekingOrientationStateRecord priorState,
        CmeTruthSeekingOrientationStateRecord proposedState)
    {
        return (priorState.CenterCoherenceScore >= MinimumPressureScore &&
                proposedState.CenterCoherenceScore < MinimumPressureScore) ||
               (priorState.EvidenceSupportScore >= MinimumPressureScore &&
                proposedState.EvidenceSupportScore < MinimumPressureScore) ||
               (priorState.MaintenanceCostScore >= MinimumPressureScore &&
                proposedState.MaintenanceCostScore < MinimumPressureScore) ||
               (priorState.UncertaintyBoundScore >= MinimumPressureScore &&
                proposedState.UncertaintyBoundScore < MinimumPressureScore);
    }

    private static bool ScoresWithinRange(
        CmeTruthSeekingOrientationStateRecord state)
    {
        return IsBoundedScore(state.CenterCoherenceScore) &&
               IsBoundedScore(state.EvidenceSupportScore) &&
               IsBoundedScore(state.MaintenanceCostScore) &&
               IsBoundedScore(state.UncertaintyBoundScore);
    }

    private static bool IsBoundedScore(decimal score)
        => score is >= 0.0m and <= 1.0m;

    private static void AddMissingConstraint(
        ICollection<string> constraints,
        bool present,
        string name)
    {
        if (!present)
        {
            constraints.Add($"cme-truth-balance-{name}-missing");
        }
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        return (tokens ?? Array.Empty<string>())
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeHandle(string? handle)
        => string.IsNullOrWhiteSpace(handle) ? string.Empty : handle.Trim();

    private static string ToReasonToken(
        CmeTruthSeekingBalanceRegionKind regionKind)
    {
        return regionKind switch
        {
            CmeTruthSeekingBalanceRegionKind.JointlySatisfiable => "jointly-satisfiable",
            CmeTruthSeekingBalanceRegionKind.FixationRegion => "fixation-region",
            CmeTruthSeekingBalanceRegionKind.DriftRegion => "drift-region",
            CmeTruthSeekingBalanceRegionKind.WeightlessRegion => "weightless-region",
            CmeTruthSeekingBalanceRegionKind.OverclaimRegion => "overclaim-region",
            CmeTruthSeekingBalanceRegionKind.BalanceRefused => "balance-refused",
            _ => "defer-with-tension"
        };
    }

    private static DateTimeOffset MaxTimestamp(
        params DateTimeOffset[] timestamps)
    {
        return timestamps.Max();
    }
}
