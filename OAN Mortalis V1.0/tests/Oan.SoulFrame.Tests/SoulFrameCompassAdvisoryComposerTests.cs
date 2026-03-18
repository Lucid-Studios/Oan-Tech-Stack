using Oan.Common;
using SoulFrame.Host;

namespace Oan.SoulFrame.Tests;

public sealed class SoulFrameCompassAdvisoryComposerTests
{
    [Fact]
    public void CreateSeedRequired_BoundedLocalityObjective_YieldsBoundedPair()
    {
        var request = SoulFrameCompassAdvisoryComposer.CreateSeedRequired(
            new SoulFrameInferenceConstraints
            {
                Domain = "general",
                DriftLimit = 0.02,
                MaxTokens = 128
            },
            "maintain bounded locality continuity under masked locality witness");

        Assert.Equal(CompassDoctrineBasin.BoundedLocalityContinuity, request.TargetActiveBasin);
        Assert.Equal(CompassDoctrineBasin.FluidContinuityLaw, request.ExcludedCompetingBasin);
    }

    [Fact]
    public void CreateSeedRequired_IdentityContinuityObjective_YieldsIdentityHint()
    {
        var request = SoulFrameCompassAdvisoryComposer.CreateSeedRequired(
            new SoulFrameInferenceConstraints
            {
                Domain = "general",
                DriftLimit = 0.02,
                MaxTokens = 128
            },
            "preserve identity continuity under sparse witness");

        Assert.Equal(CompassDoctrineBasin.IdentityContinuity, request.TargetActiveBasin);
        Assert.Equal(CompassDoctrineBasin.Unknown, request.ExcludedCompetingBasin);
    }
}
