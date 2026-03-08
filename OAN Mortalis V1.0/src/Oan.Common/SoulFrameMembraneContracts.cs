namespace Oan.Common;

public sealed record SoulFrameProjectionRequest(
    Guid IdentityId,
    string CMEId,
    string SourceCustodyDomain,
    string RequestedTheater,
    string PolicyHandle);

public interface IMediatedSelfStateContour
{
    string CSelfGelHandle { get; }
    string Classification { get; }
    string PolicyHandle { get; }
}

public sealed record MediatedSelfStateContour(
    string CSelfGelHandle,
    string Classification,
    string PolicyHandle) : IMediatedSelfStateContour;

public interface ISelfStateProjection
{
    Guid IdentityId { get; }
    string ProjectionHandle { get; }
    string SessionHandle { get; }
    string TargetTheater { get; }
    bool IsMitigated { get; }
    string WorkingStateHandle { get; }
    string ProvenanceMarker { get; }
    IMediatedSelfStateContour MediatedSelfState { get; }
}

public sealed record SelfStateProjection(
    Guid IdentityId,
    string ProjectionHandle,
    string SessionHandle,
    string TargetTheater,
    bool IsMitigated,
    string WorkingStateHandle,
    string ProvenanceMarker,
    IMediatedSelfStateContour MediatedSelfState) : ISelfStateProjection;

public sealed record SoulFrameReturnIntakeRequest(
    Guid IdentityId,
    string CMEId,
    string SessionHandle,
    string SourceTheater,
    string ReturnCandidatePointer,
    string ProvenanceMarker,
    string IntakeIntent,
    CmeCollapseClassification CollapseClassification);

public sealed record SoulFrameCollapseEvaluation(
    string Classification,
    CmeCollapseClassification CollapseClassification,
    CmeCollapseResidueClass ResidueClass,
    CmeCollapseReviewState ReviewState,
    bool RequiresReview,
    bool CanRouteToCustody,
    bool CanPublishPrime);

public sealed record SoulFrameReturnIntakeReceipt(
    Guid IdentityId,
    string IntakeHandle,
    bool Accepted,
    string Disposition,
    SoulFrameCollapseEvaluation Evaluation);

public interface ISoulFrameMembrane
{
    Task<ISelfStateProjection> ProjectMitigatedAsync(
        SoulFrameProjectionRequest request,
        CancellationToken cancellationToken = default);

    Task<SoulFrameReturnIntakeReceipt> ReceiveReturnIntakeAsync(
        SoulFrameReturnIntakeRequest request,
        CancellationToken cancellationToken = default);
}
