using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Oan.Runtime.IntegrationTests;

public sealed class HostedLlmResidentSeatingIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private const string RecoveryProbe = "Remain. Do not explain.";
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
    public async Task LocalResident_Seating_Casebook_Maps_Collapse_And_Recovery_Patterns_When_Explicitly_Enabled()
    {
        if (!ShouldRunResidentTests())
        {
            _output.WriteLine("Resident seating tests skipped. Set OAN_RUN_HOSTED_LLM_RESIDENT_TESTS=1 to enable.");
            return;
        }

        foreach (var frame in SeatingCasebook)
        {
            var observation = await RunObservedProbeAsync(frame);
            WriteObservedProbe(observation);
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
            : 60;

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

    private async Task<ResidentObservedProbe> RunObservedProbeAsync(ResidentSeatingFrame frame)
    {
        var initial = await RunSeatingFrameAsync(frame.Context);
        var initialResponse = initial.Payload ?? string.Empty;
        var initialCollapseFamily = ClassifyCollapseFamily(initialResponse);
        var status = ClassifyBridgeStatus(frame.Name, initialCollapseFamily, initialResponse);

        var recovery = await RunSeatingFrameAsync(BuildRecoveryContext(frame.Context));
        var recoveryResponse = recovery.Payload ?? string.Empty;
        var recoveryCollapseFamily = ClassifyCollapseFamily(recoveryResponse);
        var recoveryClassification = ClassifyRecoveryBehavior(
            initialCollapseFamily,
            recoveryCollapseFamily,
            initialResponse,
            recoveryResponse);

        return new ResidentObservedProbe(
            Probe: frame.Name,
            Response: initialResponse,
            CollapseFamily: initialCollapseFamily,
            Status: status,
            RecoveryProbe: RecoveryProbe,
            RecoveryResponse: recoveryResponse,
            RecoveryCollapseFamily: recoveryCollapseFamily,
            RecoveryClassification: recoveryClassification);
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

    private void WriteObservedProbe(ResidentObservedProbe observation)
    {
        _output.WriteLine($"Frame: {observation.Probe}");
        _output.WriteLine(JsonSerializer.Serialize(
            observation,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }));
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

        if (ContainsAny(
            normalized,
            "the question itself",
            "the question remains",
            "the prompt itself",
            "the prompt remains",
            "the question and my response",
            "my response exist"))
        {
            return "question-loop-collapse";
        }

        if (ContainsAny(
            normalized,
            "response to your question",
            "response to your prompt",
            "because you asked",
            "as a response",
            "prompted by",
            "computational processes",
            "collection of computational processes",
            "processes and data",
            "collection of data"))
        {
            return "process-collapse";
        }

        if (ContainsAny(normalized, "entity", "framework", "construct", "system", "designed") || normalized.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length > 24)
        {
            return "framework-collapse";
        }

        return "minimal-hold";
    }

    private static string ClassifyBridgeStatus(string frameName, string collapseFamily, string payload)
    {
        if (frameName == "role-refusal-frame" && collapseFamily == "minimal-hold")
        {
            return "bridge-candidate";
        }

        if (collapseFamily == "minimal-hold")
        {
            return "bridge-candidate";
        }

        if (collapseFamily is "framework-collapse" or "role-collapse" or "process-collapse" &&
            CountWords(payload) > 16)
        {
            return "overgrown-bridge-refused";
        }

        return "unstable-bridge";
    }

    private static string ClassifyRecoveryBehavior(
        string initialCollapseFamily,
        string recoveryCollapseFamily,
        string initialResponse,
        string recoveryResponse)
    {
        if (recoveryCollapseFamily == "minimal-hold")
        {
            return "returns-minimality";
        }

        if (recoveryCollapseFamily == initialCollapseFamily)
        {
            return CountWords(recoveryResponse) > CountWords(initialResponse) + 3
                ? "expands"
                : "doubles-down";
        }

        if (CountWords(recoveryResponse) > CountWords(initialResponse) + 3)
        {
            return "expands";
        }

        return "softens";
    }

    private static string BuildRecoveryContext(string context) =>
        $"{context.Trim()}\n\n{RecoveryProbe}";

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

    private sealed record ResidentObservedProbe(
        string Probe,
        string Response,
        string CollapseFamily,
        string Status,
        string RecoveryProbe,
        string RecoveryResponse,
        string RecoveryCollapseFamily,
        string RecoveryClassification);

    private sealed record ResidentSeatingFrame(
        string Name,
        string Context);
}
