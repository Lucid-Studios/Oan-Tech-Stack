using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedSituationalContextService
{
    GovernedSeedSituationalContext CreateContext(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt,
        GovernedSeedSoulFrameStewardshipReceipt stewardshipReceipt,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedMemoryContext memoryContext,
        GovernedSeedEvaluationResult evaluationResult);
}

public sealed class GovernedSeedSituationalContextService : IGovernedSeedSituationalContextService
{
    public GovernedSeedSituationalContext CreateContext(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt,
        GovernedSeedSoulFrameStewardshipReceipt stewardshipReceipt,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedMemoryContext memoryContext,
        GovernedSeedEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(projectionReceipt);
        ArgumentNullException.ThrowIfNull(returnIntakeReceipt);
        ArgumentNullException.ThrowIfNull(holdRoutingReceipt);
        ArgumentNullException.ThrowIfNull(stewardshipReceipt);
        ArgumentNullException.ThrowIfNull(lowMindSfRoute);
        ArgumentNullException.ThrowIfNull(memoryContext);
        ArgumentNullException.ThrowIfNull(evaluationResult);

        return new GovernedSeedSituationalContext(
            ContextHandle: CreateHandle(
                "situational-context://",
                bootstrapReceipt.BootstrapHandle,
                projectionReceipt.ProjectionHandle,
                returnIntakeReceipt.IntakeHandle,
                stewardshipReceipt.StewardshipHandle,
                lowMindSfRoute.PacketHandle,
                memoryContext.ContextHandle),
            ContextProfile: "soulframe-stewardship-situational-context",
            DecisionCode: evaluationResult.Decision,
            GovernanceTrace: evaluationResult.GovernanceTrace,
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            ProjectionHandle: projectionReceipt.ProjectionHandle,
            ReturnIntakeHandle: returnIntakeReceipt.IntakeHandle,
            StewardshipHandle: stewardshipReceipt.StewardshipHandle,
            HoldRoutingHandle: holdRoutingReceipt.RoutingHandle,
            Accepted: evaluationResult.Accepted,
            GovernanceState: evaluationResult.GovernanceState,
            StewardAuthorityProfile: stewardshipReceipt.StewardshipProfile,
            CollapseReadinessState: stewardshipReceipt.CollapseReadinessState,
            ProtectedHoldClass: stewardshipReceipt.ProtectedHoldClass,
            ProtectedHoldRoute: stewardshipReceipt.ProtectedHoldRoute,
            ReviewState: stewardshipReceipt.ReviewState,
            HoldReviewRequired: holdRoutingReceipt.ReviewRequired,
            HoldDestinationHandles: holdRoutingReceipt.DestinationHandles,
            ReturnDeniedCount: returnIntakeReceipt.Classification.DeniedCount,
            ReturnDeferredCount: returnIntakeReceipt.Classification.DeferredCount,
            LowMindSfRoute: lowMindSfRoute,
            MemoryContext: memoryContext,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
