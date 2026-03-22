using AgentiCore.Runtime;
using Oan.Common;

namespace AgentiCore.Services;

public sealed class GovernedReachRealizationService
{
    public AgentiActualUtilitySurfaceReceipt CreateAgentiActualUtilitySurface(
        GovernedThreadBirthReceipt threadBirth,
        DuplexPredicateEnvelope envelope,
        string sanctuaryActualLocality,
        string operatorActualLocality,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadBirth);
        ArgumentNullException.ThrowIfNull(envelope);

        return AgentiActualizationProjector.CreateAgentiActualUtilitySurface(
            cmeId: threadBirth.CMEId,
            threadBirthHandle: threadBirth.ThreadBirthHandle,
            identityInvariantHandle: threadBirth.IdentityInvariantHandle,
            duplexEnvelopeId: envelope.EnvelopeId,
            workPredicate: envelope.WorkPredicate,
            governancePredicate: envelope.GovernancePredicate,
            nexusPortalHandle: threadBirth.NexusPortalHandle,
            sanctuaryActualLocality: sanctuaryActualLocality,
            operatorActualLocality: operatorActualLocality,
            witnessRequirement: envelope.WitnessRequirement,
            returnCondition: envelope.ReturnCondition,
            authorityClass: envelope.AuthorityClass,
            timestampUtc: timestampUtc);
    }

    public ReachDuplexRealizationEnvelope CreateReachDuplexRealizationEnvelope(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        string sourceLocality,
        string targetLocality,
        string accessTopologyState,
        string legibilityState,
        string witnessHandle)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessHandle);

        var bondedSpaceHandle = AgentiActualizationKeys.CreateBondedSpaceHandle(
            utilitySurface.CMEId,
            utilitySurface.SanctuaryActualLocality,
            utilitySurface.OperatorActualLocality);

        return ReachDuplexRealizationSurfaceContracts.CreateEnvelope(
            utilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            duplexEnvelopeId: utilitySurface.DuplexEnvelopeId,
            sourceLocality: sourceLocality,
            targetLocality: targetLocality,
            bondedSpaceHandle: bondedSpaceHandle,
            accessTopologyState: accessTopologyState,
            legibilityState: legibilityState,
            witnessHandle: witnessHandle,
            returnCondition: utilitySurface.ReturnCondition,
            authorityClass: utilitySurface.AuthorityClass);
    }

    public ReachDuplexRealizationReceipt CreateReachDuplexRealizationReceipt(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationEnvelope envelope,
        ReachDuplexRealizationDispatchReceipt dispatchReceipt,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(dispatchReceipt);

        return AgentiActualizationProjector.CreateReachDuplexRealizationReceipt(
            utilitySurface,
            envelope.EnvelopeId,
            envelope.SourceLocality,
            envelope.TargetLocality,
            envelope.BondedSpaceHandle,
            envelope.AccessTopologyState,
            envelope.LegibilityState,
            dispatchReceipt.DispatchState,
            timestampUtc);
    }

    public BondedParticipationLocalityLedgerReceipt CreateBondedParticipationLocalityLedger(
        GovernedThreadBirthReceipt threadBirth,
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationReceipt realization,
        IReadOnlyList<string> coRealizedSurfaces,
        IReadOnlyList<string> withheldSurfaces,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadBirth);
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(realization);

        return AgentiActualizationProjector.CreateBondedParticipationLocalityLedger(
            utilitySurface,
            realization,
            threadBirth.ThreadBirthHandle,
            coRealizedSurfaces,
            withheldSurfaces,
            timestampUtc);
    }
}
