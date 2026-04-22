namespace San.Audit.Tests;

public sealed class SessionBodyStabilizationBaselineDocsTests
{
    [Fact]
    public void Docs_Record_Session_Body_Stabilization_Baseline()
    {
        var lineRoot = GetLineRoot();
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(baselinePath));

        var baselineText = File.ReadAllText(baselinePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("`Slice 1` is the core executable and doctrinal session body.", baselineText, StringComparison.Ordinal);
        Assert.Contains("`Slice 2` is the shared readiness and governance support required", baselineText, StringComparison.Ordinal);
        Assert.Contains("Ambient Drift Left Alone", baselineText, StringComparison.Ordinal);
        Assert.Contains("`AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`CME_TRUTH_SEEKING_BALANCE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`CME_TRUTH_SEEKING_ORIENTATION_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`CONSTRUCTOR_ENGRAM_BURDEN_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`DOMAIN_AND_ROLE_ADMISSION_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`GROUPOID_FIBRINOID_COLLECTION_AND_BUNDLE_MAPPING_LAW.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("`docs/SESSION_CLEANUP_AND_BRAIDING_EVENT_MATRIX.md`", baselineText, StringComparison.Ordinal);
        Assert.Contains("resonance chamber framing", baselineText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary biad truth-holding with Steward-issued cradle braid", baselineText, StringComparison.Ordinal);

        Assert.Contains("SESSION_BODY_STABILIZATION_BASELINE.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("session-body-stabilization-baseline: admitted-local-bounded", readinessText, StringComparison.Ordinal);
        Assert.Contains("Session-body stabilization baseline preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("Sanctuary biad truth-holding, Steward-issued cradle braid, and single", refinementText, StringComparison.Ordinal);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
