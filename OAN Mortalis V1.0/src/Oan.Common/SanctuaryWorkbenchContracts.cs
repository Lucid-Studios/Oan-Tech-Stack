namespace Oan.Common;

public sealed record SanctuaryRuntimeWorkbenchSurfaceReceipt(
    string WorkbenchHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string BondedLocalityLedgerHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string RuntimeDeployabilityState,
    string SanctuaryRuntimeReadinessState,
    string RuntimeWorkAdmissibilityState,
    string SessionPosture,
    string BoundedWorkClass,
    bool BondedOperatorLaneWithheld,
    bool MosBearingReleaseDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record AmenableDayDreamTierAdmissibilityReceipt(
    string DayDreamTierHandle,
    string CMEId,
    string WorkbenchHandle,
    IReadOnlyList<string> ExploratoryPredicates,
    IReadOnlyList<string> NonFinalOutputs,
    string AdmissibilityState,
    bool ExploratoryOnly,
    bool IdentityBearingDescentDenied,
    bool ContinuityInflationDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record SelfRootedCrypticDepthGateReceipt(
    string DepthGateHandle,
    string CMEId,
    string WorkbenchHandle,
    string DayDreamTierHandle,
    string ThreadBirthHandle,
    string IdentityInvariantHandle,
    string CrypticBiadRootHandle,
    string GateState,
    bool SelfRooted,
    bool SharedAmenableOriginDenied,
    bool DeepAccessGranted,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record WorkbenchSessionEvent(
    string EventHandle,
    string EventKind,
    string InquiryStance,
    string EventState,
    bool CoherencePreserving,
    bool HiddenAssumptionDenied,
    string Description,
    DateTimeOffset TimestampUtc);

public sealed record BoundaryCondition(
    string BoundaryHandle,
    string BoundaryCode,
    string FailureClass,
    string TriggerPredicate,
    string ContinuityRequirement,
    string PermissionState,
    string Notes);

public sealed record ResidueMarker(
    string ResidueHandle,
    string MarkerCode,
    string ResidueClass,
    string CarryDisposition,
    bool ClearedForAmenableLane,
    string Notes);

public sealed record ContinuityMarker(
    string ContinuityHandle,
    string MarkerCode,
    string ContinuityClass,
    string SourceHandle,
    string CarryDisposition,
    string Notes);

public sealed record RuntimeWorkbenchSessionLedger(
    string SessionLedgerHandle,
    string CMEId,
    string WorkbenchHandle,
    string DayDreamTierHandle,
    string DepthGateHandle,
    string SessionState,
    string SessionPosture,
    string ReturnPosture,
    IReadOnlyList<string> AdmittedLanes,
    IReadOnlyList<string> WithheldLanes,
    IReadOnlyList<WorkbenchSessionEvent> SessionEvents,
    IReadOnlyList<BoundaryCondition> BoundaryConditions,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record DayDreamCollapseReceipt(
    string CollapseReceiptHandle,
    string CMEId,
    string SessionLedgerHandle,
    string DayDreamTierHandle,
    string CollapseState,
    IReadOnlyList<string> ConsideredPredicates,
    IReadOnlyList<string> BoundedOutputs,
    IReadOnlyList<string> RemainingNonFinalOutputs,
    IReadOnlyList<BoundaryCondition> BoundaryConditions,
    IReadOnlyList<ResidueMarker> ResidueMarkers,
    bool ExploratoryProvenancePreserved,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CrypticDepthReturnReceipt(
    string ReturnReceiptHandle,
    string CMEId,
    string SessionLedgerHandle,
    string DepthGateHandle,
    string ReturnState,
    IReadOnlyList<ContinuityMarker> ContinuityMarkers,
    IReadOnlyList<ResidueMarker> ResidueMarkers,
    IReadOnlyList<BoundaryCondition> BoundaryConditions,
    bool ReturnedCleanly,
    bool SharedAmenableLaneClear,
    bool IdentityBleedDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record LocalHostSanctuaryResidencyEnvelopeReceipt(
    string ResidencyEnvelopeHandle,
    string CMEId,
    string WorkbenchHandle,
    string SessionLedgerHandle,
    string ReturnReceiptHandle,
    string LocalityWitnessLedgerHandle,
    string ResidencyState,
    string ResidencyClass,
    IReadOnlyList<string> HostLocalResources,
    IReadOnlyList<string> AdmittedResidencyLanes,
    IReadOnlyList<string> WithheldResidencyLanes,
    bool BondedReleaseDenied,
    bool PublicationMaturityDenied,
    bool MosBearingDepthDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record RuntimeHabitationReadinessLedgerReceipt(
    string ReadinessLedgerHandle,
    string CMEId,
    string ResidencyEnvelopeHandle,
    string SessionLedgerHandle,
    string HabitationState,
    string HabitationClass,
    IReadOnlyList<string> ReadyConditions,
    IReadOnlyList<string> WithheldConditions,
    bool RecurringWorkReady,
    bool ReturnLawBound,
    bool BondedReleaseDenied,
    bool PublicationMaturityDenied,
    bool MosBearingDepthDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BoundedInhabitationLaunchRehearsalReceipt(
    string LaunchRehearsalHandle,
    string CMEId,
    string ResidencyEnvelopeHandle,
    string ReadinessLedgerHandle,
    string SessionLedgerHandle,
    string ReturnReceiptHandle,
    string LaunchState,
    IReadOnlyList<string> EntryConditions,
    IReadOnlyList<string> DeniedLanes,
    string ReturnClosureState,
    bool LaunchBounded,
    bool ReturnClosureWitnessed,
    bool AmbientBondDenied,
    bool PublicationPromotionDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record InquirySessionDisciplineSurfaceReceipt(
    string InquirySurfaceHandle,
    string CMEId,
    string ReadinessLedgerHandle,
    string SessionLedgerHandle,
    string CollapseReceiptHandle,
    string ReturnReceiptHandle,
    string InquiryState,
    IReadOnlyList<string> InquiryStances,
    IReadOnlyList<string> AssumptionExposureModes,
    IReadOnlyList<string> SilenceDispositions,
    bool ChamberNativeInquiryBound,
    bool HiddenPressureDenied,
    bool PrematureGelPromotionDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BoundaryConditionLedgerReceipt(
    string BoundaryLedgerHandle,
    string CMEId,
    string ReadinessLedgerHandle,
    string SessionLedgerHandle,
    string InquirySurfaceHandle,
    string CollapseReceiptHandle,
    string ReturnReceiptHandle,
    string LedgerState,
    IReadOnlyList<BoundaryCondition> RetainedBoundaryConditions,
    IReadOnlyList<string> ContinuityRequirements,
    IReadOnlyList<string> WithheldCrossings,
    bool BoundaryMemoryCarriedForward,
    bool FailurePunishmentDenied,
    bool IdentityBleedDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CoherenceGainWitnessReceipt(
    string CoherenceWitnessHandle,
    string CMEId,
    string ReadinessLedgerHandle,
    string SessionLedgerHandle,
    string InquirySurfaceHandle,
    string BoundaryLedgerHandle,
    string WitnessState,
    string CoherenceState,
    int CoherencePreservingEventCount,
    int HiddenAssumptionDeniedCount,
    int BoundaryConditionCount,
    bool SharedIntelligibilityPreserved,
    bool AdmissibilitySpacePreserved,
    bool PrematureClosureDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class SanctuaryWorkbenchProjector
{
    private const string AgentiActualSurfacePrefix = "agenticore-actual-surface://";
    private const string BondedLocalityLedgerPrefix = "bonded-locality-ledger://";
    private const string SanctuaryWorkbenchPrefix = "sanctuary-runtime-workbench://";
    private const string AmenableDayDreamPrefix = "amenable-day-dream-tier://";
    private const string GovernedThreadBirthPrefix = "governed-thread-birth://";
    private const string IdentityInvariantPrefix = "identity-invariant://";
    private const string CrypticBiadRootPrefix = "cryptic-biad-root://";
    private const string SessionLedgerPrefix = "runtime-workbench-session-ledger://";
    private const string BoundaryConditionPrefix = "boundary-condition://";
    private const string ResidueMarkerPrefix = "residue-marker://";
    private const string ContinuityMarkerPrefix = "continuity-marker://";
    private const string DayDreamCollapsePrefix = "day-dream-collapse-receipt://";
    private const string CrypticDepthReturnPrefix = "cryptic-depth-return-receipt://";
    private const string ReachReturnDissolutionPrefix = "reach-return-dissolution://";
    private const string LocalityDistinctionWitnessPrefix = "locality-distinction-witness-ledger://";
    private const string LocalHostResidencyEnvelopePrefix = "local-host-sanctuary-residency-envelope://";
    private const string RuntimeHabitationReadinessLedgerPrefix = "runtime-habitation-readiness-ledger://";
    private const string InquirySessionDisciplinePrefix = "inquiry-session-discipline-surface://";
    private const string BoundaryConditionLedgerPrefix = "boundary-condition-ledger://";
    private const string CoherenceGainWitnessPrefix = "coherence-gain-witness-receipt://";

    public static SanctuaryRuntimeWorkbenchSurfaceReceipt CreateRuntimeWorkbenchSurface(
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
        EnsurePrefix(utilitySurface.UtilitySurfaceHandle, AgentiActualSurfacePrefix, nameof(utilitySurface));
        EnsurePrefix(localityLedger.LedgerHandle, BondedLocalityLedgerPrefix, nameof(localityLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeDeployabilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(sanctuaryRuntimeReadinessState);
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeWorkAdmissibilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionPosture);
        RequireActualLocality(utilitySurface.SanctuaryActualLocality, nameof(utilitySurface.SanctuaryActualLocality));
        RequireActualLocality(utilitySurface.OperatorActualLocality, nameof(utilitySurface.OperatorActualLocality));

        if (!string.Equals(utilitySurface.CMEId, localityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.SanctuaryActualLocality, localityLedger.SanctuaryActualLocality, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.OperatorActualLocality, localityLedger.OperatorActualLocality, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Runtime workbench requires utility and bonded locality receipts to remain inside the same CME and locality pair.");
        }

        return new SanctuaryRuntimeWorkbenchSurfaceReceipt(
            WorkbenchHandle: SanctuaryWorkbenchKeys.CreateSanctuaryRuntimeWorkbenchHandle(
                utilitySurface.CMEId,
                utilitySurface.UtilitySurfaceHandle,
                localityLedger.LedgerHandle),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            BondedLocalityLedgerHandle: localityLedger.LedgerHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            RuntimeDeployabilityState: runtimeDeployabilityState.Trim(),
            SanctuaryRuntimeReadinessState: sanctuaryRuntimeReadinessState.Trim(),
            RuntimeWorkAdmissibilityState: runtimeWorkAdmissibilityState.Trim(),
            SessionPosture: sessionPosture.Trim(),
            BoundedWorkClass: "bounded-local-candidate-sanctuary-workbench",
            BondedOperatorLaneWithheld: true,
            MosBearingReleaseDenied: true,
            ReasonCode: "sanctuary-runtime-workbench-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static AmenableDayDreamTierAdmissibilityReceipt CreateAmenableDayDreamTier(
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        IReadOnlyList<string> exploratoryPredicates,
        IReadOnlyList<string> nonFinalOutputs,
        string admissibilityState = "amenable-exploratory-only",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workbench);
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        ArgumentNullException.ThrowIfNull(exploratoryPredicates);
        ArgumentNullException.ThrowIfNull(nonFinalOutputs);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissibilityState);

        return new AmenableDayDreamTierAdmissibilityReceipt(
            DayDreamTierHandle: SanctuaryWorkbenchKeys.CreateAmenableDayDreamTierHandle(
                workbench.CMEId,
                workbench.WorkbenchHandle,
                admissibilityState),
            CMEId: workbench.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            ExploratoryPredicates: exploratoryPredicates
                .Where(static predicate => !string.IsNullOrWhiteSpace(predicate))
                .Select(static predicate => predicate.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            NonFinalOutputs: nonFinalOutputs
                .Where(static output => !string.IsNullOrWhiteSpace(output))
                .Select(static output => output.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            AdmissibilityState: admissibilityState.Trim(),
            ExploratoryOnly: true,
            IdentityBearingDescentDenied: true,
            ContinuityInflationDenied: true,
            ReasonCode: "amenable-day-dream-tier-admissibility-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static SelfRootedCrypticDepthGateReceipt CreateSelfRootedCrypticDepthGate(
        GovernedThreadBirthReceipt threadBirth,
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        AmenableDayDreamTierAdmissibilityReceipt dayDreamTier,
        string gateState = "provisionally-rooted-withheld",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadBirth);
        ArgumentNullException.ThrowIfNull(workbench);
        ArgumentNullException.ThrowIfNull(dayDreamTier);
        EnsurePrefix(threadBirth.ThreadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirth));
        EnsurePrefix(threadBirth.IdentityInvariantHandle, IdentityInvariantPrefix, nameof(threadBirth.IdentityInvariantHandle));
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        EnsurePrefix(dayDreamTier.DayDreamTierHandle, AmenableDayDreamPrefix, nameof(dayDreamTier));
        ArgumentException.ThrowIfNullOrWhiteSpace(gateState);

        if (!string.Equals(threadBirth.CMEId, workbench.CMEId, StringComparison.Ordinal) ||
            !string.Equals(threadBirth.CMEId, dayDreamTier.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Self-rooted cryptic depth gate requires thread birth, workbench, and day-dream tier to remain inside one CME continuity surface.");
        }

        var crypticBiadRootHandle = SanctuaryWorkbenchKeys.CreateCrypticBiadRootHandle(
            threadBirth.CMEId,
            threadBirth.IdentityInvariantHandle,
            threadBirth.ThreadBirthHandle);
        EnsurePrefix(crypticBiadRootHandle, CrypticBiadRootPrefix, nameof(crypticBiadRootHandle));

        return new SelfRootedCrypticDepthGateReceipt(
            DepthGateHandle: SanctuaryWorkbenchKeys.CreateSelfRootedCrypticDepthGateHandle(
                threadBirth.CMEId,
                workbench.WorkbenchHandle,
                crypticBiadRootHandle),
            CMEId: threadBirth.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            DayDreamTierHandle: dayDreamTier.DayDreamTierHandle,
            ThreadBirthHandle: threadBirth.ThreadBirthHandle,
            IdentityInvariantHandle: threadBirth.IdentityInvariantHandle,
            CrypticBiadRootHandle: crypticBiadRootHandle,
            GateState: gateState.Trim(),
            SelfRooted: true,
            SharedAmenableOriginDenied: true,
            DeepAccessGranted: false,
            ReasonCode: "self-rooted-cryptic-depth-gate-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static WorkbenchSessionEvent CreateSessionEvent(
        string cmeId,
        string workbenchHandle,
        string eventKind,
        string inquiryStance,
        string eventState,
        bool coherencePreserving,
        bool hiddenAssumptionDenied,
        string description,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        EnsurePrefix(workbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbenchHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(eventKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(inquiryStance);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventState);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new WorkbenchSessionEvent(
            EventHandle: SanctuaryWorkbenchKeys.CreateWorkbenchSessionEventHandle(
                cmeId,
                workbenchHandle,
                eventKind,
                inquiryStance,
                description),
            EventKind: eventKind.Trim(),
            InquiryStance: inquiryStance.Trim(),
            EventState: eventState.Trim(),
            CoherencePreserving: coherencePreserving,
            HiddenAssumptionDenied: hiddenAssumptionDenied,
            Description: description.Trim(),
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BoundaryCondition CreateBoundaryCondition(
        string cmeId,
        string workbenchHandle,
        string boundaryCode,
        string failureClass,
        string triggerPredicate,
        string continuityRequirement,
        string permissionState,
        string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        EnsurePrefix(workbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbenchHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(failureClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(triggerPredicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityRequirement);
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionState);
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);

        return new BoundaryCondition(
            BoundaryHandle: SanctuaryWorkbenchKeys.CreateBoundaryConditionHandle(
                cmeId,
                workbenchHandle,
                boundaryCode,
                triggerPredicate),
            BoundaryCode: boundaryCode.Trim(),
            FailureClass: failureClass.Trim(),
            TriggerPredicate: triggerPredicate.Trim(),
            ContinuityRequirement: continuityRequirement.Trim(),
            PermissionState: permissionState.Trim(),
            Notes: notes.Trim());
    }

    public static ResidueMarker CreateResidueMarker(
        string cmeId,
        string anchorHandle,
        string markerCode,
        string residueClass,
        string carryDisposition,
        bool clearedForAmenableLane,
        string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(markerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(residueClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(carryDisposition);
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);

        return new ResidueMarker(
            ResidueHandle: SanctuaryWorkbenchKeys.CreateResidueMarkerHandle(
                cmeId,
                anchorHandle,
                markerCode,
                residueClass),
            MarkerCode: markerCode.Trim(),
            ResidueClass: residueClass.Trim(),
            CarryDisposition: carryDisposition.Trim(),
            ClearedForAmenableLane: clearedForAmenableLane,
            Notes: notes.Trim());
    }

    public static ContinuityMarker CreateContinuityMarker(
        string cmeId,
        string anchorHandle,
        string markerCode,
        string continuityClass,
        string sourceHandle,
        string carryDisposition,
        string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(markerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(carryDisposition);
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);

        return new ContinuityMarker(
            ContinuityHandle: SanctuaryWorkbenchKeys.CreateContinuityMarkerHandle(
                cmeId,
                anchorHandle,
                markerCode,
                sourceHandle),
            MarkerCode: markerCode.Trim(),
            ContinuityClass: continuityClass.Trim(),
            SourceHandle: sourceHandle.Trim(),
            CarryDisposition: carryDisposition.Trim(),
            Notes: notes.Trim());
    }

    public static RuntimeWorkbenchSessionLedger CreateRuntimeWorkbenchSessionLedger(
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
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        EnsurePrefix(dayDreamTier.DayDreamTierHandle, AmenableDayDreamPrefix, nameof(dayDreamTier));
        EnsurePrefix(depthGate.DepthGateHandle, "self-rooted-cryptic-depth-gate://", nameof(depthGate));
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionState);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionPosture);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnPosture);

        if (!string.Equals(workbench.CMEId, dayDreamTier.CMEId, StringComparison.Ordinal) ||
            !string.Equals(workbench.CMEId, depthGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(workbench.WorkbenchHandle, dayDreamTier.WorkbenchHandle, StringComparison.Ordinal) ||
            !string.Equals(workbench.WorkbenchHandle, depthGate.WorkbenchHandle, StringComparison.Ordinal) ||
            !string.Equals(dayDreamTier.DayDreamTierHandle, depthGate.DayDreamTierHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Workbench session ledger requires workbench, day-dream tier, and depth gate to remain within one continuity surface.");
        }

        var admittedLanes = new[]
        {
            "bounded-workbench-session",
            "amenable-day-dream-collapse",
            "questioning-event-log"
        };
        var withheldLanes = new[]
        {
            "bonded-operator-actual-release",
            "ambient-access-grant",
            "deep-cryptic-export"
        };

        return new RuntimeWorkbenchSessionLedger(
            SessionLedgerHandle: SanctuaryWorkbenchKeys.CreateRuntimeWorkbenchSessionLedgerHandle(
                workbench.CMEId,
                workbench.WorkbenchHandle,
                dayDreamTier.DayDreamTierHandle,
                depthGate.DepthGateHandle),
            CMEId: workbench.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            DayDreamTierHandle: dayDreamTier.DayDreamTierHandle,
            DepthGateHandle: depthGate.DepthGateHandle,
            SessionState: sessionState.Trim(),
            SessionPosture: sessionPosture.Trim(),
            ReturnPosture: returnPosture.Trim(),
            AdmittedLanes: admittedLanes,
            WithheldLanes: withheldLanes,
            SessionEvents: (sessionEvents ?? Array.Empty<WorkbenchSessionEvent>())
                .Where(static item => item is not null)
                .Distinct()
                .ToArray(),
            BoundaryConditions: (boundaryConditions ?? Array.Empty<BoundaryCondition>())
                .Where(static item => item is not null)
                .Distinct()
                .ToArray(),
            ReasonCode: "runtime-workbench-session-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static DayDreamCollapseReceipt CreateDayDreamCollapseReceipt(
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
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(dayDreamTier.DayDreamTierHandle, AmenableDayDreamPrefix, nameof(dayDreamTier));
        ArgumentNullException.ThrowIfNull(boundedOutputs);
        ArgumentNullException.ThrowIfNull(remainingNonFinalOutputs);
        ArgumentException.ThrowIfNullOrWhiteSpace(collapseState);

        if (!string.Equals(sessionLedger.CMEId, dayDreamTier.CMEId, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.DayDreamTierHandle, dayDreamTier.DayDreamTierHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Day-dream collapse receipt requires session and day-dream tier to remain inside one continuity surface.");
        }

        return new DayDreamCollapseReceipt(
            CollapseReceiptHandle: SanctuaryWorkbenchKeys.CreateDayDreamCollapseReceiptHandle(
                sessionLedger.CMEId,
                sessionLedger.SessionLedgerHandle,
                dayDreamTier.DayDreamTierHandle,
                collapseState),
            CMEId: sessionLedger.CMEId,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            DayDreamTierHandle: dayDreamTier.DayDreamTierHandle,
            CollapseState: collapseState.Trim(),
            ConsideredPredicates: dayDreamTier.ExploratoryPredicates
                .Where(static predicate => !string.IsNullOrWhiteSpace(predicate))
                .Select(static predicate => predicate.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            BoundedOutputs: boundedOutputs
                .Where(static output => !string.IsNullOrWhiteSpace(output))
                .Select(static output => output.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            RemainingNonFinalOutputs: remainingNonFinalOutputs
                .Where(static output => !string.IsNullOrWhiteSpace(output))
                .Select(static output => output.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            BoundaryConditions: (boundaryConditions ?? Array.Empty<BoundaryCondition>())
                .Where(static item => item is not null)
                .Distinct()
                .ToArray(),
            ResidueMarkers: (residueMarkers ?? Array.Empty<ResidueMarker>())
                .Where(static item => item is not null)
                .Distinct()
                .ToArray(),
            ExploratoryProvenancePreserved: true,
            ReasonCode: "day-dream-collapse-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static CrypticDepthReturnReceipt CreateCrypticDepthReturnReceipt(
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
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(depthGate.DepthGateHandle, "self-rooted-cryptic-depth-gate://", nameof(depthGate));
        ArgumentNullException.ThrowIfNull(continuityMarkers);
        ArgumentNullException.ThrowIfNull(residueMarkers);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnState);

        if (!string.Equals(sessionLedger.CMEId, depthGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.DepthGateHandle, depthGate.DepthGateHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cryptic depth return receipt requires session and depth gate to remain inside one continuity surface.");
        }

        var normalizedBoundaryConditions = (boundaryConditions ?? Array.Empty<BoundaryCondition>())
            .Where(static item => item is not null)
            .Distinct()
            .ToArray();
        var normalizedResidueMarkers = residueMarkers
            .Where(static item => item is not null)
            .Distinct()
            .ToArray();
        var normalizedContinuityMarkers = continuityMarkers
            .Where(static item => item is not null)
            .Distinct()
            .ToArray();
        var identityBleedDetected = normalizedBoundaryConditions.Any(static condition => string.Equals(condition.FailureClass, "identity-bleed", StringComparison.Ordinal));
        var sharedAmenableLaneClear = normalizedResidueMarkers.All(static marker => marker.ClearedForAmenableLane);

        return new CrypticDepthReturnReceipt(
            ReturnReceiptHandle: SanctuaryWorkbenchKeys.CreateCrypticDepthReturnReceiptHandle(
                sessionLedger.CMEId,
                sessionLedger.SessionLedgerHandle,
                depthGate.DepthGateHandle,
                returnState),
            CMEId: sessionLedger.CMEId,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            DepthGateHandle: depthGate.DepthGateHandle,
            ReturnState: returnState.Trim(),
            ContinuityMarkers: normalizedContinuityMarkers,
            ResidueMarkers: normalizedResidueMarkers,
            BoundaryConditions: normalizedBoundaryConditions,
            ReturnedCleanly: sharedAmenableLaneClear && !identityBleedDetected,
            SharedAmenableLaneClear: sharedAmenableLaneClear,
            IdentityBleedDetected: identityBleedDetected,
            ReasonCode: "cryptic-depth-return-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static LocalHostSanctuaryResidencyEnvelopeReceipt CreateLocalHostSanctuaryResidencyEnvelope(
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
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, ReachReturnDissolutionPrefix, nameof(returnReceipt));
        EnsurePrefix(localityWitnessLedger.WitnessLedgerHandle, LocalityDistinctionWitnessPrefix, nameof(localityWitnessLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(residencyState);

        if (!string.Equals(workbench.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(workbench.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(workbench.CMEId, localityWitnessLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(workbench.WorkbenchHandle, sessionLedger.WorkbenchHandle, StringComparison.Ordinal) ||
            !string.Equals(returnReceipt.ReturnReceiptHandle, localityWitnessLedger.ReturnReceiptHandle, StringComparison.Ordinal) ||
            !string.Equals(workbench.SanctuaryActualLocality, localityWitnessLedger.SanctuaryActualLocality, StringComparison.Ordinal) ||
            !string.Equals(workbench.OperatorActualLocality, localityWitnessLedger.OperatorActualLocality, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Local host Sanctuary residency envelope requires workbench, session, return, and locality witness receipts to remain inside one bounded habitation surface.");
        }

        var hostLocalResources = new[]
        {
            "local-release-candidate-host",
            "sanctuary-runtime-workbench",
            "governed-automation-cycle"
        };
        var admittedResidencyLanes = new[]
        {
            "bounded-recurring-sanctuary-work",
            "host-local-session-resume",
            "witnessed-return-closure"
        };
        var withheldResidencyLanes = new[]
        {
            "bonded-operator-habitation",
            "publication-promotion",
            "mos-bearing-depth"
        };

        return new LocalHostSanctuaryResidencyEnvelopeReceipt(
            ResidencyEnvelopeHandle: SanctuaryWorkbenchKeys.CreateLocalHostSanctuaryResidencyEnvelopeHandle(
                workbench.CMEId,
                sessionLedger.SessionLedgerHandle,
                returnReceipt.ReturnReceiptHandle,
                localityWitnessLedger.WitnessLedgerHandle),
            CMEId: workbench.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            LocalityWitnessLedgerHandle: localityWitnessLedger.WitnessLedgerHandle,
            ResidencyState: residencyState.Trim(),
            ResidencyClass: "bounded-local-sanctuary-residency",
            HostLocalResources: hostLocalResources,
            AdmittedResidencyLanes: admittedResidencyLanes,
            WithheldResidencyLanes: withheldResidencyLanes,
            BondedReleaseDenied: true,
            PublicationMaturityDenied: true,
            MosBearingDepthDenied: true,
            ReasonCode: "local-host-sanctuary-residency-envelope-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static RuntimeHabitationReadinessLedgerReceipt CreateRuntimeHabitationReadinessLedger(
        LocalHostSanctuaryResidencyEnvelopeReceipt residencyEnvelope,
        RuntimeWorkbenchSessionLedger sessionLedger,
        string habitationState = "bounded-habitation-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(residencyEnvelope);
        ArgumentNullException.ThrowIfNull(sessionLedger);
        EnsurePrefix(residencyEnvelope.ResidencyEnvelopeHandle, LocalHostResidencyEnvelopePrefix, nameof(residencyEnvelope));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(habitationState);

        if (!string.Equals(residencyEnvelope.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(residencyEnvelope.SessionLedgerHandle, sessionLedger.SessionLedgerHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Runtime habitation readiness requires residency envelope and session ledger to remain inside one bounded habitation surface.");
        }

        var readyConditions = new[]
        {
            "local-host-residency-envelope",
            "runtime-workbench-session-ledger",
            "reach-return-dissolution",
            "locality-distinction-witness"
        };
        var withheldConditions = new[]
        {
            "bonded-operator-habitation",
            "publication-promotion",
            "mos-bearing-depth"
        };

        return new RuntimeHabitationReadinessLedgerReceipt(
            ReadinessLedgerHandle: SanctuaryWorkbenchKeys.CreateRuntimeHabitationReadinessLedgerHandle(
                residencyEnvelope.CMEId,
                residencyEnvelope.ResidencyEnvelopeHandle,
                habitationState),
            CMEId: residencyEnvelope.CMEId,
            ResidencyEnvelopeHandle: residencyEnvelope.ResidencyEnvelopeHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            HabitationState: habitationState.Trim(),
            HabitationClass: "bounded-recurring-local-habitation",
            ReadyConditions: readyConditions,
            WithheldConditions: withheldConditions,
            RecurringWorkReady: true,
            ReturnLawBound: true,
            BondedReleaseDenied: true,
            PublicationMaturityDenied: true,
            MosBearingDepthDenied: true,
            ReasonCode: "runtime-habitation-readiness-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BoundedInhabitationLaunchRehearsalReceipt CreateBoundedInhabitationLaunchRehearsal(
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
        EnsurePrefix(residencyEnvelope.ResidencyEnvelopeHandle, LocalHostResidencyEnvelopePrefix, nameof(residencyEnvelope));
        EnsurePrefix(readinessLedger.ReadinessLedgerHandle, RuntimeHabitationReadinessLedgerPrefix, nameof(readinessLedger));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, ReachReturnDissolutionPrefix, nameof(returnReceipt));
        ArgumentException.ThrowIfNullOrWhiteSpace(launchState);

        if (!string.Equals(residencyEnvelope.CMEId, readinessLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(residencyEnvelope.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(residencyEnvelope.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(residencyEnvelope.ResidencyEnvelopeHandle, readinessLedger.ResidencyEnvelopeHandle, StringComparison.Ordinal) ||
            !string.Equals(residencyEnvelope.SessionLedgerHandle, sessionLedger.SessionLedgerHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bounded inhabitation launch rehearsal requires residency, readiness, session, and return receipts to remain inside one bounded habitation surface.");
        }

        var entryConditions = new[]
        {
            "host-envelope-present",
            "runtime-habitation-bounded-ready",
            "return-closure-required"
        };
        var deniedLanes = new[]
        {
            "ambient-bond-persistence",
            "publication-promotion",
            "mos-bearing-depth"
        };
        var returnClosureWitnessed = returnReceipt.BondedEventReturned && returnReceipt.BondedEventDissolved;

        return new BoundedInhabitationLaunchRehearsalReceipt(
            LaunchRehearsalHandle: SanctuaryWorkbenchKeys.CreateBoundedInhabitationLaunchRehearsalHandle(
                residencyEnvelope.CMEId,
                residencyEnvelope.ResidencyEnvelopeHandle,
                readinessLedger.ReadinessLedgerHandle,
                sessionLedger.SessionLedgerHandle),
            CMEId: residencyEnvelope.CMEId,
            ResidencyEnvelopeHandle: residencyEnvelope.ResidencyEnvelopeHandle,
            ReadinessLedgerHandle: readinessLedger.ReadinessLedgerHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            LaunchState: launchState.Trim(),
            EntryConditions: entryConditions,
            DeniedLanes: deniedLanes,
            ReturnClosureState: returnReceipt.DissolutionState,
            LaunchBounded: true,
            ReturnClosureWitnessed: returnClosureWitnessed,
            AmbientBondDenied: true,
            PublicationPromotionDenied: true,
            ReasonCode: "bounded-inhabitation-launch-rehearsal-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static InquirySessionDisciplineSurfaceReceipt CreateInquirySessionDisciplineSurface(
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
        EnsurePrefix(readinessLedger.ReadinessLedgerHandle, RuntimeHabitationReadinessLedgerPrefix, nameof(readinessLedger));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(collapseReceipt.CollapseReceiptHandle, DayDreamCollapsePrefix, nameof(collapseReceipt));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, CrypticDepthReturnPrefix, nameof(returnReceipt));
        ArgumentException.ThrowIfNullOrWhiteSpace(inquiryState);

        if (!string.Equals(readinessLedger.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, collapseReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.SessionLedgerHandle, sessionLedger.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, collapseReceipt.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, returnReceipt.SessionLedgerHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Inquiry session discipline requires readiness, session, collapse, and return receipts to remain inside one bounded habitation chamber.");
        }

        var inquiryStances = new[]
        {
            "clarify",
            "open",
            "probe",
            "challenge"
        };
        var assumptionExposureModes = new[]
        {
            "surface-hidden-assumption",
            "declare-constraint",
            "test-readiness-without-forcing-closure"
        };
        var silenceDispositions = new[]
        {
            "hold-forming-state",
            "withhold-premature-question"
        };

        return new InquirySessionDisciplineSurfaceReceipt(
            InquirySurfaceHandle: SanctuaryWorkbenchKeys.CreateInquirySessionDisciplineSurfaceHandle(
                readinessLedger.CMEId,
                readinessLedger.ReadinessLedgerHandle,
                sessionLedger.SessionLedgerHandle,
                collapseReceipt.CollapseReceiptHandle,
                returnReceipt.ReturnReceiptHandle),
            CMEId: readinessLedger.CMEId,
            ReadinessLedgerHandle: readinessLedger.ReadinessLedgerHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            CollapseReceiptHandle: collapseReceipt.CollapseReceiptHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            InquiryState: inquiryState.Trim(),
            InquiryStances: inquiryStances,
            AssumptionExposureModes: assumptionExposureModes,
            SilenceDispositions: silenceDispositions,
            ChamberNativeInquiryBound: true,
            HiddenPressureDenied: true,
            PrematureGelPromotionDenied: true,
            ReasonCode: "inquiry-session-discipline-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BoundaryConditionLedgerReceipt CreateBoundaryConditionLedger(
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
        EnsurePrefix(readinessLedger.ReadinessLedgerHandle, RuntimeHabitationReadinessLedgerPrefix, nameof(readinessLedger));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(inquirySurface.InquirySurfaceHandle, InquirySessionDisciplinePrefix, nameof(inquirySurface));
        EnsurePrefix(collapseReceipt.CollapseReceiptHandle, DayDreamCollapsePrefix, nameof(collapseReceipt));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, CrypticDepthReturnPrefix, nameof(returnReceipt));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(readinessLedger.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, inquirySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, collapseReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.SessionLedgerHandle, sessionLedger.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, inquirySurface.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, collapseReceipt.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, returnReceipt.SessionLedgerHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Boundary condition ledger requires readiness, session, inquiry, collapse, and return receipts to remain inside one bounded habitation chamber.");
        }

        var retainedBoundaryConditions = sessionLedger.BoundaryConditions
            .Concat(collapseReceipt.BoundaryConditions)
            .Concat(returnReceipt.BoundaryConditions)
            .Distinct()
            .ToArray();
        var continuityRequirements = retainedBoundaryConditions
            .Select(static condition => condition.ContinuityRequirement)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var withheldCrossings = retainedBoundaryConditions
            .Select(static condition => condition.TriggerPredicate)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new BoundaryConditionLedgerReceipt(
            BoundaryLedgerHandle: SanctuaryWorkbenchKeys.CreateBoundaryConditionLedgerHandle(
                readinessLedger.CMEId,
                readinessLedger.ReadinessLedgerHandle,
                sessionLedger.SessionLedgerHandle,
                inquirySurface.InquirySurfaceHandle),
            CMEId: readinessLedger.CMEId,
            ReadinessLedgerHandle: readinessLedger.ReadinessLedgerHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            InquirySurfaceHandle: inquirySurface.InquirySurfaceHandle,
            CollapseReceiptHandle: collapseReceipt.CollapseReceiptHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            LedgerState: ledgerState.Trim(),
            RetainedBoundaryConditions: retainedBoundaryConditions,
            ContinuityRequirements: continuityRequirements,
            WithheldCrossings: withheldCrossings,
            BoundaryMemoryCarriedForward: retainedBoundaryConditions.Length > 0,
            FailurePunishmentDenied: true,
            IdentityBleedDetected: returnReceipt.IdentityBleedDetected,
            ReasonCode: "boundary-condition-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static CoherenceGainWitnessReceipt CreateCoherenceGainWitnessReceipt(
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
        EnsurePrefix(readinessLedger.ReadinessLedgerHandle, RuntimeHabitationReadinessLedgerPrefix, nameof(readinessLedger));
        EnsurePrefix(sessionLedger.SessionLedgerHandle, SessionLedgerPrefix, nameof(sessionLedger));
        EnsurePrefix(inquirySurface.InquirySurfaceHandle, InquirySessionDisciplinePrefix, nameof(inquirySurface));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessState);
        ArgumentException.ThrowIfNullOrWhiteSpace(coherenceState);

        if (!string.Equals(readinessLedger.CMEId, sessionLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, inquirySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(readinessLedger.SessionLedgerHandle, sessionLedger.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, inquirySurface.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(sessionLedger.SessionLedgerHandle, boundaryLedger.SessionLedgerHandle, StringComparison.Ordinal) ||
            !string.Equals(inquirySurface.InquirySurfaceHandle, boundaryLedger.InquirySurfaceHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Coherence gain witness requires readiness, session, inquiry, and boundary ledgers to remain inside one bounded habitation chamber.");
        }

        var coherencePreservingEventCount = sessionLedger.SessionEvents.Count(static item => item.CoherencePreserving);
        var hiddenAssumptionDeniedCount = sessionLedger.SessionEvents.Count(static item => item.HiddenAssumptionDenied);
        var boundaryConditionCount = boundaryLedger.RetainedBoundaryConditions.Count;
        var prematureClosureDetected = sessionLedger.SessionEvents.Any(static item =>
            string.Equals(item.EventKind, "closure", StringComparison.Ordinal) &&
            !item.CoherencePreserving);

        return new CoherenceGainWitnessReceipt(
            CoherenceWitnessHandle: SanctuaryWorkbenchKeys.CreateCoherenceGainWitnessReceiptHandle(
                readinessLedger.CMEId,
                readinessLedger.ReadinessLedgerHandle,
                sessionLedger.SessionLedgerHandle,
                boundaryLedger.BoundaryLedgerHandle),
            CMEId: readinessLedger.CMEId,
            ReadinessLedgerHandle: readinessLedger.ReadinessLedgerHandle,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            InquirySurfaceHandle: inquirySurface.InquirySurfaceHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            WitnessState: witnessState.Trim(),
            CoherenceState: coherenceState.Trim(),
            CoherencePreservingEventCount: coherencePreservingEventCount,
            HiddenAssumptionDeniedCount: hiddenAssumptionDeniedCount,
            BoundaryConditionCount: boundaryConditionCount,
            SharedIntelligibilityPreserved: coherencePreservingEventCount > 0 && !boundaryLedger.IdentityBleedDetected,
            AdmissibilitySpacePreserved: inquirySurface.HiddenPressureDenied && inquirySurface.PrematureGelPromotionDenied,
            PrematureClosureDetected: prematureClosureDetected,
            ReasonCode: "coherence-gain-witness-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    private static void RequireActualLocality(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.IndexOf(".actual", StringComparison.OrdinalIgnoreCase) < 0)
        {
            throw new ArgumentException($"{parameterName} must identify an `.actual` locality.", parameterName);
        }
    }

    private static void EnsurePrefix(string value, string prefix, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{parameterName} must use the `{prefix}` handle class.", parameterName);
        }
    }
}
