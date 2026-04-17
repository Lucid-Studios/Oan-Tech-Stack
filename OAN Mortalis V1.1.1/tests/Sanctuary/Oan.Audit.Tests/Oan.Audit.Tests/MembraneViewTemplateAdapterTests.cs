namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembraneViewTemplateAdapterTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 22, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullUiComponentBinder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneViewTemplateAdapter(null!));
    }

    [Fact]
    public void Build_WhenUiComponentContractIsUnloaded_ReturnsUnloadedTemplate()
    {
        var adapter = new DefaultMembraneViewTemplateAdapter(
            new DefaultMembraneUiComponentBinder(
                new DefaultMembraneOperatorSurfaceAdapter(
                    new DefaultMembranePanelStateBinder(
                        new DefaultMembraneDashboardAdapter(
                            new DefaultMembraneViewProjection(
                                new DefaultMembraneInspectionApi(
                                    new DefaultMembraneLaneReadModel())))))));

        var template = adapter.Build();

        Assert.False(template.IsLoaded);
        Assert.True(template.ShowEmptyState);
        Assert.Equal("MembraneOperatorSurface", template.TemplateName);
        Assert.Equal("Membrane Panel", template.HeaderTitle);
        Assert.Equal("Unloaded", template.StatusText);
        Assert.Equal("Not observed", template.ObservedAtText);
        Assert.Equal("Membrane state is not yet loaded.", template.EmptyStateText);
        Assert.Equal(["Held: 0", "Receipts: 0"], template.SummaryItems);
        Assert.Empty(template.LaneTemplates);
    }

    [Fact]
    public void BuildLane_WhenUiComponentContractIsUnloaded_ReturnsNull()
    {
        var adapter = new DefaultMembraneViewTemplateAdapter(
            new DefaultMembraneUiComponentBinder(
                new DefaultMembraneOperatorSurfaceAdapter(
                    new DefaultMembranePanelStateBinder(
                        new DefaultMembraneDashboardAdapter(
                            new DefaultMembraneViewProjection(
                                new DefaultMembraneInspectionApi(
                                    new DefaultMembraneLaneReadModel())))))));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Build_CreatesRendererNeutralTemplatesWithoutWideningState()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-801",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-201", "dispatch://membrane/accepted-202"],
                    ["trace://session-a/accepted-201", "trace://session-a/accepted-202"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-7)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Deferred,
                    ["dispatch://membrane/deferred-211"],
                    ["trace://session-a/deferred-211"],
                    [MembraneDecision.Defer],
                    FixedObservedAt)
            ]));

        var template = adapter.Build();

        Assert.True(template.IsLoaded);
        Assert.False(template.ShowEmptyState);
        Assert.Equal("MembraneOperatorSurface", template.TemplateName);
        Assert.Equal("Membrane Panel", template.HeaderTitle);
        Assert.Equal("Loaded", template.StatusText);
        Assert.Equal($"Observed: {FixedObservedAt:O}", template.ObservedAtText);
        Assert.Equal(string.Empty, template.EmptyStateText);
        Assert.Equal(["Held: 3", "Receipts: 3"], template.SummaryItems);
        Assert.Equal(2, template.LaneTemplates.Count);

        var acceptedTemplate = Assert.Single(template.LaneTemplates.Where(static lane => lane.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("accepted-lane", acceptedTemplate.SectionId);
        Assert.Equal("Accepted Lane", acceptedTemplate.SectionTitle);
        Assert.True(acceptedTemplate.ShouldRender);
        Assert.Equal(["Held: 2", "Receipts: 2"], acceptedTemplate.MetricItems);
        Assert.Equal(["Accept (2)"], acceptedTemplate.BadgeItems);
        Assert.Equal(["dispatch://membrane/accepted-201", "dispatch://membrane/accepted-202"], acceptedTemplate.DispatchItems);
        Assert.Equal(["trace://session-a/accepted-201", "trace://session-a/accepted-202"], acceptedTemplate.TraceItems);
        Assert.Equal($"Latest receipt: {FixedObservedAt.AddMinutes(-7):O}", acceptedTemplate.LatestReceiptText);

        var deferredTemplate = Assert.Single(template.LaneTemplates.Where(static lane => lane.Lane == MembraneDispatchLane.Deferred));
        Assert.Equal("deferred-lane", deferredTemplate.SectionId);
        Assert.Equal("Deferred Lane", deferredTemplate.SectionTitle);
        Assert.True(deferredTemplate.ShouldRender);
        Assert.Equal(["Held: 1", "Receipts: 1"], deferredTemplate.MetricItems);
        Assert.Equal(["Defer (1)"], deferredTemplate.BadgeItems);
        Assert.Equal(["dispatch://membrane/deferred-211"], deferredTemplate.DispatchItems);
        Assert.Equal(["trace://session-a/deferred-211"], deferredTemplate.TraceItems);
        Assert.Equal($"Latest receipt: {FixedObservedAt:O}", deferredTemplate.LatestReceiptText);
    }

    [Fact]
    public void Build_LoadedButEmptyUiContract_PreservesEmptyState()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-802",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var template = adapter.Build();

        Assert.True(template.IsLoaded);
        Assert.True(template.ShowEmptyState);
        Assert.Equal("No membrane lane activity is currently present.", template.EmptyStateText);
        Assert.Empty(template.LaneTemplates);
    }

    [Fact]
    public void BuildLane_ReturnsSingleLaneTemplate()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-803",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-221"],
                    ["trace://session-a/collapsed-221"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var laneTemplate = adapter.BuildLane(MembraneDispatchLane.Collapsed);

        Assert.NotNull(laneTemplate);
        Assert.Equal(MembraneDispatchLane.Collapsed, laneTemplate!.Lane);
        Assert.Equal("collapsed-lane", laneTemplate.SectionId);
        Assert.Equal("Collapsed Lane", laneTemplate.SectionTitle);
        Assert.True(laneTemplate.ShouldRender);
        Assert.Equal(["Held: 1", "Receipts: 1"], laneTemplate.MetricItems);
        Assert.Equal(["Collapse (1)"], laneTemplate.BadgeItems);
    }

    [Fact]
    public void BuildLane_ReturnsNullForMissingLane()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-804",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Transformed));
    }

    private static DefaultMembraneViewTemplateAdapter CreateAdapter(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        var projection = new DefaultMembraneViewProjection(inspectionApi);
        var dashboardAdapter = new DefaultMembraneDashboardAdapter(projection);
        var panelBinder = new DefaultMembranePanelStateBinder(dashboardAdapter);
        var operatorSurfaceAdapter = new DefaultMembraneOperatorSurfaceAdapter(panelBinder);
        var uiBinder = new DefaultMembraneUiComponentBinder(operatorSurfaceAdapter);
        return new DefaultMembraneViewTemplateAdapter(uiBinder);
    }

    private static MembraneLaneWitnessEntry CreateWitnessEntry(
        MembraneDispatchLane lane,
        IReadOnlyList<string> dispatchIds,
        IReadOnlyList<string> traceIds,
        IReadOnlyList<MembraneDecision> decisions,
        DateTimeOffset? latestReceivedAt)
    {
        return new MembraneLaneWitnessEntry(
            Lane: lane,
            HeldCount: dispatchIds.Count,
            ReceiptCount: dispatchIds.Count,
            DispatchIds: dispatchIds,
            TraceIds: traceIds,
            Decisions: decisions,
            LatestReceivedAt: latestReceivedAt);
    }
}
