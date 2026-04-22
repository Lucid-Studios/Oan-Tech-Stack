using System.Security.Cryptography;
using System.Text;

namespace San.Common;

public enum CrypticDerivationScope
{
    MaskedSummary = 0,
    StructuralValidation = 1,
    AuthorizedFieldSlice = 2,
    WholeProtectedSet = 3
}

public enum CrypticDerivationDecision
{
    Granted = 0,
    Deferred = 1,
    Refused = 2
}

public enum CrypticDerivationReuseConstraint
{
    NoReuse = 0,
    SamePurposeOnly = 1,
    BondedPurposeOnly = 2
}

public sealed record CrypticDerivationDirective(
    string LawHandle,
    string AuthorityClass,
    IReadOnlyList<string> ApprovedPurposes,
    CrypticDerivationScope MaxScope,
    IReadOnlyList<string> ApprovedFieldSelectors,
    bool RequiresBondedAuthority,
    bool WholeSetDerivationAllowed,
    CrypticDerivationReuseConstraint ReuseConstraint,
    bool OnwardDisclosureAllowed);

public sealed record CrypticDerivationRequest(
    Guid IdentityId,
    string ProtectedHandle,
    string RequestedBy,
    string Purpose,
    string ScopeHandle,
    string TraceHandle,
    CrypticDerivationScope RequestedScope,
    IReadOnlyList<string> RequestedFieldSelectors,
    CrypticDerivationDirective Directive,
    BondedAuthorityContext? BondedAuthorityContext = null);

public sealed record CrypticDerivationReceipt(
    string ReceiptHandle,
    string LawHandle,
    Guid IdentityId,
    string ProtectedHandle,
    string RequestedBy,
    string Purpose,
    string ScopeHandle,
    string TraceHandle,
    CrypticDerivationScope GrantedScope,
    bool PartialDerivationOnly,
    bool WholeSetGranted,
    IReadOnlyList<string> GrantedFieldSelectors,
    CrypticDerivationReuseConstraint ReuseConstraint,
    bool OnwardDisclosureAllowed,
    string? AuthorityId,
    string? AuthorityClass,
    DateTimeOffset TimestampUtc);

public sealed record CrypticDerivationResult(
    CrypticDerivationDecision Decision,
    string ReasonCode,
    Guid IdentityId,
    string ProtectedHandle,
    string LawHandle,
    string RequestedBy,
    string Purpose,
    string ScopeHandle,
    string TraceHandle,
    CrypticDerivationScope RequestedScope,
    CrypticDerivationScope? GrantedScope,
    bool RequiresBondedAuthority,
    bool PartialDerivationOnly,
    bool WholeSetGranted,
    IReadOnlyList<string> GrantedFieldSelectors,
    IReadOnlyList<string> WithheldFieldSelectors,
    CrypticDerivationReuseConstraint ReuseConstraint,
    bool OnwardDisclosureAllowed,
    CrypticDerivationReceipt? Receipt);

public interface ICrypticDerivationPolicy
{
    CrypticDerivationResult Evaluate(CrypticDerivationRequest request);
}

