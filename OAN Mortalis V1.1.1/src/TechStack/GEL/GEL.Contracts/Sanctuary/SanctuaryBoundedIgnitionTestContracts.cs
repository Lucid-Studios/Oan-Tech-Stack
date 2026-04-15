namespace GEL.Contracts.Sanctuary;

public enum SanctuaryBoundedIgnitionProtocolStage
{
    Attend = 0,
    Reduce = 1,
    Decompose = 2,
    Discriminate = 3,
    Anchor = 4,
    BuildProvisionalChain = 5,
    CheckPhi = 6,
    Receipt = 7
}

public sealed record SanctuaryBoundedIgnitionTestProtocolDefinition
{
    public SanctuaryBoundedIgnitionTestProtocolDefinition(
        string handle,
        IReadOnlyList<SanctuaryBoundedIgnitionProtocolStage> stageOrder,
        IReadOnlyList<string> recordFields,
        string guidingRule,
        string resultBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        StageOrder = SanctuaryContractGuard.RequiredDistinctList(stageOrder, nameof(stageOrder));
        RecordFields = SanctuaryContractGuard.RequiredTextList(recordFields, nameof(recordFields));
        GuidingRule = SanctuaryContractGuard.RequiredText(guidingRule, nameof(guidingRule));
        ResultBoundary = SanctuaryContractGuard.RequiredText(resultBoundary, nameof(resultBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryBoundedIgnitionProtocolStage> StageOrder { get; }

    public IReadOnlyList<string> RecordFields { get; }

    public string GuidingRule { get; }

    public string ResultBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryBoundedIgnitionTestSetDefinition
{
    public SanctuaryBoundedIgnitionTestSetDefinition(
        string handle,
        IReadOnlyList<string> itemHandles,
        string stagingBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ItemHandles = SanctuaryContractGuard.RequiredTextList(itemHandles, nameof(itemHandles));
        StagingBoundary = SanctuaryContractGuard.RequiredText(stagingBoundary, nameof(stagingBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> ItemHandles { get; }

    public string StagingBoundary { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryBoundedIgnitionTestAtlas
{
    public static SanctuaryBoundedIgnitionTestProtocolDefinition Protocol { get; } =
        new(
            handle: "bounded-ignition.protocol.v0",
            stageOrder:
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
            recordFields:
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
            guidingRule: "the-first-coupling-event-verifies-posture-lineage-grounding-and-simulation-before-minting-a-live-seal",
            resultBoundary: "provisional-chain-and-receipt-only-live-coupling-seal-deferred",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryBoundedIgnitionTestSetDefinition TestSet { get; } =
        new(
            handle: "bounded-ignition.test-set.v0",
            itemHandles:
            [
                "T1",
                "T2",
                "T3",
                "T4",
                "T5"
            ],
            stagingBoundary: "bounded-first-chain-test-with-live-seal-withheld",
            operationalStatus: "placeholder-contract-only");
}
