namespace Oan.Common;

public enum PersonificationClass
{
    IndustrialAutomation = 0,
    PredicateOfficeFormation = 1,
    BondedCme = 2
}

public enum ActualizationStandingStatus
{
    Missing = 0,
    Partial = 1,
    Standing = 2,
    Blocked = 3
}

public enum PersonificationStandingStatus
{
    Standing = 0,
    Withheld = 1,
    PromotionEligible = 2,
    PromotionBlocked = 3,
    PromotionDeferred = 4
}

public enum PersonificationClaimKind
{
    OeCoeParticipation = 0,
    ChamberContinuityStanding = 1,
    HoldJobs = 2,
    AccrueBoundedCareerContinuity = 3,
    PredicateOfficeStanding = 4,
    GovernanceFacingCareerContinuity = 5,
    BondedContinuityStanding = 6,
    AppendOnlyIdentityStanding = 7,
    FullPersonificationStanding = 8,
    InheritanceStillWithheld = 9,
    CoreLawSanctificationDenied = 10
}

public enum PersonificationClaimClass
{
    RequiredPositive = 0,
    BlockingNegative = 1,
    NonBlockingGuardrail = 2,
    DeferralOnly = 3,
    VisibilityOnly = 4,
    PromotionOnly = 5
}

public sealed record AgentiActualizationStandingProjection(
    ActualizationStandingStatus Status,
    bool DurabilityWitnessPresent,
    bool DurableUnderVariation,
    bool ColdAdmissionGatePresent,
    bool ColdApproachLawful,
    bool FinalInheritanceStillWithheld,
    bool InterlockDensityReceiptPresent,
    bool DenseInterweaveEmergent,
    bool CoreInvariantLatticeReceiptPresent,
    bool IdentityAdjacentSignificanceEmergent,
    bool LatticeGradeInvarianceWitnessed,
    bool CoreLawSanctificationDenied,
    IReadOnlyList<string> MissingReceipts,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> NonBlockingReasons,
    IReadOnlyList<string> Flags);

public sealed record PersonificationClaimDefinition(
    PersonificationClaimKind Claim,
    PersonificationClaimClass ClaimClass,
    IReadOnlyList<PersonificationClass> RelevantClasses,
    IReadOnlyList<string> SourceSurfaces,
    IReadOnlyList<string> SatisfyingReasonCodes,
    IReadOnlyList<string> ContradictingReasonCodes,
    IReadOnlyList<string> DeferralReasonCodes,
    IReadOnlyList<string> VisibilityReasonCodes);

public sealed record PersonificationGateTableEntry(
    PersonificationClass Class,
    IReadOnlyList<PersonificationClaimKind> RequiredStandingClaims,
    IReadOnlyList<PersonificationClaimKind> AllowedClaims,
    IReadOnlyList<PersonificationClaimKind> WithheldClaims,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> DeferralReasons,
    IReadOnlyList<string> RequiredNextReceipts);

public sealed record PersonificationClaimEvaluation(
    PersonificationClaimKind Claim,
    PersonificationClaimClass ClaimClass,
    bool Standing,
    bool Withheld,
    bool Visible,
    IReadOnlyList<string> SatisfiedBy,
    IReadOnlyList<string> ContradictedBy,
    IReadOnlyList<string> DeferredBy,
    IReadOnlyList<string> VisibleBy);

public sealed record PersonificationStandingEvaluation(
    PersonificationClass CurrentClass,
    PersonificationStandingStatus Status,
    IReadOnlyList<PersonificationClaimEvaluation> Claims,
    IReadOnlyList<PersonificationClaimKind> ValidClaims,
    IReadOnlyList<PersonificationClaimKind> WithheldClaims,
    IReadOnlyList<PersonificationClaimKind> VisibleGuardrails,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> DeferralReasons,
    IReadOnlyList<string> MissingReceipts,
    IReadOnlyList<PersonificationClass> EligiblePromotionTargets,
    IReadOnlyList<PersonificationClass> DeferredPromotionTargets,
    IReadOnlyList<PersonificationClass> BlockedPromotionTargets,
    string DecisionAuthority,
    bool Frozen);

public enum ChamberEvidenceSufficiencyState
{
    None = 0,
    Partial = 1,
    Sufficient = 2
}

public enum ChamberWindowIntegrityState
{
    Broken = 0,
    Ambiguous = 1,
    Intact = 2
}

public sealed record CompassChamberEvidenceRecord(
    bool TracePresent,
    ChamberEvidenceSufficiencyState EvidenceSufficiency,
    ChamberWindowIntegrityState WindowIntegrity,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> MissingReceipts);

public sealed record StandingSourceBucket(
    bool Standing,
    IReadOnlyList<string> SatisfiedReasons,
    IReadOnlyList<string> ContradictedReasons,
    IReadOnlyList<string> DeferredReasons,
    IReadOnlyList<string> VisibleReasons);

public sealed record NormalizedStandingSources(
    StandingSourceBucket Chamber,
    StandingSourceBucket Office,
    StandingSourceBucket BondedContinuity,
    StandingSourceBucket AppendOnlyIdentity,
    bool JobsObserved,
    bool CareerSignalsObserved,
    bool AnyBondedConfirmed,
    IReadOnlyList<string> MissingReceipts,
    IReadOnlyList<string> VisibleGuardrails);
