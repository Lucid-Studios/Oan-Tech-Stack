using SLI.Ingestion;

namespace CradleTek.Memory;

public sealed class GovernedEngramResolverService : IGovernedEngramResolver
{
    private readonly IGovernedEngramCorpusSource _corpusSource;
    private readonly IGovernedQueryLexicalCueService _lexicalCueService;

    public GovernedEngramResolverService(
        IGovernedEngramCorpusSource corpusSource,
        IGovernedQueryLexicalCueService? lexicalCueService = null)
    {
        _corpusSource = corpusSource ?? throw new ArgumentNullException(nameof(corpusSource));
        _lexicalCueService = lexicalCueService ?? new GovernedQueryLexicalCueService();
    }

    public Task<GovernedEngramQueryResult> ResolveRelevantAsync(
        GovernedEngramResolutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(context.TaskObjective);
        var lexicalCue = _lexicalCueService.Analyze(context.TaskObjective, context.RelevantFragments);

        var query = new GovernedEngramQuery(
            Concept: null,
            ClusterId: null,
            MaxResults: 8,
            HintTokens: lexicalCue.HintTokens);

        return ResolveByQueryAsync(query, cancellationToken);
    }

    public Task<GovernedEngramQueryResult> ResolveConceptAsync(
        string concept,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concept);
        var lexicalCue = _lexicalCueService.Analyze(concept);

