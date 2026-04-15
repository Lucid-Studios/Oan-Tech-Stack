namespace San.Audit.Tests;

public sealed class BuildContractGovernanceConsistencyTests
{
    [Fact]
    public void Build_Contracts_Share_The_Sanctuary_Root_Family_Model()
    {
        var repoRoot = GetRepoRoot();
        var contractRoot = Path.Combine(repoRoot, "Build Contracts", "Crosscutting");
        var architectureFramePath = Path.Combine(contractRoot, "ARCHITECTURE_FRAME.md");
        var dependencyContractPath = Path.Combine(contractRoot, "DEPENDENCY_CONTRACT.md");
        var familyConstitutionPath = Path.Combine(contractRoot, "FAMILY_CONSTITUTION.md");
        var glossaryContractPath = Path.Combine(contractRoot, "GLOSSARY_CONTRACT.md");
        var workspaceRulesPath = Path.Combine(contractRoot, "WORKSPACE_RULES.md");

        var architectureFrameText = File.ReadAllText(architectureFramePath);
        var dependencyContractText = File.ReadAllText(dependencyContractPath);
        var familyConstitutionText = File.ReadAllText(familyConstitutionPath);
        var glossaryContractText = File.ReadAllText(glossaryContractPath);
        var workspaceRulesText = File.ReadAllText(workspaceRulesPath);

        Assert.Contains("Sanctuary-root stack", architectureFrameText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-root stack", dependencyContractText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-root stack", glossaryContractText, StringComparison.Ordinal);
        Assert.Contains("San.*", familyConstitutionText, StringComparison.Ordinal);
        Assert.Contains("Ctk.*", dependencyContractText, StringComparison.Ordinal);
        Assert.Contains("Sfr.*", glossaryContractText, StringComparison.Ordinal);
        Assert.Contains("Acr.*", architectureFrameText, StringComparison.Ordinal);
        Assert.Contains("Oan.*` as downstream application identity or legacy migration hold only", dependencyContractText, StringComparison.Ordinal);
        Assert.Contains("New foundational stack-level composition projects should use `San.*`", glossaryContractText, StringComparison.Ordinal);
        Assert.Contains("Allowed governance write paths", workspaceRulesText, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_Contracts_Do_Not_Reassert_Old_Oan_Umbrella_Model()
    {
        var repoRoot = GetRepoRoot();
        var contractRoot = Path.Combine(repoRoot, "Build Contracts", "Crosscutting");
        var contractTexts = Directory
            .EnumerateFiles(contractRoot, "*.md", SearchOption.TopDirectoryOnly)
            .ToDictionary(
                static path => Path.GetFileName(path),
                static path => File.ReadAllText(path),
                StringComparer.OrdinalIgnoreCase);

        var priorLineDocsPathFragment = "OAN Mortalis V1" + ".0/docs";
        var forbiddenFragments = new[]
        {
            "`Oan.*` for umbrella composition",
            "New stack-level composition projects should prefer `Oan.*`",
            "Oan.*` may compose all families",
            "`CradleTek.*` is foundational"
        };

        foreach (var (fileName, text) in contractTexts)
        {
            foreach (var fragment in forbiddenFragments)
            {
                Assert.DoesNotContain(fragment, text, StringComparison.Ordinal);
            }

            if (!string.Equals(fileName, "WORKSPACE_RULES.md", StringComparison.OrdinalIgnoreCase))
            {
                Assert.DoesNotContain(priorLineDocsPathFragment, text, StringComparison.Ordinal);
            }
        }
    }

    private static string GetRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Tech Stack", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
    }
}
