using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedOfficeAuthorityTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string AuthorityHandle { get; init; }
    public required string CMEId { get; init; }
    public required InternalGoverningCmeOffice Office { get; init; }
    public required OfficeAuthoritySurface AuthoritySurface { get; init; }
    public required OfficeViewEligibility ViewEligibility { get; init; }
    public required OfficeAcknowledgmentEligibility AcknowledgmentEligibility { get; init; }
    public required OfficeActionEligibility ActionEligibility { get; init; }
    public required EvidenceSufficiencyState EvidenceSufficiencyState { get; init; }
    public required WeatherDisclosureScope DisclosureScope { get; init; }
    public required IReadOnlyList<StewardAttentionCause> AllowedReasonCodes { get; init; }
    public required IReadOnlyList<OfficeAuthorityWithheldMarker> WithheldMarkers { get; init; }
    public required IReadOnlyList<OfficeAuthorityProhibition> Prohibitions { get; init; }
    public required OfficeAuthorityRationaleCode RationaleCode { get; init; }
    public required string WeatherDisclosureHandle { get; init; }
}

internal static class GovernedOfficeAuthorityTelemetry
{
    public static GovernedOfficeAuthorityTelemetryEvent CreateRecordedEvent(
        string authorityHandle,
        GoverningOfficeAuthorityAssessment assessment,
        GoverningOfficeAuthorityView view,
        GovernanceLoopStage stage,
        string witnessedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorityHandle);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedOfficeAuthorityTelemetryEvent
        {
            EventHash = CreateHash(
                "office-authority-recorded",
                authorityHandle,
                assessment.Office.ToString(),
                stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "office-authority-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = stage,
            AuthorityHandle = authorityHandle,
            CMEId = assessment.CMEId,
            Office = assessment.Office,
            AuthoritySurface = assessment.AuthoritySurface,
            ViewEligibility = assessment.ViewEligibility,
            AcknowledgmentEligibility = assessment.AcknowledgmentEligibility,
            ActionEligibility = assessment.ActionEligibility,
            EvidenceSufficiencyState = assessment.EvidenceSufficiencyState,
            DisclosureScope = assessment.DisclosureScope,
            AllowedReasonCodes = view.AllowedReasonCodes.ToArray(),
            WithheldMarkers = view.WithheldMarkers.ToArray(),
            Prohibitions = view.Prohibitions.ToArray(),
            RationaleCode = view.RationaleCode,
            WeatherDisclosureHandle = assessment.WeatherDisclosureHandle
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
