using System.Security.Cryptography;
using System.Text;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.CognitionHost.Models;
using Oan.Common;
using SLI.Engine.Cognition;
using SLI.Engine.Nexus;
using SLI.Engine.Morphology;
using SLI.Engine.Models;
using SLI.Engine.Parser;
using SLI.Engine.Runtime;
using SLI.Engine.Telemetry;
using SLI.Lisp;
using SoulFrame.Host;

namespace SLI.Engine;

public sealed class LispBridge
{
    private static readonly string[] RequiredModules =
    [
        "core.lisp",
        "parser.lisp",
        "reasoning.lisp",
        "golden-code.lisp",
        "engram.lisp",
        "compass.lisp",
        "diagnostics.lisp",
        "morphology.lisp",
        "locality.lisp",
        "rehearsal.lisp",
        "witness.lisp",
        "transport.lisp",
        "admissibility.lisp",
        "accountability.lisp"
    ];

    private readonly IEngramResolver _resolver;
    private readonly ISoulFrameSemanticDevice _semanticDevice;
    private readonly SliParser _parser = new();
    private readonly SliInterpreter _interpreter = new(new SliSymbolTable());
    private readonly SliCoreProgramLowerer _lowerer = new();
    private SliBoundedCompositionExpander? _boundedCompositionExpander;
    private bool _initialized;

    public LispBridge(IEngramResolver resolver, ISoulFrameSemanticDevice? semanticDevice = null)
    {
        _resolver = resolver;
        _semanticDevice = semanticDevice ?? NullSoulFrameSemanticDevice.Instance;
    }

    internal static LispBridge CreateForDetachedRuntime()
    {
        return new LispBridge(NullEngramResolver.Instance);
    }

    public IReadOnlyDictionary<string, string> LoadedModules { get; private set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public SliRuntimeCapabilityManifest CapabilityManifest => _interpreter.CapabilityManifest;

    public SliRuntimeCapabilityManifest CreateTargetCapabilityManifest(
        IEnumerable<string> supportedOpcodes,
        string runtimeId = "target-sli-runtime",
        SliRuntimeRealizationProfile? realizationProfile = null)
    {
        return _interpreter.CreateTargetCapabilityManifest(supportedOpcodes, runtimeId, realizationProfile);
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var modules = LispModuleCatalog.LoadModules();
        foreach (var required in RequiredModules)
        {
            if (!modules.ContainsKey(required))
            {
                throw new InvalidOperationException($"Required Lisp module '{required}' is not available.");
            }
        }

        LoadedModules = modules;
        _boundedCompositionExpander = new SliBoundedCompositionExpander(modules);
        _initialized = true;
        return Task.CompletedTask;
    }

    public async Task<LispExecutionResult> ExecuteProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        ContextFrame frame,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        ArgumentNullException.ThrowIfNull(frame);

        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(frame, _resolver, _semanticDevice);
        if (SliCompassLocalityShards.IsPilotEligible(program))
        {
            context.EnableCompassShardMode();
        }

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        if (context.ShardModeEnabled)
        {
            context.FinalizeCompassShardRun();
        }

        EnforceCanonicalOrdering(context.TraceLines);

        var cleaveResidue = context.PrunedBranches.Count == 0
            ? "[]"
            : $"[{string.Join(", ", context.PrunedBranches)}]";
        var traceHash = HashHex(string.Join("\n", context.TraceLines));
        var trace = context.TraceLines
            .Select((line, index) => $"{index + 1}. {line}")
            .ToList();

        var compass = BuildCompassState(context.TraceLines);
        var decisionBranch = context.CandidateBranches.FirstOrDefault() ?? context.FinalDecision;
        var traceId = CreateDeterministicTraceId(frame.CMEId, frame.ContextId, traceHash).ToString("D");
        var zedThetaCandidate = BuildZedThetaCandidate(context, traceId);
        var actualizationPacket = SliActualizationWebbingPacketFactory.CreateForCognition(context, traceId, zedThetaCandidate);
        var liveRuntimeRun = context.ShardModeEnabled
            ? SliLiveEngramRuntimePacketFactory.CreateRunForCognition(context, traceId, zedThetaCandidate)
            : null;

        var result = new LispExecutionResult
        {
            TraceId = traceId,
            Decision = context.FinalDecision,
            DecisionBranch = decisionBranch,
            CleaveResidue = cleaveResidue,
            SymbolicTrace = trace,
            SymbolicTraceHash = traceHash,
            CompassState = compass,
            GoldenCodeCompass = GoldenCodeCompassProjection.FromCandidateReceipt(zedThetaCandidate),
            ZedThetaCandidate = zedThetaCandidate,
            ActualizationPacket = actualizationPacket,
            LiveRuntimePacket = liveRuntimeRun is not null
                ? SliLiveEngramRuntimePacketFactory.ResolveCompatibilityPacket(liveRuntimeRun)
                : SliLiveEngramRuntimePacketFactory.CreateForCognition(context, traceId, zedThetaCandidate),
            LiveRuntimeRun = liveRuntimeRun
        };

        var snapshot = SliExecutionSnapshotFactory.CreateForCognition(context, result);
        result.ExecutionSnapshot = snapshot;
        result.CrypticWebNexus = CrypticWebNexusFactory.Create(snapshot);
        return result;
    }

