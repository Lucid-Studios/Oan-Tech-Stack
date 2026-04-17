namespace San.Common;

public interface IMembraneDecisionPolicy
{
    MembraneDecisionResult Decide(SymbolicEnvelope envelope);
}

public sealed record MembraneDecisionReason(
    string Code,
    string Message);

public sealed record MembraneDecisionResult(
    MembraneDecision Decision,
    SymbolicEnvelope Envelope,
    IReadOnlyList<MembraneDecisionReason> Reasons,
    SymbolicEnvelopeValidationResult ValidationResult);

public static class MembraneDecisionReasonCodes
{
    public const string InvalidPassport = "invalid-passport";
    public const string CollapseClass = "collapse-class";
    public const string AdmissibilityPending = "admissibility-pending";
    public const string AdmissibilityRefused = "admissibility-refused";
    public const string RestrictedDirective = "restricted-directive";
    public const string AdmissibleBounded = "admissible-bounded";
}

public sealed class DefaultMembraneDecisionPolicy : IMembraneDecisionPolicy
{
    private readonly ISymbolicEnvelopeValidator _validator;

    public DefaultMembraneDecisionPolicy()
        : this(new DefaultSymbolicEnvelopeValidator())
    {
    }

    public DefaultMembraneDecisionPolicy(ISymbolicEnvelopeValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public MembraneDecisionResult Decide(SymbolicEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var validationResult = _validator.Validate(envelope);

        if (!validationResult.IsValid)
        {
            return new MembraneDecisionResult(
                Decision: MembraneDecision.Refuse,
                Envelope: envelope,
                Reasons:
                [
                    new MembraneDecisionReason(
                        Code: MembraneDecisionReasonCodes.InvalidPassport,
                        Message: "envelope may not cross the membrane with an invalid constitutional passport.")
                ],
                ValidationResult: validationResult);
        }

        if (envelope.ProductClass == SymbolicProductClass.CollapseProduct)
        {
            return CreateResult(
                MembraneDecision.Collapse,
                envelope,
                validationResult,
                MembraneDecisionReasonCodes.CollapseClass,
                "valid CollapseProduct enters the collapse lane rather than ordinary bounded routing.");
        }

        if (envelope.Admissibility == AdmissibilityStatus.Pending)
        {
            return CreateResult(
                MembraneDecision.Defer,
                envelope,
                validationResult,
                MembraneDecisionReasonCodes.AdmissibilityPending,
                "pending admissibility remains candidate-bearing and may not proceed as bounded runtime obligation.");
        }

        if (envelope.Admissibility == AdmissibilityStatus.Refused)
        {
            return CreateResult(
                MembraneDecision.Refuse,
                envelope,
                validationResult,
                MembraneDecisionReasonCodes.AdmissibilityRefused,
                "refused admissibility may not cross into bounded runtime obligation.");
        }

        if (envelope.ProductClass == SymbolicProductClass.DirectiveProduct &&
            envelope.MaterializationEligibility == MaterializationEligibility.Restricted)
        {
            return CreateResult(
                MembraneDecision.Transform,
                envelope,
                validationResult,
                MembraneDecisionReasonCodes.RestrictedDirective,
                "restricted directive requires bounded normalization or constraint before later runtime handling.");
        }

        return CreateResult(
            MembraneDecision.Accept,
            envelope,
            validationResult,
            MembraneDecisionReasonCodes.AdmissibleBounded,
            "valid bounded envelope may be receipted and handed to downstream non-persistent runtime handling.");
    }

    private static MembraneDecisionResult CreateResult(
        MembraneDecision decision,
        SymbolicEnvelope envelope,
        SymbolicEnvelopeValidationResult validationResult,
        string code,
        string message)
    {
        return new MembraneDecisionResult(
            Decision: decision,
            Envelope: envelope,
            Reasons:
            [
                new MembraneDecisionReason(
                    Code: code,
                    Message: message)
            ],
            ValidationResult: validationResult);
    }
}