        return ResolveByQueryAsync(
            new GovernedEngramQuery(
                Concept: concept.Trim(),
                ClusterId: null,
                MaxResults: 8,
                HintTokens: lexicalCue.HintTokens),
            cancellationToken);
    }

    public Task<GovernedEngramQueryResult> ResolveClusterAsync(
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterId);

        return ResolveByQueryAsync(
            new GovernedEngramQuery(
                Concept: null,
                ClusterId: clusterId.Trim(),
                MaxResults: 16,
                HintTokens: Array.Empty<string>()),
            cancellationToken);
    }

    public async Task<GovernedEngramSelfResolutionResult> ResolveSelfSensitiveAsync(
        GovernedEngramResolutionContext context,
        string validationReferenceHandle,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(validationReferenceHandle);

        var relevant = await ResolveRelevantAsync(context, cancellationToken).ConfigureAwait(false);
        var lexicalCue = _lexicalCueService.Analyze(context.TaskObjective, context.RelevantFragments);

        var claims = relevant.Summaries
            .Select((summary, index) => CreateSelfSensitiveClaim(
                summary,
                relevant.Source,
                validationReferenceHandle,
                index,
                _lexicalCueService.Analyze(summary.ConceptTag, [summary.SummaryText]).SelfSensitive,
                lexicalCue.SelfSensitive,
                lexicalCue.ContradictionRequested))
            .ToArray();

        return new GovernedEngramSelfResolutionResult(
            Source: relevant.Source,
            Claims: claims);
    }

    private async Task<GovernedEngramQueryResult> ResolveByQueryAsync(
        GovernedEngramQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = await _corpusSource.LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
        var clusterByNodeId = snapshot.Clusters
            .SelectMany(cluster => cluster.NodeIds.Select(nodeId => new { cluster.ClusterId, NodeId = nodeId }))
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.NodeId))
            .GroupBy(static entry => entry.NodeId, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First().ClusterId, StringComparer.Ordinal);

        IEnumerable<GovernedEngramCorpusNode> candidates = snapshot.Nodes;
        if (!string.IsNullOrWhiteSpace(query.Concept))
        {
            var concept = query.Concept.Trim();
            candidates = candidates.Where(node =>
                node.ConceptTag.Equals(concept, StringComparison.OrdinalIgnoreCase) ||
                node.ConceptTag.Contains(concept, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.ClusterId))
        {
            candidates = candidates.Where(node =>
                clusterByNodeId.TryGetValue(node.EngramId, out var clusterId) &&
                clusterId.Equals(query.ClusterId, StringComparison.OrdinalIgnoreCase));
        }

        var ranked = candidates
            .Select(node => new RankedNode(node, ComputeScore(node, query, clusterByNodeId)))
            .Where(static rankedNode => rankedNode.Score > 0.0 || string.IsNullOrWhiteSpace(rankedNode.Node.ConceptTag) is false)
            .OrderByDescending(static rankedNode => rankedNode.Score)
            .ThenByDescending(static rankedNode => rankedNode.Node.StructuralDegree)
            .ThenBy(static rankedNode => rankedNode.Node.ConceptTag, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static rankedNode => rankedNode.Node.EngramId, StringComparer.Ordinal)
            .Take(Math.Max(1, query.MaxResults))
            .ToList();

        if (ranked.Count == 0)
        {
            ranked = snapshot.Nodes
                .OrderByDescending(static node => node.StructuralDegree)
                .ThenBy(static node => node.EngramId, StringComparer.Ordinal)
                .Take(Math.Max(1, query.MaxResults))
                .Select(static node => new RankedNode(node, 0.2))
                .ToList();
        }

        var summaries = ranked
            .Select(rankedNode => new GovernedEngramSummary(
                EngramId: rankedNode.Node.EngramId,
                ConceptTag: rankedNode.Node.ConceptTag,
                DecisionSpline: BuildDecisionSpline(rankedNode.Node, clusterByNodeId),
                SummaryText: BuildSummaryText(rankedNode.Node),
                ConfidenceWeight: Math.Clamp(rankedNode.Score, 0.1, 0.99)))
            .ToArray();

        return new GovernedEngramQueryResult(
            Source: snapshot.Source,
            Summaries: summaries);
    }

    private static double ComputeScore(
        GovernedEngramCorpusNode node,
        GovernedEngramQuery query,
        IReadOnlyDictionary<string, string> clusterByNodeId)
    {
        var score = 0.15;

        if (!string.IsNullOrWhiteSpace(query.Concept) &&
            node.ConceptTag.Equals(query.Concept, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.5;
        }

        foreach (var token in query.HintTokens)
        {
            if (node.ConceptTag.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.18;
            }

            if (node.DomainTag.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.1;
            }
        }

        if (!string.IsNullOrWhiteSpace(query.ClusterId) &&
            clusterByNodeId.TryGetValue(node.EngramId, out var clusterId) &&
            clusterId.Equals(query.ClusterId, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.25;
        }

        score += Math.Min(0.35, node.StructuralDegree / 20.0);
        return score;
    }

    private static string BuildDecisionSpline(
        GovernedEngramCorpusNode node,
        IReadOnlyDictionary<string, string> clusterByNodeId)
    {
        var clusterId = clusterByNodeId.TryGetValue(node.EngramId, out var cluster)
            ? cluster
            : "CLUSTER-UNKNOWN";
        return $"cluster:{clusterId}|concept:{node.ConceptTag}|domain:{node.DomainTag}";
    }

    private static string BuildSummaryText(GovernedEngramCorpusNode node)
    {
        return $"Concept '{node.ConceptTag}' in domain '{node.DomainTag}' with structural degree {node.StructuralDegree}.";
    }

    private static GovernedEngramSelfResolutionClaim CreateSelfSensitiveClaim(
        GovernedEngramSummary summary,
        string source,
        string validationReferenceHandle,
        int index,
        bool summarySelfSensitive,
        bool selfSensitiveQuery,
        bool contradictionRequested)
    {
        var posture = DetermineSelfValidationPosture(index, summarySelfSensitive, selfSensitiveQuery, contradictionRequested);
        var obstructionCode = posture switch
        {
            GovernedEngramSelfValidationPosture.Contradicted => "self-claim-conflict",
            GovernedEngramSelfValidationPosture.Deferred => "deferred-self-admissibility",
            _ => null
        };

        return GovernedEngramSelfResolutionFactory.CreateClaim(
            summary,
            source,
            posture,
            GovernedEngramSelfResolutionOrigin.HotWorkingResolution,
            posture == GovernedEngramSelfValidationPosture.Deferred ? null : validationReferenceHandle,
            obstructionCode);
    }

    private static GovernedEngramSelfValidationPosture DetermineSelfValidationPosture(
        int index,
        bool summarySelfSensitive,
        bool selfSensitiveQuery,
        bool contradictionRequested)
    {
        if (!selfSensitiveQuery)
        {
            return GovernedEngramSelfValidationPosture.Deferred;
        }

        if (contradictionRequested && index == 0)
        {
            return GovernedEngramSelfValidationPosture.Contradicted;
        }

        if (index == 0 || summarySelfSensitive)
        {
            return GovernedEngramSelfValidationPosture.HotClaim;
        }

        return GovernedEngramSelfValidationPosture.Deferred;
    }

    private sealed record RankedNode(GovernedEngramCorpusNode Node, double Score);
}
