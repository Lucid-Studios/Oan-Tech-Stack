namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembraneInspectionApiTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 19, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullReadModel_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMembraneInspectionApi(null!));
    }

    [Fact]
    public void IsLoaded_IsFalseBeforeReadModelRefresh()
    {
        var api = new DefaultMembraneInspectionApi(new DefaultMembraneLaneReadModel());

        Assert.False(api.IsLoaded);
    }

    [Fact]
    public void Inspect_BeforeReadModelRefresh_FailsClosed()
    {
        var api = new DefaultMembraneInspectionApi(new DefaultMembraneLaneReadModel());

        Assert.Throws<InvalidOperationException>(() => api.Inspect());
    }

    [Fact]
    public void InspectLane_BeforeReadModelRefresh_FailsClosed()
    {
        var api = new DefaultMembraneInspectionApi(new DefaultMembraneLaneReadModel());

        Assert.Throws<InvalidOperationException>(() => api.InspectLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Inspect_PresentsWholeSystemObservationWithoutMutation()
    {
        var readModel = new DefaultMembraneLaneReadModel(recentItemLimit: 2);
        readModel.Refresh(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-201",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-011", "dispatch://membrane/accepted-012"],
                    ["trace://session-a/accepted-011", "trace://session-a/accepted-012"],
                    [MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-1)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Refused,
                    ["dispatch://membrane/refused-021"],
                    ["trace://session-a/refused-021"],
                    [MembraneDecision.Refuse],
                    FixedObservedAt)
            ]));

        var api = new DefaultMembraneInspectionApi(readModel);

        var snapshot = api.Inspect();

        Assert.True(api.IsLoaded);
        Assert.Equal("witness://membrane/test-201", snapshot.WitnessSnapshotId);
        Assert.Equal(FixedObservedAt, snapshot.ObservedAt);
        Assert.Equal(2, snapshot.Lanes.Count);

        var acceptedLane = Assert.Single(snapshot.Lanes.Where(static lane => lane.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal(2, acceptedLane.HeldCount);
        Assert.Equal(2, acceptedLane.ReceiptCount);
        Assert.Equal(["dispatch://membrane/accepted-011", "dispatch://membrane/accepted-012"], acceptedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-011", "trace://session-a/accepted-012"], acceptedLane.RecentTraceIds);
        Assert.Equal(MembraneDecision.Accept, Assert.Single(acceptedLane.DecisionSummaries).Decision);
        Assert.Equal(2, Assert.Single(acceptedLane.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt.AddMinutes(-1), acceptedLane.LatestReceivedAt);

        var refusedLane = Assert.Single(snapshot.Lanes.Where(static lane => lane.Lane == MembraneDispatchLane.Refused));
        Assert.Equal(1, refusedLane.HeldCount);
        Assert.Equal(1, refusedLane.ReceiptCount);
        Assert.Equal(["dispatch://membrane/refused-021"], refusedLane.RecentDispatchIds);
        Assert.Equal(["trace://session-a/refused-021"], refusedLane.RecentTraceIds);
        Assert.Equal(MembraneDecision.Refuse, Assert.Single(refusedLane.DecisionSummaries).Decision);
        Assert.Equal(1, Assert.Single(refusedLane.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt, refusedLane.LatestReceivedAt);
    }

    [Fact]
    public void InspectLane_ReturnsSingleLaneView()
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-202",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Transformed,
                    ["dispatch://membrane/transformed-031"],
                    ["trace://session-a/transformed-031"],
                    [MembraneDecision.Transform],
                    FixedObservedAt)
            ]));

        var api = new DefaultMembraneInspectionApi(readModel);

        var laneView = api.InspectLane(MembraneDispatchLane.Transformed);

        Assert.NotNull(laneView);
        Assert.Equal(MembraneDispatchLane.Transformed, laneView!.Lane);
        Assert.Equal(1, laneView.HeldCount);
        Assert.Equal(1, laneView.ReceiptCount);
        Assert.Equal(["dispatch://membrane/transformed-031"], laneView.RecentDispatchIds);
        Assert.Equal(["trace://session-a/transformed-031"], laneView.RecentTraceIds);
        Assert.Equal(MembraneDecision.Transform, Assert.Single(laneView.DecisionSummaries).Decision);
        Assert.Equal(1, Assert.Single(laneView.DecisionSummaries).Count);
    }

    [Fact]
    public void InspectLane_ReturnsNullForMissingLane()
    {
        var readModel = new DefaultMembraneLaneReadModel();
        readModel.Refresh(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-203",
            ObservedAt: FixedObservedAt,
            Entries: []));

        var api = new DefaultMembraneInspectionApi(readModel);

        Assert.Null(api.InspectLane(MembraneDispatchLane.Collapsed));
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
