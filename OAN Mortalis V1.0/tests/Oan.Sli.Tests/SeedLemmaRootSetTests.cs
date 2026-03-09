using System.Text.Json;
using CradleTek.Memory.Services;

namespace Oan.Sli.Tests;

public sealed class SeedLemmaRootSetTests
{
    [Fact]
    public void SeedLemmaRoots_HasExpectedShapeAndCounts()
    {
        var entries = LoadSeedEntries();

        Assert.Equal(350, entries.Count);
        Assert.Equal(entries.Count, entries.Select(entry => entry.Lemma).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(entries.Count, entries.Select(entry => entry.SymbolicCore).Distinct(StringComparer.Ordinal).Count());

        Assert.All(entries, entry =>
        {
            Assert.Equal(entry.Lemma, entry.Lemma.ToLowerInvariant());
            Assert.False(string.IsNullOrWhiteSpace(entry.SymbolicCore));
            Assert.StartsWith("atlas.core::", entry.SymbolicCore, StringComparison.Ordinal);
            Assert.Contains(entry.OperatorCompatibility, AllowedOperatorCompatibilities, StringComparer.Ordinal);
            Assert.Contains(entry.ReservedDomainStatus, AllowedReservedStatuses, StringComparer.Ordinal);
            if (entry.ReservedDomainStatus == "none")
            {
                Assert.Empty(entry.DisciplinaryReservations);
            }
            else
            {
                Assert.NotEmpty(entry.DisciplinaryReservations);
            }
        });

        AssertCategoryCount(entries, "action");
        AssertCategoryCount(entries, "relation");
        AssertCategoryCount(entries, "transformation");
        AssertCategoryCount(entries, "state");
        AssertCategoryCount(entries, "measurement");
        AssertCategoryCount(entries, "classification");
        AssertCategoryCount(entries, "observation");
    }

    [Fact]
    public void SeedLemmaRoots_AllEntriesExistInCandidatePool()
    {
        var entries = LoadSeedEntries();
        var gelRoots = LoadGelRoots();

        Assert.All(entries, entry => Assert.Contains(entry.Lemma, gelRoots, StringComparer.Ordinal));
    }

    [Fact]
    public async Task SeedLemmaRoots_ExactLemmaInputsResolveToSeedRoots()
    {
        var entries = LoadSeedEntries();
        var cleaver = new RootAtlasOntologicalCleaver();

        foreach (var entry in entries)
        {
            var result = await cleaver.CleaveAsync(entry.Lemma);

            Assert.Contains(result.Known, known =>
                known.RootTerm.Equals(entry.Lemma, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task SeedLemmaRoots_VariantExamplesResolveToSeedRoots()
    {
        var entries = LoadSeedEntries();
        var cleaver = new RootAtlasOntologicalCleaver();

        foreach (var entry in entries)
        {
            foreach (var variant in entry.VariantExamples
                         .Where(variant => !string.Equals(variant, entry.Lemma, StringComparison.OrdinalIgnoreCase)))
            {
                var result = await cleaver.CleaveAsync(variant);
                var resolved = result.Known.Concat(result.PartiallyKnown).ToList();

                Assert.Contains(resolved, known =>
                    known.RootTerm.Equals(entry.Lemma, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    private static void AssertCategoryCount(
        IReadOnlyList<SeedLemmaRootEntry> entries,
        string category)
    {
        Assert.Equal(50, entries.Count(entry => string.Equals(entry.PrimaryCategory, category, StringComparison.Ordinal)));
    }

    private static IReadOnlyList<SeedLemmaRootEntry> LoadSeedEntries()
    {
        var path = ResolveRepoFile("public_root", "seed", "SeedLemmaRoots.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<SeedLemmaRootEntry>>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Seed lemma root manifest could not be parsed.");
    }

    private static HashSet<string> LoadGelRoots()
    {
        var path = ResolveRepoFile("public_root", "GEL.ndjson");
        var firstLine = File.ReadLines(path).First();
        using var document = JsonDocument.Parse(firstLine);
        return document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string ResolveRepoFile(params string[] parts)
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                var expected = Path.Combine(new[] { current.FullName }.Concat(parts).ToArray());
                if (File.Exists(expected))
                {
                    return expected;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException($"Unable to locate {Path.Combine(parts)} from the current test context.");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] AllowedOperatorCompatibilities =
    [
        "core-only",
        "prefix-capable",
        "suffix-capable",
        "prefix-suffix-capable"
    ];

    private static readonly string[] AllowedReservedStatuses =
    [
        "none",
        "bridge-only"
    ];

    private sealed class SeedLemmaRootEntry
    {
        public required string Lemma { get; init; }
        public required string SymbolicCore { get; init; }
        public required string PrimaryCategory { get; init; }
        public required IReadOnlyList<string> SecondaryCategories { get; init; }
        public required string DomainDescriptor { get; init; }
        public required string OperatorCompatibility { get; init; }
        public required IReadOnlyList<string> VariantExamples { get; init; }
        public required string ReservedDomainStatus { get; init; }
        public required IReadOnlyList<string> DisciplinaryReservations { get; init; }
    }
}
