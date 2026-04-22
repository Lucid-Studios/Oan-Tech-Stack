using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryCondensationAndProceduralBasisContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Heat_And_Resonance_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryCrypticHeatSurfaceKind.CSelfGel,
                SanctuaryCrypticHeatSurfaceKind.COe,
                SanctuaryCrypticHeatSurfaceKind.CrypticLayer
            ],
            Enum.GetValues<SanctuaryCrypticHeatSurfaceKind>());

        Assert.Equal(
            [
                SanctuaryHeatCommitDecisionKind.Expand,
                SanctuaryHeatCommitDecisionKind.Hold,
                SanctuaryHeatCommitDecisionKind.PrepareCondensation
            ],
            Enum.GetValues<SanctuaryHeatCommitDecisionKind>());

        var discipline = SanctuaryHeatResonanceAtlas.HeatCommitDiscipline;
        Assert.Equal("heat.expand-before-commit.v0", discipline.Handle);
        Assert.Equal(
            [
                SanctuaryCrypticHeatSurfaceKind.CSelfGel,
                SanctuaryCrypticHeatSurfaceKind.COe,
                SanctuaryCrypticHeatSurfaceKind.CrypticLayer
            ],
            discipline.GoverningSurfaces);
        Assert.Equal(
            [
                "condense-structures",
                "merge-anchors",
                "finalize-definitions",
                "authorize-procedures-for-execution",
                "mint-final-seals"
            ],
            discipline.ForbiddenCommitActions);
        Assert.Equal(
            [
                "simulate",
                "decompose",
                "relate",
                "accumulate",
                "refine-provisionally"
            ],
            discipline.AllowedHeatActions);
        Assert.Equal("if-heat-and-stability-not-established-then-no-commit-and-expand", discipline.GoverningRule);
        Assert.Equal("no-commit-discipline-protects-truth-during-heat", discipline.TruthBoundary);
        Assert.Equal("placeholder-contract-only", discipline.OperationalStatus);

        var receipt = SanctuaryHeatResonanceAtlas.FieldStateReceipt;
        Assert.Equal("heat.field-state.receipt.v0", receipt.Handle);
        Assert.Equal(
            [
                "shell",
                "heat-intensity",
                "heat-color",
                "resonance",
                "harmonics",
                "decision"
            ],
            receipt.ReceiptFields);
        Assert.Equal(
            [
                SanctuaryHeatCommitDecisionKind.Expand,
                SanctuaryHeatCommitDecisionKind.Hold,
                SanctuaryHeatCommitDecisionKind.PrepareCondensation
            ],
            receipt.DecisionKinds);
        Assert.Equal("decision=expand|hold|prepare-condensation", receipt.DecisionGrammar);
        Assert.Equal("placeholder-contract-only", receipt.OperationalStatus);
    }

    [Fact]
    public void Stability_And_Condensation_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryStabilityMetricKind.Convergence,
                SanctuaryStabilityMetricKind.BoundaryStability,
                SanctuaryStabilityMetricKind.ConflictResolutionSaturation,
                SanctuaryStabilityMetricKind.TraceConsistency,
                SanctuaryStabilityMetricKind.LineageIntegrity,
                SanctuaryStabilityMetricKind.PostureStability
            ],
            Enum.GetValues<SanctuaryStabilityMetricKind>());

        Assert.Equal(
            [
                SanctuaryCondensationTargetKind.Root,
                SanctuaryCondensationTargetKind.Definition,
                SanctuaryCondensationTargetKind.Relation,
                SanctuaryCondensationTargetKind.Procedure
            ],
            Enum.GetValues<SanctuaryCondensationTargetKind>());

        var metrics = SanctuaryStabilityCondensationAtlas.Metrics;
        Assert.Equal(6, metrics.Count);
        Assert.Equal(SanctuaryStabilityMetricKind.Convergence, metrics[0].Metric);
        Assert.Equal("stability.convergence", metrics[0].Handle);
        Assert.Equal("repeated-lawful-processing-returns-the-same-shape", metrics[0].Meaning);
        Assert.Equal(SanctuaryStabilityMetricKind.PostureStability, metrics[5].Metric);
        Assert.Equal("stability.posture", metrics[5].Handle);
        Assert.Equal("prime-posture-remains-stable-through-processing", metrics[5].Meaning);

        var vector = SanctuaryStabilityCondensationAtlas.StabilityVector;
        Assert.Equal("stability.vector.v0", vector.Handle);
        Assert.Equal(
            [
                SanctuaryStabilityMetricKind.Convergence,
                SanctuaryStabilityMetricKind.BoundaryStability,
                SanctuaryStabilityMetricKind.ConflictResolutionSaturation,
                SanctuaryStabilityMetricKind.TraceConsistency,
                SanctuaryStabilityMetricKind.LineageIntegrity,
                SanctuaryStabilityMetricKind.PostureStability
            ],
            vector.OrderedMetrics);
        Assert.Equal("S(X)=(C,B,R,T,L,P)", vector.Formula);
        Assert.Equal("stability-does-not-override-truth", vector.TruthBoundary);
        Assert.Equal("placeholder-contract-only", vector.OperationalStatus);

        var threshold = SanctuaryStabilityCondensationAtlas.CondensationThreshold;
        Assert.Equal("condensation.threshold.v0", threshold.Handle);
        Assert.Equal(
            [
                "C>=theta_C",
                "B=1",
                "R>=theta_R",
                "T>=theta_T",
                "L=1",
                "P=1",
                "Phi(X)=1"
            ],
            threshold.ThresholdRules);
        Assert.Equal(
            [
                "keep-accumulating",
                "allow-refinement",
                "allow-decomposition"
            ],
            threshold.LowBandActions);
        Assert.Equal(
            [
                "begin-pruning",
                "test-alternate-refinements",
                "watch-divergence"
            ],
            threshold.MediumBandActions);
        Assert.Equal(
            [
                "condense",
                "canonicalize",
                "strengthen-anchor"
            ],
            threshold.HighBandActions);
        Assert.Equal("stability-does-not-override-truth", threshold.TruthBoundary);
        Assert.Equal("placeholder-contract-only", threshold.OperationalStatus);

        var output = SanctuaryStabilityCondensationAtlas.CanonicalOutput;
        Assert.Equal("condensation.output.v0", output.Handle);
        Assert.Equal(["id", "layer", "anchors", "role", "boundary"], output.PrimeFaceSlots);
        Assert.Equal(
            [
                "anchor-keys",
                "dependency-keys",
                "trace-keys",
                "condensate-seal"
            ],
            output.CrypticFaceSlots);
        Assert.Equal(
            [
                "event-id",
                "contributing-set",
                "stability-vector",
                "phi",
                "conflict-outcomes",
                "result=condensed"
            ],
            output.ReceiptSlots);
        Assert.Equal(
            [
                "allowed-ops",
                "layer-constraints",
                "posture-req",
                "scope"
            ],
            output.UsageContractSlots);
        Assert.Equal("anchor-lineage-must-be-preserved-exactly", output.LineageRule);
        Assert.Equal("placeholder-contract-only", output.OperationalStatus);

        var typeSpecific = SanctuaryStabilityCondensationAtlas.TypeSpecificOutputs;
        Assert.Equal(4, typeSpecific.Count);
        Assert.Equal(SanctuaryCondensationTargetKind.Root, typeSpecific[0].Target);
        Assert.Equal("r_c", typeSpecific[0].ResultHandle);
        Assert.Equal(SanctuaryCondensationTargetKind.Procedure, typeSpecific[3].Target);
        Assert.Equal("p_c", typeSpecific[3].ResultHandle);
        Assert.Equal(
            "keep-only-grounded-steps-compress-to-minimal-transform-high-trace-consistency-posture-gated",
            typeSpecific[3].GoverningRule);
    }

    [Fact]
    public void Procedural_Basis_And_Bounded_Ignition_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryProceduralBasisPrimitiveKind.Retain,
                SanctuaryProceduralBasisPrimitiveKind.Relate,
                SanctuaryProceduralBasisPrimitiveKind.Verify,
                SanctuaryProceduralBasisPrimitiveKind.Mint,
                SanctuaryProceduralBasisPrimitiveKind.Refuse,
                SanctuaryProceduralBasisPrimitiveKind.Preserve,
                SanctuaryProceduralBasisPrimitiveKind.Condense
            ],
            Enum.GetValues<SanctuaryProceduralBasisPrimitiveKind>());

        Assert.Equal(
            [
                SanctuaryProceduralCompositionPatternKind.Admission,
                SanctuaryProceduralCompositionPatternKind.Receipt,
                SanctuaryProceduralCompositionPatternKind.Relation,
                SanctuaryProceduralCompositionPatternKind.Prune,
                SanctuaryProceduralCompositionPatternKind.Condense
            ],
            Enum.GetValues<SanctuaryProceduralCompositionPatternKind>());

        Assert.Equal(
            [
                SanctuaryBoundedIgnitionProtocolStage.Attend,
                SanctuaryBoundedIgnitionProtocolStage.Reduce,
                SanctuaryBoundedIgnitionProtocolStage.Decompose,
                SanctuaryBoundedIgnitionProtocolStage.Discriminate,
                SanctuaryBoundedIgnitionProtocolStage.Anchor,
                SanctuaryBoundedIgnitionProtocolStage.BuildProvisionalChain,
                SanctuaryBoundedIgnitionProtocolStage.CheckPhi,
                SanctuaryBoundedIgnitionProtocolStage.Receipt
            ],
            Enum.GetValues<SanctuaryBoundedIgnitionProtocolStage>());

        var basis = SanctuaryProceduralBasisAtlas.ProceduralBasis;
        Assert.Equal("procedural-basis.condensate.v0", basis.Handle);
        Assert.Equal(
            [
                SanctuaryProceduralBasisPrimitiveKind.Retain,
                SanctuaryProceduralBasisPrimitiveKind.Relate,
                SanctuaryProceduralBasisPrimitiveKind.Verify,
                SanctuaryProceduralBasisPrimitiveKind.Mint,
                SanctuaryProceduralBasisPrimitiveKind.Refuse,
                SanctuaryProceduralBasisPrimitiveKind.Preserve,
                SanctuaryProceduralBasisPrimitiveKind.Condense
            ],
            basis.Primitives);
        Assert.Equal(
            [
                SanctuaryProceduralCompositionPatternKind.Admission,
                SanctuaryProceduralCompositionPatternKind.Receipt,
                SanctuaryProceduralCompositionPatternKind.Relation,
                SanctuaryProceduralCompositionPatternKind.Prune,
                SanctuaryProceduralCompositionPatternKind.Condense
            ],
            basis.CompositionPatterns);
        Assert.Equal("execute", basis.ExcludedFreePrimitive);
        Assert.Equal("all-primitives-are-posture-gated-and-grounded", basis.GatingRule);
        Assert.Equal("no-action-outside-this-set-may-be-retained-or-executed", basis.ExecutionBoundary);
        Assert.Equal("placeholder-contract-only", basis.OperationalStatus);

        var primeFace = SanctuaryProceduralBasisAtlas.PrimeFace;
        Assert.Equal("procedural-basis.prime-face.v0", primeFace.Handle);
        Assert.Equal("p_c", primeFace.Id);
        Assert.Equal("P", primeFace.Layer);
        Assert.Equal("proc-basis", primeFace.Role);
        Assert.Equal("Sigma_prime", primeFace.PostureGate);
        Assert.Equal("placeholder-contract-only", primeFace.OperationalStatus);

        var crypticFace = SanctuaryProceduralBasisAtlas.CrypticFace;
        Assert.Equal("procedural-basis.cryptic-face.v0", crypticFace.Handle);
        Assert.Equal(["anchor-keys", "dependency-keys", "trace-keys", "basis-seal"], crypticFace.Slots);
        Assert.Equal("cryptic-face-preserves-basis-lineage-and-trace", crypticFace.IntegrityRule);
        Assert.Equal("placeholder-contract-only", crypticFace.OperationalStatus);

        var basisReceipt = SanctuaryProceduralBasisAtlas.Receipt;
        Assert.Equal("procedural-basis.receipt.v0", basisReceipt.Handle);
        Assert.Equal(
            [
                "event-id",
                "source-set",
                "stability",
                "phi",
                "resolution",
                "result=condensed-basis"
            ],
            basisReceipt.ReceiptFields);
        Assert.Equal("condensed-basis", basisReceipt.Result);
        Assert.Equal("placeholder-contract-only", basisReceipt.OperationalStatus);

        var protocol = SanctuaryBoundedIgnitionTestAtlas.Protocol;
        Assert.Equal("bounded-ignition.protocol.v0", protocol.Handle);
        Assert.Equal(
            [
                SanctuaryBoundedIgnitionProtocolStage.Attend,
                SanctuaryBoundedIgnitionProtocolStage.Reduce,
                SanctuaryBoundedIgnitionProtocolStage.Decompose,
                SanctuaryBoundedIgnitionProtocolStage.Discriminate,
                SanctuaryBoundedIgnitionProtocolStage.Anchor,
                SanctuaryBoundedIgnitionProtocolStage.BuildProvisionalChain,
                SanctuaryBoundedIgnitionProtocolStage.CheckPhi,
                SanctuaryBoundedIgnitionProtocolStage.Receipt
            ],
            protocol.StageOrder);
        Assert.Equal(
            [
                "source-id",
                "root-candidates",
                "definition-candidates",
                "relation-candidates",
                "procedure-candidates",
                "anchor-sketch",
                "provisional-chain",
                "phi-read",
                "receipt-status"
            ],
            protocol.RecordFields);
        Assert.Equal(
            "the-first-coupling-event-verifies-posture-lineage-grounding-and-simulation-before-minting-a-live-seal",
            protocol.GuidingRule);
        Assert.Equal("provisional-chain-and-receipt-only-live-coupling-seal-deferred", protocol.ResultBoundary);
        Assert.Equal("placeholder-contract-only", protocol.OperationalStatus);

        var testSet = SanctuaryBoundedIgnitionTestAtlas.TestSet;
        Assert.Equal("bounded-ignition.test-set.v0", testSet.Handle);
        Assert.Equal(["T1", "T2", "T3", "T4", "T5"], testSet.ItemHandles);
        Assert.Equal("bounded-first-chain-test-with-live-seal-withheld", testSet.StagingBoundary);
        Assert.Equal("placeholder-contract-only", testSet.OperationalStatus);
    }

    [Fact]
    public void Condensation_And_Procedural_Basis_Contracts_Serialize_Stably()
    {
        var heat = SanctuaryHeatResonanceAtlas.HeatCommitDiscipline;
        var serializedHeat = JsonSerializer.Serialize(heat);
        var deserializedHeat = JsonSerializer.Deserialize<SanctuaryHeatCommitDisciplineDefinition>(serializedHeat, JsonOptions);

        Assert.NotNull(deserializedHeat);
        Assert.Equal(heat.Handle, deserializedHeat!.Handle);
        Assert.Equal(heat.ForbiddenCommitActions, deserializedHeat.ForbiddenCommitActions);

        var threshold = SanctuaryStabilityCondensationAtlas.CondensationThreshold;
        var serializedThreshold = JsonSerializer.Serialize(threshold);
        var deserializedThreshold = JsonSerializer.Deserialize<SanctuaryCondensationThresholdDefinition>(serializedThreshold, JsonOptions);

        Assert.NotNull(deserializedThreshold);
        Assert.Equal(threshold.Handle, deserializedThreshold!.Handle);
        Assert.Equal(threshold.ThresholdRules, deserializedThreshold.ThresholdRules);
        Assert.Equal(threshold.HighBandActions, deserializedThreshold.HighBandActions);

        var basis = SanctuaryProceduralBasisAtlas.ProceduralBasis;
        var serializedBasis = JsonSerializer.Serialize(basis);
        var deserializedBasis = JsonSerializer.Deserialize<SanctuaryProceduralBasisDefinition>(serializedBasis, JsonOptions);

        Assert.NotNull(deserializedBasis);
        Assert.Equal(basis.Handle, deserializedBasis!.Handle);
        Assert.Equal(basis.Primitives, deserializedBasis.Primitives);
        Assert.Equal(basis.ExecutionBoundary, deserializedBasis.ExecutionBoundary);

        var protocol = SanctuaryBoundedIgnitionTestAtlas.Protocol;
        var serializedProtocol = JsonSerializer.Serialize(protocol);
        var deserializedProtocol = JsonSerializer.Deserialize<SanctuaryBoundedIgnitionTestProtocolDefinition>(serializedProtocol, JsonOptions);

        Assert.NotNull(deserializedProtocol);
        Assert.Equal(protocol.Handle, deserializedProtocol!.Handle);
        Assert.Equal(protocol.StageOrder, deserializedProtocol.StageOrder);
        Assert.Equal(protocol.ResultBoundary, deserializedProtocol.ResultBoundary);
    }

    [Fact]
    public void Condensation_And_Procedural_Basis_Docs_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var heatPath = Path.Combine(lineRoot, "docs", "HEAT_RESONANCE_AND_EXPAND_BEFORE_COMMIT_LAW.md");
        var stabilityPath = Path.Combine(lineRoot, "docs", "STABILITY_METRICS_AND_CONDENSATION_THRESHOLD_NOTE.md");
        var canonicalPath = Path.Combine(lineRoot, "docs", "CANONICAL_CONDENSATION_OUTPUT_LAW.md");
        var basisPath = Path.Combine(lineRoot, "docs", "PROCEDURAL_BASIS_CONDENSATE_NOTE.md");
        var ignitionPath = Path.Combine(lineRoot, "docs", "FIRST_BOUNDED_IGNITION_CHAIN_TEST_PROTOCOL.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var heatText = File.ReadAllText(heatPath);
        var stabilityText = File.ReadAllText(stabilityPath);
        var canonicalText = File.ReadAllText(canonicalPath);
        var basisText = File.ReadAllText(basisPath);
        var ignitionText = File.ReadAllText(ignitionPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("HEAT_RESONANCE_AND_EXPAND_BEFORE_COMMIT_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("STABILITY_METRICS_AND_CONDENSATION_THRESHOLD_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CANONICAL_CONDENSATION_OUTPUT_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("PROCEDURAL_BASIS_CONDENSATE_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("FIRST_BOUNDED_IGNITION_CHAIN_TEST_PROTOCOL.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("heat-resonance-expand-before-commit-law: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("stability-metrics-condensation-threshold-note: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("canonical-condensation-output-law: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("procedural-basis-condensate-note: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first-bounded-ignition-chain-test-protocol: frame-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("embody expand-before-commit under heat", heatText, StringComparison.Ordinal);
        Assert.Contains("Under heat, `NoCommit` means do not:", heatText, StringComparison.Ordinal);
        Assert.Contains("authorize procedures for execution", heatText, StringComparison.Ordinal);
        Assert.Contains("mint final seals", heatText, StringComparison.Ordinal);

        Assert.Contains("`S(X) = (C, B, R, T, L, P)`", stabilityText, StringComparison.Ordinal);
        Assert.Contains("Stability does not override truth.", stabilityText, StringComparison.Ordinal);
        Assert.Contains("`Phi(X) = 1`", stabilityText, StringComparison.Ordinal);

        Assert.Contains("canonical, minimal, and lineage-preserving", canonicalText, StringComparison.Ordinal);
        Assert.Contains("`result = condensed`", canonicalText, StringComparison.Ordinal);
        Assert.Contains("Procedure condensation:", canonicalText, StringComparison.Ordinal);

        Assert.Contains("The first procedural basis contains seven primitives:", basisText, StringComparison.Ordinal);
        Assert.Contains("`execute` is excluded as a free primitive.", basisText, StringComparison.Ordinal);
        Assert.Contains("No action outside this set may be retained or executed.", basisText, StringComparison.Ordinal);

        Assert.Contains("The protocol stages are fixed in this exact order:", ignitionText, StringComparison.Ordinal);
        Assert.Contains("The live coupling seal remains deferred.", ignitionText, StringComparison.Ordinal);
        Assert.Contains("The live coupling seal remains withheld.", ignitionText, StringComparison.Ordinal);

        Assert.Contains("heat, resonance, and expand-before-commit law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("stability metrics, condensation threshold, and canonical condensation output", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("procedural basis condensate and first bounded ignition-chain test protocol", carryForwardText, StringComparison.Ordinal);

        Assert.Contains("the live coupling seal", refinementText, StringComparison.Ordinal);
        Assert.Contains("`Delta` accumulation and condensation engine", refinementText, StringComparison.Ordinal);
        Assert.Contains("procedural basis execution beyond bounded doctrine", refinementText, StringComparison.Ordinal);
        Assert.Contains("Prime bonded minting certification and witnessed minting act", refinementText, StringComparison.Ordinal);
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
