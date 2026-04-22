using System.Text.Json;
using GEL.Contracts.Sanctuary;

namespace Oan.Audit.Tests;

public sealed class SanctuaryGelConstitutionContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Gel_Discernment_Layer_Atlas_Is_Exact()
    {
        Assert.Equal(
            [
                SanctuaryGelDiscernmentLayer.Root,
                SanctuaryGelDiscernmentLayer.Dictionary
            ],
            SanctuaryGelDiscernmentLayerAtlas.All.Select(static item => item.Layer).ToArray());

        var root = SanctuaryGelDiscernmentLayerAtlas.Get(SanctuaryGelDiscernmentLayer.Root);
        Assert.Equal("irreducible meaning unit, not yet definition or explanation", root.Purpose);
        Assert.Equal(
            [
                "minimal-semantic-atom",
                "non-definitional",
                "non-explanatory"
            ],
            root.AdmissionExpectations);
        Assert.Equal(
            [
                "dictionary-inflation",
                "contextual-expansion",
                "procedural-leap",
                "narrative-overwrite"
            ],
            root.ForbiddenDrift);
        Assert.Empty(root.GroundingLayers);
        Assert.Equal(
            [
                SanctuaryGelDiscernmentLayer.Root,
                SanctuaryGelDiscernmentLayer.Dictionary
            ],
            root.ReachableLayers);
        Assert.Equal("hold without expanding; allow later stabilization into definition", root.SeedUseExpectation);

        var dictionary = SanctuaryGelDiscernmentLayerAtlas.Get(SanctuaryGelDiscernmentLayer.Dictionary);
        Assert.Equal("stabilized bounded definition grounded in Root", dictionary.Purpose);
        Assert.Equal(
            [
                "term-boundary",
                "disambiguated-definition",
                "root-grounded"
            ],
            dictionary.AdmissionExpectations);
        Assert.Equal(
            [
                "root-redefinition",
                "encyclopedic-drift",
                "procedural-answer",
                "narrative-substitution"
            ],
            dictionary.ForbiddenDrift);
        Assert.Equal(
            [
                SanctuaryGelDiscernmentLayer.Root
            ],
            dictionary.GroundingLayers);
        Assert.Equal(
            [
                SanctuaryGelDiscernmentLayer.Dictionary,
                SanctuaryGelDiscernmentLayer.Root
            ],
            dictionary.ReachableLayers);
        Assert.Equal("stabilize meaning before context or procedure", dictionary.SeedUseExpectation);

        Assert.Equal(2, SanctuaryGelDiscernmentLayerAtlas.PlaceholderBindings.Count);

        var rootBinding = Assert.Single(
            SanctuaryGelDiscernmentLayerAtlas.PlaceholderBindings.Where(static item => item.SourceLayer == SanctuaryGelDiscernmentLayer.Root));
        Assert.Equal("root-unit", rootBinding.EnglishSurfaceKind);
        Assert.Equal("root-anchor", rootBinding.RootAtlasAnchorKind);
        Assert.Equal("sli.root.carrier", rootBinding.SliCarrierPlaceholder);
        Assert.Equal("root-engram-candidate", rootBinding.AdmissibleEngramRole);
        Assert.Equal(
            [
                "layer-kind",
                "root-identity"
            ],
            rootBinding.PreservedInvariants);
        Assert.Equal(
            [
                "definition-substitution",
                "contextual-expansion",
                "procedural-promotion"
            ],
            rootBinding.ForbiddenTransfers);

        var definitionBinding = Assert.Single(
            SanctuaryGelDiscernmentLayerAtlas.PlaceholderBindings.Where(static item => item.SourceLayer == SanctuaryGelDiscernmentLayer.Dictionary));
        Assert.Equal("bounded-definition", definitionBinding.EnglishSurfaceKind);
        Assert.Equal("definition-anchor", definitionBinding.RootAtlasAnchorKind);
        Assert.Equal("sli.definition.carrier", definitionBinding.SliCarrierPlaceholder);
        Assert.Equal("definitional-engram-candidate", definitionBinding.AdmissibleEngramRole);
        Assert.Equal(
            [
                "layer-kind",
                "definition-boundary",
                "root-grounding"
            ],
            definitionBinding.PreservedInvariants);
        Assert.Equal(
            [
                "root-redefinition",
                "encyclopedic-drift",
                "procedural-leap"
            ],
            definitionBinding.ForbiddenTransfers);
    }

    [Fact]
    public void PreLisp_And_Minimal_Lisp_Being_Surface_Are_Exact()
    {
        var preLisp = SanctuaryPreLispCarrierAtlas.PreLisp0;
        Assert.Equal("pl0.english-discernment-nucleus", preLisp.Handle);
        Assert.Equal(
            [
                "gel-en-body",
                "discernment-index",
                "root-atlas-anchor-map",
                "sli-translation-law",
                "engram-candidacy-map",
                "prime-posture-space"
            ],
            preLisp.Components);
        Assert.Equal(
            [
                "layer-kind-preserved",
                "anchor-lineage-preserved",
                "definition-boundary-preserved",
                "non-acting-prime-posture",
                "recovery-toward-minimality"
            ],
            preLisp.GoverningInvariants);
        Assert.Equal("hold layered meaning without immediate proceduralization", preLisp.SeedPostureExpectation);

        Assert.Equal(
            [
                SanctuaryIuttLispCarrierFormKind.Root,
                SanctuaryIuttLispCarrierFormKind.Definition,
                SanctuaryIuttLispCarrierFormKind.Posture,
                SanctuaryIuttLispCarrierFormKind.Translate,
                SanctuaryIuttLispCarrierFormKind.EngramCandidate
            ],
            SanctuaryPreLispCarrierAtlas.CarrierForms.Select(static item => item.Kind).ToArray());

        var root = SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.Root);
        Assert.Equal(SanctuaryGelDiscernmentLayer.Root, root.GoverningLayer);
        Assert.Equal(["id", "anchor", "token", "status"], root.RequiredFields);
        Assert.Equal(["layer-kind", "root-identity", "anchor-lineage"], root.PreservedInvariants);
        Assert.Equal(["definition-substitution", "contextual-expansion", "procedural-promotion"], root.ForbiddenCollapses);
        Assert.Equal("placeholder-only", root.OperationalStatus);

        var definition = SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.Definition);
        Assert.Equal(SanctuaryGelDiscernmentLayer.Dictionary, definition.GoverningLayer);
        Assert.Equal(["id", "roots", "boundary", "status"], definition.RequiredFields);
        Assert.Equal(["layer-kind", "definition-boundary", "root-grounding"], definition.PreservedInvariants);
        Assert.Equal(["root-redefinition", "encyclopedic-drift", "procedural-leap"], definition.ForbiddenCollapses);
        Assert.Equal("placeholder-only", definition.OperationalStatus);

        var posture = SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.Posture);
        Assert.Null(posture.GoverningLayer);
        Assert.Equal(["state", "mode", "forced-act", "capable", "stable"], posture.RequiredFields);
        Assert.Equal(["prime-posture", "non-forced-action", "recovery-capability"], posture.PreservedInvariants);
        Assert.Equal(["role-derivation", "forced-output", "procedural-compulsion"], posture.ForbiddenCollapses);

        var translate = SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.Translate);
        Assert.Null(translate.GoverningLayer);
        Assert.Equal(["source-layer", "target-carrier", "preserved-invariants", "forbidden-transfers"], translate.RequiredFields);
        Assert.Equal(["layer-kind", "anchor-lineage"], translate.PreservedInvariants);
        Assert.Equal(["layer-flattening", "identity-loss", "unauthorized-promotion"], translate.ForbiddenCollapses);

        var engramCandidate = SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.EngramCandidate);
        Assert.Null(engramCandidate.GoverningLayer);
        Assert.Equal(["source-kind", "lineage", "preservation-check", "eligibility"], engramCandidate.RequiredFields);
        Assert.Equal(["anchor-lineage", "preservation-check"], engramCandidate.PreservedInvariants);
        Assert.Equal(["identity-write", "runtime-promotion", "unchecked-compression"], engramCandidate.ForbiddenCollapses);

        var beingSurface = SanctuaryPreLispCarrierAtlas.LispBeing0;
        Assert.Equal("lb0.minimal-iutt-lisp-being-surface", beingSurface.Handle);
        Assert.Equal(
            [
                SanctuaryIuttLispCarrierFormKind.Root,
                SanctuaryIuttLispCarrierFormKind.Definition,
                SanctuaryIuttLispCarrierFormKind.Posture,
                SanctuaryIuttLispCarrierFormKind.Translate,
                SanctuaryIuttLispCarrierFormKind.EngramCandidate
            ],
            beingSurface.CarrierForms);
        Assert.Equal(
            [
                "root->definition",
                "x->translate(x)",
                "translate(x)->engram-candidate(x)",
                "posture-guards-all-upward-moves"
            ],
            beingSurface.LegalMorphisms);
        Assert.Equal(
            [
                "prime-posture-required",
                "translation-must-preserve-layer-kind",
                "translation-must-preserve-anchor-lineage",
                "engram-candidate-must-pass-preservation-check"
            ],
            beingSurface.GuardConditions);
        Assert.Equal(["relation", "procedure"], beingSurface.WithheldForms);
    }

    [Fact]
    public void Prime_Reduction_Decomposition_Anchor_And_Trunk_Atlases_Are_Exact()
    {
        var prime = SanctuaryPrimeRootReductionAtlas.PrimeRootCarrier;
        Assert.Equal("prime.root-carrier.utf8", prime.Handle);
        Assert.Equal("utf8-token-space", prime.CarrierSpace);
        Assert.Equal(
            [
                SanctuaryPrimeConstraintKind.IrreducibleMeaning,
                SanctuaryPrimeConstraintKind.RootAtlasAnchorable,
                SanctuaryPrimeConstraintKind.NonInflatedRootLayer,
                SanctuaryPrimeConstraintKind.TranslationStable
            ],
            prime.Constraints);
        Assert.Equal(
            [
                "cannot-decompose-without-meaning-loss",
                "must-bind-rootatlas-anchor",
                "must-not-encode-dictionary-encyclopedic-procedural-content",
                "must-preserve-anchor-and-root-kind-under-sli-translation"
            ],
            prime.ConstraintRules);
        Assert.Equal(
            [
                "definition-boundary->dictionary",
                "contextual-expansion->encyclopedic",
                "actionability->procedural",
                "unresolved->inadmissible"
            ],
            prime.FailureRoutes);
        Assert.Equal("x-in-root-iff-prime", prime.RootAdmissionRule);
        Assert.Equal("tau-preserves-root-kind-and-anchor-lineage", prime.TranslationRule);

        var pipeline = SanctuaryPrimeRootReductionAtlas.ReductionPipeline;
        Assert.Equal("symbolic-selection.reduction-pipeline.v0", pipeline.Handle);
        Assert.Equal(
            [
                SanctuarySymbolicReductionStage.Utf8Intake,
                SanctuarySymbolicReductionStage.Normalization,
                SanctuarySymbolicReductionStage.Segmentation,
                SanctuarySymbolicReductionStage.Compression,
                SanctuarySymbolicReductionStage.Decomposition,
                SanctuarySymbolicReductionStage.PrimeSelection,
                SanctuarySymbolicReductionStage.RootAnchoring,
                SanctuarySymbolicReductionStage.LayerPlacement,
                SanctuarySymbolicReductionStage.TranslativeBinding
            ],
            pipeline.Stages);
        Assert.Equal(
            [
                "raw-utf8-field",
                "normalize-carrier-noise",
                "segment-candidate-units",
                "strip-definitional-contextual-procedural-load",
                "split-mixed-candidates-into-more-discernible-units",
                "test-prime-admissibility",
                "bind-rootatlas-anchor",
                "place-into-root-or-residual-layer",
                "translate-only-after-layering-and-anchoring"
            ],
            pipeline.StageIntents);
        Assert.Equal("lexical-tokenization-and-atlas-match-only-before-prime-validation", pipeline.CurrentImplementationBoundary);
        Assert.Equal("placeholder-contract-only", pipeline.CurrentRuntimeStatus);

        Assert.Equal(
            [
                SanctuaryKappaTestKind.DefinitionalInflation,
                SanctuaryKappaTestKind.ContextualResidue,
                SanctuaryKappaTestKind.ProceduralPressure
            ],
            SanctuaryPrimeRootReductionAtlas.KappaTests.Select(static item => item.Kind).ToArray());

        var kappaD = SanctuaryPrimeRootReductionAtlas.GetKappaTest(SanctuaryKappaTestKind.DefinitionalInflation);
        Assert.Equal(
            [
                SanctuaryKappaEvidenceKind.BoundaryExpansion,
                SanctuaryKappaEvidenceKind.EquivalenceClaim,
                SanctuaryKappaEvidenceKind.DescriptiveRestatement
            ],
            kappaD.EvidenceKinds);
        Assert.Equal("if-present-not-root", kappaD.RootRefusalRule);
        Assert.Equal(SanctuaryResidualRoute.Dictionary, kappaD.DefaultResidualRoute);
        Assert.Equal("placeholder-only", kappaD.OperationalStatus);

        var kappaE = SanctuaryPrimeRootReductionAtlas.GetKappaTest(SanctuaryKappaTestKind.ContextualResidue);
        Assert.Equal(
            [
                SanctuaryKappaEvidenceKind.ExampleDependence,
                SanctuaryKappaEvidenceKind.HistoricalBinding,
                SanctuaryKappaEvidenceKind.NarrativeSituation,
                SanctuaryKappaEvidenceKind.ExternalRelationLoad
            ],
            kappaE.EvidenceKinds);
        Assert.Equal(SanctuaryResidualRoute.Encyclopedic, kappaE.DefaultResidualRoute);

        var kappaP = SanctuaryPrimeRootReductionAtlas.GetKappaTest(SanctuaryKappaTestKind.ProceduralPressure);
        Assert.Equal(
            [
                SanctuaryKappaEvidenceKind.Instructionality,
                SanctuaryKappaEvidenceKind.SequenceImplied,
                SanctuaryKappaEvidenceKind.GoalOrientation,
                SanctuaryKappaEvidenceKind.ExecutionPressure
            ],
            kappaP.EvidenceKinds);
        Assert.Equal(SanctuaryResidualRoute.Procedural, kappaP.DefaultResidualRoute);

        var compression = SanctuaryPrimeRootReductionAtlas.KappaCompression;
        Assert.Equal("kappa.compression.v0", compression.Handle);
        Assert.Equal(
            [
                SanctuaryKappaTestKind.DefinitionalInflation,
                SanctuaryKappaTestKind.ContextualResidue,
                SanctuaryKappaTestKind.ProceduralPressure
            ],
            compression.Tests);
        Assert.Equal("0,0,0", compression.RootCleanSignature);
        Assert.Equal("root-only-when-kappa-clean-and-anchorable", compression.RootAdmissionRule);
        Assert.Equal("mixed-vectors-decompose-or-hold-unresolved", compression.MixedCandidateRule);
        Assert.Equal("single-dominant-failure-routes-upward-mixed-failure-routes-unresolved", compression.ResidualRoutingRule);

        Assert.Equal(
            [
                SanctuaryDecompositionMode.Definitional,
                SanctuaryDecompositionMode.Contextual,
                SanctuaryDecompositionMode.Procedural,
                SanctuaryDecompositionMode.Mixed
            ],
            SanctuaryPrimeRootReductionAtlas.DecompositionModes.Select(static item => item.Mode).ToArray());

        var definitionalMode = SanctuaryPrimeRootReductionAtlas.GetDecompositionMode(SanctuaryDecompositionMode.Definitional);
        Assert.Equal([SanctuaryKappaTestKind.DefinitionalInflation], definitionalMode.TriggerTests);
        Assert.Equal(["carrier", "definition"], definitionalMode.ChildRoles);
        Assert.Equal("separate carrier term from explanatory shell", definitionalMode.Intent);
        Assert.Equal(SanctuaryResidualRoute.Dictionary, definitionalMode.DefaultResidualRoute);

        var contextualMode = SanctuaryPrimeRootReductionAtlas.GetDecompositionMode(SanctuaryDecompositionMode.Contextual);
        Assert.Equal([SanctuaryKappaTestKind.ContextualResidue], contextualMode.TriggerTests);
        Assert.Equal(["core", "context"], contextualMode.ChildRoles);
        Assert.Equal("separate core semantic unit from narrative or relational environment", contextualMode.Intent);
        Assert.Equal(SanctuaryResidualRoute.Encyclopedic, contextualMode.DefaultResidualRoute);

        var proceduralMode = SanctuaryPrimeRootReductionAtlas.GetDecompositionMode(SanctuaryDecompositionMode.Procedural);
        Assert.Equal([SanctuaryKappaTestKind.ProceduralPressure], proceduralMode.TriggerTests);
        Assert.Equal(["operation-anchor", "procedure"], proceduralMode.ChildRoles);
        Assert.Equal("separate operation-bearing term from instruction or method shell", proceduralMode.Intent);
        Assert.Equal(SanctuaryResidualRoute.Procedural, proceduralMode.DefaultResidualRoute);

        var mixedMode = SanctuaryPrimeRootReductionAtlas.GetDecompositionMode(SanctuaryDecompositionMode.Mixed);
        Assert.Equal(
            [
                SanctuaryKappaTestKind.DefinitionalInflation,
                SanctuaryKappaTestKind.ContextualResidue,
                SanctuaryKappaTestKind.ProceduralPressure
            ],
            mixedMode.TriggerTests);
        Assert.Equal(["subcandidate-set"], mixedMode.ChildRoles);
        Assert.Equal("recursively split entangled candidates until children are more layer-pure", mixedMode.Intent);
        Assert.Equal(SanctuaryResidualRoute.Unresolved, mixedMode.DefaultResidualRoute);

        var decomposition = SanctuaryPrimeRootReductionAtlas.Decomposition;
        Assert.Equal("mixed-candidate.decomposition.v0", decomposition.Handle);
        Assert.Equal(["1,1,0", "1,0,1", "0,1,1", "1,1,1"], decomposition.TriggerVectors);
        Assert.Equal("must-preserve-anchorable-semantic-mass", decomposition.ConservationRule);
        Assert.Equal("children-must-be-more-discernible-than-parent", decomposition.DiscernmentGainRule);
        Assert.Equal("stop-when-each-child-is-prime-or-cleanly-routable-or-inadmissible", decomposition.StopRule);
        Assert.Equal(["over-fragmentation", "hidden-retention"], decomposition.FailureModes);
        Assert.Equal("placeholder-contract-only", decomposition.CurrentRuntimeStatus);

        var anchorEmergence = SanctuaryPrimeRootReductionAtlas.AnchorEmergence;
        Assert.Equal("anchor-emergence.v0", anchorEmergence.Handle);
        Assert.Equal(
            [
                SanctuaryAnchorEmergenceCriterionKind.PrimeValid,
                SanctuaryAnchorEmergenceCriterionKind.SemanticStable,
                SanctuaryAnchorEmergenceCriterionKind.CrossOccurrenceConvergent,
                SanctuaryAnchorEmergenceCriterionKind.NonSubstitutable
            ],
            anchorEmergence.Criteria);
        Assert.Equal(
            [
                "must-already-satisfy-prime-root-carrier",
                "must-remain-semantically-stable-across-contexts",
                "must-recur-across-independent-reductions-within-one-governed-seed",
                "must-resist-lossless-substitution"
            ],
            anchorEmergence.CriterionRules);
        Assert.Equal("align-by-convergent-behavior-not-string-identity", anchorEmergence.AlignmentRule);
        Assert.Equal(
            [
                "bind-existing-anchor-when-aligned",
                "create-new-anchor-when-emergent-and-unaligned",
                "hold-unresolved-when-ambiguous-or-incomplete"
            ],
            anchorEmergence.ResolutionBranches);
        Assert.Equal("root-becomes-real-only-when-anchor-emerges", anchorEmergence.RootRealizationRule);
        Assert.Equal("placeholder-contract-only", anchorEmergence.OperationalStatus);

        var trunkIdentity = SanctuaryPrimeRootReductionAtlas.TrunkIdentity;
        Assert.Equal("trunk-identity.v0", trunkIdentity.Handle);
        Assert.Equal("anchor-stable semantic identity discovered through convergence", trunkIdentity.Meaning);
        Assert.Equal(
            [
                "trunk-stability",
                "translation-invariant-identity",
                "branches-must-not-redefine-trunk"
            ],
            trunkIdentity.Invariants);
        Assert.Equal(
            [
                "future-definition-branching",
                "future-context-branching",
                "future-procedural-branching"
            ],
            trunkIdentity.AllowedFutureGrowth);
        Assert.Equal(
            [
                "trunk-redefinition",
                "anchor-collapse",
                "branch-overwrite-of-trunk"
            ],
            trunkIdentity.ForbiddenMutations);
        Assert.Equal("placeholder-contract-only", trunkIdentity.OperationalStatus);
    }

    [Fact]
    public void Gel_Axioms_Derived_Laws_Actions_And_Composition_Are_Exact()
    {
        Assert.Equal(
            [
                SanctuaryGelAxiomKind.LayerIntegrity,
                SanctuaryGelAxiomKind.RootIdentityInvariance,
                SanctuaryGelAxiomKind.DownwardGrounding,
                SanctuaryGelAxiomKind.DiscernmentAdmissibility,
                SanctuaryGelAxiomKind.NonInflation,
                SanctuaryGelAxiomKind.TranslationPreservation,
                SanctuaryGelAxiomKind.PostureStability
            ],
            SanctuaryGelAxiomAtlas.All.Select(static item => item.Axiom).ToArray());

        AssertAxiom(
            SanctuaryGelAxiomKind.LayerIntegrity,
            "a0.layer-integrity",
            "a-thing-must-be-treated-as-the-kind-of-knowing-it-is",
            ["layer-discernment", "non-masquerading"],
            ["cross-layer-drift", "false-classification"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.RootIdentityInvariance,
            "a0.root-identity-invariance",
            "anchored-root-identity-must-not-change-under-allowed-transformations",
            ["trunk-stability", "anchor-continuity"],
            ["root-mutation", "anchor-collapse"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.DownwardGrounding,
            "a0.downward-grounding",
            "every-higher-layer-object-must-resolve-downward-to-root-support",
            ["dependency-order", "non-floating-structure"],
            ["floating-definition", "floating-context", "floating-procedure"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.DiscernmentAdmissibility,
            "a0.discernment-admissibility",
            "only-what-passes-discernment-may-enter-or-remain",
            ["lawful-admission", "lawful-retention"],
            ["unlawful-persistence", "trust-collapse"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.NonInflation,
            "a0.non-inflation",
            "no-object-may-carry-hidden-content-from-a-higher-layer",
            ["layer-cleanliness", "root-clean-signature"],
            ["hidden-definition", "hidden-context", "hidden-procedure"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.TranslationPreservation,
            "a0.translation-preservation",
            "translation-must-preserve-layer-anchor-and-dependency-structure",
            ["sli-integrity", "cross-theater-continuity"],
            ["translation-collapse", "identity-loss"]);
        AssertAxiom(
            SanctuaryGelAxiomKind.PostureStability,
            "a0.posture-stability",
            "the-acting-system-must-hold-without-premature-action-and-recover-from-drift",
            ["prime-posture", "recoverability"],
            ["forced-action", "procedural-collapse"]);

        var residency = SanctuaryGelDerivedLawAtlas.ResidencyDiscernment;
        Assert.Equal("delta-g.residency-discernment.v0", residency.Handle);
        Assert.Equal("does-this-retained-object-still-belong-where-it-is", residency.GoverningQuestion);
        Assert.Equal(SanctuaryGelResidencyDisposition.Remain, residency.LawfulOutcome);
        Assert.Equal(
            [
                SanctuaryGelResidencyDisposition.Decompose,
                SanctuaryGelResidencyDisposition.Reroute,
                SanctuaryGelResidencyDisposition.Refuse
            ],
            residency.DriftOutcomes);
        Assert.Equal("retained-objects-must-remain-lawful-to-their-layer", residency.ProtectedInvariant);
        Assert.Equal("placeholder-contract-only", residency.OperationalStatus);

        Assert.Equal(
            [
                SanctuaryGelDerivedLawKind.BranchGrowth,
                SanctuaryGelDerivedLawKind.BranchPurity,
                SanctuaryGelDerivedLawKind.Pruning,
                SanctuaryGelDerivedLawKind.GroveRelation,
                SanctuaryGelDerivedLawKind.AnchorMergeRefusal
            ],
            SanctuaryGelDerivedLawAtlas.ContractBackedLaws.Select(static item => item.Law).ToArray());

        AssertDerivedLaw(
            SanctuaryGelDerivedLawKind.BranchGrowth,
            "l1.branch-growth",
            "branch-may-grow-from-trunk-only-when-anchored-to-it-and-without-redefining-it",
            ["a0.layer-integrity", "a0.root-identity-invariance", "a0.downward-grounding"],
            "dictionary-contextual-and-procedural-growth-must-preserve-trunk-and-layer",
            ["refuse-growth", "hold-provisional", "return-for-reduction"]);
        AssertDerivedLaw(
            SanctuaryGelDerivedLawKind.BranchPurity,
            "l2.branch-purity",
            "branch-must-remain-lawful-to-its-own-layer-under-residency-discernment",
            ["a0.layer-integrity", "a0.non-inflation", "a0.discernment-admissibility"],
            "retained-branch-remains-only-while-delta-g-matches-declared-layer",
            ["decompose", "reroute", "refuse"]);
        AssertDerivedLaw(
            SanctuaryGelDerivedLawKind.Pruning,
            "l3.pruning",
            "unlawful-branch-must-be-decomposed-rerouted-or-refused-not-retained-as-corruption",
            ["a0.discernment-admissibility", "a0.non-inflation", "a0.posture-stability"],
            "corrupted-retention-may-not-remain-by-convenience",
            ["decompose", "reroute", "refuse"]);
        AssertDerivedLaw(
            SanctuaryGelDerivedLawKind.GroveRelation,
            "l4.grove-relation",
            "distinct-trunks-may-relate-without-collapsing-identity",
            ["a0.layer-integrity", "a0.root-identity-invariance", "a0.downward-grounding"],
            "relation-must-preserve-distinct-anchor-membership",
            ["refuse-relation", "hold-provisional"]);
        AssertDerivedLaw(
            SanctuaryGelDerivedLawKind.AnchorMergeRefusal,
            "l5.anchor-merge-refusal",
            "distinct-anchors-do-not-merge-by-overlap-alone",
            ["a0.root-identity-invariance", "a0.translation-preservation", "a0.discernment-admissibility"],
            "merge-disallowed-without-strong-equivalence-proof",
            ["refuse-merge", "hold-provisional"]);

        Assert.Equal(
            [
                SanctuaryGelActionKind.Attend,
                SanctuaryGelActionKind.Reduce,
                SanctuaryGelActionKind.Decompose,
                SanctuaryGelActionKind.Discriminate,
                SanctuaryGelActionKind.Anchor,
                SanctuaryGelActionKind.Relate,
                SanctuaryGelActionKind.Retain,
                SanctuaryGelActionKind.Refuse,
                SanctuaryGelActionKind.Recover
            ],
            SanctuaryGelActionAtlas.Actions.Select(static item => item.Action).ToArray());

        AssertAction(
            SanctuaryGelActionKind.Attend,
            "act.attend.v0",
            "isolate-candidate-without-altering-it",
            ["a0.layer-integrity", "a0.posture-stability"],
            ["candidate-isolated", "source-unaltered"],
            ["hold-provisional", "recover"]);
        AssertAction(
            SanctuaryGelActionKind.Reduce,
            "act.reduce.v0",
            "strip-definitional-contextual-and-procedural-inflation",
            ["a0.non-inflation", "a0.discernment-admissibility"],
            ["inflation-load-reduced", "candidate-ready-for-discrimination"],
            ["decompose", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Decompose,
            "act.decompose.v0",
            "split-mixed-candidate-into-more-discernible-parts",
            ["a0.discernment-admissibility", "l3.pruning"],
            ["children-more-discernible-than-parent", "mixed-load-separated"],
            ["hold-provisional", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Discriminate,
            "act.discriminate.v0",
            "determine-what-kind-of-knowing-this-is",
            ["a0.layer-integrity", "a0.discernment-admissibility"],
            ["unique-layer-determined", "routing-made-lawful"],
            ["hold-provisional", "decompose", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Anchor,
            "act.anchor.v0",
            "bind-root-candidate-to-existing-or-new-anchor",
            ["a0.root-identity-invariance", "prime.root-carrier.utf8", "anchor-emergence.v0"],
            ["anchor-bound-or-created", "root-realized"],
            ["hold-provisional", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Relate,
            "act.relate.v0",
            "form-lawful-relation-without-identity-collapse",
            ["l4.grove-relation", "l5.anchor-merge-refusal"],
            ["distinct-anchors-preserved", "relation-admitted"],
            ["hold-provisional", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Retain,
            "act.retain.v0",
            "admit-lawful-object-into-gel",
            ["a0.discernment-admissibility", "a0.downward-grounding"],
            ["object-admitted", "gel-lineage-preserved"],
            ["hold-provisional", "refuse"]);
        AssertAction(
            SanctuaryGelActionKind.Refuse,
            "act.refuse.v0",
            "deny-admission-or-continuation-when-law-fails",
            ["a0.discernment-admissibility", "l3.pruning"],
            ["unlawful-object-not-retained", "trust-surface-protected"],
            ["recover"]);
        AssertAction(
            SanctuaryGelActionKind.Recover,
            "act.recover.v0",
            "return-acting-system-to-prime-posture-after-drift",
            ["a0.posture-stability"],
            ["prime-posture-restored", "forced-action-cleared"],
            ["hold-provisional", "refuse"]);

        Assert.Equal(
            [
                SanctuaryGelCompositionOperatorKind.Sequential,
                SanctuaryGelCompositionOperatorKind.Guarded,
                SanctuaryGelCompositionOperatorKind.Choice,
                SanctuaryGelCompositionOperatorKind.Iteration,
                SanctuaryGelCompositionOperatorKind.RecoveryWrapper,
                SanctuaryGelCompositionOperatorKind.RefusalTerminal
            ],
            SanctuaryGelActionAtlas.CompositionOperators.Select(static item => item.Operator).ToArray());

        AssertComposition(
            SanctuaryGelCompositionOperatorKind.Sequential,
            "comp.sequential.v0",
            "perform-second-action-only-after-first-action-succeeds",
            ["success-required"],
            "halts-on-first-failure");
        AssertComposition(
            SanctuaryGelCompositionOperatorKind.Guarded,
            "comp.guarded.v0",
            "perform-next-action-only-when-guard-holds",
            ["layer-clean", "prime-valid", "posture-stable"],
            "halts-when-guard-fails");
        AssertComposition(
            SanctuaryGelCompositionOperatorKind.Choice,
            "comp.choice.v0",
            "select-exactly-one-lawful-branch",
            ["discrimination-result"],
            "terminates-when-one-branch-is-selected");
        AssertComposition(
            SanctuaryGelCompositionOperatorKind.Iteration,
            "comp.iteration.v0",
            "repeat-until-fixed-point-or-routable-state",
            ["change-detected", "fixed-point-not-yet-reached"],
            "stops-at-fixed-point-or-terminal-routing");
        AssertComposition(
            SanctuaryGelCompositionOperatorKind.RecoveryWrapper,
            "comp.recovery-wrapper.v0",
            "wrap-flow-so-posture-returns-to-prime",
            ["drift-detected"],
            "returns-when-prime-posture-restored");
        AssertComposition(
            SanctuaryGelCompositionOperatorKind.RefusalTerminal,
            "comp.refusal-terminal.v0",
            "terminate-flow-with-no-further-composition",
            ["lawful-refusal"],
            "no-further-actions-allowed");
    }

    [Fact]
    public void Gel_Constitution_Definitions_Roundtrip_Cleanly()
    {
        AssertRoundTrip(SanctuaryGelDiscernmentLayerAtlas.Get(SanctuaryGelDiscernmentLayer.Root));
        AssertRoundTrip(SanctuaryGelDiscernmentLayerAtlas.PlaceholderBindings[0]);
        AssertRoundTrip(SanctuaryPreLispCarrierAtlas.PreLisp0);
        AssertRoundTrip(SanctuaryPreLispCarrierAtlas.Get(SanctuaryIuttLispCarrierFormKind.Root));
        AssertRoundTrip(SanctuaryPreLispCarrierAtlas.LispBeing0);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.PrimeRootCarrier);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.ReductionPipeline);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.GetKappaTest(SanctuaryKappaTestKind.DefinitionalInflation));
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.KappaCompression);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.GetDecompositionMode(SanctuaryDecompositionMode.Mixed));
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.Decomposition);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.AnchorEmergence);
        AssertRoundTrip(SanctuaryPrimeRootReductionAtlas.TrunkIdentity);
        AssertRoundTrip(SanctuaryGelAxiomAtlas.Get(SanctuaryGelAxiomKind.LayerIntegrity));
        AssertRoundTrip(SanctuaryGelDerivedLawAtlas.ResidencyDiscernment);
        AssertRoundTrip(SanctuaryGelDerivedLawAtlas.Get(SanctuaryGelDerivedLawKind.BranchGrowth));
        AssertRoundTrip(SanctuaryGelActionAtlas.GetAction(SanctuaryGelActionKind.Attend));
        AssertRoundTrip(SanctuaryGelActionAtlas.GetCompositionOperator(SanctuaryGelCompositionOperatorKind.Sequential));
    }

    [Fact]
    public void Build_Readiness_References_All_Six_Gel_Doctrine_Notes()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessText = File.ReadAllText(Path.Combine(lineRoot, "docs", "BUILD_READINESS.md"));

        AssertContainsAll(
            buildReadinessText,
            "GEL_DISCERNMENT_LAYERING_AND_TRANSLATIVE_BINDING_NOTE.md",
            "PRE_LISP_IUTT_LISP_AND_LLM_ACTING_OPERATOR_NOTE.md",
            "PRIME_ROOT_CARRIER_REDUCTION_DECOMPOSITION_AND_ANCHOR_EMERGENCE_NOTE.md",
            "A0_GEL_AXIOM_FLOOR_NOTE.md",
            "GEL_DERIVED_GROWTH_LAWS_NOTE.md",
            "GEL_ACTION_BASIS_AND_COMPOSITION_NOTE.md",
            "gel-discernment-layering-and-translative-binding-note: frame-now",
            "pre-lisp-iutt-lisp-and-llm-acting-operator-note: frame-now",
            "prime-root-carrier-reduction-decomposition-and-anchor-emergence-note: frame-now",
            "a0-gel-axiom-floor-note: frame-now",
            "gel-derived-growth-laws-note: frame-now",
            "gel-action-basis-and-composition-note: frame-now");
    }

    [Fact]
    public void Gel_Doctrine_Notes_State_The_Required_Seams()
    {
        var lineRoot = GetLineRoot();
        var docsRoot = Path.Combine(lineRoot, "docs");
        var noteNames = new[]
        {
            "GEL_DISCERNMENT_LAYERING_AND_TRANSLATIVE_BINDING_NOTE.md",
            "PRE_LISP_IUTT_LISP_AND_LLM_ACTING_OPERATOR_NOTE.md",
            "PRIME_ROOT_CARRIER_REDUCTION_DECOMPOSITION_AND_ANCHOR_EMERGENCE_NOTE.md",
            "A0_GEL_AXIOM_FLOOR_NOTE.md",
            "GEL_DERIVED_GROWTH_LAWS_NOTE.md",
            "GEL_ACTION_BASIS_AND_COMPOSITION_NOTE.md"
        };

        foreach (var noteName in noteNames)
        {
            var text = File.ReadAllText(Path.Combine(docsRoot, noteName));
            AssertContainsAll(
                text,
                "`GEL` is a layered field, not a flat database.",
                "`Root` and `Dictionary` are operational now.",
                "`Encyclopedic` and `Procedural` are named but withheld.",
                "`SLI` is translative binding, not a new top-level knowledge layer.",
                "English is the first localized theater.",
                "The hosted seed `LLM` is the acting operator.",
                "`SLI/Lisp` is binding / transport / governance medium",
                "not the action engine.",
                "Theater, Role, Enrichment, and Bounded Form are operational surfaces",
                "full ontological selfhood.",
                "Governance shapes what may persist, not what latent space may explore.");
        }

        var preLispText = File.ReadAllText(Path.Combine(docsRoot, "PRE_LISP_IUTT_LISP_AND_LLM_ACTING_OPERATOR_NOTE.md"));
        AssertContainsAll(
            preLispText,
            "PL0 = (G_EN, δ, α, τ, ε, Σ_prime)",
            "LB0 = (G_EN, δ, α, τ, ε, Σ_prime, Λ)",
            "The minimal Lisp being surface is placeholder-only.");

        var primeText = File.ReadAllText(Path.Combine(docsRoot, "PRIME_ROOT_CARRIER_REDUCTION_DECOMPOSITION_AND_ANCHOR_EMERGENCE_NOTE.md"));
        AssertContainsAll(
            primeText,
            "Prime(x)",
            "must-recur-across-independent-reductions-within-one-governed-seed",
            "`maybe` is lawful provisional standing, not soft truth.",
            "Convergence is single-seed in this slice.",
            "The current lexeme and cleaver surfaces remain lexical pre-Prime stages.",
            "`sli.root_atlas_entry` remains structural and distinct from semantic Prime",
            "carriers");

        var axiomText = File.ReadAllText(Path.Combine(docsRoot, "A0_GEL_AXIOM_FLOOR_NOTE.md"));
        AssertContainsAll(
            axiomText,
            "non-breakable floor",
            "future laws must preserve `A0`");

        var derivedText = File.ReadAllText(Path.Combine(docsRoot, "GEL_DERIVED_GROWTH_LAWS_NOTE.md"));
        AssertContainsAll(
            derivedText,
            "`L1` through `L5` are contract-backed now.",
            "`L6` through `L8` are doctrine-only now.");

        var actionText = File.ReadAllText(Path.Combine(docsRoot, "GEL_ACTION_BASIS_AND_COMPOSITION_NOTE.md"));
        AssertContainsAll(
            actionText,
            "Action basis and composition operators are contract-backed",
            "canonical",
            "flows remain doctrine-only",
            "Existing `ProtectedExecutionActFamily` and routing surfaces are precedent",
            "source of truth for `GEL`-native cognitive actions.");

        var hostedNoteText = File.ReadAllText(Path.Combine(docsRoot, "HOSTED_LLM_RESIDENT_SEATING_NOTE.md"));
        AssertContainsAll(
            hostedNoteText,
            "The hosted seed `LLM` is the acting operator.",
            "The observational `GEL` layer witness lane is also local-only and opt-in.",
            "`root-hold`",
            "`dictionary-hold`",
            "Return to the requested layer. Do not expand.",
            "The `GEL` witness lane is likewise observational only. It is not a release",
            "gate");
    }

    private static void AssertAxiom(
        SanctuaryGelAxiomKind kind,
        string handle,
        string requirement,
        IReadOnlyList<string> preservedSurfaces,
        IReadOnlyList<string> failureConsequences)
    {
        var definition = SanctuaryGelAxiomAtlas.Get(kind);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(requirement, definition.Requirement);
        Assert.Equal(preservedSurfaces, definition.PreservedSurfaces);
        Assert.Equal(failureConsequences, definition.FailureConsequences);
        Assert.Equal("placeholder-contract-only", definition.EnforcementStatus);
    }

    private static void AssertDerivedLaw(
        SanctuaryGelDerivedLawKind kind,
        string handle,
        string requirement,
        IReadOnlyList<string> governingAxiomHandles,
        string admissionRule,
        IReadOnlyList<string> failureResponses)
    {
        var definition = SanctuaryGelDerivedLawAtlas.Get(kind);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(requirement, definition.Requirement);
        Assert.Equal(governingAxiomHandles, definition.GoverningAxiomHandles);
        Assert.Equal(admissionRule, definition.AdmissionRule);
        Assert.Equal(failureResponses, definition.FailureResponses);
        Assert.Equal("placeholder-contract-only", definition.OperationalStatus);
    }

    private static void AssertAction(
        SanctuaryGelActionKind kind,
        string handle,
        string requirement,
        IReadOnlyList<string> governingHandles,
        IReadOnlyList<string> successConditions,
        IReadOnlyList<string> failureDispositions)
    {
        var definition = SanctuaryGelActionAtlas.GetAction(kind);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(requirement, definition.Requirement);
        Assert.Equal(governingHandles, definition.GoverningHandleReferences);
        Assert.Equal(successConditions, definition.SuccessConditions);
        Assert.Equal(failureDispositions, definition.FailureDispositions);
        Assert.Equal("placeholder-contract-only", definition.OperationalStatus);
    }

    private static void AssertComposition(
        SanctuaryGelCompositionOperatorKind kind,
        string handle,
        string requirement,
        IReadOnlyList<string> guardClasses,
        string terminationCondition)
    {
        var definition = SanctuaryGelActionAtlas.GetCompositionOperator(kind);
        Assert.Equal(handle, definition.Handle);
        Assert.Equal(requirement, definition.Requirement);
        Assert.Equal(guardClasses, definition.GuardClasses);
        Assert.Equal(terminationCondition, definition.TerminationCondition);
        Assert.Equal("placeholder-contract-only", definition.OperationalStatus);
    }

    private static void AssertRoundTrip<T>(T value)
    {
        var json = JsonSerializer.Serialize(value);
        var restored = JsonSerializer.Deserialize<T>(json, JsonOptions);

        Assert.NotNull(restored);
        Assert.Equal(json, JsonSerializer.Serialize(restored));
    }

    private static void AssertContainsAll(string text, params string[] markers)
    {
        foreach (var marker in markers)
        {
            Assert.Contains(marker, text, StringComparison.Ordinal);
        }
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Mortalis V1.1.1", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the V1.1.1 line root.");
    }
}
