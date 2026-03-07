namespace Oan.Common;

public sealed record SoulFrameProjectionRequest(
    Guid IdentityId,
    string CMEId,
    string SourceCustodyDomain,
    string RequestedTheater,
    string PolicyHandle);

public interface ISelfStateProjection
{
    Guid IdentityId { get; }
    string ProjectionHandle { get; }
    string SessionHandle { get; }
    string TargetTheater { get; }
    bool IsMitigated { get; }
    string WorkingStateHandle { get; }
    string ProvenanceMarker { get; }
}

public sealed record SelfStateProjection(
    Guid IdentityId,
    string ProjectionHandle,
    string SessionHandle,
    string TargetTheater,
    bool IsMitigated,
    string WorkingStateHandle,
    string ProvenanceMarker) : ISelfStateProjection;

public sealed record SoulFrameReturnIntakeRequest(
    Guid IdentityId,
    string CMEId,
    string SessionHandle,
    string SourceTheater,
    string ReturnCandidatePointer,
    string ProvenanceMarker,
    string IntakeIntent);

public sealed record SoulFrameReturnIntakeReceipt(
    Guid IdentityId,
    string IntakeHandle,
    bool Accepted,
    string Disposition);

public interface ISoulFrameMembrane
{
    Task<ISelfStateProjection> ProjectMitigatedAsync(
        SoulFrameProjectionRequest request,
        CancellationToken cancellationToken = default);

    Task<SoulFrameReturnIntakeReceipt> ReceiveReturnIntakeAsync(
        SoulFrameReturnIntakeRequest request,
        CancellationToken cancellationToken = default);
}
