using System.Security.Cryptography;
using System.Text;
using San.Common;
using San.Nexus.Control;

namespace SoulFrame.Membrane;

public interface IGovernedSeedStewardshipService
{
    GovernedSeedSoulFrameStewardshipReceipt CreateStewardship(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt,
        GovernedSeedEvaluationResult evaluationResult);
}

public sealed class GovernedSeedStewardshipService : IGovernedSeedStewardshipService
{
    private readonly IGovernedNexusControlService _nexusControlService;

    public GovernedSeedStewardshipService(IGovernedNexusControlService nexusControlService)
    {
        _nexusControlService = nexusControlService ?? throw new ArgumentNullException(nameof(nexusControlService));
    }

    public GovernedSeedSoulFrameStewardshipReceipt CreateStewardship(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt,
        GovernedSeedEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(projectionReceipt);
        ArgumentNullException.ThrowIfNull(returnIntakeReceipt);
        ArgumentNullException.ThrowIfNull(holdRoutingReceipt);
        ArgumentNullException.ThrowIfNull(evaluationResult);

        var stewardshipDisposition = _nexusControlService.EvaluateStewardshipDisposition(
            evaluationResult,
            holdRoutingReceipt);

        return new GovernedSeedSoulFrameStewardshipReceipt(
            StewardshipHandle: CreateHandle("stewardship://", projectionReceipt.SessionHandle, evaluationResult.Decision),
            StewardshipProfile: "projection-intake-review",
            StewardPrimary: true,
            MotherGovernanceFocus: true,
            FatherGovernanceFocus: true,
            CollapseReadinessState: stewardshipDisposition.CollapseReadinessState,
            ProtectedHoldClass: stewardshipDisposition.ProtectedHoldClass,
            HoldRoutingHandle: holdRoutingReceipt.RoutingHandle,
            ProtectedHoldRoute: holdRoutingReceipt.ProtectedHoldRoute,
            ProtectedHoldDestinationHandles: holdRoutingReceipt.DestinationHandles,
            ReviewState: stewardshipDisposition.ReviewState,
            SourceReason: stewardshipDisposition.SourceReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
