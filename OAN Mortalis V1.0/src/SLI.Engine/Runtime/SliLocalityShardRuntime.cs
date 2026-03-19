using CradleTek.Memory.Models;
using SLI.Engine.Models;

namespace SLI.Engine.Runtime;

internal enum SliLocalityShardKind
{
    Acting = 0,
    Witnessing = 1,
    AdjacentIngestion = 2
}

internal enum SliLocalityShardLifecycleState
{
    Initialized = 0,
    Bound = 1,
    Active = 2,
    WaitingForJoin = 3,
    Joined = 4,
    Obstructed = 5,
    Deferred = 6
}

internal enum SliLocalityRelationKind
{
    WitnessOf = 0,
    IngestsFrom = 1,
    AdjacentTo = 2
}

internal enum SliLocalityRelationOutcomeKind
{
    Joined = 0,
    Refused = 1,
    Obstructed = 2,
    Deferred = 3
}

internal enum SliLocalityTelemetryCarryPolicy
{
    TraceOnly = 0,
    TraceAndWitness = 1,
    TraceWitnessAndResidue = 2
}

internal sealed class SliLocalityShardRecord
{
    public required string ShardId { get; init; }
    public required SliLocalityShardKind ShardKind { get; init; }
    public required string LocalityHandle { get; init; }
    public required string ParentExecutionId { get; init; }
    public required string RootAnchor { get; init; }
    public required string SymbolBoundaryRef { get; set; }
    public SliLocalityShardLifecycleState LifecycleState { get; set; }
}

internal sealed record SliLocalityRelationEvent(
    string RelationId,
    string SourceShardId,
    string TargetShardId,
    SliLocalityRelationKind RelationKind,
    SliLocalityTelemetryCarryPolicy TelemetryCarryPolicy,
    bool JoinEligible,
    SliLocalityRelationOutcomeKind Outcome,
    string ReasonCode,
    string CycleMarker);

internal sealed record SliLocalityObstructionRecord(
    string ObstructionId,
    string SourceShardId,
    string TargetShardId,
    SliLocalityRelationKind AttemptedRelation,
    string ViolatedCondition,
    bool RetryLawful,
    bool HostFallbackOccurred,
    bool EscalationRequired,
    string CycleMarker);

internal static class SliCompassLocalityShards
{
    public const string ActingShardId = "acting";
    public const string WitnessingShardId = "witness";
    public const string AdjacentIngestionShardId = "adjacent-ingestion";

    public const string ActingBoundaryRef = "compass-acting-surface";
    public const string WitnessingBoundaryRef = "compass-witness-surface";
    public const string AdjacentIngestionBoundaryRef = "compass-adjacent-ingestion-surface";

    public const string GoldenCodeBloomTelemetryKey = "golden-code-bloom";
    public const string ThetaSealTelemetryKey = "theta-seal";
    public const string CompassWorkTelemetryKey = "compass-work";

    public static bool IsPilotEligible(SliCoreProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        var opcodes = program.Instructions
            .Select(instruction => instruction.Opcode)
            .ToArray();

        return ContainsOrderedSequence(
            opcodes,
            "omega-converge",
            "theta-seal",
            "compass-work",
            "gamma-yield",
            "compass-update");
    }

    public static string ResolveExecutionId(ContextFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return $"execution:{frame.CMEId}:{frame.ContextId:D}";
    }

    public static string ResolveRootAnchor(ContextFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return $"root-anchor:{frame.CMEId}:{frame.ContextId:D}";
    }

    public static string ResolveLocalityHandle(string rootAnchor, string shardId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootAnchor);
        ArgumentException.ThrowIfNullOrWhiteSpace(shardId);
        return $"locality:{rootAnchor}:{shardId}";
    }

    private static bool ContainsOrderedSequence(
        IReadOnlyList<string> opcodes,
        params string[] required)
    {
        if (required.Length == 0)
        {
            return true;
        }

        var requiredIndex = 0;
        for (var index = 0; index < opcodes.Count && requiredIndex < required.Length; index++)
        {
            if (string.Equals(opcodes[index], required[requiredIndex], StringComparison.OrdinalIgnoreCase))
            {
                requiredIndex++;
            }
        }

        return requiredIndex == required.Length;
    }
}

