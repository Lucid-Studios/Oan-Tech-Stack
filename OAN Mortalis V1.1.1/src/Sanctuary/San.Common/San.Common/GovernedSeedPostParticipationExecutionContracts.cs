namespace San.Common;

public enum GovernedSeedExecutionAuthorizationDisposition
{
    Unknown = 0,
    RemainAtParticipationPacket = 1,
    ExecutionPending = 2,
    ServiceBehaviorAuthorized = 3,
    ExecutionAuthorized = 4,
    ReturnToParticipationPending = 5,
    Refuse = 6
}

public sealed record GovernedSeedServiceBehaviorAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool OccupancyAuthorized,
    bool ParticipationAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ServiceScopeLawful,
    bool ServiceBehaviorAuthorized,
    string Summary);

public sealed record GovernedSeedExecutionAuthorizationAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool OccupancyAuthorized,
    bool ParticipationAuthorized,
    bool ExecutionStructurePresent,
    bool ExplicitScopePreserved,
    bool RoleBearingExecutionRequested,
    bool ExecutionAuthorized,
    string Summary);

public sealed record GovernedSeedPostParticipationExecutionAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedExecutionAuthorizationDisposition Disposition,
    bool PacketComplete,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool ServiceBehaviorAuthorized,
    bool ExecutionAuthorized,
    string Summary);

public sealed record GovernedSeedPostParticipationExecutionReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedExecutionAuthorizationDisposition Disposition,
    bool ServiceBehaviorAuthorized,
    bool ExecutionAuthorized,
    string Summary);
