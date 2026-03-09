using GEL.Models;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class EngramCanonicalizationAdapterTests
{
    [Fact]
    public async Task RootAtlasOntologicalCleaver_EmitsCanonicalAtlasAndRootEntries()
    {
        var cleaver = new CradleTek.Memory.Services.RootAtlasOntologicalCleaver();
        var result = await cleaver.CleaveAsync("Arithmetic equation variable");

        Assert.NotNull(result.CanonicalRootAtlas);
        Assert.NotEmpty(result.CanonicalRootAtlas.Entries);
        Assert.True(result.CanonicalRootAtlas.TryResolveRoot("equation", out var root));
        Assert.Equal("equation", root.Key);
        Assert.Contains(result.Known.Concat(result.PartiallyKnown), entry => entry.CanonicalAtlasEntry is not null);
        Assert.Contains(result.CanonicalRootAtlas.Entries, entry =>
            entry.Root.Key == "equation" &&
            entry.SymbolicConstructors.Any(constructor => constructor.RootKey == "equation"));
    }

    [Fact]
    public void ConstructorEngramBuilder_CanEmitCanonicalDrafts()
    {
        var builder = new ConstructorEngramBuilder();
        var records = new[]
        {
            new ConstructorEngramRecord
            {
                Domain = "mathematics",
                SymbolicStructure = "(= (+ x 2) 4)",
                RootReferences = ["equation", "variable", "addition"],
                Level = ConstructorEngramLevel.Intermediate
            }
        };

        var drafts = builder.BuildCanonicalDrafts(records);

        Assert.Single(drafts);
        Assert.Equal("equation", drafts[0].RootKey);
        Assert.Equal(EngramClosureGrade.Closed, drafts[0].RequestedClosureGrade);
        Assert.Equal(EngramEpistemicClass.Procedural, drafts[0].EpistemicClass);
        Assert.NotEmpty(drafts[0].Invariants);
        Assert.NotEmpty(drafts[0].Branches);
    }
}
