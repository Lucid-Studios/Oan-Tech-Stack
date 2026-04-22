namespace San.Common;

public interface ISymbolicEnvelopeValidator
{
    SymbolicEnvelopeValidationResult Validate(SymbolicEnvelope envelope);
}

public sealed record SymbolicEnvelopeValidationResult(
    bool IsValid,
    IReadOnlyList<SymbolicEnvelopeViolation> Violations);

public sealed record SymbolicEnvelopeViolation(
    string Code,
    string Field,
    string Message);

public static class SymbolicEnvelopeViolationCodes
{
    public const string OriginMissing = "origin-missing";
    public const string FamilyMissing = "family-missing";
    public const string ProductClassMissing = "product-class-missing";
    public const string IntentMissing = "intent-missing";
    public const string AdmissibilityMissing = "admissibility-missing";
    public const string ContradictionStateMissing = "contradiction-state-missing";
    public const string MaterializationEligibilityMissing = "materialization-eligibility-missing";
    public const string PersistenceEligibilityMissing = "persistence-eligibility-missing";
    public const string TraceIdMissing = "trace-id-missing";
    public const string ClassIntentMismatch = "class-intent-mismatch";
    public const string ContradictionAdmissibilityMismatch = "contradiction-admissibility-mismatch";
    public const string MaterializationClassMismatch = "materialization-class-mismatch";
    public const string PersistenceClassMismatch = "persistence-class-mismatch";
    public const string ReadProductAuthorityViolation = "read-product-authority-violation";
    public const string SelfPromotionRisk = "self-promotion-risk";
}

public sealed class DefaultSymbolicEnvelopeValidator : ISymbolicEnvelopeValidator
{
    private static readonly string[] ReadIntentDenylist =
    [
        "directive",
        "materialize",
        "persist",
        "promote",
        "collapse"
    ];

    private static readonly string[] CandidateIntentDenylist =
    [
        "persist",
        "promote",
        "prime-mutate"
    ];

    private static readonly string[] CollapseIntentDenylist =
    [
        "materialize",
        "persist",
        "promote"
    ];

    public SymbolicEnvelopeValidationResult Validate(SymbolicEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var violations = new List<SymbolicEnvelopeViolation>();

        RequireNonBlank(envelope.Origin, "origin", SymbolicEnvelopeViolationCodes.OriginMissing, violations);
        RequireNonBlank(envelope.Family.Value, "family", SymbolicEnvelopeViolationCodes.FamilyMissing, violations);
        RequireNonBlank(envelope.Intent.Value, "intent", SymbolicEnvelopeViolationCodes.IntentMissing, violations);
        RequireNonBlank(envelope.TraceId, "trace_id", SymbolicEnvelopeViolationCodes.TraceIdMissing, violations);

        RequireDefinedEnum(
            envelope.ProductClass,
            "product_class",
            SymbolicEnvelopeViolationCodes.ProductClassMissing,
            violations);
        RequireDefinedEnum(
            envelope.Admissibility,
            "admissibility",
            SymbolicEnvelopeViolationCodes.AdmissibilityMissing,
            violations);
        RequireDefinedEnum(
            envelope.ContradictionState,
            "contradiction_state",
            SymbolicEnvelopeViolationCodes.ContradictionStateMissing,
            violations);
        RequireDefinedEnum(
            envelope.MaterializationEligibility,
            "materialization_eligibility",
            SymbolicEnvelopeViolationCodes.MaterializationEligibilityMissing,
            violations);
        RequireDefinedEnum(
            envelope.PersistenceEligibility,
            "persistence_eligibility",
            SymbolicEnvelopeViolationCodes.PersistenceEligibilityMissing,
            violations);

        if (!Enum.IsDefined(envelope.ProductClass))
        {
            return CreateResult(violations);
        }

        ValidateClassIntent(envelope, violations);
        ValidateReadProductAuthority(envelope, violations);
        ValidateMaterializationClassCoherence(envelope, violations);
        ValidatePersistenceClassCoherence(envelope, violations);
        ValidateContradictionAdmissibilityCoherence(envelope, violations);
        ValidateSelfPromotionRisk(envelope, violations);

        return CreateResult(violations);
    }

    private static SymbolicEnvelopeValidationResult CreateResult(IReadOnlyList<SymbolicEnvelopeViolation> violations)
    {
        return new SymbolicEnvelopeValidationResult(
            IsValid: violations.Count == 0,
            Violations: violations);
    }

