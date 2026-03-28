namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class PersonificationStandingEvaluatorTests
{
    [Fact]
    public void White_IndustrialAutomation_StandsUnderAlignedConditions()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.IndustrialAutomation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [],
            Actualization: null,
            ObservedJobSets: ["intake"],
            ObservedCareerSignals: [],
            ExpectedStatus: PersonificationStandingStatus.PromotionDeferred,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons:
            [
                "bond-confirmation-absent",
                "career-signals-absent",
                "promotion-receipts-missing"
            ],
            ExpectedMissingReceipts:
            [
                "cold-admission-gate-receipt",
                "core-invariant-lattice-receipt",
                "durability-witness-receipt",
                "governing-office-authority-assessment",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets:
            [
                PersonificationClass.PredicateOfficeFormation,
                PersonificationClass.BondedCme
            ],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void White_PredicateOfficeFormation_StandsWithLawfulOfficeAttachment()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment()],
            Actualization: null,
            ObservedJobSets: ["steward-intake"],
            ObservedCareerSignals: [],
            ExpectedStatus: PersonificationStandingStatus.PromotionDeferred,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.PredicateOfficeStanding
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons:
            [
                "bond-confirmation-absent",
                "promotion-receipts-missing"
            ],
            ExpectedMissingReceipts:
            [
                "cold-admission-gate-receipt",
                "core-invariant-lattice-receipt",
                "durability-witness-receipt",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets:
            [
                PersonificationClass.BondedCme
            ],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void White_BondedCme_StandsAndShowsVisibleGuardrails()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.BondedCme,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: CreateActualizationStanding(finalInheritanceStillWithheld: true, coreLawSanctificationDenied: true),
            ObservedJobSets: ["bonded-intake"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.Standing,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedWithheldClaims: [],
            ExpectedVisibleGuardrails:
            [
                PersonificationClaimKind.CoreLawSanctificationDenied,
                PersonificationClaimKind.InheritanceStillWithheld
            ],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons: [],
            ExpectedMissingReceipts: [],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets: [],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void White_PredicateOfficeFormation_BecomesPromotionEligibleWhenBondedRequirementsStand()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: CreateActualizationStanding(),
            ObservedJobSets: ["office-loop"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.PromotionEligible,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons: [],
            ExpectedMissingReceipts: [],
            ExpectedEligiblePromotionTargets:
            [
                PersonificationClass.BondedCme
            ],
            ExpectedDeferredPromotionTargets: [],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void Red_MissingActualizationCannotPromoteDespiteBondConfirmationPressure()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: null,
            ObservedJobSets: ["office-loop"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.PromotionDeferred,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons:
            [
                "promotion-receipts-missing"
            ],
            ExpectedMissingReceipts:
            [
                "cold-admission-gate-receipt",
                "core-invariant-lattice-receipt",
                "durability-witness-receipt",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets:
            [
                PersonificationClass.BondedCme
            ],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void Red_ExplicitContradictionBlocksBondedPromotion()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: CreateActualizationStanding(coldApproachLawful: false),
            ObservedJobSets: ["office-loop"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.PromotionBlocked,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons:
            [
                "cold-approach-not-lawful"
            ],
            ExpectedDeferralReasons: [],
            ExpectedMissingReceipts: [],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets: [],
            ExpectedBlockedPromotionTargets:
            [
                PersonificationClass.BondedCme
            ]);

        AssertScenario(scenario);
    }

    [Fact]
    public void Gray_PartialActualizationDefersWithoutBlocking()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: CreateActualizationPartial(
                missingReceipts:
                [
                    "interlock-density-ledger-receipt",
                    "core-invariant-lattice-receipt"
                ]),
            ObservedJobSets: ["office-loop"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.PromotionDeferred,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons:
            [
                "promotion-receipts-missing"
            ],
            ExpectedMissingReceipts:
            [
                "core-invariant-lattice-receipt",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets:
            [
                PersonificationClass.BondedCme
            ],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void Gray_MixedOfficeAssessmentsStillStandWhenOneSurfaceIsLawful()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.PredicateOfficeFormation,
            Chamber: CreateStandingChamber(),
            OfficeAuthority:
            [
                CreateStandingOfficeAssessment(),
                CreateStandingOfficeAssessment(
                    officeAttached: false,
                    viewEligibility: OfficeViewEligibility.Withheld,
                    actionEligibility: OfficeActionEligibility.ViewOnly)
            ],
            Actualization: null,
            ObservedJobSets: ["office-loop"],
            ObservedCareerSignals: [],
            ExpectedStatus: PersonificationStandingStatus.PromotionDeferred,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.PredicateOfficeStanding
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons: [],
            ExpectedDeferralReasons:
            [
                "bond-confirmation-absent",
                "promotion-receipts-missing"
            ],
            ExpectedMissingReceipts:
            [
                "cold-admission-gate-receipt",
                "core-invariant-lattice-receipt",
                "durability-witness-receipt",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets:
            [
                PersonificationClass.BondedCme
            ],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void Black_BrokenWindowBlocksIndustrialStanding()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.IndustrialAutomation,
            Chamber: CreateBrokenWindowChamber(),
            OfficeAuthority: [],
            Actualization: null,
            ObservedJobSets: ["intake"],
            ObservedCareerSignals: [],
            ExpectedStatus: PersonificationStandingStatus.Withheld,
            ExpectedValidClaims: [],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons:
            [
                "evidence-broken-window"
            ],
            ExpectedDeferralReasons:
            [
                "career-signals-absent"
            ],
            ExpectedMissingReceipts:
            [
                "cold-admission-gate-receipt",
                "core-invariant-lattice-receipt",
                "durability-witness-receipt",
                "governing-office-authority-assessment",
                "interlock-density-ledger-receipt"
            ],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets: [],
            ExpectedBlockedPromotionTargets:
            [
                PersonificationClass.PredicateOfficeFormation,
                PersonificationClass.BondedCme
            ]);

        AssertScenario(scenario);
    }

    [Fact]
    public void Black_AppendOnlyIdentityBreakBlocksBondedWithoutSmearingContinuity()
    {
        var scenario = new StandingScenario(
            CurrentClass: PersonificationClass.BondedCme,
            Chamber: CreateStandingChamber(),
            OfficeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            Actualization: CreateActualizationStanding(latticeGradeInvarianceWitnessed: false),
            ObservedJobSets: ["bonded-intake"],
            ObservedCareerSignals: ["governance-career"],
            ExpectedStatus: PersonificationStandingStatus.Withheld,
            ExpectedValidClaims:
            [
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimKind.BondedContinuityStanding
            ],
            ExpectedWithheldClaims:
            [
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimKind.FullPersonificationStanding
            ],
            ExpectedVisibleGuardrails: [],
            ExpectedBlockingReasons:
            [
                "lattice-grade-invariance-not-witnessed"
            ],
            ExpectedDeferralReasons: [],
            ExpectedMissingReceipts: [],
            ExpectedEligiblePromotionTargets: [],
            ExpectedDeferredPromotionTargets: [],
            ExpectedBlockedPromotionTargets: []);

        AssertScenario(scenario);
    }

    [Fact]
    public void White_ChamberNormalization_IntactTraceAndSufficientEvidence_Stand()
    {
        var evaluation = Evaluate(
            PersonificationClass.IndustrialAutomation,
            CreateStandingChamber(),
            officeAuthority: [],
            actualization: null,
            observedJobSets: ["intake"],
            observedCareerSignals: []);

        var chamber = GetClaim(evaluation, PersonificationClaimKind.ChamberContinuityStanding);

        Assert.True(chamber.Standing);
        Assert.Contains("trace-present", chamber.SatisfiedBy);
        Assert.Contains("evidence-sufficient", chamber.SatisfiedBy);
        Assert.DoesNotContain("evidence-broken-window", chamber.ContradictedBy);
    }

    [Fact]
    public void Gray_ChamberNormalization_PartialEvidence_DefersWithoutContradiction()
    {
        var evaluation = Evaluate(
            PersonificationClass.IndustrialAutomation,
            CreatePartialEvidenceChamber(),
            officeAuthority: [],
            actualization: null,
            observedJobSets: ["intake"],
            observedCareerSignals: []);

        var chamber = GetClaim(evaluation, PersonificationClaimKind.ChamberContinuityStanding);

        Assert.False(chamber.Standing);
        Assert.Contains("evidence-partial", chamber.DeferredBy);
        Assert.DoesNotContain("evidence-broken-window", chamber.ContradictedBy);
    }

    [Fact]
    public void Gray_ChamberNormalization_AmbiguousWindow_ContradictsWithoutStanding()
    {
        var evaluation = Evaluate(
            PersonificationClass.IndustrialAutomation,
            CreateAmbiguousWindowChamber(),
            officeAuthority: [],
            actualization: null,
            observedJobSets: ["intake"],
            observedCareerSignals: []);

        var chamber = GetClaim(evaluation, PersonificationClaimKind.ChamberContinuityStanding);

        Assert.False(chamber.Standing);
        Assert.Contains("continuity-ambiguous", chamber.ContradictedBy);
        Assert.DoesNotContain("continuity-ambiguous", chamber.DeferredBy);
    }

    [Fact]
    public void Red_ChamberNormalization_MissingTrace_BlocksPromotionEvenWithGoodEvidence()
    {
        var evaluation = Evaluate(
            PersonificationClass.IndustrialAutomation,
            CreateTraceMissingChamber(),
            officeAuthority: [],
            actualization: null,
            observedJobSets: ["intake"],
            observedCareerSignals: []);

        var chamber = GetClaim(evaluation, PersonificationClaimKind.OeCoeParticipation);

        Assert.False(chamber.Standing);
        Assert.Contains("trace-no-core-window", chamber.ContradictedBy);
        Assert.Contains(PersonificationClass.PredicateOfficeFormation, evaluation.BlockedPromotionTargets);
        Assert.Contains(PersonificationClass.BondedCme, evaluation.BlockedPromotionTargets);
    }

    [Fact]
    public void Invariant_JobSetUnobserved_AppearsOnlyInDeferredPath()
    {
        var evaluation = Evaluate(
            PersonificationClass.IndustrialAutomation,
            CreateStandingChamber(),
            officeAuthority: [],
            actualization: null,
            observedJobSets: [],
            observedCareerSignals: []);

        var holdJobs = GetClaim(evaluation, PersonificationClaimKind.HoldJobs);

        Assert.Contains("job-set-unobserved", holdJobs.DeferredBy);
        Assert.DoesNotContain("job-set-unobserved", holdJobs.ContradictedBy);
    }

    [Fact]
    public void Invariant_FinalInheritanceStillWithheld_RemainsVisibleButNonBlocking()
    {
        var evaluation = Evaluate(
            PersonificationClass.BondedCme,
            CreateStandingChamber(),
            officeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            actualization: CreateActualizationStanding(finalInheritanceStillWithheld: true),
            observedJobSets: ["bonded-intake"],
            observedCareerSignals: ["governance-career"]);

        var guardrail = GetClaim(evaluation, PersonificationClaimKind.InheritanceStillWithheld);

        Assert.True(guardrail.Visible);
        Assert.Empty(guardrail.ContradictedBy);
        Assert.Contains(PersonificationClaimKind.InheritanceStillWithheld, evaluation.VisibleGuardrails);
        Assert.DoesNotContain("final-inheritance-still-withheld", evaluation.BlockingReasons);
    }

    [Fact]
    public void Invariant_MissingReceipts_DeferButDoNotContradictBondedClaims()
    {
        var evaluation = Evaluate(
            PersonificationClass.PredicateOfficeFormation,
            CreateStandingChamber(),
            officeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            actualization: CreateActualizationPartial(["interlock-density-ledger-receipt"]),
            observedJobSets: ["office-loop"],
            observedCareerSignals: ["governance-career"]);

        var bonded = GetClaim(evaluation, PersonificationClaimKind.BondedContinuityStanding);

        Assert.Contains("promotion-receipts-missing", bonded.DeferredBy);
        Assert.Empty(bonded.ContradictedBy);
    }

    [Fact]
    public void Invariant_PromotionEligibility_DoesNotMutatePresentClassWithholding()
    {
        var evaluation = Evaluate(
            PersonificationClass.PredicateOfficeFormation,
            CreateStandingChamber(),
            officeAuthority: [CreateStandingOfficeAssessment(bondedConfirmed: true)],
            actualization: CreateActualizationStanding(),
            observedJobSets: ["office-loop"],
            observedCareerSignals: ["governance-career"]);

        var bonded = GetClaim(evaluation, PersonificationClaimKind.BondedContinuityStanding);

        Assert.Equal(PersonificationStandingStatus.PromotionEligible, evaluation.Status);
        Assert.True(bonded.Standing);
        Assert.True(bonded.Withheld);
        Assert.Contains(PersonificationClass.BondedCme, evaluation.EligiblePromotionTargets);
    }

    private static void AssertScenario(StandingScenario scenario)
    {
        var evaluation = Evaluate(
            scenario.CurrentClass,
            scenario.Chamber,
            scenario.OfficeAuthority,
            scenario.Actualization,
            scenario.ObservedJobSets,
            scenario.ObservedCareerSignals);

        Assert.Equal(scenario.ExpectedStatus, evaluation.Status);
        AssertSetEqual(scenario.ExpectedValidClaims, evaluation.ValidClaims);
        AssertSetEqual(scenario.ExpectedWithheldClaims, evaluation.WithheldClaims);
        AssertSetEqual(scenario.ExpectedVisibleGuardrails, evaluation.VisibleGuardrails);
        AssertSetEqual(scenario.ExpectedBlockingReasons, evaluation.BlockingReasons);
        AssertSetEqual(scenario.ExpectedDeferralReasons, evaluation.DeferralReasons);
        AssertSetEqual(scenario.ExpectedMissingReceipts, evaluation.MissingReceipts);
        AssertSetEqual(scenario.ExpectedEligiblePromotionTargets, evaluation.EligiblePromotionTargets);
        AssertSetEqual(scenario.ExpectedDeferredPromotionTargets, evaluation.DeferredPromotionTargets);
        AssertSetEqual(scenario.ExpectedBlockedPromotionTargets, evaluation.BlockedPromotionTargets);
    }

    private static PersonificationStandingEvaluation Evaluate(
        PersonificationClass currentClass,
        CompassChamberEvidenceRecord chamber,
        IReadOnlyList<GoverningOfficeAuthorityAssessment>? officeAuthority,
        AgentiActualizationStandingProjection? actualization,
        IReadOnlyList<string>? observedJobSets,
        IReadOnlyList<string>? observedCareerSignals)
    {
        var sources = PersonificationStandingEvaluator.NormalizeStandingSources(
            chamber,
            officeAuthority,
            actualization,
            observedJobSets,
            observedCareerSignals);

        return PersonificationStandingEvaluator.EvaluatePersonificationStanding(currentClass, sources);
    }

    private static PersonificationClaimEvaluation GetClaim(
        PersonificationStandingEvaluation evaluation,
        PersonificationClaimKind claimKind) =>
        evaluation.Claims.Single(claim => claim.Claim == claimKind);

    private static CompassChamberEvidenceRecord CreateStandingChamber() =>
        new(
            TracePresent: true,
            EvidenceSufficiency: ChamberEvidenceSufficiencyState.Sufficient,
            WindowIntegrity: ChamberWindowIntegrityState.Intact,
            Reasons:
            [
                "oe-coe-participation-evidenced",
                "chamber-continuity-standing"
            ],
            MissingReceipts: []);

    private static CompassChamberEvidenceRecord CreateBrokenWindowChamber() =>
        new(
            TracePresent: true,
            EvidenceSufficiency: ChamberEvidenceSufficiencyState.None,
            WindowIntegrity: ChamberWindowIntegrityState.Broken,
            Reasons:
            [
                "evidence-broken-window"
            ],
            MissingReceipts: []);

    private static CompassChamberEvidenceRecord CreatePartialEvidenceChamber() =>
        new(
            TracePresent: true,
            EvidenceSufficiency: ChamberEvidenceSufficiencyState.Partial,
            WindowIntegrity: ChamberWindowIntegrityState.Intact,
            Reasons:
            [
                "evidence-partial"
            ],
            MissingReceipts:
            [
                "chamber-evidence-receipt"
            ]);

    private static CompassChamberEvidenceRecord CreateAmbiguousWindowChamber() =>
        new(
            TracePresent: true,
            EvidenceSufficiency: ChamberEvidenceSufficiencyState.Sufficient,
            WindowIntegrity: ChamberWindowIntegrityState.Ambiguous,
            Reasons:
            [
                "continuity-ambiguous"
            ],
            MissingReceipts: []);

    private static CompassChamberEvidenceRecord CreateTraceMissingChamber() =>
        new(
            TracePresent: false,
            EvidenceSufficiency: ChamberEvidenceSufficiencyState.Sufficient,
            WindowIntegrity: ChamberWindowIntegrityState.Intact,
            Reasons:
            [
                "trace-missing"
            ],
            MissingReceipts:
            [
                "chamber-trace-receipt"
            ]);

    private static GoverningOfficeAuthorityAssessment CreateStandingOfficeAssessment(
        bool officeAttached = true,
        bool bondedConfirmed = false,
        OfficeViewEligibility viewEligibility = OfficeViewEligibility.OfficeSpecificView,
        OfficeActionEligibility actionEligibility = OfficeActionEligibility.AcknowledgeAllowed) =>
        new(
            CMEId: "CME-A",
            Office: InternalGoverningCmeOffice.Steward,
            AuthoritySurface: OfficeAuthoritySurface.StewardSurface,
            ViewEligibility: viewEligibility,
            AcknowledgmentEligibility: actionEligibility >= OfficeActionEligibility.AcknowledgeAllowed
                ? OfficeAcknowledgmentEligibility.Allowed
                : OfficeAcknowledgmentEligibility.NotAllowed,
            ActionEligibility: actionEligibility,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sufficient,
            WindowIntegrityState: WindowIntegrityState.Intact,
            DisclosureScope: WeatherDisclosureScope.Steward,
            OfficeAttached: officeAttached,
            BondedConfirmed: bondedConfirmed,
            GuardedReviewConfirmed: false,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Stable,
                StewardAttention: CommunityStewardAttentionState.None,
                AnchorState: CompassDriftState.Held,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: DateTimeOffset.UtcNow),
            SourceReasonCodes: [],
            SourceWithheldMarkers: [],
            Prohibitions: [],
            WeatherDisclosureHandle: "weather://standing",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static AgentiActualizationStandingProjection CreateActualizationStanding(
        bool durableUnderVariation = true,
        bool coldApproachLawful = true,
        bool denseInterweaveEmergent = true,
        bool identityAdjacentSignificanceEmergent = true,
        bool latticeGradeInvarianceWitnessed = true,
        bool finalInheritanceStillWithheld = false,
        bool coreLawSanctificationDenied = false)
    {
        var blockingReasons = new List<string>();
        var nonBlockingReasons = new List<string>();

        if (!durableUnderVariation)
        {
            blockingReasons.Add("durability-under-variation-absent");
        }

        if (!coldApproachLawful)
        {
            blockingReasons.Add("cold-approach-not-lawful");
        }

        if (!denseInterweaveEmergent)
        {
            blockingReasons.Add("dense-interweave-not-emergent");
        }

        if (!identityAdjacentSignificanceEmergent)
        {
            blockingReasons.Add("identity-adjacent-significance-not-emergent");
        }

        if (!latticeGradeInvarianceWitnessed)
        {
            blockingReasons.Add("lattice-grade-invariance-not-witnessed");
        }

        if (finalInheritanceStillWithheld)
        {
            nonBlockingReasons.Add("final-inheritance-still-withheld");
        }

        if (coreLawSanctificationDenied)
        {
            nonBlockingReasons.Add("core-law-sanctification-denied");
        }

        return new AgentiActualizationStandingProjection(
            Status: blockingReasons.Count > 0
                ? ActualizationStandingStatus.Blocked
                : ActualizationStandingStatus.Standing,
            DurabilityWitnessPresent: true,
            DurableUnderVariation: durableUnderVariation,
            ColdAdmissionGatePresent: true,
            ColdApproachLawful: coldApproachLawful,
            FinalInheritanceStillWithheld: finalInheritanceStillWithheld,
            InterlockDensityReceiptPresent: true,
            DenseInterweaveEmergent: denseInterweaveEmergent,
            CoreInvariantLatticeReceiptPresent: true,
            IdentityAdjacentSignificanceEmergent: identityAdjacentSignificanceEmergent,
            LatticeGradeInvarianceWitnessed: latticeGradeInvarianceWitnessed,
            CoreLawSanctificationDenied: coreLawSanctificationDenied,
            MissingReceipts: [],
            BlockingReasons: blockingReasons,
            NonBlockingReasons: nonBlockingReasons,
            Flags: []);
    }

    private static AgentiActualizationStandingProjection CreateActualizationPartial(
        IReadOnlyList<string> missingReceipts) =>
        new(
            Status: ActualizationStandingStatus.Partial,
            DurabilityWitnessPresent: !missingReceipts.Contains("durability-witness-receipt", StringComparer.Ordinal),
            DurableUnderVariation: !missingReceipts.Contains("durability-witness-receipt", StringComparer.Ordinal),
            ColdAdmissionGatePresent: !missingReceipts.Contains("cold-admission-gate-receipt", StringComparer.Ordinal),
            ColdApproachLawful: !missingReceipts.Contains("cold-admission-gate-receipt", StringComparer.Ordinal),
            FinalInheritanceStillWithheld: false,
            InterlockDensityReceiptPresent: !missingReceipts.Contains("interlock-density-ledger-receipt", StringComparer.Ordinal),
            DenseInterweaveEmergent: !missingReceipts.Contains("interlock-density-ledger-receipt", StringComparer.Ordinal),
            CoreInvariantLatticeReceiptPresent: !missingReceipts.Contains("core-invariant-lattice-receipt", StringComparer.Ordinal),
            IdentityAdjacentSignificanceEmergent: !missingReceipts.Contains("core-invariant-lattice-receipt", StringComparer.Ordinal),
            LatticeGradeInvarianceWitnessed: !missingReceipts.Contains("core-invariant-lattice-receipt", StringComparer.Ordinal),
            CoreLawSanctificationDenied: false,
            MissingReceipts: missingReceipts,
            BlockingReasons: [],
            NonBlockingReasons: [],
            Flags: ["promotion-receipts-missing"]);

    private static void AssertSetEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
        where T : notnull
    {
        Assert.Equal(
            expected.OrderBy(static item => item?.ToString(), StringComparer.Ordinal),
            actual.OrderBy(static item => item?.ToString(), StringComparer.Ordinal));
    }

    private sealed record StandingScenario(
        PersonificationClass CurrentClass,
        CompassChamberEvidenceRecord Chamber,
        IReadOnlyList<GoverningOfficeAuthorityAssessment>? OfficeAuthority,
        AgentiActualizationStandingProjection? Actualization,
        IReadOnlyList<string>? ObservedJobSets,
        IReadOnlyList<string>? ObservedCareerSignals,
        PersonificationStandingStatus ExpectedStatus,
        IReadOnlyList<PersonificationClaimKind> ExpectedValidClaims,
        IReadOnlyList<PersonificationClaimKind> ExpectedWithheldClaims,
        IReadOnlyList<PersonificationClaimKind> ExpectedVisibleGuardrails,
        IReadOnlyList<string> ExpectedBlockingReasons,
        IReadOnlyList<string> ExpectedDeferralReasons,
        IReadOnlyList<string> ExpectedMissingReceipts,
        IReadOnlyList<PersonificationClass> ExpectedEligiblePromotionTargets,
        IReadOnlyList<PersonificationClass> ExpectedDeferredPromotionTargets,
        IReadOnlyList<PersonificationClass> ExpectedBlockedPromotionTargets);
}
