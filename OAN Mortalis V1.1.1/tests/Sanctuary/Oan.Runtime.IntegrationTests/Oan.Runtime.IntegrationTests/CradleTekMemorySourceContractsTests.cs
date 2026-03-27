using CradleTek.Memory;

namespace Oan.Runtime.IntegrationTests;

public sealed class CradleTekMemorySourceContractsTests
{
    [Fact]
    public async Task InMemoryEngramCorpusSource_Loads_LawfulSnapshot_Without_File_Probing()
    {
        IGovernedEngramCorpusSource source = new InMemoryEngramCorpusSource(
            new GovernedEngramCorpusSnapshot(
                Source: "Lucid Research Corpus",
                SnapshotProfile: "neutral-engram-corpus-source",
                Nodes:
                [
                    new GovernedEngramCorpusNode(
                        EngramId: "engram-001",
                        ConceptTag: "identity",
                        DomainTag: "self",
                        RelatedNodeIds: ["engram-002"],
                        StructuralDegree: 3)
                ],
                Clusters:
                [
                    new GovernedEngramCorpusCluster(
                        ClusterId: "cluster-self",
                        NodeIds: ["engram-001"],
                        ClusterProfile: "self-memory-cluster")
                ]));

        var snapshot = await source.LoadSnapshotAsync();

        Assert.Equal("Lucid Research Corpus", snapshot.Source);
        Assert.Equal("neutral-engram-corpus-source", snapshot.SnapshotProfile);
        var node = Assert.Single(snapshot.Nodes);
        Assert.Equal("engram-001", node.EngramId);
        Assert.Equal("identity", node.ConceptTag);
        Assert.Equal("self", node.DomainTag);
        Assert.Equal(3, node.StructuralDegree);
        var cluster = Assert.Single(snapshot.Clusters);
        Assert.Equal("cluster-self", cluster.ClusterId);
        Assert.Equal("self-memory-cluster", cluster.ClusterProfile);
    }

    [Fact]
    public async Task InMemoryRootAtlasSource_Loads_LawfulSnapshot_And_Diagnostics()
    {
        IGovernedRootAtlasSource source = new InMemoryRootAtlasSource(
            new GovernedRootAtlasSnapshot(
                Source: "Lucid Research Corpus",
                SnapshotProfile: "neutral-root-atlas-source",
                Entries:
                [
                    new GovernedRootAtlasEntry(
                        RootKey: "continuity",
                        RootEngram: new GovernedRootEngram(
                            SymbolicId: "ATLAS.ROOT.CONTINUITY",
                            AtlasDomain: "atlas.root.c",
                            RootTerm: "continuity",
                            VariantForms: ["continuities"],
                            FrequencyWeight: 0.71,
                            DictionaryPointer: "atlas://root/continuity"),
                        VariantForms: ["continuities"],
                        SymbolicConstructors: ["(root continuity)"])
                ],
                RootSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["continuity"] = "CONT"
                },
                PrefixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                SuffixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Diagnostics:
                [
                    new GovernedAtlasSourceDiagnostic(
                        Severity: GovernedAtlasSourceDiagnosticSeverity.Warning,
                        Code: "atlas-source-normalized",
                        SourceLayer: "memory-source",
                        Message: "Atlas normalized from lawful snapshot input.",
                        RootKey: "continuity")
                ]));

        var snapshot = await source.LoadSnapshotAsync();

        Assert.Equal("Lucid Research Corpus", snapshot.Source);
        Assert.Equal("neutral-root-atlas-source", snapshot.SnapshotProfile);
        Assert.False(snapshot.HasErrors);
        Assert.Equal("CONT", snapshot.RootSymbols["continuity"]);
        var entry = Assert.Single(snapshot.Entries);
        Assert.Equal("continuity", entry.RootKey);
        Assert.Equal("ATLAS.ROOT.CONTINUITY", entry.RootEngram.SymbolicId);
        Assert.Equal("atlas://root/continuity", entry.RootEngram.DictionaryPointer);
        var diagnostic = Assert.Single(snapshot.Diagnostics);
        Assert.Equal(GovernedAtlasSourceDiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("atlas-source-normalized", diagnostic.Code);
    }
}
