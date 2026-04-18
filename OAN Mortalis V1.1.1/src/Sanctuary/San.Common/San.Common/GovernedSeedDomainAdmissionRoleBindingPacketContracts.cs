namespace San.Common;

public sealed record GovernedSeedDomainAdmissionRoleBindingPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainRoleGatingPacket DomainRoleGatingPacket,
    GovernedSeedDomainAdmissionAssessment DomainAdmissionAssessment,
    GovernedSeedRoleBindingAssessment RoleBindingAssessment,
    GovernedSeedDomainAdmissionRoleBindingAssessment DomainAdmissionRoleBindingAssessment,
    GovernedSeedDomainAdmissionRoleBindingReceipt DomainAdmissionRoleBindingReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
