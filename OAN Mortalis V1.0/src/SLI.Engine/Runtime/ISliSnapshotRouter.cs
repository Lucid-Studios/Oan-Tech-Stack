using System.Threading;
using System.Threading.Tasks;

namespace SLI.Engine.Runtime;

/// <summary>
/// Abstraction for governed routing and persistence of symbolic cognition execution snapshots.
/// Implementations handle specific artifact postures (e.g., local debug drop, CI attachment, ledger emit).
/// </summary>
internal interface ISliSnapshotRouter
{
    /// <summary>
    /// Routes the emitted snapshot safely without bleeding identity-forming memory context.
    /// </summary>
    Task RouteAsync(SliExecutionSnapshot snapshot, CancellationToken cancellationToken = default);
}
