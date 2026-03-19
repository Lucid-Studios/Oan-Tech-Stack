namespace Oan.Common;

public enum SliBridgeOutcomeKind
{
    Ok = 0,
    NeedsSpec = 1,
    Reject = 2,
    RefuseContext = 3
}

public enum SliBridgeThresholdClass
{
    WithinBand = 0,
    NearThreshold = 1,
    ThresholdBreach = 2,
    FaultLine = 3
}

public enum SliBridgeRefusalClass
{
    None = 0,
    StageSkip = 1,
    CrossTheaterIdentification = 2,
    MissingWitness = 3,
    UnlawfulPromotion = 4
}

public enum SliPreBondScope
{
    WitnessableProtectiveSubset = 0
}

public enum SliPreBondSafeguardClass
{
    None = 0,
    PredatorySharedDomainRisk = 1,
    CoerciveBondingPosture = 2,
    ContinuityInstability = 3,
    IdentityOvercollapseRisk = 4
}

public enum SliPreBondSafeguardDisposition
{
    Witness = 0,
    Hold = 1,
    Refuse = 2,
    Escalate = 3
}

public sealed record SliPreBondSafeguardReceipt(
    SliPreBondScope Scope,
    SliPreBondSafeguardClass SafeguardClass,
    SliPreBondSafeguardDisposition Disposition,
    string ReasonCode,
    bool RequiresEscalation,
    string WitnessedBy);

public sealed record SliBridgeReviewReceipt(
    string BridgeStage,
    string SourceTheater,
    string TargetTheater,
    string BridgeWitnessHandle,
    bool WitnessPresent,
    SliBridgeOutcomeKind OutcomeKind,
    SliBridgeThresholdClass ThresholdClass,
    SliBridgeRefusalClass RefusalClass,
    string ReasonCode,
    SliPreBondSafeguardReceipt? PreBondSafeguard = null);

public sealed record SliRuntimeUseCeilingReceipt(
    bool CandidateOnly,
    bool PersistenceAuthorityGranted,
    bool DeploymentAuthorityGranted,
    bool HaltAuthorityGranted,
    string ReasonCode);

