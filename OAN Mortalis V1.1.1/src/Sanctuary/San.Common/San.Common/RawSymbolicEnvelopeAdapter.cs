namespace San.Common;

public sealed record RawSymbolicProduct(
    string? Origin,
    string? Family,
    string? ProductClass,
    string? Intent,
    string? Admissibility,
    string? ContradictionState,
    string? MaterializationEligibility,
    string? PersistenceEligibility,
    string? TraceId);

public interface IRawSymbolicEnvelopeAdapter
{
    RawSymbolicEnvelopeAdaptationResult Adapt(RawSymbolicProduct rawProduct);
}

public sealed record RawSymbolicEnvelopeAdaptationResult(
    bool IsSuccess,
    SymbolicEnvelope? Envelope,
    IReadOnlyList<RawSymbolicEnvelopeAdaptationIssue> Issues);

public sealed record RawSymbolicEnvelopeAdaptationIssue(
    string Code,
    string Field,
    string Message);

public static class RawSymbolicEnvelopeAdaptationIssueCodes
{
    public const string OriginMissing = "origin-missing";
    public const string FamilyMissing = "family-missing";
    public const string ProductClassMissing = "product-class-missing";
    public const string ProductClassUnknown = "product-class-unknown";
    public const string IntentMissing = "intent-missing";
    public const string AdmissibilityMissing = "admissibility-missing";
    public const string AdmissibilityUnknown = "admissibility-unknown";
    public const string ContradictionStateMissing = "contradiction-state-missing";
    public const string ContradictionStateUnknown = "contradiction-state-unknown";
    public const string MaterializationEligibilityMissing = "materialization-eligibility-missing";
    public const string MaterializationEligibilityUnknown = "materialization-eligibility-unknown";
    public const string PersistenceEligibilityMissing = "persistence-eligibility-missing";
    public const string PersistenceEligibilityUnknown = "persistence-eligibility-unknown";
    public const string TraceIdMissing = "trace-id-missing";
}

public sealed class DefaultRawSymbolicEnvelopeAdapter : IRawSymbolicEnvelopeAdapter
{
    public RawSymbolicEnvelopeAdaptationResult Adapt(RawSymbolicProduct rawProduct)
    {
        ArgumentNullException.ThrowIfNull(rawProduct);

        var issues = new List<RawSymbolicEnvelopeAdaptationIssue>();

        var origin = RequireNonBlank(rawProduct.Origin, "origin", RawSymbolicEnvelopeAdaptationIssueCodes.OriginMissing, issues);
        var family = RequireNonBlank(rawProduct.Family, "family", RawSymbolicEnvelopeAdaptationIssueCodes.FamilyMissing, issues);
        var productClassToken = RequireNonBlank(rawProduct.ProductClass, "product_class", RawSymbolicEnvelopeAdaptationIssueCodes.ProductClassMissing, issues);
        var intent = RequireNonBlank(rawProduct.Intent, "intent", RawSymbolicEnvelopeAdaptationIssueCodes.IntentMissing, issues);
        var admissibilityToken = RequireNonBlank(rawProduct.Admissibility, "admissibility", RawSymbolicEnvelopeAdaptationIssueCodes.AdmissibilityMissing, issues);
        var contradictionToken = RequireNonBlank(rawProduct.ContradictionState, "contradiction_state", RawSymbolicEnvelopeAdaptationIssueCodes.ContradictionStateMissing, issues);
        var materializationToken = RequireNonBlank(rawProduct.MaterializationEligibility, "materialization_eligibility", RawSymbolicEnvelopeAdaptationIssueCodes.MaterializationEligibilityMissing, issues);
        var persistenceToken = RequireNonBlank(rawProduct.PersistenceEligibility, "persistence_eligibility", RawSymbolicEnvelopeAdaptationIssueCodes.PersistenceEligibilityMissing, issues);
        var traceId = RequireNonBlank(rawProduct.TraceId, "trace_id", RawSymbolicEnvelopeAdaptationIssueCodes.TraceIdMissing, issues);

        var productClass = ParseProductClass(productClassToken, issues);
        var admissibility = ParseEnum(
            admissibilityToken,
            "admissibility",
            RawSymbolicEnvelopeAdaptationIssueCodes.AdmissibilityUnknown,
            new Dictionary<string, AdmissibilityStatus>(StringComparer.OrdinalIgnoreCase)
            {
                ["pending"] = AdmissibilityStatus.Pending,
                ["admissible"] = AdmissibilityStatus.Admissible,
                ["refused"] = AdmissibilityStatus.Refused
            },
            issues);
        var contradictionState = ParseEnum(
            contradictionToken,
            "contradiction_state",
            RawSymbolicEnvelopeAdaptationIssueCodes.ContradictionStateUnknown,
            new Dictionary<string, ContradictionState>(StringComparer.OrdinalIgnoreCase)
            {
                ["none"] = ContradictionState.None,
                ["soft"] = ContradictionState.Soft,
                ["hard"] = ContradictionState.Hard
            },
            issues);
        var materializationEligibility = ParseEnum(
            materializationToken,
            "materialization_eligibility",
            RawSymbolicEnvelopeAdaptationIssueCodes.MaterializationEligibilityUnknown,
            new Dictionary<string, MaterializationEligibility>(StringComparer.OrdinalIgnoreCase)
            {
                ["no"] = MaterializationEligibility.No,
                ["restricted"] = MaterializationEligibility.Restricted,
                ["yes"] = MaterializationEligibility.Yes
            },
            issues);
        var persistenceEligibility = ParseEnum(
            persistenceToken,
            "persistence_eligibility",
            RawSymbolicEnvelopeAdaptationIssueCodes.PersistenceEligibilityUnknown,
            new Dictionary<string, PersistenceEligibility>(StringComparer.OrdinalIgnoreCase)
            {
                ["never"] = PersistenceEligibility.Never,
                ["audit_only"] = PersistenceEligibility.AuditOnly,
                ["audit-only"] = PersistenceEligibility.AuditOnly,
                ["auditonly"] = PersistenceEligibility.AuditOnly,
                ["promotable"] = PersistenceEligibility.Promotable
            },
            issues);

        if (issues.Count > 0 ||
            productClass is null ||
            admissibility is null ||
            contradictionState is null ||
            materializationEligibility is null ||
            persistenceEligibility is null)
        {
            return new RawSymbolicEnvelopeAdaptationResult(
                IsSuccess: false,
                Envelope: null,
                Issues: issues);
        }

        return new RawSymbolicEnvelopeAdaptationResult(
            IsSuccess: true,
            Envelope: new SymbolicEnvelope(
                Origin: origin!,
                Family: new SymbolicProductFamily(family!),
                ProductClass: productClass.Value,
                Intent: new SymbolicIntent(intent!),
                Admissibility: admissibility.Value,
                ContradictionState: contradictionState.Value,
                MaterializationEligibility: materializationEligibility.Value,
                PersistenceEligibility: persistenceEligibility.Value,
                TraceId: traceId!),
            Issues: []);
    }

