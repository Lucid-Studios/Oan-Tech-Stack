using System.Security.Cryptography;
using System.Text;

namespace CradleTek.Mantle;

public interface IGovernedSelfGelValidationHandleProjector
{
    string ProjectPresentedValidationHandle(string crypticSelfGelHandle);
}

public sealed class GovernedSelfGelValidationHandleProjector : IGovernedSelfGelValidationHandleProjector
{
    public string ProjectPresentedValidationHandle(string crypticSelfGelHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(crypticSelfGelHandle);

        if (crypticSelfGelHandle.StartsWith("cselfgel://", StringComparison.Ordinal))
        {
            return "selfgel://" + crypticSelfGelHandle["cselfgel://".Length..];
        }

        if (crypticSelfGelHandle.StartsWith("soulframe-cselfgel://", StringComparison.Ordinal))
        {
            return "soulframe-selfgel://" + crypticSelfGelHandle["soulframe-cselfgel://".Length..];
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(crypticSelfGelHandle.Trim()));
        return $"selfgel://derived/{Convert.ToHexString(bytes).ToLowerInvariant()[..16]}";
    }
}
