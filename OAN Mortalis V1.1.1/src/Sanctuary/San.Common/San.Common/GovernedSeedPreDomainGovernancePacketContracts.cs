namespace San.Common;

public sealed record GovernedSeedPreDomainGovernancePacket(
    string PacketHandle,
    string CandidateId,
    PrimeSeedStateReceipt PrimeSeedStateReceipt,
    GovernedSeedCandidateBoundaryReceipt BoundaryReceipt,
    GovernedSeedCrypticHoldingInspectionReceipt HoldingInspectionReceipt,
    GovernedSeedFormOrCleaveAssessment FormOrCleaveAssessment,
    GovernedSeedCandidateSeparationReceipt SeparationReceipt,
    PrimeCrypticDuplexGovernanceReceipt DuplexGovernanceReceipt,
    PrimeSeedPreDomainAdmissionGateReceipt AdmissionGateReceipt,
    GovernedSeedPreDomainHostLoopReceipt HostLoopReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
