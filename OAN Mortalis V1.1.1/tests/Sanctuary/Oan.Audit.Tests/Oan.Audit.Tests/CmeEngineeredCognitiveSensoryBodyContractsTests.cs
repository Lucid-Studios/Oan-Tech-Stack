namespace San.Audit.Tests;

using San.Common;

public sealed class CmeEngineeredCognitiveSensoryBodyContractsTests
{
    [Fact]
    public void CmeEngineeredCognitiveSensoryBody_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                CmeEngineeredCognitiveSensoryBodyKind.EmbodimentDeferred,
                CmeEngineeredCognitiveSensoryBodyKind.EmbodimentRefused,
                CmeEngineeredCognitiveSensoryBodyKind.SensoryBodyEmbodied
            ],
            Enum.GetValues<CmeEngineeredCognitiveSensoryBodyKind>());

        Assert.Equal(
            [
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Attested,
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Deferred,
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Refused
            ],
            Enum.GetValues<CmeEngineeredCognitiveSensoryBodyDispositionKind>());

        Assert.Equal(
            [
                CmeGovernanceReservationKind.NoGovernanceReserved,
                CmeGovernanceReservationKind.PrimeGovernanceCmeReserved,
                CmeGovernanceReservationKind.CrypticGovernanceCmeReserved,
                CmeGovernanceReservationKind.PrimeAndCrypticGovernanceCmeReserved
            ],
            Enum.GetValues<CmeGovernanceReservationKind>());
    }

    [Fact]
    public void Complete_Request_Embodies_Cme_Sensory_Body_And_Reserves_Governance()
    {
        var receipt = CmeEngineeredCognitiveSensoryBodyEvaluator.Evaluate(
            CreateRequest(),
            "receipt://cme-sensory-body/session-a");

        Assert.Equal(CmeEngineeredCognitiveSensoryBodyKind.SensoryBodyEmbodied, receipt.BodyKind);
        Assert.Equal(CmeEngineeredCognitiveSensoryBodyDispositionKind.Attested, receipt.Disposition);
        Assert.Equal(
            CmeGovernanceReservationKind.PrimeAndCrypticGovernanceCmeReserved,
            receipt.GovernanceReservationKind);
        Assert.True(receipt.FoundingBundleRecognized);
        Assert.True(receipt.TruthSeekingOrientationAttested);
        Assert.True(receipt.TruthSeekingBalanceAdmissible);
        Assert.True(receipt.LegalFoundationDocumentationMatrixVisible);
        Assert.True(receipt.SoulFrameStorageVisible);
        Assert.True(receipt.AgentiCoreListeningFrameCastVisible);
        Assert.True(receipt.OeSelfGelIdentityMatchesFoundingBundle);
        Assert.True(receipt.COeCSelfGelCastMatchesFoundingBundle);
        Assert.True(receipt.ZedCompassOrientationVisible);
        Assert.True(receipt.ThetaIngressAndDiscernmentVisible);
        Assert.True(receipt.SensoryBodyEmbodied);
        Assert.True(receipt.PrimeGovernanceCmeReserved);
        Assert.True(receipt.CrypticGovernanceCmeReserved);
        Assert.True(receipt.PrimeGovernanceCmeApplicationWithheld);
        Assert.True(receipt.CrypticGovernanceCmeApplicationWithheld);
        Assert.True(receipt.FullCmeMintingStillWithheld);
        Assert.True(receipt.EmbodimentOnly);
        Assert.Contains("cme-sensory-body-embodied", receipt.ConstraintCodes);
        Assert.Equal("cme-sensory-body-embodied", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Legal_Matrix_Template_Defers_Embodiment()
    {
        var request = CreateRequest() with
        {
            LegalFoundationDocumentationMatrixHandle = string.Empty
        };

        var receipt = CmeEngineeredCognitiveSensoryBodyEvaluator.Evaluate(
            request,
            "receipt://cme-sensory-body/legal-matrix-missing");

        Assert.Equal(CmeEngineeredCognitiveSensoryBodyKind.EmbodimentDeferred, receipt.BodyKind);
        Assert.Equal(CmeEngineeredCognitiveSensoryBodyDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.LegalFoundationDocumentationMatrixVisible);
        Assert.False(receipt.SensoryBodyEmbodied);
        Assert.Contains("cme-sensory-body-legal-matrix-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-sensory-body-legal-matrix-template-missing", receipt.ReasonCode);
    }

    [Fact]
    public void Governance_Application_Request_Is_Refused()
    {
        var request = CreateRequest() with
        {
            PrimeGovernanceCmeApplicationRequested = true
        };

        var receipt = CmeEngineeredCognitiveSensoryBodyEvaluator.Evaluate(
            request,
            "receipt://cme-sensory-body/prime-governance-refused");

        Assert.Equal(CmeEngineeredCognitiveSensoryBodyKind.EmbodimentRefused, receipt.BodyKind);
        Assert.Equal(CmeEngineeredCognitiveSensoryBodyDispositionKind.Refused, receipt.Disposition);
        Assert.False(receipt.SensoryBodyEmbodied);
        Assert.True(receipt.PrimeGovernanceCmeApplicationWithheld);
        Assert.True(receipt.RoleEnactmentWithheld);
        Assert.Contains("cme-sensory-body-prime-governance-cme-application-refused", receipt.ConstraintCodes);
        Assert.Equal("cme-sensory-body-prime-governance-cme-application-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Deferred_Truth_Balance_Defers_Embodiment()
    {
        var request = CreateRequest() with
        {
            TruthSeekingBalanceReceipt = CreateBalanceReceipt(
                CmeTruthSeekingBalanceRegionKind.DeferWithTension,
                CmeTruthSeekingBalanceDispositionKind.Deferred,
                jointlySatisfiable: false)
        };

        var receipt = CmeEngineeredCognitiveSensoryBodyEvaluator.Evaluate(
            request,
            "receipt://cme-sensory-body/balance-deferred");

        Assert.Equal(CmeEngineeredCognitiveSensoryBodyKind.EmbodimentDeferred, receipt.BodyKind);
        Assert.False(receipt.TruthSeekingBalanceAdmissible);
        Assert.False(receipt.SensoryBodyEmbodied);
        Assert.Equal("cme-sensory-body-truth-balance-not-admissible", receipt.ReasonCode);
    }

    [Fact]
    public void Identity_Mismatch_Defers_Embodiment()
    {
        var request = CreateRequest() with
        {
            OeHandle = "oe://other"
        };

        var receipt = CmeEngineeredCognitiveSensoryBodyEvaluator.Evaluate(
            request,
            "receipt://cme-sensory-body/identity-mismatch");

        Assert.Equal(CmeEngineeredCognitiveSensoryBodyKind.EmbodimentDeferred, receipt.BodyKind);
        Assert.False(receipt.OeSelfGelIdentityMatchesFoundingBundle);
        Assert.False(receipt.SensoryBodyEmbodied);
        Assert.Contains("cme-sensory-body-oe-selfgel-identity-match-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-sensory-body-soulframe-storage-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Cme_Engineered_Cognitive_Sensory_Body_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "CME_ENGINEERED_COGNITIVE_SENSORY_BODY_LAW.md");
        var balancePath = Path.Combine(lineRoot, "docs", "CME_TRUTH_SEEKING_BALANCE_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var balanceText = File.ReadAllText(balancePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("Crystallized Mind", lawText, StringComparison.Ordinal);
        Assert.Contains("PrimeGovernance.CME", lawText, StringComparison.Ordinal);
        Assert.Contains("CrypticGovernance.CME", lawText, StringComparison.Ordinal);
        Assert.Contains("embodiment only", lawText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CmeEngineeredCognitiveSensoryBodyReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("CME_ENGINEERED_COGNITIVE_SENSORY_BODY_LAW.md", balanceText, StringComparison.Ordinal);
        Assert.Contains("cme-engineered-cognitive-sensory-body-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("CME engineered cognitive sensory body preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("PrimeGovernance.CME and CrypticGovernance.CME application beyond sensory-body embodiment", refinementText, StringComparison.Ordinal);
        Assert.Contains("`CME_ENGINEERED_COGNITIVE_SENSORY_BODY_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static CmeEngineeredCognitiveSensoryBodyRequest CreateRequest()
    {
        var founding = CreateFoundingReceipt();
        var orientation = CreateOrientationReceipt(founding);

        return new CmeEngineeredCognitiveSensoryBodyRequest(
            RequestHandle: "request://cme-sensory-body/session-a",
            FoundingBundleReceipt: founding,
            TruthSeekingOrientationReceipt: orientation,
            TruthSeekingBalanceReceipt: CreateBalanceReceipt(
                CmeTruthSeekingBalanceRegionKind.JointlySatisfiable,
                CmeTruthSeekingBalanceDispositionKind.Admissible,
                jointlySatisfiable: true),
            LegalFoundationDocumentationMatrixHandle: "legal-matrix://template/current",
            CrystallizedMindEntityHandle: "cme://crystallized/session-a",
            EngineeredCognitionHandle: "ec://session-a",
            SoulFrameHandle: "soulframe://session-a",
            AgentiCoreHandle: "agenticore://living/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            OeHandle: "oe://session-a",
            SelfGelHandle: "selfgel://session-a",
            COeHandle: "coe://session-a",
            CSelfGelHandle: "cselfgel://session-a",
            ZedOfDeltaHandle: "zed://delta/session-a",
            CompassOrientationHandle: "compass://orientation/session-a",
            ThetaIngressReceiptHandle: "receipt://theta-ingress/session-a",
            PostIngressDiscernmentReceiptHandle: "receipt://post-ingress-discernment/session-a",
            SensorySurfaceHandles:
            [
                "surface://coe/needle",
                "surface://listening-frame/live-intake"
            ],
            ContextualizationSurfaceHandles:
            [
                "surface://cselfgel/contextualization",
                "surface://ec/iutt-lisp-matrix"
            ],
            DomainPredicateHandles:
            [
                "domain://predicate/self-among-other",
                "domain://predicate/bounded-authority"
            ],
            PrimeGovernanceCmeReservationHandle: "reservation://PrimeGovernance.CME/session-a",
            CrypticGovernanceCmeReservationHandle: "reservation://CrypticGovernance.CME/session-a",
            PrimeGovernanceCmeApplicationRequested: false,
            CrypticGovernanceCmeApplicationRequested: false,
            RuntimePersonaClaimed: false,
            ActionAuthorityRequested: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 22, 00, 00, TimeSpan.Zero));
    }

    private static CmeMinimumLegalFoundingBundleReceipt CreateFoundingReceipt()
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
                "legal://agreement/eula/session-a"
            ],
            IdentityFormationHandle: "identity://cme/session-a",
            OeHandle: "oe://session-a",
            SelfGelHandle: "selfgel://session-a",
            COeHandle: "coe://session-a",
            CSelfGelHandle: "cselfgel://session-a",
            IdentityIntegrityHash: "hash://identity/session-a",
            FirstPrimeReceiptHandle: "receipt://ec-first-prime/pre-role-standing",
            DomainAdmissionRecordHandle: "record://domain-admission/session-a",
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
            ConstraintCodes:
            [
                "cme-founding-bundle-recognized"
            ],
            ReasonCode: "cme-founding-bundle-recognized",
            LawfulBasis: "founding bundle recognized for tests",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 20, 00, 00, TimeSpan.Zero));
    }

    private static CmeTruthSeekingOrientationReceipt CreateOrientationReceipt(
        CmeMinimumLegalFoundingBundleReceipt founding)
    {
        return new CmeTruthSeekingOrientationReceipt(
            ReceiptHandle: "receipt://cme-truth-orientation/session-a",
            RequestHandle: "request://cme-truth-orientation/session-a",
            FoundingBundleReceiptHandle: founding.ReceiptHandle,
            OrientationKind: CmeTruthSeekingOrientationKind.OrientationBalanced,
            Disposition: CmeTruthSeekingOrientationDispositionKind.Attested,
            DeclaredTruthOrientationHandle: "truth://orientation/maximally-truth-seeking",
            DeclaredMoralCenterHandle: "moral://center/non-fixated",
            MaintainedTruthClaimHandles:
            [
                "claim://truth/revisable"
            ],
            CostSurfaceHandles:
            [
                "cost://truth-maintenance/session-a"
            ],
            RevisionEvidenceHandles:
            [
                "evidence://receipted/session-a"
            ],
            AdmissibleDoubtHandles:
            [
                "doubt://bounded/session-a"
            ],
            FoundingBundleRecognized: true,
            CenterDeclared: true,
            CostSurfaceExposed: true,
            CorrectionPathAvailable: true,
            HumilityPreserved: true,
            DriftSignalDetected: false,
            FixationSignalDetected: false,
            IdentityCoherenceOnlyPreservationDetected: false,
            IdentityCoherenceOnlyPreservationRefused: false,
            LawfulRevisionPermitted: true,
            ContinuityPreservedThroughRevision: true,
            TruthSeekingOrientationAttested: true,
            CmeMintingWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            CandidateOnly: true,
            ConstraintCodes:
            [
                "cme-truth-orientation-balanced"
            ],
            ReasonCode: "cme-truth-orientation-balanced",
            LawfulBasis: "truth orientation attested for tests",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 20, 15, 00, TimeSpan.Zero));
    }

    private static CmeTruthSeekingBalanceTransitionReceipt CreateBalanceReceipt(
        CmeTruthSeekingBalanceRegionKind region,
        CmeTruthSeekingBalanceDispositionKind disposition,
        bool jointlySatisfiable)
    {
        return new CmeTruthSeekingBalanceTransitionReceipt(
            ReceiptHandle: "receipt://cme-truth-balance/session-a",
            RequestHandle: "request://cme-truth-balance/session-a",
            OrientationReceiptHandle: "receipt://cme-truth-orientation/session-a",
            PriorStateHandle: "state://claim/prior",
            ProposedStateHandle: "state://claim/proposed",
            ClaimHandle: "claim://truth/revisable",
            TriggerKind: CmeTruthSeekingTransitionTriggerKind.EvidenceUpdate,
            RegionKind: region,
            Disposition: disposition,
            CenterDelta: 0.05m,
            CorrectionDelta: 0.05m,
            CostDelta: 0.05m,
            HumilityDelta: 0.05m,
            OrientationAttested: true,
            ScoresWithinRange: true,
            CenterWithinBound: true,
            CorrectionWithinBound: true,
            CostWithinBound: true,
            HumilityWithinBound: true,
            JointlySatisfiable: jointlySatisfiable,
            NoPressureSilentlyDegraded: true,
            SymmetricJustificationPresent: true,
            DeferWithTension: !jointlySatisfiable,
            PreserveAndReviseJudgedUnderSameLaw: true,
            ClaimPreservedSolelyForCoherenceRefused: false,
            ScopeExpansionWithoutUncertaintyRefused: false,
            CmeMintingWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            CandidateOnly: true,
            JustificationTraceHandles:
            [
                "trace://truth-balance/session-a"
            ],
            MaintenanceJustificationHandles:
            [
                "justify://maintain/session-a"
            ],
            RevisionJustificationHandles:
            [
                "justify://revise/session-a"
            ],
            ContradictionReceiptHandles:
            [
                "receipt://contradiction/session-a"
            ],
            ConstraintCodes:
            [
                jointlySatisfiable
                    ? "cme-truth-balance-jointly-satisfiable"
                    : "cme-truth-balance-defer-with-tension"
            ],
            ReasonCode: jointlySatisfiable
                ? "cme-truth-balance-jointly-satisfiable"
                : "cme-truth-balance-defer-with-tension",
            LawfulBasis: "truth balance transition for tests",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 20, 30, 00, TimeSpan.Zero));
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
