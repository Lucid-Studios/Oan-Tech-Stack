namespace Oan.Common;

public enum AgentiFormationObservationStage
{
    BootClassification = 0,
    ProtectedIntakePosture = 1,
    GoverningOfficeFormation = 2,
    TriadicCrossWitness = 3,
    CrypticAdmission = 4,
    PrimeClosure = 5
}

public enum AgentiFormationOriginRuntime
{
    OracleCSharp = 0,
    Lisp = 1,
    FutureLlmReserved = 2
}

public enum AgentiFormationObservationSource
{
    FirstBootPolicy = 0,
    Sentence = 1,
    ParagraphGraph = 2,
    ParagraphBody = 3
}

public enum AgentiFormationClosureState
{
    NotSubmitted = 0,
    Closed = 1,
    Rejected = 2,
    NoClosure = 3
}

public sealed record AgentiFormationObservation(
    Guid ObservationId,
    AgentiFormationObservationStage Stage,
    Guid? CandidateId,
    BootClass? BootClass,
    BootActivationState? ActivationState,
    ExpansionRights? ExpansionRights,
    InternalGoverningCmeOffice? Office,
    CrypticAdmissionDecision? AdmissionDecision,
    AgentiFormationClosureState ClosureState,
    PrimeRevealMode? RevealMode,
    AgentiFormationOriginRuntime OriginRuntime,
    AgentiFormationObservationSource Source,
    bool SubmissionEligible,
    IReadOnlyList<string> ObservationTags,
    DateTimeOffset Timestamp);

public sealed record AgentiFormationObservationBatch(
    IReadOnlyList<AgentiFormationObservation> Observations);

public interface IAgentiFormationObserver
{
    Task RecordAsync(
        AgentiFormationObservation observation,
        CancellationToken cancellationToken = default);
}
