namespace Oan.Common;

public static class GoverningOfficeAuthorityViewReducer
{
    public static GoverningOfficeAuthorityView Reduce(GoverningOfficeAuthorityAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        return new GoverningOfficeAuthorityView(
            Office: assessment.Office,
            AuthoritySurface: assessment.AuthoritySurface,
            ViewEligibility: assessment.ViewEligibility,
            AcknowledgmentEligibility: assessment.AcknowledgmentEligibility,
            ActionEligibility: assessment.ActionEligibility,
            CommunityWeatherPacket: assessment.ViewEligibility == OfficeViewEligibility.Withheld
                ? null
                : assessment.CommunityWeatherPacket,
            AllowedReasonCodes: BuildAllowedReasonCodes(assessment),
            WithheldMarkers: BuildWithheldMarkers(assessment),
            Prohibitions: assessment.Prohibitions,
            RationaleCode: ClassifyRationaleCode(assessment),
            WeatherDisclosureHandle: assessment.WeatherDisclosureHandle,
            TimestampUtc: assessment.TimestampUtc);
    }

    private static IReadOnlyList<StewardAttentionCause> BuildAllowedReasonCodes(
        GoverningOfficeAuthorityAssessment assessment)
    {
        if (assessment.ViewEligibility is OfficeViewEligibility.Withheld or OfficeViewEligibility.CommunityReduced)
        {
            return [];
        }

        var allowed = assessment.Office switch
        {
            InternalGoverningCmeOffice.Steward => assessment.SourceReasonCodes,
            InternalGoverningCmeOffice.Father => assessment.SourceReasonCodes
                .Where(code => code is StewardAttentionCause.DriftLoss
                    or StewardAttentionCause.MissedCheckIn
                    or StewardAttentionCause.WindowIntegrityBreak)
                .ToArray(),
            InternalGoverningCmeOffice.Mother => assessment.SourceReasonCodes
                .Where(code => code is StewardAttentionCause.DriftWeakening
                    or StewardAttentionCause.ResiduePersistence
                    or StewardAttentionCause.MissedCheckIn)
                .ToArray(),
            _ => []
        };

        return allowed
            .Distinct()
            .OrderBy(code => code)
            .ToArray();
    }

    private static IReadOnlyList<OfficeAuthorityWithheldMarker> BuildWithheldMarkers(
        GoverningOfficeAuthorityAssessment assessment)
    {
        var markers = assessment.SourceWithheldMarkers
            .Select(MapMarker)
            .ToList();

        if (!assessment.OfficeAttached)
        {
            markers.Add(OfficeAuthorityWithheldMarker.OfficeNotAttached);
        }

        if (assessment.Office == InternalGoverningCmeOffice.Steward &&
            assessment.ActionEligibility == OfficeActionEligibility.EscalationReviewAllowed &&
            !assessment.BondedConfirmed)
        {
            markers.Add(OfficeAuthorityWithheldMarker.BondRequired);
        }

        if (assessment.Office == InternalGoverningCmeOffice.Mother &&
            assessment.OfficeAttached &&
            assessment.ViewEligibility == OfficeViewEligibility.Withheld &&
            !assessment.BondedConfirmed)
        {
            markers.Add(OfficeAuthorityWithheldMarker.BondRequired);
        }

        if (assessment.Office != InternalGoverningCmeOffice.Steward &&
            assessment.ViewEligibility != OfficeViewEligibility.Withheld)
        {
            markers.Add(OfficeAuthorityWithheldMarker.ActionNotAuthorized);
        }

        return markers
            .Distinct()
            .OrderBy(marker => marker)
            .ToArray();
    }

    private static OfficeAuthorityRationaleCode ClassifyRationaleCode(
        GoverningOfficeAuthorityAssessment assessment)
    {
        if (!assessment.OfficeAttached)
        {
            return OfficeAuthorityRationaleCode.WithheldForOfficeNotAttached;
        }

        if (assessment.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient)
        {
            return OfficeAuthorityRationaleCode.WithheldForInsufficientEvidence;
        }

        if (assessment.ViewEligibility == OfficeViewEligibility.Withheld)
        {
            return OfficeAuthorityRationaleCode.WithheldForAuthorityRequirement;
        }

        return assessment.ViewEligibility switch
        {
            OfficeViewEligibility.CommunityReduced => OfficeAuthorityRationaleCode.CommunityReducedForOffice,
            OfficeViewEligibility.GuardedView => OfficeAuthorityRationaleCode.GuardedViewForOffice,
            OfficeViewEligibility.OfficeSpecificView => OfficeAuthorityRationaleCode.OfficeSpecificStewardView,
            _ => OfficeAuthorityRationaleCode.WithheldForAuthorityRequirement
        };
    }

    private static OfficeAuthorityWithheldMarker MapMarker(WeatherWithheldMarker marker)
    {
        return marker switch
        {
            WeatherWithheldMarker.GuardedEvidence => OfficeAuthorityWithheldMarker.GuardedEvidence,
            WeatherWithheldMarker.CrypticEvidence => OfficeAuthorityWithheldMarker.CrypticEvidence,
            WeatherWithheldMarker.SparseEvidence => OfficeAuthorityWithheldMarker.SparseEvidence,
            WeatherWithheldMarker.BrokenWindow => OfficeAuthorityWithheldMarker.BrokenWindow,
            WeatherWithheldMarker.ContinuityAmbiguous => OfficeAuthorityWithheldMarker.ContinuityAmbiguous,
            _ => OfficeAuthorityWithheldMarker.GuardedEvidence
        };
    }
}
