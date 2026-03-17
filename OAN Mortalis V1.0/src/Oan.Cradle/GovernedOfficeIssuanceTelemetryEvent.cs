using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedOfficeIssuanceTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string IssuanceHandle { get; init; }
    public required string CMEId { get; init; }
    public required InternalGoverningCmeOffice Office { get; init; }
    public required ConstructClass ConstructClass { get; init; }
    public required string PackageId { get; init; }
    public required string IssuanceLineageId { get; init; }
    public required string OfficeInstanceId { get; init; }
    public required string ChassisClass { get; init; }
    public required string TargetRuntimeSurface { get; init; }
    public required string AuthorityScope { get; init; }
    public required OfficeActionEligibility AllowedActionCeiling { get; init; }
    public required CompassVisibilityClass DisclosureCeiling { get; init; }
    public required MaturityPosture MaturityPosture { get; init; }
    public required string OfficeAuthorityHandle { get; init; }
    public required string WeatherDisclosureHandle { get; init; }
}

internal static class GovernedOfficeIssuanceTelemetry
{
    public static GovernedOfficeIssuanceTelemetryEvent CreateRecordedEvent(
        IssuedOfficePackage package,
        GovernedOfficeIssuanceReceipt receipt,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedOfficeIssuanceTelemetryEvent
        {
            EventHash = CreateHash(
                "office-issuance-recorded",
                receipt.IssuanceHandle,
                receipt.Office.ToString(),
                receipt.Stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "office-issuance-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = receipt.Stage,
            IssuanceHandle = receipt.IssuanceHandle,
            CMEId = receipt.CMEId,
            Office = receipt.Office,
            ConstructClass = receipt.ConstructClass,
            PackageId = receipt.PackageId,
            IssuanceLineageId = receipt.IssuanceLineageId,
            OfficeInstanceId = receipt.OfficeInstanceId,
            ChassisClass = package.ChassisClass,
            TargetRuntimeSurface = package.TargetRuntimeSurface,
            AuthorityScope = package.AuthorityScope,
            AllowedActionCeiling = receipt.AllowedActionCeiling,
            DisclosureCeiling = receipt.DisclosureCeiling,
            MaturityPosture = receipt.MaturityPosture,
            OfficeAuthorityHandle = receipt.OfficeAuthorityHandle,
            WeatherDisclosureHandle = receipt.WeatherDisclosureHandle
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
