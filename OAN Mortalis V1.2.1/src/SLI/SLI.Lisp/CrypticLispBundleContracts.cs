namespace SLI.Lisp;

public sealed record HostedCrypticLispBundleReceipt(
    string BundleHandle,
    string BundleProfile,
    string HostedByIssuedRuntime,
    string CrypticCarrierKind,
    string InterconnectProfile,
    IReadOnlyList<string> ModuleNames,
    bool HostedExecutionOnly,
    bool CanonicalFloorSetReady,
    DateTimeOffset TimestampUtc);
