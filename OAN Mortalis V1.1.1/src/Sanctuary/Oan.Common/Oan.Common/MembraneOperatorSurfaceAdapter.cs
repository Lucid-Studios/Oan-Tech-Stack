namespace Oan.Common;

public interface IMembraneOperatorSurfaceAdapter
{
    MembraneOperatorSurfaceModel Build();

    MembraneOperatorLaneSection? BuildLane(MembraneDispatchLane lane);
}

public sealed record MembraneOperatorLaneSection(
    MembraneDispatchLane Lane,
    string Heading,
    bool ShowSection,
    IReadOnlyList<string> MetricPills,
    IReadOnlyList<string> DecisionBadges,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    string LatestReceiptLabel);

public sealed record MembraneOperatorSurfaceModel(
    bool IsLoaded,
    bool ShowEmptyState,
    string SurfaceTitle,
    string StatusLabel,
    string ObservedAtLabel,
    string EmptyStateMessage,
    IReadOnlyList<string> SummaryPills,
    IReadOnlyList<MembraneOperatorLaneSection> LaneSections);

public sealed class DefaultMembraneOperatorSurfaceAdapter : IMembraneOperatorSurfaceAdapter
{
    private readonly IMembranePanelStateBinder _panelStateBinder;

    public DefaultMembraneOperatorSurfaceAdapter(IMembranePanelStateBinder panelStateBinder)
    {
        _panelStateBinder = panelStateBinder ?? throw new ArgumentNullException(nameof(panelStateBinder));
    }

    public MembraneOperatorSurfaceModel Build()
    {
        var panelState = _panelStateBinder.Bind();

        if (!panelState.IsLoaded)
        {
            return new MembraneOperatorSurfaceModel(
                IsLoaded: false,
                ShowEmptyState: true,
                SurfaceTitle: panelState.PanelTitle,
                StatusLabel: panelState.StatusLabel,
                ObservedAtLabel: panelState.ObservedAtLabel,
                EmptyStateMessage: "Membrane state is not yet loaded.",
                SummaryPills:
                [
                    panelState.TotalHeldLabel,
                    panelState.TotalReceiptLabel
                ],
                LaneSections: []);
        }

        return new MembraneOperatorSurfaceModel(
            IsLoaded: true,
            ShowEmptyState: panelState.ShowEmptyState,
            SurfaceTitle: panelState.PanelTitle,
            StatusLabel: panelState.StatusLabel,
            ObservedAtLabel: panelState.ObservedAtLabel,
            EmptyStateMessage: panelState.ShowEmptyState
                ? "No membrane lane activity is currently present."
                : string.Empty,
            SummaryPills:
            [
                panelState.TotalHeldLabel,
                panelState.TotalReceiptLabel
            ],
            LaneSections: panelState.LaneStates
                .Select(CreateLaneSection)
                .ToArray());
    }

    public MembraneOperatorLaneSection? BuildLane(MembraneDispatchLane lane)
    {
        var laneState = _panelStateBinder.BindLane(lane);
        return laneState is null ? null : CreateLaneSection(laneState);
    }

    private static MembraneOperatorLaneSection CreateLaneSection(MembranePanelLaneState laneState)
    {
        return new MembraneOperatorLaneSection(
            Lane: laneState.Lane,
            Heading: laneState.Title,
            ShowSection: laneState.HasData,
            MetricPills:
            [
                laneState.HeldCountLabel,
                laneState.ReceiptCountLabel
            ],
            DecisionBadges: laneState.DecisionBadges.ToArray(),
            RecentDispatchIds: laneState.RecentDispatchIds.ToArray(),
            RecentTraceIds: laneState.RecentTraceIds.ToArray(),
            LatestReceiptLabel: laneState.LatestReceiptLabel);
    }
}
