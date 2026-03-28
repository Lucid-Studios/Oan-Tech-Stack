namespace Oan.Audit.Tests;

public sealed class BootstrapBoundaryTests
{
    [Fact]
    public void V111_Line_DoesNotContainKnownResidueTokens()
    {
        var forbiddenTokens = new[]
        {
            "LegacyPrimeDerivativePublisher",
            "LispSliBridgeStub",
            "DeterministicHarness",
            "AgentRuntime",
            "Mock for now",
            "In a real system"
        };

        var violations = FindTokenViolations(forbiddenTokens);
        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_Line_DoesNotContain_Placeholder_Source_Scaffolding()
    {
        var lineRoot = GetLineRoot();
        var violations = Directory
            .EnumerateFiles(Path.Combine(lineRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file =>
                string.Equals(Path.GetFileName(file), "Class1.cs", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetFileName(file), "BootstrapLineMarker.cs", StringComparison.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(lineRoot, file))
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_Line_ReferencesV10OnlyInMigrationNotes()
    {
        var lineRoot = GetLineRoot();
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(lineRoot, "docs", "V1_1_1_MIGRATION_CHARTER.md"),
            Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md"),
            Path.Combine(lineRoot, "docs", "V1_0_RETIREMENT_GATE.md")
        };

        var violations = EnumerateTrackedTextFiles()
            .Where(file => !allowed.Contains(file))
            .Where(file => File.ReadAllText(file).Contains("OAN Mortalis V1.0", StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(lineRoot, file))
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_Solution_ContainsOnlyBootstrapProjectFamilies()
    {
        var lineRoot = GetLineRoot();
        var solutionText = File.ReadAllText(Path.Combine(lineRoot, "Oan.sln"));
        var expectedProjects = new[]
        {
            "GEL.Contracts",
            "CradleTek.Custody",
            "CradleTek.Host",
            "CradleTek.Mantle",
            "CradleTek.Memory",
            "CradleTek.Runtime",
            "SoulFrame.Bootstrap",
            "SoulFrame.Membrane",
            "AgentiCore",
            "SLI.Engine",
            "SLI.Ingestion",
            "Oan.Common",
            "Oan.Nexus.Control",
            "Oan.HostedLlm",
            "Oan.PrimeCryptic.Services",
            "Oan.Runtime.Headless",
            "Oan.Runtime.Materialization",
            "Oan.State.Modulation",
            "Oan.Trace.Persistence",
            "Oan.Audit.Tests",
            "Oan.Runtime.IntegrationTests",
            "SLI.Lisp"
        };

        foreach (var project in expectedProjects)
        {
            Assert.Contains(project, solutionText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("Oan.Fgs", solutionText, StringComparison.Ordinal);
        Assert.DoesNotContain("OAN.Core", solutionText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Operational_Tree_Uses_Sanctuary_And_TechStack_Roots()
    {
        var lineRoot = GetLineRoot();

        Assert.True(Directory.Exists(Path.Combine(lineRoot, "src", "Sanctuary")));
        Assert.True(Directory.Exists(Path.Combine(lineRoot, "src", "TechStack")));
        Assert.True(Directory.Exists(Path.Combine(lineRoot, "tests", "Sanctuary")));
    }

    [Fact]
    public void V111_CradleTek_Runtime_Must_Not_Reference_AgentiCore_Directly()
    {
        var projectPath = GetProjectPath("CradleTek.Runtime");
        var projectText = File.ReadAllText(projectPath);

        Assert.DoesNotContain("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("Oan.Nexus.Control.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("Oan.Runtime.Materialization.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SoulFrame.Membrane.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SoulFrame.Bootstrap.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_SoulFrame_Membrane_May_Ask_Nexus_But_Not_Own_Runtime_Or_Custody()
    {
        var projectPath = GetProjectPath("SoulFrame.Membrane");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Nexus.Control.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Custody.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_SoulFrame_Bootstrap_Must_Depend_On_CradleTek_Custody()
    {
        var projectPath = GetProjectPath("SoulFrame.Bootstrap");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("CradleTek.Custody.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_CradleTek_Custody_Must_Use_CradleTek_Mantle_For_Mos_Oe_SelfGel_Bootstrap()
    {
        var projectPath = GetProjectPath("CradleTek.Custody");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("CradleTek.Mantle.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_CradleTek_Memory_Must_Remain_Substrate_And_Not_Depend_On_Cognition_Or_Runtime()
    {
        var projectPath = GetProjectPath("CradleTek.Memory");
        var projectText = File.ReadAllText(projectPath);

        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("Oan.Runtime", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("Oan.Nexus.Control", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_CradleTek_Memory_Must_Not_Probe_Repo_Local_Filesystem_Artifacts()
    {
        var lineRoot = GetLineRoot();
        var memoryRoot = Path.Combine(lineRoot, "src", "TechStack", "CradleTek", "CradleTek.Memory");
        var forbiddenTokens = new[]
        {
            "File.",
            "Directory.",
            "AppContext.BaseDirectory",
            "GetCurrentDirectory",
            "public_root",
            "corpus_index"
        };

        var violations = Directory
            .EnumerateFiles(memoryRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(file =>
            {
                var text = File.ReadAllText(file);
                return forbiddenTokens
                    .Where(token => text.Contains(token, StringComparison.Ordinal))
                    .Select(token => $"{Path.GetRelativePath(lineRoot, file)}: {token}");
            })
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_CradleTek_Memory_Must_Not_Reimplement_Custody_Or_Lexical_Adapters()
    {
        var lineRoot = GetLineRoot();
        var memoryRoot = Path.Combine(lineRoot, "src", "TechStack", "CradleTek", "CradleTek.Memory");
        var forbiddenTokens = new[]
        {
            "CreateCooledValidationHandle",
            "LexemeRegex",
            "IsSelfMarker",
            "IsContradictionMarker"
        };

        var violations = Directory
            .EnumerateFiles(memoryRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(file =>
            {
                var text = File.ReadAllText(file);
                return forbiddenTokens
                    .Where(token => text.Contains(token, StringComparison.Ordinal))
                    .Select(token => $"{Path.GetRelativePath(lineRoot, file)}: {token}");
            })
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_PrimeCryptic_Services_Must_Remain_Sanctuary_Native()
    {
        var projectPath = GetProjectPath("Oan.PrimeCryptic.Services");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Hosted_Llm_Must_Remain_Sanctuary_Native()
    {
        var projectPath = GetProjectPath("Oan.HostedLlm");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Trace_Persistence_Must_Remain_Sanctuary_Native()
    {
        var projectPath = GetProjectPath("Oan.Trace.Persistence");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_State_Modulation_Must_Not_Depend_On_AgentiCore_Or_CradleTek_Runtime()
    {
        var projectPath = GetProjectPath("Oan.State.Modulation");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.PrimeCryptic.Services.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Sli_Lisp_Must_Remain_Sanctuary_Native_Cryptic_Bundle()
    {
        var projectPath = GetProjectPath("SLI.Lisp");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Nexus_Control_Must_Remain_Sanctuary_Interface_Layer()
    {
        var projectPath = GetProjectPath("Oan.Nexus.Control");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Runtime_Materialization_Must_Remain_Sanctuary_Helper_Layer()
    {
        var projectPath = GetProjectPath("Oan.Runtime.Materialization");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("Oan.State.Modulation", projectText, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> FindTokenViolations(IEnumerable<string> forbiddenTokens)
    {
        var lineRoot = GetLineRoot();
        var violations = new List<string>();
        foreach (var file in EnumerateTrackedTextFiles())
        {
            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                if (text.Contains(token, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetRelativePath(lineRoot, file)}: {token}");
                }
            }
        }

        return violations;
    }

    private static IEnumerable<string> EnumerateTrackedTextFiles()
    {
        var lineRoot = GetLineRoot();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".csproj", ".md", ".ps1", ".props", ".sln"
        };

        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(lineRoot, "docs", "V1_1_1_MIGRATION_CHARTER.md"),
            Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md"),
            Path.Combine(lineRoot, "tests", "Sanctuary", "Oan.Audit.Tests", "Oan.Audit.Tests", "UnitTest1.cs")
        };

        return Directory
            .EnumerateFiles(lineRoot, "*", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => allowedExtensions.Contains(Path.GetExtension(file)))
            .Where(file => !excluded.Contains(file));
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Mortalis V1.1.1", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the V1.1.1 line root.");
    }

    private static string GetProjectPath(string projectName)
    {
        var lineRoot = GetLineRoot();
        var matches = Directory
            .EnumerateFiles(lineRoot, $"{projectName}.csproj", SearchOption.AllDirectories)
            .Where(static path => path.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                                  path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"Unable to resolve project '{projectName}'."),
            _ => throw new InvalidOperationException($"Resolved multiple project paths for '{projectName}'.")
        };
    }
}
