using System.Text.Json;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;
using Oan.Common;
using Oan.Cradle;
using SLI.Ingestion;

namespace Oan.Runtime.IntegrationTests;

public sealed class CrypticAdmissionMembraneIntegrationTests
{
    [Fact]
    public async Task SentenceAdmission_AdmitsClosedNarrativeCandidate()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var chamber = CreateChamber();

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[0].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Admit, result.AdmissionResult.Decision);
        Assert.True(result.AdmissionResult.SubmissionEligible);
        Assert.NotNull(result.AdmissionResult.NormalizedPrimePayload);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
        Assert.Equal(NarrativeTranslationLaneOutcome.Closed, result.SentenceResult.LaneOutcome);
        Assert.Equal("remember(gate,make)", result.SentenceResult.DiagnosticPredicateRender);
    }

    [Fact]
    public async Task SentenceAdmission_AdmitsMeasurementNarrativeCandidate()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var chamber = CreateChamber();

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[1].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Admit, result.AdmissionResult.Decision);
        Assert.True(result.AdmissionResult.SubmissionEligible);
        Assert.NotNull(result.ClosureDecision);
        Assert.Equal(EngramClosureGrade.Closed, result.ClosureDecision!.Grade);
        Assert.Equal("increase(hum,12,percent)", result.SentenceResult.DiagnosticPredicateRender);
    }

    [Fact]
    public async Task SentenceAdmission_DefersNeedsSpecificationCandidate()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var chamber = CreateChamber();

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[2].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Defer, result.AdmissionResult.Decision);
        Assert.False(result.AdmissionResult.SubmissionEligible);
        Assert.True(result.AdmissionResult.RequiresReview);
        Assert.Null(result.AdmissionResult.NormalizedPrimePayload);
        Assert.Null(result.ClosureDecision);
        Assert.Equal(NarrativeTranslationLaneOutcome.NeedsSpecification, result.SentenceResult.LaneOutcome);
    }

    [Fact]
    public async Task SentenceAdmission_RejectsUnsupportedInput()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var chamber = CreateChamber();

        var result = await chamber.FormSentenceAsync(
            "The ridge opens at dawn.",
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Reject, result.AdmissionResult.Decision);
        Assert.False(result.AdmissionResult.SubmissionEligible);
        Assert.False(result.AdmissionResult.RequiresReview);
        Assert.Null(result.AdmissionResult.NormalizedPrimePayload);
        Assert.Null(result.ClosureDecision);
        Assert.Equal(NarrativeTranslationLaneOutcome.OutOfScope, result.SentenceResult.LaneOutcome);
    }

    [Fact]
    public async Task Membrane_QuarantinesPolicyUnsafeCandidate()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var oracleSentence = await CreateOracleSentenceLane().TranslateAsync(
            fixture.Sentences[0].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        var membrane = new CrypticAdmissionMembrane();
        var candidate = new CrypticAdmissionCandidate(
            Guid.NewGuid(),
            CrypticOriginRuntime.Lisp,
            CrypticOriginLane.Sentence,
            fixture.Sentences[0].Text,
            oracleSentence,
            oracleSentence.EngramDraft,
            CrypticFormationOutcome.Closed,
            DeterministicPrimeMaterializationSucceeded: true,
            ReservedDomainViolation: true,
            DiagnosticRender: oracleSentence.DiagnosticPredicateRender,
            TelemetryTags: ["runtime:Lisp", "lane:sentence", "outcome:Closed"]);

        var result = await membrane.EvaluateAsync(candidate);

        Assert.Equal(CrypticAdmissionDecision.Quarantine, result.Decision);
        Assert.False(result.SubmissionEligible);
        Assert.True(result.RequiresReview);
        Assert.Null(result.NormalizedPrimePayload);
        Assert.Contains("reason:reserved-domain-violation", result.TelemetryTags);
    }

    [Fact]
    public async Task ParagraphGraph_AggregatesSentenceAdmissionWithoutChangingGraph()
    {
        var fixture = LoadParagraphFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var chamber = CreateChamber();
        var oracle = CreateOracleParagraphLane();

        var chamberResult = await chamber.FormParagraphAsync(fixture.Paragraph, atlas, overlayRoots);
        var oracleResult = await oracle.TranslateAsync(fixture.Paragraph, atlas, overlayRoots);

        AssertEdgeParity(oracleResult.DiagnosticGraphEdges, chamberResult.ParagraphResult.DiagnosticGraphEdges);
        Assert.Equal(oracleResult.SentenceResults.Count, chamberResult.ParagraphResult.SentenceResults.Count);
        Assert.Equal(4, chamberResult.SentenceAdmissions.Count(admission => admission.AdmissionResult.Decision == CrypticAdmissionDecision.Admit));
        Assert.Equal(1, chamberResult.SentenceAdmissions.Count(admission => admission.AdmissionResult.Decision == CrypticAdmissionDecision.Defer));
        Assert.Equal(4, chamberResult.ParagraphResult.ClosureDecisions.Count);
    }

    [Fact]
    public async Task ParagraphBody_AggregatesSentenceAdmissionWithoutChangingBody()
    {
        var fixture = LoadParagraphBodyFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var chamber = CreateChamber();
        var oracle = CreateOracleBodyLane();

        foreach (var paragraph in fixture.Paragraphs)
        {
            var chamberResult = await chamber.FormParagraphBodyAsync(paragraph.Paragraph, atlas, overlayRoots);
            var oracleResult = await oracle.TranslateAsync(paragraph.Paragraph, atlas, overlayRoots);

            Assert.Equal(oracleResult.ContinuityAnchors, chamberResult.ParagraphBody.ContinuityAnchors);
            Assert.Equal(oracleResult.ParagraphInvariants, chamberResult.ParagraphBody.ParagraphInvariants);
            Assert.Equal(oracleResult.BodySummary, chamberResult.ParagraphBody.BodySummary);
            Assert.Equal(oracleResult.DraftCluster.ClusterDiagnosticRender, chamberResult.ParagraphBody.DraftCluster.ClusterDiagnosticRender);
            Assert.Equal(oracleResult.DraftCluster.MemberClosureDecisions.Count, chamberResult.ParagraphBody.DraftCluster.MemberClosureDecisions.Count);
            Assert.Equal(
                oracleResult.SentenceResults.Count(result => result.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification),
                chamberResult.SentenceAdmissions.Count(result => result.AdmissionResult.Decision == CrypticAdmissionDecision.Defer));
        }
    }

    private static CrypticFormationChamber CreateChamber()
    {
        var validator = new EngramClosureValidator();
        return new CrypticFormationChamber(validator, new CrypticAdmissionMembrane());
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
    }

    private sealed class FixtureSentence
    {
        public required string Text { get; init; }
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
