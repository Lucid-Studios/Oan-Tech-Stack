using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using SLI.Engine.Models;
using SLI.Engine.Runtime;
using SoulFrame.Host;

namespace Oan.Sli.Tests;

public sealed class SliSemanticRequestAnchoringTests
{
    [Fact]
    public async Task LlmClassify_AnchorsBoundedLocalityDoctrineInRequestContext()
    {
        var device = new RecordingSemanticDevice();
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "cme-locality",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "maintain bounded locality continuity under masked locality witness",
                Engrams = []
            },
            new StubEngramResolver(),
            device);
        var table = new SliSymbolTable();

        var resolved = table.TryResolve("llm_classify", out var handler);

        Assert.True(resolved);

        await handler!(
            SExpression.ListNode(
            [
                SExpression.AtomNode("llm_classify"),
                SExpression.AtomNode("\"task-objective\"")
            ]),
            context,
            CancellationToken.None);

        Assert.NotNull(device.LastRequest);
        Assert.Equal("bounded-locality continuity", device.LastRequest!.OpalConstraints.Domain);
        Assert.Contains("ACTIVE_DOCTRINE_DOMAIN: bounded-locality continuity", device.LastRequest.Context, StringComparison.Ordinal);
        Assert.Contains("EXCLUDED_NEARBY_DOMAIN: fluid continuity law", device.LastRequest.Context, StringComparison.Ordinal);
        Assert.Contains("OBJECTIVE_HINT: maintain bounded locality continuity under masked locality witness", device.LastRequest.Context, StringComparison.Ordinal);
        Assert.Contains("INPUT: task-objective", device.LastRequest.Context, StringComparison.Ordinal);
    }

    private sealed class RecordingSemanticDevice : ISoulFrameSemanticDevice
    {
        public SoulFrameInferenceRequest? LastRequest { get; private set; }

        public Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Fallback(request));

        public Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(Fallback(request));
        }

        public Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Fallback(request));

        public Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Fallback(request));

        private static SoulFrameInferenceResponse Fallback(SoulFrameInferenceRequest request)
        {
            return new SoulFrameInferenceResponse
            {
                Accepted = true,
                Decision = $"{request.Task}-ok",
                Payload = request.Context,
                Confidence = 0.8,
                Governance = new SoulFrameGovernedEmissionEnvelope
                {
                    State = SoulFrameGovernedEmissionState.Query,
                    Trace = "test-response",
                    Content = request.Context
                }
            };
        }
    }

    private sealed class StubEngramResolver : IEngramResolver
    {
        public Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(Empty());

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
            => Task.FromResult(Empty());

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => Task.FromResult(Empty());

        private static EngramQueryResult Empty() =>
            new()
            {
                Source = "stub",
                Summaries = []
            };
    }
}
