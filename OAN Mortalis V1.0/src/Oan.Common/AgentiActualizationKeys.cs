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

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
