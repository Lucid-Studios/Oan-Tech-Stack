namespace San.Audit.Tests;

public sealed class SessionCleanupBraidingEventDocsTests
{
    [Fact]
    public void Docs_Record_Session_Cleanup_And_Braiding_Event_Matrix()
    {
        var lineRoot = GetLineRoot();
        var matrixPath = Path.Combine(lineRoot, "docs", "SESSION_CLEANUP_AND_BRAIDING_EVENT_MATRIX.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(matrixPath));

        var matrixText = File.ReadAllText(matrixPath);
        var baselineText = File.ReadAllText(baselinePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("a cleanup braiding event is an integration act", matrixText, StringComparison.Ordinal);
        Assert.Contains("cleanup may braid and hold relation, but it does not itself condense", matrixText, StringComparison.Ordinal);
        Assert.Contains("groupoid -> fibrinoid -> bundle", matrixText, StringComparison.Ordinal);
        Assert.Contains("SESSION_CLEANUP_AND_BRAIDING_EVENT_MATRIX.md", baselineText, StringComparison.Ordinal);
        Assert.Contains("SESSION_CLEANUP_AND_BRAIDING_EVENT_MATRIX.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("session-cleanup-braiding-event-matrix: admitted-local-bounded", readinessText, StringComparison.Ordinal);
        Assert.Contains("Session cleanup and braiding event matrix preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("post-body resonance chamber and Compass heartbeat initiation/sustain", refinementText, StringComparison.Ordinal);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
