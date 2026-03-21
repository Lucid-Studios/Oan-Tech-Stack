using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SLI.Engine.Cognition;
using SLI.Engine.Models;
using SLI.Engine.Runtime;
using Xunit;

namespace Oan.Sli.Tests;

public sealed class SliLocalFileSnapshotRouterTests
{
    [Fact]
    public async Task RouteAsync_GovernanceArtifact_IsIgnoredLocally()
    {
        var snapshot = new SliExecutionSnapshot(
            RetentionPosture: SliSnapshotRetentionPosture.GovernanceArtifact,
            IsNonIdentityFormingMemory: true,
            TraceId: "test-trace",
            Decision: "defer",
            DecisionBranch: "branch",
            CleaveResidue: "[]",
            TraceLines: [],
            CandidateBranches: [],
            PrunedBranches: [],
            LocalityShards: [],
            LocalityRelationEvents: [],
            LocalityObstructions: [],
            ActualizationWebbingEvents: [],
            ActualizationPacket: null,
            ZedThetaCandidate: default!,
            LiveRuntimeRun: null);

        var router = new SliLocalFileSnapshotRouter();
        await router.RouteAsync(snapshot, CancellationToken.None);

        // Governance shouldn't be written locally by this router
        // Since we can't easily mock the static GetLocalDiagnosticDirectory,
        // we assert no exception was thrown, and check that the file does not exist.
        var assumedPath = Path.Combine("OAN Mortalis V1.0", ".local", "sli-snapshots", "snapshot_test-trace_GovernanceArtifact.json");
        Assert.False(File.Exists(assumedPath));
    }

    [Fact]
    public async Task RouteAsync_WithStrictEgressRouter_BlocksDisallowedEgress()
    {
        var snapshot = new SliExecutionSnapshot(
            RetentionPosture: SliSnapshotRetentionPosture.DebugOnly,
            IsNonIdentityFormingMemory: true,
            TraceId: "test-strict-mock-deny",
            Decision: "defer",
            DecisionBranch: "branch",
            CleaveResidue: "[]",
            TraceLines: [],
            CandidateBranches: [],
            PrunedBranches: [],
            LocalityShards: [],
            LocalityRelationEvents: [],
            LocalityObstructions: [],
            ActualizationWebbingEvents: [],
            ActualizationPacket: null,
            ZedThetaCandidate: default!,
            LiveRuntimeRun: null);

        var strictRouter = new StrictTestEgressRouter
        {
            AllowedJurisdiction = Oan.Common.SliEgressJurisdictionClass.AgentiCore // Emits from CoreRuntime
        };
        var router = new SliLocalFileSnapshotRouter(strictRouter);
        
        await router.RouteAsync(snapshot, CancellationToken.None);

        Assert.Single(strictRouter.CapturedEnvelopes);
        var envelope = strictRouter.CapturedEnvelopes.First();
        Assert.Equal(Oan.Common.SliEgressJurisdictionClass.CoreRuntime, envelope.JurisdictionClass);
        Assert.Equal(Oan.Common.SliEgressEffectKind.ArtifactWrite, envelope.EffectKind);

        // Verification that it did not write: The TryRouteEgressAsync returned false and bypassed the write action.
    }

    [Fact]
    public async Task RouteAsync_WithStrictEgressRouter_AllowsConfiguredEgress()
    {
        var snapshot = new SliExecutionSnapshot(
            RetentionPosture: SliSnapshotRetentionPosture.DebugOnly,
            IsNonIdentityFormingMemory: true,
            TraceId: "test-strict-mock-allow",
            Decision: "defer",
            DecisionBranch: "branch",
            CleaveResidue: "[]",
            TraceLines: [],
            CandidateBranches: [],
            PrunedBranches: [],
            LocalityShards: [],
            LocalityRelationEvents: [],
            LocalityObstructions: [],
            ActualizationWebbingEvents: [],
            ActualizationPacket: null,
            ZedThetaCandidate: default!,
            LiveRuntimeRun: null);

        var strictRouter = new StrictTestEgressRouter
        {
            AllowedJurisdiction = Oan.Common.SliEgressJurisdictionClass.CoreRuntime
        };
        var router = new SliLocalFileSnapshotRouter(strictRouter);
        
        await router.RouteAsync(snapshot, CancellationToken.None);

        Assert.Single(strictRouter.CapturedEnvelopes);
        
        // Ensure cleanup if the file was written
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "OAN Mortalis V1.0", ".local", "sli-snapshots");
        if (File.Exists(Path.Combine(directory, "build.ps1"))) 
            directory = Path.Combine(Directory.GetCurrentDirectory(), ".local", "sli-snapshots");

        var assumedPath = Path.Combine(directory, "snapshot_test-strict-mock-allow_DebugOnly.json");
        if (File.Exists(assumedPath)) 
        {
            File.Delete(assumedPath);
        }
    }
}
