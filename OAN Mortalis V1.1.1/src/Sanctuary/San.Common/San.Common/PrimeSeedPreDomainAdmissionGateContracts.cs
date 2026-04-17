using System;

namespace San.Common;

public enum PrimeSeedPreDomainAdmissionDisposition
{
    Unknown = 0,
    RemainPreDomain = 1,
    CarryCrypticOnly = 2,
    Refuse = 3,
    PrepareForDomainRoleGate = 4
}

public sealed record GovernedSeedRevalidationContext(
    string ContextHandle,
    string ContextProfile,
    bool RevalidationSatisfied,
    string Summary,
    DateTimeOffset TimestampUtc);

public sealed record PrimeSeedPreDomainAdmissionAssessment(
    string CandidateId,
    PrimeSeedPreDomainAdmissionDisposition Disposition,
    bool PrimeComplianceIntact,
    bool CrypticAuthorityBleedDetected,
    bool ResponsibilityAttributable,
    string Summary);

public sealed record DomainRoleEligibilityAssessment(
    string CandidateId,
    bool DomainEligible,
    bool RoleEligible,
    string Summary);

public sealed record PrimeSeedPreDomainAdmissionGateReceipt(
    string CandidateId,
    PrimeSeedPreDomainAdmissionDisposition Disposition,
    bool DomainEligible,
    bool RoleEligible,
    string Summary);
