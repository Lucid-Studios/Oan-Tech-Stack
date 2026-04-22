namespace San.Audit.Tests;

public sealed class SanctuaryBiadCradleTekGovernanceDocsTests
{
    [Fact]
    public void Docs_Record_Sanctuary_Biad_And_Single_CradleTek_Governing_Surface()
    {
        var lineRoot = GetLineRoot();
        var notePath = Path.Combine(lineRoot, "docs", "SANCTUARY_BIAD_AND_CRADLETEK_GOVERNING_SURFACE_NOTE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("`Sanctuary` is governed by the singular constitutional biad", noteText, StringComparison.Ordinal);
        Assert.Contains("governing surface beneath that braid", noteText, StringComparison.Ordinal);
        Assert.Contains("How We Hold The Truth", noteText, StringComparison.Ordinal);
        Assert.Contains("`ParentStanding` names the singular Sanctuary biad.", firstRunText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_BIAD_AND_CRADLETEK_GOVERNING_SURFACE_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary biad and single-surface `CradleTek` governance correction", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("Steward-issued cradle braid, and single", refinementText, StringComparison.Ordinal);
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
