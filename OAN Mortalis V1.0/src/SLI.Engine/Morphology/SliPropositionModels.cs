namespace SLI.Engine.Morphology;

internal enum SliPropositionalCompileGrade
{
    Stable,
    NeedsSpecification,
    Rejected
}

internal sealed class SliPropositionTermResult
{
    public string RootKey { get; set; } = string.Empty;
    public string SymbolicHandle { get; set; } = string.Empty;
}

internal sealed class SliPropositionQualifierResult
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

internal sealed class SliPropositionContextTagResult
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

internal sealed class SliPropositionalCompileResult
{
    public required SliPropositionTermResult Subject { get; init; }
    public required string PredicateRoot { get; init; }
    public required SliPropositionTermResult Object { get; init; }
    public required IReadOnlyList<SliPropositionQualifierResult> Qualifiers { get; init; }
    public required IReadOnlyList<SliPropositionContextTagResult> ContextTags { get; init; }
    public required string DiagnosticRender { get; init; }
    public required IReadOnlyList<string> UnresolvedTensions { get; init; }
    public required SliPropositionalCompileGrade Grade { get; init; }
}

internal sealed class SliPropositionState
{
    public SliPropositionTermResult Subject { get; } = new();
    public string PredicateRoot { get; set; } = string.Empty;
    public SliPropositionTermResult Object { get; } = new();
    public List<SliPropositionQualifierResult> Qualifiers { get; } = [];
    public List<SliPropositionContextTagResult> ContextTags { get; } = [];
    public string DiagnosticRender { get; set; } = string.Empty;
    public List<string> UnresolvedTensions { get; } = [];
    public string Grade { get; set; } = SliPropositionalCompileGrade.NeedsSpecification.ToString();
}
