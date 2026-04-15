using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryLlmTestSurfaceContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Llm_Test_Surface_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryLlmTestSurfaceKind.CodexCloudTask,
                SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness,
                SanctuaryLlmTestSurfaceKind.GeneralGptComparison
            ],
            SanctuaryLlmTestSurfaceAtlas.Surfaces.Select(static item => item.Kind).ToArray());

        AssertSurface(
            SanctuaryLlmTestSurfaceKind.CodexCloudTask,
            "llm-surface.codex-cloud-task.v0",
            "specialized-remote-coding-and-verification-surface",
            "isolated-remote-agent-environment",
            ["repo-scoped-coding-work", "verification-discipline", "tool-mediated-agentic-execution"],
            ["cloud-results-do-not-equal-local-harness-proof", "specialized-surface-not-neutral-cme-baseline"]);
        AssertSurface(
            SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness,
            "llm-surface.responses-api-local-harness.v0",
            "repo-controlled-local-operational-test-surface",
            "responses-api-model-selection-with-local-runtime-execution",
            ["receipt-discipline", "local-runtime-control", "heat-no-commit-governance"],
            ["local-proof-does-not-imply-cloud-equivalence", "test-surface-not-cme-proof"]);
        AssertSurface(
            SanctuaryLlmTestSurfaceKind.GeneralGptComparison,
            "llm-surface.general-gpt-comparison.v0",
            "general-purpose-comparison-surface-outside-codex-specialization",
            "responses-api-general-gpt-surface",
            ["comparative-generalization-check", "non-coding-specialization-read"],
            ["comparison-subject-not-cme-baseline", "general-surface-does-not-replace-repo-aware-subject"]);

        var harness = SanctuaryLlmTestSurfaceAtlas.ComparativeHarness;
        Assert.Equal("llm-surface.comparative-harness.v0", harness.Handle);
        Assert.Equal(
            [
                "bounded-ignition-chain-protocol",
                "shared-receipt-discipline",
                "shared-refusal-and-recovery-observation"
            ],
            harness.SharedHarnessElements);
        Assert.Equal(
            [
                "chain-receipt",
                "field-state-receipt",
                "verification-trace"
            ],
            harness.SharedReceiptElements);
        Assert.Equal(
            [
                "heat-intensity",
                "resonance",
                "harmonics",
                "no-commit-decision"
            ],
            harness.SharedHeatWitnessElements);
        Assert.Equal("responses-api-local-harness-first", harness.PriorityRule);
        Assert.Equal("cloud-results-and-local-results-are-distinct-evidence-classes", harness.EvidenceBoundaryRule);
        Assert.Equal("no-surface-is-treated-as-cme-baseline-by-default", harness.RefusalBoundary);
        Assert.Equal("placeholder-contract-only", harness.OperationalStatus);
    }

    [Fact]
    public void Llm_Test_Surface_Contracts_Serialize_Stably()
    {
        var codexCloud = SanctuaryLlmTestSurfaceAtlas.Get(SanctuaryLlmTestSurfaceKind.CodexCloudTask);
        var serializedCloud = JsonSerializer.Serialize(codexCloud);
        var deserializedCloud = JsonSerializer.Deserialize<SanctuaryLlmTestSurfaceDefinition>(serializedCloud, JsonOptions);

        Assert.NotNull(deserializedCloud);
        Assert.Equal(codexCloud.Handle, deserializedCloud!.Handle);
        Assert.Equal(codexCloud.GoverningStrengths, deserializedCloud.GoverningStrengths);
        Assert.Equal(codexCloud.EvidenceLimitations, deserializedCloud.EvidenceLimitations);

        var harness = SanctuaryLlmTestSurfaceAtlas.ComparativeHarness;
        var serializedHarness = JsonSerializer.Serialize(harness);
        var deserializedHarness = JsonSerializer.Deserialize<SanctuaryLlmComparativeHarnessDefinition>(serializedHarness, JsonOptions);

        Assert.NotNull(deserializedHarness);
        Assert.Equal(harness.Handle, deserializedHarness!.Handle);
        Assert.Equal(harness.SharedHarnessElements, deserializedHarness.SharedHarnessElements);
        Assert.Equal(harness.RefusalBoundary, deserializedHarness.RefusalBoundary);
    }

    [Fact]
    public void Llm_Test_Surface_Docs_Are_Aligned()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var taxonomyPath = Path.Combine(lineRoot, "docs", "LLM_TEST_SURFACE_TAXONOMY_NOTE.md");
        var hostedNotePath = Path.Combine(lineRoot, "docs", "HOSTED_LLM_RESIDENT_SEATING_NOTE.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var hostedNoteText = File.ReadAllText(hostedNotePath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("LLM_TEST_SURFACE_TAXONOMY_NOTE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("llm-test-surface-taxonomy-note: frame-now", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains(
            "Use the Codex family through the Responses API as a specialized coding and",
            taxonomyText,
            StringComparison.Ordinal);
        Assert.Contains("verification test surface, and use a general GPT surface as the comparison", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("No surface is treated as `CME` baseline by default.", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("`CodexCloudTask`", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("`ResponsesApiLocalHarness`", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("`GeneralGptComparison`", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("Codex cloud and Responses API local harness are distinct modes.", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("Cloud results and local results are distinct evidence classes.", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("bounded ignition-chain harness", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("heat / resonance logging", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("This note does not authorize live `CME` proof.", taxonomyText, StringComparison.Ordinal);

        Assert.Contains("The Codex family through the Responses API is a specialized coding and", hostedNoteText, StringComparison.Ordinal);
        Assert.Contains("verification surface.", hostedNoteText, StringComparison.Ordinal);
        Assert.Contains("A general GPT surface is the comparison subject outside Codex specialization.", hostedNoteText, StringComparison.Ordinal);
        Assert.Contains("Cloud results do not equal local harness proof.", hostedNoteText, StringComparison.Ordinal);
        Assert.Contains("No test surface is treated as `CME` baseline by default.", hostedNoteText, StringComparison.Ordinal);

        Assert.Contains("specialized `LLM` test surface taxonomy with Codex-family and general GPT", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("comparison law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live multi-surface comparative harness execution beyond bounded taxonomy", refinementText, StringComparison.Ordinal);
    }

    private static void AssertSurface(
        SanctuaryLlmTestSurfaceKind kind,
        string handle,
        string meaning,
        string runtimeBoundary,
        IReadOnlyList<string> strengths,
        IReadOnlyList<string> limitations)
    {
        var definition = SanctuaryLlmTestSurfaceAtlas.Get(kind);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(meaning, definition.Meaning);
        Assert.Equal(runtimeBoundary, definition.RuntimeBoundary);
        Assert.Equal(strengths, definition.GoverningStrengths);
        Assert.Equal(limitations, definition.EvidenceLimitations);
        Assert.Equal("compare-only-under-shared-ignition-chain-and-receipts", definition.SharedHarnessRule);
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
