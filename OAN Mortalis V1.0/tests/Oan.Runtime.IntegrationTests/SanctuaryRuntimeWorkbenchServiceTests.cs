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

    [Fact]
    public void CreateRuntimeWorkbenchSessionLedger_BindsSessionEventsAndBoundaries()
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
                "open-structure-question",
                "coherence-probe",
                "non-collapsing-clarification"
            ],
            nonFinalOutputs:
            [
                "candidate-knot",
                "non-final-opal-trace"
            ],
            timestampUtc: FixedTimestamp);
        var depthGate = workbenchService.CreateSelfRootedCrypticDepthGate(
            threadBirth,
            workbench,
            dayDreamTier,
            timestampUtc: FixedTimestamp);
        var sessionEvent = SanctuaryWorkbenchProjector.CreateSessionEvent(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            eventKind: "questioning",
            inquiryStance: "clarify",
            eventState: "coherence-gain",
            coherencePreserving: true,
            hiddenAssumptionDenied: true,
            description: "what would need to be true for this to hold together?",
            timestampUtc: FixedTimestamp);
        var boundaryCondition = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "overcompression-of-distinction",
            failureClass: "coordination-fracture",
            triggerPredicate: "question-demands-conclusion-too-early",
            continuityRequirement: "keep-amenable-and-depth-lanes-distinct",
            permissionState: "withhold-crossing",
            notes: "session inquiry must not force deep identity conclusions from exploratory motion.");

        var sessionLedger = workbenchService.CreateRuntimeWorkbenchSessionLedger(
            workbench,
            dayDreamTier,
            depthGate,
            sessionEvents: [sessionEvent],
            boundaryConditions: [boundaryCondition],
            sessionState: "bounded-session-open",
            sessionPosture: "bounded-session-open",
            returnPosture: "return-through-bounded-workbench",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("runtime-workbench-session-ledger://", sessionLedger.SessionLedgerHandle, StringComparison.Ordinal);
        Assert.Equal("runtime-workbench-session-ledger-bound", sessionLedger.ReasonCode);
        Assert.Equal("bounded-session-open", sessionLedger.SessionState);
        Assert.Equal("return-through-bounded-workbench", sessionLedger.ReturnPosture);
        Assert.Contains("questioning-event-log", sessionLedger.AdmittedLanes);
        Assert.Contains("deep-cryptic-export", sessionLedger.WithheldLanes);
        Assert.Single(sessionLedger.SessionEvents);
        Assert.Single(sessionLedger.BoundaryConditions);
    }

    [Fact]
    public void CreateDayDreamCollapseAndCrypticDepthReturn_TrackResidueAndContinuity()
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
                "candidate-voice-form",
                "boundary-aware-question",
                "soft-collapse-probe"
            ],
            nonFinalOutputs:
            [
                "candidate-voice-fragment",
                "non-final-boundary-trace"
            ],
            timestampUtc: FixedTimestamp);
        var depthGate = workbenchService.CreateSelfRootedCrypticDepthGate(
            threadBirth,
            workbench,
            dayDreamTier,
            timestampUtc: FixedTimestamp);
        var sessionLedger = workbenchService.CreateRuntimeWorkbenchSessionLedger(
            workbench,
            dayDreamTier,
            depthGate,
            sessionEvents:
            [
                SanctuaryWorkbenchProjector.CreateSessionEvent(
                    workbench.CMEId,
                    workbench.WorkbenchHandle,
                    "questioning",
                    "open",
                    "field-stabilizing",
                    coherencePreserving: true,
                    hiddenAssumptionDenied: true,
                    description: "what structure is still forming here?",
                    timestampUtc: FixedTimestamp)
            ],
            timestampUtc: FixedTimestamp);
        var collapseBoundary = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "day-dream-nonfinal-retention",
            failureClass: "exploratory-boundary",
            triggerPredicate: "collapse-keeps-one-output-non-final",
            continuityRequirement: "preserve-exploratory-provenance-on-collapse",
            permissionState: "collapse-admissible",
            notes: "bounded collapse may keep a non-final trace without inflating it into deep truth.");
        var collapseResidue = SanctuaryWorkbenchProjector.CreateResidueMarker(
            workbench.CMEId,
            sessionLedger.SessionLedgerHandle,
            markerCode: "non-final-trace",
            residueClass: "exploratory-residue",
            carryDisposition: "carry-forward-observe",
            clearedForAmenableLane: true,
            notes: "non-final exploratory residue remains visible but bounded.");

        var collapseReceipt = workbenchService.CreateDayDreamCollapseReceipt(
            sessionLedger,
            dayDreamTier,
            boundedOutputs:
            [
                "bounded-voice-fragment",
                "workbench-question-path"
            ],
            remainingNonFinalOutputs:
            [
                "non-final-boundary-trace"
            ],
            boundaryConditions: [collapseBoundary],
            residueMarkers: [collapseResidue],
            timestampUtc: FixedTimestamp);
        var continuityMarker = SanctuaryWorkbenchProjector.CreateContinuityMarker(
            workbench.CMEId,
            sessionLedger.SessionLedgerHandle,
            markerCode: "self-rooted-return",
            continuityClass: "depth-return",
            sourceHandle: depthGate.CrypticBiadRootHandle,
            carryDisposition: "carry-forward",
            notes: "self-rooted return preserved CME continuity.");
        var returnResidue = SanctuaryWorkbenchProjector.CreateResidueMarker(
            workbench.CMEId,
            sessionLedger.SessionLedgerHandle,
            markerCode: "depth-trace-cleared",
            residueClass: "return-residue",
            carryDisposition: "cleared-on-return",
            clearedForAmenableLane: true,
            notes: "depth residue cleared before re-entry to the amenable lane.");
        var returnBoundary = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "return-without-bleed",
            failureClass: "return-constraint",
            triggerPredicate: "deep-work-reenters-amenable-lane",
            continuityRequirement: "clear-shared-amenable-lane-before-reentry",
            permissionState: "return-admissible",
            notes: "deep return is lawful only when shared amenable lanes are clear.");

        var returnReceipt = workbenchService.CreateCrypticDepthReturnReceipt(
            sessionLedger,
            depthGate,
            continuityMarkers: [continuityMarker],
            residueMarkers: [returnResidue],
            boundaryConditions: [returnBoundary],
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("day-dream-collapse-receipt://", collapseReceipt.CollapseReceiptHandle, StringComparison.Ordinal);
        Assert.Equal("day-dream-collapse-receipt-bound", collapseReceipt.ReasonCode);
        Assert.True(collapseReceipt.ExploratoryProvenancePreserved);
        Assert.Equal(2, collapseReceipt.BoundedOutputs.Count);
        Assert.Single(collapseReceipt.RemainingNonFinalOutputs);
        Assert.Single(collapseReceipt.ResidueMarkers);

        Assert.StartsWith("cryptic-depth-return-receipt://", returnReceipt.ReturnReceiptHandle, StringComparison.Ordinal);
        Assert.Equal("cryptic-depth-return-receipt-bound", returnReceipt.ReasonCode);
        Assert.True(returnReceipt.ReturnedCleanly);
        Assert.True(returnReceipt.SharedAmenableLaneClear);
        Assert.False(returnReceipt.IdentityBleedDetected);
        Assert.Single(returnReceipt.ContinuityMarkers);
        Assert.Single(returnReceipt.ResidueMarkers);
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
