using San.Common;

namespace SLI.Engine;

public sealed record PrimeMembraneProjectedHistoryReceiptEvaluation(
    PrimeMembraneProjectedBraidInterpretation Interpretation,
    PrimeMembraneHistoryReceipt HistoryReceipt,
    string GovernanceTrace);

public interface IPrimeMembraneProjectedHistoryReceiptIssuer
{
    PrimeMembraneProjectedHistoryReceiptEvaluation Issue(
        PrimeMembraneProjectedBraidInterpretation interpretation,
        string receiptHandle);
}

public sealed class PrimeMembraneProjectedHistoryReceiptIssuer : IPrimeMembraneProjectedHistoryReceiptIssuer
{
    public PrimeMembraneProjectedHistoryReceiptEvaluation Issue(
        PrimeMembraneProjectedBraidInterpretation interpretation,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(interpretation);

        var historyReceipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            interpretation.InterpretationReceipt,
            receiptHandle);

        return new PrimeMembraneProjectedHistoryReceiptEvaluation(
            Interpretation: interpretation,
            HistoryReceipt: historyReceipt,
            GovernanceTrace: "prime-membrane-projected-history-receipt-only");
    }
}
