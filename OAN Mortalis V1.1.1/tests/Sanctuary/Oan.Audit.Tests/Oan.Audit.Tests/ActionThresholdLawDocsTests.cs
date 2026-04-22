namespace San.Audit.Tests;

public sealed class ActionThresholdLawDocsTests
{
    [Fact]
    public void Action_Threshold_Law_Preserves_Accountable_Sufficiency()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "ACTION_THRESHOLD_LAW.md");
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
        Assert.Contains("Action does not require total agreement, but it must require accountable", lawText, StringComparison.Ordinal);
        Assert.Contains("accountable sufficiency", lawText, StringComparison.Ordinal);
        Assert.Contains("unanimity is not the threshold of lawful action", lawText, StringComparison.Ordinal);
        Assert.Contains("Discernment of action readiness must resolve to exactly one of the following", lawText, StringComparison.Ordinal);
        Assert.Contains("`Proceed`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Hold`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Refuse`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Escalate`", lawText, StringComparison.Ordinal);
        Assert.Contains("partial convergence may move; it may not masquerade as consensus", lawText, StringComparison.Ordinal);
        Assert.Contains("No Silent Coercion Rule", lawText, StringComparison.Ordinal);
        Assert.Contains("treating missing response as agreement", lawText, StringComparison.Ordinal);
        Assert.Contains("treating fatigue as consent", lawText, StringComparison.Ordinal);
        Assert.Contains("urgency can accelerate review; it cannot replace lawful threshold", lawText, StringComparison.Ordinal);
        Assert.Contains("Do not wait for impossible unanimity.", lawText, StringComparison.Ordinal);
        Assert.Contains("Do not move on convenience.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let action cross only under accountable sufficiency.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let hold remain lawful, but never costless.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let escalation name the missing authority.", lawText, StringComparison.Ordinal);

        Assert.Contains("ACTION_THRESHOLD_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("ACTION_THRESHOLD_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("action-threshold-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("action threshold preserved as the first lawful movement seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("total agreement as a prerequisite for every lawful action", refinementText, StringComparison.Ordinal);
        Assert.Contains("partial convergence as consensus by implication", refinementText, StringComparison.Ordinal);
        Assert.Contains("urgency, obviousness, routine familiarity, or silence as replacement", refinementText, StringComparison.Ordinal);
        Assert.Contains("held action as neutral, virtuous, or costless by duration alone", refinementText, StringComparison.Ordinal);
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
