namespace San.Common;

public enum GovernedSeedDomainRoleGatingDisposition
{
    Unknown = 0,
    RemainPreDomain = 1,
    DomainAdmissibleRoleIncomplete = 2,
    DomainAndRoleAdmissible = 3,
    CrypticOnlyCarry = 4,
    Refuse = 5
}

public sealed record GovernedSeedDomainEligibilityAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool PrimeAdmissionStructurePresent,
    bool CrypticAuthorityBleedDetected,
    bool ForwardMotionSupported,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool DomainEligible,
    string Summary);

public sealed record GovernedSeedRoleEligibilityAssessment(
    string PacketHandle,
    string CandidateId,
    bool DomainEligible,
    bool RoleRelevantStructurePresent,
    bool ResponsibilityAttributable,
    bool RoleEligible,
    string Summary);

public sealed record GovernedSeedDomainRoleGatingAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainRoleGatingDisposition Disposition,
    bool PacketComplete,
    bool CrypticAuthorityBleedDetected,
    bool StandingConsistent,
    bool DomainEligible,
    bool RoleEligible,
    string Summary);

public sealed record GovernedSeedDomainRoleGatingReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainRoleGatingDisposition Disposition,
    bool DomainEligible,
    bool RoleEligible,
    string Summary);
