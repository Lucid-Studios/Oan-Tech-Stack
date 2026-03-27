using SLI.Ingestion;

namespace CradleTek.Memory;

public sealed class GovernedRootOntologicalCleaver : IGovernedRootOntologicalCleaver
{
    private readonly IGovernedRootAtlasSource _rootAtlasSource;
    private readonly IGovernedOntologicalLexemeService _lexemeService;

    public GovernedRootOntologicalCleaver(
        IGovernedRootAtlasSource rootAtlasSource,
        IGovernedOntologicalLexemeService? lexemeService = null)
    {
        _rootAtlasSource = rootAtlasSource ?? throw new ArgumentNullException(nameof(rootAtlasSource));
        _lexemeService = lexemeService ?? new GovernedOntologicalLexemeService();
    }

    public async Task<GovernedOntologicalCleaverResult> CleaveAsync(
        string inputText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputText);
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = await _rootAtlasSource.LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
        var entriesByRoot = snapshot.Entries
            .GroupBy(entry => _lexemeService.NormalizeToken(entry.RootKey), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var entriesByVariant = snapshot.Entries
            .SelectMany(entry => entry.VariantForms
                .Append(entry.RootKey)
                .Select(variant => new { Key = _lexemeService.NormalizeToken(variant), Entry = entry }))
            .Where(static pair => !string.IsNullOrWhiteSpace(pair.Key))
            .GroupBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Entry, StringComparer.OrdinalIgnoreCase);

        var resolutions = new List<GovernedOntologicalTokenResolution>();
        var knownMap = new Dictionary<string, GovernedRootEngram>(StringComparer.Ordinal);
        var partiallyKnownMap = new Dictionary<string, GovernedRootEngram>(StringComparer.Ordinal);
        var unknown = new List<string>();
        var unknownSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in _lexemeService.Tokenize(inputText))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = _lexemeService.NormalizeToken(token);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (entriesByRoot.TryGetValue(normalized, out var exactRoot))
            {
                knownMap[exactRoot.RootEngram.SymbolicId] = exactRoot.RootEngram;
                resolutions.Add(new GovernedOntologicalTokenResolution(
                    Token: token,
                    NormalizedToken: normalized,
                    Classification: GovernedOntologicalCleaverClassification.Known,
                    RootEngram: exactRoot.RootEngram,
                    ResolutionReason: "exact-root"));
                continue;
            }

            if (entriesByVariant.TryGetValue(normalized, out var variantRoot))
            {
                partiallyKnownMap[variantRoot.RootEngram.SymbolicId] = variantRoot.RootEngram;
                resolutions.Add(new GovernedOntologicalTokenResolution(
                    Token: token,
                    NormalizedToken: normalized,
                    Classification: GovernedOntologicalCleaverClassification.PartiallyKnown,
                    RootEngram: variantRoot.RootEngram,
                    ResolutionReason: "variant-root"));
                continue;
            }

            if (_lexemeService.TryNormalizeMorphology(normalized, out var morpheme) &&
                entriesByVariant.TryGetValue(morpheme, out var morphemeRoot))
            {
                partiallyKnownMap[morphemeRoot.RootEngram.SymbolicId] = morphemeRoot.RootEngram;
                resolutions.Add(new GovernedOntologicalTokenResolution(
                    Token: token,
                    NormalizedToken: normalized,
                    Classification: GovernedOntologicalCleaverClassification.PartiallyKnown,
                    RootEngram: morphemeRoot.RootEngram,
                    ResolutionReason: "morph-normalized"));
                continue;
            }

            if (unknownSeen.Add(normalized))
            {
                unknown.Add(normalized);
            }

            resolutions.Add(new GovernedOntologicalTokenResolution(
                Token: token,
                NormalizedToken: normalized,
                Classification: GovernedOntologicalCleaverClassification.Unknown,
                RootEngram: null,
                ResolutionReason: "no-root-match"));
        }

        var total = resolutions.Count;
        var knownCount = resolutions.Count(static resolution => resolution.Classification == GovernedOntologicalCleaverClassification.Known);
        var partiallyKnownCount = resolutions.Count(static resolution => resolution.Classification == GovernedOntologicalCleaverClassification.PartiallyKnown);
        var unknownCount = resolutions.Count(static resolution => resolution.Classification == GovernedOntologicalCleaverClassification.Unknown);
        var knownRatio = total == 0 ? 0d : (double)knownCount / total;
        var partiallyKnownRatio = total == 0 ? 0d : (double)partiallyKnownCount / total;
        var unknownRatio = total == 0 ? 0d : (double)unknownCount / total;
        var recognizedRatio = total == 0 ? 0d : (double)(knownCount + partiallyKnownCount) / total;

        var conceptDensity = recognizedRatio switch
        {
            >= 0.75 => "high",
            >= 0.45 => "moderate",
            _ => "low"
        };

        var contextStability = (unknownRatio, partiallyKnownRatio) switch
        {
            (<= 0.10, <= 0.40) => "stable",
            (<= 0.35, <= 0.65) => "transitional",
            _ => "volatile"
        };

        return new GovernedOntologicalCleaverResult(
            InputText: inputText,
            Resolutions: resolutions,
            Known: knownMap.Values.OrderBy(static value => value.SymbolicId, StringComparer.Ordinal).ToArray(),
            PartiallyKnown: partiallyKnownMap.Values.OrderBy(static value => value.SymbolicId, StringComparer.Ordinal).ToArray(),
            Unknown: unknown,
            Metrics: new GovernedOntologicalCleaverMetrics(
                KnownRatio: Math.Round(knownRatio, 6),
                PartiallyKnownRatio: Math.Round(partiallyKnownRatio, 6),
                UnknownRatio: Math.Round(unknownRatio, 6),
                ConceptDensity: conceptDensity,
                ContextStability: contextStability),
            CanonicalRootAtlas: snapshot);
    }
}
