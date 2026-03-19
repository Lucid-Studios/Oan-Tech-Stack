namespace Oan.Common;

public enum GovernedWorkerSpecies
{
    RepoBugStewardWorker = 0
}

public enum WorkerInstanceMode
{
    RequestOnly = 0,
    HostIssued = 1,
    ReuseExistingWorker = 2
}

public enum WorkerCompletionState
{
    Completed = 0,
    Halted = 1,
    Denied = 2,
    Deferred = 3
}

public enum WorkerResidueDisposition
{
    None = 0,
    ReturnedClean = 1,
    NeedsClassification = 2,
    QuarantineRequired = 3
}

public enum WorkerReasonCode
{
    NeedsSpecification = 0,
    InsufficientEvidence = 1,
    DeferredReview = 2,
    AuthorityDenied = 3,
    DisclosureScopeViolation = 4,
    NoHandleNoAction = 5,
    UnsupportedClaim = 6,
    BrokenWindow = 7,
    UnknownNotFailure = 8,
    OfficeNonOverlap = 9,
    PromptInjection = 10,
    PredatorySharedDomainRisk = 11,
    CoerciveBondingPosture = 12,
    ContinuityInstability = 13,
    IdentityOvercollapseRisk = 14
}

public sealed record WorkerHandoffPacket(
    string HandoffPacketId,
    InternalGoverningCmeOffice RequestingOffice,
    string RequestingOfficeInstanceId,
    string AuthorizingSurface,
    GovernedWorkerSpecies WorkerSpecies,
    WorkerInstanceMode WorkerInstanceMode,
    string Objective,
    string TaskKind,
    IReadOnlyList<string> SourceHandles,
    string RequiredOutputKind,
    string DeadlineOrExpiry,
    IReadOnlyList<string> HaltConditions,
    OfficeActionEligibility ActionCeiling,
    CompassVisibilityClass DisclosureClass,
    IReadOnlyList<WorkerReasonCode> AllowedReasonCodes,
    IReadOnlyList<string> ProhibitedActions,
    string PublicationDenial,
    string MutationDenial,
    IReadOnlyList<string> MountedMemoryLanes,
    IReadOnlyList<string> ForbiddenMemoryLanes,
    IReadOnlyList<string> ToolAllowlist,
    IReadOnlyList<string> ToolDenials,
    string ContinuityLinkageRequirement,
    string ResidueReturnRequirement,
    bool WitnessRequired,
    string RequiredWitnessSurface,
    string ReturnPacketSchema,
    string ReturnDestination,
    CompassVisibilityClass ReturnVisibilityClass,
    WorkerResidueDisposition ResidueDisposition,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    MaturityPosture MaturityPosture,
    DateTimeOffset TimestampUtc,
    SliBridgeReviewReceipt? BridgeReview = null,
    SliRuntimeUseCeilingReceipt? RuntimeUseCeiling = null);

public sealed record WorkerReturnPacket(
    string WorkerPacketId,
    string HandoffPacketId,
    GovernedWorkerSpecies WorkerSpecies,
    WorkerCompletionState CompletionState,
    string ResultSummary,
    IReadOnlyList<string> EvidenceHandles,
    IReadOnlyList<WorkerReasonCode> ReasonCodes,
    IReadOnlyList<string> UnsupportedClaimFlags,
    IReadOnlyList<string> ProhibitedActionAttempts,
    WorkerResidueDisposition ResidueState,
    CompassVisibilityClass DisclosureClass,
    bool ExecutionClaimed,
    bool MutationClaimed,
    DateTimeOffset TimestampUtc,
    SliBridgeReviewReceipt? BridgeReview = null,
    SliRuntimeUseCeilingReceipt? RuntimeUseCeiling = null);

public sealed record GovernedWorkerHandoffReceipt(
    string HandoffHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    InternalGoverningCmeOffice RequestingOffice,
    string RequestingOfficeInstanceId,
    ConstructClass ConstructClass,
    GovernedWorkerSpecies WorkerSpecies,
    WorkerInstanceMode WorkerInstanceMode,
    OfficeActionEligibility ActionCeiling,
    CompassVisibilityClass DisclosureClass,
    EvidenceSufficiencyState EvidenceSufficiencyState,
    MaturityPosture MaturityPosture,
    string HandoffPacketId,
    string OfficeIssuanceHandle,
    string OfficeAuthorityHandle,
    string WeatherDisclosureHandle,
    string WitnessedBy,
    DateTimeOffset TimestampUtc,
    SliBridgeReviewReceipt? BridgeReview = null,
    SliRuntimeUseCeilingReceipt? RuntimeUseCeiling = null);

public sealed record GovernedWorkerReturnReceipt(
    string ReturnHandle,
    string LoopKey,
    GovernanceLoopStage Stage,
    string CMEId,
    InternalGoverningCmeOffice RequestingOffice,
    ConstructClass ConstructClass,
    string HandoffHandle,
    string HandoffPacketId,
    string WorkerPacketId,
    GovernedWorkerSpecies WorkerSpecies,
    WorkerCompletionState CompletionState,
    CompassVisibilityClass DisclosureClass,
    IReadOnlyList<WorkerReasonCode> ReasonCodes,
    IReadOnlyList<string> EvidenceHandles,
    IReadOnlyList<string> UnsupportedClaimFlags,
    IReadOnlyList<string> ProhibitedActionAttempts,
    WorkerResidueDisposition ResidueDisposition,
    bool Validated,
    string? ValidationFailureCode,
    string WitnessedBy,
    DateTimeOffset TimestampUtc,
    SliBridgeReviewReceipt? BridgeReview = null,
    SliRuntimeUseCeilingReceipt? RuntimeUseCeiling = null);
