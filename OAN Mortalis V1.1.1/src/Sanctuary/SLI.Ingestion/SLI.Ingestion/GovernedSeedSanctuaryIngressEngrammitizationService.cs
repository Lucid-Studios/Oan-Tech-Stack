using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace SLI.Ingestion;

public interface IGovernedSeedSanctuaryIngressEngrammitizationService
{
    GovernedSeedSanctuaryIngressPreparation Prepare(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedIngressAccessClass ingressAccessClass = GovernedSeedIngressAccessClass.PromptInput);
}

public sealed class GovernedSeedSanctuaryIngressEngrammitizationService : IGovernedSeedSanctuaryIngressEngrammitizationService
{
    public GovernedSeedSanctuaryIngressPreparation Prepare(
        string agentId,
        string theaterId,
        string input,
        GovernedSeedIngressAccessClass ingressAccessClass = GovernedSeedIngressAccessClass.PromptInput)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var preparedInput = input
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Trim();
        var sourceInputHandle = CreateHandle("sanctuary-input://", agentId, theaterId, input);
        var preparedInputHandle = CreateHandle("sanctuary-engram://", agentId, theaterId, preparedInput, ingressAccessClass.ToString());
        var packetHandle = CreateHandle("sanctuary-ingress-packet://", sourceInputHandle, preparedInputHandle, ingressAccessClass.ToString());
        var receipt = new GovernedSeedSanctuaryIngressReceipt(
            ReceiptHandle: CreateHandle("sanctuary-ingress-receipt://", packetHandle, ingressAccessClass.ToString()),
            PacketHandle: packetHandle,
            ReceiptProfile: "sanctuary-first-engrammitization-boundary",
            PacketProfile: "sanctuary-obsidian-wall-preparation",
            SourceInputHandle: sourceInputHandle,
            PreparedInputHandle: preparedInputHandle,
            IngressAccessClass: ingressAccessClass,
            ExternalInputRequiresCustodyChain: true,
            ObsidianWallApplied: true,
            EngrammitizedForCradleTek: true,
            RawPromptAuthorityTerminated: true,
            SourceReason: ResolveSourceReason(ingressAccessClass),
            TimestampUtc: DateTimeOffset.UtcNow);

        return new GovernedSeedSanctuaryIngressPreparation(
            PreparedInput: preparedInput,
            Receipt: receipt);
    }

    private static string ResolveSourceReason(GovernedSeedIngressAccessClass ingressAccessClass) => ingressAccessClass switch
    {
        GovernedSeedIngressAccessClass.ToolAccess => "sanctuary-first-engrammitization-before-tool-space-custody",
        GovernedSeedIngressAccessClass.DataAccess => "sanctuary-first-engrammitization-before-data-space-custody",
        _ => "sanctuary-first-engrammitization-at-obsidian-wall"
    };

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
