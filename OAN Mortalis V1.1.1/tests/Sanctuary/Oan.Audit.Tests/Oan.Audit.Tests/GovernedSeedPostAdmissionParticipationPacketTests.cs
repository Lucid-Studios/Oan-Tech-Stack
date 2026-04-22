using San.Common;
using San.Runtime.Materialization;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostAdmissionParticipationPacketTests
{
    [Fact]
    public void Materialized_Packet_Preserves_Admission_Binding_Identity_And_Disposition()
    {
        var service = new GovernedSeedPostAdmissionParticipationPacketMaterializationService();

        var admissionBindingPacket = CreateAdmissionBindingPacket();
        var domainOccupancyAssessment = CreateDomainOccupancyAssessment(admissionBindingPacket.PacketHandle, admissionBindingPacket.CandidateId, true);
        var roleParticipationAssessment = CreateRoleParticipationAssessment(admissionBindingPacket.PacketHandle, admissionBindingPacket.CandidateId, true, true);
        var unifiedAssessment = CreateUnifiedAssessment(
            admissionBindingPacket.PacketHandle,
            admissionBindingPacket.CandidateId,
            GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized,
            true,
            true);
        var receipt = CreateReceipt(
            admissionBindingPacket.PacketHandle,
            admissionBindingPacket.CandidateId,
            GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized,
            true,
            true);

        var packet = service.Materialize(
            admissionBindingPacket,
            domainOccupancyAssessment,
            roleParticipationAssessment,
            unifiedAssessment,
            receipt);

        Assert.Equal(admissionBindingPacket.CandidateId, packet.CandidateId);
        Assert.Equal(admissionBindingPacket.PacketHandle, packet.DomainAdmissionRoleBindingPacket.PacketHandle);
        Assert.Equal(GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized, packet.PostAdmissionParticipationAssessment.Disposition);
        Assert.Equal(packet.PostAdmissionParticipationAssessment.Disposition, packet.PostAdmissionParticipationReceipt.Disposition);
        Assert.True(packet.DomainOccupancyAssessment.OccupancyAuthorized);
        Assert.True(packet.RoleParticipationAssessment.RoleParticipationAuthorized);
    }

    private static GovernedSeedDomainAdmissionRoleBindingPacket CreateAdmissionBindingPacket()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService();
        var gatingPacket = CreateGatingPacket();

        return service.Materialize(
            gatingPacket,
            CreateDomainAdmissionAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, false),
            CreateRoleBindingAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, true),
            CreateDomainAdmissionRoleBindingAssessment(
                gatingPacket.PacketHandle,
                gatingPacket.CandidateId,
                GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound,
                true,
                true),
            CreateDomainAdmissionRoleBindingReceipt(
                gatingPacket.PacketHandle,
                gatingPacket.CandidateId,
                GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound,
                true,
                true));
    }

    private static GovernedSeedDomainRoleGatingPacket CreateGatingPacket()
    {
        var service = new GovernedSeedDomainRoleGatingPacketMaterializationService();
        var preDomainPacket = CreatePreDomainPacket();

        return service.Materialize(
            preDomainPacket,
            CreateDomainEligibilityAssessment(preDomainPacket.PacketHandle, preDomainPacket.CandidateId, true),
            CreateRoleEligibilityAssessment(preDomainPacket.PacketHandle, preDomainPacket.CandidateId, true, true),
            CreateDomainRoleGatingAssessment(
                preDomainPacket.PacketHandle,
                preDomainPacket.CandidateId,
                GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                true,
                true),
            CreateDomainRoleGatingReceipt(
                preDomainPacket.PacketHandle,
                preDomainPacket.CandidateId,
                GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                true,
                true));
    }

    private static GovernedSeedPreDomainGovernancePacket CreatePreDomainPacket()
    {
        var service = new GovernedSeedPreDomainGovernancePacketMaterializationService();
        return service.Materialize(
            CreatePrimeSeedReceipt(),
            CreateBoundaryReceipt(),
            CreateHoldingReceipt(),
            CreateFormOrCleaveAssessment(),
            CreateSeparationReceipt(),
            CreateDuplexReceipt(),
            CreateAdmissionGateReceipt(),
            CreateHostLoopReceipt());
    }

    private static GovernedSeedDomainOccupancyAssessment CreateDomainOccupancyAssessment(
        string packetHandle,
        string candidateId,
        bool occupancyAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            DomainAdmissionGranted: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            OccupancyStructurePresent: true,
            OccupancyAuthorized: occupancyAuthorized,
            Summary: "domain-occupancy");

    private static GovernedSeedRoleParticipationAssessment CreateRoleParticipationAssessment(
        string packetHandle,
        string candidateId,
        bool roleBound,
        bool roleParticipationAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainAdmissionGranted: true,
            RoleBound: roleBound,
            RoleLawfulWithinDomain: true,
            ResponsibilityBindableAtRoleScope: true,
            ScopeExpansionDetected: false,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "role-participation");

    private static GovernedSeedPostAdmissionParticipationAssessment CreateUnifiedAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedPostAdmissionParticipationDisposition disposition,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            DomainAdmissionGranted: true,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "unified");

    private static GovernedSeedPostAdmissionParticipationReceipt CreateReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedPostAdmissionParticipationDisposition disposition,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized) =>
        new(
            ReceiptHandle: "post-admission-participation://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "receipt");

    private static GovernedSeedDomainAdmissionAssessment CreateDomainAdmissionAssessment(
        string packetHandle,
        string candidateId,
        bool domainAdmissionGranted,
        bool crypticAuthorityBleedDetected) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            StandingConsistent: true,
            DomainEligibilitySatisfied: true,
            BurdenAttributableAtDomainScope: true,
            DomainAdmissionGranted: domainAdmissionGranted,
            Summary: "domain-admission");

    private static GovernedSeedRoleBindingAssessment CreateRoleBindingAssessment(
        string packetHandle,
        string candidateId,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleRelevantStructurePresent: roleBound,
            ResponsibilityBindableAtRoleScope: true,
            RoleLawfulWithinDomain: true,
            RoleBound: roleBound,
            Summary: "role-binding");

    private static GovernedSeedDomainAdmissionRoleBindingAssessment CreateDomainAdmissionRoleBindingAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainAdmissionRoleBindingDisposition disposition,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: false,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: "unified");

    private static GovernedSeedDomainAdmissionRoleBindingReceipt CreateDomainAdmissionRoleBindingReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainAdmissionRoleBindingDisposition disposition,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            ReceiptHandle: "domain-admission-role-binding://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: "receipt");

    private static GovernedSeedDomainEligibilityAssessment CreateDomainEligibilityAssessment(
        string packetHandle,
        string candidateId,
        bool packetComplete) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: packetComplete,
            PrimeAdmissionStructurePresent: true,
            CrypticAuthorityBleedDetected: false,
            ForwardMotionSupported: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            DomainEligible: true,
            Summary: "domain");

    private static GovernedSeedRoleEligibilityAssessment CreateRoleEligibilityAssessment(
        string packetHandle,
        string candidateId,
        bool domainEligible,
        bool roleEligible) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainEligible: domainEligible,
            RoleRelevantStructurePresent: roleEligible,
            ResponsibilityAttributable: true,
            RoleEligible: roleEligible,
            Summary: "role");

    private static GovernedSeedDomainRoleGatingAssessment CreateDomainRoleGatingAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainRoleGatingDisposition disposition,
        bool domainEligible,
        bool roleEligible) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: false,
            StandingConsistent: true,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: "gating");

    private static GovernedSeedDomainRoleGatingReceipt CreateDomainRoleGatingReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainRoleGatingDisposition disposition,
        bool domainEligible,
        bool roleEligible) =>
        new(
            ReceiptHandle: "domain-role-gating://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: "receipt");

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt() =>
        new(
            ReceiptHandle: "prime-seed://packet/a",
            RequestHandle: "prime-seed-request://packet/a",
            FirstPrimeReceiptHandle: "first-prime://packet/a",
            PrimeRetainedRecordHandle: "prime-retained://packet/a",
            StableOneHandle: "stable-one://packet/a",
            SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
            SeedSourceHandle: "seed-source://packet/a",
            SeedCarrierHandle: "seed-carrier://packet/a",
            SeedContinuityHandle: "seed-continuity://packet/a",
            SeedIntegrityHandle: "seed-integrity://packet/a",
            SeedEvidenceHandles: ["evidence://packet/a"],
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
            ReasonCode: "prime-seed-state-pre-domain-standing",
            LawfulBasis: "pre-domain standing.",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedCandidateBoundaryReceipt CreateBoundaryReceipt() =>
        new(
            ReceiptHandle: "candidate-boundary://packet/a",
            CandidateId: "candidate://packet/a",
            SourceType: GovernedSeedCandidateSourceType.SyntheticTest,
            SourceChannel: "audit",
            ObservedAtUtc: DateTimeOffset.UtcNow,
            ContainsAuthorityBearingFields: false,
            CandidateProposalCount: 1,
            HoldingMutationProposalCount: 0,
            ResonanceObservationCount: 1,
            DescendantProposalCount: 1,
            CollapseSuggestionCount: 0,
            Summary: "candidate-only");

    private static GovernedSeedCrypticHoldingInspectionReceipt CreateHoldingReceipt() =>
        new(
            ReceiptHandle: "holding://packet/a",
            FormationReceiptHandle: "formation://packet/a",
            ListeningFrameHandle: "listening://packet/a",
            CompassPacketHandle: "compass://packet/a",
            HoldingEntries: [],
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: "holding",
            LawfulBasis: "holding",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedFormOrCleaveAssessment CreateFormOrCleaveAssessment() =>
        new(
            AssessmentHandle: "form-or-cleave://packet/a",
            FormationReceiptHandle: "formation://packet/a",
            ListeningFrameHandle: "listening://packet/a",
            CompassPacketHandle: "compass://packet/a",
            HoldingReceiptHandle: "holding://packet/a",
            Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
            CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            DescendantCandidates: [],
            CandidateOnly: true,
            ReasonCode: "formed",
            LawfulBasis: "formed",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedCandidateSeparationReceipt CreateSeparationReceipt() =>
        new(
            ReceiptHandle: "candidate-separation://packet/a",
            CandidateId: "candidate://packet/a",
            SeparationSucceeded: true,
            PrimeMaterialCount: 2,
            CrypticMaterialCount: 1,
            CrypticAuthorityBleedDetected: false,
            Summary: "separated");

    private static PrimeCrypticDuplexGovernanceReceipt CreateDuplexReceipt() =>
        new(
            ReceiptHandle: "duplex://packet/a",
            CandidateId: "candidate://packet/a",
            PrimeSurfaceEstablished: true,
            CrypticSurfaceEstablished: true,
            Summary: "duplex");

    private static PrimeSeedPreDomainAdmissionGateReceipt CreateAdmissionGateReceipt() =>
        new(
            ReceiptHandle: "admission://packet/a",
            CandidateId: "candidate://packet/a",
            Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
            DomainEligible: true,
            RoleEligible: true,
            Summary: "admission");

    private static GovernedSeedPreDomainHostLoopReceipt CreateHostLoopReceipt() =>
        new(
            ReceiptHandle: "host-loop://packet/a",
            FirstPrimeReceiptHandle: "first-prime://packet/a",
            PrimeSeedReceiptHandle: "prime-seed://packet/a",
            PreDomainGovernancePacketHandle: null,
            CandidateBoundaryReceiptHandle: "candidate-boundary://packet/a",
            CrypticHoldingInspectionHandle: "holding://packet/a",
            FormOrCleaveAssessmentHandle: "form-or-cleave://packet/a",
            CandidateSeparationReceiptHandle: "candidate-separation://packet/a",
            DuplexGovernanceReceiptHandle: "duplex://packet/a",
            AdmissionGateReceiptHandle: "admission://packet/a",
            CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            CandidateHandles: ["candidate://packet/a"],
            SeedReady: true,
            CandidateOnly: true,
            DomainAdmissionWithheld: true,
            ActionAuthorityWithheld: true,
            ReasonCode: "carry",
            LawfulBasis: "bounded carry",
            TimestampUtc: DateTimeOffset.UtcNow);
}
