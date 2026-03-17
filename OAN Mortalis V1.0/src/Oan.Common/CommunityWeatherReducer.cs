namespace Oan.Common;

public static class CommunityWeatherReducer
{
    public static CommunityWeatherPacket Reduce(GovernedInnerWeatherReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        var status = ReduceStatus(receipt);
        var stewardAttention = ReduceStewardAttention(receipt, status);

        return new CommunityWeatherPacket(
            Status: status,
            StewardAttention: stewardAttention,
            AnchorState: receipt.DriftState,
            VisibilityClass: CompassVisibilityClass.CommunityLegible,
            TimestampUtc: receipt.TimestampUtc);
    }

    private static CommunityWeatherStatus ReduceStatus(GovernedInnerWeatherReceipt receipt)
    {
        if (receipt.HotCoolContactState == HotCoolContactState.MissedCheckIn)
        {
            return CommunityWeatherStatus.MissedCheckIn;
        }

        if (receipt.WindowIntegrityState != WindowIntegrityState.Intact)
        {
            return receipt.WindowIntegrityState == WindowIntegrityState.Sparse
                ? CommunityWeatherStatus.Unknown
                : CommunityWeatherStatus.Degraded;
        }

        if (receipt.DriftState == CompassDriftState.Lost ||
            receipt.ResidueState == AttentionResidueState.Escalating ||
            receipt.ShellCompetitionState == ShellCompetitionState.Rising)
        {
            return CommunityWeatherStatus.Degraded;
        }

        if (receipt.DriftState == CompassDriftState.Weakened ||
            receipt.ResidueState is AttentionResidueState.Present or AttentionResidueState.Persistent ||
            receipt.ShellCompetitionState == ShellCompetitionState.Present)
        {
            return CommunityWeatherStatus.Unstable;
        }

        if (receipt.DriftState == CompassDriftState.Held &&
            receipt.ResidueState is AttentionResidueState.None or AttentionResidueState.Low &&
            receipt.ShellCompetitionState == ShellCompetitionState.Absent &&
            receipt.WindowIntegrityState == WindowIntegrityState.Intact)
        {
            return CommunityWeatherStatus.Stable;
        }

        return CommunityWeatherStatus.Unknown;
    }

    private static CommunityStewardAttentionState ReduceStewardAttention(
        GovernedInnerWeatherReceipt receipt,
        CommunityWeatherStatus status)
    {
        if (status is CommunityWeatherStatus.Degraded or CommunityWeatherStatus.MissedCheckIn ||
            receipt.DriftState == CompassDriftState.Lost ||
            receipt.ResidueState == AttentionResidueState.Escalating)
        {
            return CommunityStewardAttentionState.Needed;
        }

        if (status == CommunityWeatherStatus.Unstable ||
            status == CommunityWeatherStatus.Unknown ||
            receipt.ResidueState is AttentionResidueState.Present or AttentionResidueState.Persistent ||
            receipt.ShellCompetitionState == ShellCompetitionState.Present)
        {
            return CommunityStewardAttentionState.Recommended;
        }

        return CommunityStewardAttentionState.None;
    }
}
