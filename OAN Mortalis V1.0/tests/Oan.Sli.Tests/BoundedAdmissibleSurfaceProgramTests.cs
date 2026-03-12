using SLI.Engine;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class BoundedAdmissibleSurfaceProgramTests
{
    [Fact]
    public async Task ValidTransportAndExplicitSurfaceClass_FormsAdmissibleSurface()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAdmissibleSurfaceProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)"
            ],
            "identity-continuity");

        Assert.True(result.Surface.IsConfigured);
        Assert.Equal(SliAdmissibleSurfaceState.Formed, result.Surface.Status);
        Assert.Equal(result.Transport.TransportHandle, result.Surface.TransportHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Surface.SourceLocalityHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Surface.TargetLocalityHandle);
        Assert.Equal(SliAdmissibleSurfaceState.ComparativeClass, result.Surface.SurfaceClass);
        Assert.False(result.Surface.IdentityBearingApplicable);
        Assert.Contains("witness-preserved-structure", result.Surface.EvidenceSet);
    }

    [Fact]
    public async Task MissingCompletedTransport_BlocksSurfaceWithResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAdmissibleSurfaceProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(admissible-surface-bounded transport-state informational informational-only)",
                "(surface-status formed)"
            ],
            "identity-continuity");

        Assert.False(result.Surface.IsConfigured);
        Assert.Equal(SliAdmissibleSurfaceState.Blocked, result.Surface.Status);
        Assert.Contains(
            result.Surface.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites);
    }

    [Fact]
    public async Task IdentityBearingClass_WithoutApplicability_BlocksSurface()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAdmissibleSurfaceProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state identity-bearing informational-only)",
                "(surface-status formed)"
            ],
            "identity-continuity");

        Assert.Equal(SliAdmissibleSurfaceState.Blocked, result.Surface.Status);
        Assert.Contains(
            result.Surface.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability);
    }

    [Fact]
    public async Task WidenedRevealOrBoundaryAttempt_BlocksSurface()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAdmissibleSurfaceProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(seal-posture sealed)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-reveal narrow)",
                "(surface-status formed)"
            ],
            "identity-continuity");

        Assert.Equal(SliAdmissibleSurfaceState.Blocked, result.Surface.Status);
        Assert.Contains(
            result.Surface.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidSurfaceReveal);
        Assert.Contains(
            result.Surface.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidSurfaceBoundary);
    }

    [Fact]
    public async Task AdmissibleSurfaceCannotInvokeSanctuaryOrCustodySurfaces()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAdmissibleSurfaceProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)",
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
