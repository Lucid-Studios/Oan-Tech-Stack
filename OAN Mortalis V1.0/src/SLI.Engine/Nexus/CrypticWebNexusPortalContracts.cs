namespace SLI.Engine.Nexus;

public enum NexusGateLegibilityState
{
    WitnessOnly = 0,
    AwaitingRelaxation = 1,
    ReadyForBoundedEngagement = 2,
    Contradictory = 3
}

public sealed record NexusGateLegibilityReceipt(
    string LegibilityId,
    DateTime CapturedAtUtc,
    NexusGateLegibilityState GateState,
    string AccessGrantAuthority,
    string RequiredOfficeOrState,
    bool GrantImplied,
    IReadOnlyList<string> VisibleSignals,
    IReadOnlyList<string> DeniedSurfaces,
    string ReasonCode);

public sealed record CrypticWebNexusPortalSurface(
    string PortalId,
    string NexusId,
    string TraceId,
    DateTime CapturedAtUtc,
    WebTopologySnapshot Topology,
    IReadOnlyList<MutationEvent> MutationEvents,
    RelaxationReceipt Relaxation,
    NexusTelemetryFrame Telemetry,
    NexusGateLegibilityReceipt GateLegibility);
