namespace San.Audit.Tests;

public sealed class EcLifecycleNoteDocsTests
{
    [Fact]
    public void Ec_Lifecycle_Note_Preserves_Beginning_Standing_And_Collapse()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(lineRoot, "docs", "EC_LIFECYCLE_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Note Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("`EC` begins when a field becomes differentiable enough to support inquiry", noteText, StringComparison.Ordinal);
        Assert.Contains("## Beginning", noteText, StringComparison.Ordinal);
        Assert.Contains("`Zed`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Delta`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Sigma`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Listening Frame`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Compass`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psy +/-`", noteText, StringComparison.Ordinal);
        Assert.Contains("## Standing Form", noteText, StringComparison.Ordinal);
        Assert.Contains("a lawfully admitted, role-bound, continuity-bearing enacted body whose", noteText, StringComparison.Ordinal);
        Assert.Contains("current permission to operate is valid now", noteText, StringComparison.Ordinal);
        Assert.Contains("Running is not the same as standing.", noteText, StringComparison.Ordinal);
        Assert.Contains("## Lawful Collapse", noteText, StringComparison.Ordinal);
        Assert.Contains("`Refuse`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Cleave`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Hold`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Proceed to closure`", noteText, StringComparison.Ordinal);
        Assert.Contains("Collapse must never be silent.", noteText, StringComparison.Ordinal);
        Assert.Contains("trace", noteText, StringComparison.Ordinal);
        Assert.Contains("attribution", noteText, StringComparison.Ordinal);
        Assert.Contains("basis", noteText, StringComparison.Ordinal);
        Assert.Contains("scope", noteText, StringComparison.Ordinal);

        Assert.Contains("EC_LIFECYCLE_NOTE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("EC_LIFECYCLE_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("ec-lifecycle-note: frame-now", readinessText, StringComparison.Ordinal);
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
