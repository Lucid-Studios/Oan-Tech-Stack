namespace San.Common;

using San.Common;

public enum CompassDriftState
{
    Held = 0,
    Weakened = 1,
    Lost = 2
}

public enum CompassVisibilityClass
{
    CommunityLegible = 0,
    OperatorGuarded = 1,
    CrypticOnly = 2
}

public enum EvidenceSufficiencyState
{
    Sufficient = 0,
    Sparse = 1,
    BrokenWindow = 2,
    ContinuityAmbiguous = 3
}

public enum WindowIntegrityState
{
    Intact = 0,
    Sparse = 1,
    JournalGap = 2,
    RuntimeRestart = 3
}

public enum CommunityWeatherStatus
{
    Unknown = 0,
    Stable = 1,
    Unstable = 2,
    Degraded = 3
}

public enum CommunityStewardAttentionState
{
    None = 0,
    Recommended = 1,
    Needed = 2
}

public enum WeatherDisclosureScope
{
    Community = 0,
    Steward = 1,
    OperatorGuarded = 2
}

public enum StewardAttentionCause
{
    None = 0,
    DriftWeakening = 1,
    DriftLoss = 2,
    ResiduePersistence = 3,
    WindowIntegrityBreak = 4
}

public enum WeatherWithheldMarker
{
    GuardedEvidence = 0,
    CrypticEvidence = 1,
    SparseEvidence = 2,
    BrokenWindow = 3,
    ContinuityAmbiguous = 4
}

public enum OfficeAuthoritySurface
{
    StewardSurface = 0,
    CrypticBoundarySurface = 1,
    PrimeCareSurface = 2,
    GuardedReviewSurface = 3
}

public enum OfficeViewEligibility
{
    Withheld = 0,
    CommunityReduced = 1,
    GuardedView = 2,
    OfficeSpecificView = 3
}

public enum OfficeAcknowledgmentEligibility
{
    NotAllowed = 0,
    Allowed = 1
}

public enum OfficeActionEligibility
{
    ViewOnly = 0,
    AcknowledgeAllowed = 1,
    CheckInAllowed = 2,
    EscalationReviewAllowed = 3,
    HandoffEligible = 4
}

public enum OfficeAuthorityProhibition
{
    MayNotOriginateTruth = 0,
    MayNotWidenDisclosure = 1,
    MayNotBypassBond = 2,
    MayNotAuthorPublicDisclosure = 3
}

public sealed record CommunityWeatherPacket(
    CommunityWeatherStatus Status,
    CommunityStewardAttentionState StewardAttention,
    CompassDriftState AnchorState,
    CompassVisibilityClass VisibilityClass,
    DateTimeOffset TimestampUtc,
    ListeningFrameProjectionPacket? ListeningFrameProjectionPacket = null,
    CompassProjectionPacket? CompassProjectionPacket = null,
    ListeningFrameInstrumentationReceipt? ListeningFrameInstrumentationReceipt = null,
    ZedDeltaSelfBasisReceipt? ZedDeltaSelfBasisReceipt = null,
    ThetaIngressSensoryClusterReceipt? ThetaIngressSensoryClusterReceipt = null,
    PostIngressDiscernmentReceipt? PostIngressDiscernmentReceipt = null);

public sealed record GoverningOfficeAuthorityAssessment(
    string CMEId,
    InternalGoverningCmeOffice Office,
    OfficeAuthoritySurface AuthoritySurface,
    OfficeViewEligibility ViewEligibility,
    OfficeAcknowledgmentEligibility AcknowledgmentEligibility,
    OfficeActionEligibility ActionEligibility,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    WindowIntegrityState WindowIntegrityState,
    WeatherDisclosureScope DisclosureScope,
    bool OfficeAttached,
    bool BondedConfirmed,
    bool GuardedReviewConfirmed,
    CommunityWeatherPacket CommunityWeatherPacket,
    IReadOnlyList<StewardAttentionCause> SourceReasonCodes,
    IReadOnlyList<WeatherWithheldMarker> SourceWithheldMarkers,
    IReadOnlyList<OfficeAuthorityProhibition> Prohibitions,
    string WeatherDisclosureHandle,
    DateTimeOffset TimestampUtc);
