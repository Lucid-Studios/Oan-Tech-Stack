using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Oan.Common;

namespace Oan.Sli.Tests;

internal sealed class StrictTestEgressRouter : IManagedEgressRouter
{
    private readonly ConcurrentBag<ManagedEgressEnvelope> _captured = [];

    public IReadOnlyCollection<ManagedEgressEnvelope> CapturedEnvelopes => _captured;

    public bool AllowGovernanceArtifacts { get; set; } = false;
    public bool AllowIdentityForming { get; set; } = false;
    public SliEgressJurisdictionClass AllowedJurisdiction { get; set; } = SliEgressJurisdictionClass.AgentiCore;

    public async Task<bool> TryRouteEgressAsync(ManagedEgressEnvelope envelope, Func<Task> egressAction, CancellationToken cancellationToken = default)
    {
        _captured.Add(envelope);

        if (envelope.IdentityFormingAllowed && !AllowIdentityForming)
        {
            return false;
        }

        if (envelope.RetentionPosture == SliEgressRetentionPosture.GovernanceArtifact && !AllowGovernanceArtifacts)
        {
            return false;
        }

        if (envelope.JurisdictionClass != AllowedJurisdiction)
        {
            return false;
        }

        await egressAction().ConfigureAwait(false);
        return true;
    }
}
