namespace GEL.Contracts.Sanctuary;

public enum SanctuaryStabilityMetricKind
{
    Convergence = 0,
    BoundaryStability = 1,
    ConflictResolutionSaturation = 2,
    TraceConsistency = 3,
    LineageIntegrity = 4,
    PostureStability = 5
}

public enum SanctuaryCondensationTargetKind
{
    Root = 0,
    Definition = 1,
    Relation = 2,
    Procedure = 3
}

public sealed record SanctuaryStabilityMetricDefinition
{
    public SanctuaryStabilityMetricDefinition(
        SanctuaryStabilityMetricKind metric,
        string handle,
        string meaning)
    {
        Metric = metric;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
    }

    public SanctuaryStabilityMetricKind Metric { get; }

    public string Handle { get; }

    public string Meaning { get; }
}

public sealed record SanctuaryStabilityVectorDefinition
{
    public SanctuaryStabilityVectorDefinition(
        string handle,
        IReadOnlyList<SanctuaryStabilityMetricKind> orderedMetrics,
        string formula,
        string truthBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        OrderedMetrics = SanctuaryContractGuard.RequiredDistinctList(orderedMetrics, nameof(orderedMetrics));
        Formula = SanctuaryContractGuard.RequiredText(formula, nameof(formula));
        TruthBoundary = SanctuaryContractGuard.RequiredText(truthBoundary, nameof(truthBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryStabilityMetricKind> OrderedMetrics { get; }

    public string Formula { get; }

    public string TruthBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryCondensationThresholdDefinition
{
    public SanctuaryCondensationThresholdDefinition(
        string handle,
        IReadOnlyList<string> thresholdRules,
        IReadOnlyList<string> lowBandActions,
        IReadOnlyList<string> mediumBandActions,
        IReadOnlyList<string> highBandActions,
        string truthBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ThresholdRules = SanctuaryContractGuard.RequiredTextList(thresholdRules, nameof(thresholdRules));
        LowBandActions = SanctuaryContractGuard.RequiredTextList(lowBandActions, nameof(lowBandActions));
        MediumBandActions = SanctuaryContractGuard.RequiredTextList(mediumBandActions, nameof(mediumBandActions));
        HighBandActions = SanctuaryContractGuard.RequiredTextList(highBandActions, nameof(highBandActions));
        TruthBoundary = SanctuaryContractGuard.RequiredText(truthBoundary, nameof(truthBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> ThresholdRules { get; }

    public IReadOnlyList<string> LowBandActions { get; }

    public IReadOnlyList<string> MediumBandActions { get; }

    public IReadOnlyList<string> HighBandActions { get; }

    public string TruthBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryCanonicalCondensationOutputDefinition
{
    public SanctuaryCanonicalCondensationOutputDefinition(
        string handle,
        IReadOnlyList<string> primeFaceSlots,
        IReadOnlyList<string> crypticFaceSlots,
        IReadOnlyList<string> receiptSlots,
        IReadOnlyList<string> usageContractSlots,
        string lineageRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        PrimeFaceSlots = SanctuaryContractGuard.RequiredTextList(primeFaceSlots, nameof(primeFaceSlots));
        CrypticFaceSlots = SanctuaryContractGuard.RequiredTextList(crypticFaceSlots, nameof(crypticFaceSlots));
        ReceiptSlots = SanctuaryContractGuard.RequiredTextList(receiptSlots, nameof(receiptSlots));
        UsageContractSlots = SanctuaryContractGuard.RequiredTextList(usageContractSlots, nameof(usageContractSlots));
        LineageRule = SanctuaryContractGuard.RequiredText(lineageRule, nameof(lineageRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> PrimeFaceSlots { get; }

    public IReadOnlyList<string> CrypticFaceSlots { get; }

    public IReadOnlyList<string> ReceiptSlots { get; }

    public IReadOnlyList<string> UsageContractSlots { get; }

    public string LineageRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryTypeSpecificCondensationDefinition
{
    public SanctuaryTypeSpecificCondensationDefinition(
        SanctuaryCondensationTargetKind target,
        string handle,
        string resultHandle,
        string governingRule,
        string operationalStatus)
    {
        Target = target;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ResultHandle = SanctuaryContractGuard.RequiredText(resultHandle, nameof(resultHandle));
        GoverningRule = SanctuaryContractGuard.RequiredText(governingRule, nameof(governingRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryCondensationTargetKind Target { get; }

    public string Handle { get; }

    public string ResultHandle { get; }

    public string GoverningRule { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryStabilityCondensationAtlas
{
    public static IReadOnlyList<SanctuaryStabilityMetricDefinition> Metrics { get; } =
    [
        new(
            SanctuaryStabilityMetricKind.Convergence,
            "stability.convergence",
            "repeated-lawful-processing-returns-the-same-shape"),
        new(
            SanctuaryStabilityMetricKind.BoundaryStability,
            "stability.boundary",
            "definition-or-scope-boundary-no-longer-drifts"),
        new(
            SanctuaryStabilityMetricKind.ConflictResolutionSaturation,
            "stability.conflict-resolution",
            "known-conflicts-have-been-processed-without-further-splintering"),
        new(
            SanctuaryStabilityMetricKind.TraceConsistency,
            "stability.trace",
            "trace-keys-remain-consistent-across-supporting-paths"),
        new(
            SanctuaryStabilityMetricKind.LineageIntegrity,
            "stability.lineage",
            "anchor-lineage-remains-intact-and-unbroken"),
        new(
            SanctuaryStabilityMetricKind.PostureStability,
            "stability.posture",
            "prime-posture-remains-stable-through-processing")
    ];

    public static SanctuaryStabilityVectorDefinition StabilityVector { get; } =
        new(
            handle: "stability.vector.v0",
            orderedMetrics:
            [
                SanctuaryStabilityMetricKind.Convergence,
                SanctuaryStabilityMetricKind.BoundaryStability,
                SanctuaryStabilityMetricKind.ConflictResolutionSaturation,
                SanctuaryStabilityMetricKind.TraceConsistency,
                SanctuaryStabilityMetricKind.LineageIntegrity,
                SanctuaryStabilityMetricKind.PostureStability
            ],
            formula: "S(X)=(C,B,R,T,L,P)",
            truthBoundary: "stability-does-not-override-truth",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryCondensationThresholdDefinition CondensationThreshold { get; } =
        new(
            handle: "condensation.threshold.v0",
            thresholdRules:
            [
                "C>=theta_C",
                "B=1",
                "R>=theta_R",
                "T>=theta_T",
                "L=1",
                "P=1",
                "Phi(X)=1"
            ],
            lowBandActions:
            [
                "keep-accumulating",
                "allow-refinement",
                "allow-decomposition"
            ],
            mediumBandActions:
            [
                "begin-pruning",
                "test-alternate-refinements",
                "watch-divergence"
            ],
            highBandActions:
            [
                "condense",
                "canonicalize",
                "strengthen-anchor"
            ],
            truthBoundary: "stability-does-not-override-truth",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryCanonicalCondensationOutputDefinition CanonicalOutput { get; } =
        new(
            handle: "condensation.output.v0",
            primeFaceSlots:
            [
                "id",
                "layer",
                "anchors",
                "role",
                "boundary"
            ],
            crypticFaceSlots:
            [
                "anchor-keys",
                "dependency-keys",
                "trace-keys",
                "condensate-seal"
            ],
            receiptSlots:
            [
                "event-id",
                "contributing-set",
                "stability-vector",
                "phi",
                "conflict-outcomes",
                "result=condensed"
            ],
            usageContractSlots:
            [
                "allowed-ops",
                "layer-constraints",
                "posture-req",
                "scope"
            ],
            lineageRule: "anchor-lineage-must-be-preserved-exactly",
            operationalStatus: "placeholder-contract-only");

    public static IReadOnlyList<SanctuaryTypeSpecificCondensationDefinition> TypeSpecificOutputs { get; } =
    [
        new(
            SanctuaryCondensationTargetKind.Root,
            "condensation.root.v0",
            "r_c",
            "collapse-duplicates-to-one-anchor-strengthen-convergence-keep-anchor-keys-exact",
            "placeholder-contract-only"),
        new(
            SanctuaryCondensationTargetKind.Definition,
            "condensation.definition.v0",
            "d_c",
            "merge-compatible-refinements-select-tightest-boundary-drop-narrative-residue",
            "placeholder-contract-only"),
        new(
            SanctuaryCondensationTargetKind.Relation,
            "condensation.relation.v0",
            "e_c",
            "unify-relation-graph-keep-distinct-anchors-normalize-relation-types",
            "placeholder-contract-only"),
        new(
            SanctuaryCondensationTargetKind.Procedure,
            "condensation.procedure.v0",
            "p_c",
            "keep-only-grounded-steps-compress-to-minimal-transform-high-trace-consistency-posture-gated",
            "placeholder-contract-only")
    ];
}
