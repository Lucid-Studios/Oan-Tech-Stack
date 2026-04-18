using San.Common;
using San.Runtime.Materialization;

namespace San.Audit.Tests;

public sealed class GovernedSeedDomainAdmissionRoleBindingPacketTests
{
    [Fact]
    public void Materialized_Packet_Preserves_Gating_Identity_And_Disposition()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService();

        var gatingPacket = CreateGatingPacket();
        var domainAdmissionAssessment = CreateDomainAdmissionAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, false);
        var roleBindingAssessment = CreateRoleBindingAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, false);
        var unifiedAssessment = CreateUnifiedAssessment(
            gatingPacket.PacketHandle,
            gatingPacket.CandidateId,
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
            true,
            false);
        var receipt = CreateReceipt(
            gatingPacket.PacketHandle,
            gatingPacket.CandidateId,
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
            true,
            false);

        var packet = service.Materialize(
            gatingPacket,
            domainAdmissionAssessment,
            roleBindingAssessment,
            unifiedAssessment,
            receipt);

        Assert.Equal(gatingPacket.CandidateId, packet.CandidateId);
        Assert.Equal(gatingPacket.PacketHandle, packet.DomainRoleGatingPacket.PacketHandle);
        Assert.Equal(GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending, packet.DomainAdmissionRoleBindingAssessment.Disposition);
        Assert.Equal(packet.DomainAdmissionRoleBindingAssessment.Disposition, packet.DomainAdmissionRoleBindingReceipt.Disposition);
        Assert.True(packet.DomainAdmissionAssessment.DomainAdmissionGranted);
        Assert.False(packet.RoleBindingAssessment.RoleBound);
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

    private static GovernedSeedDomainAdmissionRoleBindingAssessment CreateUnifiedAssessment(
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

    private static GovernedSeedDomainAdmissionRoleBindingReceipt CreateReceipt(
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
