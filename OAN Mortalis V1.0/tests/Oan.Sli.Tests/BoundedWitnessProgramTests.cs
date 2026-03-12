using CradleTek.Memory.Services;
using SLI.Engine;
using SLI.Engine.Models;
using SLI.Engine.Runtime;
using SLI.Engine.Parser;

namespace Oan.Sli.Tests;

public sealed class BoundedWitnessProgramTests
{
    [Fact]
    public async Task BoundedWitness_RequiresCompletedLocalityPerspectiveAndParticipation()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedWitnessProgramAsync(
            [
                "(witness-branch-compare locality-state branch-a)"
            ],
            "identity-continuity");

        Assert.False(result.Witness.IsConfigured);
        Assert.Equal(SliWitnessState.NonCandidate, result.Witness.CandidacyStatus);
        Assert.Contains(
            result.Witness.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingWitnessPrerequisites);
        Assert.Contains(
            result.Witness.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompleteWitness);
    }

    [Fact]
    public async Task SameSourceDifferentBranch_ProducesLawfulDifferenceWithResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedWitnessProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)",
                "(witness-branch-compare locality-state branch-a)"
            ],
            "identity-continuity");

        Assert.True(result.Witness.IsConfigured);
        Assert.Equal(SliWitnessState.MorphismCandidate, result.Witness.CandidacyStatus);
        Assert.Equal(result.Locality.LocalityHandle, result.Witness.LeftLocalityHandle);
        Assert.Contains(":branch:branch-a", result.Witness.RightLocalityHandle, StringComparison.Ordinal);
        Assert.Contains("rehearsal-branch", result.Witness.DifferenceSet);
        Assert.Contains("substitution", result.Witness.DifferenceSet);
        Assert.Contains(
            result.Witness.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.LawfulDifferenceResidue &&
                residue.Detail == "branch-local-comparison");
    }

    [Fact]
    public async Task PreservedAnchorsRemainPreservedAcrossWitnessComparison()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedWitnessProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)"
            ],
            "identity-continuity");

        Assert.True(result.Witness.IsConfigured);
        Assert.Contains("self-anchor-polarity", result.Witness.PreservedInvariants);
        Assert.Contains("other-anchor-polarity", result.Witness.PreservedInvariants);
        Assert.Contains("relation-anchor-polarity", result.Witness.PreservedInvariants);
        Assert.Contains("seal-posture-bound", result.Witness.PreservedInvariants);
        Assert.Contains("reveal-posture-bound", result.Witness.PreservedInvariants);
        Assert.Contains("participation-mode-limit", result.Witness.PreservedInvariants);
        Assert.Contains("identity-nonbinding", result.Witness.PreservedInvariants);
    }

    [Fact]
    public async Task InvalidComparison_NeverProducesCandidacy()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedWitnessProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)",
                "(witness-branch-compare locality-state branch-b)"
            ],
            "identity-continuity");

        Assert.False(result.Witness.IsConfigured);
        Assert.Equal(SliWitnessState.NonCandidate, result.Witness.CandidacyStatus);
        Assert.Contains(
            result.Witness.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidWitnessReference);
    }

    [Fact]
    public async Task WitnessCannotInvokeSanctuaryOrCustodySurfaces()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedWitnessProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(sanctuary-intake locality-state)",
                "(custody-write locality-state)"
            ],
            "identity-continuity");

        Assert.Contains("unknown-op(sanctuary-intake)", result.SymbolicTrace);
        Assert.Contains("unknown-op(custody-write)", result.SymbolicTrace);
    }

    [Fact]
    public async Task MorphismCandidacy_DoesNotMutateAuthoritySurfaces()
    {
        var context = await ExecuteContextAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)"
            ]);

        Assert.Empty(context.MorphologyState.ResolvedLemmaRoots);
        Assert.Equal(string.Empty, context.PropositionState.PredicateRoot);
        Assert.Equal("defer", context.FinalDecision);
        Assert.Empty(context.CandidateBranches);
        Assert.Empty(context.PrunedBranches);
        Assert.Equal(SliWitnessState.MorphismCandidate, context.HigherOrderLocalityState.Witness.CandidacyStatus);
    }

    private static async Task<LispBridge> CreateBridgeAsync()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        return bridge;
    }

    private static async Task<SliExecutionContext> ExecuteContextAsync(IReadOnlyList<string> symbolicProgram)
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(SLI.Lisp.LispModuleCatalog.LoadModules());
        var interpreter = new SliInterpreter(new SliSymbolTable());
        var program = expander.ExpandProgram(parser.ParseProgram(symbolicProgram));
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "witness-invariant-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = "identity-continuity",
                Engrams = []
            },
            new EngramResolverService());

        await interpreter.ExecuteProgramAsync(program, context);
        return context;
    }
}
