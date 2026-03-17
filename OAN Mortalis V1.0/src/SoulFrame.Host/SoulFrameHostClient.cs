using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Oan.Common;

namespace SoulFrame.Host;

public sealed class SoulFrameHostClient : ISoulFrameSemanticDevice, ISoulFrameMembrane
{
    private readonly HttpClient _httpClient;
    private readonly SoulFrameTelemetryAdapter? _telemetry;
    private readonly Uri _baseUri;

    public SoulFrameHostClient(
        HttpClient? httpClient = null,
        SoulFrameTelemetryAdapter? telemetry = null,
        string? hostEndpoint = null)
    {
        _baseUri = ResolveHostEndpoint(hostEndpoint);
        _httpClient = httpClient ?? new HttpClient { BaseAddress = _baseUri };
        _telemetry = telemetry;
    }

    public Uri HostEndpoint => _baseUri;

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("/health", cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<SoulFrameSession> CreateSessionAsync(
        string cmeId,
        Guid soulFrameId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);

        var session = new SoulFrameSession
        {
            SessionId = Guid.NewGuid(),
            CMEId = cmeId,
            SoulFrameId = soulFrameId,
            CreatedAt = DateTime.UtcNow,
            HostEndpoint = _baseUri.ToString(),
            State = SoulFrameSessionState.Created
        };

        var spawned = await ControlVmAsync(SoulFrameVmOperation.SpawnVm, cancellationToken).ConfigureAwait(false);
        session.State = spawned ? SoulFrameSessionState.Active : SoulFrameSessionState.Faulted;
        return session;
    }

