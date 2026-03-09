using System.Text.Json;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Graphs;
using GEL.Models;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class EnglishNarrativeParagraphLaneTests
{
    [Fact]
    public void Fixture_IsWellFormedAndOverlayUnchanged()
    {
        var fixture = LoadFixture();

        Assert.Equal(5, fixture.Sentences.Count);
        Assert.Equal(7, fixture.OverlayRoots.Count);
        Assert.Equal(
            ["gate", "remember", "make", "hum", "percent", "light", "lie"],
            fixture.OverlayRoots.Select(root => root.Lemma).ToArray());
        Assert.Equal(3, fixture.ExpectedContinuityEdges.Count);
    }

    [Fact]
    public async Task TranslateAsync_ReusesSentenceLaneAndAggregatesDrafts()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();

        var result = await lane.TranslateAsync(fixture.Paragraph, atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(5, result.SentenceResults.Count);
        Assert.Equal(4, result.GeneratedDrafts.Count);
        Assert.Equal(4, result.ClosureDecisions.Count);

        for (var index = 0; index < fixture.Sentences.Count; index++)
        {
            var expected = fixture.Sentences[index];
            var actual = result.SentenceResults[index];

            Assert.Equal(expected.Text, actual.Sentence);
            Assert.Equal(expected.ExpectedRoots, actual.ResolvedLemmaRoots);
            Assert.Equal(expected.ExpectedRender, actual.DiagnosticPredicateRender);
            Assert.Equal(
                Enum.Parse<NarrativeTranslationLaneOutcome>(expected.ExpectedOutcome),
                actual.LaneOutcome);

            if (expected.ExpectedClosureGrade is null)
            {
                Assert.Null(actual.EngramDraft);
                Assert.Null(actual.ClosureDecision);
            }
            else
            {
                Assert.NotNull(actual.EngramDraft);
                Assert.NotNull(actual.ClosureDecision);
                Assert.Equal(
                    Enum.Parse<EngramClosureGrade>(expected.ExpectedClosureGrade),
                    actual.ClosureDecision!.Grade);
            }
        }
    }

    [Fact]
    public async Task TranslateAsync_EmitsDeterministicGraphEdges()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();

        var result = await lane.TranslateAsync(fixture.Paragraph, atlas, ToOverlayRoots(fixture.OverlayRoots));

        var expectedEdges = new[]
        {
            new ConstructorEdge { Source = "remember", Target = "gate", Relation = "subject" },
            new ConstructorEdge { Source = "remember", Target = "make", Relation = "object" },
            new ConstructorEdge { Source = "observe", Target = "gate", Relation = "subject" },
            new ConstructorEdge { Source = "observe", Target = "hum", Relation = "object" },
            new ConstructorEdge { Source = "increase", Target = "hum", Relation = "subject" },
            new ConstructorEdge { Source = "increase", Target = "percent", Relation = "unit" },
            new ConstructorEdge { Source = "change", Target = "light", Relation = "subject" },
            new ConstructorEdge { Source = "change", Target = "gate", Relation = "object" },
            new ConstructorEdge { Source = "lie", Target = "light", Relation = "subject" },
            new ConstructorEdge { Source = "remember", Target = "observe", Relation = "continuity:gate" },
            new ConstructorEdge { Source = "observe", Target = "increase", Relation = "continuity:hum" },
            new ConstructorEdge { Source = "change", Target = "lie", Relation = "continuity:light" }
        };

        Assert.Equal(expectedEdges.Length, result.DiagnosticGraphEdges.Count);
        for (var index = 0; index < expectedEdges.Length; index++)
        {
            Assert.Equal(expectedEdges[index].Source, result.DiagnosticGraphEdges[index].Source);
            Assert.Equal(expectedEdges[index].Target, result.DiagnosticGraphEdges[index].Target);
            Assert.Equal(expectedEdges[index].Relation, result.DiagnosticGraphEdges[index].Relation);
        }

        Assert.Equal(expectedEdges.Length, result.ParagraphGraph.Edges.Count);
        Assert.DoesNotContain(result.DiagnosticGraphEdges, edge =>
            edge.Source is "has" or "was" or "first" or "its" ||
            edge.Target is "has" or "was" or "first" or "its");
    }

    private static EnglishNarrativeParagraphLane CreateLane()
    {
        var sentenceLane = new EnglishNarrativeTranslationLane(
            new EngramClosureValidator(),
            new SLI.Ingestion.OntologicalCleaver(),
            new RootAtlasOntologicalCleaver());

        return new EnglishNarrativeParagraphLane(sentenceLane);
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

    private static ParagraphFixtureDefinition LoadFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeParagraphFixture.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ParagraphFixtureDefinition>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Narrative paragraph fixture could not be parsed.");
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

    private sealed class ParagraphFixtureDefinition
    {
        public required string Paragraph { get; init; }
        public required IReadOnlyList<FixtureSentence> Sentences { get; init; }
        public required IReadOnlyList<FixtureContinuityEdge> ExpectedContinuityEdges { get; init; }
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
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
