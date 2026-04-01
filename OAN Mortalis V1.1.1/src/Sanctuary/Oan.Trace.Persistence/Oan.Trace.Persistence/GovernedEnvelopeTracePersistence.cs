using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace Oan.Trace.Persistence;

public sealed record GovernedCrypticPointer(
    string PointerId,
    string Location,
    string PointerProfile,
    DateTimeOffset TimestampUtc);

public sealed record GovernedGelTelemetryRecord(
    string RecordHandle,
    string EventHash,
    DateTimeOffset TimestampUtc,
    string RuntimeState,
    string DecisionCode,
    string GovernanceState,
    string PathHandle,
    string? OutboundLaneHandle,
    string DuplexPointerHandle,
    string? SanctuaryIngressReceiptHandle,
    bool ObsidianWallApplied,
    GovernedSeedIngressAccessClass IngressAccessClass,
    string? LowMindSfRouteHandle,
    GovernedSeedLowMindSfRouteKind? LowMindSfRouteKind,
    string? HighMindContextHandle,
    GovernedSeedHighMindUptakeKind? HighMindUptakeKind,
    GovernedSeedHostedLlmEmissionState? HostedLlmState,
    string? FirstRunReceiptHandle,
    string? PreGovernancePacketHandle,
    string? LocalAuthorityTraceHandle,
    string? ConstitutionalContactHandle,
    string? LocalKeypairGenesisSourceHandle,
    string? LocalKeypairGenesisHandle,
    string? FirstCrypticBraidEstablishmentHandle,
    string? FirstCrypticBraidHandle,
    string? FirstCrypticConditioningSourceHandle,
    string? FirstCrypticConditioningHandle,
    FirstRunConstitutionState? FirstRunState,
    FirstRunOperatorReadinessState? FirstRunReadinessState,
    bool? FirstRunStateProvisional,
    bool? FirstRunStateActualized,
    bool FirstRunOpalActualized);

public interface IGovernedCrypticPointerStore
{
    Task<GovernedCrypticPointer?> GetPointerAsync(string pointerId, CancellationToken cancellationToken = default);

    Task UpsertPointerAsync(GovernedCrypticPointer pointer, CancellationToken cancellationToken = default);
}

public interface IGovernedGelTelemetrySink
{
    Task AppendAsync(GovernedGelTelemetryRecord record, CancellationToken cancellationToken = default);
}

public interface IGovernedSeedEnvelopeTraceService
{
    Task<EvaluateEnvelope> TraceAsync(
        EvaluateEnvelope envelope,
        GovernedSeedEvaluationResult result,
        CancellationToken cancellationToken = default);
}

public sealed class InMemoryGovernedCrypticPointerStore : IGovernedCrypticPointerStore
{
    private readonly Dictionary<string, GovernedCrypticPointer> _pointers = new(StringComparer.Ordinal);

    public IReadOnlyCollection<GovernedCrypticPointer> Pointers => _pointers.Values.ToArray();

    public Task<GovernedCrypticPointer?> GetPointerAsync(string pointerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pointerId);
        cancellationToken.ThrowIfCancellationRequested();
        _pointers.TryGetValue(pointerId, out var pointer);
        return Task.FromResult(pointer);
    }

    public Task UpsertPointerAsync(GovernedCrypticPointer pointer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pointer);
        cancellationToken.ThrowIfCancellationRequested();
        _pointers[pointer.PointerId] = pointer;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryGovernedGelTelemetrySink : IGovernedGelTelemetrySink
{
    private readonly List<GovernedGelTelemetryRecord> _records = [];

    public IReadOnlyList<GovernedGelTelemetryRecord> Records => _records;

    public Task AppendAsync(GovernedGelTelemetryRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        cancellationToken.ThrowIfCancellationRequested();
        _records.Add(record);
        return Task.CompletedTask;
    }
}

public sealed class GovernedSeedEnvelopeTraceService : IGovernedSeedEnvelopeTraceService
{
    private readonly IGovernedCrypticPointerStore _pointerStore;
    private readonly IGovernedGelTelemetrySink _telemetrySink;

    public GovernedSeedEnvelopeTraceService(
        IGovernedCrypticPointerStore pointerStore,
        IGovernedGelTelemetrySink telemetrySink)
    {
        _pointerStore = pointerStore ?? throw new ArgumentNullException(nameof(pointerStore));
        _telemetrySink = telemetrySink ?? throw new ArgumentNullException(nameof(telemetrySink));
    }

