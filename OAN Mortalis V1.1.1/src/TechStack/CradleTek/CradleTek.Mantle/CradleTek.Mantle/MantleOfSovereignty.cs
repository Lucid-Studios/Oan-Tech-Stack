using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace CradleTek.Mantle;

public interface IGovernedSeedMantleSource
{
    GovernedSeedMantleReceipt CreateReceipt(string agentId, string theaterId);
}

public sealed class MantleOfSovereignty : IGovernedSeedMantleSource
{
    public GovernedSeedMantleReceipt CreateReceipt(string agentId, string theaterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);

        var timestampUtc = DateTimeOffset.UtcNow;
        var mantleHandle = CreateHandle("mos://", agentId, theaterId);
        var crypticMantleHandle = CreateHandle("cmos://", agentId, theaterId);
        var oeHandle = CreateHandle("oe://", agentId, theaterId);
        var crypticOeHandle = CreateHandle("coe://", agentId, theaterId);
        var selfGelHandle = CreateHandle("selfgel://", agentId, theaterId);
        var crypticSelfGelHandle = CreateHandle("cselfgel://", agentId, theaterId);
        var cmeHandle = CreateHandle("cme://", agentId, theaterId);
        var presentedGroupoid = new GovernedSeedMantleGroupoid(
            GroupoidHandle: CreateHandle("mos-groupoid://", agentId, theaterId, "presented"),
            OeHandle: oeHandle,
            SelfGelHandle: selfGelHandle,
            GroupoidProfile: "oe-selfgel-presented-pair",
            PresentedSide: true,
            ProtectedSide: false);
        var crypticGroupoid = new GovernedSeedMantleGroupoid(
            GroupoidHandle: CreateHandle("cmos-groupoid://", agentId, theaterId, "protected"),
            OeHandle: crypticOeHandle,
            SelfGelHandle: crypticSelfGelHandle,
            GroupoidProfile: "coe-cselfgel-protected-pair",
            PresentedSide: false,
            ProtectedSide: true);
        var appendOnlyLedger = new GovernedSeedAppendOnlyLedger(
            LedgerHandle: CreateHandle("mos-ledger://", agentId, theaterId),
            LedgerProfile: "opal-engram-append-only-ledger",
            AppendOnly: true,
            PresentedBlocks:
            [
                new GovernedSeedAppendOnlyLedgerBlock(
                    BlockHash: CreateHashHex(agentId, theaterId, "presented-selfgel-anchor"),
                    CreatedAtUtc: timestampUtc,
                    PayloadPointer: selfGelHandle,
                    BlockKind: GovernedSeedAppendOnlyLedgerBlockKind.PresentedSelfGel,
                    PointerProfile: "selfgel-anchor-pointer")
            ],
            ProtectedBlocks:
            [
                new GovernedSeedAppendOnlyLedgerBlock(
                    BlockHash: CreateHashHex(agentId, theaterId, "protected-cselfgel-anchor"),
                    CreatedAtUtc: timestampUtc,
                    PayloadPointer: crypticSelfGelHandle,
                    BlockKind: GovernedSeedAppendOnlyLedgerBlockKind.ProtectedSelfGel,
                    PointerProfile: "cselfgel-anchor-pointer")
            ]);
        var opalEngramSeatHandle = CreateHandle("opal-engram-seat://", agentId, theaterId);
        var snapshotRequest = new GovernedSeedSoulFrameSnapshotRequest(
            SnapshotHandle: CreateHandle("soulframe-snapshot://", agentId, theaterId),
            SoulFrameHandle: CreateHandle("soulframe://", agentId, theaterId),
            CmeHandle: cmeHandle,
            OpalEngramSeatHandle: opalEngramSeatHandle,
            SnapshotProfile: "mantle-shadow-copy-reentry-snapshot",
            TimestampUtc: timestampUtc);
        var opalEngramSeat = new GovernedSeedOpalEngramSeat(
            SeatHandle: opalEngramSeatHandle,
            CmeHandle: cmeHandle,
            SeatProfile: "braided-opal-engram-cme-seat",
            PresentedEngramHandle: oeHandle,
            ProtectedEngramHandle: crypticOeHandle,
            PresentedGroupoid: presentedGroupoid,
            CrypticGroupoid: crypticGroupoid,
            AppendOnlyLedger: appendOnlyLedger,
            SnapshotRequest: snapshotRequest,
            ShadowCopyOnly: true,
            RecoverableByOperator: true,
            RecoverableByCustomer: true,
            ProtectedPresentedBraided: true);

        return new GovernedSeedMantleReceipt(
            MantleHandle: mantleHandle,
            CrypticMantleHandle: crypticMantleHandle,
            OeHandle: oeHandle,
            CrypticOeHandle: crypticOeHandle,
            SelfGelHandle: selfGelHandle,
            CrypticSelfGelHandle: crypticSelfGelHandle,
            CustodyProfile: "opal-engram-oe-coe-custody",
            CmeBindingProfile: "governing-and-operator-bound-cmes",
            PrimeGovernanceOffice: "Mother",
            CrypticGovernanceOffice: "Father",
            StoresOpalEngrams: true,
            StoresCrypticOpalEngrams: true,
            OperatorRecoveryEligible: true,
            CustomerRecoveryEligible: true,
            ExclusiveRecoverySeat: true,
            RecoveryProfile: "exclusive-in-use-model-recovery-seat",
            PresentedGroupoid: presentedGroupoid,
            CrypticGroupoid: crypticGroupoid,
            OpalEngramSeat: opalEngramSeat,
            ProtectedPresentedBraided: true,
            BraidProfile: "presented-protected-groupoid-braid",
            ReceiptProfile: "bootstrap-mantle-sovereignty",
            TimestampUtc: timestampUtc);
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
