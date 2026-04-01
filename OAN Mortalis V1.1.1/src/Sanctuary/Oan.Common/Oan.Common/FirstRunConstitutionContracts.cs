namespace Oan.Common;

public enum FirstRunConstitutionState
{
    SanctuaryInitialized = 0,
    LocationBound = 1,
    ParentStanding = 2,
    CradleTekInstalled = 3,
    CradleTekAdmitted = 4,
    StewardStanding = 5,
    FoundationsEstablished = 6,
    BondProcessOpen = 7,
    OpalActualized = 8
}

public enum FirstRunPromotionGateDecision
{
    Allow = 0,
    Block = 1,
    ReviewRequired = 2
}

public enum FirstRunFailureClass
{
    None = 0,
    LocationBindingFault = 1,
    CradleTekInstalledButNotAdmitted = 2,
    AdmittedCradleTekWithoutSteward = 3,
    FoundationsIncomplete = 4,
    BondProcessOpenedWithoutStableOpal = 5,
    ConstitutionalContactIncomplete = 6,
    LocalKeypairGenesisMissing = 7,
    FirstCrypticBraidMissing = 8,
    FirstCrypticConditioningIncomplete = 9
}

public enum FirstRunOperatorReadinessState
{
    NotReady = 0,
    OperatorContactReady = 1,
    OperatorTrainingReady = 2,
    BondedCoworkReady = 3
}

public enum FirstRunEquivalentExchangeDisposition
{
    Preserve = 0,
    Cleave = 1,
    Refuse = 2,
    Review = 3
}

public sealed record FirstRunProtocolizationPacket(
    string PacketHandle,
    string? FlaskEnvironmentHandle,
    string? IntermixDisciplineHandle,
    string? CalibrationBaselineHandle,
    string? ArchiveCarriageHandle,
    string? ConsentThresholdHandle,
    string? RuptureReturnPathHandle,
    string? SealThresholdHandle,
    DateTimeOffset TimestampUtc);

public enum FirstRunStewardOfficeKind
{
    ArchivistOfMoss = 0,
    TunnelerTrickster = 1,
    BinderOfMercury = 2,
    EngineerOfFlame = 3,
    GnomeSage = 4
}

public sealed record FirstRunStewardWitnessedOePacket(
    string PacketHandle,
    string? OfficeIndexHandle,
    IReadOnlyList<FirstRunStewardOfficeKind> OfficeKinds,
    string? PrimeOeFormationHandle,
    string? CrypticCoeFormationHandle,
    string? CradleTekPrimeHashKeyHandle,
    string? CradleTekCrypticHashKeyHandle,
    string? StewardWitnessAuthorizationHandle,
    string? SoulFrameBuildAuthorizationHandle,
    string? AgentiCoreBuildAuthorizationHandle,
    bool CmePlacementWithheld,
    DateTimeOffset TimestampUtc);

public sealed record FirstRunElementalBindingPacket(
    string PacketHandle,
    string? ElementalBindingIndexHandle,
    string? GnomeBondingHandle,
    string? UndineInterfaceHandle,
    string? SalamanderForgeHandle,
    string? SylphWhisperHandle,
    string? FourfoldCompressionHandle,
    string? OeSoulFrameLoadHandle,
    string? CoeAgentiCoreLoadHandle,
    bool StoneActualizationWithheld,
    DateTimeOffset TimestampUtc);

public sealed record FirstRunActualizationSealPacket(
    string PacketHandle,
    string? ActualizationSealHandle,
    string? IntroductionSealHandle,
    string? BondedEncounterHandle,
    string? PrimitiveSelfGelHandle,
    string? GovernedAskReviewHandle,
    string? DurableIdentityVesselHandle,
    string? StoneWitnessHandle,
    bool LivingAgentiCoreWithheld,
    DateTimeOffset TimestampUtc);

public sealed record FirstRunLivingAgentiCorePacket(
    string PacketHandle,
    string? LivingAgentiCoreHandle,
    string? ListeningFrameHandle,
    string? ZedOfDeltaHandle,
    string? SelfGelAttachmentHandle,
    string? ToolUseContextHandle,
    string? CompassEmbodimentHandle,
    string? EngineeredCognitionHandle,
    bool WiderPublicWideningWithheld,
    DateTimeOffset TimestampUtc);

