using System.Text.Json.Nodes;
using Oan.Common;
using Oan.Cradle;

namespace Oan.Runtime.IntegrationTests;

public sealed class HopngArtifactServiceIntegrationTests
{
    [Fact]
    public async Task UnavailableService_EmitsExplicitUnavailableReceipt()
    {
        var service = new UnavailableHopngArtifactService();
        var request = CreateEmissionRequest(
            includeInnerWeatherReceipts: true,
            includeWeatherDisclosureReceipts: true);

        var receipt = await service.EmitAsync(request);

        Assert.Equal(GovernedHopngArtifactOutcome.Unavailable, receipt.Outcome);
        Assert.Equal(request.LoopKey, receipt.LoopKey);
        Assert.Equal(request.Profile, receipt.Profile);
        Assert.Equal("hopng-bridge-unavailable", receipt.FailureCode);
        Assert.Null(receipt.ManifestPath);
        Assert.Null(receipt.ProjectionPath);
        Assert.Contains("community-weather:unstable", receipt.ValidationSummary);
        Assert.Contains("steward-attention:recommended", receipt.ProfileSummary);
        Assert.Contains("care-routing:checkinneeded", receipt.ValidationSummary);
        Assert.Contains("disclosure-scope:steward", receipt.ValidationSummary);
        Assert.Contains("evidence-sufficiency:sufficient", receipt.ValidationSummary);
        Assert.Contains("withheld:guardedevidence", receipt.ValidationSummary);
    }

    [Fact]
    public void EvidenceReferences_IncludeWitnessAndCompassHandlesFromSnapshot()
    {
        var request = CreateEmissionRequest(
            includeTargetWitnessReceipts: true,
            includeCompassObservationReceipts: true,
            includeCompassDriftReceipts: true,
            includeInnerWeatherReceipts: true,
            includeWeatherDisclosureReceipts: true);

        var refs = GovernedHopngEvidenceReferences.Build(request, request.Snapshot);

        Assert.Contains(refs, reference => reference.PointerUri == "target-witness://admission-accepted/aaaaaaaaaaaaaaaa");
        Assert.Contains(refs, reference => reference.PointerUri == "target-lineage://bbbbbbbbbbbbbbbb");
        Assert.Contains(refs, reference => reference.PointerUri == "target-trace://cccccccccccccccc");
        Assert.Contains(refs, reference => reference.PointerUri == "target-residue://dddddddddddddddd");
        Assert.Contains(refs, reference => reference.PointerUri == "compass-witness://ffffffffffffffff");
        Assert.Contains(refs, reference => reference.PointerUri == "compass-drift://9999999999999999");
        Assert.Contains(refs, reference => reference.PointerUri == "inner-weather://1212121212121212");
        Assert.Contains(refs, reference => reference.PointerUri == "weather-disclosure://5656565656565656");
    }

#if LOCAL_HDT_BRIDGE
    [Fact]
    public async Task HdtAndFallback_KeepDisclosureParityForCommunityWeather()
    {
        var request = CreateEmissionRequest(
            includeInnerWeatherReceipts: true,
            includeWeatherDisclosureReceipts: true);
        var outputRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-hopng-disclosure-parity");
        var localService = new LocalHdtHopngArtifactService(outputRoot);
        var fallbackService = new UnavailableHopngArtifactService();

        var localReceipt = await localService.EmitAsync(request);
        var fallbackReceipt = await fallbackService.EmitAsync(request);

        Assert.Equal(GovernedHopngArtifactOutcome.Created, localReceipt.Outcome);
        Assert.Equal(GovernedHopngArtifactOutcome.Unavailable, fallbackReceipt.Outcome);

        var communityWeatherPath = Path.Combine(
            Path.GetDirectoryName(localReceipt.ManifestPath!)!,
            "governing-traffic-evidence.community-weather.json");
        Assert.True(File.Exists(communityWeatherPath));

        var communityWeatherNode = JsonNode.Parse(File.ReadAllText(communityWeatherPath));
        Assert.Equal("unstable", communityWeatherNode?["community_safe_weather"]?["status"]?.GetValue<string>());
        Assert.Equal("recommended", communityWeatherNode?["community_safe_weather"]?["steward_attention"]?.GetValue<string>());
        Assert.Equal("checkinneeded", communityWeatherNode?["community_safe_weather"]?["routing_state"]?.GetValue<string>());
        Assert.Equal("steward", communityWeatherNode?["community_safe_weather"]?["disclosure_scope"]?.GetValue<string>());
        Assert.Equal("sufficient", communityWeatherNode?["community_safe_weather"]?["evidence_sufficiency"]?.GetValue<string>());
        Assert.Equal("guardedevidence", communityWeatherNode?["community_safe_weather"]?["withheld_markers"]?[0]?.GetValue<string>());

        Assert.Contains("community-weather:unstable", fallbackReceipt.ValidationSummary);
        Assert.Contains("steward-attention:recommended", fallbackReceipt.ValidationSummary);
        Assert.Contains("care-routing:checkinneeded", fallbackReceipt.ValidationSummary);
        Assert.Contains("disclosure-scope:steward", fallbackReceipt.ValidationSummary);
        Assert.Contains("evidence-sufficiency:sufficient", fallbackReceipt.ValidationSummary);
        Assert.Contains("withheld:guardedevidence", fallbackReceipt.ValidationSummary);
    }
#endif

