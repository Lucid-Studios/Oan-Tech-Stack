using Oan.Common;
using Oan.Cradle;
using Oan.Storage;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedCompassObservationIntegrationTests
{
    [Fact]
    public async Task Bridge_EmitsTelemetryAndJournalsCompassObservationReceipt()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var journalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.compass-observation.ndjson");
        var journal = new NdjsonGovernanceReceiptJournal(journalPath);
        var bridge = new GovernedCompassObservationBridge(telemetrySink, journal);

        try
        {
            var observation = CreateObservation();

            var receipt = await bridge.WitnessAsync(
                "loop:test-compass-observation",
                observation,
                GovernanceLoopStage.BoundedCognitionCompleted);

            var telemetryEvent = Assert.Single(telemetrySink.Events.OfType<GovernedCompassObservationTelemetryEvent>());
            var replay = await journal.ReplayLoopAsync("loop:test-compass-observation");
            var entry = Assert.Single(replay);

            Assert.Equal("compass-observation-recorded", telemetryEvent.EventType);
            Assert.Equal(CompassDoctrineBasin.BoundedLocalityContinuity, telemetryEvent.ActiveBasin);
            Assert.Equal(CompassDoctrineBasin.FluidContinuityLaw, telemetryEvent.CompetingBasin);
            Assert.Equal(CompassObservationProvenance.Braided, telemetryEvent.Provenance);
            Assert.Equal(receipt.WitnessHandle, entry.CompassObservationReceipt!.WitnessHandle);
            Assert.Equal(GovernanceJournalEntryKind.CompassObservation, entry.Kind);
            Assert.Equal(CompassAnchorState.Held, entry.CompassObservationReceipt.AnchorState);
        }
        finally
        {
            File.Delete(journalPath);
        }
    }

    private static CompassObservationSurface CreateObservation()
    {
        return new CompassObservationSurface(
            ObservationHandle: "compass-observation://aaaaaaaaaaaaaaaa",
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            AnchorState: CompassAnchorState.Held,
            Provenance: CompassObservationProvenance.Braided,
            ObserverIdentity: "AgentiCore Compass",
            WorkingStateHandle: "soulframe-working://cme-alpha/test",
            CSelfGelHandle: "soulframe-cselfgel://cme-alpha/test",
            SelfGelHandle: "soulframe-selfgel://cme-alpha/test",
            ValidationReferenceHandle: "soulframe-selfgel://cme-alpha/test",
            Objective: "maintain bounded locality continuity",
            SeedAdvisory: new CompassSeedAdvisoryObservation(
                Accepted: true,
                Decision: "classify-ok",
                Trace: "classify-response-ready",
                Confidence: 0.71,
                Payload: "bounded-locality continuity"),
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private sealed class CapturingTelemetrySink : ITelemetrySink
    {
        public List<object> Events { get; } = [];

        public Task EmitAsync(object telemetryEvent)
        {
            Events.Add(telemetryEvent);
            return Task.CompletedTask;
        }
    }
}