    internal async Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "locality-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return SliHigherOrderLocalityResultFactory.Create(context);
    }

    internal SliTargetLaneEligibility EvaluateHigherOrderLocalityTargetEligibility(
        IReadOnlyList<string> symbolicProgram,
        SliRuntimeCapabilityManifest targetManifest)
    {
        ArgumentNullException.ThrowIfNull(symbolicProgram);
        ArgumentNullException.ThrowIfNull(targetManifest);

        var program = LowerProgram(symbolicProgram);
        return SliTargetLaneGuard.EvaluateHigherOrderLocality(program, targetManifest);
    }

    internal void EnsureHigherOrderLocalityTargetEligibility(
        IReadOnlyList<string> symbolicProgram,
        SliRuntimeCapabilityManifest targetManifest)
    {
        EvaluateHigherOrderLocalityTargetEligibility(symbolicProgram, targetManifest).EnsureEligible();
    }

    internal async Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityOnTargetAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        ISliTargetHigherOrderLocalityExecutor targetExecutor,
        ITelemetrySink governanceTelemetry,
        string witnessedBy = "CradleTek",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(governanceTelemetry);

        return await ExecuteHigherOrderLocalityOnTargetAsync(
                symbolicProgram,
                objective,
                targetExecutor,
                governanceTelemetry,
                witnessedBy,
                emitGovernedTelemetry: true,
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityOnTargetAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        ISliTargetHigherOrderLocalityExecutor targetExecutor,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteHigherOrderLocalityOnTargetAsync(
                symbolicProgram,
                objective,
                targetExecutor,
                governanceTelemetry: null,
                witnessedBy: "CradleTek",
                emitGovernedTelemetry: false,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityOnTargetAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        ISliTargetHigherOrderLocalityExecutor targetExecutor,
        ITelemetrySink? governanceTelemetry,
        string witnessedBy,
        bool emitGovernedTelemetry,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(symbolicProgram);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        ArgumentNullException.ThrowIfNull(targetExecutor);
        if (emitGovernedTelemetry)
        {
            ArgumentNullException.ThrowIfNull(governanceTelemetry);
            ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);
        }

        var program = LowerProgram(symbolicProgram);
        var eligibility = SliTargetLaneGuard.EvaluateHigherOrderLocality(program, targetExecutor.CapabilityManifest);
        var admission = SliTargetExecutionContracts.CreateAdmission(eligibility, targetExecutor.CapabilityManifest);
        if (!eligibility.IsEligible)
        {
            await EmitGovernedTargetTelemetryAsync(
                    governanceTelemetry,
                    SliTargetExecutionTelemetry.CreateAdmissionEvent(admission, objective, program.ProgramId, witnessedBy),
                    emitGovernedTelemetry,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new SliTargetLaneRefusalException(eligibility);
        }

        await EmitGovernedTargetTelemetryAsync(
                governanceTelemetry,
                SliTargetExecutionTelemetry.CreateAdmissionEvent(admission, objective, program.ProgramId, witnessedBy),
                emitGovernedTelemetry,
                cancellationToken)
            .ConfigureAwait(false);

        var result = await targetExecutor.ExecuteAsync(
                new SliTargetHigherOrderLocalityExecutionRequest(
                    Objective: objective,
                    Program: program,
                    Eligibility: eligibility,
                    Admission: admission),
                cancellationToken)
            .ConfigureAwait(false);

        if (result.TargetLineage is not null)
        {
            await EmitGovernedTargetTelemetryAsync(
                    governanceTelemetry,
                    SliTargetExecutionTelemetry.CreateLineageEvent(admission, result.TargetLineage, witnessedBy),
                    emitGovernedTelemetry,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    internal Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityOnTargetAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        SliRuntimeCapabilityManifest targetManifest,
        ITelemetrySink governanceTelemetry,
        string witnessedBy = "CradleTek",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targetManifest);
        ArgumentNullException.ThrowIfNull(governanceTelemetry);

        return ExecuteHigherOrderLocalityOnTargetAsync(
            symbolicProgram,
            objective,
            new SliBoundedHigherOrderLocalityTargetExecutor(targetManifest),
            governanceTelemetry,
            witnessedBy,
            cancellationToken);
    }

    internal Task<SliHigherOrderLocalityResult> ExecuteHigherOrderLocalityOnTargetAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        SliRuntimeCapabilityManifest targetManifest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targetManifest);

        return ExecuteHigherOrderLocalityOnTargetAsync(
            symbolicProgram,
            objective,
            new SliBoundedHigherOrderLocalityTargetExecutor(targetManifest),
            cancellationToken);
    }

    internal async Task<SliBoundedRehearsalResult> ExecuteBoundedRehearsalProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "rehearsal-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return CreateBoundedRehearsalResult(context);
    }

    internal async Task<SliBoundedWitnessResult> ExecuteBoundedWitnessProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "witness-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return CreateBoundedWitnessResult(context);
    }

    internal async Task<SliBoundedTransportResult> ExecuteBoundedTransportProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "transport-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return CreateBoundedTransportResult(context);
    }

    internal async Task<SliBoundedAdmissibleSurfaceResult> ExecuteAdmissibleSurfaceProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "admissibility-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return CreateAdmissibleSurfaceResult(context);
    }

    internal async Task<SliBoundedAccountabilityPacketResult> ExecuteAccountabilityPacketProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "packet-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return CreateAccountabilityPacketResult(context);
    }

    internal async Task<SliMorphologySentenceResult> ExecuteMorphologySentenceProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string sentence,
        CancellationToken cancellationToken = default)
    {
        var state = await ExecuteMorphologyProgramAsync(symbolicProgram, sentence, cancellationToken).ConfigureAwait(false);
        return new SliMorphologySentenceResult
        {
            Sentence = sentence,
            ResolvedLemmaRoots = state.ResolvedLemmaRoots.ToArray(),
            OperatorAnnotations = state.OperatorAnnotations
                .Select(annotation => new SliMorphologyOperatorAnnotation
                {
                    Token = annotation.Token,
                    Kind = annotation.Kind
                })
                .ToArray(),
            ConstructorBodies = state.ConstructorBodies
                .Select(body => new SliMorphologyConstructorBody
                {
                    Role = body.Role,
                    RootKey = body.RootKey
                })
                .ToArray(),
            DiagnosticPredicateRender = state.DiagnosticPredicateRender,
            LaneOutcome = Enum.TryParse<SliMorphologyLaneOutcome>(state.Outcome, ignoreCase: true, out var outcome)
                ? outcome
                : SliMorphologyLaneOutcome.OutOfScope,
            PredicateRoot = state.PredicateRoot,
            Summary = state.Summary,
            ScalarPayload = state.ScalarPayload
        };
    }

    internal async Task<SliMorphologyParagraphResult> ExecuteMorphologyParagraphProgramAsync(
        IReadOnlyList<SliMorphologySentenceResult> sentenceResults,
        IReadOnlyList<string> symbolicProgram,
        string paragraph,
        CancellationToken cancellationToken = default)
    {
        var state = await ExecuteMorphologyProgramAsync(symbolicProgram, paragraph, cancellationToken).ConfigureAwait(false);
        return new SliMorphologyParagraphResult
        {
            LaneOutcome = SliMorphologyLaneOutcome.Closed,
            Paragraph = paragraph,
            SentenceResults = sentenceResults,
            GraphEdges = state.GraphEdges.ToArray()
        };
    }

    internal async Task<SliMorphologyParagraphBodyResult> ExecuteMorphologyParagraphBodyProgramAsync(
        SliMorphologyParagraphResult paragraphResult,
        IReadOnlyList<string> symbolicProgram,
        string paragraph,
        CancellationToken cancellationToken = default)
    {
        var state = await ExecuteMorphologyProgramAsync(symbolicProgram, paragraph, cancellationToken).ConfigureAwait(false);
        return new SliMorphologyParagraphBodyResult
        {
            LaneOutcome = SliMorphologyLaneOutcome.Closed,
            Paragraph = paragraph,
            ParagraphResult = paragraphResult,
            ContinuityAnchors = state.ContinuityAnchors.ToArray(),
            ParagraphInvariants = state.BodyInvariants.ToArray(),
            ClusterDiagnosticRender = state.ClusterEntries.Count == 0
                ? string.Empty
                : $"cluster[{string.Join("; ", state.ClusterEntries)}]",
            BodySummary = state.BodySummary
        };
    }

    internal async Task<SliPropositionalCompileResult> ExecutePropositionProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken = default)
    {
        var state = await ExecutePropositionProgramStateAsync(symbolicProgram, objective, cancellationToken).ConfigureAwait(false);
        return new SliPropositionalCompileResult
        {
            Subject = new SliPropositionTermResult
            {
                RootKey = state.Subject.RootKey,
                SymbolicHandle = state.Subject.SymbolicHandle
            },
            PredicateRoot = state.PredicateRoot,
            Object = new SliPropositionTermResult
            {
                RootKey = state.Object.RootKey,
                SymbolicHandle = state.Object.SymbolicHandle
            },
            Qualifiers = state.Qualifiers
                .Select(qualifier => new SliPropositionQualifierResult
                {
                    Name = qualifier.Name,
                    Value = qualifier.Value
                })
                .ToArray(),
            ContextTags = state.ContextTags
                .Select(tag => new SliPropositionContextTagResult
                {
                    Name = tag.Name,
                    Value = tag.Value
                })
                .ToArray(),
            DiagnosticRender = state.DiagnosticRender,
            UnresolvedTensions = state.UnresolvedTensions.ToArray(),
            Grade = Enum.TryParse<SliPropositionalCompileGrade>(state.Grade, ignoreCase: true, out var grade)
                ? grade
                : SliPropositionalCompileGrade.NeedsSpecification
        };
    }

    private static CognitiveCompassState BuildCompassState(IReadOnlyList<string> traceLines)
    {
        var symbolicDepth = traceLines.Count;
        var branchCount = traceLines.Count(line => line.StartsWith("decision-branch(", StringComparison.Ordinal));
        var cleaveCount = traceLines.Count(line => line.StartsWith("cleave(", StringComparison.Ordinal));
        var engramCount = traceLines.Count(line => line.Contains("engram", StringComparison.OrdinalIgnoreCase));
        var contextExpandCount = traceLines.Count(line => line.StartsWith("context-expand(", StringComparison.Ordinal));
        var predicateCount = traceLines.Count(line => line.StartsWith("predicate-evaluate(", StringComparison.Ordinal));
        var governanceFlags = traceLines.Count(line => line.Contains("rejected", StringComparison.OrdinalIgnoreCase));
        var commitCount = traceLines.Count(line => line.StartsWith("commit(", StringComparison.Ordinal));
        var cleaveRatio = branchCount == 0 ? 0.0 : (double)cleaveCount / branchCount;
        var contextExpansionRate = symbolicDepth == 0 ? 0.0 : (double)contextExpandCount / symbolicDepth;
        var predicateAlignment = symbolicDepth == 0 ? 0.0 : (double)predicateCount / symbolicDepth;
        var entropy = EntropyFromTrace(traceLines);
        var commitConfidence = commitCount > 0 ? Math.Clamp(1.0 - entropy, 0.0, 1.0) : 0.0;

        var idForce = Normalize(branchCount + symbolicDepth + contextExpandCount);
        var superegoConstraint = Normalize(cleaveRatio + predicateAlignment + (governanceFlags > 0 ? 0.5 : 0.0));
        var egoStability = Normalize((1.0 / Math.Max(0.01, entropy)) + commitConfidence);
        var valueElevation = EvaluateValueElevation(predicateAlignment, governanceFlags, contextExpansionRate);

        return new CognitiveCompassState
        {
            IdForce = idForce,
            SuperegoConstraint = superegoConstraint,
            EgoStability = egoStability,
            ValueElevation = valueElevation,
            SymbolicDepth = symbolicDepth,
            BranchingFactor = branchCount,
            DecisionEntropy = entropy,
            Timestamp = DateTime.UtcNow,
            ContextExpansionRate = Math.Round(contextExpansionRate, 4),
            PredicateAlignment = Math.Round(predicateAlignment, 4),
            CleaveRatio = Math.Round(cleaveRatio, 4),
            GovernanceFlags = governanceFlags,
            CommitConfidence = Math.Round(commitConfidence, 4)
        };
    }

    private static ZedThetaCandidateReceipt BuildZedThetaCandidate(
        SliExecutionContext context,
        string traceId)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var state = context.GoldenCodeState;
        var candidateHandle = $"zed-theta:{traceId}";
        var bridgeReview = SliBridgeContracts.CreateCandidateBridgeReview(
            bridgeStage: "zed-theta-candidate",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: $"sli-bridge://{traceId}",
            thetaState: state.ThetaState,
            gammaState: state.GammaState,
            packetDirective: state.PacketDirective,
            identityKernelBoundary: state.IdentityKernelBoundary,
            validity: state.PacketValidity,
            activeBasin: state.ActiveBasin,
            competingBasin: state.CompetingBasin,
            anchorState: state.AnchorState,
            selfTouchClass: state.SelfTouchClass);
        var runtimeUseCeiling = SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();
        return new ZedThetaCandidateReceipt(
            CandidateHandle: candidateHandle,
            Objective: context.Frame.TaskObjective,
            PrimeState: state.PrimeState,
            ThetaState: state.ThetaState,
            GammaState: state.GammaState,
            PacketDirective: state.PacketDirective,
            IdentityKernelBoundary: state.IdentityKernelBoundary,
            Validity: state.PacketValidity,
            ActiveBasin: state.ActiveBasin,
            CompetingBasin: state.CompetingBasin,
            AnchorState: state.AnchorState,
            SelfTouchClass: state.SelfTouchClass,
            OeCoePosture: state.OeCoePosture,
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling);
    }

    private static double EntropyFromTrace(IReadOnlyList<string> traceLines)
    {
        if (traceLines.Count == 0)
        {
            return 0.0;
        }

        var hash = HashHex(string.Join("|", traceLines));
        var sample = hash[..8];
        var number = Convert.ToUInt32(sample, 16);
        return Math.Round(number / (double)uint.MaxValue, 6);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static Guid CreateDeterministicTraceId(string cmeId, Guid contextId, string traceHash)
    {
        var source = $"{cmeId}|{contextId:D}|{traceHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        var guidBytes = new byte[16];
        Buffer.BlockCopy(bytes, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    private static double Normalize(double value)
    {
        var normalized = value / (1.0 + Math.Abs(value));
        return Math.Round(Math.Clamp(normalized, 0.0, 1.0), 6);
    }

    private static ValueElevation EvaluateValueElevation(double predicateAlignment, int governanceFlags, double contextExpansionRate)
    {
        if (governanceFlags > 0)
        {
            return ValueElevation.Negative;
        }

        if (predicateAlignment >= 0.2 && contextExpansionRate > 0.0)
        {
            return ValueElevation.Positive;
        }

        return ValueElevation.Neutral;
    }

    private static void EnforceCanonicalOrdering(IReadOnlyList<string> traceLines)
    {
        var reasoningIndex = IndexOf(traceLines, "decision-evaluate(");
        var localityIndex = IndexOf(traceLines, "locality-bind(");
        var perspectiveIndex = IndexOf(traceLines, "perspective-configure(");
        var participationIndex = IndexOf(traceLines, "participation-configure(");
        var primeIndex = IndexOf(traceLines, "prime-reflect(");
        var zedIndex = IndexOf(traceLines, "zed-listen(");
        var deltaIndex = IndexOf(traceLines, "delta-differentiate(");
        var sigmaIndex = IndexOf(traceLines, "sigma-cleave(");
        var psiIndex = IndexOf(traceLines, "psi-modulate(");
        var omegaIndex = IndexOf(traceLines, "omega-converge(");
        var thetaIndex = IndexOf(traceLines, "theta-seal(");
        var compassWorkIndex = IndexOf(traceLines, "compass-work(");
        var compassIndex = IndexOf(traceLines, "compass-update(");
        var decisionIndex = IndexOf(traceLines, "decision-branch(");
        var cleaveIndex = IndexOf(traceLines, "cleave(");
        var commitIndex = IndexOf(traceLines, "commit(");

        if (reasoningIndex < 0 ||
            localityIndex < 0 ||
            perspectiveIndex < 0 ||
            participationIndex < 0 ||
            primeIndex < 0 ||
            zedIndex < 0 ||
            deltaIndex < 0 ||
            sigmaIndex < 0 ||
            psiIndex < 0 ||
            omegaIndex < 0 ||
            thetaIndex < 0 ||
            compassWorkIndex < 0 ||
            compassIndex < 0 ||
            decisionIndex < 0 ||
            cleaveIndex < 0 ||
            commitIndex < 0)
        {
            throw new InvalidOperationException("SLI canonical cognition cycle is incomplete.");
        }

        var valid =
            reasoningIndex < localityIndex &&
            localityIndex < perspectiveIndex &&
            perspectiveIndex < participationIndex &&
            participationIndex < primeIndex &&
            primeIndex < zedIndex &&
            zedIndex < deltaIndex &&
            deltaIndex < sigmaIndex &&
            sigmaIndex < psiIndex &&
            psiIndex < omegaIndex &&
            omegaIndex < thetaIndex &&
            thetaIndex < compassWorkIndex &&
            compassWorkIndex < compassIndex &&
            compassIndex < decisionIndex &&
            decisionIndex < cleaveIndex &&
            cleaveIndex < commitIndex;

        if (!valid)
        {
            throw new InvalidOperationException("SLI canonical cognition cycle ordering violation detected.");
        }
    }

    private async Task<SliMorphologyState> ExecuteMorphologyProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "morphology-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return context.MorphologyState;
    }

    private async Task<SliPropositionState> ExecutePropositionProgramStateAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        var program = LowerProgram(symbolicProgram);
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "proposition-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            _semanticDevice);

        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        return context.PropositionState;
    }

    public SliCoreProgram LowerProgram(IReadOnlyList<string> symbolicProgram)
    {
        if (!_initialized || _boundedCompositionExpander is null)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        var parsedProgram = _parser.ParseProgram(symbolicProgram);
        var expandedProgram = _boundedCompositionExpander.ExpandProgram(parsedProgram);
        return _lowerer.LowerProgram(expandedProgram, CapabilityManifest);
    }

    private static int IndexOf(IReadOnlyList<string> traceLines, string prefix)
    {
        for (var index = 0; index < traceLines.Count; index++)
        {
            if (traceLines[index].StartsWith(prefix, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static SliHigherOrderLocalityResult CreateHigherOrderLocalityResult(SliExecutionContext context)
    {
        return SliHigherOrderLocalityResultFactory.Create(context);
    }

    private static SliBoundedRehearsalResult CreateBoundedRehearsalResult(SliExecutionContext context)
    {
        var state = context.HigherOrderLocalityState.Rehearsal;
        return new SliBoundedRehearsalResult
        {
            Locality = CreateHigherOrderLocalityResult(context),
            Rehearsal = new SliRehearsalResult
            {
                IsConfigured = state.IsConfigured,
                RehearsalHandle = state.RehearsalHandle,
                SourceLocalityHandle = state.SourceLocalityHandle,
                Mode = state.Mode,
                IdentitySeal = state.IdentitySeal,
                AdmissionStatus = state.AdmissionStatus,
                IsBindable = state.IsBindable,
                BranchSet = state.BranchSet.ToArray(),
                SubstitutionLedger = state.SubstitutionLedger
                    .Select(entry => new SliRehearsalSubstitutionResult
                    {
                        Source = entry.Source,
                        Target = entry.Target
                    })
                    .ToArray(),
                AnalogyLedger = state.AnalogyLedger
                    .Select(entry => new SliRehearsalAnalogyResult
                    {
                        Source = entry.Source,
                        Target = entry.Target
                    })
                    .ToArray(),
                Warnings = state.Warnings.ToArray(),
                Residues = CloneResidues(state.Residues)
            },
            SymbolicTrace = context.TraceLines.ToArray()
        };
    }

    private static SliBoundedWitnessResult CreateBoundedWitnessResult(SliExecutionContext context)
    {
        var state = context.HigherOrderLocalityState.Witness;
        return new SliBoundedWitnessResult
        {
            Locality = CreateHigherOrderLocalityResult(context),
            Rehearsal = CreateBoundedRehearsalResult(context).Rehearsal,
            Witness = new SliWitnessResult
            {
                IsConfigured = state.IsConfigured,
                WitnessHandle = state.WitnessHandle,
                LeftLocalityHandle = state.LeftLocalityHandle,
                RightLocalityHandle = state.RightLocalityHandle,
                PreservedInvariants = state.PreservedInvariants.ToArray(),
                DifferenceSet = state.DifferenceSet.ToArray(),
                GlueThreshold = state.GlueThreshold,
                CandidacyStatus = state.CandidacyStatus,
                Warnings = state.Warnings.ToArray(),
                Residues = CloneResidues(state.Residues)
            },
            SymbolicTrace = context.TraceLines.ToArray()
        };
    }

    private static SliBoundedTransportResult CreateBoundedTransportResult(SliExecutionContext context)
    {
        var state = context.HigherOrderLocalityState.Transport;
        return new SliBoundedTransportResult
        {
            Locality = CreateHigherOrderLocalityResult(context),
            Rehearsal = CreateBoundedRehearsalResult(context).Rehearsal,
            Witness = CreateBoundedWitnessResult(context).Witness,
            Transport = new SliTransportResult
            {
                IsConfigured = state.IsConfigured,
                TransportHandle = state.TransportHandle,
                WitnessHandle = state.WitnessHandle,
                SourceLocalityHandle = state.SourceLocalityHandle,
                TargetLocalityHandle = state.TargetLocalityHandle,
                PreservedInvariants = state.PreservedInvariants.ToArray(),
                MappedDifferences = state.MappedDifferences
                    .Select(entry => new SliTransportMappingResult
                    {
                        Source = entry.Source,
                        Target = entry.Target
                    })
                    .ToArray(),
                Warnings = state.Warnings.ToArray(),
                Residues = CloneResidues(state.Residues),
                Status = state.Status
            },
            SymbolicTrace = context.TraceLines.ToArray()
        };
    }

    private static SliBoundedAdmissibleSurfaceResult CreateAdmissibleSurfaceResult(SliExecutionContext context)
    {
        var state = context.HigherOrderLocalityState.AdmissibleSurface;
        return new SliBoundedAdmissibleSurfaceResult
        {
            Locality = CreateHigherOrderLocalityResult(context),
            Rehearsal = CreateBoundedRehearsalResult(context).Rehearsal,
            Witness = CreateBoundedWitnessResult(context).Witness,
            Transport = CreateBoundedTransportResult(context).Transport,
            Surface = new SliAdmissibleSurfaceResult
            {
                IsConfigured = state.IsConfigured,
                SurfaceHandle = state.SurfaceHandle,
                TransportHandle = state.TransportHandle,
                SourceLocalityHandle = state.SourceLocalityHandle,
                TargetLocalityHandle = state.TargetLocalityHandle,
                SurfaceClass = state.SurfaceClass,
                IdentityBearingApplicable = state.IdentityBearingApplicable,
                RevealPosture = state.RevealPosture,
                Boundary = state.Boundary,
                EvidenceSet = state.EvidenceSet.ToArray(),
                Warnings = state.Warnings.ToArray(),
                Residues = CloneResidues(state.Residues),
                Status = state.Status
            },
            SymbolicTrace = context.TraceLines.ToArray()
        };
    }

    private static SliBoundedAccountabilityPacketResult CreateAccountabilityPacketResult(SliExecutionContext context)
    {
        var state = context.HigherOrderLocalityState.AccountabilityPacket;
        return new SliBoundedAccountabilityPacketResult
        {
            Locality = CreateHigherOrderLocalityResult(context),
            Rehearsal = CreateBoundedRehearsalResult(context).Rehearsal,
            Witness = CreateBoundedWitnessResult(context).Witness,
            Transport = CreateBoundedTransportResult(context).Transport,
            Surface = CreateAdmissibleSurfaceResult(context).Surface,
            Packet = new SliAccountabilityPacketResult
            {
                IsConfigured = state.IsConfigured,
                PacketHandle = state.PacketHandle,
                SurfaceHandle = state.SurfaceHandle,
                TransportHandle = state.TransportHandle,
                WitnessHandle = state.WitnessHandle,
                SourceLocalityHandle = state.SourceLocalityHandle,
                TargetLocalityHandle = state.TargetLocalityHandle,
                PreservedInvariants = state.PreservedInvariants.ToArray(),
                SurfaceClass = state.SurfaceClass,
                IdentityBearingApplicable = state.IdentityBearingApplicable,
                RevealPosture = state.RevealPosture,
                Warnings = state.Warnings.ToArray(),
                Residues = CloneResidues(state.Residues),
                ReadinessStatus = state.ReadinessStatus
            },
            SymbolicTrace = context.TraceLines.ToArray()
        };
    }

    private static IReadOnlyList<HigherOrderLocalityResidue> CloneResidues(IEnumerable<HigherOrderLocalityResidue> residues)
    {
        return residues
            .Select(residue => new HigherOrderLocalityResidue
            {
                Kind = residue.Kind,
                Source = residue.Source,
                Detail = residue.Detail
            })
            .ToArray();
    }

    private static Task EmitGovernedTargetTelemetryAsync(
        ITelemetrySink? governanceTelemetry,
        SliTargetExecutionTelemetryEvent telemetryEvent,
        bool emitGovernedTelemetry,
        CancellationToken cancellationToken)
    {
        if (!emitGovernedTelemetry || governanceTelemetry is null)
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return governanceTelemetry.EmitAsync(telemetryEvent);
    }

    private sealed class NullEngramResolver : IEngramResolver
    {
        public static readonly NullEngramResolver Instance = new();

        public Task<EngramQueryResult> ResolveRelevantAsync(CradleTek.CognitionHost.Models.CognitionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("relevant"));
        }

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("concept"));
        }

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("cluster"));
        }

        private static EngramQueryResult Empty(string source)
        {
            return new EngramQueryResult
            {
                Source = source,
                Summaries = Array.Empty<EngramSummary>()
            };
        }
    }
}
