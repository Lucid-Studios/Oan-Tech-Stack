using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;

namespace Oan.Sli.Tests;

public sealed class EngramResolverSelfSensitiveTests
{
    [Fact]
    public async Task ResolveSelfSensitiveAsync_ReturnsHotClaims_ForSelfObjective()
    {
        var resolver = new EngramResolverService();

        var result = await resolver.ResolveSelfSensitiveAsync(
            new CognitionContext
            {
                CMEId = "cme-test",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "identity continuity self preservation",
                RelevantEngrams = []
            },
            "soulframe-cselfgel://cme-test/self-1");

        Assert.NotEmpty(result.Claims);
        Assert.Equal("Lucid Research Corpus", result.Source);
        Assert.Equal(EngramSelfValidationPosture.HotClaim, result.Claims[0].ValidationPosture);
        Assert.All(
            result.Claims,
            claim =>
            {
                Assert.NotEqual(EngramSelfValidationPosture.CooledValidated, claim.ValidationPosture);
                Assert.Equal(EngramSelfResolutionOrigin.HotWorkingResolution, claim.Origin);
            });
        Assert.StartsWith("soulframe-selfgel://", result.Claims[0].ValidationReferenceHandle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResolveSelfSensitiveAsync_ReturnsContradictedTopClaim_ForConflictObjective()
    {
        var resolver = new EngramResolverService();

        var result = await resolver.ResolveSelfSensitiveAsync(
            new CognitionContext
            {
                CMEId = "cme-test",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "identity continuity self mismatch other",
                RelevantEngrams = []
            },
            "soulframe-cselfgel://cme-test/self-1");

        Assert.NotEmpty(result.Claims);
        Assert.Equal(EngramSelfValidationPosture.Contradicted, result.Claims[0].ValidationPosture);
        Assert.Equal("self-claim-conflict", result.Claims[0].ObstructionCode);
        Assert.StartsWith("soulframe-selfgel://", result.Claims[0].ValidationReferenceHandle, StringComparison.Ordinal);
    }
}
