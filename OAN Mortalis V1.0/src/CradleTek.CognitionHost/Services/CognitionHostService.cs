using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;
using Oan.Common;

namespace CradleTek.CognitionHost.Services;

public sealed class CognitionHostService : ICognitionEngine
{
    private RuntimePathConfiguration? _paths;
    private bool _initialized;
    private readonly IManagedEgressRouter _egressRouter;

    public CognitionHostService(IManagedEgressRouter? egressRouter = null)
    {
        _egressRouter = egressRouter ?? NullEgressRouter.Instance;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return Task.CompletedTask;
        }

        _paths = RuntimePathConfiguration.FromEnvironment();
        _paths.ValidateOutsideRepository(Directory.GetCurrentDirectory());
        if (!_paths.EnsureDirectories(_egressRouter))
        {
            throw new InvalidOperationException("Managed egress router denied required cognition host path provisioning.");
        }

        _initialized = true;
        return Task.CompletedTask;
    }

    public Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Cognition host is not initialized.");
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Context);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context.TaskObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context.CMEId);

        var engramCount = request.Context.RelevantEngrams.Count;
        var confidence = Math.Clamp(0.45 + (engramCount * 0.08), 0.0, 0.98);
        var decision = engramCount == 0 ? "collect-more-context" : "proceed-with-objective";
        var engramCandidate = confidence >= 0.5;
        var activeBasin = ResolveGoldenCodeActiveBasin(request.Context.TaskObjective);
        var competingBasin = ResolveGoldenCodeCompetingBasin(activeBasin);
        var updateLocus = activeBasin == CompassDoctrineBasin.IdentityContinuity ? SliUpdateLocus.Kernel : SliUpdateLocus.Sheaf;
        var packetDirective = new SliPacketDirective(
            SliThinkingTier.Master,
            engramCandidate ? SliPacketClass.Commitment : SliPacketClass.Observation,
            engramCandidate ? SliEngramOperation.Write : SliEngramOperation.NoOp,
            updateLocus,
            SliAuthorityClass.CandidateBearing);
        var identityKernelBoundary = new IdentityKernelBoundaryReceipt(
            CmeIdentityHandle: $"cme:{request.Context.CMEId}",
            IdentityKernelHandle: $"kernel:{request.Context.CMEId}",
            ContinuityAnchorHandle: $"anchor:{request.Context.CMEId}:{activeBasin.ToString().ToLowerInvariant()}",
            KernelBound: activeBasin == CompassDoctrineBasin.IdentityContinuity,
            CandidateLocus: updateLocus);
        var validity = new SliPacketValidityReceipt(
            SyntaxOk: true,
            HexadOk: true,
            ScepOk: true,
            PolicyEligible: true,
            ReasonCode: "sli-packet-valid");
        var candidateHandle = $"zed-theta:{Guid.NewGuid():N}";
        var bridgeReview = SliBridgeContracts.CreateCandidateBridgeReview(
            bridgeStage: "zed-theta-candidate",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: $"sli-bridge://{candidateHandle}",
            thetaState: "theta-ready",
            gammaState: "gamma-ready",
            packetDirective: packetDirective,
            identityKernelBoundary: identityKernelBoundary,
            validity: validity,
            activeBasin: activeBasin,
            competingBasin: competingBasin,
            anchorState: CompassAnchorState.Weakened,
            selfTouchClass: CompassSelfTouchClass.NoTouch);
        var runtimeUseCeiling = SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();
        var zedThetaCandidate = new ZedThetaCandidateReceipt(
            CandidateHandle: candidateHandle,
            Objective: request.Context.TaskObjective,
            PrimeState: "task-objective",
            ThetaState: "theta-ready",
            GammaState: "gamma-ready",
            PacketDirective: packetDirective,
            IdentityKernelBoundary: identityKernelBoundary,
            Validity: validity,
            ActiveBasin: activeBasin,
            CompetingBasin: competingBasin,
            AnchorState: CompassAnchorState.Weakened,
            SelfTouchClass: CompassSelfTouchClass.NoTouch,
            OeCoePosture: CompassOeCoePosture.Unresolved,
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling);

        var reasoning = $"Seed LLM evaluated objective '{request.Context.TaskObjective}' using {engramCount} relevant engrams.";
        var result = new CognitionResult
        {
            Reasoning = reasoning,
            Decision = decision,
            EngramCandidate = engramCandidate,
            CleaveResidue = "[]",
            TraceId = Guid.NewGuid().ToString("D"),
            SymbolicTrace = ["1. lowmind-evaluate(seed-llm)"],
            SliTokens = [],
            DecisionBranch = decision,
            CompassState = new CognitionCompassTelemetry
            {
                IdForce = 0.35,
                SuperegoConstraint = 0.25,
                EgoStability = 0.6,
                ValueElevation = CognitionValueElevation.Neutral,
                SymbolicDepth = 1,
                BranchingFactor = 1,
                DecisionEntropy = 0.5,
                Timestamp = DateTime.UtcNow
            },
            GoldenCodeCompass = GoldenCodeCompassProjection.FromCandidateReceipt(zedThetaCandidate),
            ZedThetaCandidate = zedThetaCandidate,
            Confidence = confidence
        };

        return Task.FromResult(result);
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        _initialized = false;
        return Task.CompletedTask;
    }

    private static CompassDoctrineBasin ResolveGoldenCodeActiveBasin(string objective)
    {
        var normalized = objective.ToLowerInvariant();
        if (normalized.Contains("bounded-locality continuity", StringComparison.Ordinal) ||
            normalized.Contains("bounded locality continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.BoundedLocalityContinuity;
        }

        if (normalized.Contains("fluid continuity law", StringComparison.Ordinal) ||
            normalized.Contains("fluid continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.FluidContinuityLaw;
        }

        if (normalized.Contains("identity continuity", StringComparison.Ordinal) ||
            normalized.Contains("identity-continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.IdentityContinuity;
        }

        if (normalized.Contains("continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.GeneralContinuityDiscourse;
        }

        return CompassDoctrineBasin.Unknown;
    }

    private static CompassDoctrineBasin ResolveGoldenCodeCompetingBasin(CompassDoctrineBasin activeBasin)
    {
        return activeBasin switch
        {
            CompassDoctrineBasin.BoundedLocalityContinuity => CompassDoctrineBasin.FluidContinuityLaw,
            CompassDoctrineBasin.FluidContinuityLaw => CompassDoctrineBasin.BoundedLocalityContinuity,
            CompassDoctrineBasin.IdentityContinuity => CompassDoctrineBasin.IdentityContinuity,
            CompassDoctrineBasin.GeneralContinuityDiscourse => CompassDoctrineBasin.GeneralContinuityDiscourse,
            _ => CompassDoctrineBasin.Unknown
        };
    }
}

internal sealed class RuntimePathConfiguration
{
    private RuntimePathConfiguration(
        string runtimeRoot,
        string modelPath,
        string selfGelPath,
        string cSelfGelPath,
        string goaPath,
        string cgoaPath)
    {
        RuntimeRoot = runtimeRoot;
        ModelPath = modelPath;
        SelfGelPath = selfGelPath;
        CSelfGelPath = cSelfGelPath;
        GoaPath = goaPath;
        CgoaPath = cgoaPath;
    }

    public string RuntimeRoot { get; }
    public string ModelPath { get; }
    public string SelfGelPath { get; }
    public string CSelfGelPath { get; }
    public string GoaPath { get; }
    public string CgoaPath { get; }

    public static RuntimePathConfiguration FromEnvironment()
    {
        var runtimeRoot = ReadRequiredPath("OAN_RUNTIME_ROOT");
        var modelPath = ReadRequiredPath("OAN_MODEL_PATH");
        var selfGelPath = ReadRequiredPath("OAN_SELF_GEL");
        var cSelfGelPath = ReadRequiredPath("OAN_CSELF_GEL");
        var goaPath = ReadRequiredPath("OAN_GOA");
        var cgoaPath = ReadRequiredPath("OAN_CGOA");

        return new RuntimePathConfiguration(runtimeRoot, modelPath, selfGelPath, cSelfGelPath, goaPath, cgoaPath);
    }

    public void ValidateOutsideRepository(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        var normalizedRepositoryRoot = NormalizeDir(repositoryRoot);

        EnsureOutsideRepo(RuntimeRoot, normalizedRepositoryRoot, "OAN_RUNTIME_ROOT");
        EnsureOutsideRepo(ModelPath, normalizedRepositoryRoot, "OAN_MODEL_PATH");
        EnsureOutsideRepo(SelfGelPath, normalizedRepositoryRoot, "OAN_SELF_GEL");
        EnsureOutsideRepo(CSelfGelPath, normalizedRepositoryRoot, "OAN_CSELF_GEL");
        EnsureOutsideRepo(GoaPath, normalizedRepositoryRoot, "OAN_GOA");
        EnsureOutsideRepo(CgoaPath, normalizedRepositoryRoot, "OAN_CGOA");
    }

    public bool EnsureDirectories(IManagedEgressRouter egressRouter)
    {
        var envelope = new ManagedEgressEnvelope(
            EffectKind: SliEgressEffectKind.StructuralCreation,
            RetentionPosture: SliEgressRetentionPosture.GovernanceArtifact,
            JurisdictionClass: SliEgressJurisdictionClass.Cradle,
            IdentityFormingAllowed: true,
            TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
            AuthorityReason: "Provisioning required structural bounds for cognition host artifacts"
        );

        return egressRouter.TryRouteEgressAsync(envelope, () =>
        {
            Directory.CreateDirectory(RuntimeRoot);
            Directory.CreateDirectory(ModelPath);
            Directory.CreateDirectory(SelfGelPath);
            Directory.CreateDirectory(CSelfGelPath);
            Directory.CreateDirectory(GoaPath);
            Directory.CreateDirectory(CgoaPath);
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    private static string ReadRequiredPath(string environmentVariable)
    {
        var raw = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException($"Missing required environment variable '{environmentVariable}'.");
        }

        return Path.GetFullPath(raw);
    }

    private static void EnsureOutsideRepo(string candidatePath, string repositoryRoot, string variableName)
    {
        var normalizedCandidate = NormalizeDir(candidatePath);
        if (normalizedCandidate.StartsWith(repositoryRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Environment variable '{variableName}' points inside the repository.");
        }
    }

    private static string NormalizeDir(string path)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return $"{fullPath}{Path.DirectorySeparatorChar}";
    }
}
