namespace San.Common;

public enum WorkerThreadGovernanceState
{
    Configured = 0,
    Hold = 1,
    Bound = 2
}

public enum WorkerThreadWitnessState
{
    AwaitingThreadRootBindWitness = 0,
    ThreadRootBindWitnessed = 1,
    ThreadRootHoldWitnessed = 2
}

public sealed record WorkerThreadIdentityInvariantRequest(
    string ProjectSpaceId,
    string ThreadId,
    string GovernanceRootId,
    string ScopeClass,
    string BindBurdenClass,
    string ContinuityParent,
    string AuthorizationBasis,
    string CarryForwardPolicy,
    string WitnessEventId,
    string ReasonCode);

public sealed record WorkerThreadIdentityInvariantThreadRoot(
    string ThreadRootHandle,
    string ProjectSpaceId,
    string ThreadId,
    string GovernanceRootId,
    string ScopeClass,
    string BindBurdenClass,
    string ContinuityParent,
    string AuthorizationBasis,
    string CarryForwardPolicy,
    WorkerThreadGovernanceState GovernanceState,
    WorkerThreadWitnessState WitnessState,
    string WitnessEventId,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class WorkerThreadGovernanceContracts
{
    public static WorkerThreadIdentityInvariantThreadRoot CreateIdentityInvariantThreadRoot(
        WorkerThreadIdentityInvariantRequest request,
        string threadRootHandle,
        DateTimeOffset timestampUtc,
        WorkerThreadGovernanceState governanceState = WorkerThreadGovernanceState.Configured,
        WorkerThreadWitnessState witnessState = WorkerThreadWitnessState.AwaitingThreadRootBindWitness)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(threadRootHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectSpaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ThreadId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.GovernanceRootId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ScopeClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.BindBurdenClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AuthorizationBasis);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CarryForwardPolicy);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WitnessEventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReasonCode);

        return new WorkerThreadIdentityInvariantThreadRoot(
            ThreadRootHandle: threadRootHandle,
            ProjectSpaceId: request.ProjectSpaceId,
            ThreadId: request.ThreadId,
            GovernanceRootId: request.GovernanceRootId,
            ScopeClass: request.ScopeClass,
            BindBurdenClass: request.BindBurdenClass,
            ContinuityParent: request.ContinuityParent,
            AuthorizationBasis: request.AuthorizationBasis,
            CarryForwardPolicy: request.CarryForwardPolicy,
            GovernanceState: governanceState,
            WitnessState: witnessState,
            WitnessEventId: request.WitnessEventId,
            ReasonCode: request.ReasonCode,
            TimestampUtc: timestampUtc);
    }
}
