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
        Assert.Equal("sli-candidate-bearing-only", receipt.ReasonCode);
    }

    [Fact]
    public void DescribeCandidate_AuthorityBearingReceipt_IsAuthorized()
    {
        var receipt = SoulFrameTheaterAuthorizationProjector.DescribeCandidate(
            CreateCandidateReceipt(SliAuthorityClass.AuthorityBearing, SliUpdateLocus.Kernel),
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
        Assert.Equal("sli-scep-reject", receipt.ReasonCode);
    }

    private static ZedThetaCandidateReceipt CreateCandidateReceipt(
        SliAuthorityClass authorityClass,
        SliUpdateLocus updateLocus)
    {
        return new ZedThetaCandidateReceipt(
            CandidateHandle: "zed-theta:test",
            Objective: "identity-continuity",
            PrimeState: "task-objective",
            ThetaState: "theta-ready",
            GammaState: "gamma-ready",
            PacketDirective: new SliPacketDirective(
                SliThinkingTier.Master,
                SliPacketClass.Commitment,
                SliEngramOperation.Write,
                updateLocus,
                authorityClass),
            IdentityKernelBoundary: new IdentityKernelBoundaryReceipt(
                CmeIdentityHandle: "cme:test",
                IdentityKernelHandle: "kernel:test",
                ContinuityAnchorHandle: "anchor:test",
                KernelBound: updateLocus == SliUpdateLocus.Kernel,
                CandidateLocus: updateLocus),
            Validity: new SliPacketValidityReceipt(
                SyntaxOk: true,
                HexadOk: true,
                ScepOk: true,
                PolicyEligible: true,
                ReasonCode: "sli-packet-valid"),
            ActiveBasin: CompassDoctrineBasin.IdentityContinuity,
            CompetingBasin: CompassDoctrineBasin.Unknown,
            AnchorState: CompassAnchorState.Held,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            OeCoePosture: CompassOeCoePosture.OeDominant);
    }
}
