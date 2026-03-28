namespace SLI.Engine.Nexus;

public interface ICrypticWebNexusPortal
{
    string PortalId { get; }
    string NexusId { get; }
    string TraceId { get; }

    CrypticWebNexusPortalSurface CapturePortalSurface();
}
