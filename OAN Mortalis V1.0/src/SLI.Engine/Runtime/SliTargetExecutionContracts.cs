using SLI.Engine.Cognition;
using System.Security.Cryptography;
using System.Text;

namespace SLI.Engine.Runtime;

internal sealed record SliTargetHigherOrderLocalityExecutionRequest(
    string Objective,
    SliCoreProgram Program,
    SliTargetLaneEligibility Eligibility,
    SliTargetExecutionAdmission Admission);

public sealed record SliTargetExecutionBudgetUsage(
    int InstructionCount,
    int SymbolicDepth,
    int ProjectedTraceEntryCount,
    int ProjectedResidueCount,
    int WitnessOperationCount,
    int TransportOperationCount);

public sealed record SliTargetExecutionAdmission(
    string AdmissionHandle,
    string LaneId,
    string RuntimeId,
    string ProfileId,
    string BudgetClass,
    string CommitAuthorityClass,
    bool Accepted,
    IReadOnlyList<string> Reasons,
    SliTargetExecutionBudgetUsage BudgetUsage,
    DateTimeOffset TimestampUtc);

public sealed record SliTargetExecutionLineage(
    string LineageHandle,
    string AdmissionHandle,
    string LaneId,
    string RuntimeId,
    string ProfileId,
    string BudgetClass,
    string CommitAuthorityClass,
    string Objective,
    string ProgramId,
    string TraceHandle,
    string ResidueHandle,
    int EmittedTraceCount,
    int EmittedResidueCount,
    DateTimeOffset TimestampUtc);

internal interface ISliTargetHigherOrderLocalityExecutor
{
    SliRuntimeCapabilityManifest CapabilityManifest { get; }

    Task<SliHigherOrderLocalityResult> ExecuteAsync(
        SliTargetHigherOrderLocalityExecutionRequest request,
        CancellationToken cancellationToken = default);
}

internal static class SliTargetExecutionContracts
{
    public static SliTargetExecutionAdmission CreateAdmission(
        SliTargetLaneEligibility eligibility,
        SliRuntimeCapabilityManifest capabilityManifest)
    {
        ArgumentNullException.ThrowIfNull(eligibility);
        ArgumentNullException.ThrowIfNull(capabilityManifest);

        var reasons = eligibility.MissingTargetCapabilities
            .Select(value => $"missing-capability:{value}")
            .Concat(eligibility.DisallowedOperations.Select(value => $"disallowed-operation:{value}"))
            .Concat(eligibility.ProfileViolations.Select(value => $"profile-violation:{value}"))
            .ToArray();
        var timestampUtc = DateTimeOffset.UtcNow;
        var admissionHandle = CreateDeterministicHandle(
            "target-admission://",
            eligibility.LaneId,
            capabilityManifest.RuntimeId,
            capabilityManifest.RealizationProfile.ProfileId,
            capabilityManifest.RealizationProfile.BudgetClass,
            capabilityManifest.RealizationProfile.CommitAuthorityClass,
            eligibility.IsEligible ? "accepted" : "refused",
            string.Join("|", reasons),
            eligibility.BudgetUsage.InstructionCount.ToString(),
            eligibility.BudgetUsage.SymbolicDepth.ToString(),
            eligibility.BudgetUsage.ProjectedTraceEntryCount.ToString(),
            eligibility.BudgetUsage.ProjectedResidueCount.ToString(),
            eligibility.BudgetUsage.WitnessOperationCount.ToString(),
            eligibility.BudgetUsage.TransportOperationCount.ToString(),
            timestampUtc.ToString("O"));

        return new SliTargetExecutionAdmission(
            AdmissionHandle: admissionHandle,
            LaneId: eligibility.LaneId,
            RuntimeId: capabilityManifest.RuntimeId,
            ProfileId: capabilityManifest.RealizationProfile.ProfileId,
            BudgetClass: capabilityManifest.RealizationProfile.BudgetClass,
            CommitAuthorityClass: capabilityManifest.RealizationProfile.CommitAuthorityClass,
            Accepted: eligibility.IsEligible,
            Reasons: reasons,
            BudgetUsage: eligibility.BudgetUsage,
            TimestampUtc: timestampUtc);
    }

    public static SliTargetExecutionLineage CreateLineage(
        SliTargetExecutionAdmission admission,
        string objective,
        string programId,
        int emittedTraceCount,
        int emittedResidueCount)
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);

        var timestampUtc = DateTimeOffset.UtcNow;
        var traceHandle = CreateDeterministicHandle(
            "target-trace://",
            admission.RuntimeId,
            admission.ProfileId,
            admission.LaneId,
            objective,
            programId,
            emittedTraceCount.ToString());
        var residueHandle = CreateDeterministicHandle(
            "target-residue://",
            admission.RuntimeId,
            admission.ProfileId,
            admission.LaneId,
            objective,
            programId,
            emittedResidueCount.ToString());
        var lineageHandle = CreateDeterministicHandle(
            "target-lineage://",
            admission.AdmissionHandle,
            objective,
            programId,
            traceHandle,
            residueHandle,
            timestampUtc.ToString("O"));

        return new SliTargetExecutionLineage(
            LineageHandle: lineageHandle,
            AdmissionHandle: admission.AdmissionHandle,
            LaneId: admission.LaneId,
            RuntimeId: admission.RuntimeId,
            ProfileId: admission.ProfileId,
            BudgetClass: admission.BudgetClass,
            CommitAuthorityClass: admission.CommitAuthorityClass,
            Objective: objective.Trim(),
            ProgramId: programId.Trim(),
            TraceHandle: traceHandle,
            ResidueHandle: residueHandle,
            EmittedTraceCount: emittedTraceCount,
            EmittedResidueCount: emittedResidueCount,
            TimestampUtc: timestampUtc);
    }

    private static string CreateDeterministicHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
