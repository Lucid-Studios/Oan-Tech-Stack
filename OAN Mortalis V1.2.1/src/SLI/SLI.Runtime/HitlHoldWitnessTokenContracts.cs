namespace SLI.Runtime;

public enum HitlHoldWitnessTokenClass
{
    AckToken = 0,
    AssentToken = 1,
    StewardAttestationToken = 2,
    EscalationTransferToken = 3,
    MotherFatherReviewToken = 4,
    QuarantineContinuationToken = 5,
    RefusalClosureToken = 6,
    RepresentToken = 7
}

public enum HitlHoldWitnessRole
{
    AcknowledgingWitness = 0,
    AssentingWitness = 1,
    ReviewWitness = 2
}

public enum HitlHoldExitRoute
{
    StewardReview = 0,
    MotherFatherReview = 1,
    GovernedReturn = 2,
    Refusal = 3,
    Quarantine = 4
}

public enum HitlHoldClass
{
    ConstitutionalReleaseOnly = 0,
    HumanAssentRequired = 1,
    QuarantineUntilReviewed = 2,
    RefusalUntilRepresented = 3
}

public sealed record HitlHoldWitnessToken(
    string TokenId,
    HitlHoldWitnessTokenClass TokenClass,
    HitlHoldWitnessRole WitnessRole,
    string IssuerSurface,
    string IssuerJurisdiction,
    string HoldTraceId,
    string StateLineage,
    HitlHoldExitRoute AuthorizedExit,
    string ExpiryOrReuseRule,
    DateTimeOffset IssuedAtUtc);
