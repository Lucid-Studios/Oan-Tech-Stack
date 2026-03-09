using GEL.Graphs;
using GEL.Models;

namespace SLI.Ingestion;

internal sealed class NarrativeDraftCluster
{
    public required IReadOnlyList<EngramDraft> MemberDrafts { get; init; }
    public required IReadOnlyList<EngramClosureDecision> MemberClosureDecisions { get; init; }
    public required IReadOnlyList<string> AmbiguousSentenceKeys { get; init; }
    public required string ClusterDiagnosticRender { get; init; }
}

internal sealed class NarrativeParagraphBody
{
    public required string Paragraph { get; init; }
    public required IReadOnlyList<NarrativeTranslationLaneResult> SentenceResults { get; init; }
    public required ConstructorGraph ParagraphGraph { get; init; }
    public required IReadOnlyList<string> ParagraphInvariants { get; init; }
    public required IReadOnlyList<string> ContinuityAnchors { get; init; }
    public required string BodySummary { get; init; }
    public required NarrativeDraftCluster DraftCluster { get; init; }
}

internal sealed class EnglishNarrativeParagraphBodyLane
{
    private readonly EnglishNarrativeParagraphLane _paragraphLane;

    public EnglishNarrativeParagraphBodyLane(EnglishNarrativeParagraphLane paragraphLane)
    {
        _paragraphLane = paragraphLane ?? throw new ArgumentNullException(nameof(paragraphLane));
    }

    public async Task<NarrativeParagraphBody> TranslateAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        var paragraphResult = await _paragraphLane.TranslateAsync(paragraph, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
        if (!ParagraphSpecifications.TryGetValue(paragraph.Trim(), out var specification))
        {
            throw new InvalidOperationException("Paragraph body lane was called with an unsupported proof paragraph.");
        }

        var continuityAnchors = paragraphResult.ParagraphGraph.Edges
            .Where(edge => edge.Relation.StartsWith("continuity:", StringComparison.Ordinal))
            .Select(edge => edge.Relation["continuity:".Length..])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var ambiguousSentenceKeys = paragraphResult.SentenceResults
            .Where(result => result.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification)
            .Select(result => result.DiagnosticPredicateRender)
            .ToArray();

        var clusterEntries = paragraphResult.SentenceResults
            .Select(result => result.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification
                ? $"ambiguous:{result.DiagnosticPredicateRender}"
                : result.DiagnosticPredicateRender)
            .ToArray();

        var cluster = new NarrativeDraftCluster
        {
            MemberDrafts = paragraphResult.GeneratedDrafts,
            MemberClosureDecisions = paragraphResult.ClosureDecisions,
            AmbiguousSentenceKeys = ambiguousSentenceKeys,
            ClusterDiagnosticRender = $"cluster[{string.Join("; ", clusterEntries)}]"
        };

        var invariants = BuildBodyInvariants(paragraphResult, continuityAnchors, specification);

        return new NarrativeParagraphBody
        {
            Paragraph = paragraphResult.Paragraph,
            SentenceResults = paragraphResult.SentenceResults,
            ParagraphGraph = paragraphResult.ParagraphGraph,
            ParagraphInvariants = invariants,
            ContinuityAnchors = continuityAnchors,
            BodySummary = BuildBodySummary(continuityAnchors, paragraphResult.GeneratedDrafts.Count, ambiguousSentenceKeys.Length),
            DraftCluster = cluster
        };
    }

    private static IReadOnlyList<string> BuildBodyInvariants(
        NarrativeParagraphLaneResult paragraphResult,
        IReadOnlyList<string> continuityAnchors,
        NarrativeParagraphSpecification specification)
    {
        var invariants = new List<string>();

        invariants.AddRange(continuityAnchors.Select(anchor => $"paragraph.continuity.root:{anchor}"));
        invariants.AddRange(specification.FixedProofLabels);

        var hasMeasurement = paragraphResult.SentenceResults.Any(result =>
            result.EngramDraft?.Invariants.Any(invariant => invariant.Key == "narrative.scalar.payload") == true);
        if (hasMeasurement)
        {
            invariants.Add("paragraph.measurement.present");
        }

        var ambiguousCount = paragraphResult.SentenceResults.Count(result =>
            result.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification);
        if (ambiguousCount > 0)
        {
            invariants.Add("paragraph.ambiguity.present");
        }

        invariants.Add($"paragraph.closed.draft.count:{paragraphResult.GeneratedDrafts.Count}");

        if (ambiguousCount > 0)
        {
            invariants.Add($"paragraph.ambiguous.sentence.count:{ambiguousCount}");
        }

        return invariants;
    }

    private static string BuildBodySummary(IReadOnlyList<string> continuityAnchors, int closedDraftCount, int ambiguousCount)
    {
        var anchorSummary = continuityAnchors.Count == 0
            ? "none"
            : string.Join(",", continuityAnchors);

        return $"body[anchors:{anchorSummary}; closed:{closedDraftCount}; ambiguous:{ambiguousCount}]";
    }

    private static readonly IReadOnlyDictionary<string, NarrativeParagraphSpecification> ParagraphSpecifications =
        new Dictionary<string, NarrativeParagraphSpecification>(StringComparer.Ordinal)
        {
            ["The dome hummed. The hum reached the hologram. The hologram pulsed in rhythm."] = new()
            {
                FixedProofLabels = ["paragraph.environment.present", "paragraph.state.field"]
            },
            ["The hum has increased twelve percent. The ridge resonates with the hum. The activity rises along the ridge."] = new()
            {
                FixedProofLabels = Array.Empty<string>()
            },
            ["The Gate remembers its makers. The light was the first lie."] = new()
            {
                FixedProofLabels = Array.Empty<string>()
            }
        };

    private sealed class NarrativeParagraphSpecification
    {
        public required IReadOnlyList<string> FixedProofLabels { get; init; }
    }
}
