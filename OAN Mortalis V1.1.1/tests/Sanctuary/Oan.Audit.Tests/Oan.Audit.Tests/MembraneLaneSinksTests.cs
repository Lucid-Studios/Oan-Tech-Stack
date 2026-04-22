namespace Oan.Audit.Tests;

using San.Common;

public sealed class MembraneLaneSinksTests
{
    private static readonly DateTimeOffset FixedReceivedAt = new(2026, 4, 9, 17, 30, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(MembraneDispatchLane.Accepted, MembraneDispatchReceiptNoteCodes.AcceptedLaneStored)]
    [InlineData(MembraneDispatchLane.Transformed, MembraneDispatchReceiptNoteCodes.TransformedLaneStored)]
    [InlineData(MembraneDispatchLane.Deferred, MembraneDispatchReceiptNoteCodes.DeferredLaneStored)]
    [InlineData(MembraneDispatchLane.Refused, MembraneDispatchReceiptNoteCodes.RefusedLaneStored)]
    [InlineData(MembraneDispatchLane.Collapsed, MembraneDispatchReceiptNoteCodes.CollapsedLaneStored)]
    public void Receive_MatchingDispatch_IsHeldAndReceipted(
        MembraneDispatchLane lane,
        string expectedReceiptCode)
    {
        var sink = CreateSink(lane, () => FixedReceivedAt);
        var dispatchResult = CreateDispatchResult(lane);

        var receipt = sink.Receive(dispatchResult);

        Assert.Equal(dispatchResult.DispatchId, receipt.DispatchId);
        Assert.Equal(lane, receipt.Lane);
        Assert.Equal(dispatchResult.DecisionResult.Envelope.TraceId, receipt.TraceId);
        Assert.Equal(dispatchResult.DecisionResult.Decision, receipt.Decision);
        Assert.Equal(FixedReceivedAt, receipt.ReceivedAt);
        Assert.Equal(expectedReceiptCode, Assert.Single(receipt.Notes).Code);
        Assert.Same(dispatchResult, Assert.Single(sink.HeldDispatches));
        Assert.Equal(receipt, Assert.Single(sink.Receipts));
    }

    [Theory]
    [InlineData(MembraneDispatchLane.Accepted, MembraneDispatchLane.Transformed)]
    [InlineData(MembraneDispatchLane.Transformed, MembraneDispatchLane.Deferred)]
    [InlineData(MembraneDispatchLane.Deferred, MembraneDispatchLane.Refused)]
    [InlineData(MembraneDispatchLane.Refused, MembraneDispatchLane.Collapsed)]
    [InlineData(MembraneDispatchLane.Collapsed, MembraneDispatchLane.Accepted)]
    public void Receive_MismatchedLane_FailsClosed(
        MembraneDispatchLane sinkLane,
        MembraneDispatchLane dispatchLane)
    {
        var sink = CreateSink(sinkLane, () => FixedReceivedAt);
        var dispatchResult = CreateDispatchResult(dispatchLane);

        var exception = Assert.Throws<InvalidOperationException>(() => sink.Receive(dispatchResult));

        Assert.Contains(sinkLane.ToString(), exception.Message, StringComparison.Ordinal);
        Assert.Empty(sink.HeldDispatches);
        Assert.Empty(sink.Receipts);
    }

    [Fact]
    public void Receive_NullDispatch_Throws()
    {
        var sink = new AcceptedMembraneLaneSink(() => FixedReceivedAt);

        Assert.Throws<ArgumentNullException>(() => sink.Receive(null!));
    }

    [Fact]
    public void Receive_BlankDispatchId_FailsClosed()
    {
        var sink = new AcceptedMembraneLaneSink(() => FixedReceivedAt);
        var dispatchResult = CreateDispatchResult(MembraneDispatchLane.Accepted) with { DispatchId = " " };

        Assert.Throws<InvalidOperationException>(() => sink.Receive(dispatchResult));
        Assert.Empty(sink.HeldDispatches);
        Assert.Empty(sink.Receipts);
    }

