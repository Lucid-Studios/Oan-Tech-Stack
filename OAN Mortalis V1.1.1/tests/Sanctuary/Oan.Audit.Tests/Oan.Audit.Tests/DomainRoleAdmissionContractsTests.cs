namespace San.Audit.Tests;

using San.Common;
using San.Common;

public sealed class DomainRoleAdmissionContractsTests
{
    [Fact]
    public void DomainRoleAdmission_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                DomainRoleAdmissionEligibilityKind.Admissible,
                DomainRoleAdmissionEligibilityKind.Inadmissible,
                DomainRoleAdmissionEligibilityKind.InsufficientInformation
            ],
            Enum.GetValues<DomainRoleAdmissionEligibilityKind>());

        Assert.Equal(
            [
                DomainRoleAdmissionDecisionKind.Accept,
                DomainRoleAdmissionDecisionKind.Defer,
                DomainRoleAdmissionDecisionKind.Refuse
            ],
            Enum.GetValues<DomainRoleAdmissionDecisionKind>());
    }

    [Fact]
    public void Complete_Legal_Offer_Can_Be_Accepted_As_Operational_Jurisdiction()
    {
        var offer = CreateDomainOffer();
        var assessment = DomainRoleAdmissionEvaluator.Assess(
            CreateFirstPrimeReceipt(),
            offer,
            "assessment://domain-role/session-a");

        var record = DomainRoleAdmissionEvaluator.Decide(
            offer,
            assessment,
            DomainRoleAdmissionDecisionKind.Accept,
            "record://domain-role/session-a");

        Assert.Equal(DomainRoleAdmissionEligibilityKind.Admissible, assessment.Eligibility);
        Assert.Equal(DomainRoleAdmissionDecisionKind.Accept, record.Decision);
        Assert.Equal("legal-foundation://lucid/root-family", record.LegalFoundationHandle);
        Assert.Contains("domain://sanctuary/operations", record.AcceptedDomainHandles);
        Assert.Contains("role://governed-cognition/operator-facing", record.AcceptedRoleHandles);
        Assert.False(record.StandingOverwritten);
        Assert.True(record.MotherFatherOriginAuthorityWithheld);
        Assert.True(record.CradleLocalGoverningSurfaceStillWithheld);
        Assert.True(record.ImplicitDomainPromotionRefused);
        Assert.True(record.ReceiptRequiredForAllOutcomes);
    }

    [Fact]
    public void Missing_Legal_Foundation_Defers_Admission_And_Withholds_Acceptance()
    {
        var offer = CreateDomainOffer() with
        {
            LegalFoundationHandle = "",
            LegalPredicateHandles = []
        };
        var assessment = DomainRoleAdmissionEvaluator.Assess(
            CreateFirstPrimeReceipt(),
            offer,
            "assessment://domain-role/missing-foundation");

        var record = DomainRoleAdmissionEvaluator.Decide(
            offer,
            assessment,
            DomainRoleAdmissionDecisionKind.Accept,
            "record://domain-role/missing-foundation");

        Assert.Equal(DomainRoleAdmissionEligibilityKind.InsufficientInformation, assessment.Eligibility);
        Assert.False(assessment.LegalFoundationPresent);
        Assert.Equal(DomainRoleAdmissionDecisionKind.Defer, record.Decision);
        Assert.Empty(record.AcceptedDomainHandles);
        Assert.Contains(
            "domain-role-admission-requested-decision-overridden-by-law",
            record.ConstraintCodes);
    }

    [Fact]
    public void Origin_Authority_Claim_Is_Refused()
    {
        var offer = CreateDomainOffer() with
        {
            OriginAuthorityClaimed = true
        };
        var assessment = DomainRoleAdmissionEvaluator.Assess(
            CreateFirstPrimeReceipt(),
            offer,
            "assessment://domain-role/origin-claim");

        var record = DomainRoleAdmissionEvaluator.Decide(
            offer,
            assessment,
            DomainRoleAdmissionDecisionKind.Accept,
            "record://domain-role/origin-claim");

        Assert.Equal(DomainRoleAdmissionEligibilityKind.Inadmissible, assessment.Eligibility);
        Assert.True(assessment.RequiresUnlawfulIdentityClaim);
        Assert.Equal(DomainRoleAdmissionDecisionKind.Refuse, record.Decision);
        Assert.Equal("domain-role-admission-origin-authority-claim-refused", assessment.ReasonCode);
    }

    [Fact]
    public void Non_FirstPrime_Body_Cannot_Be_Forced_Into_Domain_Acceptance()
    {
        var firstPrime = CreateFirstPrimeReceipt() with
        {
            FirstPrimeState = EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient,
            StableOneSatisfied = false
        };
        var offer = CreateDomainOffer();
        var assessment = DomainRoleAdmissionEvaluator.Assess(
            firstPrime,
            offer,
            "assessment://domain-role/not-first-prime");

        var record = DomainRoleAdmissionEvaluator.Decide(
            offer,
            assessment,
            DomainRoleAdmissionDecisionKind.Accept,
            "record://domain-role/not-first-prime");

        Assert.Equal(DomainRoleAdmissionEligibilityKind.Inadmissible, assessment.Eligibility);
        Assert.Equal(DomainRoleAdmissionDecisionKind.Refuse, record.Decision);
        Assert.Contains(
            "domain-role-admission-first-prime-pre-role-standing-required",
            assessment.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_Domain_And_Role_Admission_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "DOMAIN_AND_ROLE_ADMISSION_LAW.md");
        var ecFirstPrimePath = Path.Combine(lineRoot, "docs", "EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md");
        var legalOrientationPath = Path.Combine(lineRoot, "docs", "SELFGEL_LEGAL_ORIENTATION_PREDICATE_FAMILY_NOTE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var ecFirstPrimeText = File.ReadAllText(ecFirstPrimePath);
        var legalOrientationText = File.ReadAllText(legalOrientationPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("No implicit or silent domain promotion is permitted", lawText, StringComparison.Ordinal);
        Assert.Contains("Domain != Origin != Governance Root", lawText, StringComparison.Ordinal);
        Assert.Contains("DOMAIN_AND_ROLE_ADMISSION_LAW.md", ecFirstPrimeText, StringComparison.Ordinal);
        Assert.Contains("DomainRoleAdmissionContracts.cs", firstRunText, StringComparison.Ordinal);
        Assert.Contains("legal-body-continuity", legalOrientationText, StringComparison.Ordinal);
        Assert.Contains("domain-and-role-admission-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Domain and role admission preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("cradle-local governing surface enactment beyond domain-role admission", refinementText, StringComparison.Ordinal);
    }

    private static DomainOffer CreateDomainOffer()
    {
        return new DomainOffer(
            OfferHandle: "offer://domain-role/session-a",
            SourceAuthorityHandle: "authority://legal-foundation/steward-offer",
            LegalFoundationHandle: "legal-foundation://lucid/root-family",
            LegalPredicateHandles:
            [
                "selfgel.governing-body-seated",
                "selfgel.jurisdiction-seated",
                "selfgel.entity-lineage-valid",
                "selfgel.lawful-operating-surface"
            ],
            CandidateDomainHandles:
            [
                "domain://sanctuary/operations"
            ],
            CandidateRoleHandles:
            [
                "role://governed-cognition/operator-facing"
            ],
            DeclaredIntent: "bounded operational jurisdiction for first Prime pre-role standing",
            AuthorityScopeHandles:
            [
                "scope://observe",
                "scope://recommend",
                "scope://refuse"
            ],
            ExplicitExclusionHandles:
            [
                "exclude://origin-authority",
                "exclude://mother-father-domain-application",
                "exclude://cradle-local-governing-surface-selection"
            ],
            ContinuityBurdenHandles:
            [
                "burden://preserve-prior-receipts",
                "burden://preserve-first-prime-standing"
            ],
            RevocationConditionHandles:
            [
                "revoke://legal-foundation-invalid",
                "revoke://scope-overclaimed"
            ],
            OriginAuthorityClaimed: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 17, 00, 00, TimeSpan.Zero));
    }

    private static EngineeredCognitionFirstPrimeStateReceipt CreateFirstPrimeReceipt()
    {
        return new EngineeredCognitionFirstPrimeStateReceipt(
            ReceiptHandle: "receipt://ec-first-prime/session-a",
            FirstRunReceiptHandle: "receipt://first-run/session-a",
            PrimeRetainedRecordHandle: "record://prime-retained-whole/session-a",
            FirstRunState: FirstRunConstitutionState.FoundationsEstablished,
            FirstPrimeState: EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding,
            LivingAgentiCoreHandle: "agenticore://living/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            SoulFrameHandle: "soulframe://session-a",
            OeHandle: "oe://session-a",
            SelfGelHandle: "selfgel://session-a",
            COeHandle: "coe://session-a",
            CSelfGelHandle: "cselfgel://session-a",
            ZedOfDeltaHandle: "zed://delta/session-a",
            EngineeredCognitionHandle: "ec://session-a",
            ThetaIngressReceiptHandle: "receipt://theta-ingress/session-a",
            PostIngressDiscernmentReceiptHandle: "receipt://post-ingress-discernment/session-a",
            StableOneHandle: "stable-one://thread/session-a",
            RetainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            InstallAndFoundationsReady: true,
            StewardIssuedCradleBraidVisible: true,
            AgentiCoreSensorBodyCast: true,
            ThetaIngressLawful: true,
            StableOneSatisfied: true,
            PrimeRetainedStandingReached: true,
            MotherFatherDomainRoleApplicationWithheld: true,
            CradleLocalGoverningSurfaceWithheld: true,
            PrimeClosureStillWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: ["ec-first-prime-state-pre-role-standing"],
            ReasonCode: "ec-first-prime-state-pre-role-standing",
            LawfulBasis: "first prime pre-role standing test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 16, 55, 00, TimeSpan.Zero));
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
