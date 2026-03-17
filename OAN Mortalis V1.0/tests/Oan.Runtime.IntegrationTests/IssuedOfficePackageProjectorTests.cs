using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class IssuedOfficePackageProjectorTests
{
    [Fact]
    public void ProjectForLoop_StewardOfficeSpecificView_ProjectsPackageAndReceipt()
    {
        var loopKey = "loop:office-issuance:test";
        var cmeId = "cme-office-issuance";
        var disclosureHandle = "weather-disclosure://aaaaaaaaaaaaaaaa";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(loopKey, cmeId, disclosureHandle, InternalGoverningCmeOffice.Steward));

        var projection = IssuedOfficePackageProjector.ProjectForLoop(loopKey, batch);

        Assert.True(projection.HasValue);
        var (package, receipt) = projection!.Value;
        Assert.Equal(InternalGoverningCmeOffice.Steward, package.OfficeKind);
        Assert.Equal("governed-zed-base", package.ChassisClass);
        Assert.Equal("active-governance-loop", package.TargetRuntimeSurface);
        Assert.Equal("host_truth_runtime", package.IssuerSurface);
        Assert.Equal("routed_operational_vigilance", package.AuthorityScope);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, package.AllowedActionCeiling);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, package.DisclosureCeiling);
        Assert.Equal(MaturityPosture.DoctrineBacked, package.MaturityPosture);
        Assert.Equal("office-return-summary-v1", package.RequiredReturnPacket);
        Assert.Contains("task-routing", package.ToolAllowlist);
        Assert.Contains("mission-local", package.MountedMemoryLanes);
        Assert.Contains("cryptic-sealed", package.ForbiddenMemoryLanes);
        Assert.Equal("CradleTek", package.AuthorizingOperatorOrKernel);
        Assert.StartsWith("office-package://", package.PackageId, StringComparison.Ordinal);
        Assert.StartsWith("issuance-lineage://", package.IssuanceLineageId, StringComparison.Ordinal);
        Assert.StartsWith("office-instance://", package.OfficeInstanceId, StringComparison.Ordinal);

        Assert.Equal(ConstructClass.IssuedOffice, receipt.ConstructClass);
        Assert.Equal(package.PackageId, receipt.PackageId);
        Assert.Equal(package.IssuanceLineageId, receipt.IssuanceLineageId);
        Assert.Equal(package.OfficeInstanceId, receipt.OfficeInstanceId);
        Assert.Equal(InternalGoverningCmeOffice.Steward, receipt.Office);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, receipt.AllowedActionCeiling);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, receipt.DisclosureCeiling);
        Assert.Equal(MaturityPosture.DoctrineBacked, receipt.MaturityPosture);
        Assert.Equal(disclosureHandle, receipt.WeatherDisclosureHandle);
        Assert.StartsWith("office-issuance://", receipt.IssuanceHandle, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(InternalGoverningCmeOffice.Father)]
    [InlineData(InternalGoverningCmeOffice.Mother)]
    public void ProjectForLoop_FatherOrMotherAuthorityReceipt_ProjectsNoIssuance(InternalGoverningCmeOffice office)
    {
        var loopKey = "loop:office-issuance:no-issue";
        var cmeId = "cme-office-issuance";
        var disclosureHandle = "weather-disclosure://bbbbbbbbbbbbbbbb";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                office,
                viewEligibility: OfficeViewEligibility.GuardedView,
                rationaleCode: OfficeAuthorityRationaleCode.GuardedViewForOffice,
                actionEligibility: OfficeActionEligibility.AcknowledgeAllowed));

        var projection = IssuedOfficePackageProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Fact]
    public void ProjectForLoop_WithheldStewardAuthority_ProjectsNoIssuance()
    {
        var loopKey = "loop:office-issuance:withheld";
        var cmeId = "cme-office-issuance";
        var disclosureHandle = "weather-disclosure://cccccccccccccccc";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(loopKey, cmeId, disclosureHandle),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                InternalGoverningCmeOffice.Steward,
                viewEligibility: OfficeViewEligibility.Withheld,
                rationaleCode: OfficeAuthorityRationaleCode.WithheldForAuthorityRequirement,
                actionEligibility: OfficeActionEligibility.ViewOnly));

        var projection = IssuedOfficePackageProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Theory]
    [InlineData(EvidenceSufficiencyState.Sparse, WindowIntegrityState.Sparse)]
    [InlineData(EvidenceSufficiencyState.BrokenWindow, WindowIntegrityState.JournalGap)]
    [InlineData(EvidenceSufficiencyState.ContinuityAmbiguous, WindowIntegrityState.GovernanceReset)]
    public void ProjectForLoop_SparseOrBrokenEvidence_ProjectsNoIssuance(
        EvidenceSufficiencyState evidenceSufficiencyState,
        WindowIntegrityState windowIntegrityState)
    {
        var loopKey = "loop:office-issuance:insufficient";
        var cmeId = "cme-office-issuance";
        var disclosureHandle = "weather-disclosure://dddddddddddddddd";
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
                InternalGoverningCmeOffice.Steward,
                evidenceSufficiencyState: evidenceSufficiencyState,
                rationaleCode: OfficeAuthorityRationaleCode.WithheldForInsufficientEvidence,
                viewEligibility: OfficeViewEligibility.CommunityReduced,
                actionEligibility: OfficeActionEligibility.AcknowledgeAllowed));

        var projection = IssuedOfficePackageProjector.ProjectForLoop(loopKey, batch);

        Assert.False(projection.HasValue);
    }

    [Fact]
    public void ProjectForLoop_UsesCommunityDisclosureCeilingWhenStewardScopeIsCommunity()
    {
        var loopKey = "loop:office-issuance:community";
        var cmeId = "cme-office-issuance";
        var disclosureHandle = "weather-disclosure://eeeeeeeeeeeeeeee";
        var batch = CreateBatch(
            loopKey,
            cmeId,
            CreateDisclosureReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                disclosureScope: WeatherDisclosureScope.Community),
            CreateAuthorityReceipt(
                loopKey,
                cmeId,
                disclosureHandle,
                InternalGoverningCmeOffice.Steward,
                disclosureScope: WeatherDisclosureScope.Community));

        var projection = IssuedOfficePackageProjector.ProjectForLoop(loopKey, batch);

        Assert.True(projection.HasValue);
        Assert.Equal(CompassVisibilityClass.CommunityLegible, projection!.Value.Package.DisclosureCeiling);
    }

    private static GovernanceJournalReplayBatch CreateBatch(
        string loopKey,
        string cmeId,
        GovernedWeatherDisclosureReceipt disclosureReceipt,
        GovernedOfficeAuthorityReceipt authorityReceipt)
    {
        var timestamp = disclosureReceipt.TimestampUtc.UtcDateTime.AddSeconds(-5);
        var reviewRequest = CreateReviewRequest(cmeId);
        var reviewEntry = new GovernanceJournalEntry(
            LoopKey: loopKey,
            Kind: GovernanceJournalEntryKind.Annotation,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            Timestamp: timestamp,
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

        return new GovernanceJournalReplayBatch([reviewEntry, disclosureEntry, authorityEntry], []);
    }

    private static ReturnCandidateReviewRequest CreateReviewRequest(string cmeId)
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
            CandidateId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            IdentityId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            SoulFrameId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CMEId: cmeId,
            ContextId: Guid.Parse("44444444-4444-4444-4444-444444444444"),
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
            RequestEnvelope: envelope);
    }

    private static GovernedWeatherDisclosureReceipt CreateDisclosureReceipt(
        string loopKey,
        string cmeId,
        string disclosureHandle,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient,
        WindowIntegrityState windowIntegrityState = WindowIntegrityState.Intact,
        WeatherDisclosureScope disclosureScope = WeatherDisclosureScope.Steward)
    {
        var timestamp = new DateTimeOffset(2026, 3, 17, 10, 0, 0, TimeSpan.Zero);
        return new GovernedWeatherDisclosureReceipt(
            DisclosureHandle: disclosureHandle,
            LoopKey: loopKey,
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: cmeId,
            RoutingState: StewardCareRoutingState.CheckInNeeded,
            CadenceState: CheckInCadenceState.Current,
            EvidenceSufficiencyState: evidenceSufficiencyState,
            WindowIntegrityState: windowIntegrityState,
            DisclosureScope: disclosureScope,
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
            WithheldMarkers: disclosureScope == WeatherDisclosureScope.Community
                ? []
                : [WeatherWithheldMarker.GuardedEvidence],
            RationaleCode: disclosureScope == WeatherDisclosureScope.Community
                ? WeatherDisclosureRationaleCode.CommunityWhitelisted
                : WeatherDisclosureRationaleCode.GuardedReduction,
            WitnessedBy: "CradleTek",
            InnerWeatherHandle: "inner-weather://1111111111111111",
            TimestampUtc: timestamp);
    }

    private static GovernedOfficeAuthorityReceipt CreateAuthorityReceipt(
        string loopKey,
        string cmeId,
        string disclosureHandle,
        InternalGoverningCmeOffice office,
        OfficeViewEligibility viewEligibility = OfficeViewEligibility.OfficeSpecificView,
        OfficeAuthorityRationaleCode rationaleCode = OfficeAuthorityRationaleCode.OfficeSpecificStewardView,
        OfficeActionEligibility actionEligibility = OfficeActionEligibility.CheckInAllowed,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient,
        WeatherDisclosureScope disclosureScope = WeatherDisclosureScope.Steward)
    {
        var timestamp = new DateTimeOffset(2026, 3, 17, 10, 0, 5, TimeSpan.Zero);
        return new GovernedOfficeAuthorityReceipt(
            AuthorityHandle: $"office-authority://{office.ToString().ToLowerInvariant()}-aaaaaaaaaaaaaaaa",
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
            DisclosureScope: disclosureScope,
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
}
