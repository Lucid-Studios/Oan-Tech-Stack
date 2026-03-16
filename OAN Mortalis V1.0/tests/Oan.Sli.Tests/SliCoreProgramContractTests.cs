using CradleTek.Memory.Services;
using SLI.Engine;
using SLI.Engine.Cognition;
using SLI.Engine.Runtime;

namespace Oan.Sli.Tests;

public sealed class SliCoreProgramContractTests
{
    [Fact]
    public async Task LoweredProgram_UsesDeterministicIdAndHostMeaningAuthority()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();

        var program = bridge.LowerProgram(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)"
            ]);

        Assert.StartsWith("sli-core://", program.ProgramId, StringComparison.Ordinal);
        Assert.Equal("host-interpreter", program.MeaningAuthority);
        Assert.NotEmpty(program.Instructions);
        Assert.All(program.Instructions, instruction => Assert.Equal("host-interpreter", instruction.MeaningAuthority));
        Assert.All(
            program.Instructions,
            instruction => Assert.Equal(SliRuntimeCapabilityAvailability.Available, instruction.Availability));
        Assert.All(
            program.Instructions,
            instruction => Assert.Equal(SliRuntimeOperationClass.TargetCandidate, instruction.OperationClass));
        Assert.Contains(program.Instructions, instruction => instruction.Opcode.Equals("locality-bind", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(program.Instructions, instruction => instruction.Opcode.Equals("perspective-configure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CapabilityManifest_ListsCanonicalOperators()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();

        Assert.Equal("host-sli-interpreter", bridge.CapabilityManifest.RuntimeId);
        Assert.Equal("host-interpreter", bridge.CapabilityManifest.MeaningAuthority);
        Assert.Equal("host-managed", bridge.CapabilityManifest.RealizationProfile.ResidencyClass);
        Assert.True(bridge.CapabilityManifest.TryGetCapability("locality-bind", out var localityBind));
        Assert.Equal(SliRuntimeCapabilityAvailability.Available, localityBind.Availability);
        Assert.Equal("host-interpreter", localityBind.MeaningAuthority);
        Assert.Equal(SliRuntimeOperationClass.TargetCandidate, localityBind.OperationClass);
        Assert.True(bridge.CapabilityManifest.TryGetCapability("engram-query", out var engramQuery));
        Assert.Equal(SliRuntimeCapabilityAvailability.Available, engramQuery.Availability);
        Assert.Equal(SliRuntimeOperationClass.HostOnly, engramQuery.OperationClass);
        Assert.True(bridge.CapabilityManifest.TryGetCapability("decision-branch", out var decisionBranch));
        Assert.Equal(SliRuntimeOperationClass.SharedContract, decisionBranch.OperationClass);
    }

    [Fact]
    public async Task LoweredProgram_PreservesUnknownOperationsAsUnavailableInstructions()
    {
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();

        var program = bridge.LowerProgram(["(sanctuary-intake locality-state)"]);

        Assert.Single(program.Instructions);
        Assert.Equal("sanctuary-intake", program.Instructions[0].Opcode);
        Assert.Equal("unresolved", program.Instructions[0].MeaningAuthority);
        Assert.Equal(SliRuntimeCapabilityAvailability.Unavailable, program.Instructions[0].Availability);
        Assert.Equal(SliRuntimeOperationClass.HostOnly, program.Instructions[0].OperationClass);
    }

    [Fact]
    public async Task CognitionEngine_StillExecutesCanonicalProgramThroughLoweredSurface()
    {
        var engine = new SliCognitionEngine(new EngramResolverService());
        await engine.InitializeAsync();

        var program = engine.BuildProgram("identity-continuity", null);
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync();
        var lowered = bridge.LowerProgram(program);

        Assert.NotEmpty(lowered.Instructions);
        Assert.Contains(lowered.Instructions, instruction => instruction.Opcode.Equals("locality-bind", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lowered.Instructions, instruction => instruction.Opcode.Equals("compass-update", StringComparison.OrdinalIgnoreCase));
    }
}
