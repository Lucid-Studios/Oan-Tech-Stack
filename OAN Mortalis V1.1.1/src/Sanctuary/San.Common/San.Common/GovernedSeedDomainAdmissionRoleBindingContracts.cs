namespace San.Common;

public enum GovernedSeedDomainAdmissionRoleBindingDisposition
{
    Unknown = 0,
    RemainAtGatingPacket = 1,
    DomainAdmittedRolePending = 2,
    DomainAndRoleBound = 3,
    ReturnToPreDomainCarry = 4,
    Refuse = 5
}

public sealed record GovernedSeedDomainAdmissionAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool CrypticAuthorityBleedDetected,
    bool StandingConsistent,
    bool DomainEligibilitySatisfied,
    bool BurdenAttributableAtDomainScope,
    bool DomainAdmissionGranted,
    string Summary);

public sealed record GovernedSeedRoleBindingAssessment(
    string PacketHandle,
    string CandidateId,
    bool DomainAdmissionGranted,
    bool RoleRelevantStructurePresent,
    bool ResponsibilityBindableAtRoleScope,
    bool RoleLawfulWithinDomain,
    bool RoleBound,
    string Summary);

public sealed record GovernedSeedDomainAdmissionRoleBindingAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainAdmissionRoleBindingDisposition Disposition,
    bool PacketComplete,
    bool CrypticAuthorityBleedDetected,
    bool DomainAdmissionGranted,
    bool RoleBound,
    string Summary);

public sealed record GovernedSeedDomainAdmissionRoleBindingReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedDomainAdmissionRoleBindingDisposition Disposition,
    bool DomainAdmissionGranted,
    bool RoleBound,
    string Summary);
