using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedDomainRoleGatingPacketMaterializationService
{
    GovernedSeedDomainRoleGatingPacket Materialize(
        GovernedSeedPreDomainGovernancePacket preDomainGovernancePacket,
        GovernedSeedDomainEligibilityAssessment domainEligibilityAssessment,
        GovernedSeedRoleEligibilityAssessment roleEligibilityAssessment,
        GovernedSeedDomainRoleGatingAssessment gatingAssessment,
        GovernedSeedDomainRoleGatingReceipt gatingReceipt);
}

public sealed class GovernedSeedDomainRoleGatingPacketMaterializationService
    : IGovernedSeedDomainRoleGatingPacketMaterializationService
{
    public GovernedSeedDomainRoleGatingPacket Materialize(
        GovernedSeedPreDomainGovernancePacket preDomainGovernancePacket,
        GovernedSeedDomainEligibilityAssessment domainEligibilityAssessment,
        GovernedSeedRoleEligibilityAssessment roleEligibilityAssessment,
        GovernedSeedDomainRoleGatingAssessment gatingAssessment,
        GovernedSeedDomainRoleGatingReceipt gatingReceipt)
    {
        ArgumentNullException.ThrowIfNull(preDomainGovernancePacket);
        ArgumentNullException.ThrowIfNull(domainEligibilityAssessment);
        ArgumentNullException.ThrowIfNull(roleEligibilityAssessment);
        ArgumentNullException.ThrowIfNull(gatingAssessment);
        ArgumentNullException.ThrowIfNull(gatingReceipt);

        EnsureCandidateIdentity(preDomainGovernancePacket.CandidateId, domainEligibilityAssessment.CandidateId, "pre-domain/domain");
        EnsureCandidateIdentity(preDomainGovernancePacket.CandidateId, roleEligibilityAssessment.CandidateId, "pre-domain/role");
        EnsureCandidateIdentity(preDomainGovernancePacket.CandidateId, gatingAssessment.CandidateId, "pre-domain/gating");
        EnsureCandidateIdentity(preDomainGovernancePacket.CandidateId, gatingReceipt.CandidateId, "pre-domain/receipt");

        EnsurePacketIdentity(preDomainGovernancePacket.PacketHandle, domainEligibilityAssessment.PacketHandle, "pre-domain/domain");
        EnsurePacketIdentity(preDomainGovernancePacket.PacketHandle, roleEligibilityAssessment.PacketHandle, "pre-domain/role");
        EnsurePacketIdentity(preDomainGovernancePacket.PacketHandle, gatingAssessment.PacketHandle, "pre-domain/gating");
        EnsurePacketIdentity(preDomainGovernancePacket.PacketHandle, gatingReceipt.PacketHandle, "pre-domain/receipt");

        return new GovernedSeedDomainRoleGatingPacket(
            PacketHandle: CreateHandle(
                "governed-seed-domain-role-gating-packet://",
                preDomainGovernancePacket.CandidateId,
                gatingReceipt.ReceiptHandle),
            CandidateId: preDomainGovernancePacket.CandidateId,
            PreDomainGovernancePacket: preDomainGovernancePacket,
            DomainEligibilityAssessment: domainEligibilityAssessment,
            RoleEligibilityAssessment: roleEligibilityAssessment,
            GatingAssessment: gatingAssessment,
            GatingReceipt: gatingReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Domain/role gating witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Domain/role gating packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static void EnsurePacketIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Domain/role gating packet requires consistent pre-domain packet identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
