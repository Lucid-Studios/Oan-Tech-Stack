using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using SLI.Engine.Morphology;
using SLI.Engine.Models;
using SoulFrame.Host;
using Oan.Common;

namespace SLI.Engine.Runtime;

public sealed class SliExecutionContext
{
    private readonly List<EngramReference> _activeEngrams;
    private readonly Dictionary<string, SliLocalityShardRecord> _localityShards = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _shardSymbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _shardTraceLines = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SliLocalityRelationEvent> _localityRelationEvents = [];
    private readonly List<SliLocalityObstructionRecord> _localityObstructions = [];
    private readonly List<SliActualizationWebbingEvent> _actualizationWebbingEvents = [];

    public SliExecutionContext(
        ContextFrame frame,
        IEngramResolver resolver,
        ISoulFrameSemanticDevice? semanticDevice = null)
    {
        Frame = frame;
        Resolver = resolver;
        SemanticDevice = semanticDevice ?? NullSoulFrameSemanticDevice.Instance;
        OpalConstraints = BuildConstraints(frame.TaskObjective);
        _activeEngrams = frame.Engrams.ToList();
        ExecutionId = SliCompassLocalityShards.ResolveExecutionId(frame);
    }

    public ContextFrame Frame { get; }
    public IEngramResolver Resolver { get; }
    public ISoulFrameSemanticDevice SemanticDevice { get; }
    public SoulFrameInferenceConstraints OpalConstraints { get; }
    public string ExecutionId { get; }
    public IReadOnlyList<EngramReference> ActiveEngrams => _activeEngrams;
    public List<string> TraceLines { get; } = [];
    public List<string> CandidateBranches { get; } = [];
    public List<string> PrunedBranches { get; } = [];
    public SliExecutionGraph ExecutionGraph { get; } = new();
    internal bool ShardModeEnabled { get; private set; }
    internal string? PrimaryShardId { get; private set; }
    internal string? CurrentShardId { get; private set; }
    internal IReadOnlyCollection<SliLocalityShardRecord> LocalityShards => _localityShards.Values.ToArray();
    internal IReadOnlyList<SliLocalityRelationEvent> LocalityRelationEvents => _localityRelationEvents;
    internal IReadOnlyList<SliLocalityObstructionRecord> LocalityObstructions => _localityObstructions;
    internal IReadOnlyList<SliActualizationWebbingEvent> ActualizationWebbingEvents => _actualizationWebbingEvents;
    internal SliMorphologyState MorphologyState { get; } = new();
    internal SliPropositionState PropositionState { get; } = new();
    internal SliHigherOrderLocalityState HigherOrderLocalityState { get; } = new();
    internal SliGoldenCodeState GoldenCodeState { get; } = new();
    internal SoulFrameInferenceResponse? LastClassifyResponse { get; set; }
    public string FinalDecision { get; set; } = "defer";

    public void AddTrace(string trace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trace);
        TraceLines.Add(trace);

