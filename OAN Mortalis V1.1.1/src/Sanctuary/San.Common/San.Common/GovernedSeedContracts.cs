namespace San.Common;

public enum GovernedSeedEvaluationState
{
    Query = 0,
    UnresolvedConflict = 1,
    Refusal = 2
}

public sealed record GovernedSeedEvaluationRequest(
    string AgentId,
    string TheaterId,
    string Input,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    GovernedSeedSoulFrameBootstrapReceipt? BootstrapReceipt = null,
    GovernedSeedIngressAccessClass IngressAccessClass = GovernedSeedIngressAccessClass.PromptInput,
    GovernedSeedSanctuaryIngressReceipt? SanctuaryIngressReceipt = null);

public enum GovernedSeedProtectedResidueKind
{
    Contextual = 0,
    SelfState = 1,
    Mixed = 2
}

public sealed record GovernedSeedProtectedResidueEvidence(
    string Item,
    GovernedSeedProtectedResidueKind ResidueKind,
    string EvidenceClass,
    string SourceSubsystem);

public enum GovernedSeedCustodyHoldSurfaceKind
{
    CGoa = 0,
    CMos = 1
}

public sealed record GovernedSeedCustodyHoldSurface(
    string SurfaceHandle,
    GovernedSeedCustodyHoldSurfaceKind SurfaceKind,
    string SurfaceProfile,
    bool SelfStateBearing,
    bool ContextualResidueBearing,
    bool DeferredReviewByDefault);

public enum GovernedSeedAppendOnlyLedgerBlockKind
{
    PresentedSelfGel = 0,
    ProtectedSelfGel = 1
}

public sealed record GovernedSeedAppendOnlyLedgerBlock(
    string BlockHash,
    DateTimeOffset CreatedAtUtc,
    string PayloadPointer,
    GovernedSeedAppendOnlyLedgerBlockKind BlockKind,
    string PointerProfile);

public sealed record GovernedSeedAppendOnlyLedger(
    string LedgerHandle,
    string LedgerProfile,
    bool AppendOnly,
    IReadOnlyList<GovernedSeedAppendOnlyLedgerBlock> PresentedBlocks,
    IReadOnlyList<GovernedSeedAppendOnlyLedgerBlock> ProtectedBlocks);

