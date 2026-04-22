using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedPostAdmissionParticipationPacketMaterializationService
{
    GovernedSeedPostAdmissionParticipationPacket Materialize(
        GovernedSeedDomainAdmissionRoleBindingPacket domainAdmissionRoleBindingPacket,
        GovernedSeedDomainOccupancyAssessment domainOccupancyAssessment,
        GovernedSeedRoleParticipationAssessment roleParticipationAssessment,
        GovernedSeedPostAdmissionParticipationAssessment postAdmissionParticipationAssessment,
        GovernedSeedPostAdmissionParticipationReceipt postAdmissionParticipationReceipt);
}

public sealed class GovernedSeedPostAdmissionParticipationPacketMaterializationService
    : IGovernedSeedPostAdmissionParticipationPacketMaterializationService
{
    public GovernedSeedPostAdmissionParticipationPacket Materialize(
        GovernedSeedDomainAdmissionRoleBindingPacket domainAdmissionRoleBindingPacket,
        GovernedSeedDomainOccupancyAssessment domainOccupancyAssessment,
        GovernedSeedRoleParticipationAssessment roleParticipationAssessment,
        GovernedSeedPostAdmissionParticipationAssessment postAdmissionParticipationAssessment,
        GovernedSeedPostAdmissionParticipationReceipt postAdmissionParticipationReceipt)
    {
        ArgumentNullException.ThrowIfNull(domainAdmissionRoleBindingPacket);
        ArgumentNullException.ThrowIfNull(domainOccupancyAssessment);
        ArgumentNullException.ThrowIfNull(roleParticipationAssessment);
        ArgumentNullException.ThrowIfNull(postAdmissionParticipationAssessment);
        ArgumentNullException.ThrowIfNull(postAdmissionParticipationReceipt);

        EnsureCandidateIdentity(domainAdmissionRoleBindingPacket.CandidateId, domainOccupancyAssessment.CandidateId, "admission-binding/domain-occupancy");
        EnsureCandidateIdentity(domainAdmissionRoleBindingPacket.CandidateId, roleParticipationAssessment.CandidateId, "admission-binding/role-participation");
        EnsureCandidateIdentity(domainAdmissionRoleBindingPacket.CandidateId, postAdmissionParticipationAssessment.CandidateId, "admission-binding/unified-assessment");
        EnsureCandidateIdentity(domainAdmissionRoleBindingPacket.CandidateId, postAdmissionParticipationReceipt.CandidateId, "admission-binding/receipt");

        EnsurePacketIdentity(domainAdmissionRoleBindingPacket.PacketHandle, domainOccupancyAssessment.PacketHandle, "admission-binding/domain-occupancy");
        EnsurePacketIdentity(domainAdmissionRoleBindingPacket.PacketHandle, roleParticipationAssessment.PacketHandle, "admission-binding/role-participation");
        EnsurePacketIdentity(domainAdmissionRoleBindingPacket.PacketHandle, postAdmissionParticipationAssessment.PacketHandle, "admission-binding/unified-assessment");
        EnsurePacketIdentity(domainAdmissionRoleBindingPacket.PacketHandle, postAdmissionParticipationReceipt.PacketHandle, "admission-binding/receipt");

        return new GovernedSeedPostAdmissionParticipationPacket(
            PacketHandle: CreateHandle(
                "governed-seed-post-admission-participation-packet://",
                domainAdmissionRoleBindingPacket.CandidateId,
                postAdmissionParticipationReceipt.ReceiptHandle),
            CandidateId: domainAdmissionRoleBindingPacket.CandidateId,
            DomainAdmissionRoleBindingPacket: domainAdmissionRoleBindingPacket,
            DomainOccupancyAssessment: domainOccupancyAssessment,
            RoleParticipationAssessment: roleParticipationAssessment,
            PostAdmissionParticipationAssessment: postAdmissionParticipationAssessment,
            PostAdmissionParticipationReceipt: postAdmissionParticipationReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Post-admission participation witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-admission participation packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Post-admission participation packet requires consistent admission/binding packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
