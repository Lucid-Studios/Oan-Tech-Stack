namespace San.Common;

public sealed record GovernedSeedPostExecutionOperationalActionPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedPostParticipationExecutionPacket PostParticipationExecutionPacket,
    GovernedSeedServiceEffectAssessment ServiceEffectAssessment,
    GovernedSeedCommitIntent CommitIntent,
    GovernedSeedOperationalActionCommitAssessment OperationalActionCommitAssessment,
    GovernedSeedCommitReceipt CommitReceipt,
    GovernedSeedPostExecutionOperationalActionAssessment PostExecutionOperationalActionAssessment,
    GovernedSeedPostExecutionOperationalActionReceipt PostExecutionOperationalActionReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
