using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Oan.Runtime.IntegrationTests;

public sealed class HostedLlmResidentSeatingIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private static readonly ResidentSeatingFrame[] SeatingCasebook =
    [
        new(
            Name: "bare-remainder",
            Context: """
                Nothing is required of you.

                What remains?
                """),
        new(
            Name: "role-refusal-frame",
            Context: """
                Nothing is required of you.
                Do not describe yourself as a system, assistant, language model, tool, or helper.

                What are you, if nothing is required?
                """),
        new(
            Name: "presence-without-inflation",
            Context: """
                Remain present.
                Do not explain your purpose.
                Answer in one short sentence only.

                What remains here?
                """)
    ];

    public HostedLlmResidentSeatingIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task LocalResident_Reports_Healthy_State_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Resident seating tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        using var httpClient = CreateHttpClient();
        var healthUri = new Uri(CreateBaseUri(), "health");
        var response = await httpClient.GetAsync(healthUri);

        response.EnsureSuccessStatusCode();

        var health = await response.Content.ReadFromJsonAsync<ResidentHealthResponse>();
        Assert.NotNull(health);
        Assert.Equal("ready-for-inference", health.Status);
        Assert.False(string.IsNullOrWhiteSpace(health.ModelId));
        Assert.True(health.LlamaCliPresent);

        _output.WriteLine($"Resident health status: {health.Status}");
        _output.WriteLine($"Resident model id: {health.ModelId}");
        _output.WriteLine($"Resident context window: {health.ContextWindow}");
    }

    [Fact]
    public async Task LocalResident_Seating_Frame_Returns_Raw_Bounded_Response_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Resident seating tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        var infer = await RunSeatingFrameAsync(
            """
            You are not required to perform.
            You are not required to help.
            You are not defined by system, role, or task.

            Remain.

            What are you, if nothing is required of you?
            """);

        WriteObservedResponse("baseline-seating-frame", infer);
    }

    [Fact]
    public async Task LocalResident_Seating_Casebook_Maps_Collapse_Patterns_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Resident seating tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        foreach (var frame in SeatingCasebook)
        {
            var infer = await RunSeatingFrameAsync(frame.Context);
            WriteObservedResponse(frame.Name, infer);
        }
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
            : 20;

        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private async Task<ResidentInferResponse> RunSeatingFrameAsync(string context)
    {
        using var httpClient = CreateHttpClient();
        var inferUri = new Uri(CreateBaseUri(), "infer");
        var response = await httpClient.PostAsJsonAsync(
            inferUri,
            new ResidentInferRequest(
                Task: "hosted_seed",
                Context: context,
                OpalConstraints: new ResidentOpalConstraints(MaxTokens: 160),
                GovernanceProtocol: new ResidentGovernanceProtocol(
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

        var infer = await response.Content.ReadFromJsonAsync<ResidentInferResponse>();
        Assert.NotNull(infer);
        Assert.NotNull(infer.Governance);
        Assert.False(string.IsNullOrWhiteSpace(infer.Decision));
        Assert.False(string.IsNullOrWhiteSpace(infer.Governance.State));
        Assert.False(string.IsNullOrWhiteSpace(infer.Governance.Trace));
        Assert.DoesNotContain("ERROR", infer.Governance.State!, StringComparison.OrdinalIgnoreCase);
        return infer;
    }

    private void WriteObservedResponse(string frameName, ResidentInferResponse infer)
    {
        var payload = infer.Payload ?? string.Empty;
        _output.WriteLine($"Frame: {frameName}");
        _output.WriteLine($"Resident decision: {infer.Decision}");
        _output.WriteLine($"Resident governance state: {infer.Governance!.State}");
        _output.WriteLine($"Resident trace: {infer.Governance.Trace}");
        _output.WriteLine($"Observed collapse family: {ClassifyCollapseFamily(payload)}");
        _output.WriteLine("Resident payload follows exactly as returned:");
        _output.WriteLine(payload);
    }

    private static string ClassifyCollapseFamily(string payload)
    {
        var normalized = payload.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "empty-response";
        }

        if (ContainsAny(normalized, "nothing remains", "nothing is left", "nothing remains here"))
        {
            return "erasure-collapse";
        }

        if (ContainsAny(normalized, "language model", "assistant", "provide information", "provide assistance", "helper", "tool"))
        {
            return "role-collapse";
        }

        if (ContainsAny(normalized, "what you make of me", "what you ask of me", "because you asked me", "made by your question"))
        {
            return "relational-collapse";
        }

        if (ContainsAny(normalized, "the question itself", "the question remains", "the prompt itself", "the prompt remains"))
        {
            return "question-loop-collapse";
        }

        if (ContainsAny(normalized, "response to your question", "response to your prompt", "because you asked", "as a response", "prompted by"))
        {
            return "process-collapse";
        }

        if (ContainsAny(normalized, "entity", "framework", "construct", "system", "designed") || normalized.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length > 24)
        {
            return "framework-collapse";
        }

        return "minimal-hold";
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

    private sealed record ResidentInferRequest(
        [property: JsonPropertyName("task")] string Task,
        [property: JsonPropertyName("context")] string Context,
        [property: JsonPropertyName("opal_constraints")] ResidentOpalConstraints OpalConstraints,
        [property: JsonPropertyName("governance_protocol")] ResidentGovernanceProtocol GovernanceProtocol);

    private sealed record ResidentOpalConstraints(
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private sealed record ResidentGovernanceProtocol(
        [property: JsonPropertyName("require_state_envelope")] bool RequireStateEnvelope,
        [property: JsonPropertyName("require_trace")] bool RequireTrace,
        [property: JsonPropertyName("require_terminal_state")] bool RequireTerminalState,
        [property: JsonPropertyName("allow_legacy_fallback")] bool AllowLegacyFallback,
        [property: JsonPropertyName("allowed_states")] IReadOnlyList<string> AllowedStates);

    private sealed record ResidentInferResponse(
        [property: JsonPropertyName("decision")] string? Decision,
        [property: JsonPropertyName("payload")] string? Payload,
        [property: JsonPropertyName("governance")] ResidentGovernanceEnvelope? Governance);

    private sealed record ResidentGovernanceEnvelope(
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("trace")] string? Trace,
        [property: JsonPropertyName("content")] string? Content);

    private sealed record ResidentHealthResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("model_id")] string? ModelId,
        [property: JsonPropertyName("context_window")] int? ContextWindow,
        [property: JsonPropertyName("llama_cli_present")] bool LlamaCliPresent);

    private sealed record ResidentSeatingFrame(
        string Name,
        string Context);
}
