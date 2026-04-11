using System.Security.Cryptography;
using System.Text;

namespace SLI.Lisp;

public interface ICrypticLispBundleService
{
    HostedCrypticLispBundleReceipt DescribeResidentBundle();

    IReadOnlyDictionary<string, string> LoadModules();

    bool HasCanonicalFloorSet();
}

public sealed class GovernedCrypticLispBundleService : ICrypticLispBundleService
{
    private static readonly string[] CanonicalFloorModules =
    [
        "core.lisp",
        "parser.lisp",
        "transport.lisp",
        "witness.lisp",
        "admissibility.lisp",
        "compass.lisp"
    ];

    public HostedCrypticLispBundleReceipt DescribeResidentBundle()
    {
        var modules = LoadModules();
        var moduleNames = modules.Keys
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new HostedCrypticLispBundleReceipt(
            BundleHandle: CreateHandle("cryptic-lisp-bundle://", moduleNames),
            BundleProfile: "v121-sanctuary-hosted-cryptic-lisp-bundle",
            HostedByIssuedRuntime: "SanctuaryID.RTME",
            CrypticCarrierKind: "sli-lisp-symbolic-runtime",
            InterconnectProfile: "engine-facing-passive-hosted-bundle",
            ModuleNames: moduleNames,
            HostedExecutionOnly: true,
            CanonicalFloorSetReady: HasCanonicalFloorSet(),
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
