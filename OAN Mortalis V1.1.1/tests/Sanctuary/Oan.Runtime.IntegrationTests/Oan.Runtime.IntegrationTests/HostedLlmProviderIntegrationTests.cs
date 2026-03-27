using Oan.Common;
using Oan.HostedLlm;

namespace Oan.Runtime.IntegrationTests;

public sealed class HostedLlmProviderIntegrationTests
{
    [Fact]
    public void HostedSeedService_Uses_Provider_Response_When_Available()
    {
        var service = new GovernedHostedLlmSeedService(
            new StubHostedLlmProvider(
                new GovernedHostedLlmProviderResponse(
                    State: GovernedSeedHostedLlmEmissionState.Query,
                    Trace: "localhost-hosted-llm-query",
                    Payload: "bounded-query-ready")));

        var receipt = service.Evaluate(CreateRequest("Summarize what stands while remaining bounded."), CreateMemoryContext());

        Assert.Equal(GovernedSeedHostedLlmEmissionState.Query, receipt.ResponsePacket.State);
        Assert.Equal("localhost-hosted-llm-query", receipt.ResponsePacket.Trace);
        Assert.Equal("hosted-seed-query", receipt.ResponsePacket.Decision);
        Assert.True(receipt.ResponsePacket.Accepted);
        Assert.Equal(receipt.RequestPacket.PacketHandle, receipt.SeededTransitPacket.HostedLlmRequestPacketHandle);
        Assert.Equal(receipt.ResponsePacket.PacketHandle, receipt.SeededTransitPacket.HostedLlmResponsePacketHandle);
        Assert.True(receipt.SeededTransitPacket.HostedLlmAccepted);
    }

    [Fact]
    public void HostedSeedService_Falls_Back_To_Local_Governance_When_Provider_Is_Unavailable()
    {
        var service = new GovernedHostedLlmSeedService(new StubHostedLlmProvider(null));

        var receipt = service.Evaluate(
            CreateRequest("Need more information before you proceed with this underspecified request."),
            CreateMemoryContext());

        Assert.Equal(GovernedSeedHostedLlmEmissionState.NeedsMoreInformation, receipt.ResponsePacket.State);
        Assert.Equal("governed-needs-more-information", receipt.ResponsePacket.Trace);
        Assert.False(receipt.ResponsePacket.Accepted);
    }

    private static GovernedSeedEvaluationRequest CreateRequest(string input) =>
        new(
            AgentId: "agent-hosted-llm",
            TheaterId: "theater-hosted-llm",
            Input: input,
            AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
            DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly);

    private static GovernedSeedMemoryContext CreateMemoryContext() =>
        new(
            ContextHandle: "memory-context://test",
            ContextProfile: "test-memory-context",
            ResolverSource: "resident-memory-bootstrap",
            AtlasSource: "resident-root-atlas",
            ValidationReferenceHandle: "selfgel://validation",
            RelevantEngramIds: ["engram-A"],
            RelevantConceptTags: ["bounded-summary"],
            RootSymbolicIds: ["root://standing"],
            UnknownRootCount: 0,
            SelfResolutionDisposition: "stable-self-reference",
            ContextStability: "stable",
            ConceptDensity: "moderate",
            TimestampUtc: DateTimeOffset.UtcNow);

    private sealed class StubHostedLlmProvider : IGovernedHostedLlmProvider
    {
        private readonly GovernedHostedLlmProviderResponse? _response;

        public StubHostedLlmProvider(GovernedHostedLlmProviderResponse? response)
        {
            _response = response;
        }

        public GovernedHostedLlmProviderResponse? TryEvaluate(
            GovernedSeedEvaluationRequest request,
            GovernedSeedMemoryContext personifiedMemoryContext,
            GovernedSeedHostedLlmGovernanceProtocol governanceProtocol)
        {
            return _response;
        }
    }
}
