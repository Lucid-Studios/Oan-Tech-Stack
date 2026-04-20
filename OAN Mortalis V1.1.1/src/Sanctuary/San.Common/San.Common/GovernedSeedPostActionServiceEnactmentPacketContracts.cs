namespace San.Common;

public sealed record GovernedSeedPostActionServiceEnactmentPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedPostExecutionOperationalActionPacket PostExecutionOperationalActionPacket,
    GovernedSeedEffectEmissionAssessment EffectEmissionAssessment,
    GovernedSeedServiceEnactmentCommitAssessment ServiceEnactmentCommitAssessment,
    GovernedSeedPostActionServiceEnactmentAssessment PostActionServiceEnactmentAssessment,
    GovernedSeedPostActionServiceEnactmentReceipt PostActionServiceEnactmentReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
