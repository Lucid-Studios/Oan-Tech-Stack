namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class FirstBootGovernanceContractsTests
{
    private static readonly DefaultFirstBootGovernancePolicy Policy = new();

    [Fact]
    public void PersonalSolitary_DefaultsToNoExpansionRights()
    {
        var profile = Policy.EvaluateBootProfile(
            BootClass.PersonalSolitary,
            BootActivationState.Classified,
            requestedExpansionCount: 1);

        Assert.Equal(FirstBootGovernanceDecision.Allow, profile.Decision);
        Assert.Equal(ExpansionRights.None, profile.ExpansionRights);
        Assert.Equal(SwarmEligibility.Denied, profile.SwarmEligibility);
    }

    [Fact]
    public void CorporateGoverned_RemainsNonExpandableUntilBondedConfirmed()
    {
        var profile = Policy.EvaluateBootProfile(
            BootClass.CorporateGoverned,
            BootActivationState.BondedConfirmed,
            requestedExpansionCount: 1,
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: true);

        Assert.Equal(FirstBootGovernanceDecision.Allow, profile.Decision);
        Assert.Equal(ExpansionRights.None, profile.ExpansionRights);
        Assert.Equal(SwarmEligibility.Denied, profile.SwarmEligibility);
    }

    [Fact]
    public void ExpansionEligible_IsRejectedBeforeCrossWitnessAndBondedConfirmation()
    {
        var profile = Policy.EvaluateBootProfile(
            BootClass.CorporateGoverned,
            BootActivationState.ExpansionEligible,
            requestedExpansionCount: 2,
            triadicCrossWitnessComplete: false,
            bondedConfirmationComplete: false);

        Assert.Equal(FirstBootGovernanceDecision.Reject, profile.Decision);
        Assert.Equal("expansion-activation-prerequisites-missing", profile.ReasonCode);
        Assert.Equal(ExpansionRights.None, profile.ExpansionRights);
    }

    [Fact]
    public void CorporateGoverned_GainsInternalGovernedOnlyAfterFullPrerequisites()
    {
        var profile = Policy.EvaluateBootProfile(
            BootClass.CorporateGoverned,
            BootActivationState.ExpansionEligible,
            requestedExpansionCount: 2,
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: true);

        Assert.Equal(FirstBootGovernanceDecision.Allow, profile.Decision);
        Assert.Equal(ExpansionRights.InternalGovernedOnly, profile.ExpansionRights);
        Assert.Equal(SwarmEligibility.AllowedAfterBondedConfirmation, profile.SwarmEligibility);
    }

    [Fact]
    public void PersonalSolitary_SwarmRequestIsQuarantined()
    {
        var profile = Policy.EvaluateBootProfile(
            BootClass.PersonalSolitary,
            BootActivationState.Classified,
            requestedExpansionCount: 2);

        Assert.Equal(FirstBootGovernanceDecision.Quarantine, profile.Decision);
        Assert.Equal("personal-solitary-swarm-denied", profile.ReasonCode);
        Assert.Equal(ExpansionRights.None, profile.ExpansionRights);
    }

    [Theory]
    [InlineData(ProtectedIntakeKind.HumanProtectedIntake)]
    [InlineData(ProtectedIntakeKind.CorporateProtectedIntake)]
    public void ProtectedIntake_DefaultsToMaskedCrypticView(ProtectedIntakeKind kind)
    {
        var result = Policy.ClassifyProtectedIntake(
            kind,
            "ProtectedPrincipal_A",
            PrimeRevealMode.None);

        Assert.Equal(FirstBootGovernanceDecision.Allow, result.Decision);
        Assert.Equal(PrimeRevealMode.None, result.EffectiveRevealMode);
        Assert.False(result.RawFieldExposureAllowed);
        Assert.Equal("cryptic-default", result.MaskedView.StructuralLabels["masking_state"]);
    }

    [Theory]
    [InlineData(PrimeRevealMode.MaskedSummary, "prime-masked-summary-allowed")]
    [InlineData(PrimeRevealMode.StructuralValidation, "prime-structural-validation-allowed")]
    public void NarrowPrimeRevealModesAreAllowedWithoutRawFieldExposure(
        PrimeRevealMode revealMode,
        string expectedReason)
    {
        var result = Policy.ClassifyProtectedIntake(
            ProtectedIntakeKind.CorporateProtectedIntake,
            "CorporatePrincipal_A",
            revealMode);

        Assert.Equal(FirstBootGovernanceDecision.Allow, result.Decision);
        Assert.Equal(revealMode, result.EffectiveRevealMode);
        Assert.False(result.RawFieldExposureAllowed);
        Assert.Equal(expectedReason, result.ReasonCode);
    }

    [Fact]
    public void AuthorizedFieldRevealRequiresBondedAuthority()
    {
        var result = Policy.ClassifyProtectedIntake(
            ProtectedIntakeKind.HumanProtectedIntake,
            "HumanPrincipal_A",
            PrimeRevealMode.AuthorizedFieldReveal);

        Assert.Equal(FirstBootGovernanceDecision.Quarantine, result.Decision);
        Assert.True(result.RequiresBondedAuthority);
        Assert.False(result.RawFieldExposureAllowed);
    }

    [Fact]
    public void AuthorizedFieldRevealAllowsRawFieldExposureWhenBonded()
    {
        var context = new BondedAuthorityContext(
            "Authority_A",
            "BondedOperator",
            BondedConfirmed: true,
            ApprovedRevealPurposes: ["founding-validation"]);

        var result = Policy.ClassifyProtectedIntake(
            ProtectedIntakeKind.CorporateProtectedIntake,
            "CorporatePrincipal_A",
            PrimeRevealMode.AuthorizedFieldReveal,
            context);

        Assert.Equal(FirstBootGovernanceDecision.Allow, result.Decision);
        Assert.Equal(PrimeRevealMode.AuthorizedFieldReveal, result.EffectiveRevealMode);
        Assert.True(result.RawFieldExposureAllowed);
    }

    [Fact]
    public void Steward_MustBeEligibleBeforeFather()
    {
        var result = Policy.EvaluateFormationEligibility(
            new InternalGoverningCmeFormationRequest(
                BootClass.CorporateGoverned,
                BootActivationState.GovernanceForming,
                InternalGoverningCmeOffice.Father,
                AlreadyFormedOffices: [],
                TriadicCrossWitnessComplete: false,
                BondedConfirmationComplete: false));

        Assert.Equal(FirstBootGovernanceDecision.Reject, result.Decision);
        Assert.Equal("governing-office-order-violation", result.ReasonCode);
    }

    [Fact]
    public void Father_MustBeEligibleBeforeMother()
    {
        var result = Policy.EvaluateFormationEligibility(
            new InternalGoverningCmeFormationRequest(
                BootClass.CorporateGoverned,
                BootActivationState.GovernanceForming,
                InternalGoverningCmeOffice.Mother,
                AlreadyFormedOffices: [InternalGoverningCmeOffice.Steward],
                TriadicCrossWitnessComplete: false,
                BondedConfirmationComplete: false));

        Assert.Equal(FirstBootGovernanceDecision.Reject, result.Decision);
        Assert.Equal("governing-office-order-violation", result.ReasonCode);
    }

    [Fact]
    public void GoverningOffices_ExposeOnlyTheirAllowedVisibilityScopes()
    {
        var steward = Policy.GetOfficeDefinition(InternalGoverningCmeOffice.Steward);
        var father = Policy.GetOfficeDefinition(InternalGoverningCmeOffice.Father);
        var mother = Policy.GetOfficeDefinition(InternalGoverningCmeOffice.Mother);

        Assert.Equal(
            [
                GoverningOfficeVisibilityScope.CustodyClass,
                GoverningOfficeVisibilityScope.ProvenanceClass,
                GoverningOfficeVisibilityScope.RevealEligibilityState
            ],
            steward.VisibilityScopes);

        Assert.Equal(
            [
                GoverningOfficeVisibilityScope.StructuralIdentityRelations,
                GoverningOfficeVisibilityScope.GovernanceObligations
            ],
            father.VisibilityScopes);

        Assert.Equal(
            [
                GoverningOfficeVisibilityScope.ContinuityCareObligations,
                GoverningOfficeVisibilityScope.PrimeRevealPosture
            ],
            mother.VisibilityScopes);
    }

    [Fact]
    public void ProjectGovernanceLayer_Preformalizes_RoleBoundEces_ForFirstBoot()
    {
        var layer = Policy.ProjectGovernanceLayer(
            BootClass.CorporateGoverned,
            BootActivationState.TriadicActive,
            requestedExpansionCount: 1,
            formedOffices:
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: false);

        Assert.Equal(FirstBootGovernanceLayerState.RoleBoundEceReady, layer.State);
        Assert.True(layer.WitnessOnly);
        Assert.True(layer.RoleBoundEcesReady);
        Assert.False(layer.SubordinateCmeAuthorizationAllowed);
        Assert.Equal(
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            layer.FormedOffices);
        Assert.All(layer.RoleBoundEces, ece =>
        {
            Assert.Equal(RoleBoundEceState.RoleBoundTestingReady, ece.State);
            Assert.True(ece.WitnessOnly);
            Assert.False(ece.PrimeRevealWideningAllowed);
            Assert.False(ece.ExpansionAuthorizationAllowed);
        });
    }

    [Fact]
    public void ProjectGovernanceLayer_WithNoFormedOffices_LeavesEcesUnprovisioned()
    {
        var layer = Policy.ProjectGovernanceLayer(
            BootClass.CorporateGoverned,
            BootActivationState.Classified,
            requestedExpansionCount: 1,
            formedOffices: [],
            triadicCrossWitnessComplete: false,
            bondedConfirmationComplete: false);

        Assert.Equal(FirstBootGovernanceLayerState.Preformalized, layer.State);
        Assert.False(layer.RoleBoundEcesReady);
        Assert.Empty(layer.FormedOffices);
        Assert.All(layer.RoleBoundEces, ece => Assert.Equal(RoleBoundEceState.NotProvisioned, ece.State));
    }

    [Fact]
    public void ProjectFirstBootEnvelope_RoleBoundLayer_ProjectsActualized()
    {
        var layer = Policy.ProjectGovernanceLayer(
            BootClass.CorporateGoverned,
            BootActivationState.TriadicActive,
            requestedExpansionCount: 1,
            formedOffices:
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: false);

        var envelope = SliJurisdictionContracts.ProjectFirstBootEnvelope(layer);

        Assert.Equal(SliJurisdictionSurfaceClass.Actualized, envelope.SurfaceClass);
        Assert.True(envelope.WitnessOnly);
        Assert.False(envelope.BondRealizationClaimed);
        Assert.Equal(SliJurisdictionAuditDepth.Standard, envelope.AuditDepth);
        Assert.Equal(SliJurisdictionOversightRequirement.StewardReview, envelope.OversightRequirement);
        Assert.Equal(SliJurisdictionRetentionClass.GovernanceEventOnly, envelope.RetentionClass);
        Assert.Equal(PrimeRevealMode.StructuralValidation, envelope.RevealModeCeiling);
        Assert.Equal(SliJurisdictionContracts.ReasonActualizedFirstBootFormed, envelope.ReasonCode);
    }

    [Fact]
    public void ProjectFirstBootEnvelope_PreformalizedLayer_RemainsActualizedButRestrictive()
    {
        var layer = Policy.ProjectGovernanceLayer(
            BootClass.CorporateGoverned,
            BootActivationState.Classified,
            requestedExpansionCount: 1,
            formedOffices: [],
            triadicCrossWitnessComplete: false,
            bondedConfirmationComplete: false);

        var envelope = SliJurisdictionContracts.ProjectFirstBootEnvelope(layer);

        Assert.Equal(SliJurisdictionSurfaceClass.Actualized, envelope.SurfaceClass);
        Assert.Equal(SliJurisdictionContracts.ReasonEnvelopePreformalized, envelope.ReasonCode);
        Assert.True(envelope.WitnessOnly);
        Assert.False(envelope.SubordinateCmeAuthorizationAllowed);
    }
}
