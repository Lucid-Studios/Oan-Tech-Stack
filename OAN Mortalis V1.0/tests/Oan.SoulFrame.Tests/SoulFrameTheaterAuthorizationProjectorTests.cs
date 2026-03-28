using Oan.Common;
using SoulFrame.Host;

namespace Oan.SoulFrame.Tests;

public sealed class SoulFrameTheaterAuthorizationProjectorTests
{
    [Fact]
    public void DescribeCandidate_CandidateBearingReceipt_IsWithheld()
    {
        var receipt = SoulFrameTheaterAuthorizationProjector.DescribeCandidate(
            CreateCandidateReceipt(SliAuthorityClass.CandidateBearing, SliUpdateLocus.Sheaf),
            sourceTheater: "prime",
            requestedTheater: "prime");

        Assert.Equal(SliTheaterAuthorizationState.Withheld, receipt.AuthorizationState);
        Assert.Equal("sli-runtime-candidate-only", receipt.ReasonCode);
    }

    [Fact]
    public void DescribeCandidate_AuthorityBearingReceipt_IsAuthorized()
    {
        var receipt = SoulFrameTheaterAuthorizationProjector.DescribeCandidate(
            CreateCandidateReceipt(
                SliAuthorityClass.AuthorityBearing,
                SliUpdateLocus.Kernel,
                explicitBridgeReview: SliBridgeContracts.CreateReview(
                    bridgeStage: "zed-theta-candidate",
                    sourceTheater: "prime",
                    targetTheater: "prime",
                    bridgeWitnessHandle: "sli-bridge://test",
                    outcomeKind: SliBridgeOutcomeKind.Ok,
                    thresholdClass: SliBridgeThresholdClass.WithinBand,
                    reasonCode: "sli-bridge-within-band"),
                runtimeUseCeiling: new SliRuntimeUseCeilingReceipt(
                    CandidateOnly: false,
                    PersistenceAuthorityGranted: true,
                    DeploymentAuthorityGranted: false,
                    HaltAuthorityGranted: false,
                    ReasonCode: "sli-runtime-authority-granted")),
            sourceTheater: "prime",
            requestedTheater: "prime");

        Assert.Equal(SliTheaterAuthorizationState.Authorized, receipt.AuthorizationState);
        Assert.Equal("sli-authority-bearing", receipt.ReasonCode);
    }

    [Fact]
    public void DescribeCandidate_InvalidCrossTheaterRequest_IsForbidden()
    {
        var candidate = CreateCandidateReceipt(SliAuthorityClass.CandidateBearing, SliUpdateLocus.Sheaf) with
        {
            Validity = new SliPacketValidityReceipt(
                SyntaxOk: true,
                HexadOk: true,
                ScepOk: false,
                PolicyEligible: true,
                ReasonCode: "sli-scep-reject")
        };

        var receipt = SoulFrameTheaterAuthorizationProjector.DescribeCandidate(
            candidate,
            sourceTheater: "prime",
            requestedTheater: "community");

        Assert.Equal(SliTheaterAuthorizationState.Forbidden, receipt.AuthorizationState);
        Assert.Equal("sli-bridge-cross-theater-identification", receipt.ReasonCode);
    }

    [Fact]
    public void DescribeCandidate_UnlawfulPromotion_IsForbidden()
    {
        var candidate = CreateCandidateReceipt(SliAuthorityClass.AuthorityBearing, SliUpdateLocus.Kernel);

        var receipt = SoulFrameTheaterAuthorizationProjector.DescribeCandidate(
            candidate,
            sourceTheater: "prime",
            requestedTheater: "prime");

        Assert.Equal(SliTheaterAuthorizationState.Forbidden, receipt.AuthorizationState);
        Assert.Equal("sli-bridge-unlawful-promotion", receipt.ReasonCode);
    }

    private static ZedThetaCandidateReceipt CreateCandidateReceipt(
        SliAuthorityClass authorityClass,
        SliUpdateLocus updateLocus,
        SliBridgeReviewReceipt? explicitBridgeReview = null,
        SliRuntimeUseCeilingReceipt? runtimeUseCeiling = null)
    {
        var packetDirective = new SliPacketDirective(
            SliThinkingTier.Master,
            SliPacketClass.Commitment,
            SliEngramOperation.Write,
            updateLocus,
            authorityClass);
        var activeBasin = updateLocus == SliUpdateLocus.Kernel
            ? CompassDoctrineBasin.IdentityContinuity
            : CompassDoctrineBasin.GeneralContinuityDiscourse;
        var competingBasin = activeBasin;
        var identityKernelBoundary = new IdentityKernelBoundaryReceipt(
            CmeIdentityHandle: "cme:test",
            IdentityKernelHandle: "kernel:test",
            ContinuityAnchorHandle: "anchor:test",
            KernelBound: updateLocus == SliUpdateLocus.Kernel,
            CandidateLocus: updateLocus);
        var validity = new SliPacketValidityReceipt(
            SyntaxOk: true,
            HexadOk: true,
            ScepOk: true,
            PolicyEligible: true,
            ReasonCode: "sli-packet-valid");
        var bridgeReview = SliBridgeContracts.CreateCandidateBridgeReview(
            bridgeStage: "zed-theta-candidate",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: "sli-bridge://test",
            thetaState: "theta-ready",
            gammaState: "gamma-ready",
            packetDirective: packetDirective,
            identityKernelBoundary: identityKernelBoundary,
            validity: validity,
            activeBasin: activeBasin,
            competingBasin: competingBasin,
            anchorState: CompassAnchorState.Held,
            selfTouchClass: CompassSelfTouchClass.ValidationTouch);

        return new ZedThetaCandidateReceipt(
            CandidateHandle: "zed-theta:test",
            Objective: "identity-continuity",
            PrimeState: "task-objective",
            ThetaState: "theta-ready",
            GammaState: "gamma-ready",
            PacketDirective: packetDirective,
            IdentityKernelBoundary: identityKernelBoundary,
            Validity: validity,
            ActiveBasin: activeBasin,
            CompetingBasin: competingBasin,
            AnchorState: CompassAnchorState.Held,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            OeCoePosture: CompassOeCoePosture.OeDominant,
            BridgeReview: explicitBridgeReview ?? bridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling ?? SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling());
    }
}
