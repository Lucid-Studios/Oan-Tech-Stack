using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class SourceBucketReturnAutomationTests
{
    [Fact]
    public void Source_Bucket_Return_Intake_Remains_Bounded_Without_Local_Automation()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");

        var federationPolicyPath = Path.Combine(lineRoot, "Automation", "source-bucket-federation.json");
        var federationDocPath = Path.Combine(lineRoot, "docs", "SOURCE_BUCKET_FEDERATION_LANE.md");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var dispatchPromptPath = Path.Combine(lineRoot, "docs", "OAN_BUILD_DISPATCH_ROOT_PROMPT.md");
        var returnContractPath = Path.Combine(lineRoot, "Automation", "source-bucket-return-contract.json");
        var returnCycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-SourceBucket-ReturnCycle.ps1");
        var returnStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-SourceBucket-ReturnIntegrationStatus.ps1");
        var returnIndexStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-return-index.json");
        var returnIntegrationStatusStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-return-integration-status.json");

        var federationDocText = File.ReadAllText(federationDocPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var dispatchPromptText = File.ReadAllText(dispatchPromptPath);
        var returnStatusScriptText = File.ReadAllText(returnStatusScriptPath);
        var returnCycleScriptText = File.ReadAllText(returnCycleScriptPath);

        Assert.Contains("`source-bucket-return-intake: admitted-local-mechanical`", federationDocText, StringComparison.Ordinal);
        Assert.Contains("source-bucket-returns", federationDocText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-return-intake: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("source-bucket-return-index", dispatchPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("Invoke-Local-Automation-Cycle.ps1", returnCycleScriptText, StringComparison.Ordinal);
        Assert.DoesNotContain("local-automation-cycle.json", returnStatusScriptText, StringComparison.Ordinal);

        Assert.Contains("awaiting-source-bucket-returns", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("receipted-returns-ready-for-build-review", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("source-bucket-return-intake", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("return-class-mismatch-with-request", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("admit-return-not-completed", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("review-invalid-source-bucket-return", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("Get-RequestLifecycleStateFromReturn", returnStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("Write-SourceBucket-ReturnIntegrationStatus.ps1", returnCycleScriptText, StringComparison.Ordinal);
        Assert.DoesNotContain("CyclePolicyPath", returnCycleScriptText, StringComparison.Ordinal);

        using var federationPolicy = JsonDocument.Parse(File.ReadAllText(federationPolicyPath));
        using var returnContract = JsonDocument.Parse(File.ReadAllText(returnContractPath));
        using var returnIndex = JsonDocument.Parse(File.ReadAllText(returnIndexStatePath));
        using var returnIntegrationStatus = JsonDocument.Parse(File.ReadAllText(returnIntegrationStatusStatePath));

        Assert.Equal("OAN Mortalis V1.1.1/.audit/runs/source-bucket-returns", federationPolicy.RootElement.GetProperty("returnInboxRoot").GetString());
        Assert.Equal("OAN Mortalis V1.1.1/.audit/state/source-bucket-return-index.json", federationPolicy.RootElement.GetProperty("returnIndexStatePath").GetString());
        Assert.Equal("OAN Mortalis V1.1.1/.audit/state/source-bucket-return-integration-status.json", federationPolicy.RootElement.GetProperty("returnIntegrationStatusStatePath").GetString());
        Assert.Contains("implement-now", returnContract.RootElement.GetProperty("returnClasses").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("withheld_or_escalated", returnContract.RootElement.GetProperty("listenerStates").EnumerateArray().Select(x => x.GetString()));

        Assert.Equal(0, returnIndex.RootElement.GetProperty("admittedReturnCount").GetInt32());
        Assert.True(returnIndex.RootElement.TryGetProperty("rootDispatchPrompt", out _), "Expected rootDispatchPrompt on return index.");
        Assert.True(returnIntegrationStatus.RootElement.TryGetProperty("rootDispatchPrompt", out _), "Expected rootDispatchPrompt on return integration status.");

        var bucketStates = returnIntegrationStatus.RootElement.GetProperty("bucketStates").EnumerateArray().ToList();
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "IUTT SLI & Lisp", StringComparison.Ordinal));
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "Trivium Forum", StringComparison.Ordinal));
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
