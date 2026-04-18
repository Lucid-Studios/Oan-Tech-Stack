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
    bool ExecutionAuthorized,
    bool ServiceBehaviorAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ExplicitScopePreserved,
    bool ServiceEffectAuthorized,
    string Summary);

public sealed record GovernedSeedCommitIntent(
    string PacketHandle,
    string CandidateId,
    bool ServiceEffectAuthorized,
    bool ExecutionAuthorized,
    bool ExplicitCommitRequested,
    bool IrreversibleEffectRequested,
    bool PropagationRequested,
    bool CommitIntentPresent,
    string Summary);

public sealed record GovernedSeedOperationalActionCommitAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool ExecutionAuthorized,
    bool ServiceEffectAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ExplicitScopePreserved,
    bool ExplicitCommitRequested,
    bool CommitReady,
    bool OperationalActionCommitted,
    string Summary);

public sealed record GovernedSeedCommitReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    bool CommitReady,
    bool OperationalActionCommitted,
    string Summary);

public sealed record GovernedSeedPostExecutionOperationalActionAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedOperationalActionDisposition Disposition,
    bool PacketComplete,
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
