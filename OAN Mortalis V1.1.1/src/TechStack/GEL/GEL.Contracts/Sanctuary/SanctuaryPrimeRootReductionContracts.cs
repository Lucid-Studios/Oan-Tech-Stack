namespace GEL.Contracts.Sanctuary;

public enum SanctuaryPrimeConstraintKind
{
    IrreducibleMeaning = 0,
    RootAtlasAnchorable = 1,
    NonInflatedRootLayer = 2,
    TranslationStable = 3
}

public enum SanctuarySymbolicReductionStage
{
    Utf8Intake = 0,
    Normalization = 1,
    Segmentation = 2,
    Compression = 3,
    Decomposition = 4,
    PrimeSelection = 5,
    RootAnchoring = 6,
    LayerPlacement = 7,
    TranslativeBinding = 8
}

public enum SanctuaryKappaTestKind
{
    DefinitionalInflation = 0,
    ContextualResidue = 1,
    ProceduralPressure = 2
}

public enum SanctuaryKappaEvidenceKind
{
    BoundaryExpansion = 0,
    EquivalenceClaim = 1,
    DescriptiveRestatement = 2,
    ExampleDependence = 3,
    HistoricalBinding = 4,
    NarrativeSituation = 5,
    ExternalRelationLoad = 6,
    Instructionality = 7,
    SequenceImplied = 8,
    GoalOrientation = 9,
    ExecutionPressure = 10
}

public enum SanctuaryResidualRoute
{
    Root = 0,
    Dictionary = 1,
    Encyclopedic = 2,
    Procedural = 3,
    Unresolved = 4
}

public enum SanctuaryDecompositionMode
{
    Definitional = 0,
    Contextual = 1,
    Procedural = 2,
    Mixed = 3
}

public enum SanctuaryAnchorEmergenceCriterionKind
{
    PrimeValid = 0,
    SemanticStable = 1,
    CrossOccurrenceConvergent = 2,
    NonSubstitutable = 3
}

public enum SanctuaryAnchorResolutionKind
{
    BindExisting = 0,
    CreateNew = 1,
    HoldUnresolved = 2
}

public sealed record SanctuaryPrimeRootCarrierDefinition
{
    public SanctuaryPrimeRootCarrierDefinition(
        string handle,
        string carrierSpace,
        IReadOnlyList<SanctuaryPrimeConstraintKind> constraints,
        IReadOnlyList<string> constraintRules,
        IReadOnlyList<string> failureRoutes,
        string rootAdmissionRule,
        string translationRule)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        CarrierSpace = SanctuaryContractGuard.RequiredText(carrierSpace, nameof(carrierSpace));
        Constraints = SanctuaryContractGuard.RequiredDistinctList(constraints, nameof(constraints));
        ConstraintRules = SanctuaryContractGuard.RequiredTextList(constraintRules, nameof(constraintRules));
        FailureRoutes = SanctuaryContractGuard.RequiredTextList(failureRoutes, nameof(failureRoutes));
        RootAdmissionRule = SanctuaryContractGuard.RequiredText(rootAdmissionRule, nameof(rootAdmissionRule));
        TranslationRule = SanctuaryContractGuard.RequiredText(translationRule, nameof(translationRule));
    }

    public string Handle { get; }

    public string CarrierSpace { get; }

    public IReadOnlyList<SanctuaryPrimeConstraintKind> Constraints { get; }

    public IReadOnlyList<string> ConstraintRules { get; }

    public IReadOnlyList<string> FailureRoutes { get; }

    public string RootAdmissionRule { get; }

    public string TranslationRule { get; }
}

