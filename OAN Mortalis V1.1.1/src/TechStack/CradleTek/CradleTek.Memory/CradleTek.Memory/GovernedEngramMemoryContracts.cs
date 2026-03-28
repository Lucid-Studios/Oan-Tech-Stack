namespace CradleTek.Memory;

public sealed record GovernedEngramResolutionContext(
    string ContextHandle,
    string TaskObjective,
    IReadOnlyList<string> RelevantFragments,
    string SourceSubsystem);

public sealed record GovernedEngramQuery(
    string? Concept,
    string? ClusterId,
    int MaxResults,
    IReadOnlyCollection<string> HintTokens)
{
    public GovernedEngramQuery()
        : this(
            Concept: null,
            ClusterId: null,
            MaxResults: 8,
            HintTokens: Array.Empty<string>())
    {
    }
}

public sealed record GovernedEngramSummary(
    string EngramId,
    string ConceptTag,
    string DecisionSpline,
    string SummaryText,
    double ConfidenceWeight);

public sealed record GovernedEngramQueryResult(
    string Source,
    IReadOnlyList<GovernedEngramSummary> Summaries);

public interface IGovernedEngramResolver
{
    Task<GovernedEngramQueryResult> ResolveRelevantAsync(
        GovernedEngramResolutionContext context,
        CancellationToken cancellationToken = default);

    Task<GovernedEngramQueryResult> ResolveConceptAsync(
        string concept,
        CancellationToken cancellationToken = default);

    Task<GovernedEngramQueryResult> ResolveClusterAsync(
        string clusterId,
        CancellationToken cancellationToken = default);

    async Task<GovernedEngramSelfResolutionResult> ResolveSelfSensitiveAsync(
        GovernedEngramResolutionContext context,
        string validationReferenceHandle,
        CancellationToken cancellationToken = default)
    {
        var relevant = await ResolveRelevantAsync(context, cancellationToken).ConfigureAwait(false);
        return GovernedEngramSelfResolutionFactory.CreateFallback(relevant, validationReferenceHandle);
    }
}
