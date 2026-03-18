using Oan.Common;

namespace SoulFrame.Host;

public static class SoulFrameTheaterAuthorizationProjector
{
    public static SliTheaterAuthorizationReceipt DescribeCandidate(
        ZedThetaCandidateReceipt candidate,
        string sourceTheater,
        string requestedTheater,
        string witnessedBy = "SoulFrame.Host")
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        var state = ResolveAuthorizationState(candidate, sourceTheater, requestedTheater);
        var reasonCode = ResolveReasonCode(candidate, sourceTheater, requestedTheater, state);

        return new SliTheaterAuthorizationReceipt(
            CandidateHandle: candidate.CandidateHandle,
            SourceTheater: sourceTheater.Trim(),
            RequestedTheater: requestedTheater.Trim(),
            AuthorityClass: candidate.PacketDirective.AuthorityClass,
            AuthorizationState: state,
            ReasonCode: reasonCode,
            WitnessedBy: witnessedBy.Trim(),
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static SliTheaterAuthorizationState ResolveAuthorizationState(
        ZedThetaCandidateReceipt candidate,
        string sourceTheater,
        string requestedTheater)
    {
        var bridgeReview = ResolveBridgeReview(candidate, sourceTheater, requestedTheater);
        var runtimeUseCeiling = candidate.RuntimeUseCeiling
            ?? SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();

        if (bridgeReview.OutcomeKind != SliBridgeOutcomeKind.Ok ||
            bridgeReview.ThresholdClass == SliBridgeThresholdClass.FaultLine)
        {
            return SliTheaterAuthorizationState.Forbidden;
        }

        if (runtimeUseCeiling.CandidateOnly ||
            bridgeReview.ThresholdClass is SliBridgeThresholdClass.NearThreshold or SliBridgeThresholdClass.ThresholdBreach)
        {
            return SliTheaterAuthorizationState.Withheld;
        }

        return candidate.PacketDirective.AuthorityClass == SliAuthorityClass.AuthorityBearing
            ? SliTheaterAuthorizationState.Authorized
            : SliTheaterAuthorizationState.Withheld;
    }

    private static string ResolveReasonCode(
        ZedThetaCandidateReceipt candidate,
        string sourceTheater,
        string requestedTheater,
        SliTheaterAuthorizationState state)
    {
        var bridgeReview = ResolveBridgeReview(candidate, sourceTheater, requestedTheater);
        var runtimeUseCeiling = candidate.RuntimeUseCeiling
            ?? SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();

        if (state == SliTheaterAuthorizationState.Forbidden)
        {
            return bridgeReview.ReasonCode;
        }

        return state switch
        {
            SliTheaterAuthorizationState.Authorized => "sli-authority-bearing",
            SliTheaterAuthorizationState.Withheld => runtimeUseCeiling.CandidateOnly
                ? runtimeUseCeiling.ReasonCode
                : bridgeReview.ReasonCode,
            _ => "sli-theater-forbidden"
        };
    }

    private static SliBridgeReviewReceipt ResolveBridgeReview(
        ZedThetaCandidateReceipt candidate,
        string sourceTheater,
        string requestedTheater)
    {
        var bridgeReview = candidate.BridgeReview ?? SliBridgeContracts.CreateCandidateBridgeReview(
            bridgeStage: "soulframe-authorization-fallback",
            sourceTheater: sourceTheater,
            targetTheater: requestedTheater,
            bridgeWitnessHandle: candidate.CandidateHandle,
            thetaState: candidate.ThetaState,
            gammaState: candidate.GammaState,
            packetDirective: candidate.PacketDirective,
            identityKernelBoundary: candidate.IdentityKernelBoundary,
            validity: candidate.Validity,
            activeBasin: candidate.ActiveBasin,
            competingBasin: candidate.CompetingBasin,
            anchorState: candidate.AnchorState,
            selfTouchClass: candidate.SelfTouchClass);

        if (!string.Equals(sourceTheater.Trim(), requestedTheater.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            bridgeReview = SliBridgeContracts.CreateReview(
                bridgeStage: bridgeReview.BridgeStage,
                sourceTheater: sourceTheater,
                targetTheater: requestedTheater,
                bridgeWitnessHandle: bridgeReview.BridgeWitnessHandle,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-cross-theater-identification",
                refusalClass: SliBridgeRefusalClass.CrossTheaterIdentification);
        }

        return bridgeReview;
    }
}
