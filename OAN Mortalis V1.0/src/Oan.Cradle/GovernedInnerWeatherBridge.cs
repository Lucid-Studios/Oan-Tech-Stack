using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedInnerWeatherBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public GovernedInnerWeatherBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task<GovernedInnerWeatherReceipt> WitnessAsync(
        string loopKey,
        InnerWeatherEvidence evidence,
        GovernanceLoopStage stage,
        ReturnCandidateReviewRequest? reviewRequest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(evidence);

        var innerWeatherHandle = CompassObservationKeys.CreateInnerWeatherHandle(
            loopKey,
            evidence.CMEId,
            evidence.WindowIntegrityState,
            evidence.DriftState,
            evidence.ObservationHandles);
        var telemetryEvent = GovernedInnerWeatherTelemetry.CreateRecordedEvent(
            innerWeatherHandle,
            evidence,
            stage,
            witnessedBy: "CradleTek");
        await _governanceTelemetry.EmitAsync(telemetryEvent).ConfigureAwait(false);

        var receipt = new GovernedInnerWeatherReceipt(
            InnerWeatherHandle: innerWeatherHandle,
            LoopKey: loopKey,
            Stage: stage,
            CMEId: evidence.CMEId,
            ActiveBasin: evidence.ActiveBasin,
            CompetingBasin: evidence.CompetingBasin,
            DriftState: evidence.DriftState,
            WindowIntegrityState: evidence.WindowIntegrityState,
            ObservationCount: evidence.ObservationCount,
            WindowSize: evidence.WindowSize,
            ResidueState: evidence.Residue.ResidueState,
            ResidueVisibilityClass: evidence.Residue.VisibilityClass,
            ResidueContributors: evidence.Residue.Contributors.ToArray(),
            ShellCompetitionState: evidence.ShellCompetition.CompetitionState,
            ShellCompetitionVisibilityClass: evidence.ShellCompetition.VisibilityClass,
            HotCoolContactState: evidence.HotCoolContactState,
            HotCoolContactVisibilityClass: evidence.HotCoolContactVisibilityClass,
            StewardAttentionCauses: evidence.StewardAttentionCauses.ToArray(),
            WitnessedBy: "CradleTek",
            DriftHandle: evidence.DriftHandle,
            ObservationHandles: evidence.ObservationHandles.ToArray(),
            TimestampUtc: evidence.TimestampUtc);

        if (_governanceReceiptJournal is not null)
        {
            await _governanceReceiptJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.InnerWeather,
                    stage,
                    evidence.TimestampUtc.UtcDateTime,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: reviewRequest,
                    Annotation: null,
                    HopngArtifactReceipt: null,
                    TargetWitnessReceipt: null,
                    CompassObservationReceipt: null,
                    CompassDriftReceipt: null,
                    InnerWeatherReceipt: receipt),
                cancellationToken).ConfigureAwait(false);
        }

        return receipt;
    }
}
