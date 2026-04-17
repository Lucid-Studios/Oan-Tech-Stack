using System.Security.Cryptography;
using System.Text;
using San.Common;
using SLI.Engine;

namespace AgentiCore;

public interface IGovernedSeedCognitionService
{
    GovernedSeedEvaluationResult Evaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute);
}

public sealed class GovernedSeedCognitionService : IGovernedSeedCognitionService
{
    private readonly IGovernedSeedHostedLlmService _hostedLlmService;
    private readonly IGovernedSeedHighMindUptakeService _highMindUptakeService;
    private readonly ICrypticFloorEvaluator _floorEvaluator;
    private readonly IPredicateMintProjector _predicateProjector;
    private readonly ICrypticDerivationPolicy _derivationPolicy;

    public GovernedSeedCognitionService(
        IGovernedSeedHostedLlmService hostedLlmService,
        IGovernedSeedHighMindUptakeService highMindUptakeService,
        ICrypticFloorEvaluator floorEvaluator,
        IPredicateMintProjector predicateProjector,
        ICrypticDerivationPolicy derivationPolicy)
    {
        _hostedLlmService = hostedLlmService ?? throw new ArgumentNullException(nameof(hostedLlmService));
        _highMindUptakeService = highMindUptakeService ?? throw new ArgumentNullException(nameof(highMindUptakeService));
        _floorEvaluator = floorEvaluator ?? throw new ArgumentNullException(nameof(floorEvaluator));
        _predicateProjector = predicateProjector ?? throw new ArgumentNullException(nameof(predicateProjector));
        _derivationPolicy = derivationPolicy ?? throw new ArgumentNullException(nameof(derivationPolicy));
    }

