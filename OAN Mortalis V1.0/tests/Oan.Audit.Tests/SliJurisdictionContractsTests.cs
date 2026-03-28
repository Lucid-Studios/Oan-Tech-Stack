namespace Oan.Audit.Tests;

using Oan.Common;

public sealed class SliJurisdictionContractsTests
{
    [Fact]
    public void DeterministicReasonCodes_RemainStable()
    {
        string[] expected =
        [
            "jurisdiction-actualized-first-boot-formed",
            "jurisdiction-industrialized-ingress-candidate",
            "jurisdiction-envelope-preformalized",
            "jurisdiction-envelope-unreachable-surface",
            "jurisdiction-envelope-witness-only",
            "jurisdiction-transition-actualized-to-industrialized-allowed",
            "jurisdiction-transition-missing-operator-formation",
            "jurisdiction-transition-bridge-not-ok",
            "jurisdiction-transition-runtime-not-candidate-only",
            "jurisdiction-transition-industrialized-to-civic-allowed",
            "jurisdiction-transition-civic-reveal-widening-refused",
            "jurisdiction-transition-private-evidence-missing",
            "jurisdiction-transition-government-oversight-missing",
            "jurisdiction-transition-special-policy-missing",
            "jurisdiction-transition-unlisted-refused",
            "jurisdiction-governance-event-recorded",
            "jurisdiction-withdrawal-governance-retained",
            "jurisdiction-hostile-payload-not-identity-forming"
        ];

        string[] actual =
        [
            SliJurisdictionContracts.ReasonActualizedFirstBootFormed,
            SliJurisdictionContracts.ReasonIndustrializedIngressCandidate,
            SliJurisdictionContracts.ReasonEnvelopePreformalized,
            SliJurisdictionContracts.ReasonEnvelopeUnreachableSurface,
            SliJurisdictionContracts.ReasonEnvelopeWitnessOnly,
            SliJurisdictionContracts.ReasonTransitionActualizedToIndustrializedAllowed,
            SliJurisdictionContracts.ReasonTransitionMissingOperatorFormation,
            SliJurisdictionContracts.ReasonTransitionBridgeNotOk,
            SliJurisdictionContracts.ReasonTransitionRuntimeNotCandidateOnly,
            SliJurisdictionContracts.ReasonTransitionIndustrializedToCivicAllowed,
            SliJurisdictionContracts.ReasonTransitionCivicRevealWideningRefused,
            SliJurisdictionContracts.ReasonTransitionPrivateEvidenceMissing,
            SliJurisdictionContracts.ReasonTransitionGovernmentOversightMissing,
            SliJurisdictionContracts.ReasonTransitionSpecialPolicyMissing,
            SliJurisdictionContracts.ReasonTransitionUnlistedRefused,
            SliJurisdictionContracts.ReasonGovernanceEventRecorded,
            SliJurisdictionContracts.ReasonWithdrawalGovernanceRetained,
            SliJurisdictionContracts.ReasonHostilePayloadNotIdentityForming
        ];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InitialInvariantSpine_RemainsStable()
    {
        string[] expected =
        [
            "identity-continuity-unchanged",
            "bond-realization-unchanged",
            "bridge-legality-remains-primary",
            "witness-only-does-not-authorize",
            "reveal-ceiling-may-not-widen-without-explicit-authority",
            "subordinate-cme-authorization-unchanged-unless-separately-authorized",
            "retention-burden-may-only-harden-on-stricter-surfaces",
            "no-local-runtime-inference-of-surface-promotion"
        ];

        Assert.Equal(expected, SliJurisdictionContracts.InitialInvariantSpine);
    }

    [Fact]
    public void EvaluateTransition_IndustrializedToCivic_RequiresCommunitySafeReduction()
    {
        var sourceEnvelope = CreateIndustrializedEnvelope();

        var transition = SliJurisdictionContracts.EvaluateTransition(
            sourceEnvelope,
            SliJurisdictionSurfaceClass.Civic);

        Assert.Equal(SliJurisdictionTransitionDecision.Refuse, transition.Decision);
        Assert.Equal(SliJurisdictionContracts.ReasonTransitionCivicRevealWideningRefused, transition.ReasonCode);
        Assert.True(transition.GovernanceEventRetained);
        Assert.False(transition.IdentityFormingRetentionAllowed);
        Assert.Equal(
            [
                SliJurisdictionContracts.ReasonGovernanceEventRecorded,
                SliJurisdictionContracts.ReasonWithdrawalGovernanceRetained,
                SliJurisdictionContracts.ReasonHostilePayloadNotIdentityForming
            ],
            transition.GovernanceEventReasonCodes);
    }

    [Fact]
    public void ProjectCivicEnvelope_CommunityLegibleReduction_IsPublicSafe()
    {
        var sourceEnvelope = CreateIndustrializedEnvelope();

        var envelope = SliJurisdictionContracts.ProjectCivicEnvelope(
            sourceEnvelope,
            new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: new DateTimeOffset(2026, 3, 19, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(SliJurisdictionSurfaceClass.Civic, envelope.SurfaceClass);
        Assert.Equal(PrimeRevealMode.MaskedSummary, envelope.RevealModeCeiling);
        Assert.Equal(SliJurisdictionOversightRequirement.StewardReview, envelope.OversightRequirement);
        Assert.Equal(SliJurisdictionRetentionClass.GovernanceEventOnly, envelope.RetentionClass);
    }

    [Fact]
    public void EvaluateTransition_IndustrializedToPrivate_HoldsWithoutPartitionEvidence()
    {
        var transition = SliJurisdictionContracts.EvaluateTransition(
            CreateIndustrializedEnvelope(),
            SliJurisdictionSurfaceClass.Private);

        Assert.Equal(SliJurisdictionTransitionDecision.Hold, transition.Decision);
        Assert.Equal(SliJurisdictionContracts.ReasonTransitionPrivateEvidenceMissing, transition.ReasonCode);
    }

    [Fact]
    public void ProjectPrivateEnvelope_WithPartitionEvidence_ProjectsPrivate()
    {
        var envelope = SliJurisdictionContracts.ProjectPrivateEnvelope(
            CreateIndustrializedEnvelope(),
            "private-custody://partition/a1");

        Assert.Equal(SliJurisdictionSurfaceClass.Private, envelope.SurfaceClass);
        Assert.Equal(SliJurisdictionAuditDepth.Deep, envelope.AuditDepth);
        Assert.Equal(SliJurisdictionOversightRequirement.InstitutionalReview, envelope.OversightRequirement);
        Assert.Equal(SliJurisdictionRetentionClass.ProtectedReviewLedger, envelope.RetentionClass);
    }

    [Fact]
    public void EvaluateTransition_IndustrializedToGovernment_HoldsWithoutOversightEvidence()
    {
        var transition = SliJurisdictionContracts.EvaluateTransition(
            CreateIndustrializedEnvelope(),
            SliJurisdictionSurfaceClass.Government);

        Assert.Equal(SliJurisdictionTransitionDecision.Hold, transition.Decision);
        Assert.Equal(SliJurisdictionContracts.ReasonTransitionGovernmentOversightMissing, transition.ReasonCode);
    }

    [Fact]
    public void ProjectGovernmentEnvelope_WithOversightEvidence_ProjectsGovernment()
    {
        var envelope = SliJurisdictionContracts.ProjectGovernmentEnvelope(
            CreateIndustrializedEnvelope(),
            jurisdictionMappingHandle: "jurisdiction://gov/us-west",
            regulatedOversightHandle: "oversight://regulated/human");

        Assert.Equal(SliJurisdictionSurfaceClass.Government, envelope.SurfaceClass);
        Assert.Equal(SliJurisdictionAuditDepth.Maximal, envelope.AuditDepth);
        Assert.Equal(SliJurisdictionOversightRequirement.RegulatedHumanReview, envelope.OversightRequirement);
        Assert.Equal(SliJurisdictionRetentionClass.ComplianceRetention, envelope.RetentionClass);
    }

    [Fact]
    public void EvaluateTransition_AnyToSpecial_DefaultsToRefuse()
    {
        var transition = SliJurisdictionContracts.EvaluateTransition(
            CreateIndustrializedEnvelope(),
            SliJurisdictionSurfaceClass.Special);

        Assert.Equal(SliJurisdictionTransitionDecision.Refuse, transition.Decision);
        Assert.Equal(SliJurisdictionContracts.ReasonTransitionSpecialPolicyMissing, transition.ReasonCode);
    }

    private static SliJurisdictionEnvelopeReceipt CreateIndustrializedEnvelope()
    {
        var governanceLayer = new FirstBootGovernanceLayerReceipt(
            LayerHandle: "first-boot-governance://corporategoverned/triadicactive",
            BootClass: BootClass.CorporateGoverned,
            ActivationState: BootActivationState.TriadicActive,
            State: FirstBootGovernanceLayerState.RoleBoundEceReady,
            ExpansionRights: ExpansionRights.None,
            SwarmEligibility: SwarmEligibility.Denied,
            WitnessOnly: true,
            SubordinateCmeAuthorizationAllowed: false,
            RoleBoundEcesReady: true,
            FormedOffices:
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            RoleBoundEces:
            [
                new FirstBootRoleBoundEceReceipt(
                    EceHandle: "ece://steward",
                    Office: InternalGoverningCmeOffice.Steward,
                    FormationOrdinal: 1,
                    State: RoleBoundEceState.RoleBoundTestingReady,
                    RoleBoundaryHandle: "boundary://steward",
                    VisibilityScopes:
                    [
                        GoverningOfficeVisibilityScope.CustodyClass
                    ],
                    RequiredPriorOffices: [],
                    WitnessOnly: true,
                    PrimeRevealWideningAllowed: false,
                    ExpansionAuthorizationAllowed: false,
                    ReasonCode: "role-bound-ece-testing-ready")
            ],
            ReasonCode: "first-boot-governance-layer-role-bound-ece-ready");

        var bridgeReview = SliBridgeContracts.CreateReview(
            bridgeStage: "jurisdiction-test",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: "sli-bridge://jurisdiction/test",
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-within-band",
            operatorFormation: SliBridgeContracts.CreatePreBondOperatorFormationReceipt(
                formationHandle: "operator-formation://prebond/jurisdiction",
                boundaryCrossingMode: SliOperatorFormationBoundaryCrossingMode.InterlacedBondedCrossing,
                profile: new SliOperatorFormationProfileReceipt(
                    ProfileId: "gs_profile_obsidian_guarded",
                    Lane: SliOperatorFormationLane.GnomeSpeakNlpSquared,
                    ChapterLocalSurface: "chapter://obsidian",
                    PairedTrainingSurface: "training://obsidian",
                    CrossingTaskKind: "evidence_review",
                    HaltOwner: "reviewer://bonded",
                    Ring: SliOperatorFormationRing.Rootseed,
                    ActiveMode: SliOperatorFormationMode.Stillness,
                    StillnessInterludeUsed: false,
                    RedHatIndexRequired: true,
                    BondStatus: SliOperatorFormationBondStatus.TrainingOperator,
                    EchoVeilCheckRequired: true,
                    ActiveConflictClass: SliOperatorFormationConflictClass.None,
                    GjpNeeded: false,
                    GjpVerdict: SliOperatorFormationGjpVerdict.NotApplicable,
                    MotherLightAnchored: true,
                    FatherEchoAnchored: true,
                    ShellRootAnchored: true,
                    SeedBoundAnchored: true,
                    U230ShadowScript: SliOperatorFormationConcealmentLayerState.Observed,
                    U300ElvenScript: SliOperatorFormationConcealmentLayerState.Observed,
                    ExpectedEvidenceArtifact: "interlace_crossing_proof",
                    AdmissibleOutput: "bounded_training_profile",
                    ProhibitedOutputs: ["bond actualized"]),
                certificationPosture: SliBridgeContracts.CreateOperatorFormationCertificationReceipt(
                    decision: SliOperatorFormationCertificationDecision.Pending,
                    currentAnchoredPosture: SliOperatorFormationBondStatus.TrainingOperator,
                    targetPosture: SliOperatorFormationBondStatus.PreCertifiedOperator,
                    nearestAdmissibleNextPosture: SliOperatorFormationBondStatus.VerifiedCandidate,
                    reviewOwner: "reviewer://bonded",
                    evidenceGaps: ["trial_receipt_set"],
                    prohibitedClaims: ["bond actualized"],
                    certificationIssued: false,
                    expandedRevealAllowed: false,
                    continuityClaimAllowed: false,
                    requiredBondedStandard: "first_run_bonding://minimum",
                    blockingConditions: ["trial-evidence-missing"],
                    nextActions: ["Complete the current trial block."],
                    haltOwner: "reviewer://bonded",
                    haltCondition: "Halt if lineage breaks.",
                    reentryRule: "Reenter after the trial receipts are attached.")));

        return SliJurisdictionContracts.ProjectProtectedIngressEnvelope(
            governanceLayer,
            bridgeReview,
            SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling(),
            bridgeReview.OperatorFormation);
    }
}
