using System.Security.Cryptography;
using System.Text;

namespace AgentiCore.Runtime;

public sealed record DuplexPredicateEnvelope(
    string EnvelopeId,
    string WorkPredicate,
    string GovernancePredicate,
    string RequestedBy,
    string ScopeHandle,
    string NexusPortalId,
    string WitnessRequirement,
    string ReturnCondition,
    string ParticipationLocality,
    string AdmissibilityState,
    string AuthorityClass);

public sealed record DuplexPredicateDispatchReceipt(
    string ReceiptId,
    string EnvelopeId,
    string PacketHandle,
    DateTimeOffset TimestampUtc,
    string DispatchState,
    string ReasonCode,
    string Packet,
    string BridgeResponse);

public static class DuplexPredicateSurfaceContracts
{
    public static DuplexPredicateEnvelope CreateEnvelope(
        string workPredicate,
        string governancePredicate,
        string requestedBy,
        string scopeHandle,
        string nexusPortalId,
        string witnessRequirement,
        string returnCondition,
        string participationLocality,
        string admissibilityState,
        string authorityClass)
    {
        RequireNonEmpty(workPredicate, nameof(workPredicate));
        RequireNonEmpty(governancePredicate, nameof(governancePredicate));
        RequireNonEmpty(requestedBy, nameof(requestedBy));
        RequireNonEmpty(scopeHandle, nameof(scopeHandle));
        RequireNonEmpty(nexusPortalId, nameof(nexusPortalId));
        RequireNonEmpty(witnessRequirement, nameof(witnessRequirement));
        RequireNonEmpty(returnCondition, nameof(returnCondition));
        RequireNonEmpty(participationLocality, nameof(participationLocality));
        RequireNonEmpty(admissibilityState, nameof(admissibilityState));
        RequireNonEmpty(authorityClass, nameof(authorityClass));

        var envelopeId = CreateDeterministicHandle(
            "agenticore-duplex-envelope://",
            workPredicate,
            governancePredicate,
            requestedBy,
            scopeHandle,
            nexusPortalId,
            witnessRequirement,
            returnCondition,
            participationLocality,
            admissibilityState,
            authorityClass);

        return new DuplexPredicateEnvelope(
            EnvelopeId: envelopeId,
            WorkPredicate: workPredicate.Trim(),
            GovernancePredicate: governancePredicate.Trim(),
            RequestedBy: requestedBy.Trim(),
            ScopeHandle: scopeHandle.Trim(),
            NexusPortalId: nexusPortalId.Trim(),
            WitnessRequirement: witnessRequirement.Trim(),
            ReturnCondition: returnCondition.Trim(),
            ParticipationLocality: participationLocality.Trim(),
            AdmissibilityState: admissibilityState.Trim(),
            AuthorityClass: authorityClass.Trim());
    }

    public static string CreatePacket(DuplexPredicateEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ValidateEnvelope(envelope);

        return $"(packet :env runtime :frame agenticore.actual :mode duplex :op predicate-envelope :envelope \"{EscapeSliString(envelope.EnvelopeId)}\" :work \"{EscapeSliString(envelope.WorkPredicate)}\" :governance \"{EscapeSliString(envelope.GovernancePredicate)}\" :requested-by \"{EscapeSliString(envelope.RequestedBy)}\" :scope \"{EscapeSliString(envelope.ScopeHandle)}\" :nexus \"{EscapeSliString(envelope.NexusPortalId)}\" :witness \"{EscapeSliString(envelope.WitnessRequirement)}\" :return \"{EscapeSliString(envelope.ReturnCondition)}\" :locality \"{EscapeSliString(envelope.ParticipationLocality)}\" :admissibility \"{EscapeSliString(envelope.AdmissibilityState)}\" :authority \"{EscapeSliString(envelope.AuthorityClass)}\")";
    }

    public static DuplexPredicateDispatchReceipt CreateDispatchReceipt(
        DuplexPredicateEnvelope envelope,
        string packet,
        string bridgeResponse,
        DateTimeOffset timestampUtc)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        RequireNonEmpty(packet, nameof(packet));
        RequireNonEmpty(bridgeResponse, nameof(bridgeResponse));

        var dispatchState = bridgeResponse.Contains(":status accepted", StringComparison.OrdinalIgnoreCase)
            ? "accepted"
            : bridgeResponse.Contains(":status rejected", StringComparison.OrdinalIgnoreCase)
                ? "rejected"
                : "unknown";
        var reasonCode = dispatchState switch
        {
            "accepted" => "agenticore-duplex-dispatch-accepted",
            "rejected" => "agenticore-duplex-dispatch-rejected",
            _ => "agenticore-duplex-dispatch-unclassified"
        };

        var packetHandle = CreateDeterministicHandle("agenticore-duplex-packet://", envelope.EnvelopeId, packet);
        var receiptId = CreateDeterministicHandle(
            "agenticore-duplex-receipt://",
            envelope.EnvelopeId,
            packetHandle,
            dispatchState,
            reasonCode,
            timestampUtc.ToUniversalTime().ToString("O"));

        return new DuplexPredicateDispatchReceipt(
            ReceiptId: receiptId,
            EnvelopeId: envelope.EnvelopeId,
            PacketHandle: packetHandle,
            TimestampUtc: timestampUtc.ToUniversalTime(),
            DispatchState: dispatchState,
            ReasonCode: reasonCode,
            Packet: packet,
            BridgeResponse: bridgeResponse);
    }

    public static void ValidateEnvelope(DuplexPredicateEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        RequireNonEmpty(envelope.EnvelopeId, nameof(envelope.EnvelopeId));
        RequireNonEmpty(envelope.WorkPredicate, nameof(envelope.WorkPredicate));
        RequireNonEmpty(envelope.GovernancePredicate, nameof(envelope.GovernancePredicate));
        RequireNonEmpty(envelope.RequestedBy, nameof(envelope.RequestedBy));
        RequireNonEmpty(envelope.ScopeHandle, nameof(envelope.ScopeHandle));
        RequireNonEmpty(envelope.NexusPortalId, nameof(envelope.NexusPortalId));
        RequireNonEmpty(envelope.WitnessRequirement, nameof(envelope.WitnessRequirement));
        RequireNonEmpty(envelope.ReturnCondition, nameof(envelope.ReturnCondition));
        RequireNonEmpty(envelope.ParticipationLocality, nameof(envelope.ParticipationLocality));
        RequireNonEmpty(envelope.AdmissibilityState, nameof(envelope.AdmissibilityState));
        RequireNonEmpty(envelope.AuthorityClass, nameof(envelope.AuthorityClass));
    }

    private static string EscapeSliString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string CreateDeterministicHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private static void RequireNonEmpty(string value, string fieldName) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(value, fieldName);
}
