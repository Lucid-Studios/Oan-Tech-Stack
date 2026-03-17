using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWorkerHandoffTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string HandoffHandle { get; init; }
    public required string CMEId { get; init; }
    public required InternalGoverningCmeOffice RequestingOffice { get; init; }
    public required string RequestingOfficeInstanceId { get; init; }
    public required ConstructClass ConstructClass { get; init; }
    public required GovernedWorkerSpecies WorkerSpecies { get; init; }
    public required WorkerInstanceMode WorkerInstanceMode { get; init; }
    public required OfficeActionEligibility ActionCeiling { get; init; }
    public required CompassVisibilityClass DisclosureClass { get; init; }
    public required EvidenceSufficiencyState EvidenceSufficiencyState { get; init; }
    public required MaturityPosture MaturityPosture { get; init; }
    public required string HandoffPacketId { get; init; }
    public required string ReturnPacketSchema { get; init; }
    public required string ReturnDestination { get; init; }
    public required string OfficeIssuanceHandle { get; init; }
    public required string OfficeAuthorityHandle { get; init; }
    public required string WeatherDisclosureHandle { get; init; }
}

internal static class GovernedWorkerHandoffTelemetry
{
    public static GovernedWorkerHandoffTelemetryEvent CreateRecordedEvent(
        WorkerHandoffPacket packet,
        GovernedWorkerHandoffReceipt receipt,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedWorkerHandoffTelemetryEvent
        {
            EventHash = CreateHash(
                "worker-handoff-recorded",
                receipt.HandoffHandle,
                receipt.WorkerSpecies.ToString(),
                receipt.Stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "worker-handoff-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = receipt.Stage,
            HandoffHandle = receipt.HandoffHandle,
            CMEId = receipt.CMEId,
            RequestingOffice = receipt.RequestingOffice,
            RequestingOfficeInstanceId = receipt.RequestingOfficeInstanceId,
            ConstructClass = receipt.ConstructClass,
            WorkerSpecies = receipt.WorkerSpecies,
            WorkerInstanceMode = receipt.WorkerInstanceMode,
            ActionCeiling = receipt.ActionCeiling,
            DisclosureClass = receipt.DisclosureClass,
            EvidenceSufficiencyState = receipt.EvidenceSufficiencyState,
            MaturityPosture = receipt.MaturityPosture,
            HandoffPacketId = receipt.HandoffPacketId,
            ReturnPacketSchema = packet.ReturnPacketSchema,
            ReturnDestination = packet.ReturnDestination,
            OfficeIssuanceHandle = receipt.OfficeIssuanceHandle,
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
