namespace San.Audit.Tests;

public sealed class PublicReleaseArtifactTemplateDocsTests
{
    [Fact]
    public void Public_Release_Artifact_Template_Preserves_Release_Not_Promotion_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var templatePath = Path.Combine(lineRoot, "docs", "PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md");
        var readinessLawPath = Path.Combine(lineRoot, "docs", "PUBLIC_RELEASE_READINESS_WORDING_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var releaseCandidateWorkflowPath = Path.Combine(repoRoot, ".github", "workflows", "release-candidate.yml");

        Assert.True(File.Exists(templatePath));
        Assert.True(File.Exists(releaseCandidateWorkflowPath));

        var templateText = File.ReadAllText(templatePath);
        var readinessLawText = File.ReadAllText(readinessLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);
        var releaseCandidateWorkflowText = File.ReadAllText(releaseCandidateWorkflowPath);

        Assert.Contains("Release artifacts are witness summaries.", templateText, StringComparison.Ordinal);
        Assert.Contains("They are not promotion engines.", templateText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_RELEASE_READINESS_WORDING_LAW.md", templateText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_CME_EXPLANATION_BOUNDARY.md", templateText, StringComparison.Ordinal);
        Assert.Contains("pass", templateText, StringComparison.Ordinal);
        Assert.Contains("hold", templateText, StringComparison.Ordinal);
        Assert.Contains("fail", templateText, StringComparison.Ordinal);
        Assert.Contains("No \"mostly ready\" reading is lawful release artifact wording.", templateText, StringComparison.Ordinal);
        Assert.Contains("Status", templateText, StringComparison.Ordinal);
        Assert.Contains("Evidence", templateText, StringComparison.Ordinal);
        Assert.Contains("Architecture Layer", templateText, StringComparison.Ordinal);
        Assert.Contains("What Changed", templateText, StringComparison.Ordinal);
        Assert.Contains("What This Does Not Claim", templateText, StringComparison.Ordinal);
        Assert.Contains("Known Holds", templateText, StringComparison.Ordinal);
        Assert.Contains("Verification", templateText, StringComparison.Ordinal);
        Assert.Contains("A documentation seat is not runtime witness.", templateText, StringComparison.Ordinal);
        Assert.Contains("A contract seat is not live enactment.", templateText, StringComparison.Ordinal);
        Assert.Contains("A runtime witness is not installer completion.", templateText, StringComparison.Ordinal);
        Assert.Contains("A substrate support is not `CME` minting.", templateText, StringComparison.Ordinal);
        Assert.Contains("A public explanation is not ontology.", templateText, StringComparison.Ordinal);
        Assert.Contains("A release-candidate evidence bundle is not production readiness by itself.", templateText, StringComparison.Ordinal);
        Assert.Contains("does not mint `CME` standing", templateText, StringComparison.Ordinal);
        Assert.Contains("does not complete the public installer", templateText, StringComparison.Ordinal);
        Assert.Contains("does not ship the hosted seed `LLM` in the public checkout", templateText, StringComparison.Ordinal);
        Assert.Contains("Release wording is not promotion.", templateText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", templateText, StringComparison.Ordinal);

        Assert.Contains("PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md", readinessLawText, StringComparison.Ordinal);
        Assert.Contains("public-release-artifact-wording-template: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public Release Artifact Wording Template", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("release artifact wording as `CME` minting", refinementText, StringComparison.Ordinal);
        Assert.Contains("release-candidate.yml", releaseCandidateWorkflowPath, StringComparison.Ordinal);
        Assert.Contains("release-candidate-evidence", releaseCandidateWorkflowText, StringComparison.Ordinal);
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
