namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneDashboardAdapterTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 20, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullViewProjection_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneDashboardAdapter(null!));
    }

    [Fact]
    public void Build_WhenProjectionIsUnloaded_ReturnsUnloadedDashboardModel()
    {
        var adapter = new DefaultMembraneDashboardAdapter(
            new DefaultMembraneViewProjection(
                new DefaultMembraneInspectionApi(
                    new DefaultMembraneLaneReadModel())));

        var dashboard = adapter.Build();

        Assert.False(dashboard.IsLoaded);
        Assert.False(dashboard.HasData);
        Assert.True(dashboard.ShowEmptyState);
        Assert.Equal("Unloaded", dashboard.StatusLabel);
        Assert.Null(dashboard.WitnessSnapshotId);
        Assert.Null(dashboard.ObservedAt);
        Assert.Equal(0, dashboard.TotalHeldCount);
        Assert.Equal(0, dashboard.TotalReceiptCount);
        Assert.Empty(dashboard.LaneCards);
    }

    [Fact]
    public void BuildLane_WhenProjectionIsUnloaded_ReturnsNull()
    {
        var adapter = new DefaultMembraneDashboardAdapter(
            new DefaultMembraneViewProjection(
                new DefaultMembraneInspectionApi(
                    new DefaultMembraneLaneReadModel())));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Build_AdaptsProjectedViewIntoDashboardCards()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-401",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-071", "dispatch://membrane/accepted-072"],
                    ["trace://session-a/accepted-071", "trace://session-a/accepted-072"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-3)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Deferred,
                    ["dispatch://membrane/deferred-081"],
                    ["trace://session-a/deferred-081"],
                    [MembraneDecision.Defer],
                    FixedObservedAt)
            ]));

        var dashboard = adapter.Build();

        Assert.True(dashboard.IsLoaded);
        Assert.True(dashboard.HasData);
        Assert.False(dashboard.ShowEmptyState);
        Assert.Equal("Loaded", dashboard.StatusLabel);
        Assert.Equal("witness://membrane/test-401", dashboard.WitnessSnapshotId);
        Assert.Equal(FixedObservedAt, dashboard.ObservedAt);
        Assert.Equal(3, dashboard.TotalHeldCount);
        Assert.Equal(3, dashboard.TotalReceiptCount);
        Assert.Equal(2, dashboard.LaneCards.Count);

        var acceptedCard = Assert.Single(dashboard.LaneCards.Where(static card => card.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("Accepted Lane", acceptedCard.Title);
        Assert.True(acceptedCard.HasData);
        Assert.Equal(2, acceptedCard.HeldCount);
        Assert.Equal(2, acceptedCard.ReceiptCount);
        Assert.Equal(["dispatch://membrane/accepted-071", "dispatch://membrane/accepted-072"], acceptedCard.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-071", "trace://session-a/accepted-072"], acceptedCard.RecentTraceIds);
        Assert.Equal(["Accept (2)"], acceptedCard.DecisionBadges);
        Assert.Equal(FixedObservedAt.AddMinutes(-3), acceptedCard.LatestReceivedAt);

        var deferredCard = Assert.Single(dashboard.LaneCards.Where(static card => card.Lane == MembraneDispatchLane.Deferred));
        Assert.Equal("Deferred Lane", deferredCard.Title);
        Assert.True(deferredCard.HasData);
        Assert.Equal(1, deferredCard.HeldCount);
        Assert.Equal(1, deferredCard.ReceiptCount);
        Assert.Equal(["dispatch://membrane/deferred-081"], deferredCard.RecentDispatchIds);
        Assert.Equal(["trace://session-a/deferred-081"], deferredCard.RecentTraceIds);
        Assert.Equal(["Defer (1)"], deferredCard.DecisionBadges);
        Assert.Equal(FixedObservedAt, deferredCard.LatestReceivedAt);
    }

    [Fact]
    public void Build_LoadedButEmptyProjection_ShowsEmptyState()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-402",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var dashboard = adapter.Build();

        Assert.True(dashboard.IsLoaded);
        Assert.False(dashboard.HasData);
        Assert.True(dashboard.ShowEmptyState);
        Assert.Equal("Loaded", dashboard.StatusLabel);
        Assert.Empty(dashboard.LaneCards);
    }

    [Fact]
    public void BuildLane_ReturnsSingleDashboardCard()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-403",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-091"],
                    ["trace://session-a/collapsed-091"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var card = adapter.BuildLane(MembraneDispatchLane.Collapsed);

        Assert.NotNull(card);
        Assert.Equal(MembraneDispatchLane.Collapsed, card!.Lane);
        Assert.Equal("Collapsed Lane", card.Title);
        Assert.True(card.HasData);
        Assert.Equal(["Collapse (1)"], card.DecisionBadges);
    }

    [Fact]
    public void BuildLane_ReturnsNullForMissingLane()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-404",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Refused));
    }

    private static DefaultMembraneDashboardAdapter CreateAdapter(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        var projection = new DefaultMembraneViewProjection(inspectionApi);
        return new DefaultMembraneDashboardAdapter(projection);
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
