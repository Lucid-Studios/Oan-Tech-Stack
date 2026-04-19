using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedPostExecutionOperationalActionPacketMaterializationService
{
    GovernedSeedPostExecutionOperationalActionPacket Materialize(
        GovernedSeedPostParticipationExecutionPacket postParticipationExecutionPacket,
        GovernedSeedServiceEffectAssessment serviceEffectAssessment,
        GovernedSeedCommitIntent commitIntent,
        GovernedSeedOperationalActionCommitAssessment operationalActionCommitAssessment,
        GovernedSeedCommitReceipt commitReceipt,
        GovernedSeedPostExecutionOperationalActionAssessment postExecutionOperationalActionAssessment,
        GovernedSeedPostExecutionOperationalActionReceipt postExecutionOperationalActionReceipt);
}

public sealed class GovernedSeedPostExecutionOperationalActionPacketMaterializationService
    : IGovernedSeedPostExecutionOperationalActionPacketMaterializationService
{
    public GovernedSeedPostExecutionOperationalActionPacket Materialize(
        GovernedSeedPostParticipationExecutionPacket postParticipationExecutionPacket,
        GovernedSeedServiceEffectAssessment serviceEffectAssessment,
        GovernedSeedCommitIntent commitIntent,
        GovernedSeedOperationalActionCommitAssessment operationalActionCommitAssessment,
        GovernedSeedCommitReceipt commitReceipt,
        GovernedSeedPostExecutionOperationalActionAssessment postExecutionOperationalActionAssessment,
        GovernedSeedPostExecutionOperationalActionReceipt postExecutionOperationalActionReceipt)
    {
        ArgumentNullException.ThrowIfNull(postParticipationExecutionPacket);
        ArgumentNullException.ThrowIfNull(serviceEffectAssessment);
        ArgumentNullException.ThrowIfNull(commitIntent);
        ArgumentNullException.ThrowIfNull(operationalActionCommitAssessment);
        ArgumentNullException.ThrowIfNull(commitReceipt);
        ArgumentNullException.ThrowIfNull(postExecutionOperationalActionAssessment);
        ArgumentNullException.ThrowIfNull(postExecutionOperationalActionReceipt);

        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, serviceEffectAssessment.CandidateId, "execution/service-effect");
        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, commitIntent.CandidateId, "execution/commit-intent");
        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, operationalActionCommitAssessment.CandidateId, "execution/commit-assessment");
        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, commitReceipt.CandidateId, "execution/commit-receipt");
        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, postExecutionOperationalActionAssessment.CandidateId, "execution/unified-assessment");
        EnsureCandidateIdentity(postParticipationExecutionPacket.CandidateId, postExecutionOperationalActionReceipt.CandidateId, "execution/receipt");

        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, serviceEffectAssessment.PacketHandle, "execution/service-effect");
        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, commitIntent.PacketHandle, "execution/commit-intent");
        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, operationalActionCommitAssessment.PacketHandle, "execution/commit-assessment");
        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, commitReceipt.PacketHandle, "execution/commit-receipt");
        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, postExecutionOperationalActionAssessment.PacketHandle, "execution/unified-assessment");
        EnsurePacketIdentity(postParticipationExecutionPacket.PacketHandle, postExecutionOperationalActionReceipt.PacketHandle, "execution/receipt");

        return new GovernedSeedPostExecutionOperationalActionPacket(
            PacketHandle: CreateHandle(
                "governed-seed-post-execution-operational-action-packet://",
                postParticipationExecutionPacket.CandidateId,
                postExecutionOperationalActionReceipt.ReceiptHandle),
            CandidateId: postParticipationExecutionPacket.CandidateId,
            PostParticipationExecutionPacket: postParticipationExecutionPacket,
            ServiceEffectAssessment: serviceEffectAssessment,
            CommitIntent: commitIntent,
            OperationalActionCommitAssessment: operationalActionCommitAssessment,
            CommitReceipt: commitReceipt,
            PostExecutionOperationalActionAssessment: postExecutionOperationalActionAssessment,
            PostExecutionOperationalActionReceipt: postExecutionOperationalActionReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Post-execution operational-action witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-execution operational-action packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-execution operational-action packet requires consistent execution packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
