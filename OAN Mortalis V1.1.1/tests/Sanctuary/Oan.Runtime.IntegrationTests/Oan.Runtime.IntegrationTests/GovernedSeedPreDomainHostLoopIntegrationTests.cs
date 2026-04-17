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
            Standing:
            - aggregate_correlation_a_b
            Incomplete / uncertain:
            - causal_direction_unknown
            Contradiction:
            - bounded_subset_reversal
            Protected / non-disclosable:
            - raw_shards
            Permitted derivation:
            - aggregate_metrics
            """;

        var result = await host.EvaluateAsync("agent-pre-domain", "theater-pre-domain", prompt);
        var payload = JsonSerializer.Deserialize<GovernedSeedVerticalSlice>(result.Payload!);

        Assert.NotNull(payload);
        Assert.NotNull(payload.OperationalContext);
        Assert.NotNull(payload.CandidateBoundaryReceipt);
        Assert.NotNull(payload.CrypticHoldingInspectionReceipt);
        Assert.NotNull(payload.FormOrCleaveAssessment);
        Assert.NotNull(payload.CandidateSeparationReceipt);
        Assert.NotNull(payload.DuplexGovernanceReceipt);
        Assert.NotNull(payload.PreDomainAdmissionGateReceipt);
        Assert.NotNull(payload.PreDomainHostLoopReceipt);

        Assert.False(payload.CandidateBoundaryReceipt.ContainsAuthorityBearingFields);
        Assert.True(payload.CrypticHoldingInspectionReceipt.CandidateOnly);
        Assert.True(payload.FormOrCleaveAssessment.CandidateOnly);
        Assert.True(payload.PreDomainHostLoopReceipt.CandidateOnly);
        Assert.True(payload.PreDomainHostLoopReceipt.DomainAdmissionWithheld);
        Assert.True(payload.PreDomainHostLoopReceipt.ActionAuthorityWithheld);

        Assert.Equal(payload.CandidateBoundaryReceipt.ReceiptHandle, payload.OperationalContext.CandidateBoundaryReceiptHandle);
        Assert.Equal(payload.CrypticHoldingInspectionReceipt.ReceiptHandle, payload.OperationalContext.CrypticHoldingInspectionHandle);
        Assert.Equal(payload.FormOrCleaveAssessment.AssessmentHandle, payload.OperationalContext.FormOrCleaveAssessmentHandle);
        Assert.Equal(payload.CandidateSeparationReceipt.ReceiptHandle, payload.OperationalContext.CandidateSeparationReceiptHandle);
        Assert.Equal(payload.DuplexGovernanceReceipt.ReceiptHandle, payload.OperationalContext.DuplexGovernanceReceiptHandle);
        Assert.Equal(payload.PreDomainAdmissionGateReceipt.ReceiptHandle, payload.OperationalContext.PreDomainAdmissionGateReceiptHandle);
        Assert.Equal(payload.PreDomainHostLoopReceipt.ReceiptHandle, payload.OperationalContext.PreDomainHostLoopReceiptHandle);
        Assert.Equal(payload.PreDomainAdmissionGateReceipt.Disposition, payload.OperationalContext.PreDomainAdmissionDisposition);
        Assert.Equal(payload.PreDomainHostLoopReceipt.CarryDisposition, payload.OperationalContext.PreDomainCarryDisposition);
        Assert.Equal(payload.PreDomainHostLoopReceipt.CollapseDisposition, payload.OperationalContext.PreDomainCollapseDisposition);
    }
}
