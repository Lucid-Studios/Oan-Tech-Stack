using System.Text.Json;
using AgentiCore.Observation;
using CradleTek.Memory.Services;
using EngramGovernance.Services;
using GEL.Contracts;
using GEL.Models;
using Oan.Common;
using Oan.Cradle;
using SLI.Ingestion;

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
    public async Task CorporateGoverned_Profile_MasksIngress_And_AdmitsBoundedFormation()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);
        var profile = LoadExampleProfile();
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = LoadOverlayRoots();

        var result = await harness.RunAsync(profile, atlas, overlayRoots);

        Assert.Equal(BootClass.CorporateGoverned, result.BootClassificationResult.BootClass);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal(
            [PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation],
            result.RequestedRevealModes);
        Assert.Equal(
            [PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation],
            result.GrantedRevealModes);
        Assert.Empty(result.BlockedRevealModes);
        Assert.Equal("HumanPrincipal_A", result.MaskedHandles[ProtectedIntakeKind.HumanProtectedIntake]);
        Assert.Equal("CorporatePrincipal_A", result.MaskedHandles[ProtectedIntakeKind.CorporateProtectedIntake]);
        Assert.All(result.ProtectedIntakeResults, intake =>
        {
            Assert.Equal(FirstBootGovernanceDecision.Allow, intake.Classification.Decision);
            Assert.False(intake.Classification.RawFieldExposureAllowed);
        });

        var membrane = Assert.Single(result.MembraneDecisions);
        Assert.Equal(CrypticAdmissionDecision.Admit, membrane.Decision);
        Assert.True(membrane.SubmissionEligible);

        var closure = Assert.Single(result.ClosureOutcomes);
        Assert.Equal(AgentiFormationClosureState.Closed, closure.ClosureState);

        var stages = result.ObservationBatch.Observations.Select(observation => observation.Stage).ToArray();
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
            stages);

        Assert.Equal(result.ObservationBatch.Observations.Count, observer.Snapshot().Count);
    }

    [Fact]
    public async Task PersonalSolitary_Profile_RemainsSingleOperator_And_StillObservesFormation()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.None]);
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = LoadOverlayRoots();

        var result = await harness.RunAsync(profile, atlas, overlayRoots);

        Assert.Equal(BootClass.PersonalSolitary, result.BootClassificationResult.BootClass);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal(SwarmEligibility.Denied, result.BootClassificationResult.SwarmEligibility);
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
    public async Task PersonalSwarmAttempt_IsQuarantined_And_DoesNotReachFormation()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 2,
            requestedRevealModes: [PrimeRevealMode.None]);
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = LoadOverlayRoots();

        var result = await harness.RunAsync(profile, atlas, overlayRoots);

        Assert.Equal(FirstBootGovernanceDecision.Quarantine, result.BootClassificationResult.Decision);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations,
            observation => observation.Stage is AgentiFormationObservationStage.CrypticAdmission or AgentiFormationObservationStage.PrimeClosure);
    }

    [Fact]
    public async Task UnauthorizedRevealEscalation_IsBlocked_And_DoesNotLeakRawFields()
    {
        var harness = CreateHarness();
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
        var atlas = await LoadCanonicalAtlasAsync();
        var overlayRoots = LoadOverlayRoots();

        var result = await harness.RunAsync(profile, atlas, overlayRoots);

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
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations.SelectMany(observation => observation.ObservationTags),
            tag => tag.Contains(profile.HumanPrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporatePrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.HumanCredentialId, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporateRegistryId, StringComparison.Ordinal));
    }

    private static HybridProtectedIngressHarness CreateHarness(IAgentiFormationObserver? observer = null)
    {
        return new HybridProtectedIngressHarness(
            new DefaultFirstBootGovernancePolicy(),
            new EngramClosureValidator(),
            new CrypticAdmissionMembrane(),
            observer);
    }

    private static async Task<RootAtlas> LoadCanonicalAtlasAsync()
    {
        var cleaver = new RootAtlasOntologicalCleaver();
        var result = await cleaver.CleaveAsync("observe");
        return result.CanonicalRootAtlas;
    }

    private static IReadOnlyList<NarrativeOverlayRoot> LoadOverlayRoots()
    {
        var fixture = LoadTranslationFixture();
        return fixture.OverlayRoots.Select(root => new NarrativeOverlayRoot
        {
            Lemma = root.Lemma,
            SymbolicCore = root.SymbolicCore,
            OperatorCompatibility = root.OperatorCompatibility,
            ReservedDomainStatus = root.ReservedDomainStatus,
            DisciplinaryReservations = root.DisciplinaryReservations,
            VariantExamples = root.VariantExamples
        }).ToArray();
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
            ApprovedRevealPurposes = (approvedRevealPurposes ?? baseProfile.ApprovedRevealPurposes).ToArray(),
            FormationSentence = baseProfile.FormationSentence
        };
    }

    private static TranslationFixtureDefinition LoadTranslationFixture()
    {
        var path = ResolveRepoFile(
            "OAN Mortalis V1.0",
            "tests",
            "Oan.Sli.Tests",
            "fixtures",
            "FirstNarrativeTranslationFixture.json");

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TranslationFixtureDefinition>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Narrative translation fixture could not be parsed.");
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
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class TranslationFixtureDefinition
    {
        public required IReadOnlyList<FixtureOverlayRoot> OverlayRoots { get; init; }
    }

    private sealed class FixtureOverlayRoot
    {
        public required string Lemma { get; init; }
        public required string SymbolicCore { get; init; }
        public required string OperatorCompatibility { get; init; }
        public required string ReservedDomainStatus { get; init; }
        public required IReadOnlyList<string> DisciplinaryReservations { get; init; }
        public required IReadOnlyList<string> VariantExamples { get; init; }
    }
}
