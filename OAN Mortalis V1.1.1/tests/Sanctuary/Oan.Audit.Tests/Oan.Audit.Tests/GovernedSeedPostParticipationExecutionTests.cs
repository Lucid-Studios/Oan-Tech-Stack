using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostParticipationExecutionTests
{
    [Fact]
    public void Incomplete_Participation_Packet_Is_Refused()
    {
        var service = new GovernedSeedPostParticipationExecutionService();
        var packet = CreatePacket();
        packet = packet with
        {
            PostAdmissionParticipationReceipt = packet.PostAdmissionParticipationReceipt with
            {
                PacketHandle = string.Empty
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedExecutionAuthorizationDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.False(result.ServiceBehaviorAssessment.PacketComplete);
    }

    [Fact]
    public void Occupancy_Authorized_But_Not_Execution_Ready_Yields_Execution_Pending()
    {
        var service = new GovernedSeedPostParticipationExecutionService();
        var packet = CreatePacket(
            occupancyAuthorized: true,
            participationAuthorized: false,
            primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedExecutionAuthorizationDisposition.ExecutionPending, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceBehaviorAssessment.ServiceBehaviorAuthorized);
        Assert.False(result.ExecutionAuthorizationAssessment.ExecutionAuthorized);
    }

    [Fact]
    public void Packet_With_Service_Behavior_But_Not_Full_Execution_Yields_Service_Behavior_Authorized()
    {
        var service = new GovernedSeedPostParticipationExecutionService();
        var packet = CreatePacket(
            occupancyAuthorized: true,
            participationAuthorized: false,
            roleBound: true,
            primeMaterialCount: 3);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedExecutionAuthorizationDisposition.ServiceBehaviorAuthorized, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceBehaviorAssessment.ServiceBehaviorAuthorized);
        Assert.False(result.ExecutionAuthorizationAssessment.ExecutionAuthorized);
    }

    [Fact]
    public void Fully_Clean_Packet_Yields_Execution_Authorized()
    {
        var service = new GovernedSeedPostParticipationExecutionService();
        var packet = CreatePacket(
            occupancyAuthorized: true,
            participationAuthorized: true,
            primeMaterialCount: 4);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceBehaviorAssessment.ServiceBehaviorAuthorized);
        Assert.True(result.ExecutionAuthorizationAssessment.ExecutionAuthorized);
    }

    [Fact]
    public void Packet_That_Loses_Execution_Readiness_Returns_To_Participation_Pending()
    {
        var service = new GovernedSeedPostParticipationExecutionService();
        var packet = CreatePacket(
            occupancyAuthorized: true,
            participationAuthorized: true,
            primeMaterialCount: 4,
            scopeExpansionDetected: true);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedExecutionAuthorizationDisposition.ReturnToParticipationPending, result.UnifiedAssessment.Disposition);
        Assert.False(result.ServiceBehaviorAssessment.ServiceScopeLawful);
        Assert.False(result.ExecutionAuthorizationAssessment.ExecutionAuthorized);
    }

    private static GovernedSeedPostAdmissionParticipationPacket CreatePacket(
        bool occupancyAuthorized = true,
        bool participationAuthorized = false,
        bool roleBound = false,
        int primeMaterialCount = 3,
        bool scopeExpansionDetected = false,
        bool? executionStructurePresentOverride = null)
    {
        var candidateId = "candidate://post-participation/a";
        var admissionBindingPacket = CreateAdmissionBindingPacket(candidateId, primeMaterialCount, roleBound || participationAuthorized);
        var effectivePrimeMaterialCount = executionStructurePresentOverride is false ? 2 : primeMaterialCount;

        return new GovernedSeedPostAdmissionParticipationPacket(
            PacketHandle: "post-admission-participation-packet://post-participation/a",
            CandidateId: candidateId,
            DomainAdmissionRoleBindingPacket: admissionBindingPacket with
            {
                DomainRoleGatingPacket = admissionBindingPacket.DomainRoleGatingPacket with
                {
                    PreDomainGovernancePacket = admissionBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket with
                    {
                        SeparationReceipt = admissionBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket.SeparationReceipt with
                        {
                            PrimeMaterialCount = effectivePrimeMaterialCount
                        }
                    }
                }
            },
            DomainOccupancyAssessment: new GovernedSeedDomainOccupancyAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                DomainAdmissionGranted: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                AttributionPreserved: true,
                OccupancyStructurePresent: occupancyAuthorized,
                OccupancyAuthorized: occupancyAuthorized,
                Summary: "domain-occupancy"),
            RoleParticipationAssessment: new GovernedSeedRoleParticipationAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                DomainAdmissionGranted: true,
                RoleBound: roleBound || participationAuthorized,
                RoleLawfulWithinDomain: true,
                ResponsibilityBindableAtRoleScope: roleBound || participationAuthorized,
                ScopeExpansionDetected: scopeExpansionDetected,
                RoleParticipationAuthorized: participationAuthorized,
                Summary: "role-participation"),
            PostAdmissionParticipationAssessment: new GovernedSeedPostAdmissionParticipationAssessment(
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: participationAuthorized
                    ? GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized
                    : occupancyAuthorized
                        ? GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized
                        : GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyPending,
                PacketComplete: true,
                StandingConsistent: true,
                RevalidationConsistent: true,
                DomainAdmissionGranted: true,
                OccupancyAuthorized: occupancyAuthorized,
                RoleParticipationAuthorized: participationAuthorized,
                Summary: "unified"),
            PostAdmissionParticipationReceipt: new GovernedSeedPostAdmissionParticipationReceipt(
                ReceiptHandle: "post-admission-participation://post-participation/a",
                PacketHandle: admissionBindingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: participationAuthorized
                    ? GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized
                    : occupancyAuthorized
                        ? GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized
                        : GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyPending,
                OccupancyAuthorized: occupancyAuthorized,
                RoleParticipationAuthorized: participationAuthorized,
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
            PacketHandle: "domain-admission-role-binding-packet://post-participation/a",
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
                ReceiptHandle: "domain-admission-role-binding://post-participation/a",
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
            PacketHandle: "packet://post-participation/a",
            CandidateId: candidateId,
            PrimeSeedStateReceipt: new PrimeSeedStateReceipt(
                ReceiptHandle: "prime-seed://post-participation/a",
                RequestHandle: "prime-seed-request://post-participation/a",
                FirstPrimeReceiptHandle: "first-prime://post-participation/a",
                PrimeRetainedRecordHandle: "prime-retained://post-participation/a",
                StableOneHandle: "stable-one://post-participation/a",
                SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
                SeedSourceHandle: "seed-source://post-participation/a",
                SeedCarrierHandle: "seed-carrier://post-participation/a",
                SeedContinuityHandle: "seed-continuity://post-participation/a",
                SeedIntegrityHandle: "seed-integrity://post-participation/a",
                SeedEvidenceHandles: ["evidence://post-participation/a"],
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
                ReceiptHandle: "candidate-boundary://post-participation/a",
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
                ReceiptHandle: "holding://post-participation/a",
                FormationReceiptHandle: "formation://post-participation/a",
                ListeningFrameHandle: "listening://post-participation/a",
                CompassPacketHandle: "compass://post-participation/a",
                HoldingEntries: [],
                CandidateOnly: true,
                InspectionInfluenceOnly: true,
                PromotionAuthorityWithheld: true,
                ReasonCode: "holding",
                LawfulBasis: "holding",
                TimestampUtc: DateTimeOffset.UtcNow),
            FormOrCleaveAssessment: new GovernedSeedFormOrCleaveAssessment(
                AssessmentHandle: "form-or-cleave://post-participation/a",
                FormationReceiptHandle: "formation://post-participation/a",
                ListeningFrameHandle: "listening://post-participation/a",
                CompassPacketHandle: "compass://post-participation/a",
                HoldingReceiptHandle: "holding://post-participation/a",
                Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                DescendantCandidates: [],
                CandidateOnly: true,
                ReasonCode: "formed",
                LawfulBasis: "formed",
                TimestampUtc: DateTimeOffset.UtcNow),
            SeparationReceipt: new GovernedSeedCandidateSeparationReceipt(
                ReceiptHandle: "candidate-separation://post-participation/a",
                CandidateId: candidateId,
                SeparationSucceeded: true,
                PrimeMaterialCount: primeMaterialCount,
                CrypticMaterialCount: 1,
                CrypticAuthorityBleedDetected: false,
                Summary: "separated"),
            DuplexGovernanceReceipt: new PrimeCrypticDuplexGovernanceReceipt(
                ReceiptHandle: "duplex://post-participation/a",
                CandidateId: candidateId,
                PrimeSurfaceEstablished: primeMaterialCount > 0,
                CrypticSurfaceEstablished: true,
                Summary: "duplex"),
            AdmissionGateReceipt: new PrimeSeedPreDomainAdmissionGateReceipt(
                ReceiptHandle: "admission://post-participation/a",
                CandidateId: candidateId,
                Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "admission"),
            HostLoopReceipt: new GovernedSeedPreDomainHostLoopReceipt(
                ReceiptHandle: "host-loop://post-participation/a",
                FirstPrimeReceiptHandle: "first-prime://post-participation/a",
                PrimeSeedReceiptHandle: "prime-seed://post-participation/a",
                PreDomainGovernancePacketHandle: null,
                CandidateBoundaryReceiptHandle: "candidate-boundary://post-participation/a",
                CrypticHoldingInspectionHandle: "holding://post-participation/a",
                FormOrCleaveAssessmentHandle: "form-or-cleave://post-participation/a",
                CandidateSeparationReceiptHandle: "candidate-separation://post-participation/a",
                DuplexGovernanceReceiptHandle: "duplex://post-participation/a",
                AdmissionGateReceiptHandle: "admission://post-participation/a",
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
            PacketHandle: "domain-role-gating-packet://post-participation/a",
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
                ReceiptHandle: "domain-role-gating://post-participation/a",
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
