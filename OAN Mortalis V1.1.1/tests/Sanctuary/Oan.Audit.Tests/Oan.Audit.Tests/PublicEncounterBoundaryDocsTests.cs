namespace San.Audit.Tests;

public sealed class PublicEncounterBoundaryDocsTests
{
    [Fact]
    public void Public_Encounter_Boundary_Preserves_NonClaim_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var lawPath = Path.Combine(lineRoot, "docs", "PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md");
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

        Assert.Contains("lawfully bounded engineered cognition framework", lawText, StringComparison.Ordinal);
        Assert.Contains("not a person", lawText, StringComparison.Ordinal);
        Assert.Contains("not autonomous authority", lawText, StringComparison.Ordinal);
        Assert.Contains("not self-originating", lawText, StringComparison.Ordinal);
        Assert.Contains("input -> bounded processing -> receipted output", lawText, StringComparison.Ordinal);
        Assert.Contains("Domain -> Role -> Capacity", lawText, StringComparison.Ordinal);
        Assert.Contains("refusal exists", lawText, StringComparison.Ordinal);
        Assert.Contains("contextual", lawText, StringComparison.Ordinal);
        Assert.Contains("bounded", lawText, StringComparison.Ordinal);
        Assert.Contains("revisable", lawText, StringComparison.Ordinal);
        Assert.Contains("interpretation", lawText, StringComparison.Ordinal);
        Assert.Contains("high-stakes decisions", lawText, StringComparison.Ordinal);
        Assert.Contains("legal accountability", lawText, StringComparison.Ordinal);
        Assert.Contains("human oversight", lawText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", lawText, StringComparison.Ordinal);

        Assert.Contains("PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("public-encounter-boundary-non-claims: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public Encounter Boundary", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live personhood claim", refinementText, StringComparison.Ordinal);
        Assert.Contains("autonomous authority", refinementText, StringComparison.Ordinal);
        Assert.Contains("legal accountability or custody claims", refinementText, StringComparison.Ordinal);
    }

    private static string GetRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null &&
               (!File.Exists(Path.Combine(current.FullName, "build.ps1")) ||
                !File.Exists(Path.Combine(current.FullName, "README.md"))))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate repository root.");
    }
}
