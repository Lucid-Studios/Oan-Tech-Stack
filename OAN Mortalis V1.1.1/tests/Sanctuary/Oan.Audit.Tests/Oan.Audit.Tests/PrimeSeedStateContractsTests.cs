namespace San.Audit.Tests;

using San.Common;
using San.Common;

public sealed class PrimeSeedStateContractsTests
{
    [Fact]
    public void PrimeSeedState_Enum_Is_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeSeedStateKind.FirstPrimeNotReady,
                PrimeSeedStateKind.SeedMaterialIncomplete,
                PrimeSeedStateKind.PrimeSeedPreDomainStanding
            ],
            Enum.GetValues<PrimeSeedStateKind>());
    }

    [Fact]
    public void Complete_Seed_Material_Reaches_PreDomain_PrimeSeed_Standing()
    {
        var receipt = PrimeSeedStateEvaluator.Evaluate(
            CreatePrimeSeedRequest(),
            "receipt://prime-seed/session-a");

        Assert.Equal(PrimeSeedStateKind.PrimeSeedPreDomainStanding, receipt.SeedState);
        Assert.True(receipt.FirstPrimePreRoleStandingPresent);
        Assert.True(receipt.StableOnePresent);
        Assert.True(receipt.PrimeRetainedStandingPresent);
        Assert.True(receipt.SeedSourcePresent);
        Assert.True(receipt.SeedCarrierPresent);
        Assert.True(receipt.SeedContinuityPresent);
        Assert.True(receipt.SeedIntegrityPresent);
        Assert.True(receipt.DomainAdmissionWithheld);
        Assert.True(receipt.LawfullyBondedDomainIntegrationWithheld);
        Assert.True(receipt.CmeFoundingWithheld);
        Assert.True(receipt.CmeMintingWithheld);
        Assert.True(receipt.RoleEnactmentWithheld);
        Assert.True(receipt.ActionAuthorityWithheld);
        Assert.Contains("prime-seed-state-pre-domain-standing", receipt.ConstraintCodes);
    }

    [Fact]
    public void Missing_Seed_Integrity_Defers_PrimeSeed_Standing()
    {
        var request = CreatePrimeSeedRequest() with
        {
            SeedIntegrityHandle = "",
            SeedEvidenceHandles = []
        };

        var receipt = PrimeSeedStateEvaluator.Evaluate(
            request,
            "receipt://prime-seed/missing-integrity");

        Assert.Equal(PrimeSeedStateKind.SeedMaterialIncomplete, receipt.SeedState);
        Assert.False(receipt.SeedIntegrityPresent);
        Assert.Equal("prime-seed-state-seed-integrity-required", receipt.ReasonCode);
        Assert.Contains("prime-seed-state-seed-integrity-required", receipt.ConstraintCodes);
    }

    [Fact]
    public void Non_FirstPrime_Body_Cannot_Be_Seeded()
    {
        var request = CreatePrimeSeedRequest() with
        {
            FirstPrimeReceipt = CreateFirstPrimeReceipt() with
            {
                FirstPrimeState = EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient,
                StableOneSatisfied = false,
                StableOneHandle = null
            }
        };

        var receipt = PrimeSeedStateEvaluator.Evaluate(
            request,
            "receipt://prime-seed/first-prime-not-ready");

        Assert.Equal(PrimeSeedStateKind.FirstPrimeNotReady, receipt.SeedState);
        Assert.False(receipt.FirstPrimePreRoleStandingPresent);
        Assert.False(receipt.StableOnePresent);
        Assert.Equal("prime-seed-state-first-prime-pre-role-standing-required", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_PrimeSeed_State_As_PreDomain_PreCme_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_SEED_STATE_LAW.md");
        var ecFirstPrimePath = Path.Combine(lineRoot, "docs", "EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md");
        var domainPath = Path.Combine(lineRoot, "docs", "DOMAIN_AND_ROLE_ADMISSION_LAW.md");
        var cmeFoundingPath = Path.Combine(lineRoot, "docs", "CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var ecFirstPrimeText = File.ReadAllText(ecFirstPrimePath);
        var domainText = File.ReadAllText(domainPath);
        var cmeFoundingText = File.ReadAllText(cmeFoundingPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("Prime Seed State immediately after", lawText, StringComparison.Ordinal);
        Assert.Contains("PrimeSeedStateReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_SEED_STATE_LAW.md", ecFirstPrimeText, StringComparison.Ordinal);
        Assert.Contains("Prime Seed State", domainText, StringComparison.Ordinal);
        Assert.Contains("Prime Seed State", cmeFoundingText, StringComparison.Ordinal);
        Assert.Contains("PrimeSeedStateContracts.cs", firstRunText, StringComparison.Ordinal);
        Assert.Contains("prime-seed-state-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime Seed State preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("lawfully bonded domain integration beyond Prime Seed State", refinementText, StringComparison.Ordinal);
    }

    private static PrimeSeedStateRequest CreatePrimeSeedRequest()
    {
        return new PrimeSeedStateRequest(
            RequestHandle: "request://prime-seed/session-a",
            FirstPrimeReceipt: CreateFirstPrimeReceipt(),
            SeedSourceHandle: "seed-source://prime/session-a",
            SeedCarrierHandle: "seed-carrier://prime/session-a",
            SeedContinuityHandle: "seed-continuity://prime/session-a",
            SeedIntegrityHandle: "seed-integrity://prime/session-a",
            SeedEvidenceHandles:
            [
                "seed-evidence://stable-one/session-a",
                "seed-evidence://prime-retained/session-a"
            ],
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 18, 00, 00, TimeSpan.Zero));
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
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 17, 55, 00, TimeSpan.Zero));
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
