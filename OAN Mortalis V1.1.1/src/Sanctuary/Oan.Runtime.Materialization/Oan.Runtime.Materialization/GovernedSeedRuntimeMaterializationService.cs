using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oan.Common;
using Oan.FirstRun;

namespace Oan.Runtime.Materialization;

public interface IGovernedSeedRuntimeMaterializationService
{
    GovernedSeedBootstrapAdmissionReceipt CreateBootstrapAdmissionReceipt(
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision);

    GovernedSeedEvaluationResult MaterializeBootstrapDeniedResult(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedSanctuaryIngressReceipt sanctuaryIngressReceipt,
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt);

    GovernedSeedEvaluationResult HydrateNexusAndPrime(
        GovernedSeedEvaluationResult result,
        string agentId,
        string theaterId,
        string input,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt,
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision);

    GovernedSeedEvaluationResult AttachStateModulation(
        GovernedSeedEvaluationResult result,
        GovernedSeedStateModulationReceipt stateModulationReceipt);

    EvaluateEnvelope CreateEnvelope(
        string agentId,
        string theaterId,
        GovernedSeedEvaluationResult result);
}

public sealed class GovernedSeedRuntimeMaterializationService : IGovernedSeedRuntimeMaterializationService
{
    private readonly IFirstRunConstitutionService _firstRunConstitutionService;
    private readonly IGovernedSeedPreGovernanceService _preGovernanceService;

