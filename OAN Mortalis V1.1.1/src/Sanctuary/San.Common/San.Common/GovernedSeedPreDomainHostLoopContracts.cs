namespace San.Common;

public enum GovernedSeedPsyPolarityKind
{
    Positive = 0,
    Negative = 1,
    Neutral = 2
}

public enum GovernedSeedFormOrCleaveDispositionKind
{
    Form = 0,
    Cleave = 1,
    Reject = 2,
    Hold = 3
}

public enum GovernedSeedCollapseDispositionKind
{
    None = 0,
    Refuse = 1,
    Cleave = 2,
    Hold = 3,
    ClosedIntoTrace = 4
}

public enum GovernedSeedCarryDispositionKind
{
    None = 0,
    Carry = 1,
    Hold = 2,
    Cleave = 3,
    Refuse = 4
}

public sealed record GovernedSeedPreAdmissibleConstruct(
    string ConstructHandle,
    string ConstructKind,
    string Summary,
    IReadOnlyList<string> EvidenceHandles,
    bool CandidateOnly);

public sealed record GovernedSeedCrypticHoldingEntry(
    string EntryHandle,
    GovernedSeedPsyPolarityKind PsyPolarity,
    GovernedSeedPreAdmissibleConstruct Construct,
    string HoldingReason,
    bool InspectionInfluenceOnly,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedCrypticHoldingInspectionReceipt(
    string ReceiptHandle,
    string FormationReceiptHandle,
    string? ListeningFrameHandle,
    string? CompassPacketHandle,
    IReadOnlyList<GovernedSeedCrypticHoldingEntry> HoldingEntries,
    bool CandidateOnly,
    bool InspectionInfluenceOnly,
    bool PromotionAuthorityWithheld,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedDescendantCandidate(
    string CandidateHandle,
    string CandidateKind,
    string CandidateSurface,
    IReadOnlyList<string> EvidenceHandles,
    bool CandidateOnly);

public sealed record GovernedSeedFormOrCleaveAssessment(
    string AssessmentHandle,
    string FormationReceiptHandle,
    string? ListeningFrameHandle,
    string? CompassPacketHandle,
    string? HoldingReceiptHandle,
    GovernedSeedFormOrCleaveDispositionKind Disposition,
    GovernedSeedCarryDispositionKind CarryDisposition,
    GovernedSeedCollapseDispositionKind CollapseDisposition,
    IReadOnlyList<GovernedSeedDescendantCandidate> DescendantCandidates,
    bool CandidateOnly,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedPreDomainHostLoopReceipt(
    string ReceiptHandle,
    string FirstPrimeReceiptHandle,
    string PrimeSeedReceiptHandle,
    string? CandidateBoundaryReceiptHandle,
    string? CrypticHoldingInspectionHandle,
    string? FormOrCleaveAssessmentHandle,
    string? CandidateSeparationReceiptHandle,
    string? DuplexGovernanceReceiptHandle,
    string? AdmissionGateReceiptHandle,
    GovernedSeedCarryDispositionKind CarryDisposition,
    GovernedSeedCollapseDispositionKind CollapseDisposition,
    IReadOnlyList<string> CandidateHandles,
    bool SeedReady,
    bool CandidateOnly,
    bool DomainAdmissionWithheld,
    bool ActionAuthorityWithheld,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);
