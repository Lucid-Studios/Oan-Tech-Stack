using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public enum CompassDoctrineBasin
{
    Unknown = 0,
    BoundedLocalityContinuity = 1,
    FluidContinuityLaw = 2,
    IdentityContinuity = 3,
    GeneralContinuityDiscourse = 4
}

public enum CompassAnchorState
{
    Unknown = 0,
    Held = 1,
    Weakened = 2,
    Lost = 3
}

public enum CompassDriftState
{
    Held = 0,
    Weakened = 1,
    Lost = 2
}

public enum AttentionResidueState
{
    None = 0,
    Low = 1,
    Present = 2,
    Persistent = 3,
    Escalating = 4
}

public enum CompassVisibilityClass
{
    CommunityLegible = 0,
    OperatorGuarded = 1,
    CrypticOnly = 2
}

public enum ShellCompetitionState
{
    Unknown = 0,
    Absent = 1,
    Present = 2,
    Rising = 3
}

public enum HotCoolContactState
{
    Unknown = 0,
    InContact = 1,
    Cool = 2,
    MissedCheckIn = 3
}

public enum WindowIntegrityState
{
    Intact = 0,
    Sparse = 1,
    JournalGap = 2,
    RuntimeRestart = 3,
    CmeReselected = 4,
    VisibilityDowngraded = 5,
    GovernanceReset = 6
}

public enum EvidenceSufficiencyState
{
    Sufficient = 0,
    Sparse = 1,
    BrokenWindow = 2,
    ContinuityAmbiguous = 3
}

public enum AttentionResidueContributor
{
    None = 0,
    AdvisoryDivergence = 1,
    CompetingPressure = 2,
    DriftInstability = 3,
    WindowIntegrityBreak = 4,
    ContactCadenceMissed = 5
}

public enum StewardAttentionCause
{
    None = 0,
    DriftWeakening = 1,
    DriftLoss = 2,
    ResiduePersistence = 3,
    ShellCompetition = 4,
    MissedCheckIn = 5,
    WindowIntegrityBreak = 6
}

public enum CommunityWeatherStatus
{
    Unknown = 0,
    Stable = 1,
    Unstable = 2,
    Degraded = 3,
    MissedCheckIn = 4
}

public enum CommunityStewardAttentionState
{
    None = 0,
    Recommended = 1,
    Needed = 2
}

public enum StewardCareRoutingState
{
    None = 0,
    CheckInRecommended = 1,
    CheckInNeeded = 2,
    EscalationEligible = 3
}

public enum CheckInCadenceState
{
    Current = 0,
    DueSoon = 1,
    Overdue = 2,
    Broken = 3,
    Unknown = 4
}

public enum WeatherDisclosureScope
{
    Community = 0,
    Steward = 1,
    OperatorGuarded = 2
}

public enum CommunityWeatherField
{
    Status = 0,
    StewardAttention = 1,
    AnchorState = 2,
    VisibilityClass = 3,
    TimestampUtc = 4
}

public enum WeatherWithheldMarker
{
    GuardedEvidence = 0,
    CrypticEvidence = 1,
    SparseEvidence = 2,
    BrokenWindow = 3,
    ContinuityAmbiguous = 4
}

public enum WeatherDisclosureRationaleCode
{
    CommunityWhitelisted = 0,
    GuardedReduction = 1,
    SparseReduction = 2,
    BrokenWindowReduction = 3,
    ContinuityAmbiguousReduction = 4,
    OperatorGuardedReduction = 5
}

public enum CompassObservationProvenance
{
    LispNative = 0,
    SeedAssisted = 1,
    Braided = 2
}

public enum CompassSeedAdvisoryDisposition
{
    None = 0,
    Accepted = 1,
    Deferred = 2,
    Rejected = 3
}

public enum CompassSelfTouchClass
{
    NoTouch = 0,
    ValidationTouch = 1,
    HotClaimTouch = 2,
    BoundaryContact = 3
}

public enum CompassOeCoePosture
{
    Unresolved = 0,
    OeDominant = 1,
    CoeDominant = 2,
    ShuntedBalanced = 3
}

public sealed record CompassSeedAdvisoryObservation(
    bool Accepted,
    string Decision,
    string Trace,
    double Confidence,
    string? Payload,
    CompassDoctrineBasin? SuggestedActiveBasin,
    CompassDoctrineBasin? SuggestedCompetingBasin,
    CompassAnchorState? SuggestedAnchorState,
    CompassSelfTouchClass? SuggestedSelfTouchClass,
    CompassSeedAdvisoryDisposition Disposition,
    string? DispositionReason,
    string? Justification);

