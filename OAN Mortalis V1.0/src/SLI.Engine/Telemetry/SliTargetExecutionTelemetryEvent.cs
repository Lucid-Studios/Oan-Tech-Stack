using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using SLI.Engine.Runtime;

namespace SLI.Engine.Telemetry;

public sealed class SliTargetExecutionTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required bool Accepted { get; init; }
    public required string WitnessedBy { get; init; }
    public required string LaneId { get; init; }
    public required string RuntimeId { get; init; }
    public required string ProfileId { get; init; }
    public required string BudgetClass { get; init; }
    public required string CommitAuthorityClass { get; init; }
    public required string Objective { get; init; }
    public required string ProgramId { get; init; }
    public required string AdmissionHandle { get; init; }
    public string? LineageHandle { get; init; }
    public string? TraceHandle { get; init; }
    public string? ResidueHandle { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public required IReadOnlyList<string> ReasonFamilies { get; init; }
    public required SliTargetExecutionBudgetUsage BudgetUsage { get; init; }
    public int? EmittedTraceCount { get; init; }
    public int? EmittedResidueCount { get; init; }
}

internal static class SliTargetExecutionTelemetry
{
    public static SliTargetExecutionTelemetryEvent CreateAdmissionEvent(
        SliTargetExecutionAdmission admission,
        string objective,
        string programId,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        var eventType = admission.Accepted
            ? "sli-target-admission-accepted"
            : "sli-target-admission-refused";
        var reasons = admission.Reasons.ToArray();

        return new SliTargetExecutionTelemetryEvent
        {
            EventHash = CreateHash(
                eventType,
                admission.AdmissionHandle,
                admission.RuntimeId,
                admission.ProfileId,
                objective,
                programId,
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            Accepted = admission.Accepted,
            WitnessedBy = witnessedBy.Trim(),
            LaneId = admission.LaneId,
            RuntimeId = admission.RuntimeId,
            ProfileId = admission.ProfileId,
            BudgetClass = admission.BudgetClass,
            CommitAuthorityClass = admission.CommitAuthorityClass,
            Objective = objective.Trim(),
            ProgramId = programId.Trim(),
            AdmissionHandle = admission.AdmissionHandle,
            Reasons = reasons,
            ReasonFamilies = ClassifyReasonFamilies(reasons),
            BudgetUsage = admission.BudgetUsage
        };
    }

    public static SliTargetExecutionTelemetryEvent CreateLineageEvent(
        SliTargetExecutionAdmission admission,
        SliTargetExecutionLineage lineage,
        string witnessedBy)
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentNullException.ThrowIfNull(lineage);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new SliTargetExecutionTelemetryEvent
        {
            EventHash = CreateHash(
                "sli-target-lineage-recorded",
                admission.AdmissionHandle,
                lineage.LineageHandle,
                lineage.TraceHandle,
                lineage.ResidueHandle,
                witnessedBy),
            Timestamp = DateTime.UtcNow,
            EventType = "sli-target-lineage-recorded",
            Accepted = true,
            WitnessedBy = witnessedBy.Trim(),
            LaneId = admission.LaneId,
            RuntimeId = admission.RuntimeId,
            ProfileId = admission.ProfileId,
            BudgetClass = admission.BudgetClass,
            CommitAuthorityClass = admission.CommitAuthorityClass,
            Objective = lineage.Objective,
            ProgramId = lineage.ProgramId,
            AdmissionHandle = admission.AdmissionHandle,
            LineageHandle = lineage.LineageHandle,
            TraceHandle = lineage.TraceHandle,
            ResidueHandle = lineage.ResidueHandle,
            Reasons = Array.Empty<string>(),
            ReasonFamilies = Array.Empty<string>(),
            BudgetUsage = admission.BudgetUsage,
            EmittedTraceCount = lineage.EmittedTraceCount,
            EmittedResidueCount = lineage.EmittedResidueCount
        };
    }

    private static IReadOnlyList<string> ClassifyReasonFamilies(IEnumerable<string> reasons)
    {
        return reasons
            .Select(ClassifyReasonFamily)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static string ClassifyReasonFamily(string reason)
    {
        if (reason.StartsWith("missing-capability:", StringComparison.Ordinal))
        {
            return "missing-capability";
        }

        if (reason.StartsWith("disallowed-operation:", StringComparison.Ordinal))
        {
            return "disallowed-operation";
        }

        if (reason.StartsWith("profile-violation:", StringComparison.Ordinal))
        {
            return "profile-violation";
        }

        return "other";
    }

    private static string CreateHash(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
