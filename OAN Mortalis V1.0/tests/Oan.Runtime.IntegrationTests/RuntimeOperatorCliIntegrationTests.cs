using System.Text;
using System.Text.Json;
using CradleTek.Cryptic;
using CradleTek.Host.Interfaces;
using CradleTek.Mantle;
using CradleTek.Public;
using EngramGovernance.Services;
using OAN.Core.Telemetry;
using Oan.Common;
using Oan.Cradle;
using Oan.Runtime.Headless;
using Oan.Storage;
using SoulFrame.Host;
using Telemetry.GEL;

namespace Oan.Runtime.IntegrationTests;

public sealed class RuntimeOperatorCliIntegrationTests
{
    [Fact]
    public async Task Status_ByCandidateAndProvenance_ReturnsCanonicalSuccessEnvelope()
    {
        var context = CreateOperatorContext(out var manager, mode: TestLoopMode.Completed, out var request, out var workResult);
        await manager.RunGovernanceGoldenPathAsync(request);

        var response = await InvokeCliAsync(
            context,
            "status",
            "--candidate-id",
            workResult.CandidateId.ToString("D"),
            "--provenance",
            workResult.ProvenanceMarker);

        Assert.Equal(0, response.ExitCode);
        Assert.Equal(string.Empty, response.Stderr.Trim());

        using var payload = JsonDocument.Parse(response.Stdout);
        Assert.True(payload.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("status", payload.RootElement.GetProperty("command").GetString());
        Assert.Equal(
            GovernanceLoopKeys.Create(workResult.CandidateId, workResult.ProvenanceMarker),
            payload.RootElement.GetProperty("loopKey").GetString());
        Assert.Equal(
            "Completed",
            payload.RootElement.GetProperty("result").GetProperty("controlState").GetString());
        Assert.Equal(
            "cMoS",
            payload.RootElement.GetProperty("result").GetProperty("latestCollapseQualification").GetProperty("destination").GetString());
        Assert.Equal(
            "AutobiographicalSignal, SelfGelIdentitySignal",
            payload.RootElement.GetProperty("result").GetProperty("latestCollapseQualification").GetProperty("evidenceFlags").GetString());
    }

    [Fact]
    public async Task Deferred_ListGetAnnotateAndApprove_RemainWithinDeferredQueue()
    {
        var context = CreateOperatorContext(out var manager, mode: TestLoopMode.Deferred, out var request, out var workResult);
        var result = await manager.RunGovernanceGoldenPathAsync(request);

        var listResponse = await InvokeCliAsync(context, "deferred", "list");
        Assert.Equal(0, listResponse.ExitCode);
        using (var listJson = JsonDocument.Parse(listResponse.Stdout))
        {
            Assert.Single(listJson.RootElement.GetProperty("result").EnumerateArray());
        }

        var getResponse = await InvokeCliAsync(context, "deferred", "get", "--loop-key", result.LoopKey);
        Assert.Equal(0, getResponse.ExitCode);
        using (var getJson = JsonDocument.Parse(getResponse.Stdout))
        {
            Assert.Equal(result.LoopKey, getJson.RootElement.GetProperty("loopKey").GetString());
            Assert.Equal(
                workResult.CandidateId.ToString("D"),
                getJson.RootElement.GetProperty("result").GetProperty("candidateId").GetString());
        }

        var annotateResponse = await InvokeCliAsync(
            context,
            "deferred",
            "annotate",
            "--loop-key",
            result.LoopKey,
            "--reviewed-by",
            "operator:test",
            "--annotation",
            "needs manual review");
        Assert.Equal(0, annotateResponse.ExitCode);
        using (var annotateJson = JsonDocument.Parse(annotateResponse.Stdout))
        {
            Assert.Equal(
                "Deferred",
                annotateJson.RootElement.GetProperty("result").GetProperty("controlState").GetString());
        }

        var approveResponse = await InvokeCliAsync(
            context,
            "deferred",
            "approve",
            "--loop-key",
            result.LoopKey,
            "--reviewed-by",
            "operator:test",
            "--rationale",
            "steward.approved.manual-review");
        Assert.Equal(0, approveResponse.ExitCode);
        using var approveJson = JsonDocument.Parse(approveResponse.Stdout);
        Assert.Equal(
            "InProgress",
            approveJson.RootElement.GetProperty("result").GetProperty("controlState").GetString());
        Assert.False(
            approveJson.RootElement.GetProperty("result").GetProperty("reengrammitizationCompleted").GetBoolean());
    }

    [Fact]
    public async Task Recovery_Resume_CompletesPendingRecoveryLoop()
    {
        var context = CreateOperatorContext(
            out var manager,
            mode: TestLoopMode.ReengramFailsOnce,
            out var request,
            out _);

        var first = await manager.RunGovernanceGoldenPathAsync(request);
        Assert.Equal(GovernanceLoopStage.PendingRecovery, first.Stage);

        var listResponse = await InvokeCliAsync(context, "recovery", "list");
        Assert.Equal(0, listResponse.ExitCode);
        using (var listJson = JsonDocument.Parse(listResponse.Stdout))
        {
            Assert.Single(listJson.RootElement.GetProperty("result").EnumerateArray());
        }

        var resumeResponse = await InvokeCliAsync(
            context,
            "recovery",
            "resume",
            "--loop-key",
            first.LoopKey,
            "--requested-by",
            "operator:recovery",
            "--reason",
            "resume pending recovery");

        Assert.Equal(0, resumeResponse.ExitCode);
        using var resumeJson = JsonDocument.Parse(resumeResponse.Stdout);
        Assert.Equal(
            "Completed",
            resumeJson.RootElement.GetProperty("result").GetProperty("controlState").GetString());
    }

    [Fact]
    public async Task Recovery_RetryLane_RetriesOnlyRequestedLane()
    {
        var context = CreateOperatorContext(
            out var manager,
            mode: TestLoopMode.CheckedViewFailsOnce,
            out var request,
            out _);

        var first = await manager.RunGovernanceGoldenPathAsync(request);
        Assert.Equal(GovernanceLoopStage.PendingRecovery, first.Stage);

        var retryResponse = await InvokeCliAsync(
            context,
            "recovery",
            "retry-lane",
            "--loop-key",
            first.LoopKey,
            "--lane",
            "checked-view",
            "--requested-by",
            "operator:recovery",
            "--reason",
            "retry missing checked-view lane");

        Assert.Equal(0, retryResponse.ExitCode);
        using var retryJson = JsonDocument.Parse(retryResponse.Stdout);
        Assert.True(retryJson.RootElement.GetProperty("result").GetProperty("publication").GetProperty("pointerPublished").GetBoolean());
        Assert.True(retryJson.RootElement.GetProperty("result").GetProperty("publication").GetProperty("checkedViewPublished").GetBoolean());
        Assert.Equal(
            "Completed",
            retryJson.RootElement.GetProperty("result").GetProperty("controlState").GetString());
    }

    [Fact]
    public async Task Status_MalformedJournal_ReturnsFailedSafeExitCode()
    {
        var journalPath = CreateJournalPath();
        var candidateId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var provenance = "membrane-derived:broken";
        var loopKey = GovernanceLoopKeys.Create(candidateId, provenance);
        await File.WriteAllTextAsync(journalPath, $"{{\"loopKey\":\"{loopKey}\"{Environment.NewLine}");

        var manager = new StackManager(CreateStoreRegistry(
            new PublicLayerService(),
            new MantleOfSovereigntyService(),
            governedPrimePublicationSink: new PublicLayerService(),
            governanceReceiptJournal: new NdjsonGovernanceReceiptJournal(journalPath),
            governanceCognitionService: new FakeGovernanceCognitionService(CreateApprovedWorkResult(Guid.NewGuid(), Guid.NewGuid(), "cme-runtime")),
            steward: CreateSteward(new PublicLayerService(), new CrypticLayerService(), new GelTelemetryAdapter(), new NdjsonGovernanceReceiptJournal(journalPath))));
        var steward = CreateSteward(new PublicLayerService(), new CrypticLayerService(), new GelTelemetryAdapter(), new NdjsonGovernanceReceiptJournal(journalPath));
        var context = new RuntimeOperatorContext(manager, steward, manager);

        var response = await InvokeCliAsync(context, "status", "--loop-key", loopKey);

        Assert.Equal(6, response.ExitCode);
        Assert.Equal(string.Empty, response.Stdout.Trim());
        using var failureJson = JsonDocument.Parse(response.Stderr);
        Assert.False(failureJson.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("failed_safe_evidence", failureJson.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task InvalidArgs_And_NotFound_ReturnExpectedExitCodes()
    {
        var context = CreateOperatorContext(out _, mode: TestLoopMode.Completed, out _, out _);

        var invalidArgs = await InvokeCliAsync(context, "status", "--candidate-id", "not-a-guid", "--provenance", "membrane-derived:test");
        Assert.Equal(2, invalidArgs.ExitCode);

        var notFound = await InvokeCliAsync(context, "deferred", "get", "--loop-key", "loop:missing");
        Assert.Equal(3, notFound.ExitCode);
    }

    [Fact]
    public async Task EvaluateBootstrap_RemainsUsable()
    {
        var runtimeRoot = Path.Combine(Path.GetTempPath(), $"oan-headless-{Guid.NewGuid():N}");
        Directory.CreateDirectory(runtimeRoot);

        var host = await HeadlessRuntimeBootstrap.CreateEvaluateHostAsync(runtimeRoot);
        var result = await host.EvaluateAsync("agent-001", "theater-A", new { input = "CLI_TRIGGER" });

        Assert.Equal("agent-001", result.AgentId);
        Assert.Equal("theater-A", result.TheaterId);
        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
    }

    private static RuntimeOperatorContext CreateOperatorContext(
        out StackManager manager,
        TestLoopMode mode,
        out GovernanceCycleStartRequest request,
        out GovernanceCycleWorkResult workResult)
    {
        var telemetry = new GelTelemetryAdapter();
        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var steward = CreateSteward(publicLayer, crypticLayer, telemetry, journal);
        request = CreateGoldenPathRequest();
        workResult = mode == TestLoopMode.Deferred
            ? CreateDeferredWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId)
            : CreateApprovedWorkResult(request.IdentityId, request.SoulFrameId, request.CMEId);

        ICrypticReengrammitizationGate gate = mode switch
        {
            TestLoopMode.ReengramFailsOnce => new FailOnceReengrammitizationGate(mantle),
            _ => mantle
        };

        IGovernedPrimePublicationSink publicationSink = mode switch
        {
            TestLoopMode.CheckedViewFailsOnce => new FailOncePrimePublicationSink(publicLayer, GovernedPrimeDerivativeLane.CheckedView),
            _ => publicLayer
        };

        manager = new StackManager(CreateStoreRegistry(
            publicLayer,
            mantle,
            publicationSink,
            journal,
            new FakeGovernanceCognitionService(workResult),
            steward,
            gate));

        mantle.AppendAsync(new CrypticCustodyAppendRequest(request.IdentityId, "cMoS", "cmos://seed/source", "seed"))
            .GetAwaiter()
            .GetResult();

        return new RuntimeOperatorContext(manager, steward, manager);
    }

    private static async Task<CliInvocationResult> InvokeCliAsync(
        RuntimeOperatorContext context,
        params string[] args)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var exitCode = await RuntimeOperatorCli.RunAsync(args, stdout, stderr, context);
        return new CliInvocationResult(exitCode, stdout.ToString(), stderr.ToString());
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
                : CmeCollapseEvidenceFlag.ContextualSignal | CmeCollapseEvidenceFlag.ProceduralSignal | CmeCollapseEvidenceFlag.SkillMethodSignal,
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
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.operator-cli.ndjson");

    private sealed record CliInvocationResult(int ExitCode, string Stdout, string Stderr);

    private enum TestLoopMode
    {
        Completed,
        Deferred,
        ReengramFailsOnce,
        CheckedViewFailsOnce
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
                throw new InvalidOperationException("forced-reengrammitization-failure");
            }

            return _inner.ReengrammitizeAsync(request, cancellationToken);
        }
    }
}
