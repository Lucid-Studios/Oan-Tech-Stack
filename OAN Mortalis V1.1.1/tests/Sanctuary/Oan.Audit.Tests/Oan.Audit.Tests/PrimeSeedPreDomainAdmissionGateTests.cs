using San.Common;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class PrimeSeedPreDomainAdmissionGateTests
{
    [Fact]
    public void NonStanding_Prime_Surface_Is_Refused()
    {
        var service = new PrimeSeedPreDomainAdmissionGateService();

        var result = service.Evaluate(
            new GovernedSeedPrimeCandidateView(
                "candidate://gate/a",
                [new GovernedSeedPrimeMaterial(GovernedSeedPrimeMaterialKind.InvariantConcern, "Prime concern")]),
            new GovernedSeedCrypticCandidateView("candidate://gate/a", []),
            CreatePrimeSeedReceipt(PrimeSeedStateKind.SeedMaterialIncomplete),
            CreateRevalidationContext(satisfied: true));

        Assert.Equal(PrimeSeedPreDomainAdmissionDisposition.Refuse, result.AdmissionAssessment.Disposition);
        Assert.False(result.AdmissionAssessment.PrimeComplianceIntact);
        Assert.False(result.EligibilityAssessment.DomainEligible);
    }

    [Fact]
    public void CrypticRich_But_AuthorityEmpty_Candidate_Is_Carried_Not_Promoted()
    {
        var service = new PrimeSeedPreDomainAdmissionGateService();

        var result = service.Evaluate(
            new GovernedSeedPrimeCandidateView("candidate://gate/b", []),
            new GovernedSeedCrypticCandidateView(
                "candidate://gate/b",
                [new GovernedSeedCrypticMaterial(GovernedSeedCrypticMaterialKind.UnfinishedThought, "Still forming")]),
            CreatePrimeSeedReceipt(PrimeSeedStateKind.PrimeSeedPreDomainStanding),
            CreateRevalidationContext(satisfied: true));

        Assert.Equal(PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly, result.AdmissionAssessment.Disposition);
        Assert.False(result.EligibilityAssessment.DomainEligible);
        Assert.False(result.EligibilityAssessment.RoleEligible);
    }

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt(PrimeSeedStateKind state)
    {
        return new PrimeSeedStateReceipt(
            ReceiptHandle: "prime-seed://gate",
            RequestHandle: "prime-seed-request://gate",
            FirstPrimeReceiptHandle: "first-prime://gate",
            PrimeRetainedRecordHandle: "prime-retained://gate",
            StableOneHandle: "stable-one://gate",
            SeedState: state,
            SeedSourceHandle: "seed-source://gate",
            SeedCarrierHandle: "seed-carrier://gate",
            SeedContinuityHandle: "seed-continuity://gate",
            SeedIntegrityHandle: "seed-integrity://gate",
            SeedEvidenceHandles: ["evidence://gate"],
            FirstPrimePreRoleStandingPresent: true,
            StableOnePresent: true,
            PrimeRetainedStandingPresent: true,
            SeedSourcePresent: true,
            SeedCarrierPresent: true,
            SeedContinuityPresent: state == PrimeSeedStateKind.PrimeSeedPreDomainStanding,
            SeedIntegrityPresent: state == PrimeSeedStateKind.PrimeSeedPreDomainStanding,
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
            ReasonCode: "audit",
            LawfulBasis: "audit",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedRevalidationContext CreateRevalidationContext(bool satisfied)
    {
        return new GovernedSeedRevalidationContext(
            ContextHandle: "revalidation://gate",
            ContextProfile: "pre-domain",
            RevalidationSatisfied: satisfied,
            Summary: satisfied ? "Revalidation satisfied." : "Revalidation not satisfied.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
