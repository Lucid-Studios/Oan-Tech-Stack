namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembranePanelStateBinderTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 21, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullDashboardAdapter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembranePanelStateBinder(null!));
    }

    [Fact]
    public void Bind_WhenDashboardIsUnloaded_ReturnsUnloadedPanelState()
    {
        var binder = new DefaultMembranePanelStateBinder(
            new DefaultMembraneDashboardAdapter(
                new DefaultMembraneViewProjection(
                    new DefaultMembraneInspectionApi(
                        new DefaultMembraneLaneReadModel()))));

        var panel = binder.Bind();

        Assert.False(panel.IsLoaded);
        Assert.False(panel.HasData);
        Assert.True(panel.ShowEmptyState);
        Assert.Equal("Unloaded", panel.StatusLabel);
        Assert.Equal("Membrane Panel", panel.PanelTitle);
        Assert.Null(panel.WitnessSnapshotId);
        Assert.Equal("Not observed", panel.ObservedAtLabel);
        Assert.Equal("Held: 0", panel.TotalHeldLabel);
        Assert.Equal("Receipts: 0", panel.TotalReceiptLabel);
        Assert.Empty(panel.LaneStates);
    }

    [Fact]
    public void BindLane_WhenDashboardIsUnloaded_ReturnsNull()
    {
        var binder = new DefaultMembranePanelStateBinder(
            new DefaultMembraneDashboardAdapter(
                new DefaultMembraneViewProjection(
                    new DefaultMembraneInspectionApi(
                        new DefaultMembraneLaneReadModel()))));

        Assert.Null(binder.BindLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Bind_CreatesDisplaySafePanelState()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-501",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-101", "dispatch://membrane/accepted-102"],
                    ["trace://session-a/accepted-101", "trace://session-a/accepted-102"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-4)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Refused,
                    ["dispatch://membrane/refused-111"],
                    ["trace://session-a/refused-111"],
                    [MembraneDecision.Refuse],
                    FixedObservedAt)
            ]));

        var panel = binder.Bind();

        Assert.True(panel.IsLoaded);
        Assert.True(panel.HasData);
        Assert.False(panel.ShowEmptyState);
        Assert.Equal("Loaded", panel.StatusLabel);
        Assert.Equal("Membrane Panel", panel.PanelTitle);
        Assert.Equal("witness://membrane/test-501", panel.WitnessSnapshotId);
        Assert.Equal($"Observed: {FixedObservedAt:O}", panel.ObservedAtLabel);
        Assert.Equal("Held: 3", panel.TotalHeldLabel);
        Assert.Equal("Receipts: 3", panel.TotalReceiptLabel);
        Assert.Equal(2, panel.LaneStates.Count);

        var acceptedLane = Assert.Single(panel.LaneStates.Where(static lane => lane.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("Accepted Lane", acceptedLane.Title);
        Assert.True(acceptedLane.HasData);
        Assert.Equal("Held: 2", acceptedLane.HeldCountLabel);
        Assert.Equal("Receipts: 2", acceptedLane.ReceiptCountLabel);
        Assert.Equal(["Accept (2)"], acceptedLane.DecisionBadges);
        Assert.Equal(["dispatch://membrane/accepted-101", "dispatch://membrane/accepted-102"], acceptedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-101", "trace://session-a/accepted-102"], acceptedLane.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt.AddMinutes(-4):O}", acceptedLane.LatestReceiptLabel);

        var refusedLane = Assert.Single(panel.LaneStates.Where(static lane => lane.Lane == MembraneDispatchLane.Refused));
        Assert.Equal("Refused Lane", refusedLane.Title);
        Assert.True(refusedLane.HasData);
        Assert.Equal("Held: 1", refusedLane.HeldCountLabel);
        Assert.Equal("Receipts: 1", refusedLane.ReceiptCountLabel);
        Assert.Equal(["Refuse (1)"], refusedLane.DecisionBadges);
        Assert.Equal(["dispatch://membrane/refused-111"], refusedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/refused-111"], refusedLane.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt:O}", refusedLane.LatestReceiptLabel);
    }

    [Fact]
    public void Bind_LoadedButEmptyDashboard_ShowsEmptyState()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-502",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var panel = binder.Bind();

        Assert.True(panel.IsLoaded);
        Assert.False(panel.HasData);
        Assert.True(panel.ShowEmptyState);
        Assert.Equal("Loaded", panel.StatusLabel);
        Assert.Empty(panel.LaneStates);
    }

    [Fact]
    public void BindLane_ReturnsSinglePanelLaneState()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-503",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-121"],
                    ["trace://session-a/collapsed-121"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var lane = binder.BindLane(MembraneDispatchLane.Collapsed);

        Assert.NotNull(lane);
        Assert.Equal(MembraneDispatchLane.Collapsed, lane!.Lane);
        Assert.Equal("Collapsed Lane", lane.Title);
        Assert.True(lane.HasData);
        Assert.Equal(["Collapse (1)"], lane.DecisionBadges);
        Assert.Equal($"Latest receipt: {FixedObservedAt:O}", lane.LatestReceiptLabel);
    }

    [Fact]
    public void BindLane_ReturnsNullForMissingLane()
    {
        var binder = CreateBinder(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-504",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(binder.BindLane(MembraneDispatchLane.Deferred));
    }

    private static DefaultMembranePanelStateBinder CreateBinder(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        var projection = new DefaultMembraneViewProjection(inspectionApi);
        var dashboardAdapter = new DefaultMembraneDashboardAdapter(projection);
        return new DefaultMembranePanelStateBinder(dashboardAdapter);
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
