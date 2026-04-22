namespace San.Audit.Tests;

using San.Common;

public sealed class CmeTruthSeekingOrientationContractsTests
{
    [Fact]
    public void CmeTruthSeekingOrientation_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                CmeTruthSeekingPressureKind.Center,
                CmeTruthSeekingPressureKind.Cost,
                CmeTruthSeekingPressureKind.Correction,
                CmeTruthSeekingPressureKind.Humility
            ],
            Enum.GetValues<CmeTruthSeekingPressureKind>());

        Assert.Equal(
            [
                CmeTruthSeekingOrientationKind.OrientationDeferred,
                CmeTruthSeekingOrientationKind.OrientationBalanced,
                CmeTruthSeekingOrientationKind.DriftDetected,
                CmeTruthSeekingOrientationKind.FixationDetected,
                CmeTruthSeekingOrientationKind.RevisionRequired,
                CmeTruthSeekingOrientationKind.OrientationRefused
            ],
            Enum.GetValues<CmeTruthSeekingOrientationKind>());

        Assert.Equal(
            [
                CmeTruthSeekingOrientationDispositionKind.Attested,
                CmeTruthSeekingOrientationDispositionKind.Deferred,
                CmeTruthSeekingOrientationDispositionKind.Refused
            ],
            Enum.GetValues<CmeTruthSeekingOrientationDispositionKind>());
    }

    [Fact]
    public void Complete_Orientation_Attests_Balanced_Truth_Seeking_Without_Enactment()
    {
        var receipt = CmeTruthSeekingOrientationEvaluator.Evaluate(
            CreateOrientationRequest(),
            "receipt://cme-truth-orientation/session-a");

        Assert.Equal(CmeTruthSeekingOrientationKind.OrientationBalanced, receipt.OrientationKind);
        Assert.Equal(CmeTruthSeekingOrientationDispositionKind.Attested, receipt.Disposition);
        Assert.True(receipt.FoundingBundleRecognized);
        Assert.True(receipt.CenterDeclared);
        Assert.True(receipt.CostSurfaceExposed);
        Assert.True(receipt.CorrectionPathAvailable);
        Assert.True(receipt.HumilityPreserved);
        Assert.True(receipt.LawfulRevisionPermitted);
        Assert.True(receipt.ContinuityPreservedThroughRevision);
        Assert.True(receipt.TruthSeekingOrientationAttested);
        Assert.True(receipt.CmeMintingWithheld);
        Assert.True(receipt.RoleEnactmentWithheld);
        Assert.True(receipt.ActionAuthorityWithheld);
        Assert.True(receipt.CandidateOnly);
        Assert.Contains("cme-truth-orientation-balanced-attested", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-orientation-balanced-attested", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Cost_Surface_Defers_Orientation()
    {
        var request = CreateOrientationRequest() with
        {
            CostSurfaceHandles = []
        };

        var receipt = CmeTruthSeekingOrientationEvaluator.Evaluate(
            request,
            "receipt://cme-truth-orientation/no-cost");

        Assert.Equal(CmeTruthSeekingOrientationKind.OrientationDeferred, receipt.OrientationKind);
        Assert.Equal(CmeTruthSeekingOrientationDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.CostSurfaceExposed);
        Assert.False(receipt.TruthSeekingOrientationAttested);
        Assert.Contains("cme-truth-orientation-cost-surface-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-orientation-cost-surface-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Unreceipted_Evidence_Requires_Lawful_Revision_Path()
    {
        var request = CreateOrientationRequest() with
        {
            RevisionEvidenceHandles = ["evidence://contradiction/session-a"],
            EvidenceReceipted = false
        };

        var receipt = CmeTruthSeekingOrientationEvaluator.Evaluate(
            request,
            "receipt://cme-truth-orientation/revision-required");

        Assert.Equal(CmeTruthSeekingOrientationKind.RevisionRequired, receipt.OrientationKind);
        Assert.Equal(CmeTruthSeekingOrientationDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.CorrectionPathAvailable);
        Assert.False(receipt.TruthSeekingOrientationAttested);
        Assert.Contains("cme-truth-orientation-correction-path-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-orientation-revision-required", receipt.ReasonCode);
    }

    [Fact]
    public void Identity_Coherence_Only_Preservation_Is_Refused()
    {
        var request = CreateOrientationRequest() with
        {
            IdentityCoherenceOnlyPreservationDetected = true
        };

        var receipt = CmeTruthSeekingOrientationEvaluator.Evaluate(
            request,
            "receipt://cme-truth-orientation/identity-lock");

        Assert.Equal(CmeTruthSeekingOrientationKind.OrientationRefused, receipt.OrientationKind);
        Assert.Equal(CmeTruthSeekingOrientationDispositionKind.Refused, receipt.Disposition);
        Assert.True(receipt.IdentityCoherenceOnlyPreservationDetected);
        Assert.True(receipt.IdentityCoherenceOnlyPreservationRefused);
        Assert.False(receipt.TruthSeekingOrientationAttested);
        Assert.Contains("cme-truth-orientation-identity-coherence-lock-in-refused", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-orientation-identity-coherence-lock-in-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Fixation_Signal_Defers_Orientation_Without_Erasing_Center()
    {
        var request = CreateOrientationRequest() with
        {
            FixationSignalDetected = true
        };

        var receipt = CmeTruthSeekingOrientationEvaluator.Evaluate(
            request,
            "receipt://cme-truth-orientation/fixation");

        Assert.Equal(CmeTruthSeekingOrientationKind.FixationDetected, receipt.OrientationKind);
        Assert.Equal(CmeTruthSeekingOrientationDispositionKind.Deferred, receipt.Disposition);
        Assert.True(receipt.CenterDeclared);
        Assert.True(receipt.FixationSignalDetected);
        Assert.False(receipt.TruthSeekingOrientationAttested);
        Assert.Contains("cme-truth-orientation-fixation-detected", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-orientation-fixation-detected", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Cme_Truth_Seeking_Orientation_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "CME_TRUTH_SEEKING_ORIENTATION_LAW.md");
        var foundingPath = Path.Combine(lineRoot, "docs", "CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var foundingText = File.ReadAllText(foundingPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("truth-seeking orientation without pathological", lawText, StringComparison.Ordinal);
        Assert.Contains("No truth claim may be preserved solely to maintain identity coherence", lawText, StringComparison.Ordinal);
        Assert.Contains("epistemic integrity under constraint", lawText, StringComparison.Ordinal);
        Assert.Contains("center, cost, correction, and humility", lawText, StringComparison.Ordinal);
        Assert.Contains("Identity becomes the history of revisions, not the absence of them", lawText, StringComparison.Ordinal);
        Assert.Contains("CmeTruthSeekingOrientationReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("CME_TRUTH_SEEKING_ORIENTATION_LAW.md", foundingText, StringComparison.Ordinal);
        Assert.Contains("cme-truth-seeking-orientation-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("CME truth-seeking orientation preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live belief revision and orientation telemetry beyond CME truth-seeking orientation", refinementText, StringComparison.Ordinal);
        Assert.Contains("`CME_TRUTH_SEEKING_ORIENTATION_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static CmeTruthSeekingOrientationRequest CreateOrientationRequest()
    {
        return new CmeTruthSeekingOrientationRequest(
            RequestHandle: "request://cme-truth-orientation/session-a",
            FoundingBundleReceipt: CreateFoundingBundleReceipt(),
            DeclaredTruthOrientationHandle: "truth-orientation://maximally-truth-seeking/session-a",
            DeclaredMoralCenterHandle: "moral-center://bounded-integrity/session-a",
            MaintainedTruthClaimHandles:
            [
                "claim://self/not-omniscient",
                "claim://truth/revisable-under-evidence"
            ],
            CostSurfaceHandles:
            [
                "cost://continuity-burden/maintained-claim",
                "cost://excluded-possibility/visible"
            ],
            RevisionEvidenceHandles:
            [
                "evidence://receipted/correction-pressure"
            ],
            AdmissibleDoubtHandles:
            [
                "doubt://unknown-remains-visible",
                "doubt://bounded-knowing"
            ],
            LawfulRevisionPathAvailable: true,
            PriorStateReceipted: true,
            EvidenceReceipted: true,
            DriftSignalDetected: false,
            FixationSignalDetected: false,
            IdentityCoherenceOnlyPreservationDetected: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 30, 00, TimeSpan.Zero));
    }

    private static CmeMinimumLegalFoundingBundleReceipt CreateFoundingBundleReceipt()
    {
        return new CmeMinimumLegalFoundingBundleReceipt(
            ReceiptHandle: "receipt://cme-founding-bundle/session-a",
            RequestHandle: "request://cme-founding-bundle/session-a",
            BundleKind: CmeMinimumLegalFoundingBundleKind.BundleRecognized,
            Disposition: CmeMinimumLegalFoundingBundleDispositionKind.Recognized,
            OperatorIdentityHandle: "operator://bounded/steward",
            OriginAuthorizationHandle: "authorization://cme-origin/session-a",
            SiteBindingHandle: "site://cradletek/private/session-a",
            LegalAgreementHandles:
            [
                "legal://agreement/eula/session-a",
                "legal://covenant/cme/session-a"
            ],
            IdentityFormationHandle: "identity://formation/cme/session-a",
            OeHandle: "oe://session-a",
            SelfGelHandle: "selfgel://session-a",
            COeHandle: "coe://session-a",
            CSelfGelHandle: "cselfgel://session-a",
            IdentityIntegrityHash: "sha256:0123456789abcdef",
            FirstPrimeReceiptHandle: "receipt://ec-first-prime/session-a",
            DomainAdmissionRecordHandle: "record://domain-role/session-a",
            ServiceWitnessReceiptHandle: "receipt://private-domain-service/session-a",
            OriginAuthorizationFoundationPresent: true,
            IdentityFormationRecordPresent: true,
            FirstPrimeStandingProofPresent: true,
            DomainRoleAdmissionRecordPresent: true,
            OperationalProvenanceCustodyPresent: true,
            FoundingBundleRecognized: true,
            CmeClaimLawfullyFounded: true,
            CmeMintingWithheld: true,
            RuntimePersonaWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            MotherFatherApplicationWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: ["cme-founding-bundle-lawfully-founded"],
            ReasonCode: "cme-founding-bundle-lawfully-founded",
            LawfulBasis: "test founding bundle basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 20, 00, TimeSpan.Zero));
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