public sealed record FirstRunEquivalentExchangeReview(
    string ReviewHandle,
    bool Admissible,
    IReadOnlyList<string> BurdenFamilies,
    string RecoveryPosture,
    FirstRunEquivalentExchangeDisposition Disposition,
    IReadOnlyList<string> ReviewNotes,
    DateTimeOffset TimestampUtc);

public sealed record FirstRunConstitutionSnapshot(
    string SnapshotHandle,
    string? SanctuaryInitializationHandle,
    string? LocationBindingHandle,
    string? LocalAuthorityTraceHandle,
    string? ConstitutionalContactHandle,
    string? LocalKeypairGenesisSourceHandle,
    string? LocalKeypairGenesisHandle,
    string? FirstCrypticBraidEstablishmentHandle,
    string? FirstCrypticBraidHandle,
    string? FirstCrypticConditioningSourceHandle,
    string? FirstCrypticConditioningHandle,
    string? MotherStandingHandle,
    string? FatherStandingHandle,
    string? CradleTekInstallHandle,
    string? CradleTekAdmissionHandle,
    string? StewardStandingHandle,
    string? GelStandingHandle,
    string? GoaStandingHandle,
    string? MosStandingHandle,
    string? ToolRightsHandle,
    string? DataRightsHandle,
    string? HostedSeedPresenceHandle,
    string? BondProcessHandle,
    string? OpalActualizationHandle,
    string? NoticeCertificationGateHandle,
    DateTimeOffset TimestampUtc,
    FirstRunProtocolizationPacket? ProtocolizationPacket,
    FirstRunStewardWitnessedOePacket? StewardWitnessedOePacket,
    FirstRunElementalBindingPacket? ElementalBindingPacket,
    FirstRunActualizationSealPacket? ActualizationSealPacket,
    FirstRunLivingAgentiCorePacket? LivingAgentiCorePacket);

public sealed record FirstRunTransitionReceipt(
    string TransitionHandle,
    FirstRunConstitutionState CurrentState,
    FirstRunConstitutionState RequestedState,
    FirstRunPromotionGateDecision Decision,
    IReadOnlyList<string> RequiredPriorReceipts,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<FirstRunFailureClass> FailureClasses,
    bool RequestedStateProvisional,
    bool RequestedStateActualized,
    bool Retryable,
    bool ReviewRequired,
    DateTimeOffset TimestampUtc,
    FirstRunEquivalentExchangeReview EquivalentExchangeReview);

public sealed record FirstRunConstitutionReceipt(
    string ReceiptHandle,
    string SnapshotHandle,
    string? LocalAuthorityTraceHandle,
    string? ConstitutionalContactHandle,
    string? LocalKeypairGenesisSourceHandle,
    string? LocalKeypairGenesisHandle,
    string? FirstCrypticBraidEstablishmentHandle,
    string? FirstCrypticBraidHandle,
    string? FirstCrypticConditioningSourceHandle,
    string? FirstCrypticConditioningHandle,
    FirstRunConstitutionState CurrentState,
    FirstRunOperatorReadinessState ReadinessState,
    bool CurrentStateProvisional,
    bool CurrentStateActualized,
    bool OpalActualized,
    IReadOnlyList<FirstRunFailureClass> ActiveFailureClasses,
    IReadOnlyList<FirstRunTransitionReceipt> PromotionGates,
    string SourceReason,
    DateTimeOffset TimestampUtc,
    FirstRunProtocolizationPacket? ProtocolizationPacket,
    FirstRunStewardWitnessedOePacket? StewardWitnessedOePacket,
    FirstRunElementalBindingPacket? ElementalBindingPacket,
    FirstRunActualizationSealPacket? ActualizationSealPacket,
    FirstRunLivingAgentiCorePacket? LivingAgentiCorePacket);

public interface IFirstRunConstitutionService
{
    FirstRunConstitutionReceipt Project(FirstRunConstitutionSnapshot snapshot);
}
