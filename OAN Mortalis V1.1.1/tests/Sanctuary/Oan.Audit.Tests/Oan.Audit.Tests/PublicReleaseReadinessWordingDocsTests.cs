namespace San.Audit.Tests;

public sealed class PublicReleaseReadinessWordingDocsTests
{
    [Fact]
    public void Public_Release_Readiness_Wording_Preserves_Status_NonClaim_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var wordingPath = Path.Combine(lineRoot, "docs", "PUBLIC_RELEASE_READINESS_WORDING_LAW.md");
        var encounterPath = Path.Combine(lineRoot, "docs", "PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md");
        var onboardingPath = Path.Combine(lineRoot, "docs", "PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(wordingPath));

        var wordingText = File.ReadAllText(wordingPath);
        var encounterText = File.ReadAllText(encounterPath);
        var onboardingText = File.ReadAllText(onboardingPath);
        var gateText = File.ReadAllText(gatePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md", wordingText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md", wordingText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_RELEASE_GATE.md", wordingText, StringComparison.Ordinal);
        Assert.Contains("Public status language is a witness surface.", wordingText, StringComparison.Ordinal);
        Assert.Contains("It is not a promotion engine.", wordingText, StringComparison.Ordinal);
        Assert.Contains("complete public installer", wordingText, StringComparison.Ordinal);
        Assert.Contains("shipped hosted seed `LLM` runtime", wordingText, StringComparison.Ordinal);
        Assert.Contains("completed first legal run", wordingText, StringComparison.Ordinal);
        Assert.Contains("minted `CME`", wordingText, StringComparison.Ordinal);
        Assert.Contains("active legal person", wordingText, StringComparison.Ordinal);
        Assert.Contains("production-ready operational cognition", wordingText, StringComparison.Ordinal);
        Assert.Contains("pass", wordingText, StringComparison.Ordinal);
        Assert.Contains("hold", wordingText, StringComparison.Ordinal);
        Assert.Contains("fail", wordingText, StringComparison.Ordinal);
        Assert.Contains("No \"mostly ready\" reading is lawful public wording.", wordingText, StringComparison.Ordinal);
        Assert.Contains("Readiness may be witnessed only to the level currently supported by repo-local evidence.", wordingText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", wordingText, StringComparison.Ordinal);

        Assert.Contains("PUBLIC_RELEASE_READINESS_WORDING_LAW.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_RELEASE_READINESS_WORDING_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_RELEASE_READINESS_WORDING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("public-release-readiness-wording-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public Release Readiness Wording", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("public release wording as installer completion", refinementText, StringComparison.Ordinal);
        Assert.Contains("release tags or milestone summaries as promotion engines", refinementText, StringComparison.Ordinal);
        Assert.Contains("green build or test results as complete installer proof", refinementText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", encounterText, StringComparison.Ordinal);
        Assert.Contains("Contribution does not grant identity", readmeText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", onboardingText, StringComparison.Ordinal);
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
