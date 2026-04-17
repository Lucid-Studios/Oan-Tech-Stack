using System.Text.Json.Serialization;

namespace San.Common;

public enum AgentLaneKind
{
    Integrator = 0,
    InnerBuilder = 1,
    OuterBuilder = 2,
    Witness = 3
}

public enum AgentReturnDisposition
{
    Admit = 0,
    Hold = 1,
    Narrow = 2,
    Return = 3,
    Refuse = 4,
    Escalate = 5
}

public enum AgentBarrierKind
{
    WriteScopeOverlap = 0,
    ContractBarrier = 1,
    EvidenceBoundaryDrift = 2,
    BuildFailure = 3,
    TestFailure = 4,
    GovernanceOverclaim = 5,
    RuntimeAuthorityWidening = 6
}

public enum AgentExecutionStatus
{
    NotRun = 0,
    Passed = 1,
    Failed = 2
}

public sealed record AgentWorkLaneDefinition(
    AgentLaneKind Lane,
    string LaneId,
    string Label,
    IReadOnlyList<string> OwnedBuckets,
    IReadOnlyList<string> OwnedWriteSurfaces,
    IReadOnlyList<string> ForbiddenOverlappingSurfaces,
    IReadOnlyList<string> RequiredAcceptanceChecks);

public sealed record AgentWorkRequest(
    [property: JsonPropertyName("lane")] AgentLaneKind Lane,
    [property: JsonPropertyName("sliceId")] string SliceId,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("predicate")] string Predicate,
    [property: JsonPropertyName("actions")] IReadOnlyList<string> Actions,
    [property: JsonPropertyName("targetBuckets")] IReadOnlyList<string> TargetBuckets,
    [property: JsonPropertyName("ownedWriteScope")] IReadOnlyList<string> OwnedWriteScope,
    [property: JsonPropertyName("acceptanceChecks")] IReadOnlyList<string> AcceptanceChecks);

public sealed record AgentWorkReturnReceipt(
    [property: JsonPropertyName("lane")] AgentLaneKind Lane,
    [property: JsonPropertyName("sliceId")] string SliceId,
    [property: JsonPropertyName("touchedSurfaces")] IReadOnlyList<string> TouchedSurfaces,
    [property: JsonPropertyName("writeScopeSatisfied")] bool WriteScopeSatisfied,
    [property: JsonPropertyName("buildStatus")] AgentExecutionStatus BuildStatus,
    [property: JsonPropertyName("testStatus")] AgentExecutionStatus TestStatus,
    [property: JsonPropertyName("barrierKinds")] IReadOnlyList<AgentBarrierKind> BarrierKinds,
    [property: JsonPropertyName("recommendedDisposition")] AgentReturnDisposition RecommendedDisposition,
    [property: JsonPropertyName("evidenceArtifacts")] IReadOnlyList<string> EvidenceArtifacts,
    [property: JsonPropertyName("notes")] IReadOnlyList<string> Notes);

public sealed record AgentIntegrationGatePolicy(
    string OperationalModel,
    string IntegrationCadence,
    IReadOnlyList<string> IntegratorOwnedSharedSurfaces,
    IReadOnlyList<AgentBarrierKind> WitnessBlockingBarriers,
    bool ReturnsRequired,
    bool SharedWriteScopesAllowed,
    bool IntegratorOnlySharedTruthMutation);

