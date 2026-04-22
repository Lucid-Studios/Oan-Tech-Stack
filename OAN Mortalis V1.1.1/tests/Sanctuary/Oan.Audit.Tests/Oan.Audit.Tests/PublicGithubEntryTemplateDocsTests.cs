namespace San.Audit.Tests;

public sealed class PublicGithubEntryTemplateDocsTests
{
    [Fact]
    public void Public_Github_Entry_Templates_Preserve_Intake_NonClaim_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var boundaryPath = Path.Combine(lineRoot, "docs", "PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var pullRequestTemplatePath = Path.Combine(repoRoot, ".github", "pull_request_template.md");
        var bugTemplatePath = Path.Combine(repoRoot, ".github", "ISSUE_TEMPLATE", "bug_report.yml");
        var featureTemplatePath = Path.Combine(repoRoot, ".github", "ISSUE_TEMPLATE", "feature_request.yml");
        var configPath = Path.Combine(repoRoot, ".github", "ISSUE_TEMPLATE", "config.yml");

        Assert.True(File.Exists(boundaryPath));

        var boundaryText = File.ReadAllText(boundaryPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);
        var pullRequestTemplateText = File.ReadAllText(pullRequestTemplatePath);
        var bugTemplateText = File.ReadAllText(bugTemplatePath);
        var featureTemplateText = File.ReadAllText(featureTemplatePath);
        var configText = File.ReadAllText(configPath);

        Assert.Contains("The templates are intake surfaces.", boundaryText, StringComparison.Ordinal);
        Assert.Contains("They are not promotion engines", boundaryText, StringComparison.Ordinal);
        Assert.Contains("affected architecture layer", boundaryText, StringComparison.Ordinal);
        Assert.Contains("affected stratum or boundary type", boundaryText, StringComparison.Ordinal);
        Assert.Contains("non-claims", boundaryText, StringComparison.Ordinal);
        Assert.Contains("standing, governance, authority, readiness, or completion risk", boundaryText, StringComparison.Ordinal);
        Assert.Contains("Template completion is not admission.", boundaryText, StringComparison.Ordinal);
        Assert.Contains("does not grant identity, custody, governance authority", boundaryText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", boundaryText, StringComparison.Ordinal);

        Assert.Contains("Affected stratum or boundary type", pullRequestTemplateText, StringComparison.Ordinal);
        Assert.Contains("State what this pull request does not claim", pullRequestTemplateText, StringComparison.Ordinal);
        Assert.Contains("does not mint `CME` standing or role enactment", pullRequestTemplateText, StringComparison.Ordinal);
        Assert.Contains("& '.\\OAN Mortalis V1.1.1\\tools\\verify-private-corpus.ps1'", pullRequestTemplateText, StringComparison.Ordinal);
        var legacyLineLabel = string.Concat("OAN Mortalis V", "1.0");
        var encodedLegacyLineDocsPath = string.Concat("OAN%20Mortalis%20V", "1.0/docs");

        Assert.DoesNotContain(legacyLineLabel, pullRequestTemplateText, StringComparison.Ordinal);

        Assert.Contains("Affected architecture layer", bugTemplateText, StringComparison.Ordinal);
        Assert.Contains("Affected stratum or boundary type", bugTemplateText, StringComparison.Ordinal);
        Assert.Contains("Non-claims and boundary risk", bugTemplateText, StringComparison.Ordinal);
        Assert.Contains("Template completion is not admission", bugTemplateText, StringComparison.Ordinal);
        Assert.Contains("private corpus paths, credentials, tokens", bugTemplateText, StringComparison.Ordinal);

        Assert.Contains("Stratum or boundary type", featureTemplateText, StringComparison.Ordinal);
        Assert.Contains("Expected witnesses or verification", featureTemplateText, StringComparison.Ordinal);
        Assert.Contains("Non-claims and boundary risk", featureTemplateText, StringComparison.Ordinal);
        Assert.Contains("Template completion is not admission", featureTemplateText, StringComparison.Ordinal);

        Assert.Contains("OAN%20Mortalis%20V1.1.1/docs", configText, StringComparison.Ordinal);
        Assert.DoesNotContain(encodedLegacyLineDocsPath, configText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("public-github-entry-template-boundary: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public GitHub Entry Template Boundary", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("template completion as admission", refinementText, StringComparison.Ordinal);
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
