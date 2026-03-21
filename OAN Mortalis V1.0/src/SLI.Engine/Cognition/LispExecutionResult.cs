using CradleTek.CognitionHost.Models;
using Oan.Common;
using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

public sealed class LispExecutionResult
{
    public required string TraceId { get; init; }
    public required string Decision { get; init; }
    public required string DecisionBranch { get; init; }
    public required string CleaveResidue { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
    public required string SymbolicTraceHash { get; init; }
    public required CognitiveCompassState CompassState { get; init; }
    public required GoldenCodeCompassProjection GoldenCodeCompass { get; init; }
    public required ZedThetaCandidateReceipt ZedThetaCandidate { get; init; }
    internal SliActualizationWebbingPacket? ActualizationPacket { get; init; }
    internal SliLiveEngramRuntimePacket? LiveRuntimePacket { get; init; }
    internal SliLiveEngramRuntimeRun? LiveRuntimeRun { get; init; }
}
