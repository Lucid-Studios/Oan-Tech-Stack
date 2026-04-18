using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedPostParticipationExecutionPacketMaterializationService
{
    GovernedSeedPostParticipationExecutionPacket Materialize(
        GovernedSeedPostAdmissionParticipationPacket postAdmissionParticipationPacket,
        GovernedSeedServiceBehaviorAssessment serviceBehaviorAssessment,
        GovernedSeedExecutionAuthorizationAssessment executionAuthorizationAssessment,
        GovernedSeedPostParticipationExecutionAssessment postParticipationExecutionAssessment,
        GovernedSeedPostParticipationExecutionReceipt postParticipationExecutionReceipt);
}

public sealed class GovernedSeedPostParticipationExecutionPacketMaterializationService
    : IGovernedSeedPostParticipationExecutionPacketMaterializationService
{
    public GovernedSeedPostParticipationExecutionPacket Materialize(
        GovernedSeedPostAdmissionParticipationPacket postAdmissionParticipationPacket,
        GovernedSeedServiceBehaviorAssessment serviceBehaviorAssessment,
        GovernedSeedExecutionAuthorizationAssessment executionAuthorizationAssessment,
        GovernedSeedPostParticipationExecutionAssessment postParticipationExecutionAssessment,
        GovernedSeedPostParticipationExecutionReceipt postParticipationExecutionReceipt)
    {
        ArgumentNullException.ThrowIfNull(postAdmissionParticipationPacket);
        ArgumentNullException.ThrowIfNull(serviceBehaviorAssessment);
        ArgumentNullException.ThrowIfNull(executionAuthorizationAssessment);
        ArgumentNullException.ThrowIfNull(postParticipationExecutionAssessment);
        ArgumentNullException.ThrowIfNull(postParticipationExecutionReceipt);

        EnsureCandidateIdentity(postAdmissionParticipationPacket.CandidateId, serviceBehaviorAssessment.CandidateId, "participation/service-behavior");
        EnsureCandidateIdentity(postAdmissionParticipationPacket.CandidateId, executionAuthorizationAssessment.CandidateId, "participation/execution-authorization");
        EnsureCandidateIdentity(postAdmissionParticipationPacket.CandidateId, postParticipationExecutionAssessment.CandidateId, "participation/unified-assessment");
        EnsureCandidateIdentity(postAdmissionParticipationPacket.CandidateId, postParticipationExecutionReceipt.CandidateId, "participation/receipt");

        EnsurePacketIdentity(postAdmissionParticipationPacket.PacketHandle, serviceBehaviorAssessment.PacketHandle, "participation/service-behavior");
        EnsurePacketIdentity(postAdmissionParticipationPacket.PacketHandle, executionAuthorizationAssessment.PacketHandle, "participation/execution-authorization");
        EnsurePacketIdentity(postAdmissionParticipationPacket.PacketHandle, postParticipationExecutionAssessment.PacketHandle, "participation/unified-assessment");
        EnsurePacketIdentity(postAdmissionParticipationPacket.PacketHandle, postParticipationExecutionReceipt.PacketHandle, "participation/receipt");

        return new GovernedSeedPostParticipationExecutionPacket(
            PacketHandle: CreateHandle(
                "governed-seed-post-participation-execution-packet://",
                postAdmissionParticipationPacket.CandidateId,
                postParticipationExecutionReceipt.ReceiptHandle),
            CandidateId: postAdmissionParticipationPacket.CandidateId,
            PostAdmissionParticipationPacket: postAdmissionParticipationPacket,
            ServiceBehaviorAssessment: serviceBehaviorAssessment,
            ExecutionAuthorizationAssessment: executionAuthorizationAssessment,
            PostParticipationExecutionAssessment: postParticipationExecutionAssessment,
            PostParticipationExecutionReceipt: postParticipationExecutionReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Post-participation execution witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-participation execution packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-participation execution packet requires consistent participation packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
