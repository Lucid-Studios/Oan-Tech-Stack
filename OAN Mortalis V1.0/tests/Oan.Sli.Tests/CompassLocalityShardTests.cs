using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using Oan.Common;
using SLI.Engine;
using SLI.Engine.Cognition;
using SLI.Engine.Models;
using SLI.Engine.Runtime;
using SoulFrame.Host;

namespace Oan.Sli.Tests;

public sealed class CompassLocalityShardTests
{
    [Fact]
    public async Task LispBridge_CanonicalProgram_ProducesActualizationPacket_EndToEnd()
    {
        var bridge = new LispBridge(new EngramResolverService(), new AcceptedSemanticDevice());
        await bridge.InitializeAsync();

        var engine = new SliCognitionEngine(new EngramResolverService());
        var program = engine.BuildProgram("identity-continuity", null);
        var result = await bridge.ExecuteProgramAsync(program, CreateContextFrame());

        Assert.NotNull(result.ActualizationPacket);
        Assert.Equal(SliActualizationClaimClass.SelfImplicating, result.ActualizationPacket!.ClaimClass);
        Assert.Equal(SliActualizationDisposition.CandidateBearing, result.ActualizationPacket.Disposition);
        Assert.True(result.ActualizationPacket.CandidateEngramBearing);
        Assert.Contains(result.ActualizationPacket.WebbingEvents, evt => evt.Stage == SliActualizationStageKind.Bloom);
        Assert.Contains(result.ActualizationPacket.WebbingEvents, evt => evt.Stage == SliActualizationStageKind.Commit);
        Assert.NotNull(result.LiveRuntimeRun);
        Assert.Equal(SliLocalityRelationOutcomeKind.Joined, result.LiveRuntimeRun!.ReductionOutcome);
    }

    [Fact]
    public async Task PilotDetection_RecognizesCanonicalCompassOnly()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();

        var engine = new SliCognitionEngine(new EngramResolverService());
        var canonicalProgram = bridge.LowerProgram(engine.BuildProgram("identity-continuity", null));
        var higherOrderProgram = bridge.LowerProgram(
        [
            "(locality-bootstrap context cme-self task-objective identity-continuity)",
            "(perspective-bounded-observer locality-state task-objective identity-continuity)",
            "(participation-bounded-cme locality-state)",
            "(decision-branch cognition-state)"
        ]);

