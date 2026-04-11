namespace SLI.Runtime;

public static class EscalationTransitionPolicy
{
    public static SliEscalationTransitionDecision Evaluate(SliEscalationTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        SliEscalationState[] allowedTargets = request.Current.State switch
        {
            SliEscalationState.LocalResolve =>
                [SliEscalationState.LocalResolve, SliEscalationState.StewardReview, SliEscalationState.HitlHold, SliEscalationState.Quarantine],
            SliEscalationState.StewardReview =>
                [SliEscalationState.LocalResolve, SliEscalationState.StewardEscalate, SliEscalationState.HitlHold, SliEscalationState.Quarantine, SliEscalationState.Refusal],
            SliEscalationState.StewardEscalate =>
                [SliEscalationState.MotherFatherReview, SliEscalationState.HitlHold],
            SliEscalationState.MotherFatherReview =>
                [SliEscalationState.GovernedReturn, SliEscalationState.HitlHold, SliEscalationState.Quarantine, SliEscalationState.Refusal],
            SliEscalationState.HitlHold =>
                [SliEscalationState.StewardReview, SliEscalationState.MotherFatherReview, SliEscalationState.Refusal, SliEscalationState.Quarantine, SliEscalationState.GovernedReturn],
            SliEscalationState.Refusal =>
                [SliEscalationState.GovernedReturn],
            SliEscalationState.Quarantine =>
                [SliEscalationState.StewardReview, SliEscalationState.MotherFatherReview, SliEscalationState.HitlHold, SliEscalationState.GovernedReturn],
            SliEscalationState.GovernedReturn =>
                [SliEscalationState.LocalResolve, SliEscalationState.StewardReview, SliEscalationState.Refusal, SliEscalationState.Quarantine],
            _ => Array.Empty<SliEscalationState>()
        };

        if (!allowedTargets.Contains(request.TargetState))
        {
            return new SliEscalationTransitionDecision(
                Disposition: EscalationTransitionDisposition.Denied,
                OutcomeCode: "transition-not-permitted",
                GovernanceTrace: "requested-escalation-transition-is-outside-the-bounded-state-grammar",
                TimestampUtc: DateTimeOffset.UtcNow);
        }

        if (request.Current.State == SliEscalationState.HitlHold && !request.WitnessTokenPresented)
        {
            return new SliEscalationTransitionDecision(
                Disposition: EscalationTransitionDisposition.Denied,
                OutcomeCode: "hitl-witness-token-required",
                GovernanceTrace: "hitl-hold-may-not-release-without-explicit-witnessed-basis",
                TimestampUtc: DateTimeOffset.UtcNow);
        }

        return new SliEscalationTransitionDecision(
            Disposition: EscalationTransitionDisposition.Admitted,
            OutcomeCode: "transition-admitted",
            GovernanceTrace: "requested-escalation-transition-is-bounded-and-lawful",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
