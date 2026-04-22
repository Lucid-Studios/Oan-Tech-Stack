namespace San.Common;

public enum GovernedSeedPostAdmissionParticipationDisposition
{
    Unknown = 0,
    RemainAtAdmissionPacket = 1,
    DomainOccupancyPending = 2,
    DomainOccupancyAuthorized = 3,
    RoleParticipationAuthorized = 4,
    ReturnToBindingPending = 5,
    Refuse = 6
}

public sealed record GovernedSeedDomainOccupancyAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool DomainAdmissionGranted,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool OccupancyStructurePresent,
    bool OccupancyAuthorized,
    string Summary);

public sealed record GovernedSeedRoleParticipationAssessment(
    string PacketHandle,
    string CandidateId,
    bool DomainAdmissionGranted,
    bool RoleBound,
    bool RoleLawfulWithinDomain,
    bool ResponsibilityBindableAtRoleScope,
    bool ScopeExpansionDetected,
    bool RoleParticipationAuthorized,
    string Summary);

public sealed record GovernedSeedPostAdmissionParticipationAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedPostAdmissionParticipationDisposition Disposition,
    bool PacketComplete,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool DomainAdmissionGranted,
    bool OccupancyAuthorized,
    bool RoleParticipationAuthorized,
    string Summary);

public sealed record GovernedSeedPostAdmissionParticipationReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedPostAdmissionParticipationDisposition Disposition,
    bool OccupancyAuthorized,
    bool RoleParticipationAuthorized,
    string Summary);
