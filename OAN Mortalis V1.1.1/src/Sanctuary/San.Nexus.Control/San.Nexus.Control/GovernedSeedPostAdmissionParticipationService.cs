using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedPostAdmissionParticipationService
{
    GovernedSeedPostAdmissionParticipationResult Evaluate(
        GovernedSeedDomainAdmissionRoleBindingPacket packet);
}

public sealed record GovernedSeedPostAdmissionParticipationResult(
    GovernedSeedDomainOccupancyAssessment DomainOccupancyAssessment,
    GovernedSeedRoleParticipationAssessment RoleParticipationAssessment,
    GovernedSeedPostAdmissionParticipationAssessment UnifiedAssessment,
    GovernedSeedPostAdmissionParticipationReceipt Receipt);

public sealed class GovernedSeedPostAdmissionParticipationService
    : IGovernedSeedPostAdmissionParticipationService
{
    public GovernedSeedPostAdmissionParticipationResult Evaluate(
        GovernedSeedDomainAdmissionRoleBindingPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var crypticAuthorityBleedDetected =
            packet.DomainAdmissionAssessment.CrypticAuthorityBleedDetected ||
            packet.DomainAdmissionRoleBindingAssessment.CrypticAuthorityBleedDetected ||
            packet.DomainRoleGatingPacket.PreDomainGovernancePacket.SeparationReceipt.CrypticAuthorityBleedDetected;
        var standingConsistent = packet.DomainAdmissionAssessment.StandingConsistent &&
                                 packet.DomainRoleGatingPacket.PreDomainGovernancePacket.PrimeSeedStateReceipt.SeedState ==
                                 PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var revalidationConsistent = packet.DomainRoleGatingPacket.DomainEligibilityAssessment.RevalidationConsistent;
        var domainAdmissionGranted = packet.DomainAdmissionAssessment.DomainAdmissionGranted &&
                                     packet.DomainAdmissionRoleBindingAssessment.DomainAdmissionGranted &&
                                     packet.DomainAdmissionRoleBindingReceipt.DomainAdmissionGranted;
        var attributionPreserved =
            packet.DomainAdmissionAssessment.BurdenAttributableAtDomainScope &&
            packet.DomainRoleGatingPacket.PreDomainGovernancePacket.HostLoopReceipt.CandidateHandles.Count > 0;
        var occupancyStructurePresent =
            packet.DomainRoleGatingPacket.PreDomainGovernancePacket.SeparationReceipt.PrimeMaterialCount > 1 &&
            packet.DomainRoleGatingPacket.PreDomainGovernancePacket.DuplexGovernanceReceipt.PrimeSurfaceEstablished;
        var occupancyAuthorized = packetComplete &&
                                  !crypticAuthorityBleedDetected &&
                                  standingConsistent &&
                                  revalidationConsistent &&
                                  domainAdmissionGranted &&
                                  attributionPreserved &&
                                  occupancyStructurePresent;

        var roleBound = packet.RoleBindingAssessment.RoleBound &&
                        packet.DomainAdmissionRoleBindingAssessment.RoleBound &&
                        packet.DomainAdmissionRoleBindingReceipt.RoleBound;
        var scopeExpansionDetected = roleBound && !domainAdmissionGranted;
        var roleParticipationAuthorized = occupancyAuthorized &&
                                          roleBound &&
                                          packet.RoleBindingAssessment.RoleLawfulWithinDomain &&
                                          packet.RoleBindingAssessment.ResponsibilityBindableAtRoleScope &&
                                          !scopeExpansionDetected;

        var occupancyAssessment = new GovernedSeedDomainOccupancyAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            DomainAdmissionGranted: domainAdmissionGranted,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            OccupancyStructurePresent: occupancyStructurePresent,
            OccupancyAuthorized: occupancyAuthorized,
            Summary: occupancyAuthorized
                ? "Admission/binding packet supports lawful domain occupancy."
                : "Admission/binding packet does not yet support lawful domain occupancy.");

        var roleAssessment = new GovernedSeedRoleParticipationAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            RoleLawfulWithinDomain: packet.RoleBindingAssessment.RoleLawfulWithinDomain,
            ResponsibilityBindableAtRoleScope: packet.RoleBindingAssessment.ResponsibilityBindableAtRoleScope,
            ScopeExpansionDetected: scopeExpansionDetected,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: roleParticipationAuthorized
                ? "Admission/binding packet supports lawful role-bearing participation."
                : "Admission/binding packet does not yet support lawful role-bearing participation.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            crypticAuthorityBleedDetected,
            standingConsistent,
            revalidationConsistent,
            domainAdmissionGranted,
            occupancyAuthorized,
            roleParticipationAuthorized,
            roleBound);

        var unifiedAssessment = new GovernedSeedPostAdmissionParticipationAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            DomainAdmissionGranted: domainAdmissionGranted,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedPostAdmissionParticipationReceipt(
            ReceiptHandle: $"post-admission-participation://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: unifiedAssessment.Summary);

        return new GovernedSeedPostAdmissionParticipationResult(
            occupancyAssessment,
            roleAssessment,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedPostAdmissionParticipationDisposition DetermineDisposition(
        GovernedSeedDomainAdmissionRoleBindingPacket packet,
        bool packetComplete,
        bool crypticAuthorityBleedDetected,
        bool standingConsistent,
        bool revalidationConsistent,
        bool domainAdmissionGranted,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized,
        bool roleBound)
    {
        if (!packetComplete ||
            crypticAuthorityBleedDetected ||
            !standingConsistent ||
            !revalidationConsistent)
        {
            return GovernedSeedPostAdmissionParticipationDisposition.Refuse;
        }

        if (!domainAdmissionGranted)
        {
            return packet.DomainAdmissionRoleBindingReceipt.Disposition ==
                   GovernedSeedDomainAdmissionRoleBindingDisposition.RemainAtGatingPacket
                ? GovernedSeedPostAdmissionParticipationDisposition.RemainAtAdmissionPacket
                : GovernedSeedPostAdmissionParticipationDisposition.Refuse;
        }

        if (roleParticipationAuthorized)
        {
            return GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized;
        }

        if (roleBound && !roleParticipationAuthorized)
        {
            return GovernedSeedPostAdmissionParticipationDisposition.ReturnToBindingPending;
        }

        if (occupancyAuthorized)
        {
            return GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized;
        }

        return GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyPending;
    }

    private static bool PacketIsComplete(GovernedSeedDomainAdmissionRoleBindingPacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.DomainRoleGatingPacket.PacketHandle) &&
               HasValue(packet.DomainAdmissionAssessment.PacketHandle) &&
               HasValue(packet.RoleBindingAssessment.PacketHandle) &&
               HasValue(packet.DomainAdmissionRoleBindingAssessment.PacketHandle) &&
               HasValue(packet.DomainAdmissionRoleBindingReceipt.PacketHandle) &&
               HasValue(packet.DomainAdmissionRoleBindingReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.DomainRoleGatingPacket.CandidateId &&
               packet.CandidateId == packet.DomainAdmissionAssessment.CandidateId &&
               packet.CandidateId == packet.RoleBindingAssessment.CandidateId &&
               packet.CandidateId == packet.DomainAdmissionRoleBindingAssessment.CandidateId &&
               packet.CandidateId == packet.DomainAdmissionRoleBindingReceipt.CandidateId &&
               packet.DomainRoleGatingPacket.PacketHandle == packet.DomainAdmissionAssessment.PacketHandle &&
               packet.DomainRoleGatingPacket.PacketHandle == packet.RoleBindingAssessment.PacketHandle &&
               packet.DomainRoleGatingPacket.PacketHandle == packet.DomainAdmissionRoleBindingAssessment.PacketHandle &&
               packet.DomainRoleGatingPacket.PacketHandle == packet.DomainAdmissionRoleBindingReceipt.PacketHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedPostAdmissionParticipationDisposition disposition) =>
        disposition switch
        {
            GovernedSeedPostAdmissionParticipationDisposition.RoleParticipationAuthorized =>
                "Admission/binding packet may enter lawful role-bearing participation.",
            GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized =>
                "Admission/binding packet may enter lawful domain occupancy without role-bearing participation.",
            GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyPending =>
                "Admission/binding packet remains domain-admitted but occupancy-ready participation is still pending.",
            GovernedSeedPostAdmissionParticipationDisposition.ReturnToBindingPending =>
                "Admission/binding packet must return to binding-pending before role-bearing participation.",
            GovernedSeedPostAdmissionParticipationDisposition.RemainAtAdmissionPacket =>
                "Admission/binding packet remains at the admission layer without post-admission participation.",
            _ =>
                "Admission/binding packet must be refused for post-admission participation."
        };
}
