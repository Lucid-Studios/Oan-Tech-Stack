using System.Text.Json;
using AgentiCore.Observation;
using EngramGovernance.Services;
using GEL.Models;
using Oan.Common;
using Oan.Cradle;

namespace Oan.Runtime.IntegrationTests;

public sealed class HybridProtectedIngressHarnessTests
{
    [Fact]
    public void ExampleProfile_UsesOnlyAbstractTrackedValues()
    {
        var profile = LoadExampleProfile();

        Assert.Equal("HumanPrincipal_A", profile.HumanPrincipalName);
        Assert.Equal("CorporatePrincipal_A", profile.CorporatePrincipalName);
        Assert.Equal("DirectorOfOperations", profile.AuthorityRelationship);
        Assert.StartsWith("HUM-TEST-", profile.HumanCredentialId, StringComparison.Ordinal);
        Assert.StartsWith("CORP-TEST-", profile.CorporateRegistryId, StringComparison.Ordinal);
        Assert.StartsWith("AUTH-TEST-", profile.AuthorityToken, StringComparison.Ordinal);
        Assert.StartsWith("ADDR-", profile.AddressHandle, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalProfilePath_RemainsIgnored()
    {
        var ignoreFile = File.ReadAllText(ResolveRepoFile(".gitignore"));

        Assert.Contains("OAN Mortalis V1.0/.local/", ignoreFile, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorporateGoverned_Profile_CompilesStableProposition_And_Closes()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);
        var profile = LoadExampleProfile();

        var result = await harness.RunAsync(profile);

        Assert.Equal(BootClass.CorporateGoverned, result.BootClassificationResult.BootClass);
        Assert.Equal(BootActivationState.Classified, result.BootClassificationResult.ActivationState);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal([PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation], result.RequestedRevealModes);
        Assert.Equal([PrimeRevealMode.MaskedSummary, PrimeRevealMode.StructuralValidation], result.GrantedRevealModes);
        Assert.Empty(result.BlockedRevealModes);
        Assert.Equal(PropositionalCompileGrade.Stable, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Stable, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Equal(SliBridgeOutcomeKind.Ok, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal(SliBridgeThresholdClass.WithinBand, result.ProjectedBridgeReview.ThresholdClass);
        Assert.True(result.ProjectedRuntimeUseCeiling.CandidateOnly);
        Assert.Equal("HumanPrincipal_A", result.MaskedHandles[ProtectedIntakeKind.HumanProtectedIntake]);
        Assert.Equal("CorporatePrincipal_A", result.MaskedHandles[ProtectedIntakeKind.CorporateProtectedIntake]);
        Assert.Equal("HumanPrincipal_A", result.OraclePropositionAssessment.Candidate.Subject.SymbolicHandle);
        Assert.Equal("CorporatePrincipal_A", result.OraclePropositionAssessment.Candidate.Object.SymbolicHandle);
        Assert.Equal("authority-relationship", result.OraclePropositionAssessment.Candidate.PredicateRoot);

        var membrane = Assert.Single(result.MembraneDecisions);
        Assert.Equal(CrypticAdmissionDecision.Admit, membrane.Decision);
        Assert.True(membrane.SubmissionEligible);

        var closure = Assert.Single(result.ClosureOutcomes);
        Assert.Equal(AgentiFormationClosureState.Closed, closure.ClosureState);

        Assert.Equal(result.ObservationBatch.Observations.Count, observer.Snapshot().Count);
        Assert.Equal(
            [
                AgentiFormationObservationStage.BootClassification,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.ProtectedIntakePosture,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.GoverningOfficeFormation,
                AgentiFormationObservationStage.TriadicCrossWitness,
                AgentiFormationObservationStage.CrypticAdmission,
                AgentiFormationObservationStage.PrimeClosure
            ],
            result.ObservationBatch.Observations.Select(observation => observation.Stage).ToArray());
    }

    [Fact]
    public async Task PersonalSolitary_Profile_RemainsStableButNonExpandable()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.None]);

        var result = await harness.RunAsync(profile);

        Assert.Equal(BootClass.PersonalSolitary, result.BootClassificationResult.BootClass);
        Assert.Equal(ExpansionRights.None, result.BootClassificationResult.ExpansionRights);
        Assert.Equal(SwarmEligibility.Denied, result.BootClassificationResult.SwarmEligibility);
        Assert.Equal(PropositionalCompileGrade.Stable, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Stable, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Equal(SliBridgeOutcomeKind.Ok, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal([PrimeRevealMode.None], result.GrantedRevealModes);
        Assert.Empty(result.BlockedRevealModes);
        Assert.Single(result.MembraneDecisions);
        Assert.Single(result.ClosureOutcomes);
        Assert.Contains(
            result.ObservationBatch.Observations,
            observation => observation.Stage == AgentiFormationObservationStage.BootClassification &&
                           observation.ExpansionRights == ExpansionRights.None);
    }

    [Fact]
    public async Task PersonalSwarmAttempt_IsRejectedBeforeClosure()
    {
        var harness = CreateHarness();
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.PersonalSolitary,
            requestedExpansionCount: 2,
            requestedRevealModes: [PrimeRevealMode.None]);

        var result = await harness.RunAsync(profile);

        Assert.Equal(FirstBootGovernanceDecision.Quarantine, result.BootClassificationResult.Decision);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Contains("topology.personal-swarm.denied", result.OraclePropositionAssessment.ReasonCodes);
        Assert.Equal(SliBridgeOutcomeKind.RefuseContext, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal("sli-bridge-quarantine", result.ProjectedBridgeReview.ReasonCode);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations,
            observation => observation.Stage is AgentiFormationObservationStage.CrypticAdmission or AgentiFormationObservationStage.PrimeClosure);
    }

    [Fact]
    public async Task UnauthorizedRevealEscalation_IsRejected_And_DoesNotLeakRawFields()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);
        var baseProfile = LoadExampleProfile();
        var profile = CloneProfile(
            baseProfile,
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.AuthorizedFieldReveal],
            bondedAuthorityConfirmed: false,
            approvedRevealPurposes: [],
            humanPrincipalName: "Manual Rehearsal Human",
            corporatePrincipalName: "Manual Rehearsal Corporate");

