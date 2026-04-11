using San.Common;

namespace SLI.Runtime;

public interface IRecompositionCandidateEvaluationPolicy
{
    RecompositionCandidateEvaluationDecision Evaluate(RecompositionCandidateEvaluationRequest request);
}

public static class CandidateEvaluationOutcomeCodes
{
    public const string CandidateRetained = "candidate-retained";
    public const string CandidateCleaved = "candidate-cleaved";
    public const string CandidateDeferred = "candidate-deferred";
    public const string CandidateRejected = "candidate-rejected";
}

public sealed class DefaultRecompositionCandidateEvaluationPolicy : IRecompositionCandidateEvaluationPolicy
{
    public RecompositionCandidateEvaluationDecision Evaluate(RecompositionCandidateEvaluationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Candidate);

        if (string.IsNullOrWhiteSpace(request.Candidate.CandidateId))
        {
            throw new InvalidOperationException("candidate evaluation requires an explicit candidate id.");
        }

        if (string.IsNullOrWhiteSpace(request.EvaluationContext))
        {
            throw new InvalidOperationException("candidate evaluation requires an explicit evaluation context.");
        }

        if (string.IsNullOrWhiteSpace(request.Office))
        {
            throw new InvalidOperationException("candidate evaluation requires an explicit office.");
        }

        var burdenEvaluations = BuildBurdenEvaluations(request.Candidate);
        var outcome = DetermineOutcome(request.Candidate, burdenEvaluations);
        var recoveryClass = DetermineRecoveryClass(outcome, burdenEvaluations);

        var eligibleForLaterOperatorRealization = outcome == CandidateEvaluationOutcome.RetainCandidate;
        var requiresFurtherCleaveReview =
            outcome == CandidateEvaluationOutcome.CleaveCandidate ||
            request.Candidate.ContradictionState == ContradictionState.Hard;