internal static class SliLocalityRelationEvaluator
{
    public static void RecordAdjacency(SliExecutionContext context, string cycleMarker = "compass-shard-init")
    {
        ArgumentNullException.ThrowIfNull(context);

        EvaluateRelation(
            context,
            SliCompassLocalityShards.ActingShardId,
            SliCompassLocalityShards.WitnessingShardId,
            SliLocalityRelationKind.AdjacentTo,
            SliLocalityTelemetryCarryPolicy.TraceOnly,
            cycleMarker);
        EvaluateRelation(
            context,
            SliCompassLocalityShards.WitnessingShardId,
            SliCompassLocalityShards.ActingShardId,
            SliLocalityRelationKind.AdjacentTo,
            SliLocalityTelemetryCarryPolicy.TraceOnly,
            cycleMarker);
        EvaluateRelation(
            context,
            SliCompassLocalityShards.WitnessingShardId,
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliLocalityRelationKind.AdjacentTo,
            SliLocalityTelemetryCarryPolicy.TraceOnly,
            cycleMarker);
        EvaluateRelation(
            context,
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliCompassLocalityShards.WitnessingShardId,
            SliLocalityRelationKind.AdjacentTo,
            SliLocalityTelemetryCarryPolicy.TraceOnly,
            cycleMarker);
    }

    public static SliLocalityRelationEvent EvaluateWitnessOf(SliExecutionContext context, string cycleMarker)
    {
        return EvaluateRelation(
            context,
            SliCompassLocalityShards.WitnessingShardId,
            SliCompassLocalityShards.ActingShardId,
            SliLocalityRelationKind.WitnessOf,
            SliLocalityTelemetryCarryPolicy.TraceAndWitness,
            cycleMarker);
    }

    public static SliLocalityRelationEvent EvaluateIngestsFrom(SliExecutionContext context, string cycleMarker)
    {
        return EvaluateRelation(
            context,
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliCompassLocalityShards.WitnessingShardId,
            SliLocalityRelationKind.IngestsFrom,
            SliLocalityTelemetryCarryPolicy.TraceWitnessAndResidue,
            cycleMarker);
    }