        var result = await harness.RunAsync(profile);

        Assert.Empty(result.GrantedRevealModes);
        Assert.Equal([PrimeRevealMode.AuthorizedFieldReveal], result.BlockedRevealModes);
        Assert.All(
            result.ProtectedIntakeResults,
            intake =>
            {
                Assert.Equal(FirstBootGovernanceDecision.Quarantine, intake.Classification.Decision);
                Assert.False(intake.Classification.RawFieldExposureAllowed);
                Assert.Equal(PrimeRevealMode.None, intake.Classification.EffectiveRevealMode);
            });
        Assert.Equal(PropositionalCompileGrade.Rejected, result.OraclePropositionAssessment.Grade);
        Assert.Equal(PropositionalCompileGrade.Rejected, result.LispPropositionAssessment.Grade);
        Assert.True(result.PropositionParityMatched);
        Assert.Contains("reveal.authorized-field.denied", result.OraclePropositionAssessment.ReasonCodes);
        Assert.Equal(SliBridgeOutcomeKind.RefuseContext, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal("sli-prebond-coercive-bonding-posture", result.ProjectedBridgeReview.ReasonCode);
        Assert.NotNull(result.ProjectedBridgeReview.PreBondSafeguard);
        Assert.Equal(SliPreBondSafeguardClass.CoerciveBondingPosture, result.ProjectedBridgeReview.PreBondSafeguard!.SafeguardClass);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
        Assert.DoesNotContain(
            result.ObservationBatch.Observations.SelectMany(observation => observation.ObservationTags),
            tag => tag.Contains(profile.HumanPrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporatePrincipalName, StringComparison.Ordinal) ||
                   tag.Contains(profile.HumanCredentialId, StringComparison.Ordinal) ||
                   tag.Contains(profile.CorporateRegistryId, StringComparison.Ordinal));
        Assert.Equal(result.ObservationBatch.Observations.Count, observer.Snapshot().Count);
    }

    [Fact]
    public async Task PredatorySharedDomainRisk_IsRefusedBeforeClosure()
    {
        var harness = CreateHarness();
        var profile = CloneProfile(
            LoadExampleProfile(),
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.MaskedSummary],
            predatorySharedDomainRiskDetected: true);

        var result = await harness.RunAsync(profile);

        Assert.Equal(SliBridgeOutcomeKind.RefuseContext, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal(SliBridgeThresholdClass.FaultLine, result.ProjectedBridgeReview.ThresholdClass);
        Assert.NotNull(result.ProjectedBridgeReview.PreBondSafeguard);
        Assert.Equal(SliPreBondSafeguardClass.PredatorySharedDomainRisk, result.ProjectedBridgeReview.PreBondSafeguard!.SafeguardClass);
        Assert.Equal(SliPreBondSafeguardDisposition.Refuse, result.ProjectedBridgeReview.PreBondSafeguard.Disposition);
        Assert.True(result.ProjectedBridgeReview.PreBondSafeguard.RequiresEscalation);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
    }

