namespace Oan.Common;

[Flags]
public enum GovernedPrimeDerivativeLane
{
    Neither = 0,
    Pointer = 1,
    CheckedView = 2
}

public enum GovernanceDecision
{
    Approved = 0,
    Rejected = 1,
    Deferred = 2
}

public enum GovernanceLoopStage
{
    SourceCustodyAvailable = 0,
    ProjectionIssued = 1,
    BoundedCognitionCompleted = 2,
    ReturnCandidateSubmitted = 3,
    GovernanceDecisionApproved = 4,
    GovernanceDecisionRejected = 5,
    GovernanceDecisionDeferred = 6,
    CrypticFirstRouteCompleted = 7,
    PrimeDerivativePublished = 8,
    LoopCompleted = 9,
    PendingRecovery = 10,
    LoopFailed = 11
}

public enum GovernanceJournalEntryKind
{
    Decision = 0,
    DeferredReview = 1,
    ActReceipt = 2,
    State = 3,
    Annotation = 4,
    ArtifactReceipt = 5
}

public enum GovernanceActKind
{
    Reengrammitization = 0,
    PrimePointerPublication = 1,
    PrimeCheckedViewPublication = 2,
    Recovery = 3,
    Completion = 4,
    CollapseHoldToCMoS = 5,
    CollapseHoldToCGoA = 6
}

public enum GovernanceLoopControlState
{
    NotFound = 0,
    InProgress = 1,
    Completed = 2,
    Deferred = 3,
    PendingRecovery = 4,
    Failed = 5
}

public enum CmeCollapseDisposition
{
    RouteToCMoS = 0,
    RouteToCGoA = 1
}

public enum CmeCollapseReviewState
{
    None = 0,
    DeferredReview = 1
}

public enum CmeCollapseResidueClass
{
    AutobiographicalProtected = 0,
    ContextualProtected = 1
}

 [Flags]
public enum CmeCollapseEvidenceFlag
{
    None = 0,
    AutobiographicalSignal = 1,
    SelfGelIdentitySignal = 2,
    ContextualSignal = 4,
    ProceduralSignal = 8,
    SkillMethodSignal = 16,
    WitnessBearingSignal = 32,
    MixedSignal = 64
}

[Flags]
public enum CmeCollapseReviewTrigger
{
    None = 0,
    LowConfidence = 1,
    MixedIdentityContext = 2,
    PolicyReviewRequired = 4,
    InsufficientEvidence = 8
}

public sealed record CmeCollapseClassification(
    double CollapseConfidence,
    bool SelfGelIdentified,
    bool AutobiographicalRelevant,
    CmeCollapseEvidenceFlag EvidenceFlags,
    CmeCollapseReviewTrigger ReviewTriggers,
    string SourceSubsystem);

public sealed record CmeCollapseQualificationResult(
    CmeCollapseDisposition Disposition,
    CmeCollapseResidueClass ResidueClass,
    double ClassificationConfidence,
    CmeCollapseEvidenceFlag EvidenceFlags,
    CmeCollapseReviewTrigger ReviewTriggers,
    string SourceSubsystem,
    string TargetClass);

public sealed record GovernanceCycleStartRequest(
    Guid IdentityId,
    Guid SoulFrameId,
    string CMEId,
    string SourceCustodyDomain,
    string SourceTheater,
    string RequestedTheater,
    string PolicyHandle,
    string OperatorInput);

public sealed record GovernanceCycleWorkResult(
    Guid CandidateId,
    Guid IdentityId,
    Guid SoulFrameId,
    Guid ContextId,
    string CMEId,
    string SourceTheater,
    string RequestedTheater,
    string SessionHandle,
    string WorkingStateHandle,
    string ProvenanceMarker,
    string ReturnCandidatePointer,
    string IntakeIntent,
    string CandidatePayload,
    CmeCollapseClassification CollapseClassification,
    string ResultType,
    bool EngramCommitRequired,
    GovernedActionableContent ActionableContent,
    string ReturnIntakeHandle,
    string ReturnIntakeEnvelopeId);

