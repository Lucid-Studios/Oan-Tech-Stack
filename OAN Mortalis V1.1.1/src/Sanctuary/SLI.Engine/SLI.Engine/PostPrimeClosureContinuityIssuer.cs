using San.Common;

namespace SLI.Engine;

public sealed record PostPrimeClosureContinuityEvaluation(
    PrimeClosureActEvaluation ClosureActEvaluation,
    PostPrimeClosureContinuityRecord ContinuityRecord,
    string GovernanceTrace);

public interface IPostPrimeClosureContinuityIssuer
{
    PostPrimeClosureContinuityEvaluation Evaluate(
        PrimeClosureActEvaluation closureActEvaluation,
        string recordHandle);
}

public sealed class PostPrimeClosureContinuityIssuer : IPostPrimeClosureContinuityIssuer
{
    public PostPrimeClosureContinuityEvaluation Evaluate(
        PrimeClosureActEvaluation closureActEvaluation,
        string recordHandle)
    {
        ArgumentNullException.ThrowIfNull(closureActEvaluation);

        var reopeningRecord = closureActEvaluation
            .ReopeningEvaluation
            .ParticipationRecord;

        var continuityRecord = PostPrimeClosureContinuityEvaluator.Evaluate(
            closureActEvaluation.ClosureActRecord,
            reopeningRecord,
            recordHandle);

        return new PostPrimeClosureContinuityEvaluation(
            ClosureActEvaluation: closureActEvaluation,
            ContinuityRecord: continuityRecord,
            GovernanceTrace: "post-prime-closure-continuity-only");
    }
}
