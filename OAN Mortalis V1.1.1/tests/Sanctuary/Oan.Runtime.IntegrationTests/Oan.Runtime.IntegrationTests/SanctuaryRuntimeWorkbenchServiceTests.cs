using CradleTek.Runtime;

namespace Oan.Runtime.IntegrationTests;

public sealed class SanctuaryRuntimeWorkbenchServiceTests
{
    [Fact]
    public void CreateBoundaryConditionLedgerAndCoherenceGainWitness_CarryForwardConstraintMemory()
    {
        var service = new SanctuaryRuntimeWorkbenchService();

        var ledger = service.CreateBoundaryConditionLedger("oan-mortalis-v1.1.1", "carry-forward-constraint-memory");
        var witness = service.CreateCoherenceGainWitnessReceipt("oan-mortalis-v1.1.1", "carry-forward-constraint-memory");

        Assert.StartsWith("boundary-condition-ledger://", ledger.LedgerHandle, StringComparison.Ordinal);
        Assert.Equal("boundary-condition-ledger-bound", ledger.ReasonCode);
        Assert.StartsWith("coherence-gain-witness://", witness.ReceiptHandle, StringComparison.Ordinal);
        Assert.Equal("coherence-gain-witness-receipt-bound", witness.ReasonCode);
    }

    [Fact]
    public void CreateInquirySessionDisciplineSurface_BindsQuestioningAndSilenceInsideBoundedHabitation()
    {
        var service = new SanctuaryRuntimeWorkbenchService();

        var receipt = service.CreateInquirySessionDisciplineSurface(
            "oan-mortalis-v1.1.1",
            "questioning-and-silence-inside-bounded-habitation");

        Assert.StartsWith("inquiry-session-discipline-surface://", receipt.SurfaceHandle, StringComparison.Ordinal);
        Assert.Equal("inquiry-session-discipline-surface-bound", receipt.ReasonCode);
    }
}
