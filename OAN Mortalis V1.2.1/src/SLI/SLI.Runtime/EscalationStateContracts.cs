using San.Common;

namespace SLI.Runtime;

public enum SliEscalationState
{
    LocalResolve = 0,
    StewardReview = 1,
    StewardEscalate = 2,
    MotherFatherReview = 3,
    HitlHold = 4,
    Refusal = 5,
    Quarantine = 6,
    GovernedReturn = 7
}

public enum SliEscalationJurisdiction
{
    LocalCradle = 0,
    Steward = 1,
    MotherFather = 2
}

public enum EscalationTransitionDisposition
{
    Denied = 0,
    Admitted = 1
}

public sealed record SliEscalationPacket(
    string TraceId,
    SliEscalationState State,
    SliEscalationJurisdiction Jurisdiction,
    AdmissibilityStatus Admissibility,
    string BurdenOfReview,
    bool HitlRequired,
    DateTimeOffset TimestampUtc);

public sealed record SliEscalationTransitionRequest(
    SliEscalationPacket Current,
    SliEscalationState TargetState,
    string Reason,
    bool WitnessTokenPresented,
    DateTimeOffset TimestampUtc);

public sealed record SliEscalationTransitionDecision(
    EscalationTransitionDisposition Disposition,
    string OutcomeCode,
    string GovernanceTrace,
    DateTimeOffset TimestampUtc);
