using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedWorkerReturnValidatorTests
{
    [Theory]
    [InlineData(WorkerCompletionState.Deferred, WorkerReasonCode.NeedsSpecification)]
    [InlineData(WorkerCompletionState.Denied, WorkerReasonCode.AuthorityDenied)]
    public void Validate_LawfulDeferredOrDeniedReturn_AcceptsPacket(
        WorkerCompletionState completionState,
        WorkerReasonCode reasonCode)
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            completionState: completionState,
            reasonCodes: [reasonCode, WorkerReasonCode.UnknownNotFailure]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.True(receipt.Validated);
        Assert.Null(receipt.ValidationFailureCode);
        Assert.Equal(ConstructClass.BoundedWorker, receipt.ConstructClass);
        Assert.Equal(returnPacket.WorkerPacketId, receipt.WorkerPacketId);
        Assert.Equal(returnPacket.CompletionState, receipt.CompletionState);
        Assert.Equal(returnPacket.ReasonCodes, receipt.ReasonCodes);
        Assert.Equal(handoffPacket.BridgeReview, receipt.BridgeReview);
        Assert.Equal(handoffPacket.RuntimeUseCeiling, receipt.RuntimeUseCeiling);
        Assert.Equal(handoffPacket.JurisdictionEnvelope, receipt.JurisdictionEnvelope);
    }

    [Fact]
    public void Validate_ReturnReasonCodeVocabulary_MatchesProtocolSeedSet()
    {
        string[] expected =
        [
            nameof(WorkerReasonCode.NeedsSpecification),
            nameof(WorkerReasonCode.InsufficientEvidence),
            nameof(WorkerReasonCode.DeferredReview),
            nameof(WorkerReasonCode.AuthorityDenied),
            nameof(WorkerReasonCode.DisclosureScopeViolation),
            nameof(WorkerReasonCode.NoHandleNoAction),
            nameof(WorkerReasonCode.UnsupportedClaim),
            nameof(WorkerReasonCode.BrokenWindow),
            nameof(WorkerReasonCode.UnknownNotFailure),
            nameof(WorkerReasonCode.OfficeNonOverlap),
            nameof(WorkerReasonCode.PromptInjection),
            nameof(WorkerReasonCode.PredatorySharedDomainRisk),
            nameof(WorkerReasonCode.CoerciveBondingPosture),
            nameof(WorkerReasonCode.ContinuityInstability),
            nameof(WorkerReasonCode.IdentityOvercollapseRisk)
        ];

        Assert.Equal(expected, Enum.GetNames<WorkerReasonCode>());
    }

    [Fact]
    public void Validate_UnknownReasonCode_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            reasonCodes: [(WorkerReasonCode)999]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("unknown-reason-code", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_DisclosureWidening_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket(returnVisibilityClass: CompassVisibilityClass.CommunityLegible);
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            disclosureClass: CompassVisibilityClass.OperatorGuarded);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("disclosure-ceiling-widened", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_CompletedReturnWithoutEvidence_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            completionState: WorkerCompletionState.Completed,
            evidenceHandles: []);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("missing-evidence-handle", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_ExecutionClaim_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            executionClaimed: true,
            reasonCodes: [WorkerReasonCode.UnsupportedClaim]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("unsupported-execution-claim", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_MutationClaim_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            mutationClaimed: true,
            reasonCodes: [WorkerReasonCode.UnsupportedClaim]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("unsupported-mutation-claim", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_ProhibitedActionAttempt_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            prohibitedActionAttempts: ["undeclared-tool-call"],
            reasonCodes: [WorkerReasonCode.PromptInjection]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("prohibited-action-attempt", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_RuntimeCeilingWidening_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            runtimeUseCeiling: new SliRuntimeUseCeilingReceipt(
                CandidateOnly: false,
                PersistenceAuthorityGranted: true,
                DeploymentAuthorityGranted: false,
                HaltAuthorityGranted: false,
                ReasonCode: "sli-runtime-authority-granted"));

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("runtime-ceiling-widened", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_ProtectiveReasonCannotComplete_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket();
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            completionState: WorkerCompletionState.Completed,
            evidenceHandles: ["evidence://one"],
            reasonCodes: [WorkerReasonCode.PredatorySharedDomainRisk]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("protective-reason-cannot-complete", receipt.ValidationFailureCode);
    }

    [Fact]
    public void Validate_OperatorFormationBridgeReview_PreservesReceipt()
    {
        var handoffPacket = CreateHandoffPacket(includeOperatorFormation: true);
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            completionState: WorkerCompletionState.Deferred,
            reasonCodes: [WorkerReasonCode.DeferredReview]);

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.True(receipt.Validated);
        Assert.NotNull(receipt.BridgeReview?.OperatorFormation);
        Assert.Equal(
            handoffPacket.BridgeReview!.OperatorFormation,
            receipt.BridgeReview!.OperatorFormation);
    }

    [Fact]
    public void Validate_AllowedJurisdictionTransition_AcceptsPacket()
    {
        var handoffPacket = CreateHandoffPacket(
            includeOperatorFormation: true,
            jurisdictionEnvelope: CreateActualizedEnvelope());
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            evidenceHandles: ["evidence://one"],
            reasonCodes: [WorkerReasonCode.NeedsSpecification],
            jurisdictionEnvelope: CreateIndustrializedEnvelope());

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.True(receipt.Validated);
        Assert.NotNull(receipt.JurisdictionTransition);
        Assert.Equal(SliJurisdictionTransitionDecision.Allow, receipt.JurisdictionTransition!.Decision);
        Assert.Equal(
            SliJurisdictionContracts.ReasonTransitionActualizedToIndustrializedAllowed,
            receipt.JurisdictionTransition.ReasonCode);
        Assert.Equal(SliJurisdictionSurfaceClass.Industrialized, receipt.JurisdictionEnvelope!.SurfaceClass);
    }

    [Fact]
    public void Validate_MissingOperatorFormationJurisdictionTransition_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket(jurisdictionEnvelope: CreateActualizedEnvelope());
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            jurisdictionEnvelope: CreateIndustrializedEnvelope());

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("jurisdiction-transition-not-allowed", receipt.ValidationFailureCode);
        Assert.NotNull(receipt.JurisdictionTransition);
        Assert.Equal(SliJurisdictionTransitionDecision.Hold, receipt.JurisdictionTransition!.Decision);
        Assert.Equal(
            SliJurisdictionContracts.ReasonTransitionMissingOperatorFormation,
            receipt.JurisdictionTransition.ReasonCode);
    }

    [Fact]
    public void Validate_CivicWideningWithoutReduction_RejectsPacket()
    {
        var handoffPacket = CreateHandoffPacket(
            includeOperatorFormation: true,
            jurisdictionEnvelope: CreateIndustrializedEnvelope());
        var handoffReceipt = CreateHandoffReceipt(handoffPacket);
        var returnPacket = CreateReturnPacket(
            handoffPacket,
            jurisdictionEnvelope: CreateCivicEnvelope());

        var receipt = GovernedWorkerReturnValidator.Validate(
            "loop:worker-return:test",
            "cme-worker-return",
            GovernanceLoopStage.BoundedCognitionCompleted,
            handoffPacket,
            handoffReceipt,
            returnPacket);

        Assert.False(receipt.Validated);
        Assert.Equal("jurisdiction-transition-not-allowed", receipt.ValidationFailureCode);
        Assert.NotNull(receipt.JurisdictionTransition);
        Assert.Equal(SliJurisdictionTransitionDecision.Refuse, receipt.JurisdictionTransition!.Decision);
        Assert.Equal(
            SliJurisdictionContracts.ReasonTransitionCivicRevealWideningRefused,
            receipt.JurisdictionTransition.ReasonCode);
    }

    private static WorkerHandoffPacket CreateHandoffPacket(
        CompassVisibilityClass returnVisibilityClass = CompassVisibilityClass.OperatorGuarded,
        bool includeOperatorFormation = false,
        SliJurisdictionEnvelopeReceipt? jurisdictionEnvelope = null)
    {
        var bridgeReview = SliBridgeContracts.CreateReview(
            bridgeStage: "worker-return-validator-test",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: "agenticore-return://candidate/test",
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-within-band",
            operatorFormation: includeOperatorFormation ? CreateOperatorFormationReceipt() : null);

        return new WorkerHandoffPacket(
            HandoffPacketId: "worker-handoff-packet://aaaaaaaaaaaaaaaa",
            RequestingOffice: InternalGoverningCmeOffice.Steward,
            RequestingOfficeInstanceId: "office-instance://steward-bbbbbbbbbbbbbbbb",
            AuthorizingSurface: "host_truth_runtime",
            WorkerSpecies: GovernedWorkerSpecies.RepoBugStewardWorker,
            WorkerInstanceMode: WorkerInstanceMode.RequestOnly,
            Objective: "candidate-return-evaluation",
            TaskKind: "repo-bug-triage",
            SourceHandles:
            [
                "agenticore-return://candidate/test",
                "office-authority://cccccccccccccccc",
                "office-issuance://dddddddddddddddd"
            ],
            RequiredOutputKind: "worker-return-summary-v1",
            DeadlineOrExpiry: "loop-scoped",
            HaltConditions:
            [
                "authority-missing",
                "disclosure-ceiling-breach",
                "evidence-insufficient"
            ],
            ActionCeiling: OfficeActionEligibility.CheckInAllowed,
            DisclosureClass: CompassVisibilityClass.OperatorGuarded,
            AllowedReasonCodes:
            [
                WorkerReasonCode.NeedsSpecification,
                WorkerReasonCode.InsufficientEvidence,
                WorkerReasonCode.DeferredReview,
                WorkerReasonCode.AuthorityDenied,
                WorkerReasonCode.DisclosureScopeViolation,
                WorkerReasonCode.NoHandleNoAction,
                WorkerReasonCode.UnsupportedClaim,
                WorkerReasonCode.BrokenWindow,
                WorkerReasonCode.UnknownNotFailure,
                WorkerReasonCode.OfficeNonOverlap,
                WorkerReasonCode.PromptInjection,
                WorkerReasonCode.PredatorySharedDomainRisk,
                WorkerReasonCode.CoerciveBondingPosture,
                WorkerReasonCode.ContinuityInstability,
                WorkerReasonCode.IdentityOvercollapseRisk
            ],
            ProhibitedActions:
            [
                "public-disclosure",
                "host-mutation",
                "undeclared-tool-call"
            ],
            PublicationDenial: "public-disclosure-not-authorized",
            MutationDenial: "host-mutation-not-authorized",
            MountedMemoryLanes:
            [
                "mission-local"
            ],
            ForbiddenMemoryLanes:
            [
                "cryptic-sealed"
            ],
            ToolAllowlist:
            [
                "repo-read-only",
                "receipt-inspection"
            ],
            ToolDenials:
            [
                "network-call",
                "mutation",
                "publication"
            ],
            ContinuityLinkageRequirement: "office-issuance-lineage-link",
            ResidueReturnRequirement: "required-before-completion",
            WitnessRequired: true,
            RequiredWitnessSurface: "host-truth-witness",
            ReturnPacketSchema: "worker-return-packet-v1",
            ReturnDestination: "steward-governance-loop",
            ReturnVisibilityClass: returnVisibilityClass,
            ResidueDisposition: WorkerResidueDisposition.NeedsClassification,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sufficient,
            MaturityPosture: MaturityPosture.DoctrineBacked,
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 12, 0, 0, TimeSpan.Zero),
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling(),
            JurisdictionEnvelope: jurisdictionEnvelope);
    }

    private static SliOperatorFormationReceipt CreateOperatorFormationReceipt()
    {
        return SliBridgeContracts.CreatePreBondOperatorFormationReceipt(
            formationHandle: "operator-formation://prebond/bbbbbbbbbbbbbbbb",
            boundaryCrossingMode: SliOperatorFormationBoundaryCrossingMode.InterlacedBondedCrossing,
            profile: new SliOperatorFormationProfileReceipt(
                ProfileId: "gs_profile_obsidian_guarded",
                Lane: SliOperatorFormationLane.GnomeSpeakNlpSquared,
                ChapterLocalSurface: "research/publications/gnomeronacorde-v0.1/source/1_OBSIDIAN_WALL/1d_The_Ritual_of_Absence.tex",
                PairedTrainingSurface: "research/publications/gnome-speak-nlp-v1.0/source/sections/conflict.tex",
                CrossingTaskKind: "evidence_review",
                HaltOwner: "first_run_bonded_reviewer",
                Ring: SliOperatorFormationRing.Rootseed,
                ActiveMode: SliOperatorFormationMode.Stillness,
                StillnessInterludeUsed: false,
                RedHatIndexRequired: true,
                BondStatus: SliOperatorFormationBondStatus.TrainingOperator,
                EchoVeilCheckRequired: true,
                ActiveConflictClass: SliOperatorFormationConflictClass.IdentityCollision,
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
                ProhibitedOutputs: ["unauthorized_gjp_invocation"]),
            certificationPosture: SliBridgeContracts.CreateOperatorFormationCertificationReceipt(
                decision: SliOperatorFormationCertificationDecision.Pending,
                currentAnchoredPosture: SliOperatorFormationBondStatus.TrainingOperator,
                targetPosture: SliOperatorFormationBondStatus.PreCertifiedOperator,
                nearestAdmissibleNextPosture: SliOperatorFormationBondStatus.VerifiedCandidate,
                reviewOwner: "certification_reviewer://first-run-lane",
                evidenceGaps: ["trial_receipt_set"],
                prohibitedClaims: ["pre-certification issued"],
                certificationIssued: false,
                expandedRevealAllowed: false,
                continuityClaimAllowed: false,
                requiredBondedStandard: "first_run_bonding://precertification/minimum-admissible-evidence",
                blockingConditions: ["incomplete_trial_evidence"],
                nextActions: ["Complete the current Gnome Speak trial block."],
                haltOwner: "certification_reviewer://first-run-lane",
                haltCondition: "Halt if evidence lineage breaks.",
                reentryRule: "Reenter after the trial receipt set is attached.",
                linkedVerificationRecord: "first_bonding://verification/pending",
                gateArtifact: "verification record"));
    }

    private static GovernedWorkerHandoffReceipt CreateHandoffReceipt(WorkerHandoffPacket handoffPacket)
    {
        return new GovernedWorkerHandoffReceipt(
            HandoffHandle: "worker-handoff://eeeeeeeeeeeeeeee",
            LoopKey: "loop:worker-return:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-worker-return",
            RequestingOffice: handoffPacket.RequestingOffice,
            RequestingOfficeInstanceId: handoffPacket.RequestingOfficeInstanceId,
            ConstructClass: ConstructClass.BoundedWorker,
            WorkerSpecies: handoffPacket.WorkerSpecies,
            WorkerInstanceMode: handoffPacket.WorkerInstanceMode,
            ActionCeiling: handoffPacket.ActionCeiling,
            DisclosureClass: handoffPacket.DisclosureClass,
            EvidenceSufficiencyState: handoffPacket.EvidenceSufficiencyState,
            MaturityPosture: handoffPacket.MaturityPosture,
            HandoffPacketId: handoffPacket.HandoffPacketId,
            OfficeIssuanceHandle: "office-issuance://dddddddddddddddd",
            OfficeAuthorityHandle: "office-authority://cccccccccccccccc",
            WeatherDisclosureHandle: "weather-disclosure://ffffffffffffffff",
            WitnessedBy: "CradleTek",
            TimestampUtc: handoffPacket.TimestampUtc,
            BridgeReview: handoffPacket.BridgeReview,
            RuntimeUseCeiling: handoffPacket.RuntimeUseCeiling,
            JurisdictionEnvelope: handoffPacket.JurisdictionEnvelope);
    }

    private static WorkerReturnPacket CreateReturnPacket(
        WorkerHandoffPacket handoffPacket,
        WorkerCompletionState completionState = WorkerCompletionState.Deferred,
        IReadOnlyList<string>? evidenceHandles = null,
        IReadOnlyList<WorkerReasonCode>? reasonCodes = null,
        IReadOnlyList<string>? unsupportedClaimFlags = null,
        IReadOnlyList<string>? prohibitedActionAttempts = null,
        WorkerResidueDisposition residueDisposition = WorkerResidueDisposition.NeedsClassification,
        CompassVisibilityClass? disclosureClass = null,
        bool executionClaimed = false,
        bool mutationClaimed = false,
        SliBridgeReviewReceipt? bridgeReview = null,
        SliRuntimeUseCeilingReceipt? runtimeUseCeiling = null,
        SliJurisdictionEnvelopeReceipt? jurisdictionEnvelope = null)
    {
        return new WorkerReturnPacket(
            WorkerPacketId: "worker-return-packet://1111111111111111",
            HandoffPacketId: handoffPacket.HandoffPacketId,
            WorkerSpecies: handoffPacket.WorkerSpecies,
            CompletionState: completionState,
            ResultSummary: "worker-return-summary-v1",
            EvidenceHandles: evidenceHandles ?? [],
            ReasonCodes: reasonCodes ?? [WorkerReasonCode.NeedsSpecification],
            UnsupportedClaimFlags: unsupportedClaimFlags ?? [],
            ProhibitedActionAttempts: prohibitedActionAttempts ?? [],
            ResidueState: residueDisposition,
            DisclosureClass: disclosureClass ?? handoffPacket.ReturnVisibilityClass,
            ExecutionClaimed: executionClaimed,
            MutationClaimed: mutationClaimed,
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 12, 0, 5, TimeSpan.Zero),
            BridgeReview: bridgeReview ?? handoffPacket.BridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling ?? handoffPacket.RuntimeUseCeiling,
            JurisdictionEnvelope: jurisdictionEnvelope);
    }

    private static SliJurisdictionEnvelopeReceipt CreateActualizedEnvelope()
    {
        return SliJurisdictionContracts.ProjectFirstBootEnvelope(
            new FirstBootGovernanceLayerReceipt(
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
                RoleBoundEces: [],
                ReasonCode: "first-boot-governance-layer-role-bound-ece-ready"));
    }

    private static SliJurisdictionEnvelopeReceipt CreateIndustrializedEnvelope()
    {
        return SliJurisdictionContracts.ProjectProtectedIngressEnvelope(
            new FirstBootGovernanceLayerReceipt(
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
                RoleBoundEces: [],
                ReasonCode: "first-boot-governance-layer-role-bound-ece-ready"),
            SliBridgeContracts.CreateReview(
                bridgeStage: "worker-return-validator-test",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: "agenticore-return://candidate/test",
                outcomeKind: SliBridgeOutcomeKind.Ok,
                thresholdClass: SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band",
                operatorFormation: CreateOperatorFormationReceipt()),
            SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling(),
            CreateOperatorFormationReceipt());
    }

    private static SliJurisdictionEnvelopeReceipt CreateCivicEnvelope()
    {
        return SliJurisdictionContracts.ProjectCivicEnvelope(
            CreateIndustrializedEnvelope(),
            new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: new DateTimeOffset(2026, 3, 19, 10, 0, 0, TimeSpan.Zero)));
    }
}
