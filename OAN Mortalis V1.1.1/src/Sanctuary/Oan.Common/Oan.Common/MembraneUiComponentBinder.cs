namespace Oan.Common;

public interface IMembraneUiComponentBinder
{
    MembraneUiComponentContract Bind();

    MembraneUiLaneComponentContract? BindLane(MembraneDispatchLane lane);
}

public sealed record MembraneUiLaneComponentContract(
    MembraneDispatchLane Lane,
    string SectionTitle,
    bool IsVisible,
    IReadOnlyList<string> MetricPills,
    IReadOnlyList<string> DecisionBadges,
    IReadOnlyList<string> RecentDispatchIds,
    IReadOnlyList<string> RecentTraceIds,
    string LatestReceiptText);

public sealed record MembraneUiComponentContract(
    bool IsLoaded,
    bool ShowEmptyState,
    string TitleText,
    string StatusText,
    string ObservedAtText,
    string EmptyStateText,
    IReadOnlyList<string> SummaryPills,
    IReadOnlyList<MembraneUiLaneComponentContract> LaneComponents);

public sealed class DefaultMembraneUiComponentBinder : IMembraneUiComponentBinder
{
    private readonly IMembraneOperatorSurfaceAdapter _operatorSurfaceAdapter;

    public DefaultMembraneUiComponentBinder(IMembraneOperatorSurfaceAdapter operatorSurfaceAdapter)
    {
        _operatorSurfaceAdapter = operatorSurfaceAdapter ?? throw new ArgumentNullException(nameof(operatorSurfaceAdapter));
    }

    public MembraneUiComponentContract Bind()
    {
        var surface = _operatorSurfaceAdapter.Build();

        return new MembraneUiComponentContract(
            IsLoaded: surface.IsLoaded,
            ShowEmptyState: surface.ShowEmptyState,
            TitleText: surface.SurfaceTitle,
            StatusText: surface.StatusLabel,
            ObservedAtText: surface.ObservedAtLabel,
            EmptyStateText: surface.EmptyStateMessage,
            SummaryPills: surface.SummaryPills.ToArray(),
            LaneComponents: surface.LaneSections
                .Select(CreateLaneComponent)
                .ToArray());
    }

    public MembraneUiLaneComponentContract? BindLane(MembraneDispatchLane lane)
    {
        var laneSection = _operatorSurfaceAdapter.BuildLane(lane);
        return laneSection is null ? null : CreateLaneComponent(laneSection);
    }

    private static MembraneUiLaneComponentContract CreateLaneComponent(
        MembraneOperatorLaneSection laneSection)
    {
        return new MembraneUiLaneComponentContract(
            Lane: laneSection.Lane,
            SectionTitle: laneSection.Heading,
            IsVisible: laneSection.ShowSection,
            MetricPills: laneSection.MetricPills.ToArray(),
            DecisionBadges: laneSection.DecisionBadges.ToArray(),
            RecentDispatchIds: laneSection.RecentDispatchIds.ToArray(),
            RecentTraceIds: laneSection.RecentTraceIds.ToArray(),
            LatestReceiptText: laneSection.LatestReceiptLabel);
    }
}
