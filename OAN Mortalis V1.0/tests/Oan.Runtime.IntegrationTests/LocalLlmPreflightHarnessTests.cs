namespace Oan.Runtime.IntegrationTests;

public sealed class LocalLlmPreflightHarnessTests
{
    [Fact]
    public async Task RunAsync_ChecksConnectionBeforeScenarioExecution()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-preflight");
        var host = new FakePreflightHost(connected: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => LocalLlmPreflightHarness.RunAsync(
            new LocalLlmPreflightRunOptions(
                HostEndpoint: "http://127.0.0.1:8181",
                OutputRoot: tempRoot,
                RunnerVersion: LocalLlmPreflightConstants.RunnerVersion,
                GitCommit: "deadbeef"),
            host));

        Assert.Equal(1, host.CheckConnectionCalls);
        Assert.Equal(0, host.InvokeCalls);
    }

    private sealed class FakePreflightHost : ILocalLlmPreflightHost
    {
        private readonly bool _connected;

        public FakePreflightHost(bool connected)
        {
            _connected = connected;
        }

        public Uri HostEndpoint => new("http://127.0.0.1:8181");

        public int CheckConnectionCalls { get; private set; }

        public int InvokeCalls { get; private set; }

        public Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
        {
            CheckConnectionCalls++;
            return Task.FromResult(_connected);
        }

        public Task<LocalLlmPreflightHostIdentity> ResolveIdentityAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LocalLlmPreflightHostIdentity(null, null));
        }

        public Task<LocalLlmPreflightInvocationResult> InvokeAsync(
            LocalLlmPreflightScenario scenario,
            CancellationToken cancellationToken = default)
        {
            InvokeCalls++;
            return Task.FromResult(new LocalLlmPreflightInvocationResult(
                Response: null,
                TelemetryRecords: [],
                RequestStartedUtc: DateTimeOffset.UtcNow,
                RequestDurationMs: 0,
                ExceptionType: null,
                ExceptionMessage: null));
        }
    }
}
