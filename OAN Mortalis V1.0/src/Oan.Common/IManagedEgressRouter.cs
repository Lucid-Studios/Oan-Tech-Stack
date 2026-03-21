using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oan.Common;

/// <summary>
/// A strict authority gatekeeper deciding whether an internal SLI/OAN framework payload 
/// may successfully emit a side effect beyond its own memory bounds.
/// </summary>
public interface IManagedEgressRouter
{
    /// <summary>
    /// Evaluates the target egress action against the configured bounds.
    /// If authorized, invokes the action and returns true. If unauthorized, blocks the action and returns false.
    /// </summary>
    Task<bool> TryRouteEgressAsync(ManagedEgressEnvelope envelope, Func<Task> egressAction, CancellationToken cancellationToken = default);
}

/// <summary>
/// The fail-closed primitive Router inherently denying all egress.
/// Used naturally as a default for isolated test frames and non-configured endpoints.
/// </summary>
public sealed class NullEgressRouter : IManagedEgressRouter
{
    public static readonly NullEgressRouter Instance = new();

    public Task<bool> TryRouteEgressAsync(ManagedEgressEnvelope envelope, Func<Task> egressAction, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
