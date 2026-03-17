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

        var disclosureReceipt = request.Snapshot.WeatherDisclosureReceipts?.LastOrDefault();
        var officeSummary = BuildOfficeAuthoritySummary(request.Snapshot);
        var weatherPacket = request.Snapshot.CommunityWeatherPacket ?? disclosureReceipt?.CommunityWeatherPacket;
        var weatherSummary = weatherPacket is null
            ? "community-weather:unknown"
            : $"community-weather:{weatherPacket.Status.ToString().ToLowerInvariant()};steward-attention:{weatherPacket.StewardAttention.ToString().ToLowerInvariant()};anchor-state:{weatherPacket.AnchorState.ToString().ToLowerInvariant()}";
        var disclosureSummary = disclosureReceipt is null
            ? "care-routing:none;disclosure-scope:community;evidence-sufficiency:sufficient;withheld:none"
            : $"care-routing:{disclosureReceipt.RoutingState.ToString().ToLowerInvariant()};disclosure-scope:{disclosureReceipt.DisclosureScope.ToString().ToLowerInvariant()};evidence-sufficiency:{disclosureReceipt.EvidenceSufficiencyState.ToString().ToLowerInvariant().Replace('_', '-')};withheld:{FormatWithheldMarkers(disclosureReceipt.WithheldMarkers)}";

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
            ValidationSummary: $"unavailable:local-hdt-bridge-disabled;{weatherSummary};{disclosureSummary};{officeSummary}",
            ProfileSummary: $"supplemental hopng evidence unavailable in this runtime;{weatherSummary};{disclosureSummary};{officeSummary}",
            FailureCode: "hopng-bridge-unavailable"));
    }

    private static string FormatWithheldMarkers(IReadOnlyList<WeatherWithheldMarker> markers)
    {
        return markers.Count == 0
            ? "none"
            : string.Join(",", markers.Select(marker => marker.ToString().ToLowerInvariant().Replace('_', '-')));
    }

    private static string BuildOfficeAuthoritySummary(GovernanceLoopStateSnapshot snapshot)
    {
        var receipts = snapshot.OfficeAuthorityReceipts ?? [];
        if (receipts.Count == 0)
        {
            return "office-authority:none";
        }

        var officeStates = receipts
            .GroupBy(receipt => receipt.Office)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var latest = group
                    .OrderBy(receipt => receipt.TimestampUtc)
                    .ThenBy(receipt => receipt.AuthorityHandle, StringComparer.Ordinal)
                    .Last();
                return $"{latest.Office.ToString().ToLowerInvariant()}={latest.ActionEligibility.ToString().ToLowerInvariant().Replace('_', '-')}/{latest.ViewEligibility.ToString().ToLowerInvariant().Replace('_', '-')}";
            });

        return $"office-authority:{string.Join(",", officeStates)}";
    }
}
