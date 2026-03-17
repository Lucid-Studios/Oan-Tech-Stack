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

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
