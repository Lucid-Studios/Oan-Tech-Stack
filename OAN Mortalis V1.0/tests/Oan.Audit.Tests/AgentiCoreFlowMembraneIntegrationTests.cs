using System.Net;
using System.Text;
using System.Text.Json;
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
        Assert.Equal("zed-theta:test-audit", result.ZedThetaCandidate.CandidateHandle);
        Assert.Equal(SliAuthorityClass.CandidateBearing, result.ZedThetaCandidate.PacketDirective.AuthorityClass);
        Assert.Equal(SliTheaterAuthorizationState.Withheld, result.TheaterAuthorization.AuthorizationState);
        Assert.Equal("sli-candidate-bearing-only", result.TheaterAuthorization.ReasonCode);
        Assert.NotNull(result.CompassObservation);
        Assert.Equal(CompassDoctrineBasin.Unknown, result.CompassObservation!.ActiveBasin);
        Assert.Equal(CompassDoctrineBasin.Unknown, result.CompassObservation.CompetingBasin);
        Assert.True(result.CompassObservation.SeedAdvisory!.Accepted);
        Assert.Equal(CompassSeedAdvisoryDisposition.Accepted, result.CompassObservation.SeedAdvisory.Disposition);
        Assert.Equal(CompassObservationProvenance.Braided, result.CompassObservation.Provenance);
        Assert.Equal(CompassOeCoePosture.CoeDominant, result.CompassObservation.OeCoePosture);
        Assert.Equal(CompassSelfTouchClass.ValidationTouch, result.CompassObservation.SelfTouchClass);
        Assert.Equal(CompassAnchorState.Weakened, result.CompassObservation.AnchorState);
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

    [Fact]
    public async Task CognitionCycle_ClassifyRequest_AnchorsBoundedLocalityDoctrine()
    {
        string? classifyContext = null;
        string? classifyDomain = null;

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
            CreateSoulFrameHostClient(async (request, _) =>
            {
                if (request.RequestUri?.AbsolutePath == "/classify")
                {
                    using var document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                    classifyContext = document.RootElement.GetProperty("context").GetString();
                    classifyDomain = document.RootElement.GetProperty("opal_constraints").GetProperty("domain").GetString();
                }

                return await DefaultSoulFrameResponder(request);
            }),
            boundedWorker);

        var result = await cognition.ExecuteCognitionCycleAsync(
            CreateContext(),
            "maintain bounded locality continuity under masked locality witness");

        Assert.Equal("bounded-locality continuity", classifyDomain);
        Assert.NotNull(classifyContext);
        Assert.Contains("ACTIVE_DOCTRINE_DOMAIN: bounded-locality continuity", classifyContext, StringComparison.Ordinal);
        Assert.Contains("EXCLUDED_NEARBY_DOMAIN: fluid continuity law", classifyContext, StringComparison.Ordinal);
        Assert.Contains("INPUT:", classifyContext, StringComparison.Ordinal);
        Assert.Equal(CompassDoctrineBasin.BoundedLocalityContinuity, result.ZedThetaCandidate.ActiveBasin);
        Assert.Equal(SliUpdateLocus.Sheaf, result.ZedThetaCandidate.PacketDirective.UpdateLocus);
        Assert.Equal(SliTheaterAuthorizationState.Withheld, result.TheaterAuthorization.AuthorizationState);
        Assert.NotNull(result.CompassObservation);
        Assert.Equal(CompassDoctrineBasin.BoundedLocalityContinuity, result.CompassObservation!.ActiveBasin);
        Assert.Equal(CompassDoctrineBasin.FluidContinuityLaw, result.CompassObservation.CompetingBasin);
        Assert.True(result.CompassObservation.SeedAdvisory!.Accepted);
        Assert.Equal(CompassDoctrineBasin.BoundedLocalityContinuity, result.CompassObservation.SeedAdvisory.SuggestedActiveBasin);
        Assert.Equal(CompassDoctrineBasin.FluidContinuityLaw, result.CompassObservation.SeedAdvisory.SuggestedCompetingBasin);
        Assert.Equal(CompassSeedAdvisoryDisposition.Accepted, result.CompassObservation.SeedAdvisory.Disposition);
        Assert.Equal(CompassObservationProvenance.Braided, result.CompassObservation.Provenance);
        Assert.Equal(CompassOeCoePosture.CoeDominant, result.CompassObservation.OeCoePosture);
        Assert.Equal(CompassSelfTouchClass.ValidationTouch, result.CompassObservation.SelfTouchClass);
        Assert.Equal(CompassAnchorState.Held, result.CompassObservation.AnchorState);
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

    private static SoulFrameHostClient CreateSoulFrameHostClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? responder = null)
    {
        var handler = new DelegatingHandlerStub(responder ?? ((request, _) => DefaultSoulFrameResponder(request)));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:8181")
        };

        return new SoulFrameHostClient(httpClient, telemetry: null, "http://127.0.0.1:8181");
    }

    private static Task<HttpResponseMessage> DefaultSoulFrameResponder(HttpRequestMessage request)
    {
        if (request.RequestUri?.AbsolutePath == "/classify")
        {
            using var document = JsonDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            var context = document.RootElement.GetProperty("context").GetString() ?? string.Empty;
            var domain = document.RootElement.GetProperty("opal_constraints").GetProperty("domain").GetString() ?? string.Empty;
            var json = BuildClassifyResponseJson(context, domain);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        if (request.RequestUri?.AbsolutePath == "/semantic_expand")
        {
            var json = "{\"decision\":\"semantic-expand\",\"payload\":\"hint\",\"confidence\":0.61,\"governance\":{\"state\":\"QUERY\",\"trace\":\"response-ready\",\"content\":\"hint\"}}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"decision\":\"ok\",\"payload\":\"{}\",\"confidence\":0.50,\"governance\":{\"state\":\"QUERY\",\"trace\":\"response-ready\",\"content\":\"{}\"}}", Encoding.UTF8, "application/json")
        });
    }

    private static string BuildClassifyResponseJson(string context, string domain)
    {
        var activeBasin = ResolveSuggestedBasin($"{context} {domain}");
        var competingBasin = activeBasin switch
        {
            CompassDoctrineBasin.BoundedLocalityContinuity => CompassDoctrineBasin.FluidContinuityLaw,
            CompassDoctrineBasin.FluidContinuityLaw => CompassDoctrineBasin.BoundedLocalityContinuity,
            _ => CompassDoctrineBasin.Unknown
        };
        var anchorState = activeBasin == CompassDoctrineBasin.BoundedLocalityContinuity
            ? CompassAnchorState.Held
            : CompassAnchorState.Weakened;
        var payload = activeBasin == CompassDoctrineBasin.BoundedLocalityContinuity
            ? "bounded-locality continuity locality witness"
            : "bounded-payload";
        var justification = activeBasin == CompassDoctrineBasin.BoundedLocalityContinuity
            ? "bounded-locality continuity remains dominant"
            : "no stable continuity basin was selected";

        return JsonSerializer.Serialize(new
        {
            decision = "bounded-classify",
            payload,
            confidence = 0.74,
            governance = new
            {
                state = "QUERY",
                trace = "response-ready",
                content = payload
            },
            compass_advisory = new
            {
                suggested_active_basin = ToCompassToken(activeBasin),
                suggested_competing_basin = ToCompassToken(competingBasin),
                suggested_anchor_state = anchorState.ToString().ToUpperInvariant(),
                suggested_self_touch_class = "VALIDATION_TOUCH",
                confidence = 0.71,
                justification
            }
        });
    }

    private static CompassDoctrineBasin ResolveSuggestedBasin(string value)
    {
        var lowered = value.ToLowerInvariant();
        if (lowered.Contains("bounded-locality continuity", StringComparison.Ordinal) ||
            lowered.Contains("bounded locality continuity", StringComparison.Ordinal) ||
            (lowered.Contains("locality", StringComparison.Ordinal) && lowered.Contains("continuity", StringComparison.Ordinal)))
        {
            return CompassDoctrineBasin.BoundedLocalityContinuity;
        }

        if (lowered.Contains("fluid continuity law", StringComparison.Ordinal) ||
            lowered.Contains("fluid continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.FluidContinuityLaw;
        }

        return CompassDoctrineBasin.Unknown;
    }

    private static string ToCompassToken(CompassDoctrineBasin basin) => basin switch
    {
        CompassDoctrineBasin.BoundedLocalityContinuity => "BOUNDED_LOCALITY_CONTINUITY",
        CompassDoctrineBasin.FluidContinuityLaw => "FLUID_CONTINUITY_LAW",
        CompassDoctrineBasin.IdentityContinuity => "IDENTITY_CONTINUITY",
        CompassDoctrineBasin.GeneralContinuityDiscourse => "GENERAL_CONTINUITY_DISCOURSE",
        _ => "UNKNOWN"
    };

    private sealed class StubCognitionEngine : ICognitionEngine
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
        {
            var objective = request.Context.TaskObjective;
            var activeBasin = objective.Contains("bounded locality continuity", StringComparison.OrdinalIgnoreCase) ||
                              objective.Contains("bounded-locality continuity", StringComparison.OrdinalIgnoreCase)
                ? CompassDoctrineBasin.BoundedLocalityContinuity
                : CompassDoctrineBasin.Unknown;
            var competingBasin = activeBasin == CompassDoctrineBasin.BoundedLocalityContinuity
                ? CompassDoctrineBasin.FluidContinuityLaw
                : CompassDoctrineBasin.Unknown;
            var anchorState = activeBasin == CompassDoctrineBasin.BoundedLocalityContinuity
                ? CompassAnchorState.Held
                : CompassAnchorState.Weakened;
            var zedThetaCandidate = new ZedThetaCandidateReceipt(
                CandidateHandle: "zed-theta:test-audit",
                Objective: objective,
                PrimeState: "task-objective",
                ThetaState: "theta-ready",
                GammaState: "gamma-ready",
                PacketDirective: new SliPacketDirective(
                    SliThinkingTier.Master,
                    SliPacketClass.Commitment,
                    SliEngramOperation.Write,
                    activeBasin == CompassDoctrineBasin.IdentityContinuity ? SliUpdateLocus.Kernel : SliUpdateLocus.Sheaf,
                    SliAuthorityClass.CandidateBearing),
                IdentityKernelBoundary: new IdentityKernelBoundaryReceipt(
                    CmeIdentityHandle: "cme:test",
                    IdentityKernelHandle: "kernel:test",
                    ContinuityAnchorHandle: "anchor:test:bounded-locality",
                    KernelBound: activeBasin == CompassDoctrineBasin.IdentityContinuity,
                    CandidateLocus: activeBasin == CompassDoctrineBasin.IdentityContinuity ? SliUpdateLocus.Kernel : SliUpdateLocus.Sheaf),
                Validity: new SliPacketValidityReceipt(true, true, true, true, "sli-packet-valid"),
                ActiveBasin: activeBasin,
                CompetingBasin: competingBasin,
                AnchorState: anchorState,
                SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
                OeCoePosture: CompassOeCoePosture.CoeDominant);

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
                GoldenCodeCompass = GoldenCodeCompassProjection.FromCandidateReceipt(zedThetaCandidate),
                ZedThetaCandidate = zedThetaCandidate,
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
