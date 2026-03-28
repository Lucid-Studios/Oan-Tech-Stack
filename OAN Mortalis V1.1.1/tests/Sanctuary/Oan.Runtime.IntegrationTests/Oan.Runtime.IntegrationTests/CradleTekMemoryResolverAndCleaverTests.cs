using CradleTek.Memory;
using CradleTek.Mantle;
using SLI.Ingestion;

namespace Oan.Runtime.IntegrationTests;

public sealed class CradleTekMemoryResolverAndCleaverTests
{
    [Fact]
    public async Task GovernedEngramResolverService_Uses_CorpusSnapshot_For_Relevant_Ranking()
    {
        IGovernedEngramResolver resolver = new GovernedEngramResolverService(
            new InMemoryEngramCorpusSource(
                new GovernedEngramCorpusSnapshot(
                    Source: "Lucid Research Corpus",
                    SnapshotProfile: "memory-corpus-snapshot",
                    Nodes:
                    [
                        new GovernedEngramCorpusNode(
                            EngramId: "engram-identity",
                            ConceptTag: "identity",
                            DomainTag: "self",
                            RelatedNodeIds: ["engram-steward"],
                            StructuralDegree: 5),
                        new GovernedEngramCorpusNode(
                            EngramId: "engram-steward",
                            ConceptTag: "stewardship",
                            DomainTag: "governance",
                            RelatedNodeIds: ["engram-identity"],
                            StructuralDegree: 3)
                    ],
                    Clusters:
                    [
                        new GovernedEngramCorpusCluster(
                            ClusterId: "cluster-self",
                            NodeIds: ["engram-identity"],
                            ClusterProfile: "self-memory-cluster")
                    ])));

        var result = await resolver.ResolveRelevantAsync(
            new GovernedEngramResolutionContext(
                ContextHandle: "context://memory-001",
                TaskObjective: "recover identity continuity for the steward",
                RelevantFragments: ["identity continuity", "steward mediation"],
                SourceSubsystem: "soulframe-stewardship"));

        Assert.Equal("Lucid Research Corpus", result.Source);
        var top = Assert.IsType<GovernedEngramSummary>(Assert.Single(result.Summaries.Take(1)));
        Assert.Equal("engram-identity", top.EngramId);
        Assert.Equal("identity", top.ConceptTag);
        Assert.Contains("cluster:cluster-self", top.DecisionSpline, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GovernedEngramResolverService_SelfSensitivePath_Uses_Contradiction_Posture()
    {
        IGovernedEngramResolver resolver = new GovernedEngramResolverService(
            new InMemoryEngramCorpusSource(
                new GovernedEngramCorpusSnapshot(
                    Source: "Lucid Research Corpus",
                    SnapshotProfile: "memory-corpus-snapshot",
                    Nodes:
                    [
                        new GovernedEngramCorpusNode(
                            EngramId: "engram-identity",
                            ConceptTag: "identity",
                            DomainTag: "self",
                            RelatedNodeIds: Array.Empty<string>(),
                            StructuralDegree: 4),
                        new GovernedEngramCorpusNode(
                            EngramId: "engram-foreign",
                            ConceptTag: "foreign",
                            DomainTag: "other",
                            RelatedNodeIds: Array.Empty<string>(),
                            StructuralDegree: 1)
                    ],
                    Clusters: Array.Empty<GovernedEngramCorpusCluster>())));
        IGovernedSelfGelValidationHandleProjector validationHandleProjector = new GovernedSelfGelValidationHandleProjector();

        var result = await resolver.ResolveSelfSensitiveAsync(
            new GovernedEngramResolutionContext(
                ContextHandle: "context://memory-002",
                TaskObjective: "self identity contradict foreign not-self trace",
                RelevantFragments: ["identity trace", "foreign mismatch"],
                SourceSubsystem: "soulframe-stewardship"),
            validationReferenceHandle: validationHandleProjector.ProjectPresentedValidationHandle("cselfgel://seed-001"));

        Assert.Equal("Lucid Research Corpus", result.Source);
        var firstClaim = Assert.IsType<GovernedEngramSelfResolutionClaim>(Assert.Single(result.Claims.Take(1)));
        Assert.Equal(GovernedEngramSelfValidationPosture.Contradicted, firstClaim.ValidationPosture);
        Assert.Equal(GovernedEngramSelfResolutionOrigin.HotWorkingResolution, firstClaim.Origin);
        Assert.Equal("self-claim-conflict", firstClaim.ObstructionCode);
    }

    [Fact]
    public async Task GovernedRootOntologicalCleaver_Uses_RootAtlasSnapshot_Without_File_Probing()
    {
        IGovernedRootOntologicalCleaver cleaver = new GovernedRootOntologicalCleaver(
            new InMemoryRootAtlasSource(
                new GovernedRootAtlasSnapshot(
                    Source: "Lucid Research Corpus",
                    SnapshotProfile: "root-atlas-snapshot",
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
                            SymbolicConstructors: ["(root continuity)"]),
                        new GovernedRootAtlasEntry(
                            RootKey: "steward",
                            RootEngram: new GovernedRootEngram(
                                SymbolicId: "ATLAS.ROOT.STEWARD",
                                AtlasDomain: "atlas.root.s",
                                RootTerm: "steward",
                                VariantForms: ["stewards"],
                                FrequencyWeight: 0.62,
                                DictionaryPointer: "atlas://root/steward"),
                            VariantForms: ["stewards"],
                            SymbolicConstructors: ["(root steward)"])
                    ],
                    RootSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["continuity"] = "CONT",
                        ["steward"] = "STWD"
                    },
                    PrefixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    SuffixSymbols: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    Diagnostics: Array.Empty<GovernedAtlasSourceDiagnostic>())));

