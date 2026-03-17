namespace Oan.Common;

public static class StewardCareRouter
{
    public static StewardCareAssessment? AssessForLoop(
        string loopKey,
        GovernanceJournalReplayBatch batch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(batch);

        var currentReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.InnerWeatherReceipt)
            .LastOrDefault(receipt => receipt is not null);
        if (currentReceipt is null)
        {
            return null;
        }

        var currentPacket = CommunityWeatherReducer.Reduce(currentReceipt);
        var recentReceipts = batch.Entries
            .Select(entry => entry.InnerWeatherReceipt)
            .Where(receipt => receipt is not null &&
                              string.Equals(receipt.CMEId, currentReceipt.CMEId, StringComparison.Ordinal))
            .Select(receipt => receipt!)
            .OrderBy(receipt => receipt.TimestampUtc)
            .ThenBy(receipt => receipt.InnerWeatherHandle, StringComparer.Ordinal)
            .TakeLast(3)
            .ToArray();
        var evidenceSufficiency = ClassifyEvidenceSufficiency(currentReceipt);
        var cadenceState = ClassifyCadence(currentReceipt, currentPacket, evidenceSufficiency);
        var hasGuardedInfluence = HasGuardedInfluence(currentReceipt);
        var hasCrypticInfluence = HasCrypticInfluence(currentReceipt);
        var reasonCodes = currentReceipt.StewardAttentionCauses
            .Distinct()
            .OrderBy(cause => cause)
            .ToArray();
        var routingState = ClassifyRoutingState(
            currentReceipt,
            currentPacket,
            recentReceipts,
            evidenceSufficiency,
            cadenceState);

