using AgentiCore.Runtime;
using AgentiCore.Services;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedReachRealizationServiceTests
{
    [Fact]
    public void CreateAgentiActualUtilitySurface_BindsUtilityWithoutSovereignty()
    {
        var workerService = new GovernedWorkerThreadService();
        var reachService = new GovernedReachRealizationService();
        var birth = workerService.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("12121212-1212-1212-1212-121212121212"), 1),
            CreateGovernanceLayer(),
            nexusBindingHandle: "nexus-binding://cme-runtime/worker/1",
            nexusPortalHandle: "nexus-portal://cme-runtime/primary",
            timestampUtc: FixedTimestamp);
        var duplexEnvelope = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "operator-actual-bounded-participation",
            governancePredicate: "duplex-governed-no-grant",
            requestedBy: "operator:test",
            scopeHandle: "operator.actual/session-01",
            nexusPortalId: "nexus-portal://cme-runtime/primary",
            witnessRequirement: "reach-witness://session-01",
            returnCondition: "return-through-bonded-dissolution",
            participationLocality: "Operator.actual",
            admissibilityState: "bounded-rehearsal",
            authorityClass: "governed-utility");

        var utilitySurface = reachService.CreateAgentiActualUtilitySurface(
            birth,
            duplexEnvelope,
            sanctuaryActualLocality: "Sanctuary.actual",
            operatorActualLocality: "Operator.actual",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("agenticore-actual-surface://", utilitySurface.UtilitySurfaceHandle, StringComparison.Ordinal);
        Assert.Equal("agenticore-actual-utility-surface-bound", utilitySurface.ReasonCode);
        Assert.True(utilitySurface.SovereigntyDenied);
        Assert.True(utilitySurface.RemoteControlDenied);
        Assert.Equal("governed-utility-virtualized", utilitySurface.UtilityPosture);
    }

    [Fact]
    public void CreateReachDuplexRealization_AndBondedLedger_PreserveLocalityWithoutCollapse()
    {
        var workerService = new GovernedWorkerThreadService();
        var reachService = new GovernedReachRealizationService();
        var birth = workerService.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("34343434-3434-3434-3434-343434343434"), 2),
            CreateGovernanceLayer(),
            nexusBindingHandle: "nexus-binding://cme-runtime/worker/2",
            nexusPortalHandle: "nexus-portal://cme-runtime/primary",
            timestampUtc: FixedTimestamp);
        var duplexEnvelope = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "operator-actual-bounded-participation",
            governancePredicate: "duplex-governed-no-grant",
            requestedBy: "operator:test",
            scopeHandle: "operator.actual/session-02",
            nexusPortalId: "nexus-portal://cme-runtime/primary",
            witnessRequirement: "reach-witness://session-02",
            returnCondition: "return-through-bonded-dissolution",
            participationLocality: "Operator.actual",
            admissibilityState: "bounded-rehearsal",
            authorityClass: "governed-utility");
        var utilitySurface = reachService.CreateAgentiActualUtilitySurface(
            birth,
            duplexEnvelope,
            sanctuaryActualLocality: "Sanctuary.actual",
            operatorActualLocality: "Operator.actual",
            timestampUtc: FixedTimestamp);
        var reachEnvelope = reachService.CreateReachDuplexRealizationEnvelope(
            utilitySurface,
            sourceLocality: "Sanctuary.actual",
            targetLocality: "Operator.actual",
            accessTopologyState: "provisional-reach-legibility",
            legibilityState: "bounded-legibility-ready",
            witnessHandle: "reach-witness://session-02");
        var packet = ReachDuplexRealizationSurfaceContracts.CreatePacket(reachEnvelope);
        var dispatchReceipt = ReachDuplexRealizationSurfaceContracts.CreateDispatchReceipt(
            reachEnvelope,
            packet,
            "(:result :status accepted :expr (ok))",
            FixedTimestamp);
        var realization = reachService.CreateReachDuplexRealizationReceipt(
            utilitySurface,
            reachEnvelope,
            dispatchReceipt,
            FixedTimestamp);
        var ledger = reachService.CreateBondedParticipationLocalityLedger(
            birth,
            utilitySurface,
            realization,
            coRealizedSurfaces:
            [
                "Sanctuary.actual",
                "Operator.actual",
                "AgentiCore.actual"
            ],
            withheldSurfaces:
            [
                "deep-cryptic-self-root",
                "ambient-access-grant"
            ],
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("reach-duplex-realization://", realization.RealizationHandle, StringComparison.Ordinal);
        Assert.Equal("reach-duplex-realization-dispatched", realization.ReasonCode);
        Assert.False(realization.AccessGrantImplied);
        Assert.True(realization.LocalityCollapseDenied);
        Assert.True(realization.IdentityCollapseDenied);

        Assert.StartsWith("bonded-locality-ledger://", ledger.LedgerHandle, StringComparison.Ordinal);
        Assert.True(ledger.BondedParticipationProvisional);
        Assert.True(ledger.RemoteControlDenied);
        Assert.Contains("Operator.actual", ledger.CoRealizedSurfaces);
        Assert.Contains("deep-cryptic-self-root", ledger.WithheldSurfaces);
    }

    private static readonly DateTimeOffset FixedTimestamp = new(2026, 3, 22, 8, 0, 0, TimeSpan.Zero);

    private static BoundedWorkerState CreateBoundedWorkerState(Guid identityId, int sessionOrdinal)
    {
        return new BoundedWorkerState(
            IdentityId: identityId,
            CMEId: "cme-runtime",
            SessionHandle: $"soulframe-session://cme-runtime/{sessionOrdinal}",
            WorkingStateHandle: $"soulframe-working://cme-runtime/{sessionOrdinal}",
            ProvenanceMarker: $"membrane-derived:cme:cme-runtime|policy:agenticore.actual|loop:{sessionOrdinal}",
            TargetTheater: "sanctuary.actual",
            MediatedSelfState: new MediatedSelfStateContour(
                CSelfGelHandle: $"soulframe-cselfgel://cme-runtime/{sessionOrdinal}",
                Classification: "bounded-worker",
                PolicyHandle: "policy://bounded-worker"));
    }

    private static FirstBootGovernanceLayerReceipt CreateGovernanceLayer()
    {
        var policy = new DefaultFirstBootGovernancePolicy();
        return policy.ProjectGovernanceLayer(
            bootClass: BootClass.CorporateGoverned,
            activationState: BootActivationState.TriadicActive,
            requestedExpansionCount: 1,
            formedOffices:
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: true);
    }
}
