namespace San.Common;

public sealed record GovernedSeedPostParticipationExecutionPacket(
    string PacketHandle,
    string CandidateId,
    GovernedSeedPostAdmissionParticipationPacket PostAdmissionParticipationPacket,
    GovernedSeedServiceBehaviorAssessment ServiceBehaviorAssessment,
    GovernedSeedExecutionAuthorizationAssessment ExecutionAuthorizationAssessment,
    GovernedSeedPostParticipationExecutionAssessment PostParticipationExecutionAssessment,
    GovernedSeedPostParticipationExecutionReceipt PostParticipationExecutionReceipt,
    DateTimeOffset MaterializedAtUtc,
    string Summary);
