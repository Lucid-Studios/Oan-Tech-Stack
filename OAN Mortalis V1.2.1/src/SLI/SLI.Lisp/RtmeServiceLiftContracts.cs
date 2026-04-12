namespace SLI.Lisp;

public sealed record PrimeCrypticLiftBindingRecord(
    string BindingRecordId,
    string PrimeSupervisoryWitnessSurface,
    string CrypticWorkingSurface,
    string NonAutonomyRule,
    string NonGoverningRule,
    IReadOnlyList<string> WitnessRefs);

public sealed record HostedRtmeServiceLiftReceipt(
    string ReceiptHandle,
    string IssuedRtmeHandle,
    string HostedBundleHandle,
    string AuthorityRecordRef,
    string PrimeCrypticBindingRef,
    string PreLiftPosture,
    string ResultingHostedPosture,
    string CmePlacementWithheldContinuation,
    DateTimeOffset TimestampUtc);
