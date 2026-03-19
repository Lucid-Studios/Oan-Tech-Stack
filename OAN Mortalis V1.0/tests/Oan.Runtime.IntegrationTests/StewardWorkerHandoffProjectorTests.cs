using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class StewardWorkerHandoffProjectorTests
{
    [Fact]
    public void ProjectForLoop_StewardIssuanceWithSufficientEvidence_ProjectsRequestOnlyHandoffPacketAndReceipt()
    {
        var loopKey = "loop:worker-handoff:test";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://aaaaaaaaaaaaaaaa";
        var authorityHandle = "office-authority://bbbbbbbbbbbbbbbb";
        var issuanceHandle = "office-issuance://cccccccccccccccc";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(loopKey, cmeId, disclosureHandle, authorityHandle),
            CreateIssuanceReceipt(loopKey, cmeId, disclosureHandle, authorityHandle, issuanceHandle));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.True(projection.HasValue);
        var (packet, receipt) = projection!.Value;
        Assert.Equal(InternalGoverningCmeOffice.Steward, packet.RequestingOffice);
        Assert.Equal(GovernedWorkerSpecies.RepoBugStewardWorker, packet.WorkerSpecies);
        Assert.Equal(WorkerInstanceMode.RequestOnly, packet.WorkerInstanceMode);
        Assert.Equal("repo-bug-triage", packet.TaskKind);
        Assert.Equal("worker-return-summary-v1", packet.RequiredOutputKind);
        Assert.Equal("worker-return-packet-v1", packet.ReturnPacketSchema);
        Assert.Equal("steward-governance-loop", packet.ReturnDestination);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, packet.DisclosureClass);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, packet.ReturnVisibilityClass);
        Assert.Equal(WorkerResidueDisposition.NeedsClassification, packet.ResidueDisposition);
        Assert.Equal(EvidenceSufficiencyState.Sufficient, packet.EvidenceSufficiencyState);
        Assert.Equal(MaturityPosture.DoctrineBacked, packet.MaturityPosture);
        Assert.Contains(WorkerReasonCode.UnknownNotFailure, packet.AllowedReasonCodes);
        Assert.Contains("repo-read-only", packet.ToolAllowlist);
        Assert.Contains("undeclared-tool-call", packet.ProhibitedActions);
        Assert.Contains("cryptic-sealed", packet.ForbiddenMemoryLanes);
        Assert.StartsWith("worker-handoff-packet://", packet.HandoffPacketId, StringComparison.Ordinal);
        Assert.NotNull(packet.BridgeReview);
        Assert.Equal(SliBridgeOutcomeKind.Ok, packet.BridgeReview!.OutcomeKind);
        Assert.Equal("agenticore-return://candidate/cme-worker-handoff/1", packet.BridgeReview.BridgeWitnessHandle);
        Assert.NotNull(packet.RuntimeUseCeiling);
        Assert.True(packet.RuntimeUseCeiling!.CandidateOnly);

        Assert.Equal(ConstructClass.BoundedWorker, receipt.ConstructClass);
        Assert.Equal(packet.HandoffPacketId, receipt.HandoffPacketId);
        Assert.Equal(issuanceHandle, receipt.OfficeIssuanceHandle);
        Assert.Equal(authorityHandle, receipt.OfficeAuthorityHandle);
        Assert.Equal(disclosureHandle, receipt.WeatherDisclosureHandle);
        Assert.Equal(WorkerInstanceMode.RequestOnly, receipt.WorkerInstanceMode);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, receipt.ActionCeiling);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, receipt.DisclosureClass);
        Assert.Equal(MaturityPosture.DoctrineBacked, receipt.MaturityPosture);
        Assert.StartsWith("worker-handoff://", receipt.HandoffHandle, StringComparison.Ordinal);
        Assert.Equal(packet.BridgeReview, receipt.BridgeReview);
        Assert.Equal(packet.RuntimeUseCeiling, receipt.RuntimeUseCeiling);
    }

    [Theory]
    [InlineData(InternalGoverningCmeOffice.Father)]
    [InlineData(InternalGoverningCmeOffice.Mother)]
    public void ProjectForLoop_FatherOrMotherIssuance_ProjectsNoHandoff(InternalGoverningCmeOffice office)
    {
        var loopKey = "loop:worker-handoff:no-office";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://dddddddddddddddd";
        var authorityHandle = $"office-authority://{office.ToString().ToLowerInvariant()}-eeeeeeeeeeeeeeee";
        var issuanceHandle = $"office-issuance://{office.ToString().ToLowerInvariant()}-ffffffffffffffff";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                authorityHandle,
                office: office,
                viewEligibility: OfficeViewEligibility.Withheld,
                rationaleCode: OfficeAuthorityRationaleCode.WithheldForOfficeNotAttached,
                actionEligibility: OfficeActionEligibility.ViewOnly),
            CreateIssuanceReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                authorityHandle,
                issuanceHandle,
                office: office,
                allowedActionCeiling: OfficeActionEligibility.ViewOnly));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Theory]
    [InlineData(EvidenceSufficiencyState.Sparse, WindowIntegrityState.Sparse)]
    [InlineData(EvidenceSufficiencyState.BrokenWindow, WindowIntegrityState.JournalGap)]
    [InlineData(EvidenceSufficiencyState.ContinuityAmbiguous, WindowIntegrityState.GovernanceReset)]
    public void ProjectForLoop_SparseOrBrokenEvidence_ProjectsNoHandoff(
        EvidenceSufficiencyState evidenceSufficiencyState,
        WindowIntegrityState windowIntegrityState)
    {
        var loopKey = "loop:worker-handoff:insufficient";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://1111111111111111";
        var authorityHandle = "office-authority://2222222222222222";
        var issuanceHandle = "office-issuance://3333333333333333";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                evidenceSufficiencyState,
                windowIntegrityState),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                authorityHandle,
                evidenceSufficiencyState: evidenceSufficiencyState),
            CreateIssuanceReceipt(loopKey, cmeId, disclosureHandle, authorityHandle, issuanceHandle));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Theory]
    [InlineData(OfficeActionEligibility.ViewOnly)]
    [InlineData(OfficeActionEligibility.AcknowledgeAllowed)]
    public void ProjectForLoop_ActionBelowCheckInAllowed_ProjectsNoHandoff(
        OfficeActionEligibility actionEligibility)
    {
        var loopKey = "loop:worker-handoff:action-floor";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://4444444444444444";
        var authorityHandle = "office-authority://5555555555555555";
        var issuanceHandle = "office-issuance://6666666666666666";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                authorityHandle,
                actionEligibility: actionEligibility),
            CreateIssuanceReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                authorityHandle,
                issuanceHandle,
                allowedActionCeiling: actionEligibility));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Fact]
    public void ProjectForLoop_BlockingPreBondSafeguard_ProjectsNoHandoff()
    {
        var loopKey = "loop:worker-handoff:prebond-safeguard";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://7777777777777777";
        var authorityHandle = "office-authority://8888888888888888";
        var issuanceHandle = "office-issuance://9999999999999999";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(loopKey, cmeId, disclosureHandle, authorityHandle),
            CreateIssuanceReceipt(loopKey, cmeId, disclosureHandle, authorityHandle, issuanceHandle),
            CreateReviewRequest(
                cmeId,
                SliBridgeContracts.CreatePreBondProtectiveReview(
                    bridgeStage: "steward-worker-handoff-test",
                    sourceTheater: "prime",
                    targetTheater: "prime",
                    bridgeWitnessHandle: $"agenticore-return://candidate/{cmeId}/1",
                    outcomeKind: SliBridgeOutcomeKind.NeedsSpec,
                    thresholdClass: SliBridgeThresholdClass.ThresholdBreach,
                    reasonCode: "sli-prebond-continuity-instability",
                    safeguardClass: SliPreBondSafeguardClass.ContinuityInstability,
                    disposition: SliPreBondSafeguardDisposition.Hold)));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Fact]
    public void ProjectForLoop_OperatorFormationBridgeReview_AddsFormationHandleToSourceHandles()
    {
        var loopKey = "loop:worker-handoff:operator-formation";
        var cmeId = "cme-worker-handoff";
        var disclosureHandle = "weather-disclosure://operatorformation";
        var authorityHandle = "office-authority://operatorformation";
        var issuanceHandle = "office-issuance://operatorformation";
        var bridgeReview = SliBridgeContracts.CreateReview(
            bridgeStage: "steward-worker-handoff-test",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: $"agenticore-return://candidate/{cmeId}/1",
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-within-band",
            operatorFormation: CreateOperatorFormationReceipt());
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(loopKey, cmeId, disclosureHandle, authorityHandle),
            CreateIssuanceReceipt(loopKey, cmeId, disclosureHandle, authorityHandle, issuanceHandle),
            CreateReviewRequest(cmeId, bridgeReview));

        var projection = StewardWorkerHandoffProjector.ProjectForLoop(loopKey, batch);

        Assert.True(projection.HasValue);
        var (packet, receipt) = projection!.Value;
        Assert.NotNull(packet.BridgeReview!.OperatorFormation);
        Assert.Contains(packet.BridgeReview.OperatorFormation!.FormationHandle, packet.SourceHandles);
        Assert.Equal(packet.BridgeReview.OperatorFormation, receipt.BridgeReview!.OperatorFormation);
    }

    private static GovernanceJournalReplayBatch CreateBatch(
        string loopKey,
        string cmeId,
        GovernedWeatherDisclosureReceipt disclosureReceipt,
        GovernedOfficeAuthorityReceipt authorityReceipt,
        GovernedOfficeIssuanceReceipt issuanceReceipt,
        ReturnCandidateReviewRequest? explicitReviewRequest = null)
    {
        var reviewRequest = explicitReviewRequest ?? CreateReviewRequest(cmeId);
        var reviewEntry = new GovernanceJournalEntry(
            LoopKey: loopKey,
            Kind: GovernanceJournalEntryKind.Annotation,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            Timestamp: disclosureReceipt.TimestampUtc.UtcDateTime.AddSeconds(-5),
            DecisionReceipt: null,
            DeferredReview: null,
            ActReceipt: null,
            ReviewRequest: reviewRequest,
            Annotation: null);
        var disclosureEntry = new GovernanceJournalEntry(
            LoopKey: loopKey,
            Kind: GovernanceJournalEntryKind.WeatherDisclosure,
            Stage: disclosureReceipt.Stage,
            Timestamp: disclosureReceipt.TimestampUtc.UtcDateTime,
            DecisionReceipt: null,
            DeferredReview: null,
            ActReceipt: null,
            ReviewRequest: null,
            Annotation: null,
            HopngArtifactReceipt: null,
            TargetWitnessReceipt: null,
            CompassObservationReceipt: null,
            CompassDriftReceipt: null,
            InnerWeatherReceipt: null,
            WeatherDisclosureReceipt: disclosureReceipt);
        var authorityEntry = new GovernanceJournalEntry(
            LoopKey: loopKey,
            Kind: GovernanceJournalEntryKind.OfficeAuthority,
            Stage: authorityReceipt.Stage,
            Timestamp: authorityReceipt.TimestampUtc.UtcDateTime,
            DecisionReceipt: null,
            DeferredReview: null,
            ActReceipt: null,
            ReviewRequest: null,
            Annotation: null,
            HopngArtifactReceipt: null,
            TargetWitnessReceipt: null,
            CompassObservationReceipt: null,
            CompassDriftReceipt: null,
            InnerWeatherReceipt: null,
            WeatherDisclosureReceipt: null,
            OfficeAuthorityReceipt: authorityReceipt);
        var issuanceEntry = new GovernanceJournalEntry(
            LoopKey: loopKey,
            Kind: GovernanceJournalEntryKind.OfficeIssuance,
            Stage: issuanceReceipt.Stage,
            Timestamp: issuanceReceipt.TimestampUtc.UtcDateTime,
            DecisionReceipt: null,
            DeferredReview: null,
            ActReceipt: null,
            ReviewRequest: null,
            Annotation: null,
            HopngArtifactReceipt: null,
            TargetWitnessReceipt: null,
            CompassObservationReceipt: null,
            CompassDriftReceipt: null,
            InnerWeatherReceipt: null,
            WeatherDisclosureReceipt: null,
            OfficeAuthorityReceipt: null,
            OfficeIssuanceReceipt: issuanceReceipt);

        return new GovernanceJournalReplayBatch([reviewEntry, disclosureEntry, authorityEntry, issuanceEntry], []);
    }

    private static ReturnCandidateReviewRequest CreateReviewRequest(
        string cmeId,
        SliBridgeReviewReceipt? explicitBridgeReview = null)
    {
        var sessionHandle = $"soulframe-session://{cmeId}/1";
        var returnPointer = $"agenticore-return://candidate/{cmeId}/1";
        var provenance = $"membrane-derived:cme:{cmeId}|policy:agenticore.cognition.cycle|loop:1";
        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: returnPointer,
            originSurface: "prime",
            provenanceMarker: provenance,
            sourceSubsystem: "AgentiCore");
        var envelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.StewardReturnReview,
            requestedBy: "CradleTek",
            scopeHandle: sessionHandle,
            protectionClass: "cryptic-review",
            witnessRequirement: "governance-witness",
            actionableContent: actionableContent);

        return new ReturnCandidateReviewRequest(
            CandidateId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            IdentityId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            SoulFrameId: Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            CMEId: cmeId,
            ContextId: Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: sessionHandle,
            WorkingStateHandle: $"soulframe-working://{cmeId}/1",
            ReturnCandidatePointer: returnPointer,
            ProvenanceMarker: provenance,
            IntakeIntent: "candidate-return-evaluation",
            SubmittedBy: "CradleTek",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            RequestEnvelope: envelope,
            BridgeReview: explicitBridgeReview ?? SliBridgeContracts.CreateReview(
                bridgeStage: "steward-worker-handoff-test",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: returnPointer,
                outcomeKind: SliBridgeOutcomeKind.Ok,
                thresholdClass: SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band"),
            RuntimeUseCeiling: SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling());
    }

    private static SliOperatorFormationReceipt CreateOperatorFormationReceipt()
    {
        return SliBridgeContracts.CreatePreBondOperatorFormationReceipt(
            formationHandle: "operator-formation://prebond/aaaaaaaaaaaaaaaa",
            boundaryCrossingMode: SliOperatorFormationBoundaryCrossingMode.InterlacedBondedCrossing,
            profile: new SliOperatorFormationProfileReceipt(
                ProfileId: "gs_profile_obsidian_guarded",
                Lane: SliOperatorFormationLane.GnomeSpeakNlpSquared,
                ChapterLocalSurface: "research/publications/gnomeronacorde-v0.1/source/1_OBSIDIAN_WALL/1a_Casting_Shadow.tex",
                PairedTrainingSurface: "research/publications/gnome-speak-nlp-v1.0/source/sections/operator_role.tex",
                CrossingTaskKind: "literacy_alignment",
                HaltOwner: "bonded_training_reviewer",
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
                ProhibitedOutputs: ["unrestricted_archetype_claim"]),
            certificationPosture: new SliOperatorFormationCertificationReceipt(
                Decision: SliOperatorFormationCertificationDecision.Pending,
                CurrentAnchoredPosture: SliOperatorFormationBondStatus.TrainingOperator,
                TargetPosture: SliOperatorFormationBondStatus.PreCertifiedOperator,
                NearestAdmissibleNextPosture: SliOperatorFormationBondStatus.VerifiedCandidate,
                ReviewOwner: "certification_reviewer://first-run-lane",
                EvidenceGaps: ["trial_receipt_set"],
                ProhibitedClaims: ["bond actualized"],
                CertificationIssued: false,
                ExpandedRevealAllowed: false,
                ContinuityClaimAllowed: false),
            sigilAssets:
            [
                new SliOperatorFormationSigilAssetReceipt(
                    AssetId: "obsidian_zed",
                    AssetLabel: "OBSIDIANzed",
                    SigilClass: SliOperatorFormationSigilClass.MergedCompletionKey,
                    PhaseNumber: null,
                    VisibilityClass: "continuity_sealed",
                    BuildRenderPolicy: "staged_render_allowed",
                    ReductionPosture: "descriptive_reduction_allowed",
                    MergedFromAssets: ["obsidian_1", "obsidian_2", "obsidian_3", "obsidian_4", "obsidian_5"],
                    WitnessOfAsset: null)
            ]);
    }

    private static GovernedWeatherDisclosureReceipt CreateDisclosureReceipt(
        string loopKey,
        string cmeId,
        string disclosureHandle,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient,
        WindowIntegrityState windowIntegrityState = WindowIntegrityState.Intact)
    {
        var timestamp = new DateTimeOffset(2026, 3, 17, 11, 0, 0, TimeSpan.Zero);
        return new GovernedWeatherDisclosureReceipt(
            DisclosureHandle: disclosureHandle,
            LoopKey: loopKey,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: cmeId,
            RoutingState: StewardCareRoutingState.CheckInNeeded,
            CadenceState: CheckInCadenceState.Current,
            EvidenceSufficiencyState: evidenceSufficiencyState,
            WindowIntegrityState: windowIntegrityState,
            DisclosureScope: WeatherDisclosureScope.Steward,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: timestamp),
            AllowedCommunityFields:
            [
                CommunityWeatherField.Status,
                CommunityWeatherField.StewardAttention,
                CommunityWeatherField.AnchorState,
                CommunityWeatherField.VisibilityClass,
                CommunityWeatherField.TimestampUtc
            ],
            StewardReasonCodes:
            [
                StewardAttentionCause.DriftWeakening,
                StewardAttentionCause.ResiduePersistence
            ],
            WithheldMarkers:
            [
                WeatherWithheldMarker.GuardedEvidence
            ],
            RationaleCode: WeatherDisclosureRationaleCode.GuardedReduction,
            WitnessedBy: "CradleTek",
            InnerWeatherHandle: "inner-weather://aaaaaaaaaaaaaaaa",
            TimestampUtc: timestamp);
    }

    private static GovernedOfficeAuthorityReceipt CreateAuthorityReceipt(
        string loopKey,
        string cmeId,
        string disclosureHandle,
        string authorityHandle,
        InternalGoverningCmeOffice office = InternalGoverningCmeOffice.Steward,
        OfficeViewEligibility viewEligibility = OfficeViewEligibility.OfficeSpecificView,
        OfficeAuthorityRationaleCode rationaleCode = OfficeAuthorityRationaleCode.OfficeSpecificStewardView,
        OfficeActionEligibility actionEligibility = OfficeActionEligibility.CheckInAllowed,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient)
    {
        var timestamp = new DateTimeOffset(2026, 3, 17, 11, 0, 5, TimeSpan.Zero);
        return new GovernedOfficeAuthorityReceipt(
            AuthorityHandle: authorityHandle,
            LoopKey: loopKey,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: cmeId,
            Office: office,
            AuthoritySurface: office switch
            {
                InternalGoverningCmeOffice.Steward => OfficeAuthoritySurface.StewardSurface,
                InternalGoverningCmeOffice.Father => OfficeAuthoritySurface.CrypticBoundarySurface,
                InternalGoverningCmeOffice.Mother => OfficeAuthoritySurface.PrimeCareSurface,
                _ => OfficeAuthoritySurface.GuardedReviewSurface
            },
            ViewEligibility: viewEligibility,
            AcknowledgmentEligibility: viewEligibility == OfficeViewEligibility.Withheld
                ? OfficeAcknowledgmentEligibility.NotAllowed
                : OfficeAcknowledgmentEligibility.Allowed,
            ActionEligibility: actionEligibility,
            EvidenceSufficiencyState: evidenceSufficiencyState,
            DisclosureScope: WeatherDisclosureScope.Steward,
            CommunityWeatherPacket: viewEligibility == OfficeViewEligibility.Withheld
                ? null
                : new CommunityWeatherPacket(
                    Status: CommunityWeatherStatus.Unstable,
                    StewardAttention: CommunityStewardAttentionState.Recommended,
                    AnchorState: CompassDriftState.Weakened,
                    VisibilityClass: CompassVisibilityClass.CommunityLegible,
                    TimestampUtc: timestamp),
            AllowedReasonCodes: viewEligibility == OfficeViewEligibility.Withheld
                ? []
                :
                [
                    StewardAttentionCause.DriftWeakening,
                    StewardAttentionCause.ResiduePersistence
                ],
            WithheldMarkers: viewEligibility == OfficeViewEligibility.Withheld
                ? [OfficeAuthorityWithheldMarker.OfficeNotAttached]
                : [],
            Prohibitions:
            [
                OfficeAuthorityProhibition.MayNotOriginateTruth,
                OfficeAuthorityProhibition.MayNotWidenDisclosure
            ],
            RationaleCode: rationaleCode,
            WitnessedBy: "CradleTek",
            WeatherDisclosureHandle: disclosureHandle,
            TimestampUtc: timestamp);
    }

    private static GovernedOfficeIssuanceReceipt CreateIssuanceReceipt(
        string loopKey,
        string cmeId,
        string disclosureHandle,
        string authorityHandle,
        string issuanceHandle,
        InternalGoverningCmeOffice office = InternalGoverningCmeOffice.Steward,
        OfficeActionEligibility allowedActionCeiling = OfficeActionEligibility.CheckInAllowed)
    {
        return new GovernedOfficeIssuanceReceipt(
            IssuanceHandle: issuanceHandle,
            LoopKey: loopKey,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: cmeId,
            Office: office,
            ConstructClass: ConstructClass.IssuedOffice,
            PackageId: "office-package://9999999999999999",
            IssuanceLineageId: "issuance-lineage://aaaaaaaaaaaaaaaa",
            OfficeInstanceId: $"office-instance://{office.ToString().ToLowerInvariant()}-bbbbbbbbbbbbbbbb",
            AllowedActionCeiling: allowedActionCeiling,
            DisclosureCeiling: CompassVisibilityClass.OperatorGuarded,
            MaturityPosture: MaturityPosture.DoctrineBacked,
            OfficeAuthorityHandle: authorityHandle,
            WeatherDisclosureHandle: disclosureHandle,
            WitnessedBy: "CradleTek",
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 11, 0, 10, TimeSpan.Zero));
    }
}
