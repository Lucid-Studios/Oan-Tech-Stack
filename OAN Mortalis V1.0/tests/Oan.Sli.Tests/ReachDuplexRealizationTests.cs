using AgentiCore.Runtime;
using SLI.Engine;

namespace Oan.Sli.Tests;

public sealed class ReachDuplexRealizationTests
{
    [Fact]
    public async Task AgentiRuntime_ReachDispatch_CarriesUtilityAndLocalityPredicates()
    {
        var runtime = new AgentiRuntime(new LispSliBridgeStub());
        var envelope = ReachDuplexRealizationSurfaceContracts.CreateEnvelope(
            utilitySurfaceHandle: "agenticore-actual-surface://surface-01",
            duplexEnvelopeId: "agenticore-duplex-envelope://envelope-01",
            sourceLocality: "Sanctuary.actual",
            targetLocality: "Operator.actual",
            bondedSpaceHandle: "bonded-space://space-01",
            accessTopologyState: "provisional-reach-legibility",
            legibilityState: "bounded-legibility-ready",
            witnessHandle: "reach-witness://session-01",
            returnCondition: "return-through-bonded-dissolution",
            authorityClass: "governed-utility");

        var receipt = await runtime.SendReachDuplexRealizationAsync(envelope);

        Assert.Equal(envelope.EnvelopeId, receipt.EnvelopeId);
        Assert.Equal("accepted", receipt.DispatchState);
        Assert.Equal("reach-duplex-dispatch-accepted", receipt.ReasonCode);
        Assert.Contains(":frame reach", receipt.Packet, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(":op realization-envelope", receipt.Packet, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sanctuary.actual", receipt.Packet, StringComparison.Ordinal);
        Assert.Contains("Operator.actual", receipt.Packet, StringComparison.Ordinal);
        Assert.Contains(":status accepted", receipt.BridgeResponse, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReachEnvelope_Creation_IsDeterministic()
    {
        var left = ReachDuplexRealizationSurfaceContracts.CreateEnvelope(
            utilitySurfaceHandle: "agenticore-actual-surface://surface-01",
            duplexEnvelopeId: "agenticore-duplex-envelope://envelope-01",
            sourceLocality: "Sanctuary.actual",
            targetLocality: "Operator.actual",
            bondedSpaceHandle: "bonded-space://space-01",
            accessTopologyState: "provisional-reach-legibility",
            legibilityState: "bounded-legibility-ready",
            witnessHandle: "reach-witness://session-01",
            returnCondition: "return-through-bonded-dissolution",
            authorityClass: "governed-utility");
        var right = ReachDuplexRealizationSurfaceContracts.CreateEnvelope(
            utilitySurfaceHandle: "agenticore-actual-surface://surface-01",
            duplexEnvelopeId: "agenticore-duplex-envelope://envelope-01",
            sourceLocality: "Sanctuary.actual",
            targetLocality: "Operator.actual",
            bondedSpaceHandle: "bonded-space://space-01",
            accessTopologyState: "provisional-reach-legibility",
            legibilityState: "bounded-legibility-ready",
            witnessHandle: "reach-witness://session-01",
            returnCondition: "return-through-bonded-dissolution",
            authorityClass: "governed-utility");

        Assert.Equal(left.EnvelopeId, right.EnvelopeId);
    }
}
