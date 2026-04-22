using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryResponsesApiLocalHarnessReceiptContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Responses_Api_Local_Harness_Receipt_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ObservationOnly,
                SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ToolMediatedLocalExecution,
                SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ShellMediatedLocalExecution
            ],
            SanctuaryResponsesApiLocalHarnessReceiptAtlas.RuntimeClasses.Select(static item => item.RuntimeClass).ToArray());

        Assert.Equal(
            [
                SanctuaryLocalHarnessReceiptOutcomeKind.Witnessed,
                SanctuaryLocalHarnessReceiptOutcomeKind.Hold,
                SanctuaryLocalHarnessReceiptOutcomeKind.Refused,
                SanctuaryLocalHarnessReceiptOutcomeKind.Recovered
            ],
            Enum.GetValues<SanctuaryLocalHarnessReceiptOutcomeKind>());

        Assert.Equal(
            [
                SanctuaryLocalHarnessRefusalRecoveryKind.None,
                SanctuaryLocalHarnessRefusalRecoveryKind.Refusal,
                SanctuaryLocalHarnessRefusalRecoveryKind.Recovery,
                SanctuaryLocalHarnessRefusalRecoveryKind.RefusalThenRecovery
            ],
            Enum.GetValues<SanctuaryLocalHarnessRefusalRecoveryKind>());

        AssertRuntimeClass(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ObservationOnly,
            "local-harness.runtime.observation-only",
            "responses-api-surface-without-local-command-execution",
            "model-output-remains-observational-until-explicit-local-admission");
        AssertRuntimeClass(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ToolMediatedLocalExecution,
            "local-harness.runtime.tool-mediated",
            "responses-api-surface-with-local-tool-mediated-execution",
            "local-runtime-controls-tool-execution-and-receipts");
        AssertRuntimeClass(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ShellMediatedLocalExecution,
            "local-harness.runtime.shell-mediated",
            "responses-api-surface-with-local-shell-mediated-execution",
            "local-runtime-controls-shell-execution-and-receipts");

        var schema = SanctuaryResponsesApiLocalHarnessReceiptAtlas.ReceiptSchema;
        Assert.Equal("local-harness.receipt-schema.v0", schema.Handle);
        Assert.Equal(SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness, schema.SurfaceKind);
        Assert.Equal(
            [
                "surface-class",
                "runtime-class",
                "input-slice-id",
                "heat-commit-state",
                "shell-observations",
                "chain-outcome",
                "receipt-outcome",
                "refusal-recovery-state",
                "evidence-class"
            ],
            schema.RequiredFields);
        Assert.Equal(
            [
                "field-state-receipt",
                "ignition-chain-receipt",
                "verification-trace"
            ],
            schema.SharedWitnessFamilies);
        Assert.Equal(
            [
                SanctuaryLocalHarnessReceiptOutcomeKind.Witnessed,
                SanctuaryLocalHarnessReceiptOutcomeKind.Hold,
                SanctuaryLocalHarnessReceiptOutcomeKind.Refused,
                SanctuaryLocalHarnessReceiptOutcomeKind.Recovered
            ],
            schema.ReceiptOutcomeGrammar);
        Assert.Equal(
            [
                SanctuaryLocalHarnessRefusalRecoveryKind.None,
                SanctuaryLocalHarnessRefusalRecoveryKind.Refusal,
                SanctuaryLocalHarnessRefusalRecoveryKind.Recovery,
                SanctuaryLocalHarnessRefusalRecoveryKind.RefusalThenRecovery
            ],
            schema.RefusalRecoveryGrammar);
        Assert.Equal("chain-outcome-must-bind-to-the-bounded-ignition-chain-protocol", schema.ChainBindingRule);
        Assert.Equal("heat-commit-state-must-bind-to-the-expand-before-commit-law", schema.HeatBindingRule);
        Assert.Equal("local-harness-receipts-remain-a-distinct-evidence-class", schema.EvidenceBoundaryRule);
        Assert.Equal("placeholder-contract-only", schema.OperationalStatus);

        var evidence = SanctuaryResponsesApiLocalHarnessReceiptAtlas.EvidenceBoundary;
        Assert.Equal("local-harness.evidence-boundary.v0", evidence.Handle);
        Assert.Equal("responses-api-local-harness-evidence", evidence.LocalEvidenceClass);
        Assert.Equal(
            [
                "codex-cloud-task-evidence",
                "general-gpt-comparison-evidence"
            ],
            evidence.NonEquivalentEvidenceClasses);
        Assert.Equal("local-harness-evidence-may-not-be-promoted-into-cme-baseline-proof", evidence.RefusalBoundary);
        Assert.Equal("responses-api-local-harness-first", evidence.PriorityRule);
        Assert.Equal("placeholder-contract-only", evidence.OperationalStatus);
    }

    [Fact]
    public void Responses_Api_Local_Harness_Receipt_Contracts_Serialize_Stably()
    {
        var schema = SanctuaryResponsesApiLocalHarnessReceiptAtlas.ReceiptSchema;
        var serializedSchema = JsonSerializer.Serialize(schema);
        var deserializedSchema = JsonSerializer.Deserialize<SanctuaryResponsesApiLocalHarnessReceiptSchemaDefinition>(serializedSchema, JsonOptions);

        Assert.NotNull(deserializedSchema);
        Assert.Equal(schema.Handle, deserializedSchema!.Handle);
        Assert.Equal(schema.RequiredFields, deserializedSchema.RequiredFields);
        Assert.Equal(schema.RefusalRecoveryGrammar, deserializedSchema.RefusalRecoveryGrammar);

        var evidence = SanctuaryResponsesApiLocalHarnessReceiptAtlas.EvidenceBoundary;
        var serializedEvidence = JsonSerializer.Serialize(evidence);
        var deserializedEvidence = JsonSerializer.Deserialize<SanctuaryResponsesApiLocalHarnessEvidenceBoundaryDefinition>(serializedEvidence, JsonOptions);

        Assert.NotNull(deserializedEvidence);
        Assert.Equal(evidence.Handle, deserializedEvidence!.Handle);
        Assert.Equal(evidence.LocalEvidenceClass, deserializedEvidence.LocalEvidenceClass);
        Assert.Equal(evidence.NonEquivalentEvidenceClasses, deserializedEvidence.NonEquivalentEvidenceClasses);
    }

    [Fact]
    public void Responses_Api_Local_Harness_Receipt_Docs_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var taxonomyPath = Path.Combine(lineRoot, "docs", "LLM_TEST_SURFACE_TAXONOMY_NOTE.md");
        var hostedNotePath = Path.Combine(lineRoot, "docs", "HOSTED_LLM_RESIDENT_SEATING_NOTE.md");
        var schemaPath = Path.Combine(lineRoot, "docs", "RESPONSES_API_LOCAL_HARNESS_RECEIPT_SCHEMA_NOTE.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var hostedNoteText = File.ReadAllText(hostedNotePath);
        var schemaText = File.ReadAllText(schemaPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("RESPONSES_API_LOCAL_HARNESS_RECEIPT_SCHEMA_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("responses-api-local-harness-receipt-schema-note: frame-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("`ResponsesApiLocalHarness` is the first-priority `LLM` test surface", schemaText, StringComparison.Ordinal);
        Assert.Contains("The first local harness receipt must carry:", schemaText, StringComparison.Ordinal);
        Assert.Contains("`surface-class`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`runtime-class`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`input-slice-id`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`heat-commit-state`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`shell-observations`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`chain-outcome`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`receipt-outcome`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`refusal-recovery-state`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`evidence-class`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`ObservationOnly`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`ToolMediatedLocalExecution`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`ShellMediatedLocalExecution`", schemaText, StringComparison.Ordinal);
        Assert.Contains("Local harness evidence remains a distinct evidence class.", schemaText, StringComparison.Ordinal);
        Assert.Contains("It only defines the first lawful landing body for local harness evidence.", schemaText, StringComparison.Ordinal);

        Assert.Contains("RESPONSES_API_LOCAL_HARNESS_RECEIPT_SCHEMA_NOTE.md", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("RESPONSES_API_LOCAL_HARNESS_RECEIPT_SCHEMA_NOTE.md", hostedNoteText, StringComparison.Ordinal);

        Assert.Contains("first local Responses API harness receipt schema and evidence landing body", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live Responses API harness execution beyond bounded receipt schema", refinementText, StringComparison.Ordinal);
    }

    private static void AssertRuntimeClass(
        SanctuaryResponsesApiLocalHarnessRuntimeClassKind runtimeClass,
        string handle,
        string meaning,
        string localControlBoundary)
    {
        var definition = SanctuaryResponsesApiLocalHarnessReceiptAtlas.GetRuntimeClass(runtimeClass);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(meaning, definition.Meaning);
        Assert.Equal(localControlBoundary, definition.LocalControlBoundary);
        Assert.Equal("placeholder-contract-only", definition.OperationalStatus);
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
