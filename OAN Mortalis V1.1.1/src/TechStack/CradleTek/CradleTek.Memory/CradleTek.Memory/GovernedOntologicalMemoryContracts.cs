namespace CradleTek.Memory;

public enum GovernedOntologicalCleaverClassification
{
    Known = 0,
    PartiallyKnown = 1,
    Unknown = 2
}

public sealed record GovernedOntologicalTokenResolution(
    string Token,
    string NormalizedToken,
    GovernedOntologicalCleaverClassification Classification,
    GovernedRootEngram? RootEngram,
    string ResolutionReason);

public sealed record GovernedOntologicalCleaverMetrics(
    double KnownRatio,
    double PartiallyKnownRatio,
    double UnknownRatio,
    string ConceptDensity,
    string ContextStability);

public sealed record GovernedOntologicalCleaverResult(
    string InputText,
    IReadOnlyList<GovernedOntologicalTokenResolution> Resolutions,
    IReadOnlyList<GovernedRootEngram> Known,
    IReadOnlyList<GovernedRootEngram> PartiallyKnown,
    IReadOnlyList<string> Unknown,
    GovernedOntologicalCleaverMetrics Metrics,
    GovernedRootAtlasSnapshot CanonicalRootAtlas);

public interface IGovernedRootOntologicalCleaver
{
    Task<GovernedOntologicalCleaverResult> CleaveAsync(string inputText, CancellationToken cancellationToken = default);
}
