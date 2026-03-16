using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public enum GovernedTargetWitnessKind
{
    AdmissionAccepted = 0,
    AdmissionRefused = 1,
    LineageRecorded = 2
}

public sealed record GovernedTargetExecutionBudgetUsage(
    int InstructionCount,
    int SymbolicDepth,
    int ProjectedTraceEntryCount,
    int ProjectedResidueCount,
    int WitnessOperationCount,
    int TransportOperationCount);

public sealed record GovernedTargetWitnessReceipt(
    string WitnessHandle,
    GovernanceLoopStage Stage,
    GovernedTargetWitnessKind Kind,
    bool Accepted,
    string WitnessedBy,
    string LaneId,
    string RuntimeId,
    string ProfileId,
    string BudgetClass,
    string CommitAuthorityClass,
    string Objective,
    string ProgramId,
    string AdmissionHandle,
    string? LineageHandle,
    string? TraceHandle,
    string? ResidueHandle,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> ReasonFamilies,
    GovernedTargetExecutionBudgetUsage BudgetUsage,
    int? EmittedTraceCount,
    int? EmittedResidueCount,
    DateTimeOffset TimestampUtc);

public static class GovernedTargetWitnessKeys
{
    public static string CreateWitnessHandle(
        string loopKey,
        GovernedTargetWitnessKind kind,
        string eventHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventHash);
        return $"target-witness://{GetKindSlug(kind)}/{ComputeDigest(loopKey, kind.ToString(), eventHash)}";
    }

    public static string GetKindSlug(GovernedTargetWitnessKind kind) => kind switch
    {
        GovernedTargetWitnessKind.AdmissionAccepted => "admission-accepted",
        GovernedTargetWitnessKind.AdmissionRefused => "admission-refused",
        GovernedTargetWitnessKind.LineageRecorded => "lineage-recorded",
        _ => "unknown"
    };

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
