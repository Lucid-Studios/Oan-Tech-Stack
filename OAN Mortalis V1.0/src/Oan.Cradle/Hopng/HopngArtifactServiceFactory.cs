using CradleTek.Host.Interfaces;

namespace Oan.Cradle;

public static class HopngArtifactServiceFactory
{
#if LOCAL_HDT_BRIDGE
    public static bool IsLocalBridgeCompiled => true;
#else
    public static bool IsLocalBridgeCompiled => false;
#endif

    public static IHopngArtifactService Create(string? explicitOutputRoot = null)
    {
#if LOCAL_HDT_BRIDGE
        return new LocalHdtHopngArtifactService(explicitOutputRoot);
#else
        return new UnavailableHopngArtifactService();
#endif
    }
}