public static class AgentBuildOrchestrationContracts
{
    public static readonly IReadOnlyList<AgentWorkLaneDefinition> DefaultLanes =
    [
        CreateLane(
            AgentLaneKind.Integrator,
            "integrator",
            "Integrator",
            [
                "build-governance-automation",
                "cradletek-cryptic-substrate",
                "soulframe-office-governance",
                "agenticore-runtime-harness",
                "sli-lisp-topology",
                "oan-runtime-composition",
                "documentation-and-research"
            ],
            [
                "OAN Mortalis V1.1.1/build/agent-work-lanes.json",
                "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md",
                "OAN Mortalis V1.1.1/docs/INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md"
            ],
            [
                "worker-lane-owned-surfaces-during-active-slice"
            ],
            [
                "receipted-worker-return",
                "witness-review-complete",
                "shared-truth-reconciliation"
            ]),
        CreateLane(
            AgentLaneKind.InnerBuilder,
            "inner-builder",
            "Inner Builder",
            [
                "cradletek-cryptic-substrate",
                "soulframe-office-governance",
                "oan-runtime-composition"
            ],
            [
                "OAN Mortalis V1.1.1/src/TechStack/GEL/*",
                "OAN Mortalis V1.1.1/src/TechStack/CradleTek/*",
                "OAN Mortalis V1.1.1/src/TechStack/SoulFrame/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.Common/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.FirstRun/*",
                "OAN Mortalis V1.1.1/docs/GEL_*",
                "OAN Mortalis V1.1.1/docs/SELFGEL_*",
                "OAN Mortalis V1.1.1/docs/FIRST_RUN_*",
                "OAN Mortalis V1.1.1/docs/A0_*",
                "OAN Mortalis V1.1.1/docs/PRE_LISP_*",
                "OAN Mortalis V1.1.1/docs/PRIME_*",
                "OAN Mortalis V1.1.1/docs/PROC_*",
                "OAN Mortalis V1.1.1/docs/IGNITION_*",
                "OAN Mortalis V1.1.1/docs/ASSIMILATION_*",
                "OAN Mortalis V1.1.1/docs/LIGHT_*",
                "OAN Mortalis V1.1.1/docs/EC_*"
            ],
            [
                "OAN Mortalis V1.1.1/build/agent-work-lanes.json",
                "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md",
                "OAN Mortalis V1.1.1/src/TechStack/AgentiCore/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/SLI.*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.HostedLlm/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.Runtime.*/*",
                "OAN Mortalis V1.1.1/tests/Sanctuary/*"
            ],
            [
                "contract-alignment",
                "audit-update-when-needed",
                "write-scope-satisfied"
            ]),
        CreateLane(
            AgentLaneKind.OuterBuilder,
            "outer-builder",
            "Outer Builder",
            [
                "agenticore-runtime-harness",
                "sli-lisp-topology",
                "oan-runtime-composition"
            ],
            [
                "OAN Mortalis V1.1.1/src/TechStack/AgentiCore/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/SLI.*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.HostedLlm/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.PrimeCryptic.Services/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.Runtime.*/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.State.Modulation/*",
                "OAN Mortalis V1.1.1/docs/HOSTED_LLM_*",
                "OAN Mortalis V1.1.1/docs/LLM_*",
                "OAN Mortalis V1.1.1/docs/RESPONSES_API_*",
                "OAN Mortalis V1.1.1/docs/RUNTIME_*"
            ],
            [
                "OAN Mortalis V1.1.1/build/agent-work-lanes.json",
                "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md",
                "OAN Mortalis V1.1.1/src/TechStack/GEL/*",
                "OAN Mortalis V1.1.1/src/TechStack/CradleTek/*",
                "OAN Mortalis V1.1.1/src/TechStack/SoulFrame/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.Common/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/San.FirstRun/*",
                "OAN Mortalis V1.1.1/tests/Sanctuary/*"
            ],
            [
                "runtime-surface-alignment",
                "receipt-emission-alignment",
                "write-scope-satisfied"
            ]),
        CreateLane(
            AgentLaneKind.Witness,
            "witness",
            "Witness",
            [
                "build-governance-automation",
                "oan-runtime-composition"
            ],
            [
                "OAN Mortalis V1.1.1/build/*",
                "OAN Mortalis V1.1.1/tests/Sanctuary/*"
            ],
            [
                "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md",
                "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md",
                "OAN Mortalis V1.1.1/src/TechStack/GEL/*",
                "OAN Mortalis V1.1.1/src/TechStack/CradleTek/*",
                "OAN Mortalis V1.1.1/src/TechStack/SoulFrame/*",
                "OAN Mortalis V1.1.1/src/TechStack/AgentiCore/*",
                "OAN Mortalis V1.1.1/src/Sanctuary/SLI.*",
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.*"
            ],
            [
                "build",
                "test",
                "hygiene",
                "barrier-classification"
            ])
    ];

    public static readonly AgentIntegrationGatePolicy DefaultIntegrationGatePolicy = new(
        OperationalModel: "manual-integrator-mediated-subagent-orchestration",
        IntegrationCadence: "receipt-by-slice",
        IntegratorOwnedSharedSurfaces:
        [
            "OAN Mortalis V1.1.1/build/agent-work-lanes.json",
            "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md",
            "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md",
            "OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md",
            "OAN Mortalis V1.1.1/docs/MASTER_THREAD_BUCKET_ORCHESTRATION_LAW.md",
            "OAN Mortalis V1.1.1/docs/LOCAL_AUTOMATION_TASKING_SURFACE.md",
            "OAN Mortalis V1.1.1/docs/WORKSPACE_BUCKET_GROUP_SYSTEM.md",
            "OAN Mortalis V1.1.1/docs/INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md"
        ],
        WitnessBlockingBarriers: Enum.GetValues<AgentBarrierKind>(),
        ReturnsRequired: true,
        SharedWriteScopesAllowed: false,
        IntegratorOnlySharedTruthMutation: true);

