using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedPostActionServiceEnactmentPacketMaterializationService
{
    GovernedSeedPostActionServiceEnactmentPacket Materialize(
        GovernedSeedPostExecutionOperationalActionPacket postExecutionOperationalActionPacket,
        GovernedSeedEffectEmissionAssessment effectEmissionAssessment,
        GovernedSeedServiceEnactmentCommitAssessment serviceEnactmentCommitAssessment,
        GovernedSeedPostActionServiceEnactmentAssessment postActionServiceEnactmentAssessment,
        GovernedSeedPostActionServiceEnactmentReceipt postActionServiceEnactmentReceipt);
}

public sealed class GovernedSeedPostActionServiceEnactmentPacketMaterializationService
    : IGovernedSeedPostActionServiceEnactmentPacketMaterializationService
{
    public GovernedSeedPostActionServiceEnactmentPacket Materialize(
        GovernedSeedPostExecutionOperationalActionPacket postExecutionOperationalActionPacket,
        GovernedSeedEffectEmissionAssessment effectEmissionAssessment,
        GovernedSeedServiceEnactmentCommitAssessment serviceEnactmentCommitAssessment,
        GovernedSeedPostActionServiceEnactmentAssessment postActionServiceEnactmentAssessment,
        GovernedSeedPostActionServiceEnactmentReceipt postActionServiceEnactmentReceipt)
    {
        ArgumentNullException.ThrowIfNull(postExecutionOperationalActionPacket);
        ArgumentNullException.ThrowIfNull(effectEmissionAssessment);
        ArgumentNullException.ThrowIfNull(serviceEnactmentCommitAssessment);
        ArgumentNullException.ThrowIfNull(postActionServiceEnactmentAssessment);
        ArgumentNullException.ThrowIfNull(postActionServiceEnactmentReceipt);

        EnsureCandidateIdentity(postExecutionOperationalActionPacket.CandidateId, effectEmissionAssessment.CandidateId, "operational-action/effect-emission");
        EnsureCandidateIdentity(postExecutionOperationalActionPacket.CandidateId, serviceEnactmentCommitAssessment.CandidateId, "operational-action/enactment-commit");
        EnsureCandidateIdentity(postExecutionOperationalActionPacket.CandidateId, postActionServiceEnactmentAssessment.CandidateId, "operational-action/unified-assessment");
        EnsureCandidateIdentity(postExecutionOperationalActionPacket.CandidateId, postActionServiceEnactmentReceipt.CandidateId, "operational-action/receipt");

        EnsurePacketIdentity(postExecutionOperationalActionPacket.PacketHandle, effectEmissionAssessment.PacketHandle, "operational-action/effect-emission");
        EnsurePacketIdentity(postExecutionOperationalActionPacket.PacketHandle, serviceEnactmentCommitAssessment.PacketHandle, "operational-action/enactment-commit");
        EnsurePacketIdentity(postExecutionOperationalActionPacket.PacketHandle, postActionServiceEnactmentAssessment.PacketHandle, "operational-action/unified-assessment");
        EnsurePacketIdentity(postExecutionOperationalActionPacket.PacketHandle, postActionServiceEnactmentReceipt.PacketHandle, "operational-action/receipt");

        return new GovernedSeedPostActionServiceEnactmentPacket(
            PacketHandle: CreateHandle(
                "governed-seed-post-action-service-enactment-packet://",
                postExecutionOperationalActionPacket.CandidateId,
                postActionServiceEnactmentReceipt.ReceiptHandle),
            CandidateId: postExecutionOperationalActionPacket.CandidateId,
            PostExecutionOperationalActionPacket: postExecutionOperationalActionPacket,
            EffectEmissionAssessment: effectEmissionAssessment,
            ServiceEnactmentCommitAssessment: serviceEnactmentCommitAssessment,
            PostActionServiceEnactmentAssessment: postActionServiceEnactmentAssessment,
            PostActionServiceEnactmentReceipt: postActionServiceEnactmentReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Post-action service-enactment witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-action service-enactment packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-action service-enactment packet requires consistent operational-action packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
