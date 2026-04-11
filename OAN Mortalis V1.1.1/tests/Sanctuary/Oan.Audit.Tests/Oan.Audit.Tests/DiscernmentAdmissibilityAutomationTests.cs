using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class DiscernmentAdmissibilityAutomationTests
{
    [Fact]
    public void Runtime_Workbench_And_SourceBucket_Intake_Emit_Discernment_Semantics()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");

        var discernmentHelperPath = Path.Combine(repoRoot, "tools", "Discernment-Admission.ps1");
        var workbenchWriterPath = Path.Combine(repoRoot, "tools", "Write-Sanctuary-RuntimeWorkbenchSurface.ps1");
        var dayDreamWriterPath = Path.Combine(repoRoot, "tools", "Write-Amenable-DayDreamTierAdmissibility.ps1");
        var depthGateWriterPath = Path.Combine(repoRoot, "tools", "Write-SelfRooted-CrypticDepthGate.ps1");
        var sessionLedgerWriterPath = Path.Combine(repoRoot, "tools", "Write-RuntimeWorkbench-SessionLedger.ps1");
        var enrichmentWriterPath = Path.Combine(repoRoot, "tools", "Write-V111-EnrichmentPathway.ps1");
        var sourceBucketReturnWriterPath = Path.Combine(repoRoot, "tools", "Write-SourceBucket-ReturnIntegrationStatus.ps1");
        var sourceBucketReportConsumptionPath = Path.Combine(repoRoot, "tools", "Invoke-SourceBucket-ReportConsumption.ps1");
        var workbenchStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-sanctuary-runtime-workbench-surface-last-run.json");
        var dayDreamStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-amenable-day-dream-tier-admissibility-last-run.json");
        var depthGateStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-self-rooted-cryptic-depth-gate-last-run.json");
        var sessionLedgerStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-runtime-workbench-session-ledger-last-run.json");
        var enrichmentStatePath = Path.Combine(repoRoot, ".audit", "state", "local-automation-v111-enrichment-pathway-last-run.json");
        var sourceBucketReturnStatePath = Path.Combine(lineRoot, ".audit", "state", "source-bucket-return-integration-status.json");

        var discernmentHelperText = File.ReadAllText(discernmentHelperPath);
        var workbenchWriterText = File.ReadAllText(workbenchWriterPath);
        var dayDreamWriterText = File.ReadAllText(dayDreamWriterPath);
        var depthGateWriterText = File.ReadAllText(depthGateWriterPath);
        var sessionLedgerWriterText = File.ReadAllText(sessionLedgerWriterPath);
        var enrichmentWriterText = File.ReadAllText(enrichmentWriterPath);
        var sourceBucketReturnWriterText = File.ReadAllText(sourceBucketReturnWriterPath);
        var sourceBucketReportConsumptionText = File.ReadAllText(sourceBucketReportConsumptionPath);

        Assert.Contains("Get-DiscernmentAdmissionEnvelope", discernmentHelperText, StringComparison.Ordinal);

        Assert.Contains("standingSurfaceClass", workbenchWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentAction", workbenchWriterText, StringComparison.Ordinal);
        Assert.Contains("promotionReceiptState", workbenchWriterText, StringComparison.Ordinal);
        Assert.Contains("bounded-workbench-provisional", workbenchWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentEvaluation", workbenchWriterText, StringComparison.Ordinal);
        Assert.Contains("Get-DiscernmentAdmissionEnvelope", dayDreamWriterText, StringComparison.Ordinal);
        Assert.Contains("workbenchDiscernmentAction", dayDreamWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentAction", dayDreamWriterText, StringComparison.Ordinal);
        Assert.Contains("Get-DiscernmentAdmissionEnvelope", depthGateWriterText, StringComparison.Ordinal);
        Assert.Contains("dayDreamDiscernmentAction", depthGateWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentAction", depthGateWriterText, StringComparison.Ordinal);

        Assert.Contains("standingSurfaceClass", sessionLedgerWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentAction", sessionLedgerWriterText, StringComparison.Ordinal);
        Assert.Contains("promotionReceiptState", sessionLedgerWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentEvaluation", sessionLedgerWriterText, StringComparison.Ordinal);
        Assert.Contains("depthGateDiscernmentAction", sessionLedgerWriterText, StringComparison.Ordinal);
        Assert.Contains("Get-DiscernmentAdmissionEnvelope", enrichmentWriterText, StringComparison.Ordinal);
        Assert.Contains("runtimeWorkbenchSessionDiscernmentAction", enrichmentWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentAction", enrichmentWriterText, StringComparison.Ordinal);

        Assert.Contains("requestedStanding", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("standingSurfaceClass", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("categoryErrorDetected", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("promotionWithoutReceiptsDetected", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("refusedReturnCount", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("provisionalReturnCount", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("discernmentEvaluation", sourceBucketReturnWriterText, StringComparison.Ordinal);
        Assert.Contains("Get-DiscernmentAdmissionEnvelope", sourceBucketReportConsumptionText, StringComparison.Ordinal);
        Assert.Contains("returnDiscernmentAction", sourceBucketReportConsumptionText, StringComparison.Ordinal);

        using var workbenchState = JsonDocument.Parse(File.ReadAllText(workbenchStatePath));
        using var dayDreamState = JsonDocument.Parse(File.ReadAllText(dayDreamStatePath));
        using var depthGateState = JsonDocument.Parse(File.ReadAllText(depthGateStatePath));
        using var sessionLedgerState = JsonDocument.Parse(File.ReadAllText(sessionLedgerStatePath));
        using var enrichmentState = JsonDocument.Parse(File.ReadAllText(enrichmentStatePath));
        using var sourceBucketReturnState = JsonDocument.Parse(File.ReadAllText(sourceBucketReturnStatePath));

        Assert.True(workbenchState.RootElement.TryGetProperty("discernmentAction", out var workbenchDiscernmentAction), "Expected discernmentAction on runtime workbench state.");
        Assert.True(workbenchState.RootElement.TryGetProperty("standingSurfaceClass", out var workbenchStandingSurfaceClass), "Expected standingSurfaceClass on runtime workbench state.");
        Assert.True(workbenchState.RootElement.TryGetProperty("promotionReceiptState", out _), "Expected promotionReceiptState on runtime workbench state.");
        Assert.True(workbenchState.RootElement.TryGetProperty("discernmentEvaluation", out _), "Expected discernmentEvaluation on runtime workbench state.");

        var workbenchStateValue = workbenchState.RootElement.GetProperty("workbenchState").GetString();
        var sessionPostureValue = workbenchState.RootElement.GetProperty("sessionPosture").GetString();
        if (string.Equals(workbenchStateValue, "sanctuary-runtime-workbench-ready", StringComparison.Ordinal))
        {
            Assert.Equal("admit", workbenchDiscernmentAction.GetString());
            Assert.Equal("closure-bearing", workbenchStandingSurfaceClass.GetString());
            Assert.Equal("bounded-workbench-ready", sessionPostureValue);
        }
        else if (string.Equals(workbenchStateValue, "blocked", StringComparison.Ordinal))
        {
            Assert.Equal("hold", workbenchDiscernmentAction.GetString());
            Assert.Equal("refusal-surface", workbenchStandingSurfaceClass.GetString());
            Assert.Equal("bounded-workbench-hold", sessionPostureValue);
        }
        else
        {
            Assert.Equal("remain-provisional", workbenchDiscernmentAction.GetString());
            Assert.Equal("rhetoric-bearing", workbenchStandingSurfaceClass.GetString());
            Assert.Equal("bounded-workbench-provisional", sessionPostureValue);
        }

        Assert.True(dayDreamState.RootElement.TryGetProperty("workbenchDiscernmentAction", out var dayDreamWorkbenchDiscernmentAction), "Expected workbenchDiscernmentAction on day-dream tier state.");
        Assert.True(dayDreamState.RootElement.TryGetProperty("discernmentAction", out var dayDreamDiscernmentAction), "Expected discernmentAction on day-dream tier state.");
        var dayDreamTierState = dayDreamState.RootElement.GetProperty("tierState").GetString();
        Assert.Equal(workbenchDiscernmentAction.GetString(), dayDreamWorkbenchDiscernmentAction.GetString());
        if (string.Equals(dayDreamTierState, "amenable-day-dream-tier-ready", StringComparison.Ordinal))
        {
            Assert.Equal("admit", dayDreamDiscernmentAction.GetString());
        }
        else if (dayDreamTierState is not null && (dayDreamTierState.Contains("held-by-", StringComparison.Ordinal) || dayDreamTierState.Equals("blocked", StringComparison.Ordinal)))
        {
            Assert.Equal("hold", dayDreamDiscernmentAction.GetString());
        }
        else if (dayDreamTierState is not null && dayDreamTierState.Contains("refused-by-", StringComparison.Ordinal))
        {
            Assert.Equal("refuse", dayDreamDiscernmentAction.GetString());
        }
        else
        {
            Assert.Equal("remain-provisional", dayDreamDiscernmentAction.GetString());
        }

        Assert.True(depthGateState.RootElement.TryGetProperty("dayDreamDiscernmentAction", out var depthGateDayDreamDiscernmentAction), "Expected dayDreamDiscernmentAction on depth-gate state.");
        Assert.True(depthGateState.RootElement.TryGetProperty("discernmentAction", out var depthGateDiscernmentAction), "Expected discernmentAction on depth-gate state.");
        var depthGateStateValue = depthGateState.RootElement.GetProperty("gateState").GetString();
        Assert.Equal(dayDreamDiscernmentAction.GetString(), depthGateDayDreamDiscernmentAction.GetString());
        if (string.Equals(depthGateStateValue, "self-rooted-cryptic-depth-gate-ready", StringComparison.Ordinal))
        {
            Assert.Equal("admit", depthGateDiscernmentAction.GetString());
        }
        else if (depthGateStateValue is not null && (depthGateStateValue.Contains("held-by-", StringComparison.Ordinal) || depthGateStateValue.Equals("blocked", StringComparison.Ordinal)))
        {
            Assert.Equal("hold", depthGateDiscernmentAction.GetString());
        }
        else if (depthGateStateValue is not null && depthGateStateValue.Contains("refused-by-", StringComparison.Ordinal))
        {
            Assert.Equal("refuse", depthGateDiscernmentAction.GetString());
        }
        else
        {
            Assert.Equal("remain-provisional", depthGateDiscernmentAction.GetString());
        }

        Assert.True(sessionLedgerState.RootElement.TryGetProperty("discernmentAction", out var sessionDiscernmentAction), "Expected discernmentAction on session ledger state.");
        Assert.True(sessionLedgerState.RootElement.TryGetProperty("standingSurfaceClass", out var sessionStandingSurfaceClass), "Expected standingSurfaceClass on session ledger state.");
        Assert.True(sessionLedgerState.RootElement.TryGetProperty("promotionReceiptState", out _), "Expected promotionReceiptState on session ledger state.");
        Assert.True(sessionLedgerState.RootElement.TryGetProperty("discernmentEvaluation", out _), "Expected discernmentEvaluation on session ledger state.");
        Assert.True(sessionLedgerState.RootElement.TryGetProperty("depthGateDiscernmentAction", out var sessionDepthGateDiscernmentAction), "Expected depthGateDiscernmentAction on session ledger state.");
        Assert.Equal(depthGateDiscernmentAction.GetString(), sessionDepthGateDiscernmentAction.GetString());

        var sessionStateClass = sessionLedgerState.RootElement.GetProperty("currentStateClass").GetString();
        var sessionLedgerStateValue = sessionLedgerState.RootElement.GetProperty("sessionLedgerState").GetString();
        if (string.Equals(sessionStateClass, "ready", StringComparison.Ordinal))
        {
            Assert.Equal("admit", sessionDiscernmentAction.GetString());
            Assert.Equal("closure-bearing", sessionStandingSurfaceClass.GetString());
        }
        else if (sessionLedgerStateValue is not null && sessionLedgerStateValue.Contains("refused-by-", StringComparison.Ordinal))
        {
            Assert.Equal("refuse", sessionDiscernmentAction.GetString());
            Assert.Equal("refusal-surface", sessionStandingSurfaceClass.GetString());
        }
        else if (string.Equals(sessionStateClass, "hold", StringComparison.Ordinal))
        {
            Assert.Equal("hold", sessionDiscernmentAction.GetString());
            Assert.Equal("refusal-surface", sessionStandingSurfaceClass.GetString());
        }
        else
        {
            Assert.Equal("remain-provisional", sessionDiscernmentAction.GetString());
            Assert.Equal("rhetoric-bearing", sessionStandingSurfaceClass.GetString());
        }

        Assert.True(enrichmentState.RootElement.TryGetProperty("runtimeWorkbenchSessionDiscernmentAction", out var enrichmentSessionDiscernmentAction), "Expected runtimeWorkbenchSessionDiscernmentAction on enrichment pathway state.");
        Assert.True(enrichmentState.RootElement.TryGetProperty("discernmentAction", out var enrichmentDiscernmentAction), "Expected discernmentAction on enrichment pathway state.");
        Assert.Equal(sessionDiscernmentAction.GetString(), enrichmentSessionDiscernmentAction.GetString());
        var enrichmentPathwayState = enrichmentState.RootElement.GetProperty("pathwayState").GetString();
        if (string.Equals(enrichmentPathwayState, "v111-enrichment-path-open", StringComparison.Ordinal))
        {
            Assert.Equal("admit", enrichmentDiscernmentAction.GetString());
        }
        else if (enrichmentPathwayState is not null && (enrichmentPathwayState.Contains("held-", StringComparison.Ordinal) || enrichmentPathwayState.Equals("blocked", StringComparison.Ordinal)))
        {
            Assert.Equal("hold", enrichmentDiscernmentAction.GetString());
        }
        else if (enrichmentPathwayState is not null && enrichmentPathwayState.Contains("refused-", StringComparison.Ordinal))
        {
            Assert.Equal("refuse", enrichmentDiscernmentAction.GetString());
        }
        else
        {
            Assert.Equal("remain-provisional", enrichmentDiscernmentAction.GetString());
        }

        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("requestedStanding", out _), "Expected requestedStanding on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("discernmentAction", out var sourceBucketDiscernmentAction), "Expected discernmentAction on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("standingSurfaceClass", out var sourceBucketStandingSurfaceClass), "Expected standingSurfaceClass on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("promotionReceiptState", out _), "Expected promotionReceiptState on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("categoryErrorCount", out _), "Expected categoryErrorCount on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("promotionWithoutReceiptsCount", out _), "Expected promotionWithoutReceiptsCount on source-bucket return integration state.");
        Assert.True(sourceBucketReturnState.RootElement.TryGetProperty("discernmentEvaluation", out _), "Expected discernmentEvaluation on source-bucket return integration state.");

        var integrationStateValue = sourceBucketReturnState.RootElement.GetProperty("integrationState").GetString();
        if (string.Equals(integrationStateValue, "receipted-returns-ready-for-build-review", StringComparison.Ordinal))
        {
            Assert.Equal("admit", sourceBucketDiscernmentAction.GetString());
            Assert.Equal("closure-bearing", sourceBucketStandingSurfaceClass.GetString());
        }
        else if (string.Equals(integrationStateValue, "invalid-returns-require-review", StringComparison.Ordinal))
        {
            Assert.Equal("refuse", sourceBucketDiscernmentAction.GetString());
            Assert.Equal("refusal-surface", sourceBucketStandingSurfaceClass.GetString());
        }
        else if (string.Equals(integrationStateValue, "returns-held-or-escalated", StringComparison.Ordinal))
        {
            Assert.Equal("hold", sourceBucketDiscernmentAction.GetString());
            Assert.Equal("refusal-surface", sourceBucketStandingSurfaceClass.GetString());
        }
        else
        {
            Assert.Equal("remain-provisional", sourceBucketDiscernmentAction.GetString());
            Assert.Equal("rhetoric-bearing", sourceBucketStandingSurfaceClass.GetString());
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
