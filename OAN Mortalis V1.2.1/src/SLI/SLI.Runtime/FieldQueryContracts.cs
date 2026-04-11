using San.Common;

namespace SLI.Runtime;

public enum FieldQueryAxis
{
    Family = 0,
    ProductClass = 1,
    Intent = 2,
    Admissibility = 3,
    Contradiction = 4,
    TemporalWindow = 5,
    TraceLineage = 6,
    LaneScope = 7,
    Origin = 8
}

public sealed record FieldTemporalWindow(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);

public sealed record FieldQuery(
    string QueryId,
    string RequestedByTraceId,
    IReadOnlyList<FieldQueryAxis> Axes,
    SymbolicProductFamily? Family,
    SymbolicProductClass? ProductClass,
    SymbolicIntent? Intent,
    AdmissibilityStatus? Admissibility,
    ContradictionState? ContradictionState,
    MembraneDispatchLane? LaneScope,
    string? Origin,
    string? TraceLineagePrefix,
    FieldTemporalWindow? TemporalWindow,
    DateTimeOffset RequestedAtUtc);

public sealed record FieldProductSnapshot(
    string ProductId,
    SymbolicEnvelope Envelope,
    MembraneDispatchLane Lane,
    string ReceiptId,
    string WitnessSnapshotId,
    DateTimeOffset ReceivedAtUtc);

public sealed record FieldQueryMatch(
    FieldProductSnapshot Product,
    IReadOnlyList<FieldQueryAxis> MatchedAxes,
    bool PassportTruthPreserved,
    string RetrievalTrace);

public sealed record FieldQueryResult(
    FieldQuery Query,
    IReadOnlyList<FieldQueryMatch> Matches,
    QueryTensionSummary TensionSummary,
    bool MembraneReentryRequired,
    DateTimeOffset EvaluatedAtUtc);
