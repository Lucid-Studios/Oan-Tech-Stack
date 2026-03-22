using System.Security.Cryptography;
using System.Text;

namespace AgentiCore.Runtime;

public sealed record ReachDuplexRealizationEnvelope(
    string EnvelopeId,
    string UtilitySurfaceHandle,
    string DuplexEnvelopeId,
    string SourceLocality,
    string TargetLocality,
    string BondedSpaceHandle,
    string AccessTopologyState,
    string LegibilityState,
    string WitnessHandle,
    string ReturnCondition,
    string AuthorityClass);

public sealed record ReachDuplexRealizationDispatchReceipt(
    string ReceiptId,
    string EnvelopeId,
    string PacketHandle,
    DateTimeOffset TimestampUtc,
    string DispatchState,
    string ReasonCode,
    string Packet,
    string BridgeResponse);

public static class ReachDuplexRealizationSurfaceContracts
{
    public static ReachDuplexRealizationEnvelope CreateEnvelope(
        string utilitySurfaceHandle,
        string duplexEnvelopeId,
        string sourceLocality,
        string targetLocality,
        string bondedSpaceHandle,
        string accessTopologyState,
        string legibilityState,
        string witnessHandle,
        string returnCondition,
        string authorityClass)
    {
        RequireNonEmpty(utilitySurfaceHandle, nameof(utilitySurfaceHandle));
        RequireNonEmpty(duplexEnvelopeId, nameof(duplexEnvelopeId));
        RequireNonEmpty(sourceLocality, nameof(sourceLocality));
        RequireNonEmpty(targetLocality, nameof(targetLocality));
        RequireNonEmpty(bondedSpaceHandle, nameof(bondedSpaceHandle));
        RequireNonEmpty(accessTopologyState, nameof(accessTopologyState));
        RequireNonEmpty(legibilityState, nameof(legibilityState));
        RequireNonEmpty(witnessHandle, nameof(witnessHandle));
        RequireNonEmpty(returnCondition, nameof(returnCondition));
        RequireNonEmpty(authorityClass, nameof(authorityClass));

        var envelopeId = CreateDeterministicHandle(
            "reach-duplex-envelope://",
            utilitySurfaceHandle,
            duplexEnvelopeId,
            sourceLocality,
            targetLocality,
            bondedSpaceHandle,
            accessTopologyState,
            legibilityState,
            witnessHandle,
            returnCondition,
            authorityClass);

        return new ReachDuplexRealizationEnvelope(
            EnvelopeId: envelopeId,
            UtilitySurfaceHandle: utilitySurfaceHandle.Trim(),
            DuplexEnvelopeId: duplexEnvelopeId.Trim(),
            SourceLocality: sourceLocality.Trim(),
            TargetLocality: targetLocality.Trim(),
            BondedSpaceHandle: bondedSpaceHandle.Trim(),
            AccessTopologyState: accessTopologyState.Trim(),
            LegibilityState: legibilityState.Trim(),
            WitnessHandle: witnessHandle.Trim(),
            ReturnCondition: returnCondition.Trim(),
            AuthorityClass: authorityClass.Trim());
    }

    public static string CreatePacket(ReachDuplexRealizationEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ValidateEnvelope(envelope);

        return $"(packet :env runtime :frame reach :mode duplex :op realization-envelope :envelope \"{EscapeSliString(envelope.EnvelopeId)}\" :utility-surface \"{EscapeSliString(envelope.UtilitySurfaceHandle)}\" :duplex-envelope \"{EscapeSliString(envelope.DuplexEnvelopeId)}\" :source-locality \"{EscapeSliString(envelope.SourceLocality)}\" :target-locality \"{EscapeSliString(envelope.TargetLocality)}\" :bonded-space \"{EscapeSliString(envelope.BondedSpaceHandle)}\" :access-topology \"{EscapeSliString(envelope.AccessTopologyState)}\" :legibility \"{EscapeSliString(envelope.LegibilityState)}\" :witness \"{EscapeSliString(envelope.WitnessHandle)}\" :return \"{EscapeSliString(envelope.ReturnCondition)}\" :authority \"{EscapeSliString(envelope.AuthorityClass)}\")";
    }

    public static ReachDuplexRealizationDispatchReceipt CreateDispatchReceipt(
        ReachDuplexRealizationEnvelope envelope,
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
            "accepted" => "reach-duplex-dispatch-accepted",
            "rejected" => "reach-duplex-dispatch-rejected",
            _ => "reach-duplex-dispatch-unclassified"
        };

        var packetHandle = CreateDeterministicHandle("reach-duplex-packet://", envelope.EnvelopeId, packet);
        var receiptId = CreateDeterministicHandle(
            "reach-duplex-receipt://",
            envelope.EnvelopeId,
            packetHandle,
            dispatchState,
            reasonCode,
            timestampUtc.ToUniversalTime().ToString("O"));

        return new ReachDuplexRealizationDispatchReceipt(
            ReceiptId: receiptId,
            EnvelopeId: envelope.EnvelopeId,
            PacketHandle: packetHandle,
            TimestampUtc: timestampUtc.ToUniversalTime(),
            DispatchState: dispatchState,
            ReasonCode: reasonCode,
            Packet: packet,
            BridgeResponse: bridgeResponse);
    }

    public static void ValidateEnvelope(ReachDuplexRealizationEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        RequireNonEmpty(envelope.EnvelopeId, nameof(envelope.EnvelopeId));
        RequireNonEmpty(envelope.UtilitySurfaceHandle, nameof(envelope.UtilitySurfaceHandle));
        RequireNonEmpty(envelope.DuplexEnvelopeId, nameof(envelope.DuplexEnvelopeId));
        RequireNonEmpty(envelope.SourceLocality, nameof(envelope.SourceLocality));
        RequireNonEmpty(envelope.TargetLocality, nameof(envelope.TargetLocality));
        RequireNonEmpty(envelope.BondedSpaceHandle, nameof(envelope.BondedSpaceHandle));
        RequireNonEmpty(envelope.AccessTopologyState, nameof(envelope.AccessTopologyState));
        RequireNonEmpty(envelope.LegibilityState, nameof(envelope.LegibilityState));
        RequireNonEmpty(envelope.WitnessHandle, nameof(envelope.WitnessHandle));
        RequireNonEmpty(envelope.ReturnCondition, nameof(envelope.ReturnCondition));
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
