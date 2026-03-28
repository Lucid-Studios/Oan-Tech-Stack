using Oan.Common;

namespace SLI.Engine.Runtime;

internal enum SliActualizationClaimClass
{
    NonSelfOperational = 0,
    SelfImplicating = 1,
    RoleImplicating = 2,
    PermissionImplicating = 3,
    Autobiographical = 4
}

internal enum SliActualizationContradictionClass
{
    None = 0,
    SelfValidationConflict = 1,
    RoleValidationConflict = 2,
    PermissionValidationConflict = 3,
    ContinuityValidationConflict = 4
}

internal enum SliActualizationValidationRoute
{
    NoneRequired = 0,
    CooledSelfGelValidation = 1,
    NonSelfGovernanceValidation = 2,
    SoulFrameContinuityMediation = 3,
    ResidueOnly = 4,
    Obstructed = 5
}

internal enum SliActualizationDisposition
{
    Actualized = 0,
    CandidateBearing = 1,
    ResidueBearing = 2,
    Obstructed = 3,
    Deferred = 4
}

internal enum SliActualizationStageKind
{
    Bloom = 0,
    Seal = 1,
    Witness = 2,
    Ingest = 3,
    Cleave = 4,
    Commit = 5
}

internal sealed record SliActualizationWebbingEvent(
    SliActualizationStageKind Stage,
    string Detail,
    string ShardId,
    string LocalityHandle,
    string CycleMarker);

internal sealed class SliActualizationWebbingPacket
{
    public required string ActualizationHandle { get; init; }
    public required string ExecutionId { get; init; }
    public required string Objective { get; init; }
    public required SliActualizationClaimClass ClaimClass { get; init; }
    public required SliActualizationContradictionClass ContradictionClass { get; init; }
    public required SliActualizationValidationRoute ValidationRoute { get; init; }
    public required SliActualizationDisposition Disposition { get; init; }
    public required bool SelfValidationRequired { get; init; }
    public required bool CandidateEngramBearing { get; init; }
    public required bool AutobiographicalBearing { get; init; }
    public required SliLocalityRelationOutcomeKind? ReductionOutcome { get; init; }
    public required string ReductionReason { get; init; }
    public required IReadOnlyList<string> ResidueSet { get; init; }
    public required IReadOnlyList<SliActualizationWebbingEvent> WebbingEvents { get; init; }
}

internal static class SliActualizationWebbingPacketFactory
{
    public static SliActualizationWebbingPacket CreateForCognition(
        SliExecutionContext context,
        string traceId,
        ZedThetaCandidateReceipt candidate)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentNullException.ThrowIfNull(candidate);

        var claimClass = ResolveClaimClass(context);
        var contradictionClass = ResolveContradictionClass(context, candidate, claimClass);
        SliLocalityRelationOutcomeKind? reductionOutcome = context.ShardModeEnabled
            ? ResolveReductionOutcome(context.LocalityRelationEvents)
            : null;
        var reductionReason = reductionOutcome is null
            ? "serial-runtime"
            : ResolveReductionReason(context.LocalityRelationEvents, reductionOutcome.Value);
        var validationRoute = ResolveValidationRoute(claimClass, contradictionClass, reductionOutcome);
        var residueSet = CollectResidues(context, contradictionClass, reductionOutcome, reductionReason);
        var candidateBearing =
            contradictionClass == SliActualizationContradictionClass.None &&
            candidate.BridgeReview?.OutcomeKind == SliBridgeOutcomeKind.Ok &&
            candidate.RuntimeUseCeiling?.CandidateOnly == true;
        var disposition = ResolveDisposition(candidateBearing, contradictionClass, reductionOutcome, residueSet.Count);

