using Oan.Common;

namespace CradleTek.Runtime;

public sealed class SanctuaryRuntimeWorkbenchService
{
    public SanctuaryRuntimeWorkbenchSurfaceReceipt CreateRuntimeWorkbenchSurface(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        BondedParticipationLocalityLedgerReceipt localityLedger,
        string runtimeDeployabilityState,
        string sanctuaryRuntimeReadinessState,
        string runtimeWorkAdmissibilityState,
        string sessionPosture = "bounded-workbench-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(localityLedger);

        return SanctuaryWorkbenchProjector.CreateRuntimeWorkbenchSurface(
            utilitySurface,
            localityLedger,
            runtimeDeployabilityState,
            sanctuaryRuntimeReadinessState,
            runtimeWorkAdmissibilityState,
            sessionPosture,
            timestampUtc);
    }

    public AmenableDayDreamTierAdmissibilityReceipt CreateAmenableDayDreamTier(
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        IReadOnlyList<string> exploratoryPredicates,
        IReadOnlyList<string> nonFinalOutputs,
        string admissibilityState = "amenable-exploratory-only",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workbench);

        return SanctuaryWorkbenchProjector.CreateAmenableDayDreamTier(
            workbench,
            exploratoryPredicates,
            nonFinalOutputs,
            admissibilityState,
            timestampUtc);
    }

    public SelfRootedCrypticDepthGateReceipt CreateSelfRootedCrypticDepthGate(
        GovernedThreadBirthReceipt threadBirth,
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        AmenableDayDreamTierAdmissibilityReceipt dayDreamTier,
        string gateState = "provisionally-rooted-withheld",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadBirth);
        ArgumentNullException.ThrowIfNull(workbench);
        ArgumentNullException.ThrowIfNull(dayDreamTier);

        return SanctuaryWorkbenchProjector.CreateSelfRootedCrypticDepthGate(
            threadBirth,
            workbench,
            dayDreamTier,
            gateState,
            timestampUtc);
    }
}
