using San.Common;

namespace SLI.Engine;

public sealed record PrimeRetainedWholeEvaluation(
    PrimeMembraneProjectedHistoryReceiptEvaluation HistoryReceiptEvaluation,
    PrimeRetainedHistoryRecord RetainedHistoryRecord,
    string GovernanceTrace);

public interface IPrimeRetainedWholeIssuer
{
    PrimeRetainedWholeEvaluation Evaluate(
        PrimeMembraneProjectedHistoryReceiptEvaluation historyReceiptEvaluation,
        string recordHandle);
}

public sealed class PrimeRetainedWholeIssuer : IPrimeRetainedWholeIssuer
{
    public PrimeRetainedWholeEvaluation Evaluate(
        PrimeMembraneProjectedHistoryReceiptEvaluation historyReceiptEvaluation,
        string recordHandle)
    {
        ArgumentNullException.ThrowIfNull(historyReceiptEvaluation);

        var retainedHistoryRecord = PrimeRetainedWholeEvaluator.Evaluate(
            historyReceiptEvaluation.HistoryReceipt,
            recordHandle);

        return new PrimeRetainedWholeEvaluation(
            HistoryReceiptEvaluation: historyReceiptEvaluation,
            RetainedHistoryRecord: retainedHistoryRecord,
            GovernanceTrace: "prime-retained-whole-evaluation-only");
    }
}
