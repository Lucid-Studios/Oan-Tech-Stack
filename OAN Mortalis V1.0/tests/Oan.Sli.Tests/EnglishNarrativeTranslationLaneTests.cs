using System.Text.Json;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Models;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class EnglishNarrativeTranslationLaneTests
{
    [Fact]
    public void Fixture_IsWellFormedAndDoesNotMutateSeedManifest()
    {
        var fixture = LoadFixture();
        var seedLemmas = LoadSeedLemmas();

        Assert.Equal(3, fixture.Sentences.Count);
        Assert.Equal(7, fixture.OverlayRoots.Count);
        Assert.Equal(
            ["gate", "remember", "make", "hum", "percent", "light", "lie"],
            fixture.OverlayRoots.Select(root => root.Lemma).ToArray());

        Assert.All(fixture.OverlayRoots, root =>
        {
            Assert.False(string.IsNullOrWhiteSpace(root.SymbolicCore));
            Assert.Equal("none", root.ReservedDomainStatus);
            Assert.Empty(root.DisciplinaryReservations);
        });

        Assert.DoesNotContain("gate", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("remember", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("make", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("hum", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("percent", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("light", seedLemmas, StringComparer.Ordinal);
        Assert.DoesNotContain("lie", seedLemmas, StringComparer.Ordinal);
    }

    [Fact]
    public async Task TranslateAsync_TransitiveSentence_ProducesClosedDraft()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var sentence = fixture.Sentences[0];

        var result = await lane.TranslateAsync(sentence.Text, atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.Closed, result.LaneOutcome);
        Assert.Equal(sentence.ExpectedRoots, result.ResolvedLemmaRoots);
        Assert.Equal("remember(gate,make)", result.DiagnosticPredicateRender);
        Assert.Contains(result.OperatorAnnotations, annotation => annotation.Token == "its");
        Assert.NotNull(result.EngramDraft);
        Assert.Equal("remember", result.EngramDraft!.RootKey);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
    }

    [Fact]
    public async Task TranslateAsync_MeasurementSentence_ProducesClosedDraft()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var sentence = fixture.Sentences[1];

        var result = await lane.TranslateAsync(sentence.Text, atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.Closed, result.LaneOutcome);
        Assert.Equal(sentence.ExpectedRoots, result.ResolvedLemmaRoots);
        Assert.Equal("increase(hum,12,percent)", result.DiagnosticPredicateRender);
        Assert.Contains(result.OperatorAnnotations, annotation => annotation.Token == "has");
        Assert.NotNull(result.EngramDraft);
        Assert.Equal("increase", result.EngramDraft!.RootKey);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
    }

    [Fact]
    public async Task TranslateAsync_ObservationSentence_ProducesClosedDraft()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();

        var result = await lane.TranslateAsync("The Gate observes the hum.", atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.Closed, result.LaneOutcome);
        Assert.Equal(["gate", "observe", "hum"], result.ResolvedLemmaRoots);
        Assert.Equal("observe(gate,hum)", result.DiagnosticPredicateRender);
        Assert.NotNull(result.EngramDraft);
        Assert.Equal("observe", result.EngramDraft!.RootKey);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
    }

    [Fact]
    public async Task TranslateAsync_ChangeSentence_ProducesClosedDraft()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();

        var result = await lane.TranslateAsync("The light changes the Gate.", atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.Closed, result.LaneOutcome);
        Assert.Equal(["light", "change", "gate"], result.ResolvedLemmaRoots);
        Assert.Equal("change(light,gate)", result.DiagnosticPredicateRender);
        Assert.NotNull(result.EngramDraft);
        Assert.Equal("change", result.EngramDraft!.RootKey);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
    }

    [Fact]
    public async Task TranslateAsync_AmbiguousSentence_StopsBeforeValidator()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var sentence = fixture.Sentences[2];

        var result = await lane.TranslateAsync(sentence.Text, atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.NeedsSpecification, result.LaneOutcome);
        Assert.Equal(sentence.ExpectedRoots, result.ResolvedLemmaRoots);
        Assert.Equal("lie(light,first)", result.DiagnosticPredicateRender);
        Assert.Contains(result.OperatorAnnotations, annotation => annotation.Token == "was");
        Assert.Contains(result.OperatorAnnotations, annotation => annotation.Token == "first");
        Assert.Null(result.EngramDraft);
        Assert.Null(result.ClosureDecision);
    }

    [Fact]
    public async Task TranslateAsync_UnsupportedSentence_IsOutOfScope()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();

        var result = await lane.TranslateAsync("The ridge opens at dawn.", atlas, ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(NarrativeTranslationLaneOutcome.OutOfScope, result.LaneOutcome);
        Assert.Empty(result.ResolvedLemmaRoots);
        Assert.Empty(result.OperatorAnnotations);
        Assert.Empty(result.ConstructorBodies);
        Assert.Empty(result.DiagnosticPredicateRender);
        Assert.Null(result.EngramDraft);
        Assert.Null(result.ClosureDecision);
    }

    private static EnglishNarrativeTranslationLane CreateLane()
    {
        return new EnglishNarrativeTranslationLane(
            new EngramClosureValidator(),
            new SLI.Ingestion.OntologicalCleaver(),
            new RootAtlasOntologicalCleaver());
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

    private static FixtureDefinition LoadFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeTranslationFixture.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<FixtureDefinition>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Narrative translation fixture could not be parsed.");
    }

    private static HashSet<string> LoadSeedLemmas()
    {
        var path = ResolveRepoFile("public_root", "seed", "SeedLemmaRoots.json");
        var json = File.ReadAllText(path);
        var document = JsonDocument.Parse(json);
        return document.RootElement
            .EnumerateArray()
            .Select(element => element.GetProperty("lemma").GetString() ?? string.Empty)
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

    private sealed class FixtureDefinition
    {
        public required IReadOnlyList<FixtureSentence> Sentences { get; init; }
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
    }

    private sealed class FixtureSentence
    {
        public required string Text { get; init; }
        public required IReadOnlyList<string> ExpectedRoots { get; init; }
        public required IReadOnlyList<string> ExpectedOperatorTokens { get; init; }
        public required string ExpectedRender { get; init; }
        public required string ExpectedOutcome { get; init; }
        public string? ExpectedClosureGrade { get; init; }
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
