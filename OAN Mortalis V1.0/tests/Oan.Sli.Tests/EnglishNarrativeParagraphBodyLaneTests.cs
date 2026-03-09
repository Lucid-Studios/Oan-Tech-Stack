using System.Text.Json;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Models;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class EnglishNarrativeParagraphBodyLaneTests
{
    [Fact]
    public void Fixture_IsWellFormedAndOverlayIsBounded()
    {
        var fixture = LoadFixture();

        Assert.Equal(3, fixture.Paragraphs.Count);
        Assert.Equal([3, 3, 2], fixture.Paragraphs.Select(paragraph => paragraph.ExpectedSentenceCount).ToArray());
        Assert.Equal(17, fixture.OverlayRoots.Count);
        Assert.Equal(
            ["gate", "remember", "make", "hum", "percent", "light", "lie", "dome", "vibrate", "hologram", "pulse", "rhythm", "ridge", "resonate", "activity", "subsurface", "rise"],
            fixture.OverlayRoots.Select(root => root.Lemma).ToArray());
    }

    [Fact]
    public async Task TranslateAsync_FormsExpectedBodyRecords()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);

        foreach (var expected in fixture.Paragraphs)
        {
            var result = await lane.TranslateAsync(expected.Paragraph, atlas, overlayRoots);

            Assert.Equal(expected.Paragraph, result.Paragraph);
            Assert.Equal(expected.ExpectedSentenceCount, result.SentenceResults.Count);
            Assert.Equal(expected.ExpectedContinuityAnchors, result.ContinuityAnchors);
            Assert.Equal(expected.ExpectedInvariants, result.ParagraphInvariants);
            Assert.Equal(expected.ExpectedClusterRender, result.DraftCluster.ClusterDiagnosticRender);
            Assert.Equal(expected.ExpectedBodySummary, result.BodySummary);
            Assert.Equal(expected.ExpectedClosedDraftCount, result.DraftCluster.MemberDrafts.Count);
            Assert.Equal(expected.ExpectedClosedDraftCount, result.DraftCluster.MemberClosureDecisions.Count);
            Assert.Equal(expected.ExpectedAmbiguousSentenceCount, result.DraftCluster.AmbiguousSentenceKeys.Count);

            Assert.All(result.DraftCluster.MemberClosureDecisions, decision =>
                Assert.Equal(EngramClosureGrade.Closed, decision.Grade));
        }
    }

    [Fact]
    public async Task TranslateAsync_KeepsAmbiguityLocalAndDoesNotCreateParagraphDraft()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var paragraph = fixture.Paragraphs[2];

        var result = await lane.TranslateAsync(paragraph.Paragraph, atlas, overlayRoots);

        Assert.Single(result.DraftCluster.MemberDrafts);
        Assert.Single(result.DraftCluster.MemberClosureDecisions);
        Assert.Single(result.DraftCluster.AmbiguousSentenceKeys);
        Assert.Equal("lie(light,first)", result.DraftCluster.AmbiguousSentenceKeys[0]);
        Assert.Contains(result.ParagraphGraph.Edges, edge => edge.Source == "lie" && edge.Target == "light" && edge.Relation == "subject");
    }

    [Fact]
    public async Task TranslateAsync_UsesGraphProvenContinuityWithoutInventingExtraAnchors()
    {
        var fixture = LoadFixture();
        var lane = CreateLane();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);

        var paragraphA = await lane.TranslateAsync(fixture.Paragraphs[0].Paragraph, atlas, overlayRoots);
        var paragraphB = await lane.TranslateAsync(fixture.Paragraphs[1].Paragraph, atlas, overlayRoots);

        Assert.Empty(paragraphA.ContinuityAnchors);
        Assert.DoesNotContain(paragraphA.ParagraphInvariants, value => value.StartsWith("paragraph.continuity.root:", StringComparison.Ordinal));

        Assert.Equal(["activity"], paragraphB.ContinuityAnchors);
        Assert.Equal(
            ["paragraph.continuity.root:activity"],
            paragraphB.ParagraphInvariants.Where(value => value.StartsWith("paragraph.continuity.root:", StringComparison.Ordinal)).ToArray());
    }

    private static EnglishNarrativeParagraphBodyLane CreateLane()
    {
        var sentenceLane = new EnglishNarrativeTranslationLane(
            new EngramClosureValidator(),
            new SLI.Ingestion.OntologicalCleaver(),
            new RootAtlasOntologicalCleaver());
        var paragraphLane = new EnglishNarrativeParagraphLane(sentenceLane);
        return new EnglishNarrativeParagraphBodyLane(paragraphLane);
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

    private static ParagraphBodyFixtureDefinition LoadFixture()
    {
        var path = ResolveRepoFile("tests", "Oan.Sli.Tests", "fixtures", "FirstNarrativeParagraphBodyFixture.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ParagraphBodyFixtureDefinition>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Narrative paragraph body fixture could not be parsed.");
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
