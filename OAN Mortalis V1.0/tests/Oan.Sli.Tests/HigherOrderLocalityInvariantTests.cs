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
                "(rehearsal-bounded-exploration locality-state branch-a identity-continuity alternate-world)",
                "(witness-branch-compare locality-state branch-a)",
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
    public async Task TransportPrograms_DoNotMutateMorphologyPropositionOrDecisionSurfaces()
    {
        var context = await ExecuteContextAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)"
            ]);

        Assert.Empty(context.MorphologyState.ResolvedLemmaRoots);
        Assert.Equal(string.Empty, context.PropositionState.PredicateRoot);
        Assert.Equal("defer", context.FinalDecision);
        Assert.Empty(context.CandidateBranches);
        Assert.Empty(context.PrunedBranches);
        Assert.Equal(SliTransportState.Completed, context.HigherOrderLocalityState.Transport.Status);
    }

    [Fact]
    public async Task AdmissibleSurfacePrograms_DoNotMutateMorphologyPropositionOrDecisionSurfaces()
    {
        var context = await ExecuteContextAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)"
            ]);

        Assert.Empty(context.MorphologyState.ResolvedLemmaRoots);
        Assert.Equal(string.Empty, context.PropositionState.PredicateRoot);
        Assert.Equal("defer", context.FinalDecision);
        Assert.Empty(context.CandidateBranches);
        Assert.Empty(context.PrunedBranches);
        Assert.Equal(SliAdmissibleSurfaceState.Formed, context.HigherOrderLocalityState.AdmissibleSurface.Status);
    }

    [Fact]
    public async Task AccountabilityPacketPrograms_DoNotMutateMorphologyPropositionOrDecisionSurfaces()
    {
        var context = await ExecuteContextAsync(
            [
                "(locality-bootstrap context cme-self task-objective identity-continuity)",
                "(perspective-bounded-observer locality-state task-objective identity-continuity)",
                "(participation-bounded-cme locality-state)",
                "(witness-locality-compare locality-state locality-state)",
                "(transport-bounded witness-state locality-state locality-state)",
                "(transport-status completed)",
                "(admissible-surface-bounded transport-state comparative informational-only)",
                "(surface-status formed)",
                "(accountability-packet-bounded surface-state)",
                "(packet-status review-ready)"
            ]);

        Assert.Empty(context.MorphologyState.ResolvedLemmaRoots);
        Assert.Equal(string.Empty, context.PropositionState.PredicateRoot);
        Assert.Equal("defer", context.FinalDecision);
        Assert.Empty(context.CandidateBranches);
        Assert.Empty(context.PrunedBranches);
        Assert.Equal(SliAccountabilityPacketState.ReviewReady, context.HigherOrderLocalityState.AccountabilityPacket.ReadinessStatus);
    }

    [Fact]
    public void CompositeForms_ExpandOnlyToBoundedLocalityOps()
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
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
    public void WitnessComposite_ExpandsOnlyToBoundedWitnessOps()
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
        var program = parser.ParseProgram(
            [
                "(witness-branch-compare locality-state branch-a)"
            ]);

        var expanded = expander.ExpandProgram(program);
        var ops = expanded
            .Select(expression => expression.Children[0].Atom ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "witness-begin",
                "witness-compare",
                "witness-preserve",
                "witness-preserve",
                "witness-preserve",
                "witness-preserve",
                "witness-preserve",
                "witness-preserve",
                "witness-preserve",
                "witness-difference",
                "witness-difference",
                "witness-residue",
                "glue-threshold",
                "morphism-candidate"
            ],
            ops);

        Assert.DoesNotContain(ops, op => op.StartsWith("transport-", StringComparison.Ordinal));
    }

    [Fact]
    public void TransportComposite_ExpandsOnlyToBoundedTransportOps()
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
        var program = parser.ParseProgram(
            [
                "(transport-bounded witness-state locality-state locality-state)"
            ]);

        var expanded = expander.ExpandProgram(program);
        var ops = expanded
            .Select(expression => expression.Children[0].Atom ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "transport-begin",
                "transport-source",
                "transport-target",
                "transport-preserve",
                "transport-preserve",
                "transport-preserve",
                "transport-preserve",
                "transport-preserve",
                "transport-preserve",
                "transport-preserve",
                "transport-status"
            ],
            ops);

        Assert.DoesNotContain(ops, op => op.StartsWith("sanctuary-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("custody-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("surface-", StringComparison.Ordinal));
    }

    [Fact]
    public void AdmissibleSurfaceComposite_ExpandsOnlyToBoundedSurfaceOps()
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
        var program = parser.ParseProgram(
            [
                "(admissible-surface-bounded transport-state comparative informational-only)"
            ]);

        var expanded = expander.ExpandProgram(program);
        var ops = expanded
            .Select(expression => expression.Children[0].Atom ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "surface-begin",
                "surface-source",
                "surface-class",
                "surface-reveal",
                "surface-boundary",
                "surface-evidence",
                "surface-evidence",
                "surface-status"
            ],
            ops);

        Assert.DoesNotContain(ops, op => op.StartsWith("sanctuary-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("custody-", StringComparison.Ordinal));
    }

    [Fact]
    public void AccountabilityPacketComposite_ExpandsOnlyToBoundedPacketOps()
    {
        var parser = new SliParser();
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
        var program = parser.ParseProgram(
            [
                "(accountability-packet-bounded surface-state)"
            ]);

        var expanded = expander.ExpandProgram(program);
        var ops = expanded
            .Select(expression => expression.Children[0].Atom ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "packet-begin",
                "packet-lineage",
                "packet-invariants",
                "packet-class",
                "packet-reveal",
                "packet-status"
            ],
            ops);

        Assert.DoesNotContain(ops, op => op.StartsWith("sanctuary-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("judgment-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("custody-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("gel-", StringComparison.Ordinal));
        Assert.DoesNotContain(ops, op => op.StartsWith("soulframe-", StringComparison.Ordinal));
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
        var rehearsalProperties = typeof(SliRehearsalState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var witnessProperties = typeof(SliWitnessState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var transportProperties = typeof(SliTransportState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var admissibleSurfaceProperties = typeof(SliAdmissibleSurfaceState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();
        var accountabilityPacketProperties = typeof(SliAccountabilityPacketState)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();

        var allProperties = localityProperties
            .Concat(perspectiveProperties)
            .Concat(participationProperties)
            .Concat(rehearsalProperties)
            .Concat(witnessProperties)
            .Concat(transportProperties)
            .Concat(admissibleSurfaceProperties)
            .Concat(accountabilityPacketProperties)
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
            ["cleave_residue", "compass_state", "confidence", "decision", "decision_branch", "engram_candidate", "golden_code_compass", "reasoning", "sli_tokens", "symbolic_trace", "trace_id", "zed_theta_candidate"],
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
        var expander = new SliBoundedCompositionExpander(LispModuleCatalog.LoadModules());
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
