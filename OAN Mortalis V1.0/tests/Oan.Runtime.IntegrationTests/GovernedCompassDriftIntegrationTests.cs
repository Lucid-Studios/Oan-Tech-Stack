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

public sealed class GovernedCompassDriftIntegrationTests
{
    [Fact]
    public async Task GoldenPath_ProjectsCompassDriftIntoResultStatusJournalAndHopng()
    {
        var telemetry = new RecordingTelemetrySink();
        var storageTelemetry = new RecordingTelemetrySink();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath(), new TestPermissiveEgressRouter());
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
        var driftEntry = Assert.Single(replay.Where(entry => entry.CompassDriftReceipt is not null));
        var snapshot = GovernanceLoopStateModel.Project(third.LoopKey, replay);
        Assert.NotNull(third.CollapseRoutingDecision);
        var refs = GovernedHopngEvidenceReferences.Build(
            new GovernedHopngEmissionRequest(
                LoopKey: third.LoopKey,
                CandidateId: third.CandidateId,
                CandidateProvenance: third.DecisionReceipt.CandidateProvenance,
                Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
                Stage: snapshot.Stage,
                RequestedBy: "CradleTek",
                DecisionReceipt: third.DecisionReceipt,
                Snapshot: snapshot,
                JournalEntries: replay,
                CollapseRoutingDecision: third.CollapseRoutingDecision!),
            snapshot);

        var resultDrift = Assert.Single(third.CompassDriftReceipts!);
        var statusDrift = Assert.Single(status.CompassDriftReceipts!);

        Assert.Equal(CompassDriftState.Weakened, resultDrift.DriftState);
        Assert.Equal(3, resultDrift.ObservationCount);
        Assert.Equal(3, resultDrift.WindowSize);
        Assert.Equal(2, resultDrift.AdvisoryDivergenceCount);
        Assert.Equal(resultDrift.DriftHandle, statusDrift.DriftHandle);
        Assert.Equal(resultDrift.DriftHandle, driftEntry.CompassDriftReceipt!.DriftHandle);
        Assert.Equal(resultDrift.DriftHandle, Assert.Single(snapshot.CompassDriftReceipts!).DriftHandle);
        Assert.Contains(refs, reference => reference.PointerUri == resultDrift.DriftHandle);
        Assert.Contains(
            telemetry.Events.OfType<GovernedCompassDriftTelemetryEvent>(),
            item => item.DriftHandle == resultDrift.DriftHandle && item.DriftState == CompassDriftState.Weakened);
    }

    private static GovernanceCycleStartRequest CreateGoldenPathRequest(Guid identityId)
    {
        return new GovernanceCycleStartRequest(
            IdentityId: identityId,
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-compass-drift",
            SourceCustodyDomain: "cmos",
            SourceTheater: "prime",
            RequestedTheater: "prime",
            PolicyHandle: "agenticore.cognition.cycle",
            OperatorInput: "track bounded locality continuity");
    }

    private static GovernanceCycleWorkResult CreateApprovedWorkResult(
        Guid identityId,
        GovernanceCycleStartRequest request,
        int ordinal,
        CompassSeedAdvisoryDisposition advisoryDisposition)
    {
        var candidateId = Guid.Parse($"11111111-1111-1111-1111-{ordinal:000000000000}");
        var provenanceMarker = $"membrane-derived:cme:{request.CMEId}|policy:agenticore.cognition.cycle|loop:{ordinal}";
        var returnCandidatePointer = $"agenticore-return://candidate/compass-drift/{ordinal}";
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
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance-drift.ndjson");

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