        Assert.True(SliCompassLocalityShards.IsPilotEligible(canonicalProgram));
        Assert.False(SliCompassLocalityShards.IsPilotEligible(higherOrderProgram));
    }

    [Fact]
    public void EnablingShardMode_CreatesCompassTopology()
    {
        var context = CreateContext();

        context.EnableCompassShardMode();

        Assert.True(context.ShardModeEnabled);
        Assert.Equal(3, context.LocalityShards.Count);
        Assert.Equal(4, context.LocalityRelationEvents.Count(evt => evt.RelationKind == SliLocalityRelationKind.AdjacentTo));
        Assert.DoesNotContain(
            context.LocalityRelationEvents,
            evt => evt.RelationKind == SliLocalityRelationKind.AdjacentTo &&
                   string.Equals(evt.SourceShardId, SliCompassLocalityShards.ActingShardId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(evt.TargetShardId, SliCompassLocalityShards.AdjacentIngestionShardId, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void WitnessShard_CannotSeeActingTelemetryBeforeImport()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();
        context.EnterShard(SliCompassLocalityShards.ActingShardId);
        context.ExportShardSymbol(SliCompassLocalityShards.GoldenCodeBloomTelemetryKey, "omega-ready");

        Assert.False(context.TryGetShardSymbol(
            SliCompassLocalityShards.WitnessingShardId,
            SliCompassLocalityShards.GoldenCodeBloomTelemetryKey,
            out _));
    }

    [Fact]
    public void AdjacentShard_CannotImportActingTelemetryDirectly()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();
        context.EnterShard(SliCompassLocalityShards.ActingShardId);
        context.ExportShardSymbol(SliCompassLocalityShards.GoldenCodeBloomTelemetryKey, "omega-ready");

        var imported = context.TryImportShardSymbol(
            SliCompassLocalityShards.ActingShardId,
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliCompassLocalityShards.GoldenCodeBloomTelemetryKey,
            SliCompassLocalityShards.GoldenCodeBloomTelemetryKey,
            out _);

        Assert.False(imported);
    }

    [Fact]
    public void MissingActingExports_ObstructsWitnessRelation()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();

        var relation = SliLocalityRelationEvaluator.EvaluateWitnessOf(context, "compass-work-test");

        Assert.Equal(SliLocalityRelationOutcomeKind.Obstructed, relation.Outcome);
        Assert.Single(context.LocalityObstructions);
        Assert.Equal(
            SliLocalityShardLifecycleState.Obstructed,
            context.LocalityShards.Single(shard => shard.ShardId == SliCompassLocalityShards.WitnessingShardId).LifecycleState);
    }

    [Fact]
    public void InvalidRelationPair_RefusesExplicitly()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();

        var relation = SliLocalityRelationEvaluator.EvaluateRelation(
            context,
            SliCompassLocalityShards.AdjacentIngestionShardId,
            SliCompassLocalityShards.ActingShardId,
            SliLocalityRelationKind.WitnessOf,
            SliLocalityTelemetryCarryPolicy.TraceAndWitness,
            "invalid-pair");

        Assert.Equal(SliLocalityRelationOutcomeKind.Refused, relation.Outcome);
        Assert.Single(context.LocalityObstructions);
        Assert.Equal("invalid-relation-pair", context.LocalityObstructions[0].ViolatedCondition);
    }

    [Fact]
    public void FinalizeWithoutCompassUpdate_RecordsDeferredIngestion()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();
        ExportActingTelemetry(context);

        var witness = SliLocalityRelationEvaluator.EvaluateWitnessOf(context, "compass-work-test");
        context.FinalizeCompassShardRun();

        Assert.Equal(SliLocalityRelationOutcomeKind.Joined, witness.Outcome);
        Assert.Contains(
            context.LocalityRelationEvents,
            evt => evt.RelationKind == SliLocalityRelationKind.IngestsFrom &&
                   evt.Outcome == SliLocalityRelationOutcomeKind.Deferred &&
                   evt.ReasonCode.Contains("downstream-phase-not-reached", StringComparison.Ordinal));
    }

    [Fact]
    public void FinalizeAfterWitnessObstruction_RecordsDeferredWithUpstreamReason()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();

        SliLocalityRelationEvaluator.EvaluateWitnessOf(context, "compass-work-test");
        context.FinalizeCompassShardRun();

        Assert.Contains(
            context.LocalityRelationEvents,
            evt => evt.RelationKind == SliLocalityRelationKind.IngestsFrom &&
                   evt.Outcome == SliLocalityRelationOutcomeKind.Deferred &&
                   evt.ReasonCode.Contains("upstream-obstructed", StringComparison.Ordinal));
    }

    [Fact]
    public void FinalizeAfterWitnessRefusal_RecordsDeferredWithUpstreamReason()
    {
        var context = CreateContext();
        context.EnableCompassShardMode();
        context.TryGetShard(SliCompassLocalityShards.WitnessingShardId, out var witnessShard);
        witnessShard.SymbolBoundaryRef = "invalid-boundary";

        SliLocalityRelationEvaluator.EvaluateWitnessOf(context, "compass-work-test");
        context.FinalizeCompassShardRun();

        Assert.Contains(
            context.LocalityRelationEvents,
            evt => evt.RelationKind == SliLocalityRelationKind.IngestsFrom &&
                   evt.Outcome == SliLocalityRelationOutcomeKind.Deferred &&
                   evt.ReasonCode.Contains("upstream-refused", StringComparison.Ordinal));
    }

    [Fact]
    public void ShardReductionDescription_StaysHonestWhenNotJoined()
    {
        var candidate = CreateCandidateReceipt();
        var actingPacket = new SliLiveEngramRuntimePacket
        {
            EngramHandle = "live-engram:test:acting",
            EngramKind = SliLiveEngramKind.GovernanceCandidateEngram,
            RuntimeState = SliLiveEngramRuntimeState.ReturnCandidate,
            ShardId = SliCompassLocalityShards.ActingShardId,
            ShardKind = SliLocalityShardKind.Acting,
            ParentExecutionId = "execution:test",
            RootAnchor = "root-anchor:test",
            SymbolBoundaryRef = SliCompassLocalityShards.ActingBoundaryRef,
            LocalityHandle = "locality:root-anchor:test:acting",
            SourceHandle = candidate.CandidateHandle,
            InvariantSet = ["active-basin:IdentityContinuity"],
            ResidueSet = ["reduction:Obstructed:test-obstruction"],
            WitnessSet = ["bridge-outcome:Ok"],
            TraceSet = [],
            ReturnCandidateEligible = true,
            ReturnEligibilityReason = "candidate-bearing-bridge-ok"
        };

        var result = new LispExecutionResult
        {
            TraceId = "trace-test",
            Decision = "identity-continuity-path",
            DecisionBranch = "identity-continuity-path",
            CleaveResidue = "[]",
            SymbolicTrace = [],
            SymbolicTraceHash = "hash",
            CompassState = new CognitiveCompassState
            {
                IdForce = 0.5,
                SuperegoConstraint = 0.5,
                EgoStability = 0.5,
                ValueElevation = ValueElevation.Neutral,
                SymbolicDepth = 1,
                BranchingFactor = 1,
                DecisionEntropy = 0.2,
                Timestamp = DateTime.UtcNow,
                ContextExpansionRate = 0.1,
                PredicateAlignment = 0.1,
                CleaveRatio = 0.0,
                GovernanceFlags = 0,
                CommitConfidence = 0.7
            },
            GoldenCodeCompass = GoldenCodeCompassProjection.FromCandidateReceipt(candidate),
            ZedThetaCandidate = candidate,
            LiveRuntimePacket = actingPacket,
            LiveRuntimeRun = new SliLiveEngramRuntimeRun
            {
                ExecutionId = "execution:test",
                ShardModeEnabled = true,
                PrimaryShardId = SliCompassLocalityShards.ActingShardId,
                ShardPackets = [actingPacket],
                RelationEvents =
                [
                    new SliLocalityRelationEvent(
                        "relation:test",
                        SliCompassLocalityShards.WitnessingShardId,
                        SliCompassLocalityShards.ActingShardId,
                        SliLocalityRelationKind.WitnessOf,
                        SliLocalityTelemetryCarryPolicy.TraceAndWitness,
                        false,
                        SliLocalityRelationOutcomeKind.Obstructed,
                        "missing-source-telemetry:theta-seal",
                        "compass-work")
                ],
                Obstructions =
                [
                    new SliLocalityObstructionRecord(
                        "obstruction:test",
                        SliCompassLocalityShards.WitnessingShardId,
                        SliCompassLocalityShards.ActingShardId,
                        SliLocalityRelationKind.WitnessOf,
                        "missing-source-telemetry",
                        true,
                        false,
                        false,
                        "compass-work")
                ],
                ReductionOutcome = SliLocalityRelationOutcomeKind.Obstructed,
                ReductionReason = "missing-source-telemetry:theta-seal"
            }
        };

        var summary = SliCognitionEngine.DescribeShardReduction(result);

        Assert.Contains("reduction=Obstructed", summary, StringComparison.Ordinal);
        Assert.Contains("acting-shard-only", summary, StringComparison.Ordinal);
    }

    [Fact]
    public void OrdinarySerialPacket_RemainsShardFree()
    {
        var context = CreateContext();
        context.AddTrace("engram-query(identity-continuity)");
        context.CandidateBranches.Add("identity-continuity-path");
        context.FinalDecision = "identity-continuity-path";

        var packet = SliLiveEngramRuntimePacketFactory.CreateForCognition(
            context,
            traceId: "trace-test",
            CreateCandidateReceipt());

        Assert.False(context.ShardModeEnabled);
        Assert.Equal("serial", packet.ShardId);
        Assert.Equal(SliLocalityShardKind.Acting, packet.ShardKind);
    }

    private static SliExecutionContext CreateContext()
    {
        return new SliExecutionContext(CreateContextFrame(), new StubResolver());
    }

    private static ContextFrame CreateContextFrame()
    {
        return new ContextFrame
        {
            CMEId = "cme-test",
            SoulFrameId = Guid.Empty,
            ContextId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TaskObjective = "identity-continuity",
            Engrams = []
        };
    }

    private static void ExportActingTelemetry(SliExecutionContext context)
    {
        context.EnterShard(SliCompassLocalityShards.ActingShardId);
        context.ExportShardSymbol(SliCompassLocalityShards.GoldenCodeBloomTelemetryKey, "omega-ready");
        context.ExportShardSymbol(SliCompassLocalityShards.ThetaSealTelemetryKey, "theta-ready");
    }

    private static ZedThetaCandidateReceipt CreateCandidateReceipt()
    {
        return new ZedThetaCandidateReceipt(
            CandidateHandle: "zed-theta:test",
            Objective: "identity-continuity",
            PrimeState: "task-objective",
            ThetaState: "theta-ready",
            GammaState: "gamma-ready",
            PacketDirective: new SliPacketDirective(
                SliThinkingTier.Master,
                SliPacketClass.Commitment,
                SliEngramOperation.Write,
                SliUpdateLocus.Kernel,
                SliAuthorityClass.CandidateBearing),
            IdentityKernelBoundary: new IdentityKernelBoundaryReceipt(
                CmeIdentityHandle: "cme:test",
                IdentityKernelHandle: "kernel:test",
                ContinuityAnchorHandle: "anchor:test",
                KernelBound: true,
                CandidateLocus: SliUpdateLocus.Kernel),
            Validity: new SliPacketValidityReceipt(
                SyntaxOk: true,
                HexadOk: true,
                ScepOk: true,
                PolicyEligible: true,
                ReasonCode: "sli-packet-valid"),
            ActiveBasin: CompassDoctrineBasin.IdentityContinuity,
            CompetingBasin: CompassDoctrineBasin.Unknown,
            AnchorState: CompassAnchorState.Held,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            OeCoePosture: CompassOeCoePosture.OeDominant,
            BridgeReview: SliBridgeContracts.CreateReview(
                bridgeStage: "zed-theta-candidate",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: "sli-bridge://test",
                outcomeKind: SliBridgeOutcomeKind.Ok,
                thresholdClass: SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band"),
            RuntimeUseCeiling: SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling());
    }

    private sealed class StubResolver : IEngramResolver
    {
        public Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EngramQueryResult { Source = "stub", Summaries = [] });
        }

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EngramQueryResult { Source = "stub", Summaries = [] });
        }

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EngramQueryResult { Source = "stub", Summaries = [] });
        }
    }

    private sealed class AcceptedSemanticDevice : ISoulFrameSemanticDevice
    {
        public Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("infer-ok"));

        public Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("classify-ok"));

        public Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("semantic-expand-ok"));

        public Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("embedding-ok"));

        private static SoulFrameInferenceResponse CreateResponse(string decision)
        {
            return new SoulFrameInferenceResponse
            {
                Accepted = true,
                Decision = decision,
                Payload = "test",
                Confidence = 0.91,
                Governance = new SoulFrameGovernedEmissionEnvelope
                {
                    State = SoulFrameGovernedEmissionState.Complete,
                    Trace = "accepted-test-semantic-device",
                    Content = "test"
                },
                CompassAdvisory = new SoulFrameCompassAdvisoryResponse
                {
                    SuggestedActiveBasin = CompassDoctrineBasin.IdentityContinuity,
                    SuggestedCompetingBasin = CompassDoctrineBasin.Unknown,
                    SuggestedAnchorState = CompassAnchorState.Held,
                    SuggestedSelfTouchClass = CompassSelfTouchClass.ValidationTouch,
                    Confidence = 0.91,
                    Justification = "accepted-test-advisory"
                }
            };
        }
    }
}