    public static AgentWorkLaneDefinition GetLane(AgentLaneKind lane)
        => DefaultLanes.First(item => item.Lane == lane);

    public static bool CanRunConcurrently(AgentWorkRequest left, AgentWorkRequest right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftScope = NormalizeTokens(left.OwnedWriteScope);
        var rightScope = NormalizeTokens(right.OwnedWriteScope);

        return leftScope.Intersect(rightScope, StringComparer.OrdinalIgnoreCase).Any() is false;
    }

    public static bool RequiresWitnessBlock(
        AgentWorkReturnReceipt receipt,
        AgentIntegrationGatePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        var effectivePolicy = policy ?? DefaultIntegrationGatePolicy;
        var barriers = NormalizeBarrierKinds(receipt.BarrierKinds);

        return barriers.Intersect(effectivePolicy.WitnessBlockingBarriers).Any() ||
               receipt.BuildStatus == AgentExecutionStatus.Failed ||
               receipt.TestStatus == AgentExecutionStatus.Failed;
    }

    public static AgentReturnDisposition ResolveIntegratorDisposition(
        AgentWorkLaneDefinition lane,
        AgentWorkReturnReceipt receipt,
        AgentIntegrationGatePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(lane);
        ArgumentNullException.ThrowIfNull(receipt);

        if (lane.Lane != receipt.Lane)
        {
            throw new InvalidOperationException("Agent lane and return receipt lane must match.");
        }

        var effectivePolicy = policy ?? DefaultIntegrationGatePolicy;
        var barriers = NormalizeBarrierKinds(receipt.BarrierKinds);
        var touchedSurfaces = NormalizeTokens(receipt.TouchedSurfaces);

        if (lane.Lane != AgentLaneKind.Integrator &&
            touchedSurfaces.Intersect(effectivePolicy.IntegratorOwnedSharedSurfaces, StringComparer.OrdinalIgnoreCase).Any())
        {
            return AgentReturnDisposition.Refuse;
        }

        if (!receipt.WriteScopeSatisfied)
        {
            return AgentReturnDisposition.Return;
        }

        if (receipt.BuildStatus == AgentExecutionStatus.Failed ||
            receipt.TestStatus == AgentExecutionStatus.Failed ||
            barriers.Contains(AgentBarrierKind.BuildFailure) ||
            barriers.Contains(AgentBarrierKind.TestFailure))
        {
            return AgentReturnDisposition.Hold;
        }

        if (barriers.Contains(AgentBarrierKind.WriteScopeOverlap))
        {
            return AgentReturnDisposition.Return;
        }

        if (barriers.Contains(AgentBarrierKind.ContractBarrier) ||
            barriers.Contains(AgentBarrierKind.EvidenceBoundaryDrift) ||
            barriers.Contains(AgentBarrierKind.GovernanceOverclaim) ||
            barriers.Contains(AgentBarrierKind.RuntimeAuthorityWidening))
        {
            return AgentReturnDisposition.Escalate;
        }

        return receipt.RecommendedDisposition;
    }

    private static AgentWorkLaneDefinition CreateLane(
        AgentLaneKind lane,
        string laneId,
        string label,
        IReadOnlyList<string> ownedBuckets,
        IReadOnlyList<string> ownedWriteSurfaces,
        IReadOnlyList<string> forbiddenOverlappingSurfaces,
        IReadOnlyList<string> requiredAcceptanceChecks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(laneId);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        return new AgentWorkLaneDefinition(
            Lane: lane,
            LaneId: laneId.Trim(),
            Label: label.Trim(),
            OwnedBuckets: NormalizeTokens(ownedBuckets),
            OwnedWriteSurfaces: NormalizeTokens(ownedWriteSurfaces),
            ForbiddenOverlappingSurfaces: NormalizeTokens(forbiddenOverlappingSurfaces),
            RequiredAcceptanceChecks: NormalizeTokens(requiredAcceptanceChecks));
    }

    private static IReadOnlyList<string> NormalizeTokens(IReadOnlyList<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<AgentBarrierKind> NormalizeBarrierKinds(IReadOnlyList<AgentBarrierKind>? values)
    {
        return (values ?? Array.Empty<AgentBarrierKind>())
            .Distinct()
            .ToArray();
    }
}
