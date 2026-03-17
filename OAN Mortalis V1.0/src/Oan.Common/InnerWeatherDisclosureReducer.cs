namespace Oan.Common;

public static class InnerWeatherDisclosureReducer
{
    private static readonly IReadOnlyList<CommunityWeatherField> CommunityFieldWhitelist =
    [
        CommunityWeatherField.Status,
        CommunityWeatherField.StewardAttention,
        CommunityWeatherField.AnchorState,
        CommunityWeatherField.VisibilityClass,
        CommunityWeatherField.TimestampUtc
    ];

    public static WeatherDisclosureDecision Reduce(StewardCareAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        var withheldMarkers = BuildWithheldMarkers(assessment);
        var disclosureScope = ClassifyDisclosureScope(assessment, withheldMarkers);
        var rationaleCode = ClassifyRationaleCode(assessment, disclosureScope, withheldMarkers);

        return new WeatherDisclosureDecision(
            CMEId: assessment.CMEId,
            DisclosureScope: disclosureScope,
            EvidenceSufficiencyState: assessment.EvidenceSufficiencyState,
            CommunityWeatherPacket: assessment.CommunityWeatherPacket,
            AllowedCommunityFields: CommunityFieldWhitelist,
            StewardReasonCodes: assessment.ReasonCodes,
            WithheldMarkers: withheldMarkers,
            RationaleCode: rationaleCode,
            InnerWeatherHandle: assessment.InnerWeatherHandle,
            TimestampUtc: assessment.TimestampUtc);
    }

    private static IReadOnlyList<WeatherWithheldMarker> BuildWithheldMarkers(
        StewardCareAssessment assessment)
    {
        var markers = new List<WeatherWithheldMarker>();

        if (assessment.HasGuardedInfluence)
        {
            markers.Add(WeatherWithheldMarker.GuardedEvidence);
        }

        if (assessment.HasCrypticInfluence)
        {
            markers.Add(WeatherWithheldMarker.CrypticEvidence);
        }

        switch (assessment.EvidenceSufficiencyState)
        {
            case EvidenceSufficiencyState.Sparse:
                markers.Add(WeatherWithheldMarker.SparseEvidence);
                break;
            case EvidenceSufficiencyState.BrokenWindow:
                markers.Add(WeatherWithheldMarker.BrokenWindow);
                break;
            case EvidenceSufficiencyState.ContinuityAmbiguous:
                markers.Add(WeatherWithheldMarker.ContinuityAmbiguous);
                break;
        }

        return markers
            .Distinct()
            .OrderBy(marker => marker)
            .ToArray();
    }

    private static WeatherDisclosureScope ClassifyDisclosureScope(
        StewardCareAssessment assessment,
        IReadOnlyList<WeatherWithheldMarker> withheldMarkers)
    {
        if (assessment.HasCrypticInfluence ||
            assessment.EvidenceSufficiencyState == EvidenceSufficiencyState.BrokenWindow ||
            assessment.RoutingState == StewardCareRoutingState.EscalationEligible)
        {
            return WeatherDisclosureScope.OperatorGuarded;
        }

        if (assessment.HasGuardedInfluence ||
            assessment.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient ||
            assessment.RoutingState != StewardCareRoutingState.None ||
            withheldMarkers.Count > 0)
        {
            return WeatherDisclosureScope.Steward;
        }

        return WeatherDisclosureScope.Community;
    }

    private static WeatherDisclosureRationaleCode ClassifyRationaleCode(
        StewardCareAssessment assessment,
        WeatherDisclosureScope disclosureScope,
        IReadOnlyList<WeatherWithheldMarker> withheldMarkers)
    {
        if (assessment.EvidenceSufficiencyState == EvidenceSufficiencyState.Sparse)
        {
            return WeatherDisclosureRationaleCode.SparseReduction;
        }

        if (assessment.EvidenceSufficiencyState == EvidenceSufficiencyState.BrokenWindow)
        {
            return WeatherDisclosureRationaleCode.BrokenWindowReduction;
        }

        if (assessment.EvidenceSufficiencyState == EvidenceSufficiencyState.ContinuityAmbiguous)
        {
            return WeatherDisclosureRationaleCode.ContinuityAmbiguousReduction;
        }

        if (disclosureScope == WeatherDisclosureScope.OperatorGuarded)
        {
            return WeatherDisclosureRationaleCode.OperatorGuardedReduction;
        }

        if (withheldMarkers.Count > 0)
        {
            return WeatherDisclosureRationaleCode.GuardedReduction;
        }

        return WeatherDisclosureRationaleCode.CommunityWhitelisted;
    }
}
