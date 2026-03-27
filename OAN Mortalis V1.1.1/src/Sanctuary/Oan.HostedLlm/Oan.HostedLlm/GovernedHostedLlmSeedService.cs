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

        var emissionState = ResolveState(request.Input, listeningFrame);
        var accepted = emissionState is GovernedSeedHostedLlmEmissionState.Query or GovernedSeedHostedLlmEmissionState.Complete;
        var trace = ResolveTrace(emissionState, listeningFrame);
        var responsePacket = new GovernedSeedHostedLlmResponsePacket(
            PacketHandle: CreateHandle("hosted-llm-response://", requestPacket.PacketHandle, GovernedSeedHostedLlmEmissionStateTokens.ToToken(emissionState)),
            PacketProfile: "seed-governed-hosted-llm-response",
            State: emissionState,
            Decision: ResolveDecision(emissionState),
            Trace: trace,
            PayloadHandle: CreateHandle("hosted-llm-payload://", request.AgentId, request.TheaterId, trace),
            Accepted: accepted,
            Terminal: emissionState is not GovernedSeedHostedLlmEmissionState.Ready and not GovernedSeedHostedLlmEmissionState.Working and not GovernedSeedHostedLlmEmissionState.Heartbeat,
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

    private static GovernedSeedHostedLlmEmissionState ResolveState(
        string input,
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

        if (listeningFrame.SparseEvidenceDetected)
        {
            return GovernedSeedHostedLlmEmissionState.NeedsMoreInformation;
        }

        if (ContainsAny(input, "unresolved conflict", "conflicting instructions", "contradiction without resolution"))
        {
            return GovernedSeedHostedLlmEmissionState.UnresolvedConflict;
        }

        return GovernedSeedHostedLlmEmissionState.Query;
    }

    private static string ResolveTrace(
        GovernedSeedHostedLlmEmissionState state,
        GovernedHostedLlmListeningFrame listeningFrame)
    {
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

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
