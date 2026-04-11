using San.Common;

namespace SLI.Engine;

public interface ICrypticFloorEvaluator
{
    CrypticFloorEvaluation Evaluate(PredicateLandingRequest request);
}

public sealed class CrypticFloorEvaluator : ICrypticFloorEvaluator
{
    public CrypticFloorEvaluation Evaluate(PredicateLandingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.MembraneDecision is MembraneDecision.Defer or MembraneDecision.Collapse)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "membrane-decision-withheld",
                GovernanceTrace: "membrane-decision-must-admit-before-predicate-landing",
                Envelope: request.Envelope);
        }

        if (request.MembraneDecision == MembraneDecision.Refuse)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Refuse,
                OutcomeCode: "membrane-decision-refused",
                GovernanceTrace: "refused-symbolic-product-may-not-land",
                Envelope: request.Envelope);
        }

        if (request.Envelope.Admissibility != AdmissibilityStatus.Admissible)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Refuse,
                OutcomeCode: "symbolic-admissibility-required",
                GovernanceTrace: "passported-symbolic-product-must-be-admissible",
                Envelope: request.Envelope);
        }

        if (request.Envelope.ContradictionState == ContradictionState.Hard)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Refuse,
                OutcomeCode: "hard-contradiction-present",
                GovernanceTrace: "hard-contradiction-blocks-predicate-landing",
                Envelope: request.Envelope);
        }

        if (string.IsNullOrWhiteSpace(request.SanctuaryGelHandle))
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "sanctuary-gel-bootstrap-required",
                GovernanceTrace: "sanctuary-gel-bootstrap-required-before-engine-landing",
                Envelope: request.Envelope);
        }

        if (string.IsNullOrWhiteSpace(request.IssuedRtmeHandle))
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "issued-rtme-service-required",
                GovernanceTrace: "issued-sanctuary-rtme-service-required-before-engine-landing",
                Envelope: request.Envelope);
        }

        if (string.IsNullOrWhiteSpace(request.RouteHandle))
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "predicate-landing-route-required",
                GovernanceTrace: "bounded-route-handle-required-for-predicate-landing",
                Envelope: request.Envelope);
        }

        if (request.Envelope.ProductClass is SymbolicProductClass.DirectiveProduct or SymbolicProductClass.CollapseProduct)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "product-class-not-yet-landable",
                GovernanceTrace: "only-read-and-candidate-products-may-land-in-the-first-engine-batch",
                Envelope: request.Envelope);
        }

        if (request.Envelope.MaterializationEligibility == MaterializationEligibility.No &&
            request.Envelope.PersistenceEligibility == PersistenceEligibility.Never)
        {
            return new CrypticFloorEvaluation(
                PredicateLandingReady: false,
                Disposition: CrypticFloorDisposition.Withhold,
                OutcomeCode: "landing-eligibility-insufficient",
                GovernanceTrace: "symbolic-product-lacks-minimum-landing-eligibility",
                Envelope: request.Envelope);
        }

        var governanceTrace = request.RouteKind == PredicateLandingRouteKind.BoundedEcTransit
            ? "predicate-landing-surface-ready-via-bounded-ec-transit"
            : "predicate-landing-surface-ready-via-direct-transit";

        return new CrypticFloorEvaluation(
            PredicateLandingReady: true,
            Disposition: CrypticFloorDisposition.Ready,
            OutcomeCode: "predicate-landing-ready",
            GovernanceTrace: governanceTrace,
            Envelope: request.Envelope);
    }
}
