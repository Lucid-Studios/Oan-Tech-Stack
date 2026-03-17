using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedOfficeAuthorityBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedOfficeAuthorityBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedOfficeAuthorityReceipt> WitnessAsync(
        string loopKey,
        GoverningOfficeAuthorityAssessment assessment,
        GoverningOfficeAuthorityView view,
        GovernanceLoopStage stage,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(view);

        var authorityHandle = CompassObservationKeys.CreateOfficeAuthorityHandle(
            loopKey,
            assessment.CMEId,
            assessment.Office,
            assessment.ActionEligibility,
            assessment.WeatherDisclosureHandle);
        var telemetryEvent = GovernedOfficeAuthorityTelemetry.CreateRecordedEvent(
            authorityHandle,
            assessment,
            view,
            stage,
            witnessedBy: "CradleTek");
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        var receipt = new GovernedOfficeAuthorityReceipt(
            AuthorityHandle: authorityHandle,
            LoopKey: loopKey,
            Stage: stage,
            CMEId: assessment.CMEId,
            Office: assessment.Office,
            AuthoritySurface: assessment.AuthoritySurface,
            ViewEligibility: view.ViewEligibility,
            AcknowledgmentEligibility: view.AcknowledgmentEligibility,
            ActionEligibility: view.ActionEligibility,
            EvidenceSufficiencyState: assessment.EvidenceSufficiencyState,
            DisclosureScope: assessment.DisclosureScope,
            CommunityWeatherPacket: view.CommunityWeatherPacket,
            AllowedReasonCodes: view.AllowedReasonCodes.ToArray(),
            WithheldMarkers: view.WithheldMarkers.ToArray(),
            Prohibitions: view.Prohibitions.ToArray(),
            RationaleCode: view.RationaleCode,
            WitnessedBy: "CradleTek",
            WeatherDisclosureHandle: assessment.WeatherDisclosureHandle,
            TimestampUtc: view.TimestampUtc);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.OfficeAuthority,
                    stage,
                    view.TimestampUtc.UtcDateTime,
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
                    OfficeAuthorityReceipt: receipt),
                cancellationToken).ConfigureAwait(false);
        }

        return receipt;
    }
}
