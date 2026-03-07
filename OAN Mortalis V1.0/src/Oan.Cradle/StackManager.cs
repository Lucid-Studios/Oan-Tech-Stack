using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Oan.AgentiCore;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Sli;

namespace Oan.Cradle
{
    /// <summary>
    /// Orchestrator for constructing the agent stack.
    /// </summary>
    public sealed class StackManager : IGovernanceLoopStatusReader, IPendingRecoveryCoordinator
    {
        private readonly StoreRegistry _stores;
        private readonly ConcurrentDictionary<string, Task<GovernanceGoldenPathResult>> _activeLoopTasks = new(StringComparer.Ordinal);

        public StackManager(StoreRegistry stores)
        {
            _stores = stores ?? throw new ArgumentNullException(nameof(stores));
        }

        public AgentRuntime CreateStack(string agentId, string theaterId)
        {
            // 1. Create SoulFrame first
            var soulFrame = new SoulFrameAuthority(_stores.GovernanceTelemetry);

            // 2. Create Deterministic Harness (Engine gateway)
            var harness = new DeterministicHarness(_stores.Cryptic);

            // 3. Create Routing Engine (SLI layer)
            var router = new RoutingEngine(soulFrame, _stores.PrimeDerivativePublisher, _stores.Cryptic, harness);

            // 3. Create AgentiCore with SoulFrame authority reference
            var agentiCore = new AgentIdentity(soulFrame);

            // 4. Produce AgentRuntime with injected router
            return new AgentRuntime(agentId, theaterId, soulFrame, agentiCore, router);
        }

        public async Task<GovernanceLoopRunResponse> StartGovernanceGoldenPathAsync(
            GovernanceCycleStartRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var workResult = await RequireGovernanceCognitionService()
                .ExecuteGovernanceCycleAsync(request, cancellationToken)
                .ConfigureAwait(false);
            var reviewRequest = CreateReviewRequest(workResult);
            var loopKey = GovernanceLoopKeys.Create(workResult.CandidateId, workResult.ProvenanceMarker);

            if (_activeLoopTasks.TryGetValue(loopKey, out _))
            {
                return new GovernanceLoopRunResponse(
                    GovernanceLoopControlState.InProgress,
                    await GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false),
                    Result: null);
            }

            var task = ExecuteGovernanceGoldenPathCoreAsync(loopKey, reviewRequest, cancellationToken);
            if (!_activeLoopTasks.TryAdd(loopKey, task))
            {
                return new GovernanceLoopRunResponse(
                    GovernanceLoopControlState.InProgress,
                    await GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false),
                    Result: null);
            }