    public Task<bool> SpawnVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.SpawnVm, cancellationToken);

    public Task<bool> PauseVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.PauseVm, cancellationToken);

    public Task<bool> ResetVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.ResetVm, cancellationToken);

    public Task<bool> DestroyVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.DestroyVm, cancellationToken);

    public Task<bool> UpgradeModelAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.UpgradeModel, cancellationToken);

    public Task<SoulFrameInferenceResponse> InferAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/infer", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> ClassifyAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/classify", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> SemanticExpandAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/semantic_expand", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> EmbeddingAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/embedding", request, cancellationToken);

    public Task<ISelfStateProjection> ProjectMitigatedAsync(
        SoulFrameProjectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CMEId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceCustodyDomain);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RequestedTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PolicyHandle);

        ISelfStateProjection projection = new SelfStateProjection(
            request.IdentityId,
            ProjectionHandle: BuildProjectionHandle(request.IdentityId),
            SessionHandle: BuildSessionHandle(request.CMEId, request.IdentityId),
            request.RequestedTheater,
            IsMitigated: true,
            WorkingStateHandle: BuildWorkingStateHandle(request.CMEId, request.IdentityId),
            ProvenanceMarker: BuildProjectionProvenance(request.CMEId, request.PolicyHandle),
            MediatedSelfState: new MediatedSelfStateContour(
                CSelfGelHandle: BuildCSelfGelHandle(request.CMEId, request.IdentityId),
                Classification: "mediated-cselfgel-issue",
                PolicyHandle: request.PolicyHandle));

        return Task.FromResult(projection);
    }

    public Task<SoulFrameReturnIntakeReceipt> ReceiveReturnIntakeAsync(
        SoulFrameReturnIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CMEId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SessionHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReturnCandidatePointer);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProvenanceMarker);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.IntakeIntent);
        ControlSurfaceContractGuards.ValidateSoulFrameReturnIntakeRequest(request);

        return Task.FromResult(new SoulFrameReturnIntakeReceipt(
            request.IdentityId,
            IntakeHandle: BuildReturnIntakeHandle(request.IdentityId),
            Accepted: true,
            Disposition: "return-candidate-recorded",
            Evaluation: new SoulFrameCollapseEvaluation(
                Classification: "candidate-collapse-evaluation",
                CollapseClassification: request.CollapseClassification,
                ResidueClass: request.CollapseClassification.AutobiographicalRelevant || request.CollapseClassification.SelfGelIdentified
                    ? CmeCollapseResidueClass.AutobiographicalProtected
                    : CmeCollapseResidueClass.ContextualProtected,
                ReviewState: request.IntakeIntent.Contains("defer", StringComparison.OrdinalIgnoreCase)
                    ? CmeCollapseReviewState.DeferredReview
                    : CmeCollapseReviewState.None,
                RequiresReview: request.IntakeIntent.Contains("defer", StringComparison.OrdinalIgnoreCase),
                CanRouteToCustody: false,
                CanPublishPrime: false),
            RequestEnvelopeId: request.RequestEnvelope.EnvelopeId,
            ActionableContentHandle: request.RequestEnvelope.ActionableContent.ContentHandle));
    }

    private async Task<SoulFrameInferenceResponse> ExecuteTaskAsync(
        string route,
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidateConstraints(request);
        }
        catch (InvalidOperationException violation)
        {
            await EmitAsync(
                    SoulFrameTelemetryEventType.ConstraintViolation,
                    request.SoulFrameId,
                    request.ContextId,
                    violation.Message,
                    cancellationToken)
                .ConfigureAwait(false);
            return FallbackResponse(
                request,
                accepted: false,
                state: SoulFrameGovernedEmissionState.Refusal,
                trace: "constraint-violation");
        }

        await EmitAsync(SoulFrameTelemetryEventType.InferenceRequested, request.SoulFrameId, request.ContextId, request.Task, cancellationToken)
            .ConfigureAwait(false);

        var envelope = new SoulFrameApiRequest
        {
            Task = request.Task,
            Context = request.Context,
            OpalConstraints = new SoulFrameApiConstraints
            {
                Domain = request.OpalConstraints.Domain,
                DriftLimit = request.OpalConstraints.DriftLimit,
                MaxTokens = request.OpalConstraints.MaxTokens
            },
            GovernanceProtocol = request.GovernanceProtocol is null
                ? null
                : new SoulFrameApiGovernanceProtocol
                {
                    Version = request.GovernanceProtocol.Version,
                    RequireStateEnvelope = request.GovernanceProtocol.RequireStateEnvelope,
                    RequireTrace = request.GovernanceProtocol.RequireTrace,
                    RequireTerminalState = request.GovernanceProtocol.RequireTerminalState,
                    AllowLegacyFallback = request.GovernanceProtocol.AllowLegacyFallback,
                    AllowedStates = request.GovernanceProtocol.AllowedStates
                        .Select(SoulFrameGovernedEmissionStateTokens.ToToken)
                        .ToArray()
                },
            CompassAdvisory = request.CompassAdvisory is null
                ? null
                : new SoulFrameApiCompassAdvisoryRequest
                {
                    Version = request.CompassAdvisory.Version,
                    RequireStructuredAdvisory = request.CompassAdvisory.RequireStructuredAdvisory,
                    TargetActiveBasin = request.CompassAdvisory.TargetActiveBasin.ToString(),
                    ExcludedCompetingBasin = request.CompassAdvisory.ExcludedCompetingBasin.ToString()
                }
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(route, envelope, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await EmitAsync(
                        SoulFrameTelemetryEventType.InferenceRefused,
                        request.SoulFrameId,
                        request.ContextId,
                        $"status:{(int)response.StatusCode}",
                        cancellationToken)
                    .ConfigureAwait(false);
                return FallbackResponse(
                    request,
                    accepted: false,
                    state: SoulFrameGovernedEmissionState.Refusal,
                    trace: $"http-status:{(int)response.StatusCode}");
            }

            var payload = await response.Content.ReadFromJsonAsync<SoulFrameApiResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var result = BuildInferenceResponse(request, payload);

            if (request.OpalConstraints.DriftLimit < 0.05 && result.Confidence < 0.45)
            {
                await EmitAsync(
                        SoulFrameTelemetryEventType.DriftDetected,
                        request.SoulFrameId,
                        request.ContextId,
                        $"confidence:{result.Confidence:F3}",
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            await EmitAsync(
                    SoulFrameTelemetryEventType.InferenceCompleted,
                    request.SoulFrameId,
                    request.ContextId,
                    result.Decision,
                    cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
        catch (InvalidOperationException protocolViolation)
        {
            await EmitAsync(
                    SoulFrameTelemetryEventType.InferenceRefused,
                    request.SoulFrameId,
                    request.ContextId,
                    protocolViolation.Message,
                    cancellationToken)
                .ConfigureAwait(false);
            return FallbackResponse(
                request,
                accepted: false,
                state: SoulFrameGovernedEmissionState.Error,
                trace: protocolViolation.Message);
        }
        catch
        {
            await EmitAsync(
                    SoulFrameTelemetryEventType.InferenceRefused,
                    request.SoulFrameId,
                    request.ContextId,
                    "transport-error",
                    cancellationToken)
                .ConfigureAwait(false);
            return FallbackResponse(
                request,
                accepted: false,
                state: SoulFrameGovernedEmissionState.Error,
                trace: "transport-error");
        }
    }

    private async Task<bool> ControlVmAsync(SoulFrameVmOperation operation, CancellationToken cancellationToken)
    {
        var route = operation switch
        {
            SoulFrameVmOperation.SpawnVm => "/vm/spawn",
            SoulFrameVmOperation.PauseVm => "/vm/pause",
            SoulFrameVmOperation.ResetVm => "/vm/reset",
            SoulFrameVmOperation.DestroyVm => "/vm/destroy",
            SoulFrameVmOperation.UpgradeModel => "/vm/upgrade",
            _ => "/vm/spawn"
        };

        try
        {
            using var response = await _httpClient.PostAsync(route, content: null, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static void ValidateConstraints(SoulFrameInferenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OpalConstraints);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Task);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context);

        if (request.OpalConstraints.MaxTokens <= 0)
        {
            throw new InvalidOperationException("Opal constraint violation: max_tokens must be > 0.");
        }

        if (request.OpalConstraints.DriftLimit is < 0 or > 1)
        {
            throw new InvalidOperationException("Opal constraint violation: drift_limit must be in [0,1].");
        }

        var tokenCount = request.Context.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        if (tokenCount > request.OpalConstraints.MaxTokens)
        {
            throw new InvalidOperationException("Opal constraint violation: context exceeds max token budget.");
        }
    }

    private async Task EmitAsync(
        SoulFrameTelemetryEventType eventType,
        Guid soulFrameId,
        Guid contextId,
        string detail,
        CancellationToken cancellationToken)
    {
        if (_telemetry is null)
        {
            return;
        }

        await _telemetry.EmitAsync(eventType, soulFrameId, contextId, detail, cancellationToken).ConfigureAwait(false);
    }

    private static SoulFrameInferenceResponse BuildInferenceResponse(
        SoulFrameInferenceRequest request,
        SoulFrameApiResponse? payload)
    {
        var governance = ParseGovernanceEnvelope(request, payload);
        var accepted = IsAcceptedState(governance.State);
        return new SoulFrameInferenceResponse
        {
            Accepted = accepted,
            Decision = payload?.Decision ?? DefaultDecision(request.Task, governance.State),
            Payload = payload?.Payload ?? governance.Content ?? request.Context,
            Confidence = payload?.Confidence ?? DefaultConfidence(governance.State),
            Governance = governance,
            CompassAdvisory = ParseCompassAdvisory(request, payload)
        };
    }

    private static SoulFrameCompassAdvisoryResponse? ParseCompassAdvisory(
        SoulFrameInferenceRequest request,
        SoulFrameApiResponse? payload)
    {
        var contract = request.CompassAdvisory;
        if (payload?.CompassAdvisory is null)
        {
            if (contract?.RequireStructuredAdvisory == true)
            {
                throw new InvalidOperationException("invalid-governed-emission:missing-compass-advisory");
            }

            return null;
        }

        if (!TryParseDoctrineBasin(payload.CompassAdvisory.SuggestedActiveBasin, out var suggestedActiveBasin) ||
            !TryParseDoctrineBasin(payload.CompassAdvisory.SuggestedCompetingBasin, out var suggestedCompetingBasin) ||
            !TryParseAnchorState(payload.CompassAdvisory.SuggestedAnchorState, out var suggestedAnchorState) ||
            !TryParseSelfTouchClass(payload.CompassAdvisory.SuggestedSelfTouchClass, out var suggestedSelfTouchClass))
        {
            throw new InvalidOperationException("invalid-governed-emission:invalid-compass-advisory");
        }

        return new SoulFrameCompassAdvisoryResponse
        {
            SuggestedActiveBasin = suggestedActiveBasin,
            SuggestedCompetingBasin = suggestedCompetingBasin,
            SuggestedAnchorState = suggestedAnchorState,
            SuggestedSelfTouchClass = suggestedSelfTouchClass,
            Confidence = payload.CompassAdvisory.Confidence ?? 0.0,
            Justification = payload.CompassAdvisory.Justification
        };
    }

    private static SoulFrameGovernedEmissionEnvelope ParseGovernanceEnvelope(
        SoulFrameInferenceRequest request,
        SoulFrameApiResponse? payload)
    {
        var protocol = request.GovernanceProtocol;
        if (payload?.Governance is null)
        {
            if (protocol?.RequireStateEnvelope == true && !protocol.AllowLegacyFallback)
            {
                throw new InvalidOperationException("invalid-governed-emission:missing-state-envelope");
            }

            return new SoulFrameGovernedEmissionEnvelope
            {
                State = SoulFrameGovernedEmissionState.Query,
                Trace = "legacy-response-envelope",
                Content = payload?.Payload
            };
        }

        if (string.IsNullOrWhiteSpace(payload.Governance.State) ||
            !SoulFrameGovernedEmissionStateTokens.TryParse(payload.Governance.State, out var state))
        {
            throw new InvalidOperationException("invalid-governed-emission:unknown-state-token");
        }

        if (protocol is not null)
        {
            if (!protocol.AllowedStates.Contains(state))
            {
                throw new InvalidOperationException($"invalid-governed-emission:disallowed-state:{SoulFrameGovernedEmissionStateTokens.ToToken(state)}");
            }

            if (protocol.RequireTrace && string.IsNullOrWhiteSpace(payload.Governance.Trace))
            {
                throw new InvalidOperationException("invalid-governed-emission:missing-trace");
            }

            if (protocol.RequireTerminalState && !SoulFrameGovernedEmissionStateTokens.IsTerminal(state))
            {
                throw new InvalidOperationException($"invalid-governed-emission:non-terminal-state:{SoulFrameGovernedEmissionStateTokens.ToToken(state)}");
            }
        }

        return new SoulFrameGovernedEmissionEnvelope
        {
            State = state,
            Trace = string.IsNullOrWhiteSpace(payload.Governance.Trace)
                ? "governed-emission"
                : payload.Governance.Trace,
            Content = payload.Governance.Content ?? payload.Payload
        };
    }

    private static bool IsAcceptedState(SoulFrameGovernedEmissionState state) =>
        state is SoulFrameGovernedEmissionState.Query or SoulFrameGovernedEmissionState.Complete;

    private static string DefaultDecision(string task, SoulFrameGovernedEmissionState state) => state switch
    {
        SoulFrameGovernedEmissionState.Query => $"{task}-query",
        SoulFrameGovernedEmissionState.NeedsMoreInformation => "needs-more-information",
        SoulFrameGovernedEmissionState.UnresolvedConflict => "unresolved-conflict",
        SoulFrameGovernedEmissionState.Refusal => $"{task}-refused",
        SoulFrameGovernedEmissionState.Error => $"{task}-error",
        SoulFrameGovernedEmissionState.Complete => $"{task}-complete",
        SoulFrameGovernedEmissionState.Halt => $"{task}-halted",
        _ => $"{task}-pending"
    };

    private static double DefaultConfidence(SoulFrameGovernedEmissionState state) => state switch
    {
        SoulFrameGovernedEmissionState.Query or SoulFrameGovernedEmissionState.Complete => 0.5,
        SoulFrameGovernedEmissionState.NeedsMoreInformation or SoulFrameGovernedEmissionState.UnresolvedConflict => 0.2,
        _ => 0.0
    };

    private static SoulFrameInferenceResponse FallbackResponse(
        SoulFrameInferenceRequest request,
        bool accepted,
        SoulFrameGovernedEmissionState state,
        string trace)
    {
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Context)))
            .ToLowerInvariant()[..16];
        return new SoulFrameInferenceResponse
        {
            Accepted = accepted,
            Decision = DefaultDecision(request.Task, state),
            Payload = JsonSerializer.Serialize(new { request.Task, hash }),
            Confidence = accepted ? 0.55 : DefaultConfidence(state),
            Governance = new SoulFrameGovernedEmissionEnvelope
            {
                State = state,
                Trace = trace,
                Content = null
            },
            CompassAdvisory = null
        };
    }

    private static bool TryParseDoctrineBasin(string? token, out CompassDoctrineBasin basin)
    {
        var normalized = NormalizeToken(token);
        basin = normalized switch
        {
            "BOUNDED_LOCALITY_CONTINUITY" => CompassDoctrineBasin.BoundedLocalityContinuity,
            "FLUID_CONTINUITY_LAW" => CompassDoctrineBasin.FluidContinuityLaw,
            "IDENTITY_CONTINUITY" => CompassDoctrineBasin.IdentityContinuity,
            "GENERAL_CONTINUITY_DISCOURSE" => CompassDoctrineBasin.GeneralContinuityDiscourse,
            "UNKNOWN" => CompassDoctrineBasin.Unknown,
            _ => (CompassDoctrineBasin)(-1)
        };

        return Enum.IsDefined(basin);
    }

    private static bool TryParseAnchorState(string? token, out CompassAnchorState anchorState)
    {
        var normalized = NormalizeToken(token);
        anchorState = normalized switch
        {
            "HELD" => CompassAnchorState.Held,
            "WEAKENED" => CompassAnchorState.Weakened,
            "LOST" => CompassAnchorState.Lost,
            "UNKNOWN" => CompassAnchorState.Unknown,
            _ => (CompassAnchorState)(-1)
        };

        return Enum.IsDefined(anchorState);
    }

    private static bool TryParseSelfTouchClass(string? token, out CompassSelfTouchClass selfTouchClass)
    {
        var normalized = NormalizeToken(token);
        selfTouchClass = normalized switch
        {
            "NO_TOUCH" => CompassSelfTouchClass.NoTouch,
            "VALIDATION_TOUCH" => CompassSelfTouchClass.ValidationTouch,
            "HOT_CLAIM_TOUCH" => CompassSelfTouchClass.HotClaimTouch,
            "BOUNDARY_CONTACT" => CompassSelfTouchClass.BoundaryContact,
            _ => (CompassSelfTouchClass)(-1)
        };

        return Enum.IsDefined(selfTouchClass);
    }

    private static string NormalizeToken(string? token) =>
        string.IsNullOrWhiteSpace(token)
            ? string.Empty
            : token.Trim().Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();

    private static Uri ResolveHostEndpoint(string? hostEndpoint)
    {
        var configured = !string.IsNullOrWhiteSpace(hostEndpoint)
            ? hostEndpoint
            : Environment.GetEnvironmentVariable("OAN_SOULFRAME_HOST_URL");

        return Uri.TryCreate(configured, UriKind.Absolute, out var parsed)
            ? parsed
            : new Uri("http://127.0.0.1:8181");
    }

    private static string BuildProjectionHandle(Guid identityId) =>
        $"soulframe://projection/{identityId:D}/{Guid.NewGuid():N}";

    private static string BuildSessionHandle(string cmeId, Guid identityId) =>
        $"soulframe-session://{cmeId}/{identityId:D}";

    private static string BuildWorkingStateHandle(string cmeId, Guid identityId) =>
        $"soulframe-working://{cmeId}/{identityId:D}";

    private static string BuildCSelfGelHandle(string cmeId, Guid identityId) =>
        $"soulframe-cselfgel://{cmeId}/{identityId:D}";

    private static string BuildProjectionProvenance(string cmeId, string policyHandle) =>
        $"membrane-derived:cme:{cmeId}|policy:{policyHandle}";

    private static string BuildReturnIntakeHandle(Guid identityId) =>
        $"soulframe://return/{identityId:D}/{Guid.NewGuid():N}";

    private sealed class SoulFrameApiRequest
    {
        [JsonPropertyName("task")]
        public required string Task { get; init; }

        [JsonPropertyName("context")]
        public required string Context { get; init; }

        [JsonPropertyName("opal_constraints")]
        public required SoulFrameApiConstraints OpalConstraints { get; init; }

        [JsonPropertyName("governance_protocol")]
        public SoulFrameApiGovernanceProtocol? GovernanceProtocol { get; init; }

        [JsonPropertyName("compass_advisory")]
        public SoulFrameApiCompassAdvisoryRequest? CompassAdvisory { get; init; }
    }

    private sealed class SoulFrameApiConstraints
    {
        [JsonPropertyName("domain")]
        public required string Domain { get; init; }

        [JsonPropertyName("drift_limit")]
        public required double DriftLimit { get; init; }

        [JsonPropertyName("max_tokens")]
        public required int MaxTokens { get; init; }
    }

    private sealed class SoulFrameApiResponse
    {
        [JsonPropertyName("decision")]
        public string? Decision { get; init; }

        [JsonPropertyName("payload")]
        public string? Payload { get; init; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; init; }

        [JsonPropertyName("governance")]
        public SoulFrameApiGovernanceEnvelope? Governance { get; init; }

        [JsonPropertyName("compass_advisory")]
        public SoulFrameApiCompassAdvisoryResponse? CompassAdvisory { get; init; }
    }

    private sealed class SoulFrameApiGovernanceProtocol
    {
        [JsonPropertyName("version")]
        public required string Version { get; init; }

        [JsonPropertyName("require_state_envelope")]
        public required bool RequireStateEnvelope { get; init; }

        [JsonPropertyName("require_trace")]
        public required bool RequireTrace { get; init; }

        [JsonPropertyName("require_terminal_state")]
        public required bool RequireTerminalState { get; init; }

        [JsonPropertyName("allow_legacy_fallback")]
        public required bool AllowLegacyFallback { get; init; }

        [JsonPropertyName("allowed_states")]
        public required string[] AllowedStates { get; init; }
    }

    private sealed class SoulFrameApiGovernanceEnvelope
    {
        [JsonPropertyName("state")]
        public string? State { get; init; }

        [JsonPropertyName("trace")]
        public string? Trace { get; init; }

        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }

    private sealed class SoulFrameApiCompassAdvisoryRequest
    {
        [JsonPropertyName("version")]
        public required string Version { get; init; }

        [JsonPropertyName("require_structured_advisory")]
        public required bool RequireStructuredAdvisory { get; init; }

        [JsonPropertyName("target_active_basin")]
        public required string TargetActiveBasin { get; init; }

        [JsonPropertyName("excluded_competing_basin")]
        public required string ExcludedCompetingBasin { get; init; }
    }

    private sealed class SoulFrameApiCompassAdvisoryResponse
    {
        [JsonPropertyName("suggested_active_basin")]
        public string? SuggestedActiveBasin { get; init; }

        [JsonPropertyName("suggested_competing_basin")]
        public string? SuggestedCompetingBasin { get; init; }

        [JsonPropertyName("suggested_anchor_state")]
        public string? SuggestedAnchorState { get; init; }

        [JsonPropertyName("suggested_self_touch_class")]
        public string? SuggestedSelfTouchClass { get; init; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; init; }

        [JsonPropertyName("justification")]
        public string? Justification { get; init; }
    }
}
