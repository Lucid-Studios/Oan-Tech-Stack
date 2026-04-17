using San.Common;
using San.Nexus.Control;
using SLI.Engine;

namespace San.Audit.Tests;

public sealed class GovernedSeedPreDomainHostLoopTests
{
    [Fact]
    public void CrypticHolding_Stays_CandidateOnly_And_InspectionOnly()
    {
        var service = new GovernedSeedCrypticHoldingService();
        var formationReceipt = CreateFormationReceipt(
            lawfulKnownItems: [],
            retainedUnknownItems: ["candidate-a", "candidate-b"],
            deferred: true,
            refused: false);
        var listeningFrame = CreateListeningFrame();
        var compass = CreateCompass(CompassAdmissibilityEstimate.CandidateOnly, CompassTransitionRecommendation.Hold);

        var receipt = service.Inspect(formationReceipt, listeningFrame, compass);

        Assert.True(receipt.CandidateOnly);
        Assert.True(receipt.InspectionInfluenceOnly);
        Assert.True(receipt.PromotionAuthorityWithheld);
        Assert.NotEmpty(receipt.HoldingEntries);
        Assert.All(receipt.HoldingEntries, entry => Assert.True(entry.Construct.CandidateOnly));
    }

    [Fact]
    public void FormOrCleave_Cleaves_Multiple_Lawful_Candidates_Without_Promotion()
    {
        var service = new GovernedSeedFormOrCleaveService();
        var formationReceipt = CreateFormationReceipt(
            lawfulKnownItems: ["candidate-a", "candidate-b"],
            retainedUnknownItems: [],
            deferred: false,
            refused: false,
            promoteToKnownLawful: true);
        var listeningFrame = CreateListeningFrame();
        var compass = CreateCompass(CompassAdmissibilityEstimate.ProvisionallyAdmissible, CompassTransitionRecommendation.ProceedBounded);
        var holdingInspection = new GovernedSeedCrypticHoldingInspectionReceipt(
            ReceiptHandle: "holding://empty",
            FormationReceiptHandle: formationReceipt.ReceiptHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            CompassPacketHandle: compass.PacketHandle,
            HoldingEntries: [],
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: "empty",
            LawfulBasis: "empty",
            TimestampUtc: DateTimeOffset.UtcNow);

        var assessment = service.Evaluate(formationReceipt, listeningFrame, compass, holdingInspection);

        Assert.Equal(GovernedSeedFormOrCleaveDispositionKind.Cleave, assessment.Disposition);
        Assert.Equal(GovernedSeedCarryDispositionKind.Cleave, assessment.CarryDisposition);
        Assert.Equal(GovernedSeedCollapseDispositionKind.Cleave, assessment.CollapseDisposition);
        Assert.Equal(2, assessment.DescendantCandidates.Count);
        Assert.All(assessment.DescendantCandidates, candidate => Assert.True(candidate.CandidateOnly));
    }

    [Fact]
    public void PreDomainHostLoop_Stops_Cleanly_When_Seed_Is_Not_Ready()
    {
        var service = new GovernedSeedPreDomainHostLoopService(
            new GovernedSeedCrypticHoldingService(),
            new GovernedSeedFormOrCleaveService());
        var formationReceipt = CreateFormationReceipt(
            lawfulKnownItems: ["candidate-a"],
            retainedUnknownItems: ["candidate-b"],
            deferred: true,
            refused: false);
        var listeningFrame = CreateListeningFrame();
        var compass = CreateCompass(CompassAdmissibilityEstimate.CandidateOnly, CompassTransitionRecommendation.Hold);
        var firstPrimeReceipt = CreateFirstPrimeReceipt();
        var primeSeedReceipt = CreatePrimeSeedReceipt(PrimeSeedStateKind.SeedMaterialIncomplete);

        var result = service.Evaluate(formationReceipt, listeningFrame, compass, firstPrimeReceipt, primeSeedReceipt);

        Assert.False(result.HostLoopReceipt.SeedReady);
        Assert.Equal(GovernedSeedCarryDispositionKind.None, result.HostLoopReceipt.CarryDisposition);
        Assert.Equal(GovernedSeedCollapseDispositionKind.None, result.HostLoopReceipt.CollapseDisposition);
        Assert.True(result.HostLoopReceipt.CandidateOnly);
        Assert.True(result.HostLoopReceipt.DomainAdmissionWithheld);
        Assert.True(result.HostLoopReceipt.ActionAuthorityWithheld);
    }

