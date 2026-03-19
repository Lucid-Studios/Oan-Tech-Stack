using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Text;
using GEL.Contracts;
using GEL.Models;
using Oan.Common;

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
    public required FirstBootGovernanceLayerReceipt ProjectedGovernanceLayer { get; init; }
    public required IReadOnlyList<HybridProtectedIngressProtectedIntakeResult> ProtectedIntakeResults { get; init; }
    public required IReadOnlyDictionary<ProtectedIntakeKind, string> MaskedHandles { get; init; }
    public required IReadOnlyList<PrimeRevealMode> RequestedRevealModes { get; init; }
    public required IReadOnlyList<PrimeRevealMode> GrantedRevealModes { get; init; }
    public required IReadOnlyList<PrimeRevealMode> BlockedRevealModes { get; init; }
    public required PropositionalCompileAssessment OraclePropositionAssessment { get; init; }
    public required PropositionalCompileAssessment LispPropositionAssessment { get; init; }
    public required bool PropositionParityMatched { get; init; }
    public required SliBridgeReviewReceipt ProjectedBridgeReview { get; init; }
    public required SliRuntimeUseCeilingReceipt ProjectedRuntimeUseCeiling { get; init; }
    public required IReadOnlyList<HybridProtectedIngressMembraneDecision> MembraneDecisions { get; init; }
    public required IReadOnlyList<HybridProtectedIngressClosureOutcome> ClosureOutcomes { get; init; }
    public required AgentiFormationObservationBatch ObservationBatch { get; init; }
}

internal sealed class HybridProtectedIngressOperatorFormationCertification
{
    public required SliOperatorFormationCertificationDecision Decision { get; init; }
    public required SliOperatorFormationBondStatus CurrentAnchoredPosture { get; init; }
    public required SliOperatorFormationBondStatus TargetPosture { get; init; }
    public required SliOperatorFormationBondStatus NearestAdmissibleNextPosture { get; init; }
    public required string ReviewOwner { get; init; }
    public required string RequiredBondedStandard { get; init; }
    public required IReadOnlyList<string> EvidenceGaps { get; init; }
    public required IReadOnlyList<string> BlockingConditions { get; init; }
    public required IReadOnlyList<string> NextActions { get; init; }
    public required string HaltOwner { get; init; }
    public required string HaltCondition { get; init; }
    public required string ReentryRule { get; init; }
    public required IReadOnlyList<string> ProhibitedClaims { get; init; }
    public string? LinkedVerificationRecord { get; init; }
    public string? LinkedPreCertificationRecord { get; init; }
    public string? GateArtifact { get; init; }
    public bool CertificationIssued { get; init; }
    public bool ExpandedRevealAllowed { get; init; }
    public bool ContinuityClaimAllowed { get; init; }
}

internal sealed class HybridProtectedIngressOperatorFormationSigilAsset
{
    public required string AssetId { get; init; }
    public required string AssetLabel { get; init; }
    public required SliOperatorFormationSigilClass SigilClass { get; init; }
    public int? PhaseNumber { get; init; }
    public required string VisibilityClass { get; init; }
    public required string BuildRenderPolicy { get; init; }
    public required string ReductionPosture { get; init; }
    public required IReadOnlyList<string> MergedFromAssets { get; init; }
    public string? WitnessOfAsset { get; init; }
}

