using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedDomainRoleGatingService
{
    GovernedSeedDomainRoleGatingResult Evaluate(
        GovernedSeedPreDomainGovernancePacket packet);
}

public sealed record GovernedSeedDomainRoleGatingResult(
    GovernedSeedDomainEligibilityAssessment DomainAssessment,
    GovernedSeedRoleEligibilityAssessment RoleAssessment,
    GovernedSeedDomainRoleGatingAssessment GatingAssessment,
    GovernedSeedDomainRoleGatingReceipt Receipt);

public sealed class GovernedSeedDomainRoleGatingService : IGovernedSeedDomainRoleGatingService
{
    public GovernedSeedDomainRoleGatingResult Evaluate(
        GovernedSeedPreDomainGovernancePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var crypticAuthorityBleedDetected = packet.SeparationReceipt.CrypticAuthorityBleedDetected;
        var standingConsistent = packet.PrimeSeedStateReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding &&
                                 packet.HostLoopReceipt.SeedReady;
        var revalidationConsistent = packet.AdmissionGateReceipt.Disposition != PrimeSeedPreDomainAdmissionDisposition.Refuse;
        var primeAdmissionStructurePresent = packet.SeparationReceipt.PrimeMaterialCount > 0 &&
                                            packet.DuplexGovernanceReceipt.PrimeSurfaceEstablished;
        var forwardMotionSupported =
            packet.AdmissionGateReceipt.Disposition == PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate;
        var domainEligible = packetComplete &&
                             standingConsistent &&
                             revalidationConsistent &&
                             !crypticAuthorityBleedDetected &&
                             primeAdmissionStructurePresent &&
                             forwardMotionSupported;
        var roleRelevantStructurePresent = packet.SeparationReceipt.PrimeMaterialCount > 1 &&
                                           packet.DuplexGovernanceReceipt.PrimeSurfaceEstablished;
        var responsibilityAttributable = packet.HostLoopReceipt.CandidateHandles.Count > 0 &&
                                         packet.SeparationReceipt.PrimeMaterialCount > 0;
        var roleEligible = domainEligible &&
                           roleRelevantStructurePresent &&
                           responsibilityAttributable;

        var domainAssessment = new GovernedSeedDomainEligibilityAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            PrimeAdmissionStructurePresent: primeAdmissionStructurePresent,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            ForwardMotionSupported: forwardMotionSupported,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            DomainEligible: domainEligible,
            Summary: domainEligible
                ? "Packet is domain-eligible for the next gate."
                : "Packet does not yet satisfy domain gating requirements.");

        var roleAssessment = new GovernedSeedRoleEligibilityAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            DomainEligible: domainEligible,
            RoleRelevantStructurePresent: roleRelevantStructurePresent,
            ResponsibilityAttributable: responsibilityAttributable,
            RoleEligible: roleEligible,
            Summary: roleEligible
                ? "Packet carries sufficient role-relevant structure for the next gate."
                : "Packet is not yet role-eligible.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            crypticAuthorityBleedDetected,
            standingConsistent,
            domainEligible,
            roleEligible);

        var gatingAssessment = new GovernedSeedDomainRoleGatingAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            StandingConsistent: standingConsistent,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedDomainRoleGatingReceipt(
            ReceiptHandle: $"domain-role-gating://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: gatingAssessment.Summary);

        return new GovernedSeedDomainRoleGatingResult(
            domainAssessment,
            roleAssessment,
            gatingAssessment,
            receipt);
    }

    private static GovernedSeedDomainRoleGatingDisposition DetermineDisposition(
        GovernedSeedPreDomainGovernancePacket packet,
        bool packetComplete,
        bool crypticAuthorityBleedDetected,
        bool standingConsistent,
        bool domainEligible,
        bool roleEligible)
    {
        if (!packetComplete ||
            crypticAuthorityBleedDetected ||
            !standingConsistent)
        {
            return GovernedSeedDomainRoleGatingDisposition.Refuse;
        }

        if (packet.AdmissionGateReceipt.Disposition == PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly)
        {
            return GovernedSeedDomainRoleGatingDisposition.CrypticOnlyCarry;
        }

        if (domainEligible && roleEligible)
        {
            return GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible;
        }

        if (domainEligible)
        {
            return GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete;
        }

        if (packet.AdmissionGateReceipt.Disposition == PrimeSeedPreDomainAdmissionDisposition.RemainPreDomain ||
            packet.AdmissionGateReceipt.Disposition == PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate)
        {
            return GovernedSeedDomainRoleGatingDisposition.RemainPreDomain;
        }

        return GovernedSeedDomainRoleGatingDisposition.Refuse;
    }

    private static bool PacketIsComplete(GovernedSeedPreDomainGovernancePacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.PrimeSeedStateReceipt.ReceiptHandle) &&
               HasValue(packet.BoundaryReceipt.ReceiptHandle) &&
               HasValue(packet.HoldingInspectionReceipt.ReceiptHandle) &&
               HasValue(packet.FormOrCleaveAssessment.AssessmentHandle) &&
               HasValue(packet.SeparationReceipt.ReceiptHandle) &&
               HasValue(packet.DuplexGovernanceReceipt.ReceiptHandle) &&
               HasValue(packet.AdmissionGateReceipt.ReceiptHandle) &&
               HasValue(packet.HostLoopReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.BoundaryReceipt.CandidateId &&
               packet.CandidateId == packet.SeparationReceipt.CandidateId &&
               packet.CandidateId == packet.DuplexGovernanceReceipt.CandidateId &&
               packet.CandidateId == packet.AdmissionGateReceipt.CandidateId &&
               packet.PrimeSeedStateReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding &&
               packet.HostLoopReceipt.CandidateBoundaryReceiptHandle == packet.BoundaryReceipt.ReceiptHandle &&
               packet.HostLoopReceipt.CrypticHoldingInspectionHandle == packet.HoldingInspectionReceipt.ReceiptHandle &&
               packet.HostLoopReceipt.FormOrCleaveAssessmentHandle == packet.FormOrCleaveAssessment.AssessmentHandle &&
               packet.HostLoopReceipt.CandidateSeparationReceiptHandle == packet.SeparationReceipt.ReceiptHandle &&
               packet.HostLoopReceipt.DuplexGovernanceReceiptHandle == packet.DuplexGovernanceReceipt.ReceiptHandle &&
               packet.HostLoopReceipt.AdmissionGateReceiptHandle == packet.AdmissionGateReceipt.ReceiptHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedDomainRoleGatingDisposition disposition) =>
        disposition switch
        {
            GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible =>
                "Packet may approach both domain and role gating.",
            GovernedSeedDomainRoleGatingDisposition.DomainAdmissibleRoleIncomplete =>
                "Packet may approach domain gating, but role gating remains incomplete.",
            GovernedSeedDomainRoleGatingDisposition.CrypticOnlyCarry =>
                "Packet remains in cryptic-only carry without domain or role promotion.",
            GovernedSeedDomainRoleGatingDisposition.RemainPreDomain =>
                "Packet remains pre-domain pending stronger Prime-governed structure.",
            _ =>
                "Packet must be refused for domain and role gating."
        };
}
