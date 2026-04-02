using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class BuildDispatchGateReconciliationTests
{
    [Fact]
    public void BuildDispatch_RootPrompt_And_Gate_Reconciliation_Are_Wired()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");

        var rootPromptPath = Path.Combine(lineRoot, "docs", "OAN_BUILD_DISPATCH_ROOT_PROMPT.md");
        var orchestrationLawPath = Path.Combine(lineRoot, "docs", "MASTER_THREAD_BUCKET_ORCHESTRATION_LAW.md");
        var enrichmentPathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var workflowMilestoneMapPath = Path.Combine(lineRoot, "docs", "V1_1_1_WORKFLOW_MILESTONE_MAP.md");
        var seededGovernanceBuildAdmissionLawPath = Path.Combine(lineRoot, "docs", "SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md");
        var orchestrationPolicyPath = Path.Combine(lineRoot, "build", "master-thread-orchestration.json");
        var masterThreadStatePath = Path.Combine(repoRoot, ".audit", "state", "master-thread-orchestration-status.json");
        var seededGovernanceStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-seeded-governance-last-run.json");
        var runIsolatedPathwayStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-run-isolated-build-pathway-last-run.json");
        var v111EnrichmentStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-v111-enrichment-pathway-last-run.json");

        var rootPromptText = File.ReadAllText(rootPromptPath);
        var orchestrationLawText = File.ReadAllText(orchestrationLawPath);
        var enrichmentPathwayDocText = File.ReadAllText(enrichmentPathwayDocPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var workflowMilestoneMapText = File.ReadAllText(workflowMilestoneMapPath);
        var seededGovernanceBuildAdmissionLawText = File.ReadAllText(seededGovernanceBuildAdmissionLawPath);

        Assert.Contains("You are OAN Build Dispatch", rootPromptText, StringComparison.Ordinal);
        Assert.Contains("classify that as `clarify`", rootPromptText, StringComparison.Ordinal);
        Assert.Contains("OAN_BUILD_DISPATCH_ROOT_PROMPT.md", orchestrationLawText, StringComparison.Ordinal);
        Assert.Contains("gate-truth mismatch", orchestrationLawText, StringComparison.Ordinal);
        Assert.Contains("OAN_BUILD_DISPATCH_ROOT_PROMPT.md", enrichmentPathwayDocText, StringComparison.Ordinal);
        Assert.Contains("V1_1_1_WORKFLOW_MILESTONE_MAP.md", enrichmentPathwayDocText, StringComparison.Ordinal);
        Assert.Contains("SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md", enrichmentPathwayDocText, StringComparison.Ordinal);
        Assert.Contains("V1_1_1_WORKFLOW_MILESTONE_MAP.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("## End-To-End Milestones", workflowMilestoneMapText, StringComparison.Ordinal);
        Assert.Contains("### M4. Seeded-Governance Build Admission Seam", workflowMilestoneMapText, StringComparison.Ordinal);
        Assert.Contains("### M8. Executable Candidate", workflowMilestoneMapText, StringComparison.Ordinal);
        Assert.Contains("### M10. Seed-LLM Wrinkle Test Pause", workflowMilestoneMapText, StringComparison.Ordinal);
        Assert.Contains("`buildAdmissionState = admitted-local-bounded`", seededGovernanceBuildAdmissionLawText, StringComparison.Ordinal);

        using var orchestrationPolicy = JsonDocument.Parse(File.ReadAllText(orchestrationPolicyPath));
        var orchestrationPolicyRoot = orchestrationPolicy.RootElement;
        Assert.Equal("OAN Mortalis V1.1.1/docs/OAN_BUILD_DISPATCH_ROOT_PROMPT.md", orchestrationPolicyRoot.GetProperty("rootDispatchPromptMarkdownPath").GetString());
        Assert.Equal(".audit/state/local-automation-seeded-governance-last-run.json", orchestrationPolicyRoot.GetProperty("seededGovernanceStatePath").GetString());
        Assert.Equal(".audit/state/local-automation-run-isolated-build-pathway-last-run.json", orchestrationPolicyRoot.GetProperty("runIsolatedBuildPathwayStatePath").GetString());
        Assert.Equal(".audit/state/local-automation-v111-enrichment-pathway-last-run.json", orchestrationPolicyRoot.GetProperty("v111EnrichmentPathwayStatePath").GetString());

        using var seededGovernanceState = JsonDocument.Parse(File.ReadAllText(seededGovernanceStatePath));
        using var runIsolatedState = JsonDocument.Parse(File.ReadAllText(runIsolatedPathwayStatePath));
        using var v111EnrichmentState = JsonDocument.Parse(File.ReadAllText(v111EnrichmentStatePath));
        using var masterThreadState = JsonDocument.Parse(File.ReadAllText(masterThreadStatePath));

        Assert.True(masterThreadState.RootElement.TryGetProperty("rootDispatchPrompt", out _), "Expected rootDispatchPrompt on master-thread orchestration state.");
        Assert.True(masterThreadState.RootElement.TryGetProperty("gateReconciliation", out var gateReconciliation), "Expected gateReconciliation on master-thread orchestration state.");

        var seededReadyState = seededGovernanceState.RootElement.GetProperty("readyState").GetString();
        Assert.True(seededGovernanceState.RootElement.TryGetProperty("buildAdmissionState", out var buildAdmissionStateElement), "Expected buildAdmissionState on seeded governance state.");
        Assert.True(seededGovernanceState.RootElement.TryGetProperty("buildAdmissionReason", out _), "Expected buildAdmissionReason on seeded governance state.");

        if (string.Equals(seededReadyState, "ready", StringComparison.Ordinal))
        {
            var buildAdmissionState = buildAdmissionStateElement.GetString();
            Assert.NotEqual("bring-seeded-governance-to-ready-state", runIsolatedState.RootElement.GetProperty("nextAction").GetString());
            Assert.NotEqual("bring-seeded-governance-to-ready-state", v111EnrichmentState.RootElement.GetProperty("nextAction").GetString());
            if (string.Equals(buildAdmissionState, "admitted-local-bounded", StringComparison.Ordinal))
            {
                Assert.NotEqual("clarify-seeded-governance-admission", runIsolatedState.RootElement.GetProperty("pathwayState").GetString());
                Assert.NotEqual("clarify-seeded-governance-admission", v111EnrichmentState.RootElement.GetProperty("pathwayState").GetString());
            }
            Assert.False(gateReconciliation.GetProperty("gateMismatchDetected").GetBoolean());
        }
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
}
