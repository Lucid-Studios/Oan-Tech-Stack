using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedDomainAdmissionRoleBindingService
{
    GovernedSeedDomainAdmissionRoleBindingResult Evaluate(
        GovernedSeedDomainRoleGatingPacket packet);
}

public sealed record GovernedSeedDomainAdmissionRoleBindingResult(
    GovernedSeedDomainAdmissionAssessment DomainAdmissionAssessment,
    GovernedSeedRoleBindingAssessment RoleBindingAssessment,
    GovernedSeedDomainAdmissionRoleBindingAssessment UnifiedAssessment,
    GovernedSeedDomainAdmissionRoleBindingReceipt Receipt);

public sealed class GovernedSeedDomainAdmissionRoleBindingService
    : IGovernedSeedDomainAdmissionRoleBindingService
{
    public GovernedSeedDomainAdmissionRoleBindingResult Evaluate(
        GovernedSeedDomainRoleGatingPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var crypticAuthorityBleedDetected =
            packet.PreDomainGovernancePacket.SeparationReceipt.CrypticAuthorityBleedDetected ||
            packet.GatingAssessment.CrypticAuthorityBleedDetected;
        var standingConsistent = packet.GatingAssessment.PacketComplete &&
                                 packet.GatingAssessment.DomainEligible ==
                                 packet.GatingReceipt.DomainEligible &&
                                 packet.PreDomainGovernancePacket.PrimeSeedStateReceipt.SeedState ==
                                 PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var domainEligibilitySatisfied = packet.DomainEligibilityAssessment.DomainEligible &&
                                         packet.GatingReceipt.DomainEligible &&
                                         packet.GatingReceipt.Disposition is
                                             GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete or
                                             GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible;
        var burdenAttributableAtDomainScope =
            packet.RoleEligibilityAssessment.ResponsibilityAttributable &&
            packet.PreDomainGovernancePacket.SeparationReceipt.PrimeMaterialCount > 0;
        var domainAdmissionGranted = packetComplete &&
                                     !crypticAuthorityBleedDetected &&
                                     standingConsistent &&
                                     domainEligibilitySatisfied &&
                                     burdenAttributableAtDomainScope;

        var domainAssessment = new GovernedSeedDomainAdmissionAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            StandingConsistent: standingConsistent,
            DomainEligibilitySatisfied: domainEligibilitySatisfied,
            BurdenAttributableAtDomainScope: burdenAttributableAtDomainScope,
            DomainAdmissionGranted: domainAdmissionGranted,
            Summary: domainAdmissionGranted
                ? "Gating packet supports actual domain admission."
                : "Gating packet does not yet support actual domain admission.");

        var roleRelevantStructurePresent = packet.RoleEligibilityAssessment.RoleRelevantStructurePresent;
        var responsibilityBindableAtRoleScope =
            packet.RoleEligibilityAssessment.ResponsibilityAttributable &&
            packet.PreDomainGovernancePacket.SeparationReceipt.PrimeMaterialCount > 1;
        var roleLawfulWithinDomain = domainAdmissionGranted &&
                                     packet.RoleEligibilityAssessment.DomainEligible &&
                                     packet.GatingReceipt.Disposition == GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible;
        var roleBound = domainAdmissionGranted &&
                        roleRelevantStructurePresent &&
                        responsibilityBindableAtRoleScope &&
                        roleLawfulWithinDomain &&
                        packet.RoleEligibilityAssessment.RoleEligible;

        var roleAssessment = new GovernedSeedRoleBindingAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleRelevantStructurePresent: roleRelevantStructurePresent,
            ResponsibilityBindableAtRoleScope: responsibilityBindableAtRoleScope,
            RoleLawfulWithinDomain: roleLawfulWithinDomain,
            RoleBound: roleBound,
            Summary: roleBound
                ? "Gating packet supports lawful role binding."
                : "Gating packet does not yet support lawful role binding.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            crypticAuthorityBleedDetected,
            domainAdmissionGranted,
            roleBound);

        var unifiedAssessment = new GovernedSeedDomainAdmissionRoleBindingAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedDomainAdmissionRoleBindingReceipt(
            ReceiptHandle: $"domain-admission-role-binding://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: unifiedAssessment.Summary);

        return new GovernedSeedDomainAdmissionRoleBindingResult(
            domainAssessment,
            roleAssessment,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedDomainAdmissionRoleBindingDisposition DetermineDisposition(
        GovernedSeedDomainRoleGatingPacket packet,
        bool packetComplete,
        bool crypticAuthorityBleedDetected,
        bool domainAdmissionGranted,
        bool roleBound)
    {
        if (!packetComplete || crypticAuthorityBleedDetected)
        {
            return GovernedSeedDomainAdmissionRoleBindingDisposition.Refuse;
        }

        if (packet.GatingReceipt.Disposition == GovernedSeedDomainRoleGatingDisposition.CrypticOnlyCarry)
        {
            return GovernedSeedDomainAdmissionRoleBindingDisposition.ReturnToPreDomainCarry;
        }

        if (domainAdmissionGranted && roleBound)
        {
            return GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound;
        }

        if (domainAdmissionGranted)
        {
            return GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending;
        }

        if (packet.GatingReceipt.Disposition == GovernedSeedDomainRoleGatingDisposition.RemainPreDomain)
        {
            return GovernedSeedDomainAdmissionRoleBindingDisposition.RemainAtGatingPacket;
        }

        return GovernedSeedDomainAdmissionRoleBindingDisposition.Refuse;
    }

    private static bool PacketIsComplete(GovernedSeedDomainRoleGatingPacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.PreDomainGovernancePacket.PacketHandle) &&
               HasValue(packet.DomainEligibilityAssessment.PacketHandle) &&
               HasValue(packet.RoleEligibilityAssessment.PacketHandle) &&
               HasValue(packet.GatingAssessment.PacketHandle) &&
               HasValue(packet.GatingReceipt.PacketHandle) &&
               HasValue(packet.GatingReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.PreDomainGovernancePacket.CandidateId &&
               packet.CandidateId == packet.DomainEligibilityAssessment.CandidateId &&
               packet.CandidateId == packet.RoleEligibilityAssessment.CandidateId &&
               packet.CandidateId == packet.GatingAssessment.CandidateId &&
               packet.CandidateId == packet.GatingReceipt.CandidateId &&
               packet.PreDomainGovernancePacket.PacketHandle == packet.DomainEligibilityAssessment.PacketHandle &&
               packet.PreDomainGovernancePacket.PacketHandle == packet.RoleEligibilityAssessment.PacketHandle &&
               packet.PreDomainGovernancePacket.PacketHandle == packet.GatingAssessment.PacketHandle &&
               packet.PreDomainGovernancePacket.PacketHandle == packet.GatingReceipt.PacketHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedDomainAdmissionRoleBindingDisposition disposition) =>
        disposition switch
        {
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAndRoleBound =>
                "Gating packet may enter actual domain admission with lawful role binding.",
            GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending =>
                "Gating packet may enter actual domain admission, but role binding remains pending.",
            GovernedSeedDomainAdmissionRoleBindingDisposition.ReturnToPreDomainCarry =>
                "Gating packet must return to explicit pre-domain carry.",
            GovernedSeedDomainAdmissionRoleBindingDisposition.RemainAtGatingPacket =>
                "Gating packet remains at the approachability layer.",
            _ =>
                "Gating packet must be refused for admission and role binding."
        };
}
