using San.Common;

namespace SLI.Engine;

public sealed record PrimeClosureActEvaluation(
    LawfulReopeningParticipationEvaluation ReopeningEvaluation,
    PrimeClosureActRecord ClosureActRecord,
    string GovernanceTrace);

public interface IPrimeClosureActIssuer
{
    PrimeClosureActEvaluation Evaluate(
        LawfulReopeningParticipationEvaluation reopeningEvaluation,
        string recordHandle,
        string attestedRemainingProductHandle);
}

public sealed class PrimeClosureActIssuer : IPrimeClosureActIssuer
{
    public PrimeClosureActEvaluation Evaluate(
        LawfulReopeningParticipationEvaluation reopeningEvaluation,
        string recordHandle,
        string attestedRemainingProductHandle)
    {
        ArgumentNullException.ThrowIfNull(reopeningEvaluation);

        var retainedHistory = reopeningEvaluation
            .FilamentEvaluation
            .RetainedWholeEvaluation
            .RetainedHistoryRecord;

        var closureActRecord = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningEvaluation.ParticipationRecord,
            recordHandle,
            attestedRemainingProductHandle);

        return new PrimeClosureActEvaluation(
            ReopeningEvaluation: reopeningEvaluation,
            ClosureActRecord: closureActRecord,
            GovernanceTrace: "prime-closure-act-only");
    }
}
