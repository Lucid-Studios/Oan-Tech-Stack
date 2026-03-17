using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Oan.Common;

namespace Oan.Cradle;

public sealed class GovernedWorkerReturnTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required string WitnessedBy { get; init; }
    public required GovernanceLoopStage Stage { get; init; }
    public required string ReturnHandle { get; init; }
    public required string CMEId { get; init; }
    public required InternalGoverningCmeOffice RequestingOffice { get; init; }
    public required ConstructClass ConstructClass { get; init; }
    public required string HandoffHandle { get; init; }
    public required string HandoffPacketId { get; init; }
    public required string WorkerPacketId { get; init; }
    public required GovernedWorkerSpecies WorkerSpecies { get; init; }
    public required WorkerCompletionState CompletionState { get; init; }
    public required CompassVisibilityClass DisclosureClass { get; init; }
    public required IReadOnlyList<WorkerReasonCode> ReasonCodes { get; init; }
    public required WorkerResidueDisposition ResidueDisposition { get; init; }
    public required bool Validated { get; init; }
    public required string? ValidationFailureCode { get; init; }
}

internal static class GovernedWorkerReturnTelemetry
{
    public static GovernedWorkerReturnTelemetryEvent CreateRecordedEvent(
        GovernedWorkerReturnReceipt receipt,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new GovernedWorkerReturnTelemetryEvent
        {
            EventHash = CreateHash(
                "worker-return-recorded",
                receipt.ReturnHandle,
                receipt.WorkerSpecies.ToString(),
                receipt.Stage.ToString(),
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "worker-return-recorded",
            WitnessedBy = witnessedBy.Trim(),
            Stage = receipt.Stage,
            ReturnHandle = receipt.ReturnHandle,
            CMEId = receipt.CMEId,
            RequestingOffice = receipt.RequestingOffice,
            ConstructClass = receipt.ConstructClass,
            HandoffHandle = receipt.HandoffHandle,
            HandoffPacketId = receipt.HandoffPacketId,
            WorkerPacketId = receipt.WorkerPacketId,
            WorkerSpecies = receipt.WorkerSpecies,
            CompletionState = receipt.CompletionState,
            DisclosureClass = receipt.DisclosureClass,
            ReasonCodes = receipt.ReasonCodes.ToArray(),
            ResidueDisposition = receipt.ResidueDisposition,
            Validated = receipt.Validated,
            ValidationFailureCode = receipt.ValidationFailureCode
        };
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
