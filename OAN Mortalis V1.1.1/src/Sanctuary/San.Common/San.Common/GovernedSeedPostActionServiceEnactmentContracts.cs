namespace San.Common;

public enum GovernedSeedServiceEnactmentDisposition
{
    Unknown = 0,
    RemainAtOperationalActionPacket = 1,
    ServiceEnactmentPending = 2,
    EffectEmissionAuthorized = 3,
    ServiceEnactmentCommitted = 4,
    ReturnToOperationalActionPending = 5,
    Refuse = 6
}

public sealed record GovernedSeedEffectEmissionAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool OperationalActionCommitted,
    bool ServiceEffectAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ExplicitScopePreserved,
    bool EffectEmissionAuthorized,
    string Summary);

public sealed record GovernedSeedServiceEnactmentCommitAssessment(
    string PacketHandle,
    string CandidateId,
    bool PacketComplete,
    bool OperationalActionCommitted,
    bool EffectEmissionAuthorized,
    bool StandingConsistent,
    bool RevalidationConsistent,
    bool AttributionPreserved,
    bool ExplicitScopePreserved,
    bool EnactmentCommitReady,
    bool ServiceEnactmentCommitted,
    string Summary);

public sealed record GovernedSeedPostActionServiceEnactmentAssessment(
    string PacketHandle,
    string CandidateId,
    GovernedSeedServiceEnactmentDisposition Disposition,
    bool PacketComplete,
    bool EffectEmissionAuthorized,
    bool ServiceEnactmentCommitted,
    string Summary);

public sealed record GovernedSeedPostActionServiceEnactmentReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string CandidateId,
    GovernedSeedServiceEnactmentDisposition Disposition,
    bool EffectEmissionAuthorized,
    bool ServiceEnactmentCommitted,
    string Summary);

