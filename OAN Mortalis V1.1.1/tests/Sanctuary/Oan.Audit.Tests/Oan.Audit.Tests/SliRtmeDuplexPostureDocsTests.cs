namespace San.Audit.Tests;

public sealed class SliRtmeDuplexPostureDocsTests
{
    [Fact]
    public void Docs_Record_SliRtme_Duplex_Posture_Engine()
    {
        var lineRoot = GetLineRoot();
        var notePath = Path.Combine(lineRoot, "docs", "SLI_RTME_DUPLEX_POSTURE_ENGINE_NOTE.md");
        var braidNotePath = Path.Combine(lineRoot, "docs", "SLI_RTME_CLUSTERED_SWARMED_BRAID_DISCIPLINE_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var modulePath = Path.Combine(lineRoot, "src", "Sanctuary", "SLI.Lisp", "SLI.Lisp", "duplex-posture.lisp");
        var braidModulePath = Path.Combine(lineRoot, "src", "Sanctuary", "SLI.Lisp", "SLI.Lisp", "duplex-braid.lisp");

        Assert.True(File.Exists(notePath));
        Assert.True(File.Exists(braidNotePath));
        Assert.True(File.Exists(modulePath));
        Assert.True(File.Exists(braidModulePath));

        var noteText = File.ReadAllText(notePath);
        var braidNoteText = File.ReadAllText(braidNotePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("projected posture transitions may deepen, combine, or stabilize field", noteText, StringComparison.Ordinal);
        Assert.Contains("Prime closure remains outside `RTME` authority", noteText, StringComparison.Ordinal);
        Assert.Contains("duplex-posture.lisp", noteText, StringComparison.Ordinal);
        Assert.Contains("per-line distinction", braidNoteText, StringComparison.Ordinal);
        Assert.Contains("CoherentBraid", braidNoteText, StringComparison.Ordinal);
        Assert.Contains("UnstableBraid", braidNoteText, StringComparison.Ordinal);
        Assert.Contains("duplex-braid.lisp", braidNoteText, StringComparison.Ordinal);
        Assert.Contains("SLI_RTME_DUPLEX_POSTURE_ENGINE_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("SLI_RTME_CLUSTERED_SWARMED_BRAID_DISCIPLINE_NOTE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("sli-rtme-duplex-posture-engine: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("sli-rtme-clustered-swarmed-braid-discipline: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("RTME` duplex posture engine", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("clustered and swarmed braid discipline", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("full Lisp-side projected field autonomy", refinementText, StringComparison.Ordinal);
        Assert.Contains("projected braid history", refinementText, StringComparison.Ordinal);
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
