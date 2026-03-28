using System.Collections.Generic;
using System.Text.Json.Serialization;
using SLI.Engine.Cognition;
using Oan.Common;

namespace SLI.Engine.Runtime;

/// <summary>
/// Dictates the governed longevity and purpose of an execution snapshot.
/// </summary>
internal enum SliSnapshotRetentionPosture
{
    DebugOnly,
    CIArtifact,
    GovernanceArtifact
}

/// <summary>
/// Immutable snapshot of the SLI execution state for inspection and replay.
/// Captures projected runtime data and actualization packets without exposing live dependencies.
/// </summary>
internal sealed record SliExecutionSnapshot(
    [property: JsonPropertyName("retentionPosture")] SliSnapshotRetentionPosture RetentionPosture,
    [property: JsonPropertyName("isNonIdentityFormingMemory")] bool IsNonIdentityFormingMemory,
    [property: JsonPropertyName("traceId")] string TraceId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("decisionBranch")] string DecisionBranch,
    [property: JsonPropertyName("cleaveResidue")] string CleaveResidue,
    [property: JsonPropertyName("traceLines")] IReadOnlyList<string> TraceLines,
    [property: JsonPropertyName("candidateBranches")] IReadOnlyList<string> CandidateBranches,
    [property: JsonPropertyName("prunedBranches")] IReadOnlyList<string> PrunedBranches,
    [property: JsonPropertyName("localityShards")] IReadOnlyList<LocalityShardSnapshot> LocalityShards,
    [property: JsonPropertyName("localityRelationEvents")] IReadOnlyList<LocalityRelationEventSnapshot> LocalityRelationEvents,
    [property: JsonPropertyName("localityObstructions")] IReadOnlyList<LocalityObstructionSnapshot> LocalityObstructions,
    [property: JsonPropertyName("actualizationWebbingEvents")] IReadOnlyList<ActualizationWebbingEventSnapshot> ActualizationWebbingEvents,
    [property: JsonPropertyName("actualizationPacket")] ActualizationPacketSnapshot? ActualizationPacket,
    [property: JsonPropertyName("zedThetaCandidate")] ZedThetaCandidateSnapshot ZedThetaCandidate,
    [property: JsonPropertyName("liveRuntimeRun")] LiveRuntimeRunSnapshot? LiveRuntimeRun
);

// Supporting DTOs

internal sealed record LocalityShardSnapshot(
    string ShardId,
    SliLocalityShardKind ShardKind,
    string LocalityHandle,
    string ParentExecutionId,
    string RootAnchor,
    string SymbolBoundaryRef,
    SliLocalityShardLifecycleState LifecycleState
);

internal sealed record LocalityRelationEventSnapshot(
    string RelationId,
    string SourceShardId,
    string TargetShardId,
    SliLocalityRelationKind RelationKind,
    SliLocalityTelemetryCarryPolicy TelemetryCarryPolicy,
    bool JoinEligible,
    SliLocalityRelationOutcomeKind Outcome,
    string ReasonCode,
    string CycleMarker
);

internal sealed record LocalityObstructionSnapshot(
    string ObstructionId,
    string SourceShardId,
    string TargetShardId,
    SliLocalityRelationKind AttemptedRelation,
    string ViolatedCondition,
    bool RetryLawful,
    bool HostFallbackOccurred,
    bool EscalationRequired,
    string CycleMarker
);

internal sealed record ActualizationWebbingEventSnapshot(
    SliActualizationStageKind Stage,
    string Detail,
    string ShardId,
    string LocalityHandle,
    string CycleMarker
);

internal sealed record ActualizationPacketSnapshot(
    string ActualizationHandle,
    string ExecutionId,
    string Objective,
    SliActualizationClaimClass ClaimClass,
    SliActualizationContradictionClass ContradictionClass,
    SliActualizationValidationRoute ValidationRoute,
    SliActualizationDisposition Disposition,
    bool SelfValidationRequired,
    bool CandidateEngramBearing,
    bool AutobiographicalBearing,
    SliLocalityRelationOutcomeKind? ReductionOutcome,
    string ReductionReason,
    IReadOnlyList<string> ResidueSet
);

internal sealed record ZedThetaCandidateSnapshot(
    string CandidateHandle,
    string Objective,
    string PrimeState,
    string ThetaState,
    string GammaState,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassAnchorState AnchorState,
    CompassSelfTouchClass SelfTouchClass,
    CompassOeCoePosture OeCoePosture,
    BridgeReceiptSnapshot? BridgeReview,
    RuntimeCeilingSnapshot? RuntimeUseCeiling
);

internal sealed record BridgeReceiptSnapshot(
    SliBridgeOutcomeKind OutcomeKind,
    SliBridgeThresholdClass ThresholdClass,
    string BridgeWitnessHandle,
    string ReasonCode
);

internal sealed record RuntimeCeilingSnapshot(bool CandidateOnly);

internal sealed record LiveRuntimeRunSnapshot(
    string ExecutionId,
    bool ShardModeEnabled,
    string PrimaryShardId,
    SliLocalityRelationOutcomeKind ReductionOutcome,
    string ReductionReason
);
