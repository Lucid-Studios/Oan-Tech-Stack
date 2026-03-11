namespace GEL.Models;

public enum PropositionRole
{
    Subject = 0,
    Predicate = 1,
    Object = 2
}

public sealed class PropositionTerm
{
    public required PropositionRole Role { get; init; }
    public required string RootKey { get; init; }
    public required string SymbolicHandle { get; init; }
}

public sealed class PropositionQualifier
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public sealed class PropositionContextTag
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public enum PropositionalCompileGrade
{
    Stable = 0,
    NeedsSpecification = 1,
    Rejected = 2
}

public sealed class PropositionalCompileCandidate
{
    public required PropositionTerm Subject { get; init; }
    public required string PredicateRoot { get; init; }
    public required PropositionTerm Object { get; init; }
    public required IReadOnlyList<PropositionQualifier> Qualifiers { get; init; }
    public required IReadOnlyList<PropositionContextTag> ContextTags { get; init; }
    public required string DiagnosticPropositionRender { get; init; }
    public required IReadOnlyList<string> UnresolvedTensions { get; init; }
}

public sealed class PropositionalCompileAssessment
{
    public required PropositionalCompileCandidate Candidate { get; init; }
    public required PropositionalCompileGrade Grade { get; init; }
    public required IReadOnlyList<string> ReasonCodes { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public EngramDraft? ProjectedEngramDraft { get; init; }
}
