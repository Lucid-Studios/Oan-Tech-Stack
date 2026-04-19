namespace San.Audit.Tests;

using San.Common;

public sealed class CmeMinimumLegalFoundingBundleContractsTests
{
    [Fact]
    public void CmeMinimumLegalFoundingBundle_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                CmeFoundingBundlePillarKind.OriginAuthorizationFoundation,
                CmeFoundingBundlePillarKind.IdentityFormationRecord,
                CmeFoundingBundlePillarKind.FirstPrimeStandingProof,
                CmeFoundingBundlePillarKind.DomainRoleAdmissionRecord,
                CmeFoundingBundlePillarKind.OperationalProvenanceCustody
            ],
            Enum.GetValues<CmeFoundingBundlePillarKind>());

        Assert.Equal(
            [
                CmeMinimumLegalFoundingBundleKind.BundleIncomplete,
                CmeMinimumLegalFoundingBundleKind.BundleDeferred,
                CmeMinimumLegalFoundingBundleKind.BundleRefused,
                CmeMinimumLegalFoundingBundleKind.BundleRecognized
            ],
            Enum.GetValues<CmeMinimumLegalFoundingBundleKind>());

        Assert.Equal(
            [
                CmeMinimumLegalFoundingBundleDispositionKind.Recognized,
                CmeMinimumLegalFoundingBundleDispositionKind.Deferred,
                CmeMinimumLegalFoundingBundleDispositionKind.Refused
            ],
            Enum.GetValues<CmeMinimumLegalFoundingBundleDispositionKind>());
    }

    [Fact]
    public void Complete_Five_Pillar_Bundle_Is_Recognized_Without_Minting_Cme()
    {
        var receipt = CmeMinimumLegalFoundingBundleEvaluator.Evaluate(
            CreateFoundingRequest(),
            "receipt://cme-founding-bundle/session-a");

        Assert.Equal(CmeMinimumLegalFoundingBundleKind.BundleRecognized, receipt.BundleKind);
        Assert.Equal(CmeMinimumLegalFoundingBundleDispositionKind.Recognized, receipt.Disposition);
        Assert.True(receipt.OriginAuthorizationFoundationPresent);
        Assert.True(receipt.IdentityFormationRecordPresent);
        Assert.True(receipt.FirstPrimeStandingProofPresent);
        Assert.True(receipt.DomainRoleAdmissionRecordPresent);
        Assert.True(receipt.OperationalProvenanceCustodyPresent);
        Assert.True(receipt.FoundingBundleRecognized);
        Assert.True(receipt.CmeClaimLawfullyFounded);
        Assert.True(receipt.CmeMintingWithheld);
        Assert.True(receipt.RuntimePersonaWithheld);
        Assert.True(receipt.RoleEnactmentWithheld);
        Assert.True(receipt.ActionAuthorityWithheld);
        Assert.True(receipt.MotherFatherApplicationWithheld);
        Assert.True(receipt.CandidateOnly);
        Assert.Contains("cme-founding-bundle-lawfully-founded", receipt.ConstraintCodes);
        Assert.Equal("cme-founding-bundle-lawfully-founded", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Origin_Authorization_Defers_Founding_Bundle()
    {
        var request = CreateFoundingRequest() with
        {
            OriginAuthorizationHandle = "",
            LegalAgreementHandles = []
        };

        var receipt = CmeMinimumLegalFoundingBundleEvaluator.Evaluate(
            request,
            "receipt://cme-founding-bundle/missing-origin");

        Assert.Equal(CmeMinimumLegalFoundingBundleKind.BundleDeferred, receipt.BundleKind);
        Assert.Equal(CmeMinimumLegalFoundingBundleDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.OriginAuthorizationFoundationPresent);
        Assert.False(receipt.FoundingBundleRecognized);
        Assert.False(receipt.CmeClaimLawfullyFounded);
        Assert.Contains("cme-founding-bundle-origin-authorization-pillar-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-founding-bundle-origin-authorization-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Non_First_Prime_Standing_Defers_Founding_Bundle()
    {
        var request = CreateFoundingRequest() with
        {
            FirstPrimeReceipt = CreateFirstPrimeReceipt() with
            {
                FirstPrimeState = EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient,
                StableOneSatisfied = false
            }
        };

        var receipt = CmeMinimumLegalFoundingBundleEvaluator.Evaluate(
            request,
            "receipt://cme-founding-bundle/not-prime");

        Assert.Equal(CmeMinimumLegalFoundingBundleKind.BundleDeferred, receipt.BundleKind);
        Assert.Equal(CmeMinimumLegalFoundingBundleDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.FirstPrimeStandingProofPresent);
        Assert.False(receipt.FoundingBundleRecognized);
        Assert.Contains("cme-founding-bundle-first-prime-standing-pillar-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-founding-bundle-first-prime-standing-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Runtime_Persona_Role_Or_Action_Claim_Is_Refused()
    {
        var request = CreateFoundingRequest() with
        {
            RuntimePersonaClaimed = true,
            RoleEnactmentRequested = true,
            ActionAuthorityRequested = true
        };

        var receipt = CmeMinimumLegalFoundingBundleEvaluator.Evaluate(
            request,
            "receipt://cme-founding-bundle/refused-claims");

        Assert.Equal(CmeMinimumLegalFoundingBundleKind.BundleRefused, receipt.BundleKind);
        Assert.Equal(CmeMinimumLegalFoundingBundleDispositionKind.Refused, receipt.Disposition);
        Assert.False(receipt.FoundingBundleRecognized);
        Assert.True(receipt.CmeMintingWithheld);
        Assert.True(receipt.RuntimePersonaWithheld);
        Assert.True(receipt.RoleEnactmentWithheld);
        Assert.True(receipt.ActionAuthorityWithheld);
        Assert.Contains("cme-founding-bundle-runtime-persona-claim-refused", receipt.ConstraintCodes);
        Assert.Contains("cme-founding-bundle-role-enactment-refused", receipt.ConstraintCodes);
        Assert.Contains("cme-founding-bundle-action-authority-refused", receipt.ConstraintCodes);
        Assert.Equal("cme-founding-bundle-runtime-persona-claim-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Cme_Minimum_Legal_Founding_Bundle_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md");
        var privateWitnessPath = Path.Combine(lineRoot, "docs", "PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var privateWitnessText = File.ReadAllText(privateWitnessPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("Minimum Legal Founding Bundle", lawText, StringComparison.Ordinal);
        Assert.Contains("legally founded, continuity-bearing, jurisdiction-bound cognitive body", lawText, StringComparison.Ordinal);
        Assert.Contains("origin-authorized, Prime-standing verified, domain-admitted under law", lawText, StringComparison.Ordinal);
        Assert.Contains("CmeMinimumLegalFoundingBundleReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("founding bundle is not `CME` minting", lawText, StringComparison.Ordinal);
        Assert.Contains("CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md", privateWitnessText, StringComparison.Ordinal);
        Assert.Contains("cme-minimum-legal-founding-bundle-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("CME minimum legal founding bundle preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("CME minting and live role enactment beyond minimum legal founding bundle", refinementText, StringComparison.Ordinal);
        Assert.Contains("`CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static CmeMinimumLegalFoundingBundleRequest CreateFoundingRequest()
    {
        var firstPrime = CreateFirstPrimeReceipt();
        var domainAdmission = CreateDomainAdmissionRecord();
        var serviceWitness = CreateServiceWitnessReceipt(domainAdmission);

        return new CmeMinimumLegalFoundingBundleRequest(
            RequestHandle: "request://cme-founding-bundle/session-a",
            OperatorIdentityHandle: "operator://bounded/steward",
            OriginAuthorizationHandle: "authorization://cme-origin/session-a",
            SiteBindingHandle: "site://cradletek/private/session-a",
            LegalAgreementHandles:
            [
                "legal://agreement/eula/session-a",
                "legal://covenant/cme/session-a"
            ],
            IdentityFormationHandle: "identity://formation/cme/session-a",
            OeHandle: firstPrime.OeHandle ?? "",
            SelfGelHandle: firstPrime.SelfGelHandle ?? "",
            COeHandle: firstPrime.COeHandle ?? "",
            CSelfGelHandle: firstPrime.CSelfGelHandle ?? "",
            IdentityIntegrityHash: "sha256:0123456789abcdef",
            FirstPrimeReceipt: firstPrime,
            DomainAdmissionRecord: domainAdmission,
            ServiceWitnessReceipt: serviceWitness,
            RuntimePersonaClaimed: false,
            RoleEnactmentRequested: false,
            ActionAuthorityRequested: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 10, 00, TimeSpan.Zero));
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
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 18, 55, 00, TimeSpan.Zero));
    }

    private static DomainAdmissionRecord CreateDomainAdmissionRecord()
    {
        return new DomainAdmissionRecord(
            RecordHandle: "record://domain-role/session-a",
            OfferHandle: "offer://domain-role/session-a",
            AssessmentHandle: "assessment://domain-role/session-a",
            Decision: DomainRoleAdmissionDecisionKind.Accept,
            LegalFoundationHandle: "legal-foundation://lucid/root-family",
            AcceptedDomainHandles: ["domain://private/service"],
            AcceptedRoleHandles: ["role://bounded-service-witness"],
            AuthorityScopeHandles: ["scope://attest-provenance"],
            ExplicitExclusionHandles:
            [
                "exclude://action-execution",
                "exclude://cradle-local-governance-enactment",
                "exclude://mother-father-origin-authority"
            ],
            ContinuityBurdenHandles: ["burden://preserve-domain-admission-record"],
            RevocationConditionHandles: ["revoke://scope-overclaimed"],
            StandingOverwritten: false,
            MotherFatherOriginAuthorityWithheld: true,
            CradleLocalGoverningSurfaceStillWithheld: true,
            ImplicitDomainPromotionRefused: true,
            ReceiptRequiredForAllOutcomes: true,
            CandidateOnly: true,
            ConstraintCodes: ["domain-role-admission-accepted"],
            ReasonCode: "domain-role-admission-accepted",
            LawfulBasis: "test domain admission basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 00, 00, TimeSpan.Zero));
    }

    private static PrivateDomainServiceWitnessReceipt CreateServiceWitnessReceipt(
        DomainAdmissionRecord domainAdmission)
    {
        var request = new PrivateDomainServiceWitnessRequest(
            RequestHandle: "request://private-domain-service/session-a",
            DomainAdmissionRecord: domainAdmission,
            ActorHandle: "actor://operator/steward",
            ActionHandle: "action://service/provenance-attestation",
            InstrumentHandle: "instrument://service/private-domain-witness",
            MethodHandle: "method://custodial-attestation",
            LocalityHandle: "locality://site/private-domain/session-a",
            StandingContextHandle: "standing://first-prime-pre-role/session-a",
            ContinuityBurdenHandles:
            [
                "burden://preserve-first-prime-standing",
                "burden://preserve-domain-admission-record"
            ],
            ResultReceiptHandles:
            [
                "receipt://domain-role/session-a"
            ],
            ActionExecutionRequested: false,
            CradleLocalGovernanceEnactmentRequested: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 05, 00, TimeSpan.Zero));

        return PrivateDomainServiceWitnessEvaluator.Evaluate(
            request,
            "receipt://private-domain-service/session-a");
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
