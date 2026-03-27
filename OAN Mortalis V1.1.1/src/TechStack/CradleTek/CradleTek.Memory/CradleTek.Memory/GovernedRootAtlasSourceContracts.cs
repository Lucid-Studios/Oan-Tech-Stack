namespace CradleTek.Memory;

public enum GovernedAtlasSourceDiagnosticSeverity
{
    Warning = 0,
    Error = 1
}

public sealed record GovernedAtlasSourceDiagnostic(
    GovernedAtlasSourceDiagnosticSeverity Severity,
    string Code,
    string SourceLayer,
    string Message,
    string? RootKey);

public sealed record GovernedRootEngram(
    string SymbolicId,
    string AtlasDomain,
    string RootTerm,
    IReadOnlyList<string> VariantForms,
    double FrequencyWeight,
    string DictionaryPointer);

public sealed record GovernedRootAtlasEntry(
    string RootKey,
    GovernedRootEngram RootEngram,
    IReadOnlyList<string> VariantForms,
    IReadOnlyList<string> SymbolicConstructors);

public sealed record GovernedRootAtlasSnapshot(
    string Source,
    string SnapshotProfile,
    IReadOnlyList<GovernedRootAtlasEntry> Entries,
    IReadOnlyDictionary<string, string> RootSymbols,
    IReadOnlyDictionary<string, string> PrefixSymbols,
    IReadOnlyDictionary<string, string> SuffixSymbols,
    IReadOnlyList<GovernedAtlasSourceDiagnostic> Diagnostics)
{
    public bool HasErrors =>
        Diagnostics.Any(diagnostic => diagnostic.Severity == GovernedAtlasSourceDiagnosticSeverity.Error);
}

public interface IGovernedRootAtlasSource
{
    ValueTask<GovernedRootAtlasSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed class InMemoryRootAtlasSource : IGovernedRootAtlasSource
{
    private readonly GovernedRootAtlasSnapshot _snapshot;

    public InMemoryRootAtlasSource(GovernedRootAtlasSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public ValueTask<GovernedRootAtlasSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_snapshot);
    }
}
