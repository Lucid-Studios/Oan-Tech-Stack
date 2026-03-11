using System.Text.Json;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Graphs;
using GEL.Models;
using SLI.Engine.Morphology;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class LispMorphologyRuntimeParityTests
{
    [Fact]
    public async Task SentenceCorpus_MatchesOracleExactly()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var oracle = CreateOracleSentenceLane();
        var mirror = CreateLispMirror();

        foreach (var sentence in fixture.Sentences)
        {
            var oracleResult = await oracle.TranslateAsync(sentence.Text, atlas, overlayRoots);
            var mirrorResult = await mirror.TranslateSentenceAsync(sentence.Text, atlas, overlayRoots);

            AssertSentenceParity(oracleResult, mirrorResult);
        }
    }

    [Fact]
    public async Task ParagraphCorpus_MatchesOracleExactly()
    {
        var fixture = LoadParagraphFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var oracle = CreateOracleParagraphLane();
        var mirror = CreateLispMirror();

        var oracleResult = await oracle.TranslateAsync(fixture.Paragraph, atlas, overlayRoots);
        var mirrorResult = await mirror.TranslateParagraphAsync(fixture.Paragraph, atlas, overlayRoots);

        Assert.Equal(oracleResult.SentenceResults.Count, mirrorResult.SentenceResults.Count);
        for (var index = 0; index < oracleResult.SentenceResults.Count; index++)
        {
            AssertSentenceParity(oracleResult.SentenceResults[index], mirrorResult.SentenceResults[index]);
        }

        AssertEdgeParity(oracleResult.DiagnosticGraphEdges, mirrorResult.DiagnosticGraphEdges);
        Assert.Equal(oracleResult.GeneratedDrafts.Count, mirrorResult.GeneratedDrafts.Count);
        Assert.Equal(oracleResult.ClosureDecisions.Count, mirrorResult.ClosureDecisions.Count);
        Assert.All(mirrorResult.ClosureDecisions, decision => Assert.Equal(EngramClosureGrade.Closed, decision.Grade));
    }

    [Fact]
    public async Task ParagraphBodyCorpus_MatchesOracleExactly()
    {
        var fixture = LoadParagraphBodyFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var oracle = CreateOracleBodyLane();
        var mirror = CreateLispMirror();

        foreach (var paragraph in fixture.Paragraphs)
        {
            var oracleResult = await oracle.TranslateAsync(paragraph.Paragraph, atlas, overlayRoots);
            var mirrorResult = await mirror.TranslateParagraphBodyAsync(paragraph.Paragraph, atlas, overlayRoots);

            Assert.Equal(oracleResult.ContinuityAnchors, mirrorResult.ContinuityAnchors);
            Assert.Equal(oracleResult.ParagraphInvariants, mirrorResult.ParagraphInvariants);
            Assert.Equal(oracleResult.BodySummary, mirrorResult.BodySummary);
            Assert.Equal(oracleResult.DraftCluster.ClusterDiagnosticRender, mirrorResult.DraftCluster.ClusterDiagnosticRender);
            Assert.Equal(oracleResult.DraftCluster.AmbiguousSentenceKeys, mirrorResult.DraftCluster.AmbiguousSentenceKeys);
            Assert.Equal(oracleResult.DraftCluster.MemberDrafts.Count, mirrorResult.DraftCluster.MemberDrafts.Count);
            Assert.Equal(oracleResult.DraftCluster.MemberClosureDecisions.Count, mirrorResult.DraftCluster.MemberClosureDecisions.Count);
            AssertEdgeParity(oracleResult.ParagraphGraph.Edges, mirrorResult.ParagraphGraph.Edges);
        }
    }

    [Fact]
    public async Task Runtime_ReturnsBoundedOutOfScopeForUnknownInputs()
    {
        var runtime = new SliLispMorphologyRuntime();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToMorphologyOverlayRoots(LoadTranslationFixture().OverlayRoots);

        var sentenceResult = await runtime.TranslateSentenceAsync("The ridge opens at dawn.", atlas, overlayRoots);
        var paragraphResult = await runtime.TranslateParagraphAsync("The ridge opens at dawn. The ridge echoes.", atlas, overlayRoots);
        var bodyResult = await runtime.TranslateParagraphBodyAsync("The ridge opens at dawn. The ridge echoes.", atlas, overlayRoots);

        Assert.Equal(SliMorphologyLaneOutcome.OutOfScope, sentenceResult.LaneOutcome);
        Assert.Empty(sentenceResult.ResolvedLemmaRoots);
        Assert.Equal(SliMorphologyLaneOutcome.OutOfScope, paragraphResult.LaneOutcome);
        Assert.Empty(paragraphResult.SentenceResults);
        Assert.Equal(SliMorphologyLaneOutcome.OutOfScope, bodyResult.LaneOutcome);
        Assert.Empty(bodyResult.ParagraphInvariants);
    }

    private static EnglishNarrativeTranslationLane CreateOracleSentenceLane()
    {
        return new EnglishNarrativeTranslationLane(
            new EngramClosureValidator(),
            new SLI.Ingestion.OntologicalCleaver(),
            new RootAtlasOntologicalCleaver());
    }

    private static EnglishNarrativeParagraphLane CreateOracleParagraphLane()
    {
        return new EnglishNarrativeParagraphLane(CreateOracleSentenceLane());
    }

    private static EnglishNarrativeParagraphBodyLane CreateOracleBodyLane()
    {
        return new EnglishNarrativeParagraphBodyLane(CreateOracleParagraphLane());
    }

    private static LispNarrativeMirrorAdapter CreateLispMirror()
    {
        return new LispNarrativeMirrorAdapter(new EngramClosureValidator());
    }

    private static void AssertSentenceParity(NarrativeTranslationLaneResult expected, NarrativeTranslationLaneResult actual)
    {
        Assert.Equal(expected.Sentence, actual.Sentence);
        Assert.Equal(expected.ResolvedLemmaRoots, actual.ResolvedLemmaRoots);
        Assert.Equal(expected.LaneOutcome, actual.LaneOutcome);
        Assert.Equal(expected.DiagnosticPredicateRender, actual.DiagnosticPredicateRender);
        Assert.Equal(expected.PredicateRoot, actual.PredicateRoot);

        Assert.Equal(expected.OperatorAnnotations.Count, actual.OperatorAnnotations.Count);
        for (var index = 0; index < expected.OperatorAnnotations.Count; index++)
        {
            Assert.Equal(expected.OperatorAnnotations[index].Token, actual.OperatorAnnotations[index].Token);
            Assert.Equal(expected.OperatorAnnotations[index].Kind, actual.OperatorAnnotations[index].Kind);
        }

        Assert.Equal(expected.ConstructorBodies.Count, actual.ConstructorBodies.Count);
        for (var index = 0; index < expected.ConstructorBodies.Count; index++)
        {
            Assert.Equal(expected.ConstructorBodies[index].Role, actual.ConstructorBodies[index].Role);
            Assert.Equal(expected.ConstructorBodies[index].Constructor.RootKey, actual.ConstructorBodies[index].Constructor.RootKey);
            Assert.Equal(expected.ConstructorBodies[index].Constructor.CanonicalText, actual.ConstructorBodies[index].Constructor.CanonicalText);
        }

        if (expected.EngramDraft is null)
        {
            Assert.Null(actual.EngramDraft);
            Assert.Null(actual.ClosureDecision);
            return;
        }

        Assert.NotNull(actual.EngramDraft);
        Assert.Equal(expected.EngramDraft.RootKey, actual.EngramDraft!.RootKey);
        Assert.Equal(expected.EngramDraft.Trunk.Segments, actual.EngramDraft.Trunk.Segments);
        Assert.Equal(expected.EngramDraft.Trunk.Summary, actual.EngramDraft.Trunk.Summary);
        Assert.Equal(expected.EngramDraft.Branches.Select(branch => branch.Name), actual.EngramDraft.Branches.Select(branch => branch.Name));
        Assert.Equal(expected.EngramDraft.Branches.Select(branch => branch.RootKey), actual.EngramDraft.Branches.Select(branch => branch.RootKey));
        Assert.Equal(expected.EngramDraft.Invariants.Select(invariant => invariant.Key), actual.EngramDraft.Invariants.Select(invariant => invariant.Key));
        Assert.Equal(expected.EngramDraft.Invariants.Select(invariant => invariant.Statement), actual.EngramDraft.Invariants.Select(invariant => invariant.Statement));

        Assert.NotNull(actual.ClosureDecision);
        Assert.Equal(expected.ClosureDecision!.Grade, actual.ClosureDecision!.Grade);
        Assert.Equal(expected.ClosureDecision.ReasonCodes, actual.ClosureDecision.ReasonCodes);
        Assert.Equal(expected.ClosureDecision.Warnings, actual.ClosureDecision.Warnings);
    }

    private static void AssertEdgeParity(IReadOnlyList<ConstructorEdge> expected, IReadOnlyList<ConstructorEdge> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Source, actual[index].Source);
            Assert.Equal(expected[index].Target, actual[index].Target);
            Assert.Equal(expected[index].Relation, actual[index].Relation);
        }
    }

    private static async Task<RootAtlas> LoadCanonicalAtlasAsync()
    {
        var cleaver = new RootAtlasOntologicalCleaver();
        var result = await cleaver.CleaveAsync("observe");
        return result.CanonicalRootAtlas;
    }

    private static IReadOnlyList<NarrativeOverlayRoot> ToOverlayRoots(IReadOnlyList<FixtureOverlayRoot> roots)
    {
        return roots.Select(root => new NarrativeOverlayRoot
        {
            Lemma = root.Lemma,
            SymbolicCore = root.SymbolicCore,
            OperatorCompatibility = root.OperatorCompatibility,
            ReservedDomainStatus = root.ReservedDomainStatus,
            DisciplinaryReservations = root.DisciplinaryReservations,
            VariantExamples = root.VariantExamples
        }).ToArray();
    }

    private static IReadOnlyList<SliMorphologyOverlayRoot> ToMorphologyOverlayRoots(IReadOnlyList<FixtureOverlayRoot> roots)
    {
        return roots.Select(root => new SliMorphologyOverlayRoot
        {
            Lemma = root.Lemma,
            SymbolicCore = root.SymbolicCore,
            OperatorCompatibility = root.OperatorCompatibility,
            ReservedDomainStatus = root.ReservedDomainStatus,
            DisciplinaryReservations = root.DisciplinaryReservations,
            VariantExamples = root.VariantExamples
        }).ToArray();
    }

    private static TranslationFixtureDefinition LoadTranslationFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeTranslationFixture.json");
        return Deserialize<TranslationFixtureDefinition>(path, "translation fixture");
    }

    private static ParagraphFixtureDefinition LoadParagraphFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeParagraphFixture.json");
        return Deserialize<ParagraphFixtureDefinition>(path, "paragraph fixture");
    }

    private static ParagraphBodyFixtureDefinition LoadParagraphBodyFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeParagraphBodyFixture.json");
        return Deserialize<ParagraphBodyFixtureDefinition>(path, "paragraph body fixture");
    }

    private static T Deserialize<T>(string path, string description)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, SerializerOptions)
               ?? throw new InvalidOperationException($"Narrative {description} could not be parsed.");
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

    private sealed class TranslationFixtureDefinition
    {
        public required IReadOnlyList<FixtureSentence> Sentences { get; init; }
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
    }

    private sealed class ParagraphFixtureDefinition
    {
        public required string Paragraph { get; init; }
        public required IReadOnlyList<FixtureSentence> Sentences { get; init; }
        public required IReadOnlyList<FixtureContinuityEdge> ExpectedContinuityEdges { get; init; }
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
    }

    private sealed class ParagraphBodyFixtureDefinition
    {
        public required IReadOnlyList<FixtureParagraph> Paragraphs { get; init; }
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
    }

    private sealed class FixtureParagraph
    {
        public required string Paragraph { get; init; }
        public required int ExpectedSentenceCount { get; init; }
        public required IReadOnlyList<string> ExpectedContinuityAnchors { get; init; }
        public required IReadOnlyList<string> ExpectedInvariants { get; init; }
        public required string ExpectedClusterRender { get; init; }
        public required string ExpectedBodySummary { get; init; }
        public required int ExpectedClosedDraftCount { get; init; }
        public required int ExpectedAmbiguousSentenceCount { get; init; }
    }

    private sealed class FixtureSentence
    {
        public required string Text { get; init; }
        public required IReadOnlyList<string> ExpectedRoots { get; init; }
        public required string ExpectedRender { get; init; }
        public required string ExpectedOutcome { get; init; }
        public string? ExpectedClosureGrade { get; init; }
    }

    private sealed class FixtureContinuityEdge
    {
        public required string Source { get; init; }
        public required string Target { get; init; }
        public required string Relation { get; init; }
    }

    private sealed class FixtureOverlayRoot
    {
        public required string Lemma { get; init; }
        public required string SymbolicCore { get; init; }
        public required string OperatorCompatibility { get; init; }
        public required string ReservedDomainStatus { get; init; }
        public required IReadOnlyList<string> DisciplinaryReservations { get; init; }
        public required IReadOnlyList<string> VariantExamples { get; init; }
    }
}
