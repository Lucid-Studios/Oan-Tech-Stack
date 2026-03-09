using GEL.Models;

namespace Oan.Common;

public enum CrypticOriginRuntime
{
    OracleCSharp,
    Lisp
}

public enum CrypticOriginLane
{
    Sentence,
    ParagraphGraph,
    ParagraphBody
}

public enum CrypticFormationOutcome
{
    Closed,
    NeedsSpecification,
    Rejected,
    OutOfScope
}

public enum CrypticAdmissionDecision
{
    Admit,
    Defer,
    Quarantine,
    Reject
}

public sealed record CrypticAdmissionCandidate(
    Guid CandidateId,
    CrypticOriginRuntime OriginRuntime,
    CrypticOriginLane OriginLane,
    string SourceText,
    object? MaterializedPayload,
    EngramDraft? CandidateDraft,
    CrypticFormationOutcome Outcome,
    bool DeterministicPrimeMaterializationSucceeded,
    bool ReservedDomainViolation,
    string? DiagnosticRender,
    IReadOnlyList<string> TelemetryTags);

public sealed record PrimeClosureSubmission(
    Guid CandidateId,
    CrypticOriginRuntime OriginRuntime,
    CrypticOriginLane OriginLane,
    EngramDraft EngramDraft,
    IReadOnlyDictionary<string, string> PrimeMetadata);

public sealed record CrypticAdmissionResult(
    CrypticAdmissionDecision Decision,
    string ReasonCode,
    Guid CandidateId,
    CrypticOriginRuntime OriginRuntime,
    CrypticOriginLane OriginLane,
    bool SubmissionEligible,
    bool RequiresReview,
    IReadOnlyList<string> TelemetryTags,
    PrimeClosureSubmission? NormalizedPrimePayload);

public interface ICrypticAdmissionMembrane
{
    Task<CrypticAdmissionResult> EvaluateAsync(
        CrypticAdmissionCandidate candidate,
        CancellationToken cancellationToken = default);
}
