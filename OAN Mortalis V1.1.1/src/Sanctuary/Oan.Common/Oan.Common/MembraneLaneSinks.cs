namespace Oan.Common;

public interface IMembraneLaneSink
{
    MembraneDispatchLane Lane { get; }
    IReadOnlyList<MembraneDispatchResult> HeldDispatches { get; }
    IReadOnlyList<MembraneDispatchReceipt> Receipts { get; }

    MembraneDispatchReceipt Receive(MembraneDispatchResult dispatchResult);
}

public sealed record MembraneDispatchReceipt(
    string DispatchId,
    MembraneDispatchLane Lane,
    string TraceId,
    MembraneDecision Decision,
    DateTimeOffset ReceivedAt,
    IReadOnlyList<MembraneDispatchNote> Notes);

public static class MembraneDispatchReceiptNoteCodes
{
    public const string AcceptedLaneStored = "accepted-lane-stored";
    public const string TransformedLaneStored = "transformed-lane-stored";
    public const string DeferredLaneStored = "deferred-lane-stored";
    public const string RefusedLaneStored = "refused-lane-stored";
    public const string CollapsedLaneStored = "collapsed-lane-stored";
}

public abstract class MembraneLaneSinkBase : IMembraneLaneSink
{
    private readonly Func<DateTimeOffset> _clock;
    private readonly List<MembraneDispatchResult> _heldDispatches = [];
    private readonly List<MembraneDispatchReceipt> _receipts = [];

    protected MembraneLaneSinkBase(Func<DateTimeOffset>? clock = null)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public abstract MembraneDispatchLane Lane { get; }

    public IReadOnlyList<MembraneDispatchResult> HeldDispatches => _heldDispatches;

    public IReadOnlyList<MembraneDispatchReceipt> Receipts => _receipts;

    protected abstract string ReceiptCode { get; }

    protected abstract string ReceiptMessage { get; }

    public MembraneDispatchReceipt Receive(MembraneDispatchResult dispatchResult)
    {
        ArgumentNullException.ThrowIfNull(dispatchResult);

        if (dispatchResult.Lane != Lane)
        {
            throw new InvalidOperationException(
                $"membrane lane sink '{Lane}' may not receive dispatch lane '{dispatchResult.Lane}'.");
        }

        if (string.IsNullOrWhiteSpace(dispatchResult.DispatchId))
        {
            throw new InvalidOperationException("dispatch result must carry an explicit dispatch id.");
        }

        if (string.IsNullOrWhiteSpace(dispatchResult.DecisionResult.Envelope.TraceId))
        {
            throw new InvalidOperationException("dispatch result must carry an explicit trace id.");
        }

        var receipt = new MembraneDispatchReceipt(
            DispatchId: dispatchResult.DispatchId,
            Lane: dispatchResult.Lane,
            TraceId: dispatchResult.DecisionResult.Envelope.TraceId,
            Decision: dispatchResult.DecisionResult.Decision,
            ReceivedAt: _clock(),
            Notes:
            [
                new MembraneDispatchNote(
                    Code: ReceiptCode,
                    Message: ReceiptMessage)
            ]);

        _heldDispatches.Add(dispatchResult);
        _receipts.Add(receipt);

        return receipt;
    }
}

public sealed class AcceptedMembraneLaneSink : MembraneLaneSinkBase
{
    public AcceptedMembraneLaneSink(Func<DateTimeOffset>? clock = null)
        : base(clock)
    {
    }

    public override MembraneDispatchLane Lane => MembraneDispatchLane.Accepted;

    protected override string ReceiptCode => MembraneDispatchReceiptNoteCodes.AcceptedLaneStored;

    protected override string ReceiptMessage =>
        "accepted dispatch was received into the bounded accepted lane without widening authority.";
}

public sealed class TransformedMembraneLaneSink : MembraneLaneSinkBase
{
    public TransformedMembraneLaneSink(Func<DateTimeOffset>? clock = null)
        : base(clock)
    {
    }

    public override MembraneDispatchLane Lane => MembraneDispatchLane.Transformed;

    protected override string ReceiptCode => MembraneDispatchReceiptNoteCodes.TransformedLaneStored;

    protected override string ReceiptMessage =>
        "transformed dispatch was received into the bounded transformed lane without downstream reinterpretation.";
}

public sealed class DeferredMembraneLaneSink : MembraneLaneSinkBase
{
    public DeferredMembraneLaneSink(Func<DateTimeOffset>? clock = null)
        : base(clock)
    {
    }

    public override MembraneDispatchLane Lane => MembraneDispatchLane.Deferred;

    protected override string ReceiptCode => MembraneDispatchReceiptNoteCodes.DeferredLaneStored;

    protected override string ReceiptMessage =>
        "deferred dispatch was received into the bounded deferred lane and remains non-materialized.";
}

public sealed class RefusedMembraneLaneSink : MembraneLaneSinkBase
{
    public RefusedMembraneLaneSink(Func<DateTimeOffset>? clock = null)
        : base(clock)
    {
    }

    public override MembraneDispatchLane Lane => MembraneDispatchLane.Refused;

    protected override string ReceiptCode => MembraneDispatchReceiptNoteCodes.RefusedLaneStored;

    protected override string ReceiptMessage =>
        "refused dispatch was received into the bounded refusal lane without widened runtime effect.";
}

public sealed class CollapsedMembraneLaneSink : MembraneLaneSinkBase
{
    public CollapsedMembraneLaneSink(Func<DateTimeOffset>? clock = null)
        : base(clock)
    {
    }

    public override MembraneDispatchLane Lane => MembraneDispatchLane.Collapsed;

    protected override string ReceiptCode => MembraneDispatchReceiptNoteCodes.CollapsedLaneStored;

    protected override string ReceiptMessage =>
        "collapsed dispatch was received into the bounded collapse lane without direct persistence or prime exposure.";
}
