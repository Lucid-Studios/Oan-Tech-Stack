using GEL.Models;

namespace CradleTek.Memory.Models;

public sealed class AtlasSourceNormalizationResult
{
    public required RootAtlas? CanonicalRootAtlas { get; init; }
    public required IReadOnlyList<AtlasSourceNormalizationDiagnostic> Diagnostics { get; init; }
    public required IReadOnlyDictionary<string, string> RootSymbols { get; init; }
    public required IReadOnlyDictionary<string, string> PrefixSymbols { get; init; }
    public required IReadOnlyDictionary<string, string> SuffixSymbols { get; init; }

    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == AtlasSourceDiagnosticSeverity.Error);
}
