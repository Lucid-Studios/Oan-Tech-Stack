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

    public RuntimeWorkbenchSessionLedger CreateRuntimeWorkbenchSessionLedger(
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        AmenableDayDreamTierAdmissibilityReceipt dayDreamTier,
        SelfRootedCrypticDepthGateReceipt depthGate,
        IReadOnlyList<WorkbenchSessionEvent>? sessionEvents = null,
        IReadOnlyList<BoundaryCondition>? boundaryConditions = null,
        string sessionState = "bounded-session-open",
        string sessionPosture = "bounded-session-open",
        string returnPosture = "return-through-bounded-workbench",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workbench);
        ArgumentNullException.ThrowIfNull(dayDreamTier);
        ArgumentNullException.ThrowIfNull(depthGate);

        return SanctuaryWorkbenchProjector.CreateRuntimeWorkbenchSessionLedger(
            workbench,
            dayDreamTier,
            depthGate,
            sessionEvents,
            boundaryConditions,
            sessionState,
            sessionPosture,
            returnPosture,
            timestampUtc);
    }

    public DayDreamCollapseReceipt CreateDayDreamCollapseReceipt(
        RuntimeWorkbenchSessionLedger sessionLedger,
        AmenableDayDreamTierAdmissibilityReceipt dayDreamTier,
        IReadOnlyList<string> boundedOutputs,
        IReadOnlyList<string> remainingNonFinalOutputs,
        IReadOnlyList<BoundaryCondition>? boundaryConditions = null,
        IReadOnlyList<ResidueMarker>? residueMarkers = null,
        string collapseState = "bounded-collapse-recorded",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(dayDreamTier);
        ArgumentNullException.ThrowIfNull(boundedOutputs);
        ArgumentNullException.ThrowIfNull(remainingNonFinalOutputs);

        return SanctuaryWorkbenchProjector.CreateDayDreamCollapseReceipt(
            sessionLedger,
            dayDreamTier,
            boundedOutputs,
            remainingNonFinalOutputs,
            boundaryConditions,
            residueMarkers,
            collapseState,
            timestampUtc);
    }

    public CrypticDepthReturnReceipt CreateCrypticDepthReturnReceipt(
        RuntimeWorkbenchSessionLedger sessionLedger,
        SelfRootedCrypticDepthGateReceipt depthGate,
        IReadOnlyList<ContinuityMarker> continuityMarkers,
        IReadOnlyList<ResidueMarker> residueMarkers,
        IReadOnlyList<BoundaryCondition>? boundaryConditions = null,
        string returnState = "clean-return-withheld",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(depthGate);
        ArgumentNullException.ThrowIfNull(continuityMarkers);
        ArgumentNullException.ThrowIfNull(residueMarkers);

        return SanctuaryWorkbenchProjector.CreateCrypticDepthReturnReceipt(
            sessionLedger,
            depthGate,
            continuityMarkers,
            residueMarkers,
            boundaryConditions,
            returnState,
            timestampUtc);
    }
}