    private static void RequireNonBlank(
        string? value,
        string field,
        string code,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: code,
                Field: field,
                Message: $"{field} must be explicit at the membrane."));
        }
    }

    private static void RequireDefinedEnum<TEnum>(
        TEnum value,
        string field,
        string code,
        ICollection<SymbolicEnvelopeViolation> violations)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: code,
                Field: field,
                Message: $"{field} must resolve to a defined bounded value."));
        }
    }

    private static void ValidateClassIntent(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        var intent = envelope.Intent.Value ?? string.Empty;
        var denylist = envelope.ProductClass switch
        {
            SymbolicProductClass.ReadProduct => ReadIntentDenylist,
            SymbolicProductClass.CandidateProduct => CandidateIntentDenylist,
            SymbolicProductClass.CollapseProduct => CollapseIntentDenylist,
            _ => []
        };

        if (denylist.Any(token => intent.Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.ClassIntentMismatch,
                Field: "intent",
                Message: $"intent may not outrun the ceiling of {envelope.ProductClass}."));
        }
    }

    private static void ValidateReadProductAuthority(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        if (envelope.ProductClass != SymbolicProductClass.ReadProduct)
        {
            return;
        }

        if (envelope.MaterializationEligibility != MaterializationEligibility.No ||
            envelope.PersistenceEligibility != PersistenceEligibility.Never)
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.ReadProductAuthorityViolation,
                Field: "product_class",
                Message: "ReadProduct must remain observational and may not imply action or persistence authority."));
        }
    }

    private static void ValidateMaterializationClassCoherence(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        var allowed = envelope.ProductClass switch
        {
            SymbolicProductClass.ReadProduct =>
                envelope.MaterializationEligibility == MaterializationEligibility.No,
            SymbolicProductClass.CandidateProduct =>
                envelope.MaterializationEligibility is MaterializationEligibility.No or MaterializationEligibility.Restricted,
            SymbolicProductClass.DirectiveProduct =>
                envelope.MaterializationEligibility is MaterializationEligibility.Restricted or MaterializationEligibility.Yes,
            SymbolicProductClass.CollapseProduct =>
                envelope.MaterializationEligibility is MaterializationEligibility.No or MaterializationEligibility.Restricted,
            _ => false
        };

        if (!allowed)
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.MaterializationClassMismatch,
                Field: "materialization_eligibility",
                Message: $"materialization_eligibility outruns the ceiling of {envelope.ProductClass}."));
        }
    }

    private static void ValidatePersistenceClassCoherence(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        var allowed = envelope.ProductClass switch
        {
            SymbolicProductClass.ReadProduct =>
                envelope.PersistenceEligibility == PersistenceEligibility.Never,
            SymbolicProductClass.CandidateProduct =>
                envelope.PersistenceEligibility is PersistenceEligibility.Never or PersistenceEligibility.AuditOnly,
            SymbolicProductClass.DirectiveProduct =>
                envelope.PersistenceEligibility is PersistenceEligibility.Never or PersistenceEligibility.AuditOnly or PersistenceEligibility.Promotable,
            SymbolicProductClass.CollapseProduct =>
                envelope.PersistenceEligibility is PersistenceEligibility.Never or PersistenceEligibility.AuditOnly,
            _ => false
        };

        if (!allowed)
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.PersistenceClassMismatch,
                Field: "persistence_eligibility",
                Message: $"persistence_eligibility outruns the ceiling of {envelope.ProductClass}."));
        }
    }

    private static void ValidateContradictionAdmissibilityCoherence(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        if (envelope.ContradictionState == ContradictionState.Hard &&
            envelope.Admissibility == AdmissibilityStatus.Admissible)
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.ContradictionAdmissibilityMismatch,
                Field: "contradiction_state",
                Message: "hard contradiction may not coexist with ordinary admissible posture."));
        }
    }

    private static void ValidateSelfPromotionRisk(
        SymbolicEnvelope envelope,
        ICollection<SymbolicEnvelopeViolation> violations)
    {
        if (envelope.PersistenceEligibility != PersistenceEligibility.Promotable)
        {
            return;
        }

        if (envelope.ProductClass != SymbolicProductClass.DirectiveProduct ||
            envelope.Admissibility != AdmissibilityStatus.Admissible ||
            envelope.ContradictionState != ContradictionState.None)
        {
            violations.Add(new SymbolicEnvelopeViolation(
                Code: SymbolicEnvelopeViolationCodes.SelfPromotionRisk,
                Field: "persistence_eligibility",
                Message: "promotable persistence may only appear on contradiction-free admissible DirectiveProduct envelopes."));
        }
    }
}
