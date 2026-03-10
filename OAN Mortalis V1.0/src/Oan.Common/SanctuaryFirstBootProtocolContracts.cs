namespace Oan.Common;

public enum SanctuaryFirstBootPhase
{
    LegalCovenant = 0,
    CharterAlignment = 1,
    GoverningSigilNaming = 2,
    LedgerGenesis = 3,
    SliBraidActivation = 4,
    StewardFormation = 5,
    GovernanceBraiding = 6,
    CmeEcosystemAuthorization = 7
}

public abstract record SanctuaryFirstBootProtocolArtifact(
    string ArtifactId,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences);

public sealed record SanctuaryFirstBootCovenant(
    string ArtifactId,
    BootClass BootClass,
    string JurisdictionBaseline,
    string DataProtectionPosture,
    bool OperatorAcceptanceRequired,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record SanctuaryCharterAlignmentRecord(
    string ArtifactId,
    BootClass BootClass,
    string CharterClass,
    bool CorporateCharterPrecedesPrimeDirectives,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record GoverningSigilIdentity(
    string ArtifactId,
    InternalGoverningCmeOffice Office,
    string SigilHandle,
    bool HopngWitnessAllowed,
    bool HopngConstitutive,
    string LineageSummary,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record GovernanceLedgerGenesisRecord(
    string ArtifactId,
    IReadOnlyList<string> PrimeLedgerHandles,
    IReadOnlyList<string> CrypticLedgerHandles,
    IReadOnlyList<InternalGoverningCmeOffice> AnchoringOffices,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record SliBraidActivationRecord(
    string ArtifactId,
    string SliBraidHandle,
    IReadOnlyList<string> GoverningSigilHandles,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record StewardFormationGenesisRecord(
    string ArtifactId,
    string StewardBindingTarget,
    string? StewardWitnessArtifactHandle,
    bool HopngWitnessAllowed,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record GovernanceBraidActivationRecord(
    string ArtifactId,
    string PrimeGovernanceHandle,
    string CrypticGovernanceHandle,
    IReadOnlyList<InternalGoverningCmeOffice> IntegratedOffices,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);

public sealed record CmeEcosystemAuthorizationProfile(
    string ArtifactId,
    BootClass BootClass,
    BootActivationState ActivationState,
    bool SubordinateCmeAuthorizationAllowed,
    ExpansionRights ExpansionRights,
    string Purpose,
    string ProducerIntent,
    string ConsumerIntent,
    IReadOnlyList<string> Invariants,
    IReadOnlyList<SanctuaryFirstBootPhase> RequiredOrderingReferences)
    : SanctuaryFirstBootProtocolArtifact(
        ArtifactId,
        Purpose,
        ProducerIntent,
        ConsumerIntent,
        Invariants,
        RequiredOrderingReferences);