    private static GovernedHopngEmissionRequest CreateEmissionRequest(
        bool includeTargetWitnessReceipts = false,
        bool includeCompassObservationReceipts = false,
        bool includeCompassDriftReceipts = false,
        bool includeInnerWeatherReceipts = false,
        bool includeWeatherDisclosureReceipts = false)
    {
        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: "agenticore-return://candidate/test",
            originSurface: "prime",
            provenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            sourceSubsystem: "AgentiCore");
        var reviewEnvelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.StewardReturnReview,
            requestedBy: "CradleTek",
            scopeHandle: "soulframe-session://cme-runtime/test",
            protectionClass: "cryptic-review",
            witnessRequirement: "governance-witness",
            actionableContent: actionableContent,
            parentEnvelopeId: "control-envelope://seed");
        var decisionReceipt = new GovernanceDecisionReceipt(
            CandidateId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            IdempotencyKey: "loop:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:test",
            CandidateProvenance: actionableContent.ProvenanceMarker,
            Decision: GovernanceDecision.Approved,
            AdjudicatorIdentity: "Steward Agent",
            RationaleCode: "steward.approved.governed-loop",
            Timestamp: DateTime.UtcNow,
            ReengrammitizationAuthorized: true,
            PrimePublicationAuthorized: true,
            AuthorizedDerivativeLanes: GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            MutationReceipt: ControlSurfaceContractGuards.CreateMutationReceipt(
                envelopeId: reviewEnvelope.EnvelopeId,
                contentHandle: actionableContent.ContentHandle,
                targetSurface: ControlSurfaceKind.GovernanceDecision,
                outcome: ControlMutationOutcome.Authorized,
                governedBy: "Steward Agent",
                decisionCode: "steward.approved.governed-loop",
                timestampUtc: DateTimeOffset.UtcNow));
        var reviewRequest = new ReturnCandidateReviewRequest(
            CandidateId: decisionReceipt.CandidateId,
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-runtime",
            ContextId: Guid.NewGuid(),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/test",
            WorkingStateHandle: "soulframe-working://cme-runtime/test",
            ReturnCandidatePointer: actionableContent.ContentHandle,
            ProvenanceMarker: actionableContent.ProvenanceMarker,
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
            RequestEnvelope: reviewEnvelope);
        var snapshot = new GovernanceLoopStateSnapshot(
            LoopKey: decisionReceipt.IdempotencyKey,
            Stage: GovernanceLoopStage.LoopCompleted,
            DecisionReceipt: decisionReceipt,
            ReviewRequest: reviewRequest,
            ReengrammitizationReceipt: null,
            PublishedLanes: GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            FirstRouteCompleted: true,
            FirstRouteDisposition: CmeCollapseDisposition.RouteToCMoS,
            LatestCollapseQualification: new CmeCollapseQualificationView(
                Destination: "cMoS",
                ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                ClassificationConfidence: 0.92,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                ReviewState: CmeCollapseReviewState.None,
                SourceSubsystem: "AgentiCore"),
            ReengrammitizationCompleted: true,
            IsTerminal: true,
            FailureCode: null,
            FailureStage: null,
            JournalIntegrityErrorCount: 0,
            HopngArtifacts: [],
            TargetWitnessReceipts: includeTargetWitnessReceipts ? [CreateTargetWitnessReceipt()] : [],
            CompassObservationReceipts: includeCompassObservationReceipts ? [CreateCompassObservationReceipt()] : [],
            CompassDriftReceipts: includeCompassDriftReceipts ? [CreateCompassDriftReceipt()] : [],
            InnerWeatherReceipts: includeInnerWeatherReceipts ? [CreateInnerWeatherReceipt()] : [],
            CommunityWeatherPacket: includeInnerWeatherReceipts || includeWeatherDisclosureReceipts
                ? new CommunityWeatherPacket(
                    Status: CommunityWeatherStatus.Unstable,
                    StewardAttention: CommunityStewardAttentionState.Recommended,
                    AnchorState: CompassDriftState.Weakened,
                    VisibilityClass: CompassVisibilityClass.CommunityLegible,
                    TimestampUtc: DateTimeOffset.UtcNow)
                : null,
            WeatherDisclosureReceipts: includeWeatherDisclosureReceipts ? [CreateWeatherDisclosureReceipt()] : []);

        return new GovernedHopngEmissionRequest(
            LoopKey: snapshot.LoopKey,
            CandidateId: decisionReceipt.CandidateId,
            CandidateProvenance: decisionReceipt.CandidateProvenance,
            Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
            Stage: snapshot.Stage,
            RequestedBy: "CradleTek",
            DecisionReceipt: decisionReceipt,
            Snapshot: snapshot,
            JournalEntries: [],
            CollapseRoutingDecision: new CmeCollapseRoutingDecision(
                Disposition: CmeCollapseDisposition.RouteToCMoS,
                ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                ReviewState: CmeCollapseReviewState.None,
                ReasonCode: decisionReceipt.RationaleCode,
                IssuedBy: decisionReceipt.AdjudicatorIdentity,
                IssuedAt: decisionReceipt.Timestamp,
                TargetClass: "cMoS",
                ClassificationConfidence: 0.92,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"));
    }

    private static GovernedTargetWitnessReceipt CreateTargetWitnessReceipt()
    {
        return new GovernedTargetWitnessReceipt(
            WitnessHandle: "target-witness://admission-accepted/aaaaaaaaaaaaaaaa",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            Kind: GovernedTargetWitnessKind.AdmissionAccepted,
            Accepted: true,
            WitnessedBy: "CradleTek",
            LaneId: "higher-order-locality",
            RuntimeId: "gc-locality-runtime",
            ProfileId: "gc-locality-profile",
            BudgetClass: "target-bounded-lane",
            CommitAuthorityClass: "refusal-only",
            Objective: "identity-continuity",
            ProgramId: "program-001",
            AdmissionHandle: "target-admission://eeeeeeeeeeeeeeee",
            LineageHandle: "target-lineage://bbbbbbbbbbbbbbbb",
            TraceHandle: "target-trace://cccccccccccccccc",
            ResidueHandle: "target-residue://dddddddddddddddd",
            Reasons: [],
            ReasonFamilies: [],
            BudgetUsage: new GovernedTargetExecutionBudgetUsage(
                InstructionCount: 3,
                SymbolicDepth: 3,
                ProjectedTraceEntryCount: 5,
                ProjectedResidueCount: 1,
                WitnessOperationCount: 0,
                TransportOperationCount: 0),
            EmittedTraceCount: 5,
            EmittedResidueCount: 1,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedCompassObservationReceipt CreateCompassObservationReceipt()
    {
        return new GovernedCompassObservationReceipt(
            WitnessHandle: "compass-witness://ffffffffffffffff",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            ObservationHandle: "compass-observation://aaaaaaaaaaaaaaaa",
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            AnchorState: CompassAnchorState.Held,
            Provenance: CompassObservationProvenance.Braided,
            WitnessedBy: "CradleTek",
            WorkingStateHandle: "soulframe-working://cme-runtime/test",
            CSelfGelHandle: "soulframe-cselfgel://cme-runtime/test",
            SelfGelHandle: "soulframe-selfgel://cme-runtime/test",
            ValidationReferenceHandle: "soulframe-selfgel://cme-runtime/test",
            Objective: "maintain bounded locality continuity",
            AdvisoryAccepted: true,
            AdvisoryDecision: "classify-ok",
            AdvisoryTrace: "classify-response-ready",
            AdvisoryConfidence: 0.71,
            AdvisorySuggestedActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            AdvisorySuggestedCompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            AdvisorySuggestedAnchorState: CompassAnchorState.Held,
            AdvisorySuggestedSelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            AdvisoryDisposition: CompassSeedAdvisoryDisposition.Accepted,
            AdvisoryDispositionReason: "host-accepted",
            AdvisoryJustification: "bounded-locality continuity remains dominant",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedCompassDriftReceipt CreateCompassDriftReceipt()
    {
        return new GovernedCompassDriftReceipt(
            DriftHandle: "compass-drift://9999999999999999",
            LoopKey: "loop:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-runtime",
            DriftState: CompassDriftState.Weakened,
            BaselineActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            BaselineCompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            LatestActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            ObservationCount: 3,
            WindowSize: 3,
            AdvisoryDivergenceCount: 2,
            CompetingMigrationCount: 0,
            WitnessedBy: "CradleTek",
            ObservationHandles:
            [
                "compass-observation://aaaaaaaaaaaaaaaa",
                "compass-observation://bbbbbbbbbbbbbbbb",
                "compass-observation://cccccccccccccccc"
            ],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedInnerWeatherReceipt CreateInnerWeatherReceipt()
    {
        return new GovernedInnerWeatherReceipt(
            InnerWeatherHandle: "inner-weather://1212121212121212",
            LoopKey: "loop:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-runtime",
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            DriftState: CompassDriftState.Weakened,
            WindowIntegrityState: WindowIntegrityState.Intact,
            ObservationCount: 3,
            WindowSize: 3,
            ResidueState: AttentionResidueState.Persistent,
            ResidueVisibilityClass: CompassVisibilityClass.OperatorGuarded,
            ResidueContributors:
            [
                AttentionResidueContributor.AdvisoryDivergence,
                AttentionResidueContributor.DriftInstability
            ],
            ShellCompetitionState: ShellCompetitionState.Absent,
            ShellCompetitionVisibilityClass: CompassVisibilityClass.CommunityLegible,
            HotCoolContactState: HotCoolContactState.InContact,
            HotCoolContactVisibilityClass: CompassVisibilityClass.CommunityLegible,
            StewardAttentionCauses:
            [
                StewardAttentionCause.DriftWeakening,
                StewardAttentionCause.ResiduePersistence
            ],
            WitnessedBy: "CradleTek",
            DriftHandle: "compass-drift://9999999999999999",
            ObservationHandles:
            [
                "compass-observation://aaaaaaaaaaaaaaaa",
                "compass-observation://bbbbbbbbbbbbbbbb",
                "compass-observation://cccccccccccccccc"
            ],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedWeatherDisclosureReceipt CreateWeatherDisclosureReceipt()
    {
        return new GovernedWeatherDisclosureReceipt(
            DisclosureHandle: "weather-disclosure://5656565656565656",
            LoopKey: "loop:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-runtime",
            RoutingState: StewardCareRoutingState.CheckInNeeded,
            CadenceState: CheckInCadenceState.Current,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sufficient,
            WindowIntegrityState: WindowIntegrityState.Intact,
            DisclosureScope: WeatherDisclosureScope.Steward,
            CommunityWeatherPacket: new CommunityWeatherPacket(
                Status: CommunityWeatherStatus.Unstable,
                StewardAttention: CommunityStewardAttentionState.Recommended,
                AnchorState: CompassDriftState.Weakened,
                VisibilityClass: CompassVisibilityClass.CommunityLegible,
                TimestampUtc: DateTimeOffset.UtcNow),
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
            InnerWeatherHandle: "inner-weather://1212121212121212",
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
