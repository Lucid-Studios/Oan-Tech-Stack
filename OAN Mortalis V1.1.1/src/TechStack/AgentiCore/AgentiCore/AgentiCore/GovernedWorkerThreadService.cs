using San.Common;

namespace AgentiCore;

public interface IGovernedWorkerThreadService
{
    WorkerThreadIdentityInvariantThreadRoot CreateIdentityThreadRoot(WorkerThreadIdentityInvariantRequest request);
}

public sealed class GovernedWorkerThreadService : IGovernedWorkerThreadService
{
    public WorkerThreadIdentityInvariantThreadRoot CreateIdentityThreadRoot(WorkerThreadIdentityInvariantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectSpaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ThreadId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.GovernanceRootId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ScopeClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.BindBurdenClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AuthorizationBasis);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CarryForwardPolicy);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WitnessEventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReasonCode);

        var threadRootHandle = WorkerGovernanceKeys.CreateIdentityThreadRootHandle(
            request.ProjectSpaceId,
            request.ThreadId,
            request.GovernanceRootId);

        return WorkerThreadGovernanceContracts.CreateIdentityInvariantThreadRoot(
            request,
            threadRootHandle,
            DateTimeOffset.UtcNow);
    }
}