        return new StewardCareAssessment(
            CMEId: currentReceipt.CMEId,
            RoutingState: routingState,
            CadenceState: cadenceState,
            EvidenceSufficiencyState: evidenceSufficiency,
            WindowIntegrityState: currentReceipt.WindowIntegrityState,
            CommunityWeatherPacket: currentPacket,
            HasGuardedInfluence: hasGuardedInfluence,
            HasCrypticInfluence: hasCrypticInfluence,
            ReasonCodes: reasonCodes,
            InnerWeatherHandle: currentReceipt.InnerWeatherHandle,
            TimestampUtc: currentReceipt.TimestampUtc);
    }

    private static EvidenceSufficiencyState ClassifyEvidenceSufficiency(
        GovernedInnerWeatherReceipt receipt)
    {
        return receipt.WindowIntegrityState switch
        {
            WindowIntegrityState.Intact when receipt.ObservationCount >= receipt.WindowSize => EvidenceSufficiencyState.Sufficient,
            WindowIntegrityState.Intact => EvidenceSufficiencyState.Sparse,
            WindowIntegrityState.Sparse => EvidenceSufficiencyState.Sparse,
            WindowIntegrityState.JournalGap or
            WindowIntegrityState.RuntimeRestart or
            WindowIntegrityState.GovernanceReset => EvidenceSufficiencyState.BrokenWindow,
            WindowIntegrityState.CmeReselected or
            WindowIntegrityState.VisibilityDowngraded => EvidenceSufficiencyState.ContinuityAmbiguous,
            _ => EvidenceSufficiencyState.BrokenWindow
        };
    }

    private static CheckInCadenceState ClassifyCadence(
        GovernedInnerWeatherReceipt receipt,
        CommunityWeatherPacket packet,
        EvidenceSufficiencyState evidenceSufficiency)
    {
        if (evidenceSufficiency == EvidenceSufficiencyState.BrokenWindow)
        {
            return CheckInCadenceState.Broken;
        }

        if (evidenceSufficiency is EvidenceSufficiencyState.Sparse or EvidenceSufficiencyState.ContinuityAmbiguous)
        {
            return CheckInCadenceState.Unknown;
        }

        if (receipt.HotCoolContactState == HotCoolContactState.MissedCheckIn)
        {
            return CheckInCadenceState.Overdue;
        }

        if (receipt.HotCoolContactState == HotCoolContactState.Unknown)
        {
            return CheckInCadenceState.Unknown;
        }

        if (receipt.HotCoolContactState == HotCoolContactState.InContact)
        {
            return CheckInCadenceState.Current;
        }

        if (packet.Status is CommunityWeatherStatus.Unstable or CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn ||
            receipt.DriftState != CompassDriftState.Held ||
            receipt.ResidueState is AttentionResidueState.Present or AttentionResidueState.Persistent or AttentionResidueState.Escalating ||
            receipt.ShellCompetitionState is ShellCompetitionState.Present or ShellCompetitionState.Rising)
        {
            return CheckInCadenceState.DueSoon;
        }

        return CheckInCadenceState.Current;
    }

    private static StewardCareRoutingState ClassifyRoutingState(
        GovernedInnerWeatherReceipt currentReceipt,
        CommunityWeatherPacket currentPacket,
        IReadOnlyList<GovernedInnerWeatherReceipt> recentReceipts,
        EvidenceSufficiencyState evidenceSufficiency,
        CheckInCadenceState cadenceState)
    {
        if (evidenceSufficiency != EvidenceSufficiencyState.Sufficient)
        {
            return currentPacket.Status is CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn
                ? StewardCareRoutingState.CheckInRecommended
                : StewardCareRoutingState.None;
        }

        var recentPackets = recentReceipts
            .Select(CommunityWeatherReducer.Reduce)
            .ToArray();
        var repeatedInstability = recentPackets.Count(packet =>
            packet.Status is CommunityWeatherStatus.Unstable or CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn) >= 2;
        var repeatedDegraded = recentPackets.Count(packet =>
            packet.Status is CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn) >= 2;
        var repeatedMissedCheckIn = recentPackets.Count(packet =>
            packet.Status == CommunityWeatherStatus.MissedCheckIn) >= 2;

        if (currentReceipt.DriftState == CompassDriftState.Lost && repeatedInstability)
        {
            return StewardCareRoutingState.EscalationEligible;
        }

        if (repeatedMissedCheckIn && currentPacket.Status == CommunityWeatherStatus.MissedCheckIn)
        {
            return StewardCareRoutingState.EscalationEligible;
        }

        if (currentPacket.Status is CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn ||
            currentReceipt.ResidueState is AttentionResidueState.Persistent or AttentionResidueState.Escalating ||
            cadenceState == CheckInCadenceState.Overdue ||
            repeatedDegraded)
        {
            return StewardCareRoutingState.CheckInNeeded;
        }

        if (currentPacket.Status is CommunityWeatherStatus.Unstable or CommunityWeatherStatus.Unknown ||
            currentReceipt.DriftState == CompassDriftState.Weakened ||
            currentReceipt.ResidueState == AttentionResidueState.Present ||
            cadenceState == CheckInCadenceState.DueSoon)
        {
            return StewardCareRoutingState.CheckInRecommended;
        }

        return StewardCareRoutingState.None;
    }

    private static bool HasGuardedInfluence(GovernedInnerWeatherReceipt receipt)
    {
        return receipt.ResidueVisibilityClass != CompassVisibilityClass.CommunityLegible ||
               receipt.ShellCompetitionVisibilityClass != CompassVisibilityClass.CommunityLegible ||
               receipt.HotCoolContactVisibilityClass != CompassVisibilityClass.CommunityLegible;
    }

    private static bool HasCrypticInfluence(GovernedInnerWeatherReceipt receipt)
    {
        return receipt.ResidueVisibilityClass == CompassVisibilityClass.CrypticOnly ||
               receipt.ShellCompetitionVisibilityClass == CompassVisibilityClass.CrypticOnly ||
               receipt.HotCoolContactVisibilityClass == CompassVisibilityClass.CrypticOnly;
    }
}
