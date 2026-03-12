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
