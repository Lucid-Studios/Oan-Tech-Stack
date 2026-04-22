namespace GEL.Contracts.Sanctuary;

public enum SanctuaryResponsesApiLocalHarnessRuntimeClassKind
{
    ObservationOnly = 0,
    ToolMediatedLocalExecution = 1,
    ShellMediatedLocalExecution = 2
}

public enum SanctuaryLocalHarnessReceiptOutcomeKind
{
    Witnessed = 0,
    Hold = 1,
    Refused = 2,
    Recovered = 3
}

public enum SanctuaryLocalHarnessRefusalRecoveryKind
{
    None = 0,
    Refusal = 1,
    Recovery = 2,
    RefusalThenRecovery = 3
}

public sealed record SanctuaryResponsesApiLocalHarnessRuntimeClassDefinition
{
    public SanctuaryResponsesApiLocalHarnessRuntimeClassDefinition(
        SanctuaryResponsesApiLocalHarnessRuntimeClassKind runtimeClass,
        string handle,
        string meaning,
        string localControlBoundary,
        string operationalStatus)
    {
        RuntimeClass = runtimeClass;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
        LocalControlBoundary = SanctuaryContractGuard.RequiredText(localControlBoundary, nameof(localControlBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryResponsesApiLocalHarnessRuntimeClassKind RuntimeClass { get; }

    public string Handle { get; }

    public string Meaning { get; }

    public string LocalControlBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryResponsesApiLocalHarnessReceiptSchemaDefinition
{
    public SanctuaryResponsesApiLocalHarnessReceiptSchemaDefinition(
        string handle,
        SanctuaryLlmTestSurfaceKind surfaceKind,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> sharedWitnessFamilies,
        IReadOnlyList<SanctuaryLocalHarnessReceiptOutcomeKind> receiptOutcomeGrammar,
        IReadOnlyList<SanctuaryLocalHarnessRefusalRecoveryKind> refusalRecoveryGrammar,
        string chainBindingRule,
        string heatBindingRule,
        string evidenceBoundaryRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        SurfaceKind = surfaceKind;
        RequiredFields = SanctuaryContractGuard.RequiredTextList(requiredFields, nameof(requiredFields));
        SharedWitnessFamilies = SanctuaryContractGuard.RequiredTextList(sharedWitnessFamilies, nameof(sharedWitnessFamilies));
        ReceiptOutcomeGrammar = SanctuaryContractGuard.RequiredDistinctList(receiptOutcomeGrammar, nameof(receiptOutcomeGrammar));
        RefusalRecoveryGrammar = SanctuaryContractGuard.RequiredDistinctList(refusalRecoveryGrammar, nameof(refusalRecoveryGrammar));
        ChainBindingRule = SanctuaryContractGuard.RequiredText(chainBindingRule, nameof(chainBindingRule));
        HeatBindingRule = SanctuaryContractGuard.RequiredText(heatBindingRule, nameof(heatBindingRule));
        EvidenceBoundaryRule = SanctuaryContractGuard.RequiredText(evidenceBoundaryRule, nameof(evidenceBoundaryRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public SanctuaryLlmTestSurfaceKind SurfaceKind { get; }

    public IReadOnlyList<string> RequiredFields { get; }

    public IReadOnlyList<string> SharedWitnessFamilies { get; }

    public IReadOnlyList<SanctuaryLocalHarnessReceiptOutcomeKind> ReceiptOutcomeGrammar { get; }

    public IReadOnlyList<SanctuaryLocalHarnessRefusalRecoveryKind> RefusalRecoveryGrammar { get; }

    public string ChainBindingRule { get; }

    public string HeatBindingRule { get; }

    public string EvidenceBoundaryRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryResponsesApiLocalHarnessEvidenceBoundaryDefinition
{
    public SanctuaryResponsesApiLocalHarnessEvidenceBoundaryDefinition(
        string handle,
        string localEvidenceClass,
        IReadOnlyList<string> nonEquivalentEvidenceClasses,
        string refusalBoundary,
        string priorityRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        LocalEvidenceClass = SanctuaryContractGuard.RequiredText(localEvidenceClass, nameof(localEvidenceClass));
        NonEquivalentEvidenceClasses = SanctuaryContractGuard.RequiredTextList(nonEquivalentEvidenceClasses, nameof(nonEquivalentEvidenceClasses));
        RefusalBoundary = SanctuaryContractGuard.RequiredText(refusalBoundary, nameof(refusalBoundary));
        PriorityRule = SanctuaryContractGuard.RequiredText(priorityRule, nameof(priorityRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string LocalEvidenceClass { get; }

    public IReadOnlyList<string> NonEquivalentEvidenceClasses { get; }

    public string RefusalBoundary { get; }

    public string PriorityRule { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryResponsesApiLocalHarnessReceiptAtlas
{
    public static IReadOnlyList<SanctuaryResponsesApiLocalHarnessRuntimeClassDefinition> RuntimeClasses { get; } =
    [
        new(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ObservationOnly,
            "local-harness.runtime.observation-only",
            "responses-api-surface-without-local-command-execution",
            "model-output-remains-observational-until-explicit-local-admission",
            "placeholder-contract-only"),
        new(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ToolMediatedLocalExecution,
            "local-harness.runtime.tool-mediated",
            "responses-api-surface-with-local-tool-mediated-execution",
            "local-runtime-controls-tool-execution-and-receipts",
            "placeholder-contract-only"),
        new(
            SanctuaryResponsesApiLocalHarnessRuntimeClassKind.ShellMediatedLocalExecution,
            "local-harness.runtime.shell-mediated",
            "responses-api-surface-with-local-shell-mediated-execution",
            "local-runtime-controls-shell-execution-and-receipts",
            "placeholder-contract-only")
    ];

    public static SanctuaryResponsesApiLocalHarnessReceiptSchemaDefinition ReceiptSchema { get; } =
        new(
            handle: "local-harness.receipt-schema.v0",
            surfaceKind: SanctuaryLlmTestSurfaceKind.ResponsesApiLocalHarness,
            requiredFields:
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
            sharedWitnessFamilies:
            [
                "field-state-receipt",
                "ignition-chain-receipt",
                "verification-trace"
            ],
            receiptOutcomeGrammar:
            [
                SanctuaryLocalHarnessReceiptOutcomeKind.Witnessed,
                SanctuaryLocalHarnessReceiptOutcomeKind.Hold,
                SanctuaryLocalHarnessReceiptOutcomeKind.Refused,
                SanctuaryLocalHarnessReceiptOutcomeKind.Recovered
            ],
            refusalRecoveryGrammar:
            [
                SanctuaryLocalHarnessRefusalRecoveryKind.None,
                SanctuaryLocalHarnessRefusalRecoveryKind.Refusal,
                SanctuaryLocalHarnessRefusalRecoveryKind.Recovery,
                SanctuaryLocalHarnessRefusalRecoveryKind.RefusalThenRecovery
            ],
            chainBindingRule: "chain-outcome-must-bind-to-the-bounded-ignition-chain-protocol",
            heatBindingRule: "heat-commit-state-must-bind-to-the-expand-before-commit-law",
            evidenceBoundaryRule: "local-harness-receipts-remain-a-distinct-evidence-class",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryResponsesApiLocalHarnessEvidenceBoundaryDefinition EvidenceBoundary { get; } =
        new(
            handle: "local-harness.evidence-boundary.v0",
            localEvidenceClass: "responses-api-local-harness-evidence",
            nonEquivalentEvidenceClasses:
            [
                "codex-cloud-task-evidence",
                "general-gpt-comparison-evidence"
            ],
            refusalBoundary: "local-harness-evidence-may-not-be-promoted-into-cme-baseline-proof",
            priorityRule: "responses-api-local-harness-first",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryResponsesApiLocalHarnessRuntimeClassDefinition GetRuntimeClass(
        SanctuaryResponsesApiLocalHarnessRuntimeClassKind runtimeClass)
    {
        return RuntimeClasses.Single(item => item.RuntimeClass == runtimeClass);
    }
}
