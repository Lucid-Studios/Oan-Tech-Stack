namespace GEL.Contracts.Sanctuary;

public enum SanctuaryGelDerivedLawKind
{
    BranchGrowth = 0,
    BranchPurity = 1,
    Pruning = 2,
    GroveRelation = 3,
    AnchorMergeRefusal = 4
}

public enum SanctuaryGelResidencyDisposition
{
    Remain = 0,
    Decompose = 1,
    Reroute = 2,
    Refuse = 3
}

public sealed record SanctuaryGelResidencyDiscernmentDefinition
{
    public SanctuaryGelResidencyDiscernmentDefinition(
        string handle,
        string governingQuestion,
        SanctuaryGelResidencyDisposition lawfulOutcome,
        IReadOnlyList<SanctuaryGelResidencyDisposition> driftOutcomes,
        string protectedInvariant,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        GoverningQuestion = SanctuaryContractGuard.RequiredText(governingQuestion, nameof(governingQuestion));
        LawfulOutcome = lawfulOutcome;
        DriftOutcomes = SanctuaryContractGuard.RequiredDistinctList(driftOutcomes, nameof(driftOutcomes));
        ProtectedInvariant = SanctuaryContractGuard.RequiredText(protectedInvariant, nameof(protectedInvariant));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string GoverningQuestion { get; }

    public SanctuaryGelResidencyDisposition LawfulOutcome { get; }

    public IReadOnlyList<SanctuaryGelResidencyDisposition> DriftOutcomes { get; }

    public string ProtectedInvariant { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryGelDerivedLawDefinition
{
    public SanctuaryGelDerivedLawDefinition(
        SanctuaryGelDerivedLawKind law,
        string handle,
        string requirement,
        IReadOnlyList<string> governingAxiomHandles,
        string admissionRule,
        IReadOnlyList<string> failureResponses,
        string operationalStatus)
    {
        Law = law;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Requirement = SanctuaryContractGuard.RequiredText(requirement, nameof(requirement));
        GoverningAxiomHandles = SanctuaryContractGuard.RequiredTextList(governingAxiomHandles, nameof(governingAxiomHandles));
        AdmissionRule = SanctuaryContractGuard.RequiredText(admissionRule, nameof(admissionRule));
        FailureResponses = SanctuaryContractGuard.RequiredTextList(failureResponses, nameof(failureResponses));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryGelDerivedLawKind Law { get; }

    public string Handle { get; }

    public string Requirement { get; }

    public IReadOnlyList<string> GoverningAxiomHandles { get; }

    public string AdmissionRule { get; }

    public IReadOnlyList<string> FailureResponses { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryGelDerivedLawAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryGelDerivedLawKind, SanctuaryGelDerivedLawDefinition> Definitions =
        new Dictionary<SanctuaryGelDerivedLawKind, SanctuaryGelDerivedLawDefinition>
        {
            [SanctuaryGelDerivedLawKind.BranchGrowth] = new(
                law: SanctuaryGelDerivedLawKind.BranchGrowth,
                handle: "l1.branch-growth",
                requirement: "branch-may-grow-from-trunk-only-when-anchored-to-it-and-without-redefining-it",
                governingAxiomHandles:
                [
                    "a0.layer-integrity",
                    "a0.root-identity-invariance",
                    "a0.downward-grounding"
                ],
                admissionRule: "dictionary-contextual-and-procedural-growth-must-preserve-trunk-and-layer",
                failureResponses:
                [
                    "refuse-growth",
                    "hold-provisional",
                    "return-for-reduction"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelDerivedLawKind.BranchPurity] = new(
                law: SanctuaryGelDerivedLawKind.BranchPurity,
                handle: "l2.branch-purity",
                requirement: "branch-must-remain-lawful-to-its-own-layer-under-residency-discernment",
                governingAxiomHandles:
                [
                    "a0.layer-integrity",
                    "a0.non-inflation",
                    "a0.discernment-admissibility"
                ],
                admissionRule: "retained-branch-remains-only-while-delta-g-matches-declared-layer",
                failureResponses:
                [
                    "decompose",
                    "reroute",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelDerivedLawKind.Pruning] = new(
                law: SanctuaryGelDerivedLawKind.Pruning,
                handle: "l3.pruning",
                requirement: "unlawful-branch-must-be-decomposed-rerouted-or-refused-not-retained-as-corruption",
                governingAxiomHandles:
                [
                    "a0.discernment-admissibility",
                    "a0.non-inflation",
                    "a0.posture-stability"
                ],
                admissionRule: "corrupted-retention-may-not-remain-by-convenience",
                failureResponses:
                [
                    "decompose",
                    "reroute",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelDerivedLawKind.GroveRelation] = new(
                law: SanctuaryGelDerivedLawKind.GroveRelation,
                handle: "l4.grove-relation",
                requirement: "distinct-trunks-may-relate-without-collapsing-identity",
                governingAxiomHandles:
                [
                    "a0.layer-integrity",
                    "a0.root-identity-invariance",
                    "a0.downward-grounding"
                ],
                admissionRule: "relation-must-preserve-distinct-anchor-membership",
                failureResponses:
                [
                    "refuse-relation",
                    "hold-provisional"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelDerivedLawKind.AnchorMergeRefusal] = new(
                law: SanctuaryGelDerivedLawKind.AnchorMergeRefusal,
                handle: "l5.anchor-merge-refusal",
                requirement: "distinct-anchors-do-not-merge-by-overlap-alone",
                governingAxiomHandles:
                [
                    "a0.root-identity-invariance",
                    "a0.translation-preservation",
                    "a0.discernment-admissibility"
                ],
                admissionRule: "merge-disallowed-without-strong-equivalence-proof",
                failureResponses:
                [
                    "refuse-merge",
                    "hold-provisional"
                ],
                operationalStatus: "placeholder-contract-only")
        };

    public static SanctuaryGelResidencyDiscernmentDefinition ResidencyDiscernment { get; } = new(
        handle: "delta-g.residency-discernment.v0",
        governingQuestion: "does-this-retained-object-still-belong-where-it-is",
        lawfulOutcome: SanctuaryGelResidencyDisposition.Remain,
        driftOutcomes:
        [
            SanctuaryGelResidencyDisposition.Decompose,
            SanctuaryGelResidencyDisposition.Reroute,
            SanctuaryGelResidencyDisposition.Refuse
        ],
        protectedInvariant: "retained-objects-must-remain-lawful-to-their-layer",
        operationalStatus: "placeholder-contract-only");

    public static IReadOnlyList<SanctuaryGelDerivedLawDefinition> ContractBackedLaws { get; } =
        Definitions.Values
            .OrderBy(static item => item.Law)
            .ToArray();

    public static bool TryGet(
        SanctuaryGelDerivedLawKind law,
        out SanctuaryGelDerivedLawDefinition definition)
    {
        return Definitions.TryGetValue(law, out definition!);
    }

    public static SanctuaryGelDerivedLawDefinition Get(SanctuaryGelDerivedLawKind law)
    {
        if (!TryGet(law, out var definition))
        {
            throw new KeyNotFoundException($"No GEL derived law definition exists for '{law}'.");
        }

        return definition;
    }
}
