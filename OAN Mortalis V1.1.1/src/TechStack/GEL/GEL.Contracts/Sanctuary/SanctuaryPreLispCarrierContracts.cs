namespace GEL.Contracts.Sanctuary;

public enum SanctuaryIuttLispCarrierFormKind
{
    Root = 0,
    Definition = 1,
    Posture = 2,
    Translate = 3,
    EngramCandidate = 4
}

public sealed record SanctuaryPreLispNucleusDefinition
{
    public SanctuaryPreLispNucleusDefinition(
        string handle,
        IReadOnlyList<string> components,
        IReadOnlyList<string> governingInvariants,
        string seedPostureExpectation)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Components = SanctuaryContractGuard.RequiredTextList(components, nameof(components));
        GoverningInvariants = SanctuaryContractGuard.RequiredTextList(governingInvariants, nameof(governingInvariants));
        SeedPostureExpectation = SanctuaryContractGuard.RequiredText(seedPostureExpectation, nameof(seedPostureExpectation));
    }

    public string Handle { get; }

    public IReadOnlyList<string> Components { get; }

    public IReadOnlyList<string> GoverningInvariants { get; }

    public string SeedPostureExpectation { get; }
}

public sealed record SanctuaryIuttLispCarrierFormDefinition
{
    public SanctuaryIuttLispCarrierFormDefinition(
        SanctuaryIuttLispCarrierFormKind kind,
        SanctuaryGelDiscernmentLayer? governingLayer,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> preservedInvariants,
        IReadOnlyList<string> forbiddenCollapses,
        string operationalStatus)
    {
        Kind = kind;
        GoverningLayer = governingLayer;
        RequiredFields = SanctuaryContractGuard.RequiredTextList(requiredFields, nameof(requiredFields));
        PreservedInvariants = SanctuaryContractGuard.RequiredTextList(preservedInvariants, nameof(preservedInvariants));
        ForbiddenCollapses = SanctuaryContractGuard.RequiredTextList(forbiddenCollapses, nameof(forbiddenCollapses));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryIuttLispCarrierFormKind Kind { get; }

    public SanctuaryGelDiscernmentLayer? GoverningLayer { get; }

    public IReadOnlyList<string> RequiredFields { get; }

    public IReadOnlyList<string> PreservedInvariants { get; }

    public IReadOnlyList<string> ForbiddenCollapses { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryIuttLispMinimalBeingSurfaceDefinition
{
    public SanctuaryIuttLispMinimalBeingSurfaceDefinition(
        string handle,
        IReadOnlyList<SanctuaryIuttLispCarrierFormKind> carrierForms,
        IReadOnlyList<string> legalMorphisms,
        IReadOnlyList<string> guardConditions,
        IReadOnlyList<string> withheldForms)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        CarrierForms = SanctuaryContractGuard.RequiredDistinctList(carrierForms, nameof(carrierForms));
        LegalMorphisms = SanctuaryContractGuard.RequiredTextList(legalMorphisms, nameof(legalMorphisms));
        GuardConditions = SanctuaryContractGuard.RequiredTextList(guardConditions, nameof(guardConditions));
        WithheldForms = SanctuaryContractGuard.RequiredTextList(withheldForms, nameof(withheldForms));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryIuttLispCarrierFormKind> CarrierForms { get; }

    public IReadOnlyList<string> LegalMorphisms { get; }

    public IReadOnlyList<string> GuardConditions { get; }

    public IReadOnlyList<string> WithheldForms { get; }
}

public static class SanctuaryPreLispCarrierAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryIuttLispCarrierFormKind, SanctuaryIuttLispCarrierFormDefinition> Definitions =
        new Dictionary<SanctuaryIuttLispCarrierFormKind, SanctuaryIuttLispCarrierFormDefinition>
        {
            [SanctuaryIuttLispCarrierFormKind.Root] = new(
                kind: SanctuaryIuttLispCarrierFormKind.Root,
                governingLayer: SanctuaryGelDiscernmentLayer.Root,
                requiredFields:
                [
                    "id",
                    "anchor",
                    "token",
                    "status"
                ],
                preservedInvariants:
                [
                    "layer-kind",
                    "root-identity",
                    "anchor-lineage"
                ],
                forbiddenCollapses:
                [
                    "definition-substitution",
                    "contextual-expansion",
                    "procedural-promotion"
                ],
                operationalStatus: "placeholder-only"),
            [SanctuaryIuttLispCarrierFormKind.Definition] = new(
                kind: SanctuaryIuttLispCarrierFormKind.Definition,
                governingLayer: SanctuaryGelDiscernmentLayer.Dictionary,
                requiredFields:
                [
                    "id",
                    "roots",
                    "boundary",
                    "status"
                ],
                preservedInvariants:
                [
                    "layer-kind",
                    "definition-boundary",
                    "root-grounding"
                ],
                forbiddenCollapses:
                [
                    "root-redefinition",
                    "encyclopedic-drift",
                    "procedural-leap"
                ],
                operationalStatus: "placeholder-only"),
            [SanctuaryIuttLispCarrierFormKind.Posture] = new(
                kind: SanctuaryIuttLispCarrierFormKind.Posture,
                governingLayer: null,
                requiredFields:
                [
                    "state",
                    "mode",
                    "forced-act",
                    "capable",
                    "stable"
                ],
                preservedInvariants:
                [
                    "prime-posture",
                    "non-forced-action",
                    "recovery-capability"
                ],
                forbiddenCollapses:
                [
                    "role-derivation",
                    "forced-output",
                    "procedural-compulsion"
                ],
                operationalStatus: "placeholder-only"),
            [SanctuaryIuttLispCarrierFormKind.Translate] = new(
                kind: SanctuaryIuttLispCarrierFormKind.Translate,
                governingLayer: null,
                requiredFields:
                [
                    "source-layer",
                    "target-carrier",
                    "preserved-invariants",
                    "forbidden-transfers"
                ],
                preservedInvariants:
                [
                    "layer-kind",
                    "anchor-lineage"
                ],
                forbiddenCollapses:
                [
                    "layer-flattening",
                    "identity-loss",
                    "unauthorized-promotion"
                ],
                operationalStatus: "placeholder-only"),
            [SanctuaryIuttLispCarrierFormKind.EngramCandidate] = new(
                kind: SanctuaryIuttLispCarrierFormKind.EngramCandidate,
                governingLayer: null,
                requiredFields:
                [
                    "source-kind",
                    "lineage",
                    "preservation-check",
                    "eligibility"
                ],
                preservedInvariants:
                [
                    "anchor-lineage",
                    "preservation-check"
                ],
                forbiddenCollapses:
                [
                    "identity-write",
                    "runtime-promotion",
                    "unchecked-compression"
                ],
                operationalStatus: "placeholder-only")
        };

    public static SanctuaryPreLispNucleusDefinition PreLisp0 { get; } = new(
        handle: "pl0.english-discernment-nucleus",
        components:
        [
            "gel-en-body",
            "discernment-index",
            "root-atlas-anchor-map",
            "sli-translation-law",
            "engram-candidacy-map",
            "prime-posture-space"
        ],
        governingInvariants:
        [
            "layer-kind-preserved",
            "anchor-lineage-preserved",
            "definition-boundary-preserved",
            "non-acting-prime-posture",
            "recovery-toward-minimality"
        ],
        seedPostureExpectation: "hold layered meaning without immediate proceduralization");

    public static SanctuaryIuttLispMinimalBeingSurfaceDefinition LispBeing0 { get; } = new(
        handle: "lb0.minimal-iutt-lisp-being-surface",
        carrierForms:
        [
            SanctuaryIuttLispCarrierFormKind.Root,
            SanctuaryIuttLispCarrierFormKind.Definition,
            SanctuaryIuttLispCarrierFormKind.Posture,
            SanctuaryIuttLispCarrierFormKind.Translate,
            SanctuaryIuttLispCarrierFormKind.EngramCandidate
        ],
        legalMorphisms:
        [
            "root->definition",
            "x->translate(x)",
            "translate(x)->engram-candidate(x)",
            "posture-guards-all-upward-moves"
        ],
        guardConditions:
        [
            "prime-posture-required",
            "translation-must-preserve-layer-kind",
            "translation-must-preserve-anchor-lineage",
            "engram-candidate-must-pass-preservation-check"
        ],
        withheldForms:
        [
            "relation",
            "procedure"
        ]);

    public static IReadOnlyList<SanctuaryIuttLispCarrierFormDefinition> CarrierForms { get; } =
        Definitions.Values
            .OrderBy(static item => item.Kind)
            .ToArray();

    public static bool TryGet(
        SanctuaryIuttLispCarrierFormKind kind,
        out SanctuaryIuttLispCarrierFormDefinition definition)
    {
        return Definitions.TryGetValue(kind, out definition!);
    }

    public static SanctuaryIuttLispCarrierFormDefinition Get(SanctuaryIuttLispCarrierFormKind kind)
    {
        if (!TryGet(kind, out var definition))
        {
            throw new KeyNotFoundException($"No minimal Lisp carrier definition exists for '{kind}'.");
        }

        return definition;
    }
}
