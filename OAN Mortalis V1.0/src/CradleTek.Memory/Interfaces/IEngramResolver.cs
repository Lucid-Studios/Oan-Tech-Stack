using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Models;

namespace CradleTek.Memory.Interfaces;

public interface IEngramResolver
{
    Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default);
    Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default);
    Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default);

    async Task<EngramSelfResolutionResult> ResolveSelfSensitiveAsync(
        CognitionContext context,
        string cSelfGelHandle,
        CancellationToken cancellationToken = default)
    {
        var relevant = await ResolveRelevantAsync(context, cancellationToken).ConfigureAwait(false);
        return EngramSelfResolutionFactory.CreateFallback(relevant, cSelfGelHandle);
    }
}
