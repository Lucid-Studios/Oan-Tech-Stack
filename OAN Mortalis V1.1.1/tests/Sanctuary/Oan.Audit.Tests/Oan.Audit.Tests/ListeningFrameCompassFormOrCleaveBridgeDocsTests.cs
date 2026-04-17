namespace San.Audit.Tests;

public sealed class ListeningFrameCompassFormOrCleaveBridgeDocsTests
{
    [Fact]
    public void Listening_Frame_Compass_Form_Or_Cleave_Bridge_Preserves_Runtime_Safe_Translation()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(lineRoot, "docs", "LISTENING_FRAME_COMPASS_FORM_OR_CLEAVE_BRIDGE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Bridge Note Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("The inherited `OCBT` line contributes a formation grammar.", noteText, StringComparison.Ordinal);
        Assert.Contains("`Zed`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Delta`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Sigma`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Psi / Omega`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Gamma`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Sigma` should not be read as final verdict.", noteText, StringComparison.Ordinal);
        Assert.Contains("the cut is inquiry-bearing before it is verdict-bearing", noteText, StringComparison.Ordinal);
        Assert.Contains("`Listening Frame`", noteText, StringComparison.Ordinal);
        Assert.Contains("`Compass`", noteText, StringComparison.Ordinal);
        Assert.Contains("`form-or-cleave`", noteText, StringComparison.Ordinal);
        Assert.Contains("`EC`", noteText, StringComparison.Ordinal);
        Assert.Contains("Each operational beat should be read as a form-or-cleave checkpoint.", noteText, StringComparison.Ordinal);
        Assert.Contains("`Cleave` is not default.", noteText, StringComparison.Ordinal);
        Assert.Contains("If nothing lawful survives, the outcome is rejection", noteText, StringComparison.Ordinal);
        Assert.Contains("inherited `OCBT` formation grammar does not natively contain:", noteText, StringComparison.Ordinal);
        Assert.Contains("- `TAB`", noteText, StringComparison.Ordinal);
        Assert.Contains("- `REVALIDATION`", noteText, StringComparison.Ordinal);
        Assert.Contains("- `STANDING`", noteText, StringComparison.Ordinal);
        Assert.Contains("do not mythologize inherited symbolic language into runtime law", noteText, StringComparison.Ordinal);
        Assert.Contains("do not flatten inherited symbolic language into decorative metaphor", noteText, StringComparison.Ordinal);

        Assert.Contains("LISTENING_FRAME_COMPASS_FORM_OR_CLEAVE_BRIDGE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("LISTENING_FRAME_COMPASS_FORM_OR_CLEAVE_BRIDGE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("listening-frame-compass-form-or-cleave-bridge: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("runtime-safe translation seam for inherited `OCBT` formation grammar", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("inherited symbolic formation grammar mythologized into runtime law", refinementText, StringComparison.Ordinal);
        Assert.Contains("inquiry-bearing cleave flattened into decorative metaphor", refinementText, StringComparison.Ordinal);
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
