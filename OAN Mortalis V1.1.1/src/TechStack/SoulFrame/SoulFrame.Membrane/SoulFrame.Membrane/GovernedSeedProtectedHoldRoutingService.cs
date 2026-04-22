using System.Security.Cryptography;
using System.Text;
using San.Common;
using San.Nexus.Control;

namespace SoulFrame.Membrane;

public interface IGovernedSeedProtectedHoldRoutingService
{
    GovernedSeedProtectedHoldRoutingReceipt CreateRouting(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedEvaluationResult evaluationResult);
}

public sealed class GovernedSeedProtectedHoldRoutingService : IGovernedSeedProtectedHoldRoutingService
{
    private readonly IGovernedNexusControlService _nexusControlService;

    public GovernedSeedProtectedHoldRoutingService(IGovernedNexusControlService nexusControlService)
    {
        _nexusControlService = nexusControlService ?? throw new ArgumentNullException(nameof(nexusControlService));
    }

    public GovernedSeedProtectedHoldRoutingReceipt CreateRouting(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(returnIntakeReceipt);
        ArgumentNullException.ThrowIfNull(evaluationResult);

        var residueEvidence = evaluationResult.ProtectedResidueEvidence ?? [];
        var hasContextualResidue = residueEvidence.Any(static evidence =>
            evidence.ResidueKind is GovernedSeedProtectedResidueKind.Contextual or GovernedSeedProtectedResidueKind.Mixed);
        var hasSelfStateResidue = residueEvidence.Any(static evidence =>
            evidence.ResidueKind is GovernedSeedProtectedResidueKind.SelfState or GovernedSeedProtectedResidueKind.Mixed);

        var route = residueEvidence.Count switch
        {
            0 => GovernedSeedProtectedHoldRoute.None,
            _ when hasContextualResidue && hasSelfStateResidue => GovernedSeedProtectedHoldRoute.SplitRouteAcrossCGoaAndCMos,
            _ when hasSelfStateResidue => GovernedSeedProtectedHoldRoute.RouteToCMos,
            _ => GovernedSeedProtectedHoldRoute.RouteToCGoa
        };

        IReadOnlyList<GovernedSeedCustodyHoldSurface> destinationSurfaces = route switch
        {
            GovernedSeedProtectedHoldRoute.RouteToCGoa =>
            [
                bootstrapReceipt.CustodySnapshot.CGoaHoldSurface
            ],
            GovernedSeedProtectedHoldRoute.RouteToCMos =>
            [
                bootstrapReceipt.CustodySnapshot.CMosHoldSurface
            ],
            GovernedSeedProtectedHoldRoute.SplitRouteAcrossCGoaAndCMos =>
            [
                bootstrapReceipt.CustodySnapshot.CGoaHoldSurface,
                bootstrapReceipt.CustodySnapshot.CMosHoldSurface
            ],
            _ => Array.Empty<GovernedSeedCustodyHoldSurface>()
        };

        var destinationHandles = destinationSurfaces
            .Select(static surface => surface.SurfaceHandle)
            .ToArray();

        var nexusDisposition = _nexusControlService.EvaluateHoldRoutingDisposition(
            bootstrapReceipt,
            returnIntakeReceipt,
            evaluationResult,
            route);

        return new GovernedSeedProtectedHoldRoutingReceipt(
            RoutingHandle: CreateHandle(
                "hold-routing://",
                bootstrapReceipt.BootstrapHandle,
                returnIntakeReceipt.IntakeHandle,
                route.ToString()),
            RoutingProfile: "typed-first-route-protected-hold",
            ProtectedHoldRoute: route,
            DestinationSurfaces: destinationSurfaces,
            DestinationHandles: destinationHandles,
            EvidenceClass: nexusDisposition.EvidenceClass,
            ReviewRequired: nexusDisposition.ReviewRequired,
            SourceReason: nexusDisposition.SourceReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
