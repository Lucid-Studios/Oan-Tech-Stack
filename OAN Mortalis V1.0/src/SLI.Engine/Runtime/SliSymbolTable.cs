using GEL.Graphs;
using SLI.Engine.Morphology;
using SLI.Engine.Models;
using SoulFrame.Host;
using System.Globalization;

namespace SLI.Engine.Runtime;

public delegate Task<SExpression> SliOperator(
    SExpression expression,
    SliExecutionContext context,
    CancellationToken cancellationToken);

public sealed class SliSymbolTable
{
    private readonly Dictionary<string, SliOperator> _operators = new(StringComparer.OrdinalIgnoreCase);

    public SliSymbolTable()
    {
        RegisterDefaults();
    }

    public void Register(string symbol, SliOperator handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentNullException.ThrowIfNull(handler);
        _operators[symbol] = handler;
    }

    public bool TryResolve(string symbol, out SliOperator handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        return _operators.TryGetValue(symbol, out handler!);
    }

    private void RegisterDefaults()
    {
        Register("engram-ref", async (expression, context, cancellationToken) =>
        {
            var concept = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var result = await context.Resolver.ResolveConceptAsync(concept, cancellationToken).ConfigureAwait(false);
            context.AddResolvedEngrams(result.Summaries);
            context.AddTrace($"engram-ref({concept})");
            return SExpression.AtomNode(result.Summaries.Count.ToString());
        });

        Register("context-expand", (expression, context, _) =>
        {
            context.AddTrace($"context-expand({context.Frame.TaskObjective})");
            return Task.FromResult(SExpression.AtomNode("expanded"));
        });

        Register("engram-query", async (expression, context, cancellationToken) =>
        {
            var concept = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var result = await context.Resolver.ResolveConceptAsync(concept, cancellationToken).ConfigureAwait(false);
            context.AddResolvedEngrams(result.Summaries);
            context.AddTrace($"engram-query({concept})");
            return SExpression.AtomNode(result.Summaries.Count.ToString());
        });

        Register("llm_classify", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.ClassifyAsync(
                    BuildInferenceRequest("classify", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.AddTrace(response.Accepted
                ? $"llm_classify({response.Decision})"
                : "llm_classify(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        Register("llm_expand", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.SemanticExpandAsync(
                    BuildInferenceRequest("semantic_expand", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.AddTrace(response.Accepted
                ? $"llm_expand({response.Decision})"
                : "llm_expand(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        Register("llm_context_infer", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.InferAsync(
                    BuildInferenceRequest("context_infer", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.AddTrace(response.Accepted
                ? $"llm_context_infer({response.Decision})"
                : "llm_context_infer(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        Register("predicate-evaluate", (expression, context, _) =>
        {
            var predicate = Arg(expression, 1, "identity-continuity");
            context.AddTrace($"predicate-evaluate({predicate})");
            return Task.FromResult(SExpression.AtomNode("predicate-ok"));
        });

        Register("decision-evaluate", (expression, context, _) =>
        {
            var branchSet = context.ActiveEngrams
                .OrderByDescending(e => e.ConfidenceWeight)
                .ThenBy(e => e.EngramId, StringComparer.Ordinal)
                .Select(e => $"{e.ConceptTag.ToLowerInvariant()}-path")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();

            if (branchSet.Count == 0)
            {
                branchSet.Add("identity-continuity-path");
            }

            context.CandidateBranches.Clear();
            context.CandidateBranches.AddRange(branchSet);
            context.AddTrace($"decision-evaluate({context.CandidateBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("evaluated"));
        });

        Register("decision-branch", (expression, context, _) =>
        {
            context.AddTrace($"decision-branch({context.CandidateBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("branched"));
        });

        Register("compass-update", (expression, context, _) =>
        {
            var arg1 = Arg(expression, 1, "context");
            var arg2 = Arg(expression, 2, "reasoning-state");
            context.AddTrace($"compass-update({arg1} {arg2})");
            return Task.FromResult(SExpression.AtomNode("compass-ok"));
        });

        Register("cleave", (expression, context, _) =>
        {
            var survivors = context.CandidateBranches.Take(1).ToList();
            var pruned = context.CandidateBranches.Skip(1).ToList();
            context.PrunedBranches.Clear();
            context.PrunedBranches.AddRange(pruned);
            context.CandidateBranches.Clear();
            context.CandidateBranches.AddRange(survivors);
            context.AddTrace($"cleave({context.PrunedBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("cleaved"));
        });

        Register("commit", (expression, context, _) =>
        {
            context.FinalDecision = context.CandidateBranches.FirstOrDefault() ?? "defer";
            context.AddTrace($"commit({context.FinalDecision})");
            return Task.FromResult(SExpression.AtomNode(context.FinalDecision));
        });

        RegisterHigherOrderLocalityOperators();
        RegisterBoundedRehearsalOperators();
        RegisterBoundedWitnessOperators();
        RegisterBoundedTransportOperators();
        RegisterBoundedAdmissibilityOperators();

        Register("morph-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(root))
            {
                context.MorphologyState.ResolvedLemmaRoots.Add(root);
                context.AddTrace($"morph-root({root})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-root"));
        });

        Register("morph-operator", (expression, context, _) =>
        {
            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var kind = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(kind))
            {
                context.MorphologyState.OperatorAnnotations.Add((token, kind));
                context.AddTrace($"morph-operator({token}:{kind})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-operator"));
        });

        Register("morph-constructor", (expression, context, _) =>
        {
            var role = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var rootKey = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(role) && !string.IsNullOrWhiteSpace(rootKey))
            {
                context.MorphologyState.ConstructorBodies.Add((role, rootKey));
                context.AddTrace($"morph-constructor({role}:{rootKey})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-constructor"));
        });

        Register("morph-predicate-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.PredicateRoot = root;
            context.AddTrace($"morph-predicate-root({root})");
            return Task.FromResult(SExpression.AtomNode("morph-predicate-root"));
        });

        Register("morph-render", (expression, context, _) =>
        {
            var render = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.DiagnosticPredicateRender = render;
            context.AddTrace($"morph-render({render})");
            return Task.FromResult(SExpression.AtomNode("morph-render"));
        });

        Register("morph-summary", (expression, context, _) =>
        {
            var summary = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.Summary = summary;
            context.AddTrace($"morph-summary({summary})");
            return Task.FromResult(SExpression.AtomNode("morph-summary"));
        });

        Register("morph-scalar", (expression, context, _) =>
        {
            var scalar = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.ScalarPayload = scalar;
            context.AddTrace($"morph-scalar({scalar})");
            return Task.FromResult(SExpression.AtomNode("morph-scalar"));
        });

        Register("morph-outcome", (expression, context, _) =>
        {
            var outcome = UnwrapStringLiteral(Arg(expression, 1, "OutOfScope"));
            context.MorphologyState.Outcome = outcome;
            context.AddTrace($"morph-outcome({outcome})");
            return Task.FromResult(SExpression.AtomNode("morph-outcome"));
        });

        Register("morph-graph-edge", (expression, context, _) =>
        {
            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            var relation = UnwrapStringLiteral(Arg(expression, 3, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) &&
                !string.IsNullOrWhiteSpace(target) &&
                !string.IsNullOrWhiteSpace(relation))
            {
                context.MorphologyState.GraphEdges.Add(new ConstructorEdge
                {
                    Source = source,
                    Target = target,
                    Relation = relation
                });
                context.AddTrace($"morph-graph-edge({source}->{target}:{relation})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-graph-edge"));
        });

        Register("morph-anchor", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(anchor))
            {
                context.MorphologyState.ContinuityAnchors.Add(anchor);
                context.AddTrace($"morph-anchor({anchor})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-anchor"));
        });

        Register("morph-invariant", (expression, context, _) =>
        {
            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant))
            {
                context.MorphologyState.BodyInvariants.Add(invariant);
                context.AddTrace($"morph-invariant({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-invariant"));
        });

        Register("morph-cluster-entry", (expression, context, _) =>
        {
            var entry = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(entry))
            {
                context.MorphologyState.ClusterEntries.Add(entry);
                context.AddTrace($"morph-cluster-entry({entry})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-cluster-entry"));
        });

        Register("morph-body-summary", (expression, context, _) =>
        {
            var summary = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.BodySummary = summary;
            context.AddTrace($"morph-body-summary({summary})");
            return Task.FromResult(SExpression.AtomNode("morph-body-summary"));
        });

        Register("prop-subject-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Subject.RootKey = root;
            context.AddTrace($"prop-subject-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-subject-root"));
        });