public sealed record CompassObservationSurface(
    string ObservationHandle,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassOeCoePosture OeCoePosture,
    CompassSelfTouchClass SelfTouchClass,
    CompassAnchorState AnchorState,
    CompassObservationProvenance Provenance,
    string ObserverIdentity,
    string WorkingStateHandle,
    string CSelfGelHandle,
    string SelfGelHandle,
    string? ValidationReferenceHandle,
    string Objective,
    CompassSeedAdvisoryObservation? SeedAdvisory,
    DateTimeOffset TimestampUtc);

public sealed record CompassDriftAssessment(
    string CMEId,
    int WindowSize,
    int ObservationCount,
    CompassDoctrineBasin BaselineActiveBasin,
    CompassDoctrineBasin BaselineCompetingBasin,
    CompassDoctrineBasin LatestActiveBasin,
    CompassAnchorState LatestAnchorState,
    CompassDriftState DriftState,
    int AdvisoryDivergenceCount,
    int CompetingMigrationCount,
    IReadOnlyList<string> ObservationHandles,
    DateTimeOffset TimestampUtc);

public sealed record CompassResidueAssessment(
    AttentionResidueState ResidueState,
    CompassVisibilityClass VisibilityClass,
    IReadOnlyList<AttentionResidueContributor> Contributors);

public sealed record ShellCompetitionAssessment(
    ShellCompetitionState CompetitionState,
    CompassVisibilityClass VisibilityClass);

public sealed record InnerWeatherEvidence(
    string CMEId,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassDriftState DriftState,
    int WindowSize,
    int ObservationCount,
    WindowIntegrityState WindowIntegrityState,
    CompassResidueAssessment Residue,
    ShellCompetitionAssessment ShellCompetition,
    HotCoolContactState HotCoolContactState,
    CompassVisibilityClass HotCoolContactVisibilityClass,
    IReadOnlyList<StewardAttentionCause> StewardAttentionCauses,
    string DriftHandle,
    IReadOnlyList<string> ObservationHandles,
    DateTimeOffset TimestampUtc);

public sealed record StewardCareAssessment(
    string CMEId,
    StewardCareRoutingState RoutingState,
    CheckInCadenceState CadenceState,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    WindowIntegrityState WindowIntegrityState,
    CommunityWeatherPacket CommunityWeatherPacket,
    bool HasGuardedInfluence,
    bool HasCrypticInfluence,
    IReadOnlyList<StewardAttentionCause> ReasonCodes,
    string InnerWeatherHandle,
    DateTimeOffset TimestampUtc);

public sealed record WeatherDisclosureDecision(
    string CMEId,
    WeatherDisclosureScope DisclosureScope,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    CommunityWeatherPacket CommunityWeatherPacket,
    IReadOnlyList<CommunityWeatherField> AllowedCommunityFields,
    IReadOnlyList<StewardAttentionCause> StewardReasonCodes,
    IReadOnlyList<WeatherWithheldMarker> WithheldMarkers,
    WeatherDisclosureRationaleCode RationaleCode,
    string InnerWeatherHandle,
    DateTimeOffset TimestampUtc);

public sealed record GovernedCompassObservationReceipt(
    string WitnessHandle,
    GovernanceLoopStage Stage,
    string ObservationHandle,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassOeCoePosture OeCoePosture,
    CompassSelfTouchClass SelfTouchClass,
    CompassAnchorState AnchorState,
    CompassObservationProvenance Provenance,
    string WitnessedBy,
    string WorkingStateHandle,
    string CSelfGelHandle,
    string SelfGelHandle,
    string? ValidationReferenceHandle,
    string Objective,
    bool? AdvisoryAccepted,
    string? AdvisoryDecision,
    string? AdvisoryTrace,
    double? AdvisoryConfidence,
    CompassDoctrineBasin? AdvisorySuggestedActiveBasin,
    CompassDoctrineBasin? AdvisorySuggestedCompetingBasin,
    CompassAnchorState? AdvisorySuggestedAnchorState,
    CompassSelfTouchClass? AdvisorySuggestedSelfTouchClass,
    CompassSeedAdvisoryDisposition? AdvisoryDisposition,
    string? AdvisoryDispositionReason,
    string? AdvisoryJustification,
    DateTimeOffset TimestampUtc);

public sealed record GovernedCompassDriftReceipt(
    string DriftHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    CompassDriftState DriftState,
    CompassDoctrineBasin BaselineActiveBasin,
    CompassDoctrineBasin BaselineCompetingBasin,
    CompassDoctrineBasin LatestActiveBasin,
    int ObservationCount,
    int WindowSize,
    int AdvisoryDivergenceCount,
    int CompetingMigrationCount,
    string WitnessedBy,
    IReadOnlyList<string> ObservationHandles,
    DateTimeOffset TimestampUtc);

