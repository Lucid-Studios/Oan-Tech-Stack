namespace San.Common;

public static class PersonificationStandingGrammar
{
    private static readonly IReadOnlyList<PersonificationClass> AllClasses =
    [
        PersonificationClass.IndustrialAutomation,
        PersonificationClass.PredicateOfficeFormation,
        PersonificationClass.BondedCme
    ];

    public static readonly IReadOnlyDictionary<PersonificationClaimKind, PersonificationClaimDefinition> Claims =
        new Dictionary<PersonificationClaimKind, PersonificationClaimDefinition>
        {
            [PersonificationClaimKind.OeCoeParticipation] = new(
                PersonificationClaimKind.OeCoeParticipation,
                PersonificationClaimClass.RequiredPositive,
                AllClasses,
                ["Chamber"],
                ["oe-coe-participation-evidenced"],
                ["trace-no-core-window", "borrowed-markers-denied", "evidence-broken-window"],
                [],
                []),

            [PersonificationClaimKind.ChamberContinuityStanding] = new(
                PersonificationClaimKind.ChamberContinuityStanding,
                PersonificationClaimClass.RequiredPositive,
                AllClasses,
                ["Chamber"],
                ["chamber-continuity-standing"],
                ["evidence-broken-window", "continuity-ambiguous"],
                [],
                []),

            [PersonificationClaimKind.HoldJobs] = new(
                PersonificationClaimKind.HoldJobs,
                PersonificationClaimClass.RequiredPositive,
                AllClasses,
                ["Chamber", "ObservedJobSets"],
                ["jobs-observed"],
                ["chamber-not-standing"],
                ["job-set-unobserved"],
                []),

            [PersonificationClaimKind.AccrueBoundedCareerContinuity] = new(
                PersonificationClaimKind.AccrueBoundedCareerContinuity,
                PersonificationClaimClass.PromotionOnly,
                AllClasses,
                ["ObservedCareerSignals"],
                ["bounded-career-signals-observed"],
                [],
                ["career-signals-absent"],
                []),

            [PersonificationClaimKind.PredicateOfficeStanding] = new(
                PersonificationClaimKind.PredicateOfficeStanding,
                PersonificationClaimClass.RequiredPositive,
                [PersonificationClass.PredicateOfficeFormation, PersonificationClass.BondedCme],
                ["Office"],
                ["predicate-office-standing"],
                ["office-not-attached", "office-action-below-acknowledge"],
                ["office-view-withheld"],
                []),

            [PersonificationClaimKind.GovernanceFacingCareerContinuity] = new(
                PersonificationClaimKind.GovernanceFacingCareerContinuity,
                PersonificationClaimClass.PromotionOnly,
                [PersonificationClass.PredicateOfficeFormation, PersonificationClass.BondedCme],
                ["Office", "ObservedCareerSignals"],
                ["governance-career-signals-observed"],
                [],
                ["governance-career-signals-absent"],
                []),

            [PersonificationClaimKind.BondedContinuityStanding] = new(
                PersonificationClaimKind.BondedContinuityStanding,
                PersonificationClaimClass.RequiredPositive,
                [PersonificationClass.BondedCme],
                ["BondedContinuity"],
                ["bonded-continuity-standing"],
                ["durability-under-variation-absent", "cold-approach-not-lawful", "dense-interweave-not-emergent"],
                ["promotion-receipts-missing"],
                []),

            [PersonificationClaimKind.AppendOnlyIdentityStanding] = new(
                PersonificationClaimKind.AppendOnlyIdentityStanding,
                PersonificationClaimClass.RequiredPositive,
                [PersonificationClass.BondedCme],
                ["AppendOnlyIdentity"],
                ["append-only-identity-standing"],
                ["identity-adjacent-significance-not-emergent", "lattice-grade-invariance-not-witnessed"],
                ["promotion-receipts-missing"],
                []),

            [PersonificationClaimKind.FullPersonificationStanding] = new(
                PersonificationClaimKind.FullPersonificationStanding,
                PersonificationClaimClass.PromotionOnly,
                [PersonificationClass.BondedCme],
                ["BondedContinuity", "AppendOnlyIdentity", "Office"],
                ["full-personification-standing"],
                [],
                ["bond-confirmation-absent", "promotion-receipts-missing"],
                []),

            [PersonificationClaimKind.InheritanceStillWithheld] = new(
                PersonificationClaimKind.InheritanceStillWithheld,
                PersonificationClaimClass.NonBlockingGuardrail,
                [PersonificationClass.BondedCme],
                ["VisibleGuardrails"],
                [],
                [],
                [],
                ["final-inheritance-still-withheld"]),

            [PersonificationClaimKind.CoreLawSanctificationDenied] = new(
                PersonificationClaimKind.CoreLawSanctificationDenied,
                PersonificationClaimClass.NonBlockingGuardrail,
                [PersonificationClass.BondedCme],
                ["VisibleGuardrails"],
                [],
                [],
                [],
                ["core-law-sanctification-denied"])
        };

