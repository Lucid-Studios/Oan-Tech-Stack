namespace San.Audit.Tests;

public sealed class PsyPlusMinusHoldingNoteDocsTests
{
    [Fact]
    public void Psy_Plus_Minus_Holding_Note_Preserves_PreAdmissible_Chamber()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(lineRoot, "docs", "PSY_PLUS_MINUS_HOLDING_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Note Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("structured retention of not-yet-admissible constructs", noteText, StringComparison.Ordinal);
        Assert.Contains("This chamber is for pre-admissible cognition only.", noteText, StringComparison.Ordinal);
        Assert.Contains("## `Psy+`", noteText, StringComparison.Ordinal);
        Assert.Contains("## `Psy-`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psy ±` may influence attention.", noteText, StringComparison.Ordinal);
        Assert.Contains("It may not authorize action.", noteText, StringComparison.Ordinal);
        Assert.Contains("Entries in `Psy ±` must be discardable without side effects.", noteText, StringComparison.Ordinal);
        Assert.Contains("dropping a held construct does not alter standing", noteText, StringComparison.Ordinal);
        Assert.Contains("dropping a held construct does not alter permission", noteText, StringComparison.Ordinal);
        Assert.Contains("implicit carry into `CCM` as authority", noteText, StringComparison.Ordinal);
        Assert.Contains("silent promotion into action", noteText, StringComparison.Ordinal);
        Assert.Contains("confidence creep without revalidation", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psy ±` is not `CCM`.", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psy ±` is not `REVALIDATION`.", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psy ±` is not `TAB`.", noteText, StringComparison.Ordinal);

        Assert.Contains("PSY_PLUS_MINUS_HOLDING_NOTE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PSY_PLUS_MINUS_HOLDING_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("psy-plus-minus-holding-note: frame-now", readinessText, StringComparison.Ordinal);
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
