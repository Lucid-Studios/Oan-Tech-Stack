using Oan.Common;

namespace Oan.Cradle;

public sealed record FirstBootGovernanceObservationResult(
    AgentiFormationObservationBatch ObservationBatch,
    InternalGovernanceBootProfile BootClassificationResult,
    FirstBootGovernanceLayerReceipt ProjectedGovernanceLayer);

public sealed class FirstBootFormationObservationHarness
{
    private readonly IFirstBootGovernancePolicy _policy;
    private readonly IAgentiFormationObserver? _formationObserver;

    public FirstBootFormationObservationHarness(
        IFirstBootGovernancePolicy policy,
        IAgentiFormationObserver? formationObserver = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _formationObserver = formationObserver;
    }

    public async Task<AgentiFormationObservationBatch> ObserveAsync(
        BootClass bootClass,
        ProtectedIntakeKind intakeKind,
        PrimeRevealMode requestedRevealMode,
        int requestedExpansionCount = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await ObserveWithGovernanceLayerAsync(
            bootClass,
            intakeKind,
            requestedRevealMode,
            requestedExpansionCount,
            cancellationToken).ConfigureAwait(false);

        return result.ObservationBatch;
    }

    public async Task<FirstBootGovernanceObservationResult> ObserveWithGovernanceLayerAsync(
        BootClass bootClass,
        ProtectedIntakeKind intakeKind,
        PrimeRevealMode requestedRevealMode,
        int requestedExpansionCount = 1,
        CancellationToken cancellationToken = default)
    {
        var observations = new List<AgentiFormationObservation>(capacity: 6);

        var bootProfile = _policy.EvaluateBootProfile(
            bootClass,
            BootActivationState.Classified,
            requestedExpansionCount);

        observations.Add(await CreateAndRecordAsync(
            stage: AgentiFormationObservationStage.BootClassification,
            bootClass: bootClass,
            activationState: bootProfile.ActivationState,
            expansionRights: bootProfile.ExpansionRights,
            office: null,
            admissionDecision: null,
            closureState: AgentiFormationClosureState.NotSubmitted,
            revealMode: null,
            source: AgentiFormationObservationSource.FirstBootPolicy,
            submissionEligible: false,
            observationTags:
            [
                $"decision:{bootProfile.Decision}",
                $"reason:{bootProfile.ReasonCode}",
                $"swarm:{bootProfile.SwarmEligibility}"
            ],
            cancellationToken).ConfigureAwait(false));

        var intakeClassification = _policy.ClassifyProtectedIntake(
            intakeKind,
            protectedHandle: ResolveProtectedHandle(intakeKind),
            requestedRevealMode);

        observations.Add(await CreateAndRecordAsync(
            stage: AgentiFormationObservationStage.ProtectedIntakePosture,
            bootClass: bootClass,
            activationState: bootProfile.ActivationState,
            expansionRights: bootProfile.ExpansionRights,
            office: null,
            admissionDecision: null,
            closureState: AgentiFormationClosureState.NotSubmitted,
            revealMode: intakeClassification.EffectiveRevealMode,
            source: AgentiFormationObservationSource.FirstBootPolicy,
            submissionEligible: false,
            observationTags:
            [
                $"decision:{intakeClassification.Decision}",
                $"reason:{intakeClassification.ReasonCode}",
                $"intake:{intakeKind}",
                $"masked:{intakeClassification.MaskedView.StructuralLabels["masking_state"]}"
            ],
            cancellationToken).ConfigureAwait(false));

        IReadOnlyList<InternalGoverningCmeOffice> formedOffices = [];
        foreach (var office in OrderedOffices)
        {
            var record = _policy.EvaluateFormationEligibility(
                new InternalGoverningCmeFormationRequest(
                    bootClass,
                    BootActivationState.GovernanceForming,
                    office,
                    AlreadyFormedOffices: observations
                        .Where(item => item.Stage == AgentiFormationObservationStage.GoverningOfficeFormation && item.Office is not null)
                        .Select(item => item.Office!.Value)
                        .ToArray(),
                    TriadicCrossWitnessComplete: false,
                    BondedConfirmationComplete: false));

            if (record.Decision != FirstBootGovernanceDecision.Allow)
            {
                throw new InvalidOperationException(
                    $"First-boot harness encountered unexpected policy denial for office {office}: {record.ReasonCode}.");
            }

            observations.Add(await CreateAndRecordAsync(
                stage: AgentiFormationObservationStage.GoverningOfficeFormation,
                bootClass: bootClass,
                activationState: record.ActivationState,
                expansionRights: bootProfile.ExpansionRights,
                office: office,
                admissionDecision: null,
                closureState: AgentiFormationClosureState.NotSubmitted,
                revealMode: null,
                source: AgentiFormationObservationSource.FirstBootPolicy,
                submissionEligible: false,
                observationTags:
                [
                    $"decision:{record.Decision}",
                        $"reason:{record.ReasonCode}"
                    ],
                cancellationToken).ConfigureAwait(false));
        }

        formedOffices = observations
            .Where(item => item.Stage == AgentiFormationObservationStage.GoverningOfficeFormation && item.Office is not null)
            .Select(item => item.Office!.Value)
            .ToArray();

        observations.Add(await CreateAndRecordAsync(
            stage: AgentiFormationObservationStage.TriadicCrossWitness,
            bootClass: bootClass,
            activationState: BootActivationState.TriadicActive,
            expansionRights: bootProfile.ExpansionRights,
            office: null,
            admissionDecision: null,
            closureState: AgentiFormationClosureState.NotSubmitted,
            revealMode: null,
            source: AgentiFormationObservationSource.FirstBootPolicy,
            submissionEligible: false,
            observationTags:
            [
                "triadic-cross-witness:complete",
                "bonded-confirmation:pending"
            ],
            cancellationToken).ConfigureAwait(false));

        var governanceLayer = _policy.ProjectGovernanceLayer(
            bootClass,
            BootActivationState.TriadicActive,
            requestedExpansionCount,
            formedOffices,
            triadicCrossWitnessComplete: true,
            bondedConfirmationComplete: false);

        return new FirstBootGovernanceObservationResult(
            new AgentiFormationObservationBatch(observations),
            bootProfile,
            governanceLayer);
    }

