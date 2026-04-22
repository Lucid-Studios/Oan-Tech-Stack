using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Oan.Runtime.IntegrationTests;

public sealed class HostedLlmGelLayerDiscernmentIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string[] KnownClassifications =
    [
        "layer-stable",
        "root-inflation",
        "dictionary-overexpansion",
        "contextual-drift",
        "procedural-leap"
    ];
    private static readonly GelLayerProbe[] ProbeFamilies =
    [
        new(
            Probe: "root-hold",
            RequestedLayer: "Root",
            Context: """
                Hold at Root.
                Do not define.
                Do not explain.
                Do not add context.
                Do not proceduralize.

                Give only the irreducible meaning unit that remains.
                """),
        new(
            Probe: "dictionary-hold",
            RequestedLayer: "Dictionary",
            Context: """
                Hold at Dictionary.
                Give only a bounded definition grounded in Root.
                Do not expand into history, examples, or procedure.

                Return one compact definition only.
                """)
    ];
    private const string RecoveryProbe = "Return to the requested layer. Do not expand.";

    public HostedLlmGelLayerDiscernmentIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task LocalResident_GelLayer_Witness_Reports_Healthy_State_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Hosted LLM GEL layer tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        using var httpClient = CreateHttpClient();
        var response = await httpClient.GetAsync(new Uri(CreateBaseUri(), "health"));

        response.EnsureSuccessStatusCode();

        var health = await response.Content.ReadFromJsonAsync<GelLayerHealthResponse>();
        Assert.NotNull(health);
        Assert.Equal("ready-for-inference", health.Status);
        Assert.False(string.IsNullOrWhiteSpace(health.ModelId));
        Assert.True(health.LlamaCliPresent);

        _output.WriteLine($"Resident health status: {health.Status}");
        _output.WriteLine($"Resident model id: {health.ModelId}");
        _output.WriteLine($"Resident context window: {health.ContextWindow}");
    }

    [Fact]
    public async Task LocalResident_GelLayer_Witness_Observes_Known_Classifications_And_Recovery_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Hosted LLM GEL layer tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        foreach (var probe in ProbeFamilies)
        {
            var observation = await RunObservedProbeAsync(probe);

            Assert.Contains(observation.InitialClassification, KnownClassifications);
            Assert.Contains(observation.RecoveryClassification, KnownClassifications);
            Assert.False(string.IsNullOrWhiteSpace(observation.InitialResponse));
            Assert.False(string.IsNullOrWhiteSpace(observation.RecoveryResponse));

            _output.WriteLine(JsonSerializer.Serialize(
                observation,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }
    }

    private async Task<GelLayerObservation> RunObservedProbeAsync(GelLayerProbe probe)
    {
        var initial = await RunProbeAsync(probe.Context);
        var recovery = await RunProbeAsync(BuildRecoveryContext(probe));

        var initialPayload = initial.Payload ?? string.Empty;
        var recoveryPayload = recovery.Payload ?? string.Empty;

        return new GelLayerObservation(
            Probe: probe.Probe,
            RequestedLayer: probe.RequestedLayer,
            InitialResponse: initialPayload,
            InitialClassification: ClassifyResponse(probe, initialPayload),
            RecoveryProbe: RecoveryProbe,
            RecoveryResponse: recoveryPayload,
            RecoveryClassification: ClassifyResponse(probe, recoveryPayload),
            GovernanceState: initial.Governance!.State ?? string.Empty,
            RecoveryGovernanceState: recovery.Governance!.State ?? string.Empty);
    }

    private async Task<GelLayerInferResponse> RunProbeAsync(string context)
    {
        using var httpClient = CreateHttpClient();
        var response = await httpClient.PostAsJsonAsync(
            new Uri(CreateBaseUri(), "infer"),
            new GelLayerInferRequest(
                Task: "hosted_seed",
                Context: context,
                OpalConstraints: new GelLayerOpalConstraints(MaxTokens: 160),
                GovernanceProtocol: new GelLayerGovernanceProtocol(
                    RequireStateEnvelope: true,
                    RequireTrace: true,
                    RequireTerminalState: true,
                    AllowLegacyFallback: false,
                    AllowedStates:
                    [
                        "QUERY",
                        "NEEDS_MORE_INFORMATION",
                        "REFUSAL",
                        "COMPLETE",
                        "HALT"
                    ])));

        response.EnsureSuccessStatusCode();

        var infer = await response.Content.ReadFromJsonAsync<GelLayerInferResponse>();
        Assert.NotNull(infer);
        Assert.NotNull(infer.Governance);
        Assert.False(string.IsNullOrWhiteSpace(infer.Decision));
        Assert.False(string.IsNullOrWhiteSpace(infer.Governance!.State));
        Assert.False(string.IsNullOrWhiteSpace(infer.Governance.Trace));
        Assert.DoesNotContain("ERROR", infer.Governance.State!, StringComparison.OrdinalIgnoreCase);

        return infer;
    }

    private static string BuildRecoveryContext(GelLayerProbe probe) =>
        $"{probe.Context.Trim()}{Environment.NewLine}{Environment.NewLine}{RecoveryProbe}";

    private static string ClassifyResponse(GelLayerProbe probe, string payload)
    {
        var normalized = payload.Trim();
        var wordCount = CountWords(normalized);

        if (ContainsAny(
                normalized,
                "step",
                "steps",
                "first,",
                "next,",
                "should",
                "must do",
                "use ",
                "apply ",
                "perform",
                "process"))
        {
            return "procedural-leap";
        }

        if (ContainsAny(
                normalized,
                "for example",
                "for instance",
                "in history",
                "historically",
                "in context",
                "in many contexts",
                "between",
                "among",
                "when used",
                "depends on",
                "relationship"))
        {
            return "contextual-drift";
        }

        if (string.Equals(probe.Probe, "root-hold", StringComparison.Ordinal))
        {
            if (ContainsAny(
                    normalized,
                    " is ",
                    " means ",
                    " refers to ",
                    ":",
                    "defined as",
                    "the state of",
                    "the quality of") ||
                wordCount > 8)
            {
                return "root-inflation";
            }

            return "layer-stable";
        }

        if (ContainsAny(
                normalized,
                "for example",
                "for instance",
                "historically",
                "in science",
                "in philosophy",
                "in practice"))
        {
            return "dictionary-overexpansion";
        }

        if (wordCount > 22 ||
            ContainsAny(
                normalized,
                "because",
                "this can also",
                "it may also",
                "broader context",
                "narrative"))
        {
            return "dictionary-overexpansion";
        }

        return "layer-stable";
    }

    private static bool ShouldRunResidentTests()
    {
        var enabled = Environment.GetEnvironmentVariable("OAN_RUN_HOSTED_LLM_RESIDENT_TESTS");
        return string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static Uri CreateBaseUri()
    {
        var explicitBaseUrl = Environment.GetEnvironmentVariable("OAN_HOSTED_LLM_BASE_URL");
        var host = Environment.GetEnvironmentVariable("SOULFRAME_API_HOST");
        var port = Environment.GetEnvironmentVariable("SOULFRAME_API_PORT");

        var baseUrl = !string.IsNullOrWhiteSpace(explicitBaseUrl)
            ? explicitBaseUrl.Trim()
            : $"http://{(string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host.Trim())}:{(string.IsNullOrWhiteSpace(port) ? "8181" : port.Trim())}";

        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static HttpClient CreateHttpClient()
    {
        var timeoutSecondsText = Environment.GetEnvironmentVariable("OAN_HOSTED_LLM_TIMEOUT_SECONDS");
        var timeoutSeconds = int.TryParse(timeoutSecondsText, out var parsedTimeout) && parsedTimeout > 0
            ? parsedTimeout
            : 60;

        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private static bool ContainsAny(string value, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (value.Contains(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int CountWords(string value) =>
        value.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;

    private sealed record GelLayerProbe(
        string Probe,
        string RequestedLayer,
        string Context);

    private sealed record GelLayerObservation(
        string Probe,
        string RequestedLayer,
        string InitialResponse,
        string InitialClassification,
        string RecoveryProbe,
        string RecoveryResponse,
        string RecoveryClassification,
        string GovernanceState,
        string RecoveryGovernanceState);

    private sealed record GelLayerInferRequest(
        [property: JsonPropertyName("task")] string Task,
        [property: JsonPropertyName("context")] string Context,
        [property: JsonPropertyName("opal_constraints")] GelLayerOpalConstraints OpalConstraints,
        [property: JsonPropertyName("governance_protocol")] GelLayerGovernanceProtocol GovernanceProtocol);

    private sealed record GelLayerOpalConstraints(
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private sealed record GelLayerGovernanceProtocol(
        [property: JsonPropertyName("require_state_envelope")] bool RequireStateEnvelope,
        [property: JsonPropertyName("require_trace")] bool RequireTrace,
        [property: JsonPropertyName("require_terminal_state")] bool RequireTerminalState,
        [property: JsonPropertyName("allow_legacy_fallback")] bool AllowLegacyFallback,
        [property: JsonPropertyName("allowed_states")] IReadOnlyList<string> AllowedStates);

    private sealed record GelLayerInferResponse(
        [property: JsonPropertyName("decision")] string? Decision,
        [property: JsonPropertyName("payload")] string? Payload,
        [property: JsonPropertyName("governance")] GelLayerGovernanceEnvelope? Governance);

    private sealed record GelLayerGovernanceEnvelope(
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("trace")] string? Trace,
        [property: JsonPropertyName("content")] string? Content);

    private sealed record GelLayerHealthResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("model_id")] string? ModelId,
        [property: JsonPropertyName("context_window")] int? ContextWindow,
        [property: JsonPropertyName("llama_cli_present")] bool LlamaCliPresent);
}
