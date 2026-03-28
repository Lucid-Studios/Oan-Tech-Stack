namespace Oan.Runtime.IntegrationTests;

public sealed class LocalLlmPreflightLiveHarnessTests
{
    [Fact]
    public async Task Runner_EmitsRepoLocalBaselineArtifactsAgainstConfiguredHost()
    {
        if (!LocalLlmPreflightHarness.ShouldRunLiveHarness())
        {
            return;
        }

        var options = LocalLlmPreflightRunOptions.FromEnvironment();
        var summary = await LocalLlmPreflightHarness.RunAsync(options);
        var ledgerPath = Path.Combine(options.OutputRoot, "scenario-ledger.jsonl");
        var telemetryPath = Path.Combine(options.OutputRoot, "telemetry-records.jsonl");

        Assert.Equal(LocalLlmPreflightConstants.SuiteVersion, summary.SuiteVersion);
        Assert.Equal(10, summary.ScenarioCount);
        Assert.True(File.Exists(Path.Combine(options.OutputRoot, "run-summary.json")));
        Assert.True(File.Exists(ledgerPath));
        Assert.True(File.Exists(telemetryPath));
        Assert.True(File.Exists(Path.Combine(options.OutputRoot, "summary.md")));
        Assert.Equal(10, File.ReadLines(ledgerPath).Count());
        Assert.NotEmpty(File.ReadLines(telemetryPath));
    }
}
