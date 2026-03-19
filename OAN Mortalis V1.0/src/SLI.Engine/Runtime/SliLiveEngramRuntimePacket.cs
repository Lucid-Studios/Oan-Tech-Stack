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
    Obstructed = 6
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
    public required string LocalityHandle { get; init; }
    public required string SourceHandle { get; init; }
    public required IReadOnlyList<string> InvariantSet { get; init; }
    public required IReadOnlyList<string> ResidueSet { get; init; }
    public required IReadOnlyList<string> WitnessSet { get; init; }
    public required IReadOnlyList<SliLiveEngramTraceEntry> TraceSet { get; init; }
    public required bool ReturnCandidateEligible { get; init; }
    public required string ReturnEligibilityReason { get; init; }
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

        var localityHandle = ResolveLocalityHandle(context);
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

    public static SliLiveEngramRuntimePacket CreateForHigherOrderLocality(SliExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var localityState = context.HigherOrderLocalityState;
        var localityHandle = ResolveLocalityHandle(context);
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

    private static IReadOnlyList<string> CollectCognitionResidues(SliExecutionContext context)
    {
        var residues = new List<string>();
        residues.AddRange(context.PrunedBranches.Select(branch => $"cleave:{branch}"));
        residues.AddRange(CollectHigherOrderResidues(context.HigherOrderLocalityState));
        return residues;
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

    private static string ResolveLocalityHandle(SliExecutionContext context)
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
