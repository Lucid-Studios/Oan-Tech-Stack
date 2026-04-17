namespace San.Common;

public interface IMembraneViewTemplateAdapter
{
    MembraneViewTemplate Build();

    MembraneLaneViewTemplate? BuildLane(MembraneDispatchLane lane);
}

public sealed record MembraneLaneViewTemplate(
    MembraneDispatchLane Lane,
    string SectionId,
    string SectionTitle,
    bool ShouldRender,
    IReadOnlyList<string> MetricItems,
    IReadOnlyList<string> BadgeItems,
    IReadOnlyList<string> DispatchItems,
    IReadOnlyList<string> TraceItems,
    string LatestReceiptText);

public sealed record MembraneViewTemplate(
    bool IsLoaded,
    bool ShowEmptyState,
    string TemplateName,
    string HeaderTitle,
    string StatusText,
    string ObservedAtText,
    string EmptyStateText,
    IReadOnlyList<string> SummaryItems,
    IReadOnlyList<MembraneLaneViewTemplate> LaneTemplates);

public sealed class DefaultMembraneViewTemplateAdapter : IMembraneViewTemplateAdapter
{
    private readonly IMembraneUiComponentBinder _uiComponentBinder;

    public DefaultMembraneViewTemplateAdapter(IMembraneUiComponentBinder uiComponentBinder)
    {
        _uiComponentBinder = uiComponentBinder ?? throw new ArgumentNullException(nameof(uiComponentBinder));
    }

    public MembraneViewTemplate Build()
    {
        var contract = _uiComponentBinder.Bind();

        return new MembraneViewTemplate(
            IsLoaded: contract.IsLoaded,
            ShowEmptyState: contract.ShowEmptyState,
            TemplateName: "MembraneOperatorSurface",
            HeaderTitle: contract.TitleText,
            StatusText: contract.StatusText,
            ObservedAtText: contract.ObservedAtText,
            EmptyStateText: contract.EmptyStateText,
            SummaryItems: contract.SummaryPills.ToArray(),
            LaneTemplates: contract.LaneComponents
                .Select(CreateLaneTemplate)
                .ToArray());
    }

    public MembraneLaneViewTemplate? BuildLane(MembraneDispatchLane lane)
    {
        var laneContract = _uiComponentBinder.BindLane(lane);
        return laneContract is null ? null : CreateLaneTemplate(laneContract);
    }

    private static MembraneLaneViewTemplate CreateLaneTemplate(
        MembraneUiLaneComponentContract laneContract)
    {
        return new MembraneLaneViewTemplate(
            Lane: laneContract.Lane,
            SectionId: $"{laneContract.Lane.ToString().ToLowerInvariant()}-lane",
            SectionTitle: laneContract.SectionTitle,
            ShouldRender: laneContract.IsVisible,
            MetricItems: laneContract.MetricPills.ToArray(),
            BadgeItems: laneContract.DecisionBadges.ToArray(),
            DispatchItems: laneContract.RecentDispatchIds.ToArray(),
            TraceItems: laneContract.RecentTraceIds.ToArray(),
            LatestReceiptText: laneContract.LatestReceiptText);
    }
}
