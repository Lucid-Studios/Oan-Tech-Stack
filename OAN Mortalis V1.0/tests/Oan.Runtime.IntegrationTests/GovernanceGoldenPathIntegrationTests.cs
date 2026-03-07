using System.Net;
using System.Text;
using AgentiCore.Models;
using AgentiCore.Services;
using AgentiCoreService = AgentiCore.Services.AgentiCore;
using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;
using CradleTek.Cryptic;
using CradleTek.Host.Interfaces;
using CradleTek.Mantle;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using CradleTek.Public;
using EngramGovernance.Services;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using SoulFrame.Host;
using Telemetry.GEL;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernanceGoldenPathIntegrationTests
{
    [Fact]
    public async Task GoldenPath_ApprovedCycle_Reengrammitizes_AndPublishesPointerAndCheckedView()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var steward = CreateSteward(publicLayer, crypticLayer, telemetry);
        var membrane = CreateSoulFrameHostClient();
        var cognition = CreateAgentiCore(publicLayer, crypticLayer, telemetry, membrane);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(
                identityId,
                CustodyDomain: "cMoS",
                PayloadPointer: "cmos://seed/source",
                Classification: "seed"));

        var stores = CreateStoreRegistry(publicLayer, mantle, publicLayer, journal, membrane, cognition, steward);
        var manager = new StackManager(stores);

        var result = await manager.RunGovernanceGoldenPathAsync(request);
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();
        var crypticRecords = await mantle.ReadGuardedAsync(
            new CrypticGuardedReadRequest(identityId, "golden-path.policy", "integration-assert"));
        var snapshot = GovernanceLoopStateModel.Project(
            result.LoopKey,
            await journal.ReplayLoopAsync(result.LoopKey));

        Assert.Equal(GovernanceDecision.Approved, result.DecisionReceipt.Decision);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, result.Stage);
        Assert.NotNull(result.ReengrammitizationReceipt);
        Assert.True(result.ReengrammitizationReceipt!.Accepted);
        Assert.Equal(
            GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            result.PublishedLanes);
        Assert.Contains(derivativeViews, view => view.RepresentationKind == "pointer");
        Assert.Contains(derivativeViews, view => view.RepresentationKind == "checked-view");
        Assert.True(crypticRecords.Count >= 2);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, snapshot.Stage);
    }

    [Fact]
    public async Task GoldenPath_RejectedCycle_DoesNotMutateCryptic_OrPublishPrime()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(identityId, "cMoS", "cmos://seed/source", "seed"));

        var cognition = new FakeGovernanceCognitionService(new GovernanceCycleWorkResult(
            CandidateId: Guid.NewGuid(),
            IdentityId: identityId,
            SoulFrameId: request.SoulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: request.CMEId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "invalid-session",
            WorkingStateHandle: "cmos://raw-state",
            ProvenanceMarker: "not-membrane-derived",
            ReturnCandidatePointer: "agenticore-return://candidate/rejected",
            IntakeIntent: "candidate-return-evaluation",
            CandidatePayload: "{\"decision\":\"accept\"}",
            ResultType: "cognition-accepted",
            EngramCommitRequired: true));

        var stores = CreateStoreRegistry(publicLayer, mantle, publicLayer, journal, soulFrameMembrane: null, cognition, steward);
        var manager = new StackManager(stores);

        var result = await manager.RunGovernanceGoldenPathAsync(request);
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();
        var crypticRecords = await mantle.ReadGuardedAsync(
            new CrypticGuardedReadRequest(identityId, "golden-path.policy", "integration-assert"));

        Assert.Equal(GovernanceDecision.Rejected, result.DecisionReceipt.Decision);
        Assert.Equal(GovernanceLoopStage.GovernanceDecisionRejected, result.Stage);
        Assert.Null(result.ReengrammitizationReceipt);
        Assert.Equal(GovernedPrimeDerivativeLane.Neither, result.PublishedLanes);
        Assert.Empty(derivativeViews);
        Assert.Single(crypticRecords);
    }

    [Fact]
    public async Task GoldenPath_DeferredCycle_PersistsReviewWithoutMutationOrPublication()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(identityId, "cMoS", "cmos://seed/source", "seed"));

        var cognition = new FakeGovernanceCognitionService(new GovernanceCycleWorkResult(
            CandidateId: Guid.NewGuid(),
            IdentityId: identityId,
            SoulFrameId: request.SoulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: request.CMEId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/deferred",
            WorkingStateHandle: "soulframe-working://cme-runtime/deferred",
            ProvenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            ReturnCandidatePointer: "agenticore-return://candidate/deferred",
            IntakeIntent: "defer-review",
            CandidatePayload: "{\"decision\":\"accept\"}",
            ResultType: "cognition-accepted",
            EngramCommitRequired: true));

        var stores = CreateStoreRegistry(publicLayer, mantle, publicLayer, journal, soulFrameMembrane: null, cognition, steward);
        var manager = new StackManager(stores);

        var result = await manager.RunGovernanceGoldenPathAsync(request);
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();
        var crypticRecords = await mantle.ReadGuardedAsync(
            new CrypticGuardedReadRequest(identityId, "golden-path.policy", "integration-assert"));
        var deferred = await steward.ListDeferredCandidatesAsync();

        Assert.Equal(GovernanceDecision.Deferred, result.DecisionReceipt.Decision);
        Assert.Equal(GovernanceLoopStage.GovernanceDecisionDeferred, result.Stage);
        Assert.Null(result.ReengrammitizationReceipt);
        Assert.Equal(GovernedPrimeDerivativeLane.Neither, result.PublishedLanes);
        Assert.Empty(derivativeViews);
        Assert.Single(crypticRecords);
        Assert.Single(deferred);
    }

    [Fact]
    public async Task GoldenPath_DuplicateApproval_ReusesRecordedOutcome_WithoutRepeatingActs()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var steward = CreateSteward(publicLayer, crypticLayer, telemetry);
        var membrane = CreateSoulFrameHostClient();
        var cognition = CreateAgentiCore(publicLayer, crypticLayer, telemetry, membrane);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(identityId, "cMoS", "cmos://seed/source", "seed"));

        var stores = CreateStoreRegistry(publicLayer, mantle, publicLayer, journal, membrane, cognition, steward);
        var manager = new StackManager(stores);

        var first = await manager.RunGovernanceGoldenPathAsync(request);
        var second = await manager.RunGovernanceGoldenPathAsync(request);
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();
        var crypticRecords = await mantle.ReadGuardedAsync(
            new CrypticGuardedReadRequest(identityId, "golden-path.policy", "integration-assert"));

        Assert.Equal(first.LoopKey, second.LoopKey);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, second.Stage);
        Assert.Equal(2, derivativeViews.Count);
        Assert.Equal(2, crypticRecords.Count);
    }

    [Fact]
    public async Task GoldenPath_ReengrammitizationFailure_EntersPendingRecovery_WithoutPrimePublication()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var failingGate = new FailOnceReengrammitizationGate(mantle);
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(identityId, "cMoS", "cmos://seed/source", "seed"));

        var cognition = new FakeGovernanceCognitionService(CreateApprovedWorkResult(identityId, request));
        var stores = CreateStoreRegistry(publicLayer, mantle, publicLayer, journal, soulFrameMembrane: null, cognition, steward, reengrammitizationGate: failingGate);
        var manager = new StackManager(stores);

        var result = await manager.RunGovernanceGoldenPathAsync(request);
        var derivativeViews = await publicLayer.ListDerivativeViewsAsync();
        var snapshot = GovernanceLoopStateModel.Project(result.LoopKey, await journal.ReplayLoopAsync(result.LoopKey));

        Assert.Equal(GovernanceLoopStage.PendingRecovery, result.Stage);
        Assert.StartsWith("reengrammitization-failed:", result.FailureCode, StringComparison.Ordinal);
        Assert.Empty(derivativeViews);
        Assert.Equal(GovernanceLoopStage.PendingRecovery, snapshot.Stage);
    }

    [Fact]
    public async Task GoldenPath_PublicationFailure_RetriesMissingLaneOnly()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var failingSink = new FailOncePrimePublicationSink(publicLayer, GovernedPrimeDerivativeLane.CheckedView);
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), telemetry);
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());

        await mantle.AppendAsync(new CrypticCustodyAppendRequest(identityId, "cMoS", "cmos://seed/source", "seed"));

        var cognition = new FakeGovernanceCognitionService(CreateApprovedWorkResult(identityId, request));
        var stores = CreateStoreRegistry(publicLayer, mantle, failingSink, journal, soulFrameMembrane: null, cognition, steward);
        var manager = new StackManager(stores);

        var first = await manager.RunGovernanceGoldenPathAsync(request);
        var firstViews = await publicLayer.ListDerivativeViewsAsync();
        var second = await manager.RunGovernanceGoldenPathAsync(request);
        var secondViews = await publicLayer.ListDerivativeViewsAsync();

        Assert.Equal(GovernanceLoopStage.PendingRecovery, first.Stage);
        Assert.Equal(GovernedPrimeDerivativeLane.Pointer, first.PublishedLanes);
        Assert.Single(firstViews);
        Assert.Equal(GovernanceLoopStage.LoopCompleted, second.Stage);
        Assert.Equal(
            GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            second.PublishedLanes);
        Assert.Equal(2, secondViews.Count);
    }

    private static GovernanceCycleStartRequest CreateGoldenPathRequest(Guid identityId)
    {
        return new GovernanceCycleStartRequest(
            IdentityId: identityId,
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-runtime",
            SourceCustodyDomain: "cmos",
            SourceTheater: "prime",
            RequestedTheater: "prime",
            PolicyHandle: "agenticore.cognition.cycle",
            OperatorInput: "solve governed task");
    }

    private static StoreRegistry CreateStoreRegistry(
        PublicLayerService publicLayer,
        MantleOfSovereigntyService mantle,
        IGovernedPrimePublicationSink governedPrimePublicationSink,
        IGovernanceReceiptJournal governanceReceiptJournal,
        SoulFrameHostClient? soulFrameMembrane,
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
            soulFrameMembrane: soulFrameMembrane,
            governanceCognitionService: governanceCognitionService,
            returnGovernanceAdjudicator: steward,
            crypticCustodyStore: mantle,
            crypticReengrammitizationGate: reengrammitizationGate ?? mantle,
            governedPrimePublicationSink: governedPrimePublicationSink,
            governanceReceiptJournal: governanceReceiptJournal);
    }

    private static GovernanceCycleWorkResult CreateApprovedWorkResult(Guid identityId, GovernanceCycleStartRequest request)
    {
        return new GovernanceCycleWorkResult(
            CandidateId: new Guid("11111111-1111-1111-1111-111111111111"),
            IdentityId: identityId,
            SoulFrameId: request.SoulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: request.CMEId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/approved",
            WorkingStateHandle: "soulframe-working://cme-runtime/approved",
            ProvenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            ReturnCandidatePointer: "agenticore-return://candidate/approved",
            IntakeIntent: "candidate-return-evaluation",
            CandidatePayload: "{\"decision\":\"accept\"}",
            ResultType: "cognition-accepted",
            EngramCommitRequired: true);
    }

    private static string CreateJournalPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance.ndjson");

    private static AgentiCoreService CreateAgentiCore(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        SoulFrameHostClient soulFrameHostClient)
    {
        return new AgentiCoreService(
            new StubCognitionEngine(),
            new StubEngramResolver(),
            new ContextAssembler(),
            CreateCommitService(publicStore, crypticStore, telemetry),
            publicStore,
            crypticStore,
            telemetry,
            new StubRootOntologicalCleaver(),
            new GEL.Runtime.SheafMasterEngramService(),
            new SLI.Ingestion.SliIngestionEngine(),
            soulFrameHostClient,
            new BoundedMembraneWorkerService(soulFrameHostClient));
    }

    private static EngramCommitService CreateCommitService(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        return new EngramCommitService(CreateSteward(publicStore, crypticStore, telemetry));
    }

    private static StewardAgent CreateSteward(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        return new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            publicStore,
            crypticStore,
            telemetry);
    }

    private static SoulFrameHostClient CreateSoulFrameHostClient()
    {
        var handler = new DelegatingHandlerStub((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/classify")
            {
                const string json = "{\"decision\":\"bounded-classify\",\"payload\":\"bounded-payload\",\"confidence\":0.74}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            if (request.RequestUri?.AbsolutePath == "/semantic_expand")
            {
                const string json = "{\"decision\":\"semantic-expand\",\"payload\":\"hint\",\"confidence\":0.61}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"decision\":\"ok\",\"payload\":\"{}\",\"confidence\":0.50}", Encoding.UTF8, "application/json")
            });
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:8181")
        };

        return new SoulFrameHostClient(httpClient, telemetry: null, "http://127.0.0.1:8181");
    }

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

    private sealed class FailOnceReengrammitizationGate : ICrypticReengrammitizationGate
    {
        private readonly ICrypticReengrammitizationGate _inner;
        private bool _hasFailed;

        public FailOnceReengrammitizationGate(ICrypticReengrammitizationGate inner)
        {
            _inner = inner;
        }

        public Task<bool> CanAdmitAsync(
            GovernedReengrammitizationRequest request,
            CancellationToken cancellationToken = default) =>
            _inner.CanAdmitAsync(request, cancellationToken);

        public Task<CrypticReengrammitizationReceipt> ReengrammitizeAsync(
            GovernedReengrammitizationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!_hasFailed)
            {
                _hasFailed = true;
                throw new InvalidOperationException("forced-failure");
            }

            return _inner.ReengrammitizeAsync(request, cancellationToken);
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

    private sealed class StubCognitionEngine : ICognitionEngine
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CognitionResult
            {
                Reasoning = "golden path reasoning",
                Decision = "accept",
                EngramCandidate = true,
                CleaveResidue = "residue",
                TraceId = "trace-001",
                SymbolicTrace = ["step-a", "step-b"],
                SliTokens = ["token-a"],
                DecisionBranch = "branch-a",
                CompassState = new CognitionCompassTelemetry
                {
                    IdForce = 0.5,
                    SuperegoConstraint = 0.2,
                    EgoStability = 0.7,
                    ValueElevation = CognitionValueElevation.Positive,
                    SymbolicDepth = 2,
                    BranchingFactor = 1,
                    DecisionEntropy = 0.1,
                    Timestamp = DateTime.UtcNow
                },
                Confidence = 0.81
            });
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubEngramResolver : IEngramResolver
    {
        public Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(Empty());

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default) =>
            Task.FromResult(Empty());

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Empty());

        private static EngramQueryResult Empty() =>
            new()
            {
                Source = "stub",
                Summaries = []
            };
    }

    private sealed class StubRootOntologicalCleaver : IRootOntologicalCleaver
    {
        public Task<OntologicalCleaverResult> CleaveAsync(string inputText, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OntologicalCleaverResult
            {
                InputText = inputText,
                Resolutions = [],
                Known = [],
                PartiallyKnown = [],
                Unknown = ["runtime", "golden", "path"],
                Metrics = new OntologicalCleaverMetrics
                {
                    KnownRatio = 0.0,
                    PartiallyKnownRatio = 0.0,
                    UnknownRatio = 1.0,
                    ConceptDensity = "sparse",
                    ContextStability = "stable"
                }
            });
        }
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responder(request, cancellationToken);
        }
    }
}
