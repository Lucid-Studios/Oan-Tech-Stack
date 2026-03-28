using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedLowMindSfRoutingService
{
    GovernedSeedLowMindSfRoutePacket CreateRoute(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedMemoryContext memoryContext);
}

public sealed class GovernedSeedLowMindSfRoutingService : IGovernedSeedLowMindSfRoutingService
{
    private static readonly string[] HigherOrderPromptCues =
    [
        "analyze",
        "compare",
        "synthesize",
        "step by step",
        "plan",
        "investigate",
        "reason through",
        "query",
        "retrieve",
        "use tools",
        "tool access",
        "data access",
        "look up",
        "search",
        "cross-reference"
    ];

    public GovernedSeedLowMindSfRoutePacket CreateRoute(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedMemoryContext memoryContext)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(memoryContext);

        var routeKind = ResolveRouteKind(request);
        var sourceReason = ResolveSourceReason(request.IngressAccessClass, routeKind);

        return new GovernedSeedLowMindSfRoutePacket(
            PacketHandle: CreateHandle(
                "lowmind-sf-route://",
                bootstrapReceipt.BootstrapHandle,
                memoryContext.ContextHandle,
                request.IngressAccessClass.ToString(),
                routeKind.ToString(),
                request.Input),
            PacketProfile: "soulframe-lowmind-sf-ingress-route",
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            MemoryContextHandle: memoryContext.ContextHandle,
            IngressAccessClass: request.IngressAccessClass,
            RouteKind: routeKind,
            RoutedThroughSoulFrame: true,
            RequiresHigherOrderFunction: routeKind == GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction,
            SourceReason: sourceReason,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedLowMindSfRouteKind ResolveRouteKind(GovernedSeedEvaluationRequest request)
    {
        return request.IngressAccessClass switch
        {
            GovernedSeedIngressAccessClass.ToolAccess => GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction,
            GovernedSeedIngressAccessClass.DataAccess => GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction,
            _ => ContainsAny(request.Input, HigherOrderPromptCues)
                ? GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction
                : GovernedSeedLowMindSfRouteKind.DirectPrompt
        };
    }

    private static string ResolveSourceReason(
        GovernedSeedIngressAccessClass ingressAccessClass,
        GovernedSeedLowMindSfRouteKind routeKind)
    {
        return ingressAccessClass switch
        {
            GovernedSeedIngressAccessClass.ToolAccess => "tool-access-routed-through-lowmind-sf",
            GovernedSeedIngressAccessClass.DataAccess => "data-access-routed-through-lowmind-sf",
            _ when routeKind == GovernedSeedLowMindSfRouteKind.HigherOrderEcFunction => "prompt-routed-to-ec-higher-order-function",
            _ => "prompt-routed-to-direct-cryptic-prompt"
        };
    }

    private static bool ContainsAny(string input, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            if (input.Contains(value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
