using System.Text.Json;
using San.Common;

namespace San.Audit.Tests;

public sealed class AgentBuildOrchestrationContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Agent_Build_Orchestration_Enums_And_Lanes_Are_Exact()
    {
        Assert.Equal(
            [
                AgentLaneKind.Integrator,
                AgentLaneKind.InnerBuilder,
                AgentLaneKind.OuterBuilder,
                AgentLaneKind.Witness
            ],
            Enum.GetValues<AgentLaneKind>());

        Assert.Equal(
            [
                AgentReturnDisposition.Admit,
                AgentReturnDisposition.Hold,
                AgentReturnDisposition.Narrow,
                AgentReturnDisposition.Return,
                AgentReturnDisposition.Refuse,
                AgentReturnDisposition.Escalate
            ],
            Enum.GetValues<AgentReturnDisposition>());

        Assert.Equal(
            [
                AgentBarrierKind.WriteScopeOverlap,
                AgentBarrierKind.ContractBarrier,
                AgentBarrierKind.EvidenceBoundaryDrift,
                AgentBarrierKind.BuildFailure,
                AgentBarrierKind.TestFailure,
                AgentBarrierKind.GovernanceOverclaim,
                AgentBarrierKind.RuntimeAuthorityWidening
            ],
            Enum.GetValues<AgentBarrierKind>());

        Assert.Equal(
            [
                AgentExecutionStatus.NotRun,
                AgentExecutionStatus.Passed,
                AgentExecutionStatus.Failed
            ],
            Enum.GetValues<AgentExecutionStatus>());

        Assert.Equal(
            [
                AgentLaneKind.Integrator,
                AgentLaneKind.InnerBuilder,
                AgentLaneKind.OuterBuilder,
                AgentLaneKind.Witness
            ],
            AgentBuildOrchestrationContracts.DefaultLanes.Select(static item => item.Lane).ToArray());

        AssertLane(
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
            ]);
        AssertLane(
            AgentLaneKind.InnerBuilder,
            "inner-builder",
            "Inner Builder",
            [
                "cradletek-cryptic-substrate",
                "soulframe-office-governance",
                "oan-runtime-composition"
            ]);

        var innerLane = AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.InnerBuilder);
        Assert.Contains("OAN Mortalis V1.1.1/docs/LIGHT_*", innerLane.OwnedWriteSurfaces);
        Assert.Contains("OAN Mortalis V1.1.1/docs/EC_*", innerLane.OwnedWriteSurfaces);
        AssertLane(
            AgentLaneKind.OuterBuilder,
            "outer-builder",
            "Outer Builder",
            [
                "agenticore-runtime-harness",
                "sli-lisp-topology",
                "oan-runtime-composition"
            ]);
        AssertLane(
            AgentLaneKind.Witness,
            "witness",
            "Witness",
            [
                "build-governance-automation",
                "oan-runtime-composition"
            ]);

        var policy = AgentBuildOrchestrationContracts.DefaultIntegrationGatePolicy;
        Assert.Equal("manual-integrator-mediated-subagent-orchestration", policy.OperationalModel);
        Assert.Equal("receipt-by-slice", policy.IntegrationCadence);
        Assert.Equal(
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
            policy.IntegratorOwnedSharedSurfaces);
        Assert.Equal(Enum.GetValues<AgentBarrierKind>(), policy.WitnessBlockingBarriers);
        Assert.True(policy.ReturnsRequired);
        Assert.False(policy.SharedWriteScopesAllowed);
        Assert.True(policy.IntegratorOnlySharedTruthMutation);
    }

    [Fact]
    public void Agent_Work_Request_And_Return_Receipts_Serialize_With_Exact_Field_Names()
    {
        var request = new AgentWorkRequest(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.001",
            Subject: "gel-root-family",
            Predicate: "contract-aligned",
            Actions: ["patch", "audit"],
            TargetBuckets: ["cradletek-cryptic-substrate"],
            OwnedWriteScope: ["OAN Mortalis V1.1.1/src/TechStack/GEL/*"],
            AcceptanceChecks: ["contract-alignment"]);

        var serializedRequest = JsonSerializer.Serialize(request);
        using var requestDocument = JsonDocument.Parse(serializedRequest);

        Assert.True(requestDocument.RootElement.TryGetProperty("lane", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("sliceId", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("subject", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("predicate", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("actions", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("targetBuckets", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("ownedWriteScope", out _));
        Assert.True(requestDocument.RootElement.TryGetProperty("acceptanceChecks", out _));

        var roundTrippedRequest = JsonSerializer.Deserialize<AgentWorkRequest>(serializedRequest, JsonOptions);
        Assert.NotNull(roundTrippedRequest);
        Assert.Equal(request.SliceId, roundTrippedRequest!.SliceId);
        Assert.Equal(request.OwnedWriteScope, roundTrippedRequest.OwnedWriteScope);

        var receipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.001",
            TouchedSurfaces: ["OAN Mortalis V1.1.1/src/TechStack/GEL/*"],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [],
            RecommendedDisposition: AgentReturnDisposition.Admit,
            EvidenceArtifacts: ["audit://slice.inner.001"],
            Notes: ["clean-return"]);

        var serializedReceipt = JsonSerializer.Serialize(receipt);
        using var receiptDocument = JsonDocument.Parse(serializedReceipt);

        Assert.True(receiptDocument.RootElement.TryGetProperty("lane", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("sliceId", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("touchedSurfaces", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("writeScopeSatisfied", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("buildStatus", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("testStatus", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("barrierKinds", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("recommendedDisposition", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("evidenceArtifacts", out _));
        Assert.True(receiptDocument.RootElement.TryGetProperty("notes", out _));

        var roundTrippedReceipt = JsonSerializer.Deserialize<AgentWorkReturnReceipt>(serializedReceipt, JsonOptions);
        Assert.NotNull(roundTrippedReceipt);
        Assert.Equal(receipt.SliceId, roundTrippedReceipt!.SliceId);
        Assert.Equal(receipt.RecommendedDisposition, roundTrippedReceipt.RecommendedDisposition);
    }

    [Fact]
    public void Inner_Only_Slice_With_Admitted_Return_Is_Lawful()
    {
        var lane = AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.InnerBuilder);
        var receipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.002",
            TouchedSurfaces: ["OAN Mortalis V1.1.1/src/TechStack/GEL/*"],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [],
            RecommendedDisposition: AgentReturnDisposition.Admit,
            EvidenceArtifacts: ["audit://slice.inner.002"],
            Notes: ["inner-admit"]);

        Assert.False(AgentBuildOrchestrationContracts.RequiresWitnessBlock(receipt));
        Assert.Equal(
            AgentReturnDisposition.Admit,
            AgentBuildOrchestrationContracts.ResolveIntegratorDisposition(lane, receipt));
    }

    [Fact]
    public void Outer_Only_Slice_With_Admitted_Return_Is_Lawful()
    {
        var lane = AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.OuterBuilder);
        var receipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.OuterBuilder,
            SliceId: "slice.outer.001",
            TouchedSurfaces: ["OAN Mortalis V1.1.1/src/Sanctuary/Oan.HostedLlm/*"],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [],
            RecommendedDisposition: AgentReturnDisposition.Admit,
            EvidenceArtifacts: ["audit://slice.outer.001"],
            Notes: ["outer-admit"]);

        Assert.False(AgentBuildOrchestrationContracts.RequiresWitnessBlock(receipt));
        Assert.Equal(
            AgentReturnDisposition.Admit,
            AgentBuildOrchestrationContracts.ResolveIntegratorDisposition(lane, receipt));
    }

    [Fact]
    public void Concurrent_Inner_And_Outer_Slices_With_Disjoint_Write_Scopes_Are_Admissible()
    {
        var inner = new AgentWorkRequest(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.concurrent",
            Subject: "gel-lattice",
            Predicate: "contract-backed",
            Actions: ["patch"],
            TargetBuckets: ["cradletek-cryptic-substrate"],
            OwnedWriteScope: ["OAN Mortalis V1.1.1/src/TechStack/GEL/*"],
            AcceptanceChecks: ["contract-alignment"]);
        var outer = new AgentWorkRequest(
            Lane: AgentLaneKind.OuterBuilder,
            SliceId: "slice.outer.concurrent",
            Subject: "hosted-llm-harness",
            Predicate: "runtime-aligned",
            Actions: ["patch"],
            TargetBuckets: ["agenticore-runtime-harness"],
            OwnedWriteScope: ["OAN Mortalis V1.1.1/src/Sanctuary/Oan.HostedLlm/*"],
            AcceptanceChecks: ["runtime-surface-alignment"]);

        Assert.True(AgentBuildOrchestrationContracts.CanRunConcurrently(inner, outer));
    }

    [Fact]
    public void Witness_Blocks_On_Evidence_Boundary_Drift()
    {
        var lane = AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.Witness);
        var receipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.Witness,
            SliceId: "slice.witness.001",
            TouchedSurfaces: ["OAN Mortalis V1.1.1/tests/Sanctuary/*"],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [AgentBarrierKind.EvidenceBoundaryDrift],
            RecommendedDisposition: AgentReturnDisposition.Hold,
            EvidenceArtifacts: ["audit://slice.witness.001"],
            Notes: ["boundary-drift"]);

        Assert.True(AgentBuildOrchestrationContracts.RequiresWitnessBlock(receipt));
        Assert.Equal(
            AgentReturnDisposition.Escalate,
            AgentBuildOrchestrationContracts.ResolveIntegratorDisposition(lane, receipt));
    }

    [Fact]
    public void CognitiveFormation_Minimal_Set_Is_Admissible_As_Inner_Slice_And_Receipted_By_Witness()
    {
        var innerRequest = new AgentWorkRequest(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.cognitive-formation-minimal",
            Subject: "cognitive-formation-minimal-set",
            Predicate: "deterministic-formation-reflex",
            Actions: ["patch", "contract", "receipt"],
            TargetBuckets: ["soulframe-office-governance", "oan-runtime-composition"],
            OwnedWriteScope:
            [
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.Common/Oan.Common/CognitiveFormationContracts.cs",
                "OAN Mortalis V1.1.1/docs/LIGHT_CONE_AWARENESS_LINEAGE_AND_LISTENING_FRAME_SOURCE_LAW.md",
                "OAN Mortalis V1.1.1/docs/EC_FORMATION_BUILDSPACE_PREPARATION_NOTE.md"
            ],
            AcceptanceChecks: ["contract-alignment", "audit-update-when-needed", "write-scope-satisfied"]);

        var outerRequest = new AgentWorkRequest(
            Lane: AgentLaneKind.OuterBuilder,
            SliceId: "slice.outer.hosted-surface",
            Subject: "hosted-llm-surface",
            Predicate: "runtime-surface-alignment",
            Actions: ["patch"],
            TargetBuckets: ["agenticore-runtime-harness"],
            OwnedWriteScope:
            [
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.HostedLlm/Oan.HostedLlm/*"
            ],
            AcceptanceChecks: ["runtime-surface-alignment"]);

        var witnessReceipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.Witness,
            SliceId: "slice.inner.cognitive-formation-minimal",
            TouchedSurfaces:
            [
                "OAN Mortalis V1.1.1/tests/Sanctuary/Oan.Audit.Tests/Oan.Audit.Tests/CognitiveFormationContractsTests.cs"
            ],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [],
            RecommendedDisposition: AgentReturnDisposition.Admit,
            EvidenceArtifacts:
            [
                "test://cognitive-formation-minimal",
                "receipt://formation/session-a"
            ],
            Notes:
            [
                "inner-slice-reviewed",
                "distinction-preserved",
                "false-promotion-blocked"
            ]);

        Assert.True(AgentBuildOrchestrationContracts.CanRunConcurrently(innerRequest, outerRequest));
        Assert.False(AgentBuildOrchestrationContracts.RequiresWitnessBlock(witnessReceipt));
        Assert.Equal(
            AgentReturnDisposition.Admit,
            AgentBuildOrchestrationContracts.ResolveIntegratorDisposition(
                AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.Witness),
                witnessReceipt));
    }

    [Fact]
    public void Overlapping_CognitiveFormation_WriteScope_Is_Blocked_Before_Concurrent_Run()
    {
        var innerRequest = new AgentWorkRequest(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.cognitive-formation",
            Subject: "cognitive-formation",
            Predicate: "lawful-reflex",
            Actions: ["patch"],
            TargetBuckets: ["oan-runtime-composition"],
            OwnedWriteScope:
            [
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.Common/Oan.Common/CognitiveFormationContracts.cs"
            ],
            AcceptanceChecks: ["contract-alignment"]);

        var outerRequest = new AgentWorkRequest(
            Lane: AgentLaneKind.OuterBuilder,
            SliceId: "slice.outer.illegal-overlap",
            Subject: "misrouted-cognitive-formation-touch",
            Predicate: "runtime-authority-widening-risk",
            Actions: ["patch"],
            TargetBuckets: ["agenticore-runtime-harness"],
            OwnedWriteScope:
            [
                "OAN Mortalis V1.1.1/src/Sanctuary/Oan.Common/Oan.Common/CognitiveFormationContracts.cs"
            ],
            AcceptanceChecks: ["runtime-surface-alignment"]);

        Assert.False(AgentBuildOrchestrationContracts.CanRunConcurrently(innerRequest, outerRequest));
    }

    [Fact]
    public void Worker_Return_Is_Refused_When_It_Touches_An_Integrator_Owned_Surface()
    {
        var lane = AgentBuildOrchestrationContracts.GetLane(AgentLaneKind.InnerBuilder);
        var receipt = new AgentWorkReturnReceipt(
            Lane: AgentLaneKind.InnerBuilder,
            SliceId: "slice.inner.shared-surface",
            TouchedSurfaces:
            [
                "OAN Mortalis V1.1.1/docs/BUILD_READINESS.md"
            ],
            WriteScopeSatisfied: true,
            BuildStatus: AgentExecutionStatus.Passed,
            TestStatus: AgentExecutionStatus.Passed,
            BarrierKinds: [],
            RecommendedDisposition: AgentReturnDisposition.Admit,
            EvidenceArtifacts: ["audit://slice.inner.shared-surface"],
            Notes: ["touched-shared-surface"]);

        Assert.Equal(
            AgentReturnDisposition.Refuse,
            AgentBuildOrchestrationContracts.ResolveIntegratorDisposition(lane, receipt));
    }

    [Fact]
    public void Agent_Work_Lanes_Json_And_Docs_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildRoot = Path.Combine(lineRoot, "build");
        var docsRoot = Path.Combine(lineRoot, "docs");
        var jsonPath = Path.Combine(buildRoot, "agent-work-lanes.json");
        var buildReadinessPath = Path.Combine(docsRoot, "BUILD_READINESS.md");
        var orchestrationLawPath = Path.Combine(docsRoot, "INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md");
        var carryForwardPath = Path.Combine(docsRoot, "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(docsRoot, "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        using var document = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var root = document.RootElement;

        Assert.Equal("manual-integrator-mediated-subagent-orchestration", root.GetProperty("operationalModel").GetString());
        Assert.Equal("receipt-by-slice", root.GetProperty("integrationCadence").GetString());
        Assert.Equal("shared-line-verification-lock", root.GetProperty("verificationExecutionModel").GetString());
        Assert.Equal("tools/Use-LineVerificationLock.ps1", root.GetProperty("verificationLockScriptPath").GetString());
        Assert.Equal(
            [
                "build.ps1",
                "test.ps1"
            ],
            root.GetProperty("serializedVerificationScripts").EnumerateArray().Select(static item => item.GetString()).ToArray());
        Assert.Equal(
            [
                "lane",
                "sliceId",
                "subject",
                "predicate",
                "actions",
                "targetBuckets",
                "ownedWriteScope",
                "acceptanceChecks"
            ],
            root.GetProperty("requestEnvelopeFields").EnumerateArray().Select(static item => item.GetString()).ToArray());
        Assert.Equal(
            [
                "lane",
                "sliceId",
                "touchedSurfaces",
                "writeScopeSatisfied",
                "buildStatus",
                "testStatus",
                "barrierKinds",
                "recommendedDisposition",
                "evidenceArtifacts",
                "notes"
            ],
            root.GetProperty("returnReceiptFields").EnumerateArray().Select(static item => item.GetString()).ToArray());
        Assert.Equal(
            [
                "integrator",
                "inner-builder",
                "outer-builder",
                "witness"
            ],
            root.GetProperty("lanes").EnumerateArray().Select(static item => item.GetProperty("laneId").GetString()).ToArray());

        var lawText = File.ReadAllText(orchestrationLawPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("These lanes target existing workspace buckets.", lawText, StringComparison.Ordinal);
        Assert.Contains("Worker output without a lawful return receipt is not admissible build", lawText, StringComparison.Ordinal);
        Assert.Contains("Only the `Integrator` may mutate shared readiness, carry-forward, and", lawText, StringComparison.Ordinal);
        Assert.Contains("`Witness` blocks at barriers.", lawText, StringComparison.Ordinal);
        Assert.Contains("## Shared Verification Lane", lawText, StringComparison.Ordinal);
        Assert.Contains("shared line verification lock", lawText, StringComparison.Ordinal);
        Assert.Contains("tools/Use-LineVerificationLock.ps1", lawText, StringComparison.Ordinal);
        Assert.Contains("The first implementation is manual subagent orchestration only.", lawText, StringComparison.Ordinal);
        Assert.Contains("peer-to-peer worker integration", lawText, StringComparison.Ordinal);
        Assert.Contains("shared writes", lawText, StringComparison.Ordinal);
        Assert.Contains("autonomous worker promotion", lawText, StringComparison.Ordinal);
        Assert.Contains("native scheduling dependence", lawText, StringComparison.Ordinal);
        Assert.Contains("a separate condensation lane", lawText, StringComparison.Ordinal);

        Assert.Contains("INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("inner-outer-witness-agent-build-orchestration-law: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("../tools/Use-LineVerificationLock.ps1", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("shared-line-verification-lock: verify-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("manual inner / outer / witness agent build orchestration and slice-receipt policy", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("shared line-verification lock between repo-root build/test wrappers", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("automation-first multi-agent actuation beyond bounded manual orchestration", refinementText, StringComparison.Ordinal);
        Assert.Contains("true separate-artifact concurrent build/test execution", refinementText, StringComparison.Ordinal);
    }

    private static void AssertLane(
        AgentLaneKind lane,
        string laneId,
        string label,
        IReadOnlyList<string> ownedBuckets)
    {
        var definition = AgentBuildOrchestrationContracts.GetLane(lane);
        Assert.Equal(laneId, definition.LaneId);
        Assert.Equal(label, definition.Label);
        Assert.Equal(ownedBuckets, definition.OwnedBuckets);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Mortalis V1.1.1", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the V1.1.1 line root.");
    }
}
