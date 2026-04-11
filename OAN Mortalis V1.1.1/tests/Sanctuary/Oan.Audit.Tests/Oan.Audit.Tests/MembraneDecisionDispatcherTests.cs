namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class MembraneDecisionDispatcherTests
{
    [Fact]
    public void Dispatch_NullDecisionResult_Throws()
    {
        var dispatcher = new DefaultMembraneDecisionDispatcher(() => "dispatch://membrane/test-001");

        Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null!));
    }

    [Fact]
    public void Dispatch_BlankDispatchId_Throws()
    {
        var dispatcher = new DefaultMembraneDecisionDispatcher(() => " ");

        Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(CreateDecisionResult(MembraneDecision.Accept)));
    }

    [Theory]
    [InlineData(MembraneDecision.Accept, MembraneDispatchLane.Accepted, MembraneDispatchNoteCodes.AcceptedLaneReceipt)]
    [InlineData(MembraneDecision.Transform, MembraneDispatchLane.Transformed, MembraneDispatchNoteCodes.TransformLaneReceipt)]
    [InlineData(MembraneDecision.Defer, MembraneDispatchLane.Deferred, MembraneDispatchNoteCodes.DeferredLaneReceipt)]
    [InlineData(MembraneDecision.Refuse, MembraneDispatchLane.Refused, MembraneDispatchNoteCodes.RefusedLaneReceipt)]
    [InlineData(MembraneDecision.Collapse, MembraneDispatchLane.Collapsed, MembraneDispatchNoteCodes.CollapsedLaneReceipt)]
    public void Dispatch_MapsDecisionIntoBoundedLane(
        MembraneDecision decision,
        MembraneDispatchLane expectedLane,
        string expectedCode)
    {
        var decisionResult = CreateDecisionResult(decision);
        var dispatcher = new DefaultMembraneDecisionDispatcher(() => "dispatch://membrane/test-002");

        var result = dispatcher.Dispatch(decisionResult);

        Assert.Equal("dispatch://membrane/test-002", result.DispatchId);
        Assert.Equal(expectedLane, result.Lane);
        Assert.Same(decisionResult, result.DecisionResult);
        Assert.Equal(expectedCode, Assert.Single(result.Notes).Code);
    }

    private static MembraneDecisionResult CreateDecisionResult(MembraneDecision decision)
    {
        var envelope = new SymbolicEnvelope(
            Origin: "sli-lisp://compass/compass-update",
            Family: new SymbolicProductFamily("AdmissibilityRead"),
            ProductClass: decision == MembraneDecision.Collapse
                ? SymbolicProductClass.CollapseProduct
                : SymbolicProductClass.DirectiveProduct,
            Intent: new SymbolicIntent(decision == MembraneDecision.Collapse
                ? "checkpoint-close"
                : "materialize-bounded-output"),
            Admissibility: decision switch
            {
                MembraneDecision.Defer => AdmissibilityStatus.Pending,
                MembraneDecision.Refuse => AdmissibilityStatus.Refused,
                _ => AdmissibilityStatus.Admissible
            },
            ContradictionState: ContradictionState.None,
            MaterializationEligibility: decision == MembraneDecision.Transform
                ? MaterializationEligibility.Restricted
                : decision == MembraneDecision.Collapse
                    ? MaterializationEligibility.Restricted
                    : MaterializationEligibility.Yes,
            PersistenceEligibility: decision switch
            {
                MembraneDecision.Collapse => PersistenceEligibility.AuditOnly,
                MembraneDecision.Defer => PersistenceEligibility.AuditOnly,
                MembraneDecision.Refuse => PersistenceEligibility.AuditOnly,
                _ => PersistenceEligibility.Promotable
            },
            TraceId: "trace://session-a/passport-004");

        var validationResult = new SymbolicEnvelopeValidationResult(
            IsValid: decision != MembraneDecision.Refuse,
            Violations: decision == MembraneDecision.Refuse
                ? [
                    new SymbolicEnvelopeViolation(
                        Code: SymbolicEnvelopeViolationCodes.OriginMissing,
                        Field: "origin",
                        Message: "invalid envelope may not cross the membrane.")
                ]
                : []);

        return new MembraneDecisionResult(
            Decision: decision,
            Envelope: envelope,
            Reasons:
            [
                new MembraneDecisionReason(
                    Code: "test-reason",
                    Message: "test decision reason")
            ],
            ValidationResult: validationResult);
    }
}
