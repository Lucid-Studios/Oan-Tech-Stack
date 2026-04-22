namespace San.Audit.Tests;

public sealed class PublicContributionOnboardingDocsTests
{
    [Fact]
    public void Public_Contribution_Onboarding_Preserves_Entry_NonClaim_Law()
    {
        var repoRoot = GetRepoRoot();
        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var onboardingPath = Path.Combine(lineRoot, "docs", "PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md");
        var encounterPath = Path.Combine(lineRoot, "docs", "PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var contributingPath = Path.Combine(repoRoot, "CONTRIBUTING.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(onboardingPath));

        var onboardingText = File.ReadAllText(onboardingPath);
        var encounterText = File.ReadAllText(encounterPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var contributingText = File.ReadAllText(contributingPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Contributions enter as proposed changes to a governed engineering workspace.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("identity claims", onboardingText, StringComparison.Ordinal);
        Assert.Contains("personhood claims", onboardingText, StringComparison.Ordinal);
        Assert.Contains("autonomous authority", onboardingText, StringComparison.Ordinal);
        Assert.Contains("legal authority", onboardingText, StringComparison.Ordinal);
        Assert.Contains("custody authority", onboardingText, StringComparison.Ordinal);
        Assert.Contains("proof of `CME` standing", onboardingText, StringComparison.Ordinal);
        Assert.Contains("proof of installer completion", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Domain -> Role -> Capacity", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Opening an issue grants no write capacity.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Opening a pull request grants no merge authority.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Reviewing a pull request grants no governance authority.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Using stack vocabulary grants no ontology", onboardingText, StringComparison.Ordinal);
        Assert.Contains("Exploratory language may be proposed.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("It may not be presented as current executable truth.", onboardingText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", onboardingText, StringComparison.Ordinal);

        Assert.Contains("PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md", contributingText, StringComparison.Ordinal);
        Assert.Contains("Domain -> Role -> Capacity", contributingText, StringComparison.Ordinal);
        Assert.Contains("Review approval does not create governance law.", contributingText, StringComparison.Ordinal);
        Assert.Contains("Stack vocabulary does not create ontology.", contributingText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md", readmeText, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("public-contribution-onboarding-boundary: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Public Contribution Onboarding Boundary", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("contribution authorship as custody", refinementText, StringComparison.Ordinal);
        Assert.Contains("review approval as governance law", refinementText, StringComparison.Ordinal);
        Assert.Contains("issue discussion as executable truth", refinementText, StringComparison.Ordinal);
        Assert.Contains("No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.", encounterText, StringComparison.Ordinal);
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
