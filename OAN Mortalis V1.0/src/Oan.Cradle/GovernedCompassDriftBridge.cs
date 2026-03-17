using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedCompassDriftBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedCompassDriftBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedCompassDriftReceipt> WitnessAsync(
        string loopKey,
        CompassDriftAssessment assessment,
        GovernanceLoopStage stage,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(assessment);

        var driftHandle = CompassObservationKeys.CreateDriftHandle(
            loopKey,
            assessment.CMEId,
            assessment.DriftState,
            assessment.ObservationHandles);
        var telemetryEvent = GovernedCompassDriftTelemetry.CreateRecordedEvent(
            driftHandle,
            assessment,
            stage,
            witnessedBy: "CradleTek");
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        var receipt = new GovernedCompassDriftReceipt(
            DriftHandle: driftHandle,
            LoopKey: loopKey,
            Stage: stage,
            CMEId: assessment.CMEId,
            DriftState: assessment.DriftState,
            BaselineActiveBasin: assessment.BaselineActiveBasin,
            BaselineCompetingBasin: assessment.BaselineCompetingBasin,
            LatestActiveBasin: assessment.LatestActiveBasin,
            ObservationCount: assessment.ObservationCount,
            WindowSize: assessment.WindowSize,
            AdvisoryDivergenceCount: assessment.AdvisoryDivergenceCount,
            CompetingMigrationCount: assessment.CompetingMigrationCount,
            WitnessedBy: "CradleTek",
            ObservationHandles: assessment.ObservationHandles.ToArray(),
            TimestampUtc: assessment.TimestampUtc);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.CompassDrift,
                    stage,
                    assessment.TimestampUtc.UtcDateTime,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: reviewRequest,
                    Annotation: null,
                    HopngArtifactReceipt: null,
                    TargetWitnessReceipt: null,
                    CompassObservationReceipt: null,
                    CompassDriftReceipt: receipt),
                cancellationToken).ConfigureAwait(false);
        }

        return receipt;
    }
}
