namespace GEL.Contracts.Sanctuary;

public enum SanctuaryAssimilationType
{
    New = 0,
    Instance = 1,
    Refine = 2,
    Extend = 3,
    Relate = 4,
    Reject = 5
}

public enum SanctuaryAssimilationConflictOutcome
{
    Coexist = 0,
    Strengthen = 1,
    Compose = 2,
    Replace = 3,
    ScopeSplit = 4,
    Decompose = 5,
    Refuse = 6
}

public sealed record SanctuaryAssimilationReceiptDefinition
{
    public SanctuaryAssimilationReceiptDefinition(
        string handle,
        IReadOnlyList<string> receiptFields,
        IReadOnlyList<SanctuaryAssimilationType> assimilationTypes,
        IReadOnlyList<SanctuaryAssimilationConflictOutcome> conflictOutcomeSet,
        string deltaRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ReceiptFields = SanctuaryContractGuard.RequiredTextList(receiptFields, nameof(receiptFields));
        AssimilationTypes = SanctuaryContractGuard.RequiredDistinctList(assimilationTypes, nameof(assimilationTypes));
        ConflictOutcomeSet = SanctuaryContractGuard.RequiredDistinctList(conflictOutcomeSet, nameof(conflictOutcomeSet));
        DeltaRule = SanctuaryContractGuard.RequiredText(deltaRule, nameof(deltaRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> ReceiptFields { get; }

    public IReadOnlyList<SanctuaryAssimilationType> AssimilationTypes { get; }

    public IReadOnlyList<SanctuaryAssimilationConflictOutcome> ConflictOutcomeSet { get; }

    public string DeltaRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryDeltaBridgeDefinition
{
    public SanctuaryDeltaBridgeDefinition(
        string handle,
        string gelEffectRule,
        string selfGelEffectRule,
        string couplingRule,
        string executableBoundary,
        IReadOnlyList<string> deferredSurfaces,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        GelEffectRule = SanctuaryContractGuard.RequiredText(gelEffectRule, nameof(gelEffectRule));
        SelfGelEffectRule = SanctuaryContractGuard.RequiredText(selfGelEffectRule, nameof(selfGelEffectRule));
        CouplingRule = SanctuaryContractGuard.RequiredText(couplingRule, nameof(couplingRule));
        ExecutableBoundary = SanctuaryContractGuard.RequiredText(executableBoundary, nameof(executableBoundary));
        DeferredSurfaces = SanctuaryContractGuard.RequiredTextList(deferredSurfaces, nameof(deferredSurfaces));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string GelEffectRule { get; }

    public string SelfGelEffectRule { get; }

    public string CouplingRule { get; }

    public string ExecutableBoundary { get; }

    public IReadOnlyList<string> DeferredSurfaces { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryAssimilationDeltaAtlas
{
    public static SanctuaryAssimilationReceiptDefinition AssimilationReceipt { get; } =
        new(
            handle: "assimilation.receipt.v0",
            receiptFields:
            [
                "assimilation-type",
                "anchor-lineage",
                "layer",
                "boundary-context-witness",
                "integrity-seal",
                "conflict-resolution-outcome"
            ],
            assimilationTypes:
            [
                SanctuaryAssimilationType.New,
                SanctuaryAssimilationType.Instance,
                SanctuaryAssimilationType.Refine,
                SanctuaryAssimilationType.Extend,
                SanctuaryAssimilationType.Relate,
                SanctuaryAssimilationType.Reject
            ],
            conflictOutcomeSet:
            [
                SanctuaryAssimilationConflictOutcome.Coexist,
                SanctuaryAssimilationConflictOutcome.Strengthen,
                SanctuaryAssimilationConflictOutcome.Compose,
                SanctuaryAssimilationConflictOutcome.Replace,
                SanctuaryAssimilationConflictOutcome.ScopeSplit,
                SanctuaryAssimilationConflictOutcome.Decompose,
                SanctuaryAssimilationConflictOutcome.Refuse
            ],
            deltaRule: "receipt-fields-govern-delta",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryDeltaBridgeDefinition DeltaBridge { get; } =
        new(
            handle: "delta.bridge.v0",
            gelEffectRule: "same-receipt-later-drives-structural-change-in-gel",
            selfGelEffectRule: "same-receipt-later-drives-continuity-change-in-selfgel",
            couplingRule: "gel-and-selfgel-later-receive-coupled-but-non-identical-effects-from-the-same-receipt",
            executableBoundary: "no-executable-delta-accumulation-or-condensation-in-this-slice",
            deferredSurfaces:
            [
                "delta-accumulation-engine",
                "condensation-engine",
                "stability-metrics",
                "skill-condensation"
            ],
            operationalStatus: "placeholder-contract-only");
}
