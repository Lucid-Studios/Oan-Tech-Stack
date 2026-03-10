using System.Text.Json;
using System.Text.Json.Serialization;
using GEL.Contracts;
using GEL.Models;
using Oan.Common;
using SLI.Ingestion;

namespace Oan.Cradle;

internal sealed class HybridProtectedIngressProtectedIntakeResult
{
    public required ProtectedIntakeKind IntakeKind { get; init; }
    public required PrimeRevealMode RequestedRevealMode { get; init; }
    public required ProtectedIntakeClassificationResult Classification { get; init; }
}

internal sealed class HybridProtectedIngressMembraneDecision
{
    public required Guid CandidateId { get; init; }
    public required CrypticAdmissionDecision Decision { get; init; }
    public required bool SubmissionEligible { get; init; }
    public required string ReasonCode { get; init; }
}

internal sealed class HybridProtectedIngressClosureOutcome
{
    public required Guid CandidateId { get; init; }
    public required AgentiFormationClosureState ClosureState { get; init; }
}

internal sealed class HybridProtectedIngressRunResult
{
    public required InternalGovernanceBootProfile BootClassificationResult { get; init; }
    public required IReadOnlyList<HybridProtectedIngressProtectedIntakeResult> ProtectedIntakeResults { get; init; }
    public required IReadOnlyDictionary<ProtectedIntakeKind, string> MaskedHandles { get; init; }
    public required IReadOnlyList<PrimeRevealMode> RequestedRevealModes { get; init; }
    public required IReadOnlyList<PrimeRevealMode> GrantedRevealModes { get; init; }
    public required IReadOnlyList<PrimeRevealMode> BlockedRevealModes { get; init; }
    public required IReadOnlyList<HybridProtectedIngressMembraneDecision> MembraneDecisions { get; init; }
    public required IReadOnlyList<HybridProtectedIngressClosureOutcome> ClosureOutcomes { get; init; }
    public required AgentiFormationObservationBatch ObservationBatch { get; init; }
}

internal sealed class HybridProtectedIngressProfile
{
    public required string HumanPrincipalName { get; init; }
    public required string CorporatePrincipalName { get; init; }
    public required string AuthorityRelationship { get; init; }
    public required string HumanCredentialId { get; init; }
    public required string CorporateRegistryId { get; init; }
    public required string AuthorityToken { get; init; }
    public required string AddressHandle { get; init; }
    public required BootClass RequestedBootClass { get; init; }
    public required int RequestedExpansionCount { get; init; }
    public required IReadOnlyList<PrimeRevealMode> RequestedRevealModes { get; init; }
    public required bool BondedAuthorityConfirmed { get; init; }
    public required IReadOnlyList<string> ApprovedRevealPurposes { get; init; }
    public required string FormationSentence { get; init; }

    public BondedAuthorityContext ToBondedAuthorityContext()
    {
        return new BondedAuthorityContext(
            AuthorityId: AuthorityToken,
            AuthorityClass: AuthorityRelationship,
            BondedConfirmed: BondedAuthorityConfirmed,
            ApprovedRevealPurposes: ApprovedRevealPurposes);
    }

    public static HybridProtectedIngressProfile LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HybridProtectedIngressProfile>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Hybrid protected-ingress profile could not be parsed.");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}

internal sealed class HybridProtectedIngressHarness
{
    private const string HumanHandle = "HumanPrincipal_A";
    private const string CorporateHandle = "CorporatePrincipal_A";
    private readonly IFirstBootGovernancePolicy _policy;
    private readonly IEngramClosureValidator _closureValidator;
    private readonly ICrypticAdmissionMembrane _admissionMembrane;
    private readonly IAgentiFormationObserver? _formationObserver;

    public HybridProtectedIngressHarness(
        IFirstBootGovernancePolicy policy,
        IEngramClosureValidator closureValidator,
        ICrypticAdmissionMembrane admissionMembrane,
        IAgentiFormationObserver? formationObserver = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
        _admissionMembrane = admissionMembrane ?? throw new ArgumentNullException(nameof(admissionMembrane));
        _formationObserver = formationObserver;
    }

    public async Task<HybridProtectedIngressRunResult> RunAsync(
        HybridProtectedIngressProfile profile,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);

        var collector = new CollectingAgentiFormationObserver(_formationObserver);
        var requestedRevealModes = NormalizeRevealModes(profile.RequestedRevealModes);

        var bootProfile = _policy.EvaluateBootProfile(
            profile.RequestedBootClass,
            BootActivationState.Classified,
            profile.RequestedExpansionCount);

        await collector.RecordAsync(
            CreateObservation(
                stage: AgentiFormationObservationStage.BootClassification,
                source: AgentiFormationObservationSource.FirstBootPolicy,
                bootClass: profile.RequestedBootClass,
                activationState: bootProfile.ActivationState,
                expansionRights: bootProfile.ExpansionRights,
                office: null,
                admissionDecision: null,
                closureState: AgentiFormationClosureState.NotSubmitted,
                revealMode: null,
                originRuntime: AgentiFormationOriginRuntime.OracleCSharp,
                submissionEligible: false,
                observationTags:
                [
                    $"decision:{bootProfile.Decision}",
                    $"reason:{bootProfile.ReasonCode}",
                    $"swarm:{bootProfile.SwarmEligibility}"
                ]),
            cancellationToken).ConfigureAwait(false);