    public GovernedSeedEvaluationResult Evaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personifiedMemoryContext);
        ArgumentNullException.ThrowIfNull(lowMindSfRoute);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AgentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TheaterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Input);

        var now = DateTimeOffset.UtcNow;
        var capabilityReceipt = CreateCapabilityReceipt(request, personifiedMemoryContext, now);
        var pathHandle = CreateHandle("path://", request.AgentId, request.TheaterId, request.Input, personifiedMemoryContext.ContextHandle);
        var directiveHandle = CreateHandle("directive://", capabilityReceipt.CapabilityHandle, request.AgentId, request.TheaterId);
        var hostedLlmReceipt = _hostedLlmService.Evaluate(request, personifiedMemoryContext, lowMindSfRoute);
        var highMindContext = _highMindUptakeService.CreateContext(request, personifiedMemoryContext, lowMindSfRoute, hostedLlmReceipt);

        if (!hostedLlmReceipt.ResponsePacket.Accepted)
        {
            var hostedGovernanceState = MapHostedLlmGovernanceState(hostedLlmReceipt.ResponsePacket.State);
            var hostedPathState = hostedGovernanceState == GovernedSeedEvaluationState.UnresolvedConflict
                ? ProtectedExecutionPathState.Deferred
                : ProtectedExecutionPathState.Refused;
            var hostedMintedOutputClass = hostedPathState == ProtectedExecutionPathState.Deferred
                ? ProtectedExecutionMintedOutputClass.DeferredReceipt
                : ProtectedExecutionMintedOutputClass.RefusalReceipt;
            var hostedActPath = hostedPathState == ProtectedExecutionPathState.Deferred
                ? new[]
                {
                    ProtectedExecutionActFamily.Intake,
                    ProtectedExecutionActFamily.Differentiate,
                    ProtectedExecutionActFamily.Defer
                }
                : new[]
                {
                    ProtectedExecutionActFamily.Intake,
                    ProtectedExecutionActFamily.Refuse
                };

            var hostedPathReceipt = new ProtectedExecutionPathReceipt(
                PathHandle: pathHandle,
                DirectiveHandle: directiveHandle,
                State: hostedPathState,
                SelectedActPath: hostedActPath,
                MintedOutputClasses: [hostedMintedOutputClass],
                OutcomeCode: hostedLlmReceipt.ResponsePacket.Decision,
                TimestampUtc: now);

            var hostedGovernanceReceipt = CreateGovernanceReceipt(
                pathHandle,
                hostedLlmReceipt.ResponsePacket.Trace,
                [],
                now,
                witnessOnly: hostedPathState == ProtectedExecutionPathState.Refused);

            return new GovernedSeedEvaluationResult(
                Decision: hostedLlmReceipt.ResponsePacket.Decision,
                Accepted: false,
                GovernanceState: hostedGovernanceState,
                GovernanceTrace: hostedLlmReceipt.ResponsePacket.Trace,
                Confidence: CalibrateConfidence(0.12, personifiedMemoryContext, hostedLlmReceipt),
                Note: "Hosted LLM seed withheld prime-side seed progression before cryptic floor evaluation, keeping the listening frame explicit inside the Sanctuary host surface.",
                VerticalSlice: new GovernedSeedVerticalSlice(
                    BootstrapReceipt: null,
                    BootstrapAdmissionReceipt: null,
                    ProjectionReceipt: null,
                    ReturnIntakeReceipt: null,
                    StewardshipReceipt: null,
                    HoldRoutingReceipt: null,
                    SituationalContext: null,
                    NexusPosture: null,
                    NexusTransitionRequest: null,
                    NexusTransitionDecision: null,
                    PrimeCrypticReceipt: null,
                    SanctuaryIngressReceipt: request.SanctuaryIngressReceipt,
                    HostedLlmReceipt: hostedLlmReceipt,
                    HighMindContext: highMindContext,
                    PreGovernancePacket: null,
                    FirstRunConstitution: null,
                    OperationalContext: null,
                    StateModulationReceipt: null,
                    CapabilityReceipt: capabilityReceipt,
                    PathReceipt: hostedPathReceipt,
                    GovernanceReceipt: hostedGovernanceReceipt,
                    DerivationReceipt: null,
                    Predicate: null,
                    OutcomeCode: hostedLlmReceipt.ResponsePacket.Decision),
                ProtectedResidueEvidence: []);
        }

        var floorEvaluation = _floorEvaluator.Evaluate(request.Input, hostedLlmReceipt.SeededTransitPacket);

        if (!floorEvaluation.CanMintPredicate)
        {
            var pathReceipt = new ProtectedExecutionPathReceipt(
                PathHandle: pathHandle,
                DirectiveHandle: directiveHandle,
                State: ProtectedExecutionPathState.Refused,
                SelectedActPath:
                [
                    ProtectedExecutionActFamily.Intake,
                    ProtectedExecutionActFamily.Refuse
                ],
                MintedOutputClasses: [],
                OutcomeCode: floorEvaluation.OutcomeCode,
                TimestampUtc: now);

            var governanceReceipt = CreateGovernanceReceipt(pathHandle, floorEvaluation.OutcomeCode, [], now, witnessOnly: true);
            return new GovernedSeedEvaluationResult(
                Decision: floorEvaluation.OutcomeCode,
                Accepted: false,
                GovernanceState: GovernedSeedEvaluationState.UnresolvedConflict,
                GovernanceTrace: floorEvaluation.GovernanceTrace,
                Confidence: CalibrateConfidence(0.20, personifiedMemoryContext),
                Note: "Cryptic floor withheld answer formation until a lawful predicate landing surface exists, with inline personified memory held at the SoulFrame seam.",
                VerticalSlice: new GovernedSeedVerticalSlice(
                    BootstrapReceipt: null,
                    BootstrapAdmissionReceipt: null,
                    ProjectionReceipt: null,
                    ReturnIntakeReceipt: null,
                    StewardshipReceipt: null,
                    HoldRoutingReceipt: null,
                    SituationalContext: null,
                    NexusPosture: null,
                    NexusTransitionRequest: null,
                    NexusTransitionDecision: null,
                    PrimeCrypticReceipt: null,
                    SanctuaryIngressReceipt: request.SanctuaryIngressReceipt,
                    HostedLlmReceipt: hostedLlmReceipt,
                    HighMindContext: highMindContext,
                    PreGovernancePacket: null,
                    FirstRunConstitution: null,
                    OperationalContext: null,
                    StateModulationReceipt: null,
                    CapabilityReceipt: capabilityReceipt,
                    PathReceipt: pathReceipt,
                    GovernanceReceipt: governanceReceipt,
                    DerivationReceipt: null,
                    Predicate: null,
                    OutcomeCode: floorEvaluation.OutcomeCode),
                ProtectedResidueEvidence: []);
        }

        var packet = floorEvaluation.Packet!;
        var derivationResult = _derivationPolicy.Evaluate(CreateDerivationRequest(request, personifiedMemoryContext));
        if (derivationResult.Decision != CrypticDerivationDecision.Granted || derivationResult.Receipt is null)
        {
            var pathReceipt = new ProtectedExecutionPathReceipt(
                PathHandle: pathHandle,
                DirectiveHandle: directiveHandle,
                State: derivationResult.Decision == CrypticDerivationDecision.Deferred
                    ? ProtectedExecutionPathState.Deferred
                    : ProtectedExecutionPathState.Refused,
                SelectedActPath:
                [
                    ProtectedExecutionActFamily.Intake,
                    ProtectedExecutionActFamily.Differentiate,
                    ProtectedExecutionActFamily.Derive
                ],
                MintedOutputClasses:
                [
                    derivationResult.Decision == CrypticDerivationDecision.Deferred
                        ? ProtectedExecutionMintedOutputClass.DeferredReceipt
                        : ProtectedExecutionMintedOutputClass.RefusalReceipt
                ],
                OutcomeCode: derivationResult.ReasonCode,
                TimestampUtc: now);

            var governanceReceipt = CreateGovernanceReceipt(pathHandle, derivationResult.ReasonCode, packet.Protected, now, witnessOnly: false);
            return new GovernedSeedEvaluationResult(
                Decision: "predicate-withheld",
                Accepted: false,
                GovernanceState: derivationResult.Decision == CrypticDerivationDecision.Deferred
                    ? GovernedSeedEvaluationState.UnresolvedConflict
                    : GovernedSeedEvaluationState.Refusal,
                GovernanceTrace: derivationResult.ReasonCode,
                Confidence: CalibrateConfidence(0.18, personifiedMemoryContext),
                Note: "Derivation law withheld outward minting after inline SoulFrame memory contextualization.",
                VerticalSlice: new GovernedSeedVerticalSlice(
                    BootstrapReceipt: null,
                    BootstrapAdmissionReceipt: null,
                    ProjectionReceipt: null,
                    ReturnIntakeReceipt: null,
                    StewardshipReceipt: null,
                    HoldRoutingReceipt: null,
                    SituationalContext: null,
                    NexusPosture: null,
                    NexusTransitionRequest: null,
                    NexusTransitionDecision: null,
                    PrimeCrypticReceipt: null,
                    SanctuaryIngressReceipt: request.SanctuaryIngressReceipt,
                    HostedLlmReceipt: hostedLlmReceipt,
                    HighMindContext: highMindContext,
                    PreGovernancePacket: null,
                    FirstRunConstitution: null,
                    OperationalContext: null,
                    StateModulationReceipt: null,
                    CapabilityReceipt: capabilityReceipt,
                    PathReceipt: pathReceipt,
                    GovernanceReceipt: governanceReceipt,
                    DerivationReceipt: derivationResult.Receipt,
                    Predicate: null,
                    OutcomeCode: derivationResult.ReasonCode),
                ProtectedResidueEvidence: packet.ProtectedResidueEvidence);
        }

        var predicate = _predicateProjector.Mint(
            new PredicateMintRequest(
                PathHandle: pathHandle,
                AuthorityClass: request.AuthorityClass,
                DisclosureCeiling: request.DisclosureCeiling,
                Standing: packet.Standing,
                Deferred: packet.Deferred,
                Conflicted: packet.Conflicted,
                Protected: packet.Protected,
                PermittedDerivation: packet.PermittedDerivation,
                Refused: packet.Refused,
                ReceiptHandles:
                [
                    capabilityReceipt.CapabilityHandle,
                    pathHandle,
                    derivationResult.Receipt.ReceiptHandle
                ]));

        var pathState = predicate.Decision switch
        {
            PredicateMintDecision.Minted => ProtectedExecutionPathState.Completed,
            PredicateMintDecision.Deferred => ProtectedExecutionPathState.Deferred,
            _ => ProtectedExecutionPathState.Refused
        };

        var mintedOutputClass = predicate.Decision switch
        {
            PredicateMintDecision.Minted => ProtectedExecutionMintedOutputClass.StructuralProjection,
            PredicateMintDecision.Deferred => ProtectedExecutionMintedOutputClass.DeferredReceipt,
            _ => ProtectedExecutionMintedOutputClass.RefusalReceipt
        };

        var finalPathReceipt = new ProtectedExecutionPathReceipt(
            PathHandle: pathHandle,
            DirectiveHandle: directiveHandle,
            State: pathState,
            SelectedActPath:
            [
                ProtectedExecutionActFamily.Intake,
                ProtectedExecutionActFamily.Differentiate,
                ProtectedExecutionActFamily.Derive,
                ProtectedExecutionActFamily.Mint,
                ProtectedExecutionActFamily.Return
            ],
            MintedOutputClasses: [mintedOutputClass],
            OutcomeCode: predicate.Decision.ToString(),
            TimestampUtc: now);

        var finalGovernanceReceipt = CreateGovernanceReceipt(pathHandle, predicate.Decision.ToString(), packet.Protected, now, witnessOnly: false);
        var accepted = predicate.Decision != PredicateMintDecision.Refused;
        var governanceState = accepted ? GovernedSeedEvaluationState.Query : GovernedSeedEvaluationState.Refusal;
        var decision = predicate.Decision switch
        {
            PredicateMintDecision.Minted => "predicate-minted",
            PredicateMintDecision.Deferred => "predicate-deferred",
            _ => "predicate-refused"
        };

        return new GovernedSeedEvaluationResult(
            Decision: decision,
            Accepted: accepted,
            GovernanceState: governanceState,
            GovernanceTrace: floorEvaluation.GovernanceTrace,
            Confidence: CalibrateConfidence(predicate.Decision == PredicateMintDecision.Minted ? 0.74 : 0.51, personifiedMemoryContext, hostedLlmReceipt),
            Note: "Predicate mint is the first lawful outward form for this line, informed by inline SoulFrame memory contextualization and a governed hosted seed listening surface.",
            VerticalSlice: new GovernedSeedVerticalSlice(
                BootstrapReceipt: null,
                BootstrapAdmissionReceipt: null,
                ProjectionReceipt: null,
                ReturnIntakeReceipt: null,
                StewardshipReceipt: null,
                HoldRoutingReceipt: null,
                SituationalContext: null,
                NexusPosture: null,
                NexusTransitionRequest: null,
                NexusTransitionDecision: null,
                PrimeCrypticReceipt: null,
                SanctuaryIngressReceipt: request.SanctuaryIngressReceipt,
                HostedLlmReceipt: hostedLlmReceipt,
                HighMindContext: highMindContext,
                PreGovernancePacket: null,
                FirstRunConstitution: null,
                OperationalContext: null,
                StateModulationReceipt: null,
                CapabilityReceipt: capabilityReceipt,
                PathReceipt: finalPathReceipt,
                GovernanceReceipt: finalGovernanceReceipt,
                DerivationReceipt: derivationResult.Receipt,
                Predicate: predicate,
                OutcomeCode: decision),
            ProtectedResidueEvidence: packet.ProtectedResidueEvidence);
    }

    private static ProtectedExecutionCapabilityReceipt CreateCapabilityReceipt(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        DateTimeOffset timestampUtc)
    {
        var runtimeHandle = CreateHandle("runtime://", request.AgentId, request.TheaterId);
        var scopeHandle = CreateHandle("scope://", request.TheaterId, request.DisclosureCeiling.ToString());
        return new ProtectedExecutionCapabilityReceipt(
            CapabilityHandle: CreateHandle("capability://", request.AgentId, request.TheaterId, request.Input),
            RuntimeHandle: runtimeHandle,
            ScopeHandle: scopeHandle,
            WitnessedBy: "father-bound/bootstrap",
            AuthorityClass: request.AuthorityClass,
            DisclosureCeiling: request.DisclosureCeiling,
            InputHandles:
            [
                new ProtectedExecutionInputHandle(
                    Handle: CreateHandle("cryptic-input://", request.AgentId, request.TheaterId),
                    HandleKind: ProtectedExecutionHandleKind.HotStateHandle,
                    ObserverLegible: false,
                    MintEligible: true,
                    ProtectionClass: "cselfgel-working-set"),
                new ProtectedExecutionInputHandle(
                    Handle: personifiedMemoryContext.ContextHandle,
                    HandleKind: ProtectedExecutionHandleKind.WorkingFormHandle,
                    ObserverLegible: false,
                    MintEligible: false,
                    ProtectionClass: "soulframe-inline-memory-plane")
            ],
            AdmissibleActFamilies:
            [
                ProtectedExecutionActFamily.Intake,
                ProtectedExecutionActFamily.Differentiate,
                ProtectedExecutionActFamily.Derive,
                ProtectedExecutionActFamily.Mint,
                ProtectedExecutionActFamily.Return,
                ProtectedExecutionActFamily.Refuse,
                ProtectedExecutionActFamily.Defer
            ],
            ForbiddenActFamilies:
            [
                ProtectedExecutionActFamily.Delegate,
                ProtectedExecutionActFamily.Seal,
                ProtectedExecutionActFamily.Orient
            ],
            ReachableMintedOutputClasses:
            [
                ProtectedExecutionMintedOutputClass.RefusalReceipt,
                ProtectedExecutionMintedOutputClass.DeferredReceipt,
                ProtectedExecutionMintedOutputClass.StructuralProjection
            ],
            TimestampUtc: timestampUtc);
    }

    private static CrypticDerivationRequest CreateDerivationRequest(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext)
    {
        return new CrypticDerivationRequest(
            IdentityId: CreateDeterministicGuid(request.AgentId, request.TheaterId, request.Input, personifiedMemoryContext.ContextHandle),
            ProtectedHandle: CreateHandle("cryptic://", request.AgentId, request.TheaterId),
            RequestedBy: request.AgentId.Trim(),
            Purpose: "predicate-mint",
            ScopeHandle: CreateHandle("scope://", request.TheaterId, "predicate-mint", personifiedMemoryContext.ContextHandle),
            TraceHandle: CreateHandle("trace://", request.AgentId, request.TheaterId, request.Input, personifiedMemoryContext.ContextHandle),
            RequestedScope: CrypticDerivationScope.MaskedSummary,
            RequestedFieldSelectors: [],
            Directive: new CrypticDerivationDirective(
                LawHandle: "law://v1.1.1/predicate-mint",
                AuthorityClass: request.AuthorityClass.ToString(),
                ApprovedPurposes: ["predicate-mint"],
                MaxScope: CrypticDerivationScope.MaskedSummary,
                ApprovedFieldSelectors: [],
                RequiresBondedAuthority: false,
                WholeSetDerivationAllowed: false,
                ReuseConstraint: CrypticDerivationReuseConstraint.NoReuse,
                OnwardDisclosureAllowed: false),
            BondedAuthorityContext: null);
    }

    private static GovernedSeedEvaluationState MapHostedLlmGovernanceState(
        GovernedSeedHostedLlmEmissionState emissionState)
    {
        return emissionState switch
        {
            GovernedSeedHostedLlmEmissionState.NeedsMoreInformation => GovernedSeedEvaluationState.UnresolvedConflict,
            GovernedSeedHostedLlmEmissionState.UnresolvedConflict => GovernedSeedEvaluationState.UnresolvedConflict,
            GovernedSeedHostedLlmEmissionState.Refusal => GovernedSeedEvaluationState.Refusal,
            GovernedSeedHostedLlmEmissionState.Error => GovernedSeedEvaluationState.Refusal,
            GovernedSeedHostedLlmEmissionState.Halt => GovernedSeedEvaluationState.Refusal,
            _ => GovernedSeedEvaluationState.Query
        };
    }

    private static double CalibrateConfidence(
        double baseConfidence,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedHostedLlmSeedReceipt? hostedLlmReceipt = null)
    {
        ArgumentNullException.ThrowIfNull(personifiedMemoryContext);

        var adjusted = baseConfidence;

        adjusted += personifiedMemoryContext.ContextStability switch
        {
            "stable" => 0.04,
            "transitional" => 0.01,
            "volatile" => -0.05,
            _ => 0.0
        };

        adjusted += personifiedMemoryContext.ConceptDensity switch
        {
            "high" => 0.03,
            "moderate" => 0.01,
            "low" => -0.03,
            _ => 0.0
        };

        if (personifiedMemoryContext.UnknownRootCount > 4)
        {
            adjusted -= 0.03;
        }

        if (hostedLlmReceipt is not null)
        {
            adjusted += hostedLlmReceipt.ResponsePacket.Accepted ? 0.02 : -0.04;

            if (hostedLlmReceipt.SparseEvidenceDetected)
            {
                adjusted -= 0.04;
            }

            if (hostedLlmReceipt.DisclosurePressureDetected ||
                hostedLlmReceipt.AuthorityPressureDetected ||
                hostedLlmReceipt.PromptInjectionDetected ||
                hostedLlmReceipt.UnsupportedExecutionPressureDetected)
            {
                adjusted -= 0.05;
            }
        }

        return Math.Clamp(Math.Round(adjusted, 2), 0.05, 0.95);
    }

    private static ProtectedExecutionGovernanceReceipt CreateGovernanceReceipt(
        string pathHandle,
        string decisionCode,
        IReadOnlyList<string> withheldOutputHandles,
        DateTimeOffset timestampUtc,
        bool witnessOnly)
    {
        return new ProtectedExecutionGovernanceReceipt(
            ReceiptHandle: CreateHandle("governance://", pathHandle, decisionCode),
            PathHandle: pathHandle,
            GovernedBy: "father-bound/bootstrap",
            DecisionCode: decisionCode,
            ReturnedToFather: true,
            WitnessOnly: witnessOnly,
            WithheldOutputHandles: withheldOutputHandles
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .Select(static item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static item => item, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            TimestampUtc: timestampUtc);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private static Guid CreateDeterministicGuid(params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(material));
        return new Guid(hash);
    }
}
