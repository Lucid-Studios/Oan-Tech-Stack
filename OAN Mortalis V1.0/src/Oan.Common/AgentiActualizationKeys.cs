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

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
