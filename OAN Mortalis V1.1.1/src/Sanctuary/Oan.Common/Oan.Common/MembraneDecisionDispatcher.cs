namespace Oan.Common;

public interface IMembraneDecisionDispatcher
{
    MembraneDispatchResult Dispatch(MembraneDecisionResult decisionResult);
}

public enum MembraneDispatchLane
{
    Accepted = 0,
    Transformed = 1,
    Deferred = 2,
    Refused = 3,
    Collapsed = 4
}

public sealed record MembraneDispatchNote(
    string Code,
    string Message);

public sealed record MembraneDispatchResult(
    string DispatchId,
    MembraneDispatchLane Lane,
    MembraneDecisionResult DecisionResult,
    IReadOnlyList<MembraneDispatchNote> Notes);

public static class MembraneDispatchNoteCodes
{
    public const string AcceptedLaneReceipt = "accepted-lane-receipt";
    public const string TransformLaneReceipt = "transform-lane-receipt";
    public const string DeferredLaneReceipt = "deferred-lane-receipt";
    public const string RefusedLaneReceipt = "refused-lane-receipt";
    public const string CollapsedLaneReceipt = "collapsed-lane-receipt";
}

public sealed class DefaultMembraneDecisionDispatcher : IMembraneDecisionDispatcher
{
    private readonly Func<string> _dispatchIdFactory;

    public DefaultMembraneDecisionDispatcher()
        : this(() => $"dispatch://membrane/{Guid.NewGuid():N}")
    {
    }

    public DefaultMembraneDecisionDispatcher(Func<string> dispatchIdFactory)
    {
        _dispatchIdFactory = dispatchIdFactory ?? throw new ArgumentNullException(nameof(dispatchIdFactory));
    }

    public MembraneDispatchResult Dispatch(MembraneDecisionResult decisionResult)
    {
        ArgumentNullException.ThrowIfNull(decisionResult);

        var dispatchId = _dispatchIdFactory();

        if (string.IsNullOrWhiteSpace(dispatchId))
        {
            throw new InvalidOperationException("dispatch id factory must return an explicit dispatch id.");
        }

        var (lane, code, message) = decisionResult.Decision switch
        {
            MembraneDecision.Accept => (
                MembraneDispatchLane.Accepted,
                MembraneDispatchNoteCodes.AcceptedLaneReceipt,
                "accepted membrane decision entered the bounded accepted lane without widening authority."),
            MembraneDecision.Transform => (
                MembraneDispatchLane.Transformed,
                MembraneDispatchNoteCodes.TransformLaneReceipt,
                "transform membrane decision entered the bounded transform lane without self-authorizing downstream materialization."),
            MembraneDecision.Defer => (
                MembraneDispatchLane.Deferred,
                MembraneDispatchNoteCodes.DeferredLaneReceipt,
                "deferred membrane decision entered the bounded deferred lane and remains non-materialized."),
            MembraneDecision.Refuse => (
                MembraneDispatchLane.Refused,
                MembraneDispatchNoteCodes.RefusedLaneReceipt,
                "refused membrane decision entered the bounded refusal lane and may not widen into runtime obligation."),
            MembraneDecision.Collapse => (
                MembraneDispatchLane.Collapsed,
                MembraneDispatchNoteCodes.CollapsedLaneReceipt,
                "collapse membrane decision entered the bounded collapse lane without direct persistence or prime exposure."),
            _ => throw new ArgumentOutOfRangeException(nameof(decisionResult), decisionResult.Decision, "unsupported membrane decision.")
        };

        return new MembraneDispatchResult(
            DispatchId: dispatchId,
            Lane: lane,
            DecisionResult: decisionResult,
            Notes:
            [
                new MembraneDispatchNote(
                    Code: code,
                    Message: message)
            ]);
    }
}
