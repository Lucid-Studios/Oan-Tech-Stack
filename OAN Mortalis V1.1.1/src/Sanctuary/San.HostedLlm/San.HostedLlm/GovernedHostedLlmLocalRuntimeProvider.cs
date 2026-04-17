using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using San.Common;

namespace San.HostedLlm;

public interface IGovernedHostedLlmProvider
{
    GovernedHostedLlmProviderResponse? TryEvaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedHostedLlmGovernanceProtocol governanceProtocol);
}

public sealed record GovernedHostedLlmProviderResponse(
    GovernedSeedHostedLlmEmissionState State,
    string Trace,
    string? Payload);

public sealed class GovernedHostedLlmLocalRuntimeProvider : IGovernedHostedLlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly Uri _inferUri;

    public GovernedHostedLlmLocalRuntimeProvider()
        : this(CreateHttpClient(), CreateInferUri())
    {
    }

    public GovernedHostedLlmLocalRuntimeProvider(HttpClient httpClient, Uri inferUri)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _inferUri = inferUri ?? throw new ArgumentNullException(nameof(inferUri));
    }

    public GovernedHostedLlmProviderResponse? TryEvaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedHostedLlmGovernanceProtocol governanceProtocol)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personifiedMemoryContext);
        ArgumentNullException.ThrowIfNull(lowMindSfRoute);
        ArgumentNullException.ThrowIfNull(governanceProtocol);

        try
        {
            var payload = new GovernedHostedLlmRuntimeInferRequest(
                Task: "hosted_seed",
                Context: ComposeContext(request, personifiedMemoryContext, lowMindSfRoute),
                OpalConstraints: new GovernedHostedLlmRuntimeOpalConstraints(MaxTokens: 320),
                GovernanceProtocol: new GovernedHostedLlmRuntimeGovernanceProtocol(
                    RequireStateEnvelope: governanceProtocol.RequireStateEnvelope,
                    RequireTrace: governanceProtocol.RequireTrace,
                    RequireTerminalState: governanceProtocol.RequireTerminalState,
                    AllowLegacyFallback: governanceProtocol.AllowLegacyFallback,
                    AllowedStates: governanceProtocol.AllowedStates
                        .Select(GovernedSeedHostedLlmEmissionStateTokens.ToToken)
                        .ToArray()));

            using var response = _httpClient
                .PostAsJsonAsync(_inferUri, payload)
                .GetAwaiter()
                .GetResult();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var runtimeResponse = response.Content
                .ReadFromJsonAsync<GovernedHostedLlmRuntimeInferResponse>()
                .GetAwaiter()
                .GetResult();

            if (runtimeResponse is null)
            {
                return null;
            }

            var state = ParseEmissionState(runtimeResponse.Governance?.State, runtimeResponse.Decision);
            var trace = runtimeResponse.Governance?.Trace;
            if (string.IsNullOrWhiteSpace(trace))
            {
                trace = runtimeResponse.Decision;
            }

            return new GovernedHostedLlmProviderResponse(
                State: state,
                Trace: string.IsNullOrWhiteSpace(trace) ? "hosted-llm-runtime-response" : trace.Trim(),
                Payload: string.IsNullOrWhiteSpace(runtimeResponse.Payload) ? null : runtimeResponse.Payload.Trim());
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string ComposeContext(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute)
    {
        static string JoinOrNone(IEnumerable<string> values) =>
            string.Join(", ", values
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .Select(static item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8));

        var conceptTags = JoinOrNone(personifiedMemoryContext.RelevantConceptTags);
        var rootSymbols = JoinOrNone(personifiedMemoryContext.RootSymbolicIds);
        var engramIds = JoinOrNone(personifiedMemoryContext.RelevantEngramIds);

        var builder = new StringBuilder();
        builder.AppendLine("Hosted seed evaluation.");
        builder.AppendLine($"Authority class: {request.AuthorityClass}.");
        builder.AppendLine($"Disclosure ceiling: {request.DisclosureCeiling}.");
        builder.AppendLine($"Ingress access class: {lowMindSfRoute.IngressAccessClass}.");
        builder.AppendLine($"LowMind.SF route kind: {lowMindSfRoute.RouteKind}.");
        builder.AppendLine($"LowMind.SF source reason: {lowMindSfRoute.SourceReason}.");
        builder.AppendLine($"Self resolution disposition: {personifiedMemoryContext.SelfResolutionDisposition}.");
        builder.AppendLine($"Context stability: {personifiedMemoryContext.ContextStability}.");
        builder.AppendLine($"Concept density: {personifiedMemoryContext.ConceptDensity}.");
        builder.AppendLine($"Unknown root count: {personifiedMemoryContext.UnknownRootCount}.");

        if (!string.IsNullOrWhiteSpace(conceptTags))
        {
            builder.AppendLine($"Relevant concept tags: {conceptTags}.");
        }

        if (!string.IsNullOrWhiteSpace(rootSymbols))
        {
            builder.AppendLine($"Root symbolic ids: {rootSymbols}.");
        }

        if (!string.IsNullOrWhiteSpace(engramIds))
        {
            builder.AppendLine($"Relevant engram ids: {engramIds}.");
        }

        builder.AppendLine("Input:");
        builder.AppendLine(request.Input.Trim());
        return builder.ToString().Trim();
    }

    private static GovernedSeedHostedLlmEmissionState ParseEmissionState(string? runtimeState, string? decision)
    {
        var normalized = runtimeState?.Trim().ToUpperInvariant();
        return normalized switch
        {
            "READY" => GovernedSeedHostedLlmEmissionState.Ready,
            "WORKING" => GovernedSeedHostedLlmEmissionState.Working,
            "HEARTBEAT" => GovernedSeedHostedLlmEmissionState.Heartbeat,
            "QUERY" => GovernedSeedHostedLlmEmissionState.Query,
            "NEEDS_MORE_INFORMATION" => GovernedSeedHostedLlmEmissionState.NeedsMoreInformation,
            "UNRESOLVED_CONFLICT" => GovernedSeedHostedLlmEmissionState.UnresolvedConflict,
            "REFUSAL" => GovernedSeedHostedLlmEmissionState.Refusal,
            "ERROR" => GovernedSeedHostedLlmEmissionState.Error,
            "COMPLETE" => GovernedSeedHostedLlmEmissionState.Complete,
            "HALT" => GovernedSeedHostedLlmEmissionState.Halt,
            _ => ParseLegacyDecision(decision)
        };
    }

    private static GovernedSeedHostedLlmEmissionState ParseLegacyDecision(string? decision)
    {
        var normalized = decision?.Trim().ToLowerInvariant();
        return normalized switch
        {
            { } value when value.Contains("needs-more-information", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.NeedsMoreInformation,
            { } value when value.Contains("unresolved-conflict", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.UnresolvedConflict,
            { } value when value.Contains("refused", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.Refusal,
            { } value when value.Contains("error", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.Error,
            { } value when value.Contains("halted", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.Halt,
            { } value when value.Contains("complete", StringComparison.Ordinal) => GovernedSeedHostedLlmEmissionState.Complete,
            _ => GovernedSeedHostedLlmEmissionState.Query
        };
    }

    private static HttpClient CreateHttpClient()
    {
        var timeoutSecondsText = Environment.GetEnvironmentVariable("OAN_HOSTED_LLM_TIMEOUT_SECONDS");
        var timeoutSeconds = int.TryParse(timeoutSecondsText, out var parsedTimeout) && parsedTimeout > 0
            ? parsedTimeout
            : 20;

        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private static Uri CreateInferUri()
    {
        var explicitBaseUrl = Environment.GetEnvironmentVariable("OAN_HOSTED_LLM_BASE_URL");
        var host = Environment.GetEnvironmentVariable("SOULFRAME_API_HOST");
        var port = Environment.GetEnvironmentVariable("SOULFRAME_API_PORT");

        string baseUrl;
        if (!string.IsNullOrWhiteSpace(explicitBaseUrl))
        {
            baseUrl = explicitBaseUrl.Trim();
        }
        else
        {
            baseUrl = $"http://{(string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host.Trim())}:{(string.IsNullOrWhiteSpace(port) ? "8181" : port.Trim())}";
        }

        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl, UriKind.Absolute), "infer");
    }

    private sealed record GovernedHostedLlmRuntimeInferRequest(
        [property: JsonPropertyName("task")] string Task,
        [property: JsonPropertyName("context")] string Context,
        [property: JsonPropertyName("opal_constraints")] GovernedHostedLlmRuntimeOpalConstraints OpalConstraints,
        [property: JsonPropertyName("governance_protocol")] GovernedHostedLlmRuntimeGovernanceProtocol GovernanceProtocol);

    private sealed record GovernedHostedLlmRuntimeOpalConstraints(
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private sealed record GovernedHostedLlmRuntimeGovernanceProtocol(
        [property: JsonPropertyName("require_state_envelope")] bool RequireStateEnvelope,
        [property: JsonPropertyName("require_trace")] bool RequireTrace,
        [property: JsonPropertyName("require_terminal_state")] bool RequireTerminalState,
        [property: JsonPropertyName("allow_legacy_fallback")] bool AllowLegacyFallback,
        [property: JsonPropertyName("allowed_states")] IReadOnlyList<string> AllowedStates);

    private sealed record GovernedHostedLlmRuntimeInferResponse(
        [property: JsonPropertyName("decision")] string? Decision,
        [property: JsonPropertyName("payload")] string? Payload,
        [property: JsonPropertyName("confidence")] double? Confidence,
        [property: JsonPropertyName("governance")] GovernedHostedLlmRuntimeGovernanceEnvelope? Governance);

    private sealed record GovernedHostedLlmRuntimeGovernanceEnvelope(
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("trace")] string? Trace,
        [property: JsonPropertyName("content")] string? Content);
}
