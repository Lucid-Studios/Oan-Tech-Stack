using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedDomainAdmissionRoleBindingTests
{
    [Fact]
    public void Incomplete_Gating_Packet_Is_Refused()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingService();
        var packet = CreatePacket();
        packet = packet with
        {
            GatingReceipt = packet.GatingReceipt with
            {
                PacketHandle = string.Empty
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedDomainAdmissionRoleBindingDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.False(result.DomainAdmissionAssessment.PacketComplete);
    }

    [Fact]
    public void Contaminated_Packet_Is_Refused()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingService();
        var packet = CreatePacket(crypticAuthorityBleedDetected: true);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedDomainAdmissionRoleBindingDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainAdmissionAssessment.CrypticAuthorityBleedDetected);
    }

    [Fact]
    public void Domain_Admissible_But_Role_Incomplete_Yields_Domain_Admitted_Role_Pending()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingService();
        var packet = CreatePacket(
            gatingDisposition: GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete,
            roleEligible: false,
            roleRelevantStructurePresent: false,
            primeMaterialCount: 1);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainAdmissionAssessment.DomainAdmissionGranted);
        Assert.False(result.RoleBindingAssessment.RoleBound);
    }

    [Fact]
    public void Fully_Clean_Packet_Yields_Domain_And_Role_Bound()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingService();
        var packet = CreatePacket(
            gatingDisposition: GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
            roleEligible: true,
            roleRelevantStructurePresent: true,
            primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound,
            result.UnifiedAssessment.Disposition);
        Assert.True(result.DomainAdmissionAssessment.DomainAdmissionGranted);
        Assert.True(result.RoleBindingAssessment.RoleBound);
    }

    [Fact]
    public void Recoverable_Packet_Returns_To_PreDomain_Carry()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingService();
        var packet = CreatePacket(
            gatingDisposition: GovernedSeedDomainRoleGatingDisposition.CrypticOnlyCarry,
            domainEligible: false,
            roleEligible: false,
            roleRelevantStructurePresent: false,
            primeMaterialCount: 1);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainAdmissionRoleBindingDisposition.ReturnToPreDomainCarry,
            result.UnifiedAssessment.Disposition);
        Assert.False(result.DomainAdmissionAssessment.DomainAdmissionGranted);
        Assert.False(result.RoleBindingAssessment.RoleBound);
    }

    private static GovernedSeedDomainRoleGatingPacket CreatePacket(
        GovernedSeedDomainRoleGatingDisposition gatingDisposition = GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
        bool domainEligible = true,
        bool roleEligible = true,
        bool roleRelevantStructurePresent = true,
        bool crypticAuthorityBleedDetected = false,
        int primeMaterialCount = 2)
    {
        var candidateId = "candidate://domain-admission/a";
        var preDomainPacket = CreatePreDomainPacket(candidateId, crypticAuthorityBleedDetected, primeMaterialCount);

        return new GovernedSeedDomainRoleGatingPacket(
            PacketHandle: "domain-role-gating-packet://domain-admission/a",
            CandidateId: candidateId,
            PreDomainGovernancePacket: preDomainPacket,
            DomainEligibilityAssessment: new GovernedSeedDomainEligibilityAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                PacketComplete: true,
                PrimeAdmissionStructurePresent: primeMaterialCount > 0,
                CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
                ForwardMotionSupported: gatingDisposition is GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete or GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                StandingConsistent: true,
                RevalidationConsistent: true,
                DomainEligible: domainEligible,
                Summary: "domain"),
            RoleEligibilityAssessment: new GovernedSeedRoleEligibilityAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                DomainEligible: domainEligible,
                RoleRelevantStructurePresent: roleRelevantStructurePresent,
                ResponsibilityAttributable: true,
                RoleEligible: roleEligible,
                Summary: "role"),
            GatingAssessment: new GovernedSeedDomainRoleGatingAssessment(
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: gatingDisposition,
                PacketComplete: true,
                CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
                StandingConsistent: true,
                DomainEligible: domainEligible,
                RoleEligible: roleEligible,
                Summary: "gating"),
            GatingReceipt: new GovernedSeedDomainRoleGatingReceipt(
                ReceiptHandle: "domain-role-gating://domain-admission/a",
                PacketHandle: preDomainPacket.PacketHandle,
                CandidateId: candidateId,
                Disposition: gatingDisposition,
                DomainEligible: domainEligible,
                RoleEligible: roleEligible,
                Summary: "receipt"),
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }

    private static GovernedSeedPreDomainGovernancePacket CreatePreDomainPacket(
        string candidateId,
        bool crypticAuthorityBleedDetected,
        int primeMaterialCount)
    {
        return new GovernedSeedPreDomainGovernancePacket(
            PacketHandle: "packet://domain-admission/a",
            CandidateId: candidateId,
            PrimeSeedStateReceipt: new PrimeSeedStateReceipt(
                ReceiptHandle: "prime-seed://domain-admission/a",
                RequestHandle: "prime-seed-request://domain-admission/a",
                FirstPrimeReceiptHandle: "first-prime://domain-admission/a",
                PrimeRetainedRecordHandle: "prime-retained://domain-admission/a",
                StableOneHandle: "stable-one://domain-admission/a",
                SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
                SeedSourceHandle: "seed-source://domain-admission/a",
                SeedCarrierHandle: "seed-carrier://domain-admission/a",
                SeedContinuityHandle: "seed-continuity://domain-admission/a",
                SeedIntegrityHandle: "seed-integrity://domain-admission/a",
                SeedEvidenceHandles: ["evidence://domain-admission/a"],
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
                ReceiptHandle: "candidate-boundary://domain-admission/a",
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
                ReceiptHandle: "holding://domain-admission/a",
                FormationReceiptHandle: "formation://domain-admission/a",
                ListeningFrameHandle: "listening://domain-admission/a",
                CompassPacketHandle: "compass://domain-admission/a",
                HoldingEntries: [],
                CandidateOnly: true,
                InspectionInfluenceOnly: true,
                PromotionAuthorityWithheld: true,
                ReasonCode: "holding",
                LawfulBasis: "holding",
                TimestampUtc: DateTimeOffset.UtcNow),
            FormOrCleaveAssessment: new GovernedSeedFormOrCleaveAssessment(
                AssessmentHandle: "form-or-cleave://domain-admission/a",
                FormationReceiptHandle: "formation://domain-admission/a",
                ListeningFrameHandle: "listening://domain-admission/a",
                CompassPacketHandle: "compass://domain-admission/a",
                HoldingReceiptHandle: "holding://domain-admission/a",
                Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
                CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
                CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
                DescendantCandidates: [],
                CandidateOnly: true,
                ReasonCode: "formed",
                LawfulBasis: "formed",
                TimestampUtc: DateTimeOffset.UtcNow),
            SeparationReceipt: new GovernedSeedCandidateSeparationReceipt(
                ReceiptHandle: "candidate-separation://domain-admission/a",
                CandidateId: candidateId,
                SeparationSucceeded: true,
                PrimeMaterialCount: primeMaterialCount,
                CrypticMaterialCount: 1,
                CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
                Summary: "separated"),
            DuplexGovernanceReceipt: new PrimeCrypticDuplexGovernanceReceipt(
                ReceiptHandle: "duplex://domain-admission/a",
                CandidateId: candidateId,
                PrimeSurfaceEstablished: primeMaterialCount > 0,
                CrypticSurfaceEstablished: true,
                Summary: "duplex"),
            AdmissionGateReceipt: new PrimeSeedPreDomainAdmissionGateReceipt(
                ReceiptHandle: "admission://domain-admission/a",
                CandidateId: candidateId,
                Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
                DomainEligible: true,
                RoleEligible: true,
                Summary: "admission"),
            HostLoopReceipt: new GovernedSeedPreDomainHostLoopReceipt(
                ReceiptHandle: "host-loop://domain-admission/a",
                FirstPrimeReceiptHandle: "first-prime://domain-admission/a",
                PrimeSeedReceiptHandle: "prime-seed://domain-admission/a",
                PreDomainGovernancePacketHandle: "packet://domain-admission/a",
                CandidateBoundaryReceiptHandle: "candidate-boundary://domain-admission/a",
                CrypticHoldingInspectionHandle: "holding://domain-admission/a",
                FormOrCleaveAssessmentHandle: "form-or-cleave://domain-admission/a",
                CandidateSeparationReceiptHandle: "candidate-separation://domain-admission/a",
                DuplexGovernanceReceiptHandle: "duplex://domain-admission/a",
                AdmissionGateReceiptHandle: "admission://domain-admission/a",
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
    }
}