    public static SliLocalityRelationEvent EvaluateRelation(
        SliExecutionContext context,
        string sourceShardId,
        string targetShardId,
        SliLocalityRelationKind relationKind,
        SliLocalityTelemetryCarryPolicy carryPolicy,
        string cycleMarker)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceShardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetShardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cycleMarker);

        if (!context.TryGetShard(sourceShardId, out var source) ||
            !context.TryGetShard(targetShardId, out var target))
        {
            return RecordFailure(
                context,
                sourceShardId,
                targetShardId,
                relationKind,
                carryPolicy,
                SliLocalityRelationOutcomeKind.Refused,
                reasonCode: "missing-shard",
                violatedCondition: "missing-shard",
                retryLawful: false,
                cycleMarker);
        }

        if (!IsValidRelationPair(relationKind, source, target))
        {
            return RecordFailure(
                context,
                sourceShardId,
                targetShardId,
                relationKind,
                carryPolicy,
                SliLocalityRelationOutcomeKind.Refused,
                reasonCode: "invalid-relation-pair",
                violatedCondition: "invalid-relation-pair",
                retryLawful: false,
                cycleMarker);
        }

        if (!HasSharedContinuity(source, target))
        {
            return RecordFailure(
                context,
                sourceShardId,
                targetShardId,
                relationKind,
                carryPolicy,
                SliLocalityRelationOutcomeKind.Refused,
                reasonCode: "continuity-mismatch",
                violatedCondition: "continuity-mismatch",
                retryLawful: false,
                cycleMarker);
        }

        source.LifecycleState = SliLocalityShardLifecycleState.WaitingForJoin;

        if (relationKind == SliLocalityRelationKind.AdjacentTo)
        {
            source.LifecycleState = SliLocalityShardLifecycleState.Bound;
            if (target.LifecycleState == SliLocalityShardLifecycleState.Initialized)
            {
                target.LifecycleState = SliLocalityShardLifecycleState.Bound;
            }

            var adjacencyEvent = CreateEvent(
                sourceShardId,
                targetShardId,
                relationKind,
                carryPolicy,
                joinEligible: true,
                SliLocalityRelationOutcomeKind.Joined,
                "adjacent-joined",
                cycleMarker);
            context.RecordRelation(adjacencyEvent);
            return adjacencyEvent;
        }

        if (relationKind == SliLocalityRelationKind.WitnessOf)
        {
            return EvaluateWitnessOfInternal(context, source, target, carryPolicy, cycleMarker);
        }

        return EvaluateIngestsFromInternal(context, source, target, carryPolicy, cycleMarker);
    }

    public static void FinalizeDeferredRelations(SliExecutionContext context, string cycleMarker = "compass-shard-finalize")
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.ShardModeEnabled)
        {
            return;
        }

        if (context.LocalityRelationEvents.Any(evt => evt.RelationKind == SliLocalityRelationKind.IngestsFrom))
        {
            return;
        }

        var witnessOutcome = context.LocalityRelationEvents
            .LastOrDefault(evt => evt.RelationKind == SliLocalityRelationKind.WitnessOf);
        var reasonCode = witnessOutcome switch
        {
            null => "downstream-phase-not-reached",
            { Outcome: SliLocalityRelationOutcomeKind.Joined } => "downstream-phase-not-reached",
            { Outcome: SliLocalityRelationOutcomeKind.Refused } => "upstream-refused:witness-of",
            { Outcome: SliLocalityRelationOutcomeKind.Obstructed } => "upstream-obstructed:witness-of",
            _ => "upstream-deferred:witness-of"
        };

        var deferredEvent = CreateEvent(
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliCompassLocalityShards.WitnessingShardId,
            SliLocalityRelationKind.IngestsFrom,
            SliLocalityTelemetryCarryPolicy.TraceWitnessAndResidue,
            joinEligible: false,
            SliLocalityRelationOutcomeKind.Deferred,
            reasonCode,
            cycleMarker);
        context.RecordRelation(deferredEvent);
        context.SetShardLifecycleState(
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliLocalityShardLifecycleState.Deferred);
    }

    private static SliLocalityRelationEvent EvaluateWitnessOfInternal(
        SliExecutionContext context,
        SliLocalityShardRecord source,
        SliLocalityShardRecord target,
        SliLocalityTelemetryCarryPolicy carryPolicy,
        string cycleMarker)
    {
        var missingKeys = new List<string>();
        if (!context.TryImportShardSymbol(
                target.ShardId,
                source.ShardId,
                SliCompassLocalityShards.GoldenCodeBloomTelemetryKey,
                SliCompassLocalityShards.GoldenCodeBloomTelemetryKey,
                out _))
        {
            missingKeys.Add(SliCompassLocalityShards.GoldenCodeBloomTelemetryKey);
        }

        if (!context.TryImportShardSymbol(
                target.ShardId,
                source.ShardId,
                SliCompassLocalityShards.ThetaSealTelemetryKey,
                SliCompassLocalityShards.ThetaSealTelemetryKey,
                out _))
        {
            missingKeys.Add(SliCompassLocalityShards.ThetaSealTelemetryKey);
        }

        if (missingKeys.Count > 0)
        {
            return RecordFailure(
                context,
                source.ShardId,
                target.ShardId,
                SliLocalityRelationKind.WitnessOf,
                carryPolicy,
                SliLocalityRelationOutcomeKind.Obstructed,
                reasonCode: $"missing-source-telemetry:{string.Join(",", missingKeys)}",
                violatedCondition: "missing-source-telemetry",
                retryLawful: true,
                cycleMarker);
        }

        source.LifecycleState = SliLocalityShardLifecycleState.Joined;
        var relationEvent = CreateEvent(
            source.ShardId,
            target.ShardId,
            SliLocalityRelationKind.WitnessOf,
            carryPolicy,
            joinEligible: true,
            SliLocalityRelationOutcomeKind.Joined,
            "witness-joined",
            cycleMarker);
        context.RecordRelation(relationEvent);
        return relationEvent;
    }

    private static SliLocalityRelationEvent EvaluateIngestsFromInternal(
        SliExecutionContext context,
        SliLocalityShardRecord source,
        SliLocalityShardRecord target,
        SliLocalityTelemetryCarryPolicy carryPolicy,
        string cycleMarker)
    {
        var witnessOutcome = context.LocalityRelationEvents
            .LastOrDefault(evt => evt.RelationKind == SliLocalityRelationKind.WitnessOf);
        if (witnessOutcome is null)
        {
            var deferred = CreateEvent(
                source.ShardId,
                target.ShardId,
                SliLocalityRelationKind.IngestsFrom,
                carryPolicy,
                joinEligible: false,
                SliLocalityRelationOutcomeKind.Deferred,
                "upstream-phase-not-reached:witness-of",
                cycleMarker);
            context.RecordRelation(deferred);
            source.LifecycleState = SliLocalityShardLifecycleState.Deferred;
            return deferred;
        }

        if (witnessOutcome.Outcome == SliLocalityRelationOutcomeKind.Refused ||
            witnessOutcome.Outcome == SliLocalityRelationOutcomeKind.Obstructed)
        {
            var reasonCode = witnessOutcome.Outcome == SliLocalityRelationOutcomeKind.Refused
                ? "upstream-refused:witness-of"
                : "upstream-obstructed:witness-of";
            var deferred = CreateEvent(
                source.ShardId,
                target.ShardId,
                SliLocalityRelationKind.IngestsFrom,
                carryPolicy,
                joinEligible: false,
                SliLocalityRelationOutcomeKind.Deferred,
                reasonCode,
                cycleMarker);
            context.RecordRelation(deferred);
            source.LifecycleState = SliLocalityShardLifecycleState.Deferred;
            return deferred;
        }

        if (!context.TryImportShardSymbol(
                target.ShardId,
                source.ShardId,
                SliCompassLocalityShards.CompassWorkTelemetryKey,
                SliCompassLocalityShards.CompassWorkTelemetryKey,
                out _))
        {
            return RecordFailure(
                context,
                source.ShardId,
                target.ShardId,
                SliLocalityRelationKind.IngestsFrom,
                carryPolicy,
                SliLocalityRelationOutcomeKind.Obstructed,
                reasonCode: "missing-source-telemetry:compass-work",
                violatedCondition: "missing-source-telemetry",
                retryLawful: true,
                cycleMarker);
        }

        source.LifecycleState = SliLocalityShardLifecycleState.Joined;
        var relationEvent = CreateEvent(
            source.ShardId,
            target.ShardId,
            SliLocalityRelationKind.IngestsFrom,
            carryPolicy,
            joinEligible: true,
            SliLocalityRelationOutcomeKind.Joined,
            "ingestion-joined",
            cycleMarker);
        context.RecordRelation(relationEvent);
        return relationEvent;
    }

    private static SliLocalityRelationEvent RecordFailure(
        SliExecutionContext context,
        string sourceShardId,
        string targetShardId,
        SliLocalityRelationKind relationKind,
        SliLocalityTelemetryCarryPolicy carryPolicy,
        SliLocalityRelationOutcomeKind outcome,
        string reasonCode,
        string violatedCondition,
        bool retryLawful,
        string cycleMarker)
    {
        var relationEvent = CreateEvent(
            sourceShardId,
            targetShardId,
            relationKind,
            carryPolicy,
            joinEligible: false,
            outcome,
            reasonCode,
            cycleMarker);
        context.RecordRelation(relationEvent);
        context.SetShardLifecycleState(sourceShardId, SliLocalityShardLifecycleState.Obstructed);
        context.RecordObstruction(new SliLocalityObstructionRecord(
            ObstructionId: $"obstruction:{sourceShardId}:{targetShardId}:{relationKind}:{context.LocalityObstructions.Count + 1}",
            SourceShardId: sourceShardId,
            TargetShardId: targetShardId,
            AttemptedRelation: relationKind,
            ViolatedCondition: violatedCondition,
            RetryLawful: retryLawful,
            HostFallbackOccurred: false,
            EscalationRequired: false,
            CycleMarker: cycleMarker));
        return relationEvent;
    }

    private static SliLocalityRelationEvent CreateEvent(
        string sourceShardId,
        string targetShardId,
        SliLocalityRelationKind relationKind,
        SliLocalityTelemetryCarryPolicy carryPolicy,
        bool joinEligible,
        SliLocalityRelationOutcomeKind outcome,
        string reasonCode,
        string cycleMarker)
    {
        return new SliLocalityRelationEvent(
            RelationId: $"relation:{sourceShardId}:{targetShardId}:{relationKind}:{cycleMarker}",
            SourceShardId: sourceShardId,
            TargetShardId: targetShardId,
            RelationKind: relationKind,
            TelemetryCarryPolicy: carryPolicy,
            JoinEligible: joinEligible,
            Outcome: outcome,
            ReasonCode: reasonCode,
            CycleMarker: cycleMarker);
    }

    private static bool HasSharedContinuity(SliLocalityShardRecord source, SliLocalityShardRecord target)
    {
        return string.Equals(source.ParentExecutionId, target.ParentExecutionId, StringComparison.Ordinal) &&
               string.Equals(source.RootAnchor, target.RootAnchor, StringComparison.Ordinal) &&
               source.ShardKind != target.ShardKind;
    }

    private static bool IsValidRelationPair(
        SliLocalityRelationKind relationKind,
        SliLocalityShardRecord source,
        SliLocalityShardRecord target)
    {
        return relationKind switch
        {
            SliLocalityRelationKind.AdjacentTo =>
                IsAdjacencyPair(source.ShardKind, target.ShardKind),
            SliLocalityRelationKind.WitnessOf =>
                source.ShardKind == SliLocalityShardKind.Witnessing &&
                target.ShardKind == SliLocalityShardKind.Acting &&
                string.Equals(source.SymbolBoundaryRef, SliCompassLocalityShards.WitnessingBoundaryRef, StringComparison.Ordinal) &&
                string.Equals(target.SymbolBoundaryRef, SliCompassLocalityShards.ActingBoundaryRef, StringComparison.Ordinal),
            SliLocalityRelationKind.IngestsFrom =>
                source.ShardKind == SliLocalityShardKind.AdjacentIngestion &&
                target.ShardKind == SliLocalityShardKind.Witnessing &&
                string.Equals(source.SymbolBoundaryRef, SliCompassLocalityShards.AdjacentIngestionBoundaryRef, StringComparison.Ordinal) &&
                string.Equals(target.SymbolBoundaryRef, SliCompassLocalityShards.WitnessingBoundaryRef, StringComparison.Ordinal),
            _ => false
        };
    }

    private static bool IsAdjacencyPair(SliLocalityShardKind source, SliLocalityShardKind target)
    {
        return (source == SliLocalityShardKind.Acting && target == SliLocalityShardKind.Witnessing) ||
               (source == SliLocalityShardKind.Witnessing && target == SliLocalityShardKind.Acting) ||
               (source == SliLocalityShardKind.Witnessing && target == SliLocalityShardKind.AdjacentIngestion) ||
               (source == SliLocalityShardKind.AdjacentIngestion && target == SliLocalityShardKind.Witnessing);
    }
}
