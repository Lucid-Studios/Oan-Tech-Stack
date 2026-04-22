namespace San.Audit.Tests;

public sealed class HoldResolutionLawDocsTests
{
    [Fact]
    public void Hold_Resolution_Law_Preserves_Temporal_Integrity_Of_Uncertainty()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "HOLD_RESOLUTION_LAW.md");
        var actionLawPath = Path.Combine(lineRoot, "docs", "ACTION_THRESHOLD_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var actionLawText = File.ReadAllText(actionLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Anchor Law Candidate", lawText, StringComparison.Ordinal);
        Assert.Contains("Hold is a provisional protection state, not a moral shelter from", lawText, StringComparison.Ordinal);
        Assert.Contains("Legitimate Hold Conditions", lawText, StringComparison.Ordinal);
        Assert.Contains("hold is lawful only when something real is missing", lawText, StringComparison.Ordinal);
        Assert.Contains("Every `Hold` must answer:", lawText, StringComparison.Ordinal);
        Assert.Contains("what is missing, and what would change the outcome", lawText, StringComparison.Ordinal);
        Assert.Contains("All holds must remain under temporal pressure.", lawText, StringComparison.Ordinal);
        Assert.Contains("no hold is lawful without an end condition", lawText, StringComparison.Ordinal);
        Assert.Contains("A `Hold` must resolve into exactly one of the following:", lawText, StringComparison.Ordinal);
        Assert.Contains("`Proceed`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Refuse`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Escalate`", lawText, StringComparison.Ordinal);
        Assert.Contains("waiting is not neutral; it consumes capacity", lawText, StringComparison.Ordinal);
        Assert.Contains("If a `Hold` repeatedly fails to resolve, it must become `Escalate` or", lawText, StringComparison.Ordinal);
        Assert.Contains("treat prolonged `Hold` as implicit correctness", lawText, StringComparison.Ordinal);
        Assert.Contains("Hold only when you must.", lawText, StringComparison.Ordinal);
        Assert.Contains("Do not wait without cost.", lawText, StringComparison.Ordinal);
        Assert.Contains("Do not wait without limit.", lawText, StringComparison.Ordinal);
        Assert.Contains("When waiting fails to resolve,", lawText, StringComparison.Ordinal);
        Assert.Contains("this law binds directly to `ACTION_THRESHOLD_LAW.md`", lawText, StringComparison.Ordinal);

        Assert.Contains("HOLD_RESOLUTION_LAW.md", actionLawText, StringComparison.Ordinal);
        Assert.Contains("HOLD_RESOLUTION_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("HOLD_RESOLUTION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("hold-resolution-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("hold resolution preserved as the first lawful non-movement seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("hold without explicit missing condition, reevaluation basis, or expiration", refinementText, StringComparison.Ordinal);
        Assert.Contains("prolonged hold as implicit correctness or de facto refusal without receipt", refinementText, StringComparison.Ordinal);
        Assert.Contains("disappearing hold without `Proceed`, `Refuse`, or `Escalate`", refinementText, StringComparison.Ordinal);
        Assert.Contains("escalation-free hold beyond local authority or local resolution capacity", refinementText, StringComparison.Ordinal);
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