    private static FormationReceipt CreateFormationReceipt(
        IReadOnlyList<string> lawfulKnownItems,
        IReadOnlyList<string> retainedUnknownItems,
        bool deferred,
        bool refused,
        bool promoteToKnownLawful = false)
    {
        var orientation = new SensoryOrientationSnapshot(
            OrientationHandle: "orientation://audit",
            ListeningFrameHandle: "listening-frame://audit",
            CompassEmbodimentHandle: "compass://audit",
            ConeBoundary: AwarenessConeBoundaryKind.Inside,
            OrientationFacet: CompassOrientationFacetKind.Center,
            PerceptualPolarity: PerceptualPolarityKind.Direct,
            SourceRelation: SourceRelationKind.Duplex,
            ModalityMarkers: ["audit"],
            OrientationNotes: ["audit"],
            TimestampUtc: DateTimeOffset.UtcNow);

        return new FormationReceipt(
            ReceiptHandle: "formation://audit",
            EncounterHandle: "encounter://audit",
            EngineeredCognitionHandle: "ec://audit",
            Orientation: orientation,
            DiscernedItems: lawfulKnownItems.Concat(retainedUnknownItems).ToArray(),
            RequirementStates: [],
            KnowledgePosture: promoteToKnownLawful
                ? FormationKnowledgePostureKind.KnownEnoughForNextAct
                : FormationKnowledgePostureKind.NeedsInspection,
            PromotionToKnownLawful: promoteToKnownLawful,
            LawfulKnownItems: lawfulKnownItems,
            RetainedUnknownItems: retainedUnknownItems,
            WhyNotClasses: [],
            FailureEvidenceDisposition: FailureEvidenceDispositionKind.None,
            RetainedFailureSignatures: [],
            OrientationEvidenceSignatures: [],
            SelectedNextLawfulMove: deferred
                ? NextLawfulMoveKind.Defer
                : NextLawfulMoveKind.RetainCurrentFooting,
            ReasonCode: "audit-formation",
            LawfulBasis: "audit-formation-basis",
            Deferred: deferred,
            Refused: refused,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static ListeningFrameProjectionPacket CreateListeningFrame()
    {
        return new ListeningFrameProjectionPacket(
            PacketHandle: "listening-frame-packet://audit",
            ListeningFrameHandle: "listening-frame://audit",
            ChamberHandle: "chamber://audit",
            SourceSurfaceHandle: "surface://audit",
            VisibilityPosture: ListeningFrameVisibilityPosture.OperatorGuarded,
            IntegrityState: ListeningFrameIntegrityState.Usable,
            ReviewPosture: ListeningFrameReviewPosture.CandidateOnly,
            UsableForCompassProjection: true,
            PostureMarkers: ["audit"],
            ReviewNotes: [],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static CompassProjectionPacket CreateCompass(
        CompassAdmissibilityEstimate admissibilityEstimate,
        CompassTransitionRecommendation transitionRecommendation)
    {
        return new CompassProjectionPacket(
            PacketHandle: "compass-packet://audit",
            CompassEmbodimentHandle: "compass://audit",
            ListeningFrameHandle: "listening-frame://audit",
            DriftState: CompassDriftState.Held,
            OrientationPosture: CompassOrientationPosture.Centered,
            AdmissibilityEstimate: admissibilityEstimate,
            TransitionRecommendation: transitionRecommendation,
            AuthorityPosture: CompassAuthorityPosture.CandidateOnly,
            CandidateInputs:
            [
                new CompassCandidateModulationInput("candidate-a", "candidate", "audit")
            ],
            ReviewNotes: [],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static EngineeredCognitionFirstPrimeStateReceipt CreateFirstPrimeReceipt()
    {
        return new EngineeredCognitionFirstPrimeStateReceipt(
            ReceiptHandle: "first-prime://audit",
            FirstRunReceiptHandle: "first-run://audit",
            PrimeRetainedRecordHandle: "prime-retained://audit",
            FirstRunState: FirstRunConstitutionState.FoundationsEstablished,
            FirstPrimeState: EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding,
            LivingAgentiCoreHandle: "living-agenticore://audit",
            ListeningFrameHandle: "listening-frame://audit",
            SoulFrameHandle: "soul-frame://audit",
            OeHandle: "oe://audit",
            SelfGelHandle: "self-gel://audit",
            COeHandle: "coe://audit",
            CSelfGelHandle: "cself-gel://audit",
            ZedOfDeltaHandle: "zed://audit",
            EngineeredCognitionHandle: "ec://audit",
            ThetaIngressReceiptHandle: "theta://audit",
            PostIngressDiscernmentReceiptHandle: "discernment://audit",
            StableOneHandle: "stable-one://audit",
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
            ConstraintCodes: ["audit"],
            ReasonCode: "audit",
            LawfulBasis: "audit",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt(PrimeSeedStateKind state)
    {
        return new PrimeSeedStateReceipt(
            ReceiptHandle: "prime-seed://audit",
            RequestHandle: "prime-seed-request://audit",
            FirstPrimeReceiptHandle: "first-prime://audit",
            PrimeRetainedRecordHandle: "prime-retained://audit",
            StableOneHandle: "stable-one://audit",
            SeedState: state,
            SeedSourceHandle: "seed-source://audit",
            SeedCarrierHandle: "seed-carrier://audit",
            SeedContinuityHandle: "seed-continuity://audit",
            SeedIntegrityHandle: "seed-integrity://audit",
            SeedEvidenceHandles: ["evidence://audit"],
            FirstPrimePreRoleStandingPresent: true,
            StableOnePresent: true,
            PrimeRetainedStandingPresent: true,
            SeedSourcePresent: true,
            SeedCarrierPresent: true,
            SeedContinuityPresent: state == PrimeSeedStateKind.PrimeSeedPreDomainStanding,
            SeedIntegrityPresent: state == PrimeSeedStateKind.PrimeSeedPreDomainStanding,
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
            ConstraintCodes: ["audit"],
            ReasonCode: "audit",
            LawfulBasis: "audit",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
