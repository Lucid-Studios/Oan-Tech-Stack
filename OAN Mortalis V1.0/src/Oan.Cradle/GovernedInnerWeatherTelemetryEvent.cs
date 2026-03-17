using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedInnerWeatherTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string InnerWeatherHandle { get; init; }
    public required string CMEId { get; init; }
    public required CompassDoctrineBasin ActiveBasin { get; init; }
    public required CompassDoctrineBasin CompetingBasin { get; init; }
    public required CompassDriftState DriftState { get; init; }
    public required WindowIntegrityState WindowIntegrityState { get; init; }
    public required int ObservationCount { get; init; }
    public required int WindowSize { get; init; }
    public required AttentionResidueState ResidueState { get; init; }
    public required CompassVisibilityClass ResidueVisibilityClass { get; init; }
    public required IReadOnlyList<AttentionResidueContributor> ResidueContributors { get; init; }
    public required ShellCompetitionState ShellCompetitionState { get; init; }
    public required CompassVisibilityClass ShellCompetitionVisibilityClass { get; init; }
    public required HotCoolContactState HotCoolContactState { get; init; }
    public required CompassVisibilityClass HotCoolContactVisibilityClass { get; init; }
    public required IReadOnlyList<StewardAttentionCause> StewardAttentionCauses { get; init; }
    public required string DriftHandle { get; init; }
    public required IReadOnlyList<string> ObservationHandles { get; init; }
}

internal static class GovernedInnerWeatherTelemetry
{
    public static GovernedInnerWeatherTelemetryEvent CreateRecordedEvent(
        string innerWeatherHandle,
        InnerWeatherEvidence evidence,
        GovernanceLoopStage stage,
        string witnessedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(innerWeatherHandle);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedInnerWeatherTelemetryEvent
        {
            EventHash = CreateHash(
                "inner-weather-recorded",
                innerWeatherHandle,
                stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "inner-weather-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = stage,
            InnerWeatherHandle = innerWeatherHandle,
            CMEId = evidence.CMEId,
            ActiveBasin = evidence.ActiveBasin,
            CompetingBasin = evidence.CompetingBasin,
            DriftState = evidence.DriftState,
            WindowIntegrityState = evidence.WindowIntegrityState,
            ObservationCount = evidence.ObservationCount,
            WindowSize = evidence.WindowSize,
            ResidueState = evidence.Residue.ResidueState,
            ResidueVisibilityClass = evidence.Residue.VisibilityClass,
            ResidueContributors = evidence.Residue.Contributors.ToArray(),
            ShellCompetitionState = evidence.ShellCompetition.CompetitionState,
            ShellCompetitionVisibilityClass = evidence.ShellCompetition.VisibilityClass,
            HotCoolContactState = evidence.HotCoolContactState,
            HotCoolContactVisibilityClass = evidence.HotCoolContactVisibilityClass,
            StewardAttentionCauses = evidence.StewardAttentionCauses.ToArray(),
            DriftHandle = evidence.DriftHandle,
            ObservationHandles = evidence.ObservationHandles.ToArray()
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
