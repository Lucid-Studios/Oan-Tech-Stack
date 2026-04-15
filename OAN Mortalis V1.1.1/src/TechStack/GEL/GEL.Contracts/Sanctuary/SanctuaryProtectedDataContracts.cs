namespace GEL.Contracts.Sanctuary;

public enum SanctuaryProtectedDataClass
{
    ProviderIp = 0,
    CustomerOperatorData = 1,
    OrganizationControlledData = 2,
    ProtectedData = 3,
    OutputArtifact = 4,
    OperationalMetadata = 5
}

public enum SanctuaryOwnershipSurface
{
    ProviderOwned = 0,
    OperatorControlled = 1,
    OrganizationControlled = 2,
    CustomerControlledProtected = 3,
    AssignedOutput = 4,
    ServiceOperational = 5
}

public enum SanctuaryTelemetryClass
{
    None = 0,
    EntitlementIntegrity = 1,
    OperationalAudit = 2,
    BoundedSupport = 3,
    ExplicitResearch = 4
}

public sealed record SanctuaryProtectedDataProfile
{
    public SanctuaryProtectedDataProfile(
        SanctuaryProtectedDataClass dataClass,
        SanctuaryOwnershipSurface ownershipSurface,
        SanctuaryTelemetryClass telemetryClass,
        bool localOnlyByDefault,
        bool explicitRemoteActivationRequired,
        IReadOnlyList<SanctuaryContractLayer> governingLayers)
    {
        DataClass = dataClass;
        OwnershipSurface = ownershipSurface;
        TelemetryClass = telemetryClass;
        LocalOnlyByDefault = localOnlyByDefault;
        ExplicitRemoteActivationRequired = explicitRemoteActivationRequired;
        GoverningLayers = SanctuaryContractGuard.RequiredDistinctList(governingLayers, nameof(governingLayers));

        Validate();
    }

    public SanctuaryProtectedDataClass DataClass { get; }

    public SanctuaryOwnershipSurface OwnershipSurface { get; }

    public SanctuaryTelemetryClass TelemetryClass { get; }

    public bool LocalOnlyByDefault { get; }

    public bool ExplicitRemoteActivationRequired { get; }

    public IReadOnlyList<SanctuaryContractLayer> GoverningLayers { get; }

    private void Validate()
    {
        switch (DataClass)
        {
            case SanctuaryProtectedDataClass.ProviderIp when OwnershipSurface is not SanctuaryOwnershipSurface.ProviderOwned:
                throw new InvalidOperationException(
                    "Provider IP must remain on the provider-owned surface.");

            case SanctuaryProtectedDataClass.CustomerOperatorData when OwnershipSurface is not SanctuaryOwnershipSurface.OperatorControlled:
                throw new InvalidOperationException(
                    "Customer or operator data must remain on the operator-controlled surface.");

            case SanctuaryProtectedDataClass.OrganizationControlledData when OwnershipSurface is not SanctuaryOwnershipSurface.OrganizationControlled:
                throw new InvalidOperationException(
                    "Organization-controlled data must remain on the organization-controlled surface.");

            case SanctuaryProtectedDataClass.ProtectedData when OwnershipSurface is not SanctuaryOwnershipSurface.CustomerControlledProtected:
                throw new InvalidOperationException(
                    "Protected data must remain on a customer-controlled protected surface.");

            case SanctuaryProtectedDataClass.OutputArtifact when OwnershipSurface is not SanctuaryOwnershipSurface.AssignedOutput:
                throw new InvalidOperationException(
                    "Output artifacts must use the assigned-output ownership surface.");

            case SanctuaryProtectedDataClass.OperationalMetadata when OwnershipSurface is not SanctuaryOwnershipSurface.ServiceOperational:
                throw new InvalidOperationException(
                    "Operational metadata must use the service-operational ownership surface.");
        }

        if (LocalOnlyByDefault && TelemetryClass is SanctuaryTelemetryClass.ExplicitResearch)
        {
            throw new InvalidOperationException(
                "Local-only-by-default data may not default to explicit research telemetry.");
        }

        if (ExplicitRemoteActivationRequired &&
            !LocalOnlyByDefault &&
            DataClass is SanctuaryProtectedDataClass.ProviderIp or SanctuaryProtectedDataClass.OperationalMetadata)
        {
            throw new InvalidOperationException(
                "Provider IP and bounded operational metadata should not require explicit remote activation.");
        }
    }
}

