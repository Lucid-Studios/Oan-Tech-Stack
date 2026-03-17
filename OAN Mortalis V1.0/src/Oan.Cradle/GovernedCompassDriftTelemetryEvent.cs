using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedCompassDriftTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string DriftHandle { get; init; }
    public required string CMEId { get; init; }
    public required CompassDriftState DriftState { get; init; }
    public required CompassDoctrineBasin BaselineActiveBasin { get; init; }
    public required CompassDoctrineBasin BaselineCompetingBasin { get; init; }
    public required CompassDoctrineBasin LatestActiveBasin { get; init; }
    public required CompassAnchorState LatestAnchorState { get; init; }
    public required int ObservationCount { get; init; }
    public required int WindowSize { get; init; }
    public required int AdvisoryDivergenceCount { get; init; }
    public required int CompetingMigrationCount { get; init; }
    public required IReadOnlyList<string> ObservationHandles { get; init; }
}

internal static class GovernedCompassDriftTelemetry
{
    public static GovernedCompassDriftTelemetryEvent CreateRecordedEvent(
        string driftHandle,
        CompassDriftAssessment assessment,
        GovernanceLoopStage stage,
        string witnessedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driftHandle);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedCompassDriftTelemetryEvent
        {
            EventHash = CreateHash(
                "compass-drift-recorded",
                driftHandle,
                stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "compass-drift-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = stage,
            DriftHandle = driftHandle,
            CMEId = assessment.CMEId,
            DriftState = assessment.DriftState,
            BaselineActiveBasin = assessment.BaselineActiveBasin,
            BaselineCompetingBasin = assessment.BaselineCompetingBasin,
            LatestActiveBasin = assessment.LatestActiveBasin,
            LatestAnchorState = assessment.LatestAnchorState,
            ObservationCount = assessment.ObservationCount,
            WindowSize = assessment.WindowSize,
            AdvisoryDivergenceCount = assessment.AdvisoryDivergenceCount,
            CompetingMigrationCount = assessment.CompetingMigrationCount,
            ObservationHandles = assessment.ObservationHandles.ToArray()
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
