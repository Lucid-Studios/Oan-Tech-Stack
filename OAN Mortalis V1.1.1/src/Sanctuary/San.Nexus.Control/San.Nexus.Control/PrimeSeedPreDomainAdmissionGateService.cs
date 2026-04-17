using San.Common;

namespace San.Nexus.Control;

public interface IPrimeSeedPreDomainAdmissionGateService
{
    PrimeSeedPreDomainAdmissionGateResult Evaluate(
        GovernedSeedPrimeCandidateView primeView,
        GovernedSeedCrypticCandidateView crypticView,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedRevalidationContext revalidationContext);
}

public sealed record PrimeSeedPreDomainAdmissionGateResult(
    PrimeSeedPreDomainAdmissionAssessment AdmissionAssessment,
    DomainRoleEligibilityAssessment EligibilityAssessment,
    PrimeSeedPreDomainAdmissionGateReceipt Receipt);

public sealed class PrimeSeedPreDomainAdmissionGateService : IPrimeSeedPreDomainAdmissionGateService
{
    public PrimeSeedPreDomainAdmissionGateResult Evaluate(
        GovernedSeedPrimeCandidateView primeView,
        GovernedSeedCrypticCandidateView crypticView,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedRevalidationContext revalidationContext)
    {
        ArgumentNullException.ThrowIfNull(primeView);
        ArgumentNullException.ThrowIfNull(crypticView);
        ArgumentNullException.ThrowIfNull(primeSeedReceipt);
        ArgumentNullException.ThrowIfNull(revalidationContext);

        var primeComplianceIntact = primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var crypticAuthorityBleedDetected = false;
        var responsibilityAttributable = primeView.PrimeMaterials.Count > 0;

        var disposition = PrimeSeedPreDomainAdmissionDisposition.RemainPreDomain;
        var domainEligible = false;
        var roleEligible = false;

        if (!primeComplianceIntact || crypticAuthorityBleedDetected || !revalidationContext.RevalidationSatisfied)
        {
            disposition = PrimeSeedPreDomainAdmissionDisposition.Refuse;
        }
        else if (primeView.PrimeMaterials.Count > 0)
        {
            disposition = PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate;
            domainEligible = true;
            roleEligible = true;
        }
        else if (crypticView.CrypticMaterials.Count > 0)
        {
            disposition = PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly;
        }

        var admissionAssessment = new PrimeSeedPreDomainAdmissionAssessment(
            primeView.CandidateId,
            disposition,
            primeComplianceIntact,
            crypticAuthorityBleedDetected,
            responsibilityAttributable,
            "Pre-domain admission gate evaluated separated candidate surfaces.");

        var eligibilityAssessment = new DomainRoleEligibilityAssessment(
            primeView.CandidateId,
            domainEligible,
            roleEligible,
            domainEligible && roleEligible
                ? "Candidate may approach domain/role gating."
                : "Candidate is not yet eligible for domain/role gating.");

        var receipt = new PrimeSeedPreDomainAdmissionGateReceipt(
            primeView.CandidateId,
            disposition,
            domainEligible,
            roleEligible,
            "Pre-domain admission disposition emitted.");

        return new PrimeSeedPreDomainAdmissionGateResult(
            admissionAssessment,
            eligibilityAssessment,
            receipt);
    }
}