    private static string? RequireNonBlank(
        string? value,
        string field,
        string code,
        ICollection<RawSymbolicEnvelopeAdaptationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new RawSymbolicEnvelopeAdaptationIssue(
                Code: code,
                Field: field,
                Message: $"{field} must be explicit in raw symbolic ingress."));
            return null;
        }

        return value.Trim();
    }

    private static SymbolicProductClass? ParseProductClass(
        string? token,
        ICollection<RawSymbolicEnvelopeAdaptationIssue> issues)
    {
        if (token is null)
        {
            return null;
        }

        var normalized = NormalizeToken(token);
        return normalized switch
        {
            "readproduct" => SymbolicProductClass.ReadProduct,
            "candidateproduct" => SymbolicProductClass.CandidateProduct,
            "directiveproduct" => SymbolicProductClass.DirectiveProduct,
            "collapseproduct" => SymbolicProductClass.CollapseProduct,
            _ => AddUnknownProductClass(issues)
        };
    }

    private static TEnum? ParseEnum<TEnum>(
        string? token,
        string field,
        string code,
        IReadOnlyDictionary<string, TEnum> map,
        ICollection<RawSymbolicEnvelopeAdaptationIssue> issues)
        where TEnum : struct
    {
        if (token is null)
        {
            return null;
        }

        var normalized = NormalizeToken(token);
        foreach (var entry in map)
        {
            if (NormalizeToken(entry.Key) == normalized)
            {
                return entry.Value;
            }
        }

        issues.Add(new RawSymbolicEnvelopeAdaptationIssue(
            Code: code,
            Field: field,
            Message: $"{field} token '{token}' is not lawfully recognized by the membrane adapter."));
        return null;
    }

    private static SymbolicProductClass? AddUnknownProductClass(
        ICollection<RawSymbolicEnvelopeAdaptationIssue> issues)
    {
        issues.Add(new RawSymbolicEnvelopeAdaptationIssue(
            Code: RawSymbolicEnvelopeAdaptationIssueCodes.ProductClassUnknown,
            Field: "product_class",
            Message: "product_class token is not lawfully recognized by the membrane adapter."));
        return null;
    }

    private static string NormalizeToken(string token)
    {
        return token
            .Trim()
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
    }
}
