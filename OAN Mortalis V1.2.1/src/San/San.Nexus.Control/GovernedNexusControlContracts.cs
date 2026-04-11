using San.Common;

namespace San.Nexus.Control;

public enum NexusGovernanceOffice
{
    SanctuaryGoa = 0,
    CradleTekGoa = 1,
    CGoa = 2,
    Cme = 3,
    SanctuaryRtme = 4
}

public enum NexusSurfaceState
{
    Template = 0,
    Issued = 1,
    CrypticDerived = 2
}

public enum NexusConnectionDisposition
{
    Denied = 0,
    Admitted = 1
}

public enum CmeSpawnDisposition
{
    Denied = 0,
    Admitted = 1
}

public sealed record IssuedGovernanceSurface(
    string SurfaceHandle,
    NexusGovernanceOffice Office,
    NexusSurfaceState SurfaceState,
    string HashLineage,
    string? ParentIssuedHandle,
    bool Certified);

public sealed record NexusDownwardConnectionRequest(
    string RequestHandle,
    IssuedGovernanceSurface Source,
    IssuedGovernanceSurface Target,
    DateTimeOffset TimestampUtc);

public sealed record NexusDownwardConnectionDecision(
    string DecisionHandle,
    NexusConnectionDisposition Disposition,
    AdmissibilityStatus Admissibility,
    string Reason,
    DateTimeOffset TimestampUtc);

public sealed record NexusDownwardConnectionEvaluation(
    NexusDownwardConnectionRequest Request,
    NexusDownwardConnectionDecision Decision);

public sealed record CmeSpawnRequest(
    string RequestHandle,
    IssuedGovernanceSurface CradleTekGoa,
    string CGoaHandle,
    string CmeHandle,
    bool FreshCGoaRequired,
    DateTimeOffset TimestampUtc);

public sealed record CmeSpawnDecision(
    string DecisionHandle,
    CmeSpawnDisposition Disposition,
    AdmissibilityStatus Admissibility,
    string Reason,
    DateTimeOffset TimestampUtc);

public sealed record CmeSpawnEvaluation(
    CmeSpawnRequest Request,
    CmeSpawnDecision Decision);
