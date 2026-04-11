using San.Common;

namespace SLI.Runtime;

public enum RecompositionCandidateClass
{
    FieldRecall = 0,
    FieldBlend = 1,
    TensionPreservingMerge = 2,
    WitnessOnlyProjection = 3
}

public enum RecompositionCandidateDisposition
{
    Withheld = 0,
    CandidateOnly = 1
}

public sealed record RecompositionCandidateProvenance(
    string ProductId,
    string ReceiptId,
    string WitnessSnapshotId,
    string SourceTraceId,
    MembraneDispatchLane Lane,
    SymbolicProductFamily Family,
    SymbolicProductClass ProductClass,
    AdmissibilityStatus Admissibility,
    ContradictionState ContradictionState,
    DateTimeOffset ReceivedAtUtc);

public sealed record RecompositionCandidate(
    string CandidateId,
    string QueryId,
    RecompositionCandidateClass CandidateClass,
    RecompositionCandidateDisposition Disposition,
    IReadOnlyList<RecompositionCandidateProvenance> Sources,
    QueryTensionSummary TensionSummary,
    AdmissibilityStatus Admissibility,
    ContradictionState ContradictionState,
    MaterializationEligibility MaterializationEligibility,
    PersistenceEligibility PersistenceEligibility,
    bool RequiresMembraneReentry,
    DateTimeOffset CreatedAtUtc);
