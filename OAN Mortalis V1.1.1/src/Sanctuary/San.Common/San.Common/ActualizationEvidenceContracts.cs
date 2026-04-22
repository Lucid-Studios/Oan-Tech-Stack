namespace San.Common;

public sealed record DurabilityWitnessReceipt(
    string ReceiptHandle,
    bool DurableUnderVariation,
    bool InterlockDensityEmergent,
    bool ColdPromotionStillWithheld,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ColdAdmissionEligibilityGateReceipt(
    string GateHandle,
    bool ColdApproachLawful,
    bool PreFreezeOnly,
    bool FinalInheritanceStillWithheld,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record InterlockDensityLedgerReceipt(
    string LedgerHandle,
    bool DenseInterweaveEmergent,
    bool LatticeClaimStillWithheld,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CoreInvariantLatticeWitnessReceipt(
    string ReceiptHandle,
    bool IdentityAdjacentSignificanceEmergent,
    bool CoreLawSanctificationDenied,
    bool LatticeGradeInvarianceWitnessed,
    string ReasonCode,
    DateTimeOffset TimestampUtc);
