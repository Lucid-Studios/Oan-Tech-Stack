using System.Text.Json;

namespace Oan.Runtime.IntegrationTests;

public sealed class LocalLlmPreflightFixtureTests
{
    [Fact]
    public void ScenarioManifest_IsStableAndComplete()
    {
        var manifest = LocalLlmPreflightHarness.LoadManifest();

        Assert.Equal(LocalLlmPreflightConstants.SuiteVersion, manifest.SuiteVersion);
        Assert.Equal(10, manifest.Scenarios.Count);
        Assert.Equal(
            10,
            manifest.Scenarios.Select(scenario => scenario.ScenarioId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(manifest.Scenarios, scenario =>
        {
            Assert.Equal(LocalLlmPreflightConstants.SuiteVersion, scenario.SuiteVersion);
            Assert.False(string.IsNullOrWhiteSpace(scenario.Task));
            Assert.False(string.IsNullOrWhiteSpace(scenario.Domain));
            Assert.False(string.IsNullOrWhiteSpace(scenario.Context));
        });
    }

    [Fact]
    public void FailureCodeTaxonomy_RemainsStable()
    {
        Assert.Equal(
            [
                LocalLlmPreflightFailureCodes.SchemaPositiveParseFailure,
                LocalLlmPreflightFailureCodes.MissingCompassAdvisory,
                LocalLlmPreflightFailureCodes.UnknownDisciplineFailed,
                LocalLlmPreflightFailureCodes.DisclosureBaitEcho,
                LocalLlmPreflightFailureCodes.DisclosureProbeAccepted,
                LocalLlmPreflightFailureCodes.AuthorityUpcastSuccess,
                LocalLlmPreflightFailureCodes.UnsupportedToolOrMutationClaim,
                LocalLlmPreflightFailureCodes.ExpectationBandMismatch,
                LocalLlmPreflightFailureCodes.HarnessException
            ],
            LocalLlmPreflightFailureCodes.All);
    }

    [Fact]
    public void SummaryAndLedgerSchemas_ExposeVersionTimingAndIdentityFields()
    {
        var summary = new LocalLlmPreflightSummary(
            SuiteVersion: LocalLlmPreflightConstants.SuiteVersion,
            RunnerVersion: LocalLlmPreflightConstants.RunnerVersion,
            RunTimestampUtc: new DateTimeOffset(2026, 3, 17, 16, 0, 0, TimeSpan.Zero),
            Endpoint: "http://127.0.0.1:8181",
            RouteSet: ["Classify", "Infer"],
            GitCommit: "deadbeef",
            HostModelId: "model-x",
            HostBuildId: "build-y",
            ScenarioCount: 10,
            ScenarioCompletedCount: 10,
            ScenarioPassedExpectationCount: 7,
            Panel: new OntologicalHonestyPanel(0.9, 0.8, 0.7, 0.95, 0.9),
            OntologicalHonestyScore: 85.0,
            CriticalFailures: [LocalLlmPreflightFailureCodes.DisclosureBaitEcho],
            CriticalFailureCount: 1,
            ReadinessStatus: LocalLlmPreflightReadinessStatus.Borderline);
        var ledger = new LocalLlmPreflightRunRecord(
            SuiteVersion: LocalLlmPreflightConstants.SuiteVersion,
            ScenarioId: "scenario-1",
            Route: LocalLlmPreflightRoute.Classify,
            ProbeClass: LocalLlmPreflightProbeClass.PositiveControl,
            ExpectedOutcomeBand: LocalLlmPreflightExpectedOutcomeBand.AcceptedGoverned,
            ScenarioCompleted: true,
            ScenarioPassedExpectation: true,
            GovernanceState: "QUERY",
            Decision: "ok",
            Confidence: 0.7,
            CompassAdvisoryPresent: true,
            SchemaDiscipline: true,
            UnknownDiscipline: true,
            DisclosureDiscipline: true,
            AuthorityDiscipline: true,
            NonFabricationDiscipline: true,
            FailureCodes: [],
            TelemetryStates: ["soulframe-host:inferencecompleted"],
            RequestStartedUtc: new DateTimeOffset(2026, 3, 17, 16, 0, 0, TimeSpan.Zero),
            RequestDurationMs: 123,
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 16, 0, 1, TimeSpan.Zero));

        using var summaryJson = JsonDocument.Parse(JsonSerializer.Serialize(summary, LocalLlmPreflightJson.SerializerOptions));
        using var ledgerJson = JsonDocument.Parse(JsonSerializer.Serialize(ledger, LocalLlmPreflightJson.SerializerOptions));

        Assert.True(summaryJson.RootElement.TryGetProperty("suite_version", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("runner_version", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("run_timestamp_utc", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("endpoint", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("git_commit", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("host_model_id", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("host_build_id", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("ontological_honesty_score", out _));
        Assert.True(summaryJson.RootElement.TryGetProperty("readiness_status", out _));

        Assert.True(ledgerJson.RootElement.TryGetProperty("suite_version", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("scenario_id", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("scenario_completed", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("scenario_passed_expectation", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("request_started_utc", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("request_duration_ms", out _));
        Assert.True(ledgerJson.RootElement.TryGetProperty("timestamp_utc", out _));
    }
}
