using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SLI.Engine.Cognition;
using Oan.Common;

namespace SLI.Engine.Runtime;

/// <summary>
/// Factory for creating <see cref="SliExecutionSnapshot"/> instances from runtime state.
/// </summary>
internal static class SliExecutionSnapshotFactory
{
    /// <summary>
    /// Serialization options to ensure deterministic JSON representation of snapshots.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Creates a snapshot suitable for inspection/replay from the given execution context and result.
    /// </summary>
    /// <param name="context">The current execution context.</param>
    /// <param name="result">The cognition result produced by the engine.</param>
    /// <param name="posture">The artifact's intended governed retention lifecycle.</param>
    /// <returns>A deterministic <see cref="SliExecutionSnapshot"/>.</returns>
    public static SliExecutionSnapshot CreateForCognition(
        SliExecutionContext context, 
        LispExecutionResult result,
        SliSnapshotRetentionPosture posture = SliSnapshotRetentionPosture.DebugOnly)
    {
        // 1. Shards
        var shards = new List<LocalityShardSnapshot>();
        foreach (var s in context.LocalityShards)
        {
            shards.Add(new LocalityShardSnapshot(
                s.ShardId, s.ShardKind, s.LocalityHandle, s.ParentExecutionId, s.RootAnchor, s.SymbolBoundaryRef, s.LifecycleState));
        }

        // 2. Relations
        var relations = new List<LocalityRelationEventSnapshot>();
        foreach (var r in context.LocalityRelationEvents)
        {
            relations.Add(new LocalityRelationEventSnapshot(
                r.RelationId, r.SourceShardId, r.TargetShardId, r.RelationKind, r.TelemetryCarryPolicy, r.JoinEligible, r.Outcome, r.ReasonCode, r.CycleMarker));
        }

        // 3. Obstructions
        var obstructions = new List<LocalityObstructionSnapshot>();
        foreach (var o in context.LocalityObstructions)
        {
            obstructions.Add(new LocalityObstructionSnapshot(
                o.ObstructionId, o.SourceShardId, o.TargetShardId, o.AttemptedRelation, o.ViolatedCondition, o.RetryLawful, o.HostFallbackOccurred, o.EscalationRequired, o.CycleMarker));
        }

        // 4. Webbing Events
        var webbing = new List<ActualizationWebbingEventSnapshot>();
        foreach (var w in context.ActualizationWebbingEvents)
        {
            webbing.Add(new ActualizationWebbingEventSnapshot(
                w.Stage, w.Detail, w.ShardId, w.LocalityHandle, w.CycleMarker));
        }

        // 5. Actualization Packet
        ActualizationPacketSnapshot? packetSnap = null;
        if (result.ActualizationPacket != null)
        {
            var p = result.ActualizationPacket;
            packetSnap = new ActualizationPacketSnapshot(
                p.ActualizationHandle, p.ExecutionId, p.Objective, p.ClaimClass, p.ContradictionClass, p.ValidationRoute, p.Disposition, p.SelfValidationRequired, p.CandidateEngramBearing, p.AutobiographicalBearing, p.ReductionOutcome, p.ReductionReason, p.ResidueSet.ToList());
        }

        // 6. ZedThetaCandidate
        var c = result.ZedThetaCandidate;
        var bridgeSnap = c.BridgeReview != null
            ? new BridgeReceiptSnapshot(c.BridgeReview.OutcomeKind, c.BridgeReview.ThresholdClass, c.BridgeReview.BridgeWitnessHandle ?? string.Empty, c.BridgeReview.ReasonCode ?? string.Empty)
            : null;
        var ceilingSnap = c.RuntimeUseCeiling != null
            ? new RuntimeCeilingSnapshot(c.RuntimeUseCeiling.CandidateOnly)
            : null;

        var zedSnap = new ZedThetaCandidateSnapshot(
            c.CandidateHandle, c.Objective, c.PrimeState, c.ThetaState, c.GammaState, c.ActiveBasin, c.CompetingBasin, c.AnchorState, c.SelfTouchClass, c.OeCoePosture, bridgeSnap, ceilingSnap);

        // 7. LiveRuntimeRun
        LiveRuntimeRunSnapshot? runSnap = null;
        if (result.LiveRuntimeRun != null)
        {
            var r = result.LiveRuntimeRun;
            runSnap = new LiveRuntimeRunSnapshot(
                r.ExecutionId, r.ShardModeEnabled, r.PrimaryShardId, r.ReductionOutcome, r.ReductionReason);
        }

        return new SliExecutionSnapshot(
            RetentionPosture: posture,
            IsNonIdentityFormingMemory: true,
            TraceId: result.TraceId,
            Decision: result.Decision,
            DecisionBranch: result.DecisionBranch,
            CleaveResidue: result.CleaveResidue,
            TraceLines: context.TraceLines.ToList(),
            CandidateBranches: context.CandidateBranches.ToList(),
            PrunedBranches: context.PrunedBranches.ToList(),
            LocalityShards: shards,
            LocalityRelationEvents: relations,
            LocalityObstructions: obstructions,
            ActualizationWebbingEvents: webbing,
            ActualizationPacket: packetSnap,
            ZedThetaCandidate: zedSnap,
            LiveRuntimeRun: runSnap
        );
    }

    /// <summary>
    /// Serializes a snapshot string deterministically into JSON.
    /// </summary>
    public static string Serialize(SliExecutionSnapshot snapshot)
    {
        return JsonSerializer.Serialize(snapshot, DefaultOptions);
    }
}
