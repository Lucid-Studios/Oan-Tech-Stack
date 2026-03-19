using SLI.Engine;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class BoundedAccountabilityPacketProgramTests
{
    [Fact]
    public async Task BoundedAccountabilityPacketProgram_RequiresFormedAdmissibleSurface()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAccountabilityPacketProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(reveal-posture narrow)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-status review-ready)"
            ],
            "identity-continuity");

        Assert.False(result.Packet.IsConfigured);
        Assert.Equal(SliAccountabilityPacketState.Blocked, result.Packet.ReadinessStatus);
        Assert.Contains(
            result.Packet.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites);
    }

    [Fact]
    public async Task BoundedAccountabilityPacketProgram_CarriesLineageAndInvariantEvidence()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAccountabilityPacketProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(reveal-posture narrow)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-status review-ready)"
            ],
            "identity-continuity");

        Assert.True(result.Packet.IsConfigured);
        Assert.Equal(SliAccountabilityPacketState.ReviewReady, result.Packet.ReadinessStatus);
        Assert.Equal(result.Surface.SurfaceHandle, result.Packet.SurfaceHandle);
        Assert.Equal(result.Transport.TransportHandle, result.Packet.TransportHandle);
        Assert.Equal(result.Witness.WitnessHandle, result.Packet.WitnessHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Packet.SourceLocalityHandle);
        Assert.Equal(result.Locality.LocalityHandle, result.Packet.TargetLocalityHandle);
        Assert.Equal(SliAdmissibleSurfaceState.ComparativeClass, result.Packet.SurfaceClass);
        Assert.False(result.Packet.IdentityBearingApplicable);
        Assert.Contains("self-anchor-polarity", result.Packet.PreservedInvariants);
        Assert.Contains("identity-nonbinding", result.Packet.PreservedInvariants);
        Assert.NotNull(result.Locality.LiveRuntimePacket);
        Assert.Equal(SliLiveEngramKind.ReturnCandidateEngram, result.Locality.LiveRuntimePacket!.EngramKind);
        Assert.Equal(SliLiveEngramRuntimeState.ReturnCandidate, result.Locality.LiveRuntimePacket.RuntimeState);
        Assert.True(result.Locality.LiveRuntimePacket.ReturnCandidateEligible);
        Assert.Contains(result.Locality.LiveRuntimePacket.InvariantSet, invariant => invariant == "self-anchor-polarity");
        Assert.Contains(result.Locality.LiveRuntimePacket.TraceSet, entry => entry.Operation == SliLiveEngramOperationKind.Witness);
        Assert.Contains(result.Locality.LiveRuntimePacket.TraceSet, entry => entry.Operation == SliLiveEngramOperationKind.ShapeReturnCandidate);
    }

    [Fact]
    public async Task PacketReveal_CannotWidenSurfaceRevealPosture()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAccountabilityPacketProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(reveal-posture narrow)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-reveal narrow)",
                "(packet-status review-ready)"
            ],
            "identity-continuity");

        Assert.Equal(SliAccountabilityPacketState.Blocked, result.Packet.ReadinessStatus);
        Assert.Contains(
            result.Packet.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidPacketReveal);
    }

    [Fact]
    public async Task PacketStatus_ResolvesReviewReadyOnlyWhenPrerequisitesHold()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAccountabilityPacketProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(reveal-posture narrow)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state identity-bearing identity-applicable)",
                "(surface-reveal narrow)",
                "(surface-status formed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-status review-ready)"
            ],
            "identity-continuity");

        Assert.Equal(SliAccountabilityPacketState.ReviewReady, result.Packet.ReadinessStatus);
        Assert.True(result.Packet.IdentityBearingApplicable);
        Assert.Equal("narrow", result.Packet.RevealPosture);
    }

    [Fact]
    public async Task PacketFormation_RemainsBlockedFromSanctuaryAndCustodySurfaces()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteAccountabilityPacketProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-status review-ready)",
                "(sanctuary-intake surface-state)",
                "(custody-write surface-state)"
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