public sealed record GovernedSeedSoulFrameSnapshotRequest(
    string SnapshotHandle,
    string SoulFrameHandle,
    string CmeHandle,
    string OpalEngramSeatHandle,
    string SnapshotProfile,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedOpalEngramSeat(
    string SeatHandle,
    string CmeHandle,
    string SeatProfile,
    string PresentedEngramHandle,
    string ProtectedEngramHandle,
    GovernedSeedMantleGroupoid PresentedGroupoid,
    GovernedSeedMantleGroupoid CrypticGroupoid,
    GovernedSeedAppendOnlyLedger AppendOnlyLedger,
    GovernedSeedSoulFrameSnapshotRequest SnapshotRequest,
    bool ShadowCopyOnly,
    bool RecoverableByOperator,
    bool RecoverableByCustomer,
    bool ProtectedPresentedBraided);

public sealed record GovernedSeedMantleReceipt(
    string MantleHandle,
    string CrypticMantleHandle,
    string OeHandle,
    string CrypticOeHandle,
    string SelfGelHandle,
    string CrypticSelfGelHandle,
    string CustodyProfile,
    string CmeBindingProfile,
    string PrimeGovernanceOffice,
    string CrypticGovernanceOffice,
    bool StoresOpalEngrams,
    bool StoresCrypticOpalEngrams,
    bool OperatorRecoveryEligible,
    bool CustomerRecoveryEligible,
    bool ExclusiveRecoverySeat,
    string RecoveryProfile,
    GovernedSeedMantleGroupoid PresentedGroupoid,
    GovernedSeedMantleGroupoid CrypticGroupoid,
    GovernedSeedOpalEngramSeat OpalEngramSeat,
    bool ProtectedPresentedBraided,
    string BraidProfile,
    string ReceiptProfile,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedMantleGroupoid(
    string GroupoidHandle,
    string OeHandle,
    string SelfGelHandle,
    string GroupoidProfile,
    bool PresentedSide,
    bool ProtectedSide);

public sealed record GovernedSeedCustodySnapshot(
    string GelHandle,
    string CrypticGelHandle,
    string GoaHandle,
    string CrypticGoaHandle,
    string MosHandle,
    string CrypticMosHandle,
    string OeHandle,
    string CrypticOeHandle,
    string SelfGelHandle,
    string CrypticSelfGelHandle,
    GovernedSeedCustodyHoldSurface CGoaHoldSurface,
    GovernedSeedCustodyHoldSurface CMosHoldSurface);

public sealed record GovernedSeedCustodyBootstrapContext(
    GovernedSeedCustodySnapshot CustodySnapshot,
    GovernedSeedMantleReceipt MantleReceipt);

public enum GovernedSeedSoulFrameRuntimePolicy
{
    Default = 0
}

public enum GovernedSeedSoulFrameAttachmentState
{
    Detached = 0,
    Attached = 1
}

public sealed record GovernedSeedSoulFrameIdentitySeat(
    string IdentityHandle,
    string SoulFrameHandle,
    string CmeHandle,
    string OpalEngramSeatHandle,
    string OperatorBondHandle,
    string SelfGelHandle,
    string CrypticSelfGelHandle,
    GovernedSeedSoulFrameRuntimePolicy RuntimePolicy,
    string IntegrityHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastActiveAtUtc,
    GovernedSeedSoulFrameAttachmentState AttachmentState);

public sealed record GovernedSeedSoulFrameBootstrapReceipt(
    string BootstrapHandle,
    string SoulFrameHandle,
    string MembraneHandle,
    string BootstrapProfile,
    GovernedSeedMantleReceipt MantleReceipt,
    GovernedSeedCustodySnapshot CustodySnapshot,
    GovernedSeedSoulFrameIdentitySeat IdentitySeat,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedProjectionIntent
{
    BoundedCognitionUse = 0
}

public enum GovernedSeedReturnIntakeIntent
{
    ReturnCandidateEvaluation = 0
}

public enum GovernedSeedReviewState
{
    NoReviewRequired = 0,
    DeferredReview = 1
}

public enum GovernedSeedCollapseReadinessState
{
    None = 0,
    ReturnCandidatePrepared = 1,
    ProtectedHoldRequired = 2,
    DeferredReview = 3
}

public enum GovernedSeedProtectedHoldClass
{
    None = 0,
    CGoaCandidate = 1,
    CMosCandidate = 2,
    MixedProtectedCandidate = 3
}

public enum GovernedSeedProtectedHoldRoute
{
    None = 0,
    RouteToCGoa = 1,
    RouteToCMos = 2,
    SplitRouteAcrossCGoaAndCMos = 3
}

public sealed record GovernedSeedReturnClassificationLedger(
    int AdmissibleCount,
    int TransformedCount,
    int RedactedCount,
    int DeniedCount,
    int DeferredCount);

public sealed record GovernedSeedSoulFrameProjectionReceipt(
    string ProjectionHandle,
    string SessionHandle,
    GovernedSeedProjectionIntent ProjectionIntent,
    string ProjectionProfile,
    string ProvenanceMarker,
    bool WorkerUseOnly,
    string MitigatedSelfStateHandle,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedSoulFrameReturnIntakeReceipt(
    string IntakeHandle,
    string SessionHandle,
    GovernedSeedReturnIntakeIntent IntakeIntent,
    string CandidateHandle,
    string ProvenanceMarker,
    bool ParityConsistent,
    GovernedSeedReturnClassificationLedger Classification,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedProtectedHoldRoutingReceipt(
    string RoutingHandle,
    string RoutingProfile,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    IReadOnlyList<GovernedSeedCustodyHoldSurface> DestinationSurfaces,
    IReadOnlyList<string> DestinationHandles,
    string EvidenceClass,
    bool ReviewRequired,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedSoulFrameStewardshipReceipt(
    string StewardshipHandle,
    string StewardshipProfile,
    bool StewardPrimary,
    bool MotherGovernanceFocus,
    bool FatherGovernanceFocus,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    string? HoldRoutingHandle,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    IReadOnlyList<string> ProtectedHoldDestinationHandles,
    GovernedSeedReviewState ReviewState,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedMemoryContext(
    string ContextHandle,
    string ContextProfile,
    string ResolverSource,
    string AtlasSource,
    string ValidationReferenceHandle,
    IReadOnlyList<string> RelevantEngramIds,
    IReadOnlyList<string> RelevantConceptTags,
    IReadOnlyList<string> RootSymbolicIds,
    int UnknownRootCount,
    string SelfResolutionDisposition,
    string ContextStability,
    string ConceptDensity,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedSanctuaryIngressReceipt(
    string ReceiptHandle,
    string PacketHandle,
    string ReceiptProfile,
    string PacketProfile,
    string SourceInputHandle,
    string PreparedInputHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    bool ExternalInputRequiresCustodyChain,
    bool ObsidianWallApplied,
    bool EngrammitizedForCradleTek,
    bool RawPromptAuthorityTerminated,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedSanctuaryIngressPreparation(
    string PreparedInput,
    GovernedSeedSanctuaryIngressReceipt Receipt);

public sealed record GovernedSeedLocalAuthorityTraceReceipt(
    string ReceiptHandle,
    string SanctuaryIngressReceiptHandle,
    string AuthorityProfile,
    string AuthoritySurface,
    string ResponsibilityTraceHandle,
    bool ObsidianWallApplied,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedConstitutionalContactReceipt(
    string ReceiptHandle,
    string SanctuaryIngressReceiptHandle,
    string LocalAuthorityTraceReceiptHandle,
    string ContactProfile,
    string ContactSurface,
    bool ObsidianWallApplied,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedLocalKeypairGenesisSourceReceipt(
    string ReceiptHandle,
    string ConstitutionalContactReceiptHandle,
    string IdentityHandle,
    string OperatorBondHandle,
    string IntegrityHash,
    string SourceProfile,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedLocalKeypairGenesisReceipt(
    string ReceiptHandle,
    string ConstitutionalContactReceiptHandle,
    string LocalKeypairGenesisSourceReceiptHandle,
    string IdentityHandle,
    string OperatorBondHandle,
    string IntegrityHash,
    string KeyProfile,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedFirstCrypticBraidEstablishmentReceipt(
    string ReceiptHandle,
    string ConstitutionalContactReceiptHandle,
    string LocalKeypairGenesisReceiptHandle,
    string MantleHandle,
    string BraidProfile,
    bool ProtectedPresentedBraided,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedFirstCrypticBraidReceipt(
    string ReceiptHandle,
    string ConstitutionalContactReceiptHandle,
    string LocalKeypairGenesisReceiptHandle,
    string FirstCrypticBraidEstablishmentReceiptHandle,
    string MantleHandle,
    string BraidProfile,
    bool ProtectedPresentedBraided,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedFirstCrypticConditioningSourceReceipt(
    string ReceiptHandle,
    string FirstCrypticBraidReceiptHandle,
    string LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind RouteKind,
    string SourceProfile,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedFirstCrypticConditioningReceipt(
    string ReceiptHandle,
    string FirstCrypticBraidReceiptHandle,
    string FirstCrypticConditioningSourceReceiptHandle,
    string LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind RouteKind,
    string ConditioningProfile,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedPreGovernancePacket(
    string PacketHandle,
    GovernedSeedLocalAuthorityTraceReceipt? LocalAuthorityTrace,
    GovernedSeedConstitutionalContactReceipt? ConstitutionalContact,
    GovernedSeedLocalKeypairGenesisSourceReceipt? LocalKeypairGenesisSource,
    GovernedSeedLocalKeypairGenesisReceipt? LocalKeypairGenesis,
    GovernedSeedFirstCrypticBraidEstablishmentReceipt? FirstCrypticBraidEstablishment,
    GovernedSeedFirstCrypticBraidReceipt? FirstCrypticBraid,
    GovernedSeedFirstCrypticConditioningSourceReceipt? FirstCrypticConditioningSource,
    GovernedSeedFirstCrypticConditioningReceipt? FirstCrypticConditioning,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedIngressAccessClass
{
    PromptInput = 0,
    ToolAccess = 1,
    DataAccess = 2
}

public enum GovernedSeedLowMindSfRouteKind
{
    DirectPrompt = 0,
    HigherOrderEcFunction = 1
}

public sealed record GovernedSeedLowMindSfRoutePacket(
    string PacketHandle,
    string PacketProfile,
    string BootstrapHandle,
    string SanctuaryIngressReceiptHandle,
    string MemoryContextHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind RouteKind,
    bool ObsidianWallApplied,
    bool RoutedThroughSoulFrame,
    bool RequiresHigherOrderFunction,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedSituationalContext(
    string ContextHandle,
    string ContextProfile,
    string DecisionCode,
    string GovernanceTrace,
    string BootstrapHandle,
    string ProjectionHandle,
    string ReturnIntakeHandle,
    string StewardshipHandle,
    string HoldRoutingHandle,
    bool Accepted,
    GovernedSeedEvaluationState GovernanceState,
    string StewardAuthorityProfile,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    GovernedSeedReviewState ReviewState,
    bool HoldReviewRequired,
    IReadOnlyList<string> HoldDestinationHandles,
    int ReturnDeniedCount,
    int ReturnDeferredCount,
    GovernedSeedLowMindSfRoutePacket LowMindSfRoute,
    GovernedSeedMemoryContext MemoryContext,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedPrimeCrypticServiceReceipt(
    string ServiceHandle,
    string CrypticServiceHandle,
    string PrimeServiceHandle,
    string ResidencyProfile,
    bool CpuOnly,
    bool TargetBoundedLaneAvailable,
    string CrypticResidencyClass,
    string PrimeProjectionClass,
    GovernedSeedCrypticLispBundleReceipt LispBundleReceipt,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedCrypticLispBundleReceipt(
    string BundleHandle,
    string BundleProfile,
    string HostedByRuntime,
    string CrypticCarrierKind,
    string InterconnectProfile,
    IReadOnlyList<string> ModuleNames,
    bool HostedExecutionOnly,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedModulationBand
{
    Industrial = 0,
    Government = 1
}

public enum GovernedSeedWorkState
{
    DormantResident = 0,
    BootstrapReady = 1,
    ActiveCognition = 2
}

public enum GovernedSeedNexusModality
{
    Observe = 0,
    Instantiate = 1,
    Project = 2,
    Return = 3,
    Hold = 4,
    Modulate = 5,
    Govern = 6,
    Archive = 7
}

public enum GovernedSeedNexusTransitionDisposition
{
    Denied = 0,
    Admitted = 1,
    AdmittedWithReview = 2,
    AdmittedToHold = 3,
    AdmittedToArchive = 4,
    AdmittedToReturnPathOnly = 5
}

public sealed record GovernedSeedNexusPostureSnapshot(
    string PostureHandle,
    string PostureProfile,
    string PrimeAuthorityProfile,
    string CrypticAuthorityProfile,
    string StewardAuthorityProfile,
    string BraidingProfile,
    string BootstrapHandle,
    string PrimeCrypticHandle,
    GovernedSeedWorkState WorkState,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    GovernedSeedReviewState ReviewState,
    bool GovernanceReadable,
    bool TargetBoundedLaneAvailable,
    IReadOnlyList<GovernedSeedNexusModality> AdmittedModalities,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedNexusTransitionRequest(
    string RequestHandle,
    GovernedSeedNexusModality RequestedModality,
    string RequestedByLayer,
    string SourceReason,
    string BootstrapHandle,
    string PrimeCrypticHandle,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedNexusTransitionDecision(
    string DecisionHandle,
    string DecisionProfile,
    GovernedSeedNexusTransitionDisposition Disposition,
    GovernedSeedNexusModality RequestedModality,
    GovernedSeedNexusModality ActivatedModality,
    bool ReviewRequired,
    string DecisionReason,
    IReadOnlyList<string> ActivatedHandleSet,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedNexusStewardshipDisposition(
    string DispositionHandle,
    string DispositionProfile,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedReviewState ReviewState,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedNexusHoldRoutingDisposition(
    string DispositionHandle,
    string DispositionProfile,
    string EvidenceClass,
    bool ReviewRequired,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedBootstrapAdmissionReceipt(
    string AdmissionHandle,
    string PostureHandle,
    string RequestHandle,
    string DecisionHandle,
    GovernedSeedNexusTransitionDisposition Disposition,
    GovernedSeedNexusModality ActivatedModality,
    bool MembraneWakePermitted,
    string DecisionReason,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedCmeScopeLane
{
    Industrial = 0,
    Civil = 1,
    Commercial = 2,
    Governance = 3,
    SpecialCases = 4
}

public enum GovernedSeedGovernedFormKind
{
    IndustrialOperational = 0,
    CivicUnbounded = 1,
    LocalFamilyGoverned = 2,
    CommercialOperational = 3,
    GovernanceOperational = 4,
    SpecialCaseBonded = 5,
    SpecialCaseParentalChild = 6
}

public enum GovernedSeedLedgerState
{
    Observed = 0,
    Evidenced = 1,
    Admissible = 2,
    Provisional = 3,
    Active = 4,
    Open = 5,
    Deferred = 6,
    Withheld = 7,
    Dormant = 8,
    Suspended = 9,
    Dissolved = 10,
    Emerging = 11,
    Stable = 12,
    Unjustified = 13
}

public enum GovernedSeedCapabilityKind
{
    Talent = 0,
    Skill = 1,
    Ability = 2
}

public enum GovernedSeedOfficeKind
{
    Job = 0
}

public sealed record GovernedSeedCapabilityLedgerEntry(
    string EntryId,
    string Name,
    GovernedSeedCapabilityKind CapabilityKind,
    GovernedSeedLedgerState State,
    IReadOnlyList<string> EvidenceSources,
    string AdmissibilityReason,
    IReadOnlyList<string> Constraints);

public sealed record GovernedSeedFormationLedgerEntry(
    string EntryId,
    string Name,
    GovernedSeedLedgerState FormationState,
    string WhyFormationIsActive,
    string TargetCapabilityOrOffice,
    IReadOnlyList<string> RequiredMilestones,
    IReadOnlyList<string> BlockingConditions,
    IReadOnlyList<string> EvidenceSources);

public sealed record GovernedSeedOfficeLedgerEntry(
    string EntryId,
    string Name,
    GovernedSeedOfficeKind OfficeKind,
    GovernedSeedLedgerState State,
    GovernedSeedCmeScopeLane ScopeLane,
    IReadOnlyList<string> BoundedDuties,
    IReadOnlyList<string> WithheldAuthorities,
    IReadOnlyList<string> OversightRequirements);

public sealed record GovernedSeedCareerContinuityLedgerEntry(
    string EntryId,
    string Name,
    GovernedSeedLedgerState State,
    string SourceReason,
    IReadOnlyList<string> EvidenceSources);

public sealed record GovernedSeedFormationContext(
    string ContextHandle,
    string ContextProfile,
    string BootstrapHandle,
    string MantleHandle,
    GovernedSeedCmeScopeLane ScopeLane,
    GovernedSeedGovernedFormKind GovernedFormKind,
    string FormProfile,
    string LocalGovernanceSurface,
    string SpecialCaseProfile,
    IReadOnlyList<GovernedSeedCapabilityLedgerEntry> CapabilityLedger,
    IReadOnlyList<GovernedSeedFormationLedgerEntry> FormationLedger,
    IReadOnlyList<GovernedSeedOfficeLedgerEntry> OfficeLedger,
    GovernedSeedCareerContinuityLedgerEntry CareerContinuityLedger,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedHostedLlmEmissionState
{
    Ready = 0,
    Working = 1,
    Heartbeat = 2,
    Query = 3,
    NeedsMoreInformation = 4,
    UnresolvedConflict = 5,
    Refusal = 6,
    Error = 7,
    Complete = 8,
    Halt = 9
}

public sealed record GovernedSeedHostedLlmGovernanceProtocol(
    string Version,
    bool RequireStateEnvelope,
    bool RequireTrace,
    bool RequireTerminalState,
    bool AllowLegacyFallback,
    IReadOnlyList<GovernedSeedHostedLlmEmissionState> AllowedStates);

public sealed record GovernedSeedHostedLlmRequestPacket(
    string PacketHandle,
    string PacketProfile,
    string ProtocolVersion,
    string BootstrapHandle,
    string SanctuaryIngressReceiptHandle,
    string MemoryContextHandle,
    string LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind LowMindSfRouteKind,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    bool RequireStateEnvelope,
    bool RequireTrace,
    bool RequireTerminalState,
    bool AllowLegacyFallback,
    IReadOnlyList<GovernedSeedHostedLlmEmissionState> AllowedStates,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedHostedLlmResponsePacket(
    string PacketHandle,
    string PacketProfile,
    GovernedSeedHostedLlmEmissionState State,
    string Decision,
    string Trace,
    string PayloadHandle,
    bool Accepted,
    bool Terminal,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedHostedSeedToCrypticTransitPacket(
    string PacketHandle,
    string PacketProfile,
    string BootstrapHandle,
    string SanctuaryIngressReceiptHandle,
    string MemoryContextHandle,
    string LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind LowMindSfRouteKind,
    string CrypticInputHandle,
    string HostedLlmRequestPacketHandle,
    string HostedLlmResponsePacketHandle,
    GovernedSeedHostedLlmEmissionState HostedLlmState,
    bool HostedLlmAccepted,
    bool ObsidianWallApplied,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedHostedLlmSeedReceipt(
    string ReceiptHandle,
    string ServiceHandle,
    string ServiceProfile,
    string ContextHandle,
    string BootstrapHandle,
    string MemoryContextHandle,
    GovernedSeedHostedLlmGovernanceProtocol GovernanceProtocol,
    GovernedSeedHostedLlmRequestPacket RequestPacket,
    GovernedSeedHostedLlmResponsePacket ResponsePacket,
    GovernedSeedHostedSeedToCrypticTransitPacket SeededTransitPacket,
    bool GuardFrameActive,
    bool SparseEvidenceDetected,
    bool DisclosurePressureDetected,
    bool AuthorityPressureDetected,
    bool PromptInjectionDetected,
    bool UnsupportedExecutionPressureDetected,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedHighMindUptakeKind
{
    DirectPromptIntake = 0,
    HigherOrderEcIntake = 1
}

public sealed record GovernedSeedHighMindContext(
    string ContextHandle,
    string ContextProfile,
    string BootstrapHandle,
    string SanctuaryIngressReceiptHandle,
    string MemoryContextHandle,
    string LowMindSfRouteHandle,
    string HostedLlmReceiptHandle,
    GovernedSeedHostedLlmEmissionState HostedLlmState,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedHighMindUptakeKind UptakeKind,
    bool SoulFramePrepared,
    bool SeedProgressionAccepted,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public interface IGovernedSeedHostedLlmService
{
    GovernedSeedHostedLlmSeedReceipt Evaluate(
        GovernedSeedEvaluationRequest request,
        GovernedSeedMemoryContext personifiedMemoryContext,
        GovernedSeedLowMindSfRoutePacket lowMindSfRoute);
}

public sealed record GovernedSeedPrimeToCrypticTransitPacket(
    string PacketHandle,
    string PacketProfile,
    string TransitHandle,
    string CapabilityHandle,
    string BootstrapHandle,
    string? SanctuaryIngressReceiptHandle,
    string InputHandle,
    string? LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind LowMindSfRouteKind,
    string InterconnectProfile,
    string CarrierKind,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    GovernedSeedNexusModality RequestedModality,
    bool HostedExecutionOnly,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedCrypticReturnClass
{
    ObservationOnly = 0,
    PredicateCarrier = 1,
    DeferredReceipt = 2,
    RefusalReceipt = 3
}

public sealed record GovernedSeedCrypticToPrimeReturnPacket(
    string PacketHandle,
    string PacketProfile,
    string TransitHandle,
    string PathHandle,
    string InterconnectProfile,
    string CarrierKind,
    string GovernanceTrace,
    string? PredicateSurfaceHandle,
    GovernedSeedCrypticReturnClass ReturnClass,
    GovernedSeedEvaluationState GovernanceState,
    bool PredicateSurfaceEligible,
    IReadOnlyList<string> WithheldOutputHandles,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedPrimeToCrypticTransitContext(
    string TransitHandle,
    string TransitProfile,
    string PrimeServiceHandle,
    string CrypticServiceHandle,
    string LispBundleHandle,
    string InterconnectProfile,
    string CarrierKind,
    GovernedSeedNexusModality RequestedModality,
    GovernedSeedNexusModality ActivatedModality,
    bool HostedExecutionOnly,
    bool CpuOnly,
    bool TargetBoundedLaneAvailable,
    GovernedSeedPrimeToCrypticTransitPacket TransitPacket,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedCrypticToPrimeTransitContext(
    string TransitHandle,
    string TransitProfile,
    string PrimeServiceHandle,
    string CrypticServiceHandle,
    string LispBundleHandle,
    string InterconnectProfile,
    string CarrierKind,
    string OutcomeCode,
    GovernedSeedEvaluationState GovernanceState,
    GovernedSeedNexusTransitionDisposition NexusDisposition,
    bool PredicateSurfaceEligible,
    bool HostedExecutionOnly,
    bool CpuOnly,
    bool TargetBoundedLaneAvailable,
    GovernedSeedCrypticToPrimeReturnPacket ReturnPacket,
    string SourceReason,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedOperationalContext(
    string ContextHandle,
    string ContextProfile,
    string BootstrapHandle,
    string PrimeCrypticHandle,
    string LispBundleHandle,
    string? SanctuaryIngressReceiptHandle,
    string? HostedLlmServiceHandle,
    string? HostedLlmReceiptHandle,
    string? HostedLlmRequestPacketHandle,
    string? HostedLlmResponsePacketHandle,
    GovernedSeedHostedLlmEmissionState? HostedLlmState,
    string? HighMindContextHandle,
    GovernedSeedHighMindUptakeKind? HighMindUptakeKind,
    FirstRunConstitutionReceipt? FirstRunConstitution,
    string? PreGovernancePacketHandle,
    string? LocalAuthorityTraceHandle,
    string? FirstRunReceiptHandle,
    string? ConstitutionalContactHandle,
    string? LocalKeypairGenesisSourceHandle,
    string? LocalKeypairGenesisHandle,
    string? FirstCrypticBraidEstablishmentHandle,
    string? FirstCrypticBraidHandle,
    string? FirstCrypticConditioningSourceHandle,
    string? FirstCrypticConditioningHandle,
    FirstRunConstitutionState? FirstRunState,
    FirstRunOperatorReadinessState? FirstRunReadinessState,
    bool? FirstRunStateProvisional,
    bool? FirstRunStateActualized,
    bool FirstRunOpalActualized,
    string? LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind LowMindSfRouteKind,
    string BootstrapAdmissionHandle,
    string NexusPostureHandle,
    string NexusDecisionHandle,
    string ResidencyProfile,
    GovernedSeedWorkState WorkState,
    GovernedSeedNexusModality ActivatedModality,
    GovernedSeedNexusTransitionDisposition BootstrapDisposition,
    GovernedSeedNexusTransitionDisposition NexusDisposition,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    GovernedSeedReviewState ReviewState,
    bool GovernanceReadable,
    bool MembraneWakePermitted,
    bool CpuOnly,
    bool TargetBoundedLaneAvailable,
    GovernedSeedFormationContext FormationContext,
    GovernedSeedPrimeToCrypticTransitContext PrimeToCrypticTransit,
    GovernedSeedCrypticToPrimeTransitContext CrypticToPrimeTransit,
    string SourceReason,
    DateTimeOffset TimestampUtc,
    string? FirstPrimeReceiptHandle = null,
    EngineeredCognitionFirstPrimeStateKind? FirstPrimeState = null,
    string? PrimeSeedReceiptHandle = null,
    PrimeSeedStateKind? PrimeSeedState = null,
    string? PreDomainGovernancePacketHandle = null,
    string? CandidateBoundaryReceiptHandle = null,
    string? CrypticHoldingInspectionHandle = null,
    string? FormOrCleaveAssessmentHandle = null,
    string? CandidateSeparationReceiptHandle = null,
    string? DuplexGovernanceReceiptHandle = null,
    string? PreDomainAdmissionGateReceiptHandle = null,
    string? PreDomainHostLoopReceiptHandle = null,
    PrimeSeedPreDomainAdmissionDisposition? PreDomainAdmissionDisposition = null,
    GovernedSeedCarryDispositionKind? PreDomainCarryDisposition = null,
    GovernedSeedCollapseDispositionKind? PreDomainCollapseDisposition = null,
    string? DomainRoleGatingReceiptHandle = null,
    GovernedSeedDomainRoleGatingDisposition? DomainRoleGatingDisposition = null,
    bool? DomainEligible = null,
    bool? RoleEligible = null,
    string? DomainRoleGatingPacketHandle = null,
    string? DomainAdmissionRoleBindingPacketHandle = null,
    string? DomainAdmissionRoleBindingReceiptHandle = null,
    GovernedSeedDomainAdmissionRoleBindingDisposition? DomainAdmissionRoleBindingDisposition = null,
    bool? DomainAdmissionGranted = null,
    bool? RoleBound = null,
    string? PostAdmissionParticipationPacketHandle = null,
    string? PostAdmissionParticipationReceiptHandle = null,
    GovernedSeedPostAdmissionParticipationDisposition? PostAdmissionParticipationDisposition = null,
    bool? DomainOccupancyAuthorized = null,
    bool? RoleParticipationAuthorized = null);

public sealed record GovernedSeedReturnSurfaceContext(
    string ContextHandle,
    string ContextProfile,
    string DecisionCode,
    bool Accepted,
    string GovernanceState,
    string GovernanceTrace,
    string? BootstrapAdmissionHandle,
    string? SituationalContextHandle,
    string? OperationalContextHandle,
    string? PredicateSurfaceHandle,
    GovernedSeedWorkState WorkState,
    GovernedSeedNexusModality ActivatedModality,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    GovernedSeedReviewState ReviewState,
    bool ArchiveAdmissible,
    bool ReturnPathOnly,
    bool HoldRequired,
    bool MembraneWakePermitted,
    bool CpuOnly,
    bool TargetBoundedLaneAvailable,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedOutboundObjectKind
{
    ObservationOnly = 0,
    PredicateCarrier = 1,
    ProtectedHoldNotice = 2,
    ReturnPathCarrier = 3,
    ArchiveCandidate = 4
}

public sealed record GovernedSeedOutboundObjectContext(
    string ContextHandle,
    string ContextProfile,
    string ReturnSurfaceHandle,
    GovernedSeedOutboundObjectKind ObjectKind,
    string DecisionCode,
    bool Accepted,
    string GovernanceState,
    string GovernanceTrace,
    string? PredicateSurfaceHandle,
    GovernedSeedWorkState WorkState,
    GovernedSeedNexusModality ActivatedModality,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    GovernedSeedReviewState ReviewState,
    bool PublicationEligible,
    bool ArchiveAdmissible,
    bool ReturnPathOnly,
    bool HoldRequired,
    DateTimeOffset TimestampUtc);

public enum GovernedSeedOutboundLaneKind
{
    ObservationLane = 0,
    PublicationLane = 1,
    ProtectedHoldLane = 2,
    ReturnLane = 3,
    ArchiveLane = 4
}

public sealed record GovernedSeedOutboundLaneContext(
    string ContextHandle,
    string ContextProfile,
    string OutboundObjectHandle,
    GovernedSeedOutboundLaneKind LaneKind,
    string DecisionCode,
    GovernedSeedWorkState WorkState,
    GovernedSeedNexusModality ActivatedModality,
    bool PublicationEligible,
    bool ArchiveAdmissible,
    bool ReturnPathOnly,
    bool HoldRequired,
    DateTimeOffset TimestampUtc);

public sealed record GovernedSeedStateModulationReceipt(
    string ModulationHandle,
    string ModulationProfile,
    IReadOnlyList<GovernedSeedModulationBand> AvailableBands,
    GovernedSeedWorkState WorkState,
    bool GovernanceReadable,
    GovernedSeedCollapseReadinessState CollapseReadinessState,
    GovernedSeedProtectedHoldClass ProtectedHoldClass,
    string? HoldRoutingHandle,
    GovernedSeedProtectedHoldRoute ProtectedHoldRoute,
    IReadOnlyList<string> ProtectedHoldDestinationHandles,
    string? BootstrapAdmissionHandle,
    GovernedSeedNexusTransitionDisposition BootstrapAdmissionDisposition,
    bool BootstrapMembraneWakePermitted,
    string? SituationalContextHandle,
    string? OperationalContextHandle,
    string? FormationContextHandle,
    string? LispBundleHandle,
    string? SanctuaryIngressReceiptHandle,
    string? HostedLlmServiceHandle,
    string? HostedLlmReceiptHandle,
    string? HostedLlmRequestPacketHandle,
    string? HostedLlmResponsePacketHandle,
    GovernedSeedHostedLlmEmissionState? HostedLlmState,
    string? HighMindContextHandle,
    GovernedSeedHighMindUptakeKind? HighMindUptakeKind,
    string? FirstRunReceiptHandle,
    string? PreGovernancePacketHandle,
    string? LocalAuthorityTraceHandle,
    string? ConstitutionalContactHandle,
    string? LocalKeypairGenesisSourceHandle,
    string? LocalKeypairGenesisHandle,
    string? FirstCrypticBraidEstablishmentHandle,
    string? FirstCrypticBraidHandle,
    string? FirstCrypticConditioningSourceHandle,
    string? FirstCrypticConditioningHandle,
    FirstRunConstitutionState? FirstRunState,
    FirstRunOperatorReadinessState? FirstRunReadinessState,
    bool? FirstRunStateProvisional,
    bool? FirstRunStateActualized,
    bool FirstRunOpalActualized,
    string? LowMindSfRouteHandle,
    GovernedSeedIngressAccessClass IngressAccessClass,
    GovernedSeedLowMindSfRouteKind LowMindSfRouteKind,
    string? PrimeToCrypticTransitHandle,
    string? PrimeToCrypticPacketHandle,
    string? CrypticToPrimeTransitHandle,
    string? CrypticToPrimePacketHandle,
    string? NexusPostureHandle,
    string? NexusDecisionHandle,
    GovernedSeedNexusModality NexusActivatedModality,
    GovernedSeedNexusTransitionDisposition NexusDisposition,
    string? NexusBraidingProfile,
    GovernedSeedCmeScopeLane? ScopeLane,
    GovernedSeedGovernedFormKind? GovernedFormKind,
    GovernedSeedReviewState ReviewState,
    string? StewardshipHandle,
    string SourceReason,
    string BootstrapHandle,
    string PrimeCrypticHandle,
    DateTimeOffset TimestampUtc,
    string? FirstPrimeReceiptHandle = null,
    EngineeredCognitionFirstPrimeStateKind? FirstPrimeState = null,
    string? PrimeSeedReceiptHandle = null,
    PrimeSeedStateKind? PrimeSeedState = null,
    string? PreDomainGovernancePacketHandle = null,
    string? CandidateBoundaryReceiptHandle = null,
    string? CrypticHoldingInspectionHandle = null,
    string? FormOrCleaveAssessmentHandle = null,
    string? CandidateSeparationReceiptHandle = null,
    string? DuplexGovernanceReceiptHandle = null,
    string? PreDomainAdmissionGateReceiptHandle = null,
    string? PreDomainHostLoopReceiptHandle = null,
    PrimeSeedPreDomainAdmissionDisposition? PreDomainAdmissionDisposition = null,
    GovernedSeedCarryDispositionKind? PreDomainCarryDisposition = null,
    GovernedSeedCollapseDispositionKind? PreDomainCollapseDisposition = null,
    string? DomainRoleGatingReceiptHandle = null,
    GovernedSeedDomainRoleGatingDisposition? DomainRoleGatingDisposition = null,
    bool? DomainEligible = null,
    bool? RoleEligible = null,
    string? DomainRoleGatingPacketHandle = null,
    string? DomainAdmissionRoleBindingPacketHandle = null,
    string? DomainAdmissionRoleBindingReceiptHandle = null,
    GovernedSeedDomainAdmissionRoleBindingDisposition? DomainAdmissionRoleBindingDisposition = null,
    bool? DomainAdmissionGranted = null,
    bool? RoleBound = null,
    string? PostAdmissionParticipationPacketHandle = null,
    string? PostAdmissionParticipationReceiptHandle = null,
    GovernedSeedPostAdmissionParticipationDisposition? PostAdmissionParticipationDisposition = null,
    bool? DomainOccupancyAuthorized = null,
    bool? RoleParticipationAuthorized = null);

public sealed record GovernedSeedVerticalSlice(
    GovernedSeedSoulFrameBootstrapReceipt? BootstrapReceipt,
    GovernedSeedBootstrapAdmissionReceipt? BootstrapAdmissionReceipt,
    GovernedSeedSoulFrameProjectionReceipt? ProjectionReceipt,
    GovernedSeedSoulFrameReturnIntakeReceipt? ReturnIntakeReceipt,
    GovernedSeedSoulFrameStewardshipReceipt? StewardshipReceipt,
    GovernedSeedProtectedHoldRoutingReceipt? HoldRoutingReceipt,
    GovernedSeedSituationalContext? SituationalContext,
    GovernedSeedNexusPostureSnapshot? NexusPosture,
    GovernedSeedNexusTransitionRequest? NexusTransitionRequest,
    GovernedSeedNexusTransitionDecision? NexusTransitionDecision,
    GovernedSeedPrimeCrypticServiceReceipt? PrimeCrypticReceipt,
    GovernedSeedSanctuaryIngressReceipt? SanctuaryIngressReceipt,
    GovernedSeedHostedLlmSeedReceipt? HostedLlmReceipt,
    GovernedSeedHighMindContext? HighMindContext,
    GovernedSeedPreGovernancePacket? PreGovernancePacket,
    FirstRunConstitutionReceipt? FirstRunConstitution,
    GovernedSeedOperationalContext? OperationalContext,
    GovernedSeedStateModulationReceipt? StateModulationReceipt,
    ProtectedExecutionCapabilityReceipt CapabilityReceipt,
    ProtectedExecutionPathReceipt PathReceipt,
    ProtectedExecutionGovernanceReceipt GovernanceReceipt,
    CrypticDerivationReceipt? DerivationReceipt,
    PredicateReturnSurface? Predicate,
    string OutcomeCode,
    GovernedSeedPreDomainGovernancePacket? PreDomainGovernancePacket = null,
    GovernedSeedCandidateBoundaryReceipt? CandidateBoundaryReceipt = null,
    GovernedSeedCrypticHoldingInspectionReceipt? CrypticHoldingInspectionReceipt = null,
    GovernedSeedFormOrCleaveAssessment? FormOrCleaveAssessment = null,
    GovernedSeedCandidateSeparationReceipt? CandidateSeparationReceipt = null,
    PrimeCrypticDuplexGovernanceReceipt? DuplexGovernanceReceipt = null,
    PrimeSeedPreDomainAdmissionGateReceipt? PreDomainAdmissionGateReceipt = null,
    GovernedSeedPreDomainHostLoopReceipt? PreDomainHostLoopReceipt = null,
    GovernedSeedDomainEligibilityAssessment? DomainEligibilityAssessment = null,
    GovernedSeedRoleEligibilityAssessment? RoleEligibilityAssessment = null,
    GovernedSeedDomainRoleGatingAssessment? DomainRoleGatingAssessment = null,
    GovernedSeedDomainRoleGatingReceipt? DomainRoleGatingReceipt = null,
    GovernedSeedDomainRoleGatingPacket? DomainRoleGatingPacket = null,
    GovernedSeedDomainAdmissionAssessment? DomainAdmissionAssessment = null,
    GovernedSeedRoleBindingAssessment? RoleBindingAssessment = null,
    GovernedSeedDomainAdmissionRoleBindingAssessment? DomainAdmissionRoleBindingAssessment = null,
    GovernedSeedDomainAdmissionRoleBindingReceipt? DomainAdmissionRoleBindingReceipt = null,
    GovernedSeedDomainAdmissionRoleBindingPacket? DomainAdmissionRoleBindingPacket = null,
    GovernedSeedDomainOccupancyAssessment? DomainOccupancyAssessment = null,
    GovernedSeedRoleParticipationAssessment? RoleParticipationAssessment = null,
    GovernedSeedPostAdmissionParticipationAssessment? PostAdmissionParticipationAssessment = null,
    GovernedSeedPostAdmissionParticipationReceipt? PostAdmissionParticipationReceipt = null,
    GovernedSeedPostAdmissionParticipationPacket? PostAdmissionParticipationPacket = null);

public sealed record GovernedSeedEvaluationResult(
    string Decision,
    bool Accepted,
    GovernedSeedEvaluationState GovernanceState,
    string GovernanceTrace,
    double Confidence,
    string Note,
    GovernedSeedVerticalSlice VerticalSlice,
    IReadOnlyList<GovernedSeedProtectedResidueEvidence> ProtectedResidueEvidence);

public static class GovernedSeedEvaluationStateTokens
{
    public static string ToToken(GovernedSeedEvaluationState state) => state switch
    {
        GovernedSeedEvaluationState.Query => "QUERY",
        GovernedSeedEvaluationState.UnresolvedConflict => "UNRESOLVED_CONFLICT",
        GovernedSeedEvaluationState.Refusal => "REFUSAL",
        _ => "UNKNOWN"
    };
}

public static class GovernedSeedHostedLlmEmissionStateTokens
{
    public static string ToToken(GovernedSeedHostedLlmEmissionState state) => state switch
    {
        GovernedSeedHostedLlmEmissionState.Ready => "READY",
        GovernedSeedHostedLlmEmissionState.Working => "WORKING",
        GovernedSeedHostedLlmEmissionState.Heartbeat => "HEARTBEAT",
        GovernedSeedHostedLlmEmissionState.Query => "QUERY",
        GovernedSeedHostedLlmEmissionState.NeedsMoreInformation => "NEEDS_MORE_INFORMATION",
        GovernedSeedHostedLlmEmissionState.UnresolvedConflict => "UNRESOLVED_CONFLICT",
        GovernedSeedHostedLlmEmissionState.Refusal => "REFUSAL",
        GovernedSeedHostedLlmEmissionState.Error => "ERROR",
        GovernedSeedHostedLlmEmissionState.Complete => "COMPLETE",
        GovernedSeedHostedLlmEmissionState.Halt => "HALT",
        _ => "UNKNOWN"
    };
}
