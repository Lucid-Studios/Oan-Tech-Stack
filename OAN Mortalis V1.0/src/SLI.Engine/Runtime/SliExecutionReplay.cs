using System;
using System.Text.Json;

namespace SLI.Engine.Runtime;

/// <summary>
/// Recreates an inspection/replay state from a SLI execution snapshot JSON.
/// Does not reconstruct live external services; provides only projected inspection state.
/// </summary>
internal sealed class SliExecutionReplay
{
    public SliExecutionSnapshot Snapshot { get; }

    private SliExecutionReplay(SliExecutionSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    /// <summary>
    /// Deserializes a deterministic snapshot JSON into an inspection replay state.
    /// </summary>
    /// <param name="json">The JSON snapshot string to parse.</param>
    /// <returns>An instance of <see cref="SliExecutionReplay"/> encapsulating the snapshot properties.</returns>
    /// <exception cref="ArgumentException">Thrown if the JSON is malformed or invalid.</exception>
    public static SliExecutionReplay ReplayFromSnapshot(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Snapshot JSON cannot be empty.", nameof(json));
        }

        var snapshot = JsonSerializer.Deserialize<SliExecutionSnapshot>(json, SliExecutionSnapshotFactory.DefaultOptions);
        if (snapshot == null)
        {
            throw new ArgumentException("Failed to deserialize snapshot JSON.");
        }

        return new SliExecutionReplay(snapshot);
    }
}
