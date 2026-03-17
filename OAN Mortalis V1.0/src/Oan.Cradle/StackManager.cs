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
            await WitnessCompassObservationAsync(loopKey, workResult, reviewRequest, cancellationToken).ConfigureAwait(false);

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
            await WitnessCompassObservationAsync(loopKey, workResult, reviewRequest, cancellationToken).ConfigureAwait(false);

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
                return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
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

            if (decisionReceipt.Decision == GovernanceDecision.Rejected)
            {
                return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
            }

            var collapseQualification = RequireCmeCollapseQualifier().Qualify(reviewRequest.CollapseClassification);
            var collapseRoutingDecision = GovernanceLoopStateModel.BuildCollapseRoutingDecision(decisionReceipt, collapseQualification)
                ?? throw new InvalidOperationException("Approved or deferred governance decisions must produce a collapse routing decision.");

            if (decisionReceipt.Decision == GovernanceDecision.Deferred)
            {
                var deferredResult = await EnsureDeferredHoldAsync(
                        loopKey,
                        reviewRequest,
                        decisionReceipt,
                        collapseRoutingDecision,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (deferredResult is not null)
                {
                    return deferredResult;
                }

                batch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
                snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
                return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
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
            var publicationSink = RequireGovernedPrimePublicationSink();
            var journal = RequireGovernanceReceiptJournal();
            var collapseQualification = RequireCmeCollapseQualifier().Qualify(reviewRequest.CollapseClassification);
            var collapseRoutingDecision = GovernanceLoopStateModel.BuildCollapseRoutingDecision(decisionReceipt, collapseQualification)
                ?? throw new InvalidOperationException("Approved governance decision must produce a collapse routing decision.");
            var publicationRequest = adjudicator.CreatePrimePublicationRequest(reviewRequest, decisionReceipt)
                ?? throw new InvalidOperationException("Approved governance decision must authorize Prime publication.");

            var stage = snapshot.Stage == GovernanceLoopStage.SourceCustodyAvailable
                ? GovernanceLoopStage.GovernanceDecisionApproved
                : snapshot.Stage;
            var reengrammitizationReceipt = snapshot.ReengrammitizationReceipt;

            if (collapseRoutingDecision.Disposition == CmeCollapseDisposition.RouteToCMoS)
            {
                var reengrammitizationRequest = adjudicator.CreateReengrammitizationRequest(reviewRequest, decisionReceipt)
                    ?? throw new InvalidOperationException("Approved autobiographical protected routing must authorize re-engrammitization.");
                var reengrammitizationGate = RequireCrypticReengrammitizationGate();

                if (!snapshot.ReengrammitizationCompleted)
                {
                    try
                    {
                        stage = stage == GovernanceLoopStage.PendingRecovery
                            ? GovernanceLoopStage.CrypticFirstRouteCompleted
                            : GovernanceLoopStateModel.EnsureAllowedTransition(stage, GovernanceLoopStage.CrypticFirstRouteCompleted);
                        reengrammitizationReceipt = await reengrammitizationGate
                            .ReengrammitizeAsync(reengrammitizationRequest, cancellationToken)
                            .ConfigureAwait(false);

                        await journal.AppendAsync(
                            new GovernanceJournalEntry(
                                loopKey,
                                GovernanceJournalEntryKind.ActReceipt,
                                GovernanceLoopStage.CrypticFirstRouteCompleted,
                                reengrammitizationReceipt.Timestamp,
                                decisionReceipt,
                                DeferredReview: null,
                                new GovernanceActReceipt(
                                    loopKey,
                                    reengrammitizationRequest.IdempotencyKey,
                                    GovernanceActKind.Reengrammitization,
                                    GovernanceLoopStage.CrypticFirstRouteCompleted,
                                    Succeeded: true,
                                    FailureCode: null,
                                    reengrammitizationReceipt.Timestamp,
                                    PublishedLanes: GovernedPrimeDerivativeLane.Neither,
                                    ReceiptPointer: reengrammitizationReceipt.ReceiptPointer,
                                    RequestFingerprint: ComputeFingerprint(reengrammitizationRequest),
                                    TargetClass: collapseRoutingDecision.TargetClass,
                                    ResidueClass: collapseRoutingDecision.ResidueClass,
                                    ClassificationConfidence: collapseRoutingDecision.ClassificationConfidence,
                                    EvidenceFlags: collapseRoutingDecision.EvidenceFlags,
                                    ReviewTriggers: collapseRoutingDecision.ReviewTriggers,
                                    SourceSubsystem: collapseRoutingDecision.SourceSubsystem,
                                    MutationReceipt: CreateActMutationReceipt(
                                        reviewRequest,
                                        decisionReceipt,
                                        GovernanceActKind.Reengrammitization,
                                        succeeded: true,
                                        failureCode: null,
                                        timestamp: reengrammitizationReceipt.Timestamp)),
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
                                    RequestFingerprint: ComputeFingerprint(reengrammitizationRequest),
                                    TargetClass: collapseRoutingDecision.TargetClass,
                                    ResidueClass: collapseRoutingDecision.ResidueClass,
                                    ClassificationConfidence: collapseRoutingDecision.ClassificationConfidence,
                                    EvidenceFlags: collapseRoutingDecision.EvidenceFlags,
                                    ReviewTriggers: collapseRoutingDecision.ReviewTriggers,
                                    SourceSubsystem: collapseRoutingDecision.SourceSubsystem,
                                    MutationReceipt: CreateActMutationReceipt(
                                        reviewRequest,
                                        decisionReceipt,
                                        GovernanceActKind.Reengrammitization,
                                        succeeded: false,
                                        failureCode: $"reengrammitization-failed:{ex.GetType().Name}",
                                        timestamp: DateTime.UtcNow)),
                                ReviewRequest: reviewRequest,
                                Annotation: null),
                            cancellationToken).ConfigureAwait(false);

                        return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    stage = GovernanceLoopStage.CrypticFirstRouteCompleted;
                }
            }
            else
            {
                if (!snapshot.FirstRouteCompleted || snapshot.FirstRouteDisposition != CmeCollapseDisposition.RouteToCGoA)
                {
                    var cgoaResult = await TryAppendCrypticHoldAsync(
                            loopKey,
                            reviewRequest,
                            decisionReceipt,
                            collapseRoutingDecision,
                            GovernanceLoopStage.CrypticFirstRouteCompleted,
                            GovernanceLoopStage.PendingRecovery,
                            snapshot.PublishedLanes,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (!cgoaResult.Succeeded)
                    {
                        return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
                    }
                }

                stage = GovernanceLoopStage.CrypticFirstRouteCompleted;
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
                                RequestFingerprint: ComputeFingerprint(laneRequest),
                                MutationReceipt: CreateActMutationReceipt(
                                    reviewRequest,
                                    decisionReceipt,
                                    lane == GovernedPrimeDerivativeLane.Pointer
                                        ? GovernanceActKind.PrimePointerPublication
                                        : GovernanceActKind.PrimeCheckedViewPublication,
                                    succeeded: true,
                                    failureCode: null,
                                    timestamp: DateTime.UtcNow)),
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
                                RequestFingerprint: ComputeFingerprint(laneRequest),
                                MutationReceipt: CreateActMutationReceipt(
                                    reviewRequest,
                                    decisionReceipt,
                                    lane == GovernedPrimeDerivativeLane.Pointer
                                        ? GovernanceActKind.PrimePointerPublication
                                        : GovernanceActKind.PrimeCheckedViewPublication,
                                    succeeded: false,
                                    failureCode: $"publication-failed:{lane}:{ex.GetType().Name}",
                                    timestamp: DateTime.UtcNow)),
                            ReviewRequest: reviewRequest,
                            Annotation: null),
                        cancellationToken).ConfigureAwait(false);

                    return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
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
                        RequestFingerprint: null,
                        MutationReceipt: CreateActMutationReceipt(
                            reviewRequest,
                            decisionReceipt,
                            GovernanceActKind.Completion,
                            succeeded: true,
                            failureCode: null,
                            timestamp: DateTime.UtcNow)),
                    ReviewRequest: reviewRequest,
                    Annotation: null),
                cancellationToken).ConfigureAwait(false);

            return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
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

        private ICrypticCustodyStore RequireCrypticCustodyStore() =>
            _stores.CrypticCustodyStore
            ?? throw new InvalidOperationException("Golden Path requires a Cryptic custody store.");

        private IGovernedPrimePublicationSink RequireGovernedPrimePublicationSink() =>
            _stores.GovernedPrimePublicationSink
            ?? throw new InvalidOperationException("Golden Path requires a governed Prime publication sink.");

        private IGovernanceReceiptJournal RequireGovernanceReceiptJournal() =>
            _stores.GovernanceReceiptJournal
            ?? throw new InvalidOperationException("Golden Path hardening requires a governance receipt journal.");

        private ICmeCollapseQualifier RequireCmeCollapseQualifier() =>
            _stores.CmeCollapseQualifier
            ?? throw new InvalidOperationException("Golden Path qualification requires a CME collapse qualifier.");

        private CradleTek.Host.Interfaces.IHopngArtifactService RequireHopngArtifactService() =>
            _stores.HopngArtifactService;

        private static ReturnCandidateReviewRequest CreateReviewRequest(GovernanceCycleWorkResult workResult)
        {
            var requestEnvelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
                targetSurface: ControlSurfaceKind.StewardReturnReview,
                requestedBy: "CradleTek",
                scopeHandle: workResult.SessionHandle,
                protectionClass: "cryptic-review",
                witnessRequirement: "governance-witness",
                actionableContent: workResult.ActionableContent,
                parentEnvelopeId: workResult.ReturnIntakeEnvelopeId);

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
                SubmittedBy: "CradleTek",
                CandidatePayload: workResult.CandidatePayload,
                CollapseClassification: workResult.CollapseClassification,
                RequestEnvelope: requestEnvelope);
        }

        private async Task WitnessCompassObservationAsync(
            string loopKey,
            GovernanceCycleWorkResult workResult,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            if (workResult.CompassObservation is null)
            {
                return;
            }

            var bridge = new GovernedCompassObservationBridge(
                _stores.GovernanceTelemetry,
                RequireGovernanceReceiptJournal());
            var receipt = await bridge.WitnessAsync(
                    loopKey,
                    workResult.CompassObservation,
                    GovernanceLoopStage.BoundedCognitionCompleted,
                    reviewRequest,
                    cancellationToken)
                .ConfigureAwait(false);

            await WitnessCompassDriftAsync(loopKey, receipt.Stage, reviewRequest, cancellationToken).ConfigureAwait(false);
            await WitnessInnerWeatherAsync(loopKey, receipt.Stage, reviewRequest, cancellationToken).ConfigureAwait(false);
            await WitnessWeatherDisclosureAsync(loopKey, receipt.Stage, reviewRequest, cancellationToken).ConfigureAwait(false);
            await WitnessOfficeAuthorityAsync(loopKey, receipt.Stage, reviewRequest, cancellationToken).ConfigureAwait(false);
        }

        private async Task WitnessCompassDriftAsync(
            string loopKey,
            GovernanceLoopStage stage,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var entries = await journal.ReplayAsync(cancellationToken).ConfigureAwait(false);
            var assessment = CompassDriftProjector.ProjectForLoop(loopKey, entries);
            if (assessment is null)
            {
                return;
            }

            var bridge = new GovernedCompassDriftBridge(_stores.GovernanceTelemetry, journal);
            await bridge.WitnessAsync(loopKey, assessment, stage, reviewRequest, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task WitnessInnerWeatherAsync(
            string loopKey,
            GovernanceLoopStage stage,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var batch = await journal.ReplayBatchAsync(cancellationToken).ConfigureAwait(false);
            var evidence = InnerWeatherProjector.ProjectForLoop(loopKey, batch);
            if (evidence is null)
            {
                return;
            }

            var bridge = new GovernedInnerWeatherBridge(_stores.GovernanceTelemetry, journal);
            await bridge.WitnessAsync(loopKey, evidence, stage, reviewRequest, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task WitnessWeatherDisclosureAsync(
            string loopKey,
            GovernanceLoopStage stage,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var batch = await journal.ReplayBatchAsync(cancellationToken).ConfigureAwait(false);
            var assessment = StewardCareRouter.AssessForLoop(loopKey, batch);
            if (assessment is null)
            {
                return;
            }

            var decision = InnerWeatherDisclosureReducer.Reduce(assessment);
            var bridge = new GovernedWeatherDisclosureBridge(_stores.GovernanceTelemetry, journal);
            await bridge.WitnessAsync(loopKey, assessment, decision, stage, reviewRequest, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task WitnessOfficeAuthorityAsync(
            string loopKey,
            GovernanceLoopStage stage,
            ReturnCandidateReviewRequest reviewRequest,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var batch = await journal.ReplayBatchAsync(cancellationToken).ConfigureAwait(false);
            var assessments = GoverningOfficeAuthorityResolver.AssessForLoop(loopKey, batch);
            if (assessments.Count == 0)
            {
                return;
            }

            var bridge = new GovernedOfficeAuthorityBridge(_stores.GovernanceTelemetry, journal);
            foreach (var assessment in assessments)
            {
                var view = GoverningOfficeAuthorityViewReducer.Reduce(assessment);
                await bridge.WitnessAsync(loopKey, assessment, view, stage, reviewRequest, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task<GovernanceGoldenPathResult> BuildResultAsync(
            Guid candidateId,
            string loopKey,
            CancellationToken cancellationToken)
        {
            var batch = await RequireGovernanceReceiptJournal()
                .ReplayLoopBatchAsync(loopKey, cancellationToken)
                .ConfigureAwait(false);
            var snapshot = GovernanceLoopStateModel.Project(batch, loopKey);
            snapshot = await EnsureTerminalHopngArtifactsAsync(loopKey, snapshot, cancellationToken).ConfigureAwait(false);
            return BuildResult(candidateId, loopKey, snapshot);
        }

        private GovernanceGoldenPathResult BuildResult(
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
                FailureCode: snapshot.FailureCode,
                CollapseRoutingDecision: BuildCollapseRoutingDecision(snapshot),
                HopngArtifacts: snapshot.HopngArtifacts,
                TargetWitnessReceipts: snapshot.TargetWitnessReceipts,
                CompassObservationReceipts: snapshot.CompassObservationReceipts ?? [],
                CompassDriftReceipts: snapshot.CompassDriftReceipts ?? [],
                InnerWeatherReceipts: snapshot.InnerWeatherReceipts ?? [],
                CommunityWeatherPacket: snapshot.CommunityWeatherPacket,
                WeatherDisclosureReceipts: snapshot.WeatherDisclosureReceipts ?? [],
                OfficeAuthorityReceipts: snapshot.OfficeAuthorityReceipts ?? []);
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
                snapshot.LatestCollapseQualification,
                snapshot.FailureCode,
                snapshot.FailureStage,
                ResumeEligible: snapshot.DecisionReceipt?.Decision == GovernanceDecision.Approved &&
                                controlState is GovernanceLoopControlState.InProgress or GovernanceLoopControlState.PendingRecovery,
                HasJournalIntegrityErrors: snapshot.JournalIntegrityErrorCount > 0,
                JournalIntegrityErrorCount: snapshot.JournalIntegrityErrorCount,
                HopngArtifacts: snapshot.HopngArtifacts,
                TargetWitnessReceipts: snapshot.TargetWitnessReceipts,
                CompassObservationReceipts: snapshot.CompassObservationReceipts ?? [],
                CompassDriftReceipts: snapshot.CompassDriftReceipts ?? [],
                InnerWeatherReceipts: snapshot.InnerWeatherReceipts ?? [],
                CommunityWeatherPacket: snapshot.CommunityWeatherPacket,
                WeatherDisclosureReceipts: snapshot.WeatherDisclosureReceipts ?? [],
                OfficeAuthorityReceipts: snapshot.OfficeAuthorityReceipts ?? []);
        }

        private async Task<GovernanceLoopStateSnapshot> EnsureTerminalHopngArtifactsAsync(
            string loopKey,
            GovernanceLoopStateSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            if (snapshot.Stage is not (GovernanceLoopStage.LoopCompleted or GovernanceLoopStage.PendingRecovery))
            {
                return snapshot;
            }

            if (snapshot.DecisionReceipt is null)
            {
                return snapshot;
            }

            var existingProfiles = snapshot.HopngArtifacts
                .Select(receipt => receipt.Profile)
                .ToHashSet();
            var missingProfiles = new[]
            {
                GovernedHopngArtifactProfile.GoverningTrafficEvidence,
                GovernedHopngArtifactProfile.GovernanceTelemetryPhaseStack
            }.Where(profile => !existingProfiles.Contains(profile)).ToArray();
            if (missingProfiles.Length == 0)
            {
                return snapshot;
            }

            var journal = RequireGovernanceReceiptJournal();
            var loopEntries = await journal.ReplayLoopAsync(loopKey, cancellationToken).ConfigureAwait(false);
            var collapseRoutingDecision = BuildCollapseRoutingDecision(snapshot);
            foreach (var profile in missingProfiles)
            {
                var receipt = await RequireHopngArtifactService()
                    .EmitAsync(
                        new GovernedHopngEmissionRequest(
                            LoopKey: loopKey,
                            CandidateId: snapshot.DecisionReceipt.CandidateId,
                            CandidateProvenance: snapshot.DecisionReceipt.CandidateProvenance,
                            Profile: profile,
                            Stage: snapshot.Stage,
                            RequestedBy: "CradleTek",
                            DecisionReceipt: snapshot.DecisionReceipt,
                            Snapshot: snapshot,
                            JournalEntries: loopEntries,
                            CollapseRoutingDecision: collapseRoutingDecision),
                        cancellationToken)
                    .ConfigureAwait(false);

                await journal.AppendAsync(
                    new GovernanceJournalEntry(
                        loopKey,
                        GovernanceJournalEntryKind.ArtifactReceipt,
                        snapshot.Stage,
                        receipt.TimestampUtc.UtcDateTime,
                        snapshot.DecisionReceipt,
                        DeferredReview: null,
                        ActReceipt: null,
                        ReviewRequest: snapshot.ReviewRequest,
                        Annotation: null,
                        HopngArtifactReceipt: receipt),
                    cancellationToken).ConfigureAwait(false);
            }

            var refreshedBatch = await journal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
            return GovernanceLoopStateModel.Project(refreshedBatch, loopKey);
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

        private static GovernedControlSurfaceMutationReceipt CreateActMutationReceipt(
            ReturnCandidateReviewRequest reviewRequest,
            GovernanceDecisionReceipt decisionReceipt,
            GovernanceActKind actKind,
            bool succeeded,
            string? failureCode,
            DateTime timestamp)
        {
            return ControlSurfaceContractGuards.CreateMutationReceipt(
                envelopeId: reviewRequest.RequestEnvelope.EnvelopeId,
                contentHandle: reviewRequest.RequestEnvelope.ActionableContent.ContentHandle,
                targetSurface: ControlSurfaceKind.GovernanceAct,
                outcome: succeeded ? ControlMutationOutcome.Authorized : ControlMutationOutcome.Refused,
                governedBy: decisionReceipt.AdjudicatorIdentity,
                decisionCode: succeeded ? $"act:{actKind}" : failureCode ?? $"act:{actKind}:failed",
                timestampUtc: new DateTimeOffset(timestamp, TimeSpan.Zero));
        }

        private CmeCollapseRoutingDecision? BuildCollapseRoutingDecision(GovernanceLoopStateSnapshot snapshot)
        {
            if (snapshot.DecisionReceipt is null)
            {
                return null;
            }

            if (snapshot.LatestCollapseQualification is not null)
            {
                return new CmeCollapseRoutingDecision(
                    snapshot.LatestCollapseQualification.Destination == "cMoS"
                        ? CmeCollapseDisposition.RouteToCMoS
                        : CmeCollapseDisposition.RouteToCGoA,
                    snapshot.LatestCollapseQualification.ResidueClass,
                    snapshot.LatestCollapseQualification.ReviewState,
                    snapshot.DecisionReceipt.RationaleCode,
                    snapshot.DecisionReceipt.AdjudicatorIdentity,
                    snapshot.DecisionReceipt.Timestamp,
                    snapshot.LatestCollapseQualification.Destination,
                    snapshot.LatestCollapseQualification.ClassificationConfidence,
                    snapshot.LatestCollapseQualification.EvidenceFlags,
                    snapshot.LatestCollapseQualification.ReviewTriggers,
                    snapshot.LatestCollapseQualification.SourceSubsystem);
            }

            var reviewRequest = snapshot.ReviewRequest
                ?? throw new InvalidOperationException("Loop snapshot is missing a review request.");
            var qualification = RequireCmeCollapseQualifier().Qualify(reviewRequest.CollapseClassification);
            return GovernanceLoopStateModel.BuildCollapseRoutingDecision(snapshot.DecisionReceipt, qualification);
        }

        private async Task<GovernanceGoldenPathResult?> EnsureDeferredHoldAsync(
            string loopKey,
            ReturnCandidateReviewRequest reviewRequest,
            GovernanceDecisionReceipt decisionReceipt,
            CmeCollapseRoutingDecision collapseRoutingDecision,
            CancellationToken cancellationToken)
        {
            var holdResult = await TryAppendCrypticHoldAsync(
                    loopKey,
                    reviewRequest,
                    decisionReceipt,
                    collapseRoutingDecision,
                    GovernanceLoopStage.GovernanceDecisionDeferred,
                    GovernanceLoopStage.PendingRecovery,
                    GovernedPrimeDerivativeLane.Neither,
                    cancellationToken)
                .ConfigureAwait(false);
            if (holdResult.Succeeded)
            {
                return null;
            }

            return await BuildResultAsync(reviewRequest.CandidateId, loopKey, cancellationToken).ConfigureAwait(false);
        }

        private async Task<(bool Succeeded, string? FailureCode)> TryAppendCrypticHoldAsync(
            string loopKey,
            ReturnCandidateReviewRequest reviewRequest,
            GovernanceDecisionReceipt decisionReceipt,
            CmeCollapseRoutingDecision collapseRoutingDecision,
            GovernanceLoopStage successStage,
            GovernanceLoopStage failureStage,
            GovernedPrimeDerivativeLane publishedLanes,
            CancellationToken cancellationToken)
        {
            var journal = RequireGovernanceReceiptJournal();
            var custodyStore = RequireCrypticCustodyStore();
            var appendRequest = new CrypticCustodyAppendRequest(
                reviewRequest.IdentityId,
                CustodyDomain: collapseRoutingDecision.TargetClass,
                PayloadPointer: reviewRequest.ReturnCandidatePointer,
                Classification: collapseRoutingDecision.ResidueClass == CmeCollapseResidueClass.AutobiographicalProtected
                    ? "collapse-protected-autobiographical-residue"
                    : "collapse-protected-contextual-residue");

            try
            {
                var record = await custodyStore.AppendAsync(appendRequest, cancellationToken).ConfigureAwait(false);
                await journal.AppendAsync(
                    new GovernanceJournalEntry(
                        loopKey,
                        GovernanceJournalEntryKind.ActReceipt,
                        successStage,
                        record.Timestamp,
                        decisionReceipt,
                        DeferredReview: null,
                        new GovernanceActReceipt(
                            loopKey,
                            decisionReceipt.IdempotencyKey,
                            collapseRoutingDecision.Disposition == CmeCollapseDisposition.RouteToCMoS
                                ? GovernanceActKind.CollapseHoldToCMoS
                                : GovernanceActKind.CollapseHoldToCGoA,
                            successStage,
                            Succeeded: true,
                            FailureCode: null,
                            record.Timestamp,
                            publishedLanes,
                            ReceiptPointer: record.Pointer,
                            RequestFingerprint: ComputeFingerprint(appendRequest),
                            TargetClass: collapseRoutingDecision.TargetClass,
                            ResidueClass: collapseRoutingDecision.ResidueClass,
                            ClassificationConfidence: collapseRoutingDecision.ClassificationConfidence,
                            EvidenceFlags: collapseRoutingDecision.EvidenceFlags,
                            ReviewTriggers: collapseRoutingDecision.ReviewTriggers,
                            SourceSubsystem: collapseRoutingDecision.SourceSubsystem,
                            MutationReceipt: CreateActMutationReceipt(
                                reviewRequest,
                                decisionReceipt,
                                collapseRoutingDecision.Disposition == CmeCollapseDisposition.RouteToCMoS
                                    ? GovernanceActKind.CollapseHoldToCMoS
                                    : GovernanceActKind.CollapseHoldToCGoA,
                                succeeded: true,
                                failureCode: null,
                                timestamp: record.Timestamp)),
                        ReviewRequest: reviewRequest,
                        Annotation: null),
                    cancellationToken).ConfigureAwait(false);
                return (true, null);
            }
            catch (Exception ex)
            {
                await journal.AppendAsync(
                    new GovernanceJournalEntry(
                        loopKey,
                        GovernanceJournalEntryKind.ActReceipt,
                        failureStage,
                        DateTime.UtcNow,
                        decisionReceipt,
                        DeferredReview: null,
                        new GovernanceActReceipt(
                            loopKey,
                            decisionReceipt.IdempotencyKey,
                            collapseRoutingDecision.Disposition == CmeCollapseDisposition.RouteToCMoS
                                ? GovernanceActKind.CollapseHoldToCMoS
                                : GovernanceActKind.CollapseHoldToCGoA,
                            failureStage,
                            Succeeded: false,
                            FailureCode: $"collapse-hold-failed:{collapseRoutingDecision.TargetClass}:{ex.GetType().Name}",
                            DateTime.UtcNow,
                            publishedLanes,
                            ReceiptPointer: null,
                            RequestFingerprint: ComputeFingerprint(appendRequest),
                            TargetClass: collapseRoutingDecision.TargetClass,
                            ResidueClass: collapseRoutingDecision.ResidueClass,
                            ClassificationConfidence: collapseRoutingDecision.ClassificationConfidence,
                            EvidenceFlags: collapseRoutingDecision.EvidenceFlags,
                            ReviewTriggers: collapseRoutingDecision.ReviewTriggers,
                            SourceSubsystem: collapseRoutingDecision.SourceSubsystem,
                            MutationReceipt: CreateActMutationReceipt(
                                reviewRequest,
                                decisionReceipt,
                                collapseRoutingDecision.Disposition == CmeCollapseDisposition.RouteToCMoS
                                    ? GovernanceActKind.CollapseHoldToCMoS
                                    : GovernanceActKind.CollapseHoldToCGoA,
                                succeeded: false,
                                failureCode: $"collapse-hold-failed:{collapseRoutingDecision.TargetClass}:{ex.GetType().Name}",
                                timestamp: DateTime.UtcNow)),
                        ReviewRequest: reviewRequest,
                        Annotation: null),
                    cancellationToken).ConfigureAwait(false);
                return (false, $"collapse-hold-failed:{collapseRoutingDecision.TargetClass}:{ex.GetType().Name}");
            }
        }
    }
}
