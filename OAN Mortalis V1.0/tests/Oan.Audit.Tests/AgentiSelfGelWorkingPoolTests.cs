using AgentiCore.Models;
using CradleTek.Memory.Models;

namespace Oan.Audit.Tests;

public sealed class AgentiSelfGelWorkingPoolTests
{
    [Fact]
    public void WorkingPool_Create_MapsHotClaims_AndBuildsCooledValidationSurface()
    {
        var pool = AgentiSelfGelWorkingPoolFactory.Create(
            sessionHandle: "soulframe-session://cme-alpha/session-1",
            workingStateHandle: "soulframe-working://cme-alpha/state-1",
            provenanceMarker: "membrane-derived:cme:cme-alpha|policy:agenticore.cognition.cycle",
            cSelfGelHandle: "soulframe-cselfgel://cme-alpha/self-1",
            activeConcepts: ["Engram", "Identity", "SLI"],
            workingMemory: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["key"] = "value"
            },
            selfClaims:
            [
                EngramSelfResolutionFactory.CreateClaim(
                    new EngramSummary
                    {
                        EngramId = "engram-1",
                        ConceptTag = "identity-continuity",
                        DecisionSpline = "cluster:alpha|concept:identity-continuity|domain:self",
                        SummaryText = "Identity continuity remains claim-bearing until cooled validation.",
                        ConfidenceWeight = 0.82
                    },
                    provenanceSource: "Lucid Research Corpus",
                    validationPosture: EngramSelfValidationPosture.HotClaim,
                    origin: EngramSelfResolutionOrigin.HotWorkingResolution,
                    validationReferenceHandle: EngramSelfResolutionFactory.CreateCooledValidationHandle("soulframe-cselfgel://cme-alpha/self-1"),
                    obstructionCode: null)
            ]);

        Assert.Equal("cooled-selfgel-validation-surface", pool.ValidationSurface.Classification);
        Assert.StartsWith("soulframe-selfgel://cme-alpha/", pool.ValidationSurface.SelfGelHandle, StringComparison.Ordinal);
        Assert.Contains("identity-continuity", pool.ValidationSurface.ValidatedConcepts, StringComparer.OrdinalIgnoreCase);
        Assert.Single(pool.Claims);
        Assert.Equal(AgentiSelfGelClaimPosture.HotClaim, pool.Claims[0].Posture);
        Assert.Equal(AgentiSelfGelClaimOrigin.HotWorkingResolution, pool.Claims[0].Origin);
        Assert.Equal(pool.ValidationSurface.SelfGelHandle, pool.Claims[0].ValidationReferenceHandle);
        Assert.Equal("hot-selfgel-claim", pool.Claims[0].Classification);
    }

    [Fact]
    public void WorkingPool_Create_RejectsHotClaimSelfPromotionIntoCooledValidation()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            AgentiSelfGelWorkingPoolFactory.Create(
                sessionHandle: "soulframe-session://cme-alpha/session-1",
                workingStateHandle: "soulframe-working://cme-alpha/state-1",
                provenanceMarker: "membrane-derived:cme:cme-alpha|policy:agenticore.cognition.cycle",
                cSelfGelHandle: "soulframe-cselfgel://cme-alpha/self-1",
                activeConcepts: ["Identity"],
                workingMemory: new Dictionary<string, string>(StringComparer.Ordinal),
                selfClaims:
                [
                    new EngramSelfResolutionClaim
                    {
                        ClaimHandle = "selfgel-claim://forged",
                        EngramId = "engram-1",
                        ConceptTag = "identity-continuity",
                        SummaryText = "Forged cooled self truth.",
                        DecisionSpline = "cluster:alpha|concept:identity-continuity|domain:self",
                        ProvenanceSource = "hot-runtime",
                        ConfidenceWeight = 0.9,
                        ValidationPosture = EngramSelfValidationPosture.CooledValidated,
                        Origin = EngramSelfResolutionOrigin.HotWorkingResolution,
                        ValidationReferenceHandle = EngramSelfResolutionFactory.CreateCooledValidationHandle("soulframe-cselfgel://cme-alpha/self-1"),
                        ObstructionCode = null
                    }
                ]));

        Assert.Contains("Hot self claims may not be promoted directly into cooled validation truth", exception.Message, StringComparison.Ordinal);
    }
}
