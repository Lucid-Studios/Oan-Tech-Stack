using San.Common;

namespace SLI.Engine;

public interface IGovernedSeedCandidateSeparationService
{
    GovernedSeedCandidateSeparationResult Separate(
        GovernedSeedCandidateEnvelope candidateEnvelope,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspection,
        GovernedSeedFormOrCleaveAssessment formOrCleaveAssessment);
}

public sealed record GovernedSeedCandidateSeparationResult(
    GovernedSeedPrimeCandidateView PrimeView,
    GovernedSeedCrypticCandidateView CrypticView,
    GovernedSeedCandidateSeparationAssessment Assessment,
    GovernedSeedCandidateSeparationReceipt SeparationReceipt,
    PrimeCrypticDuplexGovernanceReceipt DuplexReceipt);

public sealed class GovernedSeedCandidateSeparationService : IGovernedSeedCandidateSeparationService
{
    public GovernedSeedCandidateSeparationResult Separate(
        GovernedSeedCandidateEnvelope candidateEnvelope,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspection,
        GovernedSeedFormOrCleaveAssessment formOrCleaveAssessment)
    {
        ArgumentNullException.ThrowIfNull(candidateEnvelope);
        ArgumentNullException.ThrowIfNull(primeSeedReceipt);
        ArgumentNullException.ThrowIfNull(holdingInspection);
        ArgumentNullException.ThrowIfNull(formOrCleaveAssessment);

        var primeMaterials = new List<GovernedSeedPrimeMaterial>();
        var crypticMaterials = new List<GovernedSeedCrypticMaterial>();

        if (!string.IsNullOrWhiteSpace(holdingInspection.LawfulBasis))
        {
            crypticMaterials.Add(new GovernedSeedCrypticMaterial(
                GovernedSeedCrypticMaterialKind.HoldWorthyConstruct,
                $"Holding inspection: {holdingInspection.ReasonCode}"));
        }

        foreach (var observation in candidateEnvelope.ResonanceObservations)
        {
            crypticMaterials.Add(new GovernedSeedCrypticMaterial(
                GovernedSeedCrypticMaterialKind.ResonanceGrouping,
                observation.Summary));
        }

        foreach (var proposal in candidateEnvelope.DescendantProposals)
        {
            crypticMaterials.Add(new GovernedSeedCrypticMaterial(
                GovernedSeedCrypticMaterialKind.PartialForm,
                proposal.Summary));
        }

        if (formOrCleaveAssessment.Disposition == GovernedSeedFormOrCleaveDispositionKind.Cleave)
        {
            primeMaterials.Add(new GovernedSeedPrimeMaterial(
                GovernedSeedPrimeMaterialKind.ResponsibilityBearingMarker,
                "Cleave disposition requires accountable descendant handling."));
        }

        if (primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding)
        {
            primeMaterials.Add(new GovernedSeedPrimeMaterial(
                GovernedSeedPrimeMaterialKind.AdmissionRelevantStructure,
                "Prime seed pre-domain standing remains eligible for later gate inspection."));
        }

        var authorityBleedDetected = false;

        var primeView = new GovernedSeedPrimeCandidateView(
            candidateEnvelope.CandidateId,
            primeMaterials);
        var crypticView = new GovernedSeedCrypticCandidateView(
            candidateEnvelope.CandidateId,
            crypticMaterials);

        var assessment = new GovernedSeedCandidateSeparationAssessment(
            candidateEnvelope.CandidateId,
            SeparationSucceeded: true,
            CrypticAuthorityBleedDetected: authorityBleedDetected,
            PrimeMaterialPresent: primeMaterials.Count > 0,
            CrypticMaterialPresent: crypticMaterials.Count > 0,
            Summary: "Candidate separated into Prime and Cryptic governance surfaces.");

        var separationReceipt = new GovernedSeedCandidateSeparationReceipt(
            candidateEnvelope.CandidateId,
            SeparationSucceeded: true,
            PrimeMaterialCount: primeMaterials.Count,
            CrypticMaterialCount: crypticMaterials.Count,
            CrypticAuthorityBleedDetected: authorityBleedDetected,
            Summary: assessment.Summary);

        var duplexReceipt = new PrimeCrypticDuplexGovernanceReceipt(
            candidateEnvelope.CandidateId,
            PrimeSurfaceEstablished: primeMaterials.Count > 0,
            CrypticSurfaceEstablished: crypticMaterials.Count > 0,
            Summary: "Prime/Cryptic duplex governance surfaces established.");

        return new GovernedSeedCandidateSeparationResult(
            primeView,
            crypticView,
            assessment,
            separationReceipt,
            duplexReceipt);
    }
}
