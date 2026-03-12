using SLI.Engine;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class BoundedTransportProgramTests
{
    [Fact]
    public async Task ValidWitnessAndExplicitTarget_CompletesInternalTransport()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)"
            ],
            "identity-continuity");

        Assert.True(result.Transport.IsConfigured);
        Assert.Equal(SliTransportState.Completed, result.Transport.Status);
        Assert.Equal(result.Witness.WitnessHandle, result.Transport.WitnessHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Transport.SourceLocalityHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Transport.TargetLocalityHandle);
        Assert.Contains("self-anchor-polarity", result.Transport.PreservedInvariants);
        Assert.Contains("identity-nonbinding", result.Transport.PreservedInvariants);
    }

    [Fact]
    public async Task WitnessDifferenceMapping_CompletesBranchTransport()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)",
                "(witness-branch-compare locality-state branch-a)",
                "(transport-bounded witness-state locality-state branch-a)",
                "(transport-map rehearsal-branch branch-variant)",
                "(transport-map substitution substitution)",
                "(transport-status completed)"
            ],
            "identity-continuity");

        Assert.Equal(SliTransportState.Completed, result.Transport.Status);
        Assert.Equal(2, result.Transport.MappedDifferences.Count);
        Assert.Contains(
            result.Transport.MappedDifferences,
            mapping => mapping.Source == "rehearsal-branch" && mapping.Target == "branch-variant");
        Assert.Contains(
            result.Transport.MappedDifferences,
            mapping => mapping.Source == "substitution" && mapping.Target == "substitution");
    }

    [Fact]
    public async Task MissingCandidacy_BlocksTransportWithResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)"
            ],
            "identity-continuity");

        Assert.False(result.Transport.IsConfigured);
        Assert.Equal(SliTransportState.Blocked, result.Transport.Status);
        Assert.Contains(
            result.Transport.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingTransportPrerequisites);
    }

    [Fact]
    public async Task PolarityMismatch_BlocksTransport()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-map self-anchor-polarity flipped-self)",
                "(transport-status completed)"
            ],
            "identity-continuity");

        Assert.Equal(SliTransportState.Blocked, result.Transport.Status);
        Assert.Contains(
            result.Transport.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidTransportMapping);
    }

    [Fact]
    public async Task WidenedSealRevealAttempt_BlocksTransport()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-map seal-posture-bound porous)",
                "(transport-map reveal-posture-bound panoramic)",
                "(transport-status completed)"
            ],
            "identity-continuity");

        Assert.Equal(SliTransportState.Blocked, result.Transport.Status);
        Assert.Contains(
            result.Transport.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidTransportMapping);
    }

    [Fact]
    public async Task TransportCannotInvokeSanctuaryOrCustodySurfaces()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteBoundedTransportProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(sanctuary-intake locality-state)",
                "(custody-write locality-state)"
            ],
            "identity-continuity");

        Assert.Contains("unknown-op(sanctuary-intake)", result.SymbolicTrace);
        Assert.Contains("unknown-op(custody-write)", result.SymbolicTrace);
    }

    private static async Task<LispBridge> CreateBridgeAsync()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        return bridge;
    }
}
