namespace San.Common;

public interface IMembraneViewProjection
{
    MembraneViewModel Project();

    MembraneLaneViewModel? ProjectLane(MembraneDispatchLane lane);
}

public sealed record MembraneDecisionSummaryViewModel(
    MembraneDecision Decision,
    string DecisionLabel,
    int Count);

public sealed record MembraneLaneViewModel(
    MembraneDispatchLane Lane,
    string LaneLabel,
    int HeldCount,
    int ReceiptCount,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    IReadOnlyList<MembraneDecisionSummaryViewModel> DecisionSummaries,
    DateTimeOffset? LatestReceivedAt);

public sealed record MembraneViewModel(
    bool IsLoaded,
    string StatusLabel,
    string? WitnessSnapshotId,
    DateTimeOffset? ObservedAt,
    int TotalHeldCount,
    int TotalReceiptCount,
    IReadOnlyList<MembraneLaneViewModel> Lanes);

public sealed class DefaultMembraneViewProjection : IMembraneViewProjection
{
    private readonly IMembraneInspectionApi _inspectionApi;

    public DefaultMembraneViewProjection(IMembraneInspectionApi inspectionApi)
    {
        _inspectionApi = inspectionApi ?? throw new ArgumentNullException(nameof(inspectionApi));
    }

    public MembraneViewModel Project()
    {
        if (!_inspectionApi.IsLoaded)
        {
            return new MembraneViewModel(
                IsLoaded: false,
                StatusLabel: "Unloaded",
                WitnessSnapshotId: null,
                ObservedAt: null,
                TotalHeldCount: 0,
                TotalReceiptCount: 0,
                Lanes: []);
        }

        var snapshot = _inspectionApi.Inspect();
        var lanes = snapshot.Lanes
            .Select(CreateLaneViewModel)
            .ToArray();

        return new MembraneViewModel(
            IsLoaded: true,
            StatusLabel: "Loaded",
            WitnessSnapshotId: snapshot.WitnessSnapshotId,
            ObservedAt: snapshot.ObservedAt,
            TotalHeldCount: lanes.Sum(static lane => lane.HeldCount),
            TotalReceiptCount: lanes.Sum(static lane => lane.ReceiptCount),
            Lanes: lanes);
    }

    public MembraneLaneViewModel? ProjectLane(MembraneDispatchLane lane)
    {
        if (!_inspectionApi.IsLoaded)
        {
            return null;
        }

        var laneView = _inspectionApi.InspectLane(lane);
        return laneView is null ? null : CreateLaneViewModel(laneView);
    }

    private static MembraneLaneViewModel CreateLaneViewModel(MembraneInspectionLaneView laneView)
    {
        return new MembraneLaneViewModel(
            Lane: laneView.Lane,
            LaneLabel: ToLaneLabel(laneView.Lane),
            HeldCount: laneView.HeldCount,
            ReceiptCount: laneView.ReceiptCount,
            RecentDispatchIds: laneView.RecentDispatchIds.ToArray(),
            RecentTraceIds: laneView.RecentTraceIds.ToArray(),
            DecisionSummaries: laneView.DecisionSummaries
                .Select(CreateDecisionSummaryViewModel)
                .ToArray(),
            LatestReceivedAt: laneView.LatestReceivedAt);
    }

    private static MembraneDecisionSummaryViewModel CreateDecisionSummaryViewModel(
        MembraneInspectionDecisionSummary summary)
    {
        return new MembraneDecisionSummaryViewModel(
            Decision: summary.Decision,
            DecisionLabel: ToDecisionLabel(summary.Decision),
            Count: summary.Count);
    }

    private static string ToLaneLabel(MembraneDispatchLane lane)
    {
        return lane switch
        {
            MembraneDispatchLane.Accepted => "Accepted Lane",
            MembraneDispatchLane.Transformed => "Transformed Lane",
            MembraneDispatchLane.Deferred => "Deferred Lane",
            MembraneDispatchLane.Refused => "Refused Lane",
            MembraneDispatchLane.Collapsed => "Collapsed Lane",
            _ => throw new ArgumentOutOfRangeException(nameof(lane), lane, "unsupported membrane dispatch lane.")
        };
    }

    private static string ToDecisionLabel(MembraneDecision decision)
    {
        return decision switch
        {
            MembraneDecision.Accept => "Accept",
            MembraneDecision.Transform => "Transform",
            MembraneDecision.Defer => "Defer",
            MembraneDecision.Refuse => "Refuse",
            MembraneDecision.Collapse => "Collapse",
            _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "unsupported membrane decision.")
        };
    }
}
