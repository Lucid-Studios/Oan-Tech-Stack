namespace Oan.Common;

public enum ConstructClass
{
    BoundedWorker = 0,
    IssuedOffice = 1,
    ParkedZedChassis = 2,
    ReactivationCandidate = 3
}

public enum CertificationClass
{
    C0ObserveOnly = 0,
    C1RequestingOperator = 1,
    C2WorkerSupervisor = 2,
    C3OfficeIssuer = 3,
    C4ReactivationAuthority = 4
}

public enum MaturityPosture
{
    Implemented = 0,
    DoctrineBacked = 1,
    Exploratory = 2,
    Withheld = 3
}

public sealed record IssuedOfficePackage(
    string PackageId,
    string IssuanceLineageId,
    InternalGoverningCmeOffice OfficeKind,
    string OfficeInstanceId,
    string ChassisClass,
    string TargetRuntimeSurface,
    string IssuerSurface,
    string AuthorizingOperatorOrKernel,
    string AuthorityScope,
    OfficeActionEligibility AllowedActionCeiling,
    CompassVisibilityClass DisclosureCeiling,
    string BondRequirement,
    string GuardedReviewRequirement,
    IReadOnlyList<string> RevocationConditions,
    IReadOnlyList<string> ToolAllowlist,
    IReadOnlyList<string> ToolDenials,
    string ExternalCallPolicy,
    string MutationPermissions,
    string WorkerActivationPermissions,
    IReadOnlyList<string> RequiredPacketContracts,
    IReadOnlyList<string> MountedMemoryLanes,
    IReadOnlyList<string> ForbiddenMemoryLanes,
    string ContinuityLinkageSurface,
    string ResiduePolicy,
    string ParkReturnPolicy,
    string SameOfficeLineageRule,
    IReadOnlyList<string> RequiredWitnessSurfaces,
    IReadOnlyList<string> RequiredReceipts,
    string ReducedTelemetrySurface,
    string GuardedTelemetrySurface,
    IReadOnlyList<string> CrypticOrSealedSurfaces,
    string IncidentEscalationPath,
    IReadOnlyList<string> ReturnObligations,
    string RequiredReturnPacket,
    string ParkEligibility,
    IReadOnlyList<string> QuarantineTriggers,
    string HandoffTerminationRule,
    string ExpiryOrReissueWindow,
    MaturityPosture MaturityPosture,
    string ImplementationStatus,
    string WithheldMechanicsMarker,
    string ReviewOwner,
    DateTimeOffset LastReviewedUtc);

public sealed record GovernedOfficeIssuanceReceipt(
    string IssuanceHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    InternalGoverningCmeOffice Office,
    ConstructClass ConstructClass,
    string PackageId,
    string IssuanceLineageId,
    string OfficeInstanceId,
    OfficeActionEligibility AllowedActionCeiling,
    CompassVisibilityClass DisclosureCeiling,
    MaturityPosture MaturityPosture,
    string OfficeAuthorityHandle,
    string WeatherDisclosureHandle,
    string WitnessedBy,
    DateTimeOffset TimestampUtc);
