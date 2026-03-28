namespace CradleTek.Memory;

public sealed record GovernedEngramCorpusNode(
    string EngramId,
    string ConceptTag,
    string DomainTag,
    IReadOnlyList<string> RelatedNodeIds,
    int StructuralDegree);

public sealed record GovernedEngramCorpusCluster(
    string ClusterId,
    IReadOnlyList<string> NodeIds,
    string ClusterProfile);

public sealed record GovernedEngramCorpusSnapshot(
    string Source,
    string SnapshotProfile,
    IReadOnlyList<GovernedEngramCorpusNode> Nodes,
    IReadOnlyList<GovernedEngramCorpusCluster> Clusters);

public interface IGovernedEngramCorpusSource
{
    ValueTask<GovernedEngramCorpusSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed class InMemoryEngramCorpusSource : IGovernedEngramCorpusSource
{
    private readonly GovernedEngramCorpusSnapshot _snapshot;

    public InMemoryEngramCorpusSource(GovernedEngramCorpusSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public ValueTask<GovernedEngramCorpusSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_snapshot);
    }
}
