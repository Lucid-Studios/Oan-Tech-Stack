using Oan.Spinal;

namespace GEL.Models;

public enum EngramClosureGrade
{
    BootstrapClosed,
    Closed,
    NeedsSpecification,
    Rejected
}

public sealed class EngramTrunk
{
    public required IReadOnlyList<string> Segments { get; init; }
    public string? Summary { get; init; }
}

public sealed class EngramBranch
{
    public required string Name { get; init; }
    public string? RootKey { get; init; }
    public EngramId? ReferencedEngramId { get; init; }
    public string? SymbolicHandle { get; init; }
}

public sealed class EngramInvariant
{
    public required string Key { get; init; }
    public required string Statement { get; init; }
}

public sealed class EngramDraft
{
    public EngramId? ProposedId { get; init; }
    public required string RootKey { get; init; }
    public EngramEpistemicClass? EpistemicClass { get; init; }
    public required EngramTrunk Trunk { get; init; }
    public required IReadOnlyList<EngramBranch> Branches { get; init; }
    public required IReadOnlyList<EngramInvariant> Invariants { get; init; }
    public EngramClosureGrade RequestedClosureGrade { get; init; } = EngramClosureGrade.BootstrapClosed;
}

public sealed class Engram
{
    public required EngramId Id { get; init; }
    public required string AtlasVersion { get; init; }
    public required PredicateRoot Root { get; init; }
    public required EngramEpistemicClass EpistemicClass { get; init; }
    public required EngramTrunk Trunk { get; init; }
    public required IReadOnlyList<EngramBranch> Branches { get; init; }
    public required IReadOnlyList<EngramInvariant> Invariants { get; init; }
}

public sealed class EngramClosureDecision
{
    public required EngramClosureGrade Grade { get; init; }
    public EngramId? NormalizedId { get; init; }
    public Engram? CanonicalEngram { get; init; }
    public required IReadOnlyList<string> ReasonCodes { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }

    public bool IsSuccess =>
        Grade is EngramClosureGrade.BootstrapClosed or EngramClosureGrade.Closed;
}