        return new SliActualizationWebbingPacket
        {
            ActualizationHandle = $"actualization:{traceId}",
            ExecutionId = context.ExecutionId,
            Objective = context.Frame.TaskObjective,
            ClaimClass = claimClass,
            ContradictionClass = contradictionClass,
            ValidationRoute = validationRoute,
            Disposition = disposition,
            SelfValidationRequired = RequiresSelfValidation(claimClass),
            CandidateEngramBearing = candidateBearing,
            AutobiographicalBearing = claimClass == SliActualizationClaimClass.Autobiographical,
            ReductionOutcome = reductionOutcome,
            ReductionReason = reductionReason,
            ResidueSet = residueSet,
            WebbingEvents = context.ActualizationWebbingEvents.ToArray()
        };
    }

    private static SliActualizationClaimClass ResolveClaimClass(SliExecutionContext context)
    {
        var objective = context.Frame.TaskObjective;
        if (ContainsAny(objective, "autobiography", "autobiographical", "self-history", "memory"))
        {
            return SliActualizationClaimClass.Autobiographical;
        }

        if (ContainsAny(objective, "operator", "steward", "mother", "father", "role", "office"))
        {
            return SliActualizationClaimClass.RoleImplicating;
        }

        if (ContainsAny(objective, "permission", "authorize", "authorization", "authority", "grant"))
        {
            return SliActualizationClaimClass.PermissionImplicating;
        }

        var selfHint = context.Frame.SelfStateHint;
        if (ContainsAny(objective, "self", "identity", "continuity") ||
            (selfHint is not null && (selfHint.ClaimCount > 0 || selfHint.ValidationConceptCount > 0)))
        {
            return SliActualizationClaimClass.SelfImplicating;
        }

        return SliActualizationClaimClass.NonSelfOperational;
    }

    private static SliActualizationContradictionClass ResolveContradictionClass(
        SliExecutionContext context,
        ZedThetaCandidateReceipt candidate,
        SliActualizationClaimClass claimClass)
    {
        var hint = context.Frame.SelfStateHint;
        return claimClass switch
        {
            SliActualizationClaimClass.Autobiographical or SliActualizationClaimClass.SelfImplicating
                when hint?.HasDeferredOrContradictedClaim == true =>
                SliActualizationContradictionClass.SelfValidationConflict,
            SliActualizationClaimClass.Autobiographical or SliActualizationClaimClass.SelfImplicating
                when candidate.BridgeReview is not null &&
                     candidate.BridgeReview.OutcomeKind != SliBridgeOutcomeKind.Ok =>
                SliActualizationContradictionClass.ContinuityValidationConflict,
            SliActualizationClaimClass.RoleImplicating
                when hint?.HasDeferredOrContradictedClaim == true =>
                SliActualizationContradictionClass.RoleValidationConflict,
            SliActualizationClaimClass.PermissionImplicating
                when candidate.BridgeReview is not null &&
                     candidate.BridgeReview.OutcomeKind != SliBridgeOutcomeKind.Ok =>
                SliActualizationContradictionClass.PermissionValidationConflict,
            _ => SliActualizationContradictionClass.None
        };
    }

    private static SliActualizationValidationRoute ResolveValidationRoute(
        SliActualizationClaimClass claimClass,
        SliActualizationContradictionClass contradictionClass,
        SliLocalityRelationOutcomeKind? reductionOutcome)
    {
        if (contradictionClass is SliActualizationContradictionClass.SelfValidationConflict or
            SliActualizationContradictionClass.ContinuityValidationConflict)
        {
            return SliActualizationValidationRoute.SoulFrameContinuityMediation;
        }

        if (contradictionClass is SliActualizationContradictionClass.RoleValidationConflict or
            SliActualizationContradictionClass.PermissionValidationConflict)
        {
            return SliActualizationValidationRoute.NonSelfGovernanceValidation;
        }

        if (reductionOutcome == SliLocalityRelationOutcomeKind.Obstructed ||
            reductionOutcome == SliLocalityRelationOutcomeKind.Refused)
        {
            return SliActualizationValidationRoute.Obstructed;
        }

        return claimClass switch
        {
            SliActualizationClaimClass.Autobiographical or SliActualizationClaimClass.SelfImplicating =>
                SliActualizationValidationRoute.CooledSelfGelValidation,
            SliActualizationClaimClass.RoleImplicating or SliActualizationClaimClass.PermissionImplicating =>
                SliActualizationValidationRoute.NonSelfGovernanceValidation,
            _ => reductionOutcome == SliLocalityRelationOutcomeKind.Deferred
                ? SliActualizationValidationRoute.ResidueOnly
                : SliActualizationValidationRoute.NoneRequired
        };
    }

    private static SliActualizationDisposition ResolveDisposition(
        bool candidateBearing,
        SliActualizationContradictionClass contradictionClass,
        SliLocalityRelationOutcomeKind? reductionOutcome,
        int residueCount)
    {
        if (contradictionClass != SliActualizationContradictionClass.None ||
            reductionOutcome == SliLocalityRelationOutcomeKind.Obstructed ||
            reductionOutcome == SliLocalityRelationOutcomeKind.Refused)
        {
            return SliActualizationDisposition.Obstructed;
        }

        if (reductionOutcome == SliLocalityRelationOutcomeKind.Deferred)
        {
            return SliActualizationDisposition.Deferred;
        }

        if (candidateBearing)
        {
            return SliActualizationDisposition.CandidateBearing;
        }

        return residueCount > 0
            ? SliActualizationDisposition.ResidueBearing
            : SliActualizationDisposition.Actualized;
    }

    private static IReadOnlyList<string> CollectResidues(
        SliExecutionContext context,
        SliActualizationContradictionClass contradictionClass,
        SliLocalityRelationOutcomeKind? reductionOutcome,
        string reductionReason)
    {
        var residues = new List<string>();
        if (contradictionClass != SliActualizationContradictionClass.None)
        {
            residues.Add($"contradiction:{contradictionClass}");
        }

        residues.AddRange(context.PrunedBranches.Select(branch => $"cleave:{branch}"));
        residues.AddRange(context.LocalityObstructions.Select(obstruction =>
            $"obstruction:{obstruction.AttemptedRelation}:{obstruction.ViolatedCondition}"));

        if (reductionOutcome is not null && reductionOutcome != SliLocalityRelationOutcomeKind.Joined)
        {
            residues.Add($"reduction:{reductionOutcome}:{reductionReason}");
        }

        return residues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool RequiresSelfValidation(SliActualizationClaimClass claimClass)
    {
        return claimClass is SliActualizationClaimClass.Autobiographical or
               SliActualizationClaimClass.SelfImplicating;
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

        if (required.Length == 2 && required.All(evt => evt.Outcome == SliLocalityRelationOutcomeKind.Joined))
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

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
