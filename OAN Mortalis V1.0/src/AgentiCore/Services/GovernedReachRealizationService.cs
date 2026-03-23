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

    public BondedCoWorkSessionRehearsalReceipt CreateBondedCoWorkSessionRehearsal(
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

        return AgentiActualizationProjector.CreateBondedCoWorkSessionRehearsal(
            sessionLedger,
            utilitySurface,
            realization,
            localityLedger,
            sharedWorkLoop,
            duplexPredicateLanes,
            withheldLanes,
            rehearsalState,
            timestampUtc);
    }

    public ReachReturnDissolutionReceipt CreateReachReturnDissolutionReceipt(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        ReachDuplexRealizationReceipt realization,
        string returnState = "returned-through-reach",
        string dissolutionState = "dissolution-witnessed",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(realization);

        return AgentiActualizationProjector.CreateReachReturnDissolutionReceipt(
            rehearsal,
            realization,
            returnState,
            dissolutionState,
            timestampUtc);
    }

    public LocalityDistinctionWitnessLedgerReceipt CreateLocalityDistinctionWitnessLedger(
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

        return AgentiActualizationProjector.CreateLocalityDistinctionWitnessLedger(
            rehearsal,
            returnReceipt,
            sharedSurfaces,
            sanctuaryLocalSurfaces,
            operatorLocalSurfaces,
            withheldSurfaces,
            timestampUtc);
    }

    public OperatorInquirySelectionEnvelopeReceipt CreateOperatorInquirySelectionEnvelope(
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

        return AgentiActualizationProjector.CreateOperatorInquirySelectionEnvelope(
            rehearsal,
            localityWitness,
            inquirySurface,
            boundaryLedger,
            coherenceWitness,
            envelopeState,
            timestampUtc);
    }

    public BondedCrucibleSessionRehearsalReceipt CreateBondedCrucibleSessionRehearsal(
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

        return AgentiActualizationProjector.CreateBondedCrucibleSessionRehearsal(
            coWorkRehearsal,
            operatorInquiryEnvelope,
            boundaryLedger,
            coherenceWitness,
            rehearsalState,
            timestampUtc);
    }

    public SharedBoundaryMemoryLedgerReceipt CreateSharedBoundaryMemoryLedger(
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

        return AgentiActualizationProjector.CreateSharedBoundaryMemoryLedger(
            crucibleRehearsal,
            boundaryLedger,
            returnReceipt,
            localityWitness,
            ledgerState,
            timestampUtc);
    }
}
