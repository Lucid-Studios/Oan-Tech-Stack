namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembraneLaneWitnessTests
{
    private static readonly DateTimeOffset FixedReceivedAt = new(2026, 4, 9, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FixedObservedAt = new(2026, 4, 9, 18, 5, 0, TimeSpan.Zero);

    [Fact]
    public void Observe_NullLaneSinks_Throws()
    {
        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => "witness://membrane/test-001");

        Assert.Throws<ArgumentNullException>(() => witness.Observe(null!));
    }

    [Fact]
    public void Observe_NullLaneSinkEntry_FailsClosed()
    {
        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => "witness://membrane/test-002");

        var exception = Assert.Throws<InvalidOperationException>(() => witness.Observe([new AcceptedMembraneLaneSink(() => FixedReceivedAt), null!]));

        Assert.Contains("null lane sink", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Observe_DuplicateLanes_FailClosed()
    {
        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => "witness://membrane/test-003");

        var exception = Assert.Throws<InvalidOperationException>(() => witness.Observe(
        [
            new AcceptedMembraneLaneSink(() => FixedReceivedAt),
            new AcceptedMembraneLaneSink(() => FixedReceivedAt)
        ]));

        Assert.Contains(MembraneDispatchLane.Accepted.ToString(), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Observe_BlankSnapshotId_FailsClosed()
    {
        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => " ");

        Assert.Throws<InvalidOperationException>(() => witness.Observe([new AcceptedMembraneLaneSink(() => FixedReceivedAt)]));
    }

    [Fact]
    public void Observe_CollectsLaneStateWithoutMutatingSinks()
    {
        var acceptedSink = new AcceptedMembraneLaneSink(() => FixedReceivedAt);
        var deferredSink = new DeferredMembraneLaneSink(() => FixedReceivedAt);

        acceptedSink.Receive(CreateDispatchResult(MembraneDispatchLane.Accepted, "dispatch://membrane/accepted-101", "trace://session-a/passport-101"));
        deferredSink.Receive(CreateDispatchResult(MembraneDispatchLane.Deferred, "dispatch://membrane/deferred-202", "trace://session-a/passport-202"));

        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => "witness://membrane/test-004");

        var snapshot = witness.Observe([acceptedSink, deferredSink]);

        Assert.Equal("witness://membrane/test-004", snapshot.SnapshotId);
        Assert.Equal(FixedObservedAt, snapshot.ObservedAt);
        Assert.Equal(2, snapshot.Entries.Count);

        var acceptedEntry = Assert.Single(snapshot.Entries.Where(static entry => entry.Lane == MembraneDispatchLane.Accepted));
        Assert.Equal(1, acceptedEntry.HeldCount);
        Assert.Equal(1, acceptedEntry.ReceiptCount);
        Assert.Equal(["dispatch://membrane/accepted-101"], acceptedEntry.DispatchIds);
        Assert.Equal(["trace://session-a/passport-101"], acceptedEntry.TraceIds);
        Assert.Equal([MembraneDecision.Accept], acceptedEntry.Decisions);
        Assert.Equal(FixedReceivedAt, acceptedEntry.LatestReceivedAt);

        var deferredEntry = Assert.Single(snapshot.Entries.Where(static entry => entry.Lane == MembraneDispatchLane.Deferred));
        Assert.Equal(1, deferredEntry.HeldCount);
        Assert.Equal(1, deferredEntry.ReceiptCount);
        Assert.Equal(["dispatch://membrane/deferred-202"], deferredEntry.DispatchIds);
        Assert.Equal(["trace://session-a/passport-202"], deferredEntry.TraceIds);
        Assert.Equal([MembraneDecision.Defer], deferredEntry.Decisions);
        Assert.Equal(FixedReceivedAt, deferredEntry.LatestReceivedAt);

        Assert.Single(acceptedSink.HeldDispatches);
        Assert.Single(acceptedSink.Receipts);
        Assert.Single(deferredSink.HeldDispatches);
        Assert.Single(deferredSink.Receipts);
    }

    [Fact]
    public void Observe_EmptySinkCollection_ProducesEmptySnapshot()
    {
        var witness = new DefaultMembraneLaneWitness(() => FixedObservedAt, () => "witness://membrane/test-005");

        var snapshot = witness.Observe([]);

        Assert.Equal("witness://membrane/test-005", snapshot.SnapshotId);
        Assert.Equal(FixedObservedAt, snapshot.ObservedAt);
        Assert.Empty(snapshot.Entries);
    }

    private static MembraneDispatchResult CreateDispatchResult(
        MembraneDispatchLane lane,
        string dispatchId,
        string traceId)
    {
        var decision = lane switch
        {
            MembraneDispatchLane.Accepted => MembraneDecision.Accept,
            MembraneDispatchLane.Transformed => MembraneDecision.Transform,
            MembraneDispatchLane.Deferred => MembraneDecision.Defer,
            MembraneDispatchLane.Refused => MembraneDecision.Refuse,
            MembraneDispatchLane.Collapsed => MembraneDecision.Collapse,
            _ => throw new ArgumentOutOfRangeException(nameof(lane), lane, "unsupported membrane dispatch lane.")
        };

        return new MembraneDispatchResult(
            DispatchId: dispatchId,
            Lane: lane,
            DecisionResult: new MembraneDecisionResult(
                Decision: decision,
                Envelope: new SymbolicEnvelope(
                    Origin: "sli-lisp://compass/compass-update",
                    Family: new SymbolicProductFamily("AdmissibilityRead"),
                    ProductClass: lane == MembraneDispatchLane.Collapsed
                        ? SymbolicProductClass.CollapseProduct
                        : SymbolicProductClass.DirectiveProduct,
                    Intent: new SymbolicIntent(lane == MembraneDispatchLane.Collapsed
                        ? "checkpoint-close"
                        : "materialize-bounded-output"),
                    Admissibility: lane switch
                    {
                        MembraneDispatchLane.Deferred => AdmissibilityStatus.Pending,
                        MembraneDispatchLane.Refused => AdmissibilityStatus.Refused,
                        _ => AdmissibilityStatus.Admissible
                    },
                    ContradictionState: ContradictionState.None,
                    MaterializationEligibility: lane switch
                    {
                        MembraneDispatchLane.Transformed => MaterializationEligibility.Restricted,
                        MembraneDispatchLane.Collapsed => MaterializationEligibility.Restricted,
                        _ => MaterializationEligibility.Yes
                    },
                    PersistenceEligibility: lane switch
                    {
                        MembraneDispatchLane.Accepted => PersistenceEligibility.Promotable,
                        _ => PersistenceEligibility.AuditOnly
                    },
                    TraceId: traceId),
                Reasons:
                [
                    new MembraneDecisionReason(
                        Code: "decision-note",
                        Message: "decision note")
                ],
                ValidationResult: new SymbolicEnvelopeValidationResult(
                    IsValid: true,
                    Violations: [])),
            Notes:
            [
                new MembraneDispatchNote(
                    Code: "dispatch-note",
                    Message: "dispatch note")
            ]);
    }
}
