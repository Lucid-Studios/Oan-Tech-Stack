using System.Reflection;
using System.Text.Json.Serialization;
using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using SLI.Engine.Models;
using SLI.Engine.Parser;
using SLI.Engine.Runtime;
using SLI.Engine.Telemetry;
using SLI.Lisp;

namespace Oan.Sli.Tests;

public sealed class HigherOrderLocalityInvariantTests
{
    [Fact]
    public async Task LocalityPrograms_DoNotMutateMorphologyPropositionOrDecisionSurfaces()
    {
        var context = await ExecuteContextAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(seal-posture permeable)",
                "(participation-mode improvise)"
            ]);

        Assert.Empty(context.MorphologyState.ResolvedLemmaRoots);
        Assert.Empty(context.MorphologyState.OperatorAnnotations);
        Assert.Empty(context.MorphologyState.ConstructorBodies);
        Assert.Empty(context.MorphologyState.GraphEdges);
        Assert.Empty(context.MorphologyState.ContinuityAnchors);
        Assert.Empty(context.MorphologyState.BodyInvariants);
        Assert.Empty(context.MorphologyState.ClusterEntries);
        Assert.Equal(string.Empty, context.MorphologyState.DiagnosticPredicateRender);
        Assert.Null(context.MorphologyState.PredicateRoot);
        Assert.Equal(string.Empty, context.MorphologyState.Summary);
        Assert.Null(context.MorphologyState.ScalarPayload);
        Assert.Equal(string.Empty, context.MorphologyState.BodySummary);
        Assert.Equal("OutOfScope", context.MorphologyState.Outcome);

        Assert.Equal(string.Empty, context.PropositionState.Subject.RootKey);
        Assert.Equal(string.Empty, context.PropositionState.Subject.SymbolicHandle);
        Assert.Equal(string.Empty, context.PropositionState.PredicateRoot);
        Assert.Equal(string.Empty, context.PropositionState.Object.RootKey);
        Assert.Equal(string.Empty, context.PropositionState.Object.SymbolicHandle);
        Assert.Empty(context.PropositionState.Qualifiers);
        Assert.Empty(context.PropositionState.ContextTags);
        Assert.Equal(string.Empty, context.PropositionState.DiagnosticRender);
        Assert.Empty(context.PropositionState.UnresolvedTensions);
        Assert.Equal("NeedsSpecification", context.PropositionState.Grade);

        Assert.Equal("defer", context.FinalDecision);
        Assert.Empty(context.CandidateBranches);
        Assert.Empty(context.PrunedBranches);
    }

    [Fact]
    public void CompositeForms_ExpandOnlyToBoundedLocalityOps()
    {
        var parser = new SliParser();
        var expander = new SliLocalityCompositionExpander(LispModuleCatalog.LoadModules());
        var program = parser.ParseProgram(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)"
            ]);

        var expanded = expander.ExpandProgram(program);
        var ops = expanded
            .Select(expression => expression.Children[0].Atom ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "locality-bind",
                "anchor-self",
                "anchor-other",
                "anchor-relation",
                "seal-posture",
                "reveal-posture",
                "perspective-configure",
                "perspective-orientation",
                "perspective-constraint",
                "perspective-weight",
                "participation-configure",
                "participation-mode",
                "participation-role",
                "participation-rule",
                "participation-capability"
            ],
            ops);

        Assert.DoesNotContain(ops, op => op.StartsWith("rehearsal-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("witness-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("morphism-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("transport-", StringComparison.Ordinal));
    }

    [Fact]
    public void HigherOrderLocalitySurface_RemainsFreeOfCustodyAndGovernanceHandles()
    {
        var symbolTable = new SliSymbolTable();
        var localityProperties = typeof(SliHigherOrderLocalityState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var perspectiveProperties = typeof(SliPerspectiveState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var participationProperties = typeof(SliParticipationState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();

        var allProperties = localityProperties
            .Concat(perspectiveProperties)
            .Concat(participationProperties)
            .ToArray();

        Assert.DoesNotContain(allProperties, name => name.Contains("GEL", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(allProperties, name => name.Contains("SoulFrame", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(allProperties, name => name.Contains("Govern", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(allProperties, name => name.Contains("Custody", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(allProperties, name => name.Contains("Sanctuary", StringComparison.OrdinalIgnoreCase));

        Assert.False(symbolTable.TryResolve("gel-append", out _));
        Assert.False(symbolTable.TryResolve("soulframe-mutate", out _));
        Assert.False(symbolTable.TryResolve("custody-write", out _));
        Assert.False(symbolTable.TryResolve("governance-commit", out _));
    }

    [Fact]
    public void PublicTelemetryAndResultSchemas_RemainStable()
    {
        Assert.Equal(
            ["cleave_residue", "compass_state", "confidence", "decision", "decision_branch", "engram_candidate", "reasoning", "sli_tokens", "symbolic_trace", "trace_id"],
            GetJsonPropertyNames(typeof(CognitionResult)));

        Assert.Equal(
            ["BranchingFactor", "DecisionEntropy", "EgoStability", "IdForce", "SuperegoConstraint", "SymbolicDepth", "Timestamp", "ValueElevation"],
            GetPropertyNames(typeof(CognitionCompassTelemetry)));

        Assert.Equal(
            ["CleaveResidue", "CompassState", "DecisionBranch", "EventHash", "SymbolicTrace", "SymbolicTraceHash", "Timestamp", "TraceId"],
            GetPropertyNames(typeof(SliTraceEvent)));
    }

    private static async Task<SliExecutionContext> ExecuteContextAsync(IReadOnlyList<string> symbolicProgram)
    {
        var parser = new SliParser();
        var expander = new SliLocalityCompositionExpander(LispModuleCatalog.LoadModules());
        var interpreter = new SliInterpreter(new SliSymbolTable());
        var program = expander.ExpandProgram(parser.ParseProgram(symbolicProgram));
        var context = new SliExecutionContext(
            new ContextFrame
            {
                CMEId = "locality-invariant-fixture",
                SoulFrameId = Guid.Empty,
                ContextId = Guid.Empty,
                TaskObjective = "identity-continuity",
                Engrams = []
            },
            new EngramResolverService());

        await interpreter.ExecuteProgramAsync(program, context);
        return context;
    }

    private static string[] GetJsonPropertyNames(Type type)
    {
        return type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] GetPropertyNames(Type type)
    {
        return type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }
}