public static class SliBridgeContracts
{
    public static SliRuntimeUseCeilingReceipt CreateCandidateOnlyRuntimeUseCeiling(
        string reasonCode = "sli-runtime-candidate-only")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        return new SliRuntimeUseCeilingReceipt(
            CandidateOnly: true,
            PersistenceAuthorityGranted: false,
            DeploymentAuthorityGranted: false,
            HaltAuthorityGranted: false,
            ReasonCode: reasonCode.Trim());
    }

    public static SliBridgeReviewReceipt CreateReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        SliBridgeOutcomeKind outcomeKind,
        SliBridgeThresholdClass thresholdClass,
        string reasonCode,
        SliBridgeRefusalClass refusalClass = SliBridgeRefusalClass.None,
        SliPreBondSafeguardReceipt? preBondSafeguard = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bridgeStage);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        var normalizedWitnessHandle = string.IsNullOrWhiteSpace(bridgeWitnessHandle)
            ? string.Empty
            : bridgeWitnessHandle.Trim();

        return new SliBridgeReviewReceipt(
            BridgeStage: bridgeStage.Trim(),
            SourceTheater: sourceTheater.Trim(),
            TargetTheater: targetTheater.Trim(),
            BridgeWitnessHandle: normalizedWitnessHandle,
            WitnessPresent: !string.IsNullOrWhiteSpace(normalizedWitnessHandle),
            OutcomeKind: outcomeKind,
            ThresholdClass: thresholdClass,
            RefusalClass: refusalClass,
            ReasonCode: reasonCode.Trim(),
            PreBondSafeguard: preBondSafeguard);
    }

    public static SliPreBondSafeguardReceipt CreatePreBondSafeguard(
        SliPreBondSafeguardClass safeguardClass,
        SliPreBondSafeguardDisposition disposition,
        string reasonCode,
        bool requiresEscalation = false,
        string witnessedBy = "CradleTek",
        SliPreBondScope scope = SliPreBondScope.WitnessableProtectiveSubset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new SliPreBondSafeguardReceipt(
            Scope: scope,
            SafeguardClass: safeguardClass,
            Disposition: disposition,
            ReasonCode: reasonCode.Trim(),
            RequiresEscalation: requiresEscalation,
            WitnessedBy: witnessedBy.Trim());
    }

    public static SliBridgeReviewReceipt CreatePreBondProtectiveReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        SliBridgeOutcomeKind outcomeKind,
        SliBridgeThresholdClass thresholdClass,
        string reasonCode,
        SliPreBondSafeguardClass safeguardClass,
        SliPreBondSafeguardDisposition disposition,
        bool requiresEscalation = false,
        SliBridgeRefusalClass refusalClass = SliBridgeRefusalClass.None,
        string witnessedBy = "CradleTek")
    {
        return CreateReview(
            bridgeStage: bridgeStage,
            sourceTheater: sourceTheater,
            targetTheater: targetTheater,
            bridgeWitnessHandle: bridgeWitnessHandle,
            outcomeKind: outcomeKind,
            thresholdClass: thresholdClass,
            reasonCode: reasonCode,
            refusalClass: refusalClass,
            preBondSafeguard: CreatePreBondSafeguard(
                safeguardClass,
                disposition,
                reasonCode,
                requiresEscalation,
                witnessedBy));
    }

    public static bool HasBlockingPreBondSafeguard(SliBridgeReviewReceipt? bridgeReview)
    {
        return bridgeReview?.PreBondSafeguard?.Disposition is
            SliPreBondSafeguardDisposition.Hold or
            SliPreBondSafeguardDisposition.Refuse or
            SliPreBondSafeguardDisposition.Escalate;
    }

    public static SliBridgeReviewReceipt CreateCandidateBridgeReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        string thetaState,
        string gammaState,
        SliPacketDirective packetDirective,
        IdentityKernelBoundaryReceipt identityKernelBoundary,
        SliPacketValidityReceipt validity,
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin,
        CompassAnchorState anchorState,
        CompassSelfTouchClass selfTouchClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(thetaState);
        ArgumentException.ThrowIfNullOrWhiteSpace(gammaState);
        ArgumentNullException.ThrowIfNull(packetDirective);
        ArgumentNullException.ThrowIfNull(identityKernelBoundary);
        ArgumentNullException.ThrowIfNull(validity);

        if (!IsStageReady(thetaState, gammaState))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-stage-skip",
                refusalClass: SliBridgeRefusalClass.StageSkip);
        }

        if (!string.Equals(sourceTheater, targetTheater, StringComparison.OrdinalIgnoreCase))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.RefuseContext,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-cross-theater-identification",
                refusalClass: SliBridgeRefusalClass.CrossTheaterIdentification);
        }

        if (!HasWitness(bridgeWitnessHandle, identityKernelBoundary))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-missing-witness",
                refusalClass: SliBridgeRefusalClass.MissingWitness);
        }

        if (packetDirective.AuthorityClass != SliAuthorityClass.CandidateBearing)
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-unlawful-promotion",
                refusalClass: SliBridgeRefusalClass.UnlawfulPromotion);
        }

        var thresholdClass = ClassifyThreshold(
            activeBasin,
            competingBasin,
            anchorState,
            selfTouchClass,
            packetDirective.UpdateLocus,
            validity);

        return thresholdClass switch
        {
            SliBridgeThresholdClass.FaultLine => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                thresholdClass,
                reasonCode: validity.ReasonCode),
            SliBridgeThresholdClass.ThresholdBreach => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                thresholdClass,
                reasonCode: !validity.PolicyEligible || !validity.ScepOk
                    ? validity.ReasonCode
                    : "sli-bridge-threshold-breach"),
            SliBridgeThresholdClass.NearThreshold => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                thresholdClass,
                reasonCode: "sli-bridge-near-threshold"),
            _ => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band")
        };
    }

    public static SliBridgeThresholdClass ClassifyThreshold(
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin,
        CompassAnchorState anchorState,
        CompassSelfTouchClass selfTouchClass,
        SliUpdateLocus updateLocus,
        SliPacketValidityReceipt validity)
    {
        ArgumentNullException.ThrowIfNull(validity);

        if (updateLocus == SliUpdateLocus.Reject ||
            !AreBasinsAdjacent(activeBasin, competingBasin) ||
            anchorState == CompassAnchorState.Lost)
        {
            return SliBridgeThresholdClass.FaultLine;
        }

        if (!validity.PolicyEligible ||
            !validity.ScepOk ||
            anchorState == CompassAnchorState.Weakened)
        {
            return SliBridgeThresholdClass.ThresholdBreach;
        }

        if (competingBasin != CompassDoctrineBasin.Unknown ||
            selfTouchClass == CompassSelfTouchClass.BoundaryContact)
        {
            return SliBridgeThresholdClass.NearThreshold;
        }

        return SliBridgeThresholdClass.WithinBand;
    }

    public static bool AreBasinsAdjacent(
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin)
    {
        if (activeBasin == competingBasin)
        {
            return true;
        }

        return (activeBasin, competingBasin) switch
        {
            (CompassDoctrineBasin.BoundedLocalityContinuity, CompassDoctrineBasin.FluidContinuityLaw) => true,
            (CompassDoctrineBasin.FluidContinuityLaw, CompassDoctrineBasin.BoundedLocalityContinuity) => true,
            _ => false
        };
    }

    private static bool HasWitness(
        string bridgeWitnessHandle,
        IdentityKernelBoundaryReceipt identityKernelBoundary)
    {
        return !string.IsNullOrWhiteSpace(bridgeWitnessHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.CmeIdentityHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.IdentityKernelHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.ContinuityAnchorHandle);
    }

    private static bool IsStageReady(string thetaState, string gammaState)
    {
        return string.Equals(thetaState, "theta-ready", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(gammaState, "gamma-ready", StringComparison.OrdinalIgnoreCase);
    }
}
