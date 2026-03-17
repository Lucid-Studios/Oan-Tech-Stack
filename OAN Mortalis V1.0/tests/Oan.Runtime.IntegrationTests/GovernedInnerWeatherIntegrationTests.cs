using System.Text.Json.Nodes;
using CradleTek.Cryptic;
using CradleTek.Host.Interfaces;
using CradleTek.Mantle;
using CradleTek.Public;
using EngramGovernance.Services;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using Telemetry.GEL;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedInnerWeatherIntegrationTests
{
    [Fact]
    public async Task GoldenPath_ProjectsInnerWeatherIntoResultStatusJournalAndHopng()
    {
        var telemetry = new RecordingTelemetrySink();
        var storageTelemetry = new RecordingTelemetrySink();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(
                identityId,
                CustodyDomain: "cMoS",
                PayloadPointer: "cmos://seed/source",
                Classification: "seed"));

        var cognition = new FakeGovernanceCognitionSequenceService(
            CreateApprovedWorkResult(identityId, request, 1, CompassSeedAdvisoryDisposition.Accepted),
            CreateApprovedWorkResult(identityId, request, 2, CompassSeedAdvisoryDisposition.Deferred),
            CreateApprovedWorkResult(identityId, request, 3, CompassSeedAdvisoryDisposition.Rejected));
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), new GelTelemetryAdapter(), journal);
        var stores = CreateStoreRegistry(
            telemetry,
            storageTelemetry,
            publicLayer,
            mantle,
            publicLayer,
            journal,
            cognition,
            steward);
        var manager = new StackManager(stores);

        await manager.RunGovernanceGoldenPathAsync(request);
        await manager.RunGovernanceGoldenPathAsync(request);
        var third = await manager.RunGovernanceGoldenPathAsync(request);

        var status = await manager.GetStatusByLoopKeyAsync(third.LoopKey);
        var replay = await journal.ReplayLoopAsync(third.LoopKey);
        var innerWeatherEntry = Assert.Single(replay.Where(entry => entry.InnerWeatherReceipt is not null));
        var snapshot = GovernanceLoopStateModel.Project(third.LoopKey, replay);
        Assert.NotNull(third.CollapseRoutingDecision);
        var hopngRequest = new GovernedHopngEmissionRequest(
            LoopKey: third.LoopKey,
            CandidateId: third.CandidateId,
            CandidateProvenance: third.DecisionReceipt.CandidateProvenance,
            Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
            Stage: snapshot.Stage,
            RequestedBy: "CradleTek",
            DecisionReceipt: third.DecisionReceipt,
            Snapshot: snapshot,
            JournalEntries: replay,
            CollapseRoutingDecision: third.CollapseRoutingDecision!);
        var refs = GovernedHopngEvidenceReferences.Build(hopngRequest, snapshot);

        var resultReceipt = Assert.Single(third.InnerWeatherReceipts!);
        var statusReceipt = Assert.Single(status.InnerWeatherReceipts!);
        var packet = third.CommunityWeatherPacket;
        Assert.NotNull(packet);

        Assert.Equal(resultReceipt.InnerWeatherHandle, statusReceipt.InnerWeatherHandle);
        Assert.Equal(resultReceipt.InnerWeatherHandle, innerWeatherEntry.InnerWeatherReceipt!.InnerWeatherHandle);
        Assert.Equal(resultReceipt.InnerWeatherHandle, Assert.Single(snapshot.InnerWeatherReceipts!).InnerWeatherHandle);
        Assert.Equal(AttentionResidueState.Persistent, resultReceipt.ResidueState);
        Assert.Equal(WindowIntegrityState.Intact, resultReceipt.WindowIntegrityState);
        Assert.Equal(CommunityWeatherStatus.Unstable, packet!.Status);
        Assert.Equal(CommunityStewardAttentionState.Recommended, packet.StewardAttention);
        Assert.Equal(CommunityWeatherStatus.Unstable, status.CommunityWeatherPacket!.Status);
        Assert.Contains(refs, reference => reference.PointerUri == resultReceipt.InnerWeatherHandle);
        Assert.Contains(
            telemetry.Events.OfType<GovernedInnerWeatherTelemetryEvent>(),
            item => item.InnerWeatherHandle == resultReceipt.InnerWeatherHandle &&
                    item.ResidueState == AttentionResidueState.Persistent);

#if LOCAL_HDT_BRIDGE
        var outputRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-inner-weather-hopng");
        var hopngService = new Oan.Cradle.LocalHdtHopngArtifactService(outputRoot);
        var governingTrafficArtifact = await hopngService.EmitAsync(hopngRequest);
        var projection = JsonNode.Parse(await File.ReadAllTextAsync(governingTrafficArtifact.ProjectionPath!));

        Assert.NotNull(projection?["community_safe_weather"]);
        Assert.Equal("unstable", projection?["community_safe_weather"]?["status"]?.GetValue<string>());
#else
        Assert.NotNull(third.CommunityWeatherPacket);
