namespace Oan.Common;

public interface IMembraneLaneWitness
{
    MembraneLaneWitnessSnapshot Observe(IEnumerable<IMembraneLaneSink> laneSinks);
}

public sealed record MembraneLaneWitnessEntry(
    MembraneDispatchLane Lane,
    int HeldCount,
    int ReceiptCount,
    IReadOnlyList<string> DispatchIds,
    IReadOnlyList<string> TraceIds,
    IReadOnlyList<MembraneDecision> Decisions,
    DateTimeOffset? LatestReceivedAt);

public sealed record MembraneLaneWitnessSnapshot(
    string SnapshotId,
    DateTimeOffset ObservedAt,
    IReadOnlyList<MembraneLaneWitnessEntry> Entries);

public sealed class DefaultMembraneLaneWitness : IMembraneLaneWitness
{
    private readonly Func<DateTimeOffset> _clock;
    private readonly Func<string> _snapshotIdFactory;

    public DefaultMembraneLaneWitness()
        : this(
            () => DateTimeOffset.UtcNow,
            () => $"witness://membrane/{Guid.NewGuid():N}")
    {
    }

    public DefaultMembraneLaneWitness(
        Func<DateTimeOffset> clock,
        Func<string> snapshotIdFactory)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _snapshotIdFactory = snapshotIdFactory ?? throw new ArgumentNullException(nameof(snapshotIdFactory));
    }

    public MembraneLaneWitnessSnapshot Observe(IEnumerable<IMembraneLaneSink> laneSinks)
    {
        ArgumentNullException.ThrowIfNull(laneSinks);

        var sinkList = laneSinks.ToList();

        if (sinkList.Any(static sink => sink is null))
        {
            throw new InvalidOperationException("membrane lane witness may not observe a null lane sink.");
        }

        var duplicateLane = sinkList
            .GroupBy(static sink => sink!.Lane)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateLane is not null)
        {
            throw new InvalidOperationException(
                $"membrane lane witness requires unique lane sinks; duplicate lane '{duplicateLane.Key}' was provided.");
        }

        var snapshotId = _snapshotIdFactory();

        if (string.IsNullOrWhiteSpace(snapshotId))
        {
            throw new InvalidOperationException("membrane lane witness must emit an explicit snapshot id.");
        }

        var entries = sinkList
            .Select(static sink => sink!)
            .OrderBy(static sink => sink.Lane)
            .Select(CreateEntry)
            .ToArray();

        return new MembraneLaneWitnessSnapshot(
            SnapshotId: snapshotId,
            ObservedAt: _clock(),
            Entries: entries);
    }

    private static MembraneLaneWitnessEntry CreateEntry(IMembraneLaneSink sink)
    {
        return new MembraneLaneWitnessEntry(
            Lane: sink.Lane,
            HeldCount: sink.HeldDispatches.Count,
            ReceiptCount: sink.Receipts.Count,
            DispatchIds: sink.HeldDispatches.Select(static dispatch => dispatch.DispatchId).ToArray(),
            TraceIds: sink.HeldDispatches.Select(static dispatch => dispatch.DecisionResult.Envelope.TraceId).ToArray(),
            Decisions: sink.HeldDispatches.Select(static dispatch => dispatch.DecisionResult.Decision).ToArray(),
            LatestReceivedAt: sink.Receipts.Count == 0
                ? null
                : sink.Receipts.Max(static receipt => receipt.ReceivedAt));
    }
}