        var result = await cleaver.CleaveAsync("Continuities steward unknownness");

        Assert.Equal("Continuities steward unknownness", result.InputText);
        Assert.Equal("root-atlas-snapshot", result.CanonicalRootAtlas.SnapshotProfile);
        Assert.Equal("Lucid Research Corpus", result.CanonicalRootAtlas.Source);
        Assert.Contains(result.Resolutions, resolution =>
            resolution.NormalizedToken == "continuities" &&
            resolution.Classification == GovernedOntologicalCleaverClassification.PartiallyKnown &&
            resolution.ResolutionReason == "variant-root");
        Assert.Contains(result.Resolutions, resolution =>
            resolution.NormalizedToken == "steward" &&
            resolution.Classification == GovernedOntologicalCleaverClassification.Known &&
            resolution.ResolutionReason == "exact-root");
        Assert.Contains("unknownness", result.Unknown);
        Assert.Equal("moderate", result.Metrics.ConceptDensity);
        Assert.Equal("transitional", result.Metrics.ContextStability);
    }

    [Fact]
    public void GovernedQueryLexicalCueService_Centralizes_Query_Cues()
    {
        IGovernedQueryLexicalCueService lexicalCueService = new GovernedQueryLexicalCueService();

        var cue = lexicalCueService.Analyze(
            "self identity contradict foreign trace",
            ["continuity cue", "not-self mismatch"]);

        Assert.Contains("identity", cue.HintTokens);
        Assert.Contains("continuity", cue.HintTokens);
        Assert.Contains("foreign", cue.HintTokens);
        Assert.True(cue.SelfSensitive);
        Assert.True(cue.ContradictionRequested);
    }

    [Fact]
    public void GovernedOntologicalLexemeService_Centralizes_Lexeme_Normalization()
    {
        IGovernedOntologicalLexemeService lexemeService = new GovernedOntologicalLexemeService();

        var tokens = lexemeService.Tokenize("Continuities steward unknownness");

        Assert.Equal(["Continuities", "steward", "unknownness"], tokens);
        Assert.Equal("continuities", lexemeService.NormalizeToken("Continuities"));
        Assert.True(lexemeService.TryNormalizeMorphology("continuities", out var normalized));
        Assert.Equal("continuity", normalized);
    }
}
