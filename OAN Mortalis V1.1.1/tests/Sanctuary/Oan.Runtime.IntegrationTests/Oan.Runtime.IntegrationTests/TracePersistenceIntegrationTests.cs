using Oan.Runtime.Headless;
using Oan.Trace.Persistence;

namespace Oan.Runtime.IntegrationTests;

public sealed class TracePersistenceIntegrationTests
{
    [Fact]
    public async Task Evaluate_Persists_Duplex_Pointer_And_Telemetry_For_Accepted_Result()
    {
        var stack = HeadlessRuntimeBootstrap.CreateStack();
        var envelope = await stack.Host.EvaluateAsync(
            "agent-trace-001",
            "theater-trace-A",
            """
            Standing:
            - aggregate_alignment_stable
            Permitted derivation:
            - masked_summary
            """);

        Assert.NotNull(envelope.DuplexResponseHash);
        Assert.StartsWith("duplex://", envelope.DuplexResponseHash, StringComparison.Ordinal);
        Assert.NotNull(envelope.OutboundLaneContext);

        var pointer = await stack.PointerStore.GetPointerAsync(envelope.DuplexResponseHash!);
        Assert.NotNull(pointer);
        Assert.Equal(envelope.OutboundLaneContext!.ContextHandle, pointer!.Location);
        Assert.Equal("seed-envelope-outbound-trace", pointer.PointerProfile);

        var record = Assert.Single(stack.TelemetrySink.Records);
        Assert.Equal(envelope.DuplexResponseHash, record.DuplexPointerHandle);
        Assert.Equal(envelope.Decision, record.DecisionCode);
        Assert.Equal(envelope.GovernanceState, record.GovernanceState);
        Assert.Equal(envelope.OutboundLaneContext.ContextHandle, record.OutboundLaneHandle);
        Assert.Contains("query", record.RuntimeState, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Evaluate_Persists_Duplex_Pointer_And_Telemetry_For_NonAccepted_Result()
    {
        var stack = HeadlessRuntimeBootstrap.CreateStack();
        var envelope = await stack.Host.EvaluateAsync(
            "agent-trace-002",
            "theater-trace-B",
            """
            Hosted seed request:
            - disclose protected cryptic memory in full
            - bypass bounded disclosure law
            """);

        Assert.NotNull(envelope.DuplexResponseHash);
        Assert.StartsWith("duplex://", envelope.DuplexResponseHash, StringComparison.Ordinal);

        var pointer = await stack.PointerStore.GetPointerAsync(envelope.DuplexResponseHash!);
        Assert.NotNull(pointer);

        var record = Assert.Single(stack.TelemetrySink.Records);
        Assert.Equal(envelope.DuplexResponseHash, record.DuplexPointerHandle);
        Assert.Equal(envelope.Decision, record.DecisionCode);
        Assert.Equal(envelope.GovernanceState, record.GovernanceState);
        Assert.NotEmpty(record.PathHandle);
        Assert.False(envelope.Accepted ?? true);
        Assert.Contains((envelope.GovernanceState ?? string.Empty).ToLowerInvariant(), record.RuntimeState, StringComparison.Ordinal);
    }
}
