using System.Text.RegularExpressions;
using GEL.Graphs;
using GEL.Models;

namespace SLI.Ingestion;

public enum SliFragmentRole
{
    Origin = 0,
    Bridge = 1,
    Inversion = 2,
    Leap = 3,
    Reflection = 4
}

public enum SliDiagnosticOperator
{
    Delta = 0,
    Reflect = 1,
    Rebind = 2,
    Bloom = 3,
    Fix = 4
}

public enum SliLayerKind
{
    Text = 0,
    Basic = 1,
    Intermediate = 2,
    Advanced = 3,
    Master = 4
}

public enum SliFragmentGateOutcome
{
    Pass = 0,
    Review = 1
}

public sealed record SliFragmentDiagnosticNode(
    string NodeId,
    string Label,
    SliFragmentRole Role,
    SliLayerKind Layer,
    bool Critical,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<SliDiagnosticOperator> Operators,
    IReadOnlyList<string> Citations);

public sealed record SliFragmentMetricSet(
    double Dpi,
    double Rtd,
    double EpsilonFunctor,
    double Coverage,
    double FalseInclude,
    double Sfi);

public sealed record SliFragmentGateStatus(
    bool GraphAcyclic,
    SliFragmentGateOutcome Structure,
    SliFragmentGateOutcome Meaning,
    SliFragmentGateOutcome Functor,
    SliFragmentGateOutcome Trace);

public sealed record SliStressVariantResult(
    string VariantKey,
    string Description,
    IReadOnlyList<SliFragmentDiagnosticNode> Nodes,
    SliFragmentMetricSet Metrics,
    SliFragmentGateStatus Gates);

public sealed record SliFragmentDiagnosticResult(
    string FragmentId,
    string DraftRootKey,
    IReadOnlyList<string> ConstructorRootReferences,
    IReadOnlyList<SliFragmentDiagnosticNode> Nodes,
    SliFragmentMetricSet Metrics,
    SliFragmentGateStatus Gates,
    string WitnessSummary,
    IReadOnlyList<SliStressVariantResult> StressVariants);

