using Oan.Common;
using Oan.FirstRun;

namespace Oan.Audit.Tests;

public sealed class GovernedSeedPreGovernanceServiceTests
{
    private readonly GovernedSeedPreGovernanceService _service = new();

    [Fact]
    public void Project_With_Full_Source_Seams_Mints_Explicit_Packet()
    {
        var packet = _service.Project(
            CreateBootstrapReceipt(protectedPresentedBraided: true),
            CreateSanctuaryIngressReceipt(),
            CreateLowMindRoute(),
            "theater-pre-governance");

        Assert.NotNull(packet.LocalAuthorityTrace);
        Assert.NotNull(packet.ConstitutionalContact);
        Assert.NotNull(packet.LocalKeypairGenesisSource);
        Assert.NotNull(packet.LocalKeypairGenesis);
        Assert.NotNull(packet.FirstCrypticBraidEstablishment);
        Assert.NotNull(packet.FirstCrypticBraid);
        Assert.NotNull(packet.FirstCrypticConditioningSource);
        Assert.NotNull(packet.FirstCrypticConditioning);
        Assert.Equal(packet.LocalAuthorityTrace!.ReceiptHandle, packet.ConstitutionalContact!.LocalAuthorityTraceReceiptHandle);
        Assert.Equal(packet.ConstitutionalContact.ReceiptHandle, packet.LocalKeypairGenesisSource!.ConstitutionalContactReceiptHandle);
        Assert.Equal(packet.LocalKeypairGenesisSource.ReceiptHandle, packet.LocalKeypairGenesis.LocalKeypairGenesisSourceReceiptHandle);
        Assert.Equal(packet.ConstitutionalContact!.ReceiptHandle, packet.LocalKeypairGenesis!.ConstitutionalContactReceiptHandle);
        Assert.Equal(packet.FirstCrypticBraidEstablishment!.ReceiptHandle, packet.FirstCrypticBraid!.FirstCrypticBraidEstablishmentReceiptHandle);
        Assert.Equal(packet.ConstitutionalContact.ReceiptHandle, packet.FirstCrypticBraid!.ConstitutionalContactReceiptHandle);
        Assert.Equal(packet.LocalKeypairGenesis.ReceiptHandle, packet.FirstCrypticBraid.LocalKeypairGenesisReceiptHandle);
        Assert.Equal(packet.FirstCrypticBraid.ReceiptHandle, packet.FirstCrypticConditioningSource!.FirstCrypticBraidReceiptHandle);
        Assert.Equal(packet.FirstCrypticConditioningSource.ReceiptHandle, packet.FirstCrypticConditioning!.FirstCrypticConditioningSourceReceiptHandle);
        Assert.Equal(packet.FirstCrypticBraid.ReceiptHandle, packet.FirstCrypticConditioning!.FirstCrypticBraidReceiptHandle);
        Assert.Equal("pre-governance-sequence-projected-through-first-cryptic-conditioning", packet.SourceReason);
    }

    [Fact]
    public void Project_Without_LowMind_Route_Leaves_First_Cryptic_Conditioning_Unset()
    {
        var packet = _service.Project(
            CreateBootstrapReceipt(protectedPresentedBraided: true),
            CreateSanctuaryIngressReceipt(),
            lowMindSfRoute: null,
            theaterId: "theater-pre-governance");

        Assert.NotNull(packet.LocalAuthorityTrace);
        Assert.NotNull(packet.ConstitutionalContact);
        Assert.NotNull(packet.LocalKeypairGenesisSource);
        Assert.NotNull(packet.LocalKeypairGenesis);
        Assert.NotNull(packet.FirstCrypticBraidEstablishment);
        Assert.NotNull(packet.FirstCrypticBraid);
        Assert.Null(packet.FirstCrypticConditioningSource);
        Assert.Null(packet.FirstCrypticConditioning);
        Assert.Equal("pre-governance-sequence-partially-projected-before-first-cryptic-conditioning", packet.SourceReason);
    }