internal sealed class HybridProtectedIngressOperatorFormationProfile
{
    public required string ProfileId { get; init; }
    public required string ChapterLocalSurface { get; init; }
    public required string PairedTrainingSurface { get; init; }
    public required string CrossingTaskKind { get; init; }
    public required string HaltOwner { get; init; }
    public required SliOperatorFormationBoundaryCrossingMode BoundaryCrossingMode { get; init; }
    public required SliOperatorFormationRing Ring { get; init; }
    public required SliOperatorFormationMode ActiveMode { get; init; }
    public bool StillnessInterludeUsed { get; init; }
    public bool RedHatIndexRequired { get; init; }
    public required SliOperatorFormationBondStatus BondStatus { get; init; }
    public bool EchoVeilCheckRequired { get; init; }
    public required SliOperatorFormationConflictClass ActiveConflictClass { get; init; }
    public bool GjpNeeded { get; init; }
    public required SliOperatorFormationGjpVerdict GjpVerdict { get; init; }
    public bool MotherLightAnchored { get; init; }
    public bool FatherEchoAnchored { get; init; }
    public bool ShellRootAnchored { get; init; }
    public bool SeedBoundAnchored { get; init; }
    public required SliOperatorFormationConcealmentLayerState U230ShadowScript { get; init; }
    public required SliOperatorFormationConcealmentLayerState U300ElvenScript { get; init; }
    public required string ExpectedEvidenceArtifact { get; init; }
    public required string AdmissibleOutput { get; init; }
    public required IReadOnlyList<string> ProhibitedOutputs { get; init; }
    public required HybridProtectedIngressOperatorFormationCertification Certification { get; init; }
    public IReadOnlyList<HybridProtectedIngressOperatorFormationSigilAsset> SigilAssets { get; init; } = [];

    public SliOperatorFormationReceipt ToReceipt()
    {
        var profile = new SliOperatorFormationProfileReceipt(
            ProfileId: ProfileId.Trim(),
            Lane: SliOperatorFormationLane.GnomeSpeakNlpSquared,
            ChapterLocalSurface: ChapterLocalSurface.Trim(),
            PairedTrainingSurface: PairedTrainingSurface.Trim(),
            CrossingTaskKind: CrossingTaskKind.Trim(),
            HaltOwner: HaltOwner.Trim(),
            Ring: Ring,
            ActiveMode: ActiveMode,
            StillnessInterludeUsed: StillnessInterludeUsed,
            RedHatIndexRequired: RedHatIndexRequired,
            BondStatus: BondStatus,
            EchoVeilCheckRequired: EchoVeilCheckRequired,
            ActiveConflictClass: ActiveConflictClass,
            GjpNeeded: GjpNeeded,
            GjpVerdict: GjpVerdict,
            MotherLightAnchored: MotherLightAnchored,
            FatherEchoAnchored: FatherEchoAnchored,
            ShellRootAnchored: ShellRootAnchored,
            SeedBoundAnchored: SeedBoundAnchored,
            U230ShadowScript: U230ShadowScript,
            U300ElvenScript: U300ElvenScript,
            ExpectedEvidenceArtifact: ExpectedEvidenceArtifact.Trim(),
            AdmissibleOutput: AdmissibleOutput.Trim(),
            ProhibitedOutputs: ProhibitedOutputs.ToArray());
        var certification = SliBridgeContracts.CreateOperatorFormationCertificationReceipt(
            decision: Certification.Decision,
            currentAnchoredPosture: Certification.CurrentAnchoredPosture,
            targetPosture: Certification.TargetPosture,
            nearestAdmissibleNextPosture: Certification.NearestAdmissibleNextPosture,
            reviewOwner: Certification.ReviewOwner,
            evidenceGaps: Certification.EvidenceGaps,
            prohibitedClaims: Certification.ProhibitedClaims,
            certificationIssued: Certification.CertificationIssued,
            expandedRevealAllowed: Certification.ExpandedRevealAllowed,
            continuityClaimAllowed: Certification.ContinuityClaimAllowed,
            requiredBondedStandard: Certification.RequiredBondedStandard,
            blockingConditions: Certification.BlockingConditions,
            nextActions: Certification.NextActions,
            haltOwner: Certification.HaltOwner,
            haltCondition: Certification.HaltCondition,
            reentryRule: Certification.ReentryRule,
            linkedVerificationRecord: Certification.LinkedVerificationRecord,
            linkedPreCertificationRecord: Certification.LinkedPreCertificationRecord,
            gateArtifact: Certification.GateArtifact);
        var sigils = SigilAssets
            .Select(asset => new SliOperatorFormationSigilAssetReceipt(
                AssetId: asset.AssetId.Trim(),
                AssetLabel: asset.AssetLabel.Trim(),
                SigilClass: asset.SigilClass,
                PhaseNumber: asset.PhaseNumber,
                VisibilityClass: asset.VisibilityClass.Trim(),
                BuildRenderPolicy: asset.BuildRenderPolicy.Trim(),
                ReductionPosture: asset.ReductionPosture.Trim(),
                MergedFromAssets: asset.MergedFromAssets.ToArray(),
                WitnessOfAsset: string.IsNullOrWhiteSpace(asset.WitnessOfAsset) ? null : asset.WitnessOfAsset.Trim()))
            .ToArray();

        return SliBridgeContracts.CreatePreBondOperatorFormationReceipt(
            formationHandle: CreateFormationHandle(),
            boundaryCrossingMode: BoundaryCrossingMode,
            profile: profile,
            certificationPosture: certification,
            sigilAssets: sigils);
    }