            try
            {
                var result = await task.ConfigureAwait(false);
                var status = await GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
                return new GovernanceLoopRunResponse(status.ControlState, status, result);
            }
            finally
            {
                _activeLoopTasks.TryRemove(new KeyValuePair<string, Task<GovernanceGoldenPathResult>>(loopKey, task));
            }
        }

        public async Task<GovernanceGoldenPathResult> RunGovernanceGoldenPathAsync(
            GovernanceCycleStartRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var workResult = await RequireGovernanceCognitionService()
                .ExecuteGovernanceCycleAsync(request, cancellationToken)
                .ConfigureAwait(false);
            var reviewRequest = CreateReviewRequest(workResult);
            var loopKey = GovernanceLoopKeys.Create(workResult.CandidateId, workResult.ProvenanceMarker);

            if (_activeLoopTasks.TryGetValue(loopKey, out var existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            var task = ExecuteGovernanceGoldenPathCoreAsync(loopKey, reviewRequest, cancellationToken);
            if (!_activeLoopTasks.TryAdd(loopKey, task) &&
                _activeLoopTasks.TryGetValue(loopKey, out existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            try
            {
                return await task.ConfigureAwait(false);
            }
            finally
            {
                _activeLoopTasks.TryRemove(new KeyValuePair<string, Task<GovernanceGoldenPathResult>>(loopKey, task));
            }
        }

        public async Task<GovernanceLoopStatusView> GetStatusByCandidateAsync(
            Guid candidateId,
            string provenance,
            CancellationToken cancellationToken = default)
        {
            return await GetStatusByLoopKeyAsync(
                GovernanceLoopKeys.Create(candidateId, provenance),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<GovernanceLoopStatusView> GetStatusByLoopKeyAsync(
            string loopKey,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
            var batch = await RequireGovernanceReceiptJournal()
                .ReplayLoopBatchAsync(loopKey, cancellationToken)
                .ConfigureAwait(false);
            var snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
            return BuildStatusView(loopKey, snapshot, _activeLoopTasks.ContainsKey(loopKey));
        }

        public async Task<IReadOnlyList<PendingRecoveryItemView>> ListPendingRecoveryAsync(
            CancellationToken cancellationToken = default)
        {
            var batch = await RequireGovernanceReceiptJournal()
                .ReplayBatchAsync(cancellationToken)
                .ConfigureAwait(false);

            return batch.Entries
                .Select(entry => entry.LoopKey)
                .Distinct(StringComparer.Ordinal)
                .Select(loopKey =>
                {
                    var loopBatch = new GovernanceJournalReplayBatch(
                        batch.Entries.Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal)).ToArray(),
                        batch.Issues.Where(issue => string.IsNullOrWhiteSpace(issue.LoopKey) ||
                                                    string.Equals(issue.LoopKey, loopKey, StringComparison.Ordinal)).ToArray());
                    var snapshot = GovernanceLoopStateModel.Project(loopBatch, loopKey);
                    return new { LoopKey = loopKey, Snapshot = snapshot, Status = BuildStatusView(loopKey, snapshot, _activeLoopTasks.ContainsKey(loopKey)) };
                })
                .Where(item => item.Status.ControlState == GovernanceLoopControlState.PendingRecovery)
                .Select(item => new PendingRecoveryItemView(
                    item.LoopKey,
                    item.Snapshot.DecisionReceipt?.CandidateId ?? Guid.Empty,
                    item.Snapshot.DecisionReceipt?.CandidateProvenance ?? string.Empty,
                    item.Snapshot.Stage,
                    item.Snapshot.FailureCode,
                    BuildPublicationLaneStatus(item.Snapshot.PublishedLanes),
                    item.Snapshot.ReengrammitizationCompleted,
                    item.Status.ResumeEligible))
                .OrderBy(item => item.LoopKey, StringComparer.Ordinal)
                .ToArray();
        }

        public async Task<GovernanceGoldenPathResult> ResumeGovernanceLoopAsync(
            ResumeGovernanceLoopRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.LoopKey);

            if (_activeLoopTasks.TryGetValue(request.LoopKey, out var existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            var task = ResumeGovernanceGoldenPathCoreAsync(request.LoopKey, retryLane: null, cancellationToken);
            if (!_activeLoopTasks.TryAdd(request.LoopKey, task) &&
                _activeLoopTasks.TryGetValue(request.LoopKey, out existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            try
            {
                return await task.ConfigureAwait(false);
            }
            finally
            {
                _activeLoopTasks.TryRemove(new KeyValuePair<string, Task<GovernanceGoldenPathResult>>(request.LoopKey, task));
            }
        }

        public async Task<GovernanceGoldenPathResult> RetryPublicationLaneAsync(
            ResumePublicationLaneRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.LoopKey);
            if (request.Lane == GovernedPrimeDerivativeLane.Neither ||
                request.Lane == (GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView))
            {
                throw new InvalidOperationException("RetryPublicationLaneAsync requires exactly one derivative lane.");
            }

            if (_activeLoopTasks.TryGetValue(request.LoopKey, out var existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            var task = ResumeGovernanceGoldenPathCoreAsync(request.LoopKey, request.Lane, cancellationToken);
            if (!_activeLoopTasks.TryAdd(request.LoopKey, task) &&
                _activeLoopTasks.TryGetValue(request.LoopKey, out existingTask))
            {
                return await existingTask.ConfigureAwait(false);
            }

            try
            {
                return await task.ConfigureAwait(false);
            }
            finally
            {
                _activeLoopTasks.TryRemove(new KeyValuePair<string, Task<GovernanceGoldenPathResult>>(request.LoopKey, task));
            }
        }

        private async Task<GovernanceGoldenPathResult> ExecuteGovernanceGoldenPathCoreAsync(
            string loopKey,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            var adjudicator = RequireReturnGovernanceAdjudicator();
            var journal = RequireGovernanceReceiptJournal();
            var batch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
            var snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
            if (snapshot.IsTerminal)
            {
                return BuildResult(reviewRequest.CandidateId, loopKey, snapshot);
            }

            var decisionReceipt = snapshot.DecisionReceipt;
            if (decisionReceipt is null)
            {
                var adjudication = await adjudicator.AdjudicateAsync(reviewRequest, cancellationToken).ConfigureAwait(false);
                decisionReceipt = adjudication.Receipt;
                batch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
                snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
                if (snapshot.DecisionReceipt is null)
                {
                    var stage = decisionReceipt.Decision switch
                    {
                        GovernanceDecision.Approved => GovernanceLoopStage.GovernanceDecisionApproved,
                        GovernanceDecision.Rejected => GovernanceLoopStage.GovernanceDecisionRejected,
                        GovernanceDecision.Deferred => GovernanceLoopStage.GovernanceDecisionDeferred,
                        _ => GovernanceLoopStage.GovernanceDecisionRejected
                    };

                    await journal.AppendAsync(
                        new GovernanceJournalEntry(
                            loopKey,
                            GovernanceJournalEntryKind.Decision,
                            stage,
                            decisionReceipt.Timestamp,
                            decisionReceipt,
                            DeferredReview: null,
                            ActReceipt: null,
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);

                    if (decisionReceipt.Decision == GovernanceDecision.Deferred)
                    {
                        await journal.AppendAsync(
                            new GovernanceJournalEntry(
                                loopKey,
                                GovernanceJournalEntryKind.DeferredReview,
                                GovernanceLoopStage.GovernanceDecisionDeferred,
                                decisionReceipt.Timestamp,
                                decisionReceipt,
                                new DeferredReviewRecord(
                                    loopKey,
                                    reviewRequest.CandidateId,
                                    reviewRequest.ProvenanceMarker,
                                    decisionReceipt.AdjudicatorIdentity,
                                    decisionReceipt.RationaleCode,
                                    decisionReceipt.Timestamp),
                                ActReceipt: null,
                                ReviewRequest: reviewRequest,
                                Annotation: null),
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                batch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
                snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
            }

            if (decisionReceipt.Decision is GovernanceDecision.Rejected or GovernanceDecision.Deferred)
            {
                return BuildResult(reviewRequest.CandidateId, loopKey, snapshot);
            }

            return await ContinueApprovedLoopAsync(loopKey, reviewRequest, decisionReceipt, snapshot, retryLane: null, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<GovernanceGoldenPathResult> ResumeGovernanceGoldenPathCoreAsync(
            string loopKey,
            GovernedPrimeDerivativeLane? retryLane,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var batch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
            var snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
            var controlState = GovernanceLoopStateModel.ClassifyControlState(snapshot, isInProgress: false);

            if (snapshot.ReviewRequest is null || snapshot.DecisionReceipt is null)
            {
                throw new InvalidOperationException("Resume requires a persisted review request and governance decision.");
            }

            if (controlState is GovernanceLoopControlState.NotFound or GovernanceLoopControlState.Completed or GovernanceLoopControlState.Failed)
            {
                throw new InvalidOperationException($"Loop '{loopKey}' cannot be resumed from state '{controlState}'.");
            }

            if (controlState == GovernanceLoopControlState.Deferred)
            {
                throw new InvalidOperationException($"Loop '{loopKey}' is deferred and must be explicitly reviewed before it can resume.");
            }

            if (snapshot.DecisionReceipt.Decision != GovernanceDecision.Approved)
            {
                throw new InvalidOperationException("Only approved governance decisions may resume downstream acts.");
            }

            return await ContinueApprovedLoopAsync(loopKey, snapshot.ReviewRequest, snapshot.DecisionReceipt, snapshot, retryLane, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<GovernanceGoldenPathResult> ContinueApprovedLoopAsync(
            string loopKey,
            ReturnCandidateReviewRequest reviewRequest,
            GovernanceDecisionReceipt decisionReceipt,
            GovernanceLoopStateSnapshot snapshot,
            GovernedPrimeDerivativeLane? retryLane,
            CancellationToken cancellationToken)
        {
            var adjudicator = RequireReturnGovernanceAdjudicator();
            var reengrammitizationGate = RequireCrypticReengrammitizationGate();
            var publicationSink = RequireGovernedPrimePublicationSink();
            var journal = RequireGovernanceReceiptJournal();

            var reengrammitizationRequest = adjudicator.CreateReengrammitizationRequest(reviewRequest, decisionReceipt)
                ?? throw new InvalidOperationException("Approved governance decision must authorize re-engrammitization.");
            var publicationRequest = adjudicator.CreatePrimePublicationRequest(reviewRequest, decisionReceipt)
                ?? throw new InvalidOperationException("Approved governance decision must authorize Prime publication.");

            var stage = snapshot.Stage == GovernanceLoopStage.SourceCustodyAvailable
                ? GovernanceLoopStage.GovernanceDecisionApproved
                : snapshot.Stage;
            var reengrammitizationReceipt = snapshot.ReengrammitizationReceipt;

            if (!snapshot.ReengrammitizationCompleted)
            {
                try
                {
                    stage = stage == GovernanceLoopStage.PendingRecovery
                        ? GovernanceLoopStage.CrypticReengrammitizationCompleted
                        : GovernanceLoopStateModel.EnsureAllowedTransition(stage, GovernanceLoopStage.CrypticReengrammitizationCompleted);
                    reengrammitizationReceipt = await reengrammitizationGate
                        .ReengrammitizeAsync(reengrammitizationRequest, cancellationToken)
                        .ConfigureAwait(false);

                    await journal.AppendAsync(
                        new GovernanceJournalEntry(
                            loopKey,
                            GovernanceJournalEntryKind.ActReceipt,
                            GovernanceLoopStage.CrypticReengrammitizationCompleted,
                            reengrammitizationReceipt.Timestamp,
                            decisionReceipt,
                            DeferredReview: null,
                            new GovernanceActReceipt(
                                loopKey,
                                reengrammitizationRequest.IdempotencyKey,
                                GovernanceActKind.Reengrammitization,
                                GovernanceLoopStage.CrypticReengrammitizationCompleted,
                                Succeeded: true,
                                FailureCode: null,
                                reengrammitizationReceipt.Timestamp,
                                PublishedLanes: GovernedPrimeDerivativeLane.Neither,
                                ReceiptPointer: reengrammitizationReceipt.ReceiptPointer,
                                RequestFingerprint: ComputeFingerprint(reengrammitizationRequest)),
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await journal.AppendAsync(
                        new GovernanceJournalEntry(
                            loopKey,
                            GovernanceJournalEntryKind.ActReceipt,
                            GovernanceLoopStage.PendingRecovery,
                            DateTime.UtcNow,
                            decisionReceipt,
                            DeferredReview: null,
                            new GovernanceActReceipt(
                                loopKey,
                                reengrammitizationRequest.IdempotencyKey,
                                GovernanceActKind.Reengrammitization,
                                GovernanceLoopStage.PendingRecovery,
                                Succeeded: false,
                                FailureCode: $"reengrammitization-failed:{ex.GetType().Name}",
                                DateTime.UtcNow,
                                PublishedLanes: snapshot.PublishedLanes,
                                ReceiptPointer: null,
                                RequestFingerprint: ComputeFingerprint(reengrammitizationRequest)),
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);

                    return new GovernanceGoldenPathResult(
                        CandidateId: reviewRequest.CandidateId,
                        LoopKey: loopKey,
                        Stage: GovernanceLoopStage.PendingRecovery,
                        DecisionReceipt: decisionReceipt,
                        ReengrammitizationReceipt: null,
                        PublishedLanes: snapshot.PublishedLanes,
                        FailureCode: $"reengrammitization-failed:{ex.GetType().Name}");
                }
            }
            else
            {
                stage = GovernanceLoopStage.CrypticReengrammitizationCompleted;
            }

            var publishedLanes = snapshot.PublishedLanes;
            foreach (var lane in EnumerateMissingPublicationLanes(decisionReceipt.AuthorizedDerivativeLanes, publishedLanes, retryLane))
            {
                var laneRequest = publicationRequest with { AuthorizedLanes = lane };
                try
                {
                    var emitted = await publicationSink.PublishApprovedOutcomeAsync(laneRequest, cancellationToken).ConfigureAwait(false);
                    publishedLanes |= emitted;
                    stage = stage == GovernanceLoopStage.PendingRecovery
                        ? GovernanceLoopStage.PrimeDerivativePublished
                        : GovernanceLoopStateModel.EnsureAllowedTransition(stage, GovernanceLoopStage.PrimeDerivativePublished);

                    await journal.AppendAsync(
                        new GovernanceJournalEntry(
                            loopKey,
                            GovernanceJournalEntryKind.ActReceipt,
                            GovernanceLoopStage.PrimeDerivativePublished,
                            DateTime.UtcNow,
                            decisionReceipt,
                            DeferredReview: null,
                            new GovernanceActReceipt(
                                loopKey,
                                laneRequest.IdempotencyKey,
                                lane == GovernedPrimeDerivativeLane.Pointer
                                    ? GovernanceActKind.PrimePointerPublication
                                    : GovernanceActKind.PrimeCheckedViewPublication,
                                GovernanceLoopStage.PrimeDerivativePublished,
                                Succeeded: true,
                                FailureCode: null,
                                DateTime.UtcNow,
                                PublishedLanes: emitted,
                                ReceiptPointer: lane == GovernedPrimeDerivativeLane.Pointer
                                    ? laneRequest.PointerValue
                                    : laneRequest.CheckedViewValue,
                                RequestFingerprint: ComputeFingerprint(laneRequest)),
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await journal.AppendAsync(
                        new GovernanceJournalEntry(
                            loopKey,
                            GovernanceJournalEntryKind.ActReceipt,
                            GovernanceLoopStage.PendingRecovery,
                            DateTime.UtcNow,
                            decisionReceipt,
                            DeferredReview: null,
                            new GovernanceActReceipt(
                                loopKey,
                                laneRequest.IdempotencyKey,
                                lane == GovernedPrimeDerivativeLane.Pointer
                                    ? GovernanceActKind.PrimePointerPublication
                                    : GovernanceActKind.PrimeCheckedViewPublication,
                                GovernanceLoopStage.PendingRecovery,
                                Succeeded: false,
                                FailureCode: $"publication-failed:{lane}:{ex.GetType().Name}",
                                DateTime.UtcNow,
                                PublishedLanes: publishedLanes,
                                ReceiptPointer: null,
                                RequestFingerprint: ComputeFingerprint(laneRequest)),
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);

                    return new GovernanceGoldenPathResult(
                        CandidateId: reviewRequest.CandidateId,
                        LoopKey: loopKey,
                        Stage: GovernanceLoopStage.PendingRecovery,
                        DecisionReceipt: decisionReceipt,
                        ReengrammitizationReceipt: reengrammitizationReceipt,
                        PublishedLanes: publishedLanes,
                        FailureCode: $"publication-failed:{lane}:{ex.GetType().Name}");
                }
            }

            stage = stage == GovernanceLoopStage.PendingRecovery
                ? GovernanceLoopStage.LoopCompleted
                : GovernanceLoopStateModel.EnsureAllowedTransition(stage, GovernanceLoopStage.LoopCompleted);

            await journal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.State,
                    GovernanceLoopStage.LoopCompleted,
                    DateTime.UtcNow,
                    decisionReceipt,
                    DeferredReview: null,
                    new GovernanceActReceipt(
                        loopKey,
                        decisionReceipt.IdempotencyKey,
                        GovernanceActKind.Completion,
                        GovernanceLoopStage.LoopCompleted,
                        Succeeded: true,
                        FailureCode: null,
                        DateTime.UtcNow,
                        PublishedLanes: publishedLanes,
                        ReceiptPointer: null,
                        RequestFingerprint: null),
                    ReviewRequest: reviewRequest,
                    Annotation: null),
                cancellationToken).ConfigureAwait(false);

            return new GovernanceGoldenPathResult(
                CandidateId: reviewRequest.CandidateId,
                LoopKey: loopKey,
                Stage: stage,
                DecisionReceipt: decisionReceipt,
                ReengrammitizationReceipt: reengrammitizationReceipt,
                PublishedLanes: publishedLanes,
                FailureCode: null);
        }

        private IGovernanceCycleCognitionService RequireGovernanceCognitionService() =>
            _stores.GovernanceCognitionService
            ?? throw new InvalidOperationException("Golden Path requires a governance cognition service.");

        private IReturnGovernanceAdjudicator RequireReturnGovernanceAdjudicator() =>
            _stores.ReturnGovernanceAdjudicator
            ?? throw new InvalidOperationException("Golden Path requires a return governance adjudicator.");

        private ICrypticReengrammitizationGate RequireCrypticReengrammitizationGate() =>
            _stores.CrypticReengrammitizationGate
            ?? throw new InvalidOperationException("Golden Path requires a Cryptic re-engrammitization gate.");

        private IGovernedPrimePublicationSink RequireGovernedPrimePublicationSink() =>
            _stores.GovernedPrimePublicationSink
            ?? throw new InvalidOperationException("Golden Path requires a governed Prime publication sink.");

        private IGovernanceReceiptJournal RequireGovernanceReceiptJournal() =>
            _stores.GovernanceReceiptJournal
            ?? throw new InvalidOperationException("Golden Path hardening requires a governance receipt journal.");

        private static ReturnCandidateReviewRequest CreateReviewRequest(GovernanceCycleWorkResult workResult)
        {
            return new ReturnCandidateReviewRequest(
                CandidateId: workResult.CandidateId,
                IdentityId: workResult.IdentityId,
                SoulFrameId: workResult.SoulFrameId,
                CMEId: workResult.CMEId,
                ContextId: workResult.ContextId,
                SourceTheater: workResult.SourceTheater,
                RequestedTheater: workResult.RequestedTheater,
                SessionHandle: workResult.SessionHandle,
                WorkingStateHandle: workResult.WorkingStateHandle,
                ReturnCandidatePointer: workResult.ReturnCandidatePointer,
                ProvenanceMarker: workResult.ProvenanceMarker,
                IntakeIntent: workResult.IntakeIntent,
                SubmittedBy: "AgentiCore",
                CandidatePayload: workResult.CandidatePayload);
        }

        private static GovernanceGoldenPathResult BuildResult(
            Guid candidateId,
            string loopKey,
            GovernanceLoopStateSnapshot snapshot)
        {
            if (snapshot.DecisionReceipt is null)
            {
                throw new InvalidOperationException("Loop snapshot is missing a governance decision receipt.");
            }

            return new GovernanceGoldenPathResult(
                CandidateId: candidateId,
                LoopKey: loopKey,
                Stage: snapshot.Stage,
                DecisionReceipt: snapshot.DecisionReceipt,
                ReengrammitizationReceipt: snapshot.ReengrammitizationReceipt,
                PublishedLanes: snapshot.PublishedLanes,
                FailureCode: snapshot.FailureCode);
        }

        private static GovernanceLoopStatusView BuildStatusView(
            string loopKey,
            GovernanceLoopStateSnapshot snapshot,
            bool isInProgress)
        {
            var controlState = GovernanceLoopStateModel.ClassifyControlState(snapshot, isInProgress);
            var latestDecision = snapshot.DecisionReceipt is null
                ? null
                : new GovernanceDecisionView(
                    snapshot.DecisionReceipt.CandidateId,
                    snapshot.DecisionReceipt.CandidateProvenance,
                    snapshot.DecisionReceipt.Decision,
                    snapshot.DecisionReceipt.AdjudicatorIdentity,
                    snapshot.DecisionReceipt.RationaleCode,
                    snapshot.DecisionReceipt.Timestamp,
                    snapshot.DecisionReceipt.ReengrammitizationAuthorized,
                    snapshot.DecisionReceipt.PrimePublicationAuthorized,
                    snapshot.DecisionReceipt.AuthorizedDerivativeLanes);

            return new GovernanceLoopStatusView(
                loopKey,
                snapshot.DecisionReceipt?.CandidateId,
                snapshot.DecisionReceipt?.CandidateProvenance,
                controlState,
                controlState == GovernanceLoopControlState.NotFound ? null : snapshot.Stage,
                latestDecision,
                snapshot.ReengrammitizationCompleted,
                BuildPublicationLaneStatus(snapshot.PublishedLanes),
                snapshot.FailureCode,
                snapshot.FailureStage,
                ResumeEligible: snapshot.DecisionReceipt?.Decision == GovernanceDecision.Approved &&
                                controlState is GovernanceLoopControlState.InProgress or GovernanceLoopControlState.PendingRecovery,
                HasJournalIntegrityErrors: snapshot.JournalIntegrityErrorCount > 0,
                JournalIntegrityErrorCount: snapshot.JournalIntegrityErrorCount);
        }

        private static PublicationLaneStatusView BuildPublicationLaneStatus(
            GovernedPrimeDerivativeLane publishedLanes)
        {
            return new PublicationLaneStatusView(
                publishedLanes,
                PointerPublished: publishedLanes.HasFlag(GovernedPrimeDerivativeLane.Pointer),
                CheckedViewPublished: publishedLanes.HasFlag(GovernedPrimeDerivativeLane.CheckedView));
        }

        private static IEnumerable<GovernedPrimeDerivativeLane> EnumerateMissingPublicationLanes(
            GovernedPrimeDerivativeLane authorized,
            GovernedPrimeDerivativeLane published,
            GovernedPrimeDerivativeLane? retryLane)
        {
            if (authorized.HasFlag(GovernedPrimeDerivativeLane.Pointer) &&
                !published.HasFlag(GovernedPrimeDerivativeLane.Pointer) &&
                (retryLane is null || retryLane == GovernedPrimeDerivativeLane.Pointer))
            {
                yield return GovernedPrimeDerivativeLane.Pointer;
            }

            if (authorized.HasFlag(GovernedPrimeDerivativeLane.CheckedView) &&
                !published.HasFlag(GovernedPrimeDerivativeLane.CheckedView) &&
                (retryLane is null || retryLane == GovernedPrimeDerivativeLane.CheckedView))
            {
                yield return GovernedPrimeDerivativeLane.CheckedView;
            }
        }

        private static string ComputeFingerprint(object payload)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
