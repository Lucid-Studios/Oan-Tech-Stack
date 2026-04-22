namespace San.Audit.Tests;

public sealed class ContinuousLawfulCorrectionAxiomDocsTests
{
    [Fact]
    public void Continuous_Lawful_Correction_Axiom_Preserves_AntiEntropy_Read()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var axiomPath = Path.Combine(lineRoot, "docs", "CONTINUOUS_LAWFUL_CORRECTION_AXIOM.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(axiomPath));

        var axiomText = File.ReadAllText(axiomPath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Constitutional Axiom Candidate", axiomText, StringComparison.Ordinal);
        Assert.Contains("Lawful operation is not a static property of the system.", axiomText, StringComparison.Ordinal);
        Assert.Contains("It is a continuously renewed condition maintained against drift.", axiomText, StringComparison.Ordinal);
        Assert.Contains("Prime invariants must remain unsurrendered.", axiomText, StringComparison.Ordinal);
        Assert.Contains("Transition must remain attributable and lawful.", axiomText, StringComparison.Ordinal);
        Assert.Contains("Continuity must remain live without collapsing into dead storage", axiomText, StringComparison.Ordinal);
        Assert.Contains("Carried structure must be revalidated whenever present admissibility is no", axiomText, StringComparison.Ordinal);
        Assert.Contains("Standing must be recognized as a current condition", axiomText, StringComparison.Ordinal);
        Assert.Contains("the body does not claim permanent non-violation.", axiomText, StringComparison.Ordinal);
        Assert.Contains("it continuously acts to return itself toward lawful standing", axiomText, StringComparison.Ordinal);
        Assert.Contains("Error is inevitable.", axiomText, StringComparison.Ordinal);
        Assert.Contains("Unchecked error is not.", axiomText, StringComparison.Ordinal);

        Assert.Contains("CONTINUOUS_LAWFUL_CORRECTION_AXIOM.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("CONTINUOUS_LAWFUL_CORRECTION_AXIOM.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("continuous-lawful-correction-axiom: frame-now", readinessText, StringComparison.Ordinal);
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