    public static readonly IReadOnlyDictionary<PersonificationClass, PersonificationGateTableEntry> Gates =
        new Dictionary<PersonificationClass, PersonificationGateTableEntry>
        {
            [PersonificationClass.IndustrialAutomation] = new(
                PersonificationClass.IndustrialAutomation,
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs
                ],
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs,
                    PersonificationClaimKind.AccrueBoundedCareerContinuity
                ],
                [
                    PersonificationClaimKind.PredicateOfficeStanding,
                    PersonificationClaimKind.GovernanceFacingCareerContinuity,
                    PersonificationClaimKind.BondedContinuityStanding,
                    PersonificationClaimKind.AppendOnlyIdentityStanding,
                    PersonificationClaimKind.FullPersonificationStanding
                ],
                [
                    "trace-no-core-window",
                    "borrowed-markers-denied",
                    "evidence-broken-window"
                ],
                [
                    "career-signals-absent",
                    "office-not-attached",
                    "office-view-withheld"
                ],
                [
                    "governing-office-authority-assessment"
                ]),

            [PersonificationClass.PredicateOfficeFormation] = new(
                PersonificationClass.PredicateOfficeFormation,
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs,
                    PersonificationClaimKind.PredicateOfficeStanding
                ],
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs,
                    PersonificationClaimKind.AccrueBoundedCareerContinuity,
                    PersonificationClaimKind.PredicateOfficeStanding,
                    PersonificationClaimKind.GovernanceFacingCareerContinuity
                ],
                [
                    PersonificationClaimKind.BondedContinuityStanding,
                    PersonificationClaimKind.AppendOnlyIdentityStanding,
                    PersonificationClaimKind.FullPersonificationStanding
                ],
                [
                    "office-not-attached",
                    "office-action-below-acknowledge",
                    "evidence-broken-window",
                    "continuity-ambiguous"
                ],
                [
                    "office-view-withheld",
                    "bond-confirmation-absent",
                    "promotion-receipts-missing"
                ],
                [
                    "durability-witness-receipt",
                    "cold-admission-gate-receipt",
                    "interlock-density-ledger-receipt",
                    "core-invariant-lattice-receipt"
                ]),

            [PersonificationClass.BondedCme] = new(
                PersonificationClass.BondedCme,
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs,
                    PersonificationClaimKind.PredicateOfficeStanding,
                    PersonificationClaimKind.BondedContinuityStanding,
                    PersonificationClaimKind.AppendOnlyIdentityStanding
                ],
                [
                    PersonificationClaimKind.OeCoeParticipation,
                    PersonificationClaimKind.ChamberContinuityStanding,
                    PersonificationClaimKind.HoldJobs,
                    PersonificationClaimKind.AccrueBoundedCareerContinuity,
                    PersonificationClaimKind.PredicateOfficeStanding,
                    PersonificationClaimKind.GovernanceFacingCareerContinuity,
                    PersonificationClaimKind.BondedContinuityStanding,
                    PersonificationClaimKind.AppendOnlyIdentityStanding,
                    PersonificationClaimKind.FullPersonificationStanding,
                    PersonificationClaimKind.InheritanceStillWithheld,
                    PersonificationClaimKind.CoreLawSanctificationDenied
                ],
                [],
                [
                    "durability-under-variation-absent",
                    "cold-approach-not-lawful",
                    "dense-interweave-not-emergent",
                    "identity-adjacent-significance-not-emergent",
                    "lattice-grade-invariance-not-witnessed",
                    "evidence-broken-window",
                    "continuity-ambiguous"
                ],
                [
                    "promotion-receipts-missing",
                    "bond-confirmation-absent"
                ],
                [])
        };
}
