using AgentiCore.Runtime;
using AgentiCore.Services;
using CradleTek.Runtime;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class SanctuaryRuntimeWorkbenchServiceTests
{
    [Fact]
    public void CreateRuntimeWorkbenchSurface_BindsBoundedWorkbenchWithoutReleaseInflation()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (_, utilitySurface, _, localityLedger) = CreateReachProjectionBundle();

        var workbench = workbenchService.CreateRuntimeWorkbenchSurface(
            utilitySurface,
            localityLedger,
            runtimeDeployabilityState: "deployable-candidate-ready",
            sanctuaryRuntimeReadinessState: "bounded-working-state-ready",
            runtimeWorkAdmissibilityState: "provisional-runtime-work",
            sessionPosture: "bounded-workbench-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("sanctuary-runtime-workbench://", workbench.WorkbenchHandle, StringComparison.Ordinal);
        Assert.Equal("sanctuary-runtime-workbench-surface-bound", workbench.ReasonCode);
        Assert.Equal("bounded-workbench-ready", workbench.SessionPosture);
        Assert.Equal("bounded-local-candidate-sanctuary-workbench", workbench.BoundedWorkClass);
        Assert.True(workbench.BondedOperatorLaneWithheld);
        Assert.True(workbench.MosBearingReleaseDenied);
    }

    [Fact]
    public void CreateAmenableDayDreamTier_AndSelfRootedDepthGate_KeepExplorationDistinctFromDepth()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (threadBirth, utilitySurface, _, localityLedger) = CreateReachProjectionBundle();
        var workbench = workbenchService.CreateRuntimeWorkbenchSurface(
            utilitySurface,
            localityLedger,
            runtimeDeployabilityState: "deployable-candidate-ready",
            sanctuaryRuntimeReadinessState: "bounded-working-state-ready",
            runtimeWorkAdmissibilityState: "provisional-runtime-work",
            sessionPosture: "bounded-workbench-ready",
            timestampUtc: FixedTimestamp);

        var dayDreamTier = workbenchService.CreateAmenableDayDreamTier(
            workbench,
            exploratoryPredicates:
            [
                "soft-relational-probe",
                "directional-symbolic-rehearsal",
                "candidate-pattern-braiding"
            ],
            nonFinalOutputs:
            [
                "non-final-opal-surface",
                "candidate-relational-knot"
            ],
            admissibilityState: "amenable-exploratory-only",
            timestampUtc: FixedTimestamp);
        var depthGate = workbenchService.CreateSelfRootedCrypticDepthGate(
            threadBirth,
            workbench,
            dayDreamTier,
            gateState: "provisionally-rooted-withheld",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("amenable-day-dream-tier://", dayDreamTier.DayDreamTierHandle, StringComparison.Ordinal);
        Assert.Equal("amenable-day-dream-tier-admissibility-bound", dayDreamTier.ReasonCode);
        Assert.True(dayDreamTier.ExploratoryOnly);
        Assert.True(dayDreamTier.IdentityBearingDescentDenied);
        Assert.True(dayDreamTier.ContinuityInflationDenied);
        Assert.Contains("soft-relational-probe", dayDreamTier.ExploratoryPredicates);

        Assert.StartsWith("self-rooted-cryptic-depth-gate://", depthGate.DepthGateHandle, StringComparison.Ordinal);
        Assert.StartsWith("cryptic-biad-root://", depthGate.CrypticBiadRootHandle, StringComparison.Ordinal);
        Assert.Equal("self-rooted-cryptic-depth-gate-bound", depthGate.ReasonCode);
        Assert.Equal("provisionally-rooted-withheld", depthGate.GateState);
        Assert.True(depthGate.SelfRooted);
        Assert.True(depthGate.SharedAmenableOriginDenied);
        Assert.False(depthGate.DeepAccessGranted);
    }

    private static readonly DateTimeOffset FixedTimestamp = new(2026, 3, 22, 12, 0, 0, TimeSpan.Zero);

    private static (GovernedThreadBirthReceipt ThreadBirth, AgentiActualUtilitySurfaceReceipt UtilitySurface, ReachDuplexRealizationReceipt Realization, BondedParticipationLocalityLedgerReceipt LocalityLedger) CreateReachProjectionBundle()
    {
        var workerService = new GovernedWorkerThreadService();
        var reachService = new GovernedReachRealizationService();
        var governanceLayer = CreateGovernanceLayer();
        var threadBirth = workerService.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("56565656-5656-5656-5656-565656565656"), 1),
            governanceLayer,
            nexusBindingHandle: "nexus-binding://cme-workbench/worker/1",
            nexusPortalHandle: "nexus-portal://cme-workbench/primary",
            timestampUtc: FixedTimestamp);
        var duplexEnvelope = DuplexPredicateSurfaceContracts.CreateEnvelope(
            workPredicate: "sanctuary-workbench-bounded-participation",
            governancePredicate: "duplex-governed-no-grant",
            requestedBy: "sanctuary:test",
            scopeHandle: "Sanctuary.actual/workbench/session-01",
            nexusPortalId: "nexus-portal://cme-workbench/primary",
            witnessRequirement: "reach-witness://workbench/01",
            returnCondition: "return-through-bounded-workbench-dissolution",
            participationLocality: "Sanctuary.actual",
            admissibilityState: "bounded-workbench",
            authorityClass: "governed-utility");
        var utilitySurface = reachService.CreateAgentiActualUtilitySurface(
            threadBirth,
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
            witnessHandle: "reach-witness://workbench/01");
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
        var localityLedger = reachService.CreateBondedParticipationLocalityLedger(
            threadBirth,
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
                "ambient-access-grant",
                "mos-bearing-release"
            ],
            timestampUtc: FixedTimestamp);

        return (threadBirth, utilitySurface, realization, localityLedger);
    }

    private static BoundedWorkerState CreateBoundedWorkerState(Guid identityId, int sessionOrdinal)
    {
        return new BoundedWorkerState(
            IdentityId: identityId,
            CMEId: "cme-workbench",
            SessionHandle: $"soulframe-session://cme-workbench/{sessionOrdinal}",
            WorkingStateHandle: $"soulframe-working://cme-workbench/{sessionOrdinal}",
            ProvenanceMarker: $"membrane-derived:cme:cme-workbench|policy:sanctuary.runtime.workbench|loop:{sessionOrdinal}",
            TargetTheater: "sanctuary.actual",
            MediatedSelfState: new MediatedSelfStateContour(
                CSelfGelHandle: $"soulframe-cselfgel://cme-workbench/{sessionOrdinal}",
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
