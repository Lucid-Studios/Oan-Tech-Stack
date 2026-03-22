using Oan.Common;
using SLI.Engine.Nexus;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class CrypticWebNexusTests
{
    [Fact]
    public void Nexus_ProjectsReadyField_FromJoinedSnapshot()
    {
        var snapshot = new SliExecutionSnapshot(
            SliSnapshotRetentionPosture.DebugOnly,
            true,
            "trace-ready",
            "commit",
            "branch-ready",
            "[]",
            ["trace-1"],
            [],
            [],
            [
                new LocalityShardSnapshot("acting", SliLocalityShardKind.Acting, "cryptic://acting", "exec", "root-a", "boundary-a", SliLocalityShardLifecycleState.Joined),
                new LocalityShardSnapshot("witness", SliLocalityShardKind.Witnessing, "cryptic://witness", "exec", "root-a", "boundary-w", SliLocalityShardLifecycleState.Joined)
            ],
            [
                new LocalityRelationEventSnapshot("rel-1", "acting", "witness", SliLocalityRelationKind.WitnessOf, SliLocalityTelemetryCarryPolicy.TraceAndWitness, true, SliLocalityRelationOutcomeKind.Joined, "joined", "cycle-1")
            ],
            [],
            [
                new ActualizationWebbingEventSnapshot(SliActualizationStageKind.Witness, "witness bridge formed", "acting", "cryptic://acting", "cycle-1")
            ],
            new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.NonSelfOperational, SliActualizationContradictionClass.None, SliActualizationValidationRoute.NoneRequired, SliActualizationDisposition.Actualized, false, false, false, SliLocalityRelationOutcomeKind.Joined, "joined", []),
            new ZedThetaCandidateSnapshot("candidate", "obj", "prime", "theta", "gamma", CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Held, CompassSelfTouchClass.NoTouch, CompassOeCoePosture.OeDominant, null, new RuntimeCeilingSnapshot(true)),
            new LiveRuntimeRunSnapshot("exec", true, "acting", SliLocalityRelationOutcomeKind.Joined, "joined"));

        var nexus = CrypticWebNexusFactory.Create(snapshot);
        var topology = nexus.CaptureTopologySnapshot();
        var relaxation = nexus.CaptureRelaxationReceipt();
        var telemetry = nexus.CaptureTelemetryFrame();

        Assert.Equal(WebFieldState.ReadyForReentry, topology.FieldState);
        Assert.Contains("cryptic://acting", topology.ActiveRegions);
        Assert.Contains("cryptic://acting", topology.BraidRegions);
        Assert.True(relaxation.ReadyForReentry);
        Assert.Equal(RelaxationState.ReadyForReentry, relaxation.RelaxationState);
        Assert.Equal(NexusReadinessState.ReadyForReentry, telemetry.ReadinessState);
    }

    [Fact]
    public void Nexus_ProjectsRelaxingField_FromDeferredSnapshot()
    {
        var snapshot = new SliExecutionSnapshot(
            SliSnapshotRetentionPosture.DebugOnly,
            true,
            "trace-deferred",
            "defer",
            "branch-deferred",
            "[]",
            ["trace-1"],
            [],
            [],
            [
                new LocalityShardSnapshot("acting", SliLocalityShardKind.Acting, "cryptic://acting", "exec", "root-a", "boundary-a", SliLocalityShardLifecycleState.WaitingForJoin)
            ],
            [
                new LocalityRelationEventSnapshot("rel-1", "acting", "acting", SliLocalityRelationKind.AdjacentTo, SliLocalityTelemetryCarryPolicy.TraceOnly, true, SliLocalityRelationOutcomeKind.Deferred, "waiting-for-join", "cycle-1")
            ],
            [],
            [
                new ActualizationWebbingEventSnapshot(SliActualizationStageKind.Bloom, "field still settling", "acting", "cryptic://acting", "cycle-1")
            ],
            new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.NonSelfOperational, SliActualizationContradictionClass.None, SliActualizationValidationRoute.ResidueOnly, SliActualizationDisposition.Deferred, false, false, false, SliLocalityRelationOutcomeKind.Deferred, "waiting-for-join", ["reduction:Deferred:waiting-for-join"]),
            new ZedThetaCandidateSnapshot("candidate", "obj", "prime", "theta", "gamma", CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Weakened, CompassSelfTouchClass.NoTouch, CompassOeCoePosture.OeDominant, null, new RuntimeCeilingSnapshot(true)),
            new LiveRuntimeRunSnapshot("exec", true, "acting", SliLocalityRelationOutcomeKind.Deferred, "waiting-for-join"));

        var nexus = CrypticWebNexusFactory.Create(snapshot);
        var topology = nexus.CaptureTopologySnapshot();
        var relaxation = nexus.CaptureRelaxationReceipt();
        var mutation = Assert.Single(nexus.CaptureMutationEvents());

        Assert.Equal(WebFieldState.Relaxing, topology.FieldState);
        Assert.False(relaxation.ReadyForReentry);
        Assert.Equal(RelaxationState.IncompleteRelaxation, relaxation.RelaxationState);
        Assert.Equal(MutationEventState.Deferred, mutation.EventState);
        Assert.NotEmpty(topology.UnresolvedStrain);
    }

    [Fact]
    public void Nexus_ProjectsContradictoryField_FromObstructedSnapshot()
    {
        var snapshot = new SliExecutionSnapshot(
            SliSnapshotRetentionPosture.DebugOnly,
            true,
            "trace-obstructed",
            "defer",
            "branch-obstructed",
            "blocked",
            ["trace-1"],
            [],
            [],
            [
                new LocalityShardSnapshot("acting", SliLocalityShardKind.Acting, "cryptic://acting", "exec", "root-a", "boundary-a", SliLocalityShardLifecycleState.Obstructed)
            ],
            [
                new LocalityRelationEventSnapshot("rel-1", "acting", "acting", SliLocalityRelationKind.IngestsFrom, SliLocalityTelemetryCarryPolicy.TraceWitnessAndResidue, false, SliLocalityRelationOutcomeKind.Obstructed, "boundary-blocked", "cycle-1")
            ],
            [
                new LocalityObstructionSnapshot("obs-1", "acting", "acting", SliLocalityRelationKind.IngestsFrom, "boundary-blocked", false, false, true, "cycle-1")
            ],
            [
                new ActualizationWebbingEventSnapshot(SliActualizationStageKind.Cleave, "boundary blocked", "acting", "cryptic://acting", "cycle-1")
            ],
            new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.SelfImplicating, SliActualizationContradictionClass.SelfValidationConflict, SliActualizationValidationRoute.SoulFrameContinuityMediation, SliActualizationDisposition.Obstructed, true, false, false, SliLocalityRelationOutcomeKind.Obstructed, "boundary-blocked", ["contradiction:SelfValidationConflict"]),
            new ZedThetaCandidateSnapshot("candidate", "obj", "prime", "theta", "gamma", CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Weakened, CompassSelfTouchClass.BoundaryContact, CompassOeCoePosture.OeDominant, null, new RuntimeCeilingSnapshot(true)),
            new LiveRuntimeRunSnapshot("exec", true, "acting", SliLocalityRelationOutcomeKind.Obstructed, "boundary-blocked"));

        var nexus = CrypticWebNexusFactory.Create(snapshot);
        var topology = nexus.CaptureTopologySnapshot();
        var relaxation = nexus.CaptureRelaxationReceipt();
        var telemetry = nexus.CaptureTelemetryFrame();
        var mutation = Assert.Single(nexus.CaptureMutationEvents());

        Assert.Equal(WebFieldState.Contradictory, topology.FieldState);
        Assert.Equal("violated", relaxation.BoundaryIntegrityState);
        Assert.Equal(RelaxationState.Contradictory, relaxation.RelaxationState);
        Assert.Equal(NexusReadinessState.Contradictory, telemetry.ReadinessState);
        Assert.Equal(MutationEventState.Obstructed, mutation.EventState);
        Assert.Contains(topology.UnresolvedStrain, item => item.Contains("obstruction:", StringComparison.Ordinal));
    }

    [Fact]
    public void NexusPortal_AggregatesReadySurface_WithoutImplyingGrant()
    {
        var snapshot = new SliExecutionSnapshot(
            SliSnapshotRetentionPosture.DebugOnly,
            true,
            "trace-portal-ready",
            "commit",
            "branch-ready",
            "[]",
            ["trace-1"],
            [],
            [],
            [
                new LocalityShardSnapshot("acting", SliLocalityShardKind.Acting, "cryptic://acting", "exec", "root-a", "boundary-a", SliLocalityShardLifecycleState.Joined)
            ],
            [],
            [],
            [
                new ActualizationWebbingEventSnapshot(SliActualizationStageKind.Witness, "portal witness formed", "acting", "cryptic://acting", "cycle-1")
            ],
            new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.NonSelfOperational, SliActualizationContradictionClass.None, SliActualizationValidationRoute.NoneRequired, SliActualizationDisposition.Actualized, false, false, false, SliLocalityRelationOutcomeKind.Joined, "joined", []),
            new ZedThetaCandidateSnapshot("candidate", "obj", "prime", "theta", "gamma", CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Held, CompassSelfTouchClass.NoTouch, CompassOeCoePosture.OeDominant, null, new RuntimeCeilingSnapshot(true)),
            new LiveRuntimeRunSnapshot("exec", true, "acting", SliLocalityRelationOutcomeKind.Joined, "joined"));

        var portal = CrypticWebNexusFactory.CreatePortal(snapshot);
        var surface = portal.CapturePortalSurface();

        Assert.Equal("portal:trace-portal-ready", surface.PortalId);
        Assert.Equal(WebFieldState.ReadyForReentry, surface.Topology.FieldState);
        Assert.Equal(NexusGateLegibilityState.ReadyForBoundedEngagement, surface.GateLegibility.GateState);
        Assert.False(surface.GateLegibility.GrantImplied);
        Assert.Equal("host-law-and-nexus-adjudication", surface.GateLegibility.AccessGrantAuthority);
    }

    [Fact]
    public void NexusPortal_ProjectsContradictoryGate_WhenFieldIsBlocked()
    {
        var snapshot = new SliExecutionSnapshot(
            SliSnapshotRetentionPosture.DebugOnly,
            true,
            "trace-portal-blocked",
            "defer",
            "branch-blocked",
            "blocked",
            ["trace-1"],
            [],
            [],
            [
                new LocalityShardSnapshot("acting", SliLocalityShardKind.Acting, "cryptic://acting", "exec", "root-a", "boundary-a", SliLocalityShardLifecycleState.Obstructed)
            ],
            [],
            [
                new LocalityObstructionSnapshot("obs-1", "acting", "acting", SliLocalityRelationKind.IngestsFrom, "boundary-blocked", false, false, true, "cycle-1")
            ],
            [
                new ActualizationWebbingEventSnapshot(SliActualizationStageKind.Cleave, "portal boundary blocked", "acting", "cryptic://acting", "cycle-1")
            ],
            new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.SelfImplicating, SliActualizationContradictionClass.SelfValidationConflict, SliActualizationValidationRoute.SoulFrameContinuityMediation, SliActualizationDisposition.Obstructed, true, false, false, SliLocalityRelationOutcomeKind.Obstructed, "boundary-blocked", ["contradiction:SelfValidationConflict"]),
            new ZedThetaCandidateSnapshot("candidate", "obj", "prime", "theta", "gamma", CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Weakened, CompassSelfTouchClass.BoundaryContact, CompassOeCoePosture.OeDominant, null, new RuntimeCeilingSnapshot(true)),
            new LiveRuntimeRunSnapshot("exec", true, "acting", SliLocalityRelationOutcomeKind.Obstructed, "boundary-blocked"));

        var portal = CrypticWebNexusFactory.CreatePortal(snapshot);
        var surface = portal.CapturePortalSurface();

        Assert.Equal(WebFieldState.Contradictory, surface.Topology.FieldState);
        Assert.Equal(NexusGateLegibilityState.Contradictory, surface.GateLegibility.GateState);
        Assert.Contains("bounded-engagement", surface.GateLegibility.DeniedSurfaces, StringComparer.OrdinalIgnoreCase);
    }
}
