using Oan.Common;

namespace SLI.Engine.Runtime;

internal enum SliLiveEngramKind
{
    HotWorkingEngram = 0,
    CompositeEngram = 1,
    WitnessEngram = 2,
    ResidueEngram = 3,
    GovernanceCandidateEngram = 4,
    ReturnCandidateEngram = 5
}

internal enum SliLiveEngramRuntimeState
{
    Loaded = 0,
    HotActive = 1,
    Braided = 2,
    Witnessed = 3,
    ResidueBearing = 4,
    ReturnCandidate = 5,
    Obstructed = 6,
    Deferred = 7
}

internal enum SliLiveEngramOperationKind
{
    Instantiate = 0,
    Bind = 1,
    Address = 2,
    Activate = 3,
    Braid = 4,
    Witness = 5,
    ResidueExtract = 6,
    ShapeReturnCandidate = 7
}

internal sealed record SliLiveEngramTraceEntry(
    SliLiveEngramOperationKind Operation,
    string Detail,
    string SourceHandle,
    string LocalityHandle);

internal sealed class SliLiveEngramRuntimePacket
{
    public required string EngramHandle { get; init; }
    public required SliLiveEngramKind EngramKind { get; init; }
    public required SliLiveEngramRuntimeState RuntimeState { get; init; }
    public required string ShardId { get; init; }
    public required SliLocalityShardKind ShardKind { get; init; }
    public required string ParentExecutionId { get; init; }
    public required string RootAnchor { get; init; }
    public required string SymbolBoundaryRef { get; init; }
    public required string LocalityHandle { get; init; }
    public required string SourceHandle { get; init; }
    public required IReadOnlyList<string> InvariantSet { get; init; }
    public required IReadOnlyList<string> ResidueSet { get; init; }
    public required IReadOnlyList<string> WitnessSet { get; init; }
    public required IReadOnlyList<SliLiveEngramTraceEntry> TraceSet { get; init; }
    public required bool ReturnCandidateEligible { get; init; }
    public required string ReturnEligibilityReason { get; init; }
}

internal sealed class SliLiveEngramRuntimeRun
{
    public required string ExecutionId { get; init; }
    public required bool ShardModeEnabled { get; init; }
    public required string PrimaryShardId { get; init; }
    public required IReadOnlyList<SliLiveEngramRuntimePacket> ShardPackets { get; init; }
    public required IReadOnlyList<SliLocalityRelationEvent> RelationEvents { get; init; }
    public required IReadOnlyList<SliLocalityObstructionRecord> Obstructions { get; init; }
    public required SliLocalityRelationOutcomeKind ReductionOutcome { get; init; }
    public required string ReductionReason { get; init; }
}

internal static class SliLiveEngramRuntimePacketFactory
{
    public static SliLiveEngramRuntimePacket CreateForCognition(
        SliExecutionContext context,
        string traceId,
        ZedThetaCandidateReceipt candidate)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentNullException.ThrowIfNull(candidate);

        if (context.ShardModeEnabled)
        {
            var run = CreateRunForCognition(context, traceId, candidate);
            return ResolveCompatibilityPacket(run);
        }

        var localityHandle = ResolveSerialLocalityHandle(context);
        var sourceHandle = candidate.CandidateHandle;
        var residueSet = CollectCognitionResidues(context);
        var witnessSet = CollectCognitionWitnessSet(candidate);
        var invariantSet = CollectCognitionInvariantSet(context, candidate);
        var returnCandidateEligible =
            candidate.BridgeReview?.OutcomeKind == SliBridgeOutcomeKind.Ok &&
            candidate.RuntimeUseCeiling?.CandidateOnly == true;
        var runtimeState = ResolveCognitionRuntimeState(candidate, residueSet.Count, returnCandidateEligible);
        var engramKind = ResolveCognitionEngramKind(runtimeState);

