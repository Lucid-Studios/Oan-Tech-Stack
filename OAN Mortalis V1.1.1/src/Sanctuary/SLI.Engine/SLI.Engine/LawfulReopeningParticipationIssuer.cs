using San.Common;

namespace SLI.Engine;

public sealed record LawfulReopeningParticipationEvaluation(
    CommunicativeFilamentEvaluation FilamentEvaluation,
    LawfulReopeningParticipationRecord ParticipationRecord,
    string GovernanceTrace);

public interface ILawfulReopeningParticipationIssuer
{
    LawfulReopeningParticipationEvaluation Issue(
        CommunicativeFilamentEvaluation filamentEvaluation,
        ReopeningModeKind reopeningMode,
        string recordHandle,
        IReadOnlyList<string>? reopeningMarkers = null);
}

public sealed class LawfulReopeningParticipationIssuer : ILawfulReopeningParticipationIssuer
{
    public LawfulReopeningParticipationEvaluation Issue(
        CommunicativeFilamentEvaluation filamentEvaluation,
        ReopeningModeKind reopeningMode,
        string recordHandle,
        IReadOnlyList<string>? reopeningMarkers = null)
    {
        ArgumentNullException.ThrowIfNull(filamentEvaluation);

        var participationRecord = LawfulReopeningParticipationEvaluator.Evaluate(
            filamentEvaluation.ResolutionReceipt,
            reopeningMode,
            recordHandle,
            reopeningMarkers);

        return new LawfulReopeningParticipationEvaluation(
            FilamentEvaluation: filamentEvaluation,
            ParticipationRecord: participationRecord,
            GovernanceTrace: "lawful-reopening-participation-only");
    }
}
