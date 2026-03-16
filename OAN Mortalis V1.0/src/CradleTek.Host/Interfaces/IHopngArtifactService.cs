using Oan.Common;

namespace CradleTek.Host.Interfaces;

public interface IHopngArtifactService : ICradleService
{
    Task<GovernedHopngArtifactReceipt> EmitAsync(
        GovernedHopngEmissionRequest request,
        CancellationToken cancellationToken = default);
}
