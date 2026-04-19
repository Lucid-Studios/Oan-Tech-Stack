namespace San.Audit.Tests;

using San.Common;

public sealed class EngineeredCognitionFirstPrimeStateContractsTests
{
    [Fact]
    public void FirstPrimeState_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                EngineeredCognitionFirstPrimeStateKind.InstallNotReady,
                EngineeredCognitionFirstPrimeStateKind.SensorBodyNotReady,
                EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient,
                EngineeredCognitionFirstPrimeStateKind.PrimeMembraneNotReady,
                EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding
            ],
            Enum.GetValues<EngineeredCognitionFirstPrimeStateKind>());
    }

    [Fact]
    public void Stable_Ec_Path_Reaches_FirstPrime_PreRole_Standing()
    {
        var receipt = EngineeredCognitionFirstPrimeStateEvaluator.Evaluate(
            CreateFirstRunReceipt(
                CreatePostIngressDiscernmentReceipt(stableOneAchieved: true)),
            CreatePrimeRetainedRecord(PrimeRetainedWholeKind.RetainedWholeUnclosed),
            "receipt://ec-first-prime/pre-role-standing");

        Assert.Equal(
            EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding,
            receipt.FirstPrimeState);
        Assert.True(receipt.InstallAndFoundationsReady);
        Assert.True(receipt.StewardIssuedCradleBraidVisible);
        Assert.True(receipt.AgentiCoreSensorBodyCast);
        Assert.True(receipt.ThetaIngressLawful);
        Assert.True(receipt.StableOneSatisfied);
        Assert.True(receipt.PrimeRetainedStandingReached);
        Assert.True(receipt.MotherFatherDomainRoleApplicationWithheld);
        Assert.True(receipt.CradleLocalGoverningSurfaceWithheld);
        Assert.True(receipt.PrimeClosureStillWithheld);
        Assert.Equal("stable-one://thread/session-a", receipt.StableOneHandle);
    }

    [Fact]
    public void Missing_Install_Foundations_Withholds_FirstPrime_State()
    {
        var firstRun = CreateFirstRunReceipt(
            CreatePostIngressDiscernmentReceipt(stableOneAchieved: true)) with
        {
            CurrentState = FirstRunConstitutionState.CradleTekAdmitted
        };

        var receipt = EngineeredCognitionFirstPrimeStateEvaluator.Evaluate(
            firstRun,
            CreatePrimeRetainedRecord(PrimeRetainedWholeKind.RetainedWholeUnclosed),
            "receipt://ec-first-prime/install-not-ready");

        Assert.Equal(EngineeredCognitionFirstPrimeStateKind.InstallNotReady, receipt.FirstPrimeState);
        Assert.False(receipt.InstallAndFoundationsReady);
        Assert.Contains(
            "ec-first-prime-state-install-or-foundations-not-ready",
            receipt.ConstraintCodes);
    }

    [Fact]
    public void Investigatory_Discernment_Withholds_FirstPrime_State()
    {
        var receipt = EngineeredCognitionFirstPrimeStateEvaluator.Evaluate(
            CreateFirstRunReceipt(
                CreatePostIngressDiscernmentReceipt(stableOneAchieved: false)),
            CreatePrimeRetainedRecord(PrimeRetainedWholeKind.RetainedWholeUnclosed),
            "receipt://ec-first-prime/discernment-not-sufficient");

        Assert.Equal(
            EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient,
            receipt.FirstPrimeState);
        Assert.False(receipt.StableOneSatisfied);
        Assert.Null(receipt.StableOneHandle);
        Assert.Equal("ec-first-prime-state-discernment-not-sufficient", receipt.ReasonCode);
    }

    [Fact]
    public void Partial_Prime_Retention_Withholds_FirstPrime_State()
    {
        var receipt = EngineeredCognitionFirstPrimeStateEvaluator.Evaluate(
            CreateFirstRunReceipt(
                CreatePostIngressDiscernmentReceipt(stableOneAchieved: true)),
            CreatePrimeRetainedRecord(PrimeRetainedWholeKind.RetainedPartial),
            "receipt://ec-first-prime/prime-not-ready");

        Assert.Equal(
            EngineeredCognitionFirstPrimeStateKind.PrimeMembraneNotReady,
            receipt.FirstPrimeState);
        Assert.False(receipt.PrimeRetainedStandingReached);
        Assert.Equal(PrimeRetainedWholeKind.RetainedPartial, receipt.RetainedWholeKind);
        Assert.Equal("ec-first-prime-state-prime-membrane-not-ready", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Ec_Install_To_FirstPrime_PreRole_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md");
        var ecPrepPath = Path.Combine(lineRoot, "docs", "EC_FORMATION_BUILDSPACE_PREPARATION_NOTE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var primeCrypticPath = Path.Combine(lineRoot, "docs", "PRIME_CRYPTIC_DUPLEX_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var ecPrepText = File.ReadAllText(ecPrepPath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var primeCrypticText = File.ReadAllText(primeCrypticPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("from install to first Prime pre-role standing", lawText, StringComparison.Ordinal);
        Assert.Contains("Mother/Father domain-role application remains withheld", lawText, StringComparison.Ordinal);
        Assert.Contains("EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md", ecPrepText, StringComparison.Ordinal);
        Assert.Contains("EngineeredCognitionFirstPrimeStateReceipt", firstRunText, StringComparison.Ordinal);
        Assert.Contains("EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md", primeCrypticText, StringComparison.Ordinal);
        Assert.Contains("ec-install-to-first-prime-state-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("EC install-to-first-Prime pre-role state preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("governing domain and role selection beyond first Prime pre-role standing", refinementText, StringComparison.Ordinal);
    }

    private static FirstRunConstitutionReceipt CreateFirstRunReceipt(
        PostIngressDiscernmentReceipt postIngressDiscernment)
    {
        var livingPacket = new FirstRunLivingAgentiCorePacket(
            PacketHandle: "packet://first-run/living-agenticore/session-a",
            LivingAgentiCoreHandle: "agenticore://living/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            ZedOfDeltaHandle: "zed://delta/session-a",
            SelfGelAttachmentHandle: "selfgel://attachment/session-a",
            ToolUseContextHandle: "tools://context/session-a",
            CompassEmbodimentHandle: "compass://embodiment/session-a",
            EngineeredCognitionHandle: "ec://session-a",
            WiderPublicWideningWithheld: true,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 16, 00, 00, TimeSpan.Zero),
            ListeningFrameProjectionPacket: CreateListeningFrameProjectionPacket(),
            CompassProjectionPacket: null,
            ListeningFrameInstrumentationReceipt: null,
            ZedDeltaSelfBasisReceipt: CreateZedDeltaSelfBasisReceipt(),
            ThetaIngressSensoryClusterReceipt: CreateThetaIngressReceipt(),
            PostIngressDiscernmentReceipt: postIngressDiscernment);

        return new FirstRunConstitutionReceipt(
            ReceiptHandle: "receipt://first-run/session-a",
            SnapshotHandle: "snapshot://first-run/session-a",
            LocalAuthorityTraceHandle: "local-authority://session-a",
            ConstitutionalContactHandle: "constitutional-contact://session-a",
            LocalKeypairGenesisSourceHandle: "keypair-source://session-a",
            LocalKeypairGenesisHandle: "keypair://session-a",
            FirstCrypticBraidEstablishmentHandle: "cryptic-braid-establishment://session-a",
            FirstCrypticBraidHandle: "cryptic-braid://session-a",
            FirstCrypticConditioningSourceHandle: "cryptic-conditioning-source://session-a",
            FirstCrypticConditioningHandle: "cryptic-conditioning://session-a",
            CurrentState: FirstRunConstitutionState.FoundationsEstablished,
            ReadinessState: FirstRunOperatorReadinessState.OperatorTrainingReady,
            CurrentStateProvisional: false,
            CurrentStateActualized: true,
            OpalActualized: false,
            ActiveFailureClasses: [],
            PromotionGates: [],
            SourceReason: "first-run-current-state:FoundationsEstablished",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 16, 05, 00, TimeSpan.Zero),
            ProtocolizationPacket: null,
            StewardWitnessedOePacket: null,
            ElementalBindingPacket: null,
            ActualizationSealPacket: null,
            LivingAgentiCorePacket: livingPacket);
    }

    private static PostIngressDiscernmentReceipt CreatePostIngressDiscernmentReceipt(
        bool stableOneAchieved)
    {
        return PostIngressDiscernmentEvaluator.Evaluate(
            CreateThetaIngressReceipt(),
            stableOneAchieved: stableOneAchieved,
            stableOneHandle: stableOneAchieved
                ? "stable-one://thread/session-a"
                : null,
            discernmentSignals: stableOneAchieved
                ? [PostIngressDiscernmentSignalKind.None]
                : [PostIngressDiscernmentSignalKind.Ambiguity],
            questionHandles: stableOneAchieved
                ? []
                : ["question://thread/session-a/resolve-ambiguity"],
            enrichmentHandles: [],
            carriedIncompleteHandles: [],
            receiptHandle: stableOneAchieved
                ? "receipt://post-ingress-discernment/stable"
                : "receipt://post-ingress-discernment/investigatory");
    }

    private static ThetaIngressSensoryClusterReceipt CreateThetaIngressReceipt()
    {
        return ThetaIngressSensoryClusterEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            CreateZedDeltaSelfBasisReceipt(),
            thetaHandle: "theta://thread/session-a",
            thetaMarkers:
            [
                "theta:live-thread",
                "theta:crosses-center"
            ],
            receiptHandle: "receipt://theta-ingress/session-a");
    }

    private static ZedDeltaSelfBasisReceipt CreateZedDeltaSelfBasisReceipt()
    {
        return ZedDeltaSelfBasisEvaluator.Evaluate(
            CreateListeningFrameProjectionPacket(),
            soulFrameHandle: "soulframe://session-a",
            oeHandle: "oe://session-a",
            selfGelHandle: "selfgel://session-a",
            cOeHandle: "coe://session-a",
            cSelfGelHandle: "cselfgel://session-a",
            zedOfDeltaHandle: "zed://delta/session-a",
            engineeredCognitionHandle: "ec://session-a",
            ecIuttLispMatrixHandle: "iutt-lisp://matrix/session-a",
            receiptHandle: "receipt://zed-delta-self-basis/session-a");
    }

    private static ListeningFrameProjectionPacket CreateListeningFrameProjectionPacket()
    {
        return new ListeningFrameProjectionPacket(
            PacketHandle: "packet://listening-frame/session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            ChamberHandle: "soulframe://session-a",
            SourceSurfaceHandle: "agenticore://session-a",
            VisibilityPosture: ListeningFrameVisibilityPosture.OperatorGuarded,
            IntegrityState: ListeningFrameIntegrityState.Usable,
            ReviewPosture: ListeningFrameReviewPosture.CandidateOnly,
            UsableForCompassProjection: true,
            PostureMarkers: ["theta:approaching"],
            ReviewNotes: ["candidate-only-posture-surface"],
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 15, 55, 00, TimeSpan.Zero));
    }

    private static PrimeRetainedHistoryRecord CreatePrimeRetainedRecord(
        PrimeRetainedWholeKind desiredKind)
    {
        var receiptKind = desiredKind switch
        {
            PrimeRetainedWholeKind.ClosureCandidate => PrimeMembraneReceiptKind.ReturnBearingUnclosed,
            _ => PrimeMembraneReceiptKind.ReceiptedHistory
        };
        var visibleResidues = desiredKind == PrimeRetainedWholeKind.RetainedPartial
            ? CreateResidues(1)
            : CreateResidues(3);

        return PrimeRetainedWholeEvaluator.Evaluate(
            new PrimeMembraneHistoryReceipt(
                ReceiptHandle: "receipt://prime-membrane/history/session-a",
                HistoryHandle: "history://prime-membrane/session-a",
                MembraneHandle: "membrane://prime/session-a",
                ProjectionHandle: "projection://cryptic/session-a",
                Interpretation: receiptKind == PrimeMembraneReceiptKind.ReturnBearingUnclosed
                    ? PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate
                    : PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
                ReceiptKind: receiptKind,
                AdvisoryClosureEligibility: receiptKind == PrimeMembraneReceiptKind.ReturnBearingUnclosed
                    ? PrimeClosureEligibilityKind.EligibleForMembraneReceipt
                    : PrimeClosureEligibilityKind.ReviewRequired,
                PreservedDistinctionVisible: true,
                RetainedWholenessStillWithheld: true,
                PrimeClosureStillWithheld: true,
                VisibleLineResidues: visibleResidues,
                DeferredLineResidues: [],
                ConstraintCodes: ["prime-membrane-history-receipt-not-closure"],
                ReasonCode: "history-receipt-test",
                LawfulBasis: "history receipt test basis",
                TimestampUtc: new DateTimeOffset(2026, 04, 15, 16, 10, 00, TimeSpan.Zero)),
            $"record://prime-retained-whole/{desiredKind.ToString().ToLowerInvariant()}");
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> CreateResidues(
        int count)
    {
        return Enumerable.Range(1, count)
            .Select(index => new PrimeMembraneProjectedLineResidue(
                LineHandle: $"line://{index}",
                SourceSurfaceHandle: $"surface://{index}",
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: index % 2 == 0
                    ? PrimeMembraneProjectedParticipationKind.Swarmed
                    : PrimeMembraneProjectedParticipationKind.Clustered,
                AcceptedContributionHandles: [$"contribution://{index}"],
                DistinctionPreserved: true,
                ResidueNotes: ["per-line-distinction-preserved"]))
            .ToArray();
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