        if (ShardModeEnabled &&
            CurrentShardId is not null &&
            _shardTraceLines.TryGetValue(CurrentShardId, out var shardTrace))
        {
            shardTrace.Add(trace);
        }
    }

    public void AddResolvedEngrams(IEnumerable<EngramSummary> summaries)
    {
        foreach (var summary in summaries)
        {
            _activeEngrams.Add(new EngramReference
            {
                EngramId = summary.EngramId,
                ConceptTag = summary.ConceptTag,
                SummaryText = summary.SummaryText,
                DecisionSpline = summary.DecisionSpline,
                ConfidenceWeight = summary.ConfidenceWeight
            });
        }
    }

    internal void EnableCompassShardMode()
    {
        if (ShardModeEnabled)
        {
            return;
        }

        ShardModeEnabled = true;
        var rootAnchor = SliCompassLocalityShards.ResolveRootAnchor(Frame);
        CreateShard(
            SliCompassLocalityShards.ActingShardId,
            SliLocalityShardKind.Acting,
            rootAnchor,
            SliCompassLocalityShards.ActingBoundaryRef);
        CreateShard(
            SliCompassLocalityShards.WitnessingShardId,
            SliLocalityShardKind.Witnessing,
            rootAnchor,
            SliCompassLocalityShards.WitnessingBoundaryRef);
        CreateShard(
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliLocalityShardKind.AdjacentIngestion,
            rootAnchor,
            SliCompassLocalityShards.AdjacentIngestionBoundaryRef);

        PrimaryShardId = SliCompassLocalityShards.ActingShardId;
        CurrentShardId = PrimaryShardId;
        SliLocalityRelationEvaluator.RecordAdjacency(this);
        EnterShard(PrimaryShardId);
    }

    internal void EnterShard(string? shardId)
    {
        if (!ShardModeEnabled || string.IsNullOrWhiteSpace(shardId))
        {
            return;
        }

        if (!_localityShards.TryGetValue(shardId, out var shard))
        {
            return;
        }

        CurrentShardId = shardId;
        shard.LifecycleState = SliLocalityShardLifecycleState.Active;
    }

    internal bool TryGetShard(string shardId, out SliLocalityShardRecord shard)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shardId);
        return _localityShards.TryGetValue(shardId, out shard!);
    }

    internal void SetShardLifecycleState(string shardId, SliLocalityShardLifecycleState state)
    {
        if (_localityShards.TryGetValue(shardId, out var shard))
        {
            shard.LifecycleState = state;
        }
    }

    internal void ExportShardSymbol(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!ShardModeEnabled || CurrentShardId is null)
        {
            return;
        }

        ExportShardSymbol(CurrentShardId, key, value);
    }

    internal void ExportShardSymbol(string shardId, string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!_shardSymbols.TryGetValue(shardId, out var symbols))
        {
            symbols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _shardSymbols[shardId] = symbols;
        }

        symbols[key] = value;
    }

    internal bool TryImportShardSymbol(
        string sourceShardId,
        string targetShardId,
        string key,
        string alias,
        out string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceShardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetShardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        value = string.Empty;
        if (!IsImportAllowed(sourceShardId, targetShardId))
        {
            return false;
        }

        if (!_shardSymbols.TryGetValue(sourceShardId, out var sourceSymbols) ||
            !sourceSymbols.TryGetValue(key, out var importedValue) ||
            importedValue is null)
        {
            return false;
        }

        value = importedValue;
        ExportShardSymbol(targetShardId, alias, value);
        return true;
    }

    internal bool TryGetShardSymbol(string shardId, string key, out string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_shardSymbols.TryGetValue(shardId, out var symbols) &&
            symbols.TryGetValue(key, out var shardValue) &&
            shardValue is not null)
        {
            value = shardValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    internal IReadOnlyList<string> GetShardTraceLines(string shardId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shardId);
        return _shardTraceLines.TryGetValue(shardId, out var trace)
            ? trace.ToArray()
            : Array.Empty<string>();
    }

    internal void RecordRelation(SliLocalityRelationEvent relationEvent)
    {
        ArgumentNullException.ThrowIfNull(relationEvent);
        _localityRelationEvents.Add(relationEvent);
    }

    internal void RecordObstruction(SliLocalityObstructionRecord obstructionRecord)
    {
        ArgumentNullException.ThrowIfNull(obstructionRecord);
        _localityObstructions.Add(obstructionRecord);
    }

    internal void RecordActualizationStage(
        SliActualizationStageKind stage,
        string detail,
        string cycleMarker)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        ArgumentException.ThrowIfNullOrWhiteSpace(cycleMarker);

        var shardId = CurrentShardId ??
            (ShardModeEnabled
                ? PrimaryShardId ?? SliCompassLocalityShards.ActingShardId
                : "serial");
        var localityHandle = ResolveCurrentLocalityHandle(shardId);
        _actualizationWebbingEvents.Add(new SliActualizationWebbingEvent(
            Stage: stage,
            Detail: detail,
            ShardId: shardId,
            LocalityHandle: localityHandle,
            CycleMarker: cycleMarker));
    }

    internal void FinalizeCompassShardRun()
    {
        if (!ShardModeEnabled)
        {
            return;
        }

        SliLocalityRelationEvaluator.FinalizeDeferredRelations(this);
    }

    private void CreateShard(
        string shardId,
        SliLocalityShardKind shardKind,
        string rootAnchor,
        string boundaryRef)
    {
        var localityHandle = SliCompassLocalityShards.ResolveLocalityHandle(rootAnchor, shardId);
        _localityShards[shardId] = new SliLocalityShardRecord
        {
            ShardId = shardId,
            ShardKind = shardKind,
            LocalityHandle = localityHandle,
            ParentExecutionId = ExecutionId,
            RootAnchor = rootAnchor,
            SymbolBoundaryRef = boundaryRef,
            LifecycleState = SliLocalityShardLifecycleState.Initialized
        };
        _shardSymbols[shardId] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _shardTraceLines[shardId] = [];
    }

    private static bool IsImportAllowed(string sourceShardId, string targetShardId)
    {
        if (string.Equals(sourceShardId, targetShardId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return (string.Equals(sourceShardId, SliCompassLocalityShards.ActingShardId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(targetShardId, SliCompassLocalityShards.WitnessingShardId, StringComparison.OrdinalIgnoreCase)) ||
               (string.Equals(sourceShardId, SliCompassLocalityShards.WitnessingShardId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(targetShardId, SliCompassLocalityShards.AdjacentIngestionShardId, StringComparison.OrdinalIgnoreCase));
    }

    private string ResolveCurrentLocalityHandle(string shardId)
    {
        if (ShardModeEnabled &&
            _localityShards.TryGetValue(shardId, out var shard) &&
            !string.IsNullOrWhiteSpace(shard.LocalityHandle))
        {
            return shard.LocalityHandle;
        }

        return string.IsNullOrWhiteSpace(HigherOrderLocalityState.LocalityHandle)
            ? $"locality:{Frame.CMEId}:{Frame.ContextId:D}"
            : HigherOrderLocalityState.LocalityHandle;
    }

    private static SoulFrameInferenceConstraints BuildConstraints(string objective)
    {
        var domain = objective.Contains("equation", StringComparison.OrdinalIgnoreCase) ||
                     objective.Contains("arithmetic", StringComparison.OrdinalIgnoreCase)
            ? "arithmetic"
            : "general";

        return new SoulFrameInferenceConstraints
        {
            Domain = domain,
            DriftLimit = 0.02,
            MaxTokens = 128
        };
    }
}

internal sealed class SliGoldenCodeState
{
    public string PrimeState { get; set; } = "task-objective";
    public string ThetaState { get; set; } = "theta-pending";
    public string GammaState { get; set; } = "gamma-pending";
    public CompassDoctrineBasin ActiveBasin { get; set; } = CompassDoctrineBasin.Unknown;
    public CompassDoctrineBasin CompetingBasin { get; set; } = CompassDoctrineBasin.Unknown;
    public CompassAnchorState AnchorState { get; set; } = CompassAnchorState.Unknown;
    public CompassSelfTouchClass SelfTouchClass { get; set; } = CompassSelfTouchClass.NoTouch;
    public CompassOeCoePosture OeCoePosture { get; set; } = CompassOeCoePosture.Unresolved;
    public SliPacketDirective PacketDirective { get; set; } = new(
        SliThinkingTier.Master,
        SliPacketClass.Observation,
        SliEngramOperation.NoOp,
        SliUpdateLocus.Sheaf,
        SliAuthorityClass.CandidateBearing);
    public IdentityKernelBoundaryReceipt IdentityKernelBoundary { get; set; } = new(
        CmeIdentityHandle: "cme:unknown",
        IdentityKernelHandle: "kernel:unknown",
        ContinuityAnchorHandle: "anchor:unknown",
        KernelBound: false,
        CandidateLocus: SliUpdateLocus.Sheaf);
    public SliPacketValidityReceipt PacketValidity { get; set; } = new(
        SyntaxOk: true,
        HexadOk: true,
        ScepOk: true,
        PolicyEligible: true,
        ReasonCode: "sli-packet-valid");
    public bool IsProjected { get; set; }
}
