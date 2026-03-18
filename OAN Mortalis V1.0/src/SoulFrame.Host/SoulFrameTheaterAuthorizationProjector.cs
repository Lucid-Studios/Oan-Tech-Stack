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
        if (!candidate.Validity.PolicyEligible || !candidate.Validity.ScepOk)
        {
            return SliTheaterAuthorizationState.Forbidden;
        }

        if (!string.Equals(sourceTheater.Trim(), requestedTheater.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return SliTheaterAuthorizationState.Forbidden;
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
        if (state == SliTheaterAuthorizationState.Forbidden)
        {
            if (!candidate.Validity.PolicyEligible || !candidate.Validity.ScepOk)
            {
                return candidate.Validity.ReasonCode;
            }

            if (!string.Equals(sourceTheater.Trim(), requestedTheater.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return "sli-cross-theater-forbidden";
            }
        }

        return state switch
        {
            SliTheaterAuthorizationState.Authorized => "sli-authority-bearing",
            SliTheaterAuthorizationState.Withheld => "sli-candidate-bearing-only",
            _ => "sli-theater-forbidden"
        };
    }
}
