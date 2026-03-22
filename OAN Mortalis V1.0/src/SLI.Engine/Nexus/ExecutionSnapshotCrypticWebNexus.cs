using System.Collections.ObjectModel;
using SLI.Engine.Runtime;

namespace SLI.Engine.Nexus;

internal sealed class ExecutionSnapshotCrypticWebNexus : ICrypticWebNexus
{
    private readonly SliExecutionSnapshot _snapshot;
    private readonly DateTime _capturedAtUtc;
    private readonly WebTopologySnapshot _topologySnapshot;
    private readonly IReadOnlyList<MutationEvent> _mutationEvents;
    private readonly RelaxationReceipt _relaxationReceipt;
    private readonly NexusTelemetryFrame _telemetryFrame;

    public ExecutionSnapshotCrypticWebNexus(SliExecutionSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        _capturedAtUtc = DateTime.UtcNow;

        var shardById = _snapshot.LocalityShards.ToDictionary(shard => shard.ShardId, StringComparer.OrdinalIgnoreCase);
        var unresolvedStrain = BuildUnresolvedStrain(_snapshot);
        var mutationEvents = BuildMutationEvents(_snapshot, shardById, unresolvedStrain.Count > 0);
        var fieldState = ResolveFieldState(_snapshot, unresolvedStrain);
        var activeRegions = BuildActiveRegions(_snapshot);
        var activeRelations = BuildActiveRelations(_snapshot);
        var braidRegions = BuildBraidRegions(_snapshot, shardById);
        var equilibriumMarkers = BuildEquilibriumMarkers(_snapshot, unresolvedStrain.Count);
        var boundaryIntegrityState = ResolveBoundaryIntegrityState(_snapshot);
        var relaxationState = ResolveRelaxationState(fieldState);
        var readinessState = ResolveReadinessState(fieldState);
        var reasonCode = ResolveReasonCode(fieldState, unresolvedStrain.Count);
        var focalRegion = ResolveFocalRegion(_snapshot, braidRegions, activeRegions);
        var relaxationProgress = ResolveRelaxationProgress(relaxationState);

        _mutationEvents = new ReadOnlyCollection<MutationEvent>(mutationEvents);
        _topologySnapshot = new WebTopologySnapshot(
            SnapshotId: $"topology:{_snapshot.TraceId}",
            CapturedAtUtc: _capturedAtUtc,
            FieldState: fieldState,
            ActiveRegions: activeRegions,
            ActiveRelations: activeRelations,
            BraidRegions: braidRegions,
            EquilibriumMarkers: equilibriumMarkers,
            UnresolvedStrain: unresolvedStrain);
        _relaxationReceipt = new RelaxationReceipt(
            ReceiptId: $"relaxation:{_snapshot.TraceId}",
            CapturedAtUtc: _capturedAtUtc,
            SourceMutationIds: _mutationEvents.Select(item => item.EventId).ToArray(),
            RelaxationState: relaxationState,
            ReadyForReentry: readinessState is NexusReadinessState.ReadyForReentry or NexusReadinessState.DormantCoherent,
            ResidualStrain: unresolvedStrain.Count,
            BoundaryIntegrityState: boundaryIntegrityState,
            ReasonCode: reasonCode);
        _telemetryFrame = new NexusTelemetryFrame(
            FrameId: $"nexus:{_snapshot.TraceId}",
            CapturedAtUtc: _capturedAtUtc,
            FocalRegion: focalRegion,
            TopologyState: fieldState,
            MutationIndex: _mutationEvents.Count,
            RelaxationProgress: relaxationProgress,
            ReadinessState: readinessState,
            OrientationNotes: BuildOrientationNotes(_snapshot, focalRegion),
            ReasonCode: reasonCode);
    }

    public string NexusId => $"cryptic-nexus:{_snapshot.TraceId}";
    public string TraceId => _snapshot.TraceId;

    public WebTopologySnapshot CaptureTopologySnapshot() => _topologySnapshot;

    public IReadOnlyList<MutationEvent> CaptureMutationEvents() => _mutationEvents;

    public RelaxationReceipt CaptureRelaxationReceipt() => _relaxationReceipt;

    public NexusTelemetryFrame CaptureTelemetryFrame() => _telemetryFrame;

    private static IReadOnlyList<string> BuildActiveRegions(SliExecutionSnapshot snapshot)
    {
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var shard in snapshot.LocalityShards)
        {
            if (!string.IsNullOrWhiteSpace(shard.LocalityHandle))
            {
                values.Add(shard.LocalityHandle);
            }
        }

