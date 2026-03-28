using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Models;
using Oan.Common;
using SLI.Engine.Cognition;
using SLI.Engine.Models;
using SLI.Engine.Runtime;
using SoulFrame.Host;
using Xunit;

namespace Oan.Sli.Tests;

public sealed class SliExecutionSnapshotTests
{
    [Fact]
    public void Snapshot_RoundTrips_WithDeterministicJson()
    {
        var frame = new ContextFrame
        {
            CMEId = "cme-test",
            ContextId = Guid.NewGuid(),
            TaskObjective = "test-objective",
            SoulFrameId = Guid.NewGuid(),
            Engrams = new List<SLI.Engine.Models.EngramReference>()
        };
        // Fake context and result
        var context = new SliExecutionContext(frame, null!, null!);
        context.AddTrace("trace-1");

        var result = new LispExecutionResult
        {
            TraceId = "trace-test",
            Decision = "defer",
            DecisionBranch = "branch-a",
            CleaveResidue = "none",
            SymbolicTrace = new List<string>(),
            SymbolicTraceHash = "hash",
            CompassState = new CognitiveCompassState
            {
                IdForce = 0,
                SuperegoConstraint = 0,
                EgoStability = 0,
                ValueElevation = ValueElevation.Neutral,
                SymbolicDepth = 0,
                BranchingFactor = 0,
                DecisionEntropy = 0,
                Timestamp = DateTime.UtcNow,
                ContextExpansionRate = 0,
                PredicateAlignment = 0,
                CleaveRatio = 0,
                GovernanceFlags = 0,
                CommitConfidence = 0
            },
            GoldenCodeCompass = new GoldenCodeCompassProjection
            {
                ActiveBasin = CompassDoctrineBasin.Unknown,
                CompetingBasin = CompassDoctrineBasin.Unknown,
                AnchorState = CompassAnchorState.Unknown,
                SelfTouchClass = CompassSelfTouchClass.NoTouch,
                OeCoePosture = CompassOeCoePosture.Unresolved
            },
            ZedThetaCandidate = new ZedThetaCandidateReceipt(
                "candidate", "obj", "prime", "theta", "gamma",
                new SliPacketDirective(SliThinkingTier.Master, SliPacketClass.Observation, SliEngramOperation.NoOp, SliUpdateLocus.Sheaf, SliAuthorityClass.CandidateBearing),
                new IdentityKernelBoundaryReceipt("cme", "ker", "an", false, SliUpdateLocus.Sheaf),
                new SliPacketValidityReceipt(true, true, true, true, "ok"),
                CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown, CompassAnchorState.Weakened, CompassSelfTouchClass.NoTouch, CompassOeCoePosture.OeDominant)
        };

        var snapshot = SliExecutionSnapshotFactory.CreateForCognition(context, result);
        var json = SliExecutionSnapshotFactory.Serialize(snapshot);

        Assert.NotEmpty(json);
        Assert.Contains("trace-test", json);

        var replay = SliExecutionReplay.ReplayFromSnapshot(json);
        Assert.NotNull(replay);
        Assert.Equal(snapshot.TraceId, replay.Snapshot.TraceId);
    }

    [Fact]
    public void ShardReduction_NotJoined_ReflectsInSnapshot()
    {
        var runSnap = new LiveRuntimeRunSnapshot("exec", true, "primary", SliLocalityRelationOutcomeKind.Deferred, "compass-shards-not-joined");
        var snapshot = new SliExecutionSnapshot(SliSnapshotRetentionPosture.DebugOnly, true, "trace", "decision", "branch", "cleave", new List<string>(), new List<string>(), new List<string>(), new List<LocalityShardSnapshot>(), new List<LocalityRelationEventSnapshot>(), new List<LocalityObstructionSnapshot>(), new List<ActualizationWebbingEventSnapshot>(), null, default!, runSnap);

        var json = SliExecutionSnapshotFactory.Serialize(snapshot);
        var replay = SliExecutionReplay.ReplayFromSnapshot(json);

        Assert.Equal(SliLocalityRelationOutcomeKind.Deferred, replay.Snapshot.LiveRuntimeRun!.ReductionOutcome);
    }

    [Fact]
    public void ObstructedActualization_ReflectsInSnapshot()
    {
        var actualization = new ActualizationPacketSnapshot("act", "exec", "obj", SliActualizationClaimClass.SelfImplicating, SliActualizationContradictionClass.SelfValidationConflict, SliActualizationValidationRoute.SoulFrameContinuityMediation, SliActualizationDisposition.Obstructed, true, false, false, SliLocalityRelationOutcomeKind.Obstructed, "up", new List<string>());
        var snapshot = new SliExecutionSnapshot(SliSnapshotRetentionPosture.DebugOnly, true, "trace", "decision", "branch", "cleave", new List<string>(), new List<string>(), new List<string>(), new List<LocalityShardSnapshot>(), new List<LocalityRelationEventSnapshot>(), new List<LocalityObstructionSnapshot>(), new List<ActualizationWebbingEventSnapshot>(), actualization, default!, null);

        var json = SliExecutionSnapshotFactory.Serialize(snapshot);
        var replay = SliExecutionReplay.ReplayFromSnapshot(json);

        Assert.Equal(SliActualizationDisposition.Obstructed, replay.Snapshot.ActualizationPacket!.Disposition);
    }
}
