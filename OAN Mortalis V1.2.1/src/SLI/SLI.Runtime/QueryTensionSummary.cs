namespace SLI.Runtime;

public enum QueryTensionState
{
    Stable = 0,
    Narrowed = 1,
    Contradicted = 2,
    Withheld = 3
}

public sealed record QueryTensionNote(
    string Code,
    string Message);

public sealed record QueryTensionSummary(
    IReadOnlyList<FieldQueryAxis> ActiveAxes,
    QueryTensionState TensionState,
    int SourceCount,
    int MatchCount,
    int WithheldCount,
    bool PassportTruthPreserved,
    bool AuthorityCeilingPreserved,
    bool MembraneReentryRequired,
    IReadOnlyList<QueryTensionNote> Notes);
