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
    }

    public ContextFrame Frame { get; }
    public IEngramResolver Resolver { get; }
    public ISoulFrameSemanticDevice SemanticDevice { get; }
    public SoulFrameInferenceConstraints OpalConstraints { get; }
    public IReadOnlyList<EngramReference> ActiveEngrams => _activeEngrams;
    public List<string> TraceLines { get; } = [];
    public List<string> CandidateBranches { get; } = [];
    public List<string> PrunedBranches { get; } = [];
    public SliExecutionGraph ExecutionGraph { get; } = new();
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
