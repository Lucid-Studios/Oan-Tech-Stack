using CradleTek.Memory;
using CradleTek.Mantle;

namespace Oan.Runtime.IntegrationTests;

public sealed class CradleTekMemoryCarryForwardTests
{
    [Fact]
    public async Task ResolveSelfSensitiveAsync_DefaultsToHotClaimsAgainstPresentedHandle()
    {
        IGovernedEngramResolver resolver = new StubEngramResolver(
            new GovernedEngramQueryResult(
                Source: "Lucid Research Corpus",
                Summaries:
                [
                    new GovernedEngramSummary(
                        EngramId: "engram-001",
                        ConceptTag: "identity",
                        DecisionSpline: "cluster:self|concept:identity",
                        SummaryText: "Identity continuity cue.",
                        ConfidenceWeight: 0.77)
                ]));
        IGovernedSelfGelValidationHandleProjector validationHandleProjector = new GovernedSelfGelValidationHandleProjector();

        var result = await resolver.ResolveSelfSensitiveAsync(
            new GovernedEngramResolutionContext(
                ContextHandle: "memory-context://001",
                TaskObjective: "recover self continuity cues",
                RelevantFragments: ["identity continuity cue"],
                SourceSubsystem: "soulframe-stewardship"),
            validationReferenceHandle: validationHandleProjector.ProjectPresentedValidationHandle("cselfgel://abc123"));

        Assert.Equal("Lucid Research Corpus", result.Source);
        var claim = Assert.Single(result.Claims);
        Assert.Equal(GovernedEngramSelfValidationPosture.HotClaim, claim.ValidationPosture);
        Assert.Equal(GovernedEngramSelfResolutionOrigin.HotWorkingResolution, claim.Origin);
        Assert.Equal("selfgel://abc123", claim.ValidationReferenceHandle);
        Assert.StartsWith("engram-self-claim://", claim.ClaimHandle, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateClaim_RejectsHotPromotionIntoCooledValidation()
    {
        var summary = new GovernedEngramSummary(
            EngramId: "engram-002",
            ConceptTag: "continuity",
            DecisionSpline: "cluster:self|concept:continuity",
            SummaryText: "Continuity support cue.",
            ConfidenceWeight: 0.81);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            GovernedEngramSelfResolutionFactory.CreateClaim(
                summary,
                provenanceSource: "Lucid Research Corpus",
                validationPosture: GovernedEngramSelfValidationPosture.CooledValidated,
                origin: GovernedEngramSelfResolutionOrigin.HotWorkingResolution,
                validationReferenceHandle: "selfgel://validation",
                obstructionCode: null));

        Assert.Contains("may not promote", exception.Message, StringComparison.Ordinal);
    }

    private sealed class StubEngramResolver : IGovernedEngramResolver
    {
        private readonly GovernedEngramQueryResult _result;

        public StubEngramResolver(GovernedEngramQueryResult result)
        {
            _result = result;
        }

        public Task<GovernedEngramQueryResult> ResolveRelevantAsync(
            GovernedEngramResolutionContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }

        public Task<GovernedEngramQueryResult> ResolveConceptAsync(
            string concept,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }

        public Task<GovernedEngramQueryResult> ResolveClusterAsync(
            string clusterId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }
}