        return new RecompositionCandidateEvaluationDecision(
            Outcome: outcome,
            RecoveryClass: recoveryClass,
            BurdenEvaluations: burdenEvaluations,
            OutcomeCode: GetOutcomeCode(outcome),
            GovernanceTrace: $"candidate-eval://{request.Office}/{request.Candidate.CandidateId}",
            EligibleForLaterOperatorRealization: eligibleForLaterOperatorRealization,
            RequiresFurtherCleaveReview: requiresFurtherCleaveReview,
            EvaluatedAtUtc: request.RequestedAtUtc);
    }

    private static CandidateBurdenEvaluation[] BuildBurdenEvaluations(RecompositionCandidate candidate)
    {
        var hasSources = candidate.Sources.Count > 0;
        var multipleSources = candidate.Sources.Count > 1;
        var hardContradiction = candidate.ContradictionState == ContradictionState.Hard;
        var softContradiction = candidate.ContradictionState == ContradictionState.Soft;
        var anyRefusedSource = candidate.Sources.Any(static source => source.Admissibility == AdmissibilityStatus.Refused);
        var anyCollapsedSource = candidate.Sources.Any(static source => source.Lane == MembraneDispatchLane.Collapsed);
        var uniqueFamilies = candidate.Sources.Select(static source => source.Family).Distinct().Count();
        var uniqueTraces = candidate.Sources.Select(static source => source.SourceTraceId).Where(static trace => !string.IsNullOrWhiteSpace(trace)).Distinct().Count();
        var missingTrace = candidate.Sources.Any(static source => string.IsNullOrWhiteSpace(source.SourceTraceId));

        return
        [
            new CandidateBurdenEvaluation(
                Axis: CandidateEvaluationBurdenAxis.Constitutional,
                Result: !hasSources || anyRefusedSource || anyCollapsedSource
                    ? CandidateEvaluationBurdenResult.Failed
                    : CandidateEvaluationBurdenResult.Preserved,
                Rationale: !hasSources
                    ? "candidate has no lawful sources and therefore cannot preserve constitutional standing."
                    : anyRefusedSource || anyCollapsedSource
                        ? "candidate contains refused or collapsed source matter and may not preserve constitutional burden intact."
                        : "candidate sources remain within bounded candidate posture and preserve constitutional burden for later review."),

            new CandidateBurdenEvaluation(
                Axis: CandidateEvaluationBurdenAxis.Body,
                Result: !hasSources
                    ? CandidateEvaluationBurdenResult.Failed
                    : multipleSources || candidate.CandidateClass != RecompositionCandidateClass.FieldRecall
                        ? CandidateEvaluationBurdenResult.BoundedDelta
                        : CandidateEvaluationBurdenResult.Preserved,
                Rationale: !hasSources
                    ? "candidate without sources cannot preserve body burden."
                    : multipleSources || candidate.CandidateClass != RecompositionCandidateClass.FieldRecall
                        ? "candidate body burden remains bounded but no longer reads as exact single-source recall."
                        : "single-source field recall preserves body burden without widening form."),

            new CandidateBurdenEvaluation(
                Axis: CandidateEvaluationBurdenAxis.Relational,
                Result: !hasSources
                    ? CandidateEvaluationBurdenResult.Failed
                    : multipleSources
                        ? CandidateEvaluationBurdenResult.BoundedDelta
                        : CandidateEvaluationBurdenResult.Preserved,
                Rationale: !hasSources
                    ? "candidate without lawful sources cannot preserve relational burden."
                    : multipleSources
                        ? "multiple lawful sources require bounded relational review before stronger standing may be claimed."
                        : "single-source candidate preserves relational burden without requiring braid expansion."),

            new CandidateBurdenEvaluation(
                Axis: CandidateEvaluationBurdenAxis.Cognitive,
                Result: hardContradiction
                    ? CandidateEvaluationBurdenResult.Failed
                    : softContradiction || uniqueFamilies > 1
                        ? CandidateEvaluationBurdenResult.BoundedDelta
                        : CandidateEvaluationBurdenResult.Preserved,
                Rationale: hardContradiction
                    ? "hard contradiction remains visible and candidate evaluation may not normalize it into cognitive coherence."
                    : softContradiction || uniqueFamilies > 1
                        ? "candidate remains cognitively reviewable but carries bounded tension that must stay explicit."
                        : "candidate preserves cognitive burden without visible contradiction drift."),

            new CandidateBurdenEvaluation(
                Axis: CandidateEvaluationBurdenAxis.Continuity,
                Result: !hasSources || missingTrace
                    ? CandidateEvaluationBurdenResult.Failed
                    : uniqueTraces > 1
                        ? CandidateEvaluationBurdenResult.BoundedDelta
                        : CandidateEvaluationBurdenResult.Preserved,
                Rationale: !hasSources || missingTrace
                    ? "continuity burden fails when source trace lineage is absent."
                    : uniqueTraces > 1
                        ? "multiple lawful traces require continuity-equivalent review rather than exact identity carry-forward."
                        : "single-source lineage preserves continuity burden for candidate review.")
        ];
    }

    private static CandidateEvaluationOutcome DetermineOutcome(
        RecompositionCandidate candidate,
        IReadOnlyList<CandidateBurdenEvaluation> burdenEvaluations)
    {
        if (candidate.Disposition != RecompositionCandidateDisposition.CandidateOnly ||
            candidate.Sources.Count == 0)
        {
            return CandidateEvaluationOutcome.RejectCandidate;
        }

        var constitutionalFailed = burdenEvaluations.Any(static evaluation =>
            evaluation.Axis == CandidateEvaluationBurdenAxis.Constitutional &&
            evaluation.Result == CandidateEvaluationBurdenResult.Failed);

        var continuityFailed = burdenEvaluations.Any(static evaluation =>
            evaluation.Axis == CandidateEvaluationBurdenAxis.Continuity &&
            evaluation.Result == CandidateEvaluationBurdenResult.Failed);

        if (constitutionalFailed || continuityFailed)
        {
            return candidate.Sources.Count > 1
                ? CandidateEvaluationOutcome.CleaveCandidate
                : CandidateEvaluationOutcome.RejectCandidate;
        }

        if (candidate.ContradictionState == ContradictionState.Hard)
        {
            return candidate.Sources.Count > 1
                ? CandidateEvaluationOutcome.CleaveCandidate
                : CandidateEvaluationOutcome.DeferCandidate;
        }

        var anyBoundedDelta = burdenEvaluations.Any(static evaluation =>
            evaluation.Result == CandidateEvaluationBurdenResult.BoundedDelta);

        if (candidate.ContradictionState == ContradictionState.Soft || anyBoundedDelta)
        {
            return CandidateEvaluationOutcome.DeferCandidate;
        }

        return CandidateEvaluationOutcome.RetainCandidate;
    }

    private static CandidateRecoveryClass DetermineRecoveryClass(
        CandidateEvaluationOutcome outcome,
        IReadOnlyList<CandidateBurdenEvaluation> burdenEvaluations)
    {
        if (outcome == CandidateEvaluationOutcome.RejectCandidate)
        {
            return CandidateRecoveryClass.Failure;
        }

        if (outcome == CandidateEvaluationOutcome.CleaveCandidate)
        {
            return CandidateRecoveryClass.ContinuityEquivalentRecovery;
        }

        var anyBoundedDelta = burdenEvaluations.Any(static evaluation =>
            evaluation.Result == CandidateEvaluationBurdenResult.BoundedDelta);

        return anyBoundedDelta
            ? CandidateRecoveryClass.LawfulIsomorphicRecovery
            : CandidateRecoveryClass.ExactIdentityRecovery;
    }

    private static string GetOutcomeCode(CandidateEvaluationOutcome outcome)
    {
        return outcome switch
        {
            CandidateEvaluationOutcome.RetainCandidate => CandidateEvaluationOutcomeCodes.CandidateRetained,
            CandidateEvaluationOutcome.CleaveCandidate => CandidateEvaluationOutcomeCodes.CandidateCleaved,
            CandidateEvaluationOutcome.DeferCandidate => CandidateEvaluationOutcomeCodes.CandidateDeferred,
            CandidateEvaluationOutcome.RejectCandidate => CandidateEvaluationOutcomeCodes.CandidateRejected,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "unsupported candidate evaluation outcome.")
        };
    }
}
