using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class WorkerGovernanceKeys
{
    public static string CreateIdentityInvariantHandle(
        string cmeId,
        Guid identityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentOutOfRangeException.ThrowIfEqual(identityId, Guid.Empty);

        return $"identity-invariant://{ComputeDigest(cmeId, identityId.ToString("D"))}";
    }

    public static string CreateWorkerThreadRootHandle(
        string cmeId,
        Guid identityId,
        string sessionHandle,
        string workingStateHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentOutOfRangeException.ThrowIfEqual(identityId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingStateHandle);

        return $"worker-thread-root://{ComputeDigest(cmeId, identityId.ToString("D"), sessionHandle, workingStateHandle)}";
    }

    public static string CreateGovernedThreadBirthHandle(
        string cmeId,
        string threadRootHandle,
        string governanceLayerHandle,
        string nexusBindingHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(threadRootHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(governanceLayerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(nexusBindingHandle);

        return $"governed-thread-birth://{ComputeDigest(cmeId, threadRootHandle, governanceLayerHandle, nexusBindingHandle)}";
    }

    public static string CreateInterWorkerBraidHandoffPacketId(
        string cmeId,
        string sourceThreadBirthHandle,
        string targetThreadBirthHandle,
        string predicateContextHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceThreadBirthHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetThreadBirthHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(predicateContextHandle);

        return $"worker-braid-handoff://{ComputeDigest(cmeId, sourceThreadBirthHandle, targetThreadBirthHandle, predicateContextHandle)}";
    }

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
