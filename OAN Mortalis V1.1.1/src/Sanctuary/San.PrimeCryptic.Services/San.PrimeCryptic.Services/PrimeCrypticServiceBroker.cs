using System.Security.Cryptography;
using System.Text;
using San.Common;
using SLI.Lisp;

namespace San.PrimeCryptic.Services;

public interface IPrimeCrypticServiceBroker
{
    GovernedSeedPrimeCrypticServiceReceipt DescribeResidentField(string agentId, string theaterId);
}

public sealed class PrimeCrypticServiceBroker : IPrimeCrypticServiceBroker
{
    private readonly ICrypticLispBundleService _lispBundleService;

    public PrimeCrypticServiceBroker(ICrypticLispBundleService lispBundleService)
    {
        _lispBundleService = lispBundleService ?? throw new ArgumentNullException(nameof(lispBundleService));
    }

    public GovernedSeedPrimeCrypticServiceReceipt DescribeResidentField(string agentId, string theaterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);

        return new GovernedSeedPrimeCrypticServiceReceipt(
            ServiceHandle: CreateHandle("primecryptic://", agentId, theaterId),
            CrypticServiceHandle: CreateHandle("cryptic-service://", agentId, theaterId),
            PrimeServiceHandle: CreateHandle("prime-service://", agentId, theaterId),
            ResidencyProfile: "cpu-only/resident-field",
            CpuOnly: true,
            TargetBoundedLaneAvailable: false,
            CrypticResidencyClass: "sanctuary-resident-cryptic",
            PrimeProjectionClass: "structural-only",
            LispBundleReceipt: _lispBundleService.DescribeResidentBundle(),
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