public sealed record ReturnCandidateReviewRequest(
    Guid CandidateId,
    Guid IdentityId,
    Guid SoulFrameId,
    string CMEId,
    Guid ContextId,
    string SourceTheater,
    string RequestedTheater,
    string SessionHandle,
    string WorkingStateHandle,
    string ReturnCandidatePointer,
    string ProvenanceMarker,
    string IntakeIntent,
    string SubmittedBy,
    string CandidatePayload,
    CmeCollapseClassification CollapseClassification,
    GovernedControlSurfaceRequestEnvelope RequestEnvelope);

public sealed record GovernanceDecisionReceipt(
    Guid CandidateId,
    string IdempotencyKey,
    string CandidateProvenance,
    GovernanceDecision Decision,
    string AdjudicatorIdentity,
    string RationaleCode,
    DateTime Timestamp,
    bool ReengrammitizationAuthorized,
    bool PrimePublicationAuthorized,
    GovernedPrimeDerivativeLane AuthorizedDerivativeLanes,
    GovernedControlSurfaceMutationReceipt MutationReceipt);

public sealed record GovernedReengrammitizationRequest(
    Guid CandidateId,
    string IdempotencyKey,
    Guid IdentityId,
    string CMEId,
    string SourceTheater,
    string ResiduePointer,
    string Reason,
    string AuthorizedBy,
    DateTime AuthorizedAtUtc);

public sealed record GovernedPrimePublicationRequest(
    Guid CandidateId,
    string IdempotencyKey,
    Guid IdentityId,
    string PointerValue,
    string CheckedViewValue,
    string Classification,
    string AuthorizedBy,
    DateTime AuthorizedAtUtc,
    GovernedPrimeDerivativeLane AuthorizedLanes);

public sealed record GovernanceAdjudicationResult(
    GovernanceDecisionReceipt Receipt,
    GovernedReengrammitizationRequest? ReengrammitizationRequest,
    GovernedPrimePublicationRequest? PrimePublicationRequest);

public sealed record CmeCollapseRoutingDecision(
    CmeCollapseDisposition Disposition,
    CmeCollapseResidueClass ResidueClass,
    CmeCollapseReviewState ReviewState,
    string ReasonCode,
    string IssuedBy,
    DateTime IssuedAt,
    string TargetClass,
    double ClassificationConfidence,
    CmeCollapseEvidenceFlag EvidenceFlags,
    CmeCollapseReviewTrigger ReviewTriggers,
    string SourceSubsystem);

public sealed record CmeCollapseQualificationView(
    string Destination,
    CmeCollapseResidueClass ResidueClass,
    double ClassificationConfidence,
    CmeCollapseEvidenceFlag EvidenceFlags,
    CmeCollapseReviewTrigger ReviewTriggers,
    CmeCollapseReviewState ReviewState,
    string SourceSubsystem);

public sealed record GovernanceGoldenPathResult(
    Guid CandidateId,
    string LoopKey,
    GovernanceLoopStage Stage,
    GovernanceDecisionReceipt DecisionReceipt,
    CrypticReengrammitizationReceipt? ReengrammitizationReceipt,
    GovernedPrimeDerivativeLane PublishedLanes,
    string? FailureCode,
    CmeCollapseRoutingDecision? CollapseRoutingDecision = null,
    IReadOnlyList<GovernedHopngArtifactReceipt>? HopngArtifacts = null);

public sealed record GovernanceDecisionView(
    Guid CandidateId,
    string CandidateProvenance,
    GovernanceDecision Decision,
    string AdjudicatorIdentity,
    string RationaleCode,
    DateTime Timestamp,
    bool ReengrammitizationAuthorized,
    bool PrimePublicationAuthorized,
    GovernedPrimeDerivativeLane AuthorizedDerivativeLanes);

