namespace Oan.Common;

public static class GoverningOfficeAuthorityResolver
{
    private static readonly IReadOnlyList<InternalGoverningCmeOffice> OrderedOffices =
    [
        InternalGoverningCmeOffice.Steward,
        InternalGoverningCmeOffice.Father,
        InternalGoverningCmeOffice.Mother
    ];

    public static IReadOnlyList<GoverningOfficeAuthorityAssessment> AssessForLoop(
        string loopKey,
        GovernanceJournalReplayBatch batch,
        IReadOnlyDictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>? officeContexts = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(batch);

        var currentReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.WeatherDisclosureReceipt)
            .LastOrDefault(receipt => receipt is not null);
        if (currentReceipt is null)
        {
            return [];
        }

        return OrderedOffices
            .Select(office => AssessOffice(currentReceipt, ResolveContext(office, officeContexts)))
            .ToArray();
    }

    private static GoverningOfficeAuthorityAssessment AssessOffice(
        GovernedWeatherDisclosureReceipt receipt,
        GoverningOfficeAuthorityContext context)
    {
        var viewEligibility = ClassifyViewEligibility(receipt, context);
        var actionEligibility = ClassifyActionEligibility(receipt, context, viewEligibility);
        var acknowledgmentEligibility = viewEligibility == OfficeViewEligibility.Withheld || !context.OfficeAttached
            ? OfficeAcknowledgmentEligibility.NotAllowed
            : OfficeAcknowledgmentEligibility.Allowed;

        return new GoverningOfficeAuthorityAssessment(
            CMEId: receipt.CMEId,
            Office: context.Office,
            AuthoritySurface: ClassifySurface(receipt, context, viewEligibility),
            ViewEligibility: viewEligibility,
            AcknowledgmentEligibility: acknowledgmentEligibility,
            ActionEligibility: actionEligibility,
            EvidenceSufficiencyState: receipt.EvidenceSufficiencyState,
            WindowIntegrityState: receipt.WindowIntegrityState,
            DisclosureScope: receipt.DisclosureScope,
            OfficeAttached: context.OfficeAttached,
            BondedConfirmed: context.BondedConfirmed,
            GuardedReviewConfirmed: context.GuardedReviewConfirmed,
            CommunityWeatherPacket: receipt.CommunityWeatherPacket,
            SourceReasonCodes: receipt.StewardReasonCodes
                .Distinct()
                .OrderBy(code => code)
                .ToArray(),
            SourceWithheldMarkers: receipt.WithheldMarkers
                .Distinct()
                .OrderBy(marker => marker)
                .ToArray(),
            Prohibitions: BuildProhibitions(context, receipt, actionEligibility),
            WeatherDisclosureHandle: receipt.DisclosureHandle,
            TimestampUtc: receipt.TimestampUtc);
    }

    private static GoverningOfficeAuthorityContext ResolveContext(
        InternalGoverningCmeOffice office,
        IReadOnlyDictionary<InternalGoverningCmeOffice, GoverningOfficeAuthorityContext>? officeContexts)
    {
        if (officeContexts is not null &&
            officeContexts.TryGetValue(office, out var explicitContext))
        {
            return explicitContext;
        }

        return office switch
        {
            InternalGoverningCmeOffice.Steward => new GoverningOfficeAuthorityContext(
                Office: office,
                OfficeAttached: true,
                BondedConfirmed: false,
                GuardedReviewConfirmed: true),
            InternalGoverningCmeOffice.Father => new GoverningOfficeAuthorityContext(
                Office: office,
                OfficeAttached: false,
                BondedConfirmed: false,
                GuardedReviewConfirmed: false),
            InternalGoverningCmeOffice.Mother => new GoverningOfficeAuthorityContext(
                Office: office,
                OfficeAttached: false,
                BondedConfirmed: false,
                GuardedReviewConfirmed: false),
            _ => throw new ArgumentOutOfRangeException(nameof(office), office, null)
        };
    }

    private static OfficeAuthoritySurface ClassifySurface(
        GovernedWeatherDisclosureReceipt receipt,
        GoverningOfficeAuthorityContext context,
        OfficeViewEligibility viewEligibility)
    {
        return context.Office switch
        {
            InternalGoverningCmeOffice.Steward => OfficeAuthoritySurface.StewardSurface,
            InternalGoverningCmeOffice.Father when
                viewEligibility == OfficeViewEligibility.GuardedView ||
                receipt.DisclosureScope == WeatherDisclosureScope.OperatorGuarded ||
                context.GuardedReviewConfirmed => OfficeAuthoritySurface.GuardedReviewSurface,
            InternalGoverningCmeOffice.Father => OfficeAuthoritySurface.CrypticBoundarySurface,
            InternalGoverningCmeOffice.Mother => OfficeAuthoritySurface.PrimeCareSurface,
            _ => throw new ArgumentOutOfRangeException(nameof(context.Office), context.Office, null)
        };
    }

    private static OfficeViewEligibility ClassifyViewEligibility(
        GovernedWeatherDisclosureReceipt receipt,
        GoverningOfficeAuthorityContext context)
    {
        if (!context.OfficeAttached)
        {
            return OfficeViewEligibility.Withheld;
        }

        if (receipt.EvidenceSufficiencyState is EvidenceSufficiencyState.BrokenWindow or EvidenceSufficiencyState.ContinuityAmbiguous)
        {
            return context.Office == InternalGoverningCmeOffice.Steward
                ? OfficeViewEligibility.CommunityReduced
                : OfficeViewEligibility.Withheld;
        }

        if (receipt.EvidenceSufficiencyState == EvidenceSufficiencyState.Sparse)
        {
            return context.Office == InternalGoverningCmeOffice.Steward
                ? OfficeViewEligibility.CommunityReduced
                : OfficeViewEligibility.Withheld;
        }

        return context.Office switch
        {
            InternalGoverningCmeOffice.Steward => OfficeViewEligibility.OfficeSpecificView,
            InternalGoverningCmeOffice.Father when !context.GuardedReviewConfirmed &&
                                                  receipt.DisclosureScope != WeatherDisclosureScope.Community => OfficeViewEligibility.Withheld,
            InternalGoverningCmeOffice.Father when receipt.DisclosureScope == WeatherDisclosureScope.Community => OfficeViewEligibility.CommunityReduced,
            InternalGoverningCmeOffice.Father => OfficeViewEligibility.GuardedView,
            InternalGoverningCmeOffice.Mother when receipt.DisclosureScope == WeatherDisclosureScope.Community => OfficeViewEligibility.CommunityReduced,
            InternalGoverningCmeOffice.Mother when !context.BondedConfirmed => OfficeViewEligibility.Withheld,
            InternalGoverningCmeOffice.Mother => OfficeViewEligibility.GuardedView,
            _ => OfficeViewEligibility.Withheld
        };
    }

    private static OfficeActionEligibility ClassifyActionEligibility(
        GovernedWeatherDisclosureReceipt receipt,
        GoverningOfficeAuthorityContext context,
        OfficeViewEligibility viewEligibility)
    {
        if (!context.OfficeAttached || viewEligibility == OfficeViewEligibility.Withheld)
        {
            return OfficeActionEligibility.ViewOnly;
        }

        if (context.Office != InternalGoverningCmeOffice.Steward)
        {
            return OfficeActionEligibility.AcknowledgeAllowed;
        }

        if (receipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient)
        {
            return OfficeActionEligibility.AcknowledgeAllowed;
        }

        return receipt.RoutingState switch
        {
            StewardCareRoutingState.None => OfficeActionEligibility.AcknowledgeAllowed,
            StewardCareRoutingState.CheckInRecommended or StewardCareRoutingState.CheckInNeeded => OfficeActionEligibility.CheckInAllowed,
            StewardCareRoutingState.EscalationEligible when context.BondedConfirmed => OfficeActionEligibility.HandoffEligible,
            StewardCareRoutingState.EscalationEligible => OfficeActionEligibility.EscalationReviewAllowed,
            _ => OfficeActionEligibility.ViewOnly
        };
    }

    private static IReadOnlyList<OfficeAuthorityProhibition> BuildProhibitions(
        GoverningOfficeAuthorityContext context,
        GovernedWeatherDisclosureReceipt receipt,
        OfficeActionEligibility actionEligibility)
    {
        var prohibitions = new List<OfficeAuthorityProhibition>
        {
            OfficeAuthorityProhibition.MayNotOriginateTruth,
            OfficeAuthorityProhibition.MayNotWidenDisclosure
        };

        switch (context.Office)
        {
            case InternalGoverningCmeOffice.Steward:
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverrideCrypticBoundary);
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverridePrimeContinuity);
                break;
            case InternalGoverningCmeOffice.Father:
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverrideStewardContinuity);
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverridePrimeContinuity);
                prohibitions.Add(OfficeAuthorityProhibition.MayNotAuthorPublicDisclosure);
                break;
            case InternalGoverningCmeOffice.Mother:
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverrideStewardContinuity);
                prohibitions.Add(OfficeAuthorityProhibition.MayNotOverrideCrypticBoundary);
                prohibitions.Add(OfficeAuthorityProhibition.MayNotAuthorPublicDisclosure);
                break;
        }

        if (!context.OfficeAttached || receipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient)
        {
            prohibitions.Add(OfficeAuthorityProhibition.MayNotActWithoutHostEligibility);
        }

        if (context.Office == InternalGoverningCmeOffice.Steward &&
            receipt.RoutingState == StewardCareRoutingState.EscalationEligible &&
            actionEligibility != OfficeActionEligibility.HandoffEligible)
        {
            prohibitions.Add(OfficeAuthorityProhibition.MayNotBypassBond);
        }

        return prohibitions
            .Distinct()
            .OrderBy(prohibition => prohibition)
            .ToArray();
    }
}
