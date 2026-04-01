using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryContractLayerContractsTests
{
    [Fact]
    public void LocalOperatorDefault_ConstructsAsLocalBound()
    {
        var profile = SanctuaryContractProfile.CreateLocalOperatorDefault();

        Assert.Equal(SanctuaryActorClass.LocalOperator, profile.ActorClass);
        Assert.Equal(SanctuaryDeploymentMode.LocalBound, profile.DeploymentMode);
        Assert.Equal(SanctuaryLocalizationMode.OperatorLocalOnly, profile.LocalizationMode);
        Assert.True(profile.FirstDownloadFetchLimitedToLocalBinding);
    }

    [Fact]
    public void LocalBindingDefault_DoesNotImplyResearchServiceScope()
    {
        var profile = SanctuaryContractProfile.CreateLocalOperatorDefault();

        Assert.Equal(SanctuaryResearchServiceScope.None, profile.ResearchServiceScope);
        Assert.DoesNotContain(SanctuaryContractLayer.ResearchServicesAddendum, profile.ActiveLayers);
    }

    [Fact]
    public void OrganizationManagedDeployment_RequiresExplicitOrganizationActorAndLayers()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new SanctuaryContractProfile(
                actorClass: SanctuaryActorClass.OrganizationOperator,
                deploymentMode: SanctuaryDeploymentMode.LocalBound,
                researchServiceScope: SanctuaryResearchServiceScope.None,
                localizationMode: SanctuaryLocalizationMode.OrganizationControlledLocality,
                activeLayers:
                [
                    SanctuaryContractLayer.LocalBindingLicense,
                    SanctuaryContractLayer.IpOutputTelemetrySchedule
                ],
                firstDownloadFetchLimitedToLocalBinding: true));

        Assert.Contains("organization-managed", ex.Message, StringComparison.OrdinalIgnoreCase);

        var validProfile = new SanctuaryContractProfile(
            actorClass: SanctuaryActorClass.OrganizationAdministrator,
            deploymentMode: SanctuaryDeploymentMode.OrganizationManaged,
            researchServiceScope: SanctuaryResearchServiceScope.None,
            localizationMode: SanctuaryLocalizationMode.OrganizationControlledLocality,
            activeLayers:
            [
                SanctuaryContractLayer.LocalBindingLicense,
                SanctuaryContractLayer.ProtectedDataAddendum,
                SanctuaryContractLayer.LocalizationResidencySchedule,
                SanctuaryContractLayer.IpOutputTelemetrySchedule
            ],
            firstDownloadFetchLimitedToLocalBinding: true);

        Assert.Equal(SanctuaryDeploymentMode.OrganizationManaged, validProfile.DeploymentMode);
    }

    [Fact]
    public void ProtectedDataProfiles_DistinguishOwnershipAndDataClasses()
    {
        SanctuaryProtectedDataProfile[] profiles =
        [
            new SanctuaryProtectedDataProfile(
                dataClass: SanctuaryProtectedDataClass.ProviderIp,
                ownershipSurface: SanctuaryOwnershipSurface.ProviderOwned,
                telemetryClass: SanctuaryTelemetryClass.EntitlementIntegrity,
                localOnlyByDefault: false,
                explicitRemoteActivationRequired: false,
                governingLayers:
                [
                    SanctuaryContractLayer.LocalBindingLicense,
                    SanctuaryContractLayer.IpOutputTelemetrySchedule
                ]),
            new SanctuaryProtectedDataProfile(
                dataClass: SanctuaryProtectedDataClass.CustomerOperatorData,
                ownershipSurface: SanctuaryOwnershipSurface.OperatorControlled,
                telemetryClass: SanctuaryTelemetryClass.None,
                localOnlyByDefault: true,
                explicitRemoteActivationRequired: true,
                governingLayers:
                [
                    SanctuaryContractLayer.LocalBindingLicense,
                    SanctuaryContractLayer.IpOutputTelemetrySchedule
                ]),
            new SanctuaryProtectedDataProfile(
                dataClass: SanctuaryProtectedDataClass.ProtectedData,
                ownershipSurface: SanctuaryOwnershipSurface.CustomerControlledProtected,
                telemetryClass: SanctuaryTelemetryClass.None,
                localOnlyByDefault: true,
                explicitRemoteActivationRequired: true,
                governingLayers:
                [
                    SanctuaryContractLayer.ProtectedDataAddendum,
                    SanctuaryContractLayer.LocalizationResidencySchedule
                ]),
            new SanctuaryProtectedDataProfile(
                dataClass: SanctuaryProtectedDataClass.OutputArtifact,
                ownershipSurface: SanctuaryOwnershipSurface.AssignedOutput,
                telemetryClass: SanctuaryTelemetryClass.None,
                localOnlyByDefault: true,
                explicitRemoteActivationRequired: true,
                governingLayers:
                [
                    SanctuaryContractLayer.IpOutputTelemetrySchedule
                ]),
            new SanctuaryProtectedDataProfile(
                dataClass: SanctuaryProtectedDataClass.OperationalMetadata,
                ownershipSurface: SanctuaryOwnershipSurface.ServiceOperational,
                telemetryClass: SanctuaryTelemetryClass.OperationalAudit,
                localOnlyByDefault: false,
                explicitRemoteActivationRequired: false,
                governingLayers:
                [
                    SanctuaryContractLayer.LocalBindingLicense,
                    SanctuaryContractLayer.IpOutputTelemetrySchedule
                ])
        ];

        Assert.Equal(5, profiles.Select(static item => item.DataClass).Distinct().Count());
        Assert.Equal(5, profiles.Select(static item => item.OwnershipSurface).Distinct().Count());
    }

    [Fact]
    public void FormationAtlas_PreservesBurdenForEveryCanonicalModule()
    {
        Assert.Equal(5, SanctuaryFormationPredicateAtlas.All.Count);

        Assert.All(
            SanctuaryFormationPredicateAtlas.All,
            definition =>
            {
                Assert.NotNull(definition.RoleNexus);
                Assert.NotEmpty(definition.AuthoritySurfaces);
                Assert.NotEmpty(definition.TrustInvariants);
                Assert.NotEmpty(definition.AllowedActionClasses);
                Assert.NotEmpty(definition.WitnessRequirements);
                Assert.NotEmpty(definition.PromotionRequirements);
                Assert.NotEmpty(definition.DowngradeConditions);
                Assert.NotNull(definition.GelCarryForwardEligibility);
            });
    }

    [Fact]
    public void FormationConsumers_MustLookupBurdenBearingDefinitions()
    {
        foreach (var predicate in Enum.GetValues<SanctuaryFormationPredicateKind>())
        {
            Assert.True(SanctuaryFormationPredicateAtlas.TryGet(predicate, out var definition));
            Assert.Equal(predicate, definition.Predicate);
            Assert.NotEmpty(definition.AuthoritySurfaces);
            Assert.NotEmpty(definition.TrustInvariants);
        }
    }

    [Fact]
    public void SanctuaryContracts_SerializeAndDeserializeStably()
    {
        var contractProfile = SanctuaryContractProfile.CreateLocalOperatorDefault();
        var serializedContract = JsonSerializer.Serialize(contractProfile);
        var deserializedContract = JsonSerializer.Deserialize<SanctuaryContractProfile>(serializedContract);

        Assert.NotNull(deserializedContract);
        Assert.Equal(contractProfile.ActorClass, deserializedContract!.ActorClass);
        Assert.Equal(contractProfile.DeploymentMode, deserializedContract.DeploymentMode);
        Assert.Equal(contractProfile.ResearchServiceScope, deserializedContract.ResearchServiceScope);
        Assert.Equal(contractProfile.LocalizationMode, deserializedContract.LocalizationMode);
        Assert.Equal(contractProfile.ActiveLayers, deserializedContract.ActiveLayers);
        Assert.Equal(
            contractProfile.FirstDownloadFetchLimitedToLocalBinding,
            deserializedContract.FirstDownloadFetchLimitedToLocalBinding);

        var definition = SanctuaryFormationPredicateAtlas.Get(SanctuaryFormationPredicateKind.CertifiedOperator);
        var serializedDefinition = JsonSerializer.Serialize(definition);
        var deserializedDefinition = JsonSerializer.Deserialize<SanctuaryFormationPredicateDefinition>(serializedDefinition);

        Assert.NotNull(deserializedDefinition);
        Assert.Equal(definition.Predicate, deserializedDefinition!.Predicate);
        Assert.Equal(definition.RoleNexus.Name, deserializedDefinition.RoleNexus.Name);
        Assert.Equal(definition.AllowedActionClasses, deserializedDefinition.AllowedActionClasses);
        Assert.Equal(definition.TrustInvariants.Count, deserializedDefinition.TrustInvariants.Count);
        Assert.Equal(
            definition.GelCarryForwardEligibility.CarryForwardClass,
            deserializedDefinition.GelCarryForwardEligibility.CarryForwardClass);
    }

    [Fact]
    public void FormationAtlas_LookupReturnsAllFiveCanonicalModules()
    {
        var resolved = Enum.GetValues<SanctuaryFormationPredicateKind>()
            .Select(SanctuaryFormationPredicateAtlas.Get)
            .Select(static item => item.Predicate)
            .ToArray();

        Assert.Equal(
            [
                SanctuaryFormationPredicateKind.Learner,
                SanctuaryFormationPredicateKind.Trainee,
                SanctuaryFormationPredicateKind.CertifiedOperator,
                SanctuaryFormationPredicateKind.TradeBearingPractitioner,
                SanctuaryFormationPredicateKind.CareerContinuityHolder
            ],
            resolved);
    }
}
