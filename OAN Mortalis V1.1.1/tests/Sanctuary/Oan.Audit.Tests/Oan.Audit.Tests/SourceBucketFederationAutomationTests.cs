using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class SourceBucketFederationAutomationTests
{
    [Fact]
    public void Source_Bucket_Federation_Remains_Bounded_Without_Local_Automation()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");

        var federationDocPath = Path.Combine(lineRoot, "docs", "SOURCE_BUCKET_FEDERATION_LANE.md");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var federationPolicyPath = Path.Combine(lineRoot, "Automation", "source-bucket-federation.json");
        var requestContractPath = Path.Combine(lineRoot, "Automation", "source-bucket-work-request-contract.json");
        var returnContractPath = Path.Combine(lineRoot, "Automation", "source-bucket-return-contract.json");
        var newRequestScriptPath = Path.Combine(repoRoot, "tools", "New-SourceBucket-WorkRequest.ps1");
        var federationStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-SourceBucket-FederationStatus.ps1");
        var federationCycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-SourceBucket-FederationCycle.ps1");
        var requestIndexStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-request-index.json");
        var federationStatusStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-federation-status.json");

        var federationDocText = File.ReadAllText(federationDocPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var federationPolicyText = File.ReadAllText(federationPolicyPath);
        var newRequestScriptText = File.ReadAllText(newRequestScriptPath);
        var federationStatusScriptText = File.ReadAllText(federationStatusScriptPath);
        var federationCycleScriptText = File.ReadAllText(federationCycleScriptPath);

        Assert.Contains("`source-bucket-federation-control-plane: admitted-local-bounded`", federationDocText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-federation-cycle: admitted-local-mechanical`", federationDocText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-federation-cycle: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);
        Assert.DoesNotContain("Invoke-Local-Automation-Cycle.ps1", federationDocText, StringComparison.Ordinal);
        Assert.DoesNotContain("local-automation-cycle.json", federationPolicyText, StringComparison.Ordinal);
        Assert.DoesNotContain("local-automation-tasking-status", federationStatusScriptText, StringComparison.Ordinal);

        Assert.Contains("source-bucket-request://", newRequestScriptText, StringComparison.Ordinal);
        Assert.Contains("published-awaiting-return", newRequestScriptText, StringComparison.Ordinal);
        Assert.Contains("requests-published-awaiting-returns", federationStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("research-handoff", federationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("versionedTouchPointMatrixPath", federationCycleScriptText, StringComparison.Ordinal);
        Assert.DoesNotContain("CyclePolicyPath", federationCycleScriptText, StringComparison.Ordinal);

        using var federationPolicy = JsonDocument.Parse(federationPolicyText);
        using var requestContract = JsonDocument.Parse(File.ReadAllText(requestContractPath));
        using var returnContract = JsonDocument.Parse(File.ReadAllText(returnContractPath));
        using var requestIndex = JsonDocument.Parse(File.ReadAllText(requestIndexStatePath));
        using var federationStatus = JsonDocument.Parse(File.ReadAllText(federationStatusStatePath));

        Assert.Equal("OAN Mortalis V1.1.1/.audit/runs/source-bucket-work-requests", federationPolicy.RootElement.GetProperty("requestOutboxRoot").GetString());
        Assert.True(federationPolicy.RootElement.TryGetProperty("versionedTouchPointMatrixPath", out _), "Expected versionedTouchPointMatrixPath on federation policy.");
        Assert.Contains("doping_header", requestContract.RootElement.GetProperty("requiredReceipts").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("withheld_or_escalated", returnContract.RootElement.GetProperty("listenerStates").EnumerateArray().Select(x => x.GetString()));

        var requests = requestIndex.RootElement.GetProperty("requests").EnumerateArray().ToList();
        Assert.NotEmpty(requests);

        var activeRequests = requests
            .Where(request => string.Equals(request.GetProperty("requestState").GetString(), "published-awaiting-return", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(activeRequests);
        Assert.Equal("requests-published-awaiting-returns", federationStatus.RootElement.GetProperty("federationState").GetString());
        Assert.True(federationStatus.RootElement.TryGetProperty("rootDispatchPrompt", out _), "Expected rootDispatchPrompt on federation status.");

        var bucketStates = federationStatus.RootElement.GetProperty("bucketStates").EnumerateArray().ToList();
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