    public async Task<EvaluateEnvelope> TraceAsync(
        EvaluateEnvelope envelope,
        GovernedSeedEvaluationResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(result);

        var outboundLaneHandle = envelope.OutboundLaneContext?.ContextHandle;
        var pointerLocation = outboundLaneHandle
            ?? envelope.OutboundObjectContext?.ContextHandle
            ?? envelope.ReturnSurfaceContext?.ContextHandle
            ?? result.VerticalSlice.PathReceipt.PathHandle;
        var pointerSeed = string.Join(
            "|",
            envelope.AgentId,
            envelope.TheaterId,
            result.Decision,
            envelope.GovernanceState ?? string.Empty,
            result.VerticalSlice.PathReceipt.PathHandle,
            pointerLocation);
        var pointerId = $"duplex://{ComputeHash(pointerSeed)}";
        var timestampUtc = DateTimeOffset.UtcNow;

        await _pointerStore.UpsertPointerAsync(
            new GovernedCrypticPointer(
                PointerId: pointerId,
                Location: pointerLocation,
                PointerProfile: "seed-envelope-outbound-trace",
                TimestampUtc: timestampUtc),
            cancellationToken).ConfigureAwait(false);

        var telemetrySeed = string.Join(
            "|",
            pointerId,
            result.Decision,
            envelope.GovernanceState ?? string.Empty,
            result.VerticalSlice.PathReceipt.PathHandle,
            outboundLaneHandle ?? "no-outbound-lane",
            result.VerticalSlice.SanctuaryIngressReceipt?.ReceiptHandle ?? "no-sanctuary-ingress",
            result.VerticalSlice.OperationalContext?.IngressAccessClass.ToString() ??
                result.VerticalSlice.SanctuaryIngressReceipt?.IngressAccessClass.ToString() ??
                GovernedSeedIngressAccessClass.PromptInput.ToString(),
            result.VerticalSlice.OperationalContext?.LowMindSfRouteKind.ToString() ?? "no-lowmind-route",
            result.VerticalSlice.OperationalContext?.HighMindUptakeKind?.ToString() ?? "no-highmind-context",
            result.VerticalSlice.OperationalContext?.HostedLlmState?.ToString() ?? "no-hosted-llm-state",
            result.VerticalSlice.OperationalContext?.PreGovernancePacketHandle ?? "no-pre-governance-packet",
            result.VerticalSlice.OperationalContext?.LocalAuthorityTraceHandle ?? "no-local-authority-trace",
            result.VerticalSlice.OperationalContext?.ConstitutionalContactHandle ?? "no-constitutional-contact",
            result.VerticalSlice.OperationalContext?.LocalKeypairGenesisSourceHandle ?? "no-local-keypair-genesis-source",
            result.VerticalSlice.OperationalContext?.LocalKeypairGenesisHandle ?? "no-local-keypair-genesis",
            result.VerticalSlice.OperationalContext?.FirstCrypticBraidEstablishmentHandle ?? "no-first-cryptic-braid-establishment",
            result.VerticalSlice.OperationalContext?.FirstCrypticBraidHandle ?? "no-first-cryptic-braid",
            result.VerticalSlice.OperationalContext?.FirstCrypticConditioningSourceHandle ?? "no-first-cryptic-conditioning-source",
            result.VerticalSlice.OperationalContext?.FirstCrypticConditioningHandle ?? "no-first-cryptic-conditioning",
            result.VerticalSlice.OperationalContext?.FirstRunState?.ToString() ?? "no-first-run-state",
            result.VerticalSlice.OperationalContext?.FirstRunReadinessState?.ToString() ?? "no-first-run-readiness",
            (result.VerticalSlice.OperationalContext?.FirstRunOpalActualized ?? false).ToString());
        var sanctuaryIngressReceipt = result.VerticalSlice.SanctuaryIngressReceipt;
        var operationalContext = result.VerticalSlice.OperationalContext;
        var telemetryRecord = new GovernedGelTelemetryRecord(
            RecordHandle: $"telemetry://{ComputeHash($"{pointerId}|record")}",
            EventHash: ComputeHash(telemetrySeed),
            TimestampUtc: timestampUtc,
            RuntimeState: $"{(envelope.GovernanceState ?? "unknown").ToLowerInvariant()}/{result.Decision}",
            DecisionCode: result.Decision,
            GovernanceState: envelope.GovernanceState ?? GovernedSeedEvaluationStateTokens.ToToken(result.GovernanceState),
            PathHandle: result.VerticalSlice.PathReceipt.PathHandle,
            OutboundLaneHandle: outboundLaneHandle,
            DuplexPointerHandle: pointerId,
            SanctuaryIngressReceiptHandle: sanctuaryIngressReceipt?.ReceiptHandle,
            ObsidianWallApplied: sanctuaryIngressReceipt?.ObsidianWallApplied ?? false,
            IngressAccessClass: operationalContext?.IngressAccessClass ??
                sanctuaryIngressReceipt?.IngressAccessClass ??
                GovernedSeedIngressAccessClass.PromptInput,
            LowMindSfRouteHandle: operationalContext?.LowMindSfRouteHandle,
            LowMindSfRouteKind: operationalContext?.LowMindSfRouteKind,
            HighMindContextHandle: operationalContext?.HighMindContextHandle,
            HighMindUptakeKind: operationalContext?.HighMindUptakeKind,
            HostedLlmState: operationalContext?.HostedLlmState,
            FirstRunReceiptHandle: operationalContext?.FirstRunReceiptHandle,
            PreGovernancePacketHandle: operationalContext?.PreGovernancePacketHandle,
            LocalAuthorityTraceHandle: operationalContext?.LocalAuthorityTraceHandle,
            ConstitutionalContactHandle: operationalContext?.ConstitutionalContactHandle,
            LocalKeypairGenesisSourceHandle: operationalContext?.LocalKeypairGenesisSourceHandle,
            LocalKeypairGenesisHandle: operationalContext?.LocalKeypairGenesisHandle,
            FirstCrypticBraidEstablishmentHandle: operationalContext?.FirstCrypticBraidEstablishmentHandle,
            FirstCrypticBraidHandle: operationalContext?.FirstCrypticBraidHandle,
            FirstCrypticConditioningSourceHandle: operationalContext?.FirstCrypticConditioningSourceHandle,
            FirstCrypticConditioningHandle: operationalContext?.FirstCrypticConditioningHandle,
            FirstRunState: operationalContext?.FirstRunState,
            FirstRunReadinessState: operationalContext?.FirstRunReadinessState,
            FirstRunStateProvisional: operationalContext?.FirstRunStateProvisional,
            FirstRunStateActualized: operationalContext?.FirstRunStateActualized,
            FirstRunOpalActualized: operationalContext?.FirstRunOpalActualized ?? false);

        await _telemetrySink.AppendAsync(telemetryRecord, cancellationToken).ConfigureAwait(false);

        envelope.DuplexResponseHash = pointerId;
        return envelope;
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