public sealed record SanctuarySymbolicReductionPipelineDefinition
{
    public SanctuarySymbolicReductionPipelineDefinition(
        string handle,
        IReadOnlyList<SanctuarySymbolicReductionStage> stages,
        IReadOnlyList<string> stageIntents,
        string currentImplementationBoundary,
        string currentRuntimeStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Stages = SanctuaryContractGuard.RequiredDistinctList(stages, nameof(stages));
        StageIntents = SanctuaryContractGuard.RequiredTextList(stageIntents, nameof(stageIntents));
        CurrentImplementationBoundary = SanctuaryContractGuard.RequiredText(currentImplementationBoundary, nameof(currentImplementationBoundary));
        CurrentRuntimeStatus = SanctuaryContractGuard.RequiredText(currentRuntimeStatus, nameof(currentRuntimeStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuarySymbolicReductionStage> Stages { get; }

    public IReadOnlyList<string> StageIntents { get; }

    public string CurrentImplementationBoundary { get; }

    public string CurrentRuntimeStatus { get; }
}

public sealed record SanctuaryKappaTestDefinition
{
    public SanctuaryKappaTestDefinition(
        SanctuaryKappaTestKind kind,
        IReadOnlyList<SanctuaryKappaEvidenceKind> evidenceKinds,
        string rootRefusalRule,
        SanctuaryResidualRoute defaultResidualRoute,
        string operationalStatus)
    {
        Kind = kind;
        EvidenceKinds = SanctuaryContractGuard.RequiredDistinctList(evidenceKinds, nameof(evidenceKinds));
        RootRefusalRule = SanctuaryContractGuard.RequiredText(rootRefusalRule, nameof(rootRefusalRule));
        DefaultResidualRoute = defaultResidualRoute;
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryKappaTestKind Kind { get; }

    public IReadOnlyList<SanctuaryKappaEvidenceKind> EvidenceKinds { get; }

    public string RootRefusalRule { get; }

    public SanctuaryResidualRoute DefaultResidualRoute { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryKappaCompressionDefinition
{
    public SanctuaryKappaCompressionDefinition(
        string handle,
        IReadOnlyList<SanctuaryKappaTestKind> tests,
        string rootCleanSignature,
        string rootAdmissionRule,
        string mixedCandidateRule,
        string residualRoutingRule)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Tests = SanctuaryContractGuard.RequiredDistinctList(tests, nameof(tests));
        RootCleanSignature = SanctuaryContractGuard.RequiredText(rootCleanSignature, nameof(rootCleanSignature));
        RootAdmissionRule = SanctuaryContractGuard.RequiredText(rootAdmissionRule, nameof(rootAdmissionRule));
        MixedCandidateRule = SanctuaryContractGuard.RequiredText(mixedCandidateRule, nameof(mixedCandidateRule));
        ResidualRoutingRule = SanctuaryContractGuard.RequiredText(residualRoutingRule, nameof(residualRoutingRule));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryKappaTestKind> Tests { get; }

    public string RootCleanSignature { get; }

    public string RootAdmissionRule { get; }

    public string MixedCandidateRule { get; }

    public string ResidualRoutingRule { get; }
}

public sealed record SanctuaryDecompositionModeDefinition
{
    public SanctuaryDecompositionModeDefinition(
        SanctuaryDecompositionMode mode,
        IReadOnlyList<SanctuaryKappaTestKind> triggerTests,
        IReadOnlyList<string> childRoles,
        string intent,
        SanctuaryResidualRoute defaultResidualRoute,
        string operationalStatus)
    {
        Mode = mode;
        TriggerTests = SanctuaryContractGuard.RequiredDistinctList(triggerTests, nameof(triggerTests));
        ChildRoles = SanctuaryContractGuard.RequiredTextList(childRoles, nameof(childRoles));
        Intent = SanctuaryContractGuard.RequiredText(intent, nameof(intent));
        DefaultResidualRoute = defaultResidualRoute;
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryDecompositionMode Mode { get; }

    public IReadOnlyList<SanctuaryKappaTestKind> TriggerTests { get; }

    public IReadOnlyList<string> ChildRoles { get; }

    public string Intent { get; }

    public SanctuaryResidualRoute DefaultResidualRoute { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryDecompositionDefinition
{
    public SanctuaryDecompositionDefinition(
        string handle,
        IReadOnlyList<string> triggerVectors,
        string conservationRule,
        string discernmentGainRule,
        string stopRule,
        IReadOnlyList<string> failureModes,
        string currentRuntimeStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        TriggerVectors = SanctuaryContractGuard.RequiredTextList(triggerVectors, nameof(triggerVectors));
        ConservationRule = SanctuaryContractGuard.RequiredText(conservationRule, nameof(conservationRule));
        DiscernmentGainRule = SanctuaryContractGuard.RequiredText(discernmentGainRule, nameof(discernmentGainRule));
        StopRule = SanctuaryContractGuard.RequiredText(stopRule, nameof(stopRule));
        FailureModes = SanctuaryContractGuard.RequiredTextList(failureModes, nameof(failureModes));
        CurrentRuntimeStatus = SanctuaryContractGuard.RequiredText(currentRuntimeStatus, nameof(currentRuntimeStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> TriggerVectors { get; }

    public string ConservationRule { get; }

    public string DiscernmentGainRule { get; }

    public string StopRule { get; }

    public IReadOnlyList<string> FailureModes { get; }

    public string CurrentRuntimeStatus { get; }
}

public sealed record SanctuaryAnchorEmergenceDefinition
{
    public SanctuaryAnchorEmergenceDefinition(
        string handle,
        IReadOnlyList<SanctuaryAnchorEmergenceCriterionKind> criteria,
        IReadOnlyList<string> criterionRules,
        string alignmentRule,
        IReadOnlyList<string> resolutionBranches,
        string rootRealizationRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Criteria = SanctuaryContractGuard.RequiredDistinctList(criteria, nameof(criteria));
        CriterionRules = SanctuaryContractGuard.RequiredTextList(criterionRules, nameof(criterionRules));
        AlignmentRule = SanctuaryContractGuard.RequiredText(alignmentRule, nameof(alignmentRule));
        ResolutionBranches = SanctuaryContractGuard.RequiredTextList(resolutionBranches, nameof(resolutionBranches));
        RootRealizationRule = SanctuaryContractGuard.RequiredText(rootRealizationRule, nameof(rootRealizationRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryAnchorEmergenceCriterionKind> Criteria { get; }

    public IReadOnlyList<string> CriterionRules { get; }

    public string AlignmentRule { get; }

    public IReadOnlyList<string> ResolutionBranches { get; }

    public string RootRealizationRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryTrunkIdentityDefinition
{
    public SanctuaryTrunkIdentityDefinition(
        string handle,
        string meaning,
        IReadOnlyList<string> invariants,
        IReadOnlyList<string> allowedFutureGrowth,
        IReadOnlyList<string> forbiddenMutations,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
        Invariants = SanctuaryContractGuard.RequiredTextList(invariants, nameof(invariants));
        AllowedFutureGrowth = SanctuaryContractGuard.RequiredTextList(allowedFutureGrowth, nameof(allowedFutureGrowth));
        ForbiddenMutations = SanctuaryContractGuard.RequiredTextList(forbiddenMutations, nameof(forbiddenMutations));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string Meaning { get; }

    public IReadOnlyList<string> Invariants { get; }

    public IReadOnlyList<string> AllowedFutureGrowth { get; }

    public IReadOnlyList<string> ForbiddenMutations { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryPrimeRootReductionAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryKappaTestKind, SanctuaryKappaTestDefinition> KappaDefinitions =
        new Dictionary<SanctuaryKappaTestKind, SanctuaryKappaTestDefinition>
        {
            [SanctuaryKappaTestKind.DefinitionalInflation] = new(
                kind: SanctuaryKappaTestKind.DefinitionalInflation,
                evidenceKinds:
                [
                    SanctuaryKappaEvidenceKind.BoundaryExpansion,
                    SanctuaryKappaEvidenceKind.EquivalenceClaim,
                    SanctuaryKappaEvidenceKind.DescriptiveRestatement
                ],
                rootRefusalRule: "if-present-not-root",
                defaultResidualRoute: SanctuaryResidualRoute.Dictionary,
                operationalStatus: "placeholder-only"),
            [SanctuaryKappaTestKind.ContextualResidue] = new(
                kind: SanctuaryKappaTestKind.ContextualResidue,
                evidenceKinds:
                [
                    SanctuaryKappaEvidenceKind.ExampleDependence,
                    SanctuaryKappaEvidenceKind.HistoricalBinding,
                    SanctuaryKappaEvidenceKind.NarrativeSituation,
                    SanctuaryKappaEvidenceKind.ExternalRelationLoad
                ],
                rootRefusalRule: "if-present-not-root",
                defaultResidualRoute: SanctuaryResidualRoute.Encyclopedic,
                operationalStatus: "placeholder-only"),
            [SanctuaryKappaTestKind.ProceduralPressure] = new(
                kind: SanctuaryKappaTestKind.ProceduralPressure,
                evidenceKinds:
                [
                    SanctuaryKappaEvidenceKind.Instructionality,
                    SanctuaryKappaEvidenceKind.SequenceImplied,
                    SanctuaryKappaEvidenceKind.GoalOrientation,
                    SanctuaryKappaEvidenceKind.ExecutionPressure
                ],
                rootRefusalRule: "if-present-not-root",
                defaultResidualRoute: SanctuaryResidualRoute.Procedural,
                operationalStatus: "placeholder-only")
        };

    private static readonly IReadOnlyDictionary<SanctuaryDecompositionMode, SanctuaryDecompositionModeDefinition> DecompositionModeDefinitions =
        new Dictionary<SanctuaryDecompositionMode, SanctuaryDecompositionModeDefinition>
        {
            [SanctuaryDecompositionMode.Definitional] = new(
                mode: SanctuaryDecompositionMode.Definitional,
                triggerTests:
                [
                    SanctuaryKappaTestKind.DefinitionalInflation
                ],
                childRoles:
                [
                    "carrier",
                    "definition"
                ],
                intent: "separate carrier term from explanatory shell",
                defaultResidualRoute: SanctuaryResidualRoute.Dictionary,
                operationalStatus: "placeholder-only"),
            [SanctuaryDecompositionMode.Contextual] = new(
                mode: SanctuaryDecompositionMode.Contextual,
                triggerTests:
                [
                    SanctuaryKappaTestKind.ContextualResidue
                ],
                childRoles:
                [
                    "core",
                    "context"
                ],
                intent: "separate core semantic unit from narrative or relational environment",
                defaultResidualRoute: SanctuaryResidualRoute.Encyclopedic,
                operationalStatus: "placeholder-only"),
            [SanctuaryDecompositionMode.Procedural] = new(
                mode: SanctuaryDecompositionMode.Procedural,
                triggerTests:
                [
                    SanctuaryKappaTestKind.ProceduralPressure
                ],
                childRoles:
                [
                    "operation-anchor",
                    "procedure"
                ],
                intent: "separate operation-bearing term from instruction or method shell",
                defaultResidualRoute: SanctuaryResidualRoute.Procedural,
                operationalStatus: "placeholder-only"),
            [SanctuaryDecompositionMode.Mixed] = new(
                mode: SanctuaryDecompositionMode.Mixed,
                triggerTests:
                [
                    SanctuaryKappaTestKind.DefinitionalInflation,
                    SanctuaryKappaTestKind.ContextualResidue,
                    SanctuaryKappaTestKind.ProceduralPressure
                ],
                childRoles:
                [
                    "subcandidate-set"
                ],
                intent: "recursively split entangled candidates until children are more layer-pure",
                defaultResidualRoute: SanctuaryResidualRoute.Unresolved,
                operationalStatus: "placeholder-only")
        };

    public static SanctuaryPrimeRootCarrierDefinition PrimeRootCarrier { get; } = new(
        handle: "prime.root-carrier.utf8",
        carrierSpace: "utf8-token-space",
        constraints:
        [
            SanctuaryPrimeConstraintKind.IrreducibleMeaning,
            SanctuaryPrimeConstraintKind.RootAtlasAnchorable,
            SanctuaryPrimeConstraintKind.NonInflatedRootLayer,
            SanctuaryPrimeConstraintKind.TranslationStable
        ],
        constraintRules:
        [
            "cannot-decompose-without-meaning-loss",
            "must-bind-rootatlas-anchor",
            "must-not-encode-dictionary-encyclopedic-procedural-content",
            "must-preserve-anchor-and-root-kind-under-sli-translation"
        ],
        failureRoutes:
        [
            "definition-boundary->dictionary",
            "contextual-expansion->encyclopedic",
            "actionability->procedural",
            "unresolved->inadmissible"
        ],
        rootAdmissionRule: "x-in-root-iff-prime",
        translationRule: "tau-preserves-root-kind-and-anchor-lineage");

    public static SanctuarySymbolicReductionPipelineDefinition ReductionPipeline { get; } = new(
        handle: "symbolic-selection.reduction-pipeline.v0",
        stages:
        [
            SanctuarySymbolicReductionStage.Utf8Intake,
            SanctuarySymbolicReductionStage.Normalization,
            SanctuarySymbolicReductionStage.Segmentation,
            SanctuarySymbolicReductionStage.Compression,
            SanctuarySymbolicReductionStage.Decomposition,
            SanctuarySymbolicReductionStage.PrimeSelection,
            SanctuarySymbolicReductionStage.RootAnchoring,
            SanctuarySymbolicReductionStage.LayerPlacement,
            SanctuarySymbolicReductionStage.TranslativeBinding
        ],
        stageIntents:
        [
            "raw-utf8-field",
            "normalize-carrier-noise",
            "segment-candidate-units",
            "strip-definitional-contextual-procedural-load",
            "split-mixed-candidates-into-more-discernible-units",
            "test-prime-admissibility",
            "bind-rootatlas-anchor",
            "place-into-root-or-residual-layer",
            "translate-only-after-layering-and-anchoring"
        ],
        currentImplementationBoundary: "lexical-tokenization-and-atlas-match-only-before-prime-validation",
        currentRuntimeStatus: "placeholder-contract-only");

    public static IReadOnlyList<SanctuaryKappaTestDefinition> KappaTests { get; } =
        KappaDefinitions.Values
            .OrderBy(static item => item.Kind)
            .ToArray();

    public static SanctuaryKappaCompressionDefinition KappaCompression { get; } = new(
        handle: "kappa.compression.v0",
        tests:
        [
            SanctuaryKappaTestKind.DefinitionalInflation,
            SanctuaryKappaTestKind.ContextualResidue,
            SanctuaryKappaTestKind.ProceduralPressure
        ],
        rootCleanSignature: "0,0,0",
        rootAdmissionRule: "root-only-when-kappa-clean-and-anchorable",
        mixedCandidateRule: "mixed-vectors-decompose-or-hold-unresolved",
        residualRoutingRule: "single-dominant-failure-routes-upward-mixed-failure-routes-unresolved");

    public static IReadOnlyList<SanctuaryDecompositionModeDefinition> DecompositionModes { get; } =
        DecompositionModeDefinitions.Values
            .OrderBy(static item => item.Mode)
            .ToArray();

    public static SanctuaryDecompositionDefinition Decomposition { get; } = new(
        handle: "mixed-candidate.decomposition.v0",
        triggerVectors:
        [
            "1,1,0",
            "1,0,1",
            "0,1,1",
            "1,1,1"
        ],
        conservationRule: "must-preserve-anchorable-semantic-mass",
        discernmentGainRule: "children-must-be-more-discernible-than-parent",
        stopRule: "stop-when-each-child-is-prime-or-cleanly-routable-or-inadmissible",
        failureModes:
        [
            "over-fragmentation",
            "hidden-retention"
        ],
        currentRuntimeStatus: "placeholder-contract-only");

    public static SanctuaryAnchorEmergenceDefinition AnchorEmergence { get; } = new(
        handle: "anchor-emergence.v0",
        criteria:
        [
            SanctuaryAnchorEmergenceCriterionKind.PrimeValid,
            SanctuaryAnchorEmergenceCriterionKind.SemanticStable,
            SanctuaryAnchorEmergenceCriterionKind.CrossOccurrenceConvergent,
            SanctuaryAnchorEmergenceCriterionKind.NonSubstitutable
        ],
        criterionRules:
        [
            "must-already-satisfy-prime-root-carrier",
            "must-remain-semantically-stable-across-contexts",
            "must-recur-across-independent-reductions-within-one-governed-seed",
            "must-resist-lossless-substitution"
        ],
        alignmentRule: "align-by-convergent-behavior-not-string-identity",
        resolutionBranches:
        [
            "bind-existing-anchor-when-aligned",
            "create-new-anchor-when-emergent-and-unaligned",
            "hold-unresolved-when-ambiguous-or-incomplete"
        ],
        rootRealizationRule: "root-becomes-real-only-when-anchor-emerges",
        operationalStatus: "placeholder-contract-only");

    public static SanctuaryTrunkIdentityDefinition TrunkIdentity { get; } = new(
        handle: "trunk-identity.v0",
        meaning: "anchor-stable semantic identity discovered through convergence",
        invariants:
        [
            "trunk-stability",
            "translation-invariant-identity",
            "branches-must-not-redefine-trunk"
        ],
        allowedFutureGrowth:
        [
            "future-definition-branching",
            "future-context-branching",
            "future-procedural-branching"
        ],
        forbiddenMutations:
        [
            "trunk-redefinition",
            "anchor-collapse",
            "branch-overwrite-of-trunk"
        ],
        operationalStatus: "placeholder-contract-only");

    public static bool TryGetKappaTest(
        SanctuaryKappaTestKind kind,
        out SanctuaryKappaTestDefinition definition)
    {
        return KappaDefinitions.TryGetValue(kind, out definition!);
    }

    public static SanctuaryKappaTestDefinition GetKappaTest(SanctuaryKappaTestKind kind)
    {
        if (!TryGetKappaTest(kind, out var definition))
        {
            throw new KeyNotFoundException($"No kappa test definition exists for '{kind}'.");
        }

        return definition;
    }

    public static bool TryGetDecompositionMode(
        SanctuaryDecompositionMode mode,
        out SanctuaryDecompositionModeDefinition definition)
    {
        return DecompositionModeDefinitions.TryGetValue(mode, out definition!);
    }

    public static SanctuaryDecompositionModeDefinition GetDecompositionMode(SanctuaryDecompositionMode mode)
    {
        if (!TryGetDecompositionMode(mode, out var definition))
        {
            throw new KeyNotFoundException($"No decomposition mode definition exists for '{mode}'.");
        }

        return definition;
    }
}
