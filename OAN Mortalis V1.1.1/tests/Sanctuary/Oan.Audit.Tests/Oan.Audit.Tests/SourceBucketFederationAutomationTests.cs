using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class SourceBucketFederationAutomationTests
{
    [Fact]
    public void Source_Bucket_Federation_Automation_Is_Wired_And_Bounded()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");

        var federationDocPath = Path.Combine(lineRoot, "docs", "SOURCE_BUCKET_FEDERATION_LANE.md");
        var buildReadinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var enrichmentPathwayDocPath = Path.Combine(lineRoot, "docs", "V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md");
        var cyclePolicyPath = Path.Combine(lineRoot, "build", "local-automation-cycle.json");
        var federationPolicyPath = Path.Combine(lineRoot, "build", "source-bucket-federation.json");
        var requestContractPath = Path.Combine(lineRoot, "build", "source-bucket-work-request-contract.json");
        var returnContractPath = Path.Combine(lineRoot, "build", "source-bucket-return-contract.json");
        var localAutomationCycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-Local-Automation-Cycle.ps1");
        var newRequestScriptPath = Path.Combine(repoRoot, "tools", "New-SourceBucket-WorkRequest.ps1");
        var federationStatusScriptPath = Path.Combine(repoRoot, "tools", "Write-SourceBucket-FederationStatus.ps1");
        var federationCycleScriptPath = Path.Combine(repoRoot, "tools", "Invoke-SourceBucket-FederationCycle.ps1");
        var requestIndexStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-request-index.json");
        var federationStatusStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-federation-status.json");

        var federationDocText = File.ReadAllText(federationDocPath);
        var buildReadinessText = File.ReadAllText(buildReadinessPath);
        var enrichmentPathwayDocText = File.ReadAllText(enrichmentPathwayDocPath);
        var localAutomationCycleScriptText = File.ReadAllText(localAutomationCycleScriptPath);
        var newRequestScriptText = File.ReadAllText(newRequestScriptPath);
        var federationStatusScriptText = File.ReadAllText(federationStatusScriptPath);
        var federationCycleScriptText = File.ReadAllText(federationCycleScriptPath);

        Assert.Contains("`source-bucket-federation-control-plane: admitted-local-bounded`", federationDocText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-federation-cycle: admitted-local-mechanical`", federationDocText, StringComparison.Ordinal);
        Assert.Contains("active research-handoff cluster present now", federationDocText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-federation-cycle: admitted-local-mechanical`", buildReadinessText, StringComparison.Ordinal);
        Assert.Contains("`source-bucket-federation-cycle: admitted-local-mechanical`", enrichmentPathwayDocText, StringComparison.Ordinal);
        Assert.Contains("tools/New-SourceBucket-WorkRequest.ps1", enrichmentPathwayDocText, StringComparison.Ordinal);
        Assert.Contains(".audit/state/source-bucket-request-index.json", enrichmentPathwayDocText, StringComparison.Ordinal);

        Assert.Contains("sourceBucketFederationPolicyPath", File.ReadAllText(cyclePolicyPath), StringComparison.Ordinal);
        Assert.Contains("sourceBucketRequestIndexStatePath", File.ReadAllText(cyclePolicyPath), StringComparison.Ordinal);
        Assert.Contains("sourceBucketFederationStatusStatePath", File.ReadAllText(cyclePolicyPath), StringComparison.Ordinal);

        Assert.Contains("source-bucket-request://", newRequestScriptText, StringComparison.Ordinal);
        Assert.Contains("published-awaiting-return", newRequestScriptText, StringComparison.Ordinal);
        Assert.Contains("requests-published-awaiting-returns", federationStatusScriptText, StringComparison.Ordinal);
        Assert.Contains("research-handoff", federationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("IUTT SLI & Lisp", federationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("Trivium Forum", federationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("Invoke-SourceBucket-FederationCycle.ps1", localAutomationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("sourceBucketFederationStatusStatePath", localAutomationCycleScriptText, StringComparison.Ordinal);
        Assert.Contains("sourceBucketRequestIndexStatePath", localAutomationCycleScriptText, StringComparison.Ordinal);

        using var federationPolicy = JsonDocument.Parse(File.ReadAllText(federationPolicyPath));
        using var requestContract = JsonDocument.Parse(File.ReadAllText(requestContractPath));
        using var returnContract = JsonDocument.Parse(File.ReadAllText(returnContractPath));
        using var requestIndex = JsonDocument.Parse(File.ReadAllText(requestIndexStatePath));
        using var federationStatus = JsonDocument.Parse(File.ReadAllText(federationStatusStatePath));

        Assert.Equal("OAN Mortalis V1.1.1/.audit/runs/source-bucket-work-requests", federationPolicy.RootElement.GetProperty("requestOutboxRoot").GetString());
        Assert.Contains("doping_header", requestContract.RootElement.GetProperty("requiredReceipts").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("withheld_or_escalated", returnContract.RootElement.GetProperty("listenerStates").EnumerateArray().Select(x => x.GetString()));

        var requests = requestIndex.RootElement.GetProperty("requests").EnumerateArray().ToList();
        Assert.NotEmpty(requests);

        var activeRequests = requests
            .Where(request => string.Equals(request.GetProperty("requestState").GetString(), "published-awaiting-return", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(activeRequests);

        var activeBucketLabels = activeRequests
            .Select(request => request.GetProperty("targetBucketLabel").GetString())
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.Contains("IUTT SLI & Lisp", activeBucketLabels);
        Assert.Contains("Trivium Forum", activeBucketLabels);
        Assert.DoesNotContain("Latex Styles", activeBucketLabels);
        Assert.DoesNotContain("Holographic Data Tool", activeBucketLabels);

        foreach (var activeRequest in activeRequests)
        {
            var bundlePath = activeRequest.GetProperty("bundlePath").GetString() ?? throw new InvalidOperationException("Expected bundlePath on active request.");
            var resolvedBundlePath = Path.GetFullPath(Path.Combine(repoRoot, bundlePath.Replace('/', Path.DirectorySeparatorChar)));
            Assert.True(Directory.Exists(resolvedBundlePath), $"Expected request bundle directory to exist: {resolvedBundlePath}");
            Assert.True(File.Exists(Path.Combine(resolvedBundlePath, "request.json")), $"Expected request.json in {resolvedBundlePath}");
            Assert.True(File.Exists(Path.Combine(resolvedBundlePath, "request.md")), $"Expected request.md in {resolvedBundlePath}");
        }

        Assert.Equal("requests-published-awaiting-returns", federationStatus.RootElement.GetProperty("federationState").GetString());
        Assert.True(federationStatus.RootElement.TryGetProperty("rootDispatchPrompt", out _), "Expected rootDispatchPrompt on federation status.");
        Assert.True(federationStatus.RootElement.TryGetProperty("hitlVerificationAid", out _), "Expected hitlVerificationAid on federation status.");

        var bucketStates = federationStatus.RootElement.GetProperty("bucketStates").EnumerateArray().ToList();
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "IUTT SLI & Lisp", StringComparison.Ordinal) && string.Equals(bucket.GetProperty("bucketState").GetString(), "request-published-awaiting-return", StringComparison.Ordinal));
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "Trivium Forum", StringComparison.Ordinal) && string.Equals(bucket.GetProperty("bucketState").GetString(), "request-published-awaiting-return", StringComparison.Ordinal));
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "Latex Styles", StringComparison.Ordinal) && bucket.GetProperty("activeRequestCount").GetInt32() == 0);
        Assert.Contains(bucketStates, bucket => string.Equals(bucket.GetProperty("targetBucketLabel").GetString(), "Holographic Data Tool", StringComparison.Ordinal) && bucket.GetProperty("activeRequestCount").GetInt32() == 0);
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
