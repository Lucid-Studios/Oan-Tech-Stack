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

    [Fact]
    public void CreateLocalHostSanctuaryResidencyEnvelopeAndReadinessLedger_KeepHabitationBounded()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (workbench, sessionLedger, returnReceipt, localityWitness) = CreateHabitationBundle();

        var residencyEnvelope = workbenchService.CreateLocalHostSanctuaryResidencyEnvelope(
            workbench,
            sessionLedger,
            returnReceipt,
            localityWitness,
            residencyState: "local-host-sanctuary-residency-envelope-ready",
            timestampUtc: FixedTimestamp);
        var readinessLedger = workbenchService.CreateRuntimeHabitationReadinessLedger(
            residencyEnvelope,
            sessionLedger,
            habitationState: "bounded-habitation-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("local-host-sanctuary-residency-envelope://", residencyEnvelope.ResidencyEnvelopeHandle, StringComparison.Ordinal);
        Assert.Equal("local-host-sanctuary-residency-envelope-bound", residencyEnvelope.ReasonCode);
        Assert.Equal("bounded-local-sanctuary-residency", residencyEnvelope.ResidencyClass);
        Assert.Equal("local-host-sanctuary-residency-envelope-ready", residencyEnvelope.ResidencyState);
        Assert.Equal(3, residencyEnvelope.HostLocalResources.Count);
        Assert.Equal(3, residencyEnvelope.AdmittedResidencyLanes.Count);
        Assert.Equal(3, residencyEnvelope.WithheldResidencyLanes.Count);
        Assert.True(residencyEnvelope.BondedReleaseDenied);
        Assert.True(residencyEnvelope.PublicationMaturityDenied);
        Assert.True(residencyEnvelope.MosBearingDepthDenied);

        Assert.StartsWith("runtime-habitation-readiness-ledger://", readinessLedger.ReadinessLedgerHandle, StringComparison.Ordinal);
        Assert.Equal("runtime-habitation-readiness-ledger-bound", readinessLedger.ReasonCode);
        Assert.Equal("bounded-habitation-ready", readinessLedger.HabitationState);
        Assert.Equal("bounded-recurring-local-habitation", readinessLedger.HabitationClass);
        Assert.Equal(4, readinessLedger.ReadyConditions.Count);
        Assert.Equal(3, readinessLedger.WithheldConditions.Count);
        Assert.True(readinessLedger.RecurringWorkReady);
        Assert.True(readinessLedger.ReturnLawBound);
        Assert.True(readinessLedger.BondedReleaseDenied);
        Assert.True(readinessLedger.PublicationMaturityDenied);
        Assert.True(readinessLedger.MosBearingDepthDenied);
    }

    [Fact]
    public void CreateBoundedInhabitationLaunchRehearsal_BindsEntryAndReturnClosure()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (workbench, sessionLedger, returnReceipt, localityWitness) = CreateHabitationBundle();
        var residencyEnvelope = workbenchService.CreateLocalHostSanctuaryResidencyEnvelope(
            workbench,
            sessionLedger,
            returnReceipt,
            localityWitness,
            residencyState: "local-host-sanctuary-residency-envelope-ready",
            timestampUtc: FixedTimestamp);
        var readinessLedger = workbenchService.CreateRuntimeHabitationReadinessLedger(
            residencyEnvelope,
            sessionLedger,
            habitationState: "bounded-habitation-ready",
            timestampUtc: FixedTimestamp);

        var launchRehearsal = workbenchService.CreateBoundedInhabitationLaunchRehearsal(
            residencyEnvelope,
            readinessLedger,
            sessionLedger,
            returnReceipt,
            launchState: "bounded-inhabitation-launch-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("bounded-inhabitation-launch-rehearsal://", launchRehearsal.LaunchRehearsalHandle, StringComparison.Ordinal);
        Assert.Equal("bounded-inhabitation-launch-rehearsal-bound", launchRehearsal.ReasonCode);
        Assert.Equal("bounded-inhabitation-launch-ready", launchRehearsal.LaunchState);
        Assert.Equal(3, launchRehearsal.EntryConditions.Count);
        Assert.Equal(3, launchRehearsal.DeniedLanes.Count);
        Assert.Equal("dissolution-witnessed", launchRehearsal.ReturnClosureState);
        Assert.True(launchRehearsal.LaunchBounded);
        Assert.True(launchRehearsal.ReturnClosureWitnessed);
        Assert.True(launchRehearsal.AmbientBondDenied);
        Assert.True(launchRehearsal.PublicationPromotionDenied);
    }

    [Fact]
    public void CreateInquirySessionDisciplineSurface_BindsQuestioningAndSilenceInsideBoundedHabitation()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (readinessLedger, sessionLedger, collapseReceipt, crypticReturnReceipt, _, _, _) = CreateInquiryBundle();

        var inquirySurface = workbenchService.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            crypticReturnReceipt,
            inquiryState: "inquiry-session-discipline-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("inquiry-session-discipline-surface://", inquirySurface.InquirySurfaceHandle, StringComparison.Ordinal);
        Assert.Equal("inquiry-session-discipline-surface-bound", inquirySurface.ReasonCode);
        Assert.Equal("inquiry-session-discipline-ready", inquirySurface.InquiryState);
        Assert.Equal(4, inquirySurface.InquiryStances.Count);
        Assert.Contains("challenge", inquirySurface.InquiryStances);
        Assert.Equal(3, inquirySurface.AssumptionExposureModes.Count);
        Assert.Equal(2, inquirySurface.SilenceDispositions.Count);
        Assert.True(inquirySurface.ChamberNativeInquiryBound);
        Assert.True(inquirySurface.HiddenPressureDenied);
        Assert.True(inquirySurface.PrematureGelPromotionDenied);
    }

    [Fact]
    public void CreateBoundaryConditionLedgerAndCoherenceGainWitness_CarryForwardConstraintMemory()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var (readinessLedger, sessionLedger, collapseReceipt, crypticReturnReceipt, _, _, _) = CreateInquiryBundle();
        var inquirySurface = workbenchService.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            crypticReturnReceipt,
            timestampUtc: FixedTimestamp);

        var boundaryLedger = workbenchService.CreateBoundaryConditionLedger(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            collapseReceipt,
            crypticReturnReceipt,
            ledgerState: "boundary-condition-ledger-ready",
            timestampUtc: FixedTimestamp);
        var coherenceWitness = workbenchService.CreateCoherenceGainWitnessReceipt(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            boundaryLedger,
            witnessState: "coherence-gain-witness-receipt-ready",
            coherenceState: "coherence-gain-witnessed",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("boundary-condition-ledger://", boundaryLedger.BoundaryLedgerHandle, StringComparison.Ordinal);
        Assert.Equal("boundary-condition-ledger-bound", boundaryLedger.ReasonCode);
        Assert.Equal("boundary-condition-ledger-ready", boundaryLedger.LedgerState);
        Assert.Equal(3, boundaryLedger.RetainedBoundaryConditions.Count);
        Assert.Equal(3, boundaryLedger.ContinuityRequirements.Count);
        Assert.Equal(3, boundaryLedger.WithheldCrossings.Count);
        Assert.True(boundaryLedger.BoundaryMemoryCarriedForward);
        Assert.True(boundaryLedger.FailurePunishmentDenied);
        Assert.False(boundaryLedger.IdentityBleedDetected);

        Assert.StartsWith("coherence-gain-witness-receipt://", coherenceWitness.CoherenceWitnessHandle, StringComparison.Ordinal);
        Assert.Equal("coherence-gain-witness-receipt-bound", coherenceWitness.ReasonCode);
        Assert.Equal("coherence-gain-witness-receipt-ready", coherenceWitness.WitnessState);
        Assert.Equal("coherence-gain-witnessed", coherenceWitness.CoherenceState);
        Assert.Equal(3, coherenceWitness.CoherencePreservingEventCount);
        Assert.Equal(3, coherenceWitness.HiddenAssumptionDeniedCount);
        Assert.Equal(3, coherenceWitness.BoundaryConditionCount);
        Assert.True(coherenceWitness.SharedIntelligibilityPreserved);
        Assert.True(coherenceWitness.AdmissibilitySpacePreserved);
        Assert.False(coherenceWitness.PrematureClosureDetected);
    }

    [Fact]
    public void CreateOperatorInquirySelectionEnvelope_BindsBoundaryAwareSelectionAcrossTheBond()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var reachService = new GovernedReachRealizationService();
        var (readinessLedger, sessionLedger, collapseReceipt, crypticReturnReceipt, rehearsal, _, localityWitness) = CreateInquiryBundle();
        var inquirySurface = workbenchService.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            crypticReturnReceipt,
            timestampUtc: FixedTimestamp);
        var boundaryLedger = workbenchService.CreateBoundaryConditionLedger(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            collapseReceipt,
            crypticReturnReceipt,
            timestampUtc: FixedTimestamp);
        var coherenceWitness = workbenchService.CreateCoherenceGainWitnessReceipt(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            boundaryLedger,
            timestampUtc: FixedTimestamp);
        var operatorSelection = reachService.CreateOperatorInquirySelectionEnvelope(
            rehearsal,
            localityWitness,
            inquirySurface,
            boundaryLedger,
            coherenceWitness,
            envelopeState: "operator-inquiry-selection-envelope-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("operator-inquiry-selection-envelope://", operatorSelection.EnvelopeHandle, StringComparison.Ordinal);
        Assert.Equal("operator-inquiry-selection-envelope-bound", operatorSelection.ReasonCode);
        Assert.Equal("operator-inquiry-selection-envelope-ready", operatorSelection.EnvelopeState);
        Assert.Equal("Operator.actual", operatorSelection.OperatorActualLocality);
        Assert.Equal(4, operatorSelection.AvailableInquiryStances.Count);
        Assert.Equal(3, operatorSelection.KnownBoundaryWarnings.Count);
        Assert.Equal(3, operatorSelection.LawfulUseConditions.Count);
        Assert.True(operatorSelection.ProtectedInteriorityDenied);
        Assert.True(operatorSelection.LocalityBypassDenied);
        Assert.True(operatorSelection.RawGrantDenied);
    }

    [Fact]
    public void CreateBondedCrucibleSessionRehearsalAndSharedBoundaryMemory_PreserveSharedUncertainty()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var reachService = new GovernedReachRealizationService();
        var (readinessLedger, sessionLedger, collapseReceipt, crypticReturnReceipt, rehearsal, reachReturnReceipt, localityWitness) = CreateInquiryBundle();
        var inquirySurface = workbenchService.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            crypticReturnReceipt,
            timestampUtc: FixedTimestamp);
        var boundaryLedger = workbenchService.CreateBoundaryConditionLedger(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            collapseReceipt,
            crypticReturnReceipt,
            timestampUtc: FixedTimestamp);
        var coherenceWitness = workbenchService.CreateCoherenceGainWitnessReceipt(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            boundaryLedger,
            timestampUtc: FixedTimestamp);
        var operatorSelection = reachService.CreateOperatorInquirySelectionEnvelope(
            rehearsal,
            localityWitness,
            inquirySurface,
            boundaryLedger,
            coherenceWitness,
            timestampUtc: FixedTimestamp);

        var crucibleRehearsal = reachService.CreateBondedCrucibleSessionRehearsal(
            rehearsal,
            operatorSelection,
            boundaryLedger,
            coherenceWitness,
            rehearsalState: "bonded-crucible-session-rehearsal-ready",
            timestampUtc: FixedTimestamp);
        var sharedBoundaryMemory = reachService.CreateSharedBoundaryMemoryLedger(
            crucibleRehearsal,
            boundaryLedger,
            reachReturnReceipt,
            localityWitness,
            ledgerState: "shared-boundary-memory-ledger-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("bonded-crucible-session-rehearsal://", crucibleRehearsal.RehearsalHandle, StringComparison.Ordinal);
        Assert.Equal("bonded-crucible-session-rehearsal-bound", crucibleRehearsal.ReasonCode);
        Assert.Equal("bonded-crucible-session-rehearsal-ready", crucibleRehearsal.RehearsalState);
        Assert.Equal("shared-uncertainty-bounded-crucible", crucibleRehearsal.SharedUnknownClass);
        Assert.Equal(3, crucibleRehearsal.SelectedInquiryStances.Count);
        Assert.Equal(3, crucibleRehearsal.SharedUnknownFacets.Count);
        Assert.Equal(3, crucibleRehearsal.CoordinationHoldCount);
        Assert.Equal(3, crucibleRehearsal.ExposedBoundaryCount);
        Assert.True(crucibleRehearsal.PreScriptedAnswerDenied);
        Assert.True(crucibleRehearsal.RemoteDominanceDenied);

        Assert.StartsWith("shared-boundary-memory-ledger://", sharedBoundaryMemory.LedgerHandle, StringComparison.Ordinal);
        Assert.Equal("shared-boundary-memory-ledger-bound", sharedBoundaryMemory.ReasonCode);
        Assert.Equal("shared-boundary-memory-ledger-ready", sharedBoundaryMemory.LedgerState);
        Assert.Equal(3, sharedBoundaryMemory.SharedBoundaryCodes.Count);
        Assert.Equal(3, sharedBoundaryMemory.SharedContinuityRequirements.Count);
        Assert.Equal(3, sharedBoundaryMemory.WithheldCommonPropertyClaims.Count);
        Assert.True(sharedBoundaryMemory.LocalityProvenancePreserved);
        Assert.False(sharedBoundaryMemory.IdentityBleedDetected);
        Assert.True(sharedBoundaryMemory.AmbientCommonPropertyDenied);
    }

    [Fact]
    public void CreateContinuityUnderPressureLedger_RetainsWhatHeldUnderSharedUncertainty()
    {
        var reachService = new GovernedReachRealizationService();
        var (_, _, _, _, _, _, coherenceWitness, _, crucibleRehearsal, sharedBoundaryMemory, _) = CreateCrucibleBundle();

        var continuityLedger = reachService.CreateContinuityUnderPressureLedger(
            crucibleRehearsal,
            sharedBoundaryMemory,
            coherenceWitness,
            ledgerState: "continuity-under-pressure-ledger-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("continuity-under-pressure-ledger://", continuityLedger.LedgerHandle, StringComparison.Ordinal);
        Assert.Equal("continuity-under-pressure-ledger-bound", continuityLedger.ReasonCode);
        Assert.Equal("continuity-under-pressure-ledger-ready", continuityLedger.LedgerState);
        Assert.Equal(3, continuityLedger.HeldContinuities.Count);
        Assert.Equal(3, continuityLedger.PartialContinuities.Count);
        Assert.Equal(3, continuityLedger.RequiredPreservations.Count);
        Assert.Equal(3, continuityLedger.BoundaryPressureCount);
        Assert.True(continuityLedger.FluentSuccessDenied);
    }

    [Fact]
    public void CreateExpressiveDeformationAndMutualIntelligibilityWitness_PreserveRecognizableDifference()
    {
        var reachService = new GovernedReachRealizationService();
        var (_, _, _, _, _, _, coherenceWitness, operatorSelection, crucibleRehearsal, sharedBoundaryMemory, localityWitness) = CreateCrucibleBundle();
        var continuityLedger = reachService.CreateContinuityUnderPressureLedger(
            crucibleRehearsal,
            sharedBoundaryMemory,
            coherenceWitness,
            timestampUtc: FixedTimestamp);

        var deformationReceipt = reachService.CreateExpressiveDeformationReceipt(
            crucibleRehearsal,
            operatorSelection,
            continuityLedger,
            sharedBoundaryMemory,
            receiptState: "expressive-deformation-receipt-ready",
            timestampUtc: FixedTimestamp);
        var mutualWitness = reachService.CreateMutualIntelligibilityWitness(
            crucibleRehearsal,
            continuityLedger,
            deformationReceipt,
            localityWitness,
            witnessState: "mutual-intelligibility-witness-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("expressive-deformation-receipt://", deformationReceipt.ReceiptHandle, StringComparison.Ordinal);
        Assert.Equal("expressive-deformation-receipt-bound", deformationReceipt.ReasonCode);
        Assert.Equal("expressive-deformation-receipt-ready", deformationReceipt.ReceiptState);
        Assert.Equal("adaptive-refinement-with-bounded-strain", deformationReceipt.DeformationClass);
        Assert.Equal(3, deformationReceipt.ChangedExpressions.Count);
        Assert.Equal(3, deformationReceipt.RecognizableContinuities.Count);
        Assert.Equal(3, deformationReceipt.FractureBoundaries.Count);
        Assert.True(deformationReceipt.AdaptiveRefinementPreserved);
        Assert.False(deformationReceipt.IdentityCollapseDetected);

        Assert.StartsWith("mutual-intelligibility-witness://", mutualWitness.WitnessHandle, StringComparison.Ordinal);
        Assert.Equal("mutual-intelligibility-witness-bound", mutualWitness.ReasonCode);
        Assert.Equal("mutual-intelligibility-witness-ready", mutualWitness.WitnessState);
        Assert.Equal("mutual-intelligibility-preserved", mutualWitness.SharedUnderstandingState);
        Assert.Equal(3, mutualWitness.HeldIntelligibilityCount);
        Assert.Equal(3, mutualWitness.NarrowedIntelligibilityCount);
        Assert.Equal(3, mutualWitness.BrokenIntelligibilityCount);
        Assert.True(mutualWitness.SamenessCollapseDenied);
        Assert.False(mutualWitness.OpaqueDivergenceDetected);
    }

    [Fact]
    public void CreateInquiryPatternContinuityAndBoundaryPairLedgers_RetainReusableInquiryMemory()
    {
        var reachService = new GovernedReachRealizationService();
        var (_, _, _, _, _, _, localityWitness, sharedBoundaryMemory, continuityLedger, deformationReceipt, mutualWitness) = CreatePressureBundle();
        var (_, _, _, _, _, _, _, operatorSelection, _, _, _) = CreateCrucibleBundle();

        var inquiryPatternLedger = reachService.CreateInquiryPatternContinuityLedger(
            operatorSelection,
            continuityLedger,
            mutualWitness,
            sharedBoundaryMemory,
            ledgerState: "inquiry-pattern-continuity-ledger-ready",
            timestampUtc: FixedTimestamp);
        var boundaryPairLedger = reachService.CreateQuestioningBoundaryPairLedger(
            operatorSelection,
            continuityLedger,
            deformationReceipt,
            sharedBoundaryMemory,
            ledgerState: "questioning-boundary-pair-ledger-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("inquiry-pattern-continuity-ledger://", inquiryPatternLedger.LedgerHandle, StringComparison.Ordinal);
        Assert.Equal("inquiry-pattern-continuity-ledger-bound", inquiryPatternLedger.ReasonCode);
        Assert.Equal("inquiry-pattern-continuity-ledger-ready", inquiryPatternLedger.LedgerState);
        Assert.Equal(3, inquiryPatternLedger.ReusableInquiryPatterns.Count);
        Assert.Equal(3, inquiryPatternLedger.TriggerConditions.Count);
        Assert.Equal(3, inquiryPatternLedger.PreservedConstraints.Count);
        Assert.Equal(3, inquiryPatternLedger.BoundaryPairCount);
        Assert.True(inquiryPatternLedger.IdentityBleedDenied);

        Assert.StartsWith("questioning-boundary-pair-ledger://", boundaryPairLedger.LedgerHandle, StringComparison.Ordinal);
        Assert.Equal("questioning-boundary-pair-ledger-bound", boundaryPairLedger.ReasonCode);
        Assert.Equal("questioning-boundary-pair-ledger-ready", boundaryPairLedger.LedgerState);
        Assert.Equal(3, boundaryPairLedger.InquiryPatterns.Count);
        Assert.Equal(3, boundaryPairLedger.SupportingBoundaries.Count);
        Assert.Equal(3, boundaryPairLedger.BoundaryConstraints.Count);
        Assert.Equal(3, boundaryPairLedger.OverreachWarnings.Count);
        Assert.True(boundaryPairLedger.ConstraintMemoryPreserved);
    }

    [Fact]
    public void CreateCarryForwardInquirySelectionSurface_BindsLocalitySafeReuse()
    {
        var reachService = new GovernedReachRealizationService();
        var (_, _, _, _, _, _, localityWitness, sharedBoundaryMemory, continuityLedger, deformationReceipt, mutualWitness) = CreatePressureBundle();
        var (_, _, _, _, _, _, _, operatorSelection, _, _, _) = CreateCrucibleBundle();
        var inquiryPatternLedger = reachService.CreateInquiryPatternContinuityLedger(
            operatorSelection,
            continuityLedger,
            mutualWitness,
            sharedBoundaryMemory,
            timestampUtc: FixedTimestamp);
        var boundaryPairLedger = reachService.CreateQuestioningBoundaryPairLedger(
            operatorSelection,
            continuityLedger,
            deformationReceipt,
            sharedBoundaryMemory,
            timestampUtc: FixedTimestamp);

        var carryForwardSurface = reachService.CreateCarryForwardInquirySelectionSurface(
            inquiryPatternLedger,
            boundaryPairLedger,
            operatorSelection,
            localityWitness,
            surfaceState: "carry-forward-inquiry-selection-surface-ready",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("carry-forward-inquiry-selection-surface://", carryForwardSurface.SurfaceHandle, StringComparison.Ordinal);
        Assert.Equal("carry-forward-inquiry-selection-surface-bound", carryForwardSurface.ReasonCode);
        Assert.Equal("carry-forward-inquiry-selection-surface-ready", carryForwardSurface.SurfaceState);
        Assert.Equal(3, carryForwardSurface.AvailableCarryForwardPatterns.Count);
        Assert.Equal(3, carryForwardSurface.AdmittedReuseConditions.Count);
        Assert.Equal(3, carryForwardSurface.WithheldReuseWarnings.Count);
        Assert.True(carryForwardSurface.LocalitySafeReview);
        Assert.True(carryForwardSurface.AmbientHabitDenied);
    }

    private static readonly DateTimeOffset FixedTimestamp = new(2026, 3, 22, 12, 0, 0, TimeSpan.Zero);

    private static (RuntimeHabitationReadinessLedgerReceipt ReadinessLedger, RuntimeWorkbenchSessionLedger SessionLedger, DayDreamCollapseReceipt CollapseReceipt, CrypticDepthReturnReceipt CrypticReturnReceipt, BondedCoWorkSessionRehearsalReceipt BondedCoWorkRehearsal, ReachReturnDissolutionReceipt ReachReturnReceipt, LocalityDistinctionWitnessLedgerReceipt LocalityWitness) CreateInquiryBundle()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var reachService = new GovernedReachRealizationService();
        var (threadBirth, utilitySurface, realization, localityLedger) = CreateReachProjectionBundle();
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
                "clarifying-approach-pattern",
                "silence-before-closure",
                "boundary-aware-probe"
            ],
            nonFinalOutputs:
            [
                "candidate-questioning-path",
                "non-final-silence-trace"
            ],
            timestampUtc: FixedTimestamp);
        var depthGate = workbenchService.CreateSelfRootedCrypticDepthGate(
            threadBirth,
            workbench,
            dayDreamTier,
            timestampUtc: FixedTimestamp);
        var sessionBoundary = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "premature-closure-boundary",
            failureClass: "coordination-fracture",
            triggerPredicate: "question-demands-conclusion-before-readiness",
            continuityRequirement: "preserve-admissibility-space-before-closure",
            permissionState: "withhold-premature-closure",
            notes: "session questioning must not compress the chamber into conclusion before the field is ready.");
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
                    "clarify",
                    "coherence-gain",
                    coherencePreserving: true,
                    hiddenAssumptionDenied: true,
                    description: "what would need to be true for this chamber to remain intelligible?",
                    timestampUtc: FixedTimestamp),
                SanctuaryWorkbenchProjector.CreateSessionEvent(
                    workbench.CMEId,
                    workbench.WorkbenchHandle,
                    "silence",
                    "open",
                    "forming-state-held",
                    coherencePreserving: true,
                    hiddenAssumptionDenied: true,
                    description: "hold the forming structure without forcing it into premature closure.",
                    timestampUtc: FixedTimestamp),
                SanctuaryWorkbenchProjector.CreateSessionEvent(
                    workbench.CMEId,
                    workbench.WorkbenchHandle,
                    "questioning",
                    "probe",
                    "boundary-revealed",
                    coherencePreserving: true,
                    hiddenAssumptionDenied: true,
                    description: "which boundary is protecting continuity rather than blocking motion?",
                    timestampUtc: FixedTimestamp)
            ],
            boundaryConditions: [sessionBoundary],
            timestampUtc: FixedTimestamp);
        var collapseBoundary = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "non-final-question-trace",
            failureClass: "exploratory-boundary",
            triggerPredicate: "collapse-keeps-one-question-path-non-final",
            continuityRequirement: "retain-non-final-inquiry-as-bounded-residue",
            permissionState: "withhold-non-final-promotion",
            notes: "collapse may keep one inquiry path visible without promoting it into stable operator truth.");
        var collapseResidue = SanctuaryWorkbenchProjector.CreateResidueMarker(
            workbench.CMEId,
            sessionLedger.SessionLedgerHandle,
            markerCode: "questioning-trace",
            residueClass: "exploratory-inquiry-residue",
            carryDisposition: "carry-forward-observe",
            clearedForAmenableLane: true,
            notes: "exploratory questioning residue remains visible while staying non-final.");
        var collapseReceipt = workbenchService.CreateDayDreamCollapseReceipt(
            sessionLedger,
            dayDreamTier,
            boundedOutputs:
            [
                "bounded-questioning-lane",
                "coherence-checkpoint"
            ],
            remainingNonFinalOutputs:
            [
                "non-final-silence-trace"
            ],
            boundaryConditions: [collapseBoundary],
            residueMarkers: [collapseResidue],
            timestampUtc: FixedTimestamp);
        var returnBoundary = SanctuaryWorkbenchProjector.CreateBoundaryCondition(
            workbench.CMEId,
            workbench.WorkbenchHandle,
            boundaryCode: "depth-return-boundary-memory",
            failureClass: "return-constraint",
            triggerPredicate: "self-rooted-depth-returns-to-questioning-lane",
            continuityRequirement: "carry-boundary-memory-without-identity-bleed",
            permissionState: "return-boundedly-with-memory",
            notes: "self-rooted depth may return only if the chamber carries memory of its boundaries without bleeding identity across lanes.");
        var returnResidue = SanctuaryWorkbenchProjector.CreateResidueMarker(
            workbench.CMEId,
            sessionLedger.SessionLedgerHandle,
            markerCode: "depth-trace-cleared",
            residueClass: "return-residue",
            carryDisposition: "cleared-on-return",
            clearedForAmenableLane: true,
            notes: "depth residue clears before re-entry to the bounded inquiry lane.");
        var returnReceipt = workbenchService.CreateCrypticDepthReturnReceipt(
            sessionLedger,
            depthGate,
            continuityMarkers:
            [
                SanctuaryWorkbenchProjector.CreateContinuityMarker(
                    workbench.CMEId,
                    sessionLedger.SessionLedgerHandle,
                    markerCode: "self-rooted-inquiry-return",
                    continuityClass: "depth-return",
                    sourceHandle: depthGate.CrypticBiadRootHandle,
                    carryDisposition: "carry-forward",
                    notes: "self-rooted return preserved continuity through inquiry."),
                SanctuaryWorkbenchProjector.CreateContinuityMarker(
                    workbench.CMEId,
                    sessionLedger.SessionLedgerHandle,
                    markerCode: "boundary-memory-carried",
                    continuityClass: "boundary-memory",
                    sourceHandle: collapseReceipt.CollapseReceiptHandle,
                    carryDisposition: "carry-forward",
                    notes: "boundary memory was preserved across collapse and return.")
            ],
            residueMarkers: [returnResidue],
            boundaryConditions: [returnBoundary],
            timestampUtc: FixedTimestamp);
        var rehearsal = reachService.CreateBondedCoWorkSessionRehearsal(
            sessionLedger,
            utilitySurface,
            realization,
            localityLedger,
            sharedWorkLoop:
            [
                "shared-inquiry-loop",
                "boundary-memory-check",
                "return-closure-pass"
            ],
            duplexPredicateLanes:
            [
                "work-predicate",
                "governance-predicate"
            ],
            withheldLanes:
            [
                "ambient-bond-persistence",
                "publication-promotion",
                "mos-bearing-depth"
            ],
            timestampUtc: FixedTimestamp);
        var reachReturnReceipt = reachService.CreateReachReturnDissolutionReceipt(
            rehearsal,
            realization,
            timestampUtc: FixedTimestamp);
        var localityWitness = reachService.CreateLocalityDistinctionWitnessLedger(
            rehearsal,
            reachReturnReceipt,
            sharedSurfaces:
            [
                "bounded-inquiry-loop",
                "boundary-memory-carry",
                "return-dissolution-law"
            ],
            sanctuaryLocalSurfaces:
            [
                "sanctuary-runtime-workbench",
                "local-host-residency"
            ],
            operatorLocalSurfaces:
            [
                "operator-actual-rehearsal",
                "bonded-participation-ledger"
            ],
            withheldSurfaces:
            [
                "ambient-bond-persistence",
                "publication-promotion",
                "mos-bearing-depth"
            ],
            timestampUtc: FixedTimestamp);
        var residencyEnvelope = workbenchService.CreateLocalHostSanctuaryResidencyEnvelope(
            workbench,
            sessionLedger,
            reachReturnReceipt,
            localityWitness,
            residencyState: "local-host-sanctuary-residency-envelope-ready",
            timestampUtc: FixedTimestamp);
        var readinessLedger = workbenchService.CreateRuntimeHabitationReadinessLedger(
            residencyEnvelope,
            sessionLedger,
            habitationState: "bounded-habitation-ready",
            timestampUtc: FixedTimestamp);

        return (readinessLedger, sessionLedger, collapseReceipt, returnReceipt, rehearsal, reachReturnReceipt, localityWitness);
    }

    private static (
        RuntimeHabitationReadinessLedgerReceipt ReadinessLedger,
        RuntimeWorkbenchSessionLedger SessionLedger,
        DayDreamCollapseReceipt CollapseReceipt,
        CrypticDepthReturnReceipt ReturnReceipt,
        InquirySessionDisciplineSurfaceReceipt InquirySurface,
        BoundaryConditionLedgerReceipt BoundaryLedger,
        CoherenceGainWitnessReceipt CoherenceWitness,
        OperatorInquirySelectionEnvelopeReceipt OperatorSelection,
        BondedCrucibleSessionRehearsalReceipt CrucibleRehearsal,
        SharedBoundaryMemoryLedgerReceipt SharedBoundaryMemory,
        LocalityDistinctionWitnessLedgerReceipt LocalityWitness) CreateCrucibleBundle()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var reachService = new GovernedReachRealizationService();
        var (readinessLedger, sessionLedger, collapseReceipt, returnReceipt, rehearsal, reachReturnReceipt, localityWitness) = CreateInquiryBundle();
        var inquirySurface = workbenchService.CreateInquirySessionDisciplineSurface(
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            returnReceipt,
            timestampUtc: FixedTimestamp);
        var boundaryLedger = workbenchService.CreateBoundaryConditionLedger(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            collapseReceipt,
            returnReceipt,
            timestampUtc: FixedTimestamp);
        var coherenceWitness = workbenchService.CreateCoherenceGainWitnessReceipt(
            readinessLedger,
            sessionLedger,
            inquirySurface,
            boundaryLedger,
            timestampUtc: FixedTimestamp);
        var operatorSelection = reachService.CreateOperatorInquirySelectionEnvelope(
            rehearsal,
            localityWitness,
            inquirySurface,
            boundaryLedger,
            coherenceWitness,
            timestampUtc: FixedTimestamp);
        var crucibleRehearsal = reachService.CreateBondedCrucibleSessionRehearsal(
            rehearsal,
            operatorSelection,
            boundaryLedger,
            coherenceWitness,
            timestampUtc: FixedTimestamp);
        var sharedBoundaryMemory = reachService.CreateSharedBoundaryMemoryLedger(
            crucibleRehearsal,
            boundaryLedger,
            reachReturnReceipt,
            localityWitness,
            timestampUtc: FixedTimestamp);

        return (
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            returnReceipt,
            inquirySurface,
            boundaryLedger,
            coherenceWitness,
            operatorSelection,
            crucibleRehearsal,
            sharedBoundaryMemory,
            localityWitness);
    }

    private static (
        RuntimeHabitationReadinessLedgerReceipt ReadinessLedger,
        RuntimeWorkbenchSessionLedger SessionLedger,
        DayDreamCollapseReceipt CollapseReceipt,
        CrypticDepthReturnReceipt ReturnReceipt,
        InquirySessionDisciplineSurfaceReceipt InquirySurface,
        BoundaryConditionLedgerReceipt BoundaryLedger,
        LocalityDistinctionWitnessLedgerReceipt LocalityWitness,
        SharedBoundaryMemoryLedgerReceipt SharedBoundaryMemory,
        ContinuityUnderPressureLedgerReceipt ContinuityLedger,
        ExpressiveDeformationReceipt DeformationReceipt,
        MutualIntelligibilityWitnessReceipt MutualWitness) CreatePressureBundle()
    {
        var reachService = new GovernedReachRealizationService();
        var (readinessLedger, sessionLedger, collapseReceipt, returnReceipt, inquirySurface, boundaryLedger, coherenceWitness, operatorSelection, crucibleRehearsal, sharedBoundaryMemory, localityWitness) = CreateCrucibleBundle();
        var continuityLedger = reachService.CreateContinuityUnderPressureLedger(
            crucibleRehearsal,
            sharedBoundaryMemory,
            coherenceWitness,
            timestampUtc: FixedTimestamp);
        var deformationReceipt = reachService.CreateExpressiveDeformationReceipt(
            crucibleRehearsal,
            operatorSelection,
            continuityLedger,
            sharedBoundaryMemory,
            timestampUtc: FixedTimestamp);
        var mutualWitness = reachService.CreateMutualIntelligibilityWitness(
            crucibleRehearsal,
            continuityLedger,
            deformationReceipt,
            localityWitness,
            timestampUtc: FixedTimestamp);

        return (
            readinessLedger,
            sessionLedger,
            collapseReceipt,
            returnReceipt,
            inquirySurface,
            boundaryLedger,
            localityWitness,
            sharedBoundaryMemory,
            continuityLedger,
            deformationReceipt,
            mutualWitness);
    }

    private static (SanctuaryRuntimeWorkbenchSurfaceReceipt Workbench, RuntimeWorkbenchSessionLedger SessionLedger, ReachReturnDissolutionReceipt ReturnReceipt, LocalityDistinctionWitnessLedgerReceipt LocalityWitness) CreateHabitationBundle()
    {
        var workbenchService = new SanctuaryRuntimeWorkbenchService();
        var reachService = new GovernedReachRealizationService();
        var (threadBirth, utilitySurface, realization, localityLedger) = CreateReachProjectionBundle();
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
                "co-work-boundary-question",
                "bounded-habitation-probe",
                "return-law-check"
            ],
            nonFinalOutputs:
            [
                "candidate-bounded-launch",
                "non-final-host-trace"
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
                    "probe",
                    "bounded-habitation-coherence",
                    coherencePreserving: true,
                    hiddenAssumptionDenied: true,
                    description: "what would need to hold for bounded habitation to begin cleanly here?",
                    timestampUtc: FixedTimestamp)
            ],
            boundaryConditions:
            [
                SanctuaryWorkbenchProjector.CreateBoundaryCondition(
                    workbench.CMEId,
                    workbench.WorkbenchHandle,
                    boundaryCode: "bounded-habitation-no-release-inflation",
                    failureClass: "launch-boundary",
                    triggerPredicate: "habitation-must-not-overstate-bonded-release",
                    continuityRequirement: "keep-entry-local-and-return-witnessed",
                    permissionState: "bounded-launch-only",
                    notes: "bounded habitation may begin locally but may not claim bonded release, publication maturity, or MoS-bearing depth.")
            ],
            timestampUtc: FixedTimestamp);
        var rehearsal = reachService.CreateBondedCoWorkSessionRehearsal(
            sessionLedger,
            utilitySurface,
            realization,
            localityLedger,
            sharedWorkLoop:
            [
                "shared-questioning-loop",
                "bounded-launch-check",
                "return-closure-pass"
            ],
            duplexPredicateLanes:
            [
                "work-predicate",
                "governance-predicate"
            ],
            withheldLanes:
            [
                "ambient-bond-persistence",
                "publication-promotion",
                "mos-bearing-depth"
            ],
            timestampUtc: FixedTimestamp);
        var returnReceipt = reachService.CreateReachReturnDissolutionReceipt(
            rehearsal,
            realization,
            timestampUtc: FixedTimestamp);
        var localityWitness = reachService.CreateLocalityDistinctionWitnessLedger(
            rehearsal,
            returnReceipt,
            sharedSurfaces:
            [
                "bounded-cowork-loop",
                "return-dissolution-law",
                "workbench-session-ledger"
            ],
            sanctuaryLocalSurfaces:
            [
                "sanctuary-runtime-workbench",
                "local-host-residency"
            ],
            operatorLocalSurfaces:
            [
                "operator-actual-rehearsal",
                "bonded-participation-ledger"
            ],
            withheldSurfaces:
            [
                "ambient-bond-persistence",
                "publication-promotion",
                "mos-bearing-depth"
            ],
            timestampUtc: FixedTimestamp);

        return (workbench, sessionLedger, returnReceipt, localityWitness);
    }

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
