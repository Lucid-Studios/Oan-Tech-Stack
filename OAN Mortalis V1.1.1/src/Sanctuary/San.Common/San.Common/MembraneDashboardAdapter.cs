namespace San.Common;

public interface IMembraneDashboardAdapter
{
    MembraneDashboardModel Build();

    MembraneDashboardLaneCard? BuildLane(MembraneDispatchLane lane);
}

public sealed record MembraneDashboardLaneCard(
    MembraneDispatchLane Lane,
    string Title,
    bool HasData,
    int HeldCount,
    int ReceiptCount,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    IReadOnlyList<string> DecisionBadges,
    DateTimeOffset? LatestReceivedAt);

public sealed record MembraneDashboardModel(
    bool IsLoaded,
    bool HasData,
    bool ShowEmptyState,
    string StatusLabel,
    string? WitnessSnapshotId,
    DateTimeOffset? ObservedAt,
    int TotalHeldCount,
    int TotalReceiptCount,
    IReadOnlyList<MembraneDashboardLaneCard> LaneCards);

public sealed class DefaultMembraneDashboardAdapter : IMembraneDashboardAdapter
{
    private readonly IMembraneViewProjection _viewProjection;

    public DefaultMembraneDashboardAdapter(IMembraneViewProjection viewProjection)
    {
        _viewProjection = viewProjection ?? throw new ArgumentNullException(nameof(viewProjection));
    }

    public MembraneDashboardModel Build()
    {
        var viewModel = _viewProjection.Project();

        if (!viewModel.IsLoaded)
        {
            return new MembraneDashboardModel(
                IsLoaded: false,
                HasData: false,
                ShowEmptyState: true,
                StatusLabel: viewModel.StatusLabel,
                WitnessSnapshotId: null,
                ObservedAt: null,
                TotalHeldCount: 0,
                TotalReceiptCount: 0,
                LaneCards: []);
        }

        var laneCards = viewModel.Lanes
            .Select(CreateLaneCard)
            .ToArray();

        var hasData = laneCards.Any(static card => card.HasData);

        return new MembraneDashboardModel(
            IsLoaded: true,
            HasData: hasData,
            ShowEmptyState: !hasData,
            StatusLabel: viewModel.StatusLabel,
            WitnessSnapshotId: viewModel.WitnessSnapshotId,
            ObservedAt: viewModel.ObservedAt,
            TotalHeldCount: viewModel.TotalHeldCount,
            TotalReceiptCount: viewModel.TotalReceiptCount,
            LaneCards: laneCards);
    }

    public MembraneDashboardLaneCard? BuildLane(MembraneDispatchLane lane)
    {
        var laneView = _viewProjection.ProjectLane(lane);
        return laneView is null ? null : CreateLaneCard(laneView);
    }

    private static MembraneDashboardLaneCard CreateLaneCard(MembraneLaneViewModel laneView)
    {
        return new MembraneDashboardLaneCard(
            Lane: laneView.Lane,
            Title: laneView.LaneLabel,
            HasData: laneView.HeldCount > 0 || laneView.ReceiptCount > 0,
            HeldCount: laneView.HeldCount,
            ReceiptCount: laneView.ReceiptCount,
            RecentDispatchIds: laneView.RecentDispatchIds.ToArray(),
            RecentTraceIds: laneView.RecentTraceIds.ToArray(),
            DecisionBadges: laneView.DecisionSummaries
                .Select(static summary => $"{summary.DecisionLabel} ({summary.Count})")
                .ToArray(),
            LatestReceivedAt: laneView.LatestReceivedAt);
    }
}
