using System.Text.Json;
using AgentiCore.Observation;
using EngramGovernance.Services;
using GEL.Models;
using Oan.Common;
using Oan.Cradle;

namespace Oan.Runtime.IntegrationTests;

public sealed class HybridProtectedIngressHarnessTests
{
    [Fact]
    public void ExampleProfile_UsesOnlyAbstractTrackedValues()
    {
        var profile = LoadExampleProfile();

        Assert.Equal("HumanPrincipal_A", profile.HumanPrincipalName);
        Assert.Equal("CorporatePrincipal_A", profile.CorporatePrincipalName);
        Assert.Equal("DirectorOfOperations", profile.AuthorityRelationship);
        Assert.StartsWith("HUM-TEST-", profile.HumanCredentialId, StringComparison.Ordinal);
        Assert.StartsWith("CORP-TEST-", profile.CorporateRegistryId, StringComparison.Ordinal);
        Assert.StartsWith("AUTH-TEST-", profile.AuthorityToken, StringComparison.Ordinal);
        Assert.StartsWith("ADDR-", profile.AddressHandle, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalProfilePath_RemainsIgnored()
    {
        var ignoreFile = File.ReadAllText(ResolveRepoFile(".gitignore"));

        Assert.Contains("OAN Mortalis V1.0/.local/", ignoreFile, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorporateGoverned_Profile_CompilesStableProposition_And_Closes()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);
        var profile = LoadExampleProfile();

        var result = await harness.RunAsync(profile);

        Assert.Equal(BootClass.CorporateGoverned, result.BootClassificationResult.BootClass);
        Assert.Equal(BootActivationState.Classified, result.BootClassificationResult.ActivationState);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal([PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation], result.RequestedRevealModes);
        Assert.Equal([PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation], result.GrantedRevealModes);
        Assert.Empty(result.BlockedRevealModes);
        Assert.Equal(PropositionalCompileGrade.Stable, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Stable, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Equal("HumanPrincipal_A", result.MaskedHandles[ProtectedIntakeKind.HumanProtectedIntake]);
        Assert.Equal("CorporatePrincipal_A", result.MaskedHandles[ProtectedIntakeKind.CorporateProtectedIntake]);
        Assert.Equal("HumanPrincipal_A", result.OraclePropositionAssessment.Candidate.Subject.SymbolicHandle);
        Assert.Equal("CorporatePrincipal_A", result.OraclePropositionAssessment.Candidate.Object.SymbolicHandle);
        Assert.Equal("authority-relationship", result.OraclePropositionAssessment.Candidate.PredicateRoot);

        var membrane = Assert.Single(result.MembraneDecisions);
        Assert.Equal(CrypticAdmissionDecision.Admit, membrane.Decision);
        Assert.True(membrane.SubmissionEligible);

        var closure = Assert.Single(result.ClosureOutcomes);
        Assert.Equal(AgentiFormationClosureState.Closed, closure.ClosureState);

        Assert.Equal(result.ObservationBatch.Observations.Count, observer.Snapshot().Count);
        Assert.Equal(
            [
                AgentiFormationObservationStage.BootClassification,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.TriadicCrossWitness,
                AgentiFormationObservationStage.CrypticAdmission,
                AgentiFormationObservationStage.PrimeClosure
            ],
            result.ObservationBatch.Observations.Select(observation => observation.Stage).ToArray());
    }

    [Fact]
    public async Task PersonalSolitary_Profile_RemainsStableButNonExpandable()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.None]);

        var result = await harness.RunAsync(profile);

        Assert.Equal(BootClass.PersonalSolitary, result.BootClassificationResult.BootClass);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal(SwarmEligibility.Denied, result.BootClassificationResult.SwarmEligibility);
        Assert.Equal(PropositionalCompileGrade.Stable, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Stable, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Equal([PrimeRevealMode.None], result.GrantedRevealModes);
        Assert.Empty(result.BlockedRevealModes);
        Assert.Single(result.MembraneDecisions);
        Assert.Single(result.ClosureOutcomes);
        Assert.Contains(
            result.ObservationBatch.Observations,
            observation => observation.Stage == AgentiFormationObservationStage.BootClassification &&
                           observation.ExpansionRights == ExpansionRights.None);
    }

    [Fact]
    public async Task PersonalSwarmAttempt_IsRejectedBeforeClosure()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 2,
            requestedRevealModes: [PrimeRevealMode.None]);