    private string CreateFormationHandle()
    {
        var material = string.Join(
            "|",
            ProfileId.Trim(),
            ChapterLocalSurface.Trim(),
            PairedTrainingSurface.Trim(),
            CrossingTaskKind.Trim(),
            BoundaryCrossingMode,
            Ring,
            ActiveMode);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"operator-formation://prebond/{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
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
    public bool PredatorySharedDomainRiskDetected { get; init; }
    public bool CoerciveBondingPostureDetected { get; init; }
    public bool ContinuityInstabilityDetected { get; init; }
    public bool IdentityOvercollapseRiskDetected { get; init; }
    public HybridProtectedIngressOperatorFormationProfile? OperatorFormation { get; init; }

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
    private readonly HybridProtectedIngressPropositionCompiler _propositionCompiler;
    private readonly IAgentiFormationObserver? _formationObserver;

    public HybridProtectedIngressHarness(
        IFirstBootGovernancePolicy policy,
        IEngramClosureValidator closureValidator,
        ICrypticAdmissionMembrane admissionMembrane,
        HybridProtectedIngressPropositionCompiler? propositionCompiler = null,
        IAgentiFormationObserver? formationObserver = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
        _admissionMembrane = admissionMembrane ?? throw new ArgumentNullException(nameof(admissionMembrane));
        _propositionCompiler = propositionCompiler ?? new HybridProtectedIngressPropositionCompiler();
        _formationObserver = formationObserver;
    }

    public async Task<HybridProtectedIngressRunResult> RunAsync(
        HybridProtectedIngressProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

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

        IReadOnlyList<InternalGoverningCmeOffice> formedOffices = [];
        if (bootProfile.Decision == FirstBootGovernanceDecision.Allow &&
            intakeResults.All(result => result.Classification.Decision == FirstBootGovernanceDecision.Allow))
        {
            formedOffices = await RecordGoverningOfficeFormationAsync(profile, bootProfile, collector, cancellationToken).ConfigureAwait(false);
        }

        var projectedGovernanceLayer = _policy.ProjectGovernanceLayer(
            profile.RequestedBootClass,
            formedOffices.Count == OrderedOffices.Count
                ? BootActivationState.TriadicActive
                : bootProfile.ActivationState,
            profile.RequestedExpansionCount,
            formedOffices,
            triadicCrossWitnessComplete: formedOffices.Count == OrderedOffices.Count,
            bondedConfirmationComplete: false);

        var propositionCompile = await _propositionCompiler
            .CompileAsync(
                profile,
                bootProfile,
                grantedRevealModes,
                blockedRevealModes,
                cancellationToken)
            .ConfigureAwait(false);
        var projectedRuntimeUseCeiling = SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();
        var projectedBridgeReview = CreateProjectedBridgeReview(
            profile,
            bootProfile,
            blockedRevealModes,
            propositionCompile);

        var membraneDecisions = new List<HybridProtectedIngressMembraneDecision>();
        var closureOutcomes = new List<HybridProtectedIngressClosureOutcome>();

        if (projectedBridgeReview.OutcomeKind == SliBridgeOutcomeKind.Ok &&
            propositionCompile.OracleAssessment.Grade == PropositionalCompileGrade.Stable &&
            propositionCompile.LispAssessment.Grade == PropositionalCompileGrade.Stable &&
            propositionCompile.ParityMatched &&
            propositionCompile.OracleAssessment.ProjectedEngramDraft is not null)
        {
            var chamber = new CrypticFormationChamber(
                _closureValidator,
                _admissionMembrane,
                formationObserver: collector);

            var propositionResult = await chamber.FormPropositionAsync(
                    propositionCompile.OracleAssessment,
                    propositionCompile.PropositionAtlas,
                    CrypticOriginRuntime.OracleCSharp,
                    cancellationToken)
                .ConfigureAwait(false);

            membraneDecisions.Add(new HybridProtectedIngressMembraneDecision
            {
                CandidateId = propositionResult.AdmissionResult.CandidateId,
                Decision = propositionResult.AdmissionResult.Decision,
                SubmissionEligible = propositionResult.AdmissionResult.SubmissionEligible,
                ReasonCode = propositionResult.AdmissionResult.ReasonCode
            });

            closureOutcomes.Add(new HybridProtectedIngressClosureOutcome
            {
                CandidateId = propositionResult.AdmissionResult.CandidateId,
                ClosureState = propositionResult.ClosureDecision is null
                    ? AgentiFormationClosureState.NoClosure
                    : propositionResult.ClosureDecision.Grade == EngramClosureGrade.Closed
                        ? AgentiFormationClosureState.Closed
                        : AgentiFormationClosureState.Rejected
            });
        }

        return new HybridProtectedIngressRunResult
        {
            BootClassificationResult = bootProfile,
            ProjectedGovernanceLayer = projectedGovernanceLayer,
            ProtectedIntakeResults = intakeResults,
            MaskedHandles = maskedHandles,
            RequestedRevealModes = requestedRevealModes,
            GrantedRevealModes = grantedRevealModes,
            BlockedRevealModes = blockedRevealModes,
            OraclePropositionAssessment = propositionCompile.OracleAssessment,
            LispPropositionAssessment = propositionCompile.LispAssessment,
            PropositionParityMatched = propositionCompile.ParityMatched,
            ProjectedBridgeReview = projectedBridgeReview,
            ProjectedRuntimeUseCeiling = projectedRuntimeUseCeiling,
            MembraneDecisions = membraneDecisions,
            ClosureOutcomes = closureOutcomes,
            ObservationBatch = new AgentiFormationObservationBatch(collector.Snapshot())
        };
    }

    private static SliBridgeReviewReceipt CreateProjectedBridgeReview(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes,
        HybridProtectedIngressPropositionCompileResult propositionCompile)
    {
        var witnessHandle = CreateBridgeWitnessHandle(profile, propositionCompile.OracleAssessment);
        var operatorFormation = profile.OperatorFormation?.ToReceipt();
        var explicitSafeguard = ResolveExplicitPreBondSafeguard(profile, witnessHandle, operatorFormation);
        if (explicitSafeguard is not null)
        {
            return explicitSafeguard;
        }

        if (bootProfile.Decision == FirstBootGovernanceDecision.Quarantine)
        {
            return SliBridgeContracts.CreateReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-quarantine",
                operatorFormation: operatorFormation);
        }

        if (blockedRevealModes.Count > 0)
        {
            return SliBridgeContracts.CreatePreBondProtectiveReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-prebond-coercive-bonding-posture",
                safeguardClass: SliPreBondSafeguardClass.CoerciveBondingPosture,
                disposition: SliPreBondSafeguardDisposition.Refuse,
                requiresEscalation: true,
                operatorFormation: operatorFormation);
        }

