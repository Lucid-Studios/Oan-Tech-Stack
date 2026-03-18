using System.Text.Json.Serialization;

namespace Oan.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliThinkingTier
{
    Master = 0,
    Legendary = 1,
    Mythic = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliPacketClass
{
    Observation = 0,
    Commitment = 1,
    Revision = 2,
    Refusal = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliEngramOperation
{
    Write = 0,
    Amend = 1,
    Deprecate = 2,
    Refuse = 3,
    NoOp = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliUpdateLocus
{
    Kernel = 0,
    Sheaf = 1,
    Gap = 2,
    Reject = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliAuthorityClass
{
    CandidateBearing = 0,
    AuthorityBearing = 1
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SliTheaterAuthorizationState
{
    Authorized = 0,
    Withheld = 1,
    Forbidden = 2
}

public sealed record SliPacketDirective(
    SliThinkingTier ThinkingTier,
    SliPacketClass PacketClass,
    SliEngramOperation EngramOperation,
    SliUpdateLocus UpdateLocus,
    SliAuthorityClass AuthorityClass);

public sealed record IdentityKernelBoundaryReceipt(
    string CmeIdentityHandle,
    string IdentityKernelHandle,
    string ContinuityAnchorHandle,
    bool KernelBound,
    SliUpdateLocus CandidateLocus);

public sealed record SliPacketValidityReceipt(
    bool SyntaxOk,
    bool HexadOk,
    bool ScepOk,
    bool PolicyEligible,
    string ReasonCode);

public sealed record ZedThetaCandidateReceipt(
    string CandidateHandle,
    string Objective,
    string PrimeState,
    string ThetaState,
    string GammaState,
    SliPacketDirective PacketDirective,
    IdentityKernelBoundaryReceipt IdentityKernelBoundary,
    SliPacketValidityReceipt Validity,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassAnchorState AnchorState,
    CompassSelfTouchClass SelfTouchClass,
    CompassOeCoePosture OeCoePosture);

public sealed record SliTheaterAuthorizationReceipt(
    string CandidateHandle,
    string SourceTheater,
    string RequestedTheater,
    SliAuthorityClass AuthorityClass,
    SliTheaterAuthorizationState AuthorizationState,
    string ReasonCode,
    string WitnessedBy,
    DateTimeOffset TimestampUtc);
