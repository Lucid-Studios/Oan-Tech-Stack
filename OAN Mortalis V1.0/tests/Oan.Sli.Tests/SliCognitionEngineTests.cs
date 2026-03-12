using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using SLI.Engine.Cognition;

namespace Oan.Sli.Tests;

public sealed class SliCognitionEngineTests
{
    [Fact]
    public async Task LispRuntime_LoadsAndExecutes_WithCompassMetrics()
    {
        var resolver = new EngramResolverService();
        var engine = new SliCognitionEngine(resolver);
        await engine.InitializeAsync();

        var request = new CognitionRequest
        {
            Context = new CognitionContext
            {
                CMEId = "cme-test",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "identity-continuity",
                RelevantEngrams = []
            },
            Prompt = "execute symbolic cognition"
        };

        var result = await engine.ExecuteAsync(request);

        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
        Assert.False(string.IsNullOrWhiteSpace(result.CleaveResidue));
        Assert.InRange(result.Confidence, 0.1, 0.99);

        Assert.NotNull(engine.LastTraceEvent);
        Assert.NotNull(engine.LastDecisionSpline);
        Assert.NotNull(engine.LastTraceEvent!.CompassState);
        Assert.True(engine.LastTraceEvent.CompassState.SymbolicDepth > 0);
        Assert.True(engine.LastTraceEvent.SymbolicTrace.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(engine.LastTraceEvent.TraceId));
        Assert.Equal(result.Decision, engine.LastTraceEvent.DecisionBranch);
        Assert.Equal(result.CleaveResidue, engine.LastTraceEvent.CleaveResidue);

        var localityIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "locality-bind(");
        var perspectiveIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "perspective-configure(");
        var participationIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "participation-configure(");
        var compassIndex = IndexOfContaining(engine.LastTraceEvent.SymbolicTrace, "compass-update(");

        Assert.True(localityIndex >= 0);
        Assert.True(localityIndex < perspectiveIndex);
        Assert.True(perspectiveIndex < participationIndex);
        Assert.True(participationIndex < compassIndex);
    }

    [Fact]
    public void CanonicalProgram_UsesCompositeFormsBeforeCompassUpdate()
    {
        var engine = new SliCognitionEngine(new EngramResolverService());
        var program = engine.BuildProgram("identity-continuity", null);

        var localityIndex = IndexOfContaining(program, "(locality-bootstrap");
        var perspectiveIndex = IndexOfContaining(program, "(perspective-bounded-observer");
        var participationIndex = IndexOfContaining(program, "(participation-bounded-cme");
        var compassIndex = IndexOfContaining(program, "(compass-update");

        Assert.True(localityIndex >= 0);
        Assert.True(localityIndex < perspectiveIndex);
        Assert.True(perspectiveIndex < participationIndex);
        Assert.True(participationIndex < compassIndex);
        Assert.DoesNotContain(program, line => line.Contains("rehearsal-", StringComparison.Ordinal));
        CanonicalCognitionCycle.ValidateProgramOrder(program);
    }

    [Fact]
    public void CanonicalProgramOrder_FailsWhenCompassPrecedesHigherOrderLocality()
    {
        var invalidProgram = new[]
        {
            "(decision-evaluate predicate-set)",
            "(compass-update context reasoning-state)",
            "(locality-bootstrap context cme-self task-objective identity-continuity)",
            "(perspective-bounded-observer locality-state task-objective identity-continuity)",
            "(participation-bounded-cme locality-state)",
            "(decision-branch cognition-state)",
            "(cleave branch-set)",
            "(commit decision)"
        };

        Assert.Throws<InvalidOperationException>(() => CanonicalCognitionCycle.ValidateProgramOrder(invalidProgram));
    }

    private static int IndexOfContaining(IReadOnlyList<string> entries, string fragment)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            if (entries[index].Contains(fragment, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
