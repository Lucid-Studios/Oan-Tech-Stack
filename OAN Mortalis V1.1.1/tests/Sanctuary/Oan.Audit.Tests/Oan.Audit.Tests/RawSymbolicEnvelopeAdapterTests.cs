namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class RawSymbolicEnvelopeAdapterTests
{
    private readonly DefaultRawSymbolicEnvelopeAdapter _adapter = new();

    [Fact]
    public void Adapt_ValidRawProduct_ProducesSymbolicEnvelope()
    {
        var result = _adapter.Adapt(new RawSymbolicProduct(
            Origin: "  sli-lisp://compass/compass-update  ",
            Family: "  AdmissibilityRead  ",
            ProductClass: "directive_product",
            Intent: "  materialize-bounded-output  ",
            Admissibility: "admissible",
            ContradictionState: "none",
            MaterializationEligibility: "yes",
            PersistenceEligibility: "audit_only",
            TraceId: "  trace://session-a/raw-001  "));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Issues);
        Assert.NotNull(result.Envelope);
        Assert.Equal("sli-lisp://compass/compass-update", result.Envelope!.Origin);
        Assert.Equal("AdmissibilityRead", result.Envelope.Family.Value);
        Assert.Equal(SymbolicProductClass.DirectiveProduct, result.Envelope.ProductClass);
        Assert.Equal("materialize-bounded-output", result.Envelope.Intent.Value);
        Assert.Equal(AdmissibilityStatus.Admissible, result.Envelope.Admissibility);
        Assert.Equal(ContradictionState.None, result.Envelope.ContradictionState);
        Assert.Equal(MaterializationEligibility.Yes, result.Envelope.MaterializationEligibility);
        Assert.Equal(PersistenceEligibility.AuditOnly, result.Envelope.PersistenceEligibility);
        Assert.Equal("trace://session-a/raw-001", result.Envelope.TraceId);
    }

    [Fact]
    public void Adapt_MissingOrigin_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { Origin = " " });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.OriginMissing, "origin");
    }

    [Fact]
    public void Adapt_MissingFamily_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { Family = null });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.FamilyMissing, "family");
    }

    [Fact]
    public void Adapt_MissingProductClass_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { ProductClass = "" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.ProductClassMissing, "product_class");
    }

    [Fact]
    public void Adapt_UnknownProductClass_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { ProductClass = "governor_product" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.ProductClassUnknown, "product_class");
    }

    [Fact]
    public void Adapt_MissingIntent_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { Intent = " " });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.IntentMissing, "intent");
    }

    [Fact]
    public void Adapt_MissingAdmissibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { Admissibility = null });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.AdmissibilityMissing, "admissibility");
    }

    [Fact]
    public void Adapt_UnknownAdmissibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { Admissibility = "approved-ish" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.AdmissibilityUnknown, "admissibility");
    }

    [Fact]
    public void Adapt_MissingContradictionState_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { ContradictionState = "" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.ContradictionStateMissing, "contradiction_state");
    }

    [Fact]
    public void Adapt_UnknownContradictionState_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { ContradictionState = "severe" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.ContradictionStateUnknown, "contradiction_state");
    }

    [Fact]
    public void Adapt_MissingMaterializationEligibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { MaterializationEligibility = null });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.MaterializationEligibilityMissing, "materialization_eligibility");
    }

    [Fact]
    public void Adapt_UnknownMaterializationEligibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { MaterializationEligibility = "maybe" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.MaterializationEligibilityUnknown, "materialization_eligibility");
    }

    [Fact]
    public void Adapt_MissingPersistenceEligibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { PersistenceEligibility = " " });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.PersistenceEligibilityMissing, "persistence_eligibility");
    }

    [Fact]
    public void Adapt_UnknownPersistenceEligibility_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { PersistenceEligibility = "carry-forward" });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.PersistenceEligibilityUnknown, "persistence_eligibility");
    }

    [Fact]
    public void Adapt_MissingTraceId_FailsClosed()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with { TraceId = null });

        AssertInvalid(result, RawSymbolicEnvelopeAdaptationIssueCodes.TraceIdMissing, "trace_id");
    }

    [Fact]
    public void Adapt_RecognizesSnakeHyphenAndTightTokens()
    {
        var result = _adapter.Adapt(CreateValidRawProduct() with
        {
            ProductClass = "read-product",
            Admissibility = "pending",
            ContradictionState = "soft",
            MaterializationEligibility = "no",
            PersistenceEligibility = "audit-only"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Envelope);
        Assert.Equal(SymbolicProductClass.ReadProduct, result.Envelope!.ProductClass);
        Assert.Equal(AdmissibilityStatus.Pending, result.Envelope.Admissibility);
        Assert.Equal(ContradictionState.Soft, result.Envelope.ContradictionState);
        Assert.Equal(MaterializationEligibility.No, result.Envelope.MaterializationEligibility);
        Assert.Equal(PersistenceEligibility.AuditOnly, result.Envelope.PersistenceEligibility);
    }

    private static RawSymbolicProduct CreateValidRawProduct()
    {
        return new RawSymbolicProduct(
            Origin: "sli-lisp://compass/compass-update",
            Family: "AdmissibilityRead",
            ProductClass: "DirectiveProduct",
            Intent: "materialize-bounded-output",
            Admissibility: "admissible",
            ContradictionState: "none",
            MaterializationEligibility: "yes",
            PersistenceEligibility: "audit_only",
            TraceId: "trace://session-a/raw-002");
    }

    private static void AssertInvalid(
        RawSymbolicEnvelopeAdaptationResult result,
        string code,
        string field)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Envelope);
        var issue = Assert.Single(result.Issues.Where(candidate => candidate.Code == code));
        Assert.Equal(field, issue.Field);
    }
}
