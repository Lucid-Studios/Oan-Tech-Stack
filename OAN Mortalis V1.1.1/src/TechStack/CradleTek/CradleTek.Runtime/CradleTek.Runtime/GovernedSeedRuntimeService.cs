using San.Common;
using San.Nexus.Control;
using San.PrimeCryptic.Services;
using San.Runtime.Materialization;
using San.State.Modulation;
using San.Trace.Persistence;
using SLI.Ingestion;
using SoulFrame.Bootstrap;
using SoulFrame.Membrane;

namespace CradleTek.Runtime;

public sealed class GovernedSeedRuntimeService
{
    private readonly IGovernedSeedSanctuaryIngressEngrammitizationService _sanctuaryIngressService;
    private readonly IGovernedSeedMembraneService _membraneService;
    private readonly IGovernedSeedSoulFrameBootstrapService _bootstrapService;
    private readonly IPrimeCrypticServiceBroker _primeCrypticServiceBroker;
    private readonly IGovernedNexusControlService _nexusControlService;
    private readonly IGovernedSeedRuntimeMaterializationService _materializationService;
    private readonly IGovernedSeedPreDomainHostLoopService _preDomainHostLoopService;
    private readonly IGovernedSeedDomainRoleGatingService _domainRoleGatingService;
    private readonly IGovernedStateModulationService _stateModulationService;
    private readonly IGovernedSeedEnvelopeTraceService _traceService;

    public GovernedSeedRuntimeService(
        IGovernedSeedSanctuaryIngressEngrammitizationService sanctuaryIngressService,
        IGovernedSeedMembraneService membraneService,
        IGovernedSeedSoulFrameBootstrapService bootstrapService,
        IPrimeCrypticServiceBroker primeCrypticServiceBroker,
        IGovernedNexusControlService nexusControlService,
        IGovernedSeedRuntimeMaterializationService materializationService,
        IGovernedSeedPreDomainHostLoopService preDomainHostLoopService,
        IGovernedSeedDomainRoleGatingService domainRoleGatingService,
        IGovernedStateModulationService stateModulationService,
        IGovernedSeedEnvelopeTraceService traceService)
    {
        _sanctuaryIngressService = sanctuaryIngressService ?? throw new ArgumentNullException(nameof(sanctuaryIngressService));
        _membraneService = membraneService ?? throw new ArgumentNullException(nameof(membraneService));
        _bootstrapService = bootstrapService ?? throw new ArgumentNullException(nameof(bootstrapService));
        _primeCrypticServiceBroker = primeCrypticServiceBroker ?? throw new ArgumentNullException(nameof(primeCrypticServiceBroker));
        _nexusControlService = nexusControlService ?? throw new ArgumentNullException(nameof(nexusControlService));
        _materializationService = materializationService ?? throw new ArgumentNullException(nameof(materializationService));
        _preDomainHostLoopService = preDomainHostLoopService ?? throw new ArgumentNullException(nameof(preDomainHostLoopService));
        _domainRoleGatingService = domainRoleGatingService ?? throw new ArgumentNullException(nameof(domainRoleGatingService));
        _stateModulationService = stateModulationService ?? throw new ArgumentNullException(nameof(stateModulationService));
        _traceService = traceService ?? throw new ArgumentNullException(nameof(traceService));
    }

