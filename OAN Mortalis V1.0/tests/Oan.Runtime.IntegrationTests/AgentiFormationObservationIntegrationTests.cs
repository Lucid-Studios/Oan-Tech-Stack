using System.Text.Json;
using AgentiCore.Observation;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;
using Oan.Common;
using Oan.Cradle;
using IngestionOntologicalCleaver = SLI.Ingestion.OntologicalCleaver;
using SLI.Ingestion;

namespace Oan.Runtime.IntegrationTests;

public sealed class AgentiFormationObservationIntegrationTests
{
    [Fact]
    public async Task Chamber_AdmitSentence_RecordsAdmissionAndClosedClosure()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer);

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[0].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Admit, result.AdmissionResult.Decision);
        Assert.NotNull(result.ClosureDecision);

        var snapshot = observer.Snapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Equal(AgentiFormationObservationStage.CrypticAdmission, snapshot[0].Stage);
        Assert.Equal(CrypticAdmissionDecision.Admit, snapshot[0].AdmissionDecision);
        Assert.Equal(AgentiFormationObservationSource.Sentence, snapshot[0].Source);
        Assert.True(snapshot[0].SubmissionEligible);

        Assert.Equal(AgentiFormationObservationStage.PrimeClosure, snapshot[1].Stage);
        Assert.Equal(AgentiFormationClosureState.Closed, snapshot[1].ClosureState);
        Assert.Equal(snapshot[0].CandidateId, snapshot[1].CandidateId);
        Assert.True(snapshot[1].SubmissionEligible);
    }

    [Fact]
    public async Task Chamber_DeferSentence_RecordsAdmissionAndNoClosure()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer);

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[2].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Defer, result.AdmissionResult.Decision);
        Assert.Null(result.ClosureDecision);

        var snapshot = observer.Snapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Equal(CrypticAdmissionDecision.Defer, snapshot[0].AdmissionDecision);
        Assert.False(snapshot[0].SubmissionEligible);
        Assert.Equal(AgentiFormationClosureState.NoClosure, snapshot[1].ClosureState);
        Assert.False(snapshot[1].SubmissionEligible);
    }

    [Fact]
    public async Task Chamber_RejectSentence_RecordsAdmissionAndNoClosure()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer);

        var result = await chamber.FormSentenceAsync(
            "The ridge opens at dawn.",
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Reject, result.AdmissionResult.Decision);
        Assert.Null(result.ClosureDecision);

        var snapshot = observer.Snapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Equal(CrypticAdmissionDecision.Reject, snapshot[0].AdmissionDecision);
        Assert.Equal(AgentiFormationClosureState.NoClosure, snapshot[1].ClosureState);
        Assert.False(snapshot[1].SubmissionEligible);
    }

    [Fact]
    public async Task Chamber_QuarantineSentence_RecordsAdmissionAndNoClosure()
    {
        var fixture = LoadTranslationFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer, new QuarantineMembrane());

        var result = await chamber.FormSentenceAsync(
            fixture.Sentences[0].Text,
            atlas,
            ToOverlayRoots(fixture.OverlayRoots));

        Assert.Equal(CrypticAdmissionDecision.Quarantine, result.AdmissionResult.Decision);
        Assert.Null(result.ClosureDecision);

        var snapshot = observer.Snapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Equal(CrypticAdmissionDecision.Quarantine, snapshot[0].AdmissionDecision);
        Assert.Equal(AgentiFormationClosureState.NoClosure, snapshot[1].ClosureState);
        Assert.False(snapshot[0].SubmissionEligible);
        Assert.False(snapshot[1].SubmissionEligible);
    }

    [Fact]
    public async Task ParagraphGraph_ObserverDoesNotChangeOracleOutput()
    {
        var fixture = LoadParagraphFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer);
        var oracle = CreateOracleParagraphLane();

        var chamberResult = await chamber.FormParagraphAsync(fixture.Paragraph, atlas, overlayRoots);
        var oracleResult = await oracle.TranslateAsync(fixture.Paragraph, atlas, overlayRoots);

        AssertEdgeParity(oracleResult.DiagnosticGraphEdges, chamberResult.ParagraphResult.DiagnosticGraphEdges);
        Assert.Equal(oracleResult.GeneratedDrafts.Count, chamberResult.ParagraphResult.GeneratedDrafts.Count);
        Assert.Equal(oracleResult.ClosureDecisions.Count, chamberResult.ParagraphResult.ClosureDecisions.Count);

        var snapshot = observer.Snapshot();
        Assert.Equal(oracleResult.SentenceResults.Count * 2, snapshot.Count);
        Assert.All(snapshot, observation => Assert.Equal(AgentiFormationObservationSource.ParagraphGraph, observation.Source));
        Assert.DoesNotContain(snapshot, observation => observation.Stage == AgentiFormationObservationStage.BootClassification);
    }

    [Fact]
    public async Task ParagraphBody_ObserverDoesNotChangeOracleOutput()
    {
        var fixture = LoadParagraphBodyFixture();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = ToOverlayRoots(fixture.OverlayRoots);
        var observer = new InMemoryAgentiFormationObserver();
        var chamber = CreateChamber(observer);
        var oracle = CreateOracleBodyLane();
        var paragraph = fixture.Paragraphs[2].Paragraph;

        var chamberResult = await chamber.FormParagraphBodyAsync(paragraph, atlas, overlayRoots);
        var oracleResult = await oracle.TranslateAsync(paragraph, atlas, overlayRoots);

        Assert.Equal(oracleResult.ContinuityAnchors, chamberResult.ParagraphBody.ContinuityAnchors);
        Assert.Equal(oracleResult.ParagraphInvariants, chamberResult.ParagraphBody.ParagraphInvariants);
        Assert.Equal(oracleResult.BodySummary, chamberResult.ParagraphBody.BodySummary);
        Assert.Equal(oracleResult.DraftCluster.ClusterDiagnosticRender, chamberResult.ParagraphBody.DraftCluster.ClusterDiagnosticRender);

        var snapshot = observer.Snapshot();
        Assert.Equal(oracleResult.SentenceResults.Count * 2, snapshot.Count);
        Assert.All(snapshot, observation => Assert.Equal(AgentiFormationObservationSource.ParagraphBody, observation.Source));
        Assert.Contains(snapshot, observation =>
            observation.Stage == AgentiFormationObservationStage.CrypticAdmission &&
            observation.AdmissionDecision == CrypticAdmissionDecision.Defer);
        Assert.Contains(snapshot, observation =>
            observation.Stage == AgentiFormationObservationStage.PrimeClosure &&
            observation.ClosureState == AgentiFormationClosureState.NoClosure);
    }

    private static CrypticFormationChamber CreateChamber(
        IAgentiFormationObserver observer,
        ICrypticAdmissionMembrane? admissionMembrane = null)
    {
        var validator = new EngramClosureValidator();
        return new CrypticFormationChamber(
            validator,
            admissionMembrane ?? new CrypticAdmissionMembrane(),
            formationObserver: observer);
    }

    private static EnglishNarrativeParagraphLane CreateOracleParagraphLane()
    {
        return new EnglishNarrativeParagraphLane(CreateOracleSentenceLane());
    }

    private static EnglishNarrativeParagraphBodyLane CreateOracleBodyLane()
    {
        return new EnglishNarrativeParagraphBodyLane(CreateOracleParagraphLane());
    }

    private static EnglishNarrativeTranslationLane CreateOracleSentenceLane()
    {
        return new EnglishNarrativeTranslationLane(
            new EngramClosureValidator(),
            new IngestionOntologicalCleaver(),
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

    private sealed class QuarantineMembrane : ICrypticAdmissionMembrane
    {
        public Task<CrypticAdmissionResult> EvaluateAsync(
            CrypticAdmissionCandidate candidate,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CrypticAdmissionResult(
                Decision: CrypticAdmissionDecision.Quarantine,
                ReasonCode: "test-quarantine",
                CandidateId: candidate.CandidateId,
                OriginRuntime: candidate.OriginRuntime,
                OriginLane: candidate.OriginLane,
                SubmissionEligible: false,
                RequiresReview: true,
                TelemetryTags: ["test:quarantine"],
                NormalizedPrimePayload: null));
        }
    }
}
