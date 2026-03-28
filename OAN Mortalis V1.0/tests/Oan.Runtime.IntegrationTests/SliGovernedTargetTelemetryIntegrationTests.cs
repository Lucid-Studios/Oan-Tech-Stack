using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using SLI.Engine.Runtime;
using SLI.Engine.Telemetry;

namespace Oan.Runtime.IntegrationTests;

public sealed class SliGovernedTargetTelemetryIntegrationTests
{
    [Fact]
    public async Task CradleTekBridge_EmitsSuccessTelemetryForHigherOrderLocalityTargetExecution()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var bridge = new SliGovernedTargetTelemetryBridge(telemetrySink, egressRouter: new TestPermissiveEgressRouter());

        await bridge.WitnessHigherOrderLocalityTargetExecutionAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ],
            "identity-continuity",
            runtimeId: "gc-locality-runtime",
            realizationProfile: SliRuntimeRealizationProfile.CreateTargetBounded(
                profileId: "gc-locality-profile",
                supportsHigherOrderLocality: true,
                supportsBoundedRehearsal: false,
                supportsBoundedWitness: false,
                supportsBoundedTransport: false,
                supportsAdmissibleSurface: false,
                supportsAccountabilityPacket: false));

        var events = telemetrySink.Events.OfType<SliTargetExecutionTelemetryEvent>().ToArray();

        Assert.Equal(2, events.Length);
        Assert.Equal("sli-target-admission-accepted", events[0].EventType);
        Assert.Equal("CradleTek", events[0].WitnessedBy);
        Assert.Equal("gc-locality-runtime", events[0].RuntimeId);
        Assert.Equal("gc-locality-profile", events[0].ProfileId);
        Assert.Equal("sli-target-lineage-recorded", events[1].EventType);
        Assert.NotNull(events[1].LineageHandle);
        Assert.NotNull(events[1].TraceHandle);
        Assert.NotNull(events[1].ResidueHandle);
    }

    [Fact]
    public async Task CradleTekBridge_JournalsWitnessReceiptsWhenGovernanceContextIsPresent()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var journalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.target-witness.ndjson");
        var journal = new NdjsonGovernanceReceiptJournal(journalPath, new TestPermissiveEgressRouter());
        var bridge = new SliGovernedTargetTelemetryBridge(telemetrySink, journal, new TestPermissiveEgressRouter());

        try
        {
            await bridge.WitnessHigherOrderLocalityTargetExecutionAsync(
                [
                    "(locality-bootstrap context cme-self task-objective identity-continuity)",
                    "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                    "(participation-bounded-cme locality-state)"
                ],
                "identity-continuity",
                runtimeId: "gc-locality-runtime",
                realizationProfile: SliRuntimeRealizationProfile.CreateTargetBounded(
                    profileId: "gc-locality-profile",
                    supportsHigherOrderLocality: true,
                    supportsBoundedRehearsal: false,
                    supportsBoundedWitness: false,
                    supportsBoundedTransport: false,
                    supportsAdmissibleSurface: false,
                    supportsAccountabilityPacket: false),
                journalContext: new SliGovernedTargetWitnessJournalContext(
                    "loop:test-target-witness",
                    GovernanceLoopStage.BoundedCognitionCompleted));

            var replay = await journal.ReplayLoopAsync("loop:test-target-witness");

            Assert.Equal(2, replay.Count);
            Assert.All(replay, entry => Assert.Equal(GovernanceJournalEntryKind.TargetWitness, entry.Kind));
            Assert.Equal(GovernedTargetWitnessKind.AdmissionAccepted, replay[0].TargetWitnessReceipt!.Kind);
            Assert.Equal(GovernedTargetWitnessKind.LineageRecorded, replay[1].TargetWitnessReceipt!.Kind);
            Assert.NotNull(replay[1].TargetWitnessReceipt!.LineageHandle);
        }
        finally
        {
            File.Delete(journalPath);
        }
    }

    [Fact]
    public async Task CradleTekBridge_EmitsRefusalTelemetryForMissingTargetSupport()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var bridge = new SliGovernedTargetTelemetryBridge(telemetrySink, egressRouter: new TestPermissiveEgressRouter());

        await Assert.ThrowsAsync<SliTargetLaneRefusalException>(() =>
            bridge.WitnessHigherOrderLocalityTargetExecutionAsync(
                [
                    "(locality-bootstrap context cme-self task-objective identity-continuity)",
                    "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                    "(participation-bounded-cme locality-state)"
                ],
                "identity-continuity",
                supportedOpcodes: Array.Empty<string>()));

        var telemetryEvent = Assert.Single(telemetrySink.Events.OfType<SliTargetExecutionTelemetryEvent>());

        Assert.Equal("sli-target-admission-refused", telemetryEvent.EventType);
        Assert.False(telemetryEvent.Accepted);
        Assert.Equal("CradleTek", telemetryEvent.WitnessedBy);
        Assert.Contains("missing-capability", telemetryEvent.ReasonFamilies);
        Assert.Contains("profile-violation", telemetryEvent.ReasonFamilies);
        Assert.Null(telemetryEvent.LineageHandle);
    }

    [Fact]
    public async Task CradleTekBridge_JournalsRefusalWitnessReceiptBeforeThrowing()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var journalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.target-witness-refusal.ndjson");
        var journal = new NdjsonGovernanceReceiptJournal(journalPath, new TestPermissiveEgressRouter());
        var bridge = new SliGovernedTargetTelemetryBridge(telemetrySink, journal, new TestPermissiveEgressRouter());

        try
        {
            await Assert.ThrowsAsync<SliTargetLaneRefusalException>(() =>
                bridge.WitnessHigherOrderLocalityTargetExecutionAsync(
                    [
                        "(locality-bootstrap context cme-self task-objective identity-continuity)",
                        "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                        "(participation-bounded-cme locality-state)"
                    ],
                    "identity-continuity",
                    supportedOpcodes: Array.Empty<string>(),
                    journalContext: new SliGovernedTargetWitnessJournalContext(
                        "loop:test-target-refusal",
                        GovernanceLoopStage.BoundedCognitionCompleted)));

            var replay = await journal.ReplayLoopAsync("loop:test-target-refusal");
            var entry = Assert.Single(replay);

            Assert.Equal(GovernanceJournalEntryKind.TargetWitness, entry.Kind);
            Assert.Equal(GovernedTargetWitnessKind.AdmissionRefused, entry.TargetWitnessReceipt!.Kind);
            Assert.Contains("missing-capability", entry.TargetWitnessReceipt.ReasonFamilies);
            Assert.Contains("profile-violation", entry.TargetWitnessReceipt.ReasonFamilies);
        }
        finally
        {
            File.Delete(journalPath);
        }
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
