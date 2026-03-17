using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWeatherDisclosureBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedWeatherDisclosureBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedWeatherDisclosureReceipt> WitnessAsync(
        string loopKey,
        StewardCareAssessment assessment,
        WeatherDisclosureDecision decision,
        GovernanceLoopStage stage,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(decision);

        var disclosureHandle = CompassObservationKeys.CreateWeatherDisclosureHandle(
            loopKey,
            assessment.CMEId,
            assessment.RoutingState,
            decision.DisclosureScope,
            decision.InnerWeatherHandle);
        var telemetryEvent = GovernedWeatherDisclosureTelemetry.CreateRecordedEvent(
            disclosureHandle,
            assessment,
            decision,
            stage,
            witnessedBy: "CradleTek");
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        var receipt = new GovernedWeatherDisclosureReceipt(
            DisclosureHandle: disclosureHandle,
            LoopKey: loopKey,
            Stage: stage,
            CMEId: assessment.CMEId,
            RoutingState: assessment.RoutingState,
            CadenceState: assessment.CadenceState,
            EvidenceSufficiencyState: assessment.EvidenceSufficiencyState,
            WindowIntegrityState: assessment.WindowIntegrityState,
            DisclosureScope: decision.DisclosureScope,
            CommunityWeatherPacket: decision.CommunityWeatherPacket,
            AllowedCommunityFields: decision.AllowedCommunityFields.ToArray(),
            StewardReasonCodes: decision.StewardReasonCodes.ToArray(),
            WithheldMarkers: decision.WithheldMarkers.ToArray(),
            RationaleCode: decision.RationaleCode,
            WitnessedBy: "CradleTek",
            InnerWeatherHandle: decision.InnerWeatherHandle,
            TimestampUtc: decision.TimestampUtc);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.WeatherDisclosure,
                    stage,
                    decision.TimestampUtc.UtcDateTime,
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
                    WeatherDisclosureReceipt: receipt),
                cancellationToken).ConfigureAwait(false);
        }

        return receipt;
    }
}
