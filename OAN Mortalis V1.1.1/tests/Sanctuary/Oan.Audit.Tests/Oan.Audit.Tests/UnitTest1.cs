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
            "Oan.FirstRun",
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
    public void V111_CradleTek_Host_Must_Expose_Body_Boundary_Through_Runtime_Only()
    {
        var projectPath = GetProjectPath("CradleTek.Host");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("CradleTek.Runtime.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
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
    public void V111_AgentiCore_Must_Remain_Chambered_Cognition_And_Not_Depend_On_Body_Families()
    {
        var projectPath = GetProjectPath("AgentiCore");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SLI.Engine.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SLI.Ingestion.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("Oan.Runtime", projectText, StringComparison.Ordinal);
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
    public void V111_SoulFrame_Bootstrap_Must_Not_Depend_On_AgentiCore()
    {
        var projectPath = GetProjectPath("SoulFrame.Bootstrap");
        var projectText = File.ReadAllText(projectPath);

        Assert.DoesNotContain("AgentiCore.csproj", projectText, StringComparison.Ordinal);
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

    [Fact]
    public void V111_Runtime_Headless_Must_Compose_FirstRun_Projection()
    {
        var projectPath = GetProjectPath("Oan.Runtime.Headless");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("Oan.FirstRun.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_HostedLlm_Guard_Surface_Must_Not_Use_Legacy_ListeningFrame_Name()
    {
        var lineRoot = GetLineRoot();
        var legacyFieldName = "Listening" + "FrameActive";
        var legacyTypeName = "GovernedHostedLlm" + "ListeningFrame";
        var violations = Directory
            .EnumerateFiles(lineRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => File.ReadAllText(file).Contains(legacyFieldName, StringComparison.Ordinal) ||
                           File.ReadAllText(file).Contains(legacyTypeName, StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(lineRoot, file))
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void V111_BuildHoldUnlock_Docs_Are_Aligned_And_RuntimeSpec_Is_Present()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var unlockReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_HOLD_UNLOCK_READINESS.md");
        var runtimeSpecPath = Path.Combine(lineRoot, "docs", "LATE_PATH_RUNTIME_PROJECTION_SPEC.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var unlockReadinessText = File.ReadAllText(unlockReadinessPath);
        var runtimeSpecText = File.ReadAllText(runtimeSpecPath);

        var sharedMarkers = new[]
        {
            "`chapter-5: frame-now`",
            "`chapter-6: frame-now`",
            "`chapter-7: frame-now/spec-now`",
            "`chapter-8: frame-now/spec-now`",
            "`chapter-9: hold`",
            "`companion-tool-telemetry: admitted-optional-bounded`"
        };

        foreach (var marker in sharedMarkers)
        {
            Assert.Contains(marker, buildReadinessText, StringComparison.Ordinal);
            Assert.Contains(marker, unlockReadinessText, StringComparison.Ordinal);
        }

        var runtimeSpecMarkers = new[]
        {
            "GovernedSeedElementalBindingProjectionService",
            "GovernedSeedActualizationSealProjectionService",
            "GovernedSeedLivingAgentiCoreProjectionService",
            "FirstRunElementalBindingPacket",
            "FirstRunActualizationSealPacket",
            "FirstRunLivingAgentiCorePacket",
            "live `OE -> SoulFrame` loading behavior",
            "live `cOE -> AgentiCore` loading behavior",
            "no live `ListeningFrame` runtime behavior"
        };

        foreach (var marker in runtimeSpecMarkers)
        {
            Assert.Contains(marker, runtimeSpecText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Enrichment_Automation_Pathway_Is_Aligned_Across_Docs_And_Policy()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var unlockReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_HOLD_UNLOCK_READINESS.md");
        var pathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var companionTelemetryDocPath = Path.Combine(lineRoot, "docs", "COMPANION_TOOL_TELEMETRY_LANE.md");
        var cyclePolicyPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "build", "local-automation-cycle.json");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var unlockReadinessText = File.ReadAllText(unlockReadinessPath);
        var pathwayDocText = File.ReadAllText(pathwayDocPath);
        var companionTelemetryDocText = File.ReadAllText(companionTelemetryDocPath);
        var cyclePolicyText = File.ReadAllText(cyclePolicyPath);

        Assert.Contains("`v111-enrichment-automation: admitted-local-bounded`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`v111-enrichment-automation: admitted-local-bounded`", unlockReadinessText, StringComparison.Ordinal);
        Assert.Contains("`companion-tool-telemetry: admitted-optional-bounded`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`companion-tool-telemetry: admitted-optional-bounded`", unlockReadinessText, StringComparison.Ordinal);
        Assert.Contains("`automation-close-law: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`seeded-governance-build-admission-law: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`runtime-workbench-governance-law: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`bounded-ec-loop: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`engram-predicate-minting: hold`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`single-flight-main-worker: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`hourly-watchdog-reflection: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`daily-hitl-digest-office: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);

        var pathwayMarkers = new[]
        {
            "`v111-enrichment-automation: admitted-local-bounded`",
            "`companion-tool-telemetry: admitted-optional-bounded`",
            "seed-LLM",
            "production-pre-release",
            "`.hopng: optional-bounded`",
            "COMPANION_TOOL_TELEMETRY_LANE.md",
            "SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md",
            "RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md",
            "`buildAdmissionState = admitted-local-bounded`"
        };

        foreach (var marker in pathwayMarkers)
        {
            Assert.Contains(marker, pathwayDocText, StringComparison.Ordinal);
        }

        var companionTelemetryDocMarkers = new[]
        {
            "`companion-tool-telemetry: admitted-optional-bounded`",
            "Holographic Data Tool",
            "Trivium Forum",
            "Write-CompanionToolTelemetry.ps1"
        };

        foreach (var marker in companionTelemetryDocMarkers)
        {
            Assert.Contains(marker, companionTelemetryDocText, StringComparison.Ordinal);
        }

        var policyMarkers = new[]
        {
            "\"localAutomationCadenceMinutes\"",
            "\"targetTemporalConstitution\"",
            "\"single-flight-close-governed\"",
            "\"watchdogReflectionCadenceHours\"",
            "\"fault-recoverable\"",
            "\"optionalTriviumForumToolRoot\"",
            "\"companionToolTelemetryOutputRoot\"",
            "\"companionToolTelemetryStatePath\"",
            "\"v111EnrichmentPathwayOutputRoot\"",
            "\"v111EnrichmentPathwayStatePath\"",
            "\"schedulerTaskTopology\"",
            "\"mainWorkerTaskName\"",
            "\"watchdogTaskName\"",
            "\"dailyDigestTaskName\"",
            "\"pauseMainWorkerOnTerminalStates\"",
            "\"pauseMainWorkerOnBlocked\"",
            "\"watchdogOutputRoot\"",
            "\"watchdogStatePath\"",
            "\"keepCompanionToolTelemetryBundles\"",
            "\"keepV111EnrichmentPathwayBundles\""
        };

        foreach (var marker in policyMarkers)
        {
            Assert.Contains(marker, cyclePolicyText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_RuntimeWorkbench_Governance_Law_Is_Aligned_Across_Docs_And_Writers()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var workflowMapPath = Path.Combine(lineRoot, "docs", "V1_1_1_WORKFLOW_MILESTONE_MAP.md");
        var governanceLawPath = Path.Combine(lineRoot, "docs", "RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md");
        var resolverHelperPath = Path.Combine(repoRoot, "tools", "Resolve-OanWorkspacePath.ps1");
        var threadRootWriterPath = Path.Combine(repoRoot, "tools", "Write-IdentityInvariant-ThreadRoot.ps1");
        var workbenchSurfaceWriterPath = Path.Combine(repoRoot, "tools", "Write-Sanctuary-RuntimeWorkbenchSurface.ps1");
        var sessionLedgerWriterPath = Path.Combine(repoRoot, "tools", "Write-RuntimeWorkbench-SessionLedger.ps1");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var workflowMapText = File.ReadAllText(workflowMapPath);
        var governanceLawText = File.ReadAllText(governanceLawPath);
        var resolverHelperText = File.ReadAllText(resolverHelperPath);
        var threadRootWriterText = File.ReadAllText(threadRootWriterPath);
        var workbenchSurfaceWriterText = File.ReadAllText(workbenchSurfaceWriterPath);
        var sessionLedgerWriterText = File.ReadAllText(sessionLedgerWriterPath);

        Assert.Contains("RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md", workflowMapText, StringComparison.Ordinal);
        Assert.Contains("bind-worker-thread-root-governance-surface", governanceLawText, StringComparison.Ordinal);
        Assert.Contains("bounded-governance-analysis-workbench", governanceLawText, StringComparison.Ordinal);
        Assert.Contains("CradleTek -> SLI -> SoulFrame -> ListeningFrame -> Compass -> cOE", governanceLawText, StringComparison.Ordinal);
        Assert.Contains("engramPredicateEligibilityState", governanceLawText, StringComparison.Ordinal);
        Assert.Contains("Get-OanWorkspaceTouchPointFamilyResolution", resolverHelperText, StringComparison.Ordinal);

        var threadRootMarkers = new[]
        {
            "governanceRootId",
            "scopeClass",
            "bindState",
            "witnessStatus",
            "carryForwardPolicy",
            "researchHandOffPending"
        };

        foreach (var marker in threadRootMarkers)
        {
            Assert.Contains(marker, threadRootWriterText, StringComparison.Ordinal);
        }

        var workbenchSurfaceMarkers = new[]
        {
            "firstAdmittedSurfaceClass",
            "bounded-governance-analysis-workbench",
            "ecHabitationState",
            "outwardDuplexAuthorityState",
            "researchHandOffPending"
        };

        foreach (var marker in workbenchSurfaceMarkers)
        {
            Assert.Contains(marker, workbenchSurfaceWriterText, StringComparison.Ordinal);
        }

        var sessionLedgerMarkers = new[]
        {
            "sessionId",
            "projectSpaceId",
            "currentStateClass",
            "admissibilityStatus",
            "predicateLandingClass",
            "autobiographicalPromotionState",
            "engramPredicateEligibilityState",
            "continuityAnchor",
            "researchHandOffPending"
        };

        foreach (var marker in sessionLedgerMarkers)
        {
            Assert.Contains(marker, sessionLedgerWriterText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Enrichment_Automation_Scripts_Are_Wired_Into_Cycle_And_Retention()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var cycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-Cycle.ps1");
        var companionTelemetryScriptPath = Path.Combine(repoRoot, "tools", "Write-CompanionToolTelemetry.ps1");
        var companionTelemetryWrapperPath = Path.Combine(repoRoot, "tools", "Invoke-CompanionToolTelemetry.ps1");
        var writerScriptPath = Path.Combine(repoRoot, "tools", "Write-V111-EnrichmentPathway.ps1");
        var wrapperScriptPath = Path.Combine(repoRoot, "tools", "Invoke-V111-EnrichmentPathway.ps1");
        var retentionScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Automation-RetentionPruning.ps1");
        var seededBuildGovernanceScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Seeded-Build-Governance.ps1");

        var cycleScriptText = File.ReadAllText(cycleScriptPath);
        var companionTelemetryScriptText = File.ReadAllText(companionTelemetryScriptPath);
        var companionTelemetryWrapperText = File.ReadAllText(companionTelemetryWrapperPath);
        var writerScriptText = File.ReadAllText(writerScriptPath);
        var wrapperScriptText = File.ReadAllText(wrapperScriptPath);
        var retentionScriptText = File.ReadAllText(retentionScriptPath);
        var seededBuildGovernanceScriptText = File.ReadAllText(seededBuildGovernanceScriptPath);

        var cycleMarkers = new[]
        {
            "Resolve-OanWorkspacePath.ps1",
            "Write-CompanionToolTelemetry.ps1",
            "Write-V111-EnrichmentPathway.ps1",
            "lastCompanionToolTelemetryBundle",
            "companionToolTelemetryStatePath",
            "lastV111EnrichmentPathwayBundle",
            "v111EnrichmentPathwayStatePath",
            "[local-automation-cycle] CompanionToolTelemetry",
            "[local-automation-cycle] V111EnrichmentPathway"
        };

        foreach (var marker in cycleMarkers)
        {
            Assert.Contains(marker, cycleScriptText, StringComparison.Ordinal);
        }

        var companionTelemetryMarkers = new[]
        {
            "companion-tool-telemetry",
            "Holographic Data Tool",
            "Trivium Forum",
            "awaiting-audit-lane",
            "partial-companion-tool-telemetry"
        };

        foreach (var marker in companionTelemetryMarkers)
        {
            Assert.Contains(marker, companionTelemetryScriptText, StringComparison.Ordinal);
        }

        var companionTelemetryWrapperMarkers = new[]
        {
            "Invoke-Local-Automation-Cycle.ps1",
            "Write-CompanionToolTelemetry.ps1",
            "companionToolTelemetryStatePath"
        };

        foreach (var marker in companionTelemetryWrapperMarkers)
        {
            Assert.Contains(marker, companionTelemetryWrapperText, StringComparison.Ordinal);
        }

        var writerMarkers = new[]
        {
            "v111-enrichment-pathway",
            "continue-v111-enrichment-full-body-work",
            "pause-and-run-seed-llm-wrinkle-test",
            "optional-bounded",
            "companionToolTelemetryState",
            "holographicDataToolTelemetryState",
            "triviumForumTelemetryState"
        };

        foreach (var marker in writerMarkers)
        {
            Assert.Contains(marker, writerScriptText, StringComparison.Ordinal);
        }

        var wrapperMarkers = new[]
        {
            "Invoke-Local-Automation-Cycle.ps1",
            "Write-CompanionToolTelemetry.ps1",
            "Write-V111-EnrichmentPathway.ps1",
            "v111EnrichmentPathwayStatePath"
        };

        foreach (var marker in wrapperMarkers)
        {
            Assert.Contains(marker, wrapperScriptText, StringComparison.Ordinal);
        }

        var retentionMarkers = new[]
        {
            "lastCompanionToolTelemetryBundle",
            "companionToolTelemetryOutputRoot",
            "keepCompanionToolTelemetryBundles",
            "lastV111EnrichmentPathwayBundle",
            "v111EnrichmentPathwayOutputRoot",
            "keepV111EnrichmentPathwayBundles"
        };

        foreach (var marker in retentionMarkers)
        {
            Assert.Contains(marker, retentionScriptText, StringComparison.Ordinal);
        }

        var seededGovernanceMarkers = new[]
        {
            "Resolve-OanWorkspacePath.ps1",
            "Resolve-OanWorkspaceTouchPoint",
            "tool.localLlmPreflight"
        };

        foreach (var marker in seededGovernanceMarkers)
        {
            Assert.Contains(marker, seededBuildGovernanceScriptText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_LocalAutomationCycle_Reconciles_Scheduler_After_Close_State_Is_Written()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var cycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-Cycle.ps1");
        var cycleScriptText = File.ReadAllText(cycleScriptPath);

        var pauseDecisionIndex = cycleScriptText.LastIndexOf("$pauseForExplicitHitl = ", StringComparison.Ordinal);
        var summaryWriteIndex = cycleScriptText.LastIndexOf("Write-JsonFile -Path $summaryPath -Value $summary", StringComparison.Ordinal);
        var schedulerSyncIndex = cycleScriptText.LastIndexOf("$schedulerSyncScriptPath = Join-Path $resolvedRepoRoot 'tools\\Sync-Local-AutomationScheduler.ps1'", StringComparison.Ordinal);
        var schedulerReceiptIndex = cycleScriptText.LastIndexOf("$schedulerExecutionReceiptScriptPath = Join-Path $resolvedRepoRoot 'tools\\Write-SchedulerExecution-Receipt.ps1'", StringComparison.Ordinal);
        var taskStatusIndex = cycleScriptText.LastIndexOf("$taskStatusOutput = Invoke-ChildPowershellScript", StringComparison.Ordinal);

        Assert.True(pauseDecisionIndex >= 0);
        Assert.True(summaryWriteIndex >= 0);
        Assert.True(schedulerSyncIndex >= 0);
        Assert.True(schedulerReceiptIndex >= 0);
        Assert.True(taskStatusIndex >= 0);

        Assert.True(summaryWriteIndex > pauseDecisionIndex);
        Assert.True(schedulerSyncIndex > summaryWriteIndex);
        Assert.True(schedulerReceiptIndex > schedulerSyncIndex);
        Assert.True(taskStatusIndex > schedulerReceiptIndex);
    }

    [Fact]
    public void V111_LocalAutomation_ThreeOffice_Scheduler_Scripts_Are_Wired()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var installMainWorkerPath = Path.Combine(repoRoot, "tools", "Install-Local-AutomationCycleTask.ps1");
        var installWatchdogPath = Path.Combine(repoRoot, "tools", "Install-Local-AutomationWatchdogTask.ps1");
        var installDigestPath = Path.Combine(repoRoot, "tools", "Install-Local-AutomationDigestTask.ps1");
        var watchdogScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-Watchdog.ps1");
        var digestScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-HitlDigest.ps1");
        var schedulerSyncPath = Path.Combine(repoRoot, "tools", "Sync-Local-AutomationScheduler.ps1");
        var taskStatusPath = Path.Combine(repoRoot, "tools", "Write-Local-Automation-TaskStatus.ps1");

        var installMainWorkerText = File.ReadAllText(installMainWorkerPath);
        var installWatchdogText = File.ReadAllText(installWatchdogPath);
        var installDigestText = File.ReadAllText(installDigestPath);
        var watchdogScriptText = File.ReadAllText(watchdogScriptPath);
        var digestScriptText = File.ReadAllText(digestScriptPath);
        var schedulerSyncText = File.ReadAllText(schedulerSyncPath);
        var taskStatusText = File.ReadAllText(taskStatusPath);

        Assert.Contains("Mode: one-shot-main-worker", installMainWorkerText, StringComparison.Ordinal);
        Assert.DoesNotContain("-RepetitionInterval", installMainWorkerText, StringComparison.Ordinal);

        Assert.Contains("Invoke-Local-Automation-Watchdog.ps1", installWatchdogText, StringComparison.Ordinal);
        Assert.Contains("-RepetitionInterval", installWatchdogText, StringComparison.Ordinal);

        Assert.Contains("Invoke-Local-Automation-HitlDigest.ps1", installDigestText, StringComparison.Ordinal);
        Assert.Contains("-RepetitionInterval", installDigestText, StringComparison.Ordinal);

        Assert.Contains("Sync-Local-AutomationScheduler.ps1", watchdogScriptText, StringComparison.Ordinal);
        Assert.Contains("watchdogState", watchdogScriptText, StringComparison.Ordinal);

        Assert.Contains("Write-Release-Candidate-Digest.ps1", digestScriptText, StringComparison.Ordinal);
        Assert.Contains("nextDailyHitlDigestRunUtc", digestScriptText, StringComparison.Ordinal);

        Assert.Contains("mainWorkerTaskName", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("watchdogTaskName", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("dailyDigestTaskName", schedulerSyncText, StringComparison.Ordinal);

        Assert.Contains("'main-worker-cycle'", taskStatusText, StringComparison.Ordinal);
        Assert.Contains("'hourly-watchdog'", taskStatusText, StringComparison.Ordinal);
        Assert.Contains("'daily-hitl-digest'", taskStatusText, StringComparison.Ordinal);
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
