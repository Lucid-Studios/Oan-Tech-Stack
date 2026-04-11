namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class SymbolicEnvelopeValidationTests
{
    private readonly DefaultSymbolicEnvelopeValidator _validator = new();

    [Fact]
    public void Validate_ValidDirectiveEnvelope_PassesWithoutViolations()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope());

        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Validate_OriginMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { Origin = " " });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.OriginMissing, "origin");
    }

    [Fact]
    public void Validate_FamilyMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { Family = new SymbolicProductFamily("") });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.FamilyMissing, "family");
    }

    [Fact]
    public void Validate_ProductClassMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { ProductClass = (SymbolicProductClass)999 });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.ProductClassMissing, "product_class");
    }

    [Fact]
    public void Validate_IntentMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { Intent = new SymbolicIntent(" ") });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.IntentMissing, "intent");
    }

    [Fact]
    public void Validate_AdmissibilityMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { Admissibility = (AdmissibilityStatus)999 });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.AdmissibilityMissing, "admissibility");
    }

    [Fact]
    public void Validate_ContradictionStateMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { ContradictionState = (ContradictionState)999 });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.ContradictionStateMissing, "contradiction_state");
    }

    [Fact]
    public void Validate_MaterializationEligibilityMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { MaterializationEligibility = (MaterializationEligibility)999 });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.MaterializationEligibilityMissing, "materialization_eligibility");
    }

    [Fact]
    public void Validate_PersistenceEligibilityMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { PersistenceEligibility = (PersistenceEligibility)999 });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.PersistenceEligibilityMissing, "persistence_eligibility");
    }

    [Fact]
    public void Validate_TraceIdMissing_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with { TraceId = "" });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.TraceIdMissing, "trace_id");
    }

    [Fact]
    public void Validate_ClassIntentMismatch_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.ReadProduct,
            Intent = new SymbolicIntent("materialize-bounded-output"),
            MaterializationEligibility = MaterializationEligibility.No,
            PersistenceEligibility = PersistenceEligibility.Never
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.ClassIntentMismatch, "intent");
    }

    [Fact]
    public void Validate_ContradictionAdmissibilityMismatch_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ContradictionState = ContradictionState.Hard,
            Admissibility = AdmissibilityStatus.Admissible
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.ContradictionAdmissibilityMismatch, "contradiction_state");
    }

    [Fact]
    public void Validate_MaterializationClassMismatch_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.CandidateProduct,
            MaterializationEligibility = MaterializationEligibility.Yes
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.MaterializationClassMismatch, "materialization_eligibility");
    }

    [Fact]
    public void Validate_PersistenceClassMismatch_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.CollapseProduct,
            PersistenceEligibility = PersistenceEligibility.Promotable
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.PersistenceClassMismatch, "persistence_eligibility");
    }

    [Fact]
    public void Validate_ReadProductAuthorityViolation_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.ReadProduct,
            MaterializationEligibility = MaterializationEligibility.Restricted,
            PersistenceEligibility = PersistenceEligibility.Never
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.ReadProductAuthorityViolation, "product_class");
    }

    [Fact]
    public void Validate_SelfPromotionRisk_ReturnsViolation()
    {
        var result = _validator.Validate(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.CandidateProduct,
            PersistenceEligibility = PersistenceEligibility.Promotable
        });

        AssertInvalid(result, SymbolicEnvelopeViolationCodes.SelfPromotionRisk, "persistence_eligibility");
    }

    private static SymbolicEnvelope CreateValidDirectiveEnvelope()
    {
        return new SymbolicEnvelope(
            Origin: "sli-lisp://compass/compass-update",
            Family: new SymbolicProductFamily("AdmissibilityRead"),
            ProductClass: SymbolicProductClass.DirectiveProduct,
            Intent: new SymbolicIntent("materialize-bounded-output"),
            Admissibility: AdmissibilityStatus.Admissible,
            ContradictionState: ContradictionState.None,
            MaterializationEligibility: MaterializationEligibility.Yes,
            PersistenceEligibility: PersistenceEligibility.Promotable,
            TraceId: "trace://session-a/passport-002");
    }

    private static void AssertInvalid(
        SymbolicEnvelopeValidationResult result,
        string code,
        string field)
    {
        Assert.False(result.IsValid);
        var violation = Assert.Single(result.Violations.Where(candidate => candidate.Code == code));
        Assert.Equal(field, violation.Field);
    }
}