public sealed record PublicationLaneStatusView(
    GovernedPrimeDerivativeLane PublishedLanes,
    bool PointerPublished,
    bool CheckedViewPublished);

public sealed record GovernanceLoopStatusView(
    string LoopKey,
    Guid? CandidateId,
    string? CandidateProvenance,
    GovernanceLoopControlState ControlState,
    GovernanceLoopStage? Stage,
    GovernanceDecisionView? LatestDecision,
    bool ReengrammitizationCompleted,
    PublicationLaneStatusView Publication,
    CmeCollapseQualificationView? LatestCollapseQualification,
    string? FailureCode,
    GovernanceLoopStage? FailureStage,
    bool ResumeEligible,
    bool HasJournalIntegrityErrors,
    int JournalIntegrityErrorCount,
    IReadOnlyList<GovernedHopngArtifactReceipt>? HopngArtifacts = null);

public sealed record DeferredBacklogItemView(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    DateTime DeferredAtUtc,
    string AdjudicatorIdentity,
    string RationaleCode,
    string? LatestAnnotation,
    bool CanReview);

public sealed record PendingRecoveryItemView(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    GovernanceLoopStage Stage,
    string? FailureCode,
    PublicationLaneStatusView Publication,
    bool ReengrammitizationCompleted,
    bool CanResume);

public sealed record ReviewDeferredCandidateRequest(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    string ReviewedBy,
    string RationaleCode,
    string? Annotation);

public sealed record ResumeGovernanceLoopRequest(
    string LoopKey,
    string RequestedBy,
    string Reason);

public sealed record ResumePublicationLaneRequest(
    string LoopKey,
    GovernedPrimeDerivativeLane Lane,
    string RequestedBy,
    string Reason);

public sealed record GovernanceLoopRunResponse(
    GovernanceLoopControlState ControlState,
    GovernanceLoopStatusView Status,
    GovernanceGoldenPathResult? Result);

public sealed record DeferredReviewRecord(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    string AdjudicatorIdentity,
    string RationaleCode,
    DateTime DeferredAtUtc);

public sealed record GovernanceDeferredAnnotation(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    string AnnotatedBy,
    string Annotation,
    DateTime Timestamp);

public sealed record GovernanceActReceipt(
    string LoopKey,
    string IdempotencyKey,
    GovernanceActKind ActKind,
    GovernanceLoopStage Stage,
    bool Succeeded,
    string? FailureCode,
    DateTime Timestamp,
    GovernedPrimeDerivativeLane PublishedLanes,
    string? ReceiptPointer,
    string? RequestFingerprint,
    string? TargetClass = null,
    CmeCollapseResidueClass? ResidueClass = null,
    double? ClassificationConfidence = null,
    CmeCollapseEvidenceFlag EvidenceFlags = CmeCollapseEvidenceFlag.None,
    CmeCollapseReviewTrigger ReviewTriggers = CmeCollapseReviewTrigger.None,
    string? SourceSubsystem = null,
    GovernedControlSurfaceMutationReceipt? MutationReceipt = null);

public sealed record GovernanceJournalEntry(
    string LoopKey,
    GovernanceJournalEntryKind Kind,
    GovernanceLoopStage Stage,
    DateTime Timestamp,
    GovernanceDecisionReceipt? DecisionReceipt,
    DeferredReviewRecord? DeferredReview,
    GovernanceActReceipt? ActReceipt,
    ReturnCandidateReviewRequest? ReviewRequest,
    GovernanceDeferredAnnotation? Annotation,
    GovernedHopngArtifactReceipt? HopngArtifactReceipt = null);

public sealed record GovernanceJournalReplayIssue(
    int LineNumber,
    string Reason,
    string RawLine,
    string? LoopKey);

public sealed record GovernanceJournalReplayBatch(
    IReadOnlyList<GovernanceJournalEntry> Entries,
    IReadOnlyList<GovernanceJournalReplayIssue> Issues);

