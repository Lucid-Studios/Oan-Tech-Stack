namespace Oan.Audit.Tests;

public sealed class AutomationHitlVerificationAidTests
{
    [Fact]
    public void Automation_Hitl_Verification_Aid_Is_Wired_Across_Bucket_Chain()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
        var hitlAidDocPath = Path.Combine(lineRoot, "docs", "AUTOMATION_HITL_VERIFICATION_AID.md");
        var unlockReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_HOLD_UNLOCK_READINESS.md");
        var pathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var helperScriptPath = Path.Combine(repoRoot, "tools", "Automation-CascadePrompt.ps1");
        var taskStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-Local-Automation-TaskStatus.ps1");
        var bucketStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-Workspace-BucketStatus.ps1");
        var taskMapRunScriptPath = Path.Combine(repoRoot, "tools", "Start-Local-Automation-TaskMapRun.ps1");
        var orchestrationStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-MasterThread-OrchestrationStatus.ps1");
        var orchestrationInstructionScriptPath = Path.Combine(repoRoot, "tools", "New-MasterThread-OrchestrationInstruction.ps1");
        var taskStatusStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-tasking-status.json");
        var bucketStatusStatePath = Path.Combine(repoRoot, ".audit", "state", "workspace-bucket-status.json");
        var taskMapRunStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-active-task-map-run.json");
        var orchestrationStatusStatePath = Path.Combine(repoRoot, ".audit", "state", "master-thread-orchestration-status.json");

        var hitlAidDocText = File.ReadAllText(hitlAidDocPath);
        var unlockReadinessText = File.ReadAllText(unlockReadinessPath);
        var pathwayDocText = File.ReadAllText(pathwayDocPath);
        var helperScriptText = File.ReadAllText(helperScriptPath);
        var taskStatusScriptText = File.ReadAllText(taskStatusScriptPath);
        var bucketStatusScriptText = File.ReadAllText(bucketStatusScriptPath);
        var taskMapRunScriptText = File.ReadAllText(taskMapRunScriptPath);
        var orchestrationStatusScriptText = File.ReadAllText(orchestrationStatusScriptPath);
        var orchestrationInstructionScriptText = File.ReadAllText(orchestrationInstructionScriptPath);
        var taskStatusStateText = File.ReadAllText(taskStatusStatePath);
        var bucketStatusStateText = File.ReadAllText(bucketStatusStatePath);
        var taskMapRunStateText = File.ReadAllText(taskMapRunStatePath);
        var orchestrationStatusStateText = File.ReadAllText(orchestrationStatusStatePath);

        Assert.Contains("`automation-hitl-verification-aid: admitted-operator-aid-bounded`", unlockReadinessText, StringComparison.Ordinal);
        Assert.Contains("`automation-hitl-verification-aid: admitted-operator-aid-bounded`", pathwayDocText, StringComparison.Ordinal);
        Assert.Contains("`AUTOMATION_HITL_VERIFICATION_AID.md`", pathwayDocText, StringComparison.Ordinal);

        var hitlAidDocMarkers = new[]
        {
            "`automation-hitl-verification-aid: admitted-operator-aid-bounded`",
            "`received`",
            "`understood`",
            "`admissible`",
            "`actionable`",
            "`withheld_or_escalated`",
            "Only the `Operator` may confirm direct `HITL` admission."
        };

        foreach (var marker in hitlAidDocMarkers)
        {
            Assert.Contains(marker, hitlAidDocText, StringComparison.Ordinal);
        }

        var helperMarkers = new[]
        {
            "hitlVerificationAid",
            "operator-review-and-confirmation",
            "aidState = if ($requiredNow) { 'required-now' } else { 'available-if-needed' }",
            "withheld_or_escalated",
            "Only the operator may confirm direct HITL admission; automation may prepare but may not self-ratify."
        };

        foreach (var marker in helperMarkers)
        {
            Assert.Contains(marker, helperScriptText, StringComparison.Ordinal);
        }

        var bucketScriptMarkers = new[]
        {
            "Automation-CascadePrompt.ps1",
            "Add-AutomationCascadeOperatorPromptProperty",
            "Add-AutomationCascadePromptMarkdownLines"
        };

        foreach (var marker in bucketScriptMarkers)
        {
            Assert.Contains(marker, bucketStatusScriptText, StringComparison.Ordinal);
        }

        var sharedChainScripts = new[]
        {
            taskStatusScriptText,
            taskMapRunScriptText,
            orchestrationStatusScriptText,
            orchestrationInstructionScriptText
        };

        foreach (var scriptText in sharedChainScripts)
        {
            Assert.Contains("Add-AutomationCascadeOperatorPromptProperty", scriptText, StringComparison.Ordinal);
        }

        var stateMarkers = new[]
        {
            taskStatusStateText,
            bucketStatusStateText,
            taskMapRunStateText,
            orchestrationStatusStateText
        };

        foreach (var stateText in stateMarkers)
        {
            Assert.Contains("\"hitlVerificationAid\"", stateText, StringComparison.Ordinal);
            Assert.Contains("\"received\"", stateText, StringComparison.Ordinal);
            Assert.Contains("\"withheld_or_escalated\"", stateText, StringComparison.Ordinal);
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