    private static GovernedSeedSanctuaryIngressReceipt CreateSanctuaryIngressReceipt() =>
        new(
            ReceiptHandle: "sanctuary-ingress://receipt",
            PacketHandle: "sanctuary-ingress://packet",
            ReceiptProfile: "obsidian-wall-ingress",
            PacketProfile: "engrammitized-input-packet",
            SourceInputHandle: "source-input://prompt",
            PreparedInputHandle: "prepared-input://prompt",
            IngressAccessClass: GovernedSeedIngressAccessClass.PromptInput,
            ExternalInputRequiresCustodyChain: true,
            ObsidianWallApplied: true,
            EngrammitizedForCradleTek: true,
            RawPromptAuthorityTerminated: true,
            SourceReason: "test-ingress",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedLowMindSfRoutePacket CreateLowMindRoute() =>
        new(
            PacketHandle: "lowmind-sf://route",
            PacketProfile: "prompt-to-soulframe",
            BootstrapHandle: "bootstrap://receipt",
            SanctuaryIngressReceiptHandle: "sanctuary-ingress://receipt",
            MemoryContextHandle: "memory-context://handle",
            IngressAccessClass: GovernedSeedIngressAccessClass.PromptInput,
            RouteKind: GovernedSeedLowMindSfRouteKind.DirectPrompt,
            ObsidianWallApplied: true,
            RoutedThroughSoulFrame: true,
            RequiresHigherOrderFunction: false,
            SourceReason: "test-lowmind-route",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedSoulFrameBootstrapReceipt CreateBootstrapReceipt(bool protectedPresentedBraided)
    {
        var presentedGroupoid = new GovernedSeedMantleGroupoid(
            GroupoidHandle: "groupoid://presented",
            OeHandle: "oe://presented",
            SelfGelHandle: "selfgel://presented",
            GroupoidProfile: "presented-groupoid",
            PresentedSide: true,
            ProtectedSide: false);
        var crypticGroupoid = new GovernedSeedMantleGroupoid(
            GroupoidHandle: "groupoid://cryptic",
            OeHandle: "oe://cryptic",
            SelfGelHandle: "cselfgel://cryptic",
            GroupoidProfile: "cryptic-groupoid",
            PresentedSide: false,
            ProtectedSide: true);
        var opalSeat = new GovernedSeedOpalEngramSeat(
            SeatHandle: "opal-seat://001",
            CmeHandle: "cme://001",
            SeatProfile: "opal-seat",
            PresentedEngramHandle: "engram://presented",
            ProtectedEngramHandle: "engram://protected",
            PresentedGroupoid: presentedGroupoid,
            CrypticGroupoid: crypticGroupoid,
            AppendOnlyLedger: new GovernedSeedAppendOnlyLedger(
                LedgerHandle: "ledger://001",
                LedgerProfile: "append-only",
                AppendOnly: true,
                PresentedBlocks: [],
                ProtectedBlocks: []),
            SnapshotRequest: new GovernedSeedSoulFrameSnapshotRequest(
                SnapshotHandle: "snapshot://001",
                SoulFrameHandle: "soulframe://001",
                CmeHandle: "cme://001",
                OpalEngramSeatHandle: "opal-seat://001",
                SnapshotProfile: "bootstrap-snapshot",
                TimestampUtc: DateTimeOffset.UtcNow),
            ShadowCopyOnly: false,
            RecoverableByOperator: true,
            RecoverableByCustomer: true,
            ProtectedPresentedBraided: protectedPresentedBraided);
        var mantleReceipt = new GovernedSeedMantleReceipt(
            MantleHandle: "mantle://001",
            CrypticMantleHandle: "cmantle://001",
            OeHandle: "oe://001",
            CrypticOeHandle: "coe://001",
            SelfGelHandle: "selfgel://001",
            CrypticSelfGelHandle: "cselfgel://001",
            CustodyProfile: "custody-profile",
            CmeBindingProfile: "binding-profile",
            PrimeGovernanceOffice: "Mother",
            CrypticGovernanceOffice: "Father",
            StoresOpalEngrams: true,
            StoresCrypticOpalEngrams: true,
            OperatorRecoveryEligible: true,
            CustomerRecoveryEligible: true,
            ExclusiveRecoverySeat: true,
            RecoveryProfile: "recovery-profile",
            PresentedGroupoid: presentedGroupoid,
            CrypticGroupoid: crypticGroupoid,
            OpalEngramSeat: opalSeat,
            ProtectedPresentedBraided: protectedPresentedBraided,
            BraidProfile: "prime-cryptic-steward-braid",
            ReceiptProfile: "mantle-receipt",
            TimestampUtc: DateTimeOffset.UtcNow);
        var custodySnapshot = new GovernedSeedCustodySnapshot(
            GelHandle: "gel://001",
            CrypticGelHandle: "cgel://001",
            GoaHandle: "goa://001",
            CrypticGoaHandle: "cgoa://001",
            MosHandle: "mos://001",
            CrypticMosHandle: "cmos://001",
            OeHandle: "oe://001",
            CrypticOeHandle: "coe://001",
            SelfGelHandle: "selfgel://001",
            CrypticSelfGelHandle: "cselfgel://001",
            CGoaHoldSurface: new GovernedSeedCustodyHoldSurface(
                SurfaceHandle: "hold://cgoa",
                SurfaceKind: GovernedSeedCustodyHoldSurfaceKind.CGoa,
                SurfaceProfile: "cgoa-hold",
                SelfStateBearing: true,
                ContextualResidueBearing: true,
                DeferredReviewByDefault: true),
            CMosHoldSurface: new GovernedSeedCustodyHoldSurface(
                SurfaceHandle: "hold://cmos",
                SurfaceKind: GovernedSeedCustodyHoldSurfaceKind.CMos,
                SurfaceProfile: "cmos-hold",
                SelfStateBearing: true,
                ContextualResidueBearing: true,
                DeferredReviewByDefault: true));

        return new GovernedSeedSoulFrameBootstrapReceipt(
            BootstrapHandle: "bootstrap://receipt",
            SoulFrameHandle: "soulframe://001",
            MembraneHandle: "membrane://001",
            BootstrapProfile: "cpu-only/bootstrap-resident",
            MantleReceipt: mantleReceipt,
            CustodySnapshot: custodySnapshot,
            IdentitySeat: new GovernedSeedSoulFrameIdentitySeat(
                IdentityHandle: "identity://001",
                SoulFrameHandle: "soulframe://001",
                CmeHandle: "cme://001",
                OpalEngramSeatHandle: "opal-seat://001",
                OperatorBondHandle: "operator-bond://001",
                SelfGelHandle: "selfgel://001",
                CrypticSelfGelHandle: "cselfgel://001",
                RuntimePolicy: GovernedSeedSoulFrameRuntimePolicy.Default,
                IntegrityHash: "integrity-hash-001",
                CreatedAtUtc: DateTimeOffset.UtcNow,
                LastActiveAtUtc: DateTimeOffset.UtcNow,
                AttachmentState: GovernedSeedSoulFrameAttachmentState.Detached),
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
