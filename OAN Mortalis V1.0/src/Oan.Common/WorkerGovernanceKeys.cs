using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class WorkerGovernanceKeys
{
    public static string CreateWorkerHandoffPacketId(
        string loopKey,
        string cmeId,
        GovernedWorkerSpecies workerSpecies,
        string officeIssuanceHandle,
        string requestingOfficeInstanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(officeIssuanceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestingOfficeInstanceId);

        return $"worker-handoff-packet://{ComputeDigest(loopKey, cmeId, workerSpecies.ToString(), officeIssuanceHandle, requestingOfficeInstanceId)}";
    }

    public static string CreateWorkerHandoffHandle(
        string loopKey,
        string cmeId,
        string handoffPacketId,
        string officeIssuanceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handoffPacketId);
        ArgumentException.ThrowIfNullOrWhiteSpace(officeIssuanceHandle);

        return $"worker-handoff://{ComputeDigest(loopKey, cmeId, handoffPacketId, officeIssuanceHandle)}";
    }

    public static string CreateWorkerReturnPacketId(
        string loopKey,
        string cmeId,
        string handoffPacketId,
        GovernedWorkerSpecies workerSpecies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handoffPacketId);

        return $"worker-return-packet://{ComputeDigest(loopKey, cmeId, handoffPacketId, workerSpecies.ToString())}";
    }

    public static string CreateWorkerReturnHandle(
        string loopKey,
        string cmeId,
        string workerPacketId,
        string handoffHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workerPacketId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handoffHandle);

        return $"worker-return://{ComputeDigest(loopKey, cmeId, workerPacketId, handoffHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
