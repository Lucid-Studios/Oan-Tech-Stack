using SoulFrame.Host;

namespace Oan.SoulFrame.Tests;

public sealed class SoulFrameGovernedPromptContextComposerTests
{
    [Fact]
    public void Compose_BoundedLocalityContinuity_AddsDoctrineAnchors()
    {
        var constraints = new SoulFrameInferenceConstraints
        {
            Domain = "general",
            DriftLimit = 0.02,
            MaxTokens = 128
        };

        var composed = SoulFrameGovernedPromptContextComposer.Compose(
            "classify",
            "bounded-locality continuity under masked locality witness",
            constraints,
            "maintain bounded locality continuity without fluid continuity law drift");

        Assert.Contains("ACTIVE_DOCTRINE_DOMAIN: bounded-locality continuity", composed, StringComparison.Ordinal);
        Assert.Contains("EXCLUDED_NEARBY_DOMAIN: fluid continuity law", composed, StringComparison.Ordinal);
        Assert.Contains("unresolved governed state or the needs-more-information governed state", composed, StringComparison.Ordinal);
        Assert.Contains("OBJECTIVE_HINT: maintain bounded locality continuity without fluid continuity law drift", composed, StringComparison.Ordinal);
        Assert.Contains("INPUT: bounded-locality continuity under masked locality witness", composed, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveDomain_LocalityObjective_PrefersBoundedLocalityContinuity()
    {
        var domain = SoulFrameGovernedPromptContextComposer.ResolveDomain(
            rawContext: "task-objective",
            configuredDomain: "general",
            objectiveHint: "maintain identity continuity within bounded locality");

        Assert.Equal("bounded-locality continuity", domain);
    }
}