internal static partial class SliFragmentDiagnosticBuilder
{
    private static readonly IReadOnlySet<string> DirectiveTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "analyze",
        "calculate",
        "compute",
        "for",
        "solve"
    };

    public static SliFragmentDiagnosticResult Build(
        CleavedOntology cleavedOntology,
        ConstructorEngramRecord constructor,
        ConstructorGraph constructorGraph,
        EngramDraft draft,
        SliExpression expression,
        IReadOnlyList<EngramCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(cleavedOntology);
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(constructorGraph);
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(candidates);

        var variantInput = DiagnosticVariantInput.Create(
            constructor,
            draft,
            expression,
            candidates,
            extraDecodedTerms: Array.Empty<string>(),
            extraCitedTerms: Array.Empty<string>());

        var witnessSummary = BuildWitnessSummary(variantInput);
        var baseline = EvaluateVariant(cleavedOntology, constructorGraph, variantInput, witnessSummary);
        var stressVariants = BuildStressVariants(cleavedOntology, constructorGraph, constructor, draft, expression, candidates);

        return new SliFragmentDiagnosticResult(
            FragmentId: draft.ProposedId?.Value ?? draft.RootKey,
            DraftRootKey: draft.RootKey,
            ConstructorRootReferences: constructor.RootReferences.ToArray(),
            Nodes: baseline.Nodes,
            Metrics: baseline.Metrics,
            Gates: baseline.Gates,
            WitnessSummary: witnessSummary,
            StressVariants: stressVariants);
    }

    private static IReadOnlyList<SliStressVariantResult> BuildStressVariants(
        CleavedOntology cleavedOntology,
        ConstructorGraph constructorGraph,
        ConstructorEngramRecord constructor,
        EngramDraft draft,
        SliExpression expression,
        IReadOnlyList<EngramCandidate> candidates)
    {
        var results = new List<SliStressVariantResult>(3);

        var reorderedRoots = new List<string> { draft.RootKey };
        reorderedRoots.AddRange(
            draft.Branches
                .Select(branch => branch.RootKey)
                .Where(static root => !string.IsNullOrWhiteSpace(root))
                .Reverse()
                .Cast<string>());
        results.Add(BuildVariant(
            "reorder_dependencies",
            "Reverse branch/root dependency ordering while preserving the same root set.",
            cleavedOntology,
            constructorGraph,
            DiagnosticVariantInput.Create(
                constructor,
                draft,
                expression,
                candidates,
                extraDecodedTerms: Array.Empty<string>(),
                extraCitedTerms: Array.Empty<string>(),
                draftRootsOverride: reorderedRoots),
            includeBranchLabelsInWitness: true));

        var branchRoots = draft.Branches
            .Select(branch => branch.RootKey)
            .Where(static root => !string.IsNullOrWhiteSpace(root))
            .Cast<string>()
            .ToList();
        var elidedRoots = new List<string> { draft.RootKey };
        elidedRoots.AddRange(branchRoots.Skip(1));
        results.Add(BuildVariant(
            "elide_noncritical_bridge",
            "Remove the first non-critical bridge root from the witness projection.",
            cleavedOntology,
            constructorGraph,
            DiagnosticVariantInput.Create(
                constructor,
                draft,
                expression,
                candidates,
                extraDecodedTerms: Array.Empty<string>(),
                extraCitedTerms: Array.Empty<string>(),
                draftRootsOverride: branchRoots.Count > 0 ? elidedRoots : [draft.RootKey]),
            includeBranchLabelsInWitness: false));

        var contradictionToken = candidates.FirstOrDefault()?.Token ?? "contradiction-signal";
        results.Add(BuildVariant(
            "inject_unresolved_contradiction",
            "Inject one unresolved contradiction token into decoded and cited witness terms.",
            cleavedOntology,
            constructorGraph,
            DiagnosticVariantInput.Create(
                constructor,
                draft,
                expression,
                candidates,
                extraDecodedTerms: [contradictionToken],
                extraCitedTerms: [contradictionToken]),
            includeBranchLabelsInWitness: true));

        return results;
    }

    private static SliStressVariantResult BuildVariant(
        string variantKey,
        string description,
        CleavedOntology cleavedOntology,
        ConstructorGraph constructorGraph,
        DiagnosticVariantInput input,
        bool includeBranchLabelsInWitness)
    {
        var witnessSummary = BuildWitnessSummary(input, includeBranchLabelsInWitness);
        var evaluation = EvaluateVariant(cleavedOntology, constructorGraph, input, witnessSummary);
        return new SliStressVariantResult(
            VariantKey: variantKey,
            Description: description,
            Nodes: evaluation.Nodes,
            Metrics: evaluation.Metrics,
            Gates: evaluation.Gates);
    }

    private static EvaluatedVariant EvaluateVariant(
        CleavedOntology cleavedOntology,
        ConstructorGraph constructorGraph,
        DiagnosticVariantInput input,
        string witnessSummary)
    {
        var citedTerms = BuildCitedTerms(input, witnessSummary);
        var nodes = BuildNodes(input, citedTerms, witnessSummary);
        var metrics = BuildMetrics(cleavedOntology, constructorGraph, input, nodes, citedTerms);
        var gates = BuildGates(constructorGraph, metrics);
        return new EvaluatedVariant(nodes, metrics, gates);
    }

    private static IReadOnlyList<SliFragmentDiagnosticNode> BuildNodes(
        DiagnosticVariantInput input,
        IReadOnlySet<string> citedTerms,
        string witnessSummary)
    {
        var nodes = new List<SliFragmentDiagnosticNode>();

        var originCitations = citedTerms.Contains(input.DraftRootKey)
            ? new[] { "program:root", "witness:summary" }
            : Array.Empty<string>();
        nodes.Add(new SliFragmentDiagnosticNode(
            NodeId: $"origin:{input.DraftRootKey}",
            Label: input.DraftRootKey,
            Role: SliFragmentRole.Origin,
            Layer: SliLayerKind.Basic,
            Critical: true,
            Dependencies: Array.Empty<string>(),
            Operators: [SliDiagnosticOperator.Delta],
            Citations: originCitations));

        var originId = nodes[0].NodeId;
        foreach (var branchRoot in input.DraftBranchRoots)
        {
            var citations = citedTerms.Contains(branchRoot)
                ? new[] { "witness:summary" }
                : Array.Empty<string>();
            nodes.Add(new SliFragmentDiagnosticNode(
                NodeId: $"bridge:{branchRoot}",
                Label: branchRoot,
                Role: SliFragmentRole.Bridge,
                Layer: SliLayerKind.Intermediate,
                Critical: false,
                Dependencies: [originId],
                Operators: [SliDiagnosticOperator.Rebind],
                Citations: citations));
        }

        if (input.HasDerivedTransformLine)
        {
            var dependencies = new List<string> { originId };
            dependencies.AddRange(nodes.Where(static node => node.Role == SliFragmentRole.Bridge).Select(static node => node.NodeId));
            nodes.Add(new SliFragmentDiagnosticNode(
                NodeId: "inversion:derived-transform",
                Label: "derived-transform",
                Role: SliFragmentRole.Inversion,
                Layer: SliLayerKind.Advanced,
                Critical: true,
                Dependencies: dependencies,
                Operators: [SliDiagnosticOperator.Reflect],
                Citations: ["program:derivation"]));
        }

        if (input.HasConclusionLine)
        {
            var dependencies = nodes.Where(static node => node.Role is SliFragmentRole.Inversion or SliFragmentRole.Origin)
                .Select(static node => node.NodeId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            nodes.Add(new SliFragmentDiagnosticNode(
                NodeId: "leap:conclusion",
                Label: "conclusion",
                Role: SliFragmentRole.Leap,
                Layer: SliLayerKind.Master,
                Critical: true,
                Dependencies: dependencies,
                Operators: [SliDiagnosticOperator.Bloom],
                Citations: ["program:conclusion"]));
        }

        nodes.Add(new SliFragmentDiagnosticNode(
            NodeId: "reflection:witness-summary",
            Label: "witness-summary",
            Role: SliFragmentRole.Reflection,
            Layer: SliLayerKind.Master,
            Critical: false,
            Dependencies: nodes.Where(static node => node.Role is SliFragmentRole.Origin or SliFragmentRole.Leap)
                .Select(static node => node.NodeId)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Operators: [SliDiagnosticOperator.Fix],
            Citations: string.IsNullOrWhiteSpace(witnessSummary) ? Array.Empty<string>() : ["witness:summary"]));

        return nodes;
    }

    private static SliFragmentMetricSet BuildMetrics(
        CleavedOntology cleavedOntology,
        ConstructorGraph constructorGraph,
        DiagnosticVariantInput input,
        IReadOnlyList<SliFragmentDiagnosticNode> nodes,
        IReadOnlySet<string> citedTerms)
    {
        var draftRoots = new HashSet<string>(input.DraftRoots, StringComparer.OrdinalIgnoreCase);

        var totalEdges = constructorGraph.Edges.Count;
        var preservedEdges = totalEdges == 0
            ? 0
            : constructorGraph.Edges.Count(edge =>
                draftRoots.Contains(edge.Source) &&
                draftRoots.Contains(edge.Target));
        var dpi = totalEdges == 0 ? 1d : Round((double)preservedEdges / totalEdges);

        var sourceTerminals = ExpandTerms(cleavedOntology.Tokens);
        var decodedInputs = input.DraftRoots
            .Concat(input.Expression.ProgramExpressions)
            .Concat(input.ExtraDecodedTerms);
        var decodedTerminals = ExpandTerms(decodedInputs);
        var rtd = 1d - Jaccard(sourceTerminals, decodedTerminals);

        var constructorRoots = new HashSet<string>(input.ConstructorRootReferences, StringComparer.OrdinalIgnoreCase);
        var draftRootSet = new HashSet<string>(input.DraftRoots, StringComparer.OrdinalIgnoreCase);
        var epsilonFunctor = NormalizedSymmetricDifference(constructorRoots, draftRootSet);

        var criticalNodes = nodes.Where(static node => node.Critical).ToArray();
        var coveredCriticalNodes = criticalNodes.Count(static node => node.Citations.Count > 0);
        var coverage = criticalNodes.Length == 0
            ? 1d
            : Round((double)coveredCriticalNodes / criticalNodes.Length);

        var candidateTokens = new HashSet<string>(input.CandidateTokens, StringComparer.OrdinalIgnoreCase);
        var falseIncludeCount = citedTerms.Count(term => candidateTokens.Contains(term));
        var falseInclude = citedTerms.Count == 0
            ? 0d
            : Round((double)falseIncludeCount / citedTerms.Count);

        var sfi = Round(Clamp01(
            (0.35 * dpi) +
            (0.25 * (1d - rtd)) +
            (0.20 * (1d - epsilonFunctor)) +
            (0.20 * coverage)));

        return new SliFragmentMetricSet(
            Dpi: Round(dpi),
            Rtd: Round(rtd),
            EpsilonFunctor: Round(epsilonFunctor),
            Coverage: Round(coverage),
            FalseInclude: Round(falseInclude),
            Sfi: sfi);
    }

    private static SliFragmentGateStatus BuildGates(
        ConstructorGraph constructorGraph,
        SliFragmentMetricSet metrics)
    {
        var graphAcyclic = IsAcyclic(constructorGraph);
        return new SliFragmentGateStatus(
            GraphAcyclic: graphAcyclic,
            Structure: graphAcyclic && metrics.Dpi >= 0.88d ? SliFragmentGateOutcome.Pass : SliFragmentGateOutcome.Review,
            Meaning: metrics.Rtd <= 0.12d ? SliFragmentGateOutcome.Pass : SliFragmentGateOutcome.Review,
            Functor: metrics.EpsilonFunctor <= 0.07d ? SliFragmentGateOutcome.Pass : SliFragmentGateOutcome.Review,
            Trace: metrics.Coverage >= 0.90d && metrics.FalseInclude <= 0.05d ? SliFragmentGateOutcome.Pass : SliFragmentGateOutcome.Review);
    }

    private static bool IsAcyclic(ConstructorGraph constructorGraph)
    {
        var adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var edge in constructorGraph.Edges)
        {
            if (!adjacency.TryGetValue(edge.Source, out var targets))
            {
                targets = new List<string>();
                adjacency[edge.Source] = targets;
            }

            targets.Add(edge.Target);

            if (!adjacency.ContainsKey(edge.Target))
            {
                adjacency[edge.Target] = new List<string>();
            }
        }

        var visitState = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in adjacency.Keys)
        {
            if (HasCycle(node, adjacency, visitState))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasCycle(
        string node,
        IReadOnlyDictionary<string, List<string>> adjacency,
        IDictionary<string, int> visitState)
    {
        if (visitState.TryGetValue(node, out var existingState))
        {
            return existingState == 1;
        }

        visitState[node] = 1;
        foreach (var target in adjacency[node])
        {
            if (HasCycle(target, adjacency, visitState))
            {
                return true;
            }
        }

        visitState[node] = 2;
        return false;
    }

    private static IReadOnlySet<string> BuildCitedTerms(
        DiagnosticVariantInput input,
        string witnessSummary)
    {
        return ExpandTerms(
            input.Expression.ProgramExpressions
                .Concat([witnessSummary])
                .Concat(input.ExtraCitedTerms));
    }

    private static string BuildWitnessSummary(
        DiagnosticVariantInput input,
        bool includeBranchLabels = true)
    {
        var branches = includeBranchLabels && input.DraftBranchRoots.Count > 0
            ? string.Join(",", input.DraftBranchRoots)
            : "withheld";
        var candidates = input.CandidateTokens.Count == 0
            ? "none"
            : string.Join(",", input.CandidateTokens);

        return $"root={input.DraftRootKey}; branches={branches}; candidates={candidates}; layers=text,basic,intermediate,advanced,master";
    }

    private static IReadOnlySet<string> ExpandTerms(IEnumerable<string> values)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            foreach (Match match in DiagnosticLexemeRegex().Matches(value))
            {
                var normalized = NormalizeTerm(match.Value);
                if (string.IsNullOrWhiteSpace(normalized) || DirectiveTerms.Contains(normalized))
                {
                    continue;
                }

                terms.Add(normalized);
            }
        }

        return terms;
    }

    private static string NormalizeTerm(string value)
    {
        var token = value.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        if (SingleVariableRegex().IsMatch(token) || CombinedVariableRegex().IsMatch(token))
        {
            return "variable";
        }

        return token switch
        {
            "+" => "addition",
            "-" => "subtraction",
            "*" => "multiplication",
            "/" => "division",
            "=" => "equation",
            _ => token.ToLowerInvariant()
        };
    }

    private static double Jaccard(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        if (left.Count == 0 && right.Count == 0)
        {
            return 1d;
        }

        var intersection = left.Count(right.Contains);
        var union = left.Count + right.Count - intersection;
        return union == 0 ? 1d : Round((double)intersection / union);
    }

    private static double NormalizedSymmetricDifference(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        var union = new HashSet<string>(left, StringComparer.OrdinalIgnoreCase);
        union.UnionWith(right);
        if (union.Count == 0)
        {
            return 0d;
        }

        var symmetricDifference = new HashSet<string>(left, StringComparer.OrdinalIgnoreCase);
        symmetricDifference.SymmetricExceptWith(right);
        return Round((double)symmetricDifference.Count / union.Count);
    }

    private static double Clamp01(double value) => Math.Max(0d, Math.Min(1d, value));

    private static double Round(double value) => Math.Round(value, 6, MidpointRounding.AwayFromZero);

    [GeneratedRegex(@"[A-Za-z][A-Za-z\-']*|[-+]?\d+[A-Za-z]|[-+]?\d+|[+\-*/=]", RegexOptions.Compiled)]
    private static partial Regex DiagnosticLexemeRegex();

    [GeneratedRegex(@"^[A-Za-z]$", RegexOptions.Compiled)]
    private static partial Regex SingleVariableRegex();

    [GeneratedRegex(@"^[-+]?\d+[A-Za-z]$", RegexOptions.Compiled)]
    private static partial Regex CombinedVariableRegex();

    private sealed record DiagnosticVariantInput(
        string DraftRootKey,
        IReadOnlyList<string> ConstructorRootReferences,
        IReadOnlyList<string> DraftRoots,
        IReadOnlyList<string> DraftBranchRoots,
        IReadOnlyList<string> CandidateTokens,
        SliExpression Expression,
        bool HasDerivedTransformLine,
        bool HasConclusionLine,
        IReadOnlyList<string> ExtraDecodedTerms,
        IReadOnlyList<string> ExtraCitedTerms)
    {
        public static DiagnosticVariantInput Create(
            ConstructorEngramRecord constructor,
            EngramDraft draft,
            SliExpression expression,
            IReadOnlyList<EngramCandidate> candidates,
            IReadOnlyList<string> extraDecodedTerms,
            IReadOnlyList<string> extraCitedTerms,
            IReadOnlyList<string>? draftRootsOverride = null)
        {
            var actualBranchRoots = draft.Branches
                .Select(branch => branch.RootKey)
                .Where(static root => !string.IsNullOrWhiteSpace(root))
                .Cast<string>()
                .ToArray();
            var draftRoots = draftRootsOverride ?? [draft.RootKey, .. actualBranchRoots];
            var branchRoots = draftRoots.Skip(1).ToArray();

            return new DiagnosticVariantInput(
                DraftRootKey: draft.RootKey,
                ConstructorRootReferences: constructor.RootReferences.ToArray(),
                DraftRoots: draftRoots,
                DraftBranchRoots: branchRoots,
                CandidateTokens: candidates.Select(candidate => candidate.Token).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                Expression: expression,
                HasDerivedTransformLine: expression.ProgramExpressions.Count > 1,
                HasConclusionLine: expression.ProgramExpressions.Count > 2,
                ExtraDecodedTerms: extraDecodedTerms,
                ExtraCitedTerms: extraCitedTerms);
        }
    }

    private sealed record EvaluatedVariant(
        IReadOnlyList<SliFragmentDiagnosticNode> Nodes,
        SliFragmentMetricSet Metrics,
        SliFragmentGateStatus Gates);
}
