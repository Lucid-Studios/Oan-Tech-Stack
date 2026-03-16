using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public enum GovernedHopngArtifactProfile
{
    GoverningTrafficEvidence = 0,
    GovernanceTelemetryPhaseStack = 1
}

public enum GovernedHopngArtifactOutcome
{
    Created = 0,
    Unavailable = 1,
    Refused = 2,
    FailedValidation = 3,
    Failed = 4
}

public sealed record GovernedHopngEmissionRequest(
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    GovernedHopngArtifactProfile Profile,
    GovernanceLoopStage Stage,
    string RequestedBy,
    GovernanceDecisionReceipt DecisionReceipt,
    GovernanceLoopStateSnapshot Snapshot,
    IReadOnlyList<GovernanceJournalEntry> JournalEntries,
    CmeCollapseRoutingDecision? CollapseRoutingDecision);

public sealed record GovernedHopngArtifactReceipt(
    string ArtifactHandle,
    string LoopKey,
    Guid CandidateId,
    string CandidateProvenance,
    GovernedHopngArtifactProfile Profile,
    GovernanceLoopStage Stage,
    GovernedHopngArtifactOutcome Outcome,
    string IssuedBy,
    DateTimeOffset TimestampUtc,
    string? ArtifactId,
    string? ManifestPath,
    string? ProjectionPath,
    string? ValidationSummary,
    string? ProfileSummary,
    string? FailureCode);

public static class GovernedHopngArtifactKeys
{
    public static string CreateArtifactHandle(string loopKey, GovernedHopngArtifactProfile profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        return $"hopng-artifact://{GetProfileSlug(profile)}/{ComputeDigest(loopKey, profile.ToString())}";
    }

    public static string CreateArtifactDirectoryName(string loopKey, GovernedHopngArtifactProfile profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        return $"{GetProfileSlug(profile)}-{ComputeDigest(loopKey, profile.ToString())}";
    }

    public static string GetProfileSlug(GovernedHopngArtifactProfile profile) => profile switch
    {
        GovernedHopngArtifactProfile.GoverningTrafficEvidence => "governing-traffic-evidence",
        GovernedHopngArtifactProfile.GovernanceTelemetryPhaseStack => "governance-telemetry-phase-stack",
        _ => "unknown"
    };

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
