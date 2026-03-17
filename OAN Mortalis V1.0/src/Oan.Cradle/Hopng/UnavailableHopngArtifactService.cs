using CradleTek.Host.Interfaces;
using Oan.Common;

namespace Oan.Cradle;

public sealed class UnavailableHopngArtifactService : IHopngArtifactService
{
    public string ContainerName => "cradletek-hopng";

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GovernedHopngArtifactReceipt> EmitAsync(
        GovernedHopngEmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var weatherPacket = request.Snapshot.CommunityWeatherPacket;
        var weatherSummary = weatherPacket is null
            ? "community-weather:unknown"
            : $"community-weather:{weatherPacket.Status.ToString().ToLowerInvariant()};steward-attention:{weatherPacket.StewardAttention.ToString().ToLowerInvariant()};anchor-state:{weatherPacket.AnchorState.ToString().ToLowerInvariant()}";

        return Task.FromResult(new GovernedHopngArtifactReceipt(
            ArtifactHandle: GovernedHopngArtifactKeys.CreateArtifactHandle(request.LoopKey, request.Profile),
            LoopKey: request.LoopKey,
            CandidateId: request.CandidateId,
            CandidateProvenance: request.CandidateProvenance,
            Profile: request.Profile,
            Stage: request.Stage,
            Outcome: GovernedHopngArtifactOutcome.Unavailable,
            IssuedBy: "CradleTek Governed Transit",
            TimestampUtc: DateTimeOffset.UtcNow,
            ArtifactId: null,
            ManifestPath: null,
            ProjectionPath: null,
            ValidationSummary: $"unavailable:local-hdt-bridge-disabled;{weatherSummary}",
            ProfileSummary: $"supplemental hopng evidence unavailable in this runtime;{weatherSummary}",
            FailureCode: "hopng-bridge-unavailable"));
    }
}