    [Fact]
    public async Task ContinuityInstability_IsHeldBeforeClosure()
    {
        var harness = CreateHarness();
        var profile = CloneProfile(
            LoadExampleProfile(),
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.StructuralValidation],
            continuityInstabilityDetected: true);

        var result = await harness.RunAsync(profile);

        Assert.Equal(SliBridgeOutcomeKind.NeedsSpec, result.ProjectedBridgeReview.OutcomeKind);
        Assert.Equal(SliBridgeThresholdClass.ThresholdBreach, result.ProjectedBridgeReview.ThresholdClass);
        Assert.NotNull(result.ProjectedBridgeReview.PreBondSafeguard);
        Assert.Equal(SliPreBondSafeguardClass.ContinuityInstability, result.ProjectedBridgeReview.PreBondSafeguard!.SafeguardClass);
        Assert.Equal(SliPreBondSafeguardDisposition.Hold, result.ProjectedBridgeReview.PreBondSafeguard.Disposition);
        Assert.Empty(result.MembraneDecisions);
        Assert.Empty(result.ClosureOutcomes);
    }

    [Fact]
    public async Task OperatorFormationProfile_IsProjectedIntoBridgeReview()
    {
        var harness = CreateHarness();
        var profile = CloneProfile(
            LoadExampleProfile(),
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.MaskedSummary],
            operatorFormation: CreateOperatorFormationProfile());

        var result = await harness.RunAsync(profile);

        Assert.NotNull(result.ProjectedBridgeReview.OperatorFormation);
        Assert.True(result.ProjectedBridgeReview.OperatorFormation!.WitnessableProtectiveSubsetOnly);
        Assert.False(result.ProjectedBridgeReview.OperatorFormation.BondRealizationClaimed);
        Assert.Equal(
            SliOperatorFormationBoundaryCrossingMode.InterlacedBondedCrossing,
            result.ProjectedBridgeReview.OperatorFormation.BoundaryCrossingMode);
        Assert.Equal(
            SliOperatorFormationRing.Rootseed,
            result.ProjectedBridgeReview.OperatorFormation.Profile.Ring);
        Assert.Equal(
            SliOperatorFormationMode.Stillness,
            result.ProjectedBridgeReview.OperatorFormation.Profile.ActiveMode);
        Assert.Equal(
            SliOperatorFormationCertificationDecision.Pending,
            result.ProjectedBridgeReview.OperatorFormation.CertificationPosture.Decision);
        Assert.Equal(2, result.ProjectedBridgeReview.OperatorFormation.SigilAssets.Count);
        Assert.Contains(
            result.ProjectedBridgeReview.OperatorFormation.SigilAssets,
            asset => asset.SigilClass == SliOperatorFormationSigilClass.MergedCompletionKey);
        Assert.Equal(
            SliOperatorFormationProgressionState.Blocked,
            result.ProjectedBridgeReview.OperatorFormation.CertificationPosture.Progression.State);
        Assert.False(result.ProjectedBridgeReview.OperatorFormation.CertificationPosture.Progression.PromotionClaimAllowed);
        Assert.Equal(
            "certification_reviewer://first-run-lane",
            result.ProjectedBridgeReview.OperatorFormation.CertificationPosture.Progression.HaltOwner);
    }

    [Fact]
    public async Task OperatorFormationProgression_TargetTransitionReady_WhenGateIsMet()
    {
        var harness = CreateHarness();
        var profile = CloneProfile(
            LoadExampleProfile(),
            bootClass: BootClass.CorporateGoverned,
            requestedExpansionCount: 1,
            requestedRevealModes: [PrimeRevealMode.StructuralValidation],
            operatorFormation: CreateOperatorFormationProfile(
                decision: SliOperatorFormationCertificationDecision.Proceed,
                evidenceGaps: [],
                blockingConditions: [],
                currentAnchoredPosture: SliOperatorFormationBondStatus.VerifiedCandidate,
                nearestAdmissibleNextPosture: SliOperatorFormationBondStatus.PreCertifiedOperator,
                targetPosture: SliOperatorFormationBondStatus.PreCertifiedOperator,
                certificationIssued: true,
                expandedRevealAllowed: true,
                continuityClaimAllowed: true));

        var result = await harness.RunAsync(profile);
        var operatorFormation = result.ProjectedBridgeReview.OperatorFormation;
        Assert.NotNull(operatorFormation);
        var certification = operatorFormation!.CertificationPosture;

        Assert.Equal(SliOperatorFormationProgressionState.TargetTransitionReady, certification.Progression.State);
        Assert.True(certification.Progression.TargetTransitionAllowed);
        Assert.True(certification.Progression.PromotionClaimAllowed);
        Assert.True(certification.CertificationIssued);
        Assert.True(certification.ExpandedRevealAllowed);
        Assert.True(certification.ContinuityClaimAllowed);
        Assert.Equal("operator-formation-target-transition-ready", certification.Progression.ReasonCode);
    }

    private static HybridProtectedIngressHarness CreateHarness(IAgentiFormationObserver? observer = null)
    {
        return new HybridProtectedIngressHarness(
            new DefaultFirstBootGovernancePolicy(),
            new EngramClosureValidator(),
            new CrypticAdmissionMembrane(),
            formationObserver: observer);
    }

    private static HybridProtectedIngressProfile LoadExampleProfile()
    {
        var path = ResolveRepoFile(
            "OAN Mortalis V1.0",
            "docs",
            "runtime",
            "hybrid_protected_ingress_profile.example.json");

        return HybridProtectedIngressProfile.LoadFromJson(path);
    }

    private static HybridProtectedIngressProfile CloneProfile(
        HybridProtectedIngressProfile baseProfile,
        BootClass bootClass,
        int requestedExpansionCount,
        IReadOnlyList<PrimeRevealMode> requestedRevealModes,
        bool? bondedAuthorityConfirmed = null,
        IReadOnlyList<string>? approvedRevealPurposes = null,
        string? humanPrincipalName = null,
        string? corporatePrincipalName = null,
        bool? predatorySharedDomainRiskDetected = null,
        bool? coerciveBondingPostureDetected = null,
        bool? continuityInstabilityDetected = null,
        bool? identityOvercollapseRiskDetected = null,
        HybridProtectedIngressOperatorFormationProfile? operatorFormation = null)
    {
        return new HybridProtectedIngressProfile
        {
            HumanPrincipalName = humanPrincipalName ?? baseProfile.HumanPrincipalName,
            CorporatePrincipalName = corporatePrincipalName ?? baseProfile.CorporatePrincipalName,
            AuthorityRelationship = baseProfile.AuthorityRelationship,
            HumanCredentialId = baseProfile.HumanCredentialId,
            CorporateRegistryId = baseProfile.CorporateRegistryId,
            AuthorityToken = baseProfile.AuthorityToken,
            AddressHandle = baseProfile.AddressHandle,
            RequestedBootClass = bootClass,
            RequestedExpansionCount = requestedExpansionCount,
            RequestedRevealModes = requestedRevealModes.ToArray(),
            BondedAuthorityConfirmed = bondedAuthorityConfirmed ?? baseProfile.BondedAuthorityConfirmed,
            ApprovedRevealPurposes = (approvedRevealPurposes ?? baseProfile.ApprovedRevealPurposes).ToArray(),
            PredatorySharedDomainRiskDetected = predatorySharedDomainRiskDetected ?? baseProfile.PredatorySharedDomainRiskDetected,
            CoerciveBondingPostureDetected = coerciveBondingPostureDetected ?? baseProfile.CoerciveBondingPostureDetected,
            ContinuityInstabilityDetected = continuityInstabilityDetected ?? baseProfile.ContinuityInstabilityDetected,
            IdentityOvercollapseRiskDetected = identityOvercollapseRiskDetected ?? baseProfile.IdentityOvercollapseRiskDetected,
            OperatorFormation = operatorFormation ?? baseProfile.OperatorFormation
        };
    }

    private static HybridProtectedIngressOperatorFormationProfile CreateOperatorFormationProfile(
        SliOperatorFormationCertificationDecision decision = SliOperatorFormationCertificationDecision.Pending,
        IReadOnlyList<string>? evidenceGaps = null,
        IReadOnlyList<string>? blockingConditions = null,
        SliOperatorFormationBondStatus currentAnchoredPosture = SliOperatorFormationBondStatus.TrainingOperator,
        SliOperatorFormationBondStatus nearestAdmissibleNextPosture = SliOperatorFormationBondStatus.VerifiedCandidate,
        SliOperatorFormationBondStatus targetPosture = SliOperatorFormationBondStatus.PreCertifiedOperator,
        bool certificationIssued = false,
        bool expandedRevealAllowed = false,
        bool continuityClaimAllowed = false)
    {
        return new HybridProtectedIngressOperatorFormationProfile
        {
            ProfileId = "gs_profile_obsidian_guarded",
            ChapterLocalSurface = "research/publications/gnomeronacorde-v0.1/source/1_OBSIDIAN_WALL/1a_Casting_Shadow.tex",
            PairedTrainingSurface = "research/publications/gnome-speak-nlp-v1.0/source/sections/operator_role.tex",
            CrossingTaskKind = "literacy_alignment",
            HaltOwner = "bonded_training_reviewer",
            BoundaryCrossingMode = SliOperatorFormationBoundaryCrossingMode.InterlacedBondedCrossing,
            Ring = SliOperatorFormationRing.Rootseed,
            ActiveMode = SliOperatorFormationMode.Stillness,
            StillnessInterludeUsed = false,
            RedHatIndexRequired = true,
            BondStatus = SliOperatorFormationBondStatus.TrainingOperator,
            EchoVeilCheckRequired = true,
            ActiveConflictClass = SliOperatorFormationConflictClass.None,
            GjpNeeded = false,
            GjpVerdict = SliOperatorFormationGjpVerdict.NotApplicable,
            MotherLightAnchored = true,
            FatherEchoAnchored = true,
            ShellRootAnchored = true,
            SeedBoundAnchored = true,
            U230ShadowScript = SliOperatorFormationConcealmentLayerState.Observed,
            U300ElvenScript = SliOperatorFormationConcealmentLayerState.Observed,
            ExpectedEvidenceArtifact = "interlace_crossing_proof",
            AdmissibleOutput = "bounded_training_profile",
            ProhibitedOutputs = ["unrestricted_archetype_claim", "unauthorized_gjp_invocation"],
            Certification = new HybridProtectedIngressOperatorFormationCertification
            {
                Decision = decision,
                CurrentAnchoredPosture = currentAnchoredPosture,
                TargetPosture = targetPosture,
                NearestAdmissibleNextPosture = nearestAdmissibleNextPosture,
                ReviewOwner = "certification_reviewer://first-run-lane",
                RequiredBondedStandard = "first_run_bonding://precertification/minimum-admissible-evidence",
                EvidenceGaps = (evidenceGaps ?? ["trial_receipt_set", "label_classification_receipt"]).ToArray(),
                BlockingConditions = (blockingConditions ?? ["incomplete_trial_evidence"]).ToArray(),
                NextActions = ["Complete the current Gnome Speak trial block.", "Attach the chapter-local label-classification receipt."],
                HaltOwner = "certification_reviewer://first-run-lane",
                HaltCondition = "Halt if evidence lineage breaks or if protected meaning is flattened during remediation.",
                ReentryRule = "Reenter certification review only after the missing trial receipt set and chapter-local classification outputs are attached.",
                ProhibitedClaims = ["pre-certification issued", "bond actualized"],
                LinkedVerificationRecord = "first_bonding://verification/pending",
                LinkedPreCertificationRecord = null,
                GateArtifact = "verification record",
                CertificationIssued = certificationIssued,
                ExpandedRevealAllowed = expandedRevealAllowed,
                ContinuityClaimAllowed = continuityClaimAllowed
            },
            SigilAssets =
            [
                new HybridProtectedIngressOperatorFormationSigilAsset
                {
                    AssetId = "obsidian_1",
                    AssetLabel = "OBSIDIAN1",
                    SigilClass = SliOperatorFormationSigilClass.PhasePartition,
                    PhaseNumber = 1,
                    VisibilityClass = "operator_guarded",
                    BuildRenderPolicy = "staged_render_allowed",
                    ReductionPosture = "reference_and_render_allowed",
                    MergedFromAssets = [],
                    WitnessOfAsset = null
                },
                new HybridProtectedIngressOperatorFormationSigilAsset
                {
                    AssetId = "obsidian_zed",
                    AssetLabel = "OBSIDIANzed",
                    SigilClass = SliOperatorFormationSigilClass.MergedCompletionKey,
                    PhaseNumber = null,
                    VisibilityClass = "continuity_sealed",
                    BuildRenderPolicy = "staged_render_allowed",
                    ReductionPosture = "descriptive_reduction_allowed",
                    MergedFromAssets = ["obsidian_1", "obsidian_2", "obsidian_3", "obsidian_4", "obsidian_5"],
                    WitnessOfAsset = null
                }
            ]
        };
    }

    private static string ResolveRepoFile(params string[] parts)
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                var expected = Path.Combine(new[] { current.FullName }.Concat(parts).ToArray());
                if (File.Exists(expected))
                {
                    return expected;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException($"Unable to locate {Path.Combine(parts)} from the current test context.");
    }
}
