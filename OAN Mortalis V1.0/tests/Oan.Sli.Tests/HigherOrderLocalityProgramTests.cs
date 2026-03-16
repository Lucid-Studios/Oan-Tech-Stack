using SLI.Engine;
using SLI.Engine.Cognition;
using SLI.Engine.Runtime;

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
}
