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
        Assert.NotNull(payload.PreDomainGovernancePacket);
        Assert.NotNull(payload.CandidateBoundaryReceipt);
        Assert.NotNull(payload.CrypticHoldingInspectionReceipt);
        Assert.NotNull(payload.FormOrCleaveAssessment);
        Assert.NotNull(payload.CandidateSeparationReceipt);
        Assert.NotNull(payload.DuplexGovernanceReceipt);
        Assert.NotNull(payload.PreDomainAdmissionGateReceipt);
        Assert.NotNull(payload.PreDomainHostLoopReceipt);
        Assert.NotNull(payload.DomainEligibilityAssessment);
        Assert.NotNull(payload.RoleEligibilityAssessment);
        Assert.NotNull(payload.DomainRoleGatingAssessment);
        Assert.NotNull(payload.DomainRoleGatingReceipt);
        Assert.NotNull(payload.DomainRoleGatingPacket);
        Assert.NotNull(payload.DomainAdmissionAssessment);
        Assert.NotNull(payload.RoleBindingAssessment);
        Assert.NotNull(payload.DomainAdmissionRoleBindingAssessment);
        Assert.NotNull(payload.DomainAdmissionRoleBindingReceipt);
        Assert.NotNull(payload.DomainAdmissionRoleBindingPacket);
        Assert.NotNull(payload.DomainOccupancyAssessment);
        Assert.NotNull(payload.RoleParticipationAssessment);
        Assert.NotNull(payload.PostAdmissionParticipationAssessment);
        Assert.NotNull(payload.PostAdmissionParticipationReceipt);
        Assert.NotNull(payload.PostAdmissionParticipationPacket);
        Assert.NotNull(payload.ServiceBehaviorAssessment);
        Assert.NotNull(payload.ExecutionAuthorizationAssessment);
        Assert.NotNull(payload.PostParticipationExecutionAssessment);
        Assert.NotNull(payload.PostParticipationExecutionReceipt);

        Assert.False(payload.CandidateBoundaryReceipt.ContainsAuthorityBearingFields);
        Assert.Equal(payload.CandidateBoundaryReceipt.CandidateId, payload.PreDomainGovernancePacket.CandidateId);
        Assert.Equal(payload.PreDomainGovernancePacket.PacketHandle, payload.OperationalContext.PreDomainGovernancePacketHandle);
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
        Assert.Equal(payload.PreDomainGovernancePacket.PacketHandle, payload.PreDomainHostLoopReceipt.PreDomainGovernancePacketHandle);
        Assert.Equal(payload.DomainRoleGatingReceipt.ReceiptHandle, payload.OperationalContext.DomainRoleGatingReceiptHandle);
        Assert.Equal(payload.DomainRoleGatingReceipt.Disposition, payload.OperationalContext.DomainRoleGatingDisposition);
        Assert.Equal(payload.DomainRoleGatingReceipt.DomainEligible, payload.OperationalContext.DomainEligible);
        Assert.Equal(payload.DomainRoleGatingReceipt.RoleEligible, payload.OperationalContext.RoleEligible);
        Assert.Equal(payload.DomainRoleGatingPacket.PacketHandle, payload.OperationalContext.DomainRoleGatingPacketHandle);
        Assert.Equal(payload.DomainRoleGatingReceipt.CandidateId, payload.PreDomainGovernancePacket.CandidateId);
        Assert.Equal(payload.DomainRoleGatingReceipt.PacketHandle, payload.PreDomainGovernancePacket.PacketHandle);
        Assert.Equal(payload.DomainRoleGatingPacket.CandidateId, payload.PreDomainGovernancePacket.CandidateId);
        Assert.Equal(payload.DomainRoleGatingPacket.PreDomainGovernancePacket.PacketHandle, payload.PreDomainGovernancePacket.PacketHandle);
        Assert.Equal(payload.DomainRoleGatingPacket.GatingReceipt.ReceiptHandle, payload.DomainRoleGatingReceipt.ReceiptHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.ReceiptHandle, payload.OperationalContext.DomainAdmissionRoleBindingReceiptHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.Disposition, payload.OperationalContext.DomainAdmissionRoleBindingDisposition);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.DomainAdmissionGranted, payload.OperationalContext.DomainAdmissionGranted);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.RoleBound, payload.OperationalContext.RoleBound);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.PacketHandle, payload.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingReceipt.CandidateId, payload.DomainRoleGatingPacket.CandidateId);
        Assert.Equal(payload.DomainAdmissionAssessment.PacketHandle, payload.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(payload.RoleBindingAssessment.PacketHandle, payload.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingAssessment.PacketHandle, payload.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingPacket.PacketHandle, payload.OperationalContext.DomainAdmissionRoleBindingPacketHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingPacket.CandidateId, payload.DomainRoleGatingPacket.CandidateId);
        Assert.Equal(payload.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.PacketHandle, payload.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(payload.DomainAdmissionRoleBindingPacket.DomainAdmissionRoleBindingReceipt.ReceiptHandle, payload.DomainAdmissionRoleBindingReceipt.ReceiptHandle);
        Assert.Equal(payload.PostAdmissionParticipationReceipt.ReceiptHandle, payload.OperationalContext.PostAdmissionParticipationReceiptHandle);
        Assert.Equal(payload.PostAdmissionParticipationReceipt.Disposition, payload.OperationalContext.PostAdmissionParticipationDisposition);
        Assert.Equal(payload.PostAdmissionParticipationReceipt.OccupancyAuthorized, payload.OperationalContext.DomainOccupancyAuthorized);
        Assert.Equal(payload.PostAdmissionParticipationReceipt.RoleParticipationAuthorized, payload.OperationalContext.RoleParticipationAuthorized);
        Assert.Equal(payload.PostAdmissionParticipationPacket.PacketHandle, payload.OperationalContext.PostAdmissionParticipationPacketHandle);
        Assert.Equal(payload.DomainOccupancyAssessment.PacketHandle, payload.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(payload.RoleParticipationAssessment.PacketHandle, payload.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(payload.PostAdmissionParticipationAssessment.PacketHandle, payload.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(payload.PostAdmissionParticipationReceipt.PacketHandle, payload.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(payload.PostAdmissionParticipationPacket.CandidateId, payload.DomainAdmissionRoleBindingPacket.CandidateId);
        Assert.Equal(payload.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.PacketHandle, payload.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(payload.PostAdmissionParticipationPacket.PostAdmissionParticipationReceipt.ReceiptHandle, payload.PostAdmissionParticipationReceipt.ReceiptHandle);
        Assert.Equal(payload.PostParticipationExecutionReceipt.ReceiptHandle, payload.OperationalContext.PostParticipationExecutionReceiptHandle);
        Assert.Equal(payload.PostParticipationExecutionReceipt.Disposition, payload.OperationalContext.PostParticipationExecutionDisposition);
        Assert.Equal(payload.PostParticipationExecutionReceipt.ServiceBehaviorAuthorized, payload.OperationalContext.ServiceBehaviorAuthorized);
        Assert.Equal(payload.PostParticipationExecutionReceipt.ExecutionAuthorized, payload.OperationalContext.ExecutionAuthorized);
        Assert.Equal(payload.ServiceBehaviorAssessment.PacketHandle, payload.PostAdmissionParticipationPacket.PacketHandle);
        Assert.Equal(payload.ExecutionAuthorizationAssessment.PacketHandle, payload.PostAdmissionParticipationPacket.PacketHandle);
        Assert.Equal(payload.PostParticipationExecutionAssessment.PacketHandle, payload.PostAdmissionParticipationPacket.PacketHandle);
        Assert.Equal(payload.PostParticipationExecutionReceipt.PacketHandle, payload.PostAdmissionParticipationPacket.PacketHandle);
    }
}