public sealed record SanctuaryOwnershipProfile
{
    public SanctuaryOwnershipProfile(
        SanctuaryOwnershipSurface ownershipSurface,
        IReadOnlyList<SanctuaryProtectedDataClass> coveredDataClasses,
        bool providerMayProcessForServiceDelivery,
        bool providerMayUseForModelImprovement,
        bool customerRetainsPrimaryControl,
        bool explicitAssignmentRequiredForOutputs,
        IReadOnlyList<SanctuaryContractLayer> governingLayers)
    {
        OwnershipSurface = ownershipSurface;
        CoveredDataClasses = SanctuaryContractGuard.RequiredDistinctList(coveredDataClasses, nameof(coveredDataClasses));
        ProviderMayProcessForServiceDelivery = providerMayProcessForServiceDelivery;
        ProviderMayUseForModelImprovement = providerMayUseForModelImprovement;
        CustomerRetainsPrimaryControl = customerRetainsPrimaryControl;
        ExplicitAssignmentRequiredForOutputs = explicitAssignmentRequiredForOutputs;
        GoverningLayers = SanctuaryContractGuard.RequiredDistinctList(governingLayers, nameof(governingLayers));

        Validate();
    }

    public SanctuaryOwnershipSurface OwnershipSurface { get; }

    public IReadOnlyList<SanctuaryProtectedDataClass> CoveredDataClasses { get; }

    public bool ProviderMayProcessForServiceDelivery { get; }

    public bool ProviderMayUseForModelImprovement { get; }

    public bool CustomerRetainsPrimaryControl { get; }

    public bool ExplicitAssignmentRequiredForOutputs { get; }

    public IReadOnlyList<SanctuaryContractLayer> GoverningLayers { get; }

    private void Validate()
    {
        if (ProviderMayUseForModelImprovement &&
            OwnershipSurface is not SanctuaryOwnershipSurface.ProviderOwned)
        {
            throw new InvalidOperationException(
                "Model-improvement rights may not be granted outside the provider-owned surface in the baseline contract model.");
        }

        if (CustomerRetainsPrimaryControl &&
            OwnershipSurface is SanctuaryOwnershipSurface.ProviderOwned or SanctuaryOwnershipSurface.ServiceOperational)
        {
            throw new InvalidOperationException(
                "Provider-owned and service-operational surfaces may not claim customer primary control.");
        }

        if (ExplicitAssignmentRequiredForOutputs &&
            OwnershipSurface is not SanctuaryOwnershipSurface.AssignedOutput)
        {
            throw new InvalidOperationException(
                "Explicit output assignment applies only to the assigned-output surface.");
        }
    }
}

public static class SanctuaryProtectedDataAtlas
{
    public static SanctuaryProtectedDataProfile LocalLegalOrientationEvidence { get; } =
        new(
            dataClass: SanctuaryProtectedDataClass.OrganizationControlledData,
            ownershipSurface: SanctuaryOwnershipSurface.OrganizationControlled,
            telemetryClass: SanctuaryTelemetryClass.OperationalAudit,
            localOnlyByDefault: true,
            explicitRemoteActivationRequired: true,
            governingLayers:
            [
                SanctuaryContractLayer.LocalBindingLicense,
                SanctuaryContractLayer.ProtectedDataAddendum,
                SanctuaryContractLayer.LocalizationResidencySchedule
            ]);
}
