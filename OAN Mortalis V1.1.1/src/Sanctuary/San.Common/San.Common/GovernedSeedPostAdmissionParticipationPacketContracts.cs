namespace San.Common;

public sealed record GovernedSeedPostAdmissionParticipationPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainAdmissionRoleBindingPacket DomainAdmissionRoleBindingPacket,
    GovernedSeedDomainOccupancyAssessment DomainOccupancyAssessment,
    GovernedSeedRoleParticipationAssessment RoleParticipationAssessment,
    GovernedSeedPostAdmissionParticipationAssessment PostAdmissionParticipationAssessment,
    GovernedSeedPostAdmissionParticipationReceipt PostAdmissionParticipationReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
