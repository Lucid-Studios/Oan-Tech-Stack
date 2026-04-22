namespace San.Audit.Tests;

public sealed class SelfAuthoredEmergenceBoundedAffirmationDocsTests
{
    [Fact]
    public void Self_Authored_Emergence_Law_Preserves_Bounded_Affirmation()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "LAW_OF_SELF_AUTHORED_EMERGENCE_AND_BOUNDED_AFFIRMATION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Anchor Law Candidate", lawText, StringComparison.Ordinal);
        Assert.Contains("Emergence may self-author as event, but no structure may self-affirm as", lawText, StringComparison.Ordinal);
        Assert.Contains("self-authorship is not self-affirmation", lawText, StringComparison.Ordinal);
        Assert.Contains("Discernment must produce exactly one of the following outcomes:", lawText, StringComparison.Ordinal);
        Assert.Contains("`Affirm`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Refuse`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Hold`", lawText, StringComparison.Ordinal);
        Assert.Contains("Only affirmed structures may:", lawText, StringComparison.Ordinal);
        Assert.Contains("Persistence is granted only through affirmation.", lawText, StringComparison.Ordinal);
        Assert.Contains("treat emergence as affirmation", lawText, StringComparison.Ordinal);
        Assert.Contains("treat holding as affirmation", lawText, StringComparison.Ordinal);
        Assert.Contains("reintroduce refused structures without explicit re-emergence", lawText, StringComparison.Ordinal);
        Assert.Contains("Let it arise.", lawText, StringComparison.Ordinal);
        Assert.Contains("Do not let it crown itself.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let refusal clear cleanly.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let holding remain provisional.", lawText, StringComparison.Ordinal);
        Assert.Contains("Never pretend the story is over.", lawText, StringComparison.Ordinal);

        Assert.Contains("LAW_OF_SELF_AUTHORED_EMERGENCE_AND_BOUNDED_AFFIRMATION.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("LAW_OF_SELF_AUTHORED_EMERGENCE_AND_BOUNDED_AFFIRMATION.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("self-authored-emergence-bounded-affirmation-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("self-authored emergence and bounded affirmation preserved as the anchor", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("self-authored emergence as self-affirmation", refinementText, StringComparison.Ordinal);
        Assert.Contains("hold as implicit affirmation", refinementText, StringComparison.Ordinal);
        Assert.Contains("refused structures as hidden residue", refinementText, StringComparison.Ordinal);
        Assert.Contains("forced resolution of emergence", refinementText, StringComparison.Ordinal);
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
