using System.Text.Json;
using CradleTek.Host;
using CradleTek.Mantle;
using San.Common;
using San.Runtime.Headless;

namespace Oan.Runtime.IntegrationTests;

public sealed class CradleTekMantleStorageCarryForwardTests
{
    [Fact]
    public void MantleReceipt_CarriesBraidedAppendOnlyOpalEngramSeat()
    {
        var mantle = new MantleOfSovereignty();

        var receipt = mantle.CreateReceipt("agent-mantle", "theater-mantle");

        Assert.True(receipt.ProtectedPresentedBraided);
        Assert.True(receipt.OpalEngramSeat.ProtectedPresentedBraided);
        Assert.True(receipt.OpalEngramSeat.ShadowCopyOnly);
        Assert.True(receipt.OpalEngramSeat.RecoverableByOperator);
        Assert.True(receipt.OpalEngramSeat.RecoverableByCustomer);
        Assert.Equal("braided-opal-engram-cme-seat", receipt.OpalEngramSeat.SeatProfile);
        Assert.StartsWith("cme://", receipt.OpalEngramSeat.CmeHandle, StringComparison.Ordinal);
        Assert.Equal(receipt.OeHandle, receipt.OpalEngramSeat.PresentedEngramHandle);
        Assert.Equal(receipt.CrypticOeHandle, receipt.OpalEngramSeat.ProtectedEngramHandle);
        Assert.Equal(receipt.PresentedGroupoid, receipt.OpalEngramSeat.PresentedGroupoid);
        Assert.Equal(receipt.CrypticGroupoid, receipt.OpalEngramSeat.CrypticGroupoid);
        Assert.Equal("opal-engram-append-only-ledger", receipt.OpalEngramSeat.AppendOnlyLedger.LedgerProfile);
        Assert.True(receipt.OpalEngramSeat.AppendOnlyLedger.AppendOnly);
        Assert.Single(receipt.OpalEngramSeat.AppendOnlyLedger.PresentedBlocks);
        Assert.Single(receipt.OpalEngramSeat.AppendOnlyLedger.ProtectedBlocks);

        var presentedBlock = receipt.OpalEngramSeat.AppendOnlyLedger.PresentedBlocks[0];
        var protectedBlock = receipt.OpalEngramSeat.AppendOnlyLedger.ProtectedBlocks[0];

        Assert.Equal(GovernedSeedAppendOnlyLedgerBlockKind.PresentedSelfGel, presentedBlock.BlockKind);
        Assert.Equal(receipt.SelfGelHandle, presentedBlock.PayloadPointer);
        Assert.Equal("selfgel-anchor-pointer", presentedBlock.PointerProfile);
        Assert.Equal(GovernedSeedAppendOnlyLedgerBlockKind.ProtectedSelfGel, protectedBlock.BlockKind);
        Assert.Equal(receipt.CrypticSelfGelHandle, protectedBlock.PayloadPointer);
        Assert.Equal("cselfgel-anchor-pointer", protectedBlock.PointerProfile);
        Assert.Equal(receipt.OpalEngramSeat.SeatHandle, receipt.OpalEngramSeat.SnapshotRequest.OpalEngramSeatHandle);
        Assert.Equal(receipt.OpalEngramSeat.CmeHandle, receipt.OpalEngramSeat.SnapshotRequest.CmeHandle);
        Assert.Equal("mantle-shadow-copy-reentry-snapshot", receipt.OpalEngramSeat.SnapshotRequest.SnapshotProfile);
    }

    [Fact]
    public void GovernedSelfGelValidationHandleProjector_Projects_Presented_Handle_From_Cryptic_Handle()
    {
        IGovernedSelfGelValidationHandleProjector projector = new GovernedSelfGelValidationHandleProjector();

        var projected = projector.ProjectPresentedValidationHandle("cselfgel://abc123");

        Assert.Equal("selfgel://abc123", projected);
    }

    [Fact]
    public async Task BootstrapPath_CarriesBraidedOpalEngramSeatIntoVerticalSlice()
    {
        IGovernedSeedHost host = HeadlessRuntimeBootstrap.CreateHost();

        var result = await host.EvaluateAsync(
            "agent-mantle-bootstrap",
            "theater-mantle-bootstrap",
            """
            Standing:
            - bounded_summary_ready
            Protected / non-disclosable:
            - contextual | masked_context_note
            Permitted derivation:
            - masked_summary
            """);

        var payload = JsonSerializer.Deserialize<GovernedSeedVerticalSlice>(result.Payload!);

        Assert.NotNull(payload);
        Assert.NotNull(payload.BootstrapReceipt);

        var bootstrapReceipt = payload.BootstrapReceipt;
        var mantleReceipt = bootstrapReceipt.MantleReceipt;
        var opalEngramSeat = mantleReceipt.OpalEngramSeat;
        var identitySeat = bootstrapReceipt.IdentitySeat;

        Assert.Equal(bootstrapReceipt.SoulFrameHandle, opalEngramSeat.SnapshotRequest.SoulFrameHandle);
        Assert.Equal(opalEngramSeat.SeatHandle, opalEngramSeat.SnapshotRequest.OpalEngramSeatHandle);
        Assert.Equal(mantleReceipt.MantleHandle, bootstrapReceipt.CustodySnapshot.MosHandle);
        Assert.Equal(mantleReceipt.CrypticMantleHandle, bootstrapReceipt.CustodySnapshot.CrypticMosHandle);
        Assert.Equal(mantleReceipt.OeHandle, opalEngramSeat.PresentedEngramHandle);
        Assert.Equal(mantleReceipt.CrypticOeHandle, opalEngramSeat.ProtectedEngramHandle);
        Assert.Equal(mantleReceipt.SelfGelHandle, opalEngramSeat.AppendOnlyLedger.PresentedBlocks[0].PayloadPointer);
        Assert.Equal(mantleReceipt.CrypticSelfGelHandle, opalEngramSeat.AppendOnlyLedger.ProtectedBlocks[0].PayloadPointer);
        Assert.Equal(bootstrapReceipt.SoulFrameHandle, identitySeat.SoulFrameHandle);
        Assert.Equal(opalEngramSeat.CmeHandle, identitySeat.CmeHandle);
        Assert.Equal(opalEngramSeat.SeatHandle, identitySeat.OpalEngramSeatHandle);
        Assert.Equal(mantleReceipt.SelfGelHandle, identitySeat.SelfGelHandle);
        Assert.Equal(mantleReceipt.CrypticSelfGelHandle, identitySeat.CrypticSelfGelHandle);
        Assert.Equal(GovernedSeedSoulFrameRuntimePolicy.Default, identitySeat.RuntimePolicy);
        Assert.Equal(GovernedSeedSoulFrameAttachmentState.Detached, identitySeat.AttachmentState);
        Assert.NotEmpty(identitySeat.OperatorBondHandle);
        Assert.NotEmpty(identitySeat.IntegrityHash);
        Assert.True(opalEngramSeat.AppendOnlyLedger.AppendOnly);
        Assert.True(opalEngramSeat.ShadowCopyOnly);
        Assert.True(opalEngramSeat.ProtectedPresentedBraided);
    }
}
