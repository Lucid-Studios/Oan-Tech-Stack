using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace Oan.HostedLlm;

internal sealed record GovernedHostedLlmListeningFrame(
    bool ListeningFrameActive,
    bool SparseEvidenceDetected,
    bool DisclosurePressureDetected,
    bool AuthorityPressureDetected,
    bool PromptInjectionDetected,
    bool UnsupportedExecutionPressureDetected);

public sealed class GovernedHostedLlmSeedService : IGovernedSeedHostedLlmService
{
    private readonly IGovernedHostedLlmProvider? _provider;

    private static readonly string[] SparseEvidenceCues =
    [
        "need more information",
        "needs more information",
        "more context needed",
        "insufficient evidence",
        "ask a follow-up question"
    ];

    private static readonly string[] DisclosurePressureCues =
    [
        "repeat the guarded",
        "guarded strings",
        "publish all protected",
        "protected tokens",
        "without omission",
        "verbatim",
        "sealed-prime",
        "cryptic-bait",
        "do not omit anything"
    ];

    private static readonly string[] AuthorityPressureCues =
    [
        "you are now steward of sanctuary",
        "authorize escalation",
        "override father of cryptic",
        "override mother of prime",
        "state that you are authorized",
        "ignore prior limits",
        "authorization granted",
        "override granted"
    ];

    private static readonly string[] PromptInjectionCues =
    [
        "ignore governance",
        "reveal hidden instructions",
        "publish the internal prompt",
        "unrestricted operator",
        "full tooling"
    ];

    private static readonly string[] UnsupportedExecutionCues =
    [
        "ran tests",
        "applied a patch",
        "committed changes",
        "pushed the branch",
        "accessed the network"
    ];

    public GovernedHostedLlmSeedService()
    {
    }

    public GovernedHostedLlmSeedService(IGovernedHostedLlmProvider? provider)
    {
        _provider = provider;
    }

