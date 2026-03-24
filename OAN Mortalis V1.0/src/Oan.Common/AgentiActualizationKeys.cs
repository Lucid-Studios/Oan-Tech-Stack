using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class AgentiActualizationKeys
{
    public static string CreateAgentiActualUtilitySurfaceHandle(
        string cmeId,
        string threadBirthHandle,
        string duplexEnvelopeId,
        string operatorActualLocality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(threadBirthHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(duplexEnvelopeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorActualLocality);

        return $"agenticore-actual-surface://{ComputeDigest(cmeId, threadBirthHandle, duplexEnvelopeId, operatorActualLocality)}";
    }

    public static string CreateBondedSpaceHandle(
        string cmeId,
        string sanctuaryActualLocality,
        string operatorActualLocality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sanctuaryActualLocality);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorActualLocality);

        return $"bonded-space://{ComputeDigest(cmeId, sanctuaryActualLocality, operatorActualLocality)}";
    }

    public static string CreateReachDuplexRealizationHandle(
        string cmeId,
        string utilitySurfaceHandle,
        string reachEnvelopeId,
        string targetLocality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(utilitySurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(reachEnvelopeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLocality);

        return $"reach-duplex-realization://{ComputeDigest(cmeId, utilitySurfaceHandle, reachEnvelopeId, targetLocality)}";
    }

    public static string CreateBondedParticipationLocalityLedgerHandle(
        string cmeId,
        string realizationHandle,
        string sanctuaryActualLocality,
        string operatorActualLocality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(realizationHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(sanctuaryActualLocality);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorActualLocality);

        return $"bonded-locality-ledger://{ComputeDigest(cmeId, realizationHandle, sanctuaryActualLocality, operatorActualLocality)}";
    }

    public static string CreateBondedCoWorkSessionRehearsalHandle(
        string cmeId,
        string sessionLedgerHandle,
        string realizationHandle,
        string localityLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(realizationHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(localityLedgerHandle);

        return $"bonded-cowork-session-rehearsal://{ComputeDigest(cmeId, sessionLedgerHandle, realizationHandle, localityLedgerHandle)}";
    }

    public static string CreateReachReturnDissolutionReceiptHandle(
        string cmeId,
        string rehearsalHandle,
        string returnState,
        string dissolutionState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnState);
        ArgumentException.ThrowIfNullOrWhiteSpace(dissolutionState);

        return $"reach-return-dissolution://{ComputeDigest(cmeId, rehearsalHandle, returnState, dissolutionState)}";
    }

    public static string CreateLocalityDistinctionWitnessLedgerHandle(
        string cmeId,
        string rehearsalHandle,
        string returnReceiptHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnReceiptHandle);

        return $"locality-distinction-witness-ledger://{ComputeDigest(cmeId, rehearsalHandle, returnReceiptHandle)}";
    }

    public static string CreateOperatorInquirySelectionEnvelopeHandle(
        string cmeId,
        string rehearsalHandle,
        string inquirySurfaceHandle,
        string operatorActualLocality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(inquirySurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorActualLocality);

        return $"operator-inquiry-selection-envelope://{ComputeDigest(cmeId, rehearsalHandle, inquirySurfaceHandle, operatorActualLocality)}";
    }

    public static string CreateBondedCrucibleSessionRehearsalHandle(
        string cmeId,
        string coWorkRehearsalHandle,
        string operatorInquiryEnvelopeHandle,
        string boundaryLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(coWorkRehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorInquiryEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryLedgerHandle);

        return $"bonded-crucible-session-rehearsal://{ComputeDigest(cmeId, coWorkRehearsalHandle, operatorInquiryEnvelopeHandle, boundaryLedgerHandle)}";
    }

    public static string CreateSharedBoundaryMemoryLedgerHandle(
        string cmeId,
        string crucibleRehearsalHandle,
        string boundaryLedgerHandle,
        string returnReceiptHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(crucibleRehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnReceiptHandle);

        return $"shared-boundary-memory-ledger://{ComputeDigest(cmeId, crucibleRehearsalHandle, boundaryLedgerHandle, returnReceiptHandle)}";
    }

    public static string CreateContinuityUnderPressureLedgerHandle(
        string cmeId,
        string crucibleRehearsalHandle,
        string sharedBoundaryMemoryLedgerHandle,
        string coherenceWitnessHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(crucibleRehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(sharedBoundaryMemoryLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(coherenceWitnessHandle);

        return $"continuity-under-pressure-ledger://{ComputeDigest(cmeId, crucibleRehearsalHandle, sharedBoundaryMemoryLedgerHandle, coherenceWitnessHandle)}";
    }

    public static string CreateExpressiveDeformationReceiptHandle(
        string cmeId,
        string crucibleRehearsalHandle,
        string operatorInquiryEnvelopeHandle,
        string continuityLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(crucibleRehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorInquiryEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);

        return $"expressive-deformation-receipt://{ComputeDigest(cmeId, crucibleRehearsalHandle, operatorInquiryEnvelopeHandle, continuityLedgerHandle)}";
    }

    public static string CreateMutualIntelligibilityWitnessHandle(
        string cmeId,
        string crucibleRehearsalHandle,
        string continuityLedgerHandle,
        string deformationReceiptHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(crucibleRehearsalHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(deformationReceiptHandle);

        return $"mutual-intelligibility-witness://{ComputeDigest(cmeId, crucibleRehearsalHandle, continuityLedgerHandle, deformationReceiptHandle)}";
    }

    public static string CreateInquiryPatternContinuityLedgerHandle(
        string cmeId,
        string operatorInquiryEnvelopeHandle,
        string continuityLedgerHandle,
        string mutualWitnessHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorInquiryEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(mutualWitnessHandle);

        return $"inquiry-pattern-continuity-ledger://{ComputeDigest(cmeId, operatorInquiryEnvelopeHandle, continuityLedgerHandle, mutualWitnessHandle)}";
    }

    public static string CreateQuestioningBoundaryPairLedgerHandle(
        string cmeId,
        string operatorInquiryEnvelopeHandle,
        string continuityLedgerHandle,
        string deformationReceiptHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorInquiryEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(deformationReceiptHandle);

        return $"questioning-boundary-pair-ledger://{ComputeDigest(cmeId, operatorInquiryEnvelopeHandle, continuityLedgerHandle, deformationReceiptHandle)}";
    }

    public static string CreateCarryForwardInquirySelectionSurfaceHandle(
        string cmeId,
        string inquiryPatternLedgerHandle,
        string boundaryPairLedgerHandle,
        string localityWitnessHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(inquiryPatternLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryPairLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(localityWitnessHandle);

        return $"carry-forward-inquiry-selection-surface://{ComputeDigest(cmeId, inquiryPatternLedgerHandle, boundaryPairLedgerHandle, localityWitnessHandle)}";
    }

    public static string CreateEngramDistanceClassificationLedgerHandle(
        string cmeId,
        string carryForwardInquirySelectionSurfaceHandle,
        string inquiryPatternLedgerHandle,
        string continuityLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(carryForwardInquirySelectionSurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(inquiryPatternLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);

        return $"engram-distance-classification-ledger://{ComputeDigest(cmeId, carryForwardInquirySelectionSurfaceHandle, inquiryPatternLedgerHandle, continuityLedgerHandle)}";
    }

    public static string CreateEngramPromotionRequirementsMatrixHandle(
        string cmeId,
        string classificationLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(classificationLedgerHandle);

        return $"engram-promotion-requirements-matrix://{ComputeDigest(cmeId, classificationLedgerHandle)}";
    }

    public static string CreateDistanceWeightedQuestioningAdmissionSurfaceHandle(
        string cmeId,
        string classificationLedgerHandle,
        string promotionRequirementsMatrixHandle,
        string carryForwardInquirySelectionSurfaceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(classificationLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(promotionRequirementsMatrixHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(carryForwardInquirySelectionSurfaceHandle);

        return $"distance-weighted-questioning-admission-surface://{ComputeDigest(cmeId, classificationLedgerHandle, promotionRequirementsMatrixHandle, carryForwardInquirySelectionSurfaceHandle)}";
    }

    public static string CreateQuestioningOperatorCandidateLedgerHandle(
        string cmeId,
        string carryForwardInquirySelectionSurfaceHandle,
        string continuityLedgerHandle,
        string mutualWitnessHandle,
        string distanceWeightedAdmissionSurfaceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(carryForwardInquirySelectionSurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(continuityLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(mutualWitnessHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(distanceWeightedAdmissionSurfaceHandle);

        return $"questioning-operator-candidate-ledger://{ComputeDigest(cmeId, carryForwardInquirySelectionSurfaceHandle, continuityLedgerHandle, mutualWitnessHandle, distanceWeightedAdmissionSurfaceHandle)}";
    }

    public static string CreateQuestioningGelPromotionGateHandle(
        string cmeId,
        string candidateLedgerHandle,
        string operatorInquiryEnvelopeHandle,
        string localityWitnessHandle,
        string distanceWeightedAdmissionSurfaceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorInquiryEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(localityWitnessHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(distanceWeightedAdmissionSurfaceHandle);

        return $"questioning-gel-promotion-gate://{ComputeDigest(cmeId, candidateLedgerHandle, operatorInquiryEnvelopeHandle, localityWitnessHandle, distanceWeightedAdmissionSurfaceHandle)}";
    }

    public static string CreateProtectedQuestioningPatternSurfaceHandle(
        string cmeId,
        string candidateLedgerHandle,
        string promotionGateHandle,
        string localityWitnessHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(promotionGateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(localityWitnessHandle);

        return $"protected-questioning-pattern-surface://{ComputeDigest(cmeId, candidateLedgerHandle, promotionGateHandle, localityWitnessHandle)}";
    }

    public static string CreateVariationTestedReentryLedgerHandle(
        string cmeId,
        string distanceWeightedAdmissionSurfaceHandle,
        string promotionGateHandle,
        string protectedPatternSurfaceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(distanceWeightedAdmissionSurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(promotionGateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedPatternSurfaceHandle);

        return $"variation-tested-reentry-ledger://{ComputeDigest(cmeId, distanceWeightedAdmissionSurfaceHandle, promotionGateHandle, protectedPatternSurfaceHandle)}";
    }

    public static string CreateQuestioningAdmissionRefusalReceiptHandle(
        string cmeId,
        string variationTestedReentryLedgerHandle,
        string promotionGateHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(variationTestedReentryLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(promotionGateHandle);

        return $"questioning-admission-refusal-receipt://{ComputeDigest(cmeId, variationTestedReentryLedgerHandle, promotionGateHandle)}";
    }

    public static string CreatePromotionSeductionWatchHandle(
        string cmeId,
        string candidateLedgerHandle,
        string variationTestedReentryLedgerHandle,
        string admissionRefusalReceiptHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(variationTestedReentryLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissionRefusalReceiptHandle);

        return $"promotion-seduction-watch://{ComputeDigest(cmeId, candidateLedgerHandle, variationTestedReentryLedgerHandle, admissionRefusalReceiptHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
