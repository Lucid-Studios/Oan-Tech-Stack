using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWorkerReturnBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedWorkerReturnBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedWorkerReturnReceipt> WitnessAsync(
        string loopKey,
        GovernedWorkerReturnReceipt receipt,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!string.Equals(loopKey, receipt.LoopKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Worker return receipt loop key must match the witnessed loop.");
        }

        var witnessedReceipt = receipt with
        {
            WitnessedBy = "CradleTek"
        };
        var telemetryEvent = GovernedWorkerReturnTelemetry.CreateRecordedEvent(
            witnessedReceipt,
            witnessedBy: witnessedReceipt.WitnessedBy);
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.WorkerReturn,
                    witnessedReceipt.Stage,
                    witnessedReceipt.TimestampUtc.UtcDateTime,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: reviewRequest,
                    Annotation: null,
                    HopngArtifactReceipt: null,
                    TargetWitnessReceipt: null,
                    CompassObservationReceipt: null,
                    CompassDriftReceipt: null,
                    InnerWeatherReceipt: null,
                    WeatherDisclosureReceipt: null,
                    OfficeAuthorityReceipt: null,
                    OfficeIssuanceReceipt: null,
                    WorkerHandoffReceipt: null,
                    WorkerReturnReceipt: witnessedReceipt),
                cancellationToken).ConfigureAwait(false);
        }

        return witnessedReceipt;
    }
}
