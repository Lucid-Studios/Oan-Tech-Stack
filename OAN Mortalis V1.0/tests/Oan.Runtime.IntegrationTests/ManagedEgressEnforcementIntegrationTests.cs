using System.Text.Json;
using CradleTek.CognitionHost.Services;
using Oan.Common;
using Oan.Cradle;
using Oan.Runtime.Headless;
using Oan.Storage;
using SLI.Engine.Runtime;
using Xunit;

namespace Oan.Runtime.IntegrationTests;

public sealed class ManagedEgressEnforcementIntegrationTests
{
    [Fact]
    public async Task CognitionHostService_InitializeAsync_DeniedStructuralProvisioning_Throws()
    {
        var root = Path.Combine(Path.GetTempPath(), $"oan-egress-host-{Guid.NewGuid():N}");
        var configuredPaths = new Dictionary<string, string?>
        {
            ["OAN_RUNTIME_ROOT"] = Path.Combine(root, "runtime"),
            ["OAN_MODEL_PATH"] = Path.Combine(root, "model"),
            ["OAN_SELF_GEL"] = Path.Combine(root, "self-gel"),
            ["OAN_CSELF_GEL"] = Path.Combine(root, "cself-gel"),
            ["OAN_GOA"] = Path.Combine(root, "goa"),
            ["OAN_CGOA"] = Path.Combine(root, "cgoa")
        };

        using var scope = new TemporaryEnvironmentScope(configuredPaths);
        var service = new CognitionHostService(NullEgressRouter.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitializeAsync());

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.All(configuredPaths.Values, path => Assert.False(Directory.Exists(path)));
    }

    [Fact]
    public async Task HeadlessRuntimeBootstrap_CreateEvaluateHostAsync_DeniedStructuralCreation_Throws()
    {
        var runtimeRoot = Path.Combine(Path.GetTempPath(), $"oan-headless-denied-{Guid.NewGuid():N}");
        Directory.CreateDirectory(runtimeRoot);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            HeadlessRuntimeBootstrap.CreateEvaluateHostAsync(runtimeRoot, NullEgressRouter.Instance));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(Path.Combine(runtimeRoot, "public_root")));
        Assert.False(Directory.Exists(Path.Combine(runtimeRoot, "cryptic_root")));
    }

    [Fact]
    public void HeadlessRuntimeBootstrap_CreateOperatorContext_DeniedStructuralCreation_Throws()
    {
        var runtimeRoot = Path.Combine(Path.GetTempPath(), $"oan-operator-denied-{Guid.NewGuid():N}");
        Directory.CreateDirectory(runtimeRoot);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            HeadlessRuntimeBootstrap.CreateOperatorContext(runtimeRoot, NullEgressRouter.Instance));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(Path.Combine(runtimeRoot, "public_root")));
        Assert.False(Directory.Exists(Path.Combine(runtimeRoot, "cryptic_root")));
    }

    [Fact]
    public async Task PublicPlaneStore_DeniedEgress_ThrowsAndEmitsDeniedTelemetry()
    {
        var root = Path.Combine(Path.GetTempPath(), $"oan-public-store-{Guid.NewGuid():N}");
        var telemetry = new CapturingTelemetrySink();
        var store = new PublicPlaneStore(root, telemetry, NullEgressRouter.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.AppendToGELAsync("hash-public", new { value = "test" }));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(root, "GEL.ndjson")));
        Assert.Equal("Denied", ExtractTelemetryResult(Assert.Single(telemetry.Events)));
    }

    [Fact]
    public async Task CrypticPlaneStore_DeniedEgress_ThrowsAndEmitsDeniedTelemetry()
    {
        var root = Path.Combine(Path.GetTempPath(), $"oan-cryptic-store-{Guid.NewGuid():N}");
        var telemetry = new CapturingTelemetrySink();
        var store = new CrypticPlaneStore(root, telemetry, NullEgressRouter.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.AppendToCGoAAsync("hash-cryptic", new { value = "test" }));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(root, "cGoA.ndjson")));
        Assert.Equal("Denied", ExtractTelemetryResult(Assert.Single(telemetry.Events)));
    }

    [Fact]
    public async Task GovernanceReceiptJournal_DeniedEgress_ThrowsAndCreatesNoFile()
    {
        var journalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance.ndjson");
        var journal = new NdjsonGovernanceReceiptJournal(journalPath, NullEgressRouter.Instance);
        var entry = new GovernanceJournalEntry(
            LoopKey: "loop:test",
            Kind: GovernanceJournalEntryKind.State,
            Stage: GovernanceLoopStage.SourceCustodyAvailable,
            Timestamp: DateTime.UtcNow,
            DecisionReceipt: null,
            DeferredReview: null,
            ActReceipt: null,
            ReviewRequest: null,
            Annotation: null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => journal.AppendAsync(entry));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(journalPath));
    }

    [Fact]
    public async Task GovernedTargetTelemetryBridge_DeniedEgress_ThrowsAndSuppressesTelemetry()
    {
        var telemetrySink = new CapturingTelemetrySink();
        var bridge = new SliGovernedTargetTelemetryBridge(
            telemetrySink,
            egressRouter: NullEgressRouter.Instance,
            telemetryTargetSinkClass: SliEgressTargetSinkClass.MemoryJournal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bridge.WitnessHigherOrderLocalityTargetExecutionAsync(
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
                    supportsAccountabilityPacket: false)));

        Assert.Contains("denied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(telemetrySink.Events);
    }

    private static string ExtractTelemetryResult(object telemetryEvent)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(telemetryEvent));
        return json.RootElement.GetProperty("result").GetString() ?? string.Empty;
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

    private sealed class TemporaryEnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new(StringComparer.Ordinal);

        public TemporaryEnvironmentScope(IReadOnlyDictionary<string, string?> updates)
        {
            foreach (var update in updates)
            {
                _originalValues[update.Key] = Environment.GetEnvironmentVariable(update.Key);
                Environment.SetEnvironmentVariable(update.Key, update.Value);
            }
        }

        public void Dispose()
        {
            foreach (var original in _originalValues)
            {
                Environment.SetEnvironmentVariable(original.Key, original.Value);
            }
        }
    }
}
