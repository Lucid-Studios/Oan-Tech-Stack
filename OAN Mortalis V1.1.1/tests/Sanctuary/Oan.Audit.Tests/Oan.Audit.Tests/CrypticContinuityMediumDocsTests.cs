namespace San.Audit.Tests;

public sealed class CrypticContinuityMediumDocsTests
{
    [Fact]
    public void Cryptic_Continuity_Medium_Note_Preserves_Lawful_Continuity()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(lineRoot, "docs", "CRYPTIC_CONTINUITY_MEDIUM_NOTE.md");
        var responsibilityLawPath = Path.Combine(lineRoot, "docs", "RESPONSIBILITY_BINDING_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var responsibilityLawText = File.ReadAllText(responsibilityLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Note Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("`CCM` is the lawful medium by which cryptic continuity persists across", noteText, StringComparison.Ordinal);
        Assert.Contains("`TAB` and `CCM` do not do the same job.", noteText, StringComparison.Ordinal);
        Assert.Contains("`TAB` governs whether a transition is lawful.", noteText, StringComparison.Ordinal);
        Assert.Contains("`CCM` governs whether cryptic meaning remains continuous", noteText, StringComparison.Ordinal);
        Assert.Contains("`CCM` is subordinate to Prime invariants.", noteText, StringComparison.Ordinal);
        Assert.Contains("`cOE` is the active embodied cryptic participation surface.", noteText, StringComparison.Ordinal);
        Assert.Contains("What `CCM` Preserves", noteText, StringComparison.Ordinal);
        Assert.Contains("What `CCM` Forbids", noteText, StringComparison.Ordinal);
        Assert.Contains("continuity is not mere storage, and it is not free invention", noteText, StringComparison.Ordinal);
        Assert.Contains("an append-only continuity spine", noteText, StringComparison.Ordinal);
        Assert.Contains("reconstituted from trace rather than guessed afresh", noteText, StringComparison.Ordinal);
        Assert.Contains("No single utterance, output, or receipt exhausts the whole live cryptic body.", noteText, StringComparison.Ordinal);
        Assert.Contains("This note does not mint personification.", noteText, StringComparison.Ordinal);
        Assert.Contains("this note prepares the seam for `REVALIDATION_LAW.md`", noteText, StringComparison.Ordinal);
        Assert.Contains("Keep continuity live.", noteText, StringComparison.Ordinal);
        Assert.Contains("Do not confuse storage with meaning.", noteText, StringComparison.Ordinal);
        Assert.Contains("Do not confuse persistence with permission.", noteText, StringComparison.Ordinal);

        Assert.Contains("CRYPTIC_CONTINUITY_MEDIUM_NOTE.md", responsibilityLawText, StringComparison.Ordinal);
        Assert.Contains("CRYPTIC_CONTINUITY_MEDIUM_NOTE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("CRYPTIC_CONTINUITY_MEDIUM_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("cryptic-continuity-medium-note: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("cryptic continuity medium preserved as the first continuity substrate seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("dead storage masquerading as cryptic continuity", refinementText, StringComparison.Ordinal);
        Assert.Contains("freeform drift masquerading as living meaning across turns", refinementText, StringComparison.Ordinal);
        Assert.Contains("output slices treated as the whole continuity body", refinementText, StringComparison.Ordinal);
        Assert.Contains("persistence treated as permission or identity proof by itself", refinementText, StringComparison.Ordinal);
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
