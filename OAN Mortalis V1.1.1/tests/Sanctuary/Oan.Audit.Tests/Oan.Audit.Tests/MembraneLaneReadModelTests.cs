namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembraneLaneReadModelTests
{
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 19, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Refresh_NullSnapshot_Throws()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        Assert.Throws<ArgumentNullException>(() => readModel.Refresh(null!));
    }

    [Fact]
    public void Refresh_BlankSnapshotId_FailsClosed()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        var snapshot = new MembraneLaneWitnessSnapshot(
            SnapshotId: " ",
            ObservedAt: FixedObservedAt,
            Entries: []);

        Assert.Throws<InvalidOperationException>(() => readModel.Refresh(snapshot));
    }

    [Fact]
    public void Refresh_DuplicateLanes_FailClosed()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        var snapshot = new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-101",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(MembraneDispatchLane.Accepted, ["dispatch://membrane/a-001"], ["trace://session-a/a-001"], [MembraneDecision.Accept], FixedObservedAt),
                CreateWitnessEntry(MembraneDispatchLane.Accepted, ["dispatch://membrane/a-002"], ["trace://session-a/a-002"], [MembraneDecision.Accept], FixedObservedAt)
            ]);

        var exception = Assert.Throws<InvalidOperationException>(() => readModel.Refresh(snapshot));

        Assert.Contains(MembraneDispatchLane.Accepted.ToString(), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_BeforeRefresh_FailsClosed()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        Assert.False(readModel.HasSnapshot);
        Assert.Throws<InvalidOperationException>(() => readModel.Read());
    }

    [Fact]
    public void ReadLane_BeforeRefresh_FailsClosed()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        Assert.False(readModel.HasSnapshot);
        Assert.Throws<InvalidOperationException>(() => readModel.ReadLane(MembraneDispatchLane.Accepted));
    }

    [Fact]
    public void Refresh_CreatesBoundedRuntimeReadableSnapshot()
    {
        var readModel = new DefaultMembraneLaneReadModel(recentItemLimit: 2);

        var snapshot = new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-102",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(
                    MembraneDispatchLane.Accepted,
                    ["dispatch://membrane/accepted-001", "dispatch://membrane/accepted-002", "dispatch://membrane/accepted-003"],
                    ["trace://session-a/accepted-001", "trace://session-a/accepted-002", "trace://session-a/accepted-003"],
                    [MembraneDecision.Accept, MembraneDecision.Accept, MembraneDecision.Accept],
                    FixedObservedAt.AddMinutes(-1)),
                CreateWitnessEntry(
                    MembraneDispatchLane.Transformed,
                    ["dispatch://membrane/transformed-010", "dispatch://membrane/transformed-011"],
                    ["trace://session-a/transformed-010", "trace://session-a/transformed-011"],
                    [MembraneDecision.Transform, MembraneDecision.Transform],
                    FixedObservedAt)
            ]);

        readModel.Refresh(snapshot);

        var readSnapshot = readModel.Read();

        Assert.True(readModel.HasSnapshot);
        Assert.Equal("witness://membrane/test-102", readSnapshot.WitnessSnapshotId);
        Assert.Equal(FixedObservedAt, readSnapshot.ObservedAt);
        Assert.Equal(2, readSnapshot.Entries.Count);

        var acceptedEntry = Assert.Single(readSnapshot.Entries.Where(static entry => entry.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal(3, acceptedEntry.HeldCount);
        Assert.Equal(3, acceptedEntry.ReceiptCount);
        Assert.Equal(["dispatch://membrane/accepted-002", "dispatch://membrane/accepted-003"], acceptedEntry.RecentDispatchIds);
        Assert.Equal(["trace://session-a/accepted-002", "trace://session-a/accepted-003"], acceptedEntry.RecentTraceIds);
        Assert.Equal(MembraneDecision.Accept, Assert.Single(acceptedEntry.DecisionSummaries).Decision);
        Assert.Equal(3, Assert.Single(acceptedEntry.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt.AddMinutes(-1), acceptedEntry.LatestReceivedAt);

        var transformedEntry = Assert.Single(readSnapshot.Entries.Where(static entry => entry.Lane == MembraneDispatchLane.Transformed));
        Assert.Equal(2, transformedEntry.HeldCount);
        Assert.Equal(2, transformedEntry.ReceiptCount);
        Assert.Equal(["dispatch://membrane/transformed-010", "dispatch://membrane/transformed-011"], transformedEntry.RecentDispatchIds);
        Assert.Equal(["trace://session-a/transformed-010", "trace://session-a/transformed-011"], transformedEntry.RecentTraceIds);
        Assert.Equal(MembraneDecision.Transform, Assert.Single(transformedEntry.DecisionSummaries).Decision);
        Assert.Equal(2, Assert.Single(transformedEntry.DecisionSummaries).Count);
        Assert.Equal(FixedObservedAt, transformedEntry.LatestReceivedAt);
    }

    [Fact]
    public void ReadLane_ReturnsNullForMissingLane()
    {
        var readModel = new DefaultMembraneLaneReadModel();

        readModel.Refresh(new MembraneLaneWitnessSnapshot(
            SnapshotId: "witness://membrane/test-103",
            ObservedAt: FixedObservedAt,
            Entries:
            [
                CreateWitnessEntry(MembraneDispatchLane.Refused, ["dispatch://membrane/refused-001"], ["trace://session-a/refused-001"], [MembraneDecision.Refuse], FixedObservedAt)
            ]));

        var entry = readModel.ReadLane(MembraneDispatchLane.Accepted);

        Assert.Null(entry);
    }

    [Fact]
    public void Constructor_WithNonPositiveRecentItemLimit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DefaultMembraneLaneReadModel(0));
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
