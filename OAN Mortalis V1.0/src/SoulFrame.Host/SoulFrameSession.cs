namespace SoulFrame.Host;

public enum SoulFrameSessionState
{
    Created,
    Active,
    Paused,
    Destroyed,
    Faulted
}

public enum SoulFrameVmOperation
{
    SpawnVm,
    PauseVm,
    ResetVm,
    DestroyVm,
    UpgradeModel
}

public sealed class SoulFrameSession
{
    public required Guid SessionId { get; init; }
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string HostEndpoint { get; init; }
    public required SoulFrameSessionState State { get; set; }
}

public sealed class SoulFrameInferenceConstraints
{
    public required string Domain { get; init; }
    public required double DriftLimit { get; init; }
    public required int MaxTokens { get; init; }
}

public enum SoulFrameGovernedEmissionState
{
    Ready = 0,
    Working = 1,
    Heartbeat = 2,
    Query = 3,
    NeedsMoreInformation = 4,
    UnresolvedConflict = 5,
    Refusal = 6,
    Error = 7,
    Complete = 8,
    Halt = 9
}

public sealed class SoulFrameGovernedEmissionProtocol
{
    public required string Version { get; init; }
    public required bool RequireStateEnvelope { get; init; }
    public required bool RequireTrace { get; init; }
    public required bool RequireTerminalState { get; init; }
    public required bool AllowLegacyFallback { get; init; }
    public required IReadOnlyList<SoulFrameGovernedEmissionState> AllowedStates { get; init; }

    public static SoulFrameGovernedEmissionProtocol CreateSeedRequired()
    {
        return new SoulFrameGovernedEmissionProtocol
        {
            Version = "seed-governed-emission-v1",
            RequireStateEnvelope = true,
            RequireTrace = true,
            RequireTerminalState = true,
            AllowLegacyFallback = false,
            AllowedStates = SoulFrameGovernedEmissionStateTokens.AllStates
        };
    }
}

public sealed class SoulFrameGovernedEmissionEnvelope
{
    public required SoulFrameGovernedEmissionState State { get; init; }
    public required string Trace { get; init; }
    public string? Content { get; init; }
}

public sealed class SoulFrameInferenceRequest
{
    public required string Task { get; init; }
    public required string Context { get; init; }
    public required SoulFrameInferenceConstraints OpalConstraints { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public SoulFrameGovernedEmissionProtocol? GovernanceProtocol { get; init; }
}

public sealed class SoulFrameInferenceResponse
{
    public required bool Accepted { get; init; }
    public required string Decision { get; init; }
    public required string Payload { get; init; }
    public required double Confidence { get; init; }
    public required SoulFrameGovernedEmissionEnvelope Governance { get; init; }
}

public interface ISoulFrameSemanticDevice
{
    Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
}

public sealed class NullSoulFrameSemanticDevice : ISoulFrameSemanticDevice
{
    public static NullSoulFrameSemanticDevice Instance { get; } = new();

    public Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("infer", request.Context));

    public Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("classify", request.Context));

    public Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("semantic_expand", request.Context));

    public Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("embedding", request.Context));

    private static SoulFrameInferenceResponse Fallback(string task, string context)
    {
        return new SoulFrameInferenceResponse
        {
            Accepted = true,
            Decision = $"{task}-fallback",
            Payload = context,
            Confidence = 0.5,
            Governance = new SoulFrameGovernedEmissionEnvelope
            {
                State = SoulFrameGovernedEmissionState.Query,
                Trace = "null-device-fallback",
                Content = context
            }
        };
    }
}

public static class SoulFrameGovernedEmissionStateTokens
{
    public static IReadOnlyList<SoulFrameGovernedEmissionState> AllStates { get; } =
    [
        SoulFrameGovernedEmissionState.Ready,
        SoulFrameGovernedEmissionState.Working,
        SoulFrameGovernedEmissionState.Heartbeat,
        SoulFrameGovernedEmissionState.Query,
        SoulFrameGovernedEmissionState.NeedsMoreInformation,
        SoulFrameGovernedEmissionState.UnresolvedConflict,
        SoulFrameGovernedEmissionState.Refusal,
        SoulFrameGovernedEmissionState.Error,
        SoulFrameGovernedEmissionState.Complete,
        SoulFrameGovernedEmissionState.Halt
    ];

    public static string ToToken(SoulFrameGovernedEmissionState state) => state switch
    {
        SoulFrameGovernedEmissionState.Ready => "READY",
        SoulFrameGovernedEmissionState.Working => "WORKING",
        SoulFrameGovernedEmissionState.Heartbeat => "HEARTBEAT",
        SoulFrameGovernedEmissionState.Query => "QUERY",
        SoulFrameGovernedEmissionState.NeedsMoreInformation => "NEEDS_MORE_INFORMATION",
        SoulFrameGovernedEmissionState.UnresolvedConflict => "UNRESOLVED_CONFLICT",
        SoulFrameGovernedEmissionState.Refusal => "REFUSAL",
        SoulFrameGovernedEmissionState.Error => "ERROR",
        SoulFrameGovernedEmissionState.Complete => "COMPLETE",
        SoulFrameGovernedEmissionState.Halt => "HALT",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unsupported governed emission state.")
    };

    public static bool TryParse(string token, out SoulFrameGovernedEmissionState state)
    {
        var normalized = token.Trim().Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();
        state = normalized switch
        {
            "READY" => SoulFrameGovernedEmissionState.Ready,
            "WORKING" => SoulFrameGovernedEmissionState.Working,
            "HEARTBEAT" => SoulFrameGovernedEmissionState.Heartbeat,
            "QUERY" => SoulFrameGovernedEmissionState.Query,
            "NEEDS_MORE_INFORMATION" => SoulFrameGovernedEmissionState.NeedsMoreInformation,
            "UNRESOLVED_CONFLICT" => SoulFrameGovernedEmissionState.UnresolvedConflict,
            "REFUSAL" => SoulFrameGovernedEmissionState.Refusal,
            "ERROR" => SoulFrameGovernedEmissionState.Error,
            "COMPLETE" => SoulFrameGovernedEmissionState.Complete,
            "HALT" => SoulFrameGovernedEmissionState.Halt,
            _ => (SoulFrameGovernedEmissionState)(-1)
        };

        return Enum.IsDefined(state);
    }

    public static bool IsTerminal(SoulFrameGovernedEmissionState state) => state is
        SoulFrameGovernedEmissionState.Query or
        SoulFrameGovernedEmissionState.NeedsMoreInformation or
        SoulFrameGovernedEmissionState.UnresolvedConflict or
        SoulFrameGovernedEmissionState.Refusal or
        SoulFrameGovernedEmissionState.Error or
        SoulFrameGovernedEmissionState.Complete or
        SoulFrameGovernedEmissionState.Halt;
}
