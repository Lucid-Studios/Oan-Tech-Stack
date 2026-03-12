using SLI.Engine;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class BoundedRehearsalProgramTests
{
    [Fact]
    public async Task BoundedRehearsal_RequiresCompletedLocalityPerspectiveAndParticipation()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedRehearsalProgramAsync(
            [
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)"
            ],
            "identity-continuity");

        Assert.False(result.Rehearsal.IsConfigured);
        Assert.Contains(
            result.Rehearsal.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingRehearsalPrerequisites);
        Assert.Contains(
            result.Rehearsal.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompleteRehearsal);
    }

    [Fact]
    public async Task RehearsalSubstitution_ProducesCandidateVariationWithoutStateMutation()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedRehearsalProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)"
            ],
            "identity-continuity");

        Assert.True(result.Rehearsal.IsConfigured);
        Assert.Equal(SliRehearsalState.DreamGameMode, result.Rehearsal.Mode);
        Assert.Equal(SliRehearsalState.IdentitySealed, result.Rehearsal.IdentitySeal);
        Assert.Equal(SliRehearsalState.PreAdmissible, result.Rehearsal.AdmissionStatus);
        Assert.False(result.Rehearsal.IsBindable);
        Assert.Contains("branch-a", result.Rehearsal.BranchSet);
        Assert.Single(result.Rehearsal.SubstitutionLedger);
        Assert.Equal("identity-continuity", result.Rehearsal.SubstitutionLedger[0].Source);
        Assert.Equal("alternate-world", result.Rehearsal.SubstitutionLedger[0].Target);
        Assert.Empty(result.Rehearsal.Residues);

        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Locality.Participation.Mode);
        Assert.Empty(result.SymbolicTrace.Where(line => line.StartsWith("decision-branch(", StringComparison.Ordinal)));
        Assert.Empty(result.SymbolicTrace.Where(line => line.StartsWith("commit(", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task RehearsalAnalogy_ProducesResidueBearingNonBindingBranch()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedRehearsalProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-begin locality-state dream-game)",
                "(rehearsal-branch analogy-branch)",
                "(rehearsal-analogy task-objective alternate-frame)",
                "(rehearsal-residue exploratory-overhang)",
                "(rehearsal-seal identity-sealed)"
            ],
            "identity-continuity");

        Assert.True(result.Rehearsal.IsConfigured);
        Assert.Single(result.Rehearsal.AnalogyLedger);
        Assert.Equal("task-objective", result.Rehearsal.AnalogyLedger[0].Source);
        Assert.Equal("alternate-frame", result.Rehearsal.AnalogyLedger[0].Target);
        Assert.False(result.Rehearsal.IsBindable);
        Assert.Contains(
            result.Rehearsal.Residues,
            residue => residue.Detail == "exploratory-overhang");
    }

    [Fact]
    public async Task RehearsalCannotExposeSanctuaryOrCustodySurfaces()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedRehearsalProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)",
                "(sanctuary-intake locality-state)",
                "(custody-write locality-state)"
            ],
            "identity-continuity");

        Assert.Contains("unknown-op(sanctuary-intake)", result.SymbolicTrace);
        Assert.Contains("unknown-op(custody-write)", result.SymbolicTrace);
    }

    [Fact]
    public async Task RehearsalIdentitySeal_PreventsBindingPromotion()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedRehearsalProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-begin locality-state dream-game)",
                "(rehearsal-seal porous)"
            ],
            "identity-continuity");

        Assert.True(result.Rehearsal.IsConfigured);
        Assert.Equal(SliRehearsalState.IdentitySealed, result.Rehearsal.IdentitySeal);
        Assert.False(result.Rehearsal.IsBindable);
        Assert.Contains(
            result.Rehearsal.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidIdentitySeal);
    }

    private static async Task<LispBridge> CreateBridgeAsync()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        return bridge;
    }
}
