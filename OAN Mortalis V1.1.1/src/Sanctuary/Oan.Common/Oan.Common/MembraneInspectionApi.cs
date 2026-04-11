namespace Oan.Common;

public interface IMembraneInspectionApi
{
    bool IsLoaded { get; }

    MembraneInspectionSnapshot Inspect();

    MembraneInspectionLaneView? InspectLane(MembraneDispatchLane lane);
}

public sealed record MembraneInspectionDecisionSummary(
    MembraneDecision Decision,
    int Count);

public sealed record MembraneInspectionLaneView(
    MembraneDispatchLane Lane,
    int HeldCount,
    int ReceiptCount,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    IReadOnlyList<MembraneInspectionDecisionSummary> DecisionSummaries,
    DateTimeOffset? LatestReceivedAt);

public sealed record MembraneInspectionSnapshot(
    string WitnessSnapshotId,
    DateTimeOffset ObservedAt,
    IReadOnlyList<MembraneInspectionLaneView> Lanes);

public sealed class DefaultMembraneInspectionApi : IMembraneInspectionApi
{
    private readonly IMembraneLaneReadModel _readModel;

    public DefaultMembraneInspectionApi(IMembraneLaneReadModel readModel)
    {
        _readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
    }

    public bool IsLoaded => _readModel.HasSnapshot;

    public MembraneInspectionSnapshot Inspect()
    {
        var snapshot = _readModel.Read();

        return new MembraneInspectionSnapshot(
            WitnessSnapshotId: snapshot.WitnessSnapshotId,
            ObservedAt: snapshot.ObservedAt,
            Lanes: snapshot.Entries
                .Select(CreateLaneView)
                .ToArray());
    }

    public MembraneInspectionLaneView? InspectLane(MembraneDispatchLane lane)
    {
        var entry = _readModel.ReadLane(lane);
        return entry is null ? null : CreateLaneView(entry);
    }

    private static MembraneInspectionLaneView CreateLaneView(MembraneLaneReadModelEntry entry)
    {
        return new MembraneInspectionLaneView(
            Lane: entry.Lane,
            HeldCount: entry.HeldCount,
            ReceiptCount: entry.ReceiptCount,
            RecentDispatchIds: entry.RecentDispatchIds.ToArray(),
            RecentTraceIds: entry.RecentTraceIds.ToArray(),
            DecisionSummaries: entry.DecisionSummaries
                .Select(static summary => new MembraneInspectionDecisionSummary(
                    Decision: summary.Decision,
                    Count: summary.Count))
                .ToArray(),
            LatestReceivedAt: entry.LatestReceivedAt);
    }
}
