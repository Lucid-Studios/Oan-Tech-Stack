using Oan.Common;

namespace Oan.Cradle
{
    internal sealed class CrypticAdmissionMembrane : ICrypticAdmissionMembrane
    {
        public Task<CrypticAdmissionResult> EvaluateAsync(
            CrypticAdmissionCandidate candidate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            cancellationToken.ThrowIfCancellationRequested();

            if (candidate.Outcome == CrypticFormationOutcome.OutOfScope)
            {
                return Task.FromResult(CreateResult(
                    CrypticAdmissionDecision.Reject,
                    "fixture-out-of-scope",
                    candidate,
                    submissionEligible: false,
                    requiresReview: false,
                    normalizedPrimePayload: null));
            }

            if (candidate.Outcome == CrypticFormationOutcome.NeedsSpecification)
            {
                return Task.FromResult(CreateResult(
                    CrypticAdmissionDecision.Defer,
                    "semantic-needs-specification",
                    candidate,
                    submissionEligible: false,
                    requiresReview: true,
                    normalizedPrimePayload: null));
            }

            if (candidate.Outcome == CrypticFormationOutcome.Rejected)
            {
                return Task.FromResult(CreateResult(
                    CrypticAdmissionDecision.Reject,
                    "candidate-rejected",
                    candidate,
                    submissionEligible: false,
                    requiresReview: false,
                    normalizedPrimePayload: null));
            }

            if (candidate.ReservedDomainViolation)
            {
                return Task.FromResult(CreateResult(
                    CrypticAdmissionDecision.Quarantine,
                    "reserved-domain-violation",
                    candidate,
                    submissionEligible: false,
                    requiresReview: true,
                    normalizedPrimePayload: null));
            }

            if (!candidate.DeterministicPrimeMaterializationSucceeded || candidate.CandidateDraft is null)
            {
                return Task.FromResult(CreateResult(
                    CrypticAdmissionDecision.Quarantine,
                    "prime-normalization-failed",
                    candidate,
                    submissionEligible: false,
                    requiresReview: true,
                    normalizedPrimePayload: null));
            }

            var normalizedPrimePayload = new PrimeClosureSubmission(
                candidate.CandidateId,
                candidate.OriginRuntime,
                candidate.OriginLane,
                candidate.CandidateDraft,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["origin-runtime"] = candidate.OriginRuntime.ToString(),
                    ["origin-lane"] = candidate.OriginLane.ToString(),
                    ["source-text"] = candidate.SourceText,
                    ["diagnostic-render"] = candidate.DiagnosticRender ?? string.Empty
                });

            return Task.FromResult(CreateResult(
                CrypticAdmissionDecision.Admit,
                "prime-submission-admitted",
                candidate,
                submissionEligible: true,
                requiresReview: false,
                normalizedPrimePayload));
        }

        private static CrypticAdmissionResult CreateResult(
            CrypticAdmissionDecision decision,
            string reasonCode,
            CrypticAdmissionCandidate candidate,
            bool submissionEligible,
            bool requiresReview,
            PrimeClosureSubmission? normalizedPrimePayload)
        {
            var telemetryTags = candidate.TelemetryTags
                .Append($"decision:{decision}")
                .Append($"reason:{reasonCode}")
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return new CrypticAdmissionResult(
                decision,
                reasonCode,
                candidate.CandidateId,
                candidate.OriginRuntime,
                candidate.OriginLane,
                submissionEligible,
                requiresReview,
                telemetryTags,
                normalizedPrimePayload);
        }
    }
}
