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
            ValidationSummary: "unavailable:local-hdt-bridge-disabled",
            ProfileSummary: "supplemental hopng evidence unavailable in this runtime",
            FailureCode: "hopng-bridge-unavailable"));
    }
}
