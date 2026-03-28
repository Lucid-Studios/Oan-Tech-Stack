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

    public LocalHostSanctuaryResidencyEnvelopeReceipt CreateLocalHostSanctuaryResidencyEnvelope(
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        RuntimeWorkbenchSessionLedger sessionLedger,
        ReachReturnDissolutionReceipt returnReceipt,
        LocalityDistinctionWitnessLedgerReceipt localityWitnessLedger,
        string residencyState = "local-host-sanctuary-residency-envelope-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workbench);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(returnReceipt);
        ArgumentNullException.ThrowIfNull(localityWitnessLedger);

        return SanctuaryWorkbenchProjector.CreateLocalHostSanctuaryResidencyEnvelope(
            workbench,
            sessionLedger,
            returnReceipt,
            localityWitnessLedger,
            residencyState,
            timestampUtc);
    }

    public RuntimeHabitationReadinessLedgerReceipt CreateRuntimeHabitationReadinessLedger(
        LocalHostSanctuaryResidencyEnvelopeReceipt residencyEnvelope,
        RuntimeWorkbenchSessionLedger sessionLedger,
        string habitationState = "bounded-habitation-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(residencyEnvelope);
        ArgumentNullException.ThrowIfNull(sessionLedger);

        return SanctuaryWorkbenchProjector.CreateRuntimeHabitationReadinessLedger(
            residencyEnvelope,
            sessionLedger,
            habitationState,
            timestampUtc);
    }

    public BoundedInhabitationLaunchRehearsalReceipt CreateBoundedInhabitationLaunchRehearsal(
        LocalHostSanctuaryResidencyEnvelopeReceipt residencyEnvelope,
        RuntimeHabitationReadinessLedgerReceipt readinessLedger,
        RuntimeWorkbenchSessionLedger sessionLedger,
        ReachReturnDissolutionReceipt returnReceipt,
        string launchState = "bounded-inhabitation-launch-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(residencyEnvelope);
        ArgumentNullException.ThrowIfNull(readinessLedger);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(returnReceipt);

        return SanctuaryWorkbenchProjector.CreateBoundedInhabitationLaunchRehearsal(
            residencyEnvelope,
            readinessLedger,
            sessionLedger,
            returnReceipt,
            launchState,
            timestampUtc);
    }

    public InquirySessionDisciplineSurfaceReceipt CreateInquirySessionDisciplineSurface(
        RuntimeHabitationReadinessLedgerReceipt readinessLedger,
        RuntimeWorkbenchSessionLedger sessionLedger,
        DayDreamCollapseReceipt collapseReceipt,
        CrypticDepthReturnReceipt returnReceipt,
        string inquiryState = "inquiry-session-discipline-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(readinessLedger);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(collapseReceipt);
        ArgumentNullException.ThrowIfNull(returnReceipt);

        return SanctuaryWorkbenchProjector.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            returnReceipt,
            inquiryState,
            timestampUtc);
    }

    public BoundaryConditionLedgerReceipt CreateBoundaryConditionLedger(
        RuntimeHabitationReadinessLedgerReceipt readinessLedger,
        RuntimeWorkbenchSessionLedger sessionLedger,
        InquirySessionDisciplineSurfaceReceipt inquirySurface,
        DayDreamCollapseReceipt collapseReceipt,
        CrypticDepthReturnReceipt returnReceipt,
        string ledgerState = "boundary-condition-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(readinessLedger);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(inquirySurface);
        ArgumentNullException.ThrowIfNull(collapseReceipt);
        ArgumentNullException.ThrowIfNull(returnReceipt);

        return SanctuaryWorkbenchProjector.CreateBoundaryConditionLedger(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            collapseReceipt,
            returnReceipt,
            ledgerState,
            timestampUtc);
    }

    public CoherenceGainWitnessReceipt CreateCoherenceGainWitnessReceipt(
        RuntimeHabitationReadinessLedgerReceipt readinessLedger,
        RuntimeWorkbenchSessionLedger sessionLedger,
        InquirySessionDisciplineSurfaceReceipt inquirySurface,
        BoundaryConditionLedgerReceipt boundaryLedger,
        string witnessState = "coherence-gain-witness-receipt-ready",
        string coherenceState = "coherence-gain-witnessed",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(readinessLedger);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(inquirySurface);
        ArgumentNullException.ThrowIfNull(boundaryLedger);

        return SanctuaryWorkbenchProjector.CreateCoherenceGainWitnessReceipt(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            boundaryLedger,
            witnessState,
            coherenceState,
            timestampUtc);
    }
}
