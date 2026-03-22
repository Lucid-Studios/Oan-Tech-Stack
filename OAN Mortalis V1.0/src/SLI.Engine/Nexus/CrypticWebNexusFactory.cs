using SLI.Engine.Runtime;

namespace SLI.Engine.Nexus;

internal static class CrypticWebNexusFactory
{
    public static ICrypticWebNexus Create(SliExecutionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new ExecutionSnapshotCrypticWebNexus(snapshot);
    }
}
