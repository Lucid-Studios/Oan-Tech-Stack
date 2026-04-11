namespace San.Common;

public interface IMembraneLaneReadModel
{
    bool HasSnapshot { get; }

    void Refresh(MembraneLaneWitnessSnapshot witnessSnapshot);

    MembraneLaneReadModelSnapshot Read();

    MembraneLaneReadModelEntry? ReadLane(MembraneDispatchLane lane);
}

public sealed record MembraneLaneDecisionSummary(
    MembraneDecision Decision,
    int Count);

public sealed record MembraneLaneReadModelEntry(
    MembraneDispatchLane Lane,
    int HeldCount,
    int ReceiptCount,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    IReadOnlyList<MembraneLaneDecisionSummary> DecisionSummaries,
    DateTimeOffset? LatestReceivedAt);

public sealed record MembraneLaneReadModelSnapshot(
    string WitnessSnapshotId,
    DateTimeOffset ObservedAt,
    IReadOnlyList<MembraneLaneReadModelEntry> Entries);

public sealed class DefaultMembraneLaneReadModel : IMembraneLaneReadModel
{
    private readonly int _recentItemLimit;
    private MembraneLaneReadModelSnapshot? _currentSnapshot;

    public DefaultMembraneLaneReadModel(int recentItemLimit = 5)
    {
        if (recentItemLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recentItemLimit), recentItemLimit, "recent item limit must be greater than zero.");
        }

        _recentItemLimit = recentItemLimit;
    }

    public bool HasSnapshot => _currentSnapshot is not null;

    public void Refresh(MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        ArgumentNullException.ThrowIfNull(witnessSnapshot);

        if (string.IsNullOrWhiteSpace(witnessSnapshot.SnapshotId))
        {
            throw new InvalidOperationException("membrane lane read model requires an explicit witness snapshot id.");
        }

        if (witnessSnapshot.Entries.Any(static entry => entry is null))
        {
            throw new InvalidOperationException("membrane lane read model may not consume a witness snapshot with null entries.");
        }

        var duplicateLane = witnessSnapshot.Entries
            .GroupBy(static entry => entry!.Lane)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateLane is not null)
        {
            throw new InvalidOperationException(
                $"membrane lane read model requires unique witness lanes; duplicate lane '{duplicateLane.Key}' was provided.");
        }

        var entries = witnessSnapshot.Entries
            .Select(static entry => entry!)
            .OrderBy(static entry => entry.Lane)
            .Select(entry => new MembraneLaneReadModelEntry(
                Lane: entry.Lane,
                HeldCount: entry.HeldCount,
                ReceiptCount: entry.ReceiptCount,
                RecentDispatchIds: entry.DispatchIds.TakeLast(_recentItemLimit).ToArray(),
                RecentTraceIds: entry.TraceIds.TakeLast(_recentItemLimit).ToArray(),
                DecisionSummaries: entry.Decisions
                    .GroupBy(static decision => decision)
                    .OrderBy(static group => group.Key)
                    .Select(static group => new MembraneLaneDecisionSummary(
                        Decision: group.Key,
                        Count: group.Count()))
                    .ToArray(),
                LatestReceivedAt: entry.LatestReceivedAt))
            .ToArray();

        _currentSnapshot = new MembraneLaneReadModelSnapshot(
            WitnessSnapshotId: witnessSnapshot.SnapshotId,
            ObservedAt: witnessSnapshot.ObservedAt,
            Entries: entries);
    }

    public MembraneLaneReadModelSnapshot Read()
    {
        return _currentSnapshot ?? throw new InvalidOperationException(
            "membrane lane read model has not yet consumed a witness snapshot.");
    }

    public MembraneLaneReadModelEntry? ReadLane(MembraneDispatchLane lane)
    {
        return Read().Entries.FirstOrDefault(entry => entry.Lane == lane);
    }
}
