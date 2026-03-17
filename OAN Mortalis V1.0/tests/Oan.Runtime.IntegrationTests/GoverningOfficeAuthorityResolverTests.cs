using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GoverningOfficeAuthorityResolverTests
{
    [Fact]
    public void AssessForLoop_DefaultContexts_AttachesStewardFirst()
    {
        var batch = CreateBatch(CreateDisclosureReceipt(
            routingState: StewardCareRoutingState.CheckInNeeded,
            disclosureScope: WeatherDisclosureScope.Steward));

        var assessments = GoverningOfficeAuthorityResolver.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch);

        Assert.Equal(3, assessments.Count);

        var steward = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Steward));
        var father = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Father));
        var mother = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Mother));

        Assert.Equal(OfficeViewEligibility.OfficeSpecificView, steward.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, steward.ActionEligibility);
        Assert.Equal(OfficeViewEligibility.Withheld, father.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.ViewOnly, father.ActionEligibility);
        Assert.Equal(OfficeViewEligibility.Withheld, mother.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.ViewOnly, mother.ActionEligibility);
    }

    [Fact]
    public void AssessForLoop_SameState_DoesNotGrantIdenticalActionAuthority()
    {
        var batch = CreateBatch(CreateDisclosureReceipt(
            routingState: StewardCareRoutingState.EscalationEligible,
            disclosureScope: WeatherDisclosureScope.OperatorGuarded));
        var contexts = new Dictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>
        {
            [InternalGoverningCmeOffice.Steward] = new(
                InternalGoverningCmeOffice.Steward,
                OfficeAttached: true,
                BondedConfirmed: false,
                GuardedReviewConfirmed: true),
            [InternalGoverningCmeOffice.Father] = new(
                InternalGoverningCmeOffice.Father,
                OfficeAttached: true,
                BondedConfirmed: false,
                GuardedReviewConfirmed: true),
            [InternalGoverningCmeOffice.Mother] = new(
                InternalGoverningCmeOffice.Mother,
                OfficeAttached: true,
                BondedConfirmed: true,
                GuardedReviewConfirmed: false)
        };

        var assessments = GoverningOfficeAuthorityResolver.AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch, contexts);

        var steward = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Steward));
        var father = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Father));
        var mother = Assert.Single(assessments.Where(item => item.Office == InternalGoverningCmeOffice.Mother));

        Assert.Equal(OfficeActionEligibility.EscalationReviewAllowed, steward.ActionEligibility);
        Assert.Equal(OfficeActionEligibility.AcknowledgeAllowed, father.ActionEligibility);
        Assert.Equal(OfficeActionEligibility.AcknowledgeAllowed, mother.ActionEligibility);
    }

    [Fact]
    public void AssessForLoop_FatherRequiresGuardedReviewContext()
    {
        var batch = CreateBatch(CreateDisclosureReceipt(
            routingState: StewardCareRoutingState.CheckInRecommended,
            disclosureScope: WeatherDisclosureScope.OperatorGuarded));
        var withoutGuardedReview = new Dictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>
        {
            [InternalGoverningCmeOffice.Father] = new(
                InternalGoverningCmeOffice.Father,
                OfficeAttached: true,
                BondedConfirmed: false,
                GuardedReviewConfirmed: false)
        };
        var withGuardedReview = new Dictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>
        {
            [InternalGoverningCmeOffice.Father] = new(
                InternalGoverningCmeOffice.Father,
                OfficeAttached: true,
                BondedConfirmed: false,
                GuardedReviewConfirmed: true)
        };

        var withoutAssessment = GoverningOfficeAuthorityResolver
            .AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch, withoutGuardedReview)
            .Single(item => item.Office == InternalGoverningCmeOffice.Father);
        var withAssessment = GoverningOfficeAuthorityResolver
            .AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch, withGuardedReview)
            .Single(item => item.Office == InternalGoverningCmeOffice.Father);

        Assert.Equal(OfficeViewEligibility.Withheld, withoutAssessment.ViewEligibility);
        Assert.Equal(OfficeViewEligibility.GuardedView, withAssessment.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.AcknowledgeAllowed, withAssessment.ActionEligibility);
    }

    [Fact]
    public void AssessForLoop_BondedStewardEnablesHandoffEligibility()
    {
        var batch = CreateBatch(CreateDisclosureReceipt(
            routingState: StewardCareRoutingState.EscalationEligible,
            disclosureScope: WeatherDisclosureScope.OperatorGuarded));
        var contexts = new Dictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>
        {
            [InternalGoverningCmeOffice.Steward] = new(
                InternalGoverningCmeOffice.Steward,
                OfficeAttached: true,
                BondedConfirmed: true,
                GuardedReviewConfirmed: true)
        };

        var steward = GoverningOfficeAuthorityResolver
            .AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch, contexts)
            .Single(item => item.Office == InternalGoverningCmeOffice.Steward);

        Assert.Equal(OfficeActionEligibility.HandoffEligible, steward.ActionEligibility);
    }

    [Fact]
    public void AssessForLoop_SparseEvidence_DoesNotSynthesizeActionAuthority()
    {
        var batch = CreateBatch(CreateDisclosureReceipt(
            routingState: StewardCareRoutingState.CheckInNeeded,
            disclosureScope: WeatherDisclosureScope.Steward,
            evidenceSufficiencyState: EvidenceSufficiencyState.Sparse));

        var steward = GoverningOfficeAuthorityResolver
            .AssessForLoop(batch.FinalLoopKey, batch.ReplayBatch)
            .Single(item => item.Office == InternalGoverningCmeOffice.Steward);

        Assert.Equal(OfficeViewEligibility.CommunityReduced, steward.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.AcknowledgeAllowed, steward.ActionEligibility);
    }

    private static BatchSpec CreateBatch(GovernedWeatherDisclosureReceipt receipt)
    {
        var entry = new GovernanceJournalEntry(
            LoopKey: receipt.LoopKey,
            Kind: GovernanceJournalEntryKind.WeatherDisclosure,
            Stage: receipt.Stage,
            Timestamp: receipt.TimestampUtc.UtcDateTime,
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
            WeatherDisclosureReceipt: receipt,
            OfficeAuthorityReceipt: null);

        return new BatchSpec(receipt.LoopKey, new GovernanceJournalReplayBatch([entry], []));
    }

    private static GovernedWeatherDisclosureReceipt CreateDisclosureReceipt(
        StewardCareRoutingState routingState,
        WeatherDisclosureScope disclosureScope,
        EvidenceSufficiencyState evidenceSufficiencyState = EvidenceSufficiencyState.Sufficient)
    {
        return new GovernedWeatherDisclosureReceipt(
            DisclosureHandle: "weather-disclosure://aaaaaaaaaaaaaaaa",
            LoopKey: "loop:office-authority:test",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            CMEId: "cme-office-authority",
            RoutingState: routingState,
            CadenceState: CheckInCadenceState.Current,
            EvidenceSufficiencyState: evidenceSufficiencyState,
            WindowIntegrityState: evidenceSufficiencyState == EvidenceSufficiencyState.Sufficient
                ? WindowIntegrityState.Intact
                : WindowIntegrityState.Sparse,
            DisclosureScope: disclosureScope,
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
                StewardAttentionCause.ResiduePersistence,
                StewardAttentionCause.WindowIntegrityBreak
            ],
            WithheldMarkers: disclosureScope == WeatherDisclosureScope.Community
                ? []
                : [WeatherWithheldMarker.GuardedEvidence],
            RationaleCode: disclosureScope == WeatherDisclosureScope.Community
                ? WeatherDisclosureRationaleCode.CommunityWhitelisted
                : WeatherDisclosureRationaleCode.GuardedReduction,
            WitnessedBy: "CradleTek",
            InnerWeatherHandle: "inner-weather://bbbbbbbbbbbbbbbb",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private sealed record BatchSpec(
        string FinalLoopKey,
        GovernanceJournalReplayBatch ReplayBatch);
}
