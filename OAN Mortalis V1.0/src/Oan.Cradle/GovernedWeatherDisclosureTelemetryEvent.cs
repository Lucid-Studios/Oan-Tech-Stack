using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWeatherDisclosureTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string DisclosureHandle { get; init; }
    public required string CMEId { get; init; }
    public required StewardCareRoutingState RoutingState { get; init; }
    public required CheckInCadenceState CadenceState { get; init; }
    public required EvidenceSufficiencyState EvidenceSufficiencyState { get; init; }
    public required WindowIntegrityState WindowIntegrityState { get; init; }
    public required WeatherDisclosureScope DisclosureScope { get; init; }
    public required CommunityWeatherStatus CommunityStatus { get; init; }
    public required CommunityStewardAttentionState CommunityStewardAttention { get; init; }
    public required CompassDriftState AnchorState { get; init; }
    public required IReadOnlyList<CommunityWeatherField> AllowedCommunityFields { get; init; }
    public required IReadOnlyList<StewardAttentionCause> StewardReasonCodes { get; init; }
    public required IReadOnlyList<WeatherWithheldMarker> WithheldMarkers { get; init; }
    public required WeatherDisclosureRationaleCode RationaleCode { get; init; }
    public required string InnerWeatherHandle { get; init; }
}

internal static class GovernedWeatherDisclosureTelemetry
{
    public static GovernedWeatherDisclosureTelemetryEvent CreateRecordedEvent(
        string disclosureHandle,
        StewardCareAssessment assessment,
        WeatherDisclosureDecision decision,
        GovernanceLoopStage stage,
        string witnessedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(disclosureHandle);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedWeatherDisclosureTelemetryEvent
        {
            EventHash = CreateHash(
                "weather-disclosure-recorded",
                disclosureHandle,
                stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "weather-disclosure-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = stage,
            DisclosureHandle = disclosureHandle,
            CMEId = assessment.CMEId,
            RoutingState = assessment.RoutingState,
            CadenceState = assessment.CadenceState,
            EvidenceSufficiencyState = assessment.EvidenceSufficiencyState,
            WindowIntegrityState = assessment.WindowIntegrityState,
            DisclosureScope = decision.DisclosureScope,
            CommunityStatus = decision.CommunityWeatherPacket.Status,
            CommunityStewardAttention = decision.CommunityWeatherPacket.StewardAttention,
            AnchorState = decision.CommunityWeatherPacket.AnchorState,
            AllowedCommunityFields = decision.AllowedCommunityFields.ToArray(),
            StewardReasonCodes = decision.StewardReasonCodes.ToArray(),
            WithheldMarkers = decision.WithheldMarkers.ToArray(),
            RationaleCode = decision.RationaleCode,
            InnerWeatherHandle = decision.InnerWeatherHandle
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
