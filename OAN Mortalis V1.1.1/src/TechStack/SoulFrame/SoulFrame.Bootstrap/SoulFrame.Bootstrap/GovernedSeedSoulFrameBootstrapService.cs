using System.Security.Cryptography;
using System.Text;
using CradleTek.Custody;
using Oan.Common;

namespace SoulFrame.Bootstrap;

public interface IGovernedSeedSoulFrameBootstrapService
{
    GovernedSeedSoulFrameBootstrapReceipt Bootstrap(string agentId, string theaterId);
}

public sealed class GovernedSeedSoulFrameBootstrapService : IGovernedSeedSoulFrameBootstrapService
{
    private readonly IGovernedSeedCustodySource _custodySource;

    public GovernedSeedSoulFrameBootstrapService(IGovernedSeedCustodySource custodySource)
    {
        _custodySource = custodySource ?? throw new ArgumentNullException(nameof(custodySource));
    }

    public GovernedSeedSoulFrameBootstrapReceipt Bootstrap(string agentId, string theaterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);

        var bootstrapContext = _custodySource.CreateBootstrapContext(agentId, theaterId);
        var timestampUtc = DateTimeOffset.UtcNow;
        return new GovernedSeedSoulFrameBootstrapReceipt(
            BootstrapHandle: CreateHandle("soulframe-bootstrap://", agentId, theaterId),
            SoulFrameHandle: CreateHandle("soulframe://", agentId, theaterId),
            MembraneHandle: CreateHandle("membrane://", agentId, theaterId),
            BootstrapProfile: "cpu-only/bootstrap-resident",
            MantleReceipt: bootstrapContext.MantleReceipt,
            CustodySnapshot: bootstrapContext.CustodySnapshot,
            IdentitySeat: CreateIdentitySeat(
                agentId,
                theaterId,
                bootstrapContext.MantleReceipt,
                timestampUtc),
            TimestampUtc: timestampUtc);
    }

    private static GovernedSeedSoulFrameIdentitySeat CreateIdentitySeat(
        string agentId,
        string theaterId,
        GovernedSeedMantleReceipt mantleReceipt,
        DateTimeOffset timestampUtc)
    {
        var soulFrameHandle = CreateHandle("soulframe://", agentId, theaterId);
        var cmeHandle = mantleReceipt.OpalEngramSeat.CmeHandle;
        var opalEngramSeatHandle = mantleReceipt.OpalEngramSeat.SeatHandle;
        var operatorBondHandle = CreateHandle("operator-bond://", agentId, theaterId);
        var integrityHash = CreateHashHex(
            soulFrameHandle,
            cmeHandle,
            opalEngramSeatHandle,
            operatorBondHandle,
            mantleReceipt.SelfGelHandle,
            mantleReceipt.CrypticSelfGelHandle);

        return new GovernedSeedSoulFrameIdentitySeat(
            IdentityHandle: CreateHandle("soulframe-identity://", agentId, theaterId),
            SoulFrameHandle: soulFrameHandle,
            CmeHandle: cmeHandle,
            OpalEngramSeatHandle: opalEngramSeatHandle,
            OperatorBondHandle: operatorBondHandle,
            SelfGelHandle: mantleReceipt.SelfGelHandle,
            CrypticSelfGelHandle: mantleReceipt.CrypticSelfGelHandle,
            RuntimePolicy: GovernedSeedSoulFrameRuntimePolicy.Default,
            IntegrityHash: integrityHash,
            CreatedAtUtc: timestampUtc,
            LastActiveAtUtc: timestampUtc,
            AttachmentState: GovernedSeedSoulFrameAttachmentState.Detached);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private static string CreateHashHex(params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
