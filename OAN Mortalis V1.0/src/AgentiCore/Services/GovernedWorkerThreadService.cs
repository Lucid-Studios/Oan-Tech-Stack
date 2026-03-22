using Oan.Common;

namespace AgentiCore.Services;

public sealed class GovernedWorkerThreadService
{
    public IdentityInvariantThreadRootReceipt CreateIdentityThreadRoot(
        BoundedWorkerState state,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        return WorkerThreadGovernanceProjector.CreateIdentityInvariantThreadRoot(
            identityId: state.IdentityId,
            cmeId: state.CMEId,
            sessionHandle: state.SessionHandle,
            workingStateHandle: state.WorkingStateHandle,
            provenanceMarker: state.ProvenanceMarker,
            targetTheater: state.TargetTheater,
            timestampUtc: timestampUtc);
    }

    public GovernedThreadBirthReceipt CreateGovernedThreadBirth(
        BoundedWorkerState state,
        FirstBootGovernanceLayerReceipt governanceLayer,
        string nexusBindingHandle,
        string nexusPortalHandle,
        DateTimeOffset? timestampUtc = null)
    {
        var threadRoot = CreateIdentityThreadRoot(state, timestampUtc);
        return WorkerThreadGovernanceProjector.CreateGovernedThreadBirthReceipt(
            threadRoot,
            governanceLayer,
            nexusBindingHandle,
            nexusPortalHandle,
            timestampUtc);
    }

    public InterWorkerBraidHandoffPacket CreateInterWorkerBraidHandoff(
        GovernedThreadBirthReceipt sourceThread,
        GovernedThreadBirthReceipt targetThread,
        string predicateContextHandle,
        string objective,
        IReadOnlyList<string> bridgedHandles,
        DateTimeOffset? timestampUtc = null)
    {
        return WorkerThreadGovernanceProjector.CreateInterWorkerBraidHandoffPacket(
            sourceThread,
            targetThread,
            predicateContextHandle,
            objective,
            bridgedHandles,
            timestampUtc);
    }
}
