using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedCompassObservationBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedCompassObservationBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedCompassObservationReceipt> WitnessAsync(
        string loopKey,
        CompassObservationSurface observation,
        GovernanceLoopStage stage,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(observation);

        var telemetryEvent = GovernedCompassObservationTelemetry.CreateRecordedEvent(
            observation,
            stage,
            witnessedBy: "CradleTek");
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        var receipt = new GovernedCompassObservationReceipt(
            WitnessHandle: CompassObservationKeys.CreateWitnessHandle(loopKey, observation.ObservationHandle),
            Stage: stage,
            ObservationHandle: observation.ObservationHandle,
            ActiveBasin: observation.ActiveBasin,
            CompetingBasin: observation.CompetingBasin,
            OeCoePosture: observation.OeCoePosture,
            SelfTouchClass: observation.SelfTouchClass,
            AnchorState: observation.AnchorState,
            Provenance: observation.Provenance,
            WitnessedBy: "CradleTek",
            WorkingStateHandle: observation.WorkingStateHandle,
            CSelfGelHandle: observation.CSelfGelHandle,
            SelfGelHandle: observation.SelfGelHandle,
            ValidationReferenceHandle: observation.ValidationReferenceHandle,
            Objective: observation.Objective,
            AdvisoryAccepted: observation.SeedAdvisory?.Accepted,
            AdvisoryDecision: observation.SeedAdvisory?.Decision,
            AdvisoryTrace: observation.SeedAdvisory?.Trace,
            AdvisoryConfidence: observation.SeedAdvisory?.Confidence,
            TimestampUtc: observation.TimestampUtc);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.CompassObservation,
                    stage,
                    observation.TimestampUtc.UtcDateTime,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: reviewRequest,
                    Annotation: null,
                    HopngArtifactReceipt: null,
                    TargetWitnessReceipt: null,
                    CompassObservationReceipt: receipt),
                cancellationToken).ConfigureAwait(false);
        }

        return receipt;
    }
}
