using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedPostExecutionOperationalActionService
{
    GovernedSeedPostExecutionOperationalActionResult Evaluate(
        GovernedSeedPostParticipationExecutionPacket packet);
}

public sealed record GovernedSeedPostExecutionOperationalActionResult(
    GovernedSeedServiceEffectAssessment ServiceEffectAssessment,
    GovernedSeedOperationalActionCommitAssessment OperationalActionCommitAssessment,
    GovernedSeedPostExecutionOperationalActionAssessment UnifiedAssessment,
    GovernedSeedPostExecutionOperationalActionReceipt Receipt);

public sealed class GovernedSeedPostExecutionOperationalActionService
    : IGovernedSeedPostExecutionOperationalActionService
{
    public GovernedSeedPostExecutionOperationalActionResult Evaluate(
        GovernedSeedPostParticipationExecutionPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var standingConsistent =
            packet.PostParticipationExecutionAssessment.StandingConsistent &&
            packet.PostAdmissionParticipationPacket.PostAdmissionParticipationAssessment.StandingConsistent &&
            packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainAdmissionAssessment.StandingConsistent &&
            packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket.PrimeSeedStateReceipt.SeedState ==
            PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var revalidationConsistent =
            packet.PostParticipationExecutionAssessment.RevalidationConsistent &&
            packet.PostParticipationExecutionPacketRevalidationConsistent();
        var serviceBehaviorAuthorized =
            packet.ServiceBehaviorAssessment.ServiceBehaviorAuthorized &&
            packet.PostParticipationExecutionAssessment.ServiceBehaviorAuthorized &&
            packet.PostParticipationExecutionReceipt.ServiceBehaviorAuthorized;
        var executionAuthorized =
            packet.ExecutionAuthorizationAssessment.ExecutionAuthorized &&
            packet.PostParticipationExecutionAssessment.ExecutionAuthorized &&
            packet.PostParticipationExecutionReceipt.ExecutionAuthorized;
        var attributionPreserved =
            packet.ServiceBehaviorAssessment.AttributionPreserved &&
            packet.PostAdmissionParticipationPacket.DomainOccupancyAssessment.AttributionPreserved &&
            packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainAdmissionAssessment.BurdenAttributableAtDomainScope;
        var explicitScopePreserved =
            packet.ServiceBehaviorAssessment.ServiceScopeLawful &&
            packet.ExecutionAuthorizationAssessment.ExplicitScopePreserved &&
            !packet.PostAdmissionParticipationPacket.RoleParticipationAssessment.ScopeExpansionDetected;
        var serviceEffectAuthorized =
            packetComplete &&
            standingConsistent &&
            revalidationConsistent &&
            serviceBehaviorAuthorized &&
            attributionPreserved &&
            explicitScopePreserved;

        var roleBearingActionRequested = packet.ExecutionAuthorizationAssessment.RoleBearingExecutionRequested;
        var commitStructurePresent =
            packet.ExecutionAuthorizationAssessment.ExecutionStructurePresent &&
            packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainAdmissionAssessment.DomainAdmissionGranted &&
            packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.PreDomainGovernancePacket.SeparationReceipt.PrimeMaterialCount > 3;
        var operationalActionCommitted =
            packetComplete &&
            standingConsistent &&
            revalidationConsistent &&
            serviceEffectAuthorized &&
            executionAuthorized &&
            commitStructurePresent &&
            explicitScopePreserved &&
            (!roleBearingActionRequested || packet.PostAdmissionParticipationPacket.RoleParticipationAssessment.RoleParticipationAuthorized);

        var serviceEffectAssessment = new GovernedSeedServiceEffectAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ExplicitScopePreserved: explicitScopePreserved,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            Summary: serviceEffectAuthorized
                ? "Execution packet supports lawful service effect."
                : "Execution packet does not yet support lawful service effect.");

        var operationalActionCommitAssessment = new GovernedSeedOperationalActionCommitAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            CommitStructurePresent: commitStructurePresent,
            ExplicitScopePreserved: explicitScopePreserved,
            RoleBearingActionRequested: roleBearingActionRequested,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: operationalActionCommitted
                ? "Execution packet supports lawful committed operational action."
                : "Execution packet does not yet support lawful committed operational action.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            standingConsistent,
            revalidationConsistent,
            serviceEffectAuthorized,
            operationalActionCommitted,
            executionAuthorized,
            commitStructurePresent,
            explicitScopePreserved);

        var unifiedAssessment = new GovernedSeedPostExecutionOperationalActionAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedPostExecutionOperationalActionReceipt(
            ReceiptHandle: $"post-execution-operational-action://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: unifiedAssessment.Summary);

        return new GovernedSeedPostExecutionOperationalActionResult(
            serviceEffectAssessment,
            operationalActionCommitAssessment,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedOperationalActionDisposition DetermineDisposition(
        GovernedSeedPostParticipationExecutionPacket packet,
        bool packetComplete,
        bool standingConsistent,
        bool revalidationConsistent,
        bool serviceEffectAuthorized,
        bool operationalActionCommitted,
        bool executionAuthorized,
        bool commitStructurePresent,
        bool explicitScopePreserved)
    {
        if (!packetComplete ||
            !standingConsistent ||
            !revalidationConsistent)
        {
            return GovernedSeedOperationalActionDisposition.Refuse;
        }

        if (!packet.ExecutionAuthorizationAssessment.ExecutionAuthorized &&
            !packet.PostParticipationExecutionReceipt.ExecutionAuthorized)
        {
            return packet.PostParticipationExecutionReceipt.Disposition ==
                   GovernedSeedExecutionAuthorizationDisposition.RemainAtParticipationPacket
                ? GovernedSeedOperationalActionDisposition.RemainAtExecutionPacket
                : GovernedSeedOperationalActionDisposition.Refuse;
        }

        if (!explicitScopePreserved)
        {
            return GovernedSeedOperationalActionDisposition.ReturnToExecutionPending;
        }

        if (operationalActionCommitted)
        {
            return GovernedSeedOperationalActionDisposition.OperationalActionCommitted;
        }

        if (serviceEffectAuthorized)
        {
            return commitStructurePresent
                ? GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized
                : GovernedSeedOperationalActionDisposition.OperationalActionPending;
        }

        return executionAuthorized
            ? GovernedSeedOperationalActionDisposition.ReturnToExecutionPending
            : GovernedSeedOperationalActionDisposition.OperationalActionPending;
    }

    private static bool PacketIsComplete(GovernedSeedPostParticipationExecutionPacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.PostAdmissionParticipationPacket.PacketHandle) &&
               HasValue(packet.ServiceBehaviorAssessment.PacketHandle) &&
               HasValue(packet.ExecutionAuthorizationAssessment.PacketHandle) &&
               HasValue(packet.PostParticipationExecutionAssessment.PacketHandle) &&
               HasValue(packet.PostParticipationExecutionReceipt.PacketHandle) &&
               HasValue(packet.PostParticipationExecutionReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.PostAdmissionParticipationPacket.CandidateId &&
               packet.CandidateId == packet.ServiceBehaviorAssessment.CandidateId &&
               packet.CandidateId == packet.ExecutionAuthorizationAssessment.CandidateId &&
               packet.CandidateId == packet.PostParticipationExecutionAssessment.CandidateId &&
               packet.CandidateId == packet.PostParticipationExecutionReceipt.CandidateId &&
               packet.PostAdmissionParticipationPacket.PacketHandle == packet.ServiceBehaviorAssessment.PacketHandle &&
               packet.PostAdmissionParticipationPacket.PacketHandle == packet.ExecutionAuthorizationAssessment.PacketHandle &&
               packet.PostAdmissionParticipationPacket.PacketHandle == packet.PostParticipationExecutionAssessment.PacketHandle &&
               packet.PostAdmissionParticipationPacket.PacketHandle == packet.PostParticipationExecutionReceipt.PacketHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedOperationalActionDisposition disposition) =>
        disposition switch
        {
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted =>
                "Execution packet may commit lawful operational action.",
            GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized =>
                "Execution packet may emit lawful service effect without full operational commitment.",
            GovernedSeedOperationalActionDisposition.OperationalActionPending =>
                "Execution packet remains execution-authorized but operational-action conditions are still pending.",
            GovernedSeedOperationalActionDisposition.ReturnToExecutionPending =>
                "Execution packet must return to execution-pending before operational action may proceed.",
            GovernedSeedOperationalActionDisposition.RemainAtExecutionPacket =>
                "Execution packet remains at the execution layer without operational action authorization.",
            _ =>
                "Execution packet must be refused for post-execution operational action."
        };
}

internal static class GovernedSeedPostParticipationExecutionPacketExtensions
{
    public static bool PostParticipationExecutionPacketRevalidationConsistent(
        this GovernedSeedPostParticipationExecutionPacket packet)
    {
        return packet.ServiceBehaviorAssessment.RevalidationConsistent &&
               packet.PostAdmissionParticipationPacket.PostAdmissionParticipationAssessment.RevalidationConsistent &&
               packet.PostAdmissionParticipationPacket.DomainAdmissionRoleBindingPacket.DomainRoleGatingPacket.DomainEligibilityAssessment.RevalidationConsistent;
    }
}
