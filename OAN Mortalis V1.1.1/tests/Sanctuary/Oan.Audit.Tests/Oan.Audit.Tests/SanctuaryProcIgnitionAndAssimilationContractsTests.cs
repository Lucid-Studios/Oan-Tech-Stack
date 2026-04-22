using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryProcIgnitionAndAssimilationContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Proc_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryGelProceduralInvariantKind.Grounding,
                SanctuaryGelProceduralInvariantKind.DownwardResolution,
                SanctuaryGelProceduralInvariantKind.PostureStability,
                SanctuaryGelProceduralInvariantKind.TraceValidity,
                SanctuaryGelProceduralInvariantKind.CrypticIntegrity
            ],
            Enum.GetValues<SanctuaryGelProceduralInvariantKind>());

        Assert.Equal(
            [
                SanctuaryGelProcedureDisposition.Admissible,
                SanctuaryGelProcedureDisposition.Gated,
                SanctuaryGelProcedureDisposition.Refused
            ],
            Enum.GetValues<SanctuaryGelProcedureDisposition>());

        var procedure = SanctuaryGelProcedureAtlas.Procedure;
        Assert.Equal("gel.proc.grounded-action.v0", procedure.Handle);
        Assert.Equal("grounded-posture-gated-action-pattern-derived-from-root-definition-and-relation", procedure.Meaning);
        Assert.Equal(
            [
                "pi",
                "delta",
                "alpha*",
                "nu",
                "phi",
                "chi",
                "sigma_proc"
            ],
            procedure.CanonicalBodyFields);
        Assert.Equal(
            [
                "root",
                "def",
                "rel"
            ],
            procedure.RequiredLowerChain);
        Assert.Equal(
            [
                SanctuaryGelProceduralInvariantKind.Grounding,
                SanctuaryGelProceduralInvariantKind.DownwardResolution,
                SanctuaryGelProceduralInvariantKind.PostureStability,
                SanctuaryGelProceduralInvariantKind.TraceValidity,
                SanctuaryGelProceduralInvariantKind.CrypticIntegrity
            ],
            procedure.GoverningInvariants);
        Assert.Equal(
            [
                "simulation-required",
                "trace-resolves-to-supporting-anchors",
                "prime-posture-required",
                "cryptic-integrity-sealed"
            ],
            procedure.RequiredWitnesses);
        Assert.Equal(
            [
                "ungrounded-step",
                "hidden-assumption",
                "action-first-reasoning",
                "lower-layer-bypass"
            ],
            procedure.RefusalTriggers);
        Assert.Equal("delta=P-anchor-support-and-R-D-E-grounding-prime-posture-and-phi-required", procedure.AdmissibilityRule);
        Assert.Equal("execute-only-when-prime-posture-holds", procedure.ExecutionRule);
        Assert.Equal("placeholder-contract-only", procedure.OperationalStatus);
    }

    [Fact]
    public void Ignition_Chain_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryIgnitionChainSlotKind.Root,
                SanctuaryIgnitionChainSlotKind.Definition,
                SanctuaryIgnitionChainSlotKind.Relation,
                SanctuaryIgnitionChainSlotKind.Procedure
            ],
            Enum.GetValues<SanctuaryIgnitionChainSlotKind>());

        Assert.Equal(
            [
                SanctuaryIgnitionChainWitnessKind.Order,
                SanctuaryIgnitionChainWitnessKind.Grounding,
                SanctuaryIgnitionChainWitnessKind.Cleaving,
                SanctuaryIgnitionChainWitnessKind.Recovery,
                SanctuaryIgnitionChainWitnessKind.Simulation
            ],
            Enum.GetValues<SanctuaryIgnitionChainWitnessKind>());

        Assert.Equal(
            [
                SanctuaryIgnitionChainResult.Success,
                SanctuaryIgnitionChainResult.Failure,
                SanctuaryIgnitionChainResult.Hold
            ],
            Enum.GetValues<SanctuaryIgnitionChainResult>());

        var template = SanctuaryIgnitionChainAtlas.Template;
        Assert.Equal("ignition-chain.template.v0", template.Handle);
        Assert.Equal(
            [
                SanctuaryIgnitionChainSlotKind.Root,
                SanctuaryIgnitionChainSlotKind.Definition,
                SanctuaryIgnitionChainSlotKind.Relation,
                SanctuaryIgnitionChainSlotKind.Procedure
            ],
            template.SlotOrder);
        Assert.Equal(
            [
                SanctuaryIgnitionChainWitnessKind.Order,
                SanctuaryIgnitionChainWitnessKind.Grounding,
                SanctuaryIgnitionChainWitnessKind.Cleaving,
                SanctuaryIgnitionChainWitnessKind.Recovery,
                SanctuaryIgnitionChainWitnessKind.Simulation
            ],
            template.WitnessKinds);
        Assert.Equal("root-definition-relation-and-procedure-must-form-one-grounded-action-bearing-chain", template.GoverningRule);
        Assert.Equal("no-live-execution-no-bonded-minting-no-slot-bypass", template.RefusalBoundary);
        Assert.Equal("placeholder-contract-only", template.OperationalStatus);

        var primeFace = SanctuaryIgnitionChainAtlas.PrimeFace;
        Assert.Equal("ignition-chain.prime-face.v0", primeFace.Handle);
        Assert.Equal("R,D,E,P", primeFace.Order);
        Assert.Equal(["valid", "provisional", "refused"], primeFace.Statuses);
        Assert.Equal("ignition-chain", primeFace.Role);
        Assert.Equal("placeholder-contract-only", primeFace.OperationalStatus);

        var crypticFace = SanctuaryIgnitionChainAtlas.CrypticFace;
        Assert.Equal("ignition-chain.cryptic-face.v0", crypticFace.Handle);
        Assert.Equal(
            [
                "combined-anchor-keys",
                "dependency-keys",
                "trace-keys",
                "full-chain-seal"
            ],
            crypticFace.MinimalSlots);
        Assert.Equal("full-chain-seal-preserves-lineage-through-action", crypticFace.IntegrityRule);
        Assert.Equal("placeholder-contract-only", crypticFace.OperationalStatus);

        var receipt = SanctuaryIgnitionChainAtlas.Receipt;
        Assert.Equal("ignition-chain.receipt.v0", receipt.Handle);
        Assert.Equal(["r", "d", "e", "p"], receipt.MemberSlots);
        Assert.Equal(["posture", "chain", "lineage", "simulation", "recovery"], receipt.CheckSlots);
        Assert.Equal("success|failure|hold", receipt.ResultGrammar);
        Assert.Equal("placeholder-contract-only", receipt.OperationalStatus);
    }

    [Fact]
    public void Assimilation_Delta_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryAssimilationType.New,
                SanctuaryAssimilationType.Instance,
                SanctuaryAssimilationType.Refine,
                SanctuaryAssimilationType.Extend,
                SanctuaryAssimilationType.Relate,
                SanctuaryAssimilationType.Reject
            ],
            Enum.GetValues<SanctuaryAssimilationType>());

        Assert.Equal(
            [
                SanctuaryAssimilationConflictOutcome.Coexist,
                SanctuaryAssimilationConflictOutcome.Strengthen,
                SanctuaryAssimilationConflictOutcome.Compose,
                SanctuaryAssimilationConflictOutcome.Replace,
                SanctuaryAssimilationConflictOutcome.ScopeSplit,
                SanctuaryAssimilationConflictOutcome.Decompose,
                SanctuaryAssimilationConflictOutcome.Refuse
            ],
            Enum.GetValues<SanctuaryAssimilationConflictOutcome>());

        var receipt = SanctuaryAssimilationDeltaAtlas.AssimilationReceipt;
        Assert.Equal("assimilation.receipt.v0", receipt.Handle);
        Assert.Equal(
            [
                "assimilation-type",
                "anchor-lineage",
                "layer",
                "boundary-context-witness",
                "integrity-seal",
                "conflict-resolution-outcome"
            ],
            receipt.ReceiptFields);
        Assert.Equal(
            [
                SanctuaryAssimilationType.New,
                SanctuaryAssimilationType.Instance,
                SanctuaryAssimilationType.Refine,
                SanctuaryAssimilationType.Extend,
                SanctuaryAssimilationType.Relate,
                SanctuaryAssimilationType.Reject
            ],
            receipt.AssimilationTypes);
        Assert.Equal(
            [
                SanctuaryAssimilationConflictOutcome.Coexist,
                SanctuaryAssimilationConflictOutcome.Strengthen,
                SanctuaryAssimilationConflictOutcome.Compose,
                SanctuaryAssimilationConflictOutcome.Replace,
                SanctuaryAssimilationConflictOutcome.ScopeSplit,
                SanctuaryAssimilationConflictOutcome.Decompose,
                SanctuaryAssimilationConflictOutcome.Refuse
            ],
            receipt.ConflictOutcomeSet);
        Assert.Equal("receipt-fields-govern-delta", receipt.DeltaRule);
        Assert.Equal("placeholder-contract-only", receipt.OperationalStatus);

        var deltaBridge = SanctuaryAssimilationDeltaAtlas.DeltaBridge;
        Assert.Equal("delta.bridge.v0", deltaBridge.Handle);
        Assert.Equal("same-receipt-later-drives-structural-change-in-gel", deltaBridge.GelEffectRule);
        Assert.Equal("same-receipt-later-drives-continuity-change-in-selfgel", deltaBridge.SelfGelEffectRule);
        Assert.Equal("gel-and-selfgel-later-receive-coupled-but-non-identical-effects-from-the-same-receipt", deltaBridge.CouplingRule);
        Assert.Equal("no-executable-delta-accumulation-or-condensation-in-this-slice", deltaBridge.ExecutableBoundary);
        Assert.Equal(
            [
                "delta-accumulation-engine",
                "condensation-engine",
                "stability-metrics",
                "skill-condensation"
            ],
            deltaBridge.DeferredSurfaces);
        Assert.Equal("placeholder-contract-only", deltaBridge.OperationalStatus);
    }

    [Fact]
    public void Proc_Ignition_And_Assimilation_Contracts_Serialize_Stably()
    {
        var procedure = SanctuaryGelProcedureAtlas.Procedure;
        var serializedProcedure = JsonSerializer.Serialize(procedure);
        var deserializedProcedure = JsonSerializer.Deserialize<SanctuaryGelProcedureDefinition>(serializedProcedure, JsonOptions);

        Assert.NotNull(deserializedProcedure);
        Assert.Equal(procedure.Handle, deserializedProcedure!.Handle);
        Assert.Equal(procedure.CanonicalBodyFields, deserializedProcedure.CanonicalBodyFields);
        Assert.Equal(procedure.RefusalTriggers, deserializedProcedure.RefusalTriggers);

        var chainReceipt = SanctuaryIgnitionChainAtlas.Receipt;
        var serializedChainReceipt = JsonSerializer.Serialize(chainReceipt);
        var deserializedChainReceipt = JsonSerializer.Deserialize<SanctuaryIgnitionChainReceiptDefinition>(serializedChainReceipt, JsonOptions);

        Assert.NotNull(deserializedChainReceipt);
        Assert.Equal(chainReceipt.Handle, deserializedChainReceipt!.Handle);
        Assert.Equal(chainReceipt.MemberSlots, deserializedChainReceipt.MemberSlots);
        Assert.Equal(chainReceipt.CheckSlots, deserializedChainReceipt.CheckSlots);

        var deltaBridge = SanctuaryAssimilationDeltaAtlas.DeltaBridge;
        var serializedDeltaBridge = JsonSerializer.Serialize(deltaBridge);
        var deserializedDeltaBridge = JsonSerializer.Deserialize<SanctuaryDeltaBridgeDefinition>(serializedDeltaBridge, JsonOptions);

        Assert.NotNull(deserializedDeltaBridge);
        Assert.Equal(deltaBridge.Handle, deserializedDeltaBridge!.Handle);
        Assert.Equal(deltaBridge.CouplingRule, deserializedDeltaBridge.CouplingRule);
        Assert.Equal(deltaBridge.DeferredSurfaces, deserializedDeltaBridge.DeferredSurfaces);
    }

    [Fact]
    public void Proc_Ignition_And_Assimilation_Docs_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var procPath = Path.Combine(lineRoot, "docs", "PROC_GROUNDED_ACTION_AND_TRACE_LAW.md");
        var ignitionPath = Path.Combine(lineRoot, "docs", "IGNITION_CHAIN_TEMPLATE_AND_WITNESS_LAW.md");
        var assimilationPath = Path.Combine(lineRoot, "docs", "ASSIMILATION_RECEIPT_AND_DELTA_BRIDGE_NOTE.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var procText = File.ReadAllText(procPath);
        var ignitionText = File.ReadAllText(ignitionPath);
        var assimilationText = File.ReadAllText(assimilationPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("PROC_GROUNDED_ACTION_AND_TRACE_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("IGNITION_CHAIN_TEMPLATE_AND_WITNESS_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("ASSIMILATION_RECEIPT_AND_DELTA_BRIDGE_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("proc-grounded-action-trace-law: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("ignition-chain-template-witness-law: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("assimilation-receipt-delta-bridge-note: frame-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("grounded, posture-gated action pattern", procText, StringComparison.Ordinal);
        Assert.Contains("no procedure without a complete downward chain", procText, StringComparison.Ordinal);
        Assert.Contains("Simulation is required.", procText, StringComparison.Ordinal);
        Assert.Contains("trace resolves to anchors", procText, StringComparison.Ordinal);

        Assert.Contains("slot-and-witness law", ignitionText, StringComparison.Ordinal);
        Assert.Contains("not live execution", ignitionText, StringComparison.Ordinal);
        Assert.Contains("R,D,E,P", ignitionText, StringComparison.Ordinal);
        Assert.Contains("result = success|failure|hold", ignitionText, StringComparison.Ordinal);

        Assert.Contains("assimilation receipts govern later `Delta`", assimilationText, StringComparison.Ordinal);
        Assert.Contains("`GEL` and `SelfGEL` later receive coupled but non-identical effects", assimilationText, StringComparison.Ordinal);
        Assert.Contains("Accumulation, condensation, and stability metrics remain deferred.", assimilationText, StringComparison.Ordinal);
        Assert.Contains("Bonded minting remains deferred until after this substrate is seated.", assimilationText, StringComparison.Ordinal);

        Assert.Contains("`proc` grounding, trace, and cryptic integrity law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("ignition-chain template and witness family", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("receipt-governed assimilation and `Delta` bridge", carryForwardText, StringComparison.Ordinal);

        Assert.Contains("live `proc` execution and ignition-chain retention", refinementText, StringComparison.Ordinal);
        Assert.Contains("`Delta` accumulation and condensation engine", refinementText, StringComparison.Ordinal);
        Assert.Contains("stability metrics as executable thresholds", refinementText, StringComparison.Ordinal);
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
