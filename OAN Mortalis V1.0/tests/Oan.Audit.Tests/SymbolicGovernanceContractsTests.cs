using GEL.Models;

namespace Oan.Audit.Tests;

public sealed class SymbolicGovernanceContractsTests
{
    [Fact]
    public void SymbolicDomainConstitution_Create_ProducesStableDigest()
    {
        var constitutionA = BuildConstitution();
        var constitutionB = BuildConstitution();

        Assert.Equal(constitutionA.Version, constitutionB.Version);
        Assert.Equal(constitutionA.Digest, constitutionB.Digest);
    }

    [Fact]
    public void SymbolicDomainConstitution_Create_NormalizesDomainsAndReservedSymbols()
    {
        var constitution = SymbolicDomainConstitution.Create(
            "symbolic-domain-constitution.v1",
            [
                new ReservedSymbolicDomain
                {
                    Key = "disciplinary.physics",
                    DisplayLabel = "Physics",
                    DomainClass = SymbolicDomainClass.DisciplinaryReserved,
                    Description = "Reserved physics notation space.",
                    ReservedSymbols = ["alpha", "alpha", "omega"],
                    AllowedBridgeDomainKeys = ["atlas.root.native", "atlas.root.native"],
                    AllowedConstructorRoles = [SymbolicConstructorRole.DisciplinaryOverlay, SymbolicConstructorRole.DisciplinaryOverlay],
                    AllowsMergedRenderForms = false
                },
                new ReservedSymbolicDomain
                {
                    Key = "grammar.operator",
                    DisplayLabel = "Grammar Operators",
                    DomainClass = SymbolicDomainClass.GrammarOperator,
                    Description = "Reserved grammatical operators.",
                    ReservedSymbols = ["pref.neg", "suf.aspect"],
                    AllowedBridgeDomainKeys = [],
                    AllowedConstructorRoles = [SymbolicConstructorRole.PrefixOperator, SymbolicConstructorRole.SuffixOperator],
                    AllowsMergedRenderForms = true
                }
            ],
            ["ctrl.refusal", "ctrl.refusal", "ctrl.ambiguous"]);

        Assert.Equal(["disciplinary.physics", "grammar.operator"], constitution.Domains.Select(domain => domain.Key).OrderBy(key => key, StringComparer.OrdinalIgnoreCase));
        var physicsDomain = constitution.Domains.Single(domain => domain.Key == "disciplinary.physics");
        Assert.Equal(["alpha", "omega"], physicsDomain.ReservedSymbols);
        Assert.Equal(["atlas.root.native"], physicsDomain.AllowedBridgeDomainKeys);
        Assert.Equal([SymbolicConstructorRole.DisciplinaryOverlay], physicsDomain.AllowedConstructorRoles);
        Assert.Equal(["ctrl.ambiguous", "ctrl.refusal"], constitution.GlobalReservedSymbols);
    }

    [Fact]
    public void KnownUnknownExtensionState_PreservesSeparateExtensionAndCollisionStates()
    {
        Assert.NotEqual(KnownUnknownExtensionState.ExtensionCandidate, KnownUnknownExtensionState.NeedsSpecification);
        Assert.NotEqual(KnownUnknownExtensionState.ExtensionCandidate, KnownUnknownExtensionState.ProhibitedCollision);
    }

    private static SymbolicDomainConstitution BuildConstitution()
    {
        return SymbolicDomainConstitution.Create(
            "symbolic-domain-constitution.v1",
            [
                new ReservedSymbolicDomain
                {
                    Key = "grammar.operator",
                    DisplayLabel = "Grammar Operators",
                    DomainClass = SymbolicDomainClass.GrammarOperator,
                    Description = "Cross-root grammar modifiers.",
                    ReservedSymbols = ["pref.neg", "suf.aspect"],
                    AllowedBridgeDomainKeys = [],
                    AllowedConstructorRoles = [SymbolicConstructorRole.PrefixOperator, SymbolicConstructorRole.SuffixOperator],
                    AllowsMergedRenderForms = true
                },
                new ReservedSymbolicDomain
                {
                    Key = "atlas.root.native",
                    DisplayLabel = "Root Native Core",
                    DomainClass = SymbolicDomainClass.RootNativeCore,
                    Description = "Canonical root-owned symbolic anchors.",
                    ReservedSymbols = ["root.eq", "root.var"],
                    AllowedBridgeDomainKeys = ["disciplinary.math"],
                    AllowedConstructorRoles = [SymbolicConstructorRole.RootCore],
                    AllowsMergedRenderForms = true
                },
                new ReservedSymbolicDomain
                {
                    Key = "disciplinary.math",
                    DisplayLabel = "Mathematics",
                    DomainClass = SymbolicDomainClass.DisciplinaryReserved,
                    Description = "Reserved mathematical symbol space.",
                    ReservedSymbols = ["forall", "sum"],
                    AllowedBridgeDomainKeys = ["atlas.root.native"],
                    AllowedConstructorRoles = [SymbolicConstructorRole.DisciplinaryOverlay],
                    AllowsMergedRenderForms = false
                }
            ],
            ["ctrl.refusal", "ctrl.unresolved"]);
    }
}
