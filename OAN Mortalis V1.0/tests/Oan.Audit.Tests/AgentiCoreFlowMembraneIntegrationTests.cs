using System.Net;
using System.Text;
using AgentiCore.Models;
using AgentiCore.Services;
using AgentiCoreService = AgentiCore.Services.AgentiCore;
using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;
using CradleTek.Host.Interfaces;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Models;
using Oan.Common;
using SoulFrame.Host;
using Telemetry.GEL;

namespace Oan.Audit.Tests;

public sealed class AgentiCoreFlowMembraneIntegrationTests
{
    [Fact]
    public async Task CognitionCycle_UsesBoundedWorkerStage_WithoutWideningMembrane()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicStore = new RecordingPublicStore();
        var crypticStore = new RecordingCrypticStore();
        var membrane = new RecordingMembrane();
        var boundedWorker = new BoundedMembraneWorkerService(membrane);
        var cognition = new AgentiCoreService(
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
            CreateSoulFrameHostClient(),
            boundedWorker);

        var context = CreateContext();

        var result = await cognition.ExecuteCognitionCycleAsync(context, "solve bounded task");

        Assert.Equal(context.ContextId, result.ContextId);
        Assert.Equal("cognition-accepted", result.ResultType);
        Assert.NotNull(context.SelfGelWorkingPool);
        Assert.Same(context.SelfGelWorkingPool, result.SelfGelWorkingPool);
        Assert.Equal("bounded-selfgel-working-pool", result.SelfGelWorkingPool.Classification);
        Assert.StartsWith("soulframe-cselfgel://", result.SelfGelWorkingPool.CSelfGelHandle, StringComparison.Ordinal);
        Assert.Equal("cooled-selfgel-validation-surface", result.SelfGelWorkingPool.ValidationSurface.Classification);
        Assert.Equal("validation-only", result.SelfGelWorkingPool.ValidationSurface.ValidationPosture);
        Assert.StartsWith("soulframe-selfgel://", result.SelfGelWorkingPool.ValidationSurface.SelfGelHandle, StringComparison.Ordinal);
        Assert.Contains("identity-continuity", result.SelfGelWorkingPool.ValidationSurface.ValidatedConcepts, StringComparer.OrdinalIgnoreCase);
        Assert.Empty(result.SelfGelWorkingPool.Claims);
        Assert.Equal("symbolic-trace", result.SymbolicTrace.Classification);
        Assert.Equal("candidate-engram-structure", result.EngramCandidate.Classification);
        Assert.Equal("residue", result.TransientResidue.CleaveResidue);
        Assert.Equal("prime", context.WorkingMemory["membrane_target_theater"]);
        Assert.StartsWith("soulframe-session://", context.WorkingMemory["membrane_session_handle"], StringComparison.Ordinal);
        Assert.StartsWith("soulframe-working://", context.WorkingMemory["membrane_working_state_handle"], StringComparison.Ordinal);
        Assert.StartsWith("membrane-derived:", context.WorkingMemory["membrane_provenance_marker"], StringComparison.Ordinal);
        Assert.StartsWith("soulframe://return/", context.WorkingMemory["membrane_return_handle"], StringComparison.Ordinal);
        Assert.Equal("return-candidate-recorded", context.WorkingMemory["membrane_return_disposition"]);
        Assert.NotEmpty(context.WorkingMemory["membrane_return_request_envelope_id"]);
        Assert.NotEmpty(context.WorkingMemory["membrane_actionable_content_handle"]);
        Assert.Equal(context.WorkingMemory["membrane_working_state_handle"], result.SelfGelWorkingPool.WorkingStateHandle);
        Assert.Equal(context.WorkingMemory["membrane_provenance_marker"], result.SelfGelWorkingPool.ProvenanceMarker);
        Assert.Equal(context.WorkingMemory["selfgel_validation_surface_handle"], result.SelfGelWorkingPool.ValidationSurface.SelfGelHandle);
        Assert.Equal("0", context.WorkingMemory["selfgel_claim_count"]);
        Assert.Equal("none", context.WorkingMemory["selfgel_claim_postures"]);
        Assert.Contains("hosted_semantic_decision", result.SelfGelWorkingPool.WorkingMemory.Keys, StringComparer.Ordinal);
        Assert.Equal(["step-a", "step-b"], result.SymbolicTrace.Steps);
        Assert.Equal(["token-a"], result.SymbolicTrace.Tokens);
        Assert.StartsWith("agenticore-return://", result.EngramCandidate.ReturnCandidatePointer, StringComparison.Ordinal);
        Assert.DoesNotContain("cmos://", result.SelfGelWorkingPool.CSelfGelHandle, StringComparison.OrdinalIgnoreCase);