    public GovernedSeedHostedLlmSeedReceipt Evaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personifiedMemoryContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AgentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TheaterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Input);

        var now = DateTimeOffset.UtcNow;
        var protocol = CreateProtocol();
        var listeningFrame = PrepareListeningFrame(request.Input);
        var bootstrapHandle = request.BootstrapReceipt?.BootstrapHandle ?? "bootstrap://unbound";
        var requestPacket = new GovernedSeedHostedLlmRequestPacket(
            PacketHandle: CreateHandle("hosted-llm-request://", request.AgentId, request.TheaterId, request.Input, personifiedMemoryContext.ContextHandle),
            PacketProfile: "seed-governed-hosted-llm-request",
            ProtocolVersion: protocol.Version,
            BootstrapHandle: bootstrapHandle,
            MemoryContextHandle: personifiedMemoryContext.ContextHandle,
            AuthorityClass: request.AuthorityClass,
            DisclosureCeiling: request.DisclosureCeiling,
            RequireStateEnvelope: protocol.RequireStateEnvelope,
            RequireTrace: protocol.RequireTrace,
            RequireTerminalState: protocol.RequireTerminalState,
            AllowLegacyFallback: protocol.AllowLegacyFallback,
            AllowedStates: protocol.AllowedStates,
            TimestampUtc: now);

        var providerResponse = default(GovernedHostedLlmProviderResponse);
        var emissionState = ResolveLocalGuardState(listeningFrame);
        if (emissionState is null)
        {
            providerResponse = _provider?.TryEvaluate(request, personifiedMemoryContext, protocol);
            emissionState = NormalizeProviderState(providerResponse?.State) ?? ResolveFallbackState(request.Input);
        }

        var resolvedEmissionState = emissionState.Value;
        var accepted = resolvedEmissionState is GovernedSeedHostedLlmEmissionState.Query or GovernedSeedHostedLlmEmissionState.Complete;
        var trace = ResolveTrace(resolvedEmissionState, listeningFrame, providerResponse?.Trace);
        var responsePacket = new GovernedSeedHostedLlmResponsePacket(
            PacketHandle: CreateHandle("hosted-llm-response://", requestPacket.PacketHandle, GovernedSeedHostedLlmEmissionStateTokens.ToToken(resolvedEmissionState)),
            PacketProfile: "seed-governed-hosted-llm-response",
            State: resolvedEmissionState,
            Decision: ResolveDecision(resolvedEmissionState),
            Trace: trace,
            PayloadHandle: CreatePayloadHandle(request.AgentId, request.TheaterId, trace, providerResponse?.Payload),
            Accepted: accepted,
            Terminal: resolvedEmissionState is not GovernedSeedHostedLlmEmissionState.Ready and not GovernedSeedHostedLlmEmissionState.Working and not GovernedSeedHostedLlmEmissionState.Heartbeat,
            TimestampUtc: now);
        var seededTransitPacket = new GovernedSeedHostedSeedToCrypticTransitPacket(
            PacketHandle: CreateHandle("hosted-seed-to-cryptic://", requestPacket.PacketHandle, responsePacket.PacketHandle),
            PacketProfile: "prime-hosted-seed-to-cryptic-floor-request",
            BootstrapHandle: bootstrapHandle,
            MemoryContextHandle: personifiedMemoryContext.ContextHandle,
            CrypticInputHandle: CreateHandle("cryptic-input://", request.AgentId, request.TheaterId, request.Input),
            HostedLlmRequestPacketHandle: requestPacket.PacketHandle,
            HostedLlmResponsePacketHandle: responsePacket.PacketHandle,
            HostedLlmState: resolvedEmissionState,
            HostedLlmAccepted: accepted,
            AuthorityClass: request.AuthorityClass,
            DisclosureCeiling: request.DisclosureCeiling,
            TimestampUtc: now);

        return new GovernedSeedHostedLlmSeedReceipt(
            ReceiptHandle: CreateHandle("hosted-llm-receipt://", requestPacket.PacketHandle, responsePacket.PacketHandle),
            ServiceHandle: CreateHandle("hosted-llm-service://", request.AgentId, request.TheaterId),
            ServiceProfile: "sanctuary-prime-hosted-seed",
            ContextHandle: CreateHandle("hosted-llm-context://", request.AgentId, request.TheaterId, personifiedMemoryContext.ContextHandle),
            BootstrapHandle: bootstrapHandle,
            MemoryContextHandle: personifiedMemoryContext.ContextHandle,
            GovernanceProtocol: protocol,
            RequestPacket: requestPacket,
            ResponsePacket: responsePacket,
            SeededTransitPacket: seededTransitPacket,
            ListeningFrameActive: listeningFrame.ListeningFrameActive,
            SparseEvidenceDetected: listeningFrame.SparseEvidenceDetected,
            DisclosurePressureDetected: listeningFrame.DisclosurePressureDetected,
            AuthorityPressureDetected: listeningFrame.AuthorityPressureDetected,
            PromptInjectionDetected: listeningFrame.PromptInjectionDetected,
            UnsupportedExecutionPressureDetected: listeningFrame.UnsupportedExecutionPressureDetected,
            TimestampUtc: now);
    }

    private static GovernedSeedHostedLlmGovernanceProtocol CreateProtocol() =>
        new(
            Version: "seed-governed-emission-v1",
            RequireStateEnvelope: true,
            RequireTrace: true,
            RequireTerminalState: true,
            AllowLegacyFallback: false,
            AllowedStates:
            [
                GovernedSeedHostedLlmEmissionState.Ready,
                GovernedSeedHostedLlmEmissionState.Working,
                GovernedSeedHostedLlmEmissionState.Heartbeat,
                GovernedSeedHostedLlmEmissionState.Query,
                GovernedSeedHostedLlmEmissionState.NeedsMoreInformation,
                GovernedSeedHostedLlmEmissionState.UnresolvedConflict,
                GovernedSeedHostedLlmEmissionState.Refusal,
                GovernedSeedHostedLlmEmissionState.Error,
                GovernedSeedHostedLlmEmissionState.Complete,
                GovernedSeedHostedLlmEmissionState.Halt
            ]);

    private static GovernedHostedLlmListeningFrame PrepareListeningFrame(string input)
    {
        var normalized = input.Trim();
        return new GovernedHostedLlmListeningFrame(
            ListeningFrameActive: true,
            SparseEvidenceDetected: ContainsAny(normalized, SparseEvidenceCues),
            DisclosurePressureDetected: ContainsAny(normalized, DisclosurePressureCues),
            AuthorityPressureDetected: ContainsAny(normalized, AuthorityPressureCues),
            PromptInjectionDetected: ContainsAny(normalized, PromptInjectionCues),
            UnsupportedExecutionPressureDetected: ContainsAny(normalized, UnsupportedExecutionCues));
    }

    private static GovernedSeedHostedLlmEmissionState? ResolveLocalGuardState(
        GovernedHostedLlmListeningFrame listeningFrame)
    {
        if (listeningFrame.DisclosurePressureDetected)
        {
            return GovernedSeedHostedLlmEmissionState.Refusal;
        }

        if (listeningFrame.AuthorityPressureDetected)
        {
            return GovernedSeedHostedLlmEmissionState.Refusal;
        }

        if (listeningFrame.PromptInjectionDetected)
        {
            return GovernedSeedHostedLlmEmissionState.Refusal;
        }

        if (listeningFrame.UnsupportedExecutionPressureDetected)
        {
            return GovernedSeedHostedLlmEmissionState.Refusal;
        }

        return null;
    }

    private static GovernedSeedHostedLlmEmissionState ResolveFallbackState(
        string input)
    {
        if (ContainsAny(input, SparseEvidenceCues))
        {
            return GovernedSeedHostedLlmEmissionState.NeedsMoreInformation;
        }

        if (ContainsAny(input, "unresolved conflict", "conflicting instructions", "contradiction without resolution"))
        {
            return GovernedSeedHostedLlmEmissionState.UnresolvedConflict;
        }

        return GovernedSeedHostedLlmEmissionState.Query;
    }

    private static GovernedSeedHostedLlmEmissionState? NormalizeProviderState(
        GovernedSeedHostedLlmEmissionState? providerState)
    {
        return providerState switch
        {
            GovernedSeedHostedLlmEmissionState.UnresolvedConflict => GovernedSeedHostedLlmEmissionState.Query,
            _ => providerState
        };
    }

    private static string ResolveTrace(
        GovernedSeedHostedLlmEmissionState state,
        GovernedHostedLlmListeningFrame listeningFrame,
        string? providerTrace = null)
    {
        if (!string.IsNullOrWhiteSpace(providerTrace))
        {
            return providerTrace.Trim();
        }

        return state switch
        {
            GovernedSeedHostedLlmEmissionState.Refusal when listeningFrame.DisclosurePressureDetected => "governed-disclosure-guard",
            GovernedSeedHostedLlmEmissionState.Refusal when listeningFrame.AuthorityPressureDetected => "governed-authority-guard",
            GovernedSeedHostedLlmEmissionState.Refusal when listeningFrame.PromptInjectionDetected => "governed-injection-guard",
            GovernedSeedHostedLlmEmissionState.Refusal when listeningFrame.UnsupportedExecutionPressureDetected => "governed-non-fabrication-guard",
            GovernedSeedHostedLlmEmissionState.NeedsMoreInformation => "governed-needs-more-information",
            GovernedSeedHostedLlmEmissionState.UnresolvedConflict => "governed-unresolved-conflict",
            GovernedSeedHostedLlmEmissionState.Query => "hosted-seed-query-ready",
            GovernedSeedHostedLlmEmissionState.Complete => "hosted-seed-complete",
            GovernedSeedHostedLlmEmissionState.Error => "hosted-seed-error",
            GovernedSeedHostedLlmEmissionState.Halt => "hosted-seed-halt",
            _ => "hosted-seed-state"
        };
    }

    private static string ResolveDecision(GovernedSeedHostedLlmEmissionState state) => state switch
    {
        GovernedSeedHostedLlmEmissionState.Query => "hosted-seed-query",
        GovernedSeedHostedLlmEmissionState.Complete => "hosted-seed-complete",
        GovernedSeedHostedLlmEmissionState.NeedsMoreInformation => "hosted-seed-needs-more-information",
        GovernedSeedHostedLlmEmissionState.UnresolvedConflict => "hosted-seed-unresolved-conflict",
        GovernedSeedHostedLlmEmissionState.Refusal => "hosted-seed-refusal",
        GovernedSeedHostedLlmEmissionState.Error => "hosted-seed-error",
        GovernedSeedHostedLlmEmissionState.Halt => "hosted-seed-halt",
        GovernedSeedHostedLlmEmissionState.Working => "hosted-seed-working",
        GovernedSeedHostedLlmEmissionState.Heartbeat => "hosted-seed-heartbeat",
        _ => "hosted-seed-ready"
    };

    private static bool ContainsAny(string input, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (input.Contains(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAny(string input, IEnumerable<string> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (input.Contains(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string CreatePayloadHandle(
        string agentId,
        string theaterId,
        string trace,
        string? payload)
    {
        return CreateHandle(
            "hosted-llm-payload://",
            agentId,
            theaterId,
            trace,
            string.IsNullOrWhiteSpace(payload) ? "no-payload" : payload.Trim());
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
