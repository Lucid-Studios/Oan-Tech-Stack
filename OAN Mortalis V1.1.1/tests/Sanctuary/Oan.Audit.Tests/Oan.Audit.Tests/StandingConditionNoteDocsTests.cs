namespace San.Audit.Tests;

public sealed class StandingConditionNoteDocsTests
{
    [Fact]
    public void Standing_Condition_Note_Preserves_Lawful_Motion_Predicate()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(lineRoot, "docs", "STANDING_CONDITION_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Note Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("Standing is not identity.", noteText, StringComparison.Ordinal);
        Assert.Contains("Standing is the condition under which lawful cognition is permitted to", noteText, StringComparison.Ordinal);
        Assert.Contains("This note is a convergence surface.", noteText, StringComparison.Ordinal);
        Assert.Contains("### 1. Prime Compliance", noteText, StringComparison.Ordinal);
        Assert.Contains("### 2. Domain Admission", noteText, StringComparison.Ordinal);
        Assert.Contains("### 3. Role Binding", noteText, StringComparison.Ordinal);
        Assert.Contains("### 4. Permission Validity", noteText, StringComparison.Ordinal);
        Assert.Contains("### 5. Continuity Integrity", noteText, StringComparison.Ordinal);
        Assert.Contains("### 6. Responsibility Attribution", noteText, StringComparison.Ordinal);
        Assert.Contains("The system may still be running without standing.", noteText, StringComparison.Ordinal);
        Assert.Contains("audit checkpoints", noteText, StringComparison.Ordinal);
        Assert.Contains("runtime invariant checks", noteText, StringComparison.Ordinal);
        Assert.Contains("If these fail,", noteText, StringComparison.Ordinal);
        Assert.Contains("the system may still run,", noteText, StringComparison.Ordinal);
        Assert.Contains("but it may not stand.", noteText, StringComparison.Ordinal);

        Assert.Contains("STANDING_CONDITION_NOTE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("STANDING_CONDITION_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("standing-condition-note: frame-now", readinessText, StringComparison.Ordinal);
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