        return new SliLiveEngramRuntimePacket
        {
            EngramHandle = $"live-engram:{traceId}",
            EngramKind = engramKind,
            RuntimeState = runtimeState,
            ShardId = "serial",
            ShardKind = SliLocalityShardKind.Acting,
            ParentExecutionId = context.ExecutionId,
            RootAnchor = SliCompassLocalityShards.ResolveRootAnchor(context.Frame),
            SymbolBoundaryRef = "serial-runtime-surface",
            LocalityHandle = localityHandle,
            SourceHandle = sourceHandle,
            InvariantSet = invariantSet,
            ResidueSet = residueSet,
            WitnessSet = witnessSet,
            TraceSet = BuildTraceSet(
                sourceHandle,
                localityHandle,
                context.TraceLines,
                hasBraid: false,
                hasWitness: witnessSet.Count > 0,
                hasResidue: residueSet.Count > 0,
                hasReturnCandidate: returnCandidateEligible),
            ReturnCandidateEligible = returnCandidateEligible,
            ReturnEligibilityReason = returnCandidateEligible
                ? "candidate-bearing-bridge-ok"
                : candidate.BridgeReview?.ReasonCode ?? "bridge-review-unavailable"
        };
    }

    public static SliLiveEngramRuntimeRun CreateRunForCognition(
        SliExecutionContext context,
        string traceId,
        ZedThetaCandidateReceipt candidate)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentNullException.ThrowIfNull(candidate);

        var reductionOutcome = ResolveReductionOutcome(context.LocalityRelationEvents);
        var reductionReason = ResolveReductionReason(context.LocalityRelationEvents, reductionOutcome);
        var packets = context.LocalityShards
            .OrderBy(ResolveShardOrder)
            .Select(shard => CreateShardPacket(context, traceId, candidate, shard, reductionOutcome, reductionReason))
            .ToArray();

        return new SliLiveEngramRuntimeRun
        {
            ExecutionId = context.ExecutionId,
            ShardModeEnabled = true,
            PrimaryShardId = context.PrimaryShardId ?? SliCompassLocalityShards.ActingShardId,
            ShardPackets = packets,
            RelationEvents = context.LocalityRelationEvents.ToArray(),
            Obstructions = context.LocalityObstructions.ToArray(),
            ReductionOutcome = reductionOutcome,
            ReductionReason = reductionReason
        };
    }

    public static SliLiveEngramRuntimePacket ResolveCompatibilityPacket(SliLiveEngramRuntimeRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        return run.ShardPackets.First(packet =>
            string.Equals(packet.ShardId, run.PrimaryShardId, StringComparison.OrdinalIgnoreCase));
    }

    public static SliLiveEngramRuntimePacket CreateForHigherOrderLocality(SliExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var localityState = context.HigherOrderLocalityState;
        var localityHandle = ResolveSerialLocalityHandle(context);
        var sourceHandle = string.IsNullOrWhiteSpace(localityState.AccountabilityPacket.PacketHandle)
            ? localityHandle
            : localityState.AccountabilityPacket.PacketHandle;
        var residueSet = CollectHigherOrderResidues(localityState);
        var witnessSet = CollectHigherOrderWitnessSet(localityState);
        var invariantSet = CollectHigherOrderInvariantSet(localityState);
        var returnCandidateEligible =
            localityState.AccountabilityPacket.IsConfigured &&
            string.Equals(
                localityState.AccountabilityPacket.ReadinessStatus,
                SliAccountabilityPacketState.ReviewReady,
                StringComparison.OrdinalIgnoreCase);
        var runtimeState = ResolveHigherOrderRuntimeState(localityState, residueSet.Count, returnCandidateEligible);
        var engramKind = ResolveHigherOrderEngramKind(localityState, runtimeState);

        return new SliLiveEngramRuntimePacket
        {
            EngramHandle = $"{localityHandle}:live-runtime",
            EngramKind = engramKind,
            RuntimeState = runtimeState,
            ShardId = "serial",
            ShardKind = SliLocalityShardKind.Acting,
            ParentExecutionId = context.ExecutionId,
            RootAnchor = SliCompassLocalityShards.ResolveRootAnchor(context.Frame),
            SymbolBoundaryRef = "serial-runtime-surface",
            LocalityHandle = localityHandle,
            SourceHandle = sourceHandle,
            InvariantSet = invariantSet,
            ResidueSet = residueSet,
            WitnessSet = witnessSet,
            TraceSet = BuildTraceSet(
                sourceHandle,
                localityHandle,
                context.TraceLines,
                hasBraid: localityState.Participation.IsConfigured || localityState.Rehearsal.IsConfigured,
                hasWitness: localityState.Witness.IsConfigured || witnessSet.Count > 0,
                hasResidue: residueSet.Count > 0,
                hasReturnCandidate: returnCandidateEligible),
            ReturnCandidateEligible = returnCandidateEligible,
            ReturnEligibilityReason = returnCandidateEligible
                ? "accountability-review-ready"
                : localityState.AccountabilityPacket.ReadinessStatus
        };
    }

    private static SliLiveEngramRuntimePacket CreateShardPacket(
        SliExecutionContext context,
        string traceId,
        ZedThetaCandidateReceipt candidate,
        SliLocalityShardRecord shard,
        SliLocalityRelationOutcomeKind reductionOutcome,
        string reductionReason)
    {
        var sourceHandle = candidate.CandidateHandle;
        var traceLines = context.GetShardTraceLines(shard.ShardId);
        var residueSet = CollectShardResidues(context, shard, reductionOutcome, reductionReason);
        var witnessSet = CollectShardWitnessSet(context, candidate, shard);
        var invariantSet = CollectShardInvariantSet(context, candidate, shard);
        var returnCandidateEligible = shard.ShardKind == SliLocalityShardKind.Acting &&
            candidate.BridgeReview?.OutcomeKind == SliBridgeOutcomeKind.Ok &&
            candidate.RuntimeUseCeiling?.CandidateOnly == true;

        var runtimeState = ResolveShardRuntimeState(
            shard,
            candidate,
            reductionOutcome,
            residueSet.Count,
            returnCandidateEligible);
        var engramKind = ResolveShardEngramKind(shard.ShardKind, runtimeState);

        return new SliLiveEngramRuntimePacket
        {
            EngramHandle = $"live-engram:{traceId}:{shard.ShardId}",
            EngramKind = engramKind,
            RuntimeState = runtimeState,
            ShardId = shard.ShardId,
            ShardKind = shard.ShardKind,
            ParentExecutionId = shard.ParentExecutionId,
            RootAnchor = shard.RootAnchor,
            SymbolBoundaryRef = shard.SymbolBoundaryRef,
            LocalityHandle = shard.LocalityHandle,
            SourceHandle = sourceHandle,
            InvariantSet = invariantSet,
            ResidueSet = residueSet,
            WitnessSet = witnessSet,
            TraceSet = BuildTraceSet(
                sourceHandle,
                shard.LocalityHandle,
                traceLines,
                hasBraid: shard.ShardKind == SliLocalityShardKind.AdjacentIngestion &&
                          reductionOutcome == SliLocalityRelationOutcomeKind.Joined,
                hasWitness: shard.ShardKind != SliLocalityShardKind.AdjacentIngestion &&
                            witnessSet.Count > 0,
                hasResidue: residueSet.Count > 0,
                hasReturnCandidate: returnCandidateEligible),
            ReturnCandidateEligible = returnCandidateEligible,
            ReturnEligibilityReason = returnCandidateEligible
                ? "candidate-bearing-bridge-ok"
                : shard.ShardKind == SliLocalityShardKind.Acting
                    ? candidate.BridgeReview?.ReasonCode ?? "bridge-review-unavailable"
                    : "non-primary-shard"
        };
    }

    private static SliLocalityRelationOutcomeKind ResolveReductionOutcome(
        IReadOnlyList<SliLocalityRelationEvent> relations)
    {
        var required = relations
            .Where(evt => evt.RelationKind is SliLocalityRelationKind.WitnessOf or SliLocalityRelationKind.IngestsFrom)
            .ToArray();

        if (required.Any(evt => evt.Outcome == SliLocalityRelationOutcomeKind.Refused))
        {
            return SliLocalityRelationOutcomeKind.Refused;
        }

        if (required.Any(evt => evt.Outcome == SliLocalityRelationOutcomeKind.Obstructed))
        {
            return SliLocalityRelationOutcomeKind.Obstructed;
        }

        if (required.Length == 2 &&
            required.All(evt => evt.Outcome == SliLocalityRelationOutcomeKind.Joined))
        {
            return SliLocalityRelationOutcomeKind.Joined;
        }

        return SliLocalityRelationOutcomeKind.Deferred;
    }

    private static string ResolveReductionReason(
        IReadOnlyList<SliLocalityRelationEvent> relations,
        SliLocalityRelationOutcomeKind reductionOutcome)
    {
        if (reductionOutcome == SliLocalityRelationOutcomeKind.Joined)
        {
            return "compass-shards-joined";
        }

        var firstRelevant = relations
            .Where(evt => evt.RelationKind is SliLocalityRelationKind.WitnessOf or SliLocalityRelationKind.IngestsFrom)
            .FirstOrDefault(evt => evt.Outcome == reductionOutcome);

        return firstRelevant?.ReasonCode ?? "compass-shards-not-joined";
    }

    private static int ResolveShardOrder(SliLocalityShardRecord shard)
    {
        return shard.ShardKind switch
        {
            SliLocalityShardKind.Acting => 0,
            SliLocalityShardKind.Witnessing => 1,
            _ => 2
        };
    }

    private static SliLiveEngramRuntimeState ResolveShardRuntimeState(
        SliLocalityShardRecord shard,
        ZedThetaCandidateReceipt candidate,
        SliLocalityRelationOutcomeKind reductionOutcome,
        int residueCount,
        bool returnCandidateEligible)
    {
        if (shard.ShardKind == SliLocalityShardKind.Acting)
        {
            return ResolveCognitionRuntimeState(candidate, residueCount, returnCandidateEligible);
        }

        return shard.ShardKind switch
        {
            SliLocalityShardKind.Witnessing => reductionOutcome switch
            {
                SliLocalityRelationOutcomeKind.Joined => SliLiveEngramRuntimeState.Witnessed,
                SliLocalityRelationOutcomeKind.Deferred => SliLiveEngramRuntimeState.Deferred,
                _ => SliLiveEngramRuntimeState.Obstructed
            },
            SliLocalityShardKind.AdjacentIngestion => reductionOutcome switch
            {
                SliLocalityRelationOutcomeKind.Joined => SliLiveEngramRuntimeState.Braided,
                SliLocalityRelationOutcomeKind.Deferred => SliLiveEngramRuntimeState.Deferred,
                _ => SliLiveEngramRuntimeState.Obstructed
            },
            _ => SliLiveEngramRuntimeState.Loaded
        };
    }

    private static SliLiveEngramKind ResolveShardEngramKind(
        SliLocalityShardKind shardKind,
        SliLiveEngramRuntimeState runtimeState)
    {
        if (shardKind == SliLocalityShardKind.Acting)
        {
            return ResolveCognitionEngramKind(runtimeState);
        }

        return shardKind switch
        {
            SliLocalityShardKind.Witnessing => SliLiveEngramKind.WitnessEngram,
            SliLocalityShardKind.AdjacentIngestion => SliLiveEngramKind.HotWorkingEngram,
            _ => SliLiveEngramKind.HotWorkingEngram
        };
    }

    private static SliLiveEngramRuntimeState ResolveCognitionRuntimeState(
        ZedThetaCandidateReceipt candidate,
        int residueCount,
        bool returnCandidateEligible)
    {
        if (candidate.BridgeReview is not null && candidate.BridgeReview.OutcomeKind != SliBridgeOutcomeKind.Ok)
        {
            return SliLiveEngramRuntimeState.Obstructed;
        }

        if (returnCandidateEligible)
        {
            return SliLiveEngramRuntimeState.ReturnCandidate;
        }

        if (residueCount > 0)
        {
            return SliLiveEngramRuntimeState.ResidueBearing;
        }

        return SliLiveEngramRuntimeState.HotActive;
    }

    private static SliLiveEngramKind ResolveCognitionEngramKind(SliLiveEngramRuntimeState runtimeState)
    {
        return runtimeState switch
        {
            SliLiveEngramRuntimeState.ReturnCandidate => SliLiveEngramKind.GovernanceCandidateEngram,
            SliLiveEngramRuntimeState.ResidueBearing => SliLiveEngramKind.ResidueEngram,
            SliLiveEngramRuntimeState.Obstructed => SliLiveEngramKind.ResidueEngram,
            _ => SliLiveEngramKind.HotWorkingEngram
        };
    }

    private static SliLiveEngramRuntimeState ResolveHigherOrderRuntimeState(
        SliHigherOrderLocalityState localityState,
        int residueCount,
        bool returnCandidateEligible)
    {
        if (returnCandidateEligible)
        {
            return SliLiveEngramRuntimeState.ReturnCandidate;
        }

        if (residueCount > 0)
        {
            return SliLiveEngramRuntimeState.ResidueBearing;
        }

        if (localityState.Witness.IsConfigured)
        {
            return SliLiveEngramRuntimeState.Witnessed;
        }

        if (localityState.Participation.IsConfigured || localityState.Rehearsal.IsConfigured)
        {
            return SliLiveEngramRuntimeState.Braided;
        }

        return localityState.Perspective.IsConfigured
            ? SliLiveEngramRuntimeState.HotActive
            : SliLiveEngramRuntimeState.Loaded;
    }

    private static SliLiveEngramKind ResolveHigherOrderEngramKind(
        SliHigherOrderLocalityState localityState,
        SliLiveEngramRuntimeState runtimeState)
    {
        if (runtimeState == SliLiveEngramRuntimeState.ReturnCandidate)
        {
            return SliLiveEngramKind.ReturnCandidateEngram;
        }

        if (runtimeState == SliLiveEngramRuntimeState.ResidueBearing)
        {
            return SliLiveEngramKind.ResidueEngram;
        }

        if (localityState.Witness.IsConfigured)
        {
            return SliLiveEngramKind.WitnessEngram;
        }

        if (localityState.Participation.IsConfigured || localityState.Rehearsal.IsConfigured)
        {
            return SliLiveEngramKind.CompositeEngram;
        }

        return SliLiveEngramKind.HotWorkingEngram;
    }

    private static IReadOnlyList<SliLiveEngramTraceEntry> BuildTraceSet(
        string sourceHandle,
        string localityHandle,
        IReadOnlyList<string> symbolicTrace,
        bool hasBraid,
        bool hasWitness,
        bool hasResidue,
        bool hasReturnCandidate)
    {
        var trace = new List<SliLiveEngramTraceEntry>
        {
            new(
                SliLiveEngramOperationKind.Instantiate,
                $"trace-count:{symbolicTrace.Count}",
                sourceHandle,
                localityHandle),
            new(
                SliLiveEngramOperationKind.Bind,
                "bound-runtime-locality",
                sourceHandle,
                localityHandle),
            new(
                SliLiveEngramOperationKind.Address,
                $"addressed:{sourceHandle}",
                sourceHandle,
                localityHandle),
            new(
                SliLiveEngramOperationKind.Activate,
                symbolicTrace.Count == 0 ? "no-symbolic-trace" : symbolicTrace[0],
                sourceHandle,
                localityHandle)
        };

        if (hasBraid)
        {
            trace.Add(new(
                SliLiveEngramOperationKind.Braid,
                "braided-runtime-participation",
                sourceHandle,
                localityHandle));
        }

        if (hasWitness)
        {
            trace.Add(new(
                SliLiveEngramOperationKind.Witness,
                "witness-bearing-runtime-unit",
                sourceHandle,
                localityHandle));
        }

        if (hasResidue)
        {
            trace.Add(new(
                SliLiveEngramOperationKind.ResidueExtract,
                "residue-bearing-runtime-unit",
                sourceHandle,
                localityHandle));
        }

        if (hasReturnCandidate)
        {
            trace.Add(new(
                SliLiveEngramOperationKind.ShapeReturnCandidate,
                "return-candidate-eligible",
                sourceHandle,
                localityHandle));
        }

        return trace;
    }

    private static IReadOnlyList<string> CollectCognitionInvariantSet(
        SliExecutionContext context,
        ZedThetaCandidateReceipt candidate)
    {
        var state = context.GoldenCodeState;
        var invariants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"active-basin:{state.ActiveBasin}",
            $"competing-basin:{state.CompetingBasin}",
            $"anchor-state:{state.AnchorState}",
            $"self-touch:{state.SelfTouchClass}",
            $"update-locus:{candidate.PacketDirective.UpdateLocus}"
        };

        if (candidate.BridgeReview is not null)
        {
            invariants.Add($"bridge-stage:{candidate.BridgeReview.BridgeStage}");
        }

        return invariants.ToArray();
    }

    private static IReadOnlyList<string> CollectShardInvariantSet(
        SliExecutionContext context,
        ZedThetaCandidateReceipt candidate,
        SliLocalityShardRecord shard)
    {
        var invariants = new HashSet<string>(CollectCognitionInvariantSet(context, candidate), StringComparer.OrdinalIgnoreCase)
        {
            $"shard-kind:{shard.ShardKind}",
            $"boundary:{shard.SymbolBoundaryRef}"
        };

        foreach (var relation in context.LocalityRelationEvents.Where(evt =>
                     string.Equals(evt.SourceShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(evt.TargetShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase)))
        {
            invariants.Add($"relation:{relation.RelationKind}:{relation.Outcome}");
        }

        return invariants.ToArray();
    }

    private static IReadOnlyList<string> CollectCognitionWitnessSet(ZedThetaCandidateReceipt candidate)
    {
        var witness = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (candidate.BridgeReview is not null)
        {
            witness.Add($"bridge-outcome:{candidate.BridgeReview.OutcomeKind}");
            witness.Add($"bridge-threshold:{candidate.BridgeReview.ThresholdClass}");

            if (!string.IsNullOrWhiteSpace(candidate.BridgeReview.BridgeWitnessHandle))
            {
                witness.Add(candidate.BridgeReview.BridgeWitnessHandle);
            }
        }

        return witness.ToArray();
    }

    private static IReadOnlyList<string> CollectShardWitnessSet(
        SliExecutionContext context,
        ZedThetaCandidateReceipt candidate,
        SliLocalityShardRecord shard)
    {
        var witness = new HashSet<string>(CollectCognitionWitnessSet(candidate), StringComparer.OrdinalIgnoreCase);
        foreach (var relation in context.LocalityRelationEvents.Where(evt =>
                     string.Equals(evt.SourceShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(evt.TargetShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase)))
        {
            witness.Add($"relation:{relation.RelationKind}:{relation.Outcome}:{relation.ReasonCode}");
        }

        return witness.ToArray();
    }

    private static IReadOnlyList<string> CollectCognitionResidues(SliExecutionContext context)
    {
        var residues = new List<string>();
        residues.AddRange(context.PrunedBranches.Select(branch => $"cleave:{branch}"));
        residues.AddRange(CollectHigherOrderResidues(context.HigherOrderLocalityState));
        return residues;
    }

    private static IReadOnlyList<string> CollectShardResidues(
        SliExecutionContext context,
        SliLocalityShardRecord shard,
        SliLocalityRelationOutcomeKind reductionOutcome,
        string reductionReason)
    {
        var residues = new List<string>();
        if (shard.ShardKind == SliLocalityShardKind.Acting)
        {
            residues.AddRange(CollectCognitionResidues(context));
        }

        residues.AddRange(context.LocalityObstructions
            .Where(obstruction =>
                string.Equals(obstruction.SourceShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(obstruction.TargetShardId, shard.ShardId, StringComparison.OrdinalIgnoreCase))
            .Select(obstruction => $"obstruction:{obstruction.AttemptedRelation}:{obstruction.ViolatedCondition}"));

        if (reductionOutcome != SliLocalityRelationOutcomeKind.Joined)
        {
            residues.Add($"reduction:{reductionOutcome}:{reductionReason}");
        }

        return residues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> CollectHigherOrderInvariantSet(SliHigherOrderLocalityState localityState)
    {
        var invariants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"seal:{localityState.SealPosture}",
            $"reveal:{localityState.RevealPosture}"
        };

        foreach (var preserved in localityState.Witness.PreservedInvariants)
        {
            invariants.Add(preserved);
        }

        foreach (var preserved in localityState.Transport.PreservedInvariants)
        {
            invariants.Add(preserved);
        }

        foreach (var preserved in localityState.AccountabilityPacket.PreservedInvariants)
        {
            invariants.Add(preserved);
        }

        return invariants.ToArray();
    }

    private static IReadOnlyList<string> CollectHigherOrderWitnessSet(SliHigherOrderLocalityState localityState)
    {
        var witness = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var preserved in localityState.Witness.PreservedInvariants)
        {
            witness.Add($"preserved:{preserved}");
        }

        foreach (var difference in localityState.Witness.DifferenceSet)
        {
            witness.Add($"difference:{difference}");
        }

        foreach (var evidence in localityState.AdmissibleSurface.EvidenceSet)
        {
            witness.Add($"evidence:{evidence}");
        }

        return witness.ToArray();
    }

    private static IReadOnlyList<string> CollectHigherOrderResidues(SliHigherOrderLocalityState localityState)
    {
        var residues = new List<string>();
        residues.AddRange(localityState.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.Perspective.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.Participation.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.Rehearsal.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.Witness.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.Transport.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.AdmissibleSurface.Residues.Select(ToResidueToken));
        residues.AddRange(localityState.AccountabilityPacket.Residues.Select(ToResidueToken));
        return residues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ResolveSerialLocalityHandle(SliExecutionContext context)
    {
        return string.IsNullOrWhiteSpace(context.HigherOrderLocalityState.LocalityHandle)
            ? $"locality:{context.Frame.CMEId}:{context.Frame.ContextId:D}"
            : context.HigherOrderLocalityState.LocalityHandle;
    }

    private static string ToResidueToken(HigherOrderLocalityResidue residue)
    {
        return $"{residue.Kind}:{residue.Source}:{residue.Detail}";
    }
}
