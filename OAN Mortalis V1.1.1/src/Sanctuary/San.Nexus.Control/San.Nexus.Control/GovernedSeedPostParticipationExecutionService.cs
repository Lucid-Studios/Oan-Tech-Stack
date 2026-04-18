using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedPostParticipationExecutionService
{
    GovernedSeedPostParticipationExecutionResult Evaluate(
        GovernedSeedPostAdmissionParticipationPacket packet);
}

public sealed record GovernedSeedPostParticipationExecutionResult(
    GovernedSeedServiceBehaviorAssessment ServiceBehaviorAssessment,
    GovernedSeedExecutionAuthorizationAssessment ExecutionAuthorizationAssessment,
    GovernedSeedPostParticipationExecutionAssessment UnifiedAssessment,
    GovernedSeedPostParticipationExecutionReceipt Receipt);

public sealed class GovernedSeedPostParticipationExecutionService
    : IGovernedSeedPostParticipationExecutionService
{
    public GovernedSeedPostParticipationExecutionResult Evaluate(
        GovernedSeedPostAdmissionParticipationPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var standingConsistent =
            packet.PostAdmissionParticipationAssessment.StandingConsistent &&
            packet.DomainAdmissionRoleBindingPacket.DomainAdmissionAssessment.StandingConsistent &&
            packet.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket.PrimeSeedStateReceipt.SeedState ==
            PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var revalidationConsistent =
            packet.PostAdmissionParticipationAssessment.RevalidationConsistent &&
            packet.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.DomainEligibilityAssessment.RevalidationConsistent;
        var occupancyAuthorized =
            packet.DomainOccupancyAssessment.OccupancyAuthorized &&
            packet.PostAdmissionParticipationAssessment.OccupancyAuthorized &&
            packet.PostAdmissionParticipationReceipt.OccupancyAuthorized;
        var participationAuthorized =
            packet.RoleParticipationAssessment.RoleParticipationAuthorized &&
            packet.PostAdmissionParticipationAssessment.RoleParticipationAuthorized &&
            packet.PostAdmissionParticipationReceipt.RoleParticipationAuthorized;
        var attributionPreserved =
            packet.DomainOccupancyAssessment.AttributionPreserved &&
            packet.DomainAdmissionRoleBindingPacket.DomainAdmissionAssessment.BurdenAttributableAtDomainScope;
        var serviceScopeLawful =
            occupancyAuthorized &&
            !packet.RoleParticipationAssessment.ScopeExpansionDetected;
        var serviceBehaviorAuthorized =
            packetComplete &&
            standingConsistent &&
            revalidationConsistent &&
            attributionPreserved &&
            serviceScopeLawful &&
            (occupancyAuthorized || participationAuthorized);

        var roleBearingExecutionRequested = packet.RoleParticipationAssessment.RoleBound;
        var explicitScopePreserved =
            !packet.RoleParticipationAssessment.ScopeExpansionDetected &&
            serviceScopeLawful;
        var executionStructurePresent =
            packet.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket.SeparationReceipt.PrimeMaterialCount > 2;
        var executionAuthorized =
            packetComplete &&
            standingConsistent &&
            revalidationConsistent &&
            serviceBehaviorAuthorized &&
            executionStructurePresent &&
            explicitScopePreserved &&
            (!roleBearingExecutionRequested || participationAuthorized);

        var serviceBehaviorAssessment = new GovernedSeedServiceBehaviorAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            OccupancyAuthorized: occupancyAuthorized,
            ParticipationAuthorized: participationAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ServiceScopeLawful: serviceScopeLawful,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            Summary: serviceBehaviorAuthorized
                ? "Participation packet supports lawful service behavior."
                : "Participation packet does not yet support lawful service behavior.");

        var executionAuthorizationAssessment = new GovernedSeedExecutionAuthorizationAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            OccupancyAuthorized: occupancyAuthorized,
            ParticipationAuthorized: participationAuthorized,
            ExecutionStructurePresent: executionStructurePresent,
            ExplicitScopePreserved: explicitScopePreserved,
            RoleBearingExecutionRequested: roleBearingExecutionRequested,
            ExecutionAuthorized: executionAuthorized,
            Summary: executionAuthorized
                ? "Participation packet supports lawful execution."
                : "Participation packet does not yet support lawful execution.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            standingConsistent,
            revalidationConsistent,
            serviceBehaviorAuthorized,
            executionAuthorized,
            participationAuthorized,
            executionStructurePresent,
            explicitScopePreserved);

        var unifiedAssessment = new GovernedSeedPostParticipationExecutionAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedPostParticipationExecutionReceipt(
            ReceiptHandle: $"post-participation-execution://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            Summary: unifiedAssessment.Summary);

        return new GovernedSeedPostParticipationExecutionResult(
            serviceBehaviorAssessment,
            executionAuthorizationAssessment,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedExecutionAuthorizationDisposition DetermineDisposition(
        GovernedSeedPostAdmissionParticipationPacket packet,
        bool packetComplete,
        bool standingConsistent,
        bool revalidationConsistent,
        bool serviceBehaviorAuthorized,
        bool executionAuthorized,
        bool participationAuthorized,
        bool executionStructurePresent,
        bool explicitScopePreserved)
    {
        if (!packetComplete ||
            !standingConsistent ||
            !revalidationConsistent)
        {
            return GovernedSeedExecutionAuthorizationDisposition.Refuse;
        }

        if (!packet.DomainOccupancyAssessment.OccupancyAuthorized &&
            !packet.RoleParticipationAssessment.RoleParticipationAuthorized)
        {
            return packet.PostAdmissionParticipationReceipt.Disposition ==
                   GovernedSeedPostAdmissionParticipationDisposition.RemainAtAdmissionPacket
                ? GovernedSeedExecutionAuthorizationDisposition.RemainAtParticipationPacket
                : GovernedSeedExecutionAuthorizationDisposition.Refuse;
        }

        if (!explicitScopePreserved)
        {
            return GovernedSeedExecutionAuthorizationDisposition.ReturnToParticipationPending;
        }

        if (executionAuthorized)
        {
            return GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized;
        }

        if (serviceBehaviorAuthorized)
        {
            return executionStructurePresent
                ? GovernedSeedExecutionAuthorizationDisposition.ServiceBehaviorAuthorized
                : GovernedSeedExecutionAuthorizationDisposition.ExecutionPending;
        }

        return participationAuthorized
            ? GovernedSeedExecutionAuthorizationDisposition.ReturnToParticipationPending
            : GovernedSeedExecutionAuthorizationDisposition.ExecutionPending;
    }

    private static bool PacketIsComplete(GovernedSeedPostAdmissionParticipationPacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.DomainAdmissionRoleBindingPacket.PacketHandle) &&
               HasValue(packet.DomainOccupancyAssessment.PacketHandle) &&
               HasValue(packet.RoleParticipationAssessment.PacketHandle) &&
               HasValue(packet.PostAdmissionParticipationAssessment.PacketHandle) &&
               HasValue(packet.PostAdmissionParticipationReceipt.PacketHandle) &&
               HasValue(packet.PostAdmissionParticipationReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.DomainAdmissionRoleBindingPacket.CandidateId &&
               packet.CandidateId == packet.DomainOccupancyAssessment.CandidateId &&
               packet.CandidateId == packet.RoleParticipationAssessment.CandidateId &&
               packet.CandidateId == packet.PostAdmissionParticipationAssessment.CandidateId &&
               packet.CandidateId == packet.PostAdmissionParticipationReceipt.CandidateId &&
               packet.DomainAdmissionRoleBindingPacket.PacketHandle == packet.DomainOccupancyAssessment.PacketHandle &&
               packet.DomainAdmissionRoleBindingPacket.PacketHandle == packet.RoleParticipationAssessment.PacketHandle &&
               packet.DomainAdmissionRoleBindingPacket.PacketHandle == packet.PostAdmissionParticipationAssessment.PacketHandle &&
               packet.DomainAdmissionRoleBindingPacket.PacketHandle == packet.PostAdmissionParticipationReceipt.PacketHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedExecutionAuthorizationDisposition disposition) =>
        disposition switch
        {
            GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized =>
                "Participation packet may enter lawful execution.",
            GovernedSeedExecutionAuthorizationDisposition.ServiceBehaviorAuthorized =>
                "Participation packet may expose lawful service behavior without full execution authorization.",
            GovernedSeedExecutionAuthorizationDisposition.ExecutionPending =>
                "Participation packet remains participation-authorized but execution-ready conditions are still pending.",
            GovernedSeedExecutionAuthorizationDisposition.ReturnToParticipationPending =>
                "Participation packet must return to participation-pending before execution may proceed.",
            GovernedSeedExecutionAuthorizationDisposition.RemainAtParticipationPacket =>
                "Participation packet remains at the participation layer without execution authorization.",
            _ =>
                "Participation packet must be refused for post-participation execution."
        };
}
