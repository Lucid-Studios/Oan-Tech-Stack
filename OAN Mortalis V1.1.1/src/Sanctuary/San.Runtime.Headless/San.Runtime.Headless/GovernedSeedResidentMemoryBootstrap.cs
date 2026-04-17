using CradleTek.Memory;

namespace San.Runtime.Headless;

internal static class GovernedSeedResidentMemoryBootstrap
{
    public static IGovernedEngramCorpusSource CreateEngramCorpusSource()
    {
        return new InMemoryEngramCorpusSource(
            new GovernedEngramCorpusSnapshot(
                Source: "Lucid Research Corpus",
                SnapshotProfile: "resident-memory-bootstrap-corpus",
                Nodes:
                [
                    new GovernedEngramCorpusNode(
                        EngramId: "engram-identity",
                        ConceptTag: "identity",
                        DomainTag: "self",
                        RelatedNodeIds: ["engram-continuity", "engram-stewardship"],
                        StructuralDegree: 6),
                    new GovernedEngramCorpusNode(
                        EngramId: "engram-continuity",
                        ConceptTag: "continuity",
                        DomainTag: "self",
                        RelatedNodeIds: ["engram-identity"],
                        StructuralDegree: 5),
                    new GovernedEngramCorpusNode(
                        EngramId: "engram-stewardship",
                        ConceptTag: "stewardship",
                        DomainTag: "governance",
                        RelatedNodeIds: ["engram-identity", "engram-governance"],
                        StructuralDegree: 4),
                    new GovernedEngramCorpusNode(
                        EngramId: "engram-governance",
                        ConceptTag: "governance",
                        DomainTag: "office",
                        RelatedNodeIds: ["engram-stewardship"],
                        StructuralDegree: 3)
                ],
                Clusters:
                [
                    new GovernedEngramCorpusCluster(
                        ClusterId: "cluster-self",
                        NodeIds: ["engram-identity", "engram-continuity"],
                        ClusterProfile: "resident-self-cluster"),
                    new GovernedEngramCorpusCluster(
                        ClusterId: "cluster-governance",
                        NodeIds: ["engram-stewardship", "engram-governance"],
                        ClusterProfile: "resident-governance-cluster")
                ]));
    }

    public static IGovernedRootAtlasSource CreateRootAtlasSource()
    {
        return new InMemoryRootAtlasSource(
            new GovernedRootAtlasSnapshot(
                Source: "Lucid Research Corpus",
                SnapshotProfile: "resident-memory-bootstrap-atlas",
                Entries:
                [
                    new GovernedRootAtlasEntry(
                        RootKey: "identity",
                        RootEngram: new GovernedRootEngram(
                            SymbolicId: "ATLAS.ROOT.IDENTITY",
                            AtlasDomain: "atlas.root.identity",
                            RootTerm: "identity",
                            VariantForms: ["identities"],
                            FrequencyWeight: 0.72,
                            DictionaryPointer: "atlas://root/identity"),
                        VariantForms: ["identities"],
                        SymbolicConstructors: ["(root identity)"]),
                    new GovernedRootAtlasEntry(
                        RootKey: "continuity",
                        RootEngram: new GovernedRootEngram(
                            SymbolicId: "ATLAS.ROOT.CONTINUITY",
                            AtlasDomain: "atlas.root.continuity",
                            RootTerm: "continuity",
                            VariantForms: ["continuities"],
                            FrequencyWeight: 0.69,
                            DictionaryPointer: "atlas://root/continuity"),
                        VariantForms: ["continuities"],
                        SymbolicConstructors: ["(root continuity)"]),
                    new GovernedRootAtlasEntry(
                        RootKey: "steward",
                        RootEngram: new GovernedRootEngram(
                            SymbolicId: "ATLAS.ROOT.STEWARD",
                            AtlasDomain: "atlas.root.steward",
                            RootTerm: "steward",
                            VariantForms: ["stewards", "stewardship"],
                            FrequencyWeight: 0.63,
                            DictionaryPointer: "atlas://root/steward"),
                        VariantForms: ["stewards", "stewardship"],
                        SymbolicConstructors: ["(root steward)"]),
                    new GovernedRootAtlasEntry(
                        RootKey: "govern",
                        RootEngram: new GovernedRootEngram(
                            SymbolicId: "ATLAS.ROOT.GOVERN",
                            AtlasDomain: "atlas.root.govern",
                            RootTerm: "govern",
                            VariantForms: ["governance", "governing"],
                            FrequencyWeight: 0.61,
                            DictionaryPointer: "atlas://root/govern"),
                        VariantForms: ["governance", "governing"],
                        SymbolicConstructors: ["(root govern)"])
                ],
                RootSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["identity"] = "ID",
                    ["continuity"] = "CONT",
                    ["steward"] = "STWD",
                    ["govern"] = "GOV"
                },
                PrefixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                SuffixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Diagnostics: Array.Empty<GovernedAtlasSourceDiagnostic>()));
    }
}
