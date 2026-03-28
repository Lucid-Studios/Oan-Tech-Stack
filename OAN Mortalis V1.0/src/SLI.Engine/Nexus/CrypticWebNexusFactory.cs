using SLI.Engine.Runtime;

namespace SLI.Engine.Nexus;

internal static class CrypticWebNexusFactory
{
    public static ICrypticWebNexus Create(SliExecutionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new ExecutionSnapshotCrypticWebNexus(snapshot);
    }

    public static ICrypticWebNexusPortal CreatePortal(SliExecutionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return CreatePortal(Create(snapshot));
    }

    public static ICrypticWebNexusPortal CreatePortal(ICrypticWebNexus nexus)
    {
        ArgumentNullException.ThrowIfNull(nexus);
        return new ExecutionSnapshotCrypticWebNexusPortal(nexus);
    }
}
