using San.Common;

namespace SLI.Engine;

public sealed record CommunicativeFilamentEvaluation(
    PrimeRetainedWholeEvaluation RetainedWholeEvaluation,
    CommunicativeCarrierPacket CarrierPacket,
    CommunicativeFilamentResolutionReceipt ResolutionReceipt,
    string GovernanceTrace);

public interface ICommunicativeFilamentIssuer
{
    CommunicativeFilamentEvaluation Issue(
        PrimeRetainedWholeEvaluation retainedWholeEvaluation,
        CommunicativeCarrierClassKind carrierClass,
        string carrierHandle,
        string receiptHandle,
        bool selfEchoDetected = false,
        bool gelSubstanceConsumptionRequested = false,
        IReadOnlyList<string>? pointerHandles = null,
        IReadOnlyList<string>? symbolicCarrierHandles = null,
        IReadOnlyList<string>? notes = null);
}

public sealed class CommunicativeFilamentIssuer : ICommunicativeFilamentIssuer
{
    public CommunicativeFilamentEvaluation Issue(
        PrimeRetainedWholeEvaluation retainedWholeEvaluation,
        CommunicativeCarrierClassKind carrierClass,
        string carrierHandle,
        string receiptHandle,
        bool selfEchoDetected = false,
        bool gelSubstanceConsumptionRequested = false,
        IReadOnlyList<string>? pointerHandles = null,
        IReadOnlyList<string>? symbolicCarrierHandles = null,
        IReadOnlyList<string>? notes = null)
    {
        ArgumentNullException.ThrowIfNull(retainedWholeEvaluation);

        if (string.IsNullOrWhiteSpace(carrierHandle))
        {
            throw new ArgumentException("Carrier handle must be provided.", nameof(carrierHandle));
        }

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var retainedHistory = retainedWholeEvaluation.RetainedHistoryRecord;
        var carrierPacket = new CommunicativeCarrierPacket(
            CarrierHandle: carrierHandle,
            SourceRetainedWholeRecordHandle: retainedHistory.RecordHandle,
            SourceRetainedWholeKind: retainedHistory.RetainedWholeKind,
            CarrierClass: carrierClass,
            PreservedDistinctionVisible: retainedHistory.PreservedDistinctionVisible,
            SelfEchoDetected: selfEchoDetected,
            GelSubstanceConsumptionRequested: gelSubstanceConsumptionRequested,
            PointerHandles: NormalizeTokens(pointerHandles, retainedHistory.RecordHandle, retainedHistory.MembraneHistoryReceiptHandle, retainedHistory.HistoryHandle),
            SymbolicCarrierHandles: NormalizeTokens(symbolicCarrierHandles, retainedHistory.ProjectionHandle, retainedHistory.MembraneHandle),
            PreservedResidues: retainedHistory.RetainedResidues,
            DeferredResidues: retainedHistory.UnresolvedResidues,
            Notes: NormalizeTokens(notes, "communicative-filament-issued-from-prime-retained-history"),
            TimestampUtc: retainedHistory.TimestampUtc);

        var resolutionReceipt = CommunicativeFilamentEvaluator.Evaluate(
            carrierPacket,
            receiptHandle);

        return new CommunicativeFilamentEvaluation(
            RetainedWholeEvaluation: retainedWholeEvaluation,
            CarrierPacket: carrierPacket,
            ResolutionReceipt: resolutionReceipt,
            GovernanceTrace: "communicative-filament-resolution-only");
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens,
        params string[] defaults)
    {
        return (tokens ?? defaults)
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
