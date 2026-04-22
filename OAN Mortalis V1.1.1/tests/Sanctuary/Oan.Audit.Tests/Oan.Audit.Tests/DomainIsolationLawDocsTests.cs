namespace San.Audit.Tests;

public sealed class DomainIsolationLawDocsTests
{
    [Fact]
    public void Domain_Isolation_Law_Preserves_Local_Standing_And_Rediscernment()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var lawPath = Path.Combine(lineRoot, "docs", "DOMAIN_ISOLATION_LAW.md");
        var admissionLawPath = Path.Combine(lineRoot, "docs", "DOMAIN_AND_ROLE_ADMISSION_LAW.md");
        var responsibilityLawPath = Path.Combine(lineRoot, "docs", "RESPONSIBILITY_BINDING_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var admissionLawText = File.ReadAllText(admissionLawPath);
        var responsibilityLawText = File.ReadAllText(responsibilityLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Anchor Law Candidate", lawText, StringComparison.Ordinal);
        Assert.Contains("No structure may carry legitimacy across domains without re-discernment.", lawText, StringComparison.Ordinal);
        Assert.Contains("Domain admission is local standing, not portable authority.", lawText, StringComparison.Ordinal);
        Assert.Contains("lawful in one domain is not lawful everywhere", lawText, StringComparison.Ordinal);
        Assert.Contains("No Borrowed Legitimacy Rule", lawText, StringComparison.Ordinal);
        Assert.Contains("semantic resemblance", lawText, StringComparison.Ordinal);
        Assert.Contains("shared operator", lawText, StringComparison.Ordinal);
        Assert.Contains("prior success elsewhere", lawText, StringComparison.Ordinal);
        Assert.Contains("The transfer outcome must be exactly one of:", lawText, StringComparison.Ordinal);
        Assert.Contains("`Admit`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Hold`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Refuse`", lawText, StringComparison.Ordinal);
        Assert.Contains("`Escalate`", lawText, StringComparison.Ordinal);
        Assert.Contains("prior admission may inform; it may not substitute", lawText, StringComparison.Ordinal);
        Assert.Contains("origin responsibility does not vanish during transfer", lawText, StringComparison.Ordinal);
        Assert.Contains("The system must not allow:", lawText, StringComparison.Ordinal);
        Assert.Contains("domain evidence to masquerade as domain standing", lawText, StringComparison.Ordinal);
        Assert.Contains("What stands here does not stand everywhere.", lawText, StringComparison.Ordinal);
        Assert.Contains("What was admitted there is evidence here, not permission.", lawText, StringComparison.Ordinal);
        Assert.Contains("Move across domains only by re-discernment.", lawText, StringComparison.Ordinal);
        Assert.Contains("Let transfer carry trace, not borrowed legitimacy.", lawText, StringComparison.Ordinal);

        Assert.Contains("DOMAIN_ISOLATION_LAW.md", admissionLawText, StringComparison.Ordinal);
        Assert.Contains("DOMAIN_ISOLATION_LAW.md", responsibilityLawText, StringComparison.Ordinal);
        Assert.Contains("DOMAIN_ISOLATION_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("DOMAIN_ISOLATION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("domain-isolation-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("domain isolation preserved as the first cross-domain legitimacy seam", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("cross-domain reuse as inherited legitimacy without re-discernment", refinementText, StringComparison.Ordinal);
        Assert.Contains("prior admission as automatic permission in a receiving domain", refinementText, StringComparison.Ordinal);
        Assert.Contains("originating-domain success as authority for neighboring domains", refinementText, StringComparison.Ordinal);
        Assert.Contains("transferred evidence masquerading as local standing", refinementText, StringComparison.Ordinal);
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
