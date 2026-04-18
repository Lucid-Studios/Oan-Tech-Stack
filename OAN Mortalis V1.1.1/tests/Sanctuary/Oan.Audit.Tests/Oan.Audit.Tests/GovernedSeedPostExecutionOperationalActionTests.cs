using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostExecutionOperationalActionTests
{
    [Fact]
    public void Incomplete_Execution_Packet_Is_Refused()
    {
        var service = new GovernedSeedPostExecutionOperationalActionService();
        var packet = CreatePacket();
        packet = packet with
        {
            PostParticipationExecutionReceipt = packet.PostParticipationExecutionReceipt with
            {
                PacketHandle = string.Empty
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedOperationalActionDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.False(result.ServiceEffectAssessment.PacketComplete);
    }

    [Fact]
    public void Execution_Authorized_But_Not_Commit_Ready_Yields_Operational_Action_Pending()
    {
        var service = new GovernedSeedPostExecutionOperationalActionService();
        var packet = CreatePacket(
            serviceEffectAuthorized: true,
            executionAuthorized: true,
            primeMaterialCount: 3);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedOperationalActionDisposition.OperationalActionPending, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceEffectAssessment.ServiceEffectAuthorized);
        Assert.False(result.OperationalActionCommitAssessment.OperationalActionCommitted);
    }

    [Fact]
    public void Packet_With_Service_Effect_But_Not_Full_Commit_Yields_Service_Effect_Authorized()
    {
        var service = new GovernedSeedPostExecutionOperationalActionService();
        var packet = CreatePacket(
            serviceEffectAuthorized: true,
            executionAuthorized: true,
            roleBearingActionRequested: true,
            roleParticipationAuthorized: false,
            primeMaterialCount: 4);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceEffectAssessment.ServiceEffectAuthorized);
        Assert.False(result.OperationalActionCommitAssessment.OperationalActionCommitted);
    }

    [Fact]
    public void Fully_Clean_Packet_Yields_Operational_Action_Committed()
    {
        var service = new GovernedSeedPostExecutionOperationalActionService();
        var packet = CreatePacket(
            serviceEffectAuthorized: true,
            executionAuthorized: true,
            roleBearingActionRequested: false,
            roleParticipationAuthorized: true,
            primeMaterialCount: 5);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedOperationalActionDisposition.OperationalActionCommitted, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceEffectAssessment.ServiceEffectAuthorized);
        Assert.True(result.OperationalActionCommitAssessment.OperationalActionCommitted);
    }

    [Fact]
    public void Packet_That_Loses_Commit_Readiness_Returns_To_Execution_Pending()
    {
        var service = new GovernedSeedPostExecutionOperationalActionService();
        var packet = CreatePacket(
            serviceEffectAuthorized: true,
            executionAuthorized: true,
            roleParticipationAuthorized: true,
            primeMaterialCount: 5,
            explicitScopePreserved: false);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedOperationalActionDisposition.ReturnToExecutionPending, result.UnifiedAssessment.Disposition);
        Assert.False(result.ServiceEffectAssessment.ExplicitScopePreserved);
        Assert.False(result.OperationalActionCommitAssessment.OperationalActionCommitted);
    }

    private static GovernedSeedPostParticipationExecutionPacket CreatePacket(
        bool serviceEffectAuthorized = true,
        bool executionAuthorized = true,
        bool roleBearingActionRequested = false,
        bool roleParticipationAuthorized = true,
        int primeMaterialCount = 4,
        bool explicitScopePreserved = true)
    {
        var candidateId = "candidate://post-execution/a";
        var participationPacket = CreateParticipationPacket(
            candidateId,
            roleParticipationAuthorized,
            primeMaterialCount,
            explicitScopePreserved);

        return new GovernedSeedPostParticipationExecutionPacket(
            PacketHandle: "post-participation-execution-packet://post-execution/a",
            CandidateId: candidateId,
            PostAdmissionParticipationPacket: participationPacket,
            ServiceBehaviorAssessment: new GovernedSeedServiceBehaviorAssessment(
                PacketHandle: participationPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                OccupancyAuthorized: true,
                ParticipationAuthorized: roleParticipationAuthorized,
                StandingConsistent: true,
                RevalidationConsistent: true,
                AttributionPreserved: true,
                ServiceScopeLawful: explicitScopePreserved,
                ServiceBehaviorAuthorized: serviceEffectAuthorized,
                Summary: "service-behavior"),
            ExecutionAuthorizationAssessment: new GovernedSeedExecutionAuthorizationAssessment(
                PacketHandle: participationPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                OccupancyAuthorized: true,
                ParticipationAuthorized: roleParticipationAuthorized,
                ExecutionStructurePresent: primeMaterialCount > 2,
                ExplicitScopePreserved: explicitScopePreserved,
                RoleBearingExecutionRequested: roleBearingActionRequested,
                ExecutionAuthorized: executionAuthorized,
                Summary: "execution-authorization"),
            PostParticipationExecutionAssessment: new GovernedSeedPostParticipationExecutionAssessment(
                PacketHandle: participationPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: executionAuthorized
                    ? GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized
                    : GovernedSeedExecutionAuthorizationDisposition.ExecutionPending,
                PacketComplete: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                ServiceBehaviorAuthorized: serviceEffectAuthorized,
                ExecutionAuthorized: executionAuthorized,
                Summary: "unified"),
            PostParticipationExecutionReceipt: new GovernedSeedPostParticipationExecutionReceipt(
                ReceiptHandle: "post-participation-execution://post-execution/a",
                PacketHandle: participationPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: executionAuthorized
                    ? GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized
                    : GovernedSeedExecutionAuthorizationDisposition.ExecutionPending,
                ServiceBehaviorAuthorized: serviceEffectAuthorized,
                ExecutionAuthorized: executionAuthorized,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }

    private static GovernedSeedPostAdmissionParticipationPacket CreateParticipationPacket(
        string candidateId,
        bool roleParticipationAuthorized,
        int primeMaterialCount,
        bool explicitScopePreserved)
    {
        var admissionBindingPacket = CreateAdmissionBindingPacket(candidateId, primeMaterialCount, roleParticipationAuthorized);

        return new GovernedSeedPostAdmissionParticipationPacket(
            PacketHandle: "post-admission-participation-packet://post-execution/a",
            CandidateId: candidateId,
            DomainAdmissionRoleBindingPacket: admissionBindingPacket,
            DomainOccupancyAssessment: new GovernedSeedDomainOccupancyAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                DomainAdmissionGranted: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                AttributionPreserved: true,
                OccupancyStructurePresent: true,
                OccupancyAuthorized: true,
                Summary: "domain-occupancy"),
            RoleParticipationAssessment: new GovernedSeedRoleParticipationAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                DomainAdmissionGranted: true,
                RoleBound: roleParticipationAuthorized,
                RoleLawfulWithinDomain: true,
                ResponsibilityBindableAtRoleScope: roleParticipationAuthorized,
                ScopeExpansionDetected: !explicitScopePreserved,
                RoleParticipationAuthorized: roleParticipationAuthorized,
                Summary: "role-participation"),
            PostAdmissionParticipationAssessment: new GovernedSeedPostAdmissionParticipationAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleParticipationAuthorized
                    ? GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized
                    : GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized,
                PacketComplete: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                DomainAdmissionGranted: true,
                OccupancyAuthorized: true,
                RoleParticipationAuthorized: roleParticipationAuthorized,
                Summary: "unified"),
            PostAdmissionParticipationReceipt: new GovernedSeedPostAdmissionParticipationReceipt(
                ReceiptHandle: "post-admission-participation://post-execution/a",
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleParticipationAuthorized
                    ? GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized
                    : GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized,
                OccupancyAuthorized: true,
                RoleParticipationAuthorized: roleParticipationAuthorized,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }

    private static GovernedSeedDomainAdmissionRoleBindingPacket CreateAdmissionBindingPacket(
        string candidateId,
        int primeMaterialCount,
        bool roleBound)
    {
        var gatingPacket = CreateGatingPacket(candidateId, primeMaterialCount);

        return new GovernedSeedDomainAdmissionRoleBindingPacket(
            PacketHandle: "domain-admission-role-binding-packet://post-execution/a",
            CandidateId: candidateId,
            DomainRoleGatingPacket: gatingPacket,
            DomainAdmissionAssessment: new GovernedSeedDomainAdmissionAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                StandingConsistent: true,
                DomainEligibilitySatisfied: true,
                BurdenAttributableAtDomainScope: true,
                DomainAdmissionGranted: true,
                Summary: "domain-admission"),
            RoleBindingAssessment: new GovernedSeedRoleBindingAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                DomainAdmissionGranted: true,
                RoleRelevantStructurePresent: roleBound,
                ResponsibilityBindableAtRoleScope: roleBound,
                RoleLawfulWithinDomain: true,
                RoleBound: roleBound,
                Summary: "role-binding"),
            DomainAdmissionRoleBindingAssessment: new GovernedSeedDomainAdmissionRoleBindingAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleBound
                    ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound
                    : GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                DomainAdmissionGranted: true,
                RoleBound: roleBound,
                Summary: "unified"),
            DomainAdmissionRoleBindingReceipt: new GovernedSeedDomainAdmissionRoleBindingReceipt(
                ReceiptHandle: "domain-admission-role-binding://post-execution/a",
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleBound
                    ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound
                    : GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
                DomainAdmissionGranted: true,
                RoleBound: roleBound,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }

    private static GovernedSeedDomainRoleGatingPacket CreateGatingPacket(
        string candidateId,
        int primeMaterialCount)
    {
        var preDomainPacket = new GovernedSeedPreDomainGovernancePacket(
            PacketHandle: "packet://post-execution/a",
            CandidateId: candidateId,
            PrimeSeedStateReceipt: new PrimeSeedStateReceipt(
                ReceiptHandle: "prime-seed://post-execution/a",
                RequestHandle: "prime-seed-request://post-execution/a",
                FirstPrimeReceiptHandle: "first-prime://post-execution/a",
                PrimeRetainedRecordHandle: "prime-retained://post-execution/a",
                StableOneHandle: "stable-one://post-execution/a",
                SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
                SeedSourceHandle: "seed-source://post-execution/a",
                SeedCarrierHandle: "seed-carrier://post-execution/a",
                SeedContinuityHandle: "seed-continuity://post-execution/a",
                SeedIntegrityHandle: "seed-integrity://post-execution/a",
                SeedEvidenceHandles: ["evidence://post-execution/a"],
                FirstPrimePreRoleStandingPresent: true,
                StableOnePresent: true,
                PrimeRetainedStandingPresent: true,
                SeedSourcePresent: true,
                SeedCarrierPresent: true,
                SeedContinuityPresent: true,
                SeedIntegrityPresent: true,
                DomainAdmissionWithheld: true,
                LawfullyBondedDomainIntegrationWithheld: true,
                CmeFoundingWithheld: true,
                CmeMintingWithheld: true,
                RuntimePersonaWithheld: true,
                RoleEnactmentWithheld: true,
                ActionAuthorityWithheld: true,
                MotherFatherDomainRoleApplicationWithheld: true,
                CradleLocalGoverningSurfaceWithheld: true,
                PrimeClosureStillWithheld: true,
                CandidateOnly: true,
                ConstraintCodes: ["candidate-only"],
                ReasonCode: "pre-domain-standing",
                LawfulBasis: "pre-domain-standing",
                TimestampUtc: DateTimeOffset.UtcNow),
            BoundaryReceipt: new GovernedSeedCandidateBoundaryReceipt(
                ReceiptHandle: "candidate-boundary://post-execution/a",
                CandidateId: candidateId,
                SourceType: GovernedSeedCandidateSourceType.SyntheticTest,
                SourceChannel: "audit",
                ObservedAtUtc: DateTimeOffset.UtcNow,
                ContainsAuthorityBearingFields: false,
                CandidateProposalCount: 1,
                HoldingMutationProposalCount: 0,
                ResonanceObservationCount: 1,
                DescendantProposalCount: 0,
                CollapseSuggestionCount: 0,
                Summary: "candidate-only"),
            HoldingInspectionReceipt: new GovernedSeedCrypticHoldingInspectionReceipt(
                ReceiptHandle: "holding://post-execution/a",
                FormationReceiptHandle: "formation://post-execution/a",
                ListeningFrameHandle: "listening://post-execution/a",
                CompassPacketHandle: "compass://post-execution/a",
                HoldingEntries: [],
                CandidateOnly: true,
                InspectionInfluenceOnly: true,
                PromotionAuthorityWithheld: true,
                ReasonCode: "holding",
                LawfulBasis: "holding",
                TimestampUtc: DateTimeOffset.UtcNow),
            FormOrCleaveAssessment: new GovernedSeedFormOrCleaveAssessment(
                AssessmentHandle: "form-or-cleave://post-execution/a",
                FormationReceiptHandle: "formation://post-execution/a",
                ListeningFrameHandle: "listening://post-execution/a",
                CompassPacketHandle: "compass://post-execution/a",
                HoldingReceiptHandle: "holding://post-execution/a",
                Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                DescendantCandidates: [],
                CandidateOnly: true,
                ReasonCode: "formed",
                LawfulBasis: "formed",
                TimestampUtc: DateTimeOffset.UtcNow),
            SeparationReceipt: new GovernedSeedCandidateSeparationReceipt(
                ReceiptHandle: "candidate-separation://post-execution/a",
                CandidateId: candidateId,
                SeparationSucceeded: true,
                PrimeMaterialCount: primeMaterialCount,
                CrypticMaterialCount: 1,
                CrypticAuthorityBleedDetected: false,
                Summary: "separated"),
            DuplexGovernanceReceipt: new PrimeCrypticDuplexGovernanceReceipt(
                ReceiptHandle: "duplex://post-execution/a",
                CandidateId: candidateId,
                PrimeSurfaceEstablished: primeMaterialCount > 0,
                CrypticSurfaceEstablished: true,
                Summary: "duplex"),
            AdmissionGateReceipt: new PrimeSeedPreDomainAdmissionGateReceipt(
                ReceiptHandle: "admission://post-execution/a",
                CandidateId: candidateId,
                Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "admission"),
            HostLoopReceipt: new GovernedSeedPreDomainHostLoopReceipt(
                ReceiptHandle: "host-loop://post-execution/a",
                FirstPrimeReceiptHandle: "first-prime://post-execution/a",
                PrimeSeedReceiptHandle: "prime-seed://post-execution/a",
                PreDomainGovernancePacketHandle: null,
                CandidateBoundaryReceiptHandle: "candidate-boundary://post-execution/a",
                CrypticHoldingInspectionHandle: "holding://post-execution/a",
                FormOrCleaveAssessmentHandle: "form-or-cleave://post-execution/a",
                CandidateSeparationReceiptHandle: "candidate-separation://post-execution/a",
                DuplexGovernanceReceiptHandle: "duplex://post-execution/a",
                AdmissionGateReceiptHandle: "admission://post-execution/a",
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                CandidateHandles: [candidateId],
                SeedReady: true,
                CandidateOnly: true,
                DomainAdmissionWithheld: true,
                ActionAuthorityWithheld: true,
                ReasonCode: "carry",
                LawfulBasis: "bounded carry",
                TimestampUtc: DateTimeOffset.UtcNow),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "pre-domain-packet");

        return new GovernedSeedDomainRoleGatingPacket(
            PacketHandle: "domain-role-gating-packet://post-execution/a",
            CandidateId: candidateId,
            PreDomainGovernancePacket: preDomainPacket,
            DomainEligibilityAssessment: new GovernedSeedDomainEligibilityAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                PrimeAdmissionStructurePresent: true,
                CrypticAuthorityBleedDetected: false,
                ForwardMotionSupported: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                DomainEligible: true,
                Summary: "domain"),
            RoleEligibilityAssessment: new GovernedSeedRoleEligibilityAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                DomainEligible: true,
                RoleRelevantStructurePresent: true,
                ResponsibilityAttributable: true,
                RoleEligible: true,
                Summary: "role"),
            GatingAssessment: new GovernedSeedDomainRoleGatingAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                StandingConsistent: true,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "gating"),
            GatingReceipt: new GovernedSeedDomainRoleGatingReceipt(
                ReceiptHandle: "domain-role-gating://post-execution/a",
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "gating-packet");
    }
}