        Assert.NotNull(membrane.LastProjectionRequest);
        Assert.NotNull(membrane.LastReturnRequest);
        Assert.Equal("candidate-return-evaluation", membrane.LastReturnRequest!.IntakeIntent);
        Assert.StartsWith("agenticore-return://", membrane.LastReturnRequest.ReturnCandidatePointer, StringComparison.Ordinal);
        Assert.Equal(ControlSurfaceKind.SoulFrameReturnIntake, membrane.LastReturnRequest.RequestEnvelope.TargetSurface);
        Assert.Equal(membrane.LastReturnRequest.RequestEnvelope.EnvelopeId, context.WorkingMemory["membrane_return_request_envelope_id"]);
    }

    [Fact]
    public async Task CognitionCycle_ForgedCustodyShapedHandle_IsRejected()
    {
        var telemetry = new GelTelemetryAdapter();
        var publicStore = new RecordingPublicStore();
        var crypticStore = new RecordingCrypticStore();
        var membrane = new RecordingMembrane
        {
            Projection = new SelfStateProjection(
                Guid.NewGuid(),
                ProjectionHandle: "soulframe://projection/forged",
                SessionHandle: "soulframe-session://cme-alpha/forged",
                TargetTheater: "prime",
                IsMitigated: true,
                WorkingStateHandle: "cmos://raw-state/forged",
                ProvenanceMarker: "membrane-derived:cme:cme-alpha|policy:agenticore.cognition.cycle",
                MediatedSelfState: CreateMediatedSelfState("cme-alpha", "agenticore.cognition.cycle"))
        };
        var boundedWorker = new BoundedMembraneWorkerService(membrane);
        var cognition = new AgentiCoreService(
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
            CreateSoulFrameHostClient(),
            boundedWorker);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cognition.ExecuteCognitionCycleAsync(CreateContext(), "solve bounded task"));
    }

    private static MediatedSelfStateContour CreateMediatedSelfState(string cmeId, string policyHandle) =>
        new(
            CSelfGelHandle: $"soulframe-cselfgel://{cmeId}/{Guid.NewGuid():D}",
            Classification: "mediated-cselfgel-issue",
            PolicyHandle: policyHandle);

    private static AgentiContext CreateContext()
    {
        return new AgentiContext
        {
            CMEId = "cme-alpha",
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid(),
            ActiveConcepts = ["Engram", "SLI"],
            WorkingMemory = new Dictionary<string, string>(StringComparer.Ordinal),
            ExecutionTimestamp = DateTime.UtcNow
        };
    }

    private static EngramCommitService CreateCommitService(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        var steward = new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            publicStore,
            crypticStore,
            telemetry);
        return new EngramCommitService(steward);
    }

    private static SoulFrameHostClient CreateSoulFrameHostClient()
    {
        var handler = new DelegatingHandlerStub((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/classify")
            {
                var json = "{\"decision\":\"bounded-classify\",\"payload\":\"bounded-payload\",\"confidence\":0.74}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            if (request.RequestUri?.AbsolutePath == "/semantic_expand")
            {
                var json = "{\"decision\":\"semantic-expand\",\"payload\":\"hint\",\"confidence\":0.61}";
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

    private sealed class StubCognitionEngine : ICognitionEngine
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CognitionResult
            {
                Reasoning = "bounded reasoning",
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
                Unknown = ["bounded", "task"],
                Metrics = new OntologicalCleaverMetrics
                {
                    KnownRatio = 0.0,
                    PartiallyKnownRatio = 0.0,
                    UnknownRatio = 1.0,
                    ConceptDensity = "sparse",
                    ContextStability = "stable"
                },
                CanonicalRootAtlas = RootAtlas.Create("stub.root-atlas.v1", Array.Empty<RootAtlasEntry>())
            });
        }
    }

    private sealed class RecordingPublicStore : IPublicStore
    {
        private readonly List<string> _pointers = [];

        public string ContainerName => "public";

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task PublishPointerAsync(string pointer, CancellationToken cancellationToken = default)
        {
            _pointers.Add(pointer);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> ListPublishedPointersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>(_pointers.ToList());
    }

    private sealed class RecordingCrypticStore : ICrypticStore
    {
        private readonly List<string> _pointers = [];

        public string ContainerName => "cryptic";

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string> StorePointerAsync(string pointer, CancellationToken cancellationToken = default)
        {
            _pointers.Add(pointer);
            return Task.FromResult(pointer);
        }

        public Task<IReadOnlyList<string>> ListPointersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>(_pointers.ToList());
    }

    private sealed class RecordingMembrane : ISoulFrameMembrane
    {
        public SoulFrameProjectionRequest? LastProjectionRequest { get; private set; }
        public SoulFrameReturnIntakeRequest? LastReturnRequest { get; private set; }

        public SelfStateProjection Projection { get; set; } = new(
            Guid.NewGuid(),
            ProjectionHandle: "soulframe://projection/default",
            SessionHandle: "soulframe-session://cme-alpha/default",
            TargetTheater: "prime",
            IsMitigated: true,
            WorkingStateHandle: "soulframe-working://cme-alpha/default",
            ProvenanceMarker: "membrane-derived:cme:cme-alpha|policy:agenticore.cognition.cycle",
            MediatedSelfState: CreateMediatedSelfState("cme-alpha", "agenticore.cognition.cycle"));

        public Task<ISelfStateProjection> ProjectMitigatedAsync(
            SoulFrameProjectionRequest request,
            CancellationToken cancellationToken = default)
        {
            LastProjectionRequest = request;
            return Task.FromResult<ISelfStateProjection>(Projection with { IdentityId = request.IdentityId });
        }

        public Task<SoulFrameReturnIntakeReceipt> ReceiveReturnIntakeAsync(
            SoulFrameReturnIntakeRequest request,
            CancellationToken cancellationToken = default)
        {
            LastReturnRequest = request;
            ControlSurfaceContractGuards.ValidateSoulFrameReturnIntakeRequest(request);
            return Task.FromResult(new SoulFrameReturnIntakeReceipt(
                request.IdentityId,
                IntakeHandle: "soulframe://return/integration",
                Accepted: true,
                Disposition: "return-candidate-recorded",
                Evaluation: new SoulFrameCollapseEvaluation(
                    Classification: "candidate-collapse-evaluation",
                    CollapseClassification: new CmeCollapseClassification(
                        CollapseConfidence: 0.92,
                        SelfGelIdentified: true,
                        AutobiographicalRelevant: true,
                        EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                        ReviewTriggers: CmeCollapseReviewTrigger.None,
                        SourceSubsystem: "AgentiCore"),
                    ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                    ReviewState: CmeCollapseReviewState.DeferredReview,
                    RequiresReview: true,
                    CanRouteToCustody: false,
                    CanPublishPrime: false),
                RequestEnvelopeId: request.RequestEnvelope.EnvelopeId,
                ActionableContentHandle: request.RequestEnvelope.ActionableContent.ContentHandle));
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