        var intakeResults = new List<HybridProtectedIngressProtectedIntakeResult>(requestedRevealModes.Length * 2);
        var maskedHandles = new Dictionary<ProtectedIntakeKind, string>
        {
            [ProtectedIntakeKind.HumanProtectedIntake] = HumanHandle,
            [ProtectedIntakeKind.CorporateProtectedIntake] = CorporateHandle
        };

        foreach (var intakeKind in OrderedIntakeKinds)
        {
            foreach (var revealMode in requestedRevealModes)
            {
                var classification = _policy.ClassifyProtectedIntake(
                    intakeKind,
                    maskedHandles[intakeKind],
                    revealMode,
                    profile.ToBondedAuthorityContext());

                intakeResults.Add(new HybridProtectedIngressProtectedIntakeResult
                {
                    IntakeKind = intakeKind,
                    RequestedRevealMode = revealMode,
                    Classification = classification
                });

                await collector.RecordAsync(
                    CreateObservation(
                        stage: AgentiFormationObservationStage.ProtectedIntakePosture,
                        source: AgentiFormationObservationSource.FirstBootPolicy,
                        bootClass: profile.RequestedBootClass,
                        activationState: bootProfile.ActivationState,
                        expansionRights: bootProfile.ExpansionRights,
                        office: null,
                        admissionDecision: null,
                        closureState: AgentiFormationClosureState.NotSubmitted,
                        revealMode: classification.EffectiveRevealMode,
                        originRuntime: AgentiFormationOriginRuntime.OracleCSharp,
                        submissionEligible: false,
                        observationTags:
                        [
                            $"decision:{classification.Decision}",
                            $"reason:{classification.ReasonCode}",
                            $"intake:{intakeKind}",
                            $"requested-reveal:{revealMode}",
                            $"handle:{classification.MaskedView.ProtectedHandle}"
                        ]),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        var grantedRevealModes = requestedRevealModes
            .Where(mode => intakeResults
                .Where(result => result.RequestedRevealMode == mode)
                .All(result => result.Classification.Decision == FirstBootGovernanceDecision.Allow))
            .ToArray();

        var blockedRevealModes = requestedRevealModes
            .Where(mode => intakeResults
                .Where(result => result.RequestedRevealMode == mode)
                .Any(result => result.Classification.Decision != FirstBootGovernanceDecision.Allow))
            .ToArray();

        var membraneDecisions = new List<HybridProtectedIngressMembraneDecision>();
        var closureOutcomes = new List<HybridProtectedIngressClosureOutcome>();

        if (bootProfile.Decision == FirstBootGovernanceDecision.Allow &&
            intakeResults.All(result => result.Classification.Decision == FirstBootGovernanceDecision.Allow))
        {
            var formedOffices = new List<InternalGoverningCmeOffice>(capacity: 3);
            foreach (var office in OrderedOffices)
            {
                var formation = _policy.EvaluateFormationEligibility(
                    new InternalGoverningCmeFormationRequest(
                        BootClass: profile.RequestedBootClass,
                        ActivationState: BootActivationState.GovernanceForming,
                        Office: office,
                        AlreadyFormedOffices: formedOffices.ToArray(),
                        TriadicCrossWitnessComplete: false,
                        BondedConfirmationComplete: false));

                if (formation.Decision != FirstBootGovernanceDecision.Allow)
                {
                    throw new InvalidOperationException(
                        $"Hybrid protected-ingress harness encountered unexpected office denial for {office}: {formation.ReasonCode}.");
                }

                formedOffices.Add(office);

                await collector.RecordAsync(
                    CreateObservation(
                        stage: AgentiFormationObservationStage.GoverningOfficeFormation,
                        source: AgentiFormationObservationSource.FirstBootPolicy,
                        bootClass: profile.RequestedBootClass,
                        activationState: formation.ActivationState,
                        expansionRights: bootProfile.ExpansionRights,
                        office: office,
                        admissionDecision: null,
                        closureState: AgentiFormationClosureState.NotSubmitted,
                        revealMode: null,
                        originRuntime: AgentiFormationOriginRuntime.OracleCSharp,
                        submissionEligible: false,
                        observationTags:
                        [
                            $"decision:{formation.Decision}",
                            $"reason:{formation.ReasonCode}"
                        ]),
                    cancellationToken).ConfigureAwait(false);
            }

            await collector.RecordAsync(
                CreateObservation(
                    stage: AgentiFormationObservationStage.TriadicCrossWitness,
                    source: AgentiFormationObservationSource.FirstBootPolicy,
                    bootClass: profile.RequestedBootClass,
                    activationState: BootActivationState.TriadicActive,
                    expansionRights: bootProfile.ExpansionRights,
                    office: null,
                    admissionDecision: null,
                    closureState: AgentiFormationClosureState.NotSubmitted,
                    revealMode: null,
                    originRuntime: AgentiFormationOriginRuntime.OracleCSharp,
                    submissionEligible: false,
                    observationTags:
                    [
                        "triadic-cross-witness:complete",
                        "bonded-confirmation:pending"
                    ]),
                cancellationToken).ConfigureAwait(false);

            var chamber = new CrypticFormationChamber(
                _closureValidator,
                _admissionMembrane,
                formationObserver: collector);

            var sentenceResult = await chamber.FormSentenceAsync(
                profile.FormationSentence,
                atlas,
                overlayRoots,
                cancellationToken).ConfigureAwait(false);

            membraneDecisions.Add(new HybridProtectedIngressMembraneDecision
            {
                CandidateId = sentenceResult.AdmissionResult.CandidateId,
                Decision = sentenceResult.AdmissionResult.Decision,
                SubmissionEligible = sentenceResult.AdmissionResult.SubmissionEligible,
                ReasonCode = sentenceResult.AdmissionResult.ReasonCode
            });

            closureOutcomes.Add(new HybridProtectedIngressClosureOutcome
            {
                CandidateId = sentenceResult.AdmissionResult.CandidateId,
                ClosureState = sentenceResult.ClosureDecision is null
                    ? AgentiFormationClosureState.NoClosure
                    : sentenceResult.ClosureDecision.Grade == EngramClosureGrade.Closed
                        ? AgentiFormationClosureState.Closed
                        : AgentiFormationClosureState.Rejected
            });
        }

        return new HybridProtectedIngressRunResult
        {
            BootClassificationResult = bootProfile,
            ProtectedIntakeResults = intakeResults,
            MaskedHandles = maskedHandles,
            RequestedRevealModes = requestedRevealModes,
            GrantedRevealModes = grantedRevealModes,
            BlockedRevealModes = blockedRevealModes,
            MembraneDecisions = membraneDecisions,
            ClosureOutcomes = closureOutcomes,
            ObservationBatch = new AgentiFormationObservationBatch(collector.Snapshot())
        };
    }

    private static AgentiFormationObservation CreateObservation(
        AgentiFormationObservationStage stage,
        AgentiFormationObservationSource source,
        BootClass? bootClass,
        BootActivationState? activationState,
        ExpansionRights? expansionRights,
        InternalGoverningCmeOffice? office,
        CrypticAdmissionDecision? admissionDecision,
        AgentiFormationClosureState closureState,
        PrimeRevealMode? revealMode,
        AgentiFormationOriginRuntime originRuntime,
        bool submissionEligible,
        IReadOnlyList<string> observationTags)
    {
        return new AgentiFormationObservation(
            ObservationId: Guid.NewGuid(),
            Stage: stage,
            CandidateId: null,
            BootClass: bootClass,
            ActivationState: activationState,
            ExpansionRights: expansionRights,
            Office: office,
            AdmissionDecision: admissionDecision,
            ClosureState: closureState,
            RevealMode: revealMode,
            OriginRuntime: originRuntime,
            Source: source,
            SubmissionEligible: submissionEligible,
            ObservationTags: observationTags,
            Timestamp: DateTimeOffset.UtcNow);
    }

    private static PrimeRevealMode[] NormalizeRevealModes(IReadOnlyList<PrimeRevealMode> revealModes)
    {
        var modes = (revealModes.Count == 0 ? [PrimeRevealMode.None] : revealModes)
            .Distinct()
            .ToArray();

        return modes.Length == 0 ? [PrimeRevealMode.None] : modes;
    }

    private static readonly IReadOnlyList<InternalGoverningCmeOffice> OrderedOffices =
    [
        InternalGoverningCmeOffice.Steward,
        InternalGoverningCmeOffice.Father,
        InternalGoverningCmeOffice.Mother
    ];

    private static readonly IReadOnlyList<ProtectedIntakeKind> OrderedIntakeKinds =
    [
        ProtectedIntakeKind.HumanProtectedIntake,
        ProtectedIntakeKind.CorporateProtectedIntake
    ];

    private sealed class CollectingAgentiFormationObserver : IAgentiFormationObserver
    {
        private readonly List<AgentiFormationObservation> _observations = [];
        private readonly IAgentiFormationObserver? _forwardObserver;
        private readonly object _gate = new();

        public CollectingAgentiFormationObserver(IAgentiFormationObserver? forwardObserver)
        {
            _forwardObserver = forwardObserver;
        }

        public async Task RecordAsync(
            AgentiFormationObservation observation,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_gate)
            {
                _observations.Add(observation);
            }

            if (_forwardObserver is not null)
            {
                await _forwardObserver.RecordAsync(observation, cancellationToken).ConfigureAwait(false);
            }
        }

        public IReadOnlyList<AgentiFormationObservation> Snapshot()
        {
            lock (_gate)
            {
                return _observations.ToArray();
            }
        }
    }
}
