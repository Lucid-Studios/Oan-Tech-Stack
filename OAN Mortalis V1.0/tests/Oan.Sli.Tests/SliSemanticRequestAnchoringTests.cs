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
    public async Task LlmClassify_AnchorsBoundedLocalityDoctrineWithoutPrecomposingContext()
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
        Assert.Equal("task-objective", device.LastRequest.Context);
        Assert.NotNull(device.LastRequest.GovernanceProtocol);
        Assert.True(device.LastRequest.GovernanceProtocol!.RequireStateEnvelope);
        Assert.True(device.LastRequest.GovernanceProtocol.RequireTerminalState);
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
