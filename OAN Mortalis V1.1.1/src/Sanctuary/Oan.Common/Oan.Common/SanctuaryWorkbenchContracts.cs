namespace Oan.Common;

public sealed record SanctuaryRuntimeWorkbenchSurfaceReceipt(
    string SurfaceHandle,
    string SessionPosture,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record RuntimeWorkbenchSessionLedgerReceipt(
    string LedgerHandle,
    string SessionId,
    string StateClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record WorkbenchSessionEventReceipt(
    string EventHandle,
    string SessionId,
    string EventClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BoundaryConditionReceipt(
    string ConditionHandle,
    string SessionId,
    string BoundaryClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record AmenableDayDreamTierReceipt(
    string TierHandle,
    string TierClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ResidueMarkerReceipt(
    string MarkerHandle,
    string ResidueClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record DayDreamCollapseReceipt(
    string ReceiptHandle,
    string CollapseClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record SelfRootedCrypticDepthGateReceipt(
    string GateHandle,
    string BiadRootHandle,
    string GateClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BoundaryConditionLedgerReceipt(
    string LedgerHandle,
    string LedgerClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CoherenceGainWitnessReceipt(
    string ReceiptHandle,
    string WitnessClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record InquirySessionDisciplineSurfaceReceipt(
    string SurfaceHandle,
    string InquiryClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ContinuityMarkerReceipt(
    string MarkerHandle,
    string ContinuityClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CrypticDepthReturnReceipt(
    string ReceiptHandle,
    string ReturnClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record LocalHostSanctuaryResidencyEnvelopeReceipt(
    string EnvelopeHandle,
    string ResidencyClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record RuntimeHabitationReadinessLedgerReceipt(
    string LedgerHandle,
    string ReadinessClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BoundedInhabitationLaunchRehearsalReceipt(
    string RehearsalHandle,
    string LaunchClass,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class SanctuaryWorkbenchContracts
{
    public static SanctuaryRuntimeWorkbenchSurfaceReceipt CreateRuntimeWorkbenchSurface(
        string surfaceHandle,
        string sessionPosture,
        DateTimeOffset timestampUtc,
        string reasonCode = "sanctuary-runtime-workbench-surface-bound")
        => new(surfaceHandle, sessionPosture, reasonCode, timestampUtc);

    public static RuntimeWorkbenchSessionLedgerReceipt CreateRuntimeWorkbenchSessionLedger(
        string ledgerHandle,
        string sessionId,
        string stateClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "runtime-workbench-session-ledger-bound")
        => new(ledgerHandle, sessionId, stateClass, reasonCode, timestampUtc);

    public static WorkbenchSessionEventReceipt CreateSessionEvent(
        string eventHandle,
        string sessionId,
        string eventClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "runtime-workbench-session-event-bound")
        => new(eventHandle, sessionId, eventClass, reasonCode, timestampUtc);

    public static BoundaryConditionReceipt CreateBoundaryCondition(
        string conditionHandle,
        string sessionId,
        string boundaryClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "runtime-workbench-boundary-condition-bound")
        => new(conditionHandle, sessionId, boundaryClass, reasonCode, timestampUtc);

    public static AmenableDayDreamTierReceipt CreateAmenableDayDreamTier(
        string tierHandle,
        string tierClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "amenable-day-dream-tier-admissibility-bound")
        => new(tierHandle, tierClass, reasonCode, timestampUtc);

    public static DayDreamCollapseReceipt CreateDayDreamCollapseReceipt(
        string receiptHandle,
        string collapseClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "day-dream-collapse-receipt-bound")
        => new(receiptHandle, collapseClass, reasonCode, timestampUtc);

    public static ResidueMarkerReceipt CreateResidueMarker(
        string markerHandle,
        string residueClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "residue-marker-bound")
        => new(markerHandle, residueClass, reasonCode, timestampUtc);

    public static SelfRootedCrypticDepthGateReceipt CreateSelfRootedCrypticDepthGate(
        string gateHandle,
        string biadRootHandle,
        string gateClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "self-rooted-cryptic-depth-gate-bound")
        => new(gateHandle, biadRootHandle, gateClass, reasonCode, timestampUtc);

    public static BoundaryConditionLedgerReceipt CreateBoundaryConditionLedger(
        string ledgerHandle,
        string ledgerClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "boundary-condition-ledger-bound")
        => new(ledgerHandle, ledgerClass, reasonCode, timestampUtc);

    public static CoherenceGainWitnessReceipt CreateCoherenceGainWitnessReceipt(
        string receiptHandle,
        string witnessClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "coherence-gain-witness-receipt-bound")
        => new(receiptHandle, witnessClass, reasonCode, timestampUtc);

    public static InquirySessionDisciplineSurfaceReceipt CreateInquirySessionDisciplineSurface(
        string surfaceHandle,
        string inquiryClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "inquiry-session-discipline-surface-bound")
        => new(surfaceHandle, inquiryClass, reasonCode, timestampUtc);

    public static ContinuityMarkerReceipt CreateContinuityMarker(
        string markerHandle,
        string continuityClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "continuity-marker-bound")
        => new(markerHandle, continuityClass, reasonCode, timestampUtc);

    public static CrypticDepthReturnReceipt CreateCrypticDepthReturnReceipt(
        string receiptHandle,
        string returnClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "cryptic-depth-return-receipt-bound")
        => new(receiptHandle, returnClass, reasonCode, timestampUtc);

    public static LocalHostSanctuaryResidencyEnvelopeReceipt CreateLocalHostSanctuaryResidencyEnvelope(
        string envelopeHandle,
        string residencyClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "local-host-sanctuary-residency-envelope-bound")
        => new(envelopeHandle, residencyClass, reasonCode, timestampUtc);

    public static RuntimeHabitationReadinessLedgerReceipt CreateRuntimeHabitationReadinessLedger(
        string ledgerHandle,
        string readinessClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "runtime-habitation-readiness-ledger-bound")
        => new(ledgerHandle, readinessClass, reasonCode, timestampUtc);

    public static BoundedInhabitationLaunchRehearsalReceipt CreateBoundedInhabitationLaunchRehearsal(
        string rehearsalHandle,
        string launchClass,
        DateTimeOffset timestampUtc,
        string reasonCode = "bounded-inhabitation-launch-rehearsal-bound")
        => new(rehearsalHandle, launchClass, reasonCode, timestampUtc);
}
