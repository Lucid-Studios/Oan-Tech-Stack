namespace San.Common;

public enum GovernedSeedOperationalActionDisposition
{
    Unknown = 0,
    RemainAtExecutionPacket = 1,
    OperationalActionPending = 2,
    ServiceEffectAuthorized = 3,
    OperationalActionCommitted = 4,
    ReturnToExecutionPending = 5,
    Refuse = 6
}

public sealed record GovernedSeedServiceEffectAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool ServiceBehaviorAuthorized,
    bool ExecutionAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ExplicitScopePreserved,
    bool ServiceEffectAuthorized,
    string Summary);

public sealed record GovernedSeedOperationalActionCommitAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool ServiceBehaviorAuthorized,
    bool ExecutionAuthorized,
    bool CommitStructurePresent,
    bool ExplicitScopePreserved,
    bool RoleBearingActionRequested,
    bool OperationalActionCommitted,
    string Summary);

public sealed record GovernedSeedPostExecutionOperationalActionAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedOperationalActionDisposition Disposition,
    bool PacketComplete,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool ServiceEffectAuthorized,
    bool OperationalActionCommitted,
    string Summary);

public sealed record GovernedSeedPostExecutionOperationalActionReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedOperationalActionDisposition Disposition,
    bool ServiceEffectAuthorized,
    bool OperationalActionCommitted,
    string Summary);
