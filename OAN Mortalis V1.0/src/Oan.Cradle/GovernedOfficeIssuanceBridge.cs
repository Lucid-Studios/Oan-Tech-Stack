using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedOfficeIssuanceBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedOfficeIssuanceBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedOfficeIssuanceReceipt> WitnessAsync(
        string loopKey,
        IssuedOfficePackage package,
        GovernedOfficeIssuanceReceipt receipt,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!string.Equals(loopKey, receipt.LoopKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Office issuance receipt loop key must match the witnessed loop.");
        }

        var witnessedReceipt = receipt with
        {
            WitnessedBy = "CradleTek"
        };
        var telemetryEvent = GovernedOfficeIssuanceTelemetry.CreateRecordedEvent(
            package,
            witnessedReceipt,
            witnessedBy: witnessedReceipt.WitnessedBy);
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.OfficeIssuance,
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
                    OfficeIssuanceReceipt: witnessedReceipt),
                cancellationToken).ConfigureAwait(false);
        }

        return witnessedReceipt;
    }
}
