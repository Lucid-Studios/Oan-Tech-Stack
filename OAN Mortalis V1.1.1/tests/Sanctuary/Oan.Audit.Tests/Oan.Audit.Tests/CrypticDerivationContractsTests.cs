namespace Oan.Audit.Tests;

using San.Common;

public sealed class CrypticDerivationContractsTests
{
    private static readonly DefaultCrypticDerivationPolicy Policy = new();

    [Fact]
    public void MaskedSummary_IsGrantedUnderPurposeBoundLaw()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.MaskedSummary,
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.StructuralValidation,
                requiresBondedAuthority: false)));

        Assert.Equal(CrypticDerivationDecision.Granted, result.Decision);
        Assert.Equal("masked-summary-approved", result.ReasonCode);
        Assert.Equal(CrypticDerivationScope.MaskedSummary, result.GrantedScope);
        Assert.True(result.PartialDerivationOnly);
        Assert.False(result.WholeSetGranted);
        Assert.NotNull(result.Receipt);
    }

    [Fact]
    public void AuthorizedFieldSlice_IsDeferredWithoutBondedAuthority()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.AuthorizedFieldSlice,
            requestedFieldSelectors: ["cadence", "anchor-state"],
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.AuthorizedFieldSlice,
                requiresBondedAuthority: true),
            bondedAuthorityContext: null));

        Assert.Equal(CrypticDerivationDecision.Deferred, result.Decision);
        Assert.Equal("bonded-authority-required", result.ReasonCode);
        Assert.True(result.RequiresBondedAuthority);
        Assert.Null(result.Receipt);
    }

    [Fact]
    public void AuthorizedFieldSlice_GrantsOnlyApprovedFields()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.AuthorizedFieldSlice,
            requestedFieldSelectors: ["cadence", "anchor-state", "continuity-score"],
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.AuthorizedFieldSlice,
                approvedFieldSelectors: ["anchor-state", "cadence"],
                requiresBondedAuthority: true),
            bondedAuthorityContext: CreateBondedAuthority()));

        Assert.Equal(CrypticDerivationDecision.Granted, result.Decision);
        Assert.Equal("authorized-field-slice-partial-grant", result.ReasonCode);
        Assert.Equal(CrypticDerivationScope.AuthorizedFieldSlice, result.GrantedScope);
        Assert.True(result.PartialDerivationOnly);
        Assert.False(result.WholeSetGranted);
        Assert.Equal(["anchor-state", "cadence"], result.GrantedFieldSelectors);
        Assert.Equal(["continuity-score"], result.WithheldFieldSelectors);
        Assert.NotNull(result.Receipt);
        Assert.Equal(CrypticDerivationReuseConstraint.BondedPurposeOnly, result.ReuseConstraint);
        Assert.False(result.OnwardDisclosureAllowed);
    }

    [Fact]
    public void WholeProtectedSet_IsRefusedWhenLawDoesNotAllowWholeSet()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.WholeProtectedSet,
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.WholeProtectedSet,
                wholeSetDerivationAllowed: false,
                requiresBondedAuthority: true),
            bondedAuthorityContext: CreateBondedAuthority()));

        Assert.Equal(CrypticDerivationDecision.Refused, result.Decision);
        Assert.Equal("whole-protected-set-not-lawful", result.ReasonCode);
        Assert.Null(result.Receipt);
    }

    [Fact]
    public void WholeProtectedSet_IsGrantedOnlyUnderExplicitWholeSetLaw()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.WholeProtectedSet,
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.WholeProtectedSet,
                wholeSetDerivationAllowed: true,
                requiresBondedAuthority: true,
                onwardDisclosureAllowed: false,
                reuseConstraint: CrypticDerivationReuseConstraint.BondedPurposeOnly),
            bondedAuthorityContext: CreateBondedAuthority()));

        Assert.Equal(CrypticDerivationDecision.Granted, result.Decision);
        Assert.Equal("whole-protected-set-approved", result.ReasonCode);
        Assert.Equal(CrypticDerivationScope.WholeProtectedSet, result.GrantedScope);
        Assert.False(result.PartialDerivationOnly);
        Assert.True(result.WholeSetGranted);
        Assert.NotNull(result.Receipt);
        Assert.False(result.OnwardDisclosureAllowed);
        Assert.Equal(CrypticDerivationReuseConstraint.BondedPurposeOnly, result.ReuseConstraint);
    }

    [Fact]
    public void Request_IsRefusedWhenPurposeIsNotApprovedByLaw()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.StructuralValidation,
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.StructuralValidation,
                approvedPurposes: ["bond-review"]),
            purpose: "debug-export"));

        Assert.Equal(CrypticDerivationDecision.Refused, result.Decision);
        Assert.Equal("purpose-not-approved-by-law", result.ReasonCode);
    }

    [Fact]
    public void Request_IsRefusedWhenAuthorityClassDoesNotMatchLaw()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.AuthorizedFieldSlice,
            requestedFieldSelectors: ["anchor-state"],
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.AuthorizedFieldSlice,
                authorityClass: "BondedOperator",
                requiresBondedAuthority: true),
            bondedAuthorityContext: new BondedAuthorityContext(
                AuthorityId: "authority://reviewer",
                AuthorityClass: "StewardOperator",
                BondedConfirmed: true,
                ApprovedRevealPurposes: ["bond-review"])));

        Assert.Equal(CrypticDerivationDecision.Refused, result.Decision);
        Assert.Equal("authority-class-mismatch", result.ReasonCode);
    }

    [Fact]
    public void Request_IsRefusedWhenBondedAuthorityHasNotApprovedPurpose()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.AuthorizedFieldSlice,
            requestedFieldSelectors: ["anchor-state"],
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.AuthorizedFieldSlice,
                requiresBondedAuthority: true),
            bondedAuthorityContext: new BondedAuthorityContext(
                AuthorityId: "authority://bonded-reviewer",
                AuthorityClass: "BondedOperator",
                BondedConfirmed: true,
                ApprovedRevealPurposes: ["care-review"])));

        Assert.Equal(CrypticDerivationDecision.Refused, result.Decision);
        Assert.Equal("purpose-not-approved-by-bonded-authority", result.ReasonCode);
    }

    [Fact]
    public void Request_IsRefusedWhenRequestedScopeExceedsDirective()
    {
        var result = Policy.Evaluate(CreateRequest(
            requestedScope: CrypticDerivationScope.AuthorizedFieldSlice,
            requestedFieldSelectors: ["anchor-state"],
            directive: CreateDirective(
                maxScope: CrypticDerivationScope.StructuralValidation,
                requiresBondedAuthority: true),
            bondedAuthorityContext: CreateBondedAuthority()));

        Assert.Equal(CrypticDerivationDecision.Refused, result.Decision);
        Assert.Equal("requested-scope-exceeds-lawful-directive", result.ReasonCode);
    }

    private static CrypticDerivationRequest CreateRequest(
        CrypticDerivationScope requestedScope,
        CrypticDerivationDirective directive,
        IReadOnlyList<string>? requestedFieldSelectors = null,
        BondedAuthorityContext? bondedAuthorityContext = null,
        string purpose = "bond-review")
    {
        return new CrypticDerivationRequest(
            IdentityId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            ProtectedHandle: "cryptic://identity/subject-a",
            RequestedBy: "operator://bonded-review",
            Purpose: purpose,
            ScopeHandle: "scope://review/session-a",
            TraceHandle: "trace://request/session-a",
            RequestedScope: requestedScope,
            RequestedFieldSelectors: requestedFieldSelectors ?? [],
            Directive: directive,
            BondedAuthorityContext: bondedAuthorityContext);
    }

    private static CrypticDerivationDirective CreateDirective(
        CrypticDerivationScope maxScope,
        string authorityClass = "BondedOperator",
        IReadOnlyList<string>? approvedPurposes = null,
        IReadOnlyList<string>? approvedFieldSelectors = null,
        bool requiresBondedAuthority = false,
        bool wholeSetDerivationAllowed = false,
        CrypticDerivationReuseConstraint reuseConstraint = CrypticDerivationReuseConstraint.BondedPurposeOnly,
        bool onwardDisclosureAllowed = false)
    {
        return new CrypticDerivationDirective(
            LawHandle: "law://cryptic-derivation/review",
            AuthorityClass: authorityClass,
            ApprovedPurposes: approvedPurposes ?? ["bond-review"],
            MaxScope: maxScope,
            ApprovedFieldSelectors: approvedFieldSelectors ?? ["anchor-state", "cadence"],
            RequiresBondedAuthority: requiresBondedAuthority,
            WholeSetDerivationAllowed: wholeSetDerivationAllowed,
            ReuseConstraint: reuseConstraint,
            OnwardDisclosureAllowed: onwardDisclosureAllowed);
    }

    private static BondedAuthorityContext CreateBondedAuthority()
    {
        return new BondedAuthorityContext(
            AuthorityId: "authority://bonded-reviewer",
            AuthorityClass: "BondedOperator",
            BondedConfirmed: true,
            ApprovedRevealPurposes: ["bond-review"]);
    }
}
