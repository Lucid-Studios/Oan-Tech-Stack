namespace GEL.Contracts.Sanctuary;

public enum SanctuaryGelDiscernmentLayer
{
    Root = 0,
    Dictionary = 1
}

public sealed record SanctuaryGelDiscernmentLayerDefinition
{
    public SanctuaryGelDiscernmentLayerDefinition(
        SanctuaryGelDiscernmentLayer layer,
        string purpose,
        IReadOnlyList<string> admissionExpectations,
        IReadOnlyList<string> forbiddenDrift,
        IReadOnlyList<SanctuaryGelDiscernmentLayer> groundingLayers,
        IReadOnlyList<SanctuaryGelDiscernmentLayer> reachableLayers,
        string seedUseExpectation)
    {
        Layer = layer;
        Purpose = SanctuaryContractGuard.RequiredText(purpose, nameof(purpose));
        AdmissionExpectations = SanctuaryContractGuard.RequiredTextList(admissionExpectations, nameof(admissionExpectations));
        ForbiddenDrift = SanctuaryContractGuard.RequiredTextList(forbiddenDrift, nameof(forbiddenDrift));
        GroundingLayers = SanctuaryContractGuard.DistinctListOrEmpty(groundingLayers);
        ReachableLayers = SanctuaryContractGuard.RequiredDistinctList(reachableLayers, nameof(reachableLayers));
        SeedUseExpectation = SanctuaryContractGuard.RequiredText(seedUseExpectation, nameof(seedUseExpectation));
    }

    public SanctuaryGelDiscernmentLayer Layer { get; }

    public string Purpose { get; }

    public IReadOnlyList<string> AdmissionExpectations { get; }

    public IReadOnlyList<string> ForbiddenDrift { get; }

    public IReadOnlyList<SanctuaryGelDiscernmentLayer> GroundingLayers { get; }

    public IReadOnlyList<SanctuaryGelDiscernmentLayer> ReachableLayers { get; }

    public string SeedUseExpectation { get; }
}

public sealed record SanctuaryGelTranslativeBindingPlaceholder
{
    public SanctuaryGelTranslativeBindingPlaceholder(
        SanctuaryGelDiscernmentLayer sourceLayer,
        string englishSurfaceKind,
        string rootAtlasAnchorKind,
        string sliCarrierPlaceholder,
        string admissibleEngramRole,
        IReadOnlyList<string> preservedInvariants,
        IReadOnlyList<string> forbiddenTransfers)
    {
        SourceLayer = sourceLayer;
        EnglishSurfaceKind = SanctuaryContractGuard.RequiredText(englishSurfaceKind, nameof(englishSurfaceKind));
        RootAtlasAnchorKind = SanctuaryContractGuard.RequiredText(rootAtlasAnchorKind, nameof(rootAtlasAnchorKind));
        SliCarrierPlaceholder = SanctuaryContractGuard.RequiredText(sliCarrierPlaceholder, nameof(sliCarrierPlaceholder));
        AdmissibleEngramRole = SanctuaryContractGuard.RequiredText(admissibleEngramRole, nameof(admissibleEngramRole));
        PreservedInvariants = SanctuaryContractGuard.RequiredTextList(preservedInvariants, nameof(preservedInvariants));
        ForbiddenTransfers = SanctuaryContractGuard.RequiredTextList(forbiddenTransfers, nameof(forbiddenTransfers));
    }

    public SanctuaryGelDiscernmentLayer SourceLayer { get; }

    public string EnglishSurfaceKind { get; }

    public string RootAtlasAnchorKind { get; }

    public string SliCarrierPlaceholder { get; }

    public string AdmissibleEngramRole { get; }

    public IReadOnlyList<string> PreservedInvariants { get; }

    public IReadOnlyList<string> ForbiddenTransfers { get; }
}

public static class SanctuaryGelDiscernmentLayerAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryGelDiscernmentLayer, SanctuaryGelDiscernmentLayerDefinition> Definitions =
        new Dictionary<SanctuaryGelDiscernmentLayer, SanctuaryGelDiscernmentLayerDefinition>
        {
            [SanctuaryGelDiscernmentLayer.Root] = new(
                layer: SanctuaryGelDiscernmentLayer.Root,
                purpose: "irreducible meaning unit, not yet definition or explanation",
                admissionExpectations:
                [
                    "minimal-semantic-atom",
                    "non-definitional",
                    "non-explanatory"
                ],
                forbiddenDrift:
                [
                    "dictionary-inflation",
                    "contextual-expansion",
                    "procedural-leap",
                    "narrative-overwrite"
                ],
                groundingLayers: [],
                reachableLayers:
                [
                    SanctuaryGelDiscernmentLayer.Root,
                    SanctuaryGelDiscernmentLayer.Dictionary
                ],
                seedUseExpectation: "hold without expanding; allow later stabilization into definition"),
            [SanctuaryGelDiscernmentLayer.Dictionary] = new(
                layer: SanctuaryGelDiscernmentLayer.Dictionary,
                purpose: "stabilized bounded definition grounded in Root",
                admissionExpectations:
                [
                    "term-boundary",
                    "disambiguated-definition",
                    "root-grounded"
                ],
                forbiddenDrift:
                [
                    "root-redefinition",
                    "encyclopedic-drift",
                    "procedural-answer",
                    "narrative-substitution"
                ],
                groundingLayers:
                [
                    SanctuaryGelDiscernmentLayer.Root
                ],
                reachableLayers:
                [
                    SanctuaryGelDiscernmentLayer.Dictionary,
                    SanctuaryGelDiscernmentLayer.Root
                ],
                seedUseExpectation: "stabilize meaning before context or procedure")
        };

    public static IReadOnlyList<SanctuaryGelDiscernmentLayerDefinition> All { get; } =
        Definitions.Values
            .OrderBy(static item => item.Layer)
            .ToArray();

    public static IReadOnlyList<SanctuaryGelTranslativeBindingPlaceholder> PlaceholderBindings { get; } =
    [
        new(
            sourceLayer: SanctuaryGelDiscernmentLayer.Root,
            englishSurfaceKind: "root-unit",
            rootAtlasAnchorKind: "root-anchor",
            sliCarrierPlaceholder: "sli.root.carrier",
            admissibleEngramRole: "root-engram-candidate",
            preservedInvariants:
            [
                "layer-kind",
                "root-identity"
            ],
            forbiddenTransfers:
            [
                "definition-substitution",
                "contextual-expansion",
                "procedural-promotion"
            ]),
        new(
            sourceLayer: SanctuaryGelDiscernmentLayer.Dictionary,
            englishSurfaceKind: "bounded-definition",
            rootAtlasAnchorKind: "definition-anchor",
            sliCarrierPlaceholder: "sli.definition.carrier",
            admissibleEngramRole: "definitional-engram-candidate",
            preservedInvariants:
            [
                "layer-kind",
                "definition-boundary",
                "root-grounding"
            ],
            forbiddenTransfers:
            [
                "root-redefinition",
                "encyclopedic-drift",
                "procedural-leap"
            ])
    ];

    public static bool TryGet(
        SanctuaryGelDiscernmentLayer layer,
        out SanctuaryGelDiscernmentLayerDefinition definition)
    {
        return Definitions.TryGetValue(layer, out definition!);
    }

    public static SanctuaryGelDiscernmentLayerDefinition Get(SanctuaryGelDiscernmentLayer layer)
    {
        if (!TryGet(layer, out var definition))
        {
            throw new KeyNotFoundException($"No GEL discernment layer definition exists for '{layer}'.");
        }

        return definition;
    }
}