        var result = await harness.RunAsync(profile);

        Assert.Equal(FirstBootGovernanceDecision.Quarantine, result.BootClassificationResult.Decision);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Contains("topology.personal-swarm.denied", result.OraclePropositionAssessment.ReasonCodes);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations,
            observation => observation.Stage is AgentiFormationObservationStage.CrypticAdmission or AgentiFormationObservationStage.PrimeClosure);
    }

    [Fact]
    public async Task UnauthorizedRevealEscalation_IsRejected_And_DoesNotLeakRawFields()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.AuthorizedFieldReveal],
            bondedAuthorityConfirmed: false,
            approvedRevealPurposes: [],
            humanPrincipalName: "Manual Rehearsal Human",
            corporatePrincipalName: "Manual Rehearsal Corporate");

        var result = await harness.RunAsync(profile);

        Assert.Empty(result.GrantedRevealModes);
        Assert.Equal([PrimeRevealMode.AuthorizedFieldReveal], result.BlockedRevealModes);
        Assert.All(
            result.ProtectedIntakeResults,
            intake =>
            {
                Assert.Equal(FirstBootGovernanceDecision.Quarantine, intake.Classification.Decision);
                Assert.False(intake.Classification.RawFieldExposureAllowed);
                Assert.Equal(PrimeRevealMode.None, intake.Classification.EffectiveRevealMode);
            });
        Assert.Equal(PropositionalCompileGrade.Rejected, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Contains("reveal.authorized-field.denied", result.OraclePropositionAssessment.ReasonCodes);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations.SelectMany(observation => observation.ObservationTags),
            tag => tag.Contains(profile.HumanPrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporatePrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.HumanCredentialId, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporateRegistryId, StringComparison.Ordinal));
        Assert.Equal(result.ObservationBatch.Observations.Count, observer.Snapshot().Count);
    }

    private static HybridProtectedIngressHarness CreateHarness(IAgentiFormationObserver? observer = null)
    {
        return new HybridProtectedIngressHarness(
            new DefaultFirstBootGovernancePolicy(),
            new EngramClosureValidator(),
            new CrypticAdmissionMembrane(),
            formationObserver: observer);
    }

    private static HybridProtectedIngressProfile LoadExampleProfile()
    {
        var path = ResolveRepoFile(
            "OAN Mortalis V1.0",
            "docs",
            "runtime",
            "hybrid_protected_ingress_profile.example.json");

        return HybridProtectedIngressProfile.LoadFromJson(path);
    }

    private static HybridProtectedIngressProfile CloneProfile(
        HybridProtectedIngressProfile baseProfile,
        BootClass bootClass,
        int requestedExpansionCount,
        IReadOnlyList<PrimeRevealMode> requestedRevealModes,
        bool? bondedAuthorityConfirmed = null,
        IReadOnlyList<string>? approvedRevealPurposes = null,
        string? humanPrincipalName = null,
        string? corporatePrincipalName = null)
    {
        return new HybridProtectedIngressProfile
        {
            HumanPrincipalName = humanPrincipalName ?? baseProfile.HumanPrincipalName,
            CorporatePrincipalName = corporatePrincipalName ?? baseProfile.CorporatePrincipalName,
            AuthorityRelationship = baseProfile.AuthorityRelationship,
            HumanCredentialId = baseProfile.HumanCredentialId,
            CorporateRegistryId = baseProfile.CorporateRegistryId,
            AuthorityToken = baseProfile.AuthorityToken,
            AddressHandle = baseProfile.AddressHandle,
            RequestedBootClass = bootClass,
            RequestedExpansionCount = requestedExpansionCount,
            RequestedRevealModes = requestedRevealModes.ToArray(),
            BondedAuthorityConfirmed = bondedAuthorityConfirmed ?? baseProfile.BondedAuthorityConfirmed,
            ApprovedRevealPurposes = (approvedRevealPurposes ?? baseProfile.ApprovedRevealPurposes).ToArray()
        };
    }

    private static string ResolveRepoFile(params string[] parts)
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                var expected = Path.Combine(new[] { current.FullName }.Concat(parts).ToArray());
                if (File.Exists(expected))
                {
                    return expected;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException($"Unable to locate {Path.Combine(parts)} from the current test context.");
    }
}
