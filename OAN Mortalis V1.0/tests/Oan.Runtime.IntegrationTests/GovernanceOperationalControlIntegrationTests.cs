using System.Net;
using System.Text;
using CradleTek.Cryptic;
using CradleTek.Host.Interfaces;
using CradleTek.Mantle;
using CradleTek.Public;
using EngramGovernance.Services;
using OAN.Core.Telemetry;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using SoulFrame.Host;
using Telemetry.GEL;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernanceOperationalControlIntegrationTests
{
    [Fact]
    public async Task StatusReader_CompletedLoop_ReturnsLaneAwareStatus()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var steward = CreateSteward(publicLayer, crypticLayer, telemetry, journal);
        var request = CreateGoldenPathRequest();
        var workResult = CreateApprovedWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId);

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(request.IdentityId, "cMoS", "cmos://seed/source", "seed"));

        var manager = new StackManager(CreateStoreRegistry(
            publicLayer,
            mantle,
            governedPrimePublicationSink: publicLayer,
            governanceReceiptJournal: journal,
            governanceCognitionService: new FakeGovernanceCognitionService(workResult),
            steward));

        await manager.RunGovernanceGoldenPathAsync(request);
        var status = await manager.GetStatusByCandidateAsync(workResult.CandidateId, workResult.ProvenanceMarker);

        Assert.Equal(GovernanceLoopControlState.Completed, status.ControlState);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, status.Stage);
        Assert.True(status.ReengrammitizationCompleted);
        Assert.True(status.Publication.PointerPublished);
        Assert.True(status.Publication.CheckedViewPublished);
        Assert.NotNull(status.LatestCollapseQualification);
        Assert.Equal("cMoS", status.LatestCollapseQualification!.Destination);
        Assert.True(status.LatestCollapseQualification.EvidenceFlags.HasFlag(CmeCollapseEvidenceFlag.AutobiographicalSignal));
        Assert.False(status.ResumeEligible);
    }

    [Fact]
    public async Task DeferredWorkflow_ApproveDeferred_ThenResume_CompletesLoop()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var steward = CreateSteward(publicLayer, crypticLayer, telemetry, journal);
        var request = CreateGoldenPathRequest();
        var workResult = CreateDeferredWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId);

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(request.IdentityId, "cMoS", "cmos://seed/source", "seed"));

        var manager = new StackManager(CreateStoreRegistry(
            publicLayer,
            mantle,
            governedPrimePublicationSink: publicLayer,
            governanceReceiptJournal: journal,
            governanceCognitionService: new FakeGovernanceCognitionService(workResult),
            steward));

        var deferredResult = await manager.RunGovernanceGoldenPathAsync(request);
        var deferredItems = await steward.ListDeferredAsync();
        Assert.Single(deferredItems);
        Assert.Equal(GovernanceDecision.Deferred, deferredResult.DecisionReceipt.Decision);

        var reviewRequest = new ReviewDeferredCandidateRequest(
            deferredResult.LoopKey,
            workResult.CandidateId,
            workResult.ProvenanceMarker,
            "operator:steward-review",
            "steward.approved.manual-review",
            "approved after manual review");

        await steward.AnnotateDeferredAsync(reviewRequest);
        var adjudication = await steward.ApproveDeferredAsync(reviewRequest);
        Assert.Equal(GovernanceDecision.Approved, adjudication.Receipt.Decision);

        var resumed = await manager.ResumeGovernanceLoopAsync(new ResumeGovernanceLoopRequest(
            deferredResult.LoopKey,
            RequestedBy: "operator:steward-review",
            Reason: "manual approval of deferred candidate"));

        var resolvedDeferred = await steward.ListDeferredAsync();
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();

        Assert.Empty(resolvedDeferred);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, resumed.Stage);
        Assert.Equal(GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView, resumed.PublishedLanes);
        Assert.Equal(2, derivativeViews.Count);
    }

    [Fact]
    public async Task PendingRecovery_RetryPublicationLane_RetriesOnlyRequestedLane()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry, journal);
        var request = CreateGoldenPathRequest();
        var workResult = CreateApprovedWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId);
        var failingSink = new FailOncePrimePublicationSink(publicLayer, GovernedPrimeDerivativeLane.CheckedView);

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(request.IdentityId, "cMoS", "cmos://seed/source", "seed"));

        var manager = new StackManager(CreateStoreRegistry(
            publicLayer,
            mantle,
            governedPrimePublicationSink: failingSink,
            governanceReceiptJournal: journal,
            governanceCognitionService: new FakeGovernanceCognitionService(workResult),
            steward));

        var first = await manager.RunGovernanceGoldenPathAsync(request);
        var pending = await manager.ListPendingRecoveryAsync();
        var resumed = await manager.RetryPublicationLaneAsync(new ResumePublicationLaneRequest(
            first.LoopKey,
            GovernedPrimeDerivativeLane.CheckedView,
            RequestedBy: "operator:recovery",
            Reason: "retry missing checked-view lane"));

        Assert.Equal(GovernanceLoopStage.PendingRecovery, first.Stage);
        Assert.Single(pending);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, resumed.Stage);
        Assert.Equal(GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView, resumed.PublishedLanes);
    }

    [Fact]
    public async Task StartGovernanceGoldenPath_DuplicateLoop_ReturnsInProgressStatus()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry, journal);
        var request = CreateGoldenPathRequest();
        var workResult = CreateApprovedWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId);
        var slowGate = new SlowReengrammitizationGate(mantle);

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(request.IdentityId, "cMoS", "cmos://seed/source", "seed"));

        var manager = new StackManager(CreateStoreRegistry(
            publicLayer,
            mantle,
            governedPrimePublicationSink: publicLayer,
            governanceReceiptJournal: journal,
            governanceCognitionService: new FakeGovernanceCognitionService(workResult),
            steward,
            reengrammitizationGate: slowGate));

        var firstTask = manager.StartGovernanceGoldenPathAsync(request);
        await slowGate.WaitUntilEnteredAsync();

        var second = await manager.StartGovernanceGoldenPathAsync(request);
        slowGate.Release();
        var first = await firstTask;

        Assert.Equal(GovernanceLoopControlState.InProgress, second.ControlState);
        Assert.Equal(GovernanceLoopControlState.Completed, first.ControlState);
        Assert.NotNull(first.Result);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, first.Result!.Stage);
    }

    [Fact]
    public async Task StatusReader_MalformedJournalLine_FailsSafe()
    {
        var journalPath = CreateJournalPath();
        var loopKey = GovernanceLoopKeys.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "membrane-derived:broken");
        await File.WriteAllTextAsync(journalPath, $"{{\"loopKey\":\"{loopKey}\"{Environment.NewLine}");

        var manager = new StackManager(CreateStoreRegistry(
            new PublicLayerService(),
            new MantleOfSovereigntyService(),
            governedPrimePublicationSink: new PublicLayerService(),
            governanceReceiptJournal: new NdjsonGovernanceReceiptJournal(journalPath),
            governanceCognitionService: new FakeGovernanceCognitionService(CreateApprovedWorkResult(Guid.NewGuid(), Guid.NewGuid(), "cme-runtime")),
            steward: CreateSteward(new PublicLayerService(), new CrypticLayerService(), new GelTelemetryAdapter(), new NdjsonGovernanceReceiptJournal(journalPath))));

        var status = await manager.GetStatusByLoopKeyAsync(loopKey);

        Assert.Equal(GovernanceLoopControlState.Failed, status.ControlState);
        Assert.True(status.HasJournalIntegrityErrors);
        Assert.Equal(1, status.JournalIntegrityErrorCount);
    }

    private static GovernanceCycleStartRequest CreateGoldenPathRequest()
    {
        return new GovernanceCycleStartRequest(
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-runtime",
            SourceCustodyDomain: "cmos",
            SourceTheater: "prime",
            RequestedTheater: "prime",
            PolicyHandle: "agenticore.cognition.cycle",
            OperatorInput: "solve governed task");
    }

    private static GovernanceCycleWorkResult CreateApprovedWorkResult(Guid identityId, Guid soulFrameId, string cmeId)
    {
        return new GovernanceCycleWorkResult(
            CandidateId: new Guid("11111111-1111-1111-1111-111111111111"),
            IdentityId: identityId,
            SoulFrameId: soulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: cmeId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/approved",
            WorkingStateHandle: "soulframe-working://cme-runtime/approved",
            ProvenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            ReturnCandidatePointer: "agenticore-return://candidate/approved",
            IntakeIntent: "candidate-return-evaluation",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: CreateCollapseClassification(
                autobiographicalRelevant: true,
                selfGelIdentified: true),
            ResultType: "cognition-accepted",
            EngramCommitRequired: true);
    }

    private static GovernanceCycleWorkResult CreateDeferredWorkResult(Guid identityId, Guid soulFrameId, string cmeId)
    {
        return new GovernanceCycleWorkResult(
            CandidateId: new Guid("22222222-2222-2222-2222-222222222222"),
            IdentityId: identityId,
            SoulFrameId: soulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: cmeId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/deferred",
            WorkingStateHandle: "soulframe-working://cme-runtime/deferred",
            ProvenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            ReturnCandidatePointer: "agenticore-return://candidate/deferred",
            IntakeIntent: "defer-review",
            CandidatePayload: "{\"decision\":\"review\"}",
            CollapseClassification: CreateCollapseClassification(
                autobiographicalRelevant: true,
                selfGelIdentified: true,
                collapseConfidence: 0.61),
            ResultType: "cognition-review",
            EngramCommitRequired: true);
    }

    private static CmeCollapseClassification CreateCollapseClassification(
        bool autobiographicalRelevant,
        bool selfGelIdentified,
        double collapseConfidence = 0.92) =>
        new(
            collapseConfidence,
            selfGelIdentified,
            autobiographicalRelevant,
            autobiographicalRelevant || selfGelIdentified
                ? CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal
                : CmeCollapseEvidenceFlag.ContextualSignal | CmeCollapseEvidenceFlag.ProceduralSignal,
            CmeCollapseReviewTrigger.None,
            "AgentiCore");

    private static StoreRegistry CreateStoreRegistry(
        PublicLayerService publicLayer,
        MantleOfSovereigntyService mantle,
        IGovernedPrimePublicationSink governedPrimePublicationSink,
        IGovernanceReceiptJournal governanceReceiptJournal,
        IGovernanceCycleCognitionService governanceCognitionService,
        StewardAgent steward,
        ICrypticReengrammitizationGate? reengrammitizationGate = null)
    {
        return new StoreRegistry(
            governanceTelemetry: new RecordingTelemetrySink(),
            storageTelemetry: new RecordingTelemetrySink(),
            publicStores: new NullPublicPlaneStores(),
            primeDerivativePublisher: publicLayer,
            primeDerivativeView: publicLayer,
            publicAvailable: true,
            crypticStores: new NullCrypticPlaneStores(),
            crypticAvailable: true,
            soulFrameMembrane: null,
            governanceCognitionService: governanceCognitionService,
            returnGovernanceAdjudicator: steward,
            crypticCustodyStore: mantle,
            crypticReengrammitizationGate: reengrammitizationGate ?? mantle,
            governedPrimePublicationSink: governedPrimePublicationSink,
            governanceReceiptJournal: governanceReceiptJournal,
            cmeCollapseQualifier: new CmeCollapseQualifier());
    }

    private static StewardAgent CreateSteward(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        IGovernanceReceiptJournal journal)
    {
        return new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            publicStore,
            crypticStore,
            telemetry,
            governanceJournal: journal);
    }

    private static string CreateJournalPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance.ndjson");

    private sealed class RecordingTelemetrySink : ITelemetrySink
    {
        public Task EmitAsync(object telemetryEvent) => Task.CompletedTask;
    }

    private sealed class NullPublicPlaneStores : IPublicPlaneStores
    {
        public Task AppendToGoAAsync(string engramHash, object payload) => Task.CompletedTask;

        public Task AppendToGELAsync(string engramHash, object payload) => Task.CompletedTask;
    }

    private sealed class NullCrypticPlaneStores : ICrypticPlaneStores
    {
        public Task AppendToCGoAAsync(string engramHash, object payload) => Task.CompletedTask;
    }

    private sealed class FakeGovernanceCognitionService : IGovernanceCycleCognitionService
    {
        private readonly GovernanceCycleWorkResult _workResult;

        public FakeGovernanceCognitionService(GovernanceCycleWorkResult workResult)
        {
            _workResult = workResult;
        }

        public Task<GovernanceCycleWorkResult> ExecuteGovernanceCycleAsync(
            GovernanceCycleStartRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workResult);
        }
    }

    private sealed class FailOncePrimePublicationSink : IGovernedPrimePublicationSink
    {
        private readonly IGovernedPrimePublicationSink _inner;
        private readonly GovernedPrimeDerivativeLane _laneToFail;
        private bool _hasFailed;

        public FailOncePrimePublicationSink(
            IGovernedPrimePublicationSink inner,
            GovernedPrimeDerivativeLane laneToFail)
        {
            _inner = inner;
            _laneToFail = laneToFail;
        }

        public Task<GovernedPrimeDerivativeLane> PublishApprovedOutcomeAsync(
            GovernedPrimePublicationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!_hasFailed && request.AuthorizedLanes == _laneToFail)
            {
                _hasFailed = true;
                throw new InvalidOperationException("forced-publication-failure");
            }

            return _inner.PublishApprovedOutcomeAsync(request, cancellationToken);
        }
    }

    private sealed class SlowReengrammitizationGate : ICrypticReengrammitizationGate
    {
        private readonly ICrypticReengrammitizationGate _inner;
        private readonly TaskCompletionSource<bool> _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public SlowReengrammitizationGate(ICrypticReengrammitizationGate inner)
        {
            _inner = inner;
        }

        public Task<bool> CanAdmitAsync(
            GovernedReengrammitizationRequest request,
            CancellationToken cancellationToken = default) =>
            _inner.CanAdmitAsync(request, cancellationToken);

        public async Task<CrypticReengrammitizationReceipt> ReengrammitizeAsync(
            GovernedReengrammitizationRequest request,
            CancellationToken cancellationToken = default)
        {
            _entered.TrySetResult(true);
            await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return await _inner.ReengrammitizeAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public Task WaitUntilEnteredAsync() => _entered.Task;

        public void Release() => _release.TrySetResult(true);
    }
}
