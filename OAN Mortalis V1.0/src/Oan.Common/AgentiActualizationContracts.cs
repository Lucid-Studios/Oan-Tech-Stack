namespace Oan.Common;

public sealed record AgentiActualUtilitySurfaceReceipt(
    string UtilitySurfaceHandle,
    string CMEId,
    string ThreadBirthHandle,
    string IdentityInvariantHandle,
    string DuplexEnvelopeId,
    string WorkPredicate,
    string GovernancePredicate,
    string NexusPortalHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string WitnessRequirement,
    string ReturnCondition,
    string AuthorityClass,
    string UtilityPosture,
    bool SovereigntyDenied,
    bool RemoteControlDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ReachDuplexRealizationReceipt(
    string RealizationHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string DuplexEnvelopeId,
    string ReachEnvelopeId,
    string SourceLocality,
    string TargetLocality,
    string BondedSpaceHandle,
    string AccessTopologyState,
    string LegibilityState,
    string DispatchState,
    bool AccessGrantImplied,
    bool LocalityCollapseDenied,
    bool IdentityCollapseDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedParticipationLocalityLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string RealizationHandle,
    string ThreadBirthHandle,
    string BondedSpaceHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> CoRealizedSurfaces,
    IReadOnlyList<string> WithheldSurfaces,
    bool BondedParticipationProvisional,
    bool RemoteControlDenied,
    string ReturnCondition,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedCoWorkSessionRehearsalReceipt(
    string RehearsalHandle,
    string CMEId,
    string SessionLedgerHandle,
    string UtilitySurfaceHandle,
    string RealizationHandle,
    string LocalityLedgerHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> SharedWorkLoop,
    IReadOnlyList<string> DuplexPredicateLanes,
    IReadOnlyList<string> WithheldLanes,
    string RehearsalState,
    bool LocalityCollapseDenied,
    bool RemoteControlDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ReachReturnDissolutionReceipt(
    string ReturnReceiptHandle,
    string CMEId,
    string RehearsalHandle,
    string RealizationHandle,
    string SourceLocality,
    string TargetLocality,
    string ReturnState,
    string DissolutionState,
    bool BondedEventReturned,
    bool BondedEventDissolved,
    bool AmbientGrantDenied,
    bool LocalityDistinctionPreserved,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record LocalityDistinctionWitnessLedgerReceipt(
    string WitnessLedgerHandle,
    string CMEId,
    string RehearsalHandle,
    string ReturnReceiptHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> SharedSurfaces,
    IReadOnlyList<string> SanctuaryLocalSurfaces,
    IReadOnlyList<string> OperatorLocalSurfaces,
    IReadOnlyList<string> WithheldSurfaces,
    bool LocalityCollapseDetected,
    bool ProjectionTheaterDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record OperatorInquirySelectionEnvelopeReceipt(
    string EnvelopeHandle,
    string CMEId,
    string RehearsalHandle,
    string LocalityWitnessHandle,
    string InquirySurfaceHandle,
    string BoundaryLedgerHandle,
    string CoherenceWitnessHandle,
    string OperatorActualLocality,
    string EnvelopeState,
    IReadOnlyList<string> AvailableInquiryStances,
    IReadOnlyList<string> KnownBoundaryWarnings,
    IReadOnlyList<string> LawfulUseConditions,
    bool ProtectedInteriorityDenied,
    bool LocalityBypassDenied,
    bool RawGrantDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedCrucibleSessionRehearsalReceipt(
    string RehearsalHandle,
    string CMEId,
    string CoWorkRehearsalHandle,
    string OperatorInquiryEnvelopeHandle,
    string BoundaryLedgerHandle,
    string CoherenceWitnessHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string RehearsalState,
    string SharedUnknownClass,
    IReadOnlyList<string> SelectedInquiryStances,
    IReadOnlyList<string> SharedUnknownFacets,
    int CoordinationHoldCount,
    int ExposedBoundaryCount,
    bool PreScriptedAnswerDenied,
    bool RemoteDominanceDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record SharedBoundaryMemoryLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string BoundaryLedgerHandle,
    string ReturnReceiptHandle,
    string LocalityWitnessHandle,
    string LedgerState,
    IReadOnlyList<string> SharedBoundaryCodes,
    IReadOnlyList<string> SharedContinuityRequirements,
    IReadOnlyList<string> WithheldCommonPropertyClaims,
    bool LocalityProvenancePreserved,
    bool IdentityBleedDetected,
    bool AmbientCommonPropertyDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ContinuityUnderPressureLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string CoherenceWitnessHandle,
    string LedgerState,
    IReadOnlyList<string> HeldContinuities,
    IReadOnlyList<string> PartialContinuities,
    IReadOnlyList<string> RequiredPreservations,
    int BoundaryPressureCount,
    bool FluentSuccessDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ExpressiveDeformationReceipt(
    string ReceiptHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string OperatorInquiryEnvelopeHandle,
    string ContinuityLedgerHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string ReceiptState,
    string DeformationClass,
    IReadOnlyList<string> ChangedExpressions,
    IReadOnlyList<string> RecognizableContinuities,
    IReadOnlyList<string> FractureBoundaries,
    bool AdaptiveRefinementPreserved,
    bool IdentityCollapseDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record MutualIntelligibilityWitnessReceipt(
    string WitnessHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string ContinuityLedgerHandle,
    string DeformationReceiptHandle,
    string LocalityWitnessHandle,
    string WitnessState,
    string SharedUnderstandingState,
    int HeldIntelligibilityCount,
    int NarrowedIntelligibilityCount,
    int BrokenIntelligibilityCount,
    bool SamenessCollapseDenied,
    bool OpaqueDivergenceDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class AgentiActualizationProjector
{
    private const string GovernedThreadBirthPrefix = "governed-thread-birth://";
    private const string IdentityInvariantPrefix = "identity-invariant://";
    private const string DuplexEnvelopePrefix = "agenticore-duplex-envelope://";
    private const string ReachEnvelopePrefix = "reach-duplex-envelope://";
    private const string AgentiActualSurfacePrefix = "agenticore-actual-surface://";
    private const string BondedSpacePrefix = "bonded-space://";
    private const string RuntimeWorkbenchSessionPrefix = "runtime-workbench-session-ledger://";
    private const string BondedLocalityLedgerPrefix = "bonded-locality-ledger://";
    private const string ReachRealizationPrefix = "reach-duplex-realization://";
    private const string InquirySessionDisciplinePrefix = "inquiry-session-discipline-surface://";
    private const string BoundaryConditionLedgerPrefix = "boundary-condition-ledger://";
    private const string CoherenceGainWitnessPrefix = "coherence-gain-witness-receipt://";
    private const string OperatorInquirySelectionEnvelopePrefix = "operator-inquiry-selection-envelope://";
    private const string BondedCrucibleSessionRehearsalPrefix = "bonded-crucible-session-rehearsal://";
    private const string SharedBoundaryMemoryLedgerPrefix = "shared-boundary-memory-ledger://";
    private const string ContinuityUnderPressureLedgerPrefix = "continuity-under-pressure-ledger://";
    private const string ExpressiveDeformationReceiptPrefix = "expressive-deformation-receipt://";
    private const string MutualIntelligibilityWitnessPrefix = "mutual-intelligibility-witness://";

    public static AgentiActualUtilitySurfaceReceipt CreateAgentiActualUtilitySurface(
        string cmeId,
        string threadBirthHandle,
        string identityInvariantHandle,
        string duplexEnvelopeId,
        string workPredicate,
        string governancePredicate,
        string nexusPortalHandle,
        string sanctuaryActualLocality,
        string operatorActualLocality,
        string witnessRequirement,
        string returnCondition,
        string authorityClass,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        EnsurePrefix(threadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirthHandle));
        EnsurePrefix(identityInvariantHandle, IdentityInvariantPrefix, nameof(identityInvariantHandle));
        EnsurePrefix(duplexEnvelopeId, DuplexEnvelopePrefix, nameof(duplexEnvelopeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(workPredicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(governancePredicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(nexusPortalHandle);
        RequireActualLocality(sanctuaryActualLocality, nameof(sanctuaryActualLocality));
        RequireActualLocality(operatorActualLocality, nameof(operatorActualLocality));
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessRequirement);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnCondition);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorityClass);

        return new AgentiActualUtilitySurfaceReceipt(
            UtilitySurfaceHandle: AgentiActualizationKeys.CreateAgentiActualUtilitySurfaceHandle(
                cmeId,
                threadBirthHandle,
                duplexEnvelopeId,
                operatorActualLocality),
            CMEId: cmeId.Trim(),
            ThreadBirthHandle: threadBirthHandle.Trim(),
            IdentityInvariantHandle: identityInvariantHandle.Trim(),
            DuplexEnvelopeId: duplexEnvelopeId.Trim(),
            WorkPredicate: workPredicate.Trim(),
            GovernancePredicate: governancePredicate.Trim(),
            NexusPortalHandle: nexusPortalHandle.Trim(),
            SanctuaryActualLocality: sanctuaryActualLocality.Trim(),
            OperatorActualLocality: operatorActualLocality.Trim(),
            WitnessRequirement: witnessRequirement.Trim(),
            ReturnCondition: returnCondition.Trim(),
            AuthorityClass: authorityClass.Trim(),
            UtilityPosture: "governed-utility-virtualized",
            SovereigntyDenied: true,
            RemoteControlDenied: true,
            ReasonCode: "agenticore-actual-utility-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ReachDuplexRealizationReceipt CreateReachDuplexRealizationReceipt(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        string reachEnvelopeId,
        string sourceLocality,
        string targetLocality,
        string bondedSpaceHandle,
        string accessTopologyState,
        string legibilityState,
        string dispatchState,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        EnsurePrefix(utilitySurface.UtilitySurfaceHandle, AgentiActualSurfacePrefix, nameof(utilitySurface));
        EnsurePrefix(reachEnvelopeId, ReachEnvelopePrefix, nameof(reachEnvelopeId));
        RequireActualLocality(sourceLocality, nameof(sourceLocality));
        RequireActualLocality(targetLocality, nameof(targetLocality));
        EnsurePrefix(bondedSpaceHandle, BondedSpacePrefix, nameof(bondedSpaceHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(accessTopologyState);
        ArgumentException.ThrowIfNullOrWhiteSpace(legibilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispatchState);

        var normalizedDispatchState = dispatchState.Trim();
        var reasonCode = string.Equals(normalizedDispatchState, "accepted", StringComparison.OrdinalIgnoreCase)
            ? "reach-duplex-realization-dispatched"
            : "reach-duplex-realization-withheld";

        return new ReachDuplexRealizationReceipt(
            RealizationHandle: AgentiActualizationKeys.CreateReachDuplexRealizationHandle(
                utilitySurface.CMEId,
                utilitySurface.UtilitySurfaceHandle,
                reachEnvelopeId,
                targetLocality),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            DuplexEnvelopeId: utilitySurface.DuplexEnvelopeId,
            ReachEnvelopeId: reachEnvelopeId.Trim(),
            SourceLocality: sourceLocality.Trim(),
            TargetLocality: targetLocality.Trim(),
            BondedSpaceHandle: bondedSpaceHandle.Trim(),
            AccessTopologyState: accessTopologyState.Trim(),
            LegibilityState: legibilityState.Trim(),
            DispatchState: normalizedDispatchState,
            AccessGrantImplied: false,
            LocalityCollapseDenied: true,
            IdentityCollapseDenied: true,
            ReasonCode: reasonCode,
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedParticipationLocalityLedgerReceipt CreateBondedParticipationLocalityLedger(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationReceipt realization,
        string threadBirthHandle,
        IReadOnlyList<string> coRealizedSurfaces,
        IReadOnlyList<string> withheldSurfaces,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(realization);
        EnsurePrefix(threadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirthHandle));
        ArgumentNullException.ThrowIfNull(coRealizedSurfaces);
        ArgumentNullException.ThrowIfNull(withheldSurfaces);

        if (!string.Equals(utilitySurface.CMEId, realization.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded locality ledger requires utility and reach realization to remain inside the same CME continuity surface.");
        }

        return new BondedParticipationLocalityLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateBondedParticipationLocalityLedgerHandle(
                utilitySurface.CMEId,
                realization.RealizationHandle,
                utilitySurface.SanctuaryActualLocality,
                utilitySurface.OperatorActualLocality),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            RealizationHandle: realization.RealizationHandle,
            ThreadBirthHandle: threadBirthHandle.Trim(),
            BondedSpaceHandle: realization.BondedSpaceHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            CoRealizedSurfaces: (coRealizedSurfaces ?? Array.Empty<string>()).ToArray(),
            WithheldSurfaces: (withheldSurfaces ?? Array.Empty<string>()).ToArray(),
            BondedParticipationProvisional: true,
            RemoteControlDenied: true,
            ReturnCondition: utilitySurface.ReturnCondition,
            ReasonCode: "bonded-participation-locality-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedCoWorkSessionRehearsalReceipt CreateBondedCoWorkSessionRehearsal(
        RuntimeWorkbenchSessionLedger sessionLedger,
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationReceipt realization,
        BondedParticipationLocalityLedgerReceipt localityLedger,
        IReadOnlyList<string> sharedWorkLoop,
        IReadOnlyList<string> duplexPredicateLanes,
        IReadOnlyList<string> withheldLanes,
        string rehearsalState = "bounded-cowork-rehearsal-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(realization);
        ArgumentNullException.ThrowIfNull(localityLedger);
        EnsurePrefix(sessionLedger.SessionLedgerHandle, RuntimeWorkbenchSessionPrefix, nameof(sessionLedger));
        EnsurePrefix(realization.RealizationHandle, ReachRealizationPrefix, nameof(realization));
        EnsurePrefix(localityLedger.LedgerHandle, BondedLocalityLedgerPrefix, nameof(localityLedger));
        ArgumentNullException.ThrowIfNull(sharedWorkLoop);
        ArgumentNullException.ThrowIfNull(duplexPredicateLanes);
        ArgumentNullException.ThrowIfNull(withheldLanes);
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalState);

        if (!string.Equals(sessionLedger.CMEId, utilitySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.CMEId, realization.CMEId, StringComparison.Ordinal) ||
            !string.Equals(realization.CMEId, localityLedger.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded co-work rehearsal requires session, utility, reach, and locality receipts to remain inside one CME continuity surface.");
        }

        return new BondedCoWorkSessionRehearsalReceipt(
            RehearsalHandle: AgentiActualizationKeys.CreateBondedCoWorkSessionRehearsalHandle(
                sessionLedger.CMEId,
                sessionLedger.SessionLedgerHandle,
                realization.RealizationHandle,
                localityLedger.LedgerHandle),
            CMEId: sessionLedger.CMEId,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            RealizationHandle: realization.RealizationHandle,
            LocalityLedgerHandle: localityLedger.LedgerHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            SharedWorkLoop: (sharedWorkLoop ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            DuplexPredicateLanes: (duplexPredicateLanes ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            WithheldLanes: (withheldLanes ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            RehearsalState: rehearsalState.Trim(),
            LocalityCollapseDenied: true,
            RemoteControlDenied: true,
            ReasonCode: "bonded-cowork-session-rehearsal-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ReachReturnDissolutionReceipt CreateReachReturnDissolutionReceipt(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        ReachDuplexRealizationReceipt realization,
        string returnState = "returned-through-reach",
        string dissolutionState = "dissolution-witnessed",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(realization);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(realization.RealizationHandle, ReachRealizationPrefix, nameof(realization));
        ArgumentException.ThrowIfNullOrWhiteSpace(returnState);
        ArgumentException.ThrowIfNullOrWhiteSpace(dissolutionState);

        if (!string.Equals(rehearsal.CMEId, realization.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.RealizationHandle, realization.RealizationHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Reach return dissolution requires bonded co-work and realization receipts to remain inside one realized bonded event.");
        }

        var normalizedReturnState = returnState.Trim();
        var normalizedDissolutionState = dissolutionState.Trim();
        var bondedEventReturned = normalizedReturnState.Contains("returned", StringComparison.OrdinalIgnoreCase);
        var bondedEventDissolved = normalizedDissolutionState.Contains("dissolution", StringComparison.OrdinalIgnoreCase);

        return new ReachReturnDissolutionReceipt(
            ReturnReceiptHandle: AgentiActualizationKeys.CreateReachReturnDissolutionReceiptHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                normalizedReturnState,
                normalizedDissolutionState),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            RealizationHandle: realization.RealizationHandle,
            SourceLocality: realization.SourceLocality,
            TargetLocality: realization.TargetLocality,
            ReturnState: normalizedReturnState,
            DissolutionState: normalizedDissolutionState,
            BondedEventReturned: bondedEventReturned,
            BondedEventDissolved: bondedEventDissolved,
            AmbientGrantDenied: true,
            LocalityDistinctionPreserved: rehearsal.LocalityCollapseDenied && realization.LocalityCollapseDenied,
            ReasonCode: "reach-return-dissolution-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static LocalityDistinctionWitnessLedgerReceipt CreateLocalityDistinctionWitnessLedger(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        ReachReturnDissolutionReceipt returnReceipt,
        IReadOnlyList<string> sharedSurfaces,
        IReadOnlyList<string> sanctuaryLocalSurfaces,
        IReadOnlyList<string> operatorLocalSurfaces,
        IReadOnlyList<string> withheldSurfaces,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(returnReceipt);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, "reach-return-dissolution://", nameof(returnReceipt));
        ArgumentNullException.ThrowIfNull(sharedSurfaces);
        ArgumentNullException.ThrowIfNull(sanctuaryLocalSurfaces);
        ArgumentNullException.ThrowIfNull(operatorLocalSurfaces);
        ArgumentNullException.ThrowIfNull(withheldSurfaces);

        if (!string.Equals(rehearsal.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.RehearsalHandle, returnReceipt.RehearsalHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Locality distinction witness requires rehearsal and return receipts to remain inside one bonded co-work event.");
        }

        var normalizedSharedSurfaces = (sharedSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedSanctuaryLocalSurfaces = (sanctuaryLocalSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedOperatorLocalSurfaces = (operatorLocalSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedWithheldSurfaces = (withheldSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var localityCollapseDetected = normalizedSanctuaryLocalSurfaces.Intersect(normalizedOperatorLocalSurfaces, StringComparer.Ordinal).Any();

        return new LocalityDistinctionWitnessLedgerReceipt(
            WitnessLedgerHandle: AgentiActualizationKeys.CreateLocalityDistinctionWitnessLedgerHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                returnReceipt.ReturnReceiptHandle),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            SanctuaryActualLocality: rehearsal.SanctuaryActualLocality,
            OperatorActualLocality: rehearsal.OperatorActualLocality,
            SharedSurfaces: normalizedSharedSurfaces,
            SanctuaryLocalSurfaces: normalizedSanctuaryLocalSurfaces,
            OperatorLocalSurfaces: normalizedOperatorLocalSurfaces,
            WithheldSurfaces: normalizedWithheldSurfaces,
            LocalityCollapseDetected: localityCollapseDetected,
            ProjectionTheaterDenied: !localityCollapseDetected,
            ReasonCode: "locality-distinction-witness-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static OperatorInquirySelectionEnvelopeReceipt CreateOperatorInquirySelectionEnvelope(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        InquirySessionDisciplineSurfaceReceipt inquirySurface,
        BoundaryConditionLedgerReceipt boundaryLedger,
        CoherenceGainWitnessReceipt coherenceWitness,
        string envelopeState = "operator-inquiry-selection-envelope-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(localityWitness);
        ArgumentNullException.ThrowIfNull(inquirySurface);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        EnsurePrefix(inquirySurface.InquirySurfaceHandle, InquirySessionDisciplinePrefix, nameof(inquirySurface));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(envelopeState);

        if (!string.Equals(rehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, inquirySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Operator inquiry selection requires rehearsal, locality witness, inquiry, boundary, and coherence receipts to remain inside one bonded CME surface.");
        }

        var availableInquiryStances = inquirySurface.InquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var knownBoundaryWarnings = boundaryLedger.RetainedBoundaryConditions
            .Select(static boundary => boundary.BoundaryCode)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var lawfulUseConditions = boundaryLedger.ContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new OperatorInquirySelectionEnvelopeReceipt(
            EnvelopeHandle: AgentiActualizationKeys.CreateOperatorInquirySelectionEnvelopeHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                inquirySurface.InquirySurfaceHandle,
                rehearsal.OperatorActualLocality),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            InquirySurfaceHandle: inquirySurface.InquirySurfaceHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            OperatorActualLocality: rehearsal.OperatorActualLocality,
            EnvelopeState: envelopeState.Trim(),
            AvailableInquiryStances: availableInquiryStances,
            KnownBoundaryWarnings: knownBoundaryWarnings,
            LawfulUseConditions: lawfulUseConditions,
            ProtectedInteriorityDenied: true,
            LocalityBypassDenied: !localityWitness.LocalityCollapseDetected,
            RawGrantDenied: true,
            ReasonCode: "operator-inquiry-selection-envelope-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedCrucibleSessionRehearsalReceipt CreateBondedCrucibleSessionRehearsal(
        BondedCoWorkSessionRehearsalReceipt coWorkRehearsal,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        BoundaryConditionLedgerReceipt boundaryLedger,
        CoherenceGainWitnessReceipt coherenceWitness,
        string rehearsalState = "bonded-crucible-session-rehearsal-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(coWorkRehearsal);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(coWorkRehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(coWorkRehearsal));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalState);

        if (!string.Equals(coWorkRehearsal.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(coWorkRehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(coWorkRehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded crucible rehearsal requires co-work, operator inquiry, boundary, and coherence receipts to remain inside one bonded CME surface.");
        }

        var selectedInquiryStances = operatorInquiryEnvelope.AvailableInquiryStances
            .Take(3)
            .ToArray();
        var sharedUnknownFacets = new[]
        {
            "partial-information",
            "assumption-reversal",
            "boundary-pressure"
        };

        return new BondedCrucibleSessionRehearsalReceipt(
            RehearsalHandle: AgentiActualizationKeys.CreateBondedCrucibleSessionRehearsalHandle(
                coWorkRehearsal.CMEId,
                coWorkRehearsal.RehearsalHandle,
                operatorInquiryEnvelope.EnvelopeHandle,
                boundaryLedger.BoundaryLedgerHandle),
            CMEId: coWorkRehearsal.CMEId,
            CoWorkRehearsalHandle: coWorkRehearsal.RehearsalHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            SanctuaryActualLocality: coWorkRehearsal.SanctuaryActualLocality,
            OperatorActualLocality: coWorkRehearsal.OperatorActualLocality,
            RehearsalState: rehearsalState.Trim(),
            SharedUnknownClass: "shared-uncertainty-bounded-crucible",
            SelectedInquiryStances: selectedInquiryStances,
            SharedUnknownFacets: sharedUnknownFacets,
            CoordinationHoldCount: coherenceWitness.CoherencePreservingEventCount,
            ExposedBoundaryCount: boundaryLedger.RetainedBoundaryConditions.Count,
            PreScriptedAnswerDenied: true,
            RemoteDominanceDenied: coWorkRehearsal.RemoteControlDenied && operatorInquiryEnvelope.RawGrantDenied,
            ReasonCode: "bonded-crucible-session-rehearsal-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static SharedBoundaryMemoryLedgerReceipt CreateSharedBoundaryMemoryLedger(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        BoundaryConditionLedgerReceipt boundaryLedger,
        ReachReturnDissolutionReceipt returnReceipt,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string ledgerState = "shared-boundary-memory-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(returnReceipt);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, "reach-return-dissolution://", nameof(returnReceipt));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(crucibleRehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Shared boundary memory requires crucible, boundary, return, and locality receipts to remain inside one bonded CME surface.");
        }

        var sharedBoundaryCodes = boundaryLedger.RetainedBoundaryConditions
            .Select(static boundary => boundary.BoundaryCode)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sharedContinuityRequirements = boundaryLedger.ContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var withheldCommonPropertyClaims = new[]
        {
            "ambient-shared-interiority",
            "identity-collapse",
            "sovereign-cross-grant"
        };
        var localityProvenancePreserved = !localityWitness.LocalityCollapseDetected && returnReceipt.LocalityDistinctionPreserved;

        return new SharedBoundaryMemoryLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateSharedBoundaryMemoryLedgerHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                boundaryLedger.BoundaryLedgerHandle,
                returnReceipt.ReturnReceiptHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            LedgerState: ledgerState.Trim(),
            SharedBoundaryCodes: sharedBoundaryCodes,
            SharedContinuityRequirements: sharedContinuityRequirements,
            WithheldCommonPropertyClaims: withheldCommonPropertyClaims,
            LocalityProvenancePreserved: localityProvenancePreserved,
            IdentityBleedDetected: boundaryLedger.IdentityBleedDetected,
            AmbientCommonPropertyDenied: true,
            ReasonCode: "shared-boundary-memory-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ContinuityUnderPressureLedgerReceipt CreateContinuityUnderPressureLedger(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        CoherenceGainWitnessReceipt coherenceWitness,
        string ledgerState = "continuity-under-pressure-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(crucibleRehearsal.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Continuity under pressure requires crucible, shared-boundary-memory, and coherence receipts to remain inside one bonded CME surface.");
        }

        var heldContinuities = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var partialContinuities = crucibleRehearsal.SelectedInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Select(static item => $"{item}-under-pressure")
            .Take(3)
            .ToArray();
        var requiredPreservations = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new ContinuityUnderPressureLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateContinuityUnderPressureLedgerHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                sharedBoundaryMemory.LedgerHandle,
                coherenceWitness.CoherenceWitnessHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            LedgerState: ledgerState.Trim(),
            HeldContinuities: heldContinuities,
            PartialContinuities: partialContinuities,
            RequiredPreservations: requiredPreservations,
            BoundaryPressureCount: crucibleRehearsal.ExposedBoundaryCount,
            FluentSuccessDenied: true,
            ReasonCode: "continuity-under-pressure-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ExpressiveDeformationReceipt CreateExpressiveDeformationReceipt(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string receiptState = "expressive-deformation-receipt-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        ArgumentException.ThrowIfNullOrWhiteSpace(receiptState);

        if (!string.Equals(crucibleRehearsal.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Expressive deformation requires crucible, operator inquiry, continuity, and shared-boundary-memory receipts to remain inside one bonded CME surface.");
        }

        var changedExpressions = operatorInquiryEnvelope.AvailableInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Select(static item => $"{item}-under-pressure")
            .Take(3)
            .ToArray();
        var recognizableContinuities = continuityLedger.HeldContinuities
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var fractureBoundaries = sharedBoundaryMemory.SharedBoundaryCodes
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new ExpressiveDeformationReceipt(
            ReceiptHandle: AgentiActualizationKeys.CreateExpressiveDeformationReceiptHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                operatorInquiryEnvelope.EnvelopeHandle,
                continuityLedger.LedgerHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            ReceiptState: receiptState.Trim(),
            DeformationClass: "adaptive-refinement-with-bounded-strain",
            ChangedExpressions: changedExpressions,
            RecognizableContinuities: recognizableContinuities,
            FractureBoundaries: fractureBoundaries,
            AdaptiveRefinementPreserved: true,
            IdentityCollapseDetected: sharedBoundaryMemory.IdentityBleedDetected,
            ReasonCode: "expressive-deformation-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static MutualIntelligibilityWitnessReceipt CreateMutualIntelligibilityWitness(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        ExpressiveDeformationReceipt deformationReceipt,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string witnessState = "mutual-intelligibility-witness-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(deformationReceipt);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(deformationReceipt.ReceiptHandle, ExpressiveDeformationReceiptPrefix, nameof(deformationReceipt));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessState);

        if (!string.Equals(crucibleRehearsal.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, deformationReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Mutual intelligibility witness requires crucible, continuity, deformation, and locality receipts to remain inside one bonded CME surface.");
        }

        return new MutualIntelligibilityWitnessReceipt(
            WitnessHandle: AgentiActualizationKeys.CreateMutualIntelligibilityWitnessHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                continuityLedger.LedgerHandle,
                deformationReceipt.ReceiptHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            DeformationReceiptHandle: deformationReceipt.ReceiptHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            WitnessState: witnessState.Trim(),
            SharedUnderstandingState: "mutual-intelligibility-preserved",
            HeldIntelligibilityCount: continuityLedger.HeldContinuities.Count,
            NarrowedIntelligibilityCount: deformationReceipt.RecognizableContinuities.Count,
            BrokenIntelligibilityCount: deformationReceipt.FractureBoundaries.Count,
            SamenessCollapseDenied: !localityWitness.LocalityCollapseDetected,
            OpaqueDivergenceDetected: false,
            ReasonCode: "mutual-intelligibility-witness-bound",
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
