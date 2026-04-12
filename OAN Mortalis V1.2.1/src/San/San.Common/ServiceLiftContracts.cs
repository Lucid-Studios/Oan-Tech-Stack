namespace San.Common;

public sealed record ServiceLiftRefusalReason(
    string ReasonCode,
    string Summary,
    bool RequiresExplicitReceipt);

public sealed record RtmeServiceLiftPreconditionSnapshot(
    string SnapshotId,
    string ReleaseGatePosture,
    string IssuedRtmeHandle,
    string HostedBundleHandle,
    string HostedBundleProfile,
    bool CanonicalFloorSetReady,
    string PrimeWitnessStanding,
    string CrypticResidencyStanding,
    IReadOnlyList<ServiceLiftRefusalReason> RefusalReasons);

public sealed record RtmeServiceLiftAuthorityRecord(
    string AuthorityRecordId,
    string AuthoritySurfaceName,
    string PreconditionSnapshotRef,
    string PreLiftStatus,
    string PostLiftTargetStatus,
    string NonGrantSummary,
    IReadOnlyList<string> WitnessRefs);
