using SLI.Engine;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class HigherOrderLocalityProgramTests
{
    [Fact]
    public async Task LocalityBootstrap_AppliesSafeDefaultsAtBindTime()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)"
            ],
            "identity-continuity");

        Assert.Equal("cme-self", result.SelfAnchor);
        Assert.Equal("task-objective", result.OtherAnchor);
        Assert.Equal("identity-continuity", result.RelationAnchor);
        Assert.Equal(SliHigherOrderLocalityState.BoundedSealPosture, result.SealPosture);
        Assert.Equal(SliHigherOrderLocalityState.MaskedRevealPosture, result.RevealPosture);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains("locality-bind(context)", result.SymbolicTrace);
    }

    [Fact]
    public async Task PerspectiveConfig_WithoutAnchors_ProducesTypedResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bind context)",
                "(perspective-configure locality-state)"
            ],
            "identity-continuity");

        Assert.False(result.Perspective.IsConfigured);
        Assert.Contains(
            result.Perspective.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingAnchorPrerequisites);
        Assert.Contains(
            result.Perspective.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompletePerspective);
    }

    [Fact]
    public async Task ParticipationConfig_WithoutPerspective_ProducesTypedResidue_AndStaysObserve()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(participation-configure locality-state)"
            ],
            "identity-continuity");

        Assert.False(result.Participation.IsConfigured);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains(
            result.Participation.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompleteParticipation);
    }

    [Fact]
    public async Task InvalidPostureValue_FallsBackToSafeDefault_AndRecordsResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bind context)",
                "(seal-posture permeable)",
                "(reveal-posture panoramic)"
            ],
            "identity-continuity");

        Assert.Equal(SliHigherOrderLocalityState.BoundedSealPosture, result.SealPosture);
        Assert.Equal(SliHigherOrderLocalityState.MaskedRevealPosture, result.RevealPosture);
        Assert.Contains(result.Residues, residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidPostureValue);
    }

    [Fact]
    public async Task InvalidParticipationMode_FallsBackToObserve_AndRecordsResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-configure locality-state)",
                "(participation-mode improvise)"
            ],
            "identity-continuity");

        Assert.True(result.Participation.IsConfigured);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains(
            result.Participation.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidParticipationMode);
    }

    [Fact]
    public async Task DifferentCompositePrograms_ProduceDifferentHigherOrderLocalityResults()
    {
        var bridge = await CreateBridgeAsync();

        var boundedObserver = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ],
            "identity-continuity");

        var alternateObserver = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state alternate-focus context-preservation)",
                "(participation-bounded-cme locality-state)"
            ],
            "identity-continuity");

        Assert.NotEqual(
            boundedObserver.Perspective.OrientationVector.Keys.Single(),
            alternateObserver.Perspective.OrientationVector.Keys.Single());
        Assert.NotEqual(
            boundedObserver.Perspective.EthicalConstraints.Single(),
            alternateObserver.Perspective.EthicalConstraints.Single());
    }

    [Fact]
    public async Task FutureWitnessOrTransportOps_RemainUnknownInSprintAB()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(rehearsal-branch locality-state)",
                "(witness-packet locality-state)",
                "(transport-begin locality-state)"
            ],
            "identity-continuity");

        Assert.Contains("unknown-op(rehearsal-branch)", result.SymbolicTrace);
        Assert.Contains("unknown-op(witness-packet)", result.SymbolicTrace);
        Assert.Contains("unknown-op(transport-begin)", result.SymbolicTrace);
    }

    private static async Task<LispBridge> CreateBridgeAsync()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        return bridge;
    }
}
