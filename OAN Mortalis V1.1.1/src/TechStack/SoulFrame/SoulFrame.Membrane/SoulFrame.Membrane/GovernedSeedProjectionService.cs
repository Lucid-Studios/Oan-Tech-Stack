using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedProjectionService
{
    GovernedSeedSoulFrameProjectionReceipt CreateProjection(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt);
}

public sealed class GovernedSeedProjectionService : IGovernedSeedProjectionService
{
    public GovernedSeedSoulFrameProjectionReceipt CreateProjection(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);

        var sessionHandle = CreateHandle("soulframe-session://", request.AgentId, request.TheaterId);
        return new GovernedSeedSoulFrameProjectionReceipt(
            ProjectionHandle: CreateHandle("projection://", sessionHandle, request.Input),
            SessionHandle: sessionHandle,
            ProjectionIntent: GovernedSeedProjectionIntent.BoundedCognitionUse,
            ProjectionProfile: "mitigated-worker-use-only",
            ProvenanceMarker: bootstrapReceipt.BootstrapHandle,
            WorkerUseOnly: true,
            MitigatedSelfStateHandle: bootstrapReceipt.CustodySnapshot.CrypticSelfGelHandle,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