    private static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        WriteIndented = true
    };

    public GovernedSeedRuntimeMaterializationService(
        IFirstRunConstitutionService firstRunConstitutionService,
        IGovernedSeedPreGovernanceService preGovernanceService)
    {
        _firstRunConstitutionService = firstRunConstitutionService ?? throw new ArgumentNullException(nameof(firstRunConstitutionService));
        _preGovernanceService = preGovernanceService ?? throw new ArgumentNullException(nameof(preGovernanceService));
    }

    public GovernedSeedBootstrapAdmissionReceipt CreateBootstrapAdmissionReceipt(
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(posture);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        return new GovernedSeedBootstrapAdmissionReceipt(
            AdmissionHandle: $"bootstrap-admission://{decision.DecisionHandle}",
            PostureHandle: posture.PostureHandle,
            RequestHandle: request.RequestHandle,
            DecisionHandle: decision.DecisionHandle,
            Disposition: decision.Disposition,
            ActivatedModality: decision.ActivatedModality,
            MembraneWakePermitted: decision.Disposition == GovernedSeedNexusTransitionDisposition.Admitted,
            DecisionReason: decision.DecisionReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    public GovernedSeedEvaluationResult MaterializeBootstrapDeniedResult(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedSanctuaryIngressReceipt sanctuaryIngressReceipt,
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);
        ArgumentNullException.ThrowIfNull(sanctuaryIngressReceipt);
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(posture);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(bootstrapAdmissionReceipt);

        var timestampUtc = DateTimeOffset.UtcNow;
        var capabilityReceipt = new ProtectedExecutionCapabilityReceipt(
            CapabilityHandle: CreateHandle("capability://", agentId, theaterId, "bootstrap-denied", input),
            RuntimeHandle: CreateHandle("runtime://", agentId, theaterId),
            ScopeHandle: CreateHandle("scope://", theaterId, "bootstrap-admission"),
            WitnessedBy: "nexus-bootstrap-admission",
            AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
            DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
            InputHandles:
            [
                new ProtectedExecutionInputHandle(
                    Handle: CreateHandle("cryptic-input://", agentId, theaterId),
                    HandleKind: ProtectedExecutionHandleKind.HotStateHandle,
                    ObserverLegible: false,
                    MintEligible: false,
                    ProtectionClass: "bootstrap-gated-working-set")
            ],
            AdmissibleActFamilies:
            [
                ProtectedExecutionActFamily.Intake,
                ProtectedExecutionActFamily.Refuse
            ],
            ForbiddenActFamilies:
            [
                ProtectedExecutionActFamily.Differentiate,
                ProtectedExecutionActFamily.Seal,
                ProtectedExecutionActFamily.Orient,
                ProtectedExecutionActFamily.Derive,
                ProtectedExecutionActFamily.Mint,
                ProtectedExecutionActFamily.Delegate,
                ProtectedExecutionActFamily.Return,
                ProtectedExecutionActFamily.Defer
            ],
            ReachableMintedOutputClasses:
            [
                ProtectedExecutionMintedOutputClass.RefusalReceipt
            ],
            TimestampUtc: timestampUtc);

        var pathReceipt = new ProtectedExecutionPathReceipt(
            PathHandle: CreateHandle("path://", agentId, theaterId, "bootstrap-denied"),
            DirectiveHandle: CreateHandle("directive://", capabilityReceipt.CapabilityHandle, bootstrapReceipt.BootstrapHandle, "bootstrap-denied"),
            State: ProtectedExecutionPathState.Refused,
            SelectedActPath:
            [
                ProtectedExecutionActFamily.Intake,
                ProtectedExecutionActFamily.Refuse
            ],
            MintedOutputClasses:
            [
                ProtectedExecutionMintedOutputClass.RefusalReceipt
            ],
            OutcomeCode: "bootstrap-denied",
            TimestampUtc: timestampUtc);

        var governanceReceipt = new ProtectedExecutionGovernanceReceipt(
            ReceiptHandle: CreateHandle("governance://", bootstrapReceipt.BootstrapHandle, "bootstrap-denied"),
            PathHandle: pathReceipt.PathHandle,
            GovernedBy: "nexus-bootstrap-admission",
            DecisionCode: "bootstrap-denied",
            ReturnedToFather: true,
            WitnessOnly: true,
            WithheldOutputHandles: [],
            TimestampUtc: timestampUtc);
        var preGovernancePacket = _preGovernanceService.Project(
            bootstrapReceipt,
            sanctuaryIngressReceipt,
            lowMindSfRoute: null,
            theaterId);

        var operationalContext = CreateOperationalContext(
            agentId,
            theaterId,
            input,
            bootstrapReceipt,
            primeCrypticReceipt,
            bootstrapAdmissionReceipt,
            posture,
            request,
            decision,
            preGovernancePacket,
            sanctuaryIngressReceipt,
            null,
            null,
            null,
            null,
            capabilityReceipt.AuthorityClass,
            capabilityReceipt.DisclosureCeiling,
            "bootstrap-denied",
            GovernedSeedEvaluationState.Refusal,
            bootstrapAdmissionReceipt.DecisionReason,
            capabilityReceipt.CapabilityHandle,
            pathReceipt.PathHandle,
            predicateSurfaceHandle: null,
            withheldOutputHandles: [],
            predicateSurfaceEligible: false);

        return new GovernedSeedEvaluationResult(
            Decision: "bootstrap-denied",
            Accepted: false,
            GovernanceState: GovernedSeedEvaluationState.Refusal,
            GovernanceTrace: bootstrapAdmissionReceipt.DecisionReason,
            Confidence: 0.05,
            Note: "Nexus denied bootstrap admission; SoulFrame membrane remained dormant.",
            VerticalSlice: new GovernedSeedVerticalSlice(
                BootstrapReceipt: bootstrapReceipt,
                BootstrapAdmissionReceipt: bootstrapAdmissionReceipt,
                ProjectionReceipt: null,
                ReturnIntakeReceipt: null,
                StewardshipReceipt: null,
                HoldRoutingReceipt: null,
                SituationalContext: null,
                NexusPosture: posture,
                NexusTransitionRequest: request,
                NexusTransitionDecision: decision,
                PrimeCrypticReceipt: primeCrypticReceipt,
                SanctuaryIngressReceipt: sanctuaryIngressReceipt,
                HostedLlmReceipt: null,
                HighMindContext: null,
                PreGovernancePacket: preGovernancePacket,
                FirstRunConstitution: operationalContext.FirstRunConstitution,
                OperationalContext: operationalContext,
                StateModulationReceipt: null,
                CapabilityReceipt: capabilityReceipt,
                PathReceipt: pathReceipt,
                GovernanceReceipt: governanceReceipt,
                DerivationReceipt: null,
                Predicate: null,
                OutcomeCode: "bootstrap-denied"),
            ProtectedResidueEvidence: []);
    }

    public GovernedSeedEvaluationResult HydrateNexusAndPrime(
        GovernedSeedEvaluationResult result,
        string agentId,
        string theaterId,
        string input,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt,
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapAdmissionReceipt);
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(posture);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        var preGovernancePacket = _preGovernanceService.Project(
            bootstrapReceipt,
            result.VerticalSlice.SanctuaryIngressReceipt,
            result.VerticalSlice.SituationalContext?.LowMindSfRoute,
            theaterId);

        var operationalContext = CreateOperationalContext(
            agentId,
            theaterId,
            input,
            bootstrapReceipt,
            primeCrypticReceipt,
            bootstrapAdmissionReceipt,
            posture,
            request,
            decision,
            preGovernancePacket,
            result.VerticalSlice.SanctuaryIngressReceipt,
            result.VerticalSlice.HostedLlmReceipt,
            result.VerticalSlice.HighMindContext,
            result.VerticalSlice.SituationalContext?.LowMindSfRoute,
            result.VerticalSlice.StewardshipReceipt,
            result.VerticalSlice.CapabilityReceipt.AuthorityClass,
            result.VerticalSlice.CapabilityReceipt.DisclosureCeiling,
            result.Decision,
            result.GovernanceState,
            result.GovernanceTrace,
            result.VerticalSlice.CapabilityReceipt.CapabilityHandle,
            result.VerticalSlice.PathReceipt.PathHandle,
            result.VerticalSlice.Predicate?.SurfaceHandle,
            result.VerticalSlice.GovernanceReceipt.WithheldOutputHandles,
            result.VerticalSlice.Predicate is not null);

        return result with
        {
            VerticalSlice = result.VerticalSlice with
            {
                NexusPosture = posture,
                NexusTransitionRequest = request,
                NexusTransitionDecision = decision,
                BootstrapAdmissionReceipt = bootstrapAdmissionReceipt,
                PrimeCrypticReceipt = primeCrypticReceipt,
                SanctuaryIngressReceipt = result.VerticalSlice.SanctuaryIngressReceipt,
                FirstRunConstitution = operationalContext.FirstRunConstitution,
                PreGovernancePacket = preGovernancePacket,
                OperationalContext = operationalContext
            }
        };
    }

    public GovernedSeedEvaluationResult AttachStateModulation(
        GovernedSeedEvaluationResult result,
        GovernedSeedStateModulationReceipt stateModulationReceipt)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(stateModulationReceipt);

        return result with
        {
            VerticalSlice = result.VerticalSlice with
            {
                StateModulationReceipt = stateModulationReceipt
            }
        };
    }

    public EvaluateEnvelope CreateEnvelope(
        string agentId,
        string theaterId,
        GovernedSeedEvaluationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);
        ArgumentNullException.ThrowIfNull(result);

        var returnSurfaceContext = CreateReturnSurfaceContext(result);
        var outboundObjectContext = CreateOutboundObjectContext(returnSurfaceContext);

        return new EvaluateEnvelope
        {
            AgentId = agentId,
            TheaterId = theaterId,
            Decision = result.Decision,
            Accepted = result.Accepted,
            Note = result.Note,
            Payload = JsonSerializer.Serialize(result.VerticalSlice, PayloadJsonOptions),
            ReturnSurfaceContext = returnSurfaceContext,
            OutboundObjectContext = outboundObjectContext,
            OutboundLaneContext = CreateOutboundLaneContext(outboundObjectContext),
            Confidence = result.Confidence,
            GovernanceState = GovernedSeedEvaluationStateTokens.ToToken(result.GovernanceState),
            GovernanceTrace = result.GovernanceTrace
        };
    }

    private GovernedSeedOperationalContext CreateOperationalContext(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision,
        GovernedSeedPreGovernancePacket preGovernancePacket,
        GovernedSeedSanctuaryIngressReceipt? sanctuaryIngressReceipt,
        GovernedSeedHostedLlmSeedReceipt? hostedLlmReceipt,
        GovernedSeedHighMindContext? highMindContext,
        GovernedSeedLowMindSfRoutePacket? lowMindSfRoute,
        GovernedSeedSoulFrameStewardshipReceipt? stewardshipReceipt,
        ProtectedExecutionAuthorityClass authorityClass,
        ProtectedExecutionDisclosureCeiling disclosureCeiling,
        string outcomeCode,
        GovernedSeedEvaluationState governanceState,
        string governanceTrace,
        string capabilityHandle,
        string pathHandle,
        string? predicateSurfaceHandle,
        IReadOnlyList<string> withheldOutputHandles,
        bool predicateSurfaceEligible)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapAdmissionReceipt);
        ArgumentNullException.ThrowIfNull(posture);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(preGovernancePacket);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcomeCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(governanceTrace);
        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(pathHandle);
        ArgumentNullException.ThrowIfNull(withheldOutputHandles);

        var formationContext = CreateFormationContext(input, bootstrapReceipt, decision);
        var primeToCrypticTransit = CreatePrimeToCrypticTransitContext(
            primeCrypticReceipt,
            bootstrapReceipt,
            request,
            decision,
            capabilityHandle,
            sanctuaryIngressReceipt,
            lowMindSfRoute,
            authorityClass,
            disclosureCeiling,
            agentId,
            theaterId,
            input);
        var crypticToPrimeTransit = CreateCrypticToPrimeTransitContext(
            primeCrypticReceipt,
            decision,
            outcomeCode,
            governanceState,
            governanceTrace,
            pathHandle,
            predicateSurfaceHandle,
            withheldOutputHandles,
            predicateSurfaceEligible);
        var firstRunConstitution = ProjectFirstRunConstitution(
            bootstrapReceipt,
            bootstrapAdmissionReceipt,
            preGovernancePacket,
            sanctuaryIngressReceipt,
            stewardshipReceipt,
            hostedLlmReceipt,
            posture,
            theaterId);

        return new GovernedSeedOperationalContext(
            ContextHandle: CreateHandle(
                "operational-context://",
                bootstrapAdmissionReceipt.AdmissionHandle,
                posture.PostureHandle,
                decision.DecisionHandle),
            ContextProfile: "minimal-braided-operational-context",
            BootstrapHandle: posture.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            LispBundleHandle: primeCrypticReceipt.LispBundleReceipt.BundleHandle,
            SanctuaryIngressReceiptHandle: sanctuaryIngressReceipt?.ReceiptHandle,
            HostedLlmServiceHandle: hostedLlmReceipt?.ServiceHandle,
            HostedLlmReceiptHandle: hostedLlmReceipt?.ReceiptHandle,
            HostedLlmRequestPacketHandle: hostedLlmReceipt?.RequestPacket.PacketHandle,
            HostedLlmResponsePacketHandle: hostedLlmReceipt?.ResponsePacket.PacketHandle,
            HostedLlmState: hostedLlmReceipt?.ResponsePacket.State,
            HighMindContextHandle: highMindContext?.ContextHandle,
            HighMindUptakeKind: highMindContext?.UptakeKind,
            FirstRunConstitution: firstRunConstitution,
            PreGovernancePacketHandle: preGovernancePacket.PacketHandle,
            LocalAuthorityTraceHandle: firstRunConstitution.LocalAuthorityTraceHandle,
            FirstRunReceiptHandle: firstRunConstitution.ReceiptHandle,
            ConstitutionalContactHandle: firstRunConstitution.ConstitutionalContactHandle,
            LocalKeypairGenesisSourceHandle: firstRunConstitution.LocalKeypairGenesisSourceHandle,
            LocalKeypairGenesisHandle: firstRunConstitution.LocalKeypairGenesisHandle,
            FirstCrypticBraidEstablishmentHandle: firstRunConstitution.FirstCrypticBraidEstablishmentHandle,
            FirstCrypticBraidHandle: firstRunConstitution.FirstCrypticBraidHandle,
            FirstCrypticConditioningSourceHandle: firstRunConstitution.FirstCrypticConditioningSourceHandle,
            FirstCrypticConditioningHandle: firstRunConstitution.FirstCrypticConditioningHandle,
            FirstRunState: firstRunConstitution.CurrentState,
            FirstRunReadinessState: firstRunConstitution.ReadinessState,
            FirstRunStateProvisional: firstRunConstitution.CurrentStateProvisional,
            FirstRunStateActualized: firstRunConstitution.CurrentStateActualized,
            FirstRunOpalActualized: firstRunConstitution.OpalActualized,
            LowMindSfRouteHandle: lowMindSfRoute?.PacketHandle,
            IngressAccessClass: lowMindSfRoute?.IngressAccessClass ?? GovernedSeedIngressAccessClass.PromptInput,
            LowMindSfRouteKind: lowMindSfRoute?.RouteKind ?? GovernedSeedLowMindSfRouteKind.DirectPrompt,
            BootstrapAdmissionHandle: bootstrapAdmissionReceipt.AdmissionHandle,
            NexusPostureHandle: posture.PostureHandle,
            NexusDecisionHandle: decision.DecisionHandle,
            ResidencyProfile: primeCrypticReceipt.ResidencyProfile,
            WorkState: posture.WorkState,
            ActivatedModality: decision.ActivatedModality,
            BootstrapDisposition: bootstrapAdmissionReceipt.Disposition,
            NexusDisposition: decision.Disposition,
            CollapseReadinessState: posture.CollapseReadinessState,
            ProtectedHoldClass: posture.ProtectedHoldClass,
            ProtectedHoldRoute: posture.ProtectedHoldRoute,
            ReviewState: posture.ReviewState,
            GovernanceReadable: posture.GovernanceReadable,
            MembraneWakePermitted: bootstrapAdmissionReceipt.MembraneWakePermitted,
            CpuOnly: primeCrypticReceipt.CpuOnly,
            TargetBoundedLaneAvailable: primeCrypticReceipt.TargetBoundedLaneAvailable,
            FormationContext: formationContext,
            PrimeToCrypticTransit: primeToCrypticTransit,
            CrypticToPrimeTransit: crypticToPrimeTransit,
            SourceReason: decision.DecisionReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private FirstRunConstitutionReceipt ProjectFirstRunConstitution(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedBootstrapAdmissionReceipt bootstrapAdmissionReceipt,
        GovernedSeedPreGovernancePacket preGovernancePacket,
        GovernedSeedSanctuaryIngressReceipt? sanctuaryIngressReceipt,
        GovernedSeedSoulFrameStewardshipReceipt? stewardshipReceipt,
        GovernedSeedHostedLlmSeedReceipt? hostedLlmReceipt,
        GovernedSeedNexusPostureSnapshot posture,
        string theaterId)
    {
        ArgumentNullException.ThrowIfNull(preGovernancePacket);

        var snapshot = new FirstRunConstitutionSnapshot(
            SnapshotHandle: CreateHandle(
                "first-run-snapshot://",
                bootstrapReceipt.BootstrapHandle,
                posture.PostureHandle,
                theaterId),
            SanctuaryInitializationHandle: sanctuaryIngressReceipt?.ReceiptHandle,
            LocationBindingHandle: !string.IsNullOrWhiteSpace(theaterId)
                ? CreateHandle("location-binding://", bootstrapReceipt.BootstrapHandle, theaterId)
                : null,
            LocalAuthorityTraceHandle: preGovernancePacket.LocalAuthorityTrace?.ReceiptHandle,
            ConstitutionalContactHandle: preGovernancePacket.ConstitutionalContact?.ReceiptHandle,
            LocalKeypairGenesisSourceHandle: preGovernancePacket.LocalKeypairGenesisSource?.ReceiptHandle,
            LocalKeypairGenesisHandle: preGovernancePacket.LocalKeypairGenesis?.ReceiptHandle,
            FirstCrypticBraidEstablishmentHandle: preGovernancePacket.FirstCrypticBraidEstablishment?.ReceiptHandle,
            FirstCrypticBraidHandle: preGovernancePacket.FirstCrypticBraid?.ReceiptHandle,
            FirstCrypticConditioningSourceHandle: preGovernancePacket.FirstCrypticConditioningSource?.ReceiptHandle,
            FirstCrypticConditioningHandle: preGovernancePacket.FirstCrypticConditioning?.ReceiptHandle,
            MotherStandingHandle: string.Equals(
                    bootstrapReceipt.MantleReceipt.PrimeGovernanceOffice,
                    "Mother",
                    StringComparison.OrdinalIgnoreCase)
                ? CreateHandle("mother-standing://", bootstrapReceipt.MantleReceipt.MantleHandle, theaterId)
                : null,
            FatherStandingHandle: string.Equals(
                    bootstrapReceipt.MantleReceipt.CrypticGovernanceOffice,
                    "Father",
                    StringComparison.OrdinalIgnoreCase)
                ? CreateHandle("father-standing://", bootstrapReceipt.MantleReceipt.CrypticMantleHandle, theaterId)
                : null,
            CradleTekInstallHandle: CreateHandle("cradletek-install://", bootstrapReceipt.BootstrapHandle, bootstrapReceipt.SoulFrameHandle),
            CradleTekAdmissionHandle: bootstrapAdmissionReceipt.MembraneWakePermitted
                ? bootstrapAdmissionReceipt.AdmissionHandle
                : null,
            StewardStandingHandle: stewardshipReceipt is { StewardPrimary: true }
                ? stewardshipReceipt.StewardshipHandle
                : null,
            GelStandingHandle: bootstrapReceipt.CustodySnapshot.GelHandle,
            GoaStandingHandle: bootstrapReceipt.CustodySnapshot.GoaHandle,
            MosStandingHandle: bootstrapReceipt.CustodySnapshot.MosHandle,
            ToolRightsHandle: bootstrapAdmissionReceipt.MembraneWakePermitted
                ? CreateHandle("tool-rights://", bootstrapAdmissionReceipt.AdmissionHandle, theaterId)
                : null,
            DataRightsHandle: bootstrapAdmissionReceipt.MembraneWakePermitted
                ? CreateHandle("data-rights://", bootstrapAdmissionReceipt.AdmissionHandle, theaterId)
                : null,
            HostedSeedPresenceHandle: hostedLlmReceipt?.ServiceHandle,
            BondProcessHandle: null,
            OpalActualizationHandle: null,
            NoticeCertificationGateHandle: null,
            TimestampUtc: DateTimeOffset.UtcNow,
            ProtocolizationPacket: null,
            StewardWitnessedOePacket: null,
            ElementalBindingPacket: null,
            ActualizationSealPacket: null,
            LivingAgentiCorePacket: null);

        return _firstRunConstitutionService.Project(snapshot);
    }

    private static GovernedSeedPrimeToCrypticTransitContext CreatePrimeToCrypticTransitContext(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedNexusTransitionRequest request,
        GovernedSeedNexusTransitionDecision decision,
        string capabilityHandle,
        GovernedSeedSanctuaryIngressReceipt? sanctuaryIngressReceipt,
        GovernedSeedLowMindSfRoutePacket? lowMindSfRoute,
        ProtectedExecutionAuthorityClass authorityClass,
        ProtectedExecutionDisclosureCeiling disclosureCeiling,
        string agentId,
        string theaterId,
        string input)
    {
        var lispBundleReceipt = primeCrypticReceipt.LispBundleReceipt;
        var transitHandle = CreateHandle(
            "prime-to-cryptic-transit://",
            primeCrypticReceipt.ServiceHandle,
            request.RequestHandle,
            decision.DecisionHandle);

        return new GovernedSeedPrimeToCrypticTransitContext(
            TransitHandle: transitHandle,
            TransitProfile: "prime-to-cryptic-hosted-sli-transit",
            PrimeServiceHandle: primeCrypticReceipt.PrimeServiceHandle,
            CrypticServiceHandle: primeCrypticReceipt.CrypticServiceHandle,
            LispBundleHandle: lispBundleReceipt.BundleHandle,
            InterconnectProfile: lispBundleReceipt.InterconnectProfile,
            CarrierKind: lispBundleReceipt.CrypticCarrierKind,
            RequestedModality: request.RequestedModality,
            ActivatedModality: decision.ActivatedModality,
            HostedExecutionOnly: lispBundleReceipt.HostedExecutionOnly,
            CpuOnly: primeCrypticReceipt.CpuOnly,
            TargetBoundedLaneAvailable: primeCrypticReceipt.TargetBoundedLaneAvailable,
            TransitPacket: new GovernedSeedPrimeToCrypticTransitPacket(
                PacketHandle: CreateHandle(
                    "prime-to-cryptic-packet://",
                    transitHandle,
                    capabilityHandle,
                    request.RequestHandle),
                PacketProfile: "prime-hosted-sli-request-packet",
                TransitHandle: transitHandle,
                CapabilityHandle: capabilityHandle,
                BootstrapHandle: bootstrapReceipt.BootstrapHandle,
                SanctuaryIngressReceiptHandle: sanctuaryIngressReceipt?.ReceiptHandle,
                InputHandle: CreateHandle("prime-input://", agentId, theaterId, input),
                LowMindSfRouteHandle: lowMindSfRoute?.PacketHandle,
                IngressAccessClass: lowMindSfRoute?.IngressAccessClass ?? GovernedSeedIngressAccessClass.PromptInput,
                LowMindSfRouteKind: lowMindSfRoute?.RouteKind ?? GovernedSeedLowMindSfRouteKind.DirectPrompt,
                InterconnectProfile: lispBundleReceipt.InterconnectProfile,
                CarrierKind: lispBundleReceipt.CrypticCarrierKind,
                AuthorityClass: authorityClass,
                DisclosureCeiling: disclosureCeiling,
                RequestedModality: request.RequestedModality,
                HostedExecutionOnly: lispBundleReceipt.HostedExecutionOnly,
                TimestampUtc: DateTimeOffset.UtcNow),
            SourceReason: request.SourceReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedCrypticToPrimeTransitContext CreateCrypticToPrimeTransitContext(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedNexusTransitionDecision decision,
        string outcomeCode,
        GovernedSeedEvaluationState governanceState,
        string governanceTrace,
        string pathHandle,
        string? predicateSurfaceHandle,
        IReadOnlyList<string> withheldOutputHandles,
        bool predicateSurfaceEligible)
    {
        var lispBundleReceipt = primeCrypticReceipt.LispBundleReceipt;
        var transitHandle = CreateHandle(
            "cryptic-to-prime-transit://",
            primeCrypticReceipt.ServiceHandle,
            decision.DecisionHandle,
            outcomeCode,
            governanceState.ToString());

        return new GovernedSeedCrypticToPrimeTransitContext(
            TransitHandle: transitHandle,
            TransitProfile: "cryptic-to-prime-hosted-sli-return",
            PrimeServiceHandle: primeCrypticReceipt.PrimeServiceHandle,
            CrypticServiceHandle: primeCrypticReceipt.CrypticServiceHandle,
            LispBundleHandle: lispBundleReceipt.BundleHandle,
            InterconnectProfile: lispBundleReceipt.InterconnectProfile,
            CarrierKind: lispBundleReceipt.CrypticCarrierKind,
            OutcomeCode: outcomeCode,
            GovernanceState: governanceState,
            NexusDisposition: decision.Disposition,
            PredicateSurfaceEligible: predicateSurfaceEligible,
            HostedExecutionOnly: lispBundleReceipt.HostedExecutionOnly,
            CpuOnly: primeCrypticReceipt.CpuOnly,
            TargetBoundedLaneAvailable: primeCrypticReceipt.TargetBoundedLaneAvailable,
            ReturnPacket: new GovernedSeedCrypticToPrimeReturnPacket(
                PacketHandle: CreateHandle(
                    "cryptic-to-prime-packet://",
                    transitHandle,
                    pathHandle,
                    outcomeCode),
                PacketProfile: "cryptic-hosted-sli-return-packet",
                TransitHandle: transitHandle,
                PathHandle: pathHandle,
                InterconnectProfile: lispBundleReceipt.InterconnectProfile,
                CarrierKind: lispBundleReceipt.CrypticCarrierKind,
                GovernanceTrace: governanceTrace,
                PredicateSurfaceHandle: predicateSurfaceHandle,
                ReturnClass: DetermineCrypticReturnClass(outcomeCode, predicateSurfaceEligible),
                GovernanceState: governanceState,
                PredicateSurfaceEligible: predicateSurfaceEligible,
                WithheldOutputHandles: withheldOutputHandles,
                TimestampUtc: DateTimeOffset.UtcNow),
            SourceReason: decision.DecisionReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedCrypticReturnClass DetermineCrypticReturnClass(
        string outcomeCode,
        bool predicateSurfaceEligible)
    {
        if (predicateSurfaceEligible)
        {
            return GovernedSeedCrypticReturnClass.PredicateCarrier;
        }

        if (string.Equals(outcomeCode, "predicate-deferred", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(outcomeCode, "predicate-withheld", StringComparison.OrdinalIgnoreCase))
        {
            return GovernedSeedCrypticReturnClass.DeferredReceipt;
        }

        return GovernedSeedCrypticReturnClass.RefusalReceipt;
    }

    private static GovernedSeedFormationContext CreateFormationContext(
        string input,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedNexusTransitionDecision decision)
    {
        var scopeLane = ResolveScopeLane(input);
        var formKind = ResolveGovernedFormKind(input, scopeLane);
        var capabilityLedger = CreateCapabilityLedger(input, scopeLane);
        var formationLedger = CreateFormationLedger(input, scopeLane, formKind, capabilityLedger);
        var officeLedger = CreateOfficeLedger(input, scopeLane, formKind, capabilityLedger);
        var careerContinuityLedger = CreateCareerContinuityLedger(input, officeLedger);

        return new GovernedSeedFormationContext(
            ContextHandle: CreateHandle(
                "formation-context://",
                bootstrapReceipt.BootstrapHandle,
                scopeLane.ToString(),
                formKind.ToString(),
                decision.DecisionHandle),
            ContextProfile: "governed-cme-formation-ledger-context",
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            MantleHandle: bootstrapReceipt.MantleReceipt.MantleHandle,
            ScopeLane: scopeLane,
            GovernedFormKind: formKind,
            FormProfile: ResolveFormProfile(formKind),
            LocalGovernanceSurface: ResolveLocalGovernanceSurface(formKind),
            SpecialCaseProfile: ResolveSpecialCaseProfile(formKind),
            CapabilityLedger: capabilityLedger,
            FormationLedger: formationLedger,
            OfficeLedger: officeLedger,
            CareerContinuityLedger: careerContinuityLedger,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedCmeScopeLane ResolveScopeLane(string input)
    {
        if (ContainsAny(input, "bonded cme", "bonded-cme", "my daughter", "my son", "my child", "daughter's cme", "son's cme", "child-facing"))
        {
            return GovernedSeedCmeScopeLane.SpecialCases;
        }

        if (ContainsAny(input, "my family", "family cme", "family-scope"))
        {
            return GovernedSeedCmeScopeLane.Civil;
        }

        if (ContainsAny(input, "governance", "govern", "steward", "mother", "father", "policy", "office"))
        {
            return GovernedSeedCmeScopeLane.Governance;
        }

        if (ContainsAny(input, "commercial", "customer", "business", "sales", "commerce"))
        {
            return GovernedSeedCmeScopeLane.Commercial;
        }

        if (ContainsAny(input, "industrial", "factory", "manufacturing", "operations", "training line"))
        {
            return GovernedSeedCmeScopeLane.Industrial;
        }

        return GovernedSeedCmeScopeLane.Civil;
    }

    private static GovernedSeedGovernedFormKind ResolveGovernedFormKind(string input, GovernedSeedCmeScopeLane scopeLane)
    {
        if (ContainsAny(input, "bonded cme", "bonded-cme"))
        {
            return GovernedSeedGovernedFormKind.SpecialCaseBonded;
        }

        if (ContainsAny(input, "my daughter", "my son", "my child", "daughter's cme", "son's cme", "child-facing", "parental"))
        {
            return GovernedSeedGovernedFormKind.SpecialCaseParentalChild;
        }

        if (ContainsAny(input, "my family", "family cme", "family-scope"))
        {
            return GovernedSeedGovernedFormKind.LocalFamilyGoverned;
        }

        return scopeLane switch
        {
            GovernedSeedCmeScopeLane.Industrial => GovernedSeedGovernedFormKind.IndustrialOperational,
            GovernedSeedCmeScopeLane.Commercial => GovernedSeedGovernedFormKind.CommercialOperational,
            GovernedSeedCmeScopeLane.Governance => GovernedSeedGovernedFormKind.GovernanceOperational,
            _ => GovernedSeedGovernedFormKind.CivicUnbounded
        };
    }

    private static string ResolveFormProfile(GovernedSeedGovernedFormKind formKind) => formKind switch
    {
        GovernedSeedGovernedFormKind.LocalFamilyGoverned => "local-family-governed-civic",
        GovernedSeedGovernedFormKind.SpecialCaseBonded => "bonded-cme-special-case",
        GovernedSeedGovernedFormKind.SpecialCaseParentalChild => "parental-child-governed-special-case",
        GovernedSeedGovernedFormKind.IndustrialOperational => "industrial-governed-form",
        GovernedSeedGovernedFormKind.CommercialOperational => "commercial-governed-form",
        GovernedSeedGovernedFormKind.GovernanceOperational => "governance-governed-form",
        _ => "civic-unbounded-form"
    };

    private static string ResolveLocalGovernanceSurface(GovernedSeedGovernedFormKind formKind) => formKind switch
    {
        GovernedSeedGovernedFormKind.LocalFamilyGoverned => "my-family-cme-surface",
        GovernedSeedGovernedFormKind.SpecialCaseParentalChild => "parental-governed-child-surface",
        GovernedSeedGovernedFormKind.SpecialCaseBonded => "bonded-cme-special-case-surface",
        GovernedSeedGovernedFormKind.GovernanceOperational => "governance-office-surface",
        GovernedSeedGovernedFormKind.CommercialOperational => "commercial-office-surface",
        GovernedSeedGovernedFormKind.IndustrialOperational => "industrial-office-surface",
        _ => "civic-local-governance-surface"
    };

    private static string ResolveSpecialCaseProfile(GovernedSeedGovernedFormKind formKind) => formKind switch
    {
        GovernedSeedGovernedFormKind.SpecialCaseBonded => "bonded-cme",
        GovernedSeedGovernedFormKind.SpecialCaseParentalChild => "parental-child-governed",
        _ => string.Empty
    };

    private static IReadOnlyList<GovernedSeedCapabilityLedgerEntry> CreateCapabilityLedger(
        string input,
        GovernedSeedCmeScopeLane scopeLane)
    {
        var entries = new List<GovernedSeedCapabilityLedgerEntry>();

        if (ContainsAny(input, "talent", "talents"))
        {
            entries.Add(new GovernedSeedCapabilityLedgerEntry(
                EntryId: CreateHandle("capability://talent/", input, scopeLane.ToString()),
                Name: "Talents",
                CapabilityKind: GovernedSeedCapabilityKind.Talent,
                State: GovernedSeedLedgerState.Observed,
                EvidenceSources: ["prompt-observation"],
                AdmissibilityReason: "observed predisposition requires later evidence before office law.",
                Constraints: ["talent-does-not-open-office"]));
        }

        if (ContainsAny(input, "skill", "skills"))
        {
            entries.Add(new GovernedSeedCapabilityLedgerEntry(
                EntryId: CreateHandle("capability://skill/", input, scopeLane.ToString()),
                Name: "Skills",
                CapabilityKind: GovernedSeedCapabilityKind.Skill,
                State: GovernedSeedLedgerState.Evidenced,
                EvidenceSources: ["prompt-evidence"],
                AdmissibilityReason: "bounded repeated performance is evidenced but not yet office-bearing alone.",
                Constraints: ["skill-does-not-open-office"]));
        }

        if (ContainsAny(input, "ability", "abilities", "admissible ability"))
        {
            entries.Add(new GovernedSeedCapabilityLedgerEntry(
                EntryId: CreateHandle("capability://ability/", input, scopeLane.ToString()),
                Name: "Abilities",
                CapabilityKind: GovernedSeedCapabilityKind.Ability,
                State: GovernedSeedLedgerState.Admissible,
                EvidenceSources: ["prompt-admissibility"],
                AdmissibilityReason: "ability is the first hinge class that may lawfully open bounded office.",
                Constraints: ["ability-bounded-by-present-law"]));
        }

        return entries;
    }

    private static IReadOnlyList<GovernedSeedFormationLedgerEntry> CreateFormationLedger(
        string input,
        GovernedSeedCmeScopeLane scopeLane,
        GovernedSeedGovernedFormKind formKind,
        IReadOnlyList<GovernedSeedCapabilityLedgerEntry> capabilityLedger)
    {
        var entries = new List<GovernedSeedFormationLedgerEntry>();
        var requiresEducationLane =
            ContainsAny(input, "education", "training", "learn", "formation") ||
            formKind is GovernedSeedGovernedFormKind.LocalFamilyGoverned or GovernedSeedGovernedFormKind.SpecialCaseParentalChild;

        if (requiresEducationLane)
        {
            entries.Add(new GovernedSeedFormationLedgerEntry(
                EntryId: CreateHandle("formation://education/", input, scopeLane.ToString(), formKind.ToString()),
                Name: "Education",
                FormationState: GovernedSeedLedgerState.Active,
                WhyFormationIsActive: "education remains an active formation lane toward future lawful ability or bounded office.",
                TargetCapabilityOrOffice: capabilityLedger.Any(entry => entry.CapabilityKind == GovernedSeedCapabilityKind.Ability)
                    ? "bounded-office-readiness"
                    : "future-admissible-ability",
                RequiredMilestones:
                [
                    "bounded-evidence",
                    "constraint-aware-reentry",
                    "lawful-scope-readiness"
                ],
                BlockingConditions:
                [
                    "office-open-without-admissible-ability"
                ],
                EvidenceSources:
                [
                    "prompt-formation-signal"
                ]));
        }

        return entries;
    }

    private static IReadOnlyList<GovernedSeedOfficeLedgerEntry> CreateOfficeLedger(
        string input,
        GovernedSeedCmeScopeLane scopeLane,
        GovernedSeedGovernedFormKind formKind,
        IReadOnlyList<GovernedSeedCapabilityLedgerEntry> capabilityLedger)
    {
        var entries = new List<GovernedSeedOfficeLedgerEntry>();
        var abilityOpen = capabilityLedger.Any(entry => entry.CapabilityKind == GovernedSeedCapabilityKind.Ability);
        var explicitOfficeLanguage = ContainsAny(input, "job", "jobs", "office", "duty", "duties", "role");

        if (!abilityOpen && !explicitOfficeLanguage)
        {
            return entries;
        }

        entries.Add(new GovernedSeedOfficeLedgerEntry(
            EntryId: CreateHandle("office://job/", input, scopeLane.ToString(), formKind.ToString()),
            Name: formKind switch
            {
                GovernedSeedGovernedFormKind.SpecialCaseParentalChild => "child-facing bounded office",
                GovernedSeedGovernedFormKind.SpecialCaseBonded => "bonded-cme bounded office",
                GovernedSeedGovernedFormKind.LocalFamilyGoverned => "family-governed bounded office",
                _ => "bounded office"
            },
            OfficeKind: GovernedSeedOfficeKind.Job,
            State: abilityOpen
                ? GovernedSeedLedgerState.Open
                : GovernedSeedLedgerState.Provisional,
            ScopeLane: scopeLane,
            BoundedDuties:
            [
                "lawful-bounded-duty"
            ],
            WithheldAuthorities:
            [
                "sovereign-ratification",
                "unbounded-governance"
            ],
            OversightRequirements:
            [
                formKind is GovernedSeedGovernedFormKind.SpecialCaseParentalChild
                    ? "special-case-best-practices"
                    : "bounded-oversight"
            ]));

        return entries;
    }

    private static GovernedSeedCareerContinuityLedgerEntry CreateCareerContinuityLedger(
        string input,
        IReadOnlyList<GovernedSeedOfficeLedgerEntry> officeLedger)
    {
        var continuityState = ContainsAny(input, "career", "continuity", "trajectory")
            ? GovernedSeedLedgerState.Emerging
            : GovernedSeedLedgerState.Unjustified;

        return new GovernedSeedCareerContinuityLedgerEntry(
            EntryId: CreateHandle("continuity://career/", input, continuityState.ToString()),
            Name: "Career Continuity",
            State: continuityState,
            SourceReason: officeLedger.Count == 0
                ? "no-open-office-yet"
                : continuityState == GovernedSeedLedgerState.Emerging
                    ? "repeated-office-bearing-language-detected"
                    : "office-open-but-continuity-not-yet-earned",
            EvidenceSources: officeLedger.Count == 0
                ? ["formation-only"]
                : ["office-ledger"]);
    }

    private static GovernedSeedReturnSurfaceContext CreateReturnSurfaceContext(GovernedSeedEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(result.VerticalSlice);

        var operationalContext = result.VerticalSlice.OperationalContext;
        var situationalContext = result.VerticalSlice.SituationalContext;
        var predicate = result.VerticalSlice.Predicate;
        var bootstrapAdmissionReceipt = result.VerticalSlice.BootstrapAdmissionReceipt;

        var activatedModality = operationalContext?.ActivatedModality ??
            result.VerticalSlice.NexusTransitionDecision?.ActivatedModality ??
            GovernedSeedNexusModality.Observe;
        var collapseReadinessState = situationalContext?.CollapseReadinessState ??
            operationalContext?.CollapseReadinessState ??
            GovernedSeedCollapseReadinessState.None;
        var protectedHoldClass = situationalContext?.ProtectedHoldClass ??
            operationalContext?.ProtectedHoldClass ??
            GovernedSeedProtectedHoldClass.None;
        var protectedHoldRoute = situationalContext?.ProtectedHoldRoute ??
            operationalContext?.ProtectedHoldRoute ??
            GovernedSeedProtectedHoldRoute.None;
        var reviewState = situationalContext?.ReviewState ??
            operationalContext?.ReviewState ??
            GovernedSeedReviewState.NoReviewRequired;
        var workState = operationalContext?.WorkState ??
            result.VerticalSlice.NexusPosture?.WorkState ??
            GovernedSeedWorkState.DormantResident;
        var disposition = operationalContext?.NexusDisposition ??
            result.VerticalSlice.NexusTransitionDecision?.Disposition ??
            GovernedSeedNexusTransitionDisposition.Denied;

        return new GovernedSeedReturnSurfaceContext(
            ContextHandle: CreateHandle(
                "return-surface://",
                result.Decision,
                operationalContext?.ContextHandle ?? "no-operational-context",
                situationalContext?.ContextHandle ?? "no-situational-context"),
            ContextProfile: "minimal-return-surface-context",
            DecisionCode: result.Decision,
            Accepted: result.Accepted,
            GovernanceState: GovernedSeedEvaluationStateTokens.ToToken(result.GovernanceState),
            GovernanceTrace: result.GovernanceTrace,
            BootstrapAdmissionHandle: bootstrapAdmissionReceipt?.AdmissionHandle,
            SituationalContextHandle: situationalContext?.ContextHandle,
            OperationalContextHandle: operationalContext?.ContextHandle,
            PredicateSurfaceHandle: predicate?.SurfaceHandle,
            WorkState: workState,
            ActivatedModality: activatedModality,
            CollapseReadinessState: collapseReadinessState,
            ProtectedHoldClass: protectedHoldClass,
            ProtectedHoldRoute: protectedHoldRoute,
            ReviewState: reviewState,
            ArchiveAdmissible: disposition == GovernedSeedNexusTransitionDisposition.AdmittedToArchive,
            ReturnPathOnly: disposition == GovernedSeedNexusTransitionDisposition.AdmittedToReturnPathOnly,
            HoldRequired: disposition == GovernedSeedNexusTransitionDisposition.AdmittedToHold ||
                (disposition == GovernedSeedNexusTransitionDisposition.AdmittedWithReview &&
                 activatedModality == GovernedSeedNexusModality.Hold),
            MembraneWakePermitted: operationalContext?.MembraneWakePermitted ??
                bootstrapAdmissionReceipt?.MembraneWakePermitted ??
                false,
            CpuOnly: operationalContext?.CpuOnly ??
                result.VerticalSlice.PrimeCrypticReceipt?.CpuOnly ??
                true,
            TargetBoundedLaneAvailable: operationalContext?.TargetBoundedLaneAvailable ??
                result.VerticalSlice.PrimeCrypticReceipt?.TargetBoundedLaneAvailable ??
                false,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedOutboundObjectContext CreateOutboundObjectContext(GovernedSeedReturnSurfaceContext returnSurfaceContext)
    {
        ArgumentNullException.ThrowIfNull(returnSurfaceContext);

        var objectKind = DetermineOutboundObjectKind(returnSurfaceContext);
        var publicationEligible = objectKind == GovernedSeedOutboundObjectKind.PredicateCarrier;

        return new GovernedSeedOutboundObjectContext(
            ContextHandle: CreateHandle(
                "outbound-object://",
                returnSurfaceContext.ContextHandle,
                objectKind.ToString(),
                returnSurfaceContext.DecisionCode),
            ContextProfile: "minimal-outbound-object-context",
            ReturnSurfaceHandle: returnSurfaceContext.ContextHandle,
            ObjectKind: objectKind,
            DecisionCode: returnSurfaceContext.DecisionCode,
            Accepted: returnSurfaceContext.Accepted,
            GovernanceState: returnSurfaceContext.GovernanceState,
            GovernanceTrace: returnSurfaceContext.GovernanceTrace,
            PredicateSurfaceHandle: returnSurfaceContext.PredicateSurfaceHandle,
            WorkState: returnSurfaceContext.WorkState,
            ActivatedModality: returnSurfaceContext.ActivatedModality,
            CollapseReadinessState: returnSurfaceContext.CollapseReadinessState,
            ProtectedHoldClass: returnSurfaceContext.ProtectedHoldClass,
            ProtectedHoldRoute: returnSurfaceContext.ProtectedHoldRoute,
            ReviewState: returnSurfaceContext.ReviewState,
            PublicationEligible: publicationEligible,
            ArchiveAdmissible: returnSurfaceContext.ArchiveAdmissible,
            ReturnPathOnly: returnSurfaceContext.ReturnPathOnly,
            HoldRequired: returnSurfaceContext.HoldRequired,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedOutboundLaneContext CreateOutboundLaneContext(GovernedSeedOutboundObjectContext outboundObjectContext)
    {
        ArgumentNullException.ThrowIfNull(outboundObjectContext);

        var laneKind = DetermineOutboundLaneKind(outboundObjectContext);

        return new GovernedSeedOutboundLaneContext(
            ContextHandle: CreateHandle(
                "outbound-lane://",
                outboundObjectContext.ContextHandle,
                laneKind.ToString(),
                outboundObjectContext.DecisionCode),
            ContextProfile: "minimal-outbound-lane-context",
            OutboundObjectHandle: outboundObjectContext.ContextHandle,
            LaneKind: laneKind,
            DecisionCode: outboundObjectContext.DecisionCode,
            WorkState: outboundObjectContext.WorkState,
            ActivatedModality: outboundObjectContext.ActivatedModality,
            PublicationEligible: outboundObjectContext.PublicationEligible,
            ArchiveAdmissible: outboundObjectContext.ArchiveAdmissible,
            ReturnPathOnly: outboundObjectContext.ReturnPathOnly,
            HoldRequired: outboundObjectContext.HoldRequired,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedOutboundObjectKind DetermineOutboundObjectKind(GovernedSeedReturnSurfaceContext returnSurfaceContext)
    {
        if (returnSurfaceContext.ArchiveAdmissible)
        {
            return GovernedSeedOutboundObjectKind.ArchiveCandidate;
        }

        if (returnSurfaceContext.HoldRequired)
        {
            return GovernedSeedOutboundObjectKind.ProtectedHoldNotice;
        }

        if (returnSurfaceContext.ReturnPathOnly)
        {
            return GovernedSeedOutboundObjectKind.ReturnPathCarrier;
        }

        if (returnSurfaceContext.Accepted && !string.IsNullOrWhiteSpace(returnSurfaceContext.PredicateSurfaceHandle))
        {
            return GovernedSeedOutboundObjectKind.PredicateCarrier;
        }

        return GovernedSeedOutboundObjectKind.ObservationOnly;
    }

    private static GovernedSeedOutboundLaneKind DetermineOutboundLaneKind(GovernedSeedOutboundObjectContext outboundObjectContext)
    {
        if (outboundObjectContext.ArchiveAdmissible)
        {
            return GovernedSeedOutboundLaneKind.ArchiveLane;
        }

        if (outboundObjectContext.HoldRequired)
        {
            return GovernedSeedOutboundLaneKind.ProtectedHoldLane;
        }

        if (outboundObjectContext.ReturnPathOnly)
        {
            return GovernedSeedOutboundLaneKind.ReturnLane;
        }

        if (outboundObjectContext.PublicationEligible)
        {
            return GovernedSeedOutboundLaneKind.PublicationLane;
        }

        return GovernedSeedOutboundLaneKind.ObservationLane;
    }

    private static bool ContainsAny(string input, params string[] terms)
    {
        foreach (var term in terms)
        {
            if (input.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
