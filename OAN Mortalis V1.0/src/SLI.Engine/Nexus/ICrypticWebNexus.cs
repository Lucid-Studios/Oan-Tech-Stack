namespace SLI.Engine.Nexus;

public interface ICrypticWebNexus
{
    string NexusId { get; }
    string TraceId { get; }

    WebTopologySnapshot CaptureTopologySnapshot();
    IReadOnlyList<MutationEvent> CaptureMutationEvents();
    RelaxationReceipt CaptureRelaxationReceipt();
    NexusTelemetryFrame CaptureTelemetryFrame();
}
