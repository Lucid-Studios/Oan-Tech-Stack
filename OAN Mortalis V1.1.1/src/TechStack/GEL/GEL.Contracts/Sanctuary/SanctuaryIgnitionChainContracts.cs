namespace GEL.Contracts.Sanctuary;

public enum SanctuaryIgnitionChainSlotKind
{
    Root = 0,
    Definition = 1,
    Relation = 2,
    Procedure = 3
}

public enum SanctuaryIgnitionChainWitnessKind
{
    Order = 0,
    Grounding = 1,
    Cleaving = 2,
    Recovery = 3,
    Simulation = 4
}

public enum SanctuaryIgnitionChainResult
{
    Success = 0,
    Failure = 1,
    Hold = 2
}

public sealed record SanctuaryIgnitionChainDefinition
{
    public SanctuaryIgnitionChainDefinition(
        string handle,
        IReadOnlyList<SanctuaryIgnitionChainSlotKind> slotOrder,
        IReadOnlyList<SanctuaryIgnitionChainWitnessKind> witnessKinds,
        string governingRule,
        string refusalBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        SlotOrder = SanctuaryContractGuard.RequiredDistinctList(slotOrder, nameof(slotOrder));
        WitnessKinds = SanctuaryContractGuard.RequiredDistinctList(witnessKinds, nameof(witnessKinds));
        GoverningRule = SanctuaryContractGuard.RequiredText(governingRule, nameof(governingRule));
        RefusalBoundary = SanctuaryContractGuard.RequiredText(refusalBoundary, nameof(refusalBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryIgnitionChainSlotKind> SlotOrder { get; }

    public IReadOnlyList<SanctuaryIgnitionChainWitnessKind> WitnessKinds { get; }

    public string GoverningRule { get; }

    public string RefusalBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryIgnitionChainPrimeFaceDefinition
{
    public SanctuaryIgnitionChainPrimeFaceDefinition(
        string handle,
        string order,
        IReadOnlyList<string> statuses,
        string role,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Order = SanctuaryContractGuard.RequiredText(order, nameof(order));
        Statuses = SanctuaryContractGuard.RequiredTextList(statuses, nameof(statuses));
        Role = SanctuaryContractGuard.RequiredText(role, nameof(role));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string Order { get; }

    public IReadOnlyList<string> Statuses { get; }

    public string Role { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryIgnitionChainCrypticFaceDefinition
{
    public SanctuaryIgnitionChainCrypticFaceDefinition(
        string handle,
        IReadOnlyList<string> minimalSlots,
        string integrityRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        MinimalSlots = SanctuaryContractGuard.RequiredTextList(minimalSlots, nameof(minimalSlots));
        IntegrityRule = SanctuaryContractGuard.RequiredText(integrityRule, nameof(integrityRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> MinimalSlots { get; }

    public string IntegrityRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryIgnitionChainReceiptDefinition
{
    public SanctuaryIgnitionChainReceiptDefinition(
        string handle,
        IReadOnlyList<string> memberSlots,
        IReadOnlyList<string> checkSlots,
        string resultGrammar,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        MemberSlots = SanctuaryContractGuard.RequiredTextList(memberSlots, nameof(memberSlots));
        CheckSlots = SanctuaryContractGuard.RequiredTextList(checkSlots, nameof(checkSlots));
        ResultGrammar = SanctuaryContractGuard.RequiredText(resultGrammar, nameof(resultGrammar));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> MemberSlots { get; }

    public IReadOnlyList<string> CheckSlots { get; }

    public string ResultGrammar { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryIgnitionChainAtlas
{
    public static SanctuaryIgnitionChainDefinition Template { get; } =
        new(
            handle: "ignition-chain.template.v0",
            slotOrder:
            [
                SanctuaryIgnitionChainSlotKind.Root,
                SanctuaryIgnitionChainSlotKind.Definition,
                SanctuaryIgnitionChainSlotKind.Relation,
                SanctuaryIgnitionChainSlotKind.Procedure
            ],
            witnessKinds:
            [
                SanctuaryIgnitionChainWitnessKind.Order,
                SanctuaryIgnitionChainWitnessKind.Grounding,
                SanctuaryIgnitionChainWitnessKind.Cleaving,
                SanctuaryIgnitionChainWitnessKind.Recovery,
                SanctuaryIgnitionChainWitnessKind.Simulation
            ],
            governingRule: "root-definition-relation-and-procedure-must-form-one-grounded-action-bearing-chain",
            refusalBoundary: "no-live-execution-no-bonded-minting-no-slot-bypass",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryIgnitionChainPrimeFaceDefinition PrimeFace { get; } =
        new(
            handle: "ignition-chain.prime-face.v0",
            order: "R,D,E,P",
            statuses:
            [
                "valid",
                "provisional",
                "refused"
            ],
            role: "ignition-chain",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryIgnitionChainCrypticFaceDefinition CrypticFace { get; } =
        new(
            handle: "ignition-chain.cryptic-face.v0",
            minimalSlots:
            [
                "combined-anchor-keys",
                "dependency-keys",
                "trace-keys",
                "full-chain-seal"
            ],
            integrityRule: "full-chain-seal-preserves-lineage-through-action",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryIgnitionChainReceiptDefinition Receipt { get; } =
        new(
            handle: "ignition-chain.receipt.v0",
            memberSlots:
            [
                "r",
                "d",
                "e",
                "p"
            ],
            checkSlots:
            [
                "posture",
                "chain",
                "lineage",
                "simulation",
                "recovery"
            ],
            resultGrammar: "success|failure|hold",
            operationalStatus: "placeholder-contract-only");
}