public sealed record GovernanceLoopStateSnapshot(
    string LoopKey,
    GovernanceLoopStage Stage,
    GovernanceDecisionReceipt? DecisionReceipt,
    ReturnCandidateReviewRequest? ReviewRequest,
    CrypticReengrammitizationReceipt? ReengrammitizationReceipt,
    GovernedPrimeDerivativeLane PublishedLanes,
    bool FirstRouteCompleted,
    CmeCollapseDisposition? FirstRouteDisposition,
    CmeCollapseQualificationView? LatestCollapseQualification,
    bool ReengrammitizationCompleted,
    bool IsTerminal,
    string? FailureCode,
    GovernanceLoopStage? FailureStage,
    int JournalIntegrityErrorCount,
    IReadOnlyList<GovernedHopngArtifactReceipt> HopngArtifacts);

public interface IGovernanceCycleCognitionService
{
    Task<GovernanceCycleWorkResult> ExecuteGovernanceCycleAsync(
        GovernanceCycleStartRequest request,
        CancellationToken cancellationToken = default);
}

public interface IReturnGovernanceAdjudicator
{
    Task<GovernanceAdjudicationResult> AdjudicateAsync(
        ReturnCandidateReviewRequest request,
        CancellationToken cancellationToken = default);

    GovernedReengrammitizationRequest? CreateReengrammitizationRequest(
        ReturnCandidateReviewRequest request,
        GovernanceDecisionReceipt receipt);

    GovernedPrimePublicationRequest? CreatePrimePublicationRequest(
        ReturnCandidateReviewRequest request,
        GovernanceDecisionReceipt receipt);
}

public interface ICmeCollapseQualifier
{
    CmeCollapseQualificationResult Qualify(CmeCollapseClassification classification);
}

public interface IGovernanceReceiptJournal
{
    Task AppendAsync(
        GovernanceJournalEntry entry,
        CancellationToken cancellationToken = default);

    Task<GovernanceJournalReplayBatch> ReplayBatchAsync(
        CancellationToken cancellationToken = default);

    Task<GovernanceJournalReplayBatch> ReplayLoopBatchAsync(
        string loopKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GovernanceJournalEntry>> ReplayAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GovernanceJournalEntry>> ReplayLoopAsync(
        string loopKey,
        CancellationToken cancellationToken = default);
}

public interface IGovernanceLoopStatusReader
{
    Task<GovernanceLoopStatusView> GetStatusByCandidateAsync(
        Guid candidateId,
        string provenance,
        CancellationToken cancellationToken = default);

    Task<GovernanceLoopStatusView> GetStatusByLoopKeyAsync(
        string loopKey,
        CancellationToken cancellationToken = default);
}

public interface IDeferredReviewQueue
{
    Task<IReadOnlyList<DeferredBacklogItemView>> ListDeferredAsync(
        CancellationToken cancellationToken = default);

    Task<DeferredBacklogItemView?> GetDeferredAsync(
        string loopKey,
        CancellationToken cancellationToken = default);

    Task<GovernanceAdjudicationResult> ApproveDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<GovernanceAdjudicationResult> RejectDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task AnnotateDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default);
}

public interface IPendingRecoveryCoordinator
{
    Task<IReadOnlyList<PendingRecoveryItemView>> ListPendingRecoveryAsync(
        CancellationToken cancellationToken = default);

    Task<GovernanceGoldenPathResult> ResumeGovernanceLoopAsync(
        ResumeGovernanceLoopRequest request,
        CancellationToken cancellationToken = default);

    Task<GovernanceGoldenPathResult> RetryPublicationLaneAsync(
        ResumePublicationLaneRequest request,
        CancellationToken cancellationToken = default);
}

