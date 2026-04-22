namespace GEL.Contracts.Sanctuary;

public enum SanctuaryGelAxiomKind
{
    LayerIntegrity = 0,
    RootIdentityInvariance = 1,
    DownwardGrounding = 2,
    DiscernmentAdmissibility = 3,
    NonInflation = 4,
    TranslationPreservation = 5,
    PostureStability = 6
}

public sealed record SanctuaryGelAxiomDefinition
{
    public SanctuaryGelAxiomDefinition(
        SanctuaryGelAxiomKind axiom,
        string handle,
        string requirement,
        IReadOnlyList<string> preservedSurfaces,
        IReadOnlyList<string> failureConsequences,
        string enforcementStatus)
    {
        Axiom = axiom;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Requirement = SanctuaryContractGuard.RequiredText(requirement, nameof(requirement));
        PreservedSurfaces = SanctuaryContractGuard.RequiredTextList(preservedSurfaces, nameof(preservedSurfaces));
        FailureConsequences = SanctuaryContractGuard.RequiredTextList(failureConsequences, nameof(failureConsequences));
        EnforcementStatus = SanctuaryContractGuard.RequiredText(enforcementStatus, nameof(enforcementStatus));
    }

    public SanctuaryGelAxiomKind Axiom { get; }

    public string Handle { get; }

    public string Requirement { get; }

    public IReadOnlyList<string> PreservedSurfaces { get; }

    public IReadOnlyList<string> FailureConsequences { get; }

    public string EnforcementStatus { get; }
}

public static class SanctuaryGelAxiomAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryGelAxiomKind, SanctuaryGelAxiomDefinition> Definitions =
        new Dictionary<SanctuaryGelAxiomKind, SanctuaryGelAxiomDefinition>
        {
            [SanctuaryGelAxiomKind.LayerIntegrity] = new(
                axiom: SanctuaryGelAxiomKind.LayerIntegrity,
                handle: "a0.layer-integrity",
                requirement: "a-thing-must-be-treated-as-the-kind-of-knowing-it-is",
                preservedSurfaces:
                [
                    "layer-discernment",
                    "non-masquerading"
                ],
                failureConsequences:
                [
                    "cross-layer-drift",
                    "false-classification"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.RootIdentityInvariance] = new(
                axiom: SanctuaryGelAxiomKind.RootIdentityInvariance,
                handle: "a0.root-identity-invariance",
                requirement: "anchored-root-identity-must-not-change-under-allowed-transformations",
                preservedSurfaces:
                [
                    "trunk-stability",
                    "anchor-continuity"
                ],
                failureConsequences:
                [
                    "root-mutation",
                    "anchor-collapse"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.DownwardGrounding] = new(
                axiom: SanctuaryGelAxiomKind.DownwardGrounding,
                handle: "a0.downward-grounding",
                requirement: "every-higher-layer-object-must-resolve-downward-to-root-support",
                preservedSurfaces:
                [
                    "dependency-order",
                    "non-floating-structure"
                ],
                failureConsequences:
                [
                    "floating-definition",
                    "floating-context",
                    "floating-procedure"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.DiscernmentAdmissibility] = new(
                axiom: SanctuaryGelAxiomKind.DiscernmentAdmissibility,
                handle: "a0.discernment-admissibility",
                requirement: "only-what-passes-discernment-may-enter-or-remain",
                preservedSurfaces:
                [
                    "lawful-admission",
                    "lawful-retention"
                ],
                failureConsequences:
                [
                    "unlawful-persistence",
                    "trust-collapse"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.NonInflation] = new(
                axiom: SanctuaryGelAxiomKind.NonInflation,
                handle: "a0.non-inflation",
                requirement: "no-object-may-carry-hidden-content-from-a-higher-layer",
                preservedSurfaces:
                [
                    "layer-cleanliness",
                    "root-clean-signature"
                ],
                failureConsequences:
                [
                    "hidden-definition",
                    "hidden-context",
                    "hidden-procedure"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.TranslationPreservation] = new(
                axiom: SanctuaryGelAxiomKind.TranslationPreservation,
                handle: "a0.translation-preservation",
                requirement: "translation-must-preserve-layer-anchor-and-dependency-structure",
                preservedSurfaces:
                [
                    "sli-integrity",
                    "cross-theater-continuity"
                ],
                failureConsequences:
                [
                    "translation-collapse",
                    "identity-loss"
                ],
                enforcementStatus: "placeholder-contract-only"),
            [SanctuaryGelAxiomKind.PostureStability] = new(
                axiom: SanctuaryGelAxiomKind.PostureStability,
                handle: "a0.posture-stability",
                requirement: "the-acting-system-must-hold-without-premature-action-and-recover-from-drift",
                preservedSurfaces:
                [
                    "prime-posture",
                    "recoverability"
                ],
                failureConsequences:
                [
                    "forced-action",
                    "procedural-collapse"
                ],
                enforcementStatus: "placeholder-contract-only")
        };

    public static IReadOnlyList<SanctuaryGelAxiomDefinition> All { get; } =
        Definitions.Values
            .OrderBy(static item => item.Axiom)
            .ToArray();

    public static bool TryGet(
        SanctuaryGelAxiomKind axiom,
        out SanctuaryGelAxiomDefinition definition)
    {
        return Definitions.TryGetValue(axiom, out definition!);
    }

    public static SanctuaryGelAxiomDefinition Get(SanctuaryGelAxiomKind axiom)
    {
        if (!TryGet(axiom, out var definition))
        {
            throw new KeyNotFoundException($"No GEL axiom definition exists for '{axiom}'.");
        }

        return definition;
    }
}
