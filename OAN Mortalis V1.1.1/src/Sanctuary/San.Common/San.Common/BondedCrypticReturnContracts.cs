namespace San.Common;

public enum CmeReturnCleaveClass
{
    Autobiographical = 0,
    LocalResearch = 1,
    PredicateCanonical = 2,
    RefusalQuarantine = 3
}

public enum CmeReturnAdmissibilityPosture
{
    CandidateOnly = 0,
    Reviewable = 1,
    Admissible = 2,
    Withheld = 3
}

public enum CmeReturnContradictionPosture
{
    None = 0,
    BoundedDifference = 1,
    ReviewRequired = 2,
    ContradictionPresent = 3
}

public enum CmeReturnPromotionDecision
{
    Admit = 0,
    Hold = 1,
    Refuse = 2,
    Escalate = 3
}

public enum CmeReturnAuditOfficeKind
{
    Mother = 0,
    Father = 1,
    Steward = 2
}

public enum CmeReturnPromotionTargetSurfaceKind
{
    SelfGel = 0,
    CradleTekCGoa = 1,
    CradleTekLocalResearch = 2,
    PrimeMosPredicate = 3,
    SanctuaryGoa = 4,
    Quarantine = 5
}

public sealed record CmeReturnProtectedEvidenceReference(
    string EvidenceHandle,
    string EvidenceClass,
    string SourceSurfaceHandle,
    bool ProtectedExternal,
    bool WitnessRequired);

public sealed record BondedSoulFrameCloseStateReceipt(
    string ReceiptHandle,
    string CloseStateHandle,
    string SoulFrameHandle,
    string CMosSessionHandle,
    string COeHandle,
    string CSelfGelHandle,
    IReadOnlyList<string> LinkedOeWitnessHandles,
    IReadOnlyList<string> LinkedSelfGelWitnessHandles,
    string CloseProfile,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record CrypticGoaReturnStagingReceipt(
    string ReceiptHandle,
    string CloseStateHandle,
    string EncryptedCGoaHandle,
    string StagingProfile,
    string SessionIntegrityHash,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record BondedCrypticReturnBundle(
    string BundleHandle,
    string CloseStateHandle,
    string ReturnStagingHandle,
    string CMosSessionHandle,
    string COeHandle,
    string CSelfGelHandle,
    IReadOnlyList<string> LinkedOeWitnessHandles,
    IReadOnlyList<string> LinkedSelfGelWitnessHandles,
    IReadOnlyList<string> SessionReceiptHandles,
    IReadOnlyList<string> RefusalHandles,
    IReadOnlyList<string> CandidateDeltaHandles,
    IReadOnlyList<string> ProtectedResidueHandles,
    IReadOnlyList<string> ModulationObservationHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record CmeReturnCleaveReceipt(
    string ReceiptHandle,
    string BundleHandle,
    string EncryptedCGoaHandle,
    CmeReturnCleaveClass CleaveClass,
    CmeReturnPromotionTargetSurfaceKind RequestedTargetSurface,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> AuditWitnessHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record AutobiographicalCleaveCandidateReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    string BundleHandle,
    string SelfGelTargetHandle,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> CandidateDeltaHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record LocalResearchCleaveCandidateReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    string BundleHandle,
    string LocalResearchTargetHandle,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> CandidateDeltaHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record PredicatePromotionCandidateReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    string BundleHandle,
    string PredicateTargetHandle,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> CandidateDeltaHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record ReturnQuarantineReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    string BundleHandle,
    string QuarantineHandle,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> RefusalHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record CmeReturnCandidatePacket(
    string PacketHandle,
    string CandidateHandle,
    string SourceBundleHandle,
    string SourceCMosSessionHandle,
    string SourceCOeHandle,
    string SourceCSelfGelHandle,
    string EncryptedCGoaHandle,
    CmeReturnCleaveClass CleaveClass,
    CmeReturnPromotionTargetSurfaceKind RequestedTargetSurface,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    IReadOnlyList<string> ReceiptHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    DateTimeOffset TimestampUtc);

public sealed record CmeReturnEvidenceReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    string EvidenceProfile,
    IReadOnlyList<string> AuditWitnessHandles,
    IReadOnlyList<CmeReturnProtectedEvidenceReference> ProtectedEvidenceRefs,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record CmeReturnAuditReviewReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    IReadOnlyList<CmeReturnAuditOfficeKind> AuditOffices,
    CmeReturnPromotionTargetSurfaceKind RequestedTargetSurface,
    CmeReturnAdmissibilityPosture AdmissibilityPosture,
    CmeReturnContradictionPosture ContradictionPosture,
    bool ReviewPassed,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record CmeReturnPromotionDecisionReceipt(
    string ReceiptHandle,
    string CandidateHandle,
    CmeReturnPromotionTargetSurfaceKind RequestedTargetSurface,
    CmeReturnPromotionDecision PromotionDecision,
    IReadOnlyList<CmeReturnAuditOfficeKind> AuditOffices,
    string DecisionReason,
    DateTimeOffset TimestampUtc);

public sealed record PrimeSurfacePromotionReceipt(
    string ReceiptHandle,
    string DecisionHandle,
    string CandidateHandle,
    CmeReturnPromotionTargetSurfaceKind TargetSurfaceKind,
    string TargetSurfaceHandle,
    bool AppendOnly,
    string SourceReason,
    DateTimeOffset TimestampUtc);
