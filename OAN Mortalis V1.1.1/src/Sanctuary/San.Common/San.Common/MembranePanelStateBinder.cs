namespace San.Common;

public interface IMembranePanelStateBinder
{
    MembranePanelState Bind();

    MembranePanelLaneState? BindLane(MembraneDispatchLane lane);
}

public sealed record MembranePanelLaneState(
    MembraneDispatchLane Lane,
    string Title,
    bool HasData,
    string HeldCountLabel,
    string ReceiptCountLabel,
    IReadOnlyList<string> DecisionBadges,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    string LatestReceiptLabel);

public sealed record MembranePanelState(
    bool IsLoaded,
    bool HasData,
    bool ShowEmptyState,
    string StatusLabel,
    string PanelTitle,
    string? WitnessSnapshotId,
    string ObservedAtLabel,
    string TotalHeldLabel,
    string TotalReceiptLabel,
    IReadOnlyList<MembranePanelLaneState> LaneStates);

public sealed class DefaultMembranePanelStateBinder : IMembranePanelStateBinder
{
    private readonly IMembraneDashboardAdapter _dashboardAdapter;

    public DefaultMembranePanelStateBinder(IMembraneDashboardAdapter dashboardAdapter)
    {
        _dashboardAdapter = dashboardAdapter ?? throw new ArgumentNullException(nameof(dashboardAdapter));
    }

    public MembranePanelState Bind()
    {
        var dashboard = _dashboardAdapter.Build();

        if (!dashboard.IsLoaded)
        {
            return new MembranePanelState(
                IsLoaded: false,
                HasData: false,
                ShowEmptyState: true,
                StatusLabel: dashboard.StatusLabel,
                PanelTitle: "Membrane Panel",
                WitnessSnapshotId: null,
                ObservedAtLabel: "Not observed",
                TotalHeldLabel: "Held: 0",
                TotalReceiptLabel: "Receipts: 0",
                LaneStates: []);
        }

        return new MembranePanelState(
            IsLoaded: true,
            HasData: dashboard.HasData,
            ShowEmptyState: dashboard.ShowEmptyState,
            StatusLabel: dashboard.StatusLabel,
            PanelTitle: "Membrane Panel",
            WitnessSnapshotId: dashboard.WitnessSnapshotId,
            ObservedAtLabel: FormatObservedAt(dashboard.ObservedAt),
            TotalHeldLabel: $"Held: {dashboard.TotalHeldCount}",
            TotalReceiptLabel: $"Receipts: {dashboard.TotalReceiptCount}",
            LaneStates: dashboard.LaneCards
                .Select(CreateLaneState)
                .ToArray());
    }

    public MembranePanelLaneState? BindLane(MembraneDispatchLane lane)
    {
        var laneCard = _dashboardAdapter.BuildLane(lane);
        return laneCard is null ? null : CreateLaneState(laneCard);
    }

    private static MembranePanelLaneState CreateLaneState(MembraneDashboardLaneCard laneCard)
    {
        return new MembranePanelLaneState(
            Lane: laneCard.Lane,
            Title: laneCard.Title,
            HasData: laneCard.HasData,
            HeldCountLabel: $"Held: {laneCard.HeldCount}",
            ReceiptCountLabel: $"Receipts: {laneCard.ReceiptCount}",
            DecisionBadges: laneCard.DecisionBadges.ToArray(),
            RecentDispatchIds: laneCard.RecentDispatchIds.ToArray(),
            RecentTraceIds: laneCard.RecentTraceIds.ToArray(),
            LatestReceiptLabel: laneCard.LatestReceivedAt is null
                ? "Latest receipt: none"
                : $"Latest receipt: {laneCard.LatestReceivedAt.Value:O}");
    }

    private static string FormatObservedAt(DateTimeOffset? observedAt)
    {
        return observedAt is null
            ? "Observed: not available"
            : $"Observed: {observedAt.Value:O}";
    }
}
