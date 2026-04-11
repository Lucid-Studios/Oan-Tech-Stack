namespace SLI.Runtime;

public enum CandidateEvaluationBurdenAxis
{
    Constitutional = 0,
    Body = 1,
    Relational = 2,
    Cognitive = 3,
    Continuity = 4
}

public enum CandidateEvaluationBurdenResult
{
    Preserved = 0,
    BoundedDelta = 1,
    Failed = 2
}

public enum CandidateRecoveryClass
{
    ExactIdentityRecovery = 0,
    LawfulIsomorphicRecovery = 1,
    ContinuityEquivalentRecovery = 2,
    Failure = 3
}

public enum CandidateEvaluationOutcome
{
    RetainCandidate = 0,
    CleaveCandidate = 1,
    DeferCandidate = 2,
    RejectCandidate = 3
}

public sealed record CandidateBurdenEvaluation(
    CandidateEvaluationBurdenAxis Axis,
    CandidateEvaluationBurdenResult Result,
    string Rationale);

public sealed record RecompositionCandidateEvaluationRequest(
    RecompositionCandidate Candidate,
    string EvaluationContext,
    string Office,
    DateTimeOffset RequestedAtUtc);

public sealed record RecompositionCandidateEvaluationDecision(
    CandidateEvaluationOutcome Outcome,
    CandidateRecoveryClass RecoveryClass,
    IReadOnlyList<CandidateBurdenEvaluation> BurdenEvaluations,
    string OutcomeCode,
    string GovernanceTrace,
    bool EligibleForLaterOperatorRealization,
    bool RequiresFurtherCleaveReview,
    DateTimeOffset EvaluatedAtUtc);