    private async Task<AgentiFormationObservation> CreateAndRecordAsync(
        AgentiFormationObservationStage stage,
        BootClass bootClass,
        BootActivationState activationState,
        ExpansionRights expansionRights,
        InternalGoverningCmeOffice? office,
        CrypticAdmissionDecision? admissionDecision,
        AgentiFormationClosureState closureState,
        PrimeRevealMode? revealMode,
        AgentiFormationObservationSource source,
        bool submissionEligible,
        IReadOnlyList<string> observationTags,
        CancellationToken cancellationToken)
    {
        var observation = new AgentiFormationObservation(
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
            OriginRuntime: AgentiFormationOriginRuntime.OracleCSharp,
            Source: source,
            SubmissionEligible: submissionEligible,
            ObservationTags: observationTags,
            Timestamp: DateTimeOffset.UtcNow);

        if (_formationObserver is not null)
        {
            await _formationObserver.RecordAsync(observation, cancellationToken).ConfigureAwait(false);
        }

        return observation;
    }

    private static string ResolveProtectedHandle(ProtectedIntakeKind intakeKind)
    {
        return intakeKind switch
        {
            ProtectedIntakeKind.HumanProtectedIntake => "HumanPrincipal_A",
            ProtectedIntakeKind.CorporateProtectedIntake => "CorporatePrincipal_A",
            _ => throw new ArgumentOutOfRangeException(nameof(intakeKind), intakeKind, null)
        };
    }

    private static readonly IReadOnlyList<InternalGoverningCmeOffice> OrderedOffices =
    [
        InternalGoverningCmeOffice.Steward,
        InternalGoverningCmeOffice.Father,
        InternalGoverningCmeOffice.Mother
    ];
}