#endif
    }

    private static GovernanceCycleStartRequest CreateGoldenPathRequest(Guid identityId)
    {
        return new GovernanceCycleStartRequest(
            IdentityId: identityId,
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-inner-weather",
            SourceCustodyDomain: "cmos",
            SourceTheater: "prime",
            RequestedTheater: "prime",
            PolicyHandle: "agenticore.cognition.cycle",
            OperatorInput: "track civic inner weather");
    }

    private static GovernanceCycleWorkResult CreateApprovedWorkResult(
        Guid identityId,
        GovernanceCycleStartRequest request,
        int ordinal,
        CompassSeedAdvisoryDisposition advisoryDisposition)
    {
        var candidateId = Guid.Parse($"22222222-2222-2222-2222-{ordinal:000000000000}");
        var provenanceMarker = $"membrane-derived:cme:{request.CMEId}|policy:agenticore.cognition.cycle|loop:{ordinal}";
        var returnCandidatePointer = $"agenticore-return://candidate/inner-weather/{ordinal}";
        var sessionHandle = $"soulframe-session://{request.CMEId}/{ordinal}";
        var workingStateHandle = $"soulframe-working://{request.CMEId}/{ordinal}";

        return new GovernanceCycleWorkResult(
            CandidateId: candidateId,
            IdentityId: identityId,
            SoulFrameId: request.SoulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: request.CMEId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ProvenanceMarker: provenanceMarker,
            ReturnCandidatePointer: returnCandidatePointer,
            IntakeIntent: "candidate-return-evaluation",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            ResultType: "cognition-accepted",
            EngramCommitRequired: true,
            ActionableContent: ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
                contentHandle: returnCandidatePointer,
                originSurface: "prime",
                provenanceMarker: provenanceMarker,
                sourceSubsystem: "AgentiCore"),
            ReturnIntakeHandle: $"soulframe://return/{ordinal}",
            ReturnIntakeEnvelopeId: CreateReturnIntakeEnvelopeId(
                sessionHandle,
                returnCandidatePointer,
                provenanceMarker),
            CompassObservation: CreateObservation(request.CMEId, ordinal, request.OperatorInput, advisoryDisposition));
    }

    private static CompassObservationSurface CreateObservation(
        string cmeId,
        int ordinal,
        string objective,
        CompassSeedAdvisoryDisposition advisoryDisposition)
    {
        var workingStateHandle = $"soulframe-working://{cmeId}/{ordinal}";
        var cSelfGelHandle = $"soulframe-cselfgel://{cmeId}/{ordinal}";
        var selfGelHandle = $"soulframe-selfgel://{cmeId}/{ordinal}";
        return new CompassObservationSurface(
            ObservationHandle: CompassObservationKeys.CreateObservationHandle(
                workingStateHandle,
                cSelfGelHandle,
                CompassDoctrineBasin.BoundedLocalityContinuity,
                objective),
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            AnchorState: CompassAnchorState.Held,
            Provenance: CompassObservationProvenance.Braided,
            ObserverIdentity: "AgentiCore Compass",
            WorkingStateHandle: workingStateHandle,
            CSelfGelHandle: cSelfGelHandle,
            SelfGelHandle: selfGelHandle,
            ValidationReferenceHandle: selfGelHandle,
            Objective: objective,
            SeedAdvisory: new CompassSeedAdvisoryObservation(
                Accepted: advisoryDisposition == CompassSeedAdvisoryDisposition.Accepted,
                Decision: "classify-ok",
                Trace: "response-ready",
                Confidence: advisoryDisposition == CompassSeedAdvisoryDisposition.Rejected ? 0.98 : 0.71,
                Payload: "bounded-locality continuity",
                SuggestedActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                SuggestedCompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                SuggestedAnchorState: CompassAnchorState.Held,
                SuggestedSelfTouchClass: CompassSelfTouchClass.ValidationTouch,
                Disposition: advisoryDisposition,
                DispositionReason: advisoryDisposition.ToString().ToLowerInvariant(),
                Justification: "bounded-locality continuity remains dominant"),
            TimestampUtc: DateTimeOffset.UtcNow.AddMinutes(ordinal));
    }

    private static string CreateReturnIntakeEnvelopeId(
        string sessionHandle,
        string returnPointer,
        string provenanceMarker)
    {
        return ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.SoulFrameReturnIntake,
            requestedBy: "AgentiCore",
            scopeHandle: sessionHandle,
            protectionClass: "cryptic-return",
            witnessRequirement: "membrane-witness",
            actionableContent: ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
                contentHandle: returnPointer,
                originSurface: "prime",
                provenanceMarker: provenanceMarker,
                sourceSubsystem: "AgentiCore")).EnvelopeId;
    }

    private static StoreRegistry CreateStoreRegistry(
        ITelemetrySink governanceTelemetry,
        ITelemetrySink storageTelemetry,
        PublicLayerService publicLayer,
        MantleOfSovereigntyService mantle,
        IGovernedPrimePublicationSink governedPrimePublicationSink,
        IGovernanceReceiptJournal governanceReceiptJournal,
        IGovernanceCycleCognitionService governanceCognitionService,
        StewardAgent steward)
    {
        return new StoreRegistry(
            governanceTelemetry: governanceTelemetry,
            storageTelemetry: storageTelemetry,
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
            crypticReengrammitizationGate: mantle,
            governedPrimePublicationSink: governedPrimePublicationSink,
            governanceReceiptJournal: governanceReceiptJournal,
            cmeCollapseQualifier: new CmeCollapseQualifier());
    }

    private static StewardAgent CreateSteward(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        IGovernanceReceiptJournal governanceJournal)
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
            governanceJournal);
    }

    private static string CreateJournalPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance-inner-weather.ndjson");

    private sealed class RecordingTelemetrySink : ITelemetrySink
    {
        public List<object> Events { get; } = [];

        public Task EmitAsync(object telemetryEvent)
        {
            Events.Add(telemetryEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGovernanceCognitionSequenceService : IGovernanceCycleCognitionService
    {
        private readonly Queue<GovernanceCycleWorkResult> _results;

        public FakeGovernanceCognitionSequenceService(params GovernanceCycleWorkResult[] results)
        {
            _results = new Queue<GovernanceCycleWorkResult>(results);
        }

        public Task<GovernanceCycleWorkResult> ExecuteGovernanceCycleAsync(
            GovernanceCycleStartRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_results.Count == 0)
            {
                throw new InvalidOperationException("No more queued governance cognition results.");
            }

            return Task.FromResult(_results.Dequeue());
        }
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
}
