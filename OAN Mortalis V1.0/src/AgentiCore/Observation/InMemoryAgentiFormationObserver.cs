using Oan.Common;

namespace AgentiCore.Observation;

public sealed class InMemoryAgentiFormationObserver : IAgentiFormationObserver
{
    private readonly List<AgentiFormationObservation> _observations = [];
    private readonly object _gate = new();

    public Task RecordAsync(
        AgentiFormationObservation observation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            _observations.Add(observation);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<AgentiFormationObservation> Snapshot()
    {
        lock (_gate)
        {
            return _observations.ToArray();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _observations.Clear();
        }
    }
}
