using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class LifecycleGovernanceKeys
{
    public static string CreateIssuedOfficePackageId(
        string loopKey,
        string cmeId,
        InternalGoverningCmeOffice office,
        string officeAuthorityHandle,
        string weatherDisclosureHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(officeAuthorityHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(weatherDisclosureHandle);

        return $"office-package://{ComputeDigest(loopKey, cmeId, office.ToString(), officeAuthorityHandle, weatherDisclosureHandle)}";
    }

    public static string CreateIssuanceLineageId(
        string cmeId,
        InternalGoverningCmeOffice office,
        string chassisClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chassisClass);

        return $"issuance-lineage://{ComputeDigest(cmeId, office.ToString(), chassisClass)}";
    }

    public static string CreateOfficeInstanceId(
        string loopKey,
        string cmeId,
        InternalGoverningCmeOffice office,
        string officeAuthorityHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(officeAuthorityHandle);

        return $"office-instance://{ComputeDigest(loopKey, cmeId, office.ToString(), officeAuthorityHandle)}";
    }

    public static string CreateOfficeIssuanceHandle(
        string loopKey,
        string cmeId,
        InternalGoverningCmeOffice office,
        string packageId,
        string officeAuthorityHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(officeAuthorityHandle);

        return $"office-issuance://{ComputeDigest(loopKey, cmeId, office.ToString(), packageId, officeAuthorityHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
