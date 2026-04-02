using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class WorkerGovernanceKeys
{
    public static string CreateIdentityThreadRootHandle(
        string projectSpaceId,
        string threadId,
        string governanceRootId)
    {
        return CreateHandle("worker-thread-root://", projectSpaceId, threadId, governanceRootId);
    }

    public static string CreateWorkerThreadGovernanceHandle(
        string threadId,
        string scopeClass,
        string bindBurdenClass)
    {
        return CreateHandle("worker-thread-governance://", threadId, scopeClass, bindBurdenClass);
    }

    public static string CreateWorkerThreadWitnessHandle(
        string threadId,
        string witnessEventId)
    {
        return CreateHandle("worker-thread-witness://", threadId, witnessEventId);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