public sealed record GovernedInnerWeatherReceipt(
    string InnerWeatherHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    CompassDoctrineBasin ActiveBasin,
    CompassDoctrineBasin CompetingBasin,
    CompassDriftState DriftState,
    WindowIntegrityState WindowIntegrityState,
    int ObservationCount,
    int WindowSize,
    AttentionResidueState ResidueState,
    CompassVisibilityClass ResidueVisibilityClass,
    IReadOnlyList<AttentionResidueContributor> ResidueContributors,
    ShellCompetitionState ShellCompetitionState,
    CompassVisibilityClass ShellCompetitionVisibilityClass,
    HotCoolContactState HotCoolContactState,
    CompassVisibilityClass HotCoolContactVisibilityClass,
    IReadOnlyList<StewardAttentionCause> StewardAttentionCauses,
    string WitnessedBy,
    string DriftHandle,
    IReadOnlyList<string> ObservationHandles,
    DateTimeOffset TimestampUtc);

public sealed record CommunityWeatherPacket(
    CommunityWeatherStatus Status,
    CommunityStewardAttentionState StewardAttention,
    CompassDriftState AnchorState,
    CompassVisibilityClass VisibilityClass,
    DateTimeOffset TimestampUtc);

public sealed record GovernedWeatherDisclosureReceipt(
    string DisclosureHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    StewardCareRoutingState RoutingState,
    CheckInCadenceState CadenceState,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    WindowIntegrityState WindowIntegrityState,
    WeatherDisclosureScope DisclosureScope,
    CommunityWeatherPacket CommunityWeatherPacket,
    IReadOnlyList<CommunityWeatherField> AllowedCommunityFields,
    IReadOnlyList<StewardAttentionCause> StewardReasonCodes,
    IReadOnlyList<WeatherWithheldMarker> WithheldMarkers,
    WeatherDisclosureRationaleCode RationaleCode,
    string WitnessedBy,
    string InnerWeatherHandle,
    DateTimeOffset TimestampUtc);

public static class CompassObservationKeys
{
    public static string CreateObservationHandle(
        string workingStateHandle,
        string cSelfGelHandle,
        CompassDoctrineBasin activeBasin,
        string objective)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingStateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(cSelfGelHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);

        return $"compass-observation://{ComputeDigest(workingStateHandle, cSelfGelHandle, activeBasin.ToString(), objective)}";
    }

    public static string CreateWitnessHandle(
        string loopKey,
        string observationHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(observationHandle);

        return $"compass-witness://{ComputeDigest(loopKey, observationHandle)}";
    }

    public static string CreateDriftHandle(
        string loopKey,
        string cmeId,
        CompassDriftState driftState,
        IEnumerable<string> observationHandles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentNullException.ThrowIfNull(observationHandles);

        var orderedObservationHandles = observationHandles
            .Where(handle => !string.IsNullOrWhiteSpace(handle))
            .Select(handle => handle.Trim())
            .ToArray();
        if (orderedObservationHandles.Length == 0)
        {
            throw new ArgumentException("At least one observation handle is required.", nameof(observationHandles));
        }

        return $"compass-drift://{ComputeDigest(loopKey, cmeId, driftState.ToString(), string.Join("|", orderedObservationHandles))}";
    }

    public static string CreateInnerWeatherHandle(
        string loopKey,
        string cmeId,
        WindowIntegrityState windowIntegrityState,
        CompassDriftState driftState,
        IEnumerable<string> observationHandles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentNullException.ThrowIfNull(observationHandles);

        var orderedObservationHandles = observationHandles
            .Where(handle => !string.IsNullOrWhiteSpace(handle))
            .Select(handle => handle.Trim())
            .ToArray();
        if (orderedObservationHandles.Length == 0)
        {
            throw new ArgumentException("At least one observation handle is required.", nameof(observationHandles));
        }

        return $"inner-weather://{ComputeDigest(loopKey, cmeId, windowIntegrityState.ToString(), driftState.ToString(), string.Join("|", orderedObservationHandles))}";
    }

    public static string CreateWeatherDisclosureHandle(
        string loopKey,
        string cmeId,
        StewardCareRoutingState routingState,
        WeatherDisclosureScope disclosureScope,
        string innerWeatherHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(innerWeatherHandle);

        return $"weather-disclosure://{ComputeDigest(loopKey, cmeId, routingState.ToString(), disclosureScope.ToString(), innerWeatherHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
