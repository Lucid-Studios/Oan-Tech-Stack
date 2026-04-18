using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedDomainRoleGatingTests
{
    [Fact]
    public void Incomplete_Packet_Is_Refused()
    {
        var service = new GovernedSeedDomainRoleGatingService();
        var packet = CreatePacket();
        packet = packet with
        {
            HostLoopReceipt = packet.HostLoopReceipt with
            {
                CandidateBoundaryReceiptHandle = null
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedDomainRoleGatingDisposition.Refuse, result.GatingAssessment.Disposition);
        Assert.False(result.DomainAssessment.PacketComplete);
    }

    [Fact]
    public void Cryptic_Authority_Contamination_Is_Refused()
    {
        var service = new GovernedSeedDomainRoleGatingService();
        var packet = CreatePacket(crypticAuthorityBleedDetected: true);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedDomainRoleGatingDisposition.Refuse, result.GatingAssessment.Disposition);
        Assert.True(result.DomainAssessment.CrypticAuthorityBleedDetected);
    }

    [Fact]
    public void Domain_Clean_But_Role_Thin_Packet_Is_Domain_Admissible_Role_Incomplete()
    {
        var service = new GovernedSeedDomainRoleGatingService();
        var packet = CreatePacket(primeMaterialCount: 1);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete,
            result.GatingAssessment.Disposition);
        Assert.True(result.DomainAssessment.DomainEligible);
        Assert.False(result.RoleAssessment.RoleEligible);
    }

    [Fact]
    public void Fully_Clean_Packet_Is_Domain_And_Role_Admissible()
    {
        var service = new GovernedSeedDomainRoleGatingService();
        var packet = CreatePacket(primeMaterialCount: 2);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
            result.GatingAssessment.Disposition);
        Assert.True(result.DomainAssessment.DomainEligible);
        Assert.True(result.RoleAssessment.RoleEligible);
    }

    [Fact]
    public void Cryptic_Only_Disposition_Remains_Cryptic_Only_Carry()
    {
        var service = new GovernedSeedDomainRoleGatingService();
        var packet = CreatePacket(
            primeMaterialCount: 1,
            crypticMaterialCount: 2,
            admissionDisposition: PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly,
            admissionDomainEligible: false,
            admissionRoleEligible: false);

        var result = service.Evaluate(packet);

        Assert.Equal(
            GovernedSeedDomainRoleGatingDisposition.CrypticOnlyCarry,
            result.GatingAssessment.Disposition);
        Assert.False(result.DomainAssessment.DomainEligible);
        Assert.False(result.RoleAssessment.RoleEligible);
    }

    private static GovernedSeedPreDomainGovernancePacket CreatePacket(
        int primeMaterialCount = 2,
        int crypticMaterialCount = 1,
        bool crypticAuthorityBleedDetected = false,
        PrimeSeedPreDomainAdmissionDisposition admissionDisposition = PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
        bool admissionDomainEligible = true,
        bool admissionRoleEligible = true)
    {
        var candidateId = "candidate://domain-role/a";
        var boundaryReceipt = new GovernedSeedCandidateBoundaryReceipt(
            ReceiptHandle: "candidate-boundary://domain-role/a",
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
            Summary: "candidate-only");

        var holdingReceipt = new GovernedSeedCrypticHoldingInspectionReceipt(
            ReceiptHandle: "holding://domain-role/a",
            FormationReceiptHandle: "formation://domain-role/a",
            ListeningFrameHandle: "listening://domain-role/a",
            CompassPacketHandle: "compass://domain-role/a",
            HoldingEntries: [],
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: "holding",
            LawfulBasis: "holding",
            TimestampUtc: DateTimeOffset.UtcNow);

        var formOrCleaveAssessment = new GovernedSeedFormOrCleaveAssessment(
            AssessmentHandle: "form-or-cleave://domain-role/a",
            FormationReceiptHandle: "formation://domain-role/a",
            ListeningFrameHandle: "listening://domain-role/a",
            CompassPacketHandle: "compass://domain-role/a",
            HoldingReceiptHandle: holdingReceipt.ReceiptHandle,
            Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
            CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            DescendantCandidates: [],
            CandidateOnly: true,
            ReasonCode: "formed",
            LawfulBasis: "formed",
            TimestampUtc: DateTimeOffset.UtcNow);

        var separationReceipt = new GovernedSeedCandidateSeparationReceipt(
            ReceiptHandle: "candidate-separation://domain-role/a",
            CandidateId: candidateId,
            SeparationSucceeded: true,
            PrimeMaterialCount: primeMaterialCount,
            CrypticMaterialCount: crypticMaterialCount,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            Summary: "separated");

        var duplexReceipt = new PrimeCrypticDuplexGovernanceReceipt(
            ReceiptHandle: "duplex://domain-role/a",
            CandidateId: candidateId,
            PrimeSurfaceEstablished: primeMaterialCount > 0,
            CrypticSurfaceEstablished: crypticMaterialCount > 0,
            Summary: "duplex");

        var admissionGateReceipt = new PrimeSeedPreDomainAdmissionGateReceipt(
            ReceiptHandle: "admission://domain-role/a",
            CandidateId: candidateId,
            Disposition: admissionDisposition,
            DomainEligible: admissionDomainEligible,
            RoleEligible: admissionRoleEligible,
            Summary: "admission");

        var hostLoopReceipt = new GovernedSeedPreDomainHostLoopReceipt(
            ReceiptHandle: "host-loop://domain-role/a",
            FirstPrimeReceiptHandle: "first-prime://domain-role/a",
            PrimeSeedReceiptHandle: "prime-seed://domain-role/a",
            PreDomainGovernancePacketHandle: "packet://domain-role/a",
            CandidateBoundaryReceiptHandle: boundaryReceipt.ReceiptHandle,
            CrypticHoldingInspectionHandle: holdingReceipt.ReceiptHandle,
            FormOrCleaveAssessmentHandle: formOrCleaveAssessment.AssessmentHandle,
            CandidateSeparationReceiptHandle: separationReceipt.ReceiptHandle,
            DuplexGovernanceReceiptHandle: duplexReceipt.ReceiptHandle,
            AdmissionGateReceiptHandle: admissionGateReceipt.ReceiptHandle,
            CarryDisposition: admissionDisposition == PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly
                ? GovernedSeedCarryDispositionKind.Hold
                : GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            CandidateHandles: [candidateId],
            SeedReady: true,
            CandidateOnly: true,
            DomainAdmissionWithheld: true,
            ActionAuthorityWithheld: true,
            ReasonCode: "pre-domain",
            LawfulBasis: "pre-domain",
            TimestampUtc: DateTimeOffset.UtcNow);

        return new GovernedSeedPreDomainGovernancePacket(
            PacketHandle: "packet://domain-role/a",
            CandidateId: candidateId,
            PrimeSeedStateReceipt: CreatePrimeSeedReceipt(),
            BoundaryReceipt: boundaryReceipt,
            HoldingInspectionReceipt: holdingReceipt,
            FormOrCleaveAssessment: formOrCleaveAssessment,
            SeparationReceipt: separationReceipt,
            DuplexGovernanceReceipt: duplexReceipt,
            AdmissionGateReceipt: admissionGateReceipt,
            HostLoopReceipt: hostLoopReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "packet");
    }

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt() =>
        new(
            ReceiptHandle: "prime-seed://domain-role/a",
            RequestHandle: "prime-seed-request://domain-role/a",
            FirstPrimeReceiptHandle: "first-prime://domain-role/a",
            PrimeRetainedRecordHandle: "prime-retained://domain-role/a",
            StableOneHandle: "stable-one://domain-role/a",
            SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
            SeedSourceHandle: "seed-source://domain-role/a",
            SeedCarrierHandle: "seed-carrier://domain-role/a",
            SeedContinuityHandle: "seed-continuity://domain-role/a",
            SeedIntegrityHandle: "seed-integrity://domain-role/a",
            SeedEvidenceHandles: ["evidence://domain-role/a"],
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
            TimestampUtc: DateTimeOffset.UtcNow);
}
