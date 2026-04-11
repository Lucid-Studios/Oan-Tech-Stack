namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneViewProjectionTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullInspectionApi_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneViewProjection(null!));
    }

    [Fact]
    public void Project_WhenInspectionApiIsUnloaded_ReturnsUnloadedView()
    {
        var projection = new DefaultMembraneViewProjection(
            new DefaultMembraneInspectionApi(new DefaultMembraneLaneReadModel()));

        var view = projection.Project();

        Assert.False(view.IsLoaded);
        Assert.Equal("Unloaded", view.StatusLabel);
        Assert.Null(view.WitnessSnapshotId);
        Assert.Null(view.ObservedAt);
        Assert.Equal(0, view.TotalHeldCount);
        Assert.Equal(0, view.TotalReceiptCount);
        Assert.Empty(view.Lanes);
    }

    [Fact]
    public void ProjectLane_WhenInspectionApiIsUnloaded_ReturnsNull()
    {
        var projection = new DefaultMembraneViewProjection(
            new DefaultMembraneInspectionApi(new DefaultMembraneLaneReadModel()));

        var laneView = projection.ProjectLane(MembraneDispatchLane.Accepted);

        Assert.Null(laneView);
    }

    [Fact]
    public void Project_CreatesOperatorFacingSummaryWithoutWideningState()
    {
        var projection = CreateProjection(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-301",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-041", "dispatch://membrane/accepted-042"],
                    ["trace://session-a/accepted-041", "trace://session-a/accepted-042"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-2)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Collapsed,
                    ["dispatch://membrane/collapsed-051"],
                    ["trace://session-a/collapsed-051"],
                    [MembraneDecision.Collapse],
                    FixedObservedAt)
            ]));

        var view = projection.Project();

        Assert.True(view.IsLoaded);
        Assert.Equal("Loaded", view.StatusLabel);
        Assert.Equal("witness://membrane/test-301", view.WitnessSnapshotId);
        Assert.Equal(FixedObservedAt, view.ObservedAt);
        Assert.Equal(3, view.TotalHeldCount);
        Assert.Equal(3, view.TotalReceiptCount);
        Assert.Equal(2, view.Lanes.Count);

        var acceptedLane = Assert.Single(view.Lanes.Where(static lane => lane.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal("Accepted Lane", acceptedLane.LaneLabel);
        Assert.Equal(2, acceptedLane.HeldCount);
        Assert.Equal(2, acceptedLane.ReceiptCount);
        Assert.Equal(["dispatch://membrane/accepted-041", "dispatch://membrane/accepted-042"], acceptedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-041", "trace://session-a/accepted-042"], acceptedLane.RecentTraceIds);
        Assert.Equal("Accept", Assert.Single(acceptedLane.DecisionSummaries).DecisionLabel);
        Assert.Equal(2, Assert.Single(acceptedLane.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt.AddMinutes(-2), acceptedLane.LatestReceivedAt);

        var collapsedLane = Assert.Single(view.Lanes.Where(static lane => lane.Lane == MembraneDispatchLane.Collapsed));
        Assert.Equal("Collapsed Lane", collapsedLane.LaneLabel);
        Assert.Equal(1, collapsedLane.HeldCount);
        Assert.Equal(1, collapsedLane.ReceiptCount);
        Assert.Equal(["dispatch://membrane/collapsed-051"], collapsedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/collapsed-051"], collapsedLane.RecentTraceIds);
        Assert.Equal("Collapse", Assert.Single(collapsedLane.DecisionSummaries).DecisionLabel);
        Assert.Equal(1, Assert.Single(collapsedLane.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt, collapsedLane.LatestReceivedAt);
    }

    [Fact]
    public void ProjectLane_ReturnsSingleProjectedLane()
    {
        var projection = CreateProjection(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-302",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Transformed,
                    ["dispatch://membrane/transformed-061"],
                    ["trace://session-a/transformed-061"],
                    [MembraneDecision.Transform],
                    FixedObservedAt)
            ]));

        var laneView = projection.ProjectLane(MembraneDispatchLane.Transformed);

        Assert.NotNull(laneView);
        Assert.Equal(MembraneDispatchLane.Transformed, laneView!.Lane);
        Assert.Equal("Transformed Lane", laneView.LaneLabel);
        Assert.Equal(1, laneView.HeldCount);
        Assert.Equal(1, laneView.ReceiptCount);
        Assert.Equal(["dispatch://membrane/transformed-061"], laneView.RecentDispatchIds);
        Assert.Equal(["trace://session-a/transformed-061"], laneView.RecentTraceIds);
        Assert.Equal("Transform", Assert.Single(laneView.DecisionSummaries).DecisionLabel);
        Assert.Equal(1, Assert.Single(laneView.DecisionSummaries).Count);
    }

    [Fact]
    public void ProjectLane_ReturnsNullForMissingLane()
    {
        var projection = CreateProjection(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-303",
            ObservedAt: FixedObservedAt,
            Entries: []));

        Assert.Null(projection.ProjectLane(MembraneDispatchLane.Refused));
    }

    private static DefaultMembraneViewProjection CreateProjection(
        MembraneLaneWitnessSnapshot witnessSnapshot)
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(witnessSnapshot);

        var inspectionApi = new DefaultMembraneInspectionApi(readModel);
        return new DefaultMembraneViewProjection(inspectionApi);
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
