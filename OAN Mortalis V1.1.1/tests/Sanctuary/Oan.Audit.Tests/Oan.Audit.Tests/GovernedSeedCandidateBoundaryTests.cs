using San.Common;

namespace San.Audit.Tests;

public sealed class GovernedSeedCandidateBoundaryTests
{
    [Fact]
    public void CandidateEnvelope_Exposes_Only_Candidate_And_Observation_Surfaces()
    {
        var forbiddenNames = new[]
        {
            "IsStanding",
            "PermissionGranted",
            "AdmittedDomain",
            "BoundRole",
            "ActionDisposition"
        };

        var propertyNames = typeof(GovernedSeedCandidateEnvelope)
            .GetProperties()
            .Select(static property => property.Name)
            .ToArray();

        Assert.DoesNotContain(forbiddenNames, forbidden => propertyNames.Contains(forbidden, StringComparer.Ordinal));
    }

    [Fact]
    public void BoundaryReceipt_Can_Record_CandidateOnly_Intake_Without_Authority_Promotion()
    {
        var envelope = GovernedSeedCandidateEnvelope.Empty(
            candidateId: "candidate://boundary/a",
            sourceType: GovernedSeedCandidateSourceType.LispProposal,
            sourceChannel: "sli.lisp",
            observedAtUtc: new DateTimeOffset(2026, 04, 17, 12, 00, 00, TimeSpan.Zero),
            priorContinuityReference: "continuity://seed/a");

        var receipt = new GovernedSeedCandidateBoundaryReceipt(
            CandidateId: envelope.CandidateId,
            SourceType: envelope.SourceType,
            SourceChannel: envelope.SourceChannel,
            ObservedAtUtc: envelope.ObservedAtUtc,
            ContainsAuthorityBearingFields: false,
            CandidateProposalCount: envelope.CandidateProposals.Count,
            HoldingMutationProposalCount: envelope.HoldingMutationProposals.Count,
            ResonanceObservationCount: envelope.ResonanceObservations.Count,
            DescendantProposalCount: envelope.DescendantProposals.Count,
            CollapseSuggestionCount: envelope.CollapseSuggestions.Count,
            Summary: "Candidate entered intake as candidate-only proposal material.");

        Assert.False(receipt.ContainsAuthorityBearingFields);
        Assert.Equal(GovernedSeedCandidateSourceType.LispProposal, receipt.SourceType);
        Assert.Equal("sli.lisp", receipt.SourceChannel);
        Assert.Equal(0, receipt.CandidateProposalCount);
    }
}
