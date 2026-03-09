using GEL.Graphs;
using GEL.Models;

namespace SLI.Ingestion;

internal sealed class NarrativeParagraphLaneResult
{
    public required string Paragraph { get; init; }
    public required IReadOnlyList<NarrativeTranslationLaneResult> SentenceResults { get; init; }
    public required ConstructorGraph ParagraphGraph { get; init; }
    public required IReadOnlyList<ConstructorEdge> DiagnosticGraphEdges { get; init; }
    public required IReadOnlyList<EngramDraft> GeneratedDrafts { get; init; }
    public required IReadOnlyList<EngramClosureDecision> ClosureDecisions { get; init; }
}

internal sealed class EnglishNarrativeParagraphLane
{
    private readonly EnglishNarrativeTranslationLane _sentenceLane;

    public EnglishNarrativeParagraphLane(EnglishNarrativeTranslationLane sentenceLane)
    {
        _sentenceLane = sentenceLane ?? throw new ArgumentNullException(nameof(sentenceLane));
    }

    public async Task<NarrativeParagraphLaneResult> TranslateAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        var sentences = SplitParagraph(paragraph);
        var sentenceResults = new List<NarrativeTranslationLaneResult>(sentences.Count);

        foreach (var sentence in sentences)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _sentenceLane.TranslateAsync(sentence, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
            sentenceResults.Add(result);
        }

        var edges = BuildGraphEdges(sentenceResults);
        return new NarrativeParagraphLaneResult
        {
            Paragraph = paragraph,
            SentenceResults = sentenceResults,
            ParagraphGraph = new ConstructorGraph
            {
                Edges = edges
            },
            DiagnosticGraphEdges = edges,
            GeneratedDrafts = sentenceResults
                .Where(result => result.EngramDraft is not null)
                .Select(result => result.EngramDraft!)
                .ToArray(),
            ClosureDecisions = sentenceResults
                .Where(result => result.ClosureDecision is not null)
                .Select(result => result.ClosureDecision!)
                .ToArray()
        };
    }

    private static IReadOnlyList<string> SplitParagraph(string paragraph)
    {
        return paragraph
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(sentence => $"{sentence}.")
            .ToArray();
    }

    private static IReadOnlyList<ConstructorEdge> BuildGraphEdges(IReadOnlyList<NarrativeTranslationLaneResult> sentenceResults)
    {
        var edges = new List<ConstructorEdge>();

        foreach (var sentenceResult in sentenceResults)
        {
            var predicateRoot = ResolvePredicateRoot(sentenceResult);
            if (string.IsNullOrWhiteSpace(predicateRoot))
            {
                continue;
            }

            foreach (var branchBody in sentenceResult.ConstructorBodies.Where(body =>
                         !string.Equals(body.Role, "predicate", StringComparison.OrdinalIgnoreCase)))
            {
                edges.Add(new ConstructorEdge
                {
                    Source = predicateRoot,
                    Target = branchBody.Constructor.RootKey,
                    Relation = branchBody.Role
                });
            }
        }

        for (var index = 0; index < sentenceResults.Count - 1; index++)
        {
            var currentPredicate = ResolvePredicateRoot(sentenceResults[index]);
            var nextPredicate = ResolvePredicateRoot(sentenceResults[index + 1]);
            if (string.IsNullOrWhiteSpace(currentPredicate) || string.IsNullOrWhiteSpace(nextPredicate))
            {
                continue;
            }

            var sharedRoots = sentenceResults[index].ResolvedLemmaRoots
                .Intersect(sentenceResults[index + 1].ResolvedLemmaRoots, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (sharedRoots.Length != 1)
            {
                continue;
            }

            edges.Add(new ConstructorEdge
            {
                Source = currentPredicate,
                Target = nextPredicate,
                Relation = $"continuity:{sharedRoots[0]}"
            });
        }

        return edges;
    }

    private static string? ResolvePredicateRoot(NarrativeTranslationLaneResult sentenceResult)
    {
        if (!string.IsNullOrWhiteSpace(sentenceResult.PredicateRoot))
        {
            return sentenceResult.PredicateRoot;
        }

        return sentenceResult.ConstructorBodies
            .FirstOrDefault(body => string.Equals(body.Role, "predicate", StringComparison.OrdinalIgnoreCase))
            ?.Constructor.RootKey;
    }
}
