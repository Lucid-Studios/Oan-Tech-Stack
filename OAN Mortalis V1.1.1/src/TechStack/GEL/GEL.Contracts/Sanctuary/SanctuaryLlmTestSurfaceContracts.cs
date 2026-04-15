namespace GEL.Contracts.Sanctuary;

public enum SanctuaryLlmTestSurfaceKind
{
    CodexCloudTask = 0,
    ResponsesApiLocalHarness = 1,
    GeneralGptComparison = 2
}

public sealed record SanctuaryLlmTestSurfaceDefinition
{
    public SanctuaryLlmTestSurfaceDefinition(
        SanctuaryLlmTestSurfaceKind kind,
        string handle,
        string meaning,
        string runtimeBoundary,
        IReadOnlyList<string> governingStrengths,
        IReadOnlyList<string> evidenceLimitations,
        string sharedHarnessRule,
        string operationalStatus)
    {
        Kind = kind;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
        RuntimeBoundary = SanctuaryContractGuard.RequiredText(runtimeBoundary, nameof(runtimeBoundary));
        GoverningStrengths = SanctuaryContractGuard.RequiredTextList(governingStrengths, nameof(governingStrengths));
        EvidenceLimitations = SanctuaryContractGuard.RequiredTextList(evidenceLimitations, nameof(evidenceLimitations));
        SharedHarnessRule = SanctuaryContractGuard.RequiredText(sharedHarnessRule, nameof(sharedHarnessRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryLlmTestSurfaceKind Kind { get; }

    public string Handle { get; }

    public string Meaning { get; }

    public string RuntimeBoundary { get; }

    public IReadOnlyList<string> GoverningStrengths { get; }

    public IReadOnlyList<string> EvidenceLimitations { get; }

    public string SharedHarnessRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryLlmComparativeHarnessDefinition
{
    public SanctuaryLlmComparativeHarnessDefinition(
        string handle,
        IReadOnlyList<string> sharedHarnessElements,
        IReadOnlyList<string> sharedReceiptElements,
        IReadOnlyList<string> sharedHeatWitnessElements,
        string priorityRule,
        string evidenceBoundaryRule,
        string refusalBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        SharedHarnessElements = SanctuaryContractGuard.RequiredTextList(sharedHarnessElements, nameof(sharedHarnessElements));
        SharedReceiptElements = SanctuaryContractGuard.RequiredTextList(sharedReceiptElements, nameof(sharedReceiptElements));
        SharedHeatWitnessElements = SanctuaryContractGuard.RequiredTextList(sharedHeatWitnessElements, nameof(sharedHeatWitnessElements));
        PriorityRule = SanctuaryContractGuard.RequiredText(priorityRule, nameof(priorityRule));
        EvidenceBoundaryRule = SanctuaryContractGuard.RequiredText(evidenceBoundaryRule, nameof(evidenceBoundaryRule));
        RefusalBoundary = SanctuaryContractGuard.RequiredText(refusalBoundary, nameof(refusalBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> SharedHarnessElements { get; }

    public IReadOnlyList<string> SharedReceiptElements { get; }

    public IReadOnlyList<string> SharedHeatWitnessElements { get; }

    public string PriorityRule { get; }

    public string EvidenceBoundaryRule { get; }

    public string RefusalBoundary { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryLlmTestSurfaceAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryLlmTestSurfaceKind, SanctuaryLlmTestSurfaceDefinition> SurfaceDefinitions =
        new Dictionary<SanctuaryLlmTestSurfaceKind, SanctuaryLlmTestSurfaceDefinition>
        {
            [SanctuaryLlmTestSurfaceKind.CodexCloudTask] = new(
                kind: SanctuaryLlmTestSurfaceKind.CodexCloudTask,
                handle: "llm-surface.codex-cloud-task.v0",
                meaning: "specialized-remote-coding-and-verification-surface",
                runtimeBoundary: "isolated-remote-agent-environment",
                governingStrengths:
                [
                    "repo-scoped-coding-work",
                    "verification-discipline",
                    "tool-mediated-agentic-execution"
                ],
                evidenceLimitations:
                [
                    "cloud-results-do-not-equal-local-harness-proof",
                    "specialized-surface-not-neutral-cme-baseline"
                ],
                sharedHarnessRule: "compare-only-under-shared-ignition-chain-and-receipts",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness] = new(
                kind: SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness,
                handle: "llm-surface.responses-api-local-harness.v0",
                meaning: "repo-controlled-local-operational-test-surface",
                runtimeBoundary: "responses-api-model-selection-with-local-runtime-execution",
                governingStrengths:
                [
                    "receipt-discipline",
                    "local-runtime-control",
                    "heat-no-commit-governance"
                ],
                evidenceLimitations:
                [
                    "local-proof-does-not-imply-cloud-equivalence",
                    "test-surface-not-cme-proof"
                ],
                sharedHarnessRule: "compare-only-under-shared-ignition-chain-and-receipts",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryLlmTestSurfaceKind.GeneralGptComparison] = new(
                kind: SanctuaryLlmTestSurfaceKind.GeneralGptComparison,
                handle: "llm-surface.general-gpt-comparison.v0",
                meaning: "general-purpose-comparison-surface-outside-codex-specialization",
                runtimeBoundary: "responses-api-general-gpt-surface",
                governingStrengths:
                [
                    "comparative-generalization-check",
                    "non-coding-specialization-read"
                ],
                evidenceLimitations:
                [
                    "comparison-subject-not-cme-baseline",
                    "general-surface-does-not-replace-repo-aware-subject"
                ],
                sharedHarnessRule: "compare-only-under-shared-ignition-chain-and-receipts",
                operationalStatus: "placeholder-contract-only")
        };

    public static IReadOnlyList<SanctuaryLlmTestSurfaceDefinition> Surfaces { get; } =
        SurfaceDefinitions.Values
            .OrderBy(static item => item.Kind)
            .ToArray();

    public static SanctuaryLlmComparativeHarnessDefinition ComparativeHarness { get; } =
        new(
            handle: "llm-surface.comparative-harness.v0",
            sharedHarnessElements:
            [
                "bounded-ignition-chain-protocol",
                "shared-receipt-discipline",
                "shared-refusal-and-recovery-observation"
            ],
            sharedReceiptElements:
            [
                "chain-receipt",
                "field-state-receipt",
                "verification-trace"
            ],
            sharedHeatWitnessElements:
            [
                "heat-intensity",
                "resonance",
                "harmonics",
                "no-commit-decision"
            ],
            priorityRule: "responses-api-local-harness-first",
            evidenceBoundaryRule: "cloud-results-and-local-results-are-distinct-evidence-classes",
            refusalBoundary: "no-surface-is-treated-as-cme-baseline-by-default",
            operationalStatus: "placeholder-contract-only");

    public static bool TryGet(
        SanctuaryLlmTestSurfaceKind kind,
        out SanctuaryLlmTestSurfaceDefinition definition)
    {
        return SurfaceDefinitions.TryGetValue(kind, out definition!);
    }

    public static SanctuaryLlmTestSurfaceDefinition Get(SanctuaryLlmTestSurfaceKind kind)
    {
        if (!TryGet(kind, out var definition))
        {
            throw new KeyNotFoundException($"No LLM test surface definition exists for '{kind}'.");
        }

        return definition;
    }
}
