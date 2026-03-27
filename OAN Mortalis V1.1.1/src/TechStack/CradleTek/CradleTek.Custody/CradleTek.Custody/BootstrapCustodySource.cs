using System.Security.Cryptography;
using System.Text;
using CradleTek.Mantle;
using Oan.Common;

namespace CradleTek.Custody;

public interface IGovernedSeedCustodySource
{
    GovernedSeedCustodyBootstrapContext CreateBootstrapContext(string agentId, string theaterId);
}

public sealed class BootstrapCustodySource : IGovernedSeedCustodySource
{
    private readonly IGovernedSeedMantleSource _mantleSource;

    public BootstrapCustodySource(IGovernedSeedMantleSource? mantleSource = null)
    {
        _mantleSource = mantleSource ?? new MantleOfSovereignty();
    }

    public GovernedSeedCustodyBootstrapContext CreateBootstrapContext(string agentId, string theaterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);

        var mantleReceipt = _mantleSource.CreateReceipt(agentId, theaterId);
        var custodySnapshot = new GovernedSeedCustodySnapshot(
            GelHandle: CreateHandle("gel://", agentId, theaterId),
            CrypticGelHandle: CreateHandle("cgel://", agentId, theaterId),
            GoaHandle: CreateHandle("goa://", agentId, theaterId),
            CrypticGoaHandle: CreateHandle("cgoa://", agentId, theaterId),
            MosHandle: mantleReceipt.MantleHandle,
            CrypticMosHandle: mantleReceipt.CrypticMantleHandle,
            OeHandle: mantleReceipt.OeHandle,
            CrypticOeHandle: mantleReceipt.CrypticOeHandle,
            SelfGelHandle: mantleReceipt.SelfGelHandle,
            CrypticSelfGelHandle: mantleReceipt.CrypticSelfGelHandle,
            CGoaHoldSurface: new GovernedSeedCustodyHoldSurface(
                SurfaceHandle: CreateHandle("cgoa://", agentId, theaterId),
                SurfaceKind: GovernedSeedCustodyHoldSurfaceKind.CGoa,
                SurfaceProfile: "first-route-contextual-hold",
                SelfStateBearing: false,
                ContextualResidueBearing: true,
                DeferredReviewByDefault: false),
            CMosHoldSurface: new GovernedSeedCustodyHoldSurface(
                SurfaceHandle: CreateHandle("cmos://", agentId, theaterId),
                SurfaceKind: GovernedSeedCustodyHoldSurfaceKind.CMos,
                SurfaceProfile: "first-route-self-state-hold",
                SelfStateBearing: true,
                ContextualResidueBearing: false,
                DeferredReviewByDefault: true));

        return new GovernedSeedCustodyBootstrapContext(
            CustodySnapshot: custodySnapshot,
            MantleReceipt: mantleReceipt);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
