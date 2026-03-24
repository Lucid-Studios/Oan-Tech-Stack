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

    public ContinuityUnderPressureLedgerReceipt CreateContinuityUnderPressureLedger(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        CoherenceGainWitnessReceipt coherenceWitness,
        string ledgerState = "continuity-under-pressure-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        ArgumentNullException.ThrowIfNull(coherenceWitness);

        return AgentiActualizationProjector.CreateContinuityUnderPressureLedger(
            crucibleRehearsal,
            sharedBoundaryMemory,
            coherenceWitness,
            ledgerState,
            timestampUtc);
    }

    public ExpressiveDeformationReceipt CreateExpressiveDeformationReceipt(
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

        return AgentiActualizationProjector.CreateExpressiveDeformationReceipt(
            crucibleRehearsal,
            operatorInquiryEnvelope,
            continuityLedger,
            sharedBoundaryMemory,
            receiptState,
            timestampUtc);
    }

    public MutualIntelligibilityWitnessReceipt CreateMutualIntelligibilityWitness(
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

        return AgentiActualizationProjector.CreateMutualIntelligibilityWitness(
            crucibleRehearsal,
            continuityLedger,
            deformationReceipt,
            localityWitness,
            witnessState,
            timestampUtc);
    }

    public InquiryPatternContinuityLedgerReceipt CreateInquiryPatternContinuityLedger(
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string ledgerState = "inquiry-pattern-continuity-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);

        return AgentiActualizationProjector.CreateInquiryPatternContinuityLedger(
            operatorInquiryEnvelope,
            continuityLedger,
            mutualIntelligibilityWitness,
            sharedBoundaryMemory,
            ledgerState,
            timestampUtc);
    }

    public QuestioningBoundaryPairLedgerReceipt CreateQuestioningBoundaryPairLedger(
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        ExpressiveDeformationReceipt deformationReceipt,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string ledgerState = "questioning-boundary-pair-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(deformationReceipt);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);

        return AgentiActualizationProjector.CreateQuestioningBoundaryPairLedger(
            operatorInquiryEnvelope,
            continuityLedger,
            deformationReceipt,
            sharedBoundaryMemory,
            ledgerState,
            timestampUtc);
    }

    public CarryForwardInquirySelectionSurfaceReceipt CreateCarryForwardInquirySelectionSurface(
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string surfaceState = "carry-forward-inquiry-selection-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(localityWitness);

        return AgentiActualizationProjector.CreateCarryForwardInquirySelectionSurface(
            inquiryPatternLedger,
            boundaryPairLedger,
            operatorInquiryEnvelope,
            localityWitness,
            surfaceState,
            timestampUtc);
    }

    public EngramDistanceClassificationLedgerReceipt CreateEngramDistanceClassificationLedger(
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        string ledgerState = "engram-distance-classification-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);

        return AgentiActualizationProjector.CreateEngramDistanceClassificationLedger(
            carryForwardInquirySelectionSurface,
            inquiryPatternLedger,
            boundaryPairLedger,
            continuityLedger,
            mutualIntelligibilityWitness,
            ledgerState,
            timestampUtc);
    }

    public EngramPromotionRequirementsMatrixReceipt CreateEngramPromotionRequirementsMatrix(
        EngramDistanceClassificationLedgerReceipt classificationLedger,
        string matrixState = "engram-promotion-requirements-matrix-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(classificationLedger);

        return AgentiActualizationProjector.CreateEngramPromotionRequirementsMatrix(
            classificationLedger,
            matrixState,
            timestampUtc);
    }

    public DistanceWeightedQuestioningAdmissionSurfaceReceipt CreateDistanceWeightedQuestioningAdmissionSurface(
        EngramDistanceClassificationLedgerReceipt classificationLedger,
        EngramPromotionRequirementsMatrixReceipt promotionRequirementsMatrix,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        string surfaceState = "distance-weighted-questioning-admission-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(classificationLedger);
        ArgumentNullException.ThrowIfNull(promotionRequirementsMatrix);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);

        return AgentiActualizationProjector.CreateDistanceWeightedQuestioningAdmissionSurface(
            classificationLedger,
            promotionRequirementsMatrix,
            carryForwardInquirySelectionSurface,
            surfaceState,
            timestampUtc);
    }

    public QuestioningOperatorCandidateLedgerReceipt CreateQuestioningOperatorCandidateLedger(
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        string ledgerState = "questioning-operator-candidate-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);

        return AgentiActualizationProjector.CreateQuestioningOperatorCandidateLedger(
            carryForwardInquirySelectionSurface,
            inquiryPatternLedger,
            boundaryPairLedger,
            continuityLedger,
            mutualIntelligibilityWitness,
            distanceWeightedAdmissionSurface,
            ledgerState,
            timestampUtc);
    }

    public QuestioningGelPromotionGateReceipt CreateQuestioningGelPromotionGate(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        string gateState = "questioning-gel-promotion-gate-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(localityWitness);
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);

        return AgentiActualizationProjector.CreateQuestioningGelPromotionGate(
            candidateLedger,
            carryForwardInquirySelectionSurface,
            operatorInquiryEnvelope,
            localityWitness,
            distanceWeightedAdmissionSurface,
            gateState,
            timestampUtc);
    }

    public ProtectedQuestioningPatternSurfaceReceipt CreateProtectedQuestioningPatternSurface(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string surfaceState = "protected-questioning-pattern-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(localityWitness);

        return AgentiActualizationProjector.CreateProtectedQuestioningPatternSurface(
            candidateLedger,
            promotionGate,
            carryForwardInquirySelectionSurface,
            localityWitness,
            surfaceState,
            timestampUtc);
    }

    public VariationTestedReentryLedgerReceipt CreateVariationTestedReentryLedger(
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        ProtectedQuestioningPatternSurfaceReceipt protectedPatternSurface,
        string ledgerState = "variation-tested-reentry-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(protectedPatternSurface);

        return AgentiActualizationProjector.CreateVariationTestedReentryLedger(
            distanceWeightedAdmissionSurface,
            candidateLedger,
            promotionGate,
            protectedPatternSurface,
            ledgerState,
            timestampUtc);
    }

    public QuestioningAdmissionRefusalReceipt CreateQuestioningAdmissionRefusalReceipt(
        VariationTestedReentryLedgerReceipt variationTestedReentryLedger,
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        ProtectedQuestioningPatternSurfaceReceipt protectedPatternSurface,
        string receiptState = "questioning-admission-refusal-receipt-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(variationTestedReentryLedger);
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(protectedPatternSurface);

        return AgentiActualizationProjector.CreateQuestioningAdmissionRefusalReceipt(
            variationTestedReentryLedger,
            candidateLedger,
            promotionGate,
            protectedPatternSurface,
            receiptState,
            timestampUtc);
    }

    public PromotionSeductionWatchReceipt CreatePromotionSeductionWatch(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        VariationTestedReentryLedgerReceipt variationTestedReentryLedger,
        QuestioningAdmissionRefusalReceipt admissionRefusalReceipt,
        string watchState = "promotion-seduction-watch-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(variationTestedReentryLedger);
        ArgumentNullException.ThrowIfNull(admissionRefusalReceipt);

        return AgentiActualizationProjector.CreatePromotionSeductionWatch(
            candidateLedger,
            promotionGate,
            variationTestedReentryLedger,
            admissionRefusalReceipt,
            watchState,
            timestampUtc);
    }
}
