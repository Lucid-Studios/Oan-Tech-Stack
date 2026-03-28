using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace SLI.Lisp;

public interface ICrypticLispBundleService
{
    GovernedSeedCrypticLispBundleReceipt DescribeResidentBundle();

    IReadOnlyDictionary<string, string> LoadModules();

    bool HasCanonicalFloorSet();
}

public sealed class GovernedCrypticLispBundleService : ICrypticLispBundleService
{
    private static readonly string[] CanonicalFloorModules =
    [
        "core.lisp",
        "parser.lisp",
        "reasoning.lisp",
        "engram.lisp",
        "compass.lisp",
        "morphology.lisp",
        "locality.lisp",
        "rehearsal.lisp",
        "witness.lisp",
        "transport.lisp",
        "admissibility.lisp",
        "accountability.lisp"
    ];

    public GovernedSeedCrypticLispBundleReceipt DescribeResidentBundle()
    {
        var modules = LoadModules();
        var moduleNames = modules.Keys.OrderBy(static name => name, StringComparer.OrdinalIgnoreCase).ToArray();

        return new GovernedSeedCrypticLispBundleReceipt(
            BundleHandle: CreateHandle("lisp-bundle://", moduleNames),
            BundleProfile: "sanctuary-hosted-cryptic-lisp-bundle",
            HostedByRuntime: "csharp-prime-host",
            CrypticCarrierKind: "sli-lisp-symbolic-runtime",
            InterconnectProfile: "prime-to-cryptic-sli-interconnect",
            ModuleNames: moduleNames,
            HostedExecutionOnly: true,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    public IReadOnlyDictionary<string, string> LoadModules() => LispModuleCatalog.LoadModules();

    public bool HasCanonicalFloorSet()
    {
        var modules = LoadModules();
        return CanonicalFloorModules.All(modules.ContainsKey);
    }

    private static string CreateHandle(string prefix, IEnumerable<string> moduleNames)
    {
        var material = string.Join("|", moduleNames);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
