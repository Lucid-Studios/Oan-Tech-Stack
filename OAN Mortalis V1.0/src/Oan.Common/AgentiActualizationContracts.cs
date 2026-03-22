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
