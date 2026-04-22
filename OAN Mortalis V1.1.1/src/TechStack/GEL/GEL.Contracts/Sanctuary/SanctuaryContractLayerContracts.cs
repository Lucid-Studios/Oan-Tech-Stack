namespace GEL.Contracts.Sanctuary;

public enum SanctuaryContractLayer
{
    LocalBindingLicense = 0,
    ResearchServicesAddendum = 1,
    ProtectedDataAddendum = 2,
    LocalizationResidencySchedule = 3,
    IpOutputTelemetrySchedule = 4
}

public enum SanctuaryDeploymentMode
{
    LocalBound = 0,
    VendorVisibleSupport = 1,
    VendorVisibleResearch = 2,
    OrganizationManaged = 3,
    RegulatedOrganizationManaged = 4
}

public enum SanctuaryActorClass
{
    LocalOperator = 0,
    OrganizationOperator = 1,
    OrganizationAdministrator = 2,
    RegulatedOrganizationAdministrator = 3
}

public enum SanctuaryResearchServiceScope
{
    None = 0,
    RemoteSupport = 1,
    RemoteResearch = 2,
    RemoteSupportAndResearch = 3
}

public enum SanctuaryLocalizationMode
{
    OperatorLocalOnly = 0,
    OrganizationControlledLocality = 1,
    ProviderVisibleRegional = 2,
    CrossBorderControlled = 3
}

public sealed record SanctuaryContractProfile
{
    public SanctuaryContractProfile(
        SanctuaryActorClass actorClass,
        SanctuaryDeploymentMode deploymentMode,
        SanctuaryResearchServiceScope researchServiceScope,
        SanctuaryLocalizationMode localizationMode,
        IReadOnlyList<SanctuaryContractLayer> activeLayers,
        bool firstDownloadFetchLimitedToLocalBinding)
    {
        ActorClass = actorClass;
        DeploymentMode = deploymentMode;
        ResearchServiceScope = researchServiceScope;
        LocalizationMode = localizationMode;
        ActiveLayers = SanctuaryContractGuard.RequiredDistinctList(activeLayers, nameof(activeLayers));
        FirstDownloadFetchLimitedToLocalBinding = firstDownloadFetchLimitedToLocalBinding;

        Validate();
    }

    public SanctuaryActorClass ActorClass { get; }

    public SanctuaryDeploymentMode DeploymentMode { get; }

    public SanctuaryResearchServiceScope ResearchServiceScope { get; }

    public SanctuaryLocalizationMode LocalizationMode { get; }

    public IReadOnlyList<SanctuaryContractLayer> ActiveLayers { get; }

    public bool FirstDownloadFetchLimitedToLocalBinding { get; }

    public static SanctuaryContractProfile CreateLocalOperatorDefault()
    {
        return new SanctuaryContractProfile(
            actorClass: SanctuaryActorClass.LocalOperator,
            deploymentMode: SanctuaryDeploymentMode.LocalBound,
            researchServiceScope: SanctuaryResearchServiceScope.None,
            localizationMode: SanctuaryLocalizationMode.OperatorLocalOnly,
            activeLayers:
            [
                SanctuaryContractLayer.LocalBindingLicense,
                SanctuaryContractLayer.IpOutputTelemetrySchedule
            ],
            firstDownloadFetchLimitedToLocalBinding: true);
    }

    private void Validate()
    {
        if (!FirstDownloadFetchLimitedToLocalBinding)
        {
            throw new InvalidOperationException(
                "The first Sanctuary download or fetch must remain limited to local binding, entitlement, and integrity posture.");
        }

        if (!ActiveLayers.Contains(SanctuaryContractLayer.LocalBindingLicense))
        {
            throw new InvalidOperationException(
                "Every Sanctuary contract profile must include the local binding license layer.");
        }

        if (!ActiveLayers.Contains(SanctuaryContractLayer.IpOutputTelemetrySchedule))
        {
            throw new InvalidOperationException(
                "Every Sanctuary contract profile must include the IP/output/telemetry schedule layer.");
        }

        var researchActive = ResearchServiceScope is not SanctuaryResearchServiceScope.None;
        if (researchActive && !ActiveLayers.Contains(SanctuaryContractLayer.ResearchServicesAddendum))
        {
            throw new InvalidOperationException(
                "Remote support or research scope requires the research services addendum layer.");
        }

        if (!researchActive && ActiveLayers.Contains(SanctuaryContractLayer.ResearchServicesAddendum))
        {
            throw new InvalidOperationException(
                "The research services addendum may not be active when research scope is none.");
        }

        var organizationActor = ActorClass is SanctuaryActorClass.OrganizationOperator
            or SanctuaryActorClass.OrganizationAdministrator
            or SanctuaryActorClass.RegulatedOrganizationAdministrator;
        var organizationManaged = DeploymentMode is SanctuaryDeploymentMode.OrganizationManaged
            or SanctuaryDeploymentMode.RegulatedOrganizationManaged;

        if (organizationManaged && !organizationActor)
        {
            throw new InvalidOperationException(
                "Organization-managed Sanctuary deployment requires an explicit organization actor class.");
        }

        if (organizationActor && !organizationManaged)
        {
            throw new InvalidOperationException(
                "Organization actors may be represented only through organization-managed deployment modes.");
        }

        if (DeploymentMode is SanctuaryDeploymentMode.RegulatedOrganizationManaged &&
            ActorClass is not SanctuaryActorClass.RegulatedOrganizationAdministrator)
        {
            throw new InvalidOperationException(
                "Regulated organization deployment requires the regulated organization administrator actor class.");
        }

        if (DeploymentMode is SanctuaryDeploymentMode.LocalBound &&
            ResearchServiceScope is not SanctuaryResearchServiceScope.None)
        {
            throw new InvalidOperationException(
                "Local-bound deployment may not imply remote support or research scope.");
        }

        if (LocalizationMode is SanctuaryLocalizationMode.OperatorLocalOnly && organizationActor)
        {
            throw new InvalidOperationException(
                "Organization-managed deployment requires organization-controlled or stricter localization.");
        }

        if (organizationManaged &&
            (!ActiveLayers.Contains(SanctuaryContractLayer.ProtectedDataAddendum) ||
             !ActiveLayers.Contains(SanctuaryContractLayer.LocalizationResidencySchedule)))
        {
            throw new InvalidOperationException(
                "Organization-managed deployment requires protected-data and localization layers.");
        }
    }
}

internal static class SanctuaryContractGuard
{
    public static string RequiredText(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    public static IReadOnlyList<string> RequiredTextList(IReadOnlyList<string> values, string parameterName)
    {
        var normalized = (values ?? throw new ArgumentNullException(parameterName))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", parameterName);
        }

        return normalized;
    }

    public static IReadOnlyList<string> TextListOrEmpty(IReadOnlyList<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyList<T> RequiredDistinctList<T>(IReadOnlyList<T> values, string parameterName)
    {
        var normalized = (values ?? throw new ArgumentNullException(parameterName))
            .Distinct()
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", parameterName);
        }

        return normalized;
    }

    public static IReadOnlyList<T> DistinctListOrEmpty<T>(IReadOnlyList<T>? values)
    {
        return (values ?? Array.Empty<T>())
            .Distinct()
            .ToArray();
    }
}
