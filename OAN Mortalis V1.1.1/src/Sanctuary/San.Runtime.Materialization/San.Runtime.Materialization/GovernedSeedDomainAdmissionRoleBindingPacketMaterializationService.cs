using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedDomainAdmissionRoleBindingPacketMaterializationService
{
    GovernedSeedDomainAdmissionRoleBindingPacket Materialize(
        GovernedSeedDomainRoleGatingPacket domainRoleGatingPacket,
        GovernedSeedDomainAdmissionAssessment domainAdmissionAssessment,
        GovernedSeedRoleBindingAssessment roleBindingAssessment,
        GovernedSeedDomainAdmissionRoleBindingAssessment domainAdmissionRoleBindingAssessment,
        GovernedSeedDomainAdmissionRoleBindingReceipt domainAdmissionRoleBindingReceipt);
}

public sealed class GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService
    : IGovernedSeedDomainAdmissionRoleBindingPacketMaterializationService
{
    public GovernedSeedDomainAdmissionRoleBindingPacket Materialize(
        GovernedSeedDomainRoleGatingPacket domainRoleGatingPacket,
        GovernedSeedDomainAdmissionAssessment domainAdmissionAssessment,
        GovernedSeedRoleBindingAssessment roleBindingAssessment,
        GovernedSeedDomainAdmissionRoleBindingAssessment domainAdmissionRoleBindingAssessment,
        GovernedSeedDomainAdmissionRoleBindingReceipt domainAdmissionRoleBindingReceipt)
    {
        ArgumentNullException.ThrowIfNull(domainRoleGatingPacket);
        ArgumentNullException.ThrowIfNull(domainAdmissionAssessment);
        ArgumentNullException.ThrowIfNull(roleBindingAssessment);
        ArgumentNullException.ThrowIfNull(domainAdmissionRoleBindingAssessment);
        ArgumentNullException.ThrowIfNull(domainAdmissionRoleBindingReceipt);

        EnsureCandidateIdentity(domainRoleGatingPacket.CandidateId, domainAdmissionAssessment.CandidateId, "gating/domain-admission");
        EnsureCandidateIdentity(domainRoleGatingPacket.CandidateId, roleBindingAssessment.CandidateId, "gating/role-binding");
        EnsureCandidateIdentity(domainRoleGatingPacket.CandidateId, domainAdmissionRoleBindingAssessment.CandidateId, "gating/unified-assessment");
        EnsureCandidateIdentity(domainRoleGatingPacket.CandidateId, domainAdmissionRoleBindingReceipt.CandidateId, "gating/receipt");

        EnsurePacketIdentity(domainRoleGatingPacket.PacketHandle, domainAdmissionAssessment.PacketHandle, "gating/domain-admission");
        EnsurePacketIdentity(domainRoleGatingPacket.PacketHandle, roleBindingAssessment.PacketHandle, "gating/role-binding");
        EnsurePacketIdentity(domainRoleGatingPacket.PacketHandle, domainAdmissionRoleBindingAssessment.PacketHandle, "gating/unified-assessment");
        EnsurePacketIdentity(domainRoleGatingPacket.PacketHandle, domainAdmissionRoleBindingReceipt.PacketHandle, "gating/receipt");

        return new GovernedSeedDomainAdmissionRoleBindingPacket(
            PacketHandle: CreateHandle(
                "governed-seed-domain-admission-role-binding-packet://",
                domainRoleGatingPacket.CandidateId,
                domainAdmissionRoleBindingReceipt.ReceiptHandle),
            CandidateId: domainRoleGatingPacket.CandidateId,
            DomainRoleGatingPacket: domainRoleGatingPacket,
            DomainAdmissionAssessment: domainAdmissionAssessment,
            RoleBindingAssessment: roleBindingAssessment,
            DomainAdmissionRoleBindingAssessment: domainAdmissionRoleBindingAssessment,
            DomainAdmissionRoleBindingReceipt: domainAdmissionRoleBindingReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Domain admission and role-binding witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Domain admission/role-binding packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Domain admission/role-binding packet requires consistent gating packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
