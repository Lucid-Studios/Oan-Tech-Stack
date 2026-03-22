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

public static class AgentiActualizationProjector
{
    private const string GovernedThreadBirthPrefix = "governed-thread-birth://";
    private const string IdentityInvariantPrefix = "identity-invariant://";
    private const string DuplexEnvelopePrefix = "agenticore-duplex-envelope://";
    private const string ReachEnvelopePrefix = "reach-duplex-envelope://";
    private const string AgentiActualSurfacePrefix = "agenticore-actual-surface://";
    private const string BondedSpacePrefix = "bonded-space://";

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
