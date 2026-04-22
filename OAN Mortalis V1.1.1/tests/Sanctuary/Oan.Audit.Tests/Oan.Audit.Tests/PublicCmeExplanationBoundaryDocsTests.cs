namespace San.Audit.Tests;

public sealed class PublicCmeExplanationBoundaryDocsTests
{
    [Fact]
    public void Public_Cme_Explanation_Preserves_Category_Not_Minting_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var publicCmePath = Path.Combine(lineRoot, "docs", "PUBLIC_CME_EXPLANATION_BOUNDARY.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(publicCmePath));

        var publicCmeText = File.ReadAllText(publicCmePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("bounded engineered-cognitive formation category within the Sanctuary architecture", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("This is a public explanation surface.", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("It is not `CME` minting.", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Public explanation of `CME` must not imply personhood, installer completion, autonomous authority, legal accountability, or certainty beyond the witnessed supports presently seated in the system.", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Founding Is Not Minting", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Embodiment Is Not Persona", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Return Is Not Promotion", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("CME_ENGINEERED_COGNITIVE_SENSORY_BODY_LAW.md", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("CME_RETURN_AUDIT_AND_PROMOTION_LAW.md", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Public `CME` explanation must explicitly preserve", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not a personhood claim", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not legal personhood", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not autonomous authority", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not legal accountability", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not custody authority", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not governance authority", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not installer completion", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not hosted seed `LLM` shipment", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("`CME` is not certainty beyond evidence", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("Explanation is not minting.", publicCmeText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", publicCmeText, StringComparison.Ordinal);

        Assert.Contains("PUBLIC_CME_EXPLANATION_BOUNDARY.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_CME_EXPLANATION_BOUNDARY.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("public-cme-explanation-boundary: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public CME Explanation Boundary", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("public `CME` explanation as `CME` minting", refinementText, StringComparison.Ordinal);
        Assert.Contains("founding bundle support as full `CME` minting", refinementText, StringComparison.Ordinal);
        Assert.Contains("sensory embodiment support as runtime persona", refinementText, StringComparison.Ordinal);
        Assert.Contains("bonded return, staging, cleaving, or audit receipt as canonical promotion", refinementText, StringComparison.Ordinal);
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
