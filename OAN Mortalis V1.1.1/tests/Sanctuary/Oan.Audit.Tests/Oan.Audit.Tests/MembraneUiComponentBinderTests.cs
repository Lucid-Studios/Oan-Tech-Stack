namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneUiComponentBinderTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 22, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullOperatorSurfaceAdapter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneUiComponentBinder(null!));
    }

    [Fact]
    public void Bind_WhenOperatorSurfaceIsUnloaded_ReturnsUnloadedContract()
    {
        var binder = new DefaultMembraneUiComponentBinder(
            new DefaultMembraneOperatorSurfaceAdapter(
                new DefaultMembranePanelStateBinder(
                    new DefaultMembraneDashboardAdapter(
                        new DefaultMembraneViewProjection(
                            new DefaultMembraneInspectionApi(
                                new DefaultMembraneLaneReadModel()))))));

        var contract = binder.Bind();

        Assert.False(contract.IsLoaded);
        Assert.True(contract.ShowEmptyState);
        Assert.Equal("Membrane Panel", contract.TitleText);
        Assert.Equal("Unloaded", contract.StatusText);
        Assert.Equal("Not observed", contract.ObservedAtText);
        Assert.Equal("Membrane state is not yet loaded.", contract.EmptyStateText);
        Assert.Equal(["Held: 0", "Receipts: 0"], contract.SummaryPills);
        Assert.Empty(contract.LaneComponents);
    }

    [Fact]
    public void BindLane_WhenOperatorSurfaceIsUnloaded_ReturnsNull()
    {
        var binder = new DefaultMembraneUiComponentBinder(
            new DefaultMembraneOperatorSurfaceAdapter(
                new DefaultMembranePanelStateBinder(
                    new DefaultMembraneDashboardAdapter(
                        new DefaultMembraneViewProjection(
                            new DefaultMembraneInspectionApi(
                                new DefaultMembraneLaneReadModel()))))));

        Assert.Null(binder.BindLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Bind_CreatesRenderableUiContractsWithoutWideningState()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-701",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-171", "dispatch://membrane/accepted-172"],
                    ["trace://session-a/accepted-171", "trace://session-a/accepted-172"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-6)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Refused,
                    ["dispatch://membrane/refused-181"],
                    ["trace://session-a/refused-181"],
                    [MembraneDecision.Refuse],
                    FixedObservedAt)
            ]));

        var contract = binder.Bind();

        Assert.True(contract.IsLoaded);
        Assert.False(contract.ShowEmptyState);
        Assert.Equal("Membrane Panel", contract.TitleText);
        Assert.Equal("Loaded", contract.StatusText);
        Assert.Equal($"Observed: {FixedObservedAt:O}", contract.ObservedAtText);
        Assert.Equal(string.Empty, contract.EmptyStateText);
        Assert.Equal(["Held: 3", "Receipts: 3"], contract.SummaryPills);
        Assert.Equal(2, contract.LaneComponents.Count);

        var acceptedComponent = Assert.Single(contract.LaneComponents.Where(static lane => lane.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("Accepted Lane", acceptedComponent.SectionTitle);
        Assert.True(acceptedComponent.IsVisible);
        Assert.Equal(["Held: 2", "Receipts: 2"], acceptedComponent.MetricPills);
        Assert.Equal(["Accept (2)"], acceptedComponent.DecisionBadges);
        Assert.Equal(["dispatch://membrane/accepted-171", "dispatch://membrane/accepted-172"], acceptedComponent.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-171", "trace://session-a/accepted-172"], acceptedComponent.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt.AddMinutes(-6):O}", acceptedComponent.LatestReceiptText);

        var refusedComponent = Assert.Single(contract.LaneComponents.Where(static lane => lane.Lane == MembraneDispatchLane.Refused));
        Assert.Equal("Refused Lane", refusedComponent.SectionTitle);
        Assert.True(refusedComponent.IsVisible);
        Assert.Equal(["Held: 1", "Receipts: 1"], refusedComponent.MetricPills);
        Assert.Equal(["Refuse (1)"], refusedComponent.DecisionBadges);
        Assert.Equal(["dispatch://membrane/refused-181"], refusedComponent.RecentDispatchIds);
        Assert.Equal(["trace://session-a/refused-181"], refusedComponent.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt:O}", refusedComponent.LatestReceiptText);
    }

    [Fact]
    public void Bind_LoadedButEmptyOperatorSurface_PreservesEmptyState()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-702",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var contract = binder.Bind();

        Assert.True(contract.IsLoaded);
        Assert.True(contract.ShowEmptyState);
        Assert.Equal("No membrane lane activity is currently present.", contract.EmptyStateText);
        Assert.Empty(contract.LaneComponents);
    }

    [Fact]
    public void BindLane_ReturnsSingleRenderContract()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-703",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-191"],
                    ["trace://session-a/collapsed-191"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var laneContract = binder.BindLane(MembraneDispatchLane.Collapsed);

        Assert.NotNull(laneContract);
        Assert.Equal(MembraneDispatchLane.Collapsed, laneContract!.Lane);
        Assert.Equal("Collapsed Lane", laneContract.SectionTitle);
        Assert.True(laneContract.IsVisible);
        Assert.Equal(["Held: 1", "Receipts: 1"], laneContract.MetricPills);
        Assert.Equal(["Collapse (1)"], laneContract.DecisionBadges);
    }

    [Fact]
    public void BindLane_ReturnsNullForMissingLane()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-704",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(binder.BindLane(MembraneDispatchLane.Deferred));
    }

    private static DefaultMembraneUiComponentBinder CreateBinder(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        var projection = new DefaultMembraneViewProjection(inspectionApi);
        var dashboardAdapter = new DefaultMembraneDashboardAdapter(projection);
        var panelBinder = new DefaultMembranePanelStateBinder(dashboardAdapter);
        var operatorSurfaceAdapter = new DefaultMembraneOperatorSurfaceAdapter(panelBinder);
        return new DefaultMembraneUiComponentBinder(operatorSurfaceAdapter);
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