    [Fact]
    public void Receive_BlankTraceId_FailsClosed()
    {
        var sink = new AcceptedMembraneLaneSink(() => FixedReceivedAt);
        var dispatchResult = CreateDispatchResult(MembraneDispatchLane.Accepted) with
        {
            DecisionResult = CreateDecisionResult(MembraneDecision.Accept) with
            {
                Envelope = CreateEnvelope(SymbolicProductClass.DirectiveProduct, "materialize-bounded-output") with
                {
                    TraceId = " "
                }
            }
        };

        Assert.Throws<InvalidOperationException>(() => sink.Receive(dispatchResult));
        Assert.Empty(sink.HeldDispatches);
        Assert.Empty(sink.Receipts);
    }

    private static IMembraneLaneSink CreateSink(
        MembraneDispatchLane lane,
        Func<DateTimeOffset> clock)
    {
        return lane switch
        {
            MembraneDispatchLane.Accepted => new AcceptedMembraneLaneSink(clock),
            MembraneDispatchLane.Transformed => new TransformedMembraneLaneSink(clock),
            MembraneDispatchLane.Deferred => new DeferredMembraneLaneSink(clock),
            MembraneDispatchLane.Refused => new RefusedMembraneLaneSink(clock),
            MembraneDispatchLane.Collapsed => new CollapsedMembraneLaneSink(clock),
            _ => throw new ArgumentOutOfRangeException(nameof(lane), lane, "unsupported membrane dispatch lane.")
        };
    }

    private static MembraneDispatchResult CreateDispatchResult(MembraneDispatchLane lane)
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
            DispatchId: $"dispatch://membrane/{lane.ToString().ToLowerInvariant()}-001",
            Lane: lane,
            DecisionResult: CreateDecisionResult(decision),
            Notes:
            [
                new MembraneDispatchNote(
                    Code: "dispatch-note",
                    Message: "dispatch note")
            ]);
    }

    private static MembraneDecisionResult CreateDecisionResult(MembraneDecision decision)
    {
        return new MembraneDecisionResult(
            Decision: decision,
            Envelope: decision == MembraneDecision.Collapse
                ? CreateEnvelope(SymbolicProductClass.CollapseProduct, "checkpoint-close") with
                {
                    MaterializationEligibility = MaterializationEligibility.Restricted,
                    PersistenceEligibility = PersistenceEligibility.AuditOnly
                }
                : CreateEnvelope(SymbolicProductClass.DirectiveProduct, "materialize-bounded-output") with
                {
                    Admissibility = decision switch
                    {
                        MembraneDecision.Defer => AdmissibilityStatus.Pending,
                        MembraneDecision.Refuse => AdmissibilityStatus.Refused,
                        _ => AdmissibilityStatus.Admissible
                    },
                    MaterializationEligibility = decision == MembraneDecision.Transform
                        ? MaterializationEligibility.Restricted
                        : MaterializationEligibility.Yes,
                    PersistenceEligibility = decision == MembraneDecision.Accept
                        ? PersistenceEligibility.Promotable
                        : PersistenceEligibility.AuditOnly
                },
            Reasons:
            [
                new MembraneDecisionReason(
                    Code: "decision-note",
                    Message: "decision note")
            ],
            ValidationResult: new SymbolicEnvelopeValidationResult(
                IsValid: true,
                Violations: []));
    }

    private static SymbolicEnvelope CreateEnvelope(
        SymbolicProductClass productClass,
        string intent)
    {
        return new SymbolicEnvelope(
            Origin: "sli-lisp://compass/compass-update",
            Family: new SymbolicProductFamily("AdmissibilityRead"),
            ProductClass: productClass,
            Intent: new SymbolicIntent(intent),
            Admissibility: AdmissibilityStatus.Admissible,
            ContradictionState: ContradictionState.None,
            MaterializationEligibility: MaterializationEligibility.Yes,
            PersistenceEligibility: PersistenceEligibility.Promotable,
            TraceId: "trace://session-a/passport-005");
    }
}