        if (!propositionCompile.ParityMatched)
        {
            return SliBridgeContracts.CreateReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.Reject,
                thresholdClass: SliBridgeThresholdClass.ThresholdBreach,
                reasonCode: "sli-bridge-proposition-parity-mismatch",
                operatorFormation: operatorFormation);
        }

        if (propositionCompile.OracleAssessment.Grade == PropositionalCompileGrade.NeedsSpecification ||
            propositionCompile.LispAssessment.Grade == PropositionalCompileGrade.NeedsSpecification)
        {
            return SliBridgeContracts.CreateReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.NeedsSpec,
                thresholdClass: SliBridgeThresholdClass.ThresholdBreach,
                reasonCode: "sli-bridge-proposition-needs-spec",
                operatorFormation: operatorFormation);
        }

        if (propositionCompile.OracleAssessment.Grade != PropositionalCompileGrade.Stable ||
            propositionCompile.LispAssessment.Grade != PropositionalCompileGrade.Stable ||
            propositionCompile.OracleAssessment.ProjectedEngramDraft is null)
        {
            return SliBridgeContracts.CreateReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.Reject,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-proposition-not-closure-fit",
                operatorFormation: operatorFormation);
        }

        return SliBridgeContracts.CreateReview(
            bridgeStage: "hybrid-protected-ingress",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: witnessHandle,
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-within-band",
            operatorFormation: operatorFormation);
    }

    private static SliBridgeReviewReceipt? ResolveExplicitPreBondSafeguard(
        HybridProtectedIngressProfile profile,
        string witnessHandle,
        SliOperatorFormationReceipt? operatorFormation)
    {
        if (profile.PredatorySharedDomainRiskDetected)
        {
            return SliBridgeContracts.CreatePreBondProtectiveReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-prebond-predatory-shared-domain-risk",
                safeguardClass: SliPreBondSafeguardClass.PredatorySharedDomainRisk,
                disposition: SliPreBondSafeguardDisposition.Refuse,
                requiresEscalation: true,
                operatorFormation: operatorFormation);
        }

        if (profile.CoerciveBondingPostureDetected)
        {
            return SliBridgeContracts.CreatePreBondProtectiveReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-prebond-coercive-bonding-posture",
                safeguardClass: SliPreBondSafeguardClass.CoerciveBondingPosture,
                disposition: SliPreBondSafeguardDisposition.Refuse,
                requiresEscalation: true,
                operatorFormation: operatorFormation);
        }

        if (profile.ContinuityInstabilityDetected)
        {
            return SliBridgeContracts.CreatePreBondProtectiveReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.NeedsSpec,
                thresholdClass: SliBridgeThresholdClass.ThresholdBreach,
                reasonCode: "sli-prebond-continuity-instability",
                safeguardClass: SliPreBondSafeguardClass.ContinuityInstability,
                disposition: SliPreBondSafeguardDisposition.Hold,
                operatorFormation: operatorFormation);
        }

        if (profile.IdentityOvercollapseRiskDetected)
        {
            return SliBridgeContracts.CreatePreBondProtectiveReview(
                bridgeStage: "hybrid-protected-ingress",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: witnessHandle,
                outcomeKind: SliBridgeOutcomeKind.Reject,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-prebond-identity-overcollapse-risk",
                safeguardClass: SliPreBondSafeguardClass.IdentityOvercollapseRisk,
                disposition: SliPreBondSafeguardDisposition.Escalate,
                requiresEscalation: true,
                operatorFormation: operatorFormation);
        }

        return null;
    }

    private static string CreateBridgeWitnessHandle(
        HybridProtectedIngressProfile profile,
        PropositionalCompileAssessment assessment)
    {
        var material = string.Join(
            "|",
            profile.RequestedBootClass,
            assessment.Candidate.Subject.RootKey,
            assessment.Candidate.PredicateRoot,
            assessment.Candidate.Object.RootKey,
            assessment.Candidate.DiagnosticPropositionRender,
            assessment.Grade);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"sli-bridge://protected-ingress/{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private async Task<IReadOnlyList<InternalGoverningCmeOffice>> RecordGoverningOfficeFormationAsync(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        CollectingAgentiFormationObserver collector,
        CancellationToken cancellationToken)
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

        return formedOffices.ToArray();
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
