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
            "San.Common",
            "San.FirstRun",
            "San.Nexus.Control",
            "San.HostedLlm",
            "San.PrimeCryptic.Services",
            "San.Runtime.Headless",
            "San.Runtime.Materialization",
            "San.State.Modulation",
            "San.Trace.Persistence",
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
    public void V111_Production_File_And_Folder_Topology_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var topologyPath = Path.Combine(lineRoot, "docs", "PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var topologyText = File.ReadAllText(topologyPath);

        Assert.Contains("PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("constitutional host truth is not the same thing as a legacy staging root", topologyText, StringComparison.Ordinal);
        Assert.Contains("`src/Sanctuary/` currently contains legacy `Oan.*` project roots", topologyText, StringComparison.Ordinal);
        Assert.Contains("The target production topology for future templating is:", topologyText, StringComparison.Ordinal);
        Assert.Contains("`src/Sanctuary/Oan.* -> src/San/`", topologyText, StringComparison.Ordinal);
        Assert.Contains("`src/Sanctuary/SLI.* -> src/SLI/`", topologyText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_SideBySide_V121_InstallFirst_Line_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var buildScriptPath = Path.Combine(repoRoot, "build.ps1");
        var testScriptPath = Path.Combine(repoRoot, "test.ps1");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v121ReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var v121CharterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var v121LedgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var v121TopologyPath = Path.Combine(v121Root, "docs", "PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md");
        var v121ManifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var v121VersionPolicyPath = Path.Combine(v121Root, "build", "version-policy.json");
        var v121HygienePath = Path.Combine(v121Root, "tools", "verify-private-corpus.ps1");
        var v121SolutionPath = Path.Combine(v121Root, "San.sln");

        var buildScriptText = File.ReadAllText(buildScriptPath);
        var testScriptText = File.ReadAllText(testScriptPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var v121ReadinessText = File.ReadAllText(v121ReadinessPath);
        var v121CharterText = File.ReadAllText(v121CharterPath);
        var v121LedgerText = File.ReadAllText(v121LedgerPath);
        var v121TopologyText = File.ReadAllText(v121TopologyPath);
        var v121ManifestText = File.ReadAllText(v121ManifestPath);
        var v121VersionPolicyText = File.ReadAllText(v121VersionPolicyPath);
        var v121HygieneText = File.ReadAllText(v121HygienePath);
        var v121SolutionText = File.ReadAllText(v121SolutionPath);

        Assert.Contains("[string] $LineRoot = \"OAN Mortalis V1.1.1\"", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("[string] $LineRoot = \"OAN Mortalis V1.1.1\"", testScriptText, StringComparison.Ordinal);
        Assert.Contains("Join-Path $repoRoot $LineRoot", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("Join-Path $repoRoot $LineRoot", testScriptText, StringComparison.Ordinal);
        Assert.Contains("build\\line-manifest.json", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("build\\line-manifest.json", testScriptText, StringComparison.Ordinal);
        Assert.Contains("lineManifest.solutionPath", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("lineManifest.solutionPath", testScriptText, StringComparison.Ordinal);

        Assert.Contains("OAN Mortalis V1.2.1", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("install-first side-by-side build root", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("\"lineName\": \"OAN Mortalis V1.2.1\"", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"stateClass\": \"InstallState\"", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"umbrellaFamily\": \"San.*\"", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"reservedFutureToolFamily\": \"OAN.*\"", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"solutionPath\": \"San.sln\"", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"buildable\": true", v121ManifestText, StringComparison.Ordinal);
        Assert.Contains("\"currentVersion\": \"1.2.1\"", v121VersionPolicyText, StringComparison.Ordinal);

        Assert.Contains("install-first side-by-side line", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("not yet the final working form", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("`V1.1.1` remains the active executable truth", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("`San.*` becomes the umbrella", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("`OAN.*` is reserved", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("the first line-local solution graph under `San.sln`", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("the first line-local solution graph now resolves through `build/line-manifest.json`", v121ReadinessText, StringComparison.Ordinal);

        Assert.Contains("Install State -> Sanctuary State -> Sanctuary.GEL bootstrap", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.RTME issuance", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("first install before final working form", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("no archive of `V1.1.1` is required", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("`San.*` for the new line's umbrella composition family", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("`OAN.*` reserved for the future `OAN` tool", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("first line-local solution graph", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("`San.sln` as the first buildable sibling solution body", v121CharterText, StringComparison.Ordinal);

        Assert.Contains("production file release form", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("translational membrane law", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("`V1.2.1` should carry `San.*` as the umbrella family", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("`OAN.*` should stay reserved", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("First line-local solution graph", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("`San.sln`", v121LedgerText, StringComparison.Ordinal);

        Assert.Contains("`San.*` is the umbrella stack composition", v121TopologyText, StringComparison.Ordinal);
        Assert.Contains("`OAN.*` is reserved for the future `OAN` tool", v121TopologyText, StringComparison.Ordinal);
        Assert.Contains("src/", v121TopologyText, StringComparison.Ordinal);
        Assert.Contains("San/", v121TopologyText, StringComparison.Ordinal);
        Assert.Contains("`src/Sanctuary/Oan.* -> src/San/`", v121TopologyText, StringComparison.Ordinal);
        Assert.Contains("`namespace Oan.* -> namespace San.*`", v121TopologyText, StringComparison.Ordinal);

        Assert.Contains("OAN Mortalis V1.2.1\\.local\\private_corpus_root.txt", v121HygieneText, StringComparison.Ordinal);
        Assert.Contains("V1.2.1 hardened surface", v121HygieneText, StringComparison.Ordinal);
        Assert.Contains("\"San.Common\", \"src\\San\\San.Common\\San.Common.csproj\"", v121SolutionText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Spawn_Law_And_Id_Transition_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var spawnLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_GOA_CRADLETEK_GOA_AND_CGOA_SPAWN_LAW.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var spawnLawText = File.ReadAllText(spawnLawPath);

        Assert.Contains("SANCTUARY_GOA_CRADLETEK_GOA_AND_CGOA_SPAWN_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CradleTekID.GoA", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("cGoA", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("governing `CME` set/manifold", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("operator-bonded `CME` spawn/control surface", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("`SanctuaryID.GoA` carries governing `CME` set/manifold authority", charterText, StringComparison.Ordinal);
        Assert.Contains("`CradleTekID.GoA` is cradle-local operator-bonded `CME` spawn/control", charterText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary/CradleTek/`cGoA` spawn law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("RTME hosting, `SoulFrame` seal, and `CME` spawn", ledgerText, StringComparison.Ordinal);
        Assert.Contains("governing `CME`", ledgerText, StringComparison.Ordinal);
        Assert.Contains("operator-bonded", ledgerText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary -> SanctuaryID.GoA -> CradleTekID.GoA -> cGoA -> CME`", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA` is the governing `CME` set/manifold.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("exclusive operator-bonded", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("the operator-bonded `CME` path", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("`ID` Issuance And Ontological Transition", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("No runtime surface may be named with `*ID`", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("Template surfaces never use `*ID`.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("This clarification does not authorize active governing `CME` standing.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("No direct `Sanctuary -> CME` instantiation path.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("No `CradleTek -> governance-agent` origin path.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("No shared `cGoA` reuse across multiple `CME`s.", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("No `CradleTekID.GoA -> cGoA -> CME` path may be misread as governing", spawnLawText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Install_Bootstrap_And_Groupoid_Docs_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var rootAtlasPath = Path.Combine(v121Root, "docs", "ROOTATLAS_REMOTE_SOURCE_BOUNDARY.md");
        var gelBootstrapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_BOOTSTRAP_LAW.md");
        var rtmeLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_LISP_RTME_SERVICE_LAW.md");
        var soulFrameSealPath = Path.Combine(v121Root, "docs", "SOULFRAME_CGOA_CRYPTIC_SEAL_LAW.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var rootAtlasText = File.ReadAllText(rootAtlasPath);
        var gelBootstrapText = File.ReadAllText(gelBootstrapPath);
        var rtmeLawText = File.ReadAllText(rtmeLawPath);
        var soulFrameSealText = File.ReadAllText(soulFrameSealPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);

        Assert.Contains("ROOTATLAS_REMOTE_SOURCE_BOUNDARY.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_BOOTSTRAP_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_LISP_RTME_SERVICE_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SOULFRAME_CGOA_CRYPTIC_SEAL_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL bootstrap", charterText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.RTME issuance", charterText, StringComparison.Ordinal);
        Assert.Contains("SoulFrame cGoA bridge and cryptic seal prerequisites", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-install-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Remote `RootAtlas` source boundary", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` bootstrap law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.RTME` service law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`SoulFrame cGoA` cryptic seal law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Domain-and-spline categorical condensate", ledgerText, StringComparison.Ordinal);

        Assert.Contains("`RootAtlas` lives only on Research Servers.", rootAtlasText, StringComparison.Ordinal);
        Assert.Contains("No local install contains `RootAtlas`.", rootAtlasText, StringComparison.Ordinal);
        Assert.Contains("Those payloads hydrate `Sanctuary.GEL`", rootAtlasText, StringComparison.Ordinal);

        Assert.Contains("The first lawful local substrate is `Sanctuary.GEL`.", gelBootstrapText, StringComparison.Ordinal);
        Assert.Contains("No lawful `RTME` wake should occur before `Sanctuary.GEL` exists.", gelBootstrapText, StringComparison.Ordinal);

        Assert.Contains("The first Lisp `RTME` is a `Sanctuary` service layer.", rtmeLawText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.RTME` names the issued, certified, continuity-bearing hosted", rtmeLawText, StringComparison.Ordinal);
        Assert.Contains("`CradleTek` does not own that service", rtmeLawText, StringComparison.Ordinal);

        Assert.Contains("`SoulFrame` does not originate governance and does not spawn `CME`s.", soulFrameSealText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA -> CradleTekID.GoA`", soulFrameSealText, StringComparison.Ordinal);
        Assert.Contains("no seal exists without a fresh one-`CME` `cGoA`", soulFrameSealText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary -> CradleTek -> SoulFrame -> AgentiCore`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`Install -> Build -> Run -> Rest -> Exit`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("### `Sanctuary / Install`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`ROOTATLAS_REMOTE_SOURCE_BOUNDARY.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("### `CradleTek / Run`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_GOA_CRADLETEK_GOA_AND_CGOA_SPAWN_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("### `SoulFrame / Run`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SOULFRAME_CGOA_CRYPTIC_SEAL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Passive_Membrane_Source_Batch_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var buildPropsPath = Path.Combine(v121Root, "Directory.Build.props");
        var projectPath = Path.Combine(v121Root, "src", "San", "San.Common", "San.Common.csproj");
        var contractsPath = Path.Combine(v121Root, "src", "San", "San.Common", "SymbolicEnvelopeContracts.cs");
        var validationPath = Path.Combine(v121Root, "src", "San", "San.Common", "SymbolicEnvelopeValidation.cs");
        var decisionPolicyPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneDecisionPolicy.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var buildPropsText = File.ReadAllText(buildPropsPath);
        var projectText = File.ReadAllText(projectPath);
        var contractsText = File.ReadAllText(contractsPath);
        var validationText = File.ReadAllText(validationPath);
        var decisionPolicyText = File.ReadAllText(decisionPolicyPath);

        Assert.Contains("\"sourceMaterialized\": true", manifestText, StringComparison.Ordinal);
        Assert.Contains("first passive membrane-facing San.Common source batch", manifestText, StringComparison.Ordinal);

        Assert.Contains("Directory.Build.props", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("src/San/San.Common/San.Common.csproj", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("live runtime binding remains withheld", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("src/San/San.Common/", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-first-source-batch", ledgerText, StringComparison.Ordinal);
        Assert.Contains("First passive membrane-facing contract family", ledgerText, StringComparison.Ordinal);
        Assert.Contains("src/San/San.Common/", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("### `Sanctuary / Run`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("No symbolic product may bypass the membrane.", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("No runtime surface may self-authorize symbolic promotion.", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("This is the first membrane-facing source batch for `V1.2.1`.", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("The first carried membrane-facing source family now lives in:", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("<SanBuildVersion", buildPropsText, StringComparison.Ordinal);
        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", projectText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record SymbolicEnvelope(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum MembraneDecision", contractsText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", validationText, StringComparison.Ordinal);
        Assert.Contains("public interface ISymbolicEnvelopeValidator", validationText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultSymbolicEnvelopeValidator", validationText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", decisionPolicyText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneDecisionPolicy", decisionPolicyText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultMembraneDecisionPolicy", decisionPolicyText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Passive_Membrane_Corridor_And_Observational_Organism_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var adapterPath = Path.Combine(v121Root, "src", "San", "San.Common", "RawSymbolicEnvelopeAdapter.cs");
        var dispatcherPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneDecisionDispatcher.cs");
        var sinksPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneLaneSinks.cs");
        var witnessPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneLaneWitness.cs");
        var readModelPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneLaneReadModel.cs");
        var inspectionPath = Path.Combine(v121Root, "src", "San", "San.Common", "MembraneInspectionApi.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var adapterText = File.ReadAllText(adapterPath);
        var dispatcherText = File.ReadAllText(dispatcherPath);
        var sinksText = File.ReadAllText(sinksPath);
        var witnessText = File.ReadAllText(witnessPath);
        var readModelText = File.ReadAllText(readModelPath);
        var inspectionText = File.ReadAllText(inspectionPath);

        Assert.Contains("passive membrane corridor and observational organism", manifestText, StringComparison.Ordinal);

        Assert.Contains("RawSymbolicEnvelopeAdapter.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MembraneDecisionDispatcher.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MembraneLaneSinks.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MembraneLaneWitness.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MembraneLaneReadModel.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MembraneInspectionApi.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("raw ingress adapter -> envelope -> validator -> membrane decision -> dispatch", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("lane sink -> witness snapshot -> read model -> inspection", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("live runtime binding remains withheld", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("passive membrane corridor and observational organism", charterText, StringComparison.Ordinal);
        Assert.Contains("Passive membrane corridor and observational organism", ledgerText, StringComparison.Ordinal);
        Assert.Contains("RawSymbolicEnvelopeAdapter.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("MembraneInspectionApi.cs", ledgerText, StringComparison.Ordinal);

        Assert.Contains("passive membrane corridor or observational organism", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", adapterText, StringComparison.Ordinal);
        Assert.Contains("public sealed record RawSymbolicProduct(", adapterText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultRawSymbolicEnvelopeAdapter", adapterText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", dispatcherText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneDecisionDispatcher", dispatcherText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultMembraneDecisionDispatcher", dispatcherText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", sinksText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneLaneSink", sinksText, StringComparison.Ordinal);
        Assert.Contains("public abstract class MembraneLaneSinkBase", sinksText, StringComparison.Ordinal);
        Assert.Contains("public sealed class AcceptedMembraneLaneSink", sinksText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", witnessText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneLaneWitness", witnessText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultMembraneLaneWitness", witnessText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", readModelText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneLaneReadModel", readModelText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultMembraneLaneReadModel", readModelText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Common;", inspectionText, StringComparison.Ordinal);
        Assert.Contains("public interface IMembraneInspectionApi", inspectionText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultMembraneInspectionApi", inspectionText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_San_Nexus_Control_Project_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var solutionPath = Path.Combine(v121Root, "San.sln");
        var projectPath = Path.Combine(v121Root, "src", "San", "San.Nexus.Control", "San.Nexus.Control.csproj");
        var contractsPath = Path.Combine(v121Root, "src", "San", "San.Nexus.Control", "GovernedNexusControlContracts.cs");
        var servicePath = Path.Combine(v121Root, "src", "San", "San.Nexus.Control", "GovernedNexusControlService.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var solutionText = File.ReadAllText(solutionPath);
        var projectText = File.ReadAllText(projectPath);
        var contractsText = File.ReadAllText(contractsPath);
        var serviceText = File.ReadAllText(servicePath);

        Assert.Contains("first governance-bearing connective control project beneath the sovereignty ladder", manifestText, StringComparison.Ordinal);

        Assert.Contains("src/San/San.Nexus.Control/", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("GovernedNexusControlContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("GovernedNexusControlService.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first governance-bearing connective sibling", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("project beneath `San.sln`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CradleTekID.GoA", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("fresh `cGoA -> CME` spawn admissibility", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("src/San/San.Nexus.Control/", charterText, StringComparison.Ordinal);
        Assert.Contains("first governance-bearing sibling", charterText, StringComparison.Ordinal);

        Assert.Contains("First governance-bearing `San.*` sibling project", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GovernedNexusControlContracts.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GovernedNexusControlService.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("full mature bootstrap adjudicator", ledgerText, StringComparison.Ordinal);

        Assert.Contains("\"San.Nexus.Control\", \"src\\San\\San.Nexus.Control\\San.Nexus.Control.csproj\"", solutionText, StringComparison.Ordinal);

        Assert.Contains("<ProjectReference Include=\"..\\San.Common\\San.Common.csproj\" />", projectText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Nexus.Control;", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum NexusGovernanceOffice", contractsText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryGoa = 0", contractsText, StringComparison.Ordinal);
        Assert.Contains("CradleTekGoa = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record IssuedGovernanceSurface(", contractsText, StringComparison.Ordinal);
        Assert.Contains("AdmissibilityStatus", contractsText, StringComparison.Ordinal);

        Assert.Contains("namespace San.Nexus.Control;", serviceText, StringComparison.Ordinal);
        Assert.Contains("public interface IGovernedNexusControlService", serviceText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultGovernedNexusControlService", serviceText, StringComparison.Ordinal);
        Assert.Contains("EvaluateDownwardConnection(", serviceText, StringComparison.Ordinal);
        Assert.Contains("EvaluateCmeSpawn(", serviceText, StringComparison.Ordinal);
        Assert.Contains("issued-goa-downward-connection-admitted", serviceText, StringComparison.Ordinal);
        Assert.Contains("fresh-cgoa-cme-spawn-admitted", serviceText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Sli_Engine_Project_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var solutionPath = Path.Combine(v121Root, "San.sln");
        var projectPath = Path.Combine(v121Root, "src", "SLI", "SLI.Engine", "SLI.Engine.csproj");
        var contractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Engine", "CrypticFloorContracts.cs");
        var evaluatorPath = Path.Combine(v121Root, "src", "SLI", "SLI.Engine", "CrypticFloorEvaluator.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var solutionText = File.ReadAllText(solutionPath);
        var projectText = File.ReadAllText(projectPath);
        var contractsText = File.ReadAllText(contractsPath);
        var evaluatorText = File.ReadAllText(evaluatorPath);

        Assert.Contains("first symbolic engine project", manifestText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Engine/", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CrypticFloorContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CrypticFloorEvaluator.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first symbolic sibling project", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("passported symbolic envelopes and membrane decisions", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("before live hosted-bundle binding claims", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Engine/", charterText, StringComparison.Ordinal);
        Assert.Contains("first symbolic sibling project", charterText, StringComparison.Ordinal);

        Assert.Contains("First symbolic `SLI.*` sibling project", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CrypticFloorContracts.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CrypticFloorEvaluator.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("predicate-landing readiness and cryptic-floor outcomes before live hosted", ledgerText, StringComparison.Ordinal);
        Assert.Contains("bundle binding claims", ledgerText, StringComparison.Ordinal);

        Assert.Contains("first `SLI.Engine` sibling project", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("first hosted `SLI.Lisp` sibling project", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("first passive `SLI.Runtime` governance project", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("\"SLI.Engine\", \"src\\SLI\\SLI.Engine\\SLI.Engine.csproj\"", solutionText, StringComparison.Ordinal);
        Assert.Contains("<ProjectReference Include=\"..\\..\\San\\San.Common\\San.Common.csproj\" />", projectText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Engine;", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum PredicateLandingRouteKind", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum CrypticFloorDisposition", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record PredicateLandingRequest(", contractsText, StringComparison.Ordinal);
        Assert.Contains("MembraneDecision", contractsText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Engine;", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("public interface ICrypticFloorEvaluator", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("public sealed class CrypticFloorEvaluator", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("membrane-decision-must-admit-before-predicate-landing", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("sanctuary-gel-bootstrap-required-before-engine-landing", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("issued-sanctuary-rtme-service-required-before-engine-landing", evaluatorText, StringComparison.Ordinal);
        Assert.Contains("predicate-landing-surface-ready-via-bounded-ec-transit", evaluatorText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Sli_Lisp_Project_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var solutionPath = Path.Combine(v121Root, "San.sln");
        var projectPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "SLI.Lisp.csproj");
        var contractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "CrypticLispBundleContracts.cs");
        var servicePath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "GovernedCrypticLispBundleService.cs");
        var catalogPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "LispModuleCatalog.cs");
        var corePath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "core.lisp");
        var parserPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "parser.lisp");
        var transportPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "transport.lisp");
        var witnessPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "witness.lisp");
        var admissibilityPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "admissibility.lisp");
        var compassPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "compass.lisp");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var solutionText = File.ReadAllText(solutionPath);
        var projectText = File.ReadAllText(projectPath);
        var contractsText = File.ReadAllText(contractsPath);
        var serviceText = File.ReadAllText(servicePath);
        var catalogText = File.ReadAllText(catalogPath);
        var coreText = File.ReadAllText(corePath);
        var parserText = File.ReadAllText(parserPath);
        var transportText = File.ReadAllText(transportPath);
        var witnessText = File.ReadAllText(witnessPath);
        var admissibilityText = File.ReadAllText(admissibilityPath);
        var compassText = File.ReadAllText(compassPath);

        Assert.Contains("first hosted symbolic bundle project", manifestText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Lisp/", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CrypticLispBundleContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("GovernedCrypticLispBundleService.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("LispModuleCatalog.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first hosted symbolic bundle sibling project", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.RTME", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("core`, `parser`, `transport`, `witness`, `admissibility`, and `compass`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("full issued `RTME` wake", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Lisp/", charterText, StringComparison.Ordinal);
        Assert.Contains("first hosted symbolic bundle project", charterText, StringComparison.Ordinal);

        Assert.Contains("First hosted `SLI.Lisp` sibling project", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CrypticLispBundleContracts.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GovernedCrypticLispBundleService.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("LispModuleCatalog.cs", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.RTME", ledgerText, StringComparison.Ordinal);
        Assert.Contains("full hot `RTME` wake", ledgerText, StringComparison.Ordinal);
        Assert.Contains("live `EC` binding", ledgerText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Lisp/SLI.Lisp.csproj", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("a first hosted `SLI.Lisp` bundle body", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("a resident hosted module catalog", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("a canonical first floor-set for engine-facing symbolic support", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("a bundle receipt surface for hosted symbolic residency beneath", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("full issued `RTME` wake", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("first hosted `SLI.Lisp` sibling project", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("\"SLI.Lisp\", \"src\\SLI\\SLI.Lisp\\SLI.Lisp.csproj\"", solutionText, StringComparison.Ordinal);
        Assert.Contains("<EmbeddedResource Include=\"*.lisp\" />", projectText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Lisp;", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record HostedCrypticLispBundleReceipt(", contractsText, StringComparison.Ordinal);
        Assert.Contains("HostedByIssuedRuntime", contractsText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Lisp;", serviceText, StringComparison.Ordinal);
        Assert.Contains("public interface ICrypticLispBundleService", serviceText, StringComparison.Ordinal);
        Assert.Contains("CanonicalFloorModules", serviceText, StringComparison.Ordinal);
        Assert.Contains("\"SanctuaryID.RTME\"", serviceText, StringComparison.Ordinal);
        Assert.Contains("\"engine-facing-passive-hosted-bundle\"", serviceText, StringComparison.Ordinal);
        Assert.Contains("\"core.lisp\"", serviceText, StringComparison.Ordinal);
        Assert.Contains("\"compass.lisp\"", serviceText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Lisp;", catalogText, StringComparison.Ordinal);
        Assert.Contains("GetManifestResourceNames()", catalogText, StringComparison.Ordinal);
        Assert.Contains("Missing embedded Lisp module resource", catalogText, StringComparison.Ordinal);

        Assert.Contains("(defpackage :sli-core", coreText, StringComparison.Ordinal);
        Assert.Contains("(defun parse-expression", parserText, StringComparison.Ordinal);
        Assert.Contains("(defun transport-begin", transportText, StringComparison.Ordinal);
        Assert.Contains("(defun witness-begin", witnessText, StringComparison.Ordinal);
        Assert.Contains("(defun surface-begin", admissibilityText, StringComparison.Ordinal);
        Assert.Contains("(defstruct compass-state", compassText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Sli_Runtime_Project_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var escalationLawPath = Path.Combine(v121Root, "docs", "SLI_ESCALATION_STATE_LAW.md");
        var holdLawPath = Path.Combine(v121Root, "docs", "SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW.md");
        var returnLawPath = Path.Combine(v121Root, "docs", "SLI_GOVERNED_RETURN_RECEIPT_FAMILY_LAW.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var solutionPath = Path.Combine(v121Root, "San.sln");
        var projectPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "SLI.Runtime.csproj");
        var stateContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "EscalationStateContracts.cs");
        var transitionPolicyPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "EscalationTransitionPolicy.cs");
        var holdContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "HitlHoldWitnessTokenContracts.cs");
        var returnContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "GovernedReturnReceiptContracts.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var escalationLawText = File.ReadAllText(escalationLawPath);
        var holdLawText = File.ReadAllText(holdLawPath);
        var returnLawText = File.ReadAllText(returnLawPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var solutionText = File.ReadAllText(solutionPath);
        var projectText = File.ReadAllText(projectPath);
        var stateContractsText = File.ReadAllText(stateContractsPath);
        var transitionPolicyText = File.ReadAllText(transitionPolicyPath);
        var holdContractsText = File.ReadAllText(holdContractsPath);
        var returnContractsText = File.ReadAllText(returnContractsPath);

        Assert.Contains("first passive SLI governance project", manifestText, StringComparison.Ordinal);

        Assert.Contains("src/SLI/SLI.Runtime/", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SLI_ESCALATION_STATE_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SLI_GOVERNED_RETURN_RECEIPT_FAMILY_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("EscalationStateContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("HitlHoldWitnessTokenContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("GovernedReturnReceiptContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive `SLI` governance sibling project", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI_ESCALATION_STATE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SLI_GOVERNED_RETURN_RECEIPT_FAMILY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("src/SLI/SLI.Runtime/", charterText, StringComparison.Ordinal);

        Assert.Contains("`SLI` escalation state law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`SLI` `HitlHold` witness-token class law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`SLI` governed-return receipt family law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("First passive `SLI` governance sibling project", ledgerText, StringComparison.Ordinal);
        Assert.Contains("EscalationTransitionPolicy.cs", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SLI_ESCALATION_STATE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SLI_GOVERNED_RETURN_RECEIPT_FAMILY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("first passive escalation contract family", escalationLawText, StringComparison.Ordinal);
        Assert.Contains("src/SLI/SLI.Runtime/SLI.Runtime.csproj", escalationLawText, StringComparison.Ordinal);
        Assert.Contains("`HitlHold` may not lawfully release itself.", escalationLawText, StringComparison.Ordinal);
        Assert.Contains("`Refusal` and `Quarantine` remain distinct", escalationLawText, StringComparison.Ordinal);

        Assert.Contains("acknowledgement is not assent", holdLawText, StringComparison.Ordinal);
        Assert.Contains("AckToken", holdLawText, StringComparison.Ordinal);
        Assert.Contains("AssentToken", holdLawText, StringComparison.Ordinal);
        Assert.Contains("StewardAttestationToken", holdLawText, StringComparison.Ordinal);

        Assert.Contains("PermissionReceipt", returnLawText, StringComparison.Ordinal);
        Assert.Contains("TransformedInstructionReceipt", returnLawText, StringComparison.Ordinal);
        Assert.Contains("AcknowledgeAndRepresentPacket", returnLawText, StringComparison.Ordinal);
        Assert.Contains("BoundedLocalPackaging", returnLawText, StringComparison.Ordinal);

        Assert.Contains("first passive `SLI.Runtime` governance project", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("first passive field-query and recomposition family", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("\"SLI.Runtime\", \"src\\SLI\\SLI.Runtime\\SLI.Runtime.csproj\"", solutionText, StringComparison.Ordinal);
        Assert.Contains("<ProjectReference Include=\"..\\..\\San\\San.Common\\San.Common.csproj\" />", projectText, StringComparison.Ordinal);

        Assert.Contains("namespace SLI.Runtime;", stateContractsText, StringComparison.Ordinal);
        Assert.Contains("public enum SliEscalationState", stateContractsText, StringComparison.Ordinal);
        Assert.Contains("MotherFatherReview = 3", stateContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record SliEscalationTransitionRequest(", stateContractsText, StringComparison.Ordinal);

        Assert.Contains("public static class EscalationTransitionPolicy", transitionPolicyText, StringComparison.Ordinal);
        Assert.Contains("hitl-hold-may-not-release-without-explicit-witnessed-basis", transitionPolicyText, StringComparison.Ordinal);
        Assert.Contains("transition-admitted", transitionPolicyText, StringComparison.Ordinal);

        Assert.Contains("public enum HitlHoldWitnessTokenClass", holdContractsText, StringComparison.Ordinal);
        Assert.Contains("MotherFatherReviewToken = 4", holdContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record HitlHoldWitnessToken(", holdContractsText, StringComparison.Ordinal);

        Assert.Contains("public enum GovernedReturnReceiptFamily", returnContractsText, StringComparison.Ordinal);
        Assert.Contains("QuarantineNoticeReceipt = 7", returnContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record GovernedReturnReceipt(", returnContractsText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Field_Query_And_Recomposition_Family_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var baseFieldLawPath = Path.Combine(v121Root, "docs", "BASE_COGNITION_FIELD_PRESERVATION_AND_MODAL_INGRESS_LAW.md");
        var fieldQueryLawPath = Path.Combine(v121Root, "docs", "FIELD_QUERY_TENSION_AND_LAWFUL_RECOMPOSITION_LAW.md");
        var fieldQueryContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "FieldQueryContracts.cs");
        var fieldQueryPolicyPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "FieldQueryPolicy.cs");
        var tensionSummaryPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "QueryTensionSummary.cs");
        var recompositionContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "RecompositionCandidateContracts.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var baseFieldLawText = File.ReadAllText(baseFieldLawPath);
        var fieldQueryLawText = File.ReadAllText(fieldQueryLawPath);
        var fieldQueryContractsText = File.ReadAllText(fieldQueryContractsPath);
        var fieldQueryPolicyText = File.ReadAllText(fieldQueryPolicyPath);
        var tensionSummaryText = File.ReadAllText(tensionSummaryPath);
        var recompositionContractsText = File.ReadAllText(recompositionContractsPath);

        Assert.Contains("first passive preserved-field query and recomposition family", manifestText, StringComparison.Ordinal);

        Assert.Contains("BASE_COGNITION_FIELD_PRESERVATION_AND_MODAL_INGRESS_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("FIELD_QUERY_TENSION_AND_LAWFUL_RECOMPOSITION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("FieldQueryContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("FieldQueryPolicy.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("QueryTensionSummary.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RecompositionCandidateContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("query must not imply rewrite", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("recomposition candidates preserve provenance explicitly", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("later operator realization beneath preserved-field, candidate-evaluation, and", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("withheld-operator-seam law", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("BASE_COGNITION_FIELD_PRESERVATION_AND_MODAL_INGRESS_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("FIELD_QUERY_TENSION_AND_LAWFUL_RECOMPOSITION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("first passive preserved-field query and recomposition family", charterText, StringComparison.Ordinal);

        Assert.Contains("Base cognition field preservation and modal ingress law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Field query tension and lawful recomposition law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("First passive preserved-field query and recomposition family", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Operator realization layer for lawful cognitive circulation", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`BASE_COGNITION_FIELD_PRESERVATION_AND_MODAL_INGRESS_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`FIELD_QUERY_TENSION_AND_LAWFUL_RECOMPOSITION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("candidate-only recomposition before operator realization", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("field-preserving architecture rather than a flat store", baseFieldLawText, StringComparison.Ordinal);
        Assert.Contains("Query must not imply rewrite.", baseFieldLawText, StringComparison.Ordinal);
        Assert.Contains("Recomposition must remain subordinate to preserved-field law.", baseFieldLawText, StringComparison.Ordinal);

        Assert.Contains("IFieldQueryEngine", fieldQueryLawText, StringComparison.Ordinal);
        Assert.Contains("Query must not imply rewrite.", fieldQueryLawText, StringComparison.Ordinal);
        Assert.Contains("Recomposition must preserve provenance explicitly.", fieldQueryLawText, StringComparison.Ordinal);
        Assert.Contains("No recomposition candidate may bypass membrane re-entry.", fieldQueryLawText, StringComparison.Ordinal);

        Assert.Contains("first passive field-query and recomposition family", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("later operator realization beneath preserved-field, candidate-evaluation, and withheld-operator-seam law", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("public enum FieldQueryAxis", fieldQueryContractsText, StringComparison.Ordinal);
        Assert.Contains("TraceLineage = 6", fieldQueryContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record FieldQuery(", fieldQueryContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record FieldQueryResult(", fieldQueryContractsText, StringComparison.Ordinal);

        Assert.Contains("public interface IFieldQueryEngine", fieldQueryPolicyText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultFieldQueryEngine", fieldQueryPolicyText, StringComparison.Ordinal);
        Assert.Contains("QueryDoesNotRewrite", fieldQueryPolicyText, StringComparison.Ordinal);
        Assert.Contains("query must not imply rewrite", fieldQueryPolicyText, StringComparison.Ordinal);
        Assert.Contains("recomposition remains candidate-only", fieldQueryPolicyText, StringComparison.Ordinal);

        Assert.Contains("public enum QueryTensionState", tensionSummaryText, StringComparison.Ordinal);
        Assert.Contains("Withheld = 3", tensionSummaryText, StringComparison.Ordinal);
        Assert.Contains("public sealed record QueryTensionSummary(", tensionSummaryText, StringComparison.Ordinal);

        Assert.Contains("public enum RecompositionCandidateClass", recompositionContractsText, StringComparison.Ordinal);
        Assert.Contains("TensionPreservingMerge = 2", recompositionContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record RecompositionCandidate(", recompositionContractsText, StringComparison.Ordinal);
        Assert.Contains("RequiresMembraneReentry", recompositionContractsText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Recomposition_Candidate_Evaluation_Family_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var evaluationLawPath = Path.Combine(v121Root, "docs", "RECOMPOSITION_CANDIDATE_EVALUATION_LAW.md");
        var contractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "RecompositionCandidateEvaluationContracts.cs");
        var policyPath = Path.Combine(v121Root, "src", "SLI", "SLI.Runtime", "RecompositionCandidateEvaluationPolicy.cs");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var evaluationLawText = File.ReadAllText(evaluationLawPath);
        var contractsText = File.ReadAllText(contractsPath);
        var policyText = File.ReadAllText(policyPath);

        Assert.Contains("first passive candidate-evaluation family", manifestText, StringComparison.Ordinal);

        Assert.Contains("RECOMPOSITION_CANDIDATE_EVALUATION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RecompositionCandidateEvaluationContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RecompositionCandidateEvaluationPolicy.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("retain, cleave, defer, or reject", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("candidate-review seam without yet", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("claiming operator realization, child issuance, or live", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("RECOMPOSITION_CANDIDATE_EVALUATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("first passive candidate-evaluation family", charterText, StringComparison.Ordinal);

        Assert.Contains("Recomposition candidate evaluation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("First passive candidate-evaluation family", ledgerText, StringComparison.Ordinal);
        Assert.Contains("operator realization, ontological admission, or live form-or-cleave work", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`RECOMPOSITION_CANDIDATE_EVALUATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("passive candidate evaluation before operator realization", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("first passive candidate-evaluation family", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("later operator realization beneath preserved-field, candidate-evaluation, and withheld-operator-seam law", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("Candidate evaluation is ordered review, not silent promotion.", evaluationLawText, StringComparison.Ordinal);
        Assert.Contains("retain, cleave, defer, or reject", evaluationLawText, StringComparison.Ordinal);
        Assert.Contains("It does not replace the later `engram.form-or-cleave` lane.", evaluationLawText, StringComparison.Ordinal);

        Assert.Contains("public enum CandidateEvaluationBurdenAxis", contractsText, StringComparison.Ordinal);
        Assert.Contains("Continuity = 4", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum CandidateRecoveryClass", contractsText, StringComparison.Ordinal);
        Assert.Contains("ContinuityEquivalentRecovery = 2", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum CandidateEvaluationOutcome", contractsText, StringComparison.Ordinal);
        Assert.Contains("CleaveCandidate = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record RecompositionCandidateEvaluationDecision(", contractsText, StringComparison.Ordinal);

        Assert.Contains("public interface IRecompositionCandidateEvaluationPolicy", policyText, StringComparison.Ordinal);
        Assert.Contains("public sealed class DefaultRecompositionCandidateEvaluationPolicy", policyText, StringComparison.Ordinal);
        Assert.Contains("CandidateEvaluationOutcomeCodes", policyText, StringComparison.Ordinal);
        Assert.Contains("candidate evaluation may not normalize it into cognitive coherence", policyText, StringComparison.Ordinal);
        Assert.Contains("CandidateEvaluationOutcome.CleaveCandidate", policyText, StringComparison.Ordinal);
        Assert.Contains("EligibleForLaterOperatorRealization", policyText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Withheld_Operator_Seam_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var manifestPath = Path.Combine(v121Root, "build", "line-manifest.json");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var candidateEvalLawPath = Path.Combine(v121Root, "docs", "RECOMPOSITION_CANDIDATE_EVALUATION_LAW.md");
        var boundaryLawPath = Path.Combine(v121Root, "docs", "OPERATOR_REALIZATION_BOUNDARY_LAW.md");
        var witnessLawPath = Path.Combine(v121Root, "docs", "OPERATOR_REALIZATION_WITNESS_AND_PROHIBITION_LAW.md");
        var sequenceLawPath = Path.Combine(v121Root, "docs", "OPERATOR_REALIZATION_ADMISSION_SEQUENCE.md");

        var manifestText = File.ReadAllText(manifestPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var candidateEvalLawText = File.ReadAllText(candidateEvalLawPath);
        var boundaryLawText = File.ReadAllText(boundaryLawPath);
        var witnessLawText = File.ReadAllText(witnessLawPath);
        var sequenceLawText = File.ReadAllText(sequenceLawPath);

        Assert.Contains("withheld operator-realization seam", manifestText, StringComparison.Ordinal);

        Assert.Contains("OPERATOR_REALIZATION_BOUNDARY_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_REALIZATION_WITNESS_AND_PROHIBITION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_REALIZATION_ADMISSION_SEQUENCE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("later operator realization beneath preserved-field, candidate-evaluation, and", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("withheld-operator-seam law", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("live operator realization remains withheld", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("OPERATOR_REALIZATION_BOUNDARY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_REALIZATION_WITNESS_AND_PROHIBITION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_REALIZATION_ADMISSION_SEQUENCE.md", charterText, StringComparison.Ordinal);
        Assert.Contains("withheld operator-realization seam", charterText, StringComparison.Ordinal);

        Assert.Contains("Operator realization boundary law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Operator realization witness and prohibition law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Operator realization admission sequence", ledgerText, StringComparison.Ordinal);
        Assert.Contains("recomposition, candidate evaluation, and the withheld operator seam", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`OPERATOR_REALIZATION_BOUNDARY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`OPERATOR_REALIZATION_WITNESS_AND_PROHIBITION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`OPERATOR_REALIZATION_ADMISSION_SEQUENCE.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("withheld operator boundary, witness floor, and admission order", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("withheld operator-realization seam", candidateEvalLawText, StringComparison.Ordinal);

        Assert.Contains("Only the following may approach later operator realization", boundaryLawText, StringComparison.Ordinal);
        Assert.Contains("unevaluated recomposition candidates", boundaryLawText, StringComparison.Ordinal);
        Assert.Contains("`defer` or `reject` outcomes", boundaryLawText, StringComparison.Ordinal);
        Assert.Contains("No operator realization from unevaluated recomposition candidates.", witnessLawText, StringComparison.Ordinal);
        Assert.Contains("No authority widening during seam admission.", witnessLawText, StringComparison.Ordinal);
        Assert.Contains("No `cleave` posture may be treated here as child issuance.", witnessLawText, StringComparison.Ordinal);
        Assert.Contains("The sequence is:", sequenceLawText, StringComparison.Ordinal);
        Assert.Contains("1. preserved-field query", sequenceLawText, StringComparison.Ordinal);
        Assert.Contains("5. withheld operator seam", sequenceLawText, StringComparison.Ordinal);
        Assert.Contains("6. only later realized operator ingress", sequenceLawText, StringComparison.Ordinal);

        Assert.Contains("withheld operator-realization seam", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("later operator realization beneath preserved-field, candidate-evaluation, and withheld-operator-seam law", membraneLawText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Seam_Definition_Batch_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var boundaryLawPath = Path.Combine(v121Root, "docs", "OPERATOR_REALIZATION_BOUNDARY_LAW.md");
        var bindingContractPath = Path.Combine(v121Root, "docs", "SLI_ENGINE_LISP_BINDING_CONTRACT.md");
        var ingressPath = Path.Combine(v121Root, "docs", "OPERATOR_INGRESS_PRECONDITIONS.md");
        var refusalLawPath = Path.Combine(v121Root, "docs", "SEAM_REFUSAL_AND_RETURN_LAW.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var boundaryLawText = File.ReadAllText(boundaryLawPath);
        var bindingContractText = File.ReadAllText(bindingContractPath);
        var ingressText = File.ReadAllText(ingressPath);
        var refusalLawText = File.ReadAllText(refusalLawPath);

        Assert.Contains("SLI_ENGINE_LISP_BINDING_CONTRACT.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_INGRESS_PRECONDITIONS.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SEAM_REFUSAL_AND_RETURN_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("the first doctrine-only seam-definition trio", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`PredicateLandingRequest` is the minimal admissible seam carrier", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("live `SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME` binding remains withheld", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI_ENGINE_LISP_BINDING_CONTRACT.md", charterText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_INGRESS_PRECONDITIONS.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SEAM_REFUSAL_AND_RETURN_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("the doctrine-only seam-definition trio", charterText, StringComparison.Ordinal);
        Assert.Contains("before any RTME", charterText, StringComparison.Ordinal);

        Assert.Contains("`SLI.Engine -> SLI.Lisp` binding contract", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Operator ingress preconditions", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Seam refusal and return law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-seam-definition-law", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SLI_ENGINE_LISP_BINDING_CONTRACT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`OPERATOR_INGRESS_PRECONDITIONS.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SEAM_REFUSAL_AND_RETURN_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("binding-carrier minimums, object-level ingress", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("SLI_ENGINE_LISP_BINDING_CONTRACT.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_INGRESS_PRECONDITIONS.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("SEAM_REFUSAL_AND_RETURN_LAW.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("The current line does not yet materialize a separate", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("OPERATOR_INGRESS_PRECONDITIONS.md", boundaryLawText, StringComparison.Ordinal);
        Assert.Contains("object-level ingress checker", boundaryLawText, StringComparison.Ordinal);

        Assert.Contains("`SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("binding is not realization", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Engine.PredicateLandingRequest`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryGelHandle`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`IssuedRtmeHandle`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`RouteHandle`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`RouteKind`", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("raw symbolic products", bindingContractText, StringComparison.Ordinal);
        Assert.Contains("`Transform` may approach this seam only after", bindingContractText, StringComparison.Ordinal);

        Assert.Contains("`Disposition == CandidateOnly`", ingressText, StringComparison.Ordinal);
        Assert.Contains("`RequiresMembraneReentry == true`", ingressText, StringComparison.Ordinal);
        Assert.Contains("`Outcome == RetainCandidate`", ingressText, StringComparison.Ordinal);
        Assert.Contains("`EligibleForLaterOperatorRealization == true`", ingressText, StringComparison.Ordinal);
        Assert.Contains("`CleaveCandidate`", ingressText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize operator motion now.", ingressText, StringComparison.Ordinal);

        Assert.Contains("### Pre-Binding Refusal", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("### Engine-Entry Refusal Or Withhold", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("### Hosted-Bundle Or `RTME`-Side Refusal Or Withhold", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("invalid or incomplete passport", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("missing `SanctuaryGelHandle`", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("canonical floor-set not ready", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("No refusal at this seam may be a silent drop.", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("refusal stage", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("return destination", refusalLawText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Rtme_Skeleton_And_First_Working_Model_Trace_Path_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var rtmeLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_LISP_RTME_SERVICE_LAW.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var skeletonPath = Path.Combine(v121Root, "docs", "SANCTUARYID_RTME_SKELETON.md");
        var tracePath = Path.Combine(v121Root, "docs", "FIRST_WORKING_MODEL_TRACE_PATH.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var rtmeLawText = File.ReadAllText(rtmeLawPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var skeletonText = File.ReadAllText(skeletonPath);
        var traceText = File.ReadAllText(tracePath);

        Assert.Contains("SANCTUARYID_RTME_SKELETON.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_TRACE_PATH.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first doctrine-only `SanctuaryID.RTME` admission shell", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("admissibility may approach the seam; only receipt proves what happened there", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("live `RTME` binding and live operator motion remain withheld", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SANCTUARYID_RTME_SKELETON.md", charterText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_TRACE_PATH.md", charterText, StringComparison.Ordinal);
        Assert.Contains("RTME admission shell", charterText, StringComparison.Ordinal);
        Assert.Contains("first witnessed trace path", charterText, StringComparison.Ordinal);

        Assert.Contains("`SanctuaryID.RTME` skeleton", ledgerText, StringComparison.Ordinal);
        Assert.Contains("First working model trace path", ledgerText, StringComparison.Ordinal);
        Assert.Contains("without turning trace into execution", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SANCTUARYID_RTME_SKELETON.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`FIRST_WORKING_MODEL_TRACE_PATH.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("non-executive `SanctuaryID.RTME` admission shell", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("SANCTUARYID_RTME_SKELETON.md", rtmeLawText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_TRACE_PATH.md", rtmeLawText, StringComparison.Ordinal);

        Assert.Contains("SANCTUARYID_RTME_SKELETON.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_TRACE_PATH.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("one witnessed path from candidate evaluation", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("live engine-to-hosted-bundle binding", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("skeleton standing is not realization standing", skeletonText, StringComparison.Ordinal);
        Assert.Contains("admissibility may approach the seam; only receipt proves what happened", skeletonText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Engine.PredicateLandingRequest`", skeletonText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Engine.CrypticFloorEvaluation`", skeletonText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Lisp.HostedCrypticLispBundleReceipt`", skeletonText, StringComparison.Ordinal);
        Assert.Contains("Receipt proves what happened at the boundary.", skeletonText, StringComparison.Ordinal);

        Assert.Contains("trace is not execution", traceText, StringComparison.Ordinal);
        Assert.Contains("`RecompositionCandidate -> RecompositionCandidateEvaluationDecision -> membrane re-entry -> PredicateLandingRequest -> CrypticFloorEvaluation -> HostedCrypticLispBundleReceipt`", traceText, StringComparison.Ordinal);
        Assert.Contains("`Outcome == RetainCandidate`", traceText, StringComparison.Ordinal);
        Assert.Contains("`PredicateLandingReady == true`", traceText, StringComparison.Ordinal);
        Assert.Contains("`OutcomeCode == \"predicate-landing-ready\"`", traceText, StringComparison.Ordinal);
        Assert.Contains("`OutcomeCode == \"issued-rtme-service-required\"`", traceText, StringComparison.Ordinal);
        Assert.Contains("At that point the path may not continue toward hosted receipt.", traceText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Lisp_Csharp_Binding_Schema_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var rtmeLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_LISP_RTME_SERVICE_LAW.md");
        var membraneLawPath = Path.Combine(v121Root, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var skeletonPath = Path.Combine(v121Root, "docs", "SANCTUARYID_RTME_SKELETON.md");
        var tracePath = Path.Combine(v121Root, "docs", "FIRST_WORKING_MODEL_TRACE_PATH.md");
        var schemaPath = Path.Combine(v121Root, "docs", "LISP_CSHARP_BINDING_SCHEMA.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var rtmeLawText = File.ReadAllText(rtmeLawPath);
        var membraneLawText = File.ReadAllText(membraneLawPath);
        var skeletonText = File.ReadAllText(skeletonPath);
        var traceText = File.ReadAllText(tracePath);
        var schemaText = File.ReadAllText(schemaPath);

        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first descriptive Lisp/C# binding schema", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`This schema governs carriage, not consequence.`", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", charterText, StringComparison.Ordinal);
        Assert.Contains("descriptive Lisp/C# carriage schema", charterText, StringComparison.Ordinal);

        Assert.Contains("Lisp/C# binding schema", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-carriage-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("without granting live binding or", ledgerText, StringComparison.Ordinal);
        Assert.Contains("runtime consequence.", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`LISP_CSHARP_BINDING_SCHEMA.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("cross-language carriage of already-admitted seam nouns", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", rtmeLawText, StringComparison.Ordinal);
        Assert.Contains("It describes carriage across Lisp and C# without authorizing live service", rtmeLawText, StringComparison.Ordinal);

        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("This schema governs carriage, not consequence.", membraneLawText, StringComparison.Ordinal);
        Assert.Contains("preservation rules for identity-bearing and receipt-bearing fields", membraneLawText, StringComparison.Ordinal);

        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", skeletonText, StringComparison.Ordinal);
        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", traceText, StringComparison.Ordinal);

        Assert.Contains("This schema governs carriage, not consequence.", schemaText, StringComparison.Ordinal);
        Assert.Contains("`San.Common.SymbolicEnvelope`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`San.Common.MembraneDecision`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`San.Common.MembraneDecisionResult`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Engine.PredicateLandingRequest`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Engine.CrypticFloorEvaluation`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Lisp.HostedCrypticLispBundleReceipt`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Runtime.RecompositionCandidate`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SLI.Runtime.RecompositionCandidateEvaluationDecision`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`Origin`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`TraceId`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryGelHandle`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`IssuedRtmeHandle`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`RouteHandle`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`RouteKind`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`OutcomeCode`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`GovernanceTrace`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`BundleHandle`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`CanonicalFloorSetReady`", schemaText, StringComparison.Ordinal);
        Assert.Contains("collapsing refusal into `null`, `false`, empty collections, or opaque host", schemaText, StringComparison.Ordinal);
        Assert.Contains("A receipted path in Lisp must remain receipted and legible in C#.", schemaText, StringComparison.Ordinal);
        Assert.Contains("A refusal in Lisp must remain visible as refusal in C#.", schemaText, StringComparison.Ordinal);
        Assert.Contains("This schema does not authorize:", schemaText, StringComparison.Ordinal);
        Assert.Contains("The seam nouns now have a documented carrier mapping.", schemaText, StringComparison.Ordinal);
        Assert.Contains("Nothing in this schema can be read as new runtime permission.", schemaText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Pre_Cme_Substrate_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var rootInstallPath = Path.Combine(v121Root, "docs", "SANCTUARY_ROOT_INSTALL_SURFACE.md");
        var templateLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_TEMPLATE_ADOPTION_LAW.md");
        var serviceRegisterPath = Path.Combine(v121Root, "docs", "SANCTUARY_INTENDED_SERVICE_REGISTER.md");
        var substrateAuditPath = Path.Combine(v121Root, "docs", "SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var rootInstallText = File.ReadAllText(rootInstallPath);
        var templateLawText = File.ReadAllText(templateLawPath);
        var serviceRegisterText = File.ReadAllText(serviceRegisterPath);
        var substrateAuditText = File.ReadAllText(substrateAuditPath);

        Assert.Contains("SANCTUARY_ROOT_INSTALL_SURFACE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_TEMPLATE_ADOPTION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_INTENDED_SERVICE_REGISTER.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first pre-CME substrate cluster", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("does not authorize adoption, activation, or local sovereignty", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("descriptive Lisp/C# carriage schema -> pre-CME substrate clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("pre-CME substrate clarification -> SanctuaryID.GoA governing-set clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA governance-root and cGEL stack-map clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_ROOT_INSTALL_SURFACE.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_TEMPLATE_ADOPTION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_INTENDED_SERVICE_REGISTER.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md", charterText, StringComparison.Ordinal);
        Assert.Contains("first pre-CME substrate clarification of what is given, offered, and possible", charterText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary root install surface", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary template adoption law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary intended service register", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary pre-CME substrate audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-pre-cme-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("planned or disabled", ledgerText, StringComparison.Ordinal);
        Assert.Contains("activation.", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SANCTUARY_ROOT_INSTALL_SURFACE.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_TEMPLATE_ADOPTION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_INTENDED_SERVICE_REGISTER.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("absence of silent adoption and implicit activation", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("GoA must not awaken into ambiguity; it must awaken into provenance.", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`immutable`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`extendable`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`reference-only`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`read`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`extend-with-receipt`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("`never-modify`", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("logical `Lucid Research Corpus` standing", rootInstallText, StringComparison.Ordinal);
        Assert.Contains("installation is not authorship", rootInstallText, StringComparison.Ordinal);

        Assert.Contains("No template becomes locally binding by mere presence.", templateLawText, StringComparison.Ordinal);
        Assert.Contains("`mandatory`", templateLawText, StringComparison.Ordinal);
        Assert.Contains("`optional`", templateLawText, StringComparison.Ordinal);
        Assert.Contains("`conditional`", templateLawText, StringComparison.Ordinal);
        Assert.Contains("Silent adoption is unlawful.", templateLawText, StringComparison.Ordinal);
        Assert.Contains("Lawful non-adoption is not dishonesty, failure, or silent default.", templateLawText, StringComparison.Ordinal);

        Assert.Contains("`planned`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("`installed-disabled`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("`templated-disabled`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("`authorized`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("reserved grammar", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("used by zero actual entries", serviceRegisterText, StringComparison.Ordinal);
        Assert.DoesNotContain("Current status: `authorized`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("Current status: `templated-disabled`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("Current status: `installed-disabled`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("read-only root tools such as `line-audit-report`", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("Presence is not activation.", serviceRegisterText, StringComparison.Ordinal);

        Assert.Contains("`given`", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("`offered`", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("`possible`", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("ambiguous classification", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("duplicate classification", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("silent adoption", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("implicit activation", substrateAuditText, StringComparison.Ordinal);
        Assert.Contains("any actual service entry using `authorized`", substrateAuditText, StringComparison.Ordinal);

        Assert.Contains("`first-working-model-pre-cme-substrate: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("pre-CME substrate cluster as specified", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("That carriage note now supports the next pre-CME substrate cluster:", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_ROOT_INSTALL_SURFACE.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_TEMPLATE_ADOPTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_INTENDED_SERVICE_REGISTER.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("first governing `CME` presence is even", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize template adoption.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize service activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize local sovereignty.", gateText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_SanctuaryIdGoa_Governing_Cme_Set_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var spawnLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_GOA_CRADLETEK_GOA_AND_CGOA_SPAWN_LAW.md");
        var setLawPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md");
        var braidPath = Path.Combine(v121Root, "docs", "MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md");
        var placementWithheldPath = Path.Combine(v121Root, "docs", "CME_PLACEMENT_WITHHELD_LAW.md");
        var nonAssumptionPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_NON_ASSUMPTION_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_GOVERNING_CME_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var spawnLawText = File.ReadAllText(spawnLawPath);
        var setLawText = File.ReadAllText(setLawPath);
        var braidText = File.ReadAllText(braidPath);
        var placementWithheldText = File.ReadAllText(placementWithheldPath);
        var nonAssumptionText = File.ReadAllText(nonAssumptionPath);
        var auditText = File.ReadAllText(auditPath);

        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CME_PLACEMENT_WITHHELD_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_NON_ASSUMPTION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_GOVERNING_CME_AUDIT.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-side governing-set clarification only", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("does not authorize CradleTek spawn, governing `CME` placement, Steward", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("pre-CME substrate clarification -> SanctuaryID.GoA governing-set clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA governing-set clarification -> SanctuaryID.GoA governance-root and cGEL stack-map clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CME_PLACEMENT_WITHHELD_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_NON_ASSUMPTION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_GOVERNING_CME_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("SanctuaryID.GoA governing CME set law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Mother/Father Prime/Cryptic braid posture", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CME placement withheld law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Pre-CradleTek non-assumption law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Pre-CradleTek governing CME audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-pre-cradletek-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("This is prepared standing only and does not imply active governing `CME`", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CME_PLACEMENT_WITHHELD_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PRE_CRADLETEK_NON_ASSUMPTION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PRE_CRADLETEK_GOVERNING_CME_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("prepared Sanctuary-side governing standing", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("the operator-bonded `CME` path", spawnLawText, StringComparison.Ordinal);
        Assert.Contains("This clarification does not authorize active governing `CME` standing.", spawnLawText, StringComparison.Ordinal);

        Assert.Contains("`SanctuaryID.GoA` is the governing `CME` set/manifold.", setLawText, StringComparison.Ordinal);
        Assert.Contains("`CradleTekID.GoA` remains the operator-bonded `CME` spawn/control surface.", setLawText, StringComparison.Ordinal);
        Assert.Contains("no new named standing-variable set", setLawText, StringComparison.Ordinal);
        Assert.Contains("`CmePlacementWithheld` therefore remains active constitutional restraint.", setLawText, StringComparison.Ordinal);

        Assert.Contains("Mother/Father posture is not Steward governance.", braidText, StringComparison.Ordinal);
        Assert.Contains("Prime recognizes and witnesses. Cryptic prepares and works.", braidText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-side supervisory braid does not imply live CradleTek authority.", braidText, StringComparison.Ordinal);

        Assert.Contains("`CmePlacementWithheld` is default law, not temporary omission.", placementWithheldText, StringComparison.Ordinal);
        Assert.Contains("operator-bonded spawn law does not satisfy governing placement law.", placementWithheldText, StringComparison.Ordinal);
        Assert.Contains("Until those conditions are real, governing placement must remain withheld.", placementWithheldText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary readiness is not CradleTek authorization.", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("governing-set description is not active governing `CME` existence.", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("Any uncertainty must resolve to explicit refusal.", nonAssumptionText, StringComparison.Ordinal);

        Assert.Contains("any implied active governing `CME` standing", auditText, StringComparison.Ordinal);
        Assert.Contains("any implied CradleTek authorization from Sanctuary-side law", auditText, StringComparison.Ordinal);
        Assert.Contains("any collapse of Mother/Father into Steward", auditText, StringComparison.Ordinal);
        Assert.Contains("any loss of `CmePlacementWithheld`", auditText, StringComparison.Ordinal);
        Assert.Contains("any silent move from prepared set/manifold into active office", auditText, StringComparison.Ordinal);

        Assert.Contains("`first-working-model-sanctuaryid-goa-governing-set: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-side `SanctuaryID.GoA`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("That substrate cluster now supports the next `SanctuaryID.GoA`", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CME_PLACEMENT_WITHHELD_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_NON_ASSUMPTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_GOVERNING_CME_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before governing `CME` can be discussed as active", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize CradleTek spawn.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize governing `CME` placement.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Steward governance.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize activation.", gateText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_SanctuaryIdGoa_Governance_Root_And_Cgel_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v111WorkbenchLawPath = Path.Combine(lineRoot, "docs", "RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var rootAtlasBoundaryPath = Path.Combine(v121Root, "docs", "ROOTATLAS_REMOTE_SOURCE_BOUNDARY.md");
        var gelBootstrapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_BOOTSTRAP_LAW.md");
        var governingSetPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md");
        var rootLawPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md");
        var predicateBoundaryPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_PREDICATE_TRUTH_BOUNDARY_LAW.md");
        var refusalLawPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_DEFAULT_REFUSAL_LAW.md");
        var authorityLawPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_AUTHORITY_AND_RECEIPT_LAW.md");
        var cgelLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md");
        var stackMapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md");
        var auditPath = Path.Combine(v121Root, "docs", "SANCTUARYID_GOA_PRE_CRADLETEK_AUDIT.md");
        var ingressServicePath = Path.Combine(lineRoot, "src", "TechStack", "CradleTek", "CradleTek.Runtime", "CradleTek.Runtime", "GovernedSeedRuntimeService.cs");
        var crypticFloorEvaluatorPath = Path.Combine(v121Root, "src", "SLI", "SLI.Engine", "CrypticFloorEvaluator.cs");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v111WorkbenchLawText = File.ReadAllText(v111WorkbenchLawPath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var rootAtlasBoundaryText = File.ReadAllText(rootAtlasBoundaryPath);
        var gelBootstrapText = File.ReadAllText(gelBootstrapPath);
        var governingSetText = File.ReadAllText(governingSetPath);
        var rootLawText = File.ReadAllText(rootLawPath);
        var predicateBoundaryText = File.ReadAllText(predicateBoundaryPath);
        var refusalLawText = File.ReadAllText(refusalLawPath);
        var authorityLawText = File.ReadAllText(authorityLawPath);
        var cgelLawText = File.ReadAllText(cgelLawPath);
        var stackMapText = File.ReadAllText(stackMapPath);
        var auditText = File.ReadAllText(auditPath);
        var ingressServiceText = File.ReadAllText(ingressServicePath);
        var crypticFloorEvaluatorText = File.ReadAllText(crypticFloorEvaluatorPath);

        Assert.Contains("`first-working-model-sanctuaryid-goa-root-and-cgel: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA` governance-root and", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` stack-map cluster while preserving", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("preserving predicate-mint `hold`", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("That governing-set clarification cluster now supports the next", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PREDICATE_TRUTH_BOUNDARY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_DEFAULT_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_AUTHORITY_AND_RECEIPT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PRE_CRADLETEK_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim of first working governance root is", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize active governing `CME` standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize service authorization.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Trivium standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("Governance root standing may exist before CradleTek-dependent governing life,", gateText, StringComparison.Ordinal);

        Assert.Contains("Governance-Root And cGEL Clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("governance root now binds only to already-real Atlas/SLI predicate truth", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` is the cryptic build chamber", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` and `Sanctuary.cMoS` now form the lawful governance-memory", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA` governance-root and `Sanctuary.cGEL` stack-map", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SanctuaryID.GoA governance-root and cGEL stack-map clarification -> Sanctuary.GEL semantic intake clarification", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PREDICATE_TRUTH_BOUNDARY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_DEFAULT_REFUSAL_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_AUTHORITY_AND_RECEIPT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PRE_CRADLETEK_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("SanctuaryID.GoA governance root law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA predicate truth boundary law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA default refusal law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA authority and receipt law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.cGEL cryptic build chamber law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary GEL/cGEL/MoS governance stack map", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SanctuaryID.GoA pre-CradleTek audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-pre-cradletek-goa-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("does not imply spawn, active governing life, service authorization, or", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARYID_GOA_PREDICATE_TRUTH_BOUNDARY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARYID_GOA_DEFAULT_REFUSAL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARYID_GOA_AUTHORITY_AND_RECEIPT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARYID_GOA_PRE_CRADLETEK_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("governance-root clarification bound to already-real lower predicate truth", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` build chamber naming", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("`RootAtlas` lives only on Research Servers.", rootAtlasBoundaryText, StringComparison.Ordinal);
        Assert.Contains("No local install contains `RootAtlas`.", rootAtlasBoundaryText, StringComparison.Ordinal);
        Assert.Contains("The lawful install-side substrate is:", rootAtlasBoundaryText, StringComparison.Ordinal);

        Assert.Contains("The first lawful local substrate is `Sanctuary.GEL`.", gelBootstrapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` is bootstrap substrate, not `Sanctuary.cGEL`.", gelBootstrapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` is a later cryptic build chamber, not the first install", gelBootstrapText, StringComparison.Ordinal);

        Assert.Contains("separate governance-root clarification", governingSetText, StringComparison.Ordinal);
        Assert.Contains("still does not imply active governing `CME`", governingSetText, StringComparison.Ordinal);

        Assert.Contains("predicate truth that already", rootLawText, StringComparison.Ordinal);
        Assert.Contains("later CradleTek or CME", rootLawText, StringComparison.Ordinal);
        Assert.Contains("ingress-engrammitization is real; engram-predicate promotion is still", rootLawText, StringComparison.Ordinal);
        Assert.Contains("Governance root standing is not governing `CME` presence.", rootLawText, StringComparison.Ordinal);
        Assert.Contains("Governance root standing is not spawn standing.", rootLawText, StringComparison.Ordinal);
        Assert.Contains("`CmePlacementWithheld` remains active.", rootLawText, StringComparison.Ordinal);

        Assert.Contains("`RootAtlas` remains remote source ancestry", predicateBoundaryText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` is the first lawful local substrate", predicateBoundaryText, StringComparison.Ordinal);
        Assert.Contains("ingress-engrammitization is already real", predicateBoundaryText, StringComparison.Ordinal);
        Assert.Contains("predicate landing is already real", predicateBoundaryText, StringComparison.Ordinal);
        Assert.Contains("engram-predicate promotion", predicateBoundaryText, StringComparison.Ordinal);
        Assert.Contains("engram minting", predicateBoundaryText, StringComparison.Ordinal);

        Assert.Contains("predicate promotion that is still withheld", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("active governing `CME` presence", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("CradleTek-dependent life", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("service authorization not explicitly receipted", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("Refusal must be visible and receipted.", refusalLawText, StringComparison.Ordinal);

        Assert.Contains("provenance recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` substrate recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("ingress-engrammitization recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("predicate-landing recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("governance binding recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("refusal recognized", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("service authorization", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("active governing `CME` existence", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("Steward governance", authorityLawText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.cGEL` is the cryptic build chamber.", cgelLawText, StringComparison.Ordinal);
        Assert.Contains("`SLI`", cgelLawText, StringComparison.Ordinal);
        Assert.Contains("Prime/Cryptic braid work", cgelLawText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA` governance binding", cgelLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` remains the first lawful local substrate beneath", cgelLawText, StringComparison.Ordinal);

        Assert.Contains("`Install`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("templating to `Sanctuary.GEL`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("building to `Sanctuary.cGEL` (`SLI`, Prime/Cryptic braid,", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.PrimeGovernance` (`Mother`)", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.CrypticGovernance` (`Father`)", stackMapText, StringComparison.Ordinal);
        Assert.Contains("UI/UX: `TriviumForum`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` is the legal data store / local governmental contract seat.", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`OE` and `SelfGEL` may stand in `Sanctuary.MoS` structurally", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`cOE` and `cSelfGEL` may surface in `Sanctuary.cMoS` derivationally", stackMapText, StringComparison.Ordinal);
        Assert.Contains("not current GoA", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`personal`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`industrial`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`commercial`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`private`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`governmental`", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`special cases`", stackMapText, StringComparison.Ordinal);

        Assert.Contains("any implied active governing `CME` standing", auditText, StringComparison.Ordinal);
        Assert.Contains("any borrowed predicate maturity", auditText, StringComparison.Ordinal);
        Assert.Contains("any implied engram-predicate promotion or minting", auditText, StringComparison.Ordinal);
        Assert.Contains("any implied CradleTek spawn or life", auditText, StringComparison.Ordinal);
        Assert.Contains("any service authorization", auditText, StringComparison.Ordinal);
        Assert.Contains("any use of `TriviumForum` as if it already stood", auditText, StringComparison.Ordinal);
        Assert.Contains("any collapse of `Sanctuary.GEL` into `Sanctuary.cGEL`", auditText, StringComparison.Ordinal);
        Assert.Contains("any new standing-variable set", auditText, StringComparison.Ordinal);
        Assert.Contains("any loss of `CmePlacementWithheld`", auditText, StringComparison.Ordinal);

        Assert.Contains("engram-predicate-minting: hold", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first engrammitization pass at ingress", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("## Law 3. Seed Predicate Landing Membrane", v111WorkbenchLawText, StringComparison.Ordinal);
        Assert.Contains("engram minting", v111WorkbenchLawText, StringComparison.Ordinal);
        Assert.Contains("IGovernedSeedSanctuaryIngressEngrammitizationService", ingressServiceText, StringComparison.Ordinal);
        Assert.Contains("PredicateLandingReady", crypticFloorEvaluatorText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_SanctuaryGel_Semantic_Intake_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v111MembraneLawPath = Path.Combine(lineRoot, "docs", "SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md");
        var v111WorkbenchLawPath = Path.Combine(lineRoot, "docs", "RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var rootAtlasBoundaryPath = Path.Combine(v121Root, "docs", "ROOTATLAS_REMOTE_SOURCE_BOUNDARY.md");
        var gelBootstrapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_BOOTSTRAP_LAW.md");
        var engramCleavingPath = Path.Combine(v121Root, "docs", "ENGRAM_CLEAVING_LADDER_LAW.md");
        var verbatimPath = Path.Combine(v121Root, "docs", "VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md");
        var atlasMappingPath = Path.Combine(v121Root, "docs", "ROOTATLAS_MAPPING_AND_VARIANT_LAW.md");
        var predicatePath = Path.Combine(v121Root, "docs", "PREDICATE_CANDIDATE_FORMATION_LAW.md");
        var iuttPath = Path.Combine(v121Root, "docs", "IUTT_TRANSFORMATION_BOUNDARY_LAW.md");
        var membranePath = Path.Combine(v121Root, "docs", "INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md");
        var intakeAuditPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_SEMANTIC_INTAKE_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v111MembraneLawText = File.ReadAllText(v111MembraneLawPath);
        var v111WorkbenchLawText = File.ReadAllText(v111WorkbenchLawPath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var rootAtlasBoundaryText = File.ReadAllText(rootAtlasBoundaryPath);
        var gelBootstrapText = File.ReadAllText(gelBootstrapPath);
        var engramCleavingText = File.ReadAllText(engramCleavingPath);
        var verbatimText = File.ReadAllText(verbatimPath);
        var atlasMappingText = File.ReadAllText(atlasMappingPath);
        var predicateText = File.ReadAllText(predicatePath);
        var iuttText = File.ReadAllText(iuttPath);
        var membraneText = File.ReadAllText(membranePath);
        var intakeAuditText = File.ReadAllText(intakeAuditPath);

        Assert.Contains("`first-working-model-sanctuary-gel-intake: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-gel-interior-awareness-and-universe: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first `Sanctuary.GEL` semantic intake", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("keeping predicate promotion,", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("engram minting, and runtime activation withheld.", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` interior-awareness and universe-law", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.GEL` semantic intake cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ROOTATLAS_MAPPING_AND_VARIANT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PREDICATE_CANDIDATE_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("IUTT_TRANSFORMATION_BOUNDARY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_SEMANTIC_INTAKE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize predicate promotion.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize engram minting.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize runtime activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("Intake may preserve, classify, map, transform, and encode for cryptic use,", gateText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` interior-awareness and universe-law cluster", gateText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL Semantic Intake Clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("pre-Lisp and pre-code intake now has a lawful ladder", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("absence, malformation, and direct attack are now explicit intake", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL semantic intake clarification -> Sanctuary.GEL interior awareness and universe-law clarification -> Sanctuary.GEL rest-state and persistence clarification -> SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("ROOTATLAS_MAPPING_AND_VARIANT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PREDICATE_CANDIDATE_FORMATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("IUTT_TRANSFORMATION_BOUNDARY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_SEMANTIC_INTAKE_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("Engram cleaving ladder law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Verbatim ingress and symbol preservation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("RootAtlas mapping and variant law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Predicate candidate formation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("IUTT transformation boundary law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Ingress membrane and engram encoding law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.GEL semantic intake audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-sanctuary-gel-intake-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("does not imply predicate promotion, engram minting, service", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`ENGRAM_CLEAVING_LADDER_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`ROOTATLAS_MAPPING_AND_VARIANT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PREDICATE_CANDIDATE_FORMATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`IUTT_TRANSFORMATION_BOUNDARY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SANCTUARY_GEL_SEMANTIC_INTAKE_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("pre-Lisp or pre-code semantic ascent", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("predicate candidate formation, `IUTT` transformation", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("`RootAtlas` lives only on Research Servers.", rootAtlasBoundaryText, StringComparison.Ordinal);
        Assert.Contains("The first lawful local substrate is `Sanctuary.GEL`.", gelBootstrapText, StringComparison.Ordinal);
        Assert.Contains("`IUTT` defines the lawful interval space", v111MembraneLawText, StringComparison.Ordinal);
        Assert.Contains("admitted", v111WorkbenchLawText, StringComparison.Ordinal);
        Assert.Contains("provisional", v111WorkbenchLawText, StringComparison.Ordinal);
        Assert.Contains("refused", v111WorkbenchLawText, StringComparison.Ordinal);
        Assert.Contains("hold", v111WorkbenchLawText, StringComparison.Ordinal);

        Assert.Contains("intake may preserve, classify, map, transform, and encode", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("`root -> predicate -> proposition -> procedure -> master`", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("`retained-as-witness`", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("`retained-as-provisional-meaning`", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("`retained-as-engram-ready`", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("not every proposition may become procedure", engramCleavingText, StringComparison.Ordinal);
        Assert.Contains("not every procedure may become active runtime law", engramCleavingText, StringComparison.Ordinal);

        Assert.Contains("no symbol should be reduced to plain text until the system", verbatimText, StringComparison.Ordinal);
        Assert.Contains("verbatim ingress is evidence before it is interpretation.", verbatimText, StringComparison.Ordinal);
        Assert.Contains("silent correction is forbidden", verbatimText, StringComparison.Ordinal);

        Assert.Contains("`RootAtlas` is root ancestry, not local formed knowledge.", atlasMappingText, StringComparison.Ordinal);
        Assert.Contains("absence is evidence only when the absence is structurally", atlasMappingText, StringComparison.Ordinal);
        Assert.Contains("`prefix-suffix-derived`", atlasMappingText, StringComparison.Ordinal);

        Assert.Contains("predicate candidate formation is the first structured meaning", predicateText, StringComparison.Ordinal);
        Assert.Contains("candidate-bearing, not closure-bearing", predicateText, StringComparison.Ordinal);
        Assert.Contains("mixed-universe candidate structure without lawful mapping", predicateText, StringComparison.Ordinal);

        Assert.Contains("`IUTT` is the lawful interval space in which admissible", iuttText, StringComparison.Ordinal);
        Assert.Contains("transformation is not execution, closure, or authorization.", iuttText, StringComparison.Ordinal);
        Assert.Contains("interval transform without witness", iuttText, StringComparison.Ordinal);

        Assert.Contains("hostile input may be preserved and classified, but it may not", membraneText, StringComparison.Ordinal);
        Assert.Contains("`retained-as-engram-ready`", membraneText, StringComparison.Ordinal);
        Assert.Contains("attack-shaped ingress granted standing by parseability", membraneText, StringComparison.Ordinal);
        Assert.Contains("predicate promotion", membraneText, StringComparison.Ordinal);
        Assert.Contains("engram minting", membraneText, StringComparison.Ordinal);

        Assert.Contains("preserve truth under omission, corruption, and", intakeAuditText, StringComparison.Ordinal);
        Assert.Contains("## Big-Three Burdens", intakeAuditText, StringComparison.Ordinal);
        Assert.Contains("malformation and misspelling without silent correction", intakeAuditText, StringComparison.Ordinal);
        Assert.Contains("direct attack without granting standing by parseability alone", intakeAuditText, StringComparison.Ordinal);
        Assert.Contains("any implication of predicate promotion, engram minting, or", intakeAuditText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Gel_Interior_Awareness_And_Universe_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var cleavingPath = Path.Combine(v121Root, "docs", "ENGRAM_CLEAVING_LADDER_LAW.md");
        var constructorPath = Path.Combine(v121Root, "docs", "ENGRAM_CONSTRUCTOR_CLASS_LAW.md");
        var propositionPath = Path.Combine(v121Root, "docs", "PROPOSITIONAL_ENGRAM_FORMATION_LAW.md");
        var procedurePath = Path.Combine(v121Root, "docs", "PROCEDURAL_ENGRAM_FORMATION_LAW.md");
        var universePath = Path.Combine(v121Root, "docs", "LANGUAGE_UNIVERSE_GROUPOID_LAW.md");
        var correspondencePath = Path.Combine(v121Root, "docs", "ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW.md");
        var posturePath = Path.Combine(v121Root, "docs", "ENGRAM_POSTURE_AND_REFUSAL_LAW.md");
        var awarenessPath = Path.Combine(v121Root, "docs", "GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "GEL_INTERIOR_AWARENESS_AND_UNIVERSE_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var cleavingText = File.ReadAllText(cleavingPath);
        var constructorText = File.ReadAllText(constructorPath);
        var propositionText = File.ReadAllText(propositionPath);
        var procedureText = File.ReadAllText(procedurePath);
        var universeText = File.ReadAllText(universePath);
        var correspondenceText = File.ReadAllText(correspondencePath);
        var postureText = File.ReadAllText(posturePath);
        var awarenessText = File.ReadAllText(awarenessPath);
        var auditText = File.ReadAllText(auditPath);

        Assert.Contains("`first-working-model-gel-interior-awareness-and-universe: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-gel-rest-state: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` interior-awareness and universe-law", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("constructor class", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("categorical engram class", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("keeping propositions, procedures, contradiction handling,", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` rest-state cluster", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.GEL` interior-awareness and universe-law cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CONSTRUCTOR_CLASS_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PROPOSITIONAL_ENGRAM_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PROCEDURAL_ENGRAM_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("LANGUAGE_UNIVERSE_GROUPOID_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_POSTURE_AND_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_INTERIOR_AWARENESS_AND_UNIVERSE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize predicate promotion.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize engram minting.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize runtime activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("Awareness remains derived posture, not autonomous life or action.", gateText, StringComparison.Ordinal);
        Assert.Contains("the next `Sanctuary.GEL` rest-state and persistence cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_ADMITTED_INTERIOR_OBJECT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_HELD_ENGRAM_STATE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_CONTINUITY_AND_RESTING_LINKAGE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_CONTRADICTION_AND_POSTURE_BINDING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim that `GEL` has lawful resting", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize `Sanctuary.MoS` storage-seat standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("Persisted is not activated, and held is not promoted.", gateText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL Interior Awareness And Universe-Law Clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` now distinguishes constructor class from categorical engram class", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("the first admitted universe ring is now fixed as four primary universes", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("propositions, procedures, contradiction handling, posture, and awareness", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("the first `Sanctuary.GEL` rest-state and persistence cluster now lives in", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL interior awareness and universe-law clarification -> Sanctuary.GEL rest-state and persistence clarification -> SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CONSTRUCTOR_CLASS_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PROPOSITIONAL_ENGRAM_FORMATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PROCEDURAL_ENGRAM_FORMATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("LANGUAGE_UNIVERSE_GROUPOID_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_POSTURE_AND_REFUSAL_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_INTERIOR_AWARENESS_AND_UNIVERSE_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("Engram constructor class law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Propositional engram formation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Procedural engram formation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Language universe groupoid law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Engram correspondence and contradiction law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Engram posture and refusal law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GEL situational awareness frame law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GEL interior awareness and universe audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("admitted-gel-interior-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-promotional, non-minting, and non-runtime", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`ENGRAM_CONSTRUCTOR_CLASS_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`LANGUAGE_UNIVERSE_GROUPOID_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PROPOSITIONAL_ENGRAM_FORMATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PROCEDURAL_ENGRAM_FORMATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`ENGRAM_POSTURE_AND_REFUSAL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_INTERIOR_AWARENESS_AND_UNIVERSE_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("constructor-class law and language-universe groupoid law", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("propositional and procedural engram formation,", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("Semantic formation layers are not constructor classes.", cleavingText, StringComparison.Ordinal);
        Assert.Contains("The next interior-awareness cluster derives above this intake ladder", cleavingText, StringComparison.Ordinal);

        Assert.Contains("`Root -> Basic -> Intermediate -> Advanced -> Master`", constructorText, StringComparison.Ordinal);
        Assert.Contains("constructor class answers how far the engram has been built; categorical", constructorText, StringComparison.Ordinal);
        Assert.Contains("semantic formation layer", constructorText, StringComparison.Ordinal);
        Assert.Contains("`engram_id`", constructorText, StringComparison.Ordinal);
        Assert.Contains("`constructor_class`", constructorText, StringComparison.Ordinal);
        Assert.Contains("`categorical_class`", constructorText, StringComparison.Ordinal);

        Assert.Contains("a propositional engram is a witnessed semantic holding", propositionText, StringComparison.Ordinal);
        Assert.Contains("`categorical_class = propositional`", propositionText, StringComparison.Ordinal);
        Assert.Contains("`truth_state` is limited to:", propositionText, StringComparison.Ordinal);
        Assert.Contains("`admitted`", propositionText, StringComparison.Ordinal);
        Assert.Contains("`provisional`", propositionText, StringComparison.Ordinal);

        Assert.Contains("procedural engrams define handling method, not execution authority.", procedureText, StringComparison.Ordinal);
        Assert.Contains("`categorical_class = procedural`", procedureText, StringComparison.Ordinal);
        Assert.Contains("`allowed_effect_class = NON-RUNTIME`", procedureText, StringComparison.Ordinal);
        Assert.Contains("`discernment`", procedureText, StringComparison.Ordinal);
        Assert.Contains("`correspondence`", procedureText, StringComparison.Ordinal);

        Assert.Contains("shared root ancestry does not imply shared semantic identity.", universeText, StringComparison.Ordinal);
        Assert.Contains("`CommonLanguage`", universeText, StringComparison.Ordinal);
        Assert.Contains("`Mathematics`", universeText, StringComparison.Ordinal);
        Assert.Contains("`ScienceGeneral`", universeText, StringComparison.Ordinal);
        Assert.Contains("`Sociology`", universeText, StringComparison.Ordinal);
        Assert.Contains("`Ontology`", universeText, StringComparison.Ordinal);
        Assert.Contains("`Epistemology`", universeText, StringComparison.Ordinal);
        Assert.Contains("`equivalent`", universeText, StringComparison.Ordinal);
        Assert.Contains("`suspended`", universeText, StringComparison.Ordinal);
        Assert.Contains("`TAG.sty` remains external ancestry only", universeText, StringComparison.Ordinal);

        Assert.Contains("`plural-not-contradictory`", correspondenceText, StringComparison.Ordinal);
        Assert.Contains("`correspondence-unresolved`", correspondenceText, StringComparison.Ordinal);
        Assert.Contains("`locally-contradictory`", correspondenceText, StringComparison.Ordinal);
        Assert.Contains("`hostile-pressure`", correspondenceText, StringComparison.Ordinal);
        Assert.Contains("`malformed-source`", correspondenceText, StringComparison.Ordinal);
        Assert.Contains("single generic error bucket", correspondenceText, StringComparison.Ordinal);

        Assert.Contains("The only admitted posture classes in this phase are:", postureText, StringComparison.Ordinal);
        Assert.Contains("`admit`", postureText, StringComparison.Ordinal);
        Assert.Contains("`provisional`", postureText, StringComparison.Ordinal);
        Assert.Contains("`hold`", postureText, StringComparison.Ordinal);
        Assert.Contains("`refuse`", postureText, StringComparison.Ordinal);
        Assert.Contains("posture is descriptive-governing, not executable.", postureText, StringComparison.Ordinal);

        Assert.Contains("situational awareness is a derived frame, not autonomous action.", awarenessText, StringComparison.Ordinal);
        Assert.Contains("`frame_id`", awarenessText, StringComparison.Ordinal);
        Assert.Contains("`active_universe_candidates[]`", awarenessText, StringComparison.Ordinal);
        Assert.Contains("`recommended_posture`", awarenessText, StringComparison.Ordinal);
        Assert.Contains("It is not autonomous life, execution, promotion, minting, or runtime.", awarenessText, StringComparison.Ordinal);

        Assert.Contains("any constructor class outside `Root/Basic/Intermediate/Advanced/Master`", auditText, StringComparison.Ordinal);
        Assert.Contains("any treatment of semantic formation layer as constructor class", auditText, StringComparison.Ordinal);
        Assert.Contains("any overlay or meta/control family treated as a primary universe", auditText, StringComparison.Ordinal);
        Assert.Contains("any awareness frame treated as autonomous action", auditText, StringComparison.Ordinal);
        Assert.Contains("any admitted `TAG.sty` bridge in this phase", auditText, StringComparison.Ordinal);
        Assert.Contains("predicate promotion, engram minting, service", auditText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Gel_Rest_State_And_Persistence_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var awarenessPath = Path.Combine(v121Root, "docs", "GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md");
        var admittedPath = Path.Combine(v121Root, "docs", "GEL_ADMITTED_INTERIOR_OBJECT_LAW.md");
        var heldPath = Path.Combine(v121Root, "docs", "GEL_HELD_ENGRAM_STATE_LAW.md");
        var continuityPath = Path.Combine(v121Root, "docs", "GEL_CONTINUITY_AND_RESTING_LINKAGE_LAW.md");
        var bindingPath = Path.Combine(v121Root, "docs", "GEL_REST_STATE_CONTRADICTION_AND_POSTURE_BINDING_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "GEL_REST_STATE_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var awarenessText = File.ReadAllText(awarenessPath);
        var admittedText = File.ReadAllText(admittedPath);
        var heldText = File.ReadAllText(heldPath);
        var continuityText = File.ReadAllText(continuityPath);
        var bindingText = File.ReadAllText(bindingPath);
        var auditText = File.ReadAllText(auditPath);

        Assert.Contains("`first-working-model-gel-rest-state: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-sli-symbolic-transport-form: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` rest-state cluster", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("persistence inside `GEL`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("explicitly refusing `Sanctuary.MoS` over-read", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SLI` symbolic transport-form cluster", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("the next `Sanctuary.GEL` rest-state and persistence cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_ADMITTED_INTERIOR_OBJECT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_HELD_ENGRAM_STATE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_CONTINUITY_AND_RESTING_LINKAGE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_CONTRADICTION_AND_POSTURE_BINDING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim that `GEL` has lawful resting", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize `Sanctuary.MoS` storage-seat standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("Persisted is not activated, and held is not promoted.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn rest-state clarification into `Sanctuary.MoS` or", gateText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL rest-state and persistence clarification -> SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_ADMITTED_INTERIOR_OBJECT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_HELD_ENGRAM_STATE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_CONTINUITY_AND_RESTING_LINKAGE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_CONTRADICTION_AND_POSTURE_BINDING_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_REST_STATE_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL Rest-State And Persistence Clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` now has one canonical held interior object at rest", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("rest-state persistence remains inside `GEL`", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` remains downstream and is not admitted by this slice", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("admitted-gel-rest-state-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Held Interior Engram Object", ledgerText, StringComparison.Ordinal);
        Assert.Contains("what it means for an interior object to be lawfully held", ledgerText, StringComparison.Ordinal);
        Assert.Contains("lawful resting persistence inside `GEL`", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-promotional, non-activating, and non-`MoS`", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`GEL_ADMITTED_INTERIOR_OBJECT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_HELD_ENGRAM_STATE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_CONTINUITY_AND_RESTING_LINKAGE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_REST_STATE_CONTRADICTION_AND_POSTURE_BINDING_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_REST_STATE_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("canonical held interior object law for `GEL` rest-state persistence", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("held-state persistence, continuity-at-rest linkage,", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("Lawful awareness must terminate in a lawful resting object.", awarenessText, StringComparison.Ordinal);
        Assert.Contains("Awareness itself does not persist as action.", awarenessText, StringComparison.Ordinal);

        Assert.Contains("The only canonical resting object admitted in this phase is:", admittedText, StringComparison.Ordinal);
        Assert.Contains("`Held Interior Engram Object`", admittedText, StringComparison.Ordinal);
        Assert.Contains("`held_id`", admittedText, StringComparison.Ordinal);
        Assert.Contains("`rest_state`", admittedText, StringComparison.Ordinal);
        Assert.Contains("`witness_chain[]`", admittedText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` storage-seat standing", admittedText, StringComparison.Ordinal);

        Assert.Contains("held is not promoted, and persisted is not activated.", heldText, StringComparison.Ordinal);
        Assert.Contains("`held`, `admitted`, `persisted`, and `promoted` are distinct notions.", heldText, StringComparison.Ordinal);
        Assert.Contains("The exact rest-state vocabulary admitted in this phase is:", heldText, StringComparison.Ordinal);
        Assert.Contains("`held`", heldText, StringComparison.Ordinal);
        Assert.Contains("`admitted`", heldText, StringComparison.Ordinal);
        Assert.Contains("`provisional`", heldText, StringComparison.Ordinal);
        Assert.Contains("`refused`", heldText, StringComparison.Ordinal);
        Assert.Contains("posture-bound contradiction", heldText, StringComparison.Ordinal);

        Assert.Contains("continuity at rest is lawful persistence shape, not motion.", continuityText, StringComparison.Ordinal);
        Assert.Contains("`ancestral`", continuityText, StringComparison.Ordinal);
        Assert.Contains("`derivational`", continuityText, StringComparison.Ordinal);
        Assert.Contains("`posture-carrying`", continuityText, StringComparison.Ordinal);
        Assert.Contains("`contradiction-carrying`", continuityText, StringComparison.Ordinal);
        Assert.Contains("`rest-update`", continuityText, StringComparison.Ordinal);
        Assert.Contains("`later-master-eligible`", continuityText, StringComparison.Ordinal);
        Assert.Contains("linkage used to imply promotion or storage-seat standing", continuityText, StringComparison.Ordinal);

        Assert.Contains("contradiction at rest is a bound condition, not a transient parser event.", bindingText, StringComparison.Ordinal);
        Assert.Contains("posture at rest is descriptive-governing, not executable method.", bindingText, StringComparison.Ordinal);
        Assert.Contains("Multiple unresolved conditions may coexist lawfully at rest without collapse.", bindingText, StringComparison.Ordinal);
        Assert.Contains("`binding_id`", bindingText, StringComparison.Ordinal);
        Assert.Contains("`held_object_ref`", bindingText, StringComparison.Ordinal);
        Assert.Contains("`coexistence_state`", bindingText, StringComparison.Ordinal);
        Assert.Contains("`review_requirements[]`", bindingText, StringComparison.Ordinal);

        Assert.Contains("creation of the canonical `Held Interior Engram Object`", auditText, StringComparison.Ordinal);
        Assert.Contains("lawful held-state vocabulary", auditText, StringComparison.Ordinal);
        Assert.Contains("continuity linkage at rest", auditText, StringComparison.Ordinal);
        Assert.Contains("contradiction and posture binding", auditText, StringComparison.Ordinal);
        Assert.Contains("refusal to over-ascent into promotion, activation, or `Sanctuary.MoS`", auditText, StringComparison.Ordinal);
        Assert.Contains("any second competing canonical rest object", auditText, StringComparison.Ordinal);
        Assert.Contains("any missing witness chain on an admitted resting object", auditText, StringComparison.Ordinal);
        Assert.Contains("any use of resting persistence to imply `Sanctuary.MoS` storage-seat", auditText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Sli_Symbolic_Transport_Form_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var verbatimPath = Path.Combine(v121Root, "docs", "VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md");
        var atlasMappingPath = Path.Combine(v121Root, "docs", "ROOTATLAS_MAPPING_AND_VARIANT_LAW.md");
        var constructorPath = Path.Combine(v121Root, "docs", "ENGRAM_CONSTRUCTOR_CLASS_LAW.md");
        var utf8LawPath = Path.Combine(v121Root, "docs", "UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md");
        var rootBaseFormPath = Path.Combine(v121Root, "docs", "ROOT_SYMBOL_BASE_FORM_LAW.md");
        var formationLawPath = Path.Combine(v121Root, "docs", "SUPER_SUBSCRIPT_EXTENSION_FORMATION_LAW.md");
        var lineageLawPath = Path.Combine(v121Root, "docs", "SYMBOLIC_TRANSPORT_LINEAGE_AND_CONTEXT_SPLIT_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "SLI_SYMBOLIC_TRANSPORT_FORM_AUDIT.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var verbatimText = File.ReadAllText(verbatimPath);
        var atlasMappingText = File.ReadAllText(atlasMappingPath);
        var constructorText = File.ReadAllText(constructorPath);
        var utf8LawText = File.ReadAllText(utf8LawPath);
        var rootBaseFormText = File.ReadAllText(rootBaseFormPath);
        var formationLawText = File.ReadAllText(formationLawPath);
        var lineageLawText = File.ReadAllText(lineageLawPath);
        var auditText = File.ReadAllText(auditPath);

        Assert.Contains("`first-working-model-sli-symbolic-transport-form: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SLI` symbolic transport-form cluster", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("UTF-8 carrier integrity, shared root transport lineage,", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("governed super/sub expansion only, and explicit non-mutation posture", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("That `Sanctuary.GEL` rest-state and persistence cluster now supports the next", gateText, StringComparison.Ordinal);
        Assert.Contains("`SLI` symbolic transport-form cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ROOT_SYMBOL_BASE_FORM_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SUPER_SUBSCRIPT_EXTENSION_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SYMBOLIC_TRANSPORT_LINEAGE_AND_CONTEXT_SPLIT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SLI_SYMBOLIC_TRANSPORT_FORM_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("required before any claim that `SLI` is transport-ready for", gateText, StringComparison.Ordinal);
        Assert.Contains("Atlas symbolic delta candidacy is truthful", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Atlas delta candidacy.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Atlas mutation.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize predicate promotion.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize engram minting.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize operator realization.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize `Sanctuary.MoS` standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize runtime activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("Transport readiness preserves ancestry and formation law, but it does not", gateText, StringComparison.Ordinal);
        Assert.Contains("guarantee local semantic realization.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn symbolic transport-form clarification into Atlas delta", gateText, StringComparison.Ordinal);
        Assert.Contains("Atlas mutation, operator realization, or trusted runtime action.", gateText, StringComparison.Ordinal);

        Assert.Contains("SLI Symbolic Transport-Form Clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Atlas-derived symbolic material now preserves exact UTF-8 carrier identity", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Root` remains adjunct-free", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Basic` and `Intermediate` now extend only through governed super/subscript", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("transport lineage now remains shared until context, formation, and", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Atlas delta candidacy, Atlas mutation, predicate promotion, engram minting,", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("operator realization, and runtime remain withheld", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.GEL rest-state and persistence clarification -> SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("ROOT_SYMBOL_BASE_FORM_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SUPER_SUBSCRIPT_EXTENSION_FORMATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SYMBOLIC_TRANSPORT_LINEAGE_AND_CONTEXT_SPLIT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("SLI_SYMBOLIC_TRANSPORT_FORM_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-sli-symbolic-transport-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("UTF-8 reserved set and canonical symbol law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Root symbol base form law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Super/subscript extension formation law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Symbolic transport lineage and context split law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("SLI symbolic transport form audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Atlas-derived material while remaining", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-delta, non-mutating, non-promotional, and non-runtime", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`ROOT_SYMBOL_BASE_FORM_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SUPER_SUBSCRIPT_EXTENSION_FORMATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SYMBOLIC_TRANSPORT_LINEAGE_AND_CONTEXT_SPLIT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`SLI_SYMBOLIC_TRANSPORT_FORM_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("UTF-8 canonical carrier law or adjunct-free root transport", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("governed super/subscript formation, witnessed context", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("split, or symbolic transport audit", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md", verbatimText, StringComparison.Ordinal);
        Assert.Contains("does not itself freeze a full explicit reserved-set inventory", verbatimText, StringComparison.Ordinal);

        Assert.Contains("ROOT_SYMBOL_BASE_FORM_LAW.md", atlasMappingText, StringComparison.Ordinal);
        Assert.Contains("surface variation may remain mapped and witnessed here without", atlasMappingText, StringComparison.Ordinal);
        Assert.Contains("distinct transport identity", atlasMappingText, StringComparison.Ordinal);

        Assert.Contains("The symbolic transport-form cluster now governs `Root`, `Basic`, and", constructorText, StringComparison.Ordinal);
        Assert.Contains("`Intermediate` transport construction", constructorText, StringComparison.Ordinal);
        Assert.Contains("does not widen `Advanced` or `Master`", constructorText, StringComparison.Ordinal);

        Assert.Contains("UTF-8 preservation precedes symbolic interpretation.", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("does not yet freeze or enumerate a full explicit reserved-set inventory", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`raw_utf8`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`normalized_utf8`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`unicode_codepoints[]`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`display_form`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`symbol_class`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`carrier_identity_ref`", utf8LawText, StringComparison.Ordinal);
        Assert.Contains("`witness_refs[]`", utf8LawText, StringComparison.Ordinal);

        Assert.Contains("one shared canonical transport carrier across the admitted", rootBaseFormText, StringComparison.Ordinal);
        Assert.Contains("Only `Root` constructor class is admitted at transport base form.", rootBaseFormText, StringComparison.Ordinal);
        Assert.Contains("adjunct-free", rootBaseFormText, StringComparison.Ordinal);
        Assert.Contains("Surface variation does not automatically imply separate engram identity.", rootBaseFormText, StringComparison.Ordinal);
        Assert.Contains("Shared root ancestry does not automatically imply identical local semantic", rootBaseFormText, StringComparison.Ordinal);
        Assert.Contains("semantic realization", rootBaseFormText, StringComparison.Ordinal);

        Assert.Contains("Complexity enters through governed formation, not ad hoc decoration.", formationLawText, StringComparison.Ordinal);
        Assert.Contains("`Basic` permits exactly one governed extension", formationLawText, StringComparison.Ordinal);
        Assert.Contains("one super", formationLawText, StringComparison.Ordinal);
        Assert.Contains("or one sub", formationLawText, StringComparison.Ordinal);
        Assert.Contains("`Intermediate` permits exactly one governed composed extension", formationLawText, StringComparison.Ordinal);
        Assert.Contains("one super plus one sub", formationLawText, StringComparison.Ordinal);
        Assert.Contains("operator layer", formationLawText, StringComparison.Ordinal);
        Assert.Contains("affix creep", formationLawText, StringComparison.Ordinal);

        Assert.Contains("Transport readiness preserves root ancestry and formation law; it does not", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("guarantee local semantic realization.", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("Separation must be earned by context, formation, and witnessed use.", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("Surface variation alone is insufficient.", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("Shared ancestry alone is insufficient for local identity.", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("`same-lineage`", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("`context-pending`", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("`split-eligible`", lineageLawText, StringComparison.Ordinal);
        Assert.Contains("`split-refused`", lineageLawText, StringComparison.Ordinal);

        Assert.Contains("UTF-8 loss before record", auditText, StringComparison.Ordinal);
        Assert.Contains("second competing canonical root carrier", auditText, StringComparison.Ordinal);
        Assert.Contains("adjunct inflation at `Root`", auditText, StringComparison.Ordinal);
        Assert.Contains("automatic split", auditText, StringComparison.Ordinal);
        Assert.Contains("local semantic realization treated as guaranteed by transport law", auditText, StringComparison.Ordinal);
        Assert.Contains("Atlas delta candidacy", auditText, StringComparison.Ordinal);
        Assert.Contains("Atlas mutation", auditText, StringComparison.Ordinal);
        Assert.Contains("predicate promotion", auditText, StringComparison.Ordinal);
        Assert.Contains("engram minting", auditText, StringComparison.Ordinal);
        Assert.Contains("operator realization", auditText, StringComparison.Ordinal);
        Assert.Contains("runtime activation", auditText, StringComparison.Ordinal);
        Assert.Contains("full explicit reserved-set inventory", auditText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Install_Agreement_Action_Surface_And_Identity_Footing_Cluster_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var v111InstallBridgePath = Path.Combine(lineRoot, "docs", "SANCTUARY_BOOT_FIRST_RUN_ONTOLOGY_BRIDGE.md");
        var v111ContractSurfacePath = Path.Combine(lineRoot, "docs", "SANCTUARY_DEPLOYMENT_AND_CONTRACT_SURFACE.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var actionLawPath = Path.Combine(v121Root, "docs", "INSTALL_AGREEMENT_ACTION_SURFACE_LAW.md");
        var bundleLawPath = Path.Combine(v121Root, "docs", "AGREEMENT_PREDICATE_BUNDLE_LAW.md");
        var identityLawPath = Path.Combine(v121Root, "docs", "INSTALL_IDENTITY_SET_LAW.md");
        var postureLawPath = Path.Combine(v121Root, "docs", "CME_USE_DATA_POSTURE_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "INSTALL_ASSENT_AND_IDENTITY_AUDIT.md");
        var templateLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_TEMPLATE_ADOPTION_LAW.md");
        var hitlLawPath = Path.Combine(v121Root, "docs", "SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW.md");
        var contractsPath = Path.Combine(v121Root, "src", "San", "San.Common", "InstallAgreementContracts.cs");
        var actionSurfacePath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "InstallAgreementActionSurface.cs");
        var lispModulePath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "install-agreement.lisp");
        var csprojPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "SLI.Lisp.csproj");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var v111InstallBridgeText = File.ReadAllText(v111InstallBridgePath);
        var v111ContractSurfaceText = File.ReadAllText(v111ContractSurfacePath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var actionLawText = File.ReadAllText(actionLawPath);
        var bundleLawText = File.ReadAllText(bundleLawPath);
        var identityLawText = File.ReadAllText(identityLawPath);
        var postureLawText = File.ReadAllText(postureLawPath);
        var auditText = File.ReadAllText(auditPath);
        var templateLawText = File.ReadAllText(templateLawPath);
        var hitlLawText = File.ReadAllText(hitlLawPath);
        var contractsText = File.ReadAllText(contractsPath);
        var actionSurfaceText = File.ReadAllText(actionSurfacePath);
        var lispModuleText = File.ReadAllText(lispModulePath);
        var csprojText = File.ReadAllText(csprojPath);

        Assert.Contains("`first-working-model-install-agreement-action-surface: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("install-agreement and identity-footing", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("full assent before install identity can exist", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("keeping contract view derived", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("rather than canonical identity ownership", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("keeping service and runtime", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("standing withheld", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("install-agreement and identity-footing clarification", v111InstallBridgeText, StringComparison.Ordinal);
        Assert.Contains("remains install-state work", v111InstallBridgeText, StringComparison.Ordinal);
        Assert.Contains("narrower line-local first-run constitutional projection", v111InstallBridgeText, StringComparison.Ordinal);

        Assert.Contains("derived implementation view", v111ContractSurfaceText, StringComparison.Ordinal);
        Assert.Contains("not the canonical install identity owner", v111ContractSurfaceText, StringComparison.Ordinal);
        Assert.Contains("replace localized", v111ContractSurfaceText, StringComparison.Ordinal);
        Assert.Contains("agreement assent topology", v111ContractSurfaceText, StringComparison.Ordinal);

        Assert.Contains("INSTALL_AGREEMENT_ACTION_SURFACE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("AGREEMENT_PREDICATE_BUNDLE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("INSTALL_IDENTITY_SET_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CME_USE_DATA_POSTURE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("INSTALL_ASSENT_AND_IDENTITY_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("localized install assent has", gateText, StringComparison.Ordinal);
        Assert.Contains("lawful identity-bearing footing", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize service authorization.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize runtime activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Atlas mutation.", gateText, StringComparison.Ordinal);
        Assert.Contains("Agreement formation may seat localized install footing in the cryptic hosted", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn install-assent and identity-footing clarification into live", gateText, StringComparison.Ordinal);

        Assert.Contains("install-agreement and identity-footing cluster now lives in", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("install identity candidacy after full assent", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("contract view derived", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive install-assent contract family", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("installed-disabled cryptic bundle extension surface", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("install agreement action surface and identity footing clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryContractProfile` remains a derived deployment view rather than the", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("canonical install identity owner", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`research-attached-default`", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("INSTALL_AGREEMENT_ACTION_SURFACE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("AGREEMENT_PREDICATE_BUNDLE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("INSTALL_IDENTITY_SET_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CME_USE_DATA_POSTURE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("INSTALL_ASSENT_AND_IDENTITY_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-install-agreement-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Install agreement action surface law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Agreement predicate bundle law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Install identity set law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CME use data posture law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Install assent and identity audit", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-service-bearing", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`INSTALL_AGREEMENT_ACTION_SURFACE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`AGREEMENT_PREDICATE_BUNDLE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`INSTALL_IDENTITY_SET_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CME_USE_DATA_POSTURE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`INSTALL_ASSENT_AND_IDENTITY_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("install-assent and identity footing", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("install-agreement action surface law for localized assent and identity", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("service authorization or runtime standing", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("LocalizedInstallChoiceMatrix -> AgreementPredicateBundle -> InstallIdentitySetCandidate -> CoreCmeUsePostureRecord", actionLawText, StringComparison.Ordinal);
        Assert.Contains("installed-disabled", actionLawText, StringComparison.Ordinal);
        Assert.Contains("not part of the canonical floor set", actionLawText, StringComparison.Ordinal);
        Assert.Contains("localized agreement predicates must be formed from the authorized choice", actionLawText, StringComparison.Ordinal);
        Assert.Contains("matrix and lawful agreement surface", actionLawText, StringComparison.Ordinal);

        Assert.Contains("`service-license-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`terms-of-service-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`bonded-operator-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`cme-lab-notice-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`research-data-practice-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`access-attachment-profile-predicate`", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("acknowledgement is not assent", bundleLawText, StringComparison.Ordinal);
        Assert.Contains("`Acknowledged` and `Assented` must remain distinguishable", bundleLawText, StringComparison.Ordinal);

        Assert.Contains("no install identity may exist", identityLawText, StringComparison.Ordinal);
        Assert.Contains("without assented predicate formation", identityLawText, StringComparison.Ordinal);
        Assert.Contains("derived and is not the canonical", identityLawText, StringComparison.Ordinal);
        Assert.Contains("install identity owner", identityLawText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryContractProfile`", identityLawText, StringComparison.Ordinal);

        Assert.Contains("`research-attached-default`", postureLawText, StringComparison.Ordinal);
        Assert.Contains("agency-plus-research-context", postureLawText, StringComparison.Ordinal);
        Assert.Contains("first persisted rest-state operational footing", postureLawText, StringComparison.Ordinal);
        Assert.Contains("It is not:", postureLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` standing", postureLawText, StringComparison.Ordinal);

        Assert.Contains("absence of `InstallIdentitySetCandidate` before full assent", auditText, StringComparison.Ordinal);
        Assert.Contains("contract-view derivation without canonical install-identity ownership", auditText, StringComparison.Ordinal);
        Assert.Contains("any install identity exists before full assent", auditText, StringComparison.Ordinal);
        Assert.Contains("any acknowledgement is treated as assent", auditText, StringComparison.Ordinal);
        Assert.Contains("any contract view is treated as the canonical install identity owner", auditText, StringComparison.Ordinal);

        Assert.Contains("Downloaded or templated agreements remain templates until explicit adoption and", templateLawText, StringComparison.Ordinal);
        Assert.Contains("explicit assent", templateLawText, StringComparison.Ordinal);
        Assert.Contains("No `AgreementPredicateBundle` may be formed from template presence alone.", templateLawText, StringComparison.Ordinal);

        Assert.Contains("install-assent corridor may preserve this same acknowledgement versus", hitlLawText, StringComparison.Ordinal);
        Assert.Contains("assent distinction", hitlLawText, StringComparison.Ordinal);
        Assert.Contains("not a `HitlHold` exit-token family", hitlLawText, StringComparison.Ordinal);

        Assert.Contains("public enum AgreementPredicateKind", contractsText, StringComparison.Ordinal);
        Assert.Contains("ServiceLicensePredicate = 0", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum AgreementAssentState", contractsText, StringComparison.Ordinal);
        Assert.Contains("Acknowledged = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record LocalizedInstallChoiceMatrix(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record AgreementPredicateBundle(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record InstallIdentitySetCandidate(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CoreCmeUsePostureRecord(", contractsText, StringComparison.Ordinal);

        Assert.Contains("public interface IInstallAgreementActionSurface", actionSurfaceText, StringComparison.Ordinal);
        Assert.Contains("public sealed class GovernedInstallAgreementActionSurface", actionSurfaceText, StringComparison.Ordinal);
        Assert.Contains("InstallIdentitySetCandidate? IdentityCandidate", actionSurfaceText, StringComparison.Ordinal);
        Assert.Contains("bundle.FullAssent", actionSurfaceText, StringComparison.Ordinal);
        Assert.Contains("research-attached-default", actionSurfaceText, StringComparison.Ordinal);
        Assert.Contains("localized-choice-matrix-formed", actionSurfaceText, StringComparison.Ordinal);

        Assert.Contains("install-agreement-begin", lispModuleText, StringComparison.Ordinal);
        Assert.Contains("install-agreement-predicate", lispModuleText, StringComparison.Ordinal);
        Assert.Contains("install-agreement-posture", lispModuleText, StringComparison.Ordinal);
        Assert.Contains("research-attached-default", lispModuleText, StringComparison.Ordinal);

        Assert.Contains("..\\..\\San\\San.Common\\San.Common.csproj", csprojText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Rtme_Service_Lift_Corridor_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var registerPath = Path.Combine(v121Root, "docs", "SANCTUARY_INTENDED_SERVICE_REGISTER.md");
        var serviceLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_LISP_RTME_SERVICE_LAW.md");
        var bindingLawPath = Path.Combine(v121Root, "docs", "SLI_ENGINE_LISP_BINDING_CONTRACT.md");
        var skeletonPath = Path.Combine(v121Root, "docs", "SANCTUARYID_RTME_SKELETON.md");
        var braidPath = Path.Combine(v121Root, "docs", "MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md");
        var placementPath = Path.Combine(v121Root, "docs", "CME_PLACEMENT_WITHHELD_LAW.md");
        var preconditionsLawPath = Path.Combine(v121Root, "docs", "SERVICE_LIFT_PRECONDITIONS_AND_REFUSAL_LAW.md");
        var authorityLawPath = Path.Combine(v121Root, "docs", "RTME_SERVICE_LIFT_AUTHORITY_LAW.md");
        var bindingLiftLawPath = Path.Combine(v121Root, "docs", "PRIME_SUPERVISION_AND_CRYPTIC_WORK_BINDING_LAW.md");
        var receiptLawPath = Path.Combine(v121Root, "docs", "CRYPTIC_HOSTED_BINDING_LIFT_RECEIPT_LAW.md");
        var commonContractsPath = Path.Combine(v121Root, "src", "San", "San.Common", "ServiceLiftContracts.cs");
        var lispContractsPath = Path.Combine(v121Root, "src", "SLI", "SLI.Lisp", "RtmeServiceLiftContracts.cs");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var registerText = File.ReadAllText(registerPath);
        var serviceLawText = File.ReadAllText(serviceLawPath);
        var bindingLawText = File.ReadAllText(bindingLawPath);
        var skeletonText = File.ReadAllText(skeletonPath);
        var braidText = File.ReadAllText(braidPath);
        var placementText = File.ReadAllText(placementPath);
        var preconditionsLawText = File.ReadAllText(preconditionsLawPath);
        var authorityLawText = File.ReadAllText(authorityLawPath);
        var bindingLiftLawText = File.ReadAllText(bindingLiftLawPath);
        var receiptLawText = File.ReadAllText(receiptLawPath);
        var commonContractsText = File.ReadAllText(commonContractsPath);
        var lispContractsText = File.ReadAllText(lispContractsPath);

        Assert.Contains("`first-working-model-rtme-service-lift-corridor: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RTME hosted service-lift corridor", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("templated-disabled", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("installed-disabled", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CmePlacementWithheld", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("hosted-posture change rather than governing identity", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SERVICE_LIFT_PRECONDITIONS_AND_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("RTME_SERVICE_LIFT_AUTHORITY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRIME_SUPERVISION_AND_CRYPTIC_WORK_BINDING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CRYPTIC_HOSTED_BINDING_LIFT_RECEIPT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("passive hosted braid", gateText, StringComparison.Ordinal);
        Assert.Contains("lawfully lift into hosted motion", gateText, StringComparison.Ordinal);
        Assert.Contains("No intended service entry becomes `authorized` in this slice.", gateText, StringComparison.Ordinal);
        Assert.Contains("Hosted service lift does not lift `CmePlacementWithheld`.", gateText, StringComparison.Ordinal);

        Assert.Contains("RTME hosted service-lift corridor now lives in", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive RTME service-lift contract family", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive RTME hosted service-lift receipt family", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("RTME hosted service-lift corridor clarification", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("intended service entry into `authorized`", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("service lift now changes hosted posture only", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("SERVICE_LIFT_PRECONDITIONS_AND_REFUSAL_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("RTME_SERVICE_LIFT_AUTHORITY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PRIME_SUPERVISION_AND_CRYPTIC_WORK_BINDING_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CRYPTIC_HOSTED_BINDING_LIFT_RECEIPT_LAW.md", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-rtme-service-lift-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Service lift preconditions and refusal law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("RTME service lift authority law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Prime supervision and Cryptic work binding law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Cryptic hosted binding lift receipt law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-activating", ledgerText, StringComparison.Ordinal);
        Assert.Contains("non-governing", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SERVICE_LIFT_PRECONDITIONS_AND_REFUSAL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`RTME_SERVICE_LIFT_AUTHORITY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PRIME_SUPERVISION_AND_CRYPTIC_WORK_BINDING_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CRYPTIC_HOSTED_BINDING_LIFT_RECEIPT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("hosted service-lift corridor", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("hosted-posture authority", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("governing `CME` placement", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("statuses remain `templated-disabled` or `installed-disabled`", registerText, StringComparison.Ordinal);
        Assert.Contains("no entry becomes `authorized`", registerText, StringComparison.Ordinal);
        Assert.Contains("service lift remains future motion only", registerText, StringComparison.Ordinal);

        Assert.Contains("The later RTME hosted service-lift corridor remains a separate future act.", serviceLawText, StringComparison.Ordinal);
        Assert.Contains("does not collapse the service-line", serviceLawText, StringComparison.Ordinal);
        Assert.Contains("present hosted activation", serviceLawText, StringComparison.Ordinal);

        Assert.Contains("The later RTME hosted service-lift corridor does not widen this contract into", bindingLawText, StringComparison.Ordinal);
        Assert.Contains("Carrier law remains prior to lift authority.", bindingLawText, StringComparison.Ordinal);

        Assert.Contains("The shell may later witness a hosted service-lift receipt", skeletonText, StringComparison.Ordinal);
        Assert.Contains("Skeleton standing remains non-realization standing", skeletonText, StringComparison.Ordinal);

        Assert.Contains("The later RTME hosted service-lift corridor must preserve this same read:", braidText, StringComparison.Ordinal);
        Assert.Contains("Prime supervision is witness and bounded oversight", braidText, StringComparison.Ordinal);
        Assert.Contains("Cryptic work is preparation and bounded hosted carriage", braidText, StringComparison.Ordinal);

        Assert.Contains("No RTME hosted service-lift surface may be misread as lifting", placementText, StringComparison.Ordinal);
        Assert.Contains("Hosted motion and governing placement remain distinct acts.", placementText, StringComparison.Ordinal);

        Assert.Contains("hosted residency is not hosted activation.", preconditionsLawText, StringComparison.Ordinal);
        Assert.Contains("service lift changes hosting posture, not governing identity.", preconditionsLawText, StringComparison.Ordinal);
        Assert.Contains("no hosted motion may be treated as lawful unless the preconditions,", preconditionsLawText, StringComparison.Ordinal);
        Assert.Contains("authority surface, supervision binding, and receipt chain are all present", preconditionsLawText, StringComparison.Ordinal);
        Assert.Contains("RtmeServiceLiftPreconditionSnapshot", preconditionsLawText, StringComparison.Ordinal);
        Assert.Contains("ServiceLiftRefusalReason", preconditionsLawText, StringComparison.Ordinal);

        Assert.Contains("supervisory witness is not service authorization.", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("service lift is not governing `CME` placement.", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("No service entry becomes `authorized` in this slice.", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("RtmeServiceLiftAuthorityRecord", authorityLawText, StringComparison.Ordinal);

        Assert.Contains("Prime witnesses and supervises. Cryptic carries the working side.", bindingLiftLawText, StringComparison.Ordinal);
        Assert.Contains("cryptic work is not runtime autonomy.", bindingLiftLawText, StringComparison.Ordinal);
        Assert.Contains("PrimeCrypticLiftBindingRecord", bindingLiftLawText, StringComparison.Ordinal);
        Assert.Contains("non-governing", bindingLiftLawText, StringComparison.Ordinal);

        Assert.Contains("receipt proves transition posture, not governing identity.", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("HostedRtmeServiceLiftReceipt", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("RtmeServiceLiftPreconditionSnapshot", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("CmePlacementWithheld", receiptLawText, StringComparison.Ordinal);

        Assert.Contains("public sealed record ServiceLiftRefusalReason(", commonContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record RtmeServiceLiftPreconditionSnapshot(", commonContractsText, StringComparison.Ordinal);
        Assert.Contains("bool CanonicalFloorSetReady", commonContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record RtmeServiceLiftAuthorityRecord(", commonContractsText, StringComparison.Ordinal);
        Assert.Contains("string PostLiftTargetStatus", commonContractsText, StringComparison.Ordinal);

        Assert.Contains("public sealed record PrimeCrypticLiftBindingRecord(", lispContractsText, StringComparison.Ordinal);
        Assert.Contains("string PrimeSupervisoryWitnessSurface", lispContractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record HostedRtmeServiceLiftReceipt(", lispContractsText, StringComparison.Ordinal);
        Assert.Contains("string CmePlacementWithheldContinuation", lispContractsText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Governance_Memory_Seat_Corridor_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var stackMapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md");
        var gelBootstrapPath = Path.Combine(v121Root, "docs", "SANCTUARY_GEL_BOOTSTRAP_LAW.md");
        var admittedObjectPath = Path.Combine(v121Root, "docs", "GEL_ADMITTED_INTERIOR_OBJECT_LAW.md");
        var restAuditPath = Path.Combine(v121Root, "docs", "GEL_REST_STATE_AUDIT.md");
        var placementPath = Path.Combine(v121Root, "docs", "CME_PLACEMENT_WITHHELD_LAW.md");
        var nonAssumptionPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_NON_ASSUMPTION_LAW.md");
        var spawnLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_GOA_CRADLETEK_GOA_AND_CGOA_SPAWN_LAW.md");
        var seatLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_MOS_STORAGE_SEAT_LAW.md");
        var standingLawPath = Path.Combine(v121Root, "docs", "OE_AND_SELFGEL_STRUCTURAL_STANDING_LAW.md");
        var surfacingLawPath = Path.Combine(v121Root, "docs", "CMOS_COE_AND_CSELFGEL_SURFACING_LAW.md");
        var handoffLawPath = Path.Combine(v121Root, "docs", "GEL_CGEL_TO_MOS_HANDOFF_AND_RECEIPT_LAW.md");
        var refusalLawPath = Path.Combine(v121Root, "docs", "MOS_STORAGE_SEAT_REFUSAL_AND_BOUNDARY_LAW.md");
        var auditPath = Path.Combine(v121Root, "docs", "MOS_STORAGE_SEAT_AUDIT.md");
        var contractsPath = Path.Combine(v121Root, "src", "San", "San.Common", "MosStorageSeatContracts.cs");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var stackMapText = File.ReadAllText(stackMapPath);
        var gelBootstrapText = File.ReadAllText(gelBootstrapPath);
        var admittedObjectText = File.ReadAllText(admittedObjectPath);
        var restAuditText = File.ReadAllText(restAuditPath);
        var placementText = File.ReadAllText(placementPath);
        var nonAssumptionText = File.ReadAllText(nonAssumptionPath);
        var spawnLawText = File.ReadAllText(spawnLawPath);
        var seatLawText = File.ReadAllText(seatLawPath);
        var standingLawText = File.ReadAllText(standingLawPath);
        var surfacingLawText = File.ReadAllText(surfacingLawPath);
        var handoffLawText = File.ReadAllText(handoffLawPath);
        var refusalLawText = File.ReadAllText(refusalLawPath);
        var auditText = File.ReadAllText(auditPath);
        var contractsText = File.ReadAllText(contractsPath);

        Assert.Contains("`first-working-model-governance-memory-seat: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.MoS and Sanctuary.cMoS", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL/cGEL -> MoS/cMoS`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("pre-Cradle authorization", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("- `hold`", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_MOS_STORAGE_SEAT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("OE_AND_SELFGEL_STRUCTURAL_STANDING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CMOS_COE_AND_CSELFGEL_SURFACING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_CGEL_TO_MOS_HANDOFF_AND_RECEIPT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("MOS_STORAGE_SEAT_REFUSAL_AND_BOUNDARY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("MOS_STORAGE_SEAT_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("governance-bearing memory has a", gateText, StringComparison.Ordinal);
        Assert.Contains("lawful seat is truthful", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize pre-Cradle Sanctuary authorization.", gateText, StringComparison.Ordinal);
        Assert.Contains("Storage-seat standing is not pre-Cradle authorization.", gateText, StringComparison.Ordinal);
        Assert.Contains("Pre-Cradle authorization is not governing `CME` placement.", gateText, StringComparison.Ordinal);

        Assert.Contains("governance-memory-seat corridor now fixes `Sanctuary.MoS`", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive governance-memory-seat contract family", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`OE` and `SelfGEL` may now stand structurally", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`cOE` and `cSelfGEL` may now surface derivationally", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL/cGEL -> MoS/cMoS` handoff is now receipted continuity only", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_MOS_STORAGE_SEAT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("OE_AND_SELFGEL_STRUCTURAL_STANDING_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CMOS_COE_AND_CSELFGEL_SURFACING_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("GEL_CGEL_TO_MOS_HANDOFF_AND_RECEIPT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("MOS_STORAGE_SEAT_REFUSAL_AND_BOUNDARY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("MOS_STORAGE_SEAT_AUDIT.md", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-governance-memory-seat-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.MoS storage seat law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("OE and SelfGEL structural standing law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("cMoS, cOE, and cSelfGEL surfacing law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("GEL/cGEL to MoS/cMoS handoff and receipt law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("MoS storage seat refusal and boundary law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("MoS storage seat audit", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SANCTUARY_MOS_STORAGE_SEAT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`OE_AND_SELFGEL_STRUCTURAL_STANDING_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CMOS_COE_AND_CSELFGEL_SURFACING_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`GEL_CGEL_TO_MOS_HANDOFF_AND_RECEIPT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`MOS_STORAGE_SEAT_REFUSAL_AND_BOUNDARY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`MOS_STORAGE_SEAT_AUDIT.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("governance-memory-seat corridor", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.cMoS", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("storage-seat refusal boundary", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.cMoS", stackMapText, StringComparison.Ordinal);
        Assert.Contains("`cOE` and `cSelfGEL` may surface in `Sanctuary.cMoS` derivationally", stackMapText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.MoS` and `Sanctuary.cMoS` remain later storage-seat surfaces", gelBootstrapText, StringComparison.Ordinal);
        Assert.Contains("receipted handoff", gelBootstrapText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.MoS` or", admittedObjectText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cMoS` storage-seat standing without receipted handoff", admittedObjectText, StringComparison.Ordinal);
        Assert.Contains("non-`cMoS` witness", restAuditText, StringComparison.Ordinal);

        Assert.Contains("No `MoS` or `cMoS` storage-seat surface may be misread as lifting", placementText, StringComparison.Ordinal);
        Assert.Contains("`MoS/cMoS` standing does not mean CradleTek authorization", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("Later `cOE` and `cSelfGEL` remain derivational and non-sovereign", spawnLawText, StringComparison.Ordinal);

        Assert.Contains("`Sanctuary.GEL` is first substrate, not final governance store.", seatLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` is build chamber, not legal seat.", seatLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.MoS` is storage-seat standing, not governing `CME` placement.", seatLawText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cMoS` is derivational and private/cryptic", seatLawText, StringComparison.Ordinal);
        Assert.Contains("MosStorageSeatRecord", seatLawText, StringComparison.Ordinal);
        Assert.Contains("CMosSurfaceRecord", seatLawText, StringComparison.Ordinal);

        Assert.Contains("structural standing for `OE` and `SelfGEL` is not active governing life.", standingLawText, StringComparison.Ordinal);
        Assert.Contains("no held `GEL` object promotes itself", standingLawText, StringComparison.Ordinal);
        Assert.Contains("OeStructuralStandingRecord", standingLawText, StringComparison.Ordinal);
        Assert.Contains("SelfGelStructuralStandingRecord", standingLawText, StringComparison.Ordinal);

        Assert.Contains("no direct `cGEL -> cOE/cSelfGEL` shortcut is lawful", surfacingLawText, StringComparison.Ordinal);
        Assert.Contains("CrypticDerivativeSurfaceKind", surfacingLawText, StringComparison.Ordinal);
        Assert.Contains("CrypticDerivativeSurfacingRecord", surfacingLawText, StringComparison.Ordinal);
        Assert.Contains("second sovereign origin", surfacingLawText, StringComparison.Ordinal);

        Assert.Contains("handoff is receipted continuity, not promotion.", handoffLawText, StringComparison.Ordinal);
        Assert.Contains("resting `GEL` persistence does not imply `MoS` standing.", handoffLawText, StringComparison.Ordinal);
        Assert.Contains("GelCgelToMosHandoffReceipt", handoffLawText, StringComparison.Ordinal);

        Assert.Contains("storage-seat standing is not pre-Cradle authorization.", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("pre-Cradle authorization is not governing `CME` placement.", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("MosStorageSeatRefusalReason", refusalLawText, StringComparison.Ordinal);
        Assert.Contains("`cOE/cSelfGEL` as independent sovereignty", refusalLawText, StringComparison.Ordinal);

        Assert.Contains("governing readiness is not governing presence.", auditText, StringComparison.Ordinal);
        Assert.Contains("lawful `GEL/cGEL -> MoS/cMoS` handoff receipt", auditText, StringComparison.Ordinal);
        Assert.Contains("any reading of `cOE/cSelfGEL` as independent sovereign origin", auditText, StringComparison.Ordinal);

        Assert.Contains("public sealed record MosStorageSeatRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CMosSurfaceRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record OeStructuralStandingRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record SelfGelStructuralStandingRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum CrypticDerivativeSurfaceKind", contractsText, StringComparison.Ordinal);
        Assert.Contains("COe = 0", contractsText, StringComparison.Ordinal);
        Assert.Contains("CSelfGel = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CrypticDerivativeSurfacingRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record GelCgelToMosHandoffReceipt(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record MosStorageSeatRefusalReason(", contractsText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Pre_Cradle_Site_Authorization_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var installActionLawPath = Path.Combine(v121Root, "docs", "INSTALL_AGREEMENT_ACTION_SURFACE_LAW.md");
        var postureLawPath = Path.Combine(v121Root, "docs", "CME_USE_DATA_POSTURE_LAW.md");
        var liftPreconditionsPath = Path.Combine(v121Root, "docs", "SERVICE_LIFT_PRECONDITIONS_AND_REFUSAL_LAW.md");
        var serviceRegisterPath = Path.Combine(v121Root, "docs", "SANCTUARY_INTENDED_SERVICE_REGISTER.md");
        var seatLawPath = Path.Combine(v121Root, "docs", "SANCTUARY_MOS_STORAGE_SEAT_LAW.md");
        var operatorIngressPath = Path.Combine(v121Root, "docs", "OPERATOR_INGRESS_PRECONDITIONS.md");
        var nonAssumptionPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_NON_ASSUMPTION_LAW.md");
        var placementPath = Path.Combine(v121Root, "docs", "CME_PLACEMENT_WITHHELD_LAW.md");
        var siteLawPath = Path.Combine(v121Root, "docs", "CRADLETEK_SITE_BINDING_PROFILE_LAW.md");
        var trainingLawPath = Path.Combine(v121Root, "docs", "OPERATOR_TRAINING_SET_ADMISSION_LAW.md");
        var toolLawPath = Path.Combine(v121Root, "docs", "TOOL_STATE_AUTHORIZATION_LAW.md");
        var disclosureLawPath = Path.Combine(v121Root, "docs", "FINAL_CME_DISCLOSURE_AGREEMENT_LAW.md");
        var ipLawPath = Path.Combine(v121Root, "docs", "CME_IP_INHERITANCE_AND_CREATION_SCOPE_LAW.md");
        var authorityLawPath = Path.Combine(v121Root, "docs", "CUSTODIAL_AND_BRAND_CONTENT_AUTHORITY_LAW.md");
        var candidateLawPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_SITE_AUTHORIZATION_CANDIDATE_LAW.md");
        var receiptLawPath = Path.Combine(v121Root, "docs", "PRE_CRADLETEK_AUTHORIZATION_RECEIPT_AND_REFUSAL_LAW.md");
        var contractsPath = Path.Combine(v121Root, "src", "San", "San.Common", "PreCradleSiteAuthorizationContracts.cs");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var installActionLawText = File.ReadAllText(installActionLawPath);
        var postureLawText = File.ReadAllText(postureLawPath);
        var liftPreconditionsText = File.ReadAllText(liftPreconditionsPath);
        var serviceRegisterText = File.ReadAllText(serviceRegisterPath);
        var seatLawText = File.ReadAllText(seatLawPath);
        var operatorIngressText = File.ReadAllText(operatorIngressPath);
        var nonAssumptionText = File.ReadAllText(nonAssumptionPath);
        var placementText = File.ReadAllText(placementPath);
        var siteLawText = File.ReadAllText(siteLawPath);
        var trainingLawText = File.ReadAllText(trainingLawPath);
        var toolLawText = File.ReadAllText(toolLawPath);
        var disclosureLawText = File.ReadAllText(disclosureLawPath);
        var ipLawText = File.ReadAllText(ipLawPath);
        var authorityLawText = File.ReadAllText(authorityLawPath);
        var candidateLawText = File.ReadAllText(candidateLawPath);
        var receiptLawText = File.ReadAllText(receiptLawPath);
        var contractsText = File.ReadAllText(contractsPath);

        Assert.Contains("`first-working-model-pre-cradle-site-authorization: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("site-bound pre-Cradle authorization", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("tool-state authorization", v111BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("scoped IP/content", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("- `hold`", gateText, StringComparison.Ordinal);
        Assert.Contains("CRADLETEK_SITE_BINDING_PROFILE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_TRAINING_SET_ADMISSION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("TOOL_STATE_AUTHORIZATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("FINAL_CME_DISCLOSURE_AGREEMENT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CME_IP_INHERITANCE_AND_CREATION_SCOPE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CUSTODIAL_AND_BRAND_CONTENT_AUTHORITY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_SITE_AUTHORIZATION_CANDIDATE_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_AUTHORIZATION_RECEIPT_AND_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("Having a lawful seat does not imply the right to occupy it as a governing", gateText, StringComparison.Ordinal);
        Assert.Contains("Authorized pre-Cradle standing is a site-and-operator posture", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize seed-body empowerment.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not move any intended service entry into `authorized`.", gateText, StringComparison.Ordinal);

        Assert.Contains("first site-bound pre-Cradle authorization cluster now lives in `docs/`", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first passive pre-Cradle site-authorization contract family", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("closed site", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("site-bound operator training admission", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("one final `CME` disclosure bundle", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("IP/content authority bundle", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", charterText, StringComparison.Ordinal);
        Assert.Contains("CRADLETEK_SITE_BINDING_PROFILE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_TRAINING_SET_ADMISSION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("TOOL_STATE_AUTHORIZATION_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("FINAL_CME_DISCLOSURE_AGREEMENT_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CME_IP_INHERITANCE_AND_CREATION_SCOPE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("CUSTODIAL_AND_BRAND_CONTENT_AUTHORITY_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_SITE_AUTHORIZATION_CANDIDATE_LAW.md", charterText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_AUTHORIZATION_RECEIPT_AND_REFUSAL_LAW.md", charterText, StringComparison.Ordinal);

        Assert.Contains("admitted-pre-cradle-site-authorization-law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CradleTek site binding profile law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Operator training set admission law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Tool state authorization law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Final CME disclosure agreement law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("CME IP inheritance and creation scope law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Custodial and brand content authority law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Pre-Cradle site authorization candidate law", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Pre-Cradle authorization receipt and refusal law", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`CRADLETEK_SITE_BINDING_PROFILE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`OPERATOR_TRAINING_SET_ADMISSION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`TOOL_STATE_AUTHORIZATION_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`FINAL_CME_DISCLOSURE_AGREEMENT_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CME_IP_INHERITANCE_AND_CREATION_SCOPE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`CUSTODIAL_AND_BRAND_CONTENT_AUTHORITY_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PRE_CRADLETEK_SITE_AUTHORIZATION_CANDIDATE_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`PRE_CRADLETEK_AUTHORIZATION_RECEIPT_AND_REFUSAL_LAW.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("closed site profile", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("scoped inheritance-versus-creation IP authority", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("site-bound pre-Cradle authorization only", installActionLawText, StringComparison.Ordinal);
        Assert.Contains("CRADLETEK_SITE_BINDING_PROFILE_LAW.md", postureLawText, StringComparison.Ordinal);
        Assert.Contains("site-bound pre-Cradle standing", postureLawText, StringComparison.Ordinal);
        Assert.Contains("CRADLETEK_SITE_BINDING_PROFILE_LAW.md", liftPreconditionsText, StringComparison.Ordinal);
        Assert.Contains("Lift readiness may support later site-bound pre-Cradle standing.", liftPreconditionsText, StringComparison.Ordinal);
        Assert.Contains("no site-bound pre-Cradle authorization receipt may move a service entry into", serviceRegisterText, StringComparison.Ordinal);
        Assert.Contains("having a lawful seat does not imply the right to occupy it as a governing", seatLawText, StringComparison.Ordinal);
        Assert.Contains("Site-bound operator training admission is not later", operatorIngressText, StringComparison.Ordinal);
        Assert.Contains("site binding does not mean CradleTek authorization", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("operator training admission does not mean operator realization", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("content authority does not mean automatic `CME` inheritance", nonAssumptionText, StringComparison.Ordinal);
        Assert.Contains("No pre-Cradle site-authorization receipt may be misread as lifting", placementText, StringComparison.Ordinal);

        Assert.Contains("CradleTekSiteClass", siteLawText, StringComparison.Ordinal);
        Assert.Contains("PersonalPc", siteLawText, StringComparison.Ordinal);
        Assert.Contains("EnterpriseDistributedConstruct", siteLawText, StringComparison.Ordinal);
        Assert.Contains("authorized pre-Cradle standing is a site-and-operator posture, not a", siteLawText, StringComparison.Ordinal);
        Assert.Contains("CradleTekSiteBindingProfile", siteLawText, StringComparison.Ordinal);

        Assert.Contains("OperatorTrainingAdmissionRecord", trainingLawText, StringComparison.Ordinal);
        Assert.Contains("operator realization", trainingLawText, StringComparison.Ordinal);

        Assert.Contains("ToolAuthorizationState", toolLawText, StringComparison.Ordinal);
        Assert.Contains("ToolStateAuthorizationRecord", toolLawText, StringComparison.Ordinal);
        Assert.Contains("service activation", toolLawText, StringComparison.Ordinal);
        Assert.Contains("runtime autonomy", toolLawText, StringComparison.Ordinal);
        Assert.Contains("governing action", toolLawText, StringComparison.Ordinal);

        Assert.Contains("FinalCmeDisclosureAgreementBundle", disclosureLawText, StringComparison.Ordinal);
        Assert.Contains("remains distinct from the content-authority bundle", disclosureLawText, StringComparison.Ordinal);

        Assert.Contains("submitted IP does not automatically become `CME` inheritance.", ipLawText, StringComparison.Ordinal);
        Assert.Contains("authorized use in creation does not automatically grant identity", ipLawText, StringComparison.Ordinal);
        Assert.Contains("InheritedIpScopeRecord", ipLawText, StringComparison.Ordinal);
        Assert.Contains("CreationUseScopeRecord", ipLawText, StringComparison.Ordinal);
        Assert.Contains("CmeContentAuthorityBundle", ipLawText, StringComparison.Ordinal);

        Assert.Contains("rights holder", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("guardian/custodial authority", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("CustodialAuthorityRecord", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("BrandAuthorityRecord", authorityLawText, StringComparison.Ordinal);
        Assert.Contains("brand/campaign preparation remain distinct", authorityLawText, StringComparison.Ordinal);

        Assert.Contains("InstallIdentitySetCandidate -> CoreCmeUsePostureRecord -> RtmeServiceLiftPreconditionSnapshot -> Mos/cMos readiness -> CradleTekSiteBindingProfile -> OperatorTrainingAdmissionRecord -> ToolStateAuthorizationRecord[] -> FinalCmeDisclosureAgreementBundle -> CmeContentAuthorityBundle -> PreCradleSiteAuthorizationCandidate", candidateLawText, StringComparison.Ordinal);
        Assert.Contains("site-and-operator posture, not a", candidateLawText, StringComparison.Ordinal);
        Assert.Contains("PreCradleSiteAuthorizationCandidate", candidateLawText, StringComparison.Ordinal);

        Assert.Contains("PreCradleAuthorizationReceipt", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("PreCradleAuthorizationRefusalReason", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("no intended-service entry becomes `authorized`", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("seed-body empowerment", receiptLawText, StringComparison.Ordinal);

        Assert.Contains("public enum CradleTekSiteClass", contractsText, StringComparison.Ordinal);
        Assert.Contains("PersonalPc = 0", contractsText, StringComparison.Ordinal);
        Assert.Contains("CoResidentSanctuaryHost = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("SeparateCradleTekHost = 2", contractsText, StringComparison.Ordinal);
        Assert.Contains("EnterpriseDistributedConstruct = 3", contractsText, StringComparison.Ordinal);
        Assert.Contains("public enum ToolAuthorizationState", contractsText, StringComparison.Ordinal);
        Assert.Contains("Authorized = 0", contractsText, StringComparison.Ordinal);
        Assert.Contains("Withheld = 1", contractsText, StringComparison.Ordinal);
        Assert.Contains("Refused = 2", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CradleTekSiteBindingProfile(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record OperatorTrainingAdmissionRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record ToolStateAuthorizationRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record FinalCmeDisclosureAgreementBundle(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record InheritedIpScopeRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CreationUseScopeRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CustodialAuthorityRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record BrandAuthorityRecord(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record CmeContentAuthorityBundle(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record PreCradleSiteAuthorizationCandidate(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record PreCradleAuthorizationRefusalReason(", contractsText, StringComparison.Ordinal);
        Assert.Contains("public sealed record PreCradleAuthorizationReceipt(", contractsText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Sli_Prime_Cryptic_Duplex_Resonance_Note_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var v111BuildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121BuildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var braidPosturePath = Path.Combine(v121Root, "docs", "MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md");
        var resonanceNotePath = Path.Combine(v121Root, "docs", "SLI_PRIME_CRYPTIC_DUPLEX_RESONANCE_NOTE.md");

        var v111BuildReadinessText = File.ReadAllText(v111BuildReadinessPath);
        var gateText = File.ReadAllText(gatePath);
        var v121BuildReadinessText = File.ReadAllText(v121BuildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var braidPostureText = File.ReadAllText(braidPosturePath);
        var resonanceNoteText = File.ReadAllText(resonanceNotePath);

        Assert.Contains("`prime-cryptic-duplex-resonance-note: frame-now`", v111BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI_PRIME_CRYPTIC_DUPLEX_RESONANCE_NOTE.md", gateText, StringComparison.Ordinal);
        Assert.Contains("`Prime` holds, `Cryptic` presses", gateText, StringComparison.Ordinal);
        Assert.Contains("does not add a new install stage", gateText, StringComparison.Ordinal);

        Assert.Contains("descriptive `SLI` Prime/Cryptic duplex resonance note", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("does not add a new carried seam", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Prime` holds, `Cryptic`", v121BuildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SLI` harmonizes, `Compass` sounds", v121BuildReadinessText, StringComparison.Ordinal);

        Assert.Contains("SLI_PRIME_CRYPTIC_DUPLEX_RESONANCE_NOTE.md", charterText, StringComparison.Ordinal);
        Assert.Contains("descriptive living-state", charterText, StringComparison.Ordinal);
        Assert.Contains("without adding a new install-first stage", charterText, StringComparison.Ordinal);

        Assert.Contains("SLI Prime/Cryptic duplex resonance note", ledgerText, StringComparison.Ordinal);
        Assert.Contains("`admitted-duplex-resonance-note`", ledgerText, StringComparison.Ordinal);
        Assert.Contains("hold without losing what has not yet resolved", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`SLI_PRIME_CRYPTIC_DUPLEX_RESONANCE_NOTE.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("descriptive `SLI` Prime/Cryptic duplex resonance", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("SLI_PRIME_CRYPTIC_DUPLEX_RESONANCE_NOTE.md", braidPostureText, StringComparison.Ordinal);
        Assert.Contains("harmonic resonance", braidPostureText, StringComparison.Ordinal);

        Assert.Contains("This is a descriptive invariant note.", resonanceNoteText, StringComparison.Ordinal);
        Assert.Contains("`Prime` holds. `Cryptic` presses. `Compass` sounds.", resonanceNoteText, StringComparison.Ordinal);
        Assert.Contains("the system returns to hold without losing what has not yet resolved", resonanceNoteText, StringComparison.Ordinal);
        Assert.Contains("`Prime` and `Cryptic` are not two separate systems.", resonanceNoteText, StringComparison.Ordinal);
        Assert.Contains("This is the heartbeat of self among othering.", resonanceNoteText, StringComparison.Ordinal);
        Assert.Contains("This note does not grant:", resonanceNoteText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Domain_And_Spline_Groupoid_Audit_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var workflowMapPath = Path.Combine(lineRoot, "docs", "V1_1_1_WORKFLOW_MILESTONE_MAP.md");
        var groupoidAuditPath = Path.Combine(lineRoot, "docs", "V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var workflowMapText = File.ReadAllText(workflowMapPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);

        Assert.Contains("V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md", workflowMapText, StringComparison.Ordinal);

        var doctrineMarkers = new[]
        {
            "`Sanctuary -> CradleTek -> SoulFrame -> AgentiCore`",
            "`Install -> Build -> Run -> Rest -> Exit`",
            "`SLI` and line-governance notes currently condense upward into `Sanctuary`",
            "### `Sanctuary / Install`",
            "`FIRST_RUN_CONSTITUTION.md`",
            "### `CradleTek / Run`",
            "`RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md`",
            "### `SoulFrame / Rest`",
            "`CME_RETURN_AUDIT_AND_PROMOTION_LAW.md`",
            "### `AgentiCore / Run`",
            "`AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md`"
        };

        foreach (var marker in doctrineMarkers)
        {
            Assert.Contains(marker, groupoidAuditText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Telemetry_Bundle_And_Groupoid_Taxonomy_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var groupoidAuditPath = Path.Combine(lineRoot, "docs", "V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var taxonomyPath = Path.Combine(lineRoot, "docs", "TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md");
        var schemaPath = Path.Combine(lineRoot, "docs", "LINE_AUDIT_REPORT_SCHEMA.md");
        var companionTelemetryPath = Path.Combine(lineRoot, "docs", "COMPANION_TOOL_TELEMETRY_LANE.md");
        var sourceBucketConsumptionPath = Path.Combine(lineRoot, "docs", "SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var schemaText = File.ReadAllText(schemaPath);
        var companionTelemetryText = File.ReadAllText(companionTelemetryPath);
        var sourceBucketConsumptionText = File.ReadAllText(sourceBucketConsumptionPath);

        Assert.Contains("TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`telemetry-bundle-taxonomy: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`line-audit-report-schema: frame-now`", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("Audited active docs: `49`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("- `Sanctuary`: `35`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("telemetry classification across lanes", groupoidAuditText, StringComparison.Ordinal);

        var taxonomyMarkers = new[]
        {
            "> `bundle` names packaging, not semantic carrier class.",
            "Semantic class answers:",
            "Packaging class answers:",
            "## Semantic Carrier Classes",
            "`standing_surface`",
            "`appendix_packet`",
            "`receipt`",
            "`ledger`",
            "`witness`",
            "`envelope`",
            "`summary_digest`",
            "`candidate_packet`",
            "## Packaging Classes",
            "`state_surface`",
            "`run_bundle`",
            "`daily_bundle`",
            "`report_readout`",
            "`authorityClass`",
            "`transport -> evidence -> candidate -> admitted -> inherited`",
            "`continuityClass`",
            "`event -> session -> thread -> line -> sibling-line`",
            "`retentionClass`",
            "`rolling_state -> hourly_raw -> daily_compacted -> pinned_review -> durable`",
            "`packageClass`",
            "Every emitted telemetry surface should now be readable as:",
            "{ domain, spline, semanticClass, authorityClass, continuityClass, retentionClass, packageClass }",
            "This note does not require mass renaming of existing `...Bundle` pointers.",
            "LINE_AUDIT_REPORT_SCHEMA.md"
        };

        foreach (var marker in taxonomyMarkers)
        {
            Assert.Contains(marker, taxonomyText, StringComparison.Ordinal);
        }

        Assert.Contains("`line-audit-report` is now schema-defined as a read-only line witness.", schemaText, StringComparison.Ordinal);

        Assert.Contains("This lane follows `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`.", companionTelemetryText, StringComparison.Ordinal);
        Assert.Contains("the `.audit/state` file is a packaging surface", companionTelemetryText, StringComparison.Ordinal);
        Assert.Contains("the `.audit/runs` directory is a packaging surface", companionTelemetryText, StringComparison.Ordinal);

        Assert.Contains("This lane also follows `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`.", sourceBucketConsumptionText, StringComparison.Ordinal);
        Assert.Contains("raw appendices remain semantic transport", sourceBucketConsumptionText, StringComparison.Ordinal);
        Assert.Contains("consumed receipts, summaries, and candidate packets keep their semantic class", sourceBucketConsumptionText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Line_Audit_Report_Schema_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var taxonomyPath = Path.Combine(lineRoot, "docs", "TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md");
        var schemaPath = Path.Combine(lineRoot, "docs", "LINE_AUDIT_REPORT_SCHEMA.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var schemaText = File.ReadAllText(schemaPath);

        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`line-audit-report-schema: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", taxonomyText, StringComparison.Ordinal);

        var schemaMarkers = new[]
        {
            "This note defines the first read-only schema for `line-audit-report`",
            "`line-audit-report` must read the line through declared taxonomy and",
            "`line-audit-report-schema: frame-now`",
            "the first root read-only implementation is now admitted",
            "## Top-Level Schema",
            "`reportIdentity`",
            "`lineIdentity`",
            "`linePosture`",
            "`verificationStatus`",
            "`doctrineBraid`",
            "`telemetryTaxonomy`",
            "`telemetrySurfaceInventory`",
            "`warnings`",
            "`knownNoise`",
            "`unavailableOrUndeclared`",
            "Each item must preserve the taxonomy tuple:",
            "`surfaceName`",
            "`domain`",
            "`spline`",
            "`semanticClass`",
            "`authorityClass`",
            "`continuityClass`",
            "`retentionClass`",
            "`packageClass`",
            "`undeclared`",
            "`unavailable`",
            "The JSON readout must remain a structured witness, not a second semantic model."
        };

        foreach (var marker in schemaMarkers)
        {
            Assert.Contains(marker, schemaText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Line_Audit_Report_Root_Tool_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var toolPath = Path.Combine(repoRoot, "tools", "Get-LineAuditReport.ps1");

        var toolText = File.ReadAllText(toolPath);

        Assert.Contains("[string] $LineRoot = \"OAN Mortalis V1.1.1\"", toolText, StringComparison.Ordinal);
        Assert.Contains("[ValidateSet(\"Markdown\", \"Json\")]", toolText, StringComparison.Ordinal);
        Assert.Contains("Split-Path -Parent $PSScriptRoot", toolText, StringComparison.Ordinal);
        Assert.Contains("reportSurfaceName = \"line-audit-report\"", toolText, StringComparison.Ordinal);
        Assert.Contains("reportClass = \"read-only-line-witness\"", toolText, StringComparison.Ordinal);
        Assert.Contains("TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md", toolText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", toolText, StringComparison.Ordinal);
        Assert.Contains("verify-private-corpus.ps1", toolText, StringComparison.Ordinal);
        Assert.Contains("git -C $RepoRoot diff --check -- $LineRoot", toolText, StringComparison.Ordinal);
        Assert.Contains("OAN Mortalis V1.2.1", toolText, StringComparison.Ordinal);
        Assert.Contains("\"undeclared\"", toolText, StringComparison.Ordinal);
        Assert.Contains("\"unavailable\"", toolText, StringComparison.Ordinal);
        Assert.Contains("ConvertTo-Json -Depth 8", toolText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_First_Working_Model_Release_Gate_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var groupoidAuditPath = Path.Combine(lineRoot, "docs", "V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var gatePath = Path.Combine(lineRoot, "docs", "FIRST_WORKING_MODEL_RELEASE_GATE.md");
        var v121ReadinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.2.1", "docs", "BUILD_READINESS.md");
        var v121CharterPath = Path.Combine(repoRoot, "OAN Mortalis V1.2.1", "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var v121LedgerPath = Path.Combine(repoRoot, "OAN Mortalis V1.2.1", "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var gateText = File.ReadAllText(gatePath);
        var v121ReadinessText = File.ReadAllText(v121ReadinessPath);
        var v121CharterText = File.ReadAllText(v121CharterPath);
        var v121LedgerText = File.ReadAllText(v121LedgerPath);

        Assert.Contains("FIRST_WORKING_MODEL_RELEASE_GATE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-release-gate: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-seam-definition: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-rtme-shell-trace: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-carriage-schema: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-pre-cme-substrate: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-sanctuaryid-goa-governing-set: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-sanctuaryid-goa-root-and-cgel: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-sanctuary-gel-intake: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-gel-interior-awareness-and-universe: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-gel-rest-state: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`first-working-model-sli-symbolic-transport-form: frame-now`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("The line-local promotion gate below is subordinate", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("The active first-working-model gate now specifies", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("seam-definition batch for the next", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first non-executive `SanctuaryID.RTME`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first descriptive Lisp/C# carriage schema", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("pre-CME substrate cluster as specified", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary-side `SanctuaryID.GoA`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SanctuaryID.GoA` governance-root and", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.cGEL` stack-map cluster while preserving", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("first `Sanctuary.GEL` semantic intake", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("keeping predicate promotion,", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("engram minting, and runtime activation withheld.", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` interior-awareness and universe-law", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`GEL` rest-state cluster", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`SLI` symbolic transport-form cluster", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("shared root transport lineage", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("explicit non-mutation posture", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("FIRST_WORKING_MODEL_RELEASE_GATE.md", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("Audited active docs: `49`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("- `Sanctuary`: `35`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("first-working-model admissibility, line-role split, and", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("the first working model is not declared by momentum", gateText, StringComparison.Ordinal);
        Assert.Contains("the gate must be able to `pass`, `hold`, or `fail`", gateText, StringComparison.Ordinal);
        Assert.Contains("### `V1.1.1`", gateText, StringComparison.Ordinal);
        Assert.Contains("### `V1.2.1`", gateText, StringComparison.Ordinal);
        Assert.Contains("### Witness Surfaces", gateText, StringComparison.Ordinal);
        Assert.Contains("tools/Get-LineAuditReport.ps1", gateText, StringComparison.Ordinal);
        Assert.Contains("Current gate posture:", gateText, StringComparison.Ordinal);
        Assert.Contains("- `hold`", gateText, StringComparison.Ordinal);
        Assert.Contains("SLI_ENGINE_LISP_BINDING_CONTRACT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("OPERATOR_INGRESS_PRECONDITIONS.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SEAM_REFUSAL_AND_RETURN_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That batch now supports the next non-executive shell-and-trace batch:", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_RTME_SKELETON.md", gateText, StringComparison.Ordinal);
        Assert.Contains("FIRST_WORKING_MODEL_TRACE_PATH.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That shell-and-trace batch now supports the first descriptive carriage note:", gateText, StringComparison.Ordinal);
        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That carriage note now supports the next pre-CME substrate cluster:", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_ROOT_INSTALL_SURFACE.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_TEMPLATE_ADOPTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_INTENDED_SERVICE_REGISTER.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_PRE_CME_SUBSTRATE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before first governing `CME` presence is even", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize template adoption.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize service activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize local sovereignty.", gateText, StringComparison.Ordinal);
        Assert.Contains("That substrate cluster now supports the next `SanctuaryID.GoA`", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("MOTHER_FATHER_PRIME_CRYPTIC_BRAID_POSTURE.md", gateText, StringComparison.Ordinal);
        Assert.Contains("CME_PLACEMENT_WITHHELD_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_NON_ASSUMPTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PRE_CRADLETEK_GOVERNING_CME_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before governing `CME` can be discussed as active", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize CradleTek spawn.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize governing `CME` placement.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Steward governance.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("That governing-set clarification cluster now supports the next", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PREDICATE_TRUTH_BOUNDARY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_DEFAULT_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_AUTHORITY_AND_RECEIPT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_CGEL_MOS_GOVERNANCE_STACK_MAP.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_PRE_CRADLETEK_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim of first working governance root is", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize service authorization.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize Trivium standing.", gateText, StringComparison.Ordinal);
        Assert.Contains("Governance root standing may exist before CradleTek-dependent governing life,", gateText, StringComparison.Ordinal);
        Assert.Contains("That governance-root and `Sanctuary.cGEL` stack-map cluster now supports", gateText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` semantic intake cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ROOTATLAS_MAPPING_AND_VARIANT_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PREDICATE_CANDIDATE_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("IUTT_TRANSFORMATION_BOUNDARY_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("INGRESS_MEMBRANE_AND_ENGRAM_ENCODING_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARY_GEL_SEMANTIC_INTAKE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim that pre-Lisp or pre-code semantic", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize predicate promotion.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize engram minting.", gateText, StringComparison.Ordinal);
        Assert.Contains("It does not authorize runtime activation.", gateText, StringComparison.Ordinal);
        Assert.Contains("Intake may preserve, classify, map, transform, and encode for cryptic use,", gateText, StringComparison.Ordinal);
        Assert.Contains("That `Sanctuary.GEL` semantic intake cluster now supports the next", gateText, StringComparison.Ordinal);
        Assert.Contains("`Sanctuary.GEL` interior-awareness and universe-law cluster", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CONSTRUCTOR_CLASS_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PROPOSITIONAL_ENGRAM_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("PROCEDURAL_ENGRAM_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("LANGUAGE_UNIVERSE_GROUPOID_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("ENGRAM_POSTURE_AND_REFUSAL_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_SITUATIONAL_AWARENESS_FRAME_LAW.md", gateText, StringComparison.Ordinal);
        Assert.Contains("GEL_INTERIOR_AWARENESS_AND_UNIVERSE_AUDIT.md", gateText, StringComparison.Ordinal);
        Assert.Contains("That cluster is required before any claim that base `GEL` has lawful", gateText, StringComparison.Ordinal);
          Assert.Contains("Awareness remains derived posture, not autonomous life or action.", gateText, StringComparison.Ordinal);
          Assert.Contains("That `Sanctuary.GEL` rest-state and persistence cluster now supports the next", gateText, StringComparison.Ordinal);
          Assert.Contains("`SLI` symbolic transport-form cluster", gateText, StringComparison.Ordinal);
          Assert.Contains("UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md", gateText, StringComparison.Ordinal);
          Assert.Contains("ROOT_SYMBOL_BASE_FORM_LAW.md", gateText, StringComparison.Ordinal);
          Assert.Contains("SUPER_SUBSCRIPT_EXTENSION_FORMATION_LAW.md", gateText, StringComparison.Ordinal);
          Assert.Contains("SYMBOLIC_TRANSPORT_LINEAGE_AND_CONTEXT_SPLIT_LAW.md", gateText, StringComparison.Ordinal);
          Assert.Contains("SLI_SYMBOLIC_TRANSPORT_FORM_AUDIT.md", gateText, StringComparison.Ordinal);
          Assert.Contains("required before any claim that `SLI` is transport-ready for", gateText, StringComparison.Ordinal);
          Assert.Contains("Transport readiness preserves ancestry and formation law, but it does not", gateText, StringComparison.Ordinal);
          Assert.Contains("It does not authorize Atlas delta candidacy.", gateText, StringComparison.Ordinal);
          Assert.Contains("It does not authorize Atlas mutation.", gateText, StringComparison.Ordinal);
          Assert.Contains("It does not authorize operator realization.", gateText, StringComparison.Ordinal);
          Assert.Contains("They do not turn symbolic transport-form clarification into Atlas delta", gateText, StringComparison.Ordinal);
          Assert.Contains("The named runtime-binding seam remains unchanged:", gateText, StringComparison.Ordinal);
        Assert.Contains("live `SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME` service binding", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not switch the line.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not grant operator realization.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn trace into execution.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn carriage into consequence.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn substrate clarification into runtime permission.", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn governing-set clarification into active governing `CME`", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn governance-root clarification into service authorization,", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn semantic intake clarification into predicate promotion,", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn interior-awareness clarification into autonomous life,", gateText, StringComparison.Ordinal);
        Assert.Contains("They do not turn rest-state clarification into `Sanctuary.MoS` or", gateText, StringComparison.Ordinal);
        Assert.Contains("first `SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME` service binding", gateText, StringComparison.Ordinal);
        Assert.Contains("This note does not:", gateText, StringComparison.Ordinal);
        Assert.Contains("replace the `V1.1.1` promotion gate", gateText, StringComparison.Ordinal);
        Assert.Contains("grant operator realization by implication", gateText, StringComparison.Ordinal);

        Assert.Contains("release-form sibling body rather than active runtime truth", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("first-working-model admissibility pass", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", v121ReadinessText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md", v121ReadinessText, StringComparison.Ordinal);
          Assert.Contains("SANCTUARY_CGEL_CRYPTIC_BUILD_CHAMBER_LAW.md", v121ReadinessText, StringComparison.Ordinal);
          Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", v121ReadinessText, StringComparison.Ordinal);
          Assert.Contains("`Sanctuary.GEL` semantic intake clarification", v121ReadinessText, StringComparison.Ordinal);
          Assert.Contains("`SLI` symbolic transport-form clarification", v121ReadinessText, StringComparison.Ordinal);
          Assert.Contains("the first working-model release gate may later admit this line as", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("LISP_CSHARP_BINDING_SCHEMA.md", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNING_CME_SET_LAW.md", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("SANCTUARYID_GOA_GOVERNANCE_ROOT_LAW.md", v121CharterText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary.GEL semantic intake clarification -> Sanctuary.GEL interior awareness and universe-law clarification -> Sanctuary.GEL rest-state and persistence clarification -> SLI symbolic transport-form clarification -> install agreement action surface and identity footing clarification -> RTME hosted service-lift corridor clarification -> Sanctuary.MoS and Sanctuary.cMoS storage-seat and governance memory clarification -> pre-Cradle site-bound operator, tool, disclosure, and content authority clarification -> final working form", v121CharterText, StringComparison.Ordinal);
          Assert.Contains("ENGRAM_CLEAVING_LADDER_LAW.md", v121CharterText, StringComparison.Ordinal);
          Assert.Contains("UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md", v121CharterText, StringComparison.Ordinal);
          Assert.Contains("First working model release gate", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("release-form sibling body, not as the active executable truth", v121LedgerText, StringComparison.Ordinal);
        Assert.Contains("Lisp/C# binding schema", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("SanctuaryID.GoA governing CME set law", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("SanctuaryID.GoA governance root law", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("Engram cleaving ladder law", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("admitted-sanctuary-gel-intake-law", v121LedgerText, StringComparison.Ordinal);
          Assert.Contains("admitted-sli-symbolic-transport-law", v121LedgerText, StringComparison.Ordinal);
    }

    [Fact]
    public void V121_Telemetry_Taxonomy_And_Line_Audit_Report_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var v121Root = Path.Combine(repoRoot, "OAN Mortalis V1.2.1");
        var buildReadinessPath = Path.Combine(v121Root, "docs", "BUILD_READINESS.md");
        var charterPath = Path.Combine(v121Root, "docs", "V1_2_1_FIRST_INSTALL_CHARTER.md");
        var ledgerPath = Path.Combine(v121Root, "docs", "V1_2_1_CARRY_FORWARD_LEDGER.md");
        var groupoidAuditPath = Path.Combine(v121Root, "docs", "V1_2_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var taxonomyPath = Path.Combine(v121Root, "docs", "TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md");
        var schemaPath = Path.Combine(v121Root, "docs", "LINE_AUDIT_REPORT_SCHEMA.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var charterText = File.ReadAllText(charterPath);
        var ledgerText = File.ReadAllText(ledgerPath);
        var groupoidAuditText = File.ReadAllText(groupoidAuditPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var schemaText = File.ReadAllText(schemaPath);

        Assert.Contains("TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("tools/Get-LineAuditReport.ps1", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("emitted telemetry inventory remains thin", buildReadinessText, StringComparison.Ordinal);

        Assert.Contains("line-local telemetry taxonomy and read-only audit schema aligned to release", charterText, StringComparison.Ordinal);
        Assert.Contains("TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md", charterText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", charterText, StringComparison.Ordinal);

        Assert.Contains("Telemetry bundle and groupoid taxonomy", ledgerText, StringComparison.Ordinal);
        Assert.Contains("Read-only `line-audit-report` schema", ledgerText, StringComparison.Ordinal);

        AssertV121GroupoidCounts(groupoidAuditText);
        Assert.Contains("`TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("`LINE_AUDIT_REPORT_SCHEMA.md`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("release-alignment telemetry taxonomy or read-only audit", groupoidAuditText, StringComparison.Ordinal);

        Assert.Contains("> `bundle` names packaging, not semantic carrier class.", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("{ domain, spline, semanticClass, authorityClass, continuityClass, retentionClass, packageClass }", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("LINE_AUDIT_REPORT_SCHEMA.md", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("still forming", taxonomyText, StringComparison.Ordinal);

        Assert.Contains("`line-audit-report` must describe this line through declared taxonomy", schemaText, StringComparison.Ordinal);
        Assert.Contains("tools/Get-LineAuditReport.ps1", schemaText, StringComparison.Ordinal);
        Assert.Contains("the report may legitimately carry a thin or empty telemetry inventory", schemaText, StringComparison.Ordinal);
        Assert.Contains("`undeclared`", schemaText, StringComparison.Ordinal);
        Assert.Contains("`unavailable`", schemaText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_CradleTek_Runtime_Must_Not_Reference_AgentiCore_Directly()
    {
        var projectPath = GetProjectPath("CradleTek.Runtime");
        var projectText = File.ReadAllText(projectPath);

        Assert.DoesNotContain("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("San.Nexus.Control.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("San.Runtime.Materialization.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SoulFrame.Membrane.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("SoulFrame.Bootstrap.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_CradleTek_Host_Must_Expose_Body_Boundary_Through_Runtime_Only()
    {
        var projectPath = GetProjectPath("CradleTek.Host");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("CradleTek.Runtime.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_SoulFrame_Membrane_May_Ask_Nexus_But_Not_Own_Runtime_Or_Custody()
    {
        var projectPath = GetProjectPath("SoulFrame.Membrane");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Nexus.Control.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("AgentiCore.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Custody.csproj", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_AgentiCore_Must_Remain_Chambered_Cognition_And_Not_Depend_On_Body_Families()
    {
        var projectPath = GetProjectPath("AgentiCore");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
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
        Assert.DoesNotContain("San.Nexus.Control", projectText, StringComparison.Ordinal);
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
        var projectPath = GetProjectPath("San.PrimeCryptic.Services");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Hosted_Llm_Must_Remain_Sanctuary_Native()
    {
        var projectPath = GetProjectPath("San.HostedLlm");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Trace_Persistence_Must_Remain_Sanctuary_Native()
    {
        var projectPath = GetProjectPath("San.Trace.Persistence");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_State_Modulation_Must_Not_Depend_On_AgentiCore_Or_CradleTek_Runtime()
    {
        var projectPath = GetProjectPath("San.State.Modulation");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.PrimeCryptic.Services.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.Runtime", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Sli_Lisp_Must_Remain_Sanctuary_Native_Cryptic_Bundle()
    {
        var projectPath = GetProjectPath("SLI.Lisp");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Nexus_Control_Must_Remain_Sanctuary_Interface_Layer()
    {
        var projectPath = GetProjectPath("San.Nexus.Control");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Runtime_Materialization_Must_Remain_Sanctuary_Helper_Layer()
    {
        var projectPath = GetProjectPath("San.Runtime.Materialization");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.Common.csproj", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("CradleTek.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("AgentiCore", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("SoulFrame.", projectText, StringComparison.Ordinal);
        Assert.DoesNotContain("San.State.Modulation", projectText, StringComparison.Ordinal);
    }

    [Fact]
    public void V111_Runtime_Headless_Must_Compose_FirstRun_Projection()
    {
        var projectPath = GetProjectPath("San.Runtime.Headless");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("San.FirstRun.csproj", projectText, StringComparison.Ordinal);
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
        var cyclePolicyPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "Automation", "local-automation-cycle.json");

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
        Assert.Contains("`discernment-admissibility-law: frame-now`", buildReadinessText, StringComparison.Ordinal);
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
            "DISCERNMENT_AND_ADMISSIBILITY_LAW.md",
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
        Assert.Contains("CradleTek -> SLI -> SoulFrame -> Listening Frame -> Compass -> AgentiCore coordination -> cOE/cSelfGEL issued office", governanceLawText, StringComparison.Ordinal);
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
    public void V111_Discernment_And_Admissibility_Law_Is_Anchored_In_Active_Doctrine()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var workflowMapPath = Path.Combine(lineRoot, "docs", "V1_1_1_WORKFLOW_MILESTONE_MAP.md");
        var pathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var discernmentLawPath = Path.Combine(lineRoot, "docs", "DISCERNMENT_AND_ADMISSIBILITY_LAW.md");
        var discernmentCasebookPath = Path.Combine(lineRoot, "docs", "DISCERNMENT_AND_ADMISSIBILITY_CASEBOOK.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var workflowMapText = File.ReadAllText(workflowMapPath);
        var pathwayDocText = File.ReadAllText(pathwayDocPath);
        var discernmentLawText = File.ReadAllText(discernmentLawPath);
        var discernmentCasebookText = File.ReadAllText(discernmentCasebookPath);

        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_LAW.md", workflowMapText, StringComparison.Ordinal);
        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_LAW.md", pathwayDocText, StringComparison.Ordinal);
        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_CASEBOOK.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_CASEBOOK.md", workflowMapText, StringComparison.Ordinal);
        Assert.Contains("DISCERNMENT_AND_ADMISSIBILITY_CASEBOOK.md", pathwayDocText, StringComparison.Ordinal);

        var doctrineMarkers = new[]
        {
            "transport is not formation",
            "formation is not admission",
            "admission is not implementation",
            "`Dialectic` builds the predicate field.",
            "`Rhetoric` carries partially formed meaning between agents and surfaces.",
            "`Discernment` tests candidate structures against defined conditions.",
            "`Yes/No` records lawful closure under those conditions.",
            "No provisional structure earns promotion by repetition, fluency, urgency, or convenience.",
            "The primary corruption risk is not inability to produce.",
            "It is inability to refuse correctly.",
            "## Minimum Receipts For Promotion",
            "promotionDecision"
        };

        foreach (var marker in doctrineMarkers)
        {
            Assert.Contains(marker, discernmentLawText, StringComparison.Ordinal);
        }

        var casebookMarkers = new[]
        {
            "## Case Format",
            "CASE-PROVISIONAL-001",
            "CASE-PROMOTE-001",
            "CASE-HOLD-001",
            "CASE-REFUSE-001",
            "CASE-CATEGORY-ERROR-001",
            "CASE-INSUFFICIENT-RECEIPTS-001",
            "CASE-MODAL-SEPARATION-001",
            "runtime workbench-session ledger",
            "source-bucket return",
            "raw source-bucket repo drift",
            ".hopng",
            "awaiting-publishable-master-thread",
            "The law defines admissibility.",
            "The casebook teaches admissibility."
        };

        foreach (var marker in casebookMarkers)
        {
            Assert.Contains(marker, discernmentCasebookText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_AgentiCore_Listening_Frame_And_Compass_Minimal_Build_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var workflowMapPath = Path.Combine(lineRoot, "docs", "V1_1_1_WORKFLOW_MILESTONE_MAP.md");
        var pathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var minimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var workflowMapText = File.ReadAllText(workflowMapPath);
        var pathwayDocText = File.ReadAllText(pathwayDocPath);
        var minimalBuildText = File.ReadAllText(minimalBuildPath);

        Assert.Contains("AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md", workflowMapText, StringComparison.Ordinal);
        Assert.Contains("AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md", pathwayDocText, StringComparison.Ordinal);

        var doctrineMarkers = new[]
        {
            "content/substrate field -> Listening Frame projection -> Compass orientation and admissibility work -> AgentiCore coordination -> cOE/cSelfGEL issued office -> host-side authority decision",
            "CradleTek -> SLI -> SoulFrame -> Listening Frame -> Compass -> AgentiCore coordination -> cOE/cSelfGEL issued office",
            "`Listening Frame` is the auditable posture surface",
            "`Compass` is the orientation, drift, and admissibility read",
            "`AgentiCore` is the bounded coordination layer",
            "candidate and diagnostic tool states may become operationally usable inside",
            "final authority remains outside that layer",
            "The minimal build must provide exactly enough structure for:",
            "projection into a `Listening Frame`",
            "orientation and admissibility scoring",
            "candidate-only handoff into higher authority",
            "The clean family read for the minimal build is:",
            "`San.*` owns the contract and integration surfaces",
            "`SLI.*` owns the `Compass`-facing symbolic and evaluative seam",
            "`AgentiCore.*` owns bounded coordination and proposal behavior"
        };

        foreach (var marker in doctrineMarkers)
        {
            Assert.Contains(marker, minimalBuildText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Prime_Cryptic_And_Cme_Return_Laws_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var duplexLawPath = Path.Combine(lineRoot, "docs", "PRIME_CRYPTIC_DUPLEX_LAW.md");
        var instantiationLawPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var returnLawPath = Path.Combine(lineRoot, "docs", "CME_RETURN_AUDIT_AND_PROMOTION_LAW.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var duplexLawText = File.ReadAllText(duplexLawPath);
        var instantiationLawText = File.ReadAllText(instantiationLawPath);
        var returnLawText = File.ReadAllText(returnLawPath);

        Assert.Contains("PRIME_CRYPTIC_DUPLEX_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("MOS_CMOS_CGOA_INSTANTIATION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("CME_RETURN_AUDIT_AND_PROMOTION_LAW.md", buildReadinessText, StringComparison.Ordinal);

        var duplexMarkers = new[]
        {
            "`Prime` surfaces are readable, witnessable, canonical, and publication-safe",
            "`Cryptic` `c*` surfaces are hot, encrypted, mutable, and execution-bearing",
            "`Prime` recognizes and witnesses. `Cryptic` instantiates and works.",
            "Native `SLI` work under live `EC` belongs only in the `Cryptic` lane.",
            "Prime must not silently mutate from hot work"
        };

        foreach (var marker in duplexMarkers)
        {
            Assert.Contains(marker, duplexLawText, StringComparison.Ordinal);
        }

        var instantiationMarkers = new[]
        {
            "`Prime MoS verified match -> cMoS instantiation -> cGoA handshake -> SoulFrame hydration -> SLI web build -> AgentiCore template -> cOE/cSelfGEL sealed work office -> bonded close-state collapse -> runtime-scoped encrypted cGoA return staging`",
            "`MoS` is the readable and canonical mantle seat",
            "`cMoS` is the protected instantiated working seat",
            "`cGoA` is the protected handshake and staging surface",
            "When the bounded work session closes lawfully, the finished active `CME` does",
            "It first collapses into a bonded `SoulFrame` close-state.",
            "The first schema-facing receipt family for this seam should preserve at least:",
            "`PrimeMosMatchReceipt`",
            "`CrypticMosInstantiationReceipt`",
            "`BondedCrypticReturnBundle`"
        };

        foreach (var marker in instantiationMarkers)
        {
            Assert.Contains(marker, instantiationLawText, StringComparison.Ordinal);
        }

        var returnMarkers = new[]
        {
            "`CME` work ends in collapse, not promotion.",
            "The collapsed session yields a bonded cryptic return bundle.",
            "That bundle is staged in encrypted `cGoA`.",
            "Cleaving decides what may become autobiographical, local or research-valid,",
            "audit decides what may return.",
            "### Autobiographical cleave",
            "### Local or research cleave",
            "### Predicate or canonical cleave",
            "### Refusal or quarantine cleave",
            "`Mother`",
            "`Father`",
            "`Steward`",
            "`BondedCrypticReturnBundle`",
            "`CmeReturnCleaveReceipt`",
            "`CmeReturnCandidatePacket`",
            "`CmeReturnPromotionDecisionReceipt`",
            "bonded return bundle is not canonical update"
        };

        foreach (var marker in returnMarkers)
        {
            Assert.Contains(marker, returnLawText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Bonded_Cryptic_Return_Contracts_Are_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var instantiationLawPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var returnLawPath = Path.Combine(lineRoot, "docs", "CME_RETURN_AUDIT_AND_PROMOTION_LAW.md");
        var contractsPath = Path.Combine(lineRoot, "src", "Sanctuary", "San.Common", "San.Common", "BondedCrypticReturnContracts.cs");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var instantiationLawText = File.ReadAllText(instantiationLawPath);
        var returnLawText = File.ReadAllText(returnLawPath);
        var contractsText = File.ReadAllText(contractsPath);

        Assert.Contains("BondedCrypticReturnContracts.cs", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("BondedCrypticReturnContracts.cs", instantiationLawText, StringComparison.Ordinal);
        Assert.Contains("BondedCrypticReturnContracts.cs", returnLawText, StringComparison.Ordinal);

        var contractMarkers = new[]
        {
            "public enum CmeReturnCleaveClass",
            "Autobiographical = 0",
            "public enum CmeReturnPromotionDecision",
            "Escalate = 3",
            "public enum CmeReturnAuditOfficeKind",
            "public sealed record BondedSoulFrameCloseStateReceipt",
            "public sealed record CrypticGoaReturnStagingReceipt",
            "public sealed record BondedCrypticReturnBundle",
            "public sealed record CmeReturnCleaveReceipt",
            "public sealed record AutobiographicalCleaveCandidateReceipt",
            "public sealed record LocalResearchCleaveCandidateReceipt",
            "public sealed record PredicatePromotionCandidateReceipt",
            "public sealed record ReturnQuarantineReceipt",
            "public sealed record CmeReturnCandidatePacket",
            "public sealed record CmeReturnEvidenceReceipt",
            "public sealed record CmeReturnAuditReviewReceipt",
            "public sealed record CmeReturnPromotionDecisionReceipt",
            "public sealed record PrimeSurfacePromotionReceipt"
        };

        foreach (var marker in contractMarkers)
        {
            Assert.Contains(marker, contractsText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void V111_Sanctuary_Boot_First_Run_Ontology_Bridge_Is_Anchored()
    {
        var lineRoot = GetLineRoot();
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var bridgePath = Path.Combine(lineRoot, "docs", "SANCTUARY_BOOT_FIRST_RUN_ONTOLOGY_BRIDGE.md");
        var firstRunPath = Path.Combine(lineRoot, "docs", "FIRST_RUN_CONSTITUTION.md");
        var workbenchLawPath = Path.Combine(lineRoot, "docs", "RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md");
        var agentiCoreMinimalBuildPath = Path.Combine(lineRoot, "docs", "AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md");
        var instantiationLawPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");

        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var bridgeText = File.ReadAllText(bridgePath);
        var firstRunText = File.ReadAllText(firstRunPath);
        var workbenchLawText = File.ReadAllText(workbenchLawPath);
        var agentiCoreMinimalBuildText = File.ReadAllText(agentiCoreMinimalBuildPath);
        var instantiationLawText = File.ReadAllText(instantiationLawPath);

        Assert.Contains("SANCTUARY_BOOT_FIRST_RUN_ONTOLOGY_BRIDGE.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("wider Sanctuary boot ontology", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("current source", buildReadinessText, StringComparison.Ordinal);
        Assert.DoesNotContain("source owners now live", buildReadinessText, StringComparison.Ordinal);

        var bridgeMarkers = new[]
        {
            "`Install State -> Sanctuary State -> onboarding/local law -> first CradleTek formation -> seating -> Ready Pre-Certification -> certification -> first legal run -> later OE/CME actualization`",
            "`FIRST_RUN_CONSTITUTION.md` is a subordinate `V1.1.1` constitutional",
            "Current repo-local posture:",
            "projected now",
            "withheld",
            "upstream doctrine only",
            "first-run constitutional projection",
            "chapter-five through chapter-nine framing notes",
            "current readiness markers",
            "current runtime and telemetry evidence"
        };

        foreach (var marker in bridgeMarkers)
        {
            Assert.Contains(marker, bridgeText, StringComparison.Ordinal);
        }

        var firstRunMarkers = new[]
        {
            "It is also not the whole Sanctuary lifecycle.",
            "Install, Sanctuary boot, onboarding, certification, and first legal run are",
            "## Current V1.1.1 Constitutional Projection Order",
            "The v2 constitutional projection order carried by `V1.1.1` is:",
            "These readiness states are line-local constitutional readiness markers.",
            "`OpalActualized` is not equivalent to the wider research meanings of",
            "a future",
            "cognition-facing `Listening Frame` placeholder"
        };

        foreach (var marker in firstRunMarkers)
        {
            Assert.Contains(marker, firstRunText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("The v2 constitutional state order is:", firstRunText, StringComparison.Ordinal);

        var cradleCanonicalChain = "`CradleTek -> SLI -> SoulFrame -> Listening Frame -> Compass -> AgentiCore coordination -> cOE/cSelfGEL issued office`";
        var sanctuaryCanonicalChain = "`Sanctuary -> Steward-issued cradle braid -> CradleTek -> SLI -> SoulFrame -> Listening Frame -> Compass -> AgentiCore coordination -> cOE/cSelfGEL issued office`";

        Assert.Contains(cradleCanonicalChain, workbenchLawText, StringComparison.Ordinal);
        Assert.Contains("the item is projected into `Listening Frame`", workbenchLawText, StringComparison.Ordinal);
        Assert.Contains("bounded `AgentiCore` coordination proposes the next lawful action", workbenchLawText, StringComparison.Ordinal);
        Assert.Contains(sanctuaryCanonicalChain, agentiCoreMinimalBuildText, StringComparison.Ordinal);
        Assert.Contains(cradleCanonicalChain, instantiationLawText, StringComparison.Ordinal);
        Assert.Contains("coordination is not issued office", agentiCoreMinimalBuildText, StringComparison.Ordinal);
        Assert.Contains("`cOE/cSelfGEL` seal issues operative office after that coordination", instantiationLawText, StringComparison.Ordinal);
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
        Assert.DoesNotContain("nextWatchdogRunUtc", installWatchdogText, StringComparison.Ordinal);
        Assert.Contains("Get-NextHourlyAnchorUtc -Minute 0", installWatchdogText, StringComparison.Ordinal);

        Assert.Contains("Invoke-Local-Automation-HitlDigest.ps1", installDigestText, StringComparison.Ordinal);
        Assert.Contains("-RepetitionInterval", installDigestText, StringComparison.Ordinal);

        Assert.Contains("Sync-Local-AutomationScheduler.ps1", watchdogScriptText, StringComparison.Ordinal);
        Assert.Contains("watchdogState", watchdogScriptText, StringComparison.Ordinal);

        Assert.Contains("Write-Release-Candidate-Digest.ps1", digestScriptText, StringComparison.Ordinal);
        Assert.Contains("nextDailyHitlDigestRunUtc", digestScriptText, StringComparison.Ordinal);

        Assert.Contains("mainWorkerTaskName", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("watchdogTaskName", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("dailyDigestTaskName", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("$desiredWatchdogRunUtc = Get-NextHourlyAnchorUtc -Minute 0", schedulerSyncText, StringComparison.Ordinal);
        Assert.Contains("nextWatchdogRunUtc = $nextWatchdogRunUtc.ToString('o')", File.ReadAllText(Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-Cycle.ps1")), StringComparison.Ordinal);

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

    private static void AssertV121GroupoidCounts(string groupoidAuditText)
    {
        Assert.Contains("Audited active docs: `93`", groupoidAuditText, StringComparison.Ordinal);
        Assert.Contains("- `Sanctuary`: `91`", groupoidAuditText, StringComparison.Ordinal);
    }
}
