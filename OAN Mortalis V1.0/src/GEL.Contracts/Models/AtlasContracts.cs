using Oan.Spinal;

namespace GEL.Models;

public sealed class PredicateRoot
{
    public required string Key { get; init; }
    public required string DisplayLabel { get; init; }
    public required string AtlasDomain { get; init; }
    public string? SymbolicHandle { get; init; }
    public string? DictionaryPointer { get; init; }
}

public sealed class PredicateRefinementEdge
{
    public required string ParentRootKey { get; init; }
    public required string ChildRootKey { get; init; }
    public required string Relation { get; init; }
}

public sealed class DomainDescriptor
{
    public required string DomainName { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
}

public sealed class RootAtlasEntry
{
    public required PredicateRoot Root { get; init; }
    public required IReadOnlyList<string> VariantForms { get; init; }
    public required IReadOnlyList<SymbolicConstructorTriplet> SymbolicConstructors { get; init; }
    public required double FrequencyWeight { get; init; }
}

public sealed class SymbolicConstructorTriplet
{
    public string? PrefixKey { get; init; }
    public required string RootKey { get; init; }
    public string? SuffixKey { get; init; }
    public string? PrefixSymbol { get; init; }
    public required string RootSymbol { get; init; }
    public string? SuffixSymbol { get; init; }
    public required string CanonicalText { get; init; }
    public string? MergedGlyph { get; init; }
}

public sealed class RootAtlas
{
    public required string Version { get; init; }
    public required IReadOnlyList<RootAtlasEntry> Entries { get; init; }
    public required IReadOnlyList<PredicateRefinementEdge> RefinementEdges { get; init; }
    public required IReadOnlyList<DomainDescriptor> DomainDescriptors { get; init; }
    public required string Digest { get; init; }

    public bool TryResolveRoot(string key, out PredicateRoot root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var entry = Entries.FirstOrDefault(candidate =>
            string.Equals(candidate.Root.Key, key, StringComparison.OrdinalIgnoreCase) ||
            candidate.VariantForms.Any(variant => string.Equals(variant, key, StringComparison.OrdinalIgnoreCase)));

        if (entry is null)
        {
            root = default!;
            return false;
        }

        root = entry.Root;
        return true;
    }

    public static RootAtlas Create(
        string version,
        IEnumerable<RootAtlasEntry> entries,
        IEnumerable<PredicateRefinementEdge>? refinementEdges = null,
        IEnumerable<DomainDescriptor>? domainDescriptors = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(entries);

        var normalizedEntries = entries
            .OrderBy(entry => entry.Root.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => new RootAtlasEntry
            {
                Root = new PredicateRoot
                {
                    Key = entry.Root.Key,
                    DisplayLabel = entry.Root.DisplayLabel,
                    AtlasDomain = entry.Root.AtlasDomain,
                    SymbolicHandle = entry.Root.SymbolicHandle,
                    DictionaryPointer = entry.Root.DictionaryPointer
                },
                VariantForms = entry.VariantForms
                    .Where(variant => !string.IsNullOrWhiteSpace(variant))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(variant => variant, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                SymbolicConstructors = entry.SymbolicConstructors
                    .Select(constructor => new SymbolicConstructorTriplet
                    {
                        PrefixKey = constructor.PrefixKey,
                        RootKey = constructor.RootKey,
                        SuffixKey = constructor.SuffixKey,
                        PrefixSymbol = constructor.PrefixSymbol,
                        RootSymbol = constructor.RootSymbol,
                        SuffixSymbol = constructor.SuffixSymbol,
                        CanonicalText = constructor.CanonicalText,
                        MergedGlyph = constructor.MergedGlyph
                    })
                    .OrderBy(constructor => constructor.RootKey, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(constructor => constructor.PrefixKey ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(constructor => constructor.SuffixKey ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(constructor => constructor.CanonicalText, StringComparer.Ordinal)
                    .ToArray(),
                FrequencyWeight = entry.FrequencyWeight
            })
            .ToArray();

        var normalizedEdges = (refinementEdges ?? Array.Empty<PredicateRefinementEdge>())
            .OrderBy(edge => edge.ParentRootKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(edge => edge.ChildRootKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(edge => edge.Relation, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var normalizedDomains = (domainDescriptors ?? Array.Empty<DomainDescriptor>())
            .OrderBy(descriptor => descriptor.DomainName, StringComparer.OrdinalIgnoreCase)
            .Select(descriptor => new DomainDescriptor
            {
                DomainName = descriptor.DomainName,
                Description = descriptor.Description,
                Tags = descriptor.Tags
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            })
            .ToArray();

        var digestProjection = new
        {
            Version = version,
            Entries = normalizedEntries.Select(entry => new
            {
                Root = new
                {
                    entry.Root.Key,
                    entry.Root.DisplayLabel,
                    entry.Root.AtlasDomain,
                    entry.Root.SymbolicHandle,
                    entry.Root.DictionaryPointer
                },
                entry.VariantForms,
                SymbolicConstructors = entry.SymbolicConstructors.Select(constructor => new
                {
                    constructor.PrefixKey,
                    constructor.RootKey,
                    constructor.SuffixKey,
                    constructor.PrefixSymbol,
                    constructor.RootSymbol,
                    constructor.SuffixSymbol,
                    constructor.CanonicalText,
                    constructor.MergedGlyph
                }),
                entry.FrequencyWeight
            }),
            RefinementEdges = normalizedEdges,
            DomainDescriptors = normalizedDomains.Select(descriptor => new
            {
                descriptor.DomainName,
                descriptor.Description,
                descriptor.Tags
            })
        };

        var digest = Primitives.ComputeHash(Primitives.ToCanonicalJson(digestProjection));

        return new RootAtlas
        {
            Version = version,
            Entries = normalizedEntries,
            RefinementEdges = normalizedEdges,
            DomainDescriptors = normalizedDomains,
            Digest = digest
        };
    }
}
