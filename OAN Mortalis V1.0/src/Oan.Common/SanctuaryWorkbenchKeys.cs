using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class SanctuaryWorkbenchKeys
{
    public static string CreateSanctuaryRuntimeWorkbenchHandle(
        string cmeId,
        string utilitySurfaceHandle,
        string bondedLocalityLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(utilitySurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(bondedLocalityLedgerHandle);

        return $"sanctuary-runtime-workbench://{ComputeDigest(cmeId, utilitySurfaceHandle, bondedLocalityLedgerHandle)}";
    }

    public static string CreateAmenableDayDreamTierHandle(
        string cmeId,
        string workbenchHandle,
        string admissibilityState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissibilityState);

        return $"amenable-day-dream-tier://{ComputeDigest(cmeId, workbenchHandle, admissibilityState)}";
    }

    public static string CreateCrypticBiadRootHandle(
        string cmeId,
        string identityInvariantHandle,
        string threadBirthHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityInvariantHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(threadBirthHandle);

        return $"cryptic-biad-root://{ComputeDigest(cmeId, identityInvariantHandle, threadBirthHandle)}";
    }

    public static string CreateSelfRootedCrypticDepthGateHandle(
        string cmeId,
        string workbenchHandle,
        string crypticBiadRootHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(crypticBiadRootHandle);

        return $"self-rooted-cryptic-depth-gate://{ComputeDigest(cmeId, workbenchHandle, crypticBiadRootHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
