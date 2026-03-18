using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using Oan.Common;
using SLI.Engine.Cognition;
using SoulFrame.Host;
using System.Text.Json;

namespace Oan.Sli.Tests;

public sealed class SliCognitionEngineTests
{
    [Fact]
    public async Task LispRuntime_LoadsAndExecutes_WithCompassMetrics()
    {
        var resolver = new EngramResolverService();
        var engine = new SliCognitionEngine(resolver, semanticDevice: new AcceptedCompassSemanticDevice());
        await engine.InitializeAsync();

        var request = new CognitionRequest
        {
            Context = new CognitionContext
            {
                CMEId = "cme-test",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "identity-continuity",
                RelevantEngrams = [],
                SelfStateHint = new CognitionSelfStateHint
                {
                    ClaimCount = 0,
                    HasDeferredOrContradictedClaim = false,
                    HasHotClaim = false,
                    ValidationConceptCount = 2
                },
                CleaverHint = new CognitionCleaverHint
                {
                    KnownRatio = 0.72,
                    UnknownRatio = 0.18
                }
            },
            Prompt = "execute symbolic cognition"
        };

        var result = await engine.ExecuteAsync(request);

        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
        Assert.False(string.IsNullOrWhiteSpace(result.CleaveResidue));
        Assert.InRange(result.Confidence, 0.1, 0.99);

        Assert.NotNull(engine.LastTraceEvent);
        Assert.NotNull(engine.LastDecisionSpline);
        Assert.NotNull(engine.LastTraceEvent!.CompassState);
        Assert.True(engine.LastTraceEvent.CompassState.SymbolicDepth > 0);
        Assert.True(engine.LastTraceEvent.SymbolicTrace.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(engine.LastTraceEvent.TraceId));
        Assert.Equal(result.Decision, engine.LastTraceEvent.DecisionBranch);
        Assert.Equal(result.CleaveResidue, engine.LastTraceEvent.CleaveResidue);

        var localityIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "locality-bind(");
        var perspectiveIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "perspective-configure(");
        var participationIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "participation-configure(");
        var primeIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "prime-reflect(");
        var zedIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "zed-listen(");
        var deltaIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "delta-differentiate(");
        var sigmaIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "sigma-cleave(");
        var psiIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "psi-modulate(");
        var omegaIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "omega-converge(");
        var thetaIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "theta-seal(");
        var compassWorkIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "compass-work(");
        var gammaIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "gamma-yield(");
        var compassIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "compass-update(");

        Assert.True(localityIndex >= 0);
        Assert.True(localityIndex < perspectiveIndex);
        Assert.True(perspectiveIndex < participationIndex);
        Assert.True(participationIndex < primeIndex);
        Assert.True(primeIndex < zedIndex);
        Assert.True(zedIndex < deltaIndex);
        Assert.True(deltaIndex < sigmaIndex);
        Assert.True(sigmaIndex < psiIndex);
        Assert.True(psiIndex < omegaIndex);
        Assert.True(omegaIndex < thetaIndex);
        Assert.True(thetaIndex < compassWorkIndex);
        Assert.True(compassWorkIndex < gammaIndex);
        Assert.True(gammaIndex < compassIndex);
        Assert.Contains("GoldenCode(", result.Reasoning, StringComparison.Ordinal);
        Assert.Equal(CompassDoctrineBasin.IdentityContinuity, result.GoldenCodeCompass.ActiveBasin);
        Assert.Equal(CompassDoctrineBasin.IdentityContinuity, result.GoldenCodeCompass.CompetingBasin);
        Assert.Equal(CompassAnchorState.Held, result.GoldenCodeCompass.AnchorState);
        Assert.Equal(CompassSelfTouchClass.ValidationTouch, result.GoldenCodeCompass.SelfTouchClass);
        Assert.Equal(CompassOeCoePosture.OeDominant, result.GoldenCodeCompass.OeCoePosture);
        Assert.Equal("task-objective", result.ZedThetaCandidate.PrimeState);
        Assert.Equal("theta-ready", result.ZedThetaCandidate.ThetaState);
        Assert.Equal("gamma-ready", result.ZedThetaCandidate.GammaState);
        Assert.Equal(SliThinkingTier.Master, result.ZedThetaCandidate.PacketDirective.ThinkingTier);
        Assert.Equal(SliPacketClass.Commitment, result.ZedThetaCandidate.PacketDirective.PacketClass);
        Assert.Equal(SliEngramOperation.Write, result.ZedThetaCandidate.PacketDirective.EngramOperation);
        Assert.Equal(SliUpdateLocus.Kernel, result.ZedThetaCandidate.PacketDirective.UpdateLocus);
        Assert.Equal(SliAuthorityClass.CandidateBearing, result.ZedThetaCandidate.PacketDirective.AuthorityClass);
        Assert.True(result.ZedThetaCandidate.IdentityKernelBoundary.KernelBound);
        Assert.Equal(SliUpdateLocus.Kernel, result.ZedThetaCandidate.IdentityKernelBoundary.CandidateLocus);
        Assert.Equal("sli-packet-valid", result.ZedThetaCandidate.Validity.ReasonCode);
        Assert.Equal(result.GoldenCodeCompass.ActiveBasin, result.ZedThetaCandidate.ActiveBasin);
        Assert.Equal(result.GoldenCodeCompass.CompetingBasin, result.ZedThetaCandidate.CompetingBasin);
        Assert.Equal(result.GoldenCodeCompass.AnchorState, result.ZedThetaCandidate.AnchorState);
        Assert.Equal(result.GoldenCodeCompass.SelfTouchClass, result.ZedThetaCandidate.SelfTouchClass);
        Assert.Equal(result.GoldenCodeCompass.OeCoePosture, result.ZedThetaCandidate.OeCoePosture);
        Assert.NotNull(result.ZedThetaCandidate.BridgeReview);
        Assert.Equal(SliBridgeOutcomeKind.Ok, result.ZedThetaCandidate.BridgeReview!.OutcomeKind);
        Assert.NotNull(result.ZedThetaCandidate.RuntimeUseCeiling);
        Assert.True(result.ZedThetaCandidate.RuntimeUseCeiling!.CandidateOnly);
        var compatibilityProjection = GoldenCodeCompassProjection.FromCandidateReceipt(result.ZedThetaCandidate);
        Assert.Equal(result.GoldenCodeCompass.ActiveBasin, compatibilityProjection.ActiveBasin);
        Assert.Equal(result.GoldenCodeCompass.CompetingBasin, compatibilityProjection.CompetingBasin);
        Assert.Equal(result.GoldenCodeCompass.AnchorState, compatibilityProjection.AnchorState);
        Assert.Equal(result.GoldenCodeCompass.SelfTouchClass, compatibilityProjection.SelfTouchClass);
        Assert.Equal(result.GoldenCodeCompass.OeCoePosture, compatibilityProjection.OeCoePosture);
        Assert.DoesNotContain(engine.LastTraceEvent.SymbolicTrace, line => line.Contains("surface-", StringComparison.Ordinal));
        Assert.DoesNotContain(engine.LastTraceEvent.SymbolicTrace, line => line.Contains("packet-", StringComparison.Ordinal));
    }

    [Fact]
    public void CanonicalProgram_UsesCompositeFormsBeforeCompassUpdate()
    {
        var engine = new SliCognitionEngine(new EngramResolverService());
        var program = engine.BuildProgram("identity-continuity", null);

        var localityIndex = IndexOfContaining(program, "(locality-bootstrap");
        var perspectiveIndex = IndexOfContaining(program, "(perspective-bounded-observer");
        var participationIndex = IndexOfContaining(program, "(participation-bounded-cme");
        var goldenCodeIndex = IndexOfContaining(program, "(golden-code-bloom");
        var compassIndex = IndexOfContaining(program, "(compass-update");

        Assert.True(localityIndex >= 0);
        Assert.True(localityIndex < perspectiveIndex);
        Assert.True(perspectiveIndex < participationIndex);
        Assert.True(participationIndex < goldenCodeIndex);
        Assert.True(goldenCodeIndex < compassIndex);
        Assert.DoesNotContain(program, line => line.Contains("rehearsal-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("witness-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("morphism-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("transport-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("surface-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("admissible-surface", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("packet-", StringComparison.Ordinal));
        Assert.DoesNotContain(program, line => line.Contains("accountability-packet", StringComparison.Ordinal));
        CanonicalCognitionCycle.ValidateProgramOrder(program);
    }

    [Fact]
    public void CanonicalProgramOrder_FailsWhenCompassPrecedesHigherOrderLocality()
    {
        var invalidProgram = new[]
        {
            "(decision-evaluate predicate-set)",
            "(locality-bootstrap context cme-self task-objective identity-continuity)",
            "(perspective-bounded-observer locality-state task-objective identity-continuity)",
            "(participation-bounded-cme locality-state)",
            "(compass-update context reasoning-state)",
            "(decision-branch cognition-state)",
            "(cleave branch-set)",
            "(commit decision)"
        };

        Assert.Throws<InvalidOperationException>(() => CanonicalCognitionCycle.ValidateProgramOrder(invalidProgram));
    }

    [Fact]
    public void ZedThetaCandidateReceipt_Serializes_Deterministically()
    {
        var receipt = new ZedThetaCandidateReceipt(
            CandidateHandle: "zed-theta:test",
            Objective: "identity-continuity",
            PrimeState: "prime-ready",
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

        var options = new JsonSerializerOptions();
        var first = JsonSerializer.Serialize(receipt, options);
        var second = JsonSerializer.Serialize(receipt, options);
        var roundTrip = JsonSerializer.Deserialize<ZedThetaCandidateReceipt>(first, options);

        Assert.Equal(first, second);
        Assert.NotNull(roundTrip);
        Assert.Equal(receipt, roundTrip);
        Assert.Contains("\"PacketClass\":\"Commitment\"", first, StringComparison.Ordinal);
        Assert.Contains("\"AuthorizationState\"", JsonSerializer.Serialize(
            new SliTheaterAuthorizationReceipt(
                CandidateHandle: "zed-theta:test",
                SourceTheater: "prime",
                RequestedTheater: "prime",
                AuthorityClass: SliAuthorityClass.CandidateBearing,
                AuthorizationState: SliTheaterAuthorizationState.Withheld,
                ReasonCode: "sli-runtime-candidate-only",
                WitnessedBy: "test",
                TimestampUtc: DateTimeOffset.UnixEpoch), options), StringComparison.Ordinal);
    }

    private static int IndexOfContaining(IReadOnlyList<string> entries, string fragment)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            if (entries[index].Contains(fragment, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private sealed class AcceptedCompassSemanticDevice : ISoulFrameSemanticDevice
    {
        public Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("infer-ok", null));

        public Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        {
            var activeBasin = request.CompassAdvisory?.TargetActiveBasin ?? CompassDoctrineBasin.IdentityContinuity;
            var competingBasin = request.CompassAdvisory?.ExcludedCompetingBasin ?? CompassDoctrineBasin.Unknown;
            var anchorState = activeBasin == CompassDoctrineBasin.IdentityContinuity
                ? CompassAnchorState.Held
                : CompassAnchorState.Weakened;

            return Task.FromResult(CreateResponse(
                "classify-ok",
                new SoulFrameCompassAdvisoryResponse
                {
                    SuggestedActiveBasin = activeBasin,
                    SuggestedCompetingBasin = competingBasin,
                    SuggestedAnchorState = anchorState,
                    SuggestedSelfTouchClass = CompassSelfTouchClass.ValidationTouch,
                    Confidence = 0.91,
                    Justification = "accepted-test-advisory"
                }));
        }

        public Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("semantic-expand-ok", null));

        public Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreateResponse("embedding-ok", null));

        private static SoulFrameInferenceResponse CreateResponse(
            string decision,
            SoulFrameCompassAdvisoryResponse? advisory)
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
                CompassAdvisory = advisory
            };
        }
    }
}
