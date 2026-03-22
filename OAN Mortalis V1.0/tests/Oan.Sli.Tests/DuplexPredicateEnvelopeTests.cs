using AgentiCore.Runtime;
using SLI.Engine;

namespace Oan.Sli.Tests;

public sealed class DuplexPredicateEnvelopeTests
{
    [Fact]
    public async Task AgentiRuntime_DuplexDispatch_CarriesWorkAndGovernancePredicates()
    {
        var runtime = new AgentiRuntime(new LispSliBridgeStub());
        var envelope = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "operator-actual-bounded-rehearsal",
            governancePredicate: "witness-only-no-grant",
            requestedBy: "operator:test",
            scopeHandle: "operator.actual/session-01",
            nexusPortalId: "portal:test",
            witnessRequirement: "membrane-witness",
            returnCondition: "dissolve-to-bounded-localities",
            participationLocality: "Operator.actual",
            admissibilityState: "bounded-rehearsal",
            authorityClass: "governed-utility");

        var receipt = await runtime.SendDuplexIntentAsync(envelope);

        Assert.Equal(envelope.EnvelopeId, receipt.EnvelopeId);
        Assert.Equal("accepted", receipt.DispatchState);
        Assert.Equal("agenticore-duplex-dispatch-accepted", receipt.ReasonCode);
        Assert.Contains(":frame agenticore.actual", receipt.Packet, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(":op predicate-envelope", receipt.Packet, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("operator-actual-bounded-rehearsal", receipt.Packet, StringComparison.Ordinal);
        Assert.Contains("witness-only-no-grant", receipt.Packet, StringComparison.Ordinal);
        Assert.Contains(":status accepted", receipt.BridgeResponse, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DuplexEnvelope_Creation_IsDeterministic()
    {
        var left = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "operator-actual-bounded-rehearsal",
            governancePredicate: "witness-only-no-grant",
            requestedBy: "operator:test",
            scopeHandle: "operator.actual/session-01",
            nexusPortalId: "portal:test",
            witnessRequirement: "membrane-witness",
            returnCondition: "dissolve-to-bounded-localities",
            participationLocality: "Operator.actual",
            admissibilityState: "bounded-rehearsal",
            authorityClass: "governed-utility");
        var right = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "operator-actual-bounded-rehearsal",
            governancePredicate: "witness-only-no-grant",
            requestedBy: "operator:test",
            scopeHandle: "operator.actual/session-01",
            nexusPortalId: "portal:test",
            witnessRequirement: "membrane-witness",
            returnCondition: "dissolve-to-bounded-localities",
            participationLocality: "Operator.actual",
            admissibilityState: "bounded-rehearsal",
            authorityClass: "governed-utility");

        Assert.Equal(left.EnvelopeId, right.EnvelopeId);
    }
}
