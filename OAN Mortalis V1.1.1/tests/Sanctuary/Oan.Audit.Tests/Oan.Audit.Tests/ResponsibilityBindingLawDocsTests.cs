namespace San.Audit.Tests;

public sealed class ResponsibilityBindingLawDocsTests
{
    [Fact]
    public void Responsibility_Binding_Law_Preserves_Attributable_Consequence()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "RESPONSIBILITY_BINDING_LAW.md");
        var actionLawPath = Path.Combine(lineRoot, "docs", "ACTION_THRESHOLD_LAW.md");
        var holdLawPath = Path.Combine(lineRoot, "docs", "HOLD_RESOLUTION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var actionLawText = File.ReadAllText(actionLawPath);
        var holdLawText = File.ReadAllText(holdLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Anchor Law Candidate", lawText, StringComparison.Ordinal);
        Assert.Contains("No lawful outcome may exist without an attributable bearer of consequence.", lawText, StringComparison.Ordinal);
        Assert.Contains("Permission to resolve must bind responsibility to the resolution.", lawText, StringComparison.Ordinal);
        Assert.Contains("Responsibility Of `Proceed`", lawText, StringComparison.Ordinal);
        Assert.Contains("if it proceeds, someone stands behind it", lawText, StringComparison.Ordinal);
        Assert.Contains("Responsibility Of `Refuse`", lawText, StringComparison.Ordinal);
        Assert.Contains("Stewardship Of `Hold`", lawText, StringComparison.Ordinal);
        Assert.Contains("holding is not neutral; it is actively maintained", lawText, StringComparison.Ordinal);
        Assert.Contains("Responsibility In `Escalate`", lawText, StringComparison.Ordinal);
        Assert.Contains("escalation transfers obligation to decide, not responsibility for origin", lawText, StringComparison.Ordinal);
        Assert.Contains("Plural Participation Binding", lawText, StringComparison.Ordinal);
        Assert.Contains("Receipt Requirement", lawText, StringComparison.Ordinal);
        Assert.Contains("what was permitted must remain attributable", lawText, StringComparison.Ordinal);
        Assert.Contains("Anti Diffusion Invariant", lawText, StringComparison.Ordinal);
        Assert.Contains("If it proceeds, someone stands behind it.", lawText, StringComparison.Ordinal);
        Assert.Contains("If it refuses, someone stands behind that.", lawText, StringComparison.Ordinal);
        Assert.Contains("If it holds, someone is maintaining it.", lawText, StringComparison.Ordinal);
        Assert.Contains("If it escalates, responsibility does not disappear.", lawText, StringComparison.Ordinal);
        Assert.Contains("Nothing becomes real without someone bearing it.", lawText, StringComparison.Ordinal);

        Assert.Contains("RESPONSIBILITY_BINDING_LAW.md", actionLawText, StringComparison.Ordinal);
        Assert.Contains("RESPONSIBILITY_BINDING_LAW.md", holdLawText, StringComparison.Ordinal);
        Assert.Contains("RESPONSIBILITY_BINDING_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("RESPONSIBILITY_BINDING_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("responsibility-binding-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("responsibility binding preserved as the first consequence-attribution seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("`Proceed` without a primary bearer, scope, or discernment basis", refinementText, StringComparison.Ordinal);
        Assert.Contains("`Refuse` without accountable refusal authority or closure condition", refinementText, StringComparison.Ordinal);
        Assert.Contains("`Hold` without a steward and reevaluation obligation", refinementText, StringComparison.Ordinal);
        Assert.Contains("`Escalate` as responsibility erasure or anonymous system transfer", refinementText, StringComparison.Ordinal);
        Assert.Contains("plural participation as diluted authorship or post-hoc ambiguity", refinementText, StringComparison.Ordinal);
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
