namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneOperatorSurfaceAdapterTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 21, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullPanelStateBinder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneOperatorSurfaceAdapter(null!));
    }

    [Fact]
    public void Build_WhenPanelStateIsUnloaded_ReturnsUnloadedSurface()
    {
        var adapter = new DefaultMembraneOperatorSurfaceAdapter(
            new DefaultMembranePanelStateBinder(
                new DefaultMembraneDashboardAdapter(
                    new DefaultMembraneViewProjection(
                        new DefaultMembraneInspectionApi(
                            new DefaultMembraneLaneReadModel())))));

        var surface = adapter.Build();

        Assert.False(surface.IsLoaded);
        Assert.True(surface.ShowEmptyState);
        Assert.Equal("Membrane Panel", surface.SurfaceTitle);
        Assert.Equal("Unloaded", surface.StatusLabel);
        Assert.Equal("Not observed", surface.ObservedAtLabel);
        Assert.Equal("Membrane state is not yet loaded.", surface.EmptyStateMessage);
        Assert.Equal(["Held: 0", "Receipts: 0"], surface.SummaryPills);
        Assert.Empty(surface.LaneSections);
    }

    [Fact]
    public void BuildLane_WhenPanelStateIsUnloaded_ReturnsNull()
    {
        var adapter = new DefaultMembraneOperatorSurfaceAdapter(
            new DefaultMembranePanelStateBinder(
                new DefaultMembraneDashboardAdapter(
                    new DefaultMembraneViewProjection(
                        new DefaultMembraneInspectionApi(
                            new DefaultMembraneLaneReadModel())))));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Build_CreatesComponentReadySectionsWithoutWideningState()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-601",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-131", "dispatch://membrane/accepted-132"],
                    ["trace://session-a/accepted-131", "trace://session-a/accepted-132"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-5)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Transformed,
                    ["dispatch://membrane/transformed-141"],
                    ["trace://session-a/transformed-141"],
                    [MembraneDecision.Transform],
                    FixedObservedAt)
            ]));

        var surface = adapter.Build();

        Assert.True(surface.IsLoaded);
        Assert.False(surface.ShowEmptyState);
        Assert.Equal("Membrane Panel", surface.SurfaceTitle);
        Assert.Equal("Loaded", surface.StatusLabel);
        Assert.Equal($"Observed: {FixedObservedAt:O}", surface.ObservedAtLabel);
        Assert.Equal(string.Empty, surface.EmptyStateMessage);
        Assert.Equal(["Held: 3", "Receipts: 3"], surface.SummaryPills);
        Assert.Equal(2, surface.LaneSections.Count);

        var acceptedSection = Assert.Single(surface.LaneSections.Where(static section => section.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("Accepted Lane", acceptedSection.Heading);
        Assert.True(acceptedSection.ShowSection);
        Assert.Equal(["Held: 2", "Receipts: 2"], acceptedSection.MetricPills);
        Assert.Equal(["Accept (2)"], acceptedSection.DecisionBadges);
        Assert.Equal(["dispatch://membrane/accepted-131", "dispatch://membrane/accepted-132"], acceptedSection.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-131", "trace://session-a/accepted-132"], acceptedSection.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt.AddMinutes(-5):O}", acceptedSection.LatestReceiptLabel);

        var transformedSection = Assert.Single(surface.LaneSections.Where(static section => section.Lane == MembraneDispatchLane.Transformed));
        Assert.Equal("Transformed Lane", transformedSection.Heading);
        Assert.True(transformedSection.ShowSection);
        Assert.Equal(["Held: 1", "Receipts: 1"], transformedSection.MetricPills);
        Assert.Equal(["Transform (1)"], transformedSection.DecisionBadges);
        Assert.Equal(["dispatch://membrane/transformed-141"], transformedSection.RecentDispatchIds);
        Assert.Equal(["trace://session-a/transformed-141"], transformedSection.RecentTraceIds);
        Assert.Equal($"Latest receipt: {FixedObservedAt:O}", transformedSection.LatestReceiptLabel);
    }

    [Fact]
    public void Build_LoadedButEmptyPanelState_ShowsOperatorEmptyState()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-602",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var surface = adapter.Build();

        Assert.True(surface.IsLoaded);
        Assert.True(surface.ShowEmptyState);
        Assert.Equal("No membrane lane activity is currently present.", surface.EmptyStateMessage);
        Assert.Empty(surface.LaneSections);
    }

    [Fact]
    public void BuildLane_ReturnsSingleOperatorSection()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-603",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-151"],
                    ["trace://session-a/collapsed-151"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var section = adapter.BuildLane(MembraneDispatchLane.Collapsed);

        Assert.NotNull(section);
        Assert.Equal(MembraneDispatchLane.Collapsed, section!.Lane);
        Assert.Equal("Collapsed Lane", section.Heading);
        Assert.True(section.ShowSection);
        Assert.Equal(["Held: 1", "Receipts: 1"], section.MetricPills);
        Assert.Equal(["Collapse (1)"], section.DecisionBadges);
    }

    [Fact]
    public void BuildLane_ReturnsNullForMissingLane()
    {
        var adapter = CreateAdapter(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-604",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(adapter.BuildLane(MembraneDispatchLane.Refused));
    }

    private static DefaultMembraneOperatorSurfaceAdapter CreateAdapter(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        var projection = new DefaultMembraneViewProjection(inspectionApi);
        var dashboardAdapter = new DefaultMembraneDashboardAdapter(projection);
        var binder = new DefaultMembranePanelStateBinder(dashboardAdapter);
        return new DefaultMembraneOperatorSurfaceAdapter(binder);
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
