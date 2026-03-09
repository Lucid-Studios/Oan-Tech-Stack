using GEL.Graphs;
using SLI.Engine.Models;
using SoulFrame.Host;

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
