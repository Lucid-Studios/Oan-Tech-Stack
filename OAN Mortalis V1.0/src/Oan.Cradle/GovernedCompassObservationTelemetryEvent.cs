using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedCompassObservationTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string ObservationHandle { get; init; }
    public required CompassDoctrineBasin ActiveBasin { get; init; }
    public required CompassDoctrineBasin CompetingBasin { get; init; }
    public required CompassOeCoePosture OeCoePosture { get; init; }
    public required CompassSelfTouchClass SelfTouchClass { get; init; }
    public required CompassAnchorState AnchorState { get; init; }
    public required CompassObservationProvenance Provenance { get; init; }
    public required string WorkingStateHandle { get; init; }
    public required string CSelfGelHandle { get; init; }
    public required string SelfGelHandle { get; init; }
    public string? ValidationReferenceHandle { get; init; }
    public required string Objective { get; init; }
    public bool? AdvisoryAccepted { get; init; }
    public string? AdvisoryDecision { get; init; }
    public string? AdvisoryTrace { get; init; }
    public double? AdvisoryConfidence { get; init; }
}

internal static class GovernedCompassObservationTelemetry
{
    public static GovernedCompassObservationTelemetryEvent CreateRecordedEvent(
        CompassObservationSurface observation,
        GovernanceLoopStage stage,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(observation);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedCompassObservationTelemetryEvent
        {
            EventHash = CreateHash(
                "compass-observation-recorded",
                observation.ObservationHandle,
                stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "compass-observation-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = stage,
            ObservationHandle = observation.ObservationHandle,
            ActiveBasin = observation.ActiveBasin,
            CompetingBasin = observation.CompetingBasin,
            OeCoePosture = observation.OeCoePosture,
            SelfTouchClass = observation.SelfTouchClass,
            AnchorState = observation.AnchorState,
            Provenance = observation.Provenance,
            WorkingStateHandle = observation.WorkingStateHandle,
            CSelfGelHandle = observation.CSelfGelHandle,
            SelfGelHandle = observation.SelfGelHandle,
            ValidationReferenceHandle = observation.ValidationReferenceHandle,
            Objective = observation.Objective,
            AdvisoryAccepted = observation.SeedAdvisory?.Accepted,
            AdvisoryDecision = observation.SeedAdvisory?.Decision,
            AdvisoryTrace = observation.SeedAdvisory?.Trace,
            AdvisoryConfidence = observation.SeedAdvisory?.Confidence
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
