using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Nexus.Control;

public interface IGovernedNexusControlService
{
    NexusDownwardConnectionEvaluation EvaluateDownwardConnection(
        IssuedGovernanceSurface sanctuaryGoa,
        IssuedGovernanceSurface cradleTekGoa);

    CmeSpawnEvaluation EvaluateCmeSpawn(
        IssuedGovernanceSurface cradleTekGoa,
        string cGoaHandle,
        string cmeHandle,
        bool cGoaIsFresh);
}

public sealed class DefaultGovernedNexusControlService : IGovernedNexusControlService
{
    public NexusDownwardConnectionEvaluation EvaluateDownwardConnection(
        IssuedGovernanceSurface sanctuaryGoa,
        IssuedGovernanceSurface cradleTekGoa)
    {
        ArgumentNullException.ThrowIfNull(sanctuaryGoa);
        ArgumentNullException.ThrowIfNull(cradleTekGoa);

        var request = new NexusDownwardConnectionRequest(
            RequestHandle: CreateHandle("nexus-connection-request://", sanctuaryGoa.SurfaceHandle, cradleTekGoa.SurfaceHandle),
            Source: sanctuaryGoa,
            Target: cradleTekGoa,
            TimestampUtc: DateTimeOffset.UtcNow);

        var admitted =
            sanctuaryGoa.Office == NexusGovernanceOffice.SanctuaryGoa &&
            cradleTekGoa.Office == NexusGovernanceOffice.CradleTekGoa &&
            sanctuaryGoa.SurfaceState == NexusSurfaceState.Issued &&
            cradleTekGoa.SurfaceState == NexusSurfaceState.Issued &&
            sanctuaryGoa.Certified &&
            cradleTekGoa.Certified &&
            string.Equals(cradleTekGoa.ParentIssuedHandle, sanctuaryGoa.SurfaceHandle, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(sanctuaryGoa.HashLineage) &&
            !string.IsNullOrWhiteSpace(cradleTekGoa.HashLineage);

        var decision = new NexusDownwardConnectionDecision(
            DecisionHandle: CreateHandle("nexus-connection-decision://", sanctuaryGoa.SurfaceHandle, cradleTekGoa.SurfaceHandle),
            Disposition: admitted ? NexusConnectionDisposition.Admitted : NexusConnectionDisposition.Denied,
            Admissibility: admitted ? AdmissibilityStatus.Admissible : AdmissibilityStatus.Refused,
            Reason: admitted
                ? "issued-goa-downward-connection-admitted"
                : "downward-connection-preconditions-unsatisfied",
            TimestampUtc: DateTimeOffset.UtcNow);

        return new NexusDownwardConnectionEvaluation(request, decision);
    }

    public CmeSpawnEvaluation EvaluateCmeSpawn(
        IssuedGovernanceSurface cradleTekGoa,
        string cGoaHandle,
        string cmeHandle,
        bool cGoaIsFresh)
    {
        ArgumentNullException.ThrowIfNull(cradleTekGoa);

        var request = new CmeSpawnRequest(
            RequestHandle: CreateHandle("cme-spawn-request://", cradleTekGoa.SurfaceHandle, cGoaHandle, cmeHandle),
            CradleTekGoa: cradleTekGoa,
            CGoaHandle: cGoaHandle,
            CmeHandle: cmeHandle,
            FreshCGoaRequired: true,
            TimestampUtc: DateTimeOffset.UtcNow);

        var admitted =
            cradleTekGoa.Office == NexusGovernanceOffice.CradleTekGoa &&
            cradleTekGoa.SurfaceState == NexusSurfaceState.Issued &&
            cradleTekGoa.Certified &&
            !string.IsNullOrWhiteSpace(cradleTekGoa.HashLineage) &&
            !string.IsNullOrWhiteSpace(cGoaHandle) &&
            !string.IsNullOrWhiteSpace(cmeHandle) &&
            cGoaIsFresh;

        var decision = new CmeSpawnDecision(
            DecisionHandle: CreateHandle("cme-spawn-decision://", cradleTekGoa.SurfaceHandle, cGoaHandle, cmeHandle),
            Disposition: admitted ? CmeSpawnDisposition.Admitted : CmeSpawnDisposition.Denied,
            Admissibility: admitted ? AdmissibilityStatus.Admissible : AdmissibilityStatus.Refused,
            Reason: admitted
                ? "fresh-cgoa-cme-spawn-admitted"
                : "cme-spawn-preconditions-unsatisfied",
            TimestampUtc: DateTimeOffset.UtcNow);

        return new CmeSpawnEvaluation(request, decision);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part?.Trim() ?? string.Empty));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
