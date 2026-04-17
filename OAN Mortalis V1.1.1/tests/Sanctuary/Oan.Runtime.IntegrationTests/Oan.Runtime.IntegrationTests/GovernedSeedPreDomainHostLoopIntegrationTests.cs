using System.Text.Json;
using CradleTek.Host;
using San.Common;
using San.Runtime.Headless;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedSeedPreDomainHostLoopIntegrationTests
{
    [Fact]
    public async Task Evaluate_Materializes_PreDomain_HostLoop_Receipts()
    {
        IGovernedSeedHost host = HeadlessRuntimeBootstrap.CreateHost();
        var prompt = """
            Abilities:
            - bounded_reasoning_surface
            - continuity_witness

            Office:
            - bounded office
            """;

        var result = await host.EvaluateAsync("agent-pre-domain", "theater-pre-domain", prompt);
        var payload = JsonSerializer.Deserialize<GovernedSeedVerticalSlice>(result.Payload!);

        Assert.NotNull(payload);
        Assert.NotNull(payload.OperationalContext);
        Assert.NotNull(payload.CrypticHoldingInspectionReceipt);
        Assert.NotNull(payload.FormOrCleaveAssessment);
        Assert.NotNull(payload.PreDomainHostLoopReceipt);

        Assert.True(payload.CrypticHoldingInspectionReceipt.CandidateOnly);
        Assert.True(payload.FormOrCleaveAssessment.CandidateOnly);
        Assert.True(payload.PreDomainHostLoopReceipt.CandidateOnly);
        Assert.True(payload.PreDomainHostLoopReceipt.DomainAdmissionWithheld);
        Assert.True(payload.PreDomainHostLoopReceipt.ActionAuthorityWithheld);

        Assert.Equal(payload.CrypticHoldingInspectionReceipt.ReceiptHandle, payload.OperationalContext.CrypticHoldingInspectionHandle);
        Assert.Equal(payload.FormOrCleaveAssessment.AssessmentHandle, payload.OperationalContext.FormOrCleaveAssessmentHandle);
        Assert.Equal(payload.PreDomainHostLoopReceipt.ReceiptHandle, payload.OperationalContext.PreDomainHostLoopReceiptHandle);
        Assert.Equal(payload.PreDomainHostLoopReceipt.CarryDisposition, payload.OperationalContext.PreDomainCarryDisposition);
        Assert.Equal(payload.PreDomainHostLoopReceipt.CollapseDisposition, payload.OperationalContext.PreDomainCollapseDisposition);
    }
}
