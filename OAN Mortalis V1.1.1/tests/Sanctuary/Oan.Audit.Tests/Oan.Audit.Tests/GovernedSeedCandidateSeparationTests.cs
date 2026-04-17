using San.Common;
using SLI.Engine;

namespace San.Audit.Tests;

public sealed class GovernedSeedCandidateSeparationTests
{
    [Fact]
    public void Separation_Creates_Duplex_Surfaces_Without_Authority_Bleed()
    {
        var service = new GovernedSeedCandidateSeparationService();
        var result = service.Separate(
            CreateCandidateEnvelope(),
            CreatePrimeSeedReceipt(PrimeSeedStateKind.PrimeSeedPreDomainStanding),
            CreateHoldingInspectionReceipt(),
            CreateFormOrCleaveAssessment(GovernedSeedFormOrCleaveDispositionKind.Cleave));

        Assert.True(result.Assessment.SeparationSucceeded);
        Assert.False(result.Assessment.CrypticAuthorityBleedDetected);
        Assert.True(result.DuplexReceipt.PrimeSurfaceEstablished);
        Assert.True(result.DuplexReceipt.CrypticSurfaceEstablished);
        Assert.Contains(
            result.PrimeView.PrimeMaterials,
            material => material.Kind == GovernedSeedPrimeMaterialKind.ResponsibilityBearingMarker);
        Assert.Contains(
            result.CrypticView.CrypticMaterials,
            material => material.Kind == GovernedSeedCrypticMaterialKind.ResonanceGrouping);
    }

    private static GovernedSeedCandidateEnvelope CreateCandidateEnvelope()
    {
        return new GovernedSeedCandidateEnvelope(
            CandidateId: "candidate://separation/a",
            SourceType: GovernedSeedCandidateSourceType.SyntheticTest,
            SourceChannel: "audit",
            ObservedAtUtc: new DateTimeOffset(2026, 04, 17, 12, 30, 00, TimeSpan.Zero),
            PriorContinuityReference: "continuity://seed/a",
            CandidateProposals: [],
            HoldingMutationProposals: [],
            ResonanceObservations:
            [
                new StubResonanceObservation("obs://1", "resonance", "Resonance cluster retained for inspection.")
            ],
            DescendantProposals:
            [
                new StubDescendantProposal("desc://1", "partial-form", "Partial descendant remains candidate-only.")
            ],
            CollapseSuggestions: []);
    }

    private static GovernedSeedCrypticHoldingInspectionReceipt CreateHoldingInspectionReceipt()
    {
        return new GovernedSeedCrypticHoldingInspectionReceipt(
            ReceiptHandle: "holding://separation/a",
            FormationReceiptHandle: "formation://separation/a",
            ListeningFrameHandle: "listening://separation/a",
            CompassPacketHandle: "compass://separation/a",
            HoldingEntries:
            [
                new GovernedSeedCrypticHoldingEntry(
                    EntryHandle: "entry://separation/a",
                    PsyPolarity: GovernedSeedPsyPolarityKind.Positive,
                    Construct: new GovernedSeedPreAdmissibleConstruct(
                        ConstructHandle: "construct://separation/a",
                        ConstructKind: "unfinished-thought",
                        Summary: "Candidate remains unfinished.",
                        EvidenceHandles: ["evidence://separation/a"],
                        CandidateOnly: true),
                    HoldingReason: "awaits-checkpoint",
                    InspectionInfluenceOnly: true,
                    TimestampUtc: DateTimeOffset.UtcNow)
            ],
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: "holding-retained",
            LawfulBasis: "candidate-only holding retained for inspection.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedFormOrCleaveAssessment CreateFormOrCleaveAssessment(
        GovernedSeedFormOrCleaveDispositionKind disposition)
    {
        return new GovernedSeedFormOrCleaveAssessment(
            AssessmentHandle: "form-or-cleave://separation/a",
            FormationReceiptHandle: "formation://separation/a",
            ListeningFrameHandle: "listening://separation/a",
            CompassPacketHandle: "compass://separation/a",
            HoldingReceiptHandle: "holding://separation/a",
            Disposition: disposition,
            CarryDisposition: GovernedSeedCarryDispositionKind.Cleave,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.Cleave,
            DescendantCandidates: [],
            CandidateOnly: true,
            ReasonCode: "cleave",
            LawfulBasis: "candidate requires accountable descendant handling.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt(PrimeSeedStateKind state)
    {
        return new PrimeSeedStateReceipt(
            ReceiptHandle: "prime-seed://separation/a",
            RequestHandle: "prime-seed-request://separation/a",
            FirstPrimeReceiptHandle: "first-prime://separation/a",
            PrimeRetainedRecordHandle: "prime-retained://separation/a",
            StableOneHandle: "stable-one://separation/a",
            SeedState: state,
            SeedSourceHandle: "seed-source://separation/a",
            SeedCarrierHandle: "seed-carrier://separation/a",
            SeedContinuityHandle: "seed-continuity://separation/a",
            SeedIntegrityHandle: "seed-integrity://separation/a",
            SeedEvidenceHandles: ["evidence://separation/a"],
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
            LawfulBasis: "pre-domain standing remains candidate-only.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private sealed record StubResonanceObservation(
        string ObservationId,
        string ObservationKind,
        string Summary) : IGovernedSeedResonanceObservation;

    private sealed record StubDescendantProposal(
        string DescendantId,
        string DescendantKind,
        string Summary) : IGovernedSeedDescendantProposal;
}
