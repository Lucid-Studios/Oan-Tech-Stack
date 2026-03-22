namespace SLI.Engine.Nexus;

internal sealed class ExecutionSnapshotCrypticWebNexusPortal : ICrypticWebNexusPortal
{
    private readonly ICrypticWebNexus _nexus;
    private readonly CrypticWebNexusPortalSurface _portalSurface;

    public ExecutionSnapshotCrypticWebNexusPortal(ICrypticWebNexus nexus)
    {
        _nexus = nexus ?? throw new ArgumentNullException(nameof(nexus));

        var topology = _nexus.CaptureTopologySnapshot();
        var mutationEvents = _nexus.CaptureMutationEvents();
        var relaxation = _nexus.CaptureRelaxationReceipt();
        var telemetry = _nexus.CaptureTelemetryFrame();
        var gateLegibility = BuildGateLegibility(topology, mutationEvents, relaxation, telemetry);

        _portalSurface = new CrypticWebNexusPortalSurface(
            PortalId: $"portal:{_nexus.TraceId}",
            NexusId: _nexus.NexusId,
            TraceId: _nexus.TraceId,
            CapturedAtUtc: topology.CapturedAtUtc,
            Topology: topology,
            MutationEvents: mutationEvents,
            Relaxation: relaxation,
            Telemetry: telemetry,
            GateLegibility: gateLegibility);
    }

    public string PortalId => _portalSurface.PortalId;

    public string NexusId => _portalSurface.NexusId;

    public string TraceId => _portalSurface.TraceId;

    public CrypticWebNexusPortalSurface CapturePortalSurface() => _portalSurface;

    private static NexusGateLegibilityReceipt BuildGateLegibility(
        WebTopologySnapshot topology,
        IReadOnlyList<MutationEvent> mutationEvents,
        RelaxationReceipt relaxation,
        NexusTelemetryFrame telemetry)
    {
        var gateState = ResolveGateState(telemetry.ReadinessState);
        var reasonCode = ResolveReasonCode(gateState);
        var visibleSignals = new List<string>
        {
            $"field-state:{topology.FieldState}",
            $"readiness:{telemetry.ReadinessState}",
            $"relaxation:{relaxation.RelaxationState}",
            $"focal-region:{telemetry.FocalRegion}",
            $"mutation-count:{mutationEvents.Count}"
        };

        if (!string.IsNullOrWhiteSpace(relaxation.BoundaryIntegrityState))
        {
            visibleSignals.Add($"boundary:{relaxation.BoundaryIntegrityState}");
        }

        IReadOnlyList<string> deniedSurfaces = gateState switch
        {
            NexusGateLegibilityState.WitnessOnly =>
            new[]
            {
                "ambient-access-grant",
                "bonded-participation-without-host-law",
                "deep-identity-bearing-descent"
            },
            NexusGateLegibilityState.AwaitingRelaxation =>
            new[]
            {
                "movement-before-relaxation",
                "ambient-access-grant",
                "bonded-participation-without-host-law"
            },
            NexusGateLegibilityState.ReadyForBoundedEngagement =>
            new[]
            {
                "ambient-access-grant",
                "host-law-bypass",
                "unwitnessed-bond-realization"
            },
            _ =>
            new[]
            {
                "ambient-access-grant",
                "bounded-engagement",
                "bonded-participation"
            }
        };

        var requiredOfficeOrState = gateState switch
        {
            NexusGateLegibilityState.WitnessOnly => "bounded-engagement-request-through-host-law",
            NexusGateLegibilityState.AwaitingRelaxation => "relaxation-to-ready-for-reentry",
            NexusGateLegibilityState.ReadyForBoundedEngagement => "office-bearing-bounded-engagement",
            NexusGateLegibilityState.Contradictory => "contradiction-resolution-before-engagement",
            _ => "host-law-adjudication"
        };

        return new NexusGateLegibilityReceipt(
            LegibilityId: $"legibility:{telemetry.FrameId}",
            CapturedAtUtc: telemetry.CapturedAtUtc,
            GateState: gateState,
            AccessGrantAuthority: "host-law-and-nexus-adjudication",
            RequiredOfficeOrState: requiredOfficeOrState,
            GrantImplied: false,
            VisibleSignals: visibleSignals,
            DeniedSurfaces: deniedSurfaces,
            ReasonCode: reasonCode);
    }

    private static NexusGateLegibilityState ResolveGateState(NexusReadinessState readinessState) =>
        readinessState switch
        {
            NexusReadinessState.DormantCoherent => NexusGateLegibilityState.WitnessOnly,
            NexusReadinessState.NotReady => NexusGateLegibilityState.AwaitingRelaxation,
            NexusReadinessState.ReadyForReentry => NexusGateLegibilityState.ReadyForBoundedEngagement,
            NexusReadinessState.Contradictory => NexusGateLegibilityState.Contradictory,
            _ => NexusGateLegibilityState.AwaitingRelaxation
        };

    private static string ResolveReasonCode(NexusGateLegibilityState gateState) =>
        gateState switch
        {
            NexusGateLegibilityState.WitnessOnly => "cryptic-nexus-portal-witness-only",
            NexusGateLegibilityState.AwaitingRelaxation => "cryptic-nexus-portal-awaiting-relaxation",
            NexusGateLegibilityState.ReadyForBoundedEngagement => "cryptic-nexus-portal-ready-for-bounded-engagement",
            NexusGateLegibilityState.Contradictory => "cryptic-nexus-portal-contradictory",
            _ => "cryptic-nexus-portal-unclassified"
        };
}
