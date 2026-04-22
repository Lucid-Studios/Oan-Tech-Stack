namespace GEL.Contracts.Sanctuary;

public enum SanctuaryCrypticHeatSurfaceKind
{
    CSelfGel = 0,
    COe = 1,
    CrypticLayer = 2
}

public enum SanctuaryHeatCommitDecisionKind
{
    Expand = 0,
    Hold = 1,
    PrepareCondensation = 2
}

public sealed record SanctuaryHeatCommitDisciplineDefinition
{
    public SanctuaryHeatCommitDisciplineDefinition(
        string handle,
        IReadOnlyList<SanctuaryCrypticHeatSurfaceKind> governingSurfaces,
        IReadOnlyList<string> forbiddenCommitActions,
        IReadOnlyList<string> allowedHeatActions,
        string governingRule,
        string truthBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        GoverningSurfaces = SanctuaryContractGuard.RequiredDistinctList(governingSurfaces, nameof(governingSurfaces));
        ForbiddenCommitActions = SanctuaryContractGuard.RequiredTextList(forbiddenCommitActions, nameof(forbiddenCommitActions));
        AllowedHeatActions = SanctuaryContractGuard.RequiredTextList(allowedHeatActions, nameof(allowedHeatActions));
        GoverningRule = SanctuaryContractGuard.RequiredText(governingRule, nameof(governingRule));
        TruthBoundary = SanctuaryContractGuard.RequiredText(truthBoundary, nameof(truthBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryCrypticHeatSurfaceKind> GoverningSurfaces { get; }

    public IReadOnlyList<string> ForbiddenCommitActions { get; }

    public IReadOnlyList<string> AllowedHeatActions { get; }

    public string GoverningRule { get; }

    public string TruthBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryFieldStateReceiptDefinition
{
    public SanctuaryFieldStateReceiptDefinition(
        string handle,
        IReadOnlyList<string> receiptFields,
        IReadOnlyList<SanctuaryHeatCommitDecisionKind> decisionKinds,
        string decisionGrammar,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ReceiptFields = SanctuaryContractGuard.RequiredTextList(receiptFields, nameof(receiptFields));
        DecisionKinds = SanctuaryContractGuard.RequiredDistinctList(decisionKinds, nameof(decisionKinds));
        DecisionGrammar = SanctuaryContractGuard.RequiredText(decisionGrammar, nameof(decisionGrammar));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> ReceiptFields { get; }

    public IReadOnlyList<SanctuaryHeatCommitDecisionKind> DecisionKinds { get; }

    public string DecisionGrammar { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryHeatResonanceAtlas
{
    public static SanctuaryHeatCommitDisciplineDefinition HeatCommitDiscipline { get; } =
        new(
            handle: "heat.expand-before-commit.v0",
            governingSurfaces:
            [
                SanctuaryCrypticHeatSurfaceKind.CSelfGel,
                SanctuaryCrypticHeatSurfaceKind.COe,
                SanctuaryCrypticHeatSurfaceKind.CrypticLayer
            ],
            forbiddenCommitActions:
            [
                "condense-structures",
                "merge-anchors",
                "finalize-definitions",
                "authorize-procedures-for-execution",
                "mint-final-seals"
            ],
            allowedHeatActions:
            [
                "simulate",
                "decompose",
                "relate",
                "accumulate",
                "refine-provisionally"
            ],
            governingRule: "if-heat-and-stability-not-established-then-no-commit-and-expand",
            truthBoundary: "no-commit-discipline-protects-truth-during-heat",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryFieldStateReceiptDefinition FieldStateReceipt { get; } =
        new(
            handle: "heat.field-state.receipt.v0",
            receiptFields:
            [
                "shell",
                "heat-intensity",
                "heat-color",
                "resonance",
                "harmonics",
                "decision"
            ],
            decisionKinds:
            [
                SanctuaryHeatCommitDecisionKind.Expand,
                SanctuaryHeatCommitDecisionKind.Hold,
                SanctuaryHeatCommitDecisionKind.PrepareCondensation
            ],
            decisionGrammar: "decision=expand|hold|prepare-condensation",
            operationalStatus: "placeholder-contract-only");
}
