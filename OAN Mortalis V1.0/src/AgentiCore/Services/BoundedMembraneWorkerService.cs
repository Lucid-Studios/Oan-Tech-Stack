using Oan.Common;

namespace AgentiCore.Services;

public sealed record BoundedWorkerProjectionRequest(
    Guid IdentityId,
    string CMEId,
    string SourceCustodyDomain,
    string RequestedTheater,
    string PolicyHandle);

public sealed record BoundedWorkerState(
    Guid IdentityId,
    string CMEId,
    string SessionHandle,
    string WorkingStateHandle,
    string ProvenanceMarker,
    string TargetTheater,
    IMediatedSelfStateContour MediatedSelfState);

public sealed class BoundedMembraneWorkerService
{
    private readonly ISoulFrameMembrane _membrane;

    public BoundedMembraneWorkerService(ISoulFrameMembrane membrane)
    {
        _membrane = membrane;
    }

    public async Task<BoundedWorkerState> BeginBoundedWorkAsync(
        BoundedWorkerProjectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CMEId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceCustodyDomain);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RequestedTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PolicyHandle);

        var projection = await _membrane.ProjectMitigatedAsync(
                new SoulFrameProjectionRequest(
                    request.IdentityId,
                    request.CMEId,
                    request.SourceCustodyDomain,
                    request.RequestedTheater,
                    request.PolicyHandle),
                cancellationToken)
            .ConfigureAwait(false);

        EnsureBoundedProjection(projection);

        return new BoundedWorkerState(
            projection.IdentityId,
            request.CMEId,
            projection.SessionHandle,
            projection.WorkingStateHandle,
            projection.ProvenanceMarker,
            projection.TargetTheater,
            projection.MediatedSelfState);
    }

    public Task<SoulFrameReturnIntakeReceipt> SubmitReturnCandidateAsync(
        BoundedWorkerState state,
        string sourceTheater,
        string returnCandidatePointer,
        CmeCollapseClassification collapseClassification,
        string intakeIntent = "candidate-return-evaluation",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnCandidatePointer);
        ArgumentNullException.ThrowIfNull(collapseClassification);
        ArgumentException.ThrowIfNullOrWhiteSpace(intakeIntent);

        EnsureBoundedState(state);

        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: returnCandidatePointer,
            originSurface: sourceTheater,
            provenanceMarker: state.ProvenanceMarker,
            sourceSubsystem: collapseClassification.SourceSubsystem);
        var requestEnvelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.SoulFrameReturnIntake,
            requestedBy: "AgentiCore",
            scopeHandle: state.SessionHandle,
            protectionClass: "cryptic-return",
            witnessRequirement: "membrane-witness",
            actionableContent: actionableContent);

        return _membrane.ReceiveReturnIntakeAsync(
            new SoulFrameReturnIntakeRequest(
                state.IdentityId,
                state.CMEId,
                state.SessionHandle,
                sourceTheater,
                returnCandidatePointer,
                state.ProvenanceMarker,
                intakeIntent,
                collapseClassification,
                requestEnvelope),
            cancellationToken);
    }

    private static void EnsureBoundedProjection(ISelfStateProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        if (!projection.IsMitigated)
        {
            throw new InvalidOperationException("Membrane projection must be mitigated for bounded worker cognition.");
        }

        EnsureHandlePrefix(projection.SessionHandle, "soulframe-session://", "SessionHandle");
        EnsureHandlePrefix(projection.WorkingStateHandle, "soulframe-working://", "WorkingStateHandle");

        if (!projection.ProvenanceMarker.StartsWith("membrane-derived:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Membrane projection provenance must remain witness-only and membrane-derived.");
        }

        EnsureHandlePrefix(projection.MediatedSelfState.CSelfGelHandle, "soulframe-cselfgel://", "MediatedSelfState.CSelfGelHandle");
        EnsureNoCustodyLeak(projection.MediatedSelfState.CSelfGelHandle, "MediatedSelfState.CSelfGelHandle");

        EnsureNoCustodyLeak(projection.SessionHandle, "SessionHandle");
        EnsureNoCustodyLeak(projection.WorkingStateHandle, "WorkingStateHandle");
    }

    private static void EnsureBoundedState(BoundedWorkerState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state.CMEId);
        EnsureHandlePrefix(state.SessionHandle, "soulframe-session://", "SessionHandle");
        EnsureHandlePrefix(state.WorkingStateHandle, "soulframe-working://", "WorkingStateHandle");
        EnsureNoCustodyLeak(state.SessionHandle, "SessionHandle");
        EnsureNoCustodyLeak(state.WorkingStateHandle, "WorkingStateHandle");
    }

    private static void EnsureHandlePrefix(string handle, string prefix, string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);

        if (!handle.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{fieldName} must remain a bounded SoulFrame membrane handle.");
        }
    }

    private static void EnsureNoCustodyLeak(string handle, string fieldName)
    {
        if (handle.Contains("cmos://", StringComparison.OrdinalIgnoreCase) ||
            handle.Contains("cryptic://", StringComparison.OrdinalIgnoreCase) ||
            handle.Contains("mos://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{fieldName} must not widen into Cryptic custody access.");
        }
    }
}
