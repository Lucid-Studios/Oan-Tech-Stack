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
    CrypticReengrammitizationCompleted = 7,
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
    Annotation = 4
}

public enum GovernanceActKind
{
    Reengrammitization = 0,
    PrimePointerPublication = 1,
    PrimeCheckedViewPublication = 2,
    Recovery = 3,
    Completion = 4
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
    RetainInMos = 0,
    DeferReview = 1
}

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
    string ResultType,
    bool EngramCommitRequired);

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
    string CandidatePayload);

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
    GovernedPrimeDerivativeLane AuthorizedDerivativeLanes);

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
    string ReasonCode,
    string IssuedBy,
    DateTime IssuedAt,
    string TargetClass,
    bool ReviewRequired);

public sealed record GovernanceGoldenPathResult(
    Guid CandidateId,
    string LoopKey,
    GovernanceLoopStage Stage,
    GovernanceDecisionReceipt DecisionReceipt,
    CrypticReengrammitizationReceipt? ReengrammitizationReceipt,
    GovernedPrimeDerivativeLane PublishedLanes,
    string? FailureCode,
    CmeCollapseRoutingDecision? CollapseRoutingDecision = null);

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
    string? FailureCode,
    GovernanceLoopStage? FailureStage,
    bool ResumeEligible,
    bool HasJournalIntegrityErrors,
    int JournalIntegrityErrorCount);

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
    string? RequestFingerprint);

public sealed record GovernanceJournalEntry(
    string LoopKey,
    GovernanceJournalEntryKind Kind,
    GovernanceLoopStage Stage,
    DateTime Timestamp,
    GovernanceDecisionReceipt? DecisionReceipt,
    DeferredReviewRecord? DeferredReview,
    GovernanceActReceipt? ActReceipt,
    ReturnCandidateReviewRequest? ReviewRequest,
    GovernanceDeferredAnnotation? Annotation);

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
    bool ReengrammitizationCompleted,
    bool IsTerminal,
    string? FailureCode,
    GovernanceLoopStage? FailureStage,
    int JournalIntegrityErrorCount);

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
        var reengrammitizationCompleted = false;
        var stage = GovernanceLoopStage.SourceCustodyAvailable;
        string? failureCode = null;
        GovernanceLoopStage? failureStage = null;

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
            reengrammitizationCompleted,
            isTerminal,
            failureCode,
            failureStage,
            journalIntegrityIssues);
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
            GovernanceLoopStage.GovernanceDecisionApproved => next is GovernanceLoopStage.CrypticReengrammitizationCompleted or GovernanceLoopStage.PendingRecovery,
            GovernanceLoopStage.CrypticReengrammitizationCompleted => next is GovernanceLoopStage.PrimeDerivativePublished or GovernanceLoopStage.PendingRecovery,
            GovernanceLoopStage.PrimeDerivativePublished => next is GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.PendingRecovery,
            GovernanceLoopStage.PendingRecovery => next is GovernanceLoopStage.CrypticReengrammitizationCompleted or GovernanceLoopStage.PrimeDerivativePublished or GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.LoopFailed,
            _ => false
        };
    }

    public static CmeCollapseRoutingDecision? BuildCollapseRoutingDecision(
        GovernanceDecisionReceipt decisionReceipt,
        bool reengrammitizationCompleted)
    {
        ArgumentNullException.ThrowIfNull(decisionReceipt);

        return decisionReceipt.Decision switch
        {
            GovernanceDecision.Deferred => new CmeCollapseRoutingDecision(
                Disposition: CmeCollapseDisposition.DeferReview,
                ReasonCode: decisionReceipt.RationaleCode,
                IssuedBy: decisionReceipt.AdjudicatorIdentity,
                IssuedAt: decisionReceipt.Timestamp,
                TargetClass: "deferred-review-backlog",
                ReviewRequired: true),
            GovernanceDecision.Approved when reengrammitizationCompleted => new CmeCollapseRoutingDecision(
                Disposition: CmeCollapseDisposition.RetainInMos,
                ReasonCode: decisionReceipt.RationaleCode,
                IssuedBy: decisionReceipt.AdjudicatorIdentity,
                IssuedAt: decisionReceipt.Timestamp,
                TargetClass: "cMoS",
                ReviewRequired: false),
            _ => null
        };
    }
}
