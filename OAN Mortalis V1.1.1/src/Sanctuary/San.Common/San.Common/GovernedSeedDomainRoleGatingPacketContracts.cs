namespace San.Common;

public sealed record GovernedSeedDomainRoleGatingPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedPreDomainGovernancePacket PreDomainGovernancePacket,
    GovernedSeedDomainEligibilityAssessment DomainEligibilityAssessment,
    GovernedSeedRoleEligibilityAssessment RoleEligibilityAssessment,
    GovernedSeedDomainRoleGatingAssessment GatingAssessment,
    GovernedSeedDomainRoleGatingReceipt GatingReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