public sealed class DefaultCrypticDerivationPolicy : ICrypticDerivationPolicy
{
    public CrypticDerivationResult Evaluate(CrypticDerivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProtectedHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RequestedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ScopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TraceHandle);
        ArgumentNullException.ThrowIfNull(request.Directive);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Directive.LawHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Directive.AuthorityClass);

        var approvedPurposes = NormalizeSelectors(request.Directive.ApprovedPurposes);
        var approvedFieldSelectors = NormalizeSelectors(request.Directive.ApprovedFieldSelectors);
        var requestedFieldSelectors = NormalizeSelectors(request.RequestedFieldSelectors);
        var requiresBondedAuthority =
            request.Directive.RequiresBondedAuthority ||
            request.RequestedScope is CrypticDerivationScope.AuthorizedFieldSlice or CrypticDerivationScope.WholeProtectedSet;

        if (!approvedPurposes.Contains(request.Purpose.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return Refused(
                request,
                requiresBondedAuthority,
                "purpose-not-approved-by-law");
        }

        if (request.RequestedScope > request.Directive.MaxScope)
        {
            return Refused(
                request,
                requiresBondedAuthority,
                "requested-scope-exceeds-lawful-directive");
        }

        if (request.RequestedScope == CrypticDerivationScope.WholeProtectedSet &&
            !request.Directive.WholeSetDerivationAllowed)
        {
            return Refused(
                request,
                requiresBondedAuthority,
                "whole-protected-set-not-lawful");
        }

        if (requiresBondedAuthority)
        {
            if (request.BondedAuthorityContext is null || !request.BondedAuthorityContext.BondedConfirmed)
            {
                return Deferred(
                    request,
                    requiresBondedAuthority,
                    "bonded-authority-required");
            }

            if (!string.Equals(
                    request.BondedAuthorityContext.AuthorityClass,
                    request.Directive.AuthorityClass,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Refused(
                    request,
                    requiresBondedAuthority,
                    "authority-class-mismatch");
            }

            if (!request.BondedAuthorityContext.ApprovedRevealPurposes
                    .Contains(request.Purpose.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                return Refused(
                    request,
                    requiresBondedAuthority,
                    "purpose-not-approved-by-bonded-authority");
            }
        }

        return request.RequestedScope switch
        {
            CrypticDerivationScope.MaskedSummary => Granted(
                request,
                requiresBondedAuthority,
                "masked-summary-approved",
                CrypticDerivationScope.MaskedSummary,
                partialDerivationOnly: true,
                wholeSetGranted: false,
                grantedFieldSelectors: []),
            CrypticDerivationScope.StructuralValidation => Granted(
                request,
                requiresBondedAuthority,
                "structural-validation-approved",
                CrypticDerivationScope.StructuralValidation,
                partialDerivationOnly: true,
                wholeSetGranted: false,
                grantedFieldSelectors: []),
            CrypticDerivationScope.AuthorizedFieldSlice => EvaluateFieldSlice(
                request,
                requiresBondedAuthority,
                requestedFieldSelectors,
                approvedFieldSelectors),
            CrypticDerivationScope.WholeProtectedSet => Granted(
                request,
                requiresBondedAuthority,
                "whole-protected-set-approved",
                CrypticDerivationScope.WholeProtectedSet,
                partialDerivationOnly: false,
                wholeSetGranted: true,
                grantedFieldSelectors: []),
            _ => Refused(
                request,
                requiresBondedAuthority,
                "unknown-cryptic-derivation-scope")
        };
    }

    private static CrypticDerivationResult EvaluateFieldSlice(
        CrypticDerivationRequest request,
        bool requiresBondedAuthority,
        IReadOnlyList<string> requestedFieldSelectors,
        IReadOnlyList<string> approvedFieldSelectors)
    {
        if (requestedFieldSelectors.Count == 0)
        {
            return Refused(
                request,
                requiresBondedAuthority,
                "field-slice-selectors-required");
        }

        var grantedFieldSelectors = requestedFieldSelectors
            .Where(selector => approvedFieldSelectors.Contains(selector, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(selector => selector, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (grantedFieldSelectors.Length == 0)
        {
            return Refused(
                request,
                requiresBondedAuthority,
                "requested-fields-not-approved");
        }

        var withheldFieldSelectors = requestedFieldSelectors
            .Where(selector => !grantedFieldSelectors.Contains(selector, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(selector => selector, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Granted(
            request,
            requiresBondedAuthority,
            withheldFieldSelectors.Length == 0
                ? "authorized-field-slice-approved"
                : "authorized-field-slice-partial-grant",
            CrypticDerivationScope.AuthorizedFieldSlice,
            partialDerivationOnly: true,
            wholeSetGranted: false,
            grantedFieldSelectors: grantedFieldSelectors,
            withheldFieldSelectors: withheldFieldSelectors);
    }

    private static CrypticDerivationResult Granted(
        CrypticDerivationRequest request,
        bool requiresBondedAuthority,
        string reasonCode,
        CrypticDerivationScope grantedScope,
        bool partialDerivationOnly,
        bool wholeSetGranted,
        IReadOnlyList<string> grantedFieldSelectors,
        IReadOnlyList<string>? withheldFieldSelectors = null)
    {
        var receipt = CreateReceipt(
            request,
            grantedScope,
            partialDerivationOnly,
            wholeSetGranted,
            grantedFieldSelectors);

        return new CrypticDerivationResult(
            Decision: CrypticDerivationDecision.Granted,
            ReasonCode: reasonCode,
            IdentityId: request.IdentityId,
            ProtectedHandle: request.ProtectedHandle.Trim(),
            LawHandle: request.Directive.LawHandle.Trim(),
            RequestedBy: request.RequestedBy.Trim(),
            Purpose: request.Purpose.Trim(),
            ScopeHandle: request.ScopeHandle.Trim(),
            TraceHandle: request.TraceHandle.Trim(),
            RequestedScope: request.RequestedScope,
            GrantedScope: grantedScope,
            RequiresBondedAuthority: requiresBondedAuthority,
            PartialDerivationOnly: partialDerivationOnly,
            WholeSetGranted: wholeSetGranted,
            GrantedFieldSelectors: grantedFieldSelectors,
            WithheldFieldSelectors: withheldFieldSelectors ?? [],
            ReuseConstraint: request.Directive.ReuseConstraint,
            OnwardDisclosureAllowed: request.Directive.OnwardDisclosureAllowed,
            Receipt: receipt);
    }

    private static CrypticDerivationResult Deferred(
        CrypticDerivationRequest request,
        bool requiresBondedAuthority,
        string reasonCode)
    {
        return new CrypticDerivationResult(
            Decision: CrypticDerivationDecision.Deferred,
            ReasonCode: reasonCode,
            IdentityId: request.IdentityId,
            ProtectedHandle: request.ProtectedHandle.Trim(),
            LawHandle: request.Directive.LawHandle.Trim(),
            RequestedBy: request.RequestedBy.Trim(),
            Purpose: request.Purpose.Trim(),
            ScopeHandle: request.ScopeHandle.Trim(),
            TraceHandle: request.TraceHandle.Trim(),
            RequestedScope: request.RequestedScope,
            GrantedScope: null,
            RequiresBondedAuthority: requiresBondedAuthority,
            PartialDerivationOnly: false,
            WholeSetGranted: false,
            GrantedFieldSelectors: [],
            WithheldFieldSelectors: [],
            ReuseConstraint: CrypticDerivationReuseConstraint.NoReuse,
            OnwardDisclosureAllowed: false,
            Receipt: null);
    }

    private static CrypticDerivationResult Refused(
        CrypticDerivationRequest request,
        bool requiresBondedAuthority,
        string reasonCode)
    {
        return new CrypticDerivationResult(
            Decision: CrypticDerivationDecision.Refused,
            ReasonCode: reasonCode,
            IdentityId: request.IdentityId,
            ProtectedHandle: request.ProtectedHandle.Trim(),
            LawHandle: request.Directive.LawHandle.Trim(),
            RequestedBy: request.RequestedBy.Trim(),
            Purpose: request.Purpose.Trim(),
            ScopeHandle: request.ScopeHandle.Trim(),
            TraceHandle: request.TraceHandle.Trim(),
            RequestedScope: request.RequestedScope,
            GrantedScope: null,
            RequiresBondedAuthority: requiresBondedAuthority,
            PartialDerivationOnly: false,
            WholeSetGranted: false,
            GrantedFieldSelectors: [],
            WithheldFieldSelectors: [],
            ReuseConstraint: CrypticDerivationReuseConstraint.NoReuse,
            OnwardDisclosureAllowed: false,
            Receipt: null);
    }

    private static CrypticDerivationReceipt CreateReceipt(
        CrypticDerivationRequest request,
        CrypticDerivationScope grantedScope,
        bool partialDerivationOnly,
        bool wholeSetGranted,
        IReadOnlyList<string> grantedFieldSelectors)
    {
        var receiptHandle = CreateDeterministicHandle(
            "cryptic-derivation://",
            request.Directive.LawHandle.Trim(),
            request.IdentityId.ToString("D"),
            request.ProtectedHandle.Trim(),
            request.RequestedBy.Trim(),
            request.Purpose.Trim(),
            request.ScopeHandle.Trim(),
            request.TraceHandle.Trim(),
            grantedScope.ToString(),
            string.Join(",", grantedFieldSelectors));

        return new CrypticDerivationReceipt(
            ReceiptHandle: receiptHandle,
            LawHandle: request.Directive.LawHandle.Trim(),
            IdentityId: request.IdentityId,
            ProtectedHandle: request.ProtectedHandle.Trim(),
            RequestedBy: request.RequestedBy.Trim(),
            Purpose: request.Purpose.Trim(),
            ScopeHandle: request.ScopeHandle.Trim(),
            TraceHandle: request.TraceHandle.Trim(),
            GrantedScope: grantedScope,
            PartialDerivationOnly: partialDerivationOnly,
            WholeSetGranted: wholeSetGranted,
            GrantedFieldSelectors: grantedFieldSelectors,
            ReuseConstraint: request.Directive.ReuseConstraint,
            OnwardDisclosureAllowed: request.Directive.OnwardDisclosureAllowed,
            AuthorityId: request.BondedAuthorityContext?.AuthorityId,
            AuthorityClass: request.BondedAuthorityContext?.AuthorityClass,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<string> NormalizeSelectors(IReadOnlyList<string> selectors)
    {
        ArgumentNullException.ThrowIfNull(selectors);

        return selectors
            .Where(selector => !string.IsNullOrWhiteSpace(selector))
            .Select(selector => selector.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(selector => selector, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string CreateDeterministicHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