        Register("prop-subject-handle", (expression, context, _) =>
        {
            var handle = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Subject.SymbolicHandle = handle;
            context.AddTrace($"prop-subject-handle({handle})");
            return Task.FromResult(SExpression.AtomNode("prop-subject-handle"));
        });

        Register("prop-predicate-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.PredicateRoot = root;
            context.AddTrace($"prop-predicate-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-predicate-root"));
        });

        Register("prop-object-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Object.RootKey = root;
            context.AddTrace($"prop-object-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-object-root"));
        });

        Register("prop-object-handle", (expression, context, _) =>
        {
            var handle = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Object.SymbolicHandle = handle;
            context.AddTrace($"prop-object-handle({handle})");
            return Task.FromResult(SExpression.AtomNode("prop-object-handle"));
        });

        Register("prop-qualifier", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var value = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.PropositionState.Qualifiers.Add(new SliPropositionQualifierResult
                {
                    Name = name,
                    Value = value
                });
                context.AddTrace($"prop-qualifier({name}:{value})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-qualifier"));
        });

        Register("prop-context", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var value = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.PropositionState.ContextTags.Add(new SliPropositionContextTagResult
                {
                    Name = name,
                    Value = value
                });
                context.AddTrace($"prop-context({name}:{value})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-context"));
        });

        Register("prop-render", (expression, context, _) =>
        {
            var render = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.DiagnosticRender = render;
            context.AddTrace($"prop-render({render})");
            return Task.FromResult(SExpression.AtomNode("prop-render"));
        });

        Register("prop-tension", (expression, context, _) =>
        {
            var tension = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(tension))
            {
                context.PropositionState.UnresolvedTensions.Add(tension);
                context.AddTrace($"prop-tension({tension})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-tension"));
        });

        Register("prop-grade", (expression, context, _) =>
        {
            var grade = UnwrapStringLiteral(Arg(expression, 1, SliPropositionalCompileGrade.NeedsSpecification.ToString()));
            context.PropositionState.Grade = grade;
            context.AddTrace($"prop-grade({grade})");
            return Task.FromResult(SExpression.AtomNode("prop-grade"));
        });
    }

    private void RegisterHigherOrderLocalityOperators()
    {
        Register("locality-bind", (expression, context, _) =>
        {
            var localitySeed = UnwrapStringLiteral(Arg(expression, 1, "context"));
            var localityHandle = $"{localitySeed}:{context.Frame.CMEId}:{context.Frame.ContextId:D}";
            context.HigherOrderLocalityState.Reset(localityHandle);
            context.AddTrace($"locality-bind({localitySeed})");
            return Task.FromResult(SExpression.AtomNode("locality-bind"));
        });

        Register("anchor-self", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.SelfAnchor = anchor;
            context.AddTrace($"anchor-self({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-self"));
        });

        Register("anchor-other", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.OtherAnchor = anchor;
            context.AddTrace($"anchor-other({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-other"));
        });

        Register("anchor-relation", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.RelationAnchor = anchor;
            context.AddTrace($"anchor-relation({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-relation"));
        });

        Register("seal-posture", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.BoundedSealPosture));
            if (string.Equals(value, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "sealed", StringComparison.OrdinalIgnoreCase))
            {
                state.SealPosture = value.ToLowerInvariant();
            }
            else
            {
                state.SealPosture = ResolveSafeValue(state.SealPosture, SliHigherOrderLocalityState.BoundedSealPosture);
                state.AddWarning($"seal-posture rejected invalid value '{value}'.");
                AddResidue(context, state.Residues, HigherOrderLocalityResidueKind.InvalidPostureValue, "seal-posture", $"Rejected invalid seal posture '{value}'.");
            }

            context.AddTrace($"seal-posture({state.SealPosture})");
            return Task.FromResult(SExpression.AtomNode("seal-posture"));
        });

        Register("reveal-posture", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.MaskedRevealPosture));
            if (string.Equals(value, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "narrow", StringComparison.OrdinalIgnoreCase))
            {
                state.RevealPosture = value.ToLowerInvariant();
            }
            else
            {
                state.RevealPosture = ResolveSafeValue(state.RevealPosture, SliHigherOrderLocalityState.MaskedRevealPosture);
                state.AddWarning($"reveal-posture rejected invalid value '{value}'.");
                AddResidue(context, state.Residues, HigherOrderLocalityResidueKind.InvalidPostureValue, "reveal-posture", $"Rejected invalid reveal posture '{value}'.");
            }

            context.AddTrace($"reveal-posture({state.RevealPosture})");
            return Task.FromResult(SExpression.AtomNode("reveal-posture"));
        });

        Register("perspective-configure", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var missingLocality = string.IsNullOrWhiteSpace(state.LocalityHandle);
            var missingAnchors =
                string.IsNullOrWhiteSpace(state.SelfAnchor) ||
                string.IsNullOrWhiteSpace(state.OtherAnchor) ||
                string.IsNullOrWhiteSpace(state.RelationAnchor);

            if (missingLocality)
            {
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.MissingLocalityPrerequisites,
                    "perspective-configure",
                    "Perspective configuration requires a bound locality handle.");
            }

            if (missingAnchors)
            {
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.MissingAnchorPrerequisites,
                    "perspective-configure",
                    "Perspective configuration requires self, other, and relation anchors.");
            }

            if (missingLocality || missingAnchors)
            {
                state.Perspective.IsConfigured = false;
                state.AddWarning("perspective-configure remained incomplete.");
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-configure",
                    "Perspective configuration remained incomplete because prerequisites were missing.");
                context.AddTrace("perspective-configure(incomplete)");
                return Task.FromResult(SExpression.AtomNode("perspective-incomplete"));
            }

            state.Perspective.IsConfigured = true;
            context.AddTrace($"perspective-configure({UnwrapStringLiteral(Arg(expression, 1, "locality-state"))})");
            return Task.FromResult(SExpression.AtomNode("perspective-configure"));
        });

        Register("perspective-orientation", (expression, context, _) =>
        {
            var focus = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var weightValue = UnwrapStringLiteral(Arg(expression, 2, "0.0"));
            if (string.IsNullOrWhiteSpace(focus) ||
                !double.TryParse(weightValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight))
            {
                context.HigherOrderLocalityState.AddWarning($"perspective-orientation ignored invalid weight '{weightValue}'.");
                AddResidue(
                    context,
                    context.HigherOrderLocalityState.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-orientation",
                    $"Perspective orientation rejected invalid weight '{weightValue}'.");
                context.AddTrace("perspective-orientation(invalid)");
                return Task.FromResult(SExpression.AtomNode("perspective-orientation-invalid"));
            }

            context.HigherOrderLocalityState.Perspective.OrientationVector[focus] = weight;
            context.AddTrace($"perspective-orientation({focus}:{weight.ToString("0.0###", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("perspective-orientation"));
        });

        Register("perspective-constraint", (expression, context, _) =>
        {
            var constraint = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(constraint))
            {
                context.HigherOrderLocalityState.Perspective.EthicalConstraints.Add(constraint);
                context.AddTrace($"perspective-constraint({constraint})");
            }

            return Task.FromResult(SExpression.AtomNode("perspective-constraint"));
        });

        Register("perspective-weight", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var rawValue = UnwrapStringLiteral(Arg(expression, 2, "0.0"));
            if (string.IsNullOrWhiteSpace(name) ||
                !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                context.HigherOrderLocalityState.AddWarning($"perspective-weight ignored invalid value '{rawValue}'.");
                AddResidue(
                    context,
                    context.HigherOrderLocalityState.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-weight",
                    $"Perspective weight rejected invalid value '{rawValue}'.");
                context.AddTrace("perspective-weight(invalid)");
                return Task.FromResult(SExpression.AtomNode("perspective-weight-invalid"));
            }

            context.HigherOrderLocalityState.Perspective.WeightFunctions[name] = value;
            context.AddTrace($"perspective-weight({name}:{value.ToString("0.0###", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("perspective-weight"));
        });

        Register("perspective-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "perspective residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Perspective.Residues,
                HigherOrderLocalityResidueKind.IncompletePerspective,
                "perspective-residue",
                detail);
            context.AddTrace($"perspective-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("perspective-residue"));
        });

        Register("participation-configure", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            if (!state.Perspective.IsConfigured)
            {
                state.Participation.IsConfigured = false;
                state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
                state.AddWarning("participation-configure remained incomplete because perspective is not configured.");
                AddResidue(
                    context,
                    state.Participation.Residues,
                    HigherOrderLocalityResidueKind.IncompleteParticipation,
                    "participation-configure",
                    "Participation configuration requires completed perspective configuration.");
                context.AddTrace("participation-configure(incomplete)");
                return Task.FromResult(SExpression.AtomNode("participation-incomplete"));
            }

            state.Participation.IsConfigured = true;
            state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
            context.AddTrace($"participation-configure({UnwrapStringLiteral(Arg(expression, 1, "locality-state"))})");
            return Task.FromResult(SExpression.AtomNode("participation-configure"));
        });

        Register("participation-mode", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.ObserveMode));
            if (string.Equals(value, SliHigherOrderLocalityState.ObserveMode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "engage", StringComparison.OrdinalIgnoreCase))
            {
                state.Participation.Mode = value.ToLowerInvariant();
            }
            else
            {
                state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
                state.AddWarning($"participation-mode rejected invalid value '{value}'.");
                AddResidue(
                    context,
                    state.Participation.Residues,
                    HigherOrderLocalityResidueKind.InvalidParticipationMode,
                    "participation-mode",
                    $"Rejected invalid participation mode '{value}'.");
            }

            context.AddTrace($"participation-mode({state.Participation.Mode})");
            return Task.FromResult(SExpression.AtomNode("participation-mode"));
        });

        Register("participation-role", (expression, context, _) =>
        {
            var role = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.Participation.Role = role;
            context.AddTrace($"participation-role({role})");
            return Task.FromResult(SExpression.AtomNode("participation-role"));
        });

        Register("participation-rule", (expression, context, _) =>
        {
            var rule = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(rule))
            {
                context.HigherOrderLocalityState.Participation.InteractionRules.Add(rule);
                context.AddTrace($"participation-rule({rule})");
            }

            return Task.FromResult(SExpression.AtomNode("participation-rule"));
        });

        Register("participation-capability", (expression, context, _) =>
        {
            var capability = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(capability))
            {
                context.HigherOrderLocalityState.Participation.CapabilitySet.Add(capability);
                context.AddTrace($"participation-capability({capability})");
            }

            return Task.FromResult(SExpression.AtomNode("participation-capability"));
        });

        Register("participation-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "participation residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Participation.Residues,
                HigherOrderLocalityResidueKind.IncompleteParticipation,
                "participation-residue",
                detail);
            context.AddTrace($"participation-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("participation-residue"));
        });
    }

    private void RegisterBoundedRehearsalOperators()
    {
        Register("rehearsal-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var rehearsal = state.Rehearsal;
            var modeValue = UnwrapStringLiteral(Arg(expression, 2, SliRehearsalState.DreamGameMode));
            var mode = ResolveRehearsalMode(modeValue);
            if (!string.Equals(modeValue, mode, StringComparison.OrdinalIgnoreCase))
            {
                rehearsal.Warnings.Add($"rehearsal-begin rejected invalid mode '{modeValue}'.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.InvalidRehearsalMode,
                    "rehearsal-begin",
                    $"Rejected invalid rehearsal mode '{modeValue}'.");
            }

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured ||
                !HasSafeSealPosture(state.SealPosture);

            if (missingPrerequisites)
            {
                rehearsal.IsConfigured = false;
                rehearsal.Warnings.Add("rehearsal-begin requires completed locality, perspective, participation, and safe seal posture.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.MissingRehearsalPrerequisites,
                    "rehearsal-begin",
                    "Rehearsal requires completed locality, perspective, participation, and safe seal posture.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-begin",
                    "Rehearsal remained incomplete because prerequisites were missing.");
                context.AddTrace("rehearsal-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-incomplete"));
            }

            rehearsal.Configure(state.LocalityHandle, mode);
            context.AddTrace($"rehearsal-begin({mode})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-begin"));
        });

        Register("rehearsal-branch", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-branch requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-branch",
                    "Branch formation requires an active rehearsal.");
                context.AddTrace("rehearsal-branch(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-branch-incomplete"));
            }

            var branch = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(branch))
            {
                rehearsal.BranchSet.Add(branch);
                context.AddTrace($"rehearsal-branch({branch})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-branch"));
        });

        Register("rehearsal-substitute", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-substitute requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-substitute",
                    "Substitution requires an active rehearsal.");
                context.AddTrace("rehearsal-substitute(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-substitute-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
            {
                rehearsal.SubstitutionLedger.Add(new SliRehearsalSubstitutionState
                {
                    Source = source,
                    Target = target
                });
                context.AddTrace($"rehearsal-substitute({source}->{target})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-substitute"));
        });

        Register("rehearsal-analogy", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-analogy requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-analogy",
                    "Analogy requires an active rehearsal.");
                context.AddTrace("rehearsal-analogy(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-analogy-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
            {
                rehearsal.AnalogyLedger.Add(new SliRehearsalAnalogyState
                {
                    Source = source,
                    Target = target
                });
                context.AddTrace($"rehearsal-analogy({source}~{target})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-analogy"));
        });

        Register("rehearsal-seal", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliRehearsalState.IdentitySealed));
            if (string.Equals(value, SliRehearsalState.IdentitySealed, StringComparison.OrdinalIgnoreCase))
            {
                rehearsal.IdentitySeal = SliRehearsalState.IdentitySealed;
            }
            else
            {
                rehearsal.IdentitySeal = SliRehearsalState.IdentitySealed;
                rehearsal.IsBindable = false;
                rehearsal.Warnings.Add($"rehearsal-seal rejected invalid value '{value}'.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.InvalidIdentitySeal,
                    "rehearsal-seal",
                    $"Rejected invalid rehearsal identity seal '{value}'.");
            }

            context.AddTrace($"rehearsal-seal({rehearsal.IdentitySeal})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-seal"));
        });

        Register("rehearsal-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "rehearsal residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Rehearsal.Residues,
                HigherOrderLocalityResidueKind.IncompleteRehearsal,
                "rehearsal-residue",
                detail);
            context.AddTrace($"rehearsal-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-residue"));
        });
    }

    private void RegisterBoundedWitnessOperators()
    {
        Register("witness-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var witness = state.Witness;

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured;

            if (missingPrerequisites)
            {
                witness.IsConfigured = false;
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("witness-begin requires completed locality, perspective, and participation.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.MissingWitnessPrerequisites,
                    "witness-begin",
                    "Witness formation requires completed locality, perspective, and participation.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-begin",
                    "Witness remained incomplete because prerequisites were missing.");
                context.AddTrace("witness-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-incomplete"));
            }

            var leftToken = UnwrapStringLiteral(Arg(expression, 1, "locality-state"));
            var rightToken = UnwrapStringLiteral(Arg(expression, 2, "locality-state"));

            var leftResolved = TryResolveWitnessReference(context, leftToken, out var leftHandle);
            var rightResolved = TryResolveWitnessReference(context, rightToken, out var rightHandle);
            if (!leftResolved || !rightResolved)
            {
                witness.IsConfigured = false;
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("witness-begin rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.InvalidWitnessReference,
                    "witness-begin",
                    $"Witness rejected unresolved references '{leftToken}' and/or '{rightToken}'.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-begin",
                    "Witness remained incomplete because a comparison reference could not be resolved.");
                context.AddTrace("witness-begin(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("witness-invalid-reference"));
            }

            witness.Configure(leftHandle, rightHandle);
            context.AddTrace($"witness-begin({leftToken}|{rightToken})");
            return Task.FromResult(SExpression.AtomNode("witness-begin"));
        });

        Register("witness-compare", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-compare requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-compare",
                    "Comparison requires an active witness.");
                context.AddTrace("witness-compare(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-compare-incomplete"));
            }

            witness.CandidacyStatus = SliWitnessState.Comparable;
            context.AddTrace($"witness-compare({witness.LeftLocalityHandle}|{witness.RightLocalityHandle})");
            return Task.FromResult(SExpression.AtomNode("witness-compare"));
        });

        Register("witness-preserve", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-preserve requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-preserve",
                    "Preservation claims require an active witness.");
                context.AddTrace("witness-preserve(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-preserve-incomplete"));
            }

            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant) &&
                !witness.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
            {
                witness.PreservedInvariants.Add(invariant);
                context.AddTrace($"witness-preserve({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("witness-preserve"));
        });

        Register("witness-difference", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-difference requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-difference",
                    "Difference claims require an active witness.");
                context.AddTrace("witness-difference(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-difference-incomplete"));
            }

            var difference = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(difference) &&
                !witness.DifferenceSet.Contains(difference, StringComparer.OrdinalIgnoreCase))
            {
                witness.DifferenceSet.Add(difference);
                context.AddTrace($"witness-difference({difference})");
            }

            return Task.FromResult(SExpression.AtomNode("witness-difference"));
        });

        Register("witness-residue", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            var detail = UnwrapStringLiteral(Arg(expression, 1, "witness residue"));
            AddResidue(
                context,
                witness.Residues,
                HigherOrderLocalityResidueKind.LawfulDifferenceResidue,
                "witness-residue",
                detail);
            context.AddTrace($"witness-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("witness-residue"));
        });

        Register("glue-threshold", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            var rawValue = UnwrapStringLiteral(Arg(expression, 1, SliWitnessState.DefaultGlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)));
            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold) ||
                threshold < 0.0 ||
                threshold > 1.0)
            {
                witness.GlueThreshold = SliWitnessState.DefaultGlueThreshold;
                witness.Warnings.Add($"glue-threshold rejected invalid value '{rawValue}'.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.InvalidGlueThreshold,
                    "glue-threshold",
                    $"Rejected invalid glue threshold '{rawValue}'.");
                context.AddTrace($"glue-threshold({witness.GlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)})");
                return Task.FromResult(SExpression.AtomNode("glue-threshold-invalid"));
            }

            witness.GlueThreshold = threshold;
            context.AddTrace($"glue-threshold({threshold.ToString("0.00", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("glue-threshold"));
        });

        Register("morphism-candidate", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("morphism-candidate requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "morphism-candidate",
                    "Morphism candidacy requires an active witness.");
                context.AddTrace("morphism-candidate(incomplete)");
                return Task.FromResult(SExpression.AtomNode("morphism-incomplete"));
            }

            var blockingResidues = witness.Residues.Any(residue =>
                residue.Kind is HigherOrderLocalityResidueKind.MissingWitnessPrerequisites or
                    HigherOrderLocalityResidueKind.InvalidWitnessReference or
                    HigherOrderLocalityResidueKind.InvalidGlueThreshold or
                    HigherOrderLocalityResidueKind.IncompleteWitness);

            var missingPreservedInvariants = RequiredWitnessInvariants()
                .Except(witness.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var disallowedDifferences = witness.DifferenceSet
                .Where(difference => !AllowedWitnessDifferences().Contains(difference, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (blockingResidues)
            {
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                context.AddTrace("morphism-candidate(non-candidate)");
                return Task.FromResult(SExpression.AtomNode("morphism-non-candidate"));
            }

            if (missingPreservedInvariants.Length > 0 ||
                disallowedDifferences.Length > 0 ||
                witness.GlueThreshold < SliWitnessState.DefaultGlueThreshold)
            {
                witness.CandidacyStatus = SliWitnessState.Comparable;

                if (missingPreservedInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Missing preserved invariants: {string.Join(", ", missingPreservedInvariants)}.");
                }

                if (disallowedDifferences.Length > 0)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Differences are not morphism-safe: {string.Join(", ", disallowedDifferences)}.");
                }

                if (witness.GlueThreshold < SliWitnessState.DefaultGlueThreshold)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Glue threshold {witness.GlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)} remained below candidacy floor.");
                }

                context.AddTrace("morphism-candidate(comparable)");
                return Task.FromResult(SExpression.AtomNode("morphism-comparable"));
            }

            witness.CandidacyStatus = SliWitnessState.MorphismCandidate;
            context.AddTrace("morphism-candidate(candidate)");
            return Task.FromResult(SExpression.AtomNode("morphism-candidate"));
        });
    }

    private void RegisterBoundedTransportOperators()
    {
        Register("transport-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var witness = state.Witness;
            var transport = state.Transport;

            var witnessToken = UnwrapStringLiteral(Arg(expression, 1, "witness-state"));
            if (!TryResolveTransportWitnessHandle(state, witnessToken, out var witnessHandle))
            {
                transport.IsConfigured = false;
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-begin requires a resolved witness handle.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                    "transport-begin",
                    "Transport requires a resolved witness handle.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-begin",
                    "Transport remained incomplete because no witness handle was available.");
                context.AddTrace("transport-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-incomplete"));
            }

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured ||
                !witness.IsConfigured ||
                !string.Equals(witness.CandidacyStatus, SliWitnessState.MorphismCandidate, StringComparison.OrdinalIgnoreCase) ||
                HasBlockingWitnessResidues(witness);

            if (missingPrerequisites)
            {
                transport.IsConfigured = false;
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-begin requires completed locality, perspective, participation, and positive witness candidacy.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                    "transport-begin",
                    "Transport requires completed locality, perspective, participation, and morphism candidacy.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.BlockedTransport,
                    "transport-begin",
                    "Transport was blocked because witness candidacy did not authorize carry.");
                context.AddTrace("transport-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            transport.Configure(witnessHandle);
            context.AddTrace($"transport-begin({witnessToken})");
            return Task.FromResult(SExpression.AtomNode("transport-begin"));
        });

        Register("transport-source", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-source requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-source",
                    "Transport source requires an active transport.");
                context.AddTrace("transport-source(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-source-incomplete"));
            }

            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!TryResolveWitnessReference(context, token, out var sourceHandle))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-source rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportReference,
                    "transport-source",
                    $"Transport rejected unresolved source reference '{token}'.");
                context.AddTrace("transport-source(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("transport-source-invalid"));
            }

            transport.SourceLocalityHandle = sourceHandle;
            context.AddTrace($"transport-source({token})");
            return Task.FromResult(SExpression.AtomNode("transport-source"));
        });

        Register("transport-target", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-target requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-target",
                    "Transport target requires an active transport.");
                context.AddTrace("transport-target(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-target-incomplete"));
            }

            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!TryResolveWitnessReference(context, token, out var targetHandle))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-target rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportReference,
                    "transport-target",
                    $"Transport rejected unresolved target reference '{token}'.");
                context.AddTrace("transport-target(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("transport-target-invalid"));
            }

            transport.TargetLocalityHandle = targetHandle;
            context.AddTrace($"transport-target({token})");
            return Task.FromResult(SExpression.AtomNode("transport-target"));
        });

        Register("transport-preserve", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-preserve requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-preserve",
                    "Transport preservation requires an active transport.");
                context.AddTrace("transport-preserve(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-preserve-incomplete"));
            }

            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant) &&
                !transport.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
            {
                transport.PreservedInvariants.Add(invariant);
                context.AddTrace($"transport-preserve({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("transport-preserve"));
        });

        Register("transport-map", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-map requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-map",
                    "Transport mapping requires an active transport.");
                context.AddTrace("transport-map(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-map-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                transport.Status = SliTransportState.Blocked;
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportMapping,
                    "transport-map",
                    "Transport mapping requires both source and target values.");
                context.AddTrace("transport-map(invalid)");
                return Task.FromResult(SExpression.AtomNode("transport-map-invalid"));
            }

            transport.MappedDifferences.Add(new SliTransportMappingState
            {
                Source = source,
                Target = target
            });
            context.AddTrace($"transport-map({source}->{target})");
            return Task.FromResult(SExpression.AtomNode("transport-map"));
        });

        Register("transport-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "transport residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Transport.Residues,
                HigherOrderLocalityResidueKind.TransportResidue,
                "transport-residue",
                detail);
            context.AddTrace($"transport-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("transport-residue"));
        });

        Register("transport-status", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            var requestedStatus = UnwrapStringLiteral(Arg(expression, 1, SliTransportState.Candidate));

            if (!transport.IsConfigured)
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-status requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-status",
                    "Transport status changes require an active transport.");
                context.AddTrace("transport-status(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-status-incomplete"));
            }

            if (!IsRecognizedTransportStatus(requestedStatus))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add($"transport-status rejected invalid value '{requestedStatus}'.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportStatus,
                    "transport-status",
                    $"Rejected invalid transport status '{requestedStatus}'.");
                context.AddTrace("transport-status(invalid)");
                return Task.FromResult(SExpression.AtomNode("transport-status-invalid"));
            }

            if (string.Equals(requestedStatus, SliTransportState.Blocked, StringComparison.OrdinalIgnoreCase))
            {
                transport.Status = SliTransportState.Blocked;
                context.AddTrace("transport-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            if (string.Equals(requestedStatus, SliTransportState.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                transport.Status = SliTransportState.Candidate;
                context.AddTrace("transport-status(candidate)");
                return Task.FromResult(SExpression.AtomNode("transport-candidate"));
            }

            var witness = context.HigherOrderLocalityState.Witness;
            var missingRequiredInvariants = RequiredTransportInvariants()
                .Except(transport.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var illegalCarriedInvariants = transport.PreservedInvariants
                .Where(invariant => !witness.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            var disallowedMappings = transport.MappedDifferences
                .Where(mapping => !IsAllowedTransportMapping(mapping.Source, mapping.Target))
                .ToArray();

            var missingWitnessMappings = witness.DifferenceSet
                .Except(transport.MappedDifferences.Select(mapping => mapping.Source), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var blocked =
                HasBlockingWitnessResidues(witness) ||
                !string.Equals(witness.CandidacyStatus, SliWitnessState.MorphismCandidate, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) ||
                !string.Equals(transport.SourceLocalityHandle, witness.LeftLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(transport.TargetLocalityHandle, witness.RightLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                missingRequiredInvariants.Length > 0 ||
                illegalCarriedInvariants.Length > 0 ||
                disallowedMappings.Length > 0 ||
                missingWitnessMappings.Length > 0;

            if (blocked)
            {
                transport.Status = SliTransportState.Blocked;

                if (string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) || string.IsNullOrWhiteSpace(transport.TargetLocalityHandle))
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                        "transport-status",
                        "Transport completion requires explicit source and target localities.");
                }

                if (!string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) &&
                    !string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) &&
                    (!string.Equals(transport.SourceLocalityHandle, witness.LeftLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                     !string.Equals(transport.TargetLocalityHandle, witness.RightLocalityHandle, StringComparison.OrdinalIgnoreCase)))
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        "Transport source and target must match the active witness comparison pair.");
                }

                if (missingRequiredInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        $"Transport omitted required preserved invariants: {string.Join(", ", missingRequiredInvariants)}.");
                }

                if (illegalCarriedInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.InvalidTransportMapping,
                        "transport-status",
                        $"Transport attempted to carry invariants not preserved by witness: {string.Join(", ", illegalCarriedInvariants)}.");
                }

                if (disallowedMappings.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.InvalidTransportMapping,
                        "transport-status",
                        $"Transport attempted disallowed mappings: {string.Join(", ", disallowedMappings.Select(mapping => $"{mapping.Source}->{mapping.Target}"))}.");
                }

                if (missingWitnessMappings.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        $"Transport omitted witness differences: {string.Join(", ", missingWitnessMappings)}.");
                }

                context.AddTrace("transport-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            transport.Status = SliTransportState.Completed;
            context.AddTrace("transport-status(completed)");
            return Task.FromResult(SExpression.AtomNode("transport-completed"));
        });
    }

    private void RegisterBoundedAdmissibilityOperators()
    {
        Register("surface-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            surface.Reset();

            var transportToken = UnwrapStringLiteral(Arg(expression, 1, "transport-state"));
            if (!TryResolveAdmissibleTransportHandle(state, transportToken, out var transportHandle))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-begin requires a resolved completed transport handle.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                    "surface-begin",
                    $"Admissible surface rejected unresolved transport reference '{transportToken}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-begin",
                    "Admissible surface remained incomplete because no completed transport handle was available.");
                context.AddTrace("surface-begin(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("surface-begin-invalid"));
            }

            var missingPrerequisites =
                !transport.IsConfigured ||
                !string.Equals(transport.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) ||
                HasBlockingTransportResidues(transport);

            if (missingPrerequisites)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-begin requires completed lawful transport with intact preserved invariants.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites,
                    "surface-begin",
                    "Admissible surface formation requires completed transport with explicit source and target localities.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.BlockedAdmissibleSurface,
                    "surface-begin",
                    "Admissible surface formation was blocked because transport had not completed lawfully.");
                context.AddTrace("surface-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-begin-blocked"));
            }

            surface.Configure(transportHandle, transport.SourceLocalityHandle, transport.TargetLocalityHandle);
            context.AddTrace($"surface-begin({transportToken})");
            return Task.FromResult(SExpression.AtomNode("surface-begin"));
        });

        Register("surface-source", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-source requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-source",
                    "Admissible surface source requires an active surface.");
                context.AddTrace("surface-source(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-source-incomplete"));
            }

            var transportToken = UnwrapStringLiteral(Arg(expression, 1, "transport-state"));
            if (!TryResolveAdmissibleTransportHandle(state, transportToken, out var transportHandle) ||
                !string.Equals(transport.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(surface.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-source rejected an unresolved or mismatched transport reference.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                    "surface-source",
                    $"Admissible surface rejected unresolved transport reference '{transportToken}'.");
                context.AddTrace("surface-source(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("surface-source-invalid"));
            }

            surface.Configure(transportHandle, transport.SourceLocalityHandle, transport.TargetLocalityHandle);
            context.AddTrace($"surface-source({transportToken})");
            return Task.FromResult(SExpression.AtomNode("surface-source"));
        });

        Register("surface-class", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-class requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-class",
                    "Admissible surface classification requires an active surface.");
                context.AddTrace("surface-class(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-class-incomplete"));
            }

            var requestedClass = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var identityApplicability = UnwrapStringLiteral(Arg(expression, 2, string.Empty));

            if (!IsRecognizedSurfaceClass(requestedClass))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-class rejected invalid value '{requestedClass}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceClass,
                    "surface-class",
                    $"Rejected invalid admissible surface class '{requestedClass}'.");
                context.AddTrace("surface-class(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-class-invalid"));
            }

            if (!IsRecognizedIdentityApplicability(identityApplicability))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-class rejected invalid identity applicability '{identityApplicability}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability,
                    "surface-class",
                    $"Rejected invalid identity-bearing applicability '{identityApplicability}'.");
                context.AddTrace("surface-class(invalid-identity-applicability)");
                return Task.FromResult(SExpression.AtomNode("surface-class-invalid"));
            }

            surface.SurfaceClass = requestedClass.ToLowerInvariant();
            surface.IdentityBearingApplicable = string.Equals(
                identityApplicability,
                SliAdmissibleSurfaceState.IdentityApplicable,
                StringComparison.OrdinalIgnoreCase);
            surface.HasIdentityBearingApplicability = true;
            context.AddTrace($"surface-class({requestedClass}:{identityApplicability})");
            return Task.FromResult(SExpression.AtomNode("surface-class"));
        });

        Register("surface-reveal", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-reveal requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-reveal",
                    "Admissible surface reveal posture requires an active surface.");
                context.AddTrace("surface-reveal(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-reveal-incomplete"));
            }

            var requestedReveal = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.MaskedRevealPosture));
            if (!string.Equals(requestedReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(requestedReveal, "narrow", StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.RevealPosture = ResolveSafeValue(surface.RevealPosture, SliHigherOrderLocalityState.MaskedRevealPosture);
                surface.Warnings.Add($"surface-reveal rejected invalid value '{requestedReveal}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReveal,
                    "surface-reveal",
                    $"Rejected invalid admissible surface reveal posture '{requestedReveal}'.");
                context.AddTrace("surface-reveal(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-reveal-invalid"));
            }

            surface.RevealPosture = requestedReveal.ToLowerInvariant();
            context.AddTrace($"surface-reveal({requestedReveal})");
            return Task.FromResult(SExpression.AtomNode("surface-reveal"));
        });

        Register("surface-boundary", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-boundary requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-boundary",
                    "Admissible surface boundary requires an active surface.");
                context.AddTrace("surface-boundary(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-boundary-incomplete"));
            }

            var requestedBoundary = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.BoundedSealPosture));
            if (!HasSafeSealPosture(requestedBoundary))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Boundary = ResolveSafeValue(surface.Boundary, SliHigherOrderLocalityState.BoundedSealPosture);
                surface.Warnings.Add($"surface-boundary rejected invalid value '{requestedBoundary}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceBoundary,
                    "surface-boundary",
                    $"Rejected invalid admissible surface boundary '{requestedBoundary}'.");
                context.AddTrace("surface-boundary(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-boundary-invalid"));
            }

            surface.Boundary = requestedBoundary.ToLowerInvariant();
            context.AddTrace($"surface-boundary({requestedBoundary})");
            return Task.FromResult(SExpression.AtomNode("surface-boundary"));
        });

        Register("surface-evidence", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-evidence requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-evidence",
                    "Admissible surface evidence requires an active surface.");
                context.AddTrace("surface-evidence(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-evidence-incomplete"));
            }

            var evidence = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(evidence) &&
                !surface.EvidenceSet.Contains(evidence, StringComparer.OrdinalIgnoreCase))
            {
                surface.EvidenceSet.Add(evidence);
                context.AddTrace($"surface-evidence({evidence})");
            }

            return Task.FromResult(SExpression.AtomNode("surface-evidence"));
        });

        Register("surface-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "admissible surface residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.AdmissibleSurface.Residues,
                HigherOrderLocalityResidueKind.AdmissibleSurfaceResidue,
                "surface-residue",
                detail);
            context.AddTrace($"surface-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("surface-residue"));
        });

        Register("surface-status", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            var requestedStatus = UnwrapStringLiteral(Arg(expression, 1, SliAdmissibleSurfaceState.Candidate));

            if (!surface.IsConfigured)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-status requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-status",
                    "Admissible surface status changes require an active surface.");
                context.AddTrace("surface-status(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-status-incomplete"));
            }

            if (!IsRecognizedSurfaceStatus(requestedStatus))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-status rejected invalid value '{requestedStatus}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceStatus,
                    "surface-status",
                    $"Rejected invalid admissible surface status '{requestedStatus}'.");
                context.AddTrace("surface-status(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-status-invalid"));
            }

            if (string.Equals(requestedStatus, SliAdmissibleSurfaceState.Blocked, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                context.AddTrace("surface-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-blocked"));
            }

            if (string.Equals(requestedStatus, SliAdmissibleSurfaceState.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Candidate;
                context.AddTrace("surface-status(candidate)");
                return Task.FromResult(SExpression.AtomNode("surface-candidate"));
            }

            var missingTransportInvariants = RequiredTransportInvariants()
                .Except(transport.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var revealWouldWiden = WouldWidenReveal(state.RevealPosture, surface.RevealPosture);
            var boundaryWouldWiden = WouldWidenBoundary(state.SealPosture, surface.Boundary);
            var identityApplicabilityConflict =
                string.Equals(surface.SurfaceClass, SliAdmissibleSurfaceState.IdentityBearingClass, StringComparison.OrdinalIgnoreCase) &&
                !surface.IdentityBearingApplicable;

            var blocked =
                !transport.IsConfigured ||
                !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(surface.TransportHandle) ||
                !string.Equals(surface.TransportHandle, transport.TransportHandle, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(surface.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(surface.TargetLocalityHandle) ||
                string.IsNullOrWhiteSpace(surface.SurfaceClass) ||
                !surface.HasIdentityBearingApplicability ||
                surface.EvidenceSet.Count == 0 ||
                HasBlockingTransportResidues(transport) ||
                missingTransportInvariants.Length > 0 ||
                revealWouldWiden ||
                boundaryWouldWiden ||
                identityApplicabilityConflict;

            if (blocked)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;

                if (!transport.IsConfigured ||
                    !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                    surface.EvidenceSet.Count == 0 ||
                    string.IsNullOrWhiteSpace(surface.SurfaceClass) ||
                    !surface.HasIdentityBearingApplicability ||
                    missingTransportInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites,
                        "surface-status",
                        $"Admissible surface formation requires completed transport, explicit classing, explicit identity applicability, evidence carriage, and preserved invariants. Missing invariants: {string.Join(", ", missingTransportInvariants)}.");
                }

                if (string.IsNullOrWhiteSpace(surface.TransportHandle) ||
                    !string.Equals(surface.TransportHandle, transport.TransportHandle, StringComparison.OrdinalIgnoreCase))
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                        "surface-status",
                        "Admissible surface formation requires an active transport reference that matches the completed transport lane.");
                }

                if (revealWouldWiden)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceReveal,
                        "surface-status",
                        $"Admissible surface reveal '{surface.RevealPosture}' would widen locality reveal '{state.RevealPosture}'.");
                }

                if (boundaryWouldWiden)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceBoundary,
                        "surface-status",
                        $"Admissible surface boundary '{surface.Boundary}' would widen locality seal posture '{state.SealPosture}'.");
                }

                if (identityApplicabilityConflict)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability,
                        "surface-status",
                        "Identity-bearing surface class requires explicit identity applicability.");
                }

                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.BlockedAdmissibleSurface,
                    "surface-status",
                    "Admissible surface remained blocked because the carried structure could not yet be lawfully shaped for inspection.");
                context.AddTrace("surface-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-blocked"));
            }

            surface.Status = SliAdmissibleSurfaceState.Formed;
            context.AddTrace("surface-status(formed)");
            return Task.FromResult(SExpression.AtomNode("surface-formed"));
        });
    }

    private static void AddResidue(
        SliExecutionContext context,
        ICollection<HigherOrderLocalityResidue> scopedResidues,
        HigherOrderLocalityResidueKind kind,
        string source,
        string detail)
    {
        var residue = new HigherOrderLocalityResidue
        {
            Kind = kind,
            Source = source,
            Detail = detail
        };

        context.HigherOrderLocalityState.AddResidue(residue);
        if (!ReferenceEquals(scopedResidues, context.HigherOrderLocalityState.Residues))
        {
            scopedResidues.Add(residue);
        }
    }

    private static string ResolveSafeValue(string currentValue, string safeDefault)
    {
        return string.IsNullOrWhiteSpace(currentValue)
            ? safeDefault
            : currentValue;
    }

    private static bool HasSafeSealPosture(string value)
    {
        return string.Equals(value, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "sealed", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRehearsalMode(string value)
    {
        if (string.Equals(value, SliRehearsalState.DreamGameMode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "dream", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "game", StringComparison.OrdinalIgnoreCase))
        {
            return value.ToLowerInvariant();
        }

        return SliRehearsalState.DreamGameMode;
    }

    private static IReadOnlyList<string> RequiredWitnessInvariants()
    {
        return
        [
            "self-anchor-polarity",
            "other-anchor-polarity",
            "relation-anchor-polarity",
            "seal-posture-bound",
            "reveal-posture-bound",
            "participation-mode-limit",
            "identity-nonbinding"
        ];
    }

    private static IReadOnlyList<string> AllowedWitnessDifferences()
    {
        return
        [
            "rehearsal-branch",
            "substitution",
            "analogy",
            "branch-context"
        ];
    }

    private static IReadOnlyList<string> RequiredTransportInvariants()
    {
        return
        [
            "self-anchor-polarity",
            "other-anchor-polarity",
            "relation-anchor-polarity",
            "seal-posture-bound",
            "reveal-posture-bound",
            "participation-mode-limit",
            "identity-nonbinding"
        ];
    }

    private static bool IsAllowedTransportMapping(string source, string target)
    {
        return
            (string.Equals(source, "rehearsal-branch", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "branch-variant", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "substitution", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "substitution", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "analogy", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "analogy", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "branch-context", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "branch-context", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRecognizedTransportStatus(string value)
    {
        return string.Equals(value, SliTransportState.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliTransportState.Candidate, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedSurfaceClass(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.InformationalClass, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.ComparativeClass, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.IdentityBearingClass, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedIdentityApplicability(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.InformationalOnly, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.IdentityApplicable, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedSurfaceStatus(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.Candidate, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.Formed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool WouldWidenReveal(string localityReveal, string surfaceReveal)
    {
        return string.Equals(localityReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(surfaceReveal, "narrow", StringComparison.OrdinalIgnoreCase);
    }

    private static bool WouldWidenBoundary(string localityBoundary, string surfaceBoundary)
    {
        return string.Equals(localityBoundary, "sealed", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(surfaceBoundary, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasBlockingWitnessResidues(SliWitnessState witness)
    {
        return witness.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingWitnessPrerequisites or
                HigherOrderLocalityResidueKind.InvalidWitnessReference or
                HigherOrderLocalityResidueKind.InvalidGlueThreshold or
                HigherOrderLocalityResidueKind.IncompleteWitness or
                HigherOrderLocalityResidueKind.NonCandidateWitness);
    }

    private static bool HasBlockingTransportResidues(SliTransportState transport)
    {
        return transport.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingTransportPrerequisites or
                HigherOrderLocalityResidueKind.InvalidTransportReference or
                HigherOrderLocalityResidueKind.InvalidTransportStatus or
                HigherOrderLocalityResidueKind.InvalidTransportMapping or
                HigherOrderLocalityResidueKind.IncompleteTransport or
                HigherOrderLocalityResidueKind.BlockedTransport);
    }

    private static bool TryResolveWitnessReference(SliExecutionContext context, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        var state = context.HigherOrderLocalityState;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (string.Equals(token, "locality-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.LocalityHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.LocalityHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        if (!state.Rehearsal.IsConfigured)
        {
            return false;
        }

        if (state.Rehearsal.BranchSet.Contains(token, StringComparer.OrdinalIgnoreCase))
        {
            resolvedHandle = $"{state.Rehearsal.RehearsalHandle}:branch:{token}";
            return true;
        }

        if (token.StartsWith($"{state.Rehearsal.RehearsalHandle}:branch:", StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = token;
            return true;
        }

        return false;
    }

    private static bool TryResolveTransportWitnessHandle(SliHigherOrderLocalityState state, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !state.Witness.IsConfigured)
        {
            return false;
        }

        if (string.Equals(token, "witness-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.Witness.WitnessHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.Witness.WitnessHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        return false;
    }

    private static bool TryResolveAdmissibleTransportHandle(SliHigherOrderLocalityState state, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !state.Transport.IsConfigured)
        {
            return false;
        }

        if (string.Equals(token, "transport-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.Transport.TransportHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.Transport.TransportHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        return false;
    }

    private static string Arg(SExpression expression, int index, string fallback)
    {
        if (expression.IsAtom || expression.Children.Count <= index)
        {
            return fallback;
        }

        return expression.Children[index].Atom ?? fallback;
    }

    private static string UnwrapStringLiteral(string value)
    {
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            return value[1..^1];
        }

        return value;
    }

    private static SoulFrameInferenceRequest BuildInferenceRequest(
        string task,
        string contextValue,
        SliExecutionContext context)
    {
        return new SoulFrameInferenceRequest
        {
            Task = task,
            Context = contextValue,
            OpalConstraints = context.OpalConstraints,
            SoulFrameId = context.Frame.SoulFrameId,
            ContextId = context.Frame.ContextId
        };
    }
}
