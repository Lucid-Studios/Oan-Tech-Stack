namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneDecisionPolicyTests
{
    private readonly DefaultMembraneDecisionPolicy _policy = new();

    [Fact]
    public void Decide_InvalidEnvelope_IsRefused()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with { Origin = " " });

        Assert.Equal(MembraneDecision.Refuse, result.Decision);
        Assert.False(result.ValidationResult.IsValid);
        Assert.Contains(result.ValidationResult.Violations, violation => violation.Code == SymbolicEnvelopeViolationCodes.OriginMissing);
        Assert.Equal(MembraneDecisionReasonCodes.InvalidPassport, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_ValidCollapseProduct_EntersCollapseLane()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.CollapseProduct,
            Intent = new SymbolicIntent("checkpoint-close"),
            MaterializationEligibility = MaterializationEligibility.Restricted,
            PersistenceEligibility = PersistenceEligibility.AuditOnly
        });

        Assert.Equal(MembraneDecision.Collapse, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.CollapseClass, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_PendingAdmissibility_IsDeferred()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with
        {
            Admissibility = AdmissibilityStatus.Pending,
            PersistenceEligibility = PersistenceEligibility.AuditOnly
        });

        Assert.Equal(MembraneDecision.Defer, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.AdmissibilityPending, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_RefusedAdmissibility_IsRefused()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with
        {
            Admissibility = AdmissibilityStatus.Refused,
            PersistenceEligibility = PersistenceEligibility.AuditOnly
        });

        Assert.Equal(MembraneDecision.Refuse, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.AdmissibilityRefused, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_RestrictedDirective_Transforms()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with
        {
            MaterializationEligibility = MaterializationEligibility.Restricted,
            PersistenceEligibility = PersistenceEligibility.AuditOnly
        });

        Assert.Equal(MembraneDecision.Transform, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.RestrictedDirective, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_AdmissibleReadProduct_IsAccepted()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope() with
        {
            ProductClass = SymbolicProductClass.ReadProduct,
            Intent = new SymbolicIntent("bounded-read"),
            MaterializationEligibility = MaterializationEligibility.No,
            PersistenceEligibility = PersistenceEligibility.Never
        });

        Assert.Equal(MembraneDecision.Accept, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.AdmissibleBounded, Assert.Single(result.Reasons).Code);
    }

    [Fact]
    public void Decide_AdmissibleDirectiveWithYesMaterialization_IsAccepted()
    {
        var result = _policy.Decide(CreateValidDirectiveEnvelope());

        Assert.Equal(MembraneDecision.Accept, result.Decision);
        Assert.True(result.ValidationResult.IsValid);
        Assert.Equal(MembraneDecisionReasonCodes.AdmissibleBounded, Assert.Single(result.Reasons).Code);
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
            TraceId: "trace://session-a/passport-003");
    }
}