        foreach (var webbingEvent in snapshot.ActualizationWebbingEvents)
        {
            if (!string.IsNullOrWhiteSpace(webbingEvent.LocalityHandle))
            {
                values.Add(webbingEvent.LocalityHandle);
            }
        }

        if (values.Count == 0)
        {
            values.Add("serial-runtime");
        }

        return values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<string> BuildActiveRelations(SliExecutionSnapshot snapshot)
    {
        return snapshot.LocalityRelationEvents
            .Select(item => $"{item.RelationKind}:{item.SourceShardId}->{item.TargetShardId}:{item.Outcome}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildBraidRegions(
        SliExecutionSnapshot snapshot,
        IReadOnlyDictionary<string, LocalityShardSnapshot> shardById)
    {
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relationEvent in snapshot.LocalityRelationEvents)
        {
            if (relationEvent.Outcome != SliLocalityRelationOutcomeKind.Joined)
            {
                continue;
            }

            if (shardById.TryGetValue(relationEvent.SourceShardId, out var sourceShard) &&
                !string.IsNullOrWhiteSpace(sourceShard.LocalityHandle))
            {
                values.Add(sourceShard.LocalityHandle);
            }

            if (shardById.TryGetValue(relationEvent.TargetShardId, out var targetShard) &&
                !string.IsNullOrWhiteSpace(targetShard.LocalityHandle))
            {
                values.Add(targetShard.LocalityHandle);
            }
        }

        return values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<string> BuildEquilibriumMarkers(SliExecutionSnapshot snapshot, int unresolvedStrainCount)
    {
        var markers = new List<string>
        {
            $"decision:{snapshot.Decision}",
            $"branch:{snapshot.DecisionBranch}"
        };

        if (snapshot.LiveRuntimeRun is not null)
        {
            markers.Add($"reduction:{snapshot.LiveRuntimeRun.ReductionOutcome}");
        }

        if (snapshot.ActualizationPacket is not null)
        {
            markers.Add($"actualization:{snapshot.ActualizationPacket.Disposition}");
        }

        markers.Add(unresolvedStrainCount == 0 ? "equilibrium:coherent" : "equilibrium:strained");
        return markers;
    }

    private static IReadOnlyList<string> BuildUnresolvedStrain(SliExecutionSnapshot snapshot)
    {
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var obstruction in snapshot.LocalityObstructions)
        {
            values.Add($"obstruction:{obstruction.AttemptedRelation}:{obstruction.ViolatedCondition}");
        }

        foreach (var relationEvent in snapshot.LocalityRelationEvents)
        {
            if (relationEvent.Outcome != SliLocalityRelationOutcomeKind.Joined)
            {
                values.Add($"relation:{relationEvent.RelationKind}:{relationEvent.ReasonCode}");
            }
        }

        if (snapshot.ActualizationPacket is not null)
        {
            foreach (var residue in snapshot.ActualizationPacket.ResidueSet)
            {
                if (!string.IsNullOrWhiteSpace(residue))
                {
                    values.Add(residue);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(snapshot.CleaveResidue) &&
            !string.Equals(snapshot.CleaveResidue, "none", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(snapshot.CleaveResidue, "[]", StringComparison.Ordinal))
        {
            values.Add($"cleave:{snapshot.CleaveResidue}");
        }

        return values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static List<MutationEvent> BuildMutationEvents(
        SliExecutionSnapshot snapshot,
        IReadOnlyDictionary<string, LocalityShardSnapshot> shardById,
        bool hasUnresolvedStrain)
    {
        var events = new List<MutationEvent>();
        foreach (var webbingEvent in snapshot.ActualizationWebbingEvents)
        {
            var constraints = BuildIdentityConstraints(webbingEvent.ShardId, shardById);
            var eventState = ResolveWebbingEventState(snapshot, hasUnresolvedStrain);
            events.Add(new MutationEvent(
                EventId: $"mutation:{snapshot.TraceId}:{events.Count + 1}",
                OccurredAtUtc: DateTime.UtcNow,
                OriginRegion: string.IsNullOrWhiteSpace(webbingEvent.LocalityHandle) ? "serial-runtime" : webbingEvent.LocalityHandle,
                AffectedRegions: [string.IsNullOrWhiteSpace(webbingEvent.LocalityHandle) ? "serial-runtime" : webbingEvent.LocalityHandle],
                MutationKind: webbingEvent.Stage.ToString(),
                PreservedIdentityConstraints: constraints,
                StrainDelta: ResolveStrainDelta(eventState),
                CausalReason: webbingEvent.Detail,
                EventState: eventState));
        }

        if (events.Count > 0)
        {
            return events;
        }

        foreach (var relationEvent in snapshot.LocalityRelationEvents)
        {
            var affectedRegions = new List<string>();
            if (shardById.TryGetValue(relationEvent.SourceShardId, out var sourceShard) &&
                !string.IsNullOrWhiteSpace(sourceShard.LocalityHandle))
            {
                affectedRegions.Add(sourceShard.LocalityHandle);
            }

            if (shardById.TryGetValue(relationEvent.TargetShardId, out var targetShard) &&
                !string.IsNullOrWhiteSpace(targetShard.LocalityHandle))
            {
                affectedRegions.Add(targetShard.LocalityHandle);
            }

            if (affectedRegions.Count == 0)
            {
                affectedRegions.Add("serial-runtime");
            }

            events.Add(new MutationEvent(
                EventId: $"mutation:{snapshot.TraceId}:{events.Count + 1}",
                OccurredAtUtc: DateTime.UtcNow,
                OriginRegion: affectedRegions[0],
                AffectedRegions: affectedRegions,
                MutationKind: $"Relation:{relationEvent.RelationKind}",
                PreservedIdentityConstraints: BuildIdentityConstraints(relationEvent.SourceShardId, shardById)
                    .Concat(BuildIdentityConstraints(relationEvent.TargetShardId, shardById))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                StrainDelta: ResolveStrainDelta(ResolveRelationEventState(relationEvent.Outcome)),
                CausalReason: relationEvent.ReasonCode,
                EventState: ResolveRelationEventState(relationEvent.Outcome)));
        }

        return events;
    }

    private static IReadOnlyList<string> BuildIdentityConstraints(
        string shardId,
        IReadOnlyDictionary<string, LocalityShardSnapshot> shardById)
    {
        if (!shardById.TryGetValue(shardId, out var shard))
        {
            return Array.Empty<string>();
        }

        var constraints = new List<string>();
        if (!string.IsNullOrWhiteSpace(shard.SymbolBoundaryRef))
        {
            constraints.Add($"boundary:{shard.SymbolBoundaryRef}");
        }

        if (!string.IsNullOrWhiteSpace(shard.RootAnchor))
        {
            constraints.Add($"root:{shard.RootAnchor}");
        }

        return constraints;
    }

    private static MutationEventState ResolveWebbingEventState(SliExecutionSnapshot snapshot, bool hasUnresolvedStrain)
    {
        if (snapshot.ActualizationPacket?.Disposition == SliActualizationDisposition.Obstructed ||
            snapshot.LiveRuntimeRun?.ReductionOutcome is SliLocalityRelationOutcomeKind.Obstructed or SliLocalityRelationOutcomeKind.Refused)
        {
            return MutationEventState.Obstructed;
        }

        if (hasUnresolvedStrain || snapshot.LiveRuntimeRun?.ReductionOutcome == SliLocalityRelationOutcomeKind.Deferred)
        {
            return MutationEventState.Deferred;
        }

        return MutationEventState.Completed;
    }

    private static MutationEventState ResolveRelationEventState(SliLocalityRelationOutcomeKind outcome)
    {
        return outcome switch
        {
            SliLocalityRelationOutcomeKind.Joined => MutationEventState.Completed,
            SliLocalityRelationOutcomeKind.Deferred => MutationEventState.Deferred,
            _ => MutationEventState.Obstructed
        };
    }

    private static int ResolveStrainDelta(MutationEventState eventState)
    {
        return eventState switch
        {
            MutationEventState.Completed => 0,
            MutationEventState.Deferred => 1,
            MutationEventState.Obstructed => 2,
            _ => 0
        };
    }

    private static WebFieldState ResolveFieldState(SliExecutionSnapshot snapshot, IReadOnlyList<string> unresolvedStrain)
    {
        if (snapshot.LocalityObstructions.Count > 0 ||
            snapshot.ActualizationPacket?.Disposition == SliActualizationDisposition.Obstructed ||
            snapshot.LiveRuntimeRun?.ReductionOutcome is SliLocalityRelationOutcomeKind.Obstructed or SliLocalityRelationOutcomeKind.Refused)
        {
            return WebFieldState.Contradictory;
        }

        if (snapshot.LiveRuntimeRun?.ReductionOutcome == SliLocalityRelationOutcomeKind.Deferred ||
            snapshot.LocalityShards.Any(item => item.LifecycleState is SliLocalityShardLifecycleState.WaitingForJoin or SliLocalityShardLifecycleState.Deferred))
        {
            return WebFieldState.Relaxing;
        }

        if (snapshot.LocalityShards.Count > 0 ||
            snapshot.LocalityRelationEvents.Count > 0 ||
            snapshot.ActualizationWebbingEvents.Count > 0)
        {
            return unresolvedStrain.Count > 0
                ? WebFieldState.Relaxing
                : WebFieldState.ReadyForReentry;
        }

        return WebFieldState.DormantCoherent;
    }

    private static RelaxationState ResolveRelaxationState(WebFieldState fieldState)
    {
        return fieldState switch
        {
            WebFieldState.DormantCoherent => RelaxationState.DormantCoherent,
            WebFieldState.Relaxing => RelaxationState.IncompleteRelaxation,
            WebFieldState.ReadyForReentry => RelaxationState.ReadyForReentry,
            WebFieldState.Contradictory => RelaxationState.Contradictory,
            _ => RelaxationState.IncompleteRelaxation
        };
    }

    private static NexusReadinessState ResolveReadinessState(WebFieldState fieldState)
    {
        return fieldState switch
        {
            WebFieldState.DormantCoherent => NexusReadinessState.DormantCoherent,
            WebFieldState.Relaxing => NexusReadinessState.NotReady,
            WebFieldState.ReadyForReentry => NexusReadinessState.ReadyForReentry,
            WebFieldState.Contradictory => NexusReadinessState.Contradictory,
            _ => NexusReadinessState.NotReady
        };
    }

    private static string ResolveBoundaryIntegrityState(SliExecutionSnapshot snapshot)
    {
        if (snapshot.LocalityObstructions.Count > 0 ||
            snapshot.LiveRuntimeRun?.ReductionOutcome is SliLocalityRelationOutcomeKind.Obstructed or SliLocalityRelationOutcomeKind.Refused)
        {
            return "violated";
        }

        if (snapshot.LiveRuntimeRun?.ReductionOutcome == SliLocalityRelationOutcomeKind.Deferred ||
            snapshot.LocalityShards.Any(item => item.LifecycleState is SliLocalityShardLifecycleState.WaitingForJoin or SliLocalityShardLifecycleState.Deferred))
        {
            return "strained";
        }

        return "preserved";
    }

    private static string ResolveReasonCode(WebFieldState fieldState, int unresolvedStrainCount)
    {
        return fieldState switch
        {
            WebFieldState.DormantCoherent => "cryptic-web-dormant-coherent",
            WebFieldState.Relaxing when unresolvedStrainCount > 0 => "cryptic-web-relaxing-with-strain",
            WebFieldState.Relaxing => "cryptic-web-relaxing",
            WebFieldState.ReadyForReentry => "cryptic-web-ready-for-reentry",
            WebFieldState.Contradictory => "cryptic-web-contradictory",
            _ => "cryptic-web-unclassified"
        };
    }

    private static string ResolveFocalRegion(
        SliExecutionSnapshot snapshot,
        IReadOnlyList<string> braidRegions,
        IReadOnlyList<string> activeRegions)
    {
        if (snapshot.LiveRuntimeRun is not null)
        {
            var primaryRegion = snapshot.LocalityShards
                .FirstOrDefault(item => string.Equals(item.ShardId, snapshot.LiveRuntimeRun.PrimaryShardId, StringComparison.OrdinalIgnoreCase))
                ?.LocalityHandle;
            if (!string.IsNullOrWhiteSpace(primaryRegion))
            {
                return primaryRegion;
            }
        }

        if (braidRegions.Count > 0)
        {
            return braidRegions[0];
        }

        if (activeRegions.Count > 0)
        {
            return activeRegions[0];
        }

        return "serial-runtime";
    }

    private static string ResolveRelaxationProgress(RelaxationState relaxationState)
    {
        return relaxationState switch
        {
            RelaxationState.DormantCoherent => "dormant",
            RelaxationState.IncompleteRelaxation => "settling",
            RelaxationState.ReadyForReentry => "ready",
            RelaxationState.Contradictory => "blocked",
            _ => "unclassified"
        };
    }

    private static string BuildOrientationNotes(SliExecutionSnapshot snapshot, string focalRegion)
    {
        var segments = new List<string>
        {
            $"focal-region={focalRegion}",
            $"decision={snapshot.Decision}",
            $"branch={snapshot.DecisionBranch}"
        };

        if (snapshot.LiveRuntimeRun is not null)
        {
            segments.Add($"reduction={snapshot.LiveRuntimeRun.ReductionOutcome}");
        }

        if (snapshot.ActualizationPacket is not null)
        {
            segments.Add($"actualization={snapshot.ActualizationPacket.Disposition}");
        }

        return string.Join("; ", segments);
    }
}
