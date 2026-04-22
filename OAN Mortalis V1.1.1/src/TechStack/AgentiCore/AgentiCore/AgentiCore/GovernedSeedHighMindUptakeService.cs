using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace AgentiCore;

public interface IGovernedSeedHighMindUptakeService
{
    GovernedSeedHighMindContext CreateContext(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedHostedLlmSeedReceipt hostedLlmReceipt);
}

public sealed class GovernedSeedHighMindUptakeService : IGovernedSeedHighMindUptakeService
{
    public GovernedSeedHighMindContext CreateContext(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute,
        GovernedSeedHostedLlmSeedReceipt hostedLlmReceipt)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(personifiedMemoryContext);
        ArgumentNullException.ThrowIfNull(lowMindSfRoute);
        ArgumentNullException.ThrowIfNull(hostedLlmReceipt);
        ArgumentNullException.ThrowIfNull(request.SanctuaryIngressReceipt);

        var bootstrapHandle = request.BootstrapReceipt?.BootstrapHandle ?? hostedLlmReceipt.BootstrapHandle;
        var uptakeKind = lowMindSfRoute.RouteKind == GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction
            ? GovernedSeedHighMindUptakeKind.HigherOrderEcIntake
            : GovernedSeedHighMindUptakeKind.DirectPromptIntake;

        return new GovernedSeedHighMindContext(
            ContextHandle: CreateHandle(
                "highmind://",
                bootstrapHandle,
                personifiedMemoryContext.ContextHandle,
                lowMindSfRoute.PacketHandle,
                hostedLlmReceipt.ReceiptHandle,
                uptakeKind.ToString()),
            ContextProfile: "agenticore-highmind-uptake-staging",
            BootstrapHandle: bootstrapHandle,
            SanctuaryIngressReceiptHandle: request.SanctuaryIngressReceipt.ReceiptHandle,
            MemoryContextHandle: personifiedMemoryContext.ContextHandle,
            LowMindSfRouteHandle: lowMindSfRoute.PacketHandle,
            HostedLlmReceiptHandle: hostedLlmReceipt.ReceiptHandle,
            HostedLlmState: hostedLlmReceipt.ResponsePacket.State,
            IngressAccessClass: lowMindSfRoute.IngressAccessClass,
            UptakeKind: uptakeKind,
            SoulFramePrepared: lowMindSfRoute.RoutedThroughSoulFrame,
            SeedProgressionAccepted: hostedLlmReceipt.ResponsePacket.Accepted,
            SourceReason: lowMindSfRoute.SourceReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
