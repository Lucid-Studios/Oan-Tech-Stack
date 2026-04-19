using San.Common;

namespace San.Nexus.Control;

public interface IGovernedSeedPostActionServiceEnactmentService
{
    GovernedSeedPostActionServiceEnactmentResult Evaluate(
        GovernedSeedPostExecutionOperationalActionPacket packet);
}

public sealed record GovernedSeedPostActionServiceEnactmentResult(
    GovernedSeedEffectEmissionAssessment EffectEmissionAssessment,
    GovernedSeedServiceEnactmentCommitAssessment ServiceEnactmentCommitAssessment,
    GovernedSeedPostActionServiceEnactmentAssessment UnifiedAssessment,
    GovernedSeedPostActionServiceEnactmentReceipt Receipt);

public sealed class GovernedSeedPostActionServiceEnactmentService
    : IGovernedSeedPostActionServiceEnactmentService
{
    public GovernedSeedPostActionServiceEnactmentResult Evaluate(
        GovernedSeedPostExecutionOperationalActionPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var packetComplete = PacketIsComplete(packet);
        var operationalActionCommitted =
            packet.OperationalActionCommitAssessment.OperationalActionCommitted &&
            packet.PostExecutionOperationalActionAssessment.OperationalActionCommitted &&
            packet.PostExecutionOperationalActionReceipt.OperationalActionCommitted;
        var serviceEffectAuthorized =
            packet.ServiceEffectAssessment.ServiceEffectAuthorized &&
            packet.PostExecutionOperationalActionAssessment.ServiceEffectAuthorized &&
            packet.PostExecutionOperationalActionReceipt.ServiceEffectAuthorized;
        var standingConsistent =
            packet.ServiceEffectAssessment.StandingConsistent &&
            packet.OperationalActionCommitAssessment.StandingConsistent;
        var revalidationConsistent =
            packet.ServiceEffectAssessment.RevalidationConsistent &&
            packet.OperationalActionCommitAssessment.RevalidationConsistent;
        var attributionPreserved =
            packet.ServiceEffectAssessment.AttributionPreserved &&
            packet.OperationalActionCommitAssessment.AttributionPreserved;
        var explicitScopePreserved =
            packet.ServiceEffectAssessment.ExplicitScopePreserved &&
            packet.OperationalActionCommitAssessment.ExplicitScopePreserved;
        var effectEmissionAuthorized =
            packetComplete &&
            serviceEffectAuthorized &&
            standingConsistent &&
            revalidationConsistent &&
            attributionPreserved &&
            explicitScopePreserved;
        var enactmentCommitReady =
            packetComplete &&
            operationalActionCommitted &&
            effectEmissionAuthorized &&
            packet.CommitIntent.CommitIntentPresent &&
            !packet.CommitIntent.PropagationRequested;
        var serviceEnactmentCommitted =
            enactmentCommitReady &&
            !packet.CommitIntent.IrreversibleEffectRequested;

        var effectEmissionAssessment = new GovernedSeedEffectEmissionAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            OperationalActionCommitted: operationalActionCommitted,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ExplicitScopePreserved: explicitScopePreserved,
            EffectEmissionAuthorized: effectEmissionAuthorized,
            Summary: effectEmissionAuthorized
                ? "Operational-action packet supports bounded effect emission."
                : "Operational-action packet does not yet support bounded effect emission.");

        var serviceEnactmentCommitAssessment = new GovernedSeedServiceEnactmentCommitAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            PacketComplete: packetComplete,
            OperationalActionCommitted: operationalActionCommitted,
            EffectEmissionAuthorized: effectEmissionAuthorized,
            StandingConsistent: standingConsistent,
            RevalidationConsistent: revalidationConsistent,
            AttributionPreserved: attributionPreserved,
            ExplicitScopePreserved: explicitScopePreserved,
            EnactmentCommitReady: enactmentCommitReady,
            ServiceEnactmentCommitted: serviceEnactmentCommitted,
            Summary: serviceEnactmentCommitted
                ? "Operational-action packet supports committed service enactment."
                : "Operational-action packet does not yet support committed service enactment.");

        var disposition = DetermineDisposition(
            packet,
            packetComplete,
            standingConsistent,
            revalidationConsistent,
            attributionPreserved,
            explicitScopePreserved,
            effectEmissionAuthorized,
            operationalActionCommitted,
            serviceEnactmentCommitted);

        var unifiedAssessment = new GovernedSeedPostActionServiceEnactmentAssessment(
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            PacketComplete: packetComplete,
            EffectEmissionAuthorized: effectEmissionAuthorized,
            ServiceEnactmentCommitted: serviceEnactmentCommitted,
            Summary: BuildSummary(disposition));

        var receipt = new GovernedSeedPostActionServiceEnactmentReceipt(
            ReceiptHandle: $"post-action-service-enactment://{packet.CandidateId}",
            PacketHandle: packet.PacketHandle,
            CandidateId: packet.CandidateId,
            Disposition: disposition,
            EffectEmissionAuthorized: effectEmissionAuthorized,
            ServiceEnactmentCommitted: serviceEnactmentCommitted,
            Summary: unifiedAssessment.Summary);

        return new GovernedSeedPostActionServiceEnactmentResult(
            effectEmissionAssessment,
            serviceEnactmentCommitAssessment,
            unifiedAssessment,
            receipt);
    }

    private static GovernedSeedServiceEnactmentDisposition DetermineDisposition(
        GovernedSeedPostExecutionOperationalActionPacket packet,
        bool packetComplete,
        bool standingConsistent,
        bool revalidationConsistent,
        bool attributionPreserved,
        bool explicitScopePreserved,
        bool effectEmissionAuthorized,
        bool operationalActionCommitted,
        bool serviceEnactmentCommitted)
    {
        if (!packetComplete ||
            !standingConsistent ||
            !revalidationConsistent ||
            !attributionPreserved)
        {
            return GovernedSeedServiceEnactmentDisposition.Refuse;
        }

        if (!explicitScopePreserved)
        {
            return GovernedSeedServiceEnactmentDisposition.ReturnToOperationalActionPending;
        }

        if (serviceEnactmentCommitted)
        {
            return GovernedSeedServiceEnactmentDisposition.ServiceEnactmentCommitted;
        }

        if (effectEmissionAuthorized && !operationalActionCommitted)
        {
            return GovernedSeedServiceEnactmentDisposition.EffectEmissionAuthorized;
        }

        if (operationalActionCommitted && !serviceEnactmentCommitted)
        {
            return GovernedSeedServiceEnactmentDisposition.ServiceEnactmentPending;
        }

        return packet.PostExecutionOperationalActionReceipt.Disposition switch
        {
            GovernedSeedOperationalActionDisposition.RemainAtExecutionPacket =>
                GovernedSeedServiceEnactmentDisposition.RemainAtOperationalActionPacket,
            GovernedSeedOperationalActionDisposition.ReturnToExecutionPending =>
                GovernedSeedServiceEnactmentDisposition.ReturnToOperationalActionPending,
            GovernedSeedOperationalActionDisposition.OperationalActionPending =>
                GovernedSeedServiceEnactmentDisposition.ServiceEnactmentPending,
            _ => GovernedSeedServiceEnactmentDisposition.Refuse
        };
    }

    private static bool PacketIsComplete(GovernedSeedPostExecutionOperationalActionPacket packet)
    {
        return HasValue(packet.PacketHandle) &&
               HasValue(packet.CandidateId) &&
               HasValue(packet.PostParticipationExecutionPacket.PacketHandle) &&
               HasValue(packet.ServiceEffectAssessment.PacketHandle) &&
               HasValue(packet.CommitIntent.PacketHandle) &&
               HasValue(packet.OperationalActionCommitAssessment.PacketHandle) &&
               HasValue(packet.CommitReceipt.PacketHandle) &&
               HasValue(packet.CommitReceipt.ReceiptHandle) &&
               HasValue(packet.PostExecutionOperationalActionAssessment.PacketHandle) &&
               HasValue(packet.PostExecutionOperationalActionReceipt.PacketHandle) &&
               HasValue(packet.PostExecutionOperationalActionReceipt.ReceiptHandle) &&
               packet.CandidateId == packet.PostParticipationExecutionPacket.CandidateId &&
               packet.CandidateId == packet.ServiceEffectAssessment.CandidateId &&
               packet.CandidateId == packet.CommitIntent.CandidateId &&
               packet.CandidateId == packet.OperationalActionCommitAssessment.CandidateId &&
               packet.CandidateId == packet.CommitReceipt.CandidateId &&
               packet.CandidateId == packet.PostExecutionOperationalActionAssessment.CandidateId &&
               packet.CandidateId == packet.PostExecutionOperationalActionReceipt.CandidateId &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.ServiceEffectAssessment.PacketHandle &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.CommitIntent.PacketHandle &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.OperationalActionCommitAssessment.PacketHandle &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.CommitReceipt.PacketHandle &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.PostExecutionOperationalActionAssessment.PacketHandle &&
               packet.PostParticipationExecutionPacket.PacketHandle == packet.PostExecutionOperationalActionReceipt.PacketHandle;
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static string BuildSummary(
        GovernedSeedServiceEnactmentDisposition disposition) =>
        disposition switch
        {
            GovernedSeedServiceEnactmentDisposition.ServiceEnactmentCommitted =>
                "Operational-action packet may enact lawful service consequence.",
            GovernedSeedServiceEnactmentDisposition.ServiceEnactmentPending =>
                "Operational-action packet remains committed but service enactment conditions are still pending.",
            GovernedSeedServiceEnactmentDisposition.EffectEmissionAuthorized =>
                "Operational-action packet may emit bounded effect without full service enactment.",
            GovernedSeedServiceEnactmentDisposition.ReturnToOperationalActionPending =>
                "Operational-action packet must return to operational-action-pending before service enactment may proceed.",
            GovernedSeedServiceEnactmentDisposition.RemainAtOperationalActionPacket =>
                "Operational-action packet remains at the operational-action layer without service enactment.",
            _ =>
                "Operational-action packet must be refused for post-action service enactment."
        };
}

