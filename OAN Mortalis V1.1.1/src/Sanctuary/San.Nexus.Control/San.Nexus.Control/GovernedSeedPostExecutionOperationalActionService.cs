using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedPostExecutionOperationalActionService
{
    GovernedSeedPostExecutionOperationalActionResult Evaluate(
        GovernedSeedPostParticipationExecutionPacket packet);
}

public sealed record GovernedSeedPostExecutionOperationalActionResult(
    GovernedSeedServiceEffectAssessment ServiceEffectAssessment,
    GovernedSeedCommitIntent CommitIntent,
    GovernedSeedOperationalActionCommitAssessment CommitAssessment,
    GovernedSeedCommitReceipt CommitReceipt,
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
        var executionAuthorized =
            packet.ExecutionAuthorizationAssessment.ExecutionAuthorized &&
            packet.PostParticipationExecutionAssessment.ExecutionAuthorized &&
            packet.PostParticipationExecutionReceipt.ExecutionAuthorized;
        var serviceBehaviorAuthorized =
            packet.ServiceBehaviorAssessment.ServiceBehaviorAuthorized &&
            packet.PostParticipationExecutionAssessment.ServiceBehaviorAuthorized &&
            packet.PostParticipationExecutionReceipt.ServiceBehaviorAuthorized;
        var standingConsistent =
            packet.ServiceBehaviorAssessment.StandingConsistent &&
            packet.PostParticipationExecutionAssessment.StandingConsistent;
        var revalidationConsistent =
            packet.ServiceBehaviorAssessment.RevalidationConsistent &&
            packet.PostParticipationExecutionAssessment.RevalidationConsistent;
        var attributionPreserved = packet.ServiceBehaviorAssessment.AttributionPreserved;
        var explicitScopePreserved =
            packet.ExecutionAuthorizationAssessment.ExplicitScopePreserved &&
            packet.ServiceBehaviorAssessment.ServiceScopeLawful;
        var serviceEffectAuthorized =
            packetComplete &&
            standingConsistent &&
            revalidationConsistent &&
            attributionPreserved &&
            explicitScopePreserved &&
            serviceBehaviorAuthorized;

        var explicitCommitRequested =
            executionAuthorized &&
            packet.ExecutionAuthorizationAssessment.ExecutionStructurePresent;
        var irreversibleEffectRequested =
            packet.ExecutionAuthorizationAssessment.RoleBearingExecutionRequested;
        var propagationRequested = false;
        var commitIntentPresent = explicitCommitRequested;
        var commitReady =
            packetComplete &&
            executionAuthorized &&
            serviceEffectAuthorized &&
            standingConsistent &&
            revalidationConsistent &&
            attributionPreserved &&
            explicitScopePreserved &&
            commitIntentPresent &&
            !irreversibleEffectRequested &&
            !propagationRequested;
        var operationalActionCommitted = commitReady;

        var serviceEffectAssessment = new GovernedSeedServiceEffectAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            ExecutionAuthorized: executionAuthorized,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ExplicitScopePreserved: explicitScopePreserved,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            Summary: serviceEffectAuthorized
                ? "Execution packet supports lawful externalized service effect."
                : "Execution packet does not yet support lawful externalized service effect.");

        var commitIntent = new GovernedSeedCommitIntent(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            ExecutionAuthorized: executionAuthorized,
            ExplicitCommitRequested: explicitCommitRequested,
            IrreversibleEffectRequested: irreversibleEffectRequested,
            PropagationRequested: propagationRequested,
            CommitIntentPresent: commitIntentPresent,
            Summary: commitIntentPresent
                ? "Execution packet carries explicit operational-action commit intent."
                : "Execution packet does not yet carry explicit operational-action commit intent.");

        var commitAssessment = new GovernedSeedOperationalActionCommitAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            ExecutionAuthorized: executionAuthorized,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ExplicitScopePreserved: explicitScopePreserved,
            ExplicitCommitRequested: explicitCommitRequested,
            CommitReady: commitReady,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: operationalActionCommitted
                ? "Execution packet supports committed operational action."
                : "Execution packet does not yet support committed operational action.");

        var commitReceipt = new GovernedSeedCommitReceipt(
            ReceiptHandle: $"post-execution-commit://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            CommitReady: commitReady,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: commitAssessment.Summary);

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            standingConsistent,
            revalidationConsistent,
            serviceEffectAuthorized,
            executionAuthorized,
            explicitScopePreserved,
            commitIntentPresent,
            operationalActionCommitted);

        var unifiedAssessment = new GovernedSeedPostExecutionOperationalActionAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
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
            commitIntent,
            commitAssessment,
            commitReceipt,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedOperationalActionDisposition DetermineDisposition(
        GovernedSeedPostParticipationExecutionPacket packet,
        bool packetComplete,
        bool standingConsistent,
        bool revalidationConsistent,
        bool serviceEffectAuthorized,
        bool executionAuthorized,
        bool explicitScopePreserved,
        bool commitIntentPresent,
        bool operationalActionCommitted)
    {
        if (!packetComplete ||
            !standingConsistent ||
            !revalidationConsistent)
        {
            return GovernedSeedOperationalActionDisposition.Refuse;
        }

        if (!packet.ExecutionAuthorizationAssessment.ExplicitScopePreserved ||
            !packet.ServiceBehaviorAssessment.ServiceScopeLawful)
        {
            return GovernedSeedOperationalActionDisposition.ReturnToExecutionPending;
        }

        if (operationalActionCommitted)
        {
            return GovernedSeedOperationalActionDisposition.OperationalActionCommitted;
        }

        if (serviceEffectAuthorized && !executionAuthorized)
        {
            return GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized;
        }

        if (executionAuthorized && (!commitIntentPresent || !operationalActionCommitted))
        {
            return GovernedSeedOperationalActionDisposition.OperationalActionPending;
        }

        return packet.PostParticipationExecutionReceipt.Disposition ==
               GovernedSeedExecutionAuthorizationDisposition.RemainAtParticipationPacket
            ? GovernedSeedOperationalActionDisposition.RemainAtExecutionPacket
            : GovernedSeedOperationalActionDisposition.Refuse;
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
            GovernedSeedOperationalActionDisposition.OperationalActionPending =>
                "Execution packet remains execution-authorized but commit-ready conditions are still pending.",
            GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized =>
                "Execution packet may externalize bounded service effect without committed operational action.",
            GovernedSeedOperationalActionDisposition.ReturnToExecutionPending =>
                "Execution packet must return to execution-pending before operational action may proceed.",
            GovernedSeedOperationalActionDisposition.RemainAtExecutionPacket =>
                "Execution packet remains at the execution layer without operational action.",
            _ =>
                "Execution packet must be refused for post-execution operational action."
        };
}
