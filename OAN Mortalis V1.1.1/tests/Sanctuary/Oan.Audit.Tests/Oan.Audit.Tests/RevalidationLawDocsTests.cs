namespace San.Audit.Tests;

public sealed class RevalidationLawDocsTests
{
    [Fact]
    public void Revalidation_Law_Preserves_Continued_Validity_Without_Amnesia_Or_Permission_Drift()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "REVALIDATION_LAW.md");
        var ccmPath = Path.Combine(lineRoot, "docs", "CRYPTIC_CONTINUITY_MEDIUM_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var ccmText = File.ReadAllText(ccmPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("This note governs continued validity across time", lawText, StringComparison.Ordinal);
        Assert.Contains("Continuity is not self-validating.", lawText, StringComparison.Ordinal);
        Assert.Contains("current permission", lawText, StringComparison.Ordinal);
        Assert.Contains("re-discerned when conditions demand it", lawText, StringComparison.Ordinal);
        Assert.Contains("## Law I - Admissible Persistence", lawText, StringComparison.Ordinal);
        Assert.Contains("evidence-only until revalidated", lawText, StringComparison.Ordinal);
        Assert.Contains("## Law II - Revalidation Triggers", lawText, StringComparison.Ordinal);
        Assert.Contains("`DomainShift`", lawText, StringComparison.Ordinal);
        Assert.Contains("`RoleShiftOrAmbiguity`", lawText, StringComparison.Ordinal);
        Assert.Contains("`ConstraintChange`", lawText, StringComparison.Ordinal);
        Assert.Contains("`TemporalPressure`", lawText, StringComparison.Ordinal);
        Assert.Contains("`ConflictOrChallenge`", lawText, StringComparison.Ordinal);
        Assert.Contains("`ActionEscalation`", lawText, StringComparison.Ordinal);
        Assert.Contains("Prior truth travels as evidence, not as present permission.", lawText, StringComparison.Ordinal);
        Assert.Contains("revalidation operates on admissibility, not existence", lawText, StringComparison.Ordinal);
        Assert.Contains("only permission state is reset, not historical continuity", lawText, StringComparison.Ordinal);
        Assert.Contains("revalidation produces a new accountable state rather than a silent overwrite", lawText, StringComparison.Ordinal);
        Assert.Contains("Continuity may remember, but only revalidation may authorize.", lawText, StringComparison.Ordinal);

        Assert.Contains("REVALIDATION_LAW.md", ccmText, StringComparison.Ordinal);
        Assert.Contains("REVALIDATION_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("REVALIDATION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("revalidation-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("revalidation preserved as the first temporal validity seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("stale truth acting as present authority without revalidation", refinementText, StringComparison.Ordinal);
        Assert.Contains("prior evidence treated as current permission after domain, role, or", refinementText, StringComparison.Ordinal);
        Assert.Contains("continuity collapse into forced amnesia during admissibility refresh", refinementText, StringComparison.Ordinal);
        Assert.Contains("silent overwrite of prior responsibility during revalidation", refinementText, StringComparison.Ordinal);
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
