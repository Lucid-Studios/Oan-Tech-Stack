using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using SLI.Engine.Cognition;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class SliIngestionTests
{
    [Fact]
    public async Task Ingestion_TransformsLinearEquation_ToCanonicalSliStructure()
    {
        var engine = new SliIngestionEngine();

        var result = await engine.IngestAsync("Solve for x: 3x + 7 = 25");

        Assert.Contains(
            result.SliExpression.ProgramExpressions,
            expression => expression.Contains("(=", StringComparison.Ordinal) &&
                          expression.Contains("3", StringComparison.Ordinal) &&
                          expression.Contains("25", StringComparison.Ordinal));
        Assert.Contains("(= x 6)", result.SliExpression.SymbolTree, StringComparison.Ordinal);
        Assert.NotEmpty(result.MatchResult.KnownEngrams);
        Assert.Single(result.CanonicalDrafts);
        Assert.Equal("equation", result.CanonicalDrafts[0].RootKey);
        Assert.Equal("equation", result.Diagnostic.DraftRootKey);
        Assert.Equal(7, result.Diagnostic.Nodes.Count);
        Assert.Equal(SliFragmentGateOutcome.Pass, result.Diagnostic.Gates.Structure);
        Assert.Equal(SliFragmentGateOutcome.Review, result.Diagnostic.Gates.Meaning);
        Assert.Equal(SliFragmentGateOutcome.Pass, result.Diagnostic.Gates.Functor);
        Assert.Equal(SliFragmentGateOutcome.Pass, result.Diagnostic.Gates.Trace);
        Assert.Equal(3, result.Diagnostic.StressVariants.Count);
    }

    [Fact]
    public async Task Ingestion_DetectsUnknownTokens_AsEngramCandidates()
    {
        var engine = new SliIngestionEngine();

        var result = await engine.IngestAsync("Analyze hypernumeration resonance field");

        Assert.Contains(
            result.MatchResult.EngramCandidates,
            candidate => string.Equals(candidate.Token, "hypernumeration", StringComparison.OrdinalIgnoreCase));
        Assert.Single(result.CanonicalDrafts);
        Assert.Equal(result.CanonicalDrafts[0].RootKey, result.Diagnostic.DraftRootKey);
        Assert.Contains(result.Diagnostic.Nodes, node => node.Role == SliFragmentRole.Origin);
        Assert.Contains(result.Diagnostic.Nodes, node => node.Role == SliFragmentRole.Reflection);
        Assert.Equal(SliFragmentGateOutcome.Review, result.Diagnostic.Gates.Meaning);
        Assert.Equal(SliFragmentGateOutcome.Pass, result.Diagnostic.Gates.Functor);
        Assert.Equal(SliFragmentGateOutcome.Review, result.Diagnostic.Gates.Trace);
    }

    [Fact]
    public async Task IngestionOutput_IsConsumableBySliCognitionEngine()
    {
        var resolver = new EngramResolverService();
        var ingestionEngine = new SliIngestionEngine();
        var ingestion = await ingestionEngine.IngestAsync("Solve 3x + 7 = 25");

        var cognition = new SliCognitionEngine(resolver);
        await cognition.InitializeAsync();

        var request = new CognitionRequest
        {
            Context = new CognitionContext
            {
                CMEId = "cme-ingestion",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "solve equation",
                RelevantEngrams = [],
                SymbolicProgram = ingestion.SliExpression.ProgramExpressions
            },
            Prompt = "execute symbolic cognition"
        };

        var result = await cognition.ExecuteAsync(request);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
        Assert.NotEmpty(result.SymbolicTrace);
    }

    [Fact]
    public async Task Ingestion_DiagnosticSidecar_IsDeterministicForEquationFixture()
    {
        var engine = new SliIngestionEngine();

        var first = await engine.IngestAsync("Solve for x: 3x + 7 = 25");
        var second = await engine.IngestAsync("Solve for x: 3x + 7 = 25");

        Assert.Equal(first.CanonicalDrafts[0].RootKey, second.CanonicalDrafts[0].RootKey);
        Assert.Equal(first.Diagnostic.Metrics, second.Diagnostic.Metrics);
        Assert.Equal(first.Diagnostic.Gates, second.Diagnostic.Gates);
        Assert.Equal(
            first.Diagnostic.Nodes.Select(node => (node.NodeId, node.Role, node.Critical)),
            second.Diagnostic.Nodes.Select(node => (node.NodeId, node.Role, node.Critical)));
        Assert.Equal(
            first.Diagnostic.StressVariants.Select(variant => (variant.VariantKey, variant.Metrics, variant.Gates)),
            second.Diagnostic.StressVariants.Select(variant => (variant.VariantKey, variant.Metrics, variant.Gates)));
    }

    [Fact]
    public async Task Ingestion_DiagnosticStressVariants_RecomputeWithoutMutatingBaseline()
    {
        var engine = new SliIngestionEngine();

        var result = await engine.IngestAsync("Analyze hypernumeration resonance field");

        var baselineNodeCount = result.Diagnostic.Nodes.Count;
        Assert.Equal(
            ["reorder_dependencies", "elide_noncritical_bridge", "inject_unresolved_contradiction"],
            result.Diagnostic.StressVariants.Select(variant => variant.VariantKey).ToArray());

        var reordered = result.Diagnostic.StressVariants[0];
        var elided = result.Diagnostic.StressVariants[1];
        var contradiction = result.Diagnostic.StressVariants[2];

        Assert.Equal(result.Diagnostic.Metrics.Dpi, reordered.Metrics.Dpi);
        Assert.Equal(result.Diagnostic.Metrics.EpsilonFunctor, reordered.Metrics.EpsilonFunctor);
        Assert.True(elided.Nodes.Count <= baselineNodeCount);
        Assert.True(elided.Metrics.EpsilonFunctor >= result.Diagnostic.Metrics.EpsilonFunctor);
        Assert.True(contradiction.Metrics.FalseInclude >= result.Diagnostic.Metrics.FalseInclude);
        Assert.Equal(baselineNodeCount, result.Diagnostic.Nodes.Count);
    }
}
