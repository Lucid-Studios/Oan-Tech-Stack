using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostAdmissionParticipationTests
{
    [Fact]
    public void Incomplete_Admission_Binding_Packet_Is_Refused()
    {
        var service = new GovernedSeedPostAdmissionParticipationService();
        var packet = CreatePacket();
        packet = packet with
        {
            DomainAdmissionRoleBindingReceipt = packet.DomainAdmissionRoleBindingReceipt with
            {
                PacketHandle = string.Empty
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedPostAdmissionParticipationDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.False(result.DomainOccupancyAssessment.PacketComplete);
    }

    [Fact]
    public void Admitted_Packet_With_Occupancy_Not_Ready_Yields_Domain_Occupancy_Pending()
    {
        var service = new GovernedSeedPostAdmissionParticipationService();
        var packet = CreatePacket(
            domainAdmissionGranted: true,
            roleBound: false,
            primeMaterialCount: 1);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyPending,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainOccupancyAssessment.DomainAdmissionGranted);
        Assert.False(result.DomainOccupancyAssessment.OccupancyAuthorized);
    }

    [Fact]
    public void Admitted_Packet_With_Occupancy_Ready_But_No_Bound_Role_Yields_Domain_Occupancy_Authorized()
    {
        var service = new GovernedSeedPostAdmissionParticipationService();
        var packet = CreatePacket(
            domainAdmissionGranted: true,
            roleBound: false,
            primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainOccupancyAssessment.OccupancyAuthorized);
        Assert.False(result.RoleParticipationAssessment.RoleParticipationAuthorized);
    }

    [Fact]
    public void Fully_Admitted_And_Role_Bound_Packet_Yields_Role_Participation_Authorized()
    {
        var service = new GovernedSeedPostAdmissionParticipationService();
        var packet = CreatePacket(
            domainAdmissionGranted: true,
            roleBound: true,
            primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainOccupancyAssessment.OccupancyAuthorized);
        Assert.True(result.RoleParticipationAssessment.RoleParticipationAuthorized);
    }

    [Fact]
    public void Packet_That_Loses_Role_Readiness_Returns_To_Binding_Pending()
    {
        var service = new GovernedSeedPostAdmissionParticipationService();
        var packet = CreatePacket(
            domainAdmissionGranted: true,
            roleBound: true,
            roleLawfulWithinDomain: false,
            primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedPostAdmissionParticipationDisposition.ReturnToBindingPending,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainOccupancyAssessment.OccupancyAuthorized);
        Assert.False(result.RoleParticipationAssessment.RoleParticipationAuthorized);
    }

    private static GovernedSeedDomainAdmissionRoleBindingPacket CreatePacket(
        bool domainAdmissionGranted = true,
        bool roleBound = false,
        bool roleLawfulWithinDomain = true,
        int primeMaterialCount = 2)
    {
        var candidateId = "candidate://post-admission/a";
        var gatingPacket = CreateGatingPacket(candidateId, primeMaterialCount);

        return new GovernedSeedDomainAdmissionRoleBindingPacket(
            PacketHandle: "domain-admission-role-binding-packet://post-admission/a",
            CandidateId: candidateId,
            DomainRoleGatingPacket: gatingPacket,
            DomainAdmissionAssessment: new GovernedSeedDomainAdmissionAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                StandingConsistent: true,
                DomainEligibilitySatisfied: domainAdmissionGranted,
                BurdenAttributableAtDomainScope: true,
                DomainAdmissionGranted: domainAdmissionGranted,
                Summary: "domain-admission"),
            RoleBindingAssessment: new GovernedSeedRoleBindingAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                DomainAdmissionGranted: domainAdmissionGranted,
                RoleRelevantStructurePresent: roleBound,
                ResponsibilityBindableAtRoleScope: roleBound,
                RoleLawfulWithinDomain: roleLawfulWithinDomain,
                RoleBound: roleBound,
                Summary: "role-binding"),
            DomainAdmissionRoleBindingAssessment: new GovernedSeedDomainAdmissionRoleBindingAssessment(
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleBound
                    ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound
                    : domainAdmissionGranted
                        ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending
                        : GovernedSeedDomainAdmissionRoleBindingDisposition.RemainAtGatingPacket,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                DomainAdmissionGranted: domainAdmissionGranted,
                RoleBound: roleBound,
                Summary: "unified"),
            DomainAdmissionRoleBindingReceipt: new GovernedSeedDomainAdmissionRoleBindingReceipt(
                ReceiptHandle: "domain-admission-role-binding://post-admission/a",
                PacketHandle: gatingPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: roleBound
                    ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound
                    : domainAdmissionGranted
                        ? GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending
                        : GovernedSeedDomainAdmissionRoleBindingDisposition.RemainAtGatingPacket,
                DomainAdmissionGranted: domainAdmissionGranted,
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
            PacketHandle: "packet://post-admission/a",
            CandidateId: candidateId,
            PrimeSeedStateReceipt: new PrimeSeedStateReceipt(
                ReceiptHandle: "prime-seed://post-admission/a",
                RequestHandle: "prime-seed-request://post-admission/a",
                FirstPrimeReceiptHandle: "first-prime://post-admission/a",
                PrimeRetainedRecordHandle: "prime-retained://post-admission/a",
                StableOneHandle: "stable-one://post-admission/a",
                SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
                SeedSourceHandle: "seed-source://post-admission/a",
                SeedCarrierHandle: "seed-carrier://post-admission/a",
                SeedContinuityHandle: "seed-continuity://post-admission/a",
                SeedIntegrityHandle: "seed-integrity://post-admission/a",
                SeedEvidenceHandles: ["evidence://post-admission/a"],
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
                ReceiptHandle: "candidate-boundary://post-admission/a",
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
                ReceiptHandle: "holding://post-admission/a",
                FormationReceiptHandle: "formation://post-admission/a",
                ListeningFrameHandle: "listening://post-admission/a",
                CompassPacketHandle: "compass://post-admission/a",
                HoldingEntries: [],
                CandidateOnly: true,
                InspectionInfluenceOnly: true,
                PromotionAuthorityWithheld: true,
                ReasonCode: "holding",
                LawfulBasis: "holding",
                TimestampUtc: DateTimeOffset.UtcNow),
            FormOrCleaveAssessment: new GovernedSeedFormOrCleaveAssessment(
                AssessmentHandle: "form-or-cleave://post-admission/a",
                FormationReceiptHandle: "formation://post-admission/a",
                ListeningFrameHandle: "listening://post-admission/a",
                CompassPacketHandle: "compass://post-admission/a",
                HoldingReceiptHandle: "holding://post-admission/a",
                Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                DescendantCandidates: [],
                CandidateOnly: true,
                ReasonCode: "formed",
                LawfulBasis: "formed",
                TimestampUtc: DateTimeOffset.UtcNow),
            SeparationReceipt: new GovernedSeedCandidateSeparationReceipt(
                ReceiptHandle: "candidate-separation://post-admission/a",
                CandidateId: candidateId,
                SeparationSucceeded: true,
                PrimeMaterialCount: primeMaterialCount,
                CrypticMaterialCount: 1,
                CrypticAuthorityBleedDetected: false,
                Summary: "separated"),
            DuplexGovernanceReceipt: new PrimeCrypticDuplexGovernanceReceipt(
                ReceiptHandle: "duplex://post-admission/a",
                CandidateId: candidateId,
                PrimeSurfaceEstablished: primeMaterialCount > 0,
                CrypticSurfaceEstablished: true,
                Summary: "duplex"),
            AdmissionGateReceipt: new PrimeSeedPreDomainAdmissionGateReceipt(
                ReceiptHandle: "admission://post-admission/a",
                CandidateId: candidateId,
                Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "admission"),
            HostLoopReceipt: new GovernedSeedPreDomainHostLoopReceipt(
                ReceiptHandle: "host-loop://post-admission/a",
                FirstPrimeReceiptHandle: "first-prime://post-admission/a",
                PrimeSeedReceiptHandle: "prime-seed://post-admission/a",
                PreDomainGovernancePacketHandle: "packet://post-admission/a",
                CandidateBoundaryReceiptHandle: "candidate-boundary://post-admission/a",
                CrypticHoldingInspectionHandle: "holding://post-admission/a",
                FormOrCleaveAssessmentHandle: "form-or-cleave://post-admission/a",
                CandidateSeparationReceiptHandle: "candidate-separation://post-admission/a",
                DuplexGovernanceReceiptHandle: "duplex://post-admission/a",
                AdmissionGateReceiptHandle: "admission://post-admission/a",
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                CandidateHandles: [candidateId],
                SeedReady: true,
                CandidateOnly: true,
                DomainAdmissionWithheld: true,
                ActionAuthorityWithheld: true,
                ReasonCode: "pre-domain",
                LawfulBasis: "pre-domain",
                TimestampUtc: DateTimeOffset.UtcNow),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "pre-domain");

        return new GovernedSeedDomainRoleGatingPacket(
            PacketHandle: "domain-role-gating-packet://post-admission/a",
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
                RoleRelevantStructurePresent: primeMaterialCount > 1,
                ResponsibilityAttributable: true,
                RoleEligible: primeMaterialCount > 1,
                Summary: "role"),
            GatingAssessment: new GovernedSeedDomainRoleGatingAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: primeMaterialCount > 1
                    ? GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible
                    : GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: false,
                StandingConsistent: true,
                DomainEligible: true,
                RoleEligible: primeMaterialCount > 1,
                Summary: "gating"),
            GatingReceipt: new GovernedSeedDomainRoleGatingReceipt(
                ReceiptHandle: "domain-role-gating://post-admission/a",
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: primeMaterialCount > 1
                    ? GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible
                    : GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete,
                DomainEligible: true,
                RoleEligible: primeMaterialCount > 1,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }
}
