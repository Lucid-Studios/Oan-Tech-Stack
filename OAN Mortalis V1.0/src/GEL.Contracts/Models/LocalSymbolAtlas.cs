namespace GEL.Models;

public sealed class LocalSymbolAtlas
{
    public required string DomainName { get; init; }
    public required IReadOnlyDictionary<string, string> SymbolMap { get; init; }

    public RootAtlas ToRootAtlas(string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        var entries = SymbolMap
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => new RootAtlasEntry
            {
                Root = new PredicateRoot
                {
                    Key = entry.Key,
                    DisplayLabel = entry.Key,
                    AtlasDomain = DomainName,
                    SymbolicHandle = entry.Value,
                    DictionaryPointer = $"atlas://local/{DomainName}/{entry.Key}"
                },
                VariantForms = new[] { entry.Key },
                SymbolicConstructors =
                [
                    new SymbolicConstructorTriplet
                    {
                        RootKey = entry.Key,
                        RootSymbol = entry.Value,
                        CanonicalText = entry.Key,
                        MergedGlyph = entry.Value
                    }
                ],
                FrequencyWeight = 1d
            })
            .ToArray();

        return RootAtlas.Create(
            version,
            entries,
            domainDescriptors:
            [
                new DomainDescriptor
                {
                    DomainName = DomainName,
                    Description = $"Compatibility local symbol atlas for {DomainName}.",
                    Tags = ["compatibility", "local-symbol-atlas"]
                }
            ]);
    }
}
