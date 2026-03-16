using Oan.Common;
using SLI.Engine;
using SLI.Engine.Cognition;
using SLI.Engine.Runtime;
using SLI.Engine.Telemetry;

namespace Oan.Sli.Tests;

public sealed class HigherOrderLocalityProgramTests
{
    [Fact]
    public async Task LocalityBootstrap_AppliesSafeDefaultsAtBindTime()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)"
            ],
            "identity-continuity");

        Assert.Equal("cme-self", result.SelfAnchor);
        Assert.Equal("task-objective", result.OtherAnchor);
        Assert.Equal("identity-continuity", result.RelationAnchor);
        Assert.Equal(SliHigherOrderLocalityState.BoundedSealPosture, result.SealPosture);
        Assert.Equal(SliHigherOrderLocalityState.MaskedRevealPosture, result.RevealPosture);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains("locality-bind(context)", result.SymbolicTrace);
    }

    [Fact]
    public async Task PerspectiveConfig_WithoutAnchors_ProducesTypedResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bind context)",
                "(perspective-configure locality-state)"
            ],
            "identity-continuity");

        Assert.False(result.Perspective.IsConfigured);
        Assert.Contains(
            result.Perspective.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.MissingAnchorPrerequisites);
        Assert.Contains(
            result.Perspective.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompletePerspective);
    }

    [Fact]
    public async Task ParticipationConfig_WithoutPerspective_ProducesTypedResidue_AndStaysObserve()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(participation-configure locality-state)"
            ],
            "identity-continuity");

        Assert.False(result.Participation.IsConfigured);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains(
            result.Participation.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.IncompleteParticipation);
    }

    [Fact]
    public async Task InvalidPostureValue_FallsBackToSafeDefault_AndRecordsResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bind context)",
                "(seal-posture permeable)",
                "(reveal-posture panoramic)"
            ],
            "identity-continuity");

        Assert.Equal(SliHigherOrderLocalityState.BoundedSealPosture, result.SealPosture);
        Assert.Equal(SliHigherOrderLocalityState.MaskedRevealPosture, result.RevealPosture);
        Assert.Contains(result.Residues, residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidPostureValue);
    }

    [Fact]
    public async Task InvalidParticipationMode_FallsBackToObserve_AndRecordsResidue()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-configure locality-state)",
                "(participation-mode improvise)"
            ],
            "identity-continuity");

        Assert.True(result.Participation.IsConfigured);
        Assert.Equal(SliHigherOrderLocalityState.ObserveMode, result.Participation.Mode);
        Assert.Contains(
            result.Participation.Residues,
            residue => residue.Kind == HigherOrderLocalityResidueKind.InvalidParticipationMode);
    }

    [Fact]
    public async Task DifferentCompositePrograms_ProduceDifferentHigherOrderLocalityResults()
    {
        var bridge = await CreateBridgeAsync();

        var boundedObserver = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ],
            "identity-continuity");

        var alternateObserver = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state alternate-focus context-preservation)",
                "(participation-bounded-cme locality-state)"
            ],
            "identity-continuity");

        Assert.NotEqual(
            boundedObserver.Perspective.OrientationVector.Keys.Single(),
            alternateObserver.Perspective.OrientationVector.Keys.Single());
        Assert.NotEqual(
            boundedObserver.Perspective.EthicalConstraints.Single(),
            alternateObserver.Perspective.EthicalConstraints.Single());
    }

    [Fact]
    public async Task SanctuaryAndCustodyOps_RemainUnknownInHigherOrderLocalityLane()
    {
        var bridge = await CreateBridgeAsync();

        var result = await bridge.ExecuteHigherOrderLocalityProgramAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(sanctuary-intake locality-state)",
                "(custody-write locality-state)"
            ],
            "identity-continuity");

        Assert.Contains("unknown-op(sanctuary-intake)", result.SymbolicTrace);
        Assert.Contains("unknown-op(custody-write)", result.SymbolicTrace);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetLane_RefusesWhenTargetCapabilityIsAbsent()
    {
        var bridge = await CreateBridgeAsync();
        var targetManifest = bridge.CreateTargetCapabilityManifest(Array.Empty<string>());

        var exception = Assert.Throws<SliTargetLaneRefusalException>(() =>
            bridge.EnsureHigherOrderLocalityTargetEligibility(
                [
                    "(locality-bootstrap context cme-self task-objective identity-continuity)",
                    "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                    "(participation-bounded-cme locality-state)"
                ],
                targetManifest));

        Assert.Equal("higher-order-locality", exception.Eligibility.LaneId);
        Assert.Equal("target-sli-runtime", exception.Eligibility.RuntimeId);
        Assert.Contains("locality-bind", exception.Eligibility.MissingTargetCapabilities);
        Assert.Contains("perspective-configure", exception.Eligibility.MissingTargetCapabilities);
        Assert.Contains("participation-configure", exception.Eligibility.MissingTargetCapabilities);
        Assert.Empty(exception.Eligibility.DisallowedOperations);
        Assert.Contains("higher-order-locality-unsupported", exception.Eligibility.ProfileViolations);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetLane_AcceptsCompleteTargetManifest()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode));

        var eligibility = bridge.EvaluateHigherOrderLocalityTargetEligibility(symbolicProgram, targetManifest);

        Assert.True(eligibility.IsEligible);
        Assert.Empty(eligibility.MissingTargetCapabilities);
        Assert.Empty(eligibility.DisallowedOperations);
        Assert.Empty(eligibility.ProfileViolations);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetLane_RejectsHostOnlyIngressOps()
    {
        var bridge = await CreateBridgeAsync();
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            bridge.LowerProgram(
                [
                    "(locality-bootstrap context cme-self task-objective identity-continuity)"
                ]).Instructions.Select(instruction => instruction.Opcode));

        var eligibility = bridge.EvaluateHigherOrderLocalityTargetEligibility(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(sanctuary-intake locality-state)"
            ],
            targetManifest);

        Assert.False(eligibility.IsEligible);
        Assert.Contains("sanctuary-intake", eligibility.DisallowedOperations);
        Assert.Empty(eligibility.ProfileViolations);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetLane_RejectsWitnessWorkWhenProfileIsTooNarrow()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var narrowProfile = SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: "gc-locality-only",
            supportsHigherOrderLocality: true,
            supportsBoundedRehearsal: false,
            supportsBoundedWitness: false,
            supportsBoundedTransport: false,
            supportsAdmissibleSurface: false,
            supportsAccountabilityPacket: false);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId: "gc-locality-runtime",
            realizationProfile: narrowProfile);

        var eligibility = bridge.EvaluateHigherOrderLocalityTargetEligibility(symbolicProgram, targetManifest);

        Assert.False(eligibility.IsEligible);
        Assert.Contains("bounded-witness-unsupported", eligibility.ProfileViolations);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetLane_RejectsWhenTraceBudgetIsTooNarrow()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var traceBoundProfile = SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: "gc-trace-budget-profile",
            supportsHigherOrderLocality: true,
            supportsBoundedRehearsal: false,
            supportsBoundedWitness: false,
            supportsBoundedTransport: false,
            supportsAdmissibleSurface: false,
            supportsAccountabilityPacket: false,
            maxTraceEntries: 4);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId: "gc-locality-runtime",
            realizationProfile: traceBoundProfile);

        var eligibility = bridge.EvaluateHigherOrderLocalityTargetEligibility(symbolicProgram, targetManifest);

        Assert.False(eligibility.IsEligible);
        Assert.Contains(eligibility.ProfileViolations, violation => violation.StartsWith("trace-budget-exceeded:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task HigherOrderLocalityTargetExecution_UsesExecutorWhenLaneIsEligible()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var targetProfile = SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: "gc-locality-profile",
            supportsHigherOrderLocality: true,
            supportsBoundedRehearsal: false,
            supportsBoundedWitness: false,
            supportsBoundedTransport: false,
            supportsAdmissibleSurface: false,
            supportsAccountabilityPacket: false);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId: "gc-locality-runtime",
            realizationProfile: targetProfile);
        var executor = new RecordingHigherOrderLocalityExecutor(targetManifest);

        var result = await bridge.ExecuteHigherOrderLocalityOnTargetAsync(
            symbolicProgram,
            "identity-continuity",
            executor);

        Assert.True(executor.WasInvoked);
        Assert.NotNull(executor.LastRequest);
        Assert.Equal(lowered.ProgramId, executor.LastRequest!.Program.ProgramId);
        Assert.Equal("target-locality-handle", result.LocalityHandle);
        Assert.Equal("identity-continuity", executor.LastRequest.Objective);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetExecution_UsesBoundedTargetExecutorImplementation()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var targetProfile = SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: "gc-locality-profile",
            supportsHigherOrderLocality: true,
            supportsBoundedRehearsal: false,
            supportsBoundedWitness: false,
            supportsBoundedTransport: false,
            supportsAdmissibleSurface: false,
            supportsAccountabilityPacket: false);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId: "gc-locality-runtime",
            realizationProfile: targetProfile);

        var result = await bridge.ExecuteHigherOrderLocalityOnTargetAsync(
            symbolicProgram,
            "identity-continuity",
            targetManifest);

        Assert.StartsWith("context:gc-locality-runtime:", result.LocalityHandle);
        Assert.Equal("cme-self", result.SelfAnchor);
        Assert.Equal("task-objective", result.OtherAnchor);
        Assert.Equal("identity-continuity", result.RelationAnchor);
        Assert.True(result.Perspective.IsConfigured);
        Assert.True(result.Participation.IsConfigured);
        Assert.Contains("target-runtime(gc-locality-runtime)", result.SymbolicTrace);
        Assert.Contains("target-profile(gc-locality-profile)", result.SymbolicTrace);
        Assert.Contains("locality-bind(context)", result.SymbolicTrace);
        Assert.Contains("perspective-configure(locality-state)", result.SymbolicTrace);
        Assert.Contains("participation-configure(locality-state)", result.SymbolicTrace);
        Assert.NotNull(result.TargetLineage);
        Assert.Equal("gc-locality-runtime", result.TargetLineage!.RuntimeId);
        Assert.Equal("gc-locality-profile", result.TargetLineage.ProfileId);
        Assert.Equal("target-bounded-lane", result.TargetLineage.BudgetClass);
        Assert.Equal("refusal-only", result.TargetLineage.CommitAuthorityClass);
        Assert.Equal(lowered.ProgramId, result.TargetLineage.ProgramId);
        Assert.Equal(result.SymbolicTrace.Count, result.TargetLineage.EmittedTraceCount);
        Assert.StartsWith("target-lineage://", result.TargetLineage.LineageHandle);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetExecution_EmitsGovernedTelemetryForSuccess()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var lowered = bridge.LowerProgram(symbolicProgram);
        var targetProfile = SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: "gc-locality-profile",
            supportsHigherOrderLocality: true,
            supportsBoundedRehearsal: false,
            supportsBoundedWitness: false,
            supportsBoundedTransport: false,
            supportsAdmissibleSurface: false,
            supportsAccountabilityPacket: false);
        var targetManifest = bridge.CreateTargetCapabilityManifest(
            lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId: "gc-locality-runtime",
            realizationProfile: targetProfile);
        var telemetrySink = new CapturingTelemetrySink();

        var result = await bridge.ExecuteHigherOrderLocalityOnTargetAsync(
            symbolicProgram,
            "identity-continuity",
            targetManifest,
            telemetrySink);

        var events = telemetrySink.Events
            .OfType<SliTargetExecutionTelemetryEvent>()
            .ToArray();

        Assert.Equal(2, events.Length);
        Assert.Equal("sli-target-admission-accepted", events[0].EventType);
        Assert.True(events[0].Accepted);
        Assert.Equal("CradleTek", events[0].WitnessedBy);
        Assert.Equal(lowered.ProgramId, events[0].ProgramId);
        Assert.Equal(result.TargetLineage!.AdmissionHandle, events[0].AdmissionHandle);
        Assert.Empty(events[0].Reasons);
        Assert.Empty(events[0].ReasonFamilies);
        Assert.Equal("target-bounded-lane", events[0].BudgetClass);
        Assert.Equal("refusal-only", events[0].CommitAuthorityClass);

        Assert.Equal("sli-target-lineage-recorded", events[1].EventType);
        Assert.True(events[1].Accepted);
        Assert.Equal(result.TargetLineage.LineageHandle, events[1].LineageHandle);
        Assert.Equal(result.TargetLineage.TraceHandle, events[1].TraceHandle);
        Assert.Equal(result.TargetLineage.ResidueHandle, events[1].ResidueHandle);
        Assert.Equal(result.TargetLineage.EmittedTraceCount, events[1].EmittedTraceCount);
        Assert.Equal(result.TargetLineage.EmittedResidueCount, events[1].EmittedResidueCount);
    }

    [Fact]
    public async Task HigherOrderLocalityTargetExecution_EmitsGovernedRefusalTelemetry()
    {
        var bridge = await CreateBridgeAsync();
        IReadOnlyList<string> symbolicProgram =
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ];
        var telemetrySink = new CapturingTelemetrySink();
        var targetManifest = bridge.CreateTargetCapabilityManifest(Array.Empty<string>());

        var exception = await Assert.ThrowsAsync<SliTargetLaneRefusalException>(() =>
            bridge.ExecuteHigherOrderLocalityOnTargetAsync(
                symbolicProgram,
                "identity-continuity",
                targetManifest,
                telemetrySink));

        var telemetryEvent = Assert.Single(
            telemetrySink.Events.OfType<SliTargetExecutionTelemetryEvent>());

        Assert.Equal("sli-target-admission-refused", telemetryEvent.EventType);
        Assert.False(telemetryEvent.Accepted);
        Assert.Equal("higher-order-locality", telemetryEvent.LaneId);
        Assert.Equal("target-sli-runtime", telemetryEvent.RuntimeId);
        Assert.Equal("CradleTek", telemetryEvent.WitnessedBy);
        Assert.Contains("missing-capability", telemetryEvent.ReasonFamilies);
        Assert.Contains("profile-violation", telemetryEvent.ReasonFamilies);
        Assert.Contains(
            telemetryEvent.Reasons,
            reason => reason.StartsWith("missing-capability:locality-bind", StringComparison.Ordinal));
        Assert.Contains(
            telemetryEvent.Reasons,
            reason => reason.StartsWith("profile-violation:higher-order-locality-unsupported", StringComparison.Ordinal));
        Assert.StartsWith("target-admission://", telemetryEvent.AdmissionHandle);
        Assert.Null(telemetryEvent.LineageHandle);
        Assert.Equal(exception.Eligibility.BudgetUsage.ProjectedTraceEntryCount, telemetryEvent.BudgetUsage.ProjectedTraceEntryCount);
    }

    private static async Task<LispBridge> CreateBridgeAsync()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        return bridge;
    }

    private sealed class RecordingHigherOrderLocalityExecutor : ISliTargetHigherOrderLocalityExecutor
    {
        public RecordingHigherOrderLocalityExecutor(SliRuntimeCapabilityManifest capabilityManifest)
        {
            CapabilityManifest = capabilityManifest;
        }

        public bool WasInvoked { get; private set; }
        public SliTargetHigherOrderLocalityExecutionRequest? LastRequest { get; private set; }
        public SliRuntimeCapabilityManifest CapabilityManifest { get; }

        public Task<SliHigherOrderLocalityResult> ExecuteAsync(
            SliTargetHigherOrderLocalityExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            LastRequest = request;
            return Task.FromResult(new SliHigherOrderLocalityResult
            {
                LocalityHandle = "target-locality-handle",
                SelfAnchor = "cme-self",
                OtherAnchor = "task-objective",
                RelationAnchor = "identity-continuity",
                SealPosture = SliHigherOrderLocalityState.BoundedSealPosture,
                RevealPosture = SliHigherOrderLocalityState.MaskedRevealPosture,
                Warnings = Array.Empty<string>(),
                Residues = Array.Empty<HigherOrderLocalityResidue>(),
                Perspective = new SliPerspectiveResult
                {
                    IsConfigured = true,
                    OrientationVector = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["task-objective"] = 1.0
                    },
                    EthicalConstraints = ["identity-continuity"],
                    WeightFunctions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["task-objective"] = 1.0
                    },
                    Residues = Array.Empty<HigherOrderLocalityResidue>()
                },
                Participation = new SliParticipationResult
                {
                    IsConfigured = true,
                    Mode = SliHigherOrderLocalityState.ObserveMode,
                    Role = "bounded-cme",
                    InteractionRules = ["observe-only"],
                    CapabilitySet = ["bounded-locality"],
                    Residues = Array.Empty<HigherOrderLocalityResidue>()
                },
                SymbolicTrace = ["target-executor(locality)"]
            });
        }
    }

    private sealed class CapturingTelemetrySink : ITelemetrySink
    {
        public List<object> Events { get; } = [];

        public Task EmitAsync(object telemetryEvent)
        {
            Events.Add(telemetryEvent);
            return Task.CompletedTask;
        }
    }
}
