namespace San.Common;

public enum ProtectedExecutionHandleKind
{
    CrypticCustodyPointer = 0,
    HotStateHandle = 1,
    WorkingFormHandle = 2,
    DerivationHandle = 3,
    ObservationHandle = 4,
    BondHandle = 5
}

public enum ProtectedExecutionActFamily
{
    Intake = 0,
    Differentiate = 1,
    Seal = 2,
    Orient = 3,
    Derive = 4,
    Mint = 5,
    Delegate = 6,
    Return = 7,
    Refuse = 8,
    Defer = 9
}

public enum ProtectedExecutionAuthorityClass
{
    None = 0,
    FatherBound = 1,
    BondedDelegated = 2,
    SanctuaryGoverned = 3
}

public enum ProtectedExecutionDisclosureCeiling
{
    Sealed = 0,
    StructuralOnly = 1,
    MintedPrimeOnly = 2,
    AuthorizedFieldSlice = 3
}

public enum ProtectedExecutionMintedOutputClass
{
    RefusalReceipt = 0,
    DeferredReceipt = 1,
    StructuralProjection = 2,
    StandingSummary = 3,
    PrimeDerivative = 4,
    AuthoredDerivativeSeed = 5
}

public enum ProtectedExecutionPathState
{
    Selected = 0,
    Refused = 1,
    Deferred = 2,
    Completed = 3,
    Revoked = 4
}

public sealed record ProtectedExecutionInputHandle(
    string Handle,
    ProtectedExecutionHandleKind HandleKind,
    bool ObserverLegible,
    bool MintEligible,
    string ProtectionClass);

public sealed record ProtectedExecutionCapabilityReceipt(
    string CapabilityHandle,
    string RuntimeHandle,
    string ScopeHandle,
    string WitnessedBy,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    IReadOnlyList<ProtectedExecutionInputHandle> InputHandles,
    IReadOnlyList<ProtectedExecutionActFamily> AdmissibleActFamilies,
    IReadOnlyList<ProtectedExecutionActFamily> ForbiddenActFamilies,
    IReadOnlyList<ProtectedExecutionMintedOutputClass> ReachableMintedOutputClasses,
    DateTimeOffset TimestampUtc);

public sealed record ProtectedExecutionDirective(
    string DirectiveHandle,
    string CapabilityHandle,
    string LawHandle,
    string RequestedBy,
    string PurposeClass,
    string ScopeHandle,
    string TraceHandle,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    IReadOnlyList<ProtectedExecutionInputHandle> InputHandles,
    IReadOnlyList<ProtectedExecutionActFamily> AdmissibleActFamilies,
    IReadOnlyList<ProtectedExecutionActFamily> ForbiddenActFamilies,
    IReadOnlyList<ProtectedExecutionMintedOutputClass> AllowedMintedOutputClasses,
    bool SubdelegationAllowed);

public sealed record ProtectedExecutionPathReceipt(
    string PathHandle,
    string DirectiveHandle,
    ProtectedExecutionPathState State,
    IReadOnlyList<ProtectedExecutionActFamily> SelectedActPath,
    IReadOnlyList<ProtectedExecutionMintedOutputClass> MintedOutputClasses,
    string? OutcomeCode,
    DateTimeOffset TimestampUtc);

public sealed record MintedProtectedWorkProduct(
    string OutputHandle,
    string PathHandle,
    ProtectedExecutionMintedOutputClass OutputClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    bool ObserverLegible,
    bool PrimeEligible,
    bool RightsBearingEligible,
    bool OnwardDisclosureAllowed);

public sealed record ProtectedExecutionGovernanceReceipt(
    string ReceiptHandle,
    string PathHandle,
    string GovernedBy,
    string DecisionCode,
    bool ReturnedToFather,
    bool WitnessOnly,
    IReadOnlyList<string> WithheldOutputHandles,
    DateTimeOffset TimestampUtc);
