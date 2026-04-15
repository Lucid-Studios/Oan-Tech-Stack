using System.Text.Json;

namespace San.Audit.Tests;

public sealed class StackRootRenamingMigrationTests
{
    [Fact]
    public void Stack_Root_Docs_And_Freeze_Surfaces_Are_Aligned()
    {
        var repoRoot = GetRepoRoot();
        var familyConstitutionPath = Path.Combine(repoRoot, "Build Contracts", "Crosscutting", "FAMILY_CONSTITUTION.md");
        var migrationPlanPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "STACK_ROOT_RENAMING_MIGRATION_PLAN.md");
        var topologyPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md");
        var buildReadinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        var familyText = File.ReadAllText(familyConstitutionPath);
        var planText = File.ReadAllText(migrationPlanPath);
        var topologyText = File.ReadAllText(topologyPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("San.*", familyText, StringComparison.Ordinal);
        Assert.Contains("Ctk.*", familyText, StringComparison.Ordinal);
        Assert.Contains("Sfr.*", familyText, StringComparison.Ordinal);
        Assert.Contains("Acr.*", familyText, StringComparison.Ordinal);
        Assert.Contains("`OAN` is now treated as a downstream application", familyText, StringComparison.Ordinal);
        Assert.Contains("no new foundational `Oan.*` namespaces or project names", familyText, StringComparison.Ordinal);

        Assert.Contains("stack root and constitutional host substrate", planText, StringComparison.Ordinal);
        Assert.Contains("no new foundational namespace may use `Oan.*`", planText, StringComparison.Ordinal);
        Assert.Contains("legacy-oan-namespace-allowlist.json", planText, StringComparison.Ordinal);

        Assert.Contains("`San.*` for Sanctuary-root constitutional host", topologyText, StringComparison.Ordinal);
        Assert.Contains("`src/Sanctuary/Oan.* -> src/San/`", topologyText, StringComparison.Ordinal);
        Assert.Contains("`src/San/`", topologyText, StringComparison.Ordinal);

        Assert.Contains("STACK_ROOT_RENAMING_MIGRATION_PLAN.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("build/legacy-oan-namespace-allowlist.json", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("stack-root-renaming-migration-plan: frame-now", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("legacy-oan-namespace-freeze: admitted-transition-bounded", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary stack-root correction and forward family-prefix freeze", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("governed repository, root-folder, and legacy `Oan.*` rename execution", refinementText, StringComparison.Ordinal);
    }

    [Fact]
    public void Legacy_Oan_Allowlist_Is_Exact_And_Blocks_New_Drift()
    {
        var repoRoot = GetRepoRoot();
        var allowlistPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "build", "legacy-oan-namespace-allowlist.json");
        using var document = JsonDocument.Parse(File.ReadAllText(allowlistPath));
        var root = document.RootElement;

        Assert.Equal("no-new-oan-prefix-surfaces", root.GetProperty("freezeRule").GetString());
        Assert.Equal(
            [
                "San",
                "Ctk",
                "Sfr",
                "Acr",
                "SLI"
            ],
            root.GetProperty("forwardFamilies").EnumerateArray().Select(static item => item.GetString()).ToArray());

        var allowedNamespaceFiles = root.GetProperty("legacyNamespaceFiles").EnumerateArray().Select(static item => item.GetString()).ToArray();
        var allowedProjectFiles = root.GetProperty("legacyProjectFiles").EnumerateArray().Select(static item => item.GetString()).ToArray();

        Assert.Equal(94, allowedNamespaceFiles.Length);
        Assert.Equal(11, allowedProjectFiles.Length);

        var lineRoot = Path.Combine(repoRoot, "OAN Mortalis V1.1.1");
        var actualNamespaceFiles = Directory.GetFiles(lineRoot, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(FileDeclaresLegacyOanNamespace)
            .Select(path => ToRepoRelative(repoRoot, path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        var actualProjectFiles = Directory.GetFiles(lineRoot, "Oan*.csproj", SearchOption.AllDirectories)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(path => ToRepoRelative(repoRoot, path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(allowedNamespaceFiles, actualNamespaceFiles);
        Assert.Equal(allowedProjectFiles, actualProjectFiles);
    }

    [Fact]
    public void Fresh_Seams_Already_Use_San_Prefixes()
    {
        var repoRoot = GetRepoRoot();
        var contractsPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "src", "Sanctuary", "Oan.Common", "Oan.Common", "AgentBuildOrchestrationContracts.cs");
        var testsPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "tests", "Sanctuary", "Oan.Audit.Tests", "Oan.Audit.Tests", "AgentBuildOrchestrationContractsTests.cs");

        var contractsText = File.ReadAllText(contractsPath);
        var testsText = File.ReadAllText(testsPath);

        Assert.Contains("namespace San.Common;", contractsText, StringComparison.Ordinal);
        Assert.Contains("namespace San.Audit.Tests;", testsText, StringComparison.Ordinal);
        Assert.DoesNotContain("namespace Oan.Common;", contractsText, StringComparison.Ordinal);
    }

    private static bool FileDeclaresLegacyOanNamespace(string path)
    {
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("namespace Oan.", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string ToRepoRelative(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

    private static string GetRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Tech Stack", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the repository root.");
    }
}
