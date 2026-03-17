using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWorkerHandoffBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedWorkerHandoffBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedWorkerHandoffReceipt> WitnessAsync(
        string loopKey,
        WorkerHandoffPacket packet,
        GovernedWorkerHandoffReceipt receipt,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!string.Equals(loopKey, receipt.LoopKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Worker handoff receipt loop key must match the witnessed loop.");
        }

        var witnessedReceipt = receipt with
        {
            WitnessedBy = "CradleTek"
        };
        var telemetryEvent = GovernedWorkerHandoffTelemetry.CreateRecordedEvent(
            packet,
            witnessedReceipt,
            witnessedBy: witnessedReceipt.WitnessedBy);
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.WorkerHandoff,
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
                    WorkerHandoffReceipt: witnessedReceipt),
                cancellationToken).ConfigureAwait(false);
        }

        return witnessedReceipt;
    }
}