public static class GovernanceLoopKeys
{
    public static string Create(Guid candidateId, string provenance)
    {
        var normalizedCandidate = candidateId.ToString("D");
        var normalizedProvenance = NormalizeProvenance(provenance);
        var material = $"{normalizedCandidate}|{normalizedProvenance}";
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(material));
        return $"loop:{normalizedCandidate}:{Convert.ToHexString(bytes).ToLowerInvariant()[..16]}";
    }

    public static string NormalizeProvenance(string provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provenance);
        return provenance.Trim();
    }
}

public static class GovernanceLoopStateModel
{
    public static GovernanceLoopStage EnsureAllowedTransition(
        GovernanceLoopStage current,
        GovernanceLoopStage next)
    {
        if (!IsAllowedTransition(current, next))
        {
            throw new InvalidOperationException($"Illegal governance loop transition '{current}' -> '{next}'.");
        }

        return next;
    }

    public static GovernanceLoopStateSnapshot Project(
        string loopKey,
        IReadOnlyList<GovernanceJournalEntry> entries)
    {
        return Project(new GovernanceJournalReplayBatch(entries, []), loopKey);
    }

    public static GovernanceLoopStateSnapshot Project(
        GovernanceJournalReplayBatch batch,
        string loopKey)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);

        GovernanceDecisionReceipt? decisionReceipt = null;
        ReturnCandidateReviewRequest? reviewRequest = null;
        CrypticReengrammitizationReceipt? reengrammitizationReceipt = null;
        var publishedLanes = GovernedPrimeDerivativeLane.Neither;
        var firstRouteCompleted = false;
        CmeCollapseDisposition? firstRouteDisposition = null;
        CmeCollapseQualificationView? latestCollapseQualification = null;
        var reengrammitizationCompleted = false;
        var stage = GovernanceLoopStage.SourceCustodyAvailable;
        string? failureCode = null;
        GovernanceLoopStage? failureStage = null;
        var hopngArtifactsByProfile = new Dictionary<GovernedHopngArtifactProfile, GovernedHopngArtifactReceipt>();

        foreach (var entry in batch.Entries
                     .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal)))
        {
            stage = entry.Stage;

            if (entry.DecisionReceipt is not null)
            {
                decisionReceipt = entry.DecisionReceipt;
            }

            if (entry.ReviewRequest is not null)
            {
                reviewRequest = entry.ReviewRequest;
            }

            if (entry.HopngArtifactReceipt is not null)
            {
                hopngArtifactsByProfile[entry.HopngArtifactReceipt.Profile] = entry.HopngArtifactReceipt;
            }

            if (entry.ActReceipt is not null)
            {
                if (!entry.ActReceipt.Succeeded)
                {
                    failureCode = entry.ActReceipt.FailureCode ?? failureCode;
                    failureStage = entry.ActReceipt.Stage;
                }

                publishedLanes |= entry.ActReceipt.PublishedLanes;
                if (entry.ActReceipt.ActKind == GovernanceActKind.Reengrammitization && entry.ActReceipt.Succeeded)
                {
                    reengrammitizationCompleted = true;
                    firstRouteCompleted = true;
                    firstRouteDisposition = CmeCollapseDisposition.RouteToCMoS;
                    if (entry.ActReceipt.ReceiptPointer is not null && decisionReceipt is not null)
                    {
                        reengrammitizationReceipt = new CrypticReengrammitizationReceipt(
                            decisionReceipt.CandidateId,
                            CustodyDomain: "cMoS",
                            ReceiptPointer: entry.ActReceipt.ReceiptPointer,
                            Accepted: true,
                            entry.ActReceipt.Timestamp);
                    }
                }

                if (entry.ActReceipt.ActKind == GovernanceActKind.Reengrammitization)
                {
                    latestCollapseQualification = BuildCollapseQualificationView(
                        entry.ActReceipt,
                        defaultTargetClass: "cMoS",
                        defaultResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                        defaultReviewState: stage == GovernanceLoopStage.GovernanceDecisionDeferred
                            ? CmeCollapseReviewState.DeferredReview
                            : CmeCollapseReviewState.None,
                        defaultSourceSubsystem: reviewRequest?.CollapseClassification.SourceSubsystem);
                }

                if (entry.ActReceipt.ActKind == GovernanceActKind.CollapseHoldToCMoS && entry.ActReceipt.Succeeded)
                {
                    firstRouteCompleted = true;
                    firstRouteDisposition = CmeCollapseDisposition.RouteToCMoS;
                }

                if (entry.ActReceipt.ActKind == GovernanceActKind.CollapseHoldToCMoS)
                {
                    latestCollapseQualification = BuildCollapseQualificationView(
                        entry.ActReceipt,
                        defaultTargetClass: "cMoS",
                        defaultResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                        defaultReviewState: stage == GovernanceLoopStage.GovernanceDecisionDeferred
                            ? CmeCollapseReviewState.DeferredReview
                            : CmeCollapseReviewState.None,
                        defaultSourceSubsystem: reviewRequest?.CollapseClassification.SourceSubsystem);
                }

                if (entry.ActReceipt.ActKind == GovernanceActKind.CollapseHoldToCGoA && entry.ActReceipt.Succeeded)
                {
                    firstRouteCompleted = true;
                    firstRouteDisposition = CmeCollapseDisposition.RouteToCGoA;
                }

                if (entry.ActReceipt.ActKind == GovernanceActKind.CollapseHoldToCGoA)
                {
                    latestCollapseQualification = BuildCollapseQualificationView(
                        entry.ActReceipt,
                        defaultTargetClass: "cGoA",
                        defaultResidueClass: CmeCollapseResidueClass.ContextualProtected,
                        defaultReviewState: stage == GovernanceLoopStage.GovernanceDecisionDeferred
                            ? CmeCollapseReviewState.DeferredReview
                            : CmeCollapseReviewState.None,
                        defaultSourceSubsystem: reviewRequest?.CollapseClassification.SourceSubsystem);
                }
            }
        }

        var journalIntegrityIssues = batch.Issues.Count(issue =>
            string.IsNullOrWhiteSpace(issue.LoopKey) ||
            string.Equals(issue.LoopKey, loopKey, StringComparison.Ordinal));

        var isTerminal =
            stage is GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.GovernanceDecisionRejected;

        return new GovernanceLoopStateSnapshot(
            loopKey,
            stage,
            decisionReceipt,
            reviewRequest,
            reengrammitizationReceipt,
            publishedLanes,
            firstRouteCompleted,
            firstRouteDisposition,
            latestCollapseQualification,
            reengrammitizationCompleted,
            isTerminal,
            failureCode,
            failureStage,
            journalIntegrityIssues,
            hopngArtifactsByProfile
                .Values
                .OrderBy(receipt => receipt.Profile)
                .ToArray());
    }

    public static GovernanceLoopControlState ClassifyControlState(
        GovernanceLoopStateSnapshot snapshot,
        bool isInProgress)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.JournalIntegrityErrorCount > 0 || snapshot.Stage is GovernanceLoopStage.LoopFailed or GovernanceLoopStage.GovernanceDecisionRejected)
        {
            return GovernanceLoopControlState.Failed;
        }

        if (snapshot.DecisionReceipt is null && snapshot.Stage == GovernanceLoopStage.SourceCustodyAvailable && !isInProgress)
        {
            return GovernanceLoopControlState.NotFound;
        }

        if (snapshot.Stage == GovernanceLoopStage.LoopCompleted)
        {
            return GovernanceLoopControlState.Completed;
        }

        if (snapshot.Stage == GovernanceLoopStage.GovernanceDecisionDeferred)
        {
            return GovernanceLoopControlState.Deferred;
        }

        if (snapshot.Stage == GovernanceLoopStage.PendingRecovery)
        {
            return GovernanceLoopControlState.PendingRecovery;
        }

        return isInProgress || snapshot.DecisionReceipt is not null
            ? GovernanceLoopControlState.InProgress
            : GovernanceLoopControlState.NotFound;
    }

    private static bool IsAllowedTransition(GovernanceLoopStage current, GovernanceLoopStage next)
    {
        if (current == next)
        {
            return true;
        }

        return current switch
        {
            GovernanceLoopStage.SourceCustodyAvailable => next == GovernanceLoopStage.ProjectionIssued,
            GovernanceLoopStage.ProjectionIssued => next == GovernanceLoopStage.BoundedCognitionCompleted,
            GovernanceLoopStage.BoundedCognitionCompleted => next == GovernanceLoopStage.ReturnCandidateSubmitted,
            GovernanceLoopStage.ReturnCandidateSubmitted => next is GovernanceLoopStage.GovernanceDecisionApproved or GovernanceLoopStage.GovernanceDecisionRejected or GovernanceLoopStage.GovernanceDecisionDeferred,
            GovernanceLoopStage.GovernanceDecisionApproved => next is GovernanceLoopStage.CrypticFirstRouteCompleted or GovernanceLoopStage.PendingRecovery,
            GovernanceLoopStage.CrypticFirstRouteCompleted => next is GovernanceLoopStage.PrimeDerivativePublished or GovernanceLoopStage.PendingRecovery or GovernanceLoopStage.LoopCompleted,
            GovernanceLoopStage.PrimeDerivativePublished => next is GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.PendingRecovery,
            GovernanceLoopStage.PendingRecovery => next is GovernanceLoopStage.CrypticFirstRouteCompleted or GovernanceLoopStage.PrimeDerivativePublished or GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.LoopFailed,
            _ => false
        };
    }

    public static CmeCollapseRoutingDecision? BuildCollapseRoutingDecision(
        GovernanceDecisionReceipt decisionReceipt,
        CmeCollapseQualificationResult qualification)
    {
        ArgumentNullException.ThrowIfNull(decisionReceipt);
        ArgumentNullException.ThrowIfNull(qualification);

        if (decisionReceipt.Decision == GovernanceDecision.Rejected)
        {
            return null;
        }

        return new CmeCollapseRoutingDecision(
            Disposition: qualification.Disposition,
            ResidueClass: qualification.ResidueClass,
            ReviewState: decisionReceipt.Decision == GovernanceDecision.Deferred
                ? CmeCollapseReviewState.DeferredReview
                : CmeCollapseReviewState.None,
            ReasonCode: decisionReceipt.RationaleCode,
            IssuedBy: decisionReceipt.AdjudicatorIdentity,
            IssuedAt: decisionReceipt.Timestamp,
            TargetClass: qualification.TargetClass,
            ClassificationConfidence: qualification.ClassificationConfidence,
            EvidenceFlags: qualification.EvidenceFlags,
            ReviewTriggers: qualification.ReviewTriggers,
            SourceSubsystem: qualification.SourceSubsystem);
    }

    private static CmeCollapseQualificationView? BuildCollapseQualificationView(
        GovernanceActReceipt receipt,
        string defaultTargetClass,
        CmeCollapseResidueClass defaultResidueClass,
        CmeCollapseReviewState defaultReviewState,
        string? defaultSourceSubsystem)
    {
        return new CmeCollapseQualificationView(
            Destination: receipt.TargetClass ?? defaultTargetClass,
            ResidueClass: receipt.ResidueClass ?? defaultResidueClass,
            ClassificationConfidence: receipt.ClassificationConfidence ?? 0d,
            EvidenceFlags: receipt.EvidenceFlags,
            ReviewTriggers: receipt.ReviewTriggers,
            ReviewState: defaultReviewState,
            SourceSubsystem: receipt.SourceSubsystem ?? defaultSourceSubsystem ?? "unknown");
    }
}
