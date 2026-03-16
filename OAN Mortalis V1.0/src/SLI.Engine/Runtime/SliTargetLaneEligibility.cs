namespace SLI.Engine.Runtime;

public sealed record SliTargetLaneEligibility(
    string LaneId,
    string RuntimeId,
    bool IsEligible,
    IReadOnlyList<string> MissingTargetCapabilities,
    IReadOnlyList<string> DisallowedOperations,
    IReadOnlyList<string> ProfileViolations)
{
    public void EnsureEligible()
    {
        if (IsEligible)
        {
            return;
        }

        throw new SliTargetLaneRefusalException(this);
    }
}

public sealed class SliTargetLaneRefusalException : InvalidOperationException
{
    public SliTargetLaneRefusalException(SliTargetLaneEligibility eligibility)
        : base(BuildMessage(eligibility))
    {
        ArgumentNullException.ThrowIfNull(eligibility);
        Eligibility = eligibility;
    }

    public SliTargetLaneEligibility Eligibility { get; }

    private static string BuildMessage(SliTargetLaneEligibility eligibility)
    {
        var missing = eligibility.MissingTargetCapabilities.Count == 0
            ? "none"
            : string.Join(", ", eligibility.MissingTargetCapabilities);
        var disallowed = eligibility.DisallowedOperations.Count == 0
            ? "none"
            : string.Join(", ", eligibility.DisallowedOperations);
        var profileViolations = eligibility.ProfileViolations.Count == 0
            ? "none"
            : string.Join(", ", eligibility.ProfileViolations);

        return $"Target lane '{eligibility.LaneId}' refused for runtime '{eligibility.RuntimeId}'. Missing target capabilities: {missing}. Disallowed operations: {disallowed}. Profile violations: {profileViolations}.";
    }
}

public static class SliTargetLaneGuard
{
    public static SliTargetLaneEligibility EvaluateHigherOrderLocality(
        SliCoreProgram program,
        SliRuntimeCapabilityManifest targetManifest)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(targetManifest);

        var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var disallowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var profileViolations = EvaluateProfile(program, targetManifest.RealizationProfile);

        foreach (var instruction in program.Instructions)
        {
            if (instruction.OperationClass != SliRuntimeOperationClass.TargetCandidate)
            {
                disallowed.Add(instruction.Opcode);
                continue;
            }

            if (!targetManifest.TryGetCapability(instruction.Opcode, out var capability) ||
                capability.OperationClass != SliRuntimeOperationClass.TargetCandidate ||
                capability.Availability != SliRuntimeCapabilityAvailability.Available)
            {
                missing.Add(instruction.Opcode);
            }
        }

        return new SliTargetLaneEligibility(
            LaneId: "higher-order-locality",
            RuntimeId: targetManifest.RuntimeId,
            IsEligible: missing.Count == 0 && disallowed.Count == 0 && profileViolations.Count == 0,
            MissingTargetCapabilities: missing.OrderBy(opcode => opcode, StringComparer.OrdinalIgnoreCase).ToArray(),
            DisallowedOperations: disallowed.OrderBy(opcode => opcode, StringComparer.OrdinalIgnoreCase).ToArray(),
            ProfileViolations: profileViolations);
    }

    private static IReadOnlyList<string> EvaluateProfile(
        SliCoreProgram program,
        SliRuntimeRealizationProfile profile)
    {
        var violations = new List<string>();
        if (program.Instructions.Count > profile.MaxInstructionCount)
        {
            violations.Add($"instruction-budget-exceeded:{program.Instructions.Count}>{profile.MaxInstructionCount}");
        }

        if (program.Instructions.Count > profile.MaxSymbolicDepth)
        {
            violations.Add($"symbolic-depth-exceeded:{program.Instructions.Count}>{profile.MaxSymbolicDepth}");
        }

        var opcodes = program.Instructions
            .Select(instruction => instruction.Opcode)
            .ToArray();

        Require(
            violations,
            opcodes.Any(IsHigherOrderLocalityOpcode),
            profile.SupportsHigherOrderLocality,
            "higher-order-locality-unsupported");
        Require(
            violations,
            opcodes.Any(opcode => opcode.StartsWith("rehearsal-", StringComparison.OrdinalIgnoreCase)),
            profile.SupportsBoundedRehearsal,
            "bounded-rehearsal-unsupported");
        Require(
            violations,
            opcodes.Any(IsWitnessOpcode),
            profile.SupportsBoundedWitness,
            "bounded-witness-unsupported");
        Require(
            violations,
            opcodes.Any(opcode => opcode.StartsWith("transport-", StringComparison.OrdinalIgnoreCase)),
            profile.SupportsBoundedTransport,
            "bounded-transport-unsupported");
        Require(
            violations,
            opcodes.Any(opcode => opcode.StartsWith("surface-", StringComparison.OrdinalIgnoreCase)),
            profile.SupportsAdmissibleSurface,
            "admissible-surface-unsupported");
        Require(
            violations,
            opcodes.Any(opcode => opcode.StartsWith("packet-", StringComparison.OrdinalIgnoreCase)),
            profile.SupportsAccountabilityPacket,
            "accountability-packet-unsupported");
        Require(
            violations,
            opcodes.Any(opcode => opcode.EndsWith("-residue", StringComparison.OrdinalIgnoreCase)),
            profile.SupportsResidueEmission,
            "residue-emission-unsupported");
        Require(
            violations,
            program.Instructions.Count > 0,
            profile.SupportsSymbolicTrace,
            "symbolic-trace-unsupported");

        return violations;
    }

    private static void Require(
        ICollection<string> violations,
        bool conditionApplies,
        bool isSupported,
        string violationCode)
    {
        if (conditionApplies && !isSupported)
        {
            violations.Add(violationCode);
        }
    }

    private static bool IsHigherOrderLocalityOpcode(string opcode)
    {
        return opcode.StartsWith("locality-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("anchor-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("perspective-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("participation-", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "seal-posture", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "reveal-posture", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWitnessOpcode(string opcode)
    {
        return opcode.StartsWith("witness-", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "glue-threshold", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "morphism-candidate", StringComparison.OrdinalIgnoreCase);
    }
}