    public async Task<EvaluateEnvelope> EvaluateAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default)
    {
        return await EvaluateAsync(
            agentId,
            theaterId,
            input,
            GovernedSeedIngressAccessClass.PromptInput,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<EvaluateEnvelope> EvaluateToolAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default) =>
        EvaluateAsync(agentId, theaterId, input, GovernedSeedIngressAccessClass.ToolAccess, cancellationToken);

    public Task<EvaluateEnvelope> EvaluateDataAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default) =>
        EvaluateAsync(agentId, theaterId, input, GovernedSeedIngressAccessClass.DataAccess, cancellationToken);

    private async Task<EvaluateEnvelope> EvaluateAsync(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedIngressAccessClass ingressAccessClass,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sanctuaryIngress = _sanctuaryIngressService.Prepare(agentId, theaterId, input, ingressAccessClass);
        var primeCrypticReceipt = _primeCrypticServiceBroker.DescribeResidentField(agentId, theaterId);
        var bootstrapReceipt = _bootstrapService.Bootstrap(agentId, theaterId);
        var bootstrapNexusResult = _nexusControlService.EvaluateBootstrapAdmission(primeCrypticReceipt, bootstrapReceipt);
        var bootstrapAdmissionReceipt = _materializationService.CreateBootstrapAdmissionReceipt(
            bootstrapNexusResult.Posture,
            bootstrapNexusResult.Request,
            bootstrapNexusResult.Decision);

        if (!bootstrapAdmissionReceipt.MembraneWakePermitted)
        {
            var bootstrapDeniedResult = _materializationService.MaterializeBootstrapDeniedResult(
                agentId,
                theaterId,
                input,
                sanctuaryIngress.Receipt,
                primeCrypticReceipt,
                bootstrapReceipt,
                bootstrapNexusResult.Posture,
                bootstrapNexusResult.Request,
                bootstrapNexusResult.Decision,
                bootstrapAdmissionReceipt);
            var deniedStateModulationReceipt = _stateModulationService.CreateReceipt(
                primeCrypticReceipt,
                bootstrapReceipt,
                bootstrapDeniedResult);
            var hydratedDeniedResult = _materializationService.AttachStateModulation(
                bootstrapDeniedResult,
                deniedStateModulationReceipt);

            var deniedEnvelope = _materializationService.CreateEnvelope(agentId, theaterId, hydratedDeniedResult);
            return await _traceService.TraceAsync(deniedEnvelope, hydratedDeniedResult, cancellationToken).ConfigureAwait(false);
        }

        var result = await _membraneService.EvaluateAsync(
            new GovernedSeedEvaluationRequest(
                AgentId: agentId,
                TheaterId: theaterId,
                Input: sanctuaryIngress.PreparedInput,
                AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                BootstrapReceipt: bootstrapReceipt,
                IngressAccessClass: ingressAccessClass,
                SanctuaryIngressReceipt: sanctuaryIngress.Receipt),
            cancellationToken).ConfigureAwait(false);
        var situationalContext = result.VerticalSlice.SituationalContext
            ?? throw new InvalidOperationException("SoulFrame must expose situational context before nexus evaluation.");
        var nexusResult = _nexusControlService.Evaluate(primeCrypticReceipt, bootstrapReceipt, situationalContext);
        var nexusHydratedResult = _materializationService.HydrateNexusAndPrime(
            result,
            agentId,
            theaterId,
            input,
            bootstrapReceipt,
            bootstrapAdmissionReceipt,
            primeCrypticReceipt,
            nexusResult.Posture,
            nexusResult.Request,
            nexusResult.Decision);

        var preDomainInputs = ProjectPreDomainInputs(nexusHydratedResult);
        var hostLoop = _preDomainHostLoopService.Evaluate(
            preDomainInputs.FormationReceipt,
            preDomainInputs.ListeningFrameProjection,
            preDomainInputs.CompassProjection,
            preDomainInputs.FirstPrimeReceipt,
            preDomainInputs.PrimeSeedReceipt);
        var hostLoopHydratedResult = _materializationService.AttachPreDomainHostLoop(
            nexusHydratedResult,
            preDomainInputs.FirstPrimeReceipt,
            preDomainInputs.PrimeSeedReceipt,
            hostLoop.CandidateBoundaryReceipt,
            hostLoop.HoldingInspectionReceipt,
            hostLoop.FormOrCleaveAssessment,
            hostLoop.CandidateSeparationReceipt,
            hostLoop.DuplexGovernanceReceipt,
            hostLoop.AdmissionGateReceipt,
            hostLoop.HostLoopReceipt);
        var packetGatedResult = hostLoopHydratedResult.VerticalSlice.PreDomainGovernancePacket is null
            ? hostLoopHydratedResult
            : AttachDomainRoleGating(hostLoopHydratedResult, hostLoopHydratedResult.VerticalSlice.PreDomainGovernancePacket);
        var stateModulationReceipt = _stateModulationService.CreateReceipt(primeCrypticReceipt, bootstrapReceipt, packetGatedResult);
        var hydratedResult = _materializationService.AttachStateModulation(packetGatedResult, stateModulationReceipt);

        var envelope = _materializationService.CreateEnvelope(agentId, theaterId, hydratedResult);
        return await _traceService.TraceAsync(envelope, hydratedResult, cancellationToken).ConfigureAwait(false);
    }

    private GovernedSeedEvaluationResult AttachDomainRoleGating(
        GovernedSeedEvaluationResult result,
        GovernedSeedPreDomainGovernancePacket packet)
    {
        var domainRoleGating = _domainRoleGatingService.Evaluate(packet);
        return _materializationService.AttachDomainRoleGating(
            result,
            domainRoleGating.DomainAssessment,
            domainRoleGating.RoleAssessment,
            domainRoleGating.GatingAssessment,
            domainRoleGating.Receipt);
    }

    private static PreDomainLifecycleInputs ProjectPreDomainInputs(
        GovernedSeedEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var firstRun = result.VerticalSlice.FirstRunConstitution
            ?? throw new InvalidOperationException("First-run constitution must be materialized before pre-domain host loop evaluation.");
        var livingPacket = firstRun.LivingAgentiCorePacket
            ?? CreateFallbackLivingPacket(firstRun, result);
        var listeningFrame = livingPacket.ListeningFrameProjectionPacket
            ?? CreateFallbackListeningFramePacket(firstRun, livingPacket);
        var compassProjection = livingPacket.CompassProjectionPacket
            ?? CreateFallbackCompassProjectionPacket(firstRun, livingPacket, result);
        var formationReceipt = CreateFormationReceipt(result, livingPacket, listeningFrame, compassProjection);
        var retainedPrime = CreatePrimeRetainedHistoryRecord(result, firstRun, livingPacket);
        var firstPrimeReceipt = EngineeredCognitionFirstPrimeStateEvaluator.Evaluate(
            firstRun,
            retainedPrime,
            CreateHandle("first-prime://", firstRun.ReceiptHandle, retainedPrime.RecordHandle));
        var primeSeedRequest = new PrimeSeedStateRequest(
            RequestHandle: CreateHandle("prime-seed-request://", firstPrimeReceipt.ReceiptHandle, retainedPrime.RecordHandle),
            FirstPrimeReceipt: firstPrimeReceipt,
            SeedSourceHandle: livingPacket.EngineeredCognitionHandle ?? livingPacket.LivingAgentiCoreHandle ?? firstRun.ReceiptHandle,
            SeedCarrierHandle: livingPacket.ListeningFrameHandle ?? livingPacket.PacketHandle,
            SeedContinuityHandle: retainedPrime.RecordHandle,
            SeedIntegrityHandle: formationReceipt.ReceiptHandle,
            SeedEvidenceHandles: BuildSeedEvidenceHandles(formationReceipt, retainedPrime, firstRun),
            TimestampUtc: DateTimeOffset.UtcNow);
        var primeSeedReceipt = PrimeSeedStateEvaluator.Evaluate(
            primeSeedRequest,
            CreateHandle("prime-seed://", primeSeedRequest.RequestHandle, formationReceipt.ReceiptHandle));

        return new PreDomainLifecycleInputs(
            formationReceipt,
            listeningFrame,
            compassProjection,
            firstPrimeReceipt,
            primeSeedReceipt);
    }

    private static FormationReceipt CreateFormationReceipt(
        GovernedSeedEvaluationResult result,
        FirstRunLivingAgentiCorePacket livingPacket,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection)
    {
        var orientation = new SensoryOrientationSnapshot(
            OrientationHandle: CreateHandle("orientation://", livingPacket.PacketHandle, result.Decision),
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            CompassEmbodimentHandle: compassProjection.CompassEmbodimentHandle,
            ConeBoundary: listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken
                ? AwarenessConeBoundaryKind.Outside
                : listeningFrame.IntegrityState == ListeningFrameIntegrityState.Sparse
                    ? AwarenessConeBoundaryKind.Edge
                    : AwarenessConeBoundaryKind.Inside,
            OrientationFacet: compassProjection.OrientationPosture switch
            {
                CompassOrientationPosture.Centered => CompassOrientationFacetKind.Center,
                CompassOrientationPosture.Seeking => CompassOrientationFacetKind.North,
                _ => CompassOrientationFacetKind.West
            },
            PerceptualPolarity: compassProjection.DriftState == CompassDriftState.Lost
                ? PerceptualPolarityKind.Inverted
                : PerceptualPolarityKind.Direct,
            SourceRelation: SourceRelationKind.Duplex,
            ModalityMarkers: listeningFrame.PostureMarkers,
            OrientationNotes: listeningFrame.ReviewNotes.Concat(compassProjection.ReviewNotes).Distinct(StringComparer.Ordinal).ToArray(),
            TimestampUtc: DateTimeOffset.UtcNow,
            ZedOfDeltaHandle: livingPacket.ZedOfDeltaHandle,
            ZedLocusState: listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken
                ? ZedLocusState.Lost
                : listeningFrame.IntegrityState == ListeningFrameIntegrityState.Sparse
                    ? ZedLocusState.Strained
                    : ZedLocusState.Preserved,
            OrientationIntegrity: compassProjection.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld
                ? OrientationIntegrityKind.Compromised
                : OrientationIntegrityKind.Stable,
            DeltaPressure: result.Accepted
                ? DeltaPressureKind.LoadBearing
                : DeltaPressureKind.BasinSteepening);

        var requirementStates = new List<FormationRequirementState>();

        foreach (var candidateInput in compassProjection.CandidateInputs)
        {
            requirementStates.Add(new FormationRequirementState(
                RequirementHandle: CreateHandle("formation-requirement://", candidateInput.InputHandle, candidateInput.InputKind),
                RequirementKind: candidateInput.InputKind,
                State: compassProjection.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld
                    ? RequirementStateKind.Blocked
                    : RequirementStateKind.Present,
                WhyNot: compassProjection.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld
                    ? WhyNotClassificationKind.BlockedByBoundary
                    : WhyNotClassificationKind.None,
                EvidenceHandles: [candidateInput.InputHandle],
                Notes: [candidateInput.SourceReason]));
        }

        if (result.VerticalSlice.Predicate is null)
        {
            requirementStates.Add(new FormationRequirementState(
                RequirementHandle: CreateHandle("formation-requirement://", result.VerticalSlice.PathReceipt.PathHandle, "predicate-absence"),
                RequirementKind: "predicate-surface",
                State: RequirementStateKind.Unknown,
                WhyNot: WhyNotClassificationKind.InsufficientEvidence,
                EvidenceHandles: [result.VerticalSlice.PathReceipt.PathHandle],
                Notes: ["predicate-surface-not-yet-materialized"]));
        }

        var discernedItems = compassProjection.CandidateInputs.Select(static input => input.InputHandle).ToArray();
        var knownItems = result.VerticalSlice.Predicate is null
            ? Array.Empty<string>()
            : [result.VerticalSlice.Predicate.SurfaceHandle];
        var unknownItems = discernedItems.Except(knownItems, StringComparer.Ordinal).ToArray();
        var deferredItems = result.GovernanceState == GovernedSeedEvaluationState.UnresolvedConflict
            ? unknownItems
            : Array.Empty<string>();
        var failureSignatures = result.GovernanceState == GovernedSeedEvaluationState.UnresolvedConflict
            ? new[]
            {
                new FormationFailureSignature(
                    SignatureHandle: CreateHandle("formation-failure://", result.VerticalSlice.PathReceipt.PathHandle, result.Decision),
                    SignatureKind: FailureSignatureKind.ConjunctionImbalance,
                    DeltaPressure: DeltaPressureKind.CollapsePressure,
                    EvidenceHandles: [result.VerticalSlice.PathReceipt.PathHandle],
                    Notes: [result.GovernanceTrace])
            }
            : Array.Empty<FormationFailureSignature>();

        var snapshot = new CognitiveFormationSnapshot(
            SnapshotHandle: CreateHandle("formation-snapshot://", result.VerticalSlice.PathReceipt.PathHandle, result.Decision),
            EncounterHandle: result.VerticalSlice.PathReceipt.PathHandle,
            EngineeredCognitionHandle: livingPacket.EngineeredCognitionHandle,
            Orientation: orientation,
            DiscernedItems: discernedItems,
            RequirementStates: requirementStates,
            KnownItems: knownItems,
            UnknownItems: unknownItems,
            DeferredItems: deferredItems,
            SelectedNextLawfulMove: result.Accepted
                ? NextLawfulMoveKind.RetainCurrentFooting
                : NextLawfulMoveKind.Defer,
            RetainedDecisionResult: result.Decision,
            TimestampUtc: DateTimeOffset.UtcNow,
            FailureSignatures: failureSignatures);

        return CognitiveFormationEvaluator.Evaluate(
            snapshot,
            CreateHandle("formation-receipt://", snapshot.SnapshotHandle, result.Decision));
    }

    private static PrimeRetainedHistoryRecord CreatePrimeRetainedHistoryRecord(
        GovernedSeedEvaluationResult result,
        FirstRunConstitutionReceipt firstRun,
        FirstRunLivingAgentiCorePacket livingPacket)
    {
        var visibleResidues = new List<PrimeMembraneProjectedLineResidue>
        {
            new(
                LineHandle: CreateHandle("prime-residue://", firstRun.ReceiptHandle, "listening-frame"),
                SourceSurfaceHandle: livingPacket.ListeningFrameHandle ?? livingPacket.PacketHandle,
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: PrimeMembraneProjectedParticipationKind.Clustered,
                AcceptedContributionHandles: [livingPacket.ListeningFrameHandle ?? livingPacket.PacketHandle],
                DistinctionPreserved: true,
                ResidueNotes: ["listening-frame-residue-retained"])
        };

        if (result.VerticalSlice.Predicate is not null)
        {
            visibleResidues.Add(new PrimeMembraneProjectedLineResidue(
                LineHandle: CreateHandle("prime-residue://", firstRun.ReceiptHandle, "predicate"),
                SourceSurfaceHandle: result.VerticalSlice.Predicate.SurfaceHandle,
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: PrimeMembraneProjectedParticipationKind.Clustered,
                AcceptedContributionHandles: [result.VerticalSlice.Predicate.SurfaceHandle],
                DistinctionPreserved: true,
                ResidueNotes: ["predicate-residue-retained"]));
        }

        var historyReceipt = new PrimeMembraneHistoryReceipt(
            ReceiptHandle: CreateHandle("prime-history-receipt://", firstRun.ReceiptHandle, result.Decision),
            HistoryHandle: CreateHandle("prime-history://", firstRun.ReceiptHandle, result.Decision),
            MembraneHandle: firstRun.LivingAgentiCorePacket?.ListeningFrameHandle ?? firstRun.ReceiptHandle,
            ProjectionHandle: result.VerticalSlice.ProjectionReceipt?.ProjectionHandle ?? firstRun.ReceiptHandle,
            Interpretation: result.Accepted
                ? PrimeMembraneProjectedHistoryInterpretationKind.StableBraid
                : PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection,
            ReceiptKind: result.Accepted
                ? PrimeMembraneReceiptKind.ReceiptedHistory
                : PrimeMembraneReceiptKind.Deferred,
            AdvisoryClosureEligibility: PrimeClosureEligibilityKind.CandidateOnly,
            PreservedDistinctionVisible: result.Accepted,
            RetainedWholenessStillWithheld: true,
            PrimeClosureStillWithheld: true,
            VisibleLineResidues: visibleResidues,
            DeferredLineResidues: result.Accepted
                ? []
                : visibleResidues.Take(1).ToArray(),
            ConstraintCodes: ["prime-history-candidate-only"],
            ReasonCode: result.Accepted
                ? "prime-history-visible-distinction-retained"
                : "prime-history-distinction-deferred",
            LawfulBasis: "prime-side retained history may remain candidate-only and unclosed while distinction is preserved and closure stays withheld.",
            TimestampUtc: DateTimeOffset.UtcNow);

        return PrimeRetainedWholeEvaluator.Evaluate(
            historyReceipt,
            CreateHandle("prime-retained://", historyReceipt.ReceiptHandle, result.Decision));
    }

    private static ListeningFrameProjectionPacket CreateFallbackListeningFramePacket(
        FirstRunConstitutionReceipt firstRun,
        FirstRunLivingAgentiCorePacket livingPacket)
    {
        return new ListeningFrameProjectionPacket(
            PacketHandle: CreateHandle("listening-frame://", firstRun.ReceiptHandle, livingPacket.PacketHandle),
            ListeningFrameHandle: livingPacket.ListeningFrameHandle,
            ChamberHandle: livingPacket.SelfGelAttachmentHandle,
            SourceSurfaceHandle: livingPacket.ZedOfDeltaHandle,
            VisibilityPosture: ListeningFrameVisibilityPosture.OperatorGuarded,
            IntegrityState: ListeningFrameIntegrityState.Usable,
            ReviewPosture: ListeningFrameReviewPosture.CandidateOnly,
            UsableForCompassProjection: true,
            PostureMarkers: ["fallback-listening-frame"],
            ReviewNotes: ["listening-frame-projection-fallback-materialized"],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static FirstRunLivingAgentiCorePacket CreateFallbackLivingPacket(
        FirstRunConstitutionReceipt firstRun,
        GovernedSeedEvaluationResult result)
    {
        var sanctuaryIngress = result.VerticalSlice.SanctuaryIngressReceipt;
        var operationalContext = result.VerticalSlice.OperationalContext;
        var projectionReceipt = result.VerticalSlice.ProjectionReceipt;
        var zedBasis = new ZedDeltaSelfBasisReceipt(
            ReceiptHandle: CreateHandle("zed-delta-basis://", firstRun.ReceiptHandle, result.Decision),
            ListeningFrameHandle: operationalContext?.PrimeCrypticHandle ?? firstRun.ReceiptHandle,
            SoulFrameHandle: operationalContext?.BootstrapHandle ?? firstRun.ReceiptHandle,
            OeHandle: "oe-fallback://first-run",
            SelfGelHandle: "self-gel-fallback://first-run",
            COeHandle: "coe-fallback://first-run",
            CSelfGelHandle: "cself-gel-fallback://first-run",
            ZedOfDeltaHandle: "zed-fallback://first-run",
            EngineeredCognitionHandle: "ec-fallback://first-run",
            EcIuttLispMatrixHandle: null,
            BasisBand: ZedDeltaSelfBasisBand.Stable,
            Disposition: ZedDeltaSelfBasisDisposition.Orient,
            AnchoredByOe: true,
            StoredInSoulFrame: true,
            CastIntoListeningFrame: true,
            WiredThroughSelfGel: true,
            CandidateOnly: true,
            PersistenceAuthorityWithheld: true,
            ContinuityAdmissionWithheld: true,
            CardinalDirections: [ZedBasisDirectionKind.Center],
            OrientationMarkers: ["fallback-zed-delta-self-basis"],
            ConstraintCodes: ["fallback-zed-delta-self-basis-candidate-only"],
            ReasonCode: "fallback-zed-delta-self-basis",
            LawfulBasis: "fallback-zed-delta-self-basis-is-used-only-to-complete-pre-domain-lifecycle-projection",
            TimestampUtc: DateTimeOffset.UtcNow);
        var thetaIngress = new ThetaIngressSensoryClusterReceipt(
            ReceiptHandle: CreateHandle("theta-ingress://", firstRun.ReceiptHandle, result.Decision),
            ThetaHandle: CreateHandle("theta-cluster://", firstRun.ReceiptHandle, result.Decision),
            ListeningFrameHandle: zedBasis.ListeningFrameHandle,
            SoulFrameHandle: zedBasis.SoulFrameHandle,
            ZedOfDeltaHandle: zedBasis.ZedOfDeltaHandle,
            OeHandle: zedBasis.OeHandle,
            SelfGelHandle: zedBasis.SelfGelHandle,
            COeHandle: zedBasis.COeHandle,
            CSelfGelHandle: zedBasis.CSelfGelHandle,
            EngineeredCognitionHandle: zedBasis.EngineeredCognitionHandle,
            IngressStatus: ThetaIngressStatusKind.Lawful,
            PresentedInListeningFrame: true,
            CrossedRelativeToZed: true,
            TakenUpAtCOe: true,
            EnteredCSelfGel: true,
            ContextualizationBegun: true,
            CandidateOnly: true,
            PersistenceAuthorityWithheld: true,
            SelfMutationWithheld: true,
            InheritanceWithheld: true,
            CondensationWithheld: true,
            PulseAuthorityWithheld: true,
            ThetaMarkers:
            [
                sanctuaryIngress?.SourceInputHandle ?? "source-input://fallback",
                sanctuaryIngress?.PreparedInputHandle ?? "prepared-input://fallback"
            ],
            ConstraintCodes: ["theta-ingress-fallback-lawful"],
            ReasonCode: "theta-ingress-fallback-lawful",
            LawfulBasis: "fallback-theta-ingress-is-used-only-to-complete-pre-domain-lifecycle-projection",
            TimestampUtc: DateTimeOffset.UtcNow);
        var postIngressDiscernment = new PostIngressDiscernmentReceipt(
            ReceiptHandle: CreateHandle("post-ingress-discernment://", firstRun.ReceiptHandle, result.Decision),
            ThetaIngressReceiptHandle: thetaIngress.ReceiptHandle,
            ThetaHandle: thetaIngress.ThetaHandle,
            ListeningFrameHandle: zedBasis.ListeningFrameHandle,
            ZedOfDeltaHandle: zedBasis.ZedOfDeltaHandle,
            COeHandle: zedBasis.COeHandle,
            CSelfGelHandle: zedBasis.CSelfGelHandle,
            EngineeredCognitionHandle: zedBasis.EngineeredCognitionHandle,
            StableOneHandle: CreateHandle("stable-one://", firstRun.ReceiptHandle, result.Decision),
            DiscernmentState: PostIngressDiscernmentStateKind.Stabilized,
            StableOneAchieved: true,
            CandidateOnly: true,
            SemanticRiseWithheld: true,
            PersistenceAuthorityWithheld: true,
            InheritanceWithheld: true,
            SelfMutationWithheld: true,
            PulseAuthorityWithheld: true,
            DiscernmentSignals: [],
            QuestionHandles: [],
            EnrichmentHandles: [],
            CarriedIncompleteHandles: [],
            ConstraintCodes: ["post-ingress-discernment-fallback-stabilized"],
            ReasonCode: "post-ingress-discernment-fallback-stabilized",
            LawfulBasis: "fallback-post-ingress-discernment-is-used-only-to-project-pre-domain-seed-standing",
            TimestampUtc: DateTimeOffset.UtcNow);

        return new FirstRunLivingAgentiCorePacket(
            PacketHandle: CreateHandle("living-agenticore://", firstRun.ReceiptHandle, result.Decision),
            LivingAgentiCoreHandle: "living-agenticore-fallback://first-run",
            ListeningFrameHandle: projectionReceipt?.ProjectionHandle ?? "listening-frame-fallback://first-run",
            ZedOfDeltaHandle: zedBasis.ZedOfDeltaHandle,
            SelfGelAttachmentHandle: zedBasis.SelfGelHandle,
            ToolUseContextHandle: sanctuaryIngress?.PreparedInputHandle,
            CompassEmbodimentHandle: "compass-fallback://first-run",
            EngineeredCognitionHandle: zedBasis.EngineeredCognitionHandle,
            WiderPublicWideningWithheld: true,
            TimestampUtc: DateTimeOffset.UtcNow,
            ListeningFrameProjectionPacket: null,
            CompassProjectionPacket: null,
            ListeningFrameInstrumentationReceipt: null,
            ZedDeltaSelfBasisReceipt: zedBasis,
            ThetaIngressSensoryClusterReceipt: thetaIngress,
            PostIngressDiscernmentReceipt: postIngressDiscernment);
    }

    private static CompassProjectionPacket CreateFallbackCompassProjectionPacket(
        FirstRunConstitutionReceipt firstRun,
        FirstRunLivingAgentiCorePacket livingPacket,
        GovernedSeedEvaluationResult result)
    {
        return new CompassProjectionPacket(
            PacketHandle: CreateHandle("compass-projection://", firstRun.ReceiptHandle, result.Decision),
            CompassEmbodimentHandle: livingPacket.CompassEmbodimentHandle,
            ListeningFrameHandle: livingPacket.ListeningFrameHandle,
            DriftState: result.GovernanceState == GovernedSeedEvaluationState.UnresolvedConflict
                ? CompassDriftState.Weakened
                : CompassDriftState.Held,
            OrientationPosture: result.Accepted
                ? CompassOrientationPosture.Centered
                : CompassOrientationPosture.Seeking,
            AdmissibilityEstimate: result.Accepted
                ? CompassAdmissibilityEstimate.ProvisionallyAdmissible
                : CompassAdmissibilityEstimate.CandidateOnly,
            TransitionRecommendation: result.Accepted
                ? CompassTransitionRecommendation.ProceedBounded
                : CompassTransitionRecommendation.Hold,
            AuthorityPosture: CompassAuthorityPosture.CandidateOnly,
            CandidateInputs: [new CompassCandidateModulationInput(
                InputHandle: result.VerticalSlice.PathReceipt.PathHandle,
                InputKind: "runtime-path",
                SourceReason: result.GovernanceTrace)],
            ReviewNotes: [result.Note],
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<string> BuildSeedEvidenceHandles(
        FormationReceipt formationReceipt,
        PrimeRetainedHistoryRecord retainedPrime,
        FirstRunConstitutionReceipt firstRun)
    {
        return formationReceipt.LawfulKnownItems
            .Concat(formationReceipt.RetainedUnknownItems)
            .Concat([retainedPrime.RecordHandle, firstRun.ReceiptHandle])
            .Where(static handle => !string.IsNullOrWhiteSpace(handle))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static handle => handle, StringComparer.Ordinal)
            .ToArray();
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private sealed record PreDomainLifecycleInputs(
        FormationReceipt FormationReceipt,
        ListeningFrameProjectionPacket ListeningFrameProjection,
        CompassProjectionPacket CompassProjection,
        EngineeredCognitionFirstPrimeStateReceipt FirstPrimeReceipt,
        PrimeSeedStateReceipt PrimeSeedReceipt);
}
