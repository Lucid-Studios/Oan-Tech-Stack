using System;
using System.Threading;
using System.Threading.Tasks;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

internal sealed class TestPermissiveEgressRouter : IManagedEgressRouter
{
    public async Task<bool> TryRouteEgressAsync(ManagedEgressEnvelope envelope, Func<Task> egressAction, CancellationToken cancellationToken = default)
    {
        // Enforce the execution so integration tests receive real IO
        await egressAction().ConfigureAwait(false);
        return true;
    }
}
