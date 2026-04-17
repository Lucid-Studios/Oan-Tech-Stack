using System.Security.Cryptography;
using System.Text;

namespace San.Common;

public static class SanctuaryWorkbenchKeys
{
    public static string CreateSanctuaryRuntimeWorkbenchHandle(string projectSpaceId, string sessionPosture)
        => CreateHandle("sanctuary-runtime-workbench://", projectSpaceId, sessionPosture);

    public static string CreateRuntimeWorkbenchSessionLedgerHandle(string projectSpaceId, string sessionId)
        => CreateHandle("runtime-workbench-session-ledger://", projectSpaceId, sessionId);

    public static string CreateWorkbenchSessionEventHandle(string sessionId, string eventClass)
        => CreateHandle("runtime-workbench-session-event://", sessionId, eventClass);

    public static string CreateBoundaryConditionHandle(string sessionId, string boundaryClass)
        => CreateHandle("runtime-workbench-boundary-condition://", sessionId, boundaryClass);

    public static string CreateAmenableDayDreamTierHandle(string projectSpaceId, string tierClass)
        => CreateHandle("amenable-day-dream-tier://", projectSpaceId, tierClass);

    public static string CreateDayDreamCollapseReceiptHandle(string projectSpaceId, string collapseClass)
        => CreateHandle("day-dream-collapse-receipt://", projectSpaceId, collapseClass);

    public static string CreateResidueMarkerHandle(string projectSpaceId, string residueClass)
        => CreateHandle("residue-marker://", projectSpaceId, residueClass);

    public static string CreateCrypticBiadRootHandle(string projectSpaceId, string gateClass)
        => CreateHandle("cryptic-biad-root://", projectSpaceId, gateClass);

    public static string CreateSelfRootedCrypticDepthGateHandle(string biadRootHandle, string gateClass)
        => CreateHandle("self-rooted-cryptic-depth-gate://", biadRootHandle, gateClass);

    public static string CreateBoundaryConditionLedgerHandle(string projectSpaceId, string ledgerClass)
        => CreateHandle("boundary-condition-ledger://", projectSpaceId, ledgerClass);

    public static string CreateCoherenceGainWitnessReceiptHandle(string projectSpaceId, string witnessClass)
        => CreateHandle("coherence-gain-witness://", projectSpaceId, witnessClass);

    public static string CreateInquirySessionDisciplineSurfaceHandle(string projectSpaceId, string inquiryClass)
        => CreateHandle("inquiry-session-discipline-surface://", projectSpaceId, inquiryClass);

    public static string CreateCrypticDepthReturnReceiptHandle(string projectSpaceId, string returnClass)
        => CreateHandle("cryptic-depth-return-receipt://", projectSpaceId, returnClass);

    public static string CreateContinuityMarkerHandle(string projectSpaceId, string continuityClass)
        => CreateHandle("continuity-marker://", projectSpaceId, continuityClass);

    public static string CreateLocalHostSanctuaryResidencyEnvelopeHandle(string projectSpaceId, string residencyClass)
        => CreateHandle("local-host-sanctuary-residency-envelope://", projectSpaceId, residencyClass);

    public static string CreateRuntimeHabitationReadinessLedgerHandle(string projectSpaceId, string readinessClass)
        => CreateHandle("runtime-habitation-readiness-ledger://", projectSpaceId, readinessClass);

    public static string CreateBoundedInhabitationLaunchRehearsalHandle(string projectSpaceId, string launchClass)
        => CreateHandle("bounded-inhabitation-launch-rehearsal://", projectSpaceId, launchClass);

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
